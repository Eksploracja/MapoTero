Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

Public Class FormKonwerterGeoTiff

    Private _sciezkaTif As String = String.Empty
    Private _ostatniWynik As String = String.Empty
    Private _przerwanie As CancellationTokenSource
    Private _zamknijPoZakonczeniu As Boolean

    Public Sub New()
        InitializeComponent()
        PomocProgramu.ZamykanieKlawiszemEsc(Me)
    End Sub

    Private Sub FormKonwerterGeoTiff_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WyborUkladuGeoTiff.WypelnijListeCrs(cmbUkladCrs, Nothing, lblCrsStatus)
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _przerwanie IsNot Nothing Then
            _przerwanie.Cancel()
            _zamknijPoZakonczeniu = True
            btnZamknij.Enabled = False
            lblStatus.Text = "Trwa przerywanie operacji..."
            e.Cancel = True
            Return
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub FormKonwerterGeoTiff_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        If _przerwanie IsNot Nothing Then
            e.Effect = DragDropEffects.None
            Return
        End If
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
                If ext = ".tif" OrElse ext = ".tiff" OrElse ext = ".geotiff" Then
                    e.Effect = DragDropEffects.Copy
                    Return
                End If
            End If
        End If
        e.Effect = DragDropEffects.None
    End Sub

    Private Sub FormKonwerterGeoTiff_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        If _przerwanie IsNot Nothing Then Return
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                OtworzPlik(pliki(0))
            End If
        End If
    End Sub

    Private Sub btnWybierzTif_Click(sender As Object, e As EventArgs) Handles btnWybierzTif.Click
        If dlgOtworz.ShowDialog(Me) = DialogResult.OK Then
            OtworzPlik(dlgOtworz.FileName)
        End If
    End Sub

    Private Sub OtworzPlik(sciezka As String)
        Try
            Using czytnik As New CzytnikGeoTiff(sciezka)
                _sciezkaTif = sciezka
                txtPlikTif.Text = sciezka
                lblInfoWymiary.Text = String.Format("Wymiary: {0} x {1} px ({2})", czytnik.Szerokosc, czytnik.Wysokosc,
                                                    If(czytnik.CzyKafelkowany, "kafelkowany", "pasy"))

                Dim geo = czytnik.Georeferencja
                Dim mPerPx = Math.Sqrt(geo.A * geo.A + geo.D * geo.D)

                WyborUkladuGeoTiff.WypelnijListeCrs(cmbUkladCrs, geo.KodEpsg, lblCrsStatus)
                Dim wybranyCrs = TryCast(cmbUkladCrs.SelectedItem, WyborUkladuGeoTiff.PozycjaCrs)
                Dim epsg = If(wybranyCrs IsNot Nothing, wybranyCrs.Epsg, If(geo.KodEpsg > 0, geo.KodEpsg, 2180))

                Dim p0 = geo.PikselNaWspolrzedne(0, 0)
                Dim p1 = geo.PikselNaWspolrzedne(czytnik.Szerokosc, czytnik.Wysokosc)
                lblInfoZasieg.Text = String.Format(CultureInfo.InvariantCulture, "Zasięg: ({0:0.0}, {1:0.0}) do ({2:0.0}, {3:0.0})", p0.X, p0.Y, p1.X, p1.Y)

                Dim mPerPxMetry As Double = mPerPx
                If epsg = 4326 Then
                    Dim mSz As Double, mDl As Double
                    Georeferencja.MetrowNaStopien((p0.Y + p1.Y) / 2.0, mSz, mDl)
                    mPerPxMetry = Math.Sqrt((geo.A * mDl) ^ 2 + (geo.D * mSz) ^ 2)
                End If
                If mPerPxMetry > 0 Then
                    lblInfoRozdzielczosc.Text = String.Format(CultureInfo.InvariantCulture, "Rozdzielczość terenowa: ~{0:0.##} m/px", mPerPxMetry)
                Else
                    lblInfoRozdzielczosc.Text = "Rozdzielczość terenowa: nieokreślona"
                End If

                ' Dobór poziomów zoom
                If mPerPxMetry <= 0.01 OrElse mPerPxMetry >= 50000.0 Then mPerPxMetry = 2.5
                Dim zMax = CInt(Math.Round(Math.Log(156543.03392 / mPerPxMetry) / Math.Log(2)))
                nudZoomDo.Value = Math.Max(1, Math.Min(20, zMax))
                nudZoomOd.Value = Math.Max(1, Math.Min(zMax, Math.Max(1, zMax - 4)))

                btnKonwertuj.Enabled = True
                lblStatus.Text = "Plik GeoTIFF gotowy do konwersji."
            End Using
        Catch ex As GeoTiffWysokosciowyException
            MessageBox.Show(Me, "Wskazany plik GeoTIFF to numeryczny model terenu (NMT/DEM), a nie mapa rastrowa." & vbCrLf & ex.Message,
                            "Plik NMT/DEM", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            MessageBox.Show(Me, "Nie udało się odczytać pliku GeoTIFF:" & vbCrLf & OpisBledu.Z(ex),
                            "Błąd odczytu", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub rbFormat_CheckedChanged(sender As Object, e As EventArgs) Handles rbMbtiles.CheckedChanged, rbGeoPackage.CheckedChanged,
                                                                                  rbKmz.CheckedChanged, rbJnx.CheckedChanged,
                                                                                  rbSqlitedb.CheckedChanged, rbOruxMaps.CheckedChanged
        chkPodzialKmz.Visible = rbKmz.Checked
    End Sub

    Private Async Sub btnKonwertuj_Click(sender As Object, e As EventArgs) Handles btnKonwertuj.Click
        If String.IsNullOrEmpty(_sciezkaTif) OrElse Not File.Exists(_sciezkaTif) Then Return

        Dim wybranyCrs = TryCast(cmbUkladCrs.SelectedItem, WyborUkladuGeoTiff.PozycjaCrs)
        Dim epsg = If(wybranyCrs IsNot Nothing, wybranyCrs.Epsg, 2180)

        Dim ext As String = ".mbtiles"
        Dim rodzaj As RodzajEksportu = RodzajEksportu.MBTiles
        If rbGeoPackage.Checked Then
            ext = ".gpkg"
            rodzaj = RodzajEksportu.GeoPackage
        ElseIf rbKmz.Checked Then
            ext = ".kmz"
            rodzaj = RodzajEksportu.Kmz
        ElseIf rbJnx.Checked Then
            ext = ".jnx"
            rodzaj = RodzajEksportu.Jnx
        ElseIf rbSqlitedb.Checked Then
            ext = ".sqlitedb"
            rodzaj = RodzajEksportu.SQLiteDB
        ElseIf rbOruxMaps.Checked Then
            ext = "_oruxmaps"
            rodzaj = RodzajEksportu.OruxMaps
        End If

        Dim folder = Path.GetDirectoryName(_sciezkaTif)
        Dim nazwaBazy = Path.GetFileNameWithoutExtension(_sciezkaTif)
        Dim plikWyjsciowy = Path.Combine(folder, nazwaBazy & ext)

        _przerwanie = New CancellationTokenSource()
        Dim token = _przerwanie.Token

        btnKonwertuj.Enabled = False
        btnWybierzTif.Enabled = False
        grpFormat.Enabled = False
        grpOpcje.Enabled = False
        btnPokazWFolderze.Enabled = False
        btnZamknij.Enabled = True
        btnZamknij.Text = "Przerwij"
        prgPostep.Value = 0

        Dim z1 = CInt(nudZoomOd.Value)
        Dim z2 = CInt(nudZoomDo.Value)
        Dim zOd = Math.Min(z1, z2)
        Dim zDo = Math.Max(z1, z2)
        Dim jakosc = CInt(nudJakosc.Value)
        Dim podzialKmz = chkPodzialKmz.Checked

        Dim postep As New Progress(Of Integer)(
            Sub(p)
                If IsDisposed OrElse Disposing Then Return
                prgPostep.Value = Math.Max(0, Math.Min(100, p))
                lblStatus.Text = String.Format("Trwa konwersja... {0}%", prgPostep.Value)
            End Sub)

        Try
            Await Task.Run(
                Sub()
                    KonwertujPlikGeoTiff(_sciezkaTif, plikWyjsciowy, rodzaj, epsg, zOd, zDo, jakosc, podzialKmz, postep, token)
                End Sub, token)

            If IsDisposed OrElse Disposing Then Return
            _ostatniWynik = plikWyjsciowy
            lblStatus.Text = "Konwersja zakończona sukcesem!"
            prgPostep.Value = 100
            btnPokazWFolderze.Enabled = True
            If Not _zamknijPoZakonczeniu Then
                MessageBox.Show(Me, "Konwersja pliku GeoTIFF została pomyślnie zakończona." & vbCrLf & vbCrLf & "Plik wynikowy:" & vbCrLf & plikWyjsciowy,
                                "Konwersja zakończona", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As OperationCanceledException
            If Not IsDisposed AndAlso Not Disposing Then lblStatus.Text = "Konwersja została przerwana przez użytkownika."
        Catch ex As Exception
            If Not IsDisposed AndAlso Not Disposing Then
                lblStatus.Text = "Błąd: " & OpisBledu.Z(ex)
                If Not _zamknijPoZakonczeniu Then
                    MessageBox.Show(Me, "Wystąpił błąd podczas konwersji:" & vbCrLf & OpisBledu.Z(ex),
                                    "Błąd konwersji", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        Finally
            If _przerwanie IsNot Nothing Then
                _przerwanie.Dispose()
                _przerwanie = Nothing
            End If
            If Not IsDisposed AndAlso Not Disposing Then
                btnKonwertuj.Enabled = True
                btnWybierzTif.Enabled = True
                grpFormat.Enabled = True
                grpOpcje.Enabled = True
                btnZamknij.Enabled = True
                btnZamknij.Text = "Zamknij"
                If _zamknijPoZakonczeniu Then Close()
            End If
        End Try
    End Sub

    Private Shared Sub KonwertujPlikGeoTiff(
        sciezkaWejscia As String,
        sciezkaWyjscia As String,
        rodzaj As RodzajEksportu,
        epsg As Integer,
        zoomMin As Integer,
        zoomMax As Integer,
        jakosc As Integer,
        podzialKmz As Boolean,
        postep As IProgress(Of Integer),
        token As CancellationToken)

        Const ZasiegMercatorM As Double = 20037508.342789244

        Using czytnik As New CzytnikGeoTiff(sciezkaWejscia)
            Dim geo = czytnik.Georeferencja
            Dim sz = czytnik.Szerokosc
            Dim wys = czytnik.Wysokosc
            Dim uklad As UkladWspolrzednych = If(epsg = 3857, Nothing, UkladWspolrzednych.ZKodu(epsg))

            Dim doWgs84 As Func(Of Double, Double, PunktGeo) =
                Function(xEast As Double, yNorth As Double) As PunktGeo
                    If epsg = 3857 Then
                        Dim lon = (xEast / ZasiegMercatorM) * 180.0
                        Dim lat = (180.0 / Math.PI) * (2.0 * Math.Atan(Math.Exp((yNorth / ZasiegMercatorM) * Math.PI)) - Math.PI / 2.0)
                        Return New PunktGeo(lat, lon)
                    End If
                    Return uklad.DoWgs84(yNorth, xEast)
                End Function

            ' Odwrotna transformacja afiniczna: z układu współrzędnych GeoTIFF na piksel rastra
            ' X = A*px + B*py + C (wschód), Y = D*px + E*py + F (północ)
            Dim det = geo.A * geo.E - geo.B * geo.D
            If Math.Abs(det) < 0.000000000001 Then
                Throw New InvalidOperationException("Niepoprawna macierz georeferencji GeoTIFF (wyznacznik bliski 0).")
            End If

            Dim invA = geo.E / det
            Dim invB = -geo.B / det
            Dim invD = -geo.D / det
            Dim invE = geo.A / det

            Dim geoDoPx = Function(geoPt As PunktGeo) As PunktXY
                              Dim dx As Double, dy As Double
                              If epsg = 3857 Then
                                  Dim latClamped = Math.Max(-85.05112878, Math.Min(85.05112878, geoPt.Szerokosc))
                                  Dim east = (geoPt.Dlugosc / 180.0) * ZasiegMercatorM
                                  Dim north = Math.Log(Math.Tan((90.0 + latClamped) * Math.PI / 360.0)) * (ZasiegMercatorM / Math.PI)
                                  dx = east - geo.C
                                  dy = north - geo.F
                              Else
                                  Dim wMetrach = uklad.ZWgs84(geoPt)
                                  dx = wMetrach.Y - geo.C
                                  dy = wMetrach.X - geo.F
                              End If
                              Return New PunktXY(invA * dx + invB * dy, invD * dx + invE * dy)
                          End Function

            ' Wyznaczamy narożniki w WGS84
            Dim p0 = geo.PikselNaWspolrzedne(0, 0)
            Dim p1 = geo.PikselNaWspolrzedne(sz, 0)
            Dim p2 = geo.PikselNaWspolrzedne(sz, wys)
            Dim p3 = geo.PikselNaWspolrzedne(0, wys)

            Dim g0 = doWgs84(p0.X, p0.Y)
            Dim g1 = doWgs84(p1.X, p1.Y)
            Dim g2 = doWgs84(p2.X, p2.Y)
            Dim g3 = doWgs84(p3.X, p3.Y)

            Dim minLat = Math.Min(Math.Min(g0.Szerokosc, g1.Szerokosc), Math.Min(g2.Szerokosc, g3.Szerokosc))
            Dim maxLat = Math.Max(Math.Max(g0.Szerokosc, g1.Szerokosc), Math.Max(g2.Szerokosc, g3.Szerokosc))
            Dim minLon = Math.Min(Math.Min(g0.Dlugosc, g1.Dlugosc), Math.Min(g2.Dlugosc, g3.Dlugosc))
            Dim maxLon = Math.Max(Math.Max(g0.Dlugosc, g1.Dlugosc), Math.Max(g2.Dlugosc, g3.Dlugosc))

            Dim zasiegWgs84 As New ProstokatGeo(maxLat, minLat, maxLon, minLon)

            Dim zadanie As New ZadanieEksportu With {
                .Plik = sciezkaWyjscia,
                .Rodzaj = rodzaj,
                .JakoscJpeg = jakosc,
                .PoziomMin = zoomMin,
                .PoziomMax = zoomMax,
                .PodzialNaTomy = podzialKmz,
                .MaksKafli = 100,
                .Nazwa = Path.GetFileNameWithoutExtension(sciezkaWyjscia)
            }

            Try
                EksportMapy.UsunWynik(zadanie)

                If rodzaj = RodzajEksportu.Kmz Then
                    Dim pikselMetry As Double
                    If epsg = 4326 Then
                        Dim mSz As Double, mDl As Double
                        Georeferencja.MetrowNaStopien((minLat + maxLat) / 2.0, mSz, mDl)
                        pikselMetry = Math.Max(0.1, Math.Sqrt((geo.A * mDl) ^ 2 + (geo.D * mSz) ^ 2))
                    Else
                        pikselMetry = Math.Max(0.1, Math.Sqrt(geo.A * geo.A + geo.D * geo.D))
                    End If

                    Dim plan As New PlanKmz(zasiegWgs84, pikselMetry, 1024, If(podzialKmz, 0, 100))
                    Dim tomy = If(podzialKmz, plan.WyznaczTomy(100), Nothing)
                    Dim gotowe As Integer = 0
                    Dim zapisane As Integer = 0

                    If tomy IsNot Nothing AndAlso tomy.Count > 1 Then
                        Dim lacznie = Math.Max(1, tomy.Sum(Function(t) t.LiczbaKafli))
                        For Each tom In tomy
                            Dim plikTomu = EksportMapy.NazwaPlikuTomu(sciezkaWyjscia, tom.Numer, tomy.Count)
                            Dim nazwaTomu = zadanie.Nazwa & " (Tom " & tom.Numer & " z " & tomy.Count & ")"
                            Using kmz As New ZapisKmz(plikTomu, nazwaTomu, 50)
                                For Each kfl In tom.Kafle
                                    token.ThrowIfCancellationRequested()
                                    Dim w = kfl.Item1, k = kfl.Item2
                                    Dim px = plan.PikseleKafla(w, k)
                                    Dim siatka As New SiatkaPrzeliczen(px.Item3, px.Item4, 16,
                                                                       Function(x, y) geoDoPx(New PunktGeo(plan.Obszar.Polnoc - (px.Item2 + y) * plan.PikselSzerokosc,
                                                                                                           plan.Obszar.Zachod + (px.Item1 + x) * plan.PikselDlugosc)))
                                    Dim bajty = RenderujKafelGeoTiff(px.Item3, px.Item4, siatka, czytnik, sz, wys, jakosc)
                                    If bajty IsNot Nothing Then
                                        kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & ".jpg",
                                                       bajty, plan.ZasiegKafla(w, k))
                                        zapisane += 1
                                    End If
                                    gotowe += 1
                                    postep?.Report(Math.Min(100, gotowe * 100 \ lacznie))
                                Next
                                kmz.Zakoncz()
                            End Using
                        Next
                    Else
                        Dim lacznie = Math.Max(1, plan.Wiersze * plan.Kolumny)
                        Using kmz As New ZapisKmz(sciezkaWyjscia, zadanie.Nazwa, 50)
                            For w = 0 To plan.Wiersze - 1
                                For k = 0 To plan.Kolumny - 1
                                    token.ThrowIfCancellationRequested()
                                    Dim px = plan.PikseleKafla(w, k)
                                    Dim siatka As New SiatkaPrzeliczen(px.Item3, px.Item4, 16,
                                                                       Function(x, y) geoDoPx(New PunktGeo(plan.Obszar.Polnoc - (px.Item2 + y) * plan.PikselSzerokosc,
                                                                                                           plan.Obszar.Zachod + (px.Item1 + x) * plan.PikselDlugosc)))
                                    Dim bajty = RenderujKafelGeoTiff(px.Item3, px.Item4, siatka, czytnik, sz, wys, jakosc)
                                    If bajty IsNot Nothing Then
                                        kmz.DodajKafel("kafel_" & (w + 1).ToString("D2") & "_" & (k + 1).ToString("D2") & ".jpg",
                                                       bajty, plan.ZasiegKafla(w, k))
                                        zapisane += 1
                                    End If
                                    gotowe += 1
                                    postep?.Report(Math.Min(100, gotowe * 100 \ lacznie))
                                Next
                            Next
                            kmz.Zakoncz()
                        End Using
                    End If

                    If zapisane = 0 Then
                        Throw New InvalidOperationException("Nie wygenerowano żadnych kafli KMZ w wybranym obszarze.")
                    End If
                    Return
                End If

                ' Zbieramy listę kafli Web Mercator
                Dim kafle As New List(Of KafelXYZ)()
                For z = zoomMin To zoomMax
                    Dim zakres = WebMercator.ZakresKafli(zasiegWgs84, z)
                    For y = zakres.Item2 To zakres.Item4
                        For x = zakres.Item1 To zakres.Item3
                            kafle.Add(New KafelXYZ(x, y, z))
                        Next
                    Next
                Next

                If kafle.Count = 0 Then
                    Throw New InvalidOperationException("Brak kafli w wybranym zakresie powiększeń.")
                End If

                Try
                    Using magazyn = EksportMapy.UtworzMagazyn(zadanie, zasiegWgs84)
                        Dim gotowe = 0
                        For Each grupa In kafle.GroupBy(Function(k) k.Z).OrderByDescending(Function(g) g.Key)
                            token.ThrowIfCancellationRequested()
                            Dim poziom = grupa.Key
                            magazyn.RozpocznijPoziom(poziom)

                            For Each k In grupa
                                token.ThrowIfCancellationRequested()
                                Dim xBaz = k.X * 256.0
                                Dim yBaz = k.Y * 256.0
                                Dim siatka As New SiatkaPrzeliczen(256, 256, 16,
                                                                   Function(x, y) geoDoPx(New PunktGeo(WebMercator.Szerokosc(yBaz + y, poziom),
                                                                                                       WebMercator.Dlugosc(xBaz + x, poziom))))
                                Dim bajtyJpeg = RenderujKafelGeoTiff(256, 256, siatka, czytnik, sz, wys, jakosc)
                                If bajtyJpeg IsNot Nothing Then
                                    magazyn.Zapisz(k, bajtyJpeg)
                                End If

                                gotowe += 1
                                If gotowe Mod 10 = 0 OrElse gotowe = kafle.Count Then
                                    postep?.Report(CInt(gotowe * 100L \ kafle.Count))
                                End If
                            Next

                            magazyn.ZakonczPoziom()
                        Next

                        magazyn.ZakonczZapis()
                    End Using
                Finally
                    System.Data.SQLite.SQLiteConnection.ClearAllPools()
                End Try
            Catch
                Try
                    System.Data.SQLite.SQLiteConnection.ClearAllPools()
                    EksportMapy.UsunWynik(zadanie)
                Catch
                End Try
                Throw
            End Try
        End Using
    End Sub

    Private Shared Function RenderujKafelGeoTiff(wPx As Integer, hPx As Integer, siatka As SiatkaPrzeliczen,
                                                 czytnik As CzytnikGeoTiff, sz As Integer, wys As Integer,
                                                 jakosc As Integer) As Byte()
        Dim obs = siatka.Obszar
        If obs.XGora < 0 OrElse obs.XDol >= sz OrElse obs.YPrawy < 0 OrElse obs.YLewy >= wys Then Return Nothing

        Dim dane(wPx * hPx * 4 - 1) As Byte
        Dim maPiksel As Boolean = False

        For py = 0 To hPx - 1
            Dim rowOff = py * wPx * 4
            For px = 0 To wPx - 1
                Dim pt = siatka.Punkt(px + 0.5, py + 0.5)
                Dim gx = CInt(Math.Floor(pt.X))
                Dim gy = CInt(Math.Floor(pt.Y))
                Dim off = rowOff + px * 4
                Dim bOut As Byte = 255, gOut As Byte = 255, rOut As Byte = 255

                If gx >= 0 AndAlso gx < sz AndAlso gy >= 0 AndAlso gy < wys Then
                    Dim cb As Byte, cg As Byte, cr As Byte, ca As Byte
                    If czytnik.PobierzPikselBgra(gx, gy, cb, cg, cr, ca) AndAlso ca > 0 Then
                        maPiksel = True
                        If ca = 255 Then
                            bOut = cb : gOut = cg : rOut = cr
                        Else
                            Dim k As Double = ca / 255.0
                            bOut = CByte(Math.Round(cb * k + 255.0 * (1.0 - k)))
                            gOut = CByte(Math.Round(cg * k + 255.0 * (1.0 - k)))
                            rOut = CByte(Math.Round(cr * k + 255.0 * (1.0 - k)))
                        End If
                    End If
                End If

                dane(off) = bOut
                dane(off + 1) = gOut
                dane(off + 2) = rOut
                dane(off + 3) = 255
            Next
        Next

        If Not maPiksel Then Return Nothing

        Using kafelBmp As New Bitmap(wPx, hPx, PixelFormat.Format32bppArgb)
            Dim bd = kafelBmp.LockBits(New Rectangle(0, 0, wPx, hPx), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
            Try
                System.Runtime.InteropServices.Marshal.Copy(dane, 0, bd.Scan0, dane.Length)
            Finally
                kafelBmp.UnlockBits(bd)
            End Try
            Return EksportMapy.Koduj(kafelBmp, False, jakosc)
        End Using
    End Function

    Private Sub btnPokazWFolderze_Click(sender As Object, e As EventArgs) Handles btnPokazWFolderze.Click
        If Not String.IsNullOrEmpty(_ostatniWynik) AndAlso (File.Exists(_ostatniWynik) OrElse Directory.Exists(_ostatniWynik)) Then
            Try
                Process.Start("explorer.exe", "/select,""" & _ostatniWynik & """")
            Catch ex As Exception
                MessageBox.Show(Me, "Nie udało się otworzyć folderu:" & vbCrLf & OpisBledu.Z(ex), "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        If _przerwanie IsNot Nothing Then
            _przerwanie.Cancel()
            btnZamknij.Enabled = False
            lblStatus.Text = "Trwa przerywanie operacji..."
        Else
            Close()
        End If
    End Sub

End Class