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

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml.Linq

''' <summary>Typ wykrytej usługi mapowej.</summary>
Public Enum TypUslugiMapowej
    Nieznana = 0
    Wms = 1
    Wmts = 2
    Xyz = 3
    Tms = 4
End Enum

''' <summary>Rezultat pobrania i analizy opisu usługi GetCapabilities.</summary>
Public Class WynikCapabilities

    Public Property Typ As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    Public Property UslugaWms As UslugaWmsInfo
    Public Property UslugaWmts As UslugaWmts
    Public Property AdresCapabilities As String = ""
    Public Property AdresBazowy As String = ""
    Public Property Tytul As String = ""
    Public Property SzablonKafli As String = ""
    Public Property OpcjeKafli As OpcjeKafliXyz
    Public Property PrzykladowyKafel As Byte()
    Public ReadOnly Property Ostrzezenia As New List(Of String)
    Public Property KomunikatBledu As String = ""

    Public ReadOnly Property Sukces As Boolean
        Get
            Return Typ <> TypUslugiMapowej.Nieznana AndAlso String.IsNullOrEmpty(KomunikatBledu)
        End Get
    End Property

End Class

''' <summary>
''' Asynchroniczny klient pobierający i analizujący dokument GetCapabilities z serwera WMS / WMTS.
''' Automatycznie wykrywa rodzaj usługi (WMS vs WMTS), format wersji (1.3.0 vs 1.1.1) oraz obsługuje
''' różnorodne strony kodowe XML (UTF-8, ISO-8859-2, Windows-1250).
''' </summary>
Public NotInheritable Class KlientCapabilities

    Private Sub New()
    End Sub

    ''' <summary>Pobiera i analizuje opis usługi z podanego adresu URL (WMS, WMTS, szablon XYZ/TMS, TileJSON lub OSGeo TMS XML).</summary>
    Public Shared Async Function PobierzAsync(adres As String, Optional ignorujSsl As Boolean = False,
                                             Optional timeoutSekundy As Integer = 15,
                                             Optional token As CancellationToken = Nothing,
                                             Optional handler As HttpMessageHandler = Nothing,
                                             Optional srodekTestowy As PunktGeo? = Nothing) As Task(Of WynikCapabilities)
        Dim wynik As New WynikCapabilities()
        If String.IsNullOrWhiteSpace(adres) Then
            wynik.KomunikatBledu = "Podaj adres serwera WMS, WMTS lub szablon kafelków XYZ/TMS."
            Return wynik
        End If

        Dim adresWyczyszczony As String = adres.Trim()
        If KafleXyz.CzyAdresXyz(adresWyczyszczony) Then
            Return Await PobierzXyzTmsAsync(adresWyczyszczony, ignorujSsl, timeoutSekundy, token, handler, srodekTestowy).ConfigureAwait(False)
        End If

        Dim czyPodejrzenieWmts As Boolean = Wmts.CzyAdresWmts(adresWyczyszczony)
        Dim sciezka As String = adresWyczyszczony.Split("?"c)(0)
        Dim czyBezposredniOpisKafli As Boolean = sciezka.EndsWith(".json", StringComparison.OrdinalIgnoreCase) OrElse
                                                sciezka.EndsWith("tilemapresource.xml", StringComparison.OrdinalIgnoreCase)

        'kolejność prób: dla TileJSON/TMS XML najpierw surowy adres, dla WMTS najpierw WMTS, w przeciwnym razie WMS
        Dim adresyPrób As New List(Of Tuple(Of String, TypUslugiMapowej))
        If czyBezposredniOpisKafli Then
            adresyPrób.Add(Tuple.Create(adresWyczyszczony, TypUslugiMapowej.Nieznana))
        End If
        If czyPodejrzenieWmts Then
            adresyPrób.Add(Tuple.Create(AdresCapabilitiesWmts(adresWyczyszczony), TypUslugiMapowej.Wmts))
            adresyPrób.Add(Tuple.Create(AdresCapabilitiesWms(adresWyczyszczony), TypUslugiMapowej.Wms))
        Else
            adresyPrób.Add(Tuple.Create(AdresCapabilitiesWms(adresWyczyszczony), TypUslugiMapowej.Wms))
            adresyPrób.Add(Tuple.Create(AdresCapabilitiesWmts(adresWyczyszczony), TypUslugiMapowej.Wmts))
        End If

        'błędy kolejnych prób; wyświetlany jest błąd usługi próbowanej jako pierwsza (dawniej - ostatniej, przez co
        'chwilowy błąd serwera WMS zastępował komunikat próby WMTS, np. "Can't recognize service requested")
        Dim bledy As New List(Of String)
        Dim ostatniBlad As String = ""
        For Each proba In adresyPrób
            If ostatniBlad <> "" Then bledy.Add(ostatniBlad)
            ostatniBlad = ""
            Dim adresDoPobrania As String = proba.Item1
            Try
                Dim bajty = Await PobierzBajtyAsync(adresDoPobrania, ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
                If bajty Is Nothing OrElse bajty.Length = 0 Then
                    ostatniBlad = "Serwer zwrócił pustą odpowiedź."
                    Continue For
                End If

                'sprawdzenie, czy serwer nie zwrócił strony HTML (błąd 502, Cloudflare, portal WWW)
                Dim naglowekTekst As String = System.Text.Encoding.UTF8.GetString(bajty, 0, Math.Min(bajty.Length, 200)).TrimStart()
                If naglowekTekst.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) OrElse
                   naglowekTekst.StartsWith("<html", StringComparison.OrdinalIgnoreCase) Then
                    ostatniBlad = "Serwer zwrócił stronę WWW zamiast dokumentu usługi. Upewnij się, że adres wskazuje usługę WMS/WMTS/TileJSON, a nie stronę informacyjną geoportalu."
                    Continue For
                End If

                'TileJSON
                If naglowekTekst.StartsWith("{", StringComparison.Ordinal) Then
                    Dim jsonTekst As String = System.Text.Encoding.UTF8.GetString(bajty)
                    Dim tj = KafleXyz.ParsujTileJson(jsonTekst)
                    Dim nazwaWarstwy As String = If(String.IsNullOrWhiteSpace(tj.Nazwa), MetadaneMapy.Serwer(tj.Szablon), tj.Nazwa)
                    Return Await ZbudujWynikZeSzablonuAsync(tj.Szablon, nazwaWarstwy, tj.Opcje, adresDoPobrania,
                                                             ignorujSsl, timeoutSekundy, token, handler, srodekTestowy).ConfigureAwait(False)
                End If

                'odczyt XML ze strumienia (XDocument sam odczytuje deklarację encoding=...)
                Dim doc As XDocument
                Using ms As New MemoryStream(bajty)
                    doc = XDocument.Load(ms)
                End Using

                Dim korzen = doc.Root
                If korzen Is Nothing Then
                    ostatniBlad = "Pusty dokument XML."
                    Continue For
                End If

                Dim nazwaKorzenia As String = korzen.Name.LocalName

                'obsługa błędu WFS
                If nazwaKorzenia.IndexOf("WFS_Capabilities", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    wynik.KomunikatBledu = "Podany adres wskazuje usługę wektorową WFS. MapoTero obsługuje rastrowe usługi WMS, WMTS oraz kafle XYZ/TMS."
                    Return wynik
                End If

                'obsługa ServiceExceptionReport
                If nazwaKorzenia.IndexOf("ServiceExceptionReport", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   nazwaKorzenia.IndexOf("ExceptionReport", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Dim komunikat As String = korzen.Value.Trim()
                    If komunikat.Length > 250 Then komunikat = komunikat.Substring(0, 250)
                    ostatniBlad = "Serwer zgłosił błąd: " & komunikat
                    Continue For
                End If

                'OSGeo TMS (tilemapresource.xml)
                If nazwaKorzenia.Equals("TileMap", StringComparison.OrdinalIgnoreCase) Then
                    Dim tms = KafleXyz.ParsujTmsXml(doc.ToString(SaveOptions.DisableFormatting), adresDoPobrania)
                    Dim nazwaWarstwy As String = If(String.IsNullOrWhiteSpace(tms.Tytul), MetadaneMapy.Serwer(tms.Szablon), tms.Tytul)
                    Return Await ZbudujWynikZeSzablonuAsync(tms.Szablon, nazwaWarstwy, tms.Opcje, adresDoPobrania,
                                                             ignorujSsl, timeoutSekundy, token, handler, srodekTestowy).ConfigureAwait(False)
                End If

                'sprawdzenie czy WMTS
                If nazwaKorzenia.Equals("Capabilities", StringComparison.OrdinalIgnoreCase) AndAlso
                   (doc.Descendants().Any(Function(x) x.Name.LocalName.Equals("TileMatrixSet", StringComparison.OrdinalIgnoreCase)) OrElse
                    proba.Item2 = TypUslugiMapowej.Wmts) Then
                    'tekst z dokumentu odczytanego już wg deklaracji encoding (StreamReader ją pomijał - np. ISO-8859-2)
                    Dim usluga = UslugaWmts.Wczytaj(doc.ToString(SaveOptions.DisableFormatting), adresDoPobrania)
                    wynik.Typ = TypUslugiMapowej.Wmts
                    wynik.UslugaWmts = usluga
                    wynik.AdresCapabilities = adresDoPobrania
                    wynik.AdresBazowy = adresWyczyszczony
                    wynik.Tytul = If(usluga.Warstwy.Count > 0 AndAlso Not String.IsNullOrEmpty(usluga.Warstwy(0).Tytul),
                                     usluga.Warstwy(0).Tytul, "Usługa WMTS")
                    SprawdzUkładyWmts(usluga, wynik)
                    Return wynik
                End If

                'sprawdzenie czy WMS
                If nazwaKorzenia.IndexOf("Capabilities", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Using msWms As New MemoryStream(bajty)
                        Dim usluga = WmsCapabilities.Wczytaj(msWms, adresDoPobrania)
                        wynik.Typ = TypUslugiMapowej.Wms
                        wynik.UslugaWms = usluga
                        wynik.AdresCapabilities = adresDoPobrania
                        wynik.AdresBazowy = usluga.AdresGetMap
                        wynik.Tytul = usluga.Tytul
                        SprawdzUkladyWms(usluga, wynik)
                        Return wynik
                    End Using
                End If

                ostatniBlad = "Serwer zwrócił dokument XML inny niż WMS/WMTS (element główny: " & nazwaKorzenia & ")."

            Catch ex As InvalidOperationException When CzyBladCertyfikatuSsl(ex)
                wynik.KomunikatBledu = ex.Message
                Return wynik
            Catch ex As Exception
                ostatniBlad = ex.Message
            End Try
        Next
        If ostatniBlad <> "" Then bledy.Add(ostatniBlad)

        wynik.KomunikatBledu = If(bledy.Count > 0, bledy(0), "Nie udało się połączyć z serwerem WMS/WMTS pod podanym adresem.")
        Return wynik
    End Function

    ''' <summary>Tworzy adres GetCapabilities dla WMS z zachowaniem istniejących parametrów niestandardowych (np. token, key).</summary>
    Public Shared Function AdresCapabilitiesWms(adres As String) As String
        Dim a = adres.Trim()
        If Regex.IsMatch(a, "[?&]request=getcapabilities", RegexOptions.IgnoreCase) Then Return a
        Dim separator As String = If(a.Contains("?"), "&", "?")
        If a.EndsWith("?", StringComparison.Ordinal) OrElse a.EndsWith("&", StringComparison.Ordinal) Then separator = ""
        Return a & separator & "SERVICE=WMS&REQUEST=GetCapabilities"
    End Function

    ''' <summary>
    ''' Tworzy adres GetCapabilities WMS z adresu zapisanego w pliku zbioru map (warstwy/*.txt).
    ''' Taki adres zawiera parametry GetMap (VERSION, CRS, LAYERS, FORMAT…), które trzeba usunąć;
    ''' parametry niestandardowe (np. map=, token=) zostają.
    ''' </summary>
    Public Shared Function AdresCapabilitiesWmsZAdresuPliku(adres As String) As String
        Dim a = adres.Trim().Trim(""""c)
        a = Regex.Replace(a, "(?<=[?&])(service|request|version|crs|srs|layers|format|styles|width|height|bbox|transparent)=[^&]*&?", "", RegexOptions.IgnoreCase)
        a = a.TrimEnd("&"c)
        Return AdresCapabilitiesWms(a)
    End Function

    ''' <summary>Tworzy adres GetCapabilities dla WMTS.</summary>
    Public Shared Function AdresCapabilitiesWmts(adres As String) As String
        Return Wmts.AdresCapabilities(adres)
    End Function

    ''' <summary>Sprawdza, czy warstwy WMTS posiadają macierze w układach obsługiwanych przez MapoTero.</summary>
    Private Shared Sub SprawdzUkładyWmts(usluga As UslugaWmts, wynik As WynikCapabilities)
        Dim brakujace As Integer = 0
        For Each w In usluga.Warstwy
            Dim maObslugiwany As Boolean = False
            For Each p In w.Powiazania
                Dim z = usluga.Zestaw(p.Zestaw)
                If z IsNot Nothing AndAlso z.Uklad IsNot Nothing Then
                    maObslugiwany = True
                    Exit For
                End If
            Next
            If Not maObslugiwany Then brakujace += 1
        Next
        If brakujace > 0 AndAlso brakujace = usluga.Warstwy.Count Then
            wynik.Ostrzezenia.Add("Uwaga: Serwer nie deklaruje żadnego zestawu kafli w układach obsługiwanych przez MapoTero (PL-1992, PL-2000, WGS84, Web Mercator).")
        End If
    End Sub

    ''' <summary>Sprawdza, czy warstwy WMS obsługują układy współrzędnych MapoTero.</summary>
    Private Shared Sub SprawdzUkladyWms(usluga As UslugaWmsInfo, wynik As WynikCapabilities)
        Dim warstwy = usluga.WszystkieWarstwyDoPobrania()
        If warstwy.Count = 0 Then
            wynik.Ostrzezenia.Add("Serwer nie posiada żadnych warstw gotowych do bezpośredniego pobrania (brak nazw technicznych).")
            Return
        End If
        Dim zUkladem As Integer = 0
        For Each w In warstwy
            If w.ObslugujeEpsg(2180) OrElse w.ObslugujeEpsg(2176) OrElse w.ObslugujeEpsg(2177) OrElse
               w.ObslugujeEpsg(2178) OrElse w.ObslugujeEpsg(2179) OrElse w.ObslugujeEpsg(4326) OrElse
               w.ObslugujeEpsg(3857) Then
                zUkladem += 1
            End If
        Next
        If zUkladem = 0 Then
            wynik.Ostrzezenia.Add("Uwaga: Żadna warstwa nie deklaruje obsługi układów PL-1992 ani PL-2000 (może być wymagana transformacja).")
        End If
    End Sub

    ''' <summary>Liczba prób pobrania opisu usługi przy chwilowych błędach serwera.</summary>
    Private Const LiczbaProbPobrania As Integer = 3

    ''' <summary>
    ''' Pobiera surowe bajty odpowiedzi HTTP z opcją ignorowania błędów SSL. Chwilowe błędy (brak odpowiedzi, przekroczenie
    ''' czasu, HTTP 404/408/429/5xx) są ponawiane - serwery geoportalu potrafią losowo odpowiadać 404 na poprawne zapytanie.
    ''' </summary>
    Private Shared Function CzyBladCertyfikatuSsl(ex As Exception) As Boolean
        Dim biezacy = ex
        While biezacy IsNot Nothing
            If TypeOf biezacy Is System.Security.Authentication.AuthenticationException Then Return True
            biezacy = biezacy.InnerException
        End While
        Return False
    End Function

    Private Shared Async Function PobierzBajtyAsync(url As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                    token As CancellationToken, handler As HttpMessageHandler) As Task(Of Byte())
        For proba = 1 To LiczbaProbPobrania
            Try
                Return Await PobierzBajtyRazAsync(url, ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
            Catch ex As BladHttp When ex.Chwilowy AndAlso proba < LiczbaProbPobrania
            Catch ex As TimeoutException When proba < LiczbaProbPobrania
            Catch ex As HttpRequestException When CzyBladCertyfikatuSsl(ex)
                If Not ignorujSsl Then
                    Throw New InvalidOperationException("Połączenie HTTPS nie powiodło się z powodu błędu certyfikatu SSL serwera. Zaznacz opcję ""Ignoruj błędy certyfikatów SSL"" i spróbuj ponownie.", ex)
                End If
                Throw New InvalidOperationException("Nie udało się nawiązać bezpiecznego połączenia SSL/TLS z serwerem: " & ex.Message, ex)
            Catch ex As HttpRequestException When proba < LiczbaProbPobrania AndAlso Not TypeOf ex Is BladHttp
            End Try
            Await Task.Delay(1000 * proba, token).ConfigureAwait(False)
        Next
        Return Nothing
    End Function

    ''' <summary>Błąd HTTP z kodem odpowiedzi (netstandard2.0 nie udostępnia kodu w HttpRequestException).</summary>
    Private NotInheritable Class BladHttp
        Inherits HttpRequestException

        Public ReadOnly Property Kod As Integer

        Public Sub New(kod As Integer, opis As String)
            MyBase.New("Serwer zwrócił błąd HTTP " & kod & If(String.IsNullOrEmpty(opis), "", " (" & opis.TrimEnd("."c) & ")") & ".")
            Me.Kod = kod
        End Sub

        ''' <summary>Błąd, który może zniknąć przy ponownej próbie.</summary>
        Public ReadOnly Property Chwilowy As Boolean
            Get
                Return Kod = 404 OrElse Kod = 408 OrElse Kod = 429 OrElse Kod >= 500
            End Get
        End Property
    End Class

    Private NotInheritable Class OdpowiedHttpProby
        Public Property Kod As Integer
        Public Property ContentType As String = ""
        Public Property Bajty As Byte()
        Public Property Blad As String = ""
    End Class

    Private Shared Async Function SprobujPobierzAsync(url As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                      token As CancellationToken, handler As HttpMessageHandler) As Task(Of OdpowiedHttpProby)
        Dim wlasnyHandler As Boolean = handler Is Nothing
        Dim obsluga As HttpMessageHandler = If(wlasnyHandler, UtworzDomyslnyHandler(ignorujSsl), handler)
        Using ctsTimeout As New CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, timeoutSekundy))),
              ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(token, ctsTimeout.Token),
              klient As New HttpClient(obsluga, disposeHandler:=wlasnyHandler)
            klient.DefaultRequestHeaders.UserAgent.ParseAdd(WersjaProgramu.UserAgent(GetType(KlientCapabilities).Assembly))
            Try
                Using odp = Await klient.GetAsync(url, HttpCompletionOption.ResponseContentRead, ctsLinked.Token).ConfigureAwait(False)
                    Dim dane As Byte() = Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                    Dim ct As String = If(odp.Content.Headers.ContentType IsNot Nothing, If(odp.Content.Headers.ContentType.MediaType, ""), "")
                    Return New OdpowiedHttpProby With {.Kod = CInt(odp.StatusCode), .ContentType = ct, .Bajty = dane}
                End Using
            Catch ex As OperationCanceledException When Not token.IsCancellationRequested
                Return New OdpowiedHttpProby With {.Blad = "Serwer nie odpowiedział w ciągu " & Math.Max(5, timeoutSekundy) & " s."}
            Catch ex As Exception When Not token.IsCancellationRequested
                Return New OdpowiedHttpProby With {.Blad = ex.Message}
            End Try
        End Using
    End Function

    Private Shared Function UtworzDomyslnyHandler(ignorujSsl As Boolean) As HttpClientHandler
        Dim obsluga As New HttpClientHandler() With {
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }
        If ignorujSsl Then
            obsluga.ServerCertificateCustomValidationCallback = Function(msg, cert, chain, errors) True
        End If
        Return obsluga
    End Function

    Private Shared Async Function PobierzBajtyRazAsync(url As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                       token As CancellationToken, handler As HttpMessageHandler) As Task(Of Byte())
        Dim wlasnyHandler As Boolean = handler Is Nothing
        Dim obsluga As HttpMessageHandler = If(wlasnyHandler, UtworzDomyslnyHandler(ignorujSsl), handler)

        Using ctsTimeout As New CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, timeoutSekundy))),
              ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(token, ctsTimeout.Token),
              klient As New HttpClient(obsluga, disposeHandler:=wlasnyHandler)
            klient.DefaultRequestHeaders.UserAgent.ParseAdd(WersjaProgramu.UserAgent(GetType(KlientCapabilities).Assembly))
            Dim odp As HttpResponseMessage
            Try
                odp = Await klient.GetAsync(url, HttpCompletionOption.ResponseContentRead, ctsLinked.Token).ConfigureAwait(False)
            Catch ex As OperationCanceledException When Not token.IsCancellationRequested
                Throw New TimeoutException("Serwer nie odpowiedział w ciągu " & Math.Max(5, timeoutSekundy) & " s.", ex)
            End Try
            Using odp
                If Not odp.IsSuccessStatusCode Then Throw New BladHttp(CInt(odp.StatusCode), odp.ReasonPhrase)
                Return Await odp.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
            End Using
        End Using
    End Function

    ''' <summary>Sondaż bezpośredniego szablonu URL {z}/{x}/{y} lub {-y}: wykrywa XYZ vs TMS, format obrazu, rozmiar kafla i zakres z.</summary>
    Private Shared Async Function PobierzXyzTmsAsync(adresSzablonu As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                     token As CancellationToken, handler As HttpMessageHandler,
                                                     srodekTestowy As PunktGeo?) As Task(Of WynikCapabilities)
        Dim wynik As New WynikCapabilities()
        Dim subdomeny As New List(Of String)
        Dim jestTmsZAdresu As Boolean = False
        Dim szablonNorm As String = KafleXyz.NormalizujSzablon(adresSzablonu, subdomeny, jestTmsZAdresu)

        If szablonNorm.IndexOf("{layer}", StringComparison.OrdinalIgnoreCase) >= 0 Then
            wynik.KomunikatBledu = "Szablon zawiera parametr {layer} - podaj w adresie nazwę konkretnej warstwy."
            Return wynik
        End If
        Dim sciezka As String = szablonNorm.Split("?"c)(0).ToLowerInvariant()
        If sciezka.EndsWith(".mvt", StringComparison.Ordinal) OrElse sciezka.EndsWith(".pbf", StringComparison.Ordinal) Then
            wynik.KomunikatBledu = "Podany adres wskazuje wektorowe kafle MVT/PBF, których MapoTero nie obsługuje (wymagane są kafle rastrowe PNG lub JPEG)."
            Return wynik
        End If

        Dim srodek As PunktGeo = If(srodekTestowy.HasValue, srodekTestowy.Value, New PunktGeo(52.23, 21.01))
        Dim zBase As Integer = -1
        Dim wykryteTms As Boolean = jestTmsZAdresu
        Dim probka As Byte() = Nothing
        Dim probkaCt As String = ""
        Dim rozmiarKafla As Integer = 256
        Dim ostatniBlad As String = ""

        For Each zCand In {10, 6, 2}
            Dim probujOdwrocone = If(jestTmsZAdresu, New Boolean() {True}, New Boolean() {False, True})
            For Each tmsCand In probujOdwrocone
                Dim url As String = AdresKaflaDoProby(szablonNorm, subdomeny, zCand, srodek, tmsCand)
                Dim odp = Await SprobujPobierzAsync(url, ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
                If odp.Blad <> "" Then
                    ostatniBlad = MetadaneMapy.MaskujSekrety(odp.Blad)
                    Exit For
                End If
                If odp.Kod = 200 Then
                    Dim szer As Integer, wys As Integer
                    Dim diag As String = KafleXyz.DiagnozujOdpowiedzKafla(odp.Bajty, odp.ContentType, szer, wys, url)
                    If diag <> "" Then
                        wynik.KomunikatBledu = diag
                        Return wynik
                    End If
                    zBase = zCand
                    wykryteTms = tmsCand
                    probka = odp.Bajty
                    probkaCt = odp.ContentType
                    If szer >= 64 Then rozmiarKafla = szer
                    Exit For
                ElseIf odp.Kod = 401 OrElse odp.Kod = 403 Then
                    ostatniBlad = "Serwer odmówił dostępu (HTTP " & odp.Kod & ") dla przykładowego kafelka."
                    Exit For
                Else
                    ostatniBlad = "Serwer zwrócił błąd HTTP " & odp.Kod & " dla przykładowego kafelka (" & MetadaneMapy.MaskujSekrety(url) & ")."
                End If
            Next
            If zBase >= 0 OrElse ostatniBlad.StartsWith("Serwer odmówił", StringComparison.Ordinal) Then Exit For
        Next

        If zBase < 0 Then
            wynik.KomunikatBledu = If(ostatniBlad <> "", ostatniBlad, "Nie udało się pobrać testowego kafelka z podanego szablonu.")
            Return wynik
        End If

        'wykrywanie zMin (najpierw szybka próba z=0, bo większość map działa od 0)
        Dim zMin As Integer = zBase
        Dim odpZero = Await SprobujPobierzAsync(AdresKaflaDoProby(szablonNorm, subdomeny, 0, srodek, wykryteTms),
                                               ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
        If odpZero.Kod = 200 AndAlso KafleXyz.DiagnozujOdpowiedzKafla(odpZero.Bajty, odpZero.ContentType) = "" Then
            zMin = 0
        Else
            Dim lo As Integer = 1, hi As Integer = zBase - 1
            While lo <= hi
                Dim mid As Integer = (lo + hi) \ 2
                Dim odpMid = Await SprobujPobierzAsync(AdresKaflaDoProby(szablonNorm, subdomeny, mid, srodek, wykryteTms),
                                                      ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
                If odpMid.Kod = 200 AndAlso KafleXyz.DiagnozujOdpowiedzKafla(odpMid.Bajty, odpMid.ContentType) = "" Then
                    zMin = mid
                    hi = mid - 1
                Else
                    lo = mid + 1
                End If
            End While
        End If

        'wykrywanie zMax w zakresie [zBase..20]
        Dim zMax As Integer = zBase
        Dim loMax As Integer = zBase + 1, hiMax As Integer = 20
        While loMax <= hiMax
            Dim mid As Integer = (loMax + hiMax) \ 2
            Dim odpMid = Await SprobujPobierzAsync(AdresKaflaDoProby(szablonNorm, subdomeny, mid, srodek, wykryteTms),
                                                  ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
            If odpMid.Kod = 200 AndAlso KafleXyz.DiagnozujOdpowiedzKafla(odpMid.Bajty, odpMid.ContentType) = "" Then
                zMax = mid
                loMax = mid + 1
            Else
                hiMax = mid - 1
            End If
        End While

        Dim opcje As New OpcjeKafliXyz With {
            .Typ = If(wykryteTms, TypUslugiMapowej.Tms, TypUslugiMapowej.Xyz),
            .ZMin = zMin,
            .ZMax = zMax,
            .RozmiarKafla = rozmiarKafla,
            .FormatObrazu = KafleXyz.WykryjMimeZObrazu(probka, probkaCt),
            .IgnorujSsl = ignorujSsl
        }
        opcje.Subdomeny.AddRange(subdomeny)

        Dim nazwa As String = MetadaneMapy.Serwer(szablonNorm)
        If nazwa = "" Then nazwa = "Mapa kafelkowa"
        wynik.Typ = opcje.Typ
        wynik.Tytul = nazwa
        wynik.AdresCapabilities = szablonNorm
        wynik.AdresBazowy = szablonNorm
        wynik.SzablonKafli = szablonNorm
        wynik.OpcjeKafli = opcje
        wynik.PrzykladowyKafel = probka
        wynik.UslugaWmts = KafleXyz.UtworzUsluge(szablonNorm, {nazwa}, opcje)
        Return wynik
    End Function

    Private Shared Async Function ZbudujWynikZeSzablonuAsync(szablon As String, tytul As String, opcje As OpcjeKafliXyz,
                                                             adresOpisu As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                             token As CancellationToken, handler As HttpMessageHandler,
                                                             srodekTestowy As PunktGeo?) As Task(Of WynikCapabilities)
        Dim wynik As New WynikCapabilities()
        Dim sciezka As String = If(szablon, "").Split("?"c)(0).ToLowerInvariant()
        If sciezka.EndsWith(".mvt", StringComparison.Ordinal) OrElse sciezka.EndsWith(".pbf", StringComparison.Ordinal) Then
            wynik.KomunikatBledu = "Usługa udostępnia wektorowe kafle MVT/PBF, których MapoTero nie obsługuje (wymagane są kafle rastrowe PNG lub JPEG)."
            Return wynik
        End If

        Dim kopiaOpcji As OpcjeKafliXyz = If(opcje, New OpcjeKafliXyz()).Kopia()
        kopiaOpcji.IgnorujSsl = ignorujSsl
        Dim nazwa As String = If(String.IsNullOrWhiteSpace(tytul), MetadaneMapy.Serwer(szablon), tytul.Trim())
        If nazwa = "" Then nazwa = "Mapa kafelkowa"

        'pobranie pojedynczego przykładowego kafla do podglądu i wykrycia WebP/MVT/rozmiaru
        Dim srodek As PunktGeo = New PunktGeo(52.23, 21.01)
        If kopiaOpcji.ZasiegGeo.HasValue Then
            Dim g = kopiaOpcji.ZasiegGeo.Value
            srodek = New PunktGeo((g.Polnoc + g.Poludnie) / 2.0, (g.Wschod + g.Zachod) / 2.0)
        ElseIf srodekTestowy.HasValue Then
            srodek = srodekTestowy.Value
        End If
        Dim zTest As Integer = Math.Max(kopiaOpcji.ZMin, Math.Min(kopiaOpcji.ZMax, 10))
        Dim urlTest As String = AdresKaflaDoProby(szablon, kopiaOpcji.Subdomeny, zTest, srodek, kopiaOpcji.Typ = TypUslugiMapowej.Tms)
        Dim odp = Await SprobujPobierzAsync(urlTest, ignorujSsl, timeoutSekundy, token, handler).ConfigureAwait(False)
        If odp.Kod = 200 AndAlso odp.Bajty IsNot Nothing Then
            Dim szer As Integer, wys As Integer
            Dim diag As String = KafleXyz.DiagnozujOdpowiedzKafla(odp.Bajty, odp.ContentType, szer, wys, urlTest)
            If diag <> "" Then
                wynik.KomunikatBledu = diag
                Return wynik
            End If
            If szer >= 64 Then kopiaOpcji.RozmiarKafla = szer
            Dim wykrytyMime = KafleXyz.WykryjMimeZObrazu(odp.Bajty, odp.ContentType)
            If wykrytyMime <> "" Then kopiaOpcji.FormatObrazu = wykrytyMime
            wynik.PrzykladowyKafel = odp.Bajty
        End If

        wynik.Typ = kopiaOpcji.Typ
        wynik.Tytul = nazwa
        wynik.AdresCapabilities = adresOpisu
        wynik.AdresBazowy = szablon
        wynik.SzablonKafli = szablon
        wynik.OpcjeKafli = kopiaOpcji
        wynik.UslugaWmts = KafleXyz.UtworzUsluge(szablon, {nazwa}, kopiaOpcji)
        Return wynik
    End Function

    Private Shared Function AdresKaflaDoProby(szablon As String, subdomeny As IList(Of String),
                                              z As Integer, srodek As PunktGeo, odwroconeY As Boolean) As String
        Dim maks As Integer = (1 << z) - 1
        Dim col As Integer = Math.Max(0, Math.Min(maks, CInt(Math.Floor(WebMercator.PikselX(srodek.Dlugosc, z) / WebMercator.RozmiarKafla))))
        Dim row As Integer = Math.Max(0, Math.Min(maks, CInt(Math.Floor(WebMercator.PikselY(srodek.Szerokosc, z) / WebMercator.RozmiarKafla))))
        Dim y As Integer = If(odwroconeY, maks - row, row)
        Dim s As String = If(subdomeny IsNot Nothing AndAlso subdomeny.Count > 0, subdomeny(0), "")
        Return Regex.Replace(szablon, "\{([^}]+)\}",
                             Function(m)
                                 Select Case m.Groups(1).Value.Trim().ToLowerInvariant()
                                     Case "z", "tilematrix" : Return z.ToString(Globalization.CultureInfo.InvariantCulture)
                                     Case "x", "tilecol" : Return col.ToString(Globalization.CultureInfo.InvariantCulture)
                                     Case "y", "tilerow" : Return y.ToString(Globalization.CultureInfo.InvariantCulture)
                                     Case "-y" : Return (maks - row).ToString(Globalization.CultureInfo.InvariantCulture)
                                     Case "s" : Return s
                                     Case "r" : Return "" 'kafel o zwykłej gęstości; rozmiar kafla program odczyta z próbki
                                     Case "quadkey" : Return KafleXyz.QuadKey(z, col, row)
                                     Case Else : Return m.Value
                                 End Select
                             End Function)
    End Function

End Class
