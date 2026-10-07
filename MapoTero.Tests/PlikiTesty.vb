'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Text
Imports MapoTero.Core
Imports Xunit

Public Class PlikiSesjiTesty

    'conf.txt zapisany przez dotychczasowe wersje programu (WriteLine), z polem warstwy jako #ERROR 448#
    Private Shared ReadOnly StaryConf As String = String.Join(vbCrLf, {
        """C:\MapoTero\download\""", """skany_map_topograficznych""", """500000""", """600000""", """501000""", """601500""",
        """250""", """2""", """Raster_25_1965""", "#ERROR 448#", """""", """""", """""", """""", """""", """""", """""", """""", """""", """""",
        "1", """jpeg""", """_""", "#FALSE#", """52,3""", """19,2""", """10""", """NrWiersza_NrKolumny""", ""})

    <Fact>
    Public Sub OdczytStaregoConf()
        Dim c = New CzytnikPlikuVbTest(StaryConf).Konfiguracja()
        Assert.Equal("C:\MapoTero\download\", c.Folder)
        Assert.Equal("skany_map_topograficznych", c.RodzajMapy)
        Assert.Equal("500000", c.XDol)
        Assert.Equal("601500", c.YPrawy)
        Assert.Equal("250", c.BokSegmentuPx)
        Assert.Equal("2", c.RozmiarPiksela)
        Assert.Equal("Raster_25_1965", c.Warstwy(0))
        Assert.Equal("", c.Warstwy(1))
        Assert.Equal(1, c.LiczbaWarstw)
        Assert.Equal("jpeg", c.Format)
        Assert.False(c.PobierajPowyzejOstatniego)
        Assert.Equal("52,3", c.SrodekMapySzerokosc)
        Assert.Equal("NrWiersza_NrKolumny", c.Numeracja)
        'brak pola układu w starym pliku - PL-1992
        Assert.Equal(2180, c.UkladEpsg)
    End Sub

    <Fact>
    Public Sub OdczytConfBezNumeracji()
        'pliki z wersji sprzed dodania stylu numeracji kończą się na skali mapy
        Dim tekst As String = StaryConf.Substring(0, StaryConf.IndexOf("""NrWiersza"))
        Dim c = New CzytnikPlikuVbTest(tekst).Konfiguracja()
        Assert.Equal("10", c.SkalaMapy)
        Assert.Equal("", c.Numeracja)
    End Sub

    <Fact>
    Public Sub ZapisIOdczytConf()
        Dim c As New KonfiguracjaSesji With {.Folder = "D:\mapy\", .RodzajMapy = "ortofotomapa", .XDol = "1", .YLewy = "2", .XGora = "3",
            .YPrawy = "4", .BokSegmentuPx = "2048", .RozmiarPiksela = "0.25", .LiczbaWarstw = 2, .Format = "png", .Prefiks = "_",
            .PobierajPowyzejOstatniego = True, .SrodekMapySzerokosc = "52.1", .SrodekMapyDlugosc = "21", .SkalaMapy = "12",
            .Numeracja = "1_2_3", .UkladEpsg = 2178}
        c.Warstwy(0) = "Raster"
        c.Warstwy(1) = "Inna warstwa"
        Dim tekst = c.DoTekstu()
        Assert.StartsWith("""D:\mapy\""" & vbCrLf & """ortofotomapa""", tekst)
        Assert.Contains(vbCrLf & "2" & vbCrLf & """png""", tekst)
        Assert.Contains("#TRUE#", tekst)

        Dim d = New CzytnikPlikuVbTest(tekst).Konfiguracja()
        Assert.Equal("Inna warstwa", d.Warstwy(1))
        Assert.Equal(2, d.LiczbaWarstw)
        Assert.True(d.PobierajPowyzejOstatniego)
        Assert.Equal("1_2_3", d.Numeracja)
        Assert.Equal(2178, d.UkladEpsg)
    End Sub

    <Fact>
    Public Sub CzytnikPolWJednymWierszu()
        Dim r As New CzytnikPlikuVb("""a,b"",12,#TRUE#" & vbCrLf & """x""")
        Assert.Equal("a,b", r.Nastepne())
        Assert.Equal(12, r.NastepnaLiczbaCalkowita())
        Assert.True(r.NastepnyLogiczny())
        Assert.Equal("x", r.Nastepne())
        Assert.True(r.KoniecPliku)
        Assert.Throws(Of EndOfStreamException)(Function() r.Nastepne())
    End Sub

    <Fact>
    Public Sub UstawieniaWStarymFormacie()
        'dawny plik domyślny: bez chkkml/chktab, z kluczem "numeracja" zamiast "numeracja_" i dodatkowym chkTB
        Dim sciezka = Path.GetTempFileName()
        File.WriteAllText(sciezka, String.Join(vbCrLf, {"folder segmentow", "C:\d\", "chkgmi", "True", "chkTB", "False",
                                                         "numeracja", "1_2_3", "iloscProbPobrania", "4", "x_start", "52.3"}))
        Dim u = PlikUstawien.Wczytaj(sciezka, Encoding.UTF8)
        File.Delete(sciezka)
        Assert.True(u.Logiczna("chkgmi", False))
        Assert.False(u.Logiczna("chkkml", False))
        Assert.Equal("1_2_3", u.Tekst("numeracja", "?"))
        Assert.Equal(4, u.Calkowita("iloscProbPobrania", 3))
        Assert.Equal(5, u.Calkowita("przerwaMiedzyProbami", 5))
        Assert.Equal("52.3", u.Tekst("x_start", ""))
    End Sub

    <Fact>
    Public Sub ZapisUstawien()
        Dim u As New PlikUstawien()
        u.Ustaw("chkgmi", True)
        u.Ustaw("iloscProbPobrania", 3)
        u.Ustaw("x_start", "52.3")
        Assert.Equal("chkgmi" & vbCrLf & "True" & vbCrLf & "iloscProbPobrania" & vbCrLf & "3" & vbCrLf & "x_start" & vbCrLf & "52.3" & vbCrLf, u.DoTekstu())
    End Sub

    ''' <summary>Pomocnik: odczyt konfiguracji z tekstu przez plik tymczasowy.</summary>
    Private Class CzytnikPlikuVbTest
        Private ReadOnly _tekst As String
        Public Sub New(tekst As String)
            _tekst = tekst
        End Sub
        Public Function Konfiguracja() As KonfiguracjaSesji
            Dim sciezka = Path.GetTempFileName()
            Try
                File.WriteAllText(sciezka, _tekst, Encoding.UTF8)
                Return KonfiguracjaSesji.Wczytaj(sciezka, Encoding.UTF8)
            Finally
                File.Delete(sciezka)
            End Try
        End Function
    End Class

End Class

Public Class GeoreferencjaTesty

    Private Shared Function Segment() As ObrazGeoreferencyjny
        Return New ObrazGeoreferencyjny With {
            .Uklad = UkladWspolrzednych.PL1992,
            .Zasieg = New Zasieg(500204.8, 600000, 500409.6, 600204.8),
            .SzerokoscPx = 2048, .WysokoscPx = 2048,
            .NazwaPliku = "_01_01.jpg", .SciezkaPliku = "C:\d\_01_01.jpg"}
    End Function

    <Fact>
    Public Sub WorldFileSrodekPiksela()
        Dim linie = Georeferencja.WorldFile(Segment()).Split({vbCrLf}, StringSplitOptions.None)
        Assert.Equal("0.1|0|0|-0.1|600000.05|500409.55", String.Join("|", linie))
    End Sub

    <Fact>
    Public Sub RozszerzeniaWorldFile()
        Assert.Equal("jpgw", Georeferencja.RozszerzenieWorldFile("jpg"))
        Assert.Equal("pngw", Georeferencja.RozszerzenieWorldFile(".png"))
        Assert.Equal("tifw", Georeferencja.RozszerzenieWorldFile("tiff"))
    End Sub

    <Fact>
    Public Sub PlikTab()
        Dim t = Georeferencja.Tab(Segment())
        Assert.Equal("!table" & vbCrLf & "!version 300" & vbCrLf & "!charset WindowsLatin2" & vbCrLf & "Definition Table" & vbCrLf &
                     "  File ""_01_01.jpg""" & vbCrLf & "  Type ""RASTER""" & vbCrLf &
                     "  (600000,500409.6) (0,0) Label ""Punkt 1""," & vbCrLf &
                     "  (600204.8,500409.6) (2048,0) Label ""Punkt 2""," & vbCrLf &
                     "  (600204.8,500204.8) (2048,2048) Label ""Punkt 3""," & vbCrLf &
                     "  (600000,500204.8) (0,2048) Label ""Punkt 4""" & vbCrLf &
                     "  CoordSys Earth Projection 8, 33, 7, 19, 0, 0.9993, 500000, -5300000" & vbCrLf, t)
    End Sub

    <Fact>
    Public Sub PlikMapOzi()
        Dim m = Georeferencja.MapOzi(Segment())
        Dim linie = m.Split({vbCrLf}, StringSplitOptions.None)
        Assert.Equal("OziExplorer Map Data File Version 2.2", linie(0))
        Assert.Equal("_01_01.jpg", linie(1))
        Assert.Equal("C:\d\_01_01.jpg", linie(2))
        Assert.Equal("Map Projection,Transverse Mercator,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No", linie(8))
        Assert.StartsWith("Point01,xy,    0,    0,in, deg,  52, ", linie(9))
        Assert.StartsWith("Point03,xy, 2048, 2048,in, deg,  52, ", linie(11))
        'wiersz identyczny z zapisywanym przez dotychczasowe wersje programu
        Assert.Contains("Projection Setup,     0.000000000,    19.000000000,     0.999300000,       500000.00,     -5300000.00,,,,,", m)
        Assert.Contains("MM1B,     0.1" & vbCrLf, m)
        Assert.EndsWith("IWH,Map Image Width/Height,2048,2048", m)
    End Sub

    <Fact>
    Public Sub StopnieIMinuty()
        Assert.Equal("52, 30", Georeferencja.StopnieMinuty(52.5))
        Assert.Equal("21, 7.5", Georeferencja.StopnieMinuty(21.125))
    End Sub

    <Fact>
    Public Sub PlikGmiIPoints()
        Dim g = Georeferencja.Gmi(Segment()).Split({vbCrLf}, StringSplitOptions.None)
        Assert.Equal("Map Calibration data file v3.0", g(0))
        Assert.Equal("2048", g(2))
        Assert.StartsWith("0;0;20.", g(4))
        Assert.StartsWith("2048;2048;20.", g(6))
        Dim p = Georeferencja.Points(Segment()).Split({vbCrLf}, StringSplitOptions.None)
        Assert.Equal("mapX mapY pixelX pixelY", p(0))
        Assert.EndsWith(vbTab & "2048.000000000000000" & vbTab & "-2048.000000000000000", p(3))
    End Sub

    <Fact>
    Public Sub KmlDokladneNarozniki()
        'segment przy zachodniej granicy (14,45°E) - obrót ok. +3,6°
        Dim o As New ObrazGeoreferencyjny With {.Uklad = UkladWspolrzednych.PL1992, .Zasieg = New Zasieg(500000, 190000, 501000, 191000),
                                                .SzerokoscPx = 1000, .WysokoscPx = 1000, .NazwaPliku = "a & b.jpg"}
        Dim k = Georeferencja.Kml(o, "żółw")
        Dim x = XDocument.Parse(k)
        Dim kml As XNamespace = "http://www.opengis.net/kml/2.2"
        Dim gx As XNamespace = "http://www.google.com/kml/ext/2.2"
        Assert.Equal("a & b.jpg", x.Descendants(kml + "href").Single().Value)
        Dim wsp = x.Descendants(gx + "LatLonQuad").Single().Element(kml + "coordinates").Value.Split(" "c)
        'wzorzec: PROJ 9.8 - kolejność: lewy dolny, prawy dolny, prawy górny, lewy górny
        Dim wzorzec = {"14.4535742687,52.2783758241", "14.4681903515,52.2789387945", "14.4672737377,52.2879038691", "14.4526547234,52.2873407178"}
        For i = 0 To 3
            Dim a = wsp(i).Split(","c), b = wzorzec(i).Split(","c)
            Assert.InRange(Double.Parse(a(0), Globalization.CultureInfo.InvariantCulture) - Double.Parse(b(0), Globalization.CultureInfo.InvariantCulture), -0.00000002, 0.00000002)
            Assert.InRange(Double.Parse(a(1), Globalization.CultureInfo.InvariantCulture) - Double.Parse(b(1), Globalization.CultureInfo.InvariantCulture), -0.00000002, 0.00000002)
        Next
        Dim obrot = Double.Parse(x.Descendants(kml + "rotation").Single().Value, Globalization.CultureInfo.InvariantCulture)
        Assert.InRange(obrot, 3.5, 3.7)
    End Sub

    <Fact>
    Public Sub KmlObrotNaWschodzie()
        Dim o As New ObrazGeoreferencyjny With {.Uklad = UkladWspolrzednych.PL1992, .Zasieg = New Zasieg(500000, 800000, 501000, 801000),
                                                .SzerokoscPx = 1000, .WysokoscPx = 1000, .NazwaPliku = "a.jpg"}
        Dim x = XDocument.Parse(Georeferencja.Kml(o, "a"))
        Dim kml As XNamespace = "http://www.opengis.net/kml/2.2"
        Dim obrot = Double.Parse(x.Descendants(kml + "rotation").Single().Value, Globalization.CultureInfo.InvariantCulture)
        Assert.InRange(obrot, -3.6, -3.4)
    End Sub

End Class
