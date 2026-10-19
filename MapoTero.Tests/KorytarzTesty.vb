'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Xml.Linq
Imports MapoTero.Core
Imports Xunit

Public Class KorytarzTesty

    Private Const Gpx As String =
        "<?xml version=""1.0"" encoding=""UTF-8""?>" &
        "<gpx version=""1.1"" creator=""test"" xmlns=""http://www.topografix.com/GPX/1/1"">" &
        "<trk><name>Szlak testowy</name><trkseg>" &
        "<trkpt lat=""52.20"" lon=""21.00""><ele>100</ele></trkpt>" &
        "<trkpt lat=""52.25"" lon=""21.05""/>" &
        "</trkseg><trkseg><trkpt lat=""52.30"" lon=""21.10""/></trkseg></trk>" &
        "<rte><rtept lat=""52.00"" lon=""20.00""/><rtept lat=""52.01"" lon=""20.01""/></rte>" &
        "</gpx>"

    <Fact>
    Public Sub OdczytGpxSladyITrasy()
        Dim k = Korytarz.ZTekstu(Gpx)
        Assert.Equal("Szlak testowy", k.Nazwa)
        Assert.Equal(3, k.Linie.Count)
        Assert.Equal(2, k.Linie(0).Count)
        Assert.Equal(52.25, k.Linie(0)(1).Szerokosc, 9)
        Assert.Equal(21.05, k.Linie(0)(1).Dlugosc, 9)
        Assert.Equal(5, k.LiczbaPunktow)
        Assert.True(k.Poprawny)
        'odcinek 52.20/21.00 - 52.25/21.05: ok. 6,6 km; trasa 52.00/20.00 - 52.01/20.01: ok. 1,3 km
        Assert.InRange(k.DlugoscKm, 7.5, 8.5)
    End Sub

    <Fact>
    Public Sub OdczytGpxZSamychPunktow()
        Dim k = Korytarz.ZTekstu("<gpx xmlns=""http://www.topografix.com/GPX/1/0""><wpt lat=""50"" lon=""19""/><wpt lat=""50.1"" lon=""19.1""/></gpx>")
        Assert.Single(k.Linie)
        Assert.Equal(2, k.Linie(0).Count)
    End Sub

    <Fact>
    Public Sub OdczytKmlLiniiISladu()
        Dim kml As String =
            "<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2""><Document><name>Wycieczka</name>" &
            "<Placemark><LineString><coordinates>21.0,52.2,100 21.1,52.3,110" & vbLf & "21.2,52.4</coordinates></LineString></Placemark>" &
            "<Placemark><gx:Track><when>2026-01-01T00:00:00Z</when><gx:coord>19.5 50.1 300</gx:coord><gx:coord>19.6 50.2 310</gx:coord></gx:Track></Placemark>" &
            "</Document></kml>"
        Dim k = Korytarz.ZTekstu(kml)
        Assert.Equal("Wycieczka", k.Nazwa)
        Assert.Equal(2, k.Linie.Count)
        Assert.Equal(3, k.Linie(0).Count)
        Assert.Equal(52.4, k.Linie(0)(2).Szerokosc, 9)
        Assert.Equal(21.2, k.Linie(0)(2).Dlugosc, 9)
        Assert.Equal(50.2, k.Linie(1)(1).Szerokosc, 9)
        Assert.Equal(19.6, k.Linie(1)(1).Dlugosc, 9)
    End Sub

    <Fact>
    Public Sub OdczytKmz()
        Dim kml As String = "<kml xmlns=""http://www.opengis.net/kml/2.2""><Placemark><LineString><coordinates>18,54 18.1,54.1</coordinates></LineString></Placemark></kml>"
        Dim dane() As Byte
        Using ms As New MemoryStream()
            Using zip As New ZipArchive(ms, ZipArchiveMode.Create, True)
                Using s = zip.CreateEntry("files/obraz.txt").Open()
                    s.WriteByte(1)
                End Using
                Using w As New StreamWriter(zip.CreateEntry("doc.kml").Open(), New UTF8Encoding(False))
                    w.Write(kml)
                End Using
            End Using
            dane = ms.ToArray()
        End Using
        Dim k = Korytarz.ZKmz(dane)
        Assert.Single(k.Linie)
        Assert.Equal(54.1, k.Linie(0)(1).Szerokosc, 9)
    End Sub

    <Fact>
    Public Sub NieznanyFormatToBlad()
        Assert.Throws(Of InvalidDataException)(Function() Korytarz.ZTekstu("<html><body/></html>"))
    End Sub

    <Fact>
    Public Sub ZapisIOdczytWFolderzeSesji()
        Dim k = Korytarz.ZTekstu(Gpx)
        k.SzerokoscMetry = 750
        Dim folder = Path.Combine(Path.GetTempPath(), "mapotero_korytarz_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            k.Zapisz(Path.Combine(folder, Korytarz.NazwaPliku))
            Dim w = Korytarz.ZFolderu(folder)
            Assert.NotNull(w)
            Assert.Equal(750, w.SzerokoscMetry, 9)
            Assert.Equal("Szlak testowy", w.Nazwa)
            Assert.Equal(k.Linie.Count, w.Linie.Count)
            Assert.Equal(k.Linie(0)(1).Szerokosc, w.Linie(0)(1).Szerokosc, 7)
            Assert.Equal(k.Linie(2)(1).Dlugosc, w.Linie(2)(1).Dlugosc, 7)
            'brak pliku lub plik obcy - Nothing
            Assert.Null(Korytarz.ZFolderu(Path.Combine(folder, "brak")))
            File.WriteAllText(Path.Combine(folder, Korytarz.NazwaPliku), "coś innego")
            Assert.Null(Korytarz.ZFolderu(folder))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub OdlegloscOdcinkaOdProstokata()
        'odcinek przecinający prostokąt bez końców w jego wnętrzu
        Assert.Equal(0.0, Korytarz.OdlegloscOdcinkaOdProstokata(New PunktXY(-5, 5), New PunktXY(15, 5), 0, 0, 10, 10))
        'odcinek równoległy w odległości 3
        Assert.Equal(3.0, Korytarz.OdlegloscOdcinkaOdProstokata(New PunktXY(13, 0), New PunktXY(13, 10), 0, 0, 10, 10), 9)
        'najbliżej leży narożnik prostokąta (10, 10) - odcinek x + y = 24
        Assert.Equal(4 / Math.Sqrt(2), Korytarz.OdlegloscOdcinkaOdProstokata(New PunktXY(24, 0), New PunktXY(0, 24), 0, 0, 10, 10), 9)
        'punkt (odcinek zerowej długości)
        Assert.Equal(5.0, Korytarz.OdlegloscOdcinkaOdProstokata(New PunktXY(13, 14), New PunktXY(13, 14), 0, 0, 10, 10), 9)
    End Sub

    ''' <summary>Trasa po przekątnej siatki 10 x 10 segmentów 1 km w PL-1992.</summary>
    Private Shared Function TrasaPoPrzekatnej(siatka As Siatka, szerokosc As Double) As Korytarz
        Dim u = UkladWspolrzednych.PL1992
        Dim z = siatka.ZasiegSiatki
        Dim k As New Korytarz With {.SzerokoscMetry = szerokosc}
        'od środka lewego dolnego segmentu do środka prawego górnego
        k.DodajLinie({u.DoWgs84(z.XDol + 500, z.YLewy + 500), u.DoWgs84(z.XGora - 500, z.YPrawy - 500)})
        Return k
    End Function

    <Fact>
    Public Sub SegmentyWzdluzPrzekatnej()
        Dim siatka As New Siatka(New Zasieg(480000, 630000, 490000, 640000), 1, 1000)
        Assert.Equal(10, siatka.LiczbaKolumn)
        Assert.Equal(10, siatka.LiczbaWierszy)

        'pas 100 m: segmenty na przekątnej i (przez narożniki) ich sąsiedzi wzdłuż przekątnej
        Dim waski = TrasaPoPrzekatnej(siatka, 100).Segmenty(siatka, UkladWspolrzednych.PL1992)
        For i = 1 To 10
            'wiersz 1 - górny, więc przekątna z lewego dołu do prawego góry: wiersz = 11 - kolumna
            Assert.Contains((10 - i) * 10 + (i - 1), waski)
        Next
        Assert.InRange(waski.Count, 10, 28)
        'segmenty daleko od przekątnej - pominięte
        Assert.DoesNotContain(0, waski)
        Assert.DoesNotContain(99, waski)

        'pas 1,5 km obejmuje sąsiednie przekątne, ale nie narożniki siatki
        Dim szeroki = TrasaPoPrzekatnej(siatka, 1500).Segmenty(siatka, UkladWspolrzednych.PL1992)
        Assert.True(szeroki.Count > waski.Count)
        Assert.True(szeroki.Count < 100)
        Assert.DoesNotContain(0, szeroki)
        Assert.DoesNotContain(99, szeroki)

        'zasięg korytarza: trasa poszerzona o pas
        Dim z = TrasaPoPrzekatnej(siatka, 1500).Zasieg(UkladWspolrzednych.PL1992)
        Assert.Equal(480500 - 1500, z.XDol, 3)
        Assert.Equal(640000 - 500 + 1500, z.YPrawy, 3)
    End Sub

    <Fact>
    Public Sub SegmentyWUkladzieGeograficznym()
        'siatka WGS84: 0,01° x 0,01°, trasa wzdłuż równoleżnika przez środek drugiego wiersza
        Dim siatka As New Siatka(New Zasieg(52.0, 21.0, 52.05, 21.1), 0.00001, 1000)
        Assert.Equal(10, siatka.LiczbaKolumn)
        Assert.Equal(5, siatka.LiczbaWierszy)
        Dim k As New Korytarz With {.SzerokoscMetry = 100}
        k.DodajLinie({New PunktGeo(52.035, 21.0), New PunktGeo(52.035, 21.1)})
        Dim s = k.Segmenty(siatka, UkladWspolrzednych.Wgs84)
        Assert.Equal(10, s.Count)
        For kol = 0 To 9
            Assert.Contains(1 * 10 + kol, s)
        Next
        'pas 700 m (> 0,005° szerokości, czyli ok. 556 m) obejmuje też sąsiednie wiersze
        k.SzerokoscMetry = 700
        Assert.Equal(30, k.Segmenty(siatka, UkladWspolrzednych.Wgs84).Count)
    End Sub

    <Fact>
    Public Sub PustyKorytarzBezSegmentow()
        Dim siatka As New Siatka(New Zasieg(480000, 630000, 490000, 640000), 1, 1000)
        Assert.Empty(New Korytarz().Segmenty(siatka, UkladWspolrzednych.PL1992))
        Assert.False(New Korytarz().Poprawny)
    End Sub

End Class

Public Class MetadaneTesty

    <Fact>
    Public Sub ZrodloDanychWgSerwera()
        Assert.Equal("Główny Urząd Geodezji i Kartografii (geoportal.gov.pl)",
                     MetadaneMapy.ZrodloDanych("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolution?"))
        Assert.Equal("Główny Urząd Geodezji i Kartografii (geoportal.gov.pl)", MetadaneMapy.ZrodloDanych("http://integracja.gugik.gov.pl/cgi-bin/x?"))
        Assert.Equal("Generalna Dyrekcja Ochrony Środowiska", MetadaneMapy.ZrodloDanych("https://sdi.gdos.gov.pl/wms?"))
        Assert.Equal("Państwowy Instytut Geologiczny - PIB", MetadaneMapy.ZrodloDanych("http://cbdgmapa.pgi.gov.pl/arcgis/x"))
        Assert.Equal("Bank Danych o Lasach", MetadaneMapy.ZrodloDanych("https://mapserver.bdl.lasy.gov.pl/x"))
        Assert.Equal("© autorzy OpenStreetMap", MetadaneMapy.ZrodloDanych("https://ows.terrestris.de/osm/service?"))
        Assert.Equal("serwer.example.com", MetadaneMapy.ZrodloDanych("""https://serwer.example.com:8080/wms?a=b"""))
        Assert.Equal("", MetadaneMapy.ZrodloDanych(""))
        Assert.Equal("mapy.geoportal.gov.pl", MetadaneMapy.Serwer("http://mapy.geoportal.gov.pl:80/wss"))
    End Sub

    <Fact>
    Public Sub NazwaIOpisMapy()
        Dim m As New MetadaneMapy With {.Tytul = "ortofotomapa Raster", .DataPobrania = New Date(2026, 10, 19)}
        m.Warstwy.Add("ortofotomapa Raster")
        m.DodajZrodlo("GUGiK")
        m.DodajZrodlo("gugik")
        Assert.Equal("ortofotomapa Raster (2026-10-19)", m.Nazwa)
        Assert.Equal("Źródło: GUGiK", m.Atrybucja)
        Assert.Contains("Data pobrania: 2026-10-19", m.Opis)
        m.Warstwy.Add("obszary chronione")
        m.DodajZrodlo("GDOŚ")
        Assert.Equal("Źródła: GUGiK; GDOŚ", m.Atrybucja)
        Assert.Contains("ortofotomapa Raster + obszary chronione", m.Opis)
        Dim bezDaty As New MetadaneMapy()
        Assert.Equal("Mapa MapoTero", bezDaty.Nazwa)
        Assert.Contains("Data pobrania: nieznana", bezDaty.Opis)
    End Sub

    <Fact>
    Public Sub DataWConfTxt()
        Assert.Equal("2026-01-05", MetadaneMapy.TekstDaty(New Date(2026, 1, 5)))
        Assert.Equal(New Date(2026, 1, 5), MetadaneMapy.DataZTekstu("2026-01-05").Value)
        Assert.False(MetadaneMapy.DataZTekstu("").HasValue)
        Assert.False(MetadaneMapy.DataZTekstu("5.01.2026").HasValue)
    End Sub

    <Fact>
    Public Sub NowePolaConfTxt()
        Dim c As New KonfiguracjaSesji With {.Folder = "D:\m\", .RodzajMapy = "zbior", .Przezroczystosc = True, .DataPobrania = "2026-10-19",
                                             .AdresSerwera = "https://mapy.geoportal.gov.pl/wss/x?a=1,2&layers=", .UkladEpsg = 2177}
        Dim plik = Path.GetTempFileName()
        Try
            c.Zapisz(plik, Encoding.UTF8)
            Dim d = KonfiguracjaSesji.Wczytaj(plik, Encoding.UTF8)
            Assert.True(d.Przezroczystosc)
            Assert.Equal("2026-10-19", d.DataPobrania)
            Assert.Equal("https://mapy.geoportal.gov.pl/wss/x?a=1,2&layers=", d.AdresSerwera)
            Assert.Equal(2177, d.UkladEpsg)

            'plik z wersji 3.13 (bez nowych pól) - wartości domyślne
            Dim tekst = c.DoTekstu()
            Dim stary = tekst.Substring(0, tekst.IndexOf("#TRUE#", StringComparison.Ordinal))
            File.WriteAllText(plik, stary, Encoding.UTF8)
            Dim e = KonfiguracjaSesji.Wczytaj(plik, Encoding.UTF8)
            Assert.False(e.Przezroczystosc)
            Assert.Equal("", e.DataPobrania)
            Assert.Equal("", e.AdresSerwera)
            Assert.Equal(2177, e.UkladEpsg)
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub ParametrTransparentTylkoDlaPngIGif()
        Const adres As String = "http://s/wms?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
        Dim z As New Zasieg(0, 0, 100, 100)
        Assert.EndsWith("&transparent=TRUE", Wms.ZapytanieGetMap(adres, {"A"}, UkladWspolrzednych.PL1992, z, "png", 10, 10, False, True))
        Assert.EndsWith("&transparent=TRUE", Wms.ZapytanieGetMap(adres, {"A"}, UkladWspolrzednych.PL1992, z, "image/gif", 10, 10, False, True))
        Assert.DoesNotContain("transparent", Wms.ZapytanieGetMap(adres, {"A"}, UkladWspolrzednych.PL1992, z, "jpeg", 10, 10, False, True))
        Assert.DoesNotContain("transparent", Wms.ZapytanieGetMap(adres, {"A"}, UkladWspolrzednych.PL1992, z, "png", 10, 10, False))
        'adres z własnym parametrem TRANSPARENT - bez powtórzenia
        Dim wlasny = Wms.ZapytanieGetMap(adres.Replace("layers=", "TRANSPARENT=FALSE&layers="), {"A"}, UkladWspolrzednych.PL1992, z, "png", 10, 10, False, True)
        Assert.DoesNotContain("transparent=TRUE", wlasny)
    End Sub

End Class

Public Class EksportFormatyTesty

    <Fact>
    Public Sub KalibracjaOruxMaps()
        Dim p As New ProstokatGeo(52.3, 52.2, 21.1, 21.0)
        Dim poziomy = {ZakresKafliPoziomu.Dla(p, 12), ZakresKafliPoziomu.Dla(p, 13)}
        Dim xml = MapaOruxMaps.Kalibracja("Mapa <test> & ]]> koniec", poziomy)
        Dim doc = XDocument.Parse(xml)
        Dim ns As XNamespace = "http://oruxtracker.com/app/res/calibration"
        Assert.Equal("OruxTracker", doc.Root.Name.LocalName)
        Dim glowna = doc.Root.Element(ns + "MapCalibration")
        Assert.Equal("true", glowna.Attribute("layers").Value)
        Assert.Contains("Mapa <test> &", glowna.Element(ns + "MapName").Value)
        Dim warstwy = glowna.Elements(ns + "MapCalibration").ToList()
        Assert.Equal(2, warstwy.Count)
        Dim w12 = warstwy(0)
        Assert.Equal("12", w12.Attribute("layerLevel").Value)
        Dim chunks = w12.Element(ns + "MapChunks")
        Assert.Equal(poziomy(0).Kolumny.ToString(), chunks.Attribute("xMax").Value)
        Assert.Equal(poziomy(0).Wiersze.ToString(), chunks.Attribute("yMax").Value)
        Assert.Equal("Mercator", chunks.Attribute("projection").Value)
        Assert.Equal((poziomy(0).Kolumny * 256).ToString(), w12.Element(ns + "MapDimensions").Attribute("width").Value)
        'narożniki pokrywają cały prostokąt (kafle wyrównane do siatki)
        Dim granice = w12.Element(ns + "MapBounds")
        Dim ci = Globalization.CultureInfo.InvariantCulture
        Assert.True(Double.Parse(granice.Attribute("maxLat").Value, ci) >= 52.3)
        Assert.True(Double.Parse(granice.Attribute("minLat").Value, ci) <= 52.2)
        Assert.True(Double.Parse(granice.Attribute("minLon").Value, ci) <= 21.0)
        Assert.True(Double.Parse(granice.Attribute("maxLon").Value, ci) >= 21.1)
        Dim punkty = w12.Element(ns + "CalibrationPoints").Elements(ns + "CalibrationPoint").ToList()
        Assert.Equal(4, punkty.Count)
        Assert.Equal("TL", punkty(0).Attribute("corner").Value)
        Assert.Equal(granice.Attribute("maxLat").Value, punkty(0).Attribute("lat").Value)
        Assert.Equal(granice.Attribute("minLon").Value, punkty(0).Attribute("lon").Value)
    End Sub

    <Fact>
    Public Sub NazwyPlikowISQLiteDB()
        Assert.Equal("orto_ 2026_10", MapaOruxMaps.NazwaPliku("orto/ 2026:10."))
        Assert.Equal("mapa", MapaOruxMaps.NazwaPliku("  "))
        Assert.Equal(1, MapaSQLiteDB.ZapisanyPoziom(16))
        Assert.Equal(-2, MapaSQLiteDB.ZapisanyPoziom(19))
        Assert.Equal(16, MapaSQLiteDB.ZapisanyPoziom(MapaSQLiteDB.ZapisanyPoziom(16)))
    End Sub

    <Fact>
    Public Sub OpisWDokumencieKmz()
        Dim plik = Path.GetTempFileName()
        Try
            Using kmz As New ZapisKmz(plik, "Mapa", 50)
                kmz.Opis = "Źródło: GUGiK & inni"
                kmz.DodajKafel("k.png", {1, 2, 3}, New ProstokatGeo(52.1, 52, 21.1, 21))
                kmz.Zakoncz()
            End Using
            Using zip = ZipFile.OpenRead(plik), r As New StreamReader(zip.GetEntry("doc.kml").Open())
                Dim doc = XDocument.Parse(r.ReadToEnd())
                Dim opis = doc.Descendants().First(Function(e) e.Name.LocalName = "description")
                Assert.Equal("Źródło: GUGiK & inni", opis.Value)
                Assert.NotNull(zip.GetEntry("files/k.png"))
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

End Class
