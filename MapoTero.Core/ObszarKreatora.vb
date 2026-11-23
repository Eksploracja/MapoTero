'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Text.RegularExpressions

''' <summary>Rodzaj tekstu wpisanego w pole wyszukiwania obszaru kreatora.</summary>
Public Enum RodzajZapytania
    ''' <summary>Miejscowość lub adres - wyszukiwarka miejsc.</summary>
    Miejsce
    ''' <summary>Wklejone współrzędne - punkt bez pytania serwera.</summary>
    Wspolrzedne
    ''' <summary>Działka ewidencyjna - usługa ULDK.</summary>
    Dzialka
End Enum

''' <summary>Rozpoznane zapytanie wyszukiwarki obszaru (<see cref="ObszarKreatora.RozpoznajZapytanie"/>).</summary>
Public Class ZapytanieWyszukiwarki
    Public Property Rodzaj As RodzajZapytania
    ''' <summary>Tekst dla wyszukiwarki miejsc albo zapytanie do ULDK.</summary>
    Public Property Tekst As String = ""
    ''' <summary>Wynik od razu gotowy (punkt ze współrzędnych); Nothing dla miejsc i działek.</summary>
    Public Property Punkt As WynikGeokodowania
End Class

''' <summary>Działka ewidencyjna z ULDK jako wynik wyszukiwania obszaru w kreatorze (obszar - zasięg działki z pasem).</summary>
Public Class WynikDzialki
    Inherits WynikGeokodowania

    Public Property Zasieg1992 As Zasieg

    Public Shared Function Z(d As DzialkaKatastralna) As WynikDzialki
        Dim srodek = UkladWspolrzednych.PL1992.DoWgs84(d.Zasieg1992.Srodek)
        Return New WynikDzialki With {
            .Nazwa = "Działka " & d.Numer,
            .PelnaNazwa = "Działka " & d.Numer & ", obręb " & d.Obreb & If(d.Gmina <> "", ", gm. " & d.Gmina, "") & If(d.Powiat <> "", ", pow. " & d.Powiat, "") &
                          If(d.Identyfikator <> "", ", " & d.Identyfikator, ""),
            .Typ = "dzialka", .Szerokosc = srodek.Szerokosc, .Dlugosc = srodek.Dlugosc, .Zasieg1992 = d.Zasieg1992}
    End Function
End Class

