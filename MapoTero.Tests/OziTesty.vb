'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.IO
Imports System.IO.Compression
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

<Trait("Kategoria", "Wolne")>
Public Class OziTesty

    <Fact>
    Public Sub TransformacjaAfinicznaPodstawowa()
        ' Test siatki 1000x1000 px:
        ' (0, 0) -> (50.0 N, 20.0 E)
        ' (1000, 0) -> (50.0 N, 21.0 E)
        ' (1000, 1000) -> (49.0 N, 21.0 E)
        ' (0, 1000) -> (49.0 N, 20.0 E)
        Dim punkty As New List(Of PunktKalibracjiOzi) From {
            New PunktKalibracjiOzi With {.Px = 0, .Py = 0, .Wgs84 = New PunktGeo(50.0, 20.0)},
            New PunktKalibracjiOzi With {.Px = 1000, .Py = 0, .Wgs84 = New PunktGeo(50.0, 21.0)},
            New PunktKalibracjiOzi With {.Px = 1000, .Py = 1000, .Wgs84 = New PunktGeo(49.0, 21.0)},
            New PunktKalibracjiOzi With {.Px = 0, .Py = 1000, .Wgs84 = New PunktGeo(49.0, 20.0)}
        }

        Dim t = TransformacjaAfiniczna2D.Dopasuj(punkty)

        Assert.Equal(4, t.LiczbaPunktow)
        Assert.True(t.BladRmsMetry < 1.0)

        ' Środek (500, 500) powinien wypaść dokładnie w (49.5, 20.5)
        Dim srodek = t.PikselDoGeo(500, 500)
        Assert.InRange(srodek.Szerokosc, 49.4999, 49.5001)
        Assert.InRange(srodek.Dlugosc, 20.4999, 20.5001)

        ' Odwrócenie (Geo do Piksel)
        Dim pxPy = t.GeoDoPiksel(49.5, 20.5)
        Assert.InRange(pxPy.Item1, 499.9, 500.1)
        Assert.InRange(pxPy.Item2, 499.9, 500.1)
    End Sub

    <Fact>
    Public Sub SparsujWierszPunktuOzi()
        Dim wiersz = "Point01,xy,  120,  240,in, deg,  49, 15.3000,N,  19, 58.4500,E, grid,   ,           ,           ,N"
        Dim pt = PlikKalibracjiOzi.SparsujPunkt(wiersz, "WGS 84")

        Assert.NotNull(pt)
        Assert.Equal("Point01", pt.Nazwa)
        Assert.Equal(120.0, pt.Px)
        Assert.Equal(240.0, pt.Py)

        ' 49 st 15.3 min = 49 + 15.3 / 60 = 49.255
        Assert.InRange(pt.Wgs84.Szerokosc, 49.2549, 49.2551)
        ' 19 st 58.45 min = 19 + 58.45 / 60 = 19.974166...
        Assert.InRange(pt.Wgs84.Dlugosc, 19.9741, 19.9743)
    End Sub

    <Theory>
    <InlineData(52.0004827, 21.0024869, 52.0, 21.0)>
    <InlineData(49.3001834, 19.9022046, 49.3, 19.9)>
    <InlineData(54.3008069, 18.6022721, 54.3, 18.6)>
    <InlineData(50.8004434, 15.0016048, 50.8, 15.0)>
    Public Sub BorowaGoraZgodnaZPrzesunieciamiIgik(latBg As Double, lonBg As Double, latWgs As Double, lonWgs As Double)
        'punkty kontrolne: WGS84 -> BG wg parametrów wyznaczonych z przesunięć BG -> "1942" (Cisak, Sas 2004) w PROJ
        Dim wgs = PlikKalibracjiOzi.PrzeliczDatumDoWgs84(latBg, lonBg, "Borowa Gora")
        Dim dN = (wgs.Szerokosc - latWgs) * 111133.0
        Dim dE = (wgs.Dlugosc - lonWgs) * 111413.0 * Math.Cos(latWgs * Math.PI / 180)
        Assert.True(Math.Sqrt(dN * dN + dE * dE) < 1.0)
    End Sub

    <Fact>
    Public Sub PrzeliczDatumBorowaGoraDoWgs84()
        ' Punkt w Tatrach: 49.2, 19.9
        Dim wgs = PlikKalibracjiOzi.PrzeliczDatumDoWgs84(49.2, 19.9, "Borowa Gora")

        ' Przesunięcie Borowa Góra -> WGS84 wynosi zwykle ok. 100-150 m
        Assert.NotEqual(49.2, wgs.Szerokosc)
        Assert.NotEqual(19.9, wgs.Dlugosc)

        ' Odległość w terenie rzędu 100-200 m (dLat ~45m, dLon ~160m)
        Dim dLat = Math.Abs(wgs.Szerokosc - 49.2)
        Dim dLon = Math.Abs(wgs.Dlugosc - 19.9)
        Dim dM = Math.Sqrt((dLat * 111132.0) ^ 2 + (dLon * 72700.0) ^ 2)
        Assert.True(dM > 100.0 AndAlso dM < 250.0)
    End Sub

    <Fact>
    Public Sub WczytajPelnyPlikMap()
        Dim tmpPlik = Path.GetTempFileName() & ".map"
        Try
            Dim linie = New String() {
                "OziExplorer Map Data File Version 2.2",
                "Tatry Testowa Mapa",
                "Tatry.jpg",
                "1 ,Map Code,",
                "WGS 84,,   0.0000,   0.0000,WGS 84",
                "Reserved 1",
                "Reserved 2",
                "Magnetic Variation,,,E",
                "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
                "Point01,xy,    0,    0,in, deg,  49, 30.0000,N,  19, 45.0000,E, grid,   ,           ,           ,N",
                "Point02,xy, 2000,    0,in, deg,  49, 30.0000,N,  20, 15.0000,E, grid,   ,           ,           ,N",
                "Point03,xy, 2000, 1500,in, deg,  49, 10.0000,N,  20, 15.0000,E, grid,   ,           ,           ,N",
                "Point04,xy,    0, 1500,in, deg,  49, 10.0000,N,  19, 45.0000,E, grid,   ,           ,           ,N"
            }
            File.WriteAllLines(tmpPlik, linie)

            Dim ozi = PlikKalibracjiOzi.Wczytaj(tmpPlik)

            Assert.Equal("Tatry Testowa Mapa", ozi.Tytul)
            Assert.Equal("WGS 84", ozi.Datum)
            Assert.Equal(4, ozi.Punkty.Count)
            Assert.NotNull(ozi.Transformacja)
            Assert.True(ozi.Transformacja.BladRmsMetry < 1.0)
            Assert.True(ozi.ZasiegWgs84.Polnoc > ozi.ZasiegWgs84.Poludnie)
            Assert.True(ozi.ZasiegWgs84.Wschod > ozi.ZasiegWgs84.Zachod)
        Finally
            If File.Exists(tmpPlik) Then File.Delete(tmpPlik)
        End Try
    End Sub

    <Fact>
    <Trait("Kategoria", "Siec")>
    Public Async Function KonwertujPobranaMapeOziDoKmzIJnxAsync() As Task
        Dim folderTestu = Path.Combine(Path.GetTempPath(), "MapoTero_OziTest_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folderTestu)

        Dim plikObrazu = Path.Combine(folderTestu, "malopolska_topo.png")
        Dim plikMap = Path.Combine(folderTestu, "malopolska_topo.map")
        Dim plikKmz = Path.Combine(folderTestu, "malopolska_topo.kmz")
        Dim plikJnx = Path.Combine(folderTestu, "malopolska_topo.jnx")
        Dim plikMbtiles = Path.Combine(folderTestu, "malopolska_topo.mbtiles")

        ' 1. Pobranie realnego kafelka topograficznego z internetu
        Using http As New HttpClient()
            http.DefaultRequestHeaders.UserAgent.ParseAdd("MapoTero/3.17 (Test; https://mapotero.pl)")
            Dim bajty = Await http.GetByteArrayAsync("https://tile.opentopomap.org/14/9070/5688.png")
            Assert.True(bajty.Length > 5000)
            File.WriteAllBytes(plikObrazu, bajty)
        End Using

        ' 2. Utworzenie autentycznego pliku kalibracji .map OziExplorer
        ' Współrzędne kafelka 14/9070/5688:
        ' N: 50.0641917 -> 50 st 03.8515 min
        ' S: 50.0500847 -> 50 st 03.0051 min
        ' W: 19.3359375 -> 19 st 20.1562 min
        ' E: 19.3579102 -> 19 st 21.4746 min
        Dim linieMap = New String() {
            "OziExplorer Map Data File Version 2.2",
            "Malopolska OpenTopoMap",
            Path.GetFileName(plikObrazu),
            "1 ,Map Code,",
            "WGS 84,,   0.0000,   0.0000,WGS 84",
            "Reserved 1",
            "Reserved 2",
            "Magnetic Variation,,,E",
            "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
            "Point01,xy,    0,    0,in, deg,  50, 03.8515,N,  19, 20.1562,E, grid,   ,           ,           ,N",
            "Point02,xy,  256,    0,in, deg,  50, 03.8515,N,  19, 21.4746,E, grid,   ,           ,           ,N",
            "Point03,xy,  256,  256,in, deg,  50, 03.0051,N,  19, 21.4746,E, grid,   ,           ,           ,N",
            "Point04,xy,    0,  256,in, deg,  50, 03.0051,N,  19, 20.1562,E, grid,   ,           ,           ,N"
        }
        File.WriteAllLines(plikMap, linieMap)

        ' 3. Wczytanie i weryfikacja kalibracji
        Dim ozi = PlikKalibracjiOzi.Wczytaj(plikMap)
        Assert.NotNull(ozi)
        Assert.True(ozi.PlikObrazuIstnieje)
        Assert.Equal(256, ozi.SzerokoscPx)
        Assert.Equal(256, ozi.WysokoscPx)
        Assert.True(ozi.Transformacja.BladRmsMetry < 2.0)

        ' 4. Konwersja do Garmin KMZ
        FormImportOzi.KonwertujDoKmz(ozi, plikKmz, 50, 85, False, Nothing, CancellationToken.None)
        Assert.True(File.Exists(plikKmz))
        Assert.True(New FileInfo(plikKmz).Length > 1000)

        ' Ekstrakcja rastra z utworzonego KMZ do weryfikacji wizualnej
        Dim sciezkaZKmz = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\ozi_wynik_z_kmz.jpg"
        Using zip = ZipFile.OpenRead(plikKmz)
            Dim entry = zip.Entries.FirstOrDefault(Function(e) e.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            Assert.NotNull(entry)
            entry.ExtractToFile(sciezkaZKmz, True)
        End Using
        Assert.True(File.Exists(sciezkaZKmz))
        Assert.True(New FileInfo(sciezkaZKmz).Length > 1000)

        ' 5. Konwersja do Garmin JNX
        FormImportOzi.KonwertujDoJnx(ozi, plikJnx, 30, 85, Nothing, CancellationToken.None)
        Assert.True(File.Exists(plikJnx))
        Assert.True(New FileInfo(plikJnx).Length > 2000)

        ' Weryfikacja struktury nagłówka JNX i dokładna ekstrakcja kafelków JPEG
        Dim sciezkaZJnx = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\ozi_wynik_z_jnx.jpg"
        Using fs As New FileStream(plikJnx, FileMode.Open, FileAccess.Read), br As New BinaryReader(fs)
            Assert.Equal(4, br.ReadInt32()) ' Wersja JNX = 4
            fs.Seek(24, SeekOrigin.Begin)
            Dim liczbaPoziomow = br.ReadInt32()
            Assert.True(liczbaPoziomow >= 2, "piramida poziomów JNX")
            ' ostatni poziom - najbardziej szczegółowy (poziomy zapisane od przeglądowego)
            fs.Seek(52 + 17L * (liczbaPoziomow - 1), SeekOrigin.Begin)
            Dim ileKafli = br.ReadInt32()
            Dim offsetOpisu = br.ReadUInt32()
            Assert.True(ileKafli > 0)

            Dim maxKafelBytes As Byte() = Nothing
            Dim maxLen = 0

            For k = 0 To ileKafli - 1
                fs.Seek(offsetOpisu + CLng(k) * 28 + 16, SeekOrigin.Begin)
                Dim szer = br.ReadInt16()
                Dim wys = br.ReadInt16()
                Dim dlugoscJpeg = br.ReadInt32()
                Dim offsetJpeg = br.ReadUInt32()
                Assert.Equal(256, CInt(szer))
                Assert.Equal(256, CInt(wys))

                fs.Seek(offsetJpeg, SeekOrigin.Begin)
                Dim bajtyKaf = br.ReadBytes(dlugoscJpeg)
                If dlugoscJpeg > maxLen Then
                    maxLen = dlugoscJpeg
                    maxKafelBytes = bajtyKaf
                End If
            Next

            Assert.NotNull(maxKafelBytes)
            Assert.True(maxLen > 10000, $"Największy kafel w JNX ma {maxLen} bajtów (oczekiwano > 10000)")
            ' JNX przechowuje JPEG bez znacznika SOI (FF D8) - czytnik dopisuje go przed dekodowaniem
            Assert.False(maxKafelBytes(0) = &HFF AndAlso maxKafelBytes(1) = &HD8)
            File.WriteAllBytes(sciezkaZJnx, {CByte(&HFF), CByte(&HD8)}.Concat(maxKafelBytes).ToArray())
        End Using
        Assert.True(File.Exists(sciezkaZJnx))

        ' 6. Konwersja do MBTiles
        FormImportOzi.KonwertujDoMbtiles(ozi, plikMbtiles, 85, Nothing, CancellationToken.None)
        Assert.True(File.Exists(plikMbtiles))
        Assert.True(New FileInfo(plikMbtiles).Length > 10000)

        ' Sprzątanie folderu tymczasowego
        Try
            Directory.Delete(folderTestu, True)
        Catch
        End Try
    End Function

    <Fact>
    Public Sub TestWyszukiwaniaPlikowMapWFolderze()
        Dim folderTestu = Path.Combine(Path.GetTempPath(), "MapoTero_OziFind_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folderTestu)
        Dim podfolder = Path.Combine(folderTestu, "Podfolder")
        Directory.CreateDirectory(podfolder)

        Try
            ' 1. Pusty folder -> pusta lista
            Dim brak = FormImportOzi.ZnajdzPlikiMapWFolderze(folderTestu)
            Assert.Empty(brak)

            ' 2. Pliki w podfolderze (brak w głównym) -> powinno znaleźć w podfolderze
            File.WriteAllText(Path.Combine(podfolder, "mapa1.map"), "OziExplorer Map Data File Version 2.2")
            File.WriteAllText(Path.Combine(podfolder, "mapa2.map"), "OziExplorer Map Data File Version 2.2")
            'mapa źródeł JavaScript z tym samym rozszerzeniem nie jest kalibracją mapy
            File.WriteAllText(Path.Combine(podfolder, "skrypt.js.map"), "{""version"":3,""file"":""skrypt.js""}")
            File.WriteAllText(Path.Combine(podfolder, "obraz.jpg"), "test")
            Dim wPodfolderze = FormImportOzi.ZnajdzPlikiMapWFolderze(folderTestu)
            Assert.Equal(2, wPodfolderze.Count)

            ' 3. Pliki w folderze głównym -> powinno preferować folder główny
            File.WriteAllText(Path.Combine(folderTestu, "glowna.map"), "OziExplorer Map Data File Version 2.2")
            Dim wGlownym = FormImportOzi.ZnajdzPlikiMapWFolderze(folderTestu)
            Assert.Single(wGlownym)
            Assert.EndsWith("glowna.map", wGlownym(0))
        Finally
            If Directory.Exists(folderTestu) Then Directory.Delete(folderTestu, True)
        End Try
    End Sub

    <Fact>
    Public Sub TestKonwersjiWsadowejOzi()
        Dim folderZrodlowy = Path.Combine(Path.GetTempPath(), "MapoTero_BatchSrc_" & Guid.NewGuid().ToString("N"))
        Dim folderDocelowy = Path.Combine(Path.GetTempPath(), "MapoTero_BatchDst_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folderZrodlowy)
        Directory.CreateDirectory(folderDocelowy)

        Try
            ' Utwórz małą bitmapę testową (64x64)
            Dim sciezkaBmp1 = Path.Combine(folderZrodlowy, "arkusz_a.jpg")
            Dim sciezkaBmp2 = Path.Combine(folderZrodlowy, "arkusz_b.jpg")
            Using bmp As New System.Drawing.Bitmap(64, 64)
                Using g = System.Drawing.Graphics.FromImage(bmp)
                    g.Clear(System.Drawing.Color.Green)
                End Using
                bmp.Save(sciezkaBmp1, System.Drawing.Imaging.ImageFormat.Jpeg)
                bmp.Save(sciezkaBmp2, System.Drawing.Imaging.ImageFormat.Jpeg)
            End Using

            ' Utwórz 2 poprawne pliki .map i 1 plik z brakującym obrazem
            Dim linie1 = {
                "OziExplorer Map Data File Version 2.2", "Arkusz A", "arkusz_a.jpg", "1 ,Map Code,", "WGS 84,, 0, 0,WGS 84",
                "Reserved 1", "Reserved 2", "Magnetic Variation,,,E", "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
                "Point01,xy, 0, 0,in, deg, 50, 00.00,N, 20, 00.00,E, grid,,,,N",
                "Point02,xy, 64, 0,in, deg, 50, 00.00,N, 20, 01.00,E, grid,,,,N",
                "Point03,xy, 64, 64,in, deg, 49, 59.00,N, 20, 01.00,E, grid,,,,N",
                "Point04,xy, 0, 64,in, deg, 49, 59.00,N, 20, 00.00,E, grid,,,,N"
            }
            Dim linie2 = {
                "OziExplorer Map Data File Version 2.2", "Arkusz B", "arkusz_b.jpg", "1 ,Map Code,", "WGS 84,, 0, 0,WGS 84",
                "Reserved 1", "Reserved 2", "Magnetic Variation,,,E", "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
                "Point01,xy, 0, 0,in, deg, 50, 01.00,N, 20, 00.00,E, grid,,,,N",
                "Point02,xy, 64, 0,in, deg, 50, 01.00,N, 20, 01.00,E, grid,,,,N",
                "Point03,xy, 64, 64,in, deg, 50, 00.00,N, 20, 01.00,E, grid,,,,N",
                "Point04,xy, 0, 64,in, deg, 50, 00.00,N, 20, 00.00,E, grid,,,,N"
            }
            Dim linie3Brak = {
                "OziExplorer Map Data File Version 2.2", "Brakujacy", "nieistniejacy.jpg", "1 ,Map Code,", "WGS 84,, 0, 0,WGS 84",
                "Reserved 1", "Reserved 2", "Magnetic Variation,,,E", "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
                "Point01,xy, 0, 0,in, deg, 50, 00,N, 20, 00,E, grid,,,,N",
                "Point02,xy, 64, 64,in, deg, 49, 59,N, 20, 01,E, grid,,,,N"
            }

            Dim plikMap1 = Path.Combine(folderZrodlowy, "arkusz_a.map")
            Dim plikMap2 = Path.Combine(folderZrodlowy, "arkusz_b.map")
            Dim plikMap3 = Path.Combine(folderZrodlowy, "brakujacy.map")
            File.WriteAllLines(plikMap1, linie1)
            File.WriteAllLines(plikMap2, linie2)
            File.WriteAllLines(plikMap3, linie3Brak)

            Dim listaPlikow = {plikMap1, plikMap2, plikMap3}
            Dim raporty As New List(Of RaportPostepuWsadowego)()
            Dim postep = New Progress(Of RaportPostepuWsadowego)(Sub(r) raporty.Add(r))

            ' Wykonaj konwersję wsadową do KMZ
            Dim wynik = FormImportOzi.KonwertujWsadowo(listaPlikow, folderDocelowy,
                                                       FormatKonwersjiOzi.Kmz, 50, 80, False, postep, CancellationToken.None)

            Assert.Equal(3, wynik.LaczniePlikow)
            Assert.Equal(2, wynik.Sukcesy)
            Assert.Equal(1, wynik.Pominiete)
            Assert.Single(wynik.Bledy)
            Assert.Contains("brakujacy.map", wynik.Bledy(0))

            ' Sprawdź czy pliki KMZ zostały fizycznie wygenerowane
            Dim docelowyKmz1 = Path.Combine(folderDocelowy, "arkusz_a.kmz")
            Dim docelowyKmz2 = Path.Combine(folderDocelowy, "arkusz_b.kmz")
            Assert.True(File.Exists(docelowyKmz1))
            Assert.True(File.Exists(docelowyKmz2))
            Assert.True(New FileInfo(docelowyKmz1).Length > 500)
            Assert.True(New FileInfo(docelowyKmz2).Length > 500)

            ' Wykonaj konwersję wsadową do MBTiles dla pierwszego pliku
            Dim wynikMbtiles = FormImportOzi.KonwertujWsadowo({plikMap1}, folderDocelowy,
                                                              FormatKonwersjiOzi.Mbtiles, 50, 80, False, Nothing, CancellationToken.None)
            Assert.Equal(1, wynikMbtiles.Sukcesy)
            Dim docelowyMbtiles = Path.Combine(folderDocelowy, "arkusz_a.mbtiles")
            Assert.True(File.Exists(docelowyMbtiles))
            Assert.True(New FileInfo(docelowyMbtiles).Length > 1000)
        Finally
            If Directory.Exists(folderZrodlowy) Then Directory.Delete(folderZrodlowy, True)
            If Directory.Exists(folderDocelowy) Then Directory.Delete(folderDocelowy, True)
        End Try
    End Sub

    ''' <summary>
    ''' Konwerter Ozi zapisuje SQLiteDB i OruxMaps klasami eksportu mapy (dawniej tylko JNX, KMZ i MBTiles z własnym
    ''' zapisem), a korekta obrazu zmienia kafle.
    ''' </summary>
    <Fact>
    Public Sub KonwersjaOzi_SQLiteDB_OruxMaps_IKorekta()
        Dim folder = NowyFolderOzi()
        Try
            Dim obraz = Path.Combine(folder, "mala.png")
            Using bmp As New System.Drawing.Bitmap(600, 400)
                Using g = System.Drawing.Graphics.FromImage(bmp)
                    g.Clear(System.Drawing.Color.FromArgb(150, 150, 150))
                    g.FillRectangle(System.Drawing.Brushes.DarkGreen, 100, 100, 300, 150)
                End Using
                bmp.Save(obraz, System.Drawing.Imaging.ImageFormat.Png)
            End Using
            Dim linie = NaglowekMap("Mala", "mala.png", "Latitude/Longitude")
            Dim nr = 1
            For Each p In {(0, 0, 50.0, 20.0), (600, 0, 50.0, 20.02), (600, 400, 49.99, 20.02), (0, 400, 49.99, 20.0)}
                linie.Add(String.Format(Globalization.CultureInfo.InvariantCulture, "Point{0:00},xy, {1}, {2},in, deg, {3:0.0000000}, 0,N, {4:0.0000000}, 0,E, grid,,,,N",
                                        nr, p.Item1, p.Item2, p.Item3, p.Item4))
                nr += 1
            Next
            Dim plikMap = Path.Combine(folder, "mala.map")
            File.WriteAllLines(plikMap, linie)
            Dim ozi = PlikKalibracjiOzi.Wczytaj(plikMap)

            Dim plikSqlitedb = Path.Combine(folder, "mala" & FormImportOzi.RozszerzenieWyniku(FormatKonwersjiOzi.SQLiteDB))
            FormImportOzi.Konwertuj(ozi, FormatKonwersjiOzi.SQLiteDB, plikSqlitedb, 50, 85, False, Nothing, Nothing, CancellationToken.None)
            Using pol As New System.Data.SQLite.SQLiteConnection("Data Source=" & plikSqlitedb & ";Pooling=False")
                pol.Open()
                Using cmd As New System.Data.SQLite.SQLiteCommand("SELECT count(*) FROM tiles", pol)
                    Assert.True(CLng(cmd.ExecuteScalar()) > 0)
                End Using
                Using cmd As New System.Data.SQLite.SQLiteCommand("SELECT tilenumbering FROM info", pol)
                    Assert.Equal("BigPlanet", CStr(cmd.ExecuteScalar()))
                End Using
            End Using

            Dim folderOrux = Path.Combine(folder, "mala_orux")
            FormImportOzi.Konwertuj(ozi, FormatKonwersjiOzi.OruxMaps, folderOrux, 50, 85, False, Nothing, Nothing, CancellationToken.None)
            Assert.True(File.Exists(Path.Combine(folderOrux, MapaOruxMaps.NazwaBazy)))
            Assert.Single(Directory.GetFiles(folderOrux, "*" & MapaOruxMaps.RozszerzenieKalibracji))

            'korekta (kontrast) zmienia kafel w porównaniu z kaflem bez korekty
            Dim z = FormImportOzi.PoziomSzczegolowy(ozi)
            Dim k = WebMercator.ZakresKafli(ozi.ZasiegWgs84, z)
            Dim kx = (k.Item1 + k.Item3) \ 2, ky = (k.Item2 + k.Item4) \ 2
            Dim korekta As New OpcjeKorekcjiMip()
            korekta.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
            Using src As New System.Drawing.Bitmap(obraz)
                Dim bez = FormImportOzi.KafelMercatora(src, ozi, kx, ky, z, 95)
                Dim zKorekta = FormImportOzi.KafelMercatora(src, ozi, kx, ky, z, 95, korekta)
                Assert.NotNull(bez)
                Assert.NotNull(zKorekta)
                Assert.False(bez.SequenceEqual(zKorekta))
            End Using
        Finally
            Try
                Directory.Delete(folder, True)
            Catch
            End Try
        End Try
    End Sub

    Private Shared Function NowyFolderOzi() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_Ozi_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Function NaglowekMap(tytul As String, obraz As String, projekcja As String) As List(Of String)
        Return New List(Of String) From {
            "OziExplorer Map Data File Version 2.2", tytul, obraz, "1 ,Map Code,", "WGS 84,, 0, 0,WGS 84",
            "Reserved 1", "Reserved 2", "Magnetic Variation,,,E",
            "Map Projection," & projekcja & ",PolyCal,No,AutoCalOnly,No,BSBUseWPX,No"}
    End Function

    <Fact>
    Public Sub PunktySiatkiUtmPrzeliczaneNaStopnie()
        'punkty kalibracyjne w siatce UTM strefy 34 (wartości z pyproj dla 50,0/49,9 N i 20,0/20,1 E)
        Dim folder = NowyFolderOzi()
        Try
            Dim linie = NaglowekMap("UTM", "utm.jpg", "(UTM) Universal Transverse Mercator")
            linie.AddRange({
                "Point01,xy, 0, 0,in, deg, , ,N, , ,E, grid, 34, 428333.55, 5539109.82,N",
                "Point02,xy, 1000, 0,in, deg, , ,N, , ,E, grid, 34, 435500.09, 5539018.78,N",
                "Point03,xy, 1000, 1000,in, deg, , ,N, , ,E, grid, 34, 435366.39, 5527900.65,N",
                "Point04,xy, 0, 1000,in, deg, , ,N, , ,E, grid, 34, 428185.00, 5527991.74,N"})
            Dim plik = Path.Combine(folder, "utm.map")
            File.WriteAllLines(plik, linie)
            Dim ozi = PlikKalibracjiOzi.Wczytaj(plik)
            Assert.Equal(4, ozi.Punkty.Count)
            Assert.Equal(4, ozi.PunktyWSiatce)
            Assert.InRange(ozi.Punkty(0).Wgs84.Szerokosc, 49.99999, 50.00001)
            Assert.InRange(ozi.Punkty(0).Wgs84.Dlugosc, 19.99999, 20.00001)
            Assert.InRange(ozi.Punkty(2).Wgs84.Szerokosc, 49.89999, 49.90001)
            Assert.InRange(ozi.Punkty(2).Wgs84.Dlugosc, 20.09999, 20.10001)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub PunktyBezStopniNieSaPrzyjmowaneJakoZero()
        'wcześniej puste pola stopni dawały punkt 0°/0° i "poprawną" kalibrację na Atlantyku
        Dim folder = NowyFolderOzi()
        Try
            Dim linie = NaglowekMap("Lambert", "l.jpg", "Lambert Conformal Conic")
            linie.AddRange({
                "Point01,xy, 0, 0,in, deg, , ,N, , ,E, grid, , 140000, 1250000,N",
                "Point02,xy, 1171, 1171,in, deg, , ,N, , ,E, grid, , 150000, 1240000,N"})
            Dim plik = Path.Combine(folder, "lambert.map")
            File.WriteAllLines(plik, linie)
            Dim ex = Assert.Throws(Of InvalidDataException)(Function() PlikKalibracjiOzi.Wczytaj(plik))
            Assert.Contains("siatce", ex.Message)
            Assert.Null(PlikKalibracjiOzi.SparsujPunkt("Point01,xy, 0, 0,in, deg, , ,N, , ,E, grid, , 140000, 1250000,N", "WGS 84"))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ArkuszDuzegoZasieguKalibrowanyWielomianem()
        'arkusz M-34 1:1 000 000 (punkty z prawdziwego pliku .map): afinicznie RMS ok. 6,3 km, wielomian ok. 200 m
        Dim dane = {
            (354, 192, 51.999611, 17.998189), (5211, 215, 51.999703, 23.998222), (5403, 5460, 47.999646, 23.998363),
            (120, 5438, 47.99956, 17.998333), (2770, 2876, 49.99963, 20.998278), (2781, 254, 51.999657, 20.998203),
            (237, 2814, 49.999585, 17.998265), (5308, 2838, 49.999674, 23.998297), (2760, 5502, 47.999603, 20.998346)}
        Dim punkty = dane.Select(Function(d) New PunktKalibracjiOzi With {.Px = d.Item1, .Py = d.Item2, .Wgs84 = New PunktGeo(d.Item3, d.Item4)}).ToList()
        Dim t = TransformacjaAfiniczna2D.Dopasuj(punkty)
        Assert.Equal(ModelKalibracji.Wielomian2, t.Model)
        Assert.True(t.BladRmsAfinicznyMetry > 5000)
        Assert.True(t.BladRmsMetry < 400, "RMS " & t.BladRmsMetry)
        'odwrotność modelu nieliniowego (metoda Newtona)
        Dim g = t.PikselDoGeo(1234, 4321)
        Dim px = t.GeoDoPiksel(g.Szerokosc, g.Dlugosc)
        Assert.InRange(px.Item1, 1233.99, 1234.01)
        Assert.InRange(px.Item2, 4320.99, 4321.01)

        'cztery narożniki arkusza: model rzutowy (trapez zbiegających się południków)
        Dim t4 = TransformacjaAfiniczna2D.Dopasuj(punkty.Take(4).ToList())
        Assert.Equal(ModelKalibracji.Rzutowy, t4.Model)
        Assert.True(t4.BladRmsMetry < 10)
    End Sub

    <Fact>
    Public Sub ZwyklaMapaZostajeAfiniczna()
        Dim punkty As New List(Of PunktKalibracjiOzi) From {
            New PunktKalibracjiOzi With {.Px = 0, .Py = 0, .Wgs84 = New PunktGeo(50.0, 20.0)},
            New PunktKalibracjiOzi With {.Px = 1000, .Py = 0, .Wgs84 = New PunktGeo(50.0, 20.1)},
            New PunktKalibracjiOzi With {.Px = 1000, .Py = 1000, .Wgs84 = New PunktGeo(49.9, 20.1)},
            New PunktKalibracjiOzi With {.Px = 0, .Py = 1000, .Wgs84 = New PunktGeo(49.9, 20.0)}}
        Assert.Equal(ModelKalibracji.Afiniczny, TransformacjaAfiniczna2D.Dopasuj(punkty).Model)
    End Sub

    <Fact>
    Public Sub WymiaryObrazuBmp()
        Dim folder = NowyFolderOzi()
        Try
            Dim plik = Path.Combine(folder, "skan.bmp")
            Using bmp As New System.Drawing.Bitmap(321, 123)
                bmp.Save(plik, System.Drawing.Imaging.ImageFormat.Bmp)
            End Using
            Dim n = NaglowekObrazu.Odczytaj(plik)
            Assert.Equal(321L, n.SzerokoscPx)
            Assert.Equal(123L, n.WysokoscPx)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub UnikalneNazwyWynikowWsadu()
        Dim uzyte As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Assert.Equal("arkusz", FormImportOzi.UnikalnaNazwaWyniku("C:\mapy\a\arkusz.map", uzyte))
        Assert.Equal("b_arkusz", FormImportOzi.UnikalnaNazwaWyniku("C:\mapy\b\arkusz.map", uzyte))
        Assert.Equal("b_ARKUSZ_2", FormImportOzi.UnikalnaNazwaWyniku("C:\mapy\b\ARKUSZ.map", uzyte))
    End Sub

    <Fact>
    Public Sub ObroconaMapaTrafiaWeWlasciwePolozenieWMbtiles()
        'skan obrócony o 10 stopni: czerwony znacznik w pikselu (300, 200) musi wypaść w kaflu tam, gdzie wskazuje kalibracja
        Dim folder = NowyFolderOzi()
        Try
            Dim obraz = Path.Combine(folder, "obrocona.png")
            Using bmp As New System.Drawing.Bitmap(800, 600)
                Using g = System.Drawing.Graphics.FromImage(bmp)
                    g.Clear(System.Drawing.Color.LightGray)
                    g.FillEllipse(System.Drawing.Brushes.Red, 300 - 6, 200 - 6, 12, 12)
                End Using
                bmp.Save(obraz, System.Drawing.Imaging.ImageFormat.Png)
            End Using
            Dim kat = 10.0 * Math.PI / 180
            Dim stopienNaPx = 0.00003
            Dim geo = Function(px As Double, py As Double) As PunktGeo
                          Dim e = (px * Math.Cos(kat) + py * Math.Sin(kat)) * stopienNaPx
                          Dim n = (-px * Math.Sin(kat) + py * Math.Cos(kat)) * stopienNaPx
                          Return New PunktGeo(50.0 - n, 20.0 + e / Math.Cos(50.0 * Math.PI / 180))
                      End Function
            Dim linie = NaglowekMap("Obrocona", "obrocona.png", "Latitude/Longitude")
            Dim nr = 1
            For Each p In {(0, 0), (800, 0), (800, 600), (0, 600)}
                Dim g = geo(p.Item1, p.Item2)
                linie.Add(String.Format(Globalization.CultureInfo.InvariantCulture, "Point{0:00},xy, {1}, {2},in, deg, {3:0.0000000}, 0,N, {4:0.0000000}, 0,E, grid,,,,N",
                                        nr, p.Item1, p.Item2, g.Szerokosc, g.Dlugosc))
                nr += 1
            Next
            Dim plikMap = Path.Combine(folder, "obrocona.map")
            File.WriteAllLines(plikMap, linie)
            Dim ozi = PlikKalibracjiOzi.Wczytaj(plikMap)
            Dim plikMb = Path.Combine(folder, "obrocona.mbtiles")
            FormImportOzi.KonwertujDoMbtiles(ozi, plikMb, 95, Nothing, CancellationToken.None)

            Dim cel = geo(300, 200)
            Dim z = FormImportOzi.PoziomSzczegolowy(ozi)
            Dim swiat = 256.0 * Math.Pow(2, z)
            Dim gx = (cel.Dlugosc + 180) / 360 * swiat
            Dim fi = cel.Szerokosc * Math.PI / 180
            Dim gy = (1 - Math.Log(Math.Tan(fi) + 1 / Math.Cos(fi)) / Math.PI) / 2 * swiat
            Dim kx = CInt(Math.Floor(gx / 256)), ky = CInt(Math.Floor(gy / 256))
            Dim dane As Byte()
            Using pol As New System.Data.SQLite.SQLiteConnection("Data Source=" & plikMb & ";Pooling=False")
                pol.Open()
                Using cmd As New System.Data.SQLite.SQLiteCommand("SELECT tile_data FROM tiles WHERE zoom_level=@z AND tile_column=@x AND tile_row=@y", pol)
                    cmd.Parameters.AddWithValue("@z", z)
                    cmd.Parameters.AddWithValue("@x", kx)
                    cmd.Parameters.AddWithValue("@y", New KafelXYZ(kx, ky, z).WierszTms)
                    dane = DirectCast(cmd.ExecuteScalar(), Byte())
                End Using
                Using cmd As New System.Data.SQLite.SQLiteCommand("SELECT COUNT(DISTINCT zoom_level) FROM tiles", pol)
                    Assert.True(CLng(cmd.ExecuteScalar()) >= 2, "piramida poziomów")
                End Using
            End Using
            Using ms As New MemoryStream(dane), kafel As New System.Drawing.Bitmap(ms)
                Dim u = gx - kx * 256, v = gy - ky * 256
                Dim sx = 0.0, sy = 0.0, licz = 0
                For yy = 0 To 255
                    For xx = 0 To 255
                        Dim c = kafel.GetPixel(xx, yy)
                        If c.R > 180 AndAlso c.G < 90 AndAlso c.B < 90 Then sx += xx + 0.5 : sy += yy + 0.5 : licz += 1
                    Next
                Next
                Assert.True(licz > 10, "brak znacznika w kaflu")
                Assert.InRange(sx / licz, u - 2.0, u + 2.0)
                Assert.InRange(sy / licz, v - 2.0, v + 2.0)
            End Using
        Finally
            Try
                Directory.Delete(folder, True)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                System.Data.SQLite.SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()
                Directory.Delete(folder, True)
            End Try
        End Try
    End Sub

    Private Class PostepZAnulowaniem
        Implements IProgress(Of Integer)
        Private ReadOnly _cts As CancellationTokenSource
        Public Sub New(cts As CancellationTokenSource)
            _cts = cts
        End Sub
        Public Sub Report(value As Integer) Implements IProgress(Of Integer).Report
            If value >= 30 Then _cts.Cancel()
        End Sub
    End Class

    <Theory>
    <InlineData("kmz")>
    <InlineData("jnx")>
    <InlineData("mbtiles")>
    Public Sub AnulowanaKonwersjaNieZostawiaPlikow(format As String)
        'anulowany KMZ zostawiał niekompletny plik (5,9 MB), MBTiles - pustą bazę; użytkownik mógł je wziąć za gotową mapę
        Dim folder = NowyFolderOzi()
        Try
            Dim obraz = Path.Combine(folder, "skan.png")
            Using bmp As New System.Drawing.Bitmap(3000, 3000)
                Using g = System.Drawing.Graphics.FromImage(bmp)
                    g.Clear(System.Drawing.Color.Olive)
                End Using
                bmp.Save(obraz, System.Drawing.Imaging.ImageFormat.Png)
            End Using
            Dim linie = NaglowekMap("Skan", "skan.png", "Latitude/Longitude")
            linie.AddRange({
                "Point01,xy, 0, 0,in, deg, 50, 6.0,N, 20, 0.0,E, grid,,,,N",
                "Point02,xy, 3000, 0,in, deg, 50, 6.0,N, 20, 6.0,E, grid,,,,N",
                "Point03,xy, 3000, 3000,in, deg, 50, 0.0,N, 20, 6.0,E, grid,,,,N",
                "Point04,xy, 0, 3000,in, deg, 50, 0.0,N, 20, 0.0,E, grid,,,,N"})
            Dim plikMap = Path.Combine(folder, "skan.map")
            File.WriteAllLines(plikMap, linie)
            Dim ozi = PlikKalibracjiOzi.Wczytaj(plikMap)
            Dim cts As New CancellationTokenSource()
            Dim wynik = Path.Combine(folder, "wynik." & format)
            Assert.ThrowsAny(Of OperationCanceledException)(
                Sub()
                    Select Case format
                        Case "kmz" : FormImportOzi.KonwertujDoKmz(ozi, wynik, 30, 85, True, New PostepZAnulowaniem(cts), cts.Token)
                        Case "jnx" : FormImportOzi.KonwertujDoJnx(ozi, wynik, 30, 85, New PostepZAnulowaniem(cts), cts.Token)
                        Case Else : FormImportOzi.KonwertujDoMbtiles(ozi, wynik, 85, New PostepZAnulowaniem(cts), cts.Token)
                    End Select
                End Sub)
            System.Data.SQLite.SQLiteConnection.ClearAllPools()
            Assert.Empty(Directory.GetFiles(folder, "wynik*"))
        Finally
            Try
                Directory.Delete(folder, True)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                System.Data.SQLite.SQLiteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()
                Directory.Delete(folder, True)
            End Try
        End Try
    End Sub

End Class
