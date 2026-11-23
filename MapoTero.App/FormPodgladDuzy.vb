'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Duży podgląd stylu z kreatora: próbka 1024 × 1024 px w skali 1:1 (piksel jak w małym podglądzie), przewijana,
''' z obrysem obszaru mapy. Mały podgląd (374 × 211 px) nie pozwala ocenić czytelności napisów przed pobieraniem.
''' </summary>
Public Class FormPodgladDuzy
    Inherits Form

    Private Const BokProbki As Integer = 1024
    Private ReadOnly _silnik As New PodgladWarstwSilnik() With {.Bok = BokProbki}
    Private ReadOnly _panel As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True, .BackColor = Color.FromArgb(236, 239, 243)}
    Private ReadOnly _pic As New PictureBox With {.SizeMode = PictureBoxSizeMode.AutoSize, .Location = Point.Empty}
    Private ReadOnly _lblStan As New Label With {.Dock = DockStyle.Bottom, .Height = 24, .TextAlign = ContentAlignment.MiddleLeft,
                                                  .Padding = New Padding(8, 0, 0, 0), .ForeColor = Color.FromArgb(0, 100, 180)}
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _zestaw As ZestawWarstw
    Private ReadOnly _punkt As PunktXY
    Private ReadOnly _piksel As Double
    Private ReadOnly _zrodla As Dictionary(Of String, PlikWarstw)
    Private ReadOnly _obszar As Zasieg
    Private ReadOnly _korytarz As Korytarz
    ' Przesuwanie myszką
    Private _przeciaganie As Boolean
    Private _startMyszy As Point
    Private _startPrzewiniecia As Point

    ''' <summary>Okno do testów: zakończenie pobierania próbki (powodzenie lub błąd).</summary>
    Friend Event PodgladGotowy As EventHandler

    Public Sub New(tytul As String, zestaw As ZestawWarstw, punkt As PunktXY, piksel As Double,
                   zrodla As Dictionary(Of String, PlikWarstw), obszar As Zasieg, korytarz As Korytarz)
        _zestaw = zestaw
        _punkt = punkt
        _piksel = piksel
        _zrodla = zrodla
        _obszar = obszar
        _korytarz = korytarz
        Text = "Duży podgląd - " & tytul
        Font = New Font("Segoe UI", 9.0!)
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        ShowInTaskbar = False
        Dim ekran = Screen.FromPoint(Cursor.Position).WorkingArea
        ClientSize = New Size(Math.Min(BokProbki + 20, ekran.Width - 80), Math.Min(BokProbki + 44, ekran.Height - 80))
        _panel.Controls.Add(_pic)
        Controls.Add(_panel)
        Controls.Add(_lblStan)
        _lblStan.Text = "Pobieranie próbki terenu..."
        _pic.Cursor = Cursors.Hand
        AddHandler _pic.MouseDown, AddressOf Pic_MouseDown
        AddHandler _pic.MouseMove, AddressOf Pic_MouseMove
        AddHandler _pic.MouseUp, Sub() _przeciaganie = False
        PomocProgramu.ZamykanieKlawiszemEsc(Me, True)
    End Sub

    ''' <summary>Zasięg próbki w PL-1992 (środek - punkt podglądu, bok 1024 px).</summary>
    Friend ReadOnly Property ZasiegProbki As Zasieg
        Get
            Dim pol = BokProbki * _piksel / 2.0
            Return New Zasieg(_punkt.X - pol, _punkt.Y - pol, _punkt.X + pol, _punkt.Y + pol)
        End Get
    End Property

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Dim token = _cts.Token
        Try
            _silnik.SprawdzIZaktualizujParametry(_punkt, _piksel, UkladWspolrzednych.PL1992, True)
            Dim zadanie As New ZadaniePobierania With {
                .Uklad = UkladWspolrzednych.PL1992,
                .Siatka = New Siatka(ZasiegProbki, _piksel, BokProbki),
                .Zbiory = _zestaw.Zbiory(),
                .ZrodlaMap = _zrodla,
                .Format = "png",
                .LimitCzasuSekundy = 20,
                .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_duzy") & "\"}
            Dim postep As New Progress(Of String)(Sub(msg)
                                                      If Not token.IsCancellationRequested AndAlso Not IsDisposed Then _lblStan.Text = msg
                                                  End Sub)
            Dim wynik = Await _silnik.PobierzPodgladAsync(zadanie, _zestaw, token, postep)
            If token.IsCancellationRequested OrElse IsDisposed Then
                wynik.Item1?.Dispose()
                Exit Sub
            End If
            If wynik.Item1 Is Nothing Then
                _lblStan.Text = "Brak podglądu z serwera"
                _lblStan.ForeColor = Color.FromArgb(180, 50, 0)
            Else
                RysujObszar(wynik.Item1)
                _pic.Image = wynik.Item1
                ' widok na środek próbki
                _panel.AutoScrollPosition = New Point(Math.Max(0, (_pic.Width - _panel.ClientSize.Width) \ 2),
                                                      Math.Max(0, (_pic.Height - _panel.ClientSize.Height) \ 2))
                _lblStan.Text = If(wynik.Item2.Count > 0, $"Niepełny podgląd: {wynik.Item2.Count} warstw bez odpowiedzi. ", "") &
                                $"Skala 1:1, piksel {_piksel:0.##} m - przeciągnij myszą, aby przesunąć widok."
                _lblStan.ForeColor = If(wynik.Item2.Count > 0, Color.FromArgb(180, 90, 0), Color.FromArgb(0, 130, 40))
            End If
        Catch ex As OperationCanceledException
            Exit Sub
        Catch ex As ObjectDisposedException
            Exit Sub
        Catch ex As Exception
            If IsDisposed Then Exit Sub
            _lblStan.Text = "Błąd podglądu: " & OpisBledu.Z(ex)
            _lblStan.ForeColor = Color.Red
        End Try
        RaiseEvent PodgladGotowy(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Obrys prostokąta albo pas trasy na obrazie próbki.</summary>
    Private Sub RysujObszar(obraz As Bitmap)
        If Not _obszar.Poprawny Then Exit Sub
        Dim z = ZasiegProbki
        Dim naObraz = Function(p As PunktXY) New PointF(CSng((p.Y - z.YLewy) / _piksel), CSng((z.XGora - p.X) / _piksel))
        Using g = Graphics.FromImage(obraz)
            g.SmoothingMode = SmoothingMode.AntiAlias
            If _korytarz IsNot Nothing Then
                Dim szer = CSng(Math.Max(1, 2 * _korytarz.SzerokoscMetry / _piksel))
                Using pas As New Pen(Color.FromArgb(70, 17, 88, 183), szer) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round, .LineJoin = LineJoin.Round},
                      os As New Pen(Color.FromArgb(220, 210, 20, 20), 2.0F)
                    For Each linia In _korytarz.Linie
                        Dim punkty = linia.Select(Function(p) naObraz(UkladWspolrzednych.PL1992.ZWgs84(p))).ToArray()
                        If punkty.Length > 1 Then
                            g.DrawLines(pas, punkty)
                            g.DrawLines(os, punkty)
                        End If
                    Next
                End Using
            Else
                Dim lg = naObraz(_obszar.LewyGorny)
                Dim pd = naObraz(_obszar.PrawyDolny)
                Using tlo As New Pen(Color.White, 4), linia As New Pen(Color.FromArgb(17, 88, 183), 2)
                    g.DrawRectangle(tlo, lg.X, lg.Y, pd.X - lg.X, pd.Y - lg.Y)
                    g.DrawRectangle(linia, lg.X, lg.Y, pd.X - lg.X, pd.Y - lg.Y)
                End Using
            End If
        End Using
    End Sub

    Private Sub Pic_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Exit Sub
        _przeciaganie = True
        _startMyszy = Cursor.Position
        _startPrzewiniecia = New Point(-_panel.AutoScrollPosition.X, -_panel.AutoScrollPosition.Y)
    End Sub

    Private Sub Pic_MouseMove(sender As Object, e As MouseEventArgs)
        If Not _przeciaganie Then Exit Sub
        Dim p = Cursor.Position
        _panel.AutoScrollPosition = New Point(_startPrzewiniecia.X - (p.X - _startMyszy.X), _startPrzewiniecia.Y - (p.Y - _startMyszy.Y))
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _cts.Cancel()
        _cts.Dispose()
        _silnik.Dispose()
        Dim obraz = _pic.Image
        _pic.Image = Nothing
        obraz?.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

End Class
