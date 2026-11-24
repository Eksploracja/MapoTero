'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Szybki kreator generowania map: prowadzi użytkownika krok po kroku od celu i stylu do gotowego pliku mapy.
''' </summary>
Public Class FormKreatorMapy

    <DllImport("user32.dll")>
    Friend Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    ''' <summary>
    ''' Przywraca zminimalizowane okno do stanu sprzed minimalizacji; okna niezminimalizowanego nie zmienia.
    ''' Dlaczego nie WindowState = Normal: to zdejmuje maksymalizację, więc okno główne zmniejszało się po zamknięciu kreatora.
    ''' </summary>
    Friend Shared Sub PrzywrocZMinimalizacji(okno As Form)
        Const SW_RESTORE As Integer = 9
        If okno.WindowState = FormWindowState.Minimized Then ShowWindow(okno.Handle, SW_RESTORE)
    End Sub

    ' Wybory zapamiętane między kolejnymi otwarciami kreatora w jednej sesji programu
    Private Shared _ostatniCel As CelMapy? = Nothing
    Private Shared _ostatniSzablonId As String = ""
    Private Shared _pokazZaawansowane As Boolean = True

    ''' <summary>Wskazuje szablon (np. zapisany z galerii WWW), który kreator zaznaczy przy najbliższym otwarciu.</summary>
    Friend Shared Sub WybierzSzablonNaStarcie(id As String)
        _ostatniSzablonId = If(id, "")
    End Sub

    Private ReadOnly _form1 As Form1
    Private _biezacyKrok As Integer = 1

    Private _wybranyCel As CelMapy = CelMapy.TelefonTablet
    ' Styl z kroku 3: szablon, zestaw niestandardowy, plan skal, suwaki krycia i grupy warstw (logika w MapoTero.Core)
    Private ReadOnly _styl As StylKreatora
    Private _stopienPodgladu As StopienSkali
    Private _zasiegObrazuStylu As Zasieg?
    Private _zasiegObrazuObszaru As Zasieg?
    Private cboSkalaPodgladu As ComboBox
    Private btnDuzyPodglad As Button
    Private btnZapiszStyl As Button
    Private btnProfilTrasy As Button
    Private _plikTrasy As String = ""
    Private btnUsunStyl As Button
    Private lblNazwaWyniku As Label
    ' Rodzaj grupy warstw regulowanej suwakiem w każdym z dwóch wierszy i pola wyboru grup warstw
    Private ReadOnly _rodzajeWierszy(1) As RodzajRegulacji
    Private ReadOnly _polaGrup As New List(Of CheckBox)()
    Private ReadOnly _topyWierszaStanu As New Dictionary(Of Control, Integer)()
    Private _celListy As CelMapy? = Nothing

    Private _wybranyZasieg As Zasieg
    Private _wybranyZasiegZMapy As Zasieg
    Private _wybranyZasiegMiejscowosci As Zasieg
    Private _wybranyZasiegTrasy As Zasieg
    Private _wybranyKorytarz As Korytarz
    Private _ostatnieMiejsce As WynikGeokodowania

    ' Krok 4: format i ustawienia domyślne stosowane tylko przy zmianie celu lub stylu - dawniej każda zmiana piksela
    ' lub wariantu kasowała korektę obrazu, profil i zakres powiększeń ustawione przez użytkownika
    Private _formatDocelowy As RodzajEksportu = RodzajEksportu.MBTiles
    Private _kluczDomyslnych As String = ""
    Private _wTrakciePodsumowania As Boolean = False
    Private ReadOnly _profileListy As ProfilKorektyRastra() = {
        ProfilKorektyRastra.GarminMip, ProfilKorektyRastra.WyostrzenieKrawedzi, ProfilKorektyRastra.StareMapyKontrast,
        ProfilKorektyRastra.RozjasnienieLasow, ProfilKorektyRastra.PodkladCadStonowany, ProfilKorektyRastra.SkanCadCzarnoBialy}

    Private Function PobierzEfektywnyKorytarz() As Korytarz
        If rbObszarTrasa.Checked Then
            Return _wybranyKorytarz
        End If
        Return Nothing
    End Function

    ' Silniki i bufory podglądu prawdziwych map
    Private ReadOnly _silnikPodgladuStylu As New PodgladWarstwSilnik() With {.Bok = 512}
    Private ReadOnly _silnikPodgladuObszaru As New PodgladWarstwSilnik() With {.Bok = 512}

    ' Przesuwanie (panning) podglądu stylu myszką
    Private _przeciaganieStylu As Boolean = False
    Private _punktStartStylu As Point
    Private _bazaPrzeciaganiaStylu As Point = Point.Empty
    Private _deltaPrzeciaganiaStylu As Point = Point.Empty
    Private _wlasnyPunktStylu As PunktXY? = Nothing

    ' Przesuwanie (panning) podglądu obszaru myszką
    Private _przeciaganieObszaru As Boolean = False
    Private _punktStartObszaru As Point
    Private _bazaPrzeciaganiaObszaru As Point = Point.Empty
    Private _deltaPrzeciaganiaObszaru As Point = Point.Empty
    Private _wlasnyPunktObszaru As PunktXY? = Nothing
    Private _ctsPodgladStylu As CancellationTokenSource
    Private _ctsPodgladObszaru As CancellationTokenSource
    Private _bazaPodgladuStylu As Bitmap
    Private _bazaPodgladuObszaru As Bitmap
    Private _ostatniaProbkaBmp As Bitmap
    Private _opcjeKorekty As New OpcjeKorekcjiMip()
    ' Plik zapisany przez kreator z brakami - ponowienie może go zastąpić uzupełnioną mapą
    Private _plikZBrakami As String = ""
    Private _sciezkaZmienionaPrzezUzytkownika As Boolean = False

    Private ReadOnly _poziomyZoomu As Double() = {0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 3.0, 4.0}
    Private Const IndeksZoomu1do1 As Integer = 3
    Private _indeksZoomuStylu As Integer = IndeksZoomu1do1
    Private _indeksZoomuObszaru As Integer = IndeksZoomu1do1

    Private ReadOnly _timerWyszukiwania As New System.Windows.Forms.Timer() With {.Interval = 380}
    Private _ctsWyszukiwania As CancellationTokenSource

    Private _cts As CancellationTokenSource
    Private _trwaPraca As Boolean = False
    Private _sciezkaWygenerowanegoPliku As String = ""
    Private _folderWygenerowanejSesji As String = ""
    ' Zadania pobierania: jedno dla mapy jednoskalowej, po jednym na stopień skali dla mapy wieloskalowej
    Private ReadOnly _zadaniaPobierania As New List(Of ZadaniePobierania)
    Private _planGenerowania As List(Of PlanStopnia)
    Private _zadanieEksportu As ZadanieEksportu
    Private _opcjeKompilacji As OpcjeKompilatoraMbtiles
    ' Piksel ostatniego podglądu stylu - w stylu wieloskalowym wyznacza pokazywany stopień skali
    Private _pikselPodgladuStylu As Double = 0

    ' Kontrolki tworzone w kodzie (PrzygotujDodatkoweKontrolki)
    Private ReadOnly _podpowiedzi As New ToolTip() With {.AutoPopDelay = 15000, .InitialDelay = 400}
    Private grpSzczegolowosc As GroupBox
    Private lnkPikselZalecany As LinkLabel
    Private lnkZaawansowane As LinkLabel
    Private chkWieloskalowa As CheckBox
    Private chkOrtofotomapa As CheckBox
    ' Czy szablon i format pozwalają dodać ortofotomapę (Visible pola zależy też od widoczności panelu kroku)
    Private _ortofotomapaMozliwa As Boolean
    Private btnUstawieniaSkal As Button
    Private lblRegulacja2 As Label
    Private tbRegulacja2 As TrackBar
    Private lblRegulacjaWartosc2 As Label
    Private ReadOnly _uwagiPokrycia As New List(Of Label)()
    Private lblWskazowkaKrok4 As Label
    Private btnPonow As Button
    Private btnNowaMapa As Button
    Private btnOtworzMape As Button
    Private btnWyslijNaTelefon As Button
    Private lblSukcesWskazowka As Label
    Private lblSukcesUwaga As Label
    Private pnlKroki As Panel
    ' Kolejność kroków: cel, obszar, styl, zapis - styl wybierany po obszarze, żeby podgląd pokazywał teren użytkownika.
    ' Panele z projektanta: pnlKrok3 - obszar (krok 2), pnlKrok2 - styl (krok 3).
    Private ReadOnly _nazwyKrokow As String() = {"Cel", "Obszar", "Szablon", "Zapis"}
    ' Cel, dla którego ostatnio ustawiono korektę domyślną (zmiana celu zmienia sugerowany profil)
    Private _celKorekty As CelMapy? = Nothing
    Private lblProfilKorekty As Label
    Private _srodekPodgladuStylu As PunktXY? = Nothing

    ''' <summary>Styl wybrany w kroku 3 (testy sprawdzają przez niego zestaw warstw złożony z ustawień okna).</summary>
    Friend ReadOnly Property Styl As StylKreatora
        Get
            Return _styl
        End Get
    End Property

    Friend ReadOnly Property BiezacyKrok As Integer
        Get
            Return _biezacyKrok
        End Get
    End Property

    ''' <summary>Format pliku, który powstanie przy bieżących ustawieniach kroku 4.</summary>
    Friend ReadOnly Property FormatDocelowy As RodzajEksportu
        Get
            Return _formatDocelowy
        End Get
    End Property

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            If Form1.TrybOkienBezAktywacji Then Return True
            Return MyBase.ShowWithoutActivation
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp = MyBase.CreateParams
            If Form1.TrybOkienBezAktywacji Then
                cp.ExStyle = cp.ExStyle Or &H8000000 Or &H80 Or &H20
            End If
            Return cp
        End Get
    End Property

    Public Sub New(form1 As Form1)
        ' Styl przed InitializeComponent - zdarzenia kontrolek z projektanta już z niego korzystają
        If form1 IsNot Nothing Then
            _styl = New StylKreatora(Function() form1.ZrodlaMapSesji(), Function() form1.WybranyZestaw())
        Else
            _styl = New StylKreatora()
        End If
        InitializeComponent()
        If MapoTero.Form1.TrybOkienBezAktywacji Then
            ShowInTaskbar = False
            Opacity = 0
        End If
        _form1 = form1

        PrzygotujDodatkoweKontrolki()

        ' Plik trasy GPX/KML/KMZ można przeciągnąć na okno w kroku 2 (w testach bez rejestracji OLE RegisterDragDrop)
        If Not MapoTero.Form1.TrybTestowyBezSieci Then WlaczUpuszczanie(Me)

        ' Klawisz Esc zamyka okno
        PomocProgramu.ZamykanieKlawiszemEsc(Me, True)

        ' Pomoc i klawisz F1
        _podpowiedzi.SetToolTip(btnPomoc, "Instrukcja obsługi szybkiego kreatora mapy (F1)")
        KeyPreview = True
        AddHandler KeyDown, Sub(s As Object, e As KeyEventArgs)
                                If e.KeyCode = Keys.F1 AndAlso e.Modifiers = Keys.None AndAlso Not e.Handled Then
                                    e.Handled = True
                                    FormPomocMbtiles.Pokaz(Me, TematPomocy.Kreator)
                                End If
                            End Sub

        ' Konfiguracja timera interaktywnego wyszukiwania
        AddHandler _timerWyszukiwania.Tick, AddressOf TimerWyszukiwania_Tick

        WczytajStyleWlasne()
        InicjalizujStanPoczatkowy()
        UstawKrok(1)
    End Sub

    ''' <summary>Kontrolki i układ dodane w kodzie: sekcja szczegółowości, zwijane opcje zaawansowane, pasek kroków, ponawianie.</summary>
    Private Sub PrzygotujDodatkoweKontrolki()
        ' Lista profili korekty z nazwami z MapoTero.Core - dawniej pozycje listy w projektancie nie zgadzały się
        ' z profilami w kodzie („Stare mapy” włączało podkład CAD)
        cboProfilKorekty.Items.Clear()
        For Each p In _profileListy
            cboProfilKorekty.Items.Add(OpcjeKorekcjiMip.NazwaProfilu(p))
        Next
        cboProfilKorekty.SelectedIndex = 1

        ' Warianty: stała szerokość z wielokropkiem zamiast wychodzenia tekstu poza ramkę
        For Each rb In {rbWariantPojedynczy, rbWariantWielopoziomowy}
            rb.AutoSize = False
            rb.AutoEllipsis = True
            rb.Size = New Size(336, 21)
        Next

        lblOpisSzablonu.Size = New Size(374, 32)
        lblOpisSzablonu.AutoEllipsis = True
        lblSkladnikiNaglowek.Visible = False
        lblSkladnikiSzablonu.Location = New Point(14, 72)
        lblSkladnikiSzablonu.Size = New Size(374, 18)
        lblSkladnikiSzablonu.AutoEllipsis = True
        ' Dwa wiersze suwaków (etykieta - suwak - wartość); pierwszy to dawny suwak cieniowania z projektanta
        lblCieniowanieOpis.AutoSize = False
        lblCieniowanieOpis.Location = New Point(14, 98)
        lblCieniowanieOpis.Size = New Size(148, 18)
        tbCieniowanie.Location = New Point(162, 95)
        tbCieniowanie.Size = New Size(178, 24)
        lblCieniowanieWartosc.Location = New Point(344, 98)
        lblRegulacja2 = New Label With {.Name = "lblRegulacja2", .AutoSize = False, .Location = New Point(14, 123), .Size = New Size(148, 18),
                                        .Font = lblCieniowanieOpis.Font, .Visible = False}
        tbRegulacja2 = New TrackBar With {.Name = "tbRegulacja2", .AutoSize = False, .Location = New Point(162, 120), .Size = New Size(178, 24),
                                          .Minimum = 0, .Maximum = 100, .TickStyle = TickStyle.None, .Visible = False}
        lblRegulacjaWartosc2 = New Label With {.Name = "lblRegulacjaWartosc2", .AutoSize = True, .Location = New Point(344, 123),
                                               .Font = lblCieniowanieWartosc.Font, .Visible = False}
        AddHandler tbRegulacja2.Scroll, Sub() ZmienRegulacje(1, tbRegulacja2.Value)
        grpSzczegolySzablonu.Controls.AddRange({lblRegulacja2, tbRegulacja2, lblRegulacjaWartosc2})
        For Each c As Control In New Control() {lblStanPodgladuStyl, btnZoomMinusStyl, lblZoomStyl, btnZoomPlusStyl, btnZoomResetStyl, btnOdswiezStyl}
            c.Top -= 11
        Next
        picPodgladStylu.Location = New Point(14, 169)
        picPodgladStylu.Size = New Size(374, 211)
        ' Przycisk dużego podglądu w dolnym wierszu obok listy skal (nad obrazem podglądu zasłaniałby mapę)
        btnDuzyPodglad = New Button With {.Name = "btnDuzyPodglad", .Text = ChrW(&H2922), .Font = New Font("Segoe UI Symbol", 10.0!),
                                          .Location = New Point(362, 386), .Size = New Size(26, 26),
                                          .AccessibleName = "Duży podgląd", .UseVisualStyleBackColor = True, .TabStop = True}
        AddHandler btnDuzyPodglad.Click, Sub() PokazDuzyPodglad()
        AddHandler picPodgladStylu.DoubleClick, Sub() PokazDuzyPodglad()
        grpSzczegolySzablonu.Controls.Add(btnDuzyPodglad)
        _podpowiedzi.SetToolTip(btnDuzyPodglad, "Duży podgląd w skali 1:1 - pozwala ocenić czytelność napisów (także podwójne kliknięcie podglądu).")
        For Each c As Control In New Control() {lblStanPodgladuStyl, btnZoomMinusStyl, lblZoomStyl, btnZoomPlusStyl, btnZoomResetStyl, btnOdswiezStyl, picPodgladStylu}
            _topyWierszaStanu(c) = c.Top
        Next
        btnDostosujWarstwy.Text = "Dostosuj warstwy..."
        btnDostosujWarstwy.Width = 146
        cboSkalaPodgladu = New ComboBox With {.Name = "cboSkalaPodgladu", .AccessibleName = "Skala podglądu",
            .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(174, 387), .Size = New Size(182, 23), .Visible = False}
        AddHandler cboSkalaPodgladu.SelectionChangeCommitted,
            Sub()
                _stopienPodgladu = TryCast(cboSkalaPodgladu.SelectedItem, StopienSkali)
                _indeksZoomuStylu = IndeksZoomu1do1
                OdswiezPodgladStyluAsync()
            End Sub
        grpSzczegolySzablonu.Controls.Add(cboSkalaPodgladu)

        ' Własne szablony: zapis bieżącej kompozycji pod nazwą i usuwanie zapisanych (pod listą szablonów)
        lstSzablony.Height = 360
        btnZapiszStyl = New Button With {.Name = "btnZapiszStyl", .Text = "Zapisz jako szablon...", .Location = New Point(lstSzablony.Left, lstSzablony.Bottom + 6),
                                         .Size = New Size(200, 28), .UseVisualStyleBackColor = True}
        btnUsunStyl = New Button With {.Name = "btnUsunStyl", .Text = "Usuń szablon", .Location = New Point(lstSzablony.Left + 206, lstSzablony.Bottom + 6),
                                       .Size = New Size(lstSzablony.Width - 206, 28), .UseVisualStyleBackColor = True, .Enabled = False}
        AddHandler btnZapiszStyl.Click, AddressOf btnZapiszStyl_Click
        AddHandler btnUsunStyl.Click, AddressOf btnUsunStyl_Click
        lstSzablony.Parent.Controls.AddRange({btnZapiszStyl, btnUsunStyl})
        _podpowiedzi.SetToolTip(btnZapiszStyl, "Zapisuje bieżący skład warstw z kryciem, trybami mieszania i skalami jako własny szablon na liście - także po ponownym uruchomieniu programu.")
        _podpowiedzi.SetToolTip(btnUsunStyl, "Usuwa zaznaczony szablon zapisany przez Ciebie (wbudowanych szablonów nie można usunąć).")
        _podpowiedzi.SetToolTip(cboSkalaPodgladu, "Wybierz stopień skali, aby zobaczyć jego warstwy w granicach zaznaczenia.")

        ' Uwagi o pokryciu map (np. arkusze 1:25 000 tylko dla części kraju) pod treścią kroków 2, 3 i 4
        ClientSize = New Size(ClientSize.Width, ClientSize.Height + 40)
        For Each para In {Tuple.Create(pnlKrok2, 436), Tuple.Create(pnlKrok3, 436), Tuple.Create(pnlKrok4, 446)}
            Dim uw As New Label With {.Name = "lblUwagaPokrycia" & para.Item1.Name.Substring(para.Item1.Name.Length - 1),
                                      .Location = New Point(16, para.Item2), .Size = New Size(744, 36),
                                      .ForeColor = Color.FromArgb(150, 85, 0), .Visible = False}
            para.Item1.Controls.Add(uw)
            _uwagiPokrycia.Add(uw)
        Next

        ' Krok 4: szczegółowość mapy zawsze widoczna, reszta opcji w zwijanej sekcji
        grpSzczegolowosc = New GroupBox With {.Name = "grpSzczegolowosc", .Text = "Szczegółowość mapy",
                                              .Location = New Point(16, 180), .Size = New Size(744, 90)}
        lblRozmiarPiksela.Text = "Rozmiar piksela:"
        lblRozmiarPiksela.Location = New Point(14, 25)
        numRozmiarPiksela.Location = New Point(118, 22)
        numRozmiarPiksela.Size = New Size(70, 23)
        lblPikselJednostka.AutoSize = False
        lblPikselJednostka.Location = New Point(194, 25)
        lblPikselJednostka.Size = New Size(540, 17)
        lblPikselJednostka.AutoEllipsis = True
        lnkPikselZalecany = New LinkLabel With {.Name = "lnkPikselZalecany", .Location = New Point(14, 49), .Size = New Size(720, 17),
                                                .AutoEllipsis = True, .Visible = False, .LinkColor = Color.FromArgb(180, 90, 0),
                                                .ActiveLinkColor = Color.FromArgb(200, 60, 0)}
        AddHandler lnkPikselZalecany.LinkClicked, AddressOf lnkPikselZalecany_LinkClicked
        chkWieloskalowa = New CheckBox With {.Name = "chkWieloskalowa", .Location = New Point(14, 66), .Size = New Size(292, 20),
                                             .AutoEllipsis = True, .Checked = True, .Visible = False}
        AddHandler chkWieloskalowa.CheckedChanged, Sub()
                                                       If _biezacyKrok = 4 Then PrzygotujPodsumowanie()
                                                   End Sub
        ' Ortofotomapa przy zbliżeniu - stopień opcjonalny szablonu (domyślnie wyłączony: plik rośnie kilkakrotnie)
        chkOrtofotomapa = New CheckBox With {.Name = "chkOrtofotomapa", .Location = New Point(310, 66), .Size = New Size(272, 20),
                                             .AutoEllipsis = True, .Checked = False, .Visible = False}
        AddHandler chkOrtofotomapa.CheckedChanged, Sub()
                                                       If _biezacyKrok = 4 Then PrzygotujPodsumowanie()
                                                   End Sub
        btnUstawieniaSkal = New Button With {.Name = "btnUstawieniaSkal", .Text = "Ustawienia skal...", .Location = New Point(588, 62),
                                             .Size = New Size(146, 25), .Visible = False, .UseVisualStyleBackColor = True}
        AddHandler btnUstawieniaSkal.Click, AddressOf btnUstawieniaSkal_Click
        _podpowiedzi.SetToolTip(btnUstawieniaSkal, "Włączanie skal, ich rozmiar piksela, zakresy powiększeń i warstwy każdej skali.")
        grpSzczegolowosc.Controls.AddRange({lblRozmiarPiksela, numRozmiarPiksela, lblPikselJednostka, lnkPikselZalecany, chkWieloskalowa, chkOrtofotomapa, btnUstawieniaSkal})
        pnlKrok4.Controls.Add(grpSzczegolowosc)
        lblNazwaWyniku = New Label With {.Name = "lblNazwaWyniku", .Location = New Point(14, 17),
                                        .Size = New Size(333, 17), .AutoEllipsis = True}
        grpZapis.Controls.Add(lblNazwaWyniku)
        txtSciezkaDocelowa.Top = 34
        txtSciezkaDocelowa.AccessibleName = "Ścieżka pliku mapy"
        btnPrzegladajDocelowy.Top = 33
        lblFormatInfo.Top = 61
        lblFormatInfo.Height = 16
        lblFormatInfo.AutoEllipsis = True

        ' Oddzielenie sekcji zaawansowanej od parametrów podstawowych
        Dim pnlSepZaawansowane As New Panel With {.Name = "pnlSepZaawansowane", .Location = New Point(16, 275),
                                                 .Size = New Size(744, 1), .BackColor = Color.FromArgb(215, 215, 220)}
        pnlKrok4.Controls.Add(pnlSepZaawansowane)

        lnkZaawansowane = New LinkLabel With {.Name = "lnkZaawansowane", .Location = New Point(16, 275), .Size = New Size(0, 0), .Visible = False}
        pnlKrok4.Controls.Add(lnkZaawansowane)

        grpZaawansowane.Text = "Opcje zaawansowane"
        grpZaawansowane.Location = New Point(16, 281)
        grpZaawansowane.Size = New Size(744, 153)
        grpZaawansowane.Visible = True
        ' Korekta: pole wyboru i przycisk okna korekty (aktywny tylko po zaznaczeniu opcji); lista profili ukryta
        chkKorektaObrazu.Text = "Korekta obrazu"
        chkKorektaObrazu.AutoSize = True
        chkKorektaObrazu.Location = New Point(16, 22)
        btnDostosujKorekte.Text = "Profil / Podgląd..."
        btnDostosujKorekte.Location = New Point(150, 17)
        btnDostosujKorekte.Size = New Size(150, 26)
        btnDostosujKorekte.Enabled = chkKorektaObrazu.Checked
        lblProfilKorekty = New Label With {.Name = "lblProfilKorekty", .AutoSize = False, .Location = New Point(310, 23),
                                           .Size = New Size(420, 18), .ForeColor = Color.FromArgb(70, 70, 70), .AutoEllipsis = True}
        grpZaawansowane.Controls.Add(lblProfilKorekty)
        lblProfil.Visible = False
        cboProfilKorekty.Visible = False
        _podpowiedzi.SetToolTip(chkKorektaObrazu, "Włącz korektę obrazu (kontrast, wyostrzenie, filtr MIP), aby aktywować podgląd i wybór profilu.")
        _podpowiedzi.SetToolTip(btnDostosujKorekte, "Okno korekty obrazu: profil (np. Garmin MIP), suwaki i podgląd; przycisk „Resetuj korektę”” przywraca obraz bez zmian.")
        chkWlasnyZoom.Text = "Własny zakres powiększeń (zoom) kafli:"
        chkWlasnyZoom.Location = New Point(16, 52)
        lblZoomMin.Location = New Point(330, 54)
        numZoomMin.Location = New Point(398, 51)
        lblZoomMax.Location = New Point(465, 54)
        numZoomMax.Location = New Point(535, 51)
        lblDrawOrder.Location = New Point(16, 82)
        numDrawOrder.Location = New Point(290, 79)
        chkPodzialTomow.Location = New Point(375, 81)
        lblUkladCad.Location = New Point(16, 82)
        cboUkladCad.Location = New Point(175, 79)
        lblCadInfo.Location = New Point(400, 82)
        btnOtworzWieloskalowy.Location = New Point(16, 110)
        btnOtworzWieloskalowy.Size = New Size(280, 26)
        lblWieloskalowyOpis.Location = New Point(305, 110)
        lblWieloskalowyOpis.Size = New Size(430, 28)

        ' Krok 5: ponawianie, wskazówka wgrania, kolejna mapa
        btnPonow = New Button With {.Name = "btnPonow", .Text = "Ponów pobieranie", .Location = New Point(160, 165),
                                    .Size = New Size(170, 32), .Visible = False, .UseVisualStyleBackColor = True}
        AddHandler btnPonow.Click, AddressOf btnPonow_Click
        pnlKrok5.Controls.Add(btnPonow)
        _podpowiedzi.SetToolTip(btnPonow, "Pobiera tylko brakujące segmenty (już pobrane zostają) i ponownie tworzy plik mapy.")

        pnlSukces.Size = New Size(694, 222)
        lblSukcesTytul.Location = New Point(0, 4)
        lblSukcesPlik.Location = New Point(2, 34)
        lblSukcesPlik.Size = New Size(690, 36)
        lblSukcesWskazowka = New Label With {.Name = "lblSukcesWskazowka", .Location = New Point(2, 72), .Size = New Size(690, 36),
                                             .ForeColor = Color.FromArgb(16, 44, 87), .Font = New Font("Segoe UI", 9.0!)}
        lblSukcesUwaga = New Label With {.Name = "lblSukcesUwaga", .Location = New Point(2, 110), .Size = New Size(690, 32),
                                         .ForeColor = Color.FromArgb(180, 90, 0), .Visible = False}
        btnPokazWFolderze.Location = New Point(2, 146)
        btnPokazWFolderze.Size = New Size(140, 32)
        btnWyslijNaTelefon = New Button With {.Name = "btnWyslijNaTelefon", .Text = "Wyślij na telefon...", .Location = New Point(148, 146),
                                              .Size = New Size(155, 32), .UseVisualStyleBackColor = True, .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)}
        AddHandler btnWyslijNaTelefon.Click, AddressOf btnWyslijNaTelefon_Click
        _podpowiedzi.SetToolTip(btnWyslijNaTelefon, "Prześlij wygenerowaną mapę bezpośrednio na telefon przez lokalną sieć Wi-Fi za pomocą kodu QR.")

        btnOtworzMape = New Button With {.Name = "btnOtworzMape", .Text = "Otwórz mapę", .Location = New Point(310, 146),
                                         .Size = New Size(110, 32), .UseVisualStyleBackColor = True, .Visible = False}
        AddHandler btnOtworzMape.Click, AddressOf btnOtworzMape_Click
        _podpowiedzi.SetToolTip(btnOtworzMape, "Otwiera plik mapy w programie przypisanym w Windows (np. GeoTIFF w QGIS, KMZ w Google Earth).")

        btnZaladujDoProgramu.Location = New Point(2, 182)
        btnZaladujDoProgramu.Size = New Size(240, 32)

        btnNowaMapa = New Button With {.Name = "btnNowaMapa", .Text = "Utwórz kolejną mapę", .Location = New Point(248, 182),
                                       .Size = New Size(160, 32), .UseVisualStyleBackColor = True}
        AddHandler btnNowaMapa.Click, AddressOf btnNowaMapa_Click

        pnlSukces.Controls.AddRange({lblSukcesWskazowka, lblSukcesUwaga, btnWyslijNaTelefon, btnNowaMapa, btnOtworzMape})

        ' Stopka: pasek kroków (klik w ukończony krok wraca do niego)
        pnlKroki = New Panel With {.Name = "pnlKroki", .Location = New Point(112, 10), .Size = New Size(316, 30)}
        GetType(Panel).GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).SetValue(pnlKroki, True)
        AddHandler pnlKroki.Paint, AddressOf pnlKroki_Paint
        AddHandler pnlKroki.MouseClick, AddressOf pnlKroki_MouseClick
        AddHandler pnlKroki.MouseMove, Sub(s, e) pnlKroki.Cursor = If(KrokPodKursorem(e.X) < _biezacyKrok AndAlso Not _trwaPraca, Cursors.Hand, Cursors.Default)
        pnlStopka.Controls.Add(pnlKroki)

        _podpowiedzi.SetToolTip(numRozmiarPiksela, "Ile metrów terenu przypada na jeden piksel mapy. Mniejsza wartość - więcej szczegółów i większy plik.")
        _podpowiedzi.SetToolTip(tbCieniowanie, "Krycie warstw tej grupy: 0% - niewidoczne, 100% - pełne.")
        _podpowiedzi.SetToolTip(tbRegulacja2, "Krycie warstw tej grupy: 0% - niewidoczne, 100% - pełne.")
        _podpowiedzi.SetToolTip(btnPrzegladajDocelowy, "Wybierz folder i nazwę pliku mapy.")
        ' Profil wysokościowy wczytanej trasy (okno profilu NMT z tym samym plikiem)
        btnProfilTrasy = New Button With {.Name = "btnProfilTrasy", .Text = "Profil...", .Location = New Point(368, 4), .Size = New Size(80, 28),
                                          .UseVisualStyleBackColor = True, .Enabled = False}
        AddHandler btnProfilTrasy.Click, AddressOf btnProfilTrasy_Click
        btnWybierzTrase.Parent.Controls.Add(btnProfilTrasy)
        _podpowiedzi.SetToolTip(btnProfilTrasy, "Profil wysokościowy wczytanej trasy z numerycznego modelu terenu (NMT GUGiK).")
        _podpowiedzi.SetToolTip(btnWybierzTrase, "Wskaż plik GPX, KML lub KMZ - albo przeciągnij go myszą na okno kreatora.")
        ' Obszar miejscowości: promień albo prostokąt otaczający miejscowość z wyszukiwarki (lista szersza, pole wyszukiwania węższe)
        txtSzukajMiejsca.Width = 190
        lblPromien.Left = 194
        cboPromien.Left = 248
        cboPromien.Width = 98
        cboPromien.Items.Add(ObszarKreatora.OpcjaGranic)
        _podpowiedzi.SetToolTip(cboPromien, "Promień wokół wybranego miejsca albo „" & ObszarKreatora.OpcjaGranic & "”” - prostokąt otaczający miejscowość z wyszukiwarki (dla adresu co najmniej 1 km).")
        _podpowiedzi.SetToolTip(txtSzukajMiejsca, "Nazwa miejscowości, adres albo współrzędne punktu, np. 50.0614, 19.9366 lub 50°03'41""N 19°56'12""E (WGS84)," & vbCrLf &
                                                  "albo X Y w PL-1992 / PL-2000 (X - północ). Obszar wokół punktu wyznacza promień obok." & vbCrLf &
                                                  "Działka: identyfikator TERYT (np. 121901_1.0001.123/4) albo „działka Wieliczka 123/4”” - obszar to działka z pasem 150 m.")
        UstawWidocznoscZaawansowanych()
    End Sub

    Private Sub UstawWidocznoscZaawansowanych()
        grpZaawansowane.Visible = True
        If lblWskazowkaKrok4 IsNot Nothing Then lblWskazowkaKrok4.Visible = False
        If lnkZaawansowane IsNot Nothing Then lnkZaawansowane.Visible = False
    End Sub

    Private Sub InicjalizujStanPoczatkowy()
        ' Pola XY przechowują zaznaczenie przed zaokrągleniem do segmentów.
        If _form1 IsNot Nothing Then
            Dim z = ZasiegZOknaGlownego()
            If z.Poprawny Then
                _wybranyZasiegZMapy = z
                _wybranyZasieg = _wybranyZasiegZMapy
                lblStatusZaznaczenia.Text = "Aktualny zasięg z mapy: " & ObszarKreatora.FormatujWymiary(_wybranyZasieg)
                lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                rbObszarMapa.Checked = True
            End If
        End If

        ' Cel wybrany przy poprzednim otwarciu kreatora
        If _ostatniCel.HasValue Then
            Select Case _ostatniCel.Value
                Case CelMapy.OdbiornikGarmin : rbCelGarmin.Checked = True
                Case CelMapy.WydrukKomputer : rbCelWydruk.Checked = True
                Case CelMapy.ProjektCadGis : rbCelCadGis.Checked = True
                Case CelMapy.ProjektCad : rbCelCad.Checked = True
                Case Else : rbCelTelefon.Checked = True
            End Select
            _wybranyCel = _ostatniCel.Value
        End If

        OdswiezListeSzablonow()
    End Sub

    ''' <summary>
    ''' Wypełnia listę stylów dla bieżącego celu i zaznacza poprzednio wybrany styl (dawniej powrót z kroku 3 zaznaczał
    ''' zawsze pierwszy styl i kasował warstwy dostosowane przez użytkownika).
    ''' </summary>
    Private Sub OdswiezListeSzablonow()
        Dim idDoZaznaczenia = If(_styl.Szablon IsNot Nothing, _styl.Szablon.Id, _ostatniSzablonId)
        lstSzablony.BeginUpdate()
        lstSzablony.Items.Clear()
        For Each s In ZarzadcaSzablonow.PobierzDlaCelu(_wybranyCel)
            lstSzablony.Items.Add(s)
        Next
        lstSzablony.EndUpdate()
        _celListy = _wybranyCel
        If lstSzablony.Items.Count = 0 Then Exit Sub
        Dim indeks = 0
        For i = 0 To lstSzablony.Items.Count - 1
            If String.Equals(DirectCast(lstSzablony.Items(i), SzablonMapy).Id, idDoZaznaczenia, StringComparison.OrdinalIgnoreCase) Then
                indeks = i
                Exit For
            End If
        Next
        lstSzablony.SelectedIndex = indeks
    End Sub

    ''' <param name="zachowajUstawienia">True - ten sam styl wybrany ponownie (np. po powrocie z kroku 3): suwak cieniowania
    ''' zachowuje wartość ustawioną przez użytkownika.</param>
    Private Sub PokazSzczegolySzablonu(s As SzablonMapy, Optional zachowajUstawienia As Boolean = False)
        _styl.Szablon = s
        If s Is Nothing Then Exit Sub

        ' Bez piktogramu: etykieta (GDI) pokazywała emoji jako pusty prostokąt, lista stylów rysuje je poprawnie
        lblNazwaSzablonu.Text = s.Nazwa
        lblOpisSzablonu.Text = s.Opis

        lblSkladnikiSzablonu.Text = _styl.OpisSkladu()
        _podpowiedzi.SetToolTip(lblOpisSzablonu, s.Opis)
        _podpowiedzi.SetToolTip(lblSkladnikiSzablonu, lblSkladnikiSzablonu.Text)
        OdswiezSkalePodgladu()

        ' Suwaki krycia grup warstw: ze szablonu (krycie proporcjonalne w stopniach skali) albo z zestawu własnego
        Dim wykryte = _styl.UstawRegulacje(zachowajUstawienia)
        Dim wiersze = {Tuple.Create(lblCieniowanieOpis, tbCieniowanie, lblCieniowanieWartosc), Tuple.Create(lblRegulacja2, tbRegulacja2, lblRegulacjaWartosc2)}
        For k = 0 To wiersze.Length - 1
            Dim widoczny = k < wykryte.Count
            wiersze(k).Item1.Visible = widoczny
            wiersze(k).Item2.Visible = widoczny
            wiersze(k).Item3.Visible = widoczny
            _rodzajeWierszy(k) = If(widoczny, wykryte(k).Key, RodzajRegulacji.Brak)
            If widoczny Then
                Dim r = wykryte(k).Key
                If Not _styl.Regulacje.ContainsKey(r) Then _styl.Regulacje(r) = wykryte(k).Value
                wiersze(k).Item1.Text = ZarzadcaSzablonow.NazwaRegulacji(r) & ":"
                wiersze(k).Item2.Value = Math.Max(0, Math.Min(100, _styl.Regulacje(r)))
                wiersze(k).Item3.Text = _styl.Regulacje(r) & "%"
            End If
        Next

        ' Pola wyboru grup warstw (np. zabytki archeologiczne / nieruchome) - wiersz pod suwakami
        For Each cb In _polaGrup
            grpSzczegolySzablonu.Controls.Remove(cb)
            cb.Dispose()
        Next
        _polaGrup.Clear()
        Dim x = 14
        For Each g In _styl.GrupyDoWyboru()
            Dim nazwaGrupy = g
            Dim cb As New CheckBox With {.Name = "chkGrupa" & _polaGrup.Count, .Text = g, .AutoSize = True, .Location = New Point(x, 145),
                                         .Checked = Not _styl.WylaczoneGrupy.Contains(g)}
            AddHandler cb.CheckedChanged, Sub()
                                              If cb.Checked Then _styl.WylaczoneGrupy.Remove(nazwaGrupy) Else _styl.WylaczoneGrupy.Add(nazwaGrupy)
                                              OdswiezPodgladStyluAsync()
                                          End Sub
            grpSzczegolySzablonu.Controls.Add(cb)
            _polaGrup.Add(cb)
            x += TextRenderer.MeasureText(g, cb.Font).Width + 34
        Next
        Dim przesuniecie = If(_polaGrup.Count > 0, 24, 0)
        For Each kv In _topyWierszaStanu
            kv.Key.Top = kv.Value + przesuniecie
        Next
        picPodgladStylu.Height = 380 - picPodgladStylu.Top

        ' Ograniczenia pokrycia map (kroki 2-4)
        For Each uw In _uwagiPokrycia
            uw.Text = s.UwagaPokrycia
            uw.Visible = Not String.IsNullOrEmpty(s.UwagaPokrycia) AndAlso _styl.ZestawNiestandardowy Is Nothing
        Next

        OdswiezPodgladStyluAsync()
    End Sub

    Private Sub OdswiezSkalePodgladu()
        cboSkalaPodgladu.Items.Clear()
        cboSkalaPodgladu.Items.Add("Skala automatyczna")
        cboSkalaPodgladu.Items.AddRange(_styl.StopniePodgladu().Cast(Of Object)().ToArray())
        cboSkalaPodgladu.Visible = cboSkalaPodgladu.Items.Count > 1
        cboSkalaPodgladu.SelectedIndex = Math.Max(0, If(_stopienPodgladu Is Nothing, 0, cboSkalaPodgladu.Items.IndexOf(_stopienPodgladu)))
        _stopienPodgladu = TryCast(cboSkalaPodgladu.SelectedItem, StopienSkali)
    End Sub

    ''' <summary>Zmiana suwaka w wierszu <paramref name="wiersz"/> - natychmiastowe złożenie podglądu z warstw w buforze.</summary>
    Private Sub ZmienRegulacje(wiersz As Integer, wartosc As Integer)
        Dim r = _rodzajeWierszy(wiersz)
        If r = RodzajRegulacji.Brak Then Exit Sub
        _styl.Regulacje(r) = wartosc
        If wiersz = 0 Then lblCieniowanieWartosc.Text = wartosc & "%" Else lblRegulacjaWartosc2.Text = wartosc & "%"
        If _styl.Szablon Is Nothing Then Exit Sub
        Dim zestaw = _styl.UtworzZestaw(_pikselPodgladuStylu, _stopienPodgladu)
        Dim staraBaza = _bazaPodgladuStylu
        _bazaPodgladuStylu = _silnikPodgladuStylu.ZlozObraz(zestaw)
        staraBaza?.Dispose()
        _ostatniaProbkaBmp?.Dispose()
        _ostatniaProbkaBmp = If(_bazaPodgladuStylu IsNot Nothing, New Bitmap(_bazaPodgladuStylu), Nothing)
        ZastosujZoomStylu()
    End Sub

    Private Sub UstawKrok(krok As Integer)
        _biezacyKrok = krok

        pnlKrok1.Visible = (krok = 1)
        pnlKrok3.Visible = (krok = 2)
        pnlKrok2.Visible = (krok = 3)
        pnlKrok4.Visible = (krok = 4)
        pnlKrok5.Visible = (krok = 5)

        Select Case krok
            Case 1
                lblTytulKroku.Text = "Krok 1 z 4: Do czego potrzebujesz mapy?"
                lblPodtytulKroku.Text = "Wybierz urządzenie lub zastosowanie, a program dobierze właściwy format i optymalne parametry."
                btnWstecz.Enabled = False
                btnDalej.Visible = True
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                AcceptButton = btnDalej

            Case 2
                lblTytulKroku.Text = "Krok 2 z 4: Wybierz obszar mapy"
                lblPodtytulKroku.Text = "Wskaż interesujący Cię rejon: zaznacz prostokąt na mapie, wyszukaj miejscowość lub wskaż trasę GPX."
                btnWstecz.Enabled = True
                btnDalej.Visible = True
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                ' Enter w polu wyszukiwania uruchamia szukanie, a nie przejście dalej
                AcceptButton = Nothing
                If Not _celListy.HasValue OrElse _celListy.Value <> _wybranyCel Then OdswiezListeSzablonow()
                AktualizujPodgladObszaru()

            Case 3
                lblTytulKroku.Text = "Krok 3 z 4: Wybierz szablon mapy"
                lblPodtytulKroku.Text = If(CzyPominacKrokObszaru(),
                                           "Obszar zaznaczony na mapie: " & ObszarKreatora.FormatujWymiary(_wybranyZasieg) & ". Inny obszar wybierzesz, klikając „Obszar”” na pasku kroków.",
                                           If(_wybranyZasieg.Poprawny,
                                              "Gotowe szablony i zestawy warstw. Podgląd pokazuje środek Twojego obszaru - możesz go przesuwać i oddalać myszką.",
                                              "Gotowe szablony i zestawy warstw. Bez wybranego obszaru podgląd pokazuje przykładowy fragment szablonu."))
                btnWstecz.Enabled = True
                btnDalej.Visible = True
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                AcceptButton = btnDalej
                If Not _celListy.HasValue OrElse _celListy.Value <> _wybranyCel Then OdswiezListeSzablonow()
                ' Nowy obszar - podgląd wraca na jego środek
                Dim srodek As PunktXY? = If(_wybranyZasieg.Poprawny, _wybranyZasieg.Srodek, CType(Nothing, PunktXY?))
                If Not Nullable.Equals(srodek, _srodekPodgladuStylu) Then
                    _srodekPodgladuStylu = srodek
                    _wlasnyPunktStylu = Nothing
                    _deltaPrzeciaganiaStylu = Point.Empty
                    _bazaPrzeciaganiaStylu = Point.Empty
                End If
                OdswiezPodgladStyluAsync()

            Case 4
                lblTytulKroku.Text = "Krok 4 z 4: Podsumowanie i zapis"
                lblPodtytulKroku.Text = "Sprawdź szacowany rozmiar i czas. Ustawienia są dobrane automatycznie - możesz od razu wygenerować mapę."
                btnWstecz.Enabled = True
                btnDalej.Visible = False
                btnGeneruj.Visible = True
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                AcceptButton = btnGeneruj
                PrzygotujPodsumowanie()

            Case 5
                lblTytulKroku.Text = "Tworzenie Twojej mapy"
                lblPodtytulKroku.Text = "Pobieranie danych z serwerów i zapis pliku mapy."
                btnWstecz.Enabled = False
                btnDalej.Visible = False
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Zamknij"
                btnAnuluj.Enabled = False
                AcceptButton = Nothing
        End Select
        pnlKroki?.Invalidate()
    End Sub

