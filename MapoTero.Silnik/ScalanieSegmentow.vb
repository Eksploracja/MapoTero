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
Imports System.Threading
Imports MapoTero.Core

''' <summary>Parametry scalania segmentów w jeden arkusz.</summary>
Public Class ZadanieScalania
    ''' <summary>Folder segmentów (zakończony "\").</summary>
    Public Property Folder As String = ""
    Public Property Prefiks As String = "_"
    Public Property Numeracja As StylNumeracji = StylNumeracji.WierszKolumna
    ''' <summary>Rozszerzenie plików segmentów (jpg, png, tif, gif).</summary>
    Public Property RozszerzenieSegmentow As String = "jpg"
    ''' <summary>Siatka segmentów (liczba wierszy i kolumn, bok segmentu, zasięg).</summary>
    Public Property Siatka As Siatka
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Public Property Format As FormatArkusza = FormatArkusza.GeoTiff
    Public Property JakoscJpeg As Integer = 75
    ''' <summary>Nazwa pliku arkusza bez rozszerzenia.</summary>
    Public Property NazwaArkusza As String = ""
    Public Property Georeferencja As New OpcjeGeoreferencji()
    ''' <summary>Segmenty z przezroczystym tłem - arkusz PNG i GeoTIFF bezstratny z kanałem alfa (inne formaty: tło białe).</summary>
    Public Property Przezroczystosc As Boolean
    ''' <summary>Opcjonalna korekta jakości obrazu (kontrast, wyostrzanie, MIP).</summary>
    Public Property Korekta As OpcjeKorekcjiMip
End Class

''' <summary>Wynik scalania.</summary>
Public Class WynikScalania
    Public Property Plik As String = ""
    ''' <summary>Segmenty, których plików nie znaleziono (w arkuszu pozostały białe pola).</summary>
    Public Property Brakujace As New List(Of String)
End Class

''' <summary>
''' Scalanie segmentów w jeden arkusz - od wersji 3.19 przez silnik arkusza okna Eksport mapy (EksportMapy, format
''' Arkusz). Dawniej osobny kod (z czasów zastąpienia NoToCONS.exe) robił to samo co eksport GeoTIFF, a różnice
''' (formaty, nakładki, zapis przez plik tymczasowy) wynikały z tego, że rozwijano je osobno. Klasa zostaje jako
''' prosty interfejs scalania dla testów i dla kodu, który nie potrzebuje nakładek.
''' </summary>
Public NotInheritable Class ScalanieSegmentow

    Private Sub New()
    End Sub

    Public Shared Function Scal(z As ZadanieScalania, postep As IProgress(Of Integer), token As CancellationToken) As WynikScalania
        Dim s As Siatka = z.Siatka
        If s Is Nothing OrElse Not s.Poprawna OrElse s.LiczbaKolumn = 0 OrElse s.LiczbaWierszy = 0 Then
            Throw New ArgumentException("Niepoprawne parametry siatki segmentów")
        End If
        Dim folder As String = If(String.IsNullOrEmpty(z.Folder), "", If(z.Folder.EndsWith("\") OrElse z.Folder.EndsWith("/"), z.Folder, z.Folder & Path.DirectorySeparatorChar))
        Dim plik As String = folder & z.NazwaArkusza & "." & ZapisRastra.Rozszerzenie(z.Format)
        Dim zadanie As New ZadanieEksportu With {
            .Rodzaj = RodzajEksportu.GeoTiff, .Plik = plik, .Uklad = z.Uklad, .FormatArkusza = z.Format, .JakoscJpeg = z.JakoscJpeg,
            .Zrodlo = New ZrodloSegmentow(folder, z.Prefiks, z.Numeracja, z.RozszerzenieSegmentow, s),
            .KaflePng = z.Przezroczystosc, .Georeferencja = If(z.Georeferencja, New OpcjeGeoreferencji()),
            .KorekcjaMip = If(z.Korekta, New OpcjeKorekcjiMip())}
        EksportMapy.Eksportuj(zadanie, postep, token)
        Dim wynik As New WynikScalania With {.Plik = plik}
        wynik.Brakujace.AddRange(zadanie.BrakujaceSegmenty)
        Return wynik
    End Function

    ''' <summary>Największy rozmiar pasa wierszy w pamięci (bajty; testy zmniejszają go, by sprawdzić scalanie pasami).</summary>
    Public Shared Property MaksWierszWPamieci As Long
        Get
            Return EksportMapy.BudzetPamieciGeoTiff
        End Get
        Set(value As Long)
            EksportMapy.BudzetPamieciGeoTiff = value
        End Set
    End Property

End Class
