'Copyright (C) <2026>  pajakt
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

Public Class GeoPackageTesty

    Private Shared Function Siatka2x2() As Siatka
        Return New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
    End Function

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_GPKG_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Sub Usun(folder As String)
        If Not Directory.Exists(folder) Then Exit Sub
        Try
            Directory.Delete(folder, True)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            SQLiteConnection.ClearAllPools()
            GC.Collect()
            GC.WaitForPendingFinalizers()
            Directory.Delete(folder, True)
        End Try
    End Sub

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
        Dim m As New MetadaneMapy With {.Tytul = "Test warstwa GPKG", .DataPobrania = New Date(2026, 11, 21)}
        m.Warstwy.Add("Test warstwa")
        m.DodajZrodlo("Geoportal")
        Return New ZadanieEksportu With {
            .Rodzaj = rodzaj,
            .Zrodlo = zrodlo,
            .Uklad = UkladWspolrzednych.PL1992,
            .Plik = plik,
            .PoziomMin = 15,
            .PoziomMax = 16,
            .JakoscJpeg = 90,
            .Metadane = m,
            .Nazwa = m.Nazwa
        }
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

    Private Shared Function PikselKafla(p As PunktXY, z As Integer) As Tuple(Of Integer, Integer, Integer, Integer)
        Dim g = UkladWspolrzednych.PL1992.DoWgs84(p)
        Dim px = WebMercator.PikselX(g.Dlugosc, z), py = WebMercator.PikselY(g.Szerokosc, z)
        Dim tx = CInt(Math.Floor(px / 256)), ty = CInt(Math.Floor(py / 256))
        Return Tuple.Create(tx, ty, CInt(Math.Floor(px - tx * 256)), CInt(Math.Floor(py - ty * 256)))
    End Function

    <Fact>
    Public Sub GeoPackage_MetadaneIEnumy()
        Assert.Equal("GeoPackage", ZarzadcaSzablonow.NazwaFormatu(RodzajEksportu.GeoPackage))
        Assert.Equal(".gpkg", ZarzadcaSzablonow.RozszerzenieDlaRodzaju(RodzajEksportu.GeoPackage))
        Assert.True(ZarzadcaSzablonow.CzyFormatWielopoziomowy(RodzajEksportu.GeoPackage))
        'kreator nie oferuje GeoPackage, więc nie łączy w nim skal
        Assert.False(StylKreatora.FormatWieloskalowy(RodzajEksportu.GeoPackage))
        Dim wskazowka = ZarzadcaSzablonow.WskazowkaWgrania(RodzajEksportu.GeoPackage)
        Assert.Contains("QField", wskazowka)
        Assert.DoesNotContain("Locus", wskazowka)
    End Sub

    <Fact>
    Public Sub MagazynGeoPackage_StrukturaBazyIPragmy()
        Dim folder = NowyFolder()
        Try
            Dim plik = Path.Combine(folder, "test.gpkg")
            Dim obszar = New ProstokatGeo(52.3, 52.1, 21.2, 21.0)
            Dim opis As New OpisMbtiles With {
                .Nazwa = "Testowa Mapa GPKG",
                .Opis = "Opis testowy GeoPackage",
                .Atrybucja = "© GUGiK",
                .PoziomMin = 14,
                .PoziomMax = 16,
                .Obszar = obszar,
                .Png = False
            }

            Dim probka(15) As Byte
            For i = 0 To probka.Length - 1
                probka(i) = CByte(i * 11)
            Next

            Using m As New MagazynGeoPackage(plik, opis)
                m.RozpocznijPoziom(16)
                ' W GeoPackage KafelXYZ(x=35000, y=22000, z=16) zapisuje się wprost jako tile_row=22000, tile_column=35000, zoom_level=16
                Dim k As New KafelXYZ(35000, 22000, 16)
                m.Zapisz(k, probka)
                Dim odczyt = m.Odczytaj(k)
                Assert.Equal(probka, odczyt)
                m.ZakonczPoziom()
                m.ZakonczZapis()
            End Using

            Using pol = Polacz(plik)
                ' 1. Weryfikacja pragma OGC GPKG 1.3
                Dim appId = CLng(Zapytanie(pol, "PRAGMA application_id;"))
                Assert.Equal(1196444487L, appId) ' 0x47504B47 = "GPKG"

                Dim userVer = CLng(Zapytanie(pol, "PRAGMA user_version;"))
                Assert.Equal(10300L, userVer) ' GeoPackage 1.3

                Dim journalMode = CStr(Zapytanie(pol, "PRAGMA journal_mode;")).ToLowerInvariant()
                Assert.Equal("delete", journalMode)

                ' 2. Układy odniesienia (gpkg_spatial_ref_sys)
                Assert.True(CInt(Zapytanie(pol, "SELECT COUNT(*) FROM gpkg_spatial_ref_sys WHERE srs_id = -1")) = 1)
                Assert.True(CInt(Zapytanie(pol, "SELECT COUNT(*) FROM gpkg_spatial_ref_sys WHERE srs_id = 0")) = 1)
                Assert.True(CInt(Zapytanie(pol, "SELECT COUNT(*) FROM gpkg_spatial_ref_sys WHERE srs_id = 4326")) = 1)
                Assert.True(CInt(Zapytanie(pol, "SELECT COUNT(*) FROM gpkg_spatial_ref_sys WHERE srs_id = 3857")) = 1)

                ' 3. Tabela zawartości (gpkg_contents)
                Assert.Equal("tiles", Zapytanie(pol, "SELECT table_name FROM gpkg_contents WHERE table_name = 'tiles'"))
                Assert.Equal("tiles", Zapytanie(pol, "SELECT data_type FROM gpkg_contents WHERE table_name = 'tiles'"))
                Assert.Equal("Testowa Mapa GPKG", Zapytanie(pol, "SELECT identifier FROM gpkg_contents WHERE table_name = 'tiles'"))
                Assert.Equal(3857, CInt(Zapytanie(pol, "SELECT srs_id FROM gpkg_contents WHERE table_name = 'tiles'")))
                'atrybucja w opisie warstwy - GeoPackage nie ma osobnego pola
                Assert.Equal("Opis testowy GeoPackage Źródło: © GUGiK", Zapytanie(pol, "SELECT description FROM gpkg_contents WHERE table_name = 'tiles'"))

                ' 4. Zestaw macierzy kafli (gpkg_tile_matrix_set)
                Assert.Equal("tiles", Zapytanie(pol, "SELECT table_name FROM gpkg_tile_matrix_set WHERE table_name = 'tiles'"))
                Assert.Equal(3857, CInt(Zapytanie(pol, "SELECT srs_id FROM gpkg_tile_matrix_set WHERE table_name = 'tiles'")))

                ' 5. Macierze kafli dla poziomów 14, 15, 16 (gpkg_tile_matrix)
                Dim liczbaPoziomow = CInt(Zapytanie(pol, "SELECT COUNT(*) FROM gpkg_tile_matrix WHERE table_name = 'tiles'"))
                Assert.Equal(3, liczbaPoziomow)
                Dim matrix16 = CInt(Zapytanie(pol, "SELECT matrix_width FROM gpkg_tile_matrix WHERE table_name = 'tiles' AND zoom_level = 16"))
                Assert.Equal(1 << 16, matrix16)

                ' 6. Tabela tiles: klucz id, kolumny i wiersze
                Dim countTiles = CInt(Zapytanie(pol, "SELECT COUNT(*) FROM tiles"))
                Assert.Equal(1, countTiles)
                Dim id = CInt(Zapytanie(pol, "SELECT id FROM tiles LIMIT 1"))
                Assert.True(id > 0)
                Dim col = CInt(Zapytanie(pol, "SELECT tile_column FROM tiles LIMIT 1"))
                Dim row = CInt(Zapytanie(pol, "SELECT tile_row FROM tiles LIMIT 1"))
                Assert.Equal(35000, col)
                Assert.Equal(22000, row) ' Top-Left XYZ, a NIE odwrócony TMS

                Dim blob = TryCast(Zapytanie(pol, "SELECT tile_data FROM tiles LIMIT 1"), Byte())
                Assert.Equal(probka, blob)

                ' 7. Tabela android_metadata
                Assert.Equal("pl_PL", Zapytanie(pol, "SELECT locale FROM android_metadata"))
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

    <Fact>
    Public Sub GeoPackage_EksportujePiramidyKafli()
        Dim folder = NowyFolder()
        Try
            ' Czerwone segmenty
            UtworzSegmenty(folder, "png", Sub(g, w, k) g.Clear(Color.Red))
            Dim plik = Path.Combine(folder, "mapa.gpkg")
            Dim z = Zadanie(RodzajEksportu.GeoPackage, Zrodlo(folder, "png"), plik)
            z.PoziomMin = 15
            z.PoziomMax = 16

            Dim wyniki = EksportMapy.EksportujBezpiecznie(z, Nothing, CancellationToken.None)
            Assert.Equal(plik, Assert.Single(wyniki))
            Assert.True(File.Exists(plik))

            Using pol = Polacz(plik)
                ' Weryfikacja pragma
                Assert.Equal(1196444487L, CLng(Zapytanie(pol, "PRAGMA application_id;")))
                Assert.Equal(10300L, CLng(Zapytanie(pol, "PRAGMA user_version;")))

                ' Sprawdzenie liczby kafli na poziomach 16 i 15
                Dim kafle16 = CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles WHERE zoom_level = 16"))
                Dim kafle15 = CLng(Zapytanie(pol, "SELECT COUNT(*) FROM tiles WHERE zoom_level = 15"))
                Assert.True(kafle16 > 0)
                Assert.True(kafle15 > 0)

                ' Sprawdzenie poprawnego koloru piksela w kaflu poziomu 16
                Dim seg = Siatka2x2().Segment(1, 1)
                Dim punkt = New PunktXY(seg.XDol + 128, seg.YLewy + 128)
                Dim t = PikselKafla(punkt, 16)

                ' GeoPackage używa tile_row = t.Item2 (wiersze rosną w dół od północy)
                Dim dane = TryCast(Zapytanie(pol, $"SELECT tile_data FROM tiles WHERE zoom_level = 16 AND tile_column = {t.Item1} AND tile_row = {t.Item2}"), Byte())
                Assert.NotNull(dane)
                Using bmp = Obraz(dane)
                    Dim kolor = bmp.GetPixel(t.Item3, t.Item4)
                    Assert.InRange(CInt(kolor.R), 230, 255)
                    Assert.InRange(CInt(kolor.G), 0, 25)
                    Assert.InRange(CInt(kolor.B), 0, 25)
                End Using
            End Using
        Finally
            Usun(folder)
        End Try
    End Sub

End Class
