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
''' dla eksportu do formatów z inną siatką kafli (KMZ w WGS84, kafle Web Mercator). Piksele mają kanał alfa:
''' przezroczyste tło segmentów PNG / GIF (warstwy pobrane z TRANSPARENT=TRUE) pozostaje przezroczyste.
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

    'pliki segmentów istniejące na dysku (wiersz, kolumna od 0) - brakujące, np. poza korytarzem trasy, są przezroczyste
    Private ReadOnly _istnieje(,) As Boolean
    Private ReadOnly _liczbaIstniejacych As Integer

    'pamięć podręczna zdekodowanych segmentów (BGRA, wierszami): klucz = wiersz * 100000 + kolumna; Nothing - brak pliku
    Private ReadOnly _pamiec As New Dictionary(Of Integer, LinkedListNode(Of KeyValuePair(Of Integer, Byte())))
    Private ReadOnly _kolejnosc As New LinkedList(Of KeyValuePair(Of Integer, Byte()))
    Private ReadOnly _maksSegmentow As Integer
    Private _ostatniKlucz As Integer = -1
    Private _ostatnieDane() As Byte
    Private _bialePrzezroczyste As Boolean

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
        _maksSegmentow = CInt(Math.Max(4, Math.Min(512, 256L * 1024 * 1024 \ (CLng(_bok) * _bok * 4))))

        ReDim _istnieje(Math.Max(0, siatka.LiczbaWierszy - 1), Math.Max(0, siatka.LiczbaKolumn - 1))
        For w = 1 To siatka.LiczbaWierszy
            For k = 1 To siatka.LiczbaKolumn
                If File.Exists(SciezkaSegmentu(w, k)) Then
                    _istnieje(w - 1, k - 1) = True
                    _liczbaIstniejacych += 1
                End If
            Next
        Next
    End Sub

    Public ReadOnly Property Siatka As Siatka
        Get
            Return _siatka
        End Get
    End Property

    ''' <summary>Folder segmentów.</summary>
    Public ReadOnly Property Folder As String
        Get
            Return _folder
        End Get
    End Property

    ''' <summary>Liczba segmentów, których pliki istnieją na dysku.</summary>
    Public ReadOnly Property LiczbaSegmentow As Integer
        Get
            Return _liczbaIstniejacych
        End Get
    End Property

    ''' <summary>
    ''' Białe piksele (np. tło warstw WMS, które nie obsługują TRANSPARENT=TRUE, albo segmentów JPEG) stają się przezroczyste -
    ''' tak jak w narzędziu nakładania map png. Zmiana opróżnia pamięć podręczną segmentów.
    ''' </summary>
    Public Property BialePrzezroczyste As Boolean
        Get
            Return _bialePrzezroczyste
        End Get
        Set(value As Boolean)
            If value = _bialePrzezroczyste Then Exit Property
            _bialePrzezroczyste = value
            _pamiec.Clear()
            _kolejnosc.Clear()
            _ostatniKlucz = -1
            _ostatnieDane = Nothing
        End Set
    End Property

    Private Function SciezkaSegmentu(wiersz As Integer, kolumna As Integer) As String
        Return _folder & _siatka.NazwaSegmentu(_prefiks, _styl, wiersz, kolumna) & "." & _rozszerzenie
    End Function

    ''' <summary>Czy w prostokącie (w układzie segmentów) jest co najmniej jeden istniejący segment.</summary>
    Public Function MaDane(obszar As Zasieg) As Boolean
        If _liczbaIstniejacych = 0 Then Return False
        Dim bok As Double = _siatka.BokSegmentu
        Dim k0 As Double = Math.Floor((obszar.YLewy - _zasieg.YLewy) / bok)
        Dim k1 As Double = Math.Floor((obszar.YPrawy - _zasieg.YLewy) / bok)
        Dim w0 As Double = Math.Floor((_zasieg.XGora - obszar.XGora) / bok)
        Dim w1 As Double = Math.Floor((_zasieg.XGora - obszar.XDol) / bok)
        Dim kolumny As Integer = _siatka.LiczbaKolumn, wiersze As Integer = _siatka.LiczbaWierszy
        If k1 < 0 OrElse w1 < 0 OrElse k0 > kolumny - 1 OrElse w0 > wiersze - 1 Then Return False
        For w = CInt(Math.Max(0, w0)) To CInt(Math.Min(wiersze - 1, w1))
            For k = CInt(Math.Max(0, k0)) To CInt(Math.Min(kolumny - 1, k1))
                If _istnieje(w, k) Then Return True
            Next
        Next
        Return False
    End Function

    ''' <summary>
    ''' Kolor w punkcie (X, Y) układu segmentów - interpolacja dwuliniowa. Składowe r, g, b są przemnożone przez
    ''' krycie (0-255), a - krycie (0-255). Zwraca False poza obrazem lub w miejscu brakującego segmentu.
    ''' </summary>
    Public Function Probka(x As Double, y As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double, ByRef a As Double) As Boolean
        'współrzędne piksela (środki pikseli w liczbach całkowitych)
        Dim kol As Double = (y - _zasieg.YLewy) / _pikselY - 0.5
        Dim wie As Double = (_zasieg.XGora - x) / _pikselX - 0.5
        Dim szer As Long = _siatka.SzerokoscPx, wys As Long = _siatka.WysokoscPx
        If kol < -0.5 OrElse wie < -0.5 OrElse kol > szer - 0.5 OrElse wie > wys - 0.5 Then Return False

        Dim k0 As Long = CLng(Math.Floor(kol)), w0 As Long = CLng(Math.Floor(wie))
        Dim tx As Double = kol - k0, ty As Double = wie - w0
        Dim k1 As Long = Math.Min(k0 + 1, szer - 1), w1 As Long = Math.Min(w0 + 1, wys - 1)
        k0 = Math.Max(0, k0) : w0 = Math.Max(0, w0)

        Dim r00, g00, b00, a00, r10, g10, b10, a10, r01, g01, b01, a01, r11, g11, b11, a11 As Integer
        If Not Piksel(k0, w0, r00, g00, b00, a00) Then Return False
        If Not Piksel(k1, w0, r10, g10, b10, a10) Then r10 = r00 : g10 = g00 : b10 = b00 : a10 = a00
        If Not Piksel(k0, w1, r01, g01, b01, a01) Then r01 = r00 : g01 = g00 : b01 = b00 : a01 = a00
        If Not Piksel(k1, w1, r11, g11, b11, a11) Then r11 = r10 : g11 = g10 : b11 = b10 : a11 = a10
        'interpolacja wartości przemnożonych przez krycie - przezroczyste piksele nie zabarwiają sąsiednich
        Dim w00 As Double = (1 - tx) * (1 - ty) * a00 / 255.0, w10 As Double = tx * (1 - ty) * a10 / 255.0
        Dim w01 As Double = (1 - tx) * ty * a01 / 255.0, w11 As Double = tx * ty * a11 / 255.0
        r = r00 * w00 + r10 * w10 + r01 * w01 + r11 * w11
        g = g00 * w00 + g10 * w10 + g01 * w01 + g11 * w11
        b = b00 * w00 + b10 * w10 + b01 * w01 + b11 * w11
        a = (w00 + w10 + w01 + w11) * 255.0
        Return True
    End Function

    ''' <summary>Piksel całego obrazu (kolumna, wiersz od lewego górnego narożnika).</summary>
    Private Function Piksel(kol As Long, wie As Long, ByRef r As Integer, ByRef g As Integer, ByRef b As Integer, ByRef a As Integer) As Boolean
        Dim sk As Integer = CInt(kol \ _bok) + 1, sw As Integer = CInt(wie \ _bok) + 1
        Dim dane = Segment(sw, sk)
        If dane Is Nothing Then Return False
        Dim i As Integer = (CInt(wie Mod _bok) * _bok + CInt(kol Mod _bok)) * 4
        b = dane(i) : g = dane(i + 1) : r = dane(i + 2) : a = dane(i + 3)
        Return True
    End Function

    ''' <summary>Zdekodowany segment (BGRA) albo Nothing, gdy brak pliku.</summary>
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
            dane = If(_istnieje(wiersz - 1, kolumna - 1), Wczytaj(wiersz, kolumna), Nothing)
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
        Dim sciezka As String = SciezkaSegmentu(wiersz, kolumna)
        If Not File.Exists(sciezka) Then Return Nothing
        Try
            Using zrodlo As Image = Image.FromFile(sciezka)
                Using argb As New Bitmap(_bok, _bok, PixelFormat.Format32bppArgb)
                    Using g As Graphics = Graphics.FromImage(argb)
                        g.Clear(Color.Transparent)
                        g.InterpolationMode = If(zrodlo.Width = _bok AndAlso zrodlo.Height = _bok, InterpolationMode.NearestNeighbor, InterpolationMode.HighQualityBicubic)
                        g.PixelOffsetMode = PixelOffsetMode.Half
                        g.DrawImage(zrodlo, New Rectangle(0, 0, _bok, _bok), 0, 0, zrodlo.Width, zrodlo.Height, GraphicsUnit.Pixel)
                    End Using
                    Dim wynik(_bok * _bok * 4 - 1) As Byte
                    Dim d As BitmapData = argb.LockBits(New Rectangle(0, 0, _bok, _bok), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
                    Try
                        For y = 0 To _bok - 1
                            Marshal.Copy(IntPtr.Add(d.Scan0, y * d.Stride), wynik, y * _bok * 4, _bok * 4)
                        Next
                    Finally
                        argb.UnlockBits(d)
                    End Try
                    If _bialePrzezroczyste Then
                        For i = 0 To wynik.Length - 4 Step 4
                            If wynik(i) >= ProgBieli AndAlso wynik(i + 1) >= ProgBieli AndAlso wynik(i + 2) >= ProgBieli Then wynik(i + 3) = 0
                        Next
                    End If
                    Return wynik
                End Using
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>Próg jasności "białego" piksela (zostawia margines na szum kompresji JPEG).</summary>
    Private Const ProgBieli As Byte = 248

End Class

''' <summary>Rodzaj eksportu.</summary>
Public Enum RodzajEksportu
    ''' <summary>KMZ dla odbiorników Garmin (Custom Maps): kafle JPEG do 1024 px, limit liczby kafli.</summary>
    KmzGarmin
    ''' <summary>KMZ bez limitu liczby kafli (Locus Map, OruxMaps, Google Earth).</summary>
    Kmz
    ''' <summary>MBTiles - kafle Web Mercator (Locus Map, OsmAnd, OruxMaps i inne).</summary>
    MBTiles
    ''' <summary>SQLiteDB (format RMaps) - kafle Web Mercator dla OsmAnd i Locus Map.</summary>
    SQLiteDB
    ''' <summary>Mapa offline OruxMaps: folder z bazą OruxMapsImages.db i kalibracją .otrk2.xml.</summary>
    OruxMaps
End Enum

''' <summary>Warstwa mapy nakładana podczas eksportu na mapę podstawową (np. granice działek na ortofotomapie).</summary>
Public Class WarstwaEksportu
    Public Property Zrodlo As ZrodloSegmentow
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    ''' <summary>Krycie warstwy: 1 - pełne, 0,5 - półprzezroczysta.</summary>
    Public Property Krycie As Double = 1
    ''' <summary>Opis warstwy (zbiór map i warstwa).</summary>
    Public Property Opis As String = ""
    ''' <summary>Adres serwera, z którego pobrano warstwę (źródło danych w opisie mapy).</summary>
    Public Property AdresSerwera As String = ""
End Class

''' <summary>Parametry eksportu.</summary>
Public Class ZadanieEksportu
    Public Property Rodzaj As RodzajEksportu = RodzajEksportu.KmzGarmin
    ''' <summary>Segmenty mapy podstawowej - wyznaczają obszar eksportu.</summary>
    Public Property Zrodlo As ZrodloSegmentow
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    ''' <summary>Plik wynikowy (OruxMaps - folder mapy).</summary>
    Public Property Plik As String = ""
    ''' <summary>Nazwa mapy widoczna w aplikacji.</summary>
    Public Property Nazwa As String = ""
    Public Property JakoscJpeg As Integer = 85
    ''' <summary>KMZ: maksymalna liczba kafli (Garmin: 100).</summary>
    Public Property MaksKafli As Integer = 100
    ''' <summary>Kafle Web Mercator: zakres poziomów powiększenia.</summary>
    Public Property PoziomMin As Integer = 10
    Public Property PoziomMax As Integer = 16
    ''' <summary>Kafle Web Mercator: kafle PNG (przezroczystość poza obszarem mapy) zamiast JPEG.</summary>
    Public Property KaflePng As Boolean
    ''' <summary>
    ''' Mapa-nakładka: warstwa z przezroczystym tłem wyświetlana w aplikacji nad mapą podkładową
    ''' (kafle PNG, w MBTiles typ "overlay"). Nie dotyczy KMZ dla odbiorników Garmin (tylko JPEG).
    ''' </summary>
    Public Property Nakladka As Boolean
    ''' <summary>Warstwy nakładane na mapę podstawową (od dołu do góry).</summary>
    Public ReadOnly Property Nakladki As New List(Of WarstwaEksportu)
    ''' <summary>Opis mapy: źródła danych, data pobrania, warstwy.</summary>
    Public Property Metadane As New MetadaneMapy()

    ''' <summary>Czy kafle zapisywane są w PNG (z kanałem alfa).</summary>
    Public ReadOnly Property Png As Boolean
        Get
            Select Case Rodzaj
                Case RodzajEksportu.KmzGarmin : Return False
                Case RodzajEksportu.Kmz : Return Nakladka
                Case Else : Return KaflePng OrElse Nakladka
            End Select
        End Get
    End Property

    ''' <summary>Czy eksport tworzy kafle Web Mercator w piramidzie poziomów (MBTiles, SQLiteDB, OruxMaps).</summary>
    Public ReadOnly Property Piramida As Boolean
        Get
            Return Rodzaj = RodzajEksportu.MBTiles OrElse Rodzaj = RodzajEksportu.SQLiteDB OrElse Rodzaj = RodzajEksportu.OruxMaps
        End Get
    End Property

    ''' <summary>Warstwy w kolejności rysowania: mapa podstawowa, a na niej nakładki.</summary>
    Public Function WarstwyRenderowania() As List(Of WarstwaEksportu)
        Dim lista As New List(Of WarstwaEksportu) From {New WarstwaEksportu With {.Zrodlo = Zrodlo, .Uklad = Uklad, .Krycie = 1}}
        For Each w In Nakladki
            If w.Zrodlo IsNot Nothing AndAlso w.Krycie > 0 Then lista.Add(w)
        Next
        Return lista
    End Function
End Class

''' <summary>Szacunek rozmiaru pliku eksportu (na podstawie kilku wyrenderowanych próbnych kafli).</summary>
Public Class SzacunekRozmiaru
    ''' <summary>Szacowany rozmiar całego pliku w bajtach.</summary>
    Public Property Bajty As Long
    ''' <summary>Kafle Web Mercator: szacowany rozmiar poszczególnych poziomów (bajty).</summary>
    Public ReadOnly Property Poziomy As New SortedDictionary(Of Integer, Long)
    ''' <summary>Szacowana liczba kafli z danymi (puste kafle nie są zapisywane).</summary>
    Public Property LiczbaKafli As Long
    ''' <summary>Liczba wyrenderowanych próbek.</summary>
    Public Property Probki As Integer

    ''' <summary>Rozmiar w czytelnej postaci, np. "35,2 MB".</summary>
    Public Shared Function Tekst(bajty As Long) As String
        If bajty < 1024L * 1024 Then Return (bajty / 1024.0).ToString("0") & " kB"
        If bajty < 1024L * 1024 * 1024 Then Return (bajty / 1024.0 / 1024).ToString("0.0") & " MB"
        Return (bajty / 1024.0 / 1024 / 1024).ToString("0.00") & " GB"
    End Function
End Class

''' <summary>Eksport zestawu segmentów do KMZ (WGS84) i kafli Web Mercator (MBTiles, SQLiteDB, OruxMaps).</summary>
Public NotInheritable Class EksportMapy

    Private Sub New()
    End Sub

    ''' <summary>Rozmiar piksela źródła w metrach (mapa podstawowa).</summary>
    Public Shared Function PikselWMetrach(z As ZadanieEksportu) As Double
        Return PikselWMetrach(z.Zrodlo, z.Uklad)
    End Function

    ''' <summary>Rozmiar piksela zestawu segmentów w metrach.</summary>
    Public Shared Function PikselWMetrach(zrodlo As ZrodloSegmentow, uklad As UkladWspolrzednych) As Double
        Dim s = zrodlo.Siatka
        If Not uklad.Geograficzny Then Return s.RozmiarPiksela
        Dim mSz As Double, mDl As Double
        Georeferencja.MetrowNaStopien(s.ZasiegSiatki.Srodek.X, mSz, mDl)
        Return s.RozmiarPiksela * mSz
    End Function

    ''' <summary>
    ''' Liczba próbek na bok piksela kafla (do 4): gdy kafel ma mniejszą rozdzielczość niż którakolwiek z warstw,
    ''' piksel kafla uśrednia kilka pikseli źródła.
    ''' </summary>
    Private Shared Function LiczbaProbek(z As ZadanieEksportu, pikselKaflaMetry As Double) As Integer
        Dim n As Integer = 1
        For Each w In z.WarstwyRenderowania()
            n = Math.Max(n, CInt(Math.Ceiling(pikselKaflaMetry / PikselWMetrach(w.Zrodlo, w.Uklad) - 0.01)))
        Next
        Return Math.Min(4, n)
    End Function

    ''' <summary>Obszar eksportu w WGS84 (prostokąt opisany na mapie podstawowej).</summary>
    Public Shared Function ObszarKafli(z As ZadanieEksportu) As ProstokatGeo
        Return ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.Siatka.ZasiegSiatki)
    End Function

    ''' <summary>Plan kafli KMZ dla zadania.</summary>
    Public Shared Function PlanKmz(z As ZadanieEksportu) As PlanKmz
        Dim obszar = ProstokatGeo.Wpisany(z.Uklad, z.Zrodlo.Siatka.ZasiegSiatki)
        Return New PlanKmz(obszar, PikselWMetrach(z), 1024, If(z.Rodzaj = RodzajEksportu.KmzGarmin, z.MaksKafli, 0))
    End Function

    ''' <summary>Liczba kafli Web Mercator we wszystkich poziomach (z pustymi - przed pominięciem kafli bez danych).</summary>
    Public Shared Function LiczbaKafliMBTiles(z As ZadanieEksportu) As Long
        Dim obszar = ObszarKafli(z)
        Dim suma As Long = 0
        For poziom = z.PoziomMin To z.PoziomMax
            suma += WebMercator.LiczbaKafli(obszar, poziom)
        Next
        Return suma
    End Function

    Public Shared Sub Eksportuj(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Try
            If z.Piramida Then
                EksportPiramidy(z, postep, token)
            Else
                EksportKmz(z, postep, token)
            End If
        Catch
            'niedokończony plik nie może pozostać na dysku
            Try
                SQLiteConnection.ClearAllPools()
                UsunWynik(z)
            Catch
            End Try
            Throw
        End Try
    End Sub

    ''' <summary>Usuwa plik wynikowy (OruxMaps - pliki mapy w jej folderze i pusty folder).</summary>
    Private Shared Sub UsunWynik(z As ZadanieEksportu)
        If z.Rodzaj = RodzajEksportu.OruxMaps Then
            If Not Directory.Exists(z.Plik) Then Exit Sub
            Dim baza As String = Path.Combine(z.Plik, MapaOruxMaps.NazwaBazy)
            If File.Exists(baza) Then File.Delete(baza)
            For Each plik In Directory.GetFiles(z.Plik, "*" & MapaOruxMaps.RozszerzenieKalibracji)
                File.Delete(plik)
            Next
            If Directory.GetFileSystemEntries(z.Plik).Length = 0 Then Directory.Delete(z.Plik)
        ElseIf File.Exists(z.Plik) Then
            File.Delete(z.Plik)
        End If
    End Sub

