'Copyright (C) <2026>  MapoTero

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
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <summary>Parametry usługi kafelków XYZ / TMS (Web Mercator EPSG:3857).</summary>
Public Class OpcjeKafliXyz
    Public Property Typ As TypUslugiMapowej = TypUslugiMapowej.Xyz
    Public ReadOnly Property Subdomeny As New List(Of String)
    Public Property ZMin As Integer = 0
    Public Property ZMax As Integer = 18
    Public Property RozmiarKafla As Integer = 256
    ''' <summary>Warstwa przezroczysta (nakładka, np. szlaki turystyczne): HTTP 404 oznacza brak danych na kaflu.</summary>
    Public Property Nakladka As Boolean
    ''' <summary>Minimalny odstęp czasu między kolejnymi zapytaniami do serwera [ms].</summary>
    Public Property OdstepMs As Integer = 100
    ''' <summary>Maksymalna liczba jednoczesnych zapytań do serwera (0 - wg ustawień programu).</summary>
    Public Property MaksWatkow As Integer = 2
    Public Property Referer As String = ""
    Public Property Atrybucja As String = ""
    ''' <summary>Wykryty lub jawny typ MIME obrazu kafla (np. image/jpeg dla adresów bez rozszerzenia).</summary>
    Public Property FormatObrazu As String = ""
    ''' <summary>Ignoruj błędy certyfikatu SSL przy pobieraniu kafli z tego serwera.</summary>
    Public Property IgnorujSsl As Boolean
    ''' <summary>Opcjonalny zasięg danych w WGS84 - ogranicza zapytania do wskazanego obszaru.</summary>
    Public Property ZasiegGeo As ProstokatGeo?

    Public Function Kopia() As OpcjeKafliXyz
        Dim k As New OpcjeKafliXyz With {
            .Typ = Typ, .ZMin = ZMin, .ZMax = ZMax, .RozmiarKafla = RozmiarKafla,
            .Nakladka = Nakladka, .OdstepMs = OdstepMs, .MaksWatkow = MaksWatkow,
            .Referer = Referer, .Atrybucja = Atrybucja, .FormatObrazu = FormatObrazu,
            .IgnorujSsl = IgnorujSsl, .ZasiegGeo = ZasiegGeo}
        k.Subdomeny.AddRange(Subdomeny)
        Return k
    End Function
End Class

''' <summary>Syntetyczna usługa XYZ/TMS zbioru map z parametrami kafli (wynik <see cref="KafleXyz.UtworzUslugeZbioru"/>).</summary>
Public Class UslugaXyzZbioru
    Public Property Usluga As UslugaWmts
    Public Property OpcjeBazowe As OpcjeKafliXyz
    ''' <summary>Parametry kafli poszczególnych warstw (z nadpisaniami zakresu powiększeń i roli nakładki).</summary>
    Public ReadOnly Property OpcjeWarstw As New Dictionary(Of String, OpcjeKafliXyz)(StringComparer.Ordinal)
End Class

''' <summary>Wynik odczytu dokumentu TileJSON.</summary>
Public Class WynikTileJson
    Public Property Nazwa As String = ""
    Public Property Opis As String = ""
    Public Property Szablon As String = ""
    Public ReadOnly Property Opcje As New OpcjeKafliXyz()
End Class

''' <summary>Wynik odczytu dokumentu OSGeo TMS (tilemapresource.xml).</summary>
Public Class WynikTmsXml
    Public Property Tytul As String = ""
    Public Property Opis As String = ""
    Public Property Szablon As String = ""
    Public Property Srs As String = ""
    Public ReadOnly Property Opcje As New OpcjeKafliXyz() With {.Typ = TypUslugiMapowej.Tms}
End Class

