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

Imports System.Globalization

''' <summary>
''' Prostokątny zasięg w układzie współrzędnych (X - północ, Y - wschód):
''' lewy dolny narożnik (XDol, YLewy), prawy górny (XGora, YPrawy).
''' </summary>
Public Structure Zasieg
    Public XDol As Double
    Public YLewy As Double
    Public XGora As Double
    Public YPrawy As Double

    Public Sub New(xDol As Double, yLewy As Double, xGora As Double, yPrawy As Double)
        Me.XDol = xDol
        Me.YLewy = yLewy
        Me.XGora = xGora
        Me.YPrawy = yPrawy
    End Sub

    Public ReadOnly Property Wysokosc As Double
        Get
            Return XGora - XDol
        End Get
    End Property

    Public ReadOnly Property Szerokosc As Double
        Get
            Return YPrawy - YLewy
        End Get
    End Property

    Public ReadOnly Property Srodek As PunktXY
        Get
            Return New PunktXY((XDol + XGora) / 2, (YLewy + YPrawy) / 2)
        End Get
    End Property

    Public ReadOnly Property LewyDolny As PunktXY
        Get
            Return New PunktXY(XDol, YLewy)
        End Get
    End Property

    Public ReadOnly Property PrawyDolny As PunktXY
        Get
            Return New PunktXY(XDol, YPrawy)
        End Get
    End Property

    Public ReadOnly Property PrawyGorny As PunktXY
        Get
            Return New PunktXY(XGora, YPrawy)
        End Get
    End Property

    Public ReadOnly Property LewyGorny As PunktXY
        Get
            Return New PunktXY(XGora, YLewy)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format(CultureInfo.InvariantCulture, "X {0}..{1}, Y {2}..{3}", XDol, XGora, YLewy, YPrawy)
    End Function
End Structure

''' <summary>Sposób numerowania plików segmentów (wartości jak w plikach conf.txt i lastsettings.txt).</summary>
Public Enum StylNumeracji
    ''' <summary>"NrWiersza_NrKolumny", np. _01_03</summary>
    WierszKolumna
    ''' <summary>"01_02_03", np. _07</summary>
    KolejnyDwucyfrowy
    ''' <summary>"1_2_3", np. _7</summary>
    Kolejny
End Enum

