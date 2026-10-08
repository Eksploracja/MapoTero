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

Imports System.Collections.Generic
Imports System.Globalization

''' <summary>
''' Punkt w układzie współrzędnych. Konwencja programu (geodezyjna):
''' X - współrzędna "północna" (northing, a w WGS84 szerokość geograficzna),
''' Y - współrzędna "wschodnia" (easting, a w WGS84 długość geograficzna).
''' </summary>
Public Structure PunktXY
    Public X As Double
    Public Y As Double

    Public Sub New(x As Double, y As Double)
        Me.X = x
        Me.Y = y
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format(CultureInfo.InvariantCulture, "({0}; {1})", X, Y)
    End Function
End Structure

''' <summary>Punkt w układzie WGS84 (stopnie).</summary>
Public Structure PunktGeo
    Public Szerokosc As Double
    Public Dlugosc As Double

    Public Sub New(szerokosc As Double, dlugosc As Double)
        Me.Szerokosc = szerokosc
        Me.Dlugosc = dlugosc
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format(CultureInfo.InvariantCulture, "({0}; {1})", Szerokosc, Dlugosc)
    End Function
End Structure

''' <summary>
''' Układ współrzędnych obsługiwany przez program: odwzorowanie Gaussa-Krügera (PL-1992, PL-2000, UTM)
''' na elipsoidzie GRS80 albo współrzędne geograficzne WGS84.
''' Przeliczenia wykorzystują szereg Krügera (6. rząd), dokładny do ułamków milimetra w całym zakresie
''' stosowania tych układów - zastępuje dwie wcześniejsze, różne implementacje przeliczeń w programie.
''' Różnicę między ETRF89 (PL-1992/2000) a WGS84 pomija się, tak jak robi to PROJ dla tych układów.
''' </summary>
Public NotInheritable Class UkladWspolrzednych

    'elipsoida GRS80 (WGS84 różni się od niej spłaszczeniem o mniej niż 0,1 mm na współrzędnych)
    Public Const PolosWielka As Double = 6378137.0
    Public Const Splaszczenie As Double = 1 / 298.257222101

    ''' <summary>Kod EPSG układu, np. 2180.</summary>
    Public ReadOnly Property Epsg As Integer
    ''' <summary>Nazwa wyświetlana użytkownikowi.</summary>
    Public ReadOnly Property Nazwa As String
    ''' <summary>True dla WGS84 (współrzędne w stopniach), False dla odwzorowań (metry).</summary>
    Public ReadOnly Property Geograficzny As Boolean
    ''' <summary>Południk osiowy odwzorowania w stopniach.</summary>
    Public ReadOnly Property PoludnikOsiowy As Double
    ''' <summary>Współczynnik skali na południku osiowym.</summary>
    Public ReadOnly Property Skala As Double
    ''' <summary>Przesunięcie współrzędnej wschodniej (false easting).</summary>
    Public ReadOnly Property PrzesuniecieY As Double
    ''' <summary>Przesunięcie współrzędnej północnej (false northing).</summary>
    Public ReadOnly Property PrzesuniecieX As Double
    ''' <summary>
    ''' Kolejność osi zgodna z definicją EPSG, obowiązująca w zapytaniach WMS 1.3.0:
    ''' True - najpierw X (północ / szerokość), np. EPSG:2180, 2176-2179, 4326;
    ''' False - najpierw Y (wschód), np. UTM EPSG:32633-32635.
    ''' </summary>
    Public ReadOnly Property NajpierwPolnoc As Boolean

    Private Sub New(epsg As Integer, nazwa As String, geograficzny As Boolean, poludnikOsiowy As Double, skala As Double,
                    przesuniecieY As Double, przesuniecieX As Double, najpierwPolnoc As Boolean)
        Me.Epsg = epsg
        Me.Nazwa = nazwa
        Me.Geograficzny = geograficzny
        Me.PoludnikOsiowy = poludnikOsiowy
        Me.Skala = skala
        Me.PrzesuniecieY = przesuniecieY
        Me.PrzesuniecieX = przesuniecieX
        Me.NajpierwPolnoc = najpierwPolnoc
    End Sub

    Public Overrides Function ToString() As String
        Return Nazwa
    End Function

    ''' <summary>Oznaczenie układu w zapytaniu WMS, np. "EPSG:2180".</summary>
    Public ReadOnly Property KodCrs As String
        Get
            Return "EPSG:" & Epsg.ToString(CultureInfo.InvariantCulture)
        End Get
    End Property

