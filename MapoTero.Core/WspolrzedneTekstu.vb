'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>Punkt rozpoznany we wklejonym tekście: położenie w WGS84 i opis układu, w którym go zapisano.</summary>
Public Class PunktZTekstu
    Public Property Punkt As PunktGeo
    ''' <summary>Układ zapisu dla użytkownika, np. „WGS84”, „PL-1992”, „PL-2000 strefa 7”.</summary>
    Public Property Uklad As String = ""
End Class

''' <summary>
''' Rozpoznaje współrzędne punktu wklejone jako tekst (np. z Google Maps, maila, dokumentacji geodezyjnej):
''' WGS84 w stopniach dziesiętnych lub w stopniach, minutach i sekundach, PL-1992 i PL-2000 (X - północ, Y - wschód).
''' </summary>
Public Module WspolrzedneTekstu

    ' Znaki minut (′ ’) i sekund (″ ”) przez ChrW - VB traktuje typograficzne cudzysłowy i apostrof jak " i komentarz
    Private ReadOnly ZnakMinut As String = "['" & ChrW(&H2032) & ChrW(&H2019) & "]"
    Private ReadOnly ZnakSekund As String = "(?:""|''|" & ChrW(&H2033) & "|" & ChrW(&H201D) & ")"
    Private ReadOnly WzorStopni As New Regex(
        "(?<![\d.,\p{L}])(?:(?<kierPrzed>[NSEW])\s*)?(?<st>\d{1,3}(?:[.,]\d+)?)\s*[" & ChrW(&HB0) & ChrW(&HBA) & ChrW(&H2DA) & "]\s*" &
        "(?:(?<min>\d{1,2}(?:[.,]\d+)?)\s*" & ZnakMinut & "\s*)?" &
        "(?:(?<sek>\d{1,2}(?:[.,]\d+)?)\s*" & ZnakSekund & "\s*)?" &
        "(?(kierPrzed)|(?:(?<kierPo>[NSEW])(?![\p{L}]))?)",
        RegexOptions.IgnoreCase)
    Private ReadOnly WzorLiczby As New Regex("[-+]?\d+(?:[.,]\d+)?")

    ''' <summary>Punkt z tekstu albo Nothing, gdy tekst nie wygląda na dwie współrzędne (np. jest nazwą miejscowości).</summary>
    Public Function Rozpoznaj(tekst As String) As PunktZTekstu
        If String.IsNullOrWhiteSpace(tekst) Then Return Nothing
        Dim t = tekst.Trim()
        ' litery inne niż oznaczenia kierunków i jednostek - to nazwa lub adres, nie współrzędne
        Dim bezKierunkow = Regex.Replace(t, "\b[NSEWnsew]\b|[NSEWnsew](?=\s|$|[,;])|(?<=\d)[NSEWnsew]|(?<=^|[\s,;])[NSEWnsew](?=\d)|\b(?:x|y|X|Y)\s*[=:]?", " ")
        If Regex.IsMatch(bezKierunkow, "\p{L}") Then Return Nothing

        Dim stopnie = WzorStopni.Matches(t)
        If stopnie.Count = 2 Then Return ZeStopni(stopnie(0), stopnie(1))

        Dim liczby = WzorLiczby.Matches(t)
        If liczby.Count <> 2 Then Return Nothing
        Dim a = Liczba(liczby(0).Value)
        Dim b = Liczba(liczby(1).Value)
        Dim yPrzedX = Regex.IsMatch(t, "(?i)(?:^|[\s,;])Y\s*[=:]?\s*[-+\d].*(?:[\s,;])X\s*[=:]?\s*[-+\d]")
        Dim pierwszyKierunek = Regex.Match(t, "[NSEWnsew]")
        Dim odwrotnie = yPrzedX OrElse (pierwszyKierunek.Success AndAlso "EeWw".Contains(pierwszyKierunek.Value) AndAlso pierwszyKierunek.Index < liczby(1).Index)

        If Math.Abs(a) <= 180 AndAlso Math.Abs(b) <= 180 Then
            If odwrotnie OrElse (Math.Abs(a) > 90 AndAlso Math.Abs(b) <= 90) Then
                Dim tmp = a : a = b : b = tmp
            ElseIf a >= 13 AndAlso a <= 25 AndAlso b >= 48 AndAlso b <= 56 Then
                ' kolejność długość, szerokość (np. GeoJSON) - w Polsce długość 14-24°, szerokość 49-55°
                Dim tmp = a : a = b : b = tmp
            End If
            If Regex.IsMatch(t, "[Ss](?=\s|$|[,;])|\b[Ss]\b") Then a = -Math.Abs(a)
            If Regex.IsMatch(t, "[Ww](?=\s|$|[,;])|\b[Ww]\b") Then b = -Math.Abs(b)
            ' tylko okolice Polski: mapy kreatora są w PL-1992, a para małych liczb poza tym zakresem to raczej nie punkt
            If Not WOkolicyPolski(a, b) Then Return Nothing
            Return New PunktZTekstu With {.Punkt = New PunktGeo(a, b), .Uklad = "WGS84"}
        End If
        If yPrzedX Then
            Dim tmp = a : a = b : b = tmp
        End If
        Return ZUkladuPlaskiego(a, b)
    End Function

    Private Function WOkolicyPolski(szerokosc As Double, dlugosc As Double) As Boolean
        Return szerokosc >= 45 AndAlso szerokosc <= 58 AndAlso dlugosc >= 10 AndAlso dlugosc <= 30
    End Function

    Private Function Liczba(s As String) As Double
        Return Double.Parse(s.Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Function ZeStopni(m1 As Match, m2 As Match) As PunktZTekstu
        Dim kierunek = Function(m As Match) As String
                           If m.Groups("kierPrzed").Success Then Return m.Groups("kierPrzed").Value.ToUpperInvariant()
                           If m.Groups("kierPo").Success Then Return m.Groups("kierPo").Value.ToUpperInvariant()
                           Return ""
                       End Function
        Dim wartosc = Function(m As Match) As Double
                          Dim v = Liczba(m.Groups("st").Value)
                          If m.Groups("min").Success Then v += Liczba(m.Groups("min").Value) / 60.0
                          If m.Groups("sek").Success Then v += Liczba(m.Groups("sek").Value) / 3600.0
                          Dim k = kierunek(m)
                          If k = "S" OrElse k = "W" Then v = -v
                          Return v
                      End Function
        Dim a = wartosc(m1), b = wartosc(m2)
        Dim k1 = kierunek(m1)
        If k1 = "E" OrElse k1 = "W" Then
            Dim tmp = a : a = b : b = tmp
        End If
        If Not WOkolicyPolski(a, b) Then Return Nothing
        Return New PunktZTekstu With {.Punkt = New PunktGeo(a, b), .Uklad = "WGS84"}
    End Function

    ''' <summary>PL-2000 (Y z numerem strefy 5-8 na początku) albo PL-1992; X jest współrzędną północną.</summary>
    Private Function ZUkladuPlaskiego(a As Double, b As Double) As PunktZTekstu
        Dim jestX2000 = Function(v As Double) v >= 5300000 AndAlso v <= 6200000
        Dim jestY2000 = Function(v As Double) v >= 5200000 AndAlso v <= 8800000 AndAlso Math.Floor(v / 1000000) >= 5
        Dim x As Double, y As Double
        If jestX2000(a) AndAlso jestY2000(b) Then
            x = a : y = b
        ElseIf jestX2000(b) AndAlso jestY2000(a) Then
            x = b : y = a
        Else
            x = 0 : y = 0
        End If
        If x > 0 Then
            Dim strefa = CInt(Math.Floor(y / 1000000))
            Dim uklad = Select2000(strefa)
            If uklad Is Nothing Then Return Nothing
            Return New PunktZTekstu With {.Punkt = uklad.DoWgs84(x, y), .Uklad = "PL-2000 strefa " & strefa}
        End If
        Dim jest1992 = Function(v As Double) v >= 100000 AndAlso v <= 900000
        If jest1992(a) AndAlso jest1992(b) Then
            Dim p = UkladWspolrzednych.PL1992.DoWgs84(a, b)
            Return New PunktZTekstu With {.Punkt = p, .Uklad = "PL-1992"}
        End If
        Return Nothing
    End Function

    Private Function Select2000(strefa As Integer) As UkladWspolrzednych
        Select Case strefa
            Case 5 : Return UkladWspolrzednych.PL2000Strefa5
            Case 6 : Return UkladWspolrzednych.PL2000Strefa6
            Case 7 : Return UkladWspolrzednych.PL2000Strefa7
            Case 8 : Return UkladWspolrzednych.PL2000Strefa8
            Case Else : Return Nothing
        End Select
    End Function

End Module
