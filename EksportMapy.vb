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

Imports System.Data
Imports System.Data.SQLite
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MapoTero.Core

''' <summary>
''' Odczyt pikseli z zestawu segmentów (z pamięcią podręczną ostatnio używanych segmentów) - źródło obrazu
''' dla eksportu do formatów z inną siatką kafli (KMZ w WGS84, MBTiles w Web Mercator).
''' </summary>
Public Class ZrodloSegmentow

    Private ReadOnly _folder As String
    Private ReadOnly _prefiks As String
    Private ReadOnly _styl As StylNumeracji
    Private ReadOnly _rozszerzenie As String
    Private ReadOnly _siatka As Siatka
    Private ReadOnly _bok As Integer
    Private ReadOnly _pikselX As Double
    Private ReadOnly _pikselY As Double
    Private ReadOnly _zasieg As Zasieg

    'pamięć podręczna zdekodowanych segmentów (RGB, wierszami): klucz = wiersz * 100000 + kolumna; Nothing - brak pliku
    Private ReadOnly _pamiec As New Dictionary(Of Integer, LinkedListNode(Of KeyValuePair(Of Integer, Byte())))
    Private ReadOnly _kolejnosc As New LinkedList(Of KeyValuePair(Of Integer, Byte()))
    Private ReadOnly _maksSegmentow As Integer
    Private _ostatniKlucz As Integer = -1
    Private _ostatnieDane() As Byte

    Public Sub New(folder As String, prefiks As String, styl As StylNumeracji, rozszerzenie As String, siatka As Siatka)
        _folder = folder
        _prefiks = prefiks
        _styl = styl
        _rozszerzenie = rozszerzenie
        _siatka = siatka
        _bok = siatka.BokSegmentuPx
        _zasieg = siatka.ZasiegSiatki
        _pikselX = siatka.RozmiarPiksela
        _pikselY = siatka.RozmiarPiksela
        'ok. 256 MB na zdekodowane segmenty
        _maksSegmentow = CInt(Math.Max(4, Math.Min(512, 256L * 1024 * 1024 \ (CLng(_bok) * _bok * 3))))
    End Sub

    Public ReadOnly Property Siatka As Siatka
        Get
            Return _siatka
        End Get
    End Property

    ''' <summary>
    ''' Kolor w punkcie (X, Y) układu segmentów - interpolacja dwuliniowa. Zwraca False poza obrazem
    ''' lub w miejscu brakującego segmentu.
    ''' </summary>
    Public Function Probka(x As Double, y As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double) As Boolean
        'współrzędne piksela (środki pikseli w liczbach całkowitych)
        Dim kol As Double = (y - _zasieg.YLewy) / _pikselY - 0.5
        Dim wie As Double = (_zasieg.XGora - x) / _pikselX - 0.5
        Dim szer As Long = _siatka.SzerokoscPx, wys As Long = _siatka.WysokoscPx
        If kol < -0.5 OrElse wie < -0.5 OrElse kol > szer - 0.5 OrElse wie > wys - 0.5 Then Return False

        Dim k0 As Long = CLng(Math.Floor(kol)), w0 As Long = CLng(Math.Floor(wie))
        Dim tx As Double = kol - k0, ty As Double = wie - w0
        Dim k1 As Long = Math.Min(k0 + 1, szer - 1), w1 As Long = Math.Min(w0 + 1, wys - 1)
        k0 = Math.Max(0, k0) : w0 = Math.Max(0, w0)

        Dim r00, g00, b00, r10, g10, b10, r01, g01, b01, r11, g11, b11 As Integer
        If Not Piksel(k0, w0, r00, g00, b00) Then Return False
        If Not Piksel(k1, w0, r10, g10, b10) Then r10 = r00 : g10 = g00 : b10 = b00
        If Not Piksel(k0, w1, r01, g01, b01) Then r01 = r00 : g01 = g00 : b01 = b00
        If Not Piksel(k1, w1, r11, g11, b11) Then r11 = r10 : g11 = g10 : b11 = b10
        r = (r00 * (1 - tx) + r10 * tx) * (1 - ty) + (r01 * (1 - tx) + r11 * tx) * ty
        g = (g00 * (1 - tx) + g10 * tx) * (1 - ty) + (g01 * (1 - tx) + g11 * tx) * ty
        b = (b00 * (1 - tx) + b10 * tx) * (1 - ty) + (b01 * (1 - tx) + b11 * tx) * ty
        Return True
    End Function

    ''' <summary>Piksel całego obrazu (kolumna, wiersz od lewego górnego narożnika).</summary>
    Private Function Piksel(kol As Long, wie As Long, ByRef r As Integer, ByRef g As Integer, ByRef b As Integer) As Boolean
        Dim sk As Integer = CInt(kol \ _bok) + 1, sw As Integer = CInt(wie \ _bok) + 1
        Dim dane = Segment(sw, sk)
        If dane Is Nothing Then Return False
        Dim i As Integer = (CInt(wie Mod _bok) * _bok + CInt(kol Mod _bok)) * 3
        r = dane(i) : g = dane(i + 1) : b = dane(i + 2)
        Return True
    End Function

    ''' <summary>Zdekodowany segment (RGB) albo Nothing, gdy brak pliku.</summary>
    Private Function Segment(wiersz As Integer, kolumna As Integer) As Byte()
        Dim klucz As Integer = wiersz * 100000 + kolumna
        If klucz = _ostatniKlucz Then Return _ostatnieDane
        Dim wezel As LinkedListNode(Of KeyValuePair(Of Integer, Byte())) = Nothing
        Dim dane() As Byte
        If _pamiec.TryGetValue(klucz, wezel) Then
            _kolejnosc.Remove(wezel)
            _kolejnosc.AddFirst(wezel)
            dane = wezel.Value.Value
        Else
            dane = Wczytaj(wiersz, kolumna)
            wezel = _kolejnosc.AddFirst(New KeyValuePair(Of Integer, Byte())(klucz, dane))
            _pamiec(klucz) = wezel
            While _kolejnosc.Count > _maksSegmentow
                _pamiec.Remove(_kolejnosc.Last.Value.Key)
                _kolejnosc.RemoveLast()
            End While
        End If
        _ostatniKlucz = klucz
        _ostatnieDane = dane
        Return dane
    End Function

    Private Function Wczytaj(wiersz As Integer, kolumna As Integer) As Byte()
        Dim sciezka As String = _folder & _siatka.NazwaSegmentu(_prefiks, _styl, wiersz, kolumna) & "." & _rozszerzenie
        If Not File.Exists(sciezka) Then Return Nothing
        Try
            Using zrodlo As Image = Image.FromFile(sciezka)
                Using rgb As New Bitmap(_bok, _bok, PixelFormat.Format24bppRgb)
                    Using g As Graphics = Graphics.FromImage(rgb)
                        g.Clear(Color.White)
                        g.InterpolationMode = If(zrodlo.Width = _bok AndAlso zrodlo.Height = _bok, InterpolationMode.NearestNeighbor, InterpolationMode.HighQualityBicubic)
                        g.PixelOffsetMode = PixelOffsetMode.Half
                        g.DrawImage(zrodlo, New Rectangle(0, 0, _bok, _bok), 0, 0, zrodlo.Width, zrodlo.Height, GraphicsUnit.Pixel)
                    End Using
                    Dim wynik(_bok * _bok * 3 - 1) As Byte
                    Dim linia(_bok * 3 - 1) As Byte
                    Dim d As BitmapData = rgb.LockBits(New Rectangle(0, 0, _bok, _bok), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
                    Try
                        For y = 0 To _bok - 1
                            Marshal.Copy(IntPtr.Add(d.Scan0, y * d.Stride), linia, 0, _bok * 3)
                            Dim cel As Integer = y * _bok * 3
                            For i = 0 To _bok - 1
                                wynik(cel + i * 3) = linia(i * 3 + 2)
                                wynik(cel + i * 3 + 1) = linia(i * 3 + 1)
                                wynik(cel + i * 3 + 2) = linia(i * 3)
                            Next
                        Next
                    Finally
                        rgb.UnlockBits(d)
                    End Try
                    Return wynik
                End Using
            End Using
        Catch
            Return Nothing
        End Try
    End Function

End Class

''' <summary>Rodzaj eksportu.</summary>
Public Enum RodzajEksportu
    ''' <summary>KMZ dla odbiorników Garmin (Custom Maps): kafle JPEG do 1024 px, limit liczby kafli.</summary>
    KmzGarmin
    ''' <summary>KMZ bez limitu liczby kafli (Locus Map, OruxMaps, Google Earth).</summary>
    Kmz
    ''' <summary>MBTiles - kafle Web Mercator (Locus Map, OsmAnd, OruxMaps i inne).</summary>
    MBTiles
End Enum

''' <summary>Parametry eksportu.</summary>
Public Class ZadanieEksportu
    Public Property Rodzaj As RodzajEksportu = RodzajEksportu.KmzGarmin
    Public Property Zrodlo As ZrodloSegmentow
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Public Property Plik As String = ""
    Public Property Nazwa As String = ""
    Public Property JakoscJpeg As Integer = 85
    ''' <summary>KMZ: maksymalna liczba kafli (Garmin: 100).</summary>
    Public Property MaksKafli As Integer = 100
    ''' <summary>MBTiles: zakres poziomów powiększenia.</summary>
    Public Property PoziomMin As Integer = 10
    Public Property PoziomMax As Integer = 16
    ''' <summary>MBTiles: kafle PNG (przezroczystość poza obszarem mapy) zamiast JPEG.</summary>
    Public Property KaflePng As Boolean
End Class

''' <summary>Eksport zestawu segmentów do KMZ (WGS84) i MBTiles (Web Mercator).</summary>
Public NotInheritable Class EksportMapy

    Private Sub New()
    End Sub

    ''' <summary>Rozmiar piksela źródła w metrach.</summary>
    Public Shared Function PikselWMetrach(z As ZadanieEksportu) As Double
        Dim s = z.Zrodlo.Siatka
        If Not z.Uklad.Geograficzny Then Return s.RozmiarPiksela
        Dim mSz As Double, mDl As Double
        Georeferencja.MetrowNaStopien(s.ZasiegSiatki.Srodek.X, mSz, mDl)
        Return s.RozmiarPiksela * mSz
    End Function

    ''' <summary>Plan kafli KMZ dla zadania.</summary>
    Public Shared Function PlanKmz(z As ZadanieEksportu) As PlanKmz
        Dim obszar = ProstokatGeo.Wpisany(z.Uklad, z.Zrodlo.Siatka.ZasiegSiatki)
        Return New PlanKmz(obszar, PikselWMetrach(z), 1024, If(z.Rodzaj = RodzajEksportu.KmzGarmin, z.MaksKafli, 0))
    End Function

    ''' <summary>Liczba kafli MBTiles we wszystkich poziomach.</summary>
    Public Shared Function LiczbaKafliMBTiles(z As ZadanieEksportu) As Long
        Dim obszar = ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.Siatka.ZasiegSiatki)
        Dim suma As Long = 0
        For poziom = z.PoziomMin To z.PoziomMax
            suma += WebMercator.LiczbaKafli(obszar, poziom)
        Next
        Return suma
    End Function

    Public Shared Sub Eksportuj(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Try
            If z.Rodzaj = RodzajEksportu.MBTiles Then
                EksportMBTiles(z, postep, token)
            Else
                EksportKmz(z, postep, token)
            End If
        Catch
            'niedokończony plik nie może pozostać na dysku
            Try
                SQLiteConnection.ClearAllPools()
                If File.Exists(z.Plik) Then File.Delete(z.Plik)
            Catch
            End Try
            Throw
        End Try
    End Sub

#Region "Renderowanie"

    ''' <summary>
    ''' Renderuje kafel: dla każdego piksela (px, py) funkcja przelicz zwraca punkt w układzie segmentów.
    ''' Gdy kafel ma mniejszą rozdzielczość niż źródło - nadpróbkowanie (do 4 x 4 próbek na piksel).
    ''' Zwraca bitmapę 32-bitową (piksele poza obrazem przezroczyste) albo Nothing, gdy kafel jest pusty.
    ''' </summary>
    Private Shared Function RenderujKafel(zrodlo As ZrodloSegmentow, szer As Integer, wys As Integer, nadprobkowanie As Integer,
                                          przelicz As Func(Of Double, Double, PunktXY)) As Bitmap
        Dim siatka As New SiatkaPrzeliczen(szer, wys, 16, przelicz)
        Dim n As Integer = Math.Max(1, Math.Min(4, nadprobkowanie))
        Dim dane(szer * wys * 4 - 1) As Byte
        Dim pusty As Boolean = True
        For py = 0 To wys - 1
            For px = 0 To szer - 1
                Dim sr As Double = 0, sg As Double = 0, sb As Double = 0
                Dim trafienia As Integer = 0
                For j = 0 To n - 1
                    For i = 0 To n - 1
                        'siatka przeliczeń liczona jest dla lewego górnego narożnika piksela (0,0); próbki rozłożone w pikselu
                        Dim p = siatka.Punkt(px + (i + 0.5) / n, py + (j + 0.5) / n)
                        Dim r As Double, g As Double, b As Double
                        If zrodlo.Probka(p.X, p.Y, r, g, b) Then
                            sr += r : sg += g : sb += b : trafienia += 1
                        End If
                    Next
                Next
                If trafienia > 0 Then
                    Dim o As Integer = (py * szer + px) * 4
                    dane(o) = CByte(Math.Round(sb / trafienia))
                    dane(o + 1) = CByte(Math.Round(sg / trafienia))
                    dane(o + 2) = CByte(Math.Round(sr / trafienia))
                    dane(o + 3) = CByte(Math.Round(255.0 * trafienia / (n * n)))
                    pusty = False
                End If
            Next
        Next
        If pusty Then Return Nothing
        Dim bmp As New Bitmap(szer, wys, PixelFormat.Format32bppArgb)
        Dim d = bmp.LockBits(New Rectangle(0, 0, szer, wys), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wys - 1
                Marshal.Copy(dane, y * szer * 4, IntPtr.Add(d.Scan0, y * d.Stride), szer * 4)
            Next
        Finally
            bmp.UnlockBits(d)
        End Try
        Return bmp
    End Function

    ''' <summary>Koduje bitmapę: JPEG (na białym tle) albo PNG (z przezroczystością).</summary>
    Friend Shared Function Koduj(bmp As Bitmap, png As Boolean, jakosc As Integer) As Byte()
        Using ms As New MemoryStream()
            If png Then
                bmp.Save(ms, ImageFormat.Png)
            Else
                Using tlo As New Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb)
                    Using g = Graphics.FromImage(tlo)
                        g.Clear(Color.White)
                        g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height)
                    End Using
                    Dim koder = ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)
                    Using parametry As New EncoderParameters(1)
                        parametry.Param(0) = New EncoderParameter(Imaging.Encoder.Quality, CLng(Math.Max(1, Math.Min(100, jakosc))))
                        tlo.Save(ms, koder, parametry)
                    End Using
                End Using
            End If
            Return ms.ToArray()
        End Using
    End Function

#End Region

#Region "KMZ"

    Private Shared Sub EksportKmz(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Dim plan = PlanKmz(z)
        Dim nadprobkowanie As Integer = CInt(Math.Ceiling(plan.Pomniejszenie - 0.01))
        Dim gotowe As Integer = 0
        Using kmz As New ZapisKmz(z.Plik, z.Nazwa, 50)
            For w = 0 To plan.Wiersze - 1
                For k = 0 To plan.Kolumny - 1
                    token.ThrowIfCancellationRequested()
                    Dim px = plan.PikseleKafla(w, k)
                    Dim x0 As Integer = px.Item1, y0 As Integer = px.Item2
                    Dim f = Function(x As Double, y As Double) z.Uklad.ZWgs84(plan.Obszar.Polnoc - (y0 + y) * plan.PikselSzerokosc,
                                                                            plan.Obszar.Zachod + (x0 + x) * plan.PikselDlugosc)
                    Using bmp = RenderujKafel(z.Zrodlo, px.Item3, px.Item4, nadprobkowanie, f)
                        If bmp IsNot Nothing Then
                            kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & ".jpg",
                                           Koduj(bmp, False, z.JakoscJpeg), plan.ZasiegKafla(w, k))
                        End If
                    End Using
                    gotowe += 1
                    postep?.Report(gotowe * 100 \ plan.LiczbaKafli)
                Next
            Next
            kmz.Zakoncz()
        End Using
    End Sub

