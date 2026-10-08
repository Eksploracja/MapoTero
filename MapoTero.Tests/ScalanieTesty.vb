'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports BitMiracle.LibTiff.Classic
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class ScalanieTesty

    ''' <summary>Tworzy prosty kafel jednokolorowy.</summary>
    Private Shared Sub UtworzKafel(sciezka As String, szer As Integer, wys As Integer, kolor As Color, format As ImageFormat)
        Using bmp As New Bitmap(szer, wys, PixelFormat.Format24bppRgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(kolor)
            End Using
            bmp.Save(sciezka, format)
        End Using
    End Sub

    ''' <summary>Tworzy kafel PNG z kanałem alfa.</summary>
    Private Shared Sub UtworzKafelArgb(sciezka As String, szer As Integer, wys As Integer, kolor As Color)
        Using bmp As New Bitmap(szer, wys, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(kolor)
            End Using
            bmp.Save(sciezka, ImageFormat.Png)
        End Using
    End Sub

    ''' <summary>Tworzy przykładową siatkę 2x2 o boku 100 pikseli.</summary>
    Private Shared Function Siatka2x2(bokPx As Integer) As Siatka
        Dim obszar As New Zasieg(500000, 600000, 500000 + bokPx * 2, 600000 + bokPx * 2)
        Return New Siatka(obszar, 1.0, bokPx)
    End Function

    <Fact>
    Public Sub ScalaniePng2x2_PelnySukces()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 100
            Dim s = Siatka2x2(bok)

            ' 4 kafle: (1,1)=Czerwony, (1,2)=Zielony, (2,1)=Niebieski, (2,2)=Zolty
            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.FromArgb(255, 0, 0), ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_02.png", bok, bok, Color.FromArgb(0, 255, 0), ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_01.png", bok, bok, Color.FromArgb(0, 0, 255), ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", bok, bok, Color.FromArgb(255, 255, 0), ImageFormat.Png)

            Dim postepy As New List(Of Integer)()
            Dim postep As New Progress(Of Integer)(Sub(p) postepy.Add(p))

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_png"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, postep, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Assert.Empty(wynik.Brakujace)
            Assert.True(postepy.Count > 0)
            Assert.Equal(100, postepy.Last())

            Using arkusz As New Bitmap(wynik.Plik)
                Assert.Equal(200, arkusz.Width)
                Assert.Equal(200, arkusz.Height)

                ' Kafel (1,1) - lewy gorny: czerwony
                Dim p11 = arkusz.GetPixel(50, 50)
                Assert.Equal(255, p11.R)
                Assert.Equal(0, p11.G)
                Assert.Equal(0, p11.B)

                ' Kafel (1,2) - prawy gorny: zielony
                Dim p12 = arkusz.GetPixel(150, 50)
                Assert.Equal(0, p12.R)
                Assert.Equal(255, p12.G)
                Assert.Equal(0, p12.B)

                ' Kafel (2,1) - lewy dolny: niebieski
                Dim p21 = arkusz.GetPixel(50, 150)
                Assert.Equal(0, p21.R)
                Assert.Equal(0, p21.G)
                Assert.Equal(255, p21.B)

                ' Kafel (2,2) - prawy dolny: zolty
                Dim p22 = arkusz.GetPixel(150, 150)
                Assert.Equal(255, p22.R)
                Assert.Equal(255, p22.G)
                Assert.Equal(0, p22.B)

                ' Naroniki i granice (sprawdzenie braku przesuniec pikseli)
                Dim rog00 = arkusz.GetPixel(0, 0)
                Assert.Equal(255, rog00.R) : Assert.Equal(0, rog00.G) : Assert.Equal(0, rog00.B)

                Dim granica11 = arkusz.GetPixel(99, 99)
                Assert.Equal(255, granica11.R) : Assert.Equal(0, granica11.G) : Assert.Equal(0, granica11.B)

                Dim granica12 = arkusz.GetPixel(100, 0)
                Assert.Equal(0, granica12.R) : Assert.Equal(255, granica12.G) : Assert.Equal(0, granica12.B)

                Dim granica21 = arkusz.GetPixel(0, 100)
                Assert.Equal(0, granica21.R) : Assert.Equal(0, granica21.G) : Assert.Equal(255, granica21.B)

                Dim rog199 = arkusz.GetPixel(199, 199)
                Assert.Equal(255, rog199.R) : Assert.Equal(255, rog199.G) : Assert.Equal(0, rog199.B)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieGeoTiff_DeflateZGeoreferencja()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 50
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & "_01_01.jpg", bok, bok, Color.Red, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_01_02.jpg", bok, bok, Color.Lime, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_02_01.jpg", bok, bok, Color.Blue, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_02_02.jpg", bok, bok, Color.White, ImageFormat.Jpeg)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "jpg",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.GeoTiff,
                .NazwaArkusza = "arkusz_geotiff"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Assert.Empty(wynik.Brakujace)

            Using t = Tiff.Open(wynik.Plik, "r")
                Assert.NotNull(t)
                Assert.Equal(100, t.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(100, t.GetField(TiffTag.IMAGELENGTH)(0).ToInt())

                ' Znaczniki georeferencji GeoTIFF
                Dim skala = t.GetField(CType(33550, TiffTag))
                Assert.NotNull(skala)
                Dim punkt = t.GetField(CType(33922, TiffTag))
                Assert.NotNull(punkt)
                Dim klucze = t.GetField(CType(34735, TiffTag))
                Assert.NotNull(klucze)

                ' Kod EPSG 2180 dla PL1992
                Dim k = klucze(1).ToShortArray()
                Assert.Equal(CShort(2180), k(15))

                ' Odczyt skanlinii sekwencyjnie (LibTiff wymaga czytania po kolei w obrebie pasow)
                Dim wiersz(100 * 3 - 1) As Byte
                For y = 0 To 20
                    t.ReadScanline(wiersz, y)
                Next
                ' Kafel (1,1) czerwony z kompresja JPEG (z tolerancja)
                Assert.InRange(CInt(wiersz(25 * 3)), 200, 255) ' R
                Assert.InRange(CInt(wiersz(25 * 3 + 1)), 0, 50) ' G
                Assert.InRange(CInt(wiersz(25 * 3 + 2)), 0, 50) ' B

                ' Kafel (1,2) zielony z kompresja JPEG
                Assert.InRange(CInt(wiersz(75 * 3)), 0, 50) ' R
                Assert.InRange(CInt(wiersz(75 * 3 + 1)), 200, 255) ' G
                Assert.InRange(CInt(wiersz(75 * 3 + 2)), 0, 50) ' B
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieGeoTiffJpeg_Stratny()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 40
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.Red, ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_02.png", bok, bok, Color.Blue, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_01.png", bok, bok, Color.Green, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", bok, bok, Color.Yellow, ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.GeoTiffJpeg,
                .JakoscJpeg = 85,
                .NazwaArkusza = "arkusz_gtiff_jpg"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Using t = Tiff.Open(wynik.Plik, "r")
                Assert.NotNull(t)
                Assert.Equal(80, t.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(80, t.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieJpeg_PoprawneWymiary()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 50
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & "_01_01.jpg", bok, bok, Color.Cyan, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_01_02.jpg", bok, bok, Color.Magenta, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_02_01.jpg", bok, bok, Color.Yellow, ImageFormat.Jpeg)
            UtworzKafel(tempFolder & "_02_02.jpg", bok, bok, Color.Black, ImageFormat.Jpeg)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "jpg",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Jpeg,
                .JakoscJpeg = 90,
                .NazwaArkusza = "arkusz_jpeg"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Using img = Image.FromFile(wynik.Plik)
                Assert.Equal(100, img.Width)
                Assert.Equal(100, img.Height)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieBrakujaceKafle_BialePolaIRaportowanie()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 60
            Dim s = Siatka2x2(bok)

            ' Tworzymy tylko 3 z 4 kafli (brak kafelka _01_02.png)
            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.FromArgb(255, 0, 0), ImageFormat.Png)
            ' _01_02.png BRAKUJE!
            UtworzKafel(tempFolder & "_02_01.png", bok, bok, Color.FromArgb(0, 0, 255), ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", bok, bok, Color.FromArgb(0, 255, 0), ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_brakujacy"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            ' Sprawdzamy liste brakujacych kafli
            Assert.Single(wynik.Brakujace)
            Assert.Equal("_01_02.png", wynik.Brakujace(0))

            Using arkusz As New Bitmap(wynik.Plik)
                ' Kafel (1,1) zachowal swoj czerwony kolor
                Dim p11 = arkusz.GetPixel(30, 30)
                Assert.Equal(255, p11.R) : Assert.Equal(0, p11.G) : Assert.Equal(0, p11.B)

                ' Kafel (1,2) - brakujacy - jest bialy (255, 255, 255)
                Dim p12 = arkusz.GetPixel(90, 30)
                Assert.Equal(255, p12.R) : Assert.Equal(255, p12.G) : Assert.Equal(255, p12.B)

                ' Kafel (2,1) zachowal swoj niebieski kolor
                Dim p21 = arkusz.GetPixel(30, 90)
                Assert.Equal(0, p21.R) : Assert.Equal(0, p21.G) : Assert.Equal(255, p21.B)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Theory>
    <InlineData(StylNumeracji.Kolejny, "_1.png", "_2.png", "_3.png", "_4.png")>
    <InlineData(StylNumeracji.KolejnyDwucyfrowy, "_01.png", "_02.png", "_03.png", "_04.png")>
    Public Sub ScalanieRozneStyleNumeracji(styl As StylNumeracji, plik1 As String, plik2 As String, plik3 As String, plik4 As String)
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 40
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & plik1, bok, bok, Color.Red, ImageFormat.Png)
            UtworzKafel(tempFolder & plik2, bok, bok, Color.Green, ImageFormat.Png)
            UtworzKafel(tempFolder & plik3, bok, bok, Color.Blue, ImageFormat.Png)
            UtworzKafel(tempFolder & plik4, bok, bok, Color.Yellow, ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = styl,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_styl_" & styl.ToString()
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Assert.Empty(wynik.Brakujace)

            Using arkusz As New Bitmap(wynik.Plik)
                Assert.Equal(80, arkusz.Width)
                Assert.Equal(80, arkusz.Height)
                Dim p1 = arkusz.GetPixel(20, 20)
                Assert.Equal(255, p1.R) : Assert.Equal(0, p1.G) : Assert.Equal(0, p1.B)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieZGenerowaniemPlikowGeoreferencji()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 50
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.White, ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_02.png", bok, bok, Color.White, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_01.png", bok, bok, Color.White, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", bok, bok, Color.White, ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_geo",
                .Georeferencja = New OpcjeGeoreferencji With {
                    .WorldFile = True,
                    .Kml = True,
                    .Map = True,
                    .Tab = True
                }
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            ' Sprawdzamy wygenerowane pliki georeferencyjne
            Assert.True(File.Exists(tempFolder & "arkusz_geo.pngw"), "Brak pliku .pngw")
            Assert.True(File.Exists(tempFolder & "arkusz_geo.prj"), "Brak pliku .prj")
            Assert.True(File.Exists(tempFolder & "arkusz_geo.kml"), "Brak pliku .kml")
            Assert.True(File.Exists(tempFolder & "arkusz_geo.map"), "Brak pliku .map")
            Assert.True(File.Exists(tempFolder & "arkusz_geo.tab"), "Brak pliku .tab")

            Dim trescPrj = File.ReadAllText(tempFolder & "arkusz_geo.prj")
            Assert.Contains("PROJCS", trescPrj)

            Dim trescKml = File.ReadAllText(tempFolder & "arkusz_geo.kml")
            Assert.Contains("<GroundOverlay>", trescKml)
            Assert.Contains("arkusz_geo.png", trescKml)
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieKafliZPrzezroczystoscia()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 40
            Dim s = Siatka2x2(bok)

            ' Kafel w 100% przezroczysty powinien wtopic sie w biale tlo
            UtworzKafelArgb(tempFolder & "_01_01.png", bok, bok, Color.FromArgb(0, 0, 0, 0))
            ' Kafel polprzezroczysty czerwony (128 alfa na bialym daje okolo 255, 127, 127)
            UtworzKafelArgb(tempFolder & "_01_02.png", bok, bok, Color.FromArgb(128, 255, 0, 0))
            UtworzKafelArgb(tempFolder & "_02_01.png", bok, bok, Color.FromArgb(255, 0, 255, 0))
            UtworzKafelArgb(tempFolder & "_02_02.png", bok, bok, Color.FromArgb(255, 0, 0, 255))

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_alfa"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Using arkusz As New Bitmap(wynik.Plik)
                ' Przezroczysty kafel (1,1) stal sie bialy
                Dim p11 = arkusz.GetPixel(20, 20)
                Assert.Equal(255, p11.R)
                Assert.Equal(255, p11.G)
                Assert.Equal(255, p11.B)

                ' Polprzezroczysty czerwony kafel (1,2)
                Dim p12 = arkusz.GetPixel(60, 20)
                Assert.Equal(255, p12.R)
                Assert.InRange(p12.G, 120, 135)
                Assert.InRange(p12.B, 120, 135)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieKafliONiestandardowymRozmiarze()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 60
            Dim s = Siatka2x2(bok)

            ' Tworzymy kafelki o wymiarach 30x30 zamiast 60x60
            UtworzKafel(tempFolder & "_01_01.png", 30, 30, Color.Red, ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_02.png", 30, 30, Color.Green, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_01.png", 30, 30, Color.Blue, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", 30, 30, Color.Yellow, ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_skalowany"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Using arkusz As New Bitmap(wynik.Plik)
                Assert.Equal(120, arkusz.Width)
                Assert.Equal(120, arkusz.Height)

                Dim p11 = arkusz.GetPixel(30, 30)
                Assert.Equal(255, p11.R) : Assert.Equal(0, p11.G) : Assert.Equal(0, p11.B)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalaniePrzerwanieCancellationToken()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 100
            Dim s = Siatka2x2(bok)

            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.Red, ImageFormat.Png)

            Dim cts As New CancellationTokenSource()
            cts.Cancel() ' Anulowanie z gory

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_anulowany"
            }

            Assert.Throws(Of OperationCanceledException)(
                Sub() ScalanieSegmentow.Scal(zadanie, Nothing, cts.Token))

            ' Upewniamy sie, ze niedokonczony plik zostal usuniety z dysku
            Assert.False(File.Exists(tempFolder & "arkusz_anulowany.png"))
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieNiepoprawneParametry()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Dim zadanie As New ZadanieScalania With {
            .Folder = tempFolder,
            .Siatka = New Siatka(New Zasieg(0, 0, 0, 0), 0, 0)
        }

        ' Niepoprawna siatka
        Assert.Throws(Of ArgumentException)(Sub() ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None))

        ' Zbyt duzy arkusz dla JPEG (> 65500 px)
        Dim siatkaDuza As New Siatka(New Zasieg(0, 0, 70000, 70000), 1.0, 70000)
        Dim zadanieJpeg As New ZadanieScalania With {
            .Folder = tempFolder,
            .Siatka = siatkaDuza,
            .Format = FormatArkusza.Jpeg
        }
        Assert.Throws(Of ArgumentException)(Sub() ScalanieSegmentow.Scal(zadanieJpeg, Nothing, CancellationToken.None))

        ' Brak siatki (Nothing)
        Dim zadanieBezSiatki As New ZadanieScalania With {
            .Folder = tempFolder,
            .Siatka = Nothing
        }
        Assert.Throws(Of ArgumentException)(Sub() ScalanieSegmentow.Scal(zadanieBezSiatki, Nothing, CancellationToken.None))
    End Sub

    <Fact>
    Public Sub ScalanieSiatkaProstokatna3x2()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 40
            ' 3 kolumny, 2 wiersze
            Dim obszar As New Zasieg(500000, 600000, 500000 + bok * 2, 600000 + bok * 3)
            Dim s As New Siatka(obszar, 1.0, bok)
            Assert.Equal(3, s.LiczbaKolumn)
            Assert.Equal(2, s.LiczbaWierszy)

            ' Wiersz 1
            UtworzKafel(tempFolder & "_01_01.png", bok, bok, Color.Red, ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_02.png", bok, bok, Color.Lime, ImageFormat.Png)
            UtworzKafel(tempFolder & "_01_03.png", bok, bok, Color.Blue, ImageFormat.Png)

            ' Wiersz 2
            UtworzKafel(tempFolder & "_02_01.png", bok, bok, Color.Yellow, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_02.png", bok, bok, Color.Cyan, ImageFormat.Png)
            UtworzKafel(tempFolder & "_02_03.png", bok, bok, Color.Magenta, ImageFormat.Png)

            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_3x2"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Using arkusz As New Bitmap(wynik.Plik)
                Assert.Equal(120, arkusz.Width) ' 3 * 40
                Assert.Equal(80, arkusz.Height)  ' 2 * 40

                ' Sprawdzenie kolorow w 6 kaflach
                ' (1,1)
                Dim c11 = arkusz.GetPixel(20, 20)
                Assert.Equal(255, c11.R) : Assert.Equal(0, c11.G) : Assert.Equal(0, c11.B)
                ' (1,2)
                Dim c12 = arkusz.GetPixel(60, 20)
                Assert.Equal(0, c12.R) : Assert.Equal(255, c12.G) : Assert.Equal(0, c12.B)
                ' (1,3)
                Dim c13 = arkusz.GetPixel(100, 20)
                Assert.Equal(0, c13.R) : Assert.Equal(0, c13.G) : Assert.Equal(255, c13.B)
                ' (2,1)
                Dim c21 = arkusz.GetPixel(20, 60)
                Assert.Equal(255, c21.R) : Assert.Equal(255, c21.G) : Assert.Equal(0, c21.B)
                ' (2,2)
                Dim c22 = arkusz.GetPixel(60, 60)
                Assert.Equal(0, c22.R) : Assert.Equal(255, c22.G) : Assert.Equal(255, c22.B)
                ' (2,3)
                Dim c23 = arkusz.GetPixel(100, 60)
                Assert.Equal(255, c23.R) : Assert.Equal(0, c23.G) : Assert.Equal(255, c23.B)
            End Using
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ScalanieFolderBezUkosnika()
        Dim tempFolder = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempFolder)
        Try
            Dim bok = 40
            Dim s = Siatka2x2(bok)

            UtworzKafel(Path.Combine(tempFolder, "_01_01.png"), bok, bok, Color.Red, ImageFormat.Png)
            UtworzKafel(Path.Combine(tempFolder, "_01_02.png"), bok, bok, Color.Lime, ImageFormat.Png)
            UtworzKafel(Path.Combine(tempFolder, "_02_01.png"), bok, bok, Color.Blue, ImageFormat.Png)
            UtworzKafel(Path.Combine(tempFolder, "_02_02.png"), bok, bok, Color.Yellow, ImageFormat.Png)

            ' Folder przekazany BEZ konczacego ukosnika
            Dim zadanie As New ZadanieScalania With {
                .Folder = tempFolder,
                .Prefiks = "_",
                .Numeracja = StylNumeracji.WierszKolumna,
                .RozszerzenieSegmentow = "png",
                .Siatka = s,
                .Uklad = UkladWspolrzednych.PL1992,
                .Format = FormatArkusza.Png,
                .NazwaArkusza = "arkusz_bez_slasha"
            }

            Dim wynik = ScalanieSegmentow.Scal(zadanie, Nothing, CancellationToken.None)

            Assert.True(File.Exists(wynik.Plik))
            Assert.Empty(wynik.Brakujace)
            Assert.Equal(Path.Combine(tempFolder, "arkusz_bez_slasha.png"), wynik.Plik)
        Finally
            If Directory.Exists(tempFolder) Then Directory.Delete(tempFolder, True)
        End Try
    End Sub

End Class
