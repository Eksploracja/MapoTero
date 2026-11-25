'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'Testy stylu szybkiego kreatora mapy (MapoTero.Core/StylKreatora.vb) - bez tworzenia okna kreatora.
Imports MapoTero.Core
Imports Xunit

Public Class StylKreatoraTesty

    Private Shared Function Styl(id As String) As StylKreatora
        Dim s As New StylKreatora()
        s.Wybierz(ZarzadcaSzablonow.ZnajdzSzablon(id))
        s.UstawRegulacje(False)
        Return s
    End Function

    Private Shared Function Zestaw(ParamArray nazwy() As String) As ZestawWarstw
        Dim z As New ZestawWarstw()
        For Each n In nazwy
            z.Dodaj(n, 100, "https://example.com/wms", "test")
        Next
        Return z
    End Function

    <Fact>
    Public Sub Wybierz_InnyStylKasujeZmiany_TenSamZachowuje()
        Dim s = Styl("historia_wig")
        s.ZestawNiestandardowy = Zestaw("a")
        s.PlanWlasny = New List(Of PlanStopnia)()
        Assert.False(s.Wybierz(ZarzadcaSzablonow.ZnajdzSzablon("historia_wig")))
        Assert.NotNull(s.ZestawNiestandardowy)
        Assert.NotNull(s.PlanWlasny)

        Assert.True(s.Wybierz(ZarzadcaSzablonow.ZnajdzSzablon("dziedzictwo")))
        Assert.Null(s.ZestawNiestandardowy)
        Assert.Null(s.PlanWlasny)
        Assert.Equal("dziedzictwo", s.Szablon.Id)
    End Sub

    <Fact>
    Public Sub Dziedzictwo_WylaczonaGrupaUsuwaWarstwy()
        Dim s = Styl("dziedzictwo")
        Assert.Contains("Zabytki archeologiczne", s.GrupyDoWyboru())
        s.WylaczoneGrupy.Add("Zabytki archeologiczne")
        Dim z = s.UtworzZestaw(1.0)
        Assert.DoesNotContain(z.Warstwy, Function(w) w.Zbior.Contains("archeo"))
        Assert.Contains(z.Warstwy, Function(w) w.Zbior = "zabytki_zasiegi_NID")
        ' nowy wybór stylu (bez zachowania ustawień) włącza grupy z powrotem
        s.UstawRegulacje(False)
        Assert.Empty(s.WylaczoneGrupy)
    End Sub

    <Fact>
    Public Sub ZestawNiestandardowy_KrycieZOknaNadrzedne_SuwakProporcjonalny()
        Dim s = Styl("dziedzictwo")
        Dim wlasny = s.UtworzZestaw(1.0)
        wlasny.UstawKrycie(3, 40)
        s.ZestawNiestandardowy = wlasny
        Dim wykryte = s.UstawRegulacje(False)
        Dim z = s.UtworzZestaw(1.0)
        Assert.Equal(100, z(2).Krycie)
        Assert.Equal(40, z(3).Krycie)
        ' grupy zabytków nie mają pól wyboru w zestawie własnym
        Assert.Empty(s.GrupyDoWyboru())

        ' suwak zabytków o połowę mniejszy - każda warstwa grupy proporcjonalnie (40% -> 20%)
        Dim zabytki = wykryte.First(Function(kv) kv.Key = RodzajRegulacji.Zabytki)
        s.Regulacje(RodzajRegulacji.Zabytki) = zabytki.Value \ 2
        Dim poSuwaku = s.UtworzZestaw(1.0)
        Assert.Equal(CInt(Math.Round(40 * (zabytki.Value \ 2) / CDbl(zabytki.Value), MidpointRounding.AwayFromZero)), poSuwaku(3).Krycie)
    End Sub

    <Fact>
    Public Sub ZestawWlasny_ZOknaGlownego()
        Dim s As New StylKreatora(zestawOknaGlownego:=Function() Zestaw("TOPO", "ISOK_Cien"))
        s.Wybierz(New SzablonMapy With {.Id = StylKreatora.IdWlasnegoZestawu, .Nazwa = "Warstwy z okna głównego"})
        Assert.True(s.CzyWlasnyZestaw)
        Assert.Equal(2, s.UtworzZestaw().Liczba)
        Assert.Equal("TOPO + ISOK_Cien", s.OpisSkladu())
        Assert.Null(s.BladWyboru())

        Dim pusty As New StylKreatora(zestawOknaGlownego:=Function() New ZestawWarstw())
        pusty.Wybierz(New SzablonMapy With {.Id = StylKreatora.IdWlasnegoZestawu})
        Assert.StartsWith("W oknie głównym nie wybrano", pusty.BladWyboru())
        Assert.StartsWith("(w oknie głównym", pusty.OpisSkladu())
    End Sub

    <Fact>
    Public Sub BladWyboru_BezStylu()
        Assert.Equal("Wybierz styl mapy z listy.", New StylKreatora().BladWyboru())
        Assert.Null(Styl("piesza_gory").BladWyboru())
    End Sub

    <Fact>
    Public Sub MapyWig_OknoWarstwPokazujeNajdokladniejszaSkale()
        Dim s = Styl("historia_wig")
        Dim st = s.StopienOknaWarstw(Nothing)
        Assert.Equal("1:25 000", st.Opis)
        Assert.Contains(st.Warstwy, Function(w) w.Warstwa = "wig25k_2180")
        ' skala wybrana ręcznie w podglądzie - okno pokazuje warstwy podglądu
        Assert.Null(s.StopienOknaWarstw(s.Szablon.StopnieSkali(1)))
        ' plan z okna „Ustawienia skal”: najdokładniejsza skala planu
        s.PlanWlasny = New List(Of PlanStopnia) From {
            New PlanStopnia With {.Stopien = s.Szablon.StopnieSkali(2), .Piksel = 20},
            New PlanStopnia With {.Stopien = s.Szablon.StopnieSkali(1), .Piksel = 8}}
        Assert.Same(s.Szablon.StopnieSkali(1), s.StopienOknaWarstw(Nothing))
        Assert.Equal(2, s.StopniePodgladu().Count)
        ' zestaw niestandardowy jest jeden dla wszystkich skal
        s.ZestawNiestandardowy = Zestaw("a")
        Assert.Null(s.StopienOknaWarstw(Nothing))
        Assert.Empty(s.StopniePodgladu())
    End Sub

    <Fact>
    Public Sub PrzyjmijZestawOkna_TenSamSkladZmienaWyglad_InnySkladPoPotwierdzeniu()
        Dim s = Styl("historia_wig")
        Dim przed = s.UtworzZestaw(0, s.Szablon.StopnieSkali(0))
        Dim po = przed.Kopia()
        po.UstawKrycie(0, 55)
        Assert.Equal(ZmianaZestawuOkna.ZmianaWygladu, s.PrzyjmijZestawOkna(przed, po, Function() False))
        Assert.Null(s.ZestawNiestandardowy)
        ' zmiana krycia obowiązuje także w innej skali z tą samą warstwą
        Dim nazwa = przed(0).Nazwa
        For Each st In s.Szablon.StopnieSkali
            Dim z = s.UtworzZestaw(0, st)
            For i = 0 To z.Liczba - 1
                If z(i).Nazwa = nazwa Then Assert.Equal(55, z(i).Krycie)
            Next
        Next

        Dim inny = przed.Kopia()
        inny.Usun(0)
        Assert.Equal(ZmianaZestawuOkna.Brak, s.PrzyjmijZestawOkna(przed, inny, Function() False))
        Assert.Null(s.ZestawNiestandardowy)
        Assert.Equal(ZmianaZestawuOkna.ZestawNiestandardowy, s.PrzyjmijZestawOkna(przed, inny, Function() True))
        Assert.Same(inny, s.ZestawNiestandardowy)
        Assert.False(s.MozliwaWieloskalowa(RodzajEksportu.MBTiles))
    End Sub

    <Fact>
    Public Sub Wieloskalowa_TylkoStylWieloskalowyIFormatKafelkowy()
        Assert.True(StylKreatora.FormatWieloskalowy(RodzajEksportu.MBTiles))
        Assert.True(StylKreatora.FormatWieloskalowy(RodzajEksportu.SQLiteDB))
        Assert.True(StylKreatora.FormatWieloskalowy(RodzajEksportu.Jnx))
        Assert.False(StylKreatora.FormatWieloskalowy(RodzajEksportu.KmzGarmin))
        Assert.False(StylKreatora.FormatWieloskalowy(RodzajEksportu.GeoTiff))

        Dim wig = Styl("historia_wig")
        Assert.True(wig.MozliwaWieloskalowa(RodzajEksportu.MBTiles))
        Assert.True(wig.WieloskalowaAktywna(RodzajEksportu.MBTiles, True))
        Assert.False(wig.WieloskalowaAktywna(RodzajEksportu.MBTiles, False))
        Assert.False(wig.WieloskalowaAktywna(RodzajEksportu.GeoTiff, True))
        Assert.False(New StylKreatora().MozliwaWieloskalowa(RodzajEksportu.MBTiles))
    End Sub

    <Fact>
    Public Sub PlanBiezacy_KopiaPlanuWlasnego()
        Dim s = Styl("historia_wig")
        Dim auto = s.PlanBiezacy(6, 52, False)
        Assert.NotEmpty(auto)
        Assert.Same(s.Szablon.StopnieGlowne(0), auto(0).Stopien)
        s.PlanWlasny = New List(Of PlanStopnia) From {New PlanStopnia With {.Stopien = s.Szablon.StopnieSkali(0), .Piksel = 3, .PoziomMin = 10, .PoziomMax = 16}}
        Dim kopia = s.PlanBiezacy(6, 52, False)
        Assert.Single(kopia)
        Assert.NotSame(s.PlanWlasny(0), kopia(0))
        kopia(0).Piksel = 99
        Assert.Equal(3, s.PlanWlasny(0).Piksel)
    End Sub

    <Fact>
    Public Sub OpisSkaliPodgladu_SkalaStyluWieloskalowego()
        Dim s = Styl("historia_wig")
        Assert.Equal("Skala " & s.Szablon.StopnieSkali(1).Opis, s.OpisSkaliPodgladu(s.Szablon.StopnieSkali(1), 0))
        Assert.StartsWith("Skala ", s.OpisSkaliPodgladu(Nothing, 6))
        Assert.Equal("Podgląd gotowy", Styl("dzialka_kataster").OpisSkaliPodgladu(Nothing, 1))
    End Sub

    <Fact>
    Public Sub ProbkaPodgladu_SrodekObszaruIPikselSkali()
        Dim s = Styl("historia_wig")
        Dim obszar As New Zasieg(500000, 600000, 502000, 604000)
        Assert.Equal(obszar.Srodek, s.PunktProbki(Nothing, obszar))
        Assert.Equal(New PunktXY(1, 2), s.PunktProbki(New PunktXY(1, 2), obszar))
        Dim przyklad = s.PunktProbki(Nothing, New Zasieg())
        Assert.Equal(UkladWspolrzednych.PL1992.ZWgs84(s.Szablon.ProbkaLat, s.Szablon.ProbkaLon).X, przyklad.X, 3)
        Dim st = s.Szablon.StopnieSkali(0)
        Assert.Equal(st.Piksel / 2, s.PikselProbki(st, 2.0), 6)
        Assert.Equal(s.Szablon.ProbkaPiksel, s.PikselProbki(Nothing, 1.0), 6)
    End Sub

    <Fact>
    Public Sub UstawieniaDomyslne_PikselIObszarPrzykladowy()
        Dim katastr = Styl("dzialka_kataster")
        Assert.Equal(katastr.Szablon.DomyslnyPiksel, katastr.PikselDomyslny(5))
        Assert.Equal(1500, katastr.WzorcowyZasieg().Szerokosc, 3)
        Assert.Equal(1.0, New StylKreatora().PikselDomyslny(0))
        Assert.False(New StylKreatora().WzorcowyZasieg().Poprawny)
        Dim wlasny As New StylKreatora()
        wlasny.Wybierz(New SzablonMapy With {.Id = StylKreatora.IdWlasnegoZestawu, .DomyslnyPiksel = 1})
        Assert.Equal(2.5, wlasny.PikselDomyslny(2.5))
        Assert.Equal("OdbiornikGarmin|dzialka_kataster", katastr.KluczDomyslnych(CelMapy.OdbiornikGarmin))
    End Sub

    <Fact>
    Public Sub Korekta_ProfilSugerowanyDlaGarmina()
        Dim s = Styl("piesza_gory")
        Assert.Equal(ProfilKorektyRastra.GarminMip, s.ProfilSugerowany(CelMapy.OdbiornikGarmin))
        Dim opcje As New OpcjeKorekcjiMip()
        opcje.Resetuj()
        Assert.StartsWith("Wyłączona (sugerowany profil:", s.OpisKorekty(False, opcje, CelMapy.OdbiornikGarmin))
        ' okno korekty pokazuje profil sugerowany, ale korekta zostaje wyłączona
        Dim doOkna = s.OpcjeKorektyDoOkna(opcje, CelMapy.OdbiornikGarmin)
        Assert.Equal(ProfilKorektyRastra.GarminMip, doOkna.Profil)
        Assert.False(doOkna.Wlaczona)
        Assert.Equal(ProfilKorektyRastra.Brak, opcje.Profil)
        ' zaznaczenie pola bez profilu włącza profil sugerowany
        s.WlaczKorekte(opcje, True, CelMapy.OdbiornikGarmin)
        Assert.True(opcje.Wlaczona)
        Assert.Equal(ProfilKorektyRastra.GarminMip, opcje.Profil)
        Assert.StartsWith("Profil: ", s.OpisKorekty(True, opcje, CelMapy.OdbiornikGarmin))
    End Sub

    <Fact>
    Public Sub Korekta_BezProfiluStyluWyostrzenie()
        Dim s As New StylKreatora()
        s.Wybierz(New SzablonMapy With {.Id = "x", .ProfilKorekty = ProfilKorektyRastra.Brak})
        Dim opcje As New OpcjeKorekcjiMip()
        opcje.Resetuj()
        Assert.Equal("Wyłączona", s.OpisKorekty(False, opcje, CelMapy.TelefonTablet))
        s.WlaczKorekte(opcje, True, CelMapy.TelefonTablet)
        Assert.Equal(ProfilKorektyRastra.WyostrzenieKrawedzi, opcje.Profil)
    End Sub

    <Fact>
    Public Sub StylWlasny_SkaleZKryciemSuwaka()
        Dim s = Styl("historia_wig")
        s.Regulacje(RodzajRegulacji.Cieniowanie) = 20
        Dim obszar As New Zasieg(500000, 600000, 502000, 604000)
        Dim wlasny = s.UtworzStylWlasny("Test", obszar)
        Assert.Equal(3, wlasny.StopnieSkali.Count)
        Assert.Equal(20, wlasny.StopnieSkali(0).Warstwy.First(Function(w) w.Warstwa = "ISOK_Cien").Krycie)
        Dim srodek = UkladWspolrzednych.PL1992.DoWgs84(obszar.Srodek)
        Assert.Equal(srodek.Szerokosc, wlasny.ProbkaLat, 6)
        Assert.Null(New StylKreatora().UtworzStylWlasny("x", obszar))
    End Sub

    <Fact>
    Public Sub OpisMapyWieloskalowej_BezWylaczonychGrup()
        Dim s = Styl("dziedzictwo")
        Dim plan = s.PlanBiezacy(s.Szablon.DomyslnyPiksel, 52, False)
        If plan.Count = 0 Then plan = New List(Of PlanStopnia) From {New PlanStopnia With {.Stopien = New StopienSkali(), .Warstwy = s.Szablon.Warstwy.ToList()}}
        Dim wszystkie = s.AtrybucjaWieloskalowej(plan)
        s.WylaczoneGrupy.Add("Zabytki archeologiczne")
        Dim bezArcheo = s.AtrybucjaWieloskalowej(plan)
        Assert.StartsWith("MapoTero – ", bezArcheo)
        Assert.True(bezArcheo.Length < wszystkie.Length)
        Assert.StartsWith(s.Szablon.Nazwa & " – ", s.OpisMapyWieloskalowej(plan))
    End Sub

    <Fact>
    Public Sub ZrodlaMap_WarstwyKazdejSkaliMajaZbior()
        Dim s As New StylKreatora()
        Dim zrodla = s.ZrodlaMap()
        Assert.Same(zrodla, s.ZrodlaMap())
        Assert.True(zrodla.ContainsKey("TOPO_WMTS"))
    End Sub

    <Fact>
    Public Sub StylJednoskalowy_PoDodaniuSkalWPlanWlasny_JestWieloskalowyIZapisujeSkale()
        Dim zOkna As New ZestawWarstw()
        zOkna.Dodaj("ORTOFOTOMAPA", 100, "https://example.invalid/orto", "ortofotomapa_WMTS", 0.25, TypUslugiMapowej.Wmts)
        Dim s As New StylKreatora(zestawOknaGlownego:=Function() zOkna)
        s.Wybierz(ZarzadcaSzablonow.ZnajdzSzablon(StylKreatora.IdWlasnegoZestawu))

        Assert.False(s.CzyWieloskalowy)
        Dim plan = s.PlanDoEdycjiSkal(0.25, 52, False)
        Assert.Single(plan)

        Dim stPrzegl As New StopienSkali With {.Opis = "przeglądowa", .Piksel = 10.0}
        stPrzegl.Warstwy.Add(New WarstwaSzablonu("TOPO_WMTS", "MAPA TOPOGRAFICZNA", 100))
        plan.Add(New PlanStopnia With {.Stopien = stPrzegl, .Piksel = 10.0, .PoziomMin = 10, .PoziomMax = 14, .Warstwy = stPrzegl.Warstwy.ToList()})
        s.PlanWlasny = plan

        Assert.True(s.CzyWieloskalowy)
        Assert.True(s.MozliwaWieloskalowa(RodzajEksportu.MBTiles))
        Assert.Equal("MAPA TOPOGRAFICZNA", s.UtworzZestaw(12.0)(0).Nazwa)
        Assert.Equal("ORTOFOTOMAPA", s.UtworzZestaw(0.3)(0).Nazwa)

        Dim styl = s.UtworzStylWlasny("Mój wieloskalowy", New Zasieg(500000, 600000, 502000, 604000))
        Assert.True(styl.Wieloskalowy)
        Assert.Equal(2, styl.StopnieSkali.Count)
        Assert.Equal(0.25, styl.DomyslnyPiksel, 4)
    End Sub

    <Fact>
    Public Sub PrzyjmijZestawDlaSkali_ZmieniaTylkoWybranaSkaleINieNiszczyPozostalych()
        Dim s = Styl("historia_wig")
        Dim st25 = s.Szablon.StopnieGlowne(0)
        Dim st100 = s.Szablon.StopnieGlowne(1)
        Dim nowy As New ZestawWarstw()
        nowy.Dodaj("wig25k_2180", 90, "https://example.invalid/wms", "mapy_historyczne_IHPAN", 2.0, TypUslugiMapowej.Wms)

        s.PrzyjmijZestawDlaSkali(st25, nowy, 2.0, 52, False)

        Assert.NotNull(s.PlanWlasny)
        Assert.True(s.CzyWieloskalowy)
        Assert.Equal(3, s.PlanWlasny.Count)
        Assert.Single(s.UtworzZestaw(0, s.PlanWlasny(0).Stopien).Warstwy)
        Assert.Equal(90, s.UtworzZestaw(0, s.PlanWlasny(0).Stopien)(0).Krycie)
        ' Pozostałe skale zachowały swój skład
        Assert.Equal(st100.Warstwy.Count, s.PlanWlasny(1).WarstwyEfektywne.Count)
    End Sub

End Class
