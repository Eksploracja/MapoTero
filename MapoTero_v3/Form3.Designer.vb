﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
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
    	Me.TextBox1 = New System.Windows.Forms.TextBox()
    	Me.Button2 = New System.Windows.Forms.Button()
    	Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
    	Me.Button4 = New System.Windows.Forms.Button()
    	Me.GroupBox1 = New System.Windows.Forms.GroupBox()
    	Me.ComboBox2 = New System.Windows.Forms.ComboBox()
    	Me.ComboBox1 = New System.Windows.Forms.ComboBox()
    	Me.Label17 = New System.Windows.Forms.Label()
    	Me.Label16 = New System.Windows.Forms.Label()
    	Me.Label15 = New System.Windows.Forms.Label()
    	Me.Label13 = New System.Windows.Forms.Label()
    	Me.Label12 = New System.Windows.Forms.Label()
    	Me.TextBox11 = New System.Windows.Forms.TextBox()
    	Me.Label11 = New System.Windows.Forms.Label()
    	Me.Label10 = New System.Windows.Forms.Label()
    	Me.TextBox9 = New System.Windows.Forms.TextBox()
    	Me.Label9 = New System.Windows.Forms.Label()
    	Me.TextBox8 = New System.Windows.Forms.TextBox()
    	Me.Label8 = New System.Windows.Forms.Label()
    	Me.TextBox7 = New System.Windows.Forms.TextBox()
    	Me.Label7 = New System.Windows.Forms.Label()
    	Me.TextBox6 = New System.Windows.Forms.TextBox()
    	Me.Label4 = New System.Windows.Forms.Label()
    	Me.Label5 = New System.Windows.Forms.Label()
    	Me.Label6 = New System.Windows.Forms.Label()
    	Me.TextBox4 = New System.Windows.Forms.TextBox()
    	Me.TextBox5 = New System.Windows.Forms.TextBox()
    	Me.Label3 = New System.Windows.Forms.Label()
    	Me.Label2 = New System.Windows.Forms.Label()
    	Me.Label1 = New System.Windows.Forms.Label()
    	Me.TextBox3 = New System.Windows.Forms.TextBox()
    	Me.TextBox2 = New System.Windows.Forms.TextBox()
    	Me.CheckBox1 = New System.Windows.Forms.CheckBox()
    	Me.Label21 = New System.Windows.Forms.Label()
    	Me.RichTextBox1 = New System.Windows.Forms.RichTextBox()
    	Me.Label22 = New System.Windows.Forms.Label()
    	Me.groupBox2 = New System.Windows.Forms.GroupBox()
    	Me.Label20 = New System.Windows.Forms.Label()
    	Me.Label19 = New System.Windows.Forms.Label()
    	Me.Label18 = New System.Windows.Forms.Label()
    	Me.Label14 = New System.Windows.Forms.Label()
    	Me.TrackBar1 = New System.Windows.Forms.TrackBar()
    	Me.GroupBox1.SuspendLayout
    	Me.groupBox2.SuspendLayout
    	CType(Me.TrackBar1,System.ComponentModel.ISupportInitialize).BeginInit
    	Me.SuspendLayout
    	'
    	'TextBox1
    	'
    	Me.TextBox1.Location = New System.Drawing.Point(90, 46)
    	Me.TextBox1.Name = "TextBox1"
    	Me.TextBox1.Size = New System.Drawing.Size(581, 20)
    	Me.TextBox1.TabIndex = 1
    	'
    	'Button2
    	'
    	Me.Button2.Location = New System.Drawing.Point(12, 46)
    	Me.Button2.Name = "Button2"
    	Me.Button2.Size = New System.Drawing.Size(79, 20)
    	Me.Button2.TabIndex = 2
    	Me.Button2.Text = "zmień folder"
    	Me.Button2.UseVisualStyleBackColor = true
    	'
    	'Button4
    	'
    	Me.Button4.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Button4.Location = New System.Drawing.Point(545, 106)
    	Me.Button4.Name = "Button4"
    	Me.Button4.Size = New System.Drawing.Size(117, 54)
    	Me.Button4.TabIndex = 26
    	Me.Button4.Text = "Złącz segmenty"
    	Me.Button4.UseVisualStyleBackColor = true
    	'
    	'GroupBox1
    	'
    	Me.GroupBox1.Controls.Add(Me.ComboBox2)
    	Me.GroupBox1.Controls.Add(Me.ComboBox1)
    	Me.GroupBox1.Controls.Add(Me.Label17)
    	Me.GroupBox1.Controls.Add(Me.Label16)
    	Me.GroupBox1.Controls.Add(Me.Label15)
    	Me.GroupBox1.Controls.Add(Me.Label13)
    	Me.GroupBox1.Controls.Add(Me.Label12)
    	Me.GroupBox1.Controls.Add(Me.TextBox11)
    	Me.GroupBox1.Controls.Add(Me.Label11)
    	Me.GroupBox1.Controls.Add(Me.Label10)
    	Me.GroupBox1.Controls.Add(Me.TextBox9)
    	Me.GroupBox1.Controls.Add(Me.Label9)
    	Me.GroupBox1.Controls.Add(Me.TextBox8)
    	Me.GroupBox1.Controls.Add(Me.Label8)
    	Me.GroupBox1.Controls.Add(Me.TextBox7)
    	Me.GroupBox1.Controls.Add(Me.Label7)
    	Me.GroupBox1.Controls.Add(Me.TextBox6)
    	Me.GroupBox1.Controls.Add(Me.Label4)
    	Me.GroupBox1.Controls.Add(Me.Label5)
    	Me.GroupBox1.Controls.Add(Me.Label6)
    	Me.GroupBox1.Controls.Add(Me.TextBox4)
    	Me.GroupBox1.Controls.Add(Me.TextBox5)
    	Me.GroupBox1.Controls.Add(Me.Label3)
    	Me.GroupBox1.Controls.Add(Me.Label2)
    	Me.GroupBox1.Controls.Add(Me.Label1)
    	Me.GroupBox1.Controls.Add(Me.TextBox3)
    	Me.GroupBox1.Controls.Add(Me.TextBox2)
    	Me.GroupBox1.Location = New System.Drawing.Point(12, 101)
    	Me.GroupBox1.Name = "GroupBox1"
    	Me.GroupBox1.Size = New System.Drawing.Size(281, 352)
    	Me.GroupBox1.TabIndex = 35
    	Me.GroupBox1.TabStop = false
    	Me.GroupBox1.Text = "Właściwości zbioru segmentów z pliku conf.txt"
    	'
    	'ComboBox2
    	'
    	Me.ComboBox2.FormattingEnabled = true
    	Me.ComboBox2.Items.AddRange(New Object() {"01_02_03", "1_2_3", "NrWiersza_NrKolumny"})
    	Me.ComboBox2.Location = New System.Drawing.Point(24, 302)
    	Me.ComboBox2.Name = "ComboBox2"
    	Me.ComboBox2.Size = New System.Drawing.Size(88, 21)
    	Me.ComboBox2.TabIndex = 63
    	'
    	'ComboBox1
    	'
    	Me.ComboBox1.FormattingEnabled = true
    	Me.ComboBox1.Items.AddRange(New Object() {"jpeg", "gif", "png", "png24", "png32", "png8", "svg+xml", "tiff"})
    	Me.ComboBox1.Location = New System.Drawing.Point(24, 262)
    	Me.ComboBox1.Name = "ComboBox1"
    	Me.ComboBox1.Size = New System.Drawing.Size(88, 21)
    	Me.ComboBox1.TabIndex = 62
    	'
    	'Label17
    	'
    	Me.Label17.AutoSize = true
    	Me.Label17.Location = New System.Drawing.Point(64, 242)
    	Me.Label17.Name = "Label17"
    	Me.Label17.Size = New System.Drawing.Size(134, 13)
    	Me.Label17.TabIndex = 61
    	Me.Label17.Text = "Parametry pliku rastrowego"
    	'
    	'Label16
    	'
    	Me.Label16.AutoSize = true
    	Me.Label16.Location = New System.Drawing.Point(75, 179)
    	Me.Label16.Name = "Label16"
    	Me.Label16.Size = New System.Drawing.Size(103, 13)
    	Me.Label16.TabIndex = 60
    	Me.Label16.Text = "Parametry segmentu"
    	'
    	'Label15
    	'
    	Me.Label15.AutoSize = true
    	Me.Label15.Location = New System.Drawing.Point(52, 159)
    	Me.Label15.Name = "Label15"
    	Me.Label15.Size = New System.Drawing.Size(46, 13)
    	Me.Label15.TabIndex = 59
    	Me.Label15.Text = "poziomo"
    	'
    	'Label13
    	'
    	Me.Label13.AutoSize = true
    	Me.Label13.Location = New System.Drawing.Point(28, 325)
    	Me.Label13.Name = "Label13"
    	Me.Label13.Size = New System.Drawing.Size(70, 13)
    	Me.Label13.TabIndex = 58
    	Me.Label13.Text = "styl numeracji"
    	'
    	'Label12
    	'
    	Me.Label12.AutoSize = true
    	Me.Label12.Location = New System.Drawing.Point(145, 286)
    	Me.Label12.Name = "Label12"
    	Me.Label12.Size = New System.Drawing.Size(62, 13)
    	Me.Label12.TabIndex = 56
    	Me.Label12.Text = "przedrostek"
    	'
    	'TextBox11
    	'
    	Me.TextBox11.Location = New System.Drawing.Point(136, 263)
    	Me.TextBox11.Name = "TextBox11"
    	Me.TextBox11.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox11.TabIndex = 55
    	'
    	'Label11
    	'
    	Me.Label11.AutoSize = true
    	Me.Label11.Location = New System.Drawing.Point(52, 286)
    	Me.Label11.Name = "Label11"
    	Me.Label11.Size = New System.Drawing.Size(36, 13)
    	Me.Label11.TabIndex = 54
    	Me.Label11.Text = "format"
    	'
    	'Label10
    	'
    	Me.Label10.AutoSize = true
    	Me.Label10.Location = New System.Drawing.Point(160, 159)
    	Me.Label10.Name = "Label10"
    	Me.Label10.Size = New System.Drawing.Size(47, 13)
    	Me.Label10.TabIndex = 52
    	Me.Label10.Text = "pionowo"
    	'
    	'TextBox9
    	'
    	Me.TextBox9.Location = New System.Drawing.Point(136, 136)
    	Me.TextBox9.Name = "TextBox9"
    	Me.TextBox9.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox9.TabIndex = 51
    	'
    	'Label9
    	'
    	Me.Label9.AutoSize = true
    	Me.Label9.Location = New System.Drawing.Point(75, 122)
    	Me.Label9.Name = "Label9"
    	Me.Label9.Size = New System.Drawing.Size(95, 13)
    	Me.Label9.TabIndex = 50
    	Me.Label9.Text = "Liczba segmentów"
    	'
    	'TextBox8
    	'
    	Me.TextBox8.Location = New System.Drawing.Point(24, 136)
    	Me.TextBox8.Name = "TextBox8"
    	Me.TextBox8.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox8.TabIndex = 49
    	'
    	'Label8
    	'
    	Me.Label8.AutoSize = true
    	Me.Label8.Location = New System.Drawing.Point(170, 218)
    	Me.Label8.Name = "Label8"
    	Me.Label8.Size = New System.Drawing.Size(28, 13)
    	Me.Label8.TabIndex = 48
    	Me.Label8.Text = "pixel"
    	'
    	'TextBox7
    	'
    	Me.TextBox7.Location = New System.Drawing.Point(136, 195)
    	Me.TextBox7.Name = "TextBox7"
    	Me.TextBox7.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox7.TabIndex = 47
    	'
    	'Label7
    	'
    	Me.Label7.AutoSize = true
    	Me.Label7.Location = New System.Drawing.Point(52, 218)
    	Me.Label7.Name = "Label7"
    	Me.Label7.Size = New System.Drawing.Size(44, 13)
    	Me.Label7.TabIndex = 46
    	Me.Label7.Text = "dł boku"
    	'
    	'TextBox6
    	'
    	Me.TextBox6.Location = New System.Drawing.Point(24, 195)
    	Me.TextBox6.Name = "TextBox6"
    	Me.TextBox6.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox6.TabIndex = 45
    	'
    	'Label4
    	'
    	Me.Label4.AutoSize = true
    	Me.Label4.Location = New System.Drawing.Point(6, 93)
    	Me.Label4.Name = "Label4"
    	Me.Label4.Size = New System.Drawing.Size(12, 13)
    	Me.Label4.TabIndex = 44
    	Me.Label4.Text = "x"
    	'
    	'Label5
    	'
    	Me.Label5.AutoSize = true
    	Me.Label5.Location = New System.Drawing.Point(118, 93)
    	Me.Label5.Name = "Label5"
    	Me.Label5.Size = New System.Drawing.Size(12, 13)
    	Me.Label5.TabIndex = 43
    	Me.Label5.Text = "y"
    	'
    	'Label6
    	'
    	Me.Label6.AutoSize = true
    	Me.Label6.Location = New System.Drawing.Point(63, 23)
    	Me.Label6.Name = "Label6"
    	Me.Label6.Size = New System.Drawing.Size(107, 13)
    	Me.Label6.TabIndex = 42
    	Me.Label6.Text = "prawy gorny narożnik"
    	'
    	'TextBox4
    	'
    	Me.TextBox4.Location = New System.Drawing.Point(24, 90)
    	Me.TextBox4.Name = "TextBox4"
    	Me.TextBox4.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox4.TabIndex = 41
    	'
    	'TextBox5
    	'
    	Me.TextBox5.Location = New System.Drawing.Point(136, 90)
    	Me.TextBox5.Name = "TextBox5"
    	Me.TextBox5.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox5.TabIndex = 40
    	'
    	'Label3
    	'
    	Me.Label3.AutoSize = true
    	Me.Label3.Location = New System.Drawing.Point(118, 42)
    	Me.Label3.Name = "Label3"
    	Me.Label3.Size = New System.Drawing.Size(12, 13)
    	Me.Label3.TabIndex = 39
    	Me.Label3.Text = "y"
    	'
    	'Label2
    	'
    	Me.Label2.AutoSize = true
    	Me.Label2.Location = New System.Drawing.Point(6, 42)
    	Me.Label2.Name = "Label2"
    	Me.Label2.Size = New System.Drawing.Size(12, 13)
    	Me.Label2.TabIndex = 38
    	Me.Label2.Text = "x"
    	'
    	'Label1
    	'
    	Me.Label1.AutoSize = true
    	Me.Label1.Location = New System.Drawing.Point(67, 74)
    	Me.Label1.Name = "Label1"
    	Me.Label1.Size = New System.Drawing.Size(103, 13)
    	Me.Label1.TabIndex = 37
    	Me.Label1.Text = "Lewy dolny narożnik"
    	'
    	'TextBox3
    	'
    	Me.TextBox3.Location = New System.Drawing.Point(136, 39)
    	Me.TextBox3.Name = "TextBox3"
    	Me.TextBox3.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox3.TabIndex = 36
    	'
    	'TextBox2
    	'
    	Me.TextBox2.Location = New System.Drawing.Point(24, 39)
    	Me.TextBox2.Name = "TextBox2"
    	Me.TextBox2.Size = New System.Drawing.Size(88, 20)
    	Me.TextBox2.TabIndex = 35
    	'
    	'CheckBox1
    	'
    	Me.CheckBox1.AutoSize = true
    	Me.CheckBox1.Location = New System.Drawing.Point(12, 459)
    	Me.CheckBox1.Name = "CheckBox1"
    	Me.CheckBox1.Size = New System.Drawing.Size(291, 17)
    	Me.CheckBox1.TabIndex = 63
    	Me.CheckBox1.Text = "ręczne wprowadzanie parametrów w razie braku conf.txt"
    	Me.CheckBox1.UseVisualStyleBackColor = true
    	'
    	'Label21
    	'
    	Me.Label21.AutoSize = true
    	Me.Label21.Location = New System.Drawing.Point(9, 30)
    	Me.Label21.Name = "Label21"
    	Me.Label21.Size = New System.Drawing.Size(249, 13)
    	Me.Label21.TabIndex = 69
    	Me.Label21.Text = "Lokalizacja katalogu z segmentami i plikiem conf.txt"
    	'
    	'RichTextBox1
    	'
    	Me.RichTextBox1.Enabled = false
    	Me.RichTextBox1.Location = New System.Drawing.Point(395, 206)
    	Me.RichTextBox1.Name = "RichTextBox1"
    	Me.RichTextBox1.Size = New System.Drawing.Size(267, 96)
    	Me.RichTextBox1.TabIndex = 70
    	Me.RichTextBox1.Text = ""
    	'
    	'Label22
    	'
    	Me.Label22.AutoSize = true
    	Me.Label22.Location = New System.Drawing.Point(396, 190)
    	Me.Label22.Name = "Label22"
    	Me.Label22.Size = New System.Drawing.Size(65, 13)
    	Me.Label22.TabIndex = 71
    	Me.Label22.Text = "Komunikaty:"
    	'
    	'groupBox2
    	'
    	Me.groupBox2.Controls.Add(Me.Label20)
    	Me.groupBox2.Controls.Add(Me.Label19)
    	Me.groupBox2.Controls.Add(Me.Label18)
    	Me.groupBox2.Controls.Add(Me.Label14)
    	Me.groupBox2.Controls.Add(Me.TrackBar1)
    	Me.groupBox2.Location = New System.Drawing.Point(396, 369)
    	Me.groupBox2.Name = "groupBox2"
    	Me.groupBox2.Size = New System.Drawing.Size(267, 84)
    	Me.groupBox2.TabIndex = 72
    	Me.groupBox2.TabStop = false
    	'
    	'Label20
    	'
    	Me.Label20.AutoSize = true
    	Me.Label20.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238,Byte))
    	Me.Label20.Location = New System.Drawing.Point(207, 14)
    	Me.Label20.Name = "Label20"
    	Me.Label20.Size = New System.Drawing.Size(30, 13)
    	Me.Label20.TabIndex = 73
    	Me.Label20.Text = "75%"
    	'
    	'Label19
    	'
    	Me.Label19.AutoSize = true
    	Me.Label19.Location = New System.Drawing.Point(12, 67)
    	Me.Label19.Name = "Label19"
    	Me.Label19.Size = New System.Drawing.Size(32, 13)
    	Me.Label19.TabIndex = 72
    	Me.Label19.Text = "niska"
    	'
    	'Label18
    	'
    	Me.Label18.AutoSize = true
    	Me.Label18.Location = New System.Drawing.Point(214, 67)
    	Me.Label18.Name = "Label18"
    	Me.Label18.Size = New System.Drawing.Size(43, 13)
    	Me.Label18.TabIndex = 71
    	Me.Label18.Text = "wysoka"
    	'
    	'Label14
    	'
    	Me.Label14.AutoSize = true
    	Me.Label14.Location = New System.Drawing.Point(12, 14)
    	Me.Label14.Name = "Label14"
    	Me.Label14.Size = New System.Drawing.Size(191, 13)
    	Me.Label14.TabIndex = 70
    	Me.Label14.Text = "Jakość kompresji JPG scalonego pliku:"
    	'
    	'TrackBar1
    	'
    	Me.TrackBar1.Location = New System.Drawing.Point(6, 38)
    	Me.TrackBar1.Maximum = 100
    	Me.TrackBar1.Name = "TrackBar1"
    	Me.TrackBar1.Size = New System.Drawing.Size(255, 42)
    	Me.TrackBar1.SmallChange = 5
    	Me.TrackBar1.TabIndex = 69
    	Me.TrackBar1.TickFrequency = 5
    	Me.TrackBar1.Value = 75
    	'
    	'Form3
    	'
    	Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
    	Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    	Me.ClientSize = New System.Drawing.Size(695, 507)
    	Me.Controls.Add(Me.groupBox2)
    	Me.Controls.Add(Me.Label22)
    	Me.Controls.Add(Me.RichTextBox1)
    	Me.Controls.Add(Me.Label21)
    	Me.Controls.Add(Me.CheckBox1)
    	Me.Controls.Add(Me.GroupBox1)
    	Me.Controls.Add(Me.Button4)
    	Me.Controls.Add(Me.Button2)
    	Me.Controls.Add(Me.TextBox1)
    	Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
    	Me.Name = "Form3"
    	Me.Text = "Złączanie segmentów w jeden plik"
    	Me.TopMost = true
    	Me.GroupBox1.ResumeLayout(false)
    	Me.GroupBox1.PerformLayout
    	Me.groupBox2.ResumeLayout(false)
    	Me.groupBox2.PerformLayout
    	CType(Me.TrackBar1,System.ComponentModel.ISupportInitialize).EndInit
    	Me.ResumeLayout(false)
    	Me.PerformLayout
    End Sub
    Private groupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents TextBox11 As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents TextBox9 As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents TextBox8 As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents TextBox7 As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents TextBox6 As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox5 As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents ComboBox2 As System.Windows.Forms.ComboBox
    Friend WithEvents TrackBar1 As System.Windows.Forms.TrackBar
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents RichTextBox1 As System.Windows.Forms.RichTextBox
    Friend WithEvents Label22 As System.Windows.Forms.Label
End Class
