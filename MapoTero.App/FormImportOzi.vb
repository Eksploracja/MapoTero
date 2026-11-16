'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.ComponentModel
Imports System.Data
Imports System.Data.SQLite
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

Public Class FormImportOzi

    Private _ozi As PlikKalibracjiOzi
    Private _anulowanie As CancellationTokenSource

    Public Sub New()
        InitializeComponent()
        Me.AllowDrop = True
    End Sub

    ''' <summary>Otwiera formularz i ładuje wskazany plik .map.</summary>
    Public Sub OtworzZPlikiem(sciezkaPlikuMap As String)
        ZaladujPlikMap(sciezkaPlikuMap)
    End Sub

    Protected Overrides Sub OnDragEnter(drgevent As DragEventArgs)
        MyBase.OnDragEnter(drgevent)
        If drgevent.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = DirectCast(drgevent.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
                If ext = ".map" OrElse ext = ".jpg" OrElse ext = ".png" OrElse ext = ".tif" OrElse ext = ".bmp" Then
                    drgevent.Effect = DragDropEffects.Copy
                    Exit Sub
                End If
            End If
        End If
        drgevent.Effect = DragDropEffects.None
    End Sub

    Protected Overrides Sub OnDragDrop(drgevent As DragEventArgs)
        MyBase.OnDragDrop(drgevent)
        If drgevent.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = DirectCast(drgevent.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
                If ext = ".map" Then
                    ZaladujPlikMap(pliki(0))
                ElseIf _ozi IsNot Nothing Then
                    UstawPlikObrazu(pliki(0))
                End If
            End If
        End If
    End Sub

    Private Sub btnWybierzMap_Click(sender As Object, e As EventArgs) Handles btnWybierzMap.Click
        If dlgWybierzMap.ShowDialog(Me) = DialogResult.OK Then
            ZaladujPlikMap(dlgWybierzMap.FileName)
        End If
    End Sub

    Private Sub btnWybierzObraz_Click(sender As Object, e As EventArgs) Handles btnWybierzObraz.Click
        If dlgWybierzObraz.ShowDialog(Me) = DialogResult.OK Then
            UstawPlikObrazu(dlgWybierzObraz.FileName)
        End If
    End Sub

    Private Sub ZaladujPlikMap(sciezka As String)
        Try
            _ozi = PlikKalibracjiOzi.Wczytaj(sciezka)
            txtPlikMap.Text = sciezka
            txtPlikObrazu.Text = _ozi.PlikObrazu
            OdswiezInformacje()
        Catch ex As Exception
            MessageBox.Show("Nie udało się odczytać pliku kalibracji: " & ex.Message,
                            "Błąd pliku kalibracji", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UstawPlikObrazu(sciezka As String)
        If _ozi Is Nothing Then Exit Sub
        Try
            Dim oziNowy = PlikKalibracjiOzi.Wczytaj(_ozi.SciezkaMap, sciezka)
            _ozi = oziNowy
            txtPlikObrazu.Text = sciezka
            OdswiezInformacje()
        Catch ex As Exception
            MessageBox.Show("Błąd odczytu pliku obrazu: " & ex.Message, "Błąd obrazu", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OdswiezInformacje()
        If _ozi Is Nothing Then
            lblInfoTytul.Text = "Tytuł:"
            lblInfoDatum.Text = "Datum:"
            lblInfoWymiary.Text = "Wymiary:"
            lblInfoPunkty.Text = "Punkty:"
            lblInfoZasieg.Text = "Zasięg:"
            btnKonwertuj.Enabled = False
            Exit Sub
        End If

        Dim ci = CultureInfo.InvariantCulture
        lblInfoTytul.Text = "Tytuł: " & If(String.IsNullOrEmpty(_ozi.Tytul), Path.GetFileNameWithoutExtension(_ozi.SciezkaMap), _ozi.Tytul)

        Dim korektaTekst = ""
        If Not _ozi.Datum.ToUpperInvariant().Contains("WGS") Then
            korektaTekst = " (automatyczna korekta Datum do WGS84)"
        End If
        lblInfoDatum.Text = "Układ / Datum: " & _ozi.Datum & korektaTekst

        If _ozi.PlikObrazuIstnieje Then
            txtPlikObrazu.ForeColor = SystemColors.WindowText
            lblInfoWymiary.Text = String.Format(ci, "Wymiary rastra: {0} x {1} px (~{2:0.0} m/pix)",
                                                _ozi.SzerokoscPx, _ozi.WysokoscPx, _ozi.RozdzielczoscMetrowNaPiksel)
            btnKonwertuj.Enabled = True
            lblStatus.Text = "Kalibracja poprawna. Gotowy do konwersji."
        Else
            txtPlikObrazu.ForeColor = Color.Red
            lblInfoWymiary.Text = "Wymiary: brak pliku obrazu na dysku (wskaż plik ręcznie przyciskiem Przeglądaj)"
            btnKonwertuj.Enabled = False
            lblStatus.Text = "Ostrzeżenie: wskaż plik obrazu mapy na dysku."
        End If

        lblInfoPunkty.Text = String.Format(ci, "Punkty kalibracyjne: {0} pkt, błąd RMS: {1:0.0} m",
                                           _ozi.Punkty.Count, _ozi.Transformacja.BladRmsMetry)

        lblInfoZasieg.Text = String.Format(ci, "Zasięg WGS84: N {0:0.0000}° .. {1:0.0000}°, E {2:0.0000}° .. {3:0.0000}°",
                                           _ozi.ZasiegWgs84.Poludnie, _ozi.ZasiegWgs84.Polnoc,
                                           _ozi.ZasiegWgs84.Zachod, _ozi.ZasiegWgs84.Wschod)
    End Sub

    Private Sub rbFormat_CheckedChanged(sender As Object, e As EventArgs) Handles rbKmz.CheckedChanged, rbJnx.CheckedChanged, rbMbtiles.CheckedChanged
        chkPodzialKmz.Enabled = rbKmz.Checked
    End Sub

    Private Async Sub btnKonwertuj_Click(sender As Object, e As EventArgs) Handles btnKonwertuj.Click
        If _ozi Is Nothing OrElse Not _ozi.PlikObrazuIstnieje Then Exit Sub

        Dim domyslnaNazwa = Path.GetFileNameWithoutExtension(_ozi.SciezkaMap)
        If rbJnx.Checked Then
            dlgZapisz.Filter = "Garmin BirdsEye (*.jnx)|*.jnx"
            dlgZapisz.DefaultExt = "jnx"
            dlgZapisz.FileName = domyslnaNazwa & ".jnx"
        ElseIf rbKmz.Checked Then
            dlgZapisz.Filter = "Garmin Custom Maps (*.kmz)|*.kmz"
            dlgZapisz.DefaultExt = "kmz"
            dlgZapisz.FileName = domyslnaNazwa & ".kmz"
        Else
            dlgZapisz.Filter = "Baza MBTiles (*.mbtiles)|*.mbtiles"
            dlgZapisz.DefaultExt = "mbtiles"
            dlgZapisz.FileName = domyslnaNazwa & ".mbtiles"
        End If

        If dlgZapisz.ShowDialog(Me) <> DialogResult.OK Then Exit Sub

        Dim sciezkaWyjsciowa = dlgZapisz.FileName
        Dim formatJnx = rbJnx.Checked
        Dim formatKmz = rbKmz.Checked
        Dim kolejnosc = CInt(nudKolejnosc.Value)
        Dim jakosc = CInt(nudJakosc.Value)
        Dim podzial = chkPodzialKmz.Checked

        btnKonwertuj.Enabled = False
        btnWybierzMap.Enabled = False
        btnWybierzObraz.Enabled = False
        prgPostep.Value = 0

        _anulowanie = New CancellationTokenSource()
        Dim postep = New Progress(Of Integer)(Sub(pct)
                                                  prgPostep.Value = Math.Max(0, Math.Min(100, pct))
                                                  lblStatus.Text = $"Trwa konwersja kafli... {pct}%"
                                              End Sub)

        Try
            Await Task.Run(Sub()
                               If formatKmz Then
                                   KonwertujDoKmz(_ozi, sciezkaWyjsciowa, kolejnosc, jakosc, podzial, postep, _anulowanie.Token)
                               ElseIf formatJnx Then
                                   KonwertujDoJnx(_ozi, sciezkaWyjsciowa, kolejnosc, jakosc, postep, _anulowanie.Token)
                               Else
                                   KonwertujDoMbtiles(_ozi, sciezkaWyjsciowa, jakosc, postep, _anulowanie.Token)
                               End If
                           End Sub, _anulowanie.Token)

            lblStatus.Text = "Konwersja zakończona pomyślnie!"
            prgPostep.Value = 100
            MessageBox.Show("Pomyślnie utworzono mapę:" & vbCrLf & sciezkaWyjsciowa,
                            "Konwersja zakończona", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As OperationCanceledException
            lblStatus.Text = "Konwersja została anulowana."
            prgPostep.Value = 0
        Catch ex As Exception
            lblStatus.Text = "Błąd: " & ex.Message
            MessageBox.Show("Wystąpił błąd podczas konwersji:" & vbCrLf & ex.Message,
                            "Błąd konwersji", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnKonwertuj.Enabled = True
            btnWybierzMap.Enabled = True
            btnWybierzObraz.Enabled = True
            If _anulowanie IsNot Nothing Then
                _anulowanie.Dispose()
                _anulowanie = Nothing
            End If
        End Try
    End Sub

    Friend Shared Sub KonwertujDoKmz(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                      kolejnosc As Integer, jakosc As Integer, podzial As Boolean,
                                      postep As IProgress(Of Integer), token As CancellationToken)
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim w = srcBmp.Width
            Dim h = srcBmp.Height
            Const kRozmiar As Integer = 1024
            Dim cols = CInt(Math.Ceiling(w / CDbl(kRozmiar)))
            Dim rows = CInt(Math.Ceiling(h / CDbl(kRozmiar)))
            Dim lacznieKafli = cols * rows

            Dim maxKafliNaTom = If(podzial, 100, Integer.MaxValue)
            Dim liczbaTomow = CInt(Math.Ceiling(lacznieKafli / CDbl(maxKafliNaTom)))

            Dim biezacyTom = 1
            Dim kafliWTomie = 0
            Dim sciezkaTomu = If(liczbaTomow > 1, EksportMapy.NazwaPlikuTomu(plikWyjsciowy, biezacyTom, liczbaTomow), plikWyjsciowy)
            Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
            Dim zapisKmz As New ZapisKmz(sciezkaTomu, tytul, kolejnosc)

            Try
                Dim licznik = 0
                For r = 0 To rows - 1
                    For c = 0 To cols - 1
                        token.ThrowIfCancellationRequested()
                        Dim pxX = c * kRozmiar
                        Dim pxY = r * kRozmiar
                        Dim kSzer = Math.Min(kRozmiar, w - pxX)
                        Dim kWys = Math.Min(kRozmiar, h - pxY)

                        Dim pLG = ozi.Transformacja.PikselDoGeo(pxX, pxY)
                        Dim pPG = ozi.Transformacja.PikselDoGeo(pxX + kSzer, pxY)
                        Dim pPD = ozi.Transformacja.PikselDoGeo(pxX + kSzer, pxY + kWys)
                        Dim pLD = ozi.Transformacja.PikselDoGeo(pxX, pxY + kWys)

                        Dim n = Math.Max(pLG.Szerokosc, pPG.Szerokosc)
                        Dim s = Math.Min(pLD.Szerokosc, pPD.Szerokosc)
                        Dim we = Math.Min(pLG.Dlugosc, pLD.Dlugosc)
                        Dim ea = Math.Max(pPG.Dlugosc, pPD.Dlugosc)
                        Dim zasiegKafla = New ProstokatGeo(n, s, ea, we)

                        Using kafelBmp As New Bitmap(kSzer, kWys, PixelFormat.Format24bppRgb)
                            Using g = Graphics.FromImage(kafelBmp)
                                g.DrawImage(srcBmp, New Rectangle(0, 0, kSzer, kWys), New Rectangle(pxX, pxY, kSzer, kWys), GraphicsUnit.Pixel)
                            End Using
                            Dim jpeg = EksportMapy.Koduj(kafelBmp, False, jakosc)

                            If kafliWTomie >= maxKafliNaTom Then
                                zapisKmz.Zakoncz()
                                zapisKmz.Dispose()
                                biezacyTom += 1
                                kafliWTomie = 0
                                sciezkaTomu = EksportMapy.NazwaPlikuTomu(plikWyjsciowy, biezacyTom, liczbaTomow)
                                zapisKmz = New ZapisKmz(sciezkaTomu, tytul, kolejnosc)
                            End If

                            Dim nazwaKafla = String.Format(CultureInfo.InvariantCulture, "tile_{0:00}_{1:00}.jpg", r, c)
                            zapisKmz.DodajKafel(nazwaKafla, jpeg, zasiegKafla)
                            kafliWTomie += 1
                        End Using

                        licznik += 1
                        postep?.Report(CInt(licznik * 100 / lacznieKafli))
                    Next
                Next
                zapisKmz.Zakoncz()
            Finally
                zapisKmz.Dispose()
            End Try
        End Using
    End Sub

    Friend Shared Sub KonwertujDoJnx(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                      kolejnosc As Integer, jakosc As Integer,
                                      postep As IProgress(Of Integer), token As CancellationToken)
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim w = srcBmp.Width
            Dim h = srcBmp.Height

            Dim mPerPx = ozi.RozdzielczoscMetrowNaPiksel
            If mPerPx <= 0 Then mPerPx = 2.5
            Dim srodekSz = (ozi.ZasiegWgs84.Polnoc + ozi.ZasiegWgs84.Poludnie) / 2.0
            Dim z = CInt(Math.Round(Math.Log(156543.03392 * Math.Cos(srodekSz * Math.PI / 180.0) / mPerPx) / Math.Log(2)))
            z = Math.Max(10, Math.Min(18, z))

            Dim skala = ZapisJnx.SkalaDlaPoziomu(z, srodekSz)
            Dim poziom As New PoziomJnx(skala)

            Dim zakres = WebMercator.ZakresKafli(ozi.ZasiegWgs84, z)
            Dim lacznie = CLng(zakres.Item3 - zakres.Item1 + 1) * (zakres.Item4 - zakres.Item2 + 1)
            Dim licznik = 0L

            Dim gLG = ozi.Transformacja.PikselDoGeo(0, 0)
            Dim gPG = ozi.Transformacja.PikselDoGeo(w, 0)
            Dim gLD = ozi.Transformacja.PikselDoGeo(0, h)

            For y = zakres.Item2 To zakres.Item4
                For x = zakres.Item1 To zakres.Item3
                    token.ThrowIfCancellationRequested()
                    Dim polnoc = WebMercator.Szerokosc(y * 256.0, z)
                    Dim poludnie = WebMercator.Szerokosc((y + 1) * 256.0, z)
                    Dim zachod = WebMercator.Dlugosc(x * 256.0, z)
                    Dim wschod = WebMercator.Dlugosc((x + 1) * 256.0, z)

                    Dim dLon = wschod - zachod
                    Dim dLat = polnoc - poludnie
                    If dLon > 0 AndAlso dLat > 0 Then
                        Dim u0 = (gLG.Dlugosc - zachod) / dLon * 256.0
                        Dim v0 = (polnoc - gLG.Szerokosc) / dLat * 256.0
                        Dim u1 = (gPG.Dlugosc - zachod) / dLon * 256.0
                        Dim v1 = (polnoc - gPG.Szerokosc) / dLat * 256.0
                        Dim u2 = (gLD.Dlugosc - zachod) / dLon * 256.0
                        Dim v2 = (polnoc - gLD.Szerokosc) / dLat * 256.0
                        Dim u3 = u1 + u2 - u0
                        Dim v3 = v1 + v2 - v0

                        Dim minU = Math.Min(Math.Min(u0, u1), Math.Min(u2, u3))
                        Dim maxU = Math.Max(Math.Max(u0, u1), Math.Max(u2, u3))
                        Dim minV = Math.Min(Math.Min(v0, v1), Math.Min(v2, v3))
                        Dim maxV = Math.Max(Math.Max(v0, v1), Math.Max(v2, v3))

                        If maxU >= 0 AndAlso minU <= 256.0 AndAlso maxV >= 0 AndAlso minV <= 256.0 Then
                            Using kafelBmp As New Bitmap(256, 256, PixelFormat.Format24bppRgb)
                                Using g = Graphics.FromImage(kafelBmp)
                                    g.Clear(Color.White)
                                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
                                    g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                                    Dim destPoints = {New PointF(CSng(u0), CSng(v0)), New PointF(CSng(u1), CSng(v1)), New PointF(CSng(u2), CSng(v2))}
                                    g.DrawImage(srcBmp, destPoints, New RectangleF(0, 0, w, h), GraphicsUnit.Pixel)
                                End Using
                                Dim dane = EksportMapy.Koduj(kafelBmp, False, jakosc)
                                poziom.Kafle.Add(New KafelJnx(New ProstokatGeo(polnoc, poludnie, wschod, zachod), dane))
                            End Using
                        End If
                    End If

                    licznik += 1
                    If lacznie > 0 Then postep?.Report(CInt(licznik * 100 / lacznie))
                Next
            Next

            If poziom.Kafle.Count = 0 Then
                Throw New InvalidOperationException("Nie wygenerowano żadnych kafli w wybranym obszarze.")
            End If

            Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
            ZapisJnx.Zapisz(plikWyjsciowy, tytul, kolejnosc, New List(Of PoziomJnx) From {poziom})
        End Using
    End Sub

    Friend Shared Sub KonwertujDoMbtiles(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                          jakosc As Integer, postep As IProgress(Of Integer), token As CancellationToken)
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim w = srcBmp.Width
            Dim h = srcBmp.Height

            Dim mPerPx = ozi.RozdzielczoscMetrowNaPiksel
            If mPerPx <= 0 Then mPerPx = 2.5
            Dim srodekSz = (ozi.ZasiegWgs84.Polnoc + ozi.ZasiegWgs84.Poludnie) / 2.0
            Dim z = CInt(Math.Round(Math.Log(156543.03392 * Math.Cos(srodekSz * Math.PI / 180.0) / mPerPx) / Math.Log(2)))
            z = Math.Max(10, Math.Min(18, z))

            EksportMapy.UsunPlikBazy(plikWyjsciowy)
            Using polaczenie As New SQLiteConnection(EksportMapy.ParametryPolaczenia(plikWyjsciowy, "Wal"))
                polaczenie.Open()
                Using cmdInit As New SQLiteCommand("PRAGMA synchronous = OFF; PRAGMA journal_mode = WAL; CREATE TABLE metadata (name text, value text); CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob); CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);", polaczenie)
                    cmdInit.ExecuteNonQuery()
                End Using

                Dim ci = CultureInfo.InvariantCulture
                Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
                Dim meta As New Dictionary(Of String, String) From {
                    {"name", tytul},
                    {"type", "baselayer"},
                    {"version", "1.1"},
                    {"description", "Konwersja z mapy OziExplorer (.map)"},
                    {"format", "jpg"},
                    {"minzoom", z.ToString(ci)},
                    {"maxzoom", z.ToString(ci)},
                    {"bounds", String.Format(ci, "{0},{1},{2},{3}", ozi.ZasiegWgs84.Zachod, ozi.ZasiegWgs84.Poludnie, ozi.ZasiegWgs84.Wschod, ozi.ZasiegWgs84.Polnoc)},
                    {"center", String.Format(ci, "{0},{1},{2}", (ozi.ZasiegWgs84.Zachod + ozi.ZasiegWgs84.Wschod) / 2.0, srodekSz, z)}
                }

                Using cmdMeta As New SQLiteCommand("INSERT INTO metadata (name, value) VALUES (@n, @v);", polaczenie)
                    Dim pN = cmdMeta.Parameters.Add("@n", DbType.String)
                    Dim pV = cmdMeta.Parameters.Add("@v", DbType.String)
                    For Each m In meta
                        pN.Value = m.Key
                        pV.Value = m.Value
                        cmdMeta.ExecuteNonQuery()
                    Next
                End Using

                Dim zakres = WebMercator.ZakresKafli(ozi.ZasiegWgs84, z)
                Dim lacznie = CLng(zakres.Item3 - zakres.Item1 + 1) * (zakres.Item4 - zakres.Item2 + 1)
                Dim licznik = 0L

                Dim gLG = ozi.Transformacja.PikselDoGeo(0, 0)
                Dim gPG = ozi.Transformacja.PikselDoGeo(w, 0)
                Dim gLD = ozi.Transformacja.PikselDoGeo(0, h)

                Using trans = polaczenie.BeginTransaction()
                    Using cmdTile As New SQLiteCommand("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);", polaczenie, trans)
                        Dim pZ = cmdTile.Parameters.Add("@z", DbType.Int32)
                        Dim pX = cmdTile.Parameters.Add("@x", DbType.Int32)
                        Dim pY = cmdTile.Parameters.Add("@y", DbType.Int32)
                        Dim pD = cmdTile.Parameters.Add("@d", DbType.Binary)

                        For y = zakres.Item2 To zakres.Item4
                            For x = zakres.Item1 To zakres.Item3
                                token.ThrowIfCancellationRequested()
                                Dim polnoc = WebMercator.Szerokosc(y * 256.0, z)
                                Dim poludnie = WebMercator.Szerokosc((y + 1) * 256.0, z)
                                Dim zachod = WebMercator.Dlugosc(x * 256.0, z)
                                Dim wschod = WebMercator.Dlugosc((x + 1) * 256.0, z)

                                Dim dLon = wschod - zachod
                                Dim dLat = polnoc - poludnie
                                If dLon > 0 AndAlso dLat > 0 Then
                                    Dim u0 = (gLG.Dlugosc - zachod) / dLon * 256.0
                                    Dim v0 = (polnoc - gLG.Szerokosc) / dLat * 256.0
                                    Dim u1 = (gPG.Dlugosc - zachod) / dLon * 256.0
                                    Dim v1 = (polnoc - gPG.Szerokosc) / dLat * 256.0
                                    Dim u2 = (gLD.Dlugosc - zachod) / dLon * 256.0
                                    Dim v2 = (polnoc - gLD.Szerokosc) / dLat * 256.0
                                    Dim u3 = u1 + u2 - u0
                                    Dim v3 = v1 + v2 - v0

                                    Dim minU = Math.Min(Math.Min(u0, u1), Math.Min(u2, u3))
                                    Dim maxU = Math.Max(Math.Max(u0, u1), Math.Max(u2, u3))
                                    Dim minV = Math.Min(Math.Min(v0, v1), Math.Min(v2, v3))
                                    Dim maxV = Math.Max(Math.Max(v0, v1), Math.Max(v2, v3))

                                    If maxU >= 0 AndAlso minU <= 256.0 AndAlso maxV >= 0 AndAlso minV <= 256.0 Then
                                        Using kafelBmp As New Bitmap(256, 256, PixelFormat.Format24bppRgb)
                                            Using g = Graphics.FromImage(kafelBmp)
                                                g.Clear(Color.White)
                                                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
                                                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                                                Dim destPoints = {New PointF(CSng(u0), CSng(v0)), New PointF(CSng(u1), CSng(v1)), New PointF(CSng(u2), CSng(v2))}
                                                g.DrawImage(srcBmp, destPoints, New RectangleF(0, 0, w, h), GraphicsUnit.Pixel)
                                            End Using
                                            Dim dane = EksportMapy.Koduj(kafelBmp, False, jakosc)

                                            ' W MBTiles wiersze liczone są od dołu (TMS)
                                            Dim wierszTms = (1 << z) - 1 - y
                                            pZ.Value = z
                                            pX.Value = x
                                            pY.Value = wierszTms
                                            pD.Value = dane
                                            cmdTile.ExecuteNonQuery()
                                        End Using
                                    End If
                                End If

                                licznik += 1
                                If lacznie > 0 Then postep?.Report(CInt(licznik * 100 / lacznie))
                            Next
                        Next
                        trans.Commit()
                    End Using
                End Using

                Using cmdDel As New SQLiteCommand("PRAGMA journal_mode = DELETE;", polaczenie)
                    cmdDel.ExecuteNonQuery()
                End Using
            End Using
        End Using
    End Sub

End Class
