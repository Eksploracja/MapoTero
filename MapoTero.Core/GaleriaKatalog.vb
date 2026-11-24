'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>Opis zasięgu przestrzennego warstwy w spisie galerii (<c>warstwy.json</c>).</summary>
Public Class ZasiegWarstwyGalerii
    <JsonPropertyName("typ")>
    Public Property Typ As String = "kraj"

    <JsonPropertyName("kod")>
    Public Property Kod As String = ""

    <JsonPropertyName("etykieta")>
    Public Property Etykieta As String = ""

    <JsonPropertyName("opis")>
    Public Property Opis As String = ""

    <JsonPropertyName("procent_pl")>
    Public Property ProcentPl As Integer?

    <JsonPropertyName("procent_total")>
    Public Property ProcentTotal As Integer?

    <JsonPropertyName("komorki_total")>
    Public Property KomorkiTotal As Integer?

    <JsonPropertyName("maska_hex")>
    Public Property MaskaHex As String = ""

    <JsonPropertyName("clip_polska")>
    Public Property ClipPolska As Boolean?

    <JsonPropertyName("bbox")>
    Public Property Bbox As Double()
End Class

''' <summary>Pojedyncza warstwa z pliku <c>pomoc/galeria/warstwy.json</c>.</summary>
Public Class PozycjaGaleriiWarstw
    <JsonPropertyName("id")>
    Public Property Id As String = ""

    <JsonPropertyName("zbior")>
    Public Property Zbior As String = ""

    <JsonPropertyName("warstwa")>
    Public Property Warstwa As String = ""

    <JsonPropertyName("nazwa")>
    Public Property Nazwa As String = ""

    <JsonPropertyName("identyfikator")>
    Public Property Identyfikator As String = ""

    <JsonPropertyName("grupa")>
    Public Property Grupa As String = ""

    <JsonPropertyName("podgrupa")>
    Public Property Podgrupa As String = ""

    <JsonPropertyName("typ")>
    Public Property Typ As String = "WMS"

    <JsonPropertyName("nakladka")>
    Public Property Nakladka As Boolean

    <JsonPropertyName("zalecany_piksel")>
    Public Property ZalecanyPiksel As Double?

    <JsonPropertyName("zrodlo")>
    Public Property Zrodlo As String = ""

    <JsonPropertyName("obrazek")>
    Public Property Obrazek As String = ""

    <JsonPropertyName("miejsce_probki")>
    Public Property MiejsceProbki As String = ""

    <JsonPropertyName("zasieg")>
    Public Property Zasieg As ZasiegWarstwyGalerii
End Class

''' <summary>Zawartość pliku <c>warstwy.json</c> udostępnianego przez galerię online.</summary>
Public Class KatalogGaleriiWarstw
    Public Property Wygenerowano As String = ""
    Public Property WersjaProgramu As String = ""
    Public ReadOnly Property Pozycje As New List(Of PozycjaGaleriiWarstw)()

    Private ReadOnly _indeks As New Dictionary(Of String, PozycjaGaleriiWarstw)(StringComparer.OrdinalIgnoreCase)

    Private Class SurowyPlikGalerii
        <JsonPropertyName("wygenerowano")>
        Public Property Wygenerowano As String = ""

        <JsonPropertyName("wersja_programu")>
        Public Property WersjaProgramu As String = ""

        <JsonPropertyName("warstwy")>
        Public Property Warstwy As List(Of PozycjaGaleriiWarstw)
    End Class

    Public Shared Function Klucz(zbior As String, warstwa As String) As String
        Return (If(zbior, "").Trim() & "|" & If(warstwa, "").Trim()).ToLowerInvariant()
    End Function

    ''' <summary>Wyszukuje warstwę w katalogu galerii po nazwie pliku zbioru (bez .txt) i nazwie technicznej warstwy.</summary>
    Public Function Znajdz(zbior As String, warstwa As String) As PozycjaGaleriiWarstw
        If String.IsNullOrWhiteSpace(zbior) OrElse String.IsNullOrWhiteSpace(warstwa) Then Return Nothing
        Dim p As PozycjaGaleriiWarstw = Nothing
        If _indeks.TryGetValue(Klucz(zbior, warstwa), p) Then Return p
        Return Nothing
    End Function

    ''' <summary>Czy warstwa występuje w wczytanym katalogu galerii.</summary>
    Public Function Zawiera(zbior As String, warstwa As String) As Boolean
        Return Znajdz(zbior, warstwa) IsNot Nothing
    End Function

    ''' <summary>Parsuje treść pliku <c>warstwy.json</c>; zwraca pusty katalog przy błędnym albo pustym JSON-ie.</summary>
    Public Shared Function ZTekstu(json As String) As KatalogGaleriiWarstw
        Dim wynik As New KatalogGaleriiWarstw()
        If String.IsNullOrWhiteSpace(json) Then Return wynik
        Try
            Dim opcje As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True
            }
            Dim surowy = JsonSerializer.Deserialize(Of SurowyPlikGalerii)(json, opcje)
            If surowy Is Nothing Then Return wynik
            wynik.Wygenerowano = If(surowy.Wygenerowano, "")
            wynik.WersjaProgramu = If(surowy.WersjaProgramu, "")
            If surowy.Warstwy IsNot Nothing Then
                For Each p In surowy.Warstwy
                    If p Is Nothing OrElse String.IsNullOrWhiteSpace(p.Zbior) OrElse String.IsNullOrWhiteSpace(p.Warstwa) Then Continue For
                    wynik.Pozycje.Add(p)
                    Dim k = Klucz(p.Zbior, p.Warstwa)
                    If Not wynik._indeks.ContainsKey(k) Then
                        wynik._indeks(k) = p
                    End If
                Next
            End If
        Catch
            Return New KatalogGaleriiWarstw()
        End Try
        Return wynik
    End Function
End Class

''' <summary>Prostokąt w układzie współrzędnych podglądu zasięgu (viewBox 25..340 × 0..275).</summary>
Public Structure ProstokatMapyGalerii
    Public ReadOnly X As Single
    Public ReadOnly Y As Single
    Public ReadOnly Szerokosc As Single
    Public ReadOnly Wysokosc As Single

    Public Sub New(x As Single, y As Single, szerokosc As Single, wysokosc As Single)
        Me.X = x
        Me.Y = y
        Me.Szerokosc = szerokosc
        Me.Wysokosc = wysokosc
    End Sub
