<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormSzukajDzialki
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
        Me.tabSzukaj = New System.Windows.Forms.TabControl()
        Me.tabMiejscowosc = New System.Windows.Forms.TabPage()
        Me.lvwMiejscowosci = New System.Windows.Forms.ListView()
        Me.colMiejscowoscNazwa = New System.Windows.Forms.ColumnHeader()
        Me.colMiejscowoscTyp = New System.Windows.Forms.ColumnHeader()
        Me.colMiejscowoscWspolrzedne = New System.Windows.Forms.ColumnHeader()
        Me.lblStatusMiejscowosc = New System.Windows.Forms.Label()
        Me.prgSzukajMiejscowosc = New System.Windows.Forms.ProgressBar()
        Me.btnSzukajMiejscowosc = New System.Windows.Forms.Button()
        Me.txtZapytanieMiejscowosc = New System.Windows.Forms.TextBox()
        Me.lblMiejscowoscInstrukcja = New System.Windows.Forms.Label()
        Me.tabDzialka = New System.Windows.Forms.TabPage()
        Me.chkWarstwy = New System.Windows.Forms.CheckBox()
        Me.nudMargines = New System.Windows.Forms.NumericUpDown()
        Me.lblMargines = New System.Windows.Forms.Label()
        Me.lvwWyniki = New System.Windows.Forms.ListView()
        Me.colNumer = New System.Windows.Forms.ColumnHeader()
        Me.colObreb = New System.Windows.Forms.ColumnHeader()
        Me.colGmina = New System.Windows.Forms.ColumnHeader()
        Me.colTeryt = New System.Windows.Forms.ColumnHeader()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.prgSzukaj = New System.Windows.Forms.ProgressBar()
        Me.btnSzukaj = New System.Windows.Forms.Button()
        Me.txtZapytanie = New System.Windows.Forms.TextBox()
        Me.lblInstrukcja = New System.Windows.Forms.Label()
        Me.btnWybierz = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.tabSzukaj.SuspendLayout()
        Me.tabMiejscowosc.SuspendLayout()
        Me.tabDzialka.SuspendLayout()
        CType(Me.nudMargines, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tabSzukaj
        '
        Me.tabSzukaj.Controls.Add(Me.tabMiejscowosc)
        Me.tabSzukaj.Controls.Add(Me.tabDzialka)
        Me.tabSzukaj.Location = New System.Drawing.Point(12, 12)
        Me.tabSzukaj.Name = "tabSzukaj"
        Me.tabSzukaj.SelectedIndex = 0
        Me.tabSzukaj.Size = New System.Drawing.Size(536, 362)
        Me.tabSzukaj.TabIndex = 0
        '
        'tabMiejscowosc
        '
        Me.tabMiejscowosc.Controls.Add(Me.lvwMiejscowosci)
        Me.tabMiejscowosc.Controls.Add(Me.lblStatusMiejscowosc)
        Me.tabMiejscowosc.Controls.Add(Me.prgSzukajMiejscowosc)
        Me.tabMiejscowosc.Controls.Add(Me.btnSzukajMiejscowosc)
        Me.tabMiejscowosc.Controls.Add(Me.txtZapytanieMiejscowosc)
        Me.tabMiejscowosc.Controls.Add(Me.lblMiejscowoscInstrukcja)
        Me.tabMiejscowosc.Location = New System.Drawing.Point(4, 22)
        Me.tabMiejscowosc.Name = "tabMiejscowosc"
        Me.tabMiejscowosc.Padding = New System.Windows.Forms.Padding(3)
        Me.tabMiejscowosc.Size = New System.Drawing.Size(528, 336)
        Me.tabMiejscowosc.TabIndex = 0
        Me.tabMiejscowosc.Text = "Miejscowość / Adres"
        Me.tabMiejscowosc.UseVisualStyleBackColor = True
        '
        'lvwMiejscowosci
        '
        Me.lvwMiejscowosci.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colMiejscowoscNazwa, Me.colMiejscowoscTyp, Me.colMiejscowoscWspolrzedne})
        Me.lvwMiejscowosci.FullRowSelect = True
        Me.lvwMiejscowosci.GridLines = True
        Me.lvwMiejscowosci.HideSelection = False
        Me.lvwMiejscowosci.Location = New System.Drawing.Point(12, 86)
        Me.lvwMiejscowosci.MultiSelect = False
        Me.lvwMiejscowosci.Name = "lvwMiejscowosci"
        Me.lvwMiejscowosci.Size = New System.Drawing.Size(503, 238)
        Me.lvwMiejscowosci.TabIndex = 5
        Me.lvwMiejscowosci.UseCompatibleStateImageBehavior = False
        Me.lvwMiejscowosci.View = System.Windows.Forms.View.Details
        '
        'colMiejscowoscNazwa
        '
        Me.colMiejscowoscNazwa.Text = "Nazwa / Adres"
        Me.colMiejscowoscNazwa.Width = 265
        '
        'colMiejscowoscTyp
        '
        Me.colMiejscowoscTyp.Text = "Typ"
        Me.colMiejscowoscTyp.Width = 110
        '
        'colMiejscowoscWspolrzedne
        '
        Me.colMiejscowoscWspolrzedne.Text = "Współrzędne"
        Me.colMiejscowoscWspolrzedne.Width = 120
        '
        'lblStatusMiejscowosc
        '
        Me.lblStatusMiejscowosc.AutoSize = True
        Me.lblStatusMiejscowosc.Location = New System.Drawing.Point(10, 68)
        Me.lblStatusMiejscowosc.Name = "lblStatusMiejscowosc"
        Me.lblStatusMiejscowosc.Size = New System.Drawing.Size(109, 13)
        Me.lblStatusMiejscowosc.TabIndex = 4
        Me.lblStatusMiejscowosc.Text = "Wyniki wyszukiwania:"
        '
        'prgSzukajMiejscowosc
        '
        Me.prgSzukajMiejscowosc.Location = New System.Drawing.Point(12, 54)
        Me.prgSzukajMiejscowosc.Name = "prgSzukajMiejscowosc"
        Me.prgSzukajMiejscowosc.Size = New System.Drawing.Size(503, 6)
        Me.prgSzukajMiejscowosc.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgSzukajMiejscowosc.TabIndex = 3
        Me.prgSzukajMiejscowosc.Visible = False
        '
        'btnSzukajMiejscowosc
        '
        Me.btnSzukajMiejscowosc.Location = New System.Drawing.Point(420, 27)
        Me.btnSzukajMiejscowosc.Name = "btnSzukajMiejscowosc"
        Me.btnSzukajMiejscowosc.Size = New System.Drawing.Size(95, 23)
        Me.btnSzukajMiejscowosc.TabIndex = 2
        Me.btnSzukajMiejscowosc.Text = "Szukaj"
        Me.btnSzukajMiejscowosc.UseVisualStyleBackColor = True
        '
        'txtZapytanieMiejscowosc
        '
        Me.txtZapytanieMiejscowosc.Location = New System.Drawing.Point(12, 29)
        Me.txtZapytanieMiejscowosc.Name = "txtZapytanieMiejscowosc"
        Me.txtZapytanieMiejscowosc.Size = New System.Drawing.Size(400, 20)
        Me.txtZapytanieMiejscowosc.TabIndex = 1
        '
        'lblMiejscowoscInstrukcja
        '
        Me.lblMiejscowoscInstrukcja.AutoSize = True
        Me.lblMiejscowoscInstrukcja.Location = New System.Drawing.Point(10, 11)
        Me.lblMiejscowoscInstrukcja.Name = "lblMiejscowoscInstrukcja"
        Me.lblMiejscowoscInstrukcja.Size = New System.Drawing.Size(447, 13)
        Me.lblMiejscowoscInstrukcja.TabIndex = 0
        Me.lblMiejscowoscInstrukcja.Text = "Wpisz nazwę miejscowości, wsi, ulicy lub adres (np. Kraków, Zakopane, Gdańsk Długa 1):"
        '
        'tabDzialka
        '
        Me.tabDzialka.Controls.Add(Me.chkWarstwy)
        Me.tabDzialka.Controls.Add(Me.nudMargines)
        Me.tabDzialka.Controls.Add(Me.lblMargines)
        Me.tabDzialka.Controls.Add(Me.lvwWyniki)
        Me.tabDzialka.Controls.Add(Me.lblStatus)
        Me.tabDzialka.Controls.Add(Me.prgSzukaj)
        Me.tabDzialka.Controls.Add(Me.btnSzukaj)
        Me.tabDzialka.Controls.Add(Me.txtZapytanie)
        Me.tabDzialka.Controls.Add(Me.lblInstrukcja)
        Me.tabDzialka.Location = New System.Drawing.Point(4, 22)
        Me.tabDzialka.Name = "tabDzialka"
        Me.tabDzialka.Padding = New System.Windows.Forms.Padding(3)
        Me.tabDzialka.Size = New System.Drawing.Size(528, 336)
        Me.tabDzialka.TabIndex = 1
        Me.tabDzialka.Text = "Działka katastralna (ULDK)"
        Me.tabDzialka.UseVisualStyleBackColor = True
        '
        'chkWarstwy
        '
        Me.chkWarstwy.AutoSize = True
        Me.chkWarstwy.Checked = True
        Me.chkWarstwy.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWarstwy.Location = New System.Drawing.Point(12, 307)
        Me.chkWarstwy.Name = "chkWarstwy"
        Me.chkWarstwy.Size = New System.Drawing.Size(350, 17)
        Me.chkWarstwy.TabIndex = 8
        Me.chkWarstwy.Text = "Włącz warstwy: Ortofotomapa i Działki ewidencyjne (EGiB GUGiK)"
        Me.chkWarstwy.UseVisualStyleBackColor = True
        '
        'nudMargines
        '
        Me.nudMargines.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudMargines.Location = New System.Drawing.Point(203, 279)
        Me.nudMargines.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.nudMargines.Name = "nudMargines"
        Me.nudMargines.Size = New System.Drawing.Size(70, 20)
        Me.nudMargines.TabIndex = 7
        Me.nudMargines.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'lblMargines
        '
        Me.lblMargines.AutoSize = True
        Me.lblMargines.Location = New System.Drawing.Point(9, 281)
        Me.lblMargines.Name = "lblMargines"
        Me.lblMargines.Size = New System.Drawing.Size(188, 13)
        Me.lblMargines.TabIndex = 6
        Me.lblMargines.Text = "Margines wokół działki na mapie [m]:"
        '
        'lvwWyniki
        '
        Me.lvwWyniki.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colNumer, Me.colObreb, Me.colGmina, Me.colTeryt})
        Me.lvwWyniki.FullRowSelect = True
        Me.lvwWyniki.GridLines = True
        Me.lvwWyniki.HideSelection = False
        Me.lvwWyniki.Location = New System.Drawing.Point(12, 86)
        Me.lvwWyniki.MultiSelect = False
        Me.lvwWyniki.Name = "lvwWyniki"
        Me.lvwWyniki.Size = New System.Drawing.Size(503, 185)
        Me.lvwWyniki.TabIndex = 5
        Me.lvwWyniki.UseCompatibleStateImageBehavior = False
        Me.lvwWyniki.View = System.Windows.Forms.View.Details
        '
        'colNumer
        '
        Me.colNumer.Text = "Działka"
        Me.colNumer.Width = 85
        '
        'colObreb
        '
        Me.colObreb.Text = "Obręb"
        Me.colObreb.Width = 115
        '
        'colGmina
        '
        Me.colGmina.Text = "Gmina"
        Me.colGmina.Width = 100
        '
        'colTeryt
        '
        Me.colTeryt.Text = "Identyfikator TERYT"
        Me.colTeryt.Width = 180
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(10, 68)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(109, 13)
        Me.lblStatus.TabIndex = 4
        Me.lblStatus.Text = "Wyniki wyszukiwania:"
        '
        'prgSzukaj
        '
        Me.prgSzukaj.Location = New System.Drawing.Point(12, 54)
        Me.prgSzukaj.Name = "prgSzukaj"
        Me.prgSzukaj.Size = New System.Drawing.Size(503, 6)
        Me.prgSzukaj.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgSzukaj.TabIndex = 3
        Me.prgSzukaj.Visible = False
        '
        'btnSzukaj
        '
        Me.btnSzukaj.Location = New System.Drawing.Point(420, 27)
        Me.btnSzukaj.Name = "btnSzukaj"
        Me.btnSzukaj.Size = New System.Drawing.Size(95, 23)
        Me.btnSzukaj.TabIndex = 2
        Me.btnSzukaj.Text = "Szukaj"
        Me.btnSzukaj.UseVisualStyleBackColor = True
        '
        'txtZapytanie
        '
        Me.txtZapytanie.Location = New System.Drawing.Point(12, 29)
        Me.txtZapytanie.Name = "txtZapytanie"
        Me.txtZapytanie.Size = New System.Drawing.Size(400, 20)
        Me.txtZapytanie.TabIndex = 1
        '
        'lblInstrukcja
        '
        Me.lblInstrukcja.AutoSize = True
        Me.lblInstrukcja.Location = New System.Drawing.Point(10, 11)
        Me.lblInstrukcja.Name = "lblInstrukcja"
        Me.lblInstrukcja.Size = New System.Drawing.Size(460, 13)
        Me.lblInstrukcja.TabIndex = 0
        Me.lblInstrukcja.Text = "Wpisz pełny identyfikator TERYT (np. 022503_5.0003.134) lub obręb i numer (np. Krzewina 134):"
        '
        'btnWybierz
        '
        Me.btnWybierz.Enabled = False
        Me.btnWybierz.Location = New System.Drawing.Point(308, 384)
        Me.btnWybierz.Name = "btnWybierz"
        Me.btnWybierz.Size = New System.Drawing.Size(130, 26)
        Me.btnWybierz.TabIndex = 1
        Me.btnWybierz.Text = "Pokaż na mapie"
        Me.btnWybierz.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(448, 384)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(96, 26)
        Me.btnAnuluj.TabIndex = 2
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'FormSzukajDzialki
        '
        Me.AcceptButton = Me.btnSzukajMiejscowosc
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(560, 422)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnWybierz)
        Me.Controls.Add(Me.tabSzukaj)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormSzukajDzialki"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Wyszukiwarka miejscowości i działek"
        Me.tabSzukaj.ResumeLayout(False)
        Me.tabMiejscowosc.ResumeLayout(False)
        Me.tabMiejscowosc.PerformLayout()
        Me.tabDzialka.ResumeLayout(False)
        Me.tabDzialka.PerformLayout()
        CType(Me.nudMargines, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents tabSzukaj As System.Windows.Forms.TabControl
    Friend WithEvents tabMiejscowosc As System.Windows.Forms.TabPage
    Friend WithEvents tabDzialka As System.Windows.Forms.TabPage
    Friend WithEvents lblMiejscowoscInstrukcja As System.Windows.Forms.Label
    Friend WithEvents txtZapytanieMiejscowosc As System.Windows.Forms.TextBox
    Friend WithEvents btnSzukajMiejscowosc As System.Windows.Forms.Button
    Friend WithEvents prgSzukajMiejscowosc As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatusMiejscowosc As System.Windows.Forms.Label
    Friend WithEvents lvwMiejscowosci As System.Windows.Forms.ListView
    Friend WithEvents colMiejscowoscNazwa As System.Windows.Forms.ColumnHeader
    Friend WithEvents colMiejscowoscTyp As System.Windows.Forms.ColumnHeader
    Friend WithEvents colMiejscowoscWspolrzedne As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblInstrukcja As System.Windows.Forms.Label
    Friend WithEvents txtZapytanie As System.Windows.Forms.TextBox
    Friend WithEvents btnSzukaj As System.Windows.Forms.Button
    Friend WithEvents prgSzukaj As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lvwWyniki As System.Windows.Forms.ListView
    Friend WithEvents colNumer As System.Windows.Forms.ColumnHeader
    Friend WithEvents colObreb As System.Windows.Forms.ColumnHeader
    Friend WithEvents colGmina As System.Windows.Forms.ColumnHeader
    Friend WithEvents colTeryt As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblMargines As System.Windows.Forms.Label
    Friend WithEvents nudMargines As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkWarstwy As System.Windows.Forms.CheckBox
    Friend WithEvents btnWybierz As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
End Class
