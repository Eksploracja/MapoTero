'Copyright (C) <2026>  MapoTero
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Informacje o znalezionej miejscowości, adresie lub obiekcie geograficznym (z geokodera OpenStreetMap Nominatim).
''' </summary>
Public Class WynikGeokodowania
    Public Property Nazwa As String = ""
    Public Property PelnaNazwa As String = ""
    Public Property Kategoria As String = ""
    Public Property Typ As String = ""
    Public Property Szerokosc As Double
    Public Property Dlugosc As Double
    Public Property MinSzerokosc As Double
    Public Property MaxSzerokosc As Double
    Public Property MinDlugosc As Double
    Public Property MaxDlugosc As Double

    ''' <summary>Zwraca True, jeśli obiekt posiada niezerowy prostokątny zasięg (obszar).</summary>
    Public ReadOnly Property MaObszar As Boolean
        Get
            Return MaxSzerokosc > MinSzerokosc AndAlso MaxDlugosc > MinDlugosc
        End Get
    End Property

    ''' <summary>Czytelny polski opis typu obiektu geograficznego.</summary>
    Public ReadOnly Property TypOpis As String
        Get
            Select Case Typ.ToLowerInvariant()
                Case "city" : Return "miasto"
                Case "town" : Return "miejscowość"
                Case "village" : Return "wieś"
                Case "hamlet" : Return "przysiółek / osada"
                Case "suburb", "neighbourhood" : Return "dzielnica"
                Case "administrative" : Return "obszar administracyjny"
                Case "road", "residential", "street", "primary", "secondary", "tertiary", "motorway", "trunk" : Return "ulica / droga"
                Case "house", "building" : Return "adres / budynek"
                Case "peak" : Return "szczyt górski"
                Case "water", "lake" : Return "jezioro / woda"
                Case "forest" : Return "las"
                Case Else
                    Select Case Kategoria.ToLowerInvariant()
                        Case "place" : Return "miejsce"
                        Case "highway" : Return "droga / ulica"
                        Case "building" : Return "budynek"
                        Case "amenity" : Return "obiekt użyteczności"
                        Case "natural" : Return "obiekt przyrodniczy"
                        Case "boundary" : Return "granica"
                        Case Else : Return Typ
                    End Select
            End Select
        End Get
    End Property

    ''' <summary>Sformatowane współrzędne w formacie dziesiętnym ze znakami kierunku.</summary>
    Public ReadOnly Property WspolrzedneTekst As String
        Get
            Dim nS = If(Szerokosc >= 0, "N", "S")
            Dim eW = If(Dlugosc >= 0, "E", "W")
            Return String.Format(CultureInfo.InvariantCulture, "{0:0.0000}°{1}, {2:0.0000}°{3}",
                                 Math.Abs(Szerokosc), nS, Math.Abs(Dlugosc), eW)
        End Get
    End Property

    ''' <summary>Krótka nazwa (pierwszy człon przed przecinkiem z pełnej nazwy).</summary>
    Public ReadOnly Property KrotkaNazwa As String
        Get
            If Not String.IsNullOrEmpty(Nazwa) Then Return Nazwa
            If Not String.IsNullOrEmpty(PelnaNazwa) Then
                Dim idx = PelnaNazwa.IndexOf(","c)
                Return If(idx > 0, PelnaNazwa.Substring(0, idx).Trim(), PelnaNazwa.Trim())
            End If
            Return ""
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return If(Not String.IsNullOrEmpty(PelnaNazwa), PelnaNazwa, Nazwa)
    End Function
End Class

''' <summary>
''' Klient geokodowania miejscowości i adresów (OpenStreetMap Nominatim).
''' </summary>
Public NotInheritable Class KlientGeokodowania

    Private Shared ReadOnly _klient As HttpClient = UtworzKlienta()

    Private Sub New()
    End Sub

    Private Shared Function UtworzKlienta() As HttpClient
        Dim h As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(15)}
        Dim ua = WersjaProgramu.UserAgent(GetType(KlientGeokodowania).Assembly) & " (https://github.com/eksploracja/MapoTero)"
        h.DefaultRequestHeaders.Add("User-Agent", ua)
        Return h
    End Function

    ''' <summary>
    ''' Wyszukuje miejscowości, adresy lub obiekty w geokoderze OpenStreetMap Nominatim.
    ''' </summary>
    Public Shared Async Function SzukajMiejscaAsync(zapytanie As String,
                                                    Optional token As CancellationToken = Nothing) As Task(Of List(Of WynikGeokodowania))
        Dim wynik As New List(Of WynikGeokodowania)()
        If String.IsNullOrWhiteSpace(zapytanie) Then Return wynik

        Dim czyste = zapytanie.Trim()
        Dim url = "https://nominatim.openstreetmap.org/search?q=" & Uri.EscapeDataString(czyste) &
                  "&format=json&addressdetails=1&limit=15&accept-language=pl,en"

        'błąd sieci, limitu czasu lub odpowiedź HTTP z błędem trafia do wywołującego (okno wyszukiwarki pokazuje komunikat) -
        'dawniej był ignorowany i brak internetu wyglądał jak brak wyników
        Try
            Using odp = Await _klient.GetAsync(url, token).ConfigureAwait(False)
                odp.EnsureSuccessStatusCode()
                Dim jsonTekst = Await odp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Return ParsujWynikiJson(jsonTekst)
            End Using
        Catch ex As TaskCanceledException When Not token.IsCancellationRequested
            Throw New TimeoutException("przekroczono czas oczekiwania na odpowiedź geokodera", ex)
        End Try
    End Function

    ''' <summary>Parsuje tablicę JSON zwróconą przez Nominatim do listy obiektów WynikGeokodowania.</summary>
    Public Shared Function ParsujWynikiJson(jsonTekst As String) As List(Of WynikGeokodowania)
        Dim lista As New List(Of WynikGeokodowania)()
        If String.IsNullOrWhiteSpace(jsonTekst) Then Return lista

        Try
            Using doc = JsonDocument.Parse(jsonTekst)
                If doc.RootElement.ValueKind <> JsonValueKind.Array Then Return lista

                For Each el In doc.RootElement.EnumerateArray()
                    If el.ValueKind <> JsonValueKind.Object Then Continue For

                    Dim item As New WynikGeokodowania()
                    item.PelnaNazwa = PobierzTekst(el, "display_name")
                    item.Nazwa = PobierzTekst(el, "name")
                    item.Kategoria = PobierzTekst(el, "class")
                    item.Typ = PobierzTekst(el, "type")

                    Dim latStr = PobierzTekst(el, "lat")
                    Dim lonStr = PobierzTekst(el, "lon")
                    Dim lat As Double, lon As Double
                    If Double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, lat) Then
                        item.Szerokosc = lat
                    End If
                    If Double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, lon) Then
                        item.Dlugosc = lon
                    End If

                    Dim bbEl As JsonElement
                    If el.TryGetProperty("boundingbox", bbEl) AndAlso bbEl.ValueKind = JsonValueKind.Array AndAlso bbEl.GetArrayLength() >= 4 Then
                        Dim minLatStr = bbEl(0).GetString()
                        Dim maxLatStr = bbEl(1).GetString()
                        Dim minLonStr = bbEl(2).GetString()
                        Dim maxLonStr = bbEl(3).GetString()

                        Dim minLat As Double, maxLat As Double, minLon As Double, maxLon As Double
                        If Double.TryParse(minLatStr, NumberStyles.Float, CultureInfo.InvariantCulture, minLat) AndAlso
                           Double.TryParse(maxLatStr, NumberStyles.Float, CultureInfo.InvariantCulture, maxLat) Then
                            item.MinSzerokosc = minLat
                            item.MaxSzerokosc = maxLat
                        End If
                        If Double.TryParse(minLonStr, NumberStyles.Float, CultureInfo.InvariantCulture, minLon) AndAlso
                           Double.TryParse(maxLonStr, NumberStyles.Float, CultureInfo.InvariantCulture, maxLon) Then
                            item.MinDlugosc = minLon
                            item.MaxDlugosc = maxLon
                        End If
                    End If

                    If String.IsNullOrEmpty(item.Nazwa) Then
                        item.Nazwa = item.KrotkaNazwa
                    End If

                    lista.Add(item)
                Next
            End Using
        Catch
            ' Błąd parsowania JSON
        End Try

        Return lista
    End Function

    Private Shared Function PobierzTekst(obj As JsonElement, nazwa As String) As String
        Dim el As JsonElement
        If obj.TryGetProperty(nazwa, el) AndAlso el.ValueKind = JsonValueKind.String Then
            Return If(el.GetString(), "").Trim()
        End If
        Return ""
    End Function

End Class
