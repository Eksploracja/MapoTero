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

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Xml.Linq

''' <summary>
''' Obszar pobierania wzdłuż trasy (GPX, KML, KMZ): pas o zadanej szerokości po obu stronach linii trasy.
''' Pobierane są tylko segmenty siatki, które przecinają ten pas - przy trasie ukośnej lub krętej to wielokrotnie
''' mniej danych niż prostokąt opisany na trasie.
''' </summary>
Public Class Korytarz

    ''' <summary>Nazwa pliku korytarza w folderze sesji (obok conf.txt).</summary>
    Public Const NazwaPliku As String = "korytarz.txt"

    Private Const Naglowek As String = "MapoTero korytarz trasy"

    ''' <summary>Linie trasy (ślady, trasy, linie KML) w WGS84.</summary>
    Public ReadOnly Property Linie As New List(Of List(Of PunktGeo))

    ''' <summary>Odległość granicy pasa od osi trasy w metrach (po każdej stronie trasy).</summary>
    Public Property SzerokoscMetry As Double = 1000

    ''' <summary>Nazwa trasy (z pliku lub nazwa pliku).</summary>
    Public Property Nazwa As String = ""

    ''' <summary>Czy trasa zawiera co najmniej jeden punkt.</summary>
    Public ReadOnly Property Poprawny As Boolean
        Get
            For Each l In Linie
                If l.Count > 0 Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>Liczba punktów trasy.</summary>
    Public ReadOnly Property LiczbaPunktow As Integer
        Get
            Dim n As Integer = 0
            For Each l In Linie
                n += l.Count
            Next
            Return n
        End Get
    End Property

    ''' <summary>Długość trasy w kilometrach.</summary>
    Public ReadOnly Property DlugoscKm As Double
        Get
            Dim suma As Double = 0
            For Each l In Linie
                For i = 1 To l.Count - 1
                    suma += Georeferencja.OdlegloscMetry(l(i - 1), l(i))
                Next
            Next
            Return suma / 1000
        End Get
    End Property

#Region "Odczyt tras"

    ''' <summary>Wczytuje trasę z pliku GPX, KML albo KMZ (rozpoznanie po rozszerzeniu, a gdy nieznane - po treści).</summary>
    Public Shared Function WczytajTrase(sciezka As String) As Korytarz
        Dim k As Korytarz
        If Path.GetExtension(sciezka).Equals(".kmz", StringComparison.OrdinalIgnoreCase) Then
            k = ZKmz(File.ReadAllBytes(sciezka))
        Else
            k = ZTekstu(File.ReadAllText(sciezka))
        End If
        If k.Nazwa = "" Then k.Nazwa = Path.GetFileNameWithoutExtension(sciezka)
        Return k
    End Function

    ''' <summary>Trasa z dokumentu GPX albo KML.</summary>
    Public Shared Function ZTekstu(tekst As String) As Korytarz
        Dim doc = XDocument.Parse(tekst)
        If doc.Root Is Nothing Then Throw New InvalidDataException("Pusty dokument")
        Select Case doc.Root.Name.LocalName.ToLowerInvariant()
            Case "gpx" : Return ZGpx(doc)
            Case "kml" : Return ZKml(doc)
            Case Else : Throw New InvalidDataException("Nieznany format pliku trasy (oczekiwano GPX lub KML)")
        End Select
    End Function

    ''' <summary>Trasa z archiwum KMZ (pierwszy dokument .kml w archiwum, najchętniej doc.kml).</summary>
    Public Shared Function ZKmz(dane() As Byte) As Korytarz
        Using ms As New MemoryStream(dane), zip As New ZipArchive(ms, ZipArchiveMode.Read)
            Dim wpis As ZipArchiveEntry = Nothing
            For Each e In zip.Entries
                If e.FullName.EndsWith(".kml", StringComparison.OrdinalIgnoreCase) Then
                    If wpis Is Nothing OrElse e.FullName.Equals("doc.kml", StringComparison.OrdinalIgnoreCase) Then wpis = e
                End If
            Next
            If wpis Is Nothing Then Throw New InvalidDataException("Archiwum KMZ nie zawiera dokumentu KML")
            Using s = wpis.Open(), r As New StreamReader(s, Encoding.UTF8, True)
                Return ZTekstu(r.ReadToEnd())
            End Using
        End Using
    End Function

    ''' <summary>
    ''' GPX: ślady (trk/trkseg/trkpt) i trasy (rte/rtept); gdy plik zawiera tylko punkty (wpt) - linia łącząca je w kolejności.
    ''' </summary>
    Private Shared Function ZGpx(doc As XDocument) As Korytarz
        Dim k As New Korytarz()
        For Each el In doc.Root.Elements()
            Select Case el.Name.LocalName
                Case "metadata"
                    If k.Nazwa = "" Then k.Nazwa = TekstElementu(el, "name")
                Case "trk"
                    If k.Nazwa = "" Then k.Nazwa = TekstElementu(el, "name")
                    For Each seg In el.Elements()
                        If seg.Name.LocalName <> "trkseg" Then Continue For
                        k.DodajLinie(PunktyGpx(seg, "trkpt"))
                    Next
                Case "rte"
                    If k.Nazwa = "" Then k.Nazwa = TekstElementu(el, "name")
                    k.DodajLinie(PunktyGpx(el, "rtept"))
            End Select
        Next
        If Not k.Poprawny Then k.DodajLinie(PunktyGpx(doc.Root, "wpt"))
        Return k
    End Function

    Private Shared Function PunktyGpx(rodzic As XElement, nazwa As String) As List(Of PunktGeo)
        Dim lista As New List(Of PunktGeo)
        For Each p In rodzic.Elements()
            If p.Name.LocalName <> nazwa Then Continue For
            Dim sz As Double, dl As Double
            If LiczbaAtrybutu(p, "lat", sz) AndAlso LiczbaAtrybutu(p, "lon", dl) Then lista.Add(New PunktGeo(sz, dl))
        Next
        Return lista
    End Function

    ''' <summary>KML: linie (LineString, także w MultiGeometry) i ślady gx:Track; obrys wielokąta traktowany jest jak linia.</summary>
    Private Shared Function ZKml(doc As XDocument) As Korytarz
        Dim k As New Korytarz()
        For Each el In doc.Descendants()
            Select Case el.Name.LocalName
                Case "Document", "Folder", "Placemark"
                    If k.Nazwa = "" Then k.Nazwa = TekstElementu(el, "name")
                Case "LineString", "LinearRing"
                    For Each c In el.Elements()
                        If c.Name.LocalName = "coordinates" Then k.DodajLinie(WspolrzedneKml(c.Value))
                    Next
                Case "Track"
                    Dim lista As New List(Of PunktGeo)
                    For Each c In el.Elements()
                        If c.Name.LocalName <> "coord" Then Continue For
                        Dim cz = c.Value.Split(New Char() {" "c, ChrW(9)}, StringSplitOptions.RemoveEmptyEntries)
                        Dim dl As Double, sz As Double
                        If cz.Length >= 2 AndAlso Liczba(cz(0), dl) AndAlso Liczba(cz(1), sz) Then lista.Add(New PunktGeo(sz, dl))
                    Next
                    k.DodajLinie(lista)
            End Select
        Next
        'sam punkt (Point) - korytarz wokół punktu
        If Not k.Poprawny Then
            For Each el In doc.Descendants()
                If el.Name.LocalName <> "Point" Then Continue For
                For Each c In el.Elements()
                    If c.Name.LocalName = "coordinates" Then k.DodajLinie(WspolrzedneKml(c.Value))
                Next
            Next
        End If
        Return k
    End Function

    ''' <summary>Współrzędne KML: krotki "długość,szerokość[,wysokość]" rozdzielone białymi znakami.</summary>
    Private Shared Function WspolrzedneKml(tekst As String) As List(Of PunktGeo)
        Dim lista As New List(Of PunktGeo)
        For Each krotka In tekst.Split(New Char() {" "c, ChrW(9), ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
            Dim cz = krotka.Split(","c)
            Dim dl As Double, sz As Double
            If cz.Length >= 2 AndAlso Liczba(cz(0), dl) AndAlso Liczba(cz(1), sz) Then lista.Add(New PunktGeo(sz, dl))
        Next
        Return lista
    End Function

    Private Shared Function TekstElementu(rodzic As XElement, nazwa As String) As String
        For Each e In rodzic.Elements()
            If e.Name.LocalName = nazwa Then Return e.Value.Trim()
        Next
        Return ""
    End Function

    Private Shared Function LiczbaAtrybutu(e As XElement, nazwa As String, ByRef wynik As Double) As Boolean
        Dim a = e.Attribute(nazwa)
        Return a IsNot Nothing AndAlso Liczba(a.Value, wynik)
    End Function

    Private Shared Function Liczba(tekst As String, ByRef wynik As Double) As Boolean
        Return Double.TryParse(tekst.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, wynik) AndAlso
               Not Double.IsNaN(wynik) AndAlso Not Double.IsInfinity(wynik)
    End Function

    ''' <summary>Dodaje linię (puste linie i punkty spoza zakresu współrzędnych geograficznych są pomijane).</summary>
    Public Sub DodajLinie(punkty As IEnumerable(Of PunktGeo))
        Dim lista As New List(Of PunktGeo)
        For Each p In punkty
            If Math.Abs(p.Szerokosc) <= 90 AndAlso Math.Abs(p.Dlugosc) <= 180 Then lista.Add(p)
        Next
        If lista.Count > 0 Then Linie.Add(lista)
    End Sub

