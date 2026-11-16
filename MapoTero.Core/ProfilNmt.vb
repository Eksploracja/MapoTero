'Copyright (C) <2026>  pajakt
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
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Statystyki profilu wysokościowego trasy.</summary>
    Public Class StatystykiProfilu
        Public Property MinWysokosc As Double
        Public Property MaxWysokosc As Double
        Public Property Podejscia As Double
        Public Property Zejscia As Double
        Public Property DlugoscKm As Double
        Public Property Punkty As IList(Of PunktProfilu)
        ''' <summary>Liczba punktów bez danych NMT (poza zasięgiem modelu, np. za granicą Polski).</summary>
        Public Property PunktyBezDanych As Integer

        Public Sub New()
            Punkty = New List(Of PunktProfilu)()
        End Sub
    End Class

    ''' <summary>Pojedynczy punkt profilu wysokościowego (Wysokosc = NaN - brak danych NMT).</summary>
    Public Structure PunktProfilu
        Public DystansMetry As Double
        Public Wysokosc As Double
        Public Pozycja As PunktGeo

        Public Sub New(dystans As Double, wysokosc As Double, pozycja As PunktGeo)
            Me.DystansMetry = dystans
            Me.Wysokosc = wysokosc
            Me.Pozycja = pozycja
        End Sub
    End Structure

    ''' <summary>
    ''' Obsługa pobierania rzędnych terenu z Numerycznego Modelu Terenu (NMT) GUGiK (lotniczy skaning LIDAR).
    ''' </summary>
    Public NotInheritable Class ProfilNmt

        Private Shared ReadOnly _klient As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(30)}

        ''' <summary>Próg histerezy podejść i zejść [m] - mniejsze wahania (szum położenia GPS na stoku) są pomijane.</summary>
        Public Const ProgHisterezy As Double = 1.5

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Pobiera wysokości w metrach n.p.m. dla listy punktów w WGS84 z usługi NMT GUGiK.
        ''' Punkty przeliczane są na układ PL-1992 i wysyłane w pakietach po 150 punktów.
        ''' Punkt bez danych (poza zasięgiem NMT, np. po słowackiej stronie granicy) ma wartość Double.NaN.
        ''' Gdy serwer mimo ponowień nie odpowiada, metoda zgłasza wyjątek - wcześniej zwracała po cichu zera
        ''' albo powielone wysokości, co dawało fałszywy profil.
        ''' </summary>
        Public Shared Async Function PobierzWysokosciAsync(punkty As IList(Of PunktGeo),
                                                           Optional token As CancellationToken = Nothing,
                                                           Optional postep As IProgress(Of Integer) = Nothing) As Task(Of Double())
            If punkty Is Nothing OrElse punkty.Count = 0 Then Return Array.Empty(Of Double)()

            Dim n = punkty.Count
            Dim wynik(n - 1) As Double
            Dim punkty1992(n - 1) As PunktXY
            For i = 0 To n - 1
                punkty1992(i) = UkladWspolrzednych.PL1992.ZWgs84(punkty(i))
            Next

            Const rozmiarPaczki As Integer = 150
            Dim przetworzono As Integer = 0
            For offset = 0 To n - 1 Step rozmiarPaczki
                token.ThrowIfCancellationRequested()
                Dim doPobrania = Math.Min(rozmiarPaczki, n - offset)
                Await PobierzPaczkeAsync(punkty1992, offset, doPobrania, wynik, token).ConfigureAwait(False)
                przetworzono += doPobrania
                postep?.Report(Math.Min(100, przetworzono * 100 \ n))
            Next
            Return wynik
        End Function

        ''' <summary>
        ''' Pobiera wysokości jednej paczki. Usługa GetHByPointList dla punktu bez danych zwraca 0.0, ale przesuwa
        ''' przy tym wysokości kolejnych punktów listy (wartości trafiają do złych współrzędnych - sprawdzone
        ''' 2026-11-16). Paczka z brakiem danych jest więc dzielona na połowy aż do pojedynczych punktów,
        ''' dla których odpowiedź jest jednoznaczna.
        ''' </summary>
        Private Shared Async Function PobierzPaczkeAsync(punkty1992 As PunktXY(), offset As Integer, liczba As Integer,
                                                       wynik As Double(), token As CancellationToken) As Task
            Dim h = Await ZapytajAsync(punkty1992, offset, liczba, token).ConfigureAwait(False)
            Dim brakDanych = False
            For i = 0 To liczba - 1
                If Double.IsNaN(h(i)) Then brakDanych = True : Exit For
            Next
            If Not brakDanych OrElse liczba = 1 Then
                Array.Copy(h, 0, wynik, offset, liczba)
                Return
            End If
            Dim polowa = liczba \ 2
            Await PobierzPaczkeAsync(punkty1992, offset, polowa, wynik, token).ConfigureAwait(False)
            Await PobierzPaczkeAsync(punkty1992, offset + polowa, liczba - polowa, wynik, token).ConfigureAwait(False)
        End Function

        ''' <summary>Jedno zapytanie GetHByPointList z ponawianiem (3 próby).</summary>
        Private Shared Async Function ZapytajAsync(punkty1992 As PunktXY(), offset As Integer, liczba As Integer,
                                                 token As CancellationToken) As Task(Of Double())
            Dim ci = CultureInfo.InvariantCulture
            Dim sb As New System.Text.StringBuilder("https://services.gugik.gov.pl/nmt/?request=GetHByPointList&list=")
            For i = 0 To liczba - 1
                If i > 0 Then sb.Append(","c)
                Dim p = punkty1992(offset + i)
                sb.Append(Math.Round(p.X, 1).ToString("F1", ci)).Append("%20").Append(Math.Round(p.Y, 1).ToString("F1", ci))
            Next

            Dim ostatniBlad As Exception = Nothing
            For proba = 1 To 3
                token.ThrowIfCancellationRequested()
                Try
                    Using odp = Await _klient.GetAsync(sb.ToString(), token).ConfigureAwait(False)
                        odp.EnsureSuccessStatusCode()
                        Dim tekst = Await odp.Content.ReadAsStringAsync().ConfigureAwait(False)
                        Dim h = SparsujOdpowiedz(tekst, liczba)
                        If h IsNot Nothing Then Return h
                        ostatniBlad = New InvalidDataException("nieoczekiwana odpowiedź serwera: " & Skroc(tekst))
                    End Using
                Catch ex As Exception When Not token.IsCancellationRequested
                    ostatniBlad = ex
                End Try
                If proba < 3 Then Await Task.Delay(1000 * proba, token).ConfigureAwait(False)
            Next
            Throw New InvalidOperationException("Serwer NMT GUGiK nie zwrócił wysokości po 3 próbach (" & ostatniBlad.Message & ").", ostatniBlad)
        End Function

        ''' <summary>
        ''' Odczytuje odpowiedź "X Y H,X Y H,..." usługi NMT. Zwraca Nothing, gdy liczba punktów się nie zgadza
        ''' albo wysokość nie jest liczbą; wysokość 0.0 (brak danych) - Double.NaN.
        ''' </summary>
        Public Shared Function SparsujOdpowiedz(tekst As String, oczekiwanaLiczba As Integer) As Double()
            If String.IsNullOrWhiteSpace(tekst) OrElse oczekiwanaLiczba <= 0 Then Return Nothing
            Dim ci = CultureInfo.InvariantCulture
            Dim czesci = tekst.Trim().Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
            If czesci.Length <> oczekiwanaLiczba Then Return Nothing
            Dim wynik(oczekiwanaLiczba - 1) As Double
            For i = 0 To oczekiwanaLiczba - 1
                Dim elementy = czesci(i).Trim().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                Dim h As Double
                If elementy.Length < 3 OrElse Not Double.TryParse(elementy(elementy.Length - 1), NumberStyles.Float, ci, h) Then Return Nothing
                wynik(i) = If(h = 0.0, Double.NaN, h)
            Next
            Return wynik
        End Function

        Private Shared Function Skroc(t As String) As String
            If t Is Nothing Then Return ""
            t = t.Trim()
            Return If(t.Length > 120, t.Substring(0, 120) & "…", t)
        End Function

        ''' <summary>
        ''' Oblicza statystyki profilu wysokościowego (min, max, podejścia, zejścia). Podejścia liczone są z histerezą:
        ''' różnica jest odnoszona do ostatniego punktu odniesienia, a nie do poprzedniego punktu - przy gęstym śladzie
        ''' GPS (punkty co kilka metrów) różnice sąsiednich punktów są mniejsze od progu i porównywanie sąsiadów
        ''' gubiło większość przewyższenia (np. +66 m zamiast +180 m). Punkty bez danych (NaN) są pomijane.
        ''' </summary>
        Public Shared Function ObliczStatystyki(punkty As IList(Of PunktGeo), wysokosci As IList(Of Double)) As StatystykiProfilu
            Dim st As New StatystykiProfilu()
            If punkty Is Nothing OrElse wysokosci Is Nothing OrElse punkty.Count = 0 OrElse punkty.Count <> wysokosci.Count Then
                Return st
            End If

            Dim minH As Double = Double.MaxValue
            Dim maxH As Double = Double.MinValue
            Dim podejscia As Double = 0
            Dim zejscia As Double = 0
            Dim dystans As Double = 0
            Dim odniesienie As Double = Double.NaN
            Dim bezDanych As Integer = 0

            For i = 0 To punkty.Count - 1
                Dim h = wysokosci(i)
                If i > 0 Then dystans += Georeferencja.OdlegloscMetry(punkty(i - 1), punkty(i))
                If Double.IsNaN(h) Then
                    bezDanych += 1
                Else
                    minH = Math.Min(minH, h)
                    maxH = Math.Max(maxH, h)
                    If Double.IsNaN(odniesienie) Then
                        odniesienie = h
                    ElseIf h - odniesienie >= ProgHisterezy Then
                        podejscia += h - odniesienie
                        odniesienie = h
                    ElseIf odniesienie - h >= ProgHisterezy Then
                        zejscia += odniesienie - h
                        odniesienie = h
                    End If
                End If
                st.Punkty.Add(New PunktProfilu(dystans, h, punkty(i)))
            Next

            st.MinWysokosc = If(minH = Double.MaxValue, 0, minH)
            st.MaxWysokosc = If(maxH = Double.MinValue, 0, maxH)
            st.Podejscia = podejscia
            st.Zejscia = zejscia
            st.DlugoscKm = dystans / 1000.0
            st.PunktyBezDanych = bezDanych
            Return st
        End Function

    End Class
