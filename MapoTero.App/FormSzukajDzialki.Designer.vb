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
        Me.lblInstrukcja = New System.Windows.Forms.Label()
        Me.txtZapytanie = New System.Windows.Forms.TextBox()
        Me.btnSzukaj = New System.Windows.Forms.Button()
        Me.prgSzukaj = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lvwWyniki = New System.Windows.Forms.ListView()
        Me.colNumer = New System.Windows.Forms.ColumnHeader()
        Me.colObreb = New System.Windows.Forms.ColumnHeader()
        Me.colGmina = New System.Windows.Forms.ColumnHeader()
        Me.colTeryt = New System.Windows.Forms.ColumnHeader()
        Me.lblMargines = New System.Windows.Forms.Label()
        Me.nudMargines = New System.Windows.Forms.NumericUpDown()
        Me.chkWarstwy = New System.Windows.Forms.CheckBox()
        Me.btnWybierz = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        CType(Me.nudMargines, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblInstrukcja
        '
        Me.lblInstrukcja.AutoSize = True
        Me.lblInstrukcja.Location = New System.Drawing.Point(12, 12)
        Me.lblInstrukcja.Name = "lblInstrukcja"
        Me.lblInstrukcja.Size = New System.Drawing.Size(460, 13)
        Me.lblInstrukcja.TabIndex = 0
        Me.lblInstrukcja.Text = "Wpisz pełny identyfikator TERYT (np. 022503_5.0003.134) lub obręb i numer (np. Krzewina 134):"
        '
        'txtZapytanie
        '
        Me.txtZapytanie.Location = New System.Drawing.Point(15, 30)
        Me.txtZapytanie.Name = "txtZapytanie"
        Me.txtZapytanie.Size = New System.Drawing.Size(395, 20)
        Me.txtZapytanie.TabIndex = 1
        '
        'btnSzukaj
        '
        Me.btnSzukaj.Location = New System.Drawing.Point(416, 28)
        Me.btnSzukaj.Name = "btnSzukaj"
        Me.btnSzukaj.Size = New System.Drawing.Size(100, 23)
        Me.btnSzukaj.TabIndex = 2
        Me.btnSzukaj.Text = "Szukaj"
        Me.btnSzukaj.UseVisualStyleBackColor = True
        '
        'prgSzukaj
        '
        Me.prgSzukaj.Location = New System.Drawing.Point(15, 54)
        Me.prgSzukaj.Name = "prgSzukaj"
        Me.prgSzukaj.Size = New System.Drawing.Size(501, 6)
        Me.prgSzukaj.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.prgSzukaj.TabIndex = 3
        Me.prgSzukaj.Visible = False
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 68)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(109, 13)
        Me.lblStatus.TabIndex = 4
        Me.lblStatus.Text = "Wyniki wyszukiwania:"
        '
        'lvwWyniki
        '
        Me.lvwWyniki.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colNumer, Me.colObreb, Me.colGmina, Me.colTeryt})
        Me.lvwWyniki.FullRowSelect = True
        Me.lvwWyniki.GridLines = True
        Me.lvwWyniki.HideSelection = False
        Me.lvwWyniki.Location = New System.Drawing.Point(15, 86)
        Me.lvwWyniki.MultiSelect = False
        Me.lvwWyniki.Name = "lvwWyniki"
        Me.lvwWyniki.Size = New System.Drawing.Size(501, 185)
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
        'lblMargines
        '
        Me.lblMargines.AutoSize = True
        Me.lblMargines.Location = New System.Drawing.Point(12, 286)
        Me.lblMargines.Name = "lblMargines"
        Me.lblMargines.Size = New System.Drawing.Size(188, 13)
        Me.lblMargines.TabIndex = 6
        Me.lblMargines.Text = "Margines wokół działki na mapie [m]:"
        '
        'nudMargines
        '
        Me.nudMargines.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudMargines.Location = New System.Drawing.Point(206, 284)
        Me.nudMargines.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.nudMargines.Name = "nudMargines"
        Me.nudMargines.Size = New System.Drawing.Size(70, 20)
        Me.nudMargines.TabIndex = 7
        Me.nudMargines.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'chkWarstwy
        '
        Me.chkWarstwy.AutoSize = True
        Me.chkWarstwy.Checked = True
        Me.chkWarstwy.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWarstwy.Location = New System.Drawing.Point(15, 314)
        Me.chkWarstwy.Name = "chkWarstwy"
        Me.chkWarstwy.Size = New System.Drawing.Size(350, 17)
        Me.chkWarstwy.TabIndex = 8
        Me.chkWarstwy.Text = "Włącz warstwy: Ortofotomapa i Działki ewidencyjne (EGiB GUGiK)"
        Me.chkWarstwy.UseVisualStyleBackColor = True
        '
        'btnWybierz
        '
        Me.btnWybierz.Enabled = False
        Me.btnWybierz.Location = New System.Drawing.Point(290, 345)
        Me.btnWybierz.Name = "btnWybierz"
        Me.btnWybierz.Size = New System.Drawing.Size(130, 26)
        Me.btnWybierz.TabIndex = 9
        Me.btnWybierz.Text = "Zaznacz na mapie"
        Me.btnWybierz.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(426, 345)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(90, 26)
        Me.btnAnuluj.TabIndex = 10
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'FormSzukajDzialki
        '
        Me.AcceptButton = Me.btnSzukaj
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(531, 386)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnWybierz)
        Me.Controls.Add(Me.chkWarstwy)
        Me.Controls.Add(Me.nudMargines)
        Me.Controls.Add(Me.lblMargines)
        Me.Controls.Add(Me.lvwWyniki)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.prgSzukaj)
        Me.Controls.Add(Me.btnSzukaj)
        Me.Controls.Add(Me.txtZapytanie)
        Me.Controls.Add(Me.lblInstrukcja)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormSzukajDzialki"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Wyszukiwarka działek katastralnych (ULDK GUGiK)"
        CType(Me.nudMargines, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

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
