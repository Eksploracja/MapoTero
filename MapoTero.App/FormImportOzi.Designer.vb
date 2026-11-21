<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormImportOzi
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
        Me.lblPlikMap = New System.Windows.Forms.Label()
        Me.txtPlikMap = New System.Windows.Forms.TextBox()
        Me.btnWybierzMap = New System.Windows.Forms.Button()
        Me.btnWybierzFolder = New System.Windows.Forms.Button()
        Me.lblPlikObrazu = New System.Windows.Forms.Label()
        Me.txtPlikObrazu = New System.Windows.Forms.TextBox()
        Me.btnWybierzObraz = New System.Windows.Forms.Button()
        Me.grpInfo = New System.Windows.Forms.GroupBox()
        Me.lblInfoZasieg = New System.Windows.Forms.Label()
        Me.lblInfoPunkty = New System.Windows.Forms.Label()
        Me.lblInfoWymiary = New System.Windows.Forms.Label()
        Me.lblInfoDatum = New System.Windows.Forms.Label()
        Me.lblInfoTytul = New System.Windows.Forms.Label()
        Me.grpFormat = New System.Windows.Forms.GroupBox()
        Me.rbMbtiles = New System.Windows.Forms.RadioButton()
        Me.rbSqlitedb = New System.Windows.Forms.RadioButton()
        Me.rbOruxMaps = New System.Windows.Forms.RadioButton()
        Me.chkKorekta = New System.Windows.Forms.CheckBox()
        Me.btnKorekta = New System.Windows.Forms.Button()
        Me.rbKmz = New System.Windows.Forms.RadioButton()
        Me.rbJnx = New System.Windows.Forms.RadioButton()
        Me.grpOpcje = New System.Windows.Forms.GroupBox()
        Me.chkPodzialKmz = New System.Windows.Forms.CheckBox()
        Me.nudJakosc = New System.Windows.Forms.NumericUpDown()
        Me.lblJakosc = New System.Windows.Forms.Label()
        Me.nudKolejnosc = New System.Windows.Forms.NumericUpDown()
        Me.lblKolejnosc = New System.Windows.Forms.Label()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnKonwertuj = New System.Windows.Forms.Button()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgWybierzMap = New System.Windows.Forms.OpenFileDialog()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.dlgFolderWyjsciowy = New System.Windows.Forms.FolderBrowserDialog()
        Me.dlgWybierzObraz = New System.Windows.Forms.OpenFileDialog()
        Me.dlgZapisz = New System.Windows.Forms.SaveFileDialog()
        Me.grpInfo.SuspendLayout()
        Me.grpFormat.SuspendLayout()
        Me.grpOpcje.SuspendLayout()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudKolejnosc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblPlikMap
        '
        Me.lblPlikMap.AutoSize = True
        Me.lblPlikMap.Location = New System.Drawing.Point(12, 12)
        Me.lblPlikMap.Name = "lblPlikMap"
        Me.lblPlikMap.Size = New System.Drawing.Size(200, 13)
        Me.lblPlikMap.TabIndex = 0
        Me.lblPlikMap.Text = "Plik kalibracji OziExplorer (.map):"
        '
        'txtPlikMap
        '
        Me.txtPlikMap.Location = New System.Drawing.Point(15, 28)
        Me.txtPlikMap.Name = "txtPlikMap"
        Me.txtPlikMap.ReadOnly = True
        Me.txtPlikMap.Size = New System.Drawing.Size(356, 20)
        Me.txtPlikMap.TabIndex = 1
        '
        'btnWybierzMap
        '
        Me.btnWybierzMap.Location = New System.Drawing.Point(377, 26)
        Me.btnWybierzMap.Name = "btnWybierzMap"
        Me.btnWybierzMap.Size = New System.Drawing.Size(76, 24)
        Me.btnWybierzMap.TabIndex = 2
        Me.btnWybierzMap.Text = "Plik(i)..."
        Me.btnWybierzMap.UseVisualStyleBackColor = True
        '
        'btnWybierzFolder
        '
        Me.btnWybierzFolder.Location = New System.Drawing.Point(457, 26)
        Me.btnWybierzFolder.Name = "btnWybierzFolder"
        Me.btnWybierzFolder.Size = New System.Drawing.Size(79, 24)
        Me.btnWybierzFolder.TabIndex = 3
        Me.btnWybierzFolder.Text = "Folder..."
        Me.btnWybierzFolder.UseVisualStyleBackColor = True
        '
        'lblPlikObrazu
        '
        Me.lblPlikObrazu.AutoSize = True
        Me.lblPlikObrazu.Location = New System.Drawing.Point(12, 57)
        Me.lblPlikObrazu.Name = "lblPlikObrazu"
        Me.lblPlikObrazu.Size = New System.Drawing.Size(175, 13)
        Me.lblPlikObrazu.TabIndex = 3
        Me.lblPlikObrazu.Text = "Plik obrazu (skan mapy JPG, PNG):"
        '
        'txtPlikObrazu
        '
        Me.txtPlikObrazu.Location = New System.Drawing.Point(15, 73)
        Me.txtPlikObrazu.Name = "txtPlikObrazu"
        Me.txtPlikObrazu.ReadOnly = True
        Me.txtPlikObrazu.Size = New System.Drawing.Size(430, 20)
        Me.txtPlikObrazu.TabIndex = 4
        '
        'btnWybierzObraz
        '
        Me.btnWybierzObraz.Location = New System.Drawing.Point(451, 71)
        Me.btnWybierzObraz.Name = "btnWybierzObraz"
        Me.btnWybierzObraz.Size = New System.Drawing.Size(85, 24)
        Me.btnWybierzObraz.TabIndex = 5
        Me.btnWybierzObraz.Text = "Przeglądaj..."
        Me.btnWybierzObraz.UseVisualStyleBackColor = True
        '
        'grpInfo
        '
        Me.grpInfo.Controls.Add(Me.lblInfoZasieg)
        Me.grpInfo.Controls.Add(Me.lblInfoPunkty)
        Me.grpInfo.Controls.Add(Me.lblInfoWymiary)
        Me.grpInfo.Controls.Add(Me.lblInfoDatum)
        Me.grpInfo.Controls.Add(Me.lblInfoTytul)
        Me.grpInfo.Location = New System.Drawing.Point(15, 105)
        Me.grpInfo.Name = "grpInfo"
        Me.grpInfo.Size = New System.Drawing.Size(521, 115)
        Me.grpInfo.TabIndex = 6
        Me.grpInfo.TabStop = False
        Me.grpInfo.Text = "Informacje o kalibracji i georeferencji"
        '
        'lblInfoZasieg
        '
        Me.lblInfoZasieg.AutoSize = True
        Me.lblInfoZasieg.Location = New System.Drawing.Point(10, 92)
        Me.lblInfoZasieg.Name = "lblInfoZasieg"
        Me.lblInfoZasieg.Size = New System.Drawing.Size(42, 13)
        Me.lblInfoZasieg.TabIndex = 4
        Me.lblInfoZasieg.Text = "Zasięg:"
        '
        'lblInfoPunkty
        '
        Me.lblInfoPunkty.AutoSize = True
        Me.lblInfoPunkty.Location = New System.Drawing.Point(10, 74)
        Me.lblInfoPunkty.Name = "lblInfoPunkty"
        Me.lblInfoPunkty.Size = New System.Drawing.Size(43, 13)
        Me.lblInfoPunkty.TabIndex = 3
        Me.lblInfoPunkty.Text = "Punkty:"
        '
        'lblInfoWymiary
        '
        Me.lblInfoWymiary.AutoSize = True
        Me.lblInfoWymiary.Location = New System.Drawing.Point(10, 56)
        Me.lblInfoWymiary.Name = "lblInfoWymiary"
        Me.lblInfoWymiary.Size = New System.Drawing.Size(49, 13)
        Me.lblInfoWymiary.TabIndex = 2
        Me.lblInfoWymiary.Text = "Wymiary:"
        '
        'lblInfoDatum
        '
        Me.lblInfoDatum.AutoSize = True
        Me.lblInfoDatum.Location = New System.Drawing.Point(10, 38)
        Me.lblInfoDatum.Name = "lblInfoDatum"
        Me.lblInfoDatum.Size = New System.Drawing.Size(41, 13)
        Me.lblInfoDatum.TabIndex = 1
        Me.lblInfoDatum.Text = "Datum:"
        '
        'lblInfoTytul
        '
        Me.lblInfoTytul.AutoSize = True
        Me.lblInfoTytul.Location = New System.Drawing.Point(10, 20)
        Me.lblInfoTytul.Name = "lblInfoTytul"
        Me.lblInfoTytul.Size = New System.Drawing.Size(35, 13)
        Me.lblInfoTytul.TabIndex = 0
        Me.lblInfoTytul.Text = "Tytuł:"
        '
        'grpFormat
        '
        Me.grpFormat.Controls.Add(Me.rbOruxMaps)
        Me.grpFormat.Controls.Add(Me.rbSqlitedb)
        Me.grpFormat.Controls.Add(Me.rbMbtiles)
        Me.grpFormat.Controls.Add(Me.rbKmz)
        Me.grpFormat.Controls.Add(Me.rbJnx)
        Me.grpFormat.Location = New System.Drawing.Point(15, 226)
        Me.grpFormat.Name = "grpFormat"
        Me.grpFormat.Size = New System.Drawing.Size(521, 130)
        Me.grpFormat.TabIndex = 7
        Me.grpFormat.TabStop = False
        Me.grpFormat.Text = "Docelowy format mapy"
        '
        'rbMbtiles
        '
        Me.rbMbtiles.AutoSize = True
        Me.rbMbtiles.Location = New System.Drawing.Point(13, 62)
        Me.rbMbtiles.Name = "rbMbtiles"
        Me.rbMbtiles.Size = New System.Drawing.Size(401, 17)
        Me.rbMbtiles.TabIndex = 2
        Me.rbMbtiles.Text = "Kafle MBTiles (SQLite) - dla aplikacji mobilnych (Locus Map, OsmAnd, QField)"
        Me.rbMbtiles.UseVisualStyleBackColor = True
        '
        'rbSqlitedb
        '
        Me.rbSqlitedb.AutoSize = True
        Me.rbSqlitedb.Location = New System.Drawing.Point(13, 84)
        Me.rbSqlitedb.Name = "rbSqlitedb"
        Me.rbSqlitedb.Size = New System.Drawing.Size(330, 17)
        Me.rbSqlitedb.TabIndex = 3
        Me.rbSqlitedb.Text = "SQLiteDB - OsmAnd, Locus Map (format RMaps, kafle Web Mercator)"
        Me.rbSqlitedb.UseVisualStyleBackColor = True
        '
        'rbOruxMaps
        '
        Me.rbOruxMaps.AutoSize = True
        Me.rbOruxMaps.Location = New System.Drawing.Point(13, 106)
        Me.rbOruxMaps.Name = "rbOruxMaps"
        Me.rbOruxMaps.Size = New System.Drawing.Size(420, 17)
        Me.rbOruxMaps.TabIndex = 4
        Me.rbOruxMaps.Text = "OruxMaps - mapa offline (folder z bazą OruxMapsImages.db i kalibracją .otrk2.xml)"
        Me.rbOruxMaps.UseVisualStyleBackColor = True
        '
        'chkKorekta
        '
        Me.chkKorekta.AutoSize = True
        Me.chkKorekta.Location = New System.Drawing.Point(13, 54)
        Me.chkKorekta.Name = "chkKorekta"
        Me.chkKorekta.Size = New System.Drawing.Size(98, 17)
        Me.chkKorekta.TabIndex = 5
        Me.chkKorekta.Text = "Korekta obrazu"
        Me.chkKorekta.UseVisualStyleBackColor = True
        '
        'btnKorekta
        '
        Me.btnKorekta.Location = New System.Drawing.Point(140, 49)
        Me.btnKorekta.Name = "btnKorekta"
        Me.btnKorekta.Size = New System.Drawing.Size(165, 26)
        Me.btnKorekta.TabIndex = 6
        Me.btnKorekta.Text = "Korekta obrazu..."
        Me.btnKorekta.UseVisualStyleBackColor = True
        '
        'rbKmz
        '
        Me.rbKmz.AutoSize = True
        Me.rbKmz.Location = New System.Drawing.Point(13, 40)
        Me.rbKmz.Name = "rbKmz"
        Me.rbKmz.Size = New System.Drawing.Size(447, 17)
        Me.rbKmz.TabIndex = 1
        Me.rbKmz.Text = "Garmin Custom Maps (KMZ) - dla wszystkich odbiorników Garmin (z opcją podziału na tomy)"
        Me.rbKmz.UseVisualStyleBackColor = True
        '
        'rbJnx
        '
        Me.rbJnx.AutoSize = True
        Me.rbJnx.Checked = True
        Me.rbJnx.Location = New System.Drawing.Point(13, 19)
        Me.rbJnx.Name = "rbJnx"
        Me.rbJnx.Size = New System.Drawing.Size(477, 17)
        Me.rbJnx.TabIndex = 0
        Me.rbJnx.TabStop = True
        Me.rbJnx.Text = "Garmin BirdsEye (JNX v4) - zalecane dla Garmin (bez limitu 100 kafli, płynne powiększanie)"
        Me.rbJnx.UseVisualStyleBackColor = True
        '
        'grpOpcje
        '
        Me.grpOpcje.Controls.Add(Me.btnKorekta)
        Me.grpOpcje.Controls.Add(Me.chkKorekta)
        Me.grpOpcje.Controls.Add(Me.chkPodzialKmz)
        Me.grpOpcje.Controls.Add(Me.nudJakosc)
        Me.grpOpcje.Controls.Add(Me.lblJakosc)
        Me.grpOpcje.Controls.Add(Me.nudKolejnosc)
        Me.grpOpcje.Controls.Add(Me.lblKolejnosc)
        Me.grpOpcje.Location = New System.Drawing.Point(15, 362)
        Me.grpOpcje.Name = "grpOpcje"
        Me.grpOpcje.Size = New System.Drawing.Size(521, 82)
        Me.grpOpcje.TabIndex = 8
        Me.grpOpcje.TabStop = False
        Me.grpOpcje.Text = "Opcje konwersji"
        '
        'chkPodzialKmz
        '
        Me.chkPodzialKmz.AutoSize = True
        Me.chkPodzialKmz.Checked = True
        Me.chkPodzialKmz.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkPodzialKmz.Enabled = False
        Me.chkPodzialKmz.Location = New System.Drawing.Point(308, 24)
        Me.chkPodzialKmz.Name = "chkPodzialKmz"
        Me.chkPodzialKmz.Size = New System.Drawing.Size(199, 17)
        Me.chkPodzialKmz.TabIndex = 4
        Me.chkPodzialKmz.Text = "Podział KMZ na tomy (max 100 kafli)"
        Me.chkPodzialKmz.UseVisualStyleBackColor = True
        '
        'nudJakosc
        '
        Me.nudJakosc.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudJakosc.Location = New System.Drawing.Point(232, 23)
        Me.nudJakosc.Maximum = New Decimal(New Integer() {95, 0, 0, 0})
        Me.nudJakosc.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.nudJakosc.Name = "nudJakosc"
        Me.nudJakosc.Size = New System.Drawing.Size(50, 20)
        Me.nudJakosc.TabIndex = 3
        Me.nudJakosc.Value = New Decimal(New Integer() {80, 0, 0, 0})
        '
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Location = New System.Drawing.Point(165, 25)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(61, 13)
        Me.lblJakosc.TabIndex = 2
        Me.lblJakosc.Text = "Jakość [%]:"
        '
        'nudKolejnosc
        '
        Me.nudKolejnosc.Increment = New Decimal(New Integer() {25, 0, 0, 0})
        Me.nudKolejnosc.Location = New System.Drawing.Point(95, 23)
        Me.nudKolejnosc.Maximum = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudKolejnosc.Name = "nudKolejnosc"
        Me.nudKolejnosc.Size = New System.Drawing.Size(50, 20)
        Me.nudKolejnosc.TabIndex = 1
        Me.nudKolejnosc.Value = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'lblKolejnosc
        '
        Me.lblKolejnosc.AutoSize = True
        Me.lblKolejnosc.Location = New System.Drawing.Point(10, 25)
        Me.lblKolejnosc.Name = "lblKolejnosc"
        Me.lblKolejnosc.Size = New System.Drawing.Size(79, 13)
        Me.lblKolejnosc.TabIndex = 0
        Me.lblKolejnosc.Text = "Kolejność (z-o):"
        '
        'prgPostep
        '
        Me.prgPostep.Location = New System.Drawing.Point(15, 454)
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(521, 16)
        Me.prgPostep.TabIndex = 9
        '
        'lblStatus
        '
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Location = New System.Drawing.Point(15, 475)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(521, 18)
        Me.lblStatus.TabIndex = 10
        Me.lblStatus.Text = "Wybierz plik kalibracji .map, aby rozpocząć."
        '
        'btnKonwertuj
        '
        Me.btnKonwertuj.Enabled = False
        Me.btnKonwertuj.Location = New System.Drawing.Point(265, 504)
        Me.btnKonwertuj.Name = "btnKonwertuj"
        Me.btnKonwertuj.Size = New System.Drawing.Size(180, 28)
        Me.btnKonwertuj.TabIndex = 12
        Me.btnKonwertuj.Text = "Konwertuj i zapisz..."
        Me.btnKonwertuj.UseVisualStyleBackColor = True
        '
        'btnZamknij
        '
        Me.btnZamknij.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnZamknij.Location = New System.Drawing.Point(451, 504)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(85, 28)
        Me.btnZamknij.TabIndex = 13
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'dlgWybierzMap
        '
        Me.dlgWybierzMap.Filter = "Pliki kalibracji OziExplorer (*.map)|*.map|Wszystkie pliki (*.*)|*.*"
        Me.dlgWybierzMap.Multiselect = True
        Me.dlgWybierzMap.Title = "Wybierz plik(i) kalibracji OziExplorer (.map)"
        '
        'dlgFolder
        '
        Me.dlgFolder.Description = "Wybierz folder zawierający pliki kalibracji OziExplorer (*.map)"
        '
        'dlgFolderWyjsciowy
        '
        Me.dlgFolderWyjsciowy.Description = "Wybierz folder docelowy dla przekonwertowanych map"
        '
        'dlgWybierzObraz
        '
        Me.dlgWybierzObraz.Filter = "Pliki obrazów (*.jpg;*.png;*.tif;*.bmp)|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.bmp|Wszystkie pliki (*.*)|*.*"
        Me.dlgWybierzObraz.Title = "Wskaż plik obrazu mapy"
        '
        'FormImportOzi
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnZamknij
        Me.ClientSize = New System.Drawing.Size(550, 544)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.btnKonwertuj)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.prgPostep)
        Me.Controls.Add(Me.grpOpcje)
        Me.Controls.Add(Me.grpFormat)
        Me.Controls.Add(Me.grpInfo)
        Me.Controls.Add(Me.btnWybierzObraz)
        Me.Controls.Add(Me.txtPlikObrazu)
        Me.Controls.Add(Me.lblPlikObrazu)
        Me.Controls.Add(Me.btnWybierzFolder)
        Me.Controls.Add(Me.btnWybierzMap)
        Me.Controls.Add(Me.txtPlikMap)
        Me.Controls.Add(Me.lblPlikMap)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormImportOzi"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Konwerter map OziExplorer (.map)"
        Me.grpInfo.ResumeLayout(False)
        Me.grpInfo.PerformLayout()
        Me.grpFormat.ResumeLayout(False)
        Me.grpFormat.PerformLayout()
        Me.grpOpcje.ResumeLayout(False)
        Me.grpOpcje.PerformLayout()
        CType(Me.nudJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudKolejnosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblPlikMap As System.Windows.Forms.Label
    Friend WithEvents txtPlikMap As System.Windows.Forms.TextBox
    Friend WithEvents btnWybierzMap As System.Windows.Forms.Button
    Friend WithEvents btnWybierzFolder As System.Windows.Forms.Button
    Friend WithEvents lblPlikObrazu As System.Windows.Forms.Label
    Friend WithEvents txtPlikObrazu As System.Windows.Forms.TextBox
    Friend WithEvents btnWybierzObraz As System.Windows.Forms.Button
    Friend WithEvents grpInfo As System.Windows.Forms.GroupBox
    Friend WithEvents lblInfoTytul As System.Windows.Forms.Label
    Friend WithEvents lblInfoDatum As System.Windows.Forms.Label
    Friend WithEvents lblInfoWymiary As System.Windows.Forms.Label
    Friend WithEvents lblInfoPunkty As System.Windows.Forms.Label
    Friend WithEvents lblInfoZasieg As System.Windows.Forms.Label
    Friend WithEvents grpFormat As System.Windows.Forms.GroupBox
    Friend WithEvents rbJnx As System.Windows.Forms.RadioButton
    Friend WithEvents rbKmz As System.Windows.Forms.RadioButton
    Friend WithEvents rbMbtiles As System.Windows.Forms.RadioButton
    Friend WithEvents rbSqlitedb As System.Windows.Forms.RadioButton
    Friend WithEvents rbOruxMaps As System.Windows.Forms.RadioButton
    Friend WithEvents chkKorekta As System.Windows.Forms.CheckBox
    Friend WithEvents btnKorekta As System.Windows.Forms.Button
    Friend WithEvents grpOpcje As System.Windows.Forms.GroupBox
    Friend WithEvents lblKolejnosc As System.Windows.Forms.Label
    Friend WithEvents nudKolejnosc As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblJakosc As System.Windows.Forms.Label
    Friend WithEvents nudJakosc As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkPodzialKmz As System.Windows.Forms.CheckBox
    Friend WithEvents prgPostep As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnKonwertuj As System.Windows.Forms.Button
    Friend WithEvents btnZamknij As System.Windows.Forms.Button
    Friend WithEvents dlgWybierzMap As System.Windows.Forms.OpenFileDialog
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents dlgFolderWyjsciowy As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents dlgWybierzObraz As System.Windows.Forms.OpenFileDialog
    Friend WithEvents dlgZapisz As System.Windows.Forms.SaveFileDialog
End Class
