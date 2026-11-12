'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports Xunit

''' <summary>Testy regulacji obciążenia serwerów (limit jednoczesnych zapytań, hamowanie i przyspieszanie).</summary>
Public Class RegulatorZapytanTesty

    Private Const Adres As String = "https://mapy.geoportal.gov.pl/wss/service/WMTS?SERVICE=WMTS&TileRow=1"
    Private Const InnySerwer As String = "https://mapy.inny.pl/wms?REQUEST=GetMap"

    Private Shared Function Regulator(maksimum As Integer, Optional komunikaty As List(Of String) = Nothing) As RegulatorZapytan
        Dim r As New RegulatorZapytan() With {.PrzerwaPoczatkowa = TimeSpan.FromMilliseconds(50),
                                              .PrzerwaMaksymalna = TimeSpan.FromMilliseconds(200), .SukcesyDoPrzyspieszenia = 3}
        r.Rozpocznij(maksimum, Sub(t)
                                   If komunikaty IsNot Nothing Then SyncLock komunikaty : komunikaty.Add(t) : End SyncLock
                               End Sub)
        Return r
    End Function

    <Fact>
    Public Async Function NajwyzejTyleZapytanNarazDoSerweraIleWatkow() As Task
        Dim r = Regulator(2)
        Dim p1 = Await r.WejdzAsync(Adres, CancellationToken.None)
        Dim p2 = Await r.WejdzAsync(Adres.Replace("TileRow=1", "TileRow=2"), CancellationToken.None)
        Dim trzecia = r.WejdzAsync(Adres, CancellationToken.None)
        Await Task.Delay(100)
        Assert.False(trzecia.IsCompleted)

        'inny serwer ma osobny limit
        Using p As RegulatorZapytan.Przepustka = Await r.WejdzAsync(InnySerwer, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5))
        End Using

        p1.Dispose()
        p1.Dispose() 'ponowne zwolnienie nie zwiększa limitu
        Dim p3 = Await trzecia.WaitAsync(TimeSpan.FromSeconds(5))
        Dim czwarta = r.WejdzAsync(Adres, CancellationToken.None)
        Await Task.Delay(100)
        Assert.False(czwarta.IsCompleted)
        p2.Dispose()
        p3.Dispose()
        Using p4 = Await czwarta.WaitAsync(TimeSpan.FromSeconds(5))
        End Using
    End Function

    <Fact>
    Public Async Function OczekiwanieNaPrzepustkeMoznaPrzerwac() As Task
        Dim r = Regulator(1)
        Using p1 = Await r.WejdzAsync(Adres, CancellationToken.None)
            Using przerwij As New CancellationTokenSource(50)
                Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(Function() r.WejdzAsync(Adres, przerwij.Token))
            End Using
        End Using
        'przerwane oczekiwanie nie zabiera miejsca
        Using p2 = Await r.WejdzAsync(Adres, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5))
        End Using
    End Function

    <Theory>
    <InlineData(429)>
    <InlineData(503)>
    <InlineData(502)>
    <InlineData(504)>
    Public Async Function PrzeciazenieSerwera_PrzerwaIPolowaZapytan(kod As Integer) As Task
        Dim komunikaty As New List(Of String)
        Dim r = Regulator(8, komunikaty)
        Dim p1 = Await r.WejdzAsync(Adres, CancellationToken.None)
        Dim p2 = Await r.WejdzAsync(Adres, CancellationToken.None)
        p1.Odpowiedz(CType(kod, HttpStatusCode))
        Assert.Equal(4, r.Limit(Adres))
        Assert.True(r.PozostalaPrzerwa(Adres) > TimeSpan.Zero)
        Assert.Single(komunikaty)
        Assert.Contains("mapy.geoportal.gov.pl", komunikaty(0))
        Assert.Contains("HTTP " & kod, komunikaty(0))
        Assert.Contains("najwyżej 4 zapytania naraz", komunikaty(0))

        'zapytanie wysłane przed hamowaniem zgłasza to samo przeciążenie - bez kolejnego hamowania
        p2.Odpowiedz(HttpStatusCode.ServiceUnavailable)
        Assert.Equal(4, r.Limit(Adres))
        Assert.Single(komunikaty)
        p1.Dispose()
        p2.Dispose()
        'inny serwer bez zmian
        Assert.Equal(8, r.Limit(InnySerwer))
    End Function

    <Fact>
    Public Async Function PrzepustkaCzekaNaKoniecPrzerwy() As Task
        Dim r = Regulator(4)
        r.PrzerwaPoczatkowa = TimeSpan.FromMilliseconds(400)
        r.PrzerwaMaksymalna = TimeSpan.FromSeconds(1)
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.TooManyRequests)
        End Using
        Dim czas = Stopwatch.StartNew()
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
        End Using
        Assert.True(czas.ElapsedMilliseconds >= 300, "przepustka wydana po " & czas.ElapsedMilliseconds & " ms")
    End Function

    <Fact>
    Public Async Function RetryAfterSerweraWyznaczaPrzerweZOgraniczeniem() As Task
        Dim r = Regulator(4)
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(30))
        End Using
        Dim przerwa = r.PozostalaPrzerwa(Adres)
        Assert.InRange(przerwa.TotalSeconds, 25, 30)

        Dim r2 = Regulator(4)
        Using p = Await r2.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.ServiceUnavailable, TimeSpan.FromHours(1))
        End Using
        Assert.InRange(r2.PozostalaPrzerwa(Adres).TotalSeconds, 100, 120)
    End Function

    <Fact>
    Public Async Function SeriaBrakuOdpowiedziHamuje_PojedynczyBladNie() As Task
        Dim komunikaty As New List(Of String)
        Dim r = Regulator(6, komunikaty)
        For i = 1 To 2
            Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
                p.Blad()
            End Using
        Next
        'odpowiedź serwera przerywa serię błędów
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.OK)
        End Using
        For i = 1 To 2
            Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
                p.Blad()
            End Using
        Next
        Assert.Equal(6, r.Limit(Adres))
        Assert.Empty(komunikaty)

        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Blad()
        End Using
        Assert.Equal(3, r.Limit(Adres))
        Assert.Single(komunikaty)
        Assert.Contains("brak odpowiedzi", komunikaty(0))
    End Function

    <Fact>
    Public Async Function UdaneOdpowiedziStopniowoPrzywracajaLiczbeZapytan() As Task
        Dim r = Regulator(4)
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.ServiceUnavailable)
        End Using
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
            p.Odpowiedz(HttpStatusCode.ServiceUnavailable)
        End Using
        Assert.Equal(1, r.Limit(Adres))

        'po 3 udanych odpowiedziach (SukcesyDoPrzyspieszenia) o jedno zapytanie więcej; 404 - kafel poza obszarem danych
        For i = 1 To 3
            Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
                p.Odpowiedz(If(i = 2, HttpStatusCode.NotFound, HttpStatusCode.OK))
            End Using
        Next
        Assert.Equal(2, r.Limit(Adres))
        'dwa zapytania naraz są znów możliwe
        Dim p1 = Await r.WejdzAsync(Adres, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5))
        Dim p2 = Await r.WejdzAsync(Adres, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5))
        Dim trzecia = r.WejdzAsync(Adres, CancellationToken.None)
        Await Task.Delay(100)
        Assert.False(trzecia.IsCompleted)
        p1.Odpowiedz(HttpStatusCode.OK)
        p1.Dispose()
        p2.Odpowiedz(HttpStatusCode.OK)
        p2.Dispose()
        Using p3 = Await trzecia.WaitAsync(TimeSpan.FromSeconds(5))
            p3.Odpowiedz(HttpStatusCode.OK)
        End Using

        For i = 1 To 6
            Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
                p.Odpowiedz(HttpStatusCode.OK)
            End Using
        Next
        'nigdy powyżej liczby wątków z ustawień
        Assert.Equal(4, r.Limit(Adres))
        For i = 1 To 10
            Using p = Await r.WejdzAsync(Adres, CancellationToken.None)
                p.Odpowiedz(HttpStatusCode.OK)
            End Using
        Next
        Assert.Equal(4, r.Limit(Adres))
        Dim przepustki As New List(Of RegulatorZapytan.Przepustka)
        For i = 1 To 4
            przepustki.Add(Await r.WejdzAsync(Adres, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)))
        Next
        Dim piata = r.WejdzAsync(Adres, CancellationToken.None)
        Await Task.Delay(100)
        Assert.False(piata.IsCompleted)
        przepustki.ForEach(Sub(p) p.Dispose())
        Using p5 = Await piata.WaitAsync(TimeSpan.FromSeconds(5))
        End Using
    End Function

    <Fact>
    Public Async Function ZmniejszenieLimituZatrzymujeZwalnianePrzepustki() As Task
        Dim r = Regulator(4)
        Dim przepustki As New List(Of RegulatorZapytan.Przepustka)
        For i = 1 To 4
            przepustki.Add(Await r.WejdzAsync(Adres, CancellationToken.None))
        Next
        przepustki(0).Odpowiedz(HttpStatusCode.ServiceUnavailable)
        Assert.Equal(2, r.Limit(Adres))
        'zwolnione 3 z 4 przepustek: 2 zatrzymane (limit 2), więc w toku 1 + 1 wolne miejsce
        przepustki(0).Dispose()
        przepustki(1).Dispose()
        przepustki(2).Dispose()
        Using p = Await r.WejdzAsync(Adres, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5))
            Dim kolejna = r.WejdzAsync(Adres, CancellationToken.None)
            Await Task.Delay(400)
            Assert.False(kolejna.IsCompleted)
            przepustki(3).Dispose()
            Using p2 = Await kolejna.WaitAsync(TimeSpan.FromSeconds(5))
            End Using
        End Using
    End Function

    <Theory>
    <InlineData("https://mapy.geoportal.gov.pl/wss/service/WMTS?x=1", "mapy.geoportal.gov.pl")>
    <InlineData("http://localhost:8080/geoserver/wms?", "localhost:8080")>
    <InlineData("niepoprawny adres", "niepoprawny adres")>
    Public Sub NazwaSerweraZAdresu(adres As String, serwer As String)
        Assert.Equal(serwer, RegulatorZapytan.NazwaSerwera(adres))
    End Sub

    <Fact>
    Public Async Function WspolnyKluczSerweraGrupujeSubdomenyILimitWatkow() As Task
        Dim r = Regulator(8)
        Using p1 = Await r.WejdzAsync("https://a.tile.test/1/0/0.png", CancellationToken.None, kluczSerwera:="tile.test", maksRownoleglych:=2)
            Using p2 = Await r.WejdzAsync("https://b.tile.test/1/0/1.png", CancellationToken.None, kluczSerwera:="tile.test", maksRownoleglych:=2)
                'trzecia subdomena c dzieli limit 2 z subdomenami a i b
                Dim trzecia = r.WejdzAsync("https://c.tile.test/1/0/2.png", CancellationToken.None, kluczSerwera:="tile.test", maksRownoleglych:=2)
                Await Task.Delay(120)
                Assert.False(trzecia.IsCompleted)
                p1.Odpowiedz(HttpStatusCode.OK)
                p1.Dispose()
                Using p3 = Await trzecia.WaitAsync(TimeSpan.FromSeconds(5))
                    p3.Odpowiedz(HttpStatusCode.OK)
                End Using
                p2.Odpowiedz(HttpStatusCode.OK)
            End Using
        End Using
    End Function

    <Fact>
    Public Async Function OdstepMsRozsuwaKolejneZapytaniaIHamujeNa403Oraz418() As Task
        Dim r = Regulator(4)
        Dim sw = Stopwatch.StartNew()
        Using p1 = Await r.WejdzAsync(Adres, CancellationToken.None, kluczSerwera:="pacing.test", maksRownoleglych:=2, odstepMs:=120)
            p1.Odpowiedz(HttpStatusCode.OK)
        End Using
        Using p2 = Await r.WejdzAsync(Adres, CancellationToken.None, kluczSerwera:="pacing.test", maksRownoleglych:=2, odstepMs:=120)
            p2.Odpowiedz(HttpStatusCode.OK)
        End Using
        Using p3 = Await r.WejdzAsync(Adres, CancellationToken.None, kluczSerwera:="pacing.test", maksRownoleglych:=2, odstepMs:=120)
            sw.Stop()
            'dwa interwały po >=120 ms między kolejnymi wywołaniami (ok. 240 ms)
            Assert.True(sw.ElapsedMilliseconds >= 180, "Za szybkie zapytania mimo odstepMs=120: " & sw.ElapsedMilliseconds & " ms")
            p3.Odpowiedz(HttpStatusCode.Forbidden)
        End Using
        Assert.Equal(1, r.Limit(Adres, kluczSerwera:="pacing.test"))
        Assert.True(r.PozostalaPrzerwa(Adres, kluczSerwera:="pacing.test") > TimeSpan.Zero)

        Using p4 = Await r.WejdzAsync("https://teapot.test/1/0/0.png", CancellationToken.None)
            p4.Odpowiedz(CType(418, HttpStatusCode))
        End Using
        Assert.Equal(2, r.Limit("https://teapot.test/1/0/0.png"))
    End Function

End Class
