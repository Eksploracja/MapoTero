﻿Imports GMap.NET
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
    	Me.TextBox1 = New System.Windows.Forms.TextBox()
    	Me.TextBox2 = New System.Windows.Forms.TextBox()
    	Me.TextBox3 = New System.Windows.Forms.TextBox()
    	Me.TextBox4 = New System.Windows.Forms.TextBox()
    	Me.GroupBox1 = New System.Windows.Forms.GroupBox()
    	Me.Label33 = New System.Windows.Forms.Label()
    	Me.Label31 = New System.Windows.Forms.Label()
    	Me.Label8 = New System.Windows.Forms.Label()
    	Me.Label7 = New System.Windows.Forms.Label()
    	Me.Label6 = New System.Windows.Forms.Label()
    	Me.Label5 = New System.Windows.Forms.Label()
    	Me.Label4 = New System.Windows.Forms.Label()
    	Me.Label3 = New System.Windows.Forms.Label()
    	Me.Label34 = New System.Windows.Forms.Label()
    	Me.Label32 = New System.Windows.Forms.Label()
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
    	Me.GroupBox7 = New System.Windows.Forms.GroupBox()
    	Me.TextBox9 = New System.Windows.Forms.TextBox()
    	Me.Label1 = New System.Windows.Forms.Label()
    	Me.Label2 = New System.Windows.Forms.Label()
    	Me.TextBox10 = New System.Windows.Forms.TextBox()
    	Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
    	Me.Button4 = New System.Windows.Forms.Button()
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
    	Me.Button2 = New System.Windows.Forms.Button()
    	Me.Button5 = New System.Windows.Forms.Button()
    	Me.GroupBox4 = New System.Windows.Forms.GroupBox()
    	Me.Label22 = New System.Windows.Forms.Label()
    	Me.TextBox12 = New System.Windows.Forms.TextBox()
    	Me.Label21 = New System.Windows.Forms.Label()
    	Me.TextBox11 = New System.Windows.Forms.TextBox()
    	Me.Label20 = New System.Windows.Forms.Label()
    	Me.Label19 = New System.Windows.Forms.Label()
    	Me.TextBox7 = New System.Windows.Forms.TextBox()
    	Me.TextBox8 = New System.Windows.Forms.TextBox()
    	Me.Label18 = New System.Windows.Forms.Label()
    	Me.Label9 = New System.Windows.Forms.Label()
    	Me.Label17 = New System.Windows.Forms.Label()
    	Me.Label10 = New System.Windows.Forms.Label()
    	Me.TextBox5 = New System.Windows.Forms.TextBox()
    	Me.TextBox6 = New System.Windows.Forms.TextBox()
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
    	Me.Label46 = New System.Windows.Forms.Label()
    	Me.Label63 = New System.Windows.Forms.Label()
    	Me.Label65 = New System.Windows.Forms.Label()
    	Me.Label66 = New System.Windows.Forms.Label()
    	Me.Label67 = New System.Windows.Forms.Label()
    	Me.button6 = New System.Windows.Forms.Button()
    	Me.GroupBox1.SuspendLayout
    	Me.GroupBox2.SuspendLayout
    	Me.StatusStrip1.SuspendLayout
    	Me.MenuStrip1.SuspendLayout
    	Me.GroupBox7.SuspendLayout
    	Me.GroupBox3.SuspendLayout
    	Me.GroupBox4.SuspendLayout
    	Me.SuspendLayout
    	'
    	'Button1
    	'
    	Me.Button1.BackColor = System.Drawing.SystemColors.ActiveBorder
    	Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Button1.Location = New System.Drawing.Point(786, 768)
    	Me.Button1.Name = "Button1"
    	Me.Button1.Size = New System.Drawing.Size(121, 65)
    	Me.Button1.TabIndex = 6
    	Me.Button1.Tag = ""
    	Me.Button1.Text = "Pobierz"
    	Me.ToolTip1.SetToolTip(Me.Button1, "Rozpocznij proces pobierania mapy.")
    	Me.Button1.UseVisualStyleBackColor = false
    	'
    	'TextBox1
    	'
    	Me.TextBox1.Location = New System.Drawing.Point(30, 91)
    	Me.TextBox1.Name = "TextBox1"
    	Me.TextBox1.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox1.TabIndex = 0
    	Me.TextBox1.Text = "682000"
    	'
    	'TextBox2
    	'
    	Me.TextBox2.Location = New System.Drawing.Point(117, 91)
    	Me.TextBox2.Name = "TextBox2"
    	Me.TextBox2.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox2.TabIndex = 1
    	Me.TextBox2.Text = "569000"
    	'
    	'TextBox3
    	'
    	Me.TextBox3.Location = New System.Drawing.Point(30, 40)
    	Me.TextBox3.Name = "TextBox3"
    	Me.TextBox3.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox3.TabIndex = 2
    	Me.TextBox3.Text = "684000"
    	'
    	'TextBox4
    	'
    	Me.TextBox4.Location = New System.Drawing.Point(117, 40)
    	Me.TextBox4.Name = "TextBox4"
    	Me.TextBox4.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox4.TabIndex = 3
    	Me.TextBox4.Text = "571000"
    	'
    	'GroupBox1
    	'
    	Me.GroupBox1.Controls.Add(Me.Label33)
    	Me.GroupBox1.Controls.Add(Me.Label31)
    	Me.GroupBox1.Controls.Add(Me.Label8)
    	Me.GroupBox1.Controls.Add(Me.Label7)
    	Me.GroupBox1.Controls.Add(Me.Label6)
    	Me.GroupBox1.Controls.Add(Me.Label5)
    	Me.GroupBox1.Controls.Add(Me.Label4)
    	Me.GroupBox1.Controls.Add(Me.Label3)
    	Me.GroupBox1.Controls.Add(Me.Label34)
    	Me.GroupBox1.Controls.Add(Me.TextBox1)
    	Me.GroupBox1.Controls.Add(Me.TextBox2)
    	Me.GroupBox1.Controls.Add(Me.Label32)
    	Me.GroupBox1.Controls.Add(Me.TextBox3)
    	Me.GroupBox1.Controls.Add(Me.TextBox4)
    	Me.GroupBox1.Location = New System.Drawing.Point(443, 627)
    	Me.GroupBox1.Name = "GroupBox1"
    	Me.GroupBox1.Size = New System.Drawing.Size(198, 133)
    	Me.GroupBox1.TabIndex = 207
    	Me.GroupBox1.TabStop = false
    	Me.GroupBox1.Text = "3. Współrzędne XY zaznaczenia"
    	Me.ToolTip1.SetToolTip(Me.GroupBox1, "Zasięg pobieranej mapy określamy prostokątem. Wystarczy zdefiniować dwa narożniki"& _ 
    	    	" tego prostokąta, przepisując z Geoportalu współrzędne lewego-dolnego oraz prawe"& _ 
    	    	"go-górnego narożnika.")
    	'
    	'Label33
    	'
    	Me.Label33.AutoSize = true
    	Me.Label33.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label33.Location = New System.Drawing.Point(121, 60)
    	Me.Label33.Name = "Label33"
    	Me.Label33.Size = New System.Drawing.Size(10, 13)
    	Me.Label33.TabIndex = 259
    	Me.Label33.Text = "."
    	'
    	'Label31
    	'
    	Me.Label31.AutoSize = true
    	Me.Label31.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label31.Location = New System.Drawing.Point(37, 60)
    	Me.Label31.Name = "Label31"
    	Me.Label31.Size = New System.Drawing.Size(10, 13)
    	Me.Label31.TabIndex = 258
    	Me.Label31.Text = "."
    	'
    	'Label8
    	'
    	Me.Label8.AutoSize = true
    	Me.Label8.Location = New System.Drawing.Point(15, 24)
    	Me.Label8.Name = "Label8"
    	Me.Label8.Size = New System.Drawing.Size(148, 13)
    	Me.Label8.TabIndex = 215
    	Me.Label8.Text = "Prawy górny narożnik obszaru"
    	'
    	'Label7
    	'
    	Me.Label7.AutoSize = true
    	Me.Label7.Location = New System.Drawing.Point(12, 79)
    	Me.Label7.Name = "Label7"
    	Me.Label7.Size = New System.Drawing.Size(146, 13)
    	Me.Label7.TabIndex = 214
    	Me.Label7.Text = "Lewy dolny narożnik obszaru:"
    	'
    	'Label6
    	'
    	Me.Label6.AutoSize = true
    	Me.Label6.Location = New System.Drawing.Point(16, 43)
    	Me.Label6.Name = "Label6"
    	Me.Label6.Size = New System.Drawing.Size(14, 13)
    	Me.Label6.TabIndex = 214
    	Me.Label6.Text = "X"
    	'
    	'Label5
    	'
    	Me.Label5.AutoSize = true
    	Me.Label5.Location = New System.Drawing.Point(102, 43)
    	Me.Label5.Name = "Label5"
    	Me.Label5.Size = New System.Drawing.Size(14, 13)
    	Me.Label5.TabIndex = 213
    	Me.Label5.Text = "Y"
    	'
    	'Label4
    	'
    	Me.Label4.AutoSize = true
    	Me.Label4.Location = New System.Drawing.Point(102, 93)
    	Me.Label4.Name = "Label4"
    	Me.Label4.Size = New System.Drawing.Size(14, 13)
    	Me.Label4.TabIndex = 212
    	Me.Label4.Text = "Y"
    	'
    	'Label3
    	'
    	Me.Label3.AutoSize = true
    	Me.Label3.Location = New System.Drawing.Point(15, 94)
    	Me.Label3.Name = "Label3"
    	Me.Label3.Size = New System.Drawing.Size(14, 13)
    	Me.Label3.TabIndex = 211
    	Me.Label3.Text = "X"
    	'
    	'Label34
    	'
    	Me.Label34.AutoSize = true
    	Me.Label34.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label34.Location = New System.Drawing.Point(118, 111)
    	Me.Label34.Name = "Label34"
    	Me.Label34.Size = New System.Drawing.Size(10, 13)
    	Me.Label34.TabIndex = 257
    	Me.Label34.Text = "."
    	'
    	'Label32
    	'
    	Me.Label32.AutoSize = true
    	Me.Label32.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label32.Location = New System.Drawing.Point(33, 111)
    	Me.Label32.Name = "Label32"
    	Me.Label32.Size = New System.Drawing.Size(10, 13)
    	Me.Label32.TabIndex = 255
    	Me.Label32.Text = "."
    	'
    	'GroupBox2
    	'
    	Me.GroupBox2.Controls.Add(Me.Label30)
    	Me.GroupBox2.Controls.Add(Me.Label29)
    	Me.GroupBox2.Controls.Add(Me.ListBox1)
    	Me.GroupBox2.Controls.Add(Me.ComboBox3)
    	Me.GroupBox2.Location = New System.Drawing.Point(11, 627)
    	Me.GroupBox2.Name = "GroupBox2"
    	Me.GroupBox2.Size = New System.Drawing.Size(221, 254)
    	Me.GroupBox2.TabIndex = 210
    	Me.GroupBox2.TabStop = false
    	Me.GroupBox2.Text = "1. Dane źródłowe"
    	'
    	'Label30
    	'
    	Me.Label30.AutoSize = true
    	Me.Label30.Location = New System.Drawing.Point(6, 58)
    	Me.Label30.Name = "Label30"
    	Me.Label30.Size = New System.Drawing.Size(156, 13)
    	Me.Label30.TabIndex = 217
    	Me.Label30.Text = "B. Wybierz warstwy danej mapy"
    	'
    	'Label29
    	'
    	Me.Label29.AutoSize = true
    	Me.Label29.Location = New System.Drawing.Point(6, 16)
    	Me.Label29.Name = "Label29"
    	Me.Label29.Size = New System.Drawing.Size(117, 13)
    	Me.Label29.TabIndex = 216
    	Me.Label29.Text = "A. Wybierz rodzaj mapy"
    	'
    	'ListBox1
    	'
    	Me.ListBox1.FormattingEnabled = true
    	Me.ListBox1.Location = New System.Drawing.Point(6, 72)
    	Me.ListBox1.Name = "ListBox1"
    	Me.ListBox1.Size = New System.Drawing.Size(209, 173)
    	Me.ListBox1.TabIndex = 216
    	Me.ToolTip1.SetToolTip(Me.ListBox1, resources.GetString("ListBox1.ToolTip"))
    	'
    	'ComboBox3
    	'
    	Me.ComboBox3.FormattingEnabled = true
    	Me.ComboBox3.Location = New System.Drawing.Point(6, 29)
    	Me.ComboBox3.MaxDropDownItems = 18
    	Me.ComboBox3.Name = "ComboBox3"
    	Me.ComboBox3.Size = New System.Drawing.Size(209, 21)
    	Me.ComboBox3.TabIndex = 216
    	Me.ComboBox3.Text = "skany_map_topograficznych"
    	Me.ToolTip1.SetToolTip(Me.ComboBox3, "Spis dostępnych serwerów WMS, których definicje znajdują się w plikach tekstowych"& _ 
    	    	" katalogu /warstwy/")
    	'
    	'Panel1
    	'
    	Me.Panel1.AutoSize = true
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
    	Me.StatusStrip1.Location = New System.Drawing.Point(0, 884)
    	Me.StatusStrip1.Name = "StatusStrip1"
    	Me.StatusStrip1.Size = New System.Drawing.Size(914, 22)
    	Me.StatusStrip1.TabIndex = 237
    	Me.StatusStrip1.Text = "StatusStrip1"
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
    	Me.Button3.Enabled = false
    	Me.Button3.Location = New System.Drawing.Point(786, 850)
    	Me.Button3.Name = "Button3"
    	Me.Button3.Size = New System.Drawing.Size(119, 25)
    	Me.Button3.TabIndex = 245
    	Me.Button3.Tag = ""
    	Me.Button3.Text = "Przerwij pobieranie"
    	Me.ToolTip1.SetToolTip(Me.Button3, "Anuluj rozpoczęty proces pobierania mapy")
    	Me.Button3.UseVisualStyleBackColor = true
    	'
    	'SesjaToolStripMenuItem
    	'
    	Me.SesjaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZapiszToolStripMenuItem, Me.WczytajToolStripMenuItem1})
    	Me.SesjaToolStripMenuItem.Name = "SesjaToolStripMenuItem"
    	Me.SesjaToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.S),System.Windows.Forms.Keys)
    	Me.SesjaToolStripMenuItem.Size = New System.Drawing.Size(98, 20)
    	Me.SesjaToolStripMenuItem.Text = "Sesja pobierania"
    	Me.SesjaToolStripMenuItem.ToolTipText = "Wczytaj lub zapisz plik conf.txt, który przechowuje informacje m.in. o zasięgu po"& _ 
    	"bieranego obszaru"
    	'
    	'ZapiszToolStripMenuItem
    	'
    	Me.ZapiszToolStripMenuItem.Name = "ZapiszToolStripMenuItem"
    	Me.ZapiszToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Z),System.Windows.Forms.Keys)
    	Me.ZapiszToolStripMenuItem.Size = New System.Drawing.Size(277, 22)
    	Me.ZapiszToolStripMenuItem.Text = "zapisz ustawienia sesji (conf.txt)"
    	'
    	'WczytajToolStripMenuItem1
    	'
    	Me.WczytajToolStripMenuItem1.Name = "WczytajToolStripMenuItem1"
    	Me.WczytajToolStripMenuItem1.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.W),System.Windows.Forms.Keys)
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
    	Me.ScalanieToolStripMenuItem.Size = New System.Drawing.Size(250, 22)
    	Me.ScalanieToolStripMenuItem.Text = "Łączenie segmentów"
    	'
    	'NakładanieWarstwNaSiebieToolStripMenuItem
    	'
    	Me.NakładanieWarstwNaSiebieToolStripMenuItem.Name = "NakładanieWarstwNaSiebieToolStripMenuItem"
    	Me.NakładanieWarstwNaSiebieToolStripMenuItem.Size = New System.Drawing.Size(250, 22)
    	Me.NakładanieWarstwNaSiebieToolStripMenuItem.Text = "Nakładanie dwóch map png na siebie"
    	'
    	'UsuwaniePustychSegmentówToolStripMenuItem
    	'
    	Me.UsuwaniePustychSegmentówToolStripMenuItem.Name = "UsuwaniePustychSegmentówToolStripMenuItem"
    	Me.UsuwaniePustychSegmentówToolStripMenuItem.Size = New System.Drawing.Size(250, 22)
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
    	Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(40, 20)
    	Me.HelpToolStripMenuItem.Text = "Help"
    	'
    	'AboutToolStripMenuItem
    	'
    	Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
    	Me.AboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
    	Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(284, 22)
    	Me.AboutToolStripMenuItem.Text = "About"
    	'
    	'InstrukcjaObsługiToolStripMenuItem
    	'
    	Me.InstrukcjaObsługiToolStripMenuItem.Name = "InstrukcjaObsługiToolStripMenuItem"
    	Me.InstrukcjaObsługiToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2
    	Me.InstrukcjaObsługiToolStripMenuItem.Size = New System.Drawing.Size(284, 22)
    	Me.InstrukcjaObsługiToolStripMenuItem.Text = "Podstawowa instrukcja obsługi"
    	'
    	'PomocPomorskieForumEksploracyjneToolStripMenuItem
    	'
    	Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Name = "PomocPomorskieForumEksploracyjneToolStripMenuItem"
    	Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3
    	Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Size = New System.Drawing.Size(284, 22)
    	Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Text = "Pomoc - Pomorskie Forum Eksploracyjne"
    	'
    	'MenuStrip1
    	'
    	Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SesjaToolStripMenuItem, Me.WarstwyToolStripMenuItem, Me.UstawieniaToolStripMenuItem, Me.HelpToolStripMenuItem})
    	Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
    	Me.MenuStrip1.Name = "MenuStrip1"
    	Me.MenuStrip1.Size = New System.Drawing.Size(914, 24)
    	Me.MenuStrip1.TabIndex = 230
    	Me.MenuStrip1.Text = "MenuStrip1"
    	'
    	'GroupBox7
    	'
    	Me.GroupBox7.Controls.Add(Me.TextBox9)
    	Me.GroupBox7.Controls.Add(Me.Label1)
    	Me.GroupBox7.Controls.Add(Me.Label2)
    	Me.GroupBox7.Controls.Add(Me.TextBox10)
    	Me.GroupBox7.Location = New System.Drawing.Point(443, 766)
    	Me.GroupBox7.Name = "GroupBox7"
    	Me.GroupBox7.Size = New System.Drawing.Size(198, 115)
    	Me.GroupBox7.TabIndex = 247
    	Me.GroupBox7.TabStop = false
    	Me.GroupBox7.Text = "4. Opcjonalna zmiana rozmiaru segm."
    	Me.ToolTip1.SetToolTip(Me.GroupBox7, "Pobierany obszar zostanie podzielony na siatkę segmentów o rozmiarze 2000x2000px."& _ 
    	    	" Domyślnie wprowadzone parametry zapewniają optymalną jakość obrazu.")
    	'
    	'TextBox9
    	'
    	Me.TextBox9.Location = New System.Drawing.Point(30, 80)
    	Me.TextBox9.Name = "TextBox9"
    	Me.TextBox9.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox9.TabIndex = 211
    	Me.TextBox9.Text = "2000"
    	Me.ToolTip1.SetToolTip(Me.TextBox9, "Rozmiar pojedyńczego segmentu - najmniejszej komórki siatki kwadtatów, na które z"& _ 
    	    	"ostanie podzielony pobierany obszar mapy.  Uwaga - maksymalna rozmiar segmentu d"& _ 
    	    	"la Geoportalu2 wynosi 2048px")
    	'
    	'Label1
    	'
    	Me.Label1.AutoSize = true
    	Me.Label1.Location = New System.Drawing.Point(7, 65)
    	Me.Label1.Name = "Label1"
    	Me.Label1.Size = New System.Drawing.Size(128, 13)
    	Me.Label1.TabIndex = 213
    	Me.Label1.Text = "Długość boku segm. [pix]"
    	'
    	'Label2
    	'
    	Me.Label2.AutoSize = true
    	Me.Label2.Location = New System.Drawing.Point(7, 18)
    	Me.Label2.Name = "Label2"
    	Me.Label2.Size = New System.Drawing.Size(135, 13)
    	Me.Label2.TabIndex = 214
    	Me.Label2.Text = "Wielkość piksela segm. [m]"
    	'
    	'TextBox10
    	'
    	Me.TextBox10.Location = New System.Drawing.Point(30, 33)
    	Me.TextBox10.Name = "TextBox10"
    	Me.TextBox10.Size = New System.Drawing.Size(44, 20)
    	Me.TextBox10.TabIndex = 212
    	Me.TextBox10.Text = "1"
    	Me.ToolTip1.SetToolTip(Me.TextBox10, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p"& _ 
    	    	"ojedynczego piksela, tym gorsza jakość obrazu.")
    	'
    	'ToolTip1
    	'
    	Me.ToolTip1.IsBalloon = true
    	'
    	'Button4
    	'
    	Me.Button4.Location = New System.Drawing.Point(108, 226)
    	Me.Button4.Name = "Button4"
    	Me.Button4.Size = New System.Drawing.Size(57, 22)
    	Me.Button4.TabIndex = 246
    	Me.Button4.Tag = "Wyczyść listę wprowadzonych warstw"
    	Me.Button4.Text = "Resetuj"
    	Me.ToolTip1.SetToolTip(Me.Button4, "Kasowanie wprowadzonej listy warstw")
    	Me.Button4.UseVisualStyleBackColor = true
    	'
    	'GroupBox3
    	'
    	Me.GroupBox3.Controls.Add(Me.Label23)
    	Me.GroupBox3.Controls.Add(Me.Label24)
    	Me.GroupBox3.Controls.Add(Me.Button4)
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
    	Me.GroupBox3.Location = New System.Drawing.Point(251, 627)
    	Me.GroupBox3.Name = "GroupBox3"
    	Me.GroupBox3.Size = New System.Drawing.Size(171, 254)
    	Me.GroupBox3.TabIndex = 268
    	Me.GroupBox3.TabStop = false
    	Me.GroupBox3.Text = "2. Wybrane warstwy"
    	Me.ToolTip1.SetToolTip(Me.GroupBox3, """Kanapka"" warstw. Pierwsza warstwa to bazowy podkład, który można przykryć kolejn"& _ 
    	    	"ymi warstwami o wyższym numerze, o ile mają one przeźroczyste tło, lub mają niep"& _ 
    	    	"ełne pokrycie.")
    	'
    	'Label23
    	'
    	Me.Label23.AutoSize = true
    	Me.Label23.Location = New System.Drawing.Point(6, 210)
    	Me.Label23.Name = "Label23"
    	Me.Label23.Size = New System.Drawing.Size(22, 13)
    	Me.Label23.TabIndex = 229
    	Me.Label23.Text = "12)"
    	'
    	'Label24
    	'
    	Me.Label24.AutoSize = true
    	Me.Label24.Location = New System.Drawing.Point(12, 141)
    	Me.Label24.Name = "Label24"
    	Me.Label24.Size = New System.Drawing.Size(16, 13)
    	Me.Label24.TabIndex = 225
    	Me.Label24.Text = "8)"
    	'
    	'Label25
    	'
    	Me.Label25.AutoSize = true
    	Me.Label25.Location = New System.Drawing.Point(6, 193)
    	Me.Label25.Name = "Label25"
    	Me.Label25.Size = New System.Drawing.Size(22, 13)
    	Me.Label25.TabIndex = 228
    	Me.Label25.Text = "11)"
    	'
    	'Label26
    	'
    	Me.Label26.AutoSize = true
    	Me.Label26.Location = New System.Drawing.Point(6, 176)
    	Me.Label26.Name = "Label26"
    	Me.Label26.Size = New System.Drawing.Size(22, 13)
    	Me.Label26.TabIndex = 227
    	Me.Label26.Text = "10)"
    	'
    	'Label27
    	'
    	Me.Label27.AutoSize = true
    	Me.Label27.Location = New System.Drawing.Point(12, 124)
    	Me.Label27.Name = "Label27"
    	Me.Label27.Size = New System.Drawing.Size(16, 13)
    	Me.Label27.TabIndex = 224
    	Me.Label27.Text = "7)"
    	'
    	'Label28
    	'
    	Me.Label28.AutoSize = true
    	Me.Label28.Location = New System.Drawing.Point(12, 159)
    	Me.Label28.Name = "Label28"
    	Me.Label28.Size = New System.Drawing.Size(16, 13)
    	Me.Label28.TabIndex = 226
    	Me.Label28.Text = "9)"
    	'
    	'Label16
    	'
    	Me.Label16.AutoSize = true
    	Me.Label16.Location = New System.Drawing.Point(12, 107)
    	Me.Label16.Name = "Label16"
    	Me.Label16.Size = New System.Drawing.Size(16, 13)
    	Me.Label16.TabIndex = 223
    	Me.Label16.Text = "6)"
    	'
    	'Label12
    	'
    	Me.Label12.AutoSize = true
    	Me.Label12.Location = New System.Drawing.Point(12, 38)
    	Me.Label12.Name = "Label12"
    	Me.Label12.Size = New System.Drawing.Size(16, 13)
    	Me.Label12.TabIndex = 219
    	Me.Label12.Text = "2)"
    	Me.ToolTip1.SetToolTip(Me.Label12, "Kolejna nakładana warstwa, która przykryje podkład")
    	'
    	'Label15
    	'
    	Me.Label15.AutoSize = true
    	Me.Label15.Location = New System.Drawing.Point(12, 90)
    	Me.Label15.Name = "Label15"
    	Me.Label15.Size = New System.Drawing.Size(16, 13)
    	Me.Label15.TabIndex = 222
    	Me.Label15.Text = "5)"
    	'
    	'Label14
    	'
    	Me.Label14.AutoSize = true
    	Me.Label14.Location = New System.Drawing.Point(12, 73)
    	Me.Label14.Name = "Label14"
    	Me.Label14.Size = New System.Drawing.Size(16, 13)
    	Me.Label14.TabIndex = 221
    	Me.Label14.Text = "4)"
    	'
    	'Label11
    	'
    	Me.Label11.AutoSize = true
    	Me.Label11.Location = New System.Drawing.Point(12, 21)
    	Me.Label11.Name = "Label11"
    	Me.Label11.Size = New System.Drawing.Size(16, 13)
    	Me.Label11.TabIndex = 218
    	Me.Label11.Text = "1)"
    	Me.ToolTip1.SetToolTip(Me.Label11, "Główna warstwa podkładowa, którą można przykryć kolejnymi warstwami (o ile mają p"& _ 
    	    	"rzeźroczyste tło lub nie pokrywają całkowicie podkładu)")
    	'
    	'Label13
    	'
    	Me.Label13.AutoSize = true
    	Me.Label13.Location = New System.Drawing.Point(12, 56)
    	Me.Label13.Name = "Label13"
    	Me.Label13.Size = New System.Drawing.Size(16, 13)
    	Me.Label13.TabIndex = 220
    	Me.Label13.Text = "3)"
    	'
    	'Button2
    	'
    	Me.Button2.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Button2.Location = New System.Drawing.Point(9, 36)
    	Me.Button2.Margin = New System.Windows.Forms.Padding(0)
    	Me.Button2.Name = "Button2"
    	Me.Button2.Size = New System.Drawing.Size(23, 31)
    	Me.Button2.TabIndex = 295
    	Me.Button2.Text = "+"
    	Me.ToolTip1.SetToolTip(Me.Button2, "Powiększ")
    	Me.Button2.UseVisualStyleBackColor = true
    	'
    	'Button5
    	'
    	Me.Button5.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Button5.Location = New System.Drawing.Point(9, 82)
    	Me.Button5.Margin = New System.Windows.Forms.Padding(0)
    	Me.Button5.Name = "Button5"
    	Me.Button5.Size = New System.Drawing.Size(23, 30)
    	Me.Button5.TabIndex = 296
    	Me.Button5.Text = "-"
    	Me.ToolTip1.SetToolTip(Me.Button5, "Powiększ")
    	Me.Button5.UseVisualStyleBackColor = true
    	'
    	'GroupBox4
    	'
    	Me.GroupBox4.Controls.Add(Me.Label22)
    	Me.GroupBox4.Controls.Add(Me.TextBox12)
    	Me.GroupBox4.Controls.Add(Me.Label21)
    	Me.GroupBox4.Controls.Add(Me.TextBox11)
    	Me.GroupBox4.Controls.Add(Me.Label20)
    	Me.GroupBox4.Controls.Add(Me.Label19)
    	Me.GroupBox4.Controls.Add(Me.TextBox7)
    	Me.GroupBox4.Controls.Add(Me.TextBox8)
    	Me.GroupBox4.Controls.Add(Me.Label18)
    	Me.GroupBox4.Controls.Add(Me.Label9)
    	Me.GroupBox4.Controls.Add(Me.Label17)
    	Me.GroupBox4.Controls.Add(Me.Label10)
    	Me.GroupBox4.Controls.Add(Me.TextBox5)
    	Me.GroupBox4.Controls.Add(Me.TextBox6)
    	Me.GroupBox4.Location = New System.Drawing.Point(658, 627)
    	Me.GroupBox4.Name = "GroupBox4"
    	Me.GroupBox4.Size = New System.Drawing.Size(249, 133)
    	Me.GroupBox4.TabIndex = 300
    	Me.GroupBox4.TabStop = false
    	Me.GroupBox4.Text = "5. Wynikowy rozmiar pobieranego obszaru"
    	Me.ToolTip1.SetToolTip(Me.GroupBox4, "Informacje o wynikowym rozmiarze siatki kwadratów na które zostanie podzielony ca"& _ 
    	    	"ły obszar pobieranej mapy")
    	'
    	'Label22
    	'
    	Me.Label22.AutoSize = true
    	Me.Label22.Location = New System.Drawing.Point(131, 103)
    	Me.Label22.Name = "Label22"
    	Me.Label22.Size = New System.Drawing.Size(38, 13)
    	Me.Label22.TabIndex = 240
    	Me.Label22.Text = "[segm]"
    	'
    	'TextBox12
    	'
    	Me.TextBox12.Enabled = false
    	Me.TextBox12.Location = New System.Drawing.Point(170, 99)
    	Me.TextBox12.Name = "TextBox12"
    	Me.TextBox12.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox12.TabIndex = 239
    	Me.TextBox12.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox12, "Wysokość siatki kwadratów - liczba segmentów w pionie, na który zostanie podzielo"& _ 
    	    	"ny cały obszar pobieranej mapy")
    	'
    	'Label21
    	'
    	Me.Label21.AutoSize = true
    	Me.Label21.Location = New System.Drawing.Point(12, 101)
    	Me.Label21.Name = "Label21"
    	Me.Label21.Size = New System.Drawing.Size(38, 13)
    	Me.Label21.TabIndex = 238
    	Me.Label21.Text = "[segm]"
    	'
    	'TextBox11
    	'
    	Me.TextBox11.Enabled = false
    	Me.TextBox11.Location = New System.Drawing.Point(53, 99)
    	Me.TextBox11.Name = "TextBox11"
    	Me.TextBox11.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox11.TabIndex = 237
    	Me.TextBox11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox11, "Szerokość siatki kwadratów - liczba segmentów w poziomie na który zostanie podzie"& _ 
    	    	"lony cały obszar pobieranej mapy")
    	'
    	'Label20
    	'
    	Me.Label20.AutoSize = true
    	Me.Label20.Location = New System.Drawing.Point(24, 73)
    	Me.Label20.Name = "Label20"
    	Me.Label20.Size = New System.Drawing.Size(26, 13)
    	Me.Label20.TabIndex = 235
    	Me.Label20.Text = "[pix]"
    	'
    	'Label19
    	'
    	Me.Label19.AutoSize = true
    	Me.Label19.Location = New System.Drawing.Point(142, 74)
    	Me.Label19.Name = "Label19"
    	Me.Label19.Size = New System.Drawing.Size(26, 13)
    	Me.Label19.TabIndex = 236
    	Me.Label19.Text = "[pix]"
    	'
    	'TextBox7
    	'
    	Me.TextBox7.Enabled = false
    	Me.TextBox7.Location = New System.Drawing.Point(170, 70)
    	Me.TextBox7.Name = "TextBox7"
    	Me.TextBox7.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox7.TabIndex = 234
    	Me.TextBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox7, "Całkowota wysokość pobieranego obszaru wyrażona w pikselach")
    	'
    	'TextBox8
    	'
    	Me.TextBox8.Enabled = false
    	Me.TextBox8.Location = New System.Drawing.Point(52, 70)
    	Me.TextBox8.Name = "TextBox8"
    	Me.TextBox8.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox8.TabIndex = 233
    	Me.TextBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox8, "Całkowota szerokość pobieranego obszaru wyrażona w pikselach")
    	'
    	'Label18
    	'
    	Me.Label18.AutoSize = true
    	Me.Label18.Location = New System.Drawing.Point(142, 42)
    	Me.Label18.Name = "Label18"
    	Me.Label18.Size = New System.Drawing.Size(27, 13)
    	Me.Label18.TabIndex = 232
    	Me.Label18.Text = "[km]"
    	'
    	'Label9
    	'
    	Me.Label9.AutoSize = true
    	Me.Label9.Location = New System.Drawing.Point(12, 23)
    	Me.Label9.Name = "Label9"
    	Me.Label9.Size = New System.Drawing.Size(104, 13)
    	Me.Label9.TabIndex = 214
    	Me.Label9.Text = "Dł. boku w poziomie"
    	'
    	'Label17
    	'
    	Me.Label17.AutoSize = true
    	Me.Label17.Location = New System.Drawing.Point(23, 42)
    	Me.Label17.Name = "Label17"
    	Me.Label17.Size = New System.Drawing.Size(27, 13)
    	Me.Label17.TabIndex = 231
    	Me.Label17.Text = "[km]"
    	'
    	'Label10
    	'
    	Me.Label10.AutoSize = true
    	Me.Label10.Location = New System.Drawing.Point(134, 23)
    	Me.Label10.Name = "Label10"
    	Me.Label10.Size = New System.Drawing.Size(91, 13)
    	Me.Label10.TabIndex = 215
    	Me.Label10.Text = "Dł. boku w pionie"
    	'
    	'TextBox5
    	'
    	Me.TextBox5.Enabled = false
    	Me.TextBox5.Location = New System.Drawing.Point(52, 39)
    	Me.TextBox5.Name = "TextBox5"
    	Me.TextBox5.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox5.TabIndex = 216
    	Me.TextBox5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox5, "Całkowota szerokość pobieranego obszaru wyrażona w kilometrach")
    	'
    	'TextBox6
    	'
    	Me.TextBox6.Enabled = false
    	Me.TextBox6.Location = New System.Drawing.Point(170, 39)
    	Me.TextBox6.Name = "TextBox6"
    	Me.TextBox6.Size = New System.Drawing.Size(55, 20)
    	Me.TextBox6.TabIndex = 217
    	Me.TextBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
    	Me.ToolTip1.SetToolTip(Me.TextBox6, "Całkowota wysokość pobieranego obszaru wyrażona w kilometrach")
    	'
    	'GMapControl1
    	'
    	Me.GMapControl1.Bearing = 0!
    	Me.GMapControl1.CanDragMap = true
    	Me.GMapControl1.Cursor = System.Windows.Forms.Cursors.Hand
    	Me.GMapControl1.EmptyTileColor = System.Drawing.Color.Navy
    	Me.GMapControl1.GrayScaleMode = false
    	Me.GMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow
    	Me.GMapControl1.LevelsKeepInMemmory = 5
    	Me.GMapControl1.Location = New System.Drawing.Point(7, 34)
    	Me.GMapControl1.MarkersEnabled = true
    	Me.GMapControl1.MaxZoom = 17
    	Me.GMapControl1.MinZoom = 3
    	Me.GMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter
    	Me.GMapControl1.Name = "GMapControl1"
    	Me.GMapControl1.NegativeMode = false
    	Me.GMapControl1.PolygonsEnabled = true
    	Me.GMapControl1.RetryLoadTile = 0
    	Me.GMapControl1.RoutesEnabled = true
    	Me.GMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional
    	Me.GMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(CType(CType(33,Byte),Integer), CType(CType(65,Byte),Integer), CType(CType(105,Byte),Integer), CType(CType(225,Byte),Integer))
    	Me.GMapControl1.ShowTileGridLines = false
    	Me.GMapControl1.Size = New System.Drawing.Size(900, 586)
    	Me.GMapControl1.TabIndex = 249
    	Me.GMapControl1.Zoom = 0R
    	'
    	'RadioButton1
    	'
    	Me.RadioButton1.AutoSize = true
    	Me.RadioButton1.Checked = true
    	Me.RadioButton1.Location = New System.Drawing.Point(795, 37)
    	Me.RadioButton1.Name = "RadioButton1"
    	Me.RadioButton1.Size = New System.Drawing.Size(109, 17)
    	Me.RadioButton1.TabIndex = 258
    	Me.RadioButton1.TabStop = true
    	Me.RadioButton1.Text = "OpenStreetMap   "
    	Me.RadioButton1.UseVisualStyleBackColor = true
    	'
    	'RadioButton2
    	'
    	Me.RadioButton2.AutoSize = true
    	Me.RadioButton2.Location = New System.Drawing.Point(794, 59)
    	Me.RadioButton2.Name = "RadioButton2"
    	Me.RadioButton2.Size = New System.Drawing.Size(110, 17)
    	Me.RadioButton2.TabIndex = 259
    	Me.RadioButton2.Text = "GoogleMap          "
    	Me.RadioButton2.UseVisualStyleBackColor = true
    	'
    	'RadioButton3
    	'
    	Me.RadioButton3.AutoSize = true
    	Me.RadioButton3.Location = New System.Drawing.Point(794, 81)
    	Me.RadioButton3.Name = "RadioButton3"
    	Me.RadioButton3.Size = New System.Drawing.Size(109, 17)
    	Me.RadioButton3.TabIndex = 260
    	Me.RadioButton3.Text = "SatelliteBingsMap"
    	Me.RadioButton3.UseVisualStyleBackColor = true
    	'
    	'Label35
    	'
    	Me.Label35.AutoSize = true
    	Me.Label35.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label35.Location = New System.Drawing.Point(752, 604)
    	Me.Label35.Name = "Label35"
    	Me.Label35.Size = New System.Drawing.Size(28, 13)
    	Me.Label35.TabIndex = 261
    	Me.Label35.Text = "52.3"
    	'
    	'Label37
    	'
    	Me.Label37.AutoSize = true
    	Me.Label37.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label37.Location = New System.Drawing.Point(679, 604)
    	Me.Label37.Name = "Label37"
    	Me.Label37.Size = New System.Drawing.Size(75, 13)
    	Me.Label37.TabIndex = 263
    	Me.Label37.Text = "WGS84  Lat ="
    	'
    	'Label38
    	'
    	Me.Label38.AutoSize = true
    	Me.Label38.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label38.Location = New System.Drawing.Point(679, 587)
    	Me.Label38.Name = "Label38"
    	Me.Label38.Size = New System.Drawing.Size(161, 13)
    	Me.Label38.TabIndex = 264
    	Me.Label38.Text = "Współrzędne środka okna mapy"
    	'
    	'Label39
    	'
    	Me.Label39.AutoSize = true
    	Me.Label39.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label39.Location = New System.Drawing.Point(11, 585)
    	Me.Label39.Name = "Label39"
    	Me.Label39.Size = New System.Drawing.Size(109, 13)
    	Me.Label39.TabIndex = 265
    	Me.Label39.Text = "Współrzędne kursora"
    	'
    	'Label40
    	'
    	Me.Label40.AutoSize = true
    	Me.Label40.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label40.Location = New System.Drawing.Point(12, 602)
    	Me.Label40.Name = "Label40"
    	Me.Label40.Size = New System.Drawing.Size(31, 13)
    	Me.Label40.TabIndex = 266
    	Me.Label40.Text = "1992"
    	'
    	'Label41
    	'
    	Me.Label41.AutoSize = true
    	Me.Label41.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label41.Location = New System.Drawing.Point(51, 602)
    	Me.Label41.Name = "Label41"
    	Me.Label41.Size = New System.Drawing.Size(31, 13)
    	Me.Label41.TabIndex = 267
    	Me.Label41.Text = "1992"
    	'
    	'Label46
    	'
    	Me.Label46.AutoSize = true
    	Me.Label46.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Label46.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label46.Location = New System.Drawing.Point(249, 655)
    	Me.Label46.Name = "Label46"
    	Me.Label46.Size = New System.Drawing.Size(0, 12)
    	Me.Label46.TabIndex = 274
    	'
    	'Label63
    	'
    	Me.Label63.AutoSize = true
    	Me.Label63.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label63.Location = New System.Drawing.Point(833, 604)
    	Me.Label63.Name = "Label63"
    	Me.Label63.Size = New System.Drawing.Size(31, 13)
    	Me.Label63.TabIndex = 290
    	Me.Label63.Text = "19.2 "
    	'
    	'Label65
    	'
    	Me.Label65.AutoSize = true
    	Me.Label65.Location = New System.Drawing.Point(12, 68)
    	Me.Label65.Name = "Label65"
    	Me.Label65.Size = New System.Drawing.Size(13, 13)
    	Me.Label65.TabIndex = 292
    	Me.Label65.Text = "6"
    	'
    	'Label66
    	'
    	Me.Label66.AutoSize = true
    	Me.Label66.BackColor = System.Drawing.SystemColors.ActiveCaptionText
    	Me.Label66.Location = New System.Drawing.Point(800, 604)
    	Me.Label66.Name = "Label66"
    	Me.Label66.Size = New System.Drawing.Size(31, 13)
    	Me.Label66.TabIndex = 293
    	Me.Label66.Text = "Lng="
    	'
    	'Label67
    	'
    	Me.Label67.AutoSize = true
    	Me.Label67.BackColor = System.Drawing.SystemColors.ControlLight
    	Me.Label67.Font = New System.Drawing.Font("Microsoft Sans Serif", 9!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Label67.ForeColor = System.Drawing.SystemColors.ButtonShadow
    	Me.Label67.Location = New System.Drawing.Point(519, 3)
    	Me.Label67.Name = "Label67"
    	Me.Label67.Size = New System.Drawing.Size(300, 15)
    	Me.Label67.TabIndex = 297
    	Me.Label67.Text = "Zaznacz na mapie obszar prawym  przyciskiem myszy"
    	'
    	'button6
    	'
    	Me.button6.Location = New System.Drawing.Point(663, 850)
    	Me.button6.Name = "button6"
    	Me.button6.Size = New System.Drawing.Size(103, 25)
    	Me.button6.TabIndex = 299
    	Me.button6.Text = "Złącz segmenty"
    	Me.button6.UseVisualStyleBackColor = true
    	'
    	'Form1
    	'
    	Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
    	Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    	Me.AutoScroll = true
    	Me.ClientSize = New System.Drawing.Size(914, 906)
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
    	Me.Controls.Add(Me.Label46)
    	Me.Controls.Add(Me.GroupBox3)
    	Me.Controls.Add(Me.Label41)
    	Me.Controls.Add(Me.Label40)
    	Me.Controls.Add(Me.Label39)
    	Me.Controls.Add(Me.Label38)
    	Me.Controls.Add(Me.Label37)
    	Me.Controls.Add(Me.Label35)
    	Me.Controls.Add(Me.RadioButton3)
    	Me.Controls.Add(Me.RadioButton2)
    	Me.Controls.Add(Me.RadioButton1)
    	Me.Controls.Add(Me.GMapControl1)
    	Me.Controls.Add(Me.GroupBox7)
    	Me.Controls.Add(Me.StatusStrip1)
    	Me.Controls.Add(Me.Panel1)
    	Me.Controls.Add(Me.GroupBox2)
    	Me.Controls.Add(Me.GroupBox1)
    	Me.Controls.Add(Me.MenuStrip1)
    	Me.DoubleBuffered = true
    	Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
    	Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
    	Me.MainMenuStrip = Me.MenuStrip1
    	Me.MaximizeBox = false
    	Me.Name = "Form1"
    	Me.GroupBox1.ResumeLayout(false)
    	Me.GroupBox1.PerformLayout
    	Me.GroupBox2.ResumeLayout(false)
    	Me.GroupBox2.PerformLayout
    	Me.StatusStrip1.ResumeLayout(false)
    	Me.StatusStrip1.PerformLayout
    	Me.MenuStrip1.ResumeLayout(false)
    	Me.MenuStrip1.PerformLayout
    	Me.GroupBox7.ResumeLayout(false)
    	Me.GroupBox7.PerformLayout
    	Me.GroupBox3.ResumeLayout(false)
    	Me.GroupBox3.PerformLayout
    	Me.GroupBox4.ResumeLayout(false)
    	Me.GroupBox4.PerformLayout
    	Me.ResumeLayout(false)
    	Me.PerformLayout
    End Sub
    Friend WithEvents button6 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
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
    Friend WithEvents GroupBox7 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox9 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TextBox10 As System.Windows.Forms.TextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents GMapControl1 As GMap.NET.WindowsForms.GMapControl
    Friend WithEvents Label32 As System.Windows.Forms.Label
    Friend WithEvents Label34 As System.Windows.Forms.Label
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
    Friend WithEvents Button4 As System.Windows.Forms.Button
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
    Friend WithEvents Label46 As System.Windows.Forms.Label
    Friend WithEvents Label63 As System.Windows.Forms.Label
    Friend WithEvents Label65 As System.Windows.Forms.Label
    Friend WithEvents Label66 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button5 As System.Windows.Forms.Button
    Friend WithEvents Label67 As System.Windows.Forms.Label
    Friend WithEvents ScalanieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label33 As System.Windows.Forms.Label
    Friend WithEvents Label31 As System.Windows.Forms.Label
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents TextBox12 As System.Windows.Forms.TextBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents TextBox11 As System.Windows.Forms.TextBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents TextBox7 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox8 As System.Windows.Forms.TextBox
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents TextBox5 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox6 As System.Windows.Forms.TextBox

End Class