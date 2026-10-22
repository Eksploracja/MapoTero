'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class PobierzIKompilujTest

    <Fact>
    Public Async Function PobierzObszarGminyIKompilujMbtilesAsync() As Task
        'folder wyników: zmienna środowiskowa MAPOTERO_FOLDER_TESTU (np. folder download programu, by obejrzeć mapę),
        'a gdy jej brak - folder tymczasowy (dawniej ścieżka wpisana na stałe, nieistniejąca na innych komputerach i w CI)
        Dim folderDownload = Environment.GetEnvironmentVariable("MAPOTERO_FOLDER_TESTU")
        If String.IsNullOrWhiteSpace(folderDownload) Then folderDownload = Path.Combine(Path.GetTempPath(), "MapoTero_PobierzIKompiluj")
        folderDownload = folderDownload.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(folderDownload)

        Dim folderTopo25 = Path.Combine(folderDownload, "gmina_kazimierz_topo25_65") & Path.DirectorySeparatorChar
        Dim folderTopo10 = Path.Combine(folderDownload, "gmina_kazimierz_topo10_65") & Path.DirectorySeparatorChar
        Dim folderOrto = Path.Combine(folderDownload, "gmina_kazimierz_orto") & Path.DirectorySeparatorChar

        Directory.CreateDirectory(folderTopo25)
        Directory.CreateDirectory(folderTopo10)
        Directory.CreateDirectory(folderOrto)

        ' Obszar Kazimierza Dolnego w PUWG 1992 (EPSG:2180): 2048 x 2048 m
        Dim xDol As Double = 384500
        Dim yLewy As Double = 704000
        Dim xGora As Double = 386548
        Dim yPrawy As Double = 706048
        Dim zasiegObszaru As New Zasieg(xDol, yLewy, xGora, yPrawy)

        ' 1. Pobieranie Warstwy 1: Topo 25_65 (piksel 2m, bok 512px -> 2x2 segmenty)
        Dim siatka25 As New Siatka(zasiegObszaru, 2.0, 512)
        ZapiszKonfiguracje(folderTopo25, "skany_map_topograficznych", "Raster_25_1965", siatka25, "jpeg",
                           "https://mapy.geoportal.gov.pl/wss/service/img/guest/TOPO_SERIA/MapServer/WMSServer?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=")
        Dim zadanie25 As New ZadaniePobierania With {
            .Folder = folderTopo25,
            .AdresSerwera = "https://mapy.geoportal.gov.pl/wss/service/img/guest/TOPO_SERIA/MapServer/WMSServer?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=",
            .Warstwy = New List(Of String) From {"Raster_25_1965"},
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatka25,
            .Format = "jpeg",
            .LiczbaWatkow = 4
        }
        Dim wynik25 = Await PobieranieSegmentow.PobierzAsync(zadanie25, Nothing, CancellationToken.None)
        Assert.Equal(0, wynik25.Nieudane)

        ' 2. Pobieranie Warstwy 2: Topo 10_65 (piksel 1m, bok 512px -> 4x4 segmenty)
        Dim siatka10 As New Siatka(zasiegObszaru, 1.0, 512)
        ZapiszKonfiguracje(folderTopo10, "skany_map_topograficznych", "Raster_10_1965", siatka10, "jpeg",
                           "https://mapy.geoportal.gov.pl/wss/service/img/guest/TOPO_SERIA/MapServer/WMSServer?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=")
        Dim zadanie10 As New ZadaniePobierania With {
            .Folder = folderTopo10,
            .AdresSerwera = "https://mapy.geoportal.gov.pl/wss/service/img/guest/TOPO_SERIA/MapServer/WMSServer?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=",
            .Warstwy = New List(Of String) From {"Raster_10_1965"},
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatka10,
            .Format = "jpeg",
            .LiczbaWatkow = 4
        }
        Dim wynik10 = Await PobieranieSegmentow.PobierzAsync(zadanie10, Nothing, CancellationToken.None)
        Assert.Equal(0, wynik10.Nieudane)

        ' 3. Pobieranie Warstwy 3: Ortofotomapa (piksel 1m, bok 512px -> 4x4 segmenty)
        Dim siatkaOrto As New Siatka(zasiegObszaru, 1.0, 512)
        ZapiszKonfiguracje(folderOrto, "ortofotomapa", "Raster", siatkaOrto, "jpeg",
                           "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/HighResolution?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0&CRS=EPSG:2180&LAYERS=Raster")
        Dim zadanieOrto As New ZadaniePobierania With {
            .Folder = folderOrto,
            .AdresSerwera = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/HighResolution?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0&CRS=EPSG:2180&LAYERS=Raster",
            .Warstwy = New List(Of String) From {"Raster"},
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatkaOrto,
            .Format = "jpeg",
            .LiczbaWatkow = 4
        }
        Dim wynikOrto = Await PobieranieSegmentow.PobierzAsync(zadanieOrto, Nothing, CancellationToken.None)
        Assert.Equal(0, wynikOrto.Nieudane)

        ' 4. Kompilacja Wielowarstwowego MBTiles
        ' Definicja 3 warstw:
        ' Zoom 13-14: Topo 25_65
        ' Zoom 15-16: Topo 10_65
        ' Zoom 17-18: Ortofotomapa
        Dim w1 As New WarstwaWielowarstwowa With {.Folder = folderTopo25, .Tytul = "Topo 25_65 (1:25k)", .PoziomMin = 13, .PoziomMax = 14, .Kolejnosc = 1}
        Dim b1 As String = "" : Assert.True(w1.WczytajSesje(b1), b1)

        Dim w2 As New WarstwaWielowarstwowa With {.Folder = folderTopo10, .Tytul = "Topo 10_65 (1:10k)", .PoziomMin = 15, .PoziomMax = 16, .Kolejnosc = 2}
        Dim b2 As String = "" : Assert.True(w2.WczytajSesje(b2), b2)

        Dim w3 As New WarstwaWielowarstwowa With {.Folder = folderOrto, .Tytul = "Ortofotomapa", .PoziomMin = 17, .PoziomMax = 18, .Kolejnosc = 3}
        Dim b3 As String = "" : Assert.True(w3.WczytajSesje(b3), b3)

        Dim plikMbtiles = Path.Combine(folderDownload, "gmina_kazimierz_multiscale.mbtiles")
        Dim opcje As New OpcjeKompilatoraMbtiles With {
            .PlikDocelowy = plikMbtiles,
            .NazwaMapy = "Kazimierz Dolny Wielowarstwowy",
            .OpisMapy = "Z=13-14: Topo 25_65 | Z=15-16: Topo 10_65 | Z=17-18: Ortofotomapa",
            .Atrybucja = "Geoportal / MapoTero",
            .PoziomMin = 13,
            .PoziomMax = 18,
            .FormatKafli = FormatKafliMbtiles.SmartHybrid,
            .JakoscJpeg = 85,
            .SzybkaPiramidaWPasmach = True
        }

        Dim warstwyProjektu As New List(Of WarstwaWielowarstwowa) From {w1, w2, w3}

        Await KompilatorMbtiles.KompilujAsync(warstwyProjektu, opcje, Nothing, CancellationToken.None)

        Assert.True(File.Exists(plikMbtiles), "Plik MBTiles nie został utworzony.")
        Dim fi As New FileInfo(plikMbtiles)
        Assert.True(fi.Length > 100000, "Rozmiar pliku MBTiles jest za mały: " & fi.Length)
    End Function

    Private Shared Sub ZapiszKonfiguracje(folder As String, rodzajMapy As String, warstwa As String, siatka As Siatka, format As String, adresSerwera As String)
        Dim c As New KonfiguracjaSesji With {
            .Folder = folder,
            .RodzajMapy = rodzajMapy,
            .XDol = siatka.ZasiegSiatki.XDol.ToString(Globalization.CultureInfo.InvariantCulture),
            .YLewy = siatka.ZasiegSiatki.YLewy.ToString(Globalization.CultureInfo.InvariantCulture),
            .XGora = siatka.ZasiegSiatki.XGora.ToString(Globalization.CultureInfo.InvariantCulture),
            .YPrawy = siatka.ZasiegSiatki.YPrawy.ToString(Globalization.CultureInfo.InvariantCulture),
            .RozmiarPiksela = siatka.RozmiarPiksela.ToString(Globalization.CultureInfo.InvariantCulture),
            .BokSegmentuPx = siatka.BokSegmentuPx.ToString(Globalization.CultureInfo.InvariantCulture),
            .UkladEpsg = 2180,
            .Format = format,
            .Prefiks = "_",
            .Numeracja = "1",
            .DataPobrania = DateTime.Now.ToString("yyyy-MM-dd"),
            .AdresSerwera = adresSerwera
        }
        c.Warstwy(0) = warstwa
        c.LiczbaWarstw = 1
        c.Zapisz(Path.Combine(folder, "conf.txt"), System.Text.Encoding.UTF8)
    End Sub

End Class
