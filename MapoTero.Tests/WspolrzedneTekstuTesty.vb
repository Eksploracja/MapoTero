'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
Imports MapoTero.Core
Imports Xunit

Public Class WspolrzedneTekstuTesty

    <Theory>
    <InlineData("50.0614, 19.9366", 50.0614, 19.9366)>
    <InlineData("50,0614 19,9366", 50.0614, 19.9366)>
    <InlineData("50,0614, 19,9366", 50.0614, 19.9366)>
    <InlineData("50.0614,19.9366", 50.0614, 19.9366)>
    <InlineData("N 50.0614 E 19.9366", 50.0614, 19.9366)>
    <InlineData("50.0614N 19.9366E", 50.0614, 19.9366)>
    <InlineData("E 19.9366 N 50.0614", 50.0614, 19.9366)>
    <InlineData("19.9366, 50.0614", 50.0614, 19.9366)>
    <InlineData("50°03'41.0""N 19°56'11.8""E", 50.061389, 19.936611)>
    <InlineData("50° 3.684' N, 19° 56.196' E", 50.0614, 19.9366)>
    Public Sub Wgs84_RozpoznajeZapisyStopni(tekst As String, szer As Double, dl As Double)
        Dim p = WspolrzedneTekstu.Rozpoznaj(tekst)
        Assert.NotNull(p)
        Assert.Equal("WGS84", p.Uklad)
        Assert.Equal(szer, p.Punkt.Szerokosc, 4)
        Assert.Equal(dl, p.Punkt.Dlugosc, 4)
    End Sub

    <Fact>
    Public Sub Pl1992_XPolnocnaPierwsza()
        Dim xy = UkladWspolrzednych.PL1992.ZWgs84(50.0614, 19.9366)
        Dim p = WspolrzedneTekstu.Rozpoznaj($"{xy.X.ToString("0.00", Globalization.CultureInfo.InvariantCulture)} {xy.Y.ToString("0.00", Globalization.CultureInfo.InvariantCulture)}")
        Assert.Equal("PL-1992", p.Uklad)
        Assert.Equal(50.0614, p.Punkt.Szerokosc, 4)
        Assert.Equal(19.9366, p.Punkt.Dlugosc, 4)
    End Sub

    <Theory>
    <InlineData(False)>
    <InlineData(True)>
    Public Sub Pl2000_StrefaZWspolrzednejY(odwrotnie As Boolean)
        Dim xy = UkladWspolrzednych.PL2000Strefa7.ZWgs84(50.0614, 19.9366)
        Dim k = Globalization.CultureInfo.InvariantCulture
        Dim tekst = If(odwrotnie, $"{xy.Y.ToString("0.00", k)}; {xy.X.ToString("0.00", k)}", $"X={xy.X.ToString("0.00", k)} Y={xy.Y.ToString("0.00", k)}")
        Dim p = WspolrzedneTekstu.Rozpoznaj(tekst)
        Assert.Equal("PL-2000 strefa 7", p.Uklad)
        Assert.Equal(50.0614, p.Punkt.Szerokosc, 4)
        Assert.Equal(19.9366, p.Punkt.Dlugosc, 4)
    End Sub

    <Theory>
    <InlineData("Kraków")>
    <InlineData("ul. 3 Maja 5, Sopot")>
    <InlineData("Nowy Sącz 33-300")>
    <InlineData("12 34")>
    <InlineData("50.06")>
    <InlineData("1 2 3")>
    <InlineData("")>
    Public Sub NieWspolrzedne_ZwracaNothing(tekst As String)
        Assert.Null(WspolrzedneTekstu.Rozpoznaj(tekst))
    End Sub
End Class
