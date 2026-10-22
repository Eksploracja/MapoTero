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

        Public Sub New(odpowiedz As Func(Of String, Tuple(Of Integer, String, Byte())))
            _odpowiedz = odpowiedz
            _nasluch.Start()
            Task.Run(AddressOf Obsluguj)
        End Sub

        Public ReadOnly Property Adres As String
            Get
                Return "http://127.0.0.1:" & DirectCast(_nasluch.LocalEndpoint, IPEndPoint).Port & "/wms?request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="
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
                Dim wiersz = naglowek.ToString().Split({vbCrLf}, StringSplitOptions.None)(0)
                Dim o = _odpowiedz(wiersz)
                Dim tresc = o.Item3
                Dim odp = Encoding.ASCII.GetBytes("HTTP/1.1 " & o.Item1 & " X" & vbCrLf & "Content-Type: " & o.Item2 & vbCrLf &
                                                  "Content-Length: " & tresc.Length & vbCrLf & "Connection: close" & vbCrLf & vbCrLf)
                s.Write(odp, 0, odp.Length)
                s.Write(tresc, 0, tresc.Length)
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

End Class
