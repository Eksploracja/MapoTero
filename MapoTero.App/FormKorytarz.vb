'Copyright (C) <2015>  pajakt

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
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Wybór obszaru pobierania wzdłuż trasy: plik GPX / KML / KMZ i szerokość pasa. Okno pokazuje, ile segmentów
''' zostanie pobranych w porównaniu z prostokątem obejmującym trasę (przy bieżącym rozmiarze piksela i segmentu)
''' oraz umożliwia pobranie i podgląd profilu wysokościowego z NMT Geoportalu.
''' </summary>
Public Class FormKorytarz

    Private _korytarz As Korytarz
    Private _statystyki As StatystykiProfilu
    Private _linieWysokosci As New List(Of IList(Of Double))()
    Private _pobieranieCts As CancellationTokenSource

    ''' <summary>Wybrany korytarz (po kliknięciu OK).</summary>
    <ComponentModel.DesignerSerializationVisibility(ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Wynik As Korytarz

    ''' <param name="biezacy">Korytarz bieżącej sesji (zmiana szerokości pasa bez ponownego wskazywania pliku) albo Nothing.</param>
    Public Sub New(biezacy As Korytarz)
        InitializeComponent()
        If biezacy IsNot Nothing Then
            'kopia - Anuluj nie może zmienić korytarza sesji
            _korytarz = Korytarz.ZTekstuZapisu(biezacy.DoTekstu())
            txtPlik.Text = biezacy.Nazwa
            nudSzerokosc.Value = Math.Max(nudSzerokosc.Minimum, Math.Min(nudSzerokosc.Maximum, CDec(biezacy.SzerokoscMetry)))
            chkBuforAdaptacyjny.Checked = biezacy.BuforAdaptacyjny
        End If
        OdswiezOpis()
    End Sub

    Private Sub btnPlik_Click(sender As Object, e As EventArgs) Handles btnPlik.Click
        If dlgPlik.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Try
            Dim k = Korytarz.WczytajTrase(dlgPlik.FileName)
            If Not k.Poprawny Then
                _korytarz = Nothing
                txtPlik.Text = dlgPlik.FileName
                Opis("Plik nie zawiera śladu, trasy ani linii (GPX: trk, rte, wpt; KML: LineString, gx:Track).", Color.Red)
                btnOK.Enabled = False
                Exit Sub
            End If
            _korytarz = k
            txtPlik.Text = dlgPlik.FileName
        Catch ex As Exception
            _korytarz = Nothing
            txtPlik.Text = dlgPlik.FileName
            Opis("Nie udało się odczytać trasy: " & ex.Message, Color.Red)
            btnOK.Enabled = False
            Exit Sub
        End Try
        OdswiezOpis()
    End Sub

    Private Sub nudSzerokosc_ValueChanged(sender As Object, e As EventArgs) Handles nudSzerokosc.ValueChanged
        OdswiezOpis()
    End Sub

    Private Sub Opis(tekst As String, kolor As Color)
        lblOpis.ForeColor = kolor
        lblOpis.Text = tekst
    End Sub

    ''' <summary>Długość trasy i liczba segmentów korytarza.</summary>
    Private Sub OdswiezOpis()
        If _korytarz Is Nothing Then
            btnOK.Enabled = False
            Exit Sub
        End If
        _korytarz.SzerokoscMetry = CDbl(nudSzerokosc.Value)
        Dim uklad = Ustawienia.Uklad
        Dim siatka = Form1.SiatkaDlaZasiegu(_korytarz.Zasieg(uklad))
        Dim tekst As String = "Trasa: " & If(_korytarz.Nazwa <> "", _korytarz.Nazwa & ", ", "") & _korytarz.DlugoscKm.ToString("0.0") & " km, " &
                              _korytarz.LiczbaPunktow & " punktów."
        Dim szacunek As Double = _korytarz.SzacowanaLiczbaSegmentow(siatka, uklad)
        If Not siatka.Poprawna Then
            Opis(tekst & vbCrLf & "Wpisz w oknie głównym rozmiar piksela i segmentu, aby zobaczyć liczbę segmentów.", Color.Black)
        ElseIf szacunek > Form1.MaksSegmentowKorytarzaNaBiezaco Then
            'bez wyznaczania milionów segmentów przy każdej zmianie szerokości pasa
            Opis(tekst & vbCrLf & "Pobranych zostanie ok. " & Math.Round(szacunek).ToString("N0") & " segmentów (szacunek). " &
                 "Liczba dotyczy bieżącego rozmiaru piksela (" & Form1.RozmiarPiksela & ") i segmentu (" & Form1.BokSegmentu & " px).", Color.Black)
        Else
            Dim n As Integer = _korytarz.Segmenty(siatka, uklad).Count
            Dim wszystkie As Long = siatka.LiczbaSegmentow
            Opis(tekst & vbCrLf & "Pobranych zostanie " & n & " segmentów zamiast " & wszystkie & " w prostokącie obejmującym trasę" &
                 If(wszystkie > 0, " (" & Math.Round(100.0 * (wszystkie - n) / wszystkie) & "% mniej danych).", ".") & vbCrLf &
                 "Liczba dotyczy bieżącego rozmiaru piksela (" & Form1.RozmiarPiksela & ") i segmentu (" & Form1.BokSegmentu & " px).", Color.Black)
        End If
        btnOK.Enabled = True
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If _korytarz Is Nothing Then Exit Sub
        _korytarz.SzerokoscMetry = CDbl(nudSzerokosc.Value)
        _Wynik = _korytarz
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Async Sub btnNmt_Click(sender As Object, e As EventArgs) Handles btnNmt.Click
        If _korytarz Is Nothing OrElse Not _korytarz.Poprawny Then Exit Sub
        Dim punkty = _korytarz.Linie.SelectMany(Function(l) l).ToList()
        If punkty.Count = 0 Then Exit Sub

        btnNmt.Enabled = False
        btnZapiszGpx.Enabled = False
        prgNmt.Value = 0
        prgNmt.Visible = True

        _pobieranieCts = New CancellationTokenSource()
        Dim postep = New Progress(Of Integer)(Sub(p)
                                                  If Not Me.IsDisposed AndAlso prgNmt.Visible Then
                                                      prgNmt.Value = Math.Max(0, Math.Min(100, p))
                                                  End If
                                              End Sub)
        Try
            Dim h = Await ProfilNmt.PobierzWysokosciAsync(punkty, _pobieranieCts.Token, postep)
            _linieWysokosci.Clear()
            Dim offset = 0
            For Each l In _korytarz.Linie
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
            MsgBox("Nie udało się pobrać danych wysokościowych z Geoportalu: " & ex.Message, MsgBoxStyle.Exclamation)
        Finally
            btnNmt.Enabled = True
            prgNmt.Visible = False
        End Try
    End Sub

    Private Sub btnZapiszGpx_Click(sender As Object, e As EventArgs) Handles btnZapiszGpx.Click
        If _korytarz Is Nothing OrElse _linieWysokosci.Count = 0 Then Exit Sub
        Dim nazwaPliku = If(_korytarz.Nazwa <> "", _korytarz.Nazwa, "trasa") & "_nmt.gpx"
        dlgZapiszGpx.FileName = nazwaPliku
        If dlgZapiszGpx.ShowDialog(Me) = DialogResult.OK Then
            Try
                Korytarz.ZapiszGpxZWysokosciami(dlgZapiszGpx.FileName, _korytarz.Nazwa, _korytarz.Linie, _linieWysokosci)
                MsgBox("Zapisano ślad GPX z wysokościami NMT do:" & vbCrLf & dlgZapiszGpx.FileName, MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox("Błąd zapisu pliku GPX: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub picProfil_Paint(sender As Object, e As PaintEventArgs) Handles picProfil.Paint
        Dim g = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim r = picProfil.ClientRectangle

        If _statystyki Is Nothing OrElse _statystyki.Punkty.Count < 2 Then
            Dim info = "Wskaż trasę i kliknij ""Wysokości NMT (LIDAR)..."", aby pobrać profil rzędnych z Geoportalu."
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
        If _pobieranieCts IsNot Nothing Then
            _pobieranieCts.Cancel()
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class