''' <summary>
''' Obsługa prostych serwerów kafli XYZ / TMS (z/x/y w układzie Web Mercator EPSG:3857)
''' poprzez syntetyczną usługę <see cref="UslugaWmts"/>, bez pobierania dokumentu GetCapabilities.
''' </summary>
Public NotInheritable Class KafleXyz

    Private Const PolosR As Double = 6378137.0
    Private Const PolowaSwiata As Double = Math.PI * PolosR
    Private Const ObwodSwiata As Double = 2.0 * Math.PI * PolosR

    Private Sub New()
    End Sub

    ''' <summary>Czy adres zawiera szablony współrzędnych kafelka XYZ lub TMS ({z}, {x}, {y} albo {-y}) albo klucz kafla Bing ({quadkey}).</summary>
    Public Shared Function CzyAdresXyz(adres As String) As Boolean
        If String.IsNullOrWhiteSpace(adres) Then Return False
        If adres.IndexOf("{quadkey}", StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Return adres.IndexOf("{z}", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
               adres.IndexOf("{x}", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
               (adres.IndexOf("{y}", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                adres.IndexOf("{-y}", StringComparison.OrdinalIgnoreCase) >= 0)
    End Function

    ''' <summary>Czy szablon używa odwróconej osi Y schematu TMS ({-y}).</summary>
    Public Shared Function CzyAdresTms(adres As String) As Boolean
        If String.IsNullOrWhiteSpace(adres) Then Return False
        Return adres.IndexOf("{-y}", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>
    ''' Rozstrzyga typ usługi: jawny typ ze zbioru ma pierwszeństwo, następnie szablon XYZ/TMS
    ''' (sprawdzany przed WMTS, żeby adresy XYZ zawierające "/wmts/" w ścieżce nie wywoływały GetCapabilities).
    ''' </summary>
    Public Shared Function WybierzTyp(adres As String, Optional jawnyTyp As TypUslugiMapowej = TypUslugiMapowej.Nieznana) As TypUslugiMapowej
        If jawnyTyp <> TypUslugiMapowej.Nieznana Then Return jawnyTyp
        If CzyAdresTms(adres) Then Return TypUslugiMapowej.Tms
        If CzyAdresXyz(adres) Then Return TypUslugiMapowej.Xyz
        If Wmts.CzyAdresWmts(adres) Then Return TypUslugiMapowej.Wmts
        Return TypUslugiMapowej.Wms
    End Function

    ''' <summary>Czy typ usługi jest kafelkowy (WMTS, XYZ lub TMS).</summary>
    Public Shared Function CzyKafelkowy(typ As TypUslugiMapowej) As Boolean
        Return typ = TypUslugiMapowej.Wmts OrElse typ = TypUslugiMapowej.Xyz OrElse typ = TypUslugiMapowej.Tms
    End Function

    ''' <summary>Czytelna etykieta typu usługi (WMS, WMTS, XYZ lub TMS).</summary>
    Public Shared Function NazwaTypu(typ As TypUslugiMapowej) As String
        Select Case typ
            Case TypUslugiMapowej.Wmts : Return "WMTS"
            Case TypUslugiMapowej.Xyz : Return "XYZ"
            Case TypUslugiMapowej.Tms : Return "TMS"
            Case Else : Return "WMS"
        End Select
    End Function

    ''' <summary>
    ''' Normalizuje szablon URL: zamienia zapisy subdomen w stylu Leaflet/JOSM ({a|b|c}, {a,b,c}, {switch:a,b,c})
    ''' na standardowe {s} i wypełnia listę subdomen. Wykrywa też {-y}.
    ''' </summary>
    Public Shared Function NormalizujSzablon(adres As String, ByRef subdomeny As List(Of String), ByRef jestTms As Boolean) As String
        subdomeny = New List(Of String)()
        jestTms = CzyAdresTms(adres)
        If String.IsNullOrWhiteSpace(adres) Then Return ""
        Dim a As String = adres.Trim()
        Dim wykryte As New List(Of String)()
        a = Regex.Replace(a, "\{([^}]+)\}",
                          Function(m)
                              Dim zaw As String = m.Groups(1).Value.Trim()
                              If zaw.StartsWith("switch:", StringComparison.OrdinalIgnoreCase) Then
                                  Dim czesci = zaw.Substring(7).Split({","c, "|"c}, StringSplitOptions.RemoveEmptyEntries)
                                  For Each c In czesci
                                      Dim t = c.Trim()
                                      If t <> "" Then wykryte.Add(t)
                                  Next
                                  Return "{s}"
                              End If
                              If zaw.IndexOf("|"c) >= 0 OrElse zaw.IndexOf(","c) >= 0 Then
                                  Dim czesci = zaw.Split({","c, "|"c}, StringSplitOptions.RemoveEmptyEntries)
                                  For Each c In czesci
                                      Dim t = c.Trim()
                                      If t <> "" Then wykryte.Add(t)
                                  Next
                                  Return "{s}"
                              End If
                              Return m.Value
                          End Function)
        subdomeny = wykryte.Distinct(StringComparer.Ordinal).ToList()
        Return a
    End Function

    ''' <summary>
    ''' Klucz serwera używany przez regulator zapytań: dla szablonów z {s} zwraca wspólny identyfikator
    ''' bez podstawionej subdomeny, dzięki czemu subdomeny (a, b, c) współdzielą jeden limit.
    ''' </summary>
    Public Shared Function KluczSerwera(adres As String) As String
        If String.IsNullOrWhiteSpace(adres) Then Return ""
        Dim s As String = Regex.Replace(adres.Trim(), "\{[^}]+\}", "_x_")
        Dim uri As Uri = Nothing
        If Uri.TryCreate(s, UriKind.Absolute, uri) AndAlso Not String.IsNullOrEmpty(uri.Authority) Then
            Return uri.Authority.ToLowerInvariant()
        End If
        Return adres.Trim().ToLowerInvariant()
    End Function

    ''' <summary>
    ''' Buduje syntetyczną usługę <see cref="UslugaWmts"/> (zestaw GoogleMapsCompatible EPSG:3857)
    ''' dla jednej lub wielu warstw serwera XYZ/TMS.
    ''' </summary>
    Public Shared Function UtworzUsluge(adresSzablonu As String, nazwyWarstw As IEnumerable(Of String),
                                        Optional opcje As OpcjeKafliXyz = Nothing,
                                        Optional opcjeWarstw As IDictionary(Of String, OpcjeKafliXyz) = Nothing) As UslugaWmts
        Dim bazowe As OpcjeKafliXyz = If(opcje, New OpcjeKafliXyz())
        Dim nazwy = If(nazwyWarstw, Array.Empty(Of String)()).Where(Function(n) Not String.IsNullOrEmpty(n)).Distinct(StringComparer.Ordinal).ToList()
        If nazwy.Count = 0 Then nazwy.Add("mapa")

        Dim minZ As Integer = Math.Max(0, Math.Min(22, bazowe.ZMin))
        Dim maxZ As Integer = Math.Max(minZ, Math.Min(22, bazowe.ZMax))
        Dim rozmiarKafla As Integer = Math.Max(64, bazowe.RozmiarKafla)
        If opcjeWarstw IsNot Nothing Then
            For Each n In nazwy
                Dim ow As OpcjeKafliXyz = Nothing
                If opcjeWarstw.TryGetValue(n, ow) AndAlso ow IsNot Nothing Then
                    minZ = Math.Min(minZ, Math.Max(0, ow.ZMin))
                    maxZ = Math.Max(maxZ, Math.Min(22, ow.ZMax))
                End If
            Next
        End If

        Dim zestaw As New ZestawMacierzyWmts With {
            .Identyfikator = "GoogleMapsCompatible",
            .Crs = "EPSG:3857"
        }
        For z = minZ To maxZ
            Dim liczba As Integer = 1 << z
            Dim mianownik As Double = ObwodSwiata / (rozmiarKafla * CDbl(1L << z)) / 0.00028
            zestaw.Macierze.Add(New MacierzKafliWmts With {
                .Identyfikator = z.ToString(CultureInfo.InvariantCulture),
                .MianownikSkali = mianownik,
                .NarozA = -PolowaSwiata,
                .NarozB = PolowaSwiata,
                .SzerokoscKafla = rozmiarKafla,
                .WysokoscKafla = rozmiarKafla,
                .LiczbaKolumn = liczba,
                .LiczbaWierszy = liczba
            })
        Next

        Dim usluga As New UslugaWmts()
        usluga.Zestawy.Add(zestaw)

        For Each nazwa In nazwy
            Dim ow As OpcjeKafliXyz = bazowe
            If opcjeWarstw IsNot Nothing AndAlso opcjeWarstw.ContainsKey(nazwa) AndAlso opcjeWarstw(nazwa) IsNot Nothing Then
                ow = opcjeWarstw(nazwa)
            End If
            Dim subdomeny As New List(Of String)
            Dim jestTmsZAdresu As Boolean = False
            Dim sz As String = NormalizujSzablon(adresSzablonu, subdomeny, jestTmsZAdresu)
            If ow.Subdomeny.Count > 0 Then subdomeny = ow.Subdomeny.ToList()
            If subdomeny.Count = 0 AndAlso sz.IndexOf("{s}", StringComparison.OrdinalIgnoreCase) >= 0 Then
                subdomeny = New List(Of String) From {"a", "b", "c"}
            End If

            Dim jestTms As Boolean = jestTmsZAdresu OrElse ow.Typ = TypUslugiMapowej.Tms
            sz = Regex.Replace(sz, "\{z\}", "{TileMatrix}", RegexOptions.IgnoreCase)
            sz = Regex.Replace(sz, "\{x\}", "{TileCol}", RegexOptions.IgnoreCase)
            sz = Regex.Replace(sz, "\{-y\}", "{TileRow}", RegexOptions.IgnoreCase)
            sz = Regex.Replace(sz, "\{y\}", "{TileRow}", RegexOptions.IgnoreCase)
            sz = Regex.Replace(sz, "\{layer\}", nazwa.Replace("$", "$$"), RegexOptions.IgnoreCase)
            'Leaflet: {r} to przyrostek "@2x" kafli o podwójnej gęstości pikseli na ekranach retina; program pobiera kafle
            'zwykłej gęstości, tak jak kreator przy próbie serwera (kafle @2x można wybrać, wpisując "@2x" w szablonie)
            sz = Regex.Replace(sz, "\{r\}", "", RegexOptions.IgnoreCase)
            sz = Regex.Replace(sz, "\{quadkey\}", "{QuadKey}", RegexOptions.IgnoreCase)

            Dim fmt As String = If(Not String.IsNullOrWhiteSpace(ow.FormatObrazu), ow.FormatObrazu,
                                   If(Not String.IsNullOrWhiteSpace(bazowe.FormatObrazu), bazowe.FormatObrazu, WykryjFormatZSzablonu(sz)))
            Dim szablonObj As New SzablonKafla With {
                .Format = fmt,
                .Szablon = sz,
                .OdwroconeY = jestTms
            }
            szablonObj.Subdomeny.AddRange(subdomeny)

            Dim pow As New PowiazanieZestawu With {.Zestaw = zestaw.Identyfikator}
            Dim lZMin As Integer = Math.Max(minZ, ow.ZMin)
            Dim lZMax As Integer = Math.Min(maxZ, ow.ZMax)
            If lZMin > minZ OrElse lZMax < maxZ OrElse ow.ZasiegGeo.HasValue Then
                For z = lZMin To lZMax
                    If ow.ZasiegGeo.HasValue AndAlso ow.ZasiegGeo.Value.Poprawny Then
                        Dim r = WebMercator.ZakresKafli(ow.ZasiegGeo.Value, z)
                        pow.Limity(z.ToString(CultureInfo.InvariantCulture)) = {r.Item2, r.Item4, r.Item1, r.Item3}
                    Else
                        Dim maksIndeks As Integer = (1 << z) - 1
                        pow.Limity(z.ToString(CultureInfo.InvariantCulture)) = {0, maksIndeks, 0, maksIndeks}
                    End If
                Next
            End If

            Dim w As New WarstwaWmts With {
                .Identyfikator = nazwa,
                .Tytul = nazwa,
                .TypUslugiOpis = If(jestTms, "TMS", "XYZ")
            }
            w.Formaty.Add(fmt)
            w.Powiazania.Add(pow)
            w.Szablony.Add(szablonObj)
            usluga.Warstwy.Add(w)
        Next

        Return usluga
    End Function

    ''' <summary>Klucz kafla w schemacie Bing (quadkey): po jednej cyfrze 0-3 na każdy poziom powiększenia.</summary>
    Public Shared Function QuadKey(z As Integer, x As Integer, y As Integer) As String
        Dim sb As New System.Text.StringBuilder(Math.Max(0, z))
        For i = z To 1 Step -1
            Dim cyfra As Integer = 0
            Dim maska As Integer = 1 << (i - 1)
            If (x And maska) <> 0 Then cyfra += 1
            If (y And maska) <> 0 Then cyfra += 2
            sb.Append(ChrW(AscW("0"c) + cyfra))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Usługa XYZ/TMS dla warstw zbioru map razem z parametrami kafli całego zbioru i poszczególnych warstw.
    ''' Wspólna dla szacowania liczby kafli przed pobieraniem i dla samego pobierania - oba korzystają z tych samych ustawień.
    ''' </summary>
    ''' <param name="plik">Zbiór map z metadanymi kafli; Nothing - ustawienia domyślne.</param>
    Public Shared Function UtworzUslugeZbioru(adres As String, nazwyWarstw As IEnumerable(Of String), plik As PlikWarstw,
                                              Optional typ As TypUslugiMapowej = TypUslugiMapowej.Xyz) As UslugaXyzZbioru
        Dim wynik As New UslugaXyzZbioru With {
            .OpcjeBazowe = If(plik IsNot Nothing, plik.OpcjeKafli(), New OpcjeKafliXyz With {.Typ = typ})}
        Dim nazwy = If(nazwyWarstw, Array.Empty(Of String)()).Where(Function(n) Not String.IsNullOrEmpty(n)).Distinct(StringComparer.Ordinal).ToList()
        For Each n In nazwy
            wynik.OpcjeWarstw(n) = If(plik IsNot Nothing, plik.OpcjeKafli(n), wynik.OpcjeBazowe.Kopia())
        Next
        wynik.Usluga = UtworzUsluge(adres, nazwy, wynik.OpcjeBazowe, wynik.OpcjeWarstw)
        Return wynik
    End Function

    ''' <summary>
    ''' Czy obszar (zasięg siatki w układzie segmentów) leży w całości poza zasięgiem danych warstwy (pole "zasieg" zbioru map).
    ''' Kafle poza zasięgiem nie są pobierane, więc segmenty z takiego obszaru byłyby puste.
    ''' </summary>
    Public Shared Function ObszarPozaZasiegiem(opcje As OpcjeKafliXyz, uklad As UkladWspolrzednych, obszar As Zasieg) As Boolean
        If opcje Is Nothing OrElse Not opcje.ZasiegGeo.HasValue OrElse Not opcje.ZasiegGeo.Value.Poprawny OrElse uklad Is Nothing Then Return False
        Dim g = opcje.ZasiegGeo.Value
        Dim o = ProstokatGeo.Opisany(uklad, obszar)
        Return o.Poludnie >= g.Polnoc OrElse o.Polnoc <= g.Poludnie OrElse o.Zachod >= g.Wschod OrElse o.Wschod <= g.Zachod
    End Function

    ''' <summary>
    ''' Podpowiedź do błędu HTTP 403 serwera kafli: serwer odrzuca klienta (nagłówek Referer, User-Agent, klucz dostępu),
    ''' więc ponawianie zapytania nic nie zmieni.
    ''' </summary>
    Public Shared Function OpisOdmowyDostepu(referer As String) As String
        If String.IsNullOrWhiteSpace(referer) Then
            Return "serwer odmówił dostępu (HTTP 403). Serwer może wymagać nagłówka Referer (pole ""referer"" w pliku zbioru map) albo klucza dostępu w adresie."
        End If
        Return "serwer odmówił dostępu (HTTP 403) mimo nagłówka Referer " & referer & ". Sprawdź pole ""referer"" i klucz dostępu w adresie zbioru map."
    End Function

    Private Shared Function WykryjFormatZSzablonu(szablon As String) As String
        Dim s As String = If(szablon, "").Split("?"c)(0).ToLowerInvariant()
        If s.EndsWith(".jpg", StringComparison.Ordinal) OrElse s.EndsWith(".jpeg", StringComparison.Ordinal) Then Return "image/jpeg"
        If s.EndsWith(".gif", StringComparison.Ordinal) Then Return "image/gif"
        If s.EndsWith(".tif", StringComparison.Ordinal) OrElse s.EndsWith(".tiff", StringComparison.Ordinal) Then Return "image/tiff"
        Return "image/png"
    End Function

    ''' <summary>
    ''' Szybko szacuje liczbę unikalnych kafli potrzebnych dla podanych planów i siatki segmentów
    ''' (przerywa zliczanie po osiągnięciu <paramref name="limitZliczania"/>).
    ''' </summary>
    Public Shared Function SzacujLiczbeKafli(plany As IEnumerable(Of PlanWmts), siatka As Siatka, uklad As UkladWspolrzednych,
                                             Optional korytarz As Korytarz = Nothing, Optional limitZliczania As Long = 100000L) As Long
        If plany Is Nothing OrElse siatka Is Nothing OrElse Not siatka.Poprawna Then Return 0
        Dim listaPlanow = plany.Where(Function(p) p IsNot Nothing).ToList()
        If listaPlanow.Count = 0 Then Return 0

        If korytarz Is Nothing OrElse Not korytarz.Poprawny Then
            'cały prostokąt: wystarczy wyznaczyć zakres kafli całego zasięgu siatki dla każdego planu
            Dim suma As Long = 0
            Dim z = siatka.ZasiegSiatki
            For Each p In listaPlanow
                Dim r = ZakresKafliObszaru(p, z)
                If r.Item2 >= r.Item1 AndAlso r.Item4 >= r.Item3 Then
                    suma += CLng(r.Item2 - r.Item1 + 1) * (r.Item4 - r.Item3 + 1)
                    If suma >= limitZliczania Then Return suma
                End If
            Next
            Return suma
        End If

        Dim wKorytarzu = korytarz.Segmenty(siatka, uklad)
        If wKorytarzu.Count = 0 Then Return 0
        Dim sumaK As Long = 0
        For pi = 0 To listaPlanow.Count - 1
            Dim p = listaPlanow(pi)
            Dim unikalne As New HashSet(Of Long)
            For w = 1 To siatka.LiczbaWierszy
                For k = 1 To siatka.LiczbaKolumn
                    If Not wKorytarzu.Contains(siatka.NumerKolejny(w, k) - 1) Then Continue For
                    Dim r = ZakresKafliObszaru(p, siatka.Segment(w, k))
                    For row = r.Item1 To r.Item2
                        For col = r.Item3 To r.Item4
                            If Not p.KafelDostepny(row, col) Then Continue For
                            If unikalne.Add(CLng(row) * 100000000L + col) Then
                                If sumaK + unikalne.Count >= limitZliczania Then Return sumaK + unikalne.Count
                            End If
                        Next
                    Next
                Next
            Next
            sumaK += unikalne.Count
        Next
        Return sumaK
    End Function

    Private Shared Function ZakresKafliObszaru(p As PlanWmts, z As Zasieg) As Tuple(Of Integer, Integer, Integer, Integer)
        Dim punkty = {
            z.LewyGorny, z.PrawyGorny, z.PrawyDolny, z.LewyDolny,
            New PunktXY(z.XGora, (z.YLewy + z.YPrawy) / 2),
            New PunktXY(z.XDol, (z.YLewy + z.YPrawy) / 2),
            New PunktXY((z.XDol + z.XGora) / 2, z.YLewy),
            New PunktXY((z.XDol + z.XGora) / 2, z.YPrawy)}
        Dim minK As Double = Double.MaxValue, maksK As Double = Double.MinValue
        Dim minW As Double = Double.MaxValue, maksW As Double = Double.MinValue
        For Each pt In punkty
            Dim pk = p.DoUkladuKafli(pt)
            Dim kol As Double, wie As Double
            p.PikselMacierzy(pk, kol, wie)
            minW = Math.Min(minW, wie) : maksW = Math.Max(maksW, wie)
            minK = Math.Min(minK, kol) : maksK = Math.Max(maksK, kol)
        Next
        Dim mc = p.Macierz
        Dim w0 As Integer = Math.Max(0, CInt(Math.Floor((minW - 2) / mc.WysokoscKafla)))
        Dim w1 As Integer = Math.Min(mc.LiczbaWierszy - 1, CInt(Math.Floor((maksW + 2) / mc.WysokoscKafla)))
        Dim k0 As Integer = Math.Max(0, CInt(Math.Floor((minK - 2) / mc.SzerokoscKafla)))
        Dim k1 As Integer = Math.Min(mc.LiczbaKolumn - 1, CInt(Math.Floor((maksK + 2) / mc.SzerokoscKafla)))
        Return Tuple.Create(w0, w1, k0, k1)
    End Function

    ''' <summary>Odczyt metadanych z dokumentu TileJSON.</summary>
    Public Shared Function ParsujTileJson(jsonTekst As String, Optional adresZrodla As String = "") As WynikTileJson
        Using doc = JsonDocument.Parse(jsonTekst)
            Dim root = doc.RootElement
            If root.ValueKind <> JsonValueKind.Object Then Throw New FormatException("dokument TileJSON nie jest obiektem JSON")
            Dim tilesProp As JsonElement
            If Not PobierzWlasciwosc(root, "tiles", tilesProp) OrElse tilesProp.ValueKind <> JsonValueKind.Array OrElse tilesProp.GetArrayLength() = 0 Then
                Throw New FormatException("dokument TileJSON nie zawiera tablicy tiles")
            End If

            Dim surowySzablon As String = ""
            For Each item In tilesProp.EnumerateArray()
                If item.ValueKind = JsonValueKind.String AndAlso Not String.IsNullOrWhiteSpace(item.GetString()) Then
                    surowySzablon = item.GetString().Trim()
                    Exit For
                End If
            Next
            If surowySzablon = "" Then Throw New FormatException("dokument TileJSON nie zawiera poprawnego adresu w polu tiles")

            Dim wynik As New WynikTileJson With {
                .Nazwa = TekstJson(root, "name"),
                .Opis = TekstJson(root, "description")
            }
            Dim subdomeny As New List(Of String)
            Dim tmsZAdresu As Boolean = False
            wynik.Szablon = NormalizujSzablon(surowySzablon, subdomeny, tmsZAdresu)
            wynik.Opcje.Subdomeny.AddRange(subdomeny)

            Dim scheme As String = TekstJson(root, "scheme")
            Dim jestTms As Boolean = tmsZAdresu OrElse scheme.Equals("tms", StringComparison.OrdinalIgnoreCase)
            wynik.Opcje.Typ = If(jestTms, TypUslugiMapowej.Tms, TypUslugiMapowej.Xyz)

            Dim zMin As Integer = CalkowitaJson(root, "minzoom", 0)
            Dim zMax As Integer = CalkowitaJson(root, "maxzoom", 18)
            wynik.Opcje.ZMin = Math.Max(0, Math.Min(22, zMin))
            wynik.Opcje.ZMax = Math.Max(wynik.Opcje.ZMin, Math.Min(22, zMax))
            wynik.Opcje.Atrybucja = Regex.Replace(TekstJson(root, "attribution"), "<[^>]+>", "").Trim()

            Dim boundsEl As JsonElement
            If PobierzWlasciwosc(root, "bounds", boundsEl) AndAlso boundsEl.ValueKind = JsonValueKind.Array AndAlso boundsEl.GetArrayLength() = 4 Then
                Dim vals = boundsEl.EnumerateArray().ToArray()
                Dim zach As Double, polud As Double, wsch As Double, poln As Double
                If vals(0).TryGetDouble(zach) AndAlso vals(1).TryGetDouble(polud) AndAlso
                   vals(2).TryGetDouble(wsch) AndAlso vals(3).TryGetDouble(poln) Then
                    Dim g As New ProstokatGeo(poln, polud, wsch, zach)
                    If g.Poprawny Then wynik.Opcje.ZasiegGeo = g
                End If
            End If
            Return wynik
        End Using
    End Function

    ''' <summary>Odczyt metadanych z dokumentu OSGeo TMS (tilemapresource.xml).</summary>
    Public Shared Function ParsujTmsXml(xmlTekst As String, Optional adresXml As String = "") As WynikTmsXml
        Dim doc = XDocument.Parse(xmlTekst)
        Dim root = doc.Root
        If root Is Nothing OrElse Not root.Name.LocalName.Equals("TileMap", StringComparison.OrdinalIgnoreCase) Then
            Throw New FormatException("dokument XML nie jest opisem OSGeo TMS (TileMap)")
        End If

        Dim srs As String = TekstXml(root, "SRS")
        Dim uklad = UkladKafliWmts.ZOznaczenia(srs)
        Dim czy41001 As Boolean = srs.IndexOf("41001", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                  srs.IndexOf("900913", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                  srs.IndexOf("3857", StringComparison.OrdinalIgnoreCase) >= 0
        If Not czy41001 AndAlso (uklad Is Nothing OrElse Not uklad.JestWebMercator) Then
            Throw New FormatException("usługa TMS używa układu '" & srs & "', a MapoTero obsługuje dla TMS wyłącznie Web Mercator (EPSG:3857)")
        End If

        Dim wynik As New WynikTmsXml With {
            .Tytul = TekstXml(root, "Title"),
            .Opis = TekstXml(root, "Abstract"),
            .Srs = srs
        }

        Dim tileFormat = root.Elements().FirstOrDefault(Function(x) x.Name.LocalName = "TileFormat")
        Dim ext As String = "png"
        Dim rozmiarKafla As Integer = 256
        If tileFormat IsNot Nothing Then
            Dim atrExt = tileFormat.Attribute("extension")?.Value
            If Not String.IsNullOrWhiteSpace(atrExt) Then ext = atrExt.Trim().TrimStart("."c)
            Dim wStr = tileFormat.Attribute("width")?.Value
            Dim wVal As Integer
            If Integer.TryParse(wStr, NumberStyles.Integer, CultureInfo.InvariantCulture, wVal) AndAlso wVal >= 64 Then
                rozmiarKafla = wVal
            End If
        End If
        wynik.Opcje.RozmiarKafla = rozmiarKafla

        Dim tileSets = root.Elements().FirstOrDefault(Function(x) x.Name.LocalName = "TileSets")
        Dim zList As New List(Of Integer)
        Dim bazowyHref As String = ""
        If tileSets IsNot Nothing Then
            For Each ts In tileSets.Elements().Where(Function(x) x.Name.LocalName = "TileSet")
                Dim ordStr = ts.Attribute("order")?.Value
                Dim ord As Integer
                If Integer.TryParse(ordStr, NumberStyles.Integer, CultureInfo.InvariantCulture, ord) Then
                    zList.Add(ord)
                End If
                If bazowyHref = "" Then
                    Dim href = ts.Attribute("href")?.Value
                    If Not String.IsNullOrWhiteSpace(href) Then bazowyHref = href.Trim()
                End If
            Next
        End If
        If zList.Count > 0 Then
            wynik.Opcje.ZMin = Math.Max(0, zList.Min())
            wynik.Opcje.ZMax = Math.Min(22, zList.Max())
        End If

        Dim bazaUrl As String = ""
        Dim tilemapService = root.Attribute("tilemapservice")?.Value
        If bazowyHref <> "" AndAlso Uri.TryCreate(bazowyHref, UriKind.Absolute, Nothing) Then
            'href ma postać .../1.0.0/layer/0 -> obcięcie ostatniego segmentu z numerem poziomu
            Dim idx = bazowyHref.TrimEnd("/"c).LastIndexOf("/"c)
            If idx > 0 Then bazaUrl = bazowyHref.Substring(0, idx)
        ElseIf Not String.IsNullOrWhiteSpace(adresXml) Then
            Dim a = adresXml.Split("?"c)(0)
            Dim idx = a.LastIndexOf("/"c)
            If idx > 0 Then bazaUrl = a.Substring(0, idx)
        ElseIf Not String.IsNullOrWhiteSpace(tilemapService) Then
            bazaUrl = tilemapService.TrimEnd("/"c)
        End If

        If bazaUrl = "" Then Throw New FormatException("nie udało się ustalić bazowego adresu kafli z dokumentu TileMap")
        wynik.Szablon = bazaUrl.TrimEnd("/"c) & "/{z}/{x}/{-y}." & ext

        Dim bbox = root.Elements().FirstOrDefault(Function(x) x.Name.LocalName = "BoundingBox")
        If bbox IsNot Nothing Then
            Dim minx, miny, maxx, maxy As Double
            If Double.TryParse(bbox.Attribute("minx")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, minx) AndAlso
               Double.TryParse(bbox.Attribute("miny")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, miny) AndAlso
               Double.TryParse(bbox.Attribute("maxx")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, maxx) AndAlso
               Double.TryParse(bbox.Attribute("maxy")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, maxy) Then
                'w TMS EPSG:3857 BoundingBox bywa w metrach Mercatora albo w stopniach
                If Math.Abs(minx) <= 180 AndAlso Math.Abs(maxx) <= 180 AndAlso Math.Abs(miny) <= 90 AndAlso Math.Abs(maxy) <= 90 Then
                    Dim g As New ProstokatGeo(maxy, miny, maxx, minx)
                    If g.Poprawny Then wynik.Opcje.ZasiegGeo = g
                Else
                    Dim uk3857 = UkladKafliWmts.ZOznaczenia("EPSG:3857")
                    Dim ld = uk3857.DoWgs84(New PunktXY(miny, minx))
                    Dim pg = uk3857.DoWgs84(New PunktXY(maxy, maxx))
                    Dim g As New ProstokatGeo(pg.Szerokosc, ld.Szerokosc, pg.Dlugosc, ld.Dlugosc)
                    If g.Poprawny Then wynik.Opcje.ZasiegGeo = g
                End If
            End If
        End If

        Return wynik
    End Function

    Private Shared Function PobierzWlasciwosc(obj As JsonElement, nazwa As String, ByRef wynik As JsonElement) As Boolean
        For Each prop In obj.EnumerateObject()
            If prop.Name.Equals(nazwa, StringComparison.OrdinalIgnoreCase) Then
                wynik = prop.Value
                Return True
            End If
        Next
        wynik = Nothing
        Return False
    End Function

    Private Shared Function TekstJson(obj As JsonElement, nazwa As String) As String
        Dim el As JsonElement
        If PobierzWlasciwosc(obj, nazwa, el) AndAlso el.ValueKind = JsonValueKind.String Then
            Return If(el.GetString(), "").Trim()
        End If
        Return ""
    End Function

    Private Shared Function CalkowitaJson(obj As JsonElement, nazwa As String, domyslna As Integer) As Integer
        Dim el As JsonElement
        Dim w As Integer
        If PobierzWlasciwosc(obj, nazwa, el) AndAlso el.ValueKind = JsonValueKind.Number AndAlso el.TryGetInt32(w) Then
            Return w
        End If
        Return domyslna
    End Function

    Private Shared Function TekstXml(rodzic As XElement, nazwa As String) As String
        Dim d = rodzic.Elements().FirstOrDefault(Function(x) x.Name.LocalName = nazwa)
        Return If(d Is Nothing, "", d.Value.Trim())
    End Function

    ''' <summary>
    ''' Sprawdza odpowiedź HTTP dla pojedynczego kafla (PNG/JPEG/GIF/TIFF vs WebP, MVT/PBF, HTML, XML/JSON).
    ''' Zwraca pusty tekst dla poprawnego obrazu rastrowego oraz uzupełnia jego wymiary w pikselach.
    ''' </summary>
    Public Shared Function DiagnozujOdpowiedzKafla(bajty As Byte(), contentType As String,
                                                   Optional ByRef szerokoscPx As Integer = 0,
                                                   Optional ByRef wysokoscPx As Integer = 0,
                                                   Optional adres As String = "") As String
        szerokoscPx = 0
        wysokoscPx = 0
        If bajty Is Nothing OrElse bajty.Length = 0 Then Return "Serwer zwrócił pusty kafel."

        Dim ct As String = If(contentType, "").Trim().ToLowerInvariant()
        Dim sciezka As String = If(adres, "").Split("?"c)(0).ToLowerInvariant()
        If ct.Contains("protobuf") OrElse ct.Contains("mapbox-vector-tile") OrElse ct.Contains("/mvt") OrElse
           sciezka.EndsWith(".mvt", StringComparison.Ordinal) OrElse sciezka.EndsWith(".pbf", StringComparison.Ordinal) Then
            Return "Serwer zwraca wektorowe kafle MVT/PBF, których MapoTero nie obsługuje (wymagane są kafle rastrowe PNG lub JPEG)."
        End If

        'sygnatura RIFF....WEBP
        If ct.Contains("image/webp") OrElse
           (bajty.Length >= 12 AndAlso
            bajty(0) = &H52 AndAlso bajty(1) = &H49 AndAlso bajty(2) = &H46 AndAlso bajty(3) = &H46 AndAlso
            bajty(8) = &H57 AndAlso bajty(9) = &H45 AndAlso bajty(10) = &H42 AndAlso bajty(11) = &H50) Then
            Return "Serwer zwraca kafle w formacie WebP, który nie jest obsługiwany. Jeśli serwer obsługuje PNG lub JPEG, zmień rozszerzenie w szablonie."
        End If

        'odczyt nagłówka rastrowego (PNG, JPEG, GIF, TIFF) bez dekodowania całego obrazu
        Try
            Using ms As New IO.MemoryStream(bajty)
                Dim nag = NaglowekObrazu.Odczytaj(ms)
                If nag.SzerokoscPx > 0 AndAlso nag.WysokoscPx > 0 Then
                    szerokoscPx = CInt(nag.SzerokoscPx)
                    wysokoscPx = CInt(nag.WysokoscPx)
                    Return ""
                End If
            End Using
        Catch
            'odpowiedź nie jest poprawnym rastrem - poniżej rozpoznanie komunikatu błędu
        End Try

        Dim probka As String = System.Text.Encoding.UTF8.GetString(bajty, 0, Math.Min(bajty.Length, 200)).TrimStart()
        If ct.Contains("text/html") OrElse
           probka.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) OrElse
           probka.StartsWith("<html", StringComparison.OrdinalIgnoreCase) Then
            Return "Serwer zwrócił stronę HTML zamiast obrazu kafla."
        End If
        If probka.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) OrElse
           probka.StartsWith("<Exception", StringComparison.OrdinalIgnoreCase) OrElse
           probka.StartsWith("<ServiceException", StringComparison.OrdinalIgnoreCase) OrElse
           probka.StartsWith("{", StringComparison.Ordinal) OrElse
           ct.Contains("json") OrElse ct.Contains("xml") Then
            Return "Serwer zwrócił komunikat tekstowy zamiast obrazu kafla."
        End If

        Return "Serwer zwrócił nieobsługiwany format kafla" & If(ct <> "", " (" & ct & ")", "") & " zamiast PNG lub JPEG."
    End Function

    ''' <summary>Rozpoznaje typ MIME obrazu rastrowego z nagłówka pliku (PNG, JPEG, GIF, TIFF) albo nagłówka HTTP.</summary>
    Public Shared Function WykryjMimeZObrazu(bajty As Byte(), Optional contentType As String = "") As String
        If bajty IsNot Nothing Then
            If bajty.Length >= 8 AndAlso bajty(0) = &H89 AndAlso bajty(1) = AscW("P"c) AndAlso bajty(2) = AscW("N"c) AndAlso bajty(3) = AscW("G"c) Then
                Return "image/png"
            End If
            If bajty.Length >= 3 AndAlso bajty(0) = &HFF AndAlso bajty(1) = &HD8 Then
                Return "image/jpeg"
            End If
            If bajty.Length >= 6 AndAlso bajty(0) = AscW("G"c) AndAlso bajty(1) = AscW("I"c) AndAlso bajty(2) = AscW("F"c) Then
                Return "image/gif"
            End If
            If bajty.Length >= 4 AndAlso ((bajty(0) = AscW("I"c) AndAlso bajty(1) = AscW("I"c)) OrElse (bajty(0) = AscW("M"c) AndAlso bajty(1) = AscW("M"c))) Then
                Return "image/tiff"
            End If
        End If
        Dim ct = If(contentType, "").Split(";"c)(0).Trim().ToLowerInvariant()
        If ct.StartsWith("image/", StringComparison.Ordinal) Then Return ct
        Return ""
    End Function

    ''' <summary>
    ''' Wyznacza sugerowany rozmiar piksela [m/pix] dla warstwy kafelkowej (WMTS/XYZ/TMS) na podstawie jej nazwy
    ''' i rzeczywistej rozdzielczości macierzy (dla Web Mercator przeliczonej z równika na szerokość Polski 52° N).
    ''' </summary>
    Public Shared Function SugerowanyRozmiarPiksela(usluga As UslugaWmts, w As WarstwaWmts) As Double
        If w Is Nothing Then Return 1.0

        Dim tekst = (w.Identyfikator & " " & w.Tytul).ToLowerInvariant()
        If tekst.Contains("bdoo") OrElse tekst.Contains("ogolnogeograf") OrElse tekst.Contains("ogólnogeograf") Then
            Return 25.0
        End If
        If tekst.Contains("highresolution") OrElse tekst.Contains("ortofotomapa_high") Then
            Return 0.1
        End If
        If tekst.Contains("orto") OrElse tekst.Contains("ortho") OrElse tekst.Contains("aerial") OrElse
           tekst.Contains("dzialk") OrElse tekst.Contains("ewid") OrElse tekst.Contains("katastr") Then
            Return 0.25
        End If
        If tekst.Contains("bdot10k") OrElse tekst.Contains("10k") OrElse tekst.Contains("topograf") OrElse
           tekst.Contains("cieniowan") OrElse tekst.Contains("nmt") OrElse tekst.Contains("isok") Then
            Return 1.0
        End If
        If tekst.Contains("skan") OrElse tekst.Contains("wig") OrElse tekst.Contains("messtischblatt") Then
            Return 2.0
        End If
        If tekst.Contains("geolog") OrElse tekst.Contains("sozo") OrElse tekst.Contains("hydro") Then
            Return 5.0
        End If

        If usluga IsNot Nothing Then
            For Each p In w.Powiazania
                Dim z = usluga.Zestaw(p.Zestaw)
                If z IsNot Nothing AndAlso z.Macierze.Count > 0 Then
                    Dim uk = z.Uklad
                    Dim mnj As Double = If(uk IsNot Nothing, uk.MetrowNaJednostke, 1.0)
                    'dla Web Mercator skala z mianownika odpowiada równikowi - na 52° N piksel terenowy jest ok. 0,616x mniejszy
                    Dim cos52 As Double = If(uk IsNot Nothing AndAlso uk.JestWebMercator, Math.Cos(52.0 * Math.PI / 180.0), 1.0)
                    Dim rozdz = z.Macierze.Select(Function(m) m.Rozdzielczosc(mnj) * cos52).OrderBy(Function(r) r).ToList()
                    If rozdz.Any(Function(r) r >= 0.5 AndAlso r <= 2.5) Then Return 1.0
                    If rozdz.First() > 2.5 Then
                        Dim minR = rozdz.First()
                        If minR >= 20.0 AndAlso minR <= 35.0 Then Return 25.0
                        Return Math.Round(minR, 1)
                    End If
                End If
            Next
        End If

        Return 1.0
    End Function

    ''' <summary>Krótki opis techniczny warstwy do dymka podpowiedzi w drzewie map.</summary>
    Public Shared Function OpisWarstwy(typ As TypUslugiMapowej, opcje As OpcjeKafliXyz) As String
        Dim czesci As New List(Of String)
        If typ = TypUslugiMapowej.Xyz OrElse typ = TypUslugiMapowej.Tms Then
            Dim z0 As Integer = If(opcje IsNot Nothing, opcje.ZMin, 0)
            Dim z1 As Integer = If(opcje IsNot Nothing, opcje.ZMax, 18)
            Dim rola As String = If(opcje IsNot Nothing AndAlso opcje.Nakladka, "przezroczysta nakładka", "podkład")
            czesci.Add("Typ: " & NazwaTypu(typ) & ", zoom " & z0 & ".." & z1 & ", " & rola)
            If opcje IsNot Nothing AndAlso opcje.RozmiarKafla <> 256 Then
                czesci.Add("kafel " & opcje.RozmiarKafla & " px")
            End If
        ElseIf typ = TypUslugiMapowej.Wmts Then
            czesci.Add("Typ: WMTS")
        End If
        If opcje IsNot Nothing AndAlso opcje.IgnorujSsl Then
            czesci.Add("ignoruje błędy SSL")
        End If
        Return String.Join(", ", czesci)
    End Function

End Class
