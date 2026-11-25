<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormKonwerterGeoTiff
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
        Me.lblPlikTif = New System.Windows.Forms.Label()
        Me.txtPlikTif = New System.Windows.Forms.TextBox()
        Me.btnWybierzTif = New System.Windows.Forms.Button()
        Me.grpInfo = New System.Windows.Forms.GroupBox()
        Me.lblCrsStatus = New System.Windows.Forms.Label()
        Me.cmbUkladCrs = New System.Windows.Forms.ComboBox()
        Me.lblCrsEtykieta = New System.Windows.Forms.Label()
        Me.lblInfoZasieg = New System.Windows.Forms.Label()
        Me.lblInfoRozdzielczosc = New System.Windows.Forms.Label()
        Me.lblInfoWymiary = New System.Windows.Forms.Label()
        Me.grpFormat = New System.Windows.Forms.GroupBox()
        Me.rbGeoPackage = New System.Windows.Forms.RadioButton()
        Me.rbOruxMaps = New System.Windows.Forms.RadioButton()
        Me.rbSqlitedb = New System.Windows.Forms.RadioButton()
        Me.rbMbtiles = New System.Windows.Forms.RadioButton()
        Me.rbKmz = New System.Windows.Forms.RadioButton()
        Me.rbJnx = New System.Windows.Forms.RadioButton()
        Me.grpOpcje = New System.Windows.Forms.GroupBox()
        Me.lblZoomDo = New System.Windows.Forms.Label()
        Me.nudZoomDo = New System.Windows.Forms.NumericUpDown()
        Me.lblZoomOd = New System.Windows.Forms.Label()
        Me.nudZoomOd = New System.Windows.Forms.NumericUpDown()
        Me.chkPodzialKmz = New System.Windows.Forms.CheckBox()
        Me.nudJakosc = New System.Windows.Forms.NumericUpDown()
        Me.lblJakosc = New System.Windows.Forms.Label()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnKonwertuj = New System.Windows.Forms.Button()
        Me.btnPokazWFolderze = New System.Windows.Forms.Button()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgOtworz = New System.Windows.Forms.OpenFileDialog()
        Me.grpInfo.SuspendLayout()
        Me.grpFormat.SuspendLayout()
        Me.grpOpcje.SuspendLayout()
        CType(Me.nudZoomDo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudZoomOd, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblPlikTif
        '
        Me.lblPlikTif.AutoSize = True
        Me.lblPlikTif.Location = New System.Drawing.Point(12, 15)
        Me.lblPlikTif.Name = "lblPlikTif"
        Me.lblPlikTif.Size = New System.Drawing.Size(125, 13)
        Me.lblPlikTif.TabIndex = 0
        Me.lblPlikTif.Text = "Plik GeoTIFF (.tif / .tiff):"
        '
        'txtPlikTif
        '
        Me.txtPlikTif.Location = New System.Drawing.Point(143, 12)
        Me.txtPlikTif.Name = "txtPlikTif"
        Me.txtPlikTif.ReadOnly = True
        Me.txtPlikTif.Size = New System.Drawing.Size(309, 20)
        Me.txtPlikTif.TabIndex = 1
        '
        'btnWybierzTif
        '
        Me.btnWybierzTif.Location = New System.Drawing.Point(458, 10)
        Me.btnWybierzTif.Name = "btnWybierzTif"
        Me.btnWybierzTif.Size = New System.Drawing.Size(84, 23)
        Me.btnWybierzTif.TabIndex = 2
        Me.btnWybierzTif.Text = "Wybierz..."
        Me.btnWybierzTif.UseVisualStyleBackColor = True
        '
        'grpInfo
        '
        Me.grpInfo.Controls.Add(Me.lblCrsStatus)
        Me.grpInfo.Controls.Add(Me.cmbUkladCrs)
        Me.grpInfo.Controls.Add(Me.lblCrsEtykieta)
        Me.grpInfo.Controls.Add(Me.lblInfoZasieg)
        Me.grpInfo.Controls.Add(Me.lblInfoRozdzielczosc)
        Me.grpInfo.Controls.Add(Me.lblInfoWymiary)
        Me.grpInfo.Location = New System.Drawing.Point(12, 39)
        Me.grpInfo.Name = "grpInfo"
        Me.grpInfo.Size = New System.Drawing.Size(530, 115)
        Me.grpInfo.TabIndex = 3
        Me.grpInfo.TabStop = False
        Me.grpInfo.Text = "Informacje o pliku i georeferencja"
        '
        'lblCrsStatus
        '
        Me.lblCrsStatus.AutoSize = True
        Me.lblCrsStatus.Location = New System.Drawing.Point(340, 85)
        Me.lblCrsStatus.Name = "lblCrsStatus"
        Me.lblCrsStatus.Size = New System.Drawing.Size(130, 13)
        Me.lblCrsStatus.TabIndex = 5
        Me.lblCrsStatus.Text = "Wybierz układ współrzędnych"
        '
        'cmbUkladCrs
        '
        Me.cmbUkladCrs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUkladCrs.FormattingEnabled = True
        Me.cmbUkladCrs.Location = New System.Drawing.Point(120, 82)
        Me.cmbUkladCrs.Name = "cmbUkladCrs"
        Me.cmbUkladCrs.Size = New System.Drawing.Size(210, 21)
        Me.cmbUkladCrs.TabIndex = 4
        '
        'lblCrsEtykieta
        '
        Me.lblCrsEtykieta.AutoSize = True
        Me.lblCrsEtykieta.Location = New System.Drawing.Point(10, 85)
        Me.lblCrsEtykieta.Name = "lblCrsEtykieta"
        Me.lblCrsEtykieta.Size = New System.Drawing.Size(104, 13)
        Me.lblCrsEtykieta.TabIndex = 3
        Me.lblCrsEtykieta.Text = "Układ współrzędnych:"
        '
        'lblInfoZasieg
        '
        Me.lblInfoZasieg.AutoSize = True
        Me.lblInfoZasieg.Location = New System.Drawing.Point(10, 62)
        Me.lblInfoZasieg.Name = "lblInfoZasieg"
        Me.lblInfoZasieg.Size = New System.Drawing.Size(128, 13)
        Me.lblInfoZasieg.TabIndex = 2
        Me.lblInfoZasieg.Text = "Zasięg: (wybierz plik)"
        '
        'lblInfoRozdzielczosc
        '
        Me.lblInfoRozdzielczosc.AutoSize = True
        Me.lblInfoRozdzielczosc.Location = New System.Drawing.Point(10, 41)
        Me.lblInfoRozdzielczosc.Name = "lblInfoRozdzielczosc"
        Me.lblInfoRozdzielczosc.Size = New System.Drawing.Size(161, 13)
        Me.lblInfoRozdzielczosc.TabIndex = 1
        Me.lblInfoRozdzielczosc.Text = "Rozdzielczość piksela: -"
        '
        'lblInfoWymiary
        '
        Me.lblInfoWymiary.AutoSize = True
        Me.lblInfoWymiary.Location = New System.Drawing.Point(10, 20)
        Me.lblInfoWymiary.Name = "lblInfoWymiary"
        Me.lblInfoWymiary.Size = New System.Drawing.Size(130, 13)
        Me.lblInfoWymiary.TabIndex = 0
        Me.lblInfoWymiary.Text = "Wymiary: (brak pliku)"
        '
        'grpFormat
        '
        Me.grpFormat.Controls.Add(Me.rbGeoPackage)
        Me.grpFormat.Controls.Add(Me.rbOruxMaps)
        Me.grpFormat.Controls.Add(Me.rbSqlitedb)
        Me.grpFormat.Controls.Add(Me.rbMbtiles)
        Me.grpFormat.Controls.Add(Me.rbKmz)
        Me.grpFormat.Controls.Add(Me.rbJnx)
        Me.grpFormat.Location = New System.Drawing.Point(12, 160)
        Me.grpFormat.Name = "grpFormat"
        Me.grpFormat.Size = New System.Drawing.Size(530, 95)
        Me.grpFormat.TabIndex = 4
        Me.grpFormat.TabStop = False
        Me.grpFormat.Text = "Format wynikowy"
        '
        'rbGeoPackage
        '
        Me.rbGeoPackage.AutoSize = True
        Me.rbGeoPackage.Location = New System.Drawing.Point(280, 20)
        Me.rbGeoPackage.Name = "rbGeoPackage"
        Me.rbGeoPackage.Size = New System.Drawing.Size(242, 17)
        Me.rbGeoPackage.TabIndex = 3
        Me.rbGeoPackage.Text = "GeoPackage - QField, Mergin Maps"
        Me.rbGeoPackage.UseVisualStyleBackColor = True
        '
        'rbOruxMaps
        '
        Me.rbOruxMaps.AutoSize = True
        Me.rbOruxMaps.Location = New System.Drawing.Point(280, 66)
        Me.rbOruxMaps.Name = "rbOruxMaps"
        Me.rbOruxMaps.Size = New System.Drawing.Size(175, 17)
        Me.rbOruxMaps.TabIndex = 5
        Me.rbOruxMaps.Text = "OruxMaps (folder z bazą)"
        Me.rbOruxMaps.UseVisualStyleBackColor = True
        '
        'rbSqlitedb
        '
        Me.rbSqlitedb.AutoSize = True
        Me.rbSqlitedb.Location = New System.Drawing.Point(280, 43)
        Me.rbSqlitedb.Name = "rbSqlitedb"
        Me.rbSqlitedb.Size = New System.Drawing.Size(201, 17)
        Me.rbSqlitedb.TabIndex = 4
        Me.rbSqlitedb.Text = "SQLiteDB - OsmAnd"
        Me.rbSqlitedb.UseVisualStyleBackColor = True
        '
        'rbMbtiles
        '
        Me.rbMbtiles.AutoSize = True
        Me.rbMbtiles.Checked = True
        Me.rbMbtiles.Location = New System.Drawing.Point(15, 66)
        Me.rbMbtiles.Name = "rbMbtiles"
        Me.rbMbtiles.Size = New System.Drawing.Size(217, 17)
        Me.rbMbtiles.TabIndex = 2
        Me.rbMbtiles.TabStop = True
        Me.rbMbtiles.Text = "MBTiles - Locus Map, OsmAnd"
        Me.rbMbtiles.UseVisualStyleBackColor = True
        '
        'rbKmz
        '
        Me.rbKmz.AutoSize = True
        Me.rbKmz.Location = New System.Drawing.Point(15, 20)
        Me.rbKmz.Name = "rbKmz"
        Me.rbKmz.Size = New System.Drawing.Size(215, 17)
        Me.rbKmz.TabIndex = 0
        Me.rbKmz.Text = "KMZ - Garmin Custom Maps"
        Me.rbKmz.UseVisualStyleBackColor = True
        '
        'rbJnx
        '
        Me.rbJnx.AutoSize = True
        Me.rbJnx.Location = New System.Drawing.Point(15, 43)
        Me.rbJnx.Name = "rbJnx"
        Me.rbJnx.Size = New System.Drawing.Size(211, 17)
        Me.rbJnx.TabIndex = 1
        Me.rbJnx.Text = "JNX - Garmin BirdsEye"
        Me.rbJnx.UseVisualStyleBackColor = True
        '
        'grpOpcje
        '
        Me.grpOpcje.Controls.Add(Me.lblZoomDo)
        Me.grpOpcje.Controls.Add(Me.nudZoomDo)
        Me.grpOpcje.Controls.Add(Me.lblZoomOd)
        Me.grpOpcje.Controls.Add(Me.nudZoomOd)
        Me.grpOpcje.Controls.Add(Me.chkPodzialKmz)
        Me.grpOpcje.Controls.Add(Me.nudJakosc)
        Me.grpOpcje.Controls.Add(Me.lblJakosc)
        Me.grpOpcje.Location = New System.Drawing.Point(12, 261)
        Me.grpOpcje.Name = "grpOpcje"
        Me.grpOpcje.Size = New System.Drawing.Size(530, 56)
        Me.grpOpcje.TabIndex = 5
        Me.grpOpcje.TabStop = False
        Me.grpOpcje.Text = "Parametry konwersji"
        '
        'lblZoomDo
        '
        Me.lblZoomDo.AutoSize = True
        Me.lblZoomDo.Location = New System.Drawing.Point(260, 25)
        Me.lblZoomDo.Name = "lblZoomDo"
        Me.lblZoomDo.Size = New System.Drawing.Size(19, 13)
        Me.lblZoomDo.TabIndex = 6
        Me.lblZoomDo.Text = "do"
        '
        'nudZoomDo
        '
        Me.nudZoomDo.Location = New System.Drawing.Point(283, 22)
        Me.nudZoomDo.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudZoomDo.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudZoomDo.Name = "nudZoomDo"
        Me.nudZoomDo.Size = New System.Drawing.Size(42, 20)
        Me.nudZoomDo.TabIndex = 5
        Me.nudZoomDo.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'lblZoomOd
        '
        Me.lblZoomOd.AutoSize = True
        Me.lblZoomOd.Location = New System.Drawing.Point(155, 25)
        Me.lblZoomOd.Name = "lblZoomOd"
        Me.lblZoomOd.Size = New System.Drawing.Size(52, 13)
        Me.lblZoomOd.TabIndex = 4
        Me.lblZoomOd.Text = "Powiększenie od:"
        '
        'nudZoomOd
        '
        Me.nudZoomOd.Location = New System.Drawing.Point(212, 22)
        Me.nudZoomOd.Maximum = New Decimal(New Integer() {22, 0, 0, 0})
        Me.nudZoomOd.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudZoomOd.Name = "nudZoomOd"
        Me.nudZoomOd.Size = New System.Drawing.Size(42, 20)
        Me.nudZoomOd.TabIndex = 3
        Me.nudZoomOd.Value = New Decimal(New Integer() {12, 0, 0, 0})
        '
        'chkPodzialKmz
        '
        Me.chkPodzialKmz.AutoSize = True
        Me.chkPodzialKmz.Location = New System.Drawing.Point(350, 24)
        Me.chkPodzialKmz.Name = "chkPodzialKmz"
        Me.chkPodzialKmz.Size = New System.Drawing.Size(161, 17)
        Me.chkPodzialKmz.TabIndex = 2
        Me.chkPodzialKmz.Text = "Podział KMZ na tomy (100 kafli)"
        Me.chkPodzialKmz.UseVisualStyleBackColor = True
        Me.chkPodzialKmz.Visible = False
        '
        'nudJakosc
        '
        Me.nudJakosc.Location = New System.Drawing.Point(92, 22)
        Me.nudJakosc.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudJakosc.Name = "nudJakosc"
        Me.nudJakosc.Size = New System.Drawing.Size(46, 20)
        Me.nudJakosc.TabIndex = 1
        Me.nudJakosc.Value = New Decimal(New Integer() {75, 0, 0, 0})
        '
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Location = New System.Drawing.Point(12, 25)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(73, 13)
        Me.lblJakosc.TabIndex = 0
        Me.lblJakosc.Text = "Jakość JPEG:"
        '
        'prgPostep
        '
        Me.prgPostep.Location = New System.Drawing.Point(12, 330)
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(530, 20)
        Me.prgPostep.TabIndex = 6
        '
        'lblStatus
        '
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 355)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(530, 20)
        Me.lblStatus.TabIndex = 7
        Me.lblStatus.Text = "Wskaż plik GeoTIFF do konwersji."
        '
        'btnKonwertuj
        '
        Me.btnKonwertuj.Enabled = False
        Me.btnKonwertuj.Location = New System.Drawing.Point(232, 385)
        Me.btnKonwertuj.Name = "btnKonwertuj"
        Me.btnKonwertuj.Size = New System.Drawing.Size(100, 28)
        Me.btnKonwertuj.TabIndex = 8
        Me.btnKonwertuj.Text = "Konwertuj"
        Me.btnKonwertuj.UseVisualStyleBackColor = True
        '
        'btnPokazWFolderze
        '
        Me.btnPokazWFolderze.Enabled = False
        Me.btnPokazWFolderze.Location = New System.Drawing.Point(338, 385)
        Me.btnPokazWFolderze.Name = "btnPokazWFolderze"
        Me.btnPokazWFolderze.Size = New System.Drawing.Size(110, 28)
        Me.btnPokazWFolderze.TabIndex = 9
        Me.btnPokazWFolderze.Text = "Pokaż w folderze"
        Me.btnPokazWFolderze.UseVisualStyleBackColor = True
        '
        'btnZamknij
        '
        Me.btnZamknij.Location = New System.Drawing.Point(454, 385)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(88, 28)
        Me.btnZamknij.TabIndex = 10
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'dlgOtworz
        '
        Me.dlgOtworz.Filter = "Pliki GeoTIFF (*.tif;*.tiff;*.geotiff)|*.tif;*.tiff;*.geotiff|Wszystkie pliki (*." &
    "*)|*.*"
        Me.dlgOtworz.Title = "Wybierz plik GeoTIFF do konwersji"
        '
        'FormKonwerterGeoTiff
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(554, 425)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.btnPokazWFolderze)
        Me.Controls.Add(Me.btnKonwertuj)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.prgPostep)
        Me.Controls.Add(Me.grpOpcje)
        Me.Controls.Add(Me.grpFormat)
        Me.Controls.Add(Me.grpInfo)
        Me.Controls.Add(Me.btnWybierzTif)
        Me.Controls.Add(Me.txtPlikTif)
        Me.Controls.Add(Me.lblPlikTif)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormKonwerterGeoTiff"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Konwerter map GeoTIFF"
        Me.grpInfo.ResumeLayout(False)
        Me.grpInfo.PerformLayout()
        Me.grpFormat.ResumeLayout(False)
        Me.grpFormat.PerformLayout()
        Me.grpOpcje.ResumeLayout(False)
        Me.grpOpcje.PerformLayout()
        CType(Me.nudZoomDo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudZoomOd, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblPlikTif As System.Windows.Forms.Label
    Friend WithEvents txtPlikTif As System.Windows.Forms.TextBox
    Friend WithEvents btnWybierzTif As System.Windows.Forms.Button
    Friend WithEvents grpInfo As System.Windows.Forms.GroupBox
    Friend WithEvents lblInfoWymiary As System.Windows.Forms.Label
    Friend WithEvents lblInfoRozdzielczosc As System.Windows.Forms.Label
    Friend WithEvents lblInfoZasieg As System.Windows.Forms.Label
    Friend WithEvents lblCrsEtykieta As System.Windows.Forms.Label
    Friend WithEvents cmbUkladCrs As System.Windows.Forms.ComboBox
    Friend WithEvents lblCrsStatus As System.Windows.Forms.Label
    Friend WithEvents grpFormat As System.Windows.Forms.GroupBox
    Friend WithEvents rbMbtiles As System.Windows.Forms.RadioButton
    Friend WithEvents rbKmz As System.Windows.Forms.RadioButton
    Friend WithEvents rbJnx As System.Windows.Forms.RadioButton
    Friend WithEvents rbSqlitedb As System.Windows.Forms.RadioButton
    Friend WithEvents rbOruxMaps As System.Windows.Forms.RadioButton
    Friend WithEvents rbGeoPackage As System.Windows.Forms.RadioButton
    Friend WithEvents grpOpcje As System.Windows.Forms.GroupBox
    Friend WithEvents lblJakosc As System.Windows.Forms.Label
    Friend WithEvents nudJakosc As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkPodzialKmz As System.Windows.Forms.CheckBox
    Friend WithEvents nudZoomOd As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblZoomOd As System.Windows.Forms.Label
    Friend WithEvents nudZoomDo As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblZoomDo As System.Windows.Forms.Label
    Friend WithEvents prgPostep As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnKonwertuj As System.Windows.Forms.Button
    Friend WithEvents btnPokazWFolderze As System.Windows.Forms.Button
    Friend WithEvents btnZamknij As System.Windows.Forms.Button
    Friend WithEvents dlgOtworz As System.Windows.Forms.OpenFileDialog
End Class