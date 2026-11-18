'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' Wspólny stan i narzędzia programu (dawniej zmienne publiczne Module1).
''' </summary>
Public Module Program

#If NETCOREAPP Then
    ''' <summary>Strona kodowa ANSI systemu Windows (w .NET Framework zwracana przez Encoding.Default).</summary>
    Private Declare Function GetACP Lib "kernel32" () As Integer
#End If

    ''' <summary>Katalog programu (pliki warstw, moduły pomocnicze).</summary>
    Public FolderProgramu As String = ""

    ''' <summary>
    ''' Folder zapisu danych (download, lastsettings.txt): katalog programu, a gdy nie można w nim zapisywać
    ''' (np. instalacja w Program Files) - %LocalAppData%\MapoTero.
    ''' </summary>
    Public FolderDanych As String = ""

    ''' <summary>Ustawienia programu i bieżącej sesji pobierania.</summary>
    Public ReadOnly Ustawienia As New UstawieniaProgramu()

    ''' <summary>Krótka wersja programu bez końcowych zer (np. „3.16”, „3.16.1”).</summary>
    Public ReadOnly Property Wersja As String
        Get
            Return MapoTero.Core.WersjaProgramu.Z(GetType(Program).Assembly)
        End Get
    End Property

    ''' <summary>Nagłówek User-Agent programu dla zapytań HTTP (np. „MapoTero/3.16”).</summary>
    Public ReadOnly Property UserAgentProgramu As String
        Get
            Return MapoTero.Core.WersjaProgramu.UserAgent(GetType(Program).Assembly)
        End Get
    End Property

    ''' <summary>Folder pobierania domyślny dla programu.</summary>
    Public ReadOnly Property FolderDownload As String
        Get
            Return FolderDanych & "\download\"
        End Get
    End Property

    ''' <summary>
    ''' Folder sesji dla okien, które tylko czytają segmenty (scalanie, eksport mapy, eksport do CAD): bieżąca sesja,
    ''' a gdy jej folder nie ma conf.txt (np. po „Usuń pobrane” albo przed pierwszym pobraniem) - najnowsza sesja
    ''' w download. Wtedy uwaga zawiera tekst dla użytkownika - dawniej okno przechodziło na inną sesję bez słowa.
    ''' Okno usuwania pustych segmentów nie korzysta z tej funkcji: kasuje pliki, więc działa tylko na bieżącej sesji.
    ''' </summary>
    Public Function FolderSesjiDlaNarzedzia(biezacy As String, ByRef uwaga As String) As String
        uwaga = ""
        If Not String.IsNullOrEmpty(biezacy) AndAlso File.Exists(biezacy & "conf.txt") Then Return biezacy
        Dim najnowszy = MapoTero.Core.KonfiguracjaSesji.NajnowszyFolderSesji(FolderDownload)
        If String.IsNullOrEmpty(najnowszy) Then Return FolderDownload
        uwaga = "Uwaga: folder bieżącej sesji (" & If(String.IsNullOrEmpty(biezacy), "brak", biezacy) &
                ") nie zawiera pliku conf.txt, dlatego otwarto najnowszą sesję: " & najnowszy &
                ". Inny folder wskażesz przyciskiem zmiany folderu."
        Return najnowszy
    End Function

    ''' <summary>Plik ustawień programu.</summary>
    Public ReadOnly Property PlikLastsettings As String
        Get
            Return FolderDanych & "\lastsettings.txt"
        End Get
    End Property

    ''' <summary>Katalog ze zbiorami map w folderze programu.</summary>
    Public ReadOnly Property FolderWarstwProgramu As String
        Get
            Return Path.Combine(FolderProgramu, "warstwy")
        End Get
    End Property

    ''' <summary>Katalog ze zbiorami map w folderze danych użytkownika (%LocalAppData%\MapoTero\warstwy).</summary>
    Public ReadOnly Property FolderWarstwDanych As String
        Get
            Return Path.Combine(FolderDanych, "warstwy")
        End Get
    End Property

    ''' <summary>Katalog zapisu nowych zbiorów map (katalog programu, a przy braku uprawnień - katalog użytkownika).</summary>
    Public Function FolderZapisuWarstw() As String
        Try
            Dim katProg = FolderWarstwProgramu
            Directory.CreateDirectory(katProg)
            Dim test = Path.Combine(katProg, "zapis_test.tmp")
            File.WriteAllText(test, "")
            File.Delete(test)
            Return katProg
        Catch
            Dim katDanych = FolderWarstwDanych
            Directory.CreateDirectory(katDanych)
            Return katDanych
        End Try
    End Function

    ''' <summary>
    ''' Kodowanie plików tekstowych programu (conf.txt, lastsettings.txt, pliki georeferencyjne): systemowa strona
    ''' kodowa ANSI - takie samo, jakiego używały instrukcje FileOpen/Print poprzednich wersji.
    ''' </summary>
    Public Function KodowanieSystemowe() As Encoding
#If NETCOREAPP Then
        RejestrujKodowania()
        'strona kodowa systemu (GetACP, „Język dla programów nieobsługujących Unicode”), a nie formatu regionalnego
        '(CultureInfo.CurrentCulture): przy formacie „angielski (USA)” na polskim Windows dawało to 1252 - polskie
        'litery w ścieżkach lastsettings.txt i conf.txt były tracone („Łukasz” -> „Lukasz”), a pliki zapisane przy
        'formacie polskim czytane jako „£ódŸ”
        Try
            Dim cp As Integer = GetACP()
            'Windows z opcją „Użyj UTF-8 dla obsługi języków na całym świecie” - UTF-8 bez znacznika BOM, jak w .NET Framework
            If cp = 65001 Then Return New UTF8Encoding(False)
            If cp > 0 Then Return Encoding.GetEncoding(cp)
        Catch
        End Try
        Return Encoding.GetEncoding(1250)
#Else
        Return Encoding.Default
#End If
    End Function

    ''' <summary>
    ''' W .NET 8 strony kodowe Windows (np. 1250 plików warstw) są dostępne dopiero po rejestracji dostawcy kodowań -
    ''' wywoływane na starcie programu, przed odczytem jakiegokolwiek pliku. W .NET Framework nic nie robi.
    ''' </summary>
    Public Sub RejestrujKodowania()
#If NETCOREAPP Then
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
#End If
    End Sub

    ''' <summary>Ustala folder zapisu danych programu (sprawdza możliwość zapisu w katalogu programu).</summary>
    Public Function UstalFolderDanych(folderProgramu As String) As String
        Try
            Dim plikTestowy As String = Path.Combine(folderProgramu, "zapis_test.tmp")
            File.WriteAllText(plikTestowy, "")
            File.Delete(plikTestowy)
            Return folderProgramu
        Catch
            Dim folderUzytkownika As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MapoTero")
            Directory.CreateDirectory(folderUzytkownika)
            Return folderUzytkownika
        End Try
    End Function

    ''' <summary>
    ''' Liczba wpisana przez użytkownika - akceptuje kropkę i przecinek dziesiętny (Val() przerywał odczyt na przecinku,
    ''' więc "0,5" dawało 0). Gdy tekst nie jest liczbą - wartość domyślna.
    ''' </summary>
    Public Function Wartosc(tekst As String, Optional domyslna As Double = 0) As Double
        If String.IsNullOrWhiteSpace(tekst) Then Return domyslna
        Dim wynik As Double
        'NaN i nieskończoność ("nan", "1e999") nie są liczbami dla użytkownika - dawniej kończyły się wyjątkiem przy zaokrąglaniu
        If Double.TryParse(tekst.Trim().Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture, wynik) AndAlso
           Not Double.IsNaN(wynik) AndAlso Not Double.IsInfinity(wynik) Then Return wynik
        Return domyslna
    End Function

    ''' <summary>Liczba całkowita wpisana przez użytkownika.</summary>
    Public Function WartoscCalkowita(tekst As String, Optional domyslna As Integer = 0) As Integer
        Dim w As Double = Wartosc(tekst, domyslna)
        If w > Integer.MaxValue OrElse w < Integer.MinValue Then Return domyslna
        Return CInt(Math.Round(w))
    End Function

    ''' <summary>Liczba z kropką dziesiętną, bez zbędnych zer.</summary>
    Public Function Liczba(wartosc As Double) As String
        Return MapoTero.Core.Georeferencja.Liczba(wartosc)
    End Function

    ''' <summary>Polska odmiana rzeczownika po liczbie: 1 segment, 2-4 segmenty (bez 12-14), 5 segmentów.</summary>
    Public Function Odmiana(liczba As Long, pojedyncza As String, mnoga24 As String, mnoga As String) As String
        Dim n As Long = Math.Abs(liczba)
        If n = 1 Then Return pojedyncza
        If n Mod 10 >= 2 AndAlso n Mod 10 <= 4 AndAlso (n Mod 100 < 12 OrElse n Mod 100 > 14) Then Return mnoga24
        Return mnoga
    End Function

    ''' <summary>Adres zgłaszania błędów programu.</summary>
    Public Const AdresZgloszenBledow As String = "https://github.com/Eksploracja/MapoTero/issues"

    ''' <summary>Plik z opisem nieobsłużonych błędów programu (w folderze danych).</summary>
    Public Const NazwaPlikuAwarii As String = "awarie.log"

    ''' <summary>Maksymalny rozmiar pliku awarii - większy jest zaczynany od nowa.</summary>
    Private Const MaksRozmiarPlikuAwarii As Long = 1024 * 1024

    Private ReadOnly BlokadaAwarii As New Object()

    ''' <summary>
    ''' Dopisuje opis nieobsłużonego wyjątku (data, wersja programu, system, .NET, stos wywołań) do pliku awarie.log
    ''' w folderze danych programu - a gdy nie jest on jeszcze ustalony, w %LocalAppData%\MapoTero.
    ''' Zwraca ścieżkę pliku albo pusty tekst, gdy zapis się nie udał (zapis awarii sam nie może zgłosić wyjątku).
    ''' </summary>
    Public Function ZapiszAwarie(ex As Exception, Optional folder As String = Nothing) As String
        Try
            If String.IsNullOrEmpty(folder) Then folder = FolderDanych
            If String.IsNullOrEmpty(folder) Then folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MapoTero")
            Directory.CreateDirectory(folder)
            Dim plik As String = Path.Combine(folder, NazwaPlikuAwarii)

            Dim opis As New StringBuilder()
            opis.AppendLine("==== " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            opis.AppendLine("MapoTero " & Wersja & ", " &
                            Runtime.InteropServices.RuntimeInformation.OSDescription & ", " &
                            Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
            opis.AppendLine(If(ex Is Nothing, "(brak opisu wyjątku)", ex.ToString()))
            opis.AppendLine()

            SyncLock BlokadaAwarii
                If File.Exists(plik) AndAlso New FileInfo(plik).Length > MaksRozmiarPlikuAwarii Then File.Delete(plik)
                File.AppendAllText(plik, opis.ToString(), Encoding.UTF8)
            End SyncLock
            Return plik
        Catch
            Return ""
        End Try
    End Function

    ''' <summary>Otwiera folder w Eksploratorze Windows.</summary>
    Public Sub OtworzFolder(folder As String)
        Try
            Process.Start(New ProcessStartInfo(folder) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć folderu " & folder & ": " & Global.MapoTero.Core.OpisBledu.Z(ex), MsgBoxStyle.Exclamation)
        End Try
    End Sub

End Module
