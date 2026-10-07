Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms

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
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.btnPobierz = New System.Windows.Forms.Button()
        Me.grpDaneZrodlowe = New System.Windows.Forms.GroupBox()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.lstWarstwy = New System.Windows.Forms.ListBox()
        Me.cmbZbiorMap = New System.Windows.Forms.ComboBox()
        Me.lblUklad = New System.Windows.Forms.Label()
        Me.cmbUklad = New System.Windows.Forms.ComboBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.stPostep = New System.Windows.Forms.ToolStripProgressBar()
        Me.stFormat = New System.Windows.Forms.ToolStripStatusLabel()
        Me.stFolder = New System.Windows.Forms.ToolStripStatusLabel()
        Me.stSegment = New System.Windows.Forms.ToolStripStatusLabel()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.SesjaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZapiszToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.WczytajToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.NarzedziaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ScalanieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EksportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NakładanieWarstwNaSiebieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UsuwaniePustychSegmentówToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UstawieniaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.InstrukcjaObsługiToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.Label47 = New System.Windows.Forms.Label()
        Me.Label46 = New System.Windows.Forms.Label()
        Me.txtZasiegSegmentuKm = New System.Windows.Forms.TextBox()
        Me.txtBokSegmentu = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblRozmiarPikselaOpis = New System.Windows.Forms.Label()
        Me.txtRozmiarPiksela = New System.Windows.Forms.TextBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpWybraneWarstwy = New System.Windows.Forms.GroupBox()
        Me.btnResetujWarstwy = New System.Windows.Forms.Button()
        Me.lblWarstwa12 = New System.Windows.Forms.Label()
        Me.lblWarstwa08 = New System.Windows.Forms.Label()
        Me.lblWarstwa11 = New System.Windows.Forms.Label()
        Me.lblWarstwa10 = New System.Windows.Forms.Label()
        Me.lblWarstwa07 = New System.Windows.Forms.Label()
        Me.lblWarstwa09 = New System.Windows.Forms.Label()
        Me.lblWarstwa06 = New System.Windows.Forms.Label()
        Me.lblWarstwa02 = New System.Windows.Forms.Label()
        Me.lblWarstwa05 = New System.Windows.Forms.Label()
        Me.lblWarstwa04 = New System.Windows.Forms.Label()
        Me.lblWarstwa01 = New System.Windows.Forms.Label()
        Me.lblWarstwa03 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.btnPowieksz = New System.Windows.Forms.Button()
        Me.btnPomniejsz = New System.Windows.Forms.Button()
        Me.grpRozmiarSiatki = New System.Windows.Forms.GroupBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.txtLiczbaWierszy = New System.Windows.Forms.TextBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.txtLiczbaKolumn = New System.Windows.Forms.TextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.txtWysokoscPx = New System.Windows.Forms.TextBox()
        Me.txtSzerokoscPx = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.txtSzerokoscKm = New System.Windows.Forms.TextBox()
        Me.txtWysokoscKm = New System.Windows.Forms.TextBox()
        Me.Label42 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.btnUsunPobrane = New System.Windows.Forms.Button()
        Me.btnZnaczniki = New System.Windows.Forms.Button()
        Me.btnScalanie = New System.Windows.Forms.Button()
        Me.btnOtworzFolder = New System.Windows.Forms.Button()
        Me.mapa = New GMap.NET.WindowsForms.GMapControl()
        Me.rbOsm = New System.Windows.Forms.RadioButton()
        Me.rbGoogle = New System.Windows.Forms.RadioButton()
        Me.rbBing = New System.Windows.Forms.RadioButton()
        Me.lblSrodekSzer = New System.Windows.Forms.Label()
        Me.lblSrodekOpisSzer = New System.Windows.Forms.Label()
        Me.lblSrodekTytul = New System.Windows.Forms.Label()
        Me.lblKursorTytul = New System.Windows.Forms.Label()
        Me.lblKursorUkladOpis = New System.Windows.Forms.Label()
        Me.lblKursorUklad = New System.Windows.Forms.Label()
        Me.lblSrodekDlug = New System.Windows.Forms.Label()
        Me.lblZoom = New System.Windows.Forms.Label()
        Me.lblSrodekOpisDlug = New System.Windows.Forms.Label()
        Me.Label67 = New System.Windows.Forms.Label()
        Me.txtKomunikaty = New System.Windows.Forms.RichTextBox()
        Me.lblKursorWgs = New System.Windows.Forms.Label()
        Me.grpSchemat = New System.Windows.Forms.GroupBox()
        Me.lblZaznDlugLewa = New System.Windows.Forms.Label()
        Me.lblZaznSzerDol = New System.Windows.Forms.Label()
        Me.lblZaznDlugPrawa = New System.Windows.Forms.Label()
        Me.lblZaznSzerGora = New System.Windows.Forms.Label()
        Me.lblOpisYPrawy = New System.Windows.Forms.Label()
        Me.lblOpisXGora = New System.Windows.Forms.Label()
        Me.txtYPrawy = New System.Windows.Forms.TextBox()
        Me.txtXGora = New System.Windows.Forms.TextBox()
        Me.lblOpisYLewy = New System.Windows.Forms.Label()
        Me.lblOpisXDol = New System.Windows.Forms.Label()
        Me.txtYLewy = New System.Windows.Forms.TextBox()
        Me.txtXDol = New System.Windows.Forms.TextBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.grpDaneZrodlowe.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.grpWybraneWarstwy.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpRozmiarSiatki.SuspendLayout()
        Me.grpSchemat.SuspendLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnPobierz
        '
        Me.btnPobierz.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPobierz.BackColor = System.Drawing.SystemColors.ActiveBorder
        Me.btnPobierz.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPobierz.Location = New System.Drawing.Point(110, 569)
        Me.btnPobierz.Name = "btnPobierz"
        Me.btnPobierz.Size = New System.Drawing.Size(105, 36)
        Me.btnPobierz.TabIndex = 6
        Me.btnPobierz.Tag = ""
        Me.btnPobierz.Text = "Pobierz"
        Me.ToolTip1.SetToolTip(Me.btnPobierz, "Rozpocznij proces pobierania mapy.")
        Me.btnPobierz.UseVisualStyleBackColor = False
        '
        'grpDaneZrodlowe
        '
        Me.grpDaneZrodlowe.Controls.Add(Me.Label30)
        Me.grpDaneZrodlowe.Controls.Add(Me.Label29)
        Me.grpDaneZrodlowe.Controls.Add(Me.lstWarstwy)
        Me.grpDaneZrodlowe.Controls.Add(Me.cmbZbiorMap)
        Me.grpDaneZrodlowe.Controls.Add(Me.lblUklad)
        Me.grpDaneZrodlowe.Controls.Add(Me.cmbUklad)
        Me.grpDaneZrodlowe.Location = New System.Drawing.Point(5, 3)
        Me.grpDaneZrodlowe.Name = "grpDaneZrodlowe"
        Me.grpDaneZrodlowe.Size = New System.Drawing.Size(212, 254)
        Me.grpDaneZrodlowe.TabIndex = 210
        Me.grpDaneZrodlowe.TabStop = False
        Me.grpDaneZrodlowe.Text = "Dane źródłowe"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Location = New System.Drawing.Point(6, 58)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(152, 13)
        Me.Label30.TabIndex = 217
        Me.Label30.Text = " Wybież mapę (warstwa WMS)"
        Me.ToolTip1.SetToolTip(Me.Label30, "Wskaż wybraną warst klikając na nią lewym klawiszem myszki. ")
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Location = New System.Drawing.Point(6, 16)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(166, 13)
        Me.Label29.TabIndex = 216
        Me.Label29.Text = " Wybierz zbiór map (serwer WMS)"
        Me.ToolTip1.SetToolTip(Me.Label29, "Spis dostępnych serwerów WMS, których definicje znajdują się w plikach tekstowych" &
        " katalogu /warstwy/")
        '
        'lstWarstwy
        '
        Me.lstWarstwy.FormattingEnabled = True
        Me.lstWarstwy.Location = New System.Drawing.Point(6, 72)
        Me.lstWarstwy.Name = "lstWarstwy"
        Me.lstWarstwy.Size = New System.Drawing.Size(198, 134)
        Me.lstWarstwy.TabIndex = 216
        '
        'cmbZbiorMap
        '
        Me.cmbZbiorMap.FormattingEnabled = True
        Me.cmbZbiorMap.Location = New System.Drawing.Point(6, 29)
        Me.cmbZbiorMap.MaxDropDownItems = 18
        Me.cmbZbiorMap.Name = "cmbZbiorMap"
        Me.cmbZbiorMap.Size = New System.Drawing.Size(198, 21)
        Me.cmbZbiorMap.TabIndex = 216
        Me.cmbZbiorMap.Text = "skany_map_topograficznych"
        '
        'lblUklad
        '
        Me.lblUklad.AutoSize = True
        Me.lblUklad.Location = New System.Drawing.Point(6, 208)
        Me.lblUklad.Name = "lblUklad"
        Me.lblUklad.Size = New System.Drawing.Size(160, 13)
        Me.lblUklad.TabIndex = 217
        Me.lblUklad.Text = " Układ współrzędnych pobierania"
        Me.ToolTip1.SetToolTip(Me.lblUklad, "Układ, w którym pobierane są segmenty i zapisywane ich pliki georeferencyjne. Serwer WMS musi go obsługiwać.")
        '
        'cmbUklad
        '
        Me.cmbUklad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUklad.FormattingEnabled = True
        Me.cmbUklad.Location = New System.Drawing.Point(6, 223)
        Me.cmbUklad.Name = "cmbUklad"
        Me.cmbUklad.Size = New System.Drawing.Size(198, 21)
        Me.cmbUklad.TabIndex = 218
        Me.ToolTip1.SetToolTip(Me.cmbUklad, "Układ, w którym pobierane są segmenty i zapisywane ich pliki georeferencyjne. Serwer WMS musi go obsługiwać.")
        '
        'Panel1
        '
        Me.Panel1.AutoSize = True
        Me.Panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.Location = New System.Drawing.Point(14, 315)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(0, 0)
        Me.Panel1.TabIndex = 213
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.stPostep, Me.stFormat, Me.stFolder, Me.stSegment})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 662)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(878, 22)
        Me.StatusStrip1.TabIndex = 237
        Me.StatusStrip1.Text = "StatusStrip1"
        Me.ToolTip1.SetToolTip(Me.StatusStrip1, "Informacje o aktualnym formacie plików rastrowych oraz lokalizacji katalogu ""down" &
        "load""")
        '
        'stPostep
        '
        Me.stPostep.Name = "stPostep"
        Me.stPostep.Size = New System.Drawing.Size(100, 16)
        '
        'stFormat
        '
        Me.stFormat.BackColor = System.Drawing.SystemColors.Window
        Me.stFormat.Name = "stFormat"
        Me.stFormat.Size = New System.Drawing.Size(70, 17)
        Me.stFormat.Text = "rozszerzenie"
        '
        'stFolder
        '
        Me.stFolder.Name = "stFolder"
        Me.stFolder.Size = New System.Drawing.Size(81, 17)
        Me.stFolder.Text = "wybierz folder"
        '
        'stSegment
        '
        Me.stSegment.Name = "stSegment"
        Me.stSegment.Size = New System.Drawing.Size(49, 17)
        Me.stSegment.Text = "kwadrat"
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(110, 613)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(105, 21)
        Me.btnPrzerwij.TabIndex = 245
        Me.btnPrzerwij.Tag = ""
        Me.btnPrzerwij.Text = "Przerwij pobieranie"
        Me.ToolTip1.SetToolTip(Me.btnPrzerwij, "Anuluj rozpoczęty proces pobierania mapy")
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'SesjaToolStripMenuItem
        '
        Me.SesjaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZapiszToolStripMenuItem, Me.WczytajToolStripMenuItem1})
        Me.SesjaToolStripMenuItem.Name = "SesjaToolStripMenuItem"
        Me.SesjaToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.SesjaToolStripMenuItem.Size = New System.Drawing.Size(104, 20)
        Me.SesjaToolStripMenuItem.Text = "Sesja pobierania"
        Me.SesjaToolStripMenuItem.ToolTipText = "Wczytaj lub zapisz plik conf.txt, który przechowuje informacje m.in. o zasięgu po" &
    "bieranego obszaru"
        '
        'ZapiszToolStripMenuItem
        '
        Me.ZapiszToolStripMenuItem.Name = "ZapiszToolStripMenuItem"
        Me.ZapiszToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Z), System.Windows.Forms.Keys)
        Me.ZapiszToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.ZapiszToolStripMenuItem.Text = "zapisz ustawienia sesji (conf.txt)"
        '
        'WczytajToolStripMenuItem1
        '
        Me.WczytajToolStripMenuItem1.Name = "WczytajToolStripMenuItem1"
        Me.WczytajToolStripMenuItem1.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.W), System.Windows.Forms.Keys)
        Me.WczytajToolStripMenuItem1.Size = New System.Drawing.Size(290, 22)
        Me.WczytajToolStripMenuItem1.Text = "wczytaj ustawienia sesji (conf.txt)"
        '
        'NarzedziaToolStripMenuItem
        '
        Me.NarzedziaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ScalanieToolStripMenuItem, Me.EksportToolStripMenuItem, Me.NakładanieWarstwNaSiebieToolStripMenuItem, Me.UsuwaniePustychSegmentówToolStripMenuItem})
        Me.NarzedziaToolStripMenuItem.Name = "NarzedziaToolStripMenuItem"
        Me.NarzedziaToolStripMenuItem.Size = New System.Drawing.Size(70, 20)
        Me.NarzedziaToolStripMenuItem.Text = "Narzędzia"
        Me.NarzedziaToolStripMenuItem.ToolTipText = "Opcjonalne narzędzia wykorzystywane jedynie w szczególnych przypadkach"
        '
        'ScalanieToolStripMenuItem
        '
        Me.ScalanieToolStripMenuItem.Name = "ScalanieToolStripMenuItem"
        Me.ScalanieToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.L), System.Windows.Forms.Keys)
        Me.ScalanieToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.ScalanieToolStripMenuItem.Text = "Łączenie segmentów"
        '
        'EksportToolStripMenuItem
        '
        Me.EksportToolStripMenuItem.Name = "EksportToolStripMenuItem"
        Me.EksportToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.E), System.Windows.Forms.Keys)
        Me.EksportToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.EksportToolStripMenuItem.Text = "Eksport do KMZ (Garmin, Locus) / MBTiles"
        '
        'NakładanieWarstwNaSiebieToolStripMenuItem
        '
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Name = "NakładanieWarstwNaSiebieToolStripMenuItem"
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.N), System.Windows.Forms.Keys)
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Text = "Nakładanie dwóch map png na siebie"
        '
        'UsuwaniePustychSegmentówToolStripMenuItem
        '
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Name = "UsuwaniePustychSegmentówToolStripMenuItem"
        Me.UsuwaniePustychSegmentówToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.U), System.Windows.Forms.Keys)
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Text = "Usuwanie pustych segmentów"
        '
        'UstawieniaToolStripMenuItem
        '
        Me.UstawieniaToolStripMenuItem.Name = "UstawieniaToolStripMenuItem"
        Me.UstawieniaToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
        Me.UstawieniaToolStripMenuItem.Text = "Ustawienia"
        Me.UstawieniaToolStripMenuItem.ToolTipText = "Główne ustawienia programu"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutToolStripMenuItem, Me.InstrukcjaObsługiToolStripMenuItem, Me.PomocPomorskieForumEksploracyjneToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(57, 20)
        Me.HelpToolStripMenuItem.Text = "Pomoc"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.AboutToolStripMenuItem.Text = "O programie"
        '
        'InstrukcjaObsługiToolStripMenuItem
        '
        Me.InstrukcjaObsługiToolStripMenuItem.Name = "InstrukcjaObsługiToolStripMenuItem"
        Me.InstrukcjaObsługiToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2
        Me.InstrukcjaObsługiToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.InstrukcjaObsługiToolStripMenuItem.Text = "Podstawowa instrukcja obsługi"
        '
        'PomocPomorskieForumEksploracyjneToolStripMenuItem
        '
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Name = "PomocPomorskieForumEksploracyjneToolStripMenuItem"
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Text = "Pomoc na Pomorskim Forum Eksploracyjnym"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SesjaToolStripMenuItem, Me.NarzedziaToolStripMenuItem, Me.UstawieniaToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(878, 24)
        Me.MenuStrip1.TabIndex = 230
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'Label47
        '
        Me.Label47.AutoSize = True
        Me.Label47.Enabled = False
        Me.Label47.Location = New System.Drawing.Point(262, 94)
        Me.Label47.Name = "Label47"
        Me.Label47.Size = New System.Drawing.Size(13, 13)
        Me.Label47.TabIndex = 221
        Me.Label47.Text = "="
        Me.ToolTip1.SetToolTip(Me.Label47, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" &
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'Label46
        '
        Me.Label46.AutoSize = True
        Me.Label46.Enabled = False
        Me.Label46.Location = New System.Drawing.Point(263, 57)
        Me.Label46.Name = "Label46"
        Me.Label46.Size = New System.Drawing.Size(12, 13)
        Me.Label46.TabIndex = 220
        Me.Label46.Text = "x"
        Me.ToolTip1.SetToolTip(Me.Label46, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" &
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'txtZasiegSegmentuKm
        '
        Me.txtZasiegSegmentuKm.Enabled = False
        Me.txtZasiegSegmentuKm.Location = New System.Drawing.Point(246, 108)
        Me.txtZasiegSegmentuKm.Name = "txtZasiegSegmentuKm"
        Me.txtZasiegSegmentuKm.Size = New System.Drawing.Size(44, 20)
        Me.txtZasiegSegmentuKm.TabIndex = 219
        Me.txtZasiegSegmentuKm.Text = "2000"
        Me.txtZasiegSegmentuKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtBokSegmentu
        '
        Me.txtBokSegmentu.Location = New System.Drawing.Point(246, 72)
        Me.txtBokSegmentu.Name = "txtBokSegmentu"
        Me.txtBokSegmentu.Size = New System.Drawing.Size(44, 20)
        Me.txtBokSegmentu.TabIndex = 211
        Me.txtBokSegmentu.Text = "2000"
        Me.txtBokSegmentu.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(293, 77)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(116, 13)
        Me.Label1.TabIndex = 213
        Me.Label1.Text = "Rozmiar segmentu [pix]"
        Me.ToolTip1.SetToolTip(Me.Label1, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z" &
        "ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d" &
        "la Geoportalu2 wynosi 2048px")
        '
        'lblRozmiarPikselaOpis
        '
        Me.lblRozmiarPikselaOpis.AutoSize = True
        Me.lblRozmiarPikselaOpis.Location = New System.Drawing.Point(292, 39)
        Me.lblRozmiarPikselaOpis.Name = "lblRozmiarPikselaOpis"
        Me.lblRozmiarPikselaOpis.Size = New System.Drawing.Size(116, 13)
        Me.lblRozmiarPikselaOpis.TabIndex = 214
        Me.lblRozmiarPikselaOpis.Text = "Rozmiar piksela [m/pix]"
        Me.ToolTip1.SetToolTip(Me.lblRozmiarPikselaOpis, "Domyślna wartość jest optymalna. Im większy rozmiar piksela, tym gorsza jakość ob" &
        "razu, ale jednocześnie tym większy jego przestrzenny zasięg.")
        '
        'txtRozmiarPiksela
        '
        Me.txtRozmiarPiksela.Location = New System.Drawing.Point(246, 36)
        Me.txtRozmiarPiksela.Name = "txtRozmiarPiksela"
        Me.txtRozmiarPiksela.Size = New System.Drawing.Size(44, 20)
        Me.txtRozmiarPiksela.TabIndex = 212
        Me.txtRozmiarPiksela.Text = "1"
        Me.txtRozmiarPiksela.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'ToolTip1
        '
        Me.ToolTip1.IsBalloon = True
        '
        'grpWybraneWarstwy
        '
        Me.grpWybraneWarstwy.Controls.Add(Me.btnResetujWarstwy)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa12)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa08)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa11)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa10)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa07)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa09)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa06)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa02)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa05)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa04)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa01)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblWarstwa03)
        Me.grpWybraneWarstwy.Controls.Add(Me.PictureBox1)
        Me.grpWybraneWarstwy.Location = New System.Drawing.Point(5, 258)
        Me.grpWybraneWarstwy.Name = "grpWybraneWarstwy"
        Me.grpWybraneWarstwy.Size = New System.Drawing.Size(210, 234)
        Me.grpWybraneWarstwy.TabIndex = 268
        Me.grpWybraneWarstwy.TabStop = False
        Me.grpWybraneWarstwy.Text = "Wybrane warstwy oraz ich kolejność"
        Me.ToolTip1.SetToolTip(Me.grpWybraneWarstwy, """Kanapka"" warstw. Pierwsza warstwa to bazowy podkład, który można przykryć kolejn" &
        "ymi warstwami o wyższym numerze, o ile mają one przeźroczyste tło, lub mają niep" &
        "ełne pokrycie.")
        '
        'btnResetujWarstwy
        '
        Me.btnResetujWarstwy.Image = Global.MapoTero.My.Resources.Resources.kosz
        Me.btnResetujWarstwy.Location = New System.Drawing.Point(4, 209)
        Me.btnResetujWarstwy.Name = "btnResetujWarstwy"
        Me.btnResetujWarstwy.Size = New System.Drawing.Size(26, 22)
        Me.btnResetujWarstwy.TabIndex = 304
        Me.btnResetujWarstwy.UseVisualStyleBackColor = True
        '
        'lblWarstwa12
        '
        Me.lblWarstwa12.AutoSize = True
        Me.lblWarstwa12.Location = New System.Drawing.Point(1, 16)
        Me.lblWarstwa12.Name = "lblWarstwa12"
        Me.lblWarstwa12.Size = New System.Drawing.Size(22, 13)
        Me.lblWarstwa12.TabIndex = 229
        Me.lblWarstwa12.Text = "12)"
        '
        'lblWarstwa08
        '
        Me.lblWarstwa08.AutoSize = True
        Me.lblWarstwa08.Location = New System.Drawing.Point(6, 78)
        Me.lblWarstwa08.Name = "lblWarstwa08"
        Me.lblWarstwa08.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa08.TabIndex = 225
        Me.lblWarstwa08.Text = "8)"
        '
        'lblWarstwa11
        '
        Me.lblWarstwa11.AutoSize = True
        Me.lblWarstwa11.Location = New System.Drawing.Point(1, 31)
        Me.lblWarstwa11.Name = "lblWarstwa11"
        Me.lblWarstwa11.Size = New System.Drawing.Size(22, 13)
        Me.lblWarstwa11.TabIndex = 228
        Me.lblWarstwa11.Text = "11)"
        '
        'lblWarstwa10
        '
        Me.lblWarstwa10.AutoSize = True
        Me.lblWarstwa10.Location = New System.Drawing.Point(1, 47)
        Me.lblWarstwa10.Name = "lblWarstwa10"
        Me.lblWarstwa10.Size = New System.Drawing.Size(22, 13)
        Me.lblWarstwa10.TabIndex = 227
        Me.lblWarstwa10.Text = "10)"
        '
        'lblWarstwa07
        '
        Me.lblWarstwa07.AutoSize = True
        Me.lblWarstwa07.Location = New System.Drawing.Point(6, 93)
        Me.lblWarstwa07.Name = "lblWarstwa07"
        Me.lblWarstwa07.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa07.TabIndex = 224
        Me.lblWarstwa07.Text = "7)"
        '
        'lblWarstwa09
        '
        Me.lblWarstwa09.AutoSize = True
        Me.lblWarstwa09.Location = New System.Drawing.Point(6, 63)
        Me.lblWarstwa09.Name = "lblWarstwa09"
        Me.lblWarstwa09.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa09.TabIndex = 226
        Me.lblWarstwa09.Text = "9)"
        '
        'lblWarstwa06
        '
        Me.lblWarstwa06.AutoSize = True
        Me.lblWarstwa06.Location = New System.Drawing.Point(6, 109)
        Me.lblWarstwa06.Name = "lblWarstwa06"
        Me.lblWarstwa06.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa06.TabIndex = 223
        Me.lblWarstwa06.Text = "6)"
        '
        'lblWarstwa02
        '
        Me.lblWarstwa02.AutoSize = True
        Me.lblWarstwa02.Location = New System.Drawing.Point(6, 178)
        Me.lblWarstwa02.Name = "lblWarstwa02"
        Me.lblWarstwa02.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa02.TabIndex = 219
        Me.lblWarstwa02.Text = "2)"
        Me.ToolTip1.SetToolTip(Me.lblWarstwa02, "Kolejna nakładana warstwa, która przykryje podkład")
        '
        'lblWarstwa05
        '
        Me.lblWarstwa05.AutoSize = True
        Me.lblWarstwa05.Location = New System.Drawing.Point(6, 126)
        Me.lblWarstwa05.Name = "lblWarstwa05"
        Me.lblWarstwa05.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa05.TabIndex = 222
        Me.lblWarstwa05.Text = "5)"
        '
        'lblWarstwa04
        '
        Me.lblWarstwa04.AutoSize = True
        Me.lblWarstwa04.Location = New System.Drawing.Point(6, 143)
        Me.lblWarstwa04.Name = "lblWarstwa04"
        Me.lblWarstwa04.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa04.TabIndex = 221
        Me.lblWarstwa04.Text = "4)"
        '
        'lblWarstwa01
        '
        Me.lblWarstwa01.AutoSize = True
        Me.lblWarstwa01.Location = New System.Drawing.Point(6, 194)
        Me.lblWarstwa01.Name = "lblWarstwa01"
        Me.lblWarstwa01.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa01.TabIndex = 218
        Me.lblWarstwa01.Text = "1)"
        Me.ToolTip1.SetToolTip(Me.lblWarstwa01, "Główna warstwa podkładowa, którą można przykryć kolejnymi warstwami (o ile mają p" &
        "rzeźroczyste tło lub nie pokrywają całkowicie podkładu)")
        '
        'lblWarstwa03
        '
        Me.lblWarstwa03.AutoSize = True
        Me.lblWarstwa03.Location = New System.Drawing.Point(6, 161)
        Me.lblWarstwa03.Name = "lblWarstwa03"
        Me.lblWarstwa03.Size = New System.Drawing.Size(16, 13)
        Me.lblWarstwa03.TabIndex = 220
        Me.lblWarstwa03.Text = "3)"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = Global.MapoTero.My.Resources.Resources.kanapka_gis
        Me.PictureBox1.Location = New System.Drawing.Point(92, 91)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(113, 138)
        Me.PictureBox1.TabIndex = 305
        Me.PictureBox1.TabStop = False
        '
        'btnPowieksz
        '
        Me.btnPowieksz.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPowieksz.Location = New System.Drawing.Point(6, 28)
        Me.btnPowieksz.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPowieksz.Name = "btnPowieksz"
        Me.btnPowieksz.Size = New System.Drawing.Size(23, 31)
        Me.btnPowieksz.TabIndex = 295
        Me.btnPowieksz.Text = "+"
        Me.ToolTip1.SetToolTip(Me.btnPowieksz, "Powiększ")
        Me.btnPowieksz.UseVisualStyleBackColor = True
        '
        'btnPomniejsz
        '
        Me.btnPomniejsz.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPomniejsz.Location = New System.Drawing.Point(6, 74)
        Me.btnPomniejsz.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPomniejsz.Name = "btnPomniejsz"
        Me.btnPomniejsz.Size = New System.Drawing.Size(23, 30)
        Me.btnPomniejsz.TabIndex = 296
        Me.btnPomniejsz.Text = "-"
        Me.ToolTip1.SetToolTip(Me.btnPomniejsz, "Powiększ")
        Me.btnPomniejsz.UseVisualStyleBackColor = True
        '
        'grpRozmiarSiatki
        '
        Me.grpRozmiarSiatki.Controls.Add(Me.Label22)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label19)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label18)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtLiczbaWierszy)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label21)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtLiczbaKolumn)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label20)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtWysokoscPx)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtSzerokoscPx)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label9)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label17)
        Me.grpRozmiarSiatki.Controls.Add(Me.Label10)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtSzerokoscKm)
        Me.grpRozmiarSiatki.Controls.Add(Me.txtWysokoscKm)
        Me.grpRozmiarSiatki.Enabled = False
        Me.grpRozmiarSiatki.Location = New System.Drawing.Point(434, 3)
        Me.grpRozmiarSiatki.Name = "grpRozmiarSiatki"
        Me.grpRozmiarSiatki.Size = New System.Drawing.Size(221, 141)
        Me.grpRozmiarSiatki.TabIndex = 300
        Me.grpRozmiarSiatki.TabStop = False
        Me.grpRozmiarSiatki.Text = "Wynikowy rozmiar siatki segmentów"
        Me.ToolTip1.SetToolTip(Me.grpRozmiarSiatki, "Informacje o wynikowym rozmiarze siatki kwadratów na które zostanie podzielony ca" &
        "ły obszar pobieranej mapy")
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Enabled = False
        Me.Label22.Location = New System.Drawing.Point(105, 110)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(12, 13)
        Me.Label22.TabIndex = 242
        Me.Label22.Text = "x"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Enabled = False
        Me.Label19.Location = New System.Drawing.Point(106, 73)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(12, 13)
        Me.Label19.TabIndex = 241
        Me.Label19.Text = "x"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Enabled = False
        Me.Label18.Location = New System.Drawing.Point(106, 40)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(12, 13)
        Me.Label18.TabIndex = 240
        Me.Label18.Text = "x"
        '
        'txtLiczbaWierszy
        '
        Me.txtLiczbaWierszy.Enabled = False
        Me.txtLiczbaWierszy.Location = New System.Drawing.Point(118, 108)
        Me.txtLiczbaWierszy.Name = "txtLiczbaWierszy"
        Me.txtLiczbaWierszy.Size = New System.Drawing.Size(55, 20)
        Me.txtLiczbaWierszy.TabIndex = 239
        Me.txtLiczbaWierszy.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(8, 110)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(38, 13)
        Me.Label21.TabIndex = 238
        Me.Label21.Text = "[segm]"
        '
        'txtLiczbaKolumn
        '
        Me.txtLiczbaKolumn.Enabled = False
        Me.txtLiczbaKolumn.Location = New System.Drawing.Point(49, 108)
        Me.txtLiczbaKolumn.Name = "txtLiczbaKolumn"
        Me.txtLiczbaKolumn.Size = New System.Drawing.Size(55, 20)
        Me.txtLiczbaKolumn.TabIndex = 237
        Me.txtLiczbaKolumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(21, 74)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(26, 13)
        Me.Label20.TabIndex = 235
        Me.Label20.Text = "[pix]"
        '
        'txtWysokoscPx
        '
        Me.txtWysokoscPx.Enabled = False
        Me.txtWysokoscPx.Location = New System.Drawing.Point(119, 71)
        Me.txtWysokoscPx.Name = "txtWysokoscPx"
        Me.txtWysokoscPx.Size = New System.Drawing.Size(55, 20)
        Me.txtWysokoscPx.TabIndex = 234
        Me.txtWysokoscPx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtSzerokoscPx
        '
        Me.txtSzerokoscPx.Enabled = False
        Me.txtSzerokoscPx.Location = New System.Drawing.Point(49, 71)
        Me.txtSzerokoscPx.Name = "txtSzerokoscPx"
        Me.txtSzerokoscPx.Size = New System.Drawing.Size(55, 20)
        Me.txtSzerokoscPx.TabIndex = 233
        Me.txtSzerokoscPx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(24, 19)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(84, 13)
        Me.Label9.TabIndex = 214
        Me.Label9.Text = "Szerokość siatki"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(20, 39)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(27, 13)
        Me.Label17.TabIndex = 231
        Me.Label17.Text = "[km]"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(117, 19)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(84, 13)
        Me.Label10.TabIndex = 215
        Me.Label10.Text = "Wysokość siatki"
        '
        'txtSzerokoscKm
        '
        Me.txtSzerokoscKm.Enabled = False
        Me.txtSzerokoscKm.Location = New System.Drawing.Point(49, 36)
        Me.txtSzerokoscKm.Name = "txtSzerokoscKm"
        Me.txtSzerokoscKm.Size = New System.Drawing.Size(55, 20)
        Me.txtSzerokoscKm.TabIndex = 216
        Me.txtSzerokoscKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtWysokoscKm
        '
        Me.txtWysokoscKm.Enabled = False
        Me.txtWysokoscKm.Location = New System.Drawing.Point(119, 36)
        Me.txtWysokoscKm.Name = "txtWysokoscKm"
        Me.txtWysokoscKm.Size = New System.Drawing.Size(55, 20)
        Me.txtWysokoscKm.TabIndex = 217
        Me.txtWysokoscKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label42
        '
        Me.Label42.AutoSize = True
        Me.Label42.Enabled = False
        Me.Label42.Location = New System.Drawing.Point(292, 105)
        Me.Label42.Name = "Label42"
        Me.Label42.Size = New System.Drawing.Size(87, 13)
        Me.Label42.TabIndex = 215
        Me.Label42.Text = "Terenowy zasięg"
        Me.ToolTip1.SetToolTip(Me.Label42, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z" &
        "ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d" &
        "la Geoportalu2 wynosi 2048px")
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Enabled = False
        Me.Label7.Location = New System.Drawing.Point(294, 118)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(117, 13)
        Me.Label7.TabIndex = 316
        Me.Label7.Text = "jednego segmentu [km]"
        Me.ToolTip1.SetToolTip(Me.Label7, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z" &
        "ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d" &
        "la Geoportalu2 wynosi 2048px")
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(244, 18)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(166, 13)
        Me.Label8.TabIndex = 317
        Me.Label8.Text = "Rozmiar pojedynczego segmentu:"
        Me.ToolTip1.SetToolTip(Me.Label8, "Domyślna wartość jest optymalna. Im większy rozmiar piksela, tym gorsza jakość ob" &
        "razu, ale jednocześnie tym większy jego przestrzenny zasięg.")
        '
        'btnUsunPobrane
        '
        Me.btnUsunPobrane.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnUsunPobrane.Image = Global.MapoTero.My.Resources.Resources.ico_folde_delete
        Me.btnUsunPobrane.Location = New System.Drawing.Point(39, 603)
        Me.btnUsunPobrane.Margin = New System.Windows.Forms.Padding(0)
        Me.btnUsunPobrane.Name = "btnUsunPobrane"
        Me.btnUsunPobrane.Size = New System.Drawing.Size(30, 30)
        Me.btnUsunPobrane.TabIndex = 307
        Me.ToolTip1.SetToolTip(Me.btnUsunPobrane, "usuń wszystkie pobrane mapy")
        Me.btnUsunPobrane.UseVisualStyleBackColor = True
        '
        'btnZnaczniki
        '
        Me.btnZnaczniki.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZnaczniki.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZnaczniki.Image = Global.MapoTero.My.Resources.Resources.marker_ico
        Me.btnZnaczniki.Location = New System.Drawing.Point(39, 569)
        Me.btnZnaczniki.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZnaczniki.Name = "btnZnaczniki"
        Me.btnZnaczniki.Size = New System.Drawing.Size(30, 30)
        Me.btnZnaczniki.TabIndex = 306
        Me.ToolTip1.SetToolTip(Me.btnZnaczniki, "wyświetl rzeczywisty zasięg pobieranego obszaru, uwzględniający rozmiar siatki se" &
        "gmentów")
        Me.btnZnaczniki.UseVisualStyleBackColor = True
        '
        'btnScalanie
        '
        Me.btnScalanie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnScalanie.Image = Global.MapoTero.My.Resources.Resources.ico_merge
        Me.btnScalanie.Location = New System.Drawing.Point(5, 603)
        Me.btnScalanie.Margin = New System.Windows.Forms.Padding(0)
        Me.btnScalanie.Name = "btnScalanie"
        Me.btnScalanie.Size = New System.Drawing.Size(30, 30)
        Me.btnScalanie.TabIndex = 305
        Me.ToolTip1.SetToolTip(Me.btnScalanie, "złącz pobrane segmenty w jeden arkusz")
        Me.btnScalanie.UseVisualStyleBackColor = True
        '
        'btnOtworzFolder
        '
        Me.btnOtworzFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOtworzFolder.Image = Global.MapoTero.My.Resources.Resources.ico_folder
        Me.btnOtworzFolder.Location = New System.Drawing.Point(5, 569)
        Me.btnOtworzFolder.Margin = New System.Windows.Forms.Padding(0)
        Me.btnOtworzFolder.Name = "btnOtworzFolder"
        Me.btnOtworzFolder.Size = New System.Drawing.Size(30, 30)
        Me.btnOtworzFolder.TabIndex = 304
        Me.ToolTip1.SetToolTip(Me.btnOtworzFolder, "wyświetl folder z segmentami pobranej mapy")
        Me.btnOtworzFolder.UseVisualStyleBackColor = True
        '
        'mapa
        '
        Me.mapa.Bearing = 0!
        Me.mapa.CanDragMap = True
        Me.mapa.Cursor = System.Windows.Forms.Cursors.Hand
        Me.mapa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mapa.EmptyTileColor = System.Drawing.Color.Navy
        Me.mapa.GrayScaleMode = False
        Me.mapa.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow
        Me.mapa.LevelsKeepInMemory = 5
        Me.mapa.Location = New System.Drawing.Point(0, 24)
        Me.mapa.MarkersEnabled = True
        Me.mapa.MaxZoom = 17
        Me.mapa.MinZoom = 3
        Me.mapa.MouseWheelZoomEnabled = True
        Me.mapa.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter
        Me.mapa.Name = "mapa"
        Me.mapa.NegativeMode = False
        Me.mapa.PolygonsEnabled = True
        Me.mapa.RetryLoadTile = 0
        Me.mapa.RoutesEnabled = True
        Me.mapa.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional
        Me.mapa.SelectedAreaFillColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.mapa.ShowTileGridLines = False
        Me.mapa.Size = New System.Drawing.Size(661, 491)
        Me.mapa.TabIndex = 249
        Me.mapa.Zoom = 0R
        '
        'rbOsm
        '
        Me.rbOsm.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbOsm.AutoSize = True
        Me.rbOsm.Location = New System.Drawing.Point(544, 64)
        Me.rbOsm.Name = "rbOsm"
        Me.rbOsm.Size = New System.Drawing.Size(109, 17)
        Me.rbOsm.TabIndex = 258
        Me.rbOsm.Checked = True
        Me.rbOsm.Text = "OpenStreetMap   "
        Me.rbOsm.UseVisualStyleBackColor = True
        '
        'rbGoogle
        '
        Me.rbGoogle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbGoogle.AutoSize = True
        Me.rbGoogle.Location = New System.Drawing.Point(544, 26)
        Me.rbGoogle.Name = "rbGoogle"
        Me.rbGoogle.Size = New System.Drawing.Size(110, 17)
        Me.rbGoogle.TabIndex = 259
        Me.rbGoogle.TabStop = True
        Me.rbGoogle.Text = "GoogleMap          "
        Me.rbGoogle.UseVisualStyleBackColor = True
        '
        'rbBing
        '
        Me.rbBing.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbBing.AutoSize = True
        Me.rbBing.Location = New System.Drawing.Point(544, 44)
        Me.rbBing.Name = "rbBing"
        Me.rbBing.Size = New System.Drawing.Size(109, 17)
        Me.rbBing.TabIndex = 260
        Me.rbBing.Text = "SatelliteBingsMap"
        Me.rbBing.UseVisualStyleBackColor = True
        '
        'lblSrodekSzer
        '
        Me.lblSrodekSzer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekSzer.AutoSize = True
        Me.lblSrodekSzer.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekSzer.Location = New System.Drawing.Point(528, 499)
        Me.lblSrodekSzer.Name = "lblSrodekSzer"
        Me.lblSrodekSzer.Size = New System.Drawing.Size(28, 13)
        Me.lblSrodekSzer.TabIndex = 261
        Me.lblSrodekSzer.Text = "52.3"
        '
        'lblSrodekOpisSzer
        '
        Me.lblSrodekOpisSzer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekOpisSzer.AutoSize = True
        Me.lblSrodekOpisSzer.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekOpisSzer.Location = New System.Drawing.Point(455, 499)
        Me.lblSrodekOpisSzer.Name = "lblSrodekOpisSzer"
        Me.lblSrodekOpisSzer.Size = New System.Drawing.Size(75, 13)
        Me.lblSrodekOpisSzer.TabIndex = 263
        Me.lblSrodekOpisSzer.Text = "WGS84  Lat ="
        '
        'lblSrodekTytul
        '
        Me.lblSrodekTytul.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekTytul.AutoSize = True
        Me.lblSrodekTytul.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekTytul.Location = New System.Drawing.Point(455, 482)
        Me.lblSrodekTytul.Name = "lblSrodekTytul"
        Me.lblSrodekTytul.Size = New System.Drawing.Size(161, 13)
        Me.lblSrodekTytul.TabIndex = 264
        Me.lblSrodekTytul.Text = "Współrzędne środka okna mapy"
        '
        'lblKursorTytul
        '
        Me.lblKursorTytul.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorTytul.AutoSize = True
        Me.lblKursorTytul.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorTytul.Location = New System.Drawing.Point(6, 481)
        Me.lblKursorTytul.Name = "lblKursorTytul"
        Me.lblKursorTytul.Size = New System.Drawing.Size(109, 13)
        Me.lblKursorTytul.TabIndex = 265
        Me.lblKursorTytul.Text = "Współrzędne kursora"
        '
        'lblKursorUkladOpis
        '
        Me.lblKursorUkladOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorUkladOpis.AutoSize = True
        Me.lblKursorUkladOpis.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorUkladOpis.Location = New System.Drawing.Point(7, 498)
        Me.lblKursorUkladOpis.Name = "lblKursorUkladOpis"
        Me.lblKursorUkladOpis.Size = New System.Drawing.Size(31, 13)
        Me.lblKursorUkladOpis.TabIndex = 266
        Me.lblKursorUkladOpis.Text = "1992"
        '
        'lblKursorUklad
        '
        Me.lblKursorUklad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorUklad.AutoSize = True
        Me.lblKursorUklad.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorUklad.Location = New System.Drawing.Point(46, 498)
        Me.lblKursorUklad.Name = "lblKursorUklad"
        Me.lblKursorUklad.Size = New System.Drawing.Size(31, 13)
        Me.lblKursorUklad.TabIndex = 267
        Me.lblKursorUklad.Text = "1992"
        '
        'lblSrodekDlug
        '
        Me.lblSrodekDlug.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekDlug.AutoSize = True
        Me.lblSrodekDlug.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekDlug.Location = New System.Drawing.Point(609, 499)
        Me.lblSrodekDlug.Name = "lblSrodekDlug"
        Me.lblSrodekDlug.Size = New System.Drawing.Size(31, 13)
        Me.lblSrodekDlug.TabIndex = 290
        Me.lblSrodekDlug.Text = "19.2 "
        '
        'lblZoom
        '
        Me.lblZoom.AutoSize = True
        Me.lblZoom.Location = New System.Drawing.Point(9, 60)
        Me.lblZoom.Name = "lblZoom"
        Me.lblZoom.Size = New System.Drawing.Size(13, 13)
        Me.lblZoom.TabIndex = 292
        Me.lblZoom.Text = "6"
        '
        'lblSrodekOpisDlug
        '
        Me.lblSrodekOpisDlug.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekOpisDlug.AutoSize = True
        Me.lblSrodekOpisDlug.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekOpisDlug.Location = New System.Drawing.Point(576, 499)
        Me.lblSrodekOpisDlug.Name = "lblSrodekOpisDlug"
        Me.lblSrodekOpisDlug.Size = New System.Drawing.Size(31, 13)
        Me.lblSrodekOpisDlug.TabIndex = 293
        Me.lblSrodekOpisDlug.Text = "Lng="
        '
        'Label67
        '
        Me.Label67.AutoSize = True
        Me.Label67.BackColor = System.Drawing.SystemColors.ControlLight
        Me.Label67.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label67.ForeColor = System.Drawing.Color.Green
        Me.Label67.Location = New System.Drawing.Point(334, 5)
        Me.Label67.Name = "Label67"
        Me.Label67.Size = New System.Drawing.Size(320, 13)
        Me.Label67.TabIndex = 297
        Me.Label67.Text = "Zaznacz na mapie  prawym  przyciskiem myszy obszar do pobrania"
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtKomunikaty.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.txtKomunikaty.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtKomunikaty.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtKomunikaty.Location = New System.Drawing.Point(5, 500)
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.Size = New System.Drawing.Size(210, 65)
        Me.txtKomunikaty.TabIndex = 301
        Me.txtKomunikaty.Text = "Komunikaty:"
        '
        'lblKursorWgs
        '
        Me.lblKursorWgs.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorWgs.AutoSize = True
        Me.lblKursorWgs.Location = New System.Drawing.Point(83, 499)
        Me.lblKursorWgs.Name = "lblKursorWgs"
        Me.lblKursorWgs.Size = New System.Drawing.Size(36, 13)
        Me.lblKursorWgs.TabIndex = 260
        Me.lblKursorWgs.Text = "kursor"
        '
        'grpSchemat
        '
        Me.grpSchemat.Controls.Add(Me.Label8)
        Me.grpSchemat.Controls.Add(Me.Label7)
        Me.grpSchemat.Controls.Add(Me.Label42)
        Me.grpSchemat.Controls.Add(Me.Label47)
        Me.grpSchemat.Controls.Add(Me.lblZaznDlugLewa)
        Me.grpSchemat.Controls.Add(Me.txtZasiegSegmentuKm)
        Me.grpSchemat.Controls.Add(Me.Label46)
        Me.grpSchemat.Controls.Add(Me.lblZaznSzerDol)
        Me.grpSchemat.Controls.Add(Me.lblZaznDlugPrawa)
        Me.grpSchemat.Controls.Add(Me.lblZaznSzerGora)
        Me.grpSchemat.Controls.Add(Me.lblRozmiarPikselaOpis)
        Me.grpSchemat.Controls.Add(Me.txtBokSegmentu)
        Me.grpSchemat.Controls.Add(Me.txtRozmiarPiksela)
        Me.grpSchemat.Controls.Add(Me.Label1)
        Me.grpSchemat.Controls.Add(Me.lblOpisYPrawy)
        Me.grpSchemat.Controls.Add(Me.lblOpisXGora)
        Me.grpSchemat.Controls.Add(Me.txtYPrawy)
        Me.grpSchemat.Controls.Add(Me.txtXGora)
        Me.grpSchemat.Controls.Add(Me.lblOpisYLewy)
        Me.grpSchemat.Controls.Add(Me.lblOpisXDol)
        Me.grpSchemat.Controls.Add(Me.txtYLewy)
        Me.grpSchemat.Controls.Add(Me.txtXDol)
        Me.grpSchemat.Controls.Add(Me.PictureBox2)
        Me.grpSchemat.Location = New System.Drawing.Point(5, 3)
        Me.grpSchemat.Name = "grpSchemat"
        Me.grpSchemat.Size = New System.Drawing.Size(418, 141)
        Me.grpSchemat.TabIndex = 303
        Me.grpSchemat.TabStop = False
        Me.grpSchemat.Text = "Schemat siatki segmentów pobieranej mapy i zasięgu zaznaczenia"
        '
        'lblZaznDlugLewa
        '
        Me.lblZaznDlugLewa.AutoSize = True
        Me.lblZaznDlugLewa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblZaznDlugLewa.ForeColor = System.Drawing.Color.Navy
        Me.lblZaznDlugLewa.Location = New System.Drawing.Point(4, 63)
        Me.lblZaznDlugLewa.Name = "lblZaznDlugLewa"
        Me.lblZaznDlugLewa.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznDlugLewa.TabIndex = 315
        Me.lblZaznDlugLewa.Text = "."
        '
        'lblZaznSzerDol
        '
        Me.lblZaznSzerDol.AutoSize = True
        Me.lblZaznSzerDol.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblZaznSzerDol.ForeColor = System.Drawing.Color.Navy
        Me.lblZaznSzerDol.Location = New System.Drawing.Point(72, 98)
        Me.lblZaznSzerDol.Name = "lblZaznSzerDol"
        Me.lblZaznSzerDol.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznSzerDol.TabIndex = 314
        Me.lblZaznSzerDol.Text = "."
        '
        'lblZaznDlugPrawa
        '
        Me.lblZaznDlugPrawa.AutoSize = True
        Me.lblZaznDlugPrawa.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblZaznDlugPrawa.ForeColor = System.Drawing.Color.Navy
        Me.lblZaznDlugPrawa.Location = New System.Drawing.Point(134, 64)
        Me.lblZaznDlugPrawa.Name = "lblZaznDlugPrawa"
        Me.lblZaznDlugPrawa.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznDlugPrawa.TabIndex = 313
        Me.lblZaznDlugPrawa.Text = "."
        '
        'lblZaznSzerGora
        '
        Me.lblZaznSzerGora.AutoSize = True
        Me.lblZaznSzerGora.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblZaznSzerGora.ForeColor = System.Drawing.Color.Navy
        Me.lblZaznSzerGora.Location = New System.Drawing.Point(73, 33)
        Me.lblZaznSzerGora.Name = "lblZaznSzerGora"
        Me.lblZaznSzerGora.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznSzerGora.TabIndex = 312
        Me.lblZaznSzerGora.Text = "."
        '
        'lblOpisYPrawy
        '
        Me.lblOpisYPrawy.AutoSize = True
        Me.lblOpisYPrawy.ForeColor = System.Drawing.Color.Blue
        Me.lblOpisYPrawy.Location = New System.Drawing.Point(133, 79)
        Me.lblOpisYPrawy.Name = "lblOpisYPrawy"
        Me.lblOpisYPrawy.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisYPrawy.TabIndex = 310
        Me.lblOpisYPrawy.Text = "y"
        '
        'lblOpisXGora
        '
        Me.lblOpisXGora.AutoSize = True
        Me.lblOpisXGora.ForeColor = System.Drawing.Color.Blue
        Me.lblOpisXGora.Location = New System.Drawing.Point(72, 52)
        Me.lblOpisXGora.Name = "lblOpisXGora"
        Me.lblOpisXGora.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisXGora.TabIndex = 311
        Me.lblOpisXGora.Text = "x"
        '
        'txtYPrawy
        '
        Me.txtYPrawy.ForeColor = System.Drawing.Color.Blue
        Me.txtYPrawy.Location = New System.Drawing.Point(148, 76)
        Me.txtYPrawy.Name = "txtYPrawy"
        Me.txtYPrawy.Size = New System.Drawing.Size(44, 20)
        Me.txtYPrawy.TabIndex = 309
        Me.txtYPrawy.Text = "571000"
        '
        'txtXGora
        '
        Me.txtXGora.ForeColor = System.Drawing.Color.Blue
        Me.txtXGora.Location = New System.Drawing.Point(84, 49)
        Me.txtXGora.Name = "txtXGora"
        Me.txtXGora.Size = New System.Drawing.Size(44, 20)
        Me.txtXGora.TabIndex = 308
        Me.txtXGora.Text = "684000"
        '
        'lblOpisYLewy
        '
        Me.lblOpisYLewy.AutoSize = True
        Me.lblOpisYLewy.ForeColor = System.Drawing.Color.Blue
        Me.lblOpisYLewy.Location = New System.Drawing.Point(4, 78)
        Me.lblOpisYLewy.Name = "lblOpisYLewy"
        Me.lblOpisYLewy.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisYLewy.TabIndex = 307
        Me.lblOpisYLewy.Text = "y"
        '
        'lblOpisXDol
        '
        Me.lblOpisXDol.AutoSize = True
        Me.lblOpisXDol.ForeColor = System.Drawing.Color.Blue
        Me.lblOpisXDol.Location = New System.Drawing.Point(71, 115)
        Me.lblOpisXDol.Name = "lblOpisXDol"
        Me.lblOpisXDol.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisXDol.TabIndex = 306
        Me.lblOpisXDol.Text = "x"
        '
        'txtYLewy
        '
        Me.txtYLewy.ForeColor = System.Drawing.Color.Blue
        Me.txtYLewy.Location = New System.Drawing.Point(21, 75)
        Me.txtYLewy.Name = "txtYLewy"
        Me.txtYLewy.Size = New System.Drawing.Size(44, 20)
        Me.txtYLewy.TabIndex = 305
        Me.txtYLewy.Text = "569000"
        '
        'txtXDol
        '
        Me.txtXDol.ForeColor = System.Drawing.Color.Blue
        Me.txtXDol.Location = New System.Drawing.Point(85, 111)
        Me.txtXDol.Name = "txtXDol"
        Me.txtXDol.Size = New System.Drawing.Size(44, 20)
        Me.txtXDol.TabIndex = 304
        Me.txtXDol.Text = "682000"
        '
        'PictureBox2
        '
        Me.PictureBox2.Image = Global.MapoTero.My.Resources.Resources.siatka
        Me.PictureBox2.Location = New System.Drawing.Point(10, 16)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(231, 120)
        Me.PictureBox2.TabIndex = 303
        Me.PictureBox2.TabStop = False
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.btnUsunPobrane)
        Me.Panel2.Controls.Add(Me.grpDaneZrodlowe)
        Me.Panel2.Controls.Add(Me.btnZnaczniki)
        Me.Panel2.Controls.Add(Me.grpWybraneWarstwy)
        Me.Panel2.Controls.Add(Me.btnScalanie)
        Me.Panel2.Controls.Add(Me.txtKomunikaty)
        Me.Panel2.Controls.Add(Me.btnOtworzFolder)
        Me.Panel2.Controls.Add(Me.btnPobierz)
        Me.Panel2.Controls.Add(Me.btnPrzerwij)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Right
        Me.Panel2.Location = New System.Drawing.Point(661, 24)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(217, 638)
        Me.Panel2.TabIndex = 307
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.grpSchemat)
        Me.Panel3.Controls.Add(Me.grpRozmiarSiatki)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel3.Location = New System.Drawing.Point(0, 515)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(661, 147)
        Me.Panel3.TabIndex = 308
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(878, 684)
        Me.Controls.Add(Me.lblKursorWgs)
        Me.Controls.Add(Me.lblKursorUklad)
        Me.Controls.Add(Me.lblKursorUkladOpis)
        Me.Controls.Add(Me.lblKursorTytul)
        Me.Controls.Add(Me.btnPomniejsz)
        Me.Controls.Add(Me.btnPowieksz)
        Me.Controls.Add(Me.lblSrodekOpisDlug)
        Me.Controls.Add(Me.lblZoom)
        Me.Controls.Add(Me.lblSrodekDlug)
        Me.Controls.Add(Me.lblSrodekTytul)
        Me.Controls.Add(Me.lblSrodekOpisSzer)
        Me.Controls.Add(Me.lblSrodekSzer)
        Me.Controls.Add(Me.rbBing)
        Me.Controls.Add(Me.rbGoogle)
        Me.Controls.Add(Me.rbOsm)
        Me.Controls.Add(Me.mapa)
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Label67)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Form1"
        Me.grpDaneZrodlowe.ResumeLayout(False)
        Me.grpDaneZrodlowe.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.grpWybraneWarstwy.ResumeLayout(False)
        Me.grpWybraneWarstwy.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpRozmiarSiatki.ResumeLayout(False)
        Me.grpRozmiarSiatki.PerformLayout()
        Me.grpSchemat.ResumeLayout(False)
        Me.grpSchemat.PerformLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnPobierz As System.Windows.Forms.Button
    Friend WithEvents grpDaneZrodlowe As System.Windows.Forms.GroupBox
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents stPostep As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents stFolder As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents stFormat As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents btnPrzerwij As System.Windows.Forms.Button
    Friend WithEvents stSegment As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents SesjaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZapiszToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WczytajToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NarzedziaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NakładanieWarstwNaSiebieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UstawieniaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PomocPomorskieForumEksploracyjneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents cmbZbiorMap As System.Windows.Forms.ComboBox
    Friend WithEvents lblUklad As System.Windows.Forms.Label
    Friend WithEvents cmbUklad As System.Windows.Forms.ComboBox
    Friend WithEvents lstWarstwy As System.Windows.Forms.ListBox
    Friend WithEvents InstrukcjaObsługiToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label30 As System.Windows.Forms.Label
    Friend WithEvents Label29 As System.Windows.Forms.Label
    Friend WithEvents UsuwaniePustychSegmentówToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents txtBokSegmentu As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblRozmiarPikselaOpis As System.Windows.Forms.Label
    Friend WithEvents txtRozmiarPiksela As System.Windows.Forms.TextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents mapa As GMap.NET.WindowsForms.GMapControl
    Friend WithEvents rbOsm As System.Windows.Forms.RadioButton
    Friend WithEvents rbGoogle As System.Windows.Forms.RadioButton
    Friend WithEvents rbBing As System.Windows.Forms.RadioButton
    Friend WithEvents lblSrodekSzer As System.Windows.Forms.Label
    Friend WithEvents lblSrodekOpisSzer As System.Windows.Forms.Label
    'Friend WithEvents lblKursorWgs As System.Windows.Forms.Label
    Friend WithEvents lblSrodekTytul As System.Windows.Forms.Label
    Friend WithEvents lblKursorTytul As System.Windows.Forms.Label
    Friend WithEvents lblKursorUkladOpis As System.Windows.Forms.Label
    Friend WithEvents lblKursorUklad As System.Windows.Forms.Label
    Friend WithEvents grpWybraneWarstwy As System.Windows.Forms.GroupBox
    Friend WithEvents lblWarstwa12 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa08 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa11 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa10 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa07 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa09 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa06 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa02 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa05 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa04 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa01 As System.Windows.Forms.Label
    Friend WithEvents lblWarstwa03 As System.Windows.Forms.Label
    Friend WithEvents lblSrodekDlug As System.Windows.Forms.Label
    Friend WithEvents lblZoom As System.Windows.Forms.Label
    Friend WithEvents lblSrodekOpisDlug As System.Windows.Forms.Label
    Friend WithEvents btnPowieksz As System.Windows.Forms.Button
    Friend WithEvents btnPomniejsz As System.Windows.Forms.Button
    Friend WithEvents Label67 As System.Windows.Forms.Label
    Friend WithEvents ScalanieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EksportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents grpRozmiarSiatki As System.Windows.Forms.GroupBox
    Friend WithEvents txtLiczbaWierszy As System.Windows.Forms.TextBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents txtLiczbaKolumn As System.Windows.Forms.TextBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents txtWysokoscPx As System.Windows.Forms.TextBox
    Friend WithEvents txtSzerokoscPx As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtSzerokoscKm As System.Windows.Forms.TextBox
    Friend WithEvents txtWysokoscKm As System.Windows.Forms.TextBox
    Friend WithEvents txtKomunikaty As System.Windows.Forms.RichTextBox
    Friend WithEvents lblKursorWgs As System.Windows.Forms.Label
    Friend WithEvents txtZasiegSegmentuKm As System.Windows.Forms.TextBox
    Friend WithEvents Label47 As System.Windows.Forms.Label
    Friend WithEvents Label46 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents grpSchemat As System.Windows.Forms.GroupBox
    Friend WithEvents lblZaznDlugLewa As System.Windows.Forms.Label
    Friend WithEvents lblZaznSzerDol As System.Windows.Forms.Label
    Friend WithEvents lblZaznDlugPrawa As System.Windows.Forms.Label
    Friend WithEvents lblZaznSzerGora As System.Windows.Forms.Label
    Friend WithEvents lblOpisYPrawy As System.Windows.Forms.Label
    Friend WithEvents lblOpisXGora As System.Windows.Forms.Label
    Friend WithEvents txtYPrawy As System.Windows.Forms.TextBox
    Friend WithEvents txtXGora As System.Windows.Forms.TextBox
    Friend WithEvents lblOpisYLewy As System.Windows.Forms.Label
    Friend WithEvents lblOpisXDol As System.Windows.Forms.Label
    Friend WithEvents txtYLewy As System.Windows.Forms.TextBox
    Friend WithEvents txtXDol As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents btnOtworzFolder As System.Windows.Forms.Button
    Friend WithEvents btnScalanie As System.Windows.Forms.Button
    Friend WithEvents btnResetujWarstwy As System.Windows.Forms.Button
    Friend WithEvents Label42 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents btnZnaczniki As System.Windows.Forms.Button
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Label8 As Label
    Friend WithEvents btnUsunPobrane As Button
End Class
