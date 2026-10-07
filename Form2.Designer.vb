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
        Me.grpSesja = New System.Windows.Forms.GroupBox()
        Me.lblFormat = New System.Windows.Forms.Label()
        Me.cmbFormat = New System.Windows.Forms.ComboBox()
        Me.lblPrefiks = New System.Windows.Forms.Label()
        Me.txtPrefiks = New System.Windows.Forms.TextBox()
        Me.lblNumeracja = New System.Windows.Forms.Label()
        Me.cmbNumeracja = New System.Windows.Forms.ComboBox()
        Me.chkPowyzejOstatniego = New System.Windows.Forms.CheckBox()
        Me.grpNowaSesja = New System.Windows.Forms.GroupBox()
        Me.lblUkladDomyslny = New System.Windows.Forms.Label()
        Me.cmbUkladDomyslny = New System.Windows.Forms.ComboBox()
        Me.grpWidok = New System.Windows.Forms.GroupBox()
        Me.chkKursorISrodek = New System.Windows.Forms.CheckBox()
        Me.chkKursorWgs = New System.Windows.Forms.CheckBox()
        Me.chkZaznaczenieWgs = New System.Windows.Forms.CheckBox()
        Me.chkEdycjaXY = New System.Windows.Forms.CheckBox()
        Me.grpKalibracja = New System.Windows.Forms.GroupBox()
        Me.chkMap = New System.Windows.Forms.CheckBox()
        Me.chkKml = New System.Windows.Forms.CheckBox()
        Me.chkWorldFile = New System.Windows.Forms.CheckBox()
        Me.chkTab = New System.Windows.Forms.CheckBox()
        Me.chkWldPoints = New System.Windows.Forms.CheckBox()
        Me.chkGmi = New System.Windows.Forms.CheckBox()
        Me.grpTrekBuddy = New System.Windows.Forms.GroupBox()
        Me.chkTrekBuddy = New System.Windows.Forms.CheckBox()
        Me.lblNazwaTB = New System.Windows.Forms.Label()
        Me.cmbNazwaTB = New System.Windows.Forms.ComboBox()
        Me.lblPodgladTB = New System.Windows.Forms.Label()
        Me.lblPodgladNazwyTB = New System.Windows.Forms.Label()
        Me.grpPobieranie = New System.Windows.Forms.GroupBox()
        Me.lblIloscProb = New System.Windows.Forms.Label()
        Me.nudIloscProb = New System.Windows.Forms.NumericUpDown()
        Me.lblPrzerwa = New System.Windows.Forms.Label()
        Me.nudPrzerwa = New System.Windows.Forms.NumericUpDown()
        Me.lblWatki = New System.Windows.Forms.Label()
        Me.nudWatki = New System.Windows.Forms.NumericUpDown()
        Me.lblLimitCzasu = New System.Windows.Forms.Label()
        Me.nudLimitCzasu = New System.Windows.Forms.NumericUpDown()
        Me.grpWmts = New System.Windows.Forms.GroupBox()
        Me.chkZachowajKafle = New System.Windows.Forms.CheckBox()
        Me.lblJakoscWmts = New System.Windows.Forms.Label()
        Me.nudJakoscWmts = New System.Windows.Forms.NumericUpDown()
        Me.grpZaawansowane = New System.Windows.Forms.GroupBox()
        Me.chkZamienXY = New System.Windows.Forms.CheckBox()
        Me.btnPrzywroc = New System.Windows.Forms.Button()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpSesja.SuspendLayout()
        Me.grpNowaSesja.SuspendLayout()
        Me.grpWidok.SuspendLayout()
        Me.grpKalibracja.SuspendLayout()
        Me.grpTrekBuddy.SuspendLayout()
        Me.grpPobieranie.SuspendLayout()
        Me.grpWmts.SuspendLayout()
        Me.grpZaawansowane.SuspendLayout()
        CType(Me.nudIloscProb, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudPrzerwa, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudWatki, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudLimitCzasu, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudJakoscWmts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grpSesja
        '
        Me.grpSesja.Controls.Add(Me.lblFormat)
        Me.grpSesja.Controls.Add(Me.cmbFormat)
        Me.grpSesja.Controls.Add(Me.lblPrefiks)
        Me.grpSesja.Controls.Add(Me.txtPrefiks)
        Me.grpSesja.Controls.Add(Me.lblNumeracja)
        Me.grpSesja.Controls.Add(Me.cmbNumeracja)
        Me.grpSesja.Controls.Add(Me.chkPowyzejOstatniego)
        Me.grpSesja.Location = New System.Drawing.Point(12, 12)
        Me.grpSesja.Name = "grpSesja"
        Me.grpSesja.Size = New System.Drawing.Size(240, 214)
        Me.grpSesja.TabIndex = 0
        Me.grpSesja.TabStop = False
        Me.grpSesja.Text = "Sesja pobierania (zapisywana w conf.txt)"
        '
        'lblFormat
        '
        Me.lblFormat.AutoSize = True
        Me.lblFormat.Location = New System.Drawing.Point(12, 22)
        Me.lblFormat.Name = "lblFormat"
        Me.lblFormat.TabIndex = 0
        Me.lblFormat.Text = "Format segmentów"
        '
        'cmbFormat
        '
        Me.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFormat.FormattingEnabled = True
        Me.cmbFormat.Location = New System.Drawing.Point(12, 38)
        Me.cmbFormat.Name = "cmbFormat"
        Me.cmbFormat.Size = New System.Drawing.Size(214, 21)
        Me.cmbFormat.TabIndex = 1
        Me.ToolTip1.SetToolTip(Me.cmbFormat, "Format obrazu segmentów (parametr FORMAT zapytania WMS). Dla usług WMTS - format zapisu segmentów składanych z kafli. Pliki kalibracji i scalanie obsługują wszystkie formaty z listy.")
        '
        'lblPrefiks
        '
        Me.lblPrefiks.AutoSize = True
        Me.lblPrefiks.Location = New System.Drawing.Point(12, 66)
        Me.lblPrefiks.Name = "lblPrefiks"
        Me.lblPrefiks.TabIndex = 2
        Me.lblPrefiks.Text = "Przedrostek nazw segmentów"
        '
        'txtPrefiks
        '
        Me.txtPrefiks.Location = New System.Drawing.Point(12, 82)
        Me.txtPrefiks.Name = "txtPrefiks"
        Me.txtPrefiks.Size = New System.Drawing.Size(214, 20)
        Me.txtPrefiks.TabIndex = 3
        Me.ToolTip1.SetToolTip(Me.txtPrefiks, "Wspólny przedrostek nazw wszystkich pobieranych segmentów, np. przedrostek _ daje pliki _01_02.jpg. Przedrostek jest też początkiem nazwy paczki TrekBuddy.")
        '
        'lblNumeracja
        '
        Me.lblNumeracja.AutoSize = True
        Me.lblNumeracja.Location = New System.Drawing.Point(12, 110)
        Me.lblNumeracja.Name = "lblNumeracja"
        Me.lblNumeracja.TabIndex = 4
        Me.lblNumeracja.Text = "Numeracja segmentów"
        '
        'cmbNumeracja
        '
        Me.cmbNumeracja.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbNumeracja.FormattingEnabled = True
        Me.cmbNumeracja.Location = New System.Drawing.Point(12, 126)
        Me.cmbNumeracja.Name = "cmbNumeracja"
        Me.cmbNumeracja.Size = New System.Drawing.Size(214, 21)
        Me.cmbNumeracja.TabIndex = 5
        Me.ToolTip1.SetToolTip(Me.cmbNumeracja, "Sposób numerowania segmentów w nazwach plików: NrWiersza_NrKolumny - numer wiersza i kolumny (np. _03_12), 01_02_03 - kolejne numery z zerem wiodącym (_01, _02 ...), 1_2_3 - kolejne numery bez zer (_1, _2 ...). W trybie TrekBuddy segmenty nazywane są wg położenia w pikselach.")
        '
        'chkPowyzejOstatniego
        '
        Me.chkPowyzejOstatniego.CheckAlign = System.Drawing.ContentAlignment.TopLeft
        Me.chkPowyzejOstatniego.TextAlign = System.Drawing.ContentAlignment.TopLeft
        Me.chkPowyzejOstatniego.UseVisualStyleBackColor = True
        Me.chkPowyzejOstatniego.Location = New System.Drawing.Point(12, 156)
        Me.chkPowyzejOstatniego.Name = "chkPowyzejOstatniego"
        Me.chkPowyzejOstatniego.Size = New System.Drawing.Size(214, 48)
        Me.chkPowyzejOstatniego.TabIndex = 6
        Me.chkPowyzejOstatniego.Text = "przy ponownym pobieraniu pobieraj tylko segmenty o numerze wyższym niż ostatni pobrany"
        Me.ToolTip1.SetToolTip(Me.chkPowyzejOstatniego, "Przy wznawianiu przerwanego pobierania pomija segmenty o numerach niższych lub równych numerowi ostatniego istniejącego pliku (zamiast sprawdzać każdy segment). Nie dotyczy trybu TrekBuddy.")
        '
        'grpNowaSesja
        '
        Me.grpNowaSesja.Controls.Add(Me.lblUkladDomyslny)
        Me.grpNowaSesja.Controls.Add(Me.cmbUkladDomyslny)
        Me.grpNowaSesja.Location = New System.Drawing.Point(12, 232)
        Me.grpNowaSesja.Name = "grpNowaSesja"
        Me.grpNowaSesja.Size = New System.Drawing.Size(240, 66)
        Me.grpNowaSesja.TabIndex = 1
        Me.grpNowaSesja.TabStop = False
        Me.grpNowaSesja.Text = "Nowa sesja"
        '
        'lblUkladDomyslny
        '
        Me.lblUkladDomyslny.AutoSize = True
        Me.lblUkladDomyslny.Location = New System.Drawing.Point(12, 18)
        Me.lblUkladDomyslny.Name = "lblUkladDomyslny"
        Me.lblUkladDomyslny.TabIndex = 0
        Me.lblUkladDomyslny.Text = "Domyślny układ współrzędnych"
        '
        'cmbUkladDomyslny
        '
        Me.cmbUkladDomyslny.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUkladDomyslny.FormattingEnabled = True
        Me.cmbUkladDomyslny.Location = New System.Drawing.Point(12, 34)
        Me.cmbUkladDomyslny.Name = "cmbUkladDomyslny"
        Me.cmbUkladDomyslny.Size = New System.Drawing.Size(214, 21)
        Me.cmbUkladDomyslny.TabIndex = 1
        Me.ToolTip1.SetToolTip(Me.cmbUkladDomyslny, "Układ współrzędnych ustawiany przy starcie programu, gdy w folderze pobierania nie ma zapisanej sesji (conf.txt). Układ bieżącej sesji zmienia się w oknie głównym.")
        '
        'grpWidok
        '
        Me.grpWidok.Controls.Add(Me.chkKursorISrodek)
        Me.grpWidok.Controls.Add(Me.chkKursorWgs)
        Me.grpWidok.Controls.Add(Me.chkZaznaczenieWgs)
        Me.grpWidok.Controls.Add(Me.chkEdycjaXY)
        Me.grpWidok.Location = New System.Drawing.Point(12, 304)
        Me.grpWidok.Name = "grpWidok"
        Me.grpWidok.Size = New System.Drawing.Size(240, 116)
        Me.grpWidok.TabIndex = 2
        Me.grpWidok.TabStop = False
        Me.grpWidok.Text = "Widok okna głównego"
        '
        'chkKursorISrodek
        '
        Me.chkKursorISrodek.AutoSize = True
        Me.chkKursorISrodek.UseVisualStyleBackColor = True
        Me.chkKursorISrodek.Location = New System.Drawing.Point(12, 20)
        Me.chkKursorISrodek.Name = "chkKursorISrodek"
        Me.chkKursorISrodek.Size = New System.Drawing.Size(200, 17)
        Me.chkKursorISrodek.TabIndex = 0
        Me.chkKursorISrodek.Text = "współrzędne kursora i środka mapy"
        Me.ToolTip1.SetToolTip(Me.chkKursorISrodek, "Pokazuje pod mapą współrzędne kursora myszy i środka mapy.")
        '
        'chkKursorWgs
        '
        Me.chkKursorWgs.AutoSize = True
        Me.chkKursorWgs.UseVisualStyleBackColor = True
        Me.chkKursorWgs.Location = New System.Drawing.Point(12, 43)
        Me.chkKursorWgs.Name = "chkKursorWgs"
        Me.chkKursorWgs.Size = New System.Drawing.Size(200, 17)
        Me.chkKursorWgs.TabIndex = 1
        Me.chkKursorWgs.Text = "współrzędne kursora w WGS84"
        Me.ToolTip1.SetToolTip(Me.chkKursorWgs, "Współrzędne kursora podawane w stopniach WGS84 zamiast w układzie pobierania.")
        '
        'chkZaznaczenieWgs
        '
        Me.chkZaznaczenieWgs.AutoSize = True
        Me.chkZaznaczenieWgs.UseVisualStyleBackColor = True
        Me.chkZaznaczenieWgs.Location = New System.Drawing.Point(12, 66)
        Me.chkZaznaczenieWgs.Name = "chkZaznaczenieWgs"
        Me.chkZaznaczenieWgs.Size = New System.Drawing.Size(200, 17)
        Me.chkZaznaczenieWgs.TabIndex = 2
        Me.chkZaznaczenieWgs.Text = "współrzędne zaznaczenia w WGS84"
        Me.ToolTip1.SetToolTip(Me.chkZaznaczenieWgs, "Pokazuje współrzędne granic zaznaczonego obszaru w stopniach WGS84.")
        '
        'chkEdycjaXY
        '
        Me.chkEdycjaXY.AutoSize = True
        Me.chkEdycjaXY.UseVisualStyleBackColor = True
        Me.chkEdycjaXY.Location = New System.Drawing.Point(12, 89)
        Me.chkEdycjaXY.Name = "chkEdycjaXY"
        Me.chkEdycjaXY.Size = New System.Drawing.Size(200, 17)
        Me.chkEdycjaXY.TabIndex = 3
        Me.chkEdycjaXY.Text = "wpisywanie zasięgu XY z klawiatury"
        Me.ToolTip1.SetToolTip(Me.chkEdycjaXY, "Umożliwia wpisywanie współrzędnych XY zasięgu pobierania z klawiatury; zaznaczanie zasięgu prawym przyciskiem myszy na mapie jest wtedy wyłączone.")
        '
        'grpKalibracja
        '
        Me.grpKalibracja.Controls.Add(Me.chkMap)
        Me.grpKalibracja.Controls.Add(Me.chkKml)
        Me.grpKalibracja.Controls.Add(Me.chkWorldFile)
        Me.grpKalibracja.Controls.Add(Me.chkTab)
        Me.grpKalibracja.Controls.Add(Me.chkWldPoints)
        Me.grpKalibracja.Controls.Add(Me.chkGmi)
        Me.grpKalibracja.Location = New System.Drawing.Point(264, 12)
        Me.grpKalibracja.Name = "grpKalibracja"
        Me.grpKalibracja.Size = New System.Drawing.Size(240, 166)
        Me.grpKalibracja.TabIndex = 3
        Me.grpKalibracja.TabStop = False
        Me.grpKalibracja.Text = "Pliki kalibracji segmentów"
        Me.ToolTip1.SetToolTip(Me.grpKalibracja, "Dodatkowe pliki kalibracji (georeferencji) tworzone dla każdego pobranego segmentu. W trybie TrekBuddy kalibracja zapisywana jest w pliku .map paczki, a te ustawienia nie są używane.")
        '
        'chkMap
        '
        Me.chkMap.AutoSize = True
        Me.chkMap.UseVisualStyleBackColor = True
        Me.chkMap.Location = New System.Drawing.Point(12, 20)
        Me.chkMap.Name = "chkMap"
        Me.chkMap.Size = New System.Drawing.Size(210, 17)
        Me.chkMap.TabIndex = 0
        Me.chkMap.Text = "plik .map / OziExplorer"
        Me.ToolTip1.SetToolTip(Me.chkMap, "OziExplorer oraz wiele aplikacji mobilnych (format kalibracji OziExplorer).")
        '
        'chkKml
        '
        Me.chkKml.AutoSize = True
        Me.chkKml.UseVisualStyleBackColor = True
        Me.chkKml.Location = New System.Drawing.Point(12, 43)
        Me.chkKml.Name = "chkKml"
        Me.chkKml.Size = New System.Drawing.Size(210, 17)
        Me.chkKml.TabIndex = 1
        Me.chkKml.Text = "plik .kml / Google Earth"
        Me.ToolTip1.SetToolTip(Me.chkKml, "Google Earth - dokładne narożniki obrazu (gx:LatLonQuad) i prostokąt z obrotem dla starszych programów.")
        '
        'chkWorldFile
        '
        Me.chkWorldFile.AutoSize = True
        Me.chkWorldFile.UseVisualStyleBackColor = True
        Me.chkWorldFile.Location = New System.Drawing.Point(12, 66)
        Me.chkWorldFile.Name = "chkWorldFile"
        Me.chkWorldFile.Size = New System.Drawing.Size(210, 17)
        Me.chkWorldFile.TabIndex = 2
        Me.chkWorldFile.Text = "world file i .prj / QGIS, ArcGIS"
        Me.ToolTip1.SetToolTip(Me.chkWorldFile, "Plik world file z rozszerzeniem zależnym od formatu (.jgw, .pngw, .tifw ...) oraz plik .prj z opisem układu współrzędnych - QGIS, ArcGIS, Global Mapper.")
        '
        'chkTab
        '
        Me.chkTab.AutoSize = True
        Me.chkTab.UseVisualStyleBackColor = True
        Me.chkTab.Location = New System.Drawing.Point(12, 89)
        Me.chkTab.Name = "chkTab"
        Me.chkTab.Size = New System.Drawing.Size(210, 17)
        Me.chkTab.TabIndex = 3
        Me.chkTab.Text = "plik .tab / MapInfo"
        Me.ToolTip1.SetToolTip(Me.chkTab, "MapInfo Professional.")
        '
        'chkWldPoints
        '
        Me.chkWldPoints.AutoSize = True
        Me.chkWldPoints.UseVisualStyleBackColor = True
        Me.chkWldPoints.Location = New System.Drawing.Point(12, 112)
        Me.chkWldPoints.Name = "chkWldPoints"
        Me.chkWldPoints.Size = New System.Drawing.Size(210, 17)
        Me.chkWldPoints.TabIndex = 4
        Me.chkWldPoints.Text = "pliki .wld i .points"
        Me.ToolTip1.SetToolTip(Me.chkWldPoints, "Plik .wld (world file z rozszerzeniem .wld) i plik .points z punktami dopasowania georeferencera QGIS.")
        '
        'chkGmi
        '
        Me.chkGmi.AutoSize = True
        Me.chkGmi.UseVisualStyleBackColor = True
        Me.chkGmi.Location = New System.Drawing.Point(12, 135)
        Me.chkGmi.Name = "chkGmi"
        Me.chkGmi.Size = New System.Drawing.Size(210, 17)
        Me.chkGmi.TabIndex = 5
        Me.chkGmi.Text = "plik .gmi / GPS Tuner 5.x"
        Me.ToolTip1.SetToolTip(Me.chkGmi, "GPS Tuner 5.x.")
        '
        'grpTrekBuddy
        '
        Me.grpTrekBuddy.Controls.Add(Me.chkTrekBuddy)
        Me.grpTrekBuddy.Controls.Add(Me.lblNazwaTB)
        Me.grpTrekBuddy.Controls.Add(Me.cmbNazwaTB)
        Me.grpTrekBuddy.Controls.Add(Me.lblPodgladTB)
        Me.grpTrekBuddy.Controls.Add(Me.lblPodgladNazwyTB)
        Me.grpTrekBuddy.Location = New System.Drawing.Point(264, 184)
        Me.grpTrekBuddy.Name = "grpTrekBuddy"
        Me.grpTrekBuddy.Size = New System.Drawing.Size(240, 150)
        Me.grpTrekBuddy.TabIndex = 4
        Me.grpTrekBuddy.TabStop = False
        Me.grpTrekBuddy.Text = "TrekBuddy i Locus Map"
        '
        'chkTrekBuddy
        '
        Me.chkTrekBuddy.AutoSize = True
        Me.chkTrekBuddy.UseVisualStyleBackColor = True
        Me.chkTrekBuddy.Location = New System.Drawing.Point(12, 20)
        Me.chkTrekBuddy.Name = "chkTrekBuddy"
        Me.chkTrekBuddy.Size = New System.Drawing.Size(210, 17)
        Me.chkTrekBuddy.TabIndex = 0
        Me.chkTrekBuddy.Text = "twórz mapę TrekBuddy / Locus Map (.tar)"
        Me.ToolTip1.SetToolTip(Me.chkTrekBuddy, "Tworzy mapę TrekBuddy / Locus Map: segmenty 512 px nazwane wg położenia w pikselach, plik kalibracji .map i archiwum .tar. Po wyłączeniu przywracany jest poprzedni bok segmentu.")
        '
        'lblNazwaTB
        '
        Me.lblNazwaTB.AutoSize = True
        Me.lblNazwaTB.Location = New System.Drawing.Point(12, 46)
        Me.lblNazwaTB.Name = "lblNazwaTB"
        Me.lblNazwaTB.TabIndex = 1
        Me.lblNazwaTB.Text = "nazwa paczki (po przedrostku)"
        '
        'cmbNazwaTB
        '
        Me.cmbNazwaTB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbNazwaTB.FormattingEnabled = True
        Me.cmbNazwaTB.Location = New System.Drawing.Point(12, 62)
        Me.cmbNazwaTB.Name = "cmbNazwaTB"
        Me.cmbNazwaTB.Size = New System.Drawing.Size(214, 21)
        Me.cmbNazwaTB.TabIndex = 2
        Me.ToolTip1.SetToolTip(Me.cmbNazwaTB, "Co dopisać po przedrostku w nazwie paczki TrekBuddy / Locus Map.")
        '
        'lblPodgladTB
        '
        Me.lblPodgladTB.AutoSize = True
        Me.lblPodgladTB.Location = New System.Drawing.Point(12, 94)
        Me.lblPodgladTB.Name = "lblPodgladTB"
        Me.lblPodgladTB.TabIndex = 3
        Me.lblPodgladTB.Text = "podgląd nazwy paczki:"
        '
        'lblPodgladNazwyTB
        '
        Me.lblPodgladNazwyTB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPodgladNazwyTB.Location = New System.Drawing.Point(12, 110)
        Me.lblPodgladNazwyTB.Name = "lblPodgladNazwyTB"
        Me.lblPodgladNazwyTB.Size = New System.Drawing.Size(214, 30)
        Me.lblPodgladNazwyTB.TabIndex = 4
        Me.lblPodgladNazwyTB.Text = "_"
        '
        'grpPobieranie
        '
        Me.grpPobieranie.Controls.Add(Me.lblIloscProb)
        Me.grpPobieranie.Controls.Add(Me.nudIloscProb)
        Me.grpPobieranie.Controls.Add(Me.lblPrzerwa)
        Me.grpPobieranie.Controls.Add(Me.nudPrzerwa)
        Me.grpPobieranie.Controls.Add(Me.lblWatki)
        Me.grpPobieranie.Controls.Add(Me.nudWatki)
        Me.grpPobieranie.Controls.Add(Me.lblLimitCzasu)
        Me.grpPobieranie.Controls.Add(Me.nudLimitCzasu)
        Me.grpPobieranie.Location = New System.Drawing.Point(516, 12)
        Me.grpPobieranie.Name = "grpPobieranie"
        Me.grpPobieranie.Size = New System.Drawing.Size(252, 140)
        Me.grpPobieranie.TabIndex = 5
        Me.grpPobieranie.TabStop = False
        Me.grpPobieranie.Text = "Pobieranie"
        '
        'lblIloscProb
        '
        Me.lblIloscProb.AutoSize = True
        Me.lblIloscProb.Location = New System.Drawing.Point(12, 26)
        Me.lblIloscProb.Name = "lblIloscProb"
        Me.lblIloscProb.TabIndex = 0
        Me.lblIloscProb.Text = "Liczba prób pobrania"
        '
        'nudIloscProb
        '
        Me.nudIloscProb.Maximum = New Decimal(New Integer() {20, 0, 0, 0})
        Me.nudIloscProb.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudIloscProb.Location = New System.Drawing.Point(180, 24)
        Me.nudIloscProb.Name = "nudIloscProb"
        Me.nudIloscProb.Size = New System.Drawing.Size(58, 20)
        Me.nudIloscProb.TabIndex = 1
        Me.ToolTip1.SetToolTip(Me.nudIloscProb, "Ile razy ponawiać pobieranie segmentów, których nie udało się pobrać (np. z powodu przeciążenia serwera).")
        Me.nudIloscProb.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblPrzerwa
        '
        Me.lblPrzerwa.AutoSize = True
        Me.lblPrzerwa.Location = New System.Drawing.Point(12, 54)
        Me.lblPrzerwa.Name = "lblPrzerwa"
        Me.lblPrzerwa.TabIndex = 2
        Me.lblPrzerwa.Text = "Przerwa między próbami [s]"
        '
        'nudPrzerwa
        '
        Me.nudPrzerwa.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        Me.nudPrzerwa.Minimum = New Decimal(New Integer() {0, 0, 0, 0})
        Me.nudPrzerwa.Location = New System.Drawing.Point(180, 52)
        Me.nudPrzerwa.Name = "nudPrzerwa"
        Me.nudPrzerwa.Size = New System.Drawing.Size(58, 20)
        Me.nudPrzerwa.TabIndex = 3
        Me.ToolTip1.SetToolTip(Me.nudPrzerwa, "Przerwa przed kolejną próbą pobrania nieudanych segmentów.")
        Me.nudPrzerwa.Value = New Decimal(New Integer() {0, 0, 0, 0})
        '
        'lblWatki
        '
        Me.lblWatki.AutoSize = True
        Me.lblWatki.Location = New System.Drawing.Point(12, 82)
        Me.lblWatki.Name = "lblWatki"
        Me.lblWatki.TabIndex = 4
        Me.lblWatki.Text = "Równoczesne pobrania"
        '
        'nudWatki
        '
        Me.nudWatki.Maximum = New Decimal(New Integer() {16, 0, 0, 0})
        Me.nudWatki.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudWatki.Location = New System.Drawing.Point(180, 80)
        Me.nudWatki.Name = "nudWatki"
        Me.nudWatki.Size = New System.Drawing.Size(58, 20)
        Me.nudWatki.TabIndex = 5
        Me.ToolTip1.SetToolTip(Me.nudWatki, "Liczba segmentów pobieranych z serwera jednocześnie. Większa wartość przyspiesza pobieranie, ale bardziej obciąża serwer.")
        Me.nudWatki.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblLimitCzasu
        '
        Me.lblLimitCzasu.AutoSize = True
        Me.lblLimitCzasu.Location = New System.Drawing.Point(12, 110)
        Me.lblLimitCzasu.Name = "lblLimitCzasu"
        Me.lblLimitCzasu.TabIndex = 6
        Me.lblLimitCzasu.Text = "Limit czasu odpowiedzi [s]"
        '
        'nudLimitCzasu
        '
        Me.nudLimitCzasu.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudLimitCzasu.Maximum = New Decimal(New Integer() {600, 0, 0, 0})
        Me.nudLimitCzasu.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudLimitCzasu.Location = New System.Drawing.Point(180, 108)
        Me.nudLimitCzasu.Name = "nudLimitCzasu"
        Me.nudLimitCzasu.Size = New System.Drawing.Size(58, 20)
        Me.nudLimitCzasu.TabIndex = 7
        Me.ToolTip1.SetToolTip(Me.nudLimitCzasu, "Jak długo czekać na odpowiedź serwera na jedno zapytanie. Wolne serwery (np. mapy historyczne HGIS) mogą wymagać dłuższego czasu.")
        Me.nudLimitCzasu.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'grpWmts
        '
        Me.grpWmts.Controls.Add(Me.chkZachowajKafle)
        Me.grpWmts.Controls.Add(Me.lblJakoscWmts)
        Me.grpWmts.Controls.Add(Me.nudJakoscWmts)
        Me.grpWmts.Location = New System.Drawing.Point(516, 158)
        Me.grpWmts.Name = "grpWmts"
        Me.grpWmts.Size = New System.Drawing.Size(252, 80)
        Me.grpWmts.TabIndex = 6
        Me.grpWmts.TabStop = False
        Me.grpWmts.Text = "Usługi WMTS"
        '
        'chkZachowajKafle
        '
        Me.chkZachowajKafle.AutoSize = True
        Me.chkZachowajKafle.UseVisualStyleBackColor = True
        Me.chkZachowajKafle.Location = New System.Drawing.Point(12, 20)
        Me.chkZachowajKafle.Name = "chkZachowajKafle"
        Me.chkZachowajKafle.Size = New System.Drawing.Size(230, 17)
        Me.chkZachowajKafle.TabIndex = 0
        Me.chkZachowajKafle.Text = "zachowuj pobrane kafle w folderze sesji"
        Me.ToolTip1.SetToolTip(Me.chkZachowajKafle, "Pobrane kafle WMTS pozostają w podfolderze _kafle_wmts folderu sesji - ponowne pobranie tego samego obszaru (np. z innym bokiem segmentu) nie wymaga łączenia z serwerem. Kafle zajmują miejsce na dysku.")
        '
        'lblJakoscWmts
        '
        Me.lblJakoscWmts.AutoSize = True
        Me.lblJakoscWmts.Location = New System.Drawing.Point(12, 50)
        Me.lblJakoscWmts.Name = "lblJakoscWmts"
        Me.lblJakoscWmts.TabIndex = 1
        Me.lblJakoscWmts.Text = "Jakość JPEG segmentów"
        '
        'nudJakoscWmts
        '
        Me.nudJakoscWmts.Maximum = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudJakoscWmts.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.nudJakoscWmts.Location = New System.Drawing.Point(180, 48)
        Me.nudJakoscWmts.Name = "nudJakoscWmts"
        Me.nudJakoscWmts.Size = New System.Drawing.Size(58, 20)
        Me.nudJakoscWmts.TabIndex = 2
        Me.ToolTip1.SetToolTip(Me.nudJakoscWmts, "Jakość kompresji JPEG segmentów składanych z kafli WMTS (segmenty z serwerów WMS zapisywane są bez ponownej kompresji).")
        Me.nudJakoscWmts.Value = New Decimal(New Integer() {90, 0, 0, 0})
        '
        'grpZaawansowane
        '
        Me.grpZaawansowane.Controls.Add(Me.chkZamienXY)
        Me.grpZaawansowane.Location = New System.Drawing.Point(516, 244)
        Me.grpZaawansowane.Name = "grpZaawansowane"
        Me.grpZaawansowane.Size = New System.Drawing.Size(252, 50)
        Me.grpZaawansowane.TabIndex = 7
        Me.grpZaawansowane.TabStop = False
        Me.grpZaawansowane.Text = "Nietypowe serwery WMS"
        '
        'chkZamienXY
        '
        Me.chkZamienXY.AutoSize = True
        Me.chkZamienXY.UseVisualStyleBackColor = True
        Me.chkZamienXY.Location = New System.Drawing.Point(12, 20)
        Me.chkZamienXY.Name = "chkZamienXY"
        Me.chkZamienXY.Size = New System.Drawing.Size(230, 17)
        Me.chkZamienXY.TabIndex = 0
        Me.chkZamienXY.Text = "zamień kolejność X i Y w zapytaniu"
        Me.ToolTip1.SetToolTip(Me.chkZamienXY, "Zamienia kolejność współrzędnych w parametrze BBOX zapytania - dla serwerów niezgodnych ze standardem WMS 1.3.0. Nie wpływa na pliki kalibracji. Domyślnie wyłączone (właściwe dla Geoportalu).")
        '
        'btnPrzywroc
        '
        Me.btnPrzywroc.UseVisualStyleBackColor = True
        Me.btnPrzywroc.Location = New System.Drawing.Point(12, 432)
        Me.btnPrzywroc.Name = "btnPrzywroc"
        Me.btnPrzywroc.Size = New System.Drawing.Size(180, 28)
        Me.btnPrzywroc.TabIndex = 8
        Me.btnPrzywroc.Text = "Przywróć domyślne..."
        Me.ToolTip1.SetToolTip(Me.btnPrzywroc, "Przywraca domyślne ustawienia programu, a także widok mapy, zbiór map, bok segmentu, rozmiar piksela i układ współrzędnych w oknie głównym.")
        '
        'btnOK
        '
        Me.btnOK.UseVisualStyleBackColor = True
        Me.btnOK.Location = New System.Drawing.Point(582, 432)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(90, 28)
        Me.btnOK.TabIndex = 9
        Me.btnOK.Text = "OK"
        Me.ToolTip1.SetToolTip(Me.btnOK, "Zastosuj i zapisz ustawienia")
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.UseVisualStyleBackColor = True
        Me.btnAnuluj.Location = New System.Drawing.Point(678, 432)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(90, 28)
        Me.btnAnuluj.TabIndex = 10
        Me.btnAnuluj.Text = "Anuluj"
        Me.ToolTip1.SetToolTip(Me.btnAnuluj, "Zamknij okno bez zmiany ustawień")
        '
        'ToolTip1
        '
        Me.ToolTip1.AutoPopDelay = 20000
        Me.ToolTip1.InitialDelay = 500
        Me.ToolTip1.ReshowDelay = 100
        '
        'Form2
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(780, 472)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnPrzywroc)
        Me.Controls.Add(Me.grpZaawansowane)
        Me.Controls.Add(Me.grpWmts)
        Me.Controls.Add(Me.grpPobieranie)
        Me.Controls.Add(Me.grpTrekBuddy)
        Me.Controls.Add(Me.grpKalibracja)
        Me.Controls.Add(Me.grpWidok)
        Me.Controls.Add(Me.grpNowaSesja)
        Me.Controls.Add(Me.grpSesja)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form2"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Ustawienia"
        CType(Me.nudIloscProb, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudPrzerwa, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudWatki, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudLimitCzasu, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudJakoscWmts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpSesja.ResumeLayout(False)
        Me.grpSesja.PerformLayout()
        Me.grpNowaSesja.ResumeLayout(False)
        Me.grpNowaSesja.PerformLayout()
        Me.grpWidok.ResumeLayout(False)
        Me.grpWidok.PerformLayout()
        Me.grpKalibracja.ResumeLayout(False)
        Me.grpKalibracja.PerformLayout()
        Me.grpTrekBuddy.ResumeLayout(False)
        Me.grpTrekBuddy.PerformLayout()
        Me.grpPobieranie.ResumeLayout(False)
        Me.grpPobieranie.PerformLayout()
        Me.grpWmts.ResumeLayout(False)
        Me.grpWmts.PerformLayout()
        Me.grpZaawansowane.ResumeLayout(False)
        Me.grpZaawansowane.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpSesja As System.Windows.Forms.GroupBox
    Friend WithEvents lblFormat As System.Windows.Forms.Label
    Friend WithEvents cmbFormat As System.Windows.Forms.ComboBox
    Friend WithEvents lblPrefiks As System.Windows.Forms.Label
    Friend WithEvents txtPrefiks As System.Windows.Forms.TextBox
    Friend WithEvents lblNumeracja As System.Windows.Forms.Label
    Friend WithEvents cmbNumeracja As System.Windows.Forms.ComboBox
    Friend WithEvents chkPowyzejOstatniego As System.Windows.Forms.CheckBox
    Friend WithEvents grpNowaSesja As System.Windows.Forms.GroupBox
    Friend WithEvents lblUkladDomyslny As System.Windows.Forms.Label
    Friend WithEvents cmbUkladDomyslny As System.Windows.Forms.ComboBox
    Friend WithEvents grpWidok As System.Windows.Forms.GroupBox
    Friend WithEvents chkKursorISrodek As System.Windows.Forms.CheckBox
    Friend WithEvents chkKursorWgs As System.Windows.Forms.CheckBox
    Friend WithEvents chkZaznaczenieWgs As System.Windows.Forms.CheckBox
    Friend WithEvents chkEdycjaXY As System.Windows.Forms.CheckBox
    Friend WithEvents grpKalibracja As System.Windows.Forms.GroupBox
    Friend WithEvents chkMap As System.Windows.Forms.CheckBox
    Friend WithEvents chkKml As System.Windows.Forms.CheckBox
    Friend WithEvents chkWorldFile As System.Windows.Forms.CheckBox
    Friend WithEvents chkTab As System.Windows.Forms.CheckBox
    Friend WithEvents chkWldPoints As System.Windows.Forms.CheckBox
    Friend WithEvents chkGmi As System.Windows.Forms.CheckBox
    Friend WithEvents grpTrekBuddy As System.Windows.Forms.GroupBox
    Friend WithEvents chkTrekBuddy As System.Windows.Forms.CheckBox
    Friend WithEvents lblNazwaTB As System.Windows.Forms.Label
    Friend WithEvents cmbNazwaTB As System.Windows.Forms.ComboBox
    Friend WithEvents lblPodgladTB As System.Windows.Forms.Label
    Friend WithEvents lblPodgladNazwyTB As System.Windows.Forms.Label
    Friend WithEvents grpPobieranie As System.Windows.Forms.GroupBox
    Friend WithEvents lblIloscProb As System.Windows.Forms.Label
    Friend WithEvents nudIloscProb As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPrzerwa As System.Windows.Forms.Label
    Friend WithEvents nudPrzerwa As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblWatki As System.Windows.Forms.Label
    Friend WithEvents nudWatki As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblLimitCzasu As System.Windows.Forms.Label
    Friend WithEvents nudLimitCzasu As System.Windows.Forms.NumericUpDown
    Friend WithEvents grpWmts As System.Windows.Forms.GroupBox
    Friend WithEvents chkZachowajKafle As System.Windows.Forms.CheckBox
    Friend WithEvents lblJakoscWmts As System.Windows.Forms.Label
    Friend WithEvents nudJakoscWmts As System.Windows.Forms.NumericUpDown
    Friend WithEvents grpZaawansowane As System.Windows.Forms.GroupBox
    Friend WithEvents chkZamienXY As System.Windows.Forms.CheckBox
    Friend WithEvents btnPrzywroc As System.Windows.Forms.Button
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
