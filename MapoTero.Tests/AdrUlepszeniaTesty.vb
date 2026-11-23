'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class AdrUlepszeniaTesty

#Region "U-01 .. U-04: Warstwy użytkownika i filtr plików"

    <Fact>
    Public Sub CzyWygladaNaZbiorMap_OdrzucaNotatkiISmieci()
        ' Zwykłe notatki tekstowe nie powinny być traktowane jako definicje zbioru map
        Assert.False(PlikWarstw.CzyWygladaNaZbiorMap("C:\nieistniejacy_plik_12345.txt"))

        Dim tempPlik = Path.GetTempFileName()
        Try
            ' 1. Notatki użytkownika
            File.WriteAllText(tempPlik, "To jest lista zakupów:" & vbCrLf & "1. chleb" & vbCrLf & "2. masło", Encoding.UTF8)
            Assert.False(PlikWarstw.CzyWygladaNaZbiorMap(tempPlik))

            ' 2. Poprawny format tekstowy zbioru map (adres URL na początku)
            File.WriteAllText(tempPlik, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/Standard?request=GetMap" & vbCrLf & "Raster" & vbCrLf & "1.0", Encoding.UTF8)
            Assert.True(PlikWarstw.CzyWygladaNaZbiorMap(tempPlik))

            ' 3. Poprawny format JSON zbioru map
            File.WriteAllText(tempPlik, "{ ""adres"": ""https://tile.openstreetmap.org/{z}/{x}/{y}.png"", ""typ"": ""xyz"" }", Encoding.UTF8)
            Assert.True(PlikWarstw.CzyWygladaNaZbiorMap(tempPlik))
        Finally
            If File.Exists(tempPlik) Then File.Delete(tempPlik)
        End Try
    End Sub

    <Fact>
    Public Sub MaksRozmiarSegmentu_OdczytZPlikuTekstowegoIJson()
        Dim tempPlik = Path.GetTempFileName()
        Try
            ' JSON z limitem segmentu
            Dim json = "{ ""adres"": ""https://serwer/wms"", ""maksRozmiarSegmentu"": 1500, ""warstwy"": [ { ""nazwa"": ""w1"", ""maksRozmiarSegmentu"": 1200 } ] }"
            File.WriteAllText(tempPlik, json, Encoding.UTF8)
            Dim plik = PlikWarstw.Wczytaj(tempPlik)
            Assert.Equal(1500, plik.MaksRozmiarSegmentu)
            Assert.Equal(1200, plik.Warstwy(0).MaksRozmiarSegmentu)
        Finally
            If File.Exists(tempPlik) Then File.Delete(tempPlik)
        End Try
    End Sub

#End Region

#Region "S-01 .. S-05: Bezpieczny bok segmentu w kanapce"

    <Fact>
    Public Sub BezpiecznyBokSegmentu_ZwracaMinimumDodatnichLimitow()
        Dim zestaw As New ZestawWarstw()
        ' Domyślny bok 2048 px
        Assert.Equal(2048, zestaw.BezpiecznyBokSegmentu(2048))

        ' Dodajemy warstwę bez limitu (0)
        zestaw.Dodaj("orto", 100, "https://orto", "orto_zbior", 1.0)
        Assert.Equal(2048, zestaw.BezpiecznyBokSegmentu(2048))

        ' Dodajemy warstwę z limitem 1500 px (np. EGiB)
        zestaw.Dodaj("dzialki", 100, "https://egib", "dzialki_ewid_EGIB", 0.5)
        zestaw(1).MaksRozmiarSegmentu = 1500
        Assert.Equal(1500, zestaw.BezpiecznyBokSegmentu(2048))

        ' Dodajemy warstwę z limitem 1024 px
        zestaw.Dodaj("inne", 100, "https://inne", "inne_zbior", 1.0)
        zestaw(2).MaksRozmiarSegmentu = 1024
        Assert.Equal(1024, zestaw.BezpiecznyBokSegmentu(2048))

        ' Gdy domyślny bok jest mniejszy niż limity, zostaje mniejszy bok
        Assert.Equal(512, zestaw.BezpiecznyBokSegmentu(512))
    End Sub

#End Region

#Region "C-01 .. C-05: Profil CAD, układ PL-2000 i skrypt SCR"

    <Fact>
    Public Sub SugerujStrefaPl2000_DopasowujeStrefyWzdluzPoludnikow()
        ' Strefa 5 (południk 15°, granica wsch. 16.5°)
        Assert.Equal(2176, UkladWspolrzednych.SugerujStrefaPl2000(14.5).Epsg)
        Assert.Equal(2176, UkladWspolrzednych.SugerujStrefaPl2000(16.0).Epsg)

        ' Strefa 6 (południk 18°, 16.5° - 19.5°)
        Assert.Equal(2177, UkladWspolrzednych.SugerujStrefaPl2000(17.0).Epsg)
        Assert.Equal(2177, UkladWspolrzednych.SugerujStrefaPl2000(19.0).Epsg)

        ' Strefa 7 (południk 21°, 19.5° - 22.5°)
        Assert.Equal(2178, UkladWspolrzednych.SugerujStrefaPl2000(20.0).Epsg)
        Assert.Equal(2178, UkladWspolrzednych.SugerujStrefaPl2000(21.5).Epsg)

        ' Strefa 8 (południk 24°, powyżej 22.5°)
        Assert.Equal(2179, UkladWspolrzednych.SugerujStrefaPl2000(23.0).Epsg)
        Assert.Equal(2179, UkladWspolrzednych.SugerujStrefaPl2000(24.5).Epsg)
    End Sub

    <Fact>
    Public Sub SprawdzZgodnoscUkladu_WykrywaNieobslugiwaneWmtsIXyzDlaPl2000()
        Dim zestaw As New ZestawWarstw()
        ' Warstwa WMS - obsługuje PL-2000
        zestaw.Dodaj("dzialki", 100, "https://egib/wms", "dzialki_ewid_EGIB", 0.5, typZrodla:=TypUslugiMapowej.Wms)
        Assert.Equal("", ZarzadcaSzablonow.SprawdzZgodnoscUkladu(zestaw, UkladWspolrzednych.PL2000Strefa7))

        ' Warstwa WMTS (np. TOPO_WMTS) w PL-1992 - nie obsługuje stref PL-2000
        zestaw.Dodaj("topo", 100, "https://geoportal/wmts", "TOPO_WMTS", 1.0, typZrodla:=TypUslugiMapowej.Wmts)
        Dim bladWmts = ZarzadcaSzablonow.SprawdzZgodnoscUkladu(zestaw, UkladWspolrzednych.PL2000Strefa7)
        Assert.Contains("WMTS", bladWmts)

        ' Dla PL-1992 (EPSG:2180) obie warstwy są dozwolone
        Assert.Equal("", ZarzadcaSzablonow.SprawdzZgodnoscUkladu(zestaw, UkladWspolrzednych.PL1992))
    End Sub

    <Fact>
    Public Sub SkryptCad_GenerujePoprawneOsieWspolrzednychISciezkeWzgledna()
        ' Test przypadku C-02: dla Łodzi w Strefie 7 (EPSG:2178)
        ' Geodezyjne X (północ) ok. 5 736 000, Y (wschód) ok. 7 394 000.
        ' W AutoCAD współrzędna X to wschód (Easting), Y to północ (Northing).
        Dim uklad7 = UkladWspolrzednych.PL2000Strefa7
        Dim zasieg7 As New Zasieg(5736000, 7394000, 5737000, 7395000)

        Dim zadanie As New ZadanieCad With {
            .Uklad = uklad7,
            .FolderRysunku = "C:\Projekty\Mapy\",
            .Prefiks = "seg"
        }
        Dim obraz As New ObrazGeoreferencyjny With {
            .Uklad = uklad7,
            .Zasieg = zasieg7,
            .SzerokoscPx = 1000,
            .WysokoscPx = 1000,
            .NazwaPliku = "mapa_lodz.tif",
            .SciezkaPliku = "C:\Projekty\Mapy\mapa_lodz.tif"
        }
        zadanie.Rastry.Add(obraz)

        Dim scr = EksportCad.Skrypt(zadanie)

        ' Sprawdzenie struktury skryptu:
        Assert.Contains("_UNDO _BEGIN", scr)
        Assert.Contains("_SETVAR FILEDIA 0", scr)
        Assert.Contains("_UCS _WORLD", scr)
        Assert.Contains("_-IMAGE", scr)
        Assert.Contains("_ATTACH", scr)

        ' Sprawdzenie ścieżki względnej (SciezkaWRysunku):
        Assert.Contains(".\mapa_lodz.tif", scr)

        ' Sprawdzenie zamiany osi: punkt lewy dolny w AutoCAD ma być: X_CAD (wschód), Y_CAD (północ)
        ' Czyli: 7394000,5736000
        Assert.Contains("7394000,5736000", scr)
        Assert.DoesNotContain("5736000,7394000", scr)
    End Sub

#End Region

End Class