#Region "Renderowanie"

    ''' <summary>
    ''' Renderuje kafel: dla każdego piksela (px, py) funkcja przelicz zwraca punkt w WGS84, przeliczany dalej do układu
    ''' każdej z warstw. Warstwy nakładane są kolejno z uwzględnieniem ich krycia i przezroczystości pikseli.
    ''' Gdy kafel ma mniejszą rozdzielczość niż źródło - nadpróbkowanie (do 4 x 4 próbek na piksel).
    ''' Zwraca bitmapę 32-bitową (piksele poza obrazem przezroczyste) albo Nothing, gdy kafel jest pusty.
    ''' </summary>
    Private Shared Function RenderujKafel(warstwy As List(Of WarstwaEksportu), szer As Integer, wys As Integer, nadprobkowanie As Integer,
                                          przelicz As Func(Of Double, Double, PunktGeo)) As Bitmap
        'warstwy, które mają segmenty w obszarze kafla (szybkie pominięcie pustych kafli, np. poza korytarzem trasy)
        Dim aktywne As New List(Of WarstwaEksportu)
        Dim siatki As New List(Of SiatkaPrzeliczen)
        For Each w In warstwy
            Dim uklad = w.Uklad
            Dim siatka As New SiatkaPrzeliczen(szer, wys, 16, Function(x, y) uklad.ZWgs84(przelicz(x, y)))
            If w.Zrodlo.MaDane(siatka.Obszar) Then
                aktywne.Add(w)
                siatki.Add(siatka)
            End If
        Next
        If aktywne.Count = 0 Then Return Nothing

        Dim n As Integer = Math.Max(1, Math.Min(4, nadprobkowanie))
        Dim dane(szer * wys * 4 - 1) As Byte
        Dim pusty As Boolean = True
        For py = 0 To wys - 1
            For px = 0 To szer - 1
                'kolor wynikowy przemnożony przez krycie (0-1)
                Dim wr As Double = 0, wg As Double = 0, wb As Double = 0, wa As Double = 0
                For iw = 0 To aktywne.Count - 1
                    Dim zrodlo = aktywne(iw).Zrodlo
                    Dim siatka = siatki(iw)
                    Dim sr As Double = 0, sg As Double = 0, sb As Double = 0, sa As Double = 0
                    For j = 0 To n - 1
                        For i = 0 To n - 1
                            'siatka przeliczeń liczona jest dla lewego górnego narożnika piksela (0,0); próbki rozłożone w pikselu
                            Dim p = siatka.Punkt(px + (i + 0.5) / n, py + (j + 0.5) / n)
                            Dim r As Double, g As Double, b As Double, a As Double
                            If zrodlo.Probka(p.X, p.Y, r, g, b, a) Then
                                sr += r : sg += g : sb += b : sa += a
                            End If
                        Next
                    Next
                    If sa <= 0 Then Continue For
                    Dim k As Double = aktywne(iw).Krycie / (n * n * 255.0)
                    sr *= k : sg *= k : sb *= k : sa *= k
                    'nałożenie warstwy na dotychczasowy obraz
                    wr = sr + wr * (1 - sa)
                    wg = sg + wg * (1 - sa)
                    wb = sb + wb * (1 - sa)
                    wa = sa + wa * (1 - sa)
                Next
                If wa > 0.001 Then
                    Dim o As Integer = (py * szer + px) * 4
                    dane(o) = Bajt(wb / wa)
                    dane(o + 1) = Bajt(wg / wa)
                    dane(o + 2) = Bajt(wr / wa)
                    dane(o + 3) = Bajt(wa)
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

    ''' <summary>Wartość 0-1 jako bajt 0-255.</summary>
    Private Shared Function Bajt(v As Double) As Byte
        Return CByte(Math.Max(0, Math.Min(255, Math.Round(v * 255))))
    End Function

    Private Shared ReadOnly KoderJpeg As ImageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)

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
                    If KoderJpeg IsNot Nothing Then
                        Using parametry As New EncoderParameters(1)
                            parametry.Param(0) = New EncoderParameter(Imaging.Encoder.Quality, CLng(Math.Max(1, Math.Min(100, jakosc))))
                            tlo.Save(ms, KoderJpeg, parametry)
                        End Using
                    Else
                        tlo.Save(ms, ImageFormat.Jpeg)
                    End If
                End Using
            End If
            Return ms.ToArray()
        End Using
    End Function

    ''' <summary>Przeliczenie piksela kafla Web Mercator (lewy górny narożnik kafla: kx, ky w pikselach globalnych) do WGS84.</summary>
    Private Shared Function PrzeliczenieKafla(kx As Double, ky As Double, poziom As Integer) As Func(Of Double, Double, PunktGeo)
        Return Function(x As Double, y As Double) New PunktGeo(WebMercator.Szerokosc(ky + y, poziom), WebMercator.Dlugosc(kx + x, poziom))
    End Function

    ''' <summary>Czy w kafelku Web Mercator jest choć jeden segment którejś z warstw (sprawdzenie bez renderowania).</summary>
    Private Shared Function KafelMaDane(warstwy As List(Of WarstwaEksportu), tx As Integer, ty As Integer, poziom As Integer) As Boolean
        Dim bok As Double = WebMercator.RozmiarKafla
        Dim narozniki() As PunktGeo = {
            New PunktGeo(WebMercator.Szerokosc(ty * bok, poziom), WebMercator.Dlugosc(tx * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc(ty * bok, poziom), WebMercator.Dlugosc((tx + 1) * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc((ty + 1) * bok, poziom), WebMercator.Dlugosc(tx * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc((ty + 1) * bok, poziom), WebMercator.Dlugosc((tx + 1) * bok, poziom))}
        For Each w In warstwy
            If w.Zrodlo.MaDane(ObszarPunktow(w.Uklad, narozniki)) Then Return True
        Next
        Return False
    End Function

    ''' <summary>Prostokąt w układzie obejmujący punkty WGS84.</summary>
    Private Shared Function ObszarPunktow(uklad As UkladWspolrzednych, punkty As IEnumerable(Of PunktGeo)) As Zasieg
        Dim xMin As Double = Double.MaxValue, xMax As Double = Double.MinValue, yMin As Double = Double.MaxValue, yMax As Double = Double.MinValue
        For Each g In punkty
            Dim p = uklad.ZWgs84(g)
            xMin = Math.Min(xMin, p.X) : xMax = Math.Max(xMax, p.X)
            yMin = Math.Min(yMin, p.Y) : yMax = Math.Max(yMax, p.Y)
        Next
        Return New Zasieg(xMin, yMin, xMax, yMax)
    End Function

#End Region

#Region "KMZ"

    Private Shared Sub EksportKmz(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Dim plan = PlanKmz(z)
        Dim warstwy = z.WarstwyRenderowania()
        Dim nadprobkowanie As Integer = LiczbaProbek(z, PikselWMetrach(z) * plan.Pomniejszenie)
        Dim png As Boolean = z.Png
        Dim gotowe As Integer = 0
        Using kmz As New ZapisKmz(z.Plik, z.Nazwa, 50)
            kmz.Opis = z.Metadane.Opis
            For w = 0 To plan.Wiersze - 1
                For k = 0 To plan.Kolumny - 1
                    token.ThrowIfCancellationRequested()
                    Dim px = plan.PikseleKafla(w, k)
                    Dim x0 As Integer = px.Item1, y0 As Integer = px.Item2
                    Dim f = Function(x As Double, y As Double) New PunktGeo(plan.Obszar.Polnoc - (y0 + y) * plan.PikselSzerokosc,
                                                                            plan.Obszar.Zachod + (x0 + x) * plan.PikselDlugosc)
                    Using bmp = RenderujKafel(warstwy, px.Item3, px.Item4, nadprobkowanie, f)
                        If bmp IsNot Nothing Then
                            kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & If(png, ".png", ".jpg"),
                                           Koduj(bmp, png, z.JakoscJpeg), plan.ZasiegKafla(w, k))
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

