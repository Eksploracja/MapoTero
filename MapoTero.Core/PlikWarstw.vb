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
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json

''' <summary>Warstwa mapy z pliku zbioru map: nazwa warstwy na serwerze, opcjonalny czytelny tytuł, zalecany rozmiar piksela i opcjonalne nadpisania XYZ/TMS.</summary>
Public Class WarstwaMapy
    Public Property Nazwa As String = ""
    ''' <summary>Opcjonalna czytelna nazwa wyświetlana warstwy (np. z dokumentu GetCapabilities dla dodanych serwerów).</summary>
    Public Property Tytul As String = ""
    ''' <summary>Rozmiar piksela zapisany w pliku (tekst, np. "2" lub "0.25"); może być pusty.</summary>
    Public Property RozmiarPiksela As String = ""
    Public Property ZMin As Integer?
    Public Property ZMax As Integer?
    Public Property Nakladka As Boolean?

    Public Overrides Function ToString() As String
        Return If(Not String.IsNullOrWhiteSpace(Tytul), Tytul, Nazwa)
    End Function
End Class

''' <summary>
''' Plik zbioru map z katalogu "warstwy": obsługuje zarówno klasyczny format tekstowy (Windows-1250: adres serwera,
''' dalej pary nazwa warstwy, rozmiar piksela), jak i format JSON (UTF-8, z jawnymi metadanymi XYZ/TMS oraz czytelnymi tytułami).
''' </summary>
Public Class PlikWarstw

    Public Property Adres As String = ""
    ''' <summary>Opcjonalna czytelna nazwa zbioru (np. tytuł usługi z GetCapabilities).</summary>
    Public Property Tytul As String = ""
    Public ReadOnly Property Warstwy As New List(Of WarstwaMapy)
    Public Property Typ As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    Public ReadOnly Property Subdomeny As New List(Of String)
    Public Property ZMin As Integer = 0
    Public Property ZMax As Integer = 18
    Public Property RozmiarKafla As Integer = 256
    Public Property Nakladka As Boolean
    Public Property OdstepMs As Integer = 0
    Public Property MaksWatkow As Integer = 0
    Public Property Referer As String = ""
    Public Property Atrybucja As String = ""
    Public Property FormatObrazu As String = ""
    Public Property IgnorujSsl As Boolean
    Public Property ZasiegGeo As ProstokatGeo?

    ''' <summary>Faktyczny typ usługi: jawny z pliku albo rozpoznany z adresu (XYZ/TMS przed WMTS).</summary>
    Public ReadOnly Property EfektywnyTyp As TypUslugiMapowej
        Get
            Return KafleXyz.WybierzTyp(Adres, Typ)
        End Get
    End Property

    ''' <summary>Parametry kafli XYZ/TMS dla całego zbioru lub wskazanej warstwy (z uwzględnieniem nadpisań warstwy).</summary>
    Public Function OpcjeKafli(Optional nazwaWarstwy As String = Nothing) As OpcjeKafliXyz
        Dim t As TypUslugiMapowej = EfektywnyTyp
        Dim opcje As New OpcjeKafliXyz With {
            .Typ = If(t = TypUslugiMapowej.Tms, TypUslugiMapowej.Tms, TypUslugiMapowej.Xyz),
            .ZMin = ZMin,
            .ZMax = ZMax,
            .RozmiarKafla = RozmiarKafla,
            .Nakladka = Nakladka,
            .OdstepMs = If(OdstepMs > 0, OdstepMs, If(t = TypUslugiMapowej.Xyz OrElse t = TypUslugiMapowej.Tms, 100, 0)),
            .MaksWatkow = If(MaksWatkow > 0, MaksWatkow, If(t = TypUslugiMapowej.Xyz OrElse t = TypUslugiMapowej.Tms, 2, 0)),
            .Referer = Referer,
            .Atrybucja = Atrybucja,
            .FormatObrazu = FormatObrazu,
            .IgnorujSsl = IgnorujSsl,
            .ZasiegGeo = ZasiegGeo}
        opcje.Subdomeny.AddRange(Subdomeny)
        If Not String.IsNullOrEmpty(nazwaWarstwy) Then
            Dim wm = Znajdz(nazwaWarstwy)
            If wm IsNot Nothing Then
                If wm.ZMin.HasValue Then opcje.ZMin = wm.ZMin.Value
                If wm.ZMax.HasValue Then opcje.ZMax = wm.ZMax.Value
                If wm.Nakladka.HasValue Then opcje.Nakladka = wm.Nakladka.Value
            End If
        End If
        Return opcje
    End Function

    ''' <summary>Pliki warstw w klasycznym formacie są zapisane w kodowaniu Windows-1250 (polskie znaki w nazwach warstw).</summary>
    Public Shared Function Kodowanie() As Encoding
        Try
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            Return Encoding.GetEncoding(1250)
        Catch ex As Exception
            Return Encoding.UTF8
        End Try
    End Function

    Public Shared Function Wczytaj(sciezka As String) As PlikWarstw
        Dim bajty = File.ReadAllBytes(sciezka)
        If CzyWygladaJakJson(bajty) Then
            Return ZTekstu(Encoding.UTF8.GetString(PominBomUtf8(bajty)))
        End If
        Return ZTekstu(Kodowanie().GetString(bajty))
    End Function

    Private Shared Function PominBomUtf8(bajty() As Byte) As Byte()
        If bajty.Length >= 3 AndAlso bajty(0) = &HEF AndAlso bajty(1) = &HBB AndAlso bajty(2) = &HBF Then
            Dim wynik(bajty.Length - 4) As Byte
            Array.Copy(bajty, 3, wynik, 0, wynik.Length)
            Return wynik
        End If
        Return bajty
    End Function

    Private Shared Function CzyWygladaJakJson(bajty() As Byte) As Boolean
        Dim start As Integer = If(bajty.Length >= 3 AndAlso bajty(0) = &HEF AndAlso bajty(1) = &HBB AndAlso bajty(2) = &HBF, 3, 0)
        For i = start To bajty.Length - 1
            Dim b = bajty(i)
            If b = 32 OrElse b = 9 OrElse b = 10 OrElse b = 13 Then Continue For
            Return b = AscW("{"c)
        Next
        Return False
    End Function

    Public Shared Function ZTekstu(tekst As String) As PlikWarstw
        Dim przyciety As String = If(tekst, "").TrimStart(ChrW(&HFEFF), " "c, ChrW(9), ChrW(10), ChrW(13))
        If przyciety.StartsWith("{", StringComparison.Ordinal) Then
            Return ZJsona(przyciety)
        End If

        Dim p As New PlikWarstw()
        Dim r As New CzytnikPlikuVb(tekst)
        If r.KoniecPliku Then Return p
        p.Adres = r.Nastepne()
        While Not r.KoniecPliku
            Dim w As New WarstwaMapy With {.Nazwa = r.Nastepne()}
            If Not r.KoniecPliku Then w.RozmiarPiksela = r.Nastepne()
            If w.Nazwa <> "" Then p.Warstwy.Add(w)
        End While
        Return p
    End Function

    Private Shared Function ZJsona(json As String) As PlikWarstw
        Dim p As New PlikWarstw()
        Using doc = JsonDocument.Parse(json)
            Dim root = doc.RootElement
            If root.ValueKind <> JsonValueKind.Object Then Throw New FormatException("Plik zbioru JSON musi być obiektem")
            p.Adres = TekstJson(root, "adres")
            p.Tytul = TekstJson(root, "tytul")
            p.Typ = ParsujTyp(TekstJson(root, "typ"))
            p.ZMin = CalkowitaJson(root, "zMin", 0)
            p.ZMax = CalkowitaJson(root, "zMax", 18)
            p.RozmiarKafla = Math.Max(64, CalkowitaJson(root, "rozmiarKafla", 256))
            p.Nakladka = LogicznaJson(root, "nakladka", False)
            p.OdstepMs = Math.Max(0, CalkowitaJson(root, "odstepMs", 0))
            p.MaksWatkow = Math.Max(0, CalkowitaJson(root, "maksWatkow", 0))
            p.Referer = TekstJson(root, "referer")
            p.Atrybucja = TekstJson(root, "atrybucja")
            p.FormatObrazu = TekstJson(root, "format")
            p.IgnorujSsl = LogicznaJson(root, "ignorujSsl", False)

            Dim subEl As JsonElement
            If PobierzWlasciwosc(root, "subdomeny", subEl) AndAlso subEl.ValueKind = JsonValueKind.Array Then
                For Each s In subEl.EnumerateArray()
                    If s.ValueKind = JsonValueKind.String Then
                        Dim val = If(s.GetString(), "").Trim()
                        If val <> "" Then p.Subdomeny.Add(val)
                    End If
                Next
            End If

            Dim zasEl As JsonElement
            If PobierzWlasciwosc(root, "zasieg", zasEl) AndAlso zasEl.ValueKind = JsonValueKind.Array AndAlso zasEl.GetArrayLength() = 4 Then
                Dim vals = zasEl.EnumerateArray().ToArray()
                Dim zach, polud, wsch, poln As Double
                If vals(0).TryGetDouble(zach) AndAlso vals(1).TryGetDouble(polud) AndAlso
                   vals(2).TryGetDouble(wsch) AndAlso vals(3).TryGetDouble(poln) Then
                    Dim g As New ProstokatGeo(poln, polud, wsch, zach)
                    If g.Poprawny Then p.ZasiegGeo = g
                End If
            End If

            Dim wEl As JsonElement
            If PobierzWlasciwosc(root, "warstwy", wEl) AndAlso wEl.ValueKind = JsonValueKind.Array Then
                For Each item In wEl.EnumerateArray()
                    If item.ValueKind <> JsonValueKind.Object Then Continue For
                    Dim nazwa = TekstJson(item, "nazwa")
                    If nazwa = "" Then Continue For
                    Dim wm As New WarstwaMapy With {
                        .Nazwa = nazwa,
                        .Tytul = TekstJson(item, "tytul"),
                        .RozmiarPiksela = TekstLubLiczbaJson(item, "piksel")
                    }
                    Dim zMinEl As JsonElement
                    If PobierzWlasciwosc(item, "zMin", zMinEl) AndAlso zMinEl.ValueKind = JsonValueKind.Number Then
                        Dim v As Integer
                        If zMinEl.TryGetInt32(v) Then wm.ZMin = v
                    End If
                    Dim zMaxEl As JsonElement
                    If PobierzWlasciwosc(item, "zMax", zMaxEl) AndAlso zMaxEl.ValueKind = JsonValueKind.Number Then
                        Dim v As Integer
                        If zMaxEl.TryGetInt32(v) Then wm.ZMax = v
                    End If
                    Dim nakEl As JsonElement
                    If PobierzWlasciwosc(item, "nakladka", nakEl) AndAlso (nakEl.ValueKind = JsonValueKind.True OrElse nakEl.ValueKind = JsonValueKind.False) Then
                        wm.Nakladka = nakEl.GetBoolean()
                    End If
                    p.Warstwy.Add(wm)
                Next
            End If
        End Using
        Return p
    End Function

    Private Shared Function ParsujTyp(tekst As String) As TypUslugiMapowej
        Select Case If(tekst, "").Trim().ToLowerInvariant()
            Case "wms" : Return TypUslugiMapowej.Wms
            Case "wmts" : Return TypUslugiMapowej.Wmts
            Case "xyz" : Return TypUslugiMapowej.Xyz
            Case "tms" : Return TypUslugiMapowej.Tms
            Case Else : Return TypUslugiMapowej.Nieznana
        End Select
    End Function

    Private Shared Function TekstTypu(t As TypUslugiMapowej) As String
        Select Case t
            Case TypUslugiMapowej.Wms : Return "wms"
            Case TypUslugiMapowej.Wmts : Return "wmts"
            Case TypUslugiMapowej.Xyz : Return "xyz"
            Case TypUslugiMapowej.Tms : Return "tms"
            Case Else : Return ""
        End Select
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

    Private Shared Function TekstLubLiczbaJson(obj As JsonElement, nazwa As String) As String
        Dim el As JsonElement
        If Not PobierzWlasciwosc(obj, nazwa, el) Then Return ""
        If el.ValueKind = JsonValueKind.String Then Return If(el.GetString(), "").Trim()
        If el.ValueKind = JsonValueKind.Number Then Return el.GetRawText()
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

    Private Shared Function LogicznaJson(obj As JsonElement, nazwa As String, domyslna As Boolean) As Boolean
        Dim el As JsonElement
        If PobierzWlasciwosc(obj, nazwa, el) Then
            If el.ValueKind = JsonValueKind.True Then Return True
            If el.ValueKind = JsonValueKind.False Then Return False
        End If
        Return domyslna
    End Function

    ''' <summary>Warstwa o podanej nazwie albo Nothing.</summary>
    Public Function Znajdz(nazwa As String) As WarstwaMapy
        For Each w In Warstwy
            If String.Equals(w.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase) Then Return w
        Next
        Return Nothing
    End Function

    ''' <summary>Czy plik wymaga zapisu w formacie JSON (usługa XYZ/TMS, dodatkowe metadane lub czytelne tytuły).</summary>
    Private ReadOnly Property WymagaJson As Boolean
        Get
            Dim t = EfektywnyTyp
            Return t = TypUslugiMapowej.Xyz OrElse t = TypUslugiMapowej.Tms OrElse
                   Not String.IsNullOrWhiteSpace(Tytul) OrElse
                   Subdomeny.Count > 0 OrElse Nakladka OrElse OdstepMs > 0 OrElse MaksWatkow > 0 OrElse
                   RozmiarKafla <> 256 OrElse Referer <> "" OrElse Atrybucja <> "" OrElse
                   FormatObrazu <> "" OrElse IgnorujSsl OrElse ZasiegGeo.HasValue OrElse
                   Warstwy.Any(Function(w) w.ZMin.HasValue OrElse w.ZMax.HasValue OrElse w.Nakladka.HasValue OrElse
                                           (Not String.IsNullOrWhiteSpace(w.Tytul) AndAlso Not String.Equals(w.Tytul, w.Nazwa, StringComparison.Ordinal)))
        End Get
    End Property

    ''' <summary>Zapisuje zbiór map do pliku tekstowego (JSON UTF-8 dla XYZ/TMS, klasyczny format dla WMS/WMTS).</summary>
    Public Sub Zapisz(sciezka As String)
        If WymagaJson Then
            ZapiszJson(sciezka)
            Exit Sub
        End If
        Dim sb As New ZapisPlikuVb()
        sb.Tekst(Adres)
        For Each w In Warstwy
            sb.Tekst(w.Nazwa)
            sb.Tekst(If(Not String.IsNullOrEmpty(w.RozmiarPiksela), w.RozmiarPiksela, "1"))
        Next
        sb.Zapisz(sciezka, Kodowanie())
    End Sub

    Private Sub ZapiszJson(sciezka As String)
        Using ms As New MemoryStream()
            Using w As New Utf8JsonWriter(ms, New JsonWriterOptions With {.Indented = True, .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping})
                w.WriteStartObject()
                Dim t = EfektywnyTyp
                Dim tStr = TekstTypu(t)
                If tStr <> "" Then w.WriteString("typ", tStr)
                If Not String.IsNullOrWhiteSpace(Tytul) Then w.WriteString("tytul", Tytul.Trim())
                w.WriteString("adres", Adres)
                If Subdomeny.Count > 0 Then
                    w.WriteStartArray("subdomeny")
                    For Each s In Subdomeny
                        w.WriteStringValue(s)
                    Next
                    w.WriteEndArray()
                End If
                If t = TypUslugiMapowej.Xyz OrElse t = TypUslugiMapowej.Tms Then
                    w.WriteNumber("zMin", ZMin)
                    w.WriteNumber("zMax", ZMax)
                End If
                If RozmiarKafla <> 256 Then w.WriteNumber("rozmiarKafla", RozmiarKafla)
                If Nakladka Then w.WriteBoolean("nakladka", True)
                If MaksWatkow > 0 Then w.WriteNumber("maksWatkow", MaksWatkow)
                If OdstepMs > 0 Then w.WriteNumber("odstepMs", OdstepMs)
                If Referer <> "" Then w.WriteString("referer", Referer)
                If Atrybucja <> "" Then w.WriteString("atrybucja", Atrybucja)
                If FormatObrazu <> "" Then w.WriteString("format", FormatObrazu)
                If IgnorujSsl Then w.WriteBoolean("ignorujSsl", True)
                If ZasiegGeo.HasValue AndAlso ZasiegGeo.Value.Poprawny Then
                    Dim g = ZasiegGeo.Value
                    w.WriteStartArray("zasieg")
                    w.WriteNumberValue(g.Zachod)
                    w.WriteNumberValue(g.Poludnie)
                    w.WriteNumberValue(g.Wschod)
                    w.WriteNumberValue(g.Polnoc)
                    w.WriteEndArray()
                End If
                w.WriteStartArray("warstwy")
                For Each war In Warstwy
                    w.WriteStartObject()
                    w.WriteString("nazwa", war.Nazwa)
                    If Not String.IsNullOrWhiteSpace(war.Tytul) AndAlso Not String.Equals(war.Tytul, war.Nazwa, StringComparison.Ordinal) Then
                        w.WriteString("tytul", war.Tytul.Trim())
                    End If
                    w.WriteString("piksel", If(Not String.IsNullOrEmpty(war.RozmiarPiksela), war.RozmiarPiksela, "1"))
                    If war.ZMin.HasValue Then w.WriteNumber("zMin", war.ZMin.Value)
                    If war.ZMax.HasValue Then w.WriteNumber("zMax", war.ZMax.Value)
                    If war.Nakladka.HasValue Then w.WriteBoolean("nakladka", war.Nakladka.Value)
                    w.WriteEndObject()
                Next
                w.WriteEndArray()
                w.WriteEndObject()
            End Using
            File.WriteAllBytes(sciezka, ms.ToArray())
        End Using
    End Sub

    ''' <summary>Sprawdza, czy nazwa zbioru map jest poprawna dla systemu plików i nie koliduje z nazwami zarezerwowanymi.</summary>
    Public Shared Function CzyNazwaZbioruPoprawna(nazwa As String, ByRef komunikatBledu As String) As Boolean
        komunikatBledu = ""
        If String.IsNullOrWhiteSpace(nazwa) Then
            komunikatBledu = "Nazwa zbioru map nie może być pusta."
            Return False
        End If
        Dim czysta = nazwa.Trim()
        For Each c In Path.GetInvalidFileNameChars()
            If czysta.Contains(c) Then
                komunikatBledu = "Nazwa zawiera niedozwolony znak: '" & c & "'."
                Return False
            End If
        Next
        Dim male = czysta.ToLowerInvariant()
        If male = "warstwy" OrElse male = "_archiwalne" OrElse male = "download" OrElse male = "conf" Then
            komunikatBledu = "Nazwa '" & czysta & "' jest zarezerwowana dla plików programu."
            Return False
        End If
        Dim zarezerwowaneDos As String() = {"con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6",
                                            "com7", "com8", "com9", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"}
        If Array.IndexOf(zarezerwowaneDos, male) >= 0 Then
            komunikatBledu = "Nazwa '" & czysta & "' jest nazwą zarezerwowaną systemu Windows."
            Return False
        End If
        Return True
    End Function

    ''' <summary>Tworzy bezpieczną nazwę pliku z tytułu usługi lub warstwy (usuwa znaki specjalne, zamienia polskie litery i spacje na podkreślenia).</summary>
    Public Shared Function OczyscNazweZbioru(sugerowana As String) As String
        If String.IsNullOrWhiteSpace(sugerowana) Then Return "nowy_zbior_map"
        Dim s As String = sugerowana.Trim()
        'zamiana polskich znaków
        Dim plZrodlo As String = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ"
        Dim plCel As String = "acelnoszzACELNOSZZ"
        For i = 0 To plZrodlo.Length - 1
            s = s.Replace(plZrodlo(i), plCel(i))
        Next
        Dim sb As New StringBuilder()
        For Each c In s
            If (c >= "a"c AndAlso c <= "z"c) OrElse (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "0"c AndAlso c <= "9"c) OrElse c = "_"c Then
                sb.Append(c)
            Else
                sb.Append("_"c)
            End If
        Next
        s = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "_+", "_").Trim("_"c)
        If s.Length > 60 Then s = s.Substring(0, 60).TrimEnd("_"c)
        If s = "" Then s = "nowy_zbior_map"
        Return s
    End Function

    ''' <summary>Tworzy poprawny adres zapytania GetMap dla WMS z parametrami CRS/SRS i layers=.</summary>
    Public Shared Function ZbudujAdresWmsDlaPliku(adresGetMap As String, wersja As String) As String
        Dim a As String = Wms.UzupelnijAdres(adresGetMap.Trim())
        Dim czyWersja11 As Boolean = wersja.StartsWith("1.1", StringComparison.Ordinal)
        Dim ukladParam As String = If(czyWersja11, "SRS=EPSG:2180", "CRS=EPSG:2180")
        Dim verParam As String = If(czyWersja11, "version=1.1.1", "version=1.3.0")

        'inny parametr REQUEST (np. GetCapabilities z adresu zgłoszenia) - serwer odczytałby go zamiast dopisanego GetMap
        a = System.Text.RegularExpressions.Regex.Replace(a, "(?<=[?&])request=(?!getmap(&|$))[^&]*", "request=GetMap",
                                                         System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If a.IndexOf("request=getmap", StringComparison.OrdinalIgnoreCase) < 0 Then
            Dim sep As String = If(a.Contains("?"), "&", "?")
            If a.EndsWith("?", StringComparison.Ordinal) OrElse a.EndsWith("&", StringComparison.Ordinal) Then sep = ""
            a &= sep & "request=GetMap"
        End If

        If a.IndexOf("version=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&" & verParam
        End If

        If a.IndexOf("crs=", StringComparison.OrdinalIgnoreCase) < 0 AndAlso a.IndexOf("srs=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&" & ukladParam
        End If

        If a.IndexOf("layers=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&layers="
        End If

        Return a
    End Function

    ''' <summary>Dopisuje nazwę zbioru do pliku indeksu warstwy.txt, jeśli jeszcze w nim nie istnieje.</summary>
    Public Shared Sub DodajDoIndeksu(plikIndeksu As String, nazwaZbioru As String)
        Dim linie As New List(Of String)
        If File.Exists(plikIndeksu) Then
            linie.AddRange(File.ReadAllLines(plikIndeksu, Kodowanie()).Select(Function(l) l.Trim()).Where(Function(l) l <> ""))
        End If
        If Not linie.Any(Function(l) String.Equals(l, nazwaZbioru, StringComparison.OrdinalIgnoreCase)) Then
            linie.Add(nazwaZbioru)
            File.WriteAllLines(plikIndeksu, linie, Kodowanie())
        End If
    End Sub

End Class
