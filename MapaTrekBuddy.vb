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
Imports ICSharpCode.SharpZipLib.Tar
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

    ''' <summary>Główny folder mapy.</summary>
    Public ReadOnly Property FolderMapy As String
        Get
            Return _z.Folder & _z.Prefiks & _z.NazwaTrekBuddy & "\"
        End Get
    End Property

    ''' <summary>Folder segmentów.</summary>
    Public ReadOnly Property FolderSet As String
        Get
            Return FolderMapy & "set\"
        End Get
    End Property

    Private ReadOnly Property PlikSet As String
        Get
            Return FolderMapy & _z.Prefiks & ".set"
        End Get
    End Property

    ''' <summary>Tworzy foldery i plik .map całego obszaru.</summary>
    Public Sub PrzygotujFoldery()
        Directory.CreateDirectory(FolderSet)
        If File.Exists(PlikSet) Then File.Delete(PlikSet)

        Dim s = _z.Siatka
        Dim nazwa As String = _z.Prefiks & "." & _z.Rozszerzenie
        Dim obraz = ZapisGeoreferencji.Obraz(_z.Uklad, s.ZasiegSiatki, s.SzerokoscPx, s.WysokoscPx, _z.Folder, nazwa)
        Georeferencja.Zapisz(FolderMapy & _z.Prefiks & ".map", Georeferencja.MapOzi(obraz), KodowanieSystemowe())
    End Sub

    ''' <summary>Plik .set - lista plików segmentów (raz, w kolejności siatki; dawniej dopisywany przy każdej próbie).</summary>
    Public Sub ZapiszListeSegmentow(pliki As IEnumerable(Of String))
        File.WriteAllLines(PlikSet, pliki, KodowanieSystemowe())
    End Sub

    ''' <summary>Archiwum .tar mapy (TrekBuddy, Locus Map); ścieżki w archiwum z ukośnikiem "/", zgodnie ze standardem TAR.</summary>
    Public Sub UtworzArchiwumTar()
        Dim plikTar As String = _z.Folder & _z.Prefiks & _z.NazwaTrekBuddy & ".tar"
        Using strumien As Stream = File.Create(plikTar)
            Using archiwum As TarArchive = TarArchive.CreateOutputTarArchive(strumien)
                DodajFolder(archiwum, FolderMapy, FolderMapy)
            End Using
        End Using
    End Sub

    Private Shared Sub DodajFolder(archiwum As TarArchive, folderGlowny As String, folder As String)
        For Each plik In Directory.GetFiles(folder)
            Dim wpis As TarEntry = TarEntry.CreateEntryFromFile(plik)
            wpis.Name = plik.Substring(folderGlowny.Length).Replace("\"c, "/"c)
            archiwum.WriteEntry(wpis, False)
        Next
        For Each podfolder In Directory.GetDirectories(folder)
            DodajFolder(archiwum, folderGlowny, podfolder)
        Next
    End Sub

End Class
