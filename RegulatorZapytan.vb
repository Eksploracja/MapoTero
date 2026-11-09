'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Collections.Concurrent
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Regulacja obciążenia serwerów map. Każde zapytanie pobierania (segment WMS, kafel WMTS) wysyłane jest po otrzymaniu
''' przepustki regulatora, który:
''' <list type="bullet">
''' <item>ogranicza łączną liczbę jednoczesnych zapytań do jednego serwera do liczby wątków z ustawień - dawniej pobieranie
''' WMTS wysyłało do 4 zapytań o kafle na każdy wątek (przy 16 wątkach do 64 naraz), co groziło blokadą adresu IP,</item>
''' <item>hamuje, gdy serwer zgłasza przeciążenie (HTTP 429, 502, 503, 504) albo kolejne zapytania kończą się przekroczeniem
''' czasu lub błędem połączenia: wstrzymuje zapytania do serwera (na czas z nagłówka Retry-After albo coraz dłużej:
''' 5, 10, 20... do 60 s) i o połowę zmniejsza liczbę jednoczesnych zapytań,</item>
''' <item>po serii udanych odpowiedzi przywraca liczbę jednoczesnych zapytań stopniowo, po jednym.</item>
''' </list>
''' Oczekiwanie na przepustkę nie wlicza się do limitu czasu odpowiedzi serwera (limit liczony jest od wysłania zapytania).
''' Ponawianie nieudanych segmentów pozostaje w pobieraniu (ilość prób i przerwa między próbami z ustawień).
''' </summary>
Public NotInheritable Class RegulatorZapytan

    ''' <summary>Regulator zapytań pobierania segmentów i kafli.</summary>
    Public Shared ReadOnly Pobieranie As New RegulatorZapytan()

    ''' <summary>Przerwa po pierwszym przeciążeniu serwera (kolejne - dwa razy dłuższe), gdy serwer nie podał Retry-After.</summary>
    Public Property PrzerwaPoczatkowa As TimeSpan = TimeSpan.FromSeconds(5)

    ''' <summary>Najdłuższa przerwa wyznaczana przez program.</summary>
    Public Property PrzerwaMaksymalna As TimeSpan = TimeSpan.FromSeconds(60)

    ''' <summary>Najdłuższa przerwa żądana przez serwer w nagłówku Retry-After, którą program uwzględnia.</summary>
    Public Property PrzerwaRetryAfterMaksymalna As TimeSpan = TimeSpan.FromSeconds(120)

    ''' <summary>Tyle kolejnych przekroczeń czasu lub błędów połączenia oznacza przeciążenie serwera.</summary>
    Public Property BledyDoHamowania As Integer = 3

    ''' <summary>Po tylu kolejnych udanych odpowiedziach liczba jednoczesnych zapytań rośnie o 1.</summary>
    Public Property SukcesyDoPrzyspieszenia As Integer = 10

    Private ReadOnly _serwery As New ConcurrentDictionary(Of String, StanSerwera)(StringComparer.OrdinalIgnoreCase)
    Private _maksimum As Integer = 4
    Private _informuj As Action(Of String)

    ''' <summary>Stan regulacji zapytań do jednego serwera.</summary>
    Friend NotInheritable Class StanSerwera
        Public ReadOnly Serwer As String
        Public ReadOnly Maksimum As Integer
        Public ReadOnly Semafor As SemaphoreSlim
        Public ReadOnly Blokada As New Object()
        ''' <summary>Bieżąca liczba jednoczesnych zapytań.</summary>
        Public Limit As Integer
        ''' <summary>Przepustki zatrzymywane po zmniejszeniu limitu (zwalniane przepustki nie wracają do semafora).</summary>
        Public Dlug As Integer
        Public PrzerwaDo As DateTime = DateTime.MinValue
        Public OstatnieHamowanie As DateTime = DateTime.MinValue
        Public KolejneHamowania As Integer
        Public KolejneBledy As Integer
        Public Sukcesy As Integer

        Public Sub New(serwer As String, maksimum As Integer)
            Me.Serwer = serwer
            Me.Maksimum = maksimum
            Limit = maksimum
            Semafor = New SemaphoreSlim(maksimum, maksimum)
        End Sub
    End Class

    ''' <summary>
    ''' Przepustka jednego zapytania. Po odpowiedzi serwera wywołaj <see cref="Odpowiedz(HttpResponseMessage)"/>,
    ''' po przekroczeniu czasu lub błędzie połączenia - <see cref="Blad"/>; Dispose zwalnia miejsce dla kolejnego zapytania.
    ''' </summary>
    Public NotInheritable Class Przepustka
        Implements IDisposable

        Private ReadOnly _regulator As RegulatorZapytan
        Private ReadOnly _stan As StanSerwera
        Private ReadOnly _wyslano As DateTime
        Private _zwolniona As Integer

        Friend Sub New(regulator As RegulatorZapytan, stan As StanSerwera)
            _regulator = regulator
            _stan = stan
            _wyslano = DateTime.UtcNow
        End Sub

        ''' <summary>Odpowiedź serwera (kod HTTP i ewentualny nagłówek Retry-After).</summary>
        Public Sub Odpowiedz(odpowiedz As HttpResponseMessage)
            If odpowiedz Is Nothing Then Exit Sub
            Dim retryAfter As TimeSpan? = Nothing
            Dim naglowek = odpowiedz.Headers.RetryAfter
            If naglowek IsNot Nothing Then
                If naglowek.Delta.HasValue Then
                    retryAfter = naglowek.Delta.Value
                ElseIf naglowek.Date.HasValue Then
                    retryAfter = naglowek.Date.Value.UtcDateTime - DateTime.UtcNow
                End If
            End If
            Me.Odpowiedz(odpowiedz.StatusCode, retryAfter)
        End Sub

        ''' <summary>Odpowiedź serwera o podanym kodzie HTTP.</summary>
        Public Sub Odpowiedz(kod As HttpStatusCode, Optional retryAfter As TimeSpan? = Nothing)
            _regulator.Odpowiedz(_stan, _wyslano, kod, retryAfter)
        End Sub

        ''' <summary>Brak odpowiedzi: przekroczenie czasu albo błąd połączenia.</summary>
        Public Sub Blad()
            _regulator.Blad(_stan, _wyslano)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.Exchange(_zwolniona, 1) = 0 Then Zwolnij(_stan)
        End Sub
    End Class

    ''' <summary>
    ''' Początek pobierania: najwyżej tyle jednoczesnych zapytań do jednego serwera (liczba wątków z ustawień) oraz
    ''' powiadamianie użytkownika o hamowaniu. Stan poprzedniego pobierania jest zapominany.
    ''' </summary>
    Public Sub Rozpocznij(maksimum As Integer, Optional informuj As Action(Of String) = Nothing)
        _maksimum = Math.Max(1, maksimum)
        _informuj = informuj
        _serwery.Clear()
    End Sub

    ''' <summary>
    ''' Czeka na wolne miejsce dla zapytania do serwera z adresu (i na koniec przerwy po przeciążeniu serwera).
    ''' Przepustkę trzeba zwolnić (Dispose) po otrzymaniu odpowiedzi.
    ''' </summary>
    Public Async Function WejdzAsync(adres As String, token As CancellationToken) As Task(Of Przepustka)
        Dim stan As StanSerwera = StanDla(adres)
        Await stan.Semafor.WaitAsync(token).ConfigureAwait(False)
        Try
            Do
                Dim czekaj As TimeSpan
                SyncLock stan.Blokada
                    czekaj = stan.PrzerwaDo - DateTime.UtcNow
                End SyncLock
                If czekaj <= TimeSpan.Zero Then Exit Do
                Await Task.Delay(czekaj, token).ConfigureAwait(False)
            Loop
        Catch
            Zwolnij(stan)
            Throw
        End Try
        Return New Przepustka(Me, stan)
    End Function

    ''' <summary>Bieżąca liczba jednoczesnych zapytań do serwera z adresu.</summary>
    Public Function Limit(adres As String) As Integer
        Dim stan As StanSerwera = StanDla(adres)
        SyncLock stan.Blokada
            Return stan.Limit
        End SyncLock
    End Function

    ''' <summary>Pozostała przerwa w zapytaniach do serwera z adresu (zero - bez przerwy).</summary>
    Public Function PozostalaPrzerwa(adres As String) As TimeSpan
        Dim stan As StanSerwera = StanDla(adres)
        SyncLock stan.Blokada
            Dim czas As TimeSpan = stan.PrzerwaDo - DateTime.UtcNow
            Return If(czas > TimeSpan.Zero, czas, TimeSpan.Zero)
        End SyncLock
    End Function

    ''' <summary>Serwer z adresu zapytania (nazwa i port); adres niepoprawny - cały tekst.</summary>
    Public Shared Function NazwaSerwera(adres As String) As String
        Dim uri As Uri = Nothing
        If Uri.TryCreate(adres, UriKind.Absolute, uri) AndAlso Not String.IsNullOrEmpty(uri.Authority) Then Return uri.Authority
        Return If(adres, "")
    End Function

    Private Function StanDla(adres As String) As StanSerwera
        Dim maksimum As Integer = _maksimum
        Return _serwery.GetOrAdd(NazwaSerwera(adres), Function(serwer) New StanSerwera(serwer, maksimum))
    End Function

    Private Shared Sub Zwolnij(stan As StanSerwera)
        SyncLock stan.Blokada
            If stan.Dlug > 0 Then
                stan.Dlug -= 1
                Exit Sub
            End If
        End SyncLock
        stan.Semafor.Release()
    End Sub

    Private Sub Odpowiedz(stan As StanSerwera, wyslano As DateTime, kod As HttpStatusCode, retryAfter As TimeSpan?)
        Dim komunikat As String = Nothing
        Dim zwolnij As Boolean = False
        SyncLock stan.Blokada
            stan.KolejneBledy = 0
            Select Case CInt(kod)
                Case 429, 502, 503, 504
                    komunikat = Hamuj(stan, wyslano, "HTTP " & CInt(kod), retryAfter)
                Case 200 To 299, 404
                    'przyspieszanie po serii udanych odpowiedzi (404 - kafel WMTS poza obszarem danych, też poprawna odpowiedź)
                    stan.Sukcesy += 1
                    If stan.Limit < stan.Maksimum AndAlso stan.Sukcesy >= SukcesyDoPrzyspieszenia Then
                        stan.Sukcesy = 0
                        stan.Limit += 1
                        If stan.Dlug > 0 Then
                            stan.Dlug -= 1
                        Else
                            zwolnij = True
                        End If
                        If stan.Limit = stan.Maksimum Then stan.KolejneHamowania = 0
                    End If
            End Select
        End SyncLock
        If zwolnij Then stan.Semafor.Release()
        If komunikat IsNot Nothing Then _informuj?.Invoke(komunikat)
    End Sub

    Private Sub Blad(stan As StanSerwera, wyslano As DateTime)
        Dim komunikat As String = Nothing
        SyncLock stan.Blokada
            stan.Sukcesy = 0
            stan.KolejneBledy += 1
            If stan.KolejneBledy >= BledyDoHamowania Then
                komunikat = Hamuj(stan, wyslano, "brak odpowiedzi", Nothing)
            End If
        End SyncLock
        If komunikat IsNot Nothing Then _informuj?.Invoke(komunikat)
    End Sub

    ''' <summary>
    ''' Przerwa w zapytaniach i zmniejszenie o połowę liczby jednoczesnych zapytań (wywoływane w blokadzie stanu).
    ''' Zwraca komunikat dla użytkownika albo Nothing, gdy zgłoszenie dotyczy zapytania wysłanego przed ostatnim
    ''' hamowaniem - kilka zapytań wysłanych równocześnie zgłasza zwykle to samo przeciążenie.
    ''' </summary>
    Private Function Hamuj(stan As StanSerwera, wyslano As DateTime, przyczyna As String, retryAfter As TimeSpan?) As String
        If wyslano < stan.OstatnieHamowanie Then Return Nothing
        Dim teraz As DateTime = DateTime.UtcNow
        stan.KolejneHamowania += 1
        stan.KolejneBledy = 0
        stan.Sukcesy = 0

        Dim przerwa As TimeSpan
        If retryAfter.HasValue AndAlso retryAfter.Value > TimeSpan.Zero Then
            przerwa = If(retryAfter.Value > PrzerwaRetryAfterMaksymalna, PrzerwaRetryAfterMaksymalna, retryAfter.Value)
        Else
            Dim mnoznik As Double = Math.Pow(2, Math.Min(stan.KolejneHamowania - 1, 16))
            przerwa = TimeSpan.FromTicks(CLng(Math.Min(PrzerwaPoczatkowa.Ticks * mnoznik, PrzerwaMaksymalna.Ticks)))
        End If
        stan.PrzerwaDo = teraz + przerwa
        stan.OstatnieHamowanie = teraz

        Dim nowyLimit As Integer = Math.Max(1, stan.Limit \ 2)
        stan.Dlug += stan.Limit - nowyLimit
        stan.Limit = nowyLimit

        Return "Serwer " & stan.Serwer & " jest przeciążony (" & przyczyna & ") - przerwa " &
               Math.Max(1, CInt(Math.Ceiling(przerwa.TotalSeconds))) & " s, potem najwyżej " & nowyLimit &
               Odmiana(nowyLimit, " zapytanie", " zapytania", " zapytań") & " naraz."
    End Function

End Class
