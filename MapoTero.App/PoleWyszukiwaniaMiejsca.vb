'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.ComponentModel
Imports System.Threading
Imports MapoTero.Core

''' <summary>Pozycja listy wyników: miejsce z wyszukiwarki albo działka z ULDK (wtedy <see cref="Dzialka"/> ma obrys).</summary>
Friend Class WynikWyszukiwania
    Public Miejsce As WynikGeokodowania
    Public Dzialka As DzialkaKatastralna
End Class

''' <summary>
''' Pole wyszukiwania na pasku okna głównego: miejscowość, adres, działka albo współrzędne. Lupa w polu znika po
''' wpisaniu tekstu (zastępuje ją krzyżyk czyszczący). Wyniki pokazuje lista pod polem - podpowiedzi pojawiają się
''' w trakcie pisania, Enter wybiera zaznaczony wynik, a strzałki zmieniają zaznaczenie. Lista leży na formularzu
''' (nie w ToolStripDropDown), bo rozwijane menu przechwytuje klawiaturę i przerywa pisanie w polu.
''' </summary>
Friend Class PoleWyszukiwaniaMiejsca
    Inherits Panel

    Private Shared ReadOnly CzyWindows11 As Boolean = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)

    Private ReadOnly _pole As New TextBox With {.BorderStyle = BorderStyle.None, .AutoSize = False, .Name = "txtSzukajMiejsca"}
    Private ReadOnly _ikona As New Label With {.Cursor = Cursors.Hand, .TextAlign = ContentAlignment.MiddleCenter,
                                               .Dock = DockStyle.Right, .AccessibleRole = AccessibleRole.PushButton}
    Private ReadOnly _lista As New ListBox With {.Name = "lstWynikiSzukania", .DrawMode = DrawMode.OwnerDrawFixed,
                                                 .Visible = False, .IntegralHeight = False, .BorderStyle = BorderStyle.FixedSingle}
    Private ReadOnly _stan As New Label With {.Name = "lblStanSzukania", .Visible = False, .AutoSize = False,
                                              .BorderStyle = BorderStyle.FixedSingle, .BackColor = Color.White,
                                              .TextAlign = ContentAlignment.MiddleLeft, .Padding = New Padding(6, 0, 6, 0)}
    Private ReadOnly _zegar As New System.Windows.Forms.Timer With {.Interval = 400}
    Private ReadOnly _lupa As Bitmap
    Private ReadOnly _podpowiedz As Label
    Private _cts As CancellationTokenSource
    Private _wyszukanaFraza As String = ""

    ''' <summary>Środek mapy - wyszukiwarka miejsc stawia bliższe wyniki wyżej.</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SrodekMapy As Func(Of (Lat As Double, Lng As Double))

    ''' <summary>Wybrany wynik (miejsce, punkt ze współrzędnych albo działka).</summary>
    Public Event Wybrano(wynik As WynikWyszukiwania)

    Public Sub New(lupa As Bitmap, czcionka As Font, czcionkaPola As Font)
        _lupa = lupa
        BackColor = Color.White
        Font = czcionka
        Padding = New Padding(6, 1, 2, 1)
        _pole.Font = czcionkaPola
        _pole.AccessibleName = "Szukaj miejsca lub działki"
        ' Wyśrodkowanie w OnLayout ustawia tekst na wysokości napisów przycisków paska.
        _pole.Dock = DockStyle.None
        _ikona.Width = 24
        _lista.Font = czcionka
        _lista.ItemHeight = If(CzyWindows11, 2 * (czcionka.Height + 4), CInt(czcionka.Height * 2.4))
        _stan.Font = czcionka
        _stan.UseCompatibleTextRendering = CzyWindows11
        _stan.ForeColor = Color.FromArgb(75, 94, 113)
        Controls.Add(_pole)
        Controls.Add(_ikona)
        _podpowiedz = DodajPodpowiedzPola(_pole, "Szukaj miejsca lub działki…")
        OdswiezIkone()

        AddHandler _pole.GotFocus, Sub() Invalidate()
        AddHandler _pole.LostFocus, Sub() Invalidate()
        AddHandler _pole.TextChanged, AddressOf PoZmianieTekstu
        AddHandler _pole.KeyDown, AddressOf PoleKeyDown
        AddHandler _pole.Leave, Sub()
                                    If IsHandleCreated AndAlso Not IsDisposed Then
                                        BeginInvoke(Sub() If Not IsDisposed AndAlso Not _lista.Focused Then ZamknijListe())
                                    End If
                                End Sub
        AddHandler _ikona.Click, Sub()
                                     If _pole.TextLength > 0 Then _pole.Clear()
                                     _pole.Focus()
                                 End Sub
        AddHandler Click, Sub() _pole.Focus()
        AddHandler _zegar.Tick, Sub()
                                    _zegar.Stop()
                                    Szukaj(automatyczne:=True)
                                End Sub
        AddHandler _lista.DrawItem, AddressOf RysujWynik
        AddHandler _lista.MouseClick, Sub(s, e)
                                          Dim i = _lista.IndexFromPoint(e.Location)
                                          If i >= 0 Then WybierzWynik(i)
                                      End Sub
        AddHandler _lista.KeyDown, Sub(s, e)
                                       If e.KeyCode = Keys.Enter Then WybierzWynik(_lista.SelectedIndex) : e.SuppressKeyPress = True
                                       If e.KeyCode = Keys.Escape Then ZamknijListe() : _pole.Focus()
                                   End Sub
        AddHandler _lista.Leave, Sub()
                                     If IsHandleCreated AndAlso Not IsDisposed Then
                                         BeginInvoke(Sub() If Not IsDisposed AndAlso Not _pole.Focused Then ZamknijListe())
                                     End If
                                 End Sub
    End Sub

    ''' <summary>Pole tekstowe (dostęp dla skrótu Ctrl+F i testów).</summary>
    Friend ReadOnly Property Pole As TextBox
        Get
            Return _pole
        End Get
    End Property

    Friend Sub UstawDymekPola(dymki As ToolTip, tekst As String)
        dymki.SetToolTip(_pole, tekst)
        If _podpowiedz IsNot Nothing Then dymki.SetToolTip(_podpowiedz, tekst)
    End Sub

    ''' <summary>Lista wyników (testy).</summary>
    Friend ReadOnly Property Lista As ListBox
        Get
            Return _lista
        End Get
    End Property

    Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
        MyBase.OnLayout(levent)
        Dim lewa = Padding.Left
        Dim prawa = If(_ikona.Visible, _ikona.Left, ClientSize.Width - Padding.Right)
        UlozPoleTekstowe(_pole, lewa, prawa, ClientSize.Height, _podpowiedz)
    End Sub

    Friend Shared Sub UlozPoleTekstowe(pole As TextBox, lewa As Integer, prawa As Integer, wysokoscPanelu As Integer,
                                     podpowiedz As Label)
        ' Dla pola bez ramki WinForms sprowadza PreferredHeight do wysokości czcionki. Natywne pole
        ' potrzebuje dodatkowego miejsca na tekst i podpowiedź, żeby przy większym DPI nie obcinać liter.
        Dim zapas = CInt(Math.Ceiling(3 * pole.DeviceDpi / 96.0))
        Dim wysokosc = Math.Min(wysokoscPanelu, pole.PreferredHeight + zapas)
        Dim gora = (wysokoscPanelu - wysokosc) \ 2
        Dim szerokosc = Math.Max(10, prawa - lewa - 2)
        If podpowiedz IsNot Nothing Then
            podpowiedz.SetBounds(lewa, 1, szerokosc, Math.Max(0, wysokoscPanelu - 2))
        End If
        pole.SetBounds(lewa, gora, szerokosc, wysokosc)
    End Sub

    Friend Shared Function DodajPodpowiedzPola(pole As TextBox, tekst As String) As Label
        If Not CzyWindows11 Then
            pole.PlaceholderText = tekst
            Return Nothing
        End If

        ' WinForms rysuje PlaceholderText wewnątrz natywnego pola. Na Windows 11 dolna część znaków była
        ' obcinana mimo miejsca w ramce. Etykieta ma do dyspozycji całą wysokość wnętrza ramki.
        pole.PlaceholderText = ""
        Dim etykieta As New Label With {
            .Name = pole.Name & "Podpowiedz", .Text = tekst, .Font = pole.Font, .BackColor = pole.BackColor,
            .ForeColor = SystemColors.GrayText, .TextAlign = ContentAlignment.MiddleLeft, .AutoEllipsis = True,
            .UseCompatibleTextRendering = True, .UseMnemonic = False, .Cursor = Cursors.IBeam
        }
        Dim odswiez As Action = Sub() etykieta.Visible = pole.TextLength = 0 AndAlso Not pole.Focused
        AddHandler pole.TextChanged, Sub() odswiez()
        AddHandler pole.GotFocus, Sub() odswiez()
        AddHandler pole.LostFocus, Sub() odswiez()
        AddHandler pole.BackColorChanged, Sub() etykieta.BackColor = pole.BackColor
        AddHandler etykieta.Click, Sub() pole.Focus()
        pole.Parent.Controls.Add(etykieta)
        etykieta.BringToFront()
        odswiez()
        Return etykieta
    End Function

    Friend Shared Function RozmiarCzcionkiPola(dpi As Integer, rozmiarKataloguPkt As Single, windows11 As Boolean) As Single
        ' Na Windows 11 tekst w obu polach był za duży już przy skali 100%.
        If windows11 Then Return 8.0F * dpi / 72.0F
        Return Math.Max(rozmiarKataloguPkt * 96.0F, 8.25F * dpi) / 72.0F
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Using pioro As New Pen(If(_pole.Focused, Color.FromArgb(25, 78, 128), Color.FromArgb(160, 172, 186)))
            e.Graphics.DrawRectangle(pioro, 0, 0, Width - 1, Height - 1)
        End Using
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _cts?.Cancel()
            _cts?.Dispose()
            _cts = Nothing
            _zegar.Dispose()
            _lista.Dispose()
            _stan.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub OdswiezIkone()
        If _pole.TextLength = 0 Then
            _ikona.Image = _lupa
            _ikona.Text = ""
        Else
            _ikona.Image = Nothing
            _ikona.Text = "✕"
            _ikona.ForeColor = Color.FromArgb(75, 94, 113)
        End If
        Invalidate()
    End Sub

    Private Sub PoZmianieTekstu(sender As Object, e As EventArgs)
        OdswiezIkone()
        _zegar.Stop()
        Dim fraza = _pole.Text.Trim()
        If fraza.Length = 0 Then
            _cts?.Cancel()
            ZamknijListe()
        ElseIf fraza.Length < 3 Then
            _cts?.Cancel()
        Else
            _zegar.Start()
        End If
    End Sub

    Private Sub PoleKeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Enter
                e.SuppressKeyPress = True
                _zegar.Stop()
                If _lista.Visible AndAlso _lista.Items.Count > 0 AndAlso _wyszukanaFraza = _pole.Text.Trim() Then
                    WybierzWynik(Math.Max(0, _lista.SelectedIndex))
                Else
                    Szukaj(automatyczne:=False)
                End If
            Case Keys.Down, Keys.Up
                If _lista.Visible AndAlso _lista.Items.Count > 0 Then
                    e.Handled = True
                    Dim krok = If(e.KeyCode = Keys.Down, 1, -1)
                    _lista.SelectedIndex = Math.Clamp(_lista.SelectedIndex + krok, 0, _lista.Items.Count - 1)
                End If
            Case Keys.Escape
                e.SuppressKeyPress = True
                If _lista.Visible OrElse _stan.Visible Then ZamknijListe() Else _pole.Clear()
        End Select
    End Sub

    ''' <summary>Wyszukuje frazę z pola: współrzędne bez serwera, działka w ULDK, reszta w wyszukiwarce miejsc.</summary>
    Friend Async Sub Szukaj(automatyczne As Boolean)
        Dim fraza = _pole.Text.Trim()
        If fraza = "" Then Return
        _cts?.Cancel()
        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim token = _cts.Token
        _wyszukanaFraza = fraza
        Dim zapytanie = ObszarKreatora.RozpoznajZapytanie(fraza)
        If zapytanie.Punkt IsNot Nothing Then
            ' Wklejone współrzędne: Enter od razu przesuwa mapę, w trakcie pisania - jedna podpowiedź
            Dim punkt As New WynikWyszukiwania With {.Miejsce = zapytanie.Punkt}
            If automatyczne Then PokazWyniki({punkt}.ToList()) Else ZamknijListe() : RaiseEvent Wybrano(punkt)
            Return
        End If
        ' Działkę szukamy dopiero po Enter - ULDK jest wolniejsze, a niepełny numer daje błąd zamiast podpowiedzi
        If automatyczne AndAlso zapytanie.Rodzaj = RodzajZapytania.Dzialka Then Return
        PokazStan(If(zapytanie.Rodzaj = RodzajZapytania.Dzialka, "Szukanie działki w ULDK…", "Szukanie…"))
        Try
            Dim wyniki As New List(Of WynikWyszukiwania)
            If zapytanie.Rodzaj = RodzajZapytania.Dzialka Then
                For Each d In Await KlientUldk.SzukajDzialekAsync(zapytanie.Tekst, token)
                    wyniki.Add(New WynikWyszukiwania With {.Miejsce = WynikDzialki.Z(d), .Dzialka = d})
                Next
            Else
                Dim srodek = SrodekMapy?.Invoke()
                For Each m In Await KlientGeokodowania.SzukajMiejscaAsync(fraza, token, srodek?.Lat, srodek?.Lng)
                    wyniki.Add(New WynikWyszukiwania With {.Miejsce = m})
                Next
            End If
            If token.IsCancellationRequested OrElse IsDisposed Then Return
            Dim fOkno = FindForm()
            If fOkno IsNot Nothing AndAlso fOkno.ContainsFocus AndAlso Not _pole.Focused AndAlso Not _lista.Focused Then Return
            If wyniki.Count = 0 Then
                PokazStan(If(zapytanie.Rodzaj = RodzajZapytania.Dzialka, "Usługa ULDK nie znalazła takiej działki.",
                             "Nic nie znaleziono. Spróbuj innej nazwy lub dopisz gminę."))
            ElseIf Not automatyczne AndAlso wyniki.Count = 1 Then
                ZamknijListe()
                RaiseEvent Wybrano(wyniki(0))
            Else
                PokazWyniki(wyniki)
            End If
        Catch ex As OperationCanceledException
        Catch ex As ObjectDisposedException
        Catch ex As Exception
            If Not IsDisposed AndAlso Not token.IsCancellationRequested Then PokazStan("Błąd wyszukiwania: " & OpisBledu.Z(ex))
        End Try
    End Sub

    Private Sub WybierzWynik(indeks As Integer)
        If indeks < 0 OrElse indeks >= _lista.Items.Count Then Return
        Dim wynik = DirectCast(_lista.Items(indeks), WynikWyszukiwania)
        ZamknijListe()
        RaiseEvent Wybrano(wynik)
    End Sub

    ''' <summary>Lista i komunikat leżą na formularzu nad mapą, pod polem i co najmniej tak szerokie jak 420 px.</summary>
    Private Sub UmiescNaFormularzu(kontrolka As Control, wysokosc As Integer)
        If IsDisposed OrElse Disposing Then Return
        Dim f = FindForm()
        If f Is Nothing OrElse f.IsDisposed Then Return
        If kontrolka.Parent IsNot f Then f.Controls.Add(kontrolka)
        Dim lewyGorny = f.PointToClient(PointToScreen(New Point(0, Height)))
        Dim szerokosc = Math.Max(Width, CInt(420 * DeviceDpi / 96.0))
        szerokosc = Math.Min(szerokosc, f.ClientSize.Width - lewyGorny.X - 4)
        kontrolka.Bounds = New Rectangle(lewyGorny.X, lewyGorny.Y + 1, szerokosc, wysokosc)
        kontrolka.Visible = True
        kontrolka.BringToFront()
    End Sub

    Private Sub PokazWyniki(wyniki As List(Of WynikWyszukiwania))
        _stan.Visible = False
        _lista.BeginUpdate()
        _lista.Items.Clear()
        For Each w In wyniki
            _lista.Items.Add(w)
        Next
        _lista.EndUpdate()
        _lista.SelectedIndex = 0
        UmiescNaFormularzu(_lista, _lista.ItemHeight * Math.Min(wyniki.Count, 8) + 2)
    End Sub

    Private Sub PokazStan(tekst As String)
        _lista.Visible = False
        _stan.Text = tekst
        UmiescNaFormularzu(_stan, CInt(_stan.Font.Height * 2))
    End Sub

    Friend Sub ZamknijListe()
        _lista.Visible = False
        _stan.Visible = False
        Invalidate()
    End Sub

    Private Sub RysujWynik(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim w = DirectCast(_lista.Items(e.Index), WynikWyszukiwania)
        Dim wybrany = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Using tlo As New SolidBrush(If(wybrany, Color.FromArgb(225, 238, 255), Color.White))
            e.Graphics.FillRectangle(tlo, e.Bounds)
        End Using
        Dim polowa = e.Bounds.Height \ 2
        Dim naglowek = w.Miejsce.KrotkaNazwa & If(w.Miejsce.TypOpis <> "", "  ·  " & w.Miejsce.TypOpis, "")
        Dim opis = If(w.Dzialka IsNot Nothing, w.Miejsce.PelnaNazwa, ObszarKreatora.SformatujLokalizacje(w.Miejsce))
        If opis = "" Then opis = w.Miejsce.WspolrzedneTekst
        Dim kolorNaglowka = Color.FromArgb(30, 45, 61)
        Dim kolorOpisu = Color.FromArgb(75, 94, 113)
        Dim lewa = e.Bounds.X + 6
        Dim szerokosc = e.Bounds.Width - 10
        If szerokosc <= 0 Then Return

        Using pogrubiona As New Font(_lista.Font, FontStyle.Bold)
            If CzyWindows11 Then
                ' Rysowanie w GDI+ odpowiada metrykom czcionki, z których obliczono wysokość pozycji.
                ' TextRenderer z tą czcionką rysował większe litery i przycinał dolną linię na Windows 11.
                Using format As New StringFormat With {.LineAlignment = StringAlignment.Center,
                                                       .FormatFlags = StringFormatFlags.NoWrap,
                                                       .Trimming = StringTrimming.EllipsisCharacter},
                      pedzelNaglowka As New SolidBrush(kolorNaglowka), pedzelOpisu As New SolidBrush(kolorOpisu)
                    e.Graphics.DrawString(naglowek, pogrubiona, pedzelNaglowka, New RectangleF(lewa, e.Bounds.Y, szerokosc, polowa), format)
                    e.Graphics.DrawString(opis, _lista.Font, pedzelOpisu, New RectangleF(lewa, e.Bounds.Y + polowa, szerokosc, polowa), format)
                End Using
                Return
            End If

            TextRenderer.DrawText(e.Graphics, naglowek, pogrubiona, New Rectangle(lewa, e.Bounds.Y + 2, szerokosc, polowa),
                                  kolorNaglowka, TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter)
        End Using
        TextRenderer.DrawText(e.Graphics, opis, _lista.Font, New Rectangle(lewa, e.Bounds.Y + polowa, szerokosc, polowa - 2),
                              kolorOpisu, TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter)
    End Sub
End Class
