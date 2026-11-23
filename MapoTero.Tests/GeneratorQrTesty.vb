' =====================================================================
' Testy jednostkowe generatora kodów QR (ISO/IEC 18004, wersje 1–10, M)
' =====================================================================

Imports System.Drawing
Imports System.Drawing.Imaging
Imports MapoTero
Imports Xunit

Public Class GeneratorQrTesty

    <Fact>
    Public Sub Utworz_KrotkiTekst_DobieraWersje1()
        Dim qr = GeneratorQr.Utworz("A")
        Assert.Equal(1, qr.Wersja)
        Assert.Equal(21, qr.Rozmiar)
    End Sub

    <Fact>
    Public Sub Utworz_AdresUrlTransferu_DobieraPrawidlowaWersje()
        ' Typowy adres URL generowany przez serwer transferu: ~65 znaków
        Dim url = "http://192.168.1.100:54321/0123456789abcdef0123456789abcdef"
        Dim qr = GeneratorQr.Utworz(url)

        ' Dla 62 znaków UTF-8: V4 mieści do 62 B, V5 do 84 B
        Assert.True(qr.Wersja >= 4 AndAlso qr.Wersja <= 5)
        Assert.Equal(17 + (4 * qr.Wersja), qr.Rozmiar)
    End Sub

    <Fact>
    Public Sub Utworz_WzorceNarozne_SaPrawidlowoUlozone()
        Dim qr = GeneratorQr.Utworz("MapoTero")
        Dim n = qr.Rozmiar

        ' Znacznik narożny lewy górny (środek 3,3)
        Assert.True(qr.CzyCiemny(3, 3))
        Assert.True(qr.CzyCiemny(0, 0))
        Assert.False(qr.CzyCiemny(1, 1))
        Assert.True(qr.CzyCiemny(2, 2))

        ' Znacznik prawy górny (środek n - 4, 3)
        Assert.True(qr.CzyCiemny(n - 4, 3))
        Assert.True(qr.CzyCiemny(n - 1, 0))

        ' Znacznik lewy dolny (środek 3, n - 4)
        Assert.True(qr.CzyCiemny(3, n - 4))
        Assert.True(qr.CzyCiemny(0, n - 1))

        ' Stały ciemny moduł (dark module) na (8, 4 * V + 9)
        Assert.True(qr.CzyCiemny(8, (4 * qr.Wersja) + 9))
    End Sub

    <Fact>
    Public Sub Utworz_TekstZbytDlugi_RzucaArgumentException()
        ' Wersja 10 mieści maksymalnie 213 bajtów dla korekcji M
        Dim dlugiTekst = New String("X"c, 250)
        Assert.Throws(Of ArgumentException)(Sub() GeneratorQr.Utworz(dlugiTekst))
    End Sub

    <Fact>
    Public Sub GenerujBitmape_ZwracaOstraBitmapeOIdealnymRozmiarze()
        Dim qr = GeneratorQr.Utworz("Test")
        Dim modulPx = 5
        Dim margines = 3
        Dim oczekiwanyBok = (qr.Rozmiar + (2 * margines)) * modulPx

        Using bmp = qr.RenderujBitmape(modulPx, margines)
            Assert.Equal(oczekiwanyBok, bmp.Width)
            Assert.Equal(oczekiwanyBok, bmp.Height)
            Assert.Equal(PixelFormat.Format24bppRgb, bmp.PixelFormat)

            ' Margines powinien być biały
            Dim pikselMarginesu = bmp.GetPixel(0, 0)
            Assert.Equal(255, pikselMarginesu.R)
            Assert.Equal(255, pikselMarginesu.G)
            Assert.Equal(255, pikselMarginesu.B)

            ' Środek lewego górnego znacznika (3, 3) powinien być czarny
            Dim xCentrum = (margines + 3) * modulPx + (modulPx \ 2)
            Dim yCentrum = (margines + 3) * modulPx + (modulPx \ 2)
            Dim pikselZnacznika = bmp.GetPixel(xCentrum, yCentrum)
            Assert.Equal(0, pikselZnacznika.R)
            Assert.Equal(0, pikselZnacznika.G)
            Assert.Equal(0, pikselZnacznika.B)
        End Using
    End Sub

    <Fact>
    Public Sub GenerujBitmapeDopasowana_DopasowujeRozmiarDoDocelowegoBoku()
        Dim docelowyBok = 250
        Using bmp = GeneratorQr.GenerujBitmapeDopasowana("http://192.168.1.1:1234/token", docelowyBok, marginesModuly:=4)
            Assert.True(bmp.Width <= docelowyBok)
            Assert.True(bmp.Height <= docelowyBok)
            Assert.Equal(bmp.Width, bmp.Height)
        End Using
    End Sub

End Class
