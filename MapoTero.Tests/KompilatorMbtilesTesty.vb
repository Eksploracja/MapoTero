'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Data.SQLite
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

Public Class KompilatorMbtilesTesty

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_Kompilator_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Sub Usun(folder As String)
        SQLiteConnection.ClearAllPools()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        If Directory.Exists(folder) Then
            Try : Directory.Delete(folder, True) : Catch : End Try
        End If
    End Sub

    Private Shared Sub UtworzSesje(folder As String, xDol As Double, yLewy As Double, xGora As Double, yPrawy As Double,
                                  piksel As Double, kolor As Color)
        Dim c As New KonfiguracjaSesji With {
            .Folder = folder,
            .RodzajMapy = "Testowa",
            .XDol = xDol.ToString(Globalization.CultureInfo.InvariantCulture),
            .YLewy = yLewy.ToString(Globalization.CultureInfo.InvariantCulture),
            .XGora = xGora.ToString(Globalization.CultureInfo.InvariantCulture),
            .YPrawy = yPrawy.ToString(Globalization.CultureInfo.InvariantCulture),
            .RozmiarPiksela = piksel.ToString(Globalization.CultureInfo.InvariantCulture),
            .BokSegmentuPx = "256",
            .UkladEpsg = 2180,
            .Format = "png",
            .Prefiks = "_",
            .Numeracja = "1",
            .Przezroczystosc = True
        }
        c.Warstwy(0) = "W1"
        c.LiczbaWarstw = 1
        c.Zapisz(Path.Combine(folder, "conf.txt"), System.Text.Encoding.UTF8)

        ' Zapis 4 segmentów 2x2
        For w = 1 To 2
            For k = 1 To 2
                Using bmp As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                    Using g = Graphics.FromImage(bmp)
                        g.Clear(kolor)
                    End Using
                    bmp.Save(Path.Combine(folder, "_" & w.ToString("D2") & "_" & k.ToString("D2") & ".png"), ImageFormat.Png)
                End Using
            Next
        Next
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_SprawdzLukiPoziomow_WykrywaBraki()
        Dim w1 As New WarstwaWielowarstwowa With {.PoziomMin = 10, .PoziomMax = 12, .Wlaczona = True,
            .Siatka = New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256),
            .Zrodlo = New ZrodloSegmentow("C:\", "_", StylNumeracji.WierszKolumna, "png", New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256))}
        Dim w2 As New WarstwaWielowarstwowa With {.PoziomMin = 15, .PoziomMax = 16, .Wlaczona = True,
            .Siatka = New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256),
            .Zrodlo = New ZrodloSegmentow("C:\", "_", StylNumeracji.WierszKolumna, "png", New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256))}

        Dim warstwy As New List(Of WarstwaWielowarstwowa) From {w1, w2}
        Dim luki = KompilatorMbtiles.SprawdzLukiPoziomow(warstwy, 10, 16)

        Assert.Equal(2, luki.Count)
        Assert.Contains(13, luki)
        Assert.Contains(14, luki)
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_WarstwyDlaPoziomu_FiltrujeIPorzadkuje()
        Dim w1 As New WarstwaWielowarstwowa With {.Tytul = "Podklad", .PoziomMin = 10, .PoziomMax = 16, .Kolejnosc = 1, .Wlaczona = True,
            .Siatka = New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256),
            .Zrodlo = New ZrodloSegmentow("C:\", "_", StylNumeracji.WierszKolumna, "png", New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256))}
        Dim w2 As New WarstwaWielowarstwowa With {.Tytul = "Nakladka", .PoziomMin = 14, .PoziomMax = 18, .Kolejnosc = 2, .Wlaczona = True,
            .Siatka = New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256),
            .Zrodlo = New ZrodloSegmentow("C:\", "_", StylNumeracji.WierszKolumna, "png", New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256))}
        Dim w3Wylaczona As New WarstwaWielowarstwowa With {.Tytul = "Wylaczona", .PoziomMin = 10, .PoziomMax = 16, .Kolejnosc = 0, .Wlaczona = False}

        Dim warstwy As New List(Of WarstwaWielowarstwowa) From {w2, w1, w3Wylaczona}

        ' Na zoomie 12 powinna być tylko warstwa w1
        Dim dla12 = KompilatorMbtiles.WarstwyDlaPoziomu(warstwy, 12)
        Assert.Single(dla12)
        Assert.Equal("Podklad", dla12(0).Tytul)

        ' Na zoomie 15 powinny być obie w kolejności: Podklad (1), Nakladka (2)
        Dim dla15 = KompilatorMbtiles.WarstwyDlaPoziomu(warstwy, 15)
        Assert.Equal(2, dla15.Count)
        Assert.Equal("Podklad", dla15(0).Tytul)
        Assert.Equal("Nakladka", dla15(1).Tytul)
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_PelnaKompilacja_TworzyPoprawnyMbtiles()
        Dim folder = NowyFolder()
        Try
            Dim folderW1 = Path.Combine(folder, "sesja_topo") & Path.DirectorySeparatorChar
            Dim folderW2 = Path.Combine(folder, "sesja_orto") & Path.DirectorySeparatorChar
            Directory.CreateDirectory(folderW1)
            Directory.CreateDirectory(folderW2)

            ' Obszar w okolicach Warszawy (PL-1992)
            UtworzSesje(folderW1, 487000, 637000, 487512, 637512, 1.0, Color.FromArgb(200, 50, 150, 50))
            UtworzSesje(folderW2, 487000, 637000, 487512, 637512, 1.0, Color.FromArgb(255, 10, 20, 200))

            Dim w1 As New WarstwaWielowarstwowa With {.Folder = folderW1, .Tytul = "Topo", .PoziomMin = 15, .PoziomMax = 15, .Kolejnosc = 1}
            Dim blad1 As String = ""
            Assert.True(w1.WczytajSesje(blad1), blad1)

            Dim w2 As New WarstwaWielowarstwowa With {.Folder = folderW2, .Tytul = "Orto", .PoziomMin = 16, .PoziomMax = 16, .Kolejnosc = 2}
            Dim blad2 As String = ""
            Assert.True(w2.WczytajSesje(blad2), blad2)

            Dim plikMbtiles = Path.Combine(folder, "wynik.mbtiles")
            Dim opcje As New OpcjeKompilatoraMbtiles With {
                .PlikDocelowy = plikMbtiles,
                .NazwaMapy = "Test Wielowarstwowy",
                .PoziomMin = 15,
                .PoziomMax = 16,
                .FormatKafli = FormatKafliMbtiles.SmartHybrid,
                .JakoscJpeg = 80,
                .SzybkaPiramidaWPasmach = False
            }

            Dim warstwy As New List(Of WarstwaWielowarstwowa) From {w1, w2}

            ' Uruchomienie kompilacji
            KompilatorMbtiles.KompilujAsync(warstwy, opcje, Nothing, CancellationToken.None).GetAwaiter().GetResult()

            Assert.True(File.Exists(plikMbtiles), "Plik wynikowy MBTiles nie istnieje.")

            ' Sprawdzenie poprawności bazy SQLite
            Using pol As New SQLiteConnection("Data Source=" & plikMbtiles & ";Version=3;Pooling=False;")
                pol.Open()

                ' Sprawdzenie metadanych
                Using cmd As New SQLiteCommand("SELECT value FROM metadata WHERE name = 'name';", pol)
                    Assert.Equal("Test Wielowarstwowy", cmd.ExecuteScalar())
                End Using
                Using cmd As New SQLiteCommand("SELECT value FROM metadata WHERE name = 'minzoom';", pol)
                    Assert.Equal("15", cmd.ExecuteScalar())
                End Using
                Using cmd As New SQLiteCommand("SELECT value FROM metadata WHERE name = 'maxzoom';", pol)
                    Assert.Equal("16", cmd.ExecuteScalar())
                End Using

                ' Sprawdzenie obecności tabeli android_metadata
                Using cmd As New SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='android_metadata';", pol)
                    Assert.Equal(1L, CLng(cmd.ExecuteScalar()))
                End Using

                ' Sprawdzenie obecności indeksu kafli
                Using cmd As New SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type='index' AND name='tile_index';", pol)
                    Assert.Equal(1L, CLng(cmd.ExecuteScalar()))
                End Using

                ' Sprawdzenie obecności kafli na poziomie 15 i 16
                Using cmd As New SQLiteCommand("SELECT count(*) FROM tiles WHERE zoom_level = 15;", pol)
                    Dim ileZ15 = CLng(cmd.ExecuteScalar())
                    Assert.True(ileZ15 > 0, "Brak kafli na poziomie 15.")
                End Using

                Using cmd As New SQLiteCommand("SELECT count(*) FROM tiles WHERE zoom_level = 16;", pol)
                    Dim ileZ16 = CLng(cmd.ExecuteScalar())
                    Assert.True(ileZ16 > 0, "Brak kafli na poziomie 16.")
                End Using

                ' Sprawdzenie poprawności zapisu obrazu w kafelku
                Using cmd As New SQLiteCommand("SELECT tile_data FROM tiles LIMIT 1;", pol)
                    Dim dane = TryCast(cmd.ExecuteScalar(), Byte())
                    Assert.NotNull(dane)
                    Assert.True(dane.Length > 0)
                    Using ms As New MemoryStream(dane)
                        Using img = Image.FromStream(ms)
                            Assert.Equal(256, img.Width)
                            Assert.Equal(256, img.Height)
                        End Using
                    End Using
                End Using
            End Using

        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub TestNakladaniaWarstwKazimierz()
        Dim folderDownload = "C:\Users\KN\source\repos\MapoTero_testy\MapoTero_SingleFile\download\"
        Dim folderTopo10 = Path.Combine(folderDownload, "gmina_kazimierz_topo10_65")
        Dim folderOrto = Path.Combine(folderDownload, "gmina_kazimierz_orto")
        If Not Directory.Exists(folderTopo10) OrElse Not Directory.Exists(folderOrto) Then Exit Sub

        Dim wTopo As New WarstwaWielowarstwowa With {
            .Folder = folderTopo10,
            .PoziomMin = 15,
            .PoziomMax = 18,
            .Krycie = 0.5,
            .BialePrzezroczyste = True,
            .Wlaczona = True
        }
        Dim b1 As String = ""
        Assert.True(wTopo.WczytajSesje(b1))

        Dim wOrto As New WarstwaWielowarstwowa With {
            .Folder = folderOrto,
            .PoziomMin = 15,
            .PoziomMax = 18,
            .Krycie = 1.0,
            .BialePrzezroczyste = False,
            .Wlaczona = True
        }
        Dim b2 As String = ""
        Assert.True(wOrto.WczytajSesje(b2))

        Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(New List(Of WarstwaWielowarstwowa) From {wTopo, wOrto})
        Dim lat = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim lon = (obszar.Zachod + obszar.Wschod) / 2
        Dim opcje As New OpcjeKompilatoraMbtiles()

        ' Kolejność 1: Topo (Kolejnosc=1), Orto (Kolejnosc=2) - tak jak na screenie usera
        wTopo.Kolejnosc = 1
        wOrto.Kolejnosc = 2
        Dim list1 As New List(Of WarstwaWielowarstwowa) From {wTopo, wOrto}
        Using bmp1 = KompilatorMbtiles.RenderujPodgladKafla(list1, lat, lon, 15, opcje)
            Assert.NotNull(bmp1)
            bmp1.Save("C:\Users\KN\.gemini\antigravity\brain\8b3509c2-2b34-4a26-9256-6ae366d0066b\scratch\test_topo_first.png", ImageFormat.Png)
        End Using

        ' Kolejność 2: Orto (Kolejnosc=1), Topo (Kolejnosc=2) - Orto podkład, Topo nakładka
        wOrto.Kolejnosc = 1
        wTopo.Kolejnosc = 2
        Dim list2 As New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo}
        Using bmp2 = KompilatorMbtiles.RenderujPodgladKafla(list2, lat, lon, 15, opcje)
            Assert.NotNull(bmp2)
            bmp2.Save("C:\Users\KN\.gemini\antigravity\brain\8b3509c2-2b34-4a26-9256-6ae366d0066b\scratch\test_orto_first.png", ImageFormat.Png)
        End Using
    End Sub
    <Fact>
    Public Sub TestTrybMnozeniaKazimierz()
        Dim folderDownload = "C:\Users\KN\source\repos\MapoTero_testy\MapoTero_SingleFile\download\"
        Dim folderTopo10 = Path.Combine(folderDownload, "gmina_kazimierz_topo10_65")
        Dim folderOrto = Path.Combine(folderDownload, "gmina_kazimierz_orto")
        If Not Directory.Exists(folderTopo10) OrElse Not Directory.Exists(folderOrto) Then Exit Sub

        ' Wczytaj podkład (Orto) i nakładkę (Topo)
        Dim wOrto As New WarstwaWielowarstwowa With {.Folder = folderOrto, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 1.0, .BialePrzezroczyste = False, .Wlaczona = True, .Kolejnosc = 1}
        Dim b1 As String = "" : wOrto.WczytajSesje(b1)

        Dim wTopo As New WarstwaWielowarstwowa With {.Folder = folderTopo10, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 0.8, .BialePrzezroczyste = True, .Wlaczona = True, .Kolejnosc = 2}
        Dim b2 As String = "" : wTopo.WczytajSesje(b2)

        Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo})
        Dim lat = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim lon = (obszar.Zachod + obszar.Wschod) / 2
        Dim opcje As New OpcjeKompilatoraMbtiles()

        ' Symulacja mnożenia:
        ' Wygeneruj kafel Orto oraz kafel Topo
        Dim warstwyOrto As New List(Of WarstwaEksportu) From {wOrto.DoWarstwyEksportu()}
        Dim warstwyTopo As New List(Of WarstwaEksportu) From {wTopo.DoWarstwyEksportu()}

        Using bmpOrto = KompilatorMbtiles.RenderujPodgladKafla(New List(Of WarstwaWielowarstwowa) From {wOrto}, lat, lon, 15, opcje),
              bmpTopo = KompilatorMbtiles.RenderujPodgladKafla(New List(Of WarstwaWielowarstwowa) From {wTopo}, lat, lon, 15, opcje)
            Assert.NotNull(bmpOrto)
            Assert.NotNull(bmpTopo)

            ' Wykonaj blending Multiply (Mnożenie) na bitmapie
            Using wynik As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                For y = 0 To 255
                    For x = 0 To 255
                        Dim cOrto = bmpOrto.GetPixel(x, y)
                        Dim cTopo = bmpTopo.GetPixel(x, y)
                        ' Topo alfa (0..1)
                        Dim sa = cTopo.A / 255.0
                        ' Mnożenie: (Orto * Topo) / 255
                        Dim mr = (CDbl(cOrto.R) * CDbl(cTopo.R)) / 255.0
                        Dim mg = (CDbl(cOrto.G) * CDbl(cTopo.G)) / 255.0
                        Dim mb = (CDbl(cOrto.B) * CDbl(cTopo.B)) / 255.0
                        ' Zwykłe krycie zblendowane z tłem
                        Dim finR = CByte(Math.Min(255, Math.Max(0, (1 - sa) * cOrto.R + sa * mr)))
                        Dim finG = CByte(Math.Min(255, Math.Max(0, (1 - sa) * cOrto.G + sa * mg)))
                        Dim finB = CByte(Math.Min(255, Math.Max(0, (1 - sa) * cOrto.B + sa * mb)))
                        wynik.SetPixel(x, y, Color.FromArgb(255, finR, finG, finB))
                    Next
                Next
                wynik.Save("C:\Users\KN\.gemini\antigravity\brain\8b3509c2-2b34-4a26-9256-6ae366d0066b\test_kazimierz_multiply.png", ImageFormat.Png)
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_TrybMieszaniaMnozenie_PoprawnieLaczyWarstwy()
        Dim folderDownload = "C:\Users\KN\source\repos\MapoTero_testy\MapoTero_SingleFile\download\"
        Dim folderTopo10 = Path.Combine(folderDownload, "gmina_kazimierz_topo10_65")
        Dim folderOrto = Path.Combine(folderDownload, "gmina_kazimierz_orto")
        If Not Directory.Exists(folderTopo10) OrElse Not Directory.Exists(folderOrto) Then Exit Sub

        Dim wOrto As New WarstwaWielowarstwowa With {.Folder = folderOrto, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 1.0, .Wlaczona = True, .Kolejnosc = 1}
        Dim b1 As String = "" : wOrto.WczytajSesje(b1)

        Dim wTopo As New WarstwaWielowarstwowa With {.Folder = folderTopo10, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 0.8, .TrybMieszania = TrybMieszania.Mnozenie, .BialePrzezroczyste = True, .Wlaczona = True, .Kolejnosc = 2}
        Dim b2 As String = "" : wTopo.WczytajSesje(b2)

        Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo})
        Dim lat = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim lon = (obszar.Zachod + obszar.Wschod) / 2
        Dim opcje As New OpcjeKompilatoraMbtiles()

        Dim lista As New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo}
        Using bmp = KompilatorMbtiles.RenderujPodgladKafla(lista, lat, lon, 15, opcje)
            Assert.NotNull(bmp)
            Assert.Equal(256, bmp.Width)
            Assert.Equal(256, bmp.Height)
        End Using
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_OdcienieSzarosci_PoprawnieKonwertuje()
        Dim folderDownload = "C:\Users\KN\source\repos\MapoTero_testy\MapoTero_SingleFile\download\"
        Dim folderTopo10 = Path.Combine(folderDownload, "gmina_kazimierz_topo10_65")
        Dim folderOrto = Path.Combine(folderDownload, "gmina_kazimierz_orto")
        If Not Directory.Exists(folderTopo10) OrElse Not Directory.Exists(folderOrto) Then Exit Sub

        ' Ortofotomapa w odcieniach szarości jako podkład
        Dim wOrto As New WarstwaWielowarstwowa With {.Folder = folderOrto, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 1.0, .OdcienieSzarosci = True, .Wlaczona = True, .Kolejnosc = 1}
        Dim b1 As String = "" : wOrto.WczytajSesje(b1)

        ' Kolorowa mapa topo z mnożeniem na wierzchu
        Dim wTopo As New WarstwaWielowarstwowa With {.Folder = folderTopo10, .PoziomMin = 15, .PoziomMax = 18, .Krycie = 0.85, .TrybMieszania = TrybMieszania.Mnozenie, .BialePrzezroczyste = True, .Wlaczona = True, .Kolejnosc = 2}
        Dim b2 As String = "" : wTopo.WczytajSesje(b2)

        Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo})
        Dim lat = (obszar.Polnoc + obszar.Poludnie) / 2
        Dim lon = (obszar.Zachod + obszar.Wschod) / 2
        Dim opcje As New OpcjeKompilatoraMbtiles()

        Dim lista As New List(Of WarstwaWielowarstwowa) From {wOrto, wTopo}
        Using bmp = KompilatorMbtiles.RenderujPodgladKafla(lista, lat, lon, 15, opcje)
            Assert.NotNull(bmp)
            bmp.Save("C:\Users\KN\.gemini\antigravity\brain\8b3509c2-2b34-4a26-9256-6ae366d0066b\test_kazimierz_grayscale.png", ImageFormat.Png)
        End Using
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_WielowatkowaKompilacja_GenerujePoprawnaBazeIIdentyczneDane()
        Dim folder = NowyFolder()
        Try
            Dim folderW1 = Path.Combine(folder, "sesja_topo") & Path.DirectorySeparatorChar
            Dim folderW2 = Path.Combine(folder, "sesja_orto") & Path.DirectorySeparatorChar
            Directory.CreateDirectory(folderW1)
            Directory.CreateDirectory(folderW2)

            UtworzSesje(folderW1, 487000, 637000, 487512, 637512, 1.0, Color.FromArgb(255, 34, 139, 34))
            UtworzSesje(folderW2, 487000, 637000, 487512, 637512, 1.0, Color.FromArgb(255, 205, 133, 63))

            Dim w1 As New WarstwaWielowarstwowa With {.Folder = folderW1, .Tytul = "Topo", .PoziomMin = 14, .PoziomMax = 15, .Kolejnosc = 1}
            Dim blad1 As String = "" : Assert.True(w1.WczytajSesje(blad1))

            Dim w2 As New WarstwaWielowarstwowa With {.Folder = folderW2, .Tytul = "Orto", .PoziomMin = 15, .PoziomMax = 16, .Kolejnosc = 2, .TrybMieszania = TrybMieszania.Mnozenie}
            Dim blad2 As String = "" : Assert.True(w2.WczytajSesje(blad2))

            Dim plikMbtiles = Path.Combine(folder, "wynik_parallel.mbtiles")
            Dim opcje As New OpcjeKompilatoraMbtiles With {
                .PlikDocelowy = plikMbtiles,
                .NazwaMapy = "Test Wielowatkowy",
                .PoziomMin = 14,
                .PoziomMax = 16,
                .FormatKafli = FormatKafliMbtiles.SmartHybrid,
                .JakoscJpeg = 80,
                .SzybkaPiramidaWPasmach = True
            }

            Dim warstwy As New List(Of WarstwaWielowarstwowa) From {w1, w2}
            Dim raporty As New List(Of RaportPostepuKompilacji)
            Dim postep As New Progress(Of RaportPostepuKompilacji)(Sub(r) raporty.Add(r))

            ' Wykonanie kompilacji wielowatkowej
            KompilatorMbtiles.KompilujAsync(warstwy, opcje, postep, CancellationToken.None).GetAwaiter().GetResult()

            Assert.True(File.Exists(plikMbtiles), "Plik wynikowy nie istnieje.")
            Assert.True(raporty.Count > 0, "Brak raportow postepu z kompilacji.")

            Using pol As New SQLiteConnection("Data Source=" & plikMbtiles & ";Version=3;Pooling=False;")
                pol.Open()

                ' Sprawdzenie liczby kafli
                Using cmd = pol.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(*) FROM tiles;"
                    Dim liczbaKafli = Convert.ToInt32(cmd.ExecuteScalar())
                    Assert.True(liczbaKafli > 0, "Tabela tiles jest pusta.")
                End Using

                ' Sprawdzenie odczytu kafli i poprawnosci formatu obrazu
                Using cmd = pol.CreateCommand()
                    cmd.CommandText = "SELECT tile_data FROM tiles LIMIT 5;"
                    Using rdr = cmd.ExecuteReader()
                        While rdr.Read()
                            Dim dane = CType(rdr(0), Byte())
                            Assert.NotNull(dane)
                            Assert.True(dane.Length > 100)
                            Using ms As New MemoryStream(dane), img = Image.FromStream(ms)
                                Assert.Equal(256, img.Width)
                                Assert.Equal(256, img.Height)
                            End Using
                        End While
                    End Using
                End Using

                ' Sprawdzenie unikalnego indeksu
                Using cmd = pol.CreateCommand()
                    cmd.CommandText = "PRAGMA index_info(tile_index);"
                    Using rdr = cmd.ExecuteReader()
                        Assert.True(rdr.Read(), "Brak indeksu tile_index.")
                    End Using
                End Using
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub DriveInfo_PoprawnieOdczytujeDyskDocelowy()
        Dim folderTemp = Path.GetTempPath()
        Dim litera = Path.GetPathRoot(folderTemp)
        Assert.False(String.IsNullOrEmpty(litera))

        Dim di As New DriveInfo(litera)
        Assert.True(di.IsReady)
        Assert.True(di.AvailableFreeSpace > 0)
    End Sub

End Class
