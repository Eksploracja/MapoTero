<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormEksport
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblFolder = New System.Windows.Forms.Label()
        Me.txtFolder = New System.Windows.Forms.TextBox()
        Me.btnFolder = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.grpFormat = New System.Windows.Forms.GroupBox()
        Me.rbOruxMaps = New System.Windows.Forms.RadioButton()
        Me.rbSQLiteDB = New System.Windows.Forms.RadioButton()
        Me.nudMaksKafli = New System.Windows.Forms.NumericUpDown()
        Me.rbMBTiles = New System.Windows.Forms.RadioButton()
        Me.btnWielowarstwowyMbtiles = New System.Windows.Forms.Button()
        Me.rbKmz = New System.Windows.Forms.RadioButton()
        Me.rbKmzGarmin = New System.Windows.Forms.RadioButton()
        Me.grpMBTiles = New System.Windows.Forms.GroupBox()
        Me.chkPng = New System.Windows.Forms.CheckBox()
        Me.nudPoziomMax = New System.Windows.Forms.NumericUpDown()
        Me.lblPoziomDo = New System.Windows.Forms.Label()
        Me.nudPoziomMin = New System.Windows.Forms.NumericUpDown()
        Me.lblPoziomOd = New System.Windows.Forms.Label()
        Me.grpWarstwy = New System.Windows.Forms.GroupBox()
        Me.chkBialeNakladka = New System.Windows.Forms.CheckBox()
        Me.nudKrycie = New System.Windows.Forms.NumericUpDown()
        Me.lblKrycie = New System.Windows.Forms.Label()
        Me.btnUsunNakladke = New System.Windows.Forms.Button()
        Me.btnDodajNakladke = New System.Windows.Forms.Button()
        Me.lstNakladki = New System.Windows.Forms.ListBox()
        Me.lblNakladki = New System.Windows.Forms.Label()
        Me.chkBialePodstawa = New System.Windows.Forms.CheckBox()
        Me.chkNakladka = New System.Windows.Forms.CheckBox()
        Me.grpRozmiar = New System.Windows.Forms.GroupBox()
        Me.btnDopasuj = New System.Windows.Forms.Button()
        Me.nudLimit = New System.Windows.Forms.NumericUpDown()
        Me.lblLimit = New System.Windows.Forms.Label()
        Me.lblRozmiar = New System.Windows.Forms.Label()
        Me.btnOszacuj = New System.Windows.Forms.Button()
        Me.lblJakosc = New System.Windows.Forms.Label()
        Me.nudJakosc = New System.Windows.Forms.NumericUpDown()
        Me.lblPlan = New System.Windows.Forms.Label()
        Me.txtKomunikaty = New System.Windows.Forms.RichTextBox()
        Me.prgEksport = New System.Windows.Forms.ProgressBar()
        Me.btnEksportuj = New System.Windows.Forms.Button()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpFormat.SuspendLayout()
        CType(Me.nudMaksKafli, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMBTiles.SuspendLayout()
        CType(Me.nudPoziomMax, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudPoziomMin, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpWarstwy.SuspendLayout()
        CType(Me.nudKrycie, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpRozmiar.SuspendLayout()
        CType(Me.nudLimit, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblFolder
        '
        Me.lblFolder.AutoSize = True
        Me.lblFolder.Location = New System.Drawing.Point(12, 12)
        Me.lblFolder.Name = "lblFolder"
        Me.lblFolder.Size = New System.Drawing.Size(196, 13)
        Me.lblFolder.TabIndex = 0
        Me.lblFolder.Text = "Folder segmentów (z plikiem conf.txt):"
        '
        'txtFolder
        '
        Me.txtFolder.Location = New System.Drawing.Point(12, 30)
        Me.txtFolder.Name = "txtFolder"
        Me.txtFolder.ReadOnly = True
        Me.txtFolder.Size = New System.Drawing.Size(400, 20)
        Me.txtFolder.TabIndex = 1
        '
        'btnFolder
        '
        Me.btnFolder.Location = New System.Drawing.Point(418, 28)
        Me.btnFolder.Name = "btnFolder"
        Me.btnFolder.Size = New System.Drawing.Size(80, 23)
        Me.btnFolder.TabIndex = 2
        Me.btnFolder.Text = "zmień folder"
        Me.btnFolder.UseVisualStyleBackColor = True
        '
        'lblInfo
        '
        Me.lblInfo.Location = New System.Drawing.Point(12, 56)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(486, 44)
        Me.lblInfo.TabIndex = 3
        '
        'grpFormat
        '
        Me.grpFormat.Controls.Add(Me.rbOruxMaps)
        Me.grpFormat.Controls.Add(Me.rbSQLiteDB)
        Me.grpFormat.Controls.Add(Me.nudMaksKafli)
        Me.grpFormat.Controls.Add(Me.btnWielowarstwowyMbtiles)
        Me.grpFormat.Controls.Add(Me.rbMBTiles)
        Me.grpFormat.Controls.Add(Me.rbKmz)
        Me.grpFormat.Controls.Add(Me.rbKmzGarmin)
        Me.grpFormat.Location = New System.Drawing.Point(12, 102)
        Me.grpFormat.Name = "grpFormat"
        Me.grpFormat.Size = New System.Drawing.Size(486, 136)
        Me.grpFormat.TabIndex = 4
        Me.grpFormat.TabStop = False
        Me.grpFormat.Text = "Format"
        '
        'rbOruxMaps
        '
        Me.rbOruxMaps.AutoSize = True
        Me.rbOruxMaps.Location = New System.Drawing.Point(10, 108)
        Me.rbOruxMaps.Name = "rbOruxMaps"
        Me.rbOruxMaps.Size = New System.Drawing.Size(400, 17)
        Me.rbOruxMaps.TabIndex = 5
        Me.rbOruxMaps.Text = "OruxMaps - mapa offline (folder z bazą OruxMapsImages.db i kalibracją .otrk2.xml)"
        Me.ToolTip1.SetToolTip(Me.rbOruxMaps, "Cały folder mapy skopiuj do folderu oruxmaps/mapfiles w telefonie.")
        Me.rbOruxMaps.UseVisualStyleBackColor = True
        '
        'rbSQLiteDB
        '
        Me.rbSQLiteDB.AutoSize = True
        Me.rbSQLiteDB.Location = New System.Drawing.Point(10, 86)
        Me.rbSQLiteDB.Name = "rbSQLiteDB"
        Me.rbSQLiteDB.Size = New System.Drawing.Size(330, 17)
        Me.rbSQLiteDB.TabIndex = 4
        Me.rbSQLiteDB.Text = "SQLiteDB - OsmAnd, Locus Map (format RMaps, kafle Web Mercator)"
        Me.ToolTip1.SetToolTip(Me.rbSQLiteDB, "OsmAnd: plik .sqlitedb skopiuj do folderu osmand/tiles; Locus Map: do folderu Locus/mapsOnline lub otwórz plik w aplikacji.")
        Me.rbSQLiteDB.UseVisualStyleBackColor = True
        '
        'nudMaksKafli
        '
        Me.nudMaksKafli.Location = New System.Drawing.Point(410, 18)
        Me.nudMaksKafli.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudMaksKafli.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudMaksKafli.Name = "nudMaksKafli"
        Me.nudMaksKafli.Size = New System.Drawing.Size(60, 20)
        Me.nudMaksKafli.TabIndex = 3
        Me.nudMaksKafli.Value = New Decimal(New Integer() {100, 0, 0, 0})
        Me.ToolTip1.SetToolTip(Me.nudMaksKafli, "Większość odbiorników Garmin wyświetla najwyżej 100 kafli mapy (nowsze modele - 500).")
        '
        'rbMBTiles
        '
        Me.btnWielowarstwowyMbtiles.Location = New System.Drawing.Point(345, 61)
        Me.btnWielowarstwowyMbtiles.Name = "btnWielowarstwowyMbtiles"
        Me.btnWielowarstwowyMbtiles.Size = New System.Drawing.Size(130, 23)
        Me.btnWielowarstwowyMbtiles.TabIndex = 6
        Me.btnWielowarstwowyMbtiles.Text = "Wielowarstwowy..."
        Me.ToolTip1.SetToolTip(Me.btnWielowarstwowyMbtiles, "Otwiera zaawansowany kreator tworzenia pliku MBTiles z różnych warstw i poziomów zoom.")
        Me.btnWielowarstwowyMbtiles.UseVisualStyleBackColor = True
                Me.rbMBTiles.AutoSize = True
        Me.rbMBTiles.Location = New System.Drawing.Point(10, 64)
        Me.rbMBTiles.Name = "rbMBTiles"
        Me.rbMBTiles.Size = New System.Drawing.Size(330, 17)
        Me.rbMBTiles.TabIndex = 2
        Me.rbMBTiles.Text = "MBTiles - Locus Map, OsmAnd, OruxMaps (kafle Web Mercator)"
        Me.rbMBTiles.UseVisualStyleBackColor = True
        '
        'rbKmz
        '
        Me.rbKmz.AutoSize = True
        Me.rbKmz.Location = New System.Drawing.Point(10, 42)
        Me.rbKmz.Name = "rbKmz"
        Me.rbKmz.Size = New System.Drawing.Size(360, 17)
        Me.rbKmz.TabIndex = 1
        Me.rbKmz.Text = "KMZ - Locus Map, OruxMaps, Google Earth (bez limitu liczby kafli)"
        Me.rbKmz.UseVisualStyleBackColor = True
        '
        'rbKmzGarmin
        '
        Me.rbKmzGarmin.AutoSize = True
        Me.rbKmzGarmin.Checked = True
        Me.rbKmzGarmin.Location = New System.Drawing.Point(10, 20)
        Me.rbKmzGarmin.Name = "rbKmzGarmin"
        Me.rbKmzGarmin.Size = New System.Drawing.Size(390, 17)
        Me.rbKmzGarmin.TabIndex = 0
        Me.rbKmzGarmin.TabStop = True
        Me.rbKmzGarmin.Text = "KMZ - Garmin Custom Maps (kafle JPEG do 1024 px), maksymalna liczba kafli:"
        Me.rbKmzGarmin.UseVisualStyleBackColor = True
        '
        'grpMBTiles
        '
        Me.grpMBTiles.Controls.Add(Me.chkPng)
        Me.grpMBTiles.Controls.Add(Me.nudPoziomMax)
        Me.grpMBTiles.Controls.Add(Me.lblPoziomDo)
        Me.grpMBTiles.Controls.Add(Me.nudPoziomMin)
        Me.grpMBTiles.Controls.Add(Me.lblPoziomOd)
        Me.grpMBTiles.Location = New System.Drawing.Point(12, 242)
        Me.grpMBTiles.Name = "grpMBTiles"
        Me.grpMBTiles.Size = New System.Drawing.Size(486, 50)
        Me.grpMBTiles.TabIndex = 5
        Me.grpMBTiles.TabStop = False
        Me.grpMBTiles.Text = "Kafle Web Mercator (MBTiles, SQLiteDB, OruxMaps)"
        '
        'chkPng
        '
        Me.chkPng.AutoSize = True
        Me.chkPng.Location = New System.Drawing.Point(290, 21)
        Me.chkPng.Name = "chkPng"
        Me.chkPng.Size = New System.Drawing.Size(185, 17)
        Me.chkPng.TabIndex = 4
        Me.chkPng.Text = "kafle PNG (przezroczyste brzegi)"
        Me.chkPng.UseVisualStyleBackColor = True
        '
        'nudPoziomMax
        '
        Me.nudPoziomMax.Location = New System.Drawing.Point(210, 19)
        Me.nudPoziomMax.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudPoziomMax.Name = "nudPoziomMax"
        Me.nudPoziomMax.Size = New System.Drawing.Size(45, 20)
        Me.nudPoziomMax.TabIndex = 3
        Me.nudPoziomMax.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'lblPoziomDo
        '
        Me.lblPoziomDo.AutoSize = True
        Me.lblPoziomDo.Location = New System.Drawing.Point(185, 22)
        Me.lblPoziomDo.Name = "lblPoziomDo"
        Me.lblPoziomDo.Size = New System.Drawing.Size(19, 13)
        Me.lblPoziomDo.TabIndex = 2
        Me.lblPoziomDo.Text = "do"
        '
        'nudPoziomMin
        '
        Me.nudPoziomMin.Location = New System.Drawing.Point(134, 19)
        Me.nudPoziomMin.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudPoziomMin.Name = "nudPoziomMin"
        Me.nudPoziomMin.Size = New System.Drawing.Size(45, 20)
        Me.nudPoziomMin.TabIndex = 1
        Me.nudPoziomMin.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblPoziomOd
        '
        Me.lblPoziomOd.AutoSize = True
        Me.lblPoziomOd.Location = New System.Drawing.Point(8, 22)
        Me.lblPoziomOd.Name = "lblPoziomOd"
        Me.lblPoziomOd.Size = New System.Drawing.Size(122, 13)
        Me.lblPoziomOd.TabIndex = 0
        Me.lblPoziomOd.Text = "poziomy powiększenia od"
        '
        'grpWarstwy
        '
        Me.grpWarstwy.Controls.Add(Me.chkBialeNakladka)
        Me.grpWarstwy.Controls.Add(Me.nudKrycie)
        Me.grpWarstwy.Controls.Add(Me.lblKrycie)
        Me.grpWarstwy.Controls.Add(Me.btnUsunNakladke)
        Me.grpWarstwy.Controls.Add(Me.btnDodajNakladke)
        Me.grpWarstwy.Controls.Add(Me.lstNakladki)
        Me.grpWarstwy.Controls.Add(Me.lblNakladki)
        Me.grpWarstwy.Controls.Add(Me.chkBialePodstawa)
        Me.grpWarstwy.Controls.Add(Me.chkNakladka)
        Me.grpWarstwy.Location = New System.Drawing.Point(12, 296)
        Me.grpWarstwy.Name = "grpWarstwy"
        Me.grpWarstwy.Size = New System.Drawing.Size(486, 172)
        Me.grpWarstwy.TabIndex = 6
        Me.grpWarstwy.TabStop = False
        Me.grpWarstwy.Text = "Warstwy i przezroczystość"
        '
        'chkBialeNakladka
        '
        Me.chkBialeNakladka.AutoSize = True
        Me.chkBialeNakladka.Enabled = False
        Me.chkBialeNakladka.Location = New System.Drawing.Point(134, 147)
        Me.chkBialeNakladka.Name = "chkBialeNakladka"
        Me.chkBialeNakladka.Size = New System.Drawing.Size(220, 17)
        Me.chkBialeNakladka.TabIndex = 8
        Me.chkBialeNakladka.Text = "białe tło zaznaczonej warstwy przezroczyste"
        Me.chkBialeNakladka.UseVisualStyleBackColor = True
        '
        'nudKrycie
        '
        Me.nudKrycie.Enabled = False
        Me.nudKrycie.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudKrycie.Location = New System.Drawing.Point(72, 145)
        Me.nudKrycie.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudKrycie.Name = "nudKrycie"
        Me.nudKrycie.Size = New System.Drawing.Size(50, 20)
        Me.nudKrycie.TabIndex = 7
        Me.nudKrycie.Value = New Decimal(New Integer() {100, 0, 0, 0})
        Me.ToolTip1.SetToolTip(Me.nudKrycie, "Krycie zaznaczonej warstwy: 100% - zakrywa mapę pod spodem, 50% - półprzezroczysta (np. cieniowanie rzeźby terenu).")
        '
        'lblKrycie
        '
        Me.lblKrycie.AutoSize = True
        Me.lblKrycie.Location = New System.Drawing.Point(8, 148)
        Me.lblKrycie.Name = "lblKrycie"
        Me.lblKrycie.Size = New System.Drawing.Size(59, 13)
        Me.lblKrycie.TabIndex = 6
        Me.lblKrycie.Text = "krycie [%]:"
        '
        'btnUsunNakladke
        '
        Me.btnUsunNakladke.Enabled = False
        Me.btnUsunNakladke.Location = New System.Drawing.Point(368, 110)
        Me.btnUsunNakladke.Name = "btnUsunNakladke"
        Me.btnUsunNakladke.Size = New System.Drawing.Size(108, 23)
        Me.btnUsunNakladke.TabIndex = 5
        Me.btnUsunNakladke.Text = "usuń warstwę"
        Me.btnUsunNakladke.UseVisualStyleBackColor = True
        '
        'btnDodajNakladke
        '
        Me.btnDodajNakladke.Location = New System.Drawing.Point(368, 82)
        Me.btnDodajNakladke.Name = "btnDodajNakladke"
        Me.btnDodajNakladke.Size = New System.Drawing.Size(108, 23)
        Me.btnDodajNakladke.TabIndex = 4
        Me.btnDodajNakladke.Text = "dodaj warstwę..."
        Me.ToolTip1.SetToolTip(Me.btnDodajNakladke, "Wskaż folder innej sesji pobierania (z plikiem conf.txt), np. granice działek pobrane w PNG z przezroczystym tłem.")
        Me.btnDodajNakladke.UseVisualStyleBackColor = True
        '
        'lstNakladki
        '
        Me.lstNakladki.FormattingEnabled = True
        Me.lstNakladki.HorizontalScrollbar = True
        Me.lstNakladki.Location = New System.Drawing.Point(10, 82)
        Me.lstNakladki.Name = "lstNakladki"
        Me.lstNakladki.Size = New System.Drawing.Size(350, 56)
        Me.lstNakladki.TabIndex = 3
        '
        'lblNakladki
        '
        Me.lblNakladki.AutoSize = True
        Me.lblNakladki.Location = New System.Drawing.Point(8, 65)
        Me.lblNakladki.Name = "lblNakladki"
        Me.lblNakladki.Size = New System.Drawing.Size(330, 13)
        Me.lblNakladki.TabIndex = 2
        Me.lblNakladki.Text = "Warstwy z innych folderów nakładane na mapę (od dołu do góry):"
        '
        'chkBialePodstawa
        '
        Me.chkBialePodstawa.AutoSize = True
        Me.chkBialePodstawa.Location = New System.Drawing.Point(10, 42)
        Me.chkBialePodstawa.Name = "chkBialePodstawa"
        Me.chkBialePodstawa.Size = New System.Drawing.Size(260, 17)
        Me.chkBialePodstawa.TabIndex = 1
        Me.chkBialePodstawa.Text = "białe tło mapy z tego folderu przezroczyste"
        Me.ToolTip1.SetToolTip(Me.chkBialePodstawa, "Dla serwerów, które nie obsługują przezroczystego tła (TRANSPARENT=TRUE), i segmentów JPEG.")
        Me.chkBialePodstawa.UseVisualStyleBackColor = True
        '
        'chkNakladka
        '
        Me.chkNakladka.AutoSize = True
        Me.chkNakladka.Location = New System.Drawing.Point(10, 20)
        Me.chkNakladka.Name = "chkNakladka"
        Me.chkNakladka.Size = New System.Drawing.Size(460, 17)
        Me.chkNakladka.TabIndex = 0
        Me.chkNakladka.Text = "nakładka - przezroczysta warstwa wyświetlana nad mapą podkładową aplikacji (kafle PNG)"
        Me.ToolTip1.SetToolTip(Me.chkNakladka, "Np. granice działek, obszary chronione lub mapa historyczna nad mapą OpenStreetMap w Locus Map, OsmAnd czy OruxMaps.")
        Me.chkNakladka.UseVisualStyleBackColor = True
        '
        'grpRozmiar
        '
        Me.grpRozmiar.Controls.Add(Me.btnDopasuj)
        Me.grpRozmiar.Controls.Add(Me.nudLimit)
        Me.grpRozmiar.Controls.Add(Me.lblLimit)
        Me.grpRozmiar.Controls.Add(Me.lblRozmiar)
        Me.grpRozmiar.Controls.Add(Me.btnOszacuj)
        Me.grpRozmiar.Location = New System.Drawing.Point(12, 472)
        Me.grpRozmiar.Name = "grpRozmiar"
        Me.grpRozmiar.Size = New System.Drawing.Size(486, 52)
        Me.grpRozmiar.TabIndex = 7
        Me.grpRozmiar.TabStop = False
        Me.grpRozmiar.Text = "Rozmiar pliku"
        '
        'btnDopasuj
        '
        Me.btnDopasuj.Location = New System.Drawing.Point(398, 18)
        Me.btnDopasuj.Name = "btnDopasuj"
        Me.btnDopasuj.Size = New System.Drawing.Size(80, 23)
        Me.btnDopasuj.TabIndex = 4
        Me.btnDopasuj.Text = "dopasuj"
        Me.ToolTip1.SetToolTip(Me.btnDopasuj, "Wybiera najwyższy poziom powiększenia, przy którym szacowany rozmiar mapy mieści się w limicie.")
        Me.btnDopasuj.UseVisualStyleBackColor = True
        '
        'nudLimit
        '
        Me.nudLimit.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        Me.nudLimit.Location = New System.Drawing.Point(338, 20)
        Me.nudLimit.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        Me.nudLimit.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudLimit.Name = "nudLimit"
        Me.nudLimit.Size = New System.Drawing.Size(56, 20)
        Me.nudLimit.TabIndex = 3
        Me.nudLimit.Value = New Decimal(New Integer() {500, 0, 0, 0})
        '
        'lblLimit
        '
        Me.lblLimit.AutoSize = True
        Me.lblLimit.Location = New System.Drawing.Point(280, 23)
        Me.lblLimit.Name = "lblLimit"
        Me.lblLimit.Size = New System.Drawing.Size(56, 13)
        Me.lblLimit.TabIndex = 2
        Me.lblLimit.Text = "limit [MB]:"
        '
        'lblRozmiar
        '
        Me.lblRozmiar.Location = New System.Drawing.Point(112, 16)
        Me.lblRozmiar.Name = "lblRozmiar"
        Me.lblRozmiar.Size = New System.Drawing.Size(166, 30)
        Me.lblRozmiar.TabIndex = 1
        Me.lblRozmiar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnOszacuj
        '
        Me.btnOszacuj.Location = New System.Drawing.Point(8, 18)
        Me.btnOszacuj.Name = "btnOszacuj"
        Me.btnOszacuj.Size = New System.Drawing.Size(100, 23)
        Me.btnOszacuj.TabIndex = 0
        Me.btnOszacuj.Text = "oszacuj rozmiar"
        Me.ToolTip1.SetToolTip(Me.btnOszacuj, "Renderuje kilka próbnych kafli i na ich podstawie szacuje rozmiar pliku.")
        Me.btnOszacuj.UseVisualStyleBackColor = True
        '
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Location = New System.Drawing.Point(12, 534)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(70, 13)
        Me.lblJakosc.TabIndex = 8
        Me.lblJakosc.Text = "Jakość JPEG:"
        '
        'nudJakosc
        '
        Me.nudJakosc.Location = New System.Drawing.Point(90, 532)
        Me.nudJakosc.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudJakosc.Name = "nudJakosc"
        Me.nudJakosc.Size = New System.Drawing.Size(50, 20)
        Me.nudJakosc.TabIndex = 9
        Me.nudJakosc.Value = New Decimal(New Integer() {85, 0, 0, 0})
        '
        'lblPlan
        '
        Me.lblPlan.Location = New System.Drawing.Point(150, 530)
        Me.lblPlan.Name = "lblPlan"
        Me.lblPlan.Size = New System.Drawing.Size(348, 30)
        Me.lblPlan.TabIndex = 10
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.Location = New System.Drawing.Point(12, 564)
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.ReadOnly = True
        Me.txtKomunikaty.Size = New System.Drawing.Size(486, 60)
        Me.txtKomunikaty.TabIndex = 11
        Me.txtKomunikaty.Text = ""
        '
        'prgEksport
        '
        Me.prgEksport.Location = New System.Drawing.Point(12, 638)
        Me.prgEksport.Name = "prgEksport"
        Me.prgEksport.Size = New System.Drawing.Size(316, 23)
        Me.prgEksport.TabIndex = 12
        '
        'btnEksportuj
        '
        Me.btnEksportuj.Location = New System.Drawing.Point(336, 633)
        Me.btnEksportuj.Name = "btnEksportuj"
        Me.btnEksportuj.Size = New System.Drawing.Size(80, 32)
        Me.btnEksportuj.TabIndex = 13
        Me.btnEksportuj.Text = "Eksportuj"
        Me.btnEksportuj.UseVisualStyleBackColor = True
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(420, 633)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(78, 32)
        Me.btnPrzerwij.TabIndex = 14
        Me.btnPrzerwij.Text = "Przerwij"
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'FormEksport
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(510, 676)
        Me.Controls.Add(Me.btnPrzerwij)
        Me.Controls.Add(Me.btnEksportuj)
        Me.Controls.Add(Me.prgEksport)
        Me.Controls.Add(Me.txtKomunikaty)
        Me.Controls.Add(Me.lblPlan)
        Me.Controls.Add(Me.nudJakosc)
        Me.Controls.Add(Me.lblJakosc)
        Me.Controls.Add(Me.grpRozmiar)
        Me.Controls.Add(Me.grpWarstwy)
        Me.Controls.Add(Me.grpMBTiles)
        Me.Controls.Add(Me.grpFormat)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.btnFolder)
        Me.Controls.Add(Me.txtFolder)
        Me.Controls.Add(Me.lblFolder)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormEksport"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Eksport mapy do KMZ / MBTiles / SQLiteDB / OruxMaps"
        Me.grpFormat.ResumeLayout(False)
        Me.grpFormat.PerformLayout()
        CType(Me.nudMaksKafli, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMBTiles.ResumeLayout(False)
        Me.grpMBTiles.PerformLayout()
        CType(Me.nudPoziomMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudPoziomMin, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpWarstwy.ResumeLayout(False)
        Me.grpWarstwy.PerformLayout()
        CType(Me.nudKrycie, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpRozmiar.ResumeLayout(False)
        Me.grpRozmiar.PerformLayout()
        CType(Me.nudLimit, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblFolder As System.Windows.Forms.Label
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnFolder As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents grpFormat As System.Windows.Forms.GroupBox
    Friend WithEvents rbOruxMaps As System.Windows.Forms.RadioButton
    Friend WithEvents rbSQLiteDB As System.Windows.Forms.RadioButton
    Friend WithEvents nudMaksKafli As System.Windows.Forms.NumericUpDown
    Friend WithEvents rbMBTiles As System.Windows.Forms.RadioButton
    Friend WithEvents btnWielowarstwowyMbtiles As System.Windows.Forms.Button
    Friend WithEvents rbKmz As System.Windows.Forms.RadioButton
    Friend WithEvents rbKmzGarmin As System.Windows.Forms.RadioButton
    Friend WithEvents grpMBTiles As System.Windows.Forms.GroupBox
    Friend WithEvents chkPng As System.Windows.Forms.CheckBox
    Friend WithEvents nudPoziomMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPoziomDo As System.Windows.Forms.Label
    Friend WithEvents nudPoziomMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPoziomOd As System.Windows.Forms.Label
    Friend WithEvents grpWarstwy As System.Windows.Forms.GroupBox
    Friend WithEvents chkBialeNakladka As System.Windows.Forms.CheckBox
    Friend WithEvents nudKrycie As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblKrycie As System.Windows.Forms.Label
    Friend WithEvents btnUsunNakladke As System.Windows.Forms.Button
    Friend WithEvents btnDodajNakladke As System.Windows.Forms.Button
    Friend WithEvents lstNakladki As System.Windows.Forms.ListBox
    Friend WithEvents lblNakladki As System.Windows.Forms.Label
    Friend WithEvents chkBialePodstawa As System.Windows.Forms.CheckBox
    Friend WithEvents chkNakladka As System.Windows.Forms.CheckBox
    Friend WithEvents grpRozmiar As System.Windows.Forms.GroupBox
    Friend WithEvents btnDopasuj As System.Windows.Forms.Button
    Friend WithEvents nudLimit As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblLimit As System.Windows.Forms.Label
    Friend WithEvents lblRozmiar As System.Windows.Forms.Label
    Friend WithEvents btnOszacuj As System.Windows.Forms.Button
    Friend WithEvents lblJakosc As System.Windows.Forms.Label
    Friend WithEvents nudJakosc As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPlan As System.Windows.Forms.Label
    Friend WithEvents txtKomunikaty As System.Windows.Forms.RichTextBox
    Friend WithEvents prgEksport As System.Windows.Forms.ProgressBar
    Friend WithEvents btnEksportuj As System.Windows.Forms.Button
    Friend WithEvents btnPrzerwij As System.Windows.Forms.Button
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
