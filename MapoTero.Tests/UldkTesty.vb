'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class UldkTesty

    <Fact>
    Public Sub SparsujWierszUldkPoprawny()
        Dim wiersz = "022503_5.0003.134|022503_5.0003.134|134|Krzewina|Bogatynia|powiat zgorzelecki|SRID=2180;POLYGON((216936.78 358211.15,216905.01 358178.82,216916.43 358172.05,216969.82 358235.32,216936.78 358211.15))"

        Dim dzialka = KlientUldk.SparsujWiersz(wiersz)

        Assert.NotNull(dzialka)
        Assert.Equal("022503_5.0003.134", dzialka.Identyfikator)
        Assert.Equal("134", dzialka.Numer)
        Assert.Equal("Krzewina", dzialka.Obreb)
        Assert.Equal("Bogatynia", dzialka.Gmina)
        Assert.Equal("powiat zgorzelecki", dzialka.Powiat)
        Assert.Contains("134", dzialka.Opis)

        Assert.True(dzialka.Zasieg1992.Poprawny)
        ' X to Northing (~358000), Y to Easting (~216000)
        Assert.True(dzialka.Zasieg1992.XDol >= 358100.0 AndAlso dzialka.Zasieg1992.XDol <= 358200.0)
        Assert.True(dzialka.Zasieg1992.XGora >= 358200.0 AndAlso dzialka.Zasieg1992.XGora <= 358300.0)
        Assert.True(dzialka.Zasieg1992.YLewy >= 216900.0 AndAlso dzialka.Zasieg1992.YLewy <= 217000.0)
        Assert.True(dzialka.Zasieg1992.YPrawy >= 216900.0 AndAlso dzialka.Zasieg1992.YPrawy <= 217000.0)

        Assert.NotEmpty(dzialka.ObrysWgs84)
        Dim p = dzialka.ObrysWgs84(0)
        Assert.True(p.Szerokosc >= 50.8 AndAlso p.Szerokosc <= 51.1)
        Assert.True(p.Dlugosc >= 14.8 AndAlso p.Dlugosc <= 15.1)
    End Sub

    <Fact>
    Public Sub SparsujWierszNiepoprawny()
        Assert.Null(KlientUldk.SparsujWiersz(""))
        Assert.Null(KlientUldk.SparsujWiersz("blad"))
        Assert.Null(KlientUldk.SparsujWiersz("a|b|c"))
    End Sub

    <Fact>
    Public Async Function SzukajDzialekLiveUldkAsync() As Task
        Dim wyniki As List(Of DzialkaKatastralna) = Nothing
        For proba = 1 To 3
            Dim ponow As Boolean = False
            Try
                wyniki = Await KlientUldk.SzukajDzialekAsync("022503_5.0003.134")
                If wyniki IsNot Nothing AndAlso wyniki.Count > 0 Then Exit For
            Catch
                If proba = 3 Then Throw
                ponow = True
            End Try
            If ponow Then Await Task.Delay(1500)
        Next
        Assert.NotNull(wyniki)
        Assert.Single(wyniki)
        Dim d = wyniki(0)
        Assert.Equal("022503_5.0003.134", d.Identyfikator)
        Assert.Equal("134", d.Numer)
        Assert.Equal("Krzewina", d.Obreb)
        Assert.True(d.Zasieg1992.Poprawny)
        Assert.True(d.ObrysWgs84.Count >= 4)
    End Function

    <Fact>
    Public Async Function PobierzMapeDzialkiZPodklademOrtoAsync() As Task
        Dim wyniki = Await KlientUldk.SzukajDzialekAsync("022503_5.0003.134")
        Assert.Single(wyniki)
        Dim d = wyniki(0)
        Dim margines = 30.0
        Dim z = New Zasieg(d.Zasieg1992.XDol - margines, d.Zasieg1992.YLewy - margines,
                           d.Zasieg1992.XGora + margines, d.Zasieg1992.YPrawy + margines)

        Dim wmsOrto = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolution?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
        Dim urlOrto = Wms.ZapytanieGetMap(wmsOrto, {"Raster"}, UkladWspolrzednych.PL1992, z, "jpeg", 400, 400, False, False)
        Dim odpOrto = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(urlOrto, 25, Threading.CancellationToken.None, True)
        Assert.NotNull(odpOrto.Item1)

        Dim wmsDzialki = "https://integracja.gugik.gov.pl/cgi-bin/KrajowaIntegracjaEwidencjiGruntow?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
        Dim urlDzialki = Wms.ZapytanieGetMap(wmsDzialki, {"dzialki,numery_dzialek"}, UkladWspolrzednych.PL1992, z, "png", 400, 400, False, True)
        Dim odpDzialki = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(urlDzialki, 25, Threading.CancellationToken.None, True)
        Assert.NotNull(odpDzialki.Item1)

        Dim pikseleOrto = NakladanieObrazow.Piksele(odpOrto.Item1, 400, 400)
        Dim pikseleDzialki = NakladanieObrazow.Piksele(odpDzialki.Item1, 400, 400)
        NakladanieWarstw.Naloz(pikseleOrto, pikseleDzialki, 100, TrybMieszania.Zwykly)

        Dim sciezkaWyniku = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\test_dzialka_krzewina_134.png"
        IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(sciezkaWyniku))
        Using bmp = NakladanieObrazow.DoBitmapy(pikseleOrto, 400, 400)
            bmp.Save(sciezkaWyniku, Drawing.Imaging.ImageFormat.Png)
        End Using

        Assert.True(IO.File.Exists(sciezkaWyniku))
        Assert.True(New IO.FileInfo(sciezkaWyniku).Length > 10000)
    End Function

End Class
