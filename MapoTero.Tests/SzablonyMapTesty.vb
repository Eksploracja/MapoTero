'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports MapoTero.Core
Imports Xunit

Public Class SzablonyMapTesty

    <Fact>
    Public Sub ZarzadcaSzablonow_PobierzDomyslne_ZwracaGotoweSzablony()
        Dim szablony = ZarzadcaSzablonow.PobierzDomyslne()
        Assert.NotNull(szablony)
        Assert.True(szablony.Count >= 5, "Powinno być co najmniej 5 predefiniowanych szablonów.")

        For Each s In szablony
            Assert.False(String.IsNullOrWhiteSpace(s.Id), "Szablon musi mieć Id.")
            Assert.False(String.IsNullOrWhiteSpace(s.Nazwa), "Szablon musi mieć nazwę.")
            Assert.False(String.IsNullOrWhiteSpace(s.Opis), "Szablon musi mieć opis.")
            Assert.True(s.DomyslnyPiksel > 0, "Rozmiar piksela musi być większy od 0.")
        Next
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_PobierzDlaCelu_FiltrujeZgodnieZPrzeznaczeniem()
        Dim dlaGarmina = ZarzadcaSzablonow.PobierzDlaCelu(CelMapy.OdbiornikGarmin)
        Assert.Contains(dlaGarmina, Function(s) s.Id = "piesza_gory")
        Assert.Contains(dlaGarmina, Function(s) s.Id = "rower_mtb")

        Dim dlaCad = ZarzadcaSzablonow.PobierzDlaCelu(CelMapy.ProjektCadGis)
        Assert.Contains(dlaCad, Function(s) s.Id = "dzialka_kataster")
        Assert.Contains(dlaCad, Function(s) s.Id = "las_eksploracja")
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_ZastosujSzablon_PoprawnieWypelniaZestawWarstw()
        Dim sPiesza = ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory")
        Assert.NotNull(sPiesza)

        Dim zestaw As New ZestawWarstw()
        Dim zrodla As New Dictionary(Of String, PlikWarstw)()
        zrodla("TOPO_WMTS") = New PlikWarstw With {.Adres = "https://mapy.geoportal.gov.pl/wss/service/WMTS/guest/wmts/TOPO", .Typ = TypUslugiMapowej.Wmts}
        zrodla("numer_mod_terenu_cieniowanie_WMTS") = New PlikWarstw With {.Adres = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/GRID1/WMTS/ShadedRelief", .Typ = TypUslugiMapowej.Wmts}
        zrodla("Waymarked_Trails") = New PlikWarstw With {.Adres = "https://tile.waymarkedtrails.org/hiking/{z}/{x}/{y}.png", .Typ = TypUslugiMapowej.Xyz}

        ZarzadcaSzablonow.ZastosujSzablon(sPiesza, zestaw, zrodla)

        Assert.Equal(3, zestaw.Liczba)
        Assert.Equal("MAPA TOPOGRAFICZNA", zestaw(0).Nazwa)
        Assert.Equal(100, zestaw(0).Krycie)

        Assert.Equal("ISOK_Cien", zestaw(1).Nazwa)
        Assert.Equal(70, zestaw(1).Krycie)
        Assert.Equal(TrybMieszania.RzezbaTerenu, zestaw(1).TrybMieszania)
        Assert.True(zestaw(1).Nakladka)

        Assert.Equal("hiking", zestaw(2).Nazwa)
        Assert.Equal(100, zestaw(2).Krycie)
        Assert.True(zestaw(2).Nakladka)
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_SzacujParametry_ZwracaDokladneSzacunki()
        ' Obszar 10 km x 10 km w PL-1992
        Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
        ' Piksel 1 m, bok segmentu 1000 px -> siatka 10 x 10 = 100 segmentów
        Dim szacunek = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000)

        Assert.Equal(100, szacunek.LiczbaSegmentow)
        Assert.True(szacunek.SzacowanyRozmiarMb > 0)
        Assert.True(szacunek.SzacowanyCzasSekundy > 0)
        Assert.Equal(0, szacunek.PoziomOstrzezenia) ' do 120 segmentów = zielony (bezpieczny)
        Assert.Contains("s", szacunek.SformatowanyCzas())
        Assert.Contains("MB", szacunek.SformatowanyRozmiar())
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_DopasujFormatDocelowy_WybieraWlasciwyFormat()
        ' Wariant zawsze decyduje o formacie (dawniej ignorowany dla telefonu, wydruku i CAD)
        Assert.Equal(RodzajEksportu.MBTiles, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.TelefonTablet, wielopoziomowy:=True))
        Assert.Equal(RodzajEksportu.SQLiteDB, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.TelefonTablet, wielopoziomowy:=False))

        ' Garmin: KMZ niezależnie od liczby kafli - bez cichej zamiany na JNX powyżej 100 kafli
        Assert.Equal(RodzajEksportu.KmzGarmin, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.OdbiornikGarmin, wielopoziomowy:=False))
        Assert.Equal(RodzajEksportu.Jnx, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.OdbiornikGarmin, wielopoziomowy:=True))

        ' CAD / Druk: GeoTIFF albo MBTiles
        Assert.Equal(RodzajEksportu.GeoTiff, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.ProjektCadGis, wielopoziomowy:=False))
        Assert.Equal(RodzajEksportu.GeoTiff, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.WydrukKomputer, wielopoziomowy:=False))
        Assert.Equal(RodzajEksportu.MBTiles, ZarzadcaSzablonow.DopasujFormatDocelowy(CelMapy.WydrukKomputer, wielopoziomowy:=True))
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_RozszerzenieDlaRodzaju_ZwracaPoprawneRozszerzenia()
        Assert.Equal(".mbtiles", ZarzadcaSzablonow.RozszerzenieDlaRodzaju(RodzajEksportu.MBTiles))
        Assert.Equal(".kmz", ZarzadcaSzablonow.RozszerzenieDlaRodzaju(RodzajEksportu.KmzGarmin))
        Assert.Equal(".jnx", ZarzadcaSzablonow.RozszerzenieDlaRodzaju(RodzajEksportu.Jnx))
        Assert.Equal(".tif", ZarzadcaSzablonow.RozszerzenieDlaRodzaju(RodzajEksportu.GeoTiff))
    End Sub

    <Fact>
    Public Sub ZarzadcaSzablonow_SzacujParametry_DlaKorytarzaIWieluFormatow()
        Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
        Dim uklad = UkladWspolrzednych.PL1992

        ' Utwórz prosty korytarz ukośny przez obszar
        Dim p1 = uklad.DoWgs84(500000, 600000)
        Dim p2 = uklad.DoWgs84(510000, 610000)
        Dim k As New Korytarz()
        k.DodajLinie({p1, p2})
        k.SzerokoscMetry = 500

        ' Pełny prostokąt: 10 x 10 = 100 segmentów
        Dim szacunekKorytarza = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000, k, uklad, RodzajEksportu.MBTiles, True)

        Assert.True(szacunekKorytarza.CzyKorytarz)
        Assert.Equal(100, szacunekKorytarza.LiczbaSegmentowPelnych)
        Assert.True(szacunekKorytarza.LiczbaSegmentow < 100, "Korytarz powinien mieć mniej segmentów niż pełny prostokąt.")
        Assert.True(szacunekKorytarza.OszczednoscProcent > 0, "Oszczędność powinna być dodatnia.")
        Assert.True(szacunekKorytarza.SzacowanyRozmiarMbMin > 0)
        Assert.True(szacunekKorytarza.SzacowanyRozmiarMbMax >= szacunekKorytarza.SzacowanyRozmiarMbMin)
        Assert.Contains("~", szacunekKorytarza.SformatowanyPrzedzialRozmiaru())

        ' KMZ dla Garmina
        Dim szacunekKmz = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000, Nothing, uklad, RodzajEksportu.KmzGarmin, False)
        Assert.False(szacunekKmz.CzyKorytarz)
        Assert.Equal(100, szacunekKmz.LiczbaSegmentow)
        Assert.Contains("~", szacunekKmz.SformatowanyPrzedzialRozmiaru())
    End Sub

    <Fact>
    Public Sub ZalecanyZakresPowiekszen_ZaleznyOdPiksela()
        ' 0,25 m w Polsce - poziom 19 (dawniej stałe 10-16), 4 m - poziom 15
        Assert.Equal(19, ZarzadcaSzablonow.ZalecanyZakresPowiekszen(0.25, 52).Item2)
        Assert.Equal(15, ZarzadcaSzablonow.ZalecanyZakresPowiekszen(4.0, 52).Item2)
        Dim z = ZarzadcaSzablonow.ZalecanyZakresPowiekszen(1.0, 50)
        Assert.True(z.Item1 < z.Item2)
    End Sub

    <Fact>
    Public Sub ZnajdzWarstweCieniowania_PomijaPodklad()
        Dim zestaw As New ZestawWarstw()
        zestaw.Dodaj("HipsoCien")
        zestaw.Dodaj("szczyty", nakladka:=True)
        Assert.Null(ZarzadcaSzablonow.ZnajdzWarstweCieniowania(zestaw))
        zestaw.Dodaj("ISOK_Cien", krycie:=40, nakladka:=True)
        Assert.Equal("ISOK_Cien", ZarzadcaSzablonow.ZnajdzWarstweCieniowania(zestaw).Nazwa)
    End Sub

    Private Shared ReadOnly FolderWarstwTestow As String =
        IO.Path.GetFullPath(IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MapoTero_SingleFile", "warstwy"))

    <Fact>
    Public Sub Szablony_WarstwyIstniejaWZbiorachMap()
        For Each s In ZarzadcaSzablonow.PobierzDomyslne()
            For Each w In s.Warstwy.Concat(s.StopnieSkali.SelectMany(Function(st) st.Warstwy))
                Dim plik = IO.Path.Combine(FolderWarstwTestow, w.Zbior & ".txt")
                Assert.True(IO.File.Exists(plik), s.Id & ": brak zbioru " & w.Zbior)
                Assert.NotNull(PlikWarstw.Wczytaj(plik).Znajdz(w.Warstwa))
            Next
            ' Punkt próbki podglądu w Polsce
            Assert.InRange(s.ProbkaLat, 49.0, 54.9)
            Assert.InRange(s.ProbkaLon, 14.1, 24.2)
        Next
    End Sub

    <Fact>
    Public Sub Szablony_PikselNieMniejszyNizPrzydatny()
        Dim zrodla As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        For Each s In ZarzadcaSzablonow.PobierzDomyslne()
            If s.Warstwy.Count = 0 Then Continue For
            For Each w In s.Warstwy
                If Not zrodla.ContainsKey(w.Zbior) Then zrodla(w.Zbior) = PlikWarstw.Wczytaj(IO.Path.Combine(FolderWarstwTestow, w.Zbior & ".txt"))
            Next
            Dim zestaw = s.UtworzZestaw(zrodla)
            Assert.True(s.DomyslnyPiksel >= ZarzadcaSzablonow.NajmniejszyPrzydatnyPiksel(zestaw) - 0.0001, s.Id)
        Next
    End Sub

    <Fact>
    Public Sub Szacunek_Garmin_KmzPonadLimit_WymagaPotwierdzenia()
        Dim zasieg As New Zasieg(500000, 600000, 520000, 620000)
        Dim s = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000, Nothing, UkladWspolrzednych.PL1992, RodzajEksportu.KmzGarmin, False)
        Assert.True(s.LiczbaSegmentow > ZarzadcaSzablonow.LimitKafliGarmin)
        Assert.True(s.WymagaPotwierdzenia)
        Assert.Contains("JNX", s.Ostrzezenie)
    End Sub

    <Fact>
    Public Sub Szacunek_CzasRosnieZLiczbaZrodel()
        Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
        Dim jedno = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000, liczbaZrodel:=1)
        Dim trzy = ZarzadcaSzablonow.SzacujParametry(zasieg, 1.0, 1000, liczbaZrodel:=3)
        Assert.True(trzy.SzacowanyCzasSekundy > jedno.SzacowanyCzasSekundy)
    End Sub

    <Fact>
    Public Sub Odmiana_PolskieFormyLiczebnika()
        Assert.Equal("segment", ZarzadcaSzablonow.Odmiana(1, "segment", "segmenty", "segmentów"))
        Assert.Equal("segmenty", ZarzadcaSzablonow.Odmiana(3, "segment", "segmenty", "segmentów"))
        Assert.Equal("segmentów", ZarzadcaSzablonow.Odmiana(12, "segment", "segmenty", "segmentów"))
        Assert.Equal("segmenty", ZarzadcaSzablonow.Odmiana(22, "segment", "segmenty", "segmentów"))
        Assert.Equal("segmentów", ZarzadcaSzablonow.Odmiana(25, "segment", "segmenty", "segmentów"))
    End Sub

    <Fact>
    Public Sub PlanStopni_TopoPrzelaczaSkalePoPoziomach()
        ' TOPO: 2 m (1:10 000), 10 m (1:50 000), 25 m (1:100 000), 60 m (przeglądowa) na 50° N
        Dim plan = ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"), 2.0, 50.0)
        Assert.Equal(4, plan.Count)
        Assert.Equal({15, 13, 12, 8}, plan.Select(Function(ps) ps.PoziomMin).ToArray())
        Assert.Equal({16, 14, 12, 11}, plan.Select(Function(ps) ps.PoziomMax).ToArray())
        ' poziomy ciągłe, bez luk i nakładania
        For i = 1 To plan.Count - 1
            Assert.Equal(plan(i - 1).PoziomMin - 1, plan(i).PoziomMax)
        Next
    End Sub

    <Fact>
    Public Sub PlanStopni_PikselUzytkownikaPomijaZbyteczneStopnie()
        ' Piksel 8 m: stopień 10 m za blisko - zostaje 8 m, 25 m i 60 m
        Dim plan = ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"), 8.0, 50.0)
        Assert.Equal(8.0, plan(0).Piksel)
        Assert.DoesNotContain(plan, Function(ps) ps.Piksel = 10.0)
        Assert.Empty(ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("dzialka_kataster"), 0.25, 50.0))
    End Sub

    <Fact>
    Public Sub StopienDlaPiksela_WybieraNajblizszaSkale()
        Dim s = ZarzadcaSzablonow.ZnajdzSzablon("historia_wig")
        Assert.True(s.Wieloskalowy)
        Assert.Equal("1:25 000", s.StopienDlaPiksela(1.5).Opis)
        Assert.Equal("1:100 000", s.StopienDlaPiksela(6.0).Opis)
        Assert.Equal("1:300 000", s.StopienDlaPiksela(40.0).Opis)
        ' W 1:25 000 pod mapą szczegółową leży 1:100 000 (wypełnia miejsca bez arkuszy 1:25 000)
        Assert.Equal("wig100k_2180", s.StopienDlaPiksela(2.0).Warstwy(0).Warstwa)
    End Sub

    <Fact>
    Public Sub SzacujWieloskalowo_SumujeStopnie()
        Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
        Dim plan = ZarzadcaSzablonow.PlanStopni(ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory"), 2.0, 52.0)
        Dim jedno = ZarzadcaSzablonow.SzacujParametry(zasieg, 2.0)
        Dim wiele = ZarzadcaSzablonow.SzacujWieloskalowo(plan, zasieg)
        Assert.True(wiele.LiczbaSegmentow > jedno.LiczbaSegmentow)
        Assert.True(wiele.LiczbaSegmentow < jedno.LiczbaSegmentow * 1.2, "stopnie mniej dokładne to mały dodatek")
    End Sub

    <Fact>
    Public Sub TrybRzezbyTerenu_PlaskiTerenBezZmianCienCiemniejszy()
        Dim plaski = (255 << 24) Or (180 << 16) Or (180 << 8) Or 180
        Dim cien = (255 << 24) Or (60 << 16) Or (60 << 8) Or 60
        Dim mapa = (255 << 24) Or (200 << 16) Or (230 << 8) Or 150
        Dim cel = {mapa, mapa}
        NakladanieWarstw.Naloz(cel, {plaski, cien}, 100, TrybMieszania.RzezbaTerenu)
        Assert.Equal(mapa, cel(0))
        Assert.True(((cel(1) >> 8) And &HFF) < 100, "stok w cieniu wyraźnie ciemniejszy")
        ' Mnożenie dla porównania przyciemnia także płaski teren (szara zasłona)
        Dim celMn = {mapa}
        NakladanieWarstw.Naloz(celMn, {plaski}, 100, TrybMieszania.Mnozenie)
        Assert.True(((celMn(0) >> 8) And &HFF) < 180)
    End Sub

    <Fact>
    Public Sub TrybRzezbyTerenu_ZapisIOdczyt()
        Assert.Equal(TrybMieszania.RzezbaTerenu, TrybMieszaniaRozszerzenia.ZTekstu(TrybMieszaniaRozszerzenia.NazwaWyswietlana(TrybMieszania.RzezbaTerenu)))
        Assert.Equal(TrybMieszania.RzezbaTerenu, ZestawWarstw.TrybyMieszaniaZTekstu("0;6", 2)(1))
        Assert.Contains("7 Rzeźba terenu", TrybMieszaniaRozszerzenia.WszystkieNazwy)
    End Sub

End Class
