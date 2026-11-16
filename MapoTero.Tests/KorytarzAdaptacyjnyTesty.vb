'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports MapoTero.Core
Imports Xunit

Public Class KorytarzAdaptacyjnyTesty

    <Fact>
    Public Sub BuforAdaptacyjny_WyznaczaZroznicowaneSzerokosci()
        Dim k As New Korytarz With {
            .SzerokoscMetry = 1000.0,
            .BuforAdaptacyjny = True
        }

        ' Zoom niski (przeglądowy <= 10): min. 10 km
        Assert.True(k.SzerokoscDlaPoziomu(10) >= 10000.0)

        ' Zoom średnio-niski (12): min. 5 km
        Assert.True(k.SzerokoscDlaPoziomu(12) >= 5000.0)

        ' Zoom średni (14): szerokość bazowa 1000 m
        Assert.Equal(1000.0, k.SzerokoscDlaPoziomu(14))

        ' Zoom detaliczny (16): wąski bufor (300..500 m)
        Assert.Equal(500.0, k.SzerokoscDlaPoziomu(16))
    End Sub

    <Fact>
    Public Sub BuforAdaptacyjny_SerializacjaTekstowa()
        Dim k As New Korytarz With {
            .SzerokoscMetry = 1500.0,
            .BuforAdaptacyjny = True,
            .Nazwa = "Trasa ultramaraton"
        }
        k.DodajLinie({New PunktGeo(52.0, 21.0), New PunktGeo(52.1, 21.1)})

        Dim tekst = k.DoTekstu()
        Assert.Contains("bufor_adaptacyjny=1", tekst)

        Dim odtworzony = Korytarz.ZTekstuZapisu(tekst)
        Assert.True(odtworzony.BuforAdaptacyjny)
        Assert.Equal(1500.0, odtworzony.SzerokoscMetry)
        Assert.Equal("Trasa ultramaraton", odtworzony.Nazwa)
    End Sub

End Class
