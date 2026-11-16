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

        Public Sub New()
            Punkty = New List(Of PunktProfilu)()
        End Sub
    End Class

    ''' <summary>Pojedynczy punkt profilu wysokościowego.</summary>
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

        Private Shared ReadOnly _klient As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(15)}

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Pobiera wysokości w metrach n.p.m. dla listy punktów w WGS84 z usługi NMT GUGiK.
        ''' Punkty przeliczane są na układ PL-1992 i wysyłane w pakietach po 150 punktów.
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
                Dim sb As New System.Text.StringBuilder("https://services.gugik.gov.pl/nmt/?request=GetHByPointList&list=")
                Dim ci = CultureInfo.InvariantCulture

                For i = 0 To doPobrania - 1
                    If i > 0 Then sb.Append(","c)
                    Dim p = punkty1992(offset + i)
                    sb.Append(Math.Round(p.X, 1).ToString("F1", ci)).Append("%20").Append(Math.Round(p.Y, 1).ToString("F1", ci))
                Next

                Try
                    Using odp = Await _klient.GetAsync(sb.ToString(), token).ConfigureAwait(False)
                        odp.EnsureSuccessStatusCode()
                        Dim tekst = Await odp.Content.ReadAsStringAsync().ConfigureAwait(False)
                        Dim czesci = tekst.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
                        For i = 0 To Math.Min(czesci.Length, doPobrania) - 1
                            Dim elementy = czesci(i).Trim().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                            Dim h As Double
                            If elementy.Length >= 3 AndAlso Double.TryParse(elementy(2), NumberStyles.Float, ci, h) Then
                                wynik(offset + i) = h
                            ElseIf offset + i > 0 Then
                                wynik(offset + i) = wynik(offset + i - 1)
                            End If
                        Next
                    End Using
                Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException)
                    For i = 0 To doPobrania - 1
                        wynik(offset + i) = If(offset + i > 0, wynik(offset + i - 1), 0.0)
                    Next
                End Try

                przetworzono += doPobrania
                postep?.Report(Math.Min(100, przetworzono * 100 \ n))
            Next

            Return wynik
        End Function

        ''' <summary>
        ''' Oblicza statystyki profilu wysokościowego (min, max, podejścia, zejścia).
        ''' Zastosowano próg różnicy 1.5 m dla podejść/zejść w celu wyeliminowania mikro-szumów.
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

            For i = 0 To punkty.Count - 1
                Dim h = wysokosci(i)
                If h > 0 Then
                    minH = Math.Min(minH, h)
                    maxH = Math.Max(maxH, h)
                End If
                If i > 0 Then
                    dystans += Georeferencja.OdlegloscMetry(punkty(i - 1), punkty(i))
                    Dim dh = wysokosci(i) - wysokosci(i - 1)
                    If Math.Abs(dh) >= 1.5 Then
                        If dh > 0 Then
                            podejscia += dh
                        Else
                            zejscia += Math.Abs(dh)
                        End If
                    End If
                End If
                st.Punkty.Add(New PunktProfilu(dystans, h, punkty(i)))
            Next

            st.MinWysokosc = If(minH = Double.MaxValue, 0, minH)
            st.MaxWysokosc = If(maxH = Double.MinValue, 0, maxH)
            st.Podejscia = podejscia
            st.Zejscia = zejscia
            st.DlugoscKm = dystans / 1000.0
            Return st
        End Function

    End Class
