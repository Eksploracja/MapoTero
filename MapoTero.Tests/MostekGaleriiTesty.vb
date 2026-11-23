Option Strict On
Option Explicit On
Option Infer On

Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading.Tasks
Imports MapoTero
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

End Class
