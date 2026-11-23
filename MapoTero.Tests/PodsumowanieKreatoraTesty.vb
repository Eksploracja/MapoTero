'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'Testy kroku 4 i generowania szybkiego kreatora (MapoTero.Core/PodsumowanieKreatora.vb, PomiarPostepu.vb,
'wyszukiwarka i opis obszaru w ObszarKreatora.vb) - bez tworzenia okna kreatora.
Imports System.IO
Imports MapoTero.Core
Imports Xunit

Public Class PodsumowanieKreatoraTesty

#Region "Pliki"

    <Fact>
    Public Sub NazwaPliku_ZMiejscaStyluIDaty()
        Dim data As New DateTime(2026, 11, 21, 15, 4, 5, 678)
        Assert.Equal("Mapa_Nowy_Targ_piesza_gory_2026-11-21", PodsumowanieKreatora.ProponowanaNazwaPliku("Nowy Targ", "piesza_gory", data))
        Assert.Equal("Mapa_piesza_gory_2026-11-21", PodsumowanieKreatora.ProponowanaNazwaPliku(Nothing, "piesza_gory", data))
        Assert.Equal("Mapa_kreator_2026-11-21", PodsumowanieKreatora.ProponowanaNazwaPliku(Nothing, Nothing, data))
        ' miejsce bez nazwy (np. wynik wyszukiwarki bez krótkiej nazwy) - jak dotąd „mapa”
        Assert.Equal("Mapa_mapa_x_2026-11-21", PodsumowanieKreatora.ProponowanaNazwaPliku("", "x", data))
        Assert.Equal("Kreator_Mapa_x_20261121_150405_678", PodsumowanieKreatora.NazwaFolderuSesji("C:\mapy\Mapa x.mbtiles", data))
    End Sub

    <Fact>
    Public Sub OczyscNazwePliku_BezZnakowNiedozwolonych()
        Assert.Equal("Rynek_Główny", PodsumowanieKreatora.OczyscNazwePliku(" Rynek Główny "))
        Assert.Equal("ab", PodsumowanieKreatora.OczyscNazwePliku("a/b"))
        Assert.Equal("mapa", PodsumowanieKreatora.OczyscNazwePliku("  "))
        Assert.Equal("mapa", PodsumowanieKreatora.OczyscNazwePliku("<>|"))
    End Sub

    <Fact>
    Public Sub UnikalnaSciezka_DopisujeKolejnyNumer()
        Dim zajete As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {"C:\m\a.mbtiles", "C:\m\a_2.mbtiles"}
        Assert.Equal("C:\m\a_3.mbtiles", PodsumowanieKreatora.UnikalnaSciezka("C:\m\a.mbtiles", AddressOf zajete.Contains))
        Assert.Equal("C:\m\b.mbtiles", PodsumowanieKreatora.UnikalnaSciezka("C:\m\b.mbtiles", AddressOf zajete.Contains))
    End Sub

    <Fact>
    Public Sub FolderStopnia_NajdokladniejszyWFolderzeSesji()
        Assert.Equal("C:\d\Kreator_a\", PodsumowanieKreatora.FolderStopnia("C:\d\Kreator_a\", 0))
        Assert.Equal("C:\d\Kreator_a_skala2\", PodsumowanieKreatora.FolderStopnia("C:\d\Kreator_a\", 1))
        Assert.Equal("C:\d\Kreator_a_skala3\", PodsumowanieKreatora.FolderStopnia("C:\d\Kreator_a\", 2))
    End Sub

#End Region

#Region "Sprawdzenie przed generowaniem"

    <Fact>
    Public Sub BladSiatki_PustyObszar()
        Dim dobry = PodsumowanieKreatora.SiatkaKreatora(New Zasieg(500000, 600000, 502000, 603000), 1.0)
        Assert.Null(PodsumowanieKreatora.BladSiatki(dobry))
        Dim pusty = PodsumowanieKreatora.SiatkaKreatora(New Zasieg(), 1.0)
        Assert.Equal(pusty.PowodNiepoprawnosci, PodsumowanieKreatora.BladSiatki(pusty))
        Assert.False(String.IsNullOrEmpty(PodsumowanieKreatora.BladSiatki(pusty)))
    End Sub

    <Fact>
    Public Sub SiatkaKreatora_MalyObszarMniejszySegmentTenSamPiksel()
        Dim z As New Zasieg(500000, 600000, 500100, 600070)
        Dim s = PodsumowanieKreatora.SiatkaKreatora(z, 0.25)
        Assert.Equal(0.25, s.RozmiarPiksela)
        Assert.Equal(ZarzadcaSzablonow.BokSegmentuKreatora(z, 0.25), s.BokSegmentuPx)
        Assert.True(s.BokSegmentuPx < 1024)
    End Sub

    <Theory>
    <InlineData(10, 10, False, Nothing, "Nie pobrano danych mapy.")>
    <InlineData(10, 2, True, Nothing, "Zachowano poprzednią mapę.")>
    <InlineData(10, 2, True, "C:\m\a.mbtiles", Nothing)>
    <InlineData(10, 2, False, Nothing, Nothing)>
    <InlineData(10, 0, True, Nothing, Nothing)>
    Public Sub WstrzymanieZapisu_ChroniPoprzedniaMape(wszystkie As Integer, nieudane As Integer, istnieje As Boolean, plikZBrakami As String, oczekiwany As String)
        Assert.Equal(oczekiwany, PodsumowanieKreatora.PowodWstrzymaniaZapisu(wszystkie, nieudane, istnieje, "C:\m\A.mbtiles", plikZBrakami))
    End Sub

    <Fact>
    Public Sub PotrzebneMiejsce_DwaRazyRozmiarPliku()
        Dim sz As New SzacunekKreatora With {.SzacowanyRozmiarMbMax = 10, .Ostrzezenie = "Duży obszar."}
        Assert.Equal(20L * 1024 * 1024, PodsumowanieKreatora.PotrzebneMiejsceBajty(sz))
        Assert.StartsWith("Duży obszar.", PodsumowanieKreatora.PytanieDuzyObszar(sz))
        Assert.EndsWith("Czy mimo to rozpocząć pobieranie?", PodsumowanieKreatora.PytanieDuzyObszar(sz))
    End Sub

#End Region

#Region "Opisy"

    <Fact>
    Public Sub OpisPobierania_WieloskalowaKorytarzProstokat()
        Dim sz As New SzacunekKreatora With {.LiczbaSegmentow = 5, .SiatkaKolumny = 3, .SiatkaWiersze = 2}
        Assert.StartsWith("Pobieranie: ", PodsumowanieKreatora.OpisPobierania(sz, 3))
        Assert.EndsWith(" segmentów w 3 skalach", PodsumowanieKreatora.OpisPobierania(sz, 3))
        Assert.Equal("Segmenty do pobrania: 3 × 2", PodsumowanieKreatora.OpisPobierania(sz, 0))
        sz.CzyKorytarz = True
        sz.OszczednoscProcent = 40
        Assert.Contains("(oszczędność 40% względem prostokąta)", PodsumowanieKreatora.OpisPobierania(sz, 0))
        sz.LiczbaSegmentow = 1
        Assert.EndsWith(" w 1 skali", PodsumowanieKreatora.OpisPobierania(sz, 1))
    End Sub

    <Fact>
    Public Sub OpisFormatu_PowiekszeniaTylkoDlaKafli()
        Assert.Equal("Format: " & ZarzadcaSzablonow.NazwaFormatu(RodzajEksportu.MBTiles) & " wieloskalowy, powiększenia 8-16",
                     PodsumowanieKreatora.OpisFormatu(RodzajEksportu.MBTiles, True, 8, 16))
        Assert.EndsWith(", jeden poziom szczegółowości", PodsumowanieKreatora.OpisFormatu(RodzajEksportu.GeoTiff, False, 8, 16))
    End Sub

    <Fact>
    Public Sub ZakresPowiekszen_ZPlanuAlboZalecany()
        Dim plan As New List(Of PlanStopnia) From {
            New PlanStopnia With {.PoziomMin = 12, .PoziomMax = 16}, New PlanStopnia With {.PoziomMin = 8, .PoziomMax = 11}}
        Assert.Equal(Tuple.Create(8, 16), PodsumowanieKreatora.ZakresPowiekszen(plan, 2, 52, True))
        Assert.Null(PodsumowanieKreatora.ZakresPowiekszen(Nothing, 2, 52, True))
        Assert.Equal(ZarzadcaSzablonow.ZalecanyZakresPowiekszen(2, 52), PodsumowanieKreatora.ZakresPowiekszen(Nothing, 2, 52, False))
        ' pusty plan mapy wieloskalowej - zakres zalecany jak dotąd
        Assert.Equal(ZarzadcaSzablonow.ZalecanyZakresPowiekszen(2, 52), PodsumowanieKreatora.ZakresPowiekszen(New List(Of PlanStopnia)(), 2, 52, False))
    End Sub

    <Fact>
    Public Sub PikselZalecany_GdyDokladniejszyNizDane()
        Dim z As New ZestawWarstw()
        z.Dodaj("orto", 100, "https://example.com/wms", "orto", zalecanyPiksel:=0.5)
        Dim najmniejszy = ZarzadcaSzablonow.NajmniejszyPrzydatnyPiksel(z)
        Assert.True(najmniejszy > 0)
        Assert.Equal(najmniejszy * 2, PodsumowanieKreatora.PikselZalecany(z, najmniejszy / 2))
        Assert.Equal(0, PodsumowanieKreatora.PikselZalecany(z, najmniejszy))
    End Sub

    <Fact>
    Public Sub OpisyWyniku_RozmiarISkale()
        Assert.Equal("1 KB", PodsumowanieKreatora.OpisRozmiaru(10))
        Assert.Equal((2.5).ToString("0.0") & " MB", PodsumowanieKreatora.OpisRozmiaru(CLng(2.5 * 1024 * 1024)))
        Dim plan As New List(Of PlanStopnia) From {
            New PlanStopnia With {.Stopien = New StopienSkali With {.Opis = "1:25 000"}, .Piksel = 2, .PoziomMin = 14, .PoziomMax = 16},
            New PlanStopnia With {.Stopien = New StopienSkali With {.Opis = "1:100 000"}, .Piksel = 8, .PoziomMin = 8, .PoziomMax = 13}}
        Assert.Equal("Skale: 2 · Powiększenia: 8–16", PodsumowanieKreatora.OpisSkalWyniku(plan))
        Assert.Equal("1:25 000 (14–16)" & vbCrLf & "1:100 000 (8–13)", PodsumowanieKreatora.WykazSkalWyniku(plan))
        Assert.StartsWith("1:25 000: piksel 2 m, powiększenia 14-16", PodsumowanieKreatora.OpisPlanu(plan))
        Assert.Equal("Ortofotomapa przy zbliżeniu (zoom 17)", PodsumowanieKreatora.OpisOrtofotomapy(New PlanStopnia With {.PoziomMin = 17, .PoziomMax = 17}))
        Assert.Equal("Ortofotomapa przy zbliżeniu (zoom 16-18)", PodsumowanieKreatora.OpisOrtofotomapy(New PlanStopnia With {.PoziomMin = 16, .PoziomMax = 18}))
        Assert.Equal("Ortofotomapa przy zbliżeniu", PodsumowanieKreatora.OpisOrtofotomapy(Nothing))
        Assert.StartsWith("m/px  -  jak wydruk w skali 1:", PodsumowanieKreatora.OpisPiksela(1))
    End Sub

    <Fact>
    Public Sub PomiarPostepu_PozostalyCzasZPredkosci()
        Dim t = 0.0
        Dim pomiar As New PomiarPostepu(1000, Function() t)
        Assert.Contains("szacowanie", pomiar.Opis(100))
        t = 10
        ' 200 segmentów w 10 s od pierwszego odczytu (100 już było) - 20 segm./s, zostało 700 segmentów = 35 s
        Dim opis = pomiar.Opis(300)
        Assert.StartsWith(" · pozostało ok. 35 s (", opis)
        Assert.Equal(10, pomiar.Uplynelo)
    End Sub

#End Region

#Region "Wyszukiwarka i obszar"

    <Fact>
    Public Sub Zapytanie_WspolrzedneDzialkaMiejsce()
        Dim wsp = ObszarKreatora.RozpoznajZapytanie(" 50.0614, 19.9366 ")
        Assert.Equal(RodzajZapytania.Wspolrzedne, wsp.Rodzaj)
        Assert.Equal("Punkt 50.06140 19.93660", wsp.Punkt.Nazwa)
        Assert.Equal("wspolrzedne", wsp.Punkt.Typ)
        Assert.EndsWith(": 50.0614, 19.9366", wsp.Punkt.PelnaNazwa)

        Dim dz = ObszarKreatora.RozpoznajZapytanie("działka Wieliczka 123/4")
        Assert.Equal(RodzajZapytania.Dzialka, dz.Rodzaj)
        Assert.Equal("Wieliczka 123/4", dz.Tekst)
        Assert.Null(dz.Punkt)

        Dim m = ObszarKreatora.RozpoznajZapytanie("Długa 12/4, Kraków")
        Assert.Equal(RodzajZapytania.Miejsce, m.Rodzaj)
        Assert.Equal("Długa 12/4, Kraków", m.Tekst)
    End Sub

    <Fact>
    Public Sub ZasiegWyniku_PromienGraniceDzialka()
        Dim punkt As New WynikGeokodowania With {.Szerokosc = 50.0614, .Dlugosc = 19.9366}
        Assert.Equal(4000, ObszarKreatora.ZasiegWyniku(punkt, "2 km").Szerokosc, 3)
        ' „wg granic” bez prostokąta wyszukiwarki - promień domyślny 5 km
        Assert.Equal(10000, ObszarKreatora.ZasiegWyniku(punkt, ObszarKreatora.OpcjaGranic).Szerokosc, 3)
        Assert.Equal(10000, ObszarKreatora.ZasiegWyniku(punkt, Nothing).Szerokosc, 3)

        Dim miasto As New WynikGeokodowania With {.Szerokosc = 49.3, .Dlugosc = 19.95, .MinSzerokosc = 49.25, .MaxSzerokosc = 49.33, .MinDlugosc = 19.88, .MaxDlugosc = 20.05}
        Dim granice = ObszarKreatora.ZasiegWyniku(miasto, ObszarKreatora.OpcjaGranic)
        Assert.Equal(ObszarKreatora.ZasiegZGranic(49.25, 49.33, 19.88, 20.05), granice)

        Dim d As New DzialkaKatastralna With {.Numer = "123/4", .Obreb = "Wieliczka", .Gmina = "Wieliczka", .Identyfikator = "121905_4.0001.123/4",
                                              .Zasieg1992 = New Zasieg(240000, 570000, 240050, 570030)}
        Dim wynik = WynikDzialki.Z(d)
        Assert.Equal("Działka 123/4", wynik.Nazwa)
        Assert.Equal("Działka 123/4, obręb Wieliczka, gm. Wieliczka, 121905_4.0001.123/4", wynik.PelnaNazwa)
        Dim obszar = ObszarKreatora.ZasiegWyniku(wynik, "2 km")
        Assert.Equal(30 + 2 * ObszarKreatora.BuforDzialki, obszar.Szerokosc, 3)
        Assert.Equal(50 + 2 * ObszarKreatora.BuforDzialki, obszar.Wysokosc, 3)
    End Sub

    <Fact>
    Public Sub Lokalizacja_BezMiejscowosciKoduIPolski()
        Dim m As New WynikGeokodowania With {.PelnaNazwa = "Zakopane, powiat tatrzański, województwo małopolskie, 34-500, Polska"}
        Assert.Equal("powiat tatrzański, województwo małopolskie", ObszarKreatora.SformatujLokalizacje(m))
        ' sama miejscowość - pełna nazwa bez zmian
        Assert.Equal("Zakopane, Polska", ObszarKreatora.SformatujLokalizacje(New WynikGeokodowania With {.PelnaNazwa = "Zakopane, Polska"}))
        Assert.Equal("", ObszarKreatora.SformatujLokalizacje(Nothing))
    End Sub

    <Fact>
    Public Sub Wymiary_MetryAlboKilometry()
        Assert.Equal("brak", ObszarKreatora.FormatujWymiary(New Zasieg()))
        Dim maly = ObszarKreatora.FormatujWymiary(New Zasieg(0, 0, 70, 100))
        Assert.StartsWith("100 m × 70 m (", maly)
        Assert.EndsWith(" m²)", maly)
        Assert.EndsWith(" km²)", ObszarKreatora.FormatujWymiary(New Zasieg(0, 0, 2000, 3000)))
        Assert.Equal(52.0, ObszarKreatora.SzerokoscGeo(New Zasieg()))
        Assert.InRange(ObszarKreatora.SzerokoscGeo(New Zasieg(500000, 600000, 502000, 602000)), 49.0, 55.0)
    End Sub

    <Fact>
    Public Sub OpisObszaru_PrzykladTrasaProstokat()
        Dim z As New Zasieg(500000, 600000, 502000, 603000)
        Assert.StartsWith("Nie wybrano jeszcze obszaru.", ObszarKreatora.OpisObszaru(z, True, Nothing).Item1)
        Dim zwykly = ObszarKreatora.OpisObszaru(z, False, Nothing)
        Assert.StartsWith("Wymiary: 3 km × 2 km", zwykly.Item1)
        Assert.EndsWith("Układ: PL-1992 (EPSG:2180)", zwykly.Item2)
        Dim k As New Korytarz With {.SzerokoscMetry = 500}
        k.DodajLinie(New List(Of PunktGeo) From {New PunktGeo(50.0, 20.0), New PunktGeo(50.01, 20.01)})
        Assert.StartsWith("Korytarz trasy: ", ObszarKreatora.OpisObszaru(z, False, k).Item1)
        Assert.StartsWith("Ślad: 2 punktów w 1 liniach", ObszarKreatora.OpisObszaru(z, False, k).Item2)
    End Sub

    <Theory>
    <InlineData(10000.0, 4000.0)>
    <InlineData(3000.0, 25000.0)>
    <InlineData(800.0, 600.0)>
    Public Sub PikselPodgladu_CalyObszarWRamce(szerokosc As Double, wysokosc As Double)
        Dim z As New Zasieg(500000, 600000, 500000 + wysokosc, 600000 + szerokosc)
        Dim piksel = ObszarKreatora.PikselPodgladu(z, 240, 210, 1.0, 1.0)
        Assert.True(z.Szerokosc / piksel <= 240 - 24)
        Assert.True(z.Wysokosc / piksel <= 210 - 24)
        Assert.True(Math.Max(z.Szerokosc / piksel / 216, z.Wysokosc / piksel / 186) > 0.99 OrElse piksel = 1.0)
        ' powiększenie podglądu zmniejsza piksel
        Assert.Equal(piksel / 2, ObszarKreatora.PikselPodgladu(z, 240, 210, 1.0, 2.0), 9)
    End Sub

#End Region

End Class