#End Region

#Region "MBTiles"

    Private Shared Sub EksportMBTiles(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        If File.Exists(z.Plik) Then File.Delete(z.Plik)
        Dim obszar = ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.Siatka.ZasiegSiatki)
        Dim wszystkie As Long = Math.Max(1, LiczbaKafliMBTiles(z))
        Dim gotowe As Long = 0
        Dim pikselZrodla As Double = PikselWMetrach(z)
        Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2

        Using pol As New SQLiteConnection("Data Source=" & z.Plik & ";Version=3;")
            pol.Open()
            Wykonaj(pol, "CREATE TABLE metadata (name text, value text);")
            Wykonaj(pol, "CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob);")
            Wykonaj(pol, "CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);")
            Dim ci = Globalization.CultureInfo.InvariantCulture
            Dim meta As New Dictionary(Of String, String) From {
                {"name", z.Nazwa}, {"type", "overlay"}, {"version", "1.1"}, {"description", "Mapa utworzona programem MapoTero"},
                {"format", If(z.KaflePng, "png", "jpg")}, {"minzoom", z.PoziomMin.ToString(ci)}, {"maxzoom", z.PoziomMax.ToString(ci)},
                {"bounds", String.Format(ci, "{0},{1},{2},{3}", obszar.Zachod, obszar.Poludnie, obszar.Wschod, obszar.Polnoc)},
                {"center", String.Format(ci, "{0},{1},{2}", (obszar.Zachod + obszar.Wschod) / 2, srodekSz, z.PoziomMax)}}
            For Each m In meta
                Using cmd As New SQLiteCommand("INSERT INTO metadata (name, value) VALUES (@n, @v);", pol)
                    cmd.Parameters.AddWithValue("@n", m.Key)
                    cmd.Parameters.AddWithValue("@v", m.Value)
                    cmd.ExecuteNonQuery()
                End Using
            Next

            'najwyższy poziom - kafle renderowane bezpośrednio z segmentów
            Dim zmax As Integer = z.PoziomMax
            Dim nadprobkowanie As Integer = CInt(Math.Ceiling(WebMercator.RozmiarPiksela(srodekSz, zmax) / pikselZrodla - 0.01))
            Dim r = WebMercator.ZakresKafli(obszar, zmax)
            Using tr = pol.BeginTransaction()
                For ty = r.Item2 To r.Item4
                    For tx = r.Item1 To r.Item3
                        token.ThrowIfCancellationRequested()
                        Dim kx As Double = tx * 256.0, ky As Double = ty * 256.0
                        Dim f = Function(x As Double, y As Double) z.Uklad.ZWgs84(WebMercator.Szerokosc(ky + y, zmax), WebMercator.Dlugosc(kx + x, zmax))
                        Using bmp = RenderujKafel(z.Zrodlo, 256, 256, nadprobkowanie, f)
                            If bmp IsNot Nothing Then ZapiszKafel(pol, New KafelXYZ(tx, ty, zmax), Koduj(bmp, z.KaflePng, z.JakoscJpeg))
                        End Using
                        gotowe += 1
                        postep?.Report(CInt(gotowe * 100 \ wszystkie))
                    Next
                Next
                tr.Commit()
            End Using

            'niższe poziomy - z czterech kafli poziomu wyższego (piramida)
            For poziom = zmax - 1 To z.PoziomMin Step -1
                Dim rp = WebMercator.ZakresKafli(obszar, poziom)
                Using tr = pol.BeginTransaction()
                    For ty = rp.Item2 To rp.Item4
                        For tx = rp.Item1 To rp.Item3
                            token.ThrowIfCancellationRequested()
                            Dim dane = KafelZDzieci(pol, New KafelXYZ(tx, ty, poziom), z)
                            If dane IsNot Nothing Then ZapiszKafel(pol, New KafelXYZ(tx, ty, poziom), dane)
                            gotowe += 1
                            postep?.Report(CInt(Math.Min(100, gotowe * 100 \ wszystkie)))
                        Next
                    Next
                    tr.Commit()
                End Using
            Next
        End Using
        SQLiteConnection.ClearAllPools()
    End Sub

    Private Shared Sub Wykonaj(pol As SQLiteConnection, sql As String)
        Using cmd As New SQLiteCommand(sql, pol)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Sub ZapiszKafel(pol As SQLiteConnection, k As KafelXYZ, dane() As Byte)
        Using cmd As New SQLiteCommand("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);", pol)
            cmd.Parameters.AddWithValue("@z", k.Z)
            cmd.Parameters.AddWithValue("@x", k.X)
            cmd.Parameters.AddWithValue("@y", k.WierszTms)
            cmd.Parameters.Add("@d", DbType.Binary).Value = dane
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Function OdczytajKafel(pol As SQLiteConnection, k As KafelXYZ) As Byte()
        Using cmd As New SQLiteCommand("SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;", pol)
            cmd.Parameters.AddWithValue("@z", k.Z)
            cmd.Parameters.AddWithValue("@x", k.X)
            cmd.Parameters.AddWithValue("@y", k.WierszTms)
            Return TryCast(cmd.ExecuteScalar(), Byte())
        End Using
    End Function

    ''' <summary>Kafel poziomu niższego złożony z czterech kafli poziomu wyższego (pomniejszenie 2x).</summary>
    Private Shared Function KafelZDzieci(pol As SQLiteConnection, k As KafelXYZ, z As ZadanieEksportu) As Byte()
        Using duzy As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
            Dim jest As Boolean = False
            Using g = Graphics.FromImage(duzy)
                g.Clear(Color.Transparent)
                For dy = 0 To 1
                    For dx = 0 To 1
                        Dim dane = OdczytajKafel(pol, New KafelXYZ(k.X * 2 + dx, k.Y * 2 + dy, k.Z + 1))
                        If dane Is Nothing Then Continue For
                        Using ms As New MemoryStream(dane), obraz = Image.FromStream(ms)
                            g.DrawImage(obraz, New Rectangle(dx * 256, dy * 256, 256, 256))
                            jest = True
                        End Using
                    Next
                Next
            End Using
            If Not jest Then Return Nothing
            Using maly As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                Using g = Graphics.FromImage(maly)
                    g.Clear(Color.Transparent)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    g.DrawImage(duzy, New Rectangle(0, 0, 256, 256), 0, 0, 512, 512, GraphicsUnit.Pixel)
                End Using
                Return Koduj(maly, z.KaflePng, z.JakoscJpeg)
            End Using
        End Using
    End Function

#End Region

End Class
