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
Imports System.Threading.Tasks
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
    Private ReadOnly _maska As Func(Of Double, Double, Boolean)

    'pliki segmentów istniejące na dysku (wiersz, kolumna od 0) - brakujące, np. poza korytarzem trasy, są przezroczyste
    Private ReadOnly _istnieje(,) As Boolean
    Private ReadOnly _liczbaIstniejacych As Integer

    'pamięć podręczna zdekodowanych segmentów (BGRA, wierszami): klucz = wiersz * 100000 + kolumna (Long - przy ponad
    '21474 wierszach klucz typu Integer się przepełniał); Nothing - brak pliku
    Private ReadOnly _pamiec As New Dictionary(Of Long, LinkedListNode(Of KeyValuePair(Of Long, Byte())))
    Private ReadOnly _kolejnosc As New LinkedList(Of KeyValuePair(Of Long, Byte()))
    Private ReadOnly _maksSegmentow As Integer
    Private _ostatniKlucz As Long = -1
    Private _ostatnieDane() As Byte
    Private _bialePrzezroczyste As Boolean
    Private _odcienieSzarosci As Boolean
    'numer stanu pamięci podręcznej - zmiana opcji w trakcie dekodowania segmentu unieważnia jego wynik
    Private _pokolenie As Integer
    Private ReadOnly _blokada As New Object()
    Private ReadOnly _ostatniWatek As New ThreadLocal(Of Tuple(Of Long, Integer, Byte()))()

    Public Sub New(folder As String, prefiks As String, styl As StylNumeracji, rozszerzenie As String, siatka As Siatka,
                   Optional przytnijDoObszaru As Boolean = False, Optional maska As Func(Of Double, Double, Boolean) = Nothing)
        _folder = folder
        _prefiks = prefiks
        _styl = styl
        _rozszerzenie = rozszerzenie
        _siatka = siatka
        _bok = siatka.BokSegmentuPx
        _zasieg = siatka.ZasiegSiatki
        _maska = maska
        Me.PrzytnijDoObszaru = przytnijDoObszaru
        ZasiegMapy = If(przytnijDoObszaru, siatka.Obszar, _zasieg)
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

    Public ReadOnly Property PrzytnijDoObszaru As Boolean
    Public ReadOnly Property ZasiegMapy As Zasieg
    Friend ReadOnly Property MaMaske As Boolean
        Get
            Return _maska IsNot Nothing
        End Get
    End Property

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
    ''' tak jak w dawnym narzędziu nakładania map png. Zmiana opróżnia pamięć podręczną segmentów.
    ''' </summary>
    Public Property BialePrzezroczyste As Boolean
        Get
            SyncLock _blokada
                Return _bialePrzezroczyste
            End SyncLock
        End Get
        Set(value As Boolean)
            SyncLock _blokada
                If value = _bialePrzezroczyste Then Exit Property
                _bialePrzezroczyste = value
                WyczyscPamiec()
            End SyncLock
        End Set
    End Property

    ''' <summary>
    ''' Konwertuje piksele segmentów do odcieni szarości (luminancja percepcyjna Y = 0.299R + 0.587G + 0.114B).
    ''' Idealne jako neutralny podkład ortofoto lub czytelna mapa topo bez gryzących się barw.
    ''' </summary>
    Public Property OdcienieSzarosci As Boolean
        Get
            SyncLock _blokada
                Return _odcienieSzarosci
            End SyncLock
        End Get
        Set(value As Boolean)
            SyncLock _blokada
                If value = _odcienieSzarosci Then Exit Property
                _odcienieSzarosci = value
                WyczyscPamiec()
            End SyncLock
        End Set
    End Property

    ''' <summary>Opróżnia pamięć podręczną segmentów (wywoływane pod blokadą).</summary>
    Private Sub WyczyscPamiec()
        _pamiec.Clear()
        _kolejnosc.Clear()
        _ostatniKlucz = -1
        _ostatnieDane = Nothing
        Interlocked.Increment(_pokolenie)
    End Sub

    Friend Function SciezkaSegmentu(wiersz As Integer, kolumna As Integer) As String
        Return _folder & _siatka.NazwaSegmentu(_prefiks, _styl, wiersz, kolumna) & "." & _rozszerzenie
    End Function

    ''' <summary>Zwraca ścieżkę do istniejącego segmentu najbliżej środka siatki (na potrzeby próbki podglądu korekty).</summary>
    Public Function ZnajdzIstniejacySegmentSrodkowy() As String
        If _liczbaIstniejacych = 0 Then Return ""
        Dim srW As Integer = _siatka.LiczbaWierszy \ 2 + 1
        Dim srK As Integer = _siatka.LiczbaKolumn \ 2 + 1

        ' Najpierw sprawdź segment najbliżej środka
        Dim sciezkaSrodek = SciezkaSegmentu(srW, srK)
        If File.Exists(sciezkaSrodek) Then Return sciezkaSrodek

        ' Szukaj istniejącego segmentu o najmniejszej odległości kwadratowej od środka
        Dim najlepszaSciezka As String = ""
        Dim minDystansSq As Long = Long.MaxValue

        For w = 1 To _siatka.LiczbaWierszy
            For k = 1 To _siatka.LiczbaKolumn
                If _istnieje(w - 1, k - 1) Then
                    Dim dw As Long = w - srW
                    Dim dk As Long = k - srK
                    Dim dSq As Long = dw * dw + dk * dk
                    If dSq < minDystansSq Then
                        minDystansSq = dSq
                        najlepszaSciezka = SciezkaSegmentu(w, k)
                        If dSq = 0 Then Return najlepszaSciezka
                    End If
                End If
            Next
        Next

        Return najlepszaSciezka
    End Function

    ''' <summary>Nazwa pliku segmentu (bez folderu) - w komunikacie o brakujących segmentach arkusza.</summary>
    Friend Function NazwaPliku(wiersz As Integer, kolumna As Integer) As String
        Return Path.GetFileName(SciezkaSegmentu(wiersz, kolumna))
    End Function

    ''' <summary>Czy w prostokącie (w układzie segmentów) jest co najmniej jeden istniejący segment.</summary>
    Public Function MaDane(obszar As Zasieg) As Boolean
        If _liczbaIstniejacych = 0 Then Return False
        If PrzytnijDoObszaru AndAlso (obszar.XGora <= ZasiegMapy.XDol OrElse obszar.XDol >= ZasiegMapy.XGora OrElse
                                    obszar.YPrawy <= ZasiegMapy.YLewy OrElse obszar.YLewy >= ZasiegMapy.YPrawy) Then Return False
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
        If PrzytnijDoObszaru AndAlso (x < ZasiegMapy.XDol OrElse x > ZasiegMapy.XGora OrElse
                                    y < ZasiegMapy.YLewy OrElse y > ZasiegMapy.YPrawy) Then Return False
        If _maska IsNot Nothing AndAlso Not _maska(x, y) Then Return False
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

    ''' <summary>
    ''' Zdekodowany segment (BGRA) albo Nothing, gdy brak pliku. Dekodowanie odbywa się poza blokadą - dawniej trwało
    ''' pod nią, więc wielowątkowe renderowanie kafli (kompilator MBTiles) czekało na jeden wątek dekodujący.
    ''' </summary>
    Friend Function Segment(wiersz As Integer, kolumna As Integer) As Byte()
        Dim klucz As Long = wiersz * 100000L + kolumna
        Dim genStart As Integer = Volatile.Read(_pokolenie)
        Dim thr = _ostatniWatek.Value
        If thr IsNot Nothing AndAlso thr.Item1 = klucz AndAlso thr.Item2 = genStart Then Return thr.Item3

        Dim pokolenie As Integer, biale As Boolean, szare As Boolean
        SyncLock _blokada
            If klucz = _ostatniKlucz Then
                _ostatniWatek.Value = Tuple.Create(klucz, _pokolenie, _ostatnieDane)
                Return _ostatnieDane
            End If
            Dim wezel As LinkedListNode(Of KeyValuePair(Of Long, Byte())) = Nothing
            If _pamiec.TryGetValue(klucz, wezel) Then
                _kolejnosc.Remove(wezel)
                _kolejnosc.AddFirst(wezel)
                _ostatniKlucz = klucz
                _ostatnieDane = wezel.Value.Value
                _ostatniWatek.Value = Tuple.Create(klucz, _pokolenie, _ostatnieDane)
                Return _ostatnieDane
            End If
            pokolenie = _pokolenie : biale = _bialePrzezroczyste : szare = _odcienieSzarosci
        End SyncLock

        Dim dane() As Byte = If(_istnieje(wiersz - 1, kolumna - 1), Wczytaj(wiersz, kolumna, biale, szare), Nothing)

        SyncLock _blokada
            'opcje zmieniono w trakcie dekodowania - wynik nie trafia do pamięci podręcznej
            If pokolenie <> _pokolenie Then Return dane
            If Not _pamiec.ContainsKey(klucz) Then
                _pamiec(klucz) = _kolejnosc.AddFirst(New KeyValuePair(Of Long, Byte())(klucz, dane))
                While _kolejnosc.Count > _maksSegmentow
                    _pamiec.Remove(_kolejnosc.Last.Value.Key)
                    _kolejnosc.RemoveLast()
                End While
            End If
            _ostatniKlucz = klucz
            _ostatnieDane = dane
            _ostatniWatek.Value = Tuple.Create(klucz, pokolenie, dane)
        End SyncLock
        Return dane
    End Function

    Private Function Wczytaj(wiersz As Integer, kolumna As Integer, bialePrzezroczyste As Boolean, odcienieSzarosci As Boolean) As Byte()
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
                    'białe tło wg kolorów oryginału - po przejściu do odcieni szarości także jasne barwy (np. jasnożółte)
                    'przekraczałyby próg bieli i znikały
                    If bialePrzezroczyste Then
                        For i = 0 To wynik.Length - 4 Step 4
                            If wynik(i) >= ProgBieli AndAlso wynik(i + 1) >= ProgBieli AndAlso wynik(i + 2) >= ProgBieli Then wynik(i + 3) = 0
                        Next
                    End If
                    If odcienieSzarosci Then
                        For i = 0 To wynik.Length - 4 Step 4
                            Dim y As Byte = CByte((CInt(wynik(i + 2)) * 77 + CInt(wynik(i + 1)) * 150 + CInt(wynik(i)) * 29) >> 8)
                            wynik(i) = y     ' B
                            wynik(i + 1) = y ' G
                            wynik(i + 2) = y ' R
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

