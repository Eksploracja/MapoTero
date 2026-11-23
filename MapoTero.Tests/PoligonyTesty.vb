Option Strict On
Option Explicit On

Imports System.IO
Imports MapoTero.Core
Imports Xunit

    Public Class PoligonyTesty

        <Fact>
        Public Sub Pierscien_ZamykaNiezamknietyPierscien()
            Dim punkty As Punkt2D() = {
                New Punkt2D(0, 0),
                New Punkt2D(10, 0),
                New Punkt2D(10, 10),
                New Punkt2D(0, 10)
            }
            Dim ring As New PierscienPoligonu(punkty)
            Assert.Equal(5, ring.Wierzcholki.Length)
            Assert.Equal(ring.Wierzcholki(0).Wschod, ring.Wierzcholki(4).Wschod)
            Assert.Equal(ring.Wierzcholki(0).Polnoc, ring.Wierzcholki(4).Polnoc)
            Assert.Equal(0.0, ring.Aabb.MinWschod)
            Assert.Equal(10.0, ring.Aabb.MaxWschod)
        End Sub

        <Fact>
        Public Sub Pierscien_ZawieraPunkt_RayCasting()
            Dim punkty As Punkt2D() = {
                New Punkt2D(0, 0),
                New Punkt2D(10, 0),
                New Punkt2D(10, 10),
                New Punkt2D(0, 10)
            }
            Dim ring As New PierscienPoligonu(punkty)
            Assert.True(ring.ZawieraPunkt(New Punkt2D(5, 5)))
            Assert.False(ring.ZawieraPunkt(New Punkt2D(15, 5)))
            Assert.False(ring.ZawieraPunkt(New Punkt2D(-1, 5)))
        End Sub

        <Fact>
        Public Sub Wielokat_ZDziura_NieZawieraPunktuWDziurze()
            Dim obrys As New PierscienPoligonu({
                New Punkt2D(0, 0),
                New Punkt2D(100, 0),
                New Punkt2D(100, 100),
                New Punkt2D(0, 100)
            })
            Dim dziura As New PierscienPoligonu({
                New Punkt2D(40, 40),
                New Punkt2D(60, 40),
                New Punkt2D(60, 60),
                New Punkt2D(40, 60)
            })
            Dim poly As New Wielokat(obrys, {dziura})

            ' Punkt w obrysie, poza dziurą
            Assert.True(poly.ZawieraPunkt(New Punkt2D(20, 20)))
            ' Punkt wewnątrz dziury
            Assert.False(poly.ZawieraPunkt(New Punkt2D(50, 50)))
            ' Punkt całkowicie na zewnątrz
            Assert.False(poly.ZawieraPunkt(New Punkt2D(120, 50)))
        End Sub

        <Fact>
        Public Sub Wielokat_CzySegmentPrzecina_WykrywaPrzeciecieIJednolityStan()
            Dim obrys As New PierscienPoligonu({
                New Punkt2D(0, 0),
                New Punkt2D(100, 0),
                New Punkt2D(100, 100),
                New Punkt2D(0, 100)
            })
            Dim poly As New Wielokat(obrys, Nothing)

            ' Segment w całości wewnątrz
            Dim segWewnatrz As New ZakresAABB(10, 10, 20, 20)
            Assert.True(poly.CzySegmentPrzecina(segWewnatrz))

            ' Segment przecinający krawędź
            Dim segPrzecinajacy As New ZakresAABB(-10, 50, 10, 70)
            Assert.True(poly.CzySegmentPrzecina(segPrzecinajacy))

            ' Segment całkowicie na zewnątrz
            Dim segZewnatrz As New ZakresAABB(150, 150, 170, 170)
            Assert.False(poly.CzySegmentPrzecina(segZewnatrz))
        End Sub

        <Fact>
        Public Sub ParserGeoJson_IgnorujeWspolrzedna3D_Wysokosc()
            Dim json As String = "{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""geometry"": {
                            ""type"": ""Polygon"",
                            ""coordinates"": [
                                [
                                    [19.0, 50.0, 350.5],
                                    [20.0, 50.0, 420.0],
                                    [20.0, 51.0, 210.2],
                                    [19.0, 51.0, 180.0],
                                    [19.0, 50.0, 350.5]
                                ]
                            ]
                        }
                    }
                ]
            }"

            Dim zestaw As ZestawPoligonow = ParserPoligonu.ParsujGeoJson(json)
            Assert.Single(zestaw.Wielokaty)
            Assert.Equal(5, zestaw.Wielokaty(0).ObrysZewnetrzny.Wierzcholki.Length)
            Assert.Equal(19.0, zestaw.Wielokaty(0).ObrysZewnetrzny.Wierzcholki(0).Wschod)
            Assert.Equal(50.0, zestaw.Wielokaty(0).ObrysZewnetrzny.Wierzcholki(0).Polnoc)
            Assert.Equal(RodzinaUkladuPoligonu.Wgs84, zestaw.WykrytyUklad)
        End Sub

        <Fact>
        Public Sub ParserGeoJson_RozpoznajeNaglowekCrs()
            Dim json As String = "{
                ""type"": ""FeatureCollection"",
                ""crs"": {
                    ""type"": ""name"",
                    ""properties"": {
                        ""name"": ""urn:ogc:def:crs:EPSG::2180""
                    }
                },
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""geometry"": {
                            ""type"": ""Polygon"",
                            ""coordinates"": [
                                [
                                    [500000, 400000],
                                    [510000, 400000],
                                    [510000, 410000],
                                    [500000, 410000],
                                    [500000, 400000]
                                ]
                            ]
                        }
                    }
                ]
            }"

            Dim zestaw As ZestawPoligonow = ParserPoligonu.ParsujGeoJson(json)
            Assert.Equal(RodzinaUkladuPoligonu.Pl1992, zestaw.WykrytyUklad)
            Assert.Contains("EPSG:2180", zestaw.ZrodloWykryciaUkladu)
        End Sub

        <Fact>
        Public Sub ParserGeoJson_Heurystyka_Pl2000_Strefa5_OznaczaNiejednoznacznosc()
            ' Strefa 5 (prefiks 5 na współrzędnej Y, zakres ok. 5.5 mln)
            Dim json As String = "{
                ""type"": ""Polygon"",
                ""coordinates"": [
                    [
                        [5500000, 5700000],
                        [5510000, 5700000],
                        [5510000, 5710000],
                        [5500000, 5710000],
                        [5500000, 5700000]
                    ]
                ]
            }"

            Dim zestaw As ZestawPoligonow = ParserPoligonu.ParsujGeoJson(json)
            Assert.Equal(RodzinaUkladuPoligonu.Pl2000Strefa5, zestaw.WykrytyUklad)
            Assert.True(zestaw.NiejednoznaczneOsie)
        End Sub

        <Fact>
        Public Sub ParserGeoJson_Heurystyka_WebMercator()
            Dim json As String = "{
                ""type"": ""Polygon"",
                ""coordinates"": [
                    [
                        [2100000, 6800000],
                        [2150000, 6800000],
                        [2150000, 6850000],
                        [2100000, 6850000],
                        [2100000, 6800000]
                    ]
                ]
            }"

            Dim zestaw As ZestawPoligonow = ParserPoligonu.ParsujGeoJson(json)
            Assert.Equal(RodzinaUkladuPoligonu.WebMercator, zestaw.WykrytyUklad)
            Assert.False(zestaw.NiejednoznaczneOsie)
        End Sub

        <Fact>
        Public Sub ParserKml_WczytujeOuterIInnerBoundaries()
            Dim kml As String = "<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
  <Document>
    <Placemark>
      <Polygon>
        <outerBoundaryIs>
          <LinearRing>
            <coordinates>
              19.0,50.0,0 20.0,50.0,0 20.0,51.0,0 19.0,51.0,0 19.0,50.0,0
            </coordinates>
          </LinearRing>
        </outerBoundaryIs>
        <innerBoundaryIs>
          <LinearRing>
            <coordinates>
              19.4,50.4,0 19.6,50.4,0 19.6,50.6,0 19.4,50.6,0 19.4,50.4,0
            </coordinates>
          </LinearRing>
        </innerBoundaryIs>
      </Polygon>
    </Placemark>
  </Document>
