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

Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core
Imports Xunit

Public Class KatalogSerwerowTesty

    Public Sub New()
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    End Sub

    Private NotInheritable Class MockHttpHandler
        Inherits HttpMessageHandler

        Private ReadOnly _odpowiedz As Func(Of Uri, HttpResponseMessage)
        Public ReadOnly Property OdpytywaneAdresy As New List(Of String)

        Public Sub New(odpowiedz As Func(Of Uri, HttpResponseMessage))
            _odpowiedz = odpowiedz
        End Sub

        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            SyncLock OdpytywaneAdresy
                OdpytywaneAdresy.Add(request.RequestUri.AbsoluteUri)
            End SyncLock
            Return Task.FromResult(_odpowiedz(request.RequestUri))
        End Function
    End Class

    <Fact>
    Public Sub WbudowanyKatalog_PoprawneWpisyBezDuplikatowIProgramowychZbiorow()
        Dim wpisy = KatalogSerwerow.WczytajWbudowany()
        Assert.True(wpisy.Count >= 40, "Oczekiwano co najmniej 40 usług we wbudowanym katalogu.")

        'wszystkie wpisy muszą mieć nazwę, kategorię, typ, poprawny adres HTTP/HTTPS i nie zawierać skorowidzów ani Bing/Google
        For Each w In wpisy
            Assert.False(String.IsNullOrWhiteSpace(w.Nazwa))
            Assert.False(String.IsNullOrWhiteSpace(w.Kategoria))
            Assert.NotEqual(TypUslugiMapowej.Nieznana, w.Typ)
            Assert.False(w.CzySkorowidz, "Wbudowany katalog nie powinien zawierać skorowidzów: " & w.Nazwa)
            Assert.DoesNotContain("virtualearth.net", w.Adres, StringComparison.OrdinalIgnoreCase)
            Assert.DoesNotContain("google.com", w.Adres, StringComparison.OrdinalIgnoreCase)
            Assert.True(w.Adres.StartsWith("https://", StringComparison.OrdinalIgnoreCase) OrElse
                        w.Adres.StartsWith("http://", StringComparison.OrdinalIgnoreCase), w.Nazwa)
        Next

        'brak powtórzonych adresów w samym katalogu wbudowanym
        Dim klucze = wpisy.Select(Function(w) KatalogSerwerow.NormalizujAdresDoPorownania(w.Adres)).ToList()
        Assert.Equal(klucze.Count, klucze.Distinct(StringComparer.OrdinalIgnoreCase).Count())

        'DopasujIstniejace powinno poprawnie identyfikować serwery, które są już w programie
        Dim folder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "warstwy")
        Dim istniejace = File.ReadAllLines(Path.Combine(folder, "warstwy.txt")).
            Select(Function(l) l.Trim().Trim(""""c)).
            Where(Function(l) l <> "").
            Select(Function(n) New KeyValuePair(Of String, String)(n, PlikWarstw.Wczytaj(Path.Combine(folder, n & ".txt")).Adres)).
            ToList()

        KatalogSerwerow.DopasujIstniejace(wpisy, istniejace)
        Dim dopasowane = wpisy.Where(Function(w) w.CzyJuzDodany).ToList()
        'Wbudowany katalog zawiera bogate serwery wielowarstwowe (np. PIG SMGP, GDOŚ, BDL),
        'z których w MapoTero domyślnie jest tylko pojedyncza warstwa — weryfikujemy ich poprawne oznaczenie
        Assert.NotEmpty(dopasowane)
        Assert.Contains(dopasowane, Function(w) w.JuzDodanyZbior = "geologiczna_50_SMGP")
    End Sub

    <Fact>
    Public Sub NormalizacjaAdresuIOznaczanieJuzDodanychZbiorow()
        'http vs https, końcowy ukośnik oraz parametry GetMap vs GetCapabilities muszą dawać ten sam klucz
        Dim a1 = KatalogSerwerow.NormalizujAdresDoPorownania("WMS=http://sdi.gdos.gov.pl/wms/")
        Dim a2 = KatalogSerwerow.NormalizujAdresDoPorownania("https://sdi.gdos.gov.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=")
        Assert.Equal(a1, a2)

        'parametr niestandardowy (map=, token=) musi odróżniać różne usługi na tym samym hoście
        Dim m1 = KatalogSerwerow.NormalizujAdresDoPorownania("https://serwer.pl/cgi-bin/mapserv?map=/a.map&service=WMS")
        Dim m2 = KatalogSerwerow.NormalizujAdresDoPorownania("https://serwer.pl/cgi-bin/mapserv?map=/b.map")
        Assert.NotEqual(m1, m2)

        'szablony XYZ {a,b,c} i {s} są równoważne
        Dim x1 = KatalogSerwerow.NormalizujAdresDoPorownania("https://{a,b,c}.tile.example/{z}/{x}/{y}.png")
        Dim x2 = KatalogSerwerow.NormalizujAdresDoPorownania("https://{s}.tile.example/{z}/{x}/{y}.png")
        Assert.Equal(x1, x2)

        'naprawa podwójnego "?" z EZiUDP oraz odfiltrowanie WFS=
        Dim typ As TypUslugiMapowej = TypUslugiMapowej.Nieznana
        Assert.Equal("https://wms.example/mapserv?map=/a.map&service=WMS&request=GetCapabilities",
                     KatalogSerwerow.OczyscAdresUslugi("WMS=https://wms.example/mapserv?map=/a.map?service=WMS&request=GetCapabilities", typ))
        Assert.Equal(TypUslugiMapowej.Wms, typ)
        Assert.Equal("", KatalogSerwerow.OczyscAdresUslugi("WFS=https://wfs.example/ows", typ))
    End Sub

    <Fact>
    Public Sub ParsujEziudpJson_CzysciPrefiksyDeduplikujeIKlasyfikuje()
        Dim json = "{
          ""data"": [
            {
              ""idiip"": ""PL.ZIPOP.1393"",
              ""teryt"": """",
              ""zbior"": ""Centralny Rejestr Form Ochrony Przyrody"",
              ""organ"": ""Generalny Dyrektor Ochrony Środowiska"",
              ""obszar"": ""Polska"",
              ""tematy"": [""Obszary chronione""],
              ""uslugi"": {
                ""przegladania"": [
                  ""WMS=http://sdi.gdos.gov.pl/wms"",
                  ""WFS=https://sdi.gdos.gov.pl/wfs""
                ]
              }
            },
            {
              ""idiip"": ""PL.ZIPOP.9999"",
              ""teryt"": """",
              ""zbior"": ""Inwazyjne Gatunki Obce"",
              ""organ"": ""Generalny Dyrektor Ochrony Środowiska"",
              ""obszar"": ""Polska"",
              ""tematy"": [""Siedliska""],
              ""uslugi"": {
                ""przegladania"": [""https://sdi.gdos.gov.pl/wms""]
              }
            },
            {
              ""idiip"": ""PL.PZGiK.202"",
              ""teryt"": """",
              ""zbior"": ""Baza BDOT10k"",
              ""organ"": ""Główny Geodeta Kraju"",
              ""obszar"": ""Polska"",
              ""tematy"": [],
              ""uslugi"": {
                ""przegladania"": [""WMTS=https://mapy.geoportal.gov.pl/wss/service/WMTS/guest/wmts/BDOT10k""]
              }
            }
          ]
        }"

        Dim wpisy = KatalogSerwerow.ParsujEziudpJson(json, ZrodloKataloguSerwerow.EziudpCentralne)
        'dwa wpisy pod sdi.gdos.gov.pl/wms scalone w jeden (z preferencją HTTPS) + jeden WMTS; WFS odrzucony
        Assert.Equal(2, wpisy.Count)
        Assert.Equal("https://sdi.gdos.gov.pl/wms", wpisy(0).Adres)
        Assert.Contains("Inwazyjne Gatunki Obce", wpisy(0).Opis)
        Assert.Equal("Przyroda i środowisko", wpisy(0).Kategoria)
        Assert.Equal(TypUslugiMapowej.Wmts, wpisy(1).Typ)
    End Sub

    <Fact>
    Public Sub ParsujGeoportalHtml_ZachowujeSkorowidzeIPomijaImapnext()
        Dim html = "
          <h2>Ortofotomapa</h2>
          <table>
            <tr><th>Rodzaj</th><th>Nazwa</th><th>Mapa</th><th>Adres</th></tr>
            <tr>
              <td><img src='/WMTS_thumb.png' alt='WMTS'></td>
              <td>Ortofotomapa standardowa</td>
              <td><a href='https://mapy.geoportal.gov.pl/imapnext/imap/index.html?resources=map:wmts@https://x'>Pokaż</a></td>
              <td><a href='https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution'>Kopiuj</a></td>
            </tr>
          </table>
          <h2>Skorowidze map</h2>
          <table>
            <tr>
              <td><img src='/WMS_thumb.png' alt='WMS'></td>
              <td>Ortofotomapa według aktualności</td>
              <td></td>
              <td><a href='https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/SkorowidzeWgAktualnosci'>Kopiuj</a></td>
            </tr>
          </table>"

        Dim wpisy = KatalogSerwerow.ParsujGeoportalHtml(html)
        Assert.Equal(2, wpisy.Count)
        Assert.Equal(TypUslugiMapowej.Wmts, wpisy(0).Typ)
        Assert.Equal("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution", wpisy(0).Adres)
        Assert.False(wpisy(0).CzySkorowidz)

        'skorowidze są zachowane (decyzja autora) i oznaczone flagą CzySkorowidz
        Assert.True(wpisy(1).CzySkorowidz)
        Assert.Equal("Skorowidze map", wpisy(1).Kategoria)
    End Sub

    <Fact>
    Public Sub Filtruj_IgnorujePolskieZnakiIUkrywaJuzDodaneGdyWlaczono()
        Dim lista As New List(Of WpisKataloguSerwerow) From {
            New WpisKataloguSerwerow With {.Nazwa = "Mapy zagrożenia powodziowego", .Dostawca = "Wody Polskie", .Kategoria = "Wody i zagrożenia", .Adres = "https://a.pl/wms", .JuzDodanyZbior = ""},
            New WpisKataloguSerwerow With {.Nazwa = "Działki ewidencyjne", .Dostawca = "GUGiK", .Kategoria = "Geodezja i ewidencja", .Adres = "https://b.pl/wms", .JuzDodanyZbior = "dzialki_ewid_EGIB"}
        }

        Assert.Single(KatalogSerwerow.Filtruj(lista, "zagrozenia wod"))
        Assert.Single(KatalogSerwerow.Filtruj(lista, "", "Wszystkie", ukryjJuzDodane:=True))
        Assert.Equal(2, KatalogSerwerow.Filtruj(lista, "", "Wszystkie", ukryjJuzDodane:=False).Count)
    End Sub

    <Fact>
    Public Async Function KlientKatalogu_GeoportalFallbackHtmlILokalneEziudp() As Task
        Dim htmlFallback = "<h2>Dane adresowe</h2><table><tr><td>WMS</td><td>KINA Adresy</td><td><a href='https://mapy.geoportal.gov.pl/wss/ext/KINA'>Link</a></td></tr></table>"
        Using hGp As New MockHttpHandler(
            Function(uri)
                If uri.AbsoluteUri.Contains("wp-json") Then
                    Return New HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                End If
                Return New HttpResponseMessage(HttpStatusCode.OK) With {
                    .Content = New StringContent(htmlFallback, Encoding.UTF8, "text/html")
                }
            End Function)
            Dim res = Await KlientKataloguSerwerow.PobierzGeoportalAsync(handler:=hGp)
            Assert.True(res.Sukces, res.KomunikatBledu)
            Assert.Single(res.Wpisy)
            Assert.Equal("KINA Adresy", res.Wpisy(0).Nazwa)
        End Using

        'wyszukiwanie lokalne powiaty (np. Wieliczka -> powiat wielicki, Łódź -> 1061) + gminy (np. Zielonki, Nowy Sącz)
        Dim powiatyJson = "{""data"":[{""idiip"":""PL.PZGiK.1"",""teryt"":""1219"",""zbior"":""EGiB"",""organ"":""Starosta Wielicki"",""obszar"":""powiat wielicki"",""tematy"":[],""uslugi"":{""przegladania"":[""https://wms.wielicki.pl/egib""]}},{""idiip"":""PL.PZGiK.2"",""teryt"":""1219"",""zbior"":""GESUT"",""organ"":""Starosta Wielicki"",""obszar"":""powiat wielicki"",""tematy"":[],""uslugi"":{""przegladania"":[""https://wms.wielicki.pl/gesut""]}},{""idiip"":""PL.PZGiK.3"",""teryt"":""1061"",""zbior"":""EGiB"",""organ"":""Prezydent Miasta Łodzi"",""obszar"":""miasto Łódź"",""tematy"":[],""uslugi"":{""przegladania"":[""https://wms.lodz.pl/egib""]}}]}"
        Dim gminyZielonkiJson = "{""data"":[{""idiip"":""PL.ZIPPZP.2"",""teryt"":""120617_2"",""zbior"":""MPZP"",""organ"":""Wójt Gminy Zielonki"",""obszar"":""gmina Zielonki"",""tematy"":[],""uslugi"":{""przegladania"":[""https://mpzp.zielonki.pl/wms""]}}]}"
        Dim gminyLodzJson = "{""data"":[{""idiip"":""PL.ZIPPZP.3"",""teryt"":""106101_1"",""zbior"":""MPZP"",""organ"":""Prezydent Miasta Łodzi"",""obszar"":""gmina miejska Łódź"",""tematy"":[],""uslugi"":{""przegladania"":[""https://mpzp.lodz.pl/wms""]}}]}"
        Dim pusteJson = "{""data"":[]}"
        Using hEz As New MockHttpHandler(
            Function(uri)
                If uri.Query.Contains("rodzaj=powiaty") Then
                    Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent(powiatyJson, Encoding.UTF8, "application/json")}
                End If
                If uri.Query.Contains("teryt=1061") Then
                    Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent(gminyLodzJson, Encoding.UTF8, "application/json")}
                End If
                If uri.Query.Contains("Zielonki") Then
                    Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent(gminyZielonkiJson, Encoding.UTF8, "application/json")}
                End If
                Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent(pusteJson, Encoding.UTF8, "application/json")}
            End Function)
            Dim resPow = Await KlientKataloguSerwerow.SzukajEziudpLokalneAsync("Wieliczka EGiB", "powiaty", handler:=hEz)
            Assert.True(resPow.Sukces)
            Assert.Single(resPow.Wpisy)
            Assert.Equal("https://wms.wielicki.pl/egib", resPow.Wpisy(0).Adres)

            Dim resGm = Await KlientKataloguSerwerow.SzukajEziudpLokalneAsync("Zielonki MPZP", "gminy", handler:=hEz)
            Assert.True(resGm.Sukces)
            Assert.Single(resGm.Wpisy)
            Assert.Contains("nazwa=Zielonki", hEz.OdpytywaneAdresy.Last())

            'miasto na prawach powiatu: nazwa="Łódź" w API gmin zwraca pusto, ale wykryte TERYT=1061 znajduje gmina miejska Łódź
            Dim resLodz = Await KlientKataloguSerwerow.SzukajEziudpLokalneAsync("Łódź MPZP", "oba", handler:=hEz)
            Assert.True(resLodz.Sukces)
            Assert.Single(resLodz.Wpisy)
            Assert.Equal("https://mpzp.lodz.pl/wms", resLodz.Wpisy(0).Adres)

            'wielowyrazowa nazwa miasta powinna zachować oba słowa w zapytaniu
            hEz.OdpytywaneAdresy.Clear()
            Dim resSacz = Await KlientKataloguSerwerow.SzukajEziudpLokalneAsync("Nowy Sącz MPZP", "gminy", handler:=hEz)
            Assert.True(resSacz.Sukces)
            Assert.Contains(hEz.OdpytywaneAdresy, Function(u) u.Contains("Nowy%20S"))
        End Using
    End Function

    <Fact>
    Public Sub PlikWarstw_ZapisIOdczytCzytelnychTytulowDlaDodanychSerwerow()
        Dim plik = Path.GetTempFileName()
        Try
            Dim p As New PlikWarstw With {
                .Adres = PlikWarstw.ZbudujAdresWmsDlaPliku("https://serwer.pl/wms", "1.3.0"),
                .Tytul = "System Osłony Przeciwosuwiskowej"
            }
            p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "0", .Tytul = "Osuwiska aktywne", .RozmiarPiksela = "2"})
            p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "1", .Tytul = "Tereny zagrożone", .RozmiarPiksela = "2"})
            p.Zapisz(plik)

            Dim wczytany = PlikWarstw.Wczytaj(plik)
            Assert.Equal("System Osłony Przeciwosuwiskowej", wczytany.Tytul)
            Assert.Equal(TypUslugiMapowej.Wms, wczytany.EfektywnyTyp)
            Assert.Equal(2, wczytany.Warstwy.Count)
            Assert.Equal("0", wczytany.Warstwy(0).Nazwa)
            Assert.Equal("Osuwiska aktywne", wczytany.Warstwy(0).Tytul)
            Assert.Equal("Osuwiska aktywne", wczytany.Warstwy(0).ToString())
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Async Function KlientCapabilities_BladSslBezPonawianiaZKomunikatem() As Task
        Using h As New MockHttpHandler(
            Function(uri)
                Throw New HttpRequestException("The SSL connection could not be established.",
                                               New System.Security.Authentication.AuthenticationException("The remote certificate is invalid."))
            End Function)
            Dim res = Await KlientCapabilities.PobierzAsync("https://serwer.test/wms", ignorujSsl:=False, handler:=h)
            Assert.False(res.Sukces)
            Assert.Single(h.OdpytywaneAdresy)
            Assert.Contains("Ignoruj błędy certyfikatów SSL", res.KomunikatBledu)
        End Using
    End Function

    <Fact>
    Public Async Function KlientKatalogu_OfflineWracaDoPrzeterminowanegoCache() As Task
        Dim plikGp = Path.GetTempFileName()
        Dim plikEz = Path.GetTempFileName()
        Try
            Dim wpisy = New List(Of WpisKataloguSerwerow) From {
                New WpisKataloguSerwerow With {
                    .Nazwa = "Usługa testowa offline",
                    .Adres = "https://offline.test/wms",
                    .Typ = TypUslugiMapowej.Wms,
                    .Zrodlo = ZrodloKataloguSerwerow.Geoportal
                }
            }
            File.WriteAllText(plikGp, KatalogSerwerow.DoJsona(wpisy), Encoding.UTF8)
            File.SetLastWriteTimeUtc(plikGp, DateTime.UtcNow.AddHours(-36))
            File.WriteAllText(plikEz, KatalogSerwerow.DoJsona(wpisy), Encoding.UTF8)
            File.SetLastWriteTimeUtc(plikEz, DateTime.UtcNow.AddHours(-36))

            Using hOffline As New MockHttpHandler(Function(uri) New HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                Dim resGp = Await KlientKataloguSerwerow.PobierzGeoportalAsync(sciezkaCache:=plikGp, maxWiekGodzin:=24, handler:=hOffline)
                Assert.True(resGp.Sukces)
                Assert.True(resGp.ZCache)
                Assert.True(resGp.ZCacheStary)
                Assert.Single(resGp.Wpisy)

                Dim resEz = Await KlientKataloguSerwerow.PobierzEziudpCentralneIWojewodzkieAsync(sciezkaCache:=plikEz, maxWiekGodzin:=24, handler:=hOffline)
                Assert.True(resEz.Sukces)
                Assert.True(resEz.ZCache)
                Assert.True(resEz.ZCacheStary)
                Assert.Single(resEz.Wpisy)
            End Using
        Finally
            File.Delete(plikGp)
            File.Delete(plikEz)
        End Try
    End Function

    <Fact>
    Public Sub ParsujEziudpJson_ScalaWiecejNizSzescZbiorowPodJednymAdresem()
        Dim rekordy As New List(Of String)()
        For i = 1 To 10
            rekordy.Add("{""idiip"":""PL." & i & """,""zbior"":""ZbiorNumer" & i & """,""organ"":""PGW Wody Polskie"",""obszar"":""Polska"",""tematy"":[],""uslugi"":{""przegladania"":[""https://wody.test/INSPIRE_WMS""]}}")
        Next
        Dim json = "{""data"":[" & String.Join(",", rekordy) & "]}"

        Dim wpisy = KatalogSerwerow.ParsujEziudpJson(json, ZrodloKataloguSerwerow.EziudpCentralne)
        Assert.Single(wpisy)
        Assert.Single(KatalogSerwerow.Filtruj(wpisy, "ZbiorNumer10"))
    End Sub

    <Fact>
    Public Sub DoprecyzujNazweUslugi_RozrozniaPoduslugiCbdgNmtOrtoIOsnow()
        'CBDG
        Dim nCbdg = "Centralna Baza Danych Geologicznych (CBDG)"
        Assert.Equal("Centralna Baza Danych Geologicznych (CBDG) – Geofizyka: magnetotelluryka",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nCbdg, "http://cbdgmapa.pgi.gov.pl/arcgis/services/geofizyka/magnetotelluryka/MapServer/WMSServer"))
        Assert.Equal("Centralna Baza Danych Geologicznych (CBDG) – Jaskinie",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nCbdg, "http://cbdgmapa.pgi.gov.pl/arcgis/services/jaskinie/MapServer/WMSServer"))
        Assert.Equal("Centralna Baza Danych Geologicznych (CBDG) – Otwory wiertnicze",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nCbdg, "http://cbdgmapa.pgi.gov.pl/arcgis/services/cbdg_otwory/MapServer/WMSServer"))

        'NMT
        Dim nNmt = "Numeryczny Model Terenu"
        Assert.Equal("Numeryczny Model Terenu – Cieniowanie rzeźby",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nNmt, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/GRID1/WMTS/ShadedRelief"))
        Assert.Equal("Numeryczny Model Terenu – Hipsometria barwna",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nNmt, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/GRID1/WMTS/Hypsometry"))

        'Ortofotomapa
        Dim nOrto = "Ortofotomapa"
        Assert.Equal("Ortofotomapa – Wysoka rozdzielczość",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nOrto, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/HighResolution"))
        Assert.Equal("Ortofotomapa – Prawdziwa ortofotomapa (TrueOrtho)",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nOrto, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/TrueOrtho"))

        'gdy nazwa już zawiera wyróżnik podusługi, nie dublujemy
        Dim nOrtoJuzZPodtytulem = "Ortofotomapa o wysokiej rozdzielczości"
        Assert.Equal("Ortofotomapa o wysokiej rozdzielczości",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nOrtoJuzZPodtytulem, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/HighResolution"))

        'podusługi użyteczności publicznej
        Dim nBup = "Obiekty użyteczności publicznej"
        Assert.Equal("Obiekty użyteczności publicznej – Straż pożarna",
                     KatalogSerwerow.DoprecyzujNazweUslugi(nBup, "https://uslugi.podlaskie.eu/bu_p_straz_pozarna/service.svc/get"))
    End Sub

    <Fact>
    Public Sub ParsujEziudpJson_MultiUrlCbdgDajeUnikalneNazwyIOpisy()
        Dim json = "{
          ""data"": [
            {
              ""idiip"": ""PL.ZIPGL.313"",
              ""teryt"": ""PL"",
              ""zbior"": ""Centralna Baza Danych Geologicznych (CBDG)"",
              ""organ"": ""Główny Geolog Kraju"",
              ""obszar"": ""Polska"",
              ""tematy"": [""Geologia""],
              ""uslugi"": {
                ""przegladania"": [
                  ""WMS=http://cbdgmapa.pgi.gov.pl/arcgis/services/cbdg_otwory/MapServer/WMSServer"",
                  ""WMS=http://cbdgmapa.pgi.gov.pl/arcgis/services/geofizyka/magnetotelluryka/MapServer/WMSServer"",
                  ""WMS=http://cbdgmapa.pgi.gov.pl/arcgis/services/jaskinie/MapServer/WMSServer""
                ]
              }
            }
          ]
        }"

        Dim wpisy = KatalogSerwerow.ParsujEziudpJson(json, ZrodloKataloguSerwerow.EziudpCentralne)
        Assert.Equal(3, wpisy.Count)

        'wszystkie 3 wpisy muszą mieć unikalne, czytelne nazwy rozszerzone o podusługę
        Dim nazwy = wpisy.Select(Function(w) w.Nazwa).ToList()
        Assert.Equal(3, nazwy.Distinct().Count())
        Assert.Contains("Otwory wiertnicze", nazwy(0))
        Assert.Contains("magnetotelluryka", nazwy(1))
        Assert.Contains("Jaskinie", nazwy(2))

        'opisy zawierają merytoryczną charakterystykę podusługi
        Assert.Contains("otworów wiertniczych", wpisy(0).Opis)
        Assert.Contains("magnetotelluryczne", wpisy(1).Opis)
        Assert.Contains("jaskiń", wpisy(2).Opis)
    End Sub

    <Fact>
    Public Sub StanWMapoTero_ZwracaMoznaDodacLubWMapoTero()
        Dim wNowy As New WpisKataloguSerwerow With {.Nazwa = "Testowa usługa", .Adres = "https://a.test/wms"}
        Assert.Equal(KatalogSerwerow.TekstStanuMoznaDodac, KatalogSerwerow.TekstStanuWMapoTero(wNowy))
        Assert.Equal("Można dodać", KatalogSerwerow.TekstStanuWMapoTero(wNowy))

        Dim wDodany As New WpisKataloguSerwerow With {.Nazwa = "Ortofotomapa", .Adres = "https://b.test/wms", .JuzDodanyZbior = "ortofotomapa_WMTS"}
        Assert.Equal("W MapoTero (ortofotomapa_WMTS)", KatalogSerwerow.TekstStanuWMapoTero(wDodany))
    End Sub

    <Fact>
    Public Sub FormatujPodgladCapabilities_TworzyPodsumowanieTytuluIWarstw()
        Dim res As New WynikCapabilities With {
            .Typ = TypUslugiMapowej.Wms,
            .UslugaWms = New UslugaWmsInfo With {
                .Tytul = "Serwer Geofizyki CBDG",
                .Abstrakt = "Pomiary magnetotelluryczne wgłębnej struktury skorupy ziemskiej."
            }
        }
        res.UslugaWms.Warstwy.Add(New WarstwaWmsInfo With {.Nazwa = "0", .Tytul = "Punkty pomiarowe"})
        res.UslugaWms.Warstwy.Add(New WarstwaWmsInfo With {.Nazwa = "1", .Tytul = "Profile magnetotelluryczne"})

        Dim podglad = KatalogSerwerow.FormatujPodgladCapabilities(res)
        Assert.Contains("Tytuł z serwera: Serwer Geofizyki CBDG", podglad)
        Assert.Contains("Opis z serwera: Pomiary magnetotelluryczne", podglad)
        Assert.Contains("Warstwy na serwerze (2): Punkty pomiarowe, Profile magnetotelluryczne", podglad)
    End Sub

    <Fact>
    Public Sub Filtruj_OpcjaUkryjSkorowidze_PrawidlowoFiltrujeSiatkiArkuszy()
        Dim w1 As New WpisKataloguSerwerow With {.Nazwa = "Siatka arkuszy map 1:10 000", .Adres = "https://a/wms", .CzySkorowidz = True}
        Dim w2 As New WpisKataloguSerwerow With {.Nazwa = "Rzeźba terenu", .Adres = "https://b/wms", .CzySkorowidz = False}
        Dim lista = New List(Of WpisKataloguSerwerow) From {w1, w2}

        Dim bezUkrywania = KatalogSerwerow.Filtruj(lista, "", "Wszystkie", ukryjJuzDodane:=False, ukryjSkorowidze:=False)
        Assert.Equal(2, bezUkrywania.Count)

        Dim zUkrywaniem = KatalogSerwerow.Filtruj(lista, "", "Wszystkie", ukryjJuzDodane:=False, ukryjSkorowidze:=True)
        Assert.Single(zUkrywaniem)
        Assert.Equal("Rzeźba terenu", zUkrywaniem(0).Nazwa)
    End Sub

End Class
