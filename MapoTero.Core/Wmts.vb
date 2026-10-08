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
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <summary>
''' Układ współrzędnych zestawu kafli WMTS: jeden z układów programu albo Web Mercator (EPSG:3857).
''' Współrzędne w konwencji programu: X - północna (szerokość), Y - wschodnia (długość).
''' </summary>
Public NotInheritable Class UkladKafliWmts

    Private Const R As Double = 6378137.0

    ''' <summary>Kod EPSG (dla CRS84 - 4326).</summary>
    Public ReadOnly Property Epsg As Integer
    ''' <summary>Układ programu; Nothing dla Web Mercator.</summary>
    Public ReadOnly Property Uklad As UkladWspolrzednych
    ''' <summary>Kolejność współrzędnych narożnika macierzy (TopLeftCorner) wg definicji układu: True - najpierw północna.</summary>
    Public ReadOnly Property NajpierwPolnoc As Boolean

    Private Sub New(epsg As Integer, uklad As UkladWspolrzednych, najpierwPolnoc As Boolean)
        Me.Epsg = epsg
        Me.Uklad = uklad
        Me.NajpierwPolnoc = najpierwPolnoc
    End Sub

    Public ReadOnly Property JestWebMercator As Boolean
        Get
            Return Uklad Is Nothing
        End Get
    End Property

    Public ReadOnly Property Geograficzny As Boolean
        Get
            Return Uklad IsNot Nothing AndAlso Uklad.Geograficzny
        End Get
    End Property

    ''' <summary>Liczba metrów w jednostce układu - do przeliczenia mianownika skali na rozmiar piksela (WMTS 1.0.0, 6.1).</summary>
    Public ReadOnly Property MetrowNaJednostke As Double
        Get
            Return If(Geograficzny, 2 * Math.PI * R / 360, 1.0)
        End Get
    End Property

    Public Function ZWgs84(p As PunktGeo) As PunktXY
        If Uklad IsNot Nothing Then Return Uklad.ZWgs84(p)
        Dim fi As Double = Math.Max(-85.0511287798, Math.Min(85.0511287798, p.Szerokosc)) * Math.PI / 180
        Return New PunktXY(R * Math.Log(Math.Tan(Math.PI / 4 + fi / 2)), R * p.Dlugosc * Math.PI / 180)
    End Function

    Public Function DoWgs84(p As PunktXY) As PunktGeo
        If Uklad IsNot Nothing Then Return Uklad.DoWgs84(p)
        Return New PunktGeo((2 * Math.Atan(Math.Exp(p.X / R)) - Math.PI / 2) * 180 / Math.PI, p.Y / R * 180 / Math.PI)
    End Function

    ''' <summary>
    ''' Układ z oznaczenia CRS zestawu kafli ("EPSG:2180", "urn:ogc:def:crs:EPSG::2180",
    ''' "http://www.opengis.net/def/crs/EPSG/0/3857", "urn:ogc:def:crs:OGC:1.3:CRS84").
    ''' Zwraca Nothing dla układu nieobsługiwanego przez program.
    ''' </summary>
    Public Shared Function ZOznaczenia(crs As String) As UkladKafliWmts
        If String.IsNullOrEmpty(crs) Then Return Nothing
        If crs.IndexOf("CRS84", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return New UkladKafliWmts(4326, UkladWspolrzednych.Wgs84, False)
        End If
        Dim m = Regex.Match(crs.Trim(), "(\d+)$")
        If Not m.Success Then Return Nothing
        Dim kod As Integer
        If Not Integer.TryParse(m.Groups(1).Value, NumberStyles.None, CultureInfo.InvariantCulture, kod) Then Return Nothing
        Select Case kod
            Case 3857, 900913, 102100, 102113, 3785
                Return New UkladKafliWmts(3857, Nothing, False)
        End Select
        Dim u = UkladWspolrzednych.ZKodu(kod)
        If u.Epsg <> kod Then Return Nothing
        Return New UkladKafliWmts(kod, u, u.NajpierwPolnoc)
    End Function

End Class

''' <summary>Macierz kafli (TileMatrix) - jeden poziom szczegółowości zestawu kafli.</summary>
Public Class MacierzKafliWmts
    Public Property Identyfikator As String = ""
    Public Property MianownikSkali As Double
    ''' <summary>Pierwsza i druga liczba TopLeftCorner (kolejność osi zależy od układu).</summary>
    Public Property NarozA As Double
    Public Property NarozB As Double
    Public Property SzerokoscKafla As Integer = 256
    Public Property WysokoscKafla As Integer = 256
    Public Property LiczbaKolumn As Integer
    Public Property LiczbaWierszy As Integer

    ''' <summary>Rozmiar piksela w jednostkach układu (piksel standardowy 0,28 mm).</summary>
    Public Function Rozdzielczosc(metrowNaJednostke As Double) As Double
        Return MianownikSkali * 0.00028 / metrowNaJednostke
    End Function

    ''' <summary>Lewy górny narożnik macierzy (X - północna, Y - wschodnia).</summary>
    Public Function LewyGorny(najpierwPolnoc As Boolean) As PunktXY
        Return If(najpierwPolnoc, New PunktXY(NarozA, NarozB), New PunktXY(NarozB, NarozA))
    End Function
End Class

''' <summary>Zestaw macierzy kafli (TileMatrixSet).</summary>
Public Class ZestawMacierzyWmts
    Public Property Identyfikator As String = ""
    Public Property Crs As String = ""
    Public ReadOnly Property Macierze As New List(Of MacierzKafliWmts)

    Public ReadOnly Property Uklad As UkladKafliWmts
        Get
            Return UkladKafliWmts.ZOznaczenia(Crs)
        End Get
    End Property
End Class

''' <summary>Powiązanie warstwy z zestawem macierzy, z opcjonalnymi ograniczeniami zakresu kafli (TileMatrixSetLimits).</summary>
Public Class PowiazanieZestawu
    Public Property Zestaw As String = ""
    ''' <summary>Identyfikator macierzy - {MinTileRow, MaxTileRow, MinTileCol, MaxTileCol}; pusty - bez ograniczeń.</summary>
    Public ReadOnly Property Limity As New Dictionary(Of String, Integer())
End Class

''' <summary>Szablon adresu kafla w trybie REST (ResourceURL resourceType="tile").</summary>
Public Class SzablonKafla
    Public Property Format As String = ""
    Public Property Szablon As String = ""
End Class

''' <summary>Warstwa usługi WMTS.</summary>
Public Class WarstwaWmts
    Public Property Identyfikator As String = ""
    Public Property Tytul As String = ""
    Public ReadOnly Property Formaty As New List(Of String)
    Public Property Styl As String = "default"
    Public ReadOnly Property Powiazania As New List(Of PowiazanieZestawu)
    Public ReadOnly Property Szablony As New List(Of SzablonKafla)
    ''' <summary>Wymiary (np. czas) z wartościami domyślnymi - do szablonów REST.</summary>
    Public ReadOnly Property Wymiary As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    Public Overrides Function ToString() As String
        Return Identyfikator
    End Function
End Class

''' <summary>Opis usługi WMTS odczytany z dokumentu GetCapabilities (WMTS 1.0.0).</summary>
Public Class UslugaWmts

    Public ReadOnly Property Warstwy As New List(Of WarstwaWmts)
    Public ReadOnly Property Zestawy As New List(Of ZestawMacierzyWmts)
    ''' <summary>Adres operacji GetTile w trybie KVP; pusty, gdy usługa udostępnia kafle tylko w trybie REST.</summary>
    Public Property AdresKvp As String = ""

    Public Function Warstwa(identyfikator As String) As WarstwaWmts
        Return Warstwy.FirstOrDefault(Function(w) String.Equals(w.Identyfikator, identyfikator, StringComparison.Ordinal))
    End Function

    Public Function Zestaw(identyfikator As String) As ZestawMacierzyWmts
        Return Zestawy.FirstOrDefault(Function(z) String.Equals(z.Identyfikator, identyfikator, StringComparison.Ordinal))
    End Function

    'elementy wyszukiwane po nazwie lokalnej - serwery różnie deklarują przestrzenie nazw (wmts, ows 1.1, bez prefiksów)
    Private Shared Function Dzieci(e As XElement, nazwa As String) As IEnumerable(Of XElement)
        Return e.Elements().Where(Function(x) x.Name.LocalName = nazwa)
    End Function

    Private Shared Function Dziecko(e As XElement, nazwa As String) As XElement
        Return Dzieci(e, nazwa).FirstOrDefault()
    End Function

    Private Shared Function Tekst(e As XElement, nazwa As String) As String
        Dim d = Dziecko(e, nazwa)
        Return If(d Is Nothing, "", d.Value.Trim())
    End Function

    Private Shared Function Atrybut(e As XElement, nazwa As String) As String
        Dim a = e.Attributes().FirstOrDefault(Function(x) x.Name.LocalName = nazwa)
        Return If(a Is Nothing, "", a.Value.Trim())
    End Function

    Private Shared Function LiczbaD(tekst As String) As Double
        Return Double.Parse(tekst, NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function LiczbaC(tekst As String, domyslna As Integer) As Integer
        Dim w As Integer
        If Integer.TryParse(tekst, NumberStyles.Integer, CultureInfo.InvariantCulture, w) Then Return w
        Return domyslna
    End Function

    ''' <summary>Odczyt dokumentu GetCapabilities. Adres dokumentu służy za adres KVP, gdy dokument go nie podaje.</summary>
    Public Shared Function Wczytaj(xml As String, adresCapabilities As String) As UslugaWmts
        Dim doc = XDocument.Parse(xml)
        Dim korzen = doc.Root
        If korzen Is Nothing OrElse korzen.Name.LocalName <> "Capabilities" Then
            Dim opis As String = If(korzen Is Nothing, "", korzen.Value.Trim())
            If opis.Length > 300 Then opis = opis.Substring(0, 300)
            Throw New FormatException("serwer nie zwrócił dokumentu WMTS Capabilities" & If(opis = "", "", ": " & opis))
        End If
        Dim u As New UslugaWmts()

        Dim zawartosc = Dziecko(korzen, "Contents")
        If zawartosc Is Nothing Then Throw New FormatException("dokument WMTS Capabilities nie zawiera sekcji Contents")

        For Each ez In Dzieci(zawartosc, "TileMatrixSet")
            Dim z As New ZestawMacierzyWmts With {.Identyfikator = Tekst(ez, "Identifier"), .Crs = Tekst(ez, "SupportedCRS")}
            For Each em In Dzieci(ez, "TileMatrix")
                Dim naroz = Tekst(em, "TopLeftCorner").Split({" "c, ChrW(9), ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
                If naroz.Length < 2 Then Continue For
                z.Macierze.Add(New MacierzKafliWmts With {
                    .Identyfikator = Tekst(em, "Identifier"), .MianownikSkali = LiczbaD(Tekst(em, "ScaleDenominator")),
                    .NarozA = LiczbaD(naroz(0)), .NarozB = LiczbaD(naroz(1)),
                    .SzerokoscKafla = LiczbaC(Tekst(em, "TileWidth"), 256), .WysokoscKafla = LiczbaC(Tekst(em, "TileHeight"), 256),
                    .LiczbaKolumn = LiczbaC(Tekst(em, "MatrixWidth"), 0), .LiczbaWierszy = LiczbaC(Tekst(em, "MatrixHeight"), 0)})
            Next
            u.Zestawy.Add(z)
        Next

        For Each ew In Dzieci(zawartosc, "Layer")
            Dim w As New WarstwaWmts With {.Identyfikator = Tekst(ew, "Identifier"), .Tytul = Tekst(ew, "Title")}
            For Each ef In Dzieci(ew, "Format")
                w.Formaty.Add(ef.Value.Trim())
            Next
            Dim style = Dzieci(ew, "Style").ToList()
            Dim domyslny = style.FirstOrDefault(Function(s) Atrybut(s, "isDefault").Equals("true", StringComparison.OrdinalIgnoreCase))
            If domyslny Is Nothing Then domyslny = style.FirstOrDefault()
            If domyslny IsNot Nothing AndAlso Tekst(domyslny, "Identifier") <> "" Then w.Styl = Tekst(domyslny, "Identifier")
            For Each el In Dzieci(ew, "TileMatrixSetLink")
                Dim p As New PowiazanieZestawu With {.Zestaw = Tekst(el, "TileMatrixSet")}
                Dim limity = Dziecko(el, "TileMatrixSetLimits")
                If limity IsNot Nothing Then
                    For Each lim In Dzieci(limity, "TileMatrixLimits")
                        p.Limity(Tekst(lim, "TileMatrix")) = {LiczbaC(Tekst(lim, "MinTileRow"), 0), LiczbaC(Tekst(lim, "MaxTileRow"), Integer.MaxValue),
                                                              LiczbaC(Tekst(lim, "MinTileCol"), 0), LiczbaC(Tekst(lim, "MaxTileCol"), Integer.MaxValue)}
                    Next
                End If
                w.Powiazania.Add(p)
            Next
            For Each er In Dzieci(ew, "ResourceURL")
                If Atrybut(er, "resourceType").Equals("tile", StringComparison.OrdinalIgnoreCase) Then
                    w.Szablony.Add(New SzablonKafla With {.Format = Atrybut(er, "format"), .Szablon = Atrybut(er, "template")})
                End If
            Next
            For Each ed In Dzieci(ew, "Dimension")
                w.Wymiary(Tekst(ed, "Identifier")) = Tekst(ed, "Default")
            Next
            u.Warstwy.Add(w)
        Next

        'adres GetTile w trybie KVP z sekcji OperationsMetadata
        Dim operacje = Dziecko(korzen, "OperationsMetadata")
        If operacje IsNot Nothing Then
            For Each op In Dzieci(operacje, "Operation")
                If Atrybut(op, "name") <> "GetTile" Then Continue For
                For Each eg In op.Descendants().Where(Function(x) x.Name.LocalName = "Get")
                    Dim kodowania = eg.Descendants().Where(Function(x) x.Name.LocalName = "Value").Select(Function(x) x.Value.Trim()).ToList()
                    If kodowania.Count = 0 OrElse kodowania.Any(Function(k) k.Equals("KVP", StringComparison.OrdinalIgnoreCase)) Then
                        u.AdresKvp = Atrybut(eg, "href")
                        If u.AdresKvp = "" Then u.AdresKvp = If(adresCapabilities, "")
                        Exit For
                    End If
                Next
            Next
        End If
        'usługa bez szablonów REST i bez zadeklarowanego KVP - próba KVP pod adresem dokumentu
        If u.AdresKvp = "" AndAlso u.Warstwy.All(Function(w) w.Szablony.Count = 0) Then u.AdresKvp = If(adresCapabilities, "")
        Return u
    End Function

End Class

''' <summary>Pomocnicze funkcje adresów usług WMTS w plikach warstw.</summary>
Public NotInheritable Class Wmts

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Czy adres z pliku warstw wskazuje usługę WMTS (zamiast WMS): parametr SERVICE=WMTS,
    ''' dokument WMTSCapabilities.xml albo segment ścieżki "/WMTS".
    ''' </summary>
    Public Shared Function CzyAdresWmts(adres As String) As Boolean
        If String.IsNullOrEmpty(adres) Then Return False
        Return Regex.IsMatch(adres, "[?&]service=wmts(&|$)", RegexOptions.IgnoreCase) OrElse
               adres.IndexOf("WMTSCapabilities", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               Regex.IsMatch(adres, "/wmts(/|\?|$)", RegexOptions.IgnoreCase)
    End Function

    ''' <summary>Adres dokumentu GetCapabilities: plik XML bez zmian, dla adresu KVP - uzupełnienie parametrów.</summary>
    Public Shared Function AdresCapabilities(adres As String) As String
        Dim a As String = adres.Trim()
        If Regex.IsMatch(a, "\.xml(\?|$)", RegexOptions.IgnoreCase) Then Return a
        a = AdresBazowy(a)
        Return a & "SERVICE=WMTS&REQUEST=GetCapabilities"
    End Function

    ''' <summary>Adres bez parametrów SERVICE, REQUEST i VERSION, zakończony znakiem "?" lub "&amp;" (do dopisania parametrów).</summary>
    Public Shared Function AdresBazowy(adres As String) As String
        Dim a As String = Regex.Replace(adres.Trim(), "(?<=[?&])(service|request|version)=[^&]*&?", "", RegexOptions.IgnoreCase)
        If a.IndexOf("?"c) < 0 Then Return a & "?"
        If a.EndsWith("?", StringComparison.Ordinal) OrElse a.EndsWith("&", StringComparison.Ordinal) Then Return a
        Return a & "&"
    End Function

    ''' <summary>Typ MIME kafli: zgodny z formatem segmentów, jeśli warstwa go udostępnia, inaczej pierwszy dostępny.</summary>
    Public Shared Function WybierzFormat(dostepne As IList(Of String), formatSegmentow As String) As String
        If dostepne Is Nothing OrElse dostepne.Count = 0 Then Return "image/png"
        Dim f As String = If(formatSegmentow, "").ToLowerInvariant()
        Dim szukany As String = If(f.StartsWith("png", StringComparison.Ordinal), "image/png", If(f = "jpeg" OrElse f = "jpg", "image/jpeg", "image/" & f))
        For Each d In dostepne
            If d.Equals(szukany, StringComparison.OrdinalIgnoreCase) Then Return d
        Next
        For Each d In dostepne
            If d.StartsWith(szukany, StringComparison.OrdinalIgnoreCase) Then Return d
        Next
        Return dostepne(0)
    End Function

End Class

''' <summary>
''' Plan pobierania jednej warstwy WMTS dla siatki segmentów: wybór zestawu macierzy (najlepiej w układzie segmentów -
''' bez przeliczania), wybór poziomu szczegółowości odpowiadającego rozmiarowi piksela segmentów, ustalenie kolejności
''' osi narożnika macierzy i budowa adresów kafli.
''' </summary>
Public Class PlanWmts

    ''' <summary>Dopuszczalny nadmiar rozmiaru piksela kafli względem piksela segmentów przy wyborze poziomu (10%).</summary>
    Public Const TolerancjaRozdzielczosci As Double = 1.1

    Public ReadOnly Property Warstwa As WarstwaWmts
    Public ReadOnly Property Zestaw As ZestawMacierzyWmts
    Public ReadOnly Property Macierz As MacierzKafliWmts
    Public ReadOnly Property UkladKafli As UkladKafliWmts
    Public ReadOnly Property UkladSegmentow As UkladWspolrzednych
    Public ReadOnly Property Format As String
    ''' <summary>Rozmiar piksela kafli w jednostkach układu kafli.</summary>
    Public ReadOnly Property Rozdzielczosc As Double
    ''' <summary>Lewy górny narożnik macierzy w układzie kafli.</summary>
    Public ReadOnly Property Naroznik As PunktXY
    ''' <summary>Narożnik podany w kolejności odwrotnej niż w definicji układu (serwer niezgodny ze standardem).</summary>
    Public ReadOnly Property OsieZamienione As Boolean
    ''' <summary>Liczba próbek na bok piksela segmentu (gdy kafle są wyraźnie dokładniejsze niż segmenty).</summary>
    Public ReadOnly Property Nadprobkowanie As Integer
    ''' <summary>Układ kafli zgodny z układem segmentów - bez przeliczania współrzędnych.</summary>
    Public ReadOnly Property BezPrzeliczania As Boolean

    Private ReadOnly _usluga As UslugaWmts
    Private ReadOnly _szablon As String = ""
    Private _limity As Integer()

    ''' <param name="zasieg">Zasięg całej siatki segmentów.</param>
    ''' <param name="rozmiarPiksela">Rozmiar piksela segmentów w jednostkach układu segmentów.</param>
    ''' <param name="formatSegmentow">Format zapisu segmentów (jpeg, png...) - decyduje o preferowanym formacie kafli.</param>
    Public Sub New(usluga As UslugaWmts, identyfikatorWarstwy As String, uklad As UkladWspolrzednych, zasieg As Zasieg,
                   rozmiarPiksela As Double, formatSegmentow As String)
        _usluga = usluga
        _UkladSegmentow = uklad
        Warstwa = usluga.Warstwa(identyfikatorWarstwy)
        If Warstwa Is Nothing Then
            Dim lista As String = String.Join(", ", usluga.Warstwy.Select(Function(w) w.Identyfikator).Take(30))
            Throw New InvalidOperationException("usługa WMTS nie udostępnia warstwy """ & identyfikatorWarstwy & """ (dostępne: " & lista & ")")
        End If

        'kandydaci: zestawy w obsługiwanych układach - najpierw układ segmentów, potem Web Mercator, potem pozostałe
        Dim kandydaci = Warstwa.Powiazania.
            Select(Function(p) Tuple.Create(p, usluga.Zestaw(p.Zestaw))).
            Where(Function(t) t.Item2 IsNot Nothing AndAlso t.Item2.Uklad IsNot Nothing AndAlso t.Item2.Macierze.Count > 0).
            OrderBy(Function(t) If(t.Item2.Uklad.Epsg = uklad.Epsg, 0, If(t.Item2.Uklad.JestWebMercator, 1, 2))).ToList()
        If kandydaci.Count = 0 Then
            Dim uklady As String = String.Join(", ", Warstwa.Powiazania.Select(Function(p) usluga.Zestaw(p.Zestaw)).
                                               Where(Function(z) z IsNot Nothing).Select(Function(z) z.Crs).Distinct())
            Throw New InvalidOperationException("warstwa WMTS """ & identyfikatorWarstwy & """ nie ma kafli w układzie obsługiwanym przez program (" & uklady & ")")
        End If

        Dim srodekSegmentow As PunktXY = zasieg.Srodek
        For Each k In kandydaci
            Dim uk = k.Item2.Uklad
            Dim przelicz As Func(Of PunktXY, PunktXY) = Przeliczenie(uklad, uk)
            Dim srodek = przelicz(srodekSegmentow)
            Dim najgrubsza = k.Item2.Macierze.OrderByDescending(Function(m) m.MianownikSkali).First()

            'kolejność osi narożnika: wg definicji układu, a gdy obszar nie mieści się w macierzy - odwrotna
            Dim najpierwPolnoc As Boolean = uk.NajpierwPolnoc
            If Not WMacierzy(najgrubsza, uk, najpierwPolnoc, srodek) Then
                If WMacierzy(najgrubsza, uk, Not najpierwPolnoc, srodek) Then
                    najpierwPolnoc = Not najpierwPolnoc
                Else
                    Continue For
                End If
            End If

            'potrzebny rozmiar piksela w układzie kafli: przesunięcie o piksel segmentu na wschód i na północ
            Dim pe = przelicz(New PunktXY(srodekSegmentow.X, srodekSegmentow.Y + rozmiarPiksela))
            Dim pn = przelicz(New PunktXY(srodekSegmentow.X + rozmiarPiksela, srodekSegmentow.Y))
            Dim de As Double = Math.Sqrt((pe.X - srodek.X) ^ 2 + (pe.Y - srodek.Y) ^ 2)
            Dim dn As Double = Math.Sqrt((pn.X - srodek.X) ^ 2 + (pn.Y - srodek.Y) ^ 2)
            Dim potrzebny As Double = Math.Min(de, dn)

            'poziom: najmniej szczegółowy spośród wystarczająco dokładnych (dostępnych dla warstwy); brak takiego - najdokładniejszy
            Dim powiazanie = k.Item1
            Dim dostepne = k.Item2.Macierze.Where(Function(m) powiazanie.Limity.Count = 0 OrElse powiazanie.Limity.ContainsKey(m.Identyfikator)).
                           OrderByDescending(Function(m) m.MianownikSkali).ToList()
            If dostepne.Count = 0 Then Continue For
            Dim wybrana As MacierzKafliWmts = Nothing
            For Each m In dostepne
                If m.Rozdzielczosc(uk.MetrowNaJednostke) <= potrzebny * TolerancjaRozdzielczosci Then
                    wybrana = m
                    Exit For
                End If
            Next
            If wybrana Is Nothing Then wybrana = dostepne.Last()

            _Zestaw = k.Item2
            _Macierz = wybrana
            _UkladKafli = uk
            _Rozdzielczosc = wybrana.Rozdzielczosc(uk.MetrowNaJednostke)
            _Naroznik = wybrana.LewyGorny(najpierwPolnoc)
            _OsieZamienione = najpierwPolnoc <> uk.NajpierwPolnoc
            _BezPrzeliczania = uk.Uklad IsNot Nothing AndAlso uk.Uklad.Epsg = uklad.Epsg
            _Nadprobkowanie = Math.Max(1, Math.Min(4, CInt(Math.Ceiling(Math.Max(de, dn) / _Rozdzielczosc - 0.01))))
            If Not powiazanie.Limity.TryGetValue(wybrana.Identyfikator, _limity) Then _limity = Nothing
            Exit For
        Next
        If _Zestaw Is Nothing Then
            Throw New InvalidOperationException("obszar segmentów leży poza zasięgiem kafli warstwy WMTS """ & identyfikatorWarstwy & """")
        End If

        'tryb KVP albo REST (gdy usługa nie udostępnia KVP)
        If usluga.AdresKvp <> "" OrElse Warstwa.Szablony.Count = 0 Then
            Format = Wmts.WybierzFormat(Warstwa.Formaty, formatSegmentow)
        Else
            Format = Wmts.WybierzFormat(Warstwa.Szablony.Select(Function(s) s.Format).ToList(), formatSegmentow)
            _szablon = Warstwa.Szablony.First(Function(s) String.Equals(s.Format, Format, StringComparison.OrdinalIgnoreCase)).Szablon
        End If
    End Sub

    ''' <summary>Przeliczenie z układu segmentów do układu kafli.</summary>
    Private Shared Function Przeliczenie(z As UkladWspolrzednych, doUkladu As UkladKafliWmts) As Func(Of PunktXY, PunktXY)
        If doUkladu.Uklad IsNot Nothing AndAlso doUkladu.Uklad.Epsg = z.Epsg Then Return Function(p) p
        Return Function(p) doUkladu.ZWgs84(z.DoWgs84(p))
    End Function

    Private Shared Function WMacierzy(m As MacierzKafliWmts, uk As UkladKafliWmts, najpierwPolnoc As Boolean, p As PunktXY) As Boolean
        Dim n = m.LewyGorny(najpierwPolnoc)
        Dim res = m.Rozdzielczosc(uk.MetrowNaJednostke)
        Dim kol As Double = (p.Y - n.Y) / res / m.SzerokoscKafla
        Dim wie As Double = (n.X - p.X) / res / m.WysokoscKafla
        Return kol >= 0 AndAlso wie >= 0 AndAlso kol <= m.LiczbaKolumn AndAlso wie <= m.LiczbaWierszy
    End Function

    ''' <summary>Punkt układu segmentów w układzie kafli.</summary>
    Public Function DoUkladuKafli(p As PunktXY) As PunktXY
        If BezPrzeliczania Then Return p
        Return UkladKafli.ZWgs84(UkladSegmentow.DoWgs84(p))
    End Function

    ''' <summary>Położenie punktu (w układzie kafli) w pikselach całej macierzy: (0, 0) - lewy górny narożnik macierzy.</summary>
    Public Sub PikselMacierzy(p As PunktXY, ByRef kolumna As Double, ByRef wiersz As Double)
        kolumna = (p.Y - Naroznik.Y) / Rozdzielczosc
        wiersz = (Naroznik.X - p.X) / Rozdzielczosc
    End Sub

    ''' <summary>Czy kafel istnieje w macierzy (i w zakresie ograniczeń warstwy).</summary>
    Public Function KafelDostepny(wiersz As Integer, kolumna As Integer) As Boolean
        If wiersz < 0 OrElse kolumna < 0 OrElse wiersz >= Macierz.LiczbaWierszy OrElse kolumna >= Macierz.LiczbaKolumn Then Return False
        If _limity Is Nothing Then Return True
        Return wiersz >= _limity(0) AndAlso wiersz <= _limity(1) AndAlso kolumna >= _limity(2) AndAlso kolumna <= _limity(3)
    End Function

    ''' <summary>Adres kafla (KVP GetTile albo szablon REST).</summary>
    Public Function AdresKafla(wiersz As Integer, kolumna As Integer) As String
        Dim w As String = wiersz.ToString(CultureInfo.InvariantCulture)
        Dim k As String = kolumna.ToString(CultureInfo.InvariantCulture)
        If _szablon <> "" Then
            Dim a As String = _szablon
            a = Zamien(a, "TileMatrixSet", Zestaw.Identyfikator)
            a = Zamien(a, "TileMatrix", Macierz.Identyfikator)
            a = Zamien(a, "TileRow", w)
            a = Zamien(a, "TileCol", k)
            a = Zamien(a, "Style", Warstwa.Styl)
            For Each wym In Warstwa.Wymiary
                a = Zamien(a, wym.Key, wym.Value)
            Next
            Return a
        End If
        Return Wmts.AdresBazowy(_usluga.AdresKvp) & "SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0" &
               "&LAYER=" & Uri.EscapeDataString(Warstwa.Identyfikator) & "&STYLE=" & Uri.EscapeDataString(Warstwa.Styl) &
               "&FORMAT=" & Uri.EscapeDataString(Format) & "&TILEMATRIXSET=" & Uri.EscapeDataString(Zestaw.Identyfikator) &
               "&TILEMATRIX=" & Uri.EscapeDataString(Macierz.Identyfikator) & "&TILEROW=" & w & "&TILECOL=" & k
    End Function

    Private Shared Function Zamien(tekst As String, nazwa As String, wartosc As String) As String
        Return Regex.Replace(tekst, "\{" & Regex.Escape(nazwa) & "\}", wartosc.Replace("$", "$$"), RegexOptions.IgnoreCase)
    End Function

    ''' <summary>Opis planu do komunikatów i plików z wykazem segmentów.</summary>
    Public ReadOnly Property Opis As String
        Get
            Return "WMTS " & Warstwa.Identyfikator & ", zestaw " & Zestaw.Identyfikator & ", poziom " & Macierz.Identyfikator &
                   ", piksel " & Rozdzielczosc.ToString("0.######", CultureInfo.InvariantCulture) & If(UkladKafli.Geograficzny, "°", " m") &
                   If(BezPrzeliczania, "", ", przeliczane z " & Zestaw.Crs)
        End Get
    End Property

End Class
