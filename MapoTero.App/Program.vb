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

    ''' <summary>Adres zgłaszania błędów programu.</summary>
    Public Const AdresZgloszenBledow As String = "https://github.com/Eksploracja/MapoTero/issues"
    ''' <summary>Dział MapoTero na Pomorskim Forum Eksploracyjnym (menu Pomoc - Zadaj pytanie na forum).</summary>
    Public Const AdresForumPfe As String = "https://forum.eksploracja.pl/viewforum.php?f=205"

    ''' <summary>Lista wydań programu z opisem zmian (menu Pomoc > Co nowego w programie).</summary>
    Public Const AdresWydan As String = "https://github.com/Eksploracja/MapoTero/releases"

    ''' <summary>Strona wsparcia autora programu (menu Pomoc > Wesprzyj rozwój programu, okno O programie).</summary>
    Public Const AdresWsparcia As String = "https://buycoffee.to/mapy"

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
    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential, CharSet:=Runtime.InteropServices.CharSet.Unicode)>
    Private Structure SHFILEOPSTRUCT
        Public hwnd As IntPtr
        Public wFunc As UInteger
        Public pFrom As String
        Public pTo As String
        Public fFlags As UShort
        Public fAnyOperationsAborted As Integer
        Public hNameMappings As IntPtr
        Public lpszProgressTitle As String
    End Structure

    Private Declare Unicode Function SHFileOperation Lib "shell32.dll" Alias "SHFileOperationW" (ByRef op As SHFILEOPSTRUCT) As Integer

    Private Const FO_DELETE As UInteger = 3
    Private Const FOF_SILENT As UShort = &H4
    Private Const FOF_NOCONFIRMATION As UShort = &H10
    Private Const FOF_ALLOWUNDO As UShort = &H40
    Private Const FOF_NOERRORUI As UShort = &H400
    Private Const FOF_WANTNUKEWARNING As UShort = &H4000

    ''' <summary>
    ''' Przenosi pliki do Kosza paczkami po 500 jedną operacją Windows. Dawniej każdy plik osobno
    ''' (FileSystem.DeleteFile) - ok. 70 ms na plik, więc usunięcie 30 000 plików sesji blokowało okno na pół godziny.
    ''' Pliki, których paczka nie dała się przenieść, przenoszone są pojedynczo; zwraca liczbę plików, których nie przeniesiono.
    ''' Na dysku bez Kosza (np. pendrive) Windows pyta, czy usunąć pliki trwale.
    ''' </summary>
    Public Function PrzeniesDoKosza(pliki As IEnumerable(Of String)) As Integer
        Dim bledy As Integer = 0
        Dim lista = pliki.Where(Function(p) File.Exists(p)).ToList()
        'wiele plików: Kosz i tak przenosi każdy plik osobno (ok. 18 ms) - pliki jednego folderu przenoszone są do podfolderu
        '(zmiana nazwy, ułamek milisekundy) i do Kosza trafia ten jeden folder (30 000 plików: ok. 20 s zamiast 9 minut)
        If lista.Count > MaksPlikowPojedynczo Then
            Dim pozostale As New List(Of String)
            For Each grupa In lista.GroupBy(Function(p) Path.GetDirectoryName(p), StringComparer.OrdinalIgnoreCase)
                If Not PrzeniesDoKoszaJakoFolder(grupa.Key, grupa.ToList()) Then pozostale.AddRange(grupa)
            Next
            lista = pozostale.Where(Function(p) File.Exists(p)).ToList()
        End If
        For start = 0 To lista.Count - 1 Step 500
            Dim paczka = lista.Skip(start).Take(500).ToList()
            Dim op As New SHFILEOPSTRUCT With {
                .wFunc = FO_DELETE,
                .pFrom = String.Join(vbNullChar, paczka) & vbNullChar & vbNullChar,
                .fFlags = FOF_ALLOWUNDO Or FOF_NOCONFIRMATION Or FOF_SILENT Or FOF_NOERRORUI Or FOF_WANTNUKEWARNING}
            Dim wynik As Integer = -1
            Try
                wynik = SHFileOperation(op)
            Catch
            End Try
            If wynik = 0 AndAlso op.fAnyOperationsAborted = 0 Then Continue For
            For Each plik In paczka
                If Not File.Exists(plik) Then Continue For
                Try
                    FileIO.FileSystem.DeleteFile(plik, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
                Catch
                    bledy += 1
                End Try
            Next
        Next
        Return bledy
    End Function

    ''' <summary>Powyżej tej liczby plików Kosz dostaje jeden folder z plikami zamiast pojedynczych plików.</summary>
    Public Const MaksPlikowPojedynczo As Integer = 50

    ''' <summary>Początek nazwy folderu, w którym pliki trafiają do Kosza (data i godzina w nazwie).</summary>
    Public Const NazwaFolderuKosza As String = "MapoTero - usunięte pliki"

    ''' <summary>
    ''' Przenosi pliki jednego folderu do nowego podfolderu i ten podfolder do Kosza. Gdy coś się nie uda, pliki wracają na
    ''' swoje miejsce i zwracane jest False (wtedy przenoszone są zwykłym sposobem).
    ''' </summary>
    Private Function PrzeniesDoKoszaJakoFolder(folder As String, pliki As List(Of String)) As Boolean
        Dim cel As String = Path.Combine(folder, NazwaFolderuKosza & " " & DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture))
        Dim przeniesione As New List(Of (Zrodlowy As String, Docelowy As String))
        Try
            If Directory.Exists(cel) Then cel &= " " & Guid.NewGuid().ToString("N").Substring(0, 6)
            Directory.CreateDirectory(cel)
            For Each p In pliki
                Dim docelowy = Path.Combine(cel, Path.GetFileName(p))
                Try
                    File.Move(p, docelowy)
                    przeniesione.Add((p, docelowy))
                Catch
                    'plik zajęty - zostaje i zostanie przeniesiony zwykłym sposobem (albo policzony jako błąd)
                End Try
            Next
            FileIO.FileSystem.DeleteDirectory(cel, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
            Return True
        Catch
            For Each para In przeniesione
                Try
                    If File.Exists(para.Docelowy) Then File.Move(para.Docelowy, para.Zrodlowy)
                Catch
                End Try
            Next
            Try
                If Directory.Exists(cel) AndAlso Not Directory.EnumerateFileSystemEntries(cel).Any() Then Directory.Delete(cel)
            Catch
            End Try
            Return False
        End Try
    End Function

    ''' <summary>Otwiera adres w domyślnej przeglądarce internetowej; błąd pokazuje w komunikacie z podanym tytułem.</summary>
    Public Sub OtworzStrone(adres As String, tytul As String)
        Try
            Process.Start(New ProcessStartInfo(adres) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć strony " & adres & ": " & Global.MapoTero.Core.OpisBledu.Z(ex), MsgBoxStyle.Exclamation, tytul)
        End Try
    End Sub

    Public Sub OtworzFolder(folder As String)
        Try
            Process.Start(New ProcessStartInfo(folder) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć folderu " & folder & ": " & Global.MapoTero.Core.OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Otwieranie folderu")
        End Try
    End Sub

End Module
