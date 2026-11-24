'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>Rodzaj podglądu pokazywany w oknie dymku przy warstwie.</summary>
Public Enum TrybPodgladuWarstwy
    Probka = 0
    Zasieg = 1
End Enum

''' <summary>
''' Bezramkowe okienko podglądu próbki mapy lub zasięgu warstwy (bez przejmowania fokusu).
''' </summary>
Public Class FormPodgladWarstwy
    Inherits Form

    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    Private ReadOnly _lblNaglowek As New Label()
    Private ReadOnly _lblMeta As New Label()
    Private ReadOnly _picObraz As New PictureBox()
    Private ReadOnly _lblKomunikat As New Label()
    Private ReadOnly _lblStopka As New Label()

    Private _jestNakladka As Boolean
    Private _tryb As TrybPodgladuWarstwy = TrybPodgladuWarstwy.Probka

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        DoubleBuffered = True
        BackColor = Color.White
        Padding = New Padding(1)
        AutoScaleMode = AutoScaleMode.None

        Dim szer = Skaluj(356)
        Dim wys = Skaluj(378)
        Size = New Size(szer, wys)

        _lblNaglowek.AutoEllipsis = True
        _lblNaglowek.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        _lblNaglowek.ForeColor = Color.FromArgb(15, 23, 42)
        _lblNaglowek.Location = New Point(Skaluj(12), Skaluj(10))
        _lblNaglowek.Size = New Size(szer - Skaluj(24), Skaluj(20))

        _lblMeta.AutoEllipsis = True
        _lblMeta.Font = New Font("Segoe UI", 8.0F, FontStyle.Regular)
        _lblMeta.ForeColor = Color.FromArgb(71, 85, 105)
        _lblMeta.Location = New Point(Skaluj(12), Skaluj(31))
        _lblMeta.Size = New Size(szer - Skaluj(24), Skaluj(18))

        _picObraz.Location = New Point(Skaluj(12), Skaluj(54))
        _picObraz.Size = New Size(szer - Skaluj(24), Skaluj(274))
        _picObraz.SizeMode = PictureBoxSizeMode.Zoom
        _picObraz.BackColor = Color.FromArgb(248, 250, 252)
        AddHandler _picObraz.Paint, AddressOf picObraz_Paint

        _lblKomunikat.Location = _picObraz.Location
        _lblKomunikat.Size = _picObraz.Size
        _lblKomunikat.TextAlign = ContentAlignment.MiddleCenter
        _lblKomunikat.Font = New Font("Segoe UI", 8.75F, FontStyle.Regular)
        _lblKomunikat.ForeColor = Color.FromArgb(100, 116, 139)
        _lblKomunikat.BackColor = Color.FromArgb(248, 250, 252)
        _lblKomunikat.Padding = New Padding(Skaluj(14))
        _lblKomunikat.Visible = False

        _lblStopka.Location = New Point(Skaluj(12), Skaluj(334))
        _lblStopka.Size = New Size(szer - Skaluj(24), Skaluj(36))
        _lblStopka.Font = New Font("Segoe UI", 8.0F, FontStyle.Regular)
        _lblStopka.ForeColor = Color.FromArgb(51, 65, 85)
        _lblStopka.AutoEllipsis = True

        Controls.Add(_lblNaglowek)
        Controls.Add(_lblMeta)
        Controls.Add(_picObraz)
        Controls.Add(_lblKomunikat)
        Controls.Add(_lblStopka)
        _lblKomunikat.BringToFront()
    End Sub

    Private Function Skaluj(wartosc As Integer) As Integer
        Return CInt(Math.Round(wartosc * (DeviceDpi / 96.0)))
    End Function

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW
            Return cp
        End Get
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Using pioro As New Pen(Color.FromArgb(180, 192, 206), 1.0F)
            e.Graphics.DrawRectangle(pioro, 0, 0, Width - 1, Height - 1)
        End Using
    End Sub

    Private Sub picObraz_Paint(sender As Object, e As PaintEventArgs)
        If _tryb = TrybPodgladuWarstwy.Probka AndAlso _jestNakladka AndAlso _picObraz.Image IsNot Nothing Then
            'Szachownica pod przezroczystymi nakładkami (jak w galerii online)
            Dim kratka = Math.Max(8, Skaluj(12))
            Using pJasny As New SolidBrush(Color.FromArgb(245, 247, 250))
                Using pCiemny As New SolidBrush(Color.FromArgb(228, 233, 240))
                    For y As Integer = 0 To _picObraz.Height Step kratka
                        For x As Integer = 0 To _picObraz.Width Step kratka
                            Dim ciemny = (((x \ kratka) + (y \ kratka)) Mod 2) = 1
                            e.Graphics.FillRectangle(If(ciemny, pCiemny, pJasny), x, y, kratka, kratka)
                        Next
                    Next
                End Using
            End Using
            'Ponowne narysowanie obrazu na szachownicy
            Dim img = _picObraz.Image
            Dim skala = Math.Min(_picObraz.Width / CSng(img.Width), _picObraz.Height / CSng(img.Height))
            Dim dw = img.Width * skala
            Dim dh = img.Height * skala
            Dim dx = (_picObraz.Width - dw) / 2.0F
            Dim dy = (_picObraz.Height - dh) / 2.0F
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
            e.Graphics.DrawImage(img, dx, dy, dw, dh)
        End If
        Using pioro As New Pen(Color.FromArgb(215, 223, 232), 1.0F)
            e.Graphics.DrawRectangle(pioro, 0, 0, _picObraz.Width - 1, _picObraz.Height - 1)
        End Using
    End Sub

    ''' <summary>Ustawia dane warstwy i wyświetla komunikat oczekiwania (np. podczas pobierania próbki).</summary>
    Public Sub PokazLadowanie(pozycja As PozycjaGaleriiWarstw, nazwaZapasowa As String, tryb As TrybPodgladuWarstwy, komunikat As String)
        _tryb = tryb
        _jestNakladka = If(pozycja?.Nakladka, False)
        UstawNaglowek(pozycja, nazwaZapasowa, tryb)
        UstawObraz(Nothing)
        _lblKomunikat.Text = komunikat
        _lblKomunikat.Visible = True
        _lblStopka.Text = OpisStopki(pozycja, tryb)
    End Sub

    ''' <summary>Wyświetla gotowy obraz (próbkę mapy albo mapę zasięgu) lub komunikat błędu.</summary>
    Public Sub PokazWynik(pozycja As PozycjaGaleriiWarstw, nazwaZapasowa As String, tryb As TrybPodgladuWarstwy, wynik As WynikPodgladuWarstwy)
        _tryb = tryb
        _jestNakladka = If(pozycja?.Nakladka, False)
        UstawNaglowek(pozycja, nazwaZapasowa, tryb)
        If wynik IsNot Nothing AndAlso wynik.Sukces AndAlso wynik.Obraz IsNot Nothing Then
            _lblKomunikat.Visible = False
            UstawObraz(wynik.Obraz)
        Else
            UstawObraz(Nothing)
            _lblKomunikat.Text = If(wynik?.Komunikat, "Brak podglądu dla tej warstwy.")
            _lblKomunikat.Visible = True
        End If
        _lblStopka.Text = OpisStopki(pozycja, tryb)
    End Sub

    ''' <summary>Ustawia okienko po lewej stronie kotwicy (drzewa) lub po prawej, w granicach ekranu.</summary>
    Public Sub UstawPozycjePrzyWierszu(kotwica As Control, prostokatWierszaEkran As Rectangle, wlasciciel As Form)
        If kotwica Is Nothing OrElse kotwica.IsDisposed Then Exit Sub
        Dim obszarEkranu = Screen.FromControl(kotwica).WorkingArea
        Dim lewaDrzewa = kotwica.PointToScreen(Point.Empty).X
        Dim prawaDrzewa = lewaDrzewa + kotwica.Width
        Dim odstep = Skaluj(8)

        Dim docelX = lewaDrzewa - Width - odstep
        If docelX < obszarEkranu.Left + 4 Then
            docelX = prawaDrzewa + odstep
        End If
        If docelX + Width > obszarEkranu.Right - 4 Then
            docelX = Math.Max(obszarEkranu.Left + 4, obszarEkranu.Right - Width - 4)
        End If

        Dim docelY = prostokatWierszaEkran.Top - Skaluj(40)
        If docelY + Height > obszarEkranu.Bottom - 4 Then
            docelY = obszarEkranu.Bottom - Height - 4
        End If
        If docelY < obszarEkranu.Top + 4 Then
            docelY = obszarEkranu.Top + 4
        End If

        Location = New Point(docelX, docelY)
        If Not Visible Then
            If wlasciciel IsNot Nothing AndAlso Not wlasciciel.IsDisposed Then
                Show(wlasciciel)
            Else
                Show()
            End If
        End If
    End Sub

    Private Sub UstawNaglowek(pozycja As PozycjaGaleriiWarstw, nazwaZapasowa As String, tryb As TrybPodgladuWarstwy)
        Dim tytul = If(Not String.IsNullOrWhiteSpace(pozycja?.Nazwa), pozycja.Nazwa, nazwaZapasowa)
        _lblNaglowek.Text = If(tryb = TrybPodgladuWarstwy.Zasieg, "Zasięg: " & tytul, tytul)

        If pozycja Is Nothing Then
            _lblMeta.Text = ""
            Exit Sub
        End If

        Dim czesci As New List(Of String)()
        czesci.Add(If(String.IsNullOrWhiteSpace(pozycja.Typ), "WMS", pozycja.Typ))
        If pozycja.Nakladka Then czesci.Add("nakładka")
        If pozycja.ZalecanyPiksel.HasValue AndAlso pozycja.ZalecanyPiksel.Value > 0 Then
            czesci.Add(pozycja.ZalecanyPiksel.Value.ToString("0.##", CultureInfo.GetCultureInfo("pl-PL")) & " m/px")
        End If
        If Not String.IsNullOrWhiteSpace(pozycja.Zrodlo) Then
            czesci.Add(pozycja.Zrodlo)
        End If
        _lblMeta.Text = String.Join(" · ", czesci)
    End Sub

    Private Shared Function OpisStopki(pozycja As PozycjaGaleriiWarstw, tryb As TrybPodgladuWarstwy) As String
        If pozycja Is Nothing Then Return ""
        If tryb = TrybPodgladuWarstwy.Probka Then
            If Not String.IsNullOrWhiteSpace(pozycja.MiejsceProbki) Then
                Return "Próbka: " & pozycja.MiejsceProbki
            End If
            Return "Zbiór: " & pozycja.Zbior & " (" & pozycja.Warstwa & ")"
        End If

        Dim z = pozycja.Zasieg
        If z Is Nothing Then Return "Cała Polska"
        Dim etykieta = If(Not String.IsNullOrWhiteSpace(z.Etykieta), z.Etykieta, "Cała Polska")
        Dim opis = GeometriaZasieguGalerii.OpisZasiegu(pozycja)
        If Not String.IsNullOrWhiteSpace(opis) AndAlso Not String.Equals(opis, etykieta, StringComparison.OrdinalIgnoreCase) Then
            Return etykieta & vbCrLf & opis
        End If
        Return etykieta
    End Function

    Private Sub UstawObraz(nowy As Bitmap)
        Dim stary = _picObraz.Image
        _picObraz.Image = nowy
        If stary IsNot Nothing AndAlso Not Object.ReferenceEquals(stary, nowy) Then
            stary.Dispose()
        End If
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            UstawObraz(Nothing)
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
