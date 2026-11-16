'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Okno interaktywnego podglądu i strojenia korekcji obrazu dla ekranów transflektywnych Garmin (MIP / Chroma).
''' </summary>
Public Class FormPodgladMip

    Private _oryginalBmp As Bitmap
    Private _opcje As OpcjeKorekcjiMip

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Opcje As OpcjeKorekcjiMip
        Get
            Return _opcje
        End Get
        Set(value As OpcjeKorekcjiMip)
            _opcje = If(value IsNot Nothing, value.Kopia(), New OpcjeKorekcjiMip())
        End Set
    End Property

    Public Sub New(opcje As OpcjeKorekcjiMip, Optional sciezkaProbki As String = "")
        InitializeComponent()
        Me.Opcje = opcje
        WczytajObrazProbki(sciezkaProbki)
    End Sub

    Private Sub FormPodgladMip_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UstawKontrolkiZOpcji()
        OdswiezPodglad()
    End Sub

    Private Sub WczytajObrazProbki(sciezka As String)
        Try
            If Not String.IsNullOrEmpty(sciezka) AndAlso File.Exists(sciezka) Then
                Using tmp As New Bitmap(sciezka)
                    _oryginalBmp = New Bitmap(240, 240, Imaging.PixelFormat.Format32bppArgb)
                    Using g = Graphics.FromImage(_oryginalBmp)
                        g.DrawImage(tmp, New Rectangle(0, 0, 240, 240))
                    End Using
                End Using
                Return
            End If
        Catch
        End Try

        ' Jeśli brak segmentu - generujemy syntetyczną próbkę mapy z terenem, lasem, drogą i poziomicami
        _oryginalBmp = GenerujSyntetycznaProbke()
    End Sub

    Private Shared Function GenerujSyntetycznaProbke() As Bitmap
        Dim bmp As New Bitmap(240, 240, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            ' Tło: las / zieleń
            g.Clear(Color.FromArgb(70, 105, 75))
            ' Polana / pole
            Using bPole As New SolidBrush(Color.FromArgb(145, 155, 120))
                g.FillEllipse(bPole, 40, 30, 160, 100)
            End Using
            ' Rzeka
            Using pRzeka As New Pen(Color.FromArgb(45, 110, 170), 5.0!)
                g.DrawBezier(pRzeka, New Point(10, 10), New Point(80, 100), New Point(150, 60), New Point(230, 220))
            End Using
            ' Droga gruntowa
            Using pDroga As New Pen(Color.FromArgb(115, 125, 105), 3.0!)
                g.DrawLine(pDroga, 20, 220, 220, 30)
            End Using
            ' Poziomice
            Using pPoziomica As New Pen(Color.FromArgb(110, 85, 65), 1.0!)
                g.DrawArc(pPoziomica, 30, 50, 180, 140, 0, 180)
                g.DrawArc(pPoziomica, 50, 70, 140, 100, 0, 180)
            End Using
        End Using
        Return bmp
    End Function

    Private Sub UstawKontrolkiZOpcji()
        tbGamma.Value = Math.Max(tbGamma.Minimum, Math.Min(tbGamma.Maximum, CInt(Math.Round(_opcje.Gamma * 100))))
        tbKontrast.Value = Math.Max(tbKontrast.Minimum, Math.Min(tbKontrast.Maximum, CInt(Math.Round(_opcje.Kontrast * 100))))
        tbNasycenie.Value = Math.Max(tbNasycenie.Minimum, Math.Min(tbNasycenie.Maximum, CInt(Math.Round(_opcje.Nasycenie * 100))))
        tbOstrz.Value = Math.Max(tbOstrz.Minimum, Math.Min(tbOstrz.Maximum, CInt(Math.Round(_opcje.Wyostrzanie * 100))))
        AktualizujEtykiety()
    End Sub

    Private Sub AktualizujEtykiety()
        Dim gVal As Double = tbGamma.Value / 100.0
        Dim kVal As Double = tbKontrast.Value / 100.0
        Dim nVal As Double = tbNasycenie.Value / 100.0
        Dim oVal As Double = tbOstrz.Value / 100.0

        lblGammaVal.Text = gVal.ToString("0.00", Globalization.CultureInfo.InvariantCulture)
        lblKontrastVal.Text = If(kVal >= 1.0, "+", "") & Math.Round((kVal - 1.0) * 100).ToString() & "%"
        lblNasycenieVal.Text = If(nVal >= 1.0, "+", "") & Math.Round((nVal - 1.0) * 100).ToString() & "%"
        lblOstrzVal.Text = oVal.ToString("0.00", Globalization.CultureInfo.InvariantCulture)

        _opcje.Gamma = gVal
        _opcje.Kontrast = kVal
        _opcje.Nasycenie = nVal
        _opcje.Wyostrzanie = oVal
        _opcje.Wlaczona = True
    End Sub

    Private Sub OdswiezPodglad()
        If _oryginalBmp Is Nothing Then Exit Sub

        If rbOryginal.Checked Then
            picPodglad.Image = _oryginalBmp
        Else
            Dim przetworzony As New Bitmap(_oryginalBmp)
            NakladanieObrazow.ZastosujKorekcjeMip(przetworzony, _opcje)
            picPodglad.Image = przetworzony
        End If
    End Sub

    Private Sub Suwaki_Scroll(sender As Object, e As EventArgs) Handles tbGamma.Scroll, tbKontrast.Scroll, tbNasycenie.Scroll, tbOstrz.Scroll
        AktualizujEtykiety()
        If rbPodgladMip.Checked Then OdswiezPodglad()
    End Sub

    Private Sub rbPodglad_CheckedChanged(sender As Object, e As EventArgs) Handles rbPodgladMip.CheckedChanged, rbOryginal.CheckedChanged
        OdswiezPodglad()
    End Sub

    Private Sub btnDomyslne_Click(sender As Object, e As EventArgs) Handles btnDomyslne.Click
        _opcje = New OpcjeKorekcjiMip With {.Wlaczona = True}
        UstawKontrolkiZOpcji()
        OdswiezPodglad()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If Me.DialogResult = DialogResult.OK Then
            _opcje.Wlaczona = True
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class
