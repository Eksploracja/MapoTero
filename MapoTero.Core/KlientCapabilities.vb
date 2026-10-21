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
End Enum

''' <summary>Rezultat pobrania i analizy opisu usługi GetCapabilities.</summary>
Public Class WynikCapabilities

    Public Property Typ As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    Public Property UslugaWms As UslugaWmsInfo
    Public Property UslugaWmts As UslugaWmts
    Public Property AdresCapabilities As String = ""
    Public Property AdresBazowy As String = ""
    Public Property Tytul As String = ""
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

    ''' <summary>Pobiera i analizuje opis usługi z podanego adresu URL.</summary>
    Public Shared Async Function PobierzAsync(adres As String, Optional ignorujSsl As Boolean = False,
                                             Optional timeoutSekundy As Integer = 15,
                                             Optional token As CancellationToken = Nothing) As Task(Of WynikCapabilities)
        Dim wynik As New WynikCapabilities()
        If String.IsNullOrWhiteSpace(adres) Then
            wynik.KomunikatBledu = "Podaj adres serwera WMS lub WMTS."
            Return wynik
        End If

        Dim adresWyczyszczony As String = adres.Trim()
        Dim czyPodejrzenieWmts As Boolean = Wmts.CzyAdresWmts(adresWyczyszczony)

        'kolejność prób: jeśli adres wygląda na WMTS, próbujemy najpierw WMTS; w przeciwnym razie najpierw WMS
        Dim adresyPrób As New List(Of Tuple(Of String, TypUslugiMapowej))
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
                Dim bajty = Await PobierzBajtyAsync(adresDoPobrania, ignorujSsl, timeoutSekundy, token).ConfigureAwait(False)
                If bajty Is Nothing OrElse bajty.Length = 0 Then
                    ostatniBlad = "Serwer zwrócił pustą odpowiedź."
                    Continue For
                End If

                'sprawdzenie, czy serwer nie zwrócił strony HTML (błąd 502, Cloudflare, portal WWW)
                Dim naglowekTekst As String = System.Text.Encoding.ASCII.GetString(bajty, 0, Math.Min(bajty.Length, 150)).TrimStart()
                If naglowekTekst.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) OrElse
                   naglowekTekst.StartsWith("<html", StringComparison.OrdinalIgnoreCase) Then
                    ostatniBlad = "Serwer zwrócił stronę WWW zamiast dokumentu XML. Upewnij się, że adres wskazuje usługę WMS/WMTS, a nie stronę informacyjną geoportalu."
                    Continue For
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
                    wynik.KomunikatBledu = "Podany adres wskazuje usługę wektorową WFS. MapoTero obsługuje rastrowe usługi WMS oraz kafelkowe WMTS."
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

                'sprawdzenie czy WMTS
                If nazwaKorzenia.Equals("Capabilities", StringComparison.OrdinalIgnoreCase) AndAlso
                   (doc.Descendants().Any(Function(x) x.Name.LocalName.Equals("TileMatrixSet", StringComparison.OrdinalIgnoreCase)) OrElse
                    proba.Item2 = TypUslugiMapowej.Wmts) Then
                    Using msWmts As New MemoryStream(bajty)
                        Dim xmlTekst = (New StreamReader(msWmts, True)).ReadToEnd()
                        Dim usluga = UslugaWmts.Wczytaj(xmlTekst, adresDoPobrania)
                        wynik.Typ = TypUslugiMapowej.Wmts
                        wynik.UslugaWmts = usluga
                        wynik.AdresCapabilities = adresDoPobrania
                        wynik.AdresBazowy = adresWyczyszczony
                        wynik.Tytul = If(usluga.Warstwy.Count > 0 AndAlso Not String.IsNullOrEmpty(usluga.Warstwy(0).Tytul),
                                         usluga.Warstwy(0).Tytul, "Usługa WMTS")
                        SprawdzUkładyWmts(usluga, wynik)
                        Return wynik
                    End Using
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
    Private Shared Async Function PobierzBajtyAsync(url As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                    token As CancellationToken) As Task(Of Byte())
        For proba = 1 To LiczbaProbPobrania
            Try
                Return Await PobierzBajtyRazAsync(url, ignorujSsl, timeoutSekundy, token).ConfigureAwait(False)
            Catch ex As BladHttp When ex.Chwilowy AndAlso proba < LiczbaProbPobrania
            Catch ex As TimeoutException When proba < LiczbaProbPobrania
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

    Private Shared Async Function PobierzBajtyRazAsync(url As String, ignorujSsl As Boolean, timeoutSekundy As Integer,
                                                       token As CancellationToken) As Task(Of Byte())
        Dim obsluga As New HttpClientHandler()
        obsluga.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        If ignorujSsl Then
            obsluga.ServerCertificateCustomValidationCallback = Function(msg, cert, chain, errors) True
        End If

        Using ctsTimeout As New CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, timeoutSekundy))),
              ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(token, ctsTimeout.Token),
              klient As New HttpClient(obsluga)
            klient.DefaultRequestHeaders.UserAgent.ParseAdd("MapoTero/3.14")
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

End Class
