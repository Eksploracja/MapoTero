'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Globalization
Imports System.Text
Imports MapoTero.Core
Imports Xunit

Public Class WmtsTesty

    'mianowniki skali zestawu EPSG:2180 Geoportalu (piksel 0,28 mm)
    Private Shared ReadOnly Mianowniki2180 As Double() = {30238155.714285716, 15119077.857142858, 7559538.928571429, 3779769.4642857146,
        1889884.7321428573, 944942.3660714286, 472471.1830357143, 236235.59151785716, 94494.23660714286, 47247.11830357143,
        23623.559151785714, 9449.423660714287, 4724.711830357143, 1889.8847321428573, 944.9423660714286, 472.4711830357143}

    Private Shared Function L(v As Double) As String
        Return v.ToString("R", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Dokument Capabilities: zestaw EPSG:2180 (narożnik "północ wschód", kafle 512 px) i GoogleMapsCompatible.</summary>
    Private Shared Function Capabilities(Optional narozPl As String = "850000.0 100000.0", Optional kvp As Boolean = True,
                                         Optional rest As Boolean = False, Optional limity As Boolean = False) As String
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8""?>")
        sb.Append("<Capabilities xmlns=""http://www.opengis.net/wmts/1.0"" xmlns:ows=""http://www.opengis.net/ows/1.1"" xmlns:xlink=""http://www.w3.org/1999/xlink"" version=""1.0.0"">")
        If kvp Then
            sb.Append("<ows:OperationsMetadata><ows:Operation name=""GetCapabilities""><ows:DCP><ows:HTTP><ows:Get xlink:href=""https://serwer.pl/wmts/caps?""/></ows:HTTP></ows:DCP></ows:Operation>")
            sb.Append("<ows:Operation name=""GetTile""><ows:DCP><ows:HTTP><ows:Get xlink:href=""https://serwer.pl/wmts/orto?"">")
            sb.Append("<ows:Constraint name=""GetEncoding""><ows:AllowedValues><ows:Value>KVP</ows:Value></ows:AllowedValues></ows:Constraint>")
            sb.Append("</ows:Get></ows:HTTP></ows:DCP></ows:Operation></ows:OperationsMetadata>")
        End If
        sb.Append("<Contents>")
        sb.Append("<Layer><ows:Title>Ortofotomapa</ows:Title><ows:Identifier>ORTOFOTOMAPA</ows:Identifier>")
        sb.Append("<Style isDefault=""true""><ows:Identifier>default</ows:Identifier></Style>")
        sb.Append("<Format>image/jpeg</Format><Format>image/png</Format>")
        sb.Append("<TileMatrixSetLink><TileMatrixSet>EPSG:2180</TileMatrixSet>")
        If limity Then
            sb.Append("<TileMatrixSetLimits><TileMatrixLimits><TileMatrix>EPSG:2180:14</TileMatrix><MinTileRow>10</MinTileRow><MaxTileRow>20</MaxTileRow><MinTileCol>30</MinTileCol><MaxTileCol>40</MaxTileCol></TileMatrixLimits>")
            sb.Append("<TileMatrixLimits><TileMatrix>EPSG:2180:13</TileMatrix><MinTileRow>0</MinTileRow><MaxTileRow>99</MaxTileRow><MinTileCol>0</MinTileCol><MaxTileCol>99</MaxTileCol></TileMatrixLimits></TileMatrixSetLimits>")
        End If
        sb.Append("</TileMatrixSetLink>")
        sb.Append("<TileMatrixSetLink><TileMatrixSet>GoogleMapsCompatible</TileMatrixSet></TileMatrixSetLink>")
        If rest Then
            sb.Append("<ResourceURL format=""image/jpeg"" resourceType=""tile"" template=""https://serwer.pl/rest/ORTO/{Style}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}/{Time}.jpg""/>")
            sb.Append("<Dimension><ows:Identifier>Time</ows:Identifier><Default>2024</Default><Value>2024</Value></Dimension>")
        End If
        sb.Append("</Layer>")

        sb.Append("<TileMatrixSet><ows:Identifier>EPSG:2180</ows:Identifier><ows:SupportedCRS>urn:ogc:def:crs:EPSG::2180</ows:SupportedCRS>")
        For i = 0 To Mianowniki2180.Length - 1
            Dim bok As Double = 512 * Mianowniki2180(i) * 0.00028
            sb.Append("<TileMatrix><ows:Identifier>EPSG:2180:" & i & "</ows:Identifier><ScaleDenominator>" & L(Mianowniki2180(i)) & "</ScaleDenominator>")
            sb.Append("<TopLeftCorner>" & narozPl & "</TopLeftCorner><TileWidth>512</TileWidth><TileHeight>512</TileHeight>")
            sb.Append("<MatrixWidth>" & CInt(Math.Ceiling(800000 / bok)) & "</MatrixWidth><MatrixHeight>" & CInt(Math.Ceiling(800000 / bok)) & "</MatrixHeight></TileMatrix>")
        Next
        sb.Append("</TileMatrixSet>")

        sb.Append("<TileMatrixSet><ows:Identifier>GoogleMapsCompatible</ows:Identifier><ows:SupportedCRS>urn:ogc:def:crs:EPSG:6.18.3:3857</ows:SupportedCRS>")
        For z = 0 To 18
            sb.Append("<TileMatrix><ows:Identifier>" & z & "</ows:Identifier><ScaleDenominator>" & L(559082264.0287178 / 2 ^ z) & "</ScaleDenominator>")
            sb.Append("<TopLeftCorner>-20037508.3427892 20037508.3427892</TopLeftCorner><TileWidth>256</TileWidth><TileHeight>256</TileHeight>")
            sb.Append("<MatrixWidth>" & (1 << z) & "</MatrixWidth><MatrixHeight>" & (1 << z) & "</MatrixHeight></TileMatrix>")
        Next
        sb.Append("</TileMatrixSet>")
        sb.Append("</Contents></Capabilities>")
        Return sb.ToString()
    End Function

    Private Shared ReadOnly Obszar As New Zasieg(480000, 630000, 482000, 632000)   'okolice Warszawy w PL-1992

    <Fact>
    Public Sub OdczytCapabilities()
        Dim u = UslugaWmts.Wczytaj(Capabilities(), "https://serwer.pl/wmts?SERVICE=WMTS&REQUEST=GetCapabilities")
        Assert.Single(u.Warstwy)
        Dim w = u.Warstwa("ORTOFOTOMAPA")
        Assert.Equal("Ortofotomapa", w.Tytul)
        Assert.Equal("default", w.Styl)
        Assert.Equal("image/jpeg,image/png", String.Join(",", w.Formaty))
        Assert.Equal(2, w.Powiazania.Count)
        Assert.Equal(2, u.Zestawy.Count)
        Assert.Equal(16, u.Zestaw("EPSG:2180").Macierze.Count)
        Assert.Equal(512, u.Zestaw("EPSG:2180").Macierze(0).SzerokoscKafla)
        Assert.Equal(19, u.Zestaw("GoogleMapsCompatible").Macierze.Count)
        Assert.Equal("https://serwer.pl/wmts/orto?", u.AdresKvp)
        Assert.Equal(2180, u.Zestaw("EPSG:2180").Uklad.Epsg)
        Assert.True(u.Zestaw("GoogleMapsCompatible").Uklad.JestWebMercator)
    End Sub

    <Fact>
    Public Sub DokumentInnyNizCapabilities()
        Dim ex = Assert.Throws(Of FormatException)(
            Sub() UslugaWmts.Wczytaj("<ExceptionReport><Exception><ExceptionText>brak usługi</ExceptionText></Exception></ExceptionReport>", ""))
        Assert.Contains("brak usługi", ex.Message)
    End Sub

    <Fact>
    Public Sub PlanWUkladzieSegmentow()
        Dim u = UslugaWmts.Wczytaj(Capabilities(), "")
        Dim p As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.25, "jpeg")
        Assert.Equal("EPSG:2180", p.Zestaw.Identyfikator)
        Assert.True(p.BezPrzeliczania)
        Assert.False(p.OsieZamienione)
        'piksel 0,25 m: poziom 14 (0,2646 m) mieści się w tolerancji 10%, poziom 15 (0,1323 m) byłby niepotrzebnie dokładny
        Assert.Equal("EPSG:2180:14", p.Macierz.Identyfikator)
        Assert.Equal(0.264583, p.Rozdzielczosc, 5)
        Assert.Equal(1, p.Nadprobkowanie)
        Assert.Equal("image/jpeg", p.Format)
        Assert.Equal(850000.0, p.Naroznik.X)
        Assert.Equal(100000.0, p.Naroznik.Y)
        Dim kol As Double, wie As Double
        p.PikselMacierzy(New PunktXY(850000 - 0.264583 * 1024, 100000 + 0.264583 * 512), kol, wie)
        Assert.Equal(512, kol, 2)
        Assert.Equal(1024, wie, 2)
        Assert.Equal("https://serwer.pl/wmts/orto?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ORTOFOTOMAPA&STYLE=default&FORMAT=image%2Fjpeg" &
                     "&TILEMATRIXSET=EPSG%3A2180&TILEMATRIX=EPSG%3A2180%3A14&TILEROW=7&TILECOL=9", p.AdresKafla(7, 9))
        'segmenty PNG - kafle PNG
        Assert.Equal("image/png", New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.25, "png").Format)
        'piksel 1 m: poziom 13 (0,529 m) - nadpróbkowanie 2 x 2
        Dim p1 As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 1, "jpeg")
        Assert.Equal("EPSG:2180:13", p1.Macierz.Identyfikator)
        Assert.Equal(2, p1.Nadprobkowanie)
    End Sub

    <Fact>
    Public Sub NaroznikWOdwrotnejKolejnosci()
        'serwer niezgodny ze standardem: narożnik EPSG:2180 w kolejności "wschód północ"
        Dim u = UslugaWmts.Wczytaj(Capabilities("100000.0 850000.0"), "")
        Dim p As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.25, "jpeg")
        Assert.Equal("EPSG:2180", p.Zestaw.Identyfikator)
        Assert.True(p.OsieZamienione)
        Assert.Equal(850000.0, p.Naroznik.X)
        Assert.Equal(100000.0, p.Naroznik.Y)
    End Sub

    <Fact>
    Public Sub PlanWebMercatorZgodnyZeWzoremKafli()
        'segmenty w UTM 34N - brak zestawu w tym układzie, wybór GoogleMapsCompatible z przeliczaniem
        Dim u = UslugaWmts.Wczytaj(Capabilities(), "")
        Dim obszarUtm = UkladWspolrzednych.PL1992.PrzeliczZasieg(UkladWspolrzednych.Utm34N, Obszar)
        Dim p As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.Utm34N, obszarUtm, 1, "jpeg")
        Assert.Equal("GoogleMapsCompatible", p.Zestaw.Identyfikator)
        Assert.False(p.BezPrzeliczania)
        'piksel 1 m na szerokości 52°: poziom 17 ma 1,19 m w układzie EPSG:3857, czyli ok. 0,73 m w terenie
        Dim z As Integer = Integer.Parse(p.Macierz.Identyfikator, CultureInfo.InvariantCulture)
        Assert.Equal(17, z)
        'położenie punktu w pikselach macierzy = standardowy wzór kafli OpenStreetMap
        Dim g As New PunktGeo(52.2297, 21.0122)
        Dim kol As Double, wie As Double
        p.PikselMacierzy(p.DoUkladuKafli(UkladWspolrzednych.Utm34N.ZWgs84(g)), kol, wie)
        Assert.Equal(WebMercator.PikselX(g.Dlugosc, z), kol, 3)
        Assert.Equal(WebMercator.PikselY(g.Szerokosc, z), wie, 3)
        Assert.Equal("https://serwer.pl/wmts/orto?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ORTOFOTOMAPA&STYLE=default&FORMAT=image%2Fjpeg" &
                     "&TILEMATRIXSET=GoogleMapsCompatible&TILEMATRIX=17&TILEROW=3&TILECOL=5", p.AdresKafla(3, 5))
    End Sub

    <Fact>
    Public Sub SzablonRest()
        Dim u = UslugaWmts.Wczytaj(Capabilities(kvp:=False, rest:=True), "https://serwer.pl/WMTSCapabilities.xml")
        Assert.Equal("", u.AdresKvp)
        Dim p As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.25, "png")
        Assert.Equal("image/jpeg", p.Format)
        Assert.Equal("https://serwer.pl/rest/ORTO/default/EPSG:2180/EPSG:2180:14/11/12/2024.jpg", p.AdresKafla(11, 12))
    End Sub

    <Fact>
    Public Sub OgraniczeniaZakresuKafli()
        Dim u = UslugaWmts.Wczytaj(Capabilities(limity:=True), "")
        Dim p As New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.25, "jpeg")
        Assert.Equal("EPSG:2180:14", p.Macierz.Identyfikator)
        Assert.True(p.KafelDostepny(10, 30))
        Assert.True(p.KafelDostepny(20, 40))
        Assert.False(p.KafelDostepny(9, 30))
        Assert.False(p.KafelDostepny(15, 41))
        'poziom 15 nie jest wymieniony w ograniczeniach - niedostępny dla warstwy; piksel 0,1 m: najdokładniejszy dostępny to 14
        Assert.Equal("EPSG:2180:14", New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, Obszar, 0.1, "jpeg").Macierz.Identyfikator)
        Assert.False(p.KafelDostepny(-1, 0))
    End Sub

    <Fact>
    Public Sub BrakWarstwyIObszarPozaZasiegiem()
        Dim u = UslugaWmts.Wczytaj(Capabilities(), "")
        Dim ex = Assert.Throws(Of InvalidOperationException)(Function() New PlanWmts(u, "ORTO", UkladWspolrzednych.PL1992, Obszar, 0.25, "jpeg"))
        Assert.Contains("ORTOFOTOMAPA", ex.Message)
        'obszar spoza zestawu EPSG:2180 (na zachód od narożnika) - wybrany zostaje zestaw Web Mercator
        Dim daleko As New Zasieg(480000, 50000, 482000, 52000)
        Assert.Equal("GoogleMapsCompatible", New PlanWmts(u, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, daleko, 0.25, "jpeg").Zestaw.Identyfikator)
    End Sub

    <Theory>
    <InlineData("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution", True)>
    <InlineData("https://serwer.pl/geoserver/gwc/service/wmts?SERVICE=WMTS&REQUEST=GetCapabilities", True)>
    <InlineData("https://serwer.pl/1.0.0/WMTSCapabilities.xml", True)>
    <InlineData("http://mapy.geoportal.gov.pl/wss/service/img/guest/ORTO/MapServer/WMSServer?request=GetMap&layers=", False)>
    <InlineData("https://serwer.pl/wms?SERVICE=WMS&layers=wmts_podklad", False)>
    Public Sub RozpoznanieAdresuWmts(adres As String, oczekiwane As Boolean)
        Assert.Equal(oczekiwane, Wmts.CzyAdresWmts(adres))
    End Sub

    <Fact>
    Public Sub AdresyUslugi()
        Assert.Equal("https://s.pl/WMTS/Std?SERVICE=WMTS&REQUEST=GetCapabilities", Wmts.AdresCapabilities("https://s.pl/WMTS/Std"))
        Assert.Equal("https://s.pl/wmts?SERVICE=WMTS&REQUEST=GetCapabilities", Wmts.AdresCapabilities("https://s.pl/wmts?service=WMTS&request=GetCapabilities&version=1.0.0"))
        Assert.Equal("https://s.pl/x/WMTSCapabilities.xml", Wmts.AdresCapabilities("https://s.pl/x/WMTSCapabilities.xml"))
        Assert.Equal("https://s.pl/wmts?map=orto&", Wmts.AdresBazowy("https://s.pl/wmts?SERVICE=WMTS&map=orto&REQUEST=GetCapabilities"))
        Assert.Equal("https://s.pl/wmts?", Wmts.AdresBazowy("https://s.pl/wmts?"))
    End Sub

    <Theory>
    <InlineData("urn:ogc:def:crs:EPSG::2180", 2180, True)>
    <InlineData("EPSG:2177", 2177, True)>
    <InlineData("http://www.opengis.net/def/crs/EPSG/0/32634", 32634, False)>
    <InlineData("urn:ogc:def:crs:EPSG:6.18.3:3857", 3857, False)>
    <InlineData("EPSG:900913", 3857, False)>
    <InlineData("urn:ogc:def:crs:OGC:1.3:CRS84", 4326, False)>
    <InlineData("urn:ogc:def:crs:EPSG::4326", 4326, True)>
    Public Sub OznaczeniaUkladow(crs As String, epsg As Integer, najpierwPolnoc As Boolean)
        Dim u = UkladKafliWmts.ZOznaczenia(crs)
        Assert.NotNull(u)
        Assert.Equal(epsg, u.Epsg)
        Assert.Equal(najpierwPolnoc, u.NajpierwPolnoc)
    End Sub

    <Fact>
    Public Sub UkladyNieobslugiwane()
        Assert.Null(UkladKafliWmts.ZOznaczenia("EPSG:31467"))
        Assert.Null(UkladKafliWmts.ZOznaczenia("urn:ogc:def:crs:EPSG::"))
        Assert.Null(UkladKafliWmts.ZOznaczenia(""))
    End Sub

    <Fact>
    Public Sub WebMercatorTamIZPowrotem()
        Dim u = UkladKafliWmts.ZOznaczenia("EPSG:3857")
        Dim p = u.ZWgs84(New PunktGeo(52.2297, 21.0122))
        'wartości wzorcowe: PROJ (EPSG:4326 -> EPSG:3857)
        Assert.Equal(2339067.404, p.Y, 2)
        Assert.Equal(6841765.197, p.X, 2)
        Dim g = u.DoWgs84(p)
        Assert.Equal(52.2297, g.Szerokosc, 9)
        Assert.Equal(21.0122, g.Dlugosc, 9)
    End Sub

End Class
