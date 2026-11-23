'Copyright (C) <2026>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Threading
Imports MapoTero.Core

''' <summary>
''' Profil wysokościowy trasy GPX / KML / KMZ: wysokości terenu z NMT GUGiK (LIDAR) dla punktów trasy, wykres profilu
''' z sumą podejść i zejść oraz zapis GPX z wysokościami. Dawniej część okna „Obszar pobierania wzdłuż trasy”, choć nie
''' ma związku z wyznaczaniem obszaru pobierania - teraz osobne narzędzie w menu Narzędzia.
''' </summary>
Public Class FormProfilNmt

    Private _trasa As Korytarz
    Private _statystyki As StatystykiProfilu
    Private ReadOnly _linieWysokosci As New List(Of IList(Of Double))()
    Private _pobieranieCts As CancellationTokenSource

    ''' <param name="plikTrasy">Plik trasy wczytywany od razu (np. trasa korytarza bieżącej sesji) albo pusty.</param>
    Public Sub New(Optional plikTrasy As String = "")
        InitializeComponent()
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.ProfilNmt)
        PomocProgramu.ZamykanieKlawiszemEsc(Me)
        If Not String.IsNullOrEmpty(plikTrasy) AndAlso IO.File.Exists(plikTrasy) Then WczytajPlikTrasy(plikTrasy)
    End Sub

    Private Sub btnPlik_Click(sender As Object, e As EventArgs) Handles btnPlik.Click
        If dlgPlik.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        WczytajPlikTrasy(dlgPlik.FileName)
    End Sub

    ''' <summary>
    ''' Wczytuje trasę z pliku. Profil poprzedniej trasy jest usuwany (a jego pobieranie przerywane) - inaczej zostawałby
    ''' na wykresie, a „Zapisz GPX” zapisywałby nową trasę z wysokościami poprzedniej.
    ''' </summary>
    Friend Sub WczytajPlikTrasy(plik As String)
        _pobieranieCts?.Cancel()
        _linieWysokosci.Clear()
        _statystyki = Nothing
        btnZapiszGpx.Enabled = False
        picProfil.Invalidate()
        txtPlik.Text = plik
        _trasa = Nothing
        btnNmt.Enabled = False
        Try
            Dim k = Korytarz.WczytajTrase(plik)
            If Not k.Poprawny Then
                Opis("Plik nie zawiera śladu, trasy ani linii (GPX: trk, rte, wpt; KML: LineString, gx:Track).", Color.Red)
                Exit Sub
            End If
            _trasa = k
        Catch ex As Exception
            Opis("Nie udało się odczytać trasy: " & OpisBledu.Z(ex), Color.Red)
            Exit Sub
        End Try
        Opis("Trasa: " & If(_trasa.Nazwa <> "", _trasa.Nazwa & ", ", "") & _trasa.DlugoscKm.ToString("0.0") & " km, " &
             _trasa.LiczbaPunktow & " punktów.", Color.Black)
        btnNmt.Enabled = True
    End Sub

    Private Sub Opis(tekst As String, kolor As Color)
        lblOpis.ForeColor = kolor
        lblOpis.Text = tekst
    End Sub

    Private Async Sub btnNmt_Click(sender As Object, e As EventArgs) Handles btnNmt.Click
        If _trasa Is Nothing OrElse Not _trasa.Poprawny Then Exit Sub
        Dim punkty = _trasa.Linie.SelectMany(Function(l) l).ToList()
        If punkty.Count = 0 Then Exit Sub

        btnNmt.Enabled = False
        btnZapiszGpx.Enabled = False
        prgNmt.Value = 0
        prgNmt.Visible = True

        'trasa, dla której pobierane są wysokości - użytkownik może w tym czasie wskazać inny plik
        Dim trasa As Korytarz = _trasa
        Dim cts As New CancellationTokenSource()
        Dim token = cts.Token
        _pobieranieCts = cts
        Dim postep = New Progress(Of Integer)(Sub(p)
                                                  If Not token.IsCancellationRequested AndAlso Not Me.IsDisposed AndAlso prgNmt.Visible Then
                                                      prgNmt.Value = Math.Max(0, Math.Min(100, p))
                                                  End If
                                              End Sub)
        Try
            Dim h = Await ProfilNmt.PobierzWysokosciAsync(punkty, token, postep)
            If token.IsCancellationRequested OrElse IsDisposed OrElse trasa IsNot _trasa Then Exit Sub
            _linieWysokosci.Clear()
            Dim offset = 0
            For Each l In trasa.Linie
                Dim subH As New List(Of Double)()
                For i = 0 To l.Count - 1
                    subH.Add(h(offset + i))
                Next
                offset += l.Count
                _linieWysokosci.Add(subH)
            Next

            _statystyki = ProfilNmt.ObliczStatystyki(punkty, h)
            btnZapiszGpx.Enabled = True
            picProfil.Invalidate()
        Catch ex As OperationCanceledException
            ' Anulowano
        Catch ex As Exception
            If Not token.IsCancellationRequested AndAlso Not IsDisposed AndAlso trasa Is _trasa Then
                MessageBox.Show("Nie udało się pobrać danych wysokościowych z Geoportalu: " & OpisBledu.Z(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Finally
            If _pobieranieCts Is cts Then _pobieranieCts = Nothing
            cts.Dispose()
            If Not IsDisposed AndAlso _pobieranieCts Is Nothing Then
                btnNmt.Enabled = _trasa IsNot Nothing
                prgNmt.Visible = False
            End If
        End Try
    End Sub

    Private Sub btnZapiszGpx_Click(sender As Object, e As EventArgs) Handles btnZapiszGpx.Click
        If _trasa Is Nothing OrElse _linieWysokosci.Count = 0 Then Exit Sub
        Dim bezpiecznaNazwa = If(_trasa.Nazwa, "").Trim()
        For Each znak In System.IO.Path.GetInvalidFileNameChars()
            bezpiecznaNazwa = bezpiecznaNazwa.Replace(znak, "_"c)
        Next
        Dim nazwaPliku = If(bezpiecznaNazwa <> "", bezpiecznaNazwa, "trasa") & "_nmt.gpx"
        dlgZapiszGpx.FileName = nazwaPliku
        If dlgZapiszGpx.ShowDialog(Me) = DialogResult.OK Then
            Try
                If _trasa.DokumentGpx IsNot Nothing Then
                    'trasa z pliku GPX - wysokości dopisane do oryginału (zostają czasy punktów, punkty wpt i nazwy)
                    Dim wysokosci = _linieWysokosci.SelectMany(Function(l) l).ToList()
                    Korytarz.ZapiszGpx(Korytarz.WzbogacGpx(_trasa.DokumentGpx, wysokosci), dlgZapiszGpx.FileName)
                Else
                    Korytarz.ZapiszGpxZWysokosciami(dlgZapiszGpx.FileName, _trasa.Nazwa, _trasa.Linie, _linieWysokosci)
                End If
                MsgBox("Zapisano ślad GPX z wysokościami NMT do:" & vbCrLf & dlgZapiszGpx.FileName, MsgBoxStyle.Information, "Profil wysokościowy trasy")
            Catch ex As Exception
                MsgBox("Błąd zapisu pliku GPX: " & OpisBledu.Z(ex), MsgBoxStyle.Critical, "Profil wysokościowy trasy")
            End Try
        End If
    End Sub

    Private Sub picProfil_Paint(sender As Object, e As PaintEventArgs) Handles picProfil.Paint
        Dim g = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim r = picProfil.ClientRectangle

        If _statystyki Is Nothing OrElse _statystyki.Punkty.Count < 2 Then
            Dim info = "Wskaż plik trasy i kliknij ""Pobierz wysokości z NMT"", aby pobrać profil terenu z Geoportalu."
            Using sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                Using f As New Font(Me.Font.FontFamily, 8.25!, FontStyle.Regular)
                    Using b As New SolidBrush(Color.DimGray)
                        g.DrawString(info, f, b, r, sf)
                    End Using
                End Using
            End Using
            Return
        End If

        Dim padLeft = 35, padRight = 10, padTop = 18, padBottom = 16
        Dim w = r.Width - padLeft - padRight
        Dim h = r.Height - padTop - padBottom
        If w <= 0 OrElse h <= 0 Then Return

        Dim minH = _statystyki.MinWysokosc
        Dim maxH = _statystyki.MaxWysokosc
        Dim diffH = Math.Max(5.0, maxH - minH)
        Dim totalDist = Math.Max(1.0, _statystyki.DlugoscKm * 1000.0)

        ' Rysowanie siatki
        Using pSiatka As New Pen(Color.FromArgb(235, 235, 235))
            g.DrawLine(pSiatka, padLeft, padTop, padLeft + w, padTop)
            g.DrawLine(pSiatka, padLeft, padTop + h \ 2, padLeft + w, padTop + h \ 2)
            g.DrawLine(pSiatka, padLeft, padTop + h, padLeft + w, padTop + h)
        End Using

        ' Budowanie punktów wielokąta (punkty bez danych NMT - NaN, np. za granicą - są pomijane)
        Dim punktyPoly As New List(Of PointF)()
        Dim pts = _statystyki.Punkty
        For i = 0 To pts.Count - 1
            If Double.IsNaN(pts(i).Wysokosc) Then Continue For
            Dim x = CSng(padLeft + (pts(i).DystansMetry / totalDist) * w)
            Dim y = CSng(padTop + h - ((pts(i).Wysokosc - minH) / diffH) * h)
            y = Math.Max(padTop, Math.Min(padTop + h, y))
            punktyPoly.Add(New PointF(x, y))
        Next
        If punktyPoly.Count < 2 Then Return
        punktyPoly.Insert(0, New PointF(punktyPoly(0).X, padTop + h))
        punktyPoly.Add(New PointF(punktyPoly(punktyPoly.Count - 1).X, padTop + h))

        ' Wypełnienie gradientem i obrys
        Using bFill As New SolidBrush(Color.FromArgb(40, 0, 102, 204))
            g.FillPolygon(bFill, punktyPoly.ToArray())
        End Using

        Dim liniaPts = punktyPoly.Skip(1).Take(punktyPoly.Count - 2).ToArray()
        Using pLinia As New Pen(Color.FromArgb(0, 102, 204), 1.5!)
            g.DrawLines(pLinia, liniaPts)
        End Using

        ' Etykiety wysokości
        Using fSmall As New Font(Me.Font.FontFamily, 7.5!, FontStyle.Regular)
            Using bText As New SolidBrush(Color.Gray)
                g.DrawString(Math.Round(maxH).ToString() & "m", fSmall, bText, 2, padTop - 5)
                g.DrawString(Math.Round(minH).ToString() & "m", fSmall, bText, 2, padTop + h - 8)
            End Using
            ' Podsumowanie na górze
            Dim tytul = String.Format("Min: {0:0} m  Max: {1:0} m  Podejścia: +{2:0} m  Zejścia: -{3:0} m", minH, maxH, _statystyki.Podejscia, _statystyki.Zejscia)
            If _statystyki.PunktyBezDanych > 0 Then
                tytul &= String.Format("  (bez danych NMT: {0} pkt)", _statystyki.PunktyBezDanych)
            End If
            Using bTytul As New SolidBrush(Color.Navy)
                g.DrawString(tytul, fSmall, bTytul, padLeft, 2)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        _pobieranieCts?.Cancel()
        MyBase.OnFormClosing(e)
    End Sub

End Class
