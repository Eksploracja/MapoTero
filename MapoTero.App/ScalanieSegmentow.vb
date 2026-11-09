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
    ''' <summary>Segmenty z przezroczystym tłem - arkusz PNG i GeoTIFF bezstratny z kanałem alfa (inne formaty: tło białe).</summary>
    Public Property Przezroczystosc As Boolean
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
        If s Is Nothing OrElse Not s.Poprawna OrElse s.LiczbaKolumn = 0 OrElse s.LiczbaWierszy = 0 Then
            Throw New ArgumentException("Niepoprawne parametry siatki segmentów")
        End If
        Dim bok As Integer = s.BokSegmentuPx
        Dim kolumny As Integer = s.LiczbaKolumn
        Dim wiersze As Integer = s.LiczbaWierszy
        'RGBA, gdy segmenty mają przezroczyste tło, a format arkusza ją zachowuje (dawniej zawsze RGB z białym tłem)
        Dim kanaly As Integer = If(z.Przezroczystosc AndAlso ZapisRastra.ObslugujePrzezroczystosc(z.Format), 4, 3)
        If s.SzerokoscPx > Integer.MaxValue \ kanaly OrElse s.WysokoscPx > Integer.MaxValue Then Throw New ArgumentException("Arkusz jest zbyt duży")

        Dim szer As Integer = CInt(s.SzerokoscPx)
        Dim wys As Integer = CInt(s.WysokoscPx)
        Dim maks As Integer = ZapisRastra.MaksymalnyBok(z.Format)
        If szer > maks OrElse wys > maks Then
            Throw New ArgumentException("Arkusz " & szer & " x " & wys & " pikseli przekracza możliwości formatu (maks. " & maks & " pikseli) - wybierz GeoTIFF lub PNG")
        End If

        Dim folder As String = If(String.IsNullOrEmpty(z.Folder), "", If(z.Folder.EndsWith("\") OrElse z.Folder.EndsWith("/"), z.Folder, z.Folder & Path.DirectorySeparatorChar))
        Dim rozszerzenie As String = ZapisRastra.Rozszerzenie(z.Format)
        Dim nazwaPliku As String = z.NazwaArkusza & "." & rozszerzenie
        Dim plik As String = folder & nazwaPliku
        Dim wynik As New WynikScalania With {.Plik = plik}

        'wysokość pasa wierszy mieszczącego się w budżecie pamięci
        Dim pas As Integer = CInt(Math.Max(1L, Math.Min(bok, BudzetPamieci \ (CLng(szer) * kanaly))))
        Dim bufor(pas * szer * kanaly - 1) As Byte
        Dim liniaSegmentu(bok * kanaly - 1) As Byte
        Dim formatPikseli As PixelFormat = If(kanaly = 4, PixelFormat.Format32bppArgb, PixelFormat.Format24bppRgb)

        'segmenty wiersza wczytywane raz dla wszystkich pasów, o ile cały wiersz zdekodowanych segmentów mieści się w limicie
        'pamięci; przy bardzo szerokim arkuszu (dawniej zawsze, nawet ponad 1 GB na wiersz) - każdy segment osobno dla pasa
        Dim caleWiersze As Boolean = CLng(szer) * bok * kanaly <= MaksWierszWPamieci

        Try
            Using zapis = ZapisRastra.Utworz(z.Format, plik, szer, wys, z.JakoscJpeg, z.Uklad, s.ZasiegSiatki, kanaly = 4)
                For w = 1 To wiersze
                    Dim segmentyWiersza As New Dictionary(Of Integer, Bitmap)()
                    'segmenty brakujące lub uszkodzone - zgłaszane raz, a nie przy każdym pasie
                    Dim bezObrazu As New HashSet(Of Integer)()
                    Dim wiersz As Integer = w
                    Dim segmentKolumny = Function(k As Integer) As Bitmap
                                             If bezObrazu.Contains(k) Then Return Nothing
                                             Dim nazwa As String = s.NazwaSegmentu(z.Prefiks, z.Numeracja, wiersz, k) & "." & z.RozszerzenieSegmentow
                                             Dim rgb = WczytajSegment(folder & nazwa, bok, formatPikseli, kanaly)
                                             If rgb Is Nothing Then
                                                 bezObrazu.Add(k)
                                                 wynik.Brakujace.Add(nazwa)
                                             End If
                                             Return rgb
                                         End Function
                    Try
                        If caleWiersze Then
                            For k = 1 To kolumny
                                Dim rgb = segmentKolumny(k)
                                If rgb IsNot Nothing Then segmentyWiersza(k) = rgb
                            Next
                        End If

                        For y0 = 0 To bok - 1 Step pas
                            token.ThrowIfCancellationRequested()
                            Dim n As Integer = Math.Min(pas, bok - y0)
                            'tło białe (przy przezroczystości - przezroczyste) - widoczne w miejscu brakujących segmentów
                            Array.Fill(bufor, CByte(If(kanaly = 4, 0, 255)), 0, n * szer * kanaly)

                            For k = 1 To kolumny
                                Dim rgb As Bitmap = Nothing
                                If caleWiersze Then
                                    If Not segmentyWiersza.TryGetValue(k, rgb) Then Continue For
                                Else
                                    rgb = segmentKolumny(k)
                                    If rgb Is Nothing Then Continue For
                                End If
                                Try
                                    KopiujPas(rgb, y0, n, (k - 1) * bok, bok, szer, kanaly, formatPikseli, bufor, liniaSegmentu)
                                Finally
                                    If Not caleWiersze Then rgb.Dispose()
                                End Try
                            Next
                            zapis.ZapiszWiersze(bufor, n)
                            postep?.Report(CInt(CLng(zapis.Zapisane) * 100 \ wys))
                        Next
                    Finally
                        For Each b In segmentyWiersza.Values
                            b.Dispose()
                        Next
                        segmentyWiersza.Clear()
                    End Try
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
            Dim obraz = ZapisGeoreferencji.Obraz(z.Uklad, s.ZasiegSiatki, szer, wys, folder, nazwaPliku)
            ZapisGeoreferencji.ZapiszPliki(obraz, folder, z.NazwaArkusza, rozszerzenie, z.Georeferencja)
        End If
        Return wynik
    End Function

    ''' <summary>Największy rozmiar wiersza zdekodowanych segmentów trzymanego w pamięci (bajty; testy zmniejszają go, by sprawdzić scalanie pasami).</summary>
    Public Shared Property MaksWierszWPamieci As Long = 512L * 1024 * 1024

    ''' <summary>
    ''' Segment przeskalowany do boku siatki (RGB albo RGBA); Nothing - brak pliku lub plik uszkodzony. Brak pamięci
    ''' przy tworzeniu bitmapy nie jest już zgłaszany jako brakujący segment (białe pole w arkuszu), lecz jako błąd scalania.
    ''' </summary>
    Private Shared Function WczytajSegment(sciezka As String, bok As Integer, formatPikseli As PixelFormat, kanaly As Integer) As Bitmap
        If Not File.Exists(sciezka) Then Return Nothing
        Dim zrodlo As Image
        Try
            zrodlo = Image.FromFile(sciezka)
        Catch
            'GDI+ zgłasza uszkodzony lub nieobsługiwany plik m.in. wyjątkiem OutOfMemoryException
            Return Nothing
        End Try
        Using zrodlo
            Dim rgb As New Bitmap(bok, bok, formatPikseli)
            Try
                Using g As Graphics = Graphics.FromImage(rgb)
                    g.Clear(If(kanaly = 4, Color.Transparent, Color.White))
                    If zrodlo.Width = bok AndAlso zrodlo.Height = bok Then
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                    Else
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    End If
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    g.DrawImage(zrodlo, New Rectangle(0, 0, bok, bok), 0, 0, zrodlo.Width, zrodlo.Height, GraphicsUnit.Pixel)
                End Using
            Catch
                rgb.Dispose()
                Throw
            End Try
            Return rgb
        End Using
    End Function

    ''' <summary>Kopiuje pas wierszy segmentu (od wiersza y0, n wierszy) do bufora arkusza od kolumny pikseli x0.</summary>
    Private Shared Sub KopiujPas(rgb As Bitmap, y0 As Integer, n As Integer, x0 As Integer, bok As Integer, szer As Integer, kanaly As Integer,
                                 formatPikseli As PixelFormat, bufor() As Byte, liniaSegmentu() As Byte)
        Dim dane As BitmapData = rgb.LockBits(New Rectangle(0, y0, bok, n), ImageLockMode.ReadOnly, formatPikseli)
        Try
            For r = 0 To n - 1
                Marshal.Copy(IntPtr.Add(dane.Scan0, r * dane.Stride), liniaSegmentu, 0, bok * kanaly)
                Dim cel As Integer = (r * szer + x0) * kanaly
                'GDI+ przechowuje piksele w kolejności BGR (BGRA)
                For i = 0 To bok - 1
                    Dim zr As Integer = i * kanaly, dc As Integer = cel + i * kanaly
                    bufor(dc) = liniaSegmentu(zr + 2)
                    bufor(dc + 1) = liniaSegmentu(zr + 1)
                    bufor(dc + 2) = liniaSegmentu(zr)
                    If kanaly = 4 Then bufor(dc + 3) = liniaSegmentu(zr + 3)
                Next
            Next
        Finally
            rgb.UnlockBits(dane)
        End Try
    End Sub

End Class
