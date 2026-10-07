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
                    Dim wiersz(W * 3 - 1) As Byte
                    For y = 0 To H - 1
                        t.ReadScanline(wiersz, y)
                        For i = 0 To W * 3 - 1
                            Assert.Equal(Piksel(i \ 3, y, i Mod 3), wiersz(i))
                        Next
                    Next
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

End Class
