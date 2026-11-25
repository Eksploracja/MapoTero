'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormKreatorMapy
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlNaglowek As System.Windows.Forms.Panel
    Friend WithEvents lblTytulKroku As System.Windows.Forms.Label
    Friend WithEvents lblPodtytulKroku As System.Windows.Forms.Label
    Friend WithEvents pnlSeparatorGora As System.Windows.Forms.Panel

    Friend WithEvents pnlStopka As System.Windows.Forms.Panel
    Friend WithEvents pnlSeparatorDol As System.Windows.Forms.Panel
    Friend WithEvents btnPomoc As System.Windows.Forms.Button
    Friend WithEvents btnWstecz As System.Windows.Forms.Button
    Friend WithEvents btnDalej As System.Windows.Forms.Button
    Friend WithEvents btnGeneruj As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button

    Friend WithEvents pnlZawartosc As System.Windows.Forms.Panel

    ' Krok 1: Cel
    Friend WithEvents pnlKrok1 As System.Windows.Forms.Panel
    Friend WithEvents rbCelTelefon As System.Windows.Forms.RadioButton
    Friend WithEvents rbCelGarmin As System.Windows.Forms.RadioButton
    Friend WithEvents rbCelWydruk As System.Windows.Forms.RadioButton
    Friend WithEvents rbCelCadGis As System.Windows.Forms.RadioButton
    Friend WithEvents rbCelCad As System.Windows.Forms.RadioButton
    Friend WithEvents lblOpisCelTelefon As System.Windows.Forms.Label
    Friend WithEvents lblOpisCelGarmin As System.Windows.Forms.Label
    Friend WithEvents lblOpisCelWydruk As System.Windows.Forms.Label
    Friend WithEvents lblOpisCelCadGis As System.Windows.Forms.Label
    Friend WithEvents lblOpisCelCad As System.Windows.Forms.Label

    ' Krok 2: Styl
    Friend WithEvents pnlKrok2 As System.Windows.Forms.Panel
    Friend WithEvents lblNaglowekSzablonow As System.Windows.Forms.Label
    Friend WithEvents lstSzablony As System.Windows.Forms.ListBox
    Friend WithEvents grpSzczegolySzablonu As System.Windows.Forms.GroupBox
    Friend WithEvents lblNazwaSzablonu As System.Windows.Forms.Label
    Friend WithEvents lblOpisSzablonu As System.Windows.Forms.Label
    Friend WithEvents lblSkladnikiNaglowek As System.Windows.Forms.Label
    Friend WithEvents lblSkladnikiSzablonu As System.Windows.Forms.Label
    Friend WithEvents lblCieniowanieOpis As System.Windows.Forms.Label
    Friend WithEvents tbCieniowanie As System.Windows.Forms.TrackBar
    Friend WithEvents lblCieniowanieWartosc As System.Windows.Forms.Label
    Friend WithEvents lblStanPodgladuStyl As System.Windows.Forms.Label
    Friend WithEvents btnZoomMinusStyl As System.Windows.Forms.Button
    Friend WithEvents lblZoomStyl As System.Windows.Forms.Label
    Friend WithEvents btnZoomPlusStyl As System.Windows.Forms.Button
    Friend WithEvents btnZoomResetStyl As System.Windows.Forms.Button
    Friend WithEvents btnOdswiezStyl As System.Windows.Forms.Button
    Friend WithEvents picPodgladStylu As System.Windows.Forms.PictureBox
    Friend WithEvents btnDostosujWarstwy As System.Windows.Forms.Button

    ' Krok 3: Obszar
    Friend WithEvents pnlKrok3 As System.Windows.Forms.Panel
    Friend WithEvents rbObszarMapa As System.Windows.Forms.RadioButton
    Friend WithEvents pnlObszarMapa As System.Windows.Forms.Panel
    Friend WithEvents lblStatusZaznaczenia As System.Windows.Forms.Label
    Friend WithEvents btnZaznaczNaMapie As System.Windows.Forms.Button
    Friend WithEvents rbObszarMiejscowosc As System.Windows.Forms.RadioButton
    Friend WithEvents pnlObszarMiejscowosc As System.Windows.Forms.Panel
    Friend WithEvents txtSzukajMiejsca As System.Windows.Forms.TextBox
    Friend WithEvents btnSzukajMiejsca As System.Windows.Forms.Button
    Friend WithEvents lblPromien As System.Windows.Forms.Label
    Friend WithEvents cboPromien As System.Windows.Forms.ComboBox
    Friend WithEvents lstWynikiMiejscowosci As System.Windows.Forms.ListBox
    Friend WithEvents lblWynikMiejscowosci As System.Windows.Forms.Label
    Friend WithEvents rbObszarTrasa As System.Windows.Forms.RadioButton
    Friend WithEvents pnlObszarTrasa As System.Windows.Forms.Panel
    Friend WithEvents btnWybierzTrase As System.Windows.Forms.Button
    Friend WithEvents lblStatusTrasy As System.Windows.Forms.Label
    Friend WithEvents lblKorytarzOpis As System.Windows.Forms.Label
    Friend WithEvents numKorytarz As System.Windows.Forms.NumericUpDown
    Friend WithEvents grpPodgladObszaru As System.Windows.Forms.GroupBox
    Friend WithEvents lblStanPodgladuObszar As System.Windows.Forms.Label
    Friend WithEvents btnZoomMinusObszar As System.Windows.Forms.Button
    Friend WithEvents lblZoomObszar As System.Windows.Forms.Label
    Friend WithEvents btnZoomPlusObszar As System.Windows.Forms.Button
    Friend WithEvents btnZoomResetObszar As System.Windows.Forms.Button
    Friend WithEvents picPodgladObszaru As System.Windows.Forms.PictureBox
    Friend WithEvents lblObszarWymiary As System.Windows.Forms.Label
    Friend WithEvents lblObszarWspolrzedne As System.Windows.Forms.Label

    ' Krok 4: Skala, podsumowanie i zaawansowane
    Friend WithEvents pnlKrok4 As System.Windows.Forms.Panel
    Friend WithEvents grpWariant As System.Windows.Forms.GroupBox
    Friend WithEvents rbWariantPojedynczy As System.Windows.Forms.RadioButton
    Friend WithEvents rbWariantWielopoziomowy As System.Windows.Forms.RadioButton
    Friend WithEvents grpSzacunki As System.Windows.Forms.GroupBox
    Friend WithEvents lblSzacunekSegmenty As System.Windows.Forms.Label
    Friend WithEvents lblSzacunekRozmiar As System.Windows.Forms.Label
    Friend WithEvents lblSzacunekCzas As System.Windows.Forms.Label
    Friend WithEvents lblSzacunekKomunikat As System.Windows.Forms.Label
    Friend WithEvents grpZapis As System.Windows.Forms.GroupBox
    Friend WithEvents txtSciezkaDocelowa As System.Windows.Forms.TextBox
    Friend WithEvents btnPrzegladajDocelowy As System.Windows.Forms.Button
    Friend WithEvents lblFormatInfo As System.Windows.Forms.Label
    Friend WithEvents grpZaawansowane As System.Windows.Forms.GroupBox
    Friend WithEvents chkKorektaObrazu As System.Windows.Forms.CheckBox
    Friend WithEvents lblProfil As System.Windows.Forms.Label
    Friend WithEvents cboProfilKorekty As System.Windows.Forms.ComboBox
    Friend WithEvents btnDostosujKorekte As System.Windows.Forms.Button
    Friend WithEvents chkWlasnyZoom As System.Windows.Forms.CheckBox
    Friend WithEvents lblZoomMin As System.Windows.Forms.Label
    Friend WithEvents numZoomMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblZoomMax As System.Windows.Forms.Label
    Friend WithEvents numZoomMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDrawOrder As System.Windows.Forms.Label
    Friend WithEvents numDrawOrder As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkPodzialTomow As System.Windows.Forms.CheckBox
    Friend WithEvents lblUkladCad As System.Windows.Forms.Label
    Friend WithEvents cboUkladCad As System.Windows.Forms.ComboBox
    Friend WithEvents lblCadInfo As System.Windows.Forms.Label
    Friend WithEvents lblRozmiarPiksela As System.Windows.Forms.Label
    Friend WithEvents numRozmiarPiksela As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPikselJednostka As System.Windows.Forms.Label
    Friend WithEvents btnOtworzWieloskalowy As System.Windows.Forms.Button
    Friend WithEvents lblWieloskalowyOpis As System.Windows.Forms.Label

    ' Krok 5: Postęp i wynik
    Friend WithEvents pnlKrok5 As System.Windows.Forms.Panel
    Friend WithEvents lblStanPostepu As System.Windows.Forms.Label
    Friend WithEvents lblSzczegolyPostepu As System.Windows.Forms.Label
    Friend WithEvents pbPostep As System.Windows.Forms.ProgressBar
    Friend WithEvents btnPrzerwij As System.Windows.Forms.Button
    Friend WithEvents pnlSukces As System.Windows.Forms.Panel
    Friend WithEvents lblSukcesTytul As System.Windows.Forms.Label
    Friend WithEvents lblSukcesPlik As System.Windows.Forms.Label
    Friend WithEvents btnPokazWFolderze As System.Windows.Forms.Button
    Friend WithEvents btnZaladujDoProgramu As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlNaglowek = New System.Windows.Forms.Panel()
        Me.lblTytulKroku = New System.Windows.Forms.Label()
        Me.lblPodtytulKroku = New System.Windows.Forms.Label()
        Me.pnlSeparatorGora = New System.Windows.Forms.Panel()

        Me.pnlStopka = New System.Windows.Forms.Panel()
        Me.pnlSeparatorDol = New System.Windows.Forms.Panel()
        Me.btnPomoc = New System.Windows.Forms.Button()
        Me.btnWstecz = New System.Windows.Forms.Button()
        Me.btnDalej = New System.Windows.Forms.Button()
        Me.btnGeneruj = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()

        Me.pnlZawartosc = New System.Windows.Forms.Panel()

        ' Inicjalizacja kroków
        Me.pnlKrok1 = New System.Windows.Forms.Panel()
        Me.rbCelTelefon = New System.Windows.Forms.RadioButton()
        Me.lblOpisCelTelefon = New System.Windows.Forms.Label()
        Me.rbCelGarmin = New System.Windows.Forms.RadioButton()
        Me.lblOpisCelGarmin = New System.Windows.Forms.Label()
        Me.rbCelWydruk = New System.Windows.Forms.RadioButton()
        Me.lblOpisCelWydruk = New System.Windows.Forms.Label()
        Me.rbCelCadGis = New System.Windows.Forms.RadioButton()
        Me.lblOpisCelCadGis = New System.Windows.Forms.Label()
        Me.rbCelCad = New System.Windows.Forms.RadioButton()
        Me.lblOpisCelCad = New System.Windows.Forms.Label()

        Me.pnlKrok2 = New System.Windows.Forms.Panel()
        Me.lblNaglowekSzablonow = New System.Windows.Forms.Label()
        Me.lstSzablony = New System.Windows.Forms.ListBox()
        Me.grpSzczegolySzablonu = New System.Windows.Forms.GroupBox()
        Me.lblNazwaSzablonu = New System.Windows.Forms.Label()
        Me.lblOpisSzablonu = New System.Windows.Forms.Label()
        Me.lblSkladnikiNaglowek = New System.Windows.Forms.Label()
        Me.lblSkladnikiSzablonu = New System.Windows.Forms.Label()
        Me.lblCieniowanieOpis = New System.Windows.Forms.Label()
        Me.tbCieniowanie = New System.Windows.Forms.TrackBar()
        Me.lblCieniowanieWartosc = New System.Windows.Forms.Label()
        Me.lblStanPodgladuStyl = New System.Windows.Forms.Label()
        Me.btnZoomMinusStyl = New System.Windows.Forms.Button()
        Me.lblZoomStyl = New System.Windows.Forms.Label()
        Me.btnZoomPlusStyl = New System.Windows.Forms.Button()
        Me.btnZoomResetStyl = New System.Windows.Forms.Button()
        Me.btnOdswiezStyl = New System.Windows.Forms.Button()
        Me.picPodgladStylu = New System.Windows.Forms.PictureBox()
        Me.btnDostosujWarstwy = New System.Windows.Forms.Button()

        Me.pnlKrok3 = New System.Windows.Forms.Panel()
        Me.rbObszarMapa = New System.Windows.Forms.RadioButton()
        Me.pnlObszarMapa = New System.Windows.Forms.Panel()
        Me.lblStatusZaznaczenia = New System.Windows.Forms.Label()
        Me.btnZaznaczNaMapie = New System.Windows.Forms.Button()
        Me.rbObszarMiejscowosc = New System.Windows.Forms.RadioButton()
        Me.pnlObszarMiejscowosc = New System.Windows.Forms.Panel()
        Me.txtSzukajMiejsca = New System.Windows.Forms.TextBox()
        Me.btnSzukajMiejsca = New System.Windows.Forms.Button()
        Me.lblPromien = New System.Windows.Forms.Label()
        Me.cboPromien = New System.Windows.Forms.ComboBox()
        Me.lstWynikiMiejscowosci = New System.Windows.Forms.ListBox()
        Me.lblWynikMiejscowosci = New System.Windows.Forms.Label()
        Me.rbObszarTrasa = New System.Windows.Forms.RadioButton()
        Me.pnlObszarTrasa = New System.Windows.Forms.Panel()
        Me.btnWybierzTrase = New System.Windows.Forms.Button()
        Me.lblStatusTrasy = New System.Windows.Forms.Label()
        Me.lblKorytarzOpis = New System.Windows.Forms.Label()
        Me.numKorytarz = New System.Windows.Forms.NumericUpDown()
        Me.grpPodgladObszaru = New System.Windows.Forms.GroupBox()
        Me.lblStanPodgladuObszar = New System.Windows.Forms.Label()
        Me.btnZoomMinusObszar = New System.Windows.Forms.Button()
        Me.lblZoomObszar = New System.Windows.Forms.Label()
        Me.btnZoomPlusObszar = New System.Windows.Forms.Button()
        Me.btnZoomResetObszar = New System.Windows.Forms.Button()
        Me.picPodgladObszaru = New System.Windows.Forms.PictureBox()
        Me.lblObszarWymiary = New System.Windows.Forms.Label()
        Me.lblObszarWspolrzedne = New System.Windows.Forms.Label()

        Me.pnlKrok4 = New System.Windows.Forms.Panel()
        Me.grpWariant = New System.Windows.Forms.GroupBox()
        Me.rbWariantPojedynczy = New System.Windows.Forms.RadioButton()
        Me.rbWariantWielopoziomowy = New System.Windows.Forms.RadioButton()
        Me.grpSzacunki = New System.Windows.Forms.GroupBox()
        Me.lblSzacunekSegmenty = New System.Windows.Forms.Label()
        Me.lblSzacunekRozmiar = New System.Windows.Forms.Label()
        Me.lblSzacunekCzas = New System.Windows.Forms.Label()
        Me.lblSzacunekKomunikat = New System.Windows.Forms.Label()
        Me.grpZapis = New System.Windows.Forms.GroupBox()
        Me.txtSciezkaDocelowa = New System.Windows.Forms.TextBox()
        Me.btnPrzegladajDocelowy = New System.Windows.Forms.Button()
        Me.lblFormatInfo = New System.Windows.Forms.Label()
        Me.grpZaawansowane = New System.Windows.Forms.GroupBox()
        Me.chkKorektaObrazu = New System.Windows.Forms.CheckBox()
        Me.lblProfil = New System.Windows.Forms.Label()
        Me.cboProfilKorekty = New System.Windows.Forms.ComboBox()
        Me.btnDostosujKorekte = New System.Windows.Forms.Button()
        Me.chkWlasnyZoom = New System.Windows.Forms.CheckBox()
        Me.lblZoomMin = New System.Windows.Forms.Label()
        Me.numZoomMin = New System.Windows.Forms.NumericUpDown()
        Me.lblZoomMax = New System.Windows.Forms.Label()
        Me.numZoomMax = New System.Windows.Forms.NumericUpDown()
        Me.lblDrawOrder = New System.Windows.Forms.Label()
        Me.numDrawOrder = New System.Windows.Forms.NumericUpDown()
        Me.chkPodzialTomow = New System.Windows.Forms.CheckBox()
        Me.lblUkladCad = New System.Windows.Forms.Label()
        Me.cboUkladCad = New System.Windows.Forms.ComboBox()
        Me.lblCadInfo = New System.Windows.Forms.Label()
        Me.lblRozmiarPiksela = New System.Windows.Forms.Label()
        Me.numRozmiarPiksela = New System.Windows.Forms.NumericUpDown()
        Me.lblPikselJednostka = New System.Windows.Forms.Label()
        Me.btnOtworzWieloskalowy = New System.Windows.Forms.Button()
        Me.lblWieloskalowyOpis = New System.Windows.Forms.Label()

        Me.pnlKrok5 = New System.Windows.Forms.Panel()
        Me.lblStanPostepu = New System.Windows.Forms.Label()
        Me.lblSzczegolyPostepu = New System.Windows.Forms.Label()
        Me.pbPostep = New System.Windows.Forms.ProgressBar()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.pnlSukces = New System.Windows.Forms.Panel()
        Me.lblSukcesTytul = New System.Windows.Forms.Label()
        Me.lblSukcesPlik = New System.Windows.Forms.Label()
        Me.btnPokazWFolderze = New System.Windows.Forms.Button()
        Me.btnZaladujDoProgramu = New System.Windows.Forms.Button()

        Me.SuspendLayout()

        '
        ' pnlNaglowek
        '
        Me.pnlNaglowek.BackColor = System.Drawing.Color.White
        Me.pnlNaglowek.Controls.Add(Me.lblTytulKroku)
        Me.pnlNaglowek.Controls.Add(Me.lblPodtytulKroku)
        Me.pnlNaglowek.Controls.Add(Me.pnlSeparatorGora)
        Me.pnlNaglowek.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlNaglowek.Location = New System.Drawing.Point(0, 0)
        Me.pnlNaglowek.Name = "pnlNaglowek"
        Me.pnlNaglowek.Size = New System.Drawing.Size(774, 66)
        Me.pnlNaglowek.TabIndex = 0

        ' lblTytulKroku
        Me.lblTytulKroku.AutoSize = True
        Me.lblTytulKroku.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTytulKroku.ForeColor = System.Drawing.Color.FromArgb(16, 44, 87)
        Me.lblTytulKroku.Location = New System.Drawing.Point(16, 10)
        Me.lblTytulKroku.Name = "lblTytulKroku"
        Me.lblTytulKroku.Text = "Krok 1 z 4: Do czego potrzebujesz mapy?"

        ' lblPodtytulKroku
        Me.lblPodtytulKroku.AutoSize = True
        Me.lblPodtytulKroku.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblPodtytulKroku.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80)
        Me.lblPodtytulKroku.Location = New System.Drawing.Point(17, 36)
        Me.lblPodtytulKroku.Name = "lblPodtytulKroku"
        Me.lblPodtytulKroku.Text = "Wybierz urządzenie lub zastosowanie, a program dobierze właściwy format i parametry."

        ' pnlSeparatorGora
        Me.pnlSeparatorGora.BackColor = System.Drawing.Color.FromArgb(215, 215, 220)
        Me.pnlSeparatorGora.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlSeparatorGora.Location = New System.Drawing.Point(0, 65)
        Me.pnlSeparatorGora.Name = "pnlSeparatorGora"
        Me.pnlSeparatorGora.Size = New System.Drawing.Size(774, 1)

        '
        ' pnlStopka
        '
        Me.pnlStopka.BackColor = System.Drawing.Color.FromArgb(246, 246, 248)
        Me.pnlStopka.Controls.Add(Me.pnlSeparatorDol)
        Me.pnlStopka.Controls.Add(Me.btnPomoc)
        Me.pnlStopka.Controls.Add(Me.btnWstecz)
        Me.pnlStopka.Controls.Add(Me.btnDalej)
        Me.pnlStopka.Controls.Add(Me.btnGeneruj)
        Me.pnlStopka.Controls.Add(Me.btnAnuluj)
        Me.pnlStopka.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlStopka.Location = New System.Drawing.Point(0, 510)
        Me.pnlStopka.Name = "pnlStopka"
        Me.pnlStopka.Size = New System.Drawing.Size(774, 50)
        Me.pnlStopka.TabIndex = 2

        ' pnlSeparatorDol
        Me.pnlSeparatorDol.BackColor = System.Drawing.Color.FromArgb(215, 215, 220)
        Me.pnlSeparatorDol.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSeparatorDol.Location = New System.Drawing.Point(0, 0)
        Me.pnlSeparatorDol.Name = "pnlSeparatorDol"
        Me.pnlSeparatorDol.Size = New System.Drawing.Size(774, 1)

        ' btnPomoc
        Me.btnPomoc.Location = New System.Drawing.Point(16, 10)
        Me.btnPomoc.Name = "btnPomoc"
        Me.btnPomoc.Size = New System.Drawing.Size(85, 30)
        Me.btnPomoc.Text = "Pomoc"
        Me.btnPomoc.UseVisualStyleBackColor = True

        ' btnWstecz
        Me.btnWstecz.Location = New System.Drawing.Point(435, 10)
        Me.btnWstecz.Name = "btnWstecz"
        Me.btnWstecz.Size = New System.Drawing.Size(90, 30)
        Me.btnWstecz.Text = "< Wstecz"
        Me.btnWstecz.UseVisualStyleBackColor = True

        ' btnDalej
        Me.btnDalej.Location = New System.Drawing.Point(535, 10)
        Me.btnDalej.Name = "btnDalej"
        Me.btnDalej.Size = New System.Drawing.Size(125, 30)
        Me.btnDalej.Text = "Dalej >"
        Me.btnDalej.UseVisualStyleBackColor = True

        ' btnGeneruj
        Me.btnGeneruj.BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
        Me.btnGeneruj.ForeColor = System.Drawing.Color.White
        Me.btnGeneruj.Location = New System.Drawing.Point(535, 10)
        Me.btnGeneruj.Name = "btnGeneruj"
        Me.btnGeneruj.Size = New System.Drawing.Size(125, 30)
        Me.btnGeneruj.Text = "Generuj mapę"
        Me.btnGeneruj.UseVisualStyleBackColor = False
        Me.btnGeneruj.Visible = False

        ' btnAnuluj
        Me.btnAnuluj.Location = New System.Drawing.Point(670, 10)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(90, 30)
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True

        '
        ' pnlZawartosc
        '
        Me.pnlZawartosc.Controls.Add(Me.pnlKrok1)
        Me.pnlZawartosc.Controls.Add(Me.pnlKrok2)
        Me.pnlZawartosc.Controls.Add(Me.pnlKrok3)
        Me.pnlZawartosc.Controls.Add(Me.pnlKrok4)
        Me.pnlZawartosc.Controls.Add(Me.pnlKrok5)
        Me.pnlZawartosc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlZawartosc.Location = New System.Drawing.Point(0, 66)
        Me.pnlZawartosc.Name = "pnlZawartosc"
        Me.pnlZawartosc.Size = New System.Drawing.Size(774, 444)
        Me.pnlZawartosc.TabIndex = 1

        '
        ' pnlKrok1: Cel
        '
        Me.pnlKrok1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrok1.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrok1.Name = "pnlKrok1"
        Me.pnlKrok1.Size = New System.Drawing.Size(774, 444)

        Me.rbCelTelefon.AutoSize = True
        Me.rbCelTelefon.Checked = True
        Me.rbCelTelefon.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold)
        Me.rbCelTelefon.Location = New System.Drawing.Point(30, 15)
        Me.rbCelTelefon.Name = "rbCelTelefon"
        Me.rbCelTelefon.Size = New System.Drawing.Size(560, 23)
        Me.rbCelTelefon.Text = "Smartfon lub tablet (Locus Map, OsmAnd, OruxMaps)"
        Me.rbCelTelefon.UseVisualStyleBackColor = True

        Me.lblOpisCelTelefon.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblOpisCelTelefon.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblOpisCelTelefon.Location = New System.Drawing.Point(62, 38)
        Me.lblOpisCelTelefon.Name = "lblOpisCelTelefon"
        Me.lblOpisCelTelefon.Size = New System.Drawing.Size(680, 36)
        Me.lblOpisCelTelefon.Text = "Plik MBTiles (albo SQLiteDB dla OsmAnd) z piramidą powiększeń: mapa płynnie się przybliża i oddala i działa bez internetu."

        Me.rbCelGarmin.AutoSize = True
        Me.rbCelGarmin.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold)
        Me.rbCelGarmin.Location = New System.Drawing.Point(30, 78)
        Me.rbCelGarmin.Name = "rbCelGarmin"
        Me.rbCelGarmin.Size = New System.Drawing.Size(580, 23)
        Me.rbCelGarmin.Text = "Odbiornik turystyczny lub zegarek Garmin (Fenix, GPSMAP, eTrex)"
        Me.rbCelGarmin.UseVisualStyleBackColor = True

        Me.lblOpisCelGarmin.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblOpisCelGarmin.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblOpisCelGarmin.Location = New System.Drawing.Point(62, 101)
        Me.lblOpisCelGarmin.Name = "lblOpisCelGarmin"
        Me.lblOpisCelGarmin.Size = New System.Drawing.Size(680, 36)
        Me.lblOpisCelGarmin.Text = "KMZ dla urządzeń obsługujących Garmin Custom Maps albo JNX dla urządzeń obsługujących pliki BirdsEye."

        Me.rbCelWydruk.AutoSize = True
        Me.rbCelWydruk.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold)
        Me.rbCelWydruk.Location = New System.Drawing.Point(30, 141)
        Me.rbCelWydruk.Name = "rbCelWydruk"
        Me.rbCelWydruk.Size = New System.Drawing.Size(540, 23)
        Me.rbCelWydruk.Text = "Arkusz graficzny do druku lub przeglądania na komputerze"
        Me.rbCelWydruk.UseVisualStyleBackColor = True

        Me.lblOpisCelWydruk.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblOpisCelWydruk.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblOpisCelWydruk.Location = New System.Drawing.Point(62, 164)
        Me.lblOpisCelWydruk.Name = "lblOpisCelWydruk"
        Me.lblOpisCelWydruk.Size = New System.Drawing.Size(680, 36)
        Me.lblOpisCelWydruk.Text = "Jeden scalony arkusz GeoTIFF w wysokiej rozdzielczości, z georeferencją - do druku, plakatu albo przeglądania na komputerze."

        Me.rbCelCadGis.AutoSize = True
        Me.rbCelCadGis.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold)
        Me.rbCelCadGis.Location = New System.Drawing.Point(30, 204)
        Me.rbCelCadGis.Name = "rbCelCadGis"
        Me.rbCelCadGis.Size = New System.Drawing.Size(580, 23)
        Me.rbCelCadGis.Text = "Systemy GIS i aplikacje mobilne (QGIS, QField, Avenza Maps)"
        Me.rbCelCadGis.UseVisualStyleBackColor = True

        Me.lblOpisCelCadGis.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblOpisCelCadGis.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblOpisCelCadGis.Location = New System.Drawing.Point(62, 227)
        Me.lblOpisCelCadGis.Name = "lblOpisCelCadGis"
        Me.lblOpisCelCadGis.Size = New System.Drawing.Size(680, 36)
        Me.lblOpisCelCadGis.Text = "Arkusz GeoTIFF z georeferencją w układzie PL-1992 jako podkład w QGIS, ArcGIS, Avenza Maps lub QField."

        Me.rbCelCad.AutoSize = True
        Me.rbCelCad.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold)
        Me.rbCelCad.Location = New System.Drawing.Point(30, 267)
        Me.rbCelCad.Name = "rbCelCad"
        Me.rbCelCad.Size = New System.Drawing.Size(580, 23)
        Me.rbCelCad.Text = "Projekt CAD (AutoCAD, ZWCAD, BricsCAD – raster i skrypt wpasowania)"
        Me.rbCelCad.UseVisualStyleBackColor = True

        Me.lblOpisCelCad.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblOpisCelCad.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblOpisCelCad.Location = New System.Drawing.Point(62, 290)
        Me.lblOpisCelCad.Name = "lblOpisCelCad"
        Me.lblOpisCelCad.Size = New System.Drawing.Size(680, 36)
        Me.lblOpisCelCad.Text = "Podkład rastrowy w wybranym pasie PL-2000 (strefy 5–8) lub PL-1992 wraz ze skryptem wpasowania (.scr)."

        Me.pnlKrok1.Controls.Add(Me.rbCelTelefon)
        Me.pnlKrok1.Controls.Add(Me.lblOpisCelTelefon)
        Me.pnlKrok1.Controls.Add(Me.rbCelGarmin)
        Me.pnlKrok1.Controls.Add(Me.lblOpisCelGarmin)
        Me.pnlKrok1.Controls.Add(Me.rbCelWydruk)
        Me.pnlKrok1.Controls.Add(Me.lblOpisCelWydruk)
        Me.pnlKrok1.Controls.Add(Me.rbCelCadGis)
        Me.pnlKrok1.Controls.Add(Me.lblOpisCelCadGis)
        Me.pnlKrok1.Controls.Add(Me.rbCelCad)
        Me.pnlKrok1.Controls.Add(Me.lblOpisCelCad)

        '
        ' pnlKrok2: Styl
        '
        Me.pnlKrok2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrok2.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrok2.Name = "pnlKrok2"
        Me.pnlKrok2.Size = New System.Drawing.Size(774, 444)

        Me.lblNaglowekSzablonow.AutoSize = True
        Me.lblNaglowekSzablonow.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.lblNaglowekSzablonow.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40)
        Me.lblNaglowekSzablonow.Location = New System.Drawing.Point(16, 10)
        Me.lblNaglowekSzablonow.Text = "Dostępne style mapy:"

        Me.lstSzablony.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.lstSzablony.ItemHeight = 46
        Me.lstSzablony.Location = New System.Drawing.Point(16, 32)
        Me.lstSzablony.Name = "lstSzablony"
        Me.lstSzablony.Size = New System.Drawing.Size(330, 395)
        Me.lstSzablony.TabIndex = 0

        Me.grpSzczegolySzablonu.Location = New System.Drawing.Point(358, 10)
        Me.grpSzczegolySzablonu.Name = "grpSzczegolySzablonu"
        Me.grpSzczegolySzablonu.Size = New System.Drawing.Size(402, 420)
        Me.grpSzczegolySzablonu.TabIndex = 1
        Me.grpSzczegolySzablonu.TabStop = False
        Me.grpSzczegolySzablonu.Text = "Szczegóły wybranego stylu"

        Me.lblNazwaSzablonu.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.lblNazwaSzablonu.ForeColor = System.Drawing.Color.FromArgb(16, 44, 87)
        Me.lblNazwaSzablonu.Location = New System.Drawing.Point(14, 16)
        Me.lblNazwaSzablonu.Name = "lblNazwaSzablonu"
        Me.lblNazwaSzablonu.Size = New System.Drawing.Size(374, 22)
        Me.lblNazwaSzablonu.Text = "Nazwa stylu"

        Me.lblOpisSzablonu.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblOpisSzablonu.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60)
        Me.lblOpisSzablonu.Location = New System.Drawing.Point(14, 38)
        Me.lblOpisSzablonu.Name = "lblOpisSzablonu"
        Me.lblOpisSzablonu.Size = New System.Drawing.Size(374, 32)
        Me.lblOpisSzablonu.Text = "Opis stylu"

        Me.lblSkladnikiNaglowek.AutoSize = True
        Me.lblSkladnikiNaglowek.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.lblSkladnikiNaglowek.Location = New System.Drawing.Point(14, 72)
        Me.lblSkladnikiNaglowek.Name = "lblSkladnikiNaglowek"
        Me.lblSkladnikiNaglowek.Text = "Warstwy w zestawie:"

        Me.lblSkladnikiSzablonu.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblSkladnikiSzablonu.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40)
        Me.lblSkladnikiSzablonu.Location = New System.Drawing.Point(14, 88)
        Me.lblSkladnikiSzablonu.Name = "lblSkladnikiSzablonu"
        Me.lblSkladnikiSzablonu.Size = New System.Drawing.Size(374, 24)
        Me.lblSkladnikiSzablonu.Text = "Warstwy..."

        Me.lblCieniowanieOpis.AutoSize = True
        Me.lblCieniowanieOpis.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.lblCieniowanieOpis.Location = New System.Drawing.Point(14, 114)
        Me.lblCieniowanieOpis.Name = "lblCieniowanieOpis"
        Me.lblCieniowanieOpis.Text = "Intensywność cieniowania rzeźby terenu (ISOK LiDAR):"

        Me.tbCieniowanie.AutoSize = False
        Me.tbCieniowanie.Location = New System.Drawing.Point(14, 130)
        Me.tbCieniowanie.Maximum = 100
        Me.tbCieniowanie.Minimum = 0
        Me.tbCieniowanie.Name = "tbCieniowanie"
        Me.tbCieniowanie.Size = New System.Drawing.Size(325, 24)
        Me.tbCieniowanie.TickStyle = System.Windows.Forms.TickStyle.None
        Me.tbCieniowanie.Value = 35

        Me.lblCieniowanieWartosc.AutoSize = True
        Me.lblCieniowanieWartosc.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblCieniowanieWartosc.Location = New System.Drawing.Point(344, 133)
        Me.lblCieniowanieWartosc.Name = "lblCieniowanieWartosc"
        Me.lblCieniowanieWartosc.Text = "35%"

        Me.lblStanPodgladuStyl.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.lblStanPodgladuStyl.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90)
        Me.lblStanPodgladuStyl.Location = New System.Drawing.Point(14, 160)
        Me.lblStanPodgladuStyl.Name = "lblStanPodgladuStyl"
        Me.lblStanPodgladuStyl.Size = New System.Drawing.Size(190, 20)
        Me.lblStanPodgladuStyl.Text = "Podgląd próbki terenu"

        Me.btnZoomMinusStyl.Location = New System.Drawing.Point(210, 158)
        Me.btnZoomMinusStyl.Name = "btnZoomMinusStyl"
        Me.btnZoomMinusStyl.Size = New System.Drawing.Size(26, 22)
        Me.btnZoomMinusStyl.Text = "-"
        Me.btnZoomMinusStyl.UseVisualStyleBackColor = True

        Me.lblZoomStyl.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Bold)
        Me.lblZoomStyl.Location = New System.Drawing.Point(237, 159)
        Me.lblZoomStyl.Name = "lblZoomStyl"
        Me.lblZoomStyl.Size = New System.Drawing.Size(32, 20)
        Me.lblZoomStyl.Text = "100%"
        Me.lblZoomStyl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        Me.btnZoomPlusStyl.Location = New System.Drawing.Point(270, 158)
        Me.btnZoomPlusStyl.Name = "btnZoomPlusStyl"
        Me.btnZoomPlusStyl.Size = New System.Drawing.Size(26, 22)
        Me.btnZoomPlusStyl.Text = "+"
        Me.btnZoomPlusStyl.UseVisualStyleBackColor = True

        Me.btnZoomResetStyl.Location = New System.Drawing.Point(298, 158)
        Me.btnZoomResetStyl.Name = "btnZoomResetStyl"
        Me.btnZoomResetStyl.Size = New System.Drawing.Size(34, 22)
        Me.btnZoomResetStyl.Text = "100%"
        Me.btnZoomResetStyl.UseVisualStyleBackColor = True

        Me.btnOdswiezStyl.Location = New System.Drawing.Point(334, 158)
        Me.btnOdswiezStyl.Name = "btnOdswiezStyl"
        Me.btnOdswiezStyl.Size = New System.Drawing.Size(54, 22)
        Me.btnOdswiezStyl.Text = "Odśwież"
        Me.btnOdswiezStyl.UseVisualStyleBackColor = True

        Me.picPodgladStylu.BackColor = System.Drawing.Color.White
        Me.picPodgladStylu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodgladStylu.Location = New System.Drawing.Point(14, 184)
        Me.picPodgladStylu.Name = "picPodgladStylu"
        Me.picPodgladStylu.Size = New System.Drawing.Size(374, 196)
        Me.picPodgladStylu.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage

        Me.btnDostosujWarstwy.Location = New System.Drawing.Point(14, 386)
        Me.btnDostosujWarstwy.Name = "btnDostosujWarstwy"
        Me.btnDostosujWarstwy.Size = New System.Drawing.Size(260, 26)
        Me.btnDostosujWarstwy.Text = "Dostosuj warstwy w oknie zaawansowanym..."
        Me.btnDostosujWarstwy.UseVisualStyleBackColor = True

        Me.grpSzczegolySzablonu.Controls.Add(Me.lblNazwaSzablonu)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblOpisSzablonu)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblSkladnikiNaglowek)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblSkladnikiSzablonu)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblCieniowanieOpis)
        Me.grpSzczegolySzablonu.Controls.Add(Me.tbCieniowanie)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblCieniowanieWartosc)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblStanPodgladuStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.btnZoomMinusStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.lblZoomStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.btnZoomPlusStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.btnZoomResetStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.btnOdswiezStyl)
        Me.grpSzczegolySzablonu.Controls.Add(Me.picPodgladStylu)
        Me.grpSzczegolySzablonu.Controls.Add(Me.btnDostosujWarstwy)

        Me.pnlKrok2.Controls.Add(Me.lblNaglowekSzablonow)
        Me.pnlKrok2.Controls.Add(Me.lstSzablony)
        Me.pnlKrok2.Controls.Add(Me.grpSzczegolySzablonu)

        '
        ' pnlKrok3: Obszar
        '
        Me.pnlKrok3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrok3.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrok3.Name = "pnlKrok3"
        Me.pnlKrok3.Size = New System.Drawing.Size(774, 444)

        ' 1. Zaznacz na mapie (domyślny)
        Me.rbObszarMapa.AutoSize = True
        Me.rbObszarMapa.Checked = True
        Me.rbObszarMapa.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.rbObszarMapa.Location = New System.Drawing.Point(18, 10)
        Me.rbObszarMapa.Name = "rbObszarMapa"
        Me.rbObszarMapa.Size = New System.Drawing.Size(320, 21)
        Me.rbObszarMapa.Text = "Zaznacz własny prostokąt bezpośrednio na mapie:"
        Me.rbObszarMapa.UseVisualStyleBackColor = True

        Me.pnlObszarMapa.Location = New System.Drawing.Point(36, 32)
        Me.pnlObszarMapa.Name = "pnlObszarMapa"
        Me.pnlObszarMapa.Size = New System.Drawing.Size(450, 48)

        Me.btnZaznaczNaMapie.Location = New System.Drawing.Point(0, 4)
        Me.btnZaznaczNaMapie.Name = "btnZaznaczNaMapie"
        Me.btnZaznaczNaMapie.Size = New System.Drawing.Size(185, 30)
        Me.btnZaznaczNaMapie.Text = "Przejdź do mapy i zaznacz..."
        Me.btnZaznaczNaMapie.UseVisualStyleBackColor = True

        Me.lblStatusZaznaczenia.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblStatusZaznaczenia.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60)
        Me.lblStatusZaznaczenia.Location = New System.Drawing.Point(192, 5)
        Me.lblStatusZaznaczenia.Name = "lblStatusZaznaczenia"
        Me.lblStatusZaznaczenia.Size = New System.Drawing.Size(255, 38)
        Me.lblStatusZaznaczenia.Text = "Brak zaznaczonego prostokąta na mapie."

        Me.pnlObszarMapa.Controls.Add(Me.btnZaznaczNaMapie)
        Me.pnlObszarMapa.Controls.Add(Me.lblStatusZaznaczenia)

        ' 2. Wyszukaj miejscowość
        Me.rbObszarMiejscowosc.AutoSize = True
        Me.rbObszarMiejscowosc.Checked = False
        Me.rbObszarMiejscowosc.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.rbObszarMiejscowosc.Location = New System.Drawing.Point(18, 86)
        Me.rbObszarMiejscowosc.Name = "rbObszarMiejscowosc"
        Me.rbObszarMiejscowosc.Size = New System.Drawing.Size(340, 21)
        Me.rbObszarMiejscowosc.Text = "Wyszukaj miejscowość i określ promień pobierania:"
        Me.rbObszarMiejscowosc.UseVisualStyleBackColor = True

        Me.pnlObszarMiejscowosc.Location = New System.Drawing.Point(36, 110)
        Me.pnlObszarMiejscowosc.Name = "pnlObszarMiejscowosc"
        Me.pnlObszarMiejscowosc.Size = New System.Drawing.Size(450, 198)

        Me.txtSzukajMiejsca.Location = New System.Drawing.Point(0, 2)
        Me.txtSzukajMiejsca.Name = "txtSzukajMiejsca"
        Me.txtSzukajMiejsca.Size = New System.Drawing.Size(215, 23)

        Me.lblPromien.AutoSize = True
        Me.lblPromien.Location = New System.Drawing.Point(220, 6)
        Me.lblPromien.Name = "lblPromien"
        Me.lblPromien.Text = "Promień:"

        Me.cboPromien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboPromien.FormattingEnabled = True
        Me.cboPromien.Items.AddRange(New Object() {"1 km", "2 km", "3 km", "5 km", "10 km", "15 km", "25 km", "50 km"})
        Me.cboPromien.Location = New System.Drawing.Point(275, 2)
        Me.cboPromien.Name = "cboPromien"
        Me.cboPromien.Size = New System.Drawing.Size(70, 23)
        Me.cboPromien.SelectedIndex = 3

        Me.btnSzukajMiejsca.Location = New System.Drawing.Point(352, 1)
        Me.btnSzukajMiejsca.Name = "btnSzukajMiejsca"
        Me.btnSzukajMiejsca.Size = New System.Drawing.Size(85, 25)
        Me.btnSzukajMiejsca.Text = "Szukaj"
        Me.btnSzukajMiejsca.UseVisualStyleBackColor = True

        Me.lstWynikiMiejscowosci.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.lstWynikiMiejscowosci.ItemHeight = 36
        Me.lstWynikiMiejscowosci.Location = New System.Drawing.Point(0, 30)
        Me.lstWynikiMiejscowosci.Name = "lstWynikiMiejscowosci"
        Me.lstWynikiMiejscowosci.Size = New System.Drawing.Size(437, 136)

        Me.lblWynikMiejscowosci.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblWynikMiejscowosci.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60)
        Me.lblWynikMiejscowosci.Location = New System.Drawing.Point(0, 168)
        Me.lblWynikMiejscowosci.Name = "lblWynikMiejscowosci"
        Me.lblWynikMiejscowosci.Size = New System.Drawing.Size(437, 28)
        Me.lblWynikMiejscowosci.Text = "Wpisz nazwę miejscowości lub adres..."

        Me.pnlObszarMiejscowosc.Controls.Add(Me.txtSzukajMiejsca)
        Me.pnlObszarMiejscowosc.Controls.Add(Me.lblPromien)
        Me.pnlObszarMiejscowosc.Controls.Add(Me.cboPromien)
        Me.pnlObszarMiejscowosc.Controls.Add(Me.btnSzukajMiejsca)
        Me.pnlObszarMiejscowosc.Controls.Add(Me.lstWynikiMiejscowosci)
        Me.pnlObszarMiejscowosc.Controls.Add(Me.lblWynikMiejscowosci)

        ' 3. Ślad trasy
        Me.rbObszarTrasa.AutoSize = True
        Me.rbObszarTrasa.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.rbObszarTrasa.Location = New System.Drawing.Point(18, 318)
        Me.rbObszarTrasa.Name = "rbObszarTrasa"
        Me.rbObszarTrasa.Size = New System.Drawing.Size(340, 21)
        Me.rbObszarTrasa.Text = "Pobierz korytarz wzdłuż śladu trasy (GPX / KML / KMZ):"
        Me.rbObszarTrasa.UseVisualStyleBackColor = True

        Me.pnlObszarTrasa.Location = New System.Drawing.Point(36, 340)
        Me.pnlObszarTrasa.Name = "pnlObszarTrasa"
        Me.pnlObszarTrasa.Size = New System.Drawing.Size(450, 75)

        Me.btnWybierzTrase.Location = New System.Drawing.Point(0, 4)
        Me.btnWybierzTrase.Name = "btnWybierzTrase"
        Me.btnWybierzTrase.Size = New System.Drawing.Size(140, 28)
        Me.btnWybierzTrase.Text = "Wybierz plik trasy..."
        Me.btnWybierzTrase.UseVisualStyleBackColor = True

        Me.lblKorytarzOpis.AutoSize = True
        Me.lblKorytarzOpis.Location = New System.Drawing.Point(150, 11)
        Me.lblKorytarzOpis.Name = "lblKorytarzOpis"
        Me.lblKorytarzOpis.Text = "Szerokość korytarza (m):"

        Me.numKorytarz.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        Me.numKorytarz.Location = New System.Drawing.Point(292, 9)
        Me.numKorytarz.Maximum = New Decimal(New Integer() {5000, 0, 0, 0})
        Me.numKorytarz.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.numKorytarz.Name = "numKorytarz"
        Me.numKorytarz.Size = New System.Drawing.Size(70, 23)
        Me.numKorytarz.Value = New Decimal(New Integer() {200, 0, 0, 0})

        Me.lblStatusTrasy.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblStatusTrasy.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60)
        Me.lblStatusTrasy.Location = New System.Drawing.Point(0, 36)
        Me.lblStatusTrasy.Name = "lblStatusTrasy"
        Me.lblStatusTrasy.Size = New System.Drawing.Size(440, 30)
        Me.lblStatusTrasy.Text = "Nie wybrano pliku trasy."

        Me.pnlObszarTrasa.Controls.Add(Me.btnWybierzTrase)
        Me.pnlObszarTrasa.Controls.Add(Me.lblKorytarzOpis)
        Me.pnlObszarTrasa.Controls.Add(Me.numKorytarz)
        Me.pnlObszarTrasa.Controls.Add(Me.lblStatusTrasy)

        ' grpPodgladObszaru
        Me.grpPodgladObszaru.Location = New System.Drawing.Point(498, 10)
        Me.grpPodgladObszaru.Name = "grpPodgladObszaru"
        Me.grpPodgladObszaru.Size = New System.Drawing.Size(262, 418)
        Me.grpPodgladObszaru.TabIndex = 2
        Me.grpPodgladObszaru.TabStop = False
        Me.grpPodgladObszaru.Text = "Podgląd wybranego obszaru"

        Me.lblStanPodgladuObszar.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.lblStanPodgladuObszar.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90)
        Me.lblStanPodgladuObszar.Location = New System.Drawing.Point(10, 18)
        Me.lblStanPodgladuObszar.Name = "lblStanPodgladuObszar"
        Me.lblStanPodgladuObszar.Size = New System.Drawing.Size(115, 20)
        Me.lblStanPodgladuObszar.Text = "Podgląd obszaru"

        Me.btnZoomMinusObszar.Location = New System.Drawing.Point(128, 16)
        Me.btnZoomMinusObszar.Name = "btnZoomMinusObszar"
        Me.btnZoomMinusObszar.Size = New System.Drawing.Size(26, 22)
        Me.btnZoomMinusObszar.Text = "-"
        Me.btnZoomMinusObszar.UseVisualStyleBackColor = True

        Me.lblZoomObszar.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Bold)
        Me.lblZoomObszar.Location = New System.Drawing.Point(155, 17)
        Me.lblZoomObszar.Name = "lblZoomObszar"
        Me.lblZoomObszar.Size = New System.Drawing.Size(32, 20)
        Me.lblZoomObszar.Text = "100%"
        Me.lblZoomObszar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

        Me.btnZoomPlusObszar.Location = New System.Drawing.Point(188, 16)
        Me.btnZoomPlusObszar.Name = "btnZoomPlusObszar"
        Me.btnZoomPlusObszar.Size = New System.Drawing.Size(26, 22)
        Me.btnZoomPlusObszar.Text = "+"
        Me.btnZoomPlusObszar.UseVisualStyleBackColor = True

        Me.btnZoomResetObszar.Location = New System.Drawing.Point(216, 16)
        Me.btnZoomResetObszar.Name = "btnZoomResetObszar"
        Me.btnZoomResetObszar.Size = New System.Drawing.Size(34, 22)
        Me.btnZoomResetObszar.Text = "100%"
        Me.btnZoomResetObszar.UseVisualStyleBackColor = True

        Me.picPodgladObszaru.BackColor = System.Drawing.Color.White
        Me.picPodgladObszaru.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodgladObszaru.Location = New System.Drawing.Point(10, 42)
        Me.picPodgladObszaru.Name = "picPodgladObszaru"
        Me.picPodgladObszaru.Size = New System.Drawing.Size(240, 210)
        Me.picPodgladObszaru.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage

        Me.lblObszarWymiary.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblObszarWymiary.ForeColor = System.Drawing.Color.FromArgb(16, 44, 87)
        Me.lblObszarWymiary.Location = New System.Drawing.Point(10, 258)
        Me.lblObszarWymiary.Name = "lblObszarWymiary"
        Me.lblObszarWymiary.Size = New System.Drawing.Size(240, 40)
        Me.lblObszarWymiary.Text = "Wymiary: brak"

        Me.lblObszarWspolrzedne.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.lblObszarWspolrzedne.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80)
        Me.lblObszarWspolrzedne.Location = New System.Drawing.Point(10, 300)
        Me.lblObszarWspolrzedne.Name = "lblObszarWspolrzedne"
        Me.lblObszarWspolrzedne.Size = New System.Drawing.Size(240, 105)
        Me.lblObszarWspolrzedne.Text = "Współrzędne: -"

        Me.grpPodgladObszaru.Controls.Add(Me.lblStanPodgladuObszar)
        Me.grpPodgladObszaru.Controls.Add(Me.btnZoomMinusObszar)
        Me.grpPodgladObszaru.Controls.Add(Me.lblZoomObszar)
        Me.grpPodgladObszaru.Controls.Add(Me.btnZoomPlusObszar)
        Me.grpPodgladObszaru.Controls.Add(Me.btnZoomResetObszar)
        Me.grpPodgladObszaru.Controls.Add(Me.picPodgladObszaru)
        Me.grpPodgladObszaru.Controls.Add(Me.lblObszarWymiary)
        Me.grpPodgladObszaru.Controls.Add(Me.lblObszarWspolrzedne)

        Me.pnlKrok3.Controls.Add(Me.rbObszarMapa)
        Me.pnlKrok3.Controls.Add(Me.pnlObszarMapa)
        Me.pnlKrok3.Controls.Add(Me.rbObszarMiejscowosc)
        Me.pnlKrok3.Controls.Add(Me.pnlObszarMiejscowosc)
        Me.pnlKrok3.Controls.Add(Me.rbObszarTrasa)
        Me.pnlKrok3.Controls.Add(Me.pnlObszarTrasa)
        Me.pnlKrok3.Controls.Add(Me.grpPodgladObszaru)

        '
        ' pnlKrok4: Podsumowanie i parametry
        '
        Me.pnlKrok4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrok4.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrok4.Name = "pnlKrok4"
        Me.pnlKrok4.Size = New System.Drawing.Size(774, 444)

        Me.grpWariant.Location = New System.Drawing.Point(16, 10)
        Me.grpWariant.Name = "grpWariant"
        Me.grpWariant.Size = New System.Drawing.Size(360, 80)
        Me.grpWariant.TabIndex = 0
        Me.grpWariant.TabStop = False
        Me.grpWariant.Text = "Struktura pliku mapy"

        Me.rbWariantPojedynczy.AutoSize = True
        Me.rbWariantPojedynczy.Location = New System.Drawing.Point(16, 24)
        Me.rbWariantPojedynczy.Name = "rbWariantPojedynczy"
        Me.rbWariantPojedynczy.Size = New System.Drawing.Size(315, 19)
        Me.rbWariantPojedynczy.Text = "Jeden poziom szczegółowości (pojedynczy raster)"
        Me.rbWariantPojedynczy.UseVisualStyleBackColor = True

        Me.rbWariantWielopoziomowy.AutoSize = True
        Me.rbWariantWielopoziomowy.Checked = True
        Me.rbWariantWielopoziomowy.Location = New System.Drawing.Point(16, 48)
        Me.rbWariantWielopoziomowy.Name = "rbWariantWielopoziomowy"
        Me.rbWariantWielopoziomowy.Size = New System.Drawing.Size(325, 19)
        Me.rbWariantWielopoziomowy.Text = "Wielopoziomowa piramida powiększeń (zalecana)"
        Me.rbWariantWielopoziomowy.UseVisualStyleBackColor = True

        Me.grpWariant.Controls.Add(Me.rbWariantPojedynczy)
        Me.grpWariant.Controls.Add(Me.rbWariantWielopoziomowy)

        Me.grpSzacunki.Location = New System.Drawing.Point(386, 10)
        Me.grpSzacunki.Name = "grpSzacunki"
        Me.grpSzacunki.Size = New System.Drawing.Size(374, 165)
        Me.grpSzacunki.TabIndex = 1
        Me.grpSzacunki.TabStop = False
        Me.grpSzacunki.Text = "Szacunek operacji"

        Me.lblSzacunekSegmenty.AutoSize = True
        Me.lblSzacunekSegmenty.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblSzacunekSegmenty.Location = New System.Drawing.Point(16, 22)
        Me.lblSzacunekSegmenty.Name = "lblSzacunekSegmenty"
        Me.lblSzacunekSegmenty.Text = "Liczba segmentów: 0"

        Me.lblSzacunekRozmiar.AutoSize = True
        Me.lblSzacunekRozmiar.Location = New System.Drawing.Point(16, 45)
        Me.lblSzacunekRozmiar.Name = "lblSzacunekRozmiar"
        Me.lblSzacunekRozmiar.Text = "Szacowany rozmiar danych: 0 MB"

        Me.lblSzacunekCzas.AutoSize = True
        Me.lblSzacunekCzas.Location = New System.Drawing.Point(16, 68)
        Me.lblSzacunekCzas.Name = "lblSzacunekCzas"
        Me.lblSzacunekCzas.Text = "Przewidywany czas: ok. 0 s"

        Me.lblSzacunekKomunikat.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.lblSzacunekKomunikat.Location = New System.Drawing.Point(16, 95)
        Me.lblSzacunekKomunikat.Name = "lblSzacunekKomunikat"
        Me.lblSzacunekKomunikat.Size = New System.Drawing.Size(345, 60)
        Me.lblSzacunekKomunikat.Text = "Obszar bezpieczny."

        Me.grpSzacunki.Controls.Add(Me.lblSzacunekSegmenty)
        Me.grpSzacunki.Controls.Add(Me.lblSzacunekRozmiar)
        Me.grpSzacunki.Controls.Add(Me.lblSzacunekCzas)
        Me.grpSzacunki.Controls.Add(Me.lblSzacunekKomunikat)

        Me.grpZapis.Location = New System.Drawing.Point(16, 95)
        Me.grpZapis.Name = "grpZapis"
        Me.grpZapis.Size = New System.Drawing.Size(360, 80)
        Me.grpZapis.TabIndex = 2
        Me.grpZapis.TabStop = False
        Me.grpZapis.Text = "Zapis pliku wynikowego"

        Me.txtSciezkaDocelowa.Location = New System.Drawing.Point(14, 22)
        Me.txtSciezkaDocelowa.Name = "txtSciezkaDocelowa"
        Me.txtSciezkaDocelowa.Size = New System.Drawing.Size(250, 23)

        Me.btnPrzegladajDocelowy.Location = New System.Drawing.Point(270, 21)
        Me.btnPrzegladajDocelowy.Name = "btnPrzegladajDocelowy"
        Me.btnPrzegladajDocelowy.Size = New System.Drawing.Size(78, 25)
        Me.btnPrzegladajDocelowy.Text = "Zmień..."
        Me.btnPrzegladajDocelowy.UseVisualStyleBackColor = True

        Me.lblFormatInfo.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.lblFormatInfo.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblFormatInfo.Location = New System.Drawing.Point(14, 50)
        Me.lblFormatInfo.Name = "lblFormatInfo"
        Me.lblFormatInfo.Size = New System.Drawing.Size(335, 24)
        Me.lblFormatInfo.Text = "Format pliku dobierany automatycznie wg celu."

        Me.grpZapis.Controls.Add(Me.txtSciezkaDocelowa)
        Me.grpZapis.Controls.Add(Me.btnPrzegladajDocelowy)
        Me.grpZapis.Controls.Add(Me.lblFormatInfo)

        ' grpZaawansowane
        Me.grpZaawansowane.Location = New System.Drawing.Point(16, 176)
        Me.grpZaawansowane.Name = "grpZaawansowane"
        Me.grpZaawansowane.Size = New System.Drawing.Size(744, 255)
        Me.grpZaawansowane.TabIndex = 3
        Me.grpZaawansowane.TabStop = False
        Me.grpZaawansowane.Text = "Opcje zaawansowane (parametry eksportu i korekty)"

        Me.chkKorektaObrazu.AutoSize = True
        Me.chkKorektaObrazu.Location = New System.Drawing.Point(16, 22)
        Me.chkKorektaObrazu.Name = "chkKorektaObrazu"
        Me.chkKorektaObrazu.Size = New System.Drawing.Size(350, 19)
        Me.chkKorektaObrazu.Text = "Korekta obrazu (MIP, wyostrzanie, kontrast, nasycenie)"
        Me.chkKorektaObrazu.UseVisualStyleBackColor = True

        Me.lblProfil.AutoSize = True
        Me.lblProfil.Location = New System.Drawing.Point(36, 48)
        Me.lblProfil.Name = "lblProfil"
        Me.lblProfil.Size = New System.Drawing.Size(37, 15)
        Me.lblProfil.Text = "Profil:"

        Me.cboProfilKorekty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboProfilKorekty.FormattingEnabled = True
        Me.cboProfilKorekty.Items.AddRange(New Object() {
            "Garmin MIP (podbity kontrast w słońcu)",
            "Wyostrzenie krawędzi (ostrzejsze napisy)",
            "Kontrast i nasycenie",
            "Jasne lasy (rozjaśnienie ciemnych drzewostanów)",
            "Stare mapy (korekta papieru WIG)"
        })
        Me.cboProfilKorekty.Location = New System.Drawing.Point(78, 45)
        Me.cboProfilKorekty.Name = "cboProfilKorekty"
        Me.cboProfilKorekty.Size = New System.Drawing.Size(280, 23)
        Me.cboProfilKorekty.SelectedIndex = 0

        Me.btnDostosujKorekte.Location = New System.Drawing.Point(368, 43)
        Me.btnDostosujKorekte.Name = "btnDostosujKorekte"
        Me.btnDostosujKorekte.Size = New System.Drawing.Size(130, 26)
        Me.btnDostosujKorekte.Text = "Dostosuj suwaki..."
        Me.btnDostosujKorekte.UseVisualStyleBackColor = True

        Me.chkWlasnyZoom.AutoSize = True
        Me.chkWlasnyZoom.Location = New System.Drawing.Point(16, 78)
        Me.chkWlasnyZoom.Name = "chkWlasnyZoom"
        Me.chkWlasnyZoom.Size = New System.Drawing.Size(305, 19)
        Me.chkWlasnyZoom.Text = "Własny zakres powiększeń (MBTiles / Garmin):"
        Me.chkWlasnyZoom.UseVisualStyleBackColor = True

        Me.lblZoomMin.AutoSize = True
        Me.lblZoomMin.Location = New System.Drawing.Point(330, 80)
        Me.lblZoomMin.Name = "lblZoomMin"
        Me.lblZoomMin.Size = New System.Drawing.Size(64, 15)
        Me.lblZoomMin.Text = "Powiększenie od:"

        Me.numZoomMin.Location = New System.Drawing.Point(398, 77)
        Me.numZoomMin.Maximum = New Decimal(New Integer() {20, 0, 0, 0})
        Me.numZoomMin.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.numZoomMin.Name = "numZoomMin"
        Me.numZoomMin.Size = New System.Drawing.Size(50, 23)
        Me.numZoomMin.Value = New Decimal(New Integer() {10, 0, 0, 0})

        Me.lblZoomMax.AutoSize = True
        Me.lblZoomMax.Location = New System.Drawing.Point(465, 80)
        Me.lblZoomMax.Name = "lblZoomMax"
        Me.lblZoomMax.Size = New System.Drawing.Size(66, 15)
        Me.lblZoomMax.Text = "do:"

        Me.numZoomMax.Location = New System.Drawing.Point(535, 77)
        Me.numZoomMax.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.numZoomMax.Minimum = New Decimal(New Integer() {6, 0, 0, 0})
        Me.numZoomMax.Name = "numZoomMax"
        Me.numZoomMax.Size = New System.Drawing.Size(50, 23)
        Me.numZoomMax.Value = New Decimal(New Integer() {16, 0, 0, 0})

        Me.lblDrawOrder.AutoSize = True
        Me.lblDrawOrder.Location = New System.Drawing.Point(16, 144)
        Me.lblDrawOrder.Name = "lblDrawOrder"
        Me.lblDrawOrder.Size = New System.Drawing.Size(268, 15)
        Me.lblDrawOrder.Text = "Kolejność warstwy w Garminie (10-100):"

        '
        ' lblRozmiarPiksela / numRozmiarPiksela / lblPikselJednostka
        '
        Me.lblRozmiarPiksela.AutoSize = True
        Me.lblRozmiarPiksela.Location = New System.Drawing.Point(16, 110)
        Me.lblRozmiarPiksela.Name = "lblRozmiarPiksela"
        Me.lblRozmiarPiksela.Size = New System.Drawing.Size(205, 15)
        Me.lblRozmiarPiksela.Text = "Rozmiar piksela siatki (dokładność):"

        Me.numRozmiarPiksela.DecimalPlaces = 2
        Me.numRozmiarPiksela.Increment = New Decimal(New Integer() {5, 0, 0, 65536}) ' 0.5
        Me.numRozmiarPiksela.Location = New System.Drawing.Point(228, 107)
        Me.numRozmiarPiksela.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.numRozmiarPiksela.Minimum = New Decimal(New Integer() {1, 0, 0, 65536}) ' 0.1
        Me.numRozmiarPiksela.Name = "numRozmiarPiksela"
        Me.numRozmiarPiksela.Size = New System.Drawing.Size(65, 23)
        Me.numRozmiarPiksela.Value = New Decimal(New Integer() {1, 0, 0, 0})

        Me.lblPikselJednostka.AutoSize = True
        Me.lblPikselJednostka.ForeColor = System.Drawing.Color.DimGray
        Me.lblPikselJednostka.Location = New System.Drawing.Point(298, 110)
        Me.lblPikselJednostka.Name = "lblPikselJednostka"
        Me.lblPikselJednostka.Size = New System.Drawing.Size(395, 15)
        Me.lblPikselJednostka.Text = "m/px (wyższa wartość = mniejszy plik i szybsze pobieranie)"

        Me.numDrawOrder.Location = New System.Drawing.Point(290, 141)
        Me.numDrawOrder.Maximum = New Decimal(New Integer() {100, 0, 0, 0})
        Me.numDrawOrder.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.numDrawOrder.Name = "numDrawOrder"
        Me.numDrawOrder.Size = New System.Drawing.Size(55, 23)
        Me.numDrawOrder.Value = New Decimal(New Integer() {50, 0, 0, 0})

        Me.chkPodzialTomow.AutoSize = True
        Me.chkPodzialTomow.Checked = True
        Me.chkPodzialTomow.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkPodzialTomow.Location = New System.Drawing.Point(375, 143)
        Me.chkPodzialTomow.Name = "chkPodzialTomow"
        Me.chkPodzialTomow.Size = New System.Drawing.Size(325, 19)
        Me.chkPodzialTomow.Text = "Automatyczny podział KMZ na tomy (gdy ponad 100 kafli)"
        Me.chkPodzialTomow.UseVisualStyleBackColor = True

        '
        ' lblUkladCad / cboUkladCad / lblCadInfo
        '
        Me.lblUkladCad.AutoSize = True
        Me.lblUkladCad.Location = New System.Drawing.Point(16, 144)
        Me.lblUkladCad.Name = "lblUkladCad"
        Me.lblUkladCad.Size = New System.Drawing.Size(142, 15)
        Me.lblUkladCad.Text = "Układ współrzędnych CAD:"
        Me.lblUkladCad.Visible = False

        Me.cboUkladCad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboUkladCad.FormattingEnabled = True
        Me.cboUkladCad.Items.AddRange(New Object() {
            "PL-2000 strefa 5 (EPSG:2176)",
            "PL-2000 strefa 6 (EPSG:2177)",
            "PL-2000 strefa 7 (EPSG:2178)",
            "PL-2000 strefa 8 (EPSG:2179)",
            "PL-1992 (EPSG:2180)"
        })
        Me.cboUkladCad.Location = New System.Drawing.Point(165, 141)
        Me.cboUkladCad.Name = "cboUkladCad"
        Me.cboUkladCad.Size = New System.Drawing.Size(220, 23)
        Me.cboUkladCad.Visible = False

        Me.lblCadInfo.AutoSize = True
        Me.lblCadInfo.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblCadInfo.Location = New System.Drawing.Point(395, 144)
        Me.lblCadInfo.Name = "lblCadInfo"
        Me.lblCadInfo.Size = New System.Drawing.Size(260, 15)
        Me.lblCadInfo.Text = "Generowany jest raster + skrypt wpasowania (.scr)"
        Me.lblCadInfo.Visible = False

        Me.btnOtworzWieloskalowy.Location = New System.Drawing.Point(16, 175)
        Me.btnOtworzWieloskalowy.Name = "btnOtworzWieloskalowy"
        Me.btnOtworzWieloskalowy.Size = New System.Drawing.Size(280, 30)
        Me.btnOtworzWieloskalowy.Text = "Zakresy powiększeń i warstwy..."
        Me.btnOtworzWieloskalowy.UseVisualStyleBackColor = True

        Me.lblWieloskalowyOpis.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblWieloskalowyOpis.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80)
        Me.lblWieloskalowyOpis.Location = New System.Drawing.Point(305, 179)
        Me.lblWieloskalowyOpis.Name = "lblWieloskalowyOpis"
        Me.lblWieloskalowyOpis.Size = New System.Drawing.Size(425, 30)
        Me.lblWieloskalowyOpis.Text = "Okno MBTiles wieloskalowego: zakresy powiększeń warstw, krycie, mieszanie i podgląd kafli."

        Me.grpZaawansowane.Controls.Add(Me.chkKorektaObrazu)
        Me.grpZaawansowane.Controls.Add(Me.lblProfil)
        Me.grpZaawansowane.Controls.Add(Me.cboProfilKorekty)
        Me.grpZaawansowane.Controls.Add(Me.btnDostosujKorekte)
        Me.grpZaawansowane.Controls.Add(Me.chkWlasnyZoom)
        Me.grpZaawansowane.Controls.Add(Me.lblZoomMin)
        Me.grpZaawansowane.Controls.Add(Me.numZoomMin)
        Me.grpZaawansowane.Controls.Add(Me.lblZoomMax)
        Me.grpZaawansowane.Controls.Add(Me.numZoomMax)
        Me.grpZaawansowane.Controls.Add(Me.lblDrawOrder)
        Me.grpZaawansowane.Controls.Add(Me.numDrawOrder)
        Me.grpZaawansowane.Controls.Add(Me.lblUkladCad)
        Me.grpZaawansowane.Controls.Add(Me.cboUkladCad)
        Me.grpZaawansowane.Controls.Add(Me.lblCadInfo)
        Me.grpZaawansowane.Controls.Add(Me.lblRozmiarPiksela)
        Me.grpZaawansowane.Controls.Add(Me.numRozmiarPiksela)
        Me.grpZaawansowane.Controls.Add(Me.lblPikselJednostka)
        Me.grpZaawansowane.Controls.Add(Me.chkPodzialTomow)
        Me.grpZaawansowane.Controls.Add(Me.btnOtworzWieloskalowy)
        Me.grpZaawansowane.Controls.Add(Me.lblWieloskalowyOpis)

        Me.pnlKrok4.Controls.Add(Me.grpWariant)
        Me.pnlKrok4.Controls.Add(Me.grpSzacunki)
        Me.pnlKrok4.Controls.Add(Me.grpZapis)
        Me.pnlKrok4.Controls.Add(Me.grpZaawansowane)

        '
        ' pnlKrok5: Postęp i wynik
        '
        Me.pnlKrok5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrok5.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrok5.Name = "pnlKrok5"
        Me.pnlKrok5.Size = New System.Drawing.Size(774, 444)

        Me.lblStanPostepu.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblStanPostepu.ForeColor = System.Drawing.Color.FromArgb(16, 44, 87)
        Me.lblStanPostepu.Location = New System.Drawing.Point(40, 50)
        Me.lblStanPostepu.Name = "lblStanPostepu"
        Me.lblStanPostepu.Size = New System.Drawing.Size(694, 30)
        Me.lblStanPostepu.Text = "Pobieranie mapy z serwerów..."

        Me.lblSzczegolyPostepu.Font = New System.Drawing.Font("Segoe UI", 9.5!)
        Me.lblSzczegolyPostepu.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70)
        Me.lblSzczegolyPostepu.Location = New System.Drawing.Point(40, 85)
        Me.lblSzczegolyPostepu.Name = "lblSzczegolyPostepu"
        Me.lblSzczegolyPostepu.Size = New System.Drawing.Size(694, 25)
        Me.lblSzczegolyPostepu.Text = "Inicjalizacja zadań..."

        Me.pbPostep.Location = New System.Drawing.Point(40, 120)
        Me.pbPostep.Name = "pbPostep"
        Me.pbPostep.Size = New System.Drawing.Size(694, 28)

        Me.btnPrzerwij.Location = New System.Drawing.Point(40, 165)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(110, 32)
        Me.btnPrzerwij.Text = "Przerwij"
        Me.btnPrzerwij.UseVisualStyleBackColor = True

        Me.pnlSukces.Location = New System.Drawing.Point(40, 215)
        Me.pnlSukces.Name = "pnlSukces"
        Me.pnlSukces.Size = New System.Drawing.Size(694, 180)
        Me.pnlSukces.Visible = False

        Me.lblSukcesTytul.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblSukcesTytul.ForeColor = System.Drawing.Color.FromArgb(0, 130, 40)
        Me.lblSukcesTytul.Location = New System.Drawing.Point(0, 10)
        Me.lblSukcesTytul.Name = "lblSukcesTytul"
        Me.lblSukcesTytul.Size = New System.Drawing.Size(690, 30)
        Me.lblSukcesTytul.Text = "✓ Mapa została pomyślnie wygenerowana!"

        Me.lblSukcesPlik.Font = New System.Drawing.Font("Segoe UI", 9.5!)
        Me.lblSukcesPlik.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50)
        Me.lblSukcesPlik.Location = New System.Drawing.Point(2, 48)
        Me.lblSukcesPlik.Name = "lblSukcesPlik"
        Me.lblSukcesPlik.Size = New System.Drawing.Size(690, 44)
        Me.lblSukcesPlik.Text = "Plik wynikowy: "

        Me.btnPokazWFolderze.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnPokazWFolderze.Location = New System.Drawing.Point(2, 105)
        Me.btnPokazWFolderze.Name = "btnPokazWFolderze"
        Me.btnPokazWFolderze.Size = New System.Drawing.Size(150, 34)
        Me.btnPokazWFolderze.Text = "Pokaż w folderze"
        Me.btnPokazWFolderze.UseVisualStyleBackColor = True

        Me.btnZaladujDoProgramu.Location = New System.Drawing.Point(162, 105)
        Me.btnZaladujDoProgramu.Name = "btnZaladujDoProgramu"
        Me.btnZaladujDoProgramu.Size = New System.Drawing.Size(230, 34)
        Me.btnZaladujDoProgramu.Text = "Przejdź do mapy w oknie głównym"
        Me.btnZaladujDoProgramu.UseVisualStyleBackColor = True

        Me.pnlSukces.Controls.Add(Me.lblSukcesTytul)
        Me.pnlSukces.Controls.Add(Me.lblSukcesPlik)
        Me.pnlSukces.Controls.Add(Me.btnPokazWFolderze)
        Me.pnlSukces.Controls.Add(Me.btnZaladujDoProgramu)

        Me.pnlKrok5.Controls.Add(Me.lblStanPostepu)
        Me.pnlKrok5.Controls.Add(Me.lblSzczegolyPostepu)
        Me.pnlKrok5.Controls.Add(Me.pbPostep)
        Me.pnlKrok5.Controls.Add(Me.btnPrzerwij)
        Me.pnlKrok5.Controls.Add(Me.pnlSukces)

        '
        ' FormKreatorMapy
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(774, 560)
        Me.Controls.Add(Me.pnlZawartosc)
        Me.Controls.Add(Me.pnlStopka)
        Me.Controls.Add(Me.pnlNaglowek)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormKreatorMapy"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Szybki kreator generowania mapy"

        Me.pnlNaglowek.ResumeLayout(False)
        Me.pnlNaglowek.PerformLayout()
        Me.pnlStopka.ResumeLayout(False)
        Me.pnlZawartosc.ResumeLayout(False)
        Me.pnlKrok1.ResumeLayout(False)
        Me.pnlKrok1.PerformLayout()
        Me.pnlKrok2.ResumeLayout(False)
        Me.pnlKrok2.PerformLayout()
        Me.grpSzczegolySzablonu.ResumeLayout(False)
        Me.grpSzczegolySzablonu.PerformLayout()
        CType(Me.tbCieniowanie, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picPodgladStylu, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlKrok3.ResumeLayout(False)
        Me.pnlKrok3.PerformLayout()
        Me.pnlObszarMiejscowosc.ResumeLayout(False)
        Me.pnlObszarMiejscowosc.PerformLayout()
        Me.pnlObszarMapa.ResumeLayout(False)
        Me.pnlObszarTrasa.ResumeLayout(False)
        Me.pnlObszarTrasa.PerformLayout()
        CType(Me.numKorytarz, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpPodgladObszaru.ResumeLayout(False)
        CType(Me.picPodgladObszaru, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlKrok4.ResumeLayout(False)
        Me.grpWariant.ResumeLayout(False)
        Me.grpWariant.PerformLayout()
        Me.grpSzacunki.ResumeLayout(False)
        Me.grpSzacunki.PerformLayout()
        Me.grpZapis.ResumeLayout(False)
        Me.grpZapis.PerformLayout()
        Me.grpZaawansowane.ResumeLayout(False)
        Me.grpZaawansowane.PerformLayout()
        CType(Me.numZoomMin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numZoomMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numDrawOrder, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlKrok5.ResumeLayout(False)
        Me.pnlSukces.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

End Class
