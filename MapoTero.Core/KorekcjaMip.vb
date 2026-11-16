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

Imports System

''' <summary>Parametry przetwarzania obrazu pod kątem czytelności na ekranach transflektywnych (MIP / Garmin).</summary>
Public Class OpcjeKorekcjiMip

    ''' <summary>Czy optymalizacja obrazu pod ekrany MIP/słońce jest aktywna.</summary>
    Public Property Wlaczona As Boolean = False

    ''' <summary>Korekcja gamma (rozjaśnienie cieni/lasów): standardowo 0.75 (0.5..1.0).</summary>
    Public Property Gamma As Double = 0.75

    ''' <summary>Mnożnik kontrastu: standardowo 1.25 (+25%, zakres 1.0..2.0).</summary>
    Public Property Kontrast As Double = 1.25

    ''' <summary>Mnożnik nasycenia barw: standardowo 1.35 (+35%, zakres 1.0..2.5).</summary>
    Public Property Nasycenie As Double = 1.35

    ''' <summary>Siła wyostrzania krawędzi filtrem splotowym (Laplacian 3x3): 0.0..1.5, standardowo 0.6.</summary>
    Public Property Wyostrzanie As Double = 0.6

    Public Function Kopia() As OpcjeKorekcjiMip
        Return New OpcjeKorekcjiMip With {
            .Wlaczona = Me.Wlaczona,
            .Gamma = Me.Gamma,
            .Kontrast = Me.Kontrast,
            .Nasycenie = Me.Nasycenie,
            .Wyostrzanie = Me.Wyostrzanie
        }
    End Function

End Class

