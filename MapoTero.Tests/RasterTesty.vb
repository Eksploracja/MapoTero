'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.IO.Compression
Imports BitMiracle.LibJpeg.Classic
Imports BitMiracle.LibTiff.Classic
Imports MapoTero.Core
Imports Xunit

Public Class RasterTesty

    Private Const W As Integer = 301
    Private Const H As Integer = 203

    ''' <summary>Obraz testowy: gradienty w kanałach R i G, stała wartość B.</summary>
    Private Shared Function Piksel(x As Integer, y As Integer, kanal As Integer) As Byte
        Select Case kanal
            Case 0 : Return CByte(x Mod 256)
            Case 1 : Return CByte(y Mod 256)
            Case Else : Return 77
        End Select
    End Function

    ''' <summary>Zapisuje obraz testowy porcjami po 50 wierszy.</summary>
    Private Shared Sub ZapiszObraz(z As ZapisRastra)
        Dim y As Integer = 0
        While y < H
            Dim n As Integer = Math.Min(50, H - y)
            Dim bufor(n * W * 3 - 1) As Byte
            For r = 0 To n - 1
                For x = 0 To W - 1
                    For k = 0 To 2
                        bufor((r * W + x) * 3 + k) = Piksel(x, y + r, k)
                    Next
                Next
            Next
            z.ZapiszWiersze(bufor, n)
            y += n
        End While
        z.Zakoncz()
    End Sub

    <Fact>
    Public Sub PngBezstratny()
        Dim plik = Path.GetTempFileName()
        Try
            Using z As New ZapisPng(plik, W, H)
                ZapiszObraz(z)
            End Using
            Dim dane = File.ReadAllBytes(plik)
            Assert.Equal("137,80,78,71,13,10,26,10", String.Join(",", dane.Take(8)))

            'odczyt fragmentów i dekompresja IDAT
            Dim idat As New MemoryStream()
            Dim poz As Integer = 8
            Dim koniec As Boolean = False
            While poz < dane.Length
                Dim dl As Integer = (CInt(dane(poz)) << 24) Or (CInt(dane(poz + 1)) << 16) Or (CInt(dane(poz + 2)) << 8) Or dane(poz + 3)
                Dim typ = Text.Encoding.ASCII.GetString(dane, poz + 4, 4)
                If typ = "IDAT" Then idat.Write(dane, poz + 8, dl)
                If typ = "IEND" Then koniec = True
                poz += 12 + dl
            End While
            Assert.True(koniec)
            idat.Position = 0
            Dim surowe As New MemoryStream()
            Using zl As New ZLibStream(idat, CompressionMode.Decompress)
                zl.CopyTo(surowe)
            End Using
            Dim s = surowe.ToArray()
            Assert.Equal(H * (W * 3 + 1), s.Length)
            Dim poprzedni(W * 3 - 1) As Byte
            For y = 0 To H - 1
                Assert.Equal(2, s(y * (W * 3 + 1)))   'filtr Up
                For i = 0 To W * 3 - 1
                    Dim b = CByte((s(y * (W * 3 + 1) + 1 + i) + poprzedni(i)) And &HFF)
                    poprzedni(i) = b
                    Assert.Equal(Piksel(i \ 3, y, i Mod 3), b)
                Next
            Next
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub JpegOdczytywalny()
        Dim plik = Path.GetTempFileName()
        Try
            Using z As New ZapisJpeg(plik, W, H, 95)
                ZapiszObraz(z)
            End Using
            Using f = File.OpenRead(plik)
                Dim d As New jpeg_decompress_struct(New jpeg_error_mgr())
                d.jpeg_stdio_src(f)
                d.jpeg_read_header(True)
                d.jpeg_start_decompress()
                Assert.Equal(W, d.Output_width)
                Assert.Equal(H, d.Output_height)
                Dim wiersz(0)() As Byte
                wiersz(0) = New Byte(W * 3 - 1) {}
                For y = 0 To H - 1
                    d.jpeg_read_scanlines(wiersz, 1)
                    If y = 100 Then
                        'kompresja stratna - porównanie z tolerancją
                        Assert.InRange(CInt(wiersz(0)(150 * 3)), 150 - 8, 150 + 8)
                        Assert.InRange(CInt(wiersz(0)(150 * 3 + 1)), 100 - 8, 100 + 8)
                        Assert.InRange(CInt(wiersz(0)(150 * 3 + 2)), 77 - 8, 77 + 8)
                    End If
                Next
                d.jpeg_finish_decompress()
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub JpegOgraniczenieRozmiaru()
        Assert.Throws(Of ArgumentException)(Function() New ZapisJpeg(Path.Combine(Path.GetTempPath(), "x.jpg"), 70000, 10, 80))
    End Sub

    <Theory>
    <InlineData(False)>
    <InlineData(True)>
    Public Sub GeoTiffZGeoreferencja(kompresjaJpeg As Boolean)
        Dim plik = Path.GetTempFileName()
        Try
            Dim zasieg As New Zasieg(500000, 600000, 500000 + H * 0.5, 600000 + W * 0.5)
            Using z As New ZapisGeoTiff(plik, W, H, kompresjaJpeg, 90, UkladWspolrzednych.PL1992, zasieg)
                ZapiszObraz(z)
            End Using
            Using t = Tiff.Open(plik, "r")
                Assert.Equal(W, t.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(H, t.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
                Dim skala = t.GetField(CType(33550, TiffTag))
                Assert.NotNull(skala)
                Dim punkt = t.GetField(CType(33922, TiffTag))
                Assert.NotNull(punkt)
                Dim klucze = t.GetField(CType(34735, TiffTag))
                Assert.NotNull(klucze)
                Dim k = klucze(1).ToShortArray()
                'ProjectedCSTypeGeoKey = 2180
                Assert.Equal(CShort(3072), k(12))
                Assert.Equal(CShort(2180), k(15))
                Dim tp = punkt(1).ToDoubleArray()
                Assert.Equal(600000.0, tp(3))
                Assert.Equal(500000 + H * 0.5, tp(4))
                Dim sk = skala(1).ToDoubleArray()
                Assert.Equal(0.5, sk(0), 9)
                Assert.Equal(0.5, sk(1), 9)

                If Not kompresjaJpeg Then
                    'Deflate - bezstratnie
                    If t.IsTiled() Then
                        Dim tw = t.GetField(TiffTag.TILEWIDTH)(0).ToInt()
                        Dim th = t.GetField(TiffTag.TILELENGTH)(0).ToInt()
                        Dim kafel(tw * th * 3 - 1) As Byte
                        Dim kxMax = (W + tw - 1) \ tw
                        Dim kyMax = (H + th - 1) \ th
                        For ty = 0 To kyMax - 1
                            For tx = 0 To kxMax - 1
                                Dim res = t.ReadTile(kafel, 0, tx * tw, ty * th, 0, 0)
                                Assert.True(res > 0)
                                Dim maxX = Math.Min(tw, W - tx * tw)
                                Dim maxY = Math.Min(th, H - ty * th)
                                For ly = 0 To maxY - 1
                                    For lx = 0 To maxX - 1
                                        Dim gx = tx * tw + lx
                                        Dim gy = ty * th + ly
                                        For c = 0 To 2
                                            Assert.Equal(Piksel(gx, gy, c), kafel((ly * tw + lx) * 3 + c))
                                        Next
                                    Next
                                Next
                            Next
                        Next
                    Else
                        Dim wiersz(W * 3 - 1) As Byte
                        For y = 0 To H - 1
                            t.ReadScanline(wiersz, y)
                            For i = 0 To W * 3 - 1
                                Assert.Equal(Piksel(i \ 3, y, i Mod 3), wiersz(i))
                            Next
                        Next
                    End If
                End If
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub RozszerzeniaFormatow()
        Assert.Equal("tif", ZapisRastra.Rozszerzenie(FormatArkusza.GeoTiff))
        Assert.Equal("tif", ZapisRastra.Rozszerzenie(FormatArkusza.GeoTiffJpeg))
        Assert.Equal("jpg", ZapisRastra.Rozszerzenie(FormatArkusza.Jpeg))
        Assert.Equal("png", ZapisRastra.Rozszerzenie(FormatArkusza.Png))
    End Sub

    <Fact>
    Public Sub LibTiff_MozliwosciKafelkowania()
        Dim plik = Path.GetTempFileName()
        Try
            Using t = Tiff.Open(plik, "w")
                t.SetField(TiffTag.IMAGEWIDTH, 512)
                t.SetField(TiffTag.IMAGELENGTH, 512)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.TILEWIDTH, 256)
                t.SetField(TiffTag.TILELENGTH, 256)

                Dim bufor(256 * 256 * 3 - 1) As Byte
                ' 4 kafle: (0,0), (256,0), (0,256), (256,256)
                For k = 0 To 3
                    t.WriteEncodedTile(k, bufor, bufor.Length)
                Next
                t.WriteDirectory()

                ' Drugi katalog (IFD 1): piramida (overview 1:2)
                t.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
                t.SetField(TiffTag.IMAGEWIDTH, 256)
                t.SetField(TiffTag.IMAGELENGTH, 256)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.TILEWIDTH, 256)
                t.SetField(TiffTag.TILELENGTH, 256)
                t.WriteEncodedTile(0, bufor, bufor.Length)
                t.WriteDirectory()
            End Using

            Using r = Tiff.Open(plik, "r")
                Assert.True(r.IsTiled())
                Assert.Equal(512, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILEWIDTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILELENGTH)(0).ToInt())

                ' Sprawdzenie przejścia do IFD 1 (piramida)
                Assert.True(r.ReadDirectory())
                Assert.Equal(256, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub LibTiff_WielopoziomowePiramidyWJednejSesji()
        Dim plik = Path.GetTempFileName()
        Try
            ' Zapis IFD 0 (512x512), IFD 1 (256x256), IFD 2 (128x128) w jednej sesji bez zamykania
            Using t = Tiff.Open(plik, "w")
                ' IFD 0 (główny obraz)
                t.SetField(TiffTag.IMAGEWIDTH, 512)
                t.SetField(TiffTag.IMAGELENGTH, 512)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.ROWSPERSTRIP, 16)
                Dim w512(512 * 3 - 1) As Byte
                For y = 0 To 511
                    t.WriteScanline(w512, y)
                Next
                t.WriteDirectory()

                ' IFD 1 (pomniejszenie 1:2)
                t.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
                t.SetField(TiffTag.IMAGEWIDTH, 256)
                t.SetField(TiffTag.IMAGELENGTH, 256)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.ROWSPERSTRIP, 16)
                Dim w256(256 * 3 - 1) As Byte
                For y = 0 To 255
                    t.WriteScanline(w256, y)
                Next
                t.WriteDirectory()

                ' IFD 2 (pomniejszenie 1:4)
                t.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
                t.SetField(TiffTag.IMAGEWIDTH, 128)
                t.SetField(TiffTag.IMAGELENGTH, 128)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.ROWSPERSTRIP, 16)
                Dim w128(128 * 3 - 1) As Byte
                For y = 0 To 127
                    t.WriteScanline(w128, y)
                Next
                t.WriteDirectory()
            End Using

            ' Weryfikacja: 3 katalogi
            Using r = Tiff.Open(plik, "r")
                Assert.Equal(512, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.True(r.ReadDirectory())
                Assert.Equal(256, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))
                Assert.True(r.ReadDirectory())
                Assert.Equal(128, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))
                Assert.False(r.ReadDirectory()) ' brak kolejnych katalogów
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub GeoTiff_ZapisZPiramidami_TworzyKatalogiPomniejszen()
        Dim plik = Path.GetTempFileName()
        Try
            Const W600 As Integer = 600
            Const H600 As Integer = 600
            Dim zasieg As New Zasieg(500000, 600000, 500600, 600600) ' 1 m/px

            Using z As New ZapisGeoTiff(plik, W600, H600, kompresjaJpeg:=False, jakosc:=90,
                                       uklad:=UkladWspolrzednych.PL1992, zasieg:=zasieg,
                                       kanaly:=3, piramidy:=True)
                ' Zapisz obraz 600x600 pasami po 50 wierszy
                Dim bufor(50 * W600 * 3 - 1) As Byte
                For y = 0 To H600 - 1 Step 50
                    z.ZapiszWiersze(bufor, 50)
                Next
                z.Zakoncz()
            End Using

            Using r = Tiff.Open(plik, "r")
                ' IFD 0: pełna rozdzielczość 600x600, kafelkowanie 256x256
                Assert.True(r.IsTiled())
                Assert.Equal(600, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(600, r.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILEWIDTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILELENGTH)(0).ToInt())
                Dim skala0 = r.GetField(CType(33550, TiffTag))(1).ToDoubleArray()
                Assert.Equal(1.0, skala0(0), 4)

                ' IFD 1: piramida 1:2 (300x300)
                Assert.True(r.ReadDirectory())
                Assert.True(r.IsTiled())
                Assert.Equal(300, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(300, r.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILEWIDTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILELENGTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))

                ' IFD 2: piramida 1:4 (150x150)
                Assert.True(r.ReadDirectory())
                Assert.True(r.IsTiled())
                Assert.Equal(150, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(150, r.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILEWIDTH)(0).ToInt())
                Assert.Equal(256, r.GetField(TiffTag.TILELENGTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))

                ' Koniec katalogów (150 <= 256)
                Assert.False(r.ReadDirectory())
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    ''' <summary>Obraz 600x601 z gradientem: piksel poziomu 1 to średnia 2x2 bazy, ostatni nieparzysty wiersz uśredniany sam.</summary>
    <Fact>
    Public Sub GeoTiff_Piramidy_SredniaZBazyPostepIUsunietePlikiTymczasowe()
        Dim folder = Path.Combine(Path.GetTempPath(), "mt_piramidy_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Dim plik = Path.Combine(folder, "arkusz.tif")
        Try
            Const W As Integer = 600, H As Integer = 601
            Dim piksel = Function(x As Integer, y As Integer, c As Integer) CByte((x * 3 + y * 5 + c * 40) Mod 256)
            Dim postepy As New List(Of Integer)
            Using z As New ZapisGeoTiff(plik, W, H, kompresjaJpeg:=False, jakosc:=90,
                                       uklad:=UkladWspolrzednych.PL1992, zasieg:=New Zasieg(500000, 600000, 500601, 600600),
                                       kanaly:=3, piramidy:=True)
                z.PostepPiramid = Sub(p) postepy.Add(p)
                Dim wiersz(W * 3 - 1) As Byte
                For y = 0 To H - 1
                    For x = 0 To W - 1
                        For c = 0 To 2
                            wiersz(x * 3 + c) = piksel(x, y, c)
                        Next
                    Next
                    z.ZapiszWiersze(wiersz, 1)
                Next
                z.Zakoncz()
            End Using

            Assert.Equal(100, postepy.Last())
            Assert.Equal({"arkusz.tif"}, Directory.GetFiles(folder).Select(Function(f) Path.GetFileName(f)).ToArray())

            Using r = Tiff.Open(plik, "r")
                Assert.True(r.ReadDirectory())
                Assert.Equal(300, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(301, r.GetField(TiffTag.IMAGELENGTH)(0).ToInt())
                Dim kafel(256 * 256 * 3 - 1) As Byte
                'kafel (256, 256) poziomu 1 zawiera wiersz 300 - pochodzi tylko z ostatniego wiersza bazy (600)
                Assert.True(r.ReadTile(kafel, 0, 256, 256, 0, 0) >= 0)
                For Each pr In {(dx:=0, dy:=0), (dx:=10, dy:=20), (dx:=43, dy:=44)}
                    Dim x = 256 + pr.dx, y = 256 + pr.dy
                    Dim ileWierszy = If(2 * y + 1 < H, 2, 1)
                    For c = 0 To 2
                        Dim suma = 0
                        For oy = 0 To ileWierszy - 1
                            suma += CInt(piksel(2 * x, 2 * y + oy, c)) + CInt(piksel(2 * x + 1, 2 * y + oy, c))
                        Next
                        Dim n = 2 * ileWierszy
                        Assert.Equal((suma + n \ 2) \ n, CInt(kafel((pr.dy * 256 + pr.dx) * 3 + c)))
                    Next
                Next
            End Using
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub GeoTiff_Piramidy_PrzerwanieWZakonczUsuwaPlikiTymczasowe()
        Dim folder = Path.Combine(Path.GetTempPath(), "mt_piramidy_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Dim plik = Path.Combine(folder, "arkusz.tif")
        Try
            Using zrodlo As New Threading.CancellationTokenSource()
                zrodlo.Cancel()
                Using z As New ZapisGeoTiff(plik, 600, 600, kompresjaJpeg:=True, jakosc:=80,
                                           uklad:=UkladWspolrzednych.PL1992, zasieg:=New Zasieg(500000, 600000, 500600, 600600),
                                           kanaly:=3, piramidy:=True)
                    Assert.True(z.MaPiramidy)
                    z.Przerwanie = zrodlo.Token
                    Dim bufor(100 * 600 * 3 - 1) As Byte
                    For y = 0 To 599 Step 100
                        z.ZapiszWiersze(bufor, 100)
                    Next
                    Assert.Throws(Of OperationCanceledException)(Sub() z.Zakoncz())
                End Using
            End Using
            Assert.Empty(Directory.GetFiles(folder, "*.tmp"))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub GeoTiff_ZapisBezPiramid_TylkoJedenKatalog()
        Dim plik = Path.GetTempFileName()
        Try
            Const W600 As Integer = 600
            Const H600 As Integer = 600
            Dim zasieg As New Zasieg(500000, 600000, 500600, 600600)

            Using z As New ZapisGeoTiff(plik, W600, H600, kompresjaJpeg:=False, jakosc:=90,
                                       uklad:=UkladWspolrzednych.PL1992, zasieg:=zasieg,
                                       kanaly:=3, piramidy:=False)
                Dim bufor(50 * W600 * 3 - 1) As Byte
                For y = 0 To H600 - 1 Step 50
                    z.ZapiszWiersze(bufor, 50)
                Next
                z.Zakoncz()
            End Using

            Using r = Tiff.Open(plik, "r")
                Assert.Equal(600, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.False(r.ReadDirectory()) ' brak katalogów piramid
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub LibTiff_AppendClientOpen_TiledOverviews()
        Dim plik = Path.GetTempFileName()
        Try
            Using t = Tiff.Open(plik, "w")
                t.SetField(TiffTag.IMAGEWIDTH, 512)
                t.SetField(TiffTag.IMAGELENGTH, 512)
                t.SetField(TiffTag.BITSPERSAMPLE, 8)
                t.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                t.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                t.SetField(TiffTag.TILEWIDTH, 256)
                t.SetField(TiffTag.TILELENGTH, 256)
                Dim b(256 * 256 * 3 - 1) As Byte
                For k = 0 To 3
                    t.WriteEncodedTile(k, b, b.Length)
                Next
                t.WriteDirectory()
            End Using

            Using fs As New FileStream(plik, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
                Using writer As Tiff = Tiff.ClientOpen("Writer", "a", fs, New TiffStream())
                    Assert.NotNull(writer)

                    ' IFD 1 (256x256) czytany z IFD 0
                    writer.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
                    writer.SetField(TiffTag.IMAGEWIDTH, 256)
                    writer.SetField(TiffTag.IMAGELENGTH, 256)
                    writer.SetField(TiffTag.BITSPERSAMPLE, 8)
                    writer.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                    writer.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                    writer.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                    writer.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                    writer.SetField(TiffTag.TILEWIDTH, 256)
                    writer.SetField(TiffTag.TILELENGTH, 256)

                    Using fsRead As New FileStream(plik, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As Tiff = Tiff.ClientOpen("Reader", "r", fsRead, New TiffStream())
                            Assert.True(reader.SetDirectory(0))
                            Dim readBuf(256 * 256 * 3 - 1) As Byte
                            Dim readRes = reader.ReadTile(readBuf, 0, 0, 0, 0, 0)
                            Assert.True(readRes > 0)
                        End Using
                    End Using

                    Dim b(256 * 256 * 3 - 1) As Byte
                    writer.WriteEncodedTile(0, b, b.Length)
                    writer.WriteDirectory()
                    writer.Flush()
                    fs.Flush()

                    ' IFD 2 (128x128) czytany z IFD 1
                    writer.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
                    writer.SetField(TiffTag.IMAGEWIDTH, 128)
                    writer.SetField(TiffTag.IMAGELENGTH, 128)
                    writer.SetField(TiffTag.BITSPERSAMPLE, 8)
                    writer.SetField(TiffTag.SAMPLESPERPIXEL, 3)
                    writer.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
                    writer.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
                    writer.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
                    writer.SetField(TiffTag.TILEWIDTH, 256)
                    writer.SetField(TiffTag.TILELENGTH, 256)

                    Using fsRead As New FileStream(plik, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As Tiff = Tiff.ClientOpen("Reader", "r", fsRead, New TiffStream())
                            Assert.True(reader.SetDirectory(1))
                            Dim readBuf(256 * 256 * 3 - 1) As Byte
                            Dim readRes = reader.ReadTile(readBuf, 0, 0, 0, 0, 0)
                            Assert.True(readRes > 0)
                        End Using
                    End Using

                    writer.WriteEncodedTile(0, b, b.Length)
                    writer.WriteDirectory()
                    writer.Flush()
                    fs.Flush()
                End Using
            End Using

            Using r = Tiff.Open(plik, "r")
                Assert.Equal(512, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.True(r.ReadDirectory())
                Assert.Equal(256, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))
                Assert.True(r.ReadDirectory())
                Assert.Equal(128, r.GetField(TiffTag.IMAGEWIDTH)(0).ToInt())
                Assert.Equal(FileType.REDUCEDIMAGE, CType(r.GetField(TiffTag.SUBFILETYPE)(0).ToInt(), FileType))
                Assert.False(r.ReadDirectory())
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

End Class
