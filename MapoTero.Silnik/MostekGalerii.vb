'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
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

Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>Wynik próby dodania warstwy lub zestawu warstw wywołanej ze strony internetowej galerii.</summary>
Public Class WynikMostkaGalerii
    Public Property Ok As Boolean
    ''' <summary>Kod stanu: "dodano", "juz_wybrana", "ustawiono", "nie_znaleziono", "zajety", "blad".</summary>
    Public Property Status As String = ""
    Public Property Komunikat As String = ""
    Public Property DodanoLiczba As Integer
    Public Property Pominiete As New List(Of String)()

    Public Shared Function Dodano(komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = True, .Status = "dodano", .Komunikat = komunikat, .DodanoLiczba = 1}
    End Function

    Public Shared Function JuzWybrana(komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = True, .Status = "juz_wybrana", .Komunikat = komunikat}
    End Function

    Public Shared Function Ustawiono(komunikat As String, dodanoLiczba As Integer, Optional pominiete As IEnumerable(Of String) = Nothing) As WynikMostkaGalerii
        Dim w As New WynikMostkaGalerii With {
            .Ok = True,
            .Status = "ustawiono",
            .Komunikat = komunikat,
            .DodanoLiczba = dodanoLiczba
        }
        If pominiete IsNot Nothing Then w.Pominiete.AddRange(pominiete)
        Return w
    End Function

    Public Shared Function Blad(status As String, komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = False, .Status = status, .Komunikat = komunikat}
    End Function
End Class

''' <summary>Informacja o zaznaczonym obszarze lub środku mapy w MapoTero przekazywana galerii WWW.</summary>
Public Class DaneObszaruGalerii
    <JsonPropertyName("lat")> Public Property Lat As Double
    <JsonPropertyName("lon")> Public Property Lon As Double
    <JsonPropertyName("piksel")> Public Property Piksel As Double = 1.0
    <JsonPropertyName("maSiatke")> Public Property MaSiatke As Boolean
    <JsonPropertyName("uklad")> Public Property Uklad As String = ""
    <JsonPropertyName("bboxWgs84")> Public Property BboxWgs84 As Double()
End Class

''' <summary>Bieżący stan programu (warstwy kanapki + zaznaczony obszar) zwracany przez GET /stan.</summary>
Public Class StanMostkaGalerii
    <JsonPropertyName("ok")> Public Property Ok As Boolean = True
    <JsonPropertyName("zestaw")> Public Property Zestaw As DaneZestawuGalerii = New DaneZestawuGalerii()
    <JsonPropertyName("obszar")> Public Property Obszar As DaneObszaruGalerii = New DaneObszaruGalerii()
End Class

''' <summary>Parametry żądania POST /podglad ze strony galerii WWW.</summary>
Public Class ZadaniePodgladuGalerii
    <JsonPropertyName("warstwy")> Public Property Warstwy As New List(Of PozycjaZestawuGalerii)()
    <JsonPropertyName("lat")> Public Property Lat As Double?
    <JsonPropertyName("lon")> Public Property Lon As Double?
    <JsonPropertyName("piksel")> Public Property Piksel As Double?
End Class

''' <summary>Wynik renderowania próbki podglądu dla strony galerii WWW.</summary>
Public Class WynikPodgladuGalerii
    Public Property Ok As Boolean
    Public Property PngBytes As Byte()
    Public Property Komunikat As String = ""
    Public Property Lat As Double
    Public Property Lon As Double
    Public Property Piksel As Double
    Public Property Brakujace As New List(Of String)()

    Public Shared Function Sukces(pngBytes As Byte(), lat As Double, lon As Double, piksel As Double,
                                  Optional brakujace As IEnumerable(Of String) = Nothing,
                                  Optional komunikat As String = "") As WynikPodgladuGalerii
        Dim w As New WynikPodgladuGalerii With {
            .Ok = True,
            .PngBytes = pngBytes,
            .Lat = lat,
            .Lon = lon,
            .Piksel = piksel,
            .Komunikat = komunikat
        }
        If brakujace IsNot Nothing Then w.Brakujace.AddRange(brakujace)
        Return w
    End Function

    Public Shared Function Blad(komunikat As String) As WynikPodgladuGalerii
        Return New WynikPodgladuGalerii With {.Ok = False, .Komunikat = komunikat}
    End Function
End Class

''' <summary>Konfiguracja punktów obsługi żądań dla <see cref="MostekGalerii"/>.</summary>
Public Class OpcjeMostkaGalerii
    Public Property ObslugaDodaj As Func(Of String, String, WynikMostkaGalerii)
    Public Property ObslugaPobierzStan As Func(Of StanMostkaGalerii)
    Public Property ObslugaZastosujZestaw As Func(Of DaneZestawuGalerii, WynikMostkaGalerii)
    Public Property ObslugaPodglad As Func(Of ZadaniePodgladuGalerii, CancellationToken, Task(Of WynikPodgladuGalerii))
    Public Property WersjaProgramu As String = ""