End Structure

''' <summary>Punkt w układzie współrzędnych podglądu zasięgu.</summary>
Public Structure PunktMapyGalerii
    Public ReadOnly X As Single
    Public ReadOnly Y As Single

    Public Sub New(x As Single, y As Single)
        Me.X = x
        Me.Y = y
    End Sub
End Structure

''' <summary>Etykieta miasta orientacyjnego na podglądzie zasięgu.</summary>
Public Class MiastoMapyGalerii
    Public ReadOnly Property Nazwa As String
    Public ReadOnly Property X As Single
    Public ReadOnly Property Y As Single

    Public Sub New(nazwa As String, x As Single, y As Single)
        Me.Nazwa = nazwa
        Me.X = x
        Me.Y = y
    End Sub
End Class

''' <summary>
''' Geometria i dekoder zasięgu z galerii warstw (zgodna z <c>pomoc/galeria/galeria.js</c> i <c>maska_hex</c>
''' w <c>warstwy.json</c> — uwzględnia rzeczywiste pokrycie i „dziury” wewnątrz Polski).
''' </summary>
Public NotInheritable Class GeometriaZasieguGalerii

    Private Sub New()
    End Sub

    Public Const Skala As Double = 30.9524
    Public Const PrzesuniecieX As Double = 42.46
    Public Const PrzesuniecieY As Double = 10.0
    Public Const CosLat0 As Double = 0.61291
    Public Const LonMin As Double = 11.0
    Public Const LatMax As Double = 56.4

    Public Const LiczbaKolumnMaski As Integer = 28
    Public Const LiczbaWierszyMaski As Integer = 20

    Public Shared ReadOnly Polska As PunktMapyGalerii() = ParsujPunkty("104.1,86.5 108.9,88.9 110.8,84.3 128.8,78.7 140.7,71.3 153.4,65.7 166.7,59.5 180.9,58.3 190.4,60.1 186.6,65.7 184.7,68.8 188.2,72.8 194.2,73.5 206.6,70.7 210.4,70.4 222.7,73.1 241.7,74.1 266.3,73.1 277.7,78.1 281.5,85.8 288.1,90.5 283.4,106.0 287.2,121.4 281.5,140.0 279.6,161.7 291.9,181.2 289.7,194.2 279.6,200.4 270.1,205.0 264.4,220.5 265.4,228.2 267.3,239.0 260.6,234.4 247.3,229.8 237.9,225.1 226.5,226.7 213.2,232.9 209.4,232.2 199.9,223.6 193.3,222.0 191.0,223.0 184.7,212.7 180.9,208.1 178.1,205.0 170.5,200.4 165.8,200.4 160.1,197.3 154.4,194.2 144.9,201.9 141.1,194.2 135.4,186.4 127.8,183.3 124.0,181.8 118.3,181.2 114.9,177.8 118.0,169.4 117.4,158.6 113.6,147.7 110.8,143.1 109.8,133.8 107.9,124.5 101.7,120.2 106.0,107.5 108.9,102.9 105.1,96.7")
    Public Shared ReadOnly Baltyk As PunktMapyGalerii() = ParsujPunkty("42.5,10.0 317.5,10.0 317.5,68.8 266.3,73.1 222.7,73.1 210.4,70.4 194.2,73.5 188.2,72.8 184.7,68.8 186.6,65.7 190.4,60.1 180.9,58.3 166.7,59.5 153.4,65.7 140.7,71.3 128.8,78.7 110.8,84.3 104.1,86.5 42.5,84.3")
    Public Shared ReadOnly Czechy As PunktMapyGalerii() = ParsujPunkty("114.9,177.8 118.3,181.2 124.0,181.8 127.8,183.3 135.4,186.4 141.1,194.2 144.9,201.9 154.4,194.2 160.1,197.3 165.8,200.4 170.5,200.4 178.1,205.0 180.9,208.1 184.7,212.7 191.0,223.0 186.6,232.9 165.8,245.2 154.4,251.4 118.3,251.4 99.4,251.4 80.4,239.0 63.3,205.0 67.1,198.8 80.4,192.6 103.2,183.3")
    Public Shared ReadOnly Slowacja As PunktMapyGalerii() = ParsujPunkty("191.0,223.0 193.3,222.0 199.9,223.6 209.4,232.2 213.2,232.9 226.5,226.7 237.9,225.1 247.3,229.8 260.6,234.4 267.3,239.0 260.6,257.6 241.7,257.6 194.2,266.9 165.8,270.0 154.4,251.4 165.8,245.2 186.6,232.9")
    Public Shared ReadOnly NiemcyBrandenburgia As PunktMapyGalerii() = ParsujPunkty("101.7,120.2 107.9,124.5 109.8,133.8 110.8,143.1 113.6,147.7 117.4,158.6 108.9,164.8 89.9,164.8 80.4,155.5 65.2,146.2 57.6,121.4 57.6,109.0 76.6,99.8 105.1,96.7 108.9,102.9 106.0,107.5")
    Public Shared ReadOnly NiemcySaksonia As PunktMapyGalerii() = ParsujPunkty("117.4,158.6 118.0,169.4 114.9,177.8 103.2,183.3 80.4,192.6 67.1,198.8 63.3,205.0 65.2,183.3 67.1,167.9 80.4,158.6 89.9,164.8 108.9,164.8")
    Public Shared ReadOnly NiemcyReszta As PunktMapyGalerii() = ParsujPunkty("42.5,84.3 104.1,86.5 105.1,96.7 76.6,99.8 57.6,109.0 57.6,121.4 65.2,146.2 80.4,155.5 89.9,164.8 80.4,158.6 67.1,167.9 65.2,183.3 63.3,205.0 42.5,201.9")
    Public Shared ReadOnly Litwa As PunktMapyGalerii() = ParsujPunkty("266.3,73.1 277.7,78.1 281.5,85.8 317.5,84.3 317.5,10.0 232.2,10.0 230.3,44.0")

    Public Shared ReadOnly Wisla As PunktMapyGalerii() = ParsujPunkty("194.2,220.5 198.0,211.2 211.3,206.5 224.6,201.9 247.3,186.4 249.2,164.8 236.0,143.1 232.2,136.9 220.8,133.8 194.2,127.6 186.6,112.1 184.7,99.8 188.5,75.0 193.3,73.5")
    Public Shared ReadOnly Odra As PunktMapyGalerii() = ParsujPunkty("167.7,208.1 173.4,198.8 173.4,186.4 156.3,174.0 141.1,158.6 127.8,146.2 110.8,143.1 109.8,133.8 107.9,124.5 101.7,120.2 106.0,107.5 108.9,102.9 110.8,90.5")

    Public Shared ReadOnly Miasta As MiastoMapyGalerii() = {
        New MiastoMapyGalerii("Warszawa", 232.4F, 139.1F),
        New MiastoMapyGalerii("Kraków", 212.1F, 206.2F),
        New MiastoMapyGalerii("Gdańsk", 187.6F, 73.5F),
        New MiastoMapyGalerii("Wrocław", 156.9F, 173.7F),
        New MiastoMapyGalerii("Poznań", 155.0F, 133.5F)
    }

    Public Shared ReadOnly MiastaTrueOrtho As MiastoMapyGalerii() = {
        New MiastoMapyGalerii("Warszawa", 232.4F, 139.1F),
        New MiastoMapyGalerii("Kraków", 212.1F, 206.2F),
        New MiastoMapyGalerii("Trójmiasto", 187.6F, 73.5F),
        New MiastoMapyGalerii("Wrocław", 156.9F, 173.7F),
        New MiastoMapyGalerii("Poznań", 155.0F, 133.5F),
        New MiastoMapyGalerii("Łódź", 207.7F, 151.7F),
        New MiastoMapyGalerii("GOP / Katowice", 202.9F, 196.9F)
    }

    Private Shared Function ParsujPunkty(ciag As String) As PunktMapyGalerii()
        Dim czesci = ciag.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim lista As New List(Of PunktMapyGalerii)(czesci.Length)
        For Each c In czesci
            Dim xy = c.Split(","c)
            If xy.Length = 2 Then
                Dim x = Single.Parse(xy(0), CultureInfo.InvariantCulture)
                Dim y = Single.Parse(xy(1), CultureInfo.InvariantCulture)
                lista.Add(New PunktMapyGalerii(x, y))
            End If
        Next
        Return lista.ToArray()
    End Function

    ''' <summary>Przelicza współrzędne WGS84 (długość, szerokość) na układ mapy poglądowej.</summary>
    Public Shared Function GeoDoMapy(lon As Double, lat As Double) As PunktMapyGalerii
        Dim x = CSng(PrzesuniecieX + (lon - LonMin) * CosLat0 * Skala)
        Dim y = CSng(PrzesuniecieY + (LatMax - lat) * Skala)
        Return New PunktMapyGalerii(x, y)
    End Function

    ''' <summary>
    ''' Dekoduje 140-znakowy zapis <c>maska_hex</c> (siatka 28×20 komórek w zakresie 14.0..24.2° E, 49.0..55.0° N)
    ''' na prostokąty komórek gotowe do narysowania na mapie zasięgu.
    ''' </summary>
    Public Shared Function DekodujMaskeHex(maskaHex As String) As List(Of ProstokatMapyGalerii)
        Dim komorki As New List(Of ProstokatMapyGalerii)()
        If String.IsNullOrWhiteSpace(maskaHex) OrElse maskaHex.Length <> 140 Then Return komorki

        Const dLon As Double = 10.2 / LiczbaKolumnMaski
        Const dLat As Double = 6.0 / LiczbaWierszyMaski

        For b As Integer = 0 To 69
            Dim bajt As Integer
            If Not Integer.TryParse(maskaHex.AsSpan(b * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, bajt) Then
                Continue For
            End If
            For bit As Integer = 0 To 7
                If (bajt And (1 << (7 - bit))) <> 0 Then
                    Dim idx As Integer = b * 8 + bit
                    Dim r As Integer = idx \ LiczbaKolumnMaski
                    Dim c As Integer = idx Mod LiczbaKolumnMaski
                    Dim lon1 As Double = 14.0 + c * dLon
                    Dim lon2 As Double = lon1 + dLon
                    Dim latTop As Double = 55.0 - r * dLat
                    Dim latBottom As Double = latTop - dLat
                    Dim p1 = GeoDoMapy(lon1, latTop)
                    Dim p2 = GeoDoMapy(lon2, latBottom)
                    komorki.Add(New ProstokatMapyGalerii(
                        Math.Min(p1.X, p2.X),
                        Math.Min(p1.Y, p2.Y),
                        Math.Abs(p2.X - p1.X) + 0.35F,
                        Math.Abs(p2.Y - p1.Y) + 0.35F))
                End If
            Next
        Next
        Return komorki
    End Function

    ''' <summary>Opis szczegółowy zasięgu wyświetlany pod mapą poglądową.</summary>
    Public Shared Function OpisZasiegu(p As PozycjaGaleriiWarstw) As String
        Dim z = If(p?.Zasieg, New ZasiegWarstwyGalerii())
        If Not String.IsNullOrWhiteSpace(z.Opis) Then Return z.Opis
        Dim typ = If(z.Typ, "kraj").ToLowerInvariant()
        Select Case typ
            Case "kraj"
                Return "Warstwa obejmuje obszar całej Polski (pokrycie ogólnokrajowe)."
            Case "zagranica"
                Return "Urzędowa warstwa zagraniczna (" & If(Not String.IsNullOrWhiteSpace(z.Kod), z.Kod, "sąsiedzi") & ")."
            Case "morski"
                Return "Polskie obszary morskie, wyłączna strefa ekonomiczna (WZE) lub dane batymetryczne Bałtyku."
            Case "czesciowy"
                Return "Pokrycie częściowe — warstwa dostępna wyłącznie dla wybranych obszarów kraju."
            Case "regionalny"
                If z.Bbox IsNot Nothing AndAlso z.Bbox.Length = 4 Then
                    Return "Zasięg regionalny ograniczony do wybranego obszaru."
                End If
                Return "Zasięg regionalny obejmujący wybrane obszary."
            Case "globalny"
                Return "Warstwa o zasięgu globalnym (pokrycie całego świata)."
            Case Else
                Return If(Not String.IsNullOrWhiteSpace(z.Etykieta), z.Etykieta, "Cała Polska")
        End Select
    End Function
End Class
