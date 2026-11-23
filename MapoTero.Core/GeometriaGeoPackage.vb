'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq

''' <summary>
''' Geometrie odczytane z warstwy wektorowej GeoPackage: poligony (każdy jako lista pierścieni, pierwszy - obrys)
''' i linie, we współrzędnych pliku (Wschod = x, Polnoc = y). GeoPackage zapisuje zawsze x = wschód / długość,
''' y = północ / szerokość, więc - inaczej niż w GeoJSON - kolejność osi nie wymaga zgadywania.
''' </summary>
Public NotInheritable Class GeometriaWektorowa
    Public ReadOnly Property Poligony As New List(Of List(Of List(Of Punkt2D)))
    Public ReadOnly Property Linie As New List(Of List(Of Punkt2D))
    ''' <summary>Kod EPSG układu warstwy (0 - nieznany).</summary>
    Public Property Epsg As Integer

    Public Sub Dolacz(inna As GeometriaWektorowa)
        Poligony.AddRange(inna.Poligony)
        Linie.AddRange(inna.Linie)
    End Sub

    ''' <summary>Czy program umie przeliczyć układ warstwy (WGS84, Web Mercator, PL-1992, PL-2000, UTM).</summary>
    Public Shared Function ObslugiwanyUklad(epsg As Integer) As Boolean
        Return epsg = 4326 OrElse epsg = 3857 OrElse epsg = 900913 OrElse ZnanyUklad(epsg) IsNot Nothing
    End Function

    ''' <summary>Układ programu o danym kodzie EPSG albo Nothing (UkladWspolrzednych.ZKodu przy nieznanym kodzie zwraca PL-1992).</summary>
    Private Shared Function ZnanyUklad(epsg As Integer) As UkladWspolrzednych
        Return UkladWspolrzednych.Wszystkie.FirstOrDefault(Function(u) u.Epsg = epsg)
    End Function

    ''' <summary>Punkt w układzie warstwy przeliczony na WGS84 (Wschod = długość, Polnoc = szerokość).</summary>
    Public Shared Function DoWgs84(epsg As Integer, p As Punkt2D) As Punkt2D
        Select Case epsg
            Case 4326
                Return p
            Case 3857, 900913
                Return ZestawPoligonow.PunktDoWgs84(RodzinaUkladuPoligonu.WebMercator, p)
        End Select
        Dim uklad = ZnanyUklad(epsg)
        If uklad Is Nothing Then Throw New NotSupportedException(OpisNieobslugiwanegoUkladu(epsg))
        If uklad.Geograficzny Then Return p
        Dim g = uklad.DoWgs84(New PunktXY(p.Polnoc, p.Wschod))
        Return New Punkt2D(g.Dlugosc, g.Szerokosc)
    End Function

    Public Shared Function OpisNieobslugiwanegoUkladu(epsg As Integer) As String
        Return If(epsg <= 0, "Warstwa nie ma określonego układu współrzędnych.",
                  $"Układ współrzędnych warstwy (EPSG:{epsg}) nie jest obsługiwany.") &
               " Zapisz ją w QGIS w układzie PL-1992 (EPSG:2180), PL-2000, UTM albo WGS84 (EPSG:4326)."
    End Function

    ''' <summary>Poligony przeliczone na WGS84 jako zestaw do podglądu i wyznaczenia obszaru pobierania.</summary>
    Public Function ZestawPoligonowWgs84() As ZestawPoligonow
        Dim wielokaty As New List(Of Wielokat)
        Dim przelicz As Func(Of Punkt2D, Punkt2D)
        If Epsg = 4326 Then
            przelicz = Function(p) p
        ElseIf Epsg = 3857 OrElse Epsg = 900913 Then
            przelicz = Function(p) ZestawPoligonow.PunktDoWgs84(RodzinaUkladuPoligonu.WebMercator, p)
        Else
            Dim uklad = ZnanyUklad(Epsg)
            If uklad Is Nothing Then Throw New NotSupportedException(OpisNieobslugiwanegoUkladu(Epsg))
            If uklad.Geograficzny Then
                przelicz = Function(p) p
            Else
                przelicz = Function(p)
                               Dim g = uklad.DoWgs84(New PunktXY(p.Polnoc, p.Wschod))
                               Return New Punkt2D(g.Dlugosc, g.Szerokosc)
                           End Function
            End If
        End If
        For Each poligon In Poligony
            If poligon.Count = 0 OrElse poligon(0).Count < 3 Then Continue For
            Dim pierscienie As New List(Of PierscienPoligonu)()
            For i = 0 To poligon.Count - 1
                Dim r = poligon(i)
                If r.Count < 3 Then
                    If i = 0 Then Exit For
                    Continue For
                End If
                pierscienie.Add(New PierscienPoligonu(r.Select(przelicz).ToList()))
            Next
            If pierscienie.Count > 0 Then wielokaty.Add(New Wielokat(pierscienie(0), pierscienie.Skip(1)))
        Next
        If wielokaty.Count = 0 Then Throw New InvalidDataException("Warstwa nie zawiera poligonów.")
        Return New ZestawPoligonow(wielokaty) With {
            .WykrytyUklad = RodzinaUkladuPoligonu.Wgs84,
            .NiejednoznaczneOsie = False,
            .ZrodloWykryciaUkladu = $"GeoPackage (EPSG:{Epsg}), przeliczony na WGS84"}
    End Function

    ''' <summary>Linie przeliczone na WGS84 jako trasa korytarza.</summary>
    Public Function TrasaWgs84(nazwa As String) As Korytarz
        Dim k As New Korytarz With {.Nazwa = nazwa}
        For Each linia In Linie
            If linia.Count < 2 Then Continue For
            k.DodajLinie(linia.Select(Function(p)
                                          Dim g = DoWgs84(Epsg, p)
                                          Return New PunktGeo(g.Polnoc, g.Wschod)
                                      End Function))
        Next
        Return k
    End Function
End Class

''' <summary>
''' Odczyt geometrii GeoPackage (GeoPackageBinary: nagłówek „GP”, koperta, WKB). Obsługuje Point, LineString,
''' Polygon, Multi* i GeometryCollection, także z wysokością Z lub miarą M (ISO: typ + 1000/2000/3000,
''' EWKB: flagi 0x80000000/0x40000000); punkty pomija, bo nie wyznaczają obszaru.
''' </summary>
Public Module CzytnikGeometriiGpkg

    ''' <summary>Geometria z wartości kolumny geometrii GeoPackage (pusta lub uszkodzona - pusty wynik).</summary>
    Public Function ZBlobu(dane() As Byte) As GeometriaWektorowa
        Dim wynik As New GeometriaWektorowa()
        If dane Is Nothing OrElse dane.Length < 8 OrElse dane(0) <> AscW("G"c) OrElse dane(1) <> AscW("P"c) Then Return wynik
        Dim flagi = dane(3)
        Dim rodzajKoperty = (flagi >> 1) And &H7
        Dim pusta = (flagi And &H10) <> 0
        If pusta Then Return wynik
        Dim dlugoscKoperty As Integer
        Select Case rodzajKoperty
            Case 0 : dlugoscKoperty = 0
            Case 1 : dlugoscKoperty = 32
            Case 2, 3 : dlugoscKoperty = 48
            Case 4 : dlugoscKoperty = 64
            Case Else : Throw New InvalidDataException("Nieznany rodzaj koperty geometrii GeoPackage: " & rodzajKoperty)
        End Select
        Dim poczatek = 8 + dlugoscKoperty
        If poczatek >= dane.Length Then Return wynik
        Using ms As New MemoryStream(dane, poczatek, dane.Length - poczatek), r As New BinaryReader(ms)
            CzytajWkb(r, wynik)
        End Using
        Return wynik
    End Function

    ''' <summary>Geometria z czystego WKB (bez nagłówka GeoPackage).</summary>
    Public Function ZWkb(dane() As Byte) As GeometriaWektorowa
        Dim wynik As New GeometriaWektorowa()
        Using ms As New MemoryStream(dane), r As New BinaryReader(ms)
            CzytajWkb(r, wynik)
        End Using
        Return wynik
    End Function

    Private Sub CzytajWkb(r As BinaryReader, wynik As GeometriaWektorowa, Optional glebokosc As Integer = 0)
        If glebokosc > 32 Then Throw New InvalidDataException("Przekroczono maksymalną głębokość zagnieżdżenia geometrii WKB.")
        Dim bajtEndian = r.ReadByte()
        If bajtEndian <> 0 AndAlso bajtEndian <> 1 Then Throw New InvalidDataException("Nieprawidłowy bajt kolejności bajtów WKB.")
        Dim malyEndian = (bajtEndian = 1)
        Dim typSurowy = CzytajUInt32(r, malyEndian)
        Dim maZ = (typSurowy And &H80000000UI) <> 0
        Dim maM = (typSurowy And &H40000000UI) <> 0
        If (typSurowy And &H20000000UI) <> 0 Then CzytajUInt32(r, malyEndian) 'EWKB z SRID
        Dim typ = CInt(typSurowy And &HFFFFFFUI)
        Select Case typ \ 1000
            Case 0
            Case 1 : maZ = True
            Case 2 : maM = True
            Case 3 : maZ = True : maM = True
            Case Else
                Throw New InvalidDataException("Nieobsługiwany typ geometrii WKB: " & typ)
        End Select
        Dim wymiary = 2 + If(maZ, 1, 0) + If(maM, 1, 0)
        Select Case typ Mod 1000
            Case 1 'Point
                CzytajPunkt(r, malyEndian, wymiary)
            Case 2 'LineString
                wynik.Linie.Add(CzytajCiag(r, malyEndian, wymiary))
            Case 3 'Polygon
                Dim liczba = CzytajLiczbe(r, malyEndian, 4)
                Dim poligon As New List(Of List(Of Punkt2D))
                For i = 1 To liczba
                    poligon.Add(CzytajCiag(r, malyEndian, wymiary))
                Next
                If poligon.Count > 0 Then wynik.Poligony.Add(poligon)
            Case 4, 5, 6, 7 'Multi* i GeometryCollection - każdy element ma własny nagłówek WKB
                Dim liczba = CzytajLiczbe(r, malyEndian, 5)
                For i = 1 To liczba
                    CzytajWkb(r, wynik, glebokosc + 1)
                Next
            Case Else
                Throw New InvalidDataException("Nieobsługiwany typ geometrii WKB: " & typ)
        End Select
    End Sub

    Private Function CzytajCiag(r As BinaryReader, malyEndian As Boolean, wymiary As Integer) As List(Of Punkt2D)
        Dim liczba = CzytajLiczbe(r, malyEndian, wymiary * 8)
        Dim lista As New List(Of Punkt2D)(liczba)
        For i = 1 To liczba
            lista.Add(CzytajPunkt(r, malyEndian, wymiary))
        Next
        Return lista
    End Function

    Private Function CzytajPunkt(r As BinaryReader, malyEndian As Boolean, wymiary As Integer) As Punkt2D
        Dim x = CzytajDouble(r, malyEndian), y = CzytajDouble(r, malyEndian)
        For i = 3 To wymiary
            CzytajDouble(r, malyEndian)
        Next
        Return New Punkt2D(x, y)
    End Function

    ''' <summary>Liczba elementów z zabezpieczeniem przed uszkodzonym plikiem (np. miliard punktów).</summary>
    Private Function CzytajLiczbe(r As BinaryReader, malyEndian As Boolean, Optional rozmiarElementu As Integer = 4) As Integer
        Dim n = CzytajUInt32(r, malyEndian)
        Dim pozostalo = Math.Max(0L, r.BaseStream.Length - r.BaseStream.Position)
        Dim maksElementow = pozostalo \ Math.Max(1, rozmiarElementu)
        If n > CUInt(Math.Min(Integer.MaxValue, maksElementow)) Then Throw New InvalidDataException("Uszkodzona geometria WKB.")
        Return CInt(n)
    End Function

    Private Function CzytajUInt32(r As BinaryReader, malyEndian As Boolean) As UInteger
        Dim b = r.ReadBytes(4)
        If b.Length < 4 Then Throw New EndOfStreamException()
        If BitConverter.IsLittleEndian <> malyEndian Then Array.Reverse(b)
        Return BitConverter.ToUInt32(b, 0)
    End Function

    Private Function CzytajDouble(r As BinaryReader, malyEndian As Boolean) As Double
        Dim b = r.ReadBytes(8)
        If b.Length < 8 Then Throw New EndOfStreamException()
        If BitConverter.IsLittleEndian <> malyEndian Then Array.Reverse(b)
        Return BitConverter.ToDouble(b, 0)
    End Function
End Module
