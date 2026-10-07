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
Partial Class usuwanie_p_seg
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(usuwanie_p_seg))
        Me.txtRozmiarMin = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtRozmiarMax = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtRozmiarSredni = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtProgKB = New System.Windows.Forms.TextBox()
        Me.btnUsun = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.cmbRozszerzenie = New System.Windows.Forms.ComboBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.lblLiczbaPlikow = New System.Windows.Forms.Label()
        Me.txtLiczbaPlikow = New System.Windows.Forms.TextBox()
        Me.txtOpis = New System.Windows.Forms.RichTextBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.txtDoUsuniecia = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtRozmiarMin
        '
        Me.txtRozmiarMin.Enabled = False
        Me.txtRozmiarMin.Location = New System.Drawing.Point(227, 156)
        Me.txtRozmiarMin.Name = "txtRozmiarMin"
        Me.txtRozmiarMin.Size = New System.Drawing.Size(84, 20)
        Me.txtRozmiarMin.TabIndex = 0
        Me.txtRozmiarMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 160)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(142, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Rozmiar najmniejszego pliku:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(18, 186)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(140, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Rozmiar największego pliku:"
        '
        'txtRozmiarMax
        '
        Me.txtRozmiarMax.Enabled = False
        Me.txtRozmiarMax.Location = New System.Drawing.Point(227, 182)
        Me.txtRozmiarMax.Name = "txtRozmiarMax"
        Me.txtRozmiarMax.Size = New System.Drawing.Size(84, 20)
        Me.txtRozmiarMax.TabIndex = 2
        Me.txtRozmiarMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(18, 212)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(101, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Średni rozmiar pliku:"
        '
        'txtRozmiarSredni
        '
        Me.txtRozmiarSredni.Enabled = False
        Me.txtRozmiarSredni.Location = New System.Drawing.Point(227, 208)
        Me.txtRozmiarSredni.Name = "txtRozmiarSredni"
        Me.txtRozmiarSredni.Size = New System.Drawing.Size(84, 20)
        Me.txtRozmiarSredni.TabIndex = 4
        Me.txtRozmiarSredni.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label4.Location = New System.Drawing.Point(17, 257)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(203, 20)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Usuń kafle mniejsze niż:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label5.Location = New System.Drawing.Point(110, 291)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(28, 20)
        Me.Label5.TabIndex = 8
        Me.Label5.Text = "kB"
        '
        'txtProgKB
        '
        Me.txtProgKB.Location = New System.Drawing.Point(21, 292)
        Me.txtProgKB.Name = "txtProgKB"
        Me.txtProgKB.Size = New System.Drawing.Size(84, 20)
        Me.txtProgKB.TabIndex = 7
        Me.txtProgKB.Text = "0"
        Me.txtProgKB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.ToolTip1.SetToolTip(Me.txtProgKB, "Sprawdź rozmiar przykładowego ""pustego"" segmentu i wprowadź tą wartość powiększon" &
        "ą o ok. 10%")
        '
        'btnUsun
        '
        Me.btnUsun.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnUsun.Location = New System.Drawing.Point(229, 284)
        Me.btnUsun.Name = "btnUsun"
        Me.btnUsun.Size = New System.Drawing.Size(84, 33)
        Me.btnUsun.TabIndex = 11
        Me.btnUsun.Text = "Usuń"
        Me.ToolTip1.SetToolTip(Me.btnUsun, "Usuń wskazane segmenty")
        Me.btnUsun.UseVisualStyleBackColor = True
        '
        'cmbRozszerzenie
        '
        Me.cmbRozszerzenie.FormattingEnabled = True
        Me.cmbRozszerzenie.Items.AddRange(New Object() {"jpg", "tif", "png", "png8", "png24", "png32", "gif", "svg+xml", "map", "gmi", "wld", "jpgw", "kml", "tab"})
        Me.cmbRozszerzenie.Location = New System.Drawing.Point(21, 42)
        Me.cmbRozszerzenie.Name = "cmbRozszerzenie"
        Me.cmbRozszerzenie.Size = New System.Drawing.Size(199, 21)
        Me.cmbRozszerzenie.TabIndex = 14
        Me.cmbRozszerzenie.Text = "jpg"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label8.Location = New System.Drawing.Point(17, 19)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(226, 20)
        Me.Label8.TabIndex = 15
        Me.Label8.Text = "Określ rozszerzenie plików:"
        '
        'lblLiczbaPlikow
        '
        Me.lblLiczbaPlikow.AutoSize = True
        Me.lblLiczbaPlikow.Location = New System.Drawing.Point(17, 107)
        Me.lblLiczbaPlikow.Name = "lblLiczbaPlikow"
        Me.lblLiczbaPlikow.Size = New System.Drawing.Size(142, 13)
        Me.lblLiczbaPlikow.TabIndex = 17
        Me.lblLiczbaPlikow.Text = "Liczba wszystkich plików jpg"
        '
        'txtLiczbaPlikow
        '
        Me.txtLiczbaPlikow.Enabled = False
        Me.txtLiczbaPlikow.Location = New System.Drawing.Point(227, 103)
        Me.txtLiczbaPlikow.Name = "txtLiczbaPlikow"
        Me.txtLiczbaPlikow.Size = New System.Drawing.Size(84, 20)
        Me.txtLiczbaPlikow.TabIndex = 16
        Me.txtLiczbaPlikow.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.ToolTip1.SetToolTip(Me.txtLiczbaPlikow, "Liczba plików jedynie o wskazanym rozszerzeniu")
        '
        'txtOpis
        '
        Me.txtOpis.Location = New System.Drawing.Point(327, 9)
        Me.txtOpis.Name = "txtOpis"
        Me.txtOpis.ReadOnly = True
        Me.txtOpis.Size = New System.Drawing.Size(314, 307)
        Me.txtOpis.TabIndex = 18
        Me.txtOpis.Text = resources.GetString("txtOpis.Text")
        '
        'txtDoUsuniecia
        '
        Me.txtDoUsuniecia.Enabled = False
        Me.txtDoUsuniecia.Location = New System.Drawing.Point(227, 130)
        Me.txtDoUsuniecia.Name = "txtDoUsuniecia"
        Me.txtDoUsuniecia.Size = New System.Drawing.Size(84, 20)
        Me.txtDoUsuniecia.TabIndex = 20
        Me.txtDoUsuniecia.Text = "0"
        Me.txtDoUsuniecia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.ToolTip1.SetToolTip(Me.txtDoUsuniecia, "Sprawdź rozmiar przykładowego ""pustego"" segmentu i wprowadź tą wartość powiększon" &
        "ą o ok. 10%")
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(17, 83)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(304, 13)
        Me.Label6.TabIndex = 19
        Me.Label6.Text = "Statystyki plików o określonym rozszerzeniu (katalog download)"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(18, 137)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(205, 13)
        Me.Label7.TabIndex = 21
        Me.Label7.Text = "Liczba plików zaznaczonych do usunięcia"
        '
        'usuwanie_p_seg
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(652, 328)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtDoUsuniecia)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtOpis)
        Me.Controls.Add(Me.lblLiczbaPlikow)
        Me.Controls.Add(Me.txtLiczbaPlikow)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.cmbRozszerzenie)
        Me.Controls.Add(Me.btnUsun)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtProgKB)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtRozmiarSredni)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtRozmiarMax)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtRozmiarMin)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "usuwanie_p_seg"
        Me.Text = "Usuwanie pustych segmentów"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtRozmiarMin As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtRozmiarMax As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtRozmiarSredni As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtProgKB As System.Windows.Forms.TextBox

    Private Sub Label5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label5.Click

    End Sub
    Friend WithEvents btnUsun As System.Windows.Forms.Button
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents cmbRozszerzenie As System.Windows.Forms.ComboBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents lblLiczbaPlikow As System.Windows.Forms.Label
    Friend WithEvents txtLiczbaPlikow As System.Windows.Forms.TextBox
    Friend WithEvents txtOpis As System.Windows.Forms.RichTextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtDoUsuniecia As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label


End Class