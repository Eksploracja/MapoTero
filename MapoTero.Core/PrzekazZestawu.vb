'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>Pojedyncza warstwa w przepisie kanapki przekazywanym między galerią WWW a MapoTero.</summary>
Public Class PozycjaZestawuGalerii
    <JsonPropertyName("zbior")> Public Property Zbior As String = ""
    <JsonPropertyName("warstwa")> Public Property Warstwa As String = ""
    <JsonPropertyName("krycie")> Public Property Krycie As Integer = ZestawWarstw.PelneKrycie
    <JsonPropertyName("tryb")> Public Property Tryb As String = "Zwykly"
End Class

''' <summary>Przepis zestawu warstw („kanapki”) używany w MostekGalerii, plikach eksportu i schowku.</summary>
Public Class DaneZestawuGalerii
    <JsonPropertyName("format")> Public Property Format As String = PrzekazZestawu.IdentyfikatorFormatu
    <JsonPropertyName("wersja")> Public Property Wersja As Integer = 1
    <JsonPropertyName("nazwa")> Public Property Nazwa As String = ""
    ''' <summary>Cel w programie: "okno_glowne" (domyślnie), "dolacz" albo "kreator".</summary>
    <JsonPropertyName("cel")> Public Property Cel As String = "okno_glowne"
    <JsonPropertyName("zapiszStyl")> Public Property ZapiszStyl As Boolean
    <JsonPropertyName("warstwy")> Public Property Warstwy As New List(Of PozycjaZestawuGalerii)()
End Class