' RodzajEksportu zdefiniowany jest w bibliotece MapoTero.Core (SzablonyMap.vb).

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
    ''' <summary>Tryb mieszania pikseli z warstwami pod spodem (domyślnie Zwykły / Normal).</summary>
    Public Property TrybMieszania As TrybMieszania = TrybMieszania.Zwykly
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
    ''' <summary>KMZ Garmin: automatyczny podział na tomy (paczki do MaksKafli) bez utraty rozdzielczości.</summary>
    Public Property PodzialNaTomy As Boolean
    ''' <summary>Kolejność rysowania w odbiornikach Garmin (drawOrder / zOrder: 25 - pod wektorem, 50 - standard, 75 - nad drogami).</summary>
    Public Property KolejnoscRysowania As Integer = 50
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
    ''' <summary>Arkusz (RodzajEksportu.GeoTiff): format pliku - GeoTIFF bezstratny, GeoTIFF z kompresją JPEG, JPEG albo PNG.
    ''' Arkusz zastępuje dawne okno Scalanie segmentów, więc ma wszystkie jego formaty.
    ''' </summary>
    Public Property FormatArkusza As FormatArkusza = FormatArkusza.GeoTiffJpeg
    ''' <summary>Arkusz GeoTIFF: czy tworzyć wewnętrzne piramidy (overviews) dla szybkiego otwierania w QGIS i Avenza Maps.</summary>
    Public Property PiramidyGeoTiff As Boolean = True
    ''' <summary>Arkusz: pliki georeferencji obok obrazu (world file i .prj, .map, .tab, .kml).</summary>
    Public Property Georeferencja As New OpcjeGeoreferencji With {.WorldFile = True}
    ''' <summary>Arkusz: segmenty mapy podstawowej, których brakowało w folderze (w arkuszu białe pola) - wypełniane eksportem.</summary>
    Public ReadOnly Property BrakujaceSegmenty As New List(Of String)
    ''' <summary>Arkusz: opcje wektorowej siatki kilometrowej PUWG-92 / UTM nanoszonej na arkusz.</summary>
    Public Property SiatkaKilometrowa As ParametrySiatkiKilometrowej
    ''' <summary>Arkusz: ścieżka do pliku GPX/KML ze śladem nanoszonym na arkusz.</summary>
    Public Property PlikSladuWydruku As String = ""
    ''' <summary>Arkusz: styl linii śladu nanoszonego na arkusz.</summary>
    Public Property StylSladuWydruku As New StylSladuWydruku()
    ''' <summary>Opcje korekcji obrazu pod ekrany transflektywne (Garmin MIP / pełne słońce).</summary>
    Public Property KorekcjaMip As New OpcjeKorekcjiMip()
    ''' <summary>Warstwy nakładane na mapę podstawową (od dołu do góry).</summary>
    Public ReadOnly Property Nakladki As New List(Of WarstwaEksportu)
    ''' <summary>Opis mapy: źródła danych, data pobrania, warstwy.</summary>
    Public Property Metadane As New MetadaneMapy()

    ''' <summary>Czy kafle zapisywane są w PNG (z kanałem alfa).</summary>
    Public ReadOnly Property Png As Boolean
        Get
            Select Case Rodzaj
                Case RodzajEksportu.KmzGarmin : Return Nakladka
                Case RodzajEksportu.Jnx : Return False
                Case RodzajEksportu.Kmz : Return Nakladka
                Case Else : Return KaflePng OrElse Nakladka
            End Select
        End Get
    End Property

    ''' <summary>Czy eksport tworzy kafle Web Mercator w piramidzie poziomów (MBTiles, SQLiteDB, OruxMaps, JNX, GeoPackage).</summary>
    Public ReadOnly Property Piramida As Boolean
        Get
            Return Rodzaj = RodzajEksportu.MBTiles OrElse Rodzaj = RodzajEksportu.SQLiteDB OrElse Rodzaj = RodzajEksportu.OruxMaps OrElse Rodzaj = RodzajEksportu.Jnx OrElse Rodzaj = RodzajEksportu.GeoPackage
        End Get
    End Property

    ''' <summary>
    ''' Mapa wieloskalowa (kreator, JNX): osobne źródło segmentów dla każdego zakresu poziomów. Pusta lista - wszystkie
    ''' poziomy z <see cref="Zrodlo"/>. Poziomy bez źródła na liście nie trafiają do pliku.
    ''' </summary>
    Public ReadOnly Property ZrodlaPoziomow As New List(Of ZrodloPoziomow)

    ''' <summary>Warstwy rysowane na poziomie powiększenia: źródło skali tego poziomu (mapa wieloskalowa) i nakładki; Nothing - poziom poza planem skal.</summary>
    Public Function WarstwyRenderowania(poziom As Integer) As List(Of WarstwaEksportu)
        If ZrodlaPoziomow.Count = 0 Then Return WarstwyRenderowania()
        Dim zp = ZrodlaPoziomow.FirstOrDefault(Function(x) x.PoziomMin <= poziom AndAlso poziom <= x.PoziomMax)
        If zp Is Nothing Then Return Nothing
        Dim lista As New List(Of WarstwaEksportu) From {New WarstwaEksportu With {.Zrodlo = zp.Zrodlo, .Uklad = Uklad, .Krycie = 1}}
        For Each w In Nakladki
            If w.Zrodlo IsNot Nothing AndAlso w.Krycie > 0 Then lista.Add(w)
        Next
        Return lista
    End Function

    ''' <summary>Warstwy w kolejności rysowania: mapa podstawowa, a na niej nakładki.</summary>
    Public Function WarstwyRenderowania() As List(Of WarstwaEksportu)
        Dim lista As New List(Of WarstwaEksportu) From {New WarstwaEksportu With {.Zrodlo = Zrodlo, .Uklad = Uklad, .Krycie = 1}}
        For Each w In Nakladki
            If w.Zrodlo IsNot Nothing AndAlso w.Krycie > 0 Then lista.Add(w)
        Next
        Return lista
    End Function
End Class

''' <summary>Źródło segmentów jednej skali mapy wieloskalowej i jej zakres poziomów powiększenia.</summary>
Public Class ZrodloPoziomow
    Public Property PoziomMin As Integer
    Public Property PoziomMax As Integer
    Public Property Zrodlo As ZrodloSegmentow
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
        If zrodlo Is Nothing Then Return 1.0
        Dim s = zrodlo.Siatka
        If s Is Nothing Then Return 1.0
        If uklad Is Nothing OrElse Not uklad.Geograficzny Then Return s.RozmiarPiksela
        Dim mSz As Double, mDl As Double
        Georeferencja.MetrowNaStopien(s.ZasiegSiatki.Srodek.X, mSz, mDl)
        Return s.RozmiarPiksela * mSz
    End Function

    ''' <summary>
    ''' Liczba próbek na bok piksela kafla (do 4): gdy kafel ma mniejszą rozdzielczość niż którakolwiek z warstw,
    ''' piksel kafla uśrednia kilka pikseli źródła.
    ''' </summary>
    Friend Shared Function LiczbaProbek(warstwy As IEnumerable(Of WarstwaEksportu), pikselKaflaMetry As Double) As Integer
        Dim n As Integer = 1
        For Each w In warstwy
            If w.Zrodlo IsNot Nothing Then
                n = Math.Max(n, CInt(Math.Ceiling(pikselKaflaMetry / PikselWMetrach(w.Zrodlo, w.Uklad) - 0.01)))
            End If
        Next
        Return Math.Min(4, n)
    End Function

    Private Shared Function LiczbaProbek(z As ZadanieEksportu, pikselKaflaMetry As Double) As Integer
        Return LiczbaProbek(z.WarstwyRenderowania(), pikselKaflaMetry)
    End Function

    ''' <summary>Obszar eksportu w WGS84 (prostokąt opisany na mapie podstawowej).</summary>
    Public Shared Function ObszarKafli(z As ZadanieEksportu) As ProstokatGeo
        Return ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.ZasiegMapy)
    End Function

    ''' <summary>Plan kafli KMZ dla zadania.</summary>
    Public Shared Function PlanKmz(z As ZadanieEksportu) As PlanKmz
        Dim obszar = ProstokatGeo.Wpisany(z.Uklad, z.Zrodlo.ZasiegMapy)
        If Not obszar.Poprawny Then obszar = ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.ZasiegMapy)
        'podział na tomy: plan w pełnej rozdzielczości (limit kafli dotyczy pojedynczego tomu, nie całej mapy) -
        'z limitem w planie rozdzielczość malała do MaksKafli i zawsze powstawał jeden tom
        Dim limit As Integer = If(z.Rodzaj = RodzajEksportu.KmzGarmin AndAlso Not z.PodzialNaTomy, z.MaksKafli, 0)
        Return New PlanKmz(obszar, PikselWMetrach(z), 1024, limit)
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

    Public Shared Function IstniejaceTomyKmz(plik As String) As IEnumerable(Of String)
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(plik))
        If Not Directory.Exists(folder) Then Return Enumerable.Empty(Of String)()
        Dim prefix = System.Text.RegularExpressions.Regex.Escape(Path.GetFileNameWithoutExtension(plik))
        Dim wzorzec As New System.Text.RegularExpressions.Regex("^" & prefix & "_tom[0-9]{2,}_z_[0-9]{2,}\.kmz$",
                                                                 System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        Return Directory.EnumerateFiles(folder, "*.kmz").Where(Function(p) wzorzec.IsMatch(Path.GetFileName(p)))
    End Function

    ''' <summary>Eksport kreatora zapisuje całą mapę i kalibrację przed podmianą dotychczasowego wyniku.</summary>
    Public Shared Function EksportujBezpiecznie(z As ZadanieEksportu, postep As IProgress(Of Integer),
                                                 token As CancellationToken) As String()
        Dim docelowy = z.Plik
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(docelowy))
        Dim roboczy = Path.Combine(folder, ".mapotero-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(roboczy)
        Dim kopie = Path.Combine(roboczy, "kopie")
        Dim zachowajKopie As Boolean = False
        Try
            z.Plik = Path.Combine(roboczy, Path.GetFileName(docelowy))
            Eksportuj(z, postep, token)
            token.ThrowIfCancellationRequested()
            If z.Rodzaj = RodzajEksportu.OruxMaps Then
                If Not Directory.Exists(z.Plik) OrElse Not File.Exists(Path.Combine(z.Plik, MapaOruxMaps.NazwaBazy)) Then
                    Throw New IOException("Eksport OruxMaps nie utworzył folderu mapy.")
                End If
                SQLiteConnection.ClearAllPools()
                Dim kopiaOrux As String = Path.Combine(kopie, Path.GetFileName(docelowy))
                Dim mialStary As Boolean = Directory.Exists(docelowy)
                If mialStary Then
                    Directory.CreateDirectory(kopie)
                    Directory.Move(docelowy, kopiaOrux)
                End If
                Try
                    Directory.Move(z.Plik, docelowy)
                Catch
                    If mialStary AndAlso Directory.Exists(kopiaOrux) AndAlso Not Directory.Exists(docelowy) Then
                        Try
                            Directory.Move(kopiaOrux, docelowy)
                        Catch
                        End Try
                    End If
                    Throw
                End Try
                Return {docelowy}
            End If
            Dim pliki = Directory.GetFiles(roboczy).OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase).ToArray()
            Dim mapy = pliki.Where(Function(p) String.Equals(Path.GetExtension(p), Path.GetExtension(docelowy), StringComparison.OrdinalIgnoreCase)).
                Select(Function(p) Path.Combine(folder, Path.GetFileName(p))).ToArray()
            If mapy.Length = 0 Then Throw New IOException("Eksport nie utworzył żadnego pliku mapy.")
            Dim stare As New List(Of String)
            If z.Rodzaj = RodzajEksportu.KmzGarmin Then
                stare.AddRange(IstniejaceTomyKmz(docelowy))
                If File.Exists(docelowy) Then stare.Add(docelowy)
            End If
            WymianaPlikowMapy.Zatwierdz(pliki, folder, kopie, stare.Except(mapy, StringComparer.OrdinalIgnoreCase))
            Return mapy
        Catch ex As AggregateException When Directory.Exists(kopie)
            zachowajKopie = True
            Throw
        Finally
            z.Plik = docelowy
            If Not zachowajKopie Then
                Try
                    Directory.Delete(roboczy, recursive:=True)
                Catch ex As IOException
                Catch ex As UnauthorizedAccessException
                End Try
            End If
        End Try
    End Function

    Public Shared Sub Eksportuj(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Try
            If z.Rodzaj = RodzajEksportu.GeoTiff OrElse z.Rodzaj = RodzajEksportu.TrekBuddy Then
                EksportGeoTiff(z, postep, token)
            ElseIf z.Rodzaj = RodzajEksportu.Jnx AndAlso z.Zrodlo.PrzytnijDoObszaru Then
                EksportPrzycietegoJnx(z, postep, token)
            ElseIf z.Piramida Then
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
    Public Shared Sub UsunWynik(z As ZadanieEksportu)
        If z.Rodzaj = RodzajEksportu.TrekBuddy Then
            If File.Exists(z.Plik) Then File.Delete(z.Plik)
            Dim folderMapy = FolderRoboczyTrekBuddy(z)
            If Directory.Exists(folderMapy) Then Directory.Delete(folderMapy, True)
        ElseIf z.Rodzaj = RodzajEksportu.GeoTiff Then
            'arkusz zapisywany jest do pliku tymczasowego i podmieniany na końcu - przerwanie albo błąd nie usuwa
            'poprzedniego, kompletnego arkusza o tej samej nazwie (jak dawniej w oknie scalania)
            Dim tmp = z.Plik & ".tmp"
            If File.Exists(tmp) Then File.Delete(tmp)
        ElseIf z.Rodzaj = RodzajEksportu.OruxMaps Then
            If Not Directory.Exists(z.Plik) Then Exit Sub
            UsunPlikBazy(Path.Combine(z.Plik, MapaOruxMaps.NazwaBazy))
            For Each plik In Directory.GetFiles(z.Plik, "*" & MapaOruxMaps.RozszerzenieKalibracji)
                File.Delete(plik)
            Next
            If Directory.GetFileSystemEntries(z.Plik).Length = 0 Then Directory.Delete(z.Plik)
        ElseIf z.Rodzaj = RodzajEksportu.Jnx Then
            If File.Exists(z.Plik) Then File.Delete(z.Plik)
            UsunPlikBazy(z.Plik & ".tmp_tiles")
        ElseIf z.Rodzaj = RodzajEksportu.KmzGarmin OrElse z.Rodzaj = RodzajEksportu.Kmz Then
            If File.Exists(z.Plik) Then File.Delete(z.Plik)
            If z.PodzialNaTomy Then
                For Each tom In IstniejaceTomyKmz(z.Plik)
                    If File.Exists(tom) Then File.Delete(tom)
                Next
            End If
        ElseIf z.Piramida Then
            UsunPlikBazy(z.Plik)
            UsunPlikBazy(z.Plik & ".tmp")
        ElseIf File.Exists(z.Plik) Then
            File.Delete(z.Plik)
        End If
    End Sub

    ''' <summary>Usuwa plik bazy SQLite wraz z plikami dziennika (-wal, -shm, -journal).</summary>
    Public Shared Sub UsunPlikBazy(plik As String)
        For Each p In {plik, plik & "-wal", plik & "-shm", plik & "-journal"}
            If File.Exists(p) Then File.Delete(p)
        Next
    End Sub

    ''' <summary>
    ''' Parametry połączenia z bazą SQLite. Ścieżka przekazywana przez SQLiteConnectionStringBuilder - wklejona wprost
    ''' do tekstu połączenia ścieżka ze średnikiem (dozwolonym w nazwach folderów Windows) psuła eksport.
    ''' </summary>
    ''' <param name="trybDziennika">Tryb dziennika bazy, np. "Wal" albo "Memory".</param>
    Friend Shared Function ParametryPolaczenia(plik As String, trybDziennika As String) As String
        Dim b As New SQLiteConnectionStringBuilder With {.DataSource = plik, .Version = 3,
            .JournalMode = DirectCast([Enum].Parse(GetType(SQLiteJournalModeEnum), trybDziennika, True), SQLiteJournalModeEnum),
            .SyncMode = SynchronizationModes.Off}
        Return b.ToString()
    End Function

#Region "Renderowanie"

    ''' <summary>
    ''' Renderuje kafel: dla każdego piksela (px, py) funkcja przelicz zwraca punkt w WGS84, przeliczany dalej do układu
    ''' każdej z warstw. Warstwy nakładane są kolejno z uwzględnieniem ich krycia i przezroczystości pikseli.
    ''' Gdy kafel ma mniejszą rozdzielczość niż źródło - nadpróbkowanie (do 4 x 4 próbek na piksel).
    ''' Zwraca bitmapę 32-bitową (piksele poza obrazem przezroczyste) albo Nothing, gdy kafel jest pusty.
    ''' </summary>
    Friend Shared Function RenderujKafelZKorekta(warstwy As List(Of WarstwaEksportu), szer As Integer, wys As Integer, nadprobkowanie As Integer,
                                                  przelicz As Func(Of Double, Double, PunktGeo), korekta As OpcjeKorekcjiMip) As Bitmap
        If korekta Is Nothing OrElse Not korekta.Wlaczona Then Return RenderujKafel(warstwy, szer, wys, nadprobkowanie, przelicz)
        If Not korekta.Wyostrza Then
            Dim bmp = RenderujKafel(warstwy, szer, wys, nadprobkowanie, przelicz)
            If bmp IsNot Nothing Then NakladanieObrazow.ZastosujKorekcjeMip(bmp, korekta)
            Return bmp
        End If
        'margines 1 px: wyostrzanie na brzegu kafla korzysta z prawdziwych sąsiadów - dawniej każdy kafel miał
        'na brzegach linie mniejszej ostrości (siatka co 256 px widoczna na mapie)
        Dim szerM As Integer = szer + 2, wysM As Integer = wys + 2
        Using duzy = RenderujKafel(warstwy, szerM, wysM, nadprobkowanie, Function(x, y) przelicz(x - 1, y - 1))
            If duzy Is Nothing Then Return Nothing
            Dim piksele = NakladanieObrazow.PikseleBitmapy(duzy)
            KorekcjaMip.Zastosuj(piksele, szerM, wysM, korekta)
            Dim srodek = KorekcjaMip.Wytnij(piksele, szerM, wysM, 1)
            Return If(srodek Is Nothing, Nothing, NakladanieObrazow.DoBitmapy(srodek, szer, wys))
        End Using
    End Function

    Friend Shared Function RenderujKafel(warstwy As List(Of WarstwaEksportu), szer As Integer, wys As Integer, nadprobkowanie As Integer,
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
                    If sa = 0 AndAlso zrodlo.PrzytnijDoObszaru Then
                        ' Przy bardzo małym obszarze wszystkie regularne próbki piksela mogą wypaść poza mapą.
                        Dim lewyGorny = siatka.Punkt(px, py)
                        Dim prawyDolny = siatka.Punkt(px + 1, py + 1)
                        Dim obszar = zrodlo.ZasiegMapy
                        Dim wysokosc = Math.Abs(lewyGorny.X - prawyDolny.X)
                        Dim szerokosc = Math.Abs(lewyGorny.Y - prawyDolny.Y)
                        If wysokosc > obszar.Wysokosc OrElse szerokosc > obszar.Szerokosc Then
                            Dim x0 = Math.Max(obszar.XDol, Math.Min(lewyGorny.X, prawyDolny.X))
                            Dim x1 = Math.Min(obszar.XGora, Math.Max(lewyGorny.X, prawyDolny.X))
                            Dim y0 = Math.Max(obszar.YLewy, Math.Min(lewyGorny.Y, prawyDolny.Y))
                            Dim y1 = Math.Min(obszar.YPrawy, Math.Max(lewyGorny.Y, prawyDolny.Y))
                            If x1 > x0 AndAlso y1 > y0 AndAlso zrodlo.Probka((x0 + x1) / 2, (y0 + y1) / 2, sr, sg, sb, sa) Then
                                Dim udzial = n * n * Math.Max(1.0 / 255, (x1 - x0) / wysokosc * ((y1 - y0) / szerokosc))
                                sr *= udzial : sg *= udzial : sb *= udzial : sa *= udzial
                            End If
                        End If
                    End If
                    If sa <= 0 Then Continue For
                    Dim k As Double = aktywne(iw).Krycie / (n * n * 255.0)
                    sr *= k : sg *= k : sb *= k : sa *= k
                    'nałożenie warstwy na dotychczasowy obraz z uwzględnieniem trybu mieszania
                    If wa > 0.001 Then
                        Dim br As Double = wr / wa, bg As Double = wg / wa, bb As Double = wb / wa
                        Dim lr As Double = sr / sa, lg As Double = sg / sa, lb As Double = sb / sa
                        Dim mr As Double, mg As Double, mb As Double

                        Select Case aktywne(iw).TrybMieszania
                            Case TrybMieszania.Mnozenie
                                ' 2 Mnożenie (Multiply): Cb * Cs
                                mr = br * lr
                                mg = bg * lg
                                mb = bb * lb
                            Case TrybMieszania.Ciemniej
                                ' 3 Ciemniej (Darken): min(Cb, Cs)
                                mr = Math.Min(br, lr)
                                mg = Math.Min(bg, lg)
                                mb = Math.Min(bb, lb)
                            Case TrybMieszania.Jasniej
                                ' 4 Jaśniej (Lighten): max(Cb, Cs)
                                mr = Math.Max(br, lr)
                                mg = Math.Max(bg, lg)
                                mb = Math.Max(bb, lb)
                            Case TrybMieszania.Ekran
                                ' 5 Ekran (Screen): 1 - (1-Cb)*(1-Cs)
                                mr = 1.0 - (1.0 - br) * (1.0 - lr)
                                mg = 1.0 - (1.0 - bg) * (1.0 - lg)
                                mb = 1.0 - (1.0 - bb) * (1.0 - lb)
                            Case TrybMieszania.Nakladka
                                ' 6 Nakładka (Overlay): połączenie mnożenia i ekranu
                                mr = If(br < 0.5, 2.0 * br * lr, 1.0 - 2.0 * (1.0 - br) * (1.0 - lr))
                                mg = If(bg < 0.5, 2.0 * bg * lg, 1.0 - 2.0 * (1.0 - bg) * (1.0 - lg))
                                mb = If(bb < 0.5, 2.0 * bb * lb, 1.0 - 2.0 * (1.0 - bb) * (1.0 - lb))
                            Case TrybMieszania.RzezbaTerenu
                                ' 7 Rzeźba terenu: płaski teren (jasność 180) bez zmian, stoki w cieniu ciemniejsze
                                Dim f = TrybMieszaniaRozszerzenia.WspolczynnikRzezby((lr + lg + lb) / 3.0)
                                mr = Math.Min(1.0, br * f)
                                mg = Math.Min(1.0, bg * f)
                                mb = Math.Min(1.0, bb * f)
                            Case Else
                                ' 1 Zwykły (Normal): Cs
                                mr = lr
                                mg = lg
                                mb = lb
                        End Select

                        Dim nowyWa As Double = sa + wa * (1 - sa)
                        wr = sa * (1 - wa) * lr + sa * wa * mr + (1 - sa) * wr
                        wg = sa * (1 - wa) * lg + sa * wa * mg + (1 - sa) * wg
                        wb = sa * (1 - wa) * lb + sa * wa * mb + (1 - sa) * wb
                        wa = nowyWa
                    Else
                        ' Pierwsza widoczna warstwa (podkład) - zawsze rysowana normalnie
                        wr = sr : wg = sg : wb = sb : wa = sa
                    End If
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
    Public Shared Function Koduj(bmp As Bitmap, png As Boolean, jakosc As Integer) As Byte()
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

    ''' <summary>Koduje bitmapę z określonym kolorem tła dla JPEG.</summary>
    Public Shared Function Koduj(bmp As Bitmap, png As Boolean, jakosc As Integer, kolorTla As Color) As Byte()
        Using ms As New MemoryStream()
            If png Then
                bmp.Save(ms, ImageFormat.Png)
            Else
                Using tlo As New Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb)
                    Using g = Graphics.FromImage(tlo)
                        g.Clear(kolorTla)
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

    ''' <summary>Sprawdza, czy bitmapa 32bpp zawiera przynajmniej jeden piksel z przezroczystością (alfa < 250).</summary>
    Friend Shared Function CzyPosiadaPrzezroczystosc(bmp As Bitmap) As Boolean
        If bmp.PixelFormat <> PixelFormat.Format32bppArgb Then Return False
        Dim d = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Try
            Dim scan0 As IntPtr = d.Scan0
            Dim stride As Integer = d.Stride
            Dim w As Integer = bmp.Width
            Dim h As Integer = bmp.Height
            Dim bufor(w * 4 - 1) As Byte
            For y = 0 To h - 1
                Marshal.Copy(IntPtr.Add(scan0, y * stride), bufor, 0, bufor.Length)
                For x = 3 To bufor.Length - 1 Step 4
                    If bufor(x) < 250 Then Return True
                Next
            Next
            Return False
        Finally
            bmp.UnlockBits(d)
        End Try
    End Function

    ''' <summary>Przeliczenie piksela kafla Web Mercator (lewy górny narożnik kafla: kx, ky w pikselach globalnych) do WGS84.</summary>
    Friend Shared Function PrzeliczenieKafla(kx As Double, ky As Double, poziom As Integer) As Func(Of Double, Double, PunktGeo)
        Return Function(x As Double, y As Double) New PunktGeo(WebMercator.Szerokosc(ky + y, poziom), WebMercator.Dlugosc(kx + x, poziom))
    End Function

    ''' <summary>Czy w kafelku Web Mercator jest choć jeden segment którejś z warstw (sprawdzenie bez renderowania).</summary>
    Friend Shared Function KafelMaDane(warstwy As List(Of WarstwaEksportu), tx As Integer, ty As Integer, poziom As Integer) As Boolean
        Dim bok As Double = WebMercator.RozmiarKafla
        Dim narozniki() As PunktGeo = {
            New PunktGeo(WebMercator.Szerokosc(ty * bok, poziom), WebMercator.Dlugosc(tx * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc(ty * bok, poziom), WebMercator.Dlugosc((tx + 1) * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc((ty + 1) * bok, poziom), WebMercator.Dlugosc(tx * bok, poziom)),
            New PunktGeo(WebMercator.Szerokosc((ty + 1) * bok, poziom), WebMercator.Dlugosc((tx + 1) * bok, poziom))}
        For Each w In warstwy
            If w.Zrodlo IsNot Nothing AndAlso w.Uklad IsNot Nothing AndAlso w.Zrodlo.MaDane(ObszarPunktow(w.Uklad, narozniki)) Then Return True
        Next
        Return False
    End Function

    ''' <summary>Prostokąt w układzie obejmujący punkty WGS84.</summary>
    Public Shared Function ObszarPunktow(uklad As UkladWspolrzednych, punkty As IEnumerable(Of PunktGeo)) As Zasieg
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

