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
        Me.Button1 = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.ComboBox3 = New System.Windows.Forms.ComboBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel2 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabel3 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.SesjaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZapiszToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.WczytajToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.WarstwyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ScalanieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.TextBox13 = New System.Windows.Forms.TextBox()
        Me.Label42 = New System.Windows.Forms.Label()
        Me.TextBox9 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBox10 = New System.Windows.Forms.TextBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label23 = New System.Windows.Forms.Label()
        Me.Label24 = New System.Windows.Forms.Label()
        Me.Label25 = New System.Windows.Forms.Label()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.Label27 = New System.Windows.Forms.Label()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.TextBox12 = New System.Windows.Forms.TextBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.TextBox11 = New System.Windows.Forms.TextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.TextBox7 = New System.Windows.Forms.TextBox()
        Me.TextBox8 = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TextBox5 = New System.Windows.Forms.TextBox()
        Me.TextBox6 = New System.Windows.Forms.TextBox()
        Me.button6 = New System.Windows.Forms.Button()
        Me.GMapControl1 = New GMap.NET.WindowsForms.GMapControl()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.RadioButton3 = New System.Windows.Forms.RadioButton()
        Me.Label35 = New System.Windows.Forms.Label()
        Me.Label37 = New System.Windows.Forms.Label()
        Me.Label38 = New System.Windows.Forms.Label()
        Me.Label39 = New System.Windows.Forms.Label()
        Me.Label40 = New System.Windows.Forms.Label()
        Me.Label41 = New System.Windows.Forms.Label()
        Me.Label63 = New System.Windows.Forms.Label()
        Me.Label65 = New System.Windows.Forms.Label()
        Me.Label66 = New System.Windows.Forms.Label()
        Me.Label67 = New System.Windows.Forms.Label()
        Me.RichTextBox1 = New System.Windows.Forms.RichTextBox()
        Me.Label36 = New System.Windows.Forms.Label()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.Label34 = New System.Windows.Forms.Label()
        Me.Label32 = New System.Windows.Forms.Label()
        Me.Label33 = New System.Windows.Forms.Label()
        Me.Label31 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TextBox4 = New System.Windows.Forms.TextBox()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.GroupBox2.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.SystemColors.ActiveBorder
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Button1.Location = New System.Drawing.Point(767, 583)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(105, 47)
        Me.Button1.TabIndex = 6
        Me.Button1.Tag = ""
        Me.Button1.Text = "Pobierz"
        Me.ToolTip1.SetToolTip(Me.Button1, "Rozpocznij proces pobierania mapy.")
        Me.Button1.UseVisualStyleBackColor = False
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label30)
        Me.GroupBox2.Controls.Add(Me.Label29)
        Me.GroupBox2.Controls.Add(Me.ListBox1)
        Me.GroupBox2.Controls.Add(Me.ComboBox3)
        Me.GroupBox2.Location = New System.Drawing.Point(660, 25)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(212, 254)
        Me.GroupBox2.TabIndex = 210
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Dane źródłowe"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Location = New System.Drawing.Point(6, 58)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(126, 13)
        Me.Label30.TabIndex = 217
        Me.Label30.Text = "Wybierz warstwę WMS-a"
        Me.ToolTip1.SetToolTip(Me.Label30, "Wskaż wybraną warst klikając na nią lewym klawiszem myszki. ")
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Location = New System.Drawing.Point(6, 16)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(174, 13)
        Me.Label29.TabIndex = 216
        Me.Label29.Text = "Wybierz serwer WMS (rodzaj mapy)"
        Me.ToolTip1.SetToolTip(Me.Label29, "Spis dostępnych serwerów WMS, których definicje znajdują się w plikach tekstowych" & _
        " katalogu /warstwy/")
        '
        'ListBox1
        '
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.Location = New System.Drawing.Point(6, 72)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(198, 173)
        Me.ListBox1.TabIndex = 216
        '
        'ComboBox3
        '
        Me.ComboBox3.FormattingEnabled = True
        Me.ComboBox3.Location = New System.Drawing.Point(6, 29)
        Me.ComboBox3.MaxDropDownItems = 18
        Me.ComboBox3.Name = "ComboBox3"
        Me.ComboBox3.Size = New System.Drawing.Size(198, 21)
        Me.ComboBox3.TabIndex = 216
        Me.ComboBox3.Text = "skany_map_topograficznych"
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
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel2, Me.ToolStripStatusLabel1, Me.ToolStripStatusLabel3})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 660)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(876, 22)
        Me.StatusStrip1.TabIndex = 237
        Me.StatusStrip1.Text = "StatusStrip1"
        Me.ToolTip1.SetToolTip(Me.StatusStrip1, "Informacje o aktualnym formacie plików rastrowych oraz lokalizacji katalogu ""down" & _
        "load""")
        '
        'ToolStripProgressBar1
        '
        Me.ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        Me.ToolStripProgressBar1.Size = New System.Drawing.Size(100, 16)
        '
        'ToolStripStatusLabel2
        '
        Me.ToolStripStatusLabel2.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        Me.ToolStripStatusLabel2.Size = New System.Drawing.Size(67, 17)
        Me.ToolStripStatusLabel2.Text = "rozszerzenie"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(75, 17)
        Me.ToolStripStatusLabel1.Text = "wybierz folder"
        '
        'ToolStripStatusLabel3
        '
        Me.ToolStripStatusLabel3.Name = "ToolStripStatusLabel3"
        Me.ToolStripStatusLabel3.Size = New System.Drawing.Size(46, 17)
        Me.ToolStripStatusLabel3.Text = "kwadrat"
        '
        'Button3
        '
        Me.Button3.Enabled = False
        Me.Button3.Location = New System.Drawing.Point(767, 636)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(105, 21)
        Me.Button3.TabIndex = 245
        Me.Button3.Tag = ""
        Me.Button3.Text = "Przerwij pobieranie"
        Me.ToolTip1.SetToolTip(Me.Button3, "Anuluj rozpoczęty proces pobierania mapy")
        Me.Button3.UseVisualStyleBackColor = True
        '
        'SesjaToolStripMenuItem
        '
        Me.SesjaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZapiszToolStripMenuItem, Me.WczytajToolStripMenuItem1})
        Me.SesjaToolStripMenuItem.Name = "SesjaToolStripMenuItem"
        Me.SesjaToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.SesjaToolStripMenuItem.Size = New System.Drawing.Size(98, 20)
        Me.SesjaToolStripMenuItem.Text = "Sesja pobierania"
        Me.SesjaToolStripMenuItem.ToolTipText = "Wczytaj lub zapisz plik conf.txt, który przechowuje informacje m.in. o zasięgu po" & _
    "bieranego obszaru"
        '
        'ZapiszToolStripMenuItem
        '
        Me.ZapiszToolStripMenuItem.Name = "ZapiszToolStripMenuItem"
        Me.ZapiszToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Z), System.Windows.Forms.Keys)
        Me.ZapiszToolStripMenuItem.Size = New System.Drawing.Size(277, 22)
        Me.ZapiszToolStripMenuItem.Text = "zapisz ustawienia sesji (conf.txt)"
        '
        'WczytajToolStripMenuItem1
        '
        Me.WczytajToolStripMenuItem1.Name = "WczytajToolStripMenuItem1"
        Me.WczytajToolStripMenuItem1.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.W), System.Windows.Forms.Keys)
        Me.WczytajToolStripMenuItem1.Size = New System.Drawing.Size(277, 22)
        Me.WczytajToolStripMenuItem1.Text = "wczytaj ustawienia sesji (conf.txt)"
        '
        'WarstwyToolStripMenuItem
        '
        Me.WarstwyToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ScalanieToolStripMenuItem, Me.NakładanieWarstwNaSiebieToolStripMenuItem, Me.UsuwaniePustychSegmentówToolStripMenuItem})
        Me.WarstwyToolStripMenuItem.Name = "WarstwyToolStripMenuItem"
        Me.WarstwyToolStripMenuItem.Size = New System.Drawing.Size(66, 20)
        Me.WarstwyToolStripMenuItem.Text = "Narzędzia"
        Me.WarstwyToolStripMenuItem.ToolTipText = "Opcjonalne narzędzia wykorzystywane jedynie w szczególnych przypadkach"
        '
        'ScalanieToolStripMenuItem
        '
        Me.ScalanieToolStripMenuItem.Name = "ScalanieToolStripMenuItem"
        Me.ScalanieToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.L), System.Windows.Forms.Keys)
        Me.ScalanieToolStripMenuItem.Size = New System.Drawing.Size(289, 22)
        Me.ScalanieToolStripMenuItem.Text = "Łączenie segmentów"
        '
        'NakładanieWarstwNaSiebieToolStripMenuItem
        '
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Name = "NakładanieWarstwNaSiebieToolStripMenuItem"
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.N), System.Windows.Forms.Keys)
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Size = New System.Drawing.Size(289, 22)
        Me.NakładanieWarstwNaSiebieToolStripMenuItem.Text = "Nakładanie dwóch map png na siebie"
        '
        'UsuwaniePustychSegmentówToolStripMenuItem
        '
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Name = "UsuwaniePustychSegmentówToolStripMenuItem"
        Me.UsuwaniePustychSegmentówToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.U), System.Windows.Forms.Keys)
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Size = New System.Drawing.Size(289, 22)
        Me.UsuwaniePustychSegmentówToolStripMenuItem.Text = "Usuwanie pustych segmentów"
        '
        'UstawieniaToolStripMenuItem
        '
        Me.UstawieniaToolStripMenuItem.Name = "UstawieniaToolStripMenuItem"
        Me.UstawieniaToolStripMenuItem.Size = New System.Drawing.Size(71, 20)
        Me.UstawieniaToolStripMenuItem.Text = "Ustawienia"
        Me.UstawieniaToolStripMenuItem.ToolTipText = "Główne ustawienia programu"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutToolStripMenuItem, Me.InstrukcjaObsługiToolStripMenuItem, Me.PomocPomorskieForumEksploracyjneToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(50, 20)
        Me.HelpToolStripMenuItem.Text = "Pomoc"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(302, 22)
        Me.AboutToolStripMenuItem.Text = "O programie"
        '
        'InstrukcjaObsługiToolStripMenuItem
        '
        Me.InstrukcjaObsługiToolStripMenuItem.Name = "InstrukcjaObsługiToolStripMenuItem"
        Me.InstrukcjaObsługiToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2
        Me.InstrukcjaObsługiToolStripMenuItem.Size = New System.Drawing.Size(302, 22)
        Me.InstrukcjaObsługiToolStripMenuItem.Text = "Podstawowa instrukcja obsługi"
        '
        'PomocPomorskieForumEksploracyjneToolStripMenuItem
        '
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Name = "PomocPomorskieForumEksploracyjneToolStripMenuItem"
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Size = New System.Drawing.Size(302, 22)
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Text = "Pomoc na Pomorskim Forum Eksploracyjnym"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SesjaToolStripMenuItem, Me.WarstwyToolStripMenuItem, Me.UstawieniaToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(876, 24)
        Me.MenuStrip1.TabIndex = 230
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'Label47
        '
        Me.Label47.AutoSize = True
        Me.Label47.Enabled = False
        Me.Label47.Location = New System.Drawing.Point(263, 85)
        Me.Label47.Name = "Label47"
        Me.Label47.Size = New System.Drawing.Size(13, 13)
        Me.Label47.TabIndex = 221
        Me.Label47.Text = "="
        Me.ToolTip1.SetToolTip(Me.Label47, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" & _
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'Label46
        '
        Me.Label46.AutoSize = True
        Me.Label46.Enabled = False
        Me.Label46.Location = New System.Drawing.Point(264, 46)
        Me.Label46.Name = "Label46"
        Me.Label46.Size = New System.Drawing.Size(12, 13)
        Me.Label46.TabIndex = 220
        Me.Label46.Text = "x"
        Me.ToolTip1.SetToolTip(Me.Label46, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" & _
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'TextBox13
        '
        Me.TextBox13.Enabled = False
        Me.TextBox13.Location = New System.Drawing.Point(247, 100)
        Me.TextBox13.Name = "TextBox13"
        Me.TextBox13.Size = New System.Drawing.Size(44, 20)
        Me.TextBox13.TabIndex = 219
        Me.TextBox13.Text = "2000"
        Me.TextBox13.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label42
        '
        Me.Label42.AutoSize = True
        Me.Label42.Enabled = False
        Me.Label42.Location = New System.Drawing.Point(296, 105)
        Me.Label42.Name = "Label42"
        Me.Label42.Size = New System.Drawing.Size(105, 13)
        Me.Label42.TabIndex = 215
        Me.Label42.Text = "Zasięg segmentu [m]"
        Me.ToolTip1.SetToolTip(Me.Label42, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z" & _
        "ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d" & _
        "la Geoportalu2 wynosi 2048px")
        '
        'TextBox9
        '
        Me.TextBox9.Location = New System.Drawing.Point(246, 61)
        Me.TextBox9.Name = "TextBox9"
        Me.TextBox9.Size = New System.Drawing.Size(44, 20)
        Me.TextBox9.TabIndex = 211
        Me.TextBox9.Text = "2000"
        Me.TextBox9.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(294, 66)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(116, 13)
        Me.Label1.TabIndex = 213
        Me.Label1.Text = "Rozmiar segmentu [pix]"
        Me.ToolTip1.SetToolTip(Me.Label1, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z" & _
        "ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d" & _
        "la Geoportalu2 wynosi 2048px")
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(292, 26)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(116, 13)
        Me.Label2.TabIndex = 214
        Me.Label2.Text = "Rozmiar piksela [m/pix]"
        Me.ToolTip1.SetToolTip(Me.Label2, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" & _
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'TextBox10
        '
        Me.TextBox10.Location = New System.Drawing.Point(247, 23)
        Me.TextBox10.Name = "TextBox10"
        Me.TextBox10.Size = New System.Drawing.Size(44, 20)
        Me.TextBox10.TabIndex = 212
        Me.TextBox10.Text = "1"
        Me.TextBox10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'ToolTip1
        '
        Me.ToolTip1.IsBalloon = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.Label23)
        Me.GroupBox3.Controls.Add(Me.Label24)
        Me.GroupBox3.Controls.Add(Me.Label25)
        Me.GroupBox3.Controls.Add(Me.Label26)
        Me.GroupBox3.Controls.Add(Me.Label27)
        Me.GroupBox3.Controls.Add(Me.Label28)
        Me.GroupBox3.Controls.Add(Me.Label16)
        Me.GroupBox3.Controls.Add(Me.Label12)
        Me.GroupBox3.Controls.Add(Me.Label15)
        Me.GroupBox3.Controls.Add(Me.Label14)
        Me.GroupBox3.Controls.Add(Me.Label11)
        Me.GroupBox3.Controls.Add(Me.Label13)
        Me.GroupBox3.Controls.Add(Me.PictureBox1)
        Me.GroupBox3.Controls.Add(Me.Button4)
        Me.GroupBox3.Location = New System.Drawing.Point(662, 279)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(210, 234)
        Me.GroupBox3.TabIndex = 268
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Wybrane warstwy oraz ich kolejność"
        Me.ToolTip1.SetToolTip(Me.GroupBox3, """Kanapka"" warstw. Pierwsza warstwa to bazowy podkład, który można przykryć kolejn" & _
        "ymi warstwami o wyższym numerze, o ile mają one przeźroczyste tło, lub mają niep" & _
        "ełne pokrycie.")
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(1, 16)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(22, 13)
        Me.Label23.TabIndex = 229
        Me.Label23.Text = "12)"
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Location = New System.Drawing.Point(6, 78)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(16, 13)
        Me.Label24.TabIndex = 225
        Me.Label24.Text = "8)"
        '
        'Label25
        '
        Me.Label25.AutoSize = True
        Me.Label25.Location = New System.Drawing.Point(1, 31)
        Me.Label25.Name = "Label25"
        Me.Label25.Size = New System.Drawing.Size(22, 13)
        Me.Label25.TabIndex = 228
        Me.Label25.Text = "11)"
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(1, 47)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(22, 13)
        Me.Label26.TabIndex = 227
        Me.Label26.Text = "10)"
        '
        'Label27
        '
        Me.Label27.AutoSize = True
        Me.Label27.Location = New System.Drawing.Point(6, 93)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(16, 13)
        Me.Label27.TabIndex = 224
        Me.Label27.Text = "7)"
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Location = New System.Drawing.Point(6, 63)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(16, 13)
        Me.Label28.TabIndex = 226
        Me.Label28.Text = "9)"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(6, 109)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(16, 13)
        Me.Label16.TabIndex = 223
        Me.Label16.Text = "6)"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(6, 178)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(16, 13)
        Me.Label12.TabIndex = 219
        Me.Label12.Text = "2)"
        Me.ToolTip1.SetToolTip(Me.Label12, "Kolejna nakładana warstwa, która przykryje podkład")
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(6, 126)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(16, 13)
        Me.Label15.TabIndex = 222
        Me.Label15.Text = "5)"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(6, 143)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(16, 13)
        Me.Label14.TabIndex = 221
        Me.Label14.Text = "4)"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(6, 194)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(16, 13)
        Me.Label11.TabIndex = 218
        Me.Label11.Text = "1)"
        Me.ToolTip1.SetToolTip(Me.Label11, "Główna warstwa podkładowa, którą można przykryć kolejnymi warstwami (o ile mają p" & _
        "rzeźroczyste tło lub nie pokrywają całkowicie podkładu)")
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(6, 161)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(16, 13)
        Me.Label13.TabIndex = 220
        Me.Label13.Text = "3)"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = Global.MapoTero.My.Resources.Resources.kanapka_gis1
        Me.PictureBox1.Location = New System.Drawing.Point(81, 93)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(126, 138)
        Me.PictureBox1.TabIndex = 303
        Me.PictureBox1.TabStop = False
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(3, 210)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(52, 21)
        Me.Button4.TabIndex = 302
        Me.Button4.Tag = "Wyczyść listę wprowadzonych warstw"
        Me.Button4.Text = "Resetuj"
        Me.ToolTip1.SetToolTip(Me.Button4, "Kasowanie wprowadzonej listy warstw")
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Button2.Location = New System.Drawing.Point(6, 28)
        Me.Button2.Margin = New System.Windows.Forms.Padding(0)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(23, 31)
        Me.Button2.TabIndex = 295
        Me.Button2.Text = "+"
        Me.ToolTip1.SetToolTip(Me.Button2, "Powiększ")
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button5
        '
        Me.Button5.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Button5.Location = New System.Drawing.Point(6, 74)
        Me.Button5.Margin = New System.Windows.Forms.Padding(0)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(23, 30)
        Me.Button5.TabIndex = 296
        Me.Button5.Text = "-"
        Me.ToolTip1.SetToolTip(Me.Button5, "Powiększ")
        Me.Button5.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.Label22)
        Me.GroupBox4.Controls.Add(Me.Label19)
        Me.GroupBox4.Controls.Add(Me.Label18)
        Me.GroupBox4.Controls.Add(Me.TextBox12)
        Me.GroupBox4.Controls.Add(Me.Label21)
        Me.GroupBox4.Controls.Add(Me.TextBox11)
        Me.GroupBox4.Controls.Add(Me.Label20)
        Me.GroupBox4.Controls.Add(Me.TextBox7)
        Me.GroupBox4.Controls.Add(Me.TextBox8)
        Me.GroupBox4.Controls.Add(Me.Label9)
        Me.GroupBox4.Controls.Add(Me.Label17)
        Me.GroupBox4.Controls.Add(Me.Label10)
        Me.GroupBox4.Controls.Add(Me.TextBox5)
        Me.GroupBox4.Controls.Add(Me.TextBox6)
        Me.GroupBox4.Enabled = False
        Me.GroupBox4.Location = New System.Drawing.Point(432, 519)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(224, 141)
        Me.GroupBox4.TabIndex = 300
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Wynikowy rozmiar siatki segmentów"
        Me.ToolTip1.SetToolTip(Me.GroupBox4, "Informacje o wynikowym rozmiarze siatki kwadratów na które zostanie podzielony ca" & _
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
        Me.Label19.Location = New System.Drawing.Point(106, 74)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(12, 13)
        Me.Label19.TabIndex = 241
        Me.Label19.Text = "x"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Enabled = False
        Me.Label18.Location = New System.Drawing.Point(106, 43)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(12, 13)
        Me.Label18.TabIndex = 240
        Me.Label18.Text = "x"
        '
        'TextBox12
        '
        Me.TextBox12.Enabled = False
        Me.TextBox12.Location = New System.Drawing.Point(118, 108)
        Me.TextBox12.Name = "TextBox12"
        Me.TextBox12.Size = New System.Drawing.Size(55, 20)
        Me.TextBox12.TabIndex = 239
        Me.TextBox12.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
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
        'TextBox11
        '
        Me.TextBox11.Enabled = False
        Me.TextBox11.Location = New System.Drawing.Point(49, 108)
        Me.TextBox11.Name = "TextBox11"
        Me.TextBox11.Size = New System.Drawing.Size(55, 20)
        Me.TextBox11.TabIndex = 237
        Me.TextBox11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(21, 75)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(26, 13)
        Me.Label20.TabIndex = 235
        Me.Label20.Text = "[pix]"
        '
        'TextBox7
        '
        Me.TextBox7.Enabled = False
        Me.TextBox7.Location = New System.Drawing.Point(119, 72)
        Me.TextBox7.Name = "TextBox7"
        Me.TextBox7.Size = New System.Drawing.Size(55, 20)
        Me.TextBox7.TabIndex = 234
        Me.TextBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TextBox8
        '
        Me.TextBox8.Enabled = False
        Me.TextBox8.Location = New System.Drawing.Point(49, 72)
        Me.TextBox8.Name = "TextBox8"
        Me.TextBox8.Size = New System.Drawing.Size(55, 20)
        Me.TextBox8.TabIndex = 233
        Me.TextBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(5, 23)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(84, 13)
        Me.Label9.TabIndex = 214
        Me.Label9.Text = "Szerokość siatki"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(20, 42)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(27, 13)
        Me.Label17.TabIndex = 231
        Me.Label17.Text = "[km]"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(118, 23)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(84, 13)
        Me.Label10.TabIndex = 215
        Me.Label10.Text = "Wysokość siatki"
        '
        'TextBox5
        '
        Me.TextBox5.Enabled = False
        Me.TextBox5.Location = New System.Drawing.Point(49, 39)
        Me.TextBox5.Name = "TextBox5"
        Me.TextBox5.Size = New System.Drawing.Size(55, 20)
        Me.TextBox5.TabIndex = 216
        Me.TextBox5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TextBox6
        '
        Me.TextBox6.Enabled = False
        Me.TextBox6.Location = New System.Drawing.Point(119, 39)
        Me.TextBox6.Name = "TextBox6"
        Me.TextBox6.Size = New System.Drawing.Size(55, 20)
        Me.TextBox6.TabIndex = 217
        Me.TextBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'button6
        '
        Me.button6.Location = New System.Drawing.Point(662, 636)
        Me.button6.Name = "button6"
        Me.button6.Size = New System.Drawing.Size(99, 21)
        Me.button6.TabIndex = 299
        Me.button6.Text = "Złącz segmenty"
        Me.ToolTip1.SetToolTip(Me.button6, "Po zakończeniu pobierania segmentów możesz je automatycznie scalić w jeden duży p" & _
        "lik.")
        Me.button6.UseVisualStyleBackColor = True
        '
        'GMapControl1
        '
        Me.GMapControl1.Bearing = 0.0!
        Me.GMapControl1.CanDragMap = True
        Me.GMapControl1.Cursor = System.Windows.Forms.Cursors.Hand
        Me.GMapControl1.EmptyTileColor = System.Drawing.Color.Navy
        Me.GMapControl1.GrayScaleMode = False
        Me.GMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow
        Me.GMapControl1.LevelsKeepInMemmory = 5
        Me.GMapControl1.Location = New System.Drawing.Point(4, 26)
        Me.GMapControl1.MarkersEnabled = True
        Me.GMapControl1.MaxZoom = 17
        Me.GMapControl1.MinZoom = 3
        Me.GMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter
        Me.GMapControl1.Name = "GMapControl1"
        Me.GMapControl1.NegativeMode = False
        Me.GMapControl1.PolygonsEnabled = True
        Me.GMapControl1.RetryLoadTile = 0
        Me.GMapControl1.RoutesEnabled = True
        Me.GMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional
        Me.GMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.GMapControl1.ShowTileGridLines = False
        Me.GMapControl1.Size = New System.Drawing.Size(652, 487)
        Me.GMapControl1.TabIndex = 249
        Me.GMapControl1.Zoom = 0.0R
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Checked = True
        Me.RadioButton1.Location = New System.Drawing.Point(545, 29)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(109, 17)
        Me.RadioButton1.TabIndex = 258
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.Text = "OpenStreetMap   "
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Location = New System.Drawing.Point(544, 51)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(110, 17)
        Me.RadioButton2.TabIndex = 259
        Me.RadioButton2.Text = "GoogleMap          "
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'RadioButton3
        '
        Me.RadioButton3.AutoSize = True
        Me.RadioButton3.Location = New System.Drawing.Point(544, 73)
        Me.RadioButton3.Name = "RadioButton3"
        Me.RadioButton3.Size = New System.Drawing.Size(109, 17)
        Me.RadioButton3.TabIndex = 260
        Me.RadioButton3.Text = "SatelliteBingsMap"
        Me.RadioButton3.UseVisualStyleBackColor = True
        '
        'Label35
        '
        Me.Label35.AutoSize = True
        Me.Label35.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label35.Location = New System.Drawing.Point(528, 499)
        Me.Label35.Name = "Label35"
        Me.Label35.Size = New System.Drawing.Size(28, 13)
        Me.Label35.TabIndex = 261
        Me.Label35.Text = "52.3"
        '
        'Label37
        '
        Me.Label37.AutoSize = True
        Me.Label37.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label37.Location = New System.Drawing.Point(455, 499)
        Me.Label37.Name = "Label37"
        Me.Label37.Size = New System.Drawing.Size(75, 13)
        Me.Label37.TabIndex = 263
        Me.Label37.Text = "WGS84  Lat ="
        '
        'Label38
        '
        Me.Label38.AutoSize = True
        Me.Label38.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label38.Location = New System.Drawing.Point(455, 482)
        Me.Label38.Name = "Label38"
        Me.Label38.Size = New System.Drawing.Size(161, 13)
        Me.Label38.TabIndex = 264
        Me.Label38.Text = "Współrzędne środka okna mapy"
        '
        'Label39
        '
        Me.Label39.AutoSize = True
        Me.Label39.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label39.Location = New System.Drawing.Point(6, 481)
        Me.Label39.Name = "Label39"
        Me.Label39.Size = New System.Drawing.Size(109, 13)
        Me.Label39.TabIndex = 265
        Me.Label39.Text = "Współrzędne kursora"
        '
        'Label40
        '
        Me.Label40.AutoSize = True
        Me.Label40.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label40.Location = New System.Drawing.Point(7, 498)
        Me.Label40.Name = "Label40"
        Me.Label40.Size = New System.Drawing.Size(31, 13)
        Me.Label40.TabIndex = 266
        Me.Label40.Text = "1992"
        '
        'Label41
        '
        Me.Label41.AutoSize = True
        Me.Label41.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label41.Location = New System.Drawing.Point(46, 498)
        Me.Label41.Name = "Label41"
        Me.Label41.Size = New System.Drawing.Size(31, 13)
        Me.Label41.TabIndex = 267
        Me.Label41.Text = "1992"
        '
        'Label63
        '
        Me.Label63.AutoSize = True
        Me.Label63.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label63.Location = New System.Drawing.Point(609, 499)
        Me.Label63.Name = "Label63"
        Me.Label63.Size = New System.Drawing.Size(31, 13)
        Me.Label63.TabIndex = 290
        Me.Label63.Text = "19.2 "
        '
        'Label65
        '
        Me.Label65.AutoSize = True
        Me.Label65.Location = New System.Drawing.Point(9, 60)
        Me.Label65.Name = "Label65"
        Me.Label65.Size = New System.Drawing.Size(13, 13)
        Me.Label65.TabIndex = 292
        Me.Label65.Text = "6"
        '
        'Label66
        '
        Me.Label66.AutoSize = True
        Me.Label66.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label66.Location = New System.Drawing.Point(576, 499)
        Me.Label66.Name = "Label66"
        Me.Label66.Size = New System.Drawing.Size(31, 13)
        Me.Label66.TabIndex = 293
        Me.Label66.Text = "Lng="
        '
        'Label67
        '
        Me.Label67.AutoSize = True
        Me.Label67.BackColor = System.Drawing.SystemColors.ControlLight
        Me.Label67.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label67.ForeColor = System.Drawing.SystemColors.ButtonShadow
        Me.Label67.Location = New System.Drawing.Point(355, 3)
        Me.Label67.Name = "Label67"
        Me.Label67.Size = New System.Drawing.Size(300, 15)
        Me.Label67.TabIndex = 297
        Me.Label67.Text = "Zaznacz na mapie obszar prawym  przyciskiem myszy"
        '
        'RichTextBox1
        '
        Me.RichTextBox1.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.RichTextBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.RichTextBox1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.RichTextBox1.Location = New System.Drawing.Point(662, 523)
        Me.RichTextBox1.Name = "RichTextBox1"
        Me.RichTextBox1.Size = New System.Drawing.Size(210, 56)
        Me.RichTextBox1.TabIndex = 301
        Me.RichTextBox1.Text = "Komunikaty:"
        '
        'Label36
        '
        Me.Label36.AutoSize = True
        Me.Label36.Location = New System.Drawing.Point(83, 499)
        Me.Label36.Name = "Label36"
        Me.Label36.Size = New System.Drawing.Size(36, 13)
        Me.Label36.TabIndex = 260
        Me.Label36.Text = "kursor"
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.Label42)
        Me.GroupBox5.Controls.Add(Me.Label47)
        Me.GroupBox5.Controls.Add(Me.Label34)
        Me.GroupBox5.Controls.Add(Me.TextBox13)
        Me.GroupBox5.Controls.Add(Me.Label46)
        Me.GroupBox5.Controls.Add(Me.Label32)
        Me.GroupBox5.Controls.Add(Me.Label33)
        Me.GroupBox5.Controls.Add(Me.Label31)
        Me.GroupBox5.Controls.Add(Me.Label2)
        Me.GroupBox5.Controls.Add(Me.TextBox9)
        Me.GroupBox5.Controls.Add(Me.TextBox10)
        Me.GroupBox5.Controls.Add(Me.Label1)
        Me.GroupBox5.Controls.Add(Me.Label5)
        Me.GroupBox5.Controls.Add(Me.Label6)
        Me.GroupBox5.Controls.Add(Me.TextBox4)
        Me.GroupBox5.Controls.Add(Me.TextBox3)
        Me.GroupBox5.Controls.Add(Me.Label4)
        Me.GroupBox5.Controls.Add(Me.Label3)
        Me.GroupBox5.Controls.Add(Me.TextBox2)
        Me.GroupBox5.Controls.Add(Me.TextBox1)
        Me.GroupBox5.Controls.Add(Me.PictureBox2)
        Me.GroupBox5.Location = New System.Drawing.Point(6, 519)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(420, 141)
        Me.GroupBox5.TabIndex = 303
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "Schemat siatki segmentów i zasięgu zaznaczenia. Rozmiar pojedyńczego segmentu"
        '
        'Label34
        '
        Me.Label34.AutoSize = True
        Me.Label34.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label34.ForeColor = System.Drawing.Color.Navy
        Me.Label34.Location = New System.Drawing.Point(1, 85)
        Me.Label34.Name = "Label34"
        Me.Label34.Size = New System.Drawing.Size(10, 13)
        Me.Label34.TabIndex = 315
        Me.Label34.Text = "."
        '
        'Label32
        '
        Me.Label32.AutoSize = True
        Me.Label32.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label32.ForeColor = System.Drawing.Color.Navy
        Me.Label32.Location = New System.Drawing.Point(69, 123)
        Me.Label32.Name = "Label32"
        Me.Label32.Size = New System.Drawing.Size(10, 13)
        Me.Label32.TabIndex = 314
        Me.Label32.Text = "."
        '
        'Label33
        '
        Me.Label33.AutoSize = True
        Me.Label33.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label33.ForeColor = System.Drawing.Color.Navy
        Me.Label33.Location = New System.Drawing.Point(133, 86)
        Me.Label33.Name = "Label33"
        Me.Label33.Size = New System.Drawing.Size(10, 13)
        Me.Label33.TabIndex = 313
        Me.Label33.Text = "."
        '
        'Label31
        '
        Me.Label31.AutoSize = True
        Me.Label31.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label31.ForeColor = System.Drawing.Color.Navy
        Me.Label31.Location = New System.Drawing.Point(71, 59)
        Me.Label31.Name = "Label31"
        Me.Label31.Size = New System.Drawing.Size(10, 13)
        Me.Label31.TabIndex = 312
        Me.Label31.Text = "."
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.ForeColor = System.Drawing.Color.Blue
        Me.Label5.Location = New System.Drawing.Point(132, 70)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(12, 13)
        Me.Label5.TabIndex = 310
        Me.Label5.Text = "y"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.ForeColor = System.Drawing.Color.Blue
        Me.Label6.Location = New System.Drawing.Point(70, 43)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(12, 13)
        Me.Label6.TabIndex = 311
        Me.Label6.Text = "x"
        '
        'TextBox4
        '
        Me.TextBox4.ForeColor = System.Drawing.Color.Blue
        Me.TextBox4.Location = New System.Drawing.Point(147, 67)
        Me.TextBox4.Name = "TextBox4"
        Me.TextBox4.Size = New System.Drawing.Size(44, 20)
        Me.TextBox4.TabIndex = 309
        Me.TextBox4.Text = "571000"
        '
        'TextBox3
        '
        Me.TextBox3.ForeColor = System.Drawing.Color.Blue
        Me.TextBox3.Location = New System.Drawing.Point(82, 40)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(44, 20)
        Me.TextBox3.TabIndex = 308
        Me.TextBox3.Text = "684000"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.ForeColor = System.Drawing.Color.Blue
        Me.Label4.Location = New System.Drawing.Point(0, 69)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(12, 13)
        Me.Label4.TabIndex = 307
        Me.Label4.Text = "y"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.ForeColor = System.Drawing.Color.Blue
        Me.Label3.Location = New System.Drawing.Point(68, 103)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(12, 13)
        Me.Label3.TabIndex = 306
        Me.Label3.Text = "x"
        '
        'TextBox2
        '
        Me.TextBox2.ForeColor = System.Drawing.Color.Blue
        Me.TextBox2.Location = New System.Drawing.Point(17, 66)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(44, 20)
        Me.TextBox2.TabIndex = 305
        Me.TextBox2.Text = "569000"
        '
        'TextBox1
        '
        Me.TextBox1.ForeColor = System.Drawing.Color.Blue
        Me.TextBox1.Location = New System.Drawing.Point(82, 99)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(44, 20)
        Me.TextBox1.TabIndex = 304
        Me.TextBox1.Text = "682000"
        '
        'PictureBox2
        '
        Me.PictureBox2.Image = Global.MapoTero.My.Resources.Resources.siatka
        Me.PictureBox2.Location = New System.Drawing.Point(10, 16)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(235, 119)
        Me.PictureBox2.TabIndex = 303
        Me.PictureBox2.TabStop = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(876, 682)
        Me.Controls.Add(Me.GroupBox5)
        Me.Controls.Add(Me.Label36)
        Me.Controls.Add(Me.RichTextBox1)
        Me.Controls.Add(Me.Label41)
        Me.Controls.Add(Me.Label40)
        Me.Controls.Add(Me.Label39)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.button6)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label67)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label66)
        Me.Controls.Add(Me.Label65)
        Me.Controls.Add(Me.Label63)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.Label38)
        Me.Controls.Add(Me.Label37)
        Me.Controls.Add(Me.Label35)
        Me.Controls.Add(Me.RadioButton3)
        Me.Controls.Add(Me.RadioButton2)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.GMapControl1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.MenuStrip1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents button6 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel2 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents ToolStripStatusLabel3 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents SesjaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZapiszToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WczytajToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WarstwyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NakładanieWarstwNaSiebieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UstawieniaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PomocPomorskieForumEksploracyjneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents ComboBox3 As System.Windows.Forms.ComboBox
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents InstrukcjaObsługiToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label30 As System.Windows.Forms.Label
    Friend WithEvents Label29 As System.Windows.Forms.Label
    Friend WithEvents UsuwaniePustychSegmentówToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TextBox9 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TextBox10 As System.Windows.Forms.TextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents GMapControl1 As GMap.NET.WindowsForms.GMapControl
    Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton2 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton3 As System.Windows.Forms.RadioButton
    Friend WithEvents Label35 As System.Windows.Forms.Label
    Friend WithEvents Label37 As System.Windows.Forms.Label
    'Friend WithEvents Label36 As System.Windows.Forms.Label
    Friend WithEvents Label38 As System.Windows.Forms.Label
    Friend WithEvents Label39 As System.Windows.Forms.Label
    Friend WithEvents Label40 As System.Windows.Forms.Label
    Friend WithEvents Label41 As System.Windows.Forms.Label
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents Label23 As System.Windows.Forms.Label
    Friend WithEvents Label24 As System.Windows.Forms.Label
    Friend WithEvents Label25 As System.Windows.Forms.Label
    Friend WithEvents Label26 As System.Windows.Forms.Label
    Friend WithEvents Label27 As System.Windows.Forms.Label
    Friend WithEvents Label28 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label63 As System.Windows.Forms.Label
    Friend WithEvents Label65 As System.Windows.Forms.Label
    Friend WithEvents Label66 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button5 As System.Windows.Forms.Button
    Friend WithEvents Label67 As System.Windows.Forms.Label
    Friend WithEvents ScalanieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox12 As System.Windows.Forms.TextBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents TextBox11 As System.Windows.Forms.TextBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents TextBox7 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox8 As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents TextBox5 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox6 As System.Windows.Forms.TextBox
    Friend WithEvents RichTextBox1 As System.Windows.Forms.RichTextBox
    Friend WithEvents Label36 As System.Windows.Forms.Label
    Friend WithEvents TextBox13 As System.Windows.Forms.TextBox
    Friend WithEvents Label42 As System.Windows.Forms.Label
    Friend WithEvents Label47 As System.Windows.Forms.Label
    Friend WithEvents Label46 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents Label34 As System.Windows.Forms.Label
    Friend WithEvents Label32 As System.Windows.Forms.Label
    Friend WithEvents Label33 As System.Windows.Forms.Label
    Friend WithEvents Label31 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox

End Class
