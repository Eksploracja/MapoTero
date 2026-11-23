'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Testy regresyjne dla poprawek wykrytych podczas audytu (GeoTIFF, współrzędne, kreator, WMS/XYZ, mieszanie, UI).
''' </summary>
Public Class AudytNaprawyTesty

    Private NotInheritable Class MockTileHandler
        Inherits HttpMessageHandler

        Public ReadOnly ZadaniaUrls As New List(Of String)()
        Private ReadOnly _pngBajty As Byte()

        Public Sub New()
            Using bmp As New Bitmap(64, 64, PixelFormat.Format24bppRgb)
                Using g = Graphics.FromImage(bmp)
                    g.Clear(Color.ForestGreen)
                End Using
                Using ms As New MemoryStream()
                    bmp.Save(ms, ImageFormat.Png)
                    _pngBajty = ms.ToArray()
                End Using
            End Using
        End Sub

        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            ZadaniaUrls.Add(request.RequestUri.AbsoluteUri)
            Dim resp As New HttpResponseMessage(HttpStatusCode.OK) With {
                .Content = New ByteArrayContent(_pngBajty)
            }
            resp.Content.Headers.ContentType = New Headers.MediaTypeHeaderValue("image/png")
            Return Task.FromResult(resp)
        End Function
    End Class

    Private Shared Sub WWatkuSta(akcja As Action)
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    akcja()
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw New Exception(blad.Message, blad)
    End Sub

    <Fact>
    Public Sub CzytnikGeoTiff_PobierzPikselBgra_I_PrjZUnit9001()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_AudytGeoTiff_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim tifPath = Path.Combine(folder, "test.tif")
            Dim prjPath = Path.Combine(folder, "test.prj")
            Using zap As New ZapisGeoTiff(tifPath, 8, 8, kompresjaJpeg:=False, jakosc:=90,
                                          uklad:=UkladWspolrzednych.PL1992, zasieg:=New Zasieg(500000, 500000, 500008, 500008),
                                          kanaly:=3, piramidy:=False)
                Dim rgb(8 * 8 * 3 - 1) As Byte
                For y = 0 To 7
                    For x = 0 To 7
                        Dim off = (y * 8 + x) * 3
                        rgb(off) = CByte(10 + x)
                        rgb(off + 1) = CByte(20 + y)
                        rgb(off + 2) = 150
                    Next
                Next
                zap.ZapiszWiersze(rgb, 8)
                zap.Zakoncz()
            End Using

            Dim wktPrj = "PROJCS[""PUWG 1992"",GEOGCS[""ETRS89"",DATUM[""ETRS89"",SPHEROID[""GRS 1980"",6378137,298.257222101]],PRIMEM[""Greenwich"",0],UNIT[""degree"",0.0174532925199433]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",19],PARAMETER[""scale_factor"",0.9993],PARAMETER[""false_easting"",500000],PARAMETER[""false_northing"",-5300000],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]]]"
            File.WriteAllText(prjPath, wktPrj)

            Assert.Null(CzytnikGeoTiff.RozpoznajEpsgZPlikuPrj(wktPrj))

            Using czytnik As New CzytnikGeoTiff(tifPath)
                Assert.Equal(2180, czytnik.Georeferencja.KodEpsg)
                Dim b As Byte, g As Byte, r As Byte, a As Byte
                Assert.True(czytnik.PobierzPikselBgra(3, 5, b, g, r, a))
                Assert.Equal(CByte(13), r)
                Assert.Equal(CByte(25), g)
                Assert.Equal(CByte(150), b)
                Assert.Equal(CByte(255), a)
                Assert.False(czytnik.PobierzPikselBgra(-1, 0, b, g, r, a))
                Assert.False(czytnik.PobierzPikselBgra(8, 0, b, g, r, a))
            End Using
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub WspolrzedneTekstu_RozpoznajeDziesietneIDms()
        Dim dec = WspolrzedneTekstu.Rozpoznaj("52.50°N, 21.20°E")
        Assert.NotNull(dec)
        Assert.InRange(dec.Punkt.Szerokosc, 52.499, 52.501)
        Assert.InRange(dec.Punkt.Dlugosc, 21.199, 21.201)

        Dim dms = WspolrzedneTekstu.Rozpoznaj("N 52°13'47"" E 21°00'44""")
        Assert.NotNull(dms)
        Assert.InRange(dms.Punkt.Szerokosc, 52.229, 52.2305)
        Assert.InRange(dms.Punkt.Dlugosc, 21.0115, 21.013)
    End Sub

    <Fact>
    Public Sub StylKreatora_UtworzStylWlasny_ZachowujeStopienOpcjonalnyIKrycieZero()
        Dim styl As New StylKreatora()
        styl.Wybierz(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"))
        styl.UstawRegulacje(False)

        Dim zapisany = styl.UtworzStylWlasny("Mój styl testowy", New Zasieg(490000, 540000, 495000, 545000))
        Assert.NotNull(zapisany)
        Assert.Contains(zapisany.StopnieSkali, Function(st) st.Opcjonalny)
        zapisany.StopnieSkali(0).Warstwy(0).Krycie = 0

        Dim plik = Path.Combine(Path.GetTempPath(), "MapoTero_szablon_" & Guid.NewGuid().ToString("N") & ".json")
        Try
            SzablonyWlasne.Zapisz(plik, {zapisany})
            Dim wczytane = SzablonyWlasne.Wczytaj(plik)
            Dim wczytany = Assert.Single(wczytane)
            Assert.Contains(wczytany.StopnieSkali, Function(st) st.Opcjonalny)
            Assert.Equal(0, wczytany.StopnieSkali(0).Warstwy(0).Krycie)
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Async Function KlientCapabilities_ZastepujeSubdomenyPlaceholderaS() As Task
        Using handler As New MockTileHandler()
            Dim wynik = Await KlientCapabilities.PobierzAsync(
                "https://{s}.tile.example.com/{z}/{x}/{y}.png",
                handler:=handler)
            Assert.True(wynik.Sukces, wynik.KomunikatBledu)
            Assert.NotNull(wynik.OpcjeKafli)
            Assert.Contains("a", wynik.OpcjeKafli.Subdomeny)
            Assert.NotEmpty(handler.ZadaniaUrls)
            Assert.All(handler.ZadaniaUrls, Sub(u) Assert.DoesNotContain("{s}", u, StringComparison.OrdinalIgnoreCase))
        End Using
    End Function

    <Fact>
    Public Sub WmsCapabilities_DziedziczyZasieg2180IObslugujeOsie111()
        Dim xml = "<?xml version=""1.0"" encoding=""UTF-8""?>
<WMT_MS_Capabilities version=""1.1.1"">
  <Service><Title>Test WMS</Title></Service>
  <Capability>
    <Request><GetMap><Format>image/png</Format><DCPType><HTTP><Get><OnlineResource xmlns:xlink=""http://www.w3.org/1999/xlink"" xlink:href=""http://example.com/wms?""/></Get></HTTP></DCPType></GetMap></Request>
    <Layer>
      <Title>Root</Title>
      <SRS>EPSG:2180</SRS>
      <BoundingBox SRS=""EPSG:2180"" minx=""200000"" miny=""300000"" maxx=""250000"" maxy=""360000""/>
      <Layer><Name>child</Name><Title>Dziecko</Title></Layer>
    </Layer>
  </Capability>
</WMT_MS_Capabilities>"
        Dim info = WmsCapabilities.Wczytaj(xml, "http://example.com/wms")
        Dim dziecko = info.ZnajdzWarstwe("child")
        Assert.NotNull(dziecko)
        Assert.True(dziecko.Zasieg2180.HasValue)
        Assert.Equal(300000.0, dziecko.Zasieg2180.Value.XDol)
        Assert.Equal(200000.0, dziecko.Zasieg2180.Value.YLewy)
        Assert.Equal(360000.0, dziecko.Zasieg2180.Value.XGora)
        Assert.Equal(250000.0, dziecko.Zasieg2180.Value.YPrawy)

        Dim url111 = Wms.ZapytanieGetMap("http://example.com/wms?VERSION=1.1.1", {"child"}, UkladWspolrzednych.PL1992,
                                         New Zasieg(300000, 200000, 301000, 201000), "image/png", 256, 256, False)
        Assert.Contains("BBOX=200000,300000,201000,301000", url111, StringComparison.OrdinalIgnoreCase)
    End Sub

    <Fact>
    Public Sub PlikWarstw_OdrzucaZarezerwowanaNazweKatalog()
        Dim blad As String = ""
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("katalog", blad))
        Assert.False(String.IsNullOrEmpty(blad))
    End Sub

    <Fact>
    Public Sub KatalogWarstw_SkroconaNazwaSesjiWieloserwerowej()
        Dim c As New KonfiguracjaSesji With {
            .RodzajMapy = "ortofoto",
            .ZbioryWarstw = "ortofoto|topografia",
            .LiczbaWarstw = 2
        }
        c.Warstwy(0) = "Raster"
        c.Warstwy(1) = "bdot10k"

        Dim nazwa = KatalogWarstw.PobierzDomyslny().SkroconaNazwaSesji(c)
        Assert.Contains("+", nazwa)
        Assert.Contains("BDOT10k", nazwa, StringComparison.OrdinalIgnoreCase)
    End Sub

    <Fact>
    Public Sub ZestawWarstw_NalozPolprzezroczysteTlo_ObliczaKanalyPoprawnie()
        Dim cel As Integer() = {CInt(&H80FF0000)} ' 50% alfa, czerwony
        Dim gora As Integer() = {CInt(&H800000FF)} ' 50% alfa, niebieski
        NakladanieWarstw.Naloz(cel, gora, 100)
        Dim alfaOut = (cel(0) >> 24) And &HFF
        Assert.InRange(alfaOut, 189, 193)
    End Sub

    <Fact>
    Public Sub EksportMapy_KafelZDzieciZBajtow_BezPolprzezroczystychBrzegow()
        Dim pngKafel As Byte()
        Using bmp As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.FromArgb(255, 30, 100, 200))
            End Using
            Using ms As New MemoryStream()
                bmp.Save(ms, ImageFormat.Png)
                pngKafel = ms.ToArray()
            End Using
        End Using

        Dim wynik = EksportMapy.KafelZDzieciZBajtow({pngKafel, pngKafel, pngKafel, pngKafel}, png:=True, jakoscJpeg:=90)
        Assert.NotNull(wynik)
        Using ms As New MemoryStream(wynik), bmp As New Bitmap(ms)
            Assert.Equal(255, bmp.GetPixel(0, 0).A)
            Assert.Equal(255, bmp.GetPixel(255, 0).A)
            Assert.Equal(255, bmp.GetPixel(0, 255).A)
            Assert.Equal(255, bmp.GetPixel(255, 255).A)
        End Using
    End Sub

    <Fact>
    Public Sub NakladkaWydruku_UjemnaEtykietaIDobierzKrok()
        Assert.Equal("-5", RendererNakladekWydruku.FormatujEtykieteWspolrzednej(-5000, 1000, pelneMetry:=False))
        Assert.Equal("-500 m", RendererNakladekWydruku.FormatujEtykieteWspolrzednej(-500, 100, pelneMetry:=False))

        Dim arkusz As New ZakresArkusza With {.SzerokoscPx = 100, .RozmiarPikselaM = 1.0}
        Assert.Equal(10000, RendererNakladekWydruku.DobierzKrokSiatki(arkusz, szerokoscM:=80000.0))
    End Sub

    <Fact>
    Public Sub GarminUsb_NazwaDocelowaPliku_ZachowujeRozszerzeniePrzyKropce()
        Dim wynik = MenadzerGarminUsb.NazwaDocelowaPliku("C:\mapy\gory.kmz", "Beskid.Wyspowy.2026")
        Assert.Equal("Beskid_Wyspowy_2026.kmz", wynik)
    End Sub

    <Fact>
    Public Sub FormZaawansowanyMbtiles_ColTrybZawieraRzezbeTerenu()
        WWatkuSta(
            Sub()
                Using frm As New FormZaawansowanyMbtiles()
                    Assert.Contains("7 Rzeźba terenu", frm.colTryb.Items.Cast(Of Object)().Select(Function(x) CStr(x)))
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormZasiegXY_OdrzucaNieprawidloweZnakiIWylaczaZaokraglanieWgs84()
        WWatkuSta(
            Sub()
                Using frm As New FormZasiegXY()
                    frm.CzyGeograficzny = True
                    Assert.False(frm.btnZaokraglijKm.Enabled)
                    Assert.False(frm.btnZaokraglijKm.Visible)

                    frm.XDol = "52.1"
                    frm.XGora = "52.2"
                    frm.YLewy = "21.1"
                    frm.YPrawy = "21.2"
                    frm.ZaokraglijDoKm()
                    Assert.Equal("52.1", frm.XDol)

                    Dim m = GetType(FormZasiegXY).GetMethod("SprobujLiczbe", BindingFlags.NonPublic Or BindingFlags.Static)
                    Assert.NotNull(m)
                    Dim argsSmiec As Object() = {"500000a", 0.0}
                    Assert.False(CBool(m.Invoke(Nothing, argsSmiec)))
                    Dim argsNan As Object() = {"NaN", 0.0}
                    Assert.False(CBool(m.Invoke(Nothing, argsNan)))
                    Dim argsOk As Object() = {"500000,25", 0.0}
                    Assert.True(CBool(m.Invoke(Nothing, argsOk)))
                    Assert.Equal(500000.25, CDbl(argsOk(1)), 4)
                End Using
            End Sub)
    End Sub

End Class
