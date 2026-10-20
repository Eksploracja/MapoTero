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
        Me.grpSzczegoly.SuspendLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudRozmiarPiksela, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblAdres
        '
        Me.lblAdres.AutoSize = True
        Me.lblAdres.Location = New System.Drawing.Point(12, 9)
        Me.lblAdres.Name = "lblAdres"
        Me.lblAdres.Size = New System.Drawing.Size(268, 13)
        Me.lblAdres.TabIndex = 0
        Me.lblAdres.Text = "Adres usługi WMS lub WMTS (lub adres GetCapabilities):"
        '
        'txtAdres
        '
        Me.txtAdres.Location = New System.Drawing.Point(12, 26)
        Me.txtAdres.Name = "txtAdres"
        Me.txtAdres.Size = New System.Drawing.Size(538, 20)
        Me.txtAdres.TabIndex = 1
        '
        'btnPolacz
        '
        Me.btnPolacz.Location = New System.Drawing.Point(556, 24)
        Me.btnPolacz.Name = "btnPolacz"
        Me.btnPolacz.Size = New System.Drawing.Size(176, 24)
        Me.btnPolacz.TabIndex = 2
        Me.btnPolacz.Text = "Pobierz listę warstw"
        Me.btnPolacz.UseVisualStyleBackColor = True
        '
        'chkIgnorujSsl
        '
        Me.chkIgnorujSsl.AutoSize = True
        Me.chkIgnorujSsl.Location = New System.Drawing.Point(12, 52)
        Me.chkIgnorujSsl.Name = "chkIgnorujSsl"
        Me.chkIgnorujSsl.Size = New System.Drawing.Size(225, 17)
        Me.chkIgnorujSsl.TabIndex = 3
        Me.chkIgnorujSsl.Text = "Ignoruj błędy certyfikatów SSL (starsze serwery)"
        Me.ToolTip1.SetToolTip(Me.chkIgnorujSsl, "Włącz, jeśli serwer powiatowy lub gminny posiada nieaktualny lub samopodpisany certyfikat SSL.")
        Me.chkIgnorujSsl.UseVisualStyleBackColor = True
        '
        'prgPostep
        '
        Me.prgPostep.Location = New System.Drawing.Point(245, 52)
        Me.prgPostep.MarqueeAnimationSpeed = 30
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(305, 16)
        Me.prgPostep.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgPostep.TabIndex = 4
        Me.prgPostep.Visible = False
        '
        'lblStatus
        '
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 73)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(720, 32)
        Me.lblStatus.TabIndex = 5
        Me.lblStatus.Text = "Wklej adres serwera WMS / WMTS i kliknij ""Pobierz listę warstw""."
        '
        'lblFiltr
        '
        Me.lblFiltr.AutoSize = True
        Me.lblFiltr.Location = New System.Drawing.Point(12, 113)
        Me.lblFiltr.Name = "lblFiltr"
        Me.lblFiltr.Size = New System.Drawing.Size(74, 13)
        Me.lblFiltr.TabIndex = 6
        Me.lblFiltr.Text = "Filtruj warstwy:"
        '
        'txtFiltr
        '
        Me.txtFiltr.Location = New System.Drawing.Point(92, 110)
        Me.txtFiltr.Name = "txtFiltr"
        Me.txtFiltr.Size = New System.Drawing.Size(258, 20)
        Me.txtFiltr.TabIndex = 7
        Me.ToolTip1.SetToolTip(Me.txtFiltr, "Wpisz fragment nazwy lub tytułu, aby przefiltrować listę warstw.")
        '
        'tvWarstwy
        '
        Me.tvWarstwy.CheckBoxes = True
        Me.tvWarstwy.HideSelection = False
        Me.tvWarstwy.Location = New System.Drawing.Point(12, 136)
        Me.tvWarstwy.Name = "tvWarstwy"
        Me.tvWarstwy.Size = New System.Drawing.Size(338, 300)
        Me.tvWarstwy.TabIndex = 8
        '
        'btnZaznaczWszystkie
        '
        Me.btnZaznaczWszystkie.Location = New System.Drawing.Point(12, 442)
        Me.btnZaznaczWszystkie.Name = "btnZaznaczWszystkie"
        Me.btnZaznaczWszystkie.Size = New System.Drawing.Size(120, 23)
        Me.btnZaznaczWszystkie.TabIndex = 9
        Me.btnZaznaczWszystkie.Text = "Zaznacz wszystkie"
        Me.btnZaznaczWszystkie.UseVisualStyleBackColor = True
        '
        'btnOdznaczWszystkie
        '
        Me.btnOdznaczWszystkie.Location = New System.Drawing.Point(138, 442)
        Me.btnOdznaczWszystkie.Name = "btnOdznaczWszystkie"
        Me.btnOdznaczWszystkie.Size = New System.Drawing.Size(120, 23)
        Me.btnOdznaczWszystkie.TabIndex = 10
        Me.btnOdznaczWszystkie.Text = "Odznacz wszystkie"
        Me.btnOdznaczWszystkie.UseVisualStyleBackColor = True
        '
        'grpSzczegoly
        '
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
        Me.grpSzczegoly.Location = New System.Drawing.Point(360, 110)
        Me.grpSzczegoly.Name = "grpSzczegoly"
        Me.grpSzczegoly.Size = New System.Drawing.Size(372, 355)
        Me.grpSzczegoly.TabIndex = 11
        Me.grpSzczegoly.TabStop = False
        Me.grpSzczegoly.Text = "Szczegóły wybranej warstwy"
        '
        'btnPobierzProbke
        '
        Me.btnPobierzProbke.Location = New System.Drawing.Point(232, 318)
        Me.btnPobierzProbke.Name = "btnPobierzProbke"
        Me.btnPobierzProbke.Size = New System.Drawing.Size(130, 24)
        Me.btnPobierzProbke.TabIndex = 14
        Me.btnPobierzProbke.Text = "Sprawdź warstwę..."
        Me.ToolTip1.SetToolTip(Me.btnPobierzProbke, "Pobiera próbny kafelek z serwera, aby sprawdzić czy warstwa odpowiada i jak wygląda.")
        Me.btnPobierzProbke.UseVisualStyleBackColor = True
        '
        'picPodglad
        '
        Me.picPodglad.BackColor = System.Drawing.Color.White
        Me.picPodglad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodglad.Location = New System.Drawing.Point(100, 248)
        Me.picPodglad.Name = "picPodglad"
        Me.picPodglad.Size = New System.Drawing.Size(124, 94)
        Me.picPodglad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picPodglad.TabIndex = 13
        Me.picPodglad.TabStop = False
        '
        'lblPodgladOpis
        '
        Me.lblPodgladOpis.AutoSize = True
        Me.lblPodgladOpis.Location = New System.Drawing.Point(10, 248)
        Me.lblPodgladOpis.Name = "lblPodgladOpis"
        Me.lblPodgladOpis.Size = New System.Drawing.Size(84, 13)
        Me.lblPodgladOpis.TabIndex = 12
        Me.lblPodgladOpis.Text = "Podgląd próbki:"
        '
        'nudRozmiarPiksela
        '
        Me.nudRozmiarPiksela.DecimalPlaces = 2
        Me.nudRozmiarPiksela.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.nudRozmiarPiksela.Location = New System.Drawing.Point(135, 217)
        Me.nudRozmiarPiksela.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.nudRozmiarPiksela.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudRozmiarPiksela.Name = "nudRozmiarPiksela"
        Me.nudRozmiarPiksela.Size = New System.Drawing.Size(89, 20)
        Me.nudRozmiarPiksela.TabIndex = 11
        Me.nudRozmiarPiksela.Value = New Decimal(New Integer() {100, 0, 0, 131072})
        '
        'lblPikselOpis
        '
        Me.lblPikselOpis.AutoSize = True
        Me.lblPikselOpis.Location = New System.Drawing.Point(10, 219)
        Me.lblPikselOpis.Name = "lblPikselOpis"
        Me.lblPikselOpis.Size = New System.Drawing.Size(119, 13)
        Me.lblPikselOpis.TabIndex = 10
        Me.lblPikselOpis.Text = "Zalecany piksel [m/pix]:"
        '
        'lblSkalaWartosc
        '
        Me.lblSkalaWartosc.AutoEllipsis = True
        Me.lblSkalaWartosc.Location = New System.Drawing.Point(132, 195)
        Me.lblSkalaWartosc.Name = "lblSkalaWartosc"
        Me.lblSkalaWartosc.Size = New System.Drawing.Size(230, 17)
        Me.lblSkalaWartosc.TabIndex = 9
        Me.lblSkalaWartosc.Text = "brak limitu"
        '
        'lblSkalaOpis
        '
        Me.lblSkalaOpis.AutoSize = True
        Me.lblSkalaOpis.Location = New System.Drawing.Point(10, 195)
        Me.lblSkalaOpis.Name = "lblSkalaOpis"
        Me.lblSkalaOpis.Size = New System.Drawing.Size(107, 13)
        Me.lblSkalaOpis.TabIndex = 8
        Me.lblSkalaOpis.Text = "Skala min/max (NMT):"
        '
        'lblUkladyWartosc
        '
        Me.lblUkladyWartosc.AutoEllipsis = True
        Me.lblUkladyWartosc.Location = New System.Drawing.Point(132, 173)
        Me.lblUkladyWartosc.Name = "lblUkladyWartosc"
        Me.lblUkladyWartosc.Size = New System.Drawing.Size(230, 17)
        Me.lblUkladyWartosc.TabIndex = 7
        Me.lblUkladyWartosc.Text = "-"
        '
        'lblUkladyOpis
        '
        Me.lblUkladyOpis.AutoSize = True
        Me.lblUkladyOpis.Location = New System.Drawing.Point(10, 173)
        Me.lblUkladyOpis.Name = "lblUkladyOpis"
        Me.lblUkladyOpis.Size = New System.Drawing.Size(105, 13)
        Me.lblUkladyOpis.TabIndex = 6
        Me.lblUkladyOpis.Text = "Obsługiwane układy:"
        '
        'txtAbstrakt
        '
        Me.txtAbstrakt.BackColor = System.Drawing.SystemColors.Window
        Me.txtAbstrakt.Location = New System.Drawing.Point(13, 85)
        Me.txtAbstrakt.Multiline = True
        Me.txtAbstrakt.Name = "txtAbstrakt"
        Me.txtAbstrakt.ReadOnly = True
        Me.txtAbstrakt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAbstrakt.Size = New System.Drawing.Size(349, 80)
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
        Me.lblNazwaZbioru.AutoSize = True
        Me.lblNazwaZbioru.Location = New System.Drawing.Point(12, 477)
        Me.lblNazwaZbioru.Name = "lblNazwaZbioru"
        Me.lblNazwaZbioru.Size = New System.Drawing.Size(296, 13)
        Me.lblNazwaZbioru.TabIndex = 12
        Me.lblNazwaZbioru.Text = "Nazwa nowego zbioru map (nazwa pliku .txt w folderze warstwy):"
        '
        'txtNazwaZbioru
        '
        Me.txtNazwaZbioru.Location = New System.Drawing.Point(12, 495)
        Me.txtNazwaZbioru.Name = "txtNazwaZbioru"
        Me.txtNazwaZbioru.Size = New System.Drawing.Size(338, 20)
        Me.txtNazwaZbioru.TabIndex = 13
        '
        'lblLiczbaZaznaczonych
        '
        Me.lblLiczbaZaznaczonych.AutoSize = True
        Me.lblLiczbaZaznaczonych.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblLiczbaZaznaczonych.Location = New System.Drawing.Point(360, 498)
        Me.lblLiczbaZaznaczonych.Name = "lblLiczbaZaznaczonych"
        Me.lblLiczbaZaznaczonych.Size = New System.Drawing.Size(183, 13)
        Me.lblLiczbaZaznaczonych.TabIndex = 14
        Me.lblLiczbaZaznaczonych.Text = "Zaznaczono warstw do pobrania: 0"
        '
        'btnZapisz
        '
        Me.btnZapisz.Enabled = False
        Me.btnZapisz.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZapisz.Location = New System.Drawing.Point(452, 532)
        Me.btnZapisz.Name = "btnZapisz"
        Me.btnZapisz.Size = New System.Drawing.Size(180, 28)
        Me.btnZapisz.TabIndex = 15
        Me.btnZapisz.Text = "Dodaj zbiór do MapoTero"
        Me.btnZapisz.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(638, 532)
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
        Me.ClientSize = New System.Drawing.Size(744, 571)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnZapisz)
        Me.Controls.Add(Me.lblLiczbaZaznaczonych)
        Me.Controls.Add(Me.txtNazwaZbioru)
        Me.Controls.Add(Me.lblNazwaZbioru)
        Me.Controls.Add(Me.grpSzczegoly)
        Me.Controls.Add(Me.btnOdznaczWszystkie)
        Me.Controls.Add(Me.btnZaznaczWszystkie)
        Me.Controls.Add(Me.tvWarstwy)
        Me.Controls.Add(Me.txtFiltr)
        Me.Controls.Add(Me.lblFiltr)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.prgPostep)
        Me.Controls.Add(Me.chkIgnorujSsl)
        Me.Controls.Add(Me.btnPolacz)
        Me.Controls.Add(Me.txtAdres)
        Me.Controls.Add(Me.lblAdres)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormDodajSerwer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Kreator dodawania serwera mapowego (WMS / WMTS)"
        Me.grpSzczegoly.ResumeLayout(False)
        Me.grpSzczegoly.PerformLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudRozmiarPiksela, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

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
    Friend WithEvents btnPobierzProbke As System.Windows.Forms.Button
    Friend WithEvents lblNazwaZbioru As System.Windows.Forms.Label
    Friend WithEvents txtNazwaZbioru As System.Windows.Forms.TextBox
    Friend WithEvents lblLiczbaZaznaczonych As System.Windows.Forms.Label
    Friend WithEvents btnZapisz As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
