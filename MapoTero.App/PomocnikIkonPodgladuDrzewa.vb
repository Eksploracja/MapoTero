'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Dodaje do dowolnego <see cref="TreeView"/> dwie subtelne ikonki na końcu wiersza pod kursorem
''' (podgląd próbki mapy oraz mapa zasięgu z <c>warstwy.json</c>). Nie włącza <c>OwnerDrawText</c>,
''' dzięki czemu pola wyboru i wygląd gałęzi pozostają w 100% natywne.
''' </summary>
Public Class PomocnikIkonPodgladuDrzewa
    Implements IDisposable

    Private Const TV_FIRST As Integer = &H1100
    Private Const TVM_GETTOOLTIPS As Integer = TV_FIRST + 25
    Private Const WM_USER As Integer = &H400
    Private Const TTM_POP As Integer = WM_USER + 28
    Private Const WM_PAINT As Integer = &HF
    Private Const WM_HSCROLL As Integer = &H114
    Private Const WM_VSCROLL As Integer = &H115
    Private Const WM_MOUSEWHEEL As Integer = &H20A

    <DllImport("user32.dll", EntryPoint:="SendMessageW")>
    Private Shared Function SendMessageProsty(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Private NotInheritable Class OknoDrzewaPodgladu
        Inherits NativeWindow

        Private ReadOnly _wlasciciel As PomocnikIkonPodgladuDrzewa

        Public Sub New(wlasciciel As PomocnikIkonPodgladuDrzewa)
            _wlasciciel = wlasciciel
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            MyBase.WndProc(m)
            If m.Msg = WM_PAINT Then
                _wlasciciel.RysujIkonkiPoMalowaniu(m.WParam)
            ElseIf m.Msg = WM_VSCROLL OrElse m.Msg = WM_HSCROLL OrElse m.Msg = WM_MOUSEWHEEL Then
                _wlasciciel.OdswiezPoPrzewinieciu()
            End If
        End Sub
    End Class

    Private ReadOnly _drzewo As TreeView
    Private ReadOnly _magazyn As MagazynPodgladuGalerii
    Private ReadOnly _pobierzKlucz As Func(Of TreeNode, ValueTuple(Of String, String))
    Private ReadOnly _oknoDrzewa As OknoDrzewaPodgladu
    Private ReadOnly _timerPokazu As New System.Windows.Forms.Timer() With {.Interval = 260}
    Private ReadOnly _timerUkrycia As New System.Windows.Forms.Timer() With {.Interval = 180}

    Private _oknoPodgladu As FormPodgladWarstwy
    Private _wezelPodKursorem As TreeNode
    Private _ikonaPodKursorem As TrybPodgladuWarstwy?
    Private _aktywnaIkona As TrybPodgladuWarstwy?
    Private _aktywnyWezel As TreeNode
    Private _anulowaniePobierania As CancellationTokenSource
    Private _zadanieId As Integer
    Private _poprzedniKursor As Cursor

    Public Sub New(drzewo As TreeView,
                   magazyn As MagazynPodgladuGalerii,
                   pobierzKlucz As Func(Of TreeNode, ValueTuple(Of String, String)))
        _drzewo = drzewo
        _magazyn = magazyn
        _pobierzKlucz = pobierzKlucz
        _oknoDrzewa = New OknoDrzewaPodgladu(Me)

        If _drzewo.IsHandleCreated Then _oknoDrzewa.AssignHandle(_drzewo.Handle)
        AddHandler _drzewo.HandleCreated, AddressOf Drzewo_HandleCreated
        AddHandler _drzewo.HandleDestroyed, AddressOf Drzewo_HandleDestroyed
        AddHandler _drzewo.MouseMove, AddressOf Drzewo_MouseMove
        AddHandler _drzewo.MouseLeave, AddressOf Drzewo_MouseLeave
        AddHandler _drzewo.MouseDown, AddressOf Drzewo_MouseDown
        AddHandler _drzewo.AfterCollapse, AddressOf Drzewo_UkryjZdarzenie
        AddHandler _drzewo.VisibleChanged, AddressOf Drzewo_UkryjZdarzenie
        AddHandler _drzewo.EnabledChanged, AddressOf Drzewo_UkryjZdarzenie
        AddHandler _drzewo.Disposed, AddressOf Drzewo_Disposed
        AddHandler _timerPokazu.Tick, AddressOf TimerPokazu_Tick
        AddHandler _timerUkrycia.Tick, AddressOf TimerUkrycia_Tick
        AddHandler _magazyn.KatalogZaladowany, AddressOf Magazyn_KatalogZaladowany

        'Wczytanie katalogu w tle (jeśli nie ma jeszcze świeżego cache na dysku)
        Task.Run(Async Function()
                     Try
                         Await _magazyn.ZapewnijKatalogAsync().ConfigureAwait(False)
                     Catch
                     End Try
                 End Function)
    End Sub

    Private Sub Drzewo_HandleCreated(sender As Object, e As EventArgs)
        If _oknoDrzewa.Handle = IntPtr.Zero AndAlso _drzewo.IsHandleCreated Then
            _oknoDrzewa.AssignHandle(_drzewo.Handle)
        End If
    End Sub

    Private Sub Drzewo_HandleDestroyed(sender As Object, e As EventArgs)
        _oknoDrzewa.ReleaseHandle()
    End Sub

    Private Sub Magazyn_KatalogZaladowany(sender As Object, e As EventArgs)
        If _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
        Try
            _drzewo.BeginInvoke(Sub() If Not _drzewo.IsDisposed Then _drzewo.Invalidate())
        Catch
        End Try
    End Sub

    Private Function Skaluj(wartosc As Integer) As Integer
        Return CInt(Math.Round(wartosc * (_drzewo.DeviceDpi / 96.0)))
    End Function

    Private Function PozycjaGaleriiWezla(wezel As TreeNode) As PozycjaGaleriiWarstw
        If wezel Is Nothing OrElse _magazyn Is Nothing Then Return Nothing
        Dim klucz = _pobierzKlucz(wezel)
        If String.IsNullOrWhiteSpace(klucz.Item1) OrElse String.IsNullOrWhiteSpace(klucz.Item2) Then Return Nothing
        Return _magazyn.ZnajdzWarstwe(klucz.Item1, klucz.Item2)
    End Function

    ''' <summary>Zwraca prostokąty dwóch ikonek dla zadanego wiersza (współrzędne klienta TreeView).</summary>
    Private Function ProstokatyIkon(wezel As TreeNode, ByRef rectProbka As Rectangle, ByRef rectZasieg As Rectangle) As Boolean
        If wezel Is Nothing OrElse wezel.TreeView IsNot _drzewo OrElse Not wezel.IsVisible Then Return False
        Dim b = wezel.Bounds
        If b.Height <= 0 Then Return False

        Dim rozm = Skaluj(16)
        Dim odstep = Skaluj(4)
        Dim marginesPrawy = Skaluj(4)
        Dim prawa = _drzewo.ClientSize.Width - marginesPrawy
        Dim y = b.Top + Math.Max(0, (b.Height - rozm) \ 2)

        rectZasieg = New Rectangle(prawa - rozm, y, rozm, rozm)
        rectProbka = New Rectangle(rectZasieg.Left - odstep - rozm, y, rozm, rozm)
        Return rectProbka.Left > b.Left + Skaluj(12)
    End Function

    ''' <summary>Czy podany punkt myszy (we współrzędnych drzewa) trafia w jedną z dwóch ikonek podglądu.</summary>
    Public Function CzyPunktNaIkonie(punkt As Point) As Boolean
        Return IkonaWPunkcie(punkt).HasValue
    End Function

    Private Function IkonaWPunkcie(punkt As Point) As TrybPodgladuWarstwy?
        Dim hit = _drzewo.HitTest(punkt)
        Dim wezel = hit.Node
        If wezel Is Nothing Then Return Nothing
        If punkt.Y < wezel.Bounds.Top OrElse punkt.Y >= wezel.Bounds.Bottom Then Return Nothing
        If PozycjaGaleriiWezla(wezel) Is Nothing Then Return Nothing

        Dim rProbka As Rectangle, rZasieg As Rectangle
        If Not ProstokatyIkon(wezel, rProbka, rZasieg) Then Return Nothing
        Dim rProbkaHit = Rectangle.Inflate(rProbka, 2, 2)
        Dim rZasiegHit = Rectangle.Inflate(rZasieg, 2, 2)
        If rProbkaHit.Contains(punkt) Then Return TrybPodgladuWarstwy.Probka
        If rZasiegHit.Contains(punkt) Then Return TrybPodgladuWarstwy.Zasieg
        Return Nothing
    End Function

    Private Sub InwalidujStrefeIkon(wezel As TreeNode)
        If wezel Is Nothing OrElse wezel.TreeView IsNot _drzewo OrElse _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
        If Not wezel.IsVisible Then Exit Sub
        Dim b = wezel.Bounds
        If b.Height <= 0 Then Exit Sub
        Dim szer = Math.Min(_drzewo.ClientSize.Width, Skaluj(54))
        _drzewo.Invalidate(New Rectangle(Math.Max(0, _drzewo.ClientSize.Width - szer), b.Top, szer, b.Height))
    End Sub

    Private Sub RysujIkonkiPoMalowaniu(hdcParam As IntPtr)
        If _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
        If _wezelPodKursorem Is Nothing OrElse _wezelPodKursorem.TreeView IsNot _drzewo OrElse Not _wezelPodKursorem.IsVisible Then Exit Sub
        If PozycjaGaleriiWezla(_wezelPodKursorem) Is Nothing Then Exit Sub

        Dim rProbka As Rectangle, rZasieg As Rectangle
        If Not ProstokatyIkon(_wezelPodKursorem, rProbka, rZasieg) Then Exit Sub

        Dim zanik = Skaluj(8)
        Dim lewaStrefy = Math.Max(0, rProbka.Left - zanik)
        Dim szerStrefy = _drzewo.ClientSize.Width - lewaStrefy
        Dim wysStrefy = _wezelPodKursorem.Bounds.Height
        Dim goraStrefy = _wezelPodKursorem.Bounds.Top
        If szerStrefy <= 0 OrElse wysStrefy <= 0 Then Exit Sub

        Dim tlo = If(_drzewo.BackColor.IsEmpty, SystemColors.Window, _drzewo.BackColor)
        Using bmp As New Bitmap(szerStrefy, wysStrefy, Imaging.PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bmp)
                g.SmoothingMode = SmoothingMode.AntiAlias

                'Płynne przykrycie ewentualnego długiego tekstu pod ikonkami (bez ucinania go w połowie litery)
                Dim szerZaniku = Math.Min(zanik, szerStrefy)
                If szerZaniku > 0 Then
                    Using pGrad As New LinearGradientBrush(New Point(0, 0), New Point(szerZaniku, 0), Color.FromArgb(0, tlo), Color.FromArgb(255, tlo))
                        g.FillRectangle(pGrad, 0, 0, szerZaniku, wysStrefy)
                    End Using
                End If
                If szerStrefy > szerZaniku Then
                    Using pTlo As New SolidBrush(tlo)
                        g.FillRectangle(pTlo, szerZaniku, 0, szerStrefy - szerZaniku, wysStrefy)
                    End Using
                End If

                Dim rP As New Rectangle(rProbka.X - lewaStrefy, rProbka.Y - goraStrefy, rProbka.Width, rProbka.Height)
                Dim rZ As New Rectangle(rZasieg.X - lewaStrefy, rZasieg.Y - goraStrefy, rZasieg.Width, rZasieg.Height)
                RysujIkoneProbki(g, rP, _ikonaPodKursorem.HasValue AndAlso _ikonaPodKursorem.Value = TrybPodgladuWarstwy.Probka)
                RysujIkoneZasiegu(g, rZ, _ikonaPodKursorem.HasValue AndAlso _ikonaPodKursorem.Value = TrybPodgladuWarstwy.Zasieg)
            End Using

            If hdcParam <> IntPtr.Zero Then
                Using gDoc = Graphics.FromHdc(hdcParam)
                    gDoc.DrawImageUnscaled(bmp, lewaStrefy, goraStrefy)
                End Using
            Else
                Using gDoc = Graphics.FromHwnd(_drzewo.Handle)
                    gDoc.DrawImageUnscaled(bmp, lewaStrefy, goraStrefy)
                End Using
            End If
        End Using
    End Sub

    Private Sub OdswiezPoPrzewinieciu()
        If _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
        Dim pozycja = _drzewo.PointToClient(Control.MousePosition)
        If _drzewo.ClientRectangle.Contains(pozycja) Then
            ZaktualizujHover(pozycja)
        ElseIf _wezelPodKursorem IsNot Nothing Then
            Dim stary = _wezelPodKursorem
            _wezelPodKursorem = Nothing
            _ikonaPodKursorem = Nothing
            InwalidujStrefeIkon(stary)
            UkryjPodglad()
        End If
    End Sub

    Private Shared Sub RysujIkoneProbki(g As Graphics, r As Rectangle, aktywna As Boolean)
        Dim kolorTla = If(aktywna, Color.FromArgb(219, 234, 254), Color.FromArgb(241, 245, 249))
        Dim kolorRamki = If(aktywna, Color.FromArgb(37, 99, 235), Color.FromArgb(148, 163, 184))
        Dim kolorGory = If(aktywna, Color.FromArgb(30, 64, 175), Color.FromArgb(71, 85, 105))

        Using pTlo As New SolidBrush(kolorTla)
            Using pRamka As New Pen(kolorRamki, 1.0F)
                g.FillRectangle(pTlo, r)
                g.DrawRectangle(pRamka, r.X, r.Y, r.Width - 1, r.Height - 1)
            End Using
        End Using

        'Mini krajobraz / obrazek w ramce
        Dim l = r.X + 3
        Dim b = r.Bottom - 4
        Dim pkt As Point() = {
            New Point(l, b),
            New Point(l + 3, b - 5),
            New Point(l + 6, b - 2),
            New Point(l + 8, b - 6),
            New Point(r.Right - 3, b)
        }
        Using pWyp As New SolidBrush(kolorGory)
            g.FillPolygon(pWyp, pkt)
        End Using
        Using pSlonce As New SolidBrush(If(aktywna, Color.FromArgb(234, 88, 12), Color.FromArgb(217, 119, 6)))
            g.FillEllipse(pSlonce, r.X + 3, r.Y + 3, 3, 3)
        End Using
    End Sub

    Private Shared Sub RysujIkoneZasiegu(g As Graphics, r As Rectangle, aktywna As Boolean)
        Dim kolorTla = If(aktywna, Color.FromArgb(220, 252, 231), Color.FromArgb(241, 245, 249))
        Dim kolorRamki = If(aktywna, Color.FromArgb(22, 163, 74), Color.FromArgb(148, 163, 184))
        Dim kolorKonturu = If(aktywna, Color.FromArgb(21, 128, 61), Color.FromArgb(71, 85, 105))

        Using pTlo As New SolidBrush(kolorTla)
            Using pRamka As New Pen(kolorRamki, 1.0F)
                g.FillRectangle(pTlo, r)
                g.DrawRectangle(pRamka, r.X, r.Y, r.Width - 1, r.Height - 1)
            End Using
        End Using

        'Uproszczony zarys Polski w ikonce
        Dim x0 = r.X + 3.0F
        Dim y0 = r.Y + 3.0F
        Dim s = (r.Width - 6) / 10.0F
        Dim polska As PointF() = {
            New PointF(x0 + 2 * s, y0 + 1 * s),
            New PointF(x0 + 5 * s, y0 + 0 * s),
            New PointF(x0 + 9 * s, y0 + 1 * s),
            New PointF(x0 + 9.5F * s, y0 + 5 * s),
            New PointF(x0 + 8 * s, y0 + 9 * s),
            New PointF(x0 + 4 * s, y0 + 8 * s),
            New PointF(x0 + 1 * s, y0 + 7 * s),
            New PointF(x0 + 0.5F * s, y0 + 3.5F * s)
        }
        Using pWyp As New SolidBrush(Color.FromArgb(165, kolorKonturu))
            Using pRys As New Pen(kolorKonturu, 1.0F)
                g.FillPolygon(pWyp, polska)
                g.DrawPolygon(pRys, polska)
            End Using
        End Using
    End Sub

    Private Sub Drzewo_MouseMove(sender As Object, e As MouseEventArgs)
        ZaktualizujHover(e.Location)
    End Sub

    Private Sub ZaktualizujHover(punkt As Point)
        Dim hit = _drzewo.HitTest(punkt)
        Dim nowyWezel = If(hit.Node IsNot Nothing AndAlso punkt.Y >= hit.Node.Bounds.Top AndAlso punkt.Y < hit.Node.Bounds.Bottom, hit.Node, Nothing)
        Dim maPodglad = nowyWezel IsNot Nothing AndAlso PozycjaGaleriiWezla(nowyWezel) IsNot Nothing
        If Not maPodglad Then nowyWezel = Nothing

        Dim nowaIkona = If(nowyWezel IsNot Nothing, IkonaWPunkcie(punkt), Nothing)

        Dim zmienilSieWezel = Not Object.ReferenceEquals(nowyWezel, _wezelPodKursorem)
        Dim zmienilaSieIkona = (nowaIkona.HasValue <> _ikonaPodKursorem.HasValue) OrElse
                               (nowaIkona.HasValue AndAlso _ikonaPodKursorem.HasValue AndAlso nowaIkona.Value <> _ikonaPodKursorem.Value)

        If zmienilSieWezel OrElse zmienilaSieIkona Then
            Dim stary = _wezelPodKursorem
            _wezelPodKursorem = nowyWezel
            _ikonaPodKursorem = nowaIkona
            If stary IsNot Nothing AndAlso Not Object.ReferenceEquals(stary, nowyWezel) Then InwalidujStrefeIkon(stary)
            If nowyWezel IsNot Nothing Then InwalidujStrefeIkon(nowyWezel)

            If _ikonaPodKursorem.HasValue Then
                If _poprzedniKursor Is Nothing Then _poprzedniKursor = _drzewo.Cursor
                _drzewo.Cursor = Cursors.Hand
                SchowajSystemowyDymek()
            Else
                PrzywrocKursor()
            End If
        End If

        If _wezelPodKursorem IsNot Nothing AndAlso _ikonaPodKursorem.HasValue Then
            _timerUkrycia.Stop()
            If _oknoPodgladu IsNot Nothing AndAlso _oknoPodgladu.Visible Then
                If Not Object.ReferenceEquals(_wezelPodKursorem, _aktywnyWezel) OrElse
                   Not _aktywnaIkona.HasValue OrElse _aktywnaIkona.Value <> _ikonaPodKursorem.Value Then
                    PokazPodgladNatychmiast(_wezelPodKursorem, _ikonaPodKursorem.Value)
                End If
            Else
                _timerPokazu.Stop()
                _timerPokazu.Start()
            End If
        Else
            _timerPokazu.Stop()
            If _oknoPodgladu IsNot Nothing AndAlso _oknoPodgladu.Visible Then
                _timerUkrycia.Stop()
                _timerUkrycia.Start()
            End If
        End If
    End Sub

    Private Sub Drzewo_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Exit Sub
        Dim ikona = IkonaWPunkcie(e.Location)
        If Not ikona.HasValue Then Exit Sub
        Dim hit = _drzewo.HitTest(e.Location)
        If hit.Node Is Nothing Then Exit Sub

        _timerPokazu.Stop()
        _timerUkrycia.Stop()
        _wezelPodKursorem = hit.Node
        _ikonaPodKursorem = ikona
        PokazPodgladNatychmiast(hit.Node, ikona.Value)
    End Sub

    Private Sub Drzewo_MouseLeave(sender As Object, e As EventArgs)
        _timerPokazu.Stop()
        PrzywrocKursor()
        If _wezelPodKursorem IsNot Nothing Then
            Dim stary = _wezelPodKursorem
            _wezelPodKursorem = Nothing
            _ikonaPodKursorem = Nothing
            InwalidujStrefeIkon(stary)
        End If
        If _oknoPodgladu IsNot Nothing AndAlso _oknoPodgladu.Visible Then
            _timerUkrycia.Stop()
            _timerUkrycia.Start()
        End If
    End Sub

    Private Sub Drzewo_UkryjZdarzenie(sender As Object, e As EventArgs)
        UkryjPodglad()
    End Sub

    Private Sub Drzewo_Disposed(sender As Object, e As EventArgs)
        Dispose()
    End Sub

    Private Sub TimerPokazu_Tick(sender As Object, e As EventArgs)
        _timerPokazu.Stop()
        If _wezelPodKursorem IsNot Nothing AndAlso _ikonaPodKursorem.HasValue Then
            PokazPodgladNatychmiast(_wezelPodKursorem, _ikonaPodKursorem.Value)
        End If
    End Sub

    Private Sub TimerUkrycia_Tick(sender As Object, e As EventArgs)
        _timerUkrycia.Stop()
        UkryjPodglad()
    End Sub

    Private Sub PrzywrocKursor()
        If _poprzedniKursor IsNot Nothing AndAlso Not _drzewo.IsDisposed Then
            _drzewo.Cursor = _poprzedniKursor
            _poprzedniKursor = Nothing
        End If
    End Sub

    Private Sub SchowajSystemowyDymek()
        If _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
        Dim hwndTT = SendMessageProsty(_drzewo.Handle, TVM_GETTOOLTIPS, IntPtr.Zero, IntPtr.Zero)
        If hwndTT <> IntPtr.Zero Then
            SendMessageProsty(hwndTT, TTM_POP, IntPtr.Zero, IntPtr.Zero)
        End If
    End Sub

    Private Sub UkryjPodglad()
        _timerPokazu.Stop()
        _timerUkrycia.Stop()
        _anulowaniePobierania?.Cancel()
        _aktywnyWezel = Nothing
        _aktywnaIkona = Nothing
        If _oknoPodgladu IsNot Nothing AndAlso Not _oknoPodgladu.IsDisposed Then
            _oknoPodgladu.Hide()
        End If
    End Sub

    Private Sub PokazPodgladNatychmiast(wezel As TreeNode, tryb As TrybPodgladuWarstwy)
        If wezel Is Nothing OrElse wezel.TreeView IsNot _drzewo OrElse _drzewo.IsDisposed Then Exit Sub
        Dim klucz = _pobierzKlucz(wezel)
        Dim pozycja = _magazyn.ZnajdzWarstwe(klucz.Item1, klucz.Item2)
        If pozycja Is Nothing Then Exit Sub

        _aktywnyWezel = wezel
        _aktywnaIkona = tryb

        If _oknoPodgladu Is Nothing OrElse _oknoPodgladu.IsDisposed Then
            _oknoPodgladu = New FormPodgladWarstwy()
        End If

        Dim b = wezel.Bounds
        Dim rWierszEkran = New Rectangle(_drzewo.PointToScreen(New Point(0, b.Top)), New Size(_drzewo.Width, b.Height))
        Dim wlasciciel = _drzewo.FindForm()
        SchowajSystemowyDymek()

        If tryb = TrybPodgladuWarstwy.Zasieg Then
            _anulowaniePobierania?.Cancel()
            Dim wynik = _magazyn.PobierzObrazZasiegu(klucz.Item1, klucz.Item2)
            _oknoPodgladu.PokazWynik(pozycja, wezel.Text, tryb, wynik)
            _oknoPodgladu.UstawPozycjePrzyWierszu(_drzewo, rWierszEkran, wlasciciel)
            Exit Sub
        End If

        'Tryb próbki: pobieranie asynchroniczne (z natychmiastowym wynikiem jeśli obraz jest już w cache)
        _anulowaniePobierania?.Cancel()
        _anulowaniePobierania?.Dispose()
        _anulowaniePobierania = New CancellationTokenSource()
        Dim cts = _anulowaniePobierania
        _zadanieId += 1
        Dim biezaceId = _zadanieId

        _oknoPodgladu.PokazLadowanie(pozycja, wezel.Text, tryb, "Wczytywanie próbki podglądu…")
        _oknoPodgladu.UstawPozycjePrzyWierszu(_drzewo, rWierszEkran, wlasciciel)

        Dim zadanie = _magazyn.PobierzObrazProbkiAsync(klucz.Item1, klucz.Item2, cts.Token)
        zadanie.ContinueWith(
            Sub(t)
                If _drzewo.IsDisposed OrElse Not _drzewo.IsHandleCreated Then Exit Sub
                Try
                    _drzewo.BeginInvoke(
                        Sub()
                            If _drzewo.IsDisposed OrElse biezaceId <> _zadanieId OrElse cts.IsCancellationRequested Then
                                If t.Status = TaskStatus.RanToCompletion Then t.Result?.Obraz?.Dispose()
                                Exit Sub
                            End If
                            If _oknoPodgladu Is Nothing OrElse _oknoPodgladu.IsDisposed OrElse Not _oknoPodgladu.Visible Then
                                If t.Status = TaskStatus.RanToCompletion Then t.Result?.Obraz?.Dispose()
                                Exit Sub
                            End If
                            If t.Status = TaskStatus.RanToCompletion Then
                                _oknoPodgladu.PokazWynik(pozycja, wezel.Text, TrybPodgladuWarstwy.Probka, t.Result)
                            Else
                                _oknoPodgladu.PokazWynik(pozycja, wezel.Text, TrybPodgladuWarstwy.Probka,
                                                         New WynikPodgladuWarstwy With {.Komunikat = "Nie udało się pobrać próbki podglądu."})
                            End If
                        End Sub)
                Catch
                End Try
            End Sub, TaskScheduler.Default)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _timerPokazu.Dispose()
        _timerUkrycia.Dispose()
        _anulowaniePobierania?.Cancel()
        _anulowaniePobierania?.Dispose()
        _oknoDrzewa.ReleaseHandle()
        If _drzewo IsNot Nothing AndAlso Not _drzewo.IsDisposed Then
            RemoveHandler _drzewo.HandleCreated, AddressOf Drzewo_HandleCreated
            RemoveHandler _drzewo.HandleDestroyed, AddressOf Drzewo_HandleDestroyed
            RemoveHandler _drzewo.MouseMove, AddressOf Drzewo_MouseMove
            RemoveHandler _drzewo.MouseLeave, AddressOf Drzewo_MouseLeave
            RemoveHandler _drzewo.MouseDown, AddressOf Drzewo_MouseDown
            RemoveHandler _drzewo.AfterCollapse, AddressOf Drzewo_UkryjZdarzenie
            RemoveHandler _drzewo.VisibleChanged, AddressOf Drzewo_UkryjZdarzenie
            RemoveHandler _drzewo.EnabledChanged, AddressOf Drzewo_UkryjZdarzenie
            RemoveHandler _drzewo.Disposed, AddressOf Drzewo_Disposed
        End If
        If _magazyn IsNot Nothing Then RemoveHandler _magazyn.KatalogZaladowany, AddressOf Magazyn_KatalogZaladowany
        If _oknoPodgladu IsNot Nothing AndAlso Not _oknoPodgladu.IsDisposed Then
            _oknoPodgladu.Dispose()
        End If
    End Sub
End Class
