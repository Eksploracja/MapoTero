<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormZaawansowanyMbtiles
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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormZaawansowanyMbtiles))
        Me.grpWarstwy = New System.Windows.Forms.GroupBox()
        Me.dgvWarstwy = New System.Windows.Forms.DataGridView()
        Me.colWlaczona = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colTytul = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPoziomMin = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPoziomMax = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colKrycie = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTryb = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colSzare = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colBiale = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colFolder = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnDodajSesje = New System.Windows.Forms.Button()
        Me.btnUsunSesje = New System.Windows.Forms.Button()
        Me.btnGora = New System.Windows.Forms.Button()
        Me.btnDol = New System.Windows.Forms.Button()
        Me.grpPodglad = New System.Windows.Forms.GroupBox()
        Me.lblPodgladInfo = New System.Windows.Forms.Label()
        Me.picPodglad = New System.Windows.Forms.PictureBox()
        Me.lblZoomWartosc = New System.Windows.Forms.Label()
        Me.tbZoom = New System.Windows.Forms.TrackBar()
        Me.lblZoom = New System.Windows.Forms.Label()
        Me.btnCentrumMapy = New System.Windows.Forms.Button()
        Me.grpUstawienia = New System.Windows.Forms.GroupBox()
        Me.lblPlik = New System.Windows.Forms.Label()
        Me.txtPlikWynikowy = New System.Windows.Forms.TextBox()
        Me.btnWybierzPlik = New System.Windows.Forms.Button()
        Me.btnPokazWFolderze = New System.Windows.Forms.Button()
        Me.lblNazwa = New System.Windows.Forms.Label()
        Me.txtNazwaMapy = New System.Windows.Forms.TextBox()
        Me.lblFormat = New System.Windows.Forms.Label()
        Me.cmbFormat = New System.Windows.Forms.ComboBox()
        Me.lblJakosc = New System.Windows.Forms.Label()
        Me.nudJakosc = New System.Windows.Forms.NumericUpDown()
        Me.chkPiramida = New System.Windows.Forms.CheckBox()
        Me.chkWypelnijLuki = New System.Windows.Forms.CheckBox()
        Me.grpSzacunek = New System.Windows.Forms.GroupBox()
        Me.lvwPoziomy = New System.Windows.Forms.ListView()
        Me.colZ = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colKafle = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colRozmiar = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colWarstwa = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.btnOszacuj = New System.Windows.Forms.Button()
        Me.lblSzacunekPodsumowanie = New System.Windows.Forms.Label()
        Me.lblStatusFat32 = New System.Windows.Forms.Label()
        Me.txtLog = New System.Windows.Forms.RichTextBox()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.btnPomoc = New System.Windows.Forms.Button()
        Me.btnKompiluj = New System.Windows.Forms.Button()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.dlgSave = New System.Windows.Forms.SaveFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpWarstwy.SuspendLayout()
        CType(Me.dgvWarstwy, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpPodglad.SuspendLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbZoom, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpUstawienia.SuspendLayout()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpSzacunek.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpWarstwy
        '
        Me.grpWarstwy.Controls.Add(Me.dgvWarstwy)
        Me.grpWarstwy.Controls.Add(Me.btnDodajSesje)
        Me.grpWarstwy.Controls.Add(Me.btnUsunSesje)
        Me.grpWarstwy.Controls.Add(Me.btnGora)
        Me.grpWarstwy.Controls.Add(Me.btnDol)
        Me.grpWarstwy.Location = New System.Drawing.Point(12, 12)
        Me.grpWarstwy.Name = "grpWarstwy"
        Me.grpWarstwy.Size = New System.Drawing.Size(610, 240)
        Me.grpWarstwy.TabIndex = 0
        Me.grpWarstwy.TabStop = False
        Me.grpWarstwy.Text = "Warstwy i zakresy wyświetlania (góra listy = wierzch mapy)"
        '
        'dgvWarstwy
        '
        Me.dgvWarstwy.AllowUserToAddRows = False
        Me.dgvWarstwy.AllowUserToDeleteRows = False
        Me.dgvWarstwy.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvWarstwy.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colWlaczona, Me.colTytul, Me.colPoziomMin, Me.colPoziomMax, Me.colKrycie, Me.colTryb, Me.colSzare, Me.colBiale, Me.colFolder})
        Me.dgvWarstwy.Location = New System.Drawing.Point(10, 22)
        Me.dgvWarstwy.MultiSelect = False
        Me.dgvWarstwy.Name = "dgvWarstwy"
        Me.dgvWarstwy.RowHeadersVisible = False
        Me.dgvWarstwy.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvWarstwy.Size = New System.Drawing.Size(590, 175)
        Me.dgvWarstwy.TabIndex = 0
        '
        'colWlaczona
        '
        Me.colWlaczona.HeaderText = "Wł."
        Me.colWlaczona.Name = "colWlaczona"
        Me.colWlaczona.Width = 30
        '
        'colTytul
        '
        Me.colTytul.HeaderText = "Tytuł / Mapa"
        Me.colTytul.Name = "colTytul"
        Me.colTytul.Width = 115
        '
        'colPoziomMin
        '
        Me.colPoziomMin.HeaderText = "Zoom Od"
        Me.colPoziomMin.Name = "colPoziomMin"
        Me.colPoziomMin.Width = 50
        '
        'colPoziomMax
        '
        Me.colPoziomMax.HeaderText = "Zoom Do"
        Me.colPoziomMax.Name = "colPoziomMax"
        Me.colPoziomMax.Width = 50
        '
        'colKrycie
        '
        Me.colKrycie.HeaderText = "Krycie %"
        Me.colKrycie.Name = "colKrycie"
        Me.colKrycie.Width = 45
        '
        'colTryb
        '
        Me.colTryb.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing
        Me.colTryb.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.colTryb.HeaderText = "Mieszanie"
        Me.colTryb.Items.AddRange(New Object() {"1 Zwykły", "2 Mnożenie", "3 Ciemniej", "4 Jaśniej", "5 Ekran", "6 Nakładka"})
        Me.colTryb.Name = "colTryb"
        Me.colTryb.Width = 95
        '
        'colSzare
        '
        Me.colSzare.HeaderText = "Szarość"
        Me.colSzare.Name = "colSzare"
        Me.colSzare.Width = 55
        '
        'colBiale
        '
        Me.colBiale.HeaderText = "Białe przezr."
        Me.colBiale.Name = "colBiale"
        Me.colBiale.Width = 65
        '
        'colFolder
        '
        Me.colFolder.HeaderText = "Folder sesji"
        Me.colFolder.Name = "colFolder"
        Me.colFolder.ReadOnly = True
        Me.colFolder.Width = 85
        '
        'btnDodajSesje
        '
        Me.btnDodajSesje.Location = New System.Drawing.Point(10, 204)
        Me.btnDodajSesje.Name = "btnDodajSesje"
        Me.btnDodajSesje.Size = New System.Drawing.Size(120, 26)
        Me.btnDodajSesje.TabIndex = 1
        Me.btnDodajSesje.Text = "Dodaj warstwę..."
        Me.btnDodajSesje.UseVisualStyleBackColor = True
        '
        'btnUsunSesje
        '
        Me.btnUsunSesje.Location = New System.Drawing.Point(136, 204)
        Me.btnUsunSesje.Name = "btnUsunSesje"
        Me.btnUsunSesje.Size = New System.Drawing.Size(95, 26)
        Me.btnUsunSesje.TabIndex = 2
        Me.btnUsunSesje.Text = "Usuń warstwę"
        Me.btnUsunSesje.UseVisualStyleBackColor = True
        '
        'btnGora
        '
        Me.btnGora.Location = New System.Drawing.Point(440, 204)
        Me.btnGora.Name = "btnGora"
        Me.btnGora.Size = New System.Drawing.Size(75, 26)
        Me.btnGora.TabIndex = 3
        Me.btnGora.Text = "W górę"
        Me.btnGora.UseVisualStyleBackColor = True
        '
        'btnDol
        '
        Me.btnDol.Location = New System.Drawing.Point(525, 204)
        Me.btnDol.Name = "btnDol"
        Me.btnDol.Size = New System.Drawing.Size(75, 26)
        Me.btnDol.TabIndex = 4
        Me.btnDol.Text = "W dół"
        Me.btnDol.UseVisualStyleBackColor = True
        '
        'grpPodglad
        '
        Me.grpPodglad.Controls.Add(Me.lblPodgladInfo)
        Me.grpPodglad.Controls.Add(Me.picPodglad)
        Me.grpPodglad.Controls.Add(Me.lblZoomWartosc)
        Me.grpPodglad.Controls.Add(Me.tbZoom)
        Me.grpPodglad.Controls.Add(Me.lblZoom)
        Me.grpPodglad.Controls.Add(Me.btnCentrumMapy)
        Me.grpPodglad.Location = New System.Drawing.Point(632, 12)
        Me.grpPodglad.Name = "grpPodglad"
        Me.grpPodglad.Size = New System.Drawing.Size(320, 435)
        Me.grpPodglad.TabIndex = 1
        Me.grpPodglad.TabStop = False
        Me.grpPodglad.Text = "Symulacja i podgląd zoomu"
        '
        'lblPodgladInfo
        '
        Me.lblPodgladInfo.BackColor = System.Drawing.SystemColors.ControlLight
        Me.lblPodgladInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblPodgladInfo.Location = New System.Drawing.Point(15, 335)
        Me.lblPodgladInfo.Name = "lblPodgladInfo"
        Me.lblPodgladInfo.Size = New System.Drawing.Size(290, 55)
        Me.lblPodgladInfo.TabIndex = 5
        Me.lblPodgladInfo.Text = "Brak warstw do podglądu."
        Me.lblPodgladInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'picPodglad
        '
        Me.picPodglad.BackColor = System.Drawing.Color.White
        Me.picPodglad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodglad.Location = New System.Drawing.Point(32, 70)
        Me.picPodglad.Name = "picPodglad"
        Me.picPodglad.Size = New System.Drawing.Size(256, 256)
        Me.picPodglad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.picPodglad.TabIndex = 4
        Me.picPodglad.TabStop = False
        '
        'lblZoomWartosc
        '
        Me.lblZoomWartosc.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.lblZoomWartosc.Location = New System.Drawing.Point(260, 26)
        Me.lblZoomWartosc.Name = "lblZoomWartosc"
        Me.lblZoomWartosc.Size = New System.Drawing.Size(45, 23)
        Me.lblZoomWartosc.TabIndex = 3
        Me.lblZoomWartosc.Text = "14"
        Me.lblZoomWartosc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'tbZoom
        '
        Me.tbZoom.AutoSize = False
        Me.tbZoom.Location = New System.Drawing.Point(85, 25)
        Me.tbZoom.Maximum = 20
        Me.tbZoom.Minimum = 6
        Me.tbZoom.Name = "tbZoom"
        Me.tbZoom.Size = New System.Drawing.Size(170, 30)
        Me.tbZoom.TabIndex = 2
        Me.tbZoom.TickStyle = System.Windows.Forms.TickStyle.None
        Me.tbZoom.Value = 14
        '
        'lblZoom
        '
        Me.lblZoom.AutoSize = True
        Me.lblZoom.Location = New System.Drawing.Point(12, 29)
        Me.lblZoom.Name = "lblZoom"
        Me.lblZoom.Size = New System.Drawing.Size(71, 13)
        Me.lblZoom.TabIndex = 1
        Me.lblZoom.Text = "Poziom zoom:"
        '
        'btnCentrumMapy
        '
        Me.btnCentrumMapy.Location = New System.Drawing.Point(15, 397)
        Me.btnCentrumMapy.Name = "btnCentrumMapy"
        Me.btnCentrumMapy.Size = New System.Drawing.Size(290, 26)
        Me.btnCentrumMapy.TabIndex = 0
        Me.btnCentrumMapy.Text = "Pobierz środek z mapy głównej"
        Me.btnCentrumMapy.UseVisualStyleBackColor = True
        '
        'grpUstawienia
        '
        Me.grpUstawienia.Controls.Add(Me.lblPlik)
        Me.grpUstawienia.Controls.Add(Me.txtPlikWynikowy)
        Me.grpUstawienia.Controls.Add(Me.btnWybierzPlik)
        Me.grpUstawienia.Controls.Add(Me.btnPokazWFolderze)
        Me.grpUstawienia.Controls.Add(Me.lblNazwa)
        Me.grpUstawienia.Controls.Add(Me.txtNazwaMapy)
        Me.grpUstawienia.Controls.Add(Me.lblFormat)
        Me.grpUstawienia.Controls.Add(Me.cmbFormat)
        Me.grpUstawienia.Controls.Add(Me.lblJakosc)
        Me.grpUstawienia.Controls.Add(Me.nudJakosc)
        Me.grpUstawienia.Controls.Add(Me.chkPiramida)
        Me.grpUstawienia.Controls.Add(Me.chkWypelnijLuki)
        Me.grpUstawienia.Location = New System.Drawing.Point(12, 258)
        Me.grpUstawienia.Name = "grpUstawienia"
        Me.grpUstawienia.Size = New System.Drawing.Size(610, 189)
        Me.grpUstawienia.TabIndex = 2
        Me.grpUstawienia.TabStop = False
        Me.grpUstawienia.Text = "Parametry pliku MBTiles i kompresji"
        '
        'lblPlik
        '
        Me.lblPlik.AutoSize = True
        Me.lblPlik.Location = New System.Drawing.Point(10, 25)
        Me.lblPlik.Name = "lblPlik"
        Me.lblPlik.Size = New System.Drawing.Size(73, 13)
        Me.lblPlik.TabIndex = 0
        Me.lblPlik.Text = "Plik wynikowy:"
        '
        'txtPlikWynikowy
        '
        Me.txtPlikWynikowy.Location = New System.Drawing.Point(89, 22)
        Me.txtPlikWynikowy.Name = "txtPlikWynikowy"
        Me.txtPlikWynikowy.Size = New System.Drawing.Size(345, 20)
        Me.txtPlikWynikowy.TabIndex = 1
        '
        'btnWybierzPlik
        '
        Me.btnWybierzPlik.Location = New System.Drawing.Point(440, 20)
        Me.btnWybierzPlik.Name = "btnWybierzPlik"
        Me.btnWybierzPlik.Size = New System.Drawing.Size(75, 23)
        Me.btnWybierzPlik.TabIndex = 2
        Me.btnWybierzPlik.Text = "Wybierz..."
        Me.btnWybierzPlik.UseVisualStyleBackColor = True
        '
        'btnPokazWFolderze
        '
        Me.btnPokazWFolderze.Location = New System.Drawing.Point(520, 20)
        Me.btnPokazWFolderze.Name = "btnPokazWFolderze"
        Me.btnPokazWFolderze.Size = New System.Drawing.Size(80, 23)
        Me.btnPokazWFolderze.TabIndex = 3
        Me.btnPokazWFolderze.Text = "Pokaż..."
        Me.ToolTip1.SetToolTip(Me.btnPokazWFolderze, "Otwiera folder i zaznacza plik MBTiles w Eksploratorze Windows")
        Me.btnPokazWFolderze.UseVisualStyleBackColor = True
        '
        'lblNazwa
        '
        Me.lblNazwa.AutoSize = True
        Me.lblNazwa.Location = New System.Drawing.Point(10, 56)
        Me.lblNazwa.Name = "lblNazwa"
        Me.lblNazwa.Size = New System.Drawing.Size(71, 13)
        Me.lblNazwa.TabIndex = 3
        Me.lblNazwa.Text = "Nazwa mapy:"
        '
        'txtNazwaMapy
        '
        Me.txtNazwaMapy.Location = New System.Drawing.Point(89, 53)
        Me.txtNazwaMapy.Name = "txtNazwaMapy"
        Me.txtNazwaMapy.Size = New System.Drawing.Size(240, 20)
        Me.txtNazwaMapy.TabIndex = 4
        Me.txtNazwaMapy.Text = "Mapa wielowarstwowa"
        '
        'lblFormat
        '
        Me.lblFormat.AutoSize = True
        Me.lblFormat.Location = New System.Drawing.Point(10, 88)
        Me.lblFormat.Name = "lblFormat"
        Me.lblFormat.Size = New System.Drawing.Size(69, 13)
        Me.lblFormat.TabIndex = 5
        Me.lblFormat.Text = "Format kafli:"
        '
        'cmbFormat
        '
        Me.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFormat.FormattingEnabled = True
        Me.cmbFormat.Items.AddRange(New Object() {"Smart Hybrid (JPG + PNG przezroczyste)", "Tylko JPEG (mniejszy rozmiar, białe tło)", "Tylko PNG (bezstratny, pełna przezroczystość)"})
        Me.cmbFormat.Location = New System.Drawing.Point(89, 85)
        Me.cmbFormat.Name = "cmbFormat"
        Me.cmbFormat.Size = New System.Drawing.Size(240, 21)
        Me.cmbFormat.TabIndex = 6
        '
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Location = New System.Drawing.Point(355, 88)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(89, 13)
        Me.lblJakosc.TabIndex = 7
        Me.lblJakosc.Text = "Jakość JPEG (%):"
        '
        'nudJakosc
        '
        Me.nudJakosc.Location = New System.Drawing.Point(450, 86)
        Me.nudJakosc.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudJakosc.Name = "nudJakosc"
        Me.nudJakosc.Size = New System.Drawing.Size(60, 20)
        Me.nudJakosc.TabIndex = 8
        Me.nudJakosc.Value = New Decimal(New Integer() {85, 0, 0, 0})
        '
        'chkPiramida
        '
        Me.chkPiramida.AutoSize = True
        Me.chkPiramida.Checked = True
        Me.chkPiramida.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkPiramida.Location = New System.Drawing.Point(13, 125)
        Me.chkPiramida.Name = "chkPiramida"
        Me.chkPiramida.Size = New System.Drawing.Size(395, 17)
        Me.chkPiramida.TabIndex = 9
        Me.chkPiramida.Text = "Szybka piramida w pasmach warstw (downscale 2x przyspiesza zapis o ~70%)"
        Me.chkPiramida.UseVisualStyleBackColor = True
        '
        'chkWypelnijLuki
        '
        Me.chkWypelnijLuki.AutoSize = True
        Me.chkWypelnijLuki.Location = New System.Drawing.Point(13, 153)
        Me.chkWypelnijLuki.Name = "chkWypelnijLuki"
        Me.chkWypelnijLuki.Size = New System.Drawing.Size(370, 17)
        Me.chkWypelnijLuki.TabIndex = 10
        Me.chkWypelnijLuki.Text = "Wypełnij brakujące poziomy powiększeń przeskalowaną warstwą niższą"
        Me.chkWypelnijLuki.UseVisualStyleBackColor = True
        '
        'grpSzacunek
        '
        Me.grpSzacunek.Controls.Add(Me.lvwPoziomy)
        Me.grpSzacunek.Controls.Add(Me.btnOszacuj)
        Me.grpSzacunek.Controls.Add(Me.lblSzacunekPodsumowanie)
        Me.grpSzacunek.Controls.Add(Me.lblStatusFat32)
        Me.grpSzacunek.Location = New System.Drawing.Point(12, 453)
        Me.grpSzacunek.Name = "grpSzacunek"
        Me.grpSzacunek.Size = New System.Drawing.Size(940, 155)
        Me.grpSzacunek.TabIndex = 3
        Me.grpSzacunek.TabStop = False
        Me.grpSzacunek.Text = "Szacunek rozmiaru pliku i parametrów poziomów zoom"
        '
        'lvwPoziomy
        '
        Me.lvwPoziomy.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colZ, Me.colKafle, Me.colRozmiar, Me.colWarstwa})
        Me.lvwPoziomy.FullRowSelect = True
        Me.lvwPoziomy.GridLines = True
        Me.lvwPoziomy.Location = New System.Drawing.Point(10, 22)
        Me.lvwPoziomy.Name = "lvwPoziomy"
        Me.lvwPoziomy.Size = New System.Drawing.Size(640, 122)
        Me.lvwPoziomy.TabIndex = 0
        Me.lvwPoziomy.UseCompatibleStateImageBehavior = False
        Me.lvwPoziomy.View = System.Windows.Forms.View.Details
        '
        'colZ
        '
        Me.colZ.Text = "Zoom"
        Me.colZ.Width = 50
        '
        'colKafle
        '
        Me.colKafle.Text = "Liczba kafli"
        Me.colKafle.Width = 90
        '
        'colRozmiar
        '
        Me.colRozmiar.Text = "Szacowany rozmiar"
        Me.colRozmiar.Width = 110
        '
        'colWarstwa
        '
        Me.colWarstwa.Text = "Aktywne warstwy"
        Me.colWarstwa.Width = 360
        '
        'btnOszacuj
        '
        Me.btnOszacuj.Location = New System.Drawing.Point(670, 22)
        Me.btnOszacuj.Name = "btnOszacuj"
        Me.btnOszacuj.Size = New System.Drawing.Size(255, 30)
        Me.btnOszacuj.TabIndex = 1
        Me.btnOszacuj.Text = "Oszacuj rozmiar i liczbę kafli"
        Me.btnOszacuj.UseVisualStyleBackColor = True
        '
        'lblSzacunekPodsumowanie
        '
        Me.lblSzacunekPodsumowanie.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.lblSzacunekPodsumowanie.Location = New System.Drawing.Point(670, 60)
        Me.lblSzacunekPodsumowanie.Name = "lblSzacunekPodsumowanie"
        Me.lblSzacunekPodsumowanie.Size = New System.Drawing.Size(255, 35)
        Me.lblSzacunekPodsumowanie.TabIndex = 2
        Me.lblSzacunekPodsumowanie.Text = "Kliknij ""Oszacuj rozmiar"", aby obliczyć parametry pliku."
        '
        'lblStatusFat32
        '
        Me.lblStatusFat32.Location = New System.Drawing.Point(670, 98)
        Me.lblStatusFat32.Name = "lblStatusFat32"
        Me.lblStatusFat32.Size = New System.Drawing.Size(255, 46)
        Me.lblStatusFat32.TabIndex = 3
        Me.lblStatusFat32.Text = "Zgodność z urządzeniami mobilnymi (OruxMaps, Locus Map)"
        '
        'txtLog
        '
        Me.txtLog.Location = New System.Drawing.Point(12, 614)
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ReadOnly = True
        Me.txtLog.Size = New System.Drawing.Size(940, 50)
        Me.txtLog.TabIndex = 4
        Me.txtLog.Text = ""
        '
        'prgPostep
        '
        Me.prgPostep.Location = New System.Drawing.Point(12, 672)
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(555, 26)
        Me.prgPostep.TabIndex = 5
        '
        'btnPomoc
        '
        Me.btnPomoc.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.btnPomoc.Location = New System.Drawing.Point(580, 670)
        Me.btnPomoc.Name = "btnPomoc"
        Me.btnPomoc.Size = New System.Drawing.Size(85, 28)
        Me.btnPomoc.TabIndex = 9
        Me.btnPomoc.Text = "Pomoc (?)"
        Me.ToolTip1.SetToolTip(Me.btnPomoc, "Instrukcja obsługi i wskazówki dotyczące wielowarstwowego MBTiles (F1)")
        Me.btnPomoc.UseVisualStyleBackColor = True
        '
        'btnKompiluj
        '
        Me.btnKompiluj.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btnKompiluj.Location = New System.Drawing.Point(675, 670)
        Me.btnKompiluj.Name = "btnKompiluj"
        Me.btnKompiluj.Size = New System.Drawing.Size(120, 28)
        Me.btnKompiluj.TabIndex = 6
        Me.btnKompiluj.Text = "Kompiluj MBTiles"
        Me.btnKompiluj.UseVisualStyleBackColor = True
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(805, 670)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(70, 28)
        Me.btnPrzerwij.TabIndex = 7
        Me.btnPrzerwij.Text = "Przerwij"
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'btnZamknij
        '
        Me.btnZamknij.Location = New System.Drawing.Point(882, 670)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(70, 28)
        Me.btnZamknij.TabIndex = 8
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'FormZaawansowanyMbtiles
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(964, 710)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.btnPrzerwij)
        Me.Controls.Add(Me.btnPomoc)
        Me.Controls.Add(Me.btnKompiluj)
        Me.Controls.Add(Me.prgPostep)
        Me.Controls.Add(Me.txtLog)
        Me.Controls.Add(Me.grpSzacunek)
        Me.Controls.Add(Me.grpUstawienia)
        Me.Controls.Add(Me.grpPodglad)
        Me.Controls.Add(Me.grpWarstwy)
        Me.KeyPreview = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.Name = "FormZaawansowanyMbtiles"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Zaawansowany kreator wielowarstwowego / wieloskalowego MBTiles"
        Me.grpWarstwy.ResumeLayout(False)
        CType(Me.dgvWarstwy, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpPodglad.ResumeLayout(False)
        Me.grpPodglad.PerformLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbZoom, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpUstawienia.ResumeLayout(False)
        Me.grpUstawienia.PerformLayout()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpSzacunek.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpWarstwy As GroupBox
    Friend WithEvents dgvWarstwy As DataGridView
    Friend WithEvents btnDodajSesje As Button
    Friend WithEvents btnUsunSesje As Button
    Friend WithEvents btnGora As Button
    Friend WithEvents btnDol As Button
    Friend WithEvents grpPodglad As GroupBox
    Friend WithEvents lblPodgladInfo As Label
    Friend WithEvents picPodglad As PictureBox
    Friend WithEvents lblZoomWartosc As Label
    Friend WithEvents tbZoom As TrackBar
    Friend WithEvents lblZoom As Label
    Friend WithEvents btnCentrumMapy As Button
    Friend WithEvents grpUstawienia As GroupBox
    Friend WithEvents lblPlik As Label
    Friend WithEvents txtPlikWynikowy As TextBox
    Friend WithEvents btnWybierzPlik As Button
    Friend WithEvents btnPokazWFolderze As Button
    Friend WithEvents lblNazwa As Label
    Friend WithEvents txtNazwaMapy As TextBox
    Friend WithEvents lblFormat As Label
    Friend WithEvents cmbFormat As ComboBox
    Friend WithEvents lblJakosc As Label
    Friend WithEvents nudJakosc As NumericUpDown
    Friend WithEvents chkPiramida As CheckBox
    Friend WithEvents chkWypelnijLuki As CheckBox
    Friend WithEvents grpSzacunek As GroupBox
    Friend WithEvents lvwPoziomy As ListView
    Friend WithEvents colZ As ColumnHeader
    Friend WithEvents colKafle As ColumnHeader
    Friend WithEvents colRozmiar As ColumnHeader
    Friend WithEvents colWarstwa As ColumnHeader
    Friend WithEvents btnOszacuj As Button
    Friend WithEvents lblSzacunekPodsumowanie As Label
    Friend WithEvents lblStatusFat32 As Label
    Friend WithEvents txtLog As RichTextBox
    Friend WithEvents prgPostep As ProgressBar
    Friend WithEvents btnPomoc As Button
    Friend WithEvents btnKompiluj As Button
    Friend WithEvents btnPrzerwij As Button
    Friend WithEvents btnZamknij As Button
    Friend WithEvents dlgFolder As FolderBrowserDialog
    Friend WithEvents dlgSave As SaveFileDialog
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents colWlaczona As DataGridViewCheckBoxColumn
    Friend WithEvents colTytul As DataGridViewTextBoxColumn
    Friend WithEvents colPoziomMin As DataGridViewTextBoxColumn
    Friend WithEvents colPoziomMax As DataGridViewTextBoxColumn
    Friend WithEvents colKrycie As DataGridViewTextBoxColumn
    Friend WithEvents colTryb As DataGridViewComboBoxColumn
    Friend WithEvents colSzare As DataGridViewCheckBoxColumn
    Friend WithEvents colBiale As DataGridViewCheckBoxColumn
    Friend WithEvents colFolder As DataGridViewTextBoxColumn
End Class