End Class

''' <summary>
''' Lekki lokalny serwer HTTP (127.0.0.1, losowy port, jednorazowy token sesji) pozwalający
''' statycznej stronie galerii warstw na GitHub Pages przekazać wybraną warstwę lub całą kanapkę
''' do działającego MapoTero oraz pobrać bieżący stan i podgląd na żywo.
''' Nie wymaga uprawnień administratora ani wpisów w rejestrze Windows.
''' </summary>
Public NotInheritable Class MostekGalerii
    Implements IDisposable

    Private Const MaksRozmiarZadaniaBajtow As Integer = 65536

    Private ReadOnly _listener As HttpListener
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _opcje As OpcjeMostkaGalerii
    Private ReadOnly _petla As Task
    Private ReadOnly _blokadaPodgladu As New Object()
    Private _ctsPodglad As CancellationTokenSource
    Private _zamkniety As Boolean

    Private Shared ReadOnly OpcjeJson As JsonSerializerOptions = New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True,
        .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }

    Public ReadOnly Property Port As Integer
    Public ReadOnly Property Token As String

    Public Sub New(obslugaDodaj As Func(Of String, String, WynikMostkaGalerii))
        Me.New(New OpcjeMostkaGalerii With {.ObslugaDodaj = obslugaDodaj})
    End Sub

    Public Sub New(opcje As OpcjeMostkaGalerii)
        If opcje Is Nothing Then Throw New ArgumentNullException(NameOf(opcje))
        If opcje.ObslugaDodaj Is Nothing Then Throw New ArgumentException("Brak obsługi dodawania warstwy (ObslugaDodaj).", NameOf(opcje))
        _opcje = opcje
        Token = Guid.NewGuid().ToString("N")
        Port = ZnajdzWolnyPort()

        _listener = New HttpListener()
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/")
        _listener.Start()
        _petla = Task.Run(AddressOf PetlaNasluchuAsync)
    End Sub

    ''' <summary>Dopisuje parametry połączenia (port i token) do adresu strony galerii.</summary>
    Public Function ZbudujAdresGalerii(adresBazowy As String) As String
        If String.IsNullOrWhiteSpace(adresBazowy) Then Return ""
        Dim separator = If(adresBazowy.Contains("?"c), "&", "?")
        Return $"{adresBazowy}{separator}port={Port}&token={Uri.EscapeDataString(Token)}"
    End Function

    ''' <summary>Sprawdza, czy źródło (Origin) jest dozwolone: oficjalny GitHub Pages projektu albo lokalny podgląd.</summary>
    Public Shared Function CzyDozwolonyOrigin(origin As String) As Boolean
        If String.IsNullOrWhiteSpace(origin) Then Return False
        Dim uri As Uri = Nothing
        If Not Uri.TryCreate(origin, UriKind.Absolute, uri) Then Return False
        If String.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) AndAlso
           String.Equals(uri.Host, "eksploracja.github.io", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If
        If (String.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) OrElse
            String.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)) AndAlso
           (String.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) OrElse
            String.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)) Then
            Return True
        End If
        Return False
    End Function

    Private Shared Function ZnajdzWolnyPort() As Integer
        Dim tcp As New TcpListener(IPAddress.Loopback, 0)
        tcp.Start()
        Try
            Return CType(tcp.LocalEndpoint, IPEndPoint).Port
        Finally
            tcp.Stop()
        End Try
    End Function

    Private Async Function PetlaNasluchuAsync() As Task
        While Not _cts.IsCancellationRequested AndAlso _listener.IsListening
            Dim ctx As HttpListenerContext = Nothing
            Try
                ctx = Await _listener.GetContextAsync().ConfigureAwait(False)
            Catch ex As HttpListenerException
                Exit While
            Catch ex As ObjectDisposedException
                Exit While
            Catch
                If _cts.IsCancellationRequested Then Exit While
                Continue While
            End Try

            If ctx IsNot Nothing Then
                Dim lokalnyCtx = ctx
                Dim t = Task.Run(Async Function()
                                     Try
                                         Await ObsluzZadanieAsync(lokalnyCtx).ConfigureAwait(False)
                                     Catch
                                         Try
                                             lokalnyCtx.Response.StatusCode = 500
                                             lokalnyCtx.Response.Close()
                                         Catch
                                         End Try
                                     End Try
                                 End Function)
            End If
        End While
    End Function

    Private Async Function ObsluzZadanieAsync(ctx As HttpListenerContext) As Task
        Dim req = ctx.Request
        Dim resp = ctx.Response

        Dim origin = req.Headers("Origin")
        If CzyDozwolonyOrigin(origin) Then
            resp.Headers("Access-Control-Allow-Origin") = origin
            resp.Headers("Vary") = "Origin"
        End If
        resp.Headers("Access-Control-Allow-Methods") = "GET, POST, OPTIONS"
        resp.Headers("Access-Control-Allow-Headers") = "Content-Type"
        resp.Headers("Access-Control-Expose-Headers") = "X-MapoTero-Lat, X-MapoTero-Lon, X-MapoTero-Piksel, X-MapoTero-Brakujace"
        ' Wymagane przez Chrome/Edge (Private Network Access) przy żądaniach https:// -> http://127.0.0.1
        resp.Headers("Access-Control-Allow-Private-Network") = "true"

        If String.Equals(req.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase) Then
            resp.StatusCode = 204
            resp.Close()
            Return
        End If

        Dim tokenZadania = req.QueryString("token")
        If Not String.Equals(tokenZadania, Token, StringComparison.Ordinal) Then
            WyslijJson(resp, 403, New With {.ok = False, .status = "brak_dostepu", .komunikat = "Nieprawidłowy token sesji."})
            Return
        End If

        Dim sciezka = (If(req.Url?.AbsolutePath, "")).TrimEnd("/"c).ToLowerInvariant()
        Select Case sciezka
            Case "/ping"
                Dim funkcje As New List(Of String) From {"dodaj"}
                If _opcje.ObslugaPobierzStan IsNot Nothing Then funkcje.Add("stan")
                If _opcje.ObslugaZastosujZestaw IsNot Nothing Then funkcje.Add("zestaw")
                If _opcje.ObslugaPodglad IsNot Nothing Then funkcje.Add("podglad")
                WyslijJson(resp, 200, New With {
                    .ok = True,
                    .program = "MapoTero",
                    .wersja = If(_opcje.WersjaProgramu, ""),
                    .funkcje = funkcje
                })

            Case "/dodaj"
                Dim zbior = If(req.QueryString("zbior"), "").Trim()
                Dim warstwa = If(req.QueryString("warstwa"), "").Trim()
                If zbior = "" OrElse warstwa = "" Then
                    WyslijJson(resp, 400, New With {.ok = False, .status = "blad_parametrow", .komunikat = "Brak nazwy zbioru lub warstwy."})
                    Return
                End If

                Dim wynik As WynikMostkaGalerii
                Try
                    wynik = _opcje.ObslugaDodaj.Invoke(zbior, warstwa)
                Catch ex As Exception
                    wynik = WynikMostkaGalerii.Blad("blad", ex.Message)
                End Try
                If wynik Is Nothing Then
                    wynik = WynikMostkaGalerii.Blad("blad", "Brak odpowiedzi z programu.")
                End If
                Dim kod = If(wynik.Ok, 200, If(wynik.Status = "nie_znaleziono", 404, 409))
                WyslijJson(resp, kod, New With {.ok = wynik.Ok, .status = wynik.Status, .komunikat = wynik.Komunikat})

            Case "/stan"
                If _opcje.ObslugaPobierzStan Is Nothing Then
                    WyslijJson(resp, 501, New With {.ok = False, .status = "nieobslugiwane", .komunikat = "Ta wersja programu nie udostępnia odczytu stanu."})
                    Return
                End If
                Dim stan As StanMostkaGalerii
                Try
                    stan = _opcje.ObslugaPobierzStan.Invoke()
                Catch ex As Exception
                    WyslijJson(resp, 500, New With {.ok = False, .status = "blad", .komunikat = ex.Message})
                    Return
                End Try
                If stan Is Nothing Then stan = New StanMostkaGalerii()
                WyslijJson(resp, 200, stan)

            Case "/zestaw"
                If Not String.Equals(req.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) Then
                    WyslijJson(resp, 405, New With {.ok = False, .status = "zla_metoda", .komunikat = "Endpoint /zestaw wymaga metody POST."})
                    Return
                End If
                If _opcje.ObslugaZastosujZestaw Is Nothing Then
                    WyslijJson(resp, 501, New With {.ok = False, .status = "nieobslugiwane", .komunikat = "Ta wersja programu nie obsługuje zapisu całego zestawu."})
                    Return
                End If

                Dim bodyRez = Await WczytajCialoAsync(req).ConfigureAwait(False)
                If Not bodyRez.Ok Then
                    WyslijJson(resp, bodyRez.KodBledu, New With {.ok = False, .status = "blad_parametrow", .komunikat = bodyRez.Blad})
                    Return
                End If

                Dim dane As DaneZestawuGalerii
                Try
                    dane = PrzekazZestawu.ZTekstu(bodyRez.Tekst)
                Catch ex As InvalidDataException
                    WyslijJson(resp, 400, New With {.ok = False, .status = "blad_parametrow", .komunikat = ex.Message})
                    Return
                End Try

                Dim wynik As WynikMostkaGalerii
                Try
                    wynik = _opcje.ObslugaZastosujZestaw.Invoke(dane)
                Catch ex As Exception
                    wynik = WynikMostkaGalerii.Blad("blad", ex.Message)
                End Try
                If wynik Is Nothing Then
                    wynik = WynikMostkaGalerii.Blad("blad", "Brak odpowiedzi z programu.")
                End If
                Dim kod = If(wynik.Ok, 200, If(wynik.Status = "nie_znaleziono", 404, 409))
                WyslijJson(resp, kod, New With {
                    .ok = wynik.Ok,
                    .status = wynik.Status,
                    .komunikat = wynik.Komunikat,
                    .dodano = wynik.DodanoLiczba,
                    .pominieto = wynik.Pominiete
                })

            Case "/podglad"
                If Not String.Equals(req.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) Then
                    WyslijJson(resp, 405, New With {.ok = False, .status = "zla_metoda", .komunikat = "Endpoint /podglad wymaga metody POST."})
                    Return
                End If
                If _opcje.ObslugaPodglad Is Nothing Then
                    WyslijJson(resp, 501, New With {.ok = False, .status = "nieobslugiwane", .komunikat = "Ta wersja programu nie obsługuje zdalnego podglądu."})
                    Return
                End If

                Dim bodyRez = Await WczytajCialoAsync(req).ConfigureAwait(False)
                If Not bodyRez.Ok Then
                    WyslijJson(resp, bodyRez.KodBledu, New With {.ok = False, .status = "blad_parametrow", .komunikat = bodyRez.Blad})
                    Return
                End If

                Dim zadanie As ZadaniePodgladuGalerii = Nothing
                Try
                    zadanie = JsonSerializer.Deserialize(Of ZadaniePodgladuGalerii)(bodyRez.Tekst, OpcjeJson)
                Catch ex As JsonException
                    WyslijJson(resp, 400, New With {.ok = False, .status = "blad_parametrow", .komunikat = "Nieprawidłowy JSON zadania podglądu."})
                    Return
                End Try
                If zadanie Is Nothing OrElse zadanie.Warstwy Is Nothing OrElse zadanie.Warstwy.Count = 0 Then
                    WyslijJson(resp, 400, New With {.ok = False, .status = "blad_parametrow", .komunikat = "Brak warstw do wyrenderowania podglądu."})
                    Return
                End If

                Dim mojCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token)
                SyncLock _blokadaPodgladu
                    _ctsPodglad?.Cancel()
                    _ctsPodglad = mojCts
                End SyncLock

                Dim wynikPodgladu As WynikPodgladuGalerii = Nothing
                Try
                    wynikPodgladu = Await _opcje.ObslugaPodglad.Invoke(zadanie, mojCts.Token).ConfigureAwait(False)
                Catch ex As OperationCanceledException
                    WyslijJson(resp, 409, New With {.ok = False, .status = "anulowano", .komunikat = "Nowsze żądanie podglądu zastąpiło poprzednie."})
                    Return
                Catch ex As Exception
                    WyslijJson(resp, 500, New With {.ok = False, .status = "blad", .komunikat = ex.Message})
                    Return
                Finally
                    SyncLock _blokadaPodgladu
                        If _ctsPodglad Is mojCts Then _ctsPodglad = Nothing
                    End SyncLock
                    mojCts.Dispose()
                End Try

                If wynikPodgladu Is Nothing OrElse Not wynikPodgladu.Ok OrElse wynikPodgladu.PngBytes Is Nothing Then
                    Dim kom = If(wynikPodgladu IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(wynikPodgladu.Komunikat),
                                 wynikPodgladu.Komunikat, "Nie udało się wygenerować podglądu kanapki.")
                    WyslijJson(resp, 409, New With {.ok = False, .status = "blad", .komunikat = kom})
                    Return
                End If

                resp.Headers("X-MapoTero-Lat") = wynikPodgladu.Lat.ToString("0.#####", CultureInfo.InvariantCulture)
                resp.Headers("X-MapoTero-Lon") = wynikPodgladu.Lon.ToString("0.#####", CultureInfo.InvariantCulture)
                resp.Headers("X-MapoTero-Piksel") = wynikPodgladu.Piksel.ToString("0.###", CultureInfo.InvariantCulture)
                If wynikPodgladu.Brakujace IsNot Nothing AndAlso wynikPodgladu.Brakujace.Count > 0 Then
                    resp.Headers("X-MapoTero-Brakujace") = Uri.EscapeDataString(String.Join("|", wynikPodgladu.Brakujace))
                End If
                resp.StatusCode = 200
                resp.ContentType = "image/png"
                resp.ContentLength64 = wynikPodgladu.PngBytes.Length
                resp.OutputStream.Write(wynikPodgladu.PngBytes, 0, wynikPodgladu.PngBytes.Length)
                resp.Close()

            Case Else
                WyslijJson(resp, 404, New With {.ok = False, .status = "nieznana_sciezka", .komunikat = "Nieznany adres."})
        End Select
    End Function

    Private Shared Async Function WczytajCialoAsync(req As HttpListenerRequest) As Task(Of (Ok As Boolean, Tekst As String, KodBledu As Integer, Blad As String))
        If req.ContentLength64 > MaksRozmiarZadaniaBajtow Then
            Return (False, "", 413, "Przesłany przepis kanapki jest zbyt duży.")
        End If
        Using ms As New MemoryStream()
            Dim bufor(4095) As Byte
            Dim suma As Integer = 0
            While True
                Dim przeczytano = Await req.InputStream.ReadAsync(bufor, 0, bufor.Length).ConfigureAwait(False)
                If przeczytano <= 0 Then Exit While
                suma += przeczytano
                If suma > MaksRozmiarZadaniaBajtow Then
                    Return (False, "", 413, "Przesłany przepis kanapki jest zbyt duży.")
                End If
                ms.Write(bufor, 0, przeczytano)
            End While
            Return (True, Encoding.UTF8.GetString(ms.ToArray()), 200, "")
        End Using
    End Function

    Private Shared Sub WyslijJson(resp As HttpListenerResponse, statusCode As Integer, obiekt As Object)
        Dim bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obiekt, OpcjeJson))
        resp.StatusCode = statusCode
        resp.ContentType = "application/json; charset=utf-8"
        resp.ContentLength64 = bytes.Length
        resp.OutputStream.Write(bytes, 0, bytes.Length)
        resp.Close()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _zamkniety Then Return
        _zamkniety = True
        _cts.Cancel()
        SyncLock _blokadaPodgladu
            Try
                _ctsPodglad?.Cancel()
            Catch
            End Try
        End SyncLock
        Try
            If _listener.IsListening Then _listener.Stop()
            _listener.Close()
        Catch
        End Try
        _cts.Dispose()
    End Sub

End Class
