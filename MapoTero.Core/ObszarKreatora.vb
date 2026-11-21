'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Text.RegularExpressions

''' <summary>Obszar mapy kreatora z granic miejscowości (prostokąt wyszukiwarki) i z działki ewidencyjnej (ULDK).</summary>
Public Module ObszarKreatora

    ''' <summary>Najkrótszy bok obszaru z granic - dla adresu lub ulicy prostokąt wyszukiwarki ma kilkadziesiąt metrów.</summary>
    Public Const MinBokGranic As Double = 1000.0
    ''' <summary>Pas terenu wokół działki - mapa pokazuje także sąsiedztwo.</summary>
    Public Const BuforDzialki As Double = 150.0

    ''' <summary>
    ''' Prostokąt PL-1992 obejmujący prostokąt WGS84 z wyszukiwarki z zapasem 5% z każdej strony; krótszy bok co najmniej
    ''' <see cref="MinBokGranic"/> (wydłużony symetrycznie wokół środka).
    ''' </summary>
    Public Function ZasiegZGranic(minSzer As Double, maxSzer As Double, minDl As Double, maxDl As Double) As Zasieg
        Dim u = UkladWspolrzednych.PL1992
        Dim narozniki = {u.ZWgs84(minSzer, minDl), u.ZWgs84(minSzer, maxDl), u.ZWgs84(maxSzer, minDl), u.ZWgs84(maxSzer, maxDl)}
        Dim z As New Zasieg(narozniki.Min(Function(p) p.X), narozniki.Min(Function(p) p.Y),
                            narozniki.Max(Function(p) p.X), narozniki.Max(Function(p) p.Y))
        Dim zapasX = Math.Max(z.Wysokosc * 0.05, (MinBokGranic - z.Wysokosc) / 2)
        Dim zapasY = Math.Max(z.Szerokosc * 0.05, (MinBokGranic - z.Szerokosc) / 2)
        Return New Zasieg(z.XDol - zapasX, z.YLewy - zapasY, z.XGora + zapasX, z.YPrawy + zapasY)
    End Function

    ''' <summary>Zasięg działki powiększony o <see cref="BuforDzialki"/> z każdej strony.</summary>
    Public Function ZasiegDzialki(dzialka As Zasieg) As Zasieg
        Return New Zasieg(dzialka.XDol - BuforDzialki, dzialka.YLewy - BuforDzialki,
                          dzialka.XGora + BuforDzialki, dzialka.YPrawy + BuforDzialki)
    End Function

    Private ReadOnly WzorTeryt As New Regex("^\d{6}_\d\.\d{4}\.\S+$")
    Private ReadOnly WzorSlowaDzialka As New Regex("^(?:działka|dzialka|dz\.)\s+(?<reszta>.+\S)\s*$", RegexOptions.IgnoreCase)

    ''' <summary>
    ''' Zapytanie do ULDK, gdy tekst wskazuje działkę: identyfikator TERYT (np. 121901_1.0001.123/4) albo słowo „działka”/„dz.”
    ''' z obrębem i numerem (np. „działka Wieliczka 123/4”). Nothing - zwykłe wyszukiwanie miejsc. Sam zapis „Długa 12/4”
    ''' nie jest działką, bo tak samo wygląda adres z numerem mieszkania.
    ''' </summary>
    Public Function ZapytanieDzialki(tekst As String) As String
        If String.IsNullOrWhiteSpace(tekst) Then Return Nothing
        Dim t = tekst.Trim()
        If WzorTeryt.IsMatch(t) Then Return t
        Dim m = WzorSlowaDzialka.Match(t)
        If m.Success Then Return m.Groups("reszta").Value.Trim()
        Return Nothing
    End Function

End Module
