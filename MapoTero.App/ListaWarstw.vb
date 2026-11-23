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

    ''' <summary>
    ''' Timer opóźnienia podpowiedzi (500 ms = pół sekundy od zatrzymania kursora nad wierszem).
    ''' </summary>
    Private WithEvents _timerPodpowiedzi As New Timer() With {.Interval = 500}
    Private _oczekujaceMiejsce As Integer = -2
    Private _pokazaneMiejsce As Integer = -2

    ''' <summary>
    ''' Pasek przewijania - gdy 12 miejsc o czytelnej wysokości nie mieści się w kontrolce (panel wybranych warstw
    ''' został obniżony, żeby zrobić miejsce na drzewo warstw). Widoczny fragment pokazuje zaznaczoną warstwę.
    ''' </summary>
    Private ReadOnly _pasek As New VScrollBar() With {.Dock = DockStyle.Right, .Visible = False, .TabStop = False}

    Private _edytorKrycia As Control
    'wiersz, nad którego krzyżykiem jest kursor (podświetlenie na czerwono)
    Private _krzyzykPod As Integer = -1

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
        TabStop = True
        BackColor = SystemColors.Window
        Controls.Add(_pasek)
        AddHandler _pasek.ValueChanged, Sub()
                                            Invalidate()
                                            PolozEdytorKrycia()
                                        End Sub
    End Sub

    ''' <summary>
    ''' Pole krycia (np. NumericUpDown okna) pokazywane w wierszu zaznaczonej warstwy, w miejscu napisu „80 %”.
    ''' Pozostałe wiersze rysują krycie jako tekst w ramce - jedno pole zamiast osobnego wiersza pod listą.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EdytorKrycia As Control
        Get
            Return _edytorKrycia
        End Get
        Set(value As Control)
            If _edytorKrycia IsNot Nothing Then Controls.Remove(_edytorKrycia)
            _edytorKrycia = value
            If value IsNot Nothing Then
                value.Dock = DockStyle.None
                value.Anchor = AnchorStyles.None
                value.Margin = Padding.Empty
                Controls.Add(value)
            End If
            UstawPasek()
            PolozEdytorKrycia()
        End Set
    End Property

    Private Function Skaluj(wartosc As Integer) As Integer
        Return CInt(wartosc * DeviceDpi / 96.0)
    End Function

    Private ReadOnly Property SzerokoscProcenta As Integer
        Get
            Return Skaluj(14)
        End Get
    End Property

    Private ReadOnly Property SzerokoscUchwytu As Integer
        Get
            Return If(_zestaw.Liczba > 1, Skaluj(12), 0)
        End Get
    End Property

    Private ReadOnly Property SzerokoscKrzyzyka As Integer
        Get
            Return Skaluj(24)
        End Get
    End Property

    Private Function ProstokatProcenta(r As Rectangle) As Rectangle
        Return New Rectangle(r.Right - SzerokoscKrzyzyka - SzerokoscProcenta, r.Y, SzerokoscProcenta, r.Height)
    End Function

    ''' <summary>Ramka krycia w wierszu (tam leży pole krycia zaznaczonej warstwy).</summary>
    Private Function ProstokatKrycia(r As Rectangle) As Rectangle
        Dim h As Integer = If(_edytorKrycia IsNot Nothing, _edytorKrycia.Height, Font.Height + Skaluj(6))
        Dim szer As Integer = If(_edytorKrycia IsNot Nothing, _edytorKrycia.Width, Skaluj(46))
        Return New Rectangle(r.Right - SzerokoscKrzyzyka - SzerokoscProcenta - szer - Skaluj(2), r.Y + (r.Height - h) \ 2, szer, h)
    End Function

    Private Function ProstokatKrzyzyka(r As Rectangle) As Rectangle
        Return New Rectangle(r.Right - SzerokoscKrzyzyka, r.Y, SzerokoscKrzyzyka, r.Height)
    End Function

    Friend Shared Function SkrotTrybuMieszania(tryb As TrybMieszania) As String
        Select Case tryb
            Case TrybMieszania.Mnozenie : Return "Mnoż."
            Case TrybMieszania.Ciemniej : Return "Ciemn."
            Case TrybMieszania.Jasniej : Return "Jaśn."
            Case TrybMieszania.Ekran : Return "Ekran"
            Case TrybMieszania.Nakladka : Return "Nakł."
            Case TrybMieszania.RzezbaTerenu : Return "Rzeźba"
            Case Else : Return ""
        End Select
    End Function

    ''' <summary>Pole krycia nad wierszem zaznaczonej warstwy; ukryte bez zaznaczenia albo gdy wiersz jest przewinięty poza listę.</summary>
    Private Sub PolozEdytorKrycia()
        If _edytorKrycia Is Nothing Then Exit Sub
        Dim i As Integer = Zaznaczona
        If i < 0 Then
            _edytorKrycia.Visible = False
            Exit Sub
        End If
        Dim k = ProstokatKrycia(ProstokatMiejsca(i))
        Dim widoczny As Boolean = k.Top >= 2 AndAlso k.Bottom <= ClientSize.Height - 2
        If widoczny Then _edytorKrycia.Location = k.Location
        _edytorKrycia.Visible = widoczny
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
            PolozEdytorKrycia()
            Invalidate()
            RaiseEvent ZaznaczenieZmienione(Me, EventArgs.Empty)
        End Set
    End Property

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
        UkryjPodpowiedz()
        _oczekujaceMiejsce = -2
        UstawPasek()
        PokazMiejsce(If(Zaznaczona >= 0, Zaznaczona, _zestaw.Liczba - 1))
        PolozEdytorKrycia()
        Invalidate()
    End Sub

    ''' <summary>Wysokość wiersza proporcjonalna do czcionki i skalowania okna; mieści pole krycia.</summary>
    Friend ReadOnly Property WysokoscWiersza As Integer
        Get
            Dim h As Integer = Font.Height + Skaluj(8)
            If _edytorKrycia IsNot Nothing Then h = Math.Max(h, _edytorKrycia.Height + Skaluj(6))
            Return h
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

    ''' <summary>Pasek przewijania widoczny, gdy wybrane warstwy nie mieszczą się w liście.</summary>
    Private Sub UstawPasek()
        Dim tresc As Integer = WysokoscWiersza * _zestaw.Liczba
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
        Dim gora As Integer = (_zestaw.Liczba - 1 - indeks) * h
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
        PolozEdytorKrycia()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        UstawPasek()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        UkryjPodpowiedz()
        _oczekujaceMiejsce = -2
        If Not _pasek.Visible Then Exit Sub
        Dim maks As Integer = _pasek.Maximum - _pasek.LargeChange + 1
        _pasek.Value = Math.Max(0, Math.Min(maks, _pasek.Value - Math.Sign(e.Delta) * 2 * WysokoscWiersza))
    End Sub

    ''' <summary>Prostokąt wiersza warstwy (wierzch kompozycji u góry, podkład na dole).</summary>
    Private Function ProstokatMiejsca(indeks As Integer) As Rectangle
        Dim h As Integer = WysokoscWiersza
        Dim gora As Integer = 2 - If(_pasek.Visible, _pasek.Value, 0)
        Return New Rectangle(2, gora + (_zestaw.Liczba - 1 - indeks) * h, SzerokoscWnetrza - 4, h)
    End Function

    ''' <summary>Indeks miejsca pod punktem (-1 - poza miejscami albo poza widocznym wnętrzem listy).</summary>
    Private Function MiejscePod(p As Point) As Integer
        If p.Y < 2 OrElse p.Y >= ClientSize.Height - 2 Then Return -1
        For i = 0 To _zestaw.Liczba - 1
            If ProstokatMiejsca(i).Contains(p) Then Return i
        Next
        Return -1
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.Clear(If(Enabled, BackColor, SystemColors.Control))

        If _zestaw.Liczba = 0 Then
            'tylko stan listy - co zrobić, podpowiada karta „Stan sesji” pod mapą (wcześniej ta sama rada była w trzech miejscach)
            TextRenderer.DrawText(g, "Brak wybranych warstw.", Font, Rectangle.Inflate(ClientRectangle, -6, -3),
                                  Color.FromArgb(75, 94, 113), TextFormatFlags.WordBreak Or TextFormatFlags.VerticalCenter)
            Return
        End If
        'przewinięte miejsca nie wchodzą na ramkę
        g.SetClip(New Rectangle(2, 2, SzerokoscWnetrza - 4, WysokoscWnetrza))

        Dim flagi = TextFormatFlags.SingleLine Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or TextFormatFlags.EndEllipsis
        Dim flagiKrycia = TextFormatFlags.SingleLine Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or TextFormatFlags.NoPadding
        'zmienna lokalna nie może nazywać się jak właściwość Zaznaczona (VB nie rozróżnia wielkości liter)
        Dim wybrana As Integer = Zaznaczona
        Dim wieleZrodel As Boolean = _zestaw.WieleZrodel
        Dim kolorRamki As Color = Color.FromArgb(207, 218, 229)
        For i = 0 To _zestaw.Liczba - 1
            Dim r = ProstokatMiejsca(i)
            Dim w = _zestaw(i)
            Dim tlo As Color = BackColor, kolor As Color = ForeColor
            If i = wybrana Then
                tlo = If(Focused, SystemColors.Highlight, Color.FromArgb(227, 238, 249))
                kolor = If(Focused, SystemColors.HighlightText, ForeColor)
            End If
            If Not Enabled Then kolor = SystemColors.GrayText
            Using pedzel As New SolidBrush(tlo)
                g.FillRectangle(pedzel, r)
            End Using
            'cienka linia między wierszami - bez numerów („2)”, „1)”) wiersze inaczej zlewały się ze sobą
            If i > 0 Then
                Using pioro As New Pen(Color.FromArgb(236, 241, 246))
                    g.DrawLine(pioro, r.Left + Skaluj(4), r.Bottom - 1, r.Right - Skaluj(4), r.Bottom - 1)
                End Using
            End If
            If _zestaw.Liczba > 1 Then
                Dim kropka = Math.Max(2, Skaluj(2))
                Dim odstepX = Math.Max(2, Skaluj(2))
                Dim odstepY = Math.Max(2, Skaluj(2))
                Dim calaWys = 3 * kropka + 2 * odstepY
                Dim gx = r.X + Skaluj(4)
                Dim gy = r.Y + (r.Height - calaWys) \ 2
                Dim kolorUchwytu = If(i = wybrana AndAlso Focused, kolor, Color.FromArgb(158, 172, 188))
                Using pedzelUchwytu As New SolidBrush(kolorUchwytu)
                    For kol = 0 To 1
                        For wierz = 0 To 2
                            g.FillRectangle(pedzelUchwytu, gx + kol * (kropka + odstepX), gy + wierz * (kropka + odstepY), kropka, kropka)
                        Next
                    Next
                End Using
            End If
            Dim lewaIkony = r.X + Skaluj(4) + SzerokoscUchwytu
            RysujIkoneWarstwy(g, New Rectangle(lewaIkony, r.Y, Skaluj(16), r.Height), i = 0, If(i = wybrana AndAlso Focused, kolor, Color.FromArgb(25, 78, 128)))
            Dim krycie = ProstokatKrycia(r)
            Dim lewaNazwy = lewaIkony + Skaluj(22)
            Dim prawaNazwy = krycie.Left - Skaluj(4)
            Dim skrotTrybu = SkrotTrybuMieszania(w.TrybMieszania)
            If skrotTrybu <> "" Then
                Using czcionkaTrybu As New Font(Font.FontFamily, Math.Max(7.0F, Font.Size - 1.25F), FontStyle.Regular)
                    Dim szerTekstu = TextRenderer.MeasureText(g, skrotTrybu, czcionkaTrybu, Size.Empty, flagiKrycia).Width
                    Dim szerZnacznika = szerTekstu + Skaluj(8)
                    If prawaNazwy - lewaNazwy - szerZnacznika - Skaluj(4) >= Skaluj(45) Then
                        Dim wysZnacznika = Math.Min(r.Height - Skaluj(6), czcionkaTrybu.Height + Skaluj(4))
                        Dim rectTrybu As New Rectangle(prawaNazwy - szerZnacznika, r.Y + (r.Height - wysZnacznika) \ 2, szerZnacznika, wysZnacznika)
                        Dim tloTrybu = If(i = wybrana AndAlso Focused, Color.FromArgb(60, 255, 255, 255), Color.FromArgb(232, 240, 250))
                        Dim ramkaTrybu = If(i = wybrana AndAlso Focused, Color.FromArgb(180, 255, 255, 255), Color.FromArgb(175, 196, 220))
                        Dim tekstTrybu = If(i = wybrana AndAlso Focused, kolor, Color.FromArgb(25, 78, 128))
                        Using pedzelTla As New SolidBrush(tloTrybu), pioroTrybu As New Pen(ramkaTrybu)
                            g.FillRectangle(pedzelTla, rectTrybu)
                            g.DrawRectangle(pioroTrybu, rectTrybu.X, rectTrybu.Y, rectTrybu.Width - 1, rectTrybu.Height - 1)
                        End Using
                        TextRenderer.DrawText(g, skrotTrybu, czcionkaTrybu, rectTrybu, tekstTrybu,
                                              flagiKrycia Or TextFormatFlags.HorizontalCenter)
                        prawaNazwy = rectTrybu.Left - Skaluj(4)
                    End If
                End Using
            End If
            Dim obszarNazwy As New Rectangle(lewaNazwy, r.Y, Math.Max(0, prawaNazwy - lewaNazwy), r.Height)
            'krycie zaznaczonej warstwy pokazuje pole edycji położone nad wierszem; pozostałe - napis w ramce
            If i <> wybrana OrElse _edytorKrycia Is Nothing OrElse Not _edytorKrycia.Visible Then
                Using tloPola As New SolidBrush(If(Enabled, Color.White, SystemColors.Control)), pioro As New Pen(kolorRamki)
                    g.FillRectangle(tloPola, krycie)
                    g.DrawRectangle(pioro, krycie.X, krycie.Y, krycie.Width - 1, krycie.Height - 1)
                End Using
                TextRenderer.DrawText(g, w.Krycie.ToString(), Font, New Rectangle(krycie.X, krycie.Y, krycie.Width - Skaluj(5), krycie.Height),
                                      If(Enabled, ForeColor, SystemColors.GrayText), flagiKrycia Or TextFormatFlags.Right)
            End If
            TextRenderer.DrawText(g, "%", Font, ProstokatProcenta(r),
                                  If(i = wybrana AndAlso Focused, kolor, SystemColors.GrayText),
                                  flagiKrycia Or TextFormatFlags.HorizontalCenter)
            Dim krzyzyk = ProstokatKrzyzyka(r)
            TextRenderer.DrawText(g, "✕", Font, krzyzyk, If(i = _krzyzykPod AndAlso Enabled, Color.Firebrick,
                                                             If(i = wybrana AndAlso Focused, kolor, SystemColors.GrayText)),
                                  flagiKrycia Or TextFormatFlags.HorizontalCenter)
            Dim tekstNazwy As String = NazwaWarstwy(w)
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
    End Sub

    ''' <summary>Symbol warstwy: równoległobok (arkusz mapy); podkład wypełniony, warstwy nad nim - sam obrys.</summary>
    Private Sub RysujIkoneWarstwy(g As Graphics, obszar As Rectangle, podklad As Boolean, kolor As Color)
        Dim s As Single = obszar.Width
        Dim x As Single = obszar.X, y As Single = obszar.Y + (obszar.Height - s * 0.6F) / 2
        Dim punkty = {New PointF(x + s * 0.3F, y), New PointF(x + s, y), New PointF(x + s * 0.7F, y + s * 0.6F), New PointF(x, y + s * 0.6F)}
        Dim tryb = g.SmoothingMode
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        If podklad Then
            Using pedzel As New SolidBrush(Color.FromArgb(150, kolor))
                g.FillPolygon(pedzel, punkty)
            End Using
        End If
        Using pioro As New Pen(kolor, Math.Max(1.0F, Skaluj(1) * 1.3F))
            g.DrawPolygon(pioro, punkty)
        End Using
        g.SmoothingMode = tryb
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

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If _timerPodpowiedzi IsNot Nothing Then
                _timerPodpowiedzi.Stop()
                _timerPodpowiedzi.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        UkryjPodpowiedz()
        _oczekujaceMiejsce = -2
        Focus()
        Dim i As Integer = MiejscePod(e.Location)
        If i >= _zestaw.Liczba Then i = -1
        If e.Button = MouseButtons.Left AndAlso i >= 0 Then
            Dim r = ProstokatMiejsca(i)
            If ProstokatKrzyzyka(r).Contains(e.Location) Then
                _zaznaczona = i
                RaiseEvent UsuniecieZadane(Me, EventArgs.Empty)
                Exit Sub
            End If
            If _edytorKrycia IsNot Nothing AndAlso ProstokatKrycia(r).Contains(e.Location) Then
                Zaznaczona = i
                _edytorKrycia.Focus()
                Exit Sub
            End If
        End If
        If (e.Button = MouseButtons.Left OrElse e.Button = MouseButtons.Right) AndAlso i >= 0 Then
            Zaznaczona = i
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
                UkryjPodpowiedz()
                _oczekujaceMiejsce = -2
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
                If cel < 0 Then cel = If(e.Y <= ProstokatMiejsca(_zestaw.Liczba - 1).Top, _zestaw.Liczba - 1, 0)
                cel = Math.Min(cel, _zestaw.Liczba - 1)
                If cel <> _cel Then
                    _cel = cel
                    Invalidate()
                End If
            End If
            Exit Sub
        End If
        Dim pod As Integer = MiejscePod(e.Location)
        Dim krzyzyk As Integer = If(pod >= 0 AndAlso ProstokatKrzyzyka(ProstokatMiejsca(pod)).Contains(e.Location), pod, -1)
        Dim nadUchwytem As Boolean = krzyzyk < 0 AndAlso _zestaw.Liczba > 1 AndAlso Enabled AndAlso pod >= 0 AndAlso
                                     e.X <= ProstokatMiejsca(pod).X + SzerokoscUchwytu + Skaluj(4)
        Dim nowyKursor = If(krzyzyk >= 0, Cursors.Hand, If(nadUchwytem, Cursors.SizeNS, Cursors.Default))
        If Cursor IsNot nowyKursor Then Cursor = nowyKursor
        If krzyzyk <> _krzyzykPod Then
            _krzyzykPod = krzyzyk
            Invalidate()
        End If
        ObsluzRuchPodpowiedzi(pod)
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
        If Cursor IsNot Cursors.Default Then Cursor = Cursors.Default
        If _krzyzykPod >= 0 Then
            _krzyzykPod = -1
            Invalidate()
        End If
        UkryjPodpowiedz()
        _oczekujaceMiejsce = -2
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

    ''' <summary>
    ''' Obsługuje ruch myszy nad listą: ukrywa dotychczasowy dymek przy zmianie wiersza,
    ''' a nowy dymek włącza dopiero po 500 ms (pół sekundy) od zatrzymania kursora na danym elemencie.
    ''' </summary>
    Private Sub ObsluzRuchPodpowiedzi(miejsce As Integer)
        If Podpowiedz Is Nothing Then Exit Sub

        If miejsce <> _oczekujaceMiejsce Then
            'Kursor przeszedł na inne miejsce lub wiersz - natychmiast ukrywamy stary dymek,
            'żeby nie migał i nie przeskakiwał chaotycznie po mijanych wierszach.
            UkryjPodpowiedz()
            _oczekujaceMiejsce = miejsce

            Dim tekst As String = TekstPodpowiedzi(miejsce)
            If Not String.IsNullOrEmpty(tekst) Then
                _timerPodpowiedzi.Start()
            End If
        ElseIf _pokazaneMiejsce <> miejsce AndAlso _timerPodpowiedzi.Enabled Then
            'Kursor porusza się w obrębie tego samego wiersza przed pokazaniem dymka -
            'restartujemy timer, aby dymek pojawił się dopiero po pół sekundy od ZATRZYMANIA kursora.
            _timerPodpowiedzi.Stop()
            _timerPodpowiedzi.Start()
        End If
    End Sub

    Private Sub TimerPodpowiedzi_Tick(sender As Object, e As EventArgs) Handles _timerPodpowiedzi.Tick
        _timerPodpowiedzi.Stop()
        If Podpowiedz Is Nothing OrElse Not Enabled OrElse Not Visible Then Exit Sub

        Dim ptClient As Point = PointToClient(Cursor.Position)
        Dim aktualneMiejsce As Integer = MiejscePod(ptClient)
        If aktualneMiejsce <> _oczekujaceMiejsce Then Exit Sub

        Dim tekst As String = TekstPodpowiedzi(_oczekujaceMiejsce)
        If String.IsNullOrEmpty(tekst) Then Exit Sub

        _pokazaneMiejsce = _oczekujaceMiejsce
        Dim czas As Integer = If(Podpowiedz.AutoPopDelay > 0, Podpowiedz.AutoPopDelay, 15000)

        'Wyświetlamy dymek pod wierszem warstwy (lub pod kursorem na pustym tle)
        Dim r As Rectangle = If(_oczekujaceMiejsce >= 0, ProstokatMiejsca(_oczekujaceMiejsce), ClientRectangle)
        Dim x As Integer = Math.Max(4, Math.Min(ClientSize.Width - 20, ptClient.X))
        Dim y As Integer = If(_oczekujaceMiejsce >= 0, r.Bottom + 1, Math.Min(ClientSize.Height, ptClient.Y + 18))

        Podpowiedz.Show(tekst, Me, x, y, czas)
    End Sub

    Private Sub UkryjPodpowiedz()
        _timerPodpowiedzi.Stop()
        _pokazaneMiejsce = -2
        If Podpowiedz IsNot Nothing Then
            Podpowiedz.Hide(Me)
            Podpowiedz.SetToolTip(Me, "")
        End If
    End Sub

    Private Function TekstPodpowiedzi(miejsce As Integer) As String
        If miejsce >= 0 AndAlso miejsce < _zestaw.Liczba Then
            Dim w = _zestaw(miejsce)
            Dim nazwa As String = NazwaWarstwy(w)
            Return nazwa & If(miejsce = 0, " - podkład (na spodzie)", If(miejsce = _zestaw.Liczba - 1, " - na wierzchu", "")) &
                   If(w.TrybMieszania <> TrybMieszania.Zwykly, ", tryb: " & TrybMieszaniaRozszerzenia.NazwaWyswietlana(w.TrybMieszania), "") &
                   If(w.Krycie < ZestawWarstw.PelneKrycie, ", krycie " & w.Krycie & " %", "") & vbCrLf &
                   "Warstwa " & w.Nazwa & ", zbiór map " & If(w.Zbior <> "", w.Zbior, "?") & " (" & KafleXyz.NazwaTypu(w.EfektywnyTyp) & ")" &
                   If(w.ZalecanyPiksel > 0, ", zalecany piksel " & w.ZalecanyPiksel.ToString("0.###") & " m", "") & vbCrLf &
                   "Przeciągnij lub użyj Ctrl+↑/↓, aby zmienić kolejność; ✕ usuwa warstwę; prawy przycisk - menu warstwy."
        ElseIf Not String.IsNullOrEmpty(OpisListy) Then
            Return OpisListy
        End If
        Return ""
    End Function

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Up, Keys.Down, Keys.Delete, Keys.Home, Keys.End
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        UkryjPodpowiedz()
        _oczekujaceMiejsce = -2
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
