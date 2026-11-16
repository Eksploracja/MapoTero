'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Informacje o działce ewidencyjnej pobranej z usługi ULDK GUGiK.</summary>
Public Class DzialkaKatastralna
    Public Property Identyfikator As String = ""
    Public Property Numer As String = ""
    Public Property Obreb As String = ""
    Public Property Gmina As String = ""
    Public Property Powiat As String = ""
    Public Property Wojewodztwo As String = ""
    Public Property Wkt As String = ""
    Public Property Zasieg1992 As Zasieg
    Public ReadOnly Property ObrysWgs84 As New List(Of PunktGeo)
    ''' <summary>Pierścienie zewnętrzne wszystkich części działki (MULTIPOLYGON może mieć ich kilka).</summary>
    Public ReadOnly Property ObrysyWgs84 As New List(Of List(Of PunktGeo))

    Public ReadOnly Property Opis As String
        Get
            Dim sb As New System.Text.StringBuilder()
            If Not String.IsNullOrEmpty(Numer) Then sb.Append("Działka nr ").Append(Numer)
            If Not String.IsNullOrEmpty(Obreb) Then sb.Append(", obręb: ").Append(Obreb)
            If Not String.IsNullOrEmpty(Gmina) Then sb.Append(", gm. ").Append(Gmina)
            If Not String.IsNullOrEmpty(Powiat) Then sb.Append(", pow. ").Append(Powiat)
            Return sb.ToString()
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return If(Not String.IsNullOrEmpty(Identyfikator), Identyfikator & " (" & Opis & ")", Opis)
    End Function
End Class

''' <summary>
''' Klient Usługi Lokalizacji Działek Katastralnych (ULDK) Głównego Urzędu Geodezji i Kartografii.
''' Umożliwia wyszukiwanie działek po identyfikatorze TERYT lub nazwie obrębu i numerze działki.
''' </summary>
Public NotInheritable Class KlientUldk

    Private Shared ReadOnly _klient As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(30)}

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Wyszukuje działki na podstawie zadanego zapytania (pełny kod TERYT np. '022602_1.0003.59'
    ''' albo nazwa obrębu/miejscowości i numer np. 'Krzewina 134').
    ''' </summary>
    Public Shared Async Function SzukajDzialekAsync(zapytanie As String,
                                                    Optional token As CancellationToken = Nothing) As Task(Of List(Of DzialkaKatastralna))
        Dim wynik As New List(Of DzialkaKatastralna)()
        If String.IsNullOrWhiteSpace(zapytanie) Then Return wynik

        Dim czyste = zapytanie.Trim()
        Dim jestTeryt = czyste.Contains("_") OrElse (czyste.Contains(".") AndAlso Char.IsDigit(czyste(0)))
        Dim url As String

        If jestTeryt Then
            url = "https://uldk.gugik.gov.pl/?request=GetParcelById&id=" & Uri.EscapeDataString(czyste) &
                  "&result=id,teryt,parcel,region,commune,county,geom_wkt"
        Else
            url = "https://uldk.gugik.gov.pl/?request=GetParcelByIdOrNr&id=" & Uri.EscapeDataString(czyste) &
                  "&result=id,teryt,parcel,region,commune,county,geom_wkt"
        End If

        Try
            Using odp = Await _klient.GetAsync(url, token).ConfigureAwait(False)
                odp.EnsureSuccessStatusCode()
                Dim tekst = Await odp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Dim linie = tekst.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
                If linie.Length < 2 Then Return wynik

                Dim status = linie(0).Trim()
                If status.StartsWith("-") Then Return wynik ' błąd lub brak wyników

                For i = 1 To linie.Length - 1
                    Dim d = SparsujWiersz(linie(i))
                    If d IsNot Nothing AndAlso d.Zasieg1992.Poprawny Then
                        wynik.Add(d)
                    End If
                Next
            End Using
        Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException)
            ' Błąd sieciowy lub timeout
        End Try

        Return wynik
    End Function

    ''' <summary>Parsuje pojedynczy wiersz odpowiedzi z ULDK rozdzielany pionową kreską '|'.</summary>
    Public Shared Function SparsujWiersz(wiersz As String) As DzialkaKatastralna
        If String.IsNullOrWhiteSpace(wiersz) Then Return Nothing
        Dim czesci = wiersz.Split("|"c)
        If czesci.Length < 6 Then Return Nothing

        Dim d As New DzialkaKatastralna With {
            .Identyfikator = czesci(0).Trim(),
            .Numer = czesci(2).Trim(),
            .Obreb = czesci(3).Trim(),
            .Gmina = czesci(4).Trim(),
            .Powiat = czesci(5).Trim()
        }

        Dim wkt = If(czesci.Length > 6, czesci(6).Trim(), "")
        If wkt.StartsWith("SRID=", StringComparison.OrdinalIgnoreCase) Then
            Dim idx = wkt.IndexOf(";"c)
            If idx >= 0 Then wkt = wkt.Substring(idx + 1).Trim()
        End If
        d.Wkt = wkt

        If Not String.IsNullOrEmpty(wkt) Then
            SparsujGeometrieWkt(wkt, d)
        End If

        Return d
    End Function

    ''' <summary>
    ''' Odczytuje POLYGON i MULTIPOLYGON: pierścienie zewnętrzne trafiają do ObrysyWgs84 (pierwszy także do ObrysWgs84),
    ''' otwory są pomijane. Zasięg obejmuje wszystkie części działki. Wcześniej tekst między pierwszym "((" a ostatnim
    ''' "))" był dzielony tylko przecinkami, więc wierzchołki przy nawiasach ginęły, a części działki łączyły się w jeden obrys.
    ''' </summary>
    Private Shared Sub SparsujGeometrieWkt(wkt As String, d As DzialkaKatastralna)
        Dim ci = CultureInfo.InvariantCulture
        Dim minX = Double.MaxValue, maxX = Double.MinValue
        Dim minY = Double.MaxValue, maxY = Double.MinValue
        Dim multi = wkt.TrimStart().StartsWith("MULTIPOLYGON", StringComparison.OrdinalIgnoreCase)
        Dim poziomPierscienia = If(multi, 3, 2)

        Dim glebokosc = 0
        Dim pierwszyWWielokacie = True
        Dim tekstPierscienia As New System.Text.StringBuilder()
        For Each znak In wkt
            If znak = "("c Then
                glebokosc += 1
                If glebokosc = poziomPierscienia Then tekstPierscienia.Clear()
                If glebokosc = poziomPierscienia - 1 Then pierwszyWWielokacie = True
            ElseIf znak = ")"c Then
                If glebokosc = poziomPierscienia Then
                    If pierwszyWWielokacie Then
                        ' WKT dla EPSG:2180: "wschodnia północna"; w MapoTero X - północ, Y - wschód
                        Dim pierscien As New List(Of PunktGeo)
                        For Each pt In tekstPierscienia.ToString().Split(","c)
                            Dim xy = pt.Trim().Split(New Char() {" "c, ChrW(9)}, StringSplitOptions.RemoveEmptyEntries)
                            Dim yEasting As Double, xNorthing As Double
                            If xy.Length >= 2 AndAlso Double.TryParse(xy(0), NumberStyles.Float, ci, yEasting) AndAlso
                               Double.TryParse(xy(1), NumberStyles.Float, ci, xNorthing) Then
                                minX = Math.Min(minX, xNorthing) : maxX = Math.Max(maxX, xNorthing)
                                minY = Math.Min(minY, yEasting) : maxY = Math.Max(maxY, yEasting)
                                pierscien.Add(UkladWspolrzednych.PL1992.DoWgs84(New PunktXY(xNorthing, yEasting)))
                            End If
                        Next
                        If pierscien.Count > 0 Then
                            d.ObrysyWgs84.Add(pierscien)
                            If d.ObrysWgs84.Count = 0 Then d.ObrysWgs84.AddRange(pierscien)
                        End If
                    End If
                    pierwszyWWielokacie = False
                End If
                glebokosc -= 1
            ElseIf glebokosc = poziomPierscienia Then
                tekstPierscienia.Append(znak)
            End If
        Next

        If minX < Double.MaxValue AndAlso maxX > Double.MinValue Then
            d.Zasieg1992 = New Zasieg(minX, minY, maxX, maxY)
        End If
    End Sub

End Class
