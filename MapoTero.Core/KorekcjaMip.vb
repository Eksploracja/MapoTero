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

''' <summary>Gotowe profile przetwarzania obrazu (presety).</summary>
Public Enum ProfilKorektyRastra
    ''' <summary>Brak korekty (oryginalne kolory i ostrość).</summary>
    Brak = 0
    ''' <summary>Optymalizacja dla wyświetlaczy Garmin Chroma / MIP w pełnym słońcu.</summary>
    GarminMip = 1
    ''' <summary>Umiarkowane wyostrzenie dróg, ścieżek i napisów bez zmiany barw.</summary>
    WyostrzenieKrawedzi = 2
    ''' <summary>Wysoki kontrast dla starych map topograficznych, skanów WIG i cieniowania rzeźby.</summary>
    StareMapyKontrast = 3
    ''' <summary>Rozjaśnienie gęstych lasów i głębokich cieni bez utraty bieli.</summary>
    RozjasnienieLasow = 4
    ''' <summary>Podkład do programów CAD: rozjaśniony, nienachalny, nie zasłaniający wektorów.</summary>
    PodkladCadStonowany = 5
    ''' <summary>Skan archiwalny w CAD: czarno-biały o podbitym kontraście i wyostrzonych liniach.</summary>
    SkanCadCzarnoBialy = 6
    ''' <summary>Ręcznie dostrojone parametry suwaków.</summary>
    Wlasny = 7
End Enum

''' <summary>Parametry przetwarzania obrazu pod kątem czytelności na ekranach transflektywnych (MIP / Garmin) oraz wydruku i urządzeń mobilnych.</summary>
Public Class OpcjeKorekcjiMip

    ''' <summary>Czy optymalizacja obrazu jest aktywna.</summary>
    Public Property Wlaczona As Boolean = False

    ''' <summary>Wybrany profil gotowych ustawień.</summary>
    Public Property Profil As ProfilKorektyRastra = ProfilKorektyRastra.Brak

    ''' <summary>Korekcja gamma: poniżej 1 rozjaśnia cienie i lasy, powyżej 1 przyciemnia; standardowo 0.75 (zakres 0.5..1.5).</summary>
    Public Property Gamma As Double = 0.75

    ''' <summary>
    ''' Mnożnik kontrastu: standardowo 1.25 (+25%, zakres 0.5..2.0). Powyżej 1 rozciąga tony wokół środka skali,
    ''' poniżej 1 spłaszcza je w stronę bieli (podkład rozjaśniony, biel pozostaje biała).
    ''' </summary>
    Public Property Kontrast As Double = 1.25

    ''' <summary>Mnożnik nasycenia barw: standardowo 1.35 (+35%, zakres 0.0..2.5; 0 - obraz czarno-biały).</summary>
    Public Property Nasycenie As Double = 1.35

    ''' <summary>Siła wyostrzania krawędzi filtrem splotowym (Laplacian 3x3): 0.0..1.5, standardowo 0.6.</summary>
    Public Property Wyostrzanie As Double = 0.6

    Public Sub UstawWgProfilu(p As ProfilKorektyRastra)
        Me.Profil = p
        Select Case p
            Case ProfilKorektyRastra.GarminMip
                Me.Wlaczona = True
                Me.Gamma = 0.75
                Me.Kontrast = 1.25
                Me.Nasycenie = 1.35
                Me.Wyostrzanie = 0.6
            Case ProfilKorektyRastra.WyostrzenieKrawedzi
                Me.Wlaczona = True
                Me.Gamma = 1.0
                Me.Kontrast = 1.1
                Me.Nasycenie = 1.0
                Me.Wyostrzanie = 0.5
            Case ProfilKorektyRastra.StareMapyKontrast
                Me.Wlaczona = True
                Me.Gamma = 0.9
                Me.Kontrast = 1.4
                Me.Nasycenie = 1.1
                Me.Wyostrzanie = 0.75
            Case ProfilKorektyRastra.RozjasnienieLasow
                Me.Wlaczona = True
                Me.Gamma = 0.7
                Me.Kontrast = 1.1
                Me.Nasycenie = 1.2
                Me.Wyostrzanie = 0.3
            Case ProfilKorektyRastra.PodkladCadStonowany
                'gamma < 1 rozjaśnia tony średnie, kontrast < 1 spłaszcza tony w stronę bieli (biel zostaje biała);
                'dawne gamma 1,25 i kontrast 0,85 wokół środka przyciemniały obraz i zamieniały biel w szarość (255 -> 236)
                Me.Wlaczona = True
                Me.Gamma = 0.85
                Me.Kontrast = 0.6
                Me.Nasycenie = 0.6
                Me.Wyostrzanie = 0.0
            Case ProfilKorektyRastra.SkanCadCzarnoBialy
                Me.Wlaczona = True
                Me.Gamma = 0.85
                Me.Kontrast = 1.5
                Me.Nasycenie = 0.0
                Me.Wyostrzanie = 0.75
            Case ProfilKorektyRastra.Brak
                Me.Wlaczona = False
            Case Else
                Me.Wlaczona = True
        End Select
    End Sub

    Public Shared Function NazwaProfilu(p As ProfilKorektyRastra) As String
        Select Case p
            Case ProfilKorektyRastra.GarminMip : Return "Ekran w słońcu / Garmin MIP (zalecany)"
            Case ProfilKorektyRastra.WyostrzenieKrawedzi : Return "Wyostrzenie krawędzi i napisów"
            Case ProfilKorektyRastra.StareMapyKontrast : Return "Stare mapy i skany (wysoki kontrast)"
            Case ProfilKorektyRastra.RozjasnienieLasow : Return "Rozjaśnienie ciemnych lasów i cieni"
            Case ProfilKorektyRastra.PodkladCadStonowany : Return "Podkład CAD (rozjaśniony, stonowany)"
            Case ProfilKorektyRastra.SkanCadCzarnoBialy : Return "Skan CAD (czarno-biały, wysoki kontrast)"
            Case ProfilKorektyRastra.Wlasny : Return "Własne parametry (ręczna regulacja)"
            Case Else : Return "Brak (oryginalny raster)"
        End Select
    End Function

    Public Shared Function KrotkaNazwaProfilu(p As ProfilKorektyRastra) As String
        Select Case p
            Case ProfilKorektyRastra.GarminMip : Return "Garmin MIP"
            Case ProfilKorektyRastra.WyostrzenieKrawedzi : Return "Wyostrzenie"
            Case ProfilKorektyRastra.StareMapyKontrast : Return "Wysoki kontrast"
            Case ProfilKorektyRastra.RozjasnienieLasow : Return "Jasne lasy"
            Case ProfilKorektyRastra.PodkladCadStonowany : Return "Podkład CAD"
            Case ProfilKorektyRastra.SkanCadCzarnoBialy : Return "Skan CAD"
            Case ProfilKorektyRastra.Wlasny : Return "Własny"
            Case Else : Return "Brak"
        End Select
    End Function

    ''' <summary>
    ''' Opcje odtworzone z zapisanych ustawień: profil gotowy ustawia swoje wartości, profil „Własny” - zapisane
    ''' wartości suwaków (dawniej zapisywany był tylko numer profilu, więc własne wartości ginęły po zamknięciu okna).
    ''' Korekta jest włączona tylko wtedy, gdy była włączona i profil nie jest „Brak” - UstawWgProfilu włącza ją zawsze,
    ''' więc wywołanie go po odczycie „włączona” ponownie zaznaczało odznaczoną korektę.
    ''' </summary>
    Public Shared Function Odtworz(wlaczona As Boolean, profil As ProfilKorektyRastra, gamma As Double, kontrast As Double,
                                   nasycenie As Double, wyostrzanie As Double) As OpcjeKorekcjiMip
        Dim o As New OpcjeKorekcjiMip()
        If Not [Enum].IsDefined(GetType(ProfilKorektyRastra), profil) Then profil = ProfilKorektyRastra.Brak
        If profil = ProfilKorektyRastra.Wlasny Then
            o.Profil = profil
            o.Gamma = Ogranicz(gamma, 0.5, 1.5, o.Gamma)
            o.Kontrast = Ogranicz(kontrast, 0.5, 2.0, o.Kontrast)
            o.Nasycenie = Ogranicz(nasycenie, 0.0, 2.5, o.Nasycenie)
            o.Wyostrzanie = Ogranicz(wyostrzanie, 0.0, 1.5, o.Wyostrzanie)
        Else
            o.UstawWgProfilu(profil)
        End If
        o.Wlaczona = wlaczona AndAlso profil <> ProfilKorektyRastra.Brak
        Return o
    End Function

    Private Shared Function Ogranicz(v As Double, min As Double, max As Double, domyslna As Double) As Double
        If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return domyslna
        Return Math.Max(min, Math.Min(max, v))
    End Function

    ''' <summary>Kopia z samym wyostrzaniem (barwy bez zmian) - dla kafli niższych poziomów piramidy, których barwy już poprawiono.</summary>
    Public Function TylkoWyostrzanie() As OpcjeKorekcjiMip
        Return New OpcjeKorekcjiMip With {.Wlaczona = Me.Wlaczona, .Profil = ProfilKorektyRastra.Wlasny,
                                          .Gamma = 1.0, .Kontrast = 1.0, .Nasycenie = 1.0, .Wyostrzanie = Me.Wyostrzanie}
    End Function

    ''' <summary>Czy korekta wyostrza obraz (wyostrzanie potrzebuje sąsiednich pikseli - marginesu wokół kafla lub pasa).</summary>
    Public ReadOnly Property Wyostrza As Boolean
        Get
            Return Wlaczona AndAlso Wyostrzanie > 0.05
        End Get
    End Property

    Public Function Kopia() As OpcjeKorekcjiMip
        Return New OpcjeKorekcjiMip With {
            .Wlaczona = Me.Wlaczona,
            .Profil = Me.Profil,
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
    ''' Tablica przeglądowa tonów: gamma, potem kontrast. Kontrast powyżej 1 rozciąga tony wokół środka 0,5;
    ''' poniżej 1 spłaszcza je w stronę bieli - kontrast wokół środka zamieniał biel w szarość (podkład CAD był ciemniejszy).
    ''' </summary>
    Friend Shared Function TablicaTonow(opcje As OpcjeKorekcjiMip) As Byte()
        Dim lut(255) As Byte
        Dim gamma As Double = Math.Max(0.1, Math.Min(3.0, opcje.Gamma))
        Dim kontrast As Double = Math.Max(0.5, Math.Min(3.0, opcje.Kontrast))
        For i = 0 To 255
            Dim v As Double = Math.Pow(i / 255.0, gamma)
            If kontrast >= 1.0 Then
                v = (v - 0.5) * kontrast + 0.5
            Else
                v = 1.0 - (1.0 - v) * kontrast
            End If
            lut(i) = CByte(Math.Max(0, Math.Min(255, Math.Round(v * 255.0))))
        Next
        Return lut
    End Function

    ''' <summary>
    ''' Stosuje korekcję MIP bezpośrednio na tablicy pikseli Format32bppArgb (ARGB).
    ''' </summary>
    Public Shared Sub Zastosuj(piksele As Integer(), szerokosc As Integer, wysokosc As Integer, opcje As OpcjeKorekcjiMip)
        If piksele Is Nothing OrElse piksele.Length = 0 OrElse opcje Is Nothing OrElse Not opcje.Wlaczona Then Exit Sub
        If szerokosc <= 0 OrElse wysokosc <= 0 OrElse piksele.Length < szerokosc * wysokosc Then Exit Sub

        ' Tablica przeglądowa (LUT) dla kombinacji Gamma + Kontrast (błyskawiczne obliczenia)
        Dim lut = TablicaTonow(opcje)

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
        If wSila > 0.05 Then 'także pas 1-2 wierszy - brakującego sąsiada zastępuje piksel brzegowy
            WyostrzLaplacian(piksele, szerokosc, wysokosc, wSila)
        End If
    End Sub

    ''' <summary>
    ''' Splot wyostrzający 3x3 (operator Laplace'a). Obejmuje także skrajne wiersze i kolumny - brakującego sąsiada
    ''' zastępuje piksel brzegowy. Na brzegu wyostrzanie jest przez to słabsze w jednym kierunku (o ok. 11% mniejszy
    ''' kontrast lokalny), więc obraz dzielony na pasy lub kafle trzeba poprawiać z marginesem prawdziwych sąsiadów
    ''' (KorekcjaPasow, Wytnij). Sąsiad przezroczysty jest zastępowany pikselem środkowym (bez jasnej obwódki nakładki).
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

    ''' <summary>
    ''' Stosuje korekcję obrazu bezpośrednio na buforze bajtowym RGB lub RGBA (używane m.in. przy scalaniu segmentów).
    ''' </summary>
    Public Shared Sub ZastosujDoBuforaRgb(bufor As Byte(), szerokosc As Integer, wysokosc As Integer, kanaly As Integer, opcje As OpcjeKorekcjiMip)
        If bufor Is Nothing OrElse bufor.Length = 0 OrElse opcje Is Nothing OrElse Not opcje.Wlaczona Then Exit Sub
        If szerokosc <= 0 OrElse wysokosc <= 0 OrElse kanaly < 3 OrElse bufor.Length < szerokosc * wysokosc * kanaly Then Exit Sub

        ' 1. Tablica przeglądowa LUT dla Gamma + Kontrast
        Dim lut = TablicaTonow(opcje)

        Dim sat As Double = Math.Max(0.0, Math.Min(3.0, opcje.Nasycenie))
        Dim czyNasycenie As Boolean = Math.Abs(sat - 1.0) > 0.01

        ' Krok 1: Przetworzenie kolorów (nasycenie + gamma + kontrast)
        For y = 0 To wysokosc - 1
            Dim wierszStart = y * szerokosc * kanaly
            For x = 0 To szerokosc - 1
                Dim idx = wierszStart + x * kanaly
                If kanaly = 4 AndAlso bufor(idx + 3) = 0 Then Continue For

                Dim r As Double = bufor(idx)
                Dim g As Double = bufor(idx + 1)
                Dim b As Double = bufor(idx + 2)

                If czyNasycenie Then
                    Dim lum As Double = 0.2126 * r + 0.7152 * g + 0.0722 * b
                    r = lum + (r - lum) * sat
                    g = lum + (g - lum) * sat
                    b = lum + (b - lum) * sat
                End If

                Dim ir = CInt(Math.Max(0, Math.Min(255, Math.Round(r))))
                Dim ig = CInt(Math.Max(0, Math.Min(255, Math.Round(g))))
                Dim ib = CInt(Math.Max(0, Math.Min(255, Math.Round(b))))

                bufor(idx) = lut(ir)
                bufor(idx + 1) = lut(ig)
                bufor(idx + 2) = lut(ib)
            Next
        Next

        ' Krok 2: Filtr wyostrzający krawędzie (Laplacian 3x3)
        Dim wSila As Double = Math.Max(0.0, Math.Min(2.0, opcje.Wyostrzanie))
        If wSila > 0.05 Then 'także pas 1-2 wierszy - brakującego sąsiada zastępuje piksel brzegowy
            WyostrzLaplacianRgb(bufor, szerokosc, wysokosc, kanaly, wSila)
        End If
    End Sub

    Private Shared Sub WyostrzLaplacianRgb(bufor As Byte(), szer As Integer, wys As Integer, kanaly As Integer, sila As Double)
        Dim kopia(bufor.Length - 1) As Byte
        Array.Copy(bufor, kopia, bufor.Length)

        For y = 0 To wys - 1
            Dim wierszGora = Math.Max(0, y - 1) * szer * kanaly
            Dim wierszSrodek = y * szer * kanaly
            Dim wierszDol = Math.Min(wys - 1, y + 1) * szer * kanaly

            For x = 0 To szer - 1
                Dim idx = wierszSrodek + x * kanaly
                If kanaly = 4 AndAlso kopia(idx + 3) = 0 Then Continue For

                Dim idxGora = wierszGora + x * kanaly
                Dim idxDol = wierszDol + x * kanaly
                Dim idxLewo = wierszSrodek + Math.Max(0, x - 1) * kanaly
                Dim idxPrawo = wierszSrodek + Math.Min(szer - 1, x + 1) * kanaly

                Dim rC = CInt(kopia(idx)), gC = CInt(kopia(idx + 1)), bC = CInt(kopia(idx + 2))

                Dim rUp = If(kanaly = 4 AndAlso kopia(idxGora + 3) = 0, rC, CInt(kopia(idxGora)))
                Dim gUp = If(kanaly = 4 AndAlso kopia(idxGora + 3) = 0, gC, CInt(kopia(idxGora + 1)))
                Dim bUp = If(kanaly = 4 AndAlso kopia(idxGora + 3) = 0, bC, CInt(kopia(idxGora + 2)))

                Dim rDn = If(kanaly = 4 AndAlso kopia(idxDol + 3) = 0, rC, CInt(kopia(idxDol)))
                Dim gDn = If(kanaly = 4 AndAlso kopia(idxDol + 3) = 0, gC, CInt(kopia(idxDol + 1)))
                Dim bDn = If(kanaly = 4 AndAlso kopia(idxDol + 3) = 0, bC, CInt(kopia(idxDol + 2)))

                Dim rLf = If(kanaly = 4 AndAlso kopia(idxLewo + 3) = 0, rC, CInt(kopia(idxLewo)))
                Dim gLf = If(kanaly = 4 AndAlso kopia(idxLewo + 3) = 0, gC, CInt(kopia(idxLewo + 1)))
                Dim bLf = If(kanaly = 4 AndAlso kopia(idxLewo + 3) = 0, bC, CInt(kopia(idxLewo + 2)))

                Dim rRt = If(kanaly = 4 AndAlso kopia(idxPrawo + 3) = 0, rC, CInt(kopia(idxPrawo)))
                Dim gRt = If(kanaly = 4 AndAlso kopia(idxPrawo + 3) = 0, gC, CInt(kopia(idxPrawo + 1)))
                Dim bRt = If(kanaly = 4 AndAlso kopia(idxPrawo + 3) = 0, bC, CInt(kopia(idxPrawo + 2)))

                Dim rLap = 4 * rC - (rUp + rDn + rLf + rRt)
                Dim gLap = 4 * gC - (gUp + gDn + gLf + gRt)
                Dim bLap = 4 * bC - (bUp + bDn + bLf + bRt)

                bufor(idx) = CByte(Math.Max(0, Math.Min(255, Math.Round(rC + sila * rLap))))
                bufor(idx + 1) = CByte(Math.Max(0, Math.Min(255, Math.Round(gC + sila * gLap))))
                bufor(idx + 2) = CByte(Math.Max(0, Math.Min(255, Math.Round(bC + sila * bLap))))
            Next
        Next
    End Sub

    ''' <summary>
    ''' Wycina środek obrazu ARGB bez marginesu o podanej szerokości z każdej strony (kafel renderowany z marginesem,
    ''' żeby wyostrzanie na jego brzegu korzystało z prawdziwych sąsiadów). Zwraca Nothing, gdy wycinek jest całkiem
    ''' przezroczysty - kafel z danymi tylko w marginesie nie powinien trafić do pliku.
    ''' </summary>
    Public Shared Function Wytnij(piksele As Integer(), szerokosc As Integer, wysokosc As Integer, margines As Integer) As Integer()
        Dim szer As Integer = szerokosc - 2 * margines, wys As Integer = wysokosc - 2 * margines
        If piksele Is Nothing OrElse szer <= 0 OrElse wys <= 0 OrElse piksele.Length < szerokosc * wysokosc Then Return Nothing
        Dim wynik(szer * wys - 1) As Integer
        For y = 0 To wys - 1
            Array.Copy(piksele, (y + margines) * szerokosc + margines, wynik, y * szer, szer)
        Next
        For Each p In wynik
            If ((p >> 24) And &HFF) <> 0 Then Return wynik
        Next
        Return Nothing
    End Function

