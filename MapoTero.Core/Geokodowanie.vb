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
Imports System.Text.RegularExpressions
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
                Case "wspolrzedne" : Return "współrzędne"
                Case "dzialka" : Return "działka ewidencyjna"
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
    ''' Normalizuje zapytanie adresowe: rozdziela sklejone przecinki i kropki, usuwa zbędny w OSM prefiks ul.
    ''' oraz obcina numery lokali/mieszkań, które uniemożliwiają dopasowanie w bazach geokodowania.
    ''' </summary>
    Public Shared Function NormalizujZapytanieAdresowe(zapytanie As String) As String
        If String.IsNullOrWhiteSpace(zapytanie) Then Return ""

        Dim s = zapytanie.Trim()

        ' Rozdziel przecinek sklejony ze słowem: "Gdynia,ul." -> "Gdynia, ul."
        s = Regex.Replace(s, ",([^\s])", ", $1")

        ' Rozdziel kropkę skrótu ul. sklejoną z nazwą: "ul.Piotrkowska" -> "ul. Piotrkowska"
        s = Regex.Replace(s, "(?i)\bul\.([^\s])", "ul. $1")

        ' Usuń numery mieszkań i lokali: "12 m. 4", "12 m 4", "12 lok. 5", "12/4" -> "12"
        s = Regex.Replace(s, "(?i)(\b\d+[a-ząćęłńóśźż]?)\s*(?:m\.|m|lok\.|lokal|mieszkania|nr)\s*\d+\b", "$1")
        s = Regex.Replace(s, "(\b\d+[a-ząćęłńóśźż]?)\s*/\s*\d+\b", "$1")

        ' Usuń prefiks ulicy (ul., ulica, ulicy)
        s = Regex.Replace(s, "(?i)(?:^|[\s,])ulic[aey]\s+", " ")
        s = Regex.Replace(s, "(?i)(?:^|[\s,])ul\.\s*", " ")
        s = Regex.Replace(s, "(?i)(?:^|[\s,])ul\s+", " ")

        ' Uporządkuj spacje i przecinki
        s = Regex.Replace(s, "\s*,\s*", ", ")
        s = Regex.Replace(s, "\s+", " ").Trim(" "c, ","c)

        Return s
    End Function

    ''' <summary>
    ''' Wyszukuje miejscowości, adresy lub obiekty geograficzne.
    ''' W pierwszej kolejności korzysta z szybkiego geokodera Photon (darmowy silnik dla OSM oparty o Elasticsearch,
    ''' zoptymalizowany pod podpowiedzi w trakcie pisania, tolerujący literówki i przedrostki).
    ''' W razie problemów sieciowych lub braku wyników płynnie przełącza się na geokoder Nominatim.
    ''' </summary>
    Public Shared Async Function SzukajMiejscaAsync(zapytanie As String,
                                                    Optional token As CancellationToken = Nothing,
                                                    Optional srodekLat As Double? = Nothing,
                                                    Optional srodekLon As Double? = Nothing) As Task(Of List(Of WynikGeokodowania))
        Dim wynik As New List(Of WynikGeokodowania)()
        If String.IsNullOrWhiteSpace(zapytanie) Then Return wynik

        Dim czyste = zapytanie.Trim()
        Dim znormalizowane = NormalizujZapytanieAdresowe(czyste)
        Dim frazaDoSzukania = If(Not String.IsNullOrWhiteSpace(znormalizowane), znormalizowane, czyste)

        ' 1. Próba: Photon (szybki, tolerancyjny, uwzględnia przybliżone położenie mapy jako lokalny bias)
        Try
            Dim wynikiPhoton = Await SzukajPhotonAsync(frazaDoSzukania, srodekLat, srodekLon, limit:=15, token:=token).ConfigureAwait(False)
            If wynikiPhoton IsNot Nothing AndAlso wynikiPhoton.Count > 0 Then
                Return wynikiPhoton
            End If
            ' Jeśli po znormalizowaniu nic nie ma, a surowe zapytanie było inne, spróbuj surowego w Photonie
            If Not String.Equals(frazaDoSzukania, czyste, StringComparison.OrdinalIgnoreCase) Then
                Dim wynikiPhotonSurowe = Await SzukajPhotonAsync(czyste, srodekLat, srodekLon, limit:=15, token:=token).ConfigureAwait(False)
                If wynikiPhotonSurowe IsNot Nothing AndAlso wynikiPhotonSurowe.Count > 0 Then
                    Return wynikiPhotonSurowe
                End If
            End If
        Catch ex As OperationCanceledException
            Throw
        Catch
            ' Błąd sieci Photon - przechodzimy do Nominatim
        End Try

        ' 2. Próba (fallback): Nominatim z oczyszczoną frazą
        Try
            Dim wynikiNom = Await SzukajNominatimAsync(frazaDoSzukania, token).ConfigureAwait(False)
            If wynikiNom IsNot Nothing AndAlso wynikiNom.Count > 0 Then
                Return wynikiNom
            End If
        Catch ex As OperationCanceledException
            Throw
        Catch
            ' Błąd Nominatim
        End Try

        ' 3. Próba: Nominatim z surowym zapytaniem
        If Not String.Equals(frazaDoSzukania, czyste, StringComparison.OrdinalIgnoreCase) Then
            Try
                Dim wynikiNomSurowe = Await SzukajNominatimAsync(czyste, token).ConfigureAwait(False)
                If wynikiNomSurowe IsNot Nothing AndAlso wynikiNomSurowe.Count > 0 Then
                    Return wynikiNomSurowe
                End If
            Catch ex As OperationCanceledException
                Throw
            Catch
            End Try
        End If

        Return wynik
    End Function

    ''' <summary>
    ''' Wyszukuje obiekty w geokoderze Photon (zoptymalizowanym pod wyszukiwanie w trakcie pisania).
    ''' </summary>
    Public Shared Async Function SzukajPhotonAsync(zapytanie As String,
                                                   Optional lat As Double? = Nothing,
                                                   Optional lon As Double? = Nothing,
                                                   Optional limit As Integer = 15,
                                                   Optional token As CancellationToken = Nothing) As Task(Of List(Of WynikGeokodowania))
        Dim wynik As New List(Of WynikGeokodowania)()
        If String.IsNullOrWhiteSpace(zapytanie) Then Return wynik

        Dim czyste = zapytanie.Trim()
        Dim url = "https://photon.komoot.io/api?q=" & Uri.EscapeDataString(czyste) &
                  "&limit=" & limit.ToString(CultureInfo.InvariantCulture) & "&lang=default"

        If lat.HasValue AndAlso lon.HasValue AndAlso (lat.Value <> 0.0 OrElse lon.Value <> 0.0) Then
            url &= String.Format(CultureInfo.InvariantCulture, "&lat={0:0.000000}&lon={1:0.000000}", lat.Value, lon.Value)
        End If

        Try
            Using odp = Await _klient.GetAsync(url, token).ConfigureAwait(False)
                odp.EnsureSuccessStatusCode()
                Dim jsonTekst = Await odp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Return ParsujWynikiPhotonJson(jsonTekst)
            End Using
        Catch ex As TaskCanceledException When Not token.IsCancellationRequested
            Throw New TimeoutException("przekroczono czas oczekiwania na odpowiedź geokodera Photon", ex)
        End Try
    End Function

    ''' <summary>
    ''' Wyszukuje obiekty w geokoderze OpenStreetMap Nominatim.
    ''' </summary>
    Public Shared Async Function SzukajNominatimAsync(zapytanie As String,
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

    ''' <summary>
    ''' Parsuje kolekcję obiektów GeoJSON zwróconą przez geokoder Photon do listy obiektów WynikGeokodowania.
    ''' </summary>
    Public Shared Function ParsujWynikiPhotonJson(jsonTekst As String) As List(Of WynikGeokodowania)
        Dim lista As New List(Of WynikGeokodowania)()
        If String.IsNullOrWhiteSpace(jsonTekst) Then Return lista

        Try
            Using doc = JsonDocument.Parse(jsonTekst)
                If doc.RootElement.ValueKind <> JsonValueKind.Object Then Return lista

                Dim featuresEl As JsonElement
                If Not doc.RootElement.TryGetProperty("features", featuresEl) OrElse featuresEl.ValueKind <> JsonValueKind.Array Then
                    Return lista
                End If

                For Each feat In featuresEl.EnumerateArray()
                    If feat.ValueKind <> JsonValueKind.Object Then Continue For

                    Dim geomEl As JsonElement
                    Dim propsEl As JsonElement
                    feat.TryGetProperty("geometry", geomEl)
                    feat.TryGetProperty("properties", propsEl)

                    Dim lon As Double = 0.0, lat As Double = 0.0
                    Dim coordsEl As JsonElement
                    If geomEl.ValueKind = JsonValueKind.Object AndAlso geomEl.TryGetProperty("coordinates", coordsEl) AndAlso coordsEl.ValueKind = JsonValueKind.Array AndAlso coordsEl.GetArrayLength() >= 2 Then
                        Dim pLon = PobierzLiczbe(coordsEl(0))
                        Dim pLat = PobierzLiczbe(coordsEl(1))
                        If pLon.HasValue AndAlso pLat.HasValue Then
                            lon = pLon.Value
                            lat = pLat.Value
                        End If
                    End If

                    If lat = 0.0 AndAlso lon = 0.0 Then Continue For

                    Dim name = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "name"), "")
                    Dim street = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "street"), "")
                    Dim housenumber = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "housenumber"), "")
                    Dim district = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "district"), "")
                    Dim city = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "city"), "")
                    If String.IsNullOrEmpty(city) AndAlso propsEl.ValueKind = JsonValueKind.Object Then city = PobierzTekst(propsEl, "town")
                    If String.IsNullOrEmpty(city) AndAlso propsEl.ValueKind = JsonValueKind.Object Then city = PobierzTekst(propsEl, "village")
                    If String.IsNullOrEmpty(city) AndAlso propsEl.ValueKind = JsonValueKind.Object Then city = PobierzTekst(propsEl, "hamlet")
                    Dim county = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "county"), "")
                    Dim state = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "state"), "")
                    Dim postcode = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "postcode"), "")
                    Dim country = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "country"), "")

                    Dim osmKey = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "osm_key"), "")
                    Dim osmVal = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "osm_value"), "")
                    Dim typeVal = If(propsEl.ValueKind = JsonValueKind.Object, PobierzTekst(propsEl, "type"), "")

                    Dim krotka As String
                    If Not String.IsNullOrEmpty(street) AndAlso Not String.IsNullOrEmpty(housenumber) Then
                        krotka = street & " " & housenumber
                    ElseIf Not String.IsNullOrEmpty(name) Then
                        krotka = name
                    ElseIf Not String.IsNullOrEmpty(street) Then
                        krotka = street
                    ElseIf Not String.IsNullOrEmpty(city) Then
                        krotka = city
                    Else
                        krotka = "Punkt"
                    End If

                    Dim czesci As New List(Of String)()
                    czesci.Add(krotka)
                    If Not String.IsNullOrEmpty(district) AndAlso Not String.Equals(district, krotka, StringComparison.OrdinalIgnoreCase) AndAlso Not String.Equals(district, city, StringComparison.OrdinalIgnoreCase) Then
                        czesci.Add(district)
                    End If
                    If Not String.IsNullOrEmpty(city) AndAlso Not String.Equals(city, krotka, StringComparison.OrdinalIgnoreCase) Then
                        czesci.Add(city)
                    End If
                    If Not String.IsNullOrEmpty(county) AndAlso Not String.Equals(county, city, StringComparison.OrdinalIgnoreCase) AndAlso Not String.Equals(county, state, StringComparison.OrdinalIgnoreCase) Then
                        czesci.Add(county)
                    End If
                    If Not String.IsNullOrEmpty(state) Then czesci.Add(state)
                    If Not String.IsNullOrEmpty(postcode) Then czesci.Add(postcode)
                    If Not String.IsNullOrEmpty(country) Then czesci.Add(country)

                    Dim pelna = String.Join(", ", czesci)

                    Dim item As New WynikGeokodowania With {
                        .Nazwa = krotka,
                        .PelnaNazwa = pelna,
                        .Szerokosc = lat,
                        .Dlugosc = lon,
                        .Kategoria = osmKey,
                        .Typ = If(Not String.IsNullOrEmpty(typeVal) AndAlso Not String.Equals(typeVal, "other", StringComparison.OrdinalIgnoreCase), typeVal, osmVal)
                    }

                    Dim extEl As JsonElement
                    If propsEl.ValueKind = JsonValueKind.Object AndAlso propsEl.TryGetProperty("extent", extEl) AndAlso extEl.ValueKind = JsonValueKind.Array AndAlso extEl.GetArrayLength() >= 4 Then
                        Dim p1 = PobierzLiczbe(extEl(0))
                        Dim p2 = PobierzLiczbe(extEl(1))
                        Dim p3 = PobierzLiczbe(extEl(2))
                        Dim p4 = PobierzLiczbe(extEl(3))
                        If p1.HasValue AndAlso p2.HasValue AndAlso p3.HasValue AndAlso p4.HasValue Then
                            item.MinDlugosc = Math.Min(p1.Value, p3.Value)
                            item.MaxDlugosc = Math.Max(p1.Value, p3.Value)
                            item.MinSzerokosc = Math.Min(p2.Value, p4.Value)
                            item.MaxSzerokosc = Math.Max(p2.Value, p4.Value)
                        End If
                    End If

                    lista.Add(item)
                Next
            End Using
        Catch
            ' Błąd parsowania JSON
        End Try

        Return lista
    End Function

    Private Shared Function PobierzLiczbe(el As JsonElement) As Double?
        If el.ValueKind = JsonValueKind.Number Then
            Dim d As Double
            If el.TryGetDouble(d) Then Return d
        ElseIf el.ValueKind = JsonValueKind.String Then
            Dim d As Double
            If Double.TryParse(el.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                Return d
            End If
        End If
        Return Nothing
    End Function

    Private Shared Function PobierzTekst(obj As JsonElement, nazwa As String) As String
        Dim el As JsonElement
        If obj.TryGetProperty(nazwa, el) AndAlso el.ValueKind = JsonValueKind.String Then
            Return If(el.GetString(), "").Trim()
        End If
        Return ""
    End Function

End Class
