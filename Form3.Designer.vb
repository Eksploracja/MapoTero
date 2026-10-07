<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form3
    Inherits System.Windows.Forms.Form

    'Formularz zastępuje metodę dispose, aby wyczyścić listę składników.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wymagane przez Projektanta formularzy systemu Windows
    Private components As System.ComponentModel.IContainer

    'UWAGA: Następująca procedura jest wymagana przez Projektanta formularzy systemu Windows
    'Można to modyfikować, używając Projektanta formularzy systemu Windows.  
    'Nie należy modyfikować za pomocą edytora kodu.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form3))
        Me.txtFolder = New System.Windows.Forms.TextBox()
        Me.btnZmienFolder = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnScal = New System.Windows.Forms.Button()
        Me.lblFormatArkusza = New System.Windows.Forms.Label()
        Me.cmbFormatArkusza = New System.Windows.Forms.ComboBox()
        Me.prgScalanie = New System.Windows.Forms.ProgressBar()
        Me.grpParametry = New System.Windows.Forms.GroupBox()
        Me.cmbNumeracja = New System.Windows.Forms.ComboBox()
        Me.cmbFormat = New System.Windows.Forms.ComboBox()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.txtPrefiks = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.txtLiczbaWierszy = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.txtLiczbaKolumn = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtRozmiarPiksela = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtBokSegmentu = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txtXDol = New System.Windows.Forms.TextBox()
        Me.txtYLewy = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtYPrawy = New System.Windows.Forms.TextBox()
        Me.txtXGora = New System.Windows.Forms.TextBox()
        Me.chkRecznie = New System.Windows.Forms.CheckBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.txtKomunikaty = New System.Windows.Forms.RichTextBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.lblJakosc = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.trkJakosc = New System.Windows.Forms.TrackBar()
        Me.grpPlikiReferencyjne = New System.Windows.Forms.GroupBox()
        Me.chkTab = New System.Windows.Forms.CheckBox()
        Me.chkMap = New System.Windows.Forms.CheckBox()
        Me.chkKml = New System.Windows.Forms.CheckBox()
        Me.chkWorldFile = New System.Windows.Forms.CheckBox()
        Me.grpParametry.SuspendLayout()
        Me.groupBox2.SuspendLayout()
        CType(Me.trkJakosc, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpPlikiReferencyjne.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtFolder
        '
        Me.txtFolder.Location = New System.Drawing.Point(90, 46)
        Me.txtFolder.Name = "txtFolder"
        Me.txtFolder.Size = New System.Drawing.Size(521, 20)
        Me.txtFolder.TabIndex = 1
        '
        'btnZmienFolder
        '
        Me.btnZmienFolder.Location = New System.Drawing.Point(12, 46)
        Me.btnZmienFolder.Name = "btnZmienFolder"
        Me.btnZmienFolder.Size = New System.Drawing.Size(79, 20)
        Me.btnZmienFolder.TabIndex = 2
        Me.btnZmienFolder.Text = "zmień folder"
        Me.btnZmienFolder.UseVisualStyleBackColor = True
        '
        'lblFormatArkusza
        '
        Me.lblFormatArkusza.AutoSize = True
        Me.lblFormatArkusza.Location = New System.Drawing.Point(341, 75)
        Me.lblFormatArkusza.Name = "lblFormatArkusza"
        Me.lblFormatArkusza.Size = New System.Drawing.Size(132, 13)
        Me.lblFormatArkusza.TabIndex = 70
        Me.lblFormatArkusza.Text = "Format scalonego arkusza:"
        '
        'cmbFormatArkusza
        '
        Me.cmbFormatArkusza.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFormatArkusza.FormattingEnabled = True
        Me.cmbFormatArkusza.Items.AddRange(New Object() {"GeoTIFF (bezstratny)", "GeoTIFF (kompresja JPEG)", "JPEG", "PNG"})
        Me.cmbFormatArkusza.Location = New System.Drawing.Point(478, 71)
        Me.cmbFormatArkusza.Name = "cmbFormatArkusza"
        Me.cmbFormatArkusza.Size = New System.Drawing.Size(133, 21)
        Me.cmbFormatArkusza.TabIndex = 71
        '
        'prgScalanie
        '
        Me.prgScalanie.Location = New System.Drawing.Point(344, 430)
        Me.prgScalanie.Name = "prgScalanie"
        Me.prgScalanie.Size = New System.Drawing.Size(144, 18)
        Me.prgScalanie.TabIndex = 72
        '
        'btnScal
        '
        Me.btnScal.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnScal.Location = New System.Drawing.Point(494, 426)
        Me.btnScal.Name = "btnScal"
        Me.btnScal.Size = New System.Drawing.Size(117, 54)
        Me.btnScal.TabIndex = 26
        Me.btnScal.Text = "Złącz segmenty"
        Me.btnScal.UseVisualStyleBackColor = True
        '
        'grpParametry
        '
        Me.grpParametry.Controls.Add(Me.cmbNumeracja)
        Me.grpParametry.Controls.Add(Me.cmbFormat)
        Me.grpParametry.Controls.Add(Me.Label17)
        Me.grpParametry.Controls.Add(Me.Label16)
        Me.grpParametry.Controls.Add(Me.Label15)
        Me.grpParametry.Controls.Add(Me.Label13)
        Me.grpParametry.Controls.Add(Me.Label12)
        Me.grpParametry.Controls.Add(Me.txtPrefiks)
        Me.grpParametry.Controls.Add(Me.Label11)
        Me.grpParametry.Controls.Add(Me.Label10)
        Me.grpParametry.Controls.Add(Me.txtLiczbaWierszy)
        Me.grpParametry.Controls.Add(Me.Label9)
        Me.grpParametry.Controls.Add(Me.txtLiczbaKolumn)
        Me.grpParametry.Controls.Add(Me.Label8)
        Me.grpParametry.Controls.Add(Me.txtRozmiarPiksela)
        Me.grpParametry.Controls.Add(Me.Label7)
        Me.grpParametry.Controls.Add(Me.txtBokSegmentu)
        Me.grpParametry.Controls.Add(Me.Label4)
        Me.grpParametry.Controls.Add(Me.Label5)
        Me.grpParametry.Controls.Add(Me.Label6)
        Me.grpParametry.Controls.Add(Me.txtXDol)
        Me.grpParametry.Controls.Add(Me.txtYLewy)
        Me.grpParametry.Controls.Add(Me.Label3)
        Me.grpParametry.Controls.Add(Me.Label2)
        Me.grpParametry.Controls.Add(Me.Label1)
        Me.grpParametry.Controls.Add(Me.txtYPrawy)
        Me.grpParametry.Controls.Add(Me.txtXGora)
        Me.grpParametry.Location = New System.Drawing.Point(12, 101)
        Me.grpParametry.Name = "grpParametry"
        Me.grpParametry.Size = New System.Drawing.Size(281, 352)
        Me.grpParametry.TabIndex = 35
        Me.grpParametry.TabStop = False
        Me.grpParametry.Text = "Właściwości zbioru segmentów z pliku conf.txt"
        '
        'cmbNumeracja
        '
        Me.cmbNumeracja.FormattingEnabled = True
        Me.cmbNumeracja.Items.AddRange(New Object() {"01_02_03", "1_2_3", "NrWiersza_NrKolumny"})
        Me.cmbNumeracja.Location = New System.Drawing.Point(24, 302)
        Me.cmbNumeracja.Name = "cmbNumeracja"
        Me.cmbNumeracja.Size = New System.Drawing.Size(88, 21)
        Me.cmbNumeracja.TabIndex = 63
        '
        'cmbFormat
        '
        Me.cmbFormat.FormattingEnabled = True
        Me.cmbFormat.Items.AddRange(New Object() {"jpeg", "gif", "png", "png24", "png32", "png8", "svg+xml", "tiff"})
        Me.cmbFormat.Location = New System.Drawing.Point(24, 262)
        Me.cmbFormat.Name = "cmbFormat"
        Me.cmbFormat.Size = New System.Drawing.Size(88, 21)
        Me.cmbFormat.TabIndex = 62
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(64, 242)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(134, 13)
        Me.Label17.TabIndex = 61
        Me.Label17.Text = "Parametry pliku rastrowego"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(75, 179)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(103, 13)
        Me.Label16.TabIndex = 60
        Me.Label16.Text = "Parametry segmentu"
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(52, 159)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(46, 13)
        Me.Label15.TabIndex = 59
        Me.Label15.Text = "poziomo"
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(28, 325)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(70, 13)
        Me.Label13.TabIndex = 58
        Me.Label13.Text = "styl numeracji"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(145, 286)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(62, 13)
        Me.Label12.TabIndex = 56
        Me.Label12.Text = "przedrostek"
        '
        'txtPrefiks
        '
        Me.txtPrefiks.Location = New System.Drawing.Point(136, 263)
        Me.txtPrefiks.Name = "txtPrefiks"
        Me.txtPrefiks.Size = New System.Drawing.Size(88, 20)
        Me.txtPrefiks.TabIndex = 55
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(52, 286)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(36, 13)
        Me.Label11.TabIndex = 54
        Me.Label11.Text = "format"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(160, 159)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(47, 13)
        Me.Label10.TabIndex = 52
        Me.Label10.Text = "pionowo"
        '
        'txtLiczbaWierszy
        '
        Me.txtLiczbaWierszy.Location = New System.Drawing.Point(136, 136)
        Me.txtLiczbaWierszy.Name = "txtLiczbaWierszy"
        Me.txtLiczbaWierszy.Size = New System.Drawing.Size(88, 20)
        Me.txtLiczbaWierszy.TabIndex = 51
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(75, 122)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(95, 13)
        Me.Label9.TabIndex = 50
        Me.Label9.Text = "Liczba segmentów"
        '
        'txtLiczbaKolumn
        '
        Me.txtLiczbaKolumn.Location = New System.Drawing.Point(24, 136)
        Me.txtLiczbaKolumn.Name = "txtLiczbaKolumn"
        Me.txtLiczbaKolumn.Size = New System.Drawing.Size(88, 20)
        Me.txtLiczbaKolumn.TabIndex = 49
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(170, 218)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(28, 13)
        Me.Label8.TabIndex = 48
        Me.Label8.Text = "pixel"
        '
        'txtRozmiarPiksela
        '
        Me.txtRozmiarPiksela.Location = New System.Drawing.Point(136, 195)
        Me.txtRozmiarPiksela.Name = "txtRozmiarPiksela"
        Me.txtRozmiarPiksela.Size = New System.Drawing.Size(88, 20)
        Me.txtRozmiarPiksela.TabIndex = 47
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(52, 218)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(44, 13)
        Me.Label7.TabIndex = 46
        Me.Label7.Text = "dł boku"
        '
        'txtBokSegmentu
        '
        Me.txtBokSegmentu.Location = New System.Drawing.Point(24, 195)
        Me.txtBokSegmentu.Name = "txtBokSegmentu"
        Me.txtBokSegmentu.Size = New System.Drawing.Size(88, 20)
        Me.txtBokSegmentu.TabIndex = 45
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 93)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(12, 13)
        Me.Label4.TabIndex = 44
        Me.Label4.Text = "x"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(118, 93)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(12, 13)
        Me.Label5.TabIndex = 43
        Me.Label5.Text = "y"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(63, 23)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(107, 13)
        Me.Label6.TabIndex = 42
        Me.Label6.Text = "prawy gorny narożnik"
        '
        'txtXDol
        '
        Me.txtXDol.Location = New System.Drawing.Point(24, 90)
        Me.txtXDol.Name = "txtXDol"
        Me.txtXDol.Size = New System.Drawing.Size(88, 20)
        Me.txtXDol.TabIndex = 41
        '
        'txtYLewy
        '
        Me.txtYLewy.Location = New System.Drawing.Point(136, 90)
        Me.txtYLewy.Name = "txtYLewy"
        Me.txtYLewy.Size = New System.Drawing.Size(88, 20)
        Me.txtYLewy.TabIndex = 40
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(118, 42)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(12, 13)
        Me.Label3.TabIndex = 39
        Me.Label3.Text = "y"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 42)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(12, 13)
        Me.Label2.TabIndex = 38
        Me.Label2.Text = "x"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(67, 74)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(103, 13)
        Me.Label1.TabIndex = 37
        Me.Label1.Text = "Lewy dolny narożnik"
        '
        'txtYPrawy
        '
        Me.txtYPrawy.Location = New System.Drawing.Point(136, 39)
        Me.txtYPrawy.Name = "txtYPrawy"
        Me.txtYPrawy.Size = New System.Drawing.Size(88, 20)
        Me.txtYPrawy.TabIndex = 36
        '
        'txtXGora
        '
        Me.txtXGora.Location = New System.Drawing.Point(24, 39)
        Me.txtXGora.Name = "txtXGora"
        Me.txtXGora.Size = New System.Drawing.Size(88, 20)
        Me.txtXGora.TabIndex = 35
        '
        'chkRecznie
        '
        Me.chkRecznie.AutoSize = True
        Me.chkRecznie.Location = New System.Drawing.Point(12, 459)
        Me.chkRecznie.Name = "chkRecznie"
        Me.chkRecznie.Size = New System.Drawing.Size(291, 17)
        Me.chkRecznie.TabIndex = 63
        Me.chkRecznie.Text = "ręczne wprowadzanie parametrów w razie braku conf.txt"
        Me.chkRecznie.UseVisualStyleBackColor = True
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(9, 30)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(249, 13)
        Me.Label21.TabIndex = 69
        Me.Label21.Text = "Lokalizacja katalogu z segmentami i plikiem conf.txt"
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.txtKomunikaty.Location = New System.Drawing.Point(344, 319)
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.Size = New System.Drawing.Size(267, 96)
        Me.txtKomunikaty.TabIndex = 70
        Me.txtKomunikaty.Text = ""
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(341, 303)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(65, 13)
        Me.Label22.TabIndex = 71
        Me.Label22.Text = "Komunikaty:"
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.lblJakosc)
        Me.groupBox2.Controls.Add(Me.Label19)
        Me.groupBox2.Controls.Add(Me.Label18)
        Me.groupBox2.Controls.Add(Me.Label14)
        Me.groupBox2.Controls.Add(Me.trkJakosc)
        Me.groupBox2.Location = New System.Drawing.Point(344, 204)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(267, 84)
        Me.groupBox2.TabIndex = 72
        Me.groupBox2.TabStop = False
        '
        'lblJakosc
        '
        Me.lblJakosc.AutoSize = True
        Me.lblJakosc.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblJakosc.Location = New System.Drawing.Point(207, 14)
        Me.lblJakosc.Name = "lblJakosc"
        Me.lblJakosc.Size = New System.Drawing.Size(30, 13)
        Me.lblJakosc.TabIndex = 73
        Me.lblJakosc.Text = "75%"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(12, 67)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(32, 13)
        Me.Label19.TabIndex = 72
        Me.Label19.Text = "niska"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(214, 67)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(43, 13)
        Me.Label18.TabIndex = 71
        Me.Label18.Text = "wysoka"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(12, 14)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(191, 13)
        Me.Label14.TabIndex = 70
        Me.Label14.Text = "Jakość kompresji JPEG scalonego pliku:"
        '
        'trkJakosc
        '
        Me.trkJakosc.Location = New System.Drawing.Point(6, 38)
        Me.trkJakosc.Maximum = 100
        Me.trkJakosc.Name = "trkJakosc"
        Me.trkJakosc.Size = New System.Drawing.Size(255, 42)
        Me.trkJakosc.SmallChange = 5
        Me.trkJakosc.TabIndex = 69
        Me.trkJakosc.TickFrequency = 5
        Me.trkJakosc.Value = 75
        '
        'grpPlikiReferencyjne
        '
        Me.grpPlikiReferencyjne.Controls.Add(Me.chkTab)
        Me.grpPlikiReferencyjne.Controls.Add(Me.chkMap)
        Me.grpPlikiReferencyjne.Controls.Add(Me.chkKml)
        Me.grpPlikiReferencyjne.Controls.Add(Me.chkWorldFile)
        Me.grpPlikiReferencyjne.Location = New System.Drawing.Point(344, 101)
        Me.grpPlikiReferencyjne.Name = "grpPlikiReferencyjne"
        Me.grpPlikiReferencyjne.Size = New System.Drawing.Size(267, 101)
        Me.grpPlikiReferencyjne.TabIndex = 75
        Me.grpPlikiReferencyjne.TabStop = False
        Me.grpPlikiReferencyjne.Text = "Pliki referencyjne scalonego arkusza"
        '
        'chkTab
        '
        Me.chkTab.AutoSize = True
        Me.chkTab.Location = New System.Drawing.Point(6, 69)
        Me.chkTab.Name = "chkTab"
        Me.chkTab.Size = New System.Drawing.Size(199, 17)
        Me.chkTab.TabIndex = 78
        Me.chkTab.Text = "twórz plik referencyjny MapInfo (.tab)"
        Me.chkTab.UseVisualStyleBackColor = True
        '
        'chkMap
        '
        Me.chkMap.AutoSize = True
        Me.chkMap.Location = New System.Drawing.Point(6, 19)
        Me.chkMap.Name = "chkMap"
        Me.chkMap.Size = New System.Drawing.Size(218, 17)
        Me.chkMap.TabIndex = 77
        Me.chkMap.Text = "twórz plik referencyjny OziExplorer (.map)"
        Me.chkMap.UseVisualStyleBackColor = True
        '
        'chkKml
        '
        Me.chkKml.AutoSize = True
        Me.chkKml.Location = New System.Drawing.Point(6, 36)
        Me.chkKml.Name = "chkKml"
        Me.chkKml.Size = New System.Drawing.Size(220, 17)
        Me.chkKml.TabIndex = 76
        Me.chkKml.Text = "twórz plik referencyjny GoogleEarth (.kml)"
        Me.chkKml.UseVisualStyleBackColor = True
        '
        'chkWorldFile
        '
        Me.chkWorldFile.AutoSize = True
        Me.chkWorldFile.Location = New System.Drawing.Point(6, 52)
        Me.chkWorldFile.Name = "chkWorldFile"
        Me.chkWorldFile.Size = New System.Drawing.Size(230, 17)
        Me.chkWorldFile.TabIndex = 75
        Me.chkWorldFile.Text = "twórz plik referencyjny QGIS, ArcGis (.jpgw)"
        Me.chkWorldFile.UseVisualStyleBackColor = True
        '
        'Form3
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(621, 491)
        Me.Controls.Add(Me.grpPlikiReferencyjne)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.Label22)
        Me.Controls.Add(Me.txtKomunikaty)
        Me.Controls.Add(Me.Label21)
        Me.Controls.Add(Me.chkRecznie)
        Me.Controls.Add(Me.grpParametry)
        Me.Controls.Add(Me.btnScal)
        Me.Controls.Add(Me.lblFormatArkusza)
        Me.Controls.Add(Me.cmbFormatArkusza)
        Me.Controls.Add(Me.prgScalanie)
        Me.Controls.Add(Me.btnZmienFolder)
        Me.Controls.Add(Me.txtFolder)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form3"
        Me.Text = "Złączanie segmentów w jeden plik"
        Me.grpParametry.ResumeLayout(False)
        Me.grpParametry.PerformLayout()
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        CType(Me.trkJakosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpPlikiReferencyjne.ResumeLayout(False)
        Me.grpPlikiReferencyjne.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private groupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnZmienFolder As System.Windows.Forms.Button
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents btnScal As System.Windows.Forms.Button
    Friend WithEvents lblFormatArkusza As System.Windows.Forms.Label
    Friend WithEvents cmbFormatArkusza As System.Windows.Forms.ComboBox
    Friend WithEvents prgScalanie As System.Windows.Forms.ProgressBar
    Friend WithEvents grpParametry As System.Windows.Forms.GroupBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents txtPrefiks As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtLiczbaWierszy As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtLiczbaKolumn As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtRozmiarPiksela As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtBokSegmentu As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtXDol As System.Windows.Forms.TextBox
    Friend WithEvents txtYLewy As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtYPrawy As System.Windows.Forms.TextBox
    Friend WithEvents txtXGora As System.Windows.Forms.TextBox
    Friend WithEvents chkRecznie As System.Windows.Forms.CheckBox
    Friend WithEvents cmbFormat As System.Windows.Forms.ComboBox
    Friend WithEvents cmbNumeracja As System.Windows.Forms.ComboBox
    Friend WithEvents trkJakosc As System.Windows.Forms.TrackBar
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents lblJakosc As System.Windows.Forms.Label
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents txtKomunikaty As System.Windows.Forms.RichTextBox
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents grpPlikiReferencyjne As System.Windows.Forms.GroupBox
    Friend WithEvents chkKml As System.Windows.Forms.CheckBox
    Friend WithEvents chkWorldFile As System.Windows.Forms.CheckBox
    Friend WithEvents chkMap As System.Windows.Forms.CheckBox
    Friend WithEvents chkTab As System.Windows.Forms.CheckBox
End Class
