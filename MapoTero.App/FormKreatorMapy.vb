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
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
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

    Private Const BokSegmentu As Integer = 1024
    Private Const IdWlasnegoZestawu As String = "wlasny_zestaw"

    ' Wybory zapamiętane między kolejnymi otwarciami kreatora w jednej sesji programu
    Private Shared _ostatniCel As CelMapy? = Nothing
    Private Shared _ostatniSzablonId As String = ""
    Private Shared _pokazZaawansowane As Boolean = False

    Private ReadOnly _form1 As Form1
    Private _biezacyKrok As Integer = 1

    Private _wybranyCel As CelMapy = CelMapy.TelefonTablet
    Private _wybranySzablon As SzablonMapy
    Private _zestawNiestandardowy As ZestawWarstw
    Private _krycieCieniowania As Integer = 35
    Private _maCieniowanie As Boolean = False
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
    Private ReadOnly _zrodlaKreatora As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
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
    Private lblWskazowkaKrok4 As Label
    Private btnPonow As Button
    Private btnNowaMapa As Button
    Private lblSukcesWskazowka As Label
    Private lblSukcesUwaga As Label
    Private pnlKroki As Panel
    Private ReadOnly _nazwyKrokow As String() = {"Cel", "Styl", "Obszar", "Zapis"}

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

    Public Sub New(form1 As Form1)
        InitializeComponent()
        _form1 = form1

        PrzygotujDodatkoweKontrolki()

        ' Klawisz Esc zamyka okno
        PomocProgramu.ZamykanieKlawiszemEsc(Me, True)

        ' Konfiguracja timera interaktywnego wyszukiwania
        AddHandler _timerWyszukiwania.Tick, AddressOf TimerWyszukiwania_Tick

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

        ' Krok 2: opis stylu i lista warstw w trzech i dwóch wierszach (dawniej obcinane), podgląd odpowiednio niższy
        lblOpisSzablonu.Size = New Size(374, 46)
        lblSkladnikiNaglowek.Location = New Point(14, 86)
        lblSkladnikiSzablonu.Location = New Point(14, 101)
        lblSkladnikiSzablonu.Size = New Size(374, 44)
        lblCieniowanieOpis.Location = New Point(14, 147)
        tbCieniowanie.Location = New Point(14, 163)
        lblCieniowanieWartosc.Location = New Point(344, 166)
        For Each c As Control In New Control() {lblStanPodgladuStyl, btnZoomMinusStyl, lblZoomStyl, btnZoomPlusStyl, btnZoomResetStyl, btnOdswiezStyl}
            c.Top += 33
        Next
        picPodgladStylu.Location = New Point(14, 215)
        picPodgladStylu.Size = New Size(374, 165)

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
        chkWieloskalowa = New CheckBox With {.Name = "chkWieloskalowa", .Location = New Point(14, 66), .Size = New Size(720, 20),
                                             .AutoEllipsis = True, .Checked = True, .Visible = False}
        AddHandler chkWieloskalowa.CheckedChanged, Sub()
                                                       If _biezacyKrok = 4 Then PrzygotujPodsumowanie()
                                                   End Sub
        grpSzczegolowosc.Controls.AddRange({lblRozmiarPiksela, numRozmiarPiksela, lblPikselJednostka, lnkPikselZalecany, chkWieloskalowa})
        pnlKrok4.Controls.Add(grpSzczegolowosc)

        lnkZaawansowane = New LinkLabel With {.Name = "lnkZaawansowane", .Location = New Point(16, 274), .AutoSize = True}
        AddHandler lnkZaawansowane.LinkClicked, Sub()
                                                    _pokazZaawansowane = Not _pokazZaawansowane
                                                    UstawWidocznoscZaawansowanych()
                                                End Sub
        pnlKrok4.Controls.Add(lnkZaawansowane)

        lblWskazowkaKrok4 = New Label With {.Name = "lblWskazowkaKrok4", .Location = New Point(16, 298), .Size = New Size(744, 60),
                                            .ForeColor = Color.FromArgb(70, 90, 120),
                                            .Text = "Ustawienia dobrano automatycznie do celu i stylu mapy - możesz od razu kliknąć „Generuj mapę””." & vbCrLf &
                                                    "Mniejszy rozmiar piksela daje dokładniejszą mapę, ale większy plik i dłuższe pobieranie."}
        pnlKrok4.Controls.Add(lblWskazowkaKrok4)

        grpZaawansowane.Text = "Opcje zaawansowane"
        grpZaawansowane.Location = New Point(16, 292)
        grpZaawansowane.Size = New Size(744, 150)
        chkKorektaObrazu.Location = New Point(16, 18)
        lblProfil.Location = New Point(36, 44)
        cboProfilKorekty.Location = New Point(78, 40)
        cboProfilKorekty.Size = New Size(280, 23)
        btnDostosujKorekte.Location = New Point(368, 38)
        btnDostosujKorekte.Size = New Size(150, 26)
        chkWlasnyZoom.Text = "Własny zakres powiększeń (zoom) kafli:"
        chkWlasnyZoom.Location = New Point(16, 70)
        lblZoomMin.Location = New Point(330, 72)
        numZoomMin.Location = New Point(398, 69)
        lblZoomMax.Location = New Point(465, 72)
        numZoomMax.Location = New Point(535, 69)
        lblDrawOrder.Location = New Point(16, 98)
        numDrawOrder.Location = New Point(290, 95)
        chkPodzialTomow.Location = New Point(375, 97)
        btnOtworzWieloskalowy.Location = New Point(16, 119)
        btnOtworzWieloskalowy.Size = New Size(280, 26)
        lblWieloskalowyOpis.Location = New Point(305, 119)
        lblWieloskalowyOpis.Size = New Size(430, 28)

        ' Krok 5: ponawianie, wskazówka wgrania, kolejna mapa
        btnPonow = New Button With {.Name = "btnPonow", .Text = "Ponów pobieranie", .Location = New Point(160, 165),
                                    .Size = New Size(170, 32), .Visible = False, .UseVisualStyleBackColor = True}
        AddHandler btnPonow.Click, AddressOf btnPonow_Click
        pnlKrok5.Controls.Add(btnPonow)
        _podpowiedzi.SetToolTip(btnPonow, "Pobiera tylko brakujące segmenty (już pobrane zostają) i ponownie tworzy plik mapy.")

        pnlSukces.Size = New Size(694, 200)
        lblSukcesTytul.Location = New Point(0, 4)
        lblSukcesPlik.Location = New Point(2, 38)
        lblSukcesPlik.Size = New Size(690, 40)
        lblSukcesWskazowka = New Label With {.Name = "lblSukcesWskazowka", .Location = New Point(2, 80), .Size = New Size(690, 40),
                                             .ForeColor = Color.FromArgb(16, 44, 87), .Font = New Font("Segoe UI", 9.0!)}
        lblSukcesUwaga = New Label With {.Name = "lblSukcesUwaga", .Location = New Point(2, 120), .Size = New Size(690, 34),
                                         .ForeColor = Color.FromArgb(180, 90, 0), .Visible = False}
        btnPokazWFolderze.Location = New Point(2, 158)
        btnZaladujDoProgramu.Location = New Point(162, 158)
        btnNowaMapa = New Button With {.Name = "btnNowaMapa", .Text = "Utwórz kolejną mapę", .Location = New Point(402, 158),
                                       .Size = New Size(180, 34), .UseVisualStyleBackColor = True}
        AddHandler btnNowaMapa.Click, AddressOf btnNowaMapa_Click
        pnlSukces.Controls.AddRange({lblSukcesWskazowka, lblSukcesUwaga, btnNowaMapa})

        ' Stopka: pasek kroków (klik w ukończony krok wraca do niego)
        pnlKroki = New Panel With {.Name = "pnlKroki", .Location = New Point(112, 10), .Size = New Size(316, 30)}
        GetType(Panel).GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).SetValue(pnlKroki, True)
        AddHandler pnlKroki.Paint, AddressOf pnlKroki_Paint
        AddHandler pnlKroki.MouseClick, AddressOf pnlKroki_MouseClick
        AddHandler pnlKroki.MouseMove, Sub(s, e) pnlKroki.Cursor = If(KrokPodKursorem(e.X) < _biezacyKrok AndAlso Not _trwaPraca, Cursors.Hand, Cursors.Default)
        pnlStopka.Controls.Add(pnlKroki)

        _podpowiedzi.SetToolTip(numRozmiarPiksela, "Ile metrów terenu przypada na jeden piksel mapy. Mniejsza wartość - więcej szczegółów i większy plik.")
        _podpowiedzi.SetToolTip(tbCieniowanie, "Siła cieniowania rzeźby terenu: 0% - brak, 100% - pełne cieniowanie.")
        _podpowiedzi.SetToolTip(btnPrzegladajDocelowy, "Wybierz folder i nazwę pliku mapy.")
        UstawWidocznoscZaawansowanych()
    End Sub

    Private Sub UstawWidocznoscZaawansowanych()
        grpZaawansowane.Visible = _pokazZaawansowane
        lblWskazowkaKrok4.Visible = Not _pokazZaawansowane
        lnkZaawansowane.Text = If(_pokazZaawansowane,
                                  "▾ Ukryj opcje zaawansowane",
                                  "▸ Pokaż opcje zaawansowane (korekta obrazu, powiększenia, ustawienia Garmin)")
    End Sub

    Private Sub InicjalizujStanPoczatkowy()
        ' Domyślny zasięg z bieżącej siatki okna głównego (jeśli poprawny)
        If _form1 IsNot Nothing Then
            Dim s = _form1.BiezacaSiatka()
            If s.Poprawna Then
                _wybranyZasiegZMapy = s.ZasiegSiatki
                _wybranyZasieg = _wybranyZasiegZMapy
                lblStatusZaznaczenia.Text = "Aktualny zasięg z mapy: " & FormatujWymiaryZasiegu(_wybranyZasieg)
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
        Dim idDoZaznaczenia = If(_wybranySzablon IsNot Nothing, _wybranySzablon.Id, _ostatniSzablonId)
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

    Private Function CzyWlasnyZestaw() As Boolean
        Return _wybranySzablon IsNot Nothing AndAlso _wybranySzablon.Id = IdWlasnegoZestawu
    End Function

    Private Shared Function OpisZestawu(zestaw As ZestawWarstw) As String
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return ""
        Return String.Join(" + ", zestaw.Warstwy.Select(Function(w) w.Nazwa & If(w.Krycie < 100, " (" & w.Krycie & "%)", "")))
    End Function

    ''' <param name="zachowajUstawienia">True - ten sam styl wybrany ponownie (np. po powrocie z kroku 3): suwak cieniowania
    ''' zachowuje wartość ustawioną przez użytkownika.</param>
    Private Sub PokazSzczegolySzablonu(s As SzablonMapy, Optional zachowajUstawienia As Boolean = False)
        _wybranySzablon = s
        If s Is Nothing Then Exit Sub

        ' Bez piktogramu: etykieta (GDI) pokazywała emoji jako pusty prostokąt, lista stylów rysuje je poprawnie
        lblNazwaSzablonu.Text = s.Nazwa
        lblOpisSzablonu.Text = s.Opis

        ' Warstwa cieniowania - jedna funkcja w MapoTero.Core zamiast kilku kopii wyszukiwania po nazwie
        Dim krycieCien As Integer = -1
        If _zestawNiestandardowy IsNot Nothing Then
            lblSkladnikiSzablonu.Text = OpisZestawu(_zestawNiestandardowy)
            Dim w = ZarzadcaSzablonow.ZnajdzWarstweCieniowania(_zestawNiestandardowy)
            If w IsNot Nothing Then krycieCien = w.Krycie
        ElseIf s.Id = IdWlasnegoZestawu Then
            Dim biezacy = If(_form1 IsNot Nothing, _form1.WybranyZestaw(), Nothing)
            If biezacy IsNot Nothing AndAlso biezacy.Liczba > 0 Then
                lblSkladnikiSzablonu.Text = OpisZestawu(biezacy)
                Dim w = ZarzadcaSzablonow.ZnajdzWarstweCieniowania(biezacy)
                If w IsNot Nothing Then krycieCien = w.Krycie
            Else
                lblSkladnikiSzablonu.Text = "(w oknie głównym nie wybrano jeszcze żadnej warstwy)"
            End If
        Else
            lblSkladnikiSzablonu.Text = s.OpisSkladnikow() & If(s.Wieloskalowy, vbCrLf & "Skale: " & s.OpisSkal(), "")
            Dim i = ZarzadcaSzablonow.IndeksCieniowania(s)
            If i >= 0 Then krycieCien = s.Warstwy(i).Krycie
        End If

        _maCieniowanie = krycieCien >= 0
        lblCieniowanieOpis.Visible = _maCieniowanie
        tbCieniowanie.Visible = _maCieniowanie
        lblCieniowanieWartosc.Visible = _maCieniowanie
        If _maCieniowanie Then
            If Not zachowajUstawienia Then _krycieCieniowania = krycieCien
            tbCieniowanie.Value = Math.Max(tbCieniowanie.Minimum, Math.Min(tbCieniowanie.Maximum, _krycieCieniowania))
            lblCieniowanieWartosc.Text = _krycieCieniowania & "%"
        End If

        OdswiezPodgladStyluAsync()
    End Sub

    ''' <summary>Ustawia krycie warstwy cieniowania zestawu według suwaka kreatora.</summary>
    Private Sub ZastosujKrycieCieniowania(zestaw As ZestawWarstw)
        If Not _maCieniowanie Then Exit Sub
        Dim w = ZarzadcaSzablonow.ZnajdzWarstweCieniowania(zestaw)
        If w Is Nothing Then Exit Sub
        ' W stylu wieloskalowym stopnie mniej dokładne mają słabsze cieniowanie (np. 45% przy 70% w 1:10 000) -
        ' suwak zmienia je proporcjonalnie, zamiast ustawiać wszystkim tę samą wartość
        Dim bazowe = 0
        If _zestawNiestandardowy Is Nothing AndAlso Not CzyWlasnyZestaw() AndAlso _wybranySzablon IsNot Nothing Then
            Dim i = ZarzadcaSzablonow.IndeksCieniowania(_wybranySzablon)
            If i >= 0 Then bazowe = _wybranySzablon.Warstwy(i).Krycie
        End If
        w.Krycie = If(bazowe > 0, Math.Max(0, Math.Min(100, CInt(Math.Round(_krycieCieniowania * w.Krycie / CDbl(bazowe))))), _krycieCieniowania)
    End Sub

    Private Sub UstawKrok(krok As Integer)
        _biezacyKrok = krok

        pnlKrok1.Visible = (krok = 1)
        pnlKrok2.Visible = (krok = 2)
        pnlKrok3.Visible = (krok = 3)
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
                lblTytulKroku.Text = "Krok 2 z 4: Wybierz styl mapy"
                lblPodtytulKroku.Text = "Gotowe zestawy warstw sprawdzone w praktyce. Podgląd pokazuje przykładowy fragment - możesz go przesuwać myszką."
                btnWstecz.Enabled = True
                btnDalej.Visible = True
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                AcceptButton = btnDalej
                If Not _celListy.HasValue OrElse _celListy.Value <> _wybranyCel Then OdswiezListeSzablonow()

            Case 3
                lblTytulKroku.Text = "Krok 3 z 4: Wybierz obszar mapy"
                lblPodtytulKroku.Text = "Wskaż interesujący Cię rejon: zaznacz prostokąt na mapie, wyszukaj miejscowość lub wskaż trasę GPX."
                btnWstecz.Enabled = True
                btnDalej.Visible = True
                btnGeneruj.Visible = False
                btnAnuluj.Text = "Anuluj"
                btnAnuluj.Enabled = True
                ' Enter w polu wyszukiwania uruchamia szukanie, a nie przejście dalej
                AcceptButton = Nothing
                AktualizujPodgladObszaru()

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
            Dim klucz = _wybranyCel.ToString() & "|" & If(_wybranySzablon IsNot Nothing, _wybranySzablon.Id, "")
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
        ' Piksel z szablonu - dawniej brany tylko przy wartości <= 0, czego pole (minimum 0,1) nie dopuszcza,
        ' więc kataster dostawał 1 m zamiast 0,25 m
        Dim piksel = If(_wybranySzablon IsNot Nothing, _wybranySzablon.DomyslnyPiksel, 1.0)
        If CzyWlasnyZestaw() AndAlso _form1 IsNot Nothing Then
            Dim s = _form1.BiezacaSiatka()
            If s IsNot Nothing AndAlso s.RozmiarPiksela > 0 Then piksel = s.RozmiarPiksela
        End If
        UstawPiksel(piksel)

        Dim wielo = ZarzadcaSzablonow.ZalecanyWariantWielopoziomowy(_wybranyCel)
        rbWariantWielopoziomowy.Checked = wielo
        rbWariantPojedynczy.Checked = Not wielo

        Dim profil = If(_wybranyCel = CelMapy.OdbiornikGarmin, ProfilKorektyRastra.GarminMip,
                        If(_wybranySzablon IsNot Nothing, _wybranySzablon.ProfilKorekty, ProfilKorektyRastra.Brak))
        _opcjeKorekty = New OpcjeKorekcjiMip()
        UstawProfilWLiscie(If(profil = ProfilKorektyRastra.Brak, ProfilKorektyRastra.WyostrzenieKrawedzi, profil))
        chkKorektaObrazu.Checked = profil <> ProfilKorektyRastra.Brak
        chkKorektaObrazu_CheckedChanged(chkKorektaObrazu, EventArgs.Empty)
        btnDostosujKorekte.Text = "Dostosuj suwaki..."
        chkWlasnyZoom.Checked = False
        chkWieloskalowa.Checked = True
    End Sub

    Private Sub UstawPiksel(piksel As Double)
        numRozmiarPiksela.Value = CDec(Math.Max(CDbl(numRozmiarPiksela.Minimum), Math.Min(CDbl(numRozmiarPiksela.Maximum), Math.Round(piksel, 2))))
    End Sub

    Private Sub UstawProfilWLiscie(p As ProfilKorektyRastra)
        Dim i = Array.IndexOf(_profileListy, p)
        If i < 0 Then i = 1
        cboProfilKorekty.SelectedIndex = i
        'zdarzenie nie zachodzi, gdy indeks się nie zmienia - profil ustawiany jawnie
        _opcjeKorekty.UstawWgProfilu(_profileListy(i))
        _opcjeKorekty.Wlaczona = chkKorektaObrazu.Checked
    End Sub

    Private Sub OdswiezPodsumowanie()
        Dim piksel = CDbl(numRozmiarPiksela.Value)
        Dim korytarz = PobierzEfektywnyKorytarz()
        Dim wielo = rbWariantWielopoziomowy.Checked
        _formatDocelowy = ZarzadcaSzablonow.DopasujFormatDocelowy(_wybranyCel, wielo)
        Dim zestaw = PobierzZestawStylu(piksel)

        ' Mapa wieloskalowa: tylko MBTiles (kompilator łączy osobno pobrane stopnie skali w jeden plik)
        Dim mozliwaWieloskalowa = _wybranySzablon IsNot Nothing AndAlso _wybranySzablon.Wieloskalowy AndAlso
                                  _zestawNiestandardowy Is Nothing AndAlso _formatDocelowy = RodzajEksportu.MBTiles
        chkWieloskalowa.Visible = mozliwaWieloskalowa
        Dim wieloskalowa = CzyWieloskalowaAktywna()
        Dim plan = If(wieloskalowa, PlanBiezacy(), Nothing)
        If mozliwaWieloskalowa Then
            chkWieloskalowa.Text = "Mapa wieloskalowa - przy oddalaniu przełącza się na mapy w mniejszej skali: " & _wybranySzablon.OpisSkal()
            Dim planOpis = ZarzadcaSzablonow.PlanStopni(_wybranySzablon, piksel, SzerokoscGeoObszaru())
            _podpowiedzi.SetToolTip(chkWieloskalowa, String.Join(vbCrLf, planOpis.Select(Function(ps) $"{ps.Stopien.Opis}: piksel {ps.Piksel:0.##} m, powiększenia {ps.PoziomMin}-{ps.PoziomMax}")))
        End If

        Dim szacunek = If(wieloskalowa,
                          ZarzadcaSzablonow.SzacujWieloskalowo(plan, _wybranyZasieg, korytarz, UkladWspolrzednych.PL1992, ZarzadcaSzablonow.LiczbaZrodel(zestaw)),
                          ZarzadcaSzablonow.SzacujParametry(_wybranyZasieg, piksel, BokSegmentu, korytarz, UkladWspolrzednych.PL1992,
                                                            _formatDocelowy, wielo, ZarzadcaSzablonow.LiczbaZrodel(zestaw)))
        Dim siatka As New Siatka(_wybranyZasieg, piksel, BokSegmentu)

        If wieloskalowa Then
            lblSzacunekSegmenty.Text = $"Mapa wieloskalowa: {szacunek.LiczbaSegmentow:N0} {ZarzadcaSzablonow.Odmiana(szacunek.LiczbaSegmentow, "segment", "segmenty", "segmentów")} w {plan.Count} {ZarzadcaSzablonow.Odmiana(plan.Count, "skali", "skalach", "skalach")}"
        ElseIf szacunek.CzyKorytarz Then
            lblSzacunekSegmenty.Text = $"Korytarz trasy: {szacunek.LiczbaSegmentow:N0} {ZarzadcaSzablonow.Odmiana(szacunek.LiczbaSegmentow, "segment", "segmenty", "segmentów")} (oszczędność {szacunek.OszczednoscProcent}% względem prostokąta)"
        Else
            lblSzacunekSegmenty.Text = $"Siatka: {szacunek.SiatkaKolumny} × {szacunek.SiatkaWiersze} = {szacunek.LiczbaSegmentow:N0} {ZarzadcaSzablonow.Odmiana(szacunek.LiczbaSegmentow, "segment", "segmenty", "segmentów")} ({szacunek.SzerokoscPx:N0} × {szacunek.WysokoscPx:N0} px)"
        End If
        lblSzacunekRozmiar.Text = $"Szacowany rozmiar pliku: {szacunek.SformatowanyPrzedzialRozmiaru()}"
        lblSzacunekCzas.Text = $"Orientacyjny czas: ok. {szacunek.SformatowanyCzas()} (pobieranie i zapis pliku)"

        If Not siatka.Poprawna Then
            lblSzacunekKomunikat.Text = "Nie można utworzyć mapy: " & siatka.PowodNiepoprawnosci & ". Wróć do kroku 3 i wybierz obszar."
            lblSzacunekKomunikat.ForeColor = Color.FromArgb(200, 30, 30)
        Else
            lblSzacunekKomunikat.Text = szacunek.Ostrzezenie
            Select Case szacunek.PoziomOstrzezenia
                Case 0 : lblSzacunekKomunikat.ForeColor = Color.FromArgb(0, 130, 40)
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
                rbWariantPojedynczy.Text = "Garmin KMZ - zalecany, działa na każdym Garminie"
                rbWariantWielopoziomowy.Text = "Garmin JNX (BirdsEye) - wiele skal"
            Case CelMapy.WydrukKomputer
                grpWariant.Text = "Format arkusza"
                rbWariantPojedynczy.Text = "Jeden arkusz GeoTIFF - zalecany do druku"
                rbWariantWielopoziomowy.Text = "Kafle MBTiles (przeglądarki kafelkowe)"
            Case CelMapy.ProjektCadGis
                grpWariant.Text = "Format podkładu CAD / GIS"
                rbWariantPojedynczy.Text = "Arkusz GeoTIFF w PL-1992 - zalecany"
                rbWariantWielopoziomowy.Text = "Piramida kafli MBTiles (QGIS, ArcGIS)"
        End Select

        ' Zakres powiększeń dopasowany do piksela (gdy użytkownik nie ustawił własnego)
        Dim wieloFormat = ZarzadcaSzablonow.CzyFormatWielopoziomowy(_formatDocelowy)
        If wieloskalowa AndAlso plan.Count > 0 Then
            ' zakres powiększeń wynika z planu stopni skali
            numZoomMax.Value = Math.Max(numZoomMax.Minimum, Math.Min(numZoomMax.Maximum, plan.Max(Function(ps) ps.PoziomMax)))
            numZoomMin.Value = Math.Max(numZoomMin.Minimum, Math.Min(numZoomMin.Maximum, plan.Min(Function(ps) ps.PoziomMin)))
        ElseIf Not chkWlasnyZoom.Checked Then
            Dim zakres = ZarzadcaSzablonow.ZalecanyZakresPowiekszen(piksel, SzerokoscGeoObszaru())
            numZoomMax.Value = Math.Max(numZoomMax.Minimum, Math.Min(numZoomMax.Maximum, zakres.Item2))
            numZoomMin.Value = Math.Max(numZoomMin.Minimum, Math.Min(numZoomMin.Maximum, zakres.Item1))
        End If
        chkWlasnyZoom.Enabled = wieloFormat AndAlso Not wieloskalowa
        numZoomMin.Enabled = wieloFormat AndAlso chkWlasnyZoom.Checked AndAlso Not wieloskalowa
        numZoomMax.Enabled = wieloFormat AndAlso chkWlasnyZoom.Checked AndAlso Not wieloskalowa

        lblFormatInfo.Text = "Format: " & ZarzadcaSzablonow.NazwaFormatu(_formatDocelowy) & If(wieloskalowa, " wieloskalowy", "") &
                             If(wieloFormat, $", powiększenia {numZoomMin.Value}-{numZoomMax.Value}", ", jeden poziom szczegółowości")

        ' Szczegółowość: skala wydruku, opis i ostrzeżenie przed pikselem dokładniejszym niż dane
        lblPikselJednostka.Text = $"m/px  -  jak wydruk w skali 1:{ZarzadcaSzablonow.SkalaWydruku(piksel):N0}; {ZarzadcaSzablonow.OpisSzczegolowosci(piksel)}"
        Dim najmniejszy = ZarzadcaSzablonow.NajmniejszyPrzydatnyPiksel(zestaw)
        If najmniejszy > 0 AndAlso piksel < najmniejszy - 0.0001 Then
            Dim zalecany = najmniejszy * 2
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
        Dim mbtiles = _formatDocelowy = RodzajEksportu.MBTiles
        btnOtworzWieloskalowy.Visible = mbtiles AndAlso Not wieloskalowa
        lblWieloskalowyOpis.Visible = mbtiles AndAlso Not wieloskalowa
        If mbtiles Then
            lblWieloskalowyOpis.Text = "Warstwy: " & OpisZestawu(zestaw) & vbCrLf & "Osobne warstwy dla zakresów powiększeń i mieszanie."
        End If

        UstawSciezkeDocelowa()
    End Sub

    Private Sub lnkPikselZalecany_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If TypeOf lnkPikselZalecany.Tag Is Double Then UstawPiksel(CDbl(lnkPikselZalecany.Tag))
    End Sub

    ''' <summary>Nazwa pliku z miejsca, stylu i daty (dawniej bez miejsca powstawało „Mapa_Mapa_…”).</summary>
    Private Function ProponowanaNazwaPliku() As String
        Dim czesci As New List(Of String) From {"Mapa"}
        Dim korytarz = PobierzEfektywnyKorytarz()
        If korytarz IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(korytarz.Nazwa) Then
            czesci.Add(OczyscNazwePliku(korytarz.Nazwa))
        ElseIf rbObszarMiejscowosc.Checked AndAlso _ostatnieMiejsce IsNot Nothing Then
            czesci.Add(OczyscNazwePliku(_ostatnieMiejsce.KrotkaNazwa))
        End If
        czesci.Add(If(_wybranySzablon IsNot Nothing, _wybranySzablon.Id, "kreator"))
        czesci.Add(DateTime.Now.ToString("yyyy-MM-dd"))
        Return String.Join("_", czesci)
    End Function

    ''' <summary>Ścieżka, pod którą nie ma jeszcze pliku ani folderu (dopisuje _2, _3…), żeby nie nadpisać poprzedniej mapy.</summary>
    Friend Shared Function UnikalnaSciezka(sciezka As String) As String
        If Not File.Exists(sciezka) AndAlso Not Directory.Exists(sciezka) Then Return sciezka
        Dim folder = Path.GetDirectoryName(sciezka)
        Dim nazwa = Path.GetFileNameWithoutExtension(sciezka)
        Dim ext = Path.GetExtension(sciezka)
        For i = 2 To 999
            Dim kandydat = Path.Combine(folder, $"{nazwa}_{i}{ext}")
            If Not File.Exists(kandydat) AndAlso Not Directory.Exists(kandydat) Then Return kandydat
        Next
        Return sciezka
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

    Private Function FormatujWymiaryZasiegu(z As Zasieg) As String
        If Not z.Poprawny Then Return "brak"
        Dim szerKm = Math.Round(z.Szerokosc / 1000.0, 1)
        Dim wysKm = Math.Round(z.Wysokosc / 1000.0, 1)
        Return szerKm & " km × " & wysKm & " km (" & (szerKm * wysKm).ToString("N0") & " km²)"
    End Function

    Private Shared Function OczyscNazwePliku(nazwa As String) As String
        If String.IsNullOrWhiteSpace(nazwa) Then Return "mapa"
        Dim niedozwolone = Path.GetInvalidFileNameChars()
        Dim czyste = New String(nazwa.Trim().Select(Function(c) If(c = " "c, "_"c, c)).Where(Function(c) Not niedozwolone.Contains(c)).ToArray())
        Return If(String.IsNullOrWhiteSpace(czyste), "mapa", czyste)
    End Function

#Region "Obsługa kroków i nawigacji"

    Private Sub rbCel_DoubleClick(sender As Object, e As EventArgs) Handles rbCelTelefon.DoubleClick, rbCelGarmin.DoubleClick, rbCelWydruk.DoubleClick, rbCelCadGis.DoubleClick
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
                Else
                    _wybranyCel = CelMapy.ProjektCadGis
                End If
                _ostatniCel = _wybranyCel
                UstawKrok(2)

            Case 2
                If _wybranySzablon Is Nothing Then
                    MessageBox.Show("Wybierz styl mapy z listy.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Sub
                End If
                ' Pusty zestaw dawniej przechodził dalej i kończył się błędem dopiero przy generowaniu
                If PobierzZestawStylu().Liczba = 0 Then
                    MessageBox.Show(If(CzyWlasnyZestaw(),
                                       "W oknie głównym nie wybrano żadnej warstwy." & vbCrLf & vbCrLf &
                                       "Wybierz gotowy styl z listy albo dodaj warstwy w oknie głównym (drzewo „Wybierz mapy””).",
                                       "Wybrany styl nie zawiera żadnej warstwy."),
                                    "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Sub
                End If
                UstawKrok(3)

            Case 3
                If Not _wybranyZasieg.Poprawny Then
                    Dim wskazowka As String
                    If rbObszarMiejscowosc.Checked Then
                        wskazowka = "Wpisz nazwę miejscowości, kliknij „Szukaj”” i wybierz wynik z listy."
                    ElseIf rbObszarTrasa.Checked Then
                        wskazowka = "Kliknij „Wskaż plik trasy…”” i wybierz plik GPX, KML lub KMZ."
                    Else
                        wskazowka = "Kliknij „Przejdź do mapy i zaznacz…”” i zaznacz prostokąt na mapie."
                    End If
                    Dim wzorzec = WzorcowyZasieg()
                    Dim nazwaStylu = If(_wybranySzablon IsNot Nothing, _wybranySzablon.Nazwa, "wybranego stylu")
                    Dim odp = MessageBox.Show("Nie wybrano obszaru mapy. " & wskazowka & vbCrLf & vbCrLf &
                                              $"Czy zamiast tego użyć przykładowego obszaru stylu „{nazwaStylu}”” ({FormatujWymiaryZasiegu(wzorzec)})?",
                                              "Wybór obszaru mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    If odp <> DialogResult.Yes OrElse Not wzorzec.Poprawny Then Exit Sub
                    _wybranyZasieg = wzorzec
                    _wybranyZasiegZMapy = wzorzec
                    rbObszarMapa.Checked = True
                    lblStatusZaznaczenia.Text = "Przykładowy obszar stylu: " & FormatujWymiaryZasiegu(_wybranyZasieg)
                    lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                End If
                UstawKrok(4)
        End Select
    End Sub

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
        Dim tenSam = _wybranySzablon IsNot Nothing AndAlso _wybranySzablon.Id = s.Id
        If Not tenSam Then
            _zestawNiestandardowy = Nothing
            _wlasnyPunktStylu = Nothing
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
        End If
        _ostatniSzablonId = s.Id
        PokazSzczegolySzablonu(s, tenSam)
    End Sub

    Private Sub lstSzablony_DoubleClick(sender As Object, e As EventArgs) Handles lstSzablony.DoubleClick
        If _wybranySzablon IsNot Nothing Then
            btnDalej.PerformClick()
        End If
    End Sub

    Private Function PobierzZrodlaKreatora() As Dictionary(Of String, PlikWarstw)
        If _zrodlaKreatora.Count = 0 AndAlso _form1 IsNot Nothing Then
            For Each kv In _form1.ZrodlaMapSesji()
                _zrodlaKreatora(kv.Key) = kv.Value
            Next
        End If
        ' Upewnij się, że pliki ze zbiorami map dla wszystkich szablonów są wczytane
        For Each szablon In ZarzadcaSzablonow.PobierzDomyslne()
            For Each w In szablon.Warstwy
                If Not String.IsNullOrEmpty(w.Zbior) AndAlso Not _zrodlaKreatora.ContainsKey(w.Zbior) Then
                    Dim plik = ZarzadcaSzablonow.WczytajPlikZbioru(w.Zbior)
                    If plik IsNot Nothing Then _zrodlaKreatora(w.Zbior) = plik
                End If
            Next
        Next
        Return _zrodlaKreatora
    End Function

    ''' <summary>Kopia zestawu warstw wybranego stylu z kryciem cieniowania ustawionym suwakiem.</summary>
    ''' <param name="piksel">Piksel pobierania - w stylu wieloskalowym wybiera stopień skali (0 - najdokładniejszy).</param>
    Private Function PobierzZestawStylu(Optional piksel As Double = 0) As ZestawWarstw
        Dim zestaw As ZestawWarstw
        If _zestawNiestandardowy IsNot Nothing Then
            zestaw = _zestawNiestandardowy.Kopia()
        ElseIf _wybranySzablon Is Nothing Then
            Return New ZestawWarstw()
        ElseIf CzyWlasnyZestaw() Then
            zestaw = If(_form1 IsNot Nothing, _form1.WybranyZestaw().Kopia(), New ZestawWarstw())
        Else
            zestaw = _wybranySzablon.UtworzZestaw(PobierzZrodlaKreatora(), piksel)
        End If
        ZastosujKrycieCieniowania(zestaw)
        Return zestaw
    End Function

    Private Function PobierzParametryProbkiStylu() As Tuple(Of PunktXY, Double)
        If _wybranySzablon Is Nothing Then
            Return Tuple.Create(New PunktXY(0, 0), 1.0)
        End If
        Dim zoom = _poziomyZoomu(_indeksZoomuStylu)
        If CzyWlasnyZestaw() AndAlso _form1 IsNot Nothing Then
            Dim pt = If(_wlasnyPunktStylu, UkladWspolrzednych.PL1992.ZWgs84(_form1.mapa.Position.Lat, _form1.mapa.Position.Lng))
            Dim px = Math.Max(0.1, _form1.PikselPodgladuBezSiatki())
            Dim effPx = Math.Max(0.05, px / zoom)
            Return Tuple.Create(pt, effPx)
        End If
        Dim p = If(_wlasnyPunktStylu, UkladWspolrzednych.PL1992.ZWgs84(_wybranySzablon.ProbkaLat, _wybranySzablon.ProbkaLon))
        Dim effPiksel = Math.Max(0.05, _wybranySzablon.ProbkaPiksel / zoom)
        Return Tuple.Create(p, effPiksel)
    End Function

    Private Function PobierzSkaleWyswietlaniaStylu() As Double
        If _bazaPodgladuStylu Is Nothing Then Return 1.0
        Dim scaledW = _bazaPodgladuStylu.Width * 1.0
        Dim scaledH = _bazaPodgladuStylu.Height * 1.0
        Dim celSzer = picPodgladStylu.Width
        Dim celWys = picPodgladStylu.Height
        If scaledW < celSzer OrElse scaledH < celWys Then
            Return Math.Max(celSzer / Math.Max(1.0, scaledW), celWys / Math.Max(1.0, scaledH))
        End If
        Return 1.0
    End Function

    ''' <summary>Skala pokazywana w podglądzie stylu wieloskalowego (np. „ - skala 1:50 000”).</summary>
    Private Function OpisSkaliPodgladu() As String
        If _wybranySzablon Is Nothing OrElse Not _wybranySzablon.Wieloskalowy OrElse _zestawNiestandardowy IsNot Nothing Then Return ""
        Dim st = _wybranySzablon.StopienDlaPiksela(_pikselPodgladuStylu)
        Return If(st Is Nothing, "", " - skala " & st.Opis)
    End Function

    Private Sub tbCieniowanie_Scroll(sender As Object, e As EventArgs) Handles tbCieniowanie.Scroll
        _krycieCieniowania = tbCieniowanie.Value
        lblCieniowanieWartosc.Text = _krycieCieniowania & "%"

        If _wybranySzablon IsNot Nothing Then
            ' Błyskawiczne złożenie obrazu z warstw w buforze (bez ponownego pobierania)
            Dim zestaw = PobierzZestawStylu(_pikselPodgladuStylu)
            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = _silnikPodgladuStylu.ZlozObraz(zestaw)
            staraBaza?.Dispose()
            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = If(_bazaPodgladuStylu IsNot Nothing, New Bitmap(_bazaPodgladuStylu), Nothing)
            ZastosujZoomStylu()
        End If
    End Sub

    Private Sub btnDostosujWarstwy_Click(sender As Object, e As EventArgs) Handles btnDostosujWarstwy.Click
        If _wybranySzablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        Dim zestaw = PobierzZestawStylu(_pikselPodgladuStylu)

        Dim parametry = PobierzParametryProbkiStylu()
        Dim punkt = parametry.Item1
        Dim probkaPiksel = parametry.Item2
        Dim pol = _silnikPodgladuStylu.Bok * probkaPiksel / 2.0
        Dim zasiegProbki As New Zasieg(punkt.X - pol, punkt.Y - pol, punkt.X + pol, punkt.Y + pol)
        Dim siatkaProbki As New Siatka(zasiegProbki, probkaPiksel, _silnikPodgladuStylu.Bok)

        Dim zadanie As New ZadaniePobierania With {
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatkaProbki,
            .Zbiory = zestaw.Zbiory(),
            .ZrodlaMap = PobierzZrodlaKreatora(),
            .Format = "png",
            .LimitCzasuSekundy = 15,
            .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_warstwy") & "\"
        }

        Using okno As New FormWarstwyZaawansowane(zestaw, zadanie, 0, _silnikPodgladuStylu)
            If okno.ShowDialog(Me) = DialogResult.OK Then
                _zestawNiestandardowy = zestaw
                Dim w = ZarzadcaSzablonow.ZnajdzWarstweCieniowania(zestaw)
                If w IsNot Nothing Then _krycieCieniowania = w.Krycie
                PokazSzczegolySzablonu(_wybranySzablon, True)
            End If
        End Using
    End Sub

    Private Async Sub OdswiezPodgladStyluAsync(Optional wymus As Boolean = False)
        If _wybranySzablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        _ctsPodgladStylu?.Cancel()
        _ctsPodgladStylu?.Dispose()
        _ctsPodgladStylu = New CancellationTokenSource()
        Dim token = _ctsPodgladStylu.Token

        lblStanPodgladuStyl.Text = "Pobieranie próbki terenu..."
        lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 100, 180)

        _pikselPodgladuStylu = PobierzParametryProbkiStylu().Item2
        Dim zestaw = PobierzZestawStylu(_pikselPodgladuStylu)
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then
            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = Nothing
            staraBaza?.Dispose()
            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = Nothing
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
            ZastosujZoomStylu()
            lblStanPodgladuStyl.Text = If(CzyWlasnyZestaw(), "Brak warstw w oknie głównym", "Brak warstw w zestawie")
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
            staraBaza?.Dispose()
            _ostatniaProbkaBmp?.Dispose()
            _ostatniaProbkaBmp = If(_bazaPodgladuStylu IsNot Nothing, New Bitmap(_bazaPodgladuStylu), Nothing)
            _deltaPrzeciaganiaStylu = Point.Empty
            _bazaPrzeciaganiaStylu = Point.Empty
            ZastosujZoomStylu()
            lblStanPodgladuStyl.Text = "Podgląd gotowy" & OpisSkaliPodgladu()
            lblStanPodgladuStyl.ForeColor = Color.FromArgb(0, 130, 40)
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
                .ZrodlaMap = PobierzZrodlaKreatora(),
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
            If token.IsCancellationRequested OrElse IsDisposed Then Exit Sub

            Dim staraBaza = _bazaPodgladuStylu
            _bazaPodgladuStylu = wynik.Item1
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
                lblStanPodgladuStyl.Text = "Podgląd gotowy" & OpisSkaliPodgladu()
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
        picPodgladStylu.Focus()
        picPodgladStylu.Cursor = Cursors.Hand
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
                Dim skalaWyswietlania = PobierzSkaleWyswietlaniaStylu()
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
        _ctsWyszukiwania = New CancellationTokenSource()
        Dim token = _ctsWyszukiwania.Token

        btnSzukajMiejsca.Enabled = False
        lblWynikMiejscowosci.Text = "Szukanie: " & fraza & "..."
        lblWynikMiejscowosci.ForeColor = Color.FromArgb(60, 60, 60)

        Try
            Dim wyniki = Await KlientGeokodowania.SzukajMiejscaAsync(fraza, token)
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
                lblWynikMiejscowosci.Text = "Nie znaleziono pasujących miejscowości dla podanej frazy."
                lblWynikMiejscowosci.ForeColor = Color.FromArgb(180, 50, 0)
            End If
        Catch ex As OperationCanceledException
        Catch ex As ObjectDisposedException
        Catch ex As Exception
            If IsDisposed Then Exit Sub
            lblWynikMiejscowosci.Text = "Błąd wyszukiwania: " & OpisBledu.Z(ex)
            lblWynikMiejscowosci.ForeColor = Color.Red
        Finally
            If Not IsDisposed Then btnSzukajMiejsca.Enabled = True
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
            TextRenderer.DrawText(e.Graphics, SformatujLokalizacje(m), fLokalizacja, New Rectangle(e.Bounds.X + 8, e.Bounds.Y + 18, e.Bounds.Width - 12, 15),
                                  Color.FromArgb(85, 85, 85), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End Using
    End Sub

    Private Function SformatujLokalizacje(m As WynikGeokodowania) As String
        If m Is Nothing OrElse String.IsNullOrWhiteSpace(m.PelnaNazwa) Then Return ""
        Dim segmenty = m.PelnaNazwa.Split(","c).Select(Function(s) s.Trim()).ToList()
        ' Odrzuć "Polska" i kody pocztowe na końcu
        While segmenty.Count > 1 AndAlso (segmenty.Last().Equals("Polska", StringComparison.OrdinalIgnoreCase) OrElse
                                          Regex.IsMatch(segmenty.Last(), "^\d{2}-\d{3}$"))
            segmenty.RemoveAt(segmenty.Count - 1)
        End While
        If segmenty.Count <= 1 Then Return m.PelnaNazwa
        ' Pomiń pierwszy człon (bo to sama miejscowość z linii 1)
        Return String.Join(", ", segmenty.Skip(1))
    End Function

    ' Każdy sposób wyboru obszaru używa wyłącznie własnego zasięgu - dawniej przełączenie na „Miejscowość” bez wyniku
    ' (albo anulowany wybór trasy) zostawiało zasięg z mapy i kreator pobierał obszar, którego użytkownik już nie wybierał

    Private Sub rbObszarMapa_CheckedChanged(sender As Object, e As EventArgs) Handles rbObszarMapa.CheckedChanged
        If rbObszarMapa.Checked Then
            _wlasnyPunktObszaru = Nothing
            _deltaPrzeciaganiaObszaru = Point.Empty
            If Not _wybranyZasiegZMapy.Poprawny AndAlso _form1 IsNot Nothing Then
                Dim z = _form1.ZasiegPolXY()
                If Not z.Poprawny Then
                    Dim s = _form1.BiezacaSiatka()
                    If s IsNot Nothing AndAlso s.Poprawna Then z = s.ZasiegSiatki
                End If
                If z.Poprawny Then
                    _wybranyZasiegZMapy = z
                    lblStatusZaznaczenia.Text = "Aktualny zasięg z mapy: " & FormatujWymiaryZasiegu(z)
                    lblStatusZaznaczenia.ForeColor = Color.FromArgb(0, 120, 50)
                End If
            End If
            _wybranyZasieg = _wybranyZasiegZMapy
            AktualizujPodgladObszaru()
        End If
    End Sub

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

        ' Promień w metrach
        Dim promienMetry As Double = 5000.0
        Dim promienTekst = TryCast(cboPromien.SelectedItem, String)
        If String.IsNullOrEmpty(promienTekst) Then promienTekst = cboPromien.Text
        If Not String.IsNullOrEmpty(promienTekst) Then
            Dim match = Regex.Match(promienTekst, "\d+")
            If match.Success Then
                promienMetry = Math.Max(500.0, CDbl(match.Value) * 1000.0)
            End If
        End If

        Dim srodek1992 = UkladWspolrzednych.PL1992.ZWgs84(m.Szerokosc, m.Dlugosc)
        _wybranyZasiegMiejscowosci = New Zasieg(srodek1992.X - promienMetry, srodek1992.Y - promienMetry,
                                                srodek1992.X + promienMetry, srodek1992.Y + promienMetry)

        Dim admin = SformatujLokalizacje(m)
        lblWynikMiejscowosci.Text = "Wybrano: " & m.KrotkaNazwa & If(admin <> "", " (" & admin & ")", "") & " - obszar " & FormatujWymiaryZasiegu(_wybranyZasiegMiejscowosci)
        lblWynikMiejscowosci.ForeColor = Color.FromArgb(0, 120, 50)

        If rbObszarMiejscowosc.Checked Then
            _wybranyZasieg = _wybranyZasiegMiejscowosci
            AktualizujPodgladObszaru()
        Else
            rbObszarMiejscowosc.Checked = True
        End If
    End Sub

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
        If _form1.WindowState = FormWindowState.Minimized Then
            _form1.WindowState = FormWindowState.Normal
        End If
        _form1.Show()
        _form1.Activate()
        _form1.BringToFront()
        SetForegroundWindow(_form1.Handle)

        Me.Hide()

        Dim pasek As New FormPasekZaznaczania(_form1)
        AddHandler pasek.Zatwierdzono, Sub()
                                           Dim z = _form1.ZasiegPolXY()
                                           If Not z.Poprawny Then
                                               Dim s = _form1.BiezacaSiatka()
                                               If s IsNot Nothing AndAlso s.Poprawna Then z = s.ZasiegSiatki
                                           End If
                                           If z.Poprawny Then
                                               _wybranyZasiegZMapy = z
                                               lblStatusZaznaczenia.Text = "Zaznaczono na mapie: " & FormatujWymiaryZasiegu(z)
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
                                                     _form1.Activate()
                                                 End If
                                                 Exit Sub
                                             End If
                                             _form1.Enabled = False ' Przywróć blokadę modalną dla okna kreatora
                                             Me.Visible = True
                                             If Me.WindowState = FormWindowState.Minimized Then
                                                 Me.WindowState = FormWindowState.Normal
                                             End If
                                             Me.BringToFront()
                                             Me.Activate()
                                             SetForegroundWindow(Me.Handle)
                                         Catch ex As Exception
                                             If _form1 IsNot Nothing AndAlso Not _form1.IsDisposed Then
                                                 _form1.Enabled = True
                                                 _form1.Activate()
                                             End If
                                         End Try
                                     End Sub
        pasek.Show(_form1)
    End Sub

    Private Sub btnWybierzTrase_Click(sender As Object, e As EventArgs) Handles btnWybierzTrase.Click
        Using dlg As New OpenFileDialog()
            dlg.Title = "Wskaż plik ze śladem trasy (GPX, KML, KMZ)"
            dlg.Filter = "Pliki tras GPS (*.gpx;*.kml;*.kmz)|*.gpx;*.kml;*.kmz|Pliki GPX (*.gpx)|*.gpx|Pliki KML/KMZ (*.kml;*.kmz)|*.kml;*.kmz|Wszystkie pliki (*.*)|*.*"
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _wlasnyPunktObszaru = Nothing
                _deltaPrzeciaganiaObszaru = Point.Empty
                Try
                    Dim k As Korytarz = Korytarz.WczytajTrase(dlg.FileName)
                    If k IsNot Nothing AndAlso k.Poprawny Then
                        k.SzerokoscMetry = CDbl(numKorytarz.Value)
                        _wybranyKorytarz = k
                        _wybranyZasiegTrasy = k.Zasieg(UkladWspolrzednych.PL1992)
                        _ostatnieMiejsce = Nothing
                        _sciezkaZmienionaPrzezUzytkownika = False
                        Dim nazwaTrasy = If(Not String.IsNullOrEmpty(k.Nazwa), k.Nazwa, Path.GetFileNameWithoutExtension(dlg.FileName))
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
            End If
        End Using
    End Sub

    Private Sub numKorytarz_ValueChanged(sender As Object, e As EventArgs) Handles numKorytarz.ValueChanged
        If _wybranyKorytarz IsNot Nothing Then
            _wybranyKorytarz.SzerokoscMetry = CDbl(numKorytarz.Value)
            _wybranyZasiegTrasy = _wybranyKorytarz.Zasieg(UkladWspolrzednych.PL1992)
            lblStatusTrasy.Text = $"Korytarz: pas {numKorytarz.Value} m po obu stronach trasy - zasięg {FormatujWymiaryZasiegu(_wybranyZasiegTrasy)}"
            If rbObszarTrasa.Checked Then
                _wybranyZasieg = _wybranyZasiegTrasy
                AktualizujPodgladObszaru()
            End If
        End If
    End Sub

    ''' <summary>Przykładowy obszar stylu wokół punktu próbki (1,5 km dla katastru, 8 km dla map przeglądowych, inaczej 4 km).</summary>
    Private Function WzorcowyZasieg() As Zasieg
        If _wybranySzablon Is Nothing Then Return New Zasieg()
        Dim srodek = UkladWspolrzednych.PL1992.ZWgs84(_wybranySzablon.ProbkaLat, _wybranySzablon.ProbkaLon)
        Dim szerMetry = If(_wybranySzablon.DomyslnyPiksel <= 0.5, 1500.0, If(_wybranySzablon.DomyslnyPiksel >= 4.0, 8000.0, 4000.0))
        Dim pol = szerMetry / 2.0
        Return New Zasieg(srodek.X - pol, srodek.Y - pol, srodek.X + pol, srodek.Y + pol)
    End Function

    Private Function PobierzEfektywnyZasiegObszaru() As Tuple(Of Zasieg, Boolean)
        If _wybranyZasieg.Poprawny Then
            Return Tuple.Create(_wybranyZasieg, False)
        End If
        If _wybranySzablon IsNot Nothing Then
            Return Tuple.Create(WzorcowyZasieg(), True)
        End If
        Return Tuple.Create(New Zasieg(), True)
    End Function

    Private Sub AktualizujPodgladObszaru()
        Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
        Dim zasieg = infoZasiegu.Item1
        Dim czyWzorcowy = infoZasiegu.Item2

        If zasieg.Poprawny Then
            Dim szerKm = Math.Round(zasieg.Szerokosc / 1000.0, 1)
            Dim wysKm = Math.Round(zasieg.Wysokosc / 1000.0, 1)
            Dim powKm = Math.Round(szerKm * wysKm, 1)

            If czyWzorcowy Then
                lblObszarWymiary.Text = "Nie wybrano jeszcze obszaru." & vbCrLf & "Podgląd pokazuje przykład stylu."
                lblObszarWspolrzedne.Text = $"Przykładowy fragment: {szerKm} km × {wysKm} km" & vbCrLf &
                                            "Wybierz obszar jednym ze sposobów po lewej."
            ElseIf _wybranyKorytarz IsNot Nothing AndAlso rbObszarTrasa.Checked Then
                lblObszarWymiary.Text = $"Korytarz trasy: {_wybranyKorytarz.DlugoscKm:0.0} km (pas {_wybranyKorytarz.SzerokoscMetry:0} m)" & vbCrLf &
                                        $"Powierzchnia prostokąta: {powKm} km²"
                lblObszarWspolrzedne.Text = $"Ślad: {_wybranyKorytarz.LiczbaPunktow} punktów w {_wybranyKorytarz.Linie.Count} liniach" & vbCrLf &
                                            $"Zasięg: {szerKm} km × {wysKm} km" & vbCrLf &
                                            "Układ: PL-1992 (EPSG:2180)"
            Else
                lblObszarWymiary.Text = $"Wymiary: {szerKm} km × {wysKm} km" & vbCrLf & $"Powierzchnia: {powKm} km²"
                Dim wgsMin = UkladWspolrzednych.PL1992.DoWgs84(zasieg.XDol, zasieg.YLewy)
                Dim wgsMax = UkladWspolrzednych.PL1992.DoWgs84(zasieg.XGora, zasieg.YPrawy)
                lblObszarWspolrzedne.Text = $"N: {wgsMin.Szerokosc:0.000}° - {wgsMax.Szerokosc:0.000}°" & vbCrLf &
                                            $"E: {wgsMin.Dlugosc:0.000}° - {wgsMax.Dlugosc:0.000}°" & vbCrLf &
                                            "Układ: PL-1992 (EPSG:2180)"
            End If
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

    Private Function PobierzSkaleWyswietlaniaObszaru() As Double
        If _bazaPodgladuObszaru Is Nothing Then Return 1.0
        Dim scaledW = _bazaPodgladuObszaru.Width * 1.0
        Dim scaledH = _bazaPodgladuObszaru.Height * 1.0
        Dim celSzer = picPodgladObszaru.Width
        Dim celWys = picPodgladObszaru.Height
        If scaledW < celSzer OrElse scaledH < celWys Then
            Return Math.Max(celSzer / Math.Max(1.0, scaledW), celWys / Math.Max(1.0, scaledH))
        End If
        Return 1.0
    End Function

    Private Async Sub AktualizujPodgladObszaruAsync(Optional wymus As Boolean = False)
        Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
        Dim zasieg = infoZasiegu.Item1
        Dim czyWzorcowy = infoZasiegu.Item2

        If Not zasieg.Poprawny OrElse _wybranySzablon Is Nothing OrElse _form1 Is Nothing Then Exit Sub

        _ctsPodgladObszaru?.Cancel()
        _ctsPodgladObszaru?.Dispose()
        _ctsPodgladObszaru = New CancellationTokenSource()
        Dim token = _ctsPodgladObszaru.Token

        lblStanPodgladuObszar.Text = If(czyWzorcowy, "Pobieranie przykładu...", "Pobieranie mapy obszaru...")
        lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 100, 180)

        Dim zoom = _poziomyZoomu(_indeksZoomuObszaru)
        Dim punkt = If(_wlasnyPunktObszaru, zasieg.Srodek)
        Dim bazowyPiksel = Math.Max(_wybranySzablon.DomyslnyPiksel, Math.Max(zasieg.Szerokosc, zasieg.Wysokosc) / _silnikPodgladuObszaru.Bok)
        Dim piksel = Math.Max(0.05, bazowyPiksel / zoom)
        ' Styl wieloskalowy: duży obszar w podglądzie pokazuje mapę w mniejszej skali, jak plik MBTiles po oddaleniu
        Dim zestaw = PobierzZestawStylu(piksel)
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Exit Sub
        Dim zmieniono = _silnikPodgladuObszaru.SprawdzIZaktualizujParametry(punkt, piksel, UkladWspolrzednych.PL1992, wymus)

        If Not zmieniono AndAlso zestaw.Warstwy.All(Function(w) _silnikPodgladuObszaru.CzyWarstwaWBuforze(w)) Then
            Dim staraBaza = _bazaPodgladuObszaru
            _bazaPodgladuObszaru = _silnikPodgladuObszaru.ZlozObraz(zestaw)
            staraBaza?.Dispose()
            _deltaPrzeciaganiaObszaru = Point.Empty
            _bazaPrzeciaganiaObszaru = Point.Empty
            ZastosujZoomObszaru()
            lblStanPodgladuObszar.Text = If(czyWzorcowy, "Przykład stylu", "Podgląd gotowy")
            lblStanPodgladuObszar.ForeColor = Color.FromArgb(0, 130, 40)
            Exit Sub
        End If

        Try
            Dim zasiegDoPobrania = _silnikPodgladuObszaru.ZasiegProbki().GetValueOrDefault(zasieg)
            Dim siatkaObszaru As New Siatka(zasiegDoPobrania, piksel, _silnikPodgladuObszaru.Bok)
            Dim zadanie As New ZadaniePobierania With {
                .Uklad = UkladWspolrzednych.PL1992,
                .Siatka = siatkaObszaru,
                .Zbiory = zestaw.Zbiory(),
                .ZrodlaMap = PobierzZrodlaKreatora(),
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
            If token.IsCancellationRequested OrElse IsDisposed Then Exit Sub

            Dim staraBaza = _bazaPodgladuObszaru
            _bazaPodgladuObszaru = wynik.Item1
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

        Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()

        ' Narysuj na podglądzie ślad trasy jeśli wybrano korytarz
        If _wybranyKorytarz IsNot Nothing AndAlso rbObszarTrasa.Checked Then
            Using g = Graphics.FromImage(powiekszony)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Dim marg = 15
                Dim wRect = picPodgladObszaru.Width - 2 * marg
                Dim hRect = picPodgladObszaru.Height - 2 * marg
                Using pTrasa As New Pen(Color.FromArgb(230, 210, 20, 20), 2.5!)
                    Dim zas = infoZasiegu.Item1
                    For Each linia In _wybranyKorytarz.Linie
                        If linia.Count < 2 Then Continue For
                        Dim punktyPix As New List(Of PointF)()
                        For Each pt In linia
                            Dim xy = UkladWspolrzednych.PL1992.ZWgs84(pt.Szerokosc, pt.Dlugosc)
                            Dim px = CSng(marg + (xy.Y - zas.YLewy) / zas.Szerokosc * wRect + _deltaPrzeciaganiaObszaru.X)
                            Dim py = CSng(marg + (zas.XGora - xy.X) / zas.Wysokosc * hRect + _deltaPrzeciaganiaObszaru.Y)
                            punktyPix.Add(New PointF(px, py))
                        Next
                        If punktyPix.Count >= 2 Then
                            g.DrawLines(pTrasa, punktyPix.ToArray())
                        End If
                    Next
                End Using
            End Using
        End If

        Dim staryImg = picPodgladObszaru.Image
        picPodgladObszaru.Image = powiekszony
        staryImg?.Dispose()

        lblZoomObszar.Text = If(Math.Abs(zoom - 1.0) < 0.01, "1:1", "×" & zoom.ToString("0.0"))
        btnZoomMinusObszar.Enabled = (_indeksZoomuObszaru > 0)
        btnZoomPlusObszar.Enabled = (_indeksZoomuObszaru < _poziomyZoomu.Length - 1)
        btnZoomResetObszar.Enabled = (_indeksZoomuObszaru <> IndeksZoomu1do1 OrElse _wlasnyPunktObszaru.HasValue)
    End Sub

    Private Sub picPodgladObszaru_MouseEnter(sender As Object, e As EventArgs) Handles picPodgladObszaru.MouseEnter
        picPodgladObszaru.Focus()
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

            If (Math.Abs(dx) > 2 OrElse Math.Abs(dy) > 2) AndAlso _wybranySzablon IsNot Nothing Then
                Dim infoZasiegu = PobierzEfektywnyZasiegObszaru()
                Dim zasieg = infoZasiegu.Item1
                Dim zoom = _poziomyZoomu(_indeksZoomuObszaru)
                Dim bazowyPiksel = Math.Max(_wybranySzablon.DomyslnyPiksel, Math.Max(zasieg.Szerokosc, zasieg.Wysokosc) / _silnikPodgladuObszaru.Bok)
                Dim effPiksel = Math.Max(0.05, bazowyPiksel / zoom)
                Dim skalaWyswietlania = PobierzSkaleWyswietlaniaObszaru()
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
        End If
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

    Private Sub chkKorektaObrazu_CheckedChanged(sender As Object, e As EventArgs) Handles chkKorektaObrazu.CheckedChanged
        Dim wl = chkKorektaObrazu.Checked
        _opcjeKorekty.Wlaczona = wl
        lblProfil.Enabled = wl
        cboProfilKorekty.Enabled = wl
        btnDostosujKorekte.Enabled = wl
    End Sub

    Private Sub cboProfilKorekty_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboProfilKorekty.SelectedIndexChanged
        Dim i = cboProfilKorekty.SelectedIndex
        If i < 0 OrElse i >= _profileListy.Length Then Exit Sub
        _opcjeKorekty.UstawWgProfilu(_profileListy(i))
        _opcjeKorekty.Wlaczona = chkKorektaObrazu.Checked
        btnDostosujKorekte.Text = "Dostosuj suwaki..."
    End Sub

    Private Sub btnDostosujKorekte_Click(sender As Object, e As EventArgs) Handles btnDostosujKorekte.Click
        Using dlg As New FormPodgladMip(_opcjeKorekty, _ostatniaProbkaBmp)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeKorekty = dlg.Opcje.Kopia()
                chkKorektaObrazu.Checked = _opcjeKorekty.Wlaczona
                btnDostosujKorekte.Text = "Dostosuj suwaki (własne)"
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
        Dim zestaw = PobierzZestawStylu()
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
                _zestawNiestandardowy = zestaw
                Dim nowyCien = ZarzadcaSzablonow.ZnajdzWarstweCieniowania(zestaw)
                If nowyCien IsNot Nothing Then _krycieCieniowania = nowyCien.Krycie
                PokazSzczegolySzablonu(_wybranySzablon, True)

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
        Dim nazwa = "Kreator_" & OczyscNazwePliku(Path.GetFileNameWithoutExtension(plikDocelowy)) & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss")
        Return Path.Combine(baza, nazwa) & "\"
    End Function

    ''' <summary>Czy kreator utworzy mapę wieloskalową (osobne pobranie każdego stopnia skali i kompilacja MBTiles).</summary>
    Private Function CzyWieloskalowaAktywna() As Boolean
        Return _wybranySzablon IsNot Nothing AndAlso _wybranySzablon.Wieloskalowy AndAlso _zestawNiestandardowy Is Nothing AndAlso
               _formatDocelowy = RodzajEksportu.MBTiles AndAlso chkWieloskalowa.Checked
    End Function

    Private Function SzerokoscGeoObszaru() As Double
        If Not _wybranyZasieg.Poprawny Then Return 52.0
        Return UkladWspolrzednych.PL1992.DoWgs84(_wybranyZasieg.Srodek.X, _wybranyZasieg.Srodek.Y).Szerokosc
    End Function

    Private Function PlanBiezacy() As List(Of PlanStopnia)
        Return ZarzadcaSzablonow.PlanStopni(_wybranySzablon, CDbl(numRozmiarPiksela.Value), SzerokoscGeoObszaru())
    End Function

    Private Function UtworzZadaniePobierania(zestaw As ZestawWarstw, siatka As Siatka, folder As String, korytarz As Korytarz) As ZadaniePobierania
        If korytarz IsNot Nothing Then korytarz.Zapisz(Path.Combine(folder, Korytarz.NazwaPliku))
        Return New ZadaniePobierania With {
            .AdresSerwera = zestaw(0).Adres,
            .Warstwy = zestaw.Nazwy(),
            .Krycia = zestaw.Krycia(),
            .Adresy = zestaw.Adresy(),
            .Zbiory = zestaw.Zbiory(),
            .ZrodlaMap = PobierzZrodlaKreatora(),
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatka,
            .Format = If(zestaw.CzySameNakladki() OrElse zestaw.Warstwy.Any(Function(w) w.Krycie < 100), "png", "jpg"),
            .Prefiks = "seg",
            .Folder = folder,
            .Korytarz = korytarz,
            .LiczbaWatkow = 4,
            .LimitCzasuSekundy = 25}
    End Function

    Private Async Sub btnGeneruj_Click(sender As Object, e As EventArgs) Handles btnGeneruj.Click
        Dim rozszerzenie = ZarzadcaSzablonow.RozszerzenieDlaRodzaju(_formatDocelowy)
        Dim plikDocelowy = txtSciezkaDocelowa.Text.Trim()
        If String.IsNullOrWhiteSpace(plikDocelowy) Then
            MessageBox.Show("Wskaż ścieżkę pliku docelowego.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            plikDocelowy = Path.GetFullPath(Path.ChangeExtension(plikDocelowy, rozszerzenie))
            Dim folderZapisu = Path.GetDirectoryName(plikDocelowy)
            If Not String.IsNullOrEmpty(folderZapisu) AndAlso Not Directory.Exists(folderZapisu) Then
                Directory.CreateDirectory(folderZapisu)
            End If
        Catch ex As Exception
            MessageBox.Show("Nie można zapisać pliku w tym miejscu: " & OpisBledu.Z(ex), "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End Try

        If File.Exists(plikDocelowy) OrElse Directory.Exists(plikDocelowy) Then
            If _sciezkaZmienionaPrzezUzytkownika Then
                If MessageBox.Show($"Plik {Path.GetFileName(plikDocelowy)} już istnieje. Zastąpić go nową mapą?", "Kreator mapy",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            Else
                plikDocelowy = UnikalnaSciezka(plikDocelowy)
            End If
        End If
        txtSciezkaDocelowa.Text = plikDocelowy

        Dim piksel = CDbl(numRozmiarPiksela.Value)
        Dim siatka As New Siatka(_wybranyZasieg, piksel, BokSegmentu)
        If Not siatka.Poprawna OrElse siatka.ZbytDuza Then
            MessageBox.Show("Nie można utworzyć mapy dla tego obszaru: " & If(siatka.ZbytDuza, "obszar jest zbyt duży", siatka.PowodNiepoprawnosci) & ".",
                            "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim korytarz = PobierzEfektywnyKorytarz()
        Dim zestaw = PobierzZestawStylu(piksel)
        If zestaw.Liczba = 0 Then
            MessageBox.Show("Wybrany styl nie zawiera żadnej warstwy. Wróć do kroku 2 i wybierz styl mapy.", "Kreator mapy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        Dim wieloskalowa = CzyWieloskalowaAktywna()
        Dim plan = If(wieloskalowa, PlanBiezacy(), Nothing)
        If wieloskalowa AndAlso plan.Count < 2 Then wieloskalowa = False

        ' Pytanie o duży obszar - te same progi i ten sam format, co szacunek w kroku 4
        Dim szacunek = If(wieloskalowa,
                          ZarzadcaSzablonow.SzacujWieloskalowo(plan, _wybranyZasieg, korytarz, UkladWspolrzednych.PL1992, ZarzadcaSzablonow.LiczbaZrodel(zestaw)),
                          ZarzadcaSzablonow.SzacujParametry(_wybranyZasieg, piksel, BokSegmentu, korytarz, UkladWspolrzednych.PL1992,
                                                            _formatDocelowy, rbWariantWielopoziomowy.Checked, ZarzadcaSzablonow.LiczbaZrodel(zestaw)))
        If szacunek.WymagaPotwierdzenia Then
            Dim odp = MessageBox.Show(szacunek.Ostrzezenie & vbCrLf & vbCrLf &
                                      $"Segmenty: {szacunek.LiczbaSegmentow:N0}, plik ok. {szacunek.SformatowanyPrzedzialRozmiaru()}, czas ok. {szacunek.SformatowanyCzas()}." & vbCrLf & vbCrLf &
                                      "Czy mimo to rozpocząć pobieranie?",
                                      "Duży obszar mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If odp <> DialogResult.Yes Then Exit Sub
        End If

        ' Wolne miejsce: segmenty sesji i plik wynikowy zajmują razem ok. 2 razy szacowany rozmiar pliku
        Try
            Dim dysk As New DriveInfo(Path.GetPathRoot(plikDocelowy))
            Dim potrzebneB = CLng(szacunek.SzacowanyRozmiarMbMax * 2.0 * 1024 * 1024)
            If dysk.IsReady AndAlso dysk.AvailableFreeSpace < potrzebneB Then
                If MessageBox.Show($"Na dysku {dysk.Name} jest {dysk.AvailableFreeSpace / 1024.0 / 1024 / 1024:0.0} GB wolnego miejsca, a mapa może potrzebować ok. {potrzebneB / 1024.0 / 1024 / 1024:0.0} GB." & vbCrLf & vbCrLf &
                                   "Czy mimo to kontynuować?", "Mało miejsca na dysku", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            End If
        Catch ex As Exception
            'dysk sieciowy lub nietypowa ścieżka - pomijamy sprawdzenie
        End Try

        Dim folderSesji = FolderSesjiKreatora(plikDocelowy)
        Directory.CreateDirectory(folderSesji)
        _folderWygenerowanejSesji = folderSesji
        _zadaniaPobierania.Clear()
        _zadanieEksportu = Nothing
        _opcjeKompilacji = Nothing
        _planGenerowania = Nothing
        Dim korekta = If(chkKorektaObrazu.Checked, _opcjeKorekty.Kopia(), New OpcjeKorekcjiMip With {.Wlaczona = False})

        If wieloskalowa Then
            ' Każdy stopień skali to osobna sesja (najdokładniejszy w głównym folderze, pozostałe obok: _skala2, _skala3…)
            _planGenerowania = plan
            For i = 0 To plan.Count - 1
                Dim folder = If(i = 0, folderSesji, folderSesji.TrimEnd("\"c) & "_skala" & (i + 1) & "\")
                Directory.CreateDirectory(folder)
                Dim zestawStopnia = _wybranySzablon.UtworzZestaw(PobierzZrodlaKreatora(), plan(i).Stopien)
                ZastosujKrycieCieniowania(zestawStopnia)
                _zadaniaPobierania.Add(UtworzZadaniePobierania(zestawStopnia, New Siatka(_wybranyZasieg, plan(i).Piksel, BokSegmentu), folder, korytarz))
            Next
            _opcjeKompilacji = New OpcjeKompilatoraMbtiles With {
                .PlikDocelowy = plikDocelowy,
                .NazwaMapy = Path.GetFileNameWithoutExtension(plikDocelowy),
                .OpisMapy = _wybranySzablon.Nazwa & " - mapa wieloskalowa: " & _wybranySzablon.OpisSkal(),
                .Atrybucja = "MapoTero - " & String.Join(", ", _wybranySzablon.StopnieSkali.SelectMany(Function(st) st.Warstwy).Select(Function(w) If(w.NazwaWyswietlana <> "", w.NazwaWyswietlana, w.Warstwa)).Distinct()),
                .PoziomMin = plan.Min(Function(ps) ps.PoziomMin),
                .PoziomMax = plan.Max(Function(ps) ps.PoziomMax),
                .Korekta = korekta}
        Else
            _zadaniaPobierania.Add(UtworzZadaniePobierania(zestaw, siatka, folderSesji, korytarz))
            Dim z = _zadaniaPobierania(0)
            _zadanieEksportu = New ZadanieEksportu With {
                .Zrodlo = New ZrodloSegmentow(folderSesji, z.Prefiks, z.Numeracja, z.Format, siatka),
                .Uklad = UkladWspolrzednych.PL1992,
                .Rodzaj = _formatDocelowy,
                .Plik = plikDocelowy,
                .Nazwa = Path.GetFileNameWithoutExtension(plikDocelowy),
                .PoziomMin = CInt(numZoomMin.Value),
                .PoziomMax = CInt(numZoomMax.Value),
                .FormatArkusza = FormatArkusza.GeoTiffJpeg,
                .PodzialNaTomy = chkPodzialTomow.Checked,
                .KolejnoscRysowania = CInt(numDrawOrder.Value)}
            If chkKorektaObrazu.Checked Then _zadanieEksportu.KorekcjaMip = korekta
        End If

        _sciezkaWygenerowanegoPliku = plikDocelowy
        Await UruchomGenerowanieAsync()
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

        Try
            For i = 0 To liczbaZadan - 1
                Dim etap = If(liczbaZadan > 1, $"skala {i + 1} z {liczbaZadan} ({_planGenerowania(i).Stopien.Opis}): ", "")
                lblStanPostepu.Text = "Pobieranie mapy z serwerów" & If(liczbaZadan > 1, $" - skala {_planGenerowania(i).Stopien.Opis}", "") & "..."
                Dim postepPobierania As New Progress(Of PostepPobierania)(
                    Sub(p)
                        If IsDisposed Then Exit Sub
                        If p.Wszystkie > 0 Then
                            pbPostep.Style = ProgressBarStyle.Blocks
                            pbPostep.Value = Math.Min(100, CInt(p.Pobrane * 100L \ p.Wszystkie))
                        End If
                        lblSzczegolyPostepu.Text = "Krok 1 z 2: pobieranie segmentów - " & etap & p.Pobrane & " z " & p.Wszystkie & "..."
                    End Sub)
                Dim wynik = Await PobieranieSegmentow.PobierzAsync(_zadaniaPobierania(i), postepPobierania, token)

                If wynik.Przerwano Then
                    lblStanPostepu.Text = "Pobieranie zostało przerwane."
                    lblStanPostepu.ForeColor = Color.FromArgb(200, 30, 30)
                    lblSzczegolyPostepu.Text = "Już pobrane segmenty zostają - „Ponów pobieranie”” dokończy mapę, a „Wstecz”” wraca do ustawień."
                    btnPonow.Visible = True
                    Exit Function
                End If
                ' Dawniej liczba nieudanych segmentów była pomijana: mapa z dziurami kończyła się komunikatem „Gotowe!”
                If i = 0 AndAlso wynik.Wszystkie > 0 AndAlso wynik.Nieudane >= wynik.Wszystkie Then
                    lblStanPostepu.Text = "Nie udało się pobrać żadnego fragmentu mapy."
                    lblStanPostepu.ForeColor = Color.FromArgb(200, 30, 30)
                    lblSzczegolyPostepu.Text = "Serwer mapy nie odpowiada albo nie obejmuje wybranego obszaru. Sprawdź połączenie z internetem i kliknij „Ponów pobieranie””."
                    btnPonow.Visible = True
                    Exit Function
                End If
                nieudane += wynik.Nieudane
                wszystkie += wynik.Wszystkie
            Next

            lblStanPostepu.Text = "Zapisywanie pliku mapy..."
            lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie pliku " & Path.GetFileName(plikDocelowy) & "..."
            pbPostep.Style = ProgressBarStyle.Marquee

            If _opcjeKompilacji IsNot Nothing Then
                ' Mapa wieloskalowa: każdy stopień wypełnia swój zakres powiększeń w jednym pliku MBTiles
                Dim warstwy As New List(Of WarstwaWielowarstwowa)()
                For i = 0 To liczbaZadan - 1
                    Dim z = _zadaniaPobierania(i)
                    Dim ps = _planGenerowania(i)
                    warstwy.Add(New WarstwaWielowarstwowa With {
                        .Folder = z.Folder, .Tytul = ps.Stopien.Opis, .OpisPelny = _wybranySzablon?.Nazwa & " - " & ps.Stopien.Opis,
                        .PoziomMin = ps.PoziomMin, .PoziomMax = ps.PoziomMax, .Kolejnosc = i, .Uklad = UkladWspolrzednych.PL1992,
                        .Siatka = z.Siatka, .Zrodlo = New ZrodloSegmentow(z.Folder, z.Prefiks, z.Numeracja, z.Format, z.Siatka)})
                Next
                Dim postepKompilacji As New Progress(Of RaportPostepuKompilacji)(
                    Sub(r)
                        If IsDisposed Then Exit Sub
                        pbPostep.Style = ProgressBarStyle.Blocks
                        pbPostep.Value = Math.Max(0, Math.Min(100, r.Procent))
                        lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie mapy wieloskalowej (" & r.Procent & "%" & If(r.AktualnyPoziom > 0, ", poziom " & r.AktualnyPoziom, "") & ")..."
                    End Sub)
                plikDocelowy = Await KompilatorMbtiles.KompilujAsync(warstwy, _opcjeKompilacji, postepKompilacji, token)
            Else
                Dim postepEksportu As New Progress(Of Integer)(
                    Sub(procent)
                        If IsDisposed Then Exit Sub
                        pbPostep.Style = ProgressBarStyle.Blocks
                        pbPostep.Value = Math.Max(0, Math.Min(100, procent))
                        lblSzczegolyPostepu.Text = "Krok 2 z 2: tworzenie pliku mapy (" & procent & "%)..."
                    End Sub)
                Dim zadanieEksportu = _zadanieEksportu
                Await Task.Run(Sub() EksportMapy.Eksportuj(zadanieEksportu, postepEksportu, token))
            End If
            _sciezkaWygenerowanegoPliku = plikDocelowy

            pbPostep.Style = ProgressBarStyle.Blocks
            pbPostep.Value = 100
            lblStanPostepu.Text = "Gotowe!"
            lblSzczegolyPostepu.Text = "Mapa została zapisana."
            pnlSukces.Visible = True

            Dim fi As New FileInfo(plikDocelowy)
            Dim rozmiarTekst = If(fi.Exists, (fi.Length / (1024.0 * 1024.0)).ToString("0.0") & " MB", "")
            lblSukcesPlik.Text = "Plik mapy: " & plikDocelowy & If(rozmiarTekst <> "", " (" & rozmiarTekst & ")", "") &
                                 If(_opcjeKompilacji IsNot Nothing, vbCrLf & "Mapa wieloskalowa: " & String.Join(", ", _planGenerowania.Select(Function(ps) $"{ps.Stopien.Opis} (zoom {ps.PoziomMin}-{ps.PoziomMax})")), "")
            lblSukcesWskazowka.Text = ZarzadcaSzablonow.WskazowkaWgrania(If(_zadanieEksportu IsNot Nothing, _zadanieEksportu.Rodzaj, RodzajEksportu.MBTiles))

            If nieudane > 0 Then
                lblSukcesTytul.Text = "✓ Mapa zapisana, ale z brakami"
                lblSukcesTytul.ForeColor = Color.FromArgb(180, 90, 0)
                lblSukcesUwaga.Text = $"Nie udało się pobrać {nieudane} z {wszystkie} segmentów - na mapie będą puste miejsca. " &
                                      "„Ponów pobieranie”” uzupełni braki i zapisze plik ponownie."
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
            _form1.WindowState = FormWindowState.Normal
            SetForegroundWindow(_form1.Handle)
            _form1.Activate()
            _form1.BringToFront()
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
            _form1.WindowState = FormWindowState.Normal
            _form1.Activate()
            _form1.BringToFront()
        End If
    End Sub

End Class
