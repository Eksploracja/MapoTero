Option Strict On
Option Explicit On
Option Infer On

Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class MostekGaleriiTesty

    <Fact>
    Public Sub ZbudujAdresGalerii_DopisujePortIToken()
        Using mostek As New MostekGalerii(Function(z, w) WynikMostkaGalerii.Dodano("ok"))
            Dim url = mostek.ZbudujAdresGalerii("https://eksploracja.github.io/MapoTero/pomoc/galeria-warstw/")
            Assert.Contains($"?port={mostek.Port}&token={mostek.Token}", url)

            Dim urlZParam = mostek.ZbudujAdresGalerii("http://localhost:8010/pomoc/galeria-warstw/?typ=WMTS")
            Assert.Contains($"&port={mostek.Port}&token={mostek.Token}", urlZParam)
        End Using
    End Sub

    <Fact>
    Public Sub CzyDozwolonyOrigin_AkceptujeGitHubPagesILocalhost()
        Assert.True(MostekGalerii.CzyDozwolonyOrigin("https://eksploracja.github.io"))
        Assert.True(MostekGalerii.CzyDozwolonyOrigin("http://localhost:8010"))
        Assert.True(MostekGalerii.CzyDozwolonyOrigin("http://127.0.0.1:8010"))
        Assert.False(MostekGalerii.CzyDozwolonyOrigin("https://example.com"))
        Assert.False(MostekGalerii.CzyDozwolonyOrigin("http://eksploracja.github.io"))
        Assert.False(MostekGalerii.CzyDozwolonyOrigin(""))
    End Sub

    <Fact>
    Public Async Function Ping_ZPoprawnymTokenem_ZwracaOk() As Task
        Using mostek As New MostekGalerii(Function(z, w) WynikMostkaGalerii.Dodano("ok"))
            Using klient As New HttpClient()
                Dim resp = Await klient.GetAsync($"http://127.0.0.1:{mostek.Port}/ping?token={mostek.Token}")
                Assert.Equal(HttpStatusCode.OK, resp.StatusCode)
                Dim body = Await resp.Content.ReadAsStringAsync()
                Using doc = JsonDocument.Parse(body)
                    Assert.True(doc.RootElement.GetProperty("ok").GetBoolean())
                End Using
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function Dodaj_ZPoprawnymTokenem_PrzekazujeZbiorIWarstwe() As Task
        Dim odebranyZbior As String = ""
        Dim odebranaWarstwa As String = ""

        Using mostek As New MostekGalerii(Function(z, w)
                                              odebranyZbior = z
                                              odebranaWarstwa = w
                                              Return WynikMostkaGalerii.Dodano("Dodano warstwę")
                                          End Function)
            Using klient As New HttpClient()
                Dim resp = Await klient.GetAsync($"http://127.0.0.1:{mostek.Port}/dodaj?zbior=BDOT10k_WMTS&warstwa=BDOT10k&token={mostek.Token}")
                Assert.Equal(HttpStatusCode.OK, resp.StatusCode)
                Assert.Equal("BDOT10k_WMTS", odebranyZbior)
                Assert.Equal("BDOT10k", odebranaWarstwa)
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function Dodaj_ZBlednymTokenem_Zwraca403() As Task
        Dim wywolano = False
        Using mostek As New MostekGalerii(Function(z, w)
                                              wywolano = True
                                              Return WynikMostkaGalerii.Dodano("ok")
                                          End Function)
            Using klient As New HttpClient()
                Dim resp = Await klient.GetAsync($"http://127.0.0.1:{mostek.Port}/dodaj?zbior=BDOT10k_WMTS&warstwa=BDOT10k&token=zlytoken")
                Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode)
                Assert.False(wywolano)
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function Options_ZGitHubPages_ZwracaNaglowkiCorsIPna() As Task
        Using mostek As New MostekGalerii(Function(z, w) WynikMostkaGalerii.Dodano("ok"))
            Using klient As New HttpClient()
                Dim req As New HttpRequestMessage(HttpMethod.Options, $"http://127.0.0.1:{mostek.Port}/dodaj")
                req.Headers.Add("Origin", "https://eksploracja.github.io")
                Dim resp = Await klient.SendAsync(req)
                Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode)
                Assert.Equal("https://eksploracja.github.io", String.Join("", resp.Headers.GetValues("Access-Control-Allow-Origin")))
                Assert.Equal("true", String.Join("", resp.Headers.GetValues("Access-Control-Allow-Private-Network")))
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function Zestaw_Stan_IPodglad_ObslugujaPelnyPrzeplyw() As Task
        Dim odebranaLiczba As Integer = 0
        Dim opcje As New OpcjeMostkaGalerii With {
            .WersjaProgramu = "3.19",
            .ObslugaDodaj = Function(z, w) WynikMostkaGalerii.Dodano("ok"),
            .ObslugaPobierzStan = Function()
                                       Dim stan As New StanMostkaGalerii()
                                       stan.Obszar.Lat = 52.23
                                       stan.Obszar.Lon = 21.01
                                       stan.Obszar.Piksel = 2.0
                                       stan.Zestaw.Warstwy.Add(New PozycjaZestawuGalerii With {
                                           .Zbior = "TOPO_WMTS", .Warstwa = "MAPA TOPOGRAFICZNA", .Krycie = 100, .Tryb = "Zwykly"
                                       })
                                       Return stan
                                   End Function,
            .ObslugaZastosujZestaw = Function(dane)
                                         odebranaLiczba = dane.Warstwy.Count
                                         Return WynikMostkaGalerii.Ustawiono("Ustawiono zestaw", dane.Warstwy.Count)
                                     End Function,
            .ObslugaPodglad = Function(zad, ct)
                                  Dim png = New Byte() {&H89, &H50, &H4E, &H47}
                                  Return Task.FromResult(WynikPodgladuGalerii.Sukces(png, 50.06, 19.94, 2.0, {"brak1"}))
                              End Function
        }

        Using mostek As New MostekGalerii(opcje)
            Using klient As New HttpClient()
                Dim pingResp = Await klient.GetStringAsync($"http://127.0.0.1:{mostek.Port}/ping?token={mostek.Token}")
                Assert.Contains("""zestaw""", pingResp)
                Assert.Contains("""podglad""", pingResp)

                Dim stanResp = Await klient.GetStringAsync($"http://127.0.0.1:{mostek.Port}/stan?token={mostek.Token}")
                Assert.Contains("TOPO_WMTS", stanResp)
                Assert.Contains("52.23", stanResp)

                Dim jsonZestaw = "{""format"":""mapotero-zestaw"",""warstwy"":[{""zbior"":""TOPO_WMTS"",""warstwa"":""MAPA TOPOGRAFICZNA"",""krycie"":80,""tryb"":""Mnozenie""}]}"
                Dim postZestaw = Await klient.PostAsync($"http://127.0.0.1:{mostek.Port}/zestaw?token={mostek.Token}",
                                                        New StringContent(jsonZestaw, Encoding.UTF8, "application/json"))
                Assert.Equal(HttpStatusCode.OK, postZestaw.StatusCode)
                Assert.Equal(1, odebranaLiczba)

                Dim jsonPodglad = "{""warstwy"":[{""zbior"":""TOPO_WMTS"",""warstwa"":""MAPA TOPOGRAFICZNA"",""krycie"":100,""tryb"":""Zwykly""}],""lat"":50.06,""lon"":19.94,""piksel"":2}"
                Dim postPodglad = Await klient.PostAsync($"http://127.0.0.1:{mostek.Port}/podglad?token={mostek.Token}",
                                                         New StringContent(jsonPodglad, Encoding.UTF8, "application/json"))
                Assert.Equal(HttpStatusCode.OK, postPodglad.StatusCode)
                Assert.Equal("image/png", postPodglad.Content.Headers.ContentType?.MediaType)
                Assert.Equal("50.06", String.Join("", postPodglad.Headers.GetValues("X-MapoTero-Lat")))
                Assert.Equal("brak1", Uri.UnescapeDataString(String.Join("", postPodglad.Headers.GetValues("X-MapoTero-Brakujace"))))
            End Using
        End Using
    End Function

End Class
