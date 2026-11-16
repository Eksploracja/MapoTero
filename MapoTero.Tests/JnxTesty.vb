'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class JnxTesty

    <Fact>
    Public Sub PrzeliczenieSemicircles()
        ' 0 stopni = 0
        Assert.Equal(0, ZapisJnx.StopnieDoSemicircles(0))
        Assert.Equal(0.0, ZapisJnx.SemicirclesDoStopni(0), 6)

        ' 180 stopni = 2^31 (Integer.MaxValue)
        Dim semi180 = ZapisJnx.StopnieDoSemicircles(180.0)
        Assert.Equal(Integer.MaxValue, semi180)

        ' -180 stopni = -2^31 (Integer.MinValue)
        Dim semiMinus180 = ZapisJnx.StopnieDoSemicircles(-180.0)
        Assert.Equal(Integer.MinValue, semiMinus180)

        ' Warszawa: 52.2297 N, 21.0122 E
        Dim semiSz = ZapisJnx.StopnieDoSemicircles(52.2297)
        Dim semiDl = ZapisJnx.StopnieDoSemicircles(21.0122)
        Assert.Equal(52.2297, ZapisJnx.SemicirclesDoStopni(semiSz), 6)
        Assert.Equal(21.0122, ZapisJnx.SemicirclesDoStopni(semiDl), 6)
    End Sub

    <Fact>
    Public Sub SkalaDlaPoziomowJnx()
        Dim s10 = ZapisJnx.SkalaDlaPoziomu(10, 52.0)
        Dim s12 = ZapisJnx.SkalaDlaPoziomu(12, 52.0)
        Dim s14 = ZapisJnx.SkalaDlaPoziomu(14, 52.0)
        Dim s16 = ZapisJnx.SkalaDlaPoziomu(16, 52.0)

        ' Im większe zbliżenie (większy zoom), tym mniejsza wartość parametru skali w Garminie
        Assert.True(s10 > s12)
        Assert.True(s12 > s14)
        Assert.True(s14 > s16)
        Assert.True(s16 > 0)
    End Sub

    <Fact>
    Public Sub GenerowanieProductId()
        Dim id1 = ZapisJnx.GenerujProductId("Tatry_Orto")
        Dim id2 = ZapisJnx.GenerujProductId("Tatry_Orto")
        Dim id3 = ZapisJnx.GenerujProductId("Beskidy_Topo")

        Assert.True(id1 > 0)
        Assert.Equal(id1, id2)
        Assert.NotEqual(id1, id3)
    End Sub

    <Fact>
    Public Sub ZapisIWeryfikacjaFormatJnx()
        Dim plik = Path.Combine(Path.GetTempPath(), "test_mapa_" & Guid.NewGuid().ToString("N") & ".jnx")
        Try
            ' Utworzenie testowego obrazka JPEG
            Dim jpegBytes As Byte()
            Using bmp As New Bitmap(256, 256, PixelFormat.Format24bppRgb), ms As New MemoryStream()
                Using g = Graphics.FromImage(bmp)
                    g.Clear(Color.ForestGreen)
                End Using
                bmp.Save(ms, ImageFormat.Jpeg)
                jpegBytes = ms.ToArray()
            End Using

            ' Dwa poziomy powiększenia
            Dim p1 As New PoziomJnx(ZapisJnx.SkalaDlaPoziomu(12, 52.0))
            p1.Kafle.Add(New KafelJnx(New ProstokatGeo(52.2, 52.1, 21.1, 21.0), jpegBytes))

            Dim p2 As New PoziomJnx(ZapisJnx.SkalaDlaPoziomu(13, 52.0))
            p2.Kafle.Add(New KafelJnx(New ProstokatGeo(52.2, 52.15, 21.05, 21.0), jpegBytes))
            p2.Kafle.Add(New KafelJnx(New ProstokatGeo(52.2, 52.15, 21.1, 21.05), jpegBytes))
            p2.Kafle.Add(New KafelJnx(New ProstokatGeo(52.15, 52.1, 21.05, 21.0), jpegBytes))
            p2.Kafle.Add(New KafelJnx(New ProstokatGeo(52.15, 52.1, 21.1, 21.05), jpegBytes))

            Dim poziomy As New List(Of PoziomJnx) From {p1, p2}
            ZapisJnx.Zapisz(plik, "MojaMapaJnx", 75, poziomy)

            Assert.True(File.Exists(plik))
            Dim fi As New FileInfo(plik)
            Assert.True(fi.Length > jpegBytes.Length * 5)

            Dim wersja As Integer, liczbaPoziomow As Integer, zOrder As Integer, productId As Integer, tytul As String = ""
            Dim ok = ZapisJnx.WeryfikujJnx(plik, wersja, liczbaPoziomow, zOrder, productId, tytul)
            Assert.True(ok)
            Assert.Equal(4, wersja)
            Assert.Equal(2, liczbaPoziomow)
            Assert.Equal(75, zOrder)
            Assert.Equal("MojaMapaJnx", tytul)
            Assert.True(productId > 0)
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub EksportDoJnxZSegmentow()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_JnxTest_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(folder)
        Dim plikJnx = folder & "wynik.jnx"
        Try
            ' Utworzenie 4 segmentów testowych
            For w = 1 To 2
                For k = 1 To 2
                    Using bmp As New Bitmap(256, 256, PixelFormat.Format24bppRgb)
                        Using g = Graphics.FromImage(bmp)
                            g.Clear(Color.Navy)
                        End Using
                        bmp.Save(folder & "_" & w.ToString("D2") & "_" & k.ToString("D2") & ".jpg", ImageFormat.Jpeg)
                    End Using
                Next
            Next

            Dim siatka As New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
            Dim zrodlo As New ZrodloSegmentow(folder, "_", StylNumeracji.WierszKolumna, "jpg", siatka)
            Dim m As New MetadaneMapy With {.Tytul = "Test JNX", .DataPobrania = New Date(2026, 11, 15)}
            Dim zad As New ZadanieEksportu With {
                .Rodzaj = RodzajEksportu.Jnx,
                .Zrodlo = zrodlo,
                .Uklad = UkladWspolrzednych.PL1992,
                .Plik = plikJnx,
                .PoziomMin = 15,
                .PoziomMax = 16,
                .JakoscJpeg = 85,
                .KolejnoscRysowania = 50,
                .Metadane = m,
                .Nazwa = "Test JNX"
            }

            Assert.True(zad.Piramida)
            Assert.False(zad.Png)

            EksportMapy.Eksportuj(zad, Nothing, CancellationToken.None)

            Assert.True(File.Exists(plikJnx))
            ' Plik tymczasowy bazy sqlite musi być usunięty
            Assert.False(File.Exists(plikJnx & ".tmp_tiles"))

            Dim wersja As Integer, liczbaPoziomow As Integer, zOrder As Integer, prodId As Integer, tytul As String = ""
            Dim ok = ZapisJnx.WeryfikujJnx(plikJnx, wersja, liczbaPoziomow, zOrder, prodId, tytul)
            Assert.True(ok)
            Assert.Equal(4, wersja)
            Assert.Equal(2, liczbaPoziomow)
            Assert.Equal(50, zOrder)
            Assert.Equal("Test JNX", tytul)
        Finally
            If Directory.Exists(folder) Then
                Try
                    Directory.Delete(folder, True)
                Catch
                End Try
            End If
        End Try
    End Sub

    <Fact>
    Public Sub PlanKmzInteligentnyLicznikKafliKorytarza()
        ' Obszar ok. 11 x 11 km, piksel 1 m -> siatka 11 x 11 kafli (121 kafli w prostokącie).
        ' Bez filtra siatka 121 kafli przekracza limit 100 kafli Garmina i wymusza pomniejszenie.
        Dim obszar As New ProstokatGeo(52.1, 52.0, 21.16, 21.0)
        Dim planBezFiltra As New PlanKmz(obszar, 1.0, 1024, 100)
        Assert.True(planBezFiltra.Pomniejszenie > 1.0)

        ' Z filtrem: korytarz biegnie ukośnie i obejmuje tylko 20 kafli
        Dim licznikWywołań As Integer = 0
        Dim filtrKorytarza As Func(Of ProstokatGeo, Boolean) =
            Function(zasieg)
                licznikWywołań += 1
                ' Symulacja: tylko kafle blisko przekątnej zawierają dane
                Dim srodekSz = (zasieg.Polnoc + zasieg.Poludnie) / 2.0
                Dim srodekDl = (zasieg.Wschod + zasieg.Zachod) / 2.0
                Dim delta = Math.Abs((srodekSz - 52.0) - (srodekDl - 21.0))
                Return delta < 0.04
            End Function

        Dim planZFiltrem As New PlanKmz(obszar, 1.0, 1024, 100, filtrKorytarza)
        ' Ponieważ aktywne kafle mieszczą się w limicie 100, pomniejszenie nie jest potrzebne (1.0)
        Assert.Equal(1.0, planZFiltrem.Pomniejszenie)
        Assert.True(planZFiltrem.LiczbaKafli <= 100)
        Assert.True(planZFiltrem.LiczbaKafli > 0)
    End Sub

    <Fact>
    Public Sub KmzKolejnoscRysowaniaINakladkaPng()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_KmzTest_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(folder)
        Dim plikKmz = folder & "nakladka.kmz"
        Try
            ' Utworzenie 4 segmentów PNG z przezroczystością
            For w = 1 To 2
                For k = 1 To 2
                    Using bmp As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                        Using g = Graphics.FromImage(bmp)
                            g.Clear(Color.Transparent)
                            g.DrawLine(Pens.Red, 0, 0, 256, 256)
                        End Using
                        bmp.Save(folder & "_" & w.ToString("D2") & "_" & k.ToString("D2") & ".png", ImageFormat.Png)
                    End Using
                Next
            Next

            Dim siatka As New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
            Dim zrodlo As New ZrodloSegmentow(folder, "_", StylNumeracji.WierszKolumna, "png", siatka)
            Dim m As New MetadaneMapy With {.Tytul = "Trasa rowerowa", .DataPobrania = New Date(2026, 11, 15)}
            Dim zad As New ZadanieEksportu With {
                .Rodzaj = RodzajEksportu.KmzGarmin,
                .Zrodlo = zrodlo,
                .Uklad = UkladWspolrzednych.PL1992,
                .Plik = plikKmz,
                .MaksKafli = 100,
                .KolejnoscRysowania = 75,
                .Nakladka = True,
                .Metadane = m,
                .Nazwa = "Trasa"
            }

            ' Garmin Custom Maps z Nakladka = True eksportuje kafle PNG
            Assert.True(zad.Png)

            EksportMapy.Eksportuj(zad, Nothing, CancellationToken.None)

            Assert.True(File.Exists(plikKmz))
            Using zip = System.IO.Compression.ZipFile.OpenRead(plikKmz)
                Dim wpisy = zip.Entries.Select(Function(e) e.FullName).ToList()
                Assert.Contains("doc.kml", wpisy)
                ' Sprawdzenie obecności plików png zamiast jpg
                Assert.Contains(wpisy, Function(n) n.EndsWith(".png", StringComparison.OrdinalIgnoreCase))

                ' Sprawdzenie drawOrder w doc.kml
                Using s = zip.GetEntry("doc.kml").Open(), sr As New StreamReader(s)
                    Dim kmlTekst = sr.ReadToEnd()
                    Assert.Contains("<drawOrder>75</drawOrder>", kmlTekst)
                End Using
            End Using
        Finally
            If Directory.Exists(folder) Then
                Try
                    Directory.Delete(folder, True)
                Catch
                End Try
            End If
        End Try
    End Sub

End Class
