<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PanelKatalogSerwerow
    Inherits System.Windows.Forms.UserControl

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
        Me.pnlGora = New System.Windows.Forms.Panel()
        Me.chkUkryjSkorowidze = New System.Windows.Forms.CheckBox()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.chkUkryjDodane = New System.Windows.Forms.CheckBox()
        Me.cmbKategoria = New System.Windows.Forms.ComboBox()
        Me.lblKategoria = New System.Windows.Forms.Label()
        Me.btnSzukajLokalne = New System.Windows.Forms.Button()
        Me.txtSzukaj = New System.Windows.Forms.TextBox()
        Me.lblSzukaj = New System.Windows.Forms.Label()
        Me.btnOdswiez = New System.Windows.Forms.Button()
        Me.cmbZakresLokalny = New System.Windows.Forms.ComboBox()
        Me.cmbZrodlo = New System.Windows.Forms.ComboBox()
        Me.lblZrodlo = New System.Windows.Forms.Label()
        Me.pnlDol = New System.Windows.Forms.Panel()
        Me.btnKopiujUrl = New System.Windows.Forms.Button()
        Me.btnWlasnyAdres = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.btnWybierz = New System.Windows.Forms.Button()
        Me.txtOpisPodglad = New System.Windows.Forms.TextBox()
        Me.lblOpisPodglad = New System.Windows.Forms.Label()
        Me.txtAdresPodglad = New System.Windows.Forms.TextBox()
        Me.lblAdresPodglad = New System.Windows.Forms.Label()
        Me.pnlLista = New System.Windows.Forms.Panel()
        Me.lvUslugi = New System.Windows.Forms.ListView()
        Me.colNazwa = New System.Windows.Forms.ColumnHeader()
        Me.colKategoria = New System.Windows.Forms.ColumnHeader()
        Me.colDostawca = New System.Windows.Forms.ColumnHeader()
        Me.colTyp = New System.Windows.Forms.ColumnHeader()
        Me.colStan = New System.Windows.Forms.ColumnHeader()
        Me.ctxMenuUslugi = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuDalej = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuKopiujUrl = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuKopiujNazwe = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuOtworzPrzegladarka = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.pnlGora.SuspendLayout()
        Me.pnlDol.SuspendLayout()
        Me.pnlLista.SuspendLayout()
        Me.ctxMenuUslugi.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlGora
        '
        Me.pnlGora.Controls.Add(Me.chkUkryjSkorowidze)
        Me.pnlGora.Controls.Add(Me.prgPostep)
        Me.pnlGora.Controls.Add(Me.lblStatus)
        Me.pnlGora.Controls.Add(Me.chkUkryjDodane)
        Me.pnlGora.Controls.Add(Me.cmbKategoria)
        Me.pnlGora.Controls.Add(Me.lblKategoria)
        Me.pnlGora.Controls.Add(Me.btnSzukajLokalne)
        Me.pnlGora.Controls.Add(Me.txtSzukaj)
        Me.pnlGora.Controls.Add(Me.lblSzukaj)
        Me.pnlGora.Controls.Add(Me.btnOdswiez)
        Me.pnlGora.Controls.Add(Me.cmbZakresLokalny)
        Me.pnlGora.Controls.Add(Me.cmbZrodlo)
        Me.pnlGora.Controls.Add(Me.lblZrodlo)
        Me.pnlGora.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlGora.Location = New System.Drawing.Point(0, 0)
        Me.pnlGora.Name = "pnlGora"
        Me.pnlGora.Size = New System.Drawing.Size(860, 100)
        Me.pnlGora.TabIndex = 0
        '
        'lblZrodlo
        '
        Me.lblZrodlo.AutoSize = True
        Me.lblZrodlo.Location = New System.Drawing.Point(12, 13)
        Me.lblZrodlo.Name = "lblZrodlo"
        Me.lblZrodlo.Size = New System.Drawing.Size(80, 13)
        Me.lblZrodlo.TabIndex = 0
        Me.lblZrodlo.Text = "Źródło katalogu:"
        '
        'cmbZrodlo
        '
        Me.cmbZrodlo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbZrodlo.FormattingEnabled = True
        Me.cmbZrodlo.Location = New System.Drawing.Point(98, 10)
        Me.cmbZrodlo.Name = "cmbZrodlo"
        Me.cmbZrodlo.Size = New System.Drawing.Size(324, 21)
        Me.cmbZrodlo.TabIndex = 1
        '
        'cmbZakresLokalny
        '
        Me.cmbZakresLokalny.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbZakresLokalny.FormattingEnabled = True
        Me.cmbZakresLokalny.Location = New System.Drawing.Point(428, 10)
        Me.cmbZakresLokalny.Name = "cmbZakresLokalny"
        Me.cmbZakresLokalny.Size = New System.Drawing.Size(140, 21)
        Me.cmbZakresLokalny.TabIndex = 2
        Me.cmbZakresLokalny.Visible = False
        '
        'btnOdswiez
        '
        Me.btnOdswiez.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOdswiez.Location = New System.Drawing.Point(718, 9)
        Me.btnOdswiez.Name = "btnOdswiez"
        Me.btnOdswiez.Size = New System.Drawing.Size(130, 23)
        Me.btnOdswiez.TabIndex = 3
        Me.btnOdswiez.Text = "Odśwież z internetu"
        Me.ToolTip1.SetToolTip(Me.btnOdswiez, "Pobiera najnowszą listę z internetu z pominięciem lokalnej pamięci podręcznej.")
        Me.btnOdswiez.UseVisualStyleBackColor = True
        Me.btnOdswiez.Visible = False
        '
        'lblSzukaj
        '
        Me.lblSzukaj.AutoSize = True
        Me.lblSzukaj.Location = New System.Drawing.Point(12, 41)
        Me.lblSzukaj.Name = "lblSzukaj"
        Me.lblSzukaj.Size = New System.Drawing.Size(42, 13)
        Me.lblSzukaj.TabIndex = 4
        Me.lblSzukaj.Text = "Szukaj:"
        '
        'txtSzukaj
        '
        Me.txtSzukaj.Location = New System.Drawing.Point(98, 38)
        Me.txtSzukaj.Name = "txtSzukaj"
        Me.txtSzukaj.Size = New System.Drawing.Size(246, 20)
        Me.txtSzukaj.TabIndex = 5
        Me.ToolTip1.SetToolTip(Me.txtSzukaj, "Wpisz fragment nazwy, instytucji, tematu lub kodu TERYT albo wklej własny adres http(s):// serwera.")
        '
        'btnSzukajLokalne
        '
        Me.btnSzukajLokalne.Location = New System.Drawing.Point(350, 36)
        Me.btnSzukajLokalne.Name = "btnSzukajLokalne"
        Me.btnSzukajLokalne.Size = New System.Drawing.Size(72, 23)
        Me.btnSzukajLokalne.TabIndex = 6
        Me.btnSzukajLokalne.Text = "Szukaj"
        Me.btnSzukajLokalne.UseVisualStyleBackColor = True
        Me.btnSzukajLokalne.Visible = False
        '
        'lblKategoria
        '
        Me.lblKategoria.AutoSize = True
        Me.lblKategoria.Location = New System.Drawing.Point(428, 41)
        Me.lblKategoria.Name = "lblKategoria"
        Me.lblKategoria.Size = New System.Drawing.Size(55, 13)
        Me.lblKategoria.TabIndex = 7
        Me.lblKategoria.Text = "Kategoria:"
        '
        'cmbKategoria
        '
        Me.cmbKategoria.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbKategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbKategoria.FormattingEnabled = True
        Me.cmbKategoria.Location = New System.Drawing.Point(489, 38)
        Me.cmbKategoria.Name = "cmbKategoria"
        Me.cmbKategoria.Size = New System.Drawing.Size(185, 21)
        Me.cmbKategoria.TabIndex = 8
        '
        'chkUkryjDodane
        '
        Me.chkUkryjDodane.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chkUkryjDodane.AutoSize = True
        Me.chkUkryjDodane.Location = New System.Drawing.Point(680, 31)
        Me.chkUkryjDodane.Name = "chkUkryjDodane"
        Me.chkUkryjDodane.Size = New System.Drawing.Size(168, 17)
        Me.chkUkryjDodane.TabIndex = 9
        Me.chkUkryjDodane.Text = "Ukryj już w MapoTero"
        Me.ToolTip1.SetToolTip(Me.chkUkryjDodane, "Ukrywa usługi, które znajdują się już na liście map w programie.")
        Me.chkUkryjDodane.UseVisualStyleBackColor = True
        '
        'chkUkryjSkorowidze
        '
        Me.chkUkryjSkorowidze.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chkUkryjSkorowidze.AutoSize = True
        Me.chkUkryjSkorowidze.Location = New System.Drawing.Point(680, 50)
        Me.chkUkryjSkorowidze.Name = "chkUkryjSkorowidze"
        Me.chkUkryjSkorowidze.Size = New System.Drawing.Size(168, 17)
        Me.chkUkryjSkorowidze.TabIndex = 10
        Me.chkUkryjSkorowidze.Text = "Ukryj skorowidze arkuszy"
        Me.ToolTip1.SetToolTip(Me.chkUkryjSkorowidze, "Ukrywa usługi prezentujące wyłącznie siatki arkuszy i zasięgi zamiast właściwych map.")
        Me.chkUkryjSkorowidze.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 73)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(660, 22)
        Me.lblStatus.TabIndex = 11
        Me.lblStatus.Text = "Gotowe."
        '
        'prgPostep
        '
        Me.prgPostep.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.prgPostep.Location = New System.Drawing.Point(680, 73)
        Me.prgPostep.MarqueeAnimationSpeed = 30
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(168, 15)
        Me.prgPostep.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgPostep.TabIndex = 12
        Me.prgPostep.Visible = False
        '
        'pnlDol
        '
        Me.pnlDol.Controls.Add(Me.btnKopiujUrl)
        Me.pnlDol.Controls.Add(Me.btnWlasnyAdres)
        Me.pnlDol.Controls.Add(Me.btnAnuluj)
        Me.pnlDol.Controls.Add(Me.btnWybierz)
        Me.pnlDol.Controls.Add(Me.txtOpisPodglad)
        Me.pnlDol.Controls.Add(Me.lblOpisPodglad)
        Me.pnlDol.Controls.Add(Me.txtAdresPodglad)
        Me.pnlDol.Controls.Add(Me.lblAdresPodglad)
        Me.pnlDol.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlDol.Location = New System.Drawing.Point(0, 425)
        Me.pnlDol.Name = "pnlDol"
        Me.pnlDol.Size = New System.Drawing.Size(860, 160)
        Me.pnlDol.TabIndex = 2
        '
        'lblAdresPodglad
        '
        Me.lblAdresPodglad.AutoSize = True
        Me.lblAdresPodglad.Location = New System.Drawing.Point(12, 12)
        Me.lblAdresPodglad.Name = "lblAdresPodglad"
        Me.lblAdresPodglad.Size = New System.Drawing.Size(70, 13)
        Me.lblAdresPodglad.TabIndex = 0
        Me.lblAdresPodglad.Text = "Adres usługi:"
        '
        'txtAdresPodglad
        '
        Me.txtAdresPodglad.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAdresPodglad.BackColor = System.Drawing.SystemColors.Window
        Me.txtAdresPodglad.Location = New System.Drawing.Point(94, 9)
        Me.txtAdresPodglad.Name = "txtAdresPodglad"
        Me.txtAdresPodglad.ReadOnly = True
        Me.txtAdresPodglad.Size = New System.Drawing.Size(666, 20)
        Me.txtAdresPodglad.TabIndex = 1
        '
        'btnKopiujUrl
        '
        Me.btnKopiujUrl.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnKopiujUrl.Location = New System.Drawing.Point(766, 7)
        Me.btnKopiujUrl.Name = "btnKopiujUrl"
        Me.btnKopiujUrl.Size = New System.Drawing.Size(82, 23)
        Me.btnKopiujUrl.TabIndex = 2
        Me.btnKopiujUrl.Text = "Kopiuj URL"
        Me.ToolTip1.SetToolTip(Me.btnKopiujUrl, "Kopiuje adres URL wybranej usługi do schowka systemu Windows.")
        Me.btnKopiujUrl.UseVisualStyleBackColor = True
        '
        'lblOpisPodglad
        '
        Me.lblOpisPodglad.AutoSize = True
        Me.lblOpisPodglad.Location = New System.Drawing.Point(12, 37)
        Me.lblOpisPodglad.Name = "lblOpisPodglad"
        Me.lblOpisPodglad.Size = New System.Drawing.Size(58, 13)
        Me.lblOpisPodglad.TabIndex = 3
        Me.lblOpisPodglad.Text = "Szczegóły:"
        '
        'txtOpisPodglad
        '
        Me.txtOpisPodglad.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtOpisPodglad.BackColor = System.Drawing.SystemColors.Window
        Me.txtOpisPodglad.Location = New System.Drawing.Point(94, 35)
        Me.txtOpisPodglad.Multiline = True
        Me.txtOpisPodglad.Name = "txtOpisPodglad"
        Me.txtOpisPodglad.ReadOnly = True
        Me.txtOpisPodglad.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtOpisPodglad.Size = New System.Drawing.Size(754, 74)
        Me.txtOpisPodglad.TabIndex = 4
        '
        'btnWlasnyAdres
        '
        Me.btnWlasnyAdres.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnWlasnyAdres.Location = New System.Drawing.Point(12, 120)
        Me.btnWlasnyAdres.Name = "btnWlasnyAdres"
        Me.btnWlasnyAdres.Size = New System.Drawing.Size(272, 28)
        Me.btnWlasnyAdres.TabIndex = 5
        Me.btnWlasnyAdres.Text = "+ Wpisz własny adres URL (WMS / WMTS / XYZ)..."
        Me.ToolTip1.SetToolTip(Me.btnWlasnyAdres, "Przechodzi do ręcznego wpisania własnego adresu serwera WMS, WMTS, szablonu XYZ/TMS lub TileJSON.")
        Me.btnWlasnyAdres.UseVisualStyleBackColor = True
        '
        'btnWybierz
        '
        Me.btnWybierz.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnWybierz.Enabled = False
        Me.btnWybierz.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnWybierz.Location = New System.Drawing.Point(542, 120)
        Me.btnWybierz.Name = "btnWybierz"
        Me.btnWybierz.Size = New System.Drawing.Size(206, 28)
        Me.btnWybierz.TabIndex = 6
        Me.btnWybierz.Text = "Dalej: pobierz warstwy →"
        Me.btnWybierz.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(754, 120)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(94, 28)
        Me.btnAnuluj.TabIndex = 7
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'pnlLista
        '
        Me.pnlLista.Controls.Add(Me.lvUslugi)
        Me.pnlLista.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlLista.Location = New System.Drawing.Point(0, 100)
        Me.pnlLista.Name = "pnlLista"
        Me.pnlLista.Padding = New System.Windows.Forms.Padding(12, 0, 12, 0)
        Me.pnlLista.Size = New System.Drawing.Size(860, 325)
        Me.pnlLista.TabIndex = 1
        '
        'lvUslugi
        '
        Me.lvUslugi.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colNazwa, Me.colKategoria, Me.colDostawca, Me.colTyp, Me.colStan})
        Me.lvUslugi.ContextMenuStrip = Me.ctxMenuUslugi
        Me.lvUslugi.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvUslugi.FullRowSelect = True
        Me.lvUslugi.GridLines = True
        Me.lvUslugi.HideSelection = False
        Me.lvUslugi.Location = New System.Drawing.Point(12, 0)
        Me.lvUslugi.MultiSelect = False
        Me.lvUslugi.Name = "lvUslugi"
        Me.lvUslugi.Size = New System.Drawing.Size(836, 325)
        Me.lvUslugi.TabIndex = 0
        Me.lvUslugi.UseCompatibleStateImageBehavior = False
        Me.lvUslugi.View = System.Windows.Forms.View.Details
        '
        'colNazwa
        '
        Me.colNazwa.Text = "Nazwa usługi / zbioru"
        Me.colNazwa.Width = 330
        '
        'colKategoria
        '
        Me.colKategoria.Text = "Kategoria"
        Me.colKategoria.Width = 150
        '
        'colDostawca
        '
        Me.colDostawca.Text = "Instytucja / obszar"
        Me.colDostawca.Width = 180
        '
        'colTyp
        '
        Me.colTyp.Text = "Typ"
        Me.colTyp.Width = 54
        '
        'colStan
        '
        Me.colStan.Text = "Stan w MapoTero"
        Me.colStan.Width = 120
        '
        'ctxMenuUslugi
        '
        Me.ctxMenuUslugi.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuDalej, Me.mnuKopiujUrl, Me.mnuKopiujNazwe, Me.mnuSep1, Me.mnuOtworzPrzegladarka})
        Me.ctxMenuUslugi.Name = "ctxMenuUslugi"
        Me.ctxMenuUslugi.Size = New System.Drawing.Size(236, 104)
        '
        'mnuDalej
        '
        Me.mnuDalej.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.mnuDalej.Name = "mnuDalej"
        Me.mnuDalej.Size = New System.Drawing.Size(235, 22)
        Me.mnuDalej.Text = "Dalej: pobierz warstwy (Enter)"
        '
        'mnuKopiujUrl
        '
        Me.mnuKopiujUrl.Name = "mnuKopiujUrl"
        Me.mnuKopiujUrl.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.C), System.Windows.Forms.Keys)
        Me.mnuKopiujUrl.Size = New System.Drawing.Size(235, 22)
        Me.mnuKopiujUrl.Text = "Kopiuj adres URL"
        '
        'mnuKopiujNazwe
        '
        Me.mnuKopiujNazwe.Name = "mnuKopiujNazwe"
        Me.mnuKopiujNazwe.Size = New System.Drawing.Size(235, 22)
        Me.mnuKopiujNazwe.Text = "Kopiuj nazwę usługi"
        '
        'mnuSep1
        '
        Me.mnuSep1.Name = "mnuSep1"
        Me.mnuSep1.Size = New System.Drawing.Size(232, 6)
        '
        'mnuOtworzPrzegladarka
        '
        Me.mnuOtworzPrzegladarka.Name = "mnuOtworzPrzegladarka"
        Me.mnuOtworzPrzegladarka.Size = New System.Drawing.Size(235, 22)
        Me.mnuOtworzPrzegladarka.Text = "Otwórz adres w przeglądarce..."
        '
        'PanelKatalogSerwerow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pnlLista)
        Me.Controls.Add(Me.pnlDol)
        Me.Controls.Add(Me.pnlGora)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Name = "PanelKatalogSerwerow"
        Me.Size = New System.Drawing.Size(860, 585)
        Me.pnlGora.ResumeLayout(False)
        Me.pnlGora.PerformLayout()
        Me.pnlDol.ResumeLayout(False)
        Me.pnlDol.PerformLayout()
        Me.pnlLista.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlGora As System.Windows.Forms.Panel
    Friend WithEvents lblZrodlo As System.Windows.Forms.Label
    Friend WithEvents cmbZrodlo As System.Windows.Forms.ComboBox
    Friend WithEvents cmbZakresLokalny As System.Windows.Forms.ComboBox
    Friend WithEvents btnOdswiez As System.Windows.Forms.Button
    Friend WithEvents lblSzukaj As System.Windows.Forms.Label
    Friend WithEvents txtSzukaj As System.Windows.Forms.TextBox
    Friend WithEvents btnSzukajLokalne As System.Windows.Forms.Button
    Friend WithEvents lblKategoria As System.Windows.Forms.Label
    Friend WithEvents cmbKategoria As System.Windows.Forms.ComboBox
    Friend WithEvents chkUkryjDodane As System.Windows.Forms.CheckBox
    Friend WithEvents chkUkryjSkorowidze As System.Windows.Forms.CheckBox
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents prgPostep As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlDol As System.Windows.Forms.Panel
    Friend WithEvents lblAdresPodglad As System.Windows.Forms.Label
    Friend WithEvents txtAdresPodglad As System.Windows.Forms.TextBox
    Friend WithEvents btnKopiujUrl As System.Windows.Forms.Button
    Friend WithEvents lblOpisPodglad As System.Windows.Forms.Label
    Friend WithEvents txtOpisPodglad As System.Windows.Forms.TextBox
    Friend WithEvents btnWlasnyAdres As System.Windows.Forms.Button
    Friend WithEvents btnWybierz As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents pnlLista As System.Windows.Forms.Panel
    Friend WithEvents lvUslugi As System.Windows.Forms.ListView
    Friend WithEvents colNazwa As System.Windows.Forms.ColumnHeader
    Friend WithEvents colKategoria As System.Windows.Forms.ColumnHeader
    Friend WithEvents colDostawca As System.Windows.Forms.ColumnHeader
    Friend WithEvents colTyp As System.Windows.Forms.ColumnHeader
    Friend WithEvents colStan As System.Windows.Forms.ColumnHeader
    Friend WithEvents ctxMenuUslugi As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuDalej As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuKopiujUrl As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuKopiujNazwe As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSep1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuOtworzPrzegladarka As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