#Region "Kafle Web Mercator (MBTiles, SQLiteDB, OruxMaps)"

    Private Shared Function UtworzMagazyn(z As ZadanieEksportu, obszar As ProstokatGeo) As MagazynKafli
        Select Case z.Rodzaj
            Case RodzajEksportu.SQLiteDB : Return New MagazynSQLiteDB(z)
            Case RodzajEksportu.OruxMaps : Return New MagazynOruxMaps(z, obszar)
            Case Else : Return New MagazynMBTiles(z, obszar)
        End Select
    End Function

    Private Shared Sub EksportPiramidy(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        UsunWynik(z)
        Dim obszar = ObszarKafli(z)
        Dim warstwy = z.WarstwyRenderowania()
        Dim wszystkie As Long = Math.Max(1, LiczbaKafliMBTiles(z))
        Dim gotowe As Long = 0
        Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2

        Using m = UtworzMagazyn(z, obszar)
            'najwyższy poziom - kafle renderowane bezpośrednio z segmentów
            Dim zmax As Integer = z.PoziomMax
            Dim nadprobkowanie As Integer = LiczbaProbek(z, WebMercator.RozmiarPiksela(srodekSz, zmax))
            Dim r = WebMercator.ZakresKafli(obszar, zmax)
            m.RozpocznijPoziom(zmax)
            For ty = r.Item2 To r.Item4
                For tx = r.Item1 To r.Item3
                    token.ThrowIfCancellationRequested()
                    Using bmp = RenderujKafel(warstwy, 256, 256, nadprobkowanie, PrzeliczenieKafla(tx * 256.0, ty * 256.0, zmax))
                        If bmp IsNot Nothing Then m.Zapisz(New KafelXYZ(tx, ty, zmax), Koduj(bmp, z.Png, z.JakoscJpeg))
                    End Using
                    gotowe += 1
                    postep?.Report(CInt(gotowe * 100 \ wszystkie))
                Next
            Next
            m.ZakonczPoziom()

            'niższe poziomy - z czterech kafli poziomu wyższego (piramida)
            For poziom = zmax - 1 To z.PoziomMin Step -1
                Dim rp = WebMercator.ZakresKafli(obszar, poziom)
                m.RozpocznijPoziom(poziom)
                For ty = rp.Item2 To rp.Item4
                    For tx = rp.Item1 To rp.Item3
                        token.ThrowIfCancellationRequested()
                        Dim k As New KafelXYZ(tx, ty, poziom)
                        Dim dane = KafelZDzieci(m, k, z)
                        If dane IsNot Nothing Then m.Zapisz(k, dane)
                        gotowe += 1
                        postep?.Report(CInt(Math.Min(100, gotowe * 100 \ wszystkie)))
                    Next
                Next
                m.ZakonczPoziom()
            Next
            m.ZakonczZapis()
        End Using
        SQLiteConnection.ClearAllPools()
    End Sub

    ''' <summary>Kafel poziomu niższego złożony z czterech kafli poziomu wyższego (pomniejszenie 2x).</summary>
    Private Shared Function KafelZDzieci(m As MagazynKafli, k As KafelXYZ, z As ZadanieEksportu) As Byte()
        Using duzy As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
            Dim jest As Boolean = False
            Using g = Graphics.FromImage(duzy)
                g.Clear(Color.Transparent)
                For dy = 0 To 1
                    For dx = 0 To 1
                        Dim dane = m.Odczytaj(New KafelXYZ(k.X * 2 + dx, k.Y * 2 + dy, k.Z + 1))
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
                Return Koduj(maly, z.Png, z.JakoscJpeg)
            End Using
        End Using
    End Function

#End Region

#Region "Szacowanie rozmiaru"

    ''' <summary>
    ''' Szacuje rozmiar pliku: renderuje kilka próbnych kafli z danymi i mnoży ich średni rozmiar przez liczbę
    ''' kafli z danymi (kafle bez segmentów, np. poza korytarzem trasy, nie są zapisywane).
    ''' Dla kafli Web Mercator szacowane są poziomy od PoziomMin do poziomDo (np. do najwyższego sensownego poziomu -
    ''' wynik pozwala dobrać zakres poziomów do limitu rozmiaru).
    ''' </summary>
    Public Shared Function SzacujRozmiar(z As ZadanieEksportu, poziomDo As Integer, token As CancellationToken) As SzacunekRozmiaru
        Dim wynik As New SzacunekRozmiaru()
        Dim warstwy = z.WarstwyRenderowania()
        Dim los As New Random(12345)

        If Not z.Piramida Then
            Dim plan = PlanKmz(z)
            Dim zDanymi As New List(Of Tuple(Of Integer, Integer))
            For w = 0 To plan.Wiersze - 1
                For k = 0 To plan.Kolumny - 1
                    Dim zk = plan.ZasiegKafla(w, k)
                    Dim narozniki() As PunktGeo = {New PunktGeo(zk.Polnoc, zk.Zachod), New PunktGeo(zk.Polnoc, zk.Wschod),
                                                   New PunktGeo(zk.Poludnie, zk.Zachod), New PunktGeo(zk.Poludnie, zk.Wschod)}
                    If warstwy.Any(Function(wa) wa.Zrodlo.MaDane(ObszarPunktow(wa.Uklad, narozniki))) Then zDanymi.Add(Tuple.Create(w, k))
                Next
            Next
            If zDanymi.Count = 0 Then Return wynik
            Dim nadprobkowanie As Integer = LiczbaProbek(z, PikselWMetrach(z) * plan.Pomniejszenie)
            Dim suma As Long = 0
            For Each wk In Probka(zDanymi, 2, los)
                token.ThrowIfCancellationRequested()
                Dim px = plan.PikseleKafla(wk.Item1, wk.Item2)
                Dim x0 As Integer = px.Item1, y0 As Integer = px.Item2
                Dim f = Function(x As Double, y As Double) New PunktGeo(plan.Obszar.Polnoc - (y0 + y) * plan.PikselSzerokosc,
                                                                        plan.Obszar.Zachod + (x0 + x) * plan.PikselDlugosc)
                Using bmp = RenderujKafel(warstwy, px.Item3, px.Item4, nadprobkowanie, f)
                    If bmp IsNot Nothing Then
                        suma += Koduj(bmp, z.Png, z.JakoscJpeg).Length
                        wynik.Probki += 1
                    End If
                End Using
            Next
            If wynik.Probki = 0 Then Return wynik
            wynik.LiczbaKafli = zDanymi.Count
            wynik.Bajty = CLng(suma / wynik.Probki * zDanymi.Count)
            Return wynik
        End If

        'kafle Web Mercator: udział kafli z danymi na każdym poziomie (sprawdzenie segmentów na próbie kafli)
        Dim obszar = ObszarKafli(z)
        Dim gora As Integer = Math.Max(z.PoziomMin, poziomDo)
        Dim kafleZDanymi As New Dictionary(Of Integer, Double)
        Dim probkiGora As New List(Of Tuple(Of Integer, Integer))
        For poziom = z.PoziomMin To gora
            token.ThrowIfCancellationRequested()
            Dim r = WebMercator.ZakresKafli(obszar, poziom)
            Dim liczba As Long = WebMercator.LiczbaKafli(obszar, poziom)
            Dim kandydaci As New List(Of Tuple(Of Integer, Integer))
            If liczba <= 400 Then
                For ty = r.Item2 To r.Item4
                    For tx = r.Item1 To r.Item3
                        kandydaci.Add(Tuple.Create(tx, ty))
                    Next
                Next
            Else
                For i = 1 To 400
                    kandydaci.Add(Tuple.Create(los.Next(r.Item1, r.Item3 + 1), los.Next(r.Item2, r.Item4 + 1)))
                Next
            End If
            Dim biezacy As Integer = poziom
            Dim trafione = kandydaci.FindAll(Function(t) KafelMaDane(warstwy, t.Item1, t.Item2, biezacy))
            kafleZDanymi(poziom) = liczba * trafione.Count / CDbl(Math.Max(1, kandydaci.Count))
            If poziom = gora Then probkiGora = trafione
        Next

        'średni rozmiar kafla z danymi - z próbnych kafli najwyższego poziomu
        Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim nadpr As Integer = LiczbaProbek(z, WebMercator.RozmiarPiksela(srodekSz, gora))
        Dim sumaBajtow As Long = 0
        For Each t In Probka(probkiGora, 12, los)
            token.ThrowIfCancellationRequested()
            Using bmp = RenderujKafel(warstwy, 256, 256, nadpr, PrzeliczenieKafla(t.Item1 * 256.0, t.Item2 * 256.0, gora))
                If bmp IsNot Nothing Then
                    sumaBajtow += Koduj(bmp, z.Png, z.JakoscJpeg).Length
                    wynik.Probki += 1
                End If
            End Using
        Next
        If wynik.Probki = 0 Then Return wynik
        Dim sredni As Double = sumaBajtow / CDbl(wynik.Probki)
        For poziom = z.PoziomMin To gora
            Dim bajty As Long = CLng(kafleZDanymi(poziom) * sredni)
            wynik.Poziomy(poziom) = bajty
            wynik.Bajty += bajty
            wynik.LiczbaKafli += CLng(kafleZDanymi(poziom))
        Next
        Return wynik
    End Function

    ''' <summary>Losowa próbka elementów listy (bez powtórzeń).</summary>
    Private Shared Function Probka(Of T)(lista As List(Of T), ile As Integer, los As Random) As List(Of T)
        Dim kopia As New List(Of T)(lista)
        Dim wynik As New List(Of T)
        While kopia.Count > 0 AndAlso wynik.Count < ile
            Dim i As Integer = los.Next(kopia.Count)
            wynik.Add(kopia(i))
            kopia.RemoveAt(i)
        End While
        Return wynik
    End Function

    ''' <summary>
    ''' Najwyższy poziom powiększenia, przy którym szacowany rozmiar poziomów od PoziomMin nie przekracza limitu;
    ''' -1, gdy limitu nie spełnia nawet sam poziom PoziomMin.
    ''' </summary>
    Public Shared Function PoziomDlaLimitu(szacunek As SzacunekRozmiaru, limitBajtow As Long) As Integer
        Dim suma As Long = 0
        Dim wynik As Integer = -1
        For Each p In szacunek.Poziomy
            suma += p.Value
            If suma > limitBajtow Then Exit For
            wynik = p.Key
        Next
        Return wynik
    End Function

