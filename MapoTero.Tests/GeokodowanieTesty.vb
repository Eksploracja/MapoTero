'Copyright (C) <2026>  MapoTero
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class GeokodowanieTesty

    <Fact>
    Public Sub ParsujPustaOdpowiedzJson()
        Dim wynik = KlientGeokodowania.ParsujWynikiJson("")
        Assert.NotNull(wynik)
        Assert.Empty(wynik)

        Dim wynikNiepoprawny = KlientGeokodowania.ParsujWynikiJson("{}")
        Assert.NotNull(wynikNiepoprawny)
        Assert.Empty(wynikNiepoprawny)
    End Sub

    <Fact>
    Public Sub ParsujOdpowiedzMiastaJson()
        Dim json = "[{" &
            """place_id"":180264922," &
            """lat"":""50.0469432"",""lon"":""19.9971534""," &
            """class"":""boundary"",""type"":""administrative""," &
            """name"":""Kraków""," &
            """display_name"":""Kraków, województwo małopolskie, Polska""," &
            """boundingbox"":[""49.9676668"",""50.1261338"",""19.7922355"",""20.2173455""]" &
            "}]"

        Dim lista = KlientGeokodowania.ParsujWynikiJson(json)
        Assert.Single(lista)

        Dim m = lista(0)
        Assert.Equal("Kraków", m.Nazwa)
        Assert.Equal("Kraków, województwo małopolskie, Polska", m.PelnaNazwa)
        Assert.Equal(50.0469432, m.Szerokosc, 5)
        Assert.Equal(19.9971534, m.Dlugosc, 5)
        Assert.Equal(49.9676668, m.MinSzerokosc, 5)
        Assert.Equal(50.1261338, m.MaxSzerokosc, 5)
        Assert.Equal(19.7922355, m.MinDlugosc, 5)
        Assert.Equal(20.2173455, m.MaxDlugosc, 5)
        Assert.True(m.MaObszar)
        Assert.Equal("obszar administracyjny", m.TypOpis)
        Assert.Equal("Kraków", m.KrotkaNazwa)
        Assert.Contains("50.0469°N", m.WspolrzedneTekst)
        Assert.Contains("19.9972°E", m.WspolrzedneTekst)
    End Sub

    <Fact>
    Public Sub ParsujOdpowiedzAdresuJson()
        Dim json = "[{" &
            """place_id"":184865964," &
            """lat"":""50.0705645"",""lon"":""19.9367818""," &
            """class"":""highway"",""type"":""residential""," &
            """display_name"":""Długa, Kleparz, Kraków, województwo małopolskie, 31-147, Polska""," &
            """boundingbox"":[""50.066"",""50.072"",""19.934"",""19.938""]" &
            "}]"

        Dim lista = KlientGeokodowania.ParsujWynikiJson(json)
        Assert.Single(lista)

        Dim a = lista(0)
        Assert.Equal("Długa", a.KrotkaNazwa)
        Assert.Equal("ulica / droga", a.TypOpis)
        Assert.Equal(50.0705645, a.Szerokosc, 5)
        Assert.Equal(19.9367818, a.Dlugosc, 5)
    End Sub

    <Theory>
    <InlineData("Gdynia, piotrkowska 12", "Gdynia, piotrkowska 12")>
    <InlineData("Gdynia,ul. piotrkowska 12", "Gdynia, piotrkowska 12")>
    <InlineData("Gdynia, ul. Piotrkowska 12", "Gdynia, Piotrkowska 12")>
    <InlineData("Gdynia,ul.Piotrkowska 12", "Gdynia, Piotrkowska 12")>
    <InlineData("Gdynia, ulica Piotrkowska 12", "Gdynia, Piotrkowska 12")>
    <InlineData("ul. Marszałkowska 10, Warszawa", "Marszałkowska 10, Warszawa")>
    <InlineData("Gdynia, Piotrkowska 12 m. 4", "Gdynia, Piotrkowska 12")>
    <InlineData("Gdynia, Piotrkowska 12 lok. 5", "Gdynia, Piotrkowska 12")>
    <InlineData("Gdynia, Piotrkowska 12/8", "Gdynia, Piotrkowska 12")>
    <InlineData("Poznań, ul. Półwiejska 10/4", "Poznań, Półwiejska 10")>
    Public Sub NormalizujZapytanieAdresoweTesty(wejscie As String, oczekiwane As String)
        Dim wynik = KlientGeokodowania.NormalizujZapytanieAdresowe(wejscie)
        Assert.Equal(oczekiwane, wynik)
    End Sub

    <Fact>
    Public Sub ParsujOdpowiedzPhotonGeoJson()
        Dim json = "{" &
            """type"": ""FeatureCollection""," &
            """features"": [{" &
            "  ""type"": ""Feature""," &
            "  ""geometry"": {""type"": ""Point"", ""coordinates"": [18.5259658, 54.486678]}," &
            "  ""properties"": {" &
            "    ""osm_key"": ""building"",""osm_value"": ""yes"",""type"": ""house""," &
            "    ""housenumber"": ""12"",""street"": ""Piotrkowska"",""district"": ""Mały Kack""," &
            "    ""city"": ""Gdynia"",""state"": ""województwo pomorskie"",""country"": ""Polska"",""postcode"": ""81-504""," &
            "    ""extent"": [18.5258862, 54.4867073, 18.5261025, 54.4865969]" &
            "  }" &
            "}]}"

        Dim lista = KlientGeokodowania.ParsujWynikiPhotonJson(json)
        Assert.Single(lista)

        Dim p = lista(0)
        Assert.Equal("Piotrkowska 12", p.Nazwa)
        Assert.Equal("Piotrkowska 12, Mały Kack, Gdynia, województwo pomorskie, 81-504, Polska", p.PelnaNazwa)
        Assert.Equal(54.486678, p.Szerokosc, 5)
        Assert.Equal(18.5259658, p.Dlugosc, 5)
        Assert.Equal("adres / budynek", p.TypOpis)
        Assert.True(p.MaObszar)
        Assert.Equal(54.4865969, p.MinSzerokosc, 5)
        Assert.Equal(54.4867073, p.MaxSzerokosc, 5)
    End Sub

End Class
