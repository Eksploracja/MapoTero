'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
Imports MapoTero.Core
Imports Xunit

Public Class ObszarKreatoraTesty

    <Fact>
    Public Sub Granice_ObejmujaProstokatZZapasem()
        ' Zakopane - prostokąt wyszukiwarki wydłużony
        Dim z = ObszarKreatora.ZasiegZGranic(49.25, 49.33, 19.88, 20.05)
        Dim u = UkladWspolrzednych.PL1992
        For Each p In {u.ZWgs84(49.25, 19.88), u.ZWgs84(49.33, 20.05), u.ZWgs84(49.25, 20.05), u.ZWgs84(49.33, 19.88)}
            Assert.InRange(p.X, z.XDol, z.XGora)
            Assert.InRange(p.Y, z.YLewy, z.YPrawy)
        Next
        ' prostokąt, nie kwadrat: ok. 12 km na 9 km
        Assert.InRange(z.Szerokosc / z.Wysokosc, 1.2, 1.5)
    End Sub

    <Fact>
    Public Sub Granice_AdresDostajeCoNajmniejKilometr()
        Dim z = ObszarKreatora.ZasiegZGranic(50.06, 50.0601, 19.93, 19.9302)
        Assert.True(z.Szerokosc >= ObszarKreatora.MinBokGranic - 0.001)
        Assert.True(z.Wysokosc >= ObszarKreatora.MinBokGranic - 0.001)
        Assert.InRange(z.Szerokosc, 999.0, 1001.0)
    End Sub

    <Fact>
    Public Sub Dzialka_PasWokolZasiegu()
        Dim z = ObszarKreatora.ZasiegDzialki(New Zasieg(500000, 600000, 500040, 600030))
        Assert.Equal(330.0, z.Szerokosc, 6)
        Assert.Equal(340.0, z.Wysokosc, 6)
    End Sub

    <Theory>
    <InlineData("121901_1.0001.123/4", "121901_1.0001.123/4")>
    <InlineData("  022602_1.0003.59 ", "022602_1.0003.59")>
    <InlineData("działka Wieliczka 123/4", "Wieliczka 123/4")>
    <InlineData("Dz. Krzewina 134", "Krzewina 134")>
    <InlineData("dzialka Nowa Wieś 12", "Nowa Wieś 12")>
    Public Sub Dzialka_RozpoznajeZapytanie(tekst As String, oczekiwane As String)
        Assert.Equal(oczekiwane, ObszarKreatora.ZapytanieDzialki(tekst))
    End Sub

    <Theory>
    <InlineData("Długa 12/4, Kraków")>
    <InlineData("Wieliczka")>
    <InlineData("50.06, 19.93")>
    <InlineData("działka")>
    <InlineData("")>
    Public Sub Dzialka_ZwykleZapytanieToNieDzialka(tekst As String)
        Assert.Null(ObszarKreatora.ZapytanieDzialki(tekst))
    End Sub
End Class