#End Region

End Class

''' <summary>
''' Zapis piramidy kafli Web Mercator w bazie SQLite: kafle kolejnych poziomów zapisywane są w osobnych transakcjach,
''' a kafle poziomu wyższego odczytywane z bazy przy składaniu poziomu niższego.
''' </summary>
Friend MustInherit Class MagazynKafli
    Implements IDisposable

    Protected ReadOnly Polaczenie As SQLiteConnection
    Private _transakcja As SQLiteTransaction
    Private _zapis As SQLiteCommand
    Private _odczyt As SQLiteCommand
    Private _pZ, _pX, _pY, _pD, _oZ, _oX, _oY As SQLiteParameter
    Protected ReadOnly Zadanie As ZadanieEksportu

    Protected Sub New(plikBazy As String, z As ZadanieEksportu)
        Zadanie = z
        If File.Exists(plikBazy) Then File.Delete(plikBazy)
        Polaczenie = New SQLiteConnection("Data Source=" & plikBazy & ";Version=3;Journal Mode=WAL;Synchronous=Off;")
        Polaczenie.Open()
        Wykonaj("PRAGMA synchronous = OFF;")
        Wykonaj("PRAGMA journal_mode = WAL;")
    End Sub

    ''' <summary>Przygotowanie poleceń zapisu i odczytu kafla (parametry @z, @x, @y, @d).</summary>
    Protected Sub Przygotuj(sqlZapisu As String, sqlOdczytu As String)
        _zapis = New SQLiteCommand(sqlZapisu, Polaczenie)
        _pZ = _zapis.Parameters.Add("@z", DbType.Int32)
        _pX = _zapis.Parameters.Add("@x", DbType.Int32)
        _pY = _zapis.Parameters.Add("@y", DbType.Int32)
        _pD = _zapis.Parameters.Add("@d", DbType.Binary)
        _odczyt = New SQLiteCommand(sqlOdczytu, Polaczenie)
        _oZ = _odczyt.Parameters.Add("@z", DbType.Int32)
        _oX = _odczyt.Parameters.Add("@x", DbType.Int32)
        _oY = _odczyt.Parameters.Add("@y", DbType.Int32)
    End Sub

    Protected Sub Wykonaj(sql As String)
        Using cmd As New SQLiteCommand(sql, Polaczenie)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Wstawia wiersz z wartościami parametrów @0, @1, ...</summary>
    Protected Sub Wstaw(sql As String, ParamArray wartosci() As Object)
        Using cmd As New SQLiteCommand(sql, Polaczenie)
            For i = 0 To wartosci.Length - 1
                cmd.Parameters.AddWithValue("@" & i.ToString(Globalization.CultureInfo.InvariantCulture), wartosci(i))
            Next
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Kolumny z, x, y bazy dla kafla XYZ.</summary>
    Protected MustOverride Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)

    Public Sub RozpocznijPoziom(poziom As Integer)
        _transakcja = Polaczenie.BeginTransaction()
        _zapis.Transaction = _transakcja
        _odczyt.Transaction = _transakcja
    End Sub

    Public Sub ZakonczPoziom()
        _transakcja.Commit()
        _transakcja.Dispose()
        _transakcja = Nothing
    End Sub

    Public Sub Zapisz(k As KafelXYZ, dane() As Byte)
        Dim i = Indeks(k)
        _pZ.Value = i.Item1
        _pX.Value = i.Item2
        _pY.Value = i.Item3
        _pD.Value = dane
        _zapis.ExecuteNonQuery()
    End Sub

    Public Function Odczytaj(k As KafelXYZ) As Byte()
        Dim i = Indeks(k)
        _oZ.Value = i.Item1
        _oX.Value = i.Item2
        _oY.Value = i.Item3
        Return TryCast(_odczyt.ExecuteScalar(), Byte())
    End Function

    ''' <summary>Zakończenie zapisu właściwe dla formatu (np. tabela opisu, plik kalibracji).</summary>
    Protected Overridable Sub Zakoncz()
    End Sub

    ''' <summary>
    ''' Zakończenie zapisu: opis mapy i powrót do zwykłego dziennika bazy (zamiast WAL używanego przy zapisie) -
    ''' aplikacje mobilne mogą wtedy otwierać plik tylko do odczytu, np. z karty pamięci.
    ''' </summary>
    Public Sub ZakonczZapis()
        Zakoncz()
        Wykonaj("PRAGMA journal_mode = DELETE;")
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            _transakcja?.Dispose()
            _zapis?.Dispose()
            _odczyt?.Dispose()
        Finally
            Polaczenie.Dispose()
        End Try
    End Sub