#Region "Obsługiwane układy"

    Public Shared ReadOnly PL1992 As New UkladWspolrzednych(2180, "PL-1992 (EPSG:2180)", False, 19, 0.9993, 500000, -5300000, True)
    Public Shared ReadOnly PL2000Strefa5 As New UkladWspolrzednych(2176, "PL-2000 strefa 5 (EPSG:2176)", False, 15, 0.999923, 5500000, 0, True)
    Public Shared ReadOnly PL2000Strefa6 As New UkladWspolrzednych(2177, "PL-2000 strefa 6 (EPSG:2177)", False, 18, 0.999923, 6500000, 0, True)
    Public Shared ReadOnly PL2000Strefa7 As New UkladWspolrzednych(2178, "PL-2000 strefa 7 (EPSG:2178)", False, 21, 0.999923, 7500000, 0, True)
    Public Shared ReadOnly PL2000Strefa8 As New UkladWspolrzednych(2179, "PL-2000 strefa 8 (EPSG:2179)", False, 24, 0.999923, 8500000, 0, True)
    Public Shared ReadOnly Utm33N As New UkladWspolrzednych(32633, "UTM strefa 33N (EPSG:32633)", False, 15, 0.9996, 500000, 0, False)
    Public Shared ReadOnly Utm34N As New UkladWspolrzednych(32634, "UTM strefa 34N (EPSG:32634)", False, 21, 0.9996, 500000, 0, False)
    Public Shared ReadOnly Utm35N As New UkladWspolrzednych(32635, "UTM strefa 35N (EPSG:32635)", False, 27, 0.9996, 500000, 0, False)
    Public Shared ReadOnly Wgs84 As New UkladWspolrzednych(4326, "WGS84 - współrzędne geograficzne (EPSG:4326)", True, 0, 1, 0, 0, True)

    ''' <summary>Wszystkie układy obsługiwane przez program (kolejność wyświetlania).</summary>
    Public Shared ReadOnly Property Wszystkie As IList(Of UkladWspolrzednych)
        Get
            Return New UkladWspolrzednych() {PL1992, PL2000Strefa5, PL2000Strefa6, PL2000Strefa7, PL2000Strefa8,
                                             Utm33N, Utm34N, Utm35N, Wgs84}
        End Get
    End Property

    ''' <summary>Układ o podanym kodzie EPSG; gdy kod nieznany - PL-1992.</summary>
    Public Shared Function ZKodu(epsg As Integer) As UkladWspolrzednych
        For Each u In Wszystkie
            If u.Epsg = epsg Then Return u
        Next
        Return PL1992
    End Function

#End Region

