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
''' Okno „Warstwy - zaawansowane”: krycie warstw suwakami, kolejność, usuwanie i podgląd kanapki - ta sama próbka
''' co w panelu podglądu okna głównego (wspólny PodgladWarstwSilnik), więc warstwy pobrane już przez panel nie są
''' pobierane ponownie, a zmiana krycia lub kolejności tylko ponownie nakłada pobrane obrazy.
''' Okno pracuje na kopii zestawu; zmiany przyjmuje okno główne po OK.
''' </summary>
Public Class FormWarstwyZaawansowane
    Inherits Form

    ''' <summary>Bok obrazu podglądu [px] - cała próbka silnika w skali 1:1.</summary>
    Private Const BokPodgladu As Integer = PodgladWarstwSilnik.BokProbki

    Private ReadOnly _zestaw As ZestawWarstw
    Private ReadOnly _zadanie As ZadaniePobierania
    Private _zaznaczona As Integer

    'silnik podglądu okna głównego: bufor pobranych obrazów warstw próbki i usługi WMTS (pliki kafli usuwa okno główne)
    Private ReadOnly _silnik As PodgladWarstwSilnik
    Private _przerwanie As CancellationTokenSource

    'obraz 300 px pokazuje całą próbkę w skali 1:1, więc oddalenie (50%) zmniejszałoby ją tylko i otaczało szachownicą
    Private ReadOnly _poziomyZoomu As Double() = {1.0, 2.0, 4.0}
    Private Const IndeksZoomu1do1 As Integer = 0
    Private _indeksZoomu As Integer = IndeksZoomu1do1
    Private _bazaPodgladu As Bitmap

    Private ReadOnly _wiersze As New TableLayoutPanel()
    Private ReadOnly _podglad As New PictureBox()
    Private ReadOnly _btnPodglad As New Button()
    Private ReadOnly _lblStan As New Label()
    Private ReadOnly _btnDodajWarstwe As New Button()
    Private ReadOnly _lblZapytania As New Label()
    Private ReadOnly _btnZoomMinus As New Button()
    Private ReadOnly _btnZoomReset As New Button()
    Private ReadOnly _btnZoomPlus As New Button()
    Private ReadOnly _lblZoom As New Label()
    Private ReadOnly _btnKorekta As New Button()
    Private ReadOnly _podpowiedz As New ToolTip() With {.InitialDelay = 500, .ReshowDelay = 500, .AutoPopDelay = 15000}
    Private _opcjeKorekty As New OpcjeKorekcjiMip()

    Public Sub New(zestaw As ZestawWarstw, zadanie As ZadaniePobierania, zaznaczona As Integer, silnik As PodgladWarstwSilnik)
        _zestaw = zestaw
        _zadanie = zadanie
        _silnik = silnik
        _zaznaczona = zaznaczona
        Text = "Warstwy - zaawansowane"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(880, 425)

        Dim lblOpis As New Label With {
            .Location = New Point(10, 8), .Size = New Size(540, 42),
            .Text = "Kanapka warstw: na górze warstwa najwyższa, na dole podkład (1). Krycie poniżej 100% i tryby mieszania sprawiają, że warstwy łączą się z podkładem."}
        Controls.Add(lblOpis)

        _wiersze.Location = New Point(10, 54)
        _wiersze.Size = New Size(540, 316)
        _wiersze.AutoScroll = True
        _wiersze.ColumnCount = 7
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 26))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 105))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 45))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 24))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48))
        _wiersze.BorderStyle = BorderStyle.Fixed3D
        _wiersze.BackColor = SystemColors.Window
        Controls.Add(_wiersze)

        _btnDodajWarstwe.Location = New Point(10, 376)
        _btnDodajWarstwe.Size = New Size(130, 26)
        _btnDodajWarstwe.Text = "➕ Dodaj warstwę..."
        _btnDodajWarstwe.Font = New Font(Font.FontFamily, 8.25!, FontStyle.Regular)
        _podpowiedz.SetToolTip(_btnDodajWarstwe, "Dodaj kolejną warstwę mapy z dostępnych w programie zbiorów.")
        AddHandler _btnDodajWarstwe.Click, AddressOf DodajWarstwe_Click
        Controls.Add(_btnDodajWarstwe)

        _lblZapytania.Location = New Point(148, 374)
        _lblZapytania.Size = New Size(402, 44)
        Controls.Add(_lblZapytania)

        Dim lblPodglad As New Label With {.Location = New Point(560, 8), .AutoSize = True,
                                          .Text = "Podgląd kanapki - ta sama próbka co w oknie głównym:"}
        Controls.Add(lblPodglad)
        _podglad.Location = New Point(560, 26)
        _podglad.Size = New Size(BokPodgladu, BokPodgladu)
        _podglad.BorderStyle = BorderStyle.FixedSingle
        _podglad.BackColor = Color.White
        _podglad.SizeMode = PictureBoxSizeMode.CenterImage
        _podglad.Cursor = Cursors.Hand
        AddHandler _podglad.MouseEnter, Sub(s, e) _podglad.Focus()
        AddHandler _podglad.MouseWheel, AddressOf Podglad_MouseWheel
        _podpowiedz.SetToolTip(_podglad, "Podgląd kanapki. Kółko myszy: przybliż/oddal.")
        Controls.Add(_podglad)

        _btnPodglad.Location = New Point(560, 332)
        _btnPodglad.Size = New Size(110, 24)
        _btnPodglad.Text = "Pobierz podgląd"
        AddHandler _btnPodglad.Click, AddressOf PobierzPodglad
        Controls.Add(_btnPodglad)
        _lblStan.Location = New Point(676, 330)
        _lblStan.Size = New Size(190, 44)
        Controls.Add(_lblStan)

        _btnZoomMinus.Location = New Point(560, 360)
        _btnZoomMinus.Size = New Size(24, 23)
        _btnZoomMinus.Text = "-"
        _btnZoomMinus.Font = New Font(Font.FontFamily, 8.25!, FontStyle.Bold)
        _btnZoomMinus.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomMinus, "Oddal podgląd (lupa rastrowa)")
        AddHandler _btnZoomMinus.Click, AddressOf ZoomMinus_Click
        Controls.Add(_btnZoomMinus)

        _btnZoomReset.Location = New Point(586, 360)
        _btnZoomReset.Size = New Size(32, 23)
        _btnZoomReset.Text = "1:1"
        _btnZoomReset.Font = New Font(Font.FontFamily, 7.5!, FontStyle.Regular)
        _btnZoomReset.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomReset, "Przywróć skalę 1:1 (100%)")
        AddHandler _btnZoomReset.Click, AddressOf ZoomReset_Click
        Controls.Add(_btnZoomReset)

        _btnZoomPlus.Location = New Point(620, 360)
        _btnZoomPlus.Size = New Size(24, 23)
        _btnZoomPlus.Text = "+"
        _btnZoomPlus.Font = New Font(Font.FontFamily, 8.25!, FontStyle.Bold)
        _btnZoomPlus.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomPlus, "Przybliż podgląd (lupa rastrowa)")
        AddHandler _btnZoomPlus.Click, AddressOf ZoomPlus_Click
        Controls.Add(_btnZoomPlus)

        _lblZoom.Location = New Point(646, 364)
        _lblZoom.AutoSize = True
        _lblZoom.Text = "1:1"
        _lblZoom.ForeColor = SystemColors.ControlDarkDark
        Controls.Add(_lblZoom)

        _opcjeKorekty = Ustawienia.OpcjeKorekty(ProfilKorektyRastra.GarminMip)
        _btnKorekta.Location = New Point(560, 386)
        _btnKorekta.Size = New Size(140, 24)
        AktualizujPrzyciskKorekty()
        _podpowiedz.SetToolTip(_btnKorekta, "Podgląd i strojenie korekcji obrazu (kontrast, wyostrzanie, MIP) dla skomponowanych warstw.")
        AddHandler _btnKorekta.Click, AddressOf Korekta_Click
        Controls.Add(_btnKorekta)

        Dim btnOk As New Button With {.Text = "OK", .DialogResult = DialogResult.OK, .Location = New Point(710, 386), .Size = New Size(75, 24)}
        Dim btnAnuluj As New Button With {.Text = "Anuluj", .DialogResult = DialogResult.Cancel, .Location = New Point(795, 386), .Size = New Size(75, 24)}
        Controls.Add(btnOk)
        Controls.Add(btnAnuluj)
        AcceptButton = btnOk
        CancelButton = btnAnuluj
        'fokus na OK - pierwszy suwak z fokusem zmieniałby krycie strzałkami i zaznaczał najwyższą warstwę
        ActiveControl = btnOk

        If Not _silnik.ZasiegProbki().HasValue Then
            _btnPodglad.Enabled = False
            _lblStan.Text = "Podgląd wymaga poprawnego zasięgu siatki albo widoku mapy w oknie głównym."
        End If

        ZbudujWiersze()
    End Sub

    ''' <summary>Zestaw po zmianach.</summary>
    <ComponentModel.Browsable(False), ComponentModel.DesignerSerializationVisibility(ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Zestaw As ZestawWarstw
        Get
            Return _zestaw
        End Get
    End Property

    ''' <summary>Zaznaczona warstwa (indeks w kolejności nakładania).</summary>
    <ComponentModel.Browsable(False), ComponentModel.DesignerSerializationVisibility(ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Zaznaczona As Integer
        Get
            Return _zaznaczona
        End Get
    End Property

    ''' <summary>Wiersze warstw od najwyższej do podkładu (przy 12 warstwach przebudowa trwa chwilę - tylko po zmianie listy).</summary>
    Private Sub ZbudujWiersze()
        _wiersze.SuspendLayout()
        For Each c As Control In _wiersze.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        _wiersze.Controls.Clear()
        _wiersze.RowStyles.Clear()
        _wiersze.RowCount = _zestaw.Liczba + 1
        Dim wiersz As Integer = 0
        For i = _zestaw.Liczba - 1 To 0 Step -1
            Dim indeks As Integer = i
            Dim w = _zestaw(i)
            _wiersze.RowStyles.Add(New RowStyle(SizeType.Absolute, 26))
            Dim tlo As Color = If(i = _zaznaczona, SystemColors.GradientInactiveCaption, SystemColors.Window)

            Dim lblNr As New Label With {.Text = (i + 1) & ")", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .BackColor = tlo}
            'jedna linia z wielokropkiem (etykieta wysokości wiersza zawijała długie nazwy)
            Dim lblNazwa As New Label With {.Text = Form1.NazwaWarstwy(w) & If(_zestaw.WieleZrodel AndAlso Form1.ZrodloWarstwy(w) <> "", "  [" & Form1.ZrodloWarstwy(w) & "]", ""), .Anchor = AnchorStyles.Left Or AnchorStyles.Right, .Height = 16,
                                            .TextAlign = ContentAlignment.MiddleLeft, .AutoEllipsis = True, .BackColor = tlo, .UseMnemonic = False}
            _podpowiedz.SetToolTip(lblNazwa, Form1.NazwaWarstwy(w) & If(i = 0, " - podkład", "") & vbCrLf & "Warstwa " & w.Nazwa & ", zbiór map " & w.Zbior & " (" & KafleXyz.NazwaTypu(w.EfektywnyTyp) & ")" &
                                   If(w.ZalecanyPiksel > 0, vbCrLf & "Zalecany rozmiar piksela: " & w.ZalecanyPiksel.ToString("0.###") & " m", ""))

            Dim cmbTryb As New ComboBox With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Dock = DockStyle.Fill,
                .Margin = New Padding(1, 2, 1, 1),
                .Font = New Font(Font.FontFamily, 8.25!)
            }
            cmbTryb.Items.AddRange(TrybMieszaniaRozszerzenia.WszystkieNazwy)
            If i = 0 Then
                cmbTryb.SelectedIndex = 0
                cmbTryb.Enabled = False
                _podpowiedz.SetToolTip(cmbTryb, "Podkład (warstwa 1) rysowany jest jako tło w trybie zwykłym.")
            Else
                cmbTryb.SelectedIndex = CInt(w.TrybMieszania)
                _podpowiedz.SetToolTip(cmbTryb, "Tryb mieszania tej warstwy z warstwami leżącymi pod nią.")
            End If

            Dim suwak As New TrackBar With {.Minimum = ZestawWarstw.MinKrycie, .Maximum = ZestawWarstw.PelneKrycie, .SmallChange = 5,
                                            .LargeChange = 25, .TickFrequency = 25, .Value = w.Krycie, .AutoSize = False,
                                            .Dock = DockStyle.Fill, .BackColor = tlo, .Margin = New Padding(0)}
            Dim lblProcent As New Label With {.Text = w.Krycie & " %", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleRight, .BackColor = tlo}
            Dim btnUsun As New Button With {.Text = "✕", .Dock = DockStyle.Fill, .Margin = New Padding(1), .Font = New Font("Segoe UI Symbol", 8.25!)}
            _podpowiedz.SetToolTip(btnUsun, "Usuń warstwę")
            Dim panelStrzalek As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .Margin = New Padding(0), .WrapContents = False, .BackColor = tlo}
            Dim btnWyzej As New Button With {.Text = "▲", .Size = New Size(22, 22), .Margin = New Padding(1), .Enabled = i < _zestaw.Liczba - 1,
                                             .Font = New Font("Segoe UI Symbol", 8.25!)}
            Dim btnNizej As New Button With {.Text = "▼", .Size = New Size(22, 22), .Margin = New Padding(1), .Enabled = i > 0,
                                             .Font = New Font("Segoe UI Symbol", 8.25!)}
            _podpowiedz.SetToolTip(btnWyzej, "Wyżej w kanapce")
            _podpowiedz.SetToolTip(btnNizej, "Niżej w kanapce")
            panelStrzalek.Controls.Add(btnWyzej)
            panelStrzalek.Controls.Add(btnNizej)

            Dim przyKliknieciu As EventHandler = Sub(s, e) Zaznacz(indeks)
            AddHandler lblNr.Click, przyKliknieciu
            AddHandler lblNazwa.Click, przyKliknieciu
            AddHandler cmbTryb.SelectedIndexChanged,
                Sub(s, e)
                    Zaznacz(indeks)
                    Dim wybranyTryb As TrybMieszania = CType(cmbTryb.SelectedIndex, TrybMieszania)
                    _zestaw.UstawTrybMieszania(indeks, wybranyTryb)
                    OdswiezPodglad()
                End Sub
            AddHandler suwak.MouseDown, Sub(s, e) Zaznacz(indeks)
            AddHandler suwak.ValueChanged,
                Sub(s, e)
                    'suwak co 5% - krycie w polu krycia okna głównego zmienia się o tyle samo
                    Zaznacz(indeks)
                    Dim v As Integer = CInt(Math.Round(suwak.Value / 5.0)) * 5
                    _zestaw.UstawKrycie(indeks, v)
                    lblProcent.Text = _zestaw(indeks).Krycie & " %"
                    OdswiezPodglad()
                End Sub
            AddHandler btnUsun.Click,
                Sub(s, e)
                    If _zestaw.Liczba <= 1 Then
                        MessageBox.Show("To ostatnia wybrana warstwa. Aby usunąć wszystkie warstwy, użyj kosza w oknie głównym.",
                                        Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Exit Sub
                    End If
                    _zestaw.Usun(indeks)
                    _zaznaczona = Math.Min(_zaznaczona, _zestaw.Liczba - 1)
                    PoZmianieListy()
                End Sub
            AddHandler btnWyzej.Click, Sub(s, e) Przenies(indeks, indeks + 1)
            AddHandler btnNizej.Click, Sub(s, e) Przenies(indeks, indeks - 1)

            _wiersze.Controls.Add(lblNr, 0, wiersz)
            _wiersze.Controls.Add(lblNazwa, 1, wiersz)
            _wiersze.Controls.Add(cmbTryb, 2, wiersz)
            _wiersze.Controls.Add(suwak, 3, wiersz)
            _wiersze.Controls.Add(lblProcent, 4, wiersz)
            _wiersze.Controls.Add(btnUsun, 5, wiersz)
            _wiersze.Controls.Add(panelStrzalek, 6, wiersz)
            wiersz += 1
        Next
        _wiersze.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        _wiersze.ResumeLayout()
        OdswiezLiczbeZapytan()
    End Sub

    Private Sub Zaznacz(indeks As Integer)
        If indeks = _zaznaczona Then Exit Sub
        _zaznaczona = indeks
        'kolor tła zaznaczonego wiersza bez przebudowy (przebudowa zabierałaby fokus suwakowi)
        For Each c As Control In _wiersze.Controls
            Dim poz = _wiersze.GetPositionFromControl(c)
            Dim i As Integer = _zestaw.Liczba - 1 - poz.Row
            If TypeOf c Is Button OrElse TypeOf c Is ComboBox Then Continue For
            c.BackColor = If(i = _zaznaczona, SystemColors.GradientInactiveCaption, SystemColors.Window)
        Next
    End Sub

    Private Sub Przenies(zIndeksu As Integer, naIndeks As Integer)
        _zaznaczona = _zestaw.Przenies(zIndeksu, naIndeks)
        PoZmianieListy()
    End Sub

    Private Sub PoZmianieListy()
        ZbudujWiersze()
        OdswiezPodglad()
    End Sub

    Private Sub OdswiezLiczbeZapytan()
        Dim grupy = _zestaw.GrupyZapytan()
        Dim wms As Integer = grupy.Where(Function(g) Not g.CzyKafelkowa).Count()
        Dim serwery As Integer = grupy.Select(Function(g) g.Adres.ToLowerInvariant()).Distinct().Count()
        Dim ostrzezenieNakladki As String = ""
        Dim przykryta = _zestaw.ZnajdzPrzykrytaNakladke()
        If przykryta IsNot Nothing Then
            ostrzezenieNakladki = " Uwaga: nakładka """ & Form1.NazwaWarstwy(przykryta) & """ może zostać zasłonięta."
        End If
        _lblZapytania.Text = "Zapytań WMS na segment: " & wms &
            If(grupy.Count > wms, ", warstw z kafli: " & (grupy.Count - wms), "") &
            If(serwery > 1, "; serwerów: " & serwery, "") &
            If(grupy.Count > 1, ". Grupy warstw (inny serwer, krycie lub tryb mieszania) pobierane są osobno i nakładane w programie.", ".") &
            ostrzezenieNakladki
    End Sub

    ''' <summary>Pobiera brakujące obrazy warstw próbki (każdą warstwę osobno, z przezroczystym tłem) i nakłada je.</summary>
    Private Async Sub PobierzPodglad(sender As Object, e As EventArgs)
        If Not _silnik.ZasiegProbki().HasValue Then Exit Sub
        _przerwanie?.Cancel()
        Dim przerwanie As New CancellationTokenSource()
        _przerwanie = przerwanie
        _btnPodglad.Enabled = False
        Try
            Dim postep As New Progress(Of String)(Sub(kom) If Not IsDisposed AndAlso Not przerwanie.IsCancellationRequested Then _lblStan.Text = kom)
            Dim wynik = Await _silnik.PobierzPodgladAsync(_zadanie, _zestaw, przerwanie.Token, postep)
            wynik.Item1?.Dispose()
            If IsDisposed Then Exit Sub
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            OdswiezPodglad()
            Dim bledy = wynik.Item2
            Dim brakujace = _silnik.BrakujaceWarstwy(_zestaw)
            If brakujace.Count = 0 Then
                _lblStan.Text = "Zmiana krycia, trybu lub kolejności odświeża podgląd od razu."
                _podpowiedz.SetToolTip(_lblStan, "")
            Else
                Dim nazwy = String.Join(", ", brakujace.Select(Function(w) Form1.NazwaWarstwy(w)))
                _lblStan.Text = If(brakujace.Count = _zestaw.Liczba, "Błąd pobierania: ", "Brak " & brakujace.Count & " z " & _zestaw.Liczba & " warstw: ") & nazwy
                _podpowiedz.SetToolTip(_lblStan, "Czerwona ramka podglądu: brak warstw " & nazwy & "." &
                                       If(bledy.Count > 0, vbCrLf & String.Join(vbCrLf, bledy), ""))
            End If
        Catch ex As OperationCanceledException
        Catch ex As Exception
            If Not IsDisposed Then
                _lblStan.Text = "Błąd podglądu: " & OpisBledu.Z(ex)
                _podpowiedz.SetToolTip(_lblStan, ex.ToString())
            End If
        Finally
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            przerwanie.Dispose()
            If Not IsDisposed Then _btnPodglad.Enabled = True
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If Not _silnik.ZasiegProbki().HasValue Then Exit Sub
        'obrazy pobrane przez panel okna głównego są od razu widoczne; pobierane tylko brakujące warstwy
        OdswiezPodglad()
        If _silnik.BrakujaceWarstwy(_zestaw).Count > 0 Then PobierzPodglad(Nothing, Nothing)
    End Sub

    ''' <summary>Nakłada pobrane obrazy warstw wg bieżącego krycia, trybu i kolejności (warstwy jeszcze niepobrane są pomijane).</summary>
    Private Sub OdswiezPodglad()
        OdswiezLiczbeZapytan()
        If _silnik.LiczbaWarstwWBuforze = 0 OrElse Not _silnik.ZasiegProbki().HasValue Then Exit Sub
        _bazaPodgladu?.Dispose()
        _bazaPodgladu = _silnik.ZlozObraz(_zestaw)
        ZastosujZoomDoPodgladu()
    End Sub

    Private Sub ZoomPlus_Click(sender As Object, e As EventArgs)
        If _indeksZoomu < _poziomyZoomu.Length - 1 Then
            _indeksZoomu += 1
            ZastosujZoomDoPodgladu()
        End If
    End Sub

    Private Sub ZoomMinus_Click(sender As Object, e As EventArgs)
        If _indeksZoomu > 0 Then
            _indeksZoomu -= 1
            ZastosujZoomDoPodgladu()
        End If
    End Sub

    Private Sub ZoomReset_Click(sender As Object, e As EventArgs)
        If _indeksZoomu <> IndeksZoomu1do1 Then
            _indeksZoomu = IndeksZoomu1do1
            ZastosujZoomDoPodgladu()
        End If
    End Sub

    Private Sub Podglad_MouseWheel(sender As Object, e As MouseEventArgs)
        If _bazaPodgladu Is Nothing Then Exit Sub
        If e.Delta > 0 Then
            If _indeksZoomu < _poziomyZoomu.Length - 1 Then
                _indeksZoomu += 1
                ZastosujZoomDoPodgladu()
            End If
        ElseIf e.Delta < 0 Then
            If _indeksZoomu > 0 Then
                _indeksZoomu -= 1
                ZastosujZoomDoPodgladu()
            End If
        End If
    End Sub

    Private Sub ZastosujZoomDoPodgladu()
        If _bazaPodgladu Is Nothing Then
            Dim stary = _podglad.Image
            _podglad.Image = Nothing
            stary?.Dispose()
            AktualizujStanPrzyciskowZoomu()
            Exit Sub
        End If
        Dim zoom = _poziomyZoomu(_indeksZoomu)
        Dim obrazDoWyswietlenia As Bitmap = _bazaPodgladu
        If _opcjeKorekty.Wlaczona Then
            obrazDoWyswietlenia = New Bitmap(_bazaPodgladu)
            NakladanieObrazow.ZastosujKorekcjeMip(obrazDoWyswietlenia, _opcjeKorekty)
        End If
        Dim powiekszony = PodgladWarstwSilnik.ZastosujZoom(obrazDoWyswietlenia, zoom, BokPodgladu, BokPodgladu)
        If obrazDoWyswietlenia IsNot _bazaPodgladu Then obrazDoWyswietlenia.Dispose()

        'czerwona ramka tylko po próbie pobrania - przed nią brak warstwy oznacza, że pobieranie dopiero trwa
        If _przerwanie Is Nothing AndAlso _silnik.BrakujaceWarstwy(_zestaw).Count > 0 Then PodgladWarstwSilnik.RysujRamkeBraku(powiekszony)
        Dim staryImg = _podglad.Image
        _podglad.Image = powiekszony
        staryImg?.Dispose()
        _lblZoom.Text = If(zoom > 1.001, "×" & CInt(zoom), "1:1")
        AktualizujStanPrzyciskowZoomu()
    End Sub

    Private Sub Korekta_Click(sender As Object, e As EventArgs)
        If _bazaPodgladu Is Nothing Then
            MessageBox.Show("Najpierw pobierz podgląd kanapki warstw przyciskiem 'Pobierz podgląd'.", "Brak podglądu", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using dlg As New FormPodgladMip(_opcjeKorekty, _bazaPodgladu)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeKorekty = dlg.Opcje.Kopia()
                AktualizujPrzyciskKorekty()
                ZastosujZoomDoPodgladu()
            End If
        End Using
    End Sub

    Private Sub AktualizujPrzyciskKorekty()
        _btnKorekta.Text = "Korekta obrazu..."
    End Sub

    Private Sub AktualizujStanPrzyciskowZoomu()
        Dim maObraz As Boolean = _bazaPodgladu IsNot Nothing
        _btnZoomMinus.Enabled = maObraz AndAlso _indeksZoomu > 0
        _btnZoomPlus.Enabled = maObraz AndAlso _indeksZoomu < _poziomyZoomu.Length - 1
        _btnZoomReset.Enabled = maObraz AndAlso _indeksZoomu <> IndeksZoomu1do1
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _przerwanie?.Cancel()
        If DialogResult = DialogResult.OK AndAlso _opcjeKorekty.Wlaczona Then
            Ustawienia.ZapamietajKorekte(_opcjeKorekty)
        End If
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _bazaPodgladu?.Dispose()
            _bazaPodgladu = Nothing
            _podglad.Image?.Dispose()
            _podpowiedz.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub


    Private Sub DodajWarstwe_Click(sender As Object, e As EventArgs)
        If _zestaw.Liczba >= ZestawWarstw.MaksLiczba Then
            MessageBox.Show("Osiągnięto maksymalną liczbę " & ZestawWarstw.MaksLiczba & " warstw w kompozycji.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Dim zrodla = _zadanie?.ZrodlaMap
        If zrodla Is Nothing OrElse zrodla.Count = 0 Then
            MessageBox.Show("Brak dostępnych zbiorów map w bieżącej konfiguracji.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Using dlg As New FormWybierzDodatkowaWarstwe(zrodla, _zestaw)
            If dlg.ShowDialog(Me) = DialogResult.OK AndAlso dlg.WybranaWarstwa IsNot Nothing Then
                Dim nowa = dlg.WybranaWarstwa
                If _zestaw.Warstwy.Any(Function(w) w.TaSama(nowa.Adres, nowa.Nazwa)) Then
                    MessageBox.Show("Wybrana warstwa znajduje się już w bieżącym zestawie.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Sub
                End If

                Select Case _zestaw.Dodaj(nowa.Nazwa, adres:=nowa.Adres, zbior:=nowa.Zbior, zalecanyPiksel:=nowa.ZalecanyPiksel,
                                          typZrodla:=nowa.TypZrodla, nakladka:=nowa.Nakladka, umiescPodNakladkami:=True,
                                          ignorujSsl:=nowa.IgnorujSsl)
                    Case WynikDodania.BrakMiejsca
                        MessageBox.Show("Nie można dodać warstwy - brak wolnego miejsca.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Exit Sub
                    Case WynikDodania.JuzWybrana
                        MessageBox.Show("Warstwa jest już na liście.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Exit Sub
                End Select

                Dim dodana = _zestaw.Warstwy.LastOrDefault()
                If dodana IsNot Nothing Then
                    dodana.Krycie = nowa.Krycie
                    dodana.TrybMieszania = nowa.TrybMieszania
                End If

                _zaznaczona = _zestaw.Liczba - 1
                ZbudujWiersze()
                PobierzPodglad(Nothing, EventArgs.Empty)
            End If
        End Using
    End Sub

End Class
