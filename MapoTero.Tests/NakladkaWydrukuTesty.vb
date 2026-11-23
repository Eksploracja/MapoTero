Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.IO
Imports MapoTero
Imports Xunit

Public Class NakladkaWydrukuTesty

    <Fact>
    Public Sub DobierzKrokSiatki_DlaRoznychSzerokosci_ZwracaSensownyKrok()
        ' Arkusz 8000m szerokości: docelowy = 8000 / 8 = 1000m -> krok 1000
        Dim arkusz1 As New ZakresArkusza() With {
            .SzerokoscPx = 4000,
            .WysokoscPx = 3000,
            .RozmiarPikselaM = 2.0,
            .LewyEastingM = 500000.0,
            .GornyNorthingM = 600000.0
        }
        Assert.Equal(1000, RendererNakladekWydruku.DobierzKrokSiatki(arkusz1))

        ' Arkusz 2000m szerokości: docelowy = 2000 / 8 = 250m -> krok 250
        Dim arkusz2 As New ZakresArkusza() With {
            .SzerokoscPx = 2000,
            .WysokoscPx = 2000,
            .RozmiarPikselaM = 1.0,
            .LewyEastingM = 500000.0,
            .GornyNorthingM = 600000.0
        }
        Assert.Equal(250, RendererNakladekWydruku.DobierzKrokSiatki(arkusz2))

        ' Arkusz 50 000m szerokości: docelowy = 50 000 / 8 = 6250m -> krok 10000
        Dim arkusz3 As New ZakresArkusza() With {
            .SzerokoscPx = 5000,
            .WysokoscPx = 5000,
            .RozmiarPikselaM = 10.0,
            .LewyEastingM = 500000.0,
            .GornyNorthingM = 600000.0
        }
        Assert.Equal(10000, RendererNakladekWydruku.DobierzKrokSiatki(arkusz3))
    End Sub

    <Theory>
    <InlineData(500000.0, 1000, False, "500")>
    <InlineData(500500.0, 500, False, "500.5")>
    <InlineData(500000.0, 1000, True, "500 000")>
    <InlineData(450.0, 100, False, "450 m")>
    Public Sub FormatujEtykieteWspolrzednej_FormatyZgodneZTypem(wartosc As Double, krok As Integer, pelneMetry As Boolean, oczekiwany As String)
        Dim wynik As String = RendererNakladekWydruku.FormatujEtykieteWspolrzednej(wartosc, krok, pelneMetry)
        Assert.Equal(oczekiwany, wynik)
    End Sub

    <Fact>
    Public Sub ZakresArkusza_MetryNaPiksel_PrzeliczaNiezaleznieOdRozdzielczosci()
        Dim arkusz As New ZakresArkusza() With {
            .SzerokoscPx = 1000,
            .WysokoscPx = 800,
            .RozmiarPikselaM = 2.0,
            .LewyEastingM = 200000.0,
            .GornyNorthingM = 400000.0
        }

        ' Lewy górny róg
        Dim pLG = arkusz.MetryNaPiksel(200000.0, 400000.0)
        Assert.Equal(0.0F, pLG.X)
        Assert.Equal(0.0F, pLG.Y)

        ' Prawy dolny róg: Easting = 200000 + 1000 * 2 = 202000, Northing = 400000 - 800 * 2 = 398400
        Dim pPD = arkusz.MetryNaPiksel(202000.0, 398400.0)
        Assert.Equal(1000.0F, pPD.X)
        Assert.Equal(800.0F, pPD.Y)
    End Sub

    <Fact>
    Public Sub WczytajSegmentySladu_Gpx_ZwracaSegmentyIZnaczniki()
        Dim tempGpx As String = Path.Combine(Path.GetTempPath(), "test_" & Guid.NewGuid().ToString("N") & ".gpx")
        Try
            Dim xml As String = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf &
                "<gpx version=""1.1"" creator=""Test"">" & vbCrLf &
                "  <trk><trkseg>" & vbCrLf &
                "    <trkpt lat=""52.1"" lon=""21.0""></trkpt>" & vbCrLf &
                "    <trkpt lat=""52.2"" lon=""21.1""></trkpt>" & vbCrLf &
                "  </trkseg></trk>" & vbCrLf &
                "</gpx>"
            File.WriteAllText(tempGpx, xml)

            Dim segmenty = RendererNakladekWydruku.WczytajSegmentySladu(tempGpx)
            Assert.Single(segmenty)
            Dim punkty = segmenty(0)
            Assert.Equal(2, punkty.Count)
            Assert.Equal(21.0, punkty(0).Lon)
            Assert.Equal(52.1, punkty(0).Lat)
            Assert.Equal(21.1, punkty(1).Lon)
            Assert.Equal(52.2, punkty(1).Lat)
        Finally
            If File.Exists(tempGpx) Then File.Delete(tempGpx)
        End Try
    End Sub

    <Fact>
    Public Sub WczytajSegmentySladu_Kml_ZwracaSegmenty()
        Dim tempKml As String = Path.Combine(Path.GetTempPath(), "test_" & Guid.NewGuid().ToString("N") & ".kml")
        Try
            Dim xml As String = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf &
                "<kml xmlns=""http://www.opengis.net/kml/2.2"">" & vbCrLf &
                "  <Placemark>" & vbCrLf &
                "    <LineString>" & vbCrLf &
                "      <coordinates>19.1,51.1,0 19.2,51.2,0</coordinates>" & vbCrLf &
                "    </LineString>" & vbCrLf &
                "  </Placemark>" & vbCrLf &
                "</kml>"
            File.WriteAllText(tempKml, xml)

            Dim segmenty = RendererNakladekWydruku.WczytajSegmentySladu(tempKml)
            Assert.Single(segmenty)
            Dim punkty = segmenty(0)
            Assert.Equal(2, punkty.Count)
            Assert.Equal(19.1, punkty(0).Lon)
            Assert.Equal(51.1, punkty(0).Lat)
        Finally
            If File.Exists(tempKml) Then File.Delete(tempKml)
        End Try
    End Sub

    <Fact>
    Public Sub RysujSiatkeKilometrowa_NaBitmapie_NieRzucaWyjatkow()
        Using bmp As New Bitmap(500, 500)
            Using g = Graphics.FromImage(bmp)
                Dim arkusz As New ZakresArkusza() With {
                    .SzerokoscPx = 500,
                    .WysokoscPx = 500,
                    .RozmiarPikselaM = 2.0,
                    .LewyEastingM = 500000.0,
                    .GornyNorthingM = 600000.0
                }
                Dim parametry As New ParametrySiatkiKilometrowej() With {
                    .KrokMetry = 200
                }

                RendererNakladekWydruku.RysujSiatkeKilometrowa(g, arkusz, parametry, offsetPasaY:=0)
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub ZakresArkusza_NiesymetrycznyPiksel_ZachowujeGeometrie()
        Dim arkusz As New ZakresArkusza() With {
            .SzerokoscPx = 1000,
            .WysokoscPx = 500,
            .RozmiarPikselaXM = 2.0,
            .RozmiarPikselaYM = 4.0,
            .LewyEastingM = 200000.0,
            .GornyNorthingM = 400000.0
        }

        Dim pPD = arkusz.MetryNaPiksel(202000.0, 398000.0)
        Assert.Equal(1000.0F, pPD.X)
        Assert.Equal(500.0F, pPD.Y)
    End Sub

    <Fact>
    Public Sub RysujSiatkeKilometrowa_TransformacjaSiatki_DzialaPoprawnie()
        Using bmp As New Bitmap(600, 600)
            Using g = Graphics.FromImage(bmp)
                Dim arkusz As New ZakresArkusza() With {
                    .SzerokoscPx = 600,
                    .WysokoscPx = 600,
                    .RozmiarPikselaXM = 2.0,
                    .RozmiarPikselaYM = 2.0,
                    .LewyEastingM = 500000.0,
                    .GornyNorthingM = 600000.0
                }
                Dim parametry As New ParametrySiatkiKilometrowej() With {
                    .UkladSiatki = UkladSiatkiWydruku.Utm34N,
                    .KrokMetry = 500
                }

                ' Transformacja np. z przesunięciem
                Dim transformacja As Func(Of Double, Double, (E As Double, N As Double)) =
                    Function(e, n) (e + 100.0, n + 200.0)
                Dim zakres = Tuple.Create(499000.0, 502000.0, 599000.0, 602000.0)

                RendererNakladekWydruku.RysujSiatkeKilometrowa(g, arkusz, parametry, offsetPasaY:=0, przeliczZUkSiatkiNaArkusz:=transformacja, zakresMetrowySiatki:=zakres)
            End Using
        End Using
    End Sub

End Class