Public Shared Function NazwaPlikuTomu(sciezkaBazowa As String, numerTomu As Integer, liczbaTomow As Integer) As String
        Dim dir = Path.GetDirectoryName(sciezkaBazowa)
        Dim nazwa = Path.GetFileNameWithoutExtension(sciezkaBazowa)
        Dim ext = Path.GetExtension(sciezkaBazowa)
        Dim sufiks = "_tom" & numerTomu.ToString("D2") & "_z_" & liczbaTomow.ToString("D2")
        Return Path.Combine(If(String.IsNullOrEmpty(dir), ".", dir), nazwa & sufiks & ext)
    End Function

    Private Shared Sub EksportKmz(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Dim plan = PlanKmz(z)
        Dim warstwy = z.WarstwyRenderowania()
        Dim nadprobkowanie As Integer = LiczbaProbek(z, PikselWMetrach(z) * plan.Pomniejszenie)
        Dim png As Boolean = z.Png
        Dim gotowe As Integer = 0
        Dim zapisane As Integer = 0

        Dim filtr As Func(Of ProstokatGeo, Boolean) = Function(zasiegKafla)
                                                          Dim narozniki() As PunktGeo = {
                                                              New PunktGeo(zasiegKafla.Polnoc, zasiegKafla.Zachod),
                                                              New PunktGeo(zasiegKafla.Polnoc, zasiegKafla.Wschod),
                                                              New PunktGeo(zasiegKafla.Poludnie, zasiegKafla.Zachod),
                                                              New PunktGeo(zasiegKafla.Poludnie, zasiegKafla.Wschod)
                                                          }
                                                          For Each w In warstwy
                                                              If w.Zrodlo.MaDane(ObszarPunktow(w.Uklad, narozniki)) Then Return True
                                                          Next
                                                          Return False
                                                      End Function

        Dim tomy = If(z.PodzialNaTomy, plan.WyznaczTomy(If(z.MaksKafli > 0, z.MaksKafli, 100), filtr), Nothing)
        If tomy IsNot Nothing AndAlso tomy.Count > 1 Then
            Dim lacznie = Math.Max(1, tomy.Sum(Function(t) t.LiczbaKafli))
            For Each tom In tomy
                Dim plikTomu = NazwaPlikuTomu(z.Plik, tom.Numer, tomy.Count)
                Dim nazwaTomu = z.Nazwa & " (Tom " & tom.Numer & " z " & tomy.Count & ")"
                Using kmz As New ZapisKmz(plikTomu, nazwaTomu, z.KolejnoscRysowania)
                    kmz.Opis = z.Metadane.Opis & vbCrLf & "Tom " & tom.Numer & " z " & tomy.Count & " (" & tom.LiczbaKafli & " kafli)"
                    For Each kfl In tom.Kafle
                        token.ThrowIfCancellationRequested()
                        Dim w = kfl.Item1, k = kfl.Item2
                        Dim px = plan.PikseleKafla(w, k)
                        Dim x0 As Integer = px.Item1, y0 As Integer = px.Item2
                        Dim f = Function(x As Double, y As Double) New PunktGeo(plan.Obszar.Polnoc - (y0 + y) * plan.PikselSzerokosc,
                                                                                plan.Obszar.Zachod + (x0 + x) * plan.PikselDlugosc)
                        Using bmp = RenderujKafelZKorekta(warstwy, px.Item3, px.Item4, nadprobkowanie, f, z.KorekcjaMip)
                            If bmp IsNot Nothing Then
                                kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & If(png, ".png", ".jpg"),
                                               Koduj(bmp, png, z.JakoscJpeg), plan.ZasiegKafla(w, k))
                                zapisane += 1
                            End If
                        End Using
                        gotowe += 1
                        postep?.Report(Math.Min(100, gotowe * 100 \ lacznie))
                    Next
                    kmz.Zakoncz()
                End Using
            Next
            Return
        End If

        Dim lacznieStandard As Integer = Math.Max(1, plan.Wiersze * plan.Kolumny)
        Using kmz As New ZapisKmz(z.Plik, z.Nazwa, z.KolejnoscRysowania)
            kmz.Opis = z.Metadane.Opis
            For w = 0 To plan.Wiersze - 1
                For k = 0 To plan.Kolumny - 1
                    token.ThrowIfCancellationRequested()
                    Dim px = plan.PikseleKafla(w, k)
                    Dim x0 As Integer = px.Item1, y0 As Integer = px.Item2
                    Dim f = Function(x As Double, y As Double) New PunktGeo(plan.Obszar.Polnoc - (y0 + y) * plan.PikselSzerokosc,
                                                                            plan.Obszar.Zachod + (x0 + x) * plan.PikselDlugosc)
                    Using bmp = RenderujKafelZKorekta(warstwy, px.Item3, px.Item4, nadprobkowanie, f, z.KorekcjaMip)
                        If bmp IsNot Nothing Then
                            kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & If(png, ".png", ".jpg"),
                                           Koduj(bmp, png, z.JakoscJpeg), plan.ZasiegKafla(w, k))
                            zapisane += 1
                        End If
                    End Using
                    gotowe += 1
                    postep?.Report(Math.Min(100, gotowe * 100 \ lacznieStandard))
                Next
            Next
            If z.Zrodlo.PrzytnijDoObszaru AndAlso zapisane = 0 Then
                Throw New InvalidOperationException("Brak obrazu mapy w wybranym obszarze.")
            End If
            kmz.Zakoncz()
        End Using
    End Sub