#Region "Szereg Krügera"

    Private Shared ReadOnly N As Double = Splaszczenie / (2 - Splaszczenie)
    Private Shared ReadOnly E As Double = Math.Sqrt(Splaszczenie * (2 - Splaszczenie))
    'promień prostujący (rectifying radius)
    Private Shared ReadOnly PromienA As Double = PolosWielka / (1 + N) * (1 + N ^ 2 / 4 + N ^ 4 / 64 + N ^ 6 / 256)

    Private Shared ReadOnly Alfa As Double() = {
        N / 2 - 2 / 3.0 * N ^ 2 + 5 / 16.0 * N ^ 3 + 41 / 180.0 * N ^ 4 - 127 / 288.0 * N ^ 5 + 7891 / 37800.0 * N ^ 6,
        13 / 48.0 * N ^ 2 - 3 / 5.0 * N ^ 3 + 557 / 1440.0 * N ^ 4 + 281 / 630.0 * N ^ 5 - 1983433 / 1935360.0 * N ^ 6,
        61 / 240.0 * N ^ 3 - 103 / 140.0 * N ^ 4 + 15061 / 26880.0 * N ^ 5 + 167603 / 181440.0 * N ^ 6,
        49561 / 161280.0 * N ^ 4 - 179 / 168.0 * N ^ 5 + 6601661 / 7257600.0 * N ^ 6,
        34729 / 80640.0 * N ^ 5 - 3418889 / 1995840.0 * N ^ 6,
        212378941 / 319334400.0 * N ^ 6}

    Private Shared ReadOnly Beta As Double() = {
        N / 2 - 2 / 3.0 * N ^ 2 + 37 / 96.0 * N ^ 3 - 1 / 360.0 * N ^ 4 - 81 / 512.0 * N ^ 5 + 96199 / 604800.0 * N ^ 6,
        1 / 48.0 * N ^ 2 + 1 / 15.0 * N ^ 3 - 437 / 1440.0 * N ^ 4 + 46 / 105.0 * N ^ 5 - 1118711 / 3870720.0 * N ^ 6,
        17 / 480.0 * N ^ 3 - 37 / 840.0 * N ^ 4 - 209 / 4480.0 * N ^ 5 + 5569 / 90720.0 * N ^ 6,
        4397 / 161280.0 * N ^ 4 - 11 / 504.0 * N ^ 5 - 830251 / 7257600.0 * N ^ 6,
        4583 / 161280.0 * N ^ 5 - 108847 / 3991680.0 * N ^ 6,
        20648693 / 638668800.0 * N ^ 6}

    Private Shared Function Atanh(x As Double) As Double
        Return 0.5 * Math.Log((1 + x) / (1 - x))
    End Function

    Private Shared Function Asinh(x As Double) As Double
        Return Math.Log(x + Math.Sqrt(x * x + 1))
    End Function

    ''' <summary>Tangens szerokości konforemnej dla tangensa szerokości geodezyjnej.</summary>
    Private Shared Function TanKonforemna(tanFi As Double) As Double
        Dim sinFi As Double = tanFi / Math.Sqrt(1 + tanFi * tanFi)
        Return Math.Sinh(Asinh(tanFi) - E * Atanh(E * sinFi))
    End Function

    ''' <summary>Odwrotność TanKonforemna - iteracja Newtona (zbieżna po 2-3 krokach).</summary>
    Private Shared Function TanGeodezyjna(tanChi As Double) As Double
        Dim tanFi As Double = tanChi
        For i = 1 To 10
            Dim tc As Double = TanKonforemna(tanFi)
            Dim pochodna As Double = (1 - E * E) * Math.Sqrt(1 + tc * tc) * Math.Sqrt(1 + tanFi * tanFi) /
                                     (1 + (1 - E * E) * tanFi * tanFi)
            Dim krok As Double = (tanChi - tc) / pochodna
            tanFi += krok
            If Math.Abs(krok) < 0.000000000001 * Math.Max(1, Math.Abs(tanFi)) Then Exit For
        Next
        Return tanFi
    End Function

#End Region

