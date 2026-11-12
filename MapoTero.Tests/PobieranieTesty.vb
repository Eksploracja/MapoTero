'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Testy pętli pobierania segmentów bez dostępu do internetu - lokalny serwer HTTP zwraca obrazek PNG (albo błąd)
''' na każde zapytanie.
''' </summary>
Public Class PobieranieTesty

    ''' <summary>Minimalny serwer HTTP na porcie lokalnym: odpowiedź dla każdego zapytania wg funkcji (kod, typ, dane).</summary>
    Private NotInheritable Class SerwerTestowy
        Implements IDisposable

        Private ReadOnly _nasluch As New TcpListener(IPAddress.Loopback, 0)
        Private ReadOnly _odpowiedz As Func(Of String, Tuple(Of Integer, String, Byte()))
        Private _zapytania As Integer
        Public Property OstatnieNaglowki As String = ""
        Public Property DodatkoweNaglowki As Func(Of String, String, String) = Nothing
        Public Property OdpowiedzZNaglowkami As Func(Of String, String, Tuple(Of Integer, String, Byte())) = Nothing

        Public Sub New(odpowiedz As Func(Of String, Tuple(Of Integer, String, Byte())))
            _odpowiedz = odpowiedz
            _nasluch.Start()
            Task.Run(AddressOf Obsluguj)
        End Sub

        Public ReadOnly Property Port As Integer
            Get
                Return DirectCast(_nasluch.LocalEndpoint, IPEndPoint).Port
            End Get
        End Property

        Public ReadOnly Property Adres As String
            Get
                Return "http://127.0.0.1:" & Port & "/wms?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
            End Get
        End Property

        Public ReadOnly Property LiczbaZapytan As Integer
            Get
                Return Volatile.Read(_zapytania)
            End Get
        End Property

        Private Async Function Obsluguj() As Task
            Do
                Dim klient As TcpClient
                Try
                    klient = Await _nasluch.AcceptTcpClientAsync()
                Catch
                    Exit Do
                End Try
                Dim k = klient
                Dim __ = Task.Run(Sub() Odpowiedz(k))
            Loop
        End Function

        Private Sub Odpowiedz(klient As TcpClient)
            Using klient
                Dim s = klient.GetStream()
                Dim bufor(8191) As Byte
                Dim naglowek As New StringBuilder()
                Do
                    Dim n = s.Read(bufor, 0, bufor.Length)
                    If n <= 0 Then Exit Do
                    naglowek.Append(Encoding.ASCII.GetString(bufor, 0, n))
                Loop Until naglowek.ToString().Contains(vbCrLf & vbCrLf)
                Interlocked.Increment(_zapytania)
                Dim calyNaglowek = naglowek.ToString()
                OstatnieNaglowki = calyNaglowek
                Dim wiersz = calyNaglowek.Split({vbCrLf}, StringSplitOptions.None)(0)
                Dim o = If(OdpowiedzZNaglowkami IsNot Nothing, OdpowiedzZNaglowkami(wiersz, calyNaglowek), _odpowiedz(wiersz))
                Dim tresc = o.Item3
                Dim extra = If(DodatkoweNaglowki IsNot Nothing, DodatkoweNaglowki(wiersz, calyNaglowek), "")
                If extra <> "" AndAlso Not extra.EndsWith(vbCrLf, StringComparison.Ordinal) Then extra &= vbCrLf
                Dim naglOdp As String = "HTTP/1.1 " & o.Item1 & " X" & vbCrLf & "Content-Type: " & o.Item2 & vbCrLf & extra &
                                        If(o.Item1 = 304, "", "Content-Length: " & tresc.Length & vbCrLf) &
                                        "Connection: close" & vbCrLf & vbCrLf
                Dim odp = Encoding.ASCII.GetBytes(naglOdp)
                s.Write(odp, 0, odp.Length)
                If o.Item1 <> 304 AndAlso tresc.Length > 0 Then s.Write(tresc, 0, tresc.Length)
            End Using
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            _nasluch.Stop()
        End Sub
    End Class

    Private Shared Function ObrazPng(bok As Integer) As Byte()
        Using bmp As New Bitmap(bok, bok, PixelFormat.Format24bppRgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.Green)
            End Using
            Using ms As New MemoryStream()
                bmp.Save(ms, ImageFormat.Png)
                Return ms.ToArray()
            End Using
        End Using
    End Function

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_Pobieranie_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Sub Usun(folder As String)
        Try
            Directory.Delete(folder, True)
        Catch
        End Try
    End Sub

    ''' <summary>Siatka 3 x 2 segmenty po 64 px, piksel 1 m.</summary>
    Private Shared Function Zadanie(adres As String, folder As String) As ZadaniePobierania
        Return New ZadaniePobierania With {
            .AdresSerwera = adres, .Warstwy = New List(Of String) From {"a"}, .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = New Siatka(New Zasieg(500000, 600000, 500128, 600192), 1.0, 64), .Format = "png", .Folder = folder,
            .IloscProb = 1, .PrzerwaSekundy = 0, .LiczbaWatkow = 3, .LimitCzasuSekundy = 20}
    End Function

    <Fact>
    Public Async Function KompletSegmentow_BezBledowIZPlikiemKoordynat() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(64)
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(200, "image/png", png))
            Try
                'plik tymczasowy przerwanego zapisu z poprzedniego pobierania
                File.WriteAllText(folder & "_01_01.png.0123456789.tmp", "resztka")
                Dim z = Zadanie(serwer.Adres, folder)
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)

                Assert.False(wynik.Przerwano)
                Assert.Equal(6, wynik.Wszystkie)
                Assert.Equal(0, wynik.Nieudane)
                Assert.Equal(6, Directory.GetFiles(folder, "_*.png").Length)
                Assert.Empty(Directory.GetFiles(folder, "*.tmp"))
                Assert.False(File.Exists(folder & "error.txt"))
                Assert.Equal(6, serwer.LiczbaZapytan)
                Dim koordynaty = File.ReadAllLines(folder & "koordynaty.txt")
                Assert.Equal(2 + 6 * 6, koordynaty.Length)
                Assert.Equal("_01_01", koordynaty(2))
                Assert.Contains("bbox=", koordynaty(3))
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function BladZapisuGeoreferencji_NiePrzerywaPobierania() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(64)
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(200, "image/png", png))
            Try
                'folder o nazwie pliku world file segmentu - zapis tego pliku się nie uda
                Directory.CreateDirectory(folder & "_01_01.pngw")
                Dim z = Zadanie(serwer.Adres, folder)
                z.Georeferencja = New OpcjeGeoreferencji With {.WorldFile = True}
                z.IloscProb = 2
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)

                Assert.False(wynik.Przerwano)
                Assert.Equal(1, wynik.Nieudane)
                Assert.Equal(6, Directory.GetFiles(folder, "_*.png").Length)
                Assert.True(File.Exists(folder & "_02_03.pngw"))
                Dim bledy = File.ReadAllText(folder & "error.txt")
                Assert.Contains("_01_01", bledy)
                Assert.Contains("georeferencyjnych", bledy)
                Assert.DoesNotContain("_02_03", bledy)
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function BladSerwera_WszystkieSegmentyWWykazieBledow() As Task
        Dim folder = NowyFolder()
        Dim xml = Encoding.UTF8.GetBytes("<ServiceExceptionReport><ServiceException>Layer not defined</ServiceException></ServiceExceptionReport>")
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(500, "text/xml", xml))
            Try
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(Zadanie(serwer.Adres, folder), Nothing, CancellationToken.None)
                Assert.Equal(6, wynik.Nieudane)
                Dim bledy = File.ReadAllText(folder & "error.txt")
                Assert.Contains("HTTP 500", bledy)
                Assert.Contains("Layer not defined", bledy)
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function Przerwanie_ZaznaczoneWWyniku() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(64)
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(200, "image/png", png)), przerwanie As New CancellationTokenSource()
            Try
                przerwanie.Cancel()
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(Zadanie(serwer.Adres, folder), Nothing, przerwanie.Token)
                Assert.True(wynik.Przerwano)
                Assert.Equal(6, wynik.Nieudane)
                Assert.Contains("przerwano pobieranie", File.ReadAllText(folder & "error.txt"))
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_RefererMaskowanieSekretowIBrakPlikowZeroB() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(256)
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(200, "image/png", png))
            Try
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png?key=Sekret123"
                Dim plikWarstw As New PlikWarstw With {
                    .Typ = TypUslugiMapowej.Xyz,
                    .Adres = szablon,
                    .ZMin = 0,
                    .ZMax = 18,
                    .OdstepMs = 5,
                    .MaksWatkow = 2,
                    .Referer = "https://mapotero.example/"
                }
                plikWarstw.Warstwy.Add(New WarstwaMapy With {.Nazwa = "mapa", .RozmiarPiksela = "1"})
                Dim z = Zadanie(szablon, folder)
                z.Warstwy = New List(Of String) From {"mapa"}
                z.Zbiory = New List(Of String) From {"test_xyz"}
                z.ZrodlaMap("test_xyz") = plikWarstw
                z.ZachowajKafleWmts = True

                Dim wynik = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)
                Assert.Equal(0, wynik.Nieudane)
                Assert.Equal(6, Directory.GetFiles(folder, "_*.png").Length)
                Assert.Contains("Referer: https://mapotero.example/", serwer.OstatnieNaglowki)
                Dim koord = File.ReadAllText(folder & "koordynaty.txt")
                Assert.DoesNotContain("Sekret123", koord)
                Assert.Contains("key=***", koord)
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_Nakladka404BezPlikuZeroB_Podklad404Przerywa() As Task
        Dim folderNakladki = NowyFolder()
        Dim folderPodkladu = NowyFolder()
        Using serwer As New SerwerTestowy(Function(w) Tuple.Create(404, "text/plain", Encoding.ASCII.GetBytes("Brak")))
            Try
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png"
                '1) nakładka zwracająca same 404 kończy się sukcesem i nie zapisuje plików 0 B na dysku
                Dim plikNakladka As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .Nakladka = True, .OdstepMs = 1}
                plikNakladka.Warstwy.Add(New WarstwaMapy With {.Nazwa = "szlak"})
                Dim zN = Zadanie(szablon, folderNakladki)
                zN.Warstwy = New List(Of String) From {"szlak"}
                zN.Zbiory = New List(Of String) From {"szlak_xyz"}
                zN.ZrodlaMap("szlak_xyz") = plikNakladka
                zN.ZachowajKafleWmts = True

                Dim wynikN = Await PobieranieSegmentow.PobierzAsync(zN, Nothing, CancellationToken.None)
                Assert.Equal(0, wynikN.Nieudane)
                Assert.Empty(Directory.GetFiles(Path.Combine(folderNakladki, "_kafle_wmts"), "*.kafel"))

                '2) podkład zwracający same 404 przerywa od razu na teście startowym zamiast tworzyć puste segmenty
                Dim plikPodklad As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .Nakladka = False, .OdstepMs = 1}
                plikPodklad.Warstwy.Add(New WarstwaMapy With {.Nazwa = "baza"})
                Dim zP = Zadanie(szablon, folderPodkladu)
                zP.Warstwy = New List(Of String) From {"baza"}
                zP.Zbiory = New List(Of String) From {"baza_xyz"}
                zP.ZrodlaMap("baza_xyz") = plikPodklad

                Await Assert.ThrowsAsync(Of InvalidOperationException)(
                    Function() PobieranieSegmentow.PobierzAsync(zP, Nothing, CancellationToken.None))
            Finally
                Usun(folderNakladki)
                Usun(folderPodkladu)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_UzupelnianieBrakujacychKafliPrzedScaleniemIOdmowaPrzyNiekomplecie() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(256)
        Dim brama As New Object()
        Dim pierwszyAdres As String = ""
        Dim wadliwyAdres As String = ""
        Dim licznikWadliwego As Integer = 0

        Using serwer As New SerwerTestowy(
            Function(wiersz)
                Dim sciezka = wiersz.Split(" "c)(1)
                SyncLock brama
                    If pierwszyAdres = "" Then
                        pierwszyAdres = sciezka
                    ElseIf wadliwyAdres = "" AndAlso sciezka <> pierwszyAdres Then
                        wadliwyAdres = sciezka
                    End If
                    If sciezka = wadliwyAdres Then
                        licznikWadliwego += 1
                        If licznikWadliwego <= 4 Then
                            Return Tuple.Create(500, "text/plain", Encoding.ASCII.GetBytes("Chwilowy blad"))
                        End If
                    End If
                End SyncLock
                Return Tuple.Create(200, "image/png", png)
            End Function)
            Try
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png"
                Dim plikWarstw As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .OdstepMs = 1}
                plikWarstw.Warstwy.Add(New WarstwaMapy With {.Nazwa = "mapa"})
                Dim z = Zadanie(szablon, folder)
                z.Warstwy = New List(Of String) From {"mapa"}
                z.Zbiory = New List(Of String) From {"test_uzup"}
                z.ZrodlaMap("test_uzup") = plikWarstw
                z.IloscProb = 1 'jedna próba na poziomie segmentów - uzupełnienie musi zajść wewnątrz mozaiki przed scaleniem!

                Dim wynik = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)
                Dim bledyTekst = If(File.Exists(folder & "error.txt"), File.ReadAllText(folder & "error.txt"), "")
                Assert.True(wynik.Nieudane = 0, "Niepowodzenia: " & wynik.Nieudane & " | " & bledyTekst)
                Assert.Equal(6, Directory.GetFiles(folder, "_*.png").Length)

                'stały błąd jednego z kafli po wyczerpaniu rund uzupełniania nie może zapisać niekompletnego segmentu
                Dim folderNiekompl = NowyFolder()
                Try
                    Dim liczKafel = 0
                    Using sBlad As New SerwerTestowy(
                        Function(wiersz)
                            Dim n = Interlocked.Increment(liczKafel)
                            If n = 1 Then Return Tuple.Create(200, "image/png", png)
                            Return Tuple.Create(500, "text/plain", Encoding.ASCII.GetBytes("Blad trwaly"))
                        End Function)
                        Dim szBlad As String = "http://127.0.0.1:" & sBlad.Port & "/tiles/{z}/{x}/{y}.png"
                        plikWarstw.Adres = szBlad
                        Dim zN = Zadanie(szBlad, folderNiekompl)
                        zN.Warstwy = New List(Of String) From {"mapa"}
                        zN.Zbiory = New List(Of String) From {"test_uzup"}
                        zN.ZrodlaMap("test_uzup") = plikWarstw
                        zN.Siatka = New Siatka(New Zasieg(500000, 600000, 500512, 600512), 1.0, 512)
                        zN.IloscProb = 1
                        Dim wynikBlad = Await PobieranieSegmentow.PobierzAsync(zN, Nothing, CancellationToken.None)
                        Assert.Equal(1, wynikBlad.Nieudane)
                        'nie zapisano żadnego wybrakowanego pliku segmentu na dysku
                        Assert.Empty(Directory.GetFiles(folderNiekompl, "_*.png"))
                    End Using
                Finally
                    Usun(folderNiekompl)
                End Try
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_WalidacjaCacheETagIUszkodzonyPlikNaDysku() As Task
        Dim folder = NowyFolder()
        Dim pngZielony = ObrazPng(256)
        Dim etagAktualny As String = """v1"""
        Dim wyslaneIfNoneMatch As New List(Of String)
        Dim liczba304 As Integer = 0

        Using serwer As New SerwerTestowy(Function(wiersz) Tuple.Create(200, "image/png", pngZielony))
            serwer.OdpowiedzZNaglowkami =
                Function(wiersz, nagl)
                    For Each l In nagl.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                        If l.StartsWith("If-None-Match:", StringComparison.OrdinalIgnoreCase) Then
                            SyncLock wyslaneIfNoneMatch
                                wyslaneIfNoneMatch.Add(l)
                            End SyncLock
                            If l.Contains(etagAktualny) Then
                                Interlocked.Increment(liczba304)
                                Return Tuple.Create(304, "image/png", New Byte() {})
                            End If
                        End If
                    Next
                    Return Tuple.Create(200, "image/png", pngZielony)
                End Function
            serwer.DodatkoweNaglowki = Function(wiersz, nagl) "ETag: " & etagAktualny
            Try
                '1. pierwsze pobranie zapisuje .kafel + .meta z ETag="v1"
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png"
                Dim plikWarstw As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .OdstepMs = 1}
                plikWarstw.Warstwy.Add(New WarstwaMapy With {.Nazwa = "mapa"})
                Dim z = Zadanie(szablon, folder)
                z.Warstwy = New List(Of String) From {"mapa"}
                z.Zbiory = New List(Of String) From {"test_cache"}
                z.ZrodlaMap("test_cache") = plikWarstw
                z.ZachowajKafleWmts = True
                z.Siatka = New Siatka(New Zasieg(500000, 600000, 500512, 600512), 1.0, 256)

                Dim w1 = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)
                Assert.Equal(0, w1.Nieudane)
                Dim plikiMeta = Directory.GetFiles(Path.Combine(folder, "_kafle_wmts"), "*.meta")
                Assert.NotEmpty(plikiMeta)
                Assert.True(Directory.GetFiles(Path.Combine(folder, "_kafle_wmts"), "*.kafel").Length >= 2)

                '2. kolejne pobranie z wymuszoną walidacją (WiekWalidacjiCacheSekundy = 0) wysyła If-None-Match -> 304 Not Modified
                'a uszkodzony plik .kafel na dysku jest usuwany i pobierany ponownie (HTTP 200)
                Dim z2 = Zadanie(szablon, folder)
                z2.Warstwy = New List(Of String) From {"mapa"}
                z2.Zbiory = New List(Of String) From {"test_cache"}
                z2.ZrodlaMap("test_cache") = plikWarstw
                z2.ZachowajKafleWmts = True
                z2.Siatka = z.Siatka
                z2.WiekWalidacjiCacheSekundy = 0
                For Each f In Directory.GetFiles(folder, "_*.png")
                    File.Delete(f)
                Next
                Dim kafelUszk = Directory.GetFiles(Path.Combine(folder, "_kafle_wmts"), "*.kafel").First()
                File.WriteAllBytes(kafelUszk, New Byte() {1, 2, 3, 4})

                Dim w2 = Await PobieranieSegmentow.PobierzAsync(z2, Nothing, CancellationToken.None)
                Assert.Equal(0, w2.Nieudane)
                Assert.True(File.Exists(folder & "_01_01.png"))
                Assert.Contains(wyslaneIfNoneMatch, Function(x) x.Contains("""v1"""))
                Assert.True(liczba304 >= 1, "Oczekiwano co najmniej jednej odpowiedzi 304 Not Modified dla aktualnego cache.")
                Assert.True(File.ReadAllBytes(kafelUszk).Length > 8)
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_Podklad404ZastepowanyKaflemZPoziomuWyzej() As Task
        Dim folder = NowyFolder()
        Dim png = ObrazPng(256)
        Dim brama As New Object()
        Dim pierwszy As String = ""
        Dim brakujacy As String = ""
        Dim poziomTestu As String = ""
        Dim sciezki As New List(Of String)
        Using serwer As New SerwerTestowy(
            Function(wiersz)
                Dim sciezka = wiersz.Split(" "c)(1)
                Dim poziom = sciezka.Split("/"c)(2)
                SyncLock brama
                    sciezki.Add(sciezka)
                    If pierwszy = "" Then
                        pierwszy = sciezka
                        poziomTestu = poziom
                    ElseIf brakujacy = "" AndAlso poziom = poziomTestu AndAlso sciezka <> pierwszy Then
                        brakujacy = sciezka
                    End If
                    If sciezka = brakujacy Then Return Tuple.Create(404, "text/plain", Encoding.ASCII.GetBytes("Brak"))
                End SyncLock
                Return Tuple.Create(200, "image/png", png)
            End Function)
            Try
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png"
                Dim plikWarstw As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .OdstepMs = 1}
                plikWarstw.Warstwy.Add(New WarstwaMapy With {.Nazwa = "mapa"})
                Dim z = Zadanie(szablon, folder)
                z.Warstwy = New List(Of String) From {"mapa"}
                z.Zbiory = New List(Of String) From {"test_404"}
                z.ZrodlaMap("test_404") = plikWarstw

                Dim wynik = Await PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None)
                Assert.Equal(0, wynik.Nieudane)
                Assert.NotEqual("", brakujacy)
                'brakujący kafel zastąpiony kaflem z poziomu o jeden mniej szczegółowego
                Dim poziomWyzej = (Integer.Parse(poziomTestu) - 1).ToString()
                SyncLock brama
                    Assert.Contains(sciezki, Function(s) s.Split("/"c)(2) = poziomWyzej)
                End SyncLock
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

    <Fact>
    Public Async Function PobieranieXyz_Http403BezPonawianiaZPodpowiedziaReferer() As Task
        Dim folder = NowyFolder()
        Dim zapytania As Integer = 0
        Using serwer As New SerwerTestowy(
            Function(wiersz)
                Interlocked.Increment(zapytania)
                Return Tuple.Create(403, "text/plain", Encoding.ASCII.GetBytes("Forbidden"))
            End Function)
            Try
                Dim szablon As String = "http://127.0.0.1:" & serwer.Port & "/tiles/{z}/{x}/{y}.png"
                Dim plikWarstw As New PlikWarstw With {.Typ = TypUslugiMapowej.Xyz, .Adres = szablon, .OdstepMs = 1}
                plikWarstw.Warstwy.Add(New WarstwaMapy With {.Nazwa = "mapa"})
                Dim z = Zadanie(szablon, folder)
                z.Warstwy = New List(Of String) From {"mapa"}
                z.Zbiory = New List(Of String) From {"test_403"}
                z.ZrodlaMap("test_403") = plikWarstw

                Dim ex = Await Assert.ThrowsAsync(Of InvalidOperationException)(
                    Function() PobieranieSegmentow.PobierzAsync(z, Nothing, CancellationToken.None))
                Assert.Contains("HTTP 403", ex.Message)
                Assert.Contains("referer", ex.Message)
                'test startowy sprawdza najwyżej 3 kafle, każdy jednym zapytaniem (bez ponawiania)
                Assert.InRange(zapytania, 1, 3)
            Finally
                Usun(folder)
            End Try
        End Using
    End Function

End Class
