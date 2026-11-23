' =====================================================================
' Testy jednostkowe serwera transferu Wi-Fi (HTTP, Range, Token, RFC 5987)
' =====================================================================

Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.NetworkInformation
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Threading.Tasks
Imports MapoTero
Imports Xunit

Public Class SerwerWysylkiWifiTesty

    Private Shared Function UtworzPlikTymczasowy(rozmiar As Integer) As String
        Dim sciezka = Path.Combine(Path.GetTempPath(), $"MapoTero_Test_{Guid.NewGuid():N}.mbtiles")
        Dim dane(rozmiar - 1) As Byte
        For i = 0 To dane.Length - 1
            dane(i) = CByte(i Mod 256)
        Next
        File.WriteAllBytes(sciezka, dane)
        Return sciezka
    End Function

    <Fact>
    Public Sub Konstruktor_PlikNieIstnieje_RzucaFileNotFoundException()
        Assert.Throws(Of FileNotFoundException)(Sub()
                                                    Using s = New SerwerWysylkiWifi("C:\nieistnieje_12345.mbtiles")
                                                    End Using
                                                End Sub)
    End Sub

    <Fact>
    Public Async Function Serwer_StronaHtml_Zwraca200IOpisPliku() As Task
        Dim plik = UtworzPlikTymczasowy(1024)
        Try
            Using serwer As New SerwerWysylkiWifi(plik)
                serwer.Uruchom(0)
                Dim adres = serwer.PobierzAdresStrony("127.0.0.1")

                Using klient As New HttpClient()
                    Dim resp = Await klient.GetAsync(adres)
                    Assert.Equal(HttpStatusCode.OK, resp.StatusCode)
                    Assert.Equal("text/html", resp.Content.Headers.ContentType?.MediaType)

                    Dim html = Await resp.Content.ReadAsStringAsync()
                    Assert.Contains(Path.GetFileName(plik), html)
                    Assert.Contains($"/{serwer.Token}/pobierz", html)
                End Using
            End Using
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Function

    <Fact>
    Public Async Function Serwer_PobierzPlikPelny_Zwraca200PrawidloweBajtIContentDisposition() As Task
        Dim plik = UtworzPlikTymczasowy(4096)
        Try
            Using serwer As New SerwerWysylkiWifi(plik)
                serwer.Uruchom(0)
                Dim adres = serwer.PobierzAdresPliku("127.0.0.1")

                Using klient As New HttpClient()
                    Dim resp = Await klient.GetAsync(adres)
                    Assert.Equal(HttpStatusCode.OK, resp.StatusCode)
                    Assert.Equal("application/octet-stream", resp.Content.Headers.ContentType?.MediaType)
                    Assert.Equal(4096L, resp.Content.Headers.ContentLength)

                    ' Sprawdzenie nagłówka Content-Disposition
                    Dim disp = resp.Content.Headers.ContentDisposition
                    Assert.NotNull(disp)
                    Assert.Equal("attachment", disp.DispositionType)

                    Dim bajty = Await resp.Content.ReadAsByteArrayAsync()
                    Assert.Equal(4096, bajty.Length)
                    Assert.Equal(File.ReadAllBytes(plik), bajty)
                End Using
            End Using
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Function

    <Fact>
    Public Async Function Serwer_ZadanieRange_Zwraca206IPrawidlowyWycinek() As Task
        Dim plik = UtworzPlikTymczasowy(1000)
        Try
            Using serwer As New SerwerWysylkiWifi(plik)
                serwer.Uruchom(0)
                Dim adres = serwer.PobierzAdresPliku("127.0.0.1")

                Using klient As New HttpClient()
                    Using req As New HttpRequestMessage(HttpMethod.Get, adres)
                        req.Headers.Range = New RangeHeaderValue(100, 199)

                        Using resp = Await klient.SendAsync(req)
                            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode)
                            Assert.Equal(100L, resp.Content.Headers.ContentLength)
                            Assert.NotNull(resp.Content.Headers.ContentRange)
                            Assert.Equal(100L, resp.Content.Headers.ContentRange.From)
                            Assert.Equal(199L, resp.Content.Headers.ContentRange.To)
                            Assert.Equal(1000L, resp.Content.Headers.ContentRange.Length)

                            Dim bajty = Await resp.Content.ReadAsByteArrayAsync()
                            Assert.Equal(100, bajty.Length)

                            ' Porównanie z faktycznym fragmentem pliku
                            Dim pelne = File.ReadAllBytes(plik)
                            Dim oczekiwane(99) As Byte
                            Array.Copy(pelne, 100, oczekiwane, 0, 100)
                            Assert.Equal(oczekiwane, bajty)
                        End Using
                    End Using
                End Using
            End Using
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Function

    <Fact>
    Public Async Function Serwer_ZlyToken_Zwraca404NotFound() As Task
        Dim plik = UtworzPlikTymczasowy(512)
        Try
            Using serwer As New SerwerWysylkiWifi(plik)
                serwer.Uruchom(0)
                Dim zlyAdres = $"http://127.0.0.1:{serwer.Port}/nieistniejacy_token_12345/pobierz"

                Using klient As New HttpClient()
                    Dim resp = Await klient.GetAsync(zlyAdres)
                    Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode)
                End Using
            End Using
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Function

    <Fact>
    Public Async Function Serwer_Head_ZwracaNaglowkiBezCiala() As Task
        Dim plik = UtworzPlikTymczasowy(2048)
        Try
            Using serwer As New SerwerWysylkiWifi(plik)
                serwer.Uruchom(0)
                Dim adres = serwer.PobierzAdresPliku("127.0.0.1")

                Using klient As New HttpClient()
                    Using req As New HttpRequestMessage(HttpMethod.Head, adres)
                        Using resp = Await klient.SendAsync(req)
                            Assert.Equal(HttpStatusCode.OK, resp.StatusCode)
                            Assert.Equal(2048L, resp.Content.Headers.ContentLength)
                            Dim cialo = Await resp.Content.ReadAsByteArrayAsync()
                            Assert.Empty(cialo)
                        End Using
                    End Using
                End Using
            End Using
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Function

    <Fact>
    Public Sub WykrywanieAdresowLan_ZwracaPrzynajmniejJedenAdres()
        Dim domyslny = SerwerWysylkiWifi.WykryjDomyslneIpLan()
        Assert.False(String.IsNullOrWhiteSpace(domyslny))

        Dim wszystkie = SerwerWysylkiWifi.PobierzWszystkieAdresyLan()
        Assert.NotNull(wszystkie)
    End Sub

    <Fact>
    Public Sub PriorytetKarty_KartaWifiPrzedKartaVpn()
        Dim wifi = SerwerWysylkiWifi.PriorytetKarty("Wi-Fi Intel(R) Wi-Fi 6 AX201", NetworkInterfaceType.Wireless80211, True)
        Dim vpnZBrama = SerwerWysylkiWifi.PriorytetKarty("NordVPN TAP-Windows Adapter V9", NetworkInterfaceType.Ethernet, True)
        Dim anyConnect = SerwerWysylkiWifi.PriorytetKarty("Cisco AnyConnect Secure Mobility Client", NetworkInterfaceType.Ethernet, False)
        Dim ppp = SerwerWysylkiWifi.PriorytetKarty("Firmowe połączenie", NetworkInterfaceType.Ppp, True)
        Dim ethernet = SerwerWysylkiWifi.PriorytetKarty("Ethernet Realtek PCIe GbE", NetworkInterfaceType.Ethernet, True)
        Assert.True(wifi < ethernet)
        Assert.True(ethernet < vpnZBrama)
        Assert.True(wifi < anyConnect)
        Assert.True(wifi < ppp)
    End Sub

End Class
