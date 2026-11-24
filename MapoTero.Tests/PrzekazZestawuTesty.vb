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
            ""warstwy"": [
                { ""zbior"": ""ORTOFOTOMAPA"", ""warstwa"": ""Raster"", ""krycie"": 100, ""tryb"": ""Zwykly"" },
                { ""zbior"": ""NMT_CIENIOWANIE"", ""warstwa"": ""Raster"", ""krycie"": 65, ""tryb"": ""RzezbaTerenu"" },
                { ""zbior"": ""waymarkedtrails"", ""warstwa"": ""hiking"", ""krycie"": 85, ""tryb"": ""Mnozenie"" }
            ]
        }"

        Dim dane = PrzekazZestawu.ZTekstu(json)
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

End Class
