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
''' Okno „Warstwy - zaawansowane”: krycie warstw suwakami, kolejność, usuwanie i podgląd kanapki - próbka ze środka
''' siatki, w której każda warstwa pobierana jest raz, a zmiana krycia lub kolejności tylko ponownie nakłada pobrane obrazy.
''' Okno pracuje na kopii zestawu; zmiany przyjmuje okno główne po OK.
''' </summary>
Public Class FormWarstwyZaawansowane
    Inherits Form

    ''' <summary>Bok próbki podglądu [px].</summary>
    Private Const BokPodgladu As Integer = 300

    Private ReadOnly _zestaw As ZestawWarstw
    Private ReadOnly _zadanie As ZadaniePobierania
    Private _zaznaczona As Integer

    'pobrane obrazy warstw próbki (piksele ARGB) - wspólne dla wszystkich ustawień krycia i kolejności
    Private ReadOnly _obrazy As New Dictionary(Of String, Integer())(StringComparer.Ordinal)
    'usługi WMTS podglądu (wg adresu)
    Private ReadOnly _uslugiWmts As New Dictionary(Of String, ZrodloWmts)(StringComparer.OrdinalIgnoreCase)
    Private _przerwanie As CancellationTokenSource

    Private ReadOnly _wiersze As New TableLayoutPanel()
    Private ReadOnly _podglad As New PictureBox()
    Private ReadOnly _btnPodglad As New Button()
    Private ReadOnly _lblStan As New Label()
    Private ReadOnly _lblZapytania As New Label()
    Private ReadOnly _podpowiedz As New ToolTip() With {.InitialDelay = 500, .ReshowDelay = 500, .AutoPopDelay = 15000}

    Public Sub New(zestaw As ZestawWarstw, zadanie As ZadaniePobierania, zaznaczona As Integer)
        _zestaw = zestaw
        _zadanie = zadanie
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

        _lblZapytania.Location = New Point(10, 374)
        _lblZapytania.Size = New Size(540, 44)
        Controls.Add(_lblZapytania)

        Dim lblPodglad As New Label With {.Location = New Point(560, 8), .AutoSize = True,
                                          .Text = "Podgląd kanapki - próbka ze środka siatki segmentów:"}
        Controls.Add(lblPodglad)
        _podglad.Location = New Point(560, 26)
        _podglad.Size = New Size(BokPodgladu, BokPodgladu)
        _podglad.BorderStyle = BorderStyle.FixedSingle
        _podglad.BackColor = Color.White
        _podglad.SizeMode = PictureBoxSizeMode.Zoom
        Controls.Add(_podglad)

        _btnPodglad.Location = New Point(560, 332)
        _btnPodglad.Size = New Size(110, 24)
        _btnPodglad.Text = "Pobierz podgląd"
        AddHandler _btnPodglad.Click, AddressOf PobierzPodglad
        Controls.Add(_btnPodglad)
        _lblStan.Location = New Point(676, 330)
        _lblStan.Size = New Size(190, 44)
        Controls.Add(_lblStan)

        Dim btnOk As New Button With {.Text = "OK", .DialogResult = DialogResult.OK, .Location = New Point(710, 386), .Size = New Size(75, 24)}
        Dim btnAnuluj As New Button With {.Text = "Anuluj", .DialogResult = DialogResult.Cancel, .Location = New Point(795, 386), .Size = New Size(75, 24)}
        Controls.Add(btnOk)
        Controls.Add(btnAnuluj)
        AcceptButton = btnOk
        CancelButton = btnAnuluj
        'fokus na OK - pierwszy suwak z fokusem zmieniałby krycie strzałkami i zaznaczał najwyższą warstwę
        ActiveControl = btnOk

        If _zadanie.Siatka Is Nothing Then
            _btnPodglad.Enabled = False
            _lblStan.Text = "Podgląd wymaga poprawnego zasięgu siatki w oknie głównym."
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

    Private Shared Function Klucz(w As WarstwaWybrana) As String
        Return w.Adres.ToLowerInvariant() & "|" & w.Nazwa
    End Function

    ''' <summary>Pobiera brakujące obrazy warstw próbki (każdą warstwę osobno, z przezroczystym tłem) i nakłada je.</summary>
    Private Async Sub PobierzPodglad(sender As Object, e As EventArgs)
        Dim s = _zadanie.Siatka
        If s Is Nothing Then Exit Sub
        _przerwanie?.Cancel()
        Dim przerwanie As New CancellationTokenSource()
        _przerwanie = przerwanie
        _btnPodglad.Enabled = False
        Try
            'próbka ze środka siatki z rozmiarem piksela pobierania
            Dim srodek = s.ZasiegSiatki.Srodek
            Dim pol As Double = BokPodgladu * s.RozmiarPiksela / 2
            Dim zasieg As New Zasieg(srodek.X - pol, srodek.Y - pol, srodek.X + pol, srodek.Y + pol)
            Dim bledy As New List(Of String)()
            Dim listaWarstw = _zestaw.Warstwy.ToList()
            For i = 0 To listaWarstw.Count - 1
                Dim w = listaWarstw(i)
                Dim nazwa As String = w.Nazwa
                If _obrazy.ContainsKey(Klucz(w)) Then Continue For
                _lblStan.Text = "Pobieranie warstwy " & nazwa & "…"
                Dim piksele() As Integer = Nothing
                Dim blad As String = ""
                If w.CzyKafelkowa Then
                    'kafle WMTS/XYZ/TMS - usługa przygotowana raz dla wszystkich jej warstw w kanapce
                    Dim usluga As ZrodloWmts = Nothing
                    If Not _uslugiWmts.TryGetValue(w.Adres, usluga) Then
                        Dim nazwyUslugi = _zestaw.Warstwy.Where(Function(x) String.Equals(x.Adres, w.Adres, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Nazwa).ToList()
                        Dim zrodloPliku As PlikWarstw = Nothing
                        If Not String.IsNullOrEmpty(w.Zbior) AndAlso _zadanie.ZrodlaMap IsNot Nothing Then
                            _zadanie.ZrodlaMap.TryGetValue(w.Zbior, zrodloPliku)
                        End If
                        If zrodloPliku Is Nothing Then zrodloPliku = _zadanie.ZnajdzZrodlo(-1, w.Adres)
                        usluga = Await ZrodloWmts.PrzygotujAsync(_zadanie, w.Adres, nazwyUslugi, przerwanie.Token,
                                                                 typUslugi:=w.EfektywnyTyp, plikWarstw:=zrodloPliku)
                        _uslugiWmts(w.Adres) = usluga
                    End If
                    Dim wynikWmts = Await usluga.PikseleAsync({nazwa}, zasieg, BokPodgladu, przerwanie.Token)
                    piksele = wynikWmts.Item1
                    blad = wynikWmts.Item2
                Else
                    Dim czystyAdres As String = w.Adres.Trim(""""c, " "c)
                    Dim ignorujSsl As Boolean = w.IgnorujSsl OrElse _zadanie.IgnorujSsl
                    'podkład (i = 0) nie musi mieć przezroczystości, chyba że cała sesja ma przezroczystość
                    Dim przezroczysty As Boolean = i > 0 OrElse _zadanie.Przezroczystosc
                    Dim formatWms As String = If(przezroczysty, "png", _zadanie.Format)
                    Dim adres As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, _zadanie.Uklad, zasieg, formatWms,
                                                              BokPodgladu, BokPodgladu, _zadanie.ZamienOsie, przezroczysty)
                    Dim wynik = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adres, Math.Max(5, _zadanie.LimitCzasuSekundy), przerwanie.Token, ignorujSsl)
                    
                    ' Jeśli serwer odrzucił transparentne png, spróbuj fallback na jpeg / bez przezroczystości
                    If wynik.Item1 Is Nothing AndAlso przezroczysty Then
                        Dim adresJpg As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, _zadanie.Uklad, zasieg, "jpeg",
                                                                     BokPodgladu, BokPodgladu, _zadanie.ZamienOsie, False)
                        Dim wynikJpg = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adresJpg, Math.Max(5, _zadanie.LimitCzasuSekundy), przerwanie.Token, ignorujSsl)
                        If wynikJpg.Item1 IsNot Nothing Then
                            wynik = wynikJpg
                        End If
                    End If

                    blad = wynik.Item2
                    piksele = If(blad = "", NakladanieObrazow.Piksele(wynik.Item1, BokPodgladu, BokPodgladu), Nothing)
                End If

                If blad <> "" Then
                    bledy.Add(nazwa & ": " & blad)
                ElseIf piksele IsNot Nothing Then
                    _obrazy(Klucz(w)) = piksele
                End If
            Next

            OdswiezPodglad()

            If bledy.Count = 0 Then
                _lblStan.Text = "Zmiana krycia, trybu lub kolejności odświeża podgląd od razu."
                _podpowiedz.SetToolTip(_lblStan, "")
            ElseIf _obrazy.Count > 0 Then
                _lblStan.Text = "Pobrano " & _obrazy.Count & " z " & listaWarstw.Count & " warstw. Błędy: " & String.Join("; ", bledy)
                _podpowiedz.SetToolTip(_lblStan, String.Join(vbCrLf, bledy))
            Else
                _lblStan.Text = "Błąd pobierania: " & String.Join("; ", bledy)
                _podpowiedz.SetToolTip(_lblStan, String.Join(vbCrLf, bledy))
            End If
        Catch ex As OperationCanceledException
        Catch ex As Exception
            _lblStan.Text = "Błąd podglądu: " & ex.Message
            _podpowiedz.SetToolTip(_lblStan, ex.ToString())
        Finally
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            przerwanie.Dispose()
            If Not IsDisposed Then _btnPodglad.Enabled = True
        End Try
    End Sub

    ''' <summary>Nakłada pobrane obrazy warstw wg bieżącego krycia, trybu i kolejności (warstwy jeszcze niepobrane są pomijane).</summary>
    Private Sub OdswiezPodglad()
        OdswiezLiczbeZapytan()
        If _obrazy.Count = 0 Then Exit Sub
        Dim piksele(BokPodgladu * BokPodgladu - 1) As Integer
        For i = 0 To piksele.Length - 1
            piksele(i) = &HFFFFFFFF
        Next
        For Each w In _zestaw.Warstwy
            Dim obraz() As Integer = Nothing
            If _obrazy.TryGetValue(Klucz(w), obraz) Then
                NakladanieWarstw.Naloz(piksele, obraz, w.Krycie, w.TrybMieszania)
            End If
        Next
        Dim stary = _podglad.Image
        _podglad.Image = NakladanieObrazow.DoBitmapy(piksele, BokPodgladu, BokPodgladu)
        stary?.Dispose()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _przerwanie?.Cancel()
        'kafle WMTS podglądu - w folderze tymczasowym
        For Each u In _uslugiWmts.Values
            u.UsunKafle()
        Next
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _podglad.Image?.Dispose()
            _podpowiedz.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
