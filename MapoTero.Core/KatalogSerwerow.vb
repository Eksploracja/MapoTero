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
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Źródło pochodzenia pozycji w katalogu serwerów mapowych.</summary>
Public Enum ZrodloKataloguSerwerow
    Wbudowany = 0
    Geoportal = 1
    EziudpCentralne = 2
    EziudpLokalne = 3
End Enum

''' <summary>Pojedyncza usługa mapowa dostępna do dodania w kreatorze serwerów.</summary>
Public Class WpisKataloguSerwerow
    Public Property Id As String = ""
    Public Property Teryt As String = ""
    Public Property Nazwa As String = ""
    Public Property Dostawca As String = ""
    Public Property Obszar As String = ""
    Public Property Kategoria As String = ""
    Public Property Typ As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    Public Property Adres As String = ""
    Public Property Opis As String = ""
    Public Property SugerowanaNazwaZbioru As String = ""
    Public Property CzySkorowidz As Boolean
    Public Property IgnorujSsl As Boolean
    Public Property Zrodlo As ZrodloKataloguSerwerow = ZrodloKataloguSerwerow.Wbudowany
    ''' <summary>Nazwa zbioru w programie, jeśli adres jest już wczytany w MapoTero (puste = nowa usługa).</summary>
    Public Property JuzDodanyZbior As String = ""

    Public ReadOnly Property CzyJuzDodany As Boolean
        Get
            Return Not String.IsNullOrEmpty(JuzDodanyZbior)
        End Get
    End Property

    Public ReadOnly Property NazwaTypu As String
        Get
            If Typ <> TypUslugiMapowej.Nieznana Then Return KafleXyz.NazwaTypu(Typ)
            Return KafleXyz.NazwaTypu(KafleXyz.WybierzTyp(Adres))
        End Get
    End Property
End Class

