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
Imports System.IO
Imports System.Text

''' <summary>
''' Obraz rastrowy (segment lub scalony arkusz) o znanym położeniu: prostokąt w układzie współrzędnych
''' odpowiadający całemu obrazowi (zewnętrzne krawędzie skrajnych pikseli).
''' </summary>
Public Class ObrazGeoreferencyjny

    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Public Property Zasieg As Zasieg
    Public Property SzerokoscPx As Long
    Public Property WysokoscPx As Long
    ''' <summary>Nazwa pliku obrazu z rozszerzeniem, bez ścieżki (np. _01_02.jpg).</summary>
    Public Property NazwaPliku As String = ""
    ''' <summary>Pełna ścieżka pliku obrazu (używana w pliku .map OziExplorera).</summary>
    Public Property SciezkaPliku As String = ""

    ''' <summary>Rozmiar piksela w poziomie (jednostki układu).</summary>
    Public ReadOnly Property PikselY As Double
        Get
            Return Zasieg.Szerokosc / SzerokoscPx
        End Get
    End Property

    ''' <summary>Rozmiar piksela w pionie (jednostki układu).</summary>
    Public ReadOnly Property PikselX As Double
        Get
            Return Zasieg.Wysokosc / WysokoscPx
        End Get
    End Property

    ''' <summary>Narożniki obrazu w WGS84: lewy górny, prawy górny, prawy dolny, lewy dolny.</summary>
    Public Function NaroznikiWgs84() As PunktGeo()
        Return New PunktGeo() {Uklad.DoWgs84(Zasieg.LewyGorny), Uklad.DoWgs84(Zasieg.PrawyGorny),
                               Uklad.DoWgs84(Zasieg.PrawyDolny), Uklad.DoWgs84(Zasieg.LewyDolny)}
    End Function

End Class

''' <summary>
''' Treść plików georeferencyjnych (kalibracyjnych) obrazu - jedna implementacja dla segmentów,
''' scalonych arkuszy i map TrekBuddy (dawniej każdy format miał w programie po kilka kopii kodu).
''' </summary>
Public NotInheritable Class Georeferencja

    Private Sub New()
    End Sub

    Private Shared ReadOnly Ci As CultureInfo = CultureInfo.InvariantCulture
    Private Shared ReadOnly Nl As String = (ChrW(13) & ChrW(10))

    ''' <summary>Liczba z kropką dziesiętną, bez zbędnych zer (niezależnie od ustawień regionalnych).</summary>
    Public Shared Function Liczba(wartosc As Double) As String
        Return wartosc.ToString("0.##########", Ci)
    End Function

    ''' <summary>Rozszerzenie pliku world file dla rozszerzenia obrazu: jpg - jpgw, png - pngw, tif - tifw, gif - gifw.</summary>
    Public Shared Function RozszerzenieWorldFile(rozszerzenieObrazu As String) As String
        Dim r As String = rozszerzenieObrazu.TrimStart("."c).ToLowerInvariant()
        Select Case r
            Case "jpg", "jpeg" : Return "jpgw"
            Case "tif", "tiff" : Return "tifw"
            Case Else : Return r & "w"
        End Select
    End Function

    ''' <summary>
    ''' World file (.jpgw, .pngw, ...) dla QGIS, ArcGIS i innych GIS. Zgodnie ze standardem podaje współrzędne
    ''' ŚRODKA lewego górnego piksela (wschód, północ) - dawniej zapisywany był narożnik (przesunięcie o pół piksela).
    ''' </summary>
    Public Shared Function WorldFile(o As ObrazGeoreferencyjny) As String
        Return Liczba(o.PikselY) & Nl &
               "0" & Nl &
               "0" & Nl &
               "-" & Liczba(o.PikselX) & Nl &
               Liczba(o.Zasieg.YLewy + o.PikselY / 2) & Nl &
               Liczba(o.Zasieg.XGora - o.PikselX / 2)
    End Function

    ''' <summary>Plik .prj z definicją układu (towarzyszy world file i GeoTIFF).</summary>
    Public Shared Function Prj(o As ObrazGeoreferencyjny) As String
        Return o.Uklad.Wkt
    End Function

    ''' <summary>Plik .tab programu MapInfo (cztery punkty kontrolne w narożnikach obrazu).</summary>
    Public Shared Function Tab(o As ObrazGeoreferencyjny) As String
        Dim z = o.Zasieg
        Dim w As String = o.SzerokoscPx.ToString(Ci)
        Dim h As String = o.WysokoscPx.ToString(Ci)
        Return "!table" & Nl &
               "!version 300" & Nl &
               "!charset WindowsLatin2" & Nl &
               "Definition Table" & Nl &
               "  File """ & o.NazwaPliku & """" & Nl &
               "  Type ""RASTER""" & Nl &
               "  (" & Liczba(z.YLewy) & "," & Liczba(z.XGora) & ") (0,0) Label ""Punkt 1""," & Nl &
               "  (" & Liczba(z.YPrawy) & "," & Liczba(z.XGora) & ") (" & w & ",0) Label ""Punkt 2""," & Nl &
               "  (" & Liczba(z.YPrawy) & "," & Liczba(z.XDol) & ") (" & w & "," & h & ") Label ""Punkt 3""," & Nl &
               "  (" & Liczba(z.YLewy) & "," & Liczba(z.XDol) & ") (0," & h & ") Label ""Punkt 4""" & Nl &
               "  " & o.Uklad.CoordSysMapInfo & Nl
    End Function

    ''' <summary>Stopnie i minuty dziesiętne w formacie OziExplorera, np. "52, 21.123456".</summary>
    Public Shared Function StopnieMinuty(wartosc As Double) As String
        Dim w As Double = Math.Round(wartosc, 8)
        Dim st As Double = Math.Floor(w)
        Dim min As Double = Math.Round((w - st) * 60, 6)
        If min >= 60.0 Then
            st += 1
            min = 0
        End If
        Return st.ToString("0", Ci) & ", " & min.ToString("0.######", Ci)
    End Function

    ''' <summary>Plik .map programu OziExplorer (także TrekBuddy).</summary>
    Public Shared Function MapOzi(o As ObrazGeoreferencyjny) As String
        Dim n = o.NaroznikiWgs84()    'LG, PG, PD, LD
        Dim w As String = o.SzerokoscPx.ToString(Ci)
        Dim h As String = o.WysokoscPx.ToString(Ci)
        Dim px() As String = {"    0,    0", " " & w & ",    0", " " & w & ", " & h, "    0, " & h}

        Dim sb As New StringBuilder()
        sb.Append("OziExplorer Map Data File Version 2.2").Append(Nl)
        sb.Append(o.NazwaPliku).Append(Nl)
        sb.Append(o.SciezkaPliku).Append(Nl)
        sb.Append("1 ,Map Code,").Append(Nl)
        sb.Append("WGS 84,,   0.0000,   0.0000,WGS 84").Append(Nl)
        sb.Append("Reserved 1").Append(Nl)
        sb.Append("Reserved 2").Append(Nl)
        sb.Append("Magnetic Variation,,,E").Append(Nl)
        If o.Uklad.Geograficzny Then
            sb.Append("Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No").Append(Nl)
        Else
            sb.Append("Map Projection,Transverse Mercator,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No").Append(Nl)
        End If
        For i = 1 To 30
            Dim nr As String = "Point" & i.ToString("00", Ci)
            If i <= 4 Then
                sb.Append(nr).Append(",xy,").Append(px(i - 1)).Append(",in, deg,  ").Append(StopnieMinuty(n(i - 1).Szerokosc)).
                   Append(",N,  ").Append(StopnieMinuty(n(i - 1).Dlugosc)).Append(",E, grid,   ,           ,           ,N").Append(Nl)
            Else
                sb.Append(nr).Append(",xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N").Append(Nl)
            End If
        Next
        If o.Uklad.Geograficzny Then
            sb.Append("Projection Setup,,,,,,,,,,").Append(Nl)
        Else
            sb.Append("Projection Setup,").
               Append(String.Format(Ci, "{0,16:0.000000000},{1,16:0.000000000},{2,16:0.000000000},{3,16:0.00},{4,16:0.00}",
                                    0.0, o.Uklad.PoludnikOsiowy, o.Uklad.Skala, o.Uklad.PrzesuniecieY, o.Uklad.PrzesuniecieX)).
               Append(",,,,,").Append(Nl)
        End If
        sb.Append("Map Feature = MF ; Map Comment = MC     These follow if they exist").Append(Nl)
        sb.Append("Track File = TF      These follow if they exist").Append(Nl)
        sb.Append("Moving Map Parameters = MM?    These follow if they exist").Append(Nl)
        sb.Append("MM0,Yes").Append(Nl)
        sb.Append("MMPNUM,4").Append(Nl)
        sb.Append("MMPXY,1,0,0").Append(Nl)
        sb.Append("MMPXY,2,").Append(w).Append(",0").Append(Nl)
        sb.Append("MMPXY,3,").Append(w).Append(",").Append(h).Append(Nl)
        sb.Append("MMPXY,4,0,").Append(h).Append(Nl)
        For i = 1 To 4
            sb.Append("MMPLL,").Append(i.ToString(Ci)).Append(",  ").Append(Math.Round(n(i - 1).Dlugosc, 8).ToString("0.########", Ci)).
               Append(",  ").Append(Math.Round(n(i - 1).Szerokosc, 8).ToString("0.########", Ci)).Append(Nl)
        Next
        sb.Append("MM1B,     ").Append(Liczba(Math.Round(MetrowNaPiksel(o), 6))).Append(Nl)
        sb.Append("MOP,Map Open Position,0,0").Append(Nl)
        sb.Append("IWH,Map Image Width/Height,").Append(w).Append(",").Append(h)
        Return sb.ToString()
    End Function

    ''' <summary>Rozmiar piksela w metrach w terenie (dla układów geograficznych - w środku obrazu).</summary>
    Public Shared Function MetrowNaPiksel(o As ObrazGeoreferencyjny) As Double
        If Not o.Uklad.Geograficzny Then Return o.PikselY
        Dim n = o.NaroznikiWgs84()
        Return OdlegloscMetry(n(3), n(2)) / o.SzerokoscPx
    End Function

    ''' <summary>Plik .gmi (Map Calibration data file v3.0).</summary>
    Public Shared Function Gmi(o As ObrazGeoreferencyjny) As String
        Dim n = o.NaroznikiWgs84()    'LG, PG, PD, LD
        Dim w As String = o.SzerokoscPx.ToString(Ci)
        Dim h As String = o.WysokoscPx.ToString(Ci)
        Dim xy() As String = {"0;0;", w & ";0;", w & ";" & h & ";", "0;" & h & ";"}
        Dim sb As New StringBuilder()
        sb.Append("Map Calibration data file v3.0").Append(Nl)
        sb.Append(o.NazwaPliku).Append(Nl)
        sb.Append(w).Append(Nl)
        sb.Append(h)
        For i = 0 To 3
            sb.Append(Nl).Append(xy(i)).Append(Math.Round(n(i).Dlugosc, 13).ToString("0.#############", Ci)).
               Append(";").Append(Math.Round(n(i).Szerokosc, 13).ToString("0.#############", Ci))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Plik .wld towarzyszący plikowi .points (format dotychczas tworzony przez program).</summary>
    Public Shared Function Wld(o As ObrazGeoreferencyjny) As String
        Dim lg = o.NaroznikiWgs84()(0)
        Return "0.000000000000000" & Nl &
               "0" & Nl &
               "0" & Nl &
               "-0.000000000000000" & Nl &
               lg.Dlugosc.ToString("0.000000000000000", Ci) & Nl &
               lg.Szerokosc.ToString("0.000000000000000", Ci)
    End Function

    ''' <summary>Plik punktów dopasowania georeferencera QGIS (.points): narożniki obrazu w WGS84.</summary>
    Public Shared Function Points(o As ObrazGeoreferencyjny) As String
        Dim n = o.NaroznikiWgs84()    'LG, PG, PD, LD
        Const f As String = "0.000000000000000"
        Dim w As String = o.SzerokoscPx.ToString(f, Ci)
        Dim h As String = o.WysokoscPx.ToString(f, Ci)
        Dim px() As String = {"0.000000000000000" & ChrW(9) & "0.000000000000000",
                              w & ChrW(9) & "0.000000000000000",
                              w & ChrW(9) & "-" & h,
                              "0.000000000000000" & ChrW(9) & "-" & h}
        Dim sb As New StringBuilder()
        sb.Append("mapX mapY pixelX pixelY").Append(Nl)
        For i = 0 To 3
            sb.Append(n(i).Dlugosc.ToString(f, Ci)).Append(ChrW(9)).Append(n(i).Szerokosc.ToString(f, Ci)).
               Append(ChrW(9)).Append(px(i)).Append(Nl)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Plik KML (GroundOverlay). Prostokąt z układu kartograficznego jest w WGS84 obrócony o zbieżność południków
    ''' (do ok. 4° na krańcach Polski), dlatego zapisywane są:
    '''  - gx:LatLonQuad - dokładne położenie czterech narożników obrazu,
    '''  - LatLonBox z obrotem (rotation) - przybliżenie dla programów, które nie obsługują gx:LatLonQuad.
    ''' </summary>
    Public Shared Function Kml(o As ObrazGeoreferencyjny, nazwa As String) As String
        Dim n = o.NaroznikiWgs84()    'LG, PG, PD, LD
        Dim lg = n(0), pg = n(1), pd = n(2), ld = n(3)
        Dim sr = o.Uklad.DoWgs84(o.Zasieg.Srodek)

        Dim mSz As Double, mDl As Double
        MetrowNaStopien(sr.Szerokosc, mSz, mDl)

        'azymut "północy" obrazu (lewego i prawego boku) względem północy geograficznej
        Dim azLewy As Double = Math.Atan2((lg.Dlugosc - ld.Dlugosc) * mDl, (lg.Szerokosc - ld.Szerokosc) * mSz)
        Dim azPrawy As Double = Math.Atan2((pg.Dlugosc - pd.Dlugosc) * mDl, (pg.Szerokosc - pd.Szerokosc) * mSz)
        'KML: obrót w stopniach przeciwnie do ruchu wskazówek zegara
        Dim obrot As Double = -(azLewy + azPrawy) / 2 * 180 / Math.PI

        'rzeczywiste wymiary obrazu w terenie (średnie z przeciwległych boków)
        Dim szerokosc As Double = (OdlegloscLokalna(ld, pd, mSz, mDl) + OdlegloscLokalna(lg, pg, mSz, mDl)) / 2
        Dim wysokosc As Double = (OdlegloscLokalna(ld, lg, mSz, mDl) + OdlegloscLokalna(pd, pg, mSz, mDl)) / 2
        Dim polowaSz As Double = wysokosc / 2 / mSz
        Dim polowaDl As Double = szerokosc / 2 / mDl

        Const st As String = "0.0000000000"   '10 miejsc po przecinku - ok. 0,01 mm
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8""?>").Append(Nl)
        sb.Append("<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"">").Append(Nl)
        sb.Append("<GroundOverlay>").Append(Nl)
        sb.Append("<name>").Append(System.Security.SecurityElement.Escape(nazwa)).Append("</name>").Append(Nl)
        sb.Append("<Icon>").Append(Nl)
        sb.Append("<href>").Append(System.Security.SecurityElement.Escape(o.NazwaPliku)).Append("</href>").Append(Nl)
        sb.Append("</Icon>").Append(Nl)
        sb.Append("<LatLonBox>").Append(Nl)
        sb.Append("<north>").Append((sr.Szerokosc + polowaSz).ToString(st, Ci)).Append("</north>").Append(Nl)
        sb.Append("<south>").Append((sr.Szerokosc - polowaSz).ToString(st, Ci)).Append("</south>").Append(Nl)
        sb.Append("<east>").Append((sr.Dlugosc + polowaDl).ToString(st, Ci)).Append("</east>").Append(Nl)
        sb.Append("<west>").Append((sr.Dlugosc - polowaDl).ToString(st, Ci)).Append("</west>").Append(Nl)
        sb.Append("<rotation>").Append(obrot.ToString(st, Ci)).Append("</rotation>").Append(Nl)
        sb.Append("</LatLonBox>").Append(Nl)
        sb.Append("<gx:LatLonQuad>").Append(Nl)
        'kolejność narożników wg KML: lewy dolny, prawy dolny, prawy górny, lewy górny (długość,szerokość)
        sb.Append("<coordinates>")
        Dim kolejne() As PunktGeo = {ld, pd, pg, lg}
        For i = 0 To 3
            If i > 0 Then sb.Append(" ")
            sb.Append(kolejne(i).Dlugosc.ToString(st, Ci)).Append(",").Append(kolejne(i).Szerokosc.ToString(st, Ci))
        Next
        sb.Append("</coordinates>").Append(Nl)
        sb.Append("</gx:LatLonQuad>").Append(Nl)
        sb.Append("</GroundOverlay>").Append(Nl)
        sb.Append("</kml>").Append(Nl)
        Return sb.ToString()
    End Function

    ''' <summary>Długość w metrach jednego stopnia szerokości i długości geograficznej na danej szerokości (GRS80).</summary>
    Public Shared Sub MetrowNaStopien(szerokosc As Double, ByRef mSz As Double, ByRef mDl As Double)
        Const a As Double = UkladWspolrzednych.PolosWielka
        Dim e2 As Double = UkladWspolrzednych.Splaszczenie * (2 - UkladWspolrzednych.Splaszczenie)
        Dim fi As Double = szerokosc * Math.PI / 180
        Dim w As Double = 1 - e2 * Math.Sin(fi) ^ 2
        mSz = a * (1 - e2) / (w ^ 1.5) * Math.PI / 180
        mDl = a / Math.Sqrt(w) * Math.Cos(fi) * Math.PI / 180
    End Sub

    Private Shared Function OdlegloscLokalna(p1 As PunktGeo, p2 As PunktGeo, mSz As Double, mDl As Double) As Double
        Return Math.Sqrt(((p2.Szerokosc - p1.Szerokosc) * mSz) ^ 2 + ((p2.Dlugosc - p1.Dlugosc) * mDl) ^ 2)
    End Function

    ''' <summary>Przybliżona odległość w metrach między bliskimi punktami.</summary>
    Public Shared Function OdlegloscMetry(p1 As PunktGeo, p2 As PunktGeo) As Double
        Dim mSz As Double, mDl As Double
        MetrowNaStopien((p1.Szerokosc + p2.Szerokosc) / 2, mSz, mDl)
        Return OdlegloscLokalna(p1, p2, mSz, mDl)
    End Function

    ''' <summary>Zapisuje tekst w pliku: KML w UTF-8 (zgodnie z deklaracją XML), pozostałe w kodowaniu podanym.</summary>
    Public Shared Sub Zapisz(sciezka As String, tresc As String, kodowanie As Encoding)
        File.WriteAllText(sciezka, tresc, kodowanie)
    End Sub

    ''' <summary>Kodowanie UTF-8 bez znacznika BOM (dla plików KML).</summary>
    Public Shared ReadOnly Property Utf8BezBom As Encoding
        Get
            Return New UTF8Encoding(False)
        End Get
    End Property

End Class
