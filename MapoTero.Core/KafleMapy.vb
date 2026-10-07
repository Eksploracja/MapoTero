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
Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>Prostokąt w WGS84 (stopnie).</summary>
Public Structure ProstokatGeo
    Public Polnoc As Double
    Public Poludnie As Double
    Public Wschod As Double
    Public Zachod As Double

    Public Sub New(polnoc As Double, poludnie As Double, wschod As Double, zachod As Double)
        Me.Polnoc = polnoc
        Me.Poludnie = poludnie
        Me.Wschod = wschod
        Me.Zachod = zachod
    End Sub

    Public ReadOnly Property Poprawny As Boolean
        Get
            Return Polnoc > Poludnie AndAlso Wschod > Zachod
        End Get
    End Property

    ''' <summary>Prostokąt opisany na zasięgu z innego układu (obejmuje cały obraz, z marginesami w narożnikach).</summary>
    Public Shared Function Opisany(uklad As UkladWspolrzednych, z As Zasieg) As ProstokatGeo
        Dim n() As PunktGeo = {uklad.DoWgs84(z.LewyGorny), uklad.DoWgs84(z.PrawyGorny), uklad.DoWgs84(z.PrawyDolny), uklad.DoWgs84(z.LewyDolny)}
        Dim p As New ProstokatGeo(Double.MinValue, Double.MaxValue, Double.MinValue, Double.MaxValue)
        For Each g In n
            p.Polnoc = Math.Max(p.Polnoc, g.Szerokosc)
            p.Poludnie = Math.Min(p.Poludnie, g.Szerokosc)
            p.Wschod = Math.Max(p.Wschod, g.Dlugosc)
            p.Zachod = Math.Min(p.Zachod, g.Dlugosc)
        Next
        Return p
    End Function

    ''' <summary>
    ''' Prostokąt wpisany w obraz z innego układu (w całości pokryty obrazem). Obraz z układu kartograficznego jest
    ''' w WGS84 lekko obrócony - prostokąt wpisany pozwala uniknąć białych klinów na krawędziach map bez przezroczystości.
    ''' </summary>
    Public Shared Function Wpisany(uklad As UkladWspolrzednych, z As Zasieg) As ProstokatGeo
        Dim lg = uklad.DoWgs84(z.LewyGorny), pg = uklad.DoWgs84(z.PrawyGorny), pd = uklad.DoWgs84(z.PrawyDolny), ld = uklad.DoWgs84(z.LewyDolny)
        'boki obrazu nie są w WGS84 prostymi - sprawdzane są także środki boków
        Dim sg = uklad.DoWgs84(New PunktXY(z.XGora, (z.YLewy + z.YPrawy) / 2))
        Dim sd = uklad.DoWgs84(New PunktXY(z.XDol, (z.YLewy + z.YPrawy) / 2))
        Dim sl = uklad.DoWgs84(New PunktXY((z.XDol + z.XGora) / 2, z.YLewy))
        Dim sp = uklad.DoWgs84(New PunktXY((z.XDol + z.XGora) / 2, z.YPrawy))
        Return New ProstokatGeo(Math.Min(Math.Min(lg.Szerokosc, pg.Szerokosc), sg.Szerokosc),
                                Math.Max(Math.Max(ld.Szerokosc, pd.Szerokosc), sd.Szerokosc),
                                Math.Min(Math.Min(pg.Dlugosc, pd.Dlugosc), sp.Dlugosc),
                                Math.Max(Math.Max(lg.Dlugosc, ld.Dlugosc), sl.Dlugosc))
    End Function
End Structure

''' <summary>Kafel mapy w siatce Web Mercator (XYZ, jak OpenStreetMap).</summary>
Public Structure KafelXYZ
    Public X As Integer
    Public Y As Integer
    Public Z As Integer

    Public Sub New(x As Integer, y As Integer, z As Integer)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub

    ''' <summary>Numer wiersza w schemacie TMS (MBTiles): wiersze liczone od dołu.</summary>
    Public ReadOnly Property WierszTms As Integer
        Get
            Return (1 << Z) - 1 - Y
        End Get
    End Property
End Structure

''' <summary>Obliczenia siatki kafli Web Mercator (EPSG:3857) używanej przez mapy internetowe i MBTiles.</summary>
Public NotInheritable Class WebMercator

    Private Sub New()
    End Sub

    ''' <summary>Rozmiar kafla w pikselach.</summary>
    Public Const RozmiarKafla As Integer = 256
    ''' <summary>Graniczna szerokość geograficzna odwzorowania.</summary>
    Public Const MaksSzerokosc As Double = 85.0511287798066

    ''' <summary>Globalna współrzędna piksela X (0 .. 256*2^z) dla długości geograficznej.</summary>
    Public Shared Function PikselX(dlugosc As Double, z As Integer) As Double
        Return (dlugosc + 180) / 360 * RozmiarKafla * (1L << z)
    End Function

    ''' <summary>Globalna współrzędna piksela Y (od północy) dla szerokości geograficznej.</summary>
    Public Shared Function PikselY(szerokosc As Double, z As Integer) As Double
        Dim s As Double = Math.Max(-MaksSzerokosc, Math.Min(MaksSzerokosc, szerokosc)) * Math.PI / 180
        Return (1 - Math.Log(Math.Tan(s) + 1 / Math.Cos(s)) / Math.PI) / 2 * RozmiarKafla * (1L << z)
    End Function

    ''' <summary>Długość geograficzna dla globalnej współrzędnej piksela X.</summary>
    Public Shared Function Dlugosc(px As Double, z As Integer) As Double
        Return px / (RozmiarKafla * (1L << z)) * 360 - 180
    End Function

    ''' <summary>Szerokość geograficzna dla globalnej współrzędnej piksela Y.</summary>
    Public Shared Function Szerokosc(py As Double, z As Integer) As Double
        Dim n As Double = Math.PI * (1 - 2 * py / (RozmiarKafla * (1L << z)))
        Return Math.Atan(Math.Sinh(n)) * 180 / Math.PI
    End Function

    ''' <summary>Kafle pokrywające prostokąt na danym poziomie: (xMin, yMin, xMax, yMax).</summary>
    Public Shared Function ZakresKafli(p As ProstokatGeo, z As Integer) As Tuple(Of Integer, Integer, Integer, Integer)
        Dim maks As Integer = (1 << z) - 1
        Dim x0 As Integer = Math.Max(0, CInt(Math.Floor(PikselX(p.Zachod, z) / RozmiarKafla)))
        Dim x1 As Integer = Math.Min(maks, CInt(Math.Floor((PikselX(p.Wschod, z) - 0.000001) / RozmiarKafla)))
        Dim y0 As Integer = Math.Max(0, CInt(Math.Floor(PikselY(p.Polnoc, z) / RozmiarKafla)))
        Dim y1 As Integer = Math.Min(maks, CInt(Math.Floor((PikselY(p.Poludnie, z) - 0.000001) / RozmiarKafla)))
        Return Tuple.Create(x0, y0, x1, y1)
    End Function

    ''' <summary>Liczba kafli pokrywających prostokąt na danym poziomie.</summary>
    Public Shared Function LiczbaKafli(p As ProstokatGeo, z As Integer) As Long
        Dim r = ZakresKafli(p, z)
        Return CLng(r.Item3 - r.Item1 + 1) * (r.Item4 - r.Item2 + 1)
    End Function

    ''' <summary>Rozmiar piksela w metrach na danej szerokości i poziomie powiększenia.</summary>
    Public Shared Function RozmiarPiksela(szerokosc As Double, z As Integer) As Double
        Return 2 * Math.PI * 6378137.0 * Math.Cos(szerokosc * Math.PI / 180) / (RozmiarKafla * (1L << z))
    End Function

    ''' <summary>Najmniejszy poziom, na którym piksel kafla nie jest większy od piksela źródła (pełna szczegółowość).</summary>
    Public Shared Function PoziomDlaRozdzielczosci(pikselMetry As Double, szerokosc As Double) As Integer
        For z = 0 To 22
            If RozmiarPiksela(szerokosc, z) <= pikselMetry * 1.0001 Then Return z
        Next
        Return 22
    End Function

