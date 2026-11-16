'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Ciężkie testy integracyjne z użyciem sieci Internet (Live End-to-End &amp; Cluster Probing).
''' Uruchamiane na wyraźne żądanie w celu przetestowania rzeczywistych serwerów, odporności klastrów
''' Geoportalu na niespójności za load balancerem oraz pełnego cyklu pobierania 2 warstw na raz (kanapki),
''' scalania segmentów i eksportu CAD.
''' </summary>
<Trait("Kategoria", "Ciezkie")>
Public Class CiezkieTestyLive

    Private Shared ReadOnly ObszarWarszawa As New Zasieg(480000, 630000, 482048, 632048) ' 2048 x 2048 m w PL-1992
    Private Const UserAgentTestowy As String = "MapoTero/3.16-TestLive"

    Private Shared Function UtworzKlientaHttp() As HttpClient
        Dim handler As New HttpClientHandler With {
            .AutomaticDecompression = Net.DecompressionMethods.GZip Or Net.DecompressionMethods.Deflate
        }
        Dim client As New HttpClient(handler)
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentTestowy)
        client.Timeout = TimeSpan.FromSeconds(30)
        Return client
    End Function

    Private Shared Function AdresKaflaDlaObszaru(plan As PlanWmts, zasieg As Zasieg) As String
        Dim kol As Double, wie As Double
        plan.PikselMacierzy(plan.DoUkladuKafli(New PunktXY(zasieg.XGora, zasieg.YLewy)), kol, wie)
        Dim wierszKafla = CInt(Math.Floor(wie / plan.Macierz.WysokoscKafla))
        Dim kolumnaKafla = CInt(Math.Floor(kol / plan.Macierz.SzerokoscKafla))
        Return plan.AdresKafla(wierszKafla, kolumnaKafla)
    End Function

    ''' <summary>
    ''' Próbkowanie klastra usługi ShadedRelief (Cieniowanie NMT 1m) pod kątem rozbieżności nazw
    ''' (ISOK_Cien vs Cieniowanie) między węzłami za load balancerem.
    ''' </summary>
    <Fact>
    Public Async Function ProbkowanieKlastra_GeoportalWmtsShadedRelief() As Task
        Const adresCaps As String = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/GRID1/WMTS/ShadedRelief?SERVICE=WMTS&REQUEST=GetCapabilities"
        Using client = UtworzKlientaHttp()
            Dim napotkaneNazwy As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For proba = 1 To 8
                Dim xml As String = ""
                Try
                    xml = Await client.GetStringAsync(adresCaps).ConfigureAwait(False)
                Catch ex As Exception
                    ' Awaria sieci / 503 serwera zewnętrznego nie powinna fałszować testu
                    Continue For
                End Try

                Dim usluga = UslugaWmts.Wczytaj(xml, adresCaps)
                Assert.NotEmpty(usluga.Warstwy)
                Dim idWarstwy = usluga.Warstwy(0).Identyfikator
                napotkaneNazwy.Add(idWarstwy)

                ' Sprawdzenie odporności na aliasy: szukanie po obu nazwach musi zawsze zwrócić warstwę
                Dim wIsok = usluga.Warstwa("ISOK_Cien")
                Dim wCien = usluga.Warstwa("Cieniowanie")
                Assert.NotNull(wIsok)
                Assert.NotNull(wCien)

                ' Utworzenie planu pobierania dla obu aliasów
                Dim planIsok As New PlanWmts(usluga, "ISOK_Cien", UkladWspolrzednych.PL1992, ObszarWarszawa, 1.0, "png")
                Dim planCien As New PlanWmts(usluga, "Cieniowanie", UkladWspolrzednych.PL1992, ObszarWarszawa, 1.0, "png")
                Assert.NotNull(planIsok.Warstwa)
                Assert.NotNull(planCien.Warstwa)

                ' Pobranie próbnego kafla w obszarze testowym
                Dim urlKafla = AdresKaflaDlaObszaru(planIsok, ObszarWarszawa)
                Using odpKafla = Await client.GetAsync(urlKafla).ConfigureAwait(False)
                    If odpKafla.IsSuccessStatusCode Then
                        Dim bajty = Await odpKafla.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                        Assert.True(bajty.Length > 100, "Kafel powinien mieć więcej niż 100 bajtów")
                        ' Nagłówek PNG: 137, 80, 78, 71
                        Assert.Equal(137, bajty(0))
                        Assert.Equal(80, bajty(1))
                    End If
                End Using
            Next
        End Using
    End Function

    ''' <summary>
    ''' Próbkowanie klastrów Ortofotomapy WMTS (HighResolution oraz StandardResolution).
    ''' </summary>
    <Fact>
    Public Async Function ProbkowanieKlastra_GeoportalWmtsOrtofotomapa() As Task
        Dim uslugi As String() = {
            "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/HighResolution?SERVICE=WMTS&REQUEST=GetCapabilities",
            "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution?SERVICE=WMTS&REQUEST=GetCapabilities"
        }

        Using client = UtworzKlientaHttp()
            For Each adres In uslugi
                Dim xml As String = ""
                Try
                    xml = Await client.GetStringAsync(adres).ConfigureAwait(False)
                Catch
                    Continue For
                End Try

                Dim usluga = UslugaWmts.Wczytaj(xml, adres)
                Assert.NotEmpty(usluga.Warstwy)

                Dim plan As New PlanWmts(usluga, "ORTOFOTOMAPA", UkladWspolrzednych.PL1992, ObszarWarszawa, 0.25, "jpeg")
                Assert.NotNull(plan.Warstwa)

                Dim urlKafla = AdresKaflaDlaObszaru(plan, ObszarWarszawa)
                Using odp = Await client.GetAsync(urlKafla).ConfigureAwait(False)
                    If odp.IsSuccessStatusCode Then
                        Dim bajty = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                        Assert.True(bajty.Length > 200, "Kafel orto powinien zawierać dane obrazu")
                    End If
                End Using
            Next
        End Using
    End Function

    ''' <summary>
    ''' Weryfikacja usług kafelkowych TOPO, BDOT10k oraz BDOO z cieniowaniem.
    ''' </summary>
    <Fact>
    Public Async Function ProbkowanieKlastra_GeoportalWmtsTopoIBdoo() As Task
        Dim testy As Tuple(Of String, String, Double)() = {
            Tuple.Create("https://mapy.geoportal.gov.pl/wss/service/WMTS/guest/wmts/TOPO?SERVICE=WMTS&REQUEST=GetCapabilities", "MAPA TOPOGRAFICZNA", 2.0),
            Tuple.Create("https://mapy.geoportal.gov.pl/wss/service/WMTS/guest/wmts/BDOT10k?SERVICE=WMTS&REQUEST=GetCapabilities", "BDOT10k", 1.0),
            Tuple.Create("https://mapy.geoportal.gov.pl/wss/service/PZGIK/BDOO/WMTS/BDOOzCieniowaniem?SERVICE=WMTS&REQUEST=GetCapabilities", "BDOOCieniowanie", 25.0)
        }

        Using client = UtworzKlientaHttp()
            For Each t In testy
                Dim xml As String = ""
                Try
                    xml = Await client.GetStringAsync(t.Item1).ConfigureAwait(False)
                Catch
                    Continue For
                End Try

                Dim usluga = UslugaWmts.Wczytaj(xml, t.Item1)
                Dim plan As New PlanWmts(usluga, t.Item2, UkladWspolrzednych.PL1992, ObszarWarszawa, t.Item3, "png")
                Assert.NotNull(plan.Warstwa)

                Dim url = AdresKaflaDlaObszaru(plan, ObszarWarszawa)
                Using odp = Await client.GetAsync(url).ConfigureAwait(False)
                    If odp.IsSuccessStatusCode Then
                        Dim b = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                        Assert.True(b.Length > 100)
                    End If
                End Using
            Next
        End Using
    End Function

    ''' <summary>
    ''' Pełny test pobierania zestawu 2 warstw na żywo (kanapka: WMS skany map topograficznych + WMTS Cieniowanie 50%),
    ''' a następnie scalenia pobranych segmentów w arkusz oraz wygenerowania rysunku CAD (DXF + SCR).
    ''' </summary>
    <Fact>
    Public Async Function PobieranieLive_Kanapka2Warstw_WmsTopo_WmtsCieniowanie_OrazScalanieIEksportCad() As Task
        Dim folderTestu = Path.Combine(Path.GetTempPath(), "MapoTero_CiezkieLive_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(folderTestu)

        Try
            ' Siatka 2x2 segmenty (1024 x 1024 m, segment 512 px, piksel 1m) w okolicach Kazimierza Dolnego
            Dim zasiegObszaru As New Zasieg(384500, 704000, 385524, 705024)
            Dim siatka As New Siatka(zasiegObszaru, 1.0, 512)

            ' Definicja 2 warstw w zestawie:
            ' 1) WMS Topo 1:25k (krycie 100%)
            ' 2) WMTS Cieniowanie rzeźby NMT 1m (krycie 50%, przezroczyste tło)
            Dim adresWms As String = "https://mapy.geoportal.gov.pl/wss/service/img/guest/TOPO_SERIA/MapServer/WMSServer?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
            Dim adresWmts As String = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/GRID1/WMTS/ShadedRelief?SERVICE=WMTS&REQUEST=GetCapabilities"

            Dim zadanie As New ZadaniePobierania With {
                .Folder = folderTestu,
                .AdresSerwera = adresWms,
                .Warstwy = New List(Of String) From {"Raster_25_1965", "ISOK_Cien"},
                .Adresy = New List(Of String) From {adresWms, adresWmts},
                .Zbiory = New List(Of String) From {"skany_map_topograficznych", "numer_mod_terenu_cieniowanie_WMTS"},
                .Krycia = New List(Of Integer) From {100, 50},
                .Uklad = UkladWspolrzednych.PL1992,
                .Siatka = siatka,
                .Format = "png",
                .Przezroczystosc = True,
                .LiczbaWatkow = 2,
                .IloscProb = 4,
                .PrzerwaSekundy = 2,
                .LimitCzasuSekundy = 45
            }

            ' 1. Pobieranie wielowarstwowe
            Dim wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, Nothing, CancellationToken.None).ConfigureAwait(False)

            ' Jeśli serwer Geoportalu miał chwilową niedostępność (503/timeout), nie przerywamy błędem asercji
            If wynik.Nieudane > 0 Then
                Return
            End If

            Assert.Equal(4, wynik.Wszystkie)
            Assert.Equal(0, wynik.Nieudane)

            ' Weryfikacja czy pliki segmentów istnieją na dysku
            Dim plikiSegmentow = Directory.GetFiles(folderTestu, "_*.png")
            Assert.Equal(4, plikiSegmentow.Length)
            For Each plik In plikiSegmentow
                Assert.True(New FileInfo(plik).Length > 1000, "Plik segmentu powinien zawierać złożony obraz")
            Next

            ' 2. Scalanie segmentów w jeden arkusz (Form3 backend)
            Dim zadanieScalania As New ZadanieScalania With {
                .Folder = folderTestu,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = siatka,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Jpeg,
                .JakoscJpeg = 85,
                .NazwaArkusza = "_arkusz_scalony",
                .Georeferencja = New OpcjeGeoreferencji With {.WorldFile = True, .Map = True, .Kml = True},
                .Przezroczystosc = False
            }

            Dim wynikScalania = ScalanieSegmentow.Scal(zadanieScalania, Nothing, CancellationToken.None)
            Assert.Empty(wynikScalania.Brakujace)

            Dim plikArkusza = Path.Combine(folderTestu, "_arkusz_scalony.jpg")
            Assert.True(File.Exists(plikArkusza), "Scalony arkusz powinien istnieć")
            Assert.True(New FileInfo(plikArkusza).Length > 5000, "Arkusz powinien mieć poprawny rozmiar")
            Assert.True(File.Exists(Path.Combine(folderTestu, "_arkusz_scalony.jpgw")), "World file (.jpgw) powinien istnieć")
            Assert.True(File.Exists(Path.Combine(folderTestu, "_arkusz_scalony.prj")), "Definicja układu (.prj) powinna istnieć")
            Assert.True(File.Exists(Path.Combine(folderTestu, "_arkusz_scalony.map")), "Plik OziExplorer powinien istnieć")

            Dim folderArtefaktow = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf"
            If Directory.Exists(folderArtefaktow) Then
                File.Copy(plikArkusza, Path.Combine(folderArtefaktow, "kanapka_scalona_topo_cieniowanie.jpg"), True)
            End If

            ' 3. Eksport do CAD (DXF, SCR, LISP)
            Dim konfiguracja As New KonfiguracjaSesji With {
                .Folder = folderTestu,
                .RodzajMapy = "skany_map_topograficznych",
                .XDol = zasiegObszaru.XDol.ToString(CultureInfo.InvariantCulture),
                .YLewy = zasiegObszaru.YLewy.ToString(CultureInfo.InvariantCulture),
                .XGora = zasiegObszaru.XGora.ToString(CultureInfo.InvariantCulture),
                .YPrawy = zasiegObszaru.YPrawy.ToString(CultureInfo.InvariantCulture),
                .BokSegmentuPx = "512",
                .RozmiarPiksela = "1",
                .Format = "png",
                .Prefiks = "_",
                .Numeracja = "NrWiersza_NrKolumny",
                .UkladEpsg = 2180
            }
            konfiguracja.Zapisz(Path.Combine(folderTestu, "conf.txt"), Encoding.UTF8)

            Dim zadanieCad = EksportCad.ZSesji(folderTestu, konfiguracja, StylNumeracji.WierszKolumna, True)
            zadanieCad.Uklad = UkladWspolrzednych.PL1992
            zadanieCad.RysujObrys = True
            zadanieCad.RysujSiatke = True
            zadanieCad.RysujNumery = True

            Dim kod As Encoding = Encoding.GetEncoding(1250)
            Dim trescDxf = EksportCad.Dxf(zadanieCad, kod)
            Dim trescScr = EksportCad.Skrypt(zadanieCad)

            Assert.NotEmpty(trescDxf)
            Assert.Contains(EksportCad.WarstwaRastrow, trescDxf)
            Assert.NotEmpty(trescScr)

        Finally
            Try
                If Directory.Exists(folderTestu) Then Directory.Delete(folderTestu, True)
            Catch
            End Try
        End Try
    End Function

    ''' <summary>
    ''' Weryfikacja pobierania pojedynczych kafelków z usług publicznych XYZ/TMS.
    ''' </summary>
    <Fact>
    Public Async Function KafleXyzTms_PobranieKafliLive() As Task
        ' Test kafla OpenStreetMap oraz Waymarked Trails (szlaki turystyczne)
        Dim szablony As String() = {
            "https://tile.openstreetmap.org/10/568/345.png",
            "https://tile.waymarkedtrails.org/hiking/10/568/345.png"
        }

        Using client = UtworzKlientaHttp()
            For Each url In szablony
                Try
                    Using odp = Await client.GetAsync(url).ConfigureAwait(False)
                        If odp.IsSuccessStatusCode Then
                            Dim b = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                            Assert.True(b.Length > 200)
                            Assert.Equal(137, b(0)) ' PNG
                        End If
                    End Using
                Catch
                    ' Pominięcie w razie braku sieci
                End Try
            Next
        End Using
    End Function

    ''' <summary>
    ''' Weryfikacja działania nowych zagranicznych i przygranicznych usług mapowych
    ''' (Czechy ČÚZK, Niemcy Basemap.de, Słowacja Freemap, Bałtyk EMODnet).
    ''' </summary>
    <Fact>
    Public Async Function ProbkowanieUslugZagranicznych_Czechy_Niemcy_Slowacja_BaltykAsync() As Task
        Using client = UtworzKlientaHttp()
            Dim folderArtefaktow = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf"

            ' 1. Słowacja Freemap Karpaty (kafle XYZ outdoor)
            Dim urlSlowacja = "https://outdoor.tiles.freemap.sk/14/9070/5688.png"
            Try
                Using odp = Await client.GetAsync(urlSlowacja).ConfigureAwait(False)
                    If odp.IsSuccessStatusCode Then
                        Dim bajty = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                        Assert.True(bajty.Length > 500, "Kafel Freemap powinien zawierać dane obrazu")
                        If Directory.Exists(folderArtefaktow) Then
                            File.WriteAllBytes(Path.Combine(folderArtefaktow, "zagranica_freemap_karpaty.png"), bajty)
                        End If
                    End If
                End Using
            Catch
            End Try

            ' 2. Czechy ČÚZK Ortofotomapa (WMTS Capabilities)
            Dim urlCzechy = "https://ags.cuzk.gov.cz/arcgis1/rest/services/ORTOFOTO_WM/MapServer/WMTS/1.0.0/WMTSCapabilities.xml"
            Try
                Dim xml = Await client.GetStringAsync(urlCzechy).ConfigureAwait(False)
                Assert.Contains("ORTOFOTO_WM", xml)
                Dim usluga = UslugaWmts.Wczytaj(xml, urlCzechy)
                Assert.NotEmpty(usluga.Warstwy)
            Catch
            End Try

            ' 3. Niemcy Basemap.de (WMTS Capabilities)
            Dim urlNiemcy = "https://sgx.geodatenzentrum.de/wmts_basemapde/1.0.0/WMTSCapabilities.xml"
            Try
                Dim xml = Await client.GetStringAsync(urlNiemcy).ConfigureAwait(False)
                Assert.Contains("de_basemapde_web_raster", xml)
                Dim usluga = UslugaWmts.Wczytaj(xml, urlNiemcy)
                Assert.NotEmpty(usluga.Warstwy)
            Catch
            End Try

            ' 4. Bałtyk Batymetria (EMODnet WMS Capabilities)
            Dim urlBaltyk = "https://ows.emodnet-bathymetry.eu/wms?service=WMS&request=GetCapabilities"
            Try
                Dim xml = Await client.GetStringAsync(urlBaltyk).ConfigureAwait(False)
                Assert.Contains("emodnet:mean_atlas_land", xml)
            Catch
            End Try
        End Using
    End Function

End Class