#Region "Krok 4: podsumowanie"

    ''' <summary>
    ''' Odświeża krok 4. Ustawienia domyślne (piksel, wariant, korekta, zakres powiększeń) stosuje tylko przy zmianie celu
    ''' lub stylu, a przy zmianie piksela czy wariantu przelicza jedynie szacunki, format i ścieżkę.
    ''' </summary>
    Private Sub PrzygotujPodsumowanie()
        If _wTrakciePodsumowania Then Exit Sub
        _wTrakciePodsumowania = True
        Try
            Dim klucz = _styl.KluczDomyslnych(_wybranyCel)
            If klucz <> _kluczDomyslnych Then
                _kluczDomyslnych = klucz
                ZastosujUstawieniaDomyslne()
            End If
            OdswiezPodsumowanie()
        Finally
            _wTrakciePodsumowania = False
        End Try
    End Sub

    Private Sub ZastosujUstawieniaDomyslne()
        Dim siatkaOkna = If(_styl.CzyWlasnyZestaw AndAlso _form1 IsNot Nothing, _form1.BiezacaSiatka(), Nothing)
        UstawPiksel(_styl.PikselDomyslny(If(siatkaOkna IsNot Nothing, siatkaOkna.RozmiarPiksela, 0)))

        Dim wielo = ZarzadcaSzablonow.ZalecanyWariantWielopoziomowy(_wybranyCel)
        rbWariantWielopoziomowy.Checked = wielo
        rbWariantPojedynczy.Checked = Not wielo

        chkWlasnyZoom.Checked = False
        chkWieloskalowa.Checked = True
        chkOrtofotomapa.Checked = False
        _styl.PlanWlasny = Nothing
    End Sub

    Private Sub UstawPiksel(piksel As Double)
        numRozmiarPiksela.Value = CDec(Math.Max(CDbl(numRozmiarPiksela.Minimum), Math.Min(CDbl(numRozmiarPiksela.Maximum), Math.Round(piksel, 2))))
    End Sub

    Private Sub OdswiezPodsumowanie()
        Dim piksel = CDbl(numRozmiarPiksela.Value)
        Dim korytarz = PobierzEfektywnyKorytarz()
        _formatDocelowy = ZarzadcaSzablonow.DopasujFormatDocelowy(_wybranyCel, rbWariantWielopoziomowy.Checked)
        Dim zestaw = _styl.UtworzZestaw(piksel)
        Dim szerokoscGeo = ObszarKreatora.SzerokoscGeo(_wybranyZasieg)

        ' Mapa wieloskalowa: MBTiles (kompilator łączy osobno pobrane stopnie skali), SQLiteDB i JNX (każdy poziom ze źródła swojej skali)
        Dim mozliwaWieloskalowa = _styl.MozliwaWieloskalowa(_formatDocelowy)
        chkWieloskalowa.Visible = mozliwaWieloskalowa
        btnUstawieniaSkal.Visible = mozliwaWieloskalowa
        Dim wieloskalowa = CzyWieloskalowaAktywna()
        btnUstawieniaSkal.Enabled = wieloskalowa
        ' Własny plan skal ma własne piksele - pole piksela nie zmienia wtedy mapy wieloskalowej
        numRozmiarPiksela.Enabled = Not (wieloskalowa AndAlso _styl.PlanWlasny IsNot Nothing)
        btnUstawieniaSkal.Text = If(_styl.PlanWlasny IsNot Nothing, "Ustawienia skal (własne)", "Ustawienia skal...")
        ' Własny plan skal sam decyduje o ortofotomapie (wiersz w oknie „Ustawienia skal”)
        Dim opcjonalny = If(_styl.Szablon IsNot Nothing, _styl.Szablon.StopienOpcjonalny, Nothing)
        _ortofotomapaMozliwa = mozliwaWieloskalowa AndAlso opcjonalny IsNot Nothing
        chkOrtofotomapa.Visible = _ortofotomapaMozliwa
        chkOrtofotomapa.Enabled = wieloskalowa AndAlso _styl.PlanWlasny Is Nothing
        If _ortofotomapaMozliwa Then
            Dim zOrto = ZarzadcaSzablonow.PlanStopniaOpcjonalnego(_styl.Szablon, ZarzadcaSzablonow.PlanStopni(_styl.Szablon, piksel, szerokoscGeo), szerokoscGeo)
            chkOrtofotomapa.Text = PodsumowanieKreatora.OpisOrtofotomapy(zOrto)
            _podpowiedzi.SetToolTip(chkOrtofotomapa, "Na największych powiększeniach, na których mapa byłaby już tylko powiększonymi pikselami," & vbCrLf &
                                    "plik pokaże zdjęcie lotnicze ze szlakami (piksel " & opcjonalny.Piksel.ToString("0.##") & " m)." & vbCrLf &
                                    "Plik i czas pobierania rosną kilkakrotnie - szacunek poniżej uwzględnia ortofotomapę." &
                                    If(zOrto Is Nothing, vbCrLf & "Przy obecnym rozmiarze piksela mapa jest już równie dokładna - ortofotomapa nie zostanie dodana.", ""))
        End If
        Dim plan = If(wieloskalowa, PlanBiezacy(), Nothing)
        If mozliwaWieloskalowa Then
            chkWieloskalowa.Text = "Mapa wieloskalowa - inna mapa przy oddalaniu"
            _podpowiedzi.SetToolTip(chkWieloskalowa, PodsumowanieKreatora.OpisPlanu(PlanBiezacy()))
        End If

        Dim szacunek = SzacujWynik(piksel, korytarz, plan, zestaw)
        Dim siatka = PodsumowanieKreatora.SiatkaKreatora(_wybranyZasieg, piksel)

        lblSzacunekSegmenty.Text = PodsumowanieKreatora.OpisPobierania(szacunek, If(wieloskalowa, plan.Count, 0))
        lblSzacunekRozmiar.Text = $"Szacowany plik: {szacunek.SformatowanyPrzedzialRozmiaru()}"
        lblSzacunekCzas.Text = $"Pobieranie i zapis od ok. {szacunek.SformatowanyCzas()}"
        _podpowiedzi.SetToolTip(lblSzacunekCzas, "Czas zależy od odpowiedzi źródeł. Jeden segment może wymagać wielu zapytań o kafle.")

        If Not siatka.Poprawna Then
            lblSzacunekKomunikat.Text = "Nie można utworzyć mapy: " & siatka.PowodNiepoprawnosci & ". Wróć do kroku 2 i wybierz obszar."
            lblSzacunekKomunikat.ForeColor = Color.FromArgb(200, 30, 30)
        Else
            lblSzacunekKomunikat.Text = szacunek.Ostrzezenie
            Select Case szacunek.PoziomOstrzezenia
                Case 0
                    lblSzacunekKomunikat.Text = $"Zasięg wyniku – {ObszarKreatora.FormatujWymiary(_wybranyZasieg)}"
                    ' Arkusz do druku: rozmiar papieru przy tej samej rozdzielczości co skala wydruku w polu piksela
                    If _formatDocelowy = RodzajEksportu.GeoTiff Then
                        lblSzacunekKomunikat.Text &= vbCrLf & ZarzadcaSzablonow.OpisArkuszaWydruku(_wybranyZasieg.Szerokosc, _wybranyZasieg.Wysokosc, piksel) &
                                                     $" (skala 1:{ZarzadcaSzablonow.SkalaWydruku(piksel):N0})"
                    End If
                    lblSzacunekKomunikat.ForeColor = Color.FromArgb(0, 130, 40)
                Case 1 : lblSzacunekKomunikat.ForeColor = Color.FromArgb(180, 110, 0)
                Case Else : lblSzacunekKomunikat.ForeColor = Color.FromArgb(200, 30, 30)
            End Select
        End If
        btnGeneruj.Enabled = siatka.Poprawna AndAlso zestaw.Liczba > 0

        ' Etykiety wariantów zgodne z formatem, który faktycznie powstanie
        Select Case _wybranyCel
            Case CelMapy.TelefonTablet
                grpWariant.Text = "Format pliku dla telefonu / tabletu"
                rbWariantWielopoziomowy.Text = "MBTiles - zalecany (Locus Map, OruxMaps, QGIS)"
                rbWariantPojedynczy.Text = "SQLiteDB (OsmAnd, Locus Map)"
            Case CelMapy.OdbiornikGarmin
                grpWariant.Text = "Format pliku dla odbiornika Garmin"
                rbWariantPojedynczy.Text = "Garmin KMZ (Custom Maps) - zalecany"
                rbWariantWielopoziomowy.Text = "Garmin JNX (BirdsEye) - wiele powiększeń"
            Case CelMapy.WydrukKomputer
                grpWariant.Text = "Format arkusza"
                rbWariantPojedynczy.Text = "Jeden arkusz GeoTIFF - zalecany do druku"
                rbWariantWielopoziomowy.Text = "Kafle MBTiles (przeglądarki kafelkowe)"
            Case CelMapy.ProjektCadGis
                grpWariant.Text = "Format podkładu GIS"
                rbWariantPojedynczy.Text = "Arkusz GeoTIFF w PL-1992 - zalecany"
                rbWariantWielopoziomowy.Text = "Piramida kafli MBTiles (QGIS, ArcGIS)"
            Case CelMapy.ProjektCad
                grpWariant.Text = "Format podkładu CAD"
                rbWariantPojedynczy.Text = "Arkusz GeoTIFF ze skryptem SCR - zalecany"
                rbWariantWielopoziomowy.Text = "Format kaflowy (niezalecany dla CAD)"
        End Select

        ' Zakres powiększeń z planu skal albo dopasowany do piksela (gdy użytkownik nie ustawił własnego)
        Dim wieloFormat = ZarzadcaSzablonow.CzyFormatWielopoziomowy(_formatDocelowy)
        Dim zakres = PodsumowanieKreatora.ZakresPowiekszen(plan, piksel, szerokoscGeo, chkWlasnyZoom.Checked)
        If zakres IsNot Nothing Then
            numZoomMax.Value = Math.Max(numZoomMax.Minimum, Math.Min(numZoomMax.Maximum, zakres.Item2))
            numZoomMin.Value = Math.Max(numZoomMin.Minimum, Math.Min(numZoomMin.Maximum, zakres.Item1))
        End If
        chkWlasnyZoom.Enabled = wieloFormat AndAlso Not wieloskalowa
        numZoomMin.Enabled = wieloFormat AndAlso chkWlasnyZoom.Checked AndAlso Not wieloskalowa
        numZoomMax.Enabled = wieloFormat AndAlso chkWlasnyZoom.Checked AndAlso Not wieloskalowa

        lblFormatInfo.Text = PodsumowanieKreatora.OpisFormatu(_formatDocelowy, wieloskalowa, CInt(numZoomMin.Value), CInt(numZoomMax.Value))

        ' Szczegółowość: skala wydruku, opis i ostrzeżenie przed pikselem dokładniejszym niż dane
        lblPikselJednostka.Text = PodsumowanieKreatora.OpisPiksela(piksel)
        Dim zalecany = PodsumowanieKreatora.PikselZalecany(zestaw, piksel)
        If zalecany > 0 Then
            lnkPikselZalecany.Text = $"Dane mają rozdzielczość ok. {zalecany:0.##} m - mniejszy piksel nie doda szczegółów, a powiększy plik. Kliknij, aby ustawić {zalecany:0.##} m."
            lnkPikselZalecany.Tag = zalecany
            lnkPikselZalecany.Visible = True
        Else
            lnkPikselZalecany.Visible = False
        End If

        ' Opcje zależne od formatu
        Dim garmin = _wybranyCel = CelMapy.OdbiornikGarmin
        lblDrawOrder.Visible = garmin
        numDrawOrder.Visible = garmin
        chkPodzialTomow.Visible = _formatDocelowy = RodzajEksportu.KmzGarmin
        Dim cad = _wybranyCel = CelMapy.ProjektCad
        lblUkladCad.Visible = cad
        cboUkladCad.Visible = cad
        lblCadInfo.Visible = cad
        If cad AndAlso cboUkladCad.SelectedIndex < 0 Then
            Dim srodekLon = If(_wybranyZasieg.Poprawny, UkladWspolrzednych.PL1992.DoWgs84(_wybranyZasieg.Srodek).Dlugosc, 19.0)
            Dim sug = UkladWspolrzednych.SugerujStrefaPl2000(srodekLon)
            Select Case sug.Epsg
                Case 2176 : cboUkladCad.SelectedIndex = 0
                Case 2177 : cboUkladCad.SelectedIndex = 1
                Case 2178 : cboUkladCad.SelectedIndex = 2
                Case 2179 : cboUkladCad.SelectedIndex = 3
                Case Else : cboUkladCad.SelectedIndex = 4
            End Select
        End If

        Dim ukladEfektywny = PobierzWybranyUklad()
        Dim bladUkladu = ZarzadcaSzablonow.SprawdzZgodnoscUkladu(zestaw, ukladEfektywny)
        If Not String.IsNullOrEmpty(bladUkladu) Then
            lblSzacunekKomunikat.Text = bladUkladu
            lblSzacunekKomunikat.ForeColor = Color.FromArgb(200, 30, 30)
            btnGeneruj.Enabled = False
        End If
        Dim mbtiles = _formatDocelowy = RodzajEksportu.MBTiles
        btnOtworzWieloskalowy.Visible = mbtiles AndAlso Not mozliwaWieloskalowa
        lblWieloskalowyOpis.Visible = mbtiles AndAlso Not mozliwaWieloskalowa
        If mbtiles Then
            lblWieloskalowyOpis.Text = "Warstwy: " & StylKreatora.OpisZestawu(zestaw) & vbCrLf & "Osobne warstwy dla zakresów powiększeń i mieszanie."
        End If

        UstawSciezkeDocelowa()
    End Sub

    Private Sub lnkPikselZalecany_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If TypeOf lnkPikselZalecany.Tag Is Double Then UstawPiksel(CDbl(lnkPikselZalecany.Tag))
    End Sub

    ''' <summary>Nazwa pliku z nazwy trasy albo miejscowości, stylu i daty.</summary>
    Private Function ProponowanaNazwaPliku() As String
        Dim korytarz = PobierzEfektywnyKorytarz()
        Dim miejsce As String = Nothing
        If korytarz IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(korytarz.Nazwa) Then
            miejsce = korytarz.Nazwa
        ElseIf rbObszarMiejscowosc.Checked AndAlso _ostatnieMiejsce IsNot Nothing Then
            miejsce = _ostatnieMiejsce.KrotkaNazwa
        End If
        Return PodsumowanieKreatora.ProponowanaNazwaPliku(miejsce, _styl.Szablon?.Id, DateTime.Now)
    End Function

    ''' <summary>Ścieżka, pod którą nie ma jeszcze mapy (dopisuje _2, _3…), żeby nie nadpisać poprzedniej.</summary>
    Friend Shared Function UnikalnaSciezka(sciezka As String) As String
        Return PodsumowanieKreatora.UnikalnaSciezka(sciezka, AddressOf IstniejeWynik)
    End Function

    Private Shared Function IstniejeWynik(plik As String) As Boolean
        Return File.Exists(plik) OrElse Directory.Exists(plik) OrElse
            (String.Equals(Path.GetExtension(plik), ".kmz", StringComparison.OrdinalIgnoreCase) AndAlso EksportMapy.IstniejaceTomyKmz(plik).Any())
    End Function

    Private Sub UstawSciezkeDocelowa()
        Dim rozszerzenie = ZarzadcaSzablonow.RozszerzenieDlaRodzaju(_formatDocelowy)
        If Not _sciezkaZmienionaPrzezUzytkownika OrElse String.IsNullOrWhiteSpace(txtSciezkaDocelowa.Text) Then
            Dim folderZapisu = If(Not String.IsNullOrEmpty(FolderDownload) AndAlso Directory.Exists(FolderDownload),
                                  FolderDownload,
                                  Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
            txtSciezkaDocelowa.Text = UnikalnaSciezka(Path.Combine(folderZapisu, ProponowanaNazwaPliku() & rozszerzenie))
        Else
            Dim sciezka = txtSciezkaDocelowa.Text.Trim()
            If Not String.Equals(Path.GetExtension(sciezka), rozszerzenie, StringComparison.OrdinalIgnoreCase) Then
                txtSciezkaDocelowa.Text = Path.ChangeExtension(sciezka, rozszerzenie)
            End If
        End If
    End Sub

#End Region

#Region "Obsługa kroków i nawigacji"

    Private Sub rbCel_DoubleClick(sender As Object, e As EventArgs) Handles rbCelTelefon.DoubleClick, rbCelGarmin.DoubleClick, rbCelWydruk.DoubleClick, rbCelCadGis.DoubleClick, rbCelCad.DoubleClick
        btnDalej.PerformClick()
    End Sub

    Private Sub btnDalej_Click(sender As Object, e As EventArgs) Handles btnDalej.Click
        Select Case _biezacyKrok
            Case 1
                If rbCelTelefon.Checked Then
                    _wybranyCel = CelMapy.TelefonTablet
                ElseIf rbCelGarmin.Checked Then
                    _wybranyCel = CelMapy.OdbiornikGarmin
                ElseIf rbCelWydruk.Checked Then
                    _wybranyCel = CelMapy.WydrukKomputer
                ElseIf rbCelCad.Checked Then
                    _wybranyCel = CelMapy.ProjektCad
                Else
                    _wybranyCel = CelMapy.ProjektCadGis
                End If
                If Not _celKorekty.HasValue OrElse _celKorekty.Value <> _wybranyCel Then UstawKorekteDomyslna()
                _ostatniCel = _wybranyCel
                If CzyPominacKrokObszaru() Then
                    ' obszar zaznaczony przed chwilą prawym przyciskiem na mapie - krok 2 dostępny z paska kroków
                    UstawKrok(3)
                Else
                    UstawKrok(2)
                End If

            Case 2
                ' Obszar: bez obszaru można przejść dalej - kreator użyje przykładowego obszaru wybranego stylu
                If Not _wybranyZasieg.Poprawny Then
                    Dim wskazowka As String
                    If rbObszarMiejscowosc.Checked Then
                        wskazowka = "Wpisz nazwę miejscowości, kliknij „Szukaj”” i wybierz wynik z listy."
                    ElseIf rbObszarTrasa.Checked Then
                        wskazowka = "Kliknij „Wskaż plik trasy…”” i wybierz plik GPX, KML lub KMZ."
                    Else
                        wskazowka = "Kliknij „Przejdź do mapy i zaznacz…”” i zaznacz prostokąt na mapie."
                    End If
                    If MessageBox.Show("Nie wybrano obszaru mapy. " & wskazowka & vbCrLf & vbCrLf &
                                       "Czy przejść dalej bez obszaru? Kreator użyje wtedy przykładowego obszaru wybranego stylu (kilka kilometrów).",
                                       "Wybór obszaru mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
                End If
                UstawKrok(3)

            Case 3
                Dim blad = _styl.BladWyboru()
                If blad IsNot Nothing Then
                    MessageBox.Show(blad, "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Sub
                End If
                If Not _wybranyZasieg.Poprawny Then
                    Dim wzorzec = _styl.WzorcowyZasieg()
                    If Not wzorzec.Poprawny Then Exit Sub
                    _wybranyZasieg = wzorzec
                    _wybranyZasiegZMapy = wzorzec
                    rbObszarMapa.Checked = True
                    lblStatusZaznaczenia.Text = "Przykładowy obszar stylu: " & ObszarKreatora.FormatujWymiary(_wybranyZasieg)
                    lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                End If
                UstawKrok(4)
        End Select
    End Sub

    ''' <summary>
    ''' Krok obszaru jest pomijany, gdy użytkownik zaznaczył obszar prawym przyciskiem myszy w tej sesji programu, a w kreatorze
    ''' nadal wybrany jest ten obszar z mapy. Pola XY nigdy nie są puste (obszar z poprzedniej sesji), więc sama ich
    ''' zawartość nie wystarcza - kreator podsuwałby bez pytania obszar sprzed tygodnia.
    ''' </summary>
    Friend Function CzyPominacKrokObszaru() As Boolean
        If _form1 Is Nothing OrElse Not rbObszarMapa.Checked OrElse Not _form1.CzyZasiegZaznaczonyMysza() Then Return False
        Dim z = ZasiegZOknaGlownego()
        Return z.Poprawny AndAlso z.Equals(_wybranyZasieg)
    End Function

    Private Sub btnWstecz_Click(sender As Object, e As EventArgs) Handles btnWstecz.Click
        If _trwaPraca Then Exit Sub
        If _biezacyKrok = 5 Then
            ' Po błędzie, przerwaniu albo gotowej mapie - powrót do ustawień bez zamykania kreatora
            UstawKrok(4)
        ElseIf _biezacyKrok > 1 Then
            UstawKrok(_biezacyKrok - 1)
        End If
    End Sub

    Private Sub btnAnuluj_Click(sender As Object, e As EventArgs) Handles btnAnuluj.Click
        If _trwaPraca Then
            If MessageBox.Show("Trwa generowanie mapy. Czy na pewno chcesz przerwać?", "Kreator mapy",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                _cts?.Cancel()
            End If
            Exit Sub
        End If
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnPomoc_Click(sender As Object, e As EventArgs) Handles btnPomoc.Click
        FormPomocMbtiles.Pokaz(Me, TematPomocy.Kreator)
    End Sub

    Private Function KrokPodKursorem(x As Integer) As Integer
        Return Math.Max(1, Math.Min(4, x \ Math.Max(1, pnlKroki.Width \ 4) + 1))
    End Function

    Private Sub pnlKroki_MouseClick(sender As Object, e As MouseEventArgs)
        If _trwaPraca Then Exit Sub
        Dim k = KrokPodKursorem(e.X)
        If k < _biezacyKrok Then UstawKrok(k)
    End Sub

    ''' <summary>Pasek postępu kreatora: numery kroków w kółkach - ukończone zielone, bieżący niebieski.</summary>
    Private Sub pnlKroki_Paint(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Dim szer = pnlKroki.Width \ 4
        Using fNr As New Font("Segoe UI", 7.5!, FontStyle.Bold), fTekst As New Font("Segoe UI", 8.0!), fTekstB As New Font("Segoe UI", 8.0!, FontStyle.Bold)
            For i = 0 To 3
                Dim nr = i + 1
                Dim x0 = i * szer
                Dim kolor As Color
                If nr < _biezacyKrok Then
                    kolor = Color.FromArgb(0, 140, 60)
                ElseIf nr = _biezacyKrok Then
                    kolor = Color.FromArgb(0, 120, 215)
                Else
                    kolor = Color.FromArgb(175, 180, 190)
                End If
                Dim kolo As New Rectangle(x0 + 2, 6, 17, 17)
                If i > 0 Then
                    Using p As New Pen(Color.FromArgb(205, 210, 218), 1.5!)
                        g.DrawLine(p, x0 - 8, 14, x0, 14)
                    End Using
                End If
                Using b As New SolidBrush(kolor)
                    g.FillEllipse(b, kolo)
                End Using
                Dim znak = If(nr < _biezacyKrok, "✓", nr.ToString())
                TextRenderer.DrawText(g, znak, fNr, kolo, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                TextRenderer.DrawText(g, _nazwyKrokow(i), If(nr = _biezacyKrok, fTekstB, fTekst),
                                      New Rectangle(x0 + 21, 4, szer - 29, 21), If(nr > _biezacyKrok, Color.FromArgb(130, 135, 145), Color.FromArgb(40, 45, 55)),
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            Next
        End Using
    End Sub

#End Region

#Region "Krok 2: Styl i kompozycja"

    Private Sub lstSzablony_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstSzablony.DrawItem
        If e.Index < 0 OrElse e.Index >= lstSzablony.Items.Count Then Exit Sub
        Dim s = TryCast(lstSzablony.Items(e.Index), SzablonMapy)
        If s Is Nothing Then Exit Sub

        Dim wybrany = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim tlo = If(wybrany, Color.FromArgb(232, 242, 254), Color.White)

        Using bTlo As New SolidBrush(tlo)
            e.Graphics.FillRectangle(bTlo, e.Bounds)
        End Using
        ' Cienka linia oddzielająca pozycje i pasek akcentu przy wybranej
        Using pLinia As New Pen(Color.FromArgb(236, 238, 242))
            e.Graphics.DrawLine(pLinia, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
        If wybrany Then
            Using bAkcent As New SolidBrush(Color.FromArgb(0, 120, 215))
                e.Graphics.FillRectangle(bAkcent, e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height - 1)
            End Using
        End If

        Using fTytul As New Font("Segoe UI", 9.5!, FontStyle.Bold), fOpis As New Font("Segoe UI", 8.0!), fKat As New Font("Segoe UI", 7.5!)
            Dim kategoria = s.Kategoria
            Dim szerKat = If(String.IsNullOrEmpty(kategoria), 0, TextRenderer.MeasureText(kategoria, fKat).Width)
            Dim naglowek = If(Not String.IsNullOrEmpty(s.Piktogram), s.Piktogram & "  ", "") & s.Nazwa
            TextRenderer.DrawText(e.Graphics, naglowek, fTytul, New Rectangle(e.Bounds.X + 10, e.Bounds.Y + 4, e.Bounds.Width - 24 - szerKat, 20),
                                  If(wybrany, Color.FromArgb(0, 70, 150), Color.FromArgb(20, 20, 20)), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
            If szerKat > 0 Then
                TextRenderer.DrawText(e.Graphics, kategoria, fKat, New Rectangle(e.Bounds.Right - szerKat - 8, e.Bounds.Y + 6, szerKat + 4, 16),
                                      Color.FromArgb(120, 130, 145), TextFormatFlags.Right)
            End If
            ' Linia 2: składniki z wielokropkiem na szerokości pozycji (dawniej obcinane po 50 znakach)
            Dim opisTekst = If(s.Warstwy.Count > 0, s.OpisSkladnikow(), "Warstwy wybrane w oknie głównym")
            TextRenderer.DrawText(e.Graphics, opisTekst, fOpis, New Rectangle(e.Bounds.X + 14, e.Bounds.Y + 25, e.Bounds.Width - 22, 16),
                                  Color.FromArgb(95, 100, 110), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End Using
    End Sub

    Private Sub lstSzablony_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSzablony.SelectedIndexChanged
        Dim s = TryCast(lstSzablony.SelectedItem, SzablonMapy)
        If s Is Nothing Then Exit Sub
        Dim tenSam = Not _styl.Wybierz(s)
        If Not tenSam Then
            _stopienPodgladu = Nothing
            _wlasnyPunktStylu = Nothing
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
        End If
        _ostatniSzablonId = s.Id
        btnUsunStyl.Enabled = SzablonyWlasne.CzyWlasny(s)
        ' sugerowany profil korekty domyślnej zależy od stylu
        If Not tenSam Then UstawKorekteDomyslna()
        PokazSzczegolySzablonu(s, tenSam)
    End Sub

    Private Sub lstSzablony_DoubleClick(sender As Object, e As EventArgs) Handles lstSzablony.DoubleClick
        If _styl.Szablon IsNot Nothing Then
            btnDalej.PerformClick()
        End If
    End Sub

    Private Function PobierzParametryProbkiStylu() As Tuple(Of PunktXY, Double)
        Return PobierzParametryProbkiSkali(_stopienPodgladu)
    End Function

    ''' <param name="stopien">Skala, dla której liczony jest piksel próbki (Nothing - piksel próbki szablonu).</param>
    Private Function PobierzParametryProbkiSkali(stopien As StopienSkali) As Tuple(Of PunktXY, Double)
        If _styl.Szablon Is Nothing Then
            Return Tuple.Create(New PunktXY(0, 0), 1.0)
        End If
        Dim zoom = _poziomyZoomu(_indeksZoomuStylu)
        ' Podgląd pokazuje środek obszaru wybranego w kroku 2 (dawniej zawsze przykładowe miejsce stylu)
        Dim srodekObszaru As PunktXY? = If(_wybranyZasieg.Poprawny, _wybranyZasieg.Srodek, CType(Nothing, PunktXY?))
        If _styl.CzyWlasnyZestaw AndAlso _form1 IsNot Nothing Then
            Dim pt = If(_wlasnyPunktStylu, If(srodekObszaru, UkladWspolrzednych.PL1992.ZWgs84(_form1.mapa.Position.Lat, _form1.mapa.Position.Lng)))
            Dim px = Math.Max(0.1, _form1.PikselPodgladuBezSiatki())
            Dim effPx = Math.Max(0.05, px / zoom)
            Return Tuple.Create(pt, effPx)
        End If
        Return Tuple.Create(_styl.PunktProbki(_wlasnyPunktStylu, _wybranyZasieg), _styl.PikselProbki(stopien, zoom))
    End Function

    ''' <summary>Powiększenie, przy którym próbka wypełnia ramkę podglądu (1 - próbka nie mniejsza niż ramka).</summary>
    Private Shared Function SkalaWypelnienia(probka As Bitmap, ramka As Control) As Double
        If probka Is Nothing Then Return 1.0
        If probka.Width < ramka.Width OrElse probka.Height < ramka.Height Then
            Return Math.Max(ramka.Width / Math.Max(1.0, probka.Width), ramka.Height / Math.Max(1.0, probka.Height))
        End If
        Return 1.0
    End Function

    Private Sub tbCieniowanie_Scroll(sender As Object, e As EventArgs) Handles tbCieniowanie.Scroll
        ZmienRegulacje(0, tbCieniowanie.Value)
    End Sub

    Private Sub btnDostosujWarstwy_Click(sender As Object, e As EventArgs) Handles btnDostosujWarstwy.Click
        If _styl.Szablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        ' Przy automatycznej skali podglądu okno pokazuje najdokładniejszą skalę: ma najwięcej warstw (np. w mapach WIG
        ' także arkusze 1:25 000, których nie było w próbce 1:100 000). Zmiany krycia i trybu warstw wspólnych
        ' obowiązują we wszystkich skalach (ZmianyWygladu).
        Dim stopienOkna = _styl.StopienOknaWarstw(_stopienPodgladu)
        Dim zestaw = If(stopienOkna Is Nothing, _styl.UtworzZestaw(_pikselPodgladuStylu, _stopienPodgladu), _styl.UtworzZestaw(0, stopienOkna))
        Dim przed = zestaw.Kopia()

        Dim parametry = PobierzParametryProbkiSkali(If(stopienOkna, _stopienPodgladu))
        Dim punkt = parametry.Item1
        Dim probkaPiksel = parametry.Item2
        Dim pol = _silnikPodgladuStylu.Bok * probkaPiksel / 2.0
        Dim zasiegProbki As New Zasieg(punkt.X - pol, punkt.Y - pol, punkt.X + pol, punkt.Y + pol)
        Dim siatkaProbki As New Siatka(zasiegProbki, probkaPiksel, _silnikPodgladuStylu.Bok)

        Dim zadanie As New ZadaniePobierania With {
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatkaProbki,
            .Zbiory = zestaw.Zbiory(),
            .ZrodlaMap = _styl.ZrodlaMap(),
            .Format = "png",
            .LimitCzasuSekundy = 15,
            .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_warstwy") & "\"
        }

        Using okno As New FormWarstwyZaawansowane(zestaw, zadanie, 0, _silnikPodgladuStylu, OpcjeKorektyDoOkna())
            Dim stopienTytulu = If(stopienOkna, _stopienPodgladu)
            If stopienTytulu IsNot Nothing Then okno.Text &= " - skala " & stopienTytulu.Opis
            If okno.ShowDialog(Me) = DialogResult.OK Then
                ' korekta ustawiona w oknie warstw trafia do kroku 4 (jedno ustawienie korekty w kreatorze)
                _opcjeKorekty = okno.OpcjeKorekty
                chkKorektaObrazu.Checked = _opcjeKorekty.Wlaczona
                AktualizujOpisKorekty()
                Select Case _styl.PrzyjmijZestawOkna(przed, zestaw,
                        Function() MessageBox.Show("Zmieniono skład lub kolejność warstw. Użyć tego zestawu na wszystkich powiększeniach?",
                                                   "Własny zestaw warstw", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes)
                    Case ZmianaZestawuOkna.ZmianaWygladu
                        PokazSzczegolySzablonu(_styl.Szablon, True)
                    Case ZmianaZestawuOkna.ZestawNiestandardowy
                        _stopienPodgladu = Nothing
                        PokazSzczegolySzablonu(_styl.Szablon, False)
                End Select
            End If
        End Using
    End Sub

    Private Async Sub OdswiezPodgladStyluAsync(Optional wymus As Boolean = False)
        If _styl.Szablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        _ctsPodgladStylu?.Cancel()
        _ctsPodgladStylu?.Dispose()
        _ctsPodgladStylu = New CancellationTokenSource()
        Dim token = _ctsPodgladStylu.Token

        lblStanPodgladuStyl.Text = "Pobieranie próbki terenu..."
        lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 100, 180)

        _pikselPodgladuStylu = PobierzParametryProbkiStylu().Item2
        Dim zestaw = _styl.UtworzZestaw(_pikselPodgladuStylu, _stopienPodgladu)
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then
            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = Nothing
            staraBaza?.Dispose()
            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = Nothing
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
            ZastosujZoomStylu()
            lblStanPodgladuStyl.Text = If(_styl.CzyWlasnyZestaw, "Brak warstw w oknie głównym", "Brak warstw w zestawie")
            lblStanPodgladuStyl.ForeColor = Color.DarkOrange
            picPodgladStylu.Invalidate()
            Exit Sub
        End If

        Dim parametry = PobierzParametryProbkiStylu()
        Dim punkt = parametry.Item1
        Dim probkaPiksel = parametry.Item2

        Dim zmieniono = _silnikPodgladuStylu.SprawdzIZaktualizujParametry(punkt, probkaPiksel, UkladWspolrzednych.PL1992, wymus)

        ' Jeśli parametry próbki się nie zmieniły i wszystkie warstwy są już w buforze, składamy obraz natychmiast w RAM
        If Not zmieniono AndAlso zestaw.Warstwy.All(Function(w) _silnikPodgladuStylu.CzyWarstwaWBuforze(w)) Then
            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = _silnikPodgladuStylu.ZlozObraz(zestaw)
            _zasiegObrazuStylu = _silnikPodgladuStylu.ZasiegProbki()
            staraBaza?.Dispose()
            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = If(_bazaPodgladuStylu IsNot Nothing, New Bitmap(_bazaPodgladuStylu), Nothing)
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
            ZastosujZoomStylu()
            lblStanPodgladuStyl.Text = _styl.OpisSkaliPodgladu(_stopienPodgladu, _pikselPodgladuStylu)
            lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 130, 40)
            Exit Sub
        End If

        If Form1.TrybTestowyBezSieci Then
            lblStanPodgladuStyl.Text = "Podgląd offline (tryb testowy)"
            lblStanPodgladuStyl.ForeColor = Color.FromArgb(120, 120, 120)
            Exit Sub
        End If

        Try
            Dim zasiegProbki = _silnikPodgladuStylu.ZasiegProbki()
            If Not zasiegProbki.HasValue Then
                Dim pol = _silnikPodgladuStylu.Bok * probkaPiksel / 2.0
                zasiegProbki = New Zasieg(punkt.X - pol, punkt.Y - pol, punkt.X + pol, punkt.Y + pol)
            End If
            Dim siatkaProbki As New Siatka(zasiegProbki.Value, probkaPiksel, _silnikPodgladuStylu.Bok)

            Dim zadanie As New ZadaniePobierania With {
                .Uklad = UkladWspolrzednych.PL1992,
                .Siatka = siatkaProbki,
                .Zbiory = zestaw.Zbiory(),
                .ZrodlaMap = _styl.ZrodlaMap(),
                .Format = "png",
                .LimitCzasuSekundy = 15,
                .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_styl") & "\"
            }

            Dim postep As New Progress(Of String)(Sub(msg)
                                                      If Not token.IsCancellationRequested Then
                                                          lblStanPodgladuStyl.Text = msg
                                                          lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 100, 180)
                                                      End If
                                                  End Sub)

            Dim wynik = Await _silnikPodgladuStylu.PobierzPodgladAsync(zadanie, zestaw, token, postep)
            If token.IsCancellationRequested OrElse IsDisposed Then
                wynik.Item1?.Dispose()
                Exit Sub
            End If

            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = wynik.Item1
            _zasiegObrazuStylu = zasiegProbki
            staraBaza?.Dispose()

            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = If(_bazaPodgladuStylu IsNot Nothing, New Bitmap(_bazaPodgladuStylu), Nothing)

            ' Gdy nowa próbka terenu jest gotowa, zerujemy przesunięcie ekranowe - przejście jest całkowicie płynne
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
            ZastosujZoomStylu()

            If _bazaPodgladuStylu Is Nothing Then
                lblStanPodgladuStyl.Text = "Brak podglądu z serwera"
                lblStanPodgladuStyl.ForeColor = Color.FromArgb(180, 50, 0)
            ElseIf wynik.Item2.Count > 0 Then
                ' Część warstw nie przyszła - podgląd nie wygląda tak, jak obiecuje opis stylu
                lblStanPodgladuStyl.Text = "Niepełny podgląd: " & wynik.Item2.Count & " warstw bez odpowiedzi"
                lblStanPodgladuStyl.ForeColor = Color.FromArgb(180, 90, 0)
                _podpowiedzi.SetToolTip(lblStanPodgladuStyl, String.Join(vbCrLf, wynik.Item2))
            Else
                lblStanPodgladuStyl.Text = _styl.OpisSkaliPodgladu(_stopienPodgladu, _pikselPodgladuStylu)
                lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 130, 40)
                _podpowiedzi.SetToolTip(lblStanPodgladuStyl, "")
            End If
        Catch ex As OperationCanceledException
        Catch ex As ObjectDisposedException
            'okno zamknięte w trakcie pobierania podglądu
        Catch ex As Exception
            If IsDisposed Then Exit Sub
            lblStanPodgladuStyl.Text = "Błąd podglądu: " & OpisBledu.Z(ex)
            lblStanPodgladuStyl.ForeColor = Color.Red
        End Try
    End Sub

    Private Sub ZastosujZoomStylu()
        If _bazaPodgladuStylu Is Nothing Then
            Dim stary = picPodgladStylu.Image
            picPodgladStylu.Image = Nothing
            stary?.Dispose()
            lblZoomStyl.Text = "1:1"
            Exit Sub
        End If

        Dim zoom = _poziomyZoomu(_indeksZoomuStylu)
        Dim powiekszony = PodgladWarstwSilnik.ZastosujZoom(_bazaPodgladuStylu, 1.0,
                                                          picPodgladStylu.Width, picPodgladStylu.Height,
                                                          dopasujWypelnienie:=True,
                                                          przesuniecieX:=_deltaPrzeciaganiaStylu.X,
                                                          przesuniecieY:=_deltaPrzeciaganiaStylu.Y)

        Dim staryImg = picPodgladStylu.Image
        picPodgladStylu.Image = powiekszony
        staryImg?.Dispose()

        lblZoomStyl.Text = If(Math.Abs(zoom - 1.0) < 0.01, "1:1", "×" & zoom.ToString("0.0"))
        btnZoomMinusStyl.Enabled = (_indeksZoomuStylu > 0)
        btnZoomPlusStyl.Enabled = (_indeksZoomuStylu < _poziomyZoomu.Length - 1)
        btnZoomResetStyl.Enabled = (_indeksZoomuStylu <> IndeksZoomu1do1 OrElse _wlasnyPunktStylu.HasValue)
    End Sub

    Private Sub picPodgladStylu_MouseEnter(sender As Object, e As EventArgs) Handles picPodgladStylu.MouseEnter
        PrzejmijFokusDlaKolka(picPodgladStylu)
        picPodgladStylu.Cursor = Cursors.Hand
    End Sub

    ''' <summary>
    ''' Fokus dla kółka myszy na podglądzie - ale nie zabiera go z pola edycji: dawniej przesunięcie myszy nad podgląd
    ''' w trakcie wpisywania nazwy miejscowości przenosiło dalsze znaki do podglądu.
    ''' </summary>
    Private Sub PrzejmijFokusDlaKolka(podglad As Control)
        Dim aktywna = ActiveControl
        While TypeOf aktywna Is ContainerControl AndAlso DirectCast(aktywna, ContainerControl).ActiveControl IsNot Nothing
            aktywna = DirectCast(aktywna, ContainerControl).ActiveControl
        End While
        If TypeOf aktywna Is TextBoxBase OrElse TypeOf aktywna Is UpDownBase OrElse TypeOf aktywna?.Parent Is UpDownBase OrElse
           TypeOf aktywna Is ComboBox Then Exit Sub
        podglad.Focus()
    End Sub

    Private Sub picPodgladStylu_MouseLeave(sender As Object, e As EventArgs) Handles picPodgladStylu.MouseLeave
        If Not _przeciaganieStylu Then picPodgladStylu.Cursor = Cursors.Default
    End Sub

    Private Sub picPodgladStylu_MouseDown(sender As Object, e As MouseEventArgs) Handles picPodgladStylu.MouseDown
        If e.Button = MouseButtons.Left AndAlso _bazaPodgladuStylu IsNot Nothing Then
            _przeciaganieStylu = True
            _punktStartStylu = e.Location
            _bazaPrzeciaganiaStylu = _deltaPrzeciaganiaStylu
            picPodgladStylu.Cursor = Cursors.SizeAll
        End If
    End Sub

    Private Sub picPodgladStylu_MouseMove(sender As Object, e As MouseEventArgs) Handles picPodgladStylu.MouseMove
        If _przeciaganieStylu AndAlso _bazaPodgladuStylu IsNot Nothing Then
            _deltaPrzeciaganiaStylu = New Point(_bazaPrzeciaganiaStylu.X + (e.X - _punktStartStylu.X),
                                                _bazaPrzeciaganiaStylu.Y + (e.Y - _punktStartStylu.Y))
            ZastosujZoomStylu()
        End If
    End Sub

    Private Sub picPodgladStylu_MouseUp(sender As Object, e As MouseEventArgs) Handles picPodgladStylu.MouseUp
        If _przeciaganieStylu Then
            _przeciaganieStylu = False
            picPodgladStylu.Cursor = Cursors.Hand

            Dim dx = e.X - _punktStartStylu.X
            Dim dy = e.Y - _punktStartStylu.Y

            If Math.Abs(dx) > 2 OrElse Math.Abs(dy) > 2 Then
                Dim param = PobierzParametryProbkiStylu()
                Dim effPiksel = param.Item2
                Dim skalaWyswietlania = SkalaWypelnienia(_bazaPodgladuStylu, picPodgladStylu)
                Dim effPikselEkranu = effPiksel / Math.Max(0.001, skalaWyswietlania)
                Dim starySrodek = param.Item1
                Dim nowyX = starySrodek.X + dy * effPikselEkranu
                Dim nowyY = starySrodek.Y - dx * effPikselEkranu
                _wlasnyPunktStylu = New PunktXY(nowyX, nowyY)
                btnZoomResetStyl.Enabled = True

                ' Nie zerujemy _deltaPrzeciaganiaStylu - przesunięty obraz pozostaje na ekranie
                ' aż do chwili pobrania nowej próbki terenu przez OdswiezPodgladStyluAsync
                OdswiezPodgladStyluAsync(wymus:=True)
            Else
                ZastosujZoomStylu()
            End If
        End If
    End Sub

    Private Sub picPodgladStylu_MouseWheel(sender As Object, e As MouseEventArgs) Handles picPodgladStylu.MouseWheel
        If _bazaPodgladuStylu Is Nothing Then Exit Sub
        If e.Delta > 0 Then
            If _indeksZoomuStylu < _poziomyZoomu.Length - 1 Then
                _indeksZoomuStylu += 1
                ZastosujZoomStylu()
                OdswiezPodgladStyluAsync(wymus:=True)
            End If
        ElseIf e.Delta < 0 Then
            If _indeksZoomuStylu > 0 Then
                _indeksZoomuStylu -= 1
                ZastosujZoomStylu()
                OdswiezPodgladStyluAsync(wymus:=True)
            End If
        End If
    End Sub

    Private Sub picPodgladStylu_Paint(sender As Object, e As PaintEventArgs) Handles picPodgladStylu.Paint
        If picPodgladStylu.Image Is Nothing Then
            RysujPustyPodglad(e.Graphics, picPodgladStylu.ClientRectangle, "Podgląd stylu mapy",
                             If(String.IsNullOrEmpty(lblStanPodgladuStyl.Text), "Wybierz styl z listy po lewej", lblStanPodgladuStyl.Text))
        ElseIf _zasiegObrazuStylu.HasValue AndAlso _wybranyZasieg.Poprawny AndAlso _bazaPodgladuStylu IsNot Nothing Then
            Dim z = _zasiegObrazuStylu.Value
            Dim skala = SkalaWypelnienia(_bazaPodgladuStylu, picPodgladStylu)
            Dim w = CInt(Math.Ceiling(_bazaPodgladuStylu.Width * skala))
            Dim h = CInt(Math.Ceiling(_bazaPodgladuStylu.Height * skala))
            Dim x = (picPodgladStylu.Width - w) \ 2 + _deltaPrzeciaganiaStylu.X
            Dim y = (picPodgladStylu.Height - h) \ 2 + _deltaPrzeciaganiaStylu.Y
            RysujZasiegNaPodgladzie(e.Graphics, z, New Rectangle(x, y, w, h))
        End If
    End Sub

    Private Sub btnZoomPlusStyl_Click(sender As Object, e As EventArgs) Handles btnZoomPlusStyl.Click
        If _indeksZoomuStylu < _poziomyZoomu.Length - 1 Then
            _indeksZoomuStylu += 1
            ZastosujZoomStylu()
            OdswiezPodgladStyluAsync(wymus:=True)
        End If
    End Sub

    Private Sub btnZoomMinusStyl_Click(sender As Object, e As EventArgs) Handles btnZoomMinusStyl.Click
        If _indeksZoomuStylu > 0 Then
            _indeksZoomuStylu -= 1
            ZastosujZoomStylu()
            OdswiezPodgladStyluAsync(wymus:=True)
        End If
    End Sub

    Private Sub btnZoomResetStyl_Click(sender As Object, e As EventArgs) Handles btnZoomResetStyl.Click
        _wlasnyPunktStylu = Nothing
        _bazaPrzeciaganiaStylu = Point.Empty
        _deltaPrzeciaganiaStylu = Point.Empty
        _indeksZoomuStylu = IndeksZoomu1do1
        ZastosujZoomStylu()
        OdswiezPodgladStyluAsync(wymus:=True)
    End Sub

    ''' <summary>Okno dużego podglądu z tym samym zestawem, punktem i pikselem co mały podgląd stylu.</summary>
    Private Sub PokazDuzyPodglad()
        If _styl.Szablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub
        Dim zestaw = _styl.UtworzZestaw(_pikselPodgladuStylu, _stopienPodgladu)
        If zestaw.Liczba = 0 Then Exit Sub
        Dim parametry = PobierzParametryProbkiStylu()
        Dim tytul = _styl.Szablon.Nazwa & If(_stopienPodgladu IsNot Nothing, " - skala " & _stopienPodgladu.Opis, "")
        Using okno As New FormPodgladDuzy(tytul, zestaw, parametry.Item1, parametry.Item2, _styl.ZrodlaMap(),
                                          _wybranyZasieg, PobierzEfektywnyKorytarz())
            okno.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnOdswiezStyl_Click(sender As Object, e As EventArgs) Handles btnOdswiezStyl.Click
        _bazaPrzeciaganiaStylu = Point.Empty
        _deltaPrzeciaganiaStylu = Point.Empty
        OdswiezPodgladStyluAsync(wymus:=True)
    End Sub

#End Region

#Region "Krok 3: Wybór obszaru"

    Private Sub txtSzukajMiejsca_Enter(sender As Object, e As EventArgs) Handles txtSzukajMiejsca.Enter, txtSzukajMiejsca.Click
        rbObszarMiejscowosc.Checked = True
    End Sub

    Private Sub txtSzukajMiejsca_TextChanged(sender As Object, e As EventArgs) Handles txtSzukajMiejsca.TextChanged
        rbObszarMiejscowosc.Checked = True
        _timerWyszukiwania.Stop()
        Dim fraza = txtSzukajMiejsca.Text.Trim()
        If fraza.Length >= 3 Then
            _timerWyszukiwania.Start()
        ElseIf fraza.Length = 0 Then
            _ctsWyszukiwania?.Cancel()
            lstWynikiMiejscowosci.Items.Clear()
            lblWynikMiejscowosci.Text = "Wpisz nazwę miejscowości lub adres..."
            lblWynikMiejscowosci.ForeColor = Color.FromArgb(70, 70, 70)
        End If
    End Sub

    Private Sub TimerWyszukiwania_Tick(sender As Object, e As EventArgs)
        _timerWyszukiwania.Stop()
        Dim fraza = txtSzukajMiejsca.Text.Trim()
        If fraza.Length >= 3 Then
            WyszukajMiejscowosciAsync(fraza)
        End If
    End Sub

    Private Sub txtSzukajMiejsca_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSzukajMiejsca.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            _timerWyszukiwania.Stop()
            Dim fraza = txtSzukajMiejsca.Text.Trim()
            If fraza.Length >= 2 Then
                WyszukajMiejscowosciAsync(fraza)
            End If
        ElseIf e.KeyCode = Keys.Down AndAlso lstWynikiMiejscowosci.Items.Count > 0 Then
            lstWynikiMiejscowosci.Focus()
            If lstWynikiMiejscowosci.SelectedIndex < 0 Then
                lstWynikiMiejscowosci.SelectedIndex = 0
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub btnSzukajMiejsca_Click(sender As Object, e As EventArgs) Handles btnSzukajMiejsca.Click
        _timerWyszukiwania.Stop()
        Dim fraza = txtSzukajMiejsca.Text.Trim()
        If String.IsNullOrWhiteSpace(fraza) Then Exit Sub
        WyszukajMiejscowosciAsync(fraza)
    End Sub

    Private Async Sub WyszukajMiejscowosciAsync(fraza As String)
        _ctsWyszukiwania?.Cancel()
        _ctsWyszukiwania?.Dispose()
        Dim cts = New CancellationTokenSource()
        _ctsWyszukiwania = cts
        Dim token = cts.Token

        ' Wklejone współrzędne - punkt bez pytania serwera wyszukiwania; działka - ULDK; reszta - wyszukiwarka miejsc
        Dim zapytanie = ObszarKreatora.RozpoznajZapytanie(fraza)
        If zapytanie.Punkt IsNot Nothing Then
            If _ctsWyszukiwania Is cts Then _ctsWyszukiwania = Nothing
            cts.Dispose()
            lstWynikiMiejscowosci.Items.Clear()
            lstWynikiMiejscowosci.Items.Add(zapytanie.Punkt)
            lstWynikiMiejscowosci.SelectedIndex = 0
            Exit Sub
        End If

        btnSzukajMiejsca.Enabled = False
        lblWynikMiejscowosci.Text = "Szukanie: " & fraza & "..."
        lblWynikMiejscowosci.ForeColor = Color.FromArgb(60, 60, 60)

        Try
            Dim wyniki As List(Of WynikGeokodowania)
            Dim dzialka = zapytanie.Rodzaj = RodzajZapytania.Dzialka
            If dzialka Then
                ' Działka ewidencyjna z ULDK GUGiK - obszar to jej zasięg z pasem sąsiedztwa
                Dim dzialki = Await KlientUldk.SzukajDzialekAsync(zapytanie.Tekst, token)
                wyniki = dzialki.Select(Function(d) CType(WynikDzialki.Z(d), WynikGeokodowania)).ToList()
            Else
                wyniki = Await KlientGeokodowania.SzukajMiejscaAsync(fraza, token)
            End If
            If token.IsCancellationRequested OrElse IsDisposed Then Exit Sub

            lstWynikiMiejscowosci.BeginUpdate()
            lstWynikiMiejscowosci.Items.Clear()
            If wyniki IsNot Nothing AndAlso wyniki.Count > 0 Then
                For Each m In wyniki
                    lstWynikiMiejscowosci.Items.Add(m)
                Next
                lstWynikiMiejscowosci.EndUpdate()

                ' Domyślnie zaznacz pierwszy wynik
                lstWynikiMiejscowosci.SelectedIndex = 0
            Else
                lstWynikiMiejscowosci.EndUpdate()
                lblWynikMiejscowosci.Text = If(dzialka, "Usługa ULDK nie znalazła takiej działki.",
                                               "Nie znaleziono pasujących miejscowości dla podanej frazy.")
                lblWynikMiejscowosci.ForeColor = Color.FromArgb(180, 50, 0)
            End If
        Catch ex As OperationCanceledException
        Catch ex As ObjectDisposedException
        Catch ex As Exception
            If IsDisposed Then Exit Sub
            lblWynikMiejscowosci.Text = "Błąd wyszukiwania: " & OpisBledu.Z(ex)
            lblWynikMiejscowosci.ForeColor = Color.Red
        Finally
            If _ctsWyszukiwania Is cts Then _ctsWyszukiwania = Nothing
            cts.Dispose()
            If Not IsDisposed AndAlso _ctsWyszukiwania Is Nothing Then btnSzukajMiejsca.Enabled = True
        End Try
    End Sub

    Private Sub lstWynikiMiejscowosci_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstWynikiMiejscowosci.DrawItem
        If e.Index < 0 OrElse e.Index >= lstWynikiMiejscowosci.Items.Count Then Exit Sub
        Dim m = TryCast(lstWynikiMiejscowosci.Items(e.Index), WynikGeokodowania)
        If m Is Nothing Then Exit Sub

        Dim wybrany = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim tlo = If(wybrany, Color.FromArgb(225, 238, 255), Color.White)
        Dim ramka = If(wybrany, Color.FromArgb(0, 120, 215), Color.Transparent)

        Using bTlo As New SolidBrush(tlo)
            e.Graphics.FillRectangle(bTlo, e.Bounds)
        End Using

        If wybrany Then
            Using pRamka As New Pen(ramka, 1.0!)
                e.Graphics.DrawRectangle(pRamka, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1)
            End Using
        End If

        ' Linia 1: Nazwa miejscowości + typ
        Using fNazwa As New Font("Segoe UI", 9.0!, FontStyle.Bold)
            Dim naglowek = m.KrotkaNazwa & " (" & m.TypOpis & ")"
            TextRenderer.DrawText(e.Graphics, naglowek, fNazwa, New Rectangle(e.Bounds.X + 6, e.Bounds.Y + 2, e.Bounds.Width - 10, 17),
                                  If(wybrany, Color.FromArgb(0, 70, 160), Color.FromArgb(20, 20, 20)), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End Using

        ' Linia 2: Ścieżka administracyjna (gmina, powiat, województwo)
        Using fLokalizacja As New Font("Segoe UI", 8.0!, FontStyle.Regular)
            TextRenderer.DrawText(e.Graphics, ObszarKreatora.SformatujLokalizacje(m), fLokalizacja, New Rectangle(e.Bounds.X + 8, e.Bounds.Y + 18, e.Bounds.Width - 12, 15),
                                  Color.FromArgb(85, 85, 85), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End Using
    End Sub

    ' Każdy sposób wyboru obszaru używa wyłącznie własnego zasięgu - dawniej przełączenie na „Miejscowość” bez wyniku
    ' (albo anulowany wybór trasy) zostawiało zasięg z mapy i kreator pobierał obszar, którego użytkownik już nie wybierał

    Private Sub rbObszarMapa_CheckedChanged(sender As Object, e As EventArgs) Handles rbObszarMapa.CheckedChanged
        If rbObszarMapa.Checked Then
            _wlasnyPunktObszaru = Nothing
            _deltaPrzeciaganiaObszaru = Point.Empty
            If Not _wybranyZasiegZMapy.Poprawny AndAlso _form1 IsNot Nothing Then
                Dim z = ZasiegZOknaGlownego()
                If z.Poprawny Then
                    _wybranyZasiegZMapy = z
                    lblStatusZaznaczenia.Text = "Aktualny zasięg z mapy: " & ObszarKreatora.FormatujWymiary(z)
                    lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                End If
            End If
            _wybranyZasieg = _wybranyZasiegZMapy
            AktualizujPodgladObszaru()
        End If
    End Sub

    Private Function ZasiegZOknaGlownego() As Zasieg
        Dim zasieg = _form1.ZasiegPolXY()
        Dim uklad = Program.Ustawienia.Uklad
        If Not zasieg.Poprawny OrElse uklad.Epsg = UkladWspolrzednych.PL1992.Epsg Then Return zasieg
        Dim narozniki = {zasieg.LewyGorny, zasieg.PrawyGorny, zasieg.LewyDolny, zasieg.PrawyDolny}
        Return EksportMapy.ObszarPunktow(UkladWspolrzednych.PL1992, narozniki.Select(Function(p) uklad.DoWgs84(p)))
    End Function

    Private Sub rbObszarMiejscowosc_CheckedChanged(sender As Object, e As EventArgs) Handles rbObszarMiejscowosc.CheckedChanged
        If rbObszarMiejscowosc.Checked Then
            _wlasnyPunktObszaru = Nothing
            _deltaPrzeciaganiaObszaru = Point.Empty
            If Not _wybranyZasiegMiejscowosci.Poprawny AndAlso lstWynikiMiejscowosci.SelectedItem IsNot Nothing Then
                lstWynikiMiejscowosci_SelectedIndexChanged(lstWynikiMiejscowosci, EventArgs.Empty)
                Exit Sub
            End If
            _wybranyZasieg = _wybranyZasiegMiejscowosci
            AktualizujPodgladObszaru()
        End If
    End Sub

    Private Sub rbObszarTrasa_CheckedChanged(sender As Object, e As EventArgs) Handles rbObszarTrasa.CheckedChanged
        If rbObszarTrasa.Checked Then
            _wlasnyPunktObszaru = Nothing
            _deltaPrzeciaganiaObszaru = Point.Empty
            _wybranyZasieg = If(_wybranyKorytarz IsNot Nothing, _wybranyZasiegTrasy, New Zasieg())
            AktualizujPodgladObszaru()
            If _wybranyKorytarz Is Nothing AndAlso Visible Then
                ' Okno wyboru pliku po zakończeniu obsługi kliknięcia (nie w trakcie zmiany zaznaczenia)
                BeginInvoke(New Action(Sub() btnWybierzTrase_Click(btnWybierzTrase, EventArgs.Empty)))
            End If
        End If
    End Sub

    Private Sub lstWynikiMiejscowosci_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstWynikiMiejscowosci.SelectedIndexChanged
        Dim m = TryCast(lstWynikiMiejscowosci.SelectedItem, WynikGeokodowania)
        If m Is Nothing Then Exit Sub

        _wlasnyPunktObszaru = Nothing
        _deltaPrzeciaganiaObszaru = Point.Empty
        _ostatnieMiejsce = m
        _sciezkaZmienionaPrzezUzytkownika = False

        _wybranyZasiegMiejscowosci = ZasiegWyniku(m)

        Dim admin = ObszarKreatora.SformatujLokalizacje(m)
        lblWynikMiejscowosci.Text = "Wybrano: " & m.KrotkaNazwa & If(admin <> "", " (" & admin & ")", "") & " - obszar " & ObszarKreatora.FormatujWymiary(_wybranyZasiegMiejscowosci)
        lblWynikMiejscowosci.ForeColor = Color.FromArgb(0, 120, 50)

        If rbObszarMiejscowosc.Checked Then
            _wybranyZasieg = _wybranyZasiegMiejscowosci
            AktualizujPodgladObszaru()
        Else
            rbObszarMiejscowosc.Checked = True
        End If
    End Sub

    ''' <summary>Obszar wybranego wyniku wyszukiwania przy promieniu wybranym na liście (<see cref="ObszarKreatora.ZasiegWyniku"/>).</summary>
    Friend Function ZasiegWyniku(m As WynikGeokodowania) As Zasieg
        Return ObszarKreatora.ZasiegWyniku(m, TryCast(cboPromien.SelectedItem, String))
    End Function

    Private Sub lstWynikiMiejscowosci_DoubleClick(sender As Object, e As EventArgs) Handles lstWynikiMiejscowosci.DoubleClick
        If lstWynikiMiejscowosci.SelectedItem IsNot Nothing Then
            btnDalej.PerformClick()
        End If
    End Sub

    Private Sub lstWynikiMiejscowosci_KeyDown(sender As Object, e As KeyEventArgs) Handles lstWynikiMiejscowosci.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If lstWynikiMiejscowosci.SelectedItem IsNot Nothing Then
                btnDalej.PerformClick()
            End If
        End If
    End Sub

    Private Sub cboPromien_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboPromien.SelectedIndexChanged
        If lstWynikiMiejscowosci.SelectedItem IsNot Nothing Then
            lstWynikiMiejscowosci_SelectedIndexChanged(lstWynikiMiejscowosci, EventArgs.Empty)
        End If
    End Sub

    Private Sub btnZaznaczNaMapie_Click(sender As Object, e As EventArgs) Handles btnZaznaczNaMapie.Click
        If _form1 Is Nothing Then Exit Sub

        ' Aktywuj i odblokuj okno główne przed pokazaniem pływającego paska
        _form1.UstawEdycjeXY(False)
        _form1.Enabled = True
        PrzywrocZMinimalizacji(_form1)
        _form1.Show()
        If Not Form1.TrybOkienBezAktywacji Then
            _form1.Activate()
            _form1.BringToFront()
            SetForegroundWindow(_form1.Handle)
        End If

        Me.Hide()

        Dim pasek As New FormPasekZaznaczania(_form1)
        AddHandler pasek.Zatwierdzono, Sub()
                                           Dim z = ZasiegZOknaGlownego()
                                           If z.Poprawny Then
                                               _wlasnyPunktObszaru = Nothing
                                               _wybranyZasiegZMapy = z
                                               lblStatusZaznaczenia.Text = "Zaznaczono na mapie: " & ObszarKreatora.FormatujWymiary(z)
                                               lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                                               If rbObszarMapa.Checked Then
                                                   _wybranyZasieg = z
                                                   AktualizujPodgladObszaru()
                                               Else
                                                   rbObszarMapa.Checked = True
                                               End If
                                           End If
                                       End Sub
        AddHandler pasek.FormClosed, Sub()
                                         Try
                                             If Me.IsDisposed Then
                                                 If _form1 IsNot Nothing AndAlso Not _form1.IsDisposed Then
                                                     _form1.Enabled = True
                                                     If Not Form1.TrybOkienBezAktywacji Then _form1.Activate()
                                                 End If
                                                 Exit Sub
                                             End If
                                             _form1.Enabled = False ' Przywróć blokadę modalną dla okna kreatora
                                             Me.Visible = True
                                             If Me.WindowState = FormWindowState.Minimized Then
                                                 Me.WindowState = FormWindowState.Normal
                                             End If
                                             If Not Form1.TrybOkienBezAktywacji Then
                                                 Me.BringToFront()
                                                 Me.Activate()
                                                 SetForegroundWindow(Me.Handle)
                                             End If
                                         Catch ex As Exception
                                             If _form1 IsNot Nothing AndAlso Not _form1.IsDisposed Then
                                                 _form1.Enabled = True
                                                 If Not Form1.TrybOkienBezAktywacji Then _form1.Activate()
                                             End If
                                         End Try
                                     End Sub
        pasek.Show(_form1)
    End Sub

    Private Sub btnWybierzTrase_Click(sender As Object, e As EventArgs) Handles btnWybierzTrase.Click
        Using dlg As New OpenFileDialog()
            dlg.Title = "Wskaż plik ze śladem trasy (GPX, KML, KMZ)"
            dlg.Filter = "Pliki tras GPS (*.gpx;*.kml;*.kmz)|*.gpx;*.kml;*.kmz|Pliki GPX (*.gpx)|*.gpx|Pliki KML/KMZ (*.kml;*.kmz)|*.kml;*.kmz|Wszystkie pliki (*.*)|*.*"
            If dlg.ShowDialog(Me) = DialogResult.OK Then WczytajPlikTrasy(dlg.FileName)
        End Using
    End Sub

    ''' <summary>Wczytuje ślad trasy z pliku GPX/KML/KMZ jako obszar mapy (przycisk albo plik upuszczony na okno).</summary>
    Friend Sub WczytajPlikTrasy(sciezka As String)
        _wlasnyPunktObszaru = Nothing
        _deltaPrzeciaganiaObszaru = Point.Empty
        Try
            Dim k As Korytarz = Korytarz.WczytajTrase(sciezka)
            If k IsNot Nothing AndAlso k.Poprawny Then
                k.SzerokoscMetry = CDbl(numKorytarz.Value)
                _wybranyKorytarz = k
                _wybranyZasiegTrasy = k.Zasieg(UkladWspolrzednych.PL1992)
                _plikTrasy = sciezka
                btnProfilTrasy.Enabled = True
                _ostatnieMiejsce = Nothing
                _sciezkaZmienionaPrzezUzytkownika = False
                Dim nazwaTrasy = If(Not String.IsNullOrEmpty(k.Nazwa), k.Nazwa, Path.GetFileNameWithoutExtension(sciezka))
                If String.IsNullOrEmpty(k.Nazwa) Then k.Nazwa = nazwaTrasy
                lblStatusTrasy.Text = $"Wczytano trasę: {nazwaTrasy} ({k.DlugoscKm:0.0} km, {k.LiczbaPunktow} pkt), pas {numKorytarz.Value} m"
                lblStatusTrasy.ForeColor = Color.FromArgb(0, 120, 50)
                If rbObszarTrasa.Checked Then
                    _wybranyZasieg = _wybranyZasiegTrasy
                    AktualizujPodgladObszaru()
                Else
                    rbObszarTrasa.Checked = True
                End If
            Else
                lblStatusTrasy.Text = "Plik nie zawiera poprawnego śladu ani linii trasy."
                lblStatusTrasy.ForeColor = Color.Red
            End If
        Catch ex As Exception
            lblStatusTrasy.Text = "Błąd odczytu trasy: " & OpisBledu.Z(ex)
            lblStatusTrasy.ForeColor = Color.Red
        End Try
    End Sub

    Private Sub btnProfilTrasy_Click(sender As Object, e As EventArgs)
        If Not File.Exists(_plikTrasy) Then Exit Sub
        Using okno As New FormProfilNmt(_plikTrasy)
            okno.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>Ścieżka jednego pliku GPX/KML/KMZ przeciąganego na okno (Nothing - inne dane).</summary>
    Friend Shared Function PlikTrasyZUpuszczenia(dane As IDataObject) As String
        If dane Is Nothing OrElse Not dane.GetDataPresent(DataFormats.FileDrop) Then Return Nothing
        Dim pliki = TryCast(dane.GetData(DataFormats.FileDrop), String())
        If pliki Is Nothing OrElse pliki.Length <> 1 Then Return Nothing
        Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
        Return If(ext = ".gpx" OrElse ext = ".kml" OrElse ext = ".kmz", pliki(0), Nothing)
    End Function

    ' Upuszczenie działa tylko w kroku 2 (wybór obszaru) - w innych krokach plik niczego nie zmienia
    Protected Overrides Sub OnDragEnter(e As DragEventArgs)
        MyBase.OnDragEnter(e)
        e.Effect = If(_biezacyKrok = 2 AndAlso Not _trwaPraca AndAlso PlikTrasyZUpuszczenia(e.Data) IsNot Nothing, DragDropEffects.Copy, DragDropEffects.None)
    End Sub

    Protected Overrides Sub OnDragDrop(e As DragEventArgs)
        MyBase.OnDragDrop(e)
        Dim plik = PlikTrasyZUpuszczenia(e.Data)
        If _biezacyKrok = 2 AndAlso plik IsNot Nothing Then WczytajPlikTrasy(plik)
    End Sub

    ''' <summary>Upuszczenie pliku na dowolną kontrolkę okna obsługuje formularz.</summary>
    Private Sub WlaczUpuszczanie(kontrolka As Control)
        kontrolka.AllowDrop = True
        If kontrolka IsNot Me Then
            AddHandler kontrolka.DragEnter, Sub(s, e) OnDragEnter(e)
            AddHandler kontrolka.DragDrop, Sub(s, e) OnDragDrop(e)
        End If
        For Each c As Control In kontrolka.Controls
            WlaczUpuszczanie(c)
        Next
    End Sub

    Private Sub numKorytarz_ValueChanged(sender As Object, e As EventArgs) Handles numKorytarz.ValueChanged
        If _wybranyKorytarz IsNot Nothing Then
            _wybranyKorytarz.SzerokoscMetry = CDbl(numKorytarz.Value)
            _wybranyZasiegTrasy = _wybranyKorytarz.Zasieg(UkladWspolrzednych.PL1992)
            lblStatusTrasy.Text = $"Korytarz: pas {numKorytarz.Value} m po obu stronach trasy - zasięg {ObszarKreatora.FormatujWymiary(_wybranyZasiegTrasy)}"
            If rbObszarTrasa.Checked Then
                _wybranyZasieg = _wybranyZasiegTrasy
                AktualizujPodgladObszaru()
            End If
        End If
    End Sub

    ''' <summary>Obszar podglądu: wybrany przez użytkownika albo przykładowy obszar stylu (Item2 = True).</summary>
    Private Function PobierzEfektywnyZasiegObszaru() As Tuple(Of Zasieg, Boolean)
        If _wybranyZasieg.Poprawny Then Return Tuple.Create(_wybranyZasieg, False)
        Return Tuple.Create(_styl.WzorcowyZasieg(), True)
    End Function

    Private Sub AktualizujPodgladObszaru()
        Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
        Dim zasieg = infoZasiegu.Item1
        Dim czyWzorcowy = infoZasiegu.Item2

        If zasieg.Poprawny Then
            Dim opis = ObszarKreatora.OpisObszaru(zasieg, czyWzorcowy, PobierzEfektywnyKorytarz())
            lblObszarWymiary.Text = opis.Item1
            lblObszarWspolrzedne.Text = opis.Item2
            AktualizujPodgladObszaruAsync()
        Else
            lblObszarWymiary.Text = "Wymiary: brak"
            lblObszarWspolrzedne.Text = "Współrzędne: -"
            Dim stary = picPodgladObszaru.Image
            picPodgladObszaru.Image = Nothing
            stary?.Dispose()
            lblStanPodgladuObszar.Text = "Brak obszaru"
        End If
    End Sub

    ''' <summary>
    ''' Piksel podglądu obszaru: cały obszar mieści się w ramce podglądu z marginesem. Dawniej piksel wynikał z boku próbki
    ''' (512 px), a ramka ma 240 × 210 px i pokazuje próbkę w skali 1:1 - widać było tylko środek obszaru.
    ''' </summary>
    Friend Function PikselPodgladuObszaru(zasieg As Zasieg) As Double
        Return ObszarKreatora.PikselPodgladu(zasieg, Math.Min(picPodgladObszaru.Width, _silnikPodgladuObszaru.Bok),
                                             Math.Min(picPodgladObszaru.Height, _silnikPodgladuObszaru.Bok),
                                             If(_styl.Szablon IsNot Nothing, _styl.Szablon.DomyslnyPiksel, 1.0), _poziomyZoomu(_indeksZoomuObszaru))
    End Function

    Private Async Sub AktualizujPodgladObszaruAsync(Optional wymus As Boolean = False)
        Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
        Dim zasieg = infoZasiegu.Item1
        Dim czyWzorcowy = infoZasiegu.Item2

        If Not zasieg.Poprawny OrElse _styl.Szablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        _ctsPodgladObszaru?.Cancel()
        _ctsPodgladObszaru?.Dispose()
        _ctsPodgladObszaru = New CancellationTokenSource()
        Dim token = _ctsPodgladObszaru.Token

        lblStanPodgladuObszar.Text = If(czyWzorcowy, "Pobieranie przykładu...", "Pobieranie mapy obszaru...")
        lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 100, 180)

        Dim punkt = If(_wlasnyPunktObszaru, zasieg.Srodek)
        Dim piksel = PikselPodgladuObszaru(zasieg)
        ' Styl wieloskalowy: duży obszar w podglądzie pokazuje mapę w mniejszej skali, jak plik MBTiles po oddaleniu
        Dim zestaw = _styl.UtworzZestaw(piksel)
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Exit Sub
        Dim zmieniono = _silnikPodgladuObszaru.SprawdzIZaktualizujParametry(punkt, piksel, UkladWspolrzednych.PL1992, wymus)

        If Not zmieniono AndAlso zestaw.Warstwy.All(Function(w) _silnikPodgladuObszaru.CzyWarstwaWBuforze(w)) Then
            Dim staraBaza = _bazaPodgladuObszaru
            _bazaPodgladuObszaru = _silnikPodgladuObszaru.ZlozObraz(zestaw)
            _zasiegObrazuObszaru = _silnikPodgladuObszaru.ZasiegProbki()
            staraBaza?.Dispose()
            _deltaPrzeciaganiaObszaru = Point.Empty
            _bazaPrzeciaganiaObszaru = Point.Empty
            ZastosujZoomObszaru()
            lblStanPodgladuObszar.Text = If(czyWzorcowy, "Przykład stylu", "Podgląd gotowy")
            lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 130, 40)
            Exit Sub
        End If

        If Form1.TrybTestowyBezSieci Then
            lblStanPodgladuObszar.Text = "Podgląd offline (tryb testowy)"
            lblStanPodgladuObszar.ForeColor = Color.FromArgb(120, 120, 120)
            Exit Sub
        End If

        Try
            Dim zasiegDoPobrania = _silnikPodgladuObszaru.ZasiegProbki().GetValueOrDefault(zasieg)
            Dim siatkaObszaru As New Siatka(zasiegDoPobrania, piksel, _silnikPodgladuObszaru.Bok)
            Dim zadanie As New ZadaniePobierania With {
                .Uklad = UkladWspolrzednych.PL1992,
                .Siatka = siatkaObszaru,
                .Zbiory = zestaw.Zbiory(),
                .ZrodlaMap = _styl.ZrodlaMap(),
                .Format = "png",
                .LimitCzasuSekundy = 15,
                .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_obszar") & "\"
            }

            Dim postep As New Progress(Of String)(Sub(msg)
                                                      If Not token.IsCancellationRequested Then
                                                          lblStanPodgladuObszar.Text = msg
                                                          lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 100, 180)
                                                      End If
                                                  End Sub)

            Dim wynik = Await _silnikPodgladuObszaru.PobierzPodgladAsync(zadanie, zestaw, token, postep)
            If token.IsCancellationRequested OrElse IsDisposed Then
                wynik.Item1?.Dispose()
                Exit Sub
            End If

            Dim staraBaza = _bazaPodgladuObszaru
            _bazaPodgladuObszaru = wynik.Item1
            _zasiegObrazuObszaru = zasiegDoPobrania
            staraBaza?.Dispose()

            _deltaPrzeciaganiaObszaru = Point.Empty
            _bazaPrzeciaganiaObszaru = Point.Empty
            ZastosujZoomObszaru()

            If _bazaPodgladuObszaru IsNot Nothing Then
                lblStanPodgladuObszar.Text = If(czyWzorcowy, "Przykład stylu", "Podgląd gotowy")
                lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 130, 40)
            Else
                lblStanPodgladuObszar.Text = "Brak podglądu"
                lblStanPodgladuObszar.ForeColor = Color.FromArgb(180, 50, 0)
            End If
        Catch ex As OperationCanceledException
        Catch ex As ObjectDisposedException
        Catch ex As Exception
            If IsDisposed Then Exit Sub
            lblStanPodgladuObszar.Text = "Błąd pobierania: " & OpisBledu.Z(ex)
            lblStanPodgladuObszar.ForeColor = Color.Red
        End Try
    End Sub

    Private Sub ZastosujZoomObszaru()
        If _bazaPodgladuObszaru Is Nothing Then
            Dim stary = picPodgladObszaru.Image
            picPodgladObszaru.Image = Nothing
            stary?.Dispose()
            lblZoomObszar.Text = "1:1"
            Exit Sub
        End If

        Dim zoom = _poziomyZoomu(_indeksZoomuObszaru)
        Dim powiekszony = PodgladWarstwSilnik.ZastosujZoom(_bazaPodgladuObszaru, 1.0,
                                                          picPodgladObszaru.Width, picPodgladObszaru.Height,
                                                          dopasujWypelnienie:=True,
                                                          przesuniecieX:=_deltaPrzeciaganiaObszaru.X,
                                                          przesuniecieY:=_deltaPrzeciaganiaObszaru.Y)

        Dim staryImg = picPodgladObszaru.Image
        picPodgladObszaru.Image = powiekszony
        staryImg?.Dispose()

        lblZoomObszar.Text = If(Math.Abs(zoom - 1.0) < 0.01, "1:1", "×" & zoom.ToString("0.0"))
        btnZoomMinusObszar.Enabled = (_indeksZoomuObszaru > 0)
        btnZoomPlusObszar.Enabled = (_indeksZoomuObszaru < _poziomyZoomu.Length - 1)
        btnZoomResetObszar.Enabled = (_indeksZoomuObszaru <> IndeksZoomu1do1 OrElse _wlasnyPunktObszaru.HasValue)
    End Sub

    Private Sub picPodgladObszaru_MouseEnter(sender As Object, e As EventArgs) Handles picPodgladObszaru.MouseEnter
        PrzejmijFokusDlaKolka(picPodgladObszaru)
        picPodgladObszaru.Cursor = Cursors.Hand
    End Sub

    Private Sub picPodgladObszaru_MouseLeave(sender As Object, e As EventArgs) Handles picPodgladObszaru.MouseLeave
        If Not _przeciaganieObszaru Then picPodgladObszaru.Cursor = Cursors.Default
    End Sub

    Private Sub picPodgladObszaru_MouseDown(sender As Object, e As MouseEventArgs) Handles picPodgladObszaru.MouseDown
        If e.Button = MouseButtons.Left AndAlso _bazaPodgladuObszaru IsNot Nothing Then
            _przeciaganieObszaru = True
            _punktStartObszaru = e.Location
            _bazaPrzeciaganiaObszaru = _deltaPrzeciaganiaObszaru
            picPodgladObszaru.Cursor = Cursors.SizeAll
        End If
    End Sub

    Private Sub picPodgladObszaru_MouseMove(sender As Object, e As MouseEventArgs) Handles picPodgladObszaru.MouseMove
        If _przeciaganieObszaru AndAlso _bazaPodgladuObszaru IsNot Nothing Then
            _deltaPrzeciaganiaObszaru = New Point(_bazaPrzeciaganiaObszaru.X + (e.X - _punktStartObszaru.X),
                                                 _bazaPrzeciaganiaObszaru.Y + (e.Y - _punktStartObszaru.Y))
            ZastosujZoomObszaru()
        End If
    End Sub

    Private Sub picPodgladObszaru_MouseUp(sender As Object, e As MouseEventArgs) Handles picPodgladObszaru.MouseUp
        If _przeciaganieObszaru Then
            _przeciaganieObszaru = False
            picPodgladObszaru.Cursor = Cursors.Hand

            Dim dx = e.X - _punktStartObszaru.X
            Dim dy = e.Y - _punktStartObszaru.Y

            If (Math.Abs(dx) > 2 OrElse Math.Abs(dy) > 2) AndAlso _styl.Szablon IsNot Nothing Then
                Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
                Dim zasieg = infoZasiegu.Item1
                Dim effPiksel = PikselPodgladuObszaru(zasieg)
                Dim skalaWyswietlania = SkalaWypelnienia(_bazaPodgladuObszaru, picPodgladObszaru)
                Dim effPikselEkranu = effPiksel / Math.Max(0.001, skalaWyswietlania)
                Dim starySrodek = If(_wlasnyPunktObszaru, zasieg.Srodek)
                Dim nowyX = starySrodek.X + dy * effPikselEkranu
                Dim nowyY = starySrodek.Y - dx * effPikselEkranu
                _wlasnyPunktObszaru = New PunktXY(nowyX, nowyY)
                btnZoomResetObszar.Enabled = True

                ' Nie zerujemy _deltaPrzeciaganiaObszaru - przesunięty obraz pozostaje na ekranie
                ' aż do chwili pobrania nowej próbki terenu przez AktualizujPodgladObszaruAsync
                AktualizujPodgladObszaruAsync(wymus:=True)
            Else
                ZastosujZoomObszaru()
            End If
        End If
    End Sub

    Private Sub picPodgladObszaru_MouseWheel(sender As Object, e As MouseEventArgs) Handles picPodgladObszaru.MouseWheel
        If _bazaPodgladuObszaru Is Nothing Then Exit Sub
        If e.Delta > 0 Then
            If _indeksZoomuObszaru < _poziomyZoomu.Length - 1 Then
                _indeksZoomuObszaru += 1
                ZastosujZoomObszaru()
                AktualizujPodgladObszaruAsync(wymus:=True)
            End If
        ElseIf e.Delta < 0 Then
            If _indeksZoomuObszaru > 0 Then
                _indeksZoomuObszaru -= 1
                ZastosujZoomObszaru()
                AktualizujPodgladObszaruAsync(wymus:=True)
            End If
        End If
    End Sub

    Private Sub btnZoomPlusObszar_Click(sender As Object, e As EventArgs) Handles btnZoomPlusObszar.Click
        If _indeksZoomuObszaru < _poziomyZoomu.Length - 1 Then
            _indeksZoomuObszaru += 1
            ZastosujZoomObszaru()
            AktualizujPodgladObszaruAsync(wymus:=True)
        End If
    End Sub

    Private Sub btnZoomMinusObszar_Click(sender As Object, e As EventArgs) Handles btnZoomMinusObszar.Click
        If _indeksZoomuObszaru > 0 Then
            _indeksZoomuObszaru -= 1
            ZastosujZoomObszaru()
            AktualizujPodgladObszaruAsync(wymus:=True)
        End If
    End Sub

    Private Sub btnZoomResetObszar_Click(sender As Object, e As EventArgs) Handles btnZoomResetObszar.Click
        _wlasnyPunktObszaru = Nothing
        _bazaPrzeciaganiaObszaru = Point.Empty
        _deltaPrzeciaganiaObszaru = Point.Empty
        _indeksZoomuObszaru = IndeksZoomu1do1
        ZastosujZoomObszaru()
        AktualizujPodgladObszaruAsync(wymus:=True)
    End Sub

    Private Sub picPodgladObszaru_Paint(sender As Object, e As PaintEventArgs) Handles picPodgladObszaru.Paint
        If picPodgladObszaru.Image Is Nothing Then
            RysujPustyPodglad(e.Graphics, picPodgladObszaru.ClientRectangle, "Podgląd wybranego obszaru",
                             If(String.IsNullOrEmpty(lblStanPodgladuObszar.Text), "Zaznacz obszar na mapie lub wyszukaj miejscowość", lblStanPodgladuObszar.Text))
        ElseIf _zasiegObrazuObszaru.HasValue AndAlso _wybranyZasieg.Poprawny AndAlso _bazaPodgladuObszaru IsNot Nothing Then
            ' Obrys i ślad w tym samym przekształceniu co obraz próbki (dawniej ślad był rozciągany na całą ramkę
            ' osobno w poziomie i w pionie, więc nie leżał na mapie)
            Dim skala = SkalaWypelnienia(_bazaPodgladuObszaru, picPodgladObszaru)
            Dim w = CInt(Math.Ceiling(_bazaPodgladuObszaru.Width * skala))
            Dim h = CInt(Math.Ceiling(_bazaPodgladuObszaru.Height * skala))
            Dim x = (picPodgladObszaru.Width - w) \ 2 + _deltaPrzeciaganiaObszaru.X
            Dim y = (picPodgladObszaru.Height - h) \ 2 + _deltaPrzeciaganiaObszaru.Y
            RysujZasiegNaPodgladzie(e.Graphics, _zasiegObrazuObszaru.Value, New Rectangle(x, y, w, h))
        End If
    End Sub

    ''' <summary>Obrys wybranego prostokąta albo pas trasy na obrazie próbki o zasięgu <paramref name="z"/> narysowanym w <paramref name="obraz"/>.</summary>
    Private Sub RysujZasiegNaPodgladzie(g As Graphics, z As Zasieg, obraz As Rectangle)
        Dim punktEkranu = Function(p As PunktXY) New PointF(CSng(obraz.X + (p.Y - z.YLewy) * obraz.Width / z.Szerokosc),
                                                           CSng(obraz.Y + (z.XGora - p.X) * obraz.Height / z.Wysokosc))
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim trasa = PobierzEfektywnyKorytarz()
        If trasa IsNot Nothing Then
            Dim szer = CSng(Math.Max(1, 2 * trasa.SzerokoscMetry * obraz.Width / z.Szerokosc))
            Using pas As New Pen(Color.FromArgb(80, 17, 88, 183), szer) With {
                .StartCap = LineCap.Round, .EndCap = LineCap.Round, .LineJoin = LineJoin.Round},
                pedzel As New SolidBrush(pas.Color),
                os As New Pen(Color.FromArgb(230, 210, 20, 20), 1.5F)
                For Each linia In trasa.Linie
                    Dim punkty = linia.Select(Function(p) punktEkranu(UkladWspolrzednych.PL1992.ZWgs84(p))).ToArray()
                    If punkty.Length > 1 Then
                        g.DrawLines(pas, punkty)
                        g.DrawLines(os, punkty)
                    ElseIf punkty.Length = 1 Then
                        g.FillEllipse(pedzel, punkty(0).X - szer / 2, punkty(0).Y - szer / 2, szer, szer)
                    End If
                Next
            End Using
            Exit Sub
        End If
        Dim lewyGorny = punktEkranu(_wybranyZasieg.LewyGorny)
        Dim prawyDolny = punktEkranu(_wybranyZasieg.PrawyDolny)
        Dim obrys = RectangleF.FromLTRB(lewyGorny.X, lewyGorny.Y, prawyDolny.X, prawyDolny.Y)
        Using tlo As New Pen(Color.White, 3), linia As New Pen(Color.FromArgb(17, 88, 183), 1.5F)
            g.DrawRectangle(tlo, obrys.X, obrys.Y, obrys.Width, obrys.Height)
            g.DrawRectangle(linia, obrys.X, obrys.Y, obrys.Width, obrys.Height)
        End Using
    End Sub

    Private Sub RysujPustyPodglad(g As Graphics, bounds As Rectangle, tytul As String, opis As String)
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Using bTlo As New SolidBrush(Color.FromArgb(248, 250, 253))
            g.FillRectangle(bTlo, bounds)
        End Using

        ' Subtelna siatka kartograficzna w tle
        Using pSiatka As New Pen(Color.FromArgb(230, 235, 242), 1.0!)
            For x = bounds.Left To bounds.Right Step 24
                g.DrawLine(pSiatka, x, bounds.Top, x, bounds.Bottom)
            Next
            For y = bounds.Top To bounds.Bottom Step 24
                g.DrawLine(pSiatka, bounds.Left, y, bounds.Right, y)
            Next
        End Using

        ' Tekst na środku
        Using fTytul As New Font("Segoe UI", 9.5!, FontStyle.Bold)
            Using bTytul As New SolidBrush(Color.FromArgb(70, 90, 120))
                Using sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    Dim rectTytul As New Rectangle(bounds.Left + 10, bounds.Top + bounds.Height \ 2 - 24, bounds.Width - 20, 24)
                    g.DrawString(tytul, fTytul, bTytul, rectTytul, sf)
                End Using
            End Using
        End Using

        Using fOpis As New Font("Segoe UI", 8.0!, FontStyle.Regular)
            Using bOpis As New SolidBrush(Color.FromArgb(120, 130, 145))
                Using sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    Dim rectOpis As New Rectangle(bounds.Left + 10, bounds.Top + bounds.Height \ 2 + 2, bounds.Width - 20, 36)
                    g.DrawString(opis, fOpis, bOpis, rectOpis, sf)
                End Using
            End Using
        End Using
    End Sub

#End Region

#Region "Własne style użytkownika"

    Private Shared ReadOnly Property PlikStylowWlasnych As String
        Get
            Return Path.Combine(If(String.IsNullOrEmpty(FolderDanych), AppDomain.CurrentDomain.BaseDirectory, FolderDanych), SzablonyWlasne.NazwaPliku)
        End Get
    End Property

    ''' <summary>Wczytuje style zapisane przez użytkownika do listy stylów (uszkodzony plik - komunikat, lista gotowych stylów działa dalej).</summary>
    Private Sub WczytajStyleWlasne()
        Try
            ZarzadcaSzablonow.UstawWlasne(SzablonyWlasne.Wczytaj(PlikStylowWlasnych))
        Catch ex As Exception
            ZarzadcaSzablonow.UstawWlasne(Nothing)
            MessageBox.Show("Nie udało się wczytać zapisanych stylów z pliku " & PlikStylowWlasnych & ": " & OpisBledu.Z(ex) & vbCrLf & vbCrLf &
                            "Kreator pokaże tylko gotowe style. Plik nie zostanie zmieniony, dopóki nie zapiszesz nowego stylu.",
                            "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>Styl z bieżącej kompozycji kroku 3; punkt próbki - środek wybranego obszaru.</summary>
    Friend Function UtworzStylWlasny(nazwa As String) As SzablonMapy
        Return _styl.UtworzStylWlasny(nazwa, _wybranyZasieg)
    End Function

    Private Sub btnZapiszStyl_Click(sender As Object, e As EventArgs)
        If _styl.Szablon Is Nothing Then Exit Sub
        Dim proponowana = If(SzablonyWlasne.CzyWlasny(_styl.Szablon), _styl.Szablon.Nazwa, "Mój szablon - " & _styl.Szablon.Nazwa)
        Dim nazwa = Microsoft.VisualBasic.Interaction.InputBox("Nazwa szablonu na liście szablonów kreatora:", "Zapisz jako szablon", proponowana).Trim()
        If nazwa = "" Then Exit Sub
        ZapiszStylWlasny(nazwa, Function(pytanie) MessageBox.Show(pytanie, "Zapisz jako szablon", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes)
    End Sub

    ''' <summary>Zapisuje bieżącą kompozycję jako szablon i zaznacza go na liście; <paramref name="potwierdz"/> - pytanie o zastąpienie szablonu o tej nazwie.</summary>
    Friend Function ZapiszStylWlasny(nazwa As String, potwierdz As Func(Of String, Boolean)) As SzablonMapy
        Dim styl = UtworzStylWlasny(nazwa)
        If styl Is Nothing OrElse (styl.Warstwy.Count = 0 AndAlso styl.StopnieSkali.Count = 0) Then
            MessageBox.Show("Szablon nie zawiera żadnej warstwy - nie ma czego zapisać.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Dim lista = ZarzadcaSzablonow.PobierzWlasne().ToList()
        Dim istniejacy = lista.FindIndex(Function(x) String.Equals(x.Nazwa, nazwa, StringComparison.CurrentCultureIgnoreCase))
        If istniejacy >= 0 Then
            If Not potwierdz("Szablon „" & nazwa & "”” już istnieje. Zastąpić go bieżącą kompozycją?") Then Return Nothing
            lista(istniejacy) = styl
        Else
            lista.Add(styl)
        End If
        Try
            SzablonyWlasne.Zapisz(PlikStylowWlasnych, lista)
        Catch ex As Exception
            MessageBox.Show("Nie udało się zapisać szablonu: " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return Nothing
        End Try
        ZarzadcaSzablonow.UstawWlasne(lista)
        ' zapisany szablon zastępuje kompozycję - zaznaczamy go na liście (ustawienia są już w jego warstwach)
        _styl.Szablon = Nothing
        _ostatniSzablonId = styl.Id
        OdswiezListeSzablonow()
        Return styl
    End Function

    Private Sub btnUsunStyl_Click(sender As Object, e As EventArgs)
        Dim styl = _styl.Szablon
        If Not SzablonyWlasne.CzyWlasny(styl) Then Exit Sub
        If MessageBox.Show("Usunąć szablon „" & styl.Nazwa & "”” z listy? Tej operacji nie można cofnąć.", "Usuń szablon",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        UsunStylWlasny(styl)
    End Sub

    Friend Sub UsunStylWlasny(styl As SzablonMapy)
        Dim lista = ZarzadcaSzablonow.PobierzWlasne().Where(Function(x) x.Id <> styl.Id).ToList()
        Try
            SzablonyWlasne.Zapisz(PlikStylowWlasnych, lista)
        Catch ex As Exception
            MessageBox.Show("Nie udało się usunąć szablonu: " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End Try
        ZarzadcaSzablonow.UstawWlasne(lista)
        _styl.Szablon = Nothing
        _ostatniSzablonId = ""
        OdswiezListeSzablonow()
    End Sub

#End Region

#Region "Krok 4: Zapis i opcje zaawansowane"

    Private Sub btnPrzegladajDocelowy_Click(sender As Object, e As EventArgs) Handles btnPrzegladajDocelowy.Click
        Using dlg As New SaveFileDialog()
            dlg.Title = "Wskaż plik docelowy mapy"
            dlg.Filter = ZarzadcaSzablonow.FiltrPliku(_formatDocelowy)
            dlg.DefaultExt = ZarzadcaSzablonow.RozszerzenieDlaRodzaju(_formatDocelowy).TrimStart("."c)
            dlg.AddExtension = True
            dlg.FileName = Path.GetFileName(txtSciezkaDocelowa.Text)
            Dim dir = ""
            Try
                dir = Path.GetDirectoryName(txtSciezkaDocelowa.Text)
            Catch ex As ArgumentException
            End Try
            dlg.InitialDirectory = If(Not String.IsNullOrEmpty(dir) AndAlso Directory.Exists(dir), dir, FolderDownload)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _sciezkaZmienionaPrzezUzytkownika = True
                txtSciezkaDocelowa.Text = Path.ChangeExtension(dlg.FileName, ZarzadcaSzablonow.RozszerzenieDlaRodzaju(_formatDocelowy))
            End If
        End Using
    End Sub

    Private Sub txtSciezkaDocelowa_TextChanged(sender As Object, e As EventArgs) Handles txtSciezkaDocelowa.TextChanged
        ' Pole jest wąskie - pełna ścieżka w podpowiedzi
        _podpowiedzi.SetToolTip(txtSciezkaDocelowa, txtSciezkaDocelowa.Text)
        If lblNazwaWyniku IsNot Nothing Then
            lblNazwaWyniku.Text = Path.GetFileName(txtSciezkaDocelowa.Text)
            _podpowiedzi.SetToolTip(lblNazwaWyniku, lblNazwaWyniku.Text)
        End If
        If _biezacyKrok = 4 AndAlso txtSciezkaDocelowa.Focused Then
            _sciezkaZmienionaPrzezUzytkownika = True
        End If
    End Sub

    Private Sub rbWariant_CheckedChanged(sender As Object, e As EventArgs) Handles rbWariantPojedynczy.CheckedChanged, rbWariantWielopoziomowy.CheckedChanged
        If _biezacyKrok = 4 AndAlso DirectCast(sender, RadioButton).Checked Then
            PrzygotujPodsumowanie()
        End If
    End Sub

    Private Sub numZoomMin_ValueChanged(sender As Object, e As EventArgs) Handles numZoomMin.ValueChanged
        If numZoomMin.Value > numZoomMax.Value Then
            numZoomMax.Value = Math.Min(numZoomMax.Maximum, numZoomMin.Value)
        End If
        OdswiezOpisFormatuPoZmianieZoomu()
    End Sub

    Private Sub numZoomMax_ValueChanged(sender As Object, e As EventArgs) Handles numZoomMax.ValueChanged
        If numZoomMax.Value < numZoomMin.Value Then
            numZoomMin.Value = Math.Max(numZoomMin.Minimum, numZoomMax.Value)
        End If
        OdswiezOpisFormatuPoZmianieZoomu()
    End Sub

    Private Sub OdswiezOpisFormatuPoZmianieZoomu()
        If _biezacyKrok = 4 AndAlso chkWlasnyZoom.Checked AndAlso Not _wTrakciePodsumowania Then PrzygotujPodsumowanie()
    End Sub

    ''' <summary>
    ''' Korekta obrazu domyślnie wyłączona (decyzja autora: użytkownik sam ją reguluje). Profil sugerowany - Garmin MIP
    ''' dla odbiorników Garmin albo profil stylu - pojawia się dopiero po otwarciu okna korekty lub zaznaczeniu pola.
    ''' </summary>
    Private Sub UstawKorekteDomyslna()
        _celKorekty = _wybranyCel
        _opcjeKorekty = New OpcjeKorekcjiMip()
        _opcjeKorekty.Resetuj()
        chkKorektaObrazu.Checked = False
        AktualizujOpisKorekty()
    End Sub

    Private Function OpcjeKorektyDoOkna() As OpcjeKorekcjiMip
        Return _styl.OpcjeKorektyDoOkna(_opcjeKorekty, _wybranyCel)
    End Function

    Private Sub AktualizujOpisKorekty()
        If lblProfilKorekty Is Nothing Then Exit Sub
        lblProfilKorekty.Text = _styl.OpisKorekty(chkKorektaObrazu.Checked, _opcjeKorekty, _wybranyCel)
        If btnDostosujKorekte IsNot Nothing Then
            btnDostosujKorekte.Enabled = chkKorektaObrazu.Checked
        End If
        If cboProfilKorekty IsNot Nothing Then
            cboProfilKorekty.Enabled = chkKorektaObrazu.Checked
        End If
    End Sub

    Private Sub chkKorektaObrazu_CheckedChanged(sender As Object, e As EventArgs) Handles chkKorektaObrazu.CheckedChanged
        _styl.WlaczKorekte(_opcjeKorekty, chkKorektaObrazu.Checked, _wybranyCel)
        AktualizujOpisKorekty()
    End Sub

    Private Sub cboProfilKorekty_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboProfilKorekty.SelectedIndexChanged
        'lista profili ukryta - profil wybiera się w oknie korekty (przycisk „Profil / Podgląd…”)
    End Sub

    Private Sub btnDostosujKorekte_Click(sender As Object, e As EventArgs) Handles btnDostosujKorekte.Click
        Using dlg As New FormPodgladMip(OpcjeKorektyDoOkna(), _ostatniaProbkaBmp)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeKorekty = dlg.Opcje.Kopia()
                chkKorektaObrazu.Checked = _opcjeKorekty.Wlaczona
                AktualizujOpisKorekty()
            End If
        End Using
    End Sub

    Private Sub numRozmiarPiksela_ValueChanged(sender As Object, e As EventArgs) Handles numRozmiarPiksela.ValueChanged
        If _biezacyKrok = 4 Then
            PrzygotujPodsumowanie()
        End If
    End Sub

    Private Sub chkWlasnyZoom_CheckedChanged(sender As Object, e As EventArgs) Handles chkWlasnyZoom.CheckedChanged
        If _biezacyKrok = 4 Then
            PrzygotujPodsumowanie()
        Else
            numZoomMin.Enabled = chkWlasnyZoom.Checked
            numZoomMax.Enabled = chkWlasnyZoom.Checked
        End If
    End Sub

    Private Sub btnOtworzWieloskalowy_Click(sender As Object, e As EventArgs) Handles btnOtworzWieloskalowy.Click
        Dim zestaw = _styl.UtworzZestaw()
        Dim zMin = CInt(numZoomMin.Value)
        Dim zMax = CInt(numZoomMax.Value)

        Dim uklad = UkladWspolrzednych.PL1992
        Dim plikDocelowy = txtSciezkaDocelowa.Text.Trim()
        Dim probka = _ostatniaProbkaBmp

        Using dlg As New FormZaawansowanyMbtiles()
            If dlg.OtworzDlaKreatora(zestaw, _wybranyZasieg, uklad, plikDocelowy, zMin, zMax, _opcjeKorekty, probka, Me, _silnikPodgladuStylu) Then
                dlg.PobierzUstawieniaDlaKreatora(zestaw, zMin, zMax, _opcjeKorekty, plikDocelowy)
                If Not String.Equals(plikDocelowy, txtSciezkaDocelowa.Text.Trim(), StringComparison.OrdinalIgnoreCase) Then
                    _sciezkaZmienionaPrzezUzytkownika = True
                    txtSciezkaDocelowa.Text = plikDocelowy
                End If
                _styl.ZestawNiestandardowy = zestaw
                PokazSzczegolySzablonu(_styl.Szablon, False)

                _wTrakciePodsumowania = True
                Try
                    chkWlasnyZoom.Checked = True
                    numZoomMin.Value = Math.Max(numZoomMin.Minimum, Math.Min(numZoomMin.Maximum, zMin))
                    numZoomMax.Value = Math.Max(numZoomMax.Minimum, Math.Min(numZoomMax.Maximum, zMax))
                    chkKorektaObrazu.Checked = _opcjeKorekty.Wlaczona
                Finally
                    _wTrakciePodsumowania = False
                End Try
                PrzygotujPodsumowanie()

                MessageBox.Show("Zastosowano parametry warstw i powiększeń z narzędzia wieloskalowego.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Using
    End Sub

#End Region

#Region "Krok 5: Generowanie i wykonanie"

    ''' <summary>Folder segmentów sesji kreatora w folderze download programu (dawniej %TEMP% - nigdy nieusuwany, a „Przejdź do mapy” wczytywał z niego sesję, którą Windows mógł wyczyścić).</summary>
    Private Shared Function FolderSesjiKreatora(plikDocelowy As String) As String
        Dim baza = FolderDownload
        Try
            If String.IsNullOrEmpty(baza) Then Throw New DirectoryNotFoundException()
            Directory.CreateDirectory(baza)
        Catch ex As Exception
            baza = Path.GetTempPath()
        End Try
        Return Path.Combine(baza, PodsumowanieKreatora.NazwaFolderuSesji(plikDocelowy, DateTime.Now)) & "\"
    End Function

    ''' <summary>Czy kreator utworzy mapę wieloskalową (osobne pobranie każdego stopnia skali).</summary>
    Private Function CzyWieloskalowaAktywna() As Boolean
        Return _styl.WieloskalowaAktywna(_formatDocelowy, chkWieloskalowa.Checked)
    End Function

    Private Function PlanAutomatyczny() As List(Of PlanStopnia)
        Return _styl.PlanAutomatyczny(CDbl(numRozmiarPiksela.Value), ObszarKreatora.SzerokoscGeo(_wybranyZasieg), _ortofotomapaMozliwa AndAlso chkOrtofotomapa.Checked)
    End Function

    ''' <summary>Plan skal z okna „Ustawienia skal” albo automatyczny (kopia).</summary>
    Private Function PlanBiezacy() As List(Of PlanStopnia)
        Return _styl.PlanBiezacy(CDbl(numRozmiarPiksela.Value), ObszarKreatora.SzerokoscGeo(_wybranyZasieg), _ortofotomapaMozliwa AndAlso chkOrtofotomapa.Checked)
    End Function

    Private Function PobierzWybranyUklad() As UkladWspolrzednych
        If _wybranyCel = CelMapy.ProjektCad AndAlso cboUkladCad.SelectedIndex >= 0 Then
            Select Case cboUkladCad.SelectedIndex
                Case 0 : Return UkladWspolrzednych.PL2000Strefa5
                Case 1 : Return UkladWspolrzednych.PL2000Strefa6
                Case 2 : Return UkladWspolrzednych.PL2000Strefa7
                Case 3 : Return UkladWspolrzednych.PL2000Strefa8
                Case Else : Return UkladWspolrzednych.PL1992
            End Select
        End If
        Return UkladWspolrzednych.PL1992
    End Function

    Private Sub cboUkladCad_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboUkladCad.SelectedIndexChanged
        If _biezacyKrok = 4 Then
            PrzygotujPodsumowanie()
        End If
    End Sub

    Private Function SzacujWynik(piksel As Double, korytarz As Korytarz, plan As List(Of PlanStopnia), zestaw As ZestawWarstw) As SzacunekKreatora
        Dim uklad = PobierzWybranyUklad()
        Dim zasiegEfektywny = If(uklad.Epsg = UkladWspolrzednych.PL1992.Epsg, _wybranyZasieg, UkladWspolrzednych.PL1992.PrzeliczZasieg(uklad, _wybranyZasieg))
        If plan IsNot Nothing Then
            Return ZarzadcaSzablonow.SzacujWieloskalowo(plan, zasiegEfektywny, korytarz, uklad,
                zrodlaStopni:=plan.Select(Function(p) _styl.UtworzZestawStopnia(p).GrupyZapytan().Count).ToArray())
        End If
        Return ZarzadcaSzablonow.SzacujParametry(zasiegEfektywny, piksel, ZarzadcaSzablonow.BokSegmentuKreatora(zasiegEfektywny, piksel),
            korytarz, uklad, _formatDocelowy, ZarzadcaSzablonow.CzyFormatWielopoziomowy(_formatDocelowy),
            zestaw.GrupyZapytan().Count, przyciety:=True)
    End Function

    Private Sub btnUstawieniaSkal_Click(sender As Object, e As EventArgs)
        If _styl.Szablon Is Nothing OrElse Not _styl.Szablon.Wieloskalowy Then Exit Sub
        Using okno As New FormSkaleMapy(_styl.Szablon, PlanBiezacy(), PlanAutomatyczny(), ObszarKreatora.SzerokoscGeo(_wybranyZasieg))
            If okno.ShowDialog(Me) = DialogResult.OK Then
                _styl.PlanWlasny = okno.Wynik
                OdswiezSkalePodgladu()
                PrzygotujPodsumowanie()
            End If
        End Using
    End Sub

    Private Function UtworzZadaniePobierania(zestaw As ZestawWarstw, siatka As Siatka, folder As String, korytarz As Korytarz, Optional uklad As UkladWspolrzednych = Nothing) As ZadaniePobierania
        If korytarz IsNot Nothing Then korytarz.Zapisz(Path.Combine(folder, Korytarz.NazwaPliku))
        Dim zadanie As New ZadaniePobierania With {
            .AdresSerwera = zestaw(0).Adres,
            .Warstwy = zestaw.Nazwy(),
            .Krycia = zestaw.Krycia(),
            .TrybyMieszania = zestaw.TekstTrybowMieszania(),
            .Adresy = zestaw.Adresy(),
            .Zbiory = zestaw.Zbiory(),
            .ZrodlaMap = _styl.ZrodlaMap(),
            .Uklad = If(uklad, UkladWspolrzednych.PL1992),
            .Siatka = siatka,
            .Format = "png",
            .Przezroczystosc = True,
            .Prefiks = "seg",
            .Folder = folder,
            .Korytarz = korytarz,
            .LiczbaWatkow = 4,
            .LimitCzasuSekundy = 25}
        Dim z = siatka.Obszar
        Dim kultura = CultureInfo.InvariantCulture
        Dim sesja As New KonfiguracjaSesji With {
            .Folder = folder, .RodzajMapy = zestaw(0).Zbior, .AdresSerwera = zadanie.AdresSerwera,
            .XDol = z.XDol.ToString(kultura), .YLewy = z.YLewy.ToString(kultura),
            .XGora = z.XGora.ToString(kultura), .YPrawy = z.YPrawy.ToString(kultura),
            .RozmiarPiksela = siatka.RozmiarPiksela.ToString(kultura), .BokSegmentuPx = siatka.BokSegmentuPx.ToString(kultura),
            .UkladEpsg = zadanie.Uklad.Epsg, .LiczbaWarstw = zestaw.Liczba, .KrycieWarstw = zestaw.TekstKrycia(),
            .AdresyWarstw = ZestawWarstw.Polacz(zadanie.Adresy), .ZbioryWarstw = ZestawWarstw.Polacz(zadanie.Zbiory),
            .TrybyMieszaniaWarstw = zadanie.TrybyMieszania, .Prefiks = zadanie.Prefiks, .Format = zadanie.Format,
            .Numeracja = Siatka.TekstStylu(zadanie.Numeracja), .Przezroczystosc = zadanie.Przezroczystosc,
            .DataPobrania = DateTime.Today.ToString("yyyy-MM-dd", kultura)}
        zadanie.Warstwy.CopyTo(sesja.Warstwy)
        sesja.Zapisz(Path.Combine(folder, "conf.txt"), KodowanieSystemowe())
        Return zadanie
    End Function

    Private Async Sub btnGeneruj_Click(sender As Object, e As EventArgs) Handles btnGeneruj.Click
        Dim rozszerzenie = ZarzadcaSzablonow.RozszerzenieDlaRodzaju(_formatDocelowy)
        Dim plikDocelowy = txtSciezkaDocelowa.Text.Trim()
        If String.IsNullOrWhiteSpace(plikDocelowy) Then
            MessageBox.Show("Wskaż ścieżkę pliku docelowego.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            ' Sama nazwa pliku trafia do folderu download (dawniej do bieżącego folderu procesu, np. folderu programu)
            If Not Path.IsPathRooted(plikDocelowy) Then
                plikDocelowy = Path.Combine(If(Not String.IsNullOrEmpty(FolderDownload), FolderDownload,
                                               Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)), plikDocelowy)
            End If
            plikDocelowy = Path.GetFullPath(Path.ChangeExtension(plikDocelowy, rozszerzenie))
            Dim folderZapisu = Path.GetDirectoryName(plikDocelowy)
            If Not String.IsNullOrEmpty(folderZapisu) AndAlso Not Directory.Exists(folderZapisu) Then
                Directory.CreateDirectory(folderZapisu)
            End If
        Catch ex As Exception
            MessageBox.Show("Nie można zapisać pliku w tym miejscu: " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End Try

        If IstniejeWynik(plikDocelowy) Then
            If _sciezkaZmienionaPrzezUzytkownika Then
                If MessageBox.Show($"Mapa {Path.GetFileName(plikDocelowy)} już istnieje. Zastąpić ją wraz z jej tomami, jeśli są zapisane osobno?", "Kreator mapy",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            Else
                plikDocelowy = UnikalnaSciezka(plikDocelowy)
            End If
        End If
        txtSciezkaDocelowa.Text = plikDocelowy

        Dim piksel = CDbl(numRozmiarPiksela.Value)
        Dim korytarz = PobierzEfektywnyKorytarz()
        Dim zestaw = _styl.UtworzZestaw(piksel)
        If zestaw.Liczba = 0 Then
            MessageBox.Show("Wybrany styl nie zawiera żadnej warstwy. Wróć do kroku 3 i wybierz styl mapy.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Dim ukladDocelowy = PobierzWybranyUklad()
        Dim bladUkladu = ZarzadcaSzablonow.SprawdzZgodnoscUkladu(zestaw, ukladDocelowy)
        If Not String.IsNullOrEmpty(bladUkladu) Then
            MessageBox.Show(bladUkladu, "Niezgodność układu współrzędnych", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim zasiegEfektywny = If(ukladDocelowy.Epsg = UkladWspolrzednych.PL1992.Epsg, _wybranyZasieg, UkladWspolrzednych.PL1992.PrzeliczZasieg(ukladDocelowy, _wybranyZasieg))
        Dim siatka = PodsumowanieKreatora.SiatkaKreatora(zasiegEfektywny, piksel)
        Dim bladSiatki = PodsumowanieKreatora.BladSiatki(siatka)
        If bladSiatki IsNot Nothing Then
            MessageBox.Show("Nie można utworzyć mapy dla tego obszaru: " & bladSiatki & ".", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim wieloskalowa = CzyWieloskalowaAktywna()
        Dim plan = If(wieloskalowa, PlanBiezacy(), Nothing)
        If wieloskalowa AndAlso plan.Count = 0 Then Exit Sub

        ' Pytanie o duży obszar - te same progi i ten sam format, co szacunek w kroku 4
        Dim szacunek = SzacujWynik(piksel, korytarz, plan, zestaw)
        If szacunek.WymagaPotwierdzenia Then
            Dim odp = MessageBox.Show(PodsumowanieKreatora.PytanieDuzyObszar(szacunek), "Duży obszar mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If odp <> DialogResult.Yes Then Exit Sub
        End If

        ' Wolne miejsce: segmenty sesji i plik wynikowy zajmują razem ok. 2 razy szacowany rozmiar pliku
        Try
            Dim dysk As New DriveInfo(Path.GetPathRoot(plikDocelowy))
            Dim potrzebneB = PodsumowanieKreatora.PotrzebneMiejsceBajty(szacunek)
            If dysk.IsReady AndAlso dysk.AvailableFreeSpace < potrzebneB Then
                If MessageBox.Show($"Na dysku {dysk.Name} jest {dysk.AvailableFreeSpace / 1024.0 / 1024 / 1024:0.0} GB wolnego miejsca, a mapa może potrzebować ok. {potrzebneB / 1024.0 / 1024 / 1024:0.0} GB." & vbCrLf & vbCrLf &
                                   "Czy mimo to kontynuować?", "Mało miejsca na dysku", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            End If
        Catch ex As Exception
            'dysk sieciowy lub nietypowa ścieżka - pomijamy sprawdzenie
        End Try

        Try
            PrzygotujZadaniaGenerowania(plikDocelowy, siatka, zestaw, korytarz, plan)
        Catch ex As Exception
            MessageBox.Show("Nie można przygotować pobierania. " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End Try
        _sciezkaWygenerowanegoPliku = plikDocelowy
        Await UruchomGenerowanieAsync()
    End Sub

    Private Sub PrzygotujZadaniaGenerowania(plikDocelowy As String, siatka As Siatka, zestaw As ZestawWarstw,
                                           korytarz As Korytarz, plan As List(Of PlanStopnia))
        Dim ukladDocelowy = PobierzWybranyUklad()
        Dim folderSesji = FolderSesjiKreatora(plikDocelowy)
        Directory.CreateDirectory(folderSesji)
        _folderWygenerowanejSesji = folderSesji
        _zadaniaPobierania.Clear()
        _zadanieEksportu = Nothing
        _opcjeKompilacji = Nothing
        _planGenerowania = Nothing
        Dim korekta = If(chkKorektaObrazu.Checked, _opcjeKorekty.Kopia(), New OpcjeKorekcjiMip With {.Wlaczona = False})

        If plan IsNot Nothing Then
            ' Każdy stopień skali to osobna sesja (najdokładniejszy w głównym folderze, pozostałe obok: _skala2, _skala3…)
            _planGenerowania = plan
            For i = 0 To plan.Count - 1
                Dim folder = PodsumowanieKreatora.FolderStopnia(folderSesji, i)
                Directory.CreateDirectory(folder)
                Dim zestawStopnia = _styl.UtworzZestawStopnia(plan(i))
                If zestawStopnia.Liczba = 0 Then
                    Throw New InvalidOperationException($"Skala {plan(i).Stopien.Opis} nie ma aktywnych warstw. Sprawdź ustawienia skal.")
                End If
                Dim siatkaStopnia = PodsumowanieKreatora.SiatkaKreatora(siatka.Obszar, plan(i).Piksel)
                If PodsumowanieKreatora.BladSiatki(siatkaStopnia) IsNot Nothing Then
                    Throw New InvalidOperationException($"Nie można wyznaczyć siatki dla skali {plan(i).Stopien.Opis}. Sprawdź rozmiar piksela.")
                End If
                _zadaniaPobierania.Add(UtworzZadaniePobierania(zestawStopnia, siatkaStopnia, folder, korytarz, ukladDocelowy))
            Next
            If _formatDocelowy = RodzajEksportu.Jnx OrElse _formatDocelowy = RodzajEksportu.SQLiteDB Then
                ' JNX i SQLiteDB: źródła skal przypisuje UruchomGenerowanieAsync po pobraniu (ZrodlaPoziomow)
                _zadanieEksportu = New ZadanieEksportu With {
                    .Uklad = ukladDocelowy,
                    .Rodzaj = _formatDocelowy,
                    .KaflePng = _formatDocelowy = RodzajEksportu.SQLiteDB,
                    .Plik = plikDocelowy,
                    .Nazwa = Path.GetFileNameWithoutExtension(plikDocelowy),
                    .PoziomMin = plan.Min(Function(ps) ps.PoziomMin),
                    .PoziomMax = plan.Max(Function(ps) ps.PoziomMax),
                    .KolejnoscRysowania = CInt(numDrawOrder.Value)}
                If chkKorektaObrazu.Checked Then _zadanieEksportu.KorekcjaMip = korekta
                Exit Sub
            End If
            _opcjeKompilacji = New OpcjeKompilatoraMbtiles With {
                .PlikDocelowy = plikDocelowy,
                .NazwaMapy = Path.GetFileNameWithoutExtension(plikDocelowy),
                .OpisMapy = _styl.OpisMapyWieloskalowej(plan),
                .Atrybucja = _styl.AtrybucjaWieloskalowej(plan),
                .PoziomMin = plan.Min(Function(ps) ps.PoziomMin),
                .PoziomMax = plan.Max(Function(ps) ps.PoziomMax),
                .FormatKafli = FormatKafliMbtiles.TylkoPng,
                .Korekta = korekta}
        Else
            _zadaniaPobierania.Add(UtworzZadaniePobierania(zestaw, siatka, folderSesji, korytarz, ukladDocelowy))
            _zadanieEksportu = New ZadanieEksportu With {
                .Uklad = ukladDocelowy,
                .Rodzaj = _formatDocelowy,
                .Plik = plikDocelowy,
                .Nazwa = Path.GetFileNameWithoutExtension(plikDocelowy),
                .PoziomMin = CInt(numZoomMin.Value),
                .PoziomMax = CInt(numZoomMax.Value),
                .KaflePng = _formatDocelowy = RodzajEksportu.MBTiles OrElse _formatDocelowy = RodzajEksportu.SQLiteDB OrElse
                            (_formatDocelowy = RodzajEksportu.GeoTiff AndAlso korytarz IsNot Nothing),
                .FormatArkusza = If(korytarz Is Nothing, FormatArkusza.GeoTiffJpeg, FormatArkusza.GeoTiff),
                .PodzialNaTomy = chkPodzialTomow.Checked,
                .KolejnoscRysowania = CInt(numDrawOrder.Value)}
            If chkKorektaObrazu.Checked Then _zadanieEksportu.KorekcjaMip = korekta
        End If

    End Sub

    ''' <summary>
    ''' Pobiera segmenty (w mapie wieloskalowej - każdego stopnia skali) i tworzy plik mapy z zapamiętanych zadań.
    ''' Ponowne wywołanie (przycisk „Ponów pobieranie”) pobiera tylko brakujące segmenty - PobieranieSegmentow pomija istniejące pliki.
    ''' </summary>
    Private Async Function UruchomGenerowanieAsync() As Task
        If _zadaniaPobierania.Count = 0 OrElse (_zadanieEksportu Is Nothing AndAlso _opcjeKompilacji Is Nothing) Then Exit Function

        UstawKrok(5)
        _trwaPraca = True
        btnPrzerwij.Enabled = True
        btnPrzerwij.Visible = True
        btnPonow.Visible = False
        pnlSukces.Visible = False
        lblSukcesUwaga.Visible = False
        _podpowiedzi.SetToolTip(lblSzczegolyPostepu, "")
        _podpowiedzi.SetToolTip(lblSukcesUwaga, "")
        lblStanPostepu.ForeColor = Color.FromArgb(16, 44, 87)
        lblSzczegolyPostepu.Text = "Przygotowanie..."
        pbPostep.Style = ProgressBarStyle.Blocks
        pbPostep.Value = 0

        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim token = _cts.Token
        Dim plikDocelowy = If(_zadanieEksportu IsNot Nothing, _zadanieEksportu.Plik, _opcjeKompilacji.PlikDocelowy)
        Dim liczbaZadan = _zadaniaPobierania.Count
        Dim nieudane As Integer = 0, wszystkie As Integer = 0
        Dim braki As New List(Of String)
        Dim szczegolyBledow As New List(Of String)
        Dim plikiWyniku As String() = {plikDocelowy}
        ' Pozostały czas z rzeczywistej prędkości pobierania (po kilku sekundach) i liczby segmentów wszystkich skal

        Try
            Dim liczbySegmentow = Await Task.Run(Function() _zadaniaPobierania.Select(Function(zp) If(zp.Korytarz IsNot Nothing,
                CLng(zp.Korytarz.Segmenty(zp.Siatka, zp.Uklad).Count), zp.Siatka.LiczbaSegmentow)).ToList(), token)
            If IsDisposed OrElse token.IsCancellationRequested Then Exit Function
            Dim sumaSegmentow = liczbySegmentow.Sum()
            Dim pomiar As New PomiarPostepu(sumaSegmentow)
            Dim pobranePrzed As Long = 0
            Dim pierwszaDostepnaSesja As String = Nothing
            For i = 0 To liczbaZadan - 1
                Dim etap = If(liczbaZadan > 1, $"skala {i + 1} z {liczbaZadan} ({_planGenerowania(i).Stopien.Opis}): ", "")
                Dim poczatekEtapu = pobranePrzed
                lblStanPostepu.Text = "Pobieranie mapy z serwerów" & If(liczbaZadan > 1, $" - skala {_planGenerowania(i).Stopien.Opis}", "") & "..."
                Dim postepPobierania As New Progress(Of PostepPobierania)(
                    Sub(p)
                        If IsDisposed Then Exit Sub
                        If p.Wszystkie > 0 AndAlso sumaSegmentow > 0 Then
                            pbPostep.Style = ProgressBarStyle.Blocks
                            pbPostep.Value = Math.Max(pbPostep.Value, Math.Min(100, CInt((poczatekEtapu + p.Pobrane) * 100L \ sumaSegmentow)))
                        End If
                        lblSzczegolyPostepu.Text = "Krok 1 z 2: pobieranie segmentów - " & etap & p.Pobrane & " z " & p.Wszystkie & pomiar.Opis(poczatekEtapu + p.Pobrane)
                    End Sub)
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(_zadaniaPobierania(i), postepPobierania, token)
                If IsDisposed Then Exit Function

                If wynik.Przerwano Then
                    lblStanPostepu.Text = "Pobieranie zostało przerwane."
                    lblStanPostepu.ForeColor = Color.FromArgb(200, 30, 30)
                    lblSzczegolyPostepu.Text = "Już pobrane segmenty zostają - „Ponów pobieranie”” dokończy mapę, a „Wstecz”” wraca do ustawień."
                    btnPonow.Visible = True
                    Exit Function
                End If
                If wynik.Nieudane > 0 Then
                    Dim nazwa = If(_planGenerowania IsNot Nothing, _planGenerowania(i).Stopien.Opis, _styl.Szablon.Nazwa)
                    braki.Add($"{nazwa} ({wynik.Nieudane}/{wynik.Wszystkie})")
                    If wynik.PierwszyBlad <> "" Then szczegolyBledow.Add(nazwa & " – " & wynik.PierwszyBlad)
                End If
                If pierwszaDostepnaSesja Is Nothing AndAlso wynik.Wszystkie > wynik.Nieudane Then
                    pierwszaDostepnaSesja = _zadaniaPobierania(i).Folder
                End If
                nieudane += wynik.Nieudane
                wszystkie += wynik.Wszystkie
                pobranePrzed += liczbySegmentow(i)
            Next
            Dim opisBrakow = String.Join(", ", braki)
            ' Bez danych nie ma czego zapisać, a cudzą lub wcześniejszą mapę zastępuje tylko mapa kompletna
            Dim wstrzymanie = PodsumowanieKreatora.PowodWstrzymaniaZapisu(wszystkie, nieudane, IstniejeWynik(plikDocelowy), plikDocelowy, _plikZBrakami)
            If wstrzymanie IsNot Nothing Then
                lblStanPostepu.Text = wstrzymanie
                lblStanPostepu.ForeColor = Color.DarkOrange
                lblSzczegolyPostepu.Text = $"Brakujące segmenty – {opisBrakow}. Użyj przycisku Ponów pobieranie."
                _podpowiedzi.SetToolTip(lblSzczegolyPostepu, String.Join(vbCrLf, szczegolyBledow))
                btnPonow.Visible = True
                Exit Function
            End If
            _folderWygenerowanejSesji = pierwszaDostepnaSesja
            Dim zegarZapisu = Stopwatch.StartNew()

            lblStanPostepu.Text = "Zapisywanie pliku mapy..."
            lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie pliku " & Path.GetFileName(plikDocelowy) & "..."
            pbPostep.Style = ProgressBarStyle.Marquee
            Dim maska As Func(Of Double, Double, Boolean) = Nothing
            Dim korytarz = _zadaniaPobierania(0).Korytarz
            If korytarz IsNot Nothing Then
                Dim z = _zadaniaPobierania(0)
                maska = Await Task.Run(Function() korytarz.UtworzMaske(z.Siatka.Obszar, z.Uklad), token)
                If IsDisposed OrElse token.IsCancellationRequested Then Exit Function
            End If

            If _opcjeKompilacji IsNot Nothing Then
                ' Mapa wieloskalowa: każdy stopień wypełnia swój zakres powiększeń w jednym pliku MBTiles
                Dim warstwy As New List(Of WarstwaWielowarstwowa)()
                For i = 0 To liczbaZadan - 1
                    Dim z = _zadaniaPobierania(i)
                    Dim ps = _planGenerowania(i)
                    warstwy.Add(New WarstwaWielowarstwowa With {
                        .Folder = z.Folder, .Tytul = ps.Stopien.Opis, .OpisPelny = _styl.Szablon?.Nazwa & " - " & ps.Stopien.Opis,
                        .PoziomMin = ps.PoziomMin, .PoziomMax = ps.PoziomMax, .Kolejnosc = i, .Uklad = z.Uklad,
                        .Siatka = z.Siatka, .Zrodlo = New ZrodloSegmentow(z.Folder, z.Prefiks, z.Numeracja, z.Rozszerzenie, z.Siatka, przytnijDoObszaru:=True, maska:=maska)})
                Next
                Dim postepKompilacji As New Progress(Of RaportPostepuKompilacji)(
                    Sub(r)
                        If IsDisposed Then Exit Sub
                        pbPostep.Style = ProgressBarStyle.Blocks
                        pbPostep.Value = Math.Max(0, Math.Min(100, r.Procent))
                        lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie mapy wieloskalowej (" & r.Procent & "%" & If(r.AktualnyPoziom > 0, ", poziom " & r.AktualnyPoziom, "") & ")" &
                                                   PomiarPostepu.OpisZProcentu(r.Procent, zegarZapisu.Elapsed.TotalSeconds)
                    End Sub)
                plikDocelowy = Await KompilatorMbtiles.KompilujAsync(warstwy, _opcjeKompilacji, postepKompilacji, token)
                If IsDisposed Then Exit Function
                plikiWyniku = {plikDocelowy}
            Else
                Dim postepEksportu As New Progress(Of Integer)(
                    Sub(procent)
                        If IsDisposed Then Exit Sub
                        pbPostep.Style = ProgressBarStyle.Blocks
                        pbPostep.Value = Math.Max(0, Math.Min(100, procent))
                        lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie pliku mapy (" & procent & "%)" & PomiarPostepu.OpisZProcentu(procent, zegarZapisu.Elapsed.TotalSeconds)
                    End Sub)
                Dim zadanieEksportu = _zadanieEksportu
                ' Zestaw segmentów odczytujemy po pobraniu, także przy każdej próbie uzupełniania braków.
                Dim z = _zadaniaPobierania(0)
                zadanieEksportu.Zrodlo = New ZrodloSegmentow(z.Folder, z.Prefiks, z.Numeracja, z.Rozszerzenie, z.Siatka,
                                                            przytnijDoObszaru:=True, maska:=maska)
                zadanieEksportu.ZrodlaPoziomow.Clear()
                If _planGenerowania IsNot Nothing Then
                    ' JNX i SQLiteDB wieloskalowe: każdy zakres poziomów ze źródła swojej skali
                    For i = 0 To liczbaZadan - 1
                        Dim zs = _zadaniaPobierania(i)
                        zadanieEksportu.ZrodlaPoziomow.Add(New ZrodloPoziomow With {
                            .PoziomMin = _planGenerowania(i).PoziomMin, .PoziomMax = _planGenerowania(i).PoziomMax,
                            .Zrodlo = If(i = 0, zadanieEksportu.Zrodlo,
                                         New ZrodloSegmentow(zs.Folder, zs.Prefiks, zs.Numeracja, zs.Rozszerzenie, zs.Siatka, przytnijDoObszaru:=True, maska:=maska))})
                    Next
                End If
                plikiWyniku = Await Task.Run(Function() EksportMapy.EksportujBezpiecznie(zadanieEksportu, postepEksportu, token))
                If IsDisposed Then Exit Function
                plikDocelowy = plikiWyniku(0)
            End If
            _sciezkaWygenerowanegoPliku = plikDocelowy
            FormEksport.OstatniPlik = plikDocelowy

            If _wybranyCel = CelMapy.ProjektCad AndAlso _zadanieEksportu IsNot Nothing AndAlso
               _zadanieEksportu.Rodzaj = RodzajEksportu.GeoTiff AndAlso File.Exists(plikDocelowy) Then
                Try
                    Dim ukladCad = PobierzWybranyUklad()
                    Dim zPob = _zadaniaPobierania(0)
                    Dim zadanieCad As New ZadanieCad With {
                        .Uklad = ukladCad,
                        .FolderRysunku = Path.GetDirectoryName(plikDocelowy),
                        .Prefiks = zPob.Prefiks
                    }
                    Dim obraz As New ObrazGeoreferencyjny With {
                        .Uklad = ukladCad,
                        .Zasieg = zPob.Siatka.Obszar,
                        .SzerokoscPx = CLng(Math.Ceiling(zPob.Siatka.Obszar.Szerokosc / zPob.Siatka.RozmiarPiksela)),
                        .WysokoscPx = CLng(Math.Ceiling(zPob.Siatka.Obszar.Wysokosc / zPob.Siatka.RozmiarPiksela)),
                        .NazwaPliku = Path.GetFileName(plikDocelowy),
                        .SciezkaPliku = plikDocelowy
                    }
                    zadanieCad.Rastry.Add(obraz)
                    Dim zawartoscScr = EksportCad.Skrypt(zadanieCad)
                    Dim sciezkaScr = Path.ChangeExtension(plikDocelowy, ".scr")
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
                    File.WriteAllText(sciezkaScr, zawartoscScr, System.Text.Encoding.GetEncoding(1250))
                Catch exCad As Exception
                    ' Błąd zapisu skryptu SCR nie powinien unieważniać zapisanego rastra GeoTIFF
                End Try
            End If

            pbPostep.Style = ProgressBarStyle.Blocks
            pbPostep.Value = 100
            lblStanPostepu.Text = If(nieudane > 0, "Mapa zapisana z brakami", "Gotowe")
            If nieudane > 0 Then lblStanPostepu.ForeColor = Color.DarkOrange
            lblSzczegolyPostepu.Text = "Mapa została zapisana. Całkowity czas: " & PomiarPostepu.Czas(pomiar.Uplynelo)
            pnlSukces.Visible = True

            Dim rozmiar = plikiWyniku.Sum(Function(p) New FileInfo(p).Length)
            Dim rozmiarTekst = PodsumowanieKreatora.OpisRozmiaru(rozmiar)
            lblSukcesPlik.Text = If(plikiWyniku.Length > 1, $"Tomy ({plikiWyniku.Length}, razem {rozmiarTekst}): ",
                                  $"Mapa ({rozmiarTekst}): ") & Path.GetFileName(plikDocelowy)
            Dim wykaz = String.Join(vbCrLf, plikiWyniku)
            If _planGenerowania IsNot Nothing Then
                lblSukcesPlik.Text &= vbCrLf & PodsumowanieKreatora.OpisSkalWyniku(_planGenerowania)
                wykaz &= vbCrLf & PodsumowanieKreatora.WykazSkalWyniku(_planGenerowania)
            Else
                lblSukcesPlik.Text &= vbCrLf & Path.GetDirectoryName(plikDocelowy)
            End If
            _podpowiedzi.SetToolTip(lblSukcesPlik, wykaz)
            ' tylko gdy Windows ma program dla tego rozszerzenia - .mbtiles i .jnx zwykle go nie mają
            btnOtworzMape.Visible = MaProgramDoOtwarcia(Path.GetExtension(plikDocelowy))
            If _wybranyCel = CelMapy.ProjektCad Then
                lblSukcesWskazowka.Text = "W programie CAD (AutoCAD, ZWCAD, BricsCAD) otwórz rysunek i wpisz polecenie _SCRIPT, a następnie wskaż plik " & Path.GetFileName(Path.ChangeExtension(plikDocelowy, ".scr")) & "."
            Else
                lblSukcesWskazowka.Text = ZarzadcaSzablonow.WskazowkaWgrania(If(_zadanieEksportu IsNot Nothing, _zadanieEksportu.Rodzaj, RodzajEksportu.MBTiles))
            End If

            _plikZBrakami = If(nieudane > 0, plikDocelowy, "")
            If nieudane > 0 Then
                lblSukcesTytul.Text = "✓ Mapa zapisana, ale z brakami"
                lblSukcesTytul.ForeColor = Color.FromArgb(180, 90, 0)
                lblSukcesUwaga.Text = $"Brakujące segmenty – {opisBrakow}. Wybierz Ponów pobieranie, aby uzupełnić mapę."
                _podpowiedzi.SetToolTip(lblSukcesUwaga, lblSukcesUwaga.Text & vbCrLf & String.Join(vbCrLf, szczegolyBledow))
                lblSukcesUwaga.Visible = True
                btnPonow.Visible = True
            Else
                lblSukcesTytul.Text = "✓ Mapa została pomyślnie wygenerowana!"
                lblSukcesTytul.ForeColor = Color.FromArgb(0, 130, 40)
            End If

        Catch ex As OperationCanceledException
            lblStanPostepu.Text = "Operacja została przerwana."
            lblStanPostepu.ForeColor = Color.FromArgb(200, 30, 30)
            lblSzczegolyPostepu.Text = "„Ponów pobieranie”” dokończy mapę, a „Wstecz”” wraca do ustawień."
            btnPonow.Visible = True
        Catch ex As ObjectDisposedException
            'okno zamknięte w trakcie generowania
        Catch ex As Exception
            If IsDisposed Then Exit Function
            lblStanPostepu.Text = "Wystąpił błąd podczas generowania mapy."
            lblStanPostepu.ForeColor = Color.FromArgb(200, 30, 30)
            lblSzczegolyPostepu.Text = OpisBledu.Z(ex)
            btnPonow.Visible = True
        Finally
            _trwaPraca = False
            If _cts IsNot Nothing Then
                _cts.Dispose()
                _cts = Nothing
            End If
            If Not IsDisposed Then
                btnAnuluj.Enabled = True
                btnPrzerwij.Visible = False
                btnWstecz.Enabled = True
                pnlKroki.Invalidate()
            End If
        End Try
    End Function

    Private Async Sub btnPonow_Click(sender As Object, e As EventArgs)
        If _trwaPraca Then Exit Sub
        Await UruchomGenerowanieAsync()
    End Sub

    Private Sub btnNowaMapa_Click(sender As Object, e As EventArgs)
        If _trwaPraca Then Exit Sub
        ' Wybory zostają - zmienia się tylko to, co użytkownik zmieni; nowa nazwa pliku nie nadpisze gotowej mapy
        _sciezkaZmienionaPrzezUzytkownika = False
        _zadaniaPobierania.Clear()
        _zadanieEksportu = Nothing
        _opcjeKompilacji = Nothing
        pnlSukces.Visible = False
        UstawKrok(1)
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        _cts?.Cancel()
        btnPrzerwij.Enabled = False
        lblSzczegolyPostepu.Text = "Przerywanie operacji..."
    End Sub

    <DllImport("shlwapi.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function AssocQueryString(flags As Integer, str As Integer, pszAssoc As String, pszExtra As String,
                                             pszOut As System.Text.StringBuilder, ByRef pcchOut As UInteger) As Integer
    End Function

    ''' <summary>Czy w Windows jest program otwierający pliki o tym rozszerzeniu (.mbtiles, .jnx zwykle go nie mają).</summary>
    Friend Shared Function MaProgramDoOtwarcia(rozszerzenie As String) As Boolean
        Const ASSOCSTR_EXECUTABLE As Integer = 2
        ' ASSOCF_INIT_IGNOREUNKNOWN - bez tego nieznane rozszerzenie zwraca okno „Otwórz za pomocą”
        Const ASSOCF_INIT_IGNOREUNKNOWN As Integer = &H400
        Dim dlugosc As UInteger = 0
        Try
            Return AssocQueryString(ASSOCF_INIT_IGNOREUNKNOWN, ASSOCSTR_EXECUTABLE, rozszerzenie, Nothing, Nothing, dlugosc) = 1 AndAlso dlugosc > 0
        Catch ex As Exception When TypeOf ex Is DllNotFoundException OrElse TypeOf ex Is EntryPointNotFoundException
            Return False
        End Try
    End Function

    Private Sub btnOtworzMape_Click(sender As Object, e As EventArgs)
        If Not File.Exists(_sciezkaWygenerowanegoPliku) Then Exit Sub
        Try
            Process.Start(New ProcessStartInfo(_sciezkaWygenerowanegoPliku) With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show("Nie udało się otworzyć mapy: " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub btnWyslijNaTelefon_Click(sender As Object, e As EventArgs)
        If Not File.Exists(_sciezkaWygenerowanegoPliku) Then
            MessageBox.Show("Brak wygenerowanego pliku mapy do przesłania.", "Brak pliku", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        FormWyslijNaTelefon.PokazDlaPliku(Me, _sciezkaWygenerowanegoPliku)
    End Sub

    Private Sub btnPokazWFolderze_Click(sender As Object, e As EventArgs) Handles btnPokazWFolderze.Click
        If File.Exists(_sciezkaWygenerowanegoPliku) OrElse Directory.Exists(_sciezkaWygenerowanegoPliku) Then
            Try
                Process.Start("explorer.exe", "/select,""" & _sciezkaWygenerowanegoPliku & """")
            Catch ex As Exception
                MessageBox.Show("Nie udało się otworzyć folderu: " & OpisBledu.Z(ex), "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End If
    End Sub

    Private Sub btnZaladujDoProgramu_Click(sender As Object, e As EventArgs) Handles btnZaladujDoProgramu.Click
        If _form1 IsNot Nothing Then
            If Directory.Exists(_folderWygenerowanejSesji) Then
                _form1.ZaladujSesjeZFolderu(_folderWygenerowanejSesji)
            End If
            If _wybranyZasieg.Poprawny Then
                _form1.CentrujNaZasiegu(_wybranyZasieg)
            End If
            Dim korytarz = PobierzEfektywnyKorytarz()
            If korytarz IsNot Nothing Then
                _form1.UstawKorytarz(korytarz)
            End If
            PrzywrocZMinimalizacji(_form1)
            If Not Form1.TrybOkienBezAktywacji Then
                SetForegroundWindow(_form1.Handle)
                _form1.Activate()
                _form1.BringToFront()
            End If
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

#End Region

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _trwaPraca Then
            If MessageBox.Show("Trwa generowanie mapy. Czy na pewno chcesz przerwać i zamknąć kreator?", "Kreator mapy",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                e.Cancel = True
                Exit Sub
            End If
            _cts?.Cancel()
        End If
        ' Najpierw przerwanie, potem zwolnienie - dawniej podglądy pobierały się dalej na zwolnionych obiektach
        _timerWyszukiwania.Stop()
        _ctsWyszukiwania?.Cancel()
        _ctsPodgladStylu?.Cancel()
        _ctsPodgladObszaru?.Cancel()
        _timerWyszukiwania.Dispose()
        _ctsWyszukiwania?.Dispose()
        _ctsPodgladStylu?.Dispose()
        _ctsPodgladObszaru?.Dispose()
        _silnikPodgladuStylu.Dispose()
        _silnikPodgladuObszaru.Dispose()
        _bazaPodgladuStylu?.Dispose()
        _bazaPodgladuObszaru?.Dispose()
        _ostatniaProbkaBmp?.Dispose()
        _podpowiedzi.Dispose()
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        If _form1 IsNot Nothing AndAlso Not _form1.IsDisposed Then
            _form1.Enabled = True
            PrzywrocZMinimalizacji(_form1)
            _form1.Activate()
            _form1.BringToFront()
        End If
    End Sub

End Class