End Class

''' <summary>MBTiles: wiersze kafli liczone od dołu (TMS), opis mapy w tabeli metadata.</summary>
Friend Class MagazynMBTiles
    Inherits MagazynKafli

    Public Sub New(z As ZadanieEksportu, obszar As ProstokatGeo)
        MyBase.New(z.Plik, z)
        Wykonaj("CREATE TABLE metadata (name text, value text);")
        Wykonaj("CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob);")
        Wykonaj("CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);")
        Dim ci = Globalization.CultureInfo.InvariantCulture
        Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim meta As New Dictionary(Of String, String) From {
            {"name", z.Nazwa}, {"type", If(z.Nakladka, "overlay", "baselayer")}, {"version", "1.1"}, {"description", z.Metadane.Opis},
            {"format", If(z.Png, "png", "jpg")}, {"minzoom", z.PoziomMin.ToString(ci)}, {"maxzoom", z.PoziomMax.ToString(ci)},
            {"bounds", String.Format(ci, "{0},{1},{2},{3}", obszar.Zachod, obszar.Poludnie, obszar.Wschod, obszar.Polnoc)},
            {"center", String.Format(ci, "{0},{1},{2}", (obszar.Zachod + obszar.Wschod) / 2, srodekSz, z.PoziomMax)}}
        If z.Metadane.Atrybucja <> "" Then meta("attribution") = z.Metadane.Atrybucja
        For Each m In meta
            Wstaw("INSERT INTO metadata (name, value) VALUES (@0, @1);", m.Key, m.Value)
        Next
        Przygotuj("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);",
                  "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;")
    End Sub

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        Return Tuple.Create(k.Z, k.X, k.WierszTms)
    End Function

End Class

''' <summary>
''' SQLiteDB (RMaps / BigPlanet) dla OsmAnd i Locus Map: wiersze od północy, poziom zapisany jako 17 - poziom,
''' w tabeli info zakres poziomów (w tej samej, odwróconej numeracji) i sposób numeracji.
''' </summary>
Friend Class MagazynSQLiteDB
    Inherits MagazynKafli

    Public Sub New(z As ZadanieEksportu)
        MyBase.New(z.Plik, z)
        Wykonaj("CREATE TABLE tiles (x int, y int, z int, s int, image blob, PRIMARY KEY (x, y, z, s));")
        Wykonaj("CREATE TABLE info (maxzoom int, minzoom int, tilenumbering text, tilesize int, ellipsoid int, inverted_y int);")
        'tabela wymagana przez bazy SQLite otwierane w Androidzie bez prawa zapisu
        Wykonaj("CREATE TABLE android_metadata (locale text);")
        Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")
        Przygotuj("INSERT OR REPLACE INTO tiles (x, y, z, s, image) VALUES (@x, @y, @z, 0, @d);",
                  "SELECT image FROM tiles WHERE x = @x AND y = @y AND z = @z AND s = 0;")
    End Sub

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        Return Tuple.Create(MapaSQLiteDB.ZapisanyPoziom(k.Z), k.X, k.Y)
    End Function

    Protected Overrides Sub Zakoncz()
        Wstaw("INSERT INTO info (maxzoom, minzoom, tilenumbering, tilesize, ellipsoid, inverted_y) VALUES (@0, @1, @2, @3, @4, @5);",
              MapaSQLiteDB.ZapisanyPoziom(Zadanie.PoziomMin), MapaSQLiteDB.ZapisanyPoziom(Zadanie.PoziomMax), "BigPlanet", 256, 0, 0)
    End Sub

End Class

''' <summary>
''' OruxMaps: baza OruxMapsImages.db w folderze mapy - kafle każdego poziomu numerowane od lewego górnego kafla poziomu,
''' a plik .otrk2.xml opisuje położenie poziomów.
''' </summary>
Friend Class MagazynOruxMaps
    Inherits MagazynKafli

    Private ReadOnly _obszar As ProstokatGeo
    Private ReadOnly _zakresy As New Dictionary(Of Integer, ZakresKafliPoziomu)

    Public Sub New(z As ZadanieEksportu, obszar As ProstokatGeo)
        MyBase.New(PrzygotujFolder(z.Plik), z)
        _obszar = obszar
        Wykonaj("CREATE TABLE tiles (x int, y int, z int, image blob, PRIMARY KEY (x, y, z));")
        Wykonaj("CREATE TABLE android_metadata (locale text);")
        Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")
        Przygotuj("INSERT OR REPLACE INTO tiles (x, y, z, image) VALUES (@x, @y, @z, @d);",
                  "SELECT image FROM tiles WHERE x = @x AND y = @y AND z = @z;")
    End Sub

    Private Shared Function PrzygotujFolder(folder As String) As String
        Directory.CreateDirectory(folder)
        Return Path.Combine(folder, MapaOruxMaps.NazwaBazy)
    End Function

    Private Function Zakres(poziom As Integer) As ZakresKafliPoziomu
        Dim z As ZakresKafliPoziomu
        If Not _zakresy.TryGetValue(poziom, z) Then
            z = ZakresKafliPoziomu.Dla(_obszar, poziom)
            _zakresy(poziom) = z
        End If
        Return z
    End Function

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        Dim z = Zakres(k.Z)
        Return Tuple.Create(k.Z, k.X - z.XMin, k.Y - z.YMin)
    End Function

    Protected Overrides Sub Zakoncz()
        Dim poziomy As New List(Of ZakresKafliPoziomu)
        For p = Zadanie.PoziomMin To Zadanie.PoziomMax
            poziomy.Add(Zakres(p))
        Next
        Dim nazwa As String = Path.GetFileName(Zadanie.Plik.TrimEnd("\"c, "/"c))
        File.WriteAllText(Path.Combine(Zadanie.Plik, nazwa & MapaOruxMaps.RozszerzenieKalibracji),
                          MapaOruxMaps.Kalibracja(Zadanie.Nazwa, poziomy), Georeferencja.Utf8BezBom)
    End Sub

End Class
