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

Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
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
End Class

''' <summary>Wynik scalania.</summary>
Public Class WynikScalania
    Public Property Plik As String = ""
    ''' <summary>Segmenty, których plików nie znaleziono (w arkuszu pozostały białe pola).</summary>
    Public Property Brakujace As New List(Of String)
End Class

''' <summary>
''' Scalanie segmentów w jeden arkusz w samym programie (dawniej zewnętrzny NoToCONS.exe). Arkusz zapisywany
''' jest pasami wierszy - zużycie pamięci zależy od szerokości arkusza, a nie od jego całkowitej wielkości.
''' </summary>
Public NotInheritable Class ScalanieSegmentow

    Private Sub New()
    End Sub

    ''' <summary>Maksymalny rozmiar pasa wierszy w pamięci (bajty).</summary>
    Private Const BudzetPamieci As Long = 128L * 1024 * 1024

    Public Shared Function Scal(z As ZadanieScalania, postep As IProgress(Of Integer), token As CancellationToken) As WynikScalania
        Dim s As Siatka = z.Siatka
        Dim bok As Integer = s.BokSegmentuPx
        Dim kolumny As Integer = s.LiczbaKolumn
        Dim wiersze As Integer = s.LiczbaWierszy
        If Not s.Poprawna OrElse kolumny = 0 OrElse wiersze = 0 Then Throw New ArgumentException("Niepoprawne parametry siatki segmentów")
        If s.SzerokoscPx > Integer.MaxValue \ 3 OrElse s.WysokoscPx > Integer.MaxValue Then Throw New ArgumentException("Arkusz jest zbyt duży")

        Dim szer As Integer = CInt(s.SzerokoscPx)
        Dim wys As Integer = CInt(s.WysokoscPx)
        Dim maks As Integer = ZapisRastra.MaksymalnyBok(z.Format)
        If szer > maks OrElse wys > maks Then
            Throw New ArgumentException("Arkusz " & szer & " x " & wys & " pikseli przekracza możliwości formatu (maks. " & maks & " pikseli) - wybierz GeoTIFF lub PNG")
        End If

        Dim rozszerzenie As String = ZapisRastra.Rozszerzenie(z.Format)
        Dim nazwaPliku As String = z.NazwaArkusza & "." & rozszerzenie
        Dim plik As String = z.Folder & nazwaPliku
        Dim wynik As New WynikScalania With {.Plik = plik}

        'wysokość pasa wierszy mieszczącego się w budżecie pamięci
        Dim pas As Integer = CInt(Math.Max(1L, Math.Min(bok, BudzetPamieci \ (CLng(szer) * 3))))
        Dim bufor(pas * szer * 3 - 1) As Byte
        Dim liniaSegmentu(bok * 3 - 1) As Byte

        Try
            Using zapis = ZapisRastra.Utworz(z.Format, plik, szer, wys, z.JakoscJpeg, z.Uklad, s.ZasiegSiatki)
                For w = 1 To wiersze
                    For y0 = 0 To bok - 1 Step pas
                        token.ThrowIfCancellationRequested()
                        Dim n As Integer = Math.Min(pas, bok - y0)
                        'tło białe - widoczne w miejscu brakujących segmentów
                        For i = 0 To n * szer * 3 - 1
                            bufor(i) = 255
                        Next
                        For k = 1 To kolumny
                            Dim nazwa As String = s.NazwaSegmentu(z.Prefiks, z.Numeracja, w, k) & "." & z.RozszerzenieSegmentow
                            Dim sciezka As String = z.Folder & nazwa
                            If Not File.Exists(sciezka) Then
                                If y0 = 0 Then wynik.Brakujace.Add(nazwa)
                                Continue For
                            End If
                            KopiujSegment(sciezka, bok, y0, n, bufor, szer, (k - 1) * bok, liniaSegmentu)
                        Next
                        zapis.ZapiszWiersze(bufor, n)
                        postep?.Report(CInt(CLng(zapis.Zapisane) * 100 \ wys))
                    Next
                Next
                zapis.Zakoncz()
            End Using
        Catch
            'niedokończony plik arkusza nie może pozostać na dysku
            Try
                File.Delete(plik)
            Catch
            End Try
            Throw
        End Try

        'pliki georeferencyjne arkusza (GeoTIFF ma georeferencję także w samym pliku)
        If z.Georeferencja.Dowolna Then
            Dim obraz = ZapisGeoreferencji.Obraz(z.Uklad, s.ZasiegSiatki, szer, wys, z.Folder, nazwaPliku)
            ZapisGeoreferencji.ZapiszPliki(obraz, z.Folder, z.NazwaArkusza, rozszerzenie, z.Georeferencja)
        End If
        Return wynik
    End Function

    ''' <summary>
    ''' Kopiuje wiersze y0..y0+n-1 segmentu do pasa arkusza (od kolumny pikseli x0). Segment jest sprowadzany do formatu
    ''' RGB 24 bity na białym tle (obrazy z paletą lub przezroczystością) i - gdy ma inny rozmiar - skalowany do boku segmentu.
    ''' </summary>
    Private Shared Sub KopiujSegment(sciezka As String, bok As Integer, y0 As Integer, n As Integer,
                                     bufor() As Byte, szerokoscArkusza As Integer, x0 As Integer, linia() As Byte)
        Using zrodlo As Image = Image.FromFile(sciezka)
            Using rgb As New Bitmap(bok, bok, PixelFormat.Format24bppRgb)
                Using g As Graphics = Graphics.FromImage(rgb)
                    g.Clear(Color.White)
                    'segment o właściwym rozmiarze kopiowany jest piksel w piksel, inny - skalowany
                    If zrodlo.Width = bok AndAlso zrodlo.Height = bok Then
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                    Else
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    End If
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    g.DrawImage(zrodlo, New Rectangle(0, 0, bok, bok), 0, 0, zrodlo.Width, zrodlo.Height, GraphicsUnit.Pixel)
                End Using
                Dim dane As BitmapData = rgb.LockBits(New Rectangle(0, y0, bok, n), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
                Try
                    For r = 0 To n - 1
                        Marshal.Copy(IntPtr.Add(dane.Scan0, r * dane.Stride), linia, 0, bok * 3)
                        Dim cel As Integer = (r * szerokoscArkusza + x0) * 3
                        'GDI+ przechowuje piksele w kolejności BGR
                        For i = 0 To bok - 1
                            bufor(cel + i * 3) = linia(i * 3 + 2)
                            bufor(cel + i * 3 + 1) = linia(i * 3 + 1)
                            bufor(cel + i * 3 + 2) = linia(i * 3)
                        Next
                    Next
                Finally
                    rgb.UnlockBits(dane)
                End Try
            End Using
        End Using
    End Sub

End Class
