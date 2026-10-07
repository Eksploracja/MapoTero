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

    ''' <summary>Folder pobierania domyślny dla programu.</summary>
    Public ReadOnly Property FolderDownload As String
        Get
            Return FolderDanych & "\download\"
        End Get
    End Property

    ''' <summary>Plik ustawień programu.</summary>
    Public ReadOnly Property PlikLastsettings As String
        Get
            Return FolderDanych & "\lastsettings.txt"
        End Get
    End Property

    ''' <summary>
    ''' Kodowanie plików tekstowych programu (conf.txt, lastsettings.txt, pliki georeferencyjne): systemowa strona
    ''' kodowa ANSI - takie samo, jakiego używały instrukcje FileOpen/Print poprzednich wersji.
    ''' </summary>
    Public Function KodowanieSystemowe() As Encoding
#If NETCOREAPP Then
        RejestrujKodowania()
        Return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
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
        If Double.TryParse(tekst.Trim().Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture, wynik) Then Return wynik
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

    ''' <summary>Otwiera folder w Eksploratorze Windows.</summary>
    Public Sub OtworzFolder(folder As String)
        Try
            Process.Start(New ProcessStartInfo(folder) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć folderu " & folder & ": " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
    End Sub

End Module
