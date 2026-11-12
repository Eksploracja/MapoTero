'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core
Imports Xunit

Public Class XyzTesty

    Private NotInheritable Class MockHttpHandler
        Inherits HttpMessageHandler

        Private ReadOnly _odpowiedz As Func(Of Uri, HttpResponseMessage)
        Public ReadOnly Property OdpytywaneAdresy As New List(Of String)

        Public Sub New(odpowiedz As Func(Of Uri, HttpResponseMessage))
            _odpowiedz = odpowiedz
        End Sub

        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            SyncLock OdpytywaneAdresy
                OdpytywaneAdresy.Add(request.RequestUri.ToString())
            End SyncLock
            Return Task.FromResult(_odpowiedz(request.RequestUri))
        End Function
    End Class

    Private Shared Function PngBytes(bok As Integer) As Byte()
        Using bmp As New Bitmap(bok, bok, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.ForestGreen)
            End Using
            Using ms As New MemoryStream()
                bmp.Save(ms, ImageFormat.Png)
                Return ms.ToArray()
            End Using
        End Using
    End Function

    <Fact>
    Public Sub RozpoznawanieTypu_XyzPrzedWmtsITmsZMinusY()
        Assert.True(KafleXyz.CzyAdresXyz("https://tile.waymarkedtrails.org/{layer}/{z}/{x}/{y}.png"))
        Assert.False(KafleXyz.CzyAdresTms("https://tile.waymarkedtrails.org/{layer}/{z}/{x}/{y}.png"))
        Assert.True(KafleXyz.CzyAdresTms("https://serwer.example/tms/{z}/{x}/{-y}.png"))

        'adres XYZ zawierający "/wmts/" w ścieżce nie może trafić do GetCapabilities WMTS
        Assert.Equal(TypUslugiMapowej.Xyz, KafleXyz.WybierzTyp("https://serwer.example/wmts/{z}/{x}/{y}.png"))
        Assert.Equal(TypUslugiMapowej.Tms, KafleXyz.WybierzTyp("https://serwer.example/wmts/{z}/{x}/{-y}.png"))
        Assert.Equal(TypUslugiMapowej.Wmts, KafleXyz.WybierzTyp("https://mapy.geoportal.gov.pl/wss/service/WMTS/guest/wmts/TOPO"))
        'jawny typ z pliku zbioru ma pierwszeństwo
        Assert.Equal(TypUslugiMapowej.Tms, KafleXyz.WybierzTyp("https://serwer.example/{z}/{x}/{y}.png", TypUslugiMapowej.Tms))
    End Sub

    <Theory>
    <InlineData("https://{a|b|c}.tile.openstreetmap.pl/{z}/{x}/{y}.png", "https://{s}.tile.openstreetmap.pl/{z}/{x}/{y}.png", "a,b,c")>
    <InlineData("http://{1,2,3}.tiles.ump.waw.pl/{z}/{x}/{y}.png", "http://{s}.tiles.ump.waw.pl/{z}/{x}/{y}.png", "1,2,3")>
    <InlineData("https://{switch:s1,s2}.tiles.example/{z}/{x}/{y}.png", "https://{s}.tiles.example/{z}/{x}/{y}.png", "s1,s2")>
    Public Sub NormalizujSzablon_WyciagaSubdomeny(wejscie As String, oczekiwanySzablon As String, oczekiwaneSub As String)
        Dim subdomeny As List(Of String) = Nothing
        Dim tms As Boolean = False
        Dim wynik = KafleXyz.NormalizujSzablon(wejscie, subdomeny, tms)
        Assert.Equal(oczekiwanySzablon, wynik)
        Assert.Equal(oczekiwaneSub.Split(","c), subdomeny)
    End Sub

    <Fact>
    Public Sub SyntetycznaUslugaIPlanWmts_SubdomenyDeterministyczneIKluczCacheBezSubdomeny()
        Dim opcje As New OpcjeKafliXyz With {.Typ = TypUslugiMapowej.Xyz, .ZMin = 0, .ZMax = 18, .RozmiarKafla = 512}
        opcje.Subdomeny.AddRange({"a", "b", "c"})
        Dim usluga = KafleXyz.UtworzUsluge("https://{s}.tile.example/{layer}/{z}/{x}/{y}.png", {"hiking"}, opcje)
        Dim plan As New PlanWmts(usluga, "hiking", UkladWspolrzednych.PL1992, New Zasieg(480000, 580000, 482000, 582000), 1.0, "png")

        '512 px podwaja rozdzielczość kafla w stosunku do 256 px (mianownik dwukrotnie mniejszy dla tego samego z)
        Dim macierz10 = usluga.Zestawy(0).Macierze.First(Function(m) m.Identyfikator = "10")
        Assert.Equal(512, macierz10.SzerokoscKafla)
        Assert.Equal(2.0 * Math.PI * 6378137.0 / (512.0 * 1024.0) / 0.00028, macierz10.MianownikSkali, 6)

        'klucz cache zachowuje {s}, adres wybiera deterministyczną subdomenę wg (wiersz + kolumna) mod N
        Assert.Contains("/{s}.tile.example/hiking/", plan.KluczKafla(5, 7))
        Dim a1 = plan.AdresKafla(5, 7)
        Dim a2 = plan.AdresKafla(5, 7)
        Assert.Equal(a1, a2)
        Assert.Equal("https://a.tile.example/hiking/" & plan.Macierz.Identyfikator & "/7/5.png", a1) '5+7=12 -> mod 3 = 0 -> "a"
        Assert.Contains("b.tile.example", plan.AdresKafla(5, 8)) '5+8=13 -> mod 3 = 1 -> "b"
        Assert.StartsWith("XYZ ", plan.Opis)
    End Sub

    <Fact>
    Public Sub SyntetycznaUslugaTms_OdwracaOsYIZasiegOgraniczaKafle()
        Dim opcje As New OpcjeKafliXyz With {
            .Typ = TypUslugiMapowej.Tms,
            .ZMin = 10,
            .ZMax = 12,
            .ZasiegGeo = New ProstokatGeo(52.3, 52.1, 21.2, 20.9)
        }
        Dim usluga = KafleXyz.UtworzUsluge("https://tms.example/{z}/{x}/{-y}.png", {"warstwa"}, opcje)
        Dim plan As New PlanWmts(usluga, "warstwa", UkladWspolrzednych.PL1992, New Zasieg(480000, 630000, 482000, 632000), 50.0, "png")
        Dim zNum As Integer = Integer.Parse(plan.Macierz.Identyfikator, Globalization.CultureInfo.InvariantCulture)
        Dim maks As Integer = (1 << zNum) - 1
        Dim url = plan.AdresKafla(10, 20)
        Assert.EndsWith("/" & zNum & "/20/" & (maks - 10) & ".png", url)
        'poza granicami Polski kafel jest oznaczony jako niedostępny
        Assert.False(plan.KafelDostepny(0, 0))
    End Sub

    <Fact>
    Public Sub SzacujLiczbeKafli_ZliczaIZatrzymujeNaLimicie()
        Dim usluga = KafleXyz.UtworzUsluge("https://tile.example/{z}/{x}/{y}.png", {"m"})
        Dim siatka As New Siatka(New Zasieg(480000, 580000, 490000, 590000), 2.0, 1000)
        Dim plan As New PlanWmts(usluga, "m", UkladWspolrzednych.PL1992, siatka.ZasiegSiatki, siatka.RozmiarPiksela, "png")

        Dim pelna = KafleXyz.SzacujLiczbeKafli({plan}, siatka, UkladWspolrzednych.PL1992, limitZliczania:=100000)
        Assert.InRange(pelna, 10L, 10000L)
        Dim ucieta = KafleXyz.SzacujLiczbeKafli({plan}, siatka, UkladWspolrzednych.PL1992, limitZliczania:=5)
        Assert.InRange(ucieta, 5L, pelna)
    End Sub

    <Fact>
    Public Sub ParsujTileJsonIOsGeoTmsXml_OdczytujeMetadaneIZakres()
        Dim json = "{
          ""tilejson"": ""3.0.0"",
          ""name"": ""Moja Mapa"",
          ""tiles"": [""https://{a|b}.tiles.example/{z}/{x}/{y}.png""],
          ""minzoom"": 2,
          ""maxzoom"": 17,
          ""scheme"": ""tms"",
          ""bounds"": [14.1, 49.0, 24.1, 54.8],
          ""attribution"": ""<a href='https://osm.org'>OpenStreetMap</a>""
        }"
        Dim tj = KafleXyz.ParsujTileJson(json)
        Assert.Equal("Moja Mapa", tj.Nazwa)
        Assert.Equal("https://{s}.tiles.example/{z}/{x}/{y}.png", tj.Szablon)
        Assert.Equal({"a", "b"}, tj.Opcje.Subdomeny)
        Assert.Equal(TypUslugiMapowej.Tms, tj.Opcje.Typ)
        Assert.Equal(2, tj.Opcje.ZMin)
        Assert.Equal(17, tj.Opcje.ZMax)
        Assert.Equal("OpenStreetMap", tj.Opcje.Atrybucja)
        Assert.True(tj.Opcje.ZasiegGeo.HasValue)
        Assert.InRange(tj.Opcje.ZasiegGeo.Value.Polnoc, 54.79, 54.81)

        Dim xml = "<TileMap version=""1.0.0"" tilemapservice=""https://example.org/tms/1.0.0"">
          <Title>Kraków 1944</Title>
          <SRS>OSGEO:41001</SRS>
          <BoundingBox minx=""19.8"" miny=""50.0"" maxx=""20.1"" maxy=""50.2""/>
          <TileFormat width=""256"" height=""256"" mime-type=""image/png"" extension=""png""/>
          <TileSets>
            <TileSet href=""https://example.org/tms/1.0.0/krakow/10"" units-per-pixel=""152.87"" order=""10""/>
            <TileSet href=""https://example.org/tms/1.0.0/krakow/15"" units-per-pixel=""4.77"" order=""15""/>
          </TileSets>
        </TileMap>"
        Dim tms = KafleXyz.ParsujTmsXml(xml)
        Assert.Equal("Kraków 1944", tms.Tytul)
        Assert.Equal("https://example.org/tms/1.0.0/krakow/{z}/{x}/{-y}.png", tms.Szablon)
        Assert.Equal(10, tms.Opcje.ZMin)
        Assert.Equal(15, tms.Opcje.ZMax)

        Dim nieMercator = "<TileMap><SRS>EPSG:2180</SRS><TileSets><TileSet order=""0""/></TileSets></TileMap>"
        Assert.Throws(Of FormatException)(Function() KafleXyz.ParsujTmsXml(nieMercator))
    End Sub

    <Fact>
    Public Sub DiagnozujOdpowiedzKafla_WykrywaWebpMvtHtmlIPoprawneRozmiary()
        Dim png512 = PngBytes(512)
        Dim w As Integer, h As Integer
        Assert.Equal("", KafleXyz.DiagnozujOdpowiedzKafla(png512, "image/png", w, h))
        Assert.Equal(512, w)
        Assert.Equal(512, h)

        Dim webp = Encoding.ASCII.GetBytes("RIFF" & ChrW(0) & ChrW(0) & ChrW(0) & ChrW(0) & "WEBPVP8 ")
        Assert.Contains("WebP", KafleXyz.DiagnozujOdpowiedzKafla(webp, "image/webp"))

        Dim mvt = New Byte() {&H1A, &H2, &H1, &H2}
        Assert.Contains("MVT", KafleXyz.DiagnozujOdpowiedzKafla(mvt, "application/vnd.mapbox-vector-tile"))
        Assert.Contains("MVT", KafleXyz.DiagnozujOdpowiedzKafla(mvt, "application/octet-stream", adres:="https://s/{z}/{x}/{y}.pbf"))

        Dim html = Encoding.UTF8.GetBytes("<!DOCTYPE html><html><body>502</body></html>")
        Assert.Contains("HTML", KafleXyz.DiagnozujOdpowiedzKafla(html, "text/html"))
    End Sub

    <Fact>
    Public Sub MaskowanieSekretowIAtrybucjaDlaNowychZrodel()
        Assert.Equal("https://tiles.example/m/10/5/7.png?key=***&token=***",
                     MetadaneMapy.MaskujSekrety("https://tiles.example/m/10/5/7.png?key=tajny123&token=abc987"))
        Assert.Equal("https://user:***@tiles.example/10/5/7.png",
                     MetadaneMapy.MaskujSekrety("https://user:haslo@tiles.example/10/5/7.png"))

        Assert.Contains("Waymarked Trails", MetadaneMapy.ZrodloDanych("https://tile.waymarkedtrails.org/hiking/{z}/{x}/{y}.png"))
        Assert.Contains("OSMapa.pl", MetadaneMapy.ZrodloDanych("https://a.tile.openstreetmap.pl/osmapa.pl/{z}/{x}/{y}.png"))
        Assert.Contains("UMP-pcPL", MetadaneMapy.ZrodloDanych("http://1.tiles.ump.waw.pl/ump_tiles/{z}/{x}/{y}.png"))
        Assert.Equal("Własna atrybucja", MetadaneMapy.ZrodloDanych("https://inna.example/{z}/{x}/{y}.png", "Własna atrybucja"))
    End Sub

    <Fact>
    Public Async Function KlientCapabilities_SondaXyzTms512PxOrazTileJson() As Task
        Dim kafel512 = PngBytes(512)
        'symulacja serwera TMS z zoomem 4..16 i kafelkami 512x512 px (zwykły {y} daje 404, TMS {-y} działa)
        Using h As New MockHttpHandler(
            Function(uri)
                Dim czesci = uri.AbsolutePath.Trim("/"c).Split("/"c)
                Dim z As Integer = Integer.Parse(czesci(1))
                Dim y As Integer = Integer.Parse(czesci(3).Replace(".png", ""))
                Dim maks As Integer = (1 << z) - 1
                'środek Polski ma w XYZ wiersz ~maks*0.33, w TMS (~maks*0.67)
                Dim jestTmsY As Boolean = (z = 0) OrElse (y > maks \ 2)
                If z >= 4 AndAlso z <= 16 AndAlso jestTmsY Then
                    Dim r As New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New ByteArrayContent(kafel512)}
                    r.Content.Headers.ContentType = New Headers.MediaTypeHeaderValue("image/png")
                    Return r
                End If
                Return New HttpResponseMessage(HttpStatusCode.NotFound)
            End Function)

            Dim wynik = Await KlientCapabilities.PobierzAsync("https://tiles.example/mapa/{z}/{x}/{y}.png", handler:=h, srodekTestowy:=New PunktGeo(52.23, 21.01))
            Assert.True(wynik.Sukces, wynik.KomunikatBledu)
            Assert.Equal(TypUslugiMapowej.Tms, wynik.Typ)
            Assert.Equal(4, wynik.OpcjeKafli.ZMin)
            Assert.Equal(16, wynik.OpcjeKafli.ZMax)
            Assert.Equal(512, wynik.OpcjeKafli.RozmiarKafla)
            Assert.NotNull(wynik.PrzykladowyKafel)
            Assert.Single(wynik.UslugaWmts.Warstwy)
        End Using

        'TileJSON z podglądem
        Dim kafel256 = PngBytes(256)
        Using hJson As New MockHttpHandler(
            Function(uri)
                If uri.AbsolutePath.EndsWith("tiles.json") Then
                    Dim json = "{""name"":""Szlak"",""tiles"":[""https://tiles.example/{z}/{x}/{y}.png""],""minzoom"":1,""maxzoom"":15}"
                    Return New HttpResponseMessage(HttpStatusCode.OK) With {
                        .Content = New StringContent(json, Encoding.UTF8, "application/json")
                    }
                End If
                Dim r As New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New ByteArrayContent(kafel256)}
                r.Content.Headers.ContentType = New Headers.MediaTypeHeaderValue("image/png")
                Return r
            End Function)
            Dim wJson = Await KlientCapabilities.PobierzAsync("https://tiles.example/tiles.json", handler:=hJson)
            Assert.True(wJson.Sukces, wJson.KomunikatBledu)
            Assert.Equal(TypUslugiMapowej.Xyz, wJson.Typ)
            Assert.Equal("Szlak", wJson.Tytul)
            Assert.Equal(1, wJson.OpcjeKafli.ZMin)
            Assert.Equal(15, wJson.OpcjeKafli.ZMax)
            Assert.NotNull(wJson.PrzykladowyKafel)
        End Using

        'WebP powinien zostać od razu odrzucony czytelnym komunikatem
        Using hWebp As New MockHttpHandler(
            Function(uri)
                Dim webp = Encoding.ASCII.GetBytes("RIFF" & ChrW(0) & ChrW(0) & ChrW(0) & ChrW(0) & "WEBPVP8 ")
                Dim r As New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New ByteArrayContent(webp)}
                r.Content.Headers.ContentType = New Headers.MediaTypeHeaderValue("image/webp")
                Return r
            End Function)
            Dim wWebp = Await KlientCapabilities.PobierzAsync("https://tiles.example/{z}/{x}/{y}.webp", handler:=hWebp)
            Assert.False(wWebp.Sukces)
            Assert.Contains("WebP", wWebp.KomunikatBledu)
        End Using
    End Function

    <Fact>
    Public Async Function SugerowanyPikselIOpisFormatuBezRozszerzenia() As Task
        'zMax = 15 (np. dane 10 m) -> sugerowany piksel ~2,9 m (nie domyślne 1,0 m)
        Dim u15 = KafleXyz.UtworzUsluge("https://sat.example/{z}/{x}/{y}.jpg", {"sat"},
                                        New OpcjeKafliXyz With {.Typ = TypUslugiMapowej.Xyz, .ZMin = 0, .ZMax = 15})
        Dim px15 = KafleXyz.SugerowanyRozmiarPiksela(u15, u15.Warstwy(0))
        Assert.InRange(px15, 2.6, 3.5)

        'zMax = 18 (standardowa mapa drogowa/turystyczna) -> nadal 1,0 m
        Dim u18 = KafleXyz.UtworzUsluge("https://osm.example/{z}/{x}/{y}.png", {"osm"},
                                        New OpcjeKafliXyz With {.Typ = TypUslugiMapowej.Xyz, .ZMin = 0, .ZMax = 18})
        Assert.Equal(1.0, KafleXyz.SugerowanyRozmiarPiksela(u18, u18.Warstwy(0)))

        'zMax = 12 (przeglądowa BDOO) -> 25,0 m
        Dim u12 = KafleXyz.UtworzUsluge("https://overview.example/{z}/{x}/{y}.png", {"m"},
                                        New OpcjeKafliXyz With {.Typ = TypUslugiMapowej.Xyz, .ZMin = 0, .ZMax = 12})
        Assert.Equal(25.0, KafleXyz.SugerowanyRozmiarPiksela(u12, u12.Warstwy(0)))

        'URL bez rozszerzenia (np. Esri .../tile/{z}/{y}/{x}) zwracający JPEG rozpoznawany z bajtów
        Dim jpeg() As Byte
        Using bmp As New Bitmap(256, 256)
            Using ms As New MemoryStream()
                bmp.Save(ms, ImageFormat.Jpeg)
                jpeg = ms.ToArray()
            End Using
        End Using
        Using hJpg As New MockHttpHandler(
            Function(uri)
                Dim r As New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New ByteArrayContent(jpeg)}
                r.Content.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/octet-stream")
                Return r
            End Function)
            Dim wJpg = Await KlientCapabilities.PobierzAsync("https://esri.example/tile/{z}/{y}/{x}", handler:=hJpg)
            Assert.True(wJpg.Sukces, wJpg.KomunikatBledu)
            Assert.Equal("image/jpeg", wJpg.OpcjeKafli.FormatObrazu)
            Assert.Equal({"image/jpeg"}, wJpg.UslugaWmts.Warstwy(0).Formaty)
        End Using

        'opis do dymka w drzewie warstw
        Dim opis = KafleXyz.OpisWarstwy(TypUslugiMapowej.Xyz, New OpcjeKafliXyz With {
            .Typ = TypUslugiMapowej.Xyz, .ZMin = 0, .ZMax = 18, .Nakladka = True, .RozmiarKafla = 512, .IgnorujSsl = True
        })
        Assert.Contains("XYZ", opis)
        Assert.Contains("0..18", opis)
        Assert.Contains("nakładka", opis)
        Assert.Contains("512 px", opis)
        Assert.Contains("SSL", opis)
    End Function

    <Fact>
    Public Sub QuadKeyIParametrR_AdresKaflaBingIBezPrzyrostkaRetina()
        'przykład z dokumentacji Bing Maps: z=3, x=3, y=5 -> 213
        Assert.Equal("213", KafleXyz.QuadKey(3, 3, 5))
        Assert.Equal("", KafleXyz.QuadKey(0, 0, 0))
        Assert.True(KafleXyz.CzyAdresXyz("https://t.example/tiles/a{quadkey}.jpeg"))

        Dim opcje As New OpcjeKafliXyz With {.ZMin = 3, .ZMax = 3}
        Dim usluga = KafleXyz.UtworzUsluge("https://t.example/tiles/a{quadkey}{r}.jpeg", {"mapa"}, opcje)
        Dim plan As New PlanWmts(usluga, "mapa", UkladWspolrzednych.PL1992, New Zasieg(480000, 580000, 482000, 582000), 1.0, "jpeg")
        Assert.Equal("https://t.example/tiles/a213.jpeg", plan.AdresKafla(5, 3))

        'szablon {z}/{x}/{y}{r} - {r} znika, program pobiera kafle zwykłej gęstości
        Dim uslugaR = KafleXyz.UtworzUsluge("https://t.example/{z}/{x}/{y}{r}.png", {"mapa"}, opcje)
        Dim planR As New PlanWmts(uslugaR, "mapa", UkladWspolrzednych.PL1992, New Zasieg(480000, 580000, 482000, 582000), 1.0, "png")
        Assert.Equal("https://t.example/3/3/5.png", planR.AdresKafla(5, 3))
    End Sub

    <Fact>
    Public Sub KafelNadrzedny_WskazujeKafelPoziomWyzejICzescDoPowiekszenia()
        Dim opcje As New OpcjeKafliXyz With {.ZMin = 0, .ZMax = 18}
        Dim usluga = KafleXyz.UtworzUsluge("https://t.example/{z}/{x}/{y}.png", {"mapa"}, opcje)
        Dim plan As New PlanWmts(usluga, "mapa", UkladWspolrzednych.PL1992, New Zasieg(480000, 580000, 482000, 582000), 1.0, "png")
        Dim z As Integer = Integer.Parse(plan.Macierz.Identyfikator, Globalization.CultureInfo.InvariantCulture)

        Dim n1 = plan.KafelNadrzedny(43011, 72345, 1)
        Assert.Equal("https://t.example/" & (z - 1) & "/36172/21505.png", n1.Adres)
        Assert.Equal(1, n1.CzescWiersz)
        Assert.Equal(1, n1.CzescKolumna)

        Dim n2 = plan.KafelNadrzedny(43010, 72346, 2)
        Assert.Equal("https://t.example/" & (z - 2) & "/18086/10752.png", n2.Adres)
        Assert.Equal(2, n2.CzescWiersz)
        Assert.Equal(2, n2.CzescKolumna)

        'poziomu powyżej z=0 nie ma
        Assert.Null(plan.KafelNadrzedny(0, 0, z + 1))

        'poziom poniżej zMin warstwy (limity zestawu) nie jest używany jako zastępczy
        Dim opcjeWarstw As New Dictionary(Of String, OpcjeKafliXyz) From {{"mapa", New OpcjeKafliXyz With {.ZMin = z, .ZMax = 18}}}
        Dim uslugaOgr = KafleXyz.UtworzUsluge("https://t.example/{z}/{x}/{y}.png", {"mapa"}, New OpcjeKafliXyz With {.ZMin = 0, .ZMax = 18}, opcjeWarstw)
        Dim planOgr As New PlanWmts(uslugaOgr, "mapa", UkladWspolrzednych.PL1992, New Zasieg(480000, 580000, 482000, 582000), 1.0, "png")
        Assert.Null(planOgr.KafelNadrzedny(43011, 72345, 1))
    End Sub

    <Fact>
    Public Sub UtworzUslugeZbioru_OpcjeWarstwZNadpisaniamiIDomyslnymTempem()
        Dim plik As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = "https://t.example/{layer}/{z}/{x}/{y}.png", .Referer = "https://strona.example/"}
        plik.Warstwy.Add(New WarstwaMapy With {.Nazwa = "podklad"})
        plik.Warstwy.Add(New WarstwaMapy With {.Nazwa = "szlaki", .Nakladka = True, .ZMax = 15})
        Dim u = KafleXyz.UtworzUslugeZbioru(plik.Adres, {"podklad", "szlaki"}, plik)

        Assert.Equal(2, u.Usluga.Warstwy.Count)
        Assert.False(u.OpcjeWarstw("podklad").Nakladka)
        Assert.True(u.OpcjeWarstw("szlaki").Nakladka)
        Assert.Equal(15, u.OpcjeWarstw("szlaki").ZMax)
        'tempo zapytań i Referer całego zbioru przechodzą na każdą warstwę
        Assert.Equal(100, u.OpcjeWarstw("szlaki").OdstepMs)
        Assert.Equal(2, u.OpcjeWarstw("szlaki").MaksWatkow)
        Assert.Equal("https://strona.example/", u.OpcjeWarstw("szlaki").Referer)

        'bez pliku zbioru - ustawienia domyślne podanego typu
        Dim bez = KafleXyz.UtworzUslugeZbioru("https://t.example/{z}/{x}/{-y}.png", {"mapa"}, Nothing, TypUslugiMapowej.Tms)
        Assert.Equal(TypUslugiMapowej.Tms, bez.OpcjeBazowe.Typ)
        Assert.True(bez.OpcjeWarstw.ContainsKey("mapa"))
    End Sub

    <Fact>
    Public Sub ObszarPozaZasiegiem_TylkoGdyObszarCalkiemPozaPolemZasieg()
        Dim opcje As New OpcjeKafliXyz With {.ZasiegGeo = New ProstokatGeo(55.2, 48.7, 24.6, 13.5)}
        'okolice Warszawy w PL-1992
        Assert.False(KafleXyz.ObszarPozaZasiegiem(opcje, UkladWspolrzednych.PL1992, New Zasieg(480000, 630000, 482000, 632000)))
        'ok. 47° N (X = -100 km) - na południe od pola zasieg
        Assert.True(KafleXyz.ObszarPozaZasiegiem(opcje, UkladWspolrzednych.PL1992, New Zasieg(-100000, 630000, -98000, 632000)))
        'bez pola zasieg obszar nigdy nie jest poza zasięgiem
        Assert.False(KafleXyz.ObszarPozaZasiegiem(New OpcjeKafliXyz(), UkladWspolrzednych.PL1992, New Zasieg(-100000, 630000, -98000, 632000)))
    End Sub

    <Fact>
    Public Sub OpisOdmowyDostepu_PodpowiadaPoleReferer()
        Assert.Contains("HTTP 403", KafleXyz.OpisOdmowyDostepu(""))
        Assert.Contains("""referer""", KafleXyz.OpisOdmowyDostepu(""))
        Assert.Contains("https://strona.example/", KafleXyz.OpisOdmowyDostepu("https://strona.example/"))
    End Sub

End Class