#End Region

#Region "Zapis w folderze sesji"

    ''' <summary>Zapis korytarza (plik tekstowy UTF-8: nagłówek, szerokość, nazwa, a dalej linie trasy).</summary>
    Public Function DoTekstu() As String
        Dim ci = CultureInfo.InvariantCulture
        Dim sb As New StringBuilder()
        sb.Append(Naglowek).Append(ChrW(13)).Append(ChrW(10))
        sb.Append("szerokosc_m=").Append(SzerokoscMetry.ToString("0.###", ci)).Append(ChrW(13)).Append(ChrW(10))
        sb.Append("nazwa=").Append(Nazwa.Replace(ChrW(13), " "c).Replace(ChrW(10), " "c)).Append(ChrW(13)).Append(ChrW(10))
        For Each l In Linie
            sb.Append("linia").Append(ChrW(13)).Append(ChrW(10))
            For Each p In l
                sb.Append(p.Szerokosc.ToString("0.0000000", ci)).Append(" "c).Append(p.Dlugosc.ToString("0.0000000", ci)).Append(ChrW(13)).Append(ChrW(10))
            Next
        Next
        Return sb.ToString()
    End Function

    Public Shared Function ZTekstuZapisu(tekst As String) As Korytarz
        Dim k As New Korytarz()
        Dim linie = tekst.Replace(ChrW(13), "").Split(ChrW(10))
        If linie.Length = 0 OrElse linie(0).Trim() <> Naglowek Then Throw New InvalidDataException("To nie jest plik korytarza trasy programu MapoTero")
        Dim biezaca As List(Of PunktGeo) = Nothing
        For i = 1 To linie.Length - 1
            Dim t As String = linie(i).Trim()
            If t = "" Then Continue For
            If t.StartsWith("szerokosc_m=", StringComparison.Ordinal) Then
                Dim w As Double
                If Liczba(t.Substring(12), w) AndAlso w > 0 Then k.SzerokoscMetry = w
            ElseIf t.StartsWith("nazwa=", StringComparison.Ordinal) Then
                k.Nazwa = t.Substring(6)
            ElseIf t = "linia" Then
                If biezaca IsNot Nothing Then k.DodajLinie(biezaca)
                biezaca = New List(Of PunktGeo)
            ElseIf biezaca IsNot Nothing Then
                Dim cz = t.Split(" "c)
                Dim sz As Double, dl As Double
                If cz.Length >= 2 AndAlso Liczba(cz(0), sz) AndAlso Liczba(cz(1), dl) Then biezaca.Add(New PunktGeo(sz, dl))
            End If
        Next
        If biezaca IsNot Nothing Then k.DodajLinie(biezaca)
        Return k
    End Function

    Public Sub Zapisz(sciezka As String)
        File.WriteAllText(sciezka, DoTekstu(), New UTF8Encoding(False))
    End Sub

    Public Shared Function Wczytaj(sciezka As String) As Korytarz
        Return ZTekstuZapisu(File.ReadAllText(sciezka, Encoding.UTF8))
    End Function

    ''' <summary>Korytarz zapisany w folderze sesji albo Nothing, gdy go nie ma lub nie da się go odczytać.</summary>
    Public Shared Function ZFolderu(folder As String) As Korytarz
        Try
            Dim plik As String = Path.Combine(folder, NazwaPliku)
            If Not File.Exists(plik) Then Return Nothing
            Dim k = Wczytaj(plik)
            Return If(k.Poprawny, k, Nothing)
        Catch
            Return Nothing
        End Try
    End Function

