'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.IO.Compression
Imports MapoTero.Core
Imports Xunit

Public Class KafleTesty

    <Fact>
    Public Sub NumeryKafliWebMercator()
        'Warszawa (52,2297 N; 21,0122 E): kafel 571/337 na poziomie 10 (wzorzec: standardowy wzór OpenStreetMap)
        Dim z = 10
        Assert.Equal(571, CInt(Math.Floor(WebMercator.PikselX(21.0122, z) / 256)))
        Assert.Equal(337, CInt(Math.Floor(WebMercator.PikselY(52.2297, z) / 256)))
        'przeliczenie odwrotne
        Assert.Equal(21.0122, WebMercator.Dlugosc(WebMercator.PikselX(21.0122, z), z), 9)
        Assert.Equal(52.2297, WebMercator.Szerokosc(WebMercator.PikselY(52.2297, z), z), 9)
        Dim k As New KafelXYZ(572, 335, 10)
        Assert.Equal(1023 - 335, k.WierszTms)
    End Sub

    <Fact>
    Public Sub ZakresKafliIPoziomRozdzielczosci()
        Dim p As New ProstokatGeo(52.3, 52.2, 21.1, 21.0)
        Dim r = WebMercator.ZakresKafli(p, 12)
        Assert.True(r.Item1 <= r.Item3 AndAlso r.Item2 <= r.Item4)
        Assert.Equal(CLng(r.Item3 - r.Item1 + 1) * (r.Item4 - r.Item2 + 1), WebMercator.LiczbaKafli(p, 12))
        'piksel 0,5 m na szerokości 52°: poziom 18 ma ok. 0,37 m, 17 - ok. 0,74 m
        Assert.Equal(18, WebMercator.PoziomDlaRozdzielczosci(0.5, 52))
        Assert.InRange(WebMercator.RozmiarPiksela(52, 18), 0.36, 0.38)
    End Sub

    <Fact>
    Public Sub PlanKmzZLimitemKafli()
        'obszar ok. 11 x 11 km, piksel 1 m -> ok. 11 x 11 kafli 1024 px (121) - powyżej limitu Garmin 100
        Dim p As New ProstokatGeo(52.1, 52.0, 21.16, 21.0)
        Dim bez As New PlanKmz(p, 1, 1024, 0)
        Assert.True(bez.LiczbaKafli > 100)
        Assert.Equal(1.0, bez.Pomniejszenie)
        Dim garmin As New PlanKmz(p, 1, 1024, 100)
        Assert.True(garmin.LiczbaKafli <= 100)
        Assert.True(garmin.Pomniejszenie > 1)
        'kafle pokrywają cały obszar bez przerw
        Dim pierwszy = garmin.ZasiegKafla(0, 0)
        Dim ostatni = garmin.ZasiegKafla(garmin.Wiersze - 1, garmin.Kolumny - 1)
        Assert.Equal(p.Polnoc, pierwszy.Polnoc, 9)
        Assert.Equal(p.Zachod, pierwszy.Zachod, 9)
        Assert.Equal(p.Poludnie, ostatni.Poludnie, 9)
        Assert.Equal(p.Wschod, ostatni.Wschod, 9)
        Assert.Equal(garmin.ZasiegKafla(0, 0).Wschod, garmin.ZasiegKafla(0, 1).Zachod, 12)
        For w = 0 To garmin.Wiersze - 1
            For k = 0 To garmin.Kolumny - 1
                Dim px = garmin.PikseleKafla(w, k)
                Assert.InRange(px.Item3, 1, 1024)
                Assert.InRange(px.Item4, 1, 1024)
            Next
        Next
    End Sub

    <Fact>
    Public Sub ProstokatWpisanyIOpisany()
        Dim z As New Zasieg(500000, 190000, 510000, 200000)   'zachodnia Polska - obraz obrócony o ok. 3,6°
        Dim o = ProstokatGeo.Opisany(UkladWspolrzednych.PL1992, z)
        Dim w = ProstokatGeo.Wpisany(UkladWspolrzednych.PL1992, z)
        Assert.True(w.Poprawny)
        Assert.True(o.Polnoc > w.Polnoc AndAlso o.Poludnie < w.Poludnie AndAlso o.Wschod > w.Wschod AndAlso o.Zachod < w.Zachod)
        'narożniki prostokąta wpisanego leżą wewnątrz obrazu
        For Each g In {New PunktGeo(w.Polnoc, w.Zachod), New PunktGeo(w.Polnoc, w.Wschod), New PunktGeo(w.Poludnie, w.Zachod), New PunktGeo(w.Poludnie, w.Wschod)}
            Dim p = UkladWspolrzednych.PL1992.ZWgs84(g)
            Assert.InRange(p.X, z.XDol, z.XGora)
            Assert.InRange(p.Y, z.YLewy, z.YPrawy)
        Next
    End Sub

    <Fact>
    Public Sub PlikKmz()
        Dim plik = Path.GetTempFileName()
        Try
            Using k As New ZapisKmz(plik, "Mapa & test", 50)
                k.DodajKafel("k_0_0.jpg", {1, 2, 3}, New ProstokatGeo(52.1, 52.0, 21.1, 21.0))
                k.DodajKafel("k_0_1.jpg", {4, 5}, New ProstokatGeo(52.1, 52.0, 21.2, 21.1))
                k.Zakoncz()
            End Using
            Using zip = ZipFile.OpenRead(plik)
                Dim nazwy = zip.Entries.Select(Function(e) e.FullName).ToList()
                Assert.Contains("doc.kml", nazwy)
                Assert.Contains("files/k_0_0.jpg", nazwy)
                Dim doc As XDocument
                Using s = zip.GetEntry("doc.kml").Open()
                    doc = XDocument.Load(s)
                End Using
                Dim kml As XNamespace = "http://www.opengis.net/kml/2.2"
                Assert.Equal(2, doc.Descendants(kml + "GroundOverlay").Count())
                Assert.Equal("Mapa & test", doc.Descendants(kml + "Folder").Single().Element(kml + "name").Value)
                Assert.Equal("50", doc.Descendants(kml + "drawOrder").First().Value)
                Assert.Equal("files/k_0_1.jpg", doc.Descendants(kml + "href").Last().Value)
                Assert.Equal("21.2000000000", doc.Descendants(kml + "east").Last().Value)
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub InterpolacjaPrzeliczenDokladna()
        'kafel 256 px Web Mercator (poziom 16) przeliczany do PL-1992: interpolacja co 16 px vs dokładne przeliczenie
        Dim z = 16, x0 = 572 * 64 * 256.0, y0 = 335 * 64 * 256.0
        Dim f = Function(px As Double, py As Double) UkladWspolrzednych.PL1992.ZWgs84(WebMercator.Szerokosc(y0 + py + 0.5, z), WebMercator.Dlugosc(x0 + px + 0.5, z))
        Dim s As New SiatkaPrzeliczen(256, 256, 16, f)
        Dim maks As Double = 0
        For Each py In {0, 7, 100, 255}
            For Each px In {0, 13, 128, 255}
                Dim a = s.Punkt(px, py), b = f(px, py)
                maks = Math.Max(maks, Math.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2))
            Next
        Next
        Assert.True(maks < 0.001, "błąd interpolacji " & maks & " m")
    End Sub

End Class