#End Region

#Region "GeoTIFF"

    ''' <summary>
    ''' Nakłada piksel (kolor, krycie efA 0..1) na piksel ARGB operatorem „over” dla alfy niepowiązanej (taką zapisuje
    ''' GeoTIFF RGBA, ExtraSamples = 2). Wcześniej kolor mieszano z tłem bez uwzględnienia jego przezroczystości,
    ''' więc na przezroczystym tle nakładki półprzezroczyste piksele (wygładzone linie) wychodziły ok. 40% ciemniejsze.
    ''' </summary>
    Friend Shared Function NalozPiksel(stary As Integer, r As Double, g As Double, b As Double, efA As Double) As Integer
        If efA <= 0 Then Return stary
        efA = Math.Min(1.0, efA)
        Dim staryA = ((stary >> 24) And &HFF) / 255.0
        Dim staryR = (stary >> 16) And &HFF
        Dim staryG = (stary >> 8) And &HFF
        Dim staryB = stary And &HFF
        Dim wagaTla = staryA * (1.0 - efA)
        Dim nowyA = efA + wagaTla
        If nowyA <= 0 Then Return stary
        Dim nowyR = CInt(Math.Min(255, Math.Max(0, Math.Round((r * efA + staryR * wagaTla) / nowyA))))
        Dim nowyG = CInt(Math.Min(255, Math.Max(0, Math.Round((g * efA + staryG * wagaTla) / nowyA))))
        Dim nowyB = CInt(Math.Min(255, Math.Max(0, Math.Round((b * efA + staryB * wagaTla) / nowyA))))
        Dim a = CInt(Math.Min(255, Math.Round(nowyA * 255.0)))
        Return (a << 24) Or (nowyR << 16) Or (nowyG << 8) Or nowyB
    End Function

    Friend Shared Function NalozPikselZTrybem(stary As Integer, r As Double, g As Double, b As Double, efA As Double, tryb As TrybMieszania) As Integer
        If efA <= 0 Then Return stary
        Dim da = ((stary >> 24) And &HFF) / 255.0
        If da <= 0.001 OrElse tryb = TrybMieszania.Zwykly Then Return NalozPiksel(stary, r, g, b, efA)

        Dim sa = Math.Min(1.0, efA)
        Dim br = ((stary >> 16) And &HFF) / 255.0
        Dim bg = ((stary >> 8) And &HFF) / 255.0
        Dim bb = (stary And &HFF) / 255.0
        Dim lr = Math.Max(0.0, Math.Min(1.0, r / 255.0))
        Dim lg = Math.Max(0.0, Math.Min(1.0, g / 255.0))
        Dim lb = Math.Max(0.0, Math.Min(1.0, b / 255.0))

        Dim mr As Double, mg As Double, mb As Double
        Select Case tryb
            Case TrybMieszania.Mnozenie
                mr = br * lr : mg = bg * lg : mb = bb * lb
            Case TrybMieszania.Ciemniej
                mr = Math.Min(br, lr) : mg = Math.Min(bg, lg) : mb = Math.Min(bb, lb)
            Case TrybMieszania.Jasniej
                mr = Math.Max(br, lr) : mg = Math.Max(bg, lg) : mb = Math.Max(bb, lb)
            Case TrybMieszania.Ekran
                mr = 1.0 - (1.0 - br) * (1.0 - lr)
                mg = 1.0 - (1.0 - bg) * (1.0 - lg)
                mb = 1.0 - (1.0 - bb) * (1.0 - lb)
            Case TrybMieszania.Nakladka
                mr = If(br < 0.5, 2.0 * br * lr, 1.0 - 2.0 * (1.0 - br) * (1.0 - lr))
                mg = If(bg < 0.5, 2.0 * bg * lg, 1.0 - 2.0 * (1.0 - bg) * (1.0 - lg))
                mb = If(bb < 0.5, 2.0 * bb * lb, 1.0 - 2.0 * (1.0 - bb) * (1.0 - lb))
            Case TrybMieszania.RzezbaTerenu
                Dim f = TrybMieszaniaRozszerzenia.WspolczynnikRzezby((lr + lg + lb) / 3.0)
                mr = Math.Min(1.0, br * f)
                mg = Math.Min(1.0, bg * f)
                mb = Math.Min(1.0, bb * f)
            Case Else
                mr = lr : mg = lg : mb = lb
        End Select

        Dim oa As Double = sa + da * (1.0 - sa)
        Dim wr As Double = sa * (1.0 - da) * lr + sa * da * mr + (1.0 - sa) * da * br
        Dim wg As Double = sa * (1.0 - da) * lg + sa * da * mg + (1.0 - sa) * da * bg
        Dim wb As Double = sa * (1.0 - da) * lb + sa * da * mb + (1.0 - sa) * da * bb
        Return (CInt(Bajt(oa)) << 24) Or (CInt(Bajt(wr / oa)) << 16) Or (CInt(Bajt(wg / oa)) << 8) Or CInt(Bajt(wb / oa))
    End Function

    ''' <summary>
    ''' Czy piksele warstwy można kopiować 1:1 do arkusza: ten sam układ, bok segmentu, liczba wierszy i kolumn, a także
    ''' ten sam narożnik siatki i rozmiar piksela. Dawniej porównywano tylko wymiary, więc nakładka o tych samych wymiarach,
    ''' ale z siatką przesuniętą (np. uchwytem) trafiała do GeoTIFF w położenie mapy podstawowej.
    ''' </summary>
    Friend Shared Function TaSamaSiatkaPikseli(a As Siatka, ukladA As UkladWspolrzednych, b As Siatka, ukladB As UkladWspolrzednych) As Boolean
        If ukladA.Epsg <> ukladB.Epsg OrElse a.BokSegmentuPx <> b.BokSegmentuPx OrElse
           a.LiczbaKolumn <> b.LiczbaKolumn OrElse a.LiczbaWierszy <> b.LiczbaWierszy Then Return False
        'tolerancja: setna część piksela
        Dim tol As Double = b.RozmiarPiksela * 0.01
        Dim za = a.ZasiegSiatki, zb = b.ZasiegSiatki
        Return Math.Abs(a.RozmiarPiksela - b.RozmiarPiksela) <= tol / Math.Max(1, b.SzerokoscPx) AndAlso
               Math.Abs(za.XGora - zb.XGora) <= tol AndAlso Math.Abs(za.YLewy - zb.YLewy) <= tol
    End Function

    ''' <summary>Największy rozmiar pasa wierszy eksportu GeoTIFF w pamięci (bajty; testy zmniejszają go, by sprawdzić zapis pasami).</summary>
    Friend Shared Property BudzetPamieciGeoTiff As Long = 256L * 1024 * 1024

    ''' <summary>Folder roboczy mapy TrekBuddy (obok archiwum .tar, o tej samej nazwie; usuwany po spakowaniu).</summary>
    Friend Shared Function FolderRoboczyTrekBuddy(z As ZadanieEksportu) As String
        Return Path.Combine(Path.GetDirectoryName(z.Plik), Path.GetFileNameWithoutExtension(z.Plik))
    End Function

    ''' <summary>
    ''' Arkusz (GeoTIFF, JPEG, PNG) albo mapa TrekBuddy: mapa podstawowa z nakładkami renderowana pasami wierszy w układzie
    ''' sesji. TrekBuddy tnie te same pasy na kafle 512 px (ZapisKafliTrekBuddy).
    ''' </summary>
    Private Shared Sub EksportGeoTiff(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Dim s = z.Zrodlo.Siatka
        If s Is Nothing OrElse Not s.Poprawna OrElse s.LiczbaKolumn = 0 OrElse s.LiczbaWierszy = 0 Then
            Throw New ArgumentException("Niepoprawne parametry siatki segmentów")
        End If
        Dim bok As Integer = s.BokSegmentuPx
        Dim kolumny As Integer = s.LiczbaKolumn
        Dim zasieg = z.Zrodlo.ZasiegMapy
        Dim przycinany = z.Zrodlo.PrzytnijDoObszaru
        Dim szerPx = If(przycinany, CLng(Math.Ceiling(zasieg.Szerokosc / s.RozmiarPiksela)), s.SzerokoscPx)
        Dim wysPx = If(przycinany, CLng(Math.Ceiling(zasieg.Wysokosc / s.RozmiarPiksela)), s.WysokoscPx)
        Dim trekBuddy As Boolean = z.Rodzaj = RodzajEksportu.TrekBuddy
        Dim format As FormatArkusza = If(trekBuddy, FormatArkusza.Jpeg, z.FormatArkusza)
        'kanał alfa tylko w formatach, które go zapisują (GeoTIFF bezstratny, PNG); JPEG - białe tło
        Dim kanaly As Integer = If(ZapisRastra.ObslugujePrzezroczystosc(format) AndAlso (z.Nakladka OrElse z.KaflePng), 4, 3)
        'rozmiar sprawdzany przed alokacją buforów (dawniej bardzo szeroka mapa kończyła się OverflowException)
        If szerPx > Integer.MaxValue \ 4 OrElse wysPx > Integer.MaxValue Then
            Throw New ArgumentException("Arkusz " & szerPx & " x " & wysPx & " pikseli jest zbyt duży do zapisu w jednym pliku GeoTIFF")
        End If
        Dim szer As Integer = CInt(szerPx)
        Dim wys As Integer = CInt(wysPx)
        Dim wiersze = CInt(Math.Ceiling(wys / CDbl(bok)))
        Dim pikselX = If(przycinany, zasieg.Wysokosc / wys, s.RozmiarPiksela)
        Dim pikselY = If(przycinany, zasieg.Szerokosc / szer, s.RozmiarPiksela)
        Dim maks As Integer = If(trekBuddy, Integer.MaxValue, ZapisRastra.MaksymalnyBok(format))
        If szer > maks OrElse wys > maks Then
            Throw New ArgumentException("Arkusz " & szer & " x " & wys & " pikseli przekracza możliwości formatu (maks. " & maks & " pikseli) - wybierz GeoTIFF lub PNG")
        End If
        z.BrakujaceSegmenty.Clear()

        Dim warstwy = z.WarstwyRenderowania()
        'pas wierszy mieszczący się w budżecie pamięci - dawniej cały wiersz segmentów naraz (przy szerokości
        '100 000 px i boku 2000 px ok. 1,4 GB)
        'korekta obrazu pasami z sąsiednimi wierszami (bez linii mniejszej ostrości na stykach pasów) - trzy dodatkowe
        'bufory pasa: pas oczekujący, roboczy z wierszami sąsiadów i wynik
        Dim korekta As KorekcjaPasow = If(z.KorekcjaMip IsNot Nothing AndAlso z.KorekcjaMip.Wlaczona, New KorekcjaPasow(szer, kanaly, z.KorekcjaMip), Nothing)
        Dim pas As Integer = CInt(Math.Max(1L, Math.Min(bok, BudzetPamieciGeoTiff \ (CLng(szer) * (4 + kanaly + If(korekta IsNot Nothing, 3 * kanaly, 0))))))
        Dim zapiszPas As Action(Of Byte(), Integer) = Nothing
        Dim buforRgb(pas * szer * kanaly - 1) As Byte
        Dim pikseleArgb(pas * szer - 1) As Integer
        Dim taSamaSiatka As New List(Of Boolean)
        For Each warstwa In warstwy
            taSamaSiatka.Add(Not przycinany AndAlso Not warstwa.Zrodlo.PrzytnijDoObszaru AndAlso Not warstwa.Zrodlo.MaMaske AndAlso
                             TaSamaSiatkaPikseli(warstwa.Zrodlo.Siatka, warstwa.Uklad, s, z.Uklad))
        Next

        'nakładki wydruku: plik śladu czytany raz przed pętlą (dawniej osobno dla każdego pasa arkusza)
        Dim arkuszZakres As New ZakresArkusza With {
            .SzerokoscPx = szer, .WysokoscPx = wys,
            .LewyEastingM = zasieg.YLewy, .GornyNorthingM = zasieg.XGora,
            .RozmiarPikselaXM = pikselY, .RozmiarPikselaYM = pikselX}
        Dim segmentySladu As List(Of List(Of (Lon As Double, Lat As Double))) = Nothing
        If Not String.IsNullOrEmpty(z.PlikSladuWydruku) Then
            segmentySladu = RendererNakladekWydruku.WczytajSegmentySladu(z.PlikSladuWydruku)
        End If

        Dim transformacjaSiatki As Func(Of Double, Double, (E As Double, N As Double)) = Nothing
        Dim zakresMetrowySiatki As Tuple(Of Double, Double, Double, Double) = Nothing
        If z.SiatkaKilometrowa IsNot Nothing Then
            Dim ukladSiatkiDocelowy As UkladWspolrzednych
            Select Case z.SiatkaKilometrowa.UkladSiatki
                Case UkladSiatkiWydruku.Utm33N : ukladSiatkiDocelowy = UkladWspolrzednych.Utm33N
                Case UkladSiatkiWydruku.Utm34N : ukladSiatkiDocelowy = UkladWspolrzednych.Utm34N
                Case UkladSiatkiWydruku.Utm35N : ukladSiatkiDocelowy = UkladWspolrzednych.Utm35N
                Case UkladSiatkiWydruku.Pl2000Strefa5 : ukladSiatkiDocelowy = UkladWspolrzednych.PL2000Strefa5
                Case UkladSiatkiWydruku.Pl2000Strefa6 : ukladSiatkiDocelowy = UkladWspolrzednych.PL2000Strefa6
                Case UkladSiatkiWydruku.Pl2000Strefa7 : ukladSiatkiDocelowy = UkladWspolrzednych.PL2000Strefa7
                Case UkladSiatkiWydruku.Pl2000Strefa8 : ukladSiatkiDocelowy = UkladWspolrzednych.PL2000Strefa8
                Case Else : ukladSiatkiDocelowy = UkladWspolrzednych.PL1992
            End Select

            If z.Uklad.Geograficzny OrElse z.Uklad.Epsg <> ukladSiatkiDocelowy.Epsg Then
                Dim uSiatki = ukladSiatkiDocelowy
                transformacjaSiatki = Function(easting, northing)
                                          Dim geo = uSiatki.DoWgs84(northing, easting)
                                          Dim ark = z.Uklad.ZWgs84(geo)
                                          Return (E:=ark.Y, N:=ark.X)
                                      End Function

                Dim naroznikiArk = {
                    New PunktXY(zasieg.XGora, zasieg.YLewy),
                    New PunktXY(zasieg.XGora, zasieg.YPrawy),
                    New PunktXY(zasieg.XDol, zasieg.YPrawy),
                    New PunktXY(zasieg.XDol, zasieg.YLewy),
                    New PunktXY((zasieg.XGora + zasieg.XDol) / 2.0, zasieg.YLewy),
                    New PunktXY((zasieg.XGora + zasieg.XDol) / 2.0, zasieg.YPrawy)
                }
                Dim minE As Double = Double.MaxValue, maxE As Double = Double.MinValue
                Dim minN As Double = Double.MaxValue, maxN As Double = Double.MinValue
                For Each nArk In naroznikiArk
                    Dim geo = z.Uklad.DoWgs84(nArk)
                    Dim wUk = uSiatki.ZWgs84(geo)
                    If wUk.Y < minE Then minE = wUk.Y
                    If wUk.Y > maxE Then maxE = wUk.Y
                    If wUk.X < minN Then minN = wUk.X
                    If wUk.X > maxN Then maxN = wUk.X
                Next
                Dim krok As Integer = If(z.SiatkaKilometrowa.KrokMetry > 0, z.SiatkaKilometrowa.KrokMetry, 1000)
                zakresMetrowySiatki = Tuple.Create(minE - krok, maxE + krok, minN - krok, maxN + krok)
            End If
        End If

        Dim plikTymczasowy As String = z.Plik & ".tmp"
        Dim folderTb As String = If(trekBuddy, FolderRoboczyTrekBuddy(z), "")
        Dim nazwaTb As String = Path.GetFileNameWithoutExtension(z.Plik)
        If trekBuddy AndAlso Directory.Exists(folderTb) Then Directory.Delete(folderTb, True)
        Dim zapisTb As ZapisRastra = Nothing
        Dim saPiksele = False
        Using zapis As ZapisRastra = If(trekBuddy, New ZapisKafliTrekBuddy(folderTb, nazwaTb, szer, wys, z.JakoscJpeg),
                                        ZapisRastra.Utworz(format, plikTymczasowy, szer, wys, z.JakoscJpeg, z.Uklad, zasieg, kanaly = 4, z.PiramidyGeoTiff))
            zapisTb = zapis
            Dim maNakladkiWydruku As Boolean = z.SiatkaKilometrowa IsNot Nothing OrElse segmentySladu IsNot Nothing
            Dim wierszDoNalozenia As Integer = 0
            zapiszPas = Sub(b, k)
                            If korekta IsNot Nothing AndAlso maNakladkiWydruku AndAlso k > 0 Then
                                Using bmpPas As New Bitmap(szer, k, Imaging.PixelFormat.Format32bppArgb)
                                    Dim bd = bmpPas.LockBits(New Rectangle(0, 0, szer, k), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppArgb)
                                    Try
                                        Dim wierszBgra(szer * 4 - 1) As Byte
                                        For yy = 0 To k - 1
                                            Dim off = yy * szer * kanaly
                                            For xx = 0 To szer - 1
                                                Dim di = xx * 4
                                                Dim si = off + xx * kanaly
                                                wierszBgra(di) = b(si + 2)
                                                wierszBgra(di + 1) = b(si + 1)
                                                wierszBgra(di + 2) = b(si)
                                                wierszBgra(di + 3) = If(kanaly = 4, b(si + 3), CByte(255))
                                            Next
                                            Marshal.Copy(wierszBgra, 0, IntPtr.Add(bd.Scan0, yy * bd.Stride), wierszBgra.Length)
                                        Next
                                    Finally
                                        bmpPas.UnlockBits(bd)
                                    End Try
                                    Using g = Graphics.FromImage(bmpPas)
                                        If z.SiatkaKilometrowa IsNot Nothing Then
                                            RendererNakladekWydruku.RysujSiatkeKilometrowa(g, arkuszZakres, z.SiatkaKilometrowa, wierszDoNalozenia, transformacjaSiatki, zakresMetrowySiatki)
                                        End If
                                        If segmentySladu IsNot Nothing Then
                                            Dim segs = segmentySladu
                                            Dim przelicz As Func(Of Double, Double, PointF) =
                                                Function(lon, lat)
                                                    Dim xy = z.Uklad.ZWgs84(New PunktGeo(lat, lon))
                                                    Return arkuszZakres.MetryNaPiksel(xy.Y, xy.X)
                                                End Function
                                            Dim styl = If(z.StylSladuWydruku, New StylSladuWydruku())
                                            RendererNakladekWydruku.RysujSladNaArkuszu(g, segs, przelicz, styl, wierszDoNalozenia)
                                        End If
                                    End Using
                                    Dim bdRead = bmpPas.LockBits(New Rectangle(0, 0, szer, k), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
                                    Try
                                        Dim wierszBgra(szer * 4 - 1) As Byte
                                        For yy = 0 To k - 1
                                            Marshal.Copy(IntPtr.Add(bdRead.Scan0, yy * bdRead.Stride), wierszBgra, 0, wierszBgra.Length)
                                            Dim off = yy * szer * kanaly
                                            For xx = 0 To szer - 1
                                                Dim di = xx * 4
                                                Dim si = off + xx * kanaly
                                                b(si) = wierszBgra(di + 2)
                                                b(si + 1) = wierszBgra(di + 1)
                                                b(si + 2) = wierszBgra(di)
                                                If kanaly = 4 Then b(si + 3) = wierszBgra(di + 3)
                                            Next
                                        Next
                                    Finally
                                        bmpPas.UnlockBits(bdRead)
                                    End Try
                                End Using
                                wierszDoNalozenia += k
                            End If
                            zapis.ZapiszWiersze(b, k)
                        End Sub
            'piramidy GeoTIFF dopisywane w Zakoncz dostają ostatnie 10% paska postępu i reagują na Przerwij
            Dim zapisGeoTiff = TryCast(zapis, ZapisGeoTiff)
            Dim udzialPiramid As Integer = If(zapisGeoTiff IsNot Nothing AndAlso zapisGeoTiff.MaPiramidy, 10, 0)
            If udzialPiramid > 0 Then
                zapisGeoTiff.Przerwanie = token
                zapisGeoTiff.PostepPiramid = Sub(p) postep?.Report(100 - udzialPiramid + p * udzialPiramid \ 100)
            End If
            For w = 1 To wiersze
              Dim wierszeWPaskuSegmentow = Math.Min(bok, wys - (w - 1) * bok)
              For y0 = 0 To wierszeWPaskuSegmentow - 1 Step pas
                token.ThrowIfCancellationRequested()
                Dim n As Integer = Math.Min(pas, wierszeWPaskuSegmentow - y0)
                'Domyślne tło: białe dla RGB (-1 = 0xFFFFFFFF), przezroczyste dla RGBA (0)
                Dim domyslnyKolor As Integer = If(kanaly = 4, 0, -1)
                Array.Fill(pikseleArgb, domyslnyKolor, 0, n * szer)

                For iw = 0 To warstwy.Count - 1
                    Dim warstwa = warstwy(iw)
                    Dim zr = warstwa.Zrodlo
                    Dim krycie = warstwa.Krycie

                    If taSamaSiatka(iw) Then
                        For k = 1 To kolumny
                            Dim dane = zr.Segment(w, k)
                            If dane Is Nothing Then
                                If iw = 0 AndAlso y0 = 0 Then z.BrakujaceSegmenty.Add(zr.NazwaPliku(w, k))
                                Continue For
                            End If
                            Dim x0 As Integer = (k - 1) * bok
                            For y = 0 To n - 1
                                Dim idxCel = y * szer + x0
                                Dim idxZr = (y0 + y) * bok * 4
                                For x = 0 To bok - 1
                                    Dim b = dane(idxZr)
                                    Dim g = dane(idxZr + 1)
                                    Dim r = dane(idxZr + 2)
                                    Dim a = dane(idxZr + 3)
                                    idxZr += 4
                                    If a = 0 Then Continue For
                                    Dim efA As Double = (a / 255.0) * krycie
                                    If efA <= 0 Then Continue For

                                    If iw = 0 AndAlso efA >= 0.999 AndAlso warstwa.TrybMieszania = TrybMieszania.Zwykly Then
                                        pikseleArgb(idxCel + x) = (CInt(a) << 24) Or (CInt(r) << 16) Or (CInt(g) << 8) Or b
                                    Else
                                        pikseleArgb(idxCel + x) = NalozPikselZTrybem(pikseleArgb(idxCel + x), r, g, b, efA, warstwa.TrybMieszania)
                                    End If
                                Next
                            Next
                        Next
                    Else
                        Dim wierszPasaStart As Integer = (w - 1) * bok + y0
                        Dim siatkaPasa As SiatkaPrzeliczen = If(warstwa.Uklad.Epsg <> z.Uklad.Epsg,
                            New SiatkaPrzeliczen(szer, n, 16,
                                Function(px, py) warstwa.Uklad.ZWgs84(z.Uklad.DoWgs84(New PunktXY(zasieg.XGora - (wierszPasaStart + py) * pikselX, zasieg.YLewy + px * pikselY)))),
                            Nothing)
                        For y = 0 To n - 1
                            Dim pyWiersz = wierszPasaStart + y
                            Dim pX = zasieg.XGora - (pyWiersz + 0.5) * pikselX
                            Dim idxCel = y * szer
                            For x = 0 To szer - 1
                                Dim pWarstwy As PunktXY = If(siatkaPasa IsNot Nothing, siatkaPasa.Punkt(x + 0.5, y + 0.5), New PunktXY(pX, zasieg.YLewy + (x + 0.5) * pikselY))
                                Dim r, g, b, a As Double
                                If zr.Probka(pWarstwy.X, pWarstwy.Y, r, g, b, a) AndAlso a > 0 Then
                                    saPiksele = True
                                    Dim skalaA As Double = 255.0 / a
                                    r *= skalaA : g *= skalaA : b *= skalaA
                                    Dim efA As Double = (a / 255.0) * krycie
                                    pikseleArgb(idxCel + x) = NalozPikselZTrybem(pikseleArgb(idxCel + x), r, g, b, efA, warstwa.TrybMieszania)
                                End If
                            Next
                        Next
                    End If
                Next

                If korekta Is Nothing AndAlso maNakladkiWydruku Then
                    Dim offsetPasaY As Integer = (w - 1) * bok + y0
                    Using bmpPas As New Bitmap(szer, n, Imaging.PixelFormat.Format32bppArgb)
                        Dim bd = bmpPas.LockBits(New Rectangle(0, 0, szer, n), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppArgb)
                        System.Runtime.InteropServices.Marshal.Copy(pikseleArgb, 0, bd.Scan0, szer * n)
                        bmpPas.UnlockBits(bd)

                        Using g = Graphics.FromImage(bmpPas)
                            If z.SiatkaKilometrowa IsNot Nothing Then
                                RendererNakladekWydruku.RysujSiatkeKilometrowa(g, arkuszZakres, z.SiatkaKilometrowa, offsetPasaY, transformacjaSiatki, zakresMetrowySiatki)
                            End If
                            If segmentySladu IsNot Nothing Then
                                Dim segs = segmentySladu
                                Dim przelicz As Func(Of Double, Double, PointF) =
                                    Function(lon, lat)
                                        Dim xy = z.Uklad.ZWgs84(New PunktGeo(lat, lon))
                                        Return arkuszZakres.MetryNaPiksel(xy.Y, xy.X)
                                    End Function
                                Dim styl = If(z.StylSladuWydruku, New StylSladuWydruku())
                                RendererNakladekWydruku.RysujSladNaArkuszu(g, segs, przelicz, styl, offsetPasaY)
                            End If
                        End Using

                        Dim bdRead = bmpPas.LockBits(New Rectangle(0, 0, szer, n), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
                        System.Runtime.InteropServices.Marshal.Copy(bdRead.Scan0, pikseleArgb, 0, szer * n)
                        bmpPas.UnlockBits(bdRead)
                    End Using
                End If

                Dim iRgb As Integer = 0
                For iPx = 0 To szer * n - 1
                    Dim c = pikseleArgb(iPx)
                    buforRgb(iRgb) = CByte((c >> 16) And &HFF)
                    buforRgb(iRgb + 1) = CByte((c >> 8) And &HFF)
                    buforRgb(iRgb + 2) = CByte(c And &HFF)
                    If kanaly = 4 Then
                        buforRgb(iRgb + 3) = CByte((c >> 24) And &HFF)
                        iRgb += 4
                    Else
                        iRgb += 3
                    End If
                Next

                If korekta IsNot Nothing Then
                    korekta.Dodaj(buforRgb, n, zapiszPas)
                Else
                    zapis.ZapiszWiersze(buforRgb, n)
                End If
              Next
                postep?.Report(CInt(w * (100 - udzialPiramid) \ wiersze))
            Next
            If przycinany AndAlso Not saPiksele Then Throw New InvalidOperationException("Brak obrazu mapy w wybranym obszarze.")
            korekta?.Zakoncz(zapiszPas)
            zapis.Zakoncz()
        End Using

        If trekBuddy Then
            ZapisKafliTrekBuddy.UtworzPaczke(folderTb, nazwaTb, DirectCast(zapisTb, ZapisKafliTrekBuddy).Pliki, z.Uklad, zasieg, szer, wys, z.Plik)
            Exit Sub
        End If
        File.Move(plikTymczasowy, z.Plik, overwrite:=True)

        'pliki georeferencji obok arkusza (GeoTIFF ma georeferencję także w samym pliku)
        Dim folderZapisu = Path.GetDirectoryName(z.Plik)
        If Not String.IsNullOrEmpty(folderZapisu) AndAlso Not folderZapisu.EndsWith(Path.DirectorySeparatorChar.ToString()) Then
            folderZapisu &= Path.DirectorySeparatorChar
        End If
        Dim nazwaBezRozszerzenia = Path.GetFileNameWithoutExtension(z.Plik)
        Dim rozszerzenie = ZapisRastra.Rozszerzenie(format)
        If z.Georeferencja IsNot Nothing AndAlso z.Georeferencja.Dowolna Then
            Dim obrazGeoref = ZapisGeoreferencji.Obraz(z.Uklad, zasieg, szer, wys, folderZapisu, Path.GetFileName(z.Plik))
            ZapisGeoreferencji.ZapiszPliki(obrazGeoref, folderZapisu, nazwaBezRozszerzenia, rozszerzenie, z.Georeferencja)
            'world file GeoTIFF: programy CAD szukają .tfw (ZapiszPliki tworzy rozszerzenie z pierwszej i ostatniej litery)
            Dim sciezkaTfw = Path.Combine(folderZapisu, nazwaBezRozszerzenia & ".tfw")
            If z.Georeferencja.WorldFile AndAlso rozszerzenie = "tif" AndAlso Not File.Exists(sciezkaTfw) Then
                Georeferencja.Zapisz(sciezkaTfw, Georeferencja.WorldFile(obrazGeoref), KodowanieSystemowe())
            End If
        End If
    End Sub

#End Region

#Region "Kafle Web Mercator (MBTiles, SQLiteDB, OruxMaps)"

    Public Shared Function UtworzMagazyn(z As ZadanieEksportu, obszar As ProstokatGeo) As MagazynKafli
        Select Case z.Rodzaj
            Case RodzajEksportu.SQLiteDB : Return New MagazynSQLiteDB(z)
            Case RodzajEksportu.OruxMaps : Return New MagazynOruxMaps(z, obszar)
            Case RodzajEksportu.Jnx : Return New MagazynJnx(z, obszar)
            Case RodzajEksportu.GeoPackage : Return New MagazynGeoPackage(z, obszar)
            Case Else : Return New MagazynMBTiles(z, obszar)
        End Select
    End Function

    Private Shared Sub EksportPiramidy(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        Dim plikDocelowy As String = z.Plik
        Dim uzyjPlikuTmp As Boolean = (z.Rodzaj <> RodzajEksportu.OruxMaps AndAlso z.Rodzaj <> RodzajEksportu.Jnx)
        Dim plikRoboczy As String = If(uzyjPlikuTmp, plikDocelowy & ".tmp", plikDocelowy)
        If Not uzyjPlikuTmp Then UsunWynik(z)
        Dim obszar = ObszarKafli(z)
        Dim wszystkie As Long = Math.Max(1, LiczbaKafliMBTiles(z))
        Dim gotowe As Long = 0
        Dim zapisane As Long = 0
        Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim po As New ParallelOptions With {.CancellationToken = token, .MaxDegreeOfParallelism = Environment.ProcessorCount}
        Const RozmiarPaczki As Integer = 64

        z.Plik = plikRoboczy
        Try
            Using m = UtworzMagazyn(z, obszar)
                ' Przycięte mapy renderujemy w każdym zoomie, bo pomniejszanie kafli rozmywa granicę maski. Mapa wieloskalowa
                ' (ZrodlaPoziomow) też - pomniejszony kafel szczegółowej skali nie jest mapą skali przeglądowej.
                Dim zmax As Integer = z.PoziomMax
                Dim minBezposredni = If(z.Zrodlo.PrzytnijDoObszaru OrElse z.ZrodlaPoziomow.Count > 0, z.PoziomMin, zmax)

                For poziom = zmax To z.PoziomMin Step -1
                    Dim rp = WebMercator.ZakresKafli(obszar, poziom)
                    Dim warstwy = z.WarstwyRenderowania(poziom)
                    If warstwy Is Nothing Then
                        ' poziom poza planem skal - nie trafia do pliku
                        gotowe += CLng(rp.Item3 - rp.Item1 + 1) * (rp.Item4 - rp.Item2 + 1)
                        Continue For
                    End If
                    Dim nadprobkowanie = LiczbaProbek(warstwy, WebMercator.RozmiarPiksela(srodekSz, poziom))
                    Dim kaflePoziomu As New List(Of KafelXYZ)()
                    For ty = rp.Item2 To rp.Item4
                        For tx = rp.Item1 To rp.Item3
                            kaflePoziomu.Add(New KafelXYZ(tx, ty, poziom))
                        Next
                    Next

                    m.RozpocznijPoziom(poziom)
                    For offset = 0 To kaflePoziomu.Count - 1 Step RozmiarPaczki
                        token.ThrowIfCancellationRequested()
                        Dim ile = Math.Min(RozmiarPaczki, kaflePoziomu.Count - offset)
                        Dim daneKafli(ile - 1)() As Byte
                        Dim biezacyOffset = offset

                        If poziom >= minBezposredni Then
                            Dim lokalneWarstwy = warstwy
                            Dim lokalnyPoziom = poziom
                            Dim lokalneNadpr = nadprobkowanie
                            Dim lokalnaKorekta = z.KorekcjaMip
                            Dim czyPng = z.Png
                            Dim jakosc = z.JakoscJpeg
                            Parallel.For(0, ile, po,
                                Sub(i)
                                    Dim k = kaflePoziomu(biezacyOffset + i)
                                    Using bmp = RenderujKafelZKorekta(lokalneWarstwy, 256, 256, lokalneNadpr, PrzeliczenieKafla(k.X * 256.0, k.Y * 256.0, lokalnyPoziom), lokalnaKorekta)
                                        If bmp IsNot Nothing Then daneKafli(i) = Koduj(bmp, czyPng, jakosc)
                                    End Using
                                End Sub)
                        Else
                            Dim wyostrza As Boolean = z.KorekcjaMip IsNot Nothing AndAlso z.KorekcjaMip.Wyostrza
                            Dim surowce(ile - 1)()() As Byte
                            For i = 0 To ile - 1
                                Dim k = kaflePoziomu(biezacyOffset + i)
                                If wyostrza Then
                                    surowce(i) = DzieciZSasiadami(k, Function(d) m.Odczytaj(d))
                                Else
                                    surowce(i) = {m.Odczytaj(New KafelXYZ(k.X * 2, k.Y * 2, k.Z + 1)),
                                                  m.Odczytaj(New KafelXYZ(k.X * 2 + 1, k.Y * 2, k.Z + 1)),
                                                  m.Odczytaj(New KafelXYZ(k.X * 2, k.Y * 2 + 1, k.Z + 1)),
                                                  m.Odczytaj(New KafelXYZ(k.X * 2 + 1, k.Y * 2 + 1, k.Z + 1))}
                                End If
                            Next
                            Dim lokalnaKorekta = z.KorekcjaMip
                            Dim czyPng = z.Png
                            Dim jakosc = z.JakoscJpeg
                            Parallel.For(0, ile, po,
                                Sub(i)
                                    If wyostrza Then
                                        Using bmp = KafelZDzieciWyostrzony(surowce(i), lokalnaKorekta)
                                            If bmp IsNot Nothing Then daneKafli(i) = Koduj(bmp, czyPng, jakosc)
                                        End Using
                                    Else
                                        daneKafli(i) = KafelZDzieciZBajtow(surowce(i), czyPng, jakosc)
                                    End If
                                End Sub)
                        End If

                        For i = 0 To ile - 1
                            Dim dane = daneKafli(i)
                            If dane IsNot Nothing Then
                                m.Zapisz(kaflePoziomu(biezacyOffset + i), dane)
                                zapisane += 1
                            End If
                        Next
                        gotowe += ile
                        postep?.Report(CInt(Math.Min(100, gotowe * 100 \ wszystkie)))
                    Next
                    m.ZakonczPoziom()
                Next
                If z.Zrodlo.PrzytnijDoObszaru AndAlso zapisane = 0 Then
                    Throw New InvalidOperationException("Brak obrazu mapy w wybranym obszarze.")
                End If
                m.ZakonczZapis()
            End Using
            SQLiteConnection.ClearAllPools()
            If uzyjPlikuTmp Then
                UsunPlikBazy(plikDocelowy)
                File.Move(plikRoboczy, plikDocelowy, overwrite:=True)
            End If
        Catch
            SQLiteConnection.ClearAllPools()
            If uzyjPlikuTmp Then
                Try
                    UsunPlikBazy(plikRoboczy)
                Catch
                End Try
            End If
            Throw
        Finally
            z.Plik = plikDocelowy
        End Try
    End Sub

    Private Shared Sub EksportPrzycietegoJnx(z As ZadanieEksportu, postep As IProgress(Of Integer), token As CancellationToken)
        If z.PoziomMin > z.PoziomMax Then
            Throw New ArgumentException("Nieprawidłowy zakres poziomów powiększenia JNX.")
        End If
        ' JPEG w JNX nie obsługuje przezroczystości. Każdy poziom ma własną siatkę kafli wpisaną w obszar.
        Dim obszar = ProstokatGeo.Wpisany(z.Uklad, z.Zrodlo.ZasiegMapy)
        If Not obszar.Poprawny Then obszar = ProstokatGeo.Opisany(z.Uklad, z.Zrodlo.ZasiegMapy)
        Dim szerokoscGeo = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim poziomy As New List(Of PoziomJnx)
        Dim plany = Enumerable.Range(z.PoziomMin, z.PoziomMax - z.PoziomMin + 1).
            Select(Function(p) New PlanKmz(obszar, WebMercator.RozmiarPiksela(szerokoscGeo, p), 256, 0)).ToList()
        Dim wszystkie = plany.Sum(Function(p) CLng(p.LiczbaKafli))
        Dim gotowe As Long = 0
        For i = 0 To plany.Count - 1
            Dim plan = plany(i)
            Dim zoom = z.PoziomMin + i
            ' Mapa wieloskalowa: każdy poziom rysowany ze źródła swojej skali (np. WIG 1:300 000 przy oddaleniu)
            Dim warstwy = z.WarstwyRenderowania(zoom)
            If warstwy Is Nothing Then
                gotowe += plan.LiczbaKafli
                Continue For
            End If
            Dim poziom As New PoziomJnx(ZapisJnx.SkalaDlaPoziomu(zoom, szerokoscGeo))
            Dim probki = LiczbaProbek(warstwy, WebMercator.RozmiarPiksela(szerokoscGeo, zoom))
            For w = 0 To plan.Wiersze - 1
                For k = 0 To plan.Kolumny - 1
                    token.ThrowIfCancellationRequested()
                    Dim px = plan.PikseleKafla(w, k)
                    Dim zasieg = plan.ZasiegKafla(w, k)
                    Dim przelicz = Function(x As Double, y As Double) New PunktGeo(zasieg.Polnoc - y * plan.PikselSzerokosc,
                                                                                 zasieg.Zachod + x * plan.PikselDlugosc)
                    Using bmp = RenderujKafelZKorekta(warstwy, px.Item3, px.Item4, probki, przelicz, z.KorekcjaMip)
                        If bmp IsNot Nothing Then
                            poziom.Kafle.Add(New KafelJnx(zasieg, Koduj(bmp, False, z.JakoscJpeg), CShort(bmp.Width), CShort(bmp.Height)))
                        End If
                    End Using
                    gotowe += 1
                    postep?.Report(CInt(gotowe * 100 \ wszystkie))
                Next
            Next
            If poziom.Kafle.Count > 0 Then poziomy.Add(poziom)
        Next
        If poziomy.Count = 0 Then
            Throw New InvalidOperationException("Brak kafli do zapisania w formacie JNX.")
        End If
        ZapisJnx.Zapisz(z.Plik, z.Nazwa, z.KolejnoscRysowania, poziomy)
    End Sub

    ''' <summary>Kafel poziomu niższego złożony z czterech kafli poziomu wyższego (pomniejszenie 2x).</summary>
    Private Shared Function KafelZDzieci(m As MagazynKafli, k As KafelXYZ, z As ZadanieEksportu) As Byte()
        If z.KorekcjaMip Is Nothing OrElse Not z.KorekcjaMip.Wyostrza Then Return KafelZDzieci(m, k, z.Png, z.JakoscJpeg)
        Dim dzieci = DzieciZSasiadami(k, Function(d) m.Odczytaj(d))
        Using bmp = KafelZDzieciWyostrzony(dzieci, z.KorekcjaMip)
            Return If(bmp Is Nothing, Nothing, Koduj(bmp, z.Png, z.JakoscJpeg))
        End Using
    End Function

    ''' <summary>
    ''' Kafle poziomu wyższego dla kafla k: 4 x 4 (indeks wiersz * 4 + kolumna) - środkowe 2 x 2 tworzą kafel,
    ''' zewnętrzne to sąsiedzi potrzebni do wyostrzania jego brzegów. Poza zakresem świata - Nothing.
    ''' </summary>
    Friend Shared Function DzieciZSasiadami(k As KafelXYZ, odczytaj As Func(Of KafelXYZ, Byte())) As Byte()()
        Dim dzieci(15)() As Byte
        Dim liczba As Long = 1L << (k.Z + 1)
        For dy = 0 To 3
            For dx = 0 To 3
                Dim x As Integer = k.X * 2 - 1 + dx, y As Integer = k.Y * 2 - 1 + dy
                If x < 0 OrElse y < 0 OrElse x >= liczba OrElse y >= liczba Then Continue For
                dzieci(dy * 4 + dx) = odczytaj(New KafelXYZ(x, y, k.Z + 1))
            Next
        Next
        Return dzieci
    End Function

    ''' <summary>
    ''' Kafel poziomu niższego z 4 x 4 kafli poziomu wyższego (DzieciZSasiadami), wyostrzony po pomniejszeniu.
    ''' Barwy kafli wyższego poziomu są już poprawione, więc tu działa tylko wyostrzanie - pomniejszenie 2x wygładza
    ''' krawędzie i dawniej niższe poziomy (widok po oddaleniu na urządzeniu) traciły wyostrzenie. Sąsiedzi z obu
    ''' stron sprawiają, że brzegi kafla są wyostrzone tak samo jak środek. Nothing, gdy środkowe kafle są puste.
    ''' </summary>
    Friend Shared Function KafelZDzieciWyostrzony(dzieci As Byte()(), korekta As OpcjeKorekcjiMip) As Bitmap
        If dzieci(5) Is Nothing AndAlso dzieci(6) Is Nothing AndAlso dzieci(9) Is Nothing AndAlso dzieci(10) Is Nothing Then Return Nothing
        Dim piksele() As Integer
        Using duzy As New Bitmap(1024, 1024, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(duzy)
                g.Clear(Color.Transparent)
                For i = 0 To 15
                    If dzieci(i) Is Nothing Then Continue For
                    Using ms As New MemoryStream(dzieci(i)), obraz = Image.FromStream(ms)
                        g.DrawImage(obraz, New Rectangle((i Mod 4) * 256, (i \ 4) * 256, 256, 256))
                    End Using
                Next
            End Using
            Using sredni As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
                Using g = Graphics.FromImage(sredni)
                    g.Clear(Color.Transparent)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    Using attr As New ImageAttributes()
                        attr.SetWrapMode(WrapMode.TileFlipXY)
                        g.DrawImage(duzy, New Rectangle(0, 0, 512, 512), 0, 0, 1024, 1024, GraphicsUnit.Pixel, attr)
                    End Using
                End Using
                piksele = NakladanieObrazow.PikseleBitmapy(sredni)
            End Using
        End Using
        'kafel to środek 256 x 256 (od piksela 128); wyostrzany fragment z marginesem 1 px
        Dim fragment = KorekcjaMip.Wytnij(piksele, 512, 512, 127)
        If fragment Is Nothing Then Return Nothing
        KorekcjaMip.Zastosuj(fragment, 258, 258, korekta.TylkoWyostrzanie())
        Dim srodek = KorekcjaMip.Wytnij(fragment, 258, 258, 1)
        Return If(srodek Is Nothing, Nothing, NakladanieObrazow.DoBitmapy(srodek, 256, 256))
    End Function

    Friend Shared Function KafelZDzieci(m As MagazynKafli, k As KafelXYZ, png As Boolean, jakoscJpeg As Integer) As Byte()
        Dim dzieci(3)() As Byte
        For dy = 0 To 1
            For dx = 0 To 1
                dzieci(dy * 2 + dx) = m.Odczytaj(New KafelXYZ(k.X * 2 + dx, k.Y * 2 + dy, k.Z + 1))
            Next
        Next
        Return KafelZDzieciZBajtow(dzieci, png, jakoscJpeg)
    End Function

    Friend Shared Function KafelZDzieciZBajtow(dzieci As Byte()(), png As Boolean, jakoscJpeg As Integer) As Byte()
        If dzieci Is Nothing OrElse dzieci.Length < 4 Then Return Nothing
        Using duzy As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
            Dim jest As Boolean = False
            Using g = Graphics.FromImage(duzy)
                g.Clear(Color.Transparent)
                For dy = 0 To 1
                    For dx = 0 To 1
                        Dim dane = dzieci(dy * 2 + dx)
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
                    Using attr As New ImageAttributes()
                        attr.SetWrapMode(WrapMode.TileFlipXY)
                        g.DrawImage(duzy, New Rectangle(0, 0, 256, 256), 0, 0, 512, 512, GraphicsUnit.Pixel, attr)
                    End Using
                End Using
                Return Koduj(maly, png, jakoscJpeg)
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
                Using bmp = RenderujKafelZKorekta(warstwy, px.Item3, px.Item4, nadprobkowanie, f, z.KorekcjaMip)
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
            Using bmp = RenderujKafelZKorekta(warstwy, 256, 256, nadpr, PrzeliczenieKafla(t.Item1 * 256.0, t.Item2 * 256.0, gora), z.KorekcjaMip)
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
Public MustInherit Class MagazynKafli
    Implements IDisposable

    Protected ReadOnly Polaczenie As SQLiteConnection
    Private _transakcja As SQLiteTransaction
    Private _zapis As SQLiteCommand
    Private _odczyt As SQLiteCommand
    Private _pZ, _pX, _pY, _pD, _oZ, _oX, _oY As SQLiteParameter
    Protected ReadOnly Zadanie As ZadanieEksportu

    Protected Sub New(plikBazy As String, z As ZadanieEksportu)
        Zadanie = z
        'także pliki dziennika (-wal, -shm) po przerwanym eksporcie - stary dziennik mógłby uszkodzić nową bazę
        EksportMapy.UsunPlikBazy(plikBazy)
        Polaczenie = New SQLiteConnection(EksportMapy.ParametryPolaczenia(plikBazy, "Wal"))
        Polaczenie.Open()
        Try
            Wykonaj("PRAGMA synchronous = OFF;")
            Wykonaj("PRAGMA cache_size = -64000;")
            Wykonaj("PRAGMA journal_mode = WAL;")
        Catch
            Polaczenie.Dispose()
            Throw
        End Try
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

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        Try
            _transakcja?.Dispose()
            _zapis?.Dispose()
            _odczyt?.Dispose()
        Finally
            Polaczenie.Dispose()
        End Try
    End Sub

End Class

''' <summary>Opis mapy zapisywany w tabeli metadata pliku MBTiles.</summary>
Public Class OpisMbtiles
    Public Property Nazwa As String = ""
    Public Property Opis As String = ""
    Public Property Atrybucja As String = ""
    ''' <summary>Nakładka (type = overlay) wyświetlana nad mapą podkładową aplikacji; inaczej baselayer.</summary>
    Public Property Nakladka As Boolean
    ''' <summary>Kafle PNG (format = png); inaczej JPEG.</summary>
    Public Property Png As Boolean
    Public Property PoziomMin As Integer
    Public Property PoziomMax As Integer
    Public Property Obszar As ProstokatGeo
    Public Property Wersja As String = "1.1"
End Class

''' <summary>
''' MBTiles: wiersze kafli liczone od dołu (TMS), opis mapy w tabeli metadata. Jedyny zapis MBTiles w programie -
''' używają go eksport mapy, MBTiles wieloskalowy (KompilatorMbtiles) i konwerter OziExplorer; dawniej każde z nich
''' miało własną kopię poleceń SQL, a różnice (np. tabela android_metadata tylko w jednym) wynikały z przypadku.
''' </summary>
Public Class MagazynMBTiles
    Inherits MagazynKafli

    Public Sub New(z As ZadanieEksportu, obszar As ProstokatGeo)
        Me.New(z.Plik, New OpisMbtiles With {.Nazwa = z.Nazwa, .Opis = z.Metadane.Opis, .Atrybucja = z.Metadane.Atrybucja,
                                             .Nakladka = z.Nakladka, .Png = z.Png, .PoziomMin = z.PoziomMin, .PoziomMax = z.PoziomMax,
                                             .Obszar = obszar}, z)
    End Sub

    ''' <param name="z">Zadanie eksportu albo Nothing (kompilator wieloskalowy, konwerter Ozi).</param>
    Public Sub New(plik As String, opis As OpisMbtiles, Optional z As ZadanieEksportu = Nothing)
        MyBase.New(plik, z)
        Try
            Wykonaj("CREATE TABLE metadata (name text, value text);")
            Wykonaj("CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob);")
            'indeks od początku: piramida odczytuje kafle poziomu wyżej zapytaniem SELECT - bez indeksu każde przeglądało
            'całą tabelę i czas zapisu rósł z kwadratem liczby kafli
            Wykonaj("CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);")
            'tabela oczekiwana przez bazy SQLite otwierane w Androidzie
            Wykonaj("CREATE TABLE android_metadata (locale text);")
            Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")
            Dim ci = Globalization.CultureInfo.InvariantCulture
            Dim obszar = opis.Obszar
            Dim srodekSz As Double = (obszar.Polnoc + obszar.Poludnie) / 2
            Dim meta As New Dictionary(Of String, String) From {
                {"name", opis.Nazwa}, {"type", If(opis.Nakladka, "overlay", "baselayer")}, {"version", opis.Wersja}, {"description", opis.Opis},
                {"format", If(opis.Png, "png", "jpg")}, {"minzoom", opis.PoziomMin.ToString(ci)}, {"maxzoom", opis.PoziomMax.ToString(ci)},
                {"bounds", String.Format(ci, "{0},{1},{2},{3}", obszar.Zachod, obszar.Poludnie, obszar.Wschod, obszar.Polnoc)},
                {"center", String.Format(ci, "{0},{1},{2}", (obszar.Zachod + obszar.Wschod) / 2, srodekSz, opis.PoziomMax)}}
            If Not String.IsNullOrEmpty(opis.Atrybucja) Then meta("attribution") = opis.Atrybucja
            For Each m In meta
                Wstaw("INSERT INTO metadata (name, value) VALUES (@0, @1);", m.Key, m.Value)
            Next
            Przygotuj("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);",
                      "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;")
        Catch
            Dispose()
            Throw
        End Try
    End Sub

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        Return Tuple.Create(k.Z, k.X, k.WierszTms)
    End Function

End Class

''' <summary>
''' GeoPackage (OGC GPKG): kafle Web Mercator (EPSG:3857) dla QGIS, QField i Mergin Maps.
''' Baza SQLite zgodna ze specyfikacją OGC GeoPackage 1.3 (pragmy, tabele metadanych gpkg_* oraz tabela kafli).
''' </summary>
Public Class MagazynGeoPackage
    Inherits MagazynKafli

    Private Const Granica3857 As Double = 20037508.342789244
    Private Const PromienZiemi As Double = 6378137.0
    Private Const MaksSzerokosc As Double = 85.0511287798066

    Public Sub New(z As ZadanieEksportu, obszar As ProstokatGeo)
        Me.New(z.Plik, New OpisMbtiles With {.Nazwa = z.Nazwa, .Opis = z.Metadane.Opis, .Atrybucja = z.Metadane.Atrybucja,
                                             .Nakladka = z.Nakladka, .Png = z.Png, .PoziomMin = z.PoziomMin, .PoziomMax = z.PoziomMax,
                                             .Obszar = obszar}, z)
    End Sub

    ''' <param name="z">Zadanie eksportu albo Nothing.</param>
    Public Sub New(plik As String, opis As OpisMbtiles, Optional z As ZadanieEksportu = Nothing)
        MyBase.New(plik, z)
        Try
            ' 1. Nagłówki binarne SQLite wymagane przez specyfikację OGC GeoPackage (Klauzula 1.1.1.1.1)
            Wykonaj("PRAGMA application_id = 1196444487;") ' 0x47504B47 = 'GPKG'
            Wykonaj("PRAGMA user_version = 10300;")        ' GeoPackage 1.3.0

            ' 2. Tabela gpkg_spatial_ref_sys i obowiązkowe rekordy OGC (Requirement 11) oraz EPSG:3857
            Wykonaj("CREATE TABLE gpkg_spatial_ref_sys (" &
                    "srs_name TEXT NOT NULL, " &
                    "srs_id INTEGER NOT NULL PRIMARY KEY, " &
                    "organization TEXT NOT NULL, " &
                    "organization_coordsys_id INTEGER NOT NULL, " &
                    "definition TEXT NOT NULL, " &
                    "description TEXT);")

            Wstaw("INSERT INTO gpkg_spatial_ref_sys VALUES (@0, @1, @2, @3, @4, @5);",
                  "Undefined Cartesian SRS", 0, "NONE", 0, "undefined", "undefined cartesian coordinate reference system")
            Wstaw("INSERT INTO gpkg_spatial_ref_sys VALUES (@0, @1, @2, @3, @4, @5);",
                  "Undefined geographic SRS", -1, "NONE", -1, "undefined", "undefined geographic coordinate reference system")
            Wstaw("INSERT INTO gpkg_spatial_ref_sys VALUES (@0, @1, @2, @3, @4, @5);",
                  "WGS 84", 4326, "EPSG", 4326,
                  "GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563]],PRIMEM[""Greenwich"",0],UNIT[""degree"",0.0174532925199433]]",
                  "WGS 84")
            Wstaw("INSERT INTO gpkg_spatial_ref_sys VALUES (@0, @1, @2, @3, @4, @5);",
                  "WGS 84 / Pseudo-Mercator", 3857, "EPSG", 3857,
                  "PROJCS[""WGS 84 / Pseudo-Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Popular_Visualisation_Pseudo_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",0],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH],EXTENSION[""PROJ4"",""+proj=merc +a=6378137 +b=6378137 +lat_ts=0 +lon_0=0 +x_0=0 +y_0=0 +k=1 +units=m +nadgrids=@null +wktext +no_defs""],AUTHORITY[""EPSG"",""3857""]]",
                  "Web Mercator")

            ' 3. Tabela gpkg_contents z rzeczywistym zakresem danych w metrach EPSG:3857
            Wykonaj("CREATE TABLE gpkg_contents (" &
                    "table_name TEXT NOT NULL PRIMARY KEY, " &
                    "data_type TEXT NOT NULL, " &
                    "identifier TEXT, " &
                    "description TEXT DEFAULT '', " &
                    "last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')), " &
                    "min_x DOUBLE, " &
                    "min_y DOUBLE, " &
                    "max_x DOUBLE, " &
                    "max_y DOUBLE, " &
                    "srs_id INTEGER, " &
                    "CONSTRAINT fk_gc_r_srs_id FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id));")

            Dim bb = DoMetrow3857(opis.Obszar)
            'GeoPackage nie ma pola atrybucji (MBTiles ma metadane attribution), więc trafia ona do opisu warstwy
            Dim opisWarstwy = If(opis.Opis, "")
            If Not String.IsNullOrWhiteSpace(opis.Atrybucja) Then
                opisWarstwy = If(opisWarstwy = "", "", opisWarstwy & " ") & "Źródło: " & opis.Atrybucja
            End If
            Wstaw("INSERT INTO gpkg_contents (table_name, data_type, identifier, description, min_x, min_y, max_x, max_y, srs_id) " &
                  "VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8);",
                  "tiles", "tiles", opis.Nazwa, opisWarstwy, bb.Item1, bb.Item2, bb.Item3, bb.Item4, 3857)

            ' 4. Tabela gpkg_tile_matrix_set: pełna globalna siatka Web Mercator (Requirement 37)
            Wykonaj("CREATE TABLE gpkg_tile_matrix_set (" &
                    "table_name TEXT NOT NULL PRIMARY KEY, " &
                    "srs_id INTEGER NOT NULL, " &
                    "min_x DOUBLE NOT NULL, " &
                    "min_y DOUBLE NOT NULL, " &
                    "max_x DOUBLE NOT NULL, " &
                    "max_y DOUBLE NOT NULL, " &
                    "CONSTRAINT fk_gtms_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name), " &
                    "CONSTRAINT fk_gtms_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id));")

            Wstaw("INSERT INTO gpkg_tile_matrix_set VALUES (@0, @1, @2, @3, @4, @5);",
                  "tiles", 3857, -Granica3857, -Granica3857, Granica3857, Granica3857)

            ' 5. Tabela gpkg_tile_matrix ze zdefiniowanymi poziomami powiększenia (Requirement 41)
            Wykonaj("CREATE TABLE gpkg_tile_matrix (" &
                    "table_name TEXT NOT NULL, " &
                    "zoom_level INTEGER NOT NULL, " &
                    "matrix_width INTEGER NOT NULL, " &
                    "matrix_height INTEGER NOT NULL, " &
                    "tile_width INTEGER NOT NULL, " &
                    "tile_height INTEGER NOT NULL, " &
                    "pixel_x_size DOUBLE NOT NULL, " &
                    "pixel_y_size DOUBLE NOT NULL, " &
                    "PRIMARY KEY (table_name, zoom_level), " &
                    "CONSTRAINT fk_gtm_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name));")

            Dim obwod = 2.0 * Granica3857
            For zoom = Math.Max(0, Math.Min(opis.PoziomMin, opis.PoziomMax)) To Math.Min(22, Math.Max(opis.PoziomMin, opis.PoziomMax))
                Dim n = 1L << zoom
                Dim piksel = obwod / (256.0 * n)
                Wstaw("INSERT INTO gpkg_tile_matrix VALUES (@0, @1, @2, @3, @4, @5, @6, @7);",
                      "tiles", zoom, n, n, 256, 256, piksel, piksel)
            Next

            ' 6. Tabela kafli z kluczem głównym id (Requirement 34 i 35)
            Wykonaj("CREATE TABLE tiles (" &
                    "id INTEGER PRIMARY KEY AUTOINCREMENT, " &
                    "zoom_level INTEGER NOT NULL, " &
                    "tile_column INTEGER NOT NULL, " &
                    "tile_row INTEGER NOT NULL, " &
                    "tile_data BLOB NOT NULL, " &
                    "UNIQUE (zoom_level, tile_column, tile_row));")

            ' Tabela android_metadata dla bazy SQLite otwieranej przez Androida (dodatkowe tabele są dozwolone w GeoPackage)
            Wykonaj("CREATE TABLE android_metadata (locale text);")
            Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")

            ' Przygotowanie poleceń zapisu i odczytu kafli
            Przygotuj("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);",
                      "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;")
        Catch
            Dispose()
            Throw
        End Try
    End Sub

    Private Shared Function DoMetrow3857(p As ProstokatGeo) As Tuple(Of Double, Double, Double, Double)
        If Not p.Poprawny Then Return Tuple.Create(-Granica3857, -Granica3857, Granica3857, Granica3857)
        Dim minX = PromienZiemi * p.Zachod * Math.PI / 180.0
        Dim maxX = PromienZiemi * p.Wschod * Math.PI / 180.0
        Dim sPoludnie = Math.Max(-MaksSzerokosc, Math.Min(MaksSzerokosc, p.Poludnie)) * Math.PI / 180.0
        Dim sPolnoc = Math.Max(-MaksSzerokosc, Math.Min(MaksSzerokosc, p.Polnoc)) * Math.PI / 180.0
        Dim minY = PromienZiemi * Math.Log(Math.Tan(Math.PI / 4.0 + sPoludnie / 2.0))
        Dim maxY = PromienZiemi * Math.Log(Math.Tan(Math.PI / 4.0 + sPolnoc / 2.0))
        Return Tuple.Create(minX, minY, maxX, maxY)
    End Function

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        ' W GeoPackage origin znajduje się w lewym górnym rogu (Top-Left / XYZ), wiersze rosną w dół
        Return Tuple.Create(k.Z, k.X, k.Y)
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
        Try
            Wykonaj("CREATE TABLE tiles (x int, y int, z int, s int, image blob, PRIMARY KEY (x, y, z, s));")
            Wykonaj("CREATE TABLE info (maxzoom int, minzoom int, tilenumbering text, tilesize int, ellipsoid int, inverted_y int);")
            'tabela wymagana przez bazy SQLite otwierane w Androidzie bez prawa zapisu
            Wykonaj("CREATE TABLE android_metadata (locale text);")
            Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")
            Przygotuj("INSERT OR REPLACE INTO tiles (x, y, z, s, image) VALUES (@x, @y, @z, 0, @d);",
                      "SELECT image FROM tiles WHERE x = @x AND y = @y AND z = @z AND s = 0;")
        Catch
            Dispose()
            Throw
        End Try
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
        Try
            Wykonaj("CREATE TABLE tiles (x int, y int, z int, image blob, PRIMARY KEY (x, y, z));")
            Wykonaj("CREATE TABLE android_metadata (locale text);")
            Wykonaj("INSERT INTO android_metadata VALUES ('pl_PL');")
            Przygotuj("INSERT OR REPLACE INTO tiles (x, y, z, image) VALUES (@x, @y, @z, @d);",
                      "SELECT image FROM tiles WHERE x = @x AND y = @y AND z = @z;")
        Catch
            Dispose()
            Throw
        End Try
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

Friend Class MagazynJnx
    Inherits MagazynKafli

    Private ReadOnly _plikTmp As String
    Private ReadOnly _obszar As ProstokatGeo

    Public Sub New(z As ZadanieEksportu, obszar As ProstokatGeo)
        MyBase.New(z.Plik & ".tmp_tiles", z)
        _plikTmp = z.Plik & ".tmp_tiles"
        _obszar = obszar
        Try
            Wykonaj("CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob, PRIMARY KEY (zoom_level, tile_column, tile_row));")
            Przygotuj("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);",
                      "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;")
        Catch
            Dispose()
            Throw
        End Try
    End Sub

    Protected Overrides Function Indeks(k As KafelXYZ) As Tuple(Of Integer, Integer, Integer)
        Return Tuple.Create(k.Z, k.X, k.Y)
    End Function

    Protected Overrides Sub Zakoncz()
        Dim poziomy As New List(Of PoziomJnx)
        Dim srodekSz As Double = (_obszar.Polnoc + _obszar.Poludnie) / 2.0

        Using cmd As New SQLiteCommand("SELECT zoom_level, tile_column, tile_row, tile_data FROM tiles ORDER BY zoom_level ASC, tile_row ASC, tile_column ASC;", Polaczenie),
              r = cmd.ExecuteReader()

            Dim biezacyPoziom As PoziomJnx = Nothing
            Dim biezacyZ As Integer = -1

            While r.Read()
                Dim z = r.GetInt32(0)
                Dim x = r.GetInt32(1)
                Dim y = r.GetInt32(2)
                Dim dane = DirectCast(r(3), Byte())

                If biezacyPoziom Is Nothing OrElse biezacyZ <> z Then
                    biezacyZ = z
                    Dim skala = ZapisJnx.SkalaDlaPoziomu(z, srodekSz)
                    biezacyPoziom = New PoziomJnx(skala)
                    poziomy.Add(biezacyPoziom)
                End If

                Dim polnoc As Double = WebMercator.Szerokosc(y * CDbl(WebMercator.RozmiarKafla), z)
                Dim poludnie As Double = WebMercator.Szerokosc((y + 1) * CDbl(WebMercator.RozmiarKafla), z)
                Dim zachod As Double = WebMercator.Dlugosc(x * CDbl(WebMercator.RozmiarKafla), z)
                Dim wschod As Double = WebMercator.Dlugosc((x + 1) * CDbl(WebMercator.RozmiarKafla), z)

                Dim kafel As New KafelJnx(New ProstokatGeo(polnoc, poludnie, wschod, zachod), dane)
                biezacyPoziom.Kafle.Add(kafel)
            End While
        End Using

        If poziomy.Count = 0 Then
            Throw New InvalidOperationException("Brak kafli do zapisania w formacie JNX.")
        End If

        ZapisJnx.Zapisz(Zadanie.Plik, Zadanie.Nazwa, Zadanie.KolejnoscRysowania, poziomy)
    End Sub

    Public Overrides Sub Dispose()
        Try
            MyBase.Dispose()
        Finally
            Try
                SQLiteConnection.ClearAllPools()
                EksportMapy.UsunPlikBazy(_plikTmp)
            Catch
            End Try
        End Try
    End Sub
End Class