End Class

''' <summary>Podział mapy na kafle KMZ w WGS84 (bez obrotu - wymagane przez odbiorniki Garmin).</summary>
Public Class PlanKmz

    ''' <summary>Obszar mapy.</summary>
    Public ReadOnly Property Obszar As ProstokatGeo
    ''' <summary>Rozmiar piksela w stopniach (szerokość, długość geograficzna).</summary>
    Public ReadOnly Property PikselSzerokosc As Double
    Public ReadOnly Property PikselDlugosc As Double
    ''' <summary>Rozmiar całego obrazu w pikselach.</summary>
    Public ReadOnly Property SzerokoscPx As Integer
    Public ReadOnly Property WysokoscPx As Integer
    ''' <summary>Liczba kolumn i wierszy kafli.</summary>
    Public ReadOnly Property Kolumny As Integer
    Public ReadOnly Property Wiersze As Integer
    ''' <summary>Maksymalny bok kafla w pikselach.</summary>
    Public ReadOnly Property BokKafla As Integer
    ''' <summary>Ile razy zmniejszono rozdzielczość, aby zmieścić się w limicie liczby kafli (1 - bez zmniejszenia).</summary>
    Public ReadOnly Property Pomniejszenie As Double

    ''' <param name="obszar">Obszar mapy w WGS84.</param>
    ''' <param name="pikselMetry">Rozmiar piksela źródła w metrach.</param>
    ''' <param name="bokKafla">Maksymalny bok kafla (Garmin: 1024).</param>
    ''' <param name="maksKafli">Maksymalna liczba kafli (Garmin: 100); 0 - bez limitu.</param>
    Public Sub New(obszar As ProstokatGeo, pikselMetry As Double, bokKafla As Integer, maksKafli As Integer)
        Me.Obszar = obszar
        Me.BokKafla = bokKafla
        Dim mSz As Double, mDl As Double
        Georeferencja.MetrowNaStopien((obszar.Polnoc + obszar.Poludnie) / 2, mSz, mDl)

        Dim skala As Double = 1
        Do
            Dim pSz As Double = pikselMetry * skala / mSz
            Dim pDl As Double = pikselMetry * skala / mDl
            Dim w As Integer = Math.Max(1, CInt(Math.Ceiling((obszar.Wschod - obszar.Zachod) / pDl - 0.000001)))
            Dim h As Integer = Math.Max(1, CInt(Math.Ceiling((obszar.Polnoc - obszar.Poludnie) / pSz - 0.000001)))
            Dim k As Integer = CInt(Math.Ceiling(w / CDbl(bokKafla)))
            Dim r As Integer = CInt(Math.Ceiling(h / CDbl(bokKafla)))
            If maksKafli <= 0 OrElse CLng(k) * r <= maksKafli Then
                'piksel dopasowany tak, by obraz dokładnie wypełniał obszar
                _SzerokoscPx = w : _WysokoscPx = h : _Kolumny = k : _Wiersze = r
                _PikselDlugosc = (obszar.Wschod - obszar.Zachod) / w
                _PikselSzerokosc = (obszar.Polnoc - obszar.Poludnie) / h
                _Pomniejszenie = skala
                Exit Do
            End If
            skala *= 1.05
        Loop
    End Sub

    ''' <summary>Liczba kafli.</summary>
    Public ReadOnly Property LiczbaKafli As Integer
        Get
            Return Kolumny * Wiersze
        End Get
    End Property

    ''' <summary>Zakres pikseli kafla (wiersz i kolumna od 0): x0, y0, szerokość, wysokość.</summary>
    Public Function PikseleKafla(wiersz As Integer, kolumna As Integer) As Tuple(Of Integer, Integer, Integer, Integer)
        'kafle możliwie równej wielkości (ostatni nie jest wąskim paskiem)
        Dim x0 As Integer = CInt(CLng(SzerokoscPx) * kolumna \ Kolumny)
        Dim x1 As Integer = CInt(CLng(SzerokoscPx) * (kolumna + 1) \ Kolumny)
        Dim y0 As Integer = CInt(CLng(WysokoscPx) * wiersz \ Wiersze)
        Dim y1 As Integer = CInt(CLng(WysokoscPx) * (wiersz + 1) \ Wiersze)
        Return Tuple.Create(x0, y0, x1 - x0, y1 - y0)
    End Function

    ''' <summary>Zasięg kafla w WGS84.</summary>
    Public Function ZasiegKafla(wiersz As Integer, kolumna As Integer) As ProstokatGeo
        Dim p = PikseleKafla(wiersz, kolumna)
        Return New ProstokatGeo(Obszar.Polnoc - p.Item2 * PikselSzerokosc, Obszar.Polnoc - (p.Item2 + p.Item4) * PikselSzerokosc,
                                Obszar.Zachod + (p.Item1 + p.Item3) * PikselDlugosc, Obszar.Zachod + p.Item1 * PikselDlugosc)
    End Function

