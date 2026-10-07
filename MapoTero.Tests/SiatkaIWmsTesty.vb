'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class SiatkaTesty

    <Fact>
    Public Sub LiczbaSegmentowIZasiegi()
        'obszar 1000 m (X) x 1500 m (Y), piksel 2 m, segment 250 px = 500 m
        Dim s As New Siatka(New Zasieg(500000, 600000, 501000, 601500), 2, 250)
        Assert.Equal(2, s.LiczbaWierszy)
        Assert.Equal(3, s.LiczbaKolumn)
        Assert.Equal(6, s.LiczbaSegmentow)
        Assert.Equal(750, s.SzerokoscPx)
        Assert.Equal(500, s.WysokoscPx)

        'wiersz 1 - górny
        Dim g = s.Segment(1, 1)
        Assert.Equal(500500, g.XDol)
        Assert.Equal(501000, g.XGora)
        Assert.Equal(600000, g.YLewy)
        Assert.Equal(600500, g.YPrawy)

        Dim d = s.Segment(2, 3)
        Assert.Equal(500000, d.XDol)
        Assert.Equal(601000, d.YLewy)
    End Sub

    <Fact>
    Public Sub ObszarPowiekszanyDoPelnychSegmentow()
        Dim s As New Siatka(New Zasieg(0, 0, 1001, 999), 2, 250)
        Assert.Equal(3, s.LiczbaWierszy)
        Assert.Equal(2, s.LiczbaKolumn)
        Assert.Equal(1500, s.ZasiegSiatki.XGora)
        Assert.Equal(1000, s.ZasiegSiatki.YPrawy)
    End Sub

    <Fact>
    Public Sub NiecalkowityZasiegSegmentu()
        '0,1 m x 2048 px = 204,8 m; obszar 409,6 m to dokładnie 2 segmenty (bez błędu zaokrągleń)
        Dim s As New Siatka(New Zasieg(500000, 600000, 500409.6, 600409.6), 0.1, 2048)
        Assert.Equal(2, s.LiczbaWierszy)
        Assert.Equal(2, s.LiczbaKolumn)
        Assert.Equal(500204.8, s.Segment(1, 1).XDol, 6)
        Assert.Equal(500409.6, s.Segment(1, 1).XGora, 6)
    End Sub

    <Fact>
    Public Sub NazwySegmentow()
        Dim s As New Siatka(New Zasieg(0, 0, 3000, 4000), 1, 1000)
        Assert.Equal("_02_03", s.NazwaSegmentu("_", StylNumeracji.WierszKolumna, 2, 3))
        Assert.Equal("_07", s.NazwaSegmentu("_", StylNumeracji.KolejnyDwucyfrowy, 2, 3))
        Assert.Equal("_7", s.NazwaSegmentu("_", StylNumeracji.Kolejny, 2, 3))
        Assert.Equal("__2000_1000", s.NazwaSegmentuTrekBuddy("_", 2, 3))
        Assert.Equal(StylNumeracji.Kolejny, Siatka.StylZTekstu("1_2_3"))
        Assert.Equal("NrWiersza_NrKolumny", Siatka.TekstStylu(Siatka.StylZTekstu("cokolwiek")))
    End Sub

    <Fact>
    Public Sub NiepoprawneParametry()
        Dim s As New Siatka(New Zasieg(0, 0, 100, 100), 0, 250)
        Assert.False(s.Poprawna)
        Assert.Equal(0, s.LiczbaSegmentow)
    End Sub

End Class

Public Class WmsTesty

    Private Const Adres As String = "http://serwer/wms?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="

    <Fact>
    Public Sub DopisujeService()
        Assert.Equal("http://serwer/wms?SERVICE=WMS&request=GetMap", Wms.UzupelnijAdres("http://serwer/wms?request=GetMap"))
        Assert.Equal("http://s/wms?service=wms&a=1", Wms.UzupelnijAdres("http://s/wms?service=wms&a=1"))
        Assert.Equal("http://s/wms?SERVICE=WMS&", Wms.UzupelnijAdres("http://s/wms"))
    End Sub

    <Fact>
    Public Sub PoprawiaParametrUkladu()
        'plik GDOŚ: wersja 1.3.0 z parametrem SRS - poprawnie CRS
        Dim a = Wms.UstawUklad("http://sdi.gdos.gov.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&SRS=EPSG:2180&layers=", UkladWspolrzednych.PL1992)
        Assert.Contains("&CRS=EPSG:2180&", a)
        Assert.DoesNotContain("SRS", a)
        Dim b = Wms.UstawUklad(Adres, UkladWspolrzednych.Utm34N)
        Assert.Contains("CRS=EPSG:32634", b)
        Dim c = Wms.UstawUklad("http://s/wms?version=1.1.1&layers=", UkladWspolrzednych.PL1992)
        Assert.Contains("SRS=EPSG:2180&layers=", c)
    End Sub

    <Fact>
    Public Sub KolejnoscBbox()
        Dim z As New Zasieg(500000, 600000, 500500, 600500)
        Dim u = Wms.ZapytanieGetMap(Adres, {"A", "B", ""}, UkladWspolrzednych.PL1992, z, "jpeg", 250, 250, False)
        Assert.Equal("http://serwer/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=A,B&bbox=500000,600000,500500,600500&format=image/jpeg&styles=&width=250&height=250", u)
        'zamiana osi tylko w zapytaniu
        Dim z2 = Wms.ZapytanieGetMap(Adres, {"A"}, UkladWspolrzednych.PL1992, z, "png", 250, 250, True)
        Assert.Contains("&bbox=600000,500000,600500,500500&", z2)
        'UTM - kolejność wschód, północ
        Dim z3 = Wms.ZapytanieGetMap(Adres, {"A"}, UkladWspolrzednych.Utm34N, z, "png", 250, 250, False)
        Assert.Contains("&bbox=600000,500000,600500,500500&", z3)
        'WMS 1.1.1 - zawsze wschód, północ
        Dim z4 = Wms.ZapytanieGetMap("http://s/wms?version=1.1.1&SRS=EPSG:2180&layers=", {"A"}, UkladWspolrzednych.PL1992, z, "png", 250, 250, False)
        Assert.Contains("&bbox=600000,500000,600500,500500&", z4)
    End Sub

    <Fact>
    Public Sub StalaListaWarstwWAdresie()
        'plik BDOT_wszystkie_warstwy zawiera listę warstw w adresie - wybrana warstwa dopisywana po przecinku
        Dim u = Wms.ZapytanieGetMap("http://s/wms?version=1.3.0&CRS=EPSG:2180&layers=X1,X2", {"P"}, UkladWspolrzednych.PL1992,
                                    New Zasieg(0, 0, 1, 1), "png", 1, 1, False)
        Assert.Contains("layers=X1,X2,P&bbox", u)
    End Sub

    <Fact>
    Public Sub LiczbyNiecalkowite()
        Dim u = Wms.ZapytanieGetMap(Adres, {"A"}, UkladWspolrzednych.PL1992, New Zasieg(500000, 600000, 500204.8, 600204.8), "jpeg", 2048, 2048, False)
        Assert.Contains("&bbox=500000,600000,500204.8,600204.8&", u)
    End Sub

    <Fact>
    Public Sub RozszerzeniaPlikow()
        Assert.Equal("jpg", Wms.RozszerzeniePliku("jpeg"))
        Assert.Equal("tif", Wms.RozszerzeniePliku("tiff"))
        Assert.Equal("png", Wms.RozszerzeniePliku("png24"))
    End Sub

End Class
