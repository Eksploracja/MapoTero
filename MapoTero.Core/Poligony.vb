Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.RegularExpressions

    ''' <summary>
    ''' Punkt 2D w układzie kartezjańskim lub geograficznym. Pola nazwano od kierunku, bo pliki KML i GeoJSON
    ''' podają najpierw długość (wschód), a w programie X oznacza współrzędną północną - nazwy X/Y myliły.
    ''' </summary>
    Public Structure Punkt2D
        ''' <summary>Długość geograficzna albo współrzędna wschodnia (easting).</summary>
        Public Wschod As Double
        ''' <summary>Szerokość geograficzna albo współrzędna północna (northing).</summary>
        Public Polnoc As Double

        Public Sub New(wschod As Double, polnoc As Double)
            Me.Wschod = wschod
            Me.Polnoc = polnoc
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format(CultureInfo.InvariantCulture, "({0:0.######}, {1:0.######})", Wschod, Polnoc)
        End Function
    End Structure

    ''' <summary>
    ''' Prostokąt otaczający (Axis-Aligned Bounding Box).
    ''' </summary>
    Public Structure ZakresAABB
        Public MinWschod As Double
        Public MinPolnoc As Double
        Public MaxWschod As Double
        Public MaxPolnoc As Double

        Public Sub New(minX As Double, minY As Double, maxX As Double, maxY As Double)
            Me.MinWschod = minX
            Me.MinPolnoc = minY
            Me.MaxWschod = maxX
            Me.MaxPolnoc = maxY
        End Sub

        Public Function Przecina(inny As ZakresAABB) As Boolean
            Return Not (MaxWschod < inny.MinWschod OrElse MinWschod > inny.MaxWschod OrElse MaxPolnoc < inny.MinPolnoc OrElse MinPolnoc > inny.MaxPolnoc)
        End Function

        Public Function ZawieraPunkt(p As Punkt2D) As Boolean
            Return p.Wschod >= MinWschod AndAlso p.Wschod <= MaxWschod AndAlso p.Polnoc >= MinPolnoc AndAlso p.Polnoc <= MaxPolnoc
        End Function

        Public Shared Function Polacz(a As ZakresAABB, b As ZakresAABB) As ZakresAABB
            Return New ZakresAABB(
                Math.Min(a.MinWschod, b.MinWschod),
                Math.Min(a.MinPolnoc, b.MinPolnoc),
                Math.Max(a.MaxWschod, b.MaxWschod),
                Math.Max(a.MaxPolnoc, b.MaxPolnoc))
        End Function
    End Structure

    ''' <summary>
    ''' Rozpoznane rodziny układów współrzędnych dla importowanych plików KML/GeoJSON.
    ''' </summary>
    Public Enum RodzinaUkladuPoligonu
        Wgs84 = 4326
        WebMercator = 3857
        Pl1992 = 2180
        Pl2000Strefa5 = 2176
        Pl2000Strefa6 = 2177
        Pl2000Strefa7 = 2178
        Pl2000Strefa8 = 2179
        Utm33N = 32633
        Utm34N = 32634
        Utm35N = 32635
    End Enum

    ''' <summary>
    ''' Pojedynczy pierścień (obrys zewnętrzny lub dziura/enklawa) z przestrzennym indeksem pasowym krawędzi.
    ''' Optymalizowany pod granice z PRG (do 30 000+ wierzchołków).
    ''' </summary>
    Public NotInheritable Class PierscienPoligonu
        Public ReadOnly Wierzcholki As Punkt2D()
        Public ReadOnly Aabb As ZakresAABB

        Private ReadOnly _liczbaPasow As Integer
        Private ReadOnly _szerokoscPasa As Double
        Private ReadOnly _pasyKrawedzi As Integer()()

        Public Sub New(punkty As IList(Of Punkt2D))
            If punkty Is Nothing OrElse punkty.Count < 3 Then
                Throw New ArgumentException("Pierścień poligonu musi zawierać co najmniej 3 wierzchołki.")
            End If

            Dim zamkniety As Boolean = Math.Abs(punkty(0).Wschod - punkty(punkty.Count - 1).Wschod) < 0.000000001 AndAlso
                                       Math.Abs(punkty(0).Polnoc - punkty(punkty.Count - 1).Polnoc) < 0.000000001
            Dim liczba As Integer = If(zamkniety, punkty.Count, punkty.Count + 1)
            Wierzcholki = New Punkt2D(liczba - 1) {}

            Dim minX As Double = Double.PositiveInfinity
            Dim minY As Double = Double.PositiveInfinity
            Dim maxX As Double = Double.NegativeInfinity
            Dim maxY As Double = Double.NegativeInfinity

            For i As Integer = 0 To punkty.Count - 1
                Dim p As Punkt2D = punkty(i)
                Wierzcholki(i) = p
                If p.Wschod < minX Then minX = p.Wschod
                If p.Polnoc < minY Then minY = p.Polnoc
                If p.Wschod > maxX Then maxX = p.Wschod
                If p.Polnoc > maxY Then maxY = p.Polnoc
            Next
            If Not zamkniety Then
                Wierzcholki(liczba - 1) = Wierzcholki(0)
            End If

            Aabb = New ZakresAABB(minX, minY, maxX, maxY)

            Dim liczbaKrawedzi As Integer = Wierzcholki.Length - 1
            _liczbaPasow = Math.Max(1, Math.Min(512, CInt(Math.Sqrt(liczbaKrawedzi)) * 2))
            Dim rozpietoscX As Double = Math.Max(0.000001, Aabb.MaxWschod - Aabb.MinWschod)
            _szerokoscPasa = rozpietoscX / _liczbaPasow

            Dim listyPasow(_liczbaPasow - 1) As List(Of Integer)
            For i As Integer = 0 To _liczbaPasow - 1
                listyPasow(i) = New List(Of Integer)()
            Next

            For e As Integer = 0 To liczbaKrawedzi - 1
                Dim x1 As Double = Wierzcholki(e).Wschod
                Dim x2 As Double = Wierzcholki(e + 1).Wschod
                Dim pasOd As Integer = IndeksPasa(Math.Min(x1, x2))
                Dim pasDo As Integer = IndeksPasa(Math.Max(x1, x2))
                For p As Integer = pasOd To pasDo
                    listyPasow(p).Add(e)
                Next
            Next

            _pasyKrawedzi = New Integer(_liczbaPasow - 1)() {}
            For i As Integer = 0 To _liczbaPasow - 1
                _pasyKrawedzi(i) = listyPasow(i).ToArray()
            Next
        End Sub

        Private Function IndeksPasa(x As Double) As Integer
            Dim idx As Integer = CInt(Math.Floor((x - Aabb.MinWschod) / _szerokoscPasa))
            If idx < 0 Then Return 0
            If idx >= _liczbaPasow Then Return _liczbaPasow - 1
            Return idx
        End Function

        Public Function CzyKrawedzLubWierzcholekDotykaProstokata(seg As ZakresAABB) As Boolean
            If Not Aabb.Przecina(seg) Then Return False

            Dim pasOd As Integer = IndeksPasa(seg.MinWschod)
            Dim pasDo As Integer = IndeksPasa(seg.MaxWschod)

            For pas As Integer = pasOd To pasDo
                Dim krawedzie As Integer() = _pasyKrawedzi(pas)
                For i As Integer = 0 To krawedzie.Length - 1
                    Dim e As Integer = krawedzie(i)
                    Dim a As Punkt2D = Wierzcholki(e)
                    Dim b As Punkt2D = Wierzcholki(e + 1)

                    If Math.Max(a.Polnoc, b.Polnoc) < seg.MinPolnoc OrElse Math.Min(a.Polnoc, b.Polnoc) > seg.MaxPolnoc Then
                        Continue For
                    End If

                    If seg.ZawieraPunkt(a) OrElse seg.ZawieraPunkt(b) Then
                        Return True
                    End If

                    If OdcinekPrzecinaProstokat(a, b, seg) Then
                        Return True
                    End If
                Next
            Next

            Return False
        End Function

        Public Function ZawieraPunkt(p As Punkt2D) As Boolean
            If Not Aabb.ZawieraPunkt(p) Then Return False

            Dim pas As Integer = IndeksPasa(p.Wschod)
            Dim krawedzie As Integer() = _pasyKrawedzi(pas)
            Dim wewnatrz As Boolean = False

            For i As Integer = 0 To krawedzie.Length - 1
                Dim e As Integer = krawedzie(i)
                Dim a As Punkt2D = Wierzcholki(e)
                Dim b As Punkt2D = Wierzcholki(e + 1)

                If (a.Wschod > p.Wschod) <> (b.Wschod > p.Wschod) Then
                    Dim yPrzeciecia As Double = a.Polnoc + ((p.Wschod - a.Wschod) * (b.Polnoc - a.Polnoc)) / (b.Wschod - a.Wschod)
                    If yPrzeciecia > p.Polnoc Then
                        wewnatrz = Not wewnatrz
                    End If
                End If
            Next

            Return wewnatrz
        End Function

        Private Shared Function OdcinekPrzecinaProstokat(a As Punkt2D, b As Punkt2D, r As ZakresAABB) As Boolean
            Dim dx As Double = b.Wschod - a.Wschod
            Dim dy As Double = b.Polnoc - a.Polnoc
            Dim t0 As Double = 0.0
            Dim t1 As Double = 1.0

            If Not PrzytnijParametr(-dx, a.Wschod - r.MinWschod, t0, t1) Then Return False
            If Not PrzytnijParametr(dx, r.MaxWschod - a.Wschod, t0, t1) Then Return False
            If Not PrzytnijParametr(-dy, a.Polnoc - r.MinPolnoc, t0, t1) Then Return False
            If Not PrzytnijParametr(dy, r.MaxPolnoc - a.Polnoc, t0, t1) Then Return False

            Return t0 <= t1
        End Function

        Private Shared Function PrzytnijParametr(p As Double, q As Double, ByRef t0 As Double, ByRef t1 As Double) As Boolean
            If Math.Abs(p) < 0.000000000001 Then
                Return q >= 0.0
            End If
            Dim r As Double = q / p
            If p < 0.0 Then
                If r > t1 Then Return False
                If r > t0 Then t0 = r
            Else
                If r < t0 Then Return False
                If r < t1 Then t1 = r
            End If
            Return True
        End Function
    End Class

    ''' <summary>
    ''' Wielokąt złożony z obrysu zewnętrznego oraz opcjonalnych pierścieni wewnętrznych (dziur / enklaw).
    ''' </summary>
    Public NotInheritable Class Wielokat
        Public ReadOnly ObrysZewnetrzny As PierscienPoligonu
        Public ReadOnly Dziury As List(Of PierscienPoligonu)

        Public Sub New(obrys As PierscienPoligonu, dziury As IEnumerable(Of PierscienPoligonu))
            ObrysZewnetrzny = obrys
            Me.Dziury = If(dziury Is Nothing, New List(Of PierscienPoligonu)(), New List(Of PierscienPoligonu)(dziury))
        End Sub

        Public ReadOnly Property Aabb As ZakresAABB
            Get
                Return ObrysZewnetrzny.Aabb
            End Get
        End Property

        Public Function ZawieraPunkt(p As Punkt2D) As Boolean
            If Not ObrysZewnetrzny.ZawieraPunkt(p) Then Return False
            For Each dziura As PierscienPoligonu In Dziury
                If dziura.ZawieraPunkt(p) Then Return False
            Next
            Return True
        End Function

        Public Function CzySegmentPrzecina(seg As ZakresAABB) As Boolean
            If Not ObrysZewnetrzny.Aabb.Przecina(seg) Then Return False

            If ObrysZewnetrzny.CzyKrawedzLubWierzcholekDotykaProstokata(seg) Then
                Return True
            End If

            For Each dziura As PierscienPoligonu In Dziury
                If dziura.CzyKrawedzLubWierzcholekDotykaProstokata(seg) Then
                    Return True
                End If
            Next

            ' Brak przecięć i wierzchołków wewnątrz -> test dokładnie jednego narożnika
            Dim naroznik As New Punkt2D(seg.MinWschod, seg.MinPolnoc)
            Return ZawieraPunkt(naroznik)
        End Function
    End Class

    ''' <summary>
    ''' Zestaw wielokątów (obsługuje Polygon i MultiPolygon) z globalnym AABB i metadanymi CRS.
    ''' </summary>
    Public NotInheritable Class ZestawPoligonow
        Public ReadOnly Wielokaty As List(Of Wielokat)
        Public ReadOnly GlobalnyAabb As ZakresAABB
        Public Property WykrytyUklad As RodzinaUkladuPoligonu
        Public Property NiejednoznaczneOsie As Boolean
        Public Property OsieZamienione As Boolean
        Public Property ZrodloWykryciaUkladu As String = String.Empty

        Public Sub New(wielokaty As IEnumerable(Of Wielokat))
            Me.Wielokaty = New List(Of Wielokat)(wielokaty)
            If Me.Wielokaty.Count = 0 Then
                Throw New ArgumentException("Zestaw poligonów nie zawiera żadnego wielokąta.")
            End If

            Dim bbox As ZakresAABB = Me.Wielokaty(0).Aabb
            For i As Integer = 1 To Me.Wielokaty.Count - 1
                bbox = ZakresAABB.Polacz(bbox, Me.Wielokaty(i).Aabb)
            Next
            GlobalnyAabb = bbox
        End Sub

        Public ReadOnly Property LiczbaWierzcholkow As Integer
            Get
                Dim suma As Integer = 0
                For Each w As Wielokat In Wielokaty
                    suma += w.ObrysZewnetrzny.Wierzcholki.Length
                    For Each d As PierscienPoligonu In w.Dziury
                        suma += d.Wierzcholki.Length
                    Next
                Next
                Return suma
            End Get
        End Property

        Public Function CzySegmentPrzecina(seg As ZakresAABB) As Boolean
            If Not GlobalnyAabb.Przecina(seg) Then Return False
            For Each w As Wielokat In Wielokaty
                If w.CzySegmentPrzecina(seg) Then Return True
            Next
            Return False
        End Function

        Public Function ZawieraPunkt(p As Punkt2D) As Boolean
            If Not GlobalnyAabb.ZawieraPunkt(p) Then Return False
            For Each w As Wielokat In Wielokaty
                If w.ZawieraPunkt(p) Then Return True
            Next
            Return False
        End Function

        Public Function PrzeliczWspolrzedne(transformacja As Func(Of Punkt2D, Punkt2D)) As ZestawPoligonow
            Dim noweWielokaty As New List(Of Wielokat)(Wielokaty.Count)
            For Each w As Wielokat In Wielokaty
                Dim nowyObrys As PierscienPoligonu = PrzeliczPierscien(w.ObrysZewnetrzny, transformacja)
                Dim noweDziury As New List(Of PierscienPoligonu)(w.Dziury.Count)
                For Each d As PierscienPoligonu In w.Dziury
                    noweDziury.Add(PrzeliczPierscien(d, transformacja))
                Next
                noweWielokaty.Add(New Wielokat(nowyObrys, noweDziury))
            Next

            Return New ZestawPoligonow(noweWielokaty) With {
                .WykrytyUklad = Me.WykrytyUklad,
                .NiejednoznaczneOsie = Me.NiejednoznaczneOsie,
                .OsieZamienione = Me.OsieZamienione,
                .ZrodloWykryciaUkladu = Me.ZrodloWykryciaUkladu
            }
        End Function

        ''' <summary>Przelicza punkt z rodziny układów pliku na WGS84 (Wschod = długość, Polnoc = szerokość).</summary>
        Public Shared Function PunktDoWgs84(rodzina As RodzinaUkladuPoligonu, p As Punkt2D) As Punkt2D
            Select Case rodzina
                Case RodzinaUkladuPoligonu.Wgs84
                    Return p
                Case RodzinaUkladuPoligonu.WebMercator
                    Const granica As Double = 20037508.342789244
                    Dim lon = p.Wschod / granica * 180.0
                    Dim lat = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(p.Polnoc / granica * Math.PI)) - Math.PI / 2.0)
                    Return New Punkt2D(lon, lat)
                Case Else
                    'PL-1992, PL-2000 i UTM: wartość rodziny to kod EPSG
                    Dim uklad = UkladWspolrzednych.ZKodu(CInt(rodzina))
                    If uklad Is Nothing OrElse uklad.Geograficzny Then Return p
                    Dim geo = uklad.DoWgs84(New PunktXY(p.Polnoc, p.Wschod))
                    Return New Punkt2D(geo.Dlugosc, geo.Szerokosc)
            End Select
        End Function

        ''' <summary>
        ''' Zasięg wszystkich wierzchołków w układzie sesji. Liczony z każdego wierzchołka, a nie z dwóch narożników
        ''' prostokąta w układzie pliku, bo między układami (np. PL-1992 i PL-2000) osie są obrócone
        ''' i prostokąt z dwóch narożników obcinałby poligon.
        ''' </summary>
        Public Function ZasiegWUkladzie(uklad As UkladWspolrzednych) As Zasieg
            Dim xMin = Double.MaxValue, xMax = Double.MinValue, yMin = Double.MaxValue, yMax = Double.MinValue
            Dim dodaj = Sub(p As Punkt2D)
                            Dim g = PunktDoWgs84(WykrytyUklad, p)
                            Dim xy = uklad.ZWgs84(New PunktGeo(g.Polnoc, g.Wschod))
                            xMin = Math.Min(xMin, xy.X) : xMax = Math.Max(xMax, xy.X)
                            yMin = Math.Min(yMin, xy.Y) : yMax = Math.Max(yMax, xy.Y)
                        End Sub
            For Each w As Wielokat In Wielokaty
                For Each p In w.ObrysZewnetrzny.Wierzcholki
                    dodaj(p)
                Next
            Next
            Return New Zasieg(xMin, yMin, xMax, yMax)
        End Function

        Public Function ZamienOsie() As ZestawPoligonow
            Dim kopia As ZestawPoligonow = PrzeliczWspolrzedne(Function(p) New Punkt2D(p.Polnoc, p.Wschod))
            kopia.OsieZamienione = Not Me.OsieZamienione
            Return kopia
        End Function

        Private Shared Function PrzeliczPierscien(p As PierscienPoligonu, transformacja As Func(Of Punkt2D, Punkt2D)) As PierscienPoligonu
            Dim punkty(p.Wierzcholki.Length - 1) As Punkt2D
            For i As Integer = 0 To p.Wierzcholki.Length - 1
                punkty(i) = transformacja(p.Wierzcholki(i))
            Next
            Return New PierscienPoligonu(punkty)
        End Function
    End Class

    ''' <summary>
    ''' Parser plików KML oraz GeoJSON z odczytem nagłówka "crs", heurystyką polskich układów
    ''' oraz bezpiecznym ignorowaniem współrzędnej wysokościowej 3D ([lon, lat, alt]).
    ''' </summary>
    Public NotInheritable Class ParserPoligonu

        Public Shared Function WczytajZPliku(sciezka As String) As ZestawPoligonow
            If Not File.Exists(sciezka) Then
                Throw New FileNotFoundException("Nie znaleziono pliku poligonu.", sciezka)
            End If

            Dim ext As String = Path.GetExtension(sciezka).ToLowerInvariant()
            Select Case ext
                Case ".kml"
                    Return ParsujKml(File.ReadAllText(sciezka))
                Case ".geojson", ".json"
                    Return ParsujGeoJson(File.ReadAllText(sciezka))
                Case Else
                    Throw New NotSupportedException(String.Format("Nieobsługiwane rozszerzenie poligonu: {0} (obsługiwane: .kml, .geojson, .json).", ext))
            End Select
        End Function

        Public Shared Function ParsujKml(trescXml As String) As ZestawPoligonow
            Dim doc As XDocument = XDocument.Parse(trescXml)
            Dim wielokaty As New List(Of Wielokat)()

            For Each elemPoligon As XElement In doc.Descendants().Where(Function(e) e.Name.LocalName = "Polygon")
                Dim outerElem As XElement = elemPoligon.Descendants().FirstOrDefault(Function(e) e.Name.LocalName = "outerBoundaryIs")
                If outerElem Is Nothing Then Continue For

                Dim outerCoords As XElement = outerElem.Descendants().FirstOrDefault(Function(e) e.Name.LocalName = "coordinates")
                If outerCoords Is Nothing Then Continue For

                Dim punktyObrysu As List(Of Punkt2D) = ParsujWspolrzedneKml(outerCoords.Value)
                If punktyObrysu.Count < 3 Then Continue For

                Dim dziury As New List(Of PierscienPoligonu)()
                For Each innerElem As XElement In elemPoligon.Descendants().Where(Function(e) e.Name.LocalName = "innerBoundaryIs")
                    Dim innerCoords As XElement = innerElem.Descendants().FirstOrDefault(Function(e) e.Name.LocalName = "coordinates")
                    If innerCoords Is Nothing Then Continue For
                    Dim pktDziury As List(Of Punkt2D) = ParsujWspolrzedneKml(innerCoords.Value)
                    If pktDziury.Count >= 3 Then
                        dziury.Add(New PierscienPoligonu(pktDziury))
                    End If
                Next

                wielokaty.Add(New Wielokat(New PierscienPoligonu(punktyObrysu), dziury))
            Next

            If wielokaty.Count = 0 Then
                Throw New InvalidDataException("Plik KML nie zawiera żadnych zamkniętych poligonów (<Polygon>).")
            End If

            Return New ZestawPoligonow(wielokaty) With {
                .WykrytyUklad = RodzinaUkladuPoligonu.Wgs84,
                .NiejednoznaczneOsie = False,
                .ZrodloWykryciaUkladu = "Standard KML (WGS84 [lon, lat])"
            }
        End Function

        Private Shared Function ParsujWspolrzedneKml(tekst As String) As List(Of Punkt2D)
            Dim wynik As New List(Of Punkt2D)()
            Dim tokeny As String() = tekst.Split(New Char() {" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf},
                                                 StringSplitOptions.RemoveEmptyEntries)
            For Each token As String In tokeny
                Dim czesci As String() = token.Split(","c)
                If czesci.Length < 2 Then Continue For
                Dim lon As Double
                Dim lat As Double
                If Double.TryParse(czesci(0), NumberStyles.Float, CultureInfo.InvariantCulture, lon) AndAlso
                   Double.TryParse(czesci(1), NumberStyles.Float, CultureInfo.InvariantCulture, lat) Then
                    wynik.Add(New Punkt2D(lon, lat))
                End If
            Next
            Return wynik
        End Function

        Public Shared Function ParsujGeoJson(trescJson As String) As ZestawPoligonow
            Dim wielokaty As New List(Of Wielokat)()
            Dim epsgZNaglowka As Integer? = Nothing

            Using doc As JsonDocument = JsonDocument.Parse(trescJson)
                Dim root As JsonElement = doc.RootElement
                epsgZNaglowka = OdczytajEpsgZNaglowkaCrs(root)
                ZbierzWielokatyGeoJson(root, wielokaty)
            End Using

            If wielokaty.Count = 0 Then
                Throw New InvalidDataException("Plik GeoJSON nie zawiera geometrii typu Polygon ani MultiPolygon.")
            End If

            Dim zestaw As New ZestawPoligonow(wielokaty)
            RozpoznajUkladPoligonu(zestaw, epsgZNaglowka)
            Return zestaw
        End Function

        Private Shared Function OdczytajEpsgZNaglowkaCrs(root As JsonElement) As Integer?
            Dim crsElem As JsonElement
            If Not root.TryGetProperty("crs", crsElem) OrElse crsElem.ValueKind <> JsonValueKind.Object Then
                Return Nothing
            End If

            Dim propsElem As JsonElement
            If Not crsElem.TryGetProperty("properties", propsElem) OrElse propsElem.ValueKind <> JsonValueKind.Object Then
                Return Nothing
            End If

            Dim nameElem As JsonElement
            If Not propsElem.TryGetProperty("name", nameElem) OrElse nameElem.ValueKind <> JsonValueKind.String Then
                Return Nothing
            End If

            Dim nazwa As String = If(nameElem.GetString(), String.Empty)
            Dim m As Match = Regex.Match(nazwa, "EPSG[:]{1,2}(\d{4,5})", RegexOptions.IgnoreCase)
            If m.Success Then
                Return Integer.Parse(m.Groups(1).Value, CultureInfo.InvariantCulture)
            End If

            If nazwa.IndexOf("CRS84", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return 4326
            End If

            Return Nothing
        End Function

        Private Shared Sub ZbierzWielokatyGeoJson(elem As JsonElement, wynik As List(Of Wielokat))
            If elem.ValueKind <> JsonValueKind.Object Then Return
            Dim typElem As JsonElement
            If Not elem.TryGetProperty("type", typElem) OrElse typElem.ValueKind <> JsonValueKind.String Then Return

            Select Case typElem.GetString()
                Case "FeatureCollection"
                    Dim features As JsonElement
                    If elem.TryGetProperty("features", features) AndAlso features.ValueKind = JsonValueKind.Array Then
                        For Each f As JsonElement In features.EnumerateArray()
                            ZbierzWielokatyGeoJson(f, wynik)
                        Next
                    End If

                Case "Feature"
                    Dim geom As JsonElement
                    If elem.TryGetProperty("geometry", geom) Then
                        ZbierzWielokatyGeoJson(geom, wynik)
                    End If

                Case "GeometryCollection"
                    Dim geoms As JsonElement
                    If elem.TryGetProperty("geometries", geoms) AndAlso geoms.ValueKind = JsonValueKind.Array Then
                        For Each g As JsonElement In geoms.EnumerateArray()
                            ZbierzWielokatyGeoJson(g, wynik)
                        Next
                    End If

                Case "Polygon"
                    Dim coords As JsonElement
                    If elem.TryGetProperty("coordinates", coords) Then
                        Dim w As Wielokat = ParsujPojedynczyWielokatGeoJson(coords)
                        If w IsNot Nothing Then wynik.Add(w)
                    End If

                Case "MultiPolygon"
                    Dim coords As JsonElement
                    If elem.TryGetProperty("coordinates", coords) AndAlso coords.ValueKind = JsonValueKind.Array Then
                        For Each polyCoords As JsonElement In coords.EnumerateArray()
                            Dim w As Wielokat = ParsujPojedynczyWielokatGeoJson(polyCoords)
                            If w IsNot Nothing Then wynik.Add(w)
                        Next
                    End If
            End Select
        End Sub

        Private Shared Function ParsujPojedynczyWielokatGeoJson(polyCoords As JsonElement) As Wielokat
            If polyCoords.ValueKind <> JsonValueKind.Array Then Return Nothing
            Dim pierscienie As New List(Of PierscienPoligonu)()

            For Each ringElem As JsonElement In polyCoords.EnumerateArray()
                If ringElem.ValueKind <> JsonValueKind.Array Then Continue For
                Dim punkty As New List(Of Punkt2D)()
                For Each ptElem As JsonElement In ringElem.EnumerateArray()
                    If ptElem.ValueKind = JsonValueKind.Array AndAlso ptElem.GetArrayLength() >= 2 Then
                        Dim x As Double = ptElem(0).GetDouble()
                        Dim y As Double = ptElem(1).GetDouble()
                        punkty.Add(New Punkt2D(x, y))
                    End If
                Next
                If punkty.Count >= 3 Then
                    pierscienie.Add(New PierscienPoligonu(punkty))
                End If
            Next

            If pierscienie.Count = 0 Then Return Nothing
            Return New Wielokat(pierscienie(0), pierscienie.Skip(1))
        End Function

        Private Shared Sub RozpoznajUkladPoligonu(zestaw As ZestawPoligonow, epsgZNaglowka As Integer?)
            If epsgZNaglowka.HasValue Then
                Dim kandydat As RodzinaUkladuPoligonu = CType(epsgZNaglowka.Value, RodzinaUkladuPoligonu)
                If [Enum].IsDefined(Of RodzinaUkladuPoligonu)(kandydat) Then
                    zestaw.WykrytyUklad = kandydat
                    zestaw.ZrodloWykryciaUkladu = String.Format("Nagłówek CRS (EPSG:{0})", epsgZNaglowka.Value)
                    zestaw.NiejednoznaczneOsie = (kandydat = RodzinaUkladuPoligonu.Pl1992 OrElse
                                                  kandydat = RodzinaUkladuPoligonu.Pl2000Strefa5)
                    Return
                End If
            End If

            Dim b As ZakresAABB = zestaw.GlobalnyAabb
            Dim srX As Double = (b.MinWschod + b.MaxWschod) * 0.5
            Dim srY As Double = (b.MinPolnoc + b.MaxPolnoc) * 0.5
            Dim maksAbs As Double = Math.Max(Math.Max(Math.Abs(b.MinWschod), Math.Abs(b.MaxWschod)),
                                             Math.Max(Math.Abs(b.MinPolnoc), Math.Abs(b.MaxPolnoc)))

            If maksAbs <= 180.0 Then
                zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Wgs84
                zestaw.ZrodloWykryciaUkladu = "Heurystyka: WGS84 (stopnie)"
                zestaw.NiejednoznaczneOsie = (srX >= 48.0 AndAlso srX <= 56.0 AndAlso srY >= 13.0 AndAlso srY <= 25.0)
                Return
            End If

            If (srX >= 1400000.0 AndAlso srX <= 2800000.0 AndAlso srY >= 6100000.0 AndAlso srY <= 7600000.0) OrElse
               (srY >= 1400000.0 AndAlso srY <= 2800000.0 AndAlso srX >= 6100000.0 AndAlso srX <= 7600000.0) Then
                zestaw.WykrytyUklad = RodzinaUkladuPoligonu.WebMercator
                zestaw.ZrodloWykryciaUkladu = "Heurystyka: Web Mercator (EPSG:3857)"
                zestaw.NiejednoznaczneOsie = False
                Return
            End If

            If srX >= 5200000.0 AndAlso srY >= 5200000.0 Then
                Dim maksOs As Double = Math.Max(srX, srY)
                If maksOs >= 8000000.0 Then
                    zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Pl2000Strefa8
                    zestaw.NiejednoznaczneOsie = False
                ElseIf maksOs >= 7000000.0 Then
                    zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Pl2000Strefa7
                    zestaw.NiejednoznaczneOsie = False
                ElseIf maksOs >= 6200000.0 Then
                    zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Pl2000Strefa6
                    zestaw.NiejednoznaczneOsie = False
                Else
                    zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Pl2000Strefa5
                    zestaw.NiejednoznaczneOsie = True
                End If
                zestaw.ZrodloWykryciaUkladu = String.Format("Heurystyka: {0} (metrowy z prefiksem strefy)", zestaw.WykrytyUklad)
                Return
            End If

            If (srX >= 100000.0 AndAlso srX <= 900000.0 AndAlso srY >= 5300000.0 AndAlso srY <= 6200000.0) OrElse
               (srY >= 100000.0 AndAlso srY <= 900000.0 AndAlso srX >= 5300000.0 AndAlso srX <= 6200000.0) Then
                zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Utm34N
                zestaw.ZrodloWykryciaUkladu = "Heurystyka: UTM (domyślnie 34N – zweryfikuj strefę)"
                zestaw.NiejednoznaczneOsie = False
                Return
            End If

            zestaw.WykrytyUklad = RodzinaUkladuPoligonu.Pl1992
            zestaw.ZrodloWykryciaUkladu = "Heurystyka: PL-1992 (EPSG:2180) – zweryfikuj kolejność osi [E,N] vs [N,E]"
            zestaw.NiejednoznaczneOsie = True
        End Sub
    End Class
