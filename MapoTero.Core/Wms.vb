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
Imports System.Text.RegularExpressions

''' <summary>Budowanie zapytań GetMap usług WMS na podstawie adresów z plików katalogu "warstwy".</summary>
Public NotInheritable Class Wms

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Standard WMS wymaga parametru SERVICE=WMS - większość adresów w plikach warstw go nie zawiera;
    ''' serwery ArcGIS go nie wymagają, ale MapServer czy GeoServer mogą odrzucić takie zapytanie.
    ''' </summary>
    Public Shared Function UzupelnijAdres(adres As String) As String
        If adres Is Nothing Then Return adres
        If adres.IndexOf("service=", StringComparison.OrdinalIgnoreCase) >= 0 Then Return adres
        Dim znak As Integer = adres.IndexOf("?"c)
        If znak < 0 Then Return adres & "?SERVICE=WMS&"
        Return adres.Substring(0, znak + 1) & "SERVICE=WMS&" & adres.Substring(znak + 1)
    End Function

    ''' <summary>Wersja WMS z adresu (parametr VERSION), domyślnie 1.3.0.</summary>
    Public Shared Function Wersja(adres As String) As String
        Dim m = Regex.Match(adres, "[?&]version=([^&]*)", RegexOptions.IgnoreCase)
        If m.Success AndAlso m.Groups(1).Value <> "" Then Return m.Groups(1).Value
        Return "1.3.0"
    End Function

    ''' <summary>
    ''' Ustawia układ współrzędnych zapytania: w WMS 1.3.0 parametr CRS, w starszych wersjach SRS
    ''' (plik warstw GDOŚ zawierał SRS przy wersji 1.3.0, czego serwery zgodne ze standardem nie akceptują).
    ''' </summary>
    Public Shared Function UstawUklad(adres As String, uklad As UkladWspolrzednych) As String
        Dim nazwa As String = If(Wersja(adres).StartsWith("1.3", StringComparison.Ordinal), "CRS", "SRS")
        Dim wynik As String = Regex.Replace(adres, "([?&])(CRS|SRS)=[^&]*", "$1" & nazwa & "=" & uklad.KodCrs, RegexOptions.IgnoreCase)
        If wynik = adres AndAlso Not Regex.IsMatch(adres, "[?&](CRS|SRS)=", RegexOptions.IgnoreCase) Then
            'brak parametru układu - dopisanie przed parametrem LAYERS (lub na końcu)
            Dim m = Regex.Match(adres, "[?&]layers=", RegexOptions.IgnoreCase)
            If m.Success Then
                wynik = adres.Substring(0, m.Index + 1) & nazwa & "=" & uklad.KodCrs & "&" & adres.Substring(m.Index + 1)
            Else
                wynik = adres & If(adres.EndsWith("&", StringComparison.Ordinal) OrElse adres.EndsWith("?", StringComparison.Ordinal), "", "&") &
                        nazwa & "=" & uklad.KodCrs
            End If
        End If
        Return wynik
    End Function

    ''' <summary>
    ''' Kolejność liczb w parametrze BBOX: True - najpierw współrzędna północna (X), False - najpierw wschodnia (Y).
    ''' WMS 1.3.0 stosuje kolejność osi z definicji EPSG (PL-1992, PL-2000, WGS84: północ-wschód; UTM: wschód-północ),
    ''' starsze wersje WMS - zawsze wschód-północ. Opcja "zamiana X i Y" odwraca wynik (dla serwerów niezgodnych ze standardem).
    ''' </summary>
    Public Shared Function BboxNajpierwPolnoc(adres As String, uklad As UkladWspolrzednych, zamienOsie As Boolean) As Boolean
        Dim wg As Boolean = If(Wersja(adres).StartsWith("1.3", StringComparison.Ordinal), uklad.NajpierwPolnoc, False)
        Return If(zamienOsie, Not wg, wg)
    End Function

    ''' <summary>Liczba z kropką dziesiętną (bez zbędnych zer).</summary>
    Public Shared Function Liczba(wartosc As Double) As String
        Return wartosc.ToString("0.##########", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Pełne zapytanie GetMap dla zasięgu segmentu. Adres z pliku warstw kończy się parametrem "layers=",
    ''' do którego dopisywane są wybrane warstwy (jeśli adres zawiera już stałą listę warstw, nowe dopisywane są po przecinku).
    ''' </summary>
    Public Shared Function ZapytanieGetMap(adres As String, warstwy As IEnumerable(Of String), uklad As UkladWspolrzednych,
                                           zasieg As Zasieg, format As String, szerokoscPx As Integer, wysokoscPx As Integer,
                                           zamienOsie As Boolean) As String
        Dim baza As String = UstawUklad(UzupelnijAdres(adres), uklad)

        Dim lista As New List(Of String)
        For Each w In warstwy
            If Not String.IsNullOrEmpty(w) Then lista.Add(w)
        Next
        Dim nowe As String = String.Join(",", lista)
        If nowe <> "" Then
            If Regex.IsMatch(baza, "[?&]layers=", RegexOptions.IgnoreCase) Then
                If Not baza.EndsWith("=", StringComparison.Ordinal) AndAlso Not baza.EndsWith(",", StringComparison.Ordinal) Then
                    nowe = "," & nowe
                End If
            Else
                Dim lacznik As String = If(baza.EndsWith("&", StringComparison.Ordinal) OrElse baza.EndsWith("?", StringComparison.Ordinal), "", If(baza.IndexOf("?"c) < 0, "?", "&"))
                nowe = lacznik & "layers=" & nowe
            End If
        End If

        Dim bbox As String
        If BboxNajpierwPolnoc(adres, uklad, zamienOsie) Then
            bbox = Liczba(zasieg.XDol) & "," & Liczba(zasieg.YLewy) & "," & Liczba(zasieg.XGora) & "," & Liczba(zasieg.YPrawy)
        Else
            bbox = Liczba(zasieg.YLewy) & "," & Liczba(zasieg.XDol) & "," & Liczba(zasieg.YPrawy) & "," & Liczba(zasieg.XGora)
        End If

        Dim fmt As String = If(format.StartsWith("image/", StringComparison.OrdinalIgnoreCase), format, "image/" & format)
        Return baza & nowe & "&bbox=" & bbox & "&format=" & fmt & "&styles=&width=" &
               szerokoscPx.ToString(CultureInfo.InvariantCulture) & "&height=" & wysokoscPx.ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Rozszerzenie pliku dla formatu obrazu WMS (jpeg - jpg, tiff - tif, png8/png24/png32 - png).</summary>
    Public Shared Function RozszerzeniePliku(format As String) As String
        Dim f As String = If(format, "").Trim().ToLowerInvariant()
        If f.StartsWith("image/", StringComparison.Ordinal) Then f = f.Substring(6)
        Select Case f
            Case "jpeg", "jpg" : Return "jpg"
            Case "tiff", "tif" : Return "tif"
            Case "png", "png8", "png24", "png32" : Return "png"
            Case "gif" : Return "gif"
            Case Else : Return "jpg"
        End Select
    End Function

End Class
