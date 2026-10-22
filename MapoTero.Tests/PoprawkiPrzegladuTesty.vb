'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Text
Imports MapoTero.Core
Imports Xunit

''' <summary>Testy poprawek błędów znalezionych w przeglądzie kodu.</summary>
Public Class PoprawkiPrzegladuTesty

    <Theory>
    <InlineData("https://serwer.pl/arcgis/services/10.8/MapServer/WMSServer?", False)>
    <InlineData("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolution?", False)>
    <InlineData("http://192.168.1.5/wms?", True)>
    <InlineData("http://10.0.0.1/wms?", True)>
    <InlineData("http://172.20.1.1/wms?", True)>
    <InlineData("http://172.32.1.1/wms?", False)>
    <InlineData("http://localhost:8080/geoserver/wms?", True)>
    <InlineData("http://127.0.0.1/wms?", True)>
    <InlineData("/geoserver/wms?", True)>
    <InlineData("", True)>
    Public Sub AdresLokalny_TylkoPoNazwieSerwera(adres As String, lokalny As Boolean)
        Assert.Equal(lokalny, WmsCapabilities.JestAdresemLokalnym(adres))
    End Sub

    <Fact>
    Public Sub AdresGetMapZawierajacy10_NieJestZastepowanyAdresemGetCapabilities()
        Dim xml = "<?xml version=""1.0""?><WMS_Capabilities version=""1.3.0""><Service><Title>T</Title></Service><Capability><Request><GetMap>" &
                  "<Format>image/png</Format><DCPType><HTTP><Get><OnlineResource xmlns:xlink=""http://www.w3.org/1999/xlink"" " &
                  "xlink:href=""https://serwer.pl/arcgis/services/10.8/MapServer/WMSServer?""/></Get></HTTP></DCPType></GetMap></Request>" &
                  "<Layer><Name>a</Name></Layer></Capability></WMS_Capabilities>"
        Dim adresZgl = KlientCapabilities.AdresCapabilitiesWms("https://serwer.pl/arcgis/services/10.8/MapServer/WMSServer")
        Dim u = WmsCapabilities.Wczytaj(xml, adresZgl)
        Assert.Equal("https://serwer.pl/arcgis/services/10.8/MapServer/WMSServer?", u.AdresGetMap)
    End Sub

    <Fact>
    Public Sub BrakAdresuGetMap_AdresZgloszeniaBezRequestGetCapabilities()
        Dim xml = "<?xml version=""1.0""?><WMS_Capabilities version=""1.3.0""><Capability><Layer><Name>a</Name></Layer></Capability></WMS_Capabilities>"
        Dim u = WmsCapabilities.Wczytaj(xml, "https://serwer.pl/wms?token=abc&SERVICE=WMS&REQUEST=GetCapabilities")
        Assert.DoesNotContain("GetCapabilities", u.AdresGetMap, StringComparison.OrdinalIgnoreCase)
        Assert.Contains("token=abc", u.AdresGetMap)

        Dim plik = PlikWarstw.ZbudujAdresWmsDlaPliku(u.AdresGetMap, u.Wersja)
        Dim zapytanie = Wms.ZapytanieGetMap(plik, {"a"}, UkladWspolrzednych.PL1992, New Zasieg(0, 0, 100, 100), "png", 256, 256, False)
        Assert.Equal(1, RegularExpressions.Regex.Matches(zapytanie, "[?&]request=", RegularExpressions.RegexOptions.IgnoreCase).Count)
        Assert.Contains("request=GetMap", zapytanie, StringComparison.OrdinalIgnoreCase)
    End Sub

    <Fact>
    Public Sub ZbudujAdresWmsDlaPliku_ZastepujeInnyParametrRequest()
        Dim a = PlikWarstw.ZbudujAdresWmsDlaPliku("https://mapy.pl/wms?SERVICE=WMS&REQUEST=GetCapabilities", "1.3.0")
        Assert.Equal("https://mapy.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=", a)
    End Sub

    <Fact>
    Public Sub WczytajZTekstu_DeklaracjaIso88592_ZachowujePolskieZnaki()
        Dim xml = "<?xml version=""1.0"" encoding=""ISO-8859-2""?><WMS_Capabilities version=""1.3.0""><Service><Title>Mapa gleb województwa łódzkiego</Title></Service>" &
                  "<Capability><Layer><Name>a</Name></Layer></Capability></WMS_Capabilities>"
        Dim u = WmsCapabilities.Wczytaj(xml, "https://serwer.pl/wms?")
        Assert.Equal("Mapa gleb województwa łódzkiego", u.Tytul)
    End Sub

    <Fact>
    Public Sub LiczbaSegmentow_DuzaSiatkaBezPrzepelnienia()
        'obszar 600 x 600 km, piksel 0,05 m, segment 256 px - ponad 2 mld segmentów
        Dim s As New Siatka(New Zasieg(100000, 100000, 700000, 700000), 0.05, 256)
        Assert.Equal(46875L * 46875L, s.LiczbaSegmentow)
        Assert.True(s.ZbytDuza)
        Assert.False(New Siatka(New Zasieg(0, 0, 1000, 1000), 1, 100).ZbytDuza)
    End Sub

    <Fact>
    Public Sub Siatka_ZbytWieleKolumn_Niepoprawna()
        Dim s As New Siatka(New Zasieg(0, 0, 1.0E+15, 1000), 0.001, 1)
        Assert.False(s.Poprawna)
        Assert.Equal(0, s.LiczbaKolumn)
    End Sub

    <Fact>
    Public Sub Korytarz_ZbytDuzaSiatka_BezWyjatku()
        Dim k As New Korytarz()
        k.DodajLinie({New PunktGeo(52, 19), New PunktGeo(52.1, 19.1)})
        Dim s As New Siatka(New Zasieg(100000, 100000, 700000, 700000), 0.05, 256)
        Assert.Empty(k.Segmenty(s, UkladWspolrzednych.PL1992))
    End Sub

    <Fact>
    Public Sub TrekBuddy_PustyPrzedrostek_FolderMapyInnyNizFolderSesji()
        Dim folder As String = Path.Combine(Path.GetTempPath(), "sesja") & Path.DirectorySeparatorChar
        Dim tb As New MapoTero.MapaTrekBuddy(New MapoTero.ZadaniePobierania With {.Folder = folder, .Prefiks = "", .NazwaTrekBuddy = ""})
        Assert.Equal(Path.Combine(folder, "mapa") & Path.DirectorySeparatorChar, tb.FolderMapy)
        Dim zPrzedrostkiem As New MapoTero.MapaTrekBuddy(New MapoTero.ZadaniePobierania With {.Folder = folder, .Prefiks = "_", .NazwaTrekBuddy = "orto"})
        Assert.Equal(Path.Combine(folder, "_orto") & Path.DirectorySeparatorChar, zPrzedrostkiem.FolderMapy)
    End Sub

    <Theory>
    <InlineData("image/png; mode=8bit", "png")>
    <InlineData("image/png;mode=24bit", "png")>
    <InlineData("image/jpeg", "jpg")>
    Public Sub RozszerzeniePliku_ZParametramiTypuMime(format As String, rozszerzenie As String)
        Assert.Equal(rozszerzenie, Wms.RozszerzeniePliku(format))
    End Sub

    <Fact>
    Public Sub Korytarz_SegmentyJakPrzySprawdzeniuWszystkichPol()
        'wyznaczanie tylko pól przy trasie musi dawać ten sam wynik co sprawdzenie wszystkich pól siatki
        Dim s As New Siatka(New Zasieg(500000, 600000, 502000, 603000), 1.0, 100)
        Dim u = UkladWspolrzednych.PL1992
        Dim punkty = {New PunktXY(500050, 600010), New PunktXY(501930, 602400), New PunktXY(500700, 602950), New PunktXY(500710, 601000)}
        Dim k As New Korytarz With {.SzerokoscMetry = 150}
        k.DodajLinie(punkty.Select(Function(p) u.DoWgs84(p)))
        Dim m = k.Linie(0).Select(Function(g) u.ZWgs84(g)).ToList()

        Dim oczekiwane As New HashSet(Of Integer)
        For w = 1 To s.LiczbaWierszy
            For kol = 1 To s.LiczbaKolumn
                Dim z = s.Segment(w, kol)
                For i = 0 To m.Count - 2
                    If Korytarz.OdlegloscOdcinkaOdProstokata(m(i), m(i + 1), z.XDol, z.YLewy, z.XGora, z.YPrawy) <= 150 Then
                        oczekiwane.Add(s.NumerKolejny(w, kol) - 1)
                        Exit For
                    End If
                Next
            Next
        Next
        Dim wynik = k.Segmenty(s, u)
        Assert.True(oczekiwane.SetEquals(wynik), "oczekiwano " & oczekiwane.Count & ", jest " & wynik.Count)
        'szacunek nie zaniża liczby segmentów
        Assert.True(k.SzacowanaLiczbaSegmentow(s, u) >= wynik.Count)
    End Sub

    <Fact>
    Public Sub Korytarz_MalySegment_SzacunekBezWyznaczaniaSegmentow()
        'trasa ok. 100 km i segment 0,5 m (bok wpisywany "256" po pierwszej cyfrze) - setki milionów segmentów
        Dim k As New Korytarz With {.SzerokoscMetry = 1000}
        k.DodajLinie({New PunktGeo(50.5, 19.5), New PunktGeo(51.4, 19.5)})
        Dim s As New Siatka(k.Zasieg(UkladWspolrzednych.PL1992), 0.25, 2)
        Dim czas = Diagnostics.Stopwatch.StartNew()
        Dim szacunek = k.SzacowanaLiczbaSegmentow(s, UkladWspolrzednych.PL1992)
        Assert.True(czas.ElapsedMilliseconds < 1000)
        Assert.True(szacunek > 100000000.0)
    End Sub

    <Theory>
    <InlineData("nan")>
    <InlineData("NaN")>
    <InlineData("-Infinity")>
    <InlineData("1e999")>
    Public Sub Wartosc_NieLiczbaToWartoscDomyslna(tekst As String)
        Assert.Equal(7.0, MapoTero.Program.Wartosc(tekst, 7))
        Assert.Equal(3, MapoTero.Program.WartoscCalkowita(tekst, 3))
    End Sub

    <Theory>
    <InlineData("m100k", "", 8.0)>
    <InlineData("mapa", "Mapa 1:100 000", 8.0)>
    <InlineData("m50k", "", 4.0)>
    <InlineData("m25k", "", 2.0)>
    <InlineData("m10k", "", 1.0)>
    <InlineData("mapa", "Mapa 1:10 000", 1.0)>
    Public Sub ZalecanyRozmiarPiksela_SkalaZNazwy(nazwa As String, tytul As String, piksel As Double)
        Assert.Equal(piksel, New WarstwaWmsInfo With {.Nazwa = nazwa, .Tytul = tytul}.ZalecanyRozmiarPiksela())
    End Sub

    <Fact>
    Public Async Function KompilatorMbtiles_PlikDocelowyOtwarty_WynikPodInnaNazwa() As Threading.Tasks.Task
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_Kompilator_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Dim sesja = folder & "sesja" & Path.DirectorySeparatorChar
        Directory.CreateDirectory(sesja)
        Try
            Dim s As New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
            For w = 1 To 2
                For kol = 1 To 2
                    Using bmp As New Drawing.Bitmap(256, 256)
                        Using g = Drawing.Graphics.FromImage(bmp)
                            g.Clear(Drawing.Color.Red)
                        End Using
                        bmp.Save(sesja & s.NazwaSegmentu("_", StylNumeracji.WierszKolumna, w, kol) & ".png", Drawing.Imaging.ImageFormat.Png)
                    End Using
                Next
            Next
            Dim c As New KonfiguracjaSesji With {.XDol = "487000", .YLewy = "637000", .XGora = "487512", .YPrawy = "637512",
                .RozmiarPiksela = "1", .BokSegmentuPx = "256", .UkladEpsg = 2180, .Format = "png", .Prefiks = "_",
                .Numeracja = "NrWiersza_NrKolumny", .RodzajMapy = "test", .LiczbaWarstw = 1}
            c.Warstwy(0) = "a"
            c.Zapisz(sesja & "conf.txt", Encoding.UTF8)
            Dim warstwa As New MapoTero.WarstwaWielowarstwowa With {.Folder = sesja, .PoziomMin = 15, .PoziomMax = 16}
            Dim blad As String = ""
            Assert.True(warstwa.WczytajSesje(blad), blad)

            Dim docelowy = folder & "wynik.mbtiles"
            File.WriteAllText(docelowy, "stary plik")
            Dim zapisany As String
            'plik docelowy otwarty w innym programie (np. QGIS) - nie da się go zastąpić
            Using blokada As New FileStream(docelowy, FileMode.Open, FileAccess.Read, FileShare.None)
                zapisany = Await MapoTero.KompilatorMbtiles.KompilujAsync(New List(Of MapoTero.WarstwaWielowarstwowa) From {warstwa},
                    New MapoTero.OpcjeKompilatoraMbtiles With {.PlikDocelowy = docelowy, .PoziomMin = 15, .PoziomMax = 16}, Nothing, Threading.CancellationToken.None)
            End Using
            Assert.NotEqual(docelowy, zapisany)
            Assert.True(File.Exists(zapisany))
            Assert.True(New FileInfo(zapisany).Length > 1000)
            Assert.Equal("stary plik", File.ReadAllText(docelowy))
            Assert.False(File.Exists(docelowy & ".tmp"))
        Finally
            Data.SQLite.SQLiteConnection.ClearAllPools()
            Try
                Directory.Delete(folder, True)
            Catch
            End Try
        End Try
    End Function

End Class