#End Region

#Region "Geometria"

    ''' <summary>
    ''' Przeliczenie punktów do układu siatki i do metrów: w układach kartograficznych współrzędne są już w metrach,
    ''' w WGS84 stopnie przeliczane są lokalnie (dla środka trasy) - wystarczająco dokładnie dla pasa wokół trasy.
    ''' </summary>
    Private Class Rzutowanie
        Public Uklad As UkladWspolrzednych
        Public SkalaX As Double = 1
        Public SkalaY As Double = 1

        Public Function Punkt(p As PunktGeo) As PunktXY
            Dim xy = Uklad.ZWgs84(p)
            Return New PunktXY(xy.X * SkalaX, xy.Y * SkalaY)
        End Function
    End Class

    Private Function UtworzRzutowanie(uklad As UkladWspolrzednych) As Rzutowanie
        Dim r As New Rzutowanie With {.Uklad = uklad}
        If uklad.Geograficzny Then
            Dim suma As Double = 0, n As Integer = 0
            For Each l In Linie
                For Each p In l
                    suma += p.Szerokosc : n += 1
                Next
            Next
            Dim mSz As Double, mDl As Double
            Georeferencja.MetrowNaStopien(If(n > 0, suma / n, 52), mSz, mDl)
            r.SkalaX = mSz
            r.SkalaY = mDl
        End If
        Return r
    End Function

    ''' <summary>Prostokąt opisany na korytarzu (trasa poszerzona o szerokość pasa) w układzie współrzędnych.</summary>
    Public Function Zasieg(uklad As UkladWspolrzednych) As Zasieg
        Dim r = UtworzRzutowanie(uklad)
        Dim xMin As Double = Double.MaxValue, xMax As Double = Double.MinValue, yMin As Double = Double.MaxValue, yMax As Double = Double.MinValue
        For Each l In Linie
            For Each p In l
                Dim q = r.Punkt(p)
                xMin = Math.Min(xMin, q.X) : xMax = Math.Max(xMax, q.X)
                yMin = Math.Min(yMin, q.Y) : yMax = Math.Max(yMax, q.Y)
            Next
        Next
        If xMin > xMax Then Return New Zasieg()
        Dim d As Double = SzerokoscMetry
        Return New Zasieg((xMin - d) / r.SkalaX, (yMin - d) / r.SkalaY, (xMax + d) / r.SkalaX, (yMax + d) / r.SkalaY)
    End Function

    ''' <summary>
    ''' Segmenty siatki przecinające korytarz: zbiór kluczy (wiersz - 1) * LiczbaKolumn + (kolumna - 1),
    ''' czyli numer kolejny segmentu pomniejszony o 1 (Siatka.NumerKolejny - 1).
    ''' </summary>
    Public Function Segmenty(siatka As Siatka, uklad As UkladWspolrzednych) As HashSet(Of Integer)
        Dim wynik As New HashSet(Of Integer)
        If siatka Is Nothing OrElse Not siatka.Poprawna OrElse siatka.ZbytDuza OrElse Not Poprawny Then Return wynik
        Dim r = UtworzRzutowanie(uklad)
        Dim kolumny As Integer = siatka.LiczbaKolumn, wiersze As Integer = siatka.LiczbaWierszy
        Dim d As Double = Math.Max(0, SzerokoscMetry)

        'siatka w metrach: lewy dolny narożnik i boki segmentu
        Dim x0 As Double = siatka.Obszar.XDol * r.SkalaX, y0 As Double = siatka.Obszar.YLewy * r.SkalaY
        Dim bokX As Double = siatka.BokSegmentu * r.SkalaX, bokY As Double = siatka.BokSegmentu * r.SkalaY
        Dim xGora As Double = x0 + wiersze * bokX

        For Each l In Linie
            Dim pkt As New List(Of PunktXY)(l.Count)
            For Each p In l
                pkt.Add(r.Punkt(p))
            Next
            Dim odcinki As Integer = Math.Max(1, pkt.Count - 1)
            For i = 0 To odcinki - 1
                Dim a = pkt(i)
                Dim b = If(pkt.Count > 1, pkt(i + 1), pkt(i))
                'wiersze siatki w zasięgu odcinka poszerzonego o szerokość pasa
                Dim w0 As Integer = Indeks((xGora - Math.Max(a.X, b.X) - d) / bokX, wiersze)
                Dim w1 As Integer = Indeks((xGora - Math.Min(a.X, b.X) + d) / bokX, wiersze)
                For w = w0 To w1
                    Dim xd As Double = xGora - w * bokX
                    'kolumny tylko w zasięgu części odcinka leżącej w pasie wiersza (dawniej cały prostokąt opisany na
                    'odcinku - przy długim ukośnym odcinku i małym segmencie miliony zbędnych sprawdzeń)
                    Dim yMin As Double, yMax As Double
                    If Not ZakresYWPasie(a, b, xd - d, xd + bokX + d, yMin, yMax) Then Continue For
                    Dim k0 As Integer = Indeks((yMin - d - y0) / bokY, kolumny)
                    Dim k1 As Integer = Indeks((yMax + d - y0) / bokY, kolumny)
                    For k = k0 To k1
                        Dim klucz As Integer = (w - 1) * kolumny + (k - 1)
                        If wynik.Contains(klucz) Then Continue For
                        Dim yl As Double = y0 + (k - 1) * bokY
                        If OdlegloscOdcinkaOdProstokata(a, b, xd, yl, xd + bokX, yl + bokY) <= d Then wynik.Add(klucz)
                    Next
                Next
            Next
        Next
        Return wynik
    End Function

    ''' <summary>Numer wiersza lub kolumny (od 1) dla położenia w segmentach, ograniczony do 1..maks (bez przepełnienia CInt).</summary>
    Private Shared Function Indeks(polozenie As Double, maks As Integer) As Integer
        Return CInt(Math.Max(1.0, Math.Min(maks, Math.Floor(polozenie) + 1)))
    End Function

    ''' <summary>
    ''' Zakres współrzędnej Y części odcinka AB, której współrzędna X leży w przedziale xMin..xMax; False - odcinek nie
    ''' przechodzi przez ten pas.
    ''' </summary>
    Private Shared Function ZakresYWPasie(a As PunktXY, b As PunktXY, xMin As Double, xMax As Double, ByRef yMin As Double, ByRef yMax As Double) As Boolean
        Dim t0 As Double = 0, t1 As Double = 1
        Dim dx As Double = b.X - a.X
        If dx = 0 Then
            If a.X < xMin OrElse a.X > xMax Then Return False
        Else
            Dim ta As Double = (xMin - a.X) / dx, tb As Double = (xMax - a.X) / dx
            t0 = Math.Max(0, Math.Min(ta, tb))
            t1 = Math.Min(1, Math.Max(ta, tb))
            If t0 > t1 Then Return False
        End If
        Dim ya As Double = a.Y + t0 * (b.Y - a.Y), yb As Double = a.Y + t1 * (b.Y - a.Y)
        yMin = Math.Min(ya, yb)
        yMax = Math.Max(ya, yb)
        Return True
    End Function

    ''' <summary>
    ''' Szacunkowa (zawyżona) liczba segmentów korytarza bez ich wyznaczania: pole pasa wokół trasy poszerzonego o segment,
    ''' podzielone przez pole segmentu. Pozwala uniknąć wyznaczania milionów segmentów, np. gdy bok segmentu jest
    ''' dopiero wpisywany (po pierwszej cyfrze "256" segment ma 2 piksele).
    ''' </summary>
    Public Function SzacowanaLiczbaSegmentow(siatka As Siatka, uklad As UkladWspolrzednych) As Double
        If siatka Is Nothing OrElse Not siatka.Poprawna OrElse Not Poprawny Then Return 0
        Dim r = UtworzRzutowanie(uklad)
        Dim bokX As Double = siatka.BokSegmentu * r.SkalaX, bokY As Double = siatka.BokSegmentu * r.SkalaY
        Dim pas As Double = 2 * Math.Max(0, SzerokoscMetry) + bokX + bokY
        Dim dlugosc As Double = 0
        For Each l In Linie
            For i = 1 To l.Count - 1
                Dim p = r.Punkt(l(i - 1)), q = r.Punkt(l(i))
                dlugosc += Math.Sqrt((q.X - p.X) ^ 2 + (q.Y - p.Y) ^ 2)
            Next
        Next
        Dim pole As Double = dlugosc * pas + Linie.Count * pas * pas
        Return Math.Min(siatka.LiczbaSegmentow, pole / (bokX * bokY))
    End Function

    ''' <summary>Odległość odcinka AB od prostokąta (0, gdy odcinek przecina prostokąt lub leży w nim).</summary>
    Public Shared Function OdlegloscOdcinkaOdProstokata(a As PunktXY, b As PunktXY, xMin As Double, yMin As Double, xMax As Double, yMax As Double) As Double
        If OdcinekPrzecinaProstokat(a, b, xMin, yMin, xMax, yMax) Then Return 0
        Dim wynik As Double = Math.Min(OdlegloscPunktuOdProstokata(a, xMin, yMin, xMax, yMax), OdlegloscPunktuOdProstokata(b, xMin, yMin, xMax, yMax))
        For Each n In {New PunktXY(xMin, yMin), New PunktXY(xMin, yMax), New PunktXY(xMax, yMin), New PunktXY(xMax, yMax)}
            wynik = Math.Min(wynik, OdlegloscPunktuOdOdcinka(n, a, b))
        Next
        Return wynik
    End Function

    Private Shared Function OdlegloscPunktuOdProstokata(p As PunktXY, xMin As Double, yMin As Double, xMax As Double, yMax As Double) As Double
        Dim dx As Double = Math.Max(0, Math.Max(xMin - p.X, p.X - xMax))
        Dim dy As Double = Math.Max(0, Math.Max(yMin - p.Y, p.Y - yMax))
        Return Math.Sqrt(dx * dx + dy * dy)
    End Function

    Private Shared Function OdlegloscPunktuOdOdcinka(p As PunktXY, a As PunktXY, b As PunktXY) As Double
        Dim vx As Double = b.X - a.X, vy As Double = b.Y - a.Y
        Dim dl2 As Double = vx * vx + vy * vy
        Dim t As Double = If(dl2 > 0, ((p.X - a.X) * vx + (p.Y - a.Y) * vy) / dl2, 0)
        t = Math.Max(0, Math.Min(1, t))
        Dim dx As Double = a.X + t * vx - p.X, dy As Double = a.Y + t * vy - p.Y
        Return Math.Sqrt(dx * dx + dy * dy)
    End Function

    ''' <summary>Test przecięcia odcinka z prostokątem (obcinanie Lianga-Barsky'ego).</summary>
    Private Shared Function OdcinekPrzecinaProstokat(a As PunktXY, b As PunktXY, xMin As Double, yMin As Double, xMax As Double, yMax As Double) As Boolean
        Dim t0 As Double = 0, t1 As Double = 1
        Dim dx As Double = b.X - a.X, dy As Double = b.Y - a.Y
        Dim p() As Double = {-dx, dx, -dy, dy}
        Dim q() As Double = {a.X - xMin, xMax - a.X, a.Y - yMin, yMax - a.Y}
        For i = 0 To 3
            If p(i) = 0 Then
                If q(i) < 0 Then Return False
            Else
                Dim t As Double = q(i) / p(i)
                If p(i) < 0 Then
                    If t > t1 Then Return False
                    If t > t0 Then t0 = t
                Else
                    If t < t0 Then Return False
                    If t < t1 Then t1 = t
                End If
            End If
        Next
        Return True
    End Function

#End Region

End Class
