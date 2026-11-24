Option Strict On
Option Explicit On
Option Infer On

Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class PodgladGaleriiTesty

    Private Shared Function ZnajdzFolderPomocyRepo() As String
        Dim kandydaci = {
            Path.Combine(AppContext.BaseDirectory, "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "pomoc")
        }
        For Each sc In kandydaci
            Dim pelna = Path.GetFullPath(sc)
            If File.Exists(Path.Combine(pelna, "galeria", "warstwy.json")) Then Return pelna
        Next
        Return ""
    End Function

    Private Shared Function WolnyPortTcp() As Integer
        Dim l As New TcpListener(IPAddress.Loopback, 0)
        l.Start()
        Dim port = DirectCast(l.LocalEndpoint, IPEndPoint).Port
        l.Stop()
        Return port
    End Function

    <Fact>
    Public Sub KatalogGaleriiWarstw_WczytujePrawdziwyWarstwyJsonIIndeksujePoZbiorzeIWarstwie()
        Dim pomoc = ZnajdzFolderPomocyRepo()
        Assert.NotEqual("", pomoc)

        Dim json = File.ReadAllText(Path.Combine(pomoc, "galeria", "warstwy.json"), Encoding.UTF8)
        Dim kat = KatalogGaleriiWarstw.ZTekstu(json)

        Assert.True(kat.Pozycje.Count > 100)
        Dim topo = kat.Znajdz("TOPO_WMTS", "MAPA TOPOGRAFICZNA")
        Assert.NotNull(topo)
        Assert.Equal("WMTS", topo.Typ)
        Assert.False(String.IsNullOrWhiteSpace(topo.Obrazek))

        Dim raster65 = kat.Znajdz("skany_map_topograficznych", "Raster_10_1965")
        Assert.NotNull(raster65)
        Assert.NotNull(raster65.Zasieg)
        Assert.Equal("czesciowy", raster65.Zasieg.Typ)
        Assert.Equal(140, raster65.Zasieg.MaskaHex.Length)

        Assert.Null(kat.Znajdz("prywatny_serwer", "nieistniejaca_warstwa"))
    End Sub

    <Fact>
    Public Sub DekodujMaskeHex_DekodujeKomorkiZZachowaniemDziur()
        Assert.Empty(GeometriaZasieguGalerii.DekodujMaskeHex(""))
        Assert.Empty(GeometriaZasieguGalerii.DekodujMaskeHex("abcd"))

        Dim pomoc = ZnajdzFolderPomocyRepo()
        Dim json = File.ReadAllText(Path.Combine(pomoc, "galeria", "warstwy.json"), Encoding.UTF8)
        Dim kat = KatalogGaleriiWarstw.ZTekstu(json)
        Dim raster65 = kat.Znajdz("skany_map_topograficznych", "Raster_10_1965")
        Assert.NotNull(raster65)

        Dim komorki = GeometriaZasieguGalerii.DekodujMaskeHex(raster65.Zasieg.MaskaHex)
        Assert.True(komorki.Count > 200)
        Assert.True(komorki.Count < GeometriaZasieguGalerii.LiczbaKolumnMaski * GeometriaZasieguGalerii.LiczbaWierszyMaski)
    End Sub

    <Fact>
    Public Sub DekoderWebp_DekodujePrawdziwyPlikWebpPrzezWic()
        Dim pomoc = ZnajdzFolderPomocyRepo()
        Dim plikWebp = Path.Combine(pomoc, "obrazy", "galeria", "TOPO_WMTS__MAPA_TOPOGRAFICZNA.webp")
        Assert.True(File.Exists(plikWebp), "Brak testowego pliku WebP w pomoc/obrazy/galeria.")

        Dim bladKodeka = False
        Using bmp = DekoderWebp.DekodujZPliku(plikWebp, bladKodeka)
            ' Na Windows Server (np. GitHub Actions windows-latest) brak systemowego rozszerzenia WebP WIC —
            ' wtedy dekoder powinien ustawić bladKodeka = True i bezpiecznie zwrócić Nothing.
            If bladKodeka Then
                Assert.Null(bmp)
            Else
                Assert.NotNull(bmp)
                Assert.True(bmp.Width > 50)
                Assert.True(bmp.Height > 50)
            End If
        End Using
    End Sub

    <Fact>
    Public Sub RendererZasieguGalerii_RenderujeRozneTypyZasiegu()
        Dim pomoc = ZnajdzFolderPomocyRepo()
        Dim json = File.ReadAllText(Path.Combine(pomoc, "galeria", "warstwy.json"), Encoding.UTF8)
        Dim kat = KatalogGaleriiWarstw.ZTekstu(json)

        Dim probki = {
            kat.Znajdz("TOPO_WMTS", "MAPA TOPOGRAFICZNA"),
            kat.Znajdz("skany_map_topograficznych", "Raster_10_1965"),
            kat.Znajdz("OSMapa_PL", "osmapa"),
            kat.Znajdz("OpenTopoMap", "opentopomap"),
            kat.Znajdz("rejestr_granic_jednostki_teryt", "W03_Morskie_wody_wewnetrzne"),
            kat.Znajdz("czechy_topo_WMTS", "ZTM_WM"),
            kat.Znajdz("niemcy_brandenburgia_dtk10_wms", "bb_dtk10_farbe"),
            kat.Znajdz("niemcy_saksonia_dtk_wms", "sn_dtk25_p_color"),
            kat.Znajdz("niemcy_basemapde_WMTS", "de_basemapde_web_raster_farbe")
        }

        Assert.True(GeometriaZasieguGalerii.Polska.Length > 50)
        Assert.True(GeometriaZasieguGalerii.DaniaWielokaty.Length > 1)
        Assert.True(GeometriaZasieguGalerii.WislaOdcinki.Length >= 1)

        For Each p In probki
            Assert.NotNull(p)
            Using bmp = RendererZasieguGalerii.Renderuj(p, 330, 288)
                Assert.Equal(330, bmp.Width)
                Assert.Equal(288, bmp.Height)
            End Using
        Next
    End Sub

    Private Shared Function UtworzTestowyObrazPng() As Byte()
        Using bmp As New Bitmap(64, 64)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.ForestGreen)
            End Using
            Using ms As New MemoryStream()
                bmp.Save(ms, Imaging.ImageFormat.Png)
                Return ms.ToArray()
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function MagazynPodgladuGalerii_PobieraZHttpIZapisujeWCacheNaDysku() As Task
        Dim pomoc = ZnajdzFolderPomocyRepo()
        Dim jsonBytes = File.ReadAllBytes(Path.Combine(pomoc, "galeria", "warstwy.json"))
        ' DekoderWebp obsługuje także PNG bez wymogu kodeka WebP WIC (ważne na runnerach Windows Server CI)
        Dim obrazBytes = UtworzTestowyObrazPng()

        Dim tempDir = Path.Combine(Path.GetTempPath(), "MapoTero_PodgladTest_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir)
        Dim port = WolnyPortTcp()
        Dim prefiks = $"http://127.0.0.1:{port}/pomoc/"
        Dim zadanieJson = 0
        Dim zadanieObraz = 0

        Try
            Using serwer As New HttpListener()
                serwer.Prefixes.Add(prefiks)
                serwer.Start()

                Dim petla = Task.Run(Async Function()
                                         While serwer.IsListening
                                             Try
                                                 Dim ctx = Await serwer.GetContextAsync()
                                                 Dim sc = ctx.Request.Url.AbsolutePath
                                                 If sc.EndsWith("warstwy.json", StringComparison.OrdinalIgnoreCase) Then
                                                     zadanieJson += 1
                                                     ctx.Response.ContentType = "application/json"
                                                     ctx.Response.ContentLength64 = jsonBytes.Length
                                                     Await ctx.Response.OutputStream.WriteAsync(jsonBytes, 0, jsonBytes.Length)
                                                 ElseIf sc.EndsWith("TOPO_WMTS__MAPA_TOPOGRAFICZNA.webp", StringComparison.OrdinalIgnoreCase) Then
                                                     zadanieObraz += 1
                                                     ctx.Response.ContentType = "image/png"
                                                     ctx.Response.ContentLength64 = obrazBytes.Length
                                                     Await ctx.Response.OutputStream.WriteAsync(obrazBytes, 0, obrazBytes.Length)
                                                 Else
                                                     ctx.Response.StatusCode = 404
                                                 End If
                                                 ctx.Response.Close()
                                             Catch
                                                 Exit While
                                             End Try
                                         End While
                                     End Function)

                Using magazyn1 As New MagazynPodgladuGalerii(tempDir, prefiks, "")
                    Dim ok = Await magazyn1.ZapewnijKatalogAsync()
                    Assert.True(ok)
                    Dim wynik = Await magazyn1.PobierzObrazProbkiAsync("TOPO_WMTS", "MAPA TOPOGRAFICZNA")
                    Assert.True(wynik.Sukces)
                    Assert.NotNull(wynik.Obraz)
                    wynik.Obraz.Dispose()

                    Dim zas = magazyn1.PobierzObrazZasiegu("skany_map_topograficznych", "Raster_10_1965")
                    Assert.True(zas.Sukces)
                    Assert.NotNull(zas.Obraz)
                    zas.Obraz.Dispose()
                End Using

                Assert.Equal(1, zadanieJson)
                Assert.Equal(1, zadanieObraz)
                serwer.Stop()
                Await petla
            End Using

            'Druga instancja bez serwera HTTP powinna wszystko wczytać z cache na dysku
            Using magazyn2 As New MagazynPodgladuGalerii(tempDir, prefiks, "", Nothing, wylaczSiec:=True)
                Assert.True(magazyn2.CzyDostepna("TOPO_WMTS", "MAPA TOPOGRAFICZNA"))
                Dim wynikCache = Await magazyn2.PobierzObrazProbkiAsync("TOPO_WMTS", "MAPA TOPOGRAFICZNA")
                Assert.True(wynikCache.Sukces)
                Assert.NotNull(wynikCache.Obraz)
                wynikCache.Obraz.Dispose()
            End Using
        Finally
            Try
                If Directory.Exists(tempDir) Then Directory.Delete(tempDir, True)
            Catch
            End Try
        End Try
    End Function

    <Fact>
    Public Sub FolderCacheGalerii_WskazujeLocalAppData()
        Dim folder = Global.MapoTero.Program.FolderCacheGalerii()
        Dim localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        If Not String.IsNullOrWhiteSpace(localAppData) Then
            Assert.Equal(Path.Combine(localAppData, "MapoTero"), folder)
        Else
            Assert.False(String.IsNullOrWhiteSpace(folder))
        End If
    End Sub

End Class