End Class

''' <summary>
''' Korekta obrazu zapisywanego kolejnymi pasami wierszy (GeoTIFF, scalanie segmentów). Każdy pas jest poprawiany
''' razem z ostatnim wierszem poprzedniego i pierwszym wierszem następnego pasa, więc wyostrzanie na styku pasów
''' działa tak samo jak w środku obrazu (dawniej na każdym styku zostawały dwa wiersze o mniejszej ostrości).
''' Poprawiony pas trafia do zapisu dopiero po nadejściu następnego albo w Zakoncz - kolejność wierszy się nie zmienia.
''' </summary>
Public NotInheritable Class KorekcjaPasow

    Private ReadOnly _szerokosc As Integer
    Private ReadOnly _kanaly As Integer
    Private ReadOnly _opcje As OpcjeKorekcjiMip
    Private ReadOnly _bajtyWiersza As Integer
    'pas czekający na pierwszy wiersz następnego pasa (surowe piksele)
    Private _oczekujacy As Byte()
    Private _wierszeOczekujace As Integer
    'ostatni surowy wiersz pasa już zapisanego - górny sąsiad następnego pasa
    Private ReadOnly _wierszNad As Byte()
    Private _jestWierszNad As Boolean
    Private _robocze As Byte()
    Private _wynik As Byte()

    Public Sub New(szerokosc As Integer, kanaly As Integer, opcje As OpcjeKorekcjiMip)
        If szerokosc <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(szerokosc))
        If kanaly < 3 OrElse kanaly > 4 Then Throw New ArgumentOutOfRangeException(NameOf(kanaly))
        _szerokosc = szerokosc
        _kanaly = kanaly
        _opcje = If(opcje, New OpcjeKorekcjiMip()).Kopia()
        _bajtyWiersza = szerokosc * kanaly
        _wierszNad = New Byte(_bajtyWiersza - 1) {}
    End Sub

    ''' <summary>Przyjmuje kolejny pas (surowe piksele RGB/RGBA) i przekazuje do zapisu poprzedni pas, już poprawiony.</summary>
    Public Sub Dodaj(bufor As Byte(), liczbaWierszy As Integer, zapis As Action(Of Byte(), Integer))
        If liczbaWierszy <= 0 Then Exit Sub
        If bufor Is Nothing OrElse bufor.Length < liczbaWierszy * _bajtyWiersza Then Throw New ArgumentException("Bufor jest krótszy niż pas wierszy", NameOf(bufor))
        If _wierszeOczekujace > 0 Then Przetworz(bufor, zapis)
        Dim rozmiar As Integer = liczbaWierszy * _bajtyWiersza
        If _oczekujacy Is Nothing OrElse _oczekujacy.Length < rozmiar Then _oczekujacy = New Byte(rozmiar - 1) {}
        Buffer.BlockCopy(bufor, 0, _oczekujacy, 0, rozmiar)
        _wierszeOczekujace = liczbaWierszy
    End Sub

    ''' <summary>Przekazuje do zapisu ostatni pas (bez wiersza poniżej - to dolna krawędź obrazu).</summary>
    Public Sub Zakoncz(zapis As Action(Of Byte(), Integer))
        If _wierszeOczekujace > 0 Then Przetworz(Nothing, zapis)
    End Sub

    Private Sub Przetworz(nastepny As Byte(), zapis As Action(Of Byte(), Integer))
        Dim n As Integer = _wierszeOczekujace
        Dim nad As Integer = If(_jestWierszNad, 1, 0)
        Dim pod As Integer = If(nastepny IsNot Nothing, 1, 0)
        Dim wiersze As Integer = nad + n + pod
        Dim rozmiar As Integer = wiersze * _bajtyWiersza
        If _robocze Is Nothing OrElse _robocze.Length < rozmiar Then _robocze = New Byte(rozmiar - 1) {}
        If nad = 1 Then Buffer.BlockCopy(_wierszNad, 0, _robocze, 0, _bajtyWiersza)
        Buffer.BlockCopy(_oczekujacy, 0, _robocze, nad * _bajtyWiersza, n * _bajtyWiersza)
        If pod = 1 Then Buffer.BlockCopy(nastepny, 0, _robocze, (nad + n) * _bajtyWiersza, _bajtyWiersza)

        Buffer.BlockCopy(_oczekujacy, (n - 1) * _bajtyWiersza, _wierszNad, 0, _bajtyWiersza)
        _jestWierszNad = True
        _wierszeOczekujace = 0

        KorekcjaMip.ZastosujDoBuforaRgb(_robocze, _szerokosc, wiersze, _kanaly, _opcje)

        Dim rozmiarWyniku As Integer = n * _bajtyWiersza
        If _wynik Is Nothing OrElse _wynik.Length < rozmiarWyniku Then _wynik = New Byte(rozmiarWyniku - 1) {}
        Buffer.BlockCopy(_robocze, nad * _bajtyWiersza, _wynik, 0, rozmiarWyniku)
        zapis(_wynik, n)
    End Sub

End Class
