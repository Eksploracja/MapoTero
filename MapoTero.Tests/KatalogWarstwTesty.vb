'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports MapoTero.Core
Imports Xunit

Public Class KatalogWarstwTesty

    Private Const Przyklad As String =
        "# komentarz" & vbCrLf &
        "> Ortofotomapy" & vbCrLf &
        "ortofotomapa | Raster | Ortofotomapa (WMS)" & vbCrLf &
        "" & vbCrLf &
        "> Mapy historyczne" & vbCrLf &
        ">> Plany miast" & vbCrLf &
        "plany | warszawa_2180 | Warszawa" & vbCrLf &
        "plany | krakow_2180" & vbCrLf &
        ">> WIG" & vbCrLf &
        "ihpan | wig25k_2180 | WIG 1:25 000 | Hist_WIG_25k" & vbCrLf &
        "> Rzeźba terenu" & vbCrLf &
        "ortofotomapa | Raster | Ta sama warstwa w drugiej grupie"

    <Fact>
    Public Sub OdczytGrupPodgrupIWarstw()
        Dim k = KatalogWarstw.ZTekstu(Przyklad)
        Assert.Empty(k.Bledy)
        Assert.Equal({"Ortofotomapy", "Mapy historyczne", "Rzeźba terenu"}, k.Grupy.Select(Function(g) g.Nazwa))
        Dim historyczne = k.Grupy(1)
        Assert.Equal({"Plany miast", "WIG"}, historyczne.Dzieci.Select(Function(g) g.Nazwa))
        Dim plany = historyczne.Dzieci(0)
        Assert.Equal("Warszawa", plany.Dzieci(0).Nazwa)
        Assert.Equal("warszawa_2180", plany.Dzieci(0).Warstwa)
        Assert.Equal("plany", plany.Dzieci(0).Zbior)
        Assert.Equal("", plany.Dzieci(0).Identyfikator)
        'bez nazwy wyświetlanej - nazwa warstwy na serwerze
        Assert.Equal("krakow_2180", plany.Dzieci(1).Nazwa)
        Assert.Equal("Hist_WIG_25k", historyczne.Dzieci(1).Dzieci(0).Identyfikator)
        Assert.Equal("Hist_WIG_25k", k.ZnajdzIdentyfikator("ihpan", "wig25k_2180"))
        Assert.Equal(5, k.Warstwy().Count())
        Assert.True(k.Zawiera("ORTOFOTOMAPA", "Raster"))
        Assert.False(k.Zawiera("ortofotomapa", "raster"))
    End Sub

    <Fact>
    Public Sub BledneWierszeSaPomijaneZOpisem()
        Dim k = KatalogWarstw.ZTekstu("zbior | w | przed grupą" & vbLf & ">> bez nadrzędnej" & vbLf & "> A" & vbLf & "tylko_zbior" & vbLf & ">" & vbLf & "z | w")
        Assert.Equal(4, k.Bledy.Count)
        Assert.Contains(k.Bledy, Function(b) b.StartsWith("wiersz 1:"))
        Assert.Single(k.Grupy)
        Assert.Single(k.Grupy(0).Dzieci)
    End Sub

    <Fact>
    Public Sub NiewymienioneWarstwyZbioru()
        Dim k = KatalogWarstw.ZTekstu(Przyklad)
        Dim plik = PlikWarstw.ZTekstu("http://s/wms?layers=" & vbLf & "warszawa_2180" & vbLf & "2" & vbLf & "lodz_2180" & vbLf & "2")
        Assert.Equal({"lodz_2180"}, k.NiewymienioneWarstwy("plany", plik).Select(Function(w) w.Nazwa))
        Assert.True(k.ZawieraZbior("Plany"))
        Assert.False(k.ZawieraZbior("dodany_kreatorem"))
    End Sub

    <Fact>
    Public Sub NazwyWyswietlanePoAdresieSerwera()
        Dim k = KatalogWarstw.ZTekstu(Przyklad)
        Dim zbiory As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase) From {
            {"ihpan", PlikWarstw.ZTekstu("""https://Pastmaps/wms?layers=""" & vbLf & "wig25k_2180" & vbLf & "2")},
            {"ortofotomapa", PlikWarstw.ZTekstu("https://orto/wms?layers=" & vbLf & "Raster" & vbLf & "1")}}
        Dim mapa = k.MapaNazw(zbiory)
        'warstwa sesji zapisanej z innym zbiorem tego samego serwera (np. dawny Hgis_25_WIG_topograficzna)
        Dim n = mapa(KatalogWarstw.KluczAliasu("https://pastmaps/wms?layers=", "wig25k_2180"))
        Assert.Equal("WIG 1:25 000", n.Nazwa)
        Assert.Equal("WIG", n.Grupa)
        'pierwsze wystąpienie w katalogu
        Assert.Equal("Ortofotomapa (WMS)", mapa(KatalogWarstw.KluczAliasu("https://orto/wms?layers=", "Raster")).Nazwa)
        'zbioru "plany" nie ma w słowniku - pominięty
        Assert.Equal(2, mapa.Count)
    End Sub

    ''' <summary>
    ''' Katalog z repozytorium zgadza się z plikami zbiorów map: każdy odnośnik wskazuje istniejący zbiór i warstwę,
    ''' każda warstwa zbiorów z warstwy.txt jest w katalogu, nazwy wyświetlane się nie powtarzają w grupie, a programowi
    ''' nie zostały warstwy-skorowidze (siatki arkuszy, zasięgi danych) - autor chce pobierać tylko treść map.
    ''' </summary>
    <Fact>
    Public Sub KatalogProgramuZgadzaSieZeZbioramiMap()
        Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)
        Dim folder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "warstwy")
        Dim k = KatalogWarstw.Wczytaj(Path.Combine(folder, KatalogWarstw.NazwaPliku))
        Assert.Empty(k.Bledy)

        Dim zbiory As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        For Each nazwa In File.ReadAllLines(Path.Combine(folder, "warstwy.txt")).Select(Function(l) l.Trim().Trim(""""c)).Where(Function(l) l <> "")
            zbiory(nazwa) = PlikWarstw.Wczytaj(Path.Combine(folder, nazwa & ".txt"))
        Next

        For Each w In k.Warstwy()
            Assert.True(zbiory.ContainsKey(w.Zbior), "katalog: brak zbioru " & w.Zbior & " w warstwy.txt")
            Assert.True(zbiory(w.Zbior).Znajdz(w.Warstwa) IsNot Nothing, "katalog: brak warstwy " & w.Warstwa & " w zbiorze " & w.Zbior)
            Assert.False(String.IsNullOrWhiteSpace(w.Identyfikator), "brak identyfikatora dla warstwy: " & w.Zbior & " | " & w.Warstwa)
            Assert.True(w.Identyfikator.Length <= 20, "identyfikator dłuższy niż 20 znaków: " & w.Identyfikator)
            Assert.Matches("^[a-zA-Z0-9_\-+]+$", w.Identyfikator)
        Next
        For Each z In zbiory
            Assert.True(k.NiewymienioneWarstwy(z.Key, z.Value).Count = 0,
                        "warstwy zbioru " & z.Key & " spoza katalogu: " & String.Join(", ", k.NiewymienioneWarstwy(z.Key, z.Value).Select(Function(x) x.Nazwa)))
            For Each w In z.Value.Warstwy
                Assert.False(FiltrWarstw.Uprosc(w.Nazwa).Contains("skorowid"), "skorowidz w zbiorze " & z.Key & ": " & w.Nazwa)
            Next
        Next

        'ta sama warstwa serwera (adres + nazwa) tylko w jednym zbiorze map - dawniej dublowały ją zbiory Hgis_*
        Dim klucze = zbiory.SelectMany(Function(z) z.Value.Warstwy.Select(Function(w) (Klucz:=KatalogWarstw.KluczAliasu(z.Value.Adres, w.Nazwa), Zbior:=z.Key)))
        For Each grupa In klucze.GroupBy(Function(x) x.Klucz)
            Assert.True(grupa.Count() = 1, "warstwa w kilku zbiorach: " & String.Join(", ", grupa.Select(Function(x) x.Zbior)) & " - " & grupa.Key)
        Next

        'unikalność identyfikatorów (każdy identyfikator reprezentuje dokładnie jedną warstwę)
        For Each grp In k.Warstwy().GroupBy(Function(w) w.Identyfikator)
            Dim unikalneWarstwy = grp.Select(Function(w) zbiory(w.Zbior).Adres & "|" & w.Warstwa).Distinct().ToList()
            Assert.True(unikalneWarstwy.Count = 1, "identyfikator " & grp.Key & " przypisany do różnych warstw: " & String.Join(", ", unikalneWarstwy))
        Next

        SprawdzNazwyWGrupach(k.Grupy)
    End Sub

    Private Shared Sub SprawdzNazwyWGrupach(wezly As IEnumerable(Of WezelKatalogu))
        For Each powtorzona In wezly.GroupBy(Function(w) w.Nazwa).Where(Function(g) g.Count() > 1)
            Assert.Fail("powtórzona nazwa w jednej grupie: " & powtorzona.Key)
        Next
        For Each w In wezly.Where(Function(x) Not x.CzyWarstwa)
            Assert.NotEmpty(w.Dzieci)
            SprawdzNazwyWGrupach(w.Dzieci)
        Next
    End Sub

    <Fact>
    Public Sub ZbioryNidMajaPoprawneZapytaniaGetMapICapabilities()
        Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)
        Dim folder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "warstwy")
        Dim z = New Zasieg(486944, 637244, 487456, 637756)

        'warstwy wieloskalowe (ZEN, ZN_ZASIEGI, GCW) łączą pasma skali bez długiego zlepka jako nazwy warstwy
        Dim zen = PlikWarstw.Wczytaj(Path.Combine(folder, "zabytki_ewidencja_nieruchomych_NID.txt"))
        Assert.Single(zen.Warstwy)
        Dim urlZen = Wms.ZapytanieGetMap(zen.Adres, {zen.Warstwy(0).Nazwa}, UkladWspolrzednych.PL1992, z, "png", 512, 512, False, True)
        Assert.Contains("layers=Ewidencja_zabytkow_nieruchomych_II,Ewidencja_zabytkow_nieruchomych_III,Ewidencja_zabytkow_nieruchomych_IV,Ewidencja_zabytkow_nieruchomych_I&", urlZen)
        Assert.Equal("https://usluga.zabytek.gov.pl/ZEN/service.svc/get?SERVICE=WMS&REQUEST=GetCapabilities", KlientCapabilities.AdresCapabilitiesWmsZAdresuPliku(zen.Adres))

        Dim zas = PlikWarstw.Wczytaj(Path.Combine(folder, "zabytki_zasiegi_NID.txt"))
        Assert.Single(zas.Warstwy)
        Dim urlZas = Wms.ZapytanieGetMap(zas.Adres, {zas.Warstwy(0).Nazwa}, UkladWspolrzednych.PL1992, z, "png", 512, 512, False, True)
        Assert.Contains("layers=Zabytki_nieruchome_II,Zabytki_nieruchome_III,Zabytki_nieruchome_IV,Zabytki_nieruchome_I&", urlZas)
        Assert.Equal("https://usluga.zabytek.gov.pl/ZN_ZASIEGI/service.svc/get?SERVICE=WMS&REQUEST=GetCapabilities", KlientCapabilities.AdresCapabilitiesWmsZAdresuPliku(zas.Adres))

        Dim gcw = PlikWarstw.Wczytaj(Path.Combine(folder, "zabytki_groby_wojenne_NID.txt"))
        Assert.Single(gcw.Warstwy)
        Dim urlGcw = Wms.ZapytanieGetMap(gcw.Adres, {gcw.Warstwy(0).Nazwa}, UkladWspolrzednych.PL1992, z, "png", 512, 512, False, True)
        Assert.Contains("layers=Groby_i_cmentarze_wojenne_II,Groby_i_cmentarze_wojenne_III,Groby_i_cmentarze_wojenne_I&", urlGcw)
        Assert.Equal("https://usluga.zabytek.gov.pl/GCW/service.svc/get?SERVICE=WMS&REQUEST=GetCapabilities", KlientCapabilities.AdresCapabilitiesWmsZAdresuPliku(gcw.Adres))

        'warstwa ZEA z polskim znakiem 'ó' odczytana w Windows-1250 i zakodowana w URI jako UTF-8 (%C3%B3)
        Dim zea = PlikWarstw.Wczytaj(Path.Combine(folder, "zabytki_ewidencja_archeo_NID.txt"))
        Assert.Equal("Ewidencja_zabytków_archeologicznych", zea.Warstwy(0).Nazwa)
        Dim urlZea = Wms.ZapytanieGetMap(zea.Adres, {zea.Warstwy(0).Nazwa}, UkladWspolrzednych.PL1992, z, "png", 512, 512, False, True)
        Assert.Contains("%C3%B3", New Uri(urlZea).AbsoluteUri)
    End Sub

    <Fact>
    Public Sub SkroconaNazwaSesji_ZwracaKrotkiIdentyfikator()
        Dim folder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "warstwy")
        Dim k = KatalogWarstw.Wczytaj(Path.Combine(folder, KatalogWarstw.NazwaPliku))

        ' Test dla skanów map topograficznych
        Dim c10 As New KonfiguracjaSesji With {.RodzajMapy = "skany_map_topograficznych", .LiczbaWarstw = 1}
        c10.Warstwy(0) = "Raster_10_1965"
        Assert.Equal("Topo_65_10k", k.SkroconaNazwaSesji(c10))

        Dim c25 As New KonfiguracjaSesji With {.RodzajMapy = "skany_map_topograficznych", .LiczbaWarstw = 1}
        c25.Warstwy(0) = "Raster_25_1965"
        Assert.Equal("Topo_65_25k", k.SkroconaNazwaSesji(c25))

        Dim c50 As New KonfiguracjaSesji With {.RodzajMapy = "skany_map_topograficznych", .LiczbaWarstw = 1}
        c50.Warstwy(0) = "Raster_50_1965"
        Assert.Equal("Topo_65_50k", k.SkroconaNazwaSesji(c50))

        ' Test dla ortofotomapy WMTS
        Dim cOrto As New KonfiguracjaSesji With {.RodzajMapy = "ortofotomapa_WMTS", .LiczbaWarstw = 1}
        cOrto.Warstwy(0) = "ORTOFOTOMAPA"
        Assert.Equal("Orto_Std_25cm", k.SkroconaNazwaSesji(cOrto))

        ' Test dla sesji z dwiema warstwami
        Dim cMix As New KonfiguracjaSesji With {.RodzajMapy = "ortofotomapa_WMTS", .LiczbaWarstw = 2}
        cMix.Warstwy(0) = "ORTOFOTOMAPA"
        cMix.Warstwy(1) = "Raster_10_1965"
        Assert.Equal("Orto_Std_25cm+Topo_65_10k", k.SkroconaNazwaSesji(cMix))

        ' Test pełnego opisu
        Dim opis = k.PelnyOpisSesji(c10)
        Assert.Contains("1:10 000, układ 1965", opis)
        Assert.Contains("skany_map_topograficznych Raster_10_1965", opis)
    End Sub

End Class
