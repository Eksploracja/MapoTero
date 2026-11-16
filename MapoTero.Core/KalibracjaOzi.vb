'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO

''' <summary>Pojedynczy punkt kalibracyjny z pliku .map OziExplorera.</summary>
Public Class PunktKalibracjiOzi
    Public Property Nazwa As String = ""
    Public Property Px As Double
    Public Property Py As Double
    Public Property ZrodloweLat As Double
    Public Property ZrodloweLon As Double
    Public Property Wgs84 As PunktGeo

    Public Overrides Function ToString() As String
        Return String.Format(CultureInfo.InvariantCulture, "{0}: px({1}, {2}) -> WGS84({3:0.000000}, {4:0.000000})",
                             Nazwa, Px, Py, Wgs84.Szerokosc, Wgs84.Dlugosc)
    End Function
End Class

''' <summary>
''' Dwuwymiarowa transformacja afiniczna (6 parametrów) łącząca współrzędne pikselowe obrazu (px, py)
''' ze współrzędnymi geograficznymi WGS84 (lon, lat).
''' lon = A*px + B*py + C
''' lat = D*px + E*py + F
''' </summary>
Public Class TransformacjaAfiniczna2D
    Public Property A As Double
    Public Property B As Double
    Public Property C As Double
    Public Property D As Double
    Public Property E As Double
    Public Property F As Double

    Public Property BladRmsMetry As Double
    Public Property LiczbaPunktow As Integer

    Public Function PikselDoGeo(px As Double, py As Double) As PunktGeo
        Dim lon = A * px + B * py + C
        Dim lat = D * px + E * py + F
        Return New PunktGeo(lat, lon)
    End Function

    Public Function GeoDoPiksel(lat As Double, lon As Double) As Tuple(Of Double, Double)
        Dim det = A * E - B * D
        If Math.Abs(det) < 1.0E-15 Then Return Tuple.Create(0.0, 0.0)
        Dim dLon = lon - C
        Dim dLat = lat - F
        Dim px = (E * dLon - B * dLat) / det
        Dim py = (-D * dLon + A * dLat) / det
        Return Tuple.Create(px, py)
    End Function

    ''' <summary>
    ''' Wyznacza optymalne współczynniki transformacji afinicznej metodą najmniejszych kwadratów (LSM)
    ''' na podstawie listy punktów kalibracyjnych.
    ''' </summary>
    Public Shared Function Dopasuj(punkty As IList(Of PunktKalibracjiOzi)) As TransformacjaAfiniczna2D
        If punkty Is Nothing OrElse punkty.Count < 2 Then
            Throw New InvalidDataException("Wymagane są co najmniej 2 punkty kalibracyjne.")
        End If

        Dim t As New TransformacjaAfiniczna2D With {.LiczbaPunktow = punkty.Count}

        If punkty.Count = 2 Then
            ' Uproszczona transformacja dla 2 punktów (brak obrotu / rozciągania skośnego)
            Dim p1 = punkty(0)
            Dim p2 = punkty(1)
            Dim dpx = p2.Px - p1.Px
            Dim dpy = p2.Py - p1.Py
            If Math.Abs(dpx) < 0.001 Then dpx = 0.001
            If Math.Abs(dpy) < 0.001 Then dpy = 0.001

            t.A = (p2.Wgs84.Dlugosc - p1.Wgs84.Dlugosc) / dpx
            t.B = 0
            t.C = p1.Wgs84.Dlugosc - t.A * p1.Px

            t.D = 0
            t.E = (p2.Wgs84.Szerokosc - p1.Wgs84.Szerokosc) / dpy
            t.F = p1.Wgs84.Szerokosc - t.E * p1.Py
            t.BladRmsMetry = 0.0
            Return t
        End If

        ' Regresja afiniczna LSM dla N >= 3 punktów
        Dim n As Double = punkty.Count
        Dim sX As Double = 0, sY As Double = 0, sX2 As Double = 0, sY2 As Double = 0, sXY As Double = 0
        Dim sLon As Double = 0, sXLon As Double = 0, sYLon As Double = 0
        Dim sLat As Double = 0, sXLat As Double = 0, sYLat As Double = 0

        For Each pt In punkty
            Dim x = pt.Px
            Dim y = pt.Py
            Dim lon = pt.Wgs84.Dlugosc
            Dim lat = pt.Wgs84.Szerokosc

            sX += x
            sY += y
            sX2 += x * x
            sY2 += y * y
            sXY += x * y

            sLon += lon
            sXLon += x * lon
            sYLon += y * lon

            sLat += lat
            sXLat += x * lat
            sYLat += y * lat
        Next

        ' Macierz 3x3 normalna
        ' [ sX2  sXY  sX  ] [ A ]   [ sXLon ]
        ' [ sXY  sY2  sY  ] [ B ] = [ sYLon ]
        ' [ sX   sY   n   ] [ C ]   [ sLon  ]

        Dim c00 = sY2 * n - sY * sY
        Dim c01 = -(sXY * n - sY * sX)
        Dim c02 = sXY * sY - sY2 * sX
        Dim c11 = sX2 * n - sX * sX
        Dim c12 = -(sX2 * sY - sXY * sX)
        Dim c22 = sX2 * sY2 - sXY * sXY

        Dim det = sX2 * c00 + sXY * c01 + sX * c02

        If Math.Abs(det) < 1.0E-12 Then
            ' Punkty współliniowe - fallback do 2 pierwszych punktów
            Return Dopasuj(New List(Of PunktKalibracjiOzi) From {punkty(0), punkty(1)})
        End If

        t.A = (c00 * sXLon + c01 * sYLon + c02 * sLon) / det
        t.B = (c01 * sXLon + c11 * sYLon + c12 * sLon) / det
        t.C = (c02 * sXLon + c12 * sYLon + c22 * sLon) / det

        t.D = (c00 * sXLat + c01 * sYLat + c02 * sLat) / det
        t.E = (c01 * sXLat + c11 * sYLat + c12 * sLat) / det
        t.F = (c02 * sXLat + c12 * sYLat + c22 * sLat) / det

        ' Obliczenie błędu średniokwadratowego RMS w metrach
        Dim sumaKwadratow As Double = 0
        Dim srSzer As Double = sLat / n
        Dim cosFi = Math.Cos(srSzer * Math.PI / 180.0)
        Dim mLat = 111132.95
        Dim mLon = 111412.84 * cosFi

        For Each pt In punkty
            Dim geo = t.PikselDoGeo(pt.Px, pt.Py)
            Dim dLatM = (geo.Szerokosc - pt.Wgs84.Szerokosc) * mLat
            Dim dLonM = (geo.Dlugosc - pt.Wgs84.Dlugosc) * mLon
            sumaKwadratow += dLatM * dLatM + dLonM * dLonM
        Next

        t.BladRmsMetry = Math.Round(Math.Sqrt(sumaKwadratow / n), 2)
        Return t
    End Function
End Class

''' <summary>Odczyt i obsługa pliku kalibracyjnego .map OziExplorera.</summary>
Public Class PlikKalibracjiOzi
    Public Property SciezkaMap As String = ""
    Public Property Tytul As String = ""
    Public Property PlikObrazu As String = ""
    Public Property PlikObrazuIstnieje As Boolean
    Public Property Datum As String = "WGS 84"
    Public Property Punkty As New List(Of PunktKalibracjiOzi)
    Public Property Transformacja As TransformacjaAfiniczna2D
    Public Property ZasiegWgs84 As ProstokatGeo
    Public Property SzerokoscPx As Integer
    Public Property WysokoscPx As Integer
    Public Property RozdzielczoscMetrowNaPiksel As Double

    ''' <summary>
    ''' Wczytuje plik .map OziExplorera, wyznacza punkty kalibracyjne i przelicza datum do WGS84.
    ''' </summary>
    Public Shared Function Wczytaj(sciezkaPlikuMap As String, Optional sciezkaObrazu As String = Nothing) As PlikKalibracjiOzi
        If Not File.Exists(sciezkaPlikuMap) Then
            Throw New FileNotFoundException("Nie odnaleziono pliku mapy: " & sciezkaPlikuMap)
        End If

        Dim ozi As New PlikKalibracjiOzi With {.SciezkaMap = sciezkaPlikuMap}
        Dim linie = File.ReadAllLines(sciezkaPlikuMap, System.Text.Encoding.Default)
        If linie.Length < 5 Then
            Throw New InvalidDataException("Plik nie jest poprawnym plikiem kalibracyjnym OziExplorer (.map).")
        End If

        ozi.Tytul = linie(1).Trim()
        Dim obrazWPliku = linie(2).Trim()

        ' Wyznaczenie ścieżki pliku obrazu
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(sciezkaPlikuMap))
        Dim kandydat As String = Nothing

        If Not String.IsNullOrEmpty(sciezkaObrazu) AndAlso File.Exists(sciezkaObrazu) Then
            kandydat = sciezkaObrazu
        ElseIf File.Exists(obrazWPliku) Then
            kandydat = obrazWPliku
        Else
            Dim nazwa = Path.GetFileName(obrazWPliku)
            Dim sciezkaFolder = Path.Combine(folder, nazwa)
            If File.Exists(sciezkaFolder) Then
                kandydat = sciezkaFolder
            Else
                Dim rdzen = Path.GetFileNameWithoutExtension(sciezkaPlikuMap)
                For Each rozszerzenie In New String() {".jpg", ".png", ".tif", ".tiff", ".bmp", ".jpeg", ".gif"}
                    Dim p = Path.Combine(folder, rdzen & rozszerzenie)
                    If File.Exists(p) Then
                        kandydat = p
                        Exit For
                    End If
                Next
            End If
        End If

        If kandydat IsNot Nothing AndAlso File.Exists(kandydat) Then
            ozi.PlikObrazu = kandydat
            ozi.PlikObrazuIstnieje = True
            Try
                Dim nag = NaglowekObrazu.Odczytaj(kandydat)
                If nag IsNot Nothing Then
                    ozi.SzerokoscPx = CInt(nag.SzerokoscPx)
                    ozi.WysokoscPx = CInt(nag.WysokoscPx)
                End If
            Catch
            End Try
        Else
            ozi.PlikObrazu = If(String.IsNullOrEmpty(obrazWPliku), Path.GetFileNameWithoutExtension(sciezkaPlikuMap) & ".jpg", obrazWPliku)
            ozi.PlikObrazuIstnieje = False
        End If

        ' Odczyt Datum (linia 5)
        Dim liniaDatum = linie(4).Trim()
        Dim czesciDatum = liniaDatum.Split(","c)
        ozi.Datum = If(czesciDatum.Length > 0 AndAlso Not String.IsNullOrWhiteSpace(czesciDatum(0)), czesciDatum(0).Trim(), "WGS 84")

        ' Odczyt punktów kalibracyjnych
        For i = 0 To linie.Length - 1
            Dim l = linie(i).Trim()
            If l.StartsWith("Point", StringComparison.OrdinalIgnoreCase) Then
                Dim pt = SparsujPunkt(l, ozi.Datum)
                If pt IsNot Nothing Then
                    ozi.Punkty.Add(pt)
                End If
            End If
        Next

        If ozi.Punkty.Count < 2 Then
            Throw New InvalidDataException("Plik .map zawiera mniej niż 2 aktywne punkty kalibracyjne.")
        End If

        ' Wyznaczenie transformacji afinicznej
        ozi.Transformacja = TransformacjaAfiniczna2D.Dopasuj(ozi.Punkty)

        ' Wyznaczenie zasięgu WGS84 i rozdzielczości
        If ozi.SzerokoscPx > 0 AndAlso ozi.WysokoscPx > 0 Then
            Dim w = CDbl(ozi.SzerokoscPx)
            Dim h = CDbl(ozi.WysokoscPx)
            Dim pLG = ozi.Transformacja.PikselDoGeo(0, 0)
            Dim pPG = ozi.Transformacja.PikselDoGeo(w, 0)
            Dim pPD = ozi.Transformacja.PikselDoGeo(w, h)
            Dim pLD = ozi.Transformacja.PikselDoGeo(0, h)

            Dim minLat = Math.Min(Math.Min(pLG.Szerokosc, pPG.Szerokosc), Math.Min(pPD.Szerokosc, pLD.Szerokosc))
            Dim maxLat = Math.Max(Math.Max(pLG.Szerokosc, pPG.Szerokosc), Math.Max(pPD.Szerokosc, pLD.Szerokosc))
            Dim minLon = Math.Min(Math.Min(pLG.Dlugosc, pPG.Dlugosc), Math.Min(pPD.Dlugosc, pLD.Dlugosc))
            Dim maxLon = Math.Max(Math.Max(pLG.Dlugosc, pPG.Dlugosc), Math.Max(pPD.Dlugosc, pLD.Dlugosc))

            ozi.ZasiegWgs84 = New ProstokatGeo(maxLat, minLat, maxLon, minLon)

            Dim cosFi = Math.Cos((maxLat + minLat) / 2.0 * Math.PI / 180.0)
            Dim szerM = (maxLon - minLon) * 111412.84 * cosFi
            Dim wysM = (maxLat - minLat) * 111132.95
            ozi.RozdzielczoscMetrowNaPiksel = Math.Round((szerM / w + wysM / h) / 2.0, 2)
        Else
            ' Przy braku obrazu bazujemy na punktach
            Dim minLat = Double.MaxValue, maxLat = Double.MinValue
            Dim minLon = Double.MaxValue, maxLon = Double.MinValue
            For Each pt In ozi.Punkty
                minLat = Math.Min(minLat, pt.Wgs84.Szerokosc)
                maxLat = Math.Max(maxLat, pt.Wgs84.Szerokosc)
                minLon = Math.Min(minLon, pt.Wgs84.Dlugosc)
                maxLon = Math.Max(maxLon, pt.Wgs84.Dlugosc)
            Next
            ozi.ZasiegWgs84 = New ProstokatGeo(maxLat, minLat, maxLon, minLon)
        End If

        Return ozi
    End Function

    ''' <summary>Parsuje pojedynczy wiersz punktu kalibracyjnego PointXX z pliku .map.</summary>
    Public Shared Function SparsujPunkt(wiersz As String, datum As String) As PunktKalibracjiOzi
        Dim ci = CultureInfo.InvariantCulture
        Dim czesci = wiersz.Split(","c)
        If czesci.Length < 11 Then Return Nothing

        Dim nazwa = czesci(0).Trim()
        Dim pxStr = czesci(2).Trim()
        Dim pyStr = czesci(3).Trim()

        Dim px As Double, py As Double
        If Not Double.TryParse(pxStr, NumberStyles.Float, ci, px) OrElse
           Not Double.TryParse(pyStr, NumberStyles.Float, ci, py) Then
            Return Nothing
        End If

        ' Szukamy indeksów N/S oraz E/W
        Dim idxNS As Integer = -1
        Dim idxEW As Integer = -1
        For i = 4 To czesci.Length - 1
            Dim t = czesci(i).Trim().ToUpperInvariant()
            If (t = "N" OrElse t = "S") AndAlso idxNS < 0 Then
                idxNS = i
            ElseIf (t = "E" OrElse t = "W") AndAlso idxEW < 0 Then
                idxEW = i
            End If
        Next

        If idxNS < 0 OrElse idxEW < 0 Then Return Nothing

        Dim lat = WyznaczWspolrzedna(czesci, 4, idxNS - 1, czesci(idxNS).Trim().ToUpperInvariant() = "S")
        Dim lon = WyznaczWspolrzedna(czesci, idxNS + 1, idxEW - 1, czesci(idxEW).Trim().ToUpperInvariant() = "W")

        Dim wgs = PrzeliczDatumDoWgs84(lat, lon, datum)

        Return New PunktKalibracjiOzi With {
            .Nazwa = nazwa,
            .Px = px,
            .Py = py,
            .ZrodloweLat = lat,
            .ZrodloweLon = lon,
            .Wgs84 = wgs
        }
    End Function

    Private Shared Function WyznaczWspolrzedna(czesci As String(), startIdx As Integer, endIdx As Integer, ujemna As Boolean) As Double
        Dim ci = CultureInfo.InvariantCulture
        Dim liczby As New List(Of Double)
        For i = startIdx To endIdx
            Dim s = czesci(i).Trim()
            If s.Equals("deg", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("in", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("min", StringComparison.OrdinalIgnoreCase) OrElse
               String.IsNullOrEmpty(s) Then
                Continue For
            End If
            Dim val As Double
            If Double.TryParse(s, NumberStyles.Float, ci, val) Then
                liczby.Add(val)
            End If
        Next

        Dim wynik As Double = 0
        If liczby.Count = 1 Then
            wynik = liczby(0)
        ElseIf liczby.Count >= 2 Then
            ' Stopnie i minuty
            wynik = liczby(0) + liczby(1) / 60.0
            If liczby.Count >= 3 Then
                ' Stopnie, minuty i sekundy
                wynik += liczby(2) / 3600.0
            End If
        End If

        Return If(ujemna, -wynik, wynik)
    End Function

    ''' <summary>
    ''' Przelicza współrzędne geograficzne z zadanego Datum (np. Borowa Góra, Potsdam, Pulkovo 1942) do WGS84
    ''' przy użyciu standardowej 3-parametrowej transformacji geocentrycznej.
    ''' </summary>
    Public Shared Function PrzeliczDatumDoWgs84(lat As Double, lon As Double, datum As String) As PunktGeo
        If String.IsNullOrWhiteSpace(datum) Then Return New PunktGeo(lat, lon)

        Dim dNorm = datum.Trim().ToUpperInvariant()
        If dNorm.Contains("WGS 84") OrElse dNorm.Contains("WGS84") OrElse dNorm.Contains("ETRF") OrElse dNorm.Contains("ETRS") Then
            Return New PunktGeo(lat, lon)
        End If

        Dim a As Double = 6378137.0
        Dim invF As Double = 298.257223563
        Dim dX As Double = 0, dY As Double = 0, dZ As Double = 0

        If dNorm.Contains("BOROWA") Then
            ' Układ Borowa Góra (elipsoida Bessela 1841) -> WGS84 (parametry GUGiK)
            a = 6377397.155
            invF = 299.1528128
            dX = 580.0 : dY = 37.0 : dZ = 476.0
        ElseIf dNorm.Contains("POTSDAM") OrElse dNorm.Contains("DHDN") Then
            ' Potsdam / DHDN (elipsoida Bessela 1841) -> WGS84
            a = 6377397.155
            invF = 299.1528128
            dX = 598.1 : dY = 73.7 : dZ = 418.2
        ElseIf dNorm.Contains("PULKOVO") OrElse dNorm.Contains("S-42") OrElse dNorm.Contains("42") Then
            ' Pulkovo 1942 / Układ 1942 (elipsoida Krassowskiego 1940) -> WGS84
            a = 6378245.0
            invF = 298.3
            dX = 24.0 : dY = -123.0 : dZ = -94.0
        Else
            ' Nieznany datum - brak transformacji
            Return New PunktGeo(lat, lon)
        End If

        ' Konwersja geodezyjna na elipsoidzie źródłowej do geocentrycznych X, Y, Z
        Dim f = 1.0 / invF
        Dim e2 = 2 * f - f * f
        Dim phi = lat * Math.PI / 180.0
        Dim lam = lon * Math.PI / 180.0
        Dim sinPhi = Math.Sin(phi)
        Dim cosPhi = Math.Cos(phi)
        Dim sinLam = Math.Sin(lam)
        Dim cosLam = Math.Cos(lam)

        Dim nVal = a / Math.Sqrt(1.0 - e2 * sinPhi * sinPhi)
        Dim x = nVal * cosPhi * cosLam + dX
        Dim y = nVal * cosPhi * sinLam + dY
        Dim z = nVal * (1.0 - e2) * sinPhi + dZ

        ' Konwersja geocentrycznych X, Y, Z do geodezyjnych na elipsoidzie WGS84
        Dim aWgs = 6378137.0
        Dim fWgs = 1.0 / 298.257223563
        Dim e2Wgs = 2 * fWgs - fWgs * fWgs

        Dim lamOut = Math.Atan2(y, x) * 180.0 / Math.PI
        Dim p = Math.Sqrt(x * x + y * y)
        Dim phiOutRad = Math.Atan2(z, p * (1.0 - e2Wgs))

        For iter = 1 To 4
            Dim sP = Math.Sin(phiOutRad)
            Dim nW = aWgs / Math.Sqrt(1.0 - e2Wgs * sP * sP)
            phiOutRad = Math.Atan2(z + e2Wgs * nW * sP, p)
        Next

        Dim latOut = phiOutRad * 180.0 / Math.PI
        Return New PunktGeo(latOut, lamOut)
    End Function

End Class
