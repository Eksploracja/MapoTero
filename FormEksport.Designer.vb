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
        Me.nudMaksKafli = New System.Windows.Forms.NumericUpDown()
        Me.rbMBTiles = New System.Windows.Forms.RadioButton()
        Me.rbKmz = New System.Windows.Forms.RadioButton()
        Me.rbKmzGarmin = New System.Windows.Forms.RadioButton()
        Me.grpMBTiles = New System.Windows.Forms.GroupBox()
        Me.chkPng = New System.Windows.Forms.CheckBox()
        Me.nudPoziomMax = New System.Windows.Forms.NumericUpDown()
        Me.lblPoziomDo = New System.Windows.Forms.Label()
        Me.nudPoziomMin = New System.Windows.Forms.NumericUpDown()
        Me.lblPoziomOd = New System.Windows.Forms.Label()
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
        Me.lblInfo.Location = New System.Drawing.Point(12, 58)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(486, 30)
        Me.lblInfo.TabIndex = 3
        '
        'grpFormat
        '
        Me.grpFormat.Controls.Add(Me.nudMaksKafli)
        Me.grpFormat.Controls.Add(Me.rbMBTiles)
        Me.grpFormat.Controls.Add(Me.rbKmz)
        Me.grpFormat.Controls.Add(Me.rbKmzGarmin)
        Me.grpFormat.Location = New System.Drawing.Point(12, 92)
        Me.grpFormat.Name = "grpFormat"
        Me.grpFormat.Size = New System.Drawing.Size(486, 92)
        Me.grpFormat.TabIndex = 4
        Me.grpFormat.TabStop = False
        Me.grpFormat.Text = "Format"
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
        Me.grpMBTiles.Location = New System.Drawing.Point(12, 188)
        Me.grpMBTiles.Name = "grpMBTiles"
        Me.grpMBTiles.Size = New System.Drawing.Size(486, 50)
        Me.grpMBTiles.TabIndex = 5
        Me.grpMBTiles.TabStop = False
        Me.grpMBTiles.Text = "MBTiles"
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
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Location = New System.Drawing.Point(12, 248)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(70, 13)
        Me.lblJakosc.TabIndex = 6
        Me.lblJakosc.Text = "Jakość JPEG:"
        '
        'nudJakosc
        '
        Me.nudJakosc.Location = New System.Drawing.Point(90, 246)
        Me.nudJakosc.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudJakosc.Name = "nudJakosc"
        Me.nudJakosc.Size = New System.Drawing.Size(50, 20)
        Me.nudJakosc.TabIndex = 7
        Me.nudJakosc.Value = New Decimal(New Integer() {85, 0, 0, 0})
        '
        'lblPlan
        '
        Me.lblPlan.Location = New System.Drawing.Point(150, 244)
        Me.lblPlan.Name = "lblPlan"
        Me.lblPlan.Size = New System.Drawing.Size(348, 30)
        Me.lblPlan.TabIndex = 8
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.Location = New System.Drawing.Point(12, 278)
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.ReadOnly = True
        Me.txtKomunikaty.Size = New System.Drawing.Size(486, 60)
        Me.txtKomunikaty.TabIndex = 9
        Me.txtKomunikaty.Text = ""
        '
        'prgEksport
        '
        Me.prgEksport.Location = New System.Drawing.Point(12, 352)
        Me.prgEksport.Name = "prgEksport"
        Me.prgEksport.Size = New System.Drawing.Size(316, 23)
        Me.prgEksport.TabIndex = 10
        '
        'btnEksportuj
        '
        Me.btnEksportuj.Location = New System.Drawing.Point(336, 347)
        Me.btnEksportuj.Name = "btnEksportuj"
        Me.btnEksportuj.Size = New System.Drawing.Size(80, 32)
        Me.btnEksportuj.TabIndex = 11
        Me.btnEksportuj.Text = "Eksportuj"
        Me.btnEksportuj.UseVisualStyleBackColor = True
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(420, 347)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(78, 32)
        Me.btnPrzerwij.TabIndex = 12
        Me.btnPrzerwij.Text = "Przerwij"
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'FormEksport
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(510, 390)
        Me.Controls.Add(Me.btnPrzerwij)
        Me.Controls.Add(Me.btnEksportuj)
        Me.Controls.Add(Me.prgEksport)
        Me.Controls.Add(Me.txtKomunikaty)
        Me.Controls.Add(Me.lblPlan)
        Me.Controls.Add(Me.nudJakosc)
        Me.Controls.Add(Me.lblJakosc)
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
        Me.Text = "Eksport mapy do KMZ / MBTiles"
        Me.grpFormat.ResumeLayout(False)
        Me.grpFormat.PerformLayout()
        CType(Me.nudMaksKafli, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMBTiles.ResumeLayout(False)
        Me.grpMBTiles.PerformLayout()
        CType(Me.nudPoziomMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudPoziomMin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblFolder As System.Windows.Forms.Label
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnFolder As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents grpFormat As System.Windows.Forms.GroupBox
    Friend WithEvents nudMaksKafli As System.Windows.Forms.NumericUpDown
    Friend WithEvents rbMBTiles As System.Windows.Forms.RadioButton
    Friend WithEvents rbKmz As System.Windows.Forms.RadioButton
    Friend WithEvents rbKmzGarmin As System.Windows.Forms.RadioButton
    Friend WithEvents grpMBTiles As System.Windows.Forms.GroupBox
    Friend WithEvents chkPng As System.Windows.Forms.CheckBox
    Friend WithEvents nudPoziomMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPoziomDo As System.Windows.Forms.Label
    Friend WithEvents nudPoziomMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPoziomOd As System.Windows.Forms.Label
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
