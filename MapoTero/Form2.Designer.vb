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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.CheckBox7 = New System.Windows.Forms.CheckBox()
        Me.CheckBox4 = New System.Windows.Forms.CheckBox()
        Me.CheckBox2 = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.TextBox4 = New System.Windows.Forms.TextBox()
        Me.TextBox5 = New System.Windows.Forms.TextBox()
        Me.ButtonDolna = New System.Windows.Forms.Button()
        Me.ButtonGorna = New System.Windows.Forms.Button()
        Me.ButtonPolaczone = New System.Windows.Forms.Button()
        Me.GroupBoxWarstwy = New System.Windows.Forms.GroupBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.CheckBox3 = New System.Windows.Forms.CheckBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TextBox6 = New System.Windows.Forms.TextBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.CheckBox8 = New System.Windows.Forms.CheckBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.CheckBox6 = New System.Windows.Forms.CheckBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.CheckBox5 = New System.Windows.Forms.CheckBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBoxWarstwy.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.SuspendLayout()
        '
        'ComboBox1
        '
        Me.ComboBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Items.AddRange(New Object() {"jpeg", "tiff", "png", "png8", "png24", "png32", "gif", "svg+xml"})
        Me.ComboBox1.Location = New System.Drawing.Point(15, 95)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(152, 21)
        Me.ComboBox1.TabIndex = 0
        Me.ComboBox1.Text = Module1.format
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 79)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(114, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "rozszerzenie segmentu"
        '
        'CheckBox1
        '
        Me.CheckBox1.AccessibleDescription = ""
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(8, 20)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(129, 17)
        Me.CheckBox1.TabIndex = 2
        Me.CheckBox1.Text = "plik .map /OziExplorer"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.CheckBox7)
        Me.GroupBox1.Controls.Add(Me.CheckBox4)
        Me.GroupBox1.Controls.Add(Me.CheckBox2)
        Me.GroupBox1.Controls.Add(Me.CheckBox1)
        Me.GroupBox1.Location = New System.Drawing.Point(30, 135)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(170, 97)
        Me.GroupBox1.TabIndex = 3
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Kalibruj segmenty"
        '
        'CheckBox7
        '
        Me.CheckBox7.AutoSize = True
        Me.CheckBox7.Location = New System.Drawing.Point(8, 76)
        Me.CheckBox7.Name = "CheckBox7"
        Me.CheckBox7.Size = New System.Drawing.Size(132, 17)
        Me.CheckBox7.TabIndex = 5
        Me.CheckBox7.Text = "plik .jpgw /nowy QGIS"
        Me.CheckBox7.UseVisualStyleBackColor = True
        '
        'CheckBox4
        '
        Me.CheckBox4.AutoSize = True
        Me.CheckBox4.Location = New System.Drawing.Point(8, 59)
        Me.CheckBox4.Name = "CheckBox4"
        Me.CheckBox4.Size = New System.Drawing.Size(159, 17)
        Me.CheckBox4.TabIndex = 4
        Me.CheckBox4.Text = "pliki .wld .points /stary QGIS"
        Me.CheckBox4.UseVisualStyleBackColor = True
        '
        'CheckBox2
        '
        Me.CheckBox2.AutoSize = True
        Me.CheckBox2.Location = New System.Drawing.Point(8, 39)
        Me.CheckBox2.Name = "CheckBox2"
        Me.CheckBox2.Size = New System.Drawing.Size(139, 17)
        Me.CheckBox2.TabIndex = 3
        Me.CheckBox2.Text = "plik .gmi /GPSTuner 5.x"
        Me.CheckBox2.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.TextBox2)
        Me.GroupBox2.Controls.Add(Me.TextBox1)
        Me.GroupBox2.Location = New System.Drawing.Point(217, 135)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(152, 97)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Ponowne próby pobrania"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(15, 52)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(70, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "co ile sekund"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 26)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Ilość prób"
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(102, 45)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(38, 20)
        Me.TextBox2.TabIndex = 1
        Me.TextBox2.Text = "5"
        Me.TextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(102, 19)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(38, 20)
        Me.TextBox1.TabIndex = 0
        Me.TextBox1.Text = "3"
        Me.TextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(146, 28)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(417, 20)
        Me.TextBox3.TabIndex = 4
        '
        'TextBox4
        '
        Me.TextBox4.Location = New System.Drawing.Point(146, 51)
        Me.TextBox4.Name = "TextBox4"
        Me.TextBox4.Size = New System.Drawing.Size(417, 20)
        Me.TextBox4.TabIndex = 5
        '
        'TextBox5
        '
        Me.TextBox5.Location = New System.Drawing.Point(146, 74)
        Me.TextBox5.Name = "TextBox5"
        Me.TextBox5.Size = New System.Drawing.Size(417, 20)
        Me.TextBox5.TabIndex = 6
        '
        'ButtonDolna
        '
        Me.ButtonDolna.Location = New System.Drawing.Point(15, 25)
        Me.ButtonDolna.Name = "ButtonDolna"
        Me.ButtonDolna.Size = New System.Drawing.Size(122, 23)
        Me.ButtonDolna.TabIndex = 7
        Me.ButtonDolna.Text = "dolna (podkładowa)"
        Me.ButtonDolna.UseVisualStyleBackColor = True
        '
        'ButtonGorna
        '
        Me.ButtonGorna.Location = New System.Drawing.Point(15, 49)
        Me.ButtonGorna.Name = "ButtonGorna"
        Me.ButtonGorna.Size = New System.Drawing.Size(122, 23)
        Me.ButtonGorna.TabIndex = 8
        Me.ButtonGorna.Text = "górna (nakładana)"
        Me.ButtonGorna.UseVisualStyleBackColor = True
        '
        'ButtonPolaczone
        '
        Me.ButtonPolaczone.Location = New System.Drawing.Point(15, 72)
        Me.ButtonPolaczone.Name = "ButtonPolaczone"
        Me.ButtonPolaczone.Size = New System.Drawing.Size(122, 23)
        Me.ButtonPolaczone.TabIndex = 9
        Me.ButtonPolaczone.Text = "połączone"
        Me.ButtonPolaczone.UseVisualStyleBackColor = True
        '
        'GroupBoxWarstwy
        '
        Me.GroupBoxWarstwy.Controls.Add(Me.ButtonPolaczone)
        Me.GroupBoxWarstwy.Controls.Add(Me.ButtonGorna)
        Me.GroupBoxWarstwy.Controls.Add(Me.ButtonDolna)
        Me.GroupBoxWarstwy.Controls.Add(Me.TextBox5)
        Me.GroupBoxWarstwy.Controls.Add(Me.TextBox4)
        Me.GroupBoxWarstwy.Controls.Add(Me.TextBox3)
        Me.GroupBoxWarstwy.Location = New System.Drawing.Point(30, 22)
        Me.GroupBoxWarstwy.Name = "GroupBoxWarstwy"
        Me.GroupBoxWarstwy.Size = New System.Drawing.Size(579, 107)
        Me.GroupBoxWarstwy.TabIndex = 10
        Me.GroupBoxWarstwy.TabStop = False
        Me.GroupBoxWarstwy.Text = "Katalogi warstw do połączenia (opcjonalne narzędzie nakładania dwóch warstw segme" & _
    "ntów png)"
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Button1.Location = New System.Drawing.Point(287, 375)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(90, 39)
        Me.Button1.TabIndex = 11
        Me.Button1.Text = "OK"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'CheckBox3
        '
        Me.CheckBox3.AutoSize = True
        Me.CheckBox3.Location = New System.Drawing.Point(404, 154)
        Me.CheckBox3.Name = "CheckBox3"
        Me.CheckBox3.Size = New System.Drawing.Size(143, 17)
        Me.CheckBox3.TabIndex = 12
        Me.CheckBox3.Text = "zamień X i Y w zapytaniu"
        Me.CheckBox3.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(15, 34)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(152, 13)
        Me.Label4.TabIndex = 13
        Me.Label4.Text = "przedrostek nazwy segmentów"
        '
        'TextBox6
        '
        Me.TextBox6.Location = New System.Drawing.Point(15, 50)
        Me.TextBox6.Name = "TextBox6"
        Me.TextBox6.Size = New System.Drawing.Size(152, 20)
        Me.TextBox6.TabIndex = 14
        Me.TextBox6.Text = "segment"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.GroupBox6)
        Me.GroupBox3.Controls.Add(Me.GroupBox5)
        Me.GroupBox3.Controls.Add(Me.CheckBox3)
        Me.GroupBox3.Controls.Add(Me.GroupBoxWarstwy)
        Me.GroupBox3.Controls.Add(Me.GroupBox2)
        Me.GroupBox3.Controls.Add(Me.GroupBox1)
        Me.GroupBox3.Location = New System.Drawing.Point(287, 19)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(625, 338)
        Me.GroupBox3.TabIndex = 15
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Ustawienia programu (lastsettings.txt)"
        '
        'GroupBox6
        '
        Me.GroupBox6.Controls.Add(Me.CheckBox8)
        Me.GroupBox6.Location = New System.Drawing.Point(217, 238)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(152, 71)
        Me.GroupBox6.TabIndex = 14
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "Zmiana domyślnej numeracji segmentów"
        '
        'CheckBox8
        '
        Me.CheckBox8.AutoSize = True
        Me.CheckBox8.Location = New System.Drawing.Point(9, 29)
        Me.CheckBox8.Name = "CheckBox8"
        Me.CheckBox8.Size = New System.Drawing.Size(136, 17)
        Me.CheckBox8.TabIndex = 5
        Me.CheckBox8.Text = "numeruj kolejno 1,2,3..."
        Me.CheckBox8.UseVisualStyleBackColor = True
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.CheckBox6)
        Me.GroupBox5.Location = New System.Drawing.Point(29, 238)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(171, 71)
        Me.GroupBox5.TabIndex = 13
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "TrekBuddy"
        '
        'CheckBox6
        '
        Me.CheckBox6.AutoSize = True
        Me.CheckBox6.Location = New System.Drawing.Point(9, 29)
        Me.CheckBox6.Name = "CheckBox6"
        Me.CheckBox6.Size = New System.Drawing.Size(80, 17)
        Me.CheckBox6.TabIndex = 5
        Me.CheckBox6.Text = "twórz mapę"
        Me.CheckBox6.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.Label7)
        Me.GroupBox4.Controls.Add(Me.Label6)
        Me.GroupBox4.Controls.Add(Me.Label5)
        Me.GroupBox4.Controls.Add(Me.CheckBox5)
        Me.GroupBox4.Controls.Add(Me.TextBox6)
        Me.GroupBox4.Controls.Add(Me.Label4)
        Me.GroupBox4.Controls.Add(Me.Label1)
        Me.GroupBox4.Controls.Add(Me.ComboBox1)
        Me.GroupBox4.Location = New System.Drawing.Point(12, 19)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(259, 338)
        Me.GroupBox4.TabIndex = 16
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Ustawienia sesji (conf.txt)"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(15, 167)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(122, 13)
        Me.Label7.TabIndex = 20
        Me.Label7.Text = "ostatni pobrany segment"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(15, 153)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(165, 13)
        Me.Label6.TabIndex = 19
        Me.Label6.Text = "segmenty o numerze wyższym jak"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(15, 139)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(194, 13)
        Me.Label5.TabIndex = 18
        Me.Label5.Text = "Przy powtórnym pobieraniu ściągaj tylko"
        '
        'CheckBox5
        '
        Me.CheckBox5.AutoSize = True
        Me.CheckBox5.Location = New System.Drawing.Point(186, 165)
        Me.CheckBox5.Name = "CheckBox5"
        Me.CheckBox5.Size = New System.Drawing.Size(15, 14)
        Me.CheckBox5.TabIndex = 17
        Me.CheckBox5.UseVisualStyleBackColor = True
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(926, 440)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.Button1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form2"
        Me.RightToLeftLayout = True
        Me.Text = "Ustawienia"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBoxWarstwy.ResumeLayout(False)
        Me.GroupBoxWarstwy.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents CheckBox2 As System.Windows.Forms.CheckBox
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox5 As System.Windows.Forms.TextBox
    Friend WithEvents ButtonDolna As System.Windows.Forms.Button
    Friend WithEvents ButtonGorna As System.Windows.Forms.Button
    Friend WithEvents ButtonPolaczone As System.Windows.Forms.Button
    Friend WithEvents GroupBoxWarstwy As System.Windows.Forms.GroupBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents CheckBox3 As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox4 As System.Windows.Forms.CheckBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TextBox6 As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents CheckBox5 As System.Windows.Forms.CheckBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents CheckBox6 As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Friend WithEvents CheckBox8 As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox7 As System.Windows.Forms.CheckBox
End Class
