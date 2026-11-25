Option Strict On
Option Explicit On
Option Infer On

Imports System.Collections.Generic
Imports System.IO
Imports MapoTero.Core
Imports Xunit

Public Class PrzekazZestawuTesty

    Private Shared Function UtworzTestoweZrodla() As Dictionary(Of String, PlikWarstw)
        Dim orto As New PlikWarstw With {
            .Adres = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolution",
            .Typ = TypUslugiMapowej.Wms,
            .MaksRozmiarSegmentu = 2000
        }
        orto.Warstwy.Add(New WarstwaMapy With {.Nazwa = "Raster", .RozmiarPiksela = "0.25"})

        Dim nmt As New PlikWarstw With {
            .Adres = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/WMS/ShadedRelief",
            .Typ = TypUslugiMapowej.Wms
        }
        nmt.Warstwy.Add(New WarstwaMapy With {.Nazwa = "Raster", .RozmiarPiksela = "1"})

        Dim szlaki As New PlikWarstw With {
            .Adres = "https://tile.waymarkedtrails.org/hiking/{z}/{x}/{y}.png",
            .Typ = TypUslugiMapowej.Xyz,
            .Nakladka = True
        }
        szlaki.Warstwy.Add(New WarstwaMapy With {.Nazwa = "hiking", .RozmiarPiksela = "2"})

        Return New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase) From {
            {"ORTOFOTOMAPA", orto},
            {"NMT_CIENIOWANIE", nmt},
            {"waymarkedtrails", szlaki}
        }
    End Function

    <Fact>
    Public Sub CzyWygladaJakZestawGalerii_RozpoznajePrzepisIOdrzucaGeoJson()
        Dim przepis = "{""format"":""mapotero-zestaw"",""warstwy"":[{""zbior"":""ORTO"",""warstwa"":""Raster""}]}"
        Dim geojson = "{""type"":""FeatureCollection"",""features"":[]}"
        Assert.True(PrzekazZestawu.CzyWygladaJakZestawGalerii(przepis))
        Assert.False(PrzekazZestawu.CzyWygladaJakZestawGalerii(geojson))
        Assert.False(PrzekazZestawu.CzyWygladaJakZestawGalerii("nie json"))
    End Sub

    <Fact>
    Public Sub ZbudujZestaw_ZachowujeKolejnoscKrycieITrybyMieszania()
        Dim json = "{
            ""format"": ""mapotero-zestaw"",
            ""wersja"": 1,
            ""nazwa"": ""Orto z rzeźbą i szlakami"",
            ""cel"": ""okno_glowne"",
            ""zapiszStyl"": true,
            ""obszarWgs84"": [19.9, 49.25, 20.0, 49.3],
            ""warstwy"": [
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100, ""tryb"": ""Zwykly"" },
                { ""zbior"": ""NMT_CIENIOWANIE"", ""warstwa"": ""Raster"", ""krycie"": 65, ""tryb"": ""RzezbaTerenu"" },
                { ""zbior"": ""waymarkedtrails"", ""warstwa"": ""hiking"", ""krycie"": 85, ""tryb"": ""Mnozenie"" }
            ]
        }"

        Dim dane = PrzekazZestawu.ZTekstu(json)
        Assert.NotNull(dane.ObszarWgs84)
        Assert.Equal(4, dane.ObszarWgs84.Length)
        Dim wynik = PrzekazZestawu.ZbudujZestaw(dane, UtworzTestoweZrodla())

        Assert.Equal(3, wynik.Zestaw.Liczba)
        Assert.Empty(wynik.Pominiete)
        Assert.True(wynik.ZapiszStyl)
        Assert.Equal("Orto z rzeźbą i szlakami", wynik.Nazwa)

        Assert.Equal("ORTOFOTOMAPA", wynik.Zestaw(0).Zbior)
        Assert.Equal(100, wynik.Zestaw(0).Krycie)
        Assert.Equal(TrybMieszania.Zwykly, wynik.Zestaw(0).TrybMieszania)
        Assert.Equal(2000, wynik.Zestaw(0).MaksRozmiarSegmentu)

        Assert.Equal("NMT_CIENIOWANIE", wynik.Zestaw(1).Zbior)
        Assert.Equal(65, wynik.Zestaw(1).Krycie)
        Assert.Equal(TrybMieszania.RzezbaTerenu, wynik.Zestaw(1).TrybMieszania)

        Assert.Equal("waymarkedtrails", wynik.Zestaw(2).Zbior)
        Assert.Equal(85, wynik.Zestaw(2).Krycie)
        Assert.Equal(TrybMieszania.Mnozenie, wynik.Zestaw(2).TrybMieszania)
        Assert.True(wynik.Zestaw(2).Nakladka)
    End Sub

    <Fact>
    Public Sub ZbudujZestaw_OgraniczaKrycieIPomijaBrakujaceWarstwy()
        Dim json = "{
            ""format"": ""mapotero-zestaw"",
            ""warstwy"": [
                { ""zbior"": ""NIEISTNIEJACY"", ""warstwa"": ""X"", ""krycie"": 50 },
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 1, ""tryb"": ""7 Rzeźba terenu"" }
            ]
        }"

        Dim wynik = PrzekazZestawu.ZbudujZestaw(PrzekazZestawu.ZTekstu(json), UtworzTestoweZrodla())
        Assert.Equal(1, wynik.Zestaw.Liczba)
        Assert.Equal(ZestawWarstw.MinKrycie, wynik.Zestaw(0).Krycie)
        Assert.Equal(TrybMieszania.RzezbaTerenu, wynik.Zestaw(0).TrybMieszania)
        Assert.Single(wynik.Pominiete)
    End Sub

    <Fact>
    Public Sub ZbudujZestaw_PrzyPrzekroczeniuLimitu12Warstw_PrzycinaIRaportuje()
        Dim zrodla = UtworzTestoweZrodla()
        Dim mega As New PlikWarstw With {.Adres = "https://example.com/wms", .Typ = TypUslugiMapowej.Wms}
        Dim dane As New DaneZestawuGalerii()
        For i = 1 To 14
            mega.Warstwy.Add(New WarstwaMapy With {.Nazwa = "W" & i, .RozmiarPiksela = "1"})
            dane.Warstwy.Add(New PozycjaZestawuGalerii With {.Zbior = "MEGA", .Warstwa = "W" & i, .Krycie = 100})
        Next
        zrodla("MEGA") = mega

        Dim wynik = PrzekazZestawu.ZbudujZestaw(dane, zrodla)
        Assert.Equal(ZestawWarstw.MaksLiczba, wynik.Zestaw.Liczba)
        Assert.True(wynik.PrzycietoDoLimitu)
        Assert.Equal(2, wynik.Pominiete.Count)
    End Sub

    <Fact>
    Public Sub UtworzStylSzablonu_TworzyPoprawnyWlasnyStylIZapisujeDoJson()
        Dim dane = PrzekazZestawu.ZTekstu("{
            ""format"": ""mapotero-zestaw"",
            ""warstwy"": [
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100, ""tryb"": ""Zwykly"" },
                { ""zbior"": ""NMT_CIENIOWANIE"", ""warstwa"": ""Raster"", ""krycie"": 70, ""tryb"": ""RzezbaTerenu"" },
                { ""zbior"": ""waymarkedtrails"", ""warstwa"": ""hiking"", ""krycie"": 100, ""tryb"": ""Zwykly"" }
            ]
        }")
        Dim wynik = PrzekazZestawu.ZbudujZestaw(dane, UtworzTestoweZrodla())
        Dim styl = PrzekazZestawu.UtworzStylSzablonu(wynik.Zestaw, "Mój testowy miks", 50.06, 19.94)

        Assert.NotNull(styl)
        Assert.True(SzablonyWlasne.CzyWlasny(styl))
        Assert.Equal(3, styl.Warstwy.Count)
        Assert.Equal(RodzajRegulacji.Cieniowanie, styl.Warstwy(1).Regulacja)
        Assert.Equal(RodzajRegulacji.Szlaki, styl.Warstwy(2).Regulacja)
        Assert.Equal(0.25, styl.DomyslnyPiksel)

        Dim tmp = Path.Combine(Path.GetTempPath(), "test_styl_" & Path.GetRandomFileName() & ".json")
        Try
            SzablonyWlasne.Zapisz(tmp, {styl})
            Dim wczytane = SzablonyWlasne.Wczytaj(tmp)
            Assert.Single(wczytane)
            Assert.Equal("Mój testowy miks", wczytane(0).Nazwa)
            Assert.Equal(TrybMieszania.RzezbaTerenu, wczytane(0).Warstwy(1).TrybMieszania)
        Finally
            If File.Exists(tmp) Then File.Delete(tmp)
        End Try
    End Sub

    Private Shared Function UtworzZrodlaWieloskalowe() As Dictionary(Of String, PlikWarstw)
        Dim zrodla = UtworzTestoweZrodla()
        Dim topo As New PlikWarstw With {.Adres = "https://example.com/topo", .Typ = TypUslugiMapowej.Wmts}
        topo.Warstwy.Add(New WarstwaMapy With {.Nazwa = "MAPA_TOPOGRAFICZNA", .RozmiarPiksela = "2"})
        zrodla("TOPO_WMTS") = topo

        Dim bdoo As New PlikWarstw With {.Adres = "https://example.com/bdoo", .Typ = TypUslugiMapowej.Wmts}
        bdoo.Warstwy.Add(New WarstwaMapy With {.Nazwa = "BDOO", .RozmiarPiksela = "15"})
        zrodla("BDOO_WMTS") = bdoo

        Dim dzialki As New PlikWarstw With {.Adres = "https://example.com/egib", .Typ = TypUslugiMapowej.Wms, .Nakladka = True}
        dzialki.Warstwy.Add(New WarstwaMapy With {.Nazwa = "dzialki", .RozmiarPiksela = "0.5"})
        zrodla("dzialki_ewid_EGIB") = dzialki

        Return zrodla
    End Function

    <Fact>
    Public Sub WykryjRozpietoscSkal_RozpoznajeDuzaRozpietoscINieAlarmujeDlaJednoskalowej()
        Dim zrodla = UtworzZrodlaWieloskalowe()

        Dim jednoSkalowy = PrzekazZestawu.ZbudujZestaw(PrzekazZestawu.ZTekstu("{
            ""format"": ""mapotero-zestaw"",
            ""warstwy"": [
                { ""zbior"": ""TOPO_WMTS"", ""warstwa"": ""MAPA_TOPOGRAFICZNA"", ""krycie"": 100 },
                { ""zbior"": ""NMT_CIENIOWANIE"", ""warstwa"": ""Raster"", ""krycie"": 70, ""tryb"": ""RzezbaTerenu"" },
                { ""zbior"": ""waymarkedtrails"", ""warstwa"": ""hiking"", ""krycie"": 100 }
            ]
        }"), zrodla).Zestaw
        Assert.False(PrzekazZestawu.WykryjRozpietoscSkal(jednoSkalowy))

        Dim wieloSkalowy = PrzekazZestawu.ZbudujZestaw(PrzekazZestawu.ZTekstu("{
            ""format"": ""mapotero-zestaw"",
            ""warstwy"": [
                { ""zbior"": ""BDOO_WMTS"", ""warstwa"": ""BDOO"", ""krycie"": 100 },
                { ""zbior"": ""TOPO_WMTS"", ""warstwa"": ""MAPA_TOPOGRAFICZNA"", ""krycie"": 100 },
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100 }
            ]
        }"), zrodla).Zestaw
        Assert.True(PrzekazZestawu.WykryjRozpietoscSkal(wieloSkalowy))
    End Sub

    <Fact>
    Public Sub UtworzStylSzablonu_ZRozdzieleniemSkal_BudujePoprawnyPlanWieloskalowy()
        Dim zrodla = UtworzZrodlaWieloskalowe()
        Dim zestaw = PrzekazZestawu.ZbudujZestaw(PrzekazZestawu.ZTekstu("{
            ""format"": ""mapotero-zestaw"",
            ""warstwy"": [
                { ""zbior"": ""BDOO_WMTS"", ""warstwa"": ""BDOO"", ""krycie"": 100 },
                { ""zbior"": ""TOPO_WMTS"", ""warstwa"": ""MAPA_TOPOGRAFICZNA"", ""krycie"": 100 },
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100 },
                { ""zbior"": ""NMT_CIENIOWANIE"", ""warstwa"": ""Raster"", ""krycie"": 65, ""tryb"": ""RzezbaTerenu"" },
                { ""zbior"": ""dzialki_ewid_EGIB"", ""warstwa"": ""dzialki"", ""krycie"": 90 },
                { ""zbior"": ""waymarkedtrails"", ""warstwa"": ""hiking"", ""krycie"": 85, ""tryb"": ""Mnozenie"" }
            ]
        }"), zrodla).Zestaw

        Dim styl = PrzekazZestawu.UtworzStylSzablonu(zestaw, "Miks wieloskalowy", 50.06, 19.94, rozdzielSkale:=True)
        Assert.True(styl.Wieloskalowy)
        Assert.Equal(3, styl.StopnieSkali.Count)
        Assert.Equal(0.25, styl.StopnieSkali(0).Piksel)
        Assert.Equal(2.0, styl.StopnieSkali(1).Piksel)
        Assert.Equal(15.0, styl.StopnieSkali(2).Piksel)

        ' Działki tylko w najdokładniejszym stopniu (<= 2 m), cieniowanie i szlaki do 10 m, przeglądowe BDOO bez nakładek szczegółowych
        Assert.Contains(styl.StopnieSkali(0).Warstwy, Function(w) w.Zbior = "dzialki_ewid_EGIB")
        Assert.Contains(styl.StopnieSkali(1).Warstwy, Function(w) w.Zbior = "NMT_CIENIOWANIE" AndAlso w.Krycie = 65 AndAlso w.TrybMieszania = TrybMieszania.RzezbaTerenu)
        Assert.Contains(styl.StopnieSkali(1).Warstwy, Function(w) w.Zbior = "waymarkedtrails" AndAlso w.Krycie = 85 AndAlso w.TrybMieszania = TrybMieszania.Mnozenie)
        Assert.DoesNotContain(styl.StopnieSkali(2).Warstwy, Function(w) w.Zbior = "dzialki_ewid_EGIB")

        Dim plan = ZarzadcaSzablonow.PlanStopni(styl, styl.DomyslnyPiksel, 50.06)
        Assert.True(plan.Count >= 2)
        Assert.Equal("", ZarzadcaSzablonow.SprawdzPlan(plan))
    End Sub

    <Fact>
    Public Sub ZSzablonuMapy_IZbudujSzablonWieloskalowy_RoundTripZZoomami()
        Dim zrodla = UtworzZrodlaWieloskalowe()
        Dim styl As New SzablonMapy("styl_uzytkownika_test", "Beskidy test", SzablonyWlasne.KategoriaWlasna, "Opis", "⭐", 0.5, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet)
        Dim st1 = styl.DodajStopien("Orto", 0.5, New WarstwaSzablonu("ORTOFOTOMAPA", "Raster", 100), New WarstwaSzablonu("waymarkedtrails", "hiking", 85, True, TrybMieszania.Mnozenie))
        Dim st2 = styl.DodajStopien("Topo", 2.0, New WarstwaSzablonu("TOPO_WMTS", "MAPA_TOPOGRAFICZNA", 100), New WarstwaSzablonu("NMT_CIENIOWANIE", "Raster", 70, False, TrybMieszania.RzezbaTerenu))
        Dim st3 = styl.DodajStopien("Przegląd", 15.0, New WarstwaSzablonu("BDOO_WMTS", "BDOO", 100))

        Dim plan As New List(Of PlanStopnia) From {
            New PlanStopnia With {.Stopien = st1, .Piksel = 0.5, .PoziomMin = 17, .PoziomMax = 19},
            New PlanStopnia With {.Stopien = st2, .Piksel = 2.0, .PoziomMin = 14, .PoziomMax = 16},
            New PlanStopnia With {.Stopien = st3, .Piksel = 15.0, .PoziomMin = 11, .PoziomMax = 13}
        }

        Dim dane = PrzekazZestawu.ZSzablonuMapy(styl, plan)
        Assert.Equal(2, dane.Wersja)
        Assert.Equal("kreator", dane.Cel)
        Assert.Equal(3, dane.Skale.Count)
        Assert.NotEmpty(dane.Warstwy)

        Dim json = PrzekazZestawu.DoTekstu(dane)
        Assert.True(PrzekazZestawu.CzyWygladaJakZestawGalerii(json))

        Dim odczyt = PrzekazZestawu.ZTekstu(json)
        Dim wynik = PrzekazZestawu.ZbudujSzablonWieloskalowy(odczyt, zrodla)

        Assert.NotNull(wynik.Szablon)
        Assert.Empty(wynik.Pominiete)
        Assert.Equal(3, wynik.Szablon.StopnieSkali.Count)
        Assert.NotNull(wynik.PlanWlasny)
        Assert.Equal(3, wynik.PlanWlasny.Count)
        Assert.Equal(17, wynik.PlanWlasny(0).PoziomMin)
        Assert.Equal(19, wynik.PlanWlasny(0).PoziomMax)
        Assert.Equal(11, wynik.PlanWlasny(2).PoziomMin)
        Assert.Equal(13, wynik.PlanWlasny(2).PoziomMax)

        ' Fallback jednowarstwowy dla starszych wersji i okna głównego nadal działa
        Dim fallback = PrzekazZestawu.ZbudujZestaw(odczyt, zrodla)
        Assert.Equal(2, fallback.Zestaw.Liczba)
    End Sub

    <Fact>
    Public Sub ZbudujSzablonWieloskalowy_NakladajaceSieZoomy_OdrzucaPlanWlasnyAleZachowujeSkale()
        Dim zrodla = UtworzZrodlaWieloskalowe()
        Dim json = "{
            ""format"": ""mapotero-zestaw"",
            ""wersja"": 2,
            ""nazwa"": ""Błędne zoomy"",
            ""skale"": [
                { ""opis"": ""Orto"", ""piksel"": 0.5, ""zoomMin"": 15, ""zoomMax"": 18, ""warstwy"": [{ ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100 }] },
                { ""opis"": ""Topo"", ""piksel"": 2.0, ""zoomMin"": 13, ""zoomMax"": 16, ""warstwy"": [{ ""zbior"": ""TOPO_WMTS"", ""warstwa"": ""MAPA_TOPOGRAFICZNA"", ""krycie"": 100 }] }
            ]
        }"

        Dim wynik = PrzekazZestawu.ZbudujSzablonWieloskalowy(PrzekazZestawu.ZTekstu(json), zrodla)
        Assert.NotNull(wynik.Szablon)
        Assert.Equal(2, wynik.Szablon.StopnieSkali.Count)
        Assert.Null(wynik.PlanWlasny)
    End Sub

End Class
