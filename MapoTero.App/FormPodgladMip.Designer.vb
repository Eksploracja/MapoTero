Imports System.Drawing
Imports System.Windows.Forms

Partial Class FormPodgladMip
    Inherits Form

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Me.picPodglad = New PictureBox()
        Me.rbPodgladMip = New RadioButton()
        Me.rbOryginal = New RadioButton()
        Me.lblGamma = New Label()
        Me.tbGamma = New TrackBar()
        Me.lblGammaVal = New Label()
        Me.lblKontrast = New Label()
        Me.tbKontrast = New TrackBar()
        Me.lblKontrastVal = New Label()
        Me.lblNasycenie = New Label()
        Me.tbNasycenie = New TrackBar()
        Me.lblNasycenieVal = New Label()
        Me.lblOstrz = New Label()
        Me.tbOstrz = New TrackBar()
        Me.lblOstrzVal = New Label()
        Me.btnDomyslne = New Button()
        Me.btnZastosuj = New Button()
        Me.btnAnuluj = New Button()
        Me.lblOpis = New Label()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbGamma, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbKontrast, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbNasycenie, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbOstrz, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picPodglad
        '
        Me.picPodglad.BorderStyle = BorderStyle.FixedSingle
        Me.picPodglad.Location = New Point(12, 12)
        Me.picPodglad.Name = "picPodglad"
        Me.picPodglad.Size = New Size(240, 240)
        Me.picPodglad.SizeMode = PictureBoxSizeMode.Zoom
        Me.picPodglad.TabIndex = 0
        Me.picPodglad.TabStop = False
        '
        'rbPodgladMip
        '
        Me.rbPodgladMip.AutoSize = True
        Me.rbPodgladMip.Checked = True
        Me.rbPodgladMip.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.rbPodgladMip.Location = New Point(12, 260)
        Me.rbPodgladMip.Name = "rbPodgladMip"
        Me.rbPodgladMip.Size = New Size(160, 19)
        Me.rbPodgladMip.TabIndex = 1
        Me.rbPodgladMip.TabStop = True
        Me.rbPodgladMip.Text = "Po korekcji (MIP słońce)"
        Me.rbPodgladMip.UseVisualStyleBackColor = True
        '
        'rbOryginal
        '
        Me.rbOryginal.AutoSize = True
        Me.rbOryginal.Location = New Point(175, 260)
        Me.rbOryginal.Name = "rbOryginal"
        Me.rbOryginal.Size = New Size(71, 19)
        Me.rbOryginal.TabIndex = 2
        Me.rbOryginal.Text = "Oryginał"
        Me.rbOryginal.UseVisualStyleBackColor = True
        '
        'lblOpis
        '
        Me.lblOpis.Location = New Point(265, 12)
        Me.lblOpis.Name = "lblOpis"
        Me.lblOpis.Size = New Size(250, 40)
        Me.lblOpis.TabIndex = 3
        Me.lblOpis.Text = "Symulacja i optymalizacja kontrastu dla ekranów Garmin Chroma / MIP w pełnym słońcu."
        '
        'lblGamma
        '
        Me.lblGamma.AutoSize = True
        Me.lblGamma.Location = New Point(265, 55)
        Me.lblGamma.Name = "lblGamma"
        Me.lblGamma.Size = New Size(130, 13)
        Me.lblGamma.TabIndex = 4
        Me.lblGamma.Text = "Jasność / Gamma (lasy/cienie):"
        '
        'tbGamma
        '
        Me.tbGamma.LargeChange = 5
        Me.tbGamma.Location = New Point(265, 72)
        Me.tbGamma.Maximum = 120
        Me.tbGamma.Minimum = 50
        Me.tbGamma.Name = "tbGamma"
        Me.tbGamma.Size = New Size(190, 45)
        Me.tbGamma.TabIndex = 5
        Me.tbGamma.TickFrequency = 10
        Me.tbGamma.Value = 75
        '
        'lblGammaVal
        '
        Me.lblGammaVal.AutoSize = True
        Me.lblGammaVal.Location = New Point(465, 75)
        Me.lblGammaVal.Name = "lblGammaVal"
        Me.lblGammaVal.Size = New Size(28, 13)
        Me.lblGammaVal.TabIndex = 6
        Me.lblGammaVal.Text = "0.75"
        '
        'lblKontrast
        '
        Me.lblKontrast.AutoSize = True
        Me.lblKontrast.Location = New Point(265, 110)
        Me.lblKontrast.Name = "lblKontrast"
        Me.lblKontrast.Size = New Size(50, 13)
        Me.lblKontrast.TabIndex = 7
        Me.lblKontrast.Text = "Kontrast:"
        '
        'tbKontrast
        '
        Me.tbKontrast.LargeChange = 5
        Me.tbKontrast.Location = New Point(265, 127)
        Me.tbKontrast.Maximum = 200
        Me.tbKontrast.Minimum = 100
        Me.tbKontrast.Name = "tbKontrast"
        Me.tbKontrast.Size = New Size(190, 45)
        Me.tbKontrast.TabIndex = 8
        Me.tbKontrast.TickFrequency = 10
        Me.tbKontrast.Value = 125
        '
        'lblKontrastVal
        '
        Me.lblKontrastVal.AutoSize = True
        Me.lblKontrastVal.Location = New Point(465, 130)
        Me.lblKontrastVal.Name = "lblKontrastVal"
        Me.lblKontrastVal.Size = New Size(34, 13)
        Me.lblKontrastVal.TabIndex = 9
        Me.lblKontrastVal.Text = "+25%"
        '
        'lblNasycenie
        '
        Me.lblNasycenie.AutoSize = True
        Me.lblNasycenie.Location = New Point(265, 165)
        Me.lblNasycenie.Name = "lblNasycenie"
        Me.lblNasycenie.Size = New Size(88, 13)
        Me.lblNasycenie.TabIndex = 10
        Me.lblNasycenie.Text = "Nasycenie barw:"
        '
        'tbNasycenie
        '
        Me.tbNasycenie.LargeChange = 5
        Me.tbNasycenie.Location = New Point(265, 182)
        Me.tbNasycenie.Maximum = 200
        Me.tbNasycenie.Minimum = 100
        Me.tbNasycenie.Name = "tbNasycenie"
        Me.tbNasycenie.Size = New Size(190, 45)
        Me.tbNasycenie.TabIndex = 11
        Me.tbNasycenie.TickFrequency = 10
        Me.tbNasycenie.Value = 135
        '
        'lblNasycenieVal
        '
        Me.lblNasycenieVal.AutoSize = True
        Me.lblNasycenieVal.Location = New Point(465, 185)
        Me.lblNasycenieVal.Name = "lblNasycenieVal"
        Me.lblNasycenieVal.Size = New Size(34, 13)
        Me.lblNasycenieVal.TabIndex = 12
        Me.lblNasycenieVal.Text = "+35%"
        '
        'lblOstrz
        '
        Me.lblOstrz.AutoSize = True
        Me.lblOstrz.Location = New Point(265, 220)
        Me.lblOstrz.Name = "lblOstrz"
        Me.lblOstrz.Size = New Size(107, 13)
        Me.lblOstrz.TabIndex = 13
        Me.lblOstrz.Text = "Wyostrzanie krawędzi:"
        '
        'tbOstrz
        '
        Me.tbOstrz.LargeChange = 2
        Me.tbOstrz.Location = New Point(265, 237)
        Me.tbOstrz.Maximum = 150
        Me.tbOstrz.Minimum = 0
        Me.tbOstrz.Name = "tbOstrz"
        Me.tbOstrz.Size = New Size(190, 45)
        Me.tbOstrz.TabIndex = 14
        Me.tbOstrz.TickFrequency = 15
        Me.tbOstrz.Value = 60
        '
        'lblOstrzVal
        '
        Me.lblOstrzVal.AutoSize = True
        Me.lblOstrzVal.Location = New Point(465, 240)
        Me.lblOstrzVal.Name = "lblOstrzVal"
        Me.lblOstrzVal.Size = New Size(22, 13)
        Me.lblOstrzVal.TabIndex = 15
        Me.lblOstrzVal.Text = "0.6"
        '
        'btnDomyslne
        '
        Me.btnDomyslne.Location = New Point(12, 295)
        Me.btnDomyslne.Name = "btnDomyslne"
        Me.btnDomyslne.Size = New Size(130, 26)
        Me.btnDomyslne.TabIndex = 16
        Me.btnDomyslne.Text = "Domyślne Garmin"
        Me.btnDomyslne.UseVisualStyleBackColor = True
        '
        'btnZastosuj
        '
        Me.btnZastosuj.DialogResult = DialogResult.OK
        Me.btnZastosuj.Location = New Point(325, 295)
        Me.btnZastosuj.Name = "btnZastosuj"
        Me.btnZastosuj.Size = New Size(90, 26)
        Me.btnZastosuj.TabIndex = 17
        Me.btnZastosuj.Text = "Zastosuj"
        Me.btnZastosuj.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = DialogResult.Cancel
        Me.btnAnuluj.Location = New Point(425, 295)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New Size(85, 26)
        Me.btnAnuluj.TabIndex = 18
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'FormPodgladMip
        '
        Me.AcceptButton = Me.btnZastosuj
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New Size(524, 335)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnZastosuj)
        Me.Controls.Add(Me.btnDomyslne)
        Me.Controls.Add(Me.lblOstrzVal)
        Me.Controls.Add(Me.tbOstrz)
        Me.Controls.Add(Me.lblOstrz)
        Me.Controls.Add(Me.lblNasycenieVal)
        Me.Controls.Add(Me.tbNasycenie)
        Me.Controls.Add(Me.lblNasycenie)
        Me.Controls.Add(Me.lblKontrastVal)
        Me.Controls.Add(Me.tbKontrast)
        Me.Controls.Add(Me.lblKontrast)
        Me.Controls.Add(Me.lblGammaVal)
        Me.Controls.Add(Me.tbGamma)
        Me.Controls.Add(Me.lblGamma)
        Me.Controls.Add(Me.lblOpis)
        Me.Controls.Add(Me.rbOryginal)
        Me.Controls.Add(Me.rbPodgladMip)
        Me.Controls.Add(Me.picPodglad)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormPodgladMip"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "Optymalizacja obrazu pod ekrany MIP (Garmin / pełne słońce)"
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbGamma, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbKontrast, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbNasycenie, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbOstrz, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents picPodglad As PictureBox
    Friend WithEvents rbPodgladMip As RadioButton
    Friend WithEvents rbOryginal As RadioButton
    Friend WithEvents lblOpis As Label
    Friend WithEvents lblGamma As Label
    Friend WithEvents tbGamma As TrackBar
    Friend WithEvents lblGammaVal As Label
    Friend WithEvents lblKontrast As Label
    Friend WithEvents tbKontrast As TrackBar
    Friend WithEvents lblKontrastVal As Label
    Friend WithEvents lblNasycenie As Label
    Friend WithEvents tbNasycenie As TrackBar
    Friend WithEvents lblNasycenieVal As Label
    Friend WithEvents lblOstrz As Label
    Friend WithEvents tbOstrz As TrackBar
    Friend WithEvents lblOstrzVal As Label
    Friend WithEvents btnDomyslne As Button
    Friend WithEvents btnZastosuj As Button
    Friend WithEvents btnAnuluj As Button
End Class
