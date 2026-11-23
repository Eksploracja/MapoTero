'Copyright (C) <2015>  pajakt
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
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Wynik próby dodania warstwy wywołanej ze strony internetowej galerii.</summary>
Public Class WynikMostkaGalerii
    Public Property Ok As Boolean
    ''' <summary>Kod stanu: "dodano", "juz_wybrana", "nie_znaleziono", "zajety", "blad".</summary>
    Public Property Status As String = ""
    Public Property Komunikat As String = ""

    Public Shared Function Dodano(komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = True, .Status = "dodano", .Komunikat = komunikat}
    End Function

    Public Shared Function JuzWybrana(komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = True, .Status = "juz_wybrana", .Komunikat = komunikat}
    End Function

    Public Shared Function Blad(status As String, komunikat As String) As WynikMostkaGalerii
        Return New WynikMostkaGalerii With {.Ok = False, .Status = status, .Komunikat = komunikat}
    End Function
End Class

''' <summary>
''' Lekki lokalny serwer HTTP (127.0.0.1, losowy port, jednorazowy token sesji) pozwalający
''' statycznej stronie galerii warstw na GitHub Pages przekazać wybraną warstwę do działającego MapoTero.
''' Nie wymaga uprawnień administratora ani wpisów w rejestrze Windows.
''' </summary>
Public NotInheritable Class MostekGalerii
    Implements IDisposable

    Private ReadOnly _listener As HttpListener
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _obslugaDodaj As Func(Of String, String, WynikMostkaGalerii)
    Private ReadOnly _petla As Task
    Private _zamkniety As Boolean

    Public ReadOnly Property Port As Integer
    Public ReadOnly Property Token As String

    Public Sub New(obslugaDodaj As Func(Of String, String, WynikMostkaGalerii))
        If obslugaDodaj Is Nothing Then Throw New ArgumentNullException(NameOf(obslugaDodaj))
        _obslugaDodaj = obslugaDodaj
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
                Try
                    ObsluzZadanie(ctx)
                Catch
                    Try
                        ctx.Response.StatusCode = 500
                        ctx.Response.Close()
                    Catch
                    End Try
                End Try
            End If
        End While
    End Function

    Private Sub ObsluzZadanie(ctx As HttpListenerContext)
        Dim req = ctx.Request
        Dim resp = ctx.Response

        Dim origin = req.Headers("Origin")
        If CzyDozwolonyOrigin(origin) Then
            resp.Headers("Access-Control-Allow-Origin") = origin
            resp.Headers("Vary") = "Origin"
        End If
        resp.Headers("Access-Control-Allow-Methods") = "GET, POST, OPTIONS"
        resp.Headers("Access-Control-Allow-Headers") = "Content-Type"
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
                WyslijJson(resp, 200, New With {.ok = True, .program = "MapoTero"})

            Case "/dodaj"
                Dim zbior = If(req.QueryString("zbior"), "").Trim()
                Dim warstwa = If(req.QueryString("warstwa"), "").Trim()
                If zbior = "" OrElse warstwa = "" Then
                    WyslijJson(resp, 400, New With {.ok = False, .status = "blad_parametrow", .komunikat = "Brak nazwy zbioru lub warstwy."})
                    Return
                End If

                Dim wynik As WynikMostkaGalerii
                Try
                    wynik = _obslugaDodaj(zbior, warstwa)
                Catch ex As Exception
                    wynik = WynikMostkaGalerii.Blad("blad", ex.Message)
                End Try
                If wynik Is Nothing Then
                    wynik = WynikMostkaGalerii.Blad("blad", "Brak odpowiedzi z programu.")
                End If
                Dim kod = If(wynik.Ok, 200, If(wynik.Status = "nie_znaleziono", 404, 409))
                WyslijJson(resp, kod, New With {.ok = wynik.Ok, .status = wynik.Status, .komunikat = wynik.Komunikat})

            Case Else
                WyslijJson(resp, 404, New With {.ok = False, .status = "nieznana_sciezka", .komunikat = "Nieznany adres."})
        End Select
    End Sub

    Private Shared Sub WyslijJson(resp As HttpListenerResponse, statusCode As Integer, obiekt As Object)
        Dim opcje As New JsonSerializerOptions With {
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }
        Dim bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obiekt, opcje))
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
        Try
            If _listener.IsListening Then _listener.Stop()
            _listener.Close()
        Catch
        End Try
        _cts.Dispose()
    End Sub

End Class