#Region "Przeliczenia"

    ''' <summary>Przelicza punkt WGS84 na współrzędne tego układu.</summary>
    Public Function ZWgs84(punkt As PunktGeo) As PunktXY
        If Geograficzny Then Return New PunktXY(punkt.Szerokosc, punkt.Dlugosc)

        Dim fi As Double = punkt.Szerokosc * Math.PI / 180
        Dim dl As Double = (punkt.Dlugosc - PoludnikOsiowy) * Math.PI / 180
        Dim tanChi As Double = TanKonforemna(Math.Tan(fi))
        Dim ksiP As Double = Math.Atan2(tanChi, Math.Cos(dl))
        Dim etaP As Double = Asinh(Math.Sin(dl) / Math.Sqrt(tanChi * tanChi + Math.Cos(dl) ^ 2))

        Dim ksi As Double = ksiP
        Dim eta As Double = etaP
        For j = 1 To 6
            ksi += Alfa(j - 1) * Math.Sin(2 * j * ksiP) * Math.Cosh(2 * j * etaP)
            eta += Alfa(j - 1) * Math.Cos(2 * j * ksiP) * Math.Sinh(2 * j * etaP)
        Next

        Return New PunktXY(PrzesuniecieX + Skala * PromienA * ksi, PrzesuniecieY + Skala * PromienA * eta)
    End Function

    ''' <summary>Przelicza punkt tego układu na WGS84.</summary>
    Public Function DoWgs84(punkt As PunktXY) As PunktGeo
        If Geograficzny Then Return New PunktGeo(punkt.X, punkt.Y)

        Dim ksi As Double = (punkt.X - PrzesuniecieX) / (Skala * PromienA)
        Dim eta As Double = (punkt.Y - PrzesuniecieY) / (Skala * PromienA)
        Dim ksiP As Double = ksi
        Dim etaP As Double = eta
        For j = 1 To 6
            ksiP -= Beta(j - 1) * Math.Sin(2 * j * ksi) * Math.Cosh(2 * j * eta)
            etaP -= Beta(j - 1) * Math.Cos(2 * j * ksi) * Math.Sinh(2 * j * eta)
        Next

        Dim tanChi As Double = Math.Sin(ksiP) / Math.Sqrt(Math.Sinh(etaP) ^ 2 + Math.Cos(ksiP) ^ 2)
        Dim dl As Double = Math.Atan2(Math.Sinh(etaP), Math.Cos(ksiP))
        Dim fi As Double = Math.Atan(TanGeodezyjna(tanChi))

        Return New PunktGeo(fi * 180 / Math.PI, PoludnikOsiowy + dl * 180 / Math.PI)
    End Function

    Public Function DoWgs84(x As Double, y As Double) As PunktGeo
        Return DoWgs84(New PunktXY(x, y))
    End Function

    Public Function ZWgs84(szerokosc As Double, dlugosc As Double) As PunktXY
        Return ZWgs84(New PunktGeo(szerokosc, dlugosc))
    End Function

    ''' <summary>Przelicza punkt z tego układu do innego (przez WGS84).</summary>
    Public Function PrzeliczDo(cel As UkladWspolrzednych, punkt As PunktXY) As PunktXY
        If cel Is Me Then Return punkt
        Return cel.ZWgs84(DoWgs84(punkt))
    End Function

    ''' <summary>
    ''' Zasięg w innym układzie: prostokąt obejmujący wszystkie narożniki (prostokąt z jednego układu jest w innym
    ''' lekko obróconym czworokątem). W układach metrycznych zaokrąglany na zewnątrz do pełnych metrów.
    ''' </summary>
    Public Function PrzeliczZasieg(cel As UkladWspolrzednych, z As Zasieg) As Zasieg
        If cel Is Me Then Return z
        Dim xMin As Double = Double.MaxValue, xMax As Double = Double.MinValue
        Dim yMin As Double = Double.MaxValue, yMax As Double = Double.MinValue
        For Each p In New PunktXY() {z.LewyDolny, z.PrawyDolny, z.PrawyGorny, z.LewyGorny}
            Dim q = PrzeliczDo(cel, p)
            xMin = Math.Min(xMin, q.X) : xMax = Math.Max(xMax, q.X)
            yMin = Math.Min(yMin, q.Y) : yMax = Math.Max(yMax, q.Y)
        Next
        If cel.Geograficzny Then
            Return New Zasieg(Math.Round(xMin, 7), Math.Round(yMin, 7), Math.Round(xMax, 7), Math.Round(yMax, 7))
        End If
        Return New Zasieg(Math.Floor(xMin), Math.Floor(yMin), Math.Ceiling(xMax), Math.Ceiling(yMax))
    End Function

    ''' <summary>
    ''' Rozmiar piksela w innym układzie (metry - stopnie) w pobliżu podanego punktu WGS84. Dla WGS84 przyjmowana jest
    ''' rozdzielczość odpowiadająca kierunkowi północ-południe. Między układami metrycznymi - bez zmian.
    ''' </summary>
    Public Function PrzeliczRozmiarPiksela(cel As UkladWspolrzednych, piksel As Double, miejsce As PunktGeo) As Double
        If cel.Geograficzny = Geograficzny Then Return piksel
        Dim mSz As Double, mDl As Double
        Georeferencja.MetrowNaStopien(miejsce.Szerokosc, mSz, mDl)
        If mSz <= 0 Then Return piksel
        If cel.Geograficzny Then Return Math.Round(piksel / mSz, 9)
        Return Math.Round(piksel * mSz, 3)
    End Function

#End Region

#Region "Opisy układu dla plików georeferencyjnych"

    ''' <summary>Definicja układu w formacie WKT (ESRI) - zawartość pliku .prj.</summary>
    Public ReadOnly Property Wkt As String
        Get
            Dim ci = CultureInfo.InvariantCulture
            Const geogGrs80 As String = "GEOGCS[""GCS_ETRS_1989"",DATUM[""D_ETRS_1989"",SPHEROID[""GRS_1980"",6378137.0,298.257222101]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]]"
            Const geogWgs84 As String = "GEOGCS[""GCS_WGS_1984"",DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137.0,298.257223563]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]]"
            If Geograficzny Then Return geogWgs84
            Dim nazwaWkt As String
            Select Case Epsg
                Case 2180 : nazwaWkt = "ETRS_1989_Poland_CS92"
                Case 2176 To 2179 : nazwaWkt = "ETRS_1989_Poland_CS2000_Zone_" & (Epsg - 2171).ToString(ci)
                Case Else : nazwaWkt = "WGS_1984_UTM_Zone_" & (Epsg - 32600).ToString(ci) & "N"
            End Select
            Dim geog As String = If(Epsg >= 32600, geogWgs84, geogGrs80)
            Return String.Format(ci,
                "PROJCS[""{0}"",{1},PROJECTION[""Transverse_Mercator""],PARAMETER[""False_Easting"",{2}],PARAMETER[""False_Northing"",{3}],PARAMETER[""Central_Meridian"",{4}],PARAMETER[""Scale_Factor"",{5}],PARAMETER[""Latitude_Of_Origin"",0.0],UNIT[""Meter"",1.0]]",
                nazwaWkt, geog, PrzesuniecieY.ToString("0.0", ci), PrzesuniecieX.ToString("0.0", ci),
                PoludnikOsiowy.ToString("0.0", ci), Skala.ToString("0.0#######", ci))
        End Get
    End Property

    ''' <summary>Opis układu dla MapInfo (.tab).</summary>
    Public ReadOnly Property CoordSysMapInfo As String
        Get
            Dim ci = CultureInfo.InvariantCulture
            If Geograficzny Then Return "CoordSys Earth Projection 1, 104"
            'Projection 8 - Transverse Mercator; datum 33 - GRS80 (ETRF89), 104 - WGS84; jednostka 7 - metry
            Dim datum As String = If(Epsg >= 32600, "104", "33")
            Return "CoordSys Earth Projection 8, " & datum & ", 7, " &
                   PoludnikOsiowy.ToString("0.########", ci) & ", 0, " & Skala.ToString("0.########", ci) & ", " &
                   PrzesuniecieY.ToString("0.##", ci) & ", " & PrzesuniecieX.ToString("0.##", ci)
        End Get
    End Property

#End Region

End Class
