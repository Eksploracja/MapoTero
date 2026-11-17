<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormKorektaFolderu
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblFolderZrodlo = New System.Windows.Forms.Label()
        Me.txtFolderZrodlo = New System.Windows.Forms.TextBox()
        Me.btnWybierzZrodlo = New System.Windows.Forms.Button()
        Me.lblFolderCel = New System.Windows.Forms.Label()
        Me.txtFolderCel = New System.Windows.Forms.TextBox()
        Me.btnWybierzCel = New System.Windows.Forms.Button()
        Me.grpParametry = New System.Windows.Forms.GroupBox()
        Me.lblProfilOpis = New System.Windows.Forms.Label()
        Me.btnDostosuj = New System.Windows.Forms.Button()
        Me.chkKopiujGeo = New System.Windows.Forms.CheckBox()
        Me.prgPostep = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpParametry.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblFolderZrodlo
        '
        Me.lblFolderZrodlo.AutoSize = True
        Me.lblFolderZrodlo.Location = New System.Drawing.Point(12, 15)
        Me.lblFolderZrodlo.Name = "lblFolderZrodlo"
        Me.lblFolderZrodlo.Size = New System.Drawing.Size(125, 13)
        Me.lblFolderZrodlo.TabIndex = 0
        Me.lblFolderZrodlo.Text = "Folder z obrazami (źródło):"
        '
        'txtFolderZrodlo
        '
        Me.txtFolderZrodlo.Location = New System.Drawing.Point(15, 31)
        Me.txtFolderZrodlo.Name = "txtFolderZrodlo"
        Me.txtFolderZrodlo.Size = New System.Drawing.Size(430, 20)
        Me.txtFolderZrodlo.TabIndex = 1
        '
        'btnWybierzZrodlo
        '
        Me.btnWybierzZrodlo.Location = New System.Drawing.Point(451, 29)
        Me.btnWybierzZrodlo.Name = "btnWybierzZrodlo"
        Me.btnWybierzZrodlo.Size = New System.Drawing.Size(85, 23)
        Me.btnWybierzZrodlo.TabIndex = 2
        Me.btnWybierzZrodlo.Text = "Przeglądaj..."
        Me.btnWybierzZrodlo.UseVisualStyleBackColor = True
        '
        'lblFolderCel
        '
        Me.lblFolderCel.AutoSize = True
        Me.lblFolderCel.Location = New System.Drawing.Point(12, 64)
        Me.lblFolderCel.Name = "lblFolderCel"
        Me.lblFolderCel.Size = New System.Drawing.Size(142, 13)
        Me.lblFolderCel.TabIndex = 3
        Me.lblFolderCel.Text = "Folder wynikowy (docelowy):"
        '
        'txtFolderCel
        '
        Me.txtFolderCel.Location = New System.Drawing.Point(15, 80)
        Me.txtFolderCel.Name = "txtFolderCel"
        Me.txtFolderCel.Size = New System.Drawing.Size(430, 20)
        Me.txtFolderCel.TabIndex = 4
        '
        'btnWybierzCel
        '
        Me.btnWybierzCel.Location = New System.Drawing.Point(451, 78)
        Me.btnWybierzCel.Name = "btnWybierzCel"
        Me.btnWybierzCel.Size = New System.Drawing.Size(85, 23)
        Me.btnWybierzCel.TabIndex = 5
        Me.btnWybierzCel.Text = "Przeglądaj..."
        Me.btnWybierzCel.UseVisualStyleBackColor = True
        '
        'grpParametry
        '
        Me.grpParametry.Controls.Add(Me.lblProfilOpis)
        Me.grpParametry.Controls.Add(Me.btnDostosuj)
        Me.grpParametry.Controls.Add(Me.chkKopiujGeo)
        Me.grpParametry.Location = New System.Drawing.Point(15, 115)
        Me.grpParametry.Name = "grpParametry"
        Me.grpParametry.Size = New System.Drawing.Size(521, 95)
        Me.grpParametry.TabIndex = 6
        Me.grpParametry.TabStop = False
        Me.grpParametry.Text = "Parametry przetwarzania obrazu"
        '
        'lblProfilOpis
        '
        Me.lblProfilOpis.AutoSize = True
        Me.lblProfilOpis.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.lblProfilOpis.Location = New System.Drawing.Point(15, 25)
        Me.lblProfilOpis.Name = "lblProfilOpis"
        Me.lblProfilOpis.Size = New System.Drawing.Size(255, 13)
        Me.lblProfilOpis.TabIndex = 0
        Me.lblProfilOpis.Text = "Profil: Ekran w słońcu / Garmin MIP (zalecany)"
        '
        'btnDostosuj
        '
        Me.btnDostosuj.Location = New System.Drawing.Point(15, 50)
        Me.btnDostosuj.Name = "btnDostosuj"
        Me.btnDostosuj.Size = New System.Drawing.Size(260, 26)
        Me.btnDostosuj.TabIndex = 1
        Me.btnDostosuj.Text = "Dostosuj parametry (kontrast, ostrość, MIP)..."
        Me.btnDostosuj.UseVisualStyleBackColor = True
        '
        'chkKopiujGeo
        '
        Me.chkKopiujGeo.AutoSize = True
        Me.chkKopiujGeo.Checked = True
        Me.chkKopiujGeo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkKopiujGeo.Location = New System.Drawing.Point(295, 56)
        Me.chkKopiujGeo.Name = "chkKopiujGeo"
        Me.chkKopiujGeo.Size = New System.Drawing.Size(215, 17)
        Me.chkKopiujGeo.TabIndex = 2
        Me.chkKopiujGeo.Text = "Kopiuj pliki georeferencji (.wld, .map itp.)"
        Me.chkKopiujGeo.UseVisualStyleBackColor = True
        '
        'prgPostep
        '
        Me.prgPostep.Location = New System.Drawing.Point(15, 225)
        Me.prgPostep.Name = "prgPostep"
        Me.prgPostep.Size = New System.Drawing.Size(521, 23)
        Me.prgPostep.TabIndex = 7
        '
        'lblStatus
        '
        Me.lblStatus.Location = New System.Drawing.Point(15, 255)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(521, 20)
        Me.lblStatus.TabIndex = 8
        Me.lblStatus.Text = "Gotowy do przetwarzania."
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btnStart.Location = New System.Drawing.Point(215, 285)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(150, 30)
        Me.btnStart.TabIndex = 9
        Me.btnStart.Text = "Rozpocznij korektę"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(375, 285)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(75, 30)
        Me.btnPrzerwij.TabIndex = 10
        Me.btnPrzerwij.Text = "Przerwij"
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'btnZamknij
        '
        Me.btnZamknij.Location = New System.Drawing.Point(461, 285)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(75, 30)
        Me.btnZamknij.TabIndex = 11
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'FormKorektaFolderu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(550, 328)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.btnPrzerwij)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.prgPostep)
        Me.Controls.Add(Me.grpParametry)
        Me.Controls.Add(Me.btnWybierzCel)
        Me.Controls.Add(Me.txtFolderCel)
        Me.Controls.Add(Me.lblFolderCel)
        Me.Controls.Add(Me.btnWybierzZrodlo)
        Me.Controls.Add(Me.txtFolderZrodlo)
        Me.Controls.Add(Me.lblFolderZrodlo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormKorektaFolderu"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Korekta i optymalizacja obrazów w folderze"
        Me.grpParametry.ResumeLayout(False)
        Me.grpParametry.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblFolderZrodlo As Label
    Friend WithEvents txtFolderZrodlo As TextBox
    Friend WithEvents btnWybierzZrodlo As Button
    Friend WithEvents lblFolderCel As Label
    Friend WithEvents txtFolderCel As TextBox
    Friend WithEvents btnWybierzCel As Button
    Friend WithEvents grpParametry As GroupBox
    Friend WithEvents lblProfilOpis As Label
    Friend WithEvents btnDostosuj As Button
    Friend WithEvents chkKopiujGeo As CheckBox
    Friend WithEvents prgPostep As ProgressBar
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnStart As Button
    Friend WithEvents btnPrzerwij As Button
    Friend WithEvents btnZamknij As Button
    Friend WithEvents dlgFolder As FolderBrowserDialog
    Friend WithEvents ToolTip1 As ToolTip
End Class