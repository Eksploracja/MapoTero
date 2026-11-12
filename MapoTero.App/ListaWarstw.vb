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

Imports System.ComponentModel
Imports MapoTero.Core

''' <summary>Przeniesienie warstwy na inną pozycję (indeksy w kolejności nakładania, 0 - podkład).</summary>
Public Class PrzeniesienieWarstwyEventArgs
    Inherits EventArgs
    Public ReadOnly Property ZIndeksu As Integer
    Public ReadOnly Property NaIndeks As Integer
    Public Sub New(zIndeksu As Integer, naIndeks As Integer)
        Me.ZIndeksu = zIndeksu
        Me.NaIndeks = naIndeks
    End Sub
End Class

''' <summary>
''' Lista wybranych warstw („kanapka”): 12 miejsc, podkład (warstwa 1) na dole, krycie mniejsze niż 100% na końcu wiersza;
''' gdy miejsca nie mieszczą się w kontrolce - pasek przewijania (kółko myszy, przeciąganie przy krawędzi).
''' W tle, w prawym dolnym rogu, rysunek kanapki - widoczny przy pustej liście i zasłaniany przez kolejne warstwy
''' (puste miejsca są przezroczyste, zajęte - mają tło). Zwykły ListBox nie pozwala na obraz pod wierszami.
''' Kontrolka nie zmienia zestawu sama - zgłasza zdarzenia, a zmiany wykonuje okno (sprawdza, czy nie trwa pobieranie).
''' </summary>
Public Class ListaWarstw
    Inherits Control

    ''' <summary>Zmiana zaznaczonej warstwy.</summary>
    Public Event ZaznaczenieZmienione As EventHandler
    ''' <summary>Przeciągnięcie warstwy myszą lub Ctrl+strzałka.</summary>
    Public Event PrzeniesienieZadane As EventHandler(Of PrzeniesienieWarstwyEventArgs)
    ''' <summary>Klawisz Delete na zaznaczonej warstwie.</summary>
    Public Event UsuniecieZadane As EventHandler

    Private _zestaw As New ZestawWarstw()
    Private _zaznaczona As Integer = -1

    'przeciąganie warstwy myszą
    Private _chwyconaWarstwa As Integer = -1
    Private _punktChwytu As Point
    Private _przeciaganie As Boolean
    Private _cel As Integer = -1

    Private _wierszPodpowiedzi As Integer = -2

    ''' <summary>
    ''' Pasek przewijania - gdy 12 miejsc o czytelnej wysokości nie mieści się w kontrolce (panel wybranych warstw
    ''' został obniżony, żeby zrobić miejsce na drzewo warstw). Widoczny fragment pokazuje zaznaczoną warstwę.
    ''' </summary>
    Private ReadOnly _pasek As New VScrollBar() With {.Dock = DockStyle.Right, .Visible = False, .TabStop = False}

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
        TabStop = True
        BackColor = SystemColors.Window
        Controls.Add(_pasek)
        AddHandler _pasek.ValueChanged, Sub() Invalidate()
    End Sub

    ''' <summary>Wyświetlany zestaw warstw (po zmianie zestawu wywołaj <see cref="Odswiez"/>).</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Zestaw As ZestawWarstw
        Get
            Return _zestaw
        End Get
        Set(value As ZestawWarstw)
            _zestaw = If(value, New ZestawWarstw())
            Odswiez()
        End Set
    End Property

    ''' <summary>Indeks zaznaczonej warstwy (kolejność nakładania); -1 - brak zaznaczenia.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Zaznaczona As Integer
        Get
            Return If(_zaznaczona < _zestaw.Liczba, _zaznaczona, -1)
        End Get
        Set(value As Integer)
            Dim nowa As Integer = If(value >= 0 AndAlso value < _zestaw.Liczba, value, -1)
            If nowa = _zaznaczona Then Exit Property
            _zaznaczona = nowa
            PokazMiejsce(nowa)
            Invalidate()
            RaiseEvent ZaznaczenieZmienione(Me, EventArgs.Empty)
        End Set
    End Property

    ''' <summary>Obraz w tle listy (prawy dolny róg).</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ObrazTla As Image

    ''' <summary>Menu warstwy (prawy przycisk myszy na zajętym wierszu, klawisz menu).</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property MenuWarstwy As ContextMenuStrip

    ''' <summary>Podpowiedź z pełną nazwą warstwy pod kursorem.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Podpowiedz As ToolTip

    ''' <summary>Nazwa wyświetlana warstwy (np. z katalogu warstw); domyślnie nazwa warstwy na serwerze.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property NazwaWarstwy As Func(Of WarstwaWybrana, String) = Function(w) w.Nazwa

    ''' <summary>Źródło warstwy pokazywane szarym przy warstwach z różnych serwerów; domyślnie nazwa zbioru map.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ZrodloWarstwy As Func(Of WarstwaWybrana, String) = Function(w) w.Zbior

    ''' <summary>Opis listy wyświetlany, gdy kursor nie wskazuje warstwy.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property OpisListy As String = ""

    ''' <summary>Przerysowanie po zmianie zestawu; zaznaczenie poza listą jest usuwane.</summary>
    Public Sub Odswiez()
        If _zaznaczona >= _zestaw.Liczba Then
            _zaznaczona = _zestaw.Liczba - 1
            RaiseEvent ZaznaczenieZmienione(Me, EventArgs.Empty)
        End If
        _wierszPodpowiedzi = -2
        UstawPasek()
        PokazMiejsce(If(Zaznaczona >= 0, Zaznaczona, _zestaw.Liczba - 1))
        Invalidate()
    End Sub

    ''' <summary>Wysokość miejsca: całe wnętrze podzielone na 12, ale nie mniej niż wiersz tekstu (niżej - przewijanie).</summary>
    Private ReadOnly Property WysokoscWiersza As Integer
        Get
            Return Math.Max(Font.Height + 2, (ClientSize.Height - 4) \ ZestawWarstw.MaksLiczba)
        End Get
    End Property

    ''' <summary>Wysokość wnętrza listy (bez ramki).</summary>
    Private ReadOnly Property WysokoscWnetrza As Integer
        Get
            Return Math.Max(1, ClientSize.Height - 4)
        End Get
    End Property

    ''' <summary>Szerokość obszaru miejsc (bez paska przewijania).</summary>
    Private ReadOnly Property SzerokoscWnetrza As Integer
        Get
            Return ClientSize.Width - If(_pasek.Visible, _pasek.Width, 0)
        End Get
    End Property

    ''' <summary>Pasek przewijania widoczny, gdy 12 miejsc nie mieści się we wnętrzu listy.</summary>
    Private Sub UstawPasek()
        Dim tresc As Integer = WysokoscWiersza * ZestawWarstw.MaksLiczba
        Dim przewijanie As Boolean = tresc > WysokoscWnetrza
        If przewijanie Then
            _pasek.Minimum = 0
            _pasek.Maximum = tresc - 1
            _pasek.LargeChange = WysokoscWnetrza
            _pasek.SmallChange = WysokoscWiersza
            If _pasek.Value > tresc - WysokoscWnetrza Then _pasek.Value = tresc - WysokoscWnetrza
        Else
            _pasek.Value = 0
        End If
        _pasek.Visible = przewijanie
    End Sub

    ''' <summary>Przewija listę tak, aby miejsce o indeksie było widoczne (bez przewijania - nic nie robi).</summary>
    Private Sub PokazMiejsce(indeks As Integer)
        If Not _pasek.Visible Then Exit Sub
        If indeks < 0 Then indeks = 0
        Dim h As Integer = WysokoscWiersza
        Dim gora As Integer = (ZestawWarstw.MaksLiczba - 1 - indeks) * h
        Dim maks As Integer = _pasek.Maximum - _pasek.LargeChange + 1
        If gora < _pasek.Value Then
            _pasek.Value = Math.Max(0, gora)
        ElseIf gora + h > _pasek.Value + WysokoscWnetrza Then
            _pasek.Value = Math.Min(maks, gora + h - WysokoscWnetrza)
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        UstawPasek()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        UstawPasek()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        If Not _pasek.Visible Then Exit Sub
        Dim maks As Integer = _pasek.Maximum - _pasek.LargeChange + 1
        _pasek.Value = Math.Max(0, Math.Min(maks, _pasek.Value - Math.Sign(e.Delta) * 2 * WysokoscWiersza))
    End Sub

    ''' <summary>Prostokąt miejsca warstwy o indeksie (miejsce 12 u góry, podkład na dole), z uwzględnieniem przewinięcia.</summary>
    Private Function ProstokatMiejsca(indeks As Integer) As Rectangle
        Dim h As Integer = WysokoscWiersza
        Dim gora As Integer = If(_pasek.Visible, 2 - _pasek.Value, 2 + (WysokoscWnetrza - h * ZestawWarstw.MaksLiczba) \ 2)
        Return New Rectangle(2, gora + (ZestawWarstw.MaksLiczba - 1 - indeks) * h, SzerokoscWnetrza - 4, h)
    End Function

    ''' <summary>Indeks miejsca pod punktem (-1 - poza miejscami albo poza widocznym wnętrzem listy).</summary>
    Private Function MiejscePod(p As Point) As Integer
        If p.Y < 2 OrElse p.Y >= ClientSize.Height - 2 Then Return -1
        For i = 0 To ZestawWarstw.MaksLiczba - 1
            If ProstokatMiejsca(i).Contains(p) Then Return i
        Next
        Return -1
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.Clear(If(Enabled, BackColor, SystemColors.Control))

        If ObrazTla IsNot Nothing Then
            Dim skala As Double = Math.Min(1.0, Math.Min((SzerokoscWnetrza - 6) / ObrazTla.Width, (ClientSize.Height - 6) / ObrazTla.Height))
            Dim w As Integer = CInt(ObrazTla.Width * skala), h As Integer = CInt(ObrazTla.Height * skala)
            g.DrawImage(ObrazTla, New Rectangle(SzerokoscWnetrza - w - 4, ClientSize.Height - h - 3, w, h))
        End If
        'przewinięte miejsca nie wchodzą na ramkę
        g.SetClip(New Rectangle(2, 2, SzerokoscWnetrza - 4, WysokoscWnetrza))

        Dim flagi = TextFormatFlags.SingleLine Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or TextFormatFlags.EndEllipsis
        Dim flagiKrycia = TextFormatFlags.SingleLine Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or TextFormatFlags.NoPadding
        'zmienna lokalna nie może nazywać się jak właściwość Zaznaczona (VB nie rozróżnia wielkości liter)
        Dim wybrana As Integer = Zaznaczona
        Dim wieleZrodel As Boolean = _zestaw.WieleZrodel
        For i = 0 To ZestawWarstw.MaksLiczba - 1
            Dim r = ProstokatMiejsca(i)
            Dim numer As String = (i + 1).ToString() & ")"
            If i >= _zestaw.Liczba Then
                TextRenderer.DrawText(g, numer, Font, New Rectangle(r.X + 2, r.Y, r.Width - 2, r.Height), SystemColors.GrayText, flagi)
                Continue For
            End If
            Dim w = _zestaw(i)
            Dim tlo As Color = BackColor, kolor As Color = ForeColor
            If i = wybrana Then
                tlo = If(Focused, SystemColors.Highlight, SystemColors.ControlLight)
                kolor = If(Focused, SystemColors.HighlightText, ForeColor)
            End If
            If Not Enabled Then kolor = SystemColors.GrayText
            Using pedzel As New SolidBrush(tlo)
                g.FillRectangle(pedzel, r)
            End Using
            Dim krycie As String = If(w.Krycie < ZestawWarstw.PelneKrycie, w.Krycie & " %", "")
            Dim szerKrycia As Integer = If(krycie = "", 0, TextRenderer.MeasureText(g, krycie, Font, Size.Empty, flagiKrycia).Width + 4)
            Dim obszarNazwy As New Rectangle(r.X + 2, r.Y, r.Width - 4 - szerKrycia, r.Height)
            Dim tekstNazwy As String = numer & " " & NazwaWarstwy(w)
            TextRenderer.DrawText(g, tekstNazwy, Font, obszarNazwy, kolor, flagi)
            'warstwy z różnych serwerów - po nazwie (szarym) zbiór map, jeśli się zmieści
            Dim zrodlo As String = ZrodloWarstwy(w)
            If wieleZrodel AndAlso zrodlo <> "" Then
                Dim szerNazwy As Integer = TextRenderer.MeasureText(g, tekstNazwy, Font, Size.Empty, flagiKrycia).Width + 6
                If szerNazwy < obszarNazwy.Width - 20 Then
                    TextRenderer.DrawText(g, zrodlo, Font, New Rectangle(obszarNazwy.X + szerNazwy, r.Y, obszarNazwy.Width - szerNazwy, r.Height),
                                          If(i = wybrana AndAlso Focused, kolor, SystemColors.GrayText), flagi)
                End If
            End If
            If krycie <> "" Then
                TextRenderer.DrawText(g, krycie, Font, New Rectangle(r.Right - szerKrycia - 2, r.Y, szerKrycia, r.Height),
                                      If(i = wybrana AndAlso Focused, kolor, SystemColors.GrayText), flagiKrycia Or TextFormatFlags.Right)
            End If
            If i = wybrana AndAlso Focused Then ControlPaint.DrawFocusRectangle(g, r, kolor, tlo)
        Next

        'miejsce, na które trafi przeciągana warstwa
        If _przeciaganie AndAlso _cel >= 0 AndAlso _cel <> _chwyconaWarstwa Then
            Dim r = ProstokatMiejsca(_cel)
            Dim y As Integer = If(_cel > _chwyconaWarstwa, r.Top, r.Bottom - 1)
            Using pioro As New Pen(SystemColors.Highlight, 2)
                g.DrawLine(pioro, r.Left, y, r.Right, y)
            End Using
        End If

        g.ResetClip()
        ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Sunken)
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Focus()
        Dim i As Integer = MiejscePod(e.Location)
        If i >= _zestaw.Liczba Then i = -1
        If e.Button = MouseButtons.Left OrElse e.Button = MouseButtons.Right Then
            If i >= 0 Then Zaznaczona = i
        End If
        If e.Button = MouseButtons.Left AndAlso i >= 0 Then
            _chwyconaWarstwa = i
            _punktChwytu = e.Location
        End If
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If _chwyconaWarstwa >= 0 AndAlso e.Button = MouseButtons.Left Then
            If Not _przeciaganie AndAlso Math.Abs(e.Y - _punktChwytu.Y) > SystemInformation.DragSize.Height Then
                _przeciaganie = True
                Cursor = Cursors.SizeNS
            End If
            If _przeciaganie Then
                'przeciąganie przy górnej lub dolnej krawędzi przewija listę
                If _pasek.Visible Then
                    Dim h As Integer = WysokoscWiersza, maks As Integer = _pasek.Maximum - _pasek.LargeChange + 1
                    If e.Y < 2 + h \ 2 Then
                        _pasek.Value = Math.Max(0, _pasek.Value - h)
                    ElseIf e.Y > ClientSize.Height - 2 - h \ 2 Then
                        _pasek.Value = Math.Min(maks, _pasek.Value + h)
                    End If
                End If
                Dim cel As Integer = MiejscePod(e.Location)
                If cel < 0 Then cel = If(e.Y < ClientSize.Height \ 2, _zestaw.Liczba - 1, 0)
                cel = Math.Min(cel, _zestaw.Liczba - 1)
                If cel <> _cel Then
                    _cel = cel
                    Invalidate()
                End If
            End If
            Exit Sub
        End If
        PokazPodpowiedz(MiejscePod(e.Location))
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            Dim z As Integer = _chwyconaWarstwa, cel As Integer = _cel
            Dim przeniesienie As Boolean = _przeciaganie AndAlso cel >= 0 AndAlso cel <> z
            ZakonczPrzeciaganie()
            If przeniesienie Then RaiseEvent PrzeniesienieZadane(Me, New PrzeniesienieWarstwyEventArgs(z, cel))
        ElseIf e.Button = MouseButtons.Right Then
            Dim i As Integer = MiejscePod(e.Location)
            If i >= 0 AndAlso i < _zestaw.Liczba AndAlso MenuWarstwy IsNot Nothing Then MenuWarstwy.Show(Me, e.Location)
        End If
    End Sub

    Protected Overrides Sub OnMouseCaptureChanged(e As EventArgs)
        MyBase.OnMouseCaptureChanged(e)
        If _przeciaganie AndAlso Not Capture Then ZakonczPrzeciaganie()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        PokazPodpowiedz(-1)
    End Sub

    Private Sub ZakonczPrzeciaganie()
        _chwyconaWarstwa = -1
        _cel = -1
        If _przeciaganie Then
            _przeciaganie = False
            Cursor = Cursors.Default
            Invalidate()
        End If
    End Sub

    Private Sub PokazPodpowiedz(miejsce As Integer)
        If Podpowiedz Is Nothing OrElse miejsce = _wierszPodpowiedzi Then Exit Sub
        _wierszPodpowiedzi = miejsce
        Dim tekst As String = OpisListy
        If miejsce >= 0 AndAlso miejsce < _zestaw.Liczba Then
            Dim w = _zestaw(miejsce)
            Dim nazwa As String = NazwaWarstwy(w)
            tekst = (miejsce + 1) & ") " & nazwa & If(miejsce = 0, " - podkład", "") &
                    If(w.Krycie < ZestawWarstw.PelneKrycie, ", krycie " & w.Krycie & " %", "") & vbCrLf &
                    "Warstwa " & w.Nazwa & ", zbiór map " & If(w.Zbior <> "", w.Zbior, "?") & " (" & KafleXyz.NazwaTypu(w.EfektywnyTyp) & ")" &
                    If(w.ZalecanyPiksel > 0, ", zalecany piksel " & w.ZalecanyPiksel.ToString("0.###") & " m", "") & vbCrLf &
                    "Przeciągnij, aby zmienić kolejność; prawy przycisk - menu warstwy."
        End If
        Podpowiedz.SetToolTip(Me, tekst)
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Up, Keys.Down, Keys.Delete, Keys.Home, Keys.End
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Dim i As Integer = Zaznaczona
        Select Case e.KeyCode
            Case Keys.Delete
                If i >= 0 Then RaiseEvent UsuniecieZadane(Me, EventArgs.Empty)
            Case Keys.Up
                'w górę listy - wyżej w kanapce (większy indeks)
                If e.Control Then
                    If i >= 0 AndAlso i < _zestaw.Liczba - 1 Then RaiseEvent PrzeniesienieZadane(Me, New PrzeniesienieWarstwyEventArgs(i, i + 1))
                ElseIf _zestaw.Liczba > 0 Then
                    Zaznaczona = If(i < 0, 0, Math.Min(_zestaw.Liczba - 1, i + 1))
                End If
            Case Keys.Down
                If e.Control Then
                    If i > 0 Then RaiseEvent PrzeniesienieZadane(Me, New PrzeniesienieWarstwyEventArgs(i, i - 1))
                ElseIf _zestaw.Liczba > 0 Then
                    Zaznaczona = If(i < 0, 0, Math.Max(0, i - 1))
                End If
            Case Keys.Home
                If _zestaw.Liczba > 0 Then Zaznaczona = _zestaw.Liczba - 1
            Case Keys.End
                If _zestaw.Liczba > 0 Then Zaznaczona = 0
            Case Keys.Apps
                If i >= 0 AndAlso MenuWarstwy IsNot Nothing Then
                    Dim r = ProstokatMiejsca(i)
                    MenuWarstwy.Show(Me, New Point(r.Left + 10, r.Bottom))
                End If
            Case Else
                Exit Sub
        End Select
        e.Handled = True
    End Sub

End Class