''' <summary>
''' Siatka kwadratowych segmentów pokrywająca wybrany obszar. Wiersze numerowane od góry (wiersz 1 - najbardziej
''' na północ), kolumny od lewej. Obszar jest powiększany w górę i w prawo do pełnej liczby segmentów -
''' tak jak dotychczas liczył to program.
''' </summary>
Public Class Siatka

    Public ReadOnly Property Obszar As Zasieg
    ''' <summary>Rozmiar piksela w jednostkach układu (m lub stopnie).</summary>
    Public ReadOnly Property RozmiarPiksela As Double
    ''' <summary>Bok segmentu w pikselach.</summary>
    Public ReadOnly Property BokSegmentuPx As Integer

    Public Sub New(obszar As Zasieg, rozmiarPiksela As Double, bokSegmentuPx As Integer)
        Me.Obszar = obszar
        Me.RozmiarPiksela = rozmiarPiksela
        Me.BokSegmentuPx = bokSegmentuPx
    End Sub

    ''' <summary>Czy parametry pozwalają wyznaczyć siatkę (dodatni zasięg, piksel i bok).</summary>
    Public ReadOnly Property Poprawna As Boolean
        Get
            Return RozmiarPiksela > 0 AndAlso BokSegmentuPx > 0 AndAlso Obszar.Szerokosc > 0 AndAlso Obszar.Wysokosc > 0 AndAlso
                   Not Double.IsNaN(RozmiarPiksela) AndAlso Not Double.IsInfinity(RozmiarPiksela)
        End Get
    End Property

    ''' <summary>Terenowy bok segmentu (w jednostkach układu).</summary>
    Public ReadOnly Property BokSegmentu As Double
        Get
            Return RozmiarPiksela * BokSegmentuPx
        End Get
    End Property

    ''' <summary>Liczba segmentów w poziomie (kolumn).</summary>
    Public ReadOnly Property LiczbaKolumn As Integer
        Get
            If Not Poprawna Then Return 0
            Return CInt(Math.Ceiling(Math.Round(Obszar.Szerokosc / BokSegmentu, 9)))
        End Get
    End Property

    ''' <summary>Liczba segmentów w pionie (wierszy).</summary>
    Public ReadOnly Property LiczbaWierszy As Integer
        Get
            If Not Poprawna Then Return 0
            Return CInt(Math.Ceiling(Math.Round(Obszar.Wysokosc / BokSegmentu, 9)))
        End Get
    End Property

    Public ReadOnly Property LiczbaSegmentow As Integer
        Get
            Return LiczbaKolumn * LiczbaWierszy
        End Get
    End Property

    ''' <summary>Szerokość całej siatki w pikselach.</summary>
    Public ReadOnly Property SzerokoscPx As Long
        Get
            Return CLng(LiczbaKolumn) * BokSegmentuPx
        End Get
    End Property

    ''' <summary>Wysokość całej siatki w pikselach.</summary>
    Public ReadOnly Property WysokoscPx As Long
        Get
            Return CLng(LiczbaWierszy) * BokSegmentuPx
        End Get
    End Property

    ''' <summary>Zasięg całej siatki (obszar powiększony do pełnych segmentów).</summary>
    Public ReadOnly Property ZasiegSiatki As Zasieg
        Get
            Return New Zasieg(Obszar.XDol, Obszar.YLewy, Obszar.XDol + LiczbaWierszy * BokSegmentu, Obszar.YLewy + LiczbaKolumn * BokSegmentu)
        End Get
    End Property

    ''' <summary>Zasięg segmentu; wiersz 1 - górny, kolumna 1 - lewa.</summary>
    Public Function Segment(wiersz As Integer, kolumna As Integer) As Zasieg
        Dim xDol As Double = Obszar.XDol + BokSegmentu * (LiczbaWierszy - wiersz)
        Dim yLewy As Double = Obszar.YLewy + BokSegmentu * (kolumna - 1)
        Return New Zasieg(xDol, yLewy, xDol + BokSegmentu, yLewy + BokSegmentu)
    End Function

    ''' <summary>Kolejny numer segmentu (od 1, wierszami od góry).</summary>
    Public Function NumerKolejny(wiersz As Integer, kolumna As Integer) As Integer
        Return (wiersz - 1) * LiczbaKolumn + kolumna
    End Function

    ''' <summary>Nazwa pliku segmentu (bez rozszerzenia) wg stylu numeracji.</summary>
    Public Function NazwaSegmentu(prefiks As String, styl As StylNumeracji, wiersz As Integer, kolumna As Integer) As String
        Dim ci = CultureInfo.InvariantCulture
        Select Case styl
            Case StylNumeracji.KolejnyDwucyfrowy
                Return prefiks & NumerKolejny(wiersz, kolumna).ToString("D2", ci)
            Case StylNumeracji.Kolejny
                Return prefiks & NumerKolejny(wiersz, kolumna).ToString(ci)
            Case Else
                Return prefiks & wiersz.ToString("D2", ci) & "_" & kolumna.ToString("D2", ci)
        End Select
    End Function

    ''' <summary>Nazwa segmentu mapy TrekBuddy: przesunięcie lewego górnego rogu w pikselach (_x_y).</summary>
    Public Function NazwaSegmentuTrekBuddy(prefiks As String, wiersz As Integer, kolumna As Integer) As String
        Dim ci = CultureInfo.InvariantCulture
        Return prefiks & "_" & ((kolumna - 1) * BokSegmentuPx).ToString(ci) & "_" & ((wiersz - 1) * BokSegmentuPx).ToString(ci)
    End Function

    ''' <summary>Styl numeracji z tekstu zapisanego w plikach ustawień.</summary>
    Public Shared Function StylZTekstu(tekst As String) As StylNumeracji
        Select Case tekst
            Case "01_02_03" : Return StylNumeracji.KolejnyDwucyfrowy
            Case "1_2_3" : Return StylNumeracji.Kolejny
            Case Else : Return StylNumeracji.WierszKolumna
        End Select
    End Function

    ''' <summary>Tekst stylu numeracji zapisywany w plikach ustawień.</summary>
    Public Shared Function TekstStylu(styl As StylNumeracji) As String
        Select Case styl
            Case StylNumeracji.KolejnyDwucyfrowy : Return "01_02_03"
            Case StylNumeracji.Kolejny : Return "1_2_3"
            Case Else : Return "NrWiersza_NrKolumny"
        End Select
    End Function

End Class