''' <summary>Wynik zmapowania przepisu kanapki na lokalny zestaw warstw programu.</summary>
Public Class WynikBudowyZestawuGalerii
    Public Property Zestaw As New ZestawWarstw()
    Public Property Pominiete As New List(Of String)()
    Public Property PrzycietoDoLimitu As Boolean
    Public Property Nazwa As String = ""
    Public Property Cel As String = "okno_glowne"
    Public Property ZapiszStyl As Boolean
End Class

''' <summary>
''' Serializacja, walidacja i mapowanie „kanapki” warstw między stroną Galerii WWW (JSON / plik / schowek)
''' a strukturami <see cref="ZestawWarstw"/> oraz <see cref="SzablonMapy"/> w MapoTero.
''' </summary>
Public Module PrzekazZestawu

    Public Const IdentyfikatorFormatu As String = "mapotero-zestaw"
    Public Const RozszerzeniePliku As String = ".mapotero.json"

    Private ReadOnly OpcjeJson As JsonSerializerOptions = UtworzOpcjeJson()

    Private Function UtworzOpcjeJson() As JsonSerializerOptions
        Dim o As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .WriteIndented = True,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }
        Return o
    End Function

    ''' <summary>Sprawdza bez rzucania wyjątków, czy podany tekst jest JSON-em przepisu kanapki MapoTero.</summary>
    Public Function CzyWygladaJakZestawGalerii(tekst As String) As Boolean
        If String.IsNullOrWhiteSpace(tekst) Then Return False
        Dim trimmed = tekst.TrimStart()
        If Not trimmed.StartsWith("{", StringComparison.Ordinal) Then Return False
        Try
            Using doc = JsonDocument.Parse(trimmed)
                Dim root = doc.RootElement
                If root.ValueKind <> JsonValueKind.Object Then Return False
                Dim fmt As JsonElement
                If root.TryGetProperty("format", fmt) AndAlso fmt.ValueKind = JsonValueKind.String Then
                    Return String.Equals(fmt.GetString(), IdentyfikatorFormatu, StringComparison.OrdinalIgnoreCase)
                End If
                Return False
            End Using
        Catch
            Return False
        End Try
    End Function

    ''' <summary>Parsuje tekst JSON do <see cref="DaneZestawuGalerii"/>; przy błędnych danych zgłasza <see cref="InvalidDataException"/>.</summary>
    Public Function ZTekstu(json As String) As DaneZestawuGalerii
        If String.IsNullOrWhiteSpace(json) Then
            Throw New InvalidDataException("Pusty przepis kanapki warstw.")
        End If
        Dim dane As DaneZestawuGalerii
        Try
            dane = JsonSerializer.Deserialize(Of DaneZestawuGalerii)(json, OpcjeJson)
        Catch ex As JsonException
            Throw New InvalidDataException("Nieprawidłowy format JSON przepisu kanapki: " & ex.Message, ex)
        End Try
        If dane Is Nothing Then
            Throw New InvalidDataException("Nie udało się odczytać przepisu kanapki.")
        End If
        If Not String.IsNullOrWhiteSpace(dane.Format) AndAlso
           Not String.Equals(dane.Format.Trim(), IdentyfikatorFormatu, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException($"Nieznany format pliku kanapki: '{dane.Format}'.")
        End If
        If dane.Warstwy Is Nothing OrElse dane.Warstwy.Count = 0 Then
            Throw New InvalidDataException("Przepis kanapki nie zawiera żadnej warstwy.")
        End If
        dane.Format = IdentyfikatorFormatu
        If String.IsNullOrWhiteSpace(dane.Cel) Then dane.Cel = "okno_glowne"
        Return dane
    End Function

    ''' <summary>Serializuje przepis kanapki do czytelnego JSON UTF-8.</summary>
    Public Function DoTekstu(dane As DaneZestawuGalerii) As String
        If dane Is Nothing Then Throw New ArgumentNullException(NameOf(dane))
        If String.IsNullOrWhiteSpace(dane.Format) Then dane.Format = IdentyfikatorFormatu
        If dane.Wersja <= 0 Then dane.Wersja = 1
        Return JsonSerializer.Serialize(dane, OpcjeJson)
    End Function

    ''' <summary>Konwertuje bieżący zestaw warstw z programu do przepisu JSON (np. dla endpointu GET /stan).</summary>
    Public Function ZZestawuWarstw(zestaw As ZestawWarstw, Optional nazwa As String = "") As DaneZestawuGalerii
        Dim dane As New DaneZestawuGalerii With {
            .Format = IdentyfikatorFormatu,
            .Wersja = 1,
            .Nazwa = If(nazwa, "").Trim(),
            .Cel = "okno_glowne"
        }
        If zestaw Is Nothing Then Return dane
        For Each w In zestaw.Warstwy
            dane.Warstwy.Add(New PozycjaZestawuGalerii With {
                .Zbior = If(w.Zbior, ""),
                .Warstwa = If(w.Nazwa, ""),
                .Krycie = w.Krycie,
                .Tryb = NazwaTechnicznaTrybu(w.TrybMieszania)
            })
        Next
        Return dane
    End Function

    ''' <summary>Buduje lokalny <see cref="ZestawWarstw"/> na podstawie słownika zbiorów map (nazwa zbioru -> PlikWarstw).</summary>
    Public Function ZbudujZestaw(dane As DaneZestawuGalerii,
                                 zrodla As IDictionary(Of String, PlikWarstw),
                                 Optional baza As ZestawWarstw = Nothing) As WynikBudowyZestawuGalerii
        If dane Is Nothing Then Throw New ArgumentNullException(NameOf(dane))
        Dim wynik As New WynikBudowyZestawuGalerii With {
            .Nazwa = If(dane.Nazwa, "").Trim(),
            .Cel = NormalizujCel(dane.Cel),
            .ZapiszStyl = dane.ZapiszStyl
        }

        If wynik.Cel = "dolacz" AndAlso baza IsNot Nothing Then
            wynik.Zestaw = baza.Kopia()
        End If

        If dane.Warstwy Is Nothing OrElse zrodla Is Nothing Then Return wynik

        ' Słownik porównujący nazwy zbiorów bez wielkości liter (na wypadek gdyby caller przekazał zwykły Dictionary)
        Dim mapaZrodel As New Dictionary(Of String, PlikWarstw)(zrodla, StringComparer.OrdinalIgnoreCase)

        For Each poz In dane.Warstwy
            If poz Is Nothing Then Continue For
            Dim nazwaZbioru = If(poz.Zbior, "").Trim()
            Dim nazwaWarstwy = If(poz.Warstwa, "").Trim()
            If nazwaZbioru = "" OrElse nazwaWarstwy = "" Then Continue For

            Dim plik As PlikWarstw = Nothing
            If Not mapaZrodel.TryGetValue(nazwaZbioru, plik) OrElse plik Is Nothing OrElse String.IsNullOrWhiteSpace(plik.Adres) Then
                wynik.Pominiete.Add($"{nazwaZbioru}/{nazwaWarstwy} (brak zbioru)")
                Continue For
            End If

            Dim defWarstwy = ZnajdzWarstweWPliku(plik, nazwaWarstwy)
            If defWarstwy Is Nothing Then
                wynik.Pominiete.Add($"{nazwaZbioru}/{nazwaWarstwy} (brak warstwy)")
                Continue For
            End If

            Dim zalecany = ParsujPiksel(defWarstwy.RozmiarPiksela)
            Dim trybMieszania = TrybMieszaniaRozszerzenia.ZTekstu(poz.Tryb)
            Dim krycie = ZestawWarstw.PoprawneKrycie(poz.Krycie)
            Dim nakladka = plik.OpcjeKafli(defWarstwy.Nazwa).Nakladka

            Dim res = wynik.Zestaw.Dodaj(
                nazwa:=defWarstwy.Nazwa,
                krycie:=krycie,
                adres:=plik.Adres,
                zbior:=nazwaZbioru,
                zalecanyPiksel:=zalecany,
                typZrodla:=plik.EfektywnyTyp,
                nakladka:=nakladka,
                umiescPodNakladkami:=False,
                ignorujSsl:=plik.IgnorujSsl,
                trybMieszania:=trybMieszania)

            Select Case res
                Case WynikDodania.Dodano
                    Dim dodana = wynik.Zestaw.Warstwy.LastOrDefault()
                    If dodana IsNot Nothing Then
                        dodana.MaksRozmiarSegmentu = If(defWarstwy.MaksRozmiarSegmentu > 0, defWarstwy.MaksRozmiarSegmentu, plik.MaksRozmiarSegmentu)
                    End If
                Case WynikDodania.BrakMiejsca
                    wynik.PrzycietoDoLimitu = True
                    wynik.Pominiete.Add($"{nazwaZbioru}/{defWarstwy.Nazwa} (limit {ZestawWarstw.MaksLiczba} warstw)")
                Case WynikDodania.JuzWybrana
                    ' Pomijamy duplikat po cichu w obrębie jednej kanapki
            End Select
        Next

        Return wynik
    End Function

    ''' <summary>Tworzy styl kreatora (<see cref="SzablonMapy"/>) gotowy do zapisu w <see cref="SzablonyWlasne"/>.</summary>
    Public Function UtworzStylSzablonu(zestaw As ZestawWarstw,
                                       nazwa As String,
                                       Optional probkaLat As Double = 52.2297,
                                       Optional probkaLon As Double = 21.0122,
                                       Optional probkaPiksel As Double = 0) As SzablonMapy
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return Nothing
        Dim czystaNazwa = If(nazwa, "").Trim()
        If czystaNazwa = "" Then
            czystaNazwa = "Kanapka z galerii (" & DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) & ")"
        End If

        Dim najdokladniejsza = zestaw.NajdokladniejszaWarstwa()
        Dim domyslnyPiksel = If(probkaPiksel > 0, probkaPiksel, If(najdokladniejsza IsNot Nothing AndAlso najdokladniejsza.ZalecanyPiksel > 0, najdokladniejsza.ZalecanyPiksel, 1.0))

        Dim wszystkieCele As CelMapy() = {
            CelMapy.TelefonTablet,
            CelMapy.OdbiornikGarmin,
            CelMapy.WydrukKomputer,
            CelMapy.ProjektCadGis,
            CelMapy.ProjektCad
        }

        Dim szablon As New SzablonMapy(
            SzablonyWlasne.NoweId(),
            czystaNazwa,
            SzablonyWlasne.KategoriaWlasna,
            "Zestaw ułożony w Galerii warstw (" & zestaw.Liczba & " warstw).",
            "🧩",
            domyslnyPiksel,
            ProfilKorektyRastra.Brak,
            wszystkieCele) With {
            .ProbkaLat = If(probkaLat <> 0, probkaLat, 52.2297),
            .ProbkaLon = If(probkaLon <> 0, probkaLon, 21.0122),
            .ProbkaPiksel = domyslnyPiksel
        }

        For Each w In zestaw.Warstwy
            Dim ws = WarstwaSzablonu.ZWarstwyWybranej(w)
            ws.Regulacja = RozpoznajRegulacje(w)
            szablon.Warstwy.Add(ws)
        Next

        Return szablon
    End Function

    Public Function NazwaTechnicznaTrybu(t As TrybMieszania) As String
        Select Case t
            Case TrybMieszania.Mnozenie : Return "Mnozenie"
            Case TrybMieszania.Ciemniej : Return "Ciemniej"
            Case TrybMieszania.Jasniej : Return "Jasniej"
            Case TrybMieszania.Ekran : Return "Ekran"
            Case TrybMieszania.Nakladka : Return "Nakladka"
            Case TrybMieszania.RzezbaTerenu : Return "RzezbaTerenu"
            Case Else : Return "Zwykly"
        End Select
    End Function

    Private Function NormalizujCel(cel As String) As String
        Dim c = If(cel, "").Trim().ToLowerInvariant()
        Select Case c
            Case "dolacz", "kreator", "okno_glowne"
                Return c
            Case Else
                Return "okno_glowne"
        End Select
    End Function

    Private Function ZnajdzWarstweWPliku(plik As PlikWarstw, nazwa As String) As WarstwaMapy
        Dim dokladna = plik.Znajdz(nazwa)
        If dokladna IsNot Nothing Then Return dokladna
        Return plik.Warstwy.FirstOrDefault(Function(x) String.Equals(x.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Function ParsujPiksel(tekst As String) As Double
        If String.IsNullOrWhiteSpace(tekst) Then Return 0
        Dim val As Double
        If Double.TryParse(tekst.Trim().Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, val) AndAlso val > 0 AndAlso Not Double.IsInfinity(val) Then
            Return val
        End If
        Return 0
    End Function

    Private Function RozpoznajRegulacje(w As WarstwaWybrana) As RodzajRegulacji
        Dim z = If(w.Zbior, "").ToLowerInvariant()
        Dim n = If(w.Nazwa, "").ToLowerInvariant()
        If w.TrybMieszania = TrybMieszania.RzezbaTerenu OrElse z.Contains("cieniow") OrElse n.Contains("cien") OrElse n.Contains("hillshade") Then
            Return RodzajRegulacji.Cieniowanie
        End If
        If z.Contains("waymarked") OrElse z.Contains("szlak") OrElse n.Contains("hiking") OrElse n.Contains("cycling") OrElse n.Contains("mtb") Then
            Return RodzajRegulacji.Szlaki
        End If
        If z.Contains("nid") OrElse z.Contains("zabytk") Then
            Return RodzajRegulacji.Zabytki
        End If
        Return RodzajRegulacji.Brak
    End Function

End Module
