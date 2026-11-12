<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormDodajSerwer
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
        Me.pnlKatalog = New MapoTero.PanelKatalogSerwerow()
        Me.pnlKrokWarstwy = New System.Windows.Forms.Panel()
        Me.btnWrocDoKatalogu = New System.Windows.Forms.Button()
        Me.lblAdres = New System.Windows.Forms.Label()
        Me.txtAdres = New System.Windows.Forms.TextBox()
        Me.btnPolacz = New System.Windows.Forms.Button()
        Me.chkIgnorujSsl = New System.Windows.Forms.CheckBox()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblFiltr = New System.Windows.Forms.Label()
        Me.txtFiltr = New System.Windows.Forms.TextBox()
        Me.tvWarstwy = New System.Windows.Forms.TreeView()
        Me.btnZaznaczWszystkie = New System.Windows.Forms.Button()
        Me.btnOdznaczWszystkie = New System.Windows.Forms.Button()
        Me.grpSzczegoly = New System.Windows.Forms.GroupBox()
        Me.chkNakladka = New System.Windows.Forms.CheckBox()
        Me.chkTms = New System.Windows.Forms.CheckBox()
        Me.nudZMax = New System.Windows.Forms.NumericUpDown()
        Me.lblZoomDo = New System.Windows.Forms.Label()
        Me.nudZMin = New System.Windows.Forms.NumericUpDown()
        Me.lblZoomZakres = New System.Windows.Forms.Label()
        Me.btnPobierzProbke = New System.Windows.Forms.Button()
        Me.picPodglad = New System.Windows.Forms.PictureBox()
        Me.lblPodgladOpis = New System.Windows.Forms.Label()
        Me.nudRozmiarPiksela = New System.Windows.Forms.NumericUpDown()
        Me.lblPikselOpis = New System.Windows.Forms.Label()
        Me.lblSkalaWartosc = New System.Windows.Forms.Label()
        Me.lblSkalaOpis = New System.Windows.Forms.Label()
        Me.lblUkladyWartosc = New System.Windows.Forms.Label()
        Me.lblUkladyOpis = New System.Windows.Forms.Label()
        Me.txtAbstrakt = New System.Windows.Forms.TextBox()
        Me.lblAbstraktOpis = New System.Windows.Forms.Label()
        Me.lblTytulWartosc = New System.Windows.Forms.Label()
        Me.lblTytulOpis = New System.Windows.Forms.Label()
        Me.lblNazwaWartosc = New System.Windows.Forms.Label()
        Me.lblNazwaOpis = New System.Windows.Forms.Label()
        Me.lblNazwaZbioru = New System.Windows.Forms.Label()
        Me.txtNazwaZbioru = New System.Windows.Forms.TextBox()
        Me.lblLiczbaZaznaczonych = New System.Windows.Forms.Label()
        Me.btnZapisz = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.pnlKrokWarstwy.SuspendLayout()
        Me.grpSzczegoly.SuspendLayout()
        CType(Me.nudZMax, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudZMin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudRozmiarPiksela, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlKatalog
        '
        Me.pnlKatalog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKatalog.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.pnlKatalog.Location = New System.Drawing.Point(0, 0)
        Me.pnlKatalog.Name = "pnlKatalog"
        Me.pnlKatalog.Size = New System.Drawing.Size(860, 585)
        Me.pnlKatalog.TabIndex = 0
        '
        'pnlKrokWarstwy
        '
        Me.pnlKrokWarstwy.Controls.Add(Me.btnWrocDoKatalogu)
        Me.pnlKrokWarstwy.Controls.Add(Me.btnAnuluj)
        Me.pnlKrokWarstwy.Controls.Add(Me.btnZapisz)
        Me.pnlKrokWarstwy.Controls.Add(Me.lblLiczbaZaznaczonych)
        Me.pnlKrokWarstwy.Controls.Add(Me.txtNazwaZbioru)
        Me.pnlKrokWarstwy.Controls.Add(Me.lblNazwaZbioru)
        Me.pnlKrokWarstwy.Controls.Add(Me.grpSzczegoly)
        Me.pnlKrokWarstwy.Controls.Add(Me.btnOdznaczWszystkie)
        Me.pnlKrokWarstwy.Controls.Add(Me.btnZaznaczWszystkie)
        Me.pnlKrokWarstwy.Controls.Add(Me.tvWarstwy)
        Me.pnlKrokWarstwy.Controls.Add(Me.txtFiltr)
        Me.pnlKrokWarstwy.Controls.Add(Me.lblFiltr)
        Me.pnlKrokWarstwy.Controls.Add(Me.lblStatus)
        Me.pnlKrokWarstwy.Controls.Add(Me.prgPostep)
        Me.pnlKrokWarstwy.Controls.Add(Me.chkIgnorujSsl)
        Me.pnlKrokWarstwy.Controls.Add(Me.btnPolacz)
        Me.pnlKrokWarstwy.Controls.Add(Me.txtAdres)
        Me.pnlKrokWarstwy.Controls.Add(Me.lblAdres)
        Me.pnlKrokWarstwy.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlKrokWarstwy.Location = New System.Drawing.Point(0, 0)
        Me.pnlKrokWarstwy.Name = "pnlKrokWarstwy"
        Me.pnlKrokWarstwy.Size = New System.Drawing.Size(860, 585)
        Me.pnlKrokWarstwy.TabIndex = 1
        Me.pnlKrokWarstwy.Visible = False
        '
        'lblAdres
        '
        Me.lblAdres.AutoSize = True
        Me.lblAdres.Location = New System.Drawing.Point(12, 10)
        Me.lblAdres.Name = "lblAdres"
        Me.lblAdres.Size = New System.Drawing.Size(340, 13)
        Me.lblAdres.TabIndex = 0
        Me.lblAdres.Text = "Adres WMS / WMTS, szablon XYZ/TMS ({z}/{x}/{y}) lub TileJSON:"
        '
        'txtAdres
        '
        Me.txtAdres.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAdres.Location = New System.Drawing.Point(12, 30)
        Me.txtAdres.Name = "txtAdres"
        Me.txtAdres.Size = New System.Drawing.Size(654, 20)
        Me.txtAdres.TabIndex = 1
        '
        'btnPolacz
        '
        Me.btnPolacz.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPolacz.Location = New System.Drawing.Point(672, 28)
        Me.btnPolacz.Name = "btnPolacz"
        Me.btnPolacz.Size = New System.Drawing.Size(176, 24)
        Me.btnPolacz.TabIndex = 2
        Me.btnPolacz.Text = "Pobierz listę warstw"
        Me.btnPolacz.UseVisualStyleBackColor = True
        '
        'chkIgnorujSsl
        '
        Me.chkIgnorujSsl.AutoSize = True
        Me.chkIgnorujSsl.Location = New System.Drawing.Point(12, 57)
        Me.chkIgnorujSsl.Name = "chkIgnorujSsl"
        Me.chkIgnorujSsl.Size = New System.Drawing.Size(225, 17)
        Me.chkIgnorujSsl.TabIndex = 3
        Me.chkIgnorujSsl.Text = "Ignoruj błędy certyfikatów SSL (starsze serwery)"
        Me.ToolTip1.SetToolTip(Me.chkIgnorujSsl, "Włącz, jeśli serwer powiatowy lub gminny posiada nieaktualny lub samopodpisany certyfikat SSL.")
        Me.chkIgnorujSsl.UseVisualStyleBackColor = True
        '
        'prgPostep
        '
        Me.prgPostep.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.prgPostep.Location = New System.Drawing.Point(250, 57)
        Me.prgPostep.MarqueeAnimationSpeed = 30
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(416, 16)
        Me.prgPostep.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgPostep.TabIndex = 4
        Me.prgPostep.Visible = False
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 78)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(836, 30)
        Me.lblStatus.TabIndex = 5
        Me.lblStatus.Text = "Wpisz lub wklej adres serwera WMS / WMTS / XYZ i kliknij ""Pobierz listę warstw""."
        '
        'lblFiltr
        '
        Me.lblFiltr.AutoSize = True
        Me.lblFiltr.Location = New System.Drawing.Point(12, 114)
        Me.lblFiltr.Name = "lblFiltr"
        Me.lblFiltr.Size = New System.Drawing.Size(74, 13)
        Me.lblFiltr.TabIndex = 6
        Me.lblFiltr.Text = "Filtruj warstwy:"
        '
        'txtFiltr
        '
        Me.txtFiltr.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFiltr.Location = New System.Drawing.Point(92, 111)
        Me.txtFiltr.Name = "txtFiltr"
        Me.txtFiltr.Size = New System.Drawing.Size(374, 20)
        Me.txtFiltr.TabIndex = 7
        Me.ToolTip1.SetToolTip(Me.txtFiltr, "Wpisz fragment nazwy lub tytułu, aby przefiltrować listę warstw.")
        '
        'tvWarstwy
        '
        Me.tvWarstwy.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tvWarstwy.CheckBoxes = True
        Me.tvWarstwy.HideSelection = False
        Me.tvWarstwy.Location = New System.Drawing.Point(12, 137)
        Me.tvWarstwy.Name = "tvWarstwy"
        Me.tvWarstwy.Size = New System.Drawing.Size(454, 305)
        Me.tvWarstwy.TabIndex = 8
        '
        'btnZaznaczWszystkie
        '
        Me.btnZaznaczWszystkie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnZaznaczWszystkie.Location = New System.Drawing.Point(12, 448)
        Me.btnZaznaczWszystkie.Name = "btnZaznaczWszystkie"
        Me.btnZaznaczWszystkie.Size = New System.Drawing.Size(120, 23)
        Me.btnZaznaczWszystkie.TabIndex = 9
        Me.btnZaznaczWszystkie.Text = "Zaznacz wszystkie"
        Me.btnZaznaczWszystkie.UseVisualStyleBackColor = True
        '
        'btnOdznaczWszystkie
        '
        Me.btnOdznaczWszystkie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnOdznaczWszystkie.Location = New System.Drawing.Point(138, 448)
        Me.btnOdznaczWszystkie.Name = "btnOdznaczWszystkie"
        Me.btnOdznaczWszystkie.Size = New System.Drawing.Size(120, 23)
        Me.btnOdznaczWszystkie.TabIndex = 10
        Me.btnOdznaczWszystkie.Text = "Odznacz wszystkie"
        Me.btnOdznaczWszystkie.UseVisualStyleBackColor = True
        '
        'grpSzczegoly
        '
        Me.grpSzczegoly.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpSzczegoly.Controls.Add(Me.chkNakladka)
        Me.grpSzczegoly.Controls.Add(Me.chkTms)
        Me.grpSzczegoly.Controls.Add(Me.nudZMax)
        Me.grpSzczegoly.Controls.Add(Me.lblZoomDo)
        Me.grpSzczegoly.Controls.Add(Me.nudZMin)
        Me.grpSzczegoly.Controls.Add(Me.lblZoomZakres)
        Me.grpSzczegoly.Controls.Add(Me.btnPobierzProbke)
        Me.grpSzczegoly.Controls.Add(Me.picPodglad)
        Me.grpSzczegoly.Controls.Add(Me.lblPodgladOpis)
        Me.grpSzczegoly.Controls.Add(Me.nudRozmiarPiksela)
        Me.grpSzczegoly.Controls.Add(Me.lblPikselOpis)
        Me.grpSzczegoly.Controls.Add(Me.lblSkalaWartosc)
        Me.grpSzczegoly.Controls.Add(Me.lblSkalaOpis)
        Me.grpSzczegoly.Controls.Add(Me.lblUkladyWartosc)
        Me.grpSzczegoly.Controls.Add(Me.lblUkladyOpis)
        Me.grpSzczegoly.Controls.Add(Me.txtAbstrakt)
        Me.grpSzczegoly.Controls.Add(Me.lblAbstraktOpis)
        Me.grpSzczegoly.Controls.Add(Me.lblTytulWartosc)
        Me.grpSzczegoly.Controls.Add(Me.lblTytulOpis)
        Me.grpSzczegoly.Controls.Add(Me.lblNazwaWartosc)
        Me.grpSzczegoly.Controls.Add(Me.lblNazwaOpis)
        Me.grpSzczegoly.Location = New System.Drawing.Point(476, 110)
        Me.grpSzczegoly.Name = "grpSzczegoly"
        Me.grpSzczegoly.Size = New System.Drawing.Size(372, 361)
        Me.grpSzczegoly.TabIndex = 11
        Me.grpSzczegoly.TabStop = False
        Me.grpSzczegoly.Text = "Szczegóły wybranej warstwy"
        '
        'lblZoomZakres
        '
        Me.lblZoomZakres.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblZoomZakres.AutoSize = True
        Me.lblZoomZakres.Location = New System.Drawing.Point(230, 256)
        Me.lblZoomZakres.Name = "lblZoomZakres"
        Me.lblZoomZakres.Size = New System.Drawing.Size(37, 13)
        Me.lblZoomZakres.TabIndex = 15
        Me.lblZoomZakres.Text = "Zoom:"
        Me.lblZoomZakres.Visible = False
        '
        'nudZMin
        '
        Me.nudZMin.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.nudZMin.Location = New System.Drawing.Point(270, 253)
        Me.nudZMin.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudZMin.Name = "nudZMin"
        Me.nudZMin.Size = New System.Drawing.Size(40, 20)
        Me.nudZMin.TabIndex = 16
        Me.ToolTip1.SetToolTip(Me.nudZMin, "Minimalny poziom powiększeń (zMin) udostępniany przez serwer.")
        Me.nudZMin.Visible = False
        '
        'lblZoomDo
        '
        Me.lblZoomDo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblZoomDo.AutoSize = True
        Me.lblZoomDo.Location = New System.Drawing.Point(312, 256)
        Me.lblZoomDo.Name = "lblZoomDo"
        Me.lblZoomDo.Size = New System.Drawing.Size(10, 13)
        Me.lblZoomDo.TabIndex = 17
        Me.lblZoomDo.Text = "-"
        Me.lblZoomDo.Visible = False
        '
        'nudZMax
        '
        Me.nudZMax.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.nudZMax.Location = New System.Drawing.Point(324, 253)
        Me.nudZMax.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudZMax.Name = "nudZMax"
        Me.nudZMax.Size = New System.Drawing.Size(40, 20)
        Me.nudZMax.TabIndex = 18
        Me.ToolTip1.SetToolTip(Me.nudZMax, "Maksymalny poziom powiększeń (zMax) udostępniany przez serwer.")
        Me.nudZMax.Value = New Decimal(New Integer() {18, 0, 0, 0})
        Me.nudZMax.Visible = False
        '
        'chkTms
        '
        Me.chkTms.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkTms.AutoSize = True
        Me.chkTms.Location = New System.Drawing.Point(232, 279)
        Me.chkTms.Name = "chkTms"
        Me.chkTms.Size = New System.Drawing.Size(96, 17)
        Me.chkTms.TabIndex = 19
        Me.chkTms.Text = "Oś Y TMS (-y)"
        Me.ToolTip1.SetToolTip(Me.chkTms, "Odwrócona oś pionowa Y standardu OSGeo TMS (liczba wierszy od południa).")
        Me.chkTms.UseVisualStyleBackColor = True
        Me.chkTms.Visible = False
        '
        'chkNakladka
        '
        Me.chkNakladka.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkNakladka.AutoSize = True
        Me.chkNakladka.Location = New System.Drawing.Point(232, 300)
        Me.chkNakladka.Name = "chkNakladka"
        Me.chkNakladka.Size = New System.Drawing.Size(113, 17)
        Me.chkNakladka.TabIndex = 20
        Me.chkNakladka.Text = "Nakładka (szlaki)"
        Me.ToolTip1.SetToolTip(Me.chkNakladka, "Warstwa przezroczysta bez pełnego pokrycia (np. szlaki turystyczne): odpowiedź HTTP 404 traktowana jest jako pusty kafel.")
        Me.chkNakladka.UseVisualStyleBackColor = True
        Me.chkNakladka.Visible = False
        '
        'btnPobierzProbke
        '
        Me.btnPobierzProbke.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnPobierzProbke.Location = New System.Drawing.Point(232, 324)
        Me.btnPobierzProbke.Name = "btnPobierzProbke"
        Me.btnPobierzProbke.Size = New System.Drawing.Size(130, 24)
        Me.btnPobierzProbke.TabIndex = 14
        Me.btnPobierzProbke.Text = "Sprawdź warstwę..."
        Me.ToolTip1.SetToolTip(Me.btnPobierzProbke, "Pobiera próbny kafelek z serwera, aby sprawdzić czy warstwa odpowiada i jak wygląda.")
        Me.btnPobierzProbke.UseVisualStyleBackColor = True
        '
        'picPodglad
        '
        Me.picPodglad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.picPodglad.BackColor = System.Drawing.Color.White
        Me.picPodglad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodglad.Location = New System.Drawing.Point(100, 254)
        Me.picPodglad.Name = "picPodglad"
        Me.picPodglad.Size = New System.Drawing.Size(124, 94)
        Me.picPodglad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picPodglad.TabIndex = 13
        Me.picPodglad.TabStop = False
        '
        'lblPodgladOpis
        '
        Me.lblPodgladOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblPodgladOpis.AutoSize = True
        Me.lblPodgladOpis.Location = New System.Drawing.Point(10, 254)
        Me.lblPodgladOpis.Name = "lblPodgladOpis"
        Me.lblPodgladOpis.Size = New System.Drawing.Size(84, 13)
        Me.lblPodgladOpis.TabIndex = 12
        Me.lblPodgladOpis.Text = "Podgląd próbki:"
        '
        'nudRozmiarPiksela
        '
        Me.nudRozmiarPiksela.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.nudRozmiarPiksela.DecimalPlaces = 2
        Me.nudRozmiarPiksela.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudRozmiarPiksela.Location = New System.Drawing.Point(135, 223)
        Me.nudRozmiarPiksela.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.nudRozmiarPiksela.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudRozmiarPiksela.Name = "nudRozmiarPiksela"
        Me.nudRozmiarPiksela.Size = New System.Drawing.Size(89, 20)
        Me.nudRozmiarPiksela.TabIndex = 11
        Me.nudRozmiarPiksela.Value = New Decimal(New Integer() {100, 0, 0, 131072})
        '
        'lblPikselOpis
        '
        Me.lblPikselOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblPikselOpis.AutoSize = True
        Me.lblPikselOpis.Location = New System.Drawing.Point(10, 225)
        Me.lblPikselOpis.Name = "lblPikselOpis"
        Me.lblPikselOpis.Size = New System.Drawing.Size(119, 13)
        Me.lblPikselOpis.TabIndex = 10
        Me.lblPikselOpis.Text = "Zalecany piksel [m/pix]:"
        '
        'lblSkalaWartosc
        '
        Me.lblSkalaWartosc.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblSkalaWartosc.AutoEllipsis = True
        Me.lblSkalaWartosc.Location = New System.Drawing.Point(132, 201)
        Me.lblSkalaWartosc.Name = "lblSkalaWartosc"
        Me.lblSkalaWartosc.Size = New System.Drawing.Size(230, 17)
        Me.lblSkalaWartosc.TabIndex = 9
        Me.lblSkalaWartosc.Text = "brak limitu"
        '
        'lblSkalaOpis
        '
        Me.lblSkalaOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblSkalaOpis.AutoSize = True
        Me.lblSkalaOpis.Location = New System.Drawing.Point(10, 201)
        Me.lblSkalaOpis.Name = "lblSkalaOpis"
        Me.lblSkalaOpis.Size = New System.Drawing.Size(107, 13)
        Me.lblSkalaOpis.TabIndex = 8
        Me.lblSkalaOpis.Text = "Skala min/max (NMT):"
        '
        'lblUkladyWartosc
        '
        Me.lblUkladyWartosc.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblUkladyWartosc.AutoEllipsis = True
        Me.lblUkladyWartosc.Location = New System.Drawing.Point(132, 179)
        Me.lblUkladyWartosc.Name = "lblUkladyWartosc"
        Me.lblUkladyWartosc.Size = New System.Drawing.Size(230, 17)
        Me.lblUkladyWartosc.TabIndex = 7
        Me.lblUkladyWartosc.Text = "-"
        '
        'lblUkladyOpis
        '
        Me.lblUkladyOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblUkladyOpis.AutoSize = True
        Me.lblUkladyOpis.Location = New System.Drawing.Point(10, 179)
        Me.lblUkladyOpis.Name = "lblUkladyOpis"
        Me.lblUkladyOpis.Size = New System.Drawing.Size(105, 13)
        Me.lblUkladyOpis.TabIndex = 6
        Me.lblUkladyOpis.Text = "Obsługiwane układy:"
        '
        'txtAbstrakt
        '
        Me.txtAbstrakt.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAbstrakt.BackColor = System.Drawing.SystemColors.Window
        Me.txtAbstrakt.Location = New System.Drawing.Point(13, 85)
        Me.txtAbstrakt.Multiline = True
        Me.txtAbstrakt.Name = "txtAbstrakt"
        Me.txtAbstrakt.ReadOnly = True
        Me.txtAbstrakt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAbstrakt.Size = New System.Drawing.Size(349, 86)
        Me.txtAbstrakt.TabIndex = 5
        '
        'lblAbstraktOpis
        '
        Me.lblAbstraktOpis.AutoSize = True
        Me.lblAbstraktOpis.Location = New System.Drawing.Point(10, 68)
        Me.lblAbstraktOpis.Name = "lblAbstraktOpis"
        Me.lblAbstraktOpis.Size = New System.Drawing.Size(69, 13)
        Me.lblAbstraktOpis.TabIndex = 4
        Me.lblAbstraktOpis.Text = "Opis warstwy:"
        '
        'lblTytulWartosc
        '
        Me.lblTytulWartosc.AutoEllipsis = True
        Me.lblTytulWartosc.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTytulWartosc.Location = New System.Drawing.Point(110, 44)
        Me.lblTytulWartosc.Name = "lblTytulWartosc"
        Me.lblTytulWartosc.Size = New System.Drawing.Size(252, 18)
        Me.lblTytulWartosc.TabIndex = 3
        Me.lblTytulWartosc.Text = "-"
        '
        'lblTytulOpis
        '
        Me.lblTytulOpis.AutoSize = True
        Me.lblTytulOpis.Location = New System.Drawing.Point(10, 44)
        Me.lblTytulOpis.Name = "lblTytulOpis"
        Me.lblTytulOpis.Size = New System.Drawing.Size(35, 13)
        Me.lblTytulOpis.TabIndex = 2
        Me.lblTytulOpis.Text = "Tytuł:"
        '
        'lblNazwaWartosc
        '
        Me.lblNazwaWartosc.AutoEllipsis = True
        Me.lblNazwaWartosc.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblNazwaWartosc.Location = New System.Drawing.Point(110, 22)
        Me.lblNazwaWartosc.Name = "lblNazwaWartosc"
        Me.lblNazwaWartosc.Size = New System.Drawing.Size(252, 18)
        Me.lblNazwaWartosc.TabIndex = 1
        Me.lblNazwaWartosc.Text = "-"
        '
        'lblNazwaOpis
        '
        Me.lblNazwaOpis.AutoSize = True
        Me.lblNazwaOpis.Location = New System.Drawing.Point(10, 22)
        Me.lblNazwaOpis.Name = "lblNazwaOpis"
        Me.lblNazwaOpis.Size = New System.Drawing.Size(96, 13)
        Me.lblNazwaOpis.TabIndex = 0
        Me.lblNazwaOpis.Text = "Nazwa techniczna:"
        '
        'lblNazwaZbioru
        '
        Me.lblNazwaZbioru.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblNazwaZbioru.AutoSize = True
        Me.lblNazwaZbioru.Location = New System.Drawing.Point(12, 480)
        Me.lblNazwaZbioru.Name = "lblNazwaZbioru"
        Me.lblNazwaZbioru.Size = New System.Drawing.Size(296, 13)
        Me.lblNazwaZbioru.TabIndex = 12
        Me.lblNazwaZbioru.Text = "Nazwa nowego zbioru map (nazwa pliku .txt w folderze warstwy):"
        '
        'txtNazwaZbioru
        '
        Me.txtNazwaZbioru.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtNazwaZbioru.Location = New System.Drawing.Point(12, 498)
        Me.txtNazwaZbioru.Name = "txtNazwaZbioru"
        Me.txtNazwaZbioru.Size = New System.Drawing.Size(454, 20)
        Me.txtNazwaZbioru.TabIndex = 13
        '
        'lblLiczbaZaznaczonych
        '
        Me.lblLiczbaZaznaczonych.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblLiczbaZaznaczonych.AutoSize = True
        Me.lblLiczbaZaznaczonych.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblLiczbaZaznaczonych.Location = New System.Drawing.Point(476, 501)
        Me.lblLiczbaZaznaczonych.Name = "lblLiczbaZaznaczonych"
        Me.lblLiczbaZaznaczonych.Size = New System.Drawing.Size(183, 13)
        Me.lblLiczbaZaznaczonych.TabIndex = 14
        Me.lblLiczbaZaznaczonych.Text = "Zaznaczono warstw do pobrania: 0"
        '
        'btnWrocDoKatalogu
        '
        Me.btnWrocDoKatalogu.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnWrocDoKatalogu.Location = New System.Drawing.Point(12, 544)
        Me.btnWrocDoKatalogu.Name = "btnWrocDoKatalogu"
        Me.btnWrocDoKatalogu.Size = New System.Drawing.Size(188, 28)
        Me.btnWrocDoKatalogu.TabIndex = 17
        Me.btnWrocDoKatalogu.Text = "← Wróć do listy usług"
        Me.ToolTip1.SetToolTip(Me.btnWrocDoKatalogu, "Wraca do katalogu predefiniowanych usług oraz rejestrów Geoportal i EZiUDP.")
        Me.btnWrocDoKatalogu.UseVisualStyleBackColor = True
        '
        'btnZapisz
        '
        Me.btnZapisz.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZapisz.Enabled = False
        Me.btnZapisz.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZapisz.Location = New System.Drawing.Point(568, 544)
        Me.btnZapisz.Name = "btnZapisz"
        Me.btnZapisz.Size = New System.Drawing.Size(180, 28)
        Me.btnZapisz.TabIndex = 15
        Me.btnZapisz.Text = "Dodaj zbiór do MapoTero"
        Me.btnZapisz.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(754, 544)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(94, 28)
        Me.btnAnuluj.TabIndex = 16
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'FormDodajSerwer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(860, 585)
        Me.Controls.Add(Me.pnlKatalog)
        Me.Controls.Add(Me.pnlKrokWarstwy)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.MinimumSize = New System.Drawing.Size(780, 590)
        Me.MinimizeBox = False
        Me.Name = "FormDodajSerwer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Kreator dodawania serwera mapowego (WMS / WMTS / XYZ / TMS)"
        Me.pnlKrokWarstwy.ResumeLayout(False)
        Me.pnlKrokWarstwy.PerformLayout()
        Me.grpSzczegoly.ResumeLayout(False)
        Me.grpSzczegoly.PerformLayout()
        CType(Me.nudZMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudZMin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudRozmiarPiksela, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlKatalog As MapoTero.PanelKatalogSerwerow
    Friend WithEvents pnlKrokWarstwy As System.Windows.Forms.Panel
    Friend WithEvents btnWrocDoKatalogu As System.Windows.Forms.Button
    Friend WithEvents lblAdres As System.Windows.Forms.Label
    Friend WithEvents txtAdres As System.Windows.Forms.TextBox
    Friend WithEvents btnPolacz As System.Windows.Forms.Button
    Friend WithEvents chkIgnorujSsl As System.Windows.Forms.CheckBox
    Friend WithEvents prgPostep As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblFiltr As System.Windows.Forms.Label
    Friend WithEvents txtFiltr As System.Windows.Forms.TextBox
    Friend WithEvents tvWarstwy As System.Windows.Forms.TreeView
    Friend WithEvents btnZaznaczWszystkie As System.Windows.Forms.Button
    Friend WithEvents btnOdznaczWszystkie As System.Windows.Forms.Button
    Friend WithEvents grpSzczegoly As System.Windows.Forms.GroupBox
    Friend WithEvents lblNazwaOpis As System.Windows.Forms.Label
    Friend WithEvents lblNazwaWartosc As System.Windows.Forms.Label
    Friend WithEvents lblTytulOpis As System.Windows.Forms.Label
    Friend WithEvents lblTytulWartosc As System.Windows.Forms.Label
    Friend WithEvents lblAbstraktOpis As System.Windows.Forms.Label
    Friend WithEvents txtAbstrakt As System.Windows.Forms.TextBox
    Friend WithEvents lblUkladyOpis As System.Windows.Forms.Label
    Friend WithEvents lblUkladyWartosc As System.Windows.Forms.Label
    Friend WithEvents lblSkalaOpis As System.Windows.Forms.Label
    Friend WithEvents lblSkalaWartosc As System.Windows.Forms.Label
    Friend WithEvents lblPikselOpis As System.Windows.Forms.Label
    Friend WithEvents nudRozmiarPiksela As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPodgladOpis As System.Windows.Forms.Label
    Friend WithEvents picPodglad As System.Windows.Forms.PictureBox
    Friend WithEvents lblZoomZakres As System.Windows.Forms.Label
    Friend WithEvents nudZMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblZoomDo As System.Windows.Forms.Label
    Friend WithEvents nudZMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkTms As System.Windows.Forms.CheckBox
    Friend WithEvents chkNakladka As System.Windows.Forms.CheckBox
    Friend WithEvents btnPobierzProbke As System.Windows.Forms.Button
    Friend WithEvents lblNazwaZbioru As System.Windows.Forms.Label
    Friend WithEvents txtNazwaZbioru As System.Windows.Forms.TextBox
    Friend WithEvents lblLiczbaZaznaczonych As System.Windows.Forms.Label
    Friend WithEvents btnZapisz As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
