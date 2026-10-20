'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Collections.Generic
Imports System.Globalization

''' <summary>
''' Opis mapy zapisywany w plikach eksportu (MBTiles, KMZ, OruxMaps): nazwa z datą pobrania, źródło danych
''' i opis warstw - na telefonie z kilkunastoma mapami pozwala odróżnić np. ortofotomapy pobrane w różnych latach.
''' </summary>
Public Class MetadaneMapy

    ''' <summary>Nazwa zbioru map i warstwa (np. "ortofotomapa Raster").</summary>
    Public Property Tytul As String = ""
    ''' <summary>Data pobrania segmentów (Nothing - nieznana, np. sesja z poprzednich wersji programu).</summary>
    Public Property DataPobrania As Date?
    ''' <summary>Źródła danych (instytucje lub serwery) wszystkich warstw mapy.</summary>
    Public ReadOnly Property Zrodla As New List(Of String)
    ''' <summary>Opisy warstw (zbiór map i warstwa) - pierwsza warstwa podkładowa, kolejne nakładane na nią.</summary>
    Public ReadOnly Property Warstwy As New List(Of String)

    ''' <summary>Data w formacie zapisywanym w conf.txt (rrrr-mm-dd).</summary>
    Public Shared Function TekstDaty(data As Date) As String
        Return data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Data z conf.txt (rrrr-mm-dd); Nothing, gdy pole jest puste lub niepoprawne.</summary>
    Public Shared Function DataZTekstu(tekst As String) As Date?
        Dim d As Date
        If Date.TryParseExact(If(tekst, "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d
        Return Nothing
    End Function

    ''' <summary>Dodaje źródło (bez powtórzeń).</summary>
    Public Sub DodajZrodlo(zrodlo As String)
        If String.IsNullOrWhiteSpace(zrodlo) Then Exit Sub
        For Each z In Zrodla
            If String.Equals(z, zrodlo, StringComparison.OrdinalIgnoreCase) Then Exit Sub
        Next
        Zrodla.Add(zrodlo)
    End Sub

    ''' <summary>Nazwa mapy widoczna w aplikacji: tytuł i data pobrania, np. "ortofotomapa Raster (2026-10-19)".</summary>
    Public ReadOnly Property Nazwa As String
        Get
            Dim t As String = If(Tytul.Trim() = "", "Mapa MapoTero", Tytul.Trim())
            If DataPobrania.HasValue Then t &= " (" & TekstDaty(DataPobrania.Value) & ")"
            Return t
        End Get
    End Property

    ''' <summary>Źródła danych w jednym wierszu, np. "Źródło: Główny Urząd Geodezji i Kartografii (geoportal.gov.pl)".</summary>
    Public ReadOnly Property Atrybucja As String
        Get
            If Zrodla.Count = 0 Then Return ""
            Return If(Zrodla.Count = 1, "Źródło: ", "Źródła: ") & String.Join("; ", Zrodla)
        End Get
    End Property

    ''' <summary>Pełny opis mapy: warstwy, data pobrania, źródła i program.</summary>
    Public ReadOnly Property Opis As String
        Get
            Dim cz As New List(Of String)
            If Warstwy.Count = 1 Then
                cz.Add("Warstwa: " & Warstwy(0))
            ElseIf Warstwy.Count > 1 Then
                cz.Add("Warstwy (od dołu): " & String.Join(" + ", Warstwy))
            End If
            cz.Add("Data pobrania: " & If(DataPobrania.HasValue, TekstDaty(DataPobrania.Value), "nieznana"))
            If Atrybucja <> "" Then cz.Add(Atrybucja)
            cz.Add("Mapa utworzona programem MapoTero")
            Return String.Join(". ", cz) & "."
        End Get
    End Property

    ''' <summary>
    ''' Źródło danych wg adresu serwera WMS / WMTS: instytucja udostępniająca dane dla znanych serwerów,
    ''' a dla pozostałych - nazwa serwera. Pusty tekst, gdy adres jest pusty.
    ''' </summary>
    Public Shared Function ZrodloDanych(adres As String) As String
        Dim host As String = Serwer(adres)
        If host = "" Then Return ""
        Dim h As String = host.ToLowerInvariant()
        If h.EndsWith("geoportal.gov.pl", StringComparison.Ordinal) OrElse h.EndsWith("gugik.gov.pl", StringComparison.Ordinal) Then
            Return "Główny Urząd Geodezji i Kartografii (geoportal.gov.pl)"
        ElseIf h.EndsWith("gdos.gov.pl", StringComparison.Ordinal) Then
            Return "Generalna Dyrekcja Ochrony Środowiska"
        ElseIf h.EndsWith("pgi.gov.pl", StringComparison.Ordinal) Then
            Return "Państwowy Instytut Geologiczny - PIB"
        ElseIf h.EndsWith("lasy.gov.pl", StringComparison.Ordinal) Then
            Return "Bank Danych o Lasach"
        ElseIf h.EndsWith("zabytek.gov.pl", StringComparison.Ordinal) Then
            Return "Narodowy Instytut Dziedzictwa"
        ElseIf h.EndsWith("terrestris.de", StringComparison.Ordinal) OrElse h.Contains("openstreetmap") Then
            Return "© autorzy OpenStreetMap"
        ElseIf h.EndsWith("cenagis.edu.pl", StringComparison.Ordinal) Then
            Return "pastmaps.cenagis.edu.pl"
        End If
        Return host
    End Function

    ''' <summary>Nazwa serwera z adresu (bez protokołu, portu i ścieżki).</summary>
    Public Shared Function Serwer(adres As String) As String
        Dim a As String = If(adres, "").Trim().Trim(""""c)
        If a = "" Then Return ""
        Dim i As Integer = a.IndexOf("://", StringComparison.Ordinal)
        If i >= 0 Then a = a.Substring(i + 3)
        For Each znak In {"/"c, "?"c, ":"c}
            Dim j As Integer = a.IndexOf(znak)
            If j >= 0 Then a = a.Substring(0, j)
        Next
        Return a
    End Function

End Class
