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

''' <summary>Model przekształcenia pikseli obrazu na współrzędne geograficzne.</summary>
Public Enum ModelKalibracji
    ''' <summary>Afiniczny (6 parametrów): przesunięcie, obrót, skala, skos.</summary>
    Afiniczny
    ''' <summary>Rzutowy (8 parametrów): odwzorowuje trapez arkusza (zbiegające się południki).</summary>
    Rzutowy
    ''' <summary>Wielomian 2. stopnia (12 parametrów): także łuki równoleżników arkuszy o dużym zasięgu.</summary>
    Wielomian2
End Enum

''' <summary>
''' Przekształcenie współrzędnych pikselowych obrazu (px, py) na WGS84 (lon, lat).
''' Współczynniki A..F to zawsze dopasowanie afiniczne (lon = A*px + B*py + C, lat = D*px + E*py + F).
''' Dla co najmniej 4 punktów program sprawdza też model rzutowy (4-6 punktów) albo wielomian 2. stopnia (7 i więcej)
''' i wybiera go, gdy wyraźnie zmniejsza błąd - jedno przekształcenie afiniczne dla arkusza 1:1 000 000
''' (stożkowe odwzorowanie, trapez) dawało błędy do 9 km, wielomian - ok. 200 m (2 piksele).
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
    ''' <summary>Model użyty w <see cref="PikselDoGeo"/>.</summary>
    Public Property Model As ModelKalibracji = ModelKalibracji.Afiniczny
    ''' <summary>Błąd RMS samego dopasowania afinicznego (dla porównania z wybranym modelem).</summary>
    Public Property BladRmsAfinicznyMetry As Double

    'normalizacja pikseli (lepsze uwarunkowanie układów równań) i współczynniki modelu nieliniowego
    Private _sx, _sy, _skala As Double
    Private _wLon(), _wLat() As Double

    Public Function PikselDoGeo(px As Double, py As Double) As PunktGeo
        Select Case Model
            Case ModelKalibracji.Rzutowy
                Dim u = (px - _sx) / _skala, v = (py - _sy) / _skala
                Dim m = _wLon(6) * u + _wLon(7) * v + 1.0
                Return New PunktGeo((_wLon(3) * u + _wLon(4) * v + _wLon(5)) / m, (_wLon(0) * u + _wLon(1) * v + _wLon(2)) / m)
            Case ModelKalibracji.Wielomian2
                Dim u = (px - _sx) / _skala, v = (py - _sy) / _skala
                Dim t = {1.0, u, v, u * u, u * v, v * v}
                Dim lon = 0.0, lat = 0.0
                For i = 0 To 5
                    lon += _wLon(i) * t(i)
                    lat += _wLat(i) * t(i)
                Next
                Return New PunktGeo(lat, lon)
            Case Else
                Return New PunktGeo(D * px + E * py + F, A * px + B * py + C)
        End Select
    End Function

    ''' <summary>Piksel dla współrzędnych WGS84; dla modeli nieliniowych - metodą Newtona od rozwiązania afinicznego.</summary>
    Public Function GeoDoPiksel(lat As Double, lon As Double) As Tuple(Of Double, Double)
        Dim det = A * E - B * D
        If Math.Abs(det) < 1.0E-30 Then Return Tuple.Create(0.0, 0.0)
        Dim dLon = lon - C
        Dim dLat = lat - F
        Dim px = (E * dLon - B * dLat) / det
        Dim py = (-D * dLon + A * dLat) / det
        If Model = ModelKalibracji.Afiniczny Then Return Tuple.Create(px, py)

        For iteracja = 1 To 8
            Dim g = PikselDoGeo(px, py)
            Dim gx = PikselDoGeo(px + 1, py)
            Dim gy = PikselDoGeo(px, py + 1)
            Dim j11 = gx.Dlugosc - g.Dlugosc, j12 = gy.Dlugosc - g.Dlugosc
            Dim j21 = gx.Szerokosc - g.Szerokosc, j22 = gy.Szerokosc - g.Szerokosc
            Dim dj = j11 * j22 - j12 * j21
            If Math.Abs(dj) < 1.0E-30 Then Exit For
            Dim rLon = lon - g.Dlugosc, rLat = lat - g.Szerokosc
            Dim krokX = (j22 * rLon - j12 * rLat) / dj
            Dim krokY = (-j21 * rLon + j11 * rLat) / dj
            px += krokX
            py += krokY
            If Math.Abs(krokX) < 0.001 AndAlso Math.Abs(krokY) < 0.001 Then Exit For
        Next
        Return Tuple.Create(px, py)
    End Function

    ''' <summary>
    ''' Wyznacza przekształcenie metodą najmniejszych kwadratów. 2 punkty - skala bez obrotu, 3 - afiniczne,
    ''' 4-6 - afiniczne albo rzutowe, 7 i więcej - afiniczne albo wielomian 2. stopnia (model nieliniowy tylko,
    ''' gdy zmniejsza błąd RMS co najmniej o połowę i o więcej niż 1 m).
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
            If Math.Abs(dpx) < 0.001 OrElse Math.Abs(dpy) < 0.001 Then
                Throw New InvalidDataException("Dwa punkty kalibracyjne leżą w jednej linii obrazu - potrzebne są co najmniej 3 punkty.")
            End If

            t.A = (p2.Wgs84.Dlugosc - p1.Wgs84.Dlugosc) / dpx
            t.B = 0
            t.C = p1.Wgs84.Dlugosc - t.A * p1.Px

            t.D = 0
            t.E = (p2.Wgs84.Szerokosc - p1.Wgs84.Szerokosc) / dpy
            t.F = p1.Wgs84.Szerokosc - t.E * p1.Py
            t.BladRmsMetry = 0.0
            Return t
        End If

        ' Dopasowanie afiniczne (zawsze - także jako przybliżenie startowe odwrotności)
        Dim wiersze As New List(Of Double())
        For Each pt In punkty
            wiersze.Add({pt.Px, pt.Py, 1.0})
        Next
        Dim wLon = NajmniejszeKwadraty(wiersze, punkty, Function(p) p.Wgs84.Dlugosc)
        Dim wLat = NajmniejszeKwadraty(wiersze, punkty, Function(p) p.Wgs84.Szerokosc)
        If wLon Is Nothing OrElse wLat Is Nothing Then
            Throw New InvalidDataException("Punkty kalibracyjne leżą na jednej linii - nie da się wyznaczyć kalibracji mapy.")
        End If
        t.A = wLon(0) : t.B = wLon(1) : t.C = wLon(2)
        t.D = wLat(0) : t.E = wLat(1) : t.F = wLat(2)
        t.BladRmsMetry = Rms(t, punkty)
        t.BladRmsAfinicznyMetry = t.BladRmsMetry

        If punkty.Count >= 4 AndAlso t.BladRmsMetry > 1.0 Then
            Dim kandydat = DopasujNieliniowy(t, punkty)
            If kandydat IsNot Nothing Then
                Dim rmsKandydata = Rms(kandydat, punkty)
                If rmsKandydata < t.BladRmsMetry * 0.5 Then
                    kandydat.BladRmsMetry = rmsKandydata
                    Return kandydat
                End If
            End If
        End If
        Return t
    End Function

    Private Shared Function DopasujNieliniowy(afin As TransformacjaAfiniczna2D, punkty As IList(Of PunktKalibracjiOzi)) As TransformacjaAfiniczna2D
        Dim t As New TransformacjaAfiniczna2D With {
            .A = afin.A, .B = afin.B, .C = afin.C, .D = afin.D, .E = afin.E, .F = afin.F,
            .LiczbaPunktow = afin.LiczbaPunktow, .BladRmsAfinicznyMetry = afin.BladRmsMetry}
        Dim minX = Double.MaxValue, maxX = Double.MinValue, minY = Double.MaxValue, maxY = Double.MinValue
        For Each p In punkty
            minX = Math.Min(minX, p.Px) : maxX = Math.Max(maxX, p.Px)
            minY = Math.Min(minY, p.Py) : maxY = Math.Max(maxY, p.Py)
        Next
        t._sx = (minX + maxX) / 2 : t._sy = (minY + maxY) / 2
        t._skala = Math.Max(1.0, Math.Max(maxX - minX, maxY - minY) / 2)

        If punkty.Count >= 7 Then
            Dim wiersze As New List(Of Double())
            For Each p In punkty
                Dim u = (p.Px - t._sx) / t._skala, v = (p.Py - t._sy) / t._skala
                wiersze.Add({1.0, u, v, u * u, u * v, v * v})
            Next
            t._wLon = NajmniejszeKwadraty(wiersze, punkty, Function(p) p.Wgs84.Dlugosc)
            t._wLat = NajmniejszeKwadraty(wiersze, punkty, Function(p) p.Wgs84.Szerokosc)
            If t._wLon Is Nothing OrElse t._wLat Is Nothing Then Return Nothing
            t.Model = ModelKalibracji.Wielomian2
        Else
            'rzutowy: lon = (a u + b v + c) / (g u + h v + 1), lat = (d u + e v + f) / (g u + h v + 1),
            'liniowo: a u + b v + c - lon g u - lon h v = lon (i analogicznie lat) - 2 równania na punkt, 8 niewiadomych
            Dim m As New List(Of Double())
            Dim prawa As New List(Of Double)
            For Each p In punkty
                Dim u = (p.Px - t._sx) / t._skala, v = (p.Py - t._sy) / t._skala
                Dim lon = p.Wgs84.Dlugosc, lat = p.Wgs84.Szerokosc
                m.Add({u, v, 1, 0, 0, 0, -lon * u, -lon * v}) : prawa.Add(lon)
                m.Add({0, 0, 0, u, v, 1, -lat * u, -lat * v}) : prawa.Add(lat)
            Next
            Dim w = Rozwiaz(m, prawa)
            If w Is Nothing Then Return Nothing
            t._wLon = w
            t.Model = ModelKalibracji.Rzutowy
        End If
        Return t
    End Function

    Private Shared Function NajmniejszeKwadraty(wiersze As List(Of Double()), punkty As IList(Of PunktKalibracjiOzi),
                                                wartosc As Func(Of PunktKalibracjiOzi, Double)) As Double()
        Dim prawa As New List(Of Double)
        For Each p In punkty
            prawa.Add(wartosc(p))
        Next
        Return Rozwiaz(wiersze, prawa)
    End Function

    ''' <summary>Rozwiązanie układu nadokreślonego równaniami normalnymi (eliminacja Gaussa z wyborem elementu); Nothing - układ osobliwy.</summary>
    Private Shared Function Rozwiaz(wiersze As IList(Of Double()), prawa As IList(Of Double)) As Double()
        Dim n = wiersze(0).Length
        If wiersze.Count < n Then Return Nothing
        Dim m(n - 1, n) As Double
        For k = 0 To wiersze.Count - 1
            Dim r = wiersze(k)
            For i = 0 To n - 1
                For j = 0 To n - 1
                    m(i, j) += r(i) * r(j)
                Next
                m(i, n) += r(i) * prawa(k)
            Next
        Next
        Dim skalaMacierzy = 0.0
        For i = 0 To n - 1
            skalaMacierzy = Math.Max(skalaMacierzy, Math.Abs(m(i, i)))
        Next
        For kol = 0 To n - 1
            Dim piv = kol
            For i = kol + 1 To n - 1
                If Math.Abs(m(i, kol)) > Math.Abs(m(piv, kol)) Then piv = i
            Next
            If Math.Abs(m(piv, kol)) <= skalaMacierzy * 1.0E-13 Then Return Nothing
            If piv <> kol Then
                For j = 0 To n
                    Dim tmp = m(kol, j) : m(kol, j) = m(piv, j) : m(piv, j) = tmp
                Next
            End If
            For i = 0 To n - 1
                If i = kol Then Continue For
                Dim f = m(i, kol) / m(kol, kol)
                If f = 0 Then Continue For
                For j = kol To n
                    m(i, j) -= f * m(kol, j)
                Next
            Next
        Next
        Dim w(n - 1) As Double
        For i = 0 To n - 1
            w(i) = m(i, n) / m(i, i)
        Next
        Return w
    End Function

    Private Shared Function Rms(t As TransformacjaAfiniczna2D, punkty As IList(Of PunktKalibracjiOzi)) As Double
        Dim suma As Double = 0, sumaSz As Double = 0
        For Each pt In punkty
            sumaSz += pt.Wgs84.Szerokosc
        Next
        Dim cosFi = Math.Cos(sumaSz / punkty.Count * Math.PI / 180.0)
        For Each pt In punkty
            Dim geo = t.PikselDoGeo(pt.Px, pt.Py)
            Dim dLatM = (geo.Szerokosc - pt.Wgs84.Szerokosc) * 111132.95
            Dim dLonM = (geo.Dlugosc - pt.Wgs84.Dlugosc) * 111412.84 * cosFi
            suma += dLatM * dLatM + dLonM * dLonM
        Next
        Return Math.Round(Math.Sqrt(suma / punkty.Count), 2)
    End Function
End Class

''' <summary>Odczyt i obsługa pliku kalibracyjnego .map OziExplorera.</summary>
Public Class PlikKalibracjiOzi
    Public Property SciezkaMap As String = ""
    Public Property Tytul As String = ""
    Public Property PlikObrazu As String = ""
    Public Property PlikObrazuIstnieje As Boolean
    Public Property Datum As String = "WGS 84"
    ''' <summary>False, gdy program nie zna parametrów datum - współrzędne użyte bez korekty (możliwe przesunięcie do kilkuset metrów).</summary>
    Public Property DatumObslugiwane As Boolean = True
    ''' <summary>Liczba punktów podanych w siatce kilometrowej (UTM / odwzorowanie poprzeczne Merkatora) zamiast w stopniach.</summary>
    Public Property PunktyWSiatce As Integer
    Public Property Punkty As New List(Of PunktKalibracjiOzi)
    Public Property Transformacja As TransformacjaAfiniczna2D
    Public Property ZasiegWgs84 As ProstokatGeo
    Public Property SzerokoscPx As Integer
    Public Property WysokoscPx As Integer
    Public Property RozdzielczoscMetrowNaPiksel As Double

    ''' <summary>Próg błędu RMS kalibracji (w pikselach), powyżej którego okno konwertera ostrzega użytkownika.</summary>
    Public Const ProgOstrzezeniaRmsPx As Double = 3.0

    ''' <summary>Czy błąd dopasowania punktów jest duży w stosunku do rozmiaru piksela (mapa będzie przesunięta).</summary>
    Public ReadOnly Property DuzyBladKalibracji As Boolean
        Get
            If Transformacja Is Nothing Then Return False
            Dim piksel = If(RozdzielczoscMetrowNaPiksel > 0, RozdzielczoscMetrowNaPiksel, 1.0)
            Return Transformacja.BladRmsMetry > Math.Max(5.0, ProgOstrzezeniaRmsPx * piksel)
        End Get
    End Property

    'parametry odwzorowania z pliku (dla punktów w siatce)
    Private _projekcja As String = ""
    Private _ustawieniaProjekcji As Double() = Nothing

    ''' <summary>
    ''' Wczytuje plik .map OziExplorera, wyznacza punkty kalibracyjne i przelicza datum do WGS84.
    ''' </summary>
    Public Shared Function Wczytaj(sciezkaPlikuMap As String, Optional sciezkaObrazu As String = Nothing) As PlikKalibracjiOzi
        If Not File.Exists(sciezkaPlikuMap) Then
            Throw New FileNotFoundException("Nie odnaleziono pliku mapy: " & sciezkaPlikuMap)
        End If

        Dim ozi As New PlikKalibracjiOzi With {.SciezkaMap = sciezkaPlikuMap}
        'pliki .map z OziExplorera zapisywane są w kodowaniu Windows (polskie nazwy - cp1250); poprawny UTF-8 też jest czytany
        Dim linie = WczytajLinie(sciezkaPlikuMap)
        If linie.Length < 5 OrElse Not linie(0).TrimStart().StartsWith("OziExplorer Map Data File", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("Plik nie jest poprawnym plikiem kalibracyjnym OziExplorer (.map).")
        End If

        ozi.Tytul = linie(1).Trim()
        Dim obrazWPliku = linie(2).Trim()

        ' Wyznaczenie ścieżki pliku obrazu
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(sciezkaPlikuMap))
        Dim kandydat As String = Nothing

        If Not String.IsNullOrEmpty(sciezkaObrazu) AndAlso File.Exists(sciezkaObrazu) Then
            kandydat = sciezkaObrazu
        ElseIf obrazWPliku <> "" AndAlso File.Exists(obrazWPliku) Then
            kandydat = obrazWPliku
        Else
            Dim nazwa = Path.GetFileName(obrazWPliku.Replace("\"c, Path.DirectorySeparatorChar))
            Dim sciezkaFolder = If(nazwa = "", "", Path.Combine(folder, nazwa))
            If sciezkaFolder <> "" AndAlso File.Exists(sciezkaFolder) Then
                kandydat = sciezkaFolder
            Else
                Dim rdzenie = New List(Of String) From {Path.GetFileNameWithoutExtension(sciezkaPlikuMap)}
                If nazwa <> "" Then rdzenie.Add(Path.GetFileNameWithoutExtension(nazwa))
                For Each rdzen In rdzenie
                    For Each rozszerzenie In New String() {".jpg", ".png", ".tif", ".tiff", ".bmp", ".jpeg", ".gif"}
                        Dim p = Path.Combine(folder, rdzen & rozszerzenie)
                        If File.Exists(p) Then
                            kandydat = p
                            Exit For
                        End If
                    Next
                    If kandydat IsNot Nothing Then Exit For
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
        Dim czesciDatum = linie(4).Trim().Split(","c)
        ozi.Datum = If(czesciDatum.Length > 0 AndAlso Not String.IsNullOrWhiteSpace(czesciDatum(0)), czesciDatum(0).Trim(), "WGS 84")
        ozi.DatumObslugiwane = ParametryDatum(ozi.Datum) IsNot Nothing

        ' Odwzorowanie i jego parametry (dla punktów podanych w siatce kilometrowej)
        Dim ci = CultureInfo.InvariantCulture
        For Each l In linie
            Dim t = l.Trim()
            If t.StartsWith("Map Projection", StringComparison.OrdinalIgnoreCase) Then
                Dim c = t.Split(","c)
                If c.Length > 1 Then ozi._projekcja = c(1).Trim()
            ElseIf t.StartsWith("Projection Setup", StringComparison.OrdinalIgnoreCase) Then
                Dim c = t.Split(","c)
                Dim w As New List(Of Double)
                For i = 1 To Math.Min(5, c.Length - 1)
                    Dim v As Double
                    w.Add(If(Double.TryParse(c(i).Trim(), NumberStyles.Float, ci, v), v, Double.NaN))
                Next
                If w.Count = 5 Then ozi._ustawieniaProjekcji = w.ToArray()
            ElseIf (ozi.SzerokoscPx <= 0 OrElse ozi.WysokoscPx <= 0) AndAlso t.StartsWith("IWH", StringComparison.OrdinalIgnoreCase) Then
                'IWH,Map Image Width/Height,szer,wys - wymiary obrazu zapisane przez OziExplorer
                Dim c = t.Split(","c)
                Dim szer, wys As Integer
                If c.Length >= 4 AndAlso Integer.TryParse(c(2).Trim(), szer) AndAlso Integer.TryParse(c(3).Trim(), wys) Then
                    ozi.SzerokoscPx = szer : ozi.WysokoscPx = wys
                End If
            End If
        Next

        ' Odczyt punktów kalibracyjnych
        For i = 0 To linie.Length - 1
            Dim l = linie(i).Trim()
            If l.StartsWith("Point", StringComparison.OrdinalIgnoreCase) Then
                Dim pt = SparsujPunkt(l, ozi.Datum)
                If pt Is Nothing Then
                    pt = ozi.SparsujPunktSiatki(l)
                    If pt IsNot Nothing Then ozi.PunktyWSiatce += 1
                End If
                If pt IsNot Nothing Then ozi.Punkty.Add(pt)
            End If
        Next

        If ozi.Punkty.Count < 2 Then
            If ozi._siatkaNieobslugiwana Then
                Throw New InvalidDataException("Punkty kalibracyjne pliku .map są podane w siatce kilometrowej odwzorowania """ & ozi._projekcja &
                                               """, którego program nie obsługuje (obsługiwane: UTM i poprzeczne Merkatora). " &
                                               "Skalibruj mapę w OziExplorerze punktami w stopniach (Lat/Lon).")
            End If
            Throw New InvalidDataException("Plik .map zawiera mniej niż 2 aktywne punkty kalibracyjne.")
        End If

        ozi.Transformacja = TransformacjaAfiniczna2D.Dopasuj(ozi.Punkty)

        ' Zasięg WGS84 (próbkowanie krawędzi obrazu - modele nieliniowe wyginają krawędzie) i rozdzielczość
        If ozi.SzerokoscPx > 0 AndAlso ozi.WysokoscPx > 0 Then
            Dim w = CDbl(ozi.SzerokoscPx)
            Dim h = CDbl(ozi.WysokoscPx)
            Dim minLat = Double.MaxValue, maxLat = Double.MinValue, minLon = Double.MaxValue, maxLon = Double.MinValue
            For k = 0 To 16
                Dim f = k / 16.0
                For Each p In {ozi.Transformacja.PikselDoGeo(f * w, 0), ozi.Transformacja.PikselDoGeo(f * w, h),
                               ozi.Transformacja.PikselDoGeo(0, f * h), ozi.Transformacja.PikselDoGeo(w, f * h)}
                    minLat = Math.Min(minLat, p.Szerokosc) : maxLat = Math.Max(maxLat, p.Szerokosc)
                    minLon = Math.Min(minLon, p.Dlugosc) : maxLon = Math.Max(maxLon, p.Dlugosc)
                Next
            Next
            ozi.ZasiegWgs84 = New ProstokatGeo(maxLat, minLat, maxLon, minLon)

            'rozdzielczość w środku obrazu (przekątna obróconej mapy zawyżała wynik liczony z prostokąta zasięgu)
            Dim s0 = ozi.Transformacja.PikselDoGeo(w / 2, h / 2)
            Dim sx = ozi.Transformacja.PikselDoGeo(w / 2 + 100, h / 2)
            Dim sy = ozi.Transformacja.PikselDoGeo(w / 2, h / 2 + 100)
            Dim mx = Georeferencja.OdlegloscMetry(s0, sx) / 100.0
            Dim my = Georeferencja.OdlegloscMetry(s0, sy) / 100.0
            ozi.RozdzielczoscMetrowNaPiksel = Math.Round((mx + my) / 2.0, 2)
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

    Private Shared Function WczytajLinie(sciezka As String) As String()
        Dim bajty = File.ReadAllBytes(sciezka)
        Dim tekst As String
        Try
            tekst = New System.Text.UTF8Encoding(False, True).GetString(bajty)
        Catch ex As System.Text.DecoderFallbackException
            Try
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
                tekst = System.Text.Encoding.GetEncoding(1250).GetString(bajty)
            Catch
                tekst = System.Text.Encoding.GetEncoding(28591).GetString(bajty)
            End Try
        End Try
        If tekst.Length > 0 AndAlso tekst(0) = ChrW(&HFEFF) Then tekst = tekst.Substring(1)
        Return tekst.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
    End Function

    ''' <summary>Parsuje pojedynczy wiersz punktu kalibracyjnego PointXX z pliku .map (współrzędne w stopniach).</summary>
    ''' <returns>Nothing, gdy punkt jest pusty albo nie ma współrzędnych w stopniach (np. tylko w siatce).</returns>
    Public Shared Function SparsujPunkt(wiersz As String, datum As String) As PunktKalibracjiOzi
        Dim ci = CultureInfo.InvariantCulture
        Dim czesci = wiersz.Split(","c)
        If czesci.Length < 11 Then Return Nothing

        Dim nazwa = czesci(0).Trim()
        Dim px As Double, py As Double
        If Not Double.TryParse(czesci(2).Trim(), NumberStyles.Float, ci, px) OrElse
           Not Double.TryParse(czesci(3).Trim(), NumberStyles.Float, ci, py) Then
            Return Nothing
        End If

        ' Szukamy indeksów N/S oraz E/W
        Dim idxNS As Integer = -1
        Dim idxEW As Integer = -1
        For i = 4 To czesci.Length - 1
            Dim t = czesci(i).Trim().ToUpperInvariant()
            If (t = "N" OrElse t = "S") AndAlso idxNS < 0 Then
                idxNS = i
            ElseIf (t = "E" OrElse t = "W") AndAlso idxEW < 0 AndAlso idxNS >= 0 Then
                idxEW = i
            End If
        Next

        If idxNS < 0 OrElse idxEW < 0 Then Return Nothing

        'punkt bez stopni (pola puste - kalibracja w siatce) nie może trafić do kalibracji jako 0°/0°
        Dim lat = WyznaczWspolrzedna(czesci, 4, idxNS - 1, czesci(idxNS).Trim().ToUpperInvariant() = "S")
        Dim lon = WyznaczWspolrzedna(czesci, idxNS + 1, idxEW - 1, czesci(idxEW).Trim().ToUpperInvariant() = "W")
        If Double.IsNaN(lat) OrElse Double.IsNaN(lon) Then Return Nothing

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

    Private _siatkaNieobslugiwana As Boolean

    ''' <summary>
    ''' Punkt podany w siatce kilometrowej: "..., grid, strefa, wschodnia, północna, N/S". Obsługiwane odwzorowania:
    ''' UTM (strefa z pola punktu) i poprzeczne Merkatora z wiersza "Projection Setup" (szerokość i długość początku,
    ''' skala, przesunięcia). Współrzędne liczone na elipsoidzie datum mapy, potem przeliczane do WGS84.
    ''' </summary>
    Private Function SparsujPunktSiatki(wiersz As String) As PunktKalibracjiOzi
        Dim ci = CultureInfo.InvariantCulture
        Dim czesci = wiersz.Split(","c)
        Dim idxGrid = -1
        For i = 4 To czesci.Length - 1
            If czesci(i).Trim().Equals("grid", StringComparison.OrdinalIgnoreCase) Then idxGrid = i : Exit For
        Next
        If idxGrid < 0 OrElse czesci.Length < idxGrid + 4 Then Return Nothing
        Dim px, py, wsch, poln As Double
        If Not Double.TryParse(czesci(2).Trim(), NumberStyles.Float, ci, px) OrElse
           Not Double.TryParse(czesci(3).Trim(), NumberStyles.Float, ci, py) OrElse
           Not Double.TryParse(czesci(idxGrid + 2).Trim(), NumberStyles.Float, ci, wsch) OrElse
           Not Double.TryParse(czesci(idxGrid + 3).Trim(), NumberStyles.Float, ci, poln) Then
            Return Nothing
        End If
        Dim strefaTekst = czesci(idxGrid + 1).Trim()
        Dim poludniowa = czesci.Length > idxGrid + 4 AndAlso czesci(idxGrid + 4).Trim().ToUpperInvariant() = "S"

        Dim lat0, lon0, k0, fe, fn As Double
        Dim proj = _projekcja.ToUpperInvariant()
        Dim strefa As Integer
        If proj.Contains("UTM") OrElse proj.Contains("UNIVERSAL TRANSVERSE") Then
            If Not Integer.TryParse(New String(strefaTekst.TakeWhile(Function(c) Char.IsDigit(c)).ToArray()), strefa) OrElse strefa < 1 OrElse strefa > 60 Then
                _siatkaNieobslugiwana = True
                Return Nothing
            End If
            lat0 = 0 : lon0 = strefa * 6 - 183 : k0 = 0.9996 : fe = 500000 : fn = If(poludniowa, 10000000, 0)
        ElseIf proj.Contains("TRANSVERSE MERCATOR") AndAlso _ustawieniaProjekcji IsNot Nothing AndAlso
               Not Double.IsNaN(_ustawieniaProjekcji(0)) AndAlso Not Double.IsNaN(_ustawieniaProjekcji(1)) Then
            lat0 = _ustawieniaProjekcji(0) : lon0 = _ustawieniaProjekcji(1)
            k0 = If(Double.IsNaN(_ustawieniaProjekcji(2)) OrElse _ustawieniaProjekcji(2) <= 0, 1.0, _ustawieniaProjekcji(2))
            fe = If(Double.IsNaN(_ustawieniaProjekcji(3)), 0, _ustawieniaProjekcji(3))
            fn = If(Double.IsNaN(_ustawieniaProjekcji(4)), 0, _ustawieniaProjekcji(4))
        Else
            _siatkaNieobslugiwana = True
            Return Nothing
        End If

        Dim elipsoida = ParametryDatum(Datum)
        Dim a = If(elipsoida Is Nothing, 6378137.0, elipsoida(0))
        Dim invF = If(elipsoida Is Nothing, 298.257223563, elipsoida(1))
        Dim geo = OdwrotnyMerkatorPoprzeczny(wsch, poln, a, invF, lat0, lon0, k0, fe, fn)
        Return New PunktKalibracjiOzi With {
            .Nazwa = czesci(0).Trim(), .Px = px, .Py = py,
            .ZrodloweLat = geo.Szerokosc, .ZrodloweLon = geo.Dlugosc,
            .Wgs84 = PrzeliczDatumDoWgs84(geo.Szerokosc, geo.Dlugosc, Datum)
        }
    End Function

    ''' <summary>Odwrotne odwzorowanie poprzeczne Merkatora na dowolnej elipsoidzie (wzory Snydera, dokładność poniżej 1 m w strefie).</summary>
    Public Shared Function OdwrotnyMerkatorPoprzeczny(wschodnia As Double, polnocna As Double, a As Double, invF As Double,
                                                      lat0 As Double, lon0 As Double, k0 As Double, fe As Double, fn As Double) As PunktGeo
        Dim f = 1.0 / invF
        Dim e2 = 2 * f - f * f
        Dim ep2 = e2 / (1 - e2)
        Dim fi0 = lat0 * Math.PI / 180.0
        Dim m0 = LukPoludnika(fi0, a, e2)
        Dim m = m0 + (polnocna - fn) / k0
        Dim mu = m / (a * (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256))
        Dim e1 = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2))
        Dim fi1 = mu + (3 * e1 / 2 - 27 * e1 ^ 3 / 32) * Math.Sin(2 * mu) + (21 * e1 * e1 / 16 - 55 * e1 ^ 4 / 32) * Math.Sin(4 * mu) +
                  (151 * e1 ^ 3 / 96) * Math.Sin(6 * mu) + (1097 * e1 ^ 4 / 512) * Math.Sin(8 * mu)
        Dim sin1 = Math.Sin(fi1), cos1 = Math.Cos(fi1), tan1 = Math.Tan(fi1)
        Dim c1 = ep2 * cos1 * cos1
        Dim t1 = tan1 * tan1
        Dim n1 = a / Math.Sqrt(1 - e2 * sin1 * sin1)
        Dim r1 = a * (1 - e2) / Math.Pow(1 - e2 * sin1 * sin1, 1.5)
        Dim d = (wschodnia - fe) / (n1 * k0)
        Dim fi = fi1 - (n1 * tan1 / r1) * (d * d / 2 - (5 + 3 * t1 + 10 * c1 - 4 * c1 * c1 - 9 * ep2) * d ^ 4 / 24 +
                                            (61 + 90 * t1 + 298 * c1 + 45 * t1 * t1 - 252 * ep2 - 3 * c1 * c1) * d ^ 6 / 720)
        Dim lam = (d - (1 + 2 * t1 + c1) * d ^ 3 / 6 + (5 - 2 * c1 + 28 * t1 - 3 * c1 * c1 + 8 * ep2 + 24 * t1 * t1) * d ^ 5 / 120) / cos1
        Return New PunktGeo(fi * 180.0 / Math.PI, lon0 + lam * 180.0 / Math.PI)
    End Function

    Private Shared Function LukPoludnika(fi As Double, a As Double, e2 As Double) As Double
        Return a * ((1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 ^ 3 / 256) * fi - (3 * e2 / 8 + 3 * e2 * e2 / 32 + 45 * e2 ^ 3 / 1024) * Math.Sin(2 * fi) +
                    (15 * e2 * e2 / 256 + 45 * e2 ^ 3 / 1024) * Math.Sin(4 * fi) - (35 * e2 ^ 3 / 3072) * Math.Sin(6 * fi))
    End Function

    ''' <summary>Stopnie (i ewentualnie minuty, sekundy) z kolejnych pól; NaN, gdy pola są puste.</summary>
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

        If liczby.Count = 0 Then Return Double.NaN
        Dim wynik As Double = liczby(0)
        If liczby.Count >= 2 Then wynik += liczby(1) / 60.0
        If liczby.Count >= 3 Then wynik += liczby(2) / 3600.0
        Return If(ujemna, -wynik, wynik)
    End Function

    ''' <summary>
    ''' Parametry datum: półoś wielka, odwrotność spłaszczenia i przesunięcia dX, dY, dZ do WGS84;
    ''' Nothing - datum nieznane programowi.
    ''' </summary>
    Public Shared Function ParametryDatum(datum As String) As Double()
        If String.IsNullOrWhiteSpace(datum) Then Return {6378137.0, 298.257223563, 0, 0, 0}
        Dim dNorm = datum.Trim().ToUpperInvariant()
        If dNorm.Contains("WGS 84") OrElse dNorm.Contains("WGS84") OrElse dNorm.Contains("ETRF") OrElse dNorm.Contains("ETRS") Then
            Return {6378137.0, 298.257223563, 0, 0, 0}
        End If
        If dNorm.Contains("BOROWA") Then
            ' Układ Borowa Góra (elipsoida Bessela 1841) -> WGS84
            Return {6377397.155, 299.1528128, 580.0, 37.0, 476.0}
        End If
        If dNorm.Contains("POTSDAM") OrElse dNorm.Contains("DHDN") Then
            ' Potsdam / DHDN (elipsoida Bessela 1841) -> WGS84 (EPSG:1777)
            Return {6377397.155, 299.1528128, 598.1, 73.7, 418.2}
        End If
        If dNorm.Contains("PULKOVO") OrElse dNorm.Contains("S-42") OrElse dNorm.Contains("1942") Then
            ' Pulkovo 1942 / Układ 1942 (elipsoida Krassowskiego 1940) -> WGS84
            Return {6378245.0, 298.3, 24.0, -123.0, -94.0}
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Przelicza współrzędne geograficzne z zadanego Datum (np. Borowa Góra, Potsdam, Pulkovo 1942) do WGS84
    ''' przy użyciu standardowej 3-parametrowej transformacji geocentrycznej. Datum nieznane - bez zmian.
    ''' </summary>
    Public Shared Function PrzeliczDatumDoWgs84(lat As Double, lon As Double, datum As String) As PunktGeo
        Dim p = ParametryDatum(datum)
        If p Is Nothing OrElse (p(2) = 0 AndAlso p(3) = 0 AndAlso p(4) = 0) Then Return New PunktGeo(lat, lon)

        Dim a = p(0), invF = p(1), dX = p(2), dY = p(3), dZ = p(4)

        ' Konwersja geodezyjna na elipsoidzie źródłowej do geocentrycznych X, Y, Z
        Dim f = 1.0 / invF
        Dim e2 = 2 * f - f * f
        Dim phi = lat * Math.PI / 180.0
        Dim lam = lon * Math.PI / 180.0
        Dim sinPhi = Math.Sin(phi)
        Dim cosPhi = Math.Cos(phi)

        Dim nVal = a / Math.Sqrt(1.0 - e2 * sinPhi * sinPhi)
        Dim x = nVal * cosPhi * Math.Cos(lam) + dX
        Dim y = nVal * cosPhi * Math.Sin(lam) + dY
        Dim z = nVal * (1.0 - e2) * sinPhi + dZ

        ' Konwersja geocentrycznych X, Y, Z do geodezyjnych na elipsoidzie WGS84
        Dim aWgs = 6378137.0
        Dim fWgs = 1.0 / 298.257223563
        Dim e2Wgs = 2 * fWgs - fWgs * fWgs

        Dim lamOut = Math.Atan2(y, x) * 180.0 / Math.PI
        Dim pp = Math.Sqrt(x * x + y * y)
        Dim phiOutRad = Math.Atan2(z, pp * (1.0 - e2Wgs))
        For iter = 1 To 5
            Dim sP = Math.Sin(phiOutRad)
            Dim nW = aWgs / Math.Sqrt(1.0 - e2Wgs * sP * sP)
            phiOutRad = Math.Atan2(z + e2Wgs * nW * sP, pp)
        Next

        Return New PunktGeo(phiOutRad * 180.0 / Math.PI, lamOut)
    End Function

End Class
