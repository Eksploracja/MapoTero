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
            fs.Seek(52, SeekOrigin.Begin) ' Pozycja Level Info
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
            File.WriteAllBytes(sciezkaZJnx, maxKafelBytes)
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
            File.WriteAllText(Path.Combine(podfolder, "mapa1.map"), "test")
            File.WriteAllText(Path.Combine(podfolder, "mapa2.map"), "test")
            File.WriteAllText(Path.Combine(podfolder, "obraz.jpg"), "test")
            Dim wPodfolderze = FormImportOzi.ZnajdzPlikiMapWFolderze(folderTestu)
            Assert.Equal(2, wPodfolderze.Count)

            ' 3. Pliki w folderze głównym -> powinno preferować folder główny
            File.WriteAllText(Path.Combine(folderTestu, "glowna.map"), "test")
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

End Class
