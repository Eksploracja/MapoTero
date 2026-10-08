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

Imports System.Collections.Concurrent
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Źródło segmentów z usługi WMTS. Usługa udostępnia gotowe kafle o stałych poziomach szczegółowości, więc segment
''' składany jest z kafli wybranego poziomu i przeliczany na siatkę segmentów (rozmiar piksela i układ współrzędnych
''' wybrane przez użytkownika). Dzięki temu segmenty WMTS mają te same pliki georeferencji co segmenty WMS i działają
''' z nimi scalanie, eksport i paczki TrekBuddy. Pobrane kafle przechowywane są w folderze _kafle_wmts sesji
''' (wspólne kafle sąsiednich segmentów pobierane są raz, ponowienie po błędzie nie pobiera ich ponownie);
''' folder jest usuwany po pobraniu wszystkich segmentów, chyba że w ustawieniach wybrano zachowywanie kafli.
''' </summary>
Public Class ZrodloWmts

    ''' <summary>Plany kolejnych warstw (kolejność rysowania jak w WMS - pierwsza warstwa na spodzie).</summary>
    Public ReadOnly Property Plany As New List(Of PlanWmts)
    Public ReadOnly Property FolderKafli As String

    'liczba kafli pobieranych jednocześnie dla jednego segmentu
    Private Const WatkiKafli As Integer = 4
    'największa mozaika kafli jednej warstwy segmentu (piksele)
    Private Const MaksMozaika As Long = 64L * 1024 * 1024

    'kafle pobierane w tej chwili - sąsiednie segmenty, pobierane równolegle, czekają na ten sam kafel zamiast pobierać go ponownie
    Private ReadOnly _wTrakcie As New ConcurrentDictionary(Of String, Lazy(Of Task(Of Tuple(Of Byte(), String))))

    Private ReadOnly _limitCzasu As Integer
    Private ReadOnly _jakoscJpeg As Integer

    Private Sub New(folderKafli As String, limitCzasuSekundy As Integer, jakoscJpeg As Integer)
        Me.FolderKafli = folderKafli
        _limitCzasu = limitCzasuSekundy
        _jakoscJpeg = jakoscJpeg
    End Sub

    ''' <summary>Odczyt dokumentu GetCapabilities i wybór poziomu kafli dla każdej warstwy.</summary>
    Public Shared Async Function PrzygotujAsync(z As ZadaniePobierania, token As CancellationToken) As Task(Of ZrodloWmts)
        Dim adres As String = Wmts.AdresCapabilities(z.AdresSerwera)
        Dim xml As String = ""
        For proba = 1 To Math.Max(1, z.IloscProb)
            Try
                Using limit = PobieranieSegmentow.LimitCzasu(z.LimitCzasuSekundy, token),
                      odpowiedz = Await PobieranieSegmentow.Klient.GetAsync(adres, limit.Token).ConfigureAwait(False)
                    odpowiedz.EnsureSuccessStatusCode()
                    xml = Await odpowiedz.Content.ReadAsStringAsync().ConfigureAwait(False)
                End Using
                Exit For
            Catch ex As OperationCanceledException When token.IsCancellationRequested
                Throw
            Catch ex As OperationCanceledException When proba >= z.IloscProb
                Throw New InvalidOperationException("nie udało się pobrać opisu usługi WMTS (" & adres & "): " &
                                                    PobieranieSegmentow.OpisLimituCzasu(z.LimitCzasuSekundy), ex)
            Catch ex As Exception When proba < z.IloscProb
            Catch ex As Exception
                Throw New InvalidOperationException("nie udało się pobrać opisu usługi WMTS (" & adres & "): " & ex.Message, ex)
            End Try
            Await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, z.PrzerwaSekundy)), token).ConfigureAwait(False)
        Next

        Dim usluga As UslugaWmts
        Try
            usluga = UslugaWmts.Wczytaj(xml, adres)
        Catch ex As Exception
            Throw New InvalidOperationException("niepoprawny opis usługi WMTS (" & adres & "): " & ex.Message, ex)
        End Try
        Dim zrodlo As New ZrodloWmts(z.Folder & "_kafle_wmts\", z.LimitCzasuSekundy, Math.Max(1, Math.Min(100, z.JakoscJpegWmts)))
        For Each w In z.Warstwy
            zrodlo.Plany.Add(New PlanWmts(usluga, w, z.Uklad, z.Siatka.ZasiegSiatki, z.Siatka.RozmiarPiksela, z.Format))
        Next
        If zrodlo.Plany.Count = 0 Then Throw New InvalidOperationException("nie wybrano warstwy WMTS")
        Directory.CreateDirectory(zrodlo.FolderKafli)
        Return zrodlo
    End Function

    ''' <summary>Opis wybranych poziomów kafli.</summary>
    Public ReadOnly Property Opis As String
        Get
            Return String.Join("; ", Plany.ConvertAll(Function(p) p.Opis))
        End Get
    End Property

    ''' <summary>Adres kafla w lewym górnym narożniku segmentu (do wykazów koordynaty.txt i error.txt).</summary>
    Public Function AdresSegmentu(zasieg As Zasieg) As String
        Dim p = Plany(0)
        Dim kol As Double, wie As Double
        p.PikselMacierzy(p.DoUkladuKafli(New PunktXY(zasieg.XGora, zasieg.YLewy)), kol, wie)
        Return p.AdresKafla(CInt(Math.Floor(wie / p.Macierz.WysokoscKafla)), CInt(Math.Floor(kol / p.Macierz.SzerokoscKafla)))
    End Function

    ''' <summary>Usuwa folder pobranych kafli.</summary>
    Public Sub UsunKafle()
        Try
            If Directory.Exists(FolderKafli) Then Directory.Delete(FolderKafli, True)
        Catch
            'folder może być chwilowo zablokowany (np. przez program antywirusowy) - pozostaje do usunięcia ręcznie
        End Try
    End Sub

    ''' <summary>Mozaika kafli jednej warstwy obejmująca segment.</summary>
    Private Class Mozaika
        Public Plan As PlanWmts
        Public Przeliczenia As SiatkaPrzeliczen
        Public Kolumna0 As Integer
        Public Wiersz0 As Integer
        Public Kolumna1 As Integer
        Public Wiersz1 As Integer
        Public Szerokosc As Integer
        Public Wysokosc As Integer
        ''' <summary>Piksele ARGB (wierszami); Nothing - segment poza zasięgiem kafli.</summary>
        Public Piksele() As Integer
    End Class

    ''' <summary>
    ''' Składa segment z kafli i zapisuje w pliku. Zwraca pusty tekst, gdy się udało, albo opis błędu.
    ''' </summary>
    Public Async Function PobierzSegmentAsync(zasieg As Zasieg, bok As Integer, format As String, plikDocelowy As String,
                                              token As CancellationToken) As Task(Of String)
        Try
            Dim mozaiki As New List(Of Mozaika)
            For Each plan In Plany
                Dim m = PrzygotujMozaike(plan, zasieg, bok)
                Dim blad As String = Await WypelnijMozaikeAsync(m, token).ConfigureAwait(False)
                If blad <> "" Then Return blad
                mozaiki.Add(m)
            Next
            token.ThrowIfCancellationRequested()

            Dim dane() As Byte = Await Task.Run(Function() Koduj(Renderuj(mozaiki, bok), format, _jakoscJpeg), token).ConfigureAwait(False)
            Dim tymczasowy As String = plikDocelowy & ".tmp"
            File.WriteAllBytes(tymczasowy, dane)
            If File.Exists(plikDocelowy) Then File.Delete(plikDocelowy)
            File.Move(tymczasowy, plikDocelowy)
            Return ""
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return opis.Replace(vbCr, " ").Replace(vbLf, " ")
        End Try
    End Function

    ''' <summary>Zakres kafli potrzebnych dla segmentu (z marginesem piksela na interpolację).</summary>
    Private Shared Function PrzygotujMozaike(plan As PlanWmts, zasieg As Zasieg, bok As Integer) As Mozaika
        Dim px As Double = (zasieg.YPrawy - zasieg.YLewy) / bok
        Dim py As Double = (zasieg.XGora - zasieg.XDol) / bok
        'siatka przeliczeń dla narożników pikseli segmentu: (0, 0) - lewy górny narożnik segmentu
        Dim siatka As New SiatkaPrzeliczen(bok, bok, 16,
            Function(x As Double, y As Double)
                Dim p = plan.DoUkladuKafli(New PunktXY(zasieg.XGora - y * py, zasieg.YLewy + x * px))
                Dim kol As Double, wie As Double
                plan.PikselMacierzy(p, kol, wie)
                Return New PunktXY(wie, kol)
            End Function)

        'skrajne położenia w macierzy - wzdłuż krawędzi segmentu
        Dim minK As Double = Double.MaxValue, maksK As Double = Double.MinValue
        Dim minW As Double = Double.MaxValue, maksW As Double = Double.MinValue
        Dim krawedz = Sub(x As Double, y As Double)
                          Dim p = siatka.Punkt(x, y)
                          minW = Math.Min(minW, p.X) : maksW = Math.Max(maksW, p.X)
                          minK = Math.Min(minK, p.Y) : maksK = Math.Max(maksK, p.Y)
                      End Sub
        For i = 0 To bok Step 16
            krawedz(i, 0) : krawedz(i, bok) : krawedz(0, i) : krawedz(bok, i)
        Next
        krawedz(bok, bok)

        Dim mc = plan.Macierz
        Dim m As New Mozaika With {.Plan = plan, .Przeliczenia = siatka}
        m.Kolumna0 = CInt(Math.Max(0, Math.Floor((minK - 2) / mc.SzerokoscKafla)))
        m.Kolumna1 = CInt(Math.Min(mc.LiczbaKolumn - 1, Math.Floor((maksK + 2) / mc.SzerokoscKafla)))
        m.Wiersz0 = CInt(Math.Max(0, Math.Floor((minW - 2) / mc.WysokoscKafla)))
        m.Wiersz1 = CInt(Math.Min(mc.LiczbaWierszy - 1, Math.Floor((maksW + 2) / mc.WysokoscKafla)))
        If m.Kolumna1 >= m.Kolumna0 AndAlso m.Wiersz1 >= m.Wiersz0 Then
            m.Szerokosc = (m.Kolumna1 - m.Kolumna0 + 1) * mc.SzerokoscKafla
            m.Wysokosc = (m.Wiersz1 - m.Wiersz0 + 1) * mc.WysokoscKafla
            If CLng(m.Szerokosc) * m.Wysokosc > MaksMozaika Then
                Throw New InvalidOperationException("segment wymaga zbyt wielu kafli WMTS - zmniejsz bok segmentu")
            End If
        End If
        Return m
    End Function

    ''' <summary>Pobiera kafle mozaiki (lub odczytuje je z folderu kafli) i składa je w jeden obraz.</summary>
    Private Async Function WypelnijMozaikeAsync(m As Mozaika, token As CancellationToken) As Task(Of String)
        If m.Szerokosc = 0 Then Return ""
        Dim mc = m.Plan.Macierz
        Dim kafle As New Dictionary(Of Long, Byte())
        Dim bledy As New List(Of String)
        Using semafor As New SemaphoreSlim(WatkiKafli)
            Dim zadania As New List(Of Task)
            For w = m.Wiersz0 To m.Wiersz1
                For k = m.Kolumna0 To m.Kolumna1
                    If Not m.Plan.KafelDostepny(w, k) Then Continue For
                    Dim wiersz As Integer = w, kolumna As Integer = k
                    zadania.Add(Task.Run(
                        Async Function()
                            Await semafor.WaitAsync(token).ConfigureAwait(False)
                            Try
                                Dim adres As String = m.Plan.AdresKafla(wiersz, kolumna)
                                Dim wynik = Await PobierzKafelAsync(adres, token).ConfigureAwait(False)
                                SyncLock kafle
                                    If wynik.Item2 <> "" Then
                                        bledy.Add("kafel " & adres & ": " & wynik.Item2)
                                    Else
                                        kafle(CLng(wiersz) * 100000000L + kolumna) = wynik.Item1
                                    End If
                                End SyncLock
                            Finally
                                semafor.Release()
                            End Try
                        End Function, token))
                Next
            Next
            Await Task.WhenAll(zadania).ConfigureAwait(False)
        End Using
        If bledy.Count > 0 Then Return bledy(0) & If(bledy.Count > 1, " (oraz " & (bledy.Count - 1) & " innych kafli)", "")

        Using obraz As New Bitmap(m.Szerokosc, m.Wysokosc, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(obraz)
                g.Clear(Color.Transparent)
                g.CompositingMode = CompositingMode.SourceCopy
                g.PixelOffsetMode = PixelOffsetMode.Half
                For Each para In kafle
                    If para.Value Is Nothing OrElse para.Value.Length = 0 Then Continue For
                    Dim w As Integer = CInt(para.Key \ 100000000L), k As Integer = CInt(para.Key Mod 100000000L)
                    Using ms As New MemoryStream(para.Value)
                        Using kafel As Image = Image.FromStream(ms)
                            g.InterpolationMode = If(kafel.Width = mc.SzerokoscKafla AndAlso kafel.Height = mc.WysokoscKafla,
                                                     InterpolationMode.NearestNeighbor, InterpolationMode.HighQualityBicubic)
                            g.DrawImage(kafel, New Rectangle((k - m.Kolumna0) * mc.SzerokoscKafla, (w - m.Wiersz0) * mc.WysokoscKafla,
                                                             mc.SzerokoscKafla, mc.WysokoscKafla), 0, 0, kafel.Width, kafel.Height, GraphicsUnit.Pixel)
                        End Using
                    End Using
                Next
            End Using
            ReDim m.Piksele(m.Szerokosc * m.Wysokosc - 1)
            Dim d = obraz.LockBits(New Rectangle(0, 0, m.Szerokosc, m.Wysokosc), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            Try
                For y = 0 To m.Wysokosc - 1
                    Marshal.Copy(IntPtr.Add(d.Scan0, y * d.Stride), m.Piksele, y * m.Szerokosc, m.Szerokosc)
                Next
            Finally
                obraz.UnlockBits(d)
            End Try
        End Using
        Return ""
    End Function

    ''' <summary>
    ''' Kafel z folderu kafli albo z serwera. Wynik: (dane, błąd); dane puste - kafla nie ma
    ''' (serwery WMTS zgłaszają w ten sposób kafle poza obszarem danych: HTTP 404 lub 204).
    ''' </summary>
    Private Async Function PobierzKafelAsync(adres As String, token As CancellationToken) As Task(Of Tuple(Of Byte(), String))
        Dim plik As String = Path.Combine(FolderKafli, NazwaKafla(adres))
        If File.Exists(plik) Then Return Tuple.Create(File.ReadAllBytes(plik), "")
        Dim zadanie = _wTrakcie.GetOrAdd(adres, Function(a) New Lazy(Of Task(Of Tuple(Of Byte(), String)))(Function() PobierzKafelZSerweraAsync(a, plik, _limitCzasu)))
        Try
            Return Await zadanie.Value.WaitAsync(token).ConfigureAwait(False)
        Finally
            'kafel pobrany jest już w folderze kafli; nieudany - przy kolejnej próbie pobierany od nowa
            Dim usuniete As Lazy(Of Task(Of Tuple(Of Byte(), String))) = Nothing
            _wTrakcie.TryRemove(adres, usuniete)
        End Try
    End Function

    Private Shared Async Function PobierzKafelZSerweraAsync(adres As String, plik As String, limitCzasuSekundy As Integer) As Task(Of Tuple(Of Byte(), String))
        Try
            Using limit = PobieranieSegmentow.LimitCzasu(limitCzasuSekundy, CancellationToken.None),
                  odpowiedz = Await PobieranieSegmentow.Klient.GetAsync(adres, HttpCompletionOption.ResponseContentRead, limit.Token).ConfigureAwait(False)
                Dim dane() As Byte = Await odpowiedz.Content.ReadAsByteArrayAsync(limit.Token).ConfigureAwait(False)
                If odpowiedz.StatusCode = HttpStatusCode.NotFound OrElse odpowiedz.StatusCode = HttpStatusCode.NoContent Then
                    dane = New Byte() {}
                ElseIf Not PobieranieSegmentow.CzyObraz(dane) Then
                    Dim tekst As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 300)).Replace(vbCr, " ").Replace(vbLf, " ")
                    If Not odpowiedz.IsSuccessStatusCode Then Return Tuple.Create(CType(Nothing, Byte()), "HTTP " & CInt(odpowiedz.StatusCode) & " " & odpowiedz.ReasonPhrase & ": " & tekst)
                    Return Tuple.Create(CType(Nothing, Byte()), "serwer nie zwrócił obrazka: " & tekst)
                End If
                'zapis przez plik tymczasowy o unikalnej nazwie - ten sam kafel mogą pobierać jednocześnie dwa sąsiednie segmenty
                Dim tymczasowy As String = plik & "." & Guid.NewGuid().ToString("N") & ".tmp"
                File.WriteAllBytes(tymczasowy, dane)
                Try
                    If Not File.Exists(plik) Then File.Move(tymczasowy, plik)
                Catch ex As IOException
                    'kafel zapisany w międzyczasie przez inny segment
                End Try
                If File.Exists(tymczasowy) Then File.Delete(tymczasowy)
                Return Tuple.Create(dane, "")
            End Using
        Catch ex As OperationCanceledException
            Return Tuple.Create(CType(Nothing, Byte()), PobieranieSegmentow.OpisLimituCzasu(limitCzasuSekundy))
        Catch ex As Exception
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return Tuple.Create(CType(Nothing, Byte()), opis.Replace(vbCr, " ").Replace(vbLf, " "))
        End Try
    End Function

    ''' <summary>Nazwa pliku kafla - skrót adresu (adres jednoznacznie określa warstwę, poziom i położenie kafla).</summary>
    Private Shared Function NazwaKafla(adres As String) As String
        Using sha As SHA1 = SHA1.Create()
            Dim skrot = sha.ComputeHash(Text.Encoding.UTF8.GetBytes(adres))
            Return BitConverter.ToString(skrot).Replace("-", "").ToLowerInvariant() & ".kafel"
        End Using
    End Function

    ''' <summary>
    ''' Piksele segmentu: interpolacja dwuliniowa kafli (z nadpróbkowaniem, gdy kafle są dokładniejsze od segmentu),
    ''' warstwy nakładane kolejno z uwzględnieniem przezroczystości. Wynik - bitmapa 32-bitowa.
    ''' </summary>
    Private Shared Function Renderuj(mozaiki As List(Of Mozaika), bok As Integer) As Bitmap
        Dim wynik(bok * bok - 1) As Integer
        For py = 0 To bok - 1
            For px = 0 To bok - 1
                'kolor z premnożoną przezroczystością (0-1)
                Dim r As Double = 0, g As Double = 0, b As Double = 0, a As Double = 0
                For Each m In mozaiki
                    If m.Piksele Is Nothing Then Continue For
                    Dim n As Integer = m.Plan.Nadprobkowanie
                    Dim sr As Double = 0, sg As Double = 0, sb As Double = 0, sa As Double = 0
                    For j = 0 To n - 1
                        For i = 0 To n - 1
                            Dim p = m.Przeliczenia.Punkt(px + (i + 0.5) / n, py + (j + 0.5) / n)
                            Probka(m, p.Y, p.X, sr, sg, sb, sa)
                        Next
                    Next
                    Dim nn As Double = n * n
                    sr /= nn : sg /= nn : sb /= nn : sa /= nn
                    r = sr + r * (1 - sa) : g = sg + g * (1 - sa) : b = sb + b * (1 - sa) : a = sa + a * (1 - sa)
                Next
                If a > 0.000001 Then
                    Dim alfa As Integer = CInt(Math.Round(a * 255))
                    wynik(py * bok + px) = (alfa << 24) Or (Bajt(r / a) << 16) Or (Bajt(g / a) << 8) Or Bajt(b / a)
                End If
            Next
        Next
        Dim bmp As New Bitmap(bok, bok, PixelFormat.Format32bppArgb)
        Dim d = bmp.LockBits(New Rectangle(0, 0, bok, bok), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To bok - 1
                Marshal.Copy(wynik, y * bok, IntPtr.Add(d.Scan0, y * d.Stride), bok)
            Next
        Finally
            bmp.UnlockBits(d)
        End Try
        Return bmp
    End Function

    Private Shared Function Bajt(v As Double) As Integer
        Return Math.Max(0, Math.Min(255, CInt(Math.Round(v * 255))))
    End Function

    ''' <summary>
    ''' Dodaje próbkę mozaiki w punkcie (kolumna, wiersz) całej macierzy (narożniki pikseli w liczbach całkowitych):
    ''' interpolacja dwuliniowa kolorów premnożonych przez przezroczystość. Poza mozaiką - próbka przezroczysta.
    ''' </summary>
    Private Shared Sub Probka(m As Mozaika, kolumna As Double, wiersz As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double, ByRef a As Double)
        Dim u As Double = kolumna - m.Kolumna0 * m.Plan.Macierz.SzerokoscKafla - 0.5
        Dim v As Double = wiersz - m.Wiersz0 * m.Plan.Macierz.WysokoscKafla - 0.5
        If u < -0.5 OrElse v < -0.5 OrElse u > m.Szerokosc - 0.5 OrElse v > m.Wysokosc - 0.5 Then Exit Sub
        Dim k0 As Integer = CInt(Math.Floor(u)), w0 As Integer = CInt(Math.Floor(v))
        Dim tx As Double = u - k0, ty As Double = v - w0
        Dim k1 As Integer = Math.Min(k0 + 1, m.Szerokosc - 1), w1 As Integer = Math.Min(w0 + 1, m.Wysokosc - 1)
        k0 = Math.Max(0, k0) : w0 = Math.Max(0, w0)
        Dodaj(m.Piksele(w0 * m.Szerokosc + k0), (1 - tx) * (1 - ty), r, g, b, a)
        Dodaj(m.Piksele(w0 * m.Szerokosc + k1), tx * (1 - ty), r, g, b, a)
        Dodaj(m.Piksele(w1 * m.Szerokosc + k0), (1 - tx) * ty, r, g, b, a)
        Dodaj(m.Piksele(w1 * m.Szerokosc + k1), tx * ty, r, g, b, a)
    End Sub

    Private Shared Sub Dodaj(argb As Integer, waga As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double, ByRef a As Double)
        Dim alfa As Double = ((argb >> 24) And &HFF) / 255.0 * waga
        If alfa = 0 Then Exit Sub
        r += ((argb >> 16) And &HFF) / 255.0 * alfa
        g += ((argb >> 8) And &HFF) / 255.0 * alfa
        b += (argb And &HFF) / 255.0 * alfa
        a += alfa
    End Sub

    ''' <summary>Zapis segmentu w formacie wybranym w ustawieniach (JPEG na białym tle, PNG z przezroczystością).</summary>
    Private Shared Function Koduj(bmp As Bitmap, format As String, jakoscJpeg As Integer) As Byte()
        Using bmp
            Select Case Wms.RozszerzeniePliku(format)
                Case "png"
                    Return EksportMapy.Koduj(bmp, True, 0)
                Case "tif", "gif"
                    Using ms As New MemoryStream()
                        bmp.Save(ms, If(Wms.RozszerzeniePliku(format) = "tif", ImageFormat.Tiff, ImageFormat.Gif))
                        Return ms.ToArray()
                    End Using
                Case Else
                    Return EksportMapy.Koduj(bmp, False, jakoscJpeg)
            End Select
        End Using
    End Function

End Class
