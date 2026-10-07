'Copyright (C) <2015>  pajakt

    'This program is free software: you can redistribute it and/or modify
    'it under the terms of the GNU General Public License as published by
    'the Free Software Foundation, either version 3 of the License, or
    '(at your option) any later version.

   'This program is distributed in the hope that it will be useful,
    'but WITHOUT ANY WARRANTY; without even the implied warranty of
    'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    'GNU General Public License for more details.

    'You should have received a copy of the GNU General Public License
    'along with this program.  If not, see <http://www.gnu.org/licenses/>.

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.cmbFormat = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkMap = New System.Windows.Forms.CheckBox()
        Me.grpKalibracja = New System.Windows.Forms.GroupBox()
        Me.chkTab = New System.Windows.Forms.CheckBox()
        Me.chkKml = New System.Windows.Forms.CheckBox()
        Me.chkWorldFile = New System.Windows.Forms.CheckBox()
        Me.chkWldPoints = New System.Windows.Forms.CheckBox()
        Me.chkGmi = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtPrzerwa = New System.Windows.Forms.TextBox()
        Me.txtIloscProb = New System.Windows.Forms.TextBox()
        Me.btnZapisz = New System.Windows.Forms.Button()
        Me.chkZamienXY = New System.Windows.Forms.CheckBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtPrefiks = New System.Windows.Forms.TextBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.cmbNumeracja = New System.Windows.Forms.ComboBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.lblNazwaTB = New System.Windows.Forms.Label()
        Me.cmbNazwaTB = New System.Windows.Forms.ComboBox()
        Me.chkTrekBuddy = New System.Windows.Forms.CheckBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.chkPowyzejOstatniego = New System.Windows.Forms.CheckBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.chkKursorISrodek = New System.Windows.Forms.CheckBox()
        Me.chkZaznaczenieWgs = New System.Windows.Forms.CheckBox()
        Me.chkKursorWgs = New System.Windows.Forms.CheckBox()
        Me.chkEdycjaXY = New System.Windows.Forms.CheckBox()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.btnResetuj = New System.Windows.Forms.Button()
        Me.lblPodgladNazwyTB = New System.Windows.Forms.Label()
        Me.lblPodgladTB = New System.Windows.Forms.Label()
        Me.grpKalibracja.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.grpPobieranie = New System.Windows.Forms.GroupBox()
        Me.lblWatki = New System.Windows.Forms.Label()
        Me.nudWatki = New System.Windows.Forms.NumericUpDown()
        Me.grpPobieranie.SuspendLayout()
        CType(Me.nudWatki, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox5.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmbFormat
        '
        Me.cmbFormat.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar
        Me.cmbFormat.FormattingEnabled = True
        Me.cmbFormat.Items.AddRange(New Object() {"jpeg", "gif", "png", "png24", "png32", "png8", "svg+xml", "tiff"})
        Me.cmbFormat.Location = New System.Drawing.Point(15, 81)
        Me.cmbFormat.Name = "cmbFormat"
        Me.cmbFormat.Size = New System.Drawing.Size(152, 21)
        Me.cmbFormat.TabIndex = 0
        Me.cmbFormat.TabStop = False
        Me.cmbFormat.Tag = ""
        Me.ToolTip1.SetToolTip(Me.cmbFormat, "format pobieranych segmentów map")
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 65)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(114, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "rozszerzenie segmentu"
        '
        'chkMap
        '
        Me.chkMap.AccessibleDescription = ""
        Me.chkMap.AutoSize = True
        Me.chkMap.Location = New System.Drawing.Point(8, 20)
        Me.chkMap.Name = "chkMap"
        Me.chkMap.Size = New System.Drawing.Size(132, 17)
        Me.chkMap.TabIndex = 2
        Me.chkMap.Text = "plik .map / OziExplorer"
        Me.chkMap.UseVisualStyleBackColor = True
        '
        'grpKalibracja
        '
        Me.grpKalibracja.Controls.Add(Me.chkTab)
        Me.grpKalibracja.Controls.Add(Me.chkKml)
        Me.grpKalibracja.Controls.Add(Me.chkWorldFile)
        Me.grpKalibracja.Controls.Add(Me.chkWldPoints)
        Me.grpKalibracja.Controls.Add(Me.chkGmi)
        Me.grpKalibracja.Controls.Add(Me.chkMap)
        Me.grpKalibracja.Location = New System.Drawing.Point(11, 29)
        Me.grpKalibracja.Name = "grpKalibracja"
        Me.grpKalibracja.Size = New System.Drawing.Size(170, 120)
        Me.grpKalibracja.TabIndex = 3
        Me.grpKalibracja.TabStop = False
        Me.grpKalibracja.Text = "Kalibruj segmenty"
        Me.ToolTip1.SetToolTip(Me.grpKalibracja, "Opcjonalne zaopatrywanie każdego pobranego segmentu rastrowego w dodatkowy plik k" &
        "alibracyjny, umożliwiający wyświetlenie  segmentów w programach GIS/GPS z zachow" &
        "aniem ""georeferencji""")
        '
        'chkTab
        '
        Me.chkTab.AutoSize = True
        Me.chkTab.Location = New System.Drawing.Point(8, 67)
        Me.chkTab.Name = "chkTab"
        Me.chkTab.Size = New System.Drawing.Size(113, 17)
        Me.chkTab.TabIndex = 7
        Me.chkTab.Text = "plik .tab / MapInfo"
        Me.chkTab.UseVisualStyleBackColor = True
        '
        'chkKml
        '
        Me.chkKml.AutoSize = True
        Me.chkKml.Location = New System.Drawing.Point(8, 36)
        Me.chkKml.Name = "chkKml"
        Me.chkKml.Size = New System.Drawing.Size(134, 17)
        Me.chkKml.TabIndex = 6
        Me.chkKml.Text = "plik .kml / GoogleEarth"
        Me.chkKml.UseVisualStyleBackColor = True
        '
        'chkWorldFile
        '
        Me.chkWorldFile.AutoSize = True
        Me.chkWorldFile.Location = New System.Drawing.Point(8, 51)
        Me.chkWorldFile.Name = "chkWorldFile"
        Me.chkWorldFile.Size = New System.Drawing.Size(144, 17)
        Me.chkWorldFile.TabIndex = 5
        Me.chkWorldFile.Text = "plik .jpgw / QGIS, ArcGis"
        Me.ToolTip1.SetToolTip(Me.chkWorldFile, "plik .jpgw obsługuje jedynie segmenty pobierane w formacie jpeg")
        Me.chkWorldFile.UseVisualStyleBackColor = True
        '
        'chkWldPoints
        '
        Me.chkWldPoints.AutoSize = True
        Me.chkWldPoints.Location = New System.Drawing.Point(8, 82)
        Me.chkWldPoints.Name = "chkWldPoints"
        Me.chkWldPoints.Size = New System.Drawing.Size(100, 17)
        Me.chkWldPoints.TabIndex = 4
        Me.chkWldPoints.Text = "pliki .wld .points"
        Me.chkWldPoints.UseVisualStyleBackColor = True
        '
        'chkGmi
        '
        Me.chkGmi.AutoSize = True
        Me.chkGmi.Location = New System.Drawing.Point(8, 97)
        Me.chkGmi.Name = "chkGmi"
        Me.chkGmi.Size = New System.Drawing.Size(142, 17)
        Me.chkGmi.TabIndex = 3
        Me.chkGmi.Text = "plik .gmi / GPSTuner 5.x"
        Me.chkGmi.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.txtPrzerwa)
        Me.GroupBox2.Controls.Add(Me.txtIloscProb)
        Me.GroupBox2.Location = New System.Drawing.Point(190, 29)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(159, 68)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Ponowne próby pobrania"
        Me.ToolTip1.SetToolTip(Me.GroupBox2, "Domyślne ustawienia są optymalne. Określają one maksymalną liczbę zapytań wysyłan" &
        "ych do serwera WMS oraz odstępy czasu między nimi")
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(13, 50)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(70, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "co ile sekund"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 22)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Ilość prób"
        '
        'txtPrzerwa
        '
        Me.txtPrzerwa.Location = New System.Drawing.Point(98, 45)
        Me.txtPrzerwa.Name = "txtPrzerwa"
        Me.txtPrzerwa.Size = New System.Drawing.Size(38, 20)
        Me.txtPrzerwa.TabIndex = 1
        Me.txtPrzerwa.Text = "5"
        Me.txtPrzerwa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.ToolTip1.SetToolTip(Me.txtPrzerwa, "W jakich odstępach czasowych wysyłać kolejne zapytania do serwera WMS?")
        '
        'txtIloscProb
        '
        Me.txtIloscProb.Location = New System.Drawing.Point(98, 18)
        Me.txtIloscProb.Name = "txtIloscProb"
        Me.txtIloscProb.Size = New System.Drawing.Size(38, 20)
        Me.txtIloscProb.TabIndex = 0
        Me.txtIloscProb.Text = "3"
        Me.txtIloscProb.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.ToolTip1.SetToolTip(Me.txtIloscProb, "Ile razy wysyłać zapytanie do serwera WMS w przypadku napotkania problemu z pobra" &
        "niem segmentu?")
        '
        'btnZapisz
        '
        Me.btnZapisz.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZapisz.Location = New System.Drawing.Point(477, 304)
        Me.btnZapisz.Name = "btnZapisz"
        Me.btnZapisz.Size = New System.Drawing.Size(168, 39)
        Me.btnZapisz.TabIndex = 11
        Me.btnZapisz.Text = "Zapisz ustawienia"
        Me.btnZapisz.UseVisualStyleBackColor = True
        '
        'chkZamienXY
        '
        Me.chkZamienXY.AutoSize = True
        Me.chkZamienXY.Location = New System.Drawing.Point(9, 35)
        Me.chkZamienXY.Name = "chkZamienXY"
        Me.chkZamienXY.Size = New System.Drawing.Size(143, 17)
        Me.chkZamienXY.TabIndex = 12
        Me.chkZamienXY.Text = "zamień X i Y w zapytaniu"
        Me.ToolTip1.SetToolTip(Me.chkZamienXY, "Domyślny brak zaznaczenia jest optymalnym ustawieniem dla Geoportalu2. Opcja wyko" &
        "rzystywana jedynie w szczególnych sytuacjach nietypowych serwerów WMS.")
        Me.chkZamienXY.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(15, 20)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(152, 13)
        Me.Label4.TabIndex = 13
        Me.Label4.Text = "przedrostek nazwy segmentów"
        '
        'txtPrefiks
        '
        Me.txtPrefiks.Location = New System.Drawing.Point(15, 36)
        Me.txtPrefiks.Name = "txtPrefiks"
        Me.txtPrefiks.Size = New System.Drawing.Size(152, 20)
        Me.txtPrefiks.TabIndex = 14
        Me.ToolTip1.SetToolTip(Me.txtPrefiks, "opcjonalny wspólny przedrostek którym zostaną poprzedzone numery wszystkich pobie" &
        "ranych segmentów map")
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.GroupBox8)
        Me.GroupBox3.Controls.Add(Me.grpPobieranie)
        Me.GroupBox3.Controls.Add(Me.GroupBox6)
        Me.GroupBox3.Controls.Add(Me.GroupBox5)
        Me.GroupBox3.Controls.Add(Me.GroupBox2)
        Me.GroupBox3.Controls.Add(Me.grpKalibracja)
        Me.GroupBox3.Location = New System.Drawing.Point(287, 19)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(358, 286)
        Me.GroupBox3.TabIndex = 15
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Utawienia programu (zapisane w lastsettings.txt)"
        '
        'GroupBox8
        '
        Me.GroupBox8.Controls.Add(Me.Label8)
        Me.GroupBox8.Controls.Add(Me.chkZamienXY)
        Me.GroupBox8.Location = New System.Drawing.Point(190, 214)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.Size = New System.Drawing.Size(162, 62)
        Me.GroupBox8.TabIndex = 16
        Me.GroupBox8.TabStop = False
        Me.GroupBox8.Text = "Opcje zaawansowane dla"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(9, 17)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(144, 13)
        Me.Label8.TabIndex = 13
        Me.Label8.Text = "nietypowych serwerów WMS"
        '
        'grpPobieranie
        '
        Me.grpPobieranie.Controls.Add(Me.nudWatki)
        Me.grpPobieranie.Controls.Add(Me.lblWatki)
        Me.grpPobieranie.Location = New System.Drawing.Point(11, 231)
        Me.grpPobieranie.Name = "grpPobieranie"
        Me.grpPobieranie.Size = New System.Drawing.Size(170, 48)
        Me.grpPobieranie.TabIndex = 20
        Me.grpPobieranie.TabStop = False
        Me.grpPobieranie.Text = "Pobieranie"
        Me.ToolTip1.SetToolTip(Me.grpPobieranie, "Liczba segmentów pobieranych z serwera jednocześnie. Większa wartość przyspiesza pobieranie, ale bardziej obciąża serwer.")
        '
        'lblWatki
        '
        Me.lblWatki.AutoSize = True
        Me.lblWatki.Location = New System.Drawing.Point(6, 21)
        Me.lblWatki.Name = "lblWatki"
        Me.lblWatki.Size = New System.Drawing.Size(105, 13)
        Me.lblWatki.TabIndex = 0
        Me.lblWatki.Text = "równoczesne pobrania"
        '
        'nudWatki
        '
        Me.nudWatki.Location = New System.Drawing.Point(120, 18)
        Me.nudWatki.Maximum = New Decimal(New Integer() {16, 0, 0, 0})
        Me.nudWatki.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudWatki.Name = "nudWatki"
        Me.nudWatki.Size = New System.Drawing.Size(40, 20)
        Me.nudWatki.TabIndex = 1
        Me.nudWatki.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.nudWatki.Value = New Decimal(New Integer() {4, 0, 0, 0})
        Me.ToolTip1.SetToolTip(Me.nudWatki, "Liczba segmentów pobieranych z serwera jednocześnie (1-16).")
        '
        'GroupBox6
        '
        Me.GroupBox6.Controls.Add(Me.cmbNumeracja)
        Me.GroupBox6.Location = New System.Drawing.Point(11, 156)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(170, 69)
        Me.GroupBox6.TabIndex = 14
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "Zmiana domyślnej numeracji pobieranych segmentów"
        '
        'cmbNumeracja
        '
        Me.cmbNumeracja.FormattingEnabled = True
        Me.cmbNumeracja.Items.AddRange(New Object() {"01_02_03", "1_2_3", "NrWiersza_NrKolumny"})
        Me.cmbNumeracja.Location = New System.Drawing.Point(9, 32)
        Me.cmbNumeracja.Name = "cmbNumeracja"
        Me.cmbNumeracja.Size = New System.Drawing.Size(144, 21)
        Me.cmbNumeracja.TabIndex = 8
        Me.ToolTip1.SetToolTip(Me.cmbNumeracja, "Zmień sposób numerowania pobieranych segmentów")
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.lblNazwaTB)
        Me.GroupBox5.Controls.Add(Me.cmbNazwaTB)
        Me.GroupBox5.Controls.Add(Me.chkTrekBuddy)
        Me.GroupBox5.Location = New System.Drawing.Point(191, 100)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(158, 114)
        Me.GroupBox5.TabIndex = 13
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "TrekBuddy i Locus Map"
        '
        'lblNazwaTB
        '
        Me.lblNazwaTB.AutoSize = True
        Me.lblNazwaTB.Enabled = False
        Me.lblNazwaTB.Location = New System.Drawing.Point(8, 50)
        Me.lblNazwaTB.Name = "lblNazwaTB"
        Me.lblNazwaTB.Size = New System.Drawing.Size(148, 13)
        Me.lblNazwaTB.TabIndex = 4
        Me.lblNazwaTB.Text = "nazwa finalnej paczki TB i LM"
        '
        'cmbNazwaTB
        '
        Me.cmbNazwaTB.Enabled = False
        Me.cmbNazwaTB.FormattingEnabled = True
        Me.cmbNazwaTB.Items.AddRange(New Object() {"przedrostek nazwy segm.", "nazwa serwera WMS", "nazwa warstwy WMS", "serwer + warstwa WMS"})
        Me.cmbNazwaTB.Location = New System.Drawing.Point(9, 66)
        Me.cmbNazwaTB.Name = "cmbNazwaTB"
        Me.cmbNazwaTB.Size = New System.Drawing.Size(144, 21)
        Me.cmbNazwaTB.TabIndex = 9
        Me.cmbNazwaTB.Text = "przedrostek nazwy segm."
        Me.ToolTip1.SetToolTip(Me.cmbNazwaTB, "Zmień sposób numerowania pobieranych segmentów")
        '
        'chkTrekBuddy
        '
        Me.chkTrekBuddy.AutoSize = True
        Me.chkTrekBuddy.Location = New System.Drawing.Point(9, 21)
        Me.chkTrekBuddy.Name = "chkTrekBuddy"
        Me.chkTrekBuddy.Size = New System.Drawing.Size(135, 17)
        Me.chkTrekBuddy.TabIndex = 5
        Me.chkTrekBuddy.Text = "twórz mapę TB i LM.tar"
        Me.chkTrekBuddy.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.Label7)
        Me.GroupBox4.Controls.Add(Me.Label6)
        Me.GroupBox4.Controls.Add(Me.Label5)
        Me.GroupBox4.Controls.Add(Me.chkPowyzejOstatniego)
        Me.GroupBox4.Controls.Add(Me.txtPrefiks)
        Me.GroupBox4.Controls.Add(Me.Label4)
        Me.GroupBox4.Controls.Add(Me.Label1)
        Me.GroupBox4.Controls.Add(Me.cmbFormat)
        Me.GroupBox4.Location = New System.Drawing.Point(12, 19)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(269, 163)
        Me.GroupBox4.TabIndex = 16
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Ustawienia sesji (zapisane w conf.txt)"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(41, 139)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(122, 13)
        Me.Label7.TabIndex = 20
        Me.Label7.Text = "ostatni pobrany segment"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(41, 125)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(164, 13)
        Me.Label6.TabIndex = 19
        Me.Label6.Text = "segmenty o numerze wyższym niż"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(41, 111)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(194, 13)
        Me.Label5.TabIndex = 18
        Me.Label5.Text = "Przy powtórnym pobieraniu ściągaj tylko"
        '
        'chkPowyzejOstatniego
        '
        Me.chkPowyzejOstatniego.AutoSize = True
        Me.chkPowyzejOstatniego.Location = New System.Drawing.Point(17, 125)
        Me.chkPowyzejOstatniego.Name = "chkPowyzejOstatniego"
        Me.chkPowyzejOstatniego.Size = New System.Drawing.Size(15, 14)
        Me.chkPowyzejOstatniego.TabIndex = 17
        Me.ToolTip1.SetToolTip(Me.chkPowyzejOstatniego, "Opcjonalna funkcja wykorzystywana głównie w przypadku wznawiania niedokończonych " &
        "operacji pobierania map")
        Me.chkPowyzejOstatniego.UseVisualStyleBackColor = True
        '
        'chkKursorISrodek
        '
        Me.chkKursorISrodek.AutoSize = True
        Me.chkKursorISrodek.Location = New System.Drawing.Point(8, 21)
        Me.chkKursorISrodek.Name = "chkKursorISrodek"
        Me.chkKursorISrodek.Size = New System.Drawing.Size(235, 17)
        Me.chkKursorISrodek.TabIndex = 25
        Me.chkKursorISrodek.Text = "wyświetl współrzędne kursora i srodka mapy"
        Me.ToolTip1.SetToolTip(Me.chkKursorISrodek, "umożliwia wprowadzanie z klawiatury współrzędnych XY zasięgu pobieranej mapy ")
        Me.chkKursorISrodek.UseVisualStyleBackColor = True
        '
        'chkZaznaczenieWgs
        '
        Me.chkZaznaczenieWgs.AutoSize = True
        Me.chkZaznaczenieWgs.Location = New System.Drawing.Point(8, 61)
        Me.chkZaznaczenieWgs.Name = "chkZaznaczenieWgs"
        Me.chkZaznaczenieWgs.Size = New System.Drawing.Size(246, 17)
        Me.chkZaznaczenieWgs.TabIndex = 23
        Me.chkZaznaczenieWgs.Text = "wyświetl współrzędne  zaznaczenia w WGS84"
        Me.ToolTip1.SetToolTip(Me.chkZaznaczenieWgs, "umożliwia wprowadzanie z klawiatury współrzędnych XY zasięgu pobieranej mapy ")
        Me.chkZaznaczenieWgs.UseVisualStyleBackColor = True
        '
        'chkKursorWgs
        '
        Me.chkKursorWgs.AutoSize = True
        Me.chkKursorWgs.Location = New System.Drawing.Point(8, 41)
        Me.chkKursorWgs.Name = "chkKursorWgs"
        Me.chkKursorWgs.Size = New System.Drawing.Size(241, 17)
        Me.chkKursorWgs.TabIndex = 20
        Me.chkKursorWgs.Text = "wyświetl współrzędne kursora w ukł. WGS84"
        Me.ToolTip1.SetToolTip(Me.chkKursorWgs, "umożliwia wprowadzanie z klawiatury współrzędnych XY zasięgu pobieranej mapy ")
        Me.chkKursorWgs.UseVisualStyleBackColor = True
        '
        'chkEdycjaXY
        '
        Me.chkEdycjaXY.AutoSize = True
        Me.chkEdycjaXY.Location = New System.Drawing.Point(8, 83)
        Me.chkEdycjaXY.Name = "chkEdycjaXY"
        Me.chkEdycjaXY.Size = New System.Drawing.Size(195, 17)
        Me.chkEdycjaXY.TabIndex = 18
        Me.chkEdycjaXY.Text = "edytuj pola XY zasięgu zaznaczenia"
        Me.ToolTip1.SetToolTip(Me.chkEdycjaXY, "umożliwia wprowadzanie z klawiatury współrzędnych XY zasięgu pobieranej mapy ")
        Me.chkEdycjaXY.UseVisualStyleBackColor = True
        '
        'GroupBox7
        '
        Me.GroupBox7.Controls.Add(Me.chkKursorISrodek)
        Me.GroupBox7.Controls.Add(Me.chkZaznaczenieWgs)
        Me.GroupBox7.Controls.Add(Me.chkKursorWgs)
        Me.GroupBox7.Controls.Add(Me.chkEdycjaXY)
        Me.GroupBox7.Location = New System.Drawing.Point(12, 195)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Size = New System.Drawing.Size(269, 110)
        Me.GroupBox7.TabIndex = 19
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "Opcje widoku okna głównego"
        '
        'btnResetuj
        '
        Me.btnResetuj.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnResetuj.Image = Global.MapoTero.My.Resources.Resources.kosz
        Me.btnResetuj.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnResetuj.Location = New System.Drawing.Point(287, 308)
        Me.btnResetuj.Name = "btnResetuj"
        Me.btnResetuj.Size = New System.Drawing.Size(74, 32)
        Me.btnResetuj.TabIndex = 305
        Me.btnResetuj.Text = "Resetuj ustawienia"
        Me.btnResetuj.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnResetuj.UseVisualStyleBackColor = True
        '
        'lblPodgladNazwyTB
        '
        Me.lblPodgladNazwyTB.AutoSize = True
        Me.lblPodgladNazwyTB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPodgladNazwyTB.ForeColor = System.Drawing.Color.Green
        Me.lblPodgladNazwyTB.Location = New System.Drawing.Point(17, 326)
        Me.lblPodgladNazwyTB.Name = "lblPodgladNazwyTB"
        Me.lblPodgladNazwyTB.Size = New System.Drawing.Size(13, 13)
        Me.lblPodgladNazwyTB.TabIndex = 14
        Me.lblPodgladNazwyTB.Text = "_"
        Me.lblPodgladNazwyTB.Visible = False
        '
        'lblPodgladTB
        '
        Me.lblPodgladTB.AutoSize = True
        Me.lblPodgladTB.ForeColor = System.Drawing.Color.Green
        Me.lblPodgladTB.Location = New System.Drawing.Point(17, 310)
        Me.lblPodgladTB.Name = "lblPodgladTB"
        Me.lblPodgladTB.Size = New System.Drawing.Size(189, 13)
        Me.lblPodgladTB.TabIndex = 306
        Me.lblPodgladTB.Text = "Podgląd nazwy finalnej paczki TB i LM"
        Me.lblPodgladTB.Visible = False
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(656, 344)
        Me.Controls.Add(Me.lblPodgladTB)
        Me.Controls.Add(Me.lblPodgladNazwyTB)
        Me.Controls.Add(Me.btnResetuj)
        Me.Controls.Add(Me.GroupBox7)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.btnZapisz)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form2"
        Me.RightToLeftLayout = True
        Me.Text = "Ustawienia"
        Me.ToolTip1.SetToolTip(Me, "Przywróć domyślne ustawienia programu")
        Me.grpKalibracja.ResumeLayout(False)
        Me.grpKalibracja.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.grpPobieranie.ResumeLayout(False)
        Me.grpPobieranie.PerformLayout()
        CType(Me.nudWatki, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmbFormat As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents chkMap As System.Windows.Forms.CheckBox
    Friend WithEvents grpKalibracja As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtPrzerwa As System.Windows.Forms.TextBox
    Friend WithEvents txtIloscProb As System.Windows.Forms.TextBox
    Friend WithEvents chkGmi As System.Windows.Forms.CheckBox
    Friend WithEvents btnZapisz As System.Windows.Forms.Button
    Friend WithEvents chkZamienXY As System.Windows.Forms.CheckBox
    Friend WithEvents chkWldPoints As System.Windows.Forms.CheckBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtPrefiks As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents chkPowyzejOstatniego As System.Windows.Forms.CheckBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents chkTrekBuddy As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Friend WithEvents grpPobieranie As System.Windows.Forms.GroupBox
    Friend WithEvents lblWatki As System.Windows.Forms.Label
    Friend WithEvents nudWatki As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkWorldFile As System.Windows.Forms.CheckBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents cmbNumeracja As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBox8 As System.Windows.Forms.GroupBox
    Friend WithEvents chkKml As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox7 As System.Windows.Forms.GroupBox
    Friend WithEvents chkKursorISrodek As System.Windows.Forms.CheckBox
    Friend WithEvents chkZaznaczenieWgs As System.Windows.Forms.CheckBox
    Friend WithEvents chkKursorWgs As System.Windows.Forms.CheckBox
    Friend WithEvents chkEdycjaXY As System.Windows.Forms.CheckBox
    Friend WithEvents chkTab As System.Windows.Forms.CheckBox
    Friend WithEvents btnResetuj As System.Windows.Forms.Button
    Friend WithEvents Label8 As Label
    Friend WithEvents lblPodgladNazwyTB As Label
    Friend WithEvents lblNazwaTB As Label
    Friend WithEvents cmbNazwaTB As ComboBox
    Friend WithEvents lblPodgladTB As Label
End Class