''' <summary>
''' Obszar mapy kreatora: z granic miejscowości (prostokąt wyszukiwarki), z działki ewidencyjnej (ULDK), promień wokół
''' punktu; rozpoznanie tekstu wyszukiwarki i opis wymiarów obszaru.
''' </summary>
Public Module ObszarKreatora

    ''' <summary>Pozycja listy promieni: prostokąt miejscowości z wyszukiwarki zamiast kwadratu wokół punktu.</summary>
    Public Const OpcjaGranic As String = "wg granic"
    ''' <summary>Promień, gdy lista go nie podaje (np. „wg granic” dla wklejonych współrzędnych).</summary>
    Public Const PromienDomyslny As Double = 5000.0

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

    ''' <summary>
    ''' Rozpoznaje tekst wyszukiwarki: wklejone współrzędne (Google Maps, dokumentacja geodezyjna) dają punkt bez pytania
    ''' serwera, identyfikator lub słowo „działka” - zapytanie do ULDK, a reszta idzie do wyszukiwarki miejsc.
    ''' </summary>
    Public Function RozpoznajZapytanie(fraza As String) As ZapytanieWyszukiwarki
        Dim tekst = If(fraza, "").Trim()
        Dim punkt = WspolrzedneTekstu.Rozpoznaj(tekst)
        If punkt IsNot Nothing Then
            Dim kultura = Globalization.CultureInfo.InvariantCulture
            Return New ZapytanieWyszukiwarki With {
                .Rodzaj = RodzajZapytania.Wspolrzedne, .Tekst = tekst,
                .Punkt = New WynikGeokodowania With {
                    .Nazwa = String.Format(kultura, "Punkt {0:0.00000} {1:0.00000}", punkt.Punkt.Szerokosc, punkt.Punkt.Dlugosc),
                    .PelnaNazwa = "Punkt, współrzędne " & punkt.Uklad & ": " & tekst,
                    .Typ = "wspolrzedne", .Szerokosc = punkt.Punkt.Szerokosc, .Dlugosc = punkt.Punkt.Dlugosc}}
        End If
        Dim dzialka = ZapytanieDzialki(tekst)
        If dzialka IsNot Nothing Then Return New ZapytanieWyszukiwarki With {.Rodzaj = RodzajZapytania.Dzialka, .Tekst = dzialka}
        Return New ZapytanieWyszukiwarki With {.Rodzaj = RodzajZapytania.Miejsce, .Tekst = tekst}
    End Function

    ''' <summary>Promień w metrach z pozycji listy („2 km” - 2000 m, najmniej 500 m); bez liczby - <see cref="PromienDomyslny"/>.</summary>
    Public Function PromienZOpcji(opcja As String) As Double
        Dim liczba = If(opcja Is Nothing, Nothing, Regex.Match(opcja, "\d+"))
        If liczba IsNot Nothing AndAlso liczba.Success Then Return Math.Max(500.0, CDbl(liczba.Value) * 1000.0)
        Return PromienDomyslny
    End Function

    ''' <summary>
    ''' Obszar wybranego wyniku wyszukiwania: działka z pasem <see cref="BuforDzialki"/>, prostokąt miejscowości przy opcji
    ''' <see cref="OpcjaGranic"/>, a gdy wynik nie ma prostokąta albo wybrano promień - kwadrat wokół punktu.
    ''' </summary>
    Public Function ZasiegWyniku(m As WynikGeokodowania, opcjaPromienia As String) As Zasieg
        Dim dzialka = TryCast(m, WynikDzialki)
        If dzialka IsNot Nothing Then Return ZasiegDzialki(dzialka.Zasieg1992)
        If opcjaPromienia = OpcjaGranic AndAlso m.MaObszar Then
            Return ZasiegZGranic(m.MinSzerokosc, m.MaxSzerokosc, m.MinDlugosc, m.MaxDlugosc)
        End If
        Dim promien = PromienZOpcji(opcjaPromienia)
        Dim srodek = UkladWspolrzednych.PL1992.ZWgs84(m.Szerokosc, m.Dlugosc)
        Return New Zasieg(srodek.X - promien, srodek.Y - promien, srodek.X + promien, srodek.Y + promien)
    End Function

    ''' <summary>
    ''' Lokalizacja wyniku pod jego nazwą na liście: pełna nazwa bez pierwszego członu (sama miejscowość), bez „Polska”
    ''' i kodu pocztowego na końcu.
    ''' </summary>
    Public Function SformatujLokalizacje(m As WynikGeokodowania) As String
        If m Is Nothing OrElse String.IsNullOrWhiteSpace(m.PelnaNazwa) Then Return ""
        Dim segmenty = m.PelnaNazwa.Split(","c).Select(Function(s) s.Trim()).ToList()
        While segmenty.Count > 1 AndAlso (segmenty.Last().Equals("Polska", StringComparison.OrdinalIgnoreCase) OrElse
                                          Regex.IsMatch(segmenty.Last(), "^\d{2}-\d{3}$"))
            segmenty.RemoveAt(segmenty.Count - 1)
        End While
        If segmenty.Count <= 1 Then Return m.PelnaNazwa
        Return String.Join(", ", segmenty.Skip(1))
    End Function

    ''' <summary>Wymiary i pole obszaru: w metrach, gdy krótszy bok ma mniej niż 1 km, inaczej w kilometrach.</summary>
    Public Function FormatujWymiary(z As Zasieg) As String
        If Not z.Poprawny Then Return "brak"
        Dim pole = z.Szerokosc * z.Wysokosc
        If Math.Min(z.Szerokosc, z.Wysokosc) < 1000 Then
            Return $"{z.Szerokosc:N0} m × {z.Wysokosc:N0} m ({pole:N0} m²)"
        End If
        Return $"{z.Szerokosc / 1000:0.##} km × {z.Wysokosc / 1000:0.##} km ({pole / 1000000:0.##} km²)"
    End Function

    ''' <summary>Szerokość geograficzna środka obszaru PL-1992 (bez obszaru - 52°, środek Polski).</summary>
    Public Function SzerokoscGeo(z As Zasieg) As Double
        If Not z.Poprawny Then Return 52.0
        Return UkladWspolrzednych.PL1992.DoWgs84(z.Srodek.X, z.Srodek.Y).Szerokosc
    End Function

    ''' <summary>
    ''' Opis obszaru obok podglądu w kroku 2: wymiary (Item1) i położenie (Item2). <paramref name="przykladowy"/> - obszar
    ''' przykładowy stylu (użytkownik nie wybrał obszaru), <paramref name="korytarz"/> - pas wzdłuż wybranej trasy.
    ''' </summary>
    Public Function OpisObszaru(zasieg As Zasieg, przykladowy As Boolean, korytarz As Korytarz) As Tuple(Of String, String)
        Dim szerKm = Math.Round(zasieg.Szerokosc / 1000.0, 1)
        Dim wysKm = Math.Round(zasieg.Wysokosc / 1000.0, 1)
        Dim powKm = Math.Round(szerKm * wysKm, 1)
        If przykladowy Then
            Return Tuple.Create("Nie wybrano jeszcze obszaru." & vbCrLf & "Podgląd pokazuje przykład stylu.",
                                $"Przykładowy fragment: {szerKm} km × {wysKm} km" & vbCrLf & "Wybierz obszar jednym ze sposobów po lewej.")
        End If
        If korytarz IsNot Nothing Then
            Return Tuple.Create($"Korytarz trasy: {korytarz.DlugoscKm:0.0} km (pas {korytarz.SzerokoscMetry:0} m)" & vbCrLf &
                                $"Powierzchnia prostokąta: {powKm} km²",
                                $"Ślad: {korytarz.LiczbaPunktow} punktów w {korytarz.Linie.Count} liniach" & vbCrLf &
                                $"Zasięg: {szerKm} km × {wysKm} km" & vbCrLf & "Układ: PL-1992 (EPSG:2180)")
        End If
        Dim wgsMin = UkladWspolrzednych.PL1992.DoWgs84(zasieg.XDol, zasieg.YLewy)
        Dim wgsMax = UkladWspolrzednych.PL1992.DoWgs84(zasieg.XGora, zasieg.YPrawy)
        Return Tuple.Create($"Wymiary: {szerKm} km × {wysKm} km" & vbCrLf & $"Powierzchnia: {powKm} km²",
                            $"N: {wgsMin.Szerokosc:0.000}° - {wgsMax.Szerokosc:0.000}°" & vbCrLf &
                            $"E: {wgsMin.Dlugosc:0.000}° - {wgsMax.Dlugosc:0.000}°" & vbCrLf & "Układ: PL-1992 (EPSG:2180)")
    End Function

    ''' <summary>
    ''' Piksel podglądu obszaru: cały obszar mieści się w ramce z marginesem 12 px, ale piksel nie jest mniejszy niż
    ''' <paramref name="minPiksel"/> (piksel stylu). Dawniej piksel wynikał z boku próbki (512 px), a ramka ma 240 × 210 px
    ''' i pokazuje próbkę w skali 1:1 - widać było tylko środek obszaru.
    ''' </summary>
    ''' <param name="szerokoscRamki">Szerokość ramki w pikselach, najwyżej bok próbki.</param>
    Public Function PikselPodgladu(zasieg As Zasieg, szerokoscRamki As Integer, wysokoscRamki As Integer,
                                   minPiksel As Double, powiekszenie As Double) As Double
        Const Margines As Integer = 12
        Dim szer = Math.Max(1, szerokoscRamki - 2 * Margines)
        Dim wys = Math.Max(1, wysokoscRamki - 2 * Margines)
        Dim bazowy = Math.Max(minPiksel, Math.Max(zasieg.Szerokosc / szer, zasieg.Wysokosc / wys))
        Return Math.Max(0.05, bazowy / powiekszenie)
    End Function

End Module