End Class

''' <summary>Zapis pliku KMZ: archiwum ZIP z dokumentem doc.kml i kaflami w folderze "files".</summary>
Public Class ZapisKmz
    Implements IDisposable

    Private ReadOnly _plik As FileStream
    Private ReadOnly _zip As ZipArchive
    Private ReadOnly _kml As New StringBuilder()
    Private ReadOnly _nazwa As String
    Private ReadOnly _kolejnosc As Integer
    Private _zakonczony As Boolean

    ''' <param name="kolejnoscRysowania">drawOrder nakładek (Garmin: mapy o wyższej wartości rysowane są na wierzchu).</param>
    Public Sub New(sciezka As String, nazwa As String, kolejnoscRysowania As Integer)
        _plik = New FileStream(sciezka, FileMode.Create, FileAccess.ReadWrite)
        _zip = New ZipArchive(_plik, ZipArchiveMode.Create, True)
        _nazwa = nazwa
        _kolejnosc = kolejnoscRysowania
    End Sub

    ''' <summary>Dodaje kafel (obraz JPEG) z jego zasięgiem.</summary>
    Public Sub DodajKafel(nazwaPliku As String, obraz() As Byte, zasieg As ProstokatGeo)
        Dim wpis = _zip.CreateEntry("files/" & nazwaPliku, CompressionLevel.NoCompression)
        Using s = wpis.Open()
            s.Write(obraz, 0, obraz.Length)
        End Using
        Dim ci = CultureInfo.InvariantCulture
        Const f As String = "0.0000000000"
        _kml.Append("<GroundOverlay>").Append(ChrW(10))
        _kml.Append("<name>").Append(Security.SecurityElement.Escape(Path.GetFileNameWithoutExtension(nazwaPliku))).Append("</name>").Append(ChrW(10))
        _kml.Append("<drawOrder>").Append(_kolejnosc.ToString(ci)).Append("</drawOrder>").Append(ChrW(10))
        _kml.Append("<Icon><href>files/").Append(Security.SecurityElement.Escape(nazwaPliku)).Append("</href></Icon>").Append(ChrW(10))
        _kml.Append("<LatLonBox><north>").Append(zasieg.Polnoc.ToString(f, ci)).Append("</north><south>").Append(zasieg.Poludnie.ToString(f, ci)).
             Append("</south><east>").Append(zasieg.Wschod.ToString(f, ci)).Append("</east><west>").Append(zasieg.Zachod.ToString(f, ci)).
             Append("</west></LatLonBox>").Append(ChrW(10))
        _kml.Append("</GroundOverlay>").Append(ChrW(10))
    End Sub

    ''' <summary>Zapisuje doc.kml i zamyka archiwum.</summary>
    Public Sub Zakoncz()
        Dim doc As New StringBuilder()
        doc.Append("<?xml version=""1.0"" encoding=""UTF-8""?>").Append(ChrW(10))
        doc.Append("<kml xmlns=""http://www.opengis.net/kml/2.2"">").Append(ChrW(10))
        doc.Append("<Folder>").Append(ChrW(10))
        doc.Append("<name>").Append(Security.SecurityElement.Escape(_nazwa)).Append("</name>").Append(ChrW(10))
        doc.Append(_kml)
        doc.Append("</Folder>").Append(ChrW(10))
        doc.Append("</kml>").Append(ChrW(10))
        Dim wpis = _zip.CreateEntry("doc.kml", CompressionLevel.Optimal)
        Using s = wpis.Open()
            Dim b() As Byte = New UTF8Encoding(False).GetBytes(doc.ToString())
            s.Write(b, 0, b.Length)
        End Using
        _zip.Dispose()
        _plik.Dispose()
        _zakonczony = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _zakonczony Then
            _zip.Dispose()
            _plik.Dispose()
        End If
    End Sub

