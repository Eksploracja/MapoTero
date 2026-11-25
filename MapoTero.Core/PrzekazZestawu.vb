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

''' <summary>Pojedyncza skala w wieloskalowym przepisie zestawu (.mapotero.json wersja 2).</summary>
Public Class PozycjaSkaliGalerii
    <JsonPropertyName("opis")> Public Property Opis As String = ""
    <JsonPropertyName("piksel")> Public Property Piksel As Double = 1.0
    <JsonPropertyName("zoomMin")> <JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)> Public Property ZoomMin As Integer?
    <JsonPropertyName("zoomMax")> <JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)> Public Property ZoomMax As Integer?
    <JsonPropertyName("opcjonalny")> Public Property Opcjonalny As Boolean
    <JsonPropertyName("warstwy")> Public Property Warstwy As New List(Of PozycjaZestawuGalerii)()
End Class

''' <summary>Przepis zestawu warstw („kanapki”) używany w MostekGalerii, plikach eksportu i schowku.</summary>
Public Class DaneZestawuGalerii
    <JsonPropertyName("format")> Public Property Format As String = PrzekazZestawu.IdentyfikatorFormatu
    <JsonPropertyName("wersja")> Public Property Wersja As Integer = 1
    <JsonPropertyName("nazwa")> Public Property Nazwa As String = ""
    ''' <summary>Cel w programie: "okno_glowne" (domyślnie), "dolacz" albo "kreator".</summary>
    <JsonPropertyName("cel")> Public Property Cel As String = "okno_glowne"
    <JsonPropertyName("zapiszStyl")> Public Property ZapiszStyl As Boolean
    ''' <summary>Opcjonalny prostokąt obszaru w WGS84 [minLon, minLat, maxLon, maxLat] zaznaczony na mapce w galerii.</summary>
    <JsonPropertyName("obszarWgs84")> <JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)> Public Property ObszarWgs84 As Double()
    <JsonPropertyName("warstwy")> Public Property Warstwy As New List(Of PozycjaZestawuGalerii)()
    ''' <summary>Opcjonalne stopnie skali dla map wieloskalowych (wersja 2).</summary>
    <JsonPropertyName("skale")> <JsonIgnore(Condition:=JsonIgnoreCondition.WhenWritingNull)> Public Property Skale As List(Of PozycjaSkaliGalerii)
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

''' <summary>Wynik zmapowania wieloskalowego przepisu (.mapotero.json wersja 2) na styl kreatora.</summary>
Public Class WynikBudowySzablonuGalerii
    Public Property Szablon As SzablonMapy
    Public Property PlanWlasny As List(Of PlanStopnia)
    Public Property Pominiete As New List(Of String)()
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
        Dim maWarstwy = dane.Warstwy IsNot Nothing AndAlso dane.Warstwy.Count > 0
        Dim maSkale = dane.Skale IsNot Nothing AndAlso dane.Skale.Any(Function(s) s IsNot Nothing AndAlso s.Warstwy IsNot Nothing AndAlso s.Warstwy.Count > 0)
        If Not maWarstwy AndAlso Not maSkale Then
            Throw New InvalidDataException("Przepis kanapki nie zawiera żadnej warstwy.")
        End If
        If dane.Warstwy Is Nothing Then dane.Warstwy = New List(Of PozycjaZestawuGalerii)()
        dane.Format = IdentyfikatorFormatu
        If String.IsNullOrWhiteSpace(dane.Cel) Then dane.Cel = "okno_glowne"
        Return dane
    End Function

    ''' <summary>Serializuje przepis kanapki do czytelnego JSON UTF-8.</summary>
    Public Function DoTekstu(dane As DaneZestawuGalerii) As String
        If dane Is Nothing Then Throw New ArgumentNullException(NameOf(dane))
        If String.IsNullOrWhiteSpace(dane.Format) Then dane.Format = IdentyfikatorFormatu
        If dane.Skale IsNot Nothing AndAlso dane.Skale.Count > 0 Then
            If dane.Wersja < 2 Then dane.Wersja = 2
        ElseIf dane.Wersja <= 0 Then
            dane.Wersja = 1
        End If
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

        Dim warstwyDoBudowy = dane.Warstwy
        If (warstwyDoBudowy Is Nothing OrElse warstwyDoBudowy.Count = 0) AndAlso dane.Skale IsNot Nothing Then
            Dim najdokladniejsza = dane.Skale.Where(Function(s) s IsNot Nothing AndAlso s.Warstwy IsNot Nothing AndAlso s.Warstwy.Count > 0).
                OrderBy(Function(s) If(s.Opcjonalny, 1, 0)).ThenBy(Function(s) If(s.Piksel > 0, s.Piksel, Double.MaxValue)).FirstOrDefault()
            If najdokladniejsza IsNot Nothing Then warstwyDoBudowy = najdokladniejsza.Warstwy
        End If
        If warstwyDoBudowy Is Nothing OrElse zrodla Is Nothing Then Return wynik

        ' Słownik porównujący nazwy zbiorów bez wielkości liter (na wypadek gdyby caller przekazał zwykły Dictionary)
        Dim mapaZrodel As New Dictionary(Of String, PlikWarstw)(zrodla, StringComparer.OrdinalIgnoreCase)

        For Each poz In warstwyDoBudowy
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

    ''' <summary>Sprawdza, czy zestaw ma warstwy podkładowe o dużej rozpiętości skal (kwalifikuje się do mapy wieloskalowej).</summary>
    Public Function WykryjRozpietoscSkal(zestaw As ZestawWarstw) As Boolean
        If zestaw Is Nothing OrElse zestaw.Liczba < 2 Then Return False

        Dim minKryjacy As Double = 0
        Dim maxKryjacy As Double = 0
        Dim ileKryjacych As Integer = 0

        Dim minRastrowy As Double = 0
        Dim maxRastrowy As Double = 0
        Dim ileRastrowych As Integer = 0

        For i = 0 To zestaw.Liczba - 1
            Dim item = zestaw(i)
            Dim px = item.ZalecanyPiksel
            If px <= 0 OrElse item.Nakladka OrElse ZestawWarstw.RozpoznajRole(item) = RolaWarstwy.Cieniowanie Then Continue For

            Dim jestKryjaca = (i = 0) OrElse (item.Krycie >= ZestawWarstw.PelneKrycie AndAlso item.TrybMieszania = TrybMieszania.Zwykly)
            If jestKryjaca Then
                ileKryjacych += 1
                If minKryjacy = 0 OrElse px < minKryjacy Then minKryjacy = px
                If px > maxKryjacy Then maxKryjacy = px
            End If

            If Not CzyWarstwaReferencyjnaGranice(item.Zbior) Then
                ileRastrowych += 1
                If minRastrowy = 0 OrElse px < minRastrowy Then minRastrowy = px
                If px > maxRastrowy Then maxRastrowy = px
            End If
        Next

        If ileKryjacych >= 2 AndAlso minKryjacy > 0 AndAlso (maxKryjacy / minKryjacy) >= 6.0 Then Return True
        If ileRastrowych >= 2 AndAlso minRastrowy > 0 AndAlso maxRastrowy >= 15.0 AndAlso minRastrowy <= 2.0 Then Return True
        Return False
    End Function

    ''' <summary>Rozdziela kanapkę warstw na stopnie skali (od najdokładniejszego), grupując podkłady o zbliżonym pikselu.</summary>
    Public Function RozdzielNaStopnieSkali(zestaw As ZestawWarstw) As List(Of StopienSkali)
        Dim wynik As New List(Of StopienSkali)()
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return wynik

        Dim podklady As New List(Of (Indeks As Integer, Warstwa As WarstwaWybrana))()
        Dim towarzyszace As New List(Of (Indeks As Integer, Warstwa As WarstwaWybrana, Rola As RolaWarstwy))()

        For i = 0 To zestaw.Liczba - 1
            Dim w = zestaw(i)
            Dim rola = ZestawWarstw.RozpoznajRole(w)
            If rola = RolaWarstwy.Podklad AndAlso Not w.Nakladka Then
                podklady.Add((i, w))
            Else
                towarzyszace.Add((i, w, rola))
            End If
        Next

        Dim podkladyZSkala = podklady.Where(Function(p) p.Warstwa.ZalecanyPiksel > 0).
            OrderBy(Function(p) p.Warstwa.ZalecanyPiksel).ThenBy(Function(p) p.Indeks).ToList()
        Dim podkladyBezSkali = podklady.Where(Function(p) p.Warstwa.ZalecanyPiksel <= 0).ToList()

        If podkladyZSkala.Count < 2 Then
            Dim jedyny As New StopienSkali With {
                .Opis = "podstawowa",
                .Piksel = If(podkladyZSkala.Count = 1, podkladyZSkala(0).Warstwa.ZalecanyPiksel, 1.0)
            }
            For Each w In zestaw.Warstwy
                jedyny.Warstwy.Add(KonwertujDoSzablonu(w))
            Next
            wynik.Add(jedyny)
            Return wynik
        End If

        ' Klastry podkładów: nowy stopień, gdy piksel różni się co najmniej 3-krotnie od najdokładniejszej warstwy klastra
        Dim klastry As New List(Of List(Of (Indeks As Integer, Warstwa As WarstwaWybrana)))()
        Dim biezacyKlaster As New List(Of (Indeks As Integer, Warstwa As WarstwaWybrana)) From {podkladyZSkala(0)}
        Dim minPxKlastra = podkladyZSkala(0).Warstwa.ZalecanyPiksel

        For i = 1 To podkladyZSkala.Count - 1
            Dim p = podkladyZSkala(i)
            If p.Warstwa.ZalecanyPiksel >= minPxKlastra * 3.0 Then
                klastry.Add(biezacyKlaster)
                biezacyKlaster = New List(Of (Indeks As Integer, Warstwa As WarstwaWybrana)) From {p}
                minPxKlastra = p.Warstwa.ZalecanyPiksel
            Else
                biezacyKlaster.Add(p)
            End If
        Next
        klastry.Add(biezacyKlaster)

        If podkladyBezSkali.Count > 0 Then
            klastry(0).AddRange(podkladyBezSkali)
        End If

        For k = 0 To klastry.Count - 1
            Dim klaster = klastry(k).OrderBy(Function(p) p.Indeks).ToList()
            Dim pikselStopnia = klaster.Where(Function(p) p.Warstwa.ZalecanyPiksel > 0).Min(Function(p) p.Warstwa.ZalecanyPiksel)
            Dim glowny = klaster(0).Warstwa
            Dim stopien As New StopienSkali With {
                .Opis = FormatujOpisStopnia(glowny, pikselStopnia),
                .Piksel = pikselStopnia
            }
            For Each p In klaster
                stopien.Warstwy.Add(KonwertujDoSzablonu(p.Warstwa))
            Next
            For Each t In towarzyszace
                If PasujeTowarzyszacaDoSkali(t.Warstwa, t.Rola, pikselStopnia) Then
                    stopien.Warstwy.Add(KonwertujDoSzablonu(t.Warstwa))
                End If
            Next
            wynik.Add(stopien)
        Next

        Return wynik
    End Function

    ''' <summary>Tworzy styl kreatora (<see cref="SzablonMapy"/>) gotowy do zapisu w <see cref="SzablonyWlasne"/>.</summary>
    Public Function UtworzStylSzablonu(zestaw As ZestawWarstw,
                                       nazwa As String,
                                       Optional probkaLat As Double = 52.2297,
                                       Optional probkaLon As Double = 21.0122,
                                       Optional probkaPiksel As Double = 0,
                                       Optional rozdzielSkale As Boolean = False) As SzablonMapy
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

        If rozdzielSkale Then
            Dim stopnie = RozdzielNaStopnieSkali(zestaw)
            If stopnie.Count > 1 Then
                For Each st In stopnie
                    szablon.DodajStopien(st.Opis, st.Piksel, st.Warstwy.ToArray())
                Next
                szablon.DomyslnyPiksel = stopnie(0).Piksel
                szablon.ProbkaPiksel = stopnie(0).Piksel
                szablon.Opis = "Zestaw wieloskalowy z Galerii warstw (" & stopnie.Count & " skale)."
                Return szablon
            End If
        End If

        For Each w In zestaw.Warstwy
            szablon.Warstwy.Add(KonwertujDoSzablonu(w))
        Next

        Return szablon
    End Function

    ''' <summary>Konwertuje styl kreatora (wraz ze stopniami skali i opcjonalnym planem zoomów) do przepisu .mapotero.json.</summary>
    Public Function ZSzablonuMapy(szablon As SzablonMapy,
                                  Optional plan As IList(Of PlanStopnia) = Nothing,
                                  Optional nazwa As String = Nothing) As DaneZestawuGalerii
        If szablon Is Nothing Then Throw New ArgumentNullException(NameOf(szablon))
        Dim czystaNazwa = If(String.IsNullOrWhiteSpace(nazwa), szablon.Nazwa, nazwa).Trim()
        Dim dane As New DaneZestawuGalerii With {
            .Format = IdentyfikatorFormatu,
            .Nazwa = czystaNazwa,
            .Cel = "kreator",
            .ZapiszStyl = True
        }

        Dim maPlan = plan IsNot Nothing AndAlso plan.Count > 1
        Dim maStopnie = szablon.StopnieSkali.Count > 1

        If maPlan Then
            dane.Wersja = 2
            dane.Skale = New List(Of PozycjaSkaliGalerii)()
            For Each ps In plan
                Dim warstwy = ps.WarstwyEfektywne
                If warstwy Is Nothing OrElse warstwy.Count = 0 Then Continue For
                Dim pozSkali As New PozycjaSkaliGalerii With {
                    .Opis = If(ps.Stopien?.Opis, ""),
                    .Piksel = If(ps.Piksel > 0, ps.Piksel, 1.0),
                    .ZoomMin = ps.PoziomMin,
                    .ZoomMax = ps.PoziomMax,
                    .Opcjonalny = If(ps.Stopien?.Opcjonalny, False)
                }
                For Each ws In warstwy
                    pozSkali.Warstwy.Add(PozycjaZWarstwySzablonu(ws))
                Next
                dane.Skale.Add(pozSkali)
            Next
        ElseIf maStopnie Then
            dane.Wersja = 2
            dane.Skale = New List(Of PozycjaSkaliGalerii)()
            For Each st In szablon.StopnieSkali
                If st.Warstwy.Count = 0 Then Continue For
                Dim pozSkali As New PozycjaSkaliGalerii With {
                    .Opis = st.Opis,
                    .Piksel = If(st.Piksel > 0, st.Piksel, 1.0),
                    .Opcjonalny = st.Opcjonalny
                }
                For Each ws In st.Warstwy
                    pozSkali.Warstwy.Add(PozycjaZWarstwySzablonu(ws))
                Next
                dane.Skale.Add(pozSkali)
            Next
        End If

        If dane.Skale IsNot Nothing AndAlso dane.Skale.Count > 1 Then
            Dim fallback = dane.Skale.Where(Function(s) Not s.Opcjonalny).OrderBy(Function(s) s.Piksel).FirstOrDefault()
            If fallback Is Nothing Then fallback = dane.Skale(0)
            dane.Warstwy.AddRange(fallback.Warstwy)
        Else
            dane.Wersja = 1
            dane.Skale = Nothing
            Dim zrodloWarstw As IEnumerable(Of WarstwaSzablonu) =
                If(plan IsNot Nothing AndAlso plan.Count = 1, CType(plan(0).WarstwyEfektywne, IEnumerable(Of WarstwaSzablonu)), szablon.Warstwy)
            For Each ws In zrodloWarstw
                dane.Warstwy.Add(PozycjaZWarstwySzablonu(ws))
            Next
        End If

        Return dane
    End Function

    ''' <summary>Odtwarza wieloskalowy <see cref="SzablonMapy"/> (i opcjonalny <see cref="PlanStopnia"/>) z przepisu wersji 2.</summary>
    Public Function ZbudujSzablonWieloskalowy(dane As DaneZestawuGalerii,
                                              zrodla As IDictionary(Of String, PlikWarstw),
                                              Optional probkaLat As Double = 52.2297,
                                              Optional probkaLon As Double = 21.0122) As WynikBudowySzablonuGalerii
        Dim wynik As New WynikBudowySzablonuGalerii()
        If dane Is Nothing OrElse dane.Skale Is Nothing OrElse dane.Skale.Count = 0 OrElse zrodla Is Nothing Then Return wynik

        Dim wszystkieCele As CelMapy() = {
            CelMapy.TelefonTablet,
            CelMapy.OdbiornikGarmin,
            CelMapy.WydrukKomputer,
            CelMapy.ProjektCadGis,
            CelMapy.ProjektCad
        }
        Dim nazwa = If(dane.Nazwa, "").Trim()
        If nazwa = "" Then
            nazwa = "Styl wieloskalowy (" & DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) & ")"
        End If

        Dim posortowane = dane.Skale.
            Where(Function(s) s IsNot Nothing AndAlso s.Warstwy IsNot Nothing AndAlso s.Warstwy.Count > 0).
            OrderBy(Function(s) If(s.Opcjonalny, 1, 0)).
            ThenBy(Function(s) If(s.Piksel > 0, s.Piksel, Double.MaxValue)).
            ToList()
        If posortowane.Count = 0 Then Return wynik

        Dim szablon As New SzablonMapy(
            SzablonyWlasne.NoweId(),
            nazwa,
            SzablonyWlasne.KategoriaWlasna,
            "Zaimportowany styl wieloskalowy (" & posortowane.Count & " skale).",
            "🧩",
            1.0,
            ProfilKorektyRastra.Brak,
            wszystkieCele) With {
            .ProbkaLat = If(probkaLat <> 0, probkaLat, 52.2297),
            .ProbkaLon = If(probkaLon <> 0, probkaLon, 21.0122)
        }

        Dim planKandydat As New List(Of PlanStopnia)()
        Dim wszystkieMajaZoom As Boolean = True

        For Each sk In posortowane
            Dim resSkali = ZbudujZestaw(New DaneZestawuGalerii With {.Warstwy = sk.Warstwy}, zrodla)
            wynik.Pominiete.AddRange(resSkali.Pominiete)
            If resSkali.Zestaw.Liczba = 0 Then Continue For

            Dim piksel = If(sk.Piksel > 0, sk.Piksel, 1.0)
            Dim opis = If(String.IsNullOrWhiteSpace(sk.Opis), piksel.ToString("0.##", CultureInfo.CurrentCulture) & " m/px", sk.Opis.Trim())
            Dim warstwySzablonu = resSkali.Zestaw.Warstwy.Select(AddressOf KonwertujDoSzablonu).ToArray()

            Dim dodanyStopien As StopienSkali = If(sk.Opcjonalny,
                szablon.DodajStopienOpcjonalny(opis, piksel, warstwySzablonu),
                szablon.DodajStopien(opis, piksel, warstwySzablonu))

            If sk.ZoomMin.HasValue AndAlso sk.ZoomMax.HasValue Then
                planKandydat.Add(New PlanStopnia With {
                    .Stopien = dodanyStopien,
                    .Piksel = piksel,
                    .PoziomMin = sk.ZoomMin.Value,
                    .PoziomMax = sk.ZoomMax.Value,
                    .Warstwy = New List(Of WarstwaSzablonu)(warstwySzablonu)
                })
            Else
                wszystkieMajaZoom = False
            End If
        Next

        If szablon.StopnieSkali.Count = 0 Then Return wynik

        Dim najdokladniejszyGlowny = szablon.StopnieGlowne.FirstOrDefault()
        Dim pikselBazowy = If(najdokladniejszyGlowny IsNot Nothing, najdokladniejszyGlowny.Piksel, szablon.StopnieSkali(0).Piksel)
        szablon.DomyslnyPiksel = pikselBazowy
        szablon.ProbkaPiksel = pikselBazowy

        wynik.Szablon = szablon
        If wszystkieMajaZoom AndAlso planKandydat.Count > 1 AndAlso ZarzadcaSzablonow.SprawdzPlan(planKandydat) = "" Then
            wynik.PlanWlasny = planKandydat
        End If
        Return wynik
    End Function

    Private Function KonwertujDoSzablonu(w As WarstwaWybrana) As WarstwaSzablonu
        Dim ws = WarstwaSzablonu.ZWarstwyWybranej(w)
        ws.Regulacja = RozpoznajRegulacje(w)
        Return ws
    End Function

    Private Function PozycjaZWarstwySzablonu(ws As WarstwaSzablonu) As PozycjaZestawuGalerii
        Return New PozycjaZestawuGalerii With {
            .Zbior = If(ws.Zbior, ""),
            .Warstwa = If(ws.Warstwa, ""),
            .Krycie = ZestawWarstw.PoprawneKrycie(ws.Krycie),
            .Tryb = NazwaTechnicznaTrybu(ws.TrybMieszania)
        }
    End Function

    Private Function CzyWarstwaReferencyjnaGranice(zbior As String) As Boolean
        Dim z = If(zbior, "")
        Return z.IndexOf("rejestr_granic", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               z.IndexOf("regiony_", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               String.Equals(z, "lasy_panstwowe_bdl", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(z, "administracyjna", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function PasujeTowarzyszacaDoSkali(w As WarstwaWybrana, rola As RolaWarstwy, pikselStopnia As Double) As Boolean
        Dim jestSzlak = String.Equals(w.Zbior, "Waymarked_Trails", StringComparison.OrdinalIgnoreCase)
        If rola = RolaWarstwy.Nakladka AndAlso Not jestSzlak AndAlso w.ZalecanyPiksel > 0 AndAlso w.ZalecanyPiksel <= 1.0 Then
            Return pikselStopnia <= 2.0
        End If
        Return pikselStopnia <= 10.0
    End Function

    Private Function FormatujOpisStopnia(glowny As WarstwaWybrana, piksel As Double) As String
        Dim n = If(glowny.Nazwa, "").Trim()
        If n = "" OrElse String.Equals(n, "Raster", StringComparison.OrdinalIgnoreCase) OrElse n = "0" OrElse n = "1" Then
            n = If(glowny.Zbior, "").Replace("_", " ").Trim()
        End If
        If n = "" Then n = "Skala"
        If n.Length > 28 Then n = n.Substring(0, 28).TrimEnd() & "…"
        Return $"{n} ({piksel.ToString("0.##", CultureInfo.CurrentCulture)} m/px)"
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
