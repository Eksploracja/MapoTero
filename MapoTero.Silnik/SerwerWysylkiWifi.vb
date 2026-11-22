' =====================================================================
' Serwer HTTP transferu plików map przez lokalną sieć Wi-Fi / LAN
' Czysty TcpListener w przestrzeni użytkownika (brak praw administratora).
' Bezpieczny token hex per sesja, obsługa Range (RFC 9110) i RFC 5987.
' =====================================================================

Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Public NotInheritable Class SerwerWysylkiWifi
    Implements IDisposable

    Private Const ROZMIAR_BUFORA As Integer = 65536 ' 64 KB
    Private Const MAX_LINII_NAGLOWKA As Integer = 64
    Private Const MAX_DLUGOSC_LINII As Integer = 4096
    Private Const TIMEOUT_NAGLOWKOW_SEK As Integer = 15
    Private Const TIMEOUT_ZAPISU_SEK As Integer = 60

    Private ReadOnly _sciezkaPliku As String
    Private ReadOnly _nazwaPliku As String
    Private ReadOnly _dlugoscPliku As Long
    Private ReadOnly _token As String
    Private ReadOnly _naglowekDisposition As String
    Private ReadOnly _stronaHtmlBajty As Byte()

    Private _listener As TcpListener
    Private _cts As CancellationTokenSource
    Private _zadanieNasluchu As Task
    Private _aktywneTransfery As Integer
    Private _zwolniony As Boolean
    Private _port As Integer

    Public ReadOnly Property Port As Integer
        Get
            Return _port
        End Get
    End Property

    Public ReadOnly Property Token As String
        Get
            Return _token
        End Get
    End Property

    Public ReadOnly Property NazwaPliku As String
        Get
            Return _nazwaPliku
        End Get
    End Property

    Public ReadOnly Property DlugoscPliku As Long
        Get
            Return _dlugoscPliku
        End Get
    End Property

    Public ReadOnly Property AktywneTransfery As Integer
        Get
            Return Volatile.Read(_aktywneTransfery)
        End Get
    End Property

    ''' <summary>
    ''' Zdarzenie wywoływane z wątków puli roboczej (w WinForms użyj BeginInvoke).
    ''' </summary>
    Public Event ZmianaStatusu(aktywnePolaczenia As Integer, ostatniKomunikat As String)

    Public Sub New(sciezkaPliku As String)
        If String.IsNullOrWhiteSpace(sciezkaPliku) OrElse Not File.Exists(sciezkaPliku) Then
            Throw New FileNotFoundException("Nie znaleziono pliku mapy do udostępnienia.", sciezkaPliku)
        End If

        Dim info As New FileInfo(sciezkaPliku)
        _sciezkaPliku = info.FullName
        _nazwaPliku = info.Name
        _dlugoscPliku = info.Length

        ' 128-bitowy losowy token hex (32 znaki) — odporny na zgadywanie w LAN
        Dim losowe(15) As Byte
        RandomNumberGenerator.Fill(losowe)
        _token = Convert.ToHexString(losowe).ToLowerInvariant()

        _naglowekDisposition = ZbudujContentDisposition(_nazwaPliku)
        _stronaHtmlBajty = ZbudujStroneHtml(_nazwaPliku, _dlugoscPliku, _token)
    End Sub

    ''' <summary>
    ''' Uruchamia nasłuch na wszystkich interfejsach IPv4. Port 0 = wolny port dobrany przez system.
    ''' </summary>
    Public Sub Uruchom(Optional preferowanyPort As Integer = 0)
        If _zwolniony Then Throw New ObjectDisposedException(NameOf(SerwerWysylkiWifi))
        If _listener IsNot Nothing Then Throw New InvalidOperationException("Serwer jest już uruchomiony.")

        _cts = New CancellationTokenSource()
        _listener = New TcpListener(IPAddress.Any, preferowanyPort)
        _listener.Start()
        _port = DirectCast(_listener.LocalEndpoint, IPEndPoint).Port

        _zadanieNasluchu = Task.Run(Function() PetlaAkceptacjiAsync(_cts.Token))
    End Sub

    ''' <summary>
    ''' Zwraca adres bezpośredniego pobrania pliku (np. http://192.168.1.50:54321/{token}/pobierz).
    ''' </summary>
    Public Function PobierzAdresPliku(adresIp As String) As String
        Return $"http://{adresIp}:{Port}/{_token}/pobierz"
    End Function

    ''' <summary>
    ''' Zwraca adres lekkiej strony HTML z przyciskiem pobierania (bezpieczniejsze dla części skanerów QR).
    ''' </summary>
    Public Function PobierzAdresStrony(adresIp As String) As String
        Return $"http://{adresIp}:{Port}/{_token}"
    End Function

    Private Async Function PetlaAkceptacjiAsync(ct As CancellationToken) As Task
        Try
            While Not ct.IsCancellationRequested
                Dim klient As TcpClient = Await _listener.AcceptTcpClientAsync(ct).ConfigureAwait(False)
                ' Każde połączenie obsługiwane równolegle (Safari HEAD + GET, DownloadManager multi-range)
                Dim ignoruj = Task.Run(Function() ObsluzKlientaAsync(klient, ct))
            End While
        Catch ex As OperationCanceledException
            ' Normalne zatrzymanie serwera
        Catch ex As ObjectDisposedException
            ' Zamknięcie gniazda podczas Dispose
        Catch ex As SocketException When ct.IsCancellationRequested
            ' Zamknięcie nasłuchu
        End Try
    End Function

    Private Async Function ObsluzKlientaAsync(klient As TcpClient, ctSerwera As CancellationToken) As Task
        Using klient
            klient.NoDelay = True
            Try
                Using strumien As NetworkStream = klient.GetStream()
                    Dim zadanie = Await OdczytajZadanieHttpAsync(strumien, ctSerwera).ConfigureAwait(False)
                    If zadanie Is Nothing Then Return

                    ' Obsługa wyłącznie GET i HEAD
                    Dim czyHead As Boolean = String.Equals(zadanie.Metoda, "HEAD", StringComparison.OrdinalIgnoreCase)
                    Dim czyGet As Boolean = String.Equals(zadanie.Metoda, "GET", StringComparison.OrdinalIgnoreCase)
                    If Not czyHead AndAlso Not czyGet Then
                        Await WyslijProstaOdpowiedzAsync(strumien, "405 Method Not Allowed",
                                                         "Allow: GET, HEAD" & vbCrLf,
                                                         "Dozwolone tylko GET i HEAD.", ctSerwera).ConfigureAwait(False)
                        Return
                    End If

                    ' Ścisła walidacja tokenu — zero odczytu ścieżek dyskowych z URL (brak ryzyka path traversal)
                    Dim sciezkaBezQuery As String = zadanie.Sciezka
                    Dim idxQuery As Integer = sciezkaBezQuery.IndexOf("?"c)
                    If idxQuery >= 0 Then sciezkaBezQuery = sciezkaBezQuery.Substring(0, idxQuery)
                    sciezkaBezQuery = sciezkaBezQuery.TrimEnd("/"c)

                    Dim prefiksTokenu As String = "/" & _token
                    If String.Equals(sciezkaBezQuery, prefiksTokenu, StringComparison.Ordinal) Then
                        ' Minimalna strona powitalna z przyciskiem "Pobierz mapę"
                        Await WyslijStroneHtmlAsync(strumien, czyHead, ctSerwera).ConfigureAwait(False)
                        Return
                    End If

                    If Not sciezkaBezQuery.StartsWith(prefiksTokenu & "/", StringComparison.Ordinal) Then
                        Await WyslijProstaOdpowiedzAsync(strumien, "404 Not Found", String.Empty,
                                                         "Nie znaleziono zasobu.", ctSerwera).ConfigureAwait(False)
                        Return
                    End If

                    ' Zawsze strumieniujemy wyłącznie ten jeden plik skonfigurowany w konstruktorze
                    Await WyslijPlikAsync(strumien, czyHead, zadanie.NaglowekRange, ctSerwera).ConfigureAwait(False)
                End Using
            Catch ex As OperationCanceledException
                ' Przerwanie przez zamknięcie okna lub timeout
            Catch ex As IOException
                ' Telefon zamknął połączenie (np. anulowanie lub przełączenie połączenia w Safari)
            Catch ex As SocketException
                ' Zerwanie sieci Wi-Fi
            End Try
        End Using
    End Function

    Private Async Function OdczytajZadanieHttpAsync(strumien As NetworkStream, ctSerwera As CancellationToken) As Task(Of ZadanieHttp)
        Using ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(ctSerwera)
            ctsTimeout.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_NAGLOWKOW_SEK))

            Using czytnik As New StreamReader(strumien, Encoding.ASCII, detectEncodingFromByteOrderMarks:=False, bufferSize:=4096, leaveOpen:=True)
                Dim liniaStartowa As String = Await czytnik.ReadLineAsync(ctsTimeout.Token).ConfigureAwait(False)
                If String.IsNullOrWhiteSpace(liniaStartowa) OrElse liniaStartowa.Length > MAX_DLUGOSC_LINII Then
                    Return Nothing
                End If

                Dim czesci = liniaStartowa.Split(" "c, StringSplitOptions.RemoveEmptyEntries)
                If czesci.Length < 2 Then Return Nothing

                Dim wynik As New ZadanieHttp With {
                    .Metoda = czesci(0),
                    .Sciezka = czesci(1),
                    .NaglowekRange = Nothing
                }

                For i = 1 To MAX_LINII_NAGLOWKA
                    Dim linia As String = Await czytnik.ReadLineAsync(ctsTimeout.Token).ConfigureAwait(False)
                    If linia Is Nothing Then Return Nothing
                    If linia.Length = 0 Then Exit For ' Koniec sekcji nagłówków
                    If linia.Length > MAX_DLUGOSC_LINII Then Return Nothing

                    If linia.StartsWith("Range:", StringComparison.OrdinalIgnoreCase) Then
                        wynik.NaglowekRange = linia.Substring(6).Trim()
                    End If
                Next

                Return wynik
            End Using
        End Using
    End Function

    Private Async Function WyslijPlikAsync(
        strumien As NetworkStream,
        czyHead As Boolean,
        naglowekRange As String,
        ctSerwera As CancellationToken) As Task

        Dim zakres = ParsujRange(naglowekRange, _dlugoscPliku)

        If zakres.Status = StatusRange.Nieprawidlowy Then
            Dim extra = $"Content-Range: bytes */{_dlugoscPliku}" & vbCrLf
            Await WyslijProstaOdpowiedzAsync(strumien, "416 Range Not Satisfiable", extra,
                                             String.Empty, ctSerwera).ConfigureAwait(False)
            Return
        End If

        Dim poczatek As Long = 0
        Dim koniec As Long = _dlugoscPliku - 1
        Dim czyCzesciowy As Boolean = (zakres.Status = StatusRange.Czesciowy)

        If czyCzesciowy Then
            poczatek = zakres.Poczatek
            koniec = zakres.Koniec
        End If

        Dim dlugoscOdpowiedzi As Long = If(_dlugoscPliku = 0, 0L, (koniec - poczatek) + 1L)

        Dim sb As New StringBuilder(512)
        If czyCzesciowy Then
            sb.Append("HTTP/1.1 206 Partial Content").Append(vbCrLf)
            sb.Append($"Content-Range: bytes {poczatek}-{koniec}/{_dlugoscPliku}").Append(vbCrLf)
        Else
            sb.Append("HTTP/1.1 200 OK").Append(vbCrLf)
        End If

        sb.Append("Content-Type: application/octet-stream").Append(vbCrLf)
        sb.Append("Content-Disposition: ").Append(_naglowekDisposition).Append(vbCrLf)
        sb.Append("Accept-Ranges: bytes").Append(vbCrLf)
        sb.Append($"Content-Length: {dlugoscOdpowiedzi}").Append(vbCrLf)
        sb.Append("Cache-Control: no-store").Append(vbCrLf)
        sb.Append("Connection: close").Append(vbCrLf)
        sb.Append(vbCrLf)

        Dim naglowkiBajty = Encoding.ASCII.GetBytes(sb.ToString())
        Await ZapiszZTimeoutemAsync(strumien, naglowkiBajty, 0, naglowkiBajty.Length, ctSerwera).ConfigureAwait(False)

        ' Dla HEAD lub pustego pliku kończymy po wysłaniu nagłówków
        If czyHead OrElse dlugoscOdpowiedzi <= 0 Then
            Await strumien.FlushAsync(ctSerwera).ConfigureAwait(False)
            Return
        End If

        Dim aktywne = Interlocked.Increment(_aktywneTransfery)
        RaiseEvent ZmianaStatusu(aktywne, $"Wysyłanie ({SzacunekRozmiaru.Tekst(dlugoscOdpowiedzi)})...")

        Try
            ' Pojedynczy CancellationTokenSource na cały transfer — zero alokacji na kawałek 64 KB
            Using ctsZapis = CancellationTokenSource.CreateLinkedTokenSource(ctSerwera)
                Using fs As New FileStream(_sciezkaPliku, FileMode.Open, FileAccess.Read, FileShare.Read,
                                           ROZMIAR_BUFORA, FileOptions.Asynchronous Or FileOptions.SequentialScan)
                    If poczatek > 0 Then
                        fs.Seek(poczatek, SeekOrigin.Begin)
                    End If

                    Dim bufor(ROZMIAR_BUFORA - 1) As Byte
                    Dim pozostalo As Long = dlugoscOdpowiedzi

                    While pozostalo > 0 AndAlso Not ctSerwera.IsCancellationRequested
                        Dim doOdczytu As Integer = CInt(Math.Min(bufor.Length, pozostalo))
                        Dim odczytano As Integer = Await fs.ReadAsync(bufor.AsMemory(0, doOdczytu), ctSerwera).ConfigureAwait(False)
                        If odczytano <= 0 Then Exit While

                        ctsZapis.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_ZAPISU_SEK))
                        Await strumien.WriteAsync(bufor.AsMemory(0, odczytano), ctsZapis.Token).ConfigureAwait(False)
                        pozostalo -= odczytano
                    End While
                End Using
            End Using

            Await strumien.FlushAsync(ctSerwera).ConfigureAwait(False)
        Finally
            Dim po = Interlocked.Decrement(_aktywneTransfery)
            RaiseEvent ZmianaStatusu(po, If(po > 0, "Trwa transfer...", "Gotowy do pobrania"))
        End Try
    End Function

    Private Async Function WyslijStroneHtmlAsync(strumien As NetworkStream, czyHead As Boolean, ct As CancellationToken) As Task
        Dim sb As New StringBuilder(256)
        sb.Append("HTTP/1.1 200 OK").Append(vbCrLf)
        sb.Append("Content-Type: text/html; charset=utf-8").Append(vbCrLf)
        sb.Append($"Content-Length: {_stronaHtmlBajty.Length}").Append(vbCrLf)
        sb.Append("Cache-Control: no-store").Append(vbCrLf)
        sb.Append("Connection: close").Append(vbCrLf)
        sb.Append(vbCrLf)

        Dim nagl = Encoding.ASCII.GetBytes(sb.ToString())
        Await ZapiszZTimeoutemAsync(strumien, nagl, 0, nagl.Length, ct).ConfigureAwait(False)
        If Not czyHead Then
            Await ZapiszZTimeoutemAsync(strumien, _stronaHtmlBajty, 0, _stronaHtmlBajty.Length, ct).ConfigureAwait(False)
        End If
        Await strumien.FlushAsync(ct).ConfigureAwait(False)
    End Function

    Private Async Function WyslijProstaOdpowiedzAsync(
        strumien As NetworkStream,
        kodStatusu As String,
        dodatkoweNaglowki As String,
        tresc As String,
        ct As CancellationToken) As Task

        Dim cialo = Encoding.UTF8.GetBytes(tresc)
        Dim sb As New StringBuilder(256)
        sb.Append("HTTP/1.1 ").Append(kodStatusu).Append(vbCrLf)
        If Not String.IsNullOrEmpty(dodatkoweNaglowki) Then sb.Append(dodatkoweNaglowki)
        sb.Append("Content-Type: text/plain; charset=utf-8").Append(vbCrLf)
        sb.Append($"Content-Length: {cialo.Length}").Append(vbCrLf)
        sb.Append("Connection: close").Append(vbCrLf)
        sb.Append(vbCrLf)

        Dim nagl = Encoding.ASCII.GetBytes(sb.ToString())
        Await ZapiszZTimeoutemAsync(strumien, nagl, 0, nagl.Length, ct).ConfigureAwait(False)
        If cialo.Length > 0 Then
            Await ZapiszZTimeoutemAsync(strumien, cialo, 0, cialo.Length, ct).ConfigureAwait(False)
        End If
    End Function

    Private Shared Async Function ZapiszZTimeoutemAsync(
        strumien As NetworkStream,
        bufor As Byte(),
        offset As Integer,
        ilosc As Integer,
        ctSerwera As CancellationToken) As Task

        Using cts = CancellationTokenSource.CreateLinkedTokenSource(ctSerwera)
            cts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_ZAPISU_SEK))
            Await strumien.WriteAsync(bufor.AsMemory(offset, ilosc), cts.Token).ConfigureAwait(False)
        End Using
    End Function

    ' =====================================================================
    ' PARSOWANIE NAGŁÓWKA RANGE (RFC 9110)
    ' =====================================================================
    Private Enum StatusRange
        Brak
        Czesciowy
        Nieprawidlowy
    End Enum

    Private Structure WynikRange
        Public Status As StatusRange
        Public Poczatek As Long
        Public Koniec As Long
    End Structure

    Private Shared Function ParsujRange(naglowek As String, dlugoscPliku As Long) As WynikRange
        If String.IsNullOrWhiteSpace(naglowek) Then
            Return New WynikRange With {.Status = StatusRange.Brak}
        End If

        If Not naglowek.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase) Then
            Return New WynikRange With {.Status = StatusRange.Brak}
        End If

        Dim spec = naglowek.Substring(6).Trim()

        ' Wielokrotne zakresy (multipart/byteranges) -> zgodnie z RFC 9110 zwracamy pełny 200 OK
        If spec.Contains(","c) Then
            Return New WynikRange With {.Status = StatusRange.Brak}
        End If

        If dlugoscPliku <= 0 Then
            Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
        End If

        Dim czesci = spec.Split("-"c)
        If czesci.Length <> 2 Then
            Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
        End If

        Dim odStr = czesci(0).Trim()
        Dim doStr = czesci(1).Trim()

        ' Wariant 1: suffix range "bytes=-500" (ostatnie 500 bajtów)
        If odStr.Length = 0 AndAlso doStr.Length > 0 Then
            Dim ilosc As Long
            If Not Long.TryParse(doStr, NumberStyles.None, CultureInfo.InvariantCulture, ilosc) OrElse ilosc <= 0 Then
                Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
            End If
            Return New WynikRange With {
                .Status = StatusRange.Czesciowy,
                .Poczatek = Math.Max(0L, dlugoscPliku - ilosc),
                .Koniec = dlugoscPliku - 1L
            }
        End If

        ' Wariant 2: otwarty "bytes=1000-" lub zamknięty "bytes=1000-4999"
        Dim start As Long
        If Not Long.TryParse(odStr, NumberStyles.None, CultureInfo.InvariantCulture, start) OrElse start < 0 Then
            Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
        End If

        If start >= dlugoscPliku Then
            Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
        End If

        Dim koniec As Long = dlugoscPliku - 1L
        If doStr.Length > 0 Then
            If Not Long.TryParse(doStr, NumberStyles.None, CultureInfo.InvariantCulture, koniec) OrElse koniec < start Then
                Return New WynikRange With {.Status = StatusRange.Nieprawidlowy}
            End If
            ' Zgodnie z RFC 9110 koniec wykraczający poza rozmiar pliku przycinamy do EOF
            If koniec >= dlugoscPliku Then koniec = dlugoscPliku - 1L
        End If

        Return New WynikRange With {
            .Status = StatusRange.Czesciowy,
            .Poczatek = start,
            .Koniec = koniec
        }
    End Function

    ' =====================================================================
    ' CONTENT-DISPOSITION (RFC 5987 / RFC 6266) DLA POLSKICH ZNAKÓW
    ' =====================================================================
    Private Shared Function ZbudujContentDisposition(nazwaPliku As String) As String
        Dim nazwaAscii = NaBezpieczneAscii(nazwaPliku)
        Dim nazwaUtf8 = KodujRfc5987(nazwaPliku)
        Return $"attachment; filename=""{nazwaAscii}""; filename*=UTF-8''{nazwaUtf8}"
    End Function

    Private Shared Function NaBezpieczneAscii(tekst As String) As String
        ' Litera ł/Ł nie rozkłada się w Unicode NormalizationForm.FormD — zamieniamy jawnie
        Dim wstepny = tekst.Replace("ł"c, "l"c).Replace("Ł"c, "L"c).Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(wstepny.Length)

        For Each ch In wstepny
            Dim kat = CharUnicodeInfo.GetUnicodeCategory(ch)
            If kat = UnicodeCategory.NonSpacingMark Then Continue For

            Dim kod = AscW(ch)
            If ch = " "c Then
                sb.Append("_"c)
            ElseIf kod >= 33 AndAlso kod <= 126 AndAlso ch <> """"c AndAlso ch <> "\"c AndAlso ch <> "/"c AndAlso ch <> ";"c Then
                sb.Append(ch)
            End If
        Next

        Dim wynik = sb.ToString().Trim()
        Return If(String.IsNullOrEmpty(wynik), "mapa.bin", wynik)
    End Function

    Private Shared Function KodujRfc5987(tekst As String) As String
        ' Uri.EscapeDataString koduje UTF-8 wg RFC 3986; dla ścisłego RFC 5987 kodujemy też znaki ' ( ) *
        Return Uri.EscapeDataString(tekst) _
            .Replace("'", "%27") _
            .Replace("(", "%28") _
            .Replace(")", "%29") _
            .Replace("*", "%2A")
    End Function

    Private Shared Function ZbudujStroneHtml(nazwaPliku As String, rozmiar As Long, token As String) As Byte()
        Dim bezpNazwa = WebUtility.HtmlEncode(nazwaPliku)
        Dim rozStr = WebUtility.HtmlEncode(SzacunekRozmiaru.Tekst(rozmiar))
        Dim html =
            "<!doctype html><html lang=""pl""><head><meta charset=""utf-8"">" &
            "<meta name=""viewport"" content=""width=device-width,initial-scale=1"">" &
            "<title>MapoTero – pobieranie mapy</title>" &
            "<style>body{font-family:system-ui,sans-serif;max-width:460px;margin:2rem auto;padding:1rem;text-align:center}" &
            ".btn{display:inline-block;padding:1rem 1.5rem;background:#1b6ec2;color:#fff;text-decoration:none;border-radius:8px;font-weight:600;font-size:1.1rem}" &
            ".meta{color:#555;margin:.8rem 0 1.5rem;word-break:break-word}</style></head><body>" &
            $"<h2>MapoTero</h2><div class=""meta""><strong>{bezpNazwa}</strong><br>{rozStr}</div>" &
            $"<a class=""btn"" href=""/{token}/pobierz"">Pobierz mapę na telefon</a></body></html>"
        Return Encoding.UTF8.GetBytes(html)
    End Function


    ' =====================================================================
    ' WYKRYWANIE ADRESU IP LAN (TRIK UDP + FALLBACK NETWORKINTERFACE)
    ' =====================================================================
    Public Shared Function WykryjDomyslneIpLan() As String
        'najpierw karta fizyczna z listy: trasa do internetu (trik UDP) przy włączonym VPN prowadzi przez kartę VPN
        'i do kodu QR trafiał adres VPN, którego telefon w sieci Wi-Fi nie widzi
        Dim kandydaci = PobierzWszystkieAdresyLan()
        If kandydaci.Count > 0 Then Return kandydaci(0)

        'brak kart na liście - trasa domyślna
        Try
            Using s As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                s.Connect(New IPEndPoint(IPAddress.Parse("8.8.8.8"), 80)) 'nie wysyła pakietów, tylko wybiera trasę
                Dim ep = TryCast(s.LocalEndPoint, IPEndPoint)
                If ep IsNot Nothing AndAlso Not IPAddress.IsLoopback(ep.Address) Then Return ep.Address.ToString()
            End Using
        Catch
        End Try
        Return "127.0.0.1"
    End Function

    ''' <summary>
    ''' Zwraca posortowaną listę adresów IPv4 LAN do ewentualnego ComboBoxa w oknie dialogowym.
    ''' Fizyczne Wi-Fi/Ethernet z bramą trafiają na początek listy.
    ''' </summary>
    Public Shared Function PobierzWszystkieAdresyLan() As List(Of String)
        Dim wyniki As New List(Of (Priorytet As Integer, Adres As String))()

        For Each nic In NetworkInterface.GetAllNetworkInterfaces()
            If nic.OperationalStatus <> OperationalStatus.Up Then Continue For
            If nic.NetworkInterfaceType = NetworkInterfaceType.Loopback OrElse
               nic.NetworkInterfaceType = NetworkInterfaceType.Tunnel Then Continue For

            Dim props = nic.GetIPProperties()
            Dim maBrame = props.GatewayAddresses.Any(Function(g) g.Address.AddressFamily = AddressFamily.InterNetwork AndAlso
                                                                 Not g.Address.Equals(IPAddress.Any))

            Dim priorytet As Integer = PriorytetKarty($"{nic.Name} {nic.Description}", nic.NetworkInterfaceType, maBrame)

            For Each ua In props.UnicastAddresses
                If ua.Address.AddressFamily <> AddressFamily.InterNetwork Then Continue For
                If IPAddress.IsLoopback(ua.Address) Then Continue For

                Dim tekstIp = ua.Address.ToString()
                ' Pomiń adresy APIPA (169.254.x.x)
                If tekstIp.StartsWith("169.254.", StringComparison.Ordinal) Then Continue For

                wyniki.Add((priorytet, tekstIp))
            Next
        Next

        Return wyniki _
            .OrderBy(Function(x) x.Priorytet) _
            .Select(Function(x) x.Adres) _
            .Distinct(StringComparer.Ordinal) _
            .ToList()
    End Function

    ''' <summary>
    ''' Kolejność kart na liście adresów (mniej = wyżej): karta fizyczna przed wirtualną i VPN, z bramą przed bez bramy,
    ''' Wi-Fi przed przewodową. Telefon łączy się przez sieć Wi-Fi, więc adres karty VPN w kodzie QR jest bezużyteczny.
    ''' </summary>
    Friend Shared Function PriorytetKarty(nazwaIOpis As String, typ As NetworkInterfaceType, maBrame As Boolean) As Integer
        Dim opis = If(nazwaIOpis, "").ToLowerInvariant()
        Dim slowaWirtualne = {"virtual", "vmware", "vbox", "hyper-v", "vethernet", "wsl", "loopback", "docker", "tailscale",
                              "wireguard", "zerotier", "openvpn", "nordlynx", "proton", "vpn", "anyconnect", "fortinet",
                              "tap-windows", "wintun"}
        Dim czyWirtualny = typ = NetworkInterfaceType.Ppp OrElse slowaWirtualne.Any(Function(w) opis.Contains(w))
        Dim priorytet As Integer = 50
        If Not czyWirtualny Then priorytet -= 30
        If maBrame Then priorytet -= 10
        If typ = NetworkInterfaceType.Wireless80211 Then priorytet -= 5
        Return priorytet
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _zwolniony Then Return
        _zwolniony = True
        Try
            _cts?.Cancel()
        Catch
        End Try
        Try
            _listener?.Stop()
        Catch
        End Try
        _cts?.Dispose()
    End Sub

    Private NotInheritable Class ZadanieHttp
        Public Metoda As String
        Public Sciezka As String
        Public NaglowekRange As String
    End Class

End Class