''' <summary>
''' Przetwarzanie rastrów pod kątem ekranów transflektywnych Garmin Memory-In-Pixel (MIP):
''' - korekcja gamma (rozjaśnia ciemne partie terenu i gęste lasy, które na MIP w słońcu stają się czarną plamą),
''' - podbicie kontrastu i nasycenia barw (przełamuje bladość 64-kolorowej palety w pełnym słońcu),
''' - filtr wyostrzający krawędzie (wydobywa wąskie ścieżki, granice i poziomice).
''' </summary>
Public NotInheritable Class KorekcjaMip

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Stosuje korekcję MIP bezpośrednio na tablicy pikseli Format32bppArgb (ARGB).
    ''' </summary>
    Public Shared Sub Zastosuj(piksele As Integer(), szerokosc As Integer, wysokosc As Integer, opcje As OpcjeKorekcjiMip)
        If piksele Is Nothing OrElse piksele.Length = 0 OrElse opcje Is Nothing OrElse Not opcje.Wlaczona Then Exit Sub
        If szerokosc <= 0 OrElse wysokosc <= 0 OrElse piksele.Length < szerokosc * wysokosc Then Exit Sub

        ' Tablica przeglądowa (LUT) dla kombinacji Gamma + Kontrast (błyskawiczne obliczenia)
        Dim lut(255) As Byte
        Dim gamma As Double = Math.Max(0.1, Math.Min(3.0, opcje.Gamma))
        Dim kontrast As Double = Math.Max(0.5, Math.Min(3.0, opcje.Kontrast))
        Dim invGamma As Double = 1.0 / gamma

        For i = 0 To 255
            ' 1. Normalizacja do 0..1
            Dim v As Double = i / 255.0
            ' 2. Korekcja gamma (dla gamma < 1 rozjaśnia tony ciemne i średnie)
            v = Math.Pow(v, gamma)
            ' 3. Kontrast wokół środka 0.5
            v = (v - 0.5) * kontrast + 0.5
            ' 4. Obcięcie i powrót do bajta
            lut(i) = CByte(Math.Max(0, Math.Min(255, Math.Round(v * 255.0))))
        Next

        Dim sat As Double = Math.Max(0.0, Math.Min(3.0, opcje.Nasycenie))
        Dim czyNasycenie As Boolean = Math.Abs(sat - 1.0) > 0.01

        ' Krok 1: Przetworzenie kolorów (nasycenie + gamma + kontrast)
        For i = 0 To piksele.Length - 1
            Dim c As Integer = piksele(i)
            Dim a As Integer = (c >> 24) And &HFF
            If a = 0 Then Continue For

            Dim r As Double = (c >> 16) And &HFF
            Dim g As Double = (c >> 8) And &HFF
            Dim b As Double = c And &HFF

            If czyNasycenie Then
                ' Jasność luminancji Rec. 709
                Dim lum As Double = 0.2126 * r + 0.7152 * g + 0.0722 * b
                r = lum + (r - lum) * sat
                g = lum + (g - lum) * sat
                b = lum + (b - lum) * sat
            End If

            Dim ir As Integer = CInt(Math.Max(0, Math.Min(255, Math.Round(r))))
            Dim ig As Integer = CInt(Math.Max(0, Math.Min(255, Math.Round(g))))
            Dim ib As Integer = CInt(Math.Max(0, Math.Min(255, Math.Round(b))))

            piksele(i) = (a << 24) Or (CInt(lut(ir)) << 16) Or (CInt(lut(ig)) << 8) Or CInt(lut(ib))
        Next

        ' Krok 2: Filtr wyostrzający krawędzie (Laplacian 3x3)
        Dim wSila As Double = Math.Max(0.0, Math.Min(2.0, opcje.Wyostrzanie))
        If wSila > 0.05 AndAlso szerokosc >= 3 AndAlso wysokosc >= 3 Then
            WyostrzLaplacian(piksele, szerokosc, wysokosc, wSila)
        End If
    End Sub

    ''' <summary>
    ''' Splot wyostrzający 3x3 (operator Laplace'a). Obejmuje także skrajne wiersze i kolumny - brakującego sąsiada
    ''' zastępuje piksel brzegowy. Pomijanie brzegów zostawiało na stykach pasów segmentów (GeoTIFF) i kafli linie
    ''' o połowę mniejszej ostrości. Sąsiad przezroczysty jest zastępowany pikselem środkowym (bez jasnej obwódki nakładki).
    ''' </summary>
    Private Shared Sub WyostrzLaplacian(piksele As Integer(), szer As Integer, wys As Integer, sila As Double)
        Dim kopia(piksele.Length - 1) As Integer
        Array.Copy(piksele, kopia, piksele.Length)

        For y = 0 To wys - 1
            Dim wierszGora = Math.Max(0, y - 1) * szer
            Dim wierszSrodek = y * szer
            Dim wierszDol = Math.Min(wys - 1, y + 1) * szer

            For x = 0 To szer - 1
                Dim idx = wierszSrodek + x
                Dim c = kopia(idx)
                Dim a = (c >> 24) And &HFF
                If a = 0 Then Continue For

                Dim rCenter = (c >> 16) And &HFF
                Dim gCenter = (c >> 8) And &HFF
                Dim bCenter = c And &HFF

                Dim up = Sasiad(kopia(wierszGora + x), c)
                Dim down = Sasiad(kopia(wierszDol + x), c)
                Dim left = Sasiad(kopia(wierszSrodek + Math.Max(0, x - 1)), c)
                Dim right = Sasiad(kopia(wierszSrodek + Math.Min(szer - 1, x + 1)), c)

                Dim rLap = 4 * rCenter - (((up >> 16) And &HFF) + ((down >> 16) And &HFF) + ((left >> 16) And &HFF) + ((right >> 16) And &HFF))
                Dim gLap = 4 * gCenter - (((up >> 8) And &HFF) + ((down >> 8) And &HFF) + ((left >> 8) And &HFF) + ((right >> 8) And &HFF))
                Dim bLap = 4 * bCenter - ((up And &HFF) + (down And &HFF) + (left And &HFF) + (right And &HFF))

                Dim rOut = CInt(Math.Max(0, Math.Min(255, Math.Round(rCenter + sila * rLap))))
                Dim gOut = CInt(Math.Max(0, Math.Min(255, Math.Round(gCenter + sila * gLap))))
                Dim bOut = CInt(Math.Max(0, Math.Min(255, Math.Round(bCenter + sila * bLap))))

                piksele(idx) = (a << 24) Or (rOut << 16) Or (gOut << 8) Or bOut
            Next
        Next
    End Sub

    Private Shared Function Sasiad(p As Integer, srodek As Integer) As Integer
        Return If(((p >> 24) And &HFF) = 0, srodek, p)
    End Function

End Class
