'Copyright (C) <2026>  MapoTero
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <summary>Informacje o warstwie WMS z dokumentu GetCapabilities.</summary>
Public Class WarstwaWmsInfo

    ''' <summary>Identyfikator techniczny warstwy (używany w parametrze layers= zapytania GetMap).</summary>
    Public Property Nazwa As String = ""
    ''' <summary>Czytelny tytuł warstwy.</summary>
    Public Property Tytul As String = ""
    ''' <summary>Streszczenie / opis warstwy.</summary>
    Public Property Abstrakt As String = ""
    ''' <summary>Mianownik skali minimalnej (poniżej tej skali serwer nie wyświetla danych, np. 5000 dla 1:5000).</summary>
    Public Property MinScaleDenominator As Double = 0
    ''' <summary>Mianownik skali maksymalnej.</summary>
    Public Property MaxScaleDenominator As Double = 0
    ''' <summary>Identyfikator numeryczny warstwy (np. z parametru LAYER= w LegendURL dla serwerów ArcGIS).</summary>
    Public Property IdentyfikatorNumeryczny As String = ""
    ''' <summary>Deklarowany w Capabilities zasięg w układzie PL-1992 (EPSG:2180) dla dokładnego próbkowania.</summary>
    Public Property Zasieg2180 As Zasieg? = Nothing
    ''' <summary>Kody CRS/SRS deklarowane dla warstwy (np. "EPSG:2180", "EPSG:2177", "EPSG:4326").</summary>
    Public ReadOnly Property ObslugiwaneCrs As New List(Of String)
    ''' <summary>Podwarstwy w drzewie hierarchii.</summary>
    Public ReadOnly Property Dzieci As New List(Of WarstwaWmsInfo)
    ''' <summary>Referencja do węzła nadrzędnego (Nothing dla korzenia).</summary>
    Public Property Rodzic As WarstwaWmsInfo

    ''' <summary>Czy węzeł jest grupą/katalogiem zawierającym podwarstwy.</summary>
    Public ReadOnly Property CzyGrupa As Boolean
        Get
            Return Dzieci.Count > 0
        End Get
    End Property

    ''' <summary>Czy warstwa posiada nazwę techniczną i może być bezpośrednio pobierana zapytaniem GetMap.</summary>
    Public ReadOnly Property CzyWarstwaDoPobrania As Boolean
        Get
            Return Not String.IsNullOrEmpty(Nazwa)
        End Get
    End Property

    ''' <summary>
    ''' Automatycznie wyliczony zalecany rozmiar piksela [m/pix]:
    ''' z mianownika skali minimalnej (skala * 0.00028) zaokrąglony do typowych wartości (0.25, 0.5, 1, 2, 4...)
    ''' lub domyślny 1.0 m/pix.
    ''' </summary>
    Public Function ZalecanyRozmiarPiksela() As Double
        If MinScaleDenominator > 0 Then
            Dim px As Double = MinScaleDenominator * 0.00028
            If px <= 0.35 Then Return 0.25
            If px <= 0.75 Then Return 0.5
            If px <= 1.5 Then Return 1.0
            If px <= 3.0 Then Return 2.0
            If px <= 6.0 Then Return 4.0
            If px <= 12.0 Then Return 8.0
            Return Math.Round(px, 1)
        End If

        'heurystyka po nazwie warstwy
        Dim n As String = Nazwa.ToLowerInvariant()
        Dim t As String = Tytul.ToLowerInvariant()
        If n.Contains("orto") OrElse t.Contains("orto") Then Return 0.25
        'od najmniejszej skali - "100" zawiera "10", a "1:100" zawiera "1:10" (warstwa 1:100 000 dostawała piksel 1 m)
        If n.Contains("100") OrElse t.Contains("1:100") Then Return 8.0
        If n.Contains("50") OrElse t.Contains("1:50") Then Return 4.0
        If n.Contains("25") OrElse t.Contains("1:25") Then Return 2.0
        If n.Contains("10") OrElse t.Contains("1:10") Then Return 1.0
        If n.Contains("dzialk") OrElse t.Contains("działk") OrElse n.Contains("budyn") Then Return 0.5
        Return 1.0
    End Function

    ''' <summary>Sprawdza, czy warstwa deklaruje obsługę danego kodu EPSG (uwzględnia dziedziczenie z rodziców).</summary>
    Public Function ObslugujeEpsg(epsg As Integer) As Boolean
        Dim szukany As String = "EPSG:" & epsg.ToString(CultureInfo.InvariantCulture)
        Dim w As WarstwaWmsInfo = Me
        While w IsNot Nothing
            For Each c In w.ObslugiwaneCrs
                If c.IndexOf(szukany, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
            Next
            w = w.Rodzic
        End While
        Return False
    End Function

    Public Overrides Function ToString() As String
        If Not String.IsNullOrEmpty(Tytul) AndAlso Not String.IsNullOrEmpty(Nazwa) AndAlso Tytul <> Nazwa Then
            Return Tytul & " (" & Nazwa & ")"
        End If
        Return If(Not String.IsNullOrEmpty(Tytul), Tytul, Nazwa)
    End Function

End Class

''' <summary>Opis usługi WMS odczytany z dokumentu GetCapabilities.</summary>
Public Class UslugaWmsInfo

    ''' <summary>Wersja WMS serwera (np. "1.3.0" lub "1.1.1").</summary>
    Public Property Wersja As String = "1.3.0"
    ''' <summary>Tytuł usługi (z sekcji Service/Title).</summary>
    Public Property Tytul As String = ""
    ''' <summary>Opis / abstrakt usługi.</summary>
    Public Property Abstrakt As String = ""
    ''' <summary>Formaty graficzne GetMap obsługiwane przez serwer (np. "image/png", "image/jpeg").</summary>
    Public ReadOnly Property Formaty As New List(Of String)
    ''' <summary>Adres punktu końcowego GetMap (z sekcji OnlineResource lub adres zgłoszenia).</summary>
    Public Property AdresGetMap As String = ""
    ''' <summary>Warstwy główne (korzenie drzewa).</summary>
    Public ReadOnly Property Warstwy As New List(Of WarstwaWmsInfo)
    ''' <summary>Maksymalna dopuszczalna szerokość pojedynczego kafelka (0 = brak limitu).</summary>
    Public Property MaxWidth As Integer = 0
    ''' <summary>Maksymalna dopuszczalna wysokość pojedynczego kafelka (0 = brak limitu).</summary>
    Public Property MaxHeight As Integer = 0

    ''' <summary>Płaska lista wszystkich warstw, które można pobrać (posiadają nazwę techniczną).</summary>
    Public Function WszystkieWarstwyDoPobrania() As List(Of WarstwaWmsInfo)
        Dim wynik As New List(Of WarstwaWmsInfo)
        ZbierzWarstwy(Warstwy, wynik)
        Return wynik
    End Function

    Private Shared Sub ZbierzWarstwy(zrodlo As IEnumerable(Of WarstwaWmsInfo), cel As List(Of WarstwaWmsInfo))
        For Each w In zrodlo
            If w.CzyWarstwaDoPobrania Then cel.Add(w)
            ZbierzWarstwy(w.Dzieci, cel)
        Next
    End Sub

    ''' <summary>Wyszukuje warstwę o podanej nazwie technicznej.</summary>
    Public Function ZnajdzWarstwe(nazwa As String) As WarstwaWmsInfo
        Return WszystkieWarstwyDoPobrania().FirstOrDefault(Function(w) String.Equals(w.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase))
    End Function

End Class

''' <summary>Parser dokumentu GetCapabilities usługi WMS (wersje 1.3.0 i 1.1.1).</summary>
Public NotInheritable Class WmsCapabilities

    Private Sub New()
    End Sub

    ''' <summary>Parsuje dokument GetCapabilities z tekstu XML.</summary>
    Public Shared Function Wczytaj(xml As String, adresZgloszenia As String) As UslugaWmsInfo
        If String.IsNullOrWhiteSpace(xml) Then Throw New ArgumentException("Pusty dokument XML.", NameOf(xml))
        'tekst jest już zdekodowany - dawniej był kodowany w UTF-8 i ponownie dekodowany wg deklaracji encoding
        '(np. ISO-8859-2), co przekłamywało polskie znaki
        Return ZDokumentu(XDocument.Parse(xml), adresZgloszenia)
    End Function

    ''' <summary>
    ''' Parsuje dokument GetCapabilities ze strumienia bajtów.
    ''' XDocument.Load ze strumienia automatycznie odczytuje deklarację kodowania (UTF-8, ISO-8859-2, Windows-1250).
    ''' </summary>
    Public Shared Function Wczytaj(strumien As Stream, adresZgloszenia As String) As UslugaWmsInfo
        Return ZDokumentu(XDocument.Load(strumien), adresZgloszenia)
    End Function

    Private Shared Function ZDokumentu(doc As XDocument, adresZgloszenia As String) As UslugaWmsInfo
        Dim korzen = doc.Root
        If korzen Is Nothing Then Throw New FormatException("Pusty dokument XML.")

        Dim nazwaKorzenia As String = korzen.Name.LocalName
        If nazwaKorzenia.IndexOf("ServiceExceptionReport", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
           nazwaKorzenia.IndexOf("ExceptionReport", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Dim komunikat As String = korzen.Value.Trim()
            If komunikat.Length > 300 Then komunikat = komunikat.Substring(0, 300)
            Throw New InvalidOperationException("Serwer WMS zgłosił błąd: " & komunikat)
        End If

        If nazwaKorzenia.IndexOf("Capabilities", StringComparison.OrdinalIgnoreCase) < 0 Then
            Throw New FormatException("Dokument XML nie jest dokumentem Capabilities (korzeń: " & nazwaKorzenia & ").")
        End If

        Dim u As New UslugaWmsInfo()

        'wersja standardu
        Dim wersjaAttr = Atrybut(korzen, "version")
        If Not String.IsNullOrEmpty(wersjaAttr) Then
            u.Wersja = wersjaAttr
        Else
            u.Wersja = If(nazwaKorzenia.StartsWith("WMT_MS", StringComparison.OrdinalIgnoreCase), "1.1.1", "1.3.0")
        End If

        'sekcja Service
        Dim secService = Dziecko(korzen, "Service")
        If secService IsNot Nothing Then
            u.Tytul = Tekst(secService, "Title")
            u.Abstrakt = Tekst(secService, "Abstract")
            u.MaxWidth = LiczbaC(Tekst(secService, "MaxWidth"), 0)
            u.MaxHeight = LiczbaC(Tekst(secService, "MaxHeight"), 0)
        End If
        If String.IsNullOrEmpty(u.Tytul) Then u.Tytul = "Serwer WMS (" & u.Wersja & ")"

        'sekcja Capability
        Dim secCap = Dziecko(korzen, "Capability")
        If secCap Is Nothing Then Throw New FormatException("Dokument WMS Capabilities nie zawiera sekcji Capability.")

        'Request -> GetMap (formaty i adres OnlineResource)
        Dim secReq = Dziecko(secCap, "Request")
        If secReq IsNot Nothing Then
            Dim secGetMap = Dziecko(secReq, "GetMap")
            If secGetMap IsNot Nothing Then
                For Each ef In Dzieci(secGetMap, "Format")
                    Dim fmt As String = ef.Value.Trim()
                    If Not String.IsNullOrEmpty(fmt) AndAlso Not u.Formaty.Contains(fmt) Then u.Formaty.Add(fmt)
                Next

                Dim secDcp = Dziecko(secGetMap, "DCPType")
                Dim secHttp = If(secDcp IsNot Nothing, Dziecko(secDcp, "HTTP"), Nothing)
                Dim secGet = If(secHttp IsNot Nothing, Dziecko(secHttp, "Get"), Nothing)
                Dim secRes = If(secGet IsNot Nothing, Dziecko(secGet, "OnlineResource"), Nothing)
                If secRes IsNot Nothing Then
                    Dim href As String = AtrybutXlink(secRes, "href")
                    If Not String.IsNullOrEmpty(href) AndAlso Not JestAdresemLokalnym(href) Then
                        u.AdresGetMap = href
                    End If
                End If
            End If
        End If

        If String.IsNullOrEmpty(u.AdresGetMap) Then
            u.AdresGetMap = AdresGetMapZAdresuZgloszenia(adresZgloszenia)
        End If

        'drzewo warstw
        For Each el In Dzieci(secCap, "Layer")
            Dim w = ParsujWarstwe(el, Nothing, u.Wersja)
            If w IsNot Nothing Then u.Warstwy.Add(w)
        Next

        Return u
    End Function

    Private Shared Function ParsujWarstwe(el As XElement, rodzic As WarstwaWmsInfo, wersja As String) As WarstwaWmsInfo
        Dim w As New WarstwaWmsInfo With {
            .Rodzic = rodzic,
            .Nazwa = Tekst(el, "Name"),
            .Tytul = Tekst(el, "Title"),
            .Abstrakt = Tekst(el, "Abstract")
        }

        'skale
        Dim minScale = Tekst(el, "MinScaleDenominator")
        If Not String.IsNullOrEmpty(minScale) Then
            w.MinScaleDenominator = LiczbaD(minScale, 0)
        Else
            'WMS 1.1.1: ScaleHint (diagonal px w metrach)
            Dim sh = Dziecko(el, "ScaleHint")
            If sh IsNot Nothing Then
                Dim minSh = Atrybut(sh, "min")
                If Not String.IsNullOrEmpty(minSh) Then
                    Dim dVal As Double = LiczbaD(minSh, 0)
                    'ScaleHint = skan * 0.00028 * sqrt(2)
                    If dVal > 0 Then w.MinScaleDenominator = dVal / (0.00028 * 1.4142)
                End If
            End If
        End If

        Dim maxScale = Tekst(el, "MaxScaleDenominator")
        If Not String.IsNullOrEmpty(maxScale) Then w.MaxScaleDenominator = LiczbaD(maxScale, 0)

        'układy CRS / SRS
        Dim tagUkladu As String = If(wersja.StartsWith("1.1", StringComparison.Ordinal), "SRS", "CRS")
        For Each ec In Dzieci(el, tagUkladu)
            For Each c In ec.Value.Split({" "c, ChrW(9), ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
                Dim trimmed As String = c.Trim().ToUpperInvariant()
                If Not w.ObslugiwaneCrs.Contains(trimmed) Then w.ObslugiwaneCrs.Add(trimmed)
            Next
        Next
        'niektóre serwery 1.3.0 deklarują SRS obok CRS
        If tagUkladu = "CRS" Then
            For Each ec In Dzieci(el, "SRS")
                For Each c In ec.Value.Split({" "c, ChrW(9), ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
                    Dim trimmed As String = c.Trim().ToUpperInvariant()
                    If Not w.ObslugiwaneCrs.Contains(trimmed) Then w.ObslugiwaneCrs.Add(trimmed)
                Next
            Next
        End If

        'podwarstwy
        For Each ed In Dzieci(el, "Layer")
            Dim dziecko = ParsujWarstwe(ed, w, wersja)
            If dziecko IsNot Nothing Then w.Dzieci.Add(dziecko)
        Next

        'LegendURL -> poszukiwanie parametru LAYER=(\d+) dla serwerów ArcGIS MapServer
        Dim secStyle = Dziecko(el, "Style")
        If secStyle IsNot Nothing Then
            Dim secLeg = Dziecko(secStyle, "LegendURL")
            If secLeg IsNot Nothing Then
                Dim secOnline = Dziecko(secLeg, "OnlineResource")
                If secOnline IsNot Nothing Then
                    Dim legHref = AtrybutXlink(secOnline, "href")
                    Dim legZdekodowany = System.Net.WebUtility.UrlDecode(legHref)
                    Dim mLeg = System.Text.RegularExpressions.Regex.Match(legZdekodowany, "[?&]layer=(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    If mLeg.Success Then
                        w.IdentyfikatorNumeryczny = mLeg.Groups(1).Value
                    End If
                End If
            End If
        End If

        'BoundingBox w EPSG:2180
        For Each ebb In Dzieci(el, "BoundingBox")
            Dim crs = Atrybut(ebb, "CRS")
            If String.IsNullOrEmpty(crs) Then crs = Atrybut(ebb, "SRS")
            If crs.IndexOf("2180", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Dim minx = LiczbaD(Atrybut(ebb, "minx"), 0)
                Dim miny = LiczbaD(Atrybut(ebb, "miny"), 0)
                Dim maxx = LiczbaD(Atrybut(ebb, "maxx"), 0)
                Dim maxy = LiczbaD(Atrybut(ebb, "maxy"), 0)
                Dim x1 = Math.Min(minx, maxx)
                Dim x2 = Math.Max(minx, maxx)
                Dim y1 = Math.Min(miny, maxy)
                Dim y2 = Math.Max(miny, maxy)
                If x2 > x1 AndAlso y2 > y1 Then
                    w.Zasieg2180 = New Zasieg(x1, y1, x2, y2)
                End If
            End If
        Next
        If Not w.Zasieg2180.HasValue AndAlso rodzic IsNot Nothing Then
            w.Zasieg2180 = rodzic.Zasieg2180
        End If

        Return w
    End Function

    Private Shared Function Dzieci(e As XElement, nazwa As String) As IEnumerable(Of XElement)
        Return e.Elements().Where(Function(x) x.Name.LocalName.Equals(nazwa, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function Dziecko(e As XElement, nazwa As String) As XElement
        Return Dzieci(e, nazwa).FirstOrDefault()
    End Function

    Private Shared Function Tekst(e As XElement, nazwa As String) As String
        Dim d = Dziecko(e, nazwa)
        Return If(d Is Nothing, "", d.Value.Trim())
    End Function

    Private Shared Function Atrybut(e As XElement, nazwa As String) As String
        Dim a = e.Attributes().FirstOrDefault(Function(x) x.Name.LocalName.Equals(nazwa, StringComparison.OrdinalIgnoreCase))
        Return If(a Is Nothing, "", a.Value.Trim())
    End Function

    Private Shared Function AtrybutXlink(e As XElement, nazwa As String) As String
        Dim a = e.Attributes().FirstOrDefault(Function(x) x.Name.LocalName.Equals(nazwa, StringComparison.OrdinalIgnoreCase))
        Return If(a Is Nothing, "", a.Value.Trim())
    End Function

    Private Shared Function LiczbaD(tekst As String, domyslna As Double) As Double
        Dim w As Double
        If Double.TryParse(tekst.Trim().Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture, w) Then Return w
        Return domyslna
    End Function

    Private Shared Function LiczbaC(tekst As String, domyslna As Integer) As Integer
        Dim w As Integer
        If Integer.TryParse(tekst.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, w) Then Return w
        Return domyslna
    End Function

    ''' <summary>
    ''' Czy adres OnlineResource nie nadaje się do zapytań z zewnątrz: adres względny lub niepoprawny, localhost albo
    ''' adres IP sieci lokalnej (10.x, 172.16-31.x, 192.168.x, 127.x). Sprawdzana jest sama nazwa serwera - dawniej
    ''' wystarczał ciąg "10." w dowolnym miejscu adresu (np. ".../services/10.8/..."), a wtedy zbiór map dostawał
    ''' adres dokumentu GetCapabilities.
    ''' </summary>
    Public Shared Function JestAdresemLokalnym(adres As String) As Boolean
        If String.IsNullOrEmpty(adres) Then Return True
        Dim u As Uri = Nothing
        If Not Uri.TryCreate(adres, UriKind.Absolute, u) OrElse (u.Scheme <> Uri.UriSchemeHttp AndAlso u.Scheme <> Uri.UriSchemeHttps) Then Return True
        If u.IsLoopback OrElse u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) Then Return True
        Dim ip As Net.IPAddress = Nothing
        If Net.IPAddress.TryParse(u.Host, ip) AndAlso ip.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
            Dim b() As Byte = ip.GetAddressBytes()
            Return b(0) = 10 OrElse b(0) = 127 OrElse (b(0) = 192 AndAlso b(1) = 168) OrElse (b(0) = 172 AndAlso b(1) >= 16 AndAlso b(1) <= 31)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Adres GetMap z adresu zgłoszenia GetCapabilities (gdy dokument nie podaje użytecznego adresu GetMap):
    ''' bez parametrów SERVICE, REQUEST i VERSION - inaczej w zbiorze map zostawał parametr REQUEST=GetCapabilities,
    ''' który serwer odczytywał przed REQUEST=GetMap i zamiast obrazów zwracał opis usługi.
    ''' </summary>
    Public Shared Function AdresGetMapZAdresuZgloszenia(adresZgloszenia As String) As String
        If String.IsNullOrWhiteSpace(adresZgloszenia) Then Return ""
        Return Wms.UzupelnijAdres(Wmts.AdresBazowy(adresZgloszenia))
    End Function

End Class
