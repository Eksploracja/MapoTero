'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'Stopień opcjonalny szablonu: ortofotomapa na powiększeniach ponad najdokładniejszą mapę (Turystyka piesza, Góry z poziomicami).
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports MapoTero.Core
Imports Xunit

Public Class OrtofotomapaPrzyZblizeniuTesty

    <Theory>
    <InlineData("piesza_gory")>
    <InlineData("gory_poziomice")>
    Public Sub SzablonyTurystyczne_MajaOrtofotomapeJakoStopienOpcjonalny(id As String)
        Dim s = ZarzadcaSzablonow.ZnajdzSzablon(id)
        Dim st = s.StopienOpcjonalny
        Assert.NotNull(st)
        Assert.Equal(ZarzadcaSzablonow.PikselStopniaOrtofotomapy, st.Piksel)
        ' polska ortofotomapa na spodzie - poza Polską ma nieprzezroczystą czerń, która zasłoniłaby niższe warstwy
        Assert.Equal("ortofotomapa_WMTS", st.Warstwy(0).Zbior)
        Assert.False(st.Warstwy(0).Nakladka)
        Assert.Equal("Waymarked_Trails", st.Warstwy.Last().Zbior)
        ' cieniowanie rzeźby tylko przyciemnia zdjęcie
        Assert.DoesNotContain(st.Warstwy, Function(w) w.Warstwa = "ISOK_Cien")
        ' stopień opcjonalny nie zmienia najdokładniejszej skali ani opisu skal
        Assert.False(s.StopnieSkali(0).Opcjonalny)
        Assert.DoesNotContain("ortofotomapa", s.OpisSkal())
    End Sub

    <Fact>
    Public Sub GoryZPoziomicami_OrtofotomapySasiadowZWmsNadPolska()
        Dim st = ZarzadcaSzablonow.ZnajdzSzablon("gory_poziomice").StopienOpcjonalny
        Dim zbiory = st.Warstwy.Select(Function(w) w.Zbior).ToList()
        Assert.Equal({"ortofotomapa_WMTS", "slowacja_orto_wms", "czechy_orto_wms", "niemcy_saksonia_dop_wms", "Waymarked_Trails"}, zbiory.ToArray())
        ' czeskie kafle WMTS mają dziury przy granicy (HTTP 400), a brak kafla przerywa pobieranie segmentu
        Assert.DoesNotContain("czechy_orto_WMTS", zbiory)
    End Sub

    <Fact>
    Public Sub PlanStopni_BezZgodyUzytkownikaNieMaOrtofotomapy()
        Dim plan = ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"), 2.0, 50.0)
        Assert.DoesNotContain(plan, Function(ps) ps.Stopien.Opcjonalny)
        Assert.Equal(16, plan.Max(Function(ps) ps.PoziomMax))
    End Sub

    <Fact>
    Public Sub PlanStopni_OrtofotomapaZaczynaSieZarazNadMapa()
        Dim s = ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory")
        ' południe Polski: mapa 2 m kończy się na zoom 16, ortofotomapa 0,75 m - zoom 17
        Dim plan = ZarzadcaSzablonow.PlanStopni(s, 2.0, 50.0, zOpcjonalnym:=True)
        Dim orto = plan(0)
        Assert.True(orto.Stopien.Opcjonalny)
        Assert.Equal(17, orto.PoziomMin)
        Assert.Equal(17, orto.PoziomMax)
        Assert.Equal(0.75, orto.Piksel)
        Assert.Equal("", ZarzadcaSzablonow.SprawdzPlan(plan))
        Assert.Empty(ZarzadcaSzablonow.LukiPlanu(plan))
        ' północ: mapa 2 m kończy się na zoom 15 - ortofotomapa wypełnia też zoom 16 (bez pustego poziomu)
        Dim planPolnoc = ZarzadcaSzablonow.PlanStopni(s, 2.0, 54.5, zOpcjonalnym:=True)
        Assert.Equal(15, planPolnoc.Where(Function(ps) Not ps.Stopien.Opcjonalny).Max(Function(ps) ps.PoziomMax))
        Assert.Equal(16, planPolnoc(0).PoziomMin)
        Assert.Equal(17, planPolnoc(0).PoziomMax)
        Assert.Empty(ZarzadcaSzablonow.LukiPlanu(planPolnoc))
    End Sub

    <Fact>
    Public Sub PlanStopni_DokladnaMapaNiePotrzebujeOrtofotomapy()
        ' piksel mapy 1 m jest już prawie tak dokładny jak ortofotomapa 0,75 m
        Dim plan = ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"), 1.0, 50.0, zOpcjonalnym:=True)
        Assert.DoesNotContain(plan, Function(ps) ps.Stopien.Opcjonalny)
        ' szablon bez stopnia opcjonalnego - plan bez zmian
        Dim rower = ZarzadcaSzablonow.ZnajdzSzablon("rower_mtb")
        Assert.Equal(ZarzadcaSzablonow.PlanStopni(rower, 2.0, 50.0).Count, ZarzadcaSzablonow.PlanStopni(rower, 2.0, 50.0, zOpcjonalnym:=True).Count)
    End Sub

    <Fact>
    Public Sub StopienDlaPiksela_NieWybieraOrtofotomapy()
        ' piksel 1 m jest bliżej 0,75 m niż 2 m - arkusz do druku miałby zdjęcie zamiast mapy topograficznej
        Dim s = ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory")
        Assert.Equal("1:10 000", s.StopienDlaPiksela(1.0).Opis)
        Assert.Equal("1:10 000", s.StopienDlaPiksela(0.5).Opis)
        Assert.DoesNotContain(s.WarstwyDlaPiksela(0.5), Function(w) w.Zbior = "ortofotomapa_WMTS")
    End Sub

    <Fact>
    Public Sub StyleWlasne_ZachowujaStopienOpcjonalny()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_style_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim plik = Path.Combine(folder, SzablonyWlasne.NazwaPliku)
            Dim s As New SzablonMapy(SzablonyWlasne.NoweId(), "Moje góry", SzablonyWlasne.KategoriaWlasna, "", "⭐", 2.0, ProfilKorektyRastra.Brak)
            s.DodajStopien("szczegółowa", 2.0, New WarstwaSzablonu("OpenTopoMap", "opentopomap", 100))
            s.DodajStopien("przeglądowa", 30.0, New WarstwaSzablonu("OpenTopoMap", "opentopomap", 100))
            s.DodajStopienOpcjonalny(ZarzadcaSzablonow.OpisStopniaOrtofotomapy, 0.75, New WarstwaSzablonu("ortofotomapa_WMTS", "ORTOFOTOMAPA", 100))
            SzablonyWlasne.Zapisz(plik, {s})
            Dim w = SzablonyWlasne.Wczytaj(plik)(0)
            Assert.Equal({False, False, True}, w.StopnieSkali.Select(Function(st) st.Opcjonalny).ToArray())
            Assert.Equal("szczegółowa", w.StopnieSkali(0).Opis)
            Assert.Equal("ORTOFOTOMAPA", w.StopienOpcjonalny.Warstwy(0).Warstwa)
            ' stopnie główne zapisane bez pola Opcjonalny - plik czytelny dla poprzedniej wersji kreatora
            Assert.Equal(1, File.ReadAllText(plik).Split("""Opcjonalny""").Length - 1)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

End Class