''' <summary>Rezultat pobrania listy usług z katalogu online lub pamięci podręcznej.</summary>
Public Class WynikPobraniaKatalogu
    Public Property Sukces As Boolean
    Public Property ZCache As Boolean
    Public Property ZCacheStary As Boolean
    Public Property KomunikatBledu As String = ""
    Public ReadOnly Property Wpisy As New List(Of WpisKataloguSerwerow)
End Class

''' <summary>
''' Obsługa wbudowanej listy predefiniowanych usług mapowych oraz parsery publicznych wykazów
''' (oficjalny wykaz Geoportal.gov.pl oraz Ewidencja Zbiorów i Usług Danych Przestrzennych EZiUDP).
''' </summary>
Public NotInheritable Class KatalogSerwerow

    Public Const NazwaZasobuWbudowanego As String = "MapoTero.Core.serwery_dodatkowe.json"
    Public Const TekstStanuMoznaDodac As String = "Można dodać"

    Private Structure WpisZnanejPoduslugi
        Public ReadOnly Wzorzec As String
        Public ReadOnly Podtytul As String
        Public ReadOnly Opis As String

        Public Sub New(wzorzec As String, podtytul As String, opis As String)
            Me.Wzorzec = wzorzec
            Me.Podtytul = podtytul
            Me.Opis = opis
        End Sub
    End Structure

    Private Shared ReadOnly ParametryOgcDoPomijania As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "service", "request", "version", "crs", "srs", "layers", "format", "styles", "width", "height", "bbox", "transparent"
    }

    Private Shared ReadOnly SegmentyAdresuPomijane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "wms", "wmts", "tms", "xyz", "ows", "wfs", "wmsserver", "wmtsserver", "mapserver", "imageserver",
        "rest", "services", "service", "arcgis", "geoserver", "cgi-bin", "mapserv", "mapserv.exe",
        "service.svc", "get", "wss", "ext", "guest", "pub", "public", "default", "server", "inspire", "pzgik"
    }

    'kolejność ma znaczenie: warianty dłuższe/bardziej szczegółowe (np. StandardResolutionTime) przed krótszymi (StandardResolution)
    Private Shared ReadOnly ZnanePoduslugi As WpisZnanejPoduslugi() = {
        New WpisZnanejPoduslugi("cbdg_otwory", "Otwory wiertnicze", "Lokalizacja i dane podstawowe otworów wiertniczych z Centralnej Bazy Danych Geologicznych (CBDG)."),
        New WpisZnanejPoduslugi("geofizyka/grawimetria", "Geofizyka: grawimetria", "Pomiary i punkty grawimetryczne z bazy CBDG."),
        New WpisZnanejPoduslugi("geofizyka/geofiz_geoel_zasiegi", "Geofizyka: zasięgi badań geoelektrycznych", "Zasięgi projektów i badań geoelektrycznych z bazy CBDG."),
        New WpisZnanejPoduslugi("geofizyka/magnetotelluryka", "Geofizyka: magnetotelluryka", "Sondowania i profile magnetotelluryczne z bazy CBDG."),
        New WpisZnanejPoduslugi("geofizyka/magnetyka", "Geofizyka: magnetyka", "Pomiary i anomalie magnetyczne z bazy CBDG."),
        New WpisZnanejPoduslugi("geofizyka/profile_2d_3d", "Geofizyka: profile sejsmiczne 2D/3D", "Przebieg profili sejsmicznych 2D oraz zasięgi badań 3D z bazy CBDG."),
        New WpisZnanejPoduslugi("geofizyka/geofiz_sge_pkt", "Geofizyka: sondowania geoelektryczne (SGE)", "Punkty sondowań geoelektrycznych (SGE) z bazy CBDG."),
        New WpisZnanejPoduslugi("/jaskinie/", "Jaskinie", "Lokalizacja i rejestr jaskiń Polski z bazy CBDG."),
        New WpisZnanejPoduslugi("cbdh_otwory", "Otwory hydrogeologiczne", "Otwory i punkty hydrogeologiczne z bazy Bank HYDRO."),
        New WpisZnanejPoduslugi("smgp50k", "SMGP 1:50 000", "Szczegółowa Mapa Geologiczna Polski w skali 1:50 000."),
        New WpisZnanejPoduslugi("mgp500k", "MGP 1:500 000", "Mapa Geologiczna Polski w skali 1:500 000."),
        New WpisZnanejPoduslugi("mlp50k", "MLP 1:50 000", "Mapa Litogenetyczna Polski w skali 1:50 000."),
        New WpisZnanejPoduslugi("mhp100k", "MHP 1:100 000", "Mapa Hydrogeologiczna Polski w skali 1:100 000."),
        New WpisZnanejPoduslugi("mso50k", "MGP/MSO 1:50 000", "Mapa Geośrodowiskowa Polski w skali 1:50 000."),
        New WpisZnanejPoduslugi("PRG/WMS/AdministrativeBoundaries", "Granice administracyjne", "Granice państwa, województw, powiatów i gmin (PRG)."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaNumeracjiAdresowej", "Adresy i ulice (KINA)", "Krajowa Integracja Numeracji Adresowej – punkty adresowe, place i ulice."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaMiejscowychPlanowZagospodarowaniaPrzestrzennego", "Plany zagospodarowania (KIMP)", "Usługa zbiorcza MPZP z serwerów gmin w Polsce."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaStudiumKierunkowZagospodarowaniaPrzestrzennego", "Studium kierunków (KISKZP)", "Usługa zbiorcza studiów uwarunkowań i kierunków zagospodarowania gmin."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaBazDanychObiektowTopograficznych", "Obiekty BDOT500 (KIBDOT)", "Usługa zbiorcza obiektów topograficznych BDOT500 z powiatów."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaEwidencjiGruntow", "Działki i budynki (KIEG)", "Usługa zbiorcza ewidencji gruntów i budynków z powiatów."),
        New WpisZnanejPoduslugi("KrajowaIntegracjaUzbrojeniaTerenu", "Uzbrojenie terenu (KIUT)", "Usługa zbiorcza sieci uzbrojenia terenu z powiatów."),
        New WpisZnanejPoduslugi("BDOT10k-BDOO", "Podkład BDOT10k i BDOO", "Płynna mapa podkładowa łącząca BDOO (małe skale) i BDOT10k (duże skale)."),
        New WpisZnanejPoduslugi("G2_MOBILE_500", "Kafle mobilne BDOT500", "Zoptymalizowana wersja mobilna kafli obiektów topograficznych."),
        New WpisZnanejPoduslugi("BDOO/WMTS/2021", "Wydanie 2021", "Baza Danych Obiektów Ogólnogeograficznych – wydanie archiwalne z 2021 r."),
        New WpisZnanejPoduslugi("BDOO/WMS/2021", "Wydanie 2021", "Baza Danych Obiektów Ogólnogeograficznych – wydanie z 2021 r. (WMS)."),
        New WpisZnanejPoduslugi("BDOO/WMTS/2015", "Wydanie 2015", "Baza Danych Obiektów Ogólnogeograficznych – wydanie archiwalne z 2015 r."),
        New WpisZnanejPoduslugi("kompozycja_BDOT10k_WMS", "Kompozycja tematyczna", "Kompozycja BDOT10k z możliwością wyboru pojedynczych warstw."),
        New WpisZnanejPoduslugi("StandardResolutionTime", "Standardowa archiwalna (TIME)", "Archiwalne roczniki ortofotomapy o standardowej rozdzielczości (wymiar czasu TIME)."),
        New WpisZnanejPoduslugi("HighResolutionTime", "Wysoka rozdzielczość archiwalna (TIME)", "Archiwalne roczniki ortofotomapy o wysokiej rozdzielczości (wymiar czasu TIME)."),
        New WpisZnanejPoduslugi("StandardResolution", "Rozdzielczość standardowa", "Aktualna ortofotomapa Polski o standardowej rozdzielczości terenowej."),
        New WpisZnanejPoduslugi("HighResolution", "Wysoka rozdzielczość", "Ortofotomapa o podwyższonej rozdzielczości (dla dostępnych obszarów)."),
        New WpisZnanejPoduslugi("TrueOrtho", "Prawdziwa ortofotomapa (TrueOrtho)", "Ortofotomapa bez przesunięć perspektywicznych wysokich obiektów (TrueOrtho)."),
        New WpisZnanejPoduslugi("HypsometryAndShadedRelief", "Hipsometria i cieniowanie", "Połączona mapa hipsometryczna oraz cieniowana rzeźba terenu (NMT GRID1)."),
        New WpisZnanejPoduslugi("ShadedRelief", "Cieniowanie rzeźby", "Cieniowana rzeźba terenu z Numerycznego Modelu Terenu (NMT GRID1)."),
        New WpisZnanejPoduslugi("Hypsometry", "Hipsometria barwna", "Barwna mapa wysokościowa z Numerycznego Modelu Terenu (NMT GRID1)."),
        New WpisZnanejPoduslugi("OsnowaGrawimetryczna", "Osnowa grawimetryczna", "Punkty podstawowej osnowy grawimetrycznej kraju."),
        New WpisZnanejPoduslugi("OsnowaMagnetyczna", "Osnowa magnetyczna", "Punkty podstawowej osnowy magnetycznej kraju."),
        New WpisZnanejPoduslugi("PodstawowaOsnowaGeodezyjnaPozioma", "Osnowa pozioma", "Punkty podstawowej osnowy geodezyjnej poziomej."),
        New WpisZnanejPoduslugi("PodstawowaOsnowaGeodezyjnaWysokosciowa", "Osnowa wysokościowa", "Repery podstawowej osnowy geodezyjnej wysokościowej."),
        New WpisZnanejPoduslugi("PodstawowaOsnowaGeodezyjna", "Osnowa geodezyjna podstawowa", "Połączona usługa punktów podstawowej osnowy geodezyjnej."),
        New WpisZnanejPoduslugi("Archiwalne_kartoteki", "Archiwalne kartoteki", "Skorowidze i archiwalne kartoteki punktów osnów geodezyjnych."),
        New WpisZnanejPoduslugi("bu_p_administracja", "Administracja publiczna", "Obiekty użyteczności publicznej – administracja."),
        New WpisZnanejPoduslugi("bu_p_dworzec", "Dworce", "Obiekty użyteczności publicznej – dworce kolejowe i autobusowe."),
        New WpisZnanejPoduslugi("bup_informacja_turystyczna", "Informacja turystyczna", "Punkty informacji turystycznej."),
        New WpisZnanejPoduslugi("bup_kultura", "Kultura", "Obiekty i instytucje kultury."),
        New WpisZnanejPoduslugi("bup_obiekty_sportowe", "Obiekty sportowe", "Hale, stadiony i obiekty sportowe."),
        New WpisZnanejPoduslugi("bu_p_obiekty_turystyczne", "Obiekty turystyczne", "Obiekty i atrakcje turystyczne."),
        New WpisZnanejPoduslugi("bu_p_ochrona_zdrowia", "Ochrona zdrowia", "Szpitale, przychodnie i placówki medyczne."),
        New WpisZnanejPoduslugi("bu_p_placowki_opiekuncze", "Placówki opiekuńcze", "Placówki opiekuńczo-wychowawcze i domy pomocy."),
        New WpisZnanejPoduslugi("bu_p_placowki_oswiatowe", "Placówki oświatowe", "Szkoły, przedszkola i placówki edukacyjne."),
        New WpisZnanejPoduslugi("bu_p_placowki_sadowo_karne", "Placówki sądowo-karne", "Sądy, prokuratury i zakłady karne."),
        New WpisZnanejPoduslugi("bu_p_policja", "Policja", "Komendy i posterunki policji."),
        New WpisZnanejPoduslugi("bu_p_port_lotniczy", "Porty lotnicze", "Lotniska i lądowiska."),
        New WpisZnanejPoduslugi("bu_p_straz_pozarna", "Straż pożarna", "Jednostki i remizy straży pożarnej."),
        New WpisZnanejPoduslugi("bu_p_swiatynie", "Świątynie", "Kościoły i obiekty sakralne."),
        New WpisZnanejPoduslugi("UMWL_GLEB_ORGANICZ", "Gleby organiczne", "Zasięg i charakterystyka gleb organicznych."),
        New WpisZnanejPoduslugi("UMWL_GLEB_ROL_wektor", "Mapa gleb – wektorowa", "Mapa glebowo-rolnicza w wersji wektorowej (kafelkowej)."),
        New WpisZnanejPoduslugi("UMWL_GLEB_ROL_ras", "Mapa gleb – rastrowa", "Mapa glebowo-rolnicza w wersji rastrowej (kafelkowej)."),
        New WpisZnanejPoduslugi("UMWL_GLEB_ROL_WMS", "Kompozycja WMS", "Mapa glebowo-rolnicza w usłudze WMS.")
    }

    Private Shared ReadOnly MapaWbudowanych As New Lazy(Of Dictionary(Of String, WpisKataloguSerwerow))(
        Function()
            Dim mapa As New Dictionary(Of String, WpisKataloguSerwerow)(StringComparer.OrdinalIgnoreCase)
            Try
                For Each w In WczytajWbudowany()
                    Dim k = NormalizujAdresDoPorownania(w.Adres)
                    If k <> "" AndAlso Not mapa.ContainsKey(k) Then mapa(k) = w
                Next
            Catch
            End Try
            Return mapa
        End Function)

    Private Sub New()
    End Sub

    ''' <summary>Wczytuje wbudowaną listę sprawdzonych usług (z opcjonalnym nadpisaniem z zewnętrznego pliku JSON).</summary>
    Public Shared Function WczytajWbudowany(Optional sciezkaZewnetrzna As String = Nothing) As List(Of WpisKataloguSerwerow)
        If Not String.IsNullOrWhiteSpace(sciezkaZewnetrzna) AndAlso File.Exists(sciezkaZewnetrzna) Then
            Return ZJsona(File.ReadAllText(sciezkaZewnetrzna, Encoding.UTF8), ZrodloKataloguSerwerow.Wbudowany)
        End If

        Dim asm = GetType(KatalogSerwerow).Assembly
        Using strumien = asm.GetManifestResourceStream(NazwaZasobuWbudowanego)
            If strumien Is Nothing Then
                Dim nazwaDopasowana = asm.GetManifestResourceNames().
                    FirstOrDefault(Function(n) n.EndsWith("serwery_dodatkowe.json", StringComparison.OrdinalIgnoreCase))
                If nazwaDopasowana Is Nothing Then Return New List(Of WpisKataloguSerwerow)()
                Using s2 = asm.GetManifestResourceStream(nazwaDopasowana)
                    Using sr As New StreamReader(s2, Encoding.UTF8)
                        Return ZJsona(sr.ReadToEnd(), ZrodloKataloguSerwerow.Wbudowany)
                    End Using
                End Using
            End If
            Using sr As New StreamReader(strumien, Encoding.UTF8)
                Return ZJsona(sr.ReadToEnd(), ZrodloKataloguSerwerow.Wbudowany)
            End Using
        End Using
    End Function

    ''' <summary>Odczytuje listę wpisów katalogu z dokumentu JSON (wbudowanego lub z pamięci podręcznej).</summary>
    Public Shared Function ZJsona(json As String, Optional domyslneZrodlo As ZrodloKataloguSerwerow = ZrodloKataloguSerwerow.Wbudowany) As List(Of WpisKataloguSerwerow)
        Dim wynik As New List(Of WpisKataloguSerwerow)()
        If String.IsNullOrWhiteSpace(json) Then Return wynik

        Using doc = JsonDocument.Parse(json)
            If doc.RootElement.ValueKind <> JsonValueKind.Array Then Return wynik
            For Each el In doc.RootElement.EnumerateArray()
                If el.ValueKind <> JsonValueKind.Object Then Continue For
                Dim typJawny = ParsujTyp(TekstJson(el, "typ"))
                Dim typWykryty As TypUslugiMapowej = typJawny
                Dim adres = OczyscAdresUslugi(TekstJson(el, "adres"), typWykryty)
                If adres = "" Then Continue For
                If typJawny <> TypUslugiMapowej.Nieznana Then typWykryty = typJawny

                Dim zrodloTekst = TekstJson(el, "zrodlo")
                Dim zrodlo As ZrodloKataloguSerwerow = domyslneZrodlo
                If zrodloTekst <> "" Then [Enum].TryParse(zrodloTekst, True, zrodlo)

                Dim nazwa = TekstJson(el, "nazwa")
                If nazwa = "" Then Continue For

                Dim czySkorowidz = LogicznaJson(el, "czySkorowidz", False) OrElse
                                   CzyWygladaNaSkorowidz(nazwa, TekstJson(el, "kategoria"), adres)

                wynik.Add(New WpisKataloguSerwerow With {
                    .Id = TekstJson(el, "id"),
                    .Teryt = TekstJson(el, "teryt"),
                    .Nazwa = nazwa,
                    .Dostawca = TekstJson(el, "dostawca"),
                    .Obszar = TekstJson(el, "obszar"),
                    .Kategoria = TekstJson(el, "kategoria"),
                    .Typ = typWykryty,
                    .Adres = adres,
                    .Opis = TekstJson(el, "opis"),
                    .SugerowanaNazwaZbioru = TekstJson(el, "sugerowanaNazwaZbioru"),
                    .CzySkorowidz = czySkorowidz,
                    .IgnorujSsl = LogicznaJson(el, "ignorujSsl", False),
                    .Zrodlo = zrodlo
                })
            Next
        End Using
        Return wynik
    End Function

    ''' <summary>Serializuje wpisy katalogu do JSON UTF-8 (m.in. do lokalnej pamięci podręcznej).</summary>
    Public Shared Function DoJsona(wpisy As IEnumerable(Of WpisKataloguSerwerow)) As String
        Using ms As New MemoryStream()
            Using w As New Utf8JsonWriter(ms, New JsonWriterOptions With {.Indented = True, .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping})
                w.WriteStartArray()
                For Each p In If(wpisy, Enumerable.Empty(Of WpisKataloguSerwerow)())
                    w.WriteStartObject()
                    If p.Id <> "" Then w.WriteString("id", p.Id)
                    If p.Teryt <> "" Then w.WriteString("teryt", p.Teryt)
                    w.WriteString("nazwa", p.Nazwa)
                    If p.Dostawca <> "" Then w.WriteString("dostawca", p.Dostawca)
                    If p.Obszar <> "" Then w.WriteString("obszar", p.Obszar)
                    If p.Kategoria <> "" Then w.WriteString("kategoria", p.Kategoria)
                    Dim typTekst = TekstTypu(p.Typ)
                    If typTekst <> "" Then w.WriteString("typ", typTekst)
                    w.WriteString("adres", p.Adres)
                    If p.Opis <> "" Then w.WriteString("opis", p.Opis)
                    If p.SugerowanaNazwaZbioru <> "" Then w.WriteString("sugerowanaNazwaZbioru", p.SugerowanaNazwaZbioru)
                    If p.CzySkorowidz Then w.WriteBoolean("czySkorowidz", True)
                    If p.IgnorujSsl Then w.WriteBoolean("ignorujSsl", True)
                    w.WriteString("zrodlo", p.Zrodlo.ToString())
                    w.WriteEndObject()
                Next
                w.WriteEndArray()
            End Using
            Return Encoding.UTF8.GetString(ms.ToArray())
        End Using
    End Function

    ''' <summary>
    ''' Czyści adres usługi pobrany ze spisu rządowego: usuwa prefiksy WMS=/WMTS=, odrzuca WFS=,
    ''' dekoduje encje HTML i naprawia zdublowany znak zapytania w query stringu.
    ''' </summary>
    Public Shared Function OczyscAdresUslugi(surowy As String, ByRef wykrytyTyp As TypUslugiMapowej) As String
        If String.IsNullOrWhiteSpace(surowy) Then Return ""
        Dim s As String = WebUtility.HtmlDecode(surowy).Trim().Trim(""""c)
        s = Regex.Replace(s, "\s+", "")
        If s = "" Then Return ""

        If s.StartsWith("WFS=", StringComparison.OrdinalIgnoreCase) Then
            wykrytyTyp = TypUslugiMapowej.Nieznana
            Return ""
        ElseIf s.StartsWith("WMS=", StringComparison.OrdinalIgnoreCase) Then
            wykrytyTyp = TypUslugiMapowej.Wms
            s = s.Substring(4).Trim()
        ElseIf s.StartsWith("WMTS=", StringComparison.OrdinalIgnoreCase) Then
            wykrytyTyp = TypUslugiMapowej.Wmts
            s = s.Substring(5).Trim()
        End If

        If Not (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) Then
            Return ""
        End If

        'niektóre wpisy EZiUDP mają drugie "?" przed ?service=WMS zamiast "&"
        Dim pierwszePyt As Integer = s.IndexOf("?"c)
        If pierwszePyt >= 0 Then
            Dim przed = s.Substring(0, pierwszePyt + 1)
            Dim po = s.Substring(pierwszePyt + 1).Replace("?"c, "&"c)
            s = przed & po
        End If

        If wykrytyTyp = TypUslugiMapowej.Nieznana Then
            wykrytyTyp = KafleXyz.WybierzTyp(s)
        End If
        Return s
    End Function

    ''' <summary>
    ''' Tworzy kanoniczny klucz adresu do wykrywania duplikatów między katalogiem a zbiorami już
    ''' dodanymi w MapoTero (ignoruje protokół http/https, porty domyślne i parametry GetMap/GetCapabilities).
    ''' </summary>
    Public Shared Function NormalizujAdresDoPorownania(adres As String) As String
        Dim t As TypUslugiMapowej = TypUslugiMapowej.Nieznana
        Dim czysty = OczyscAdresUslugi(adres, t)
        If czysty = "" Then Return ""

        If KafleXyz.CzyAdresXyz(czysty) Then
            Dim subdomeny As List(Of String) = Nothing
            Dim tms As Boolean = False
            Dim norm = KafleXyz.NormalizujSzablon(czysty, subdomeny, tms)
            norm = Regex.Replace(norm, "^https?://", "", RegexOptions.IgnoreCase).TrimEnd("/"c)
            Return norm.ToLowerInvariant()
        End If

        Dim idxPyt As Integer = czysty.IndexOf("?"c)
        Dim baza As String = If(idxPyt >= 0, czysty.Substring(0, idxPyt), czysty)
        Dim query As String = If(idxPyt >= 0, czysty.Substring(idxPyt + 1), "")

        Dim uri As Uri = Nothing
        If Uri.TryCreate(baza, UriKind.Absolute, uri) Then
            Dim host = uri.Host.ToLowerInvariant()
            Dim portStr = If(uri.IsDefaultPort OrElse uri.Port = 80 OrElse uri.Port = 443, "", ":" & uri.Port)
            Dim sciezka = uri.AbsolutePath.TrimEnd("/"c).ToLowerInvariant()
            baza = host & portStr & sciezka
        Else
            baza = Regex.Replace(baza, "^https?://", "", RegexOptions.IgnoreCase).TrimEnd("/"c).ToLowerInvariant()
        End If

        If query <> "" Then
            Dim zachowane As New List(Of String)
            For Each czesc In query.Split({"&"c}, StringSplitOptions.RemoveEmptyEntries)
                Dim eq = czesc.IndexOf("="c)
                Dim klucz = If(eq >= 0, czesc.Substring(0, eq), czesc).Trim()
                If klucz = "" OrElse ParametryOgcDoPomijania.Contains(klucz) Then Continue For
                zachowane.Add(czesc.ToLowerInvariant())
            Next
            zachowane.Sort(StringComparer.Ordinal)
            If zachowane.Count > 0 Then baza &= "?" & String.Join("&", zachowane)
        End If

        Return baza
    End Function

    ''' <summary>Oznacza wpisy katalogu, których adres odpowiada już wczytanemu zbiorowi map w programie.</summary>
    Public Shared Sub DopasujIstniejace(wpisy As IEnumerable(Of WpisKataloguSerwerow),
                                        istniejaceZbiory As IEnumerable(Of KeyValuePair(Of String, String)))
        If wpisy Is Nothing Then Exit Sub
        Dim mapa As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If istniejaceZbiory IsNot Nothing Then
            For Each kv In istniejaceZbiory
                Dim k = NormalizujAdresDoPorownania(kv.Value)
                If k <> "" AndAlso Not mapa.ContainsKey(k) Then mapa(k) = kv.Key
            Next
        End If

        For Each w In wpisy
            Dim k = NormalizujAdresDoPorownania(w.Adres)
            Dim nazwaZbioru As String = ""
            If k <> "" AndAlso mapa.TryGetValue(k, nazwaZbioru) Then
                w.JuzDodanyZbior = nazwaZbioru
            Else
                w.JuzDodanyZbior = ""
            End If
        Next
    End Sub

    ''' <summary>Parsuje odpowiedź JSON z API EZiUDP (integracja.gugik.gov.pl/eziudp/api.php) z deduplikacją po adresie URL.</summary>
    Public Shared Function ParsujEziudpJson(json As String, zrodlo As ZrodloKataloguSerwerow) As List(Of WpisKataloguSerwerow)
        Dim wynik As New List(Of WpisKataloguSerwerow)()
        If String.IsNullOrWhiteSpace(json) Then Return wynik

        Dim poAdresie As New Dictionary(Of String, WpisKataloguSerwerow)(StringComparer.OrdinalIgnoreCase)
        Dim zbioryDlaAdresu As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)

        Using doc = JsonDocument.Parse(json)
            Dim dataEl As JsonElement
            If Not doc.RootElement.TryGetProperty("data", dataEl) OrElse dataEl.ValueKind <> JsonValueKind.Array Then
                Return wynik
            End If

            For Each item In dataEl.EnumerateArray()
                If item.ValueKind <> JsonValueKind.Object Then Continue For
                Dim uslugiEl As JsonElement
                If Not item.TryGetProperty("uslugi", uslugiEl) OrElse uslugiEl.ValueKind <> JsonValueKind.Object Then Continue For
                Dim przeglEl As JsonElement
                If Not uslugiEl.TryGetProperty("przegladania", przeglEl) OrElse przeglEl.ValueKind <> JsonValueKind.Array Then Continue For

                Dim idiip = TekstJson(item, "idiip")
                Dim teryt = TekstJson(item, "teryt")
                Dim zbior = TekstJson(item, "zbior")
                Dim organ = TekstJson(item, "organ")
                Dim obszar = TekstJson(item, "obszar")

                Dim tematy As New List(Of String)
                Dim temEl As JsonElement
                If item.TryGetProperty("tematy", temEl) AndAlso temEl.ValueKind = JsonValueKind.Array Then
                    For Each t In temEl.EnumerateArray()
                        If t.ValueKind = JsonValueKind.String Then
                            Dim tv = If(t.GetString(), "").Trim()
                            If tv <> "" Then tematy.Add(tv)
                        End If
                    Next
                End If

                For Each uEl In przeglEl.EnumerateArray()
                    If uEl.ValueKind <> JsonValueKind.String Then Continue For
                    Dim surowyUrl = If(uEl.GetString(), "")
                    Dim wykrytyTyp As TypUslugiMapowej = TypUslugiMapowej.Nieznana
                    Dim czystyUrl = OczyscAdresUslugi(surowyUrl, wykrytyTyp)
                    If czystyUrl = "" Then Continue For

                    Dim kluczUrl = NormalizujAdresDoPorownania(czystyUrl)
                    If kluczUrl = "" Then Continue For

                    Dim istniejacy As WpisKataloguSerwerow = Nothing
                    If poAdresie.TryGetValue(kluczUrl, istniejacy) Then
                        Dim nazwyZbiorow = zbioryDlaAdresu(kluczUrl)
                        If zbior <> "" AndAlso nazwyZbiorow.Add(zbior) AndAlso nazwyZbiorow.Count <= 25 Then
                            istniejacy.Opis = DopiszDoOpisu(istniejacy.Opis, zbior)
                        End If
                        'preferuj HTTPS nad HTTP jeśli oba warianty wskazują ten sam endpoint
                        If istniejacy.Adres.StartsWith("http://", StringComparison.OrdinalIgnoreCase) AndAlso
                           czystyUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                            istniejacy.Adres = czystyUrl
                        End If
                        Continue For
                    End If

                    Dim kategoria = WyznaczKategorieEziudp(zbior, organ, obszar, zrodlo)
                    Dim czySkor = CzyWygladaNaSkorowidz(zbior, kategoria, czystyUrl)
                    Dim opisSb As New StringBuilder()
                    If tematy.Count > 0 Then opisSb.Append("Tematy: ").Append(String.Join(", ", tematy.Take(4)))
                    If teryt <> "" Then
                        If opisSb.Length > 0 Then opisSb.Append(" | ")
                        opisSb.Append("TERYT: ").Append(teryt)
                    End If
                    If idiip <> "" Then
                        If opisSb.Length > 0 Then opisSb.Append(" | ")
                        opisSb.Append("IdIIP: ").Append(idiip)
                    End If

                    Dim nazwaWyswietlana = If(zbior <> "", zbior, czystyUrl)
                    If zrodlo = ZrodloKataloguSerwerow.EziudpLokalne AndAlso obszar <> "" AndAlso
                       nazwaWyswietlana.IndexOf(obszar, StringComparison.OrdinalIgnoreCase) < 0 Then
                        nazwaWyswietlana &= " – " & obszar
                    End If

                    Dim wpis As New WpisKataloguSerwerow With {
                        .Id = idiip,
                        .Teryt = teryt,
                        .Nazwa = nazwaWyswietlana,
                        .Dostawca = organ,
                        .Obszar = If(obszar <> "", obszar, "Polska"),
                        .Kategoria = kategoria,
                        .Typ = wykrytyTyp,
                        .Adres = czystyUrl,
                        .Opis = opisSb.ToString(),
                        .CzySkorowidz = czySkor,
                        .Zrodlo = zrodlo
                    }
                    poAdresie(kluczUrl) = wpis
                    zbioryDlaAdresu(kluczUrl) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                    If zbior <> "" Then zbioryDlaAdresu(kluczUrl).Add(zbior)
                    wynik.Add(wpis)
                Next
            Next
        End Using

        WzbogacWpisy(wynik)
        Return wynik
    End Function

    ''' <summary>Parsuje odpowiedź JSON WordPress API strony Geoportal.gov.pl z wykazem usług WMS/WMTS.</summary>
    Public Shared Function ParsujGeoportalWpJson(json As String) As List(Of WpisKataloguSerwerow)
        If String.IsNullOrWhiteSpace(json) Then Return New List(Of WpisKataloguSerwerow)()
        Using doc = JsonDocument.Parse(json)
            Dim contentEl As JsonElement
            Dim renderedEl As JsonElement
            If doc.RootElement.TryGetProperty("content", contentEl) AndAlso
               contentEl.ValueKind = JsonValueKind.Object AndAlso
               contentEl.TryGetProperty("rendered", renderedEl) AndAlso
               renderedEl.ValueKind = JsonValueKind.String Then
                Return ParsujGeoportalHtml(If(renderedEl.GetString(), ""))
            End If
        End Using
        Return New List(Of WpisKataloguSerwerow)()
    End Function

    ''' <summary>Parsuje tabele HTML oficjalnej listy usług przeglądania Geoportal.gov.pl (łącznie ze skorowidzami).</summary>
    Public Shared Function ParsujGeoportalHtml(htmlTekst As String) As List(Of WpisKataloguSerwerow)
        Dim wynik As New List(Of WpisKataloguSerwerow)()
        If String.IsNullOrWhiteSpace(htmlTekst) Then Return wynik

        Dim dodaneAdresy As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim biezacaSekcja As String = "Usługi GUGiK"

        Dim wzorzec As New Regex("(<h[23][^>]*>.*?</h[23]>|<tr[^>]*>.*?</tr>)", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        For Each m As Match In wzorzec.Matches(htmlTekst)
            Dim frag = m.Value
            If frag.StartsWith("<h", StringComparison.OrdinalIgnoreCase) Then
                Dim naglowek = OczyscHtml(frag)
                If naglowek <> "" Then biezacaSekcja = naglowek
                Continue For
            End If

            If frag.IndexOf("<th", StringComparison.OrdinalIgnoreCase) >= 0 Then Continue For

            Dim komorki = Regex.Matches(frag, "<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If komorki.Count < 2 Then Continue For

            Dim pierwsza = komorki(0).Groups(1).Value
            Dim typ As TypUslugiMapowej = If(pierwsza.IndexOf("WMTS", StringComparison.OrdinalIgnoreCase) >= 0,
                                             TypUslugiMapowej.Wmts, TypUslugiMapowej.Wms)

            Dim nazwa = OczyscHtml(komorki(1).Groups(1).Value)
            If nazwa = "" Then Continue For

            Dim znalezionyUrl As String = ""
            For Each lm As Match In Regex.Matches(frag, "href\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)
                Dim kand = WebUtility.HtmlDecode(lm.Groups(1).Value).Trim()
                If kand.IndexOf("imapnext", StringComparison.OrdinalIgnoreCase) >= 0 Then Continue For
                If kand.IndexOf("adres_us", StringComparison.OrdinalIgnoreCase) >= 0 Then Continue For
                Dim tKand As TypUslugiMapowej = typ
                Dim czysty = OczyscAdresUslugi(kand, tKand)
                If czysty <> "" Then
                    znalezionyUrl = czysty
                    Exit For
                End If
            Next
            If znalezionyUrl = "" Then Continue For

            Dim kluczUrlPorownania = NormalizujAdresDoPorownania(znalezionyUrl)
            Dim klucz = kluczUrlPorownania & "|" & typ.ToString()
            If Not dodaneAdresy.Add(klucz) Then Continue For

            Dim czySkor = CzyWygladaNaSkorowidz(nazwa, biezacaSekcja, znalezionyUrl)
            Dim opis As String = If(czySkor, "Skorowidz / siatka arkuszy z wykazu Geoportal.gov.pl.", "Oficjalna usługa z wykazu Geoportal.gov.pl.")
            If znalezionyUrl.IndexOf("Time", StringComparison.OrdinalIgnoreCase) >= 0 Then
                opis &= " Uwaga: usługa archiwalna z wymiarem czasu (TIME)."
            ElseIf znalezionyUrl.IndexOf("KrajowaIntegracjaMiejscowychPlanow", StringComparison.OrdinalIgnoreCase) >= 0 Then
                opis &= " Przekierowuje zapytania na serwery gmin."
            End If

            wynik.Add(New WpisKataloguSerwerow With {
                .Nazwa = nazwa,
                .Dostawca = "GUGiK / Geoportal",
                .Obszar = "Polska",
                .Kategoria = biezacaSekcja,
                .Typ = typ,
                .Adres = znalezionyUrl,
                .Opis = opis,
                .CzySkorowidz = czySkor,
                .Zrodlo = ZrodloKataloguSerwerow.Geoportal
            })
        Next

        WzbogacWpisy(wynik)
        Return wynik
    End Function

    ''' <summary>Zwraca etykietę kolumny „Stan w MapoTero” dla podanego wpisu katalogu.</summary>
    Public Shared Function TekstStanuWMapoTero(wpis As WpisKataloguSerwerow) As String
        If wpis IsNot Nothing AndAlso wpis.CzyJuzDodany Then
            Return "W MapoTero (" & wpis.JuzDodanyZbior & ")"
        End If
        Return TekstStanuMoznaDodac
    End Function

    ''' <summary>Zwraca krótki, czytelny opis charakterystyki technicznej usługi według jej typu (WMS/WMTS/XYZ/TMS).</summary>
    Public Shared Function OpisTypuUslugi(typ As TypUslugiMapowej) As String
        Select Case typ
            Case TypUslugiMapowej.Wms
                Return "Usługa rastrowa WMS – generuje mapę na żądanie w dowolnej skali i zasięgu (możliwość wyboru pojedynczych warstw)."
            Case TypUslugiMapowej.Wmts
                Return "Usługa kafelkowa WMTS – bardzo szybkie pobieranie gotowych kafelków w stałych poziomach powiększenia."
            Case TypUslugiMapowej.Xyz, TypUslugiMapowej.Tms
                Return "Usługa kafelkowa " & KafleXyz.NazwaTypu(typ) & " – szybka siatka kafelków w układzie Web Mercator."
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Wyznacza czytelny polski podtytuł usługi (np. "Otwory wiertnicze", "Geofizyka: magnetotelluryka")
    ''' na podstawie adresu URL punktu końcowego.
    ''' </summary>
    Public Shared Function WyznaczPodtytulUslugi(adres As String, Optional tylkoZeSlownika As Boolean = False) As String
        If String.IsNullOrWhiteSpace(adres) Then Return ""
        Dim a = adres.Trim()

        For Each z In ZnanePoduslugi
            If a.IndexOf(z.Wzorzec, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return z.Podtytul
            End If
        Next

        If tylkoZeSlownika Then Return ""

        'sprawdź parametr map=... (częsty w MapServer CGI)
        Dim mapMatch = Regex.Match(a, "[?&]map=([^&]+)", RegexOptions.IgnoreCase)
        If mapMatch.Success Then
            Dim sciezkaMap = WebUtility.UrlDecode(mapMatch.Groups(1).Value)
            Dim nazwaPliku = Path.GetFileNameWithoutExtension(sciezkaMap)
            Dim sformatowana = FormatujSegmentAdresu(nazwaPliku)
            If sformatowana <> "" Then Return sformatowana
        End If

        'uniwersalna heurystyka ze ścieżki URL
        Dim bezQuery = a.Split("?"c)(0)
        Dim segmenty = bezQuery.Split({"/"c, "\"c}, StringSplitOptions.RemoveEmptyEntries)
        If segmenty.Length <= 2 Then Return ""

        For i = segmenty.Length - 1 To 2 Step -1
            Dim seg = segmenty(i).Trim()
            Dim sformatowana = FormatujSegmentAdresu(seg)
            If sformatowana <> "" Then Return sformatowana
        Next

        Return ""
    End Function

    ''' <summary>Zwraca krótki opis merytoryczny podusługi ze słownika znanych usług.</summary>
    Public Shared Function WyznaczOpisPoduslugi(adres As String) As String
        If String.IsNullOrWhiteSpace(adres) Then Return ""
        Dim a = adres.Trim()
        For Each z In ZnanePoduslugi
            If a.IndexOf(z.Wzorzec, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return z.Opis
            End If
        Next
        Return ""
    End Function

    ''' <summary>
    ''' Doprecyzowuje ogólną nazwę zbioru (np. "Centralna Baza Danych Geologicznych (CBDG)")
    ''' o czytelny podtytuł z adresu URL, jeśli nazwa nie zawiera jeszcze tego wyróżnika.
    ''' </summary>
    Public Shared Function DoprecyzujNazweUslugi(nazwaZbioru As String, adres As String, Optional tylkoZeSlownika As Boolean = True) As String
        Dim baza = If(nazwaZbioru, "").Trim()
        If baza = "" Then baza = adres
        Dim podtytul = WyznaczPodtytulUslugi(adres, tylkoZeSlownika:=tylkoZeSlownika)
        If String.IsNullOrWhiteSpace(podtytul) Then Return baza

        Dim uBaza = FiltrWarstw.Uprosc(baza)
        Dim slowaBaza As New HashSet(Of String)(FiltrWarstw.Slowa(baza))
        Dim slowaPod = FiltrWarstw.Slowa(podtytul)
        'szukamy słów wyróżniających w podtytule (które nie występują w samej bazie)
        Dim slowaWyrozniajace = slowaPod.Where(Function(s) Not slowaBaza.Contains(s)).ToList()
        If slowaWyrozniajace.Count = 0 Then Return baza

        'jeśli baza zawiera już rdzenie wszystkich słów wyróżniających (np. "wysokiej" vs "wysoka"), nie powielamy
        Dim juzZawieraWszystkie = slowaWyrozniajace.All(Function(s)
            If s.Length < 3 Then Return True
            Dim rdzen = If(s.Length >= 6, s.Substring(0, s.Length - 2), s)
            Return uBaza.Contains(rdzen)
        End Function)
        If juzZawieraWszystkie Then Return baza

        Return baza & " – " & podtytul
    End Function

    ''' <summary>
    ''' Wzbogaca wpisy z rejestrów online: uzupełnia nazwy o podusługę i obszar (dla usług wojewódzkich/lokalnych),
    ''' przenosi sprawdzone opisy z katalogu wbudowanego oraz rozróżnia ewentualne pozostałe wpisy o identycznej nazwie.
    ''' </summary>
    Public Shared Sub WzbogacWpisy(wpisy As IList(Of WpisKataloguSerwerow))
        If wpisy Is Nothing OrElse wpisy.Count = 0 Then Exit Sub

        For Each w In wpisy
            If w Is Nothing OrElse w.Zrodlo = ZrodloKataloguSerwerow.Wbudowany Then Continue For

            w.Nazwa = DoprecyzujNazweUslugi(w.Nazwa, w.Adres, tylkoZeSlownika:=True)

            If Not String.IsNullOrWhiteSpace(w.Obszar) AndAlso
               Not String.Equals(w.Obszar, "Polska", StringComparison.OrdinalIgnoreCase) AndAlso
               Not String.Equals(w.Obszar, "Świat", StringComparison.OrdinalIgnoreCase) AndAlso
               Not ZawieraWszystkieSlowa(w.Nazwa, w.Obszar) Then
                w.Nazwa &= " – " & w.Obszar
            End If

            Dim kluczUrl = NormalizujAdresDoPorownania(w.Adres)
            Dim wbudowany As WpisKataloguSerwerow = Nothing
            Dim dopasowanoWbudowany = (kluczUrl <> "" AndAlso MapaWbudowanych.Value.TryGetValue(kluczUrl, wbudowany))
            If dopasowanoWbudowany Then
                If Not String.IsNullOrWhiteSpace(wbudowany.Opis) AndAlso Not ZawieraFragment(w.Opis, wbudowany.Opis) Then
                    w.Opis = PolaczOpis(wbudowany.Opis, w.Opis)
                End If
                If String.IsNullOrWhiteSpace(w.SugerowanaNazwaZbioru) AndAlso Not String.IsNullOrWhiteSpace(wbudowany.SugerowanaNazwaZbioru) Then
                    w.SugerowanaNazwaZbioru = wbudowany.SugerowanaNazwaZbioru
                End If
            Else
                Dim opisPod = WyznaczOpisPoduslugi(w.Adres)
                If opisPod <> "" AndAlso Not ZawieraFragment(w.Opis, opisPod) Then
                    w.Opis = PolaczOpis(opisPod, w.Opis)
                End If
            End If
        Next

        'jeśli mimo słownika nadal istnieje kilka wpisów o identycznej nazwie, doprecyzuj je z segmentu ścieżki URL
        Dim grupy = wpisy.Where(Function(w) w IsNot Nothing AndAlso w.Zrodlo <> ZrodloKataloguSerwerow.Wbudowany).
                          GroupBy(Function(w) FiltrWarstw.Uprosc(w.Nazwa))
        For Each g In grupy
            If g.Count() <= 1 Then Continue For
            For Each w In g
                w.Nazwa = DoprecyzujNazweUslugi(w.Nazwa, w.Adres, tylkoZeSlownika:=False)
            Next
        Next
    End Sub

    ''' <summary>Formatuje zwięzły podgląd zawartości serwera na podstawie odpowiedzi GetCapabilities.</summary>
    Public Shared Function FormatujPodgladCapabilities(res As WynikCapabilities, Optional maxWarstw As Integer = 6) As String
        If res Is Nothing OrElse Not res.Sukces Then Return ""

        Dim linie As New List(Of String)()
        If res.Typ = TypUslugiMapowej.Wms AndAlso res.UslugaWms IsNot Nothing Then
            Dim tytul = Regex.Replace(If(res.UslugaWms.Tytul, ""), "\s+", " ").Trim()
            Dim abstrakt = Regex.Replace(If(res.UslugaWms.Abstrakt, ""), "\s+", " ").Trim()
            If abstrakt.Length > 220 Then abstrakt = abstrakt.Substring(0, 217).TrimEnd() & "…"
            If tytul <> "" Then linie.Add("Tytuł z serwera: " & tytul)
            If abstrakt <> "" AndAlso Not String.Equals(abstrakt, tytul, StringComparison.OrdinalIgnoreCase) Then
                linie.Add("Opis z serwera: " & abstrakt)
            End If
            Dim warstwy = res.UslugaWms.WszystkieWarstwyDoPobrania()
            If warstwy.Count > 0 Then
                Dim nazwy = warstwy.Select(Function(x) If(Not String.IsNullOrWhiteSpace(x.Tytul), x.Tytul.Trim(), x.Nazwa.Trim())).
                                    Where(Function(s) s <> "").
                                    Distinct(StringComparer.OrdinalIgnoreCase).
                                    ToList()
                Dim przyklady = nazwy.Take(Math.Max(1, maxWarstw)).ToList()
                Dim tekstListy = String.Join(", ", przyklady)
                If nazwy.Count > przyklady.Count Then
                    tekstListy &= " (i jeszcze " & (nazwy.Count - przyklady.Count) & ")…"
                End If
                linie.Add("Warstwy na serwerze (" & warstwy.Count & "): " & tekstListy)
            End If
        ElseIf res.Typ = TypUslugiMapowej.Wmts AndAlso res.UslugaWmts IsNot Nothing Then
            Dim warstwy = res.UslugaWmts.Warstwy
            If warstwy.Count > 0 Then
                Dim nazwy = warstwy.Select(Function(x) If(Not String.IsNullOrWhiteSpace(x.Tytul), x.Tytul.Trim(), x.Identyfikator.Trim())).
                                    Where(Function(s) s <> "").
                                    Distinct(StringComparer.OrdinalIgnoreCase).
                                    ToList()
                Dim przyklady = nazwy.Take(Math.Max(1, maxWarstw)).ToList()
                Dim tekstListy = String.Join(", ", przyklady)
                If nazwy.Count > przyklady.Count Then
                    tekstListy &= " (i jeszcze " & (nazwy.Count - przyklady.Count) & ")…"
                End If
                linie.Add("Warstwy WMTS na serwerze (" & warstwy.Count & "): " & tekstListy)
            End If
        End If

        Return String.Join(vbCrLf, linie)
    End Function

    Private Shared Function FormatujSegmentAdresu(segment As String) As String
        Dim seg = If(segment, "").Trim()
        If seg.Length < 3 OrElse SegmentyAdresuPomijane.Contains(seg) Then Return ""
        'pomijamy identyfikatory UUID/GUID i techniczne kody org.2.<guid>_wms
        If Regex.IsMatch(seg, "[0-9a-f]{8}-[0-9a-f]{4}-", RegexOptions.IgnoreCase) OrElse
           seg.StartsWith("org.", StringComparison.OrdinalIgnoreCase) OrElse
           Regex.IsMatch(seg, "^\d+$") Then
            Return ""
        End If
        seg = Regex.Replace(seg, "^(bu_p_|wms_|wmts_|umwl_|bup_)", "", RegexOptions.IgnoreCase)
        If seg.Length < 3 OrElse SegmentyAdresuPomijane.Contains(seg) Then Return ""
        seg = seg.Replace("_"c, " "c).Replace("-"c, " "c)
        seg = Regex.Replace(seg, "([a-z])([A-Z])", "$1 $2")
        seg = Regex.Replace(seg, "\s+", " ").Trim()
        If seg.Length < 3 Then Return ""
        Return Char.ToUpper(seg(0)) & seg.Substring(1)
    End Function

    Private Shared Function ZawieraWszystkieSlowa(tekst As String, szukane As String) As Boolean
        Dim uTekst = FiltrWarstw.Uprosc(tekst)
        Dim slowa = FiltrWarstw.Slowa(szukane)
        Return slowa.Length > 0 AndAlso slowa.All(Function(s) uTekst.Contains(s))
    End Function

    Private Shared Function ZawieraFragment(tekst As String, fragment As String) As Boolean
        If String.IsNullOrWhiteSpace(tekst) OrElse String.IsNullOrWhiteSpace(fragment) Then Return False
        Return tekst.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function PolaczOpis(nowyPoczatek As String, istniejacy As String) As String
        If String.IsNullOrWhiteSpace(istniejacy) Then Return nowyPoczatek
        'usuwamy ogólnikowy szablon z Geoportalu, jeśli zastępujemy go pełniejszym opisem
        Dim oczyszczony = istniejacy.Replace("Oficjalna usługa z wykazu Geoportal.gov.pl.", "").
                                     Replace("Skorowidz / siatka arkuszy z wykazu Geoportal.gov.pl.", "").
                                     Trim().Trim("|"c).Trim()
        If oczyszczony = "" Then Return nowyPoczatek
        Return nowyPoczatek & " | " & oczyszczony
    End Function

    ''' <summary>Filtruje listę wpisów po frazie tekstowej (bez polskich znaków), kategorii i statusie obecności w programie.</summary>
    Public Shared Function Filtruj(wpisy As IEnumerable(Of WpisKataloguSerwerow),
                                   Optional fraza As String = "",
                                   Optional kategoria As String = "",
                                   Optional ukryjJuzDodane As Boolean = False,
                                   Optional ukryjSkorowidze As Boolean = False) As List(Of WpisKataloguSerwerow)
        If wpisy Is Nothing Then Return New List(Of WpisKataloguSerwerow)()
        Dim maKategorie = Not String.IsNullOrWhiteSpace(kategoria) AndAlso
                          Not String.Equals(kategoria.Trim(), "Wszystkie", StringComparison.OrdinalIgnoreCase)
        Dim slowa = FiltrWarstw.Slowa(fraza)

        Dim wynik As New List(Of WpisKataloguSerwerow)()
        For Each w In wpisy
            If ukryjJuzDodane AndAlso w.CzyJuzDodany Then Continue For
            If ukryjSkorowidze AndAlso w.CzySkorowidz Then Continue For
            If maKategorie AndAlso Not String.Equals(w.Kategoria, kategoria, StringComparison.OrdinalIgnoreCase) Then Continue For
            If slowa.Length > 0 Then
                Dim pelnyTekst = FiltrWarstw.Uprosc(w.Nazwa & " " & w.Dostawca & " " & w.Obszar & " " & w.Kategoria & " " & w.Opis & " " & w.Adres & " " & w.NazwaTypu & " " & w.Teryt)
                If Not slowa.All(Function(s) pelnyTekst.Contains(s)) Then Continue For
            End If
            wynik.Add(w)
        Next
        Return wynik
    End Function

    Private Shared Function CzyWygladaNaSkorowidz(nazwa As String, kategoria As String, adres As String) As Boolean
        Dim tekst = FiltrWarstw.Uprosc(nazwa & " " & kategoria & " " & adres)
        Return tekst.Contains("skorowidz") OrElse tekst.Contains("sheetsgrid")
    End Function

    Private Shared Function WyznaczKategorieEziudp(zbior As String, organ As String, obszar As String,
                                                   zrodlo As ZrodloKataloguSerwerow) As String
        Dim t = FiltrWarstw.Uprosc(zbior & " " & organ)
        If t.Contains("skorowidz") Then Return "Skorowidze map"
        If t.Contains("miejscow") OrElse t.Contains("plan") OrElse t.Contains("studium") OrElse
           t.Contains("zagospodarowan") OrElse t.Contains("mpzp") OrElse t.Contains("suikzp") Then
            Return "Zagospodarowanie przestrzenne"
        End If
        If t.Contains("ewidencj") OrElse t.Contains("egib") OrElse t.Contains("gesut") OrElse
           t.Contains("bdot500") OrElse t.Contains("osnow") OrElse t.Contains("cen nieruch") OrElse
           t.Contains("zasadnicz") Then
            Return "Geodezja i ewidencja"
        End If
        If t.Contains("powodz") OrElse t.Contains("wod") OrElse t.Contains("dorzecz") OrElse
           t.Contains("hydrograf") OrElse t.Contains("morsk") OrElse t.Contains("kapielisk") Then
            Return "Wody i zagrożenia"
        End If
        If t.Contains("geolog") OrElse t.Contains("osuwisk") OrElse t.Contains("jaskini") OrElse
           t.Contains("gornicz") OrElse t.Contains("kopalin") OrElse t.Contains("odwiert") OrElse
           t.Contains("sejsmik") OrElse t.Contains("hydrogeol") Then
            Return "Geologia i surowce"
        End If
        If t.Contains("ochron") OrElse t.Contains("srodowisk") OrElse t.Contains("natura") OrElse
           t.Contains("gatunk") OrElse t.Contains("siedlisk") OrElse t.Contains("las") OrElse
           t.Contains("halas") OrElse t.Contains("powietrz") OrElse t.Contains("seveso") Then
            Return "Przyroda i środowisko"
        End If
        If t.Contains("granic") OrElse t.Contains("adres") OrElse t.Contains("emuia") OrElse t.Contains("nazw geograf") Then
            Return "Granice i adresy"
        End If
        If t.Contains("statystycz") OrElse t.Contains("ludnosci") OrElse t.Contains("gus") Then
            Return "Statystyka i demografia"
        End If
        If t.Contains("orto") Then Return "Ortofotomapy"
        If t.Contains("bdot10k") OrElse t.Contains("bdoo") OrElse t.Contains("topograf") Then Return "Mapy topograficzne"
        If t.Contains("zabytk") OrElse t.Contains("archeolog") OrElse t.Contains("dziedzictw") Then Return "Mapy historyczne i zabytki"
        If zrodlo = ZrodloKataloguSerwerow.EziudpLokalne Then Return "Usługi samorządowe"
        Return "Inne usługi publiczne"
    End Function

    Private Shared Function DopiszDoOpisu(istniejacyOpis As String, kolejnyZbior As String) As String
        If String.IsNullOrWhiteSpace(kolejnyZbior) Then Return istniejacyOpis
        If String.IsNullOrWhiteSpace(istniejacyOpis) Then Return "Inne zbiory w tej usłudze: " & kolejnyZbior
        If istniejacyOpis.IndexOf("Inne zbiory w tej usłudze:", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return istniejacyOpis & ", " & kolejnyZbior
        End If
        Return istniejacyOpis & " | Inne zbiory: " & kolejnyZbior
    End Function

    Private Shared Function OczyscHtml(fragment As String) As String
        If String.IsNullOrEmpty(fragment) Then Return ""
        Dim bezTagow = Regex.Replace(fragment, "<[^>]+>", " ")
        Return Regex.Replace(WebUtility.HtmlDecode(bezTagow), "\s+", " ").Trim()
    End Function

    Private Shared Function TekstJson(obj As JsonElement, nazwa As String) As String
        Dim el As JsonElement
        If obj.TryGetProperty(nazwa, el) AndAlso el.ValueKind = JsonValueKind.String Then
            Return If(el.GetString(), "").Trim()
        End If
        Return ""
    End Function

    Private Shared Function LogicznaJson(obj As JsonElement, nazwa As String, domyslna As Boolean) As Boolean
        Dim el As JsonElement
        If obj.TryGetProperty(nazwa, el) Then
            If el.ValueKind = JsonValueKind.True Then Return True
            If el.ValueKind = JsonValueKind.False Then Return False
        End If
        Return domyslna
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

End Class

''' <summary>Asynchroniczny klient pobierający spisy usług z Geoportal.gov.pl i API EZiUDP (z pamięcią podręczną).</summary>
Public NotInheritable Class KlientKataloguSerwerow

    Public Const UrlGeoportalWpJson As String = "https://www.geoportal.gov.pl/wp-json/wp/v2/pages/107"
    Public Const UrlGeoportalHtml As String = "https://www.geoportal.gov.pl/pl/usluga/uslugi-przegladania-wms-i-wmts/"
    Public Const UrlEziudpApi As String = "https://integracja.gugik.gov.pl/eziudp/api.php"

    Private Sub New()
    End Sub

    ''' <summary>Pobiera wykaz usług przeglądania Geoportal.gov.pl (z lokalnym buforem ważnym maxWiekGodzin).</summary>
    Public Shared Async Function PobierzGeoportalAsync(Optional sciezkaCache As String = Nothing,
                                                      Optional wymusOdswiezenie As Boolean = False,
                                                      Optional maxWiekGodzin As Integer = 24,
                                                      Optional timeoutSekundy As Integer = 20,
                                                      Optional token As CancellationToken = Nothing,
                                                      Optional handler As HttpMessageHandler = Nothing) As Task(Of WynikPobraniaKatalogu)
        If Not wymusOdswiezenie Then
            Dim zCache = SprobujZCache(sciezkaCache, maxWiekGodzin, ZrodloKataloguSerwerow.Geoportal)
            If zCache IsNot Nothing Then Return zCache
        End If

        Dim wynik As New WynikPobraniaKatalogu()
        Try
            Dim wpJson = Await PobierzTekstAsync(UrlGeoportalWpJson, timeoutSekundy, token, handler).ConfigureAwait(False)
            Dim wpisy = KatalogSerwerow.ParsujGeoportalWpJson(wpJson)
            If wpisy.Count > 0 Then
                wynik.Sukces = True
                wynik.Wpisy.AddRange(wpisy)
                ZapiszDoCache(sciezkaCache, wpisy)
                Return wynik
            End If
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch
            'próba zapasowa ze strony HTML
        End Try

        Try
            Dim html = Await PobierzTekstAsync(UrlGeoportalHtml, timeoutSekundy, token, handler).ConfigureAwait(False)
            Dim wpisy = KatalogSerwerow.ParsujGeoportalHtml(html)
            If wpisy.Count > 0 Then
                wynik.Sukces = True
                wynik.Wpisy.AddRange(wpisy)
                ZapiszDoCache(sciezkaCache, wpisy)
                Return wynik
            End If
            wynik.KomunikatBledu = "Nie znaleziono usług na stronie Geoportal.gov.pl."
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            wynik.KomunikatBledu = "Nie udało się pobrać wykazu Geoportal.gov.pl: " & MetadaneMapy.MaskujSekrety(ex.Message)
        End Try
        Dim staryCache = SprobujZCache(sciezkaCache, maxWiekGodzin, ZrodloKataloguSerwerow.Geoportal, pozwolNaPrzeterminowany:=True)
        If staryCache IsNot Nothing Then
            staryCache.KomunikatBledu = wynik.KomunikatBledu
            Return staryCache
        End If
        Return wynik
    End Function

    ''' <summary>Pobiera usługi centralne i wojewódzkie z oficjalnego API EZiUDP GUGiK.</summary>
    Public Shared Async Function PobierzEziudpCentralneIWojewodzkieAsync(Optional sciezkaCache As String = Nothing,
                                                                        Optional wymusOdswiezenie As Boolean = False,
                                                                        Optional maxWiekGodzin As Integer = 24,
                                                                        Optional timeoutSekundy As Integer = 25,
                                                                        Optional token As CancellationToken = Nothing,
                                                                        Optional handler As HttpMessageHandler = Nothing) As Task(Of WynikPobraniaKatalogu)
        If Not wymusOdswiezenie Then
            Dim zCache = SprobujZCache(sciezkaCache, maxWiekGodzin, ZrodloKataloguSerwerow.EziudpCentralne)
            If zCache IsNot Nothing Then Return zCache
        End If

        Dim wynik As New WynikPobraniaKatalogu()
        Try
            Dim urlCentr = UrlEziudpApi & "?rodzaj=centralne&usluga=przegladania&result=idiip,teryt,zbior,organ,obszar,tematy,uslugi"
            Dim urlWoj = UrlEziudpApi & "?rodzaj=wojewodztwa&usluga=przegladania&result=idiip,teryt,zbior,organ,obszar,tematy,uslugi"

            Dim tCentr = PobierzTekstAsync(urlCentr, timeoutSekundy, token, handler)
            Dim tWoj = PobierzTekstAsync(urlWoj, timeoutSekundy, token, handler)
            Await Task.WhenAll(tCentr, tWoj).ConfigureAwait(False)

            Dim lista As New List(Of WpisKataloguSerwerow)()
            lista.AddRange(KatalogSerwerow.ParsujEziudpJson(tCentr.Result, ZrodloKataloguSerwerow.EziudpCentralne))
            lista.AddRange(KatalogSerwerow.ParsujEziudpJson(tWoj.Result, ZrodloKataloguSerwerow.EziudpCentralne))

            'usuwamy powtórzenia jeśli ta sama usługa była w centralnych i wojewódzkich
            Dim unikalne As New List(Of WpisKataloguSerwerow)()
            Dim widziane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each w In lista
                Dim k = KatalogSerwerow.NormalizujAdresDoPorownania(w.Adres)
                If k <> "" AndAlso widziane.Add(k) Then unikalne.Add(w)
            Next

            wynik.Sukces = True
            wynik.Wpisy.AddRange(unikalne)
            ZapiszDoCache(sciezkaCache, unikalne)
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            wynik.KomunikatBledu = "Nie udało się pobrać rejestru EZiUDP: " & MetadaneMapy.MaskujSekrety(ex.Message)
            Dim staryCache = SprobujZCache(sciezkaCache, maxWiekGodzin, ZrodloKataloguSerwerow.EziudpCentralne, pozwolNaPrzeterminowany:=True)
            If staryCache IsNot Nothing Then
                staryCache.KomunikatBledu = wynik.KomunikatBledu
                Return staryCache
            End If
        End Try
        Return wynik
    End Function

    ''' <summary>
    ''' Wyszukuje lokalne usługi powiatowe i gminne w EZiUDP po nazwie miejscowości/organu lub kodzie TERYT.
    ''' Wykorzystuje lokalny cache spisu powiatów (~700 KB), dzięki czemu dopasowuje też formy przymiotnikowe
    ''' i miasta na prawach powiatu (np. "Łódź", "Wieliczka") oraz dopytuje API gmin także po wykrytym kodzie TERYT.
    ''' </summary>
    Public Shared Async Function SzukajEziudpLokalneAsync(fraza As String,
                                                          Optional zakres As String = "oba",
                                                          Optional sciezkaCachePowiatow As String = Nothing,
                                                          Optional wymusOdswiezeniePowiatow As Boolean = False,
                                                          Optional timeoutSekundy As Integer = 25,
                                                          Optional token As CancellationToken = Nothing,
                                                          Optional handler As HttpMessageHandler = Nothing) As Task(Of WynikPobraniaKatalogu)
        Dim wynik As New WynikPobraniaKatalogu()
        Dim przycieta = If(fraza, "").Trim()
        If przycieta.Length < 2 Then
            wynik.KomunikatBledu = "Wpisz co najmniej 2 znaki nazwy gminy, powiatu albo kodu TERYT (np. Kraków, Zielonki, 1206)."
            Return wynik
        End If

        Dim tryb = If(zakres, "oba").Trim().ToLowerInvariant()
        Dim szukajPowiaty = (tryb = "oba" OrElse tryb = "powiaty")
        Dim szukajGminy = (tryb = "oba" OrElse tryb = "gminy")

        Dim tylkoCyfry = Regex.IsMatch(przycieta, "^\d{2,7}$")
        Dim slowoGlowne = WybierzSlowoDoZapytaniaEziudp(przycieta)
        Dim frazaNazwy = FrazaBezSkrotowZbiorow(przycieta)

        Try
            Dim polaczone As New List(Of WpisKataloguSerwerow)()
            Dim kodyTerytZPowiatow As New List(Of String)()

            'spis powiatów i początkowe zapytanie o gminy uruchamiamy równolegle (gdy brak świeżego cache powiatów)
            Dim zadaniePowiaty As Task(Of List(Of WpisKataloguSerwerow)) = Nothing
            If szukajPowiaty OrElse (szukajGminy AndAlso Not tylkoCyfry) Then
                zadaniePowiaty = PobierzLubWczytajPowiatyAsync(sciezkaCachePowiatow, wymusOdswiezeniePowiatow, timeoutSekundy, token, handler)
            End If

            Dim zadanieGminyPoczatkowe As Task(Of List(Of WpisKataloguSerwerow)) = Nothing
            Dim zapytanieNazwaPoczatkowe As String = ""
            If szukajGminy Then
                If tylkoCyfry Then
                    zadanieGminyPoczatkowe = PobierzGminyEziudpAsync("teryt=" & Uri.EscapeDataString(przycieta), timeoutSekundy, token, handler)
                Else
                    zapytanieNazwaPoczatkowe = If(frazaNazwy <> "", frazaNazwy, slowoGlowne)
                    zadanieGminyPoczatkowe = PobierzGminyEziudpAsync("nazwa=" & Uri.EscapeDataString(zapytanieNazwaPoczatkowe), timeoutSekundy, token, handler)
                End If
                Dim obslugaBledu = zadanieGminyPoczatkowe.ContinueWith(Sub(t)
                                                                           Dim ignorowanyBlad = t.Exception
                                                                       End Sub, TaskContinuationOptions.OnlyOnFaulted)
            End If

            Dim powiaty As List(Of WpisKataloguSerwerow) = Nothing
            If zadaniePowiaty IsNot Nothing Then
                Try
                    powiaty = Await zadaniePowiaty.ConfigureAwait(False)
                Catch ex As OperationCanceledException When token.IsCancellationRequested
                    Throw
                Catch
                    If szukajPowiaty Then Throw
                End Try
            End If

            Dim frazaRdzeniowaPowiaty As String = Nothing
            If powiaty IsNot Nothing Then
                If szukajPowiaty Then
                    Dim dopasowanePowiaty = KatalogSerwerow.Filtruj(powiaty, przycieta)
                    If dopasowanePowiaty.Count = 0 AndAlso Not tylkoCyfry AndAlso slowoGlowne.Length >= 6 Then
                        frazaRdzeniowaPowiaty = ZastapSlowoRdzeniem(przycieta, slowoGlowne, 5)
                        dopasowanePowiaty = KatalogSerwerow.Filtruj(powiaty, frazaRdzeniowaPowiaty)
                    End If
                    polaczone.AddRange(dopasowanePowiaty)
                End If

                'kody TERYT rozpoznajemy po samej nazwie miejscowości/powiatu (bez słów typu MPZP/POG, które nie występują w powiatach)
                If szukajGminy AndAlso Not tylkoCyfry Then
                    Dim frazaTeryt = If(frazaNazwy <> "", frazaNazwy, przycieta)
                    Dim pasujaceDoTeryt = KatalogSerwerow.Filtruj(powiaty, frazaTeryt)
                    If pasujaceDoTeryt.Count = 0 AndAlso slowoGlowne.Length >= 6 Then
                        pasujaceDoTeryt = KatalogSerwerow.Filtruj(powiaty, ZastapSlowoRdzeniem(frazaTeryt, slowoGlowne, 5))
                    End If
                    kodyTerytZPowiatow = pasujaceDoTeryt.
                        Select(Function(p) If(p.Teryt, "").Trim()).
                        Where(Function(t) t.Length >= 4).
                        Select(Function(t) t.Substring(0, 4)).
                        Distinct(StringComparer.OrdinalIgnoreCase).
                        ToList()
                End If
            End If

            If szukajGminy Then
                Dim gminy As List(Of WpisKataloguSerwerow) = Await zadanieGminyPoczatkowe.ConfigureAwait(False)
                If Not tylkoCyfry Then
                    'jeśli fraza wielowyrazowa nie dała wyniku, próba po najdłuższym słowie oraz po rdzeniu
                    If gminy.Count = 0 AndAlso slowoGlowne <> "" AndAlso Not String.Equals(zapytanieNazwaPoczatkowe, slowoGlowne, StringComparison.OrdinalIgnoreCase) Then
                        gminy = Await PobierzGminyEziudpAsync("nazwa=" & Uri.EscapeDataString(slowoGlowne), timeoutSekundy, token, handler).ConfigureAwait(False)
                    End If
                    If gminy.Count = 0 AndAlso slowoGlowne.Length >= 6 Then
                        Dim rdzen = slowoGlowne.Substring(0, slowoGlowne.Length - 2)
                        gminy = Await PobierzGminyEziudpAsync("nazwa=" & Uri.EscapeDataString(rdzen), timeoutSekundy, token, handler).ConfigureAwait(False)
                    End If

                    'dla miast na prawach powiatu (np. Łódź, Bielsko-Biała) API po organie ("Łodzi") nie zwraca wyników,
                    'ale 4-cyfrowy TERYT z listy powiatów odnajduje wszystkie usługi gminne miasta
                    If kodyTerytZPowiatow.Count > 0 AndAlso kodyTerytZPowiatow.Count <= 3 Then
                        Dim zadaniaTeryt = kodyTerytZPowiatow.Select(Function(kod) PobierzGminyEziudpAsync("teryt=" & Uri.EscapeDataString(kod), timeoutSekundy, token, handler)).ToList()
                        Dim wynikiTeryt = Await Task.WhenAll(zadaniaTeryt).ConfigureAwait(False)
                        For Each listaT In wynikiTeryt
                            gminy.AddRange(listaT)
                        Next
                    End If
                End If

                Dim przefiltrowaneGminy = KatalogSerwerow.Filtruj(gminy, przycieta)
                If przefiltrowaneGminy.Count = 0 AndAlso Not tylkoCyfry AndAlso slowoGlowne.Length >= 6 Then
                    Dim frazaZRdzeniem = If(frazaRdzeniowaPowiaty, ZastapSlowoRdzeniem(przycieta, slowoGlowne, Math.Min(5, slowoGlowne.Length - 2)))
                    przefiltrowaneGminy = KatalogSerwerow.Filtruj(gminy, frazaZRdzeniem)
                End If
                polaczone.AddRange(przefiltrowaneGminy)
            End If

            'deduplikacja końcowa między powiatami i gminami
            Dim widziane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each w In polaczone
                Dim k = KatalogSerwerow.NormalizujAdresDoPorownania(w.Adres)
                If k <> "" AndAlso widziane.Add(k) Then wynik.Wpisy.Add(w)
            Next
            wynik.Sukces = True
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            wynik.KomunikatBledu = "Błąd wyszukiwania w EZiUDP: " & MetadaneMapy.MaskujSekrety(ex.Message)
        End Try
        Return wynik
    End Function

    Private Shared Async Function PobierzGminyEziudpAsync(parametrZapytania As String, timeoutSekundy As Integer,
                                                          token As CancellationToken, handler As HttpMessageHandler) As Task(Of List(Of WpisKataloguSerwerow))
        Dim url = UrlEziudpApi & "?rodzaj=gminy&usluga=przegladania&" & parametrZapytania & "&result=idiip,teryt,zbior,organ,obszar,tematy,uslugi&count=200"
        Dim json = Await PobierzTekstAsync(url, timeoutSekundy, token, handler).ConfigureAwait(False)
        Return KatalogSerwerow.ParsujEziudpJson(json, ZrodloKataloguSerwerow.EziudpLokalne)
    End Function

    Private Shared Async Function PobierzLubWczytajPowiatyAsync(sciezkaCache As String,
                                                               wymusOdswiezenie As Boolean,
                                                               timeoutSekundy As Integer,
                                                               token As CancellationToken,
                                                               handler As HttpMessageHandler) As Task(Of List(Of WpisKataloguSerwerow))
        If Not wymusOdswiezenie Then
            Dim zCache = SprobujZCache(sciezkaCache, maxWiekGodzin:=48, ZrodloKataloguSerwerow.EziudpLokalne)
            If zCache IsNot Nothing Then Return zCache.Wpisy
        End If
        Try
            Dim url = UrlEziudpApi & "?rodzaj=powiaty&usluga=przegladania&result=idiip,teryt,zbior,organ,obszar,tematy,uslugi"
            Dim json = Await PobierzTekstAsync(url, timeoutSekundy, token, handler).ConfigureAwait(False)
            Dim wpisy = KatalogSerwerow.ParsujEziudpJson(json, ZrodloKataloguSerwerow.EziudpLokalne)
            ZapiszDoCache(sciezkaCache, wpisy)
            Return wpisy
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch
            Dim staryCache = SprobujZCache(sciezkaCache, maxWiekGodzin:=48, ZrodloKataloguSerwerow.EziudpLokalne, pozwolNaPrzeterminowany:=True)
            If staryCache IsNot Nothing Then Return staryCache.Wpisy
            Throw
        End Try
    End Function

    Private Shared ReadOnly SkrotyZbiorowPomijane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "gmina", "miasto", "powiat", "mpzp", "pog", "egib", "bdot", "bdot500", "gesut", "suikzp", "emuia", "rcn", "bdsog", "wms", "wmts"
    }

    ''' <summary>Zwraca frazę oczyszczoną ze skrótów typów zbiorów (np. "Nowy Sącz MPZP" -> "Nowy Sącz").</summary>
    Private Shared Function FrazaBezSkrotowZbiorow(fraza As String) As String
        Dim oryginalne = If(fraza, "").Trim().Split({" "c, ","c, ";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim pozostale = oryginalne.Where(Function(s) s.Length >= 2 AndAlso Not SkrotyZbiorowPomijane.Contains(FiltrWarstw.Uprosc(s))).ToList()
        Return String.Join(" ", pozostale)
    End Function

    Private Shared Function ZastapSlowoRdzeniem(calaFraza As String, slowo As String, dlugoscRdzenia As Integer) As String
        If String.IsNullOrEmpty(slowo) OrElse dlugoscRdzenia <= 0 OrElse dlugoscRdzenia >= slowo.Length Then Return calaFraza
        Dim rdzen = FiltrWarstw.Uprosc(slowo).Substring(0, dlugoscRdzenia)
        Dim czesci = FiltrWarstw.Slowa(calaFraza).Select(Function(s) If(String.Equals(s, FiltrWarstw.Uprosc(slowo), StringComparison.OrdinalIgnoreCase), rdzen, s))
        Return String.Join(" ", czesci)
    End Function

    ''' <summary>Wybiera najistotniejsze słowo (nazwę miejscowości/gminy) pomijając skróty typów zbiorów (MPZP, POG, EGiB).</summary>
    Private Shared Function WybierzSlowoDoZapytaniaEziudp(fraza As String) As String
        Dim oryginalne = If(fraza, "").Trim().Split({" "c, ","c, ";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim kandydaci = oryginalne.Where(Function(s) s.Length >= 2 AndAlso Not SkrotyZbiorowPomijane.Contains(FiltrWarstw.Uprosc(s))).ToList()
        If kandydaci.Count > 0 Then
            Return kandydaci.OrderByDescending(Function(s) s.Length).First()
        End If
        Return If(oryginalne.Length > 0, oryginalne(0), fraza.Trim())
    End Function

    Private Shared Function SprobujZCache(sciezka As String, maxWiekGodzin As Integer, zrodlo As ZrodloKataloguSerwerow,
                                          Optional pozwolNaPrzeterminowany As Boolean = False) As WynikPobraniaKatalogu
        If String.IsNullOrWhiteSpace(sciezka) OrElse Not File.Exists(sciezka) Then Return Nothing
        Try
            Dim wiek = DateTime.UtcNow - File.GetLastWriteTimeUtc(sciezka)
            Dim przeterminowany = wiek > TimeSpan.FromHours(Math.Max(1, maxWiekGodzin))
            If przeterminowany AndAlso Not pozwolNaPrzeterminowany Then Return Nothing
            Dim wpisy = KatalogSerwerow.ZJsona(File.ReadAllText(sciezka, Encoding.UTF8), zrodlo)
            If wpisy.Count = 0 Then Return Nothing
            Dim w As New WynikPobraniaKatalogu With {.Sukces = True, .ZCache = True, .ZCacheStary = przeterminowany}
            w.Wpisy.AddRange(wpisy)
            Return w
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Sub ZapiszDoCache(sciezka As String, wpisy As IEnumerable(Of WpisKataloguSerwerow))
        If String.IsNullOrWhiteSpace(sciezka) Then Exit Sub
        Try
            Dim dir = Path.GetDirectoryName(sciezka)
            If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)
            File.WriteAllText(sciezka, KatalogSerwerow.DoJsona(wpisy), Encoding.UTF8)
        Catch
        End Try
    End Sub

    Private Shared Async Function PobierzTekstAsync(url As String, timeoutSekundy As Integer,
                                                    token As CancellationToken, handler As HttpMessageHandler) As Task(Of String)
        Dim wlasnyHandler As Boolean = handler Is Nothing
        Dim obsluga As HttpMessageHandler = If(wlasnyHandler, New HttpClientHandler() With {
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }, handler)

        Using ctsTimeout As New CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, timeoutSekundy))),
              ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(token, ctsTimeout.Token),
              klient As New HttpClient(obsluga, disposeHandler:=wlasnyHandler)
            klient.DefaultRequestHeaders.UserAgent.ParseAdd(WersjaProgramu.UserAgent(GetType(KlientKataloguSerwerow).Assembly))
            Try
                Using odp = Await klient.GetAsync(url, HttpCompletionOption.ResponseContentRead, ctsLinked.Token).ConfigureAwait(False)
                    odp.EnsureSuccessStatusCode()
                    Dim bajty = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                    Dim kodowanie As Encoding = Encoding.UTF8
                    Dim charset = odp.Content.Headers?.ContentType?.CharSet
                    If Not String.IsNullOrWhiteSpace(charset) Then
                        Try
                            kodowanie = Encoding.GetEncoding(charset.Trim(""""c, " "c))
                        Catch
                        End Try
                    End If
                    Return kodowanie.GetString(bajty)
                End Using
            Catch ex As OperationCanceledException When Not token.IsCancellationRequested
                Throw New TimeoutException("Serwer katalogu nie odpowiedział w ciągu " & Math.Max(5, timeoutSekundy) & " s.", ex)
            End Try
        End Using
    End Function

End Class