</kml>"

            Dim zestaw As ZestawPoligonow = ParserPoligonu.ParsujKml(kml)
            Assert.Single(zestaw.Wielokaty)
            Dim poly As Wielokat = zestaw.Wielokaty(0)
            Assert.Equal(5, poly.ObrysZewnetrzny.Wierzcholki.Length)
            Assert.Single(poly.Dziury)
            Assert.Equal(5, poly.Dziury(0).Wierzcholki.Length)

            ' Punkt w obrysie, poza dziurą
            Assert.True(poly.ZawieraPunkt(New Punkt2D(19.2, 50.2)))
            ' Punkt w dziurze
            Assert.False(poly.ZawieraPunkt(New Punkt2D(19.5, 50.5)))
        End Sub

        <Fact>
        Public Sub Wydajnosc_TysiaceWierzcholkow_SzybkiTestPrzeciecia()
            ' Symulacja granicy z PRG o 5000 wierzchołkach
            Const n As Integer = 5000
            Dim punkty(n) As Punkt2D
            Dim r As Double = 10000.0
            For i As Integer = 0 To n - 1
                Dim kat As Double = (i * 2.0 * Math.PI) / n
                punkty(i) = New Punkt2D(500000.0 + r * Math.Cos(kat), 400000.0 + r * Math.Sin(kat))
            Next
            punkty(n) = punkty(0)

            Dim ring As New PierscienPoligonu(punkty)
            Dim poly As New Wielokat(ring, Nothing)

            ' Sprawdzamy 1000 zapytań o segmenty – powinno zająć ułamek sekundy dzięki pasom
            Dim trafione As Integer = 0
            For k As Integer = 0 To 999
                Dim seg As New ZakresAABB(500000.0 + k * 15.0, 400000.0 + k * 15.0, 500100.0 + k * 15.0, 400100.0 + k * 15.0)
                If poly.CzySegmentPrzecina(seg) Then
                    trafione += 1
                End If
            Next

            Assert.True(trafione > 0)
        End Sub


        <Fact>
        Public Sub ZasiegWUkladzie_ObejmujeWszystkieWierzcholkiPoPrzeliczeniu()
            'Romb w PL-1992: skrajne punkty leżą w środkach boków prostokąta, nie w jego narożnikach
            Dim punkty = {New Punkt2D(500000, 480000), New Punkt2D(510000, 490000), New Punkt2D(500000, 500000), New Punkt2D(490000, 490000)}
            Dim zestaw As New ZestawPoligonow({New Wielokat(New PierscienPoligonu(punkty), Array.Empty(Of PierscienPoligonu)())}) With {
                .WykrytyUklad = RodzinaUkladuPoligonu.Pl1992}
            Dim pl1992 = UkladWspolrzednych.ZKodu(2180)
            Dim pl2000 = UkladWspolrzednych.ZKodu(2178)

            'Ten sam układ: zasięg równy obrysowi
            Dim z = zestaw.ZasiegWUkladzie(pl1992)
            Assert.Equal(480000, z.XDol, 3)
            Assert.Equal(500000, z.XGora, 3)
            Assert.Equal(490000, z.YLewy, 3)
            Assert.Equal(510000, z.YPrawy, 3)

            'Inny układ: każdy wierzchołek leży w zasięgu
            Dim z2 = zestaw.ZasiegWUkladzie(pl2000)
            For Each p In punkty
                Dim g = pl1992.DoWgs84(New PunktXY(p.Polnoc, p.Wschod))
                Dim xy = pl2000.ZWgs84(g)
                Assert.InRange(xy.X, z2.XDol - 0.001, z2.XGora + 0.001)
                Assert.InRange(xy.Y, z2.YLewy - 0.001, z2.YPrawy + 0.001)
            Next
            Assert.True(z2.YLewy > 7000000, "PL-2000 strefa 7 ma współrzędną wschodnią z numerem strefy")
        End Sub

        <Fact>
        Public Sub PunktDoWgs84_WebMercatorIPl1992()
            Dim wm = ZestawPoligonow.PunktDoWgs84(RodzinaUkladuPoligonu.WebMercator, New Punkt2D(2226389.8, 6800125.5))
            Assert.Equal(20.0, wm.Wschod, 4)
            Assert.Equal(52.0, wm.Polnoc, 4)
            'Punkt środkowy PL-1992 (19°E na równoleżniku 52°N) ma Y = 500000
            Dim pl = ZestawPoligonow.PunktDoWgs84(RodzinaUkladuPoligonu.Pl1992, New Punkt2D(500000, 470000))
            Assert.Equal(19.0, pl.Wschod, 4)
        End Sub

    End Class
