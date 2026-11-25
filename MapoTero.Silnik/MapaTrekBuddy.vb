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

Imports System.IO
Imports System.Formats.Tar
Imports MapoTero.Core

''' <summary>
''' Mapa w formacie TrekBuddy / Locus Map: folder z plikiem .map całego obszaru, plikiem .set (lista segmentów)
''' i podfolderem "set" z segmentami nazwanymi wg położenia w pikselach; całość pakowana do archiwum .tar.
''' </summary>
Public Class MapaTrekBuddy

    ''' <summary>Bok segmentu mapy TrekBuddy w pikselach.</summary>
    Public Const BokSegmentu As Integer = 512

    Private ReadOnly _z As ZadaniePobierania

    Public Sub New(zadanie As ZadaniePobierania)
        _z = zadanie
    End Sub

    ''' <summary>Usuwa lub zastępuje znaki niedozwolone w nazwach plików i folderów systemu Windows.</summary>
    Public Shared Function BezpiecznaNazwa(tekst As String) As String
        If String.IsNullOrEmpty(tekst) Then Return ""
        Return System.Text.RegularExpressions.Regex.Replace(tekst, "[\\/:*?""<>|]", "_").Trim()
    End Function

    ''' <summary>Nazwa używana, gdy przedrostek (i dopisek) są puste.</summary>
    Private Const NazwaDomyslna As String = "mapa"

    ''' <summary>
    ''' Nazwa folderu i archiwum mapy: przedrostek z dopiskiem. Przy pustym przedrostku i dopisku folderem mapy był sam
    ''' folder sesji, a archiwum TAR całego folderu sesji powstawało w nim samym.
    ''' </summary>
    Private ReadOnly Property NazwaMapy As String
        Get
            Dim nazwa As String = _z.Prefiks & BezpiecznaNazwa(_z.NazwaTrekBuddy)
            Return If(nazwa.Trim() = "", NazwaDomyslna, nazwa)
        End Get
    End Property

    ''' <summary>Nazwa plików .map i .set: przedrostek (pusty - nazwa domyślna).</summary>
    Private ReadOnly Property NazwaPlikow As String
        Get
            Return If(_z.Prefiks = "", NazwaDomyslna, _z.Prefiks)
        End Get
    End Property

    ''' <summary>Główny folder mapy.</summary>
    Public ReadOnly Property FolderMapy As String
        Get
            Return Path.Combine(_z.Folder, NazwaMapy) & Path.DirectorySeparatorChar
        End Get
    End Property

    ''' <summary>Folder segmentów.</summary>
    Public ReadOnly Property FolderSet As String
        Get
            Return Path.Combine(FolderMapy, "set") & Path.DirectorySeparatorChar
        End Get
    End Property

    Private ReadOnly Property PlikSet As String
        Get
            Return Path.Combine(FolderMapy, NazwaPlikow & ".set")
        End Get
    End Property

    ''' <summary>Tworzy foldery i plik .map całego obszaru.</summary>
    Public Sub PrzygotujFoldery()
        Directory.CreateDirectory(FolderSet)
        If File.Exists(PlikSet) Then File.Delete(PlikSet)

        Dim s = _z.Siatka
        Dim nazwa As String = NazwaPlikow & "." & _z.Rozszerzenie
        Dim obraz = ZapisGeoreferencji.Obraz(_z.Uklad, s.ZasiegSiatki, s.SzerokoscPx, s.WysokoscPx, _z.Folder, nazwa)
        Georeferencja.Zapisz(Path.Combine(FolderMapy, NazwaPlikow & ".map"), Georeferencja.MapOzi(obraz), KodowanieSystemowe())
    End Sub

    ''' <summary>Plik .set - lista plików segmentów (raz, w kolejności siatki; dawniej dopisywany przy każdej próbie).</summary>
    Public Sub ZapiszListeSegmentow(pliki As IEnumerable(Of String))
        File.WriteAllLines(PlikSet, pliki, KodowanieSystemowe())
    End Sub

    ''' <summary>Archiwum .tar mapy (TrekBuddy, Locus Map) w formacie ustar (<see cref="UtworzTar"/>).</summary>
    Public Sub UtworzArchiwumTar()
        Dim nazwaTar As String = NazwaMapy & ".tar"
        Dim plikTar As String = Path.Combine(_z.Folder, nazwaTar)
        Dim plikTmp As String = plikTar & ".tmp"
        Try
            If File.Exists(plikTmp) Then File.Delete(plikTmp)
            UtworzTar(FolderMapy, plikTmp)
            File.Move(plikTmp, plikTar, overwrite:=True)
        Catch ex As Exception
            Try
                If File.Exists(plikTmp) Then File.Delete(plikTmp)
            Catch
            End Try
            Throw New IOException("Nie udało się utworzyć archiwum TAR (" & plikTar & "): " & OpisBledu.Z(ex), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Archiwum TAR w formacie ustar - folder mapy bez folderu nadrzędnego, ścieżki z ukośnikiem „/”.
    ''' TarFile.CreateFromDirectory zapisuje format PAX, w którym przed każdym plikiem stoi dodatkowy wpis
    ''' „PaxHeaders…” z czasami pliku; dawna biblioteka SharpZipLib (i programy, które czytają mapy TrekBuddy)
    ''' używały zwykłego ustar. Nazwy kafli są krótkie, więc ograniczenia ustar (100 + 155 znaków) nie przeszkadzają.
    ''' </summary>
    Friend Shared Sub UtworzTar(folder As String, plikTar As String)
        Using fs = File.Create(plikTar), w As New TarWriter(fs, TarEntryFormat.Ustar, leaveOpen:=False)
            DodajFolderDoTar(w, folder, "")
        End Using
    End Sub

    Private Shared Sub DodajFolderDoTar(w As TarWriter, folder As String, prefiks As String)
        For Each plik In Directory.GetFiles(folder).OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase)
            w.WriteEntry(plik, prefiks & Path.GetFileName(plik))
        Next
        For Each podfolder In Directory.GetDirectories(folder).OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase)
            'same pliki, bez wpisów folderów - tak jak archiwa z dawnej wersji (SharpZipLib)
            DodajFolderDoTar(w, podfolder, prefiks & Path.GetFileName(podfolder) & "/")
        Next
    End Sub
End Class

''' <summary>
''' Eksport mapy TrekBuddy / Locus Map z okna Eksport mapy: arkusz sesji (z nakładkami i korektą, jak arkusz GeoTIFF)
''' cięty pasami na kafle 512 x 512 px nazwane wg położenia w pikselach (_x_y), plik .map całego obszaru, lista kafli
''' .set i archiwum .tar. Dawniej mapę TrekBuddy trzeba było włączyć w Ustawieniach przed pobieraniem (segmenty 512 px
''' i paczka powstawały przy pobieraniu), więc z gotowej sesji nie dało się jej zrobić.
''' </summary>
Public Class ZapisKafliTrekBuddy
    Inherits ZapisRastra

    Private ReadOnly _folderSet As String
    Private ReadOnly _nazwa As String
    Private ReadOnly _jakosc As Integer
    Private ReadOnly _pas() As Byte
    Private _wierszWPasie As Integer
    Private _pasOd As Integer
    Private ReadOnly _pliki As New List(Of String)

    ''' <param name="folderMapy">Folder roboczy mapy (powstaje w nim podfolder set).</param>
    ''' <param name="nazwa">Nazwa mapy - przedrostek nazw kafli i nazwa plików .map i .set.</param>
    Public Sub New(folderMapy As String, nazwa As String, szerokosc As Integer, wysokosc As Integer, jakosc As Integer)
        MyBase.New(szerokosc, wysokosc, 3)
        _folderSet = Path.Combine(folderMapy, "set")
        _nazwa = nazwa
        _jakosc = jakosc
        Directory.CreateDirectory(_folderSet)
        Dim rozmiarPas As Long = CLng(MapaTrekBuddy.BokSegmentu) * CLng(szerokosc) * 3L
        If rozmiarPas <= 0 OrElse rozmiarPas > Integer.MaxValue Then
            Throw New ArgumentOutOfRangeException(NameOf(szerokosc), "Szerokość mapy TrekBuddy przekracza dopuszczalny rozmiar bufora.")
        End If
        ReDim _pas(CInt(rozmiarPas) - 1)
    End Sub

    ''' <summary>Nazwy plików kafli (w kolejności wierszy) - zawartość pliku .set.</summary>
    Public ReadOnly Property Pliki As List(Of String)
        Get
            Return _pliki
        End Get
    End Property

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim bajtyWiersza As Integer = Szerokosc * 3
        Dim r As Integer = 0
        While r < liczbaWierszy
            Dim ile As Integer = Math.Min(liczbaWierszy - r, MapaTrekBuddy.BokSegmentu - _wierszWPasie)
            Buffer.BlockCopy(rgb, r * bajtyWiersza, _pas, _wierszWPasie * bajtyWiersza, ile * bajtyWiersza)
            _wierszWPasie += ile
            r += ile
            If _wierszWPasie = MapaTrekBuddy.BokSegmentu Then ZapiszPas()
        End While
    End Sub

    ''' <summary>Tnie pas wierszy na kafle; kafle na prawym i dolnym brzegu uzupełniane bielą do pełnego boku.</summary>
    Private Sub ZapiszPas()
        If _wierszWPasie = 0 Then Exit Sub
        Dim bok As Integer = MapaTrekBuddy.BokSegmentu
        Dim ci = Globalization.CultureInfo.InvariantCulture
        For x0 = 0 To Szerokosc - 1 Step bok
            Dim w As Integer = Math.Min(bok, Szerokosc - x0)
            Using bmp As New Bitmap(bok, bok, Imaging.PixelFormat.Format24bppRgb)
                Dim dane = bmp.LockBits(New Rectangle(0, 0, bok, bok), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format24bppRgb)
                Try
                    Dim linia(bok * 3 - 1) As Byte
                    For y = 0 To bok - 1
                        Array.Fill(linia, CByte(255))
                        If y < _wierszWPasie Then
                            Dim zr As Integer = (y * Szerokosc + x0) * 3
                            For i = 0 To w - 1
                                'GDI+ przechowuje piksele w kolejności BGR
                                linia(i * 3) = _pas(zr + i * 3 + 2)
                                linia(i * 3 + 1) = _pas(zr + i * 3 + 1)
                                linia(i * 3 + 2) = _pas(zr + i * 3)
                            Next
                        End If
                        Runtime.InteropServices.Marshal.Copy(linia, 0, IntPtr.Add(dane.Scan0, y * dane.Stride), bok * 3)
                    Next
                Finally
                    bmp.UnlockBits(dane)
                End Try
                Dim nazwaPliku As String = _nazwa & "_" & x0.ToString(ci) & "_" & _pasOd.ToString(ci) & ".jpg"
                File.WriteAllBytes(Path.Combine(_folderSet, nazwaPliku), EksportMapy.Koduj(bmp, False, _jakosc))
                _pliki.Add(nazwaPliku)
            End Using
        Next
        _pasOd += _wierszWPasie
        _wierszWPasie = 0
    End Sub

    Public Overrides Sub Zakoncz()
        ZapiszPas()
    End Sub

    Public Overrides Sub Dispose()
    End Sub

    ''' <summary>
    ''' Kończy mapę: plik .map całego obszaru i .set w folderze mapy, archiwum .tar (System.Formats.Tar) i usunięcie
    ''' folderu roboczego.
    ''' </summary>
    Public Shared Sub UtworzPaczke(folderMapy As String, nazwa As String, pliki As IEnumerable(Of String), uklad As UkladWspolrzednych,
                                   zasieg As Zasieg, szerokosc As Integer, wysokosc As Integer, plikTar As String)
        Dim obraz = ZapisGeoreferencji.Obraz(uklad, zasieg, szerokosc, wysokosc, folderMapy, nazwa & ".jpg")
        Georeferencja.Zapisz(Path.Combine(folderMapy, nazwa & ".map"), Georeferencja.MapOzi(obraz), KodowanieSystemowe())
        File.WriteAllLines(Path.Combine(folderMapy, nazwa & ".set"), pliki, KodowanieSystemowe())
        Dim plikTmp As String = plikTar & ".tmp"
        Try
            If File.Exists(plikTmp) Then File.Delete(plikTmp)
            MapaTrekBuddy.UtworzTar(folderMapy, plikTmp)
            File.Move(plikTmp, plikTar, overwrite:=True)
        Catch
            Try
                If File.Exists(plikTmp) Then File.Delete(plikTmp)
            Catch
            End Try
            Throw
        End Try
        Directory.Delete(folderMapy, True)
    End Sub
End Class