End Class

''' <summary>
''' Interpolacja przeliczeń współrzędnych w obrębie kafla: dokładne przeliczenie tylko w węzłach siatki
''' (np. co 16 pikseli), między nimi interpolacja dwuliniowa - błąd pomijalny, a obliczenia wielokrotnie szybsze.
''' </summary>
Public Class SiatkaPrzeliczen

    Private ReadOnly _x(,) As Double
    Private ReadOnly _y(,) As Double
    Private ReadOnly _krok As Integer
    Private ReadOnly _szer As Integer
    Private ReadOnly _wys As Integer

    ''' <param name="szerokosc">Szerokość obszaru w pikselach.</param>
    ''' <param name="wysokosc">Wysokość obszaru w pikselach.</param>
    ''' <param name="krok">Odstęp węzłów siatki w pikselach.</param>
    ''' <param name="przelicz">Przeliczenie dla środka piksela (px, py) - wynik w układzie źródła.</param>
    Public Sub New(szerokosc As Integer, wysokosc As Integer, krok As Integer, przelicz As Func(Of Double, Double, PunktXY))
        _krok = krok
        _szer = szerokosc
        _wys = wysokosc
        Dim nx As Integer = szerokosc \ krok + 2
        Dim ny As Integer = wysokosc \ krok + 2
        ReDim _x(nx - 1, ny - 1)
        ReDim _y(nx - 1, ny - 1)
        For j = 0 To ny - 1
            For i = 0 To nx - 1
                Dim p = przelicz(i * krok, j * krok)
                _x(i, j) = p.X
                _y(i, j) = p.Y
            Next
        Next
    End Sub

    ''' <summary>Przeliczony punkt dla piksela (px, py).</summary>
    Public Function Punkt(px As Double, py As Double) As PunktXY
        Dim fx As Double = px / _krok, fy As Double = py / _krok
        Dim i As Integer = Math.Min(_x.GetLength(0) - 2, Math.Max(0, CInt(Math.Floor(fx))))
        Dim j As Integer = Math.Min(_x.GetLength(1) - 2, Math.Max(0, CInt(Math.Floor(fy))))
        Dim tx As Double = fx - i, ty As Double = fy - j
        Dim x As Double = (_x(i, j) * (1 - tx) + _x(i + 1, j) * tx) * (1 - ty) + (_x(i, j + 1) * (1 - tx) + _x(i + 1, j + 1) * tx) * ty
        Dim y As Double = (_y(i, j) * (1 - tx) + _y(i + 1, j) * tx) * (1 - ty) + (_y(i, j + 1) * (1 - tx) + _y(i + 1, j + 1) * tx) * ty
        Return New PunktXY(x, y)
    End Function

End Class
