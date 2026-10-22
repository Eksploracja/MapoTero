'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Data.SQLite
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.IO.Compression
Imports System.Threading
Imports System.Xml.Linq
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class EksportTesty

    ''' <summary>Siatka 2 x 2 segmenty po 256 px, piksel 1 m (PL-1992, okolice Warszawy).</summary>
    Private Shared Function Siatka2x2() As Siatka
        Return New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
    End Function

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_Eksport_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Sub Usun(folder As String)
        SQLiteConnection.ClearAllPools()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        If Directory.Exists(folder) Then Directory.Delete(folder, True)
    End Sub

    ''' <summary>Zapisuje 4 segmenty; rysuj - rysowanie zawartości segmentu (wiersz, kolumna) na przezroczystym tle.</summary>
    Private Shared Sub UtworzSegmenty(folder As String, rozszerzenie As String, rysuj As Action(Of Graphics, Integer, Integer))
        For w = 1 To 2
            For k = 1 To 2
                Using bmp As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                    Using g = Graphics.FromImage(bmp)
                        g.Clear(Color.Transparent)
                        rysuj(g, w, k)
                    End Using
                    Dim plik = folder & "_" & w.ToString("D2") & "_" & k.ToString("D2") & "." & rozszerzenie
                    If rozszerzenie = "jpg" Then
                        Using rgb As New Bitmap(256, 256, PixelFormat.Format24bppRgb)
                            Using g = Graphics.FromImage(rgb)
                                g.Clear(Color.White)
                                g.DrawImage(bmp, 0, 0, 256, 256)
                            End Using
                            rgb.Save(plik, ImageFormat.Jpeg)
                        End Using
                    Else
                        bmp.Save(plik, ImageFormat.Png)
                    End If
                End Using
            Next
        Next
    End Sub

    Private Shared Function Zrodlo(folder As String, rozszerzenie As String) As ZrodloSegmentow
        Return New ZrodloSegmentow(folder, "_", StylNumeracji.WierszKolumna, rozszerzenie, Siatka2x2())
    End Function

    Private Shared Function Zadanie(rodzaj As RodzajEksportu, zrodlo As ZrodloSegmentow, plik As String) As ZadanieEksportu
        Dim m As New MetadaneMapy With {.Tytul = "Test warstwa", .DataPobrania = New Date(2026, 10, 19)}
        m.Warstwy.Add("Test warstwa")
        m.DodajZrodlo("Źródło testowe")
        Return New ZadanieEksportu With {.Rodzaj = rodzaj, .Zrodlo = zrodlo, .Uklad = UkladWspolrzednych.PL1992, .Plik = plik,
                                         .PoziomMin = 15, .PoziomMax = 16, .JakoscJpeg = 90, .Metadane = m, .Nazwa = m.Nazwa}
    End Function

    Private Shared Function Polacz(plik As String) As SQLiteConnection
        Dim p As New SQLiteConnection("Data Source=" & plik & ";Version=3;Pooling=False;")
        p.Open()
        Return p
    End Function

    Private Shared Function Zapytanie(pol As SQLiteConnection, sql As String) As Object
        Using cmd As New SQLiteCommand(sql, pol)
            Return cmd.ExecuteScalar()
        End Using
    End Function

    Private Shared Function Obraz(dane() As Byte) As Bitmap
        Using ms As New MemoryStream(dane), o = Image.FromStream(ms)
            Return New Bitmap(o)
        End Using
    End Function

    ''' <summary>Kafel i piksel Web Mercator punktu siatki PL-1992 na poziomie z.</summary>
    Private Shared Function PikselKafla(p As PunktXY, z As Integer) As Tuple(Of Integer, Integer, Integer, Integer)
        Dim g = UkladWspolrzednych.PL1992.DoWgs84(p)
        Dim px = WebMercator.PikselX(g.Dlugosc, z), py = WebMercator.PikselY(g.Szerokosc, z)
        Dim tx = CInt(Math.Floor(px / 256)), ty = CInt(Math.Floor(py / 256))
        Return Tuple.Create(tx, ty, CInt(Math.Floor(px - tx * 256)), CInt(Math.Floor(py - ty * 256)))
    End Function

    <Fact>
    Public Sub MBTilesNakladkaZPrzezroczystoscia()
        Dim folder = NowyFolder()
        Try
            'lewa połowa każdego segmentu czerwona, prawa - przezroczysta
            UtworzSegmenty(folder, "png", Sub(g, w, k) g.FillRectangle(Brushes.Red, 0, 0, 128, 256))
            Dim z = Zadanie(RodzajEksportu.MBTiles, Zrodlo(folder, "png"), folder & "mapa.mbtiles")
            z.Nakladka = True
            Assert.True(z.Png)
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)

            Using pol = Polacz(z.Plik)
                Assert.Equal("overlay", Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'type'"))
                Assert.Equal("png", Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'format'"))
                Assert.Equal("Test warstwa (2026-10-19)", Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'name'"))
                Assert.Equal("Źródło: Źródło testowe", Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'attribution'"))
                Assert.Contains("Data pobrania: 2026-10-19", CStr(Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'description'")))
                Assert.True(CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles WHERE zoom_level = 16")) > 0)
                Assert.True(CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles WHERE zoom_level = 15")) > 0)

                'środek lewej (czerwonej) i prawej (przezroczystej) połowy segmentu (1,1)
                Dim s = Siatka2x2()
                Dim seg = s.Segment(1, 1)
                For Each przypadek In {Tuple.Create(New PunktXY(seg.XDol + 128, seg.YLewy + 64), True),
                                       Tuple.Create(New PunktXY(seg.XDol + 128, seg.YLewy + 192), False)}
                    Dim t = PikselKafla(przypadek.Item1, 16)
                    Dim dane = TryCast(Zapytanie(pol, "SELECT tile_data FROM tiles WHERE zoom_level = 16 AND tile_column = " & t.Item1 &
                                                      " AND tile_row = " & ((1 << 16) - 1 - t.Item2)), Byte())
                    Assert.NotNull(dane)
                    Using bmp = Obraz(dane)
                        Dim kolor = bmp.GetPixel(t.Item3, t.Item4)
                        If przypadek.Item2 Then
                            Assert.Equal(255, kolor.A)
                            Assert.InRange(kolor.R, 250, 255)
                            Assert.InRange(kolor.G, 0, 5)
                        Else
                            Assert.Equal(0, kolor.A)
                        End If
                    End Using
                Next
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub LaczenieWarstwZKryciemIBialymTlem()
        Dim podstawa = NowyFolder()
        Dim nakladka = NowyFolder()
        Try
            UtworzSegmenty(podstawa, "jpg", Sub(g, w, k) g.Clear(Color.Red))
            'nakładka: białe tło z niebieskim kwadratem w środku segmentu (1,1)
            UtworzSegmenty(nakladka, "jpg", Sub(g, w, k)
                                                 g.Clear(Color.White)
                                                 If w = 1 AndAlso k = 1 Then g.FillRectangle(Brushes.Blue, 64, 64, 128, 128)
                                             End Sub)
            Dim warstwa = Zrodlo(nakladka, "jpg")
            warstwa.BialePrzezroczyste = True
            Dim z = Zadanie(RodzajEksportu.MBTiles, Zrodlo(podstawa, "jpg"), podstawa & "mapa.mbtiles")
            z.PoziomMin = 16
            z.Nakladki.Add(New WarstwaEksportu With {.Zrodlo = warstwa, .Uklad = UkladWspolrzednych.PL1992, .Krycie = 0.5})
            Assert.Equal(2, z.WarstwyRenderowania().Count)
            Assert.False(z.Png)
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)

            Using pol = Polacz(z.Plik)
                Assert.Equal("baselayer", Zapytanie(pol, "SELECT value FROM metadata WHERE name = 'type'"))
                Dim seg = Siatka2x2().Segment(1, 1)
                'środek niebieskiego kwadratu: czerwony + niebieski z kryciem 50%; narożnik segmentu (białe tło nakładki) - czerwony
                For Each przypadek In {Tuple.Create(New PunktXY(seg.XDol + 128, seg.YLewy + 128), Color.FromArgb(128, 0, 127)),
                                       Tuple.Create(New PunktXY(seg.XGora - 20, seg.YLewy + 20), Color.FromArgb(255, 0, 0))}
                    Dim t = PikselKafla(przypadek.Item1, 16)
                    Dim dane = TryCast(Zapytanie(pol, "SELECT tile_data FROM tiles WHERE zoom_level = 16 AND tile_column = " & t.Item1 &
                                                      " AND tile_row = " & ((1 << 16) - 1 - t.Item2)), Byte())
                    Assert.NotNull(dane)
                    Using bmp = Obraz(dane)
                        Dim kolor = bmp.GetPixel(t.Item3, t.Item4)
                        Assert.InRange(CInt(kolor.R), przypadek.Item2.R - 25, przypadek.Item2.R + 25)
                        Assert.InRange(CInt(kolor.G), przypadek.Item2.G - 25, przypadek.Item2.G + 25)
                        Assert.InRange(CInt(kolor.B), przypadek.Item2.B - 25, przypadek.Item2.B + 25)
                    End Using
                Next
            End Using
        Finally
            Usun(podstawa)
            Usun(nakladka)
        End Try
    End Sub

    <Fact>
    Public Sub SQLiteDBOdwroconePoziomyITabelaInfo()
        Dim folder = NowyFolder()
        Try
            UtworzSegmenty(folder, "jpg", Sub(g, w, k) g.Clear(Color.Green))
            Dim z = Zadanie(RodzajEksportu.SQLiteDB, Zrodlo(folder, "jpg"), folder & "mapa.sqlitedb")
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)

            Using pol = Polacz(z.Plik)
                Assert.Equal(2L, CLng(Zapytanie(pol, "SELECT COUNT(DISTINCT z) FROM tiles")))
                Assert.Equal(1L, CLng(Zapytanie(pol, "SELECT MIN(z) FROM tiles")))
                Assert.Equal(2L, CLng(Zapytanie(pol, "SELECT MAX(z) FROM tiles")))
                Assert.Equal(0L, CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles WHERE s <> 0")))
                Assert.Equal(2L, CLng(Zapytanie(pol, "SELECT maxzoom FROM info")))
                Assert.Equal(1L, CLng(Zapytanie(pol, "SELECT minzoom FROM info")))
                Assert.Equal("BigPlanet", Zapytanie(pol, "SELECT tilenumbering FROM info"))
                'po zapisie baza wraca do zwykłego dziennika (WAL tylko na czas eksportu)
                Assert.Equal("delete", CStr(Zapytanie(pol, "PRAGMA journal_mode")).ToLowerInvariant())
                'kafle numerowane jak XYZ (wiersze od północy)
                Dim r = WebMercator.ZakresKafli(EksportMapy.ObszarKafli(z), 16)
                Assert.Equal(CLng(r.Item1), CLng(Zapytanie(pol, "SELECT MIN(x) FROM tiles WHERE z = 1")))
                Assert.Equal(CLng(r.Item2), CLng(Zapytanie(pol, "SELECT MIN(y) FROM tiles WHERE z = 1")))
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub OruxMapsFolderBazaIKalibracja()
        Dim folder = NowyFolder()
        Try
            UtworzSegmenty(folder, "png", Sub(g, w, k) g.Clear(Color.Yellow))
            Dim z = Zadanie(RodzajEksportu.OruxMaps, Zrodlo(folder, "png"), folder & "_eksport_2x2_oruxmaps")
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)

            Assert.True(File.Exists(Path.Combine(z.Plik, MapaOruxMaps.NazwaBazy)))
            Dim xmlPlik = Path.Combine(z.Plik, "_eksport_2x2_oruxmaps" & MapaOruxMaps.RozszerzenieKalibracji)
            Assert.True(File.Exists(xmlPlik))
            Dim doc = XDocument.Load(xmlPlik)
            Dim ns As XNamespace = "http://oruxtracker.com/app/res/calibration"
            Dim poziomy = doc.Root.Element(ns + "MapCalibration").Elements(ns + "MapCalibration").ToList()
            Assert.Equal(2, poziomy.Count)

            Using pol = Polacz(Path.Combine(z.Plik, MapaOruxMaps.NazwaBazy))
                Assert.Equal("pl_PL", Zapytanie(pol, "SELECT locale FROM android_metadata"))
                For Each p In poziomy
                    Dim poziom = p.Attribute("layerLevel").Value
                    Dim chunks = p.Element(ns + "MapChunks")
                    'kafle poziomu numerowane od 0 i mieszczą się w zakresie z kalibracji
                    Assert.Equal(0L, CLng(Zapytanie(pol, "SELECT MIN(x) FROM tiles WHERE z = " & poziom)))
                    Assert.Equal(0L, CLng(Zapytanie(pol, "SELECT MIN(y) FROM tiles WHERE z = " & poziom)))
                    Assert.True(CLng(Zapytanie(pol, "SELECT MAX(x) FROM tiles WHERE z = " & poziom)) < CLng(chunks.Attribute("xMax").Value))
                    Assert.True(CLng(Zapytanie(pol, "SELECT MAX(y) FROM tiles WHERE z = " & poziom)) < CLng(chunks.Attribute("yMax").Value))
                Next
            End Using
            Assert.Contains("Test warstwa (2026-10-19)", doc.Root.Element(ns + "MapCalibration").Element(ns + "MapName").Value)
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub KmzNakladkaWPngZOpisem()
        Dim folder = NowyFolder()
        Try
            UtworzSegmenty(folder, "png", Sub(g, w, k) g.FillEllipse(Brushes.Black, 50, 50, 100, 100))
            Dim z = Zadanie(RodzajEksportu.Kmz, Zrodlo(folder, "png"), folder & "mapa.kmz")
            z.Nakladka = True
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)
            Using zip = ZipFile.OpenRead(z.Plik)
                Assert.Contains(zip.Entries, Function(e) e.FullName.StartsWith("files/") AndAlso e.FullName.EndsWith(".png"))
                Using r As New StreamReader(zip.GetEntry("doc.kml").Open())
                    Assert.Contains("Źródło testowe", r.ReadToEnd())
                End Using
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub BrakujaceSegmentyIMaDane()
        Dim folder = NowyFolder()
        Try
            UtworzSegmenty(folder, "png", Sub(g, w, k) g.Clear(Color.Gray))
            File.Delete(folder & "_02_02.png")
            Dim zr = Zrodlo(folder, "png")
            Assert.Equal(3, zr.LiczbaSegmentow)
            Dim s = Siatka2x2()
            Assert.True(zr.MaDane(s.Segment(1, 1)))
            Dim brak = s.Segment(2, 2)
            Assert.False(zr.MaDane(New Zasieg(brak.XDol + 10, brak.YLewy + 10, brak.XGora - 10, brak.YPrawy - 10)))
            Assert.False(zr.MaDane(New Zasieg(0, 0, 10, 10)))
            Dim r As Double, g2 As Double, b As Double, a As Double
            Assert.False(zr.Probka(brak.Srodek.X, brak.Srodek.Y, r, g2, b, a))
            Assert.True(zr.Probka(s.Segment(1, 1).Srodek.X, s.Segment(1, 1).Srodek.Y, r, g2, b, a))
            Assert.Equal(255.0, a, 3)
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub SzacowanieRozmiaruILimit()
        Dim folder = NowyFolder()
        Try
            'segmenty z szumem - kafle JPEG o niebanalnym rozmiarze
            Dim los As New Random(7)
            UtworzSegmenty(folder, "png", Sub(g, w, k)
                                              For i = 0 To 400
                                                  Using pedzel As New SolidBrush(Color.FromArgb(255, los.Next(256), los.Next(256), los.Next(256)))
                                                      g.FillRectangle(pedzel, los.Next(256), los.Next(256), 12, 12)
                                                  End Using
                                              Next
                                          End Sub)
            Dim z = Zadanie(RodzajEksportu.MBTiles, Zrodlo(folder, "png"), folder & "mapa.mbtiles")
            Dim s = EksportMapy.SzacujRozmiar(z, 16, CancellationToken.None)
            Assert.True(s.Probki > 0)
            Assert.True(s.Bajty > 0)
            Assert.Equal(2, s.Poziomy.Count)
            Assert.Equal(16, EksportMapy.PoziomDlaLimitu(s, Long.MaxValue))
            Assert.Equal(-1, EksportMapy.PoziomDlaLimitu(s, 0))
            Assert.Equal(15, EksportMapy.PoziomDlaLimitu(s, s.Poziomy(15)))

            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)
            Using pol = Polacz(z.Plik)
                Dim rzeczywisty = CLng(Zapytanie(pol, "SELECT SUM(LENGTH(tile_data)) FROM tiles"))
                Assert.InRange(rzeczywisty, s.Bajty \ 4, s.Bajty * 4)
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub MBTilesSciezkaZeSrednikiemIStarymDziennikiem()
        'średnik jest dozwolony w nazwach folderów Windows - dawniej psuł tekst połączenia z bazą;
        'pozostały plik -wal przerwanego eksportu nie może trafić do nowej bazy
        Dim folder = NowyFolder()
        Try
            Dim podfolder = folder & "mapy;2026" & Path.DirectorySeparatorChar
            Directory.CreateDirectory(podfolder)
            UtworzSegmenty(podfolder, "png", Sub(g, w, k) g.Clear(Color.Blue))
            Dim plik = podfolder & "mapa.mbtiles"
            File.WriteAllBytes(plik & "-wal", New Byte() {1, 2, 3, 4, 5, 6, 7, 8})
            Dim z = Zadanie(RodzajEksportu.MBTiles, Zrodlo(podfolder, "png"), plik)
            EksportMapy.Eksportuj(z, Nothing, CancellationToken.None)

            Assert.False(File.Exists(plik & "-wal"))
            Dim b As New SQLiteConnectionStringBuilder With {.DataSource = plik, .Pooling = False}
            Using pol As New SQLiteConnection(b.ToString())
                pol.Open()
                Assert.True(CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles")) > 0)
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

End Class
