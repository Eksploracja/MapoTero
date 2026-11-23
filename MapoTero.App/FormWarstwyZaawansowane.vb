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
    Private ReadOnly _czekajNaPobieranie As Task
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
    Private ReadOnly _fontCombo As New Font(Font.FontFamily, 8.25!)
    Private ReadOnly _fontSymbol As New Font("Segoe UI Symbol", 8.25!)
    Private _opcjeKorekty As New OpcjeKorekcjiMip()

    ' Korekta przekazana przez kreator: domyślnie wyłączona, wraca do kreatora i nie zmienia zapamiętanej korekty programu
    Private ReadOnly _korektaZKreatora As Boolean

    ''' <summary>Korekta obrazu ustawiona w oknie (kreator odczytuje ją po zamknięciu przyciskiem OK).</summary>
    Public ReadOnly Property OpcjeKorekty As OpcjeKorekcjiMip
        Get
            Return _opcjeKorekty.Kopia()
        End Get
    End Property

    Public Sub New(zestaw As ZestawWarstw, zadanie As ZadaniePobierania, zaznaczona As Integer, silnik As PodgladWarstwSilnik,
                   Optional korekta As OpcjeKorekcjiMip = Nothing, Optional czekajNaPobieranie As Task = Nothing)
        _korektaZKreatora = korekta IsNot Nothing
        _zestaw = zestaw
        _zadanie = zadanie
        _silnik = silnik
        _czekajNaPobieranie = czekajNaPobieranie
        _zaznaczona = zaznaczona
        Text = "Warstwy - zaawansowane"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        AutoScaleMode = AutoScaleMode.Font
        ' Dwie kolumny z marginesem 12 px: lista warstw (540 px) i podgląd (BokPodgladu). Lista i podgląd mają wspólną
        ' górną krawędź, wiersz informacji jest na jednej wysokości w obu kolumnach, a przyciski dolnego paska trzymają się
        ' krawędzi kolumn (dawniej elementy stały na przypadkowych współrzędnych i nie tworzyły linii).
        Const Margines As Integer = 12
        Const SzerokoscListy As Integer = 540
        Const WysokoscPrzycisku As Integer = 28
        Dim lewa As Integer = Margines
        Dim prawa As Integer = Margines + SzerokoscListy + Margines
        Dim koniecPrawej As Integer = prawa + BokPodgladu
        Dim gora As Integer = 50
        Dim wierszPodPodgladem As Integer = gora + BokPodgladu + 8
        Dim wierszInformacji As Integer = wierszPodPodgladem + WysokoscPrzycisku + 6
        Dim pasekDolny As Integer = wierszInformacji + 40
        ClientSize = New Size(koniecPrawej + Margines, pasekDolny + WysokoscPrzycisku + Margines)

        Dim lblOpis As New Label With {
            .Location = New Point(lewa, 8), .Size = New Size(SzerokoscListy, 38), .TextAlign = ContentAlignment.BottomLeft,
            .Text = "Kanapka warstw: na górze warstwa najwyższa, na dole podkład (1). Krycie poniżej 100% i tryby mieszania sprawiają, że warstwy łączą się z podkładem."}
        Controls.Add(lblOpis)

        _wiersze.Location = New Point(lewa, gora)
        _wiersze.Size = New Size(SzerokoscListy, wierszPodPodgladem + WysokoscPrzycisku - gora)
        _wiersze.AutoScroll = True
        _wiersze.ColumnCount = 7
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 26))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 105))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 45))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 24))
        _wiersze.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48))
        _wiersze.BorderStyle = BorderStyle.FixedSingle
        _wiersze.BackColor = SystemColors.Window
        Controls.Add(_wiersze)

        _lblZapytania.Location = New Point(lewa, wierszInformacji)
        _lblZapytania.Size = New Size(SzerokoscListy, 34)
        Controls.Add(_lblZapytania)

        _btnDodajWarstwe.Location = New Point(lewa, pasekDolny)
        _btnDodajWarstwe.Size = New Size(150, WysokoscPrzycisku)
        _btnDodajWarstwe.Text = "➕ Dodaj warstwę..."
        _podpowiedz.SetToolTip(_btnDodajWarstwe, "Dodaj kolejną warstwę mapy z dostępnych w programie zbiorów.")
        AddHandler _btnDodajWarstwe.Click, AddressOf DodajWarstwe_Click
        Controls.Add(_btnDodajWarstwe)

        Dim btnPomoc As New Button With {
            .Name = "btnPomoc",
            .Text = "Pomoc",
            .Location = New Point(_btnDodajWarstwe.Right + 6, pasekDolny),
            .Size = New Size(80, WysokoscPrzycisku)
        }
        _podpowiedz.SetToolTip(btnPomoc, "Instrukcja obsługi zaawansowanych warstw i trybów mieszania (F1)")
        AddHandler btnPomoc.Click, Sub() FormPomocMbtiles.Pokaz(Me, TematPomocy.WarstwyZaawansowane)
        Controls.Add(btnPomoc)

        Dim lblPodglad As New Label With {.Location = New Point(prawa, 8), .Size = New Size(BokPodgladu, 38),
                                          .TextAlign = ContentAlignment.BottomLeft,
                                          .Text = "Podgląd kanapki - ta sama próbka co w oknie głównym:"}
        Controls.Add(lblPodglad)
        _podglad.Location = New Point(prawa, gora)
        _podglad.Size = New Size(BokPodgladu, BokPodgladu)
        _podglad.BorderStyle = BorderStyle.FixedSingle
        _podglad.BackColor = Color.White
        _podglad.SizeMode = PictureBoxSizeMode.CenterImage
        _podglad.Cursor = Cursors.Hand
        AddHandler _podglad.MouseEnter, Sub(s, e) _podglad.Focus()
        AddHandler _podglad.MouseWheel, AddressOf Podglad_MouseWheel
        AddHandler _podglad.Paint, AddressOf RysujStanPodgladu
        _podpowiedz.SetToolTip(_podglad, "Podgląd kanapki. Kółko myszy: przybliż/oddal.")
        Controls.Add(_podglad)

        ' Lupa przy lewej krawędzi podglądu, odświeżenie przy prawej - w jednym wierszu pod podglądem
        _btnZoomMinus.Location = New Point(prawa, wierszPodPodgladem)
        _btnZoomMinus.Size = New Size(WysokoscPrzycisku, WysokoscPrzycisku)
        _btnZoomMinus.Text = "−"
        _btnZoomMinus.Font = New Font(Font.FontFamily, 10.0!, FontStyle.Bold)
        _btnZoomMinus.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomMinus, "Oddal podgląd (lupa rastrowa)")
        AddHandler _btnZoomMinus.Click, AddressOf ZoomMinus_Click
        Controls.Add(_btnZoomMinus)

        _btnZoomReset.Location = New Point(_btnZoomMinus.Right + 4, wierszPodPodgladem)
        _btnZoomReset.Size = New Size(40, WysokoscPrzycisku)
        _btnZoomReset.Text = "1:1"
        _btnZoomReset.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomReset, "Przywróć skalę 1:1 (100%)")
        AddHandler _btnZoomReset.Click, AddressOf ZoomReset_Click
        Controls.Add(_btnZoomReset)

        _btnZoomPlus.Location = New Point(_btnZoomReset.Right + 4, wierszPodPodgladem)
        _btnZoomPlus.Size = New Size(WysokoscPrzycisku, WysokoscPrzycisku)
        _btnZoomPlus.Text = "+"
        _btnZoomPlus.Font = New Font(Font.FontFamily, 10.0!, FontStyle.Bold)
        _btnZoomPlus.Enabled = False
        _podpowiedz.SetToolTip(_btnZoomPlus, "Przybliż podgląd (lupa rastrowa)")
        AddHandler _btnZoomPlus.Click, AddressOf ZoomPlus_Click
        Controls.Add(_btnZoomPlus)

        _lblZoom.Location = New Point(_btnZoomPlus.Right + 6, wierszPodPodgladem)
        _lblZoom.Size = New Size(40, WysokoscPrzycisku)
        _lblZoom.TextAlign = ContentAlignment.MiddleLeft
        _lblZoom.Text = "1:1"
        _lblZoom.ForeColor = SystemColors.ControlDarkDark
        Controls.Add(_lblZoom)

        ' „Odśwież podgląd” zamiast „Pobierz podgląd”: przycisk pobiera z serwerów próbkę i składa obraz w oknie,
        ' niczego nie zapisuje na dysku - stary napis sugerował pobranie pliku z podglądem.
        _btnPodglad.Size = New Size(130, WysokoscPrzycisku)
        _btnPodglad.Location = New Point(koniecPrawej - _btnPodglad.Width, wierszPodPodgladem)
        _btnPodglad.Text = "Odśwież podgląd"
        _podpowiedz.SetToolTip(_btnPodglad, "Pobiera z serwerów próbkę wybranych warstw i składa podgląd w tym oknie (nie zapisuje pliku).")
        AddHandler _btnPodglad.Click, AddressOf PobierzPodglad
        Controls.Add(_btnPodglad)
        _lblStan.Location = New Point(prawa, wierszInformacji)
        _lblStan.Size = New Size(BokPodgladu, 34)
        Controls.Add(_lblStan)

        ' Z kreatora korekta jest domyślnie wyłączona (dawniej zawsze zapamiętana korekta albo profil Garmin MIP)
        ' Z okna głównego podgląd startuje bez korekty, żeby pokazać warstwy takie, jakie zwraca serwer (dawniej od razu
        ' z zapamiętaną korektą). Zapamiętane wartości suwaków zostają - okno korekty zaczyna od nich po włączeniu.
        _opcjeKorekty = If(korekta IsNot Nothing, korekta.Kopia(), Ustawienia.OpcjeKorekty(ProfilKorektyRastra.GarminMip))
        If korekta Is Nothing Then _opcjeKorekty.Wlaczona = False
        _btnKorekta.Location = New Point(prawa, pasekDolny)
        _btnKorekta.Size = New Size(132, WysokoscPrzycisku)
        AktualizujPrzyciskKorekty()
        _podpowiedz.SetToolTip(_btnKorekta, "Podgląd i strojenie korekcji obrazu (kontrast, wyostrzanie, MIP) dla skomponowanych warstw.")
        AddHandler _btnKorekta.Click, AddressOf Korekta_Click
        Controls.Add(_btnKorekta)

        Dim btnAnuluj As New Button With {.Text = "Anuluj", .DialogResult = DialogResult.Cancel, .Size = New Size(78, WysokoscPrzycisku)}
        btnAnuluj.Location = New Point(koniecPrawej - btnAnuluj.Width, pasekDolny)
        Dim btnOk As New Button With {.Text = "OK", .DialogResult = DialogResult.OK, .Size = New Size(78, WysokoscPrzycisku)}
        btnOk.Location = New Point(btnAnuluj.Left - 6 - btnOk.Width, pasekDolny)
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

        KeyPreview = True
        AddHandler KeyDown, Sub(s As Object, e As KeyEventArgs)
                                If e.KeyCode = Keys.F1 AndAlso e.Modifiers = Keys.None AndAlso Not e.Handled Then
                                    e.Handled = True
                                    FormPomocMbtiles.Pokaz(Me, TematPomocy.WarstwyZaawansowane)
                                End If
                            End Sub

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
                .Font = _fontCombo
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
                                            .LargeChange = 25, .TickFrequency = 25,
                                            .Value = Math.Clamp(w.Krycie, ZestawWarstw.MinKrycie, ZestawWarstw.PelneKrycie),
                                            .AutoSize = False,
                                            .Dock = DockStyle.Fill, .BackColor = tlo, .Margin = New Padding(0)}
            Dim lblProcent As New Label With {.Text = w.Krycie & " %", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleRight, .BackColor = tlo}
            Dim btnUsun As New Button With {.Text = "✕", .Dock = DockStyle.Fill, .Margin = New Padding(1), .Font = _fontSymbol}
            _podpowiedz.SetToolTip(btnUsun, "Usuń warstwę")
            Dim panelStrzalek As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .Margin = New Padding(0), .WrapContents = False, .BackColor = tlo}
            Dim btnWyzej As New Button With {.Text = "▲", .Size = New Size(22, 22), .Margin = New Padding(1), .Enabled = i < _zestaw.Liczba - 1,
                                             .Font = _fontSymbol}
            Dim btnNizej As New Button With {.Text = "▼", .Size = New Size(22, 22), .Margin = New Padding(1), .Enabled = i > 0,
                                             .Font = _fontSymbol}
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
            If IsDisposed Then
                wynik.Item1?.Dispose()
                Exit Sub
            End If
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            OdswiezLiczbeZapytan()
            If wynik.Item1 IsNot Nothing Then
                _bazaPodgladu?.Dispose()
                _bazaPodgladu = wynik.Item1
                ZastosujZoomDoPodgladu()
            Else
                OdswiezPodglad()
            End If
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
            If Not IsDisposed AndAlso _przerwanie Is Nothing Then
                _btnPodglad.Enabled = True
                ZakonczWczytywanie()
            End If
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If Not _silnik.ZasiegProbki().HasValue Then Exit Sub
        ' Najpierw okno z napisem „Wczytywanie podglądu…”, a składanie i pobieranie próbki po jego narysowaniu
        ' (dawniej działo się przed pierwszym rysowaniem, więc okno pojawiało się z opóźnieniem).
        _wczytywanie = True
        _lblStan.Text = "Wczytywanie podglądu…"
        _podglad.Invalidate()
        Update()
        BeginInvoke(
            Async Sub()
                If IsDisposed Then Exit Sub
                If _czekajNaPobieranie IsNot Nothing AndAlso Not _czekajNaPobieranie.IsCompleted Then
                    Try
                        Await _czekajNaPobieranie
                    Catch
                    End Try
                    If IsDisposed Then Exit Sub
                End If
                'obrazy pobrane przez panel okna głównego są od razu widoczne; pobierane tylko brakujące warstwy
                OdswiezPodglad()
                If _silnik.BrakujaceWarstwy(_zestaw).Count > 0 Then
                    PobierzPodglad(Nothing, Nothing)
                Else
                    ZakonczWczytywanie()
                    _lblStan.Text = "Zmiana krycia, trybu lub kolejności odświeża podgląd od razu."
                End If
            End Sub)
    End Sub

    Private _wczytywanie As Boolean

    Private Sub ZakonczWczytywanie()
        _wczytywanie = False
        _podglad.Invalidate()
    End Sub

    ''' <summary>Napis na pustym podglądzie, dopóki próbka się wczytuje.</summary>
    Private Sub RysujStanPodgladu(sender As Object, e As PaintEventArgs)
        If Not _wczytywanie OrElse _podglad.Image IsNot Nothing Then Exit Sub
        TextRenderer.DrawText(e.Graphics, "Wczytywanie podglądu…", Font, _podglad.ClientRectangle, SystemColors.GrayText,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
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
            MessageBox.Show("Najpierw pobierz podgląd kanapki warstw przyciskiem 'Odśwież podgląd'.", "Brak podglądu", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using dlg As New FormPodgladMip(_opcjeKorekty, _bazaPodgladu)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeKorekty = dlg.Opcje.Kopia()
                _korektaZmieniona = True
                AktualizujPrzyciskKorekty()
                ZastosujZoomDoPodgladu()
            End If
        End Using
    End Sub

    Private Sub AktualizujPrzyciskKorekty()
        _btnKorekta.Text = "Korekta obrazu..."
    End Sub

    ''' <summary>Korektę zmieniono w tym oknie - tylko wtedy OK zapisuje ją w ustawieniach (także wyłączenie).</summary>
    Private _korektaZmieniona As Boolean

    Private Sub AktualizujStanPrzyciskowZoomu()
        Dim maObraz As Boolean = _bazaPodgladu IsNot Nothing
        _btnZoomMinus.Enabled = maObraz AndAlso _indeksZoomu > 0
        _btnZoomPlus.Enabled = maObraz AndAlso _indeksZoomu < _poziomyZoomu.Length - 1
        _btnZoomReset.Enabled = maObraz AndAlso _indeksZoomu <> IndeksZoomu1do1
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _przerwanie?.Cancel()
        If DialogResult = DialogResult.OK AndAlso _korektaZmieniona AndAlso Not _korektaZKreatora Then
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
            _fontCombo.Dispose()
            _fontSymbol.Dispose()
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

                Select Case _zestaw.Dodaj(nowa.Nazwa, krycie:=nowa.Krycie, adres:=nowa.Adres, zbior:=nowa.Zbior,
                                          zalecanyPiksel:=nowa.ZalecanyPiksel, typZrodla:=nowa.TypZrodla,
                                          nakladka:=nowa.Nakladka, umiescPodNakladkami:=True,
                                          ignorujSsl:=nowa.IgnorujSsl, trybMieszania:=nowa.TrybMieszania)
                    Case WynikDodania.BrakMiejsca
                        MessageBox.Show("Nie można dodać warstwy - brak wolnego miejsca.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Exit Sub
                    Case WynikDodania.JuzWybrana
                        MessageBox.Show("Warstwa jest już na liście.", "Dodaj warstwę", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Exit Sub
                End Select

                Dim indeksDodanej = -1
                For j = 0 To _zestaw.Liczba - 1
                    If _zestaw.Warstwy(j).TaSama(nowa.Adres, nowa.Nazwa) Then
                        indeksDodanej = j
                        Exit For
                    End If
                Next
                _zaznaczona = If(indeksDodanej >= 0, indeksDodanej, _zestaw.Liczba - 1)
                ZbudujWiersze()
                PobierzPodglad(Nothing, EventArgs.Empty)
            End If
        End Using
    End Sub

End Class
