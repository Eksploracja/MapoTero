'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class ZestawWarstwTesty

    Private Shared Function Zestaw(ParamArray warstwy() As (String, Integer)) As ZestawWarstw
        Dim z As New ZestawWarstw()
        For Each w In warstwy
            z.Dodaj(w.Item1, w.Item2)
        Next
        Return z
    End Function

    <Fact>
    Public Sub DodawanieBezPowtorzenILimit12()
        Dim z As New ZestawWarstw()
        Assert.Equal(WynikDodania.Dodano, z.Dodaj("orto"))
        Assert.Equal(WynikDodania.JuzWybrana, z.Dodaj("orto"))
        For i = 2 To 12
            Assert.Equal(WynikDodania.Dodano, z.Dodaj("w" & i))
        Next
        Assert.Equal(WynikDodania.BrakMiejsca, z.Dodaj("w13"))
        Assert.Equal(12, z.Liczba)
    End Sub

    <Fact>
    Public Sub UsuwaniePojedynczejWarstwy()
        Dim z = Zestaw(("a", 100), ("b", 100), ("c", 100))
        z.Usun(1)
        Assert.Equal({"a", "c"}, z.Nazwy())
        z.Usun(5)
        Assert.Equal(2, z.Liczba)
    End Sub

    <Fact>
    Public Sub PrzenoszenieWarstwy()
        Dim z = Zestaw(("a", 100), ("b", 60), ("c", 100))
        Assert.Equal(2, z.Przenies(0, 2))
        Assert.Equal({"b", "c", "a"}, z.Nazwy())
        'poza zakresem - na skraj listy; krycie wędruje z warstwą
        Assert.Equal(0, z.Przenies(2, -3))
        Assert.Equal({"a", "b", "c"}, z.Nazwy())
        Assert.Equal({100, 60, 100}, z.Krycia())
    End Sub

    <Fact>
    Public Sub KrycieOgraniczoneDoZakresu()
        Dim z = Zestaw(("a", 100))
        z.UstawKrycie(0, 0)
        Assert.Equal(ZestawWarstw.MinKrycie, z(0).Krycie)
        z.UstawKrycie(0, 250)
        Assert.Equal(100, z(0).Krycie)
    End Sub

    <Fact>
    Public Sub GrupyZapytanPelneKrycieJednoZapytanie()
        Dim g = Zestaw(("a", 100), ("b", 100), ("c", 100)).GrupyZapytan()
        Assert.Single(g)
        Assert.Equal({"a", "b", "c"}, g(0).Warstwy)
    End Sub

    <Fact>
    Public Sub GrupyZapytanWarstwaPrzezroczystaOsobno()
        Dim g = Zestaw(("orto", 100), ("budynki", 100), ("dzialki", 50), ("ulice", 100), ("adresy", 100)).GrupyZapytan()
        Assert.Equal(3, g.Count)
        Assert.Equal({"orto", "budynki"}, g(0).Warstwy)
        Assert.Equal({"dzialki"}, g(1).Warstwy)
        Assert.Equal(50, g(1).Krycie)
        Assert.Equal({"ulice", "adresy"}, g(2).Warstwy)
        Assert.Equal(100, g(2).Krycie)
    End Sub

    <Fact>
    Public Sub GrupyZapytanSasiedniePrzezroczysteNieLaczone()
        Dim g = Zestaw(("a", 60), ("b", 60)).GrupyZapytan()
        Assert.Equal(2, g.Count)
    End Sub

    <Fact>
    Public Sub TekstKryciaZgodnyZeStarszymiPlikami()
        Assert.Equal("", Zestaw(("a", 100), ("b", 100)).TekstKrycia())
        Assert.Equal("100;60", Zestaw(("a", 100), ("b", 60)).TekstKrycia())
        Assert.Equal({100, 60, 100}, ZestawWarstw.KryciaZTekstu("100;60", 3))
        Assert.Equal({100, 100}, ZestawWarstw.KryciaZTekstu("", 2))
        Assert.Equal({100, 5}, ZestawWarstw.KryciaZTekstu("x;1", 2))
    End Sub

    <Fact>
    Public Sub KrycieWConfTxt()
        Dim c As New KonfiguracjaSesji With {.LiczbaWarstw = 2, .KrycieWarstw = "100;40", .AdresSerwera = "https://serwer/wms"}
        c.Warstwy(0) = "orto"
        c.Warstwy(1) = "dzialki"
        Dim sciezka = IO.Path.GetTempFileName()
        Dim d As KonfiguracjaSesji
        Try
            c.Zapisz(sciezka, Text.Encoding.UTF8)
            d = KonfiguracjaSesji.Wczytaj(sciezka, Text.Encoding.UTF8)
        Finally
            IO.File.Delete(sciezka)
        End Try
        Assert.Equal("https://serwer/wms", d.AdresSerwera)
        Assert.Equal({100, 40}, d.KryciaWarstw())
        Dim z = d.WybranyZestaw()
        Assert.Equal({"orto", "dzialki"}, z.Nazwy())
        Assert.Equal(40, z(1).Krycie)
    End Sub

    Private Const Topo As String = "https://topo/wms?layers="
    Private Const Isok As String = "https://isok/wms?layers="
    Private Const Orto As String = "https://orto/wmts?SERVICE=WMTS&REQUEST=GetCapabilities"

    <Fact>
    Public Sub TaSamaNazwaZInnegoSerweraToInnaWarstwa()
        Dim z As New ZestawWarstw()
        Assert.Equal(WynikDodania.Dodano, z.Dodaj("Raster", adres:=Topo, zbior:="topo"))
        Assert.Equal(WynikDodania.Dodano, z.Dodaj("Raster", adres:=Isok, zbior:="isok"))
        Assert.Equal(WynikDodania.JuzWybrana, z.Dodaj("Raster", adres:=Topo))
        Assert.True(z.WieleZrodel)
    End Sub

    <Fact>
    Public Sub GrupyZapytanDzielonePoSerwerach()
        Dim z As New ZestawWarstw()
        z.Dodaj("Raster_25", adres:=Topo)
        z.Dodaj("Raster_10", adres:=Topo)
        z.Dodaj("cieniowanie", 40, Isok)
        z.Dodaj("ORTO", adres:=Orto)
        z.Dodaj("napisy", adres:=Topo)
        Dim g = z.GrupyZapytan()
        Assert.Equal(4, g.Count)
        Assert.Equal({"Raster_25", "Raster_10"}, g(0).Warstwy)
        Assert.Equal(Topo, g(0).Adres)
        Assert.Equal(Isok, g(1).Adres)
        Assert.Equal(40, g(1).Krycie)
        Assert.True(g(2).CzyWmts)
        Assert.False(g(0).CzyWmts)
        Assert.Equal(Topo, g(3).Adres)
    End Sub

    <Fact>
    Public Sub ZrodlaWarstwWConfTxt()
        Dim z As New ZestawWarstw()
        z.Dodaj("Raster_25", adres:=Topo, zbior:="skany")
        z.Dodaj("cieniowanie", 40, Isok, "isok")
        Dim c As New KonfiguracjaSesji With {.LiczbaWarstw = 2, .KrycieWarstw = z.TekstKrycia(), .AdresSerwera = Topo, .RodzajMapy = "skany", .XDol = "0", .YLewy = "0", .XGora = "1000", .YPrawy = "1000", .BokSegmentuPx = "500", .RozmiarPiksela = "1", .Format = "jpeg",
            .AdresyWarstw = ZestawWarstw.Polacz(z.Adresy()), .ZbioryWarstw = ZestawWarstw.Polacz(z.Zbiory())}
        c.Warstwy(0) = "Raster_25"
        c.Warstwy(1) = "cieniowanie"
        Dim sciezka = IO.Path.GetTempFileName()
        Dim d As KonfiguracjaSesji
        Try
            c.Zapisz(sciezka, Text.Encoding.UTF8)
            d = KonfiguracjaSesji.Wczytaj(sciezka, Text.Encoding.UTF8)
        Finally
            IO.File.Delete(sciezka)
        End Try
        Dim w = d.WybranyZestaw()
        Assert.Equal(Isok, w(1).Adres)
        Assert.Equal("isok", w(1).Zbior)
        Assert.Equal(40, w(1).Krycie)
        Assert.True(c.TaSamaSiatka(d))
        'ta sama nazwa warstwy z innego serwera - inna sesja
        d.AdresyWarstw = ZestawWarstw.Polacz({Topo, Topo})
        Assert.False(c.TaSamaSiatka(d))
    End Sub

    <Fact>
    Public Sub ZrodlaZPlikuStarszejWersji()
        'brak pól adresów i zbiorów - wszystkie warstwy z serwera i zbioru sesji
        Dim c As New KonfiguracjaSesji With {.LiczbaWarstw = 2, .AdresSerwera = Topo, .RodzajMapy = "skany"}
        c.Warstwy(0) = "a"
        c.Warstwy(1) = "b"
        Dim w = c.WybranyZestaw()
        Assert.Equal({Topo, Topo}, w.Adresy())
        Assert.Equal({"skany", "skany"}, w.Zbiory())
    End Sub

    <Fact>
    Public Sub NajdokladniejszaWarstwaWyznaczaPiksel()
        Dim z As New ZestawWarstw()
        Assert.Null(z.NajdokladniejszaWarstwa())
        z.Dodaj("Raster_25_1965", adres:=Topo, zalecanyPiksel:=2)
        z.Dodaj("Administracyjna", adres:=Isok, zalecanyPiksel:=16)
        z.Dodaj("bez_zalecenia", adres:=Isok)
        'kolejność dodawania nie ma znaczenia - najmniejszy zalecany piksel
        Assert.Equal("Raster_25_1965", z.NajdokladniejszaWarstwa().Nazwa)
        Assert.Equal("Administracyjna", z.NajmniejDokladnaWarstwa().Nazwa)
        z.Dodaj("ORTOFOTOMAPA", adres:=Orto, zalecanyPiksel:=0.25)
        Assert.Equal(0.25, z.NajdokladniejszaWarstwa().ZalecanyPiksel)
        z.Usun(3)
        Assert.Equal(2.0, z.NajdokladniejszaWarstwa().ZalecanyPiksel)
        Assert.Equal(2.0, z.Kopia().NajdokladniejszaWarstwa().ZalecanyPiksel)
    End Sub

    <Fact>
    Public Sub NakladanieZKryciem()
        Dim bialy As Integer = &HFFFFFFFF
        Dim czarny As Integer = &HFF000000
        Dim przezroczysty As Integer = 0
        Dim cel() As Integer = {bialy, bialy, przezroczysty}
        NakladanieWarstw.Naloz(cel, {czarny, przezroczysty, czarny}, 50)
        'połowa krycia: szary na białym, przezroczysty piksel warstwy nic nie zmienia, na pustym tle - półprzezroczysty czarny
        Assert.Equal(&HFF808080, cel(0))
        Assert.Equal(bialy, cel(1))
        Assert.Equal(128, (cel(2) >> 24) And &HFF)
        Assert.Equal(0, cel(2) And &HFFFFFF)
    End Sub

    <Fact>
    Public Sub NakladaniePelneKrycieZastepujePiksel()
        Dim cel() As Integer = {&HFFFFFFFF}
        NakladanieWarstw.Naloz(cel, {&HFF336699}, 100)
        Assert.Equal(&HFF336699, cel(0))
    End Sub

End Class
