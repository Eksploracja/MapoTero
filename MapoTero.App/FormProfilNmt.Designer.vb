<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormProfilNmt
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblPlik = New System.Windows.Forms.Label()
        Me.txtPlik = New System.Windows.Forms.TextBox()
        Me.btnPlik = New System.Windows.Forms.Button()
        Me.lblOpis = New System.Windows.Forms.Label()
        Me.btnNmt = New System.Windows.Forms.Button()
        Me.btnZapiszGpx = New System.Windows.Forms.Button()
        Me.picProfil = New System.Windows.Forms.PictureBox()
        Me.prgNmt = New System.Windows.Forms.ProgressBar()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgPlik = New System.Windows.Forms.OpenFileDialog()
        Me.dlgZapiszGpx = New System.Windows.Forms.SaveFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.picProfil, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblPlik
        '
        Me.lblPlik.AutoSize = True
        Me.lblPlik.Location = New System.Drawing.Point(12, 12)
        Me.lblPlik.Name = "lblPlik"
        Me.lblPlik.Size = New System.Drawing.Size(150, 13)
        Me.lblPlik.TabIndex = 0
        Me.lblPlik.Text = "Plik trasy (GPX, KML, KMZ):"
        '
        'txtPlik
        '
        Me.txtPlik.Location = New System.Drawing.Point(12, 30)
        Me.txtPlik.Name = "txtPlik"
        Me.txtPlik.ReadOnly = True
        Me.txtPlik.Size = New System.Drawing.Size(440, 20)
        Me.txtPlik.TabIndex = 1
        '
        'btnPlik
        '
        Me.btnPlik.Location = New System.Drawing.Point(458, 28)
        Me.btnPlik.Name = "btnPlik"
        Me.btnPlik.Size = New System.Drawing.Size(90, 23)
        Me.btnPlik.TabIndex = 2
        Me.btnPlik.Text = "Wybierz..."
        Me.ToolTip1.SetToolTip(Me.btnPlik, "Ślad lub trasa zapisana np. w Mapy.com, Komoot, Locus Map, OsmAnd albo Google Earth.")
        Me.btnPlik.UseVisualStyleBackColor = True
        '
        'lblOpis
        '
        Me.lblOpis.Location = New System.Drawing.Point(12, 58)
        Me.lblOpis.Name = "lblOpis"
        Me.lblOpis.Size = New System.Drawing.Size(536, 30)
        Me.lblOpis.TabIndex = 3
        Me.lblOpis.Text = "Wskaż plik z trasą. Program pobierze wysokości terenu z NMT GUGiK (lotniczy skaning laserowy) dla punktów trasy - tylko na obszarze Polski."
        '
        'btnNmt
        '
        Me.btnNmt.Enabled = False
        Me.btnNmt.Location = New System.Drawing.Point(12, 92)
        Me.btnNmt.Name = "btnNmt"
        Me.btnNmt.Size = New System.Drawing.Size(210, 24)
        Me.btnNmt.TabIndex = 4
        Me.btnNmt.Text = "Pobierz wysokości z NMT"
        Me.ToolTip1.SetToolTip(Me.btnNmt, "Pobiera rzędne terenu z lotniczego skaningu laserowego (LIDAR) GUGiK dla punktów trasy.")
        Me.btnNmt.UseVisualStyleBackColor = True
        '
        'btnZapiszGpx
        '
        Me.btnZapiszGpx.Enabled = False
        Me.btnZapiszGpx.Location = New System.Drawing.Point(228, 92)
        Me.btnZapiszGpx.Name = "btnZapiszGpx"
        Me.btnZapiszGpx.Size = New System.Drawing.Size(210, 24)
        Me.btnZapiszGpx.TabIndex = 5
        Me.btnZapiszGpx.Text = "Zapisz GPX z wysokościami..."
        Me.ToolTip1.SetToolTip(Me.btnZapiszGpx, "Zapisuje ślad GPX z wysokościami terenu dla odbiorników Garmin i zegarków sportowych.")
        Me.btnZapiszGpx.UseVisualStyleBackColor = True
        '
        'picProfil
        '
        Me.picProfil.BackColor = System.Drawing.Color.White
        Me.picProfil.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picProfil.Location = New System.Drawing.Point(12, 124)
        Me.picProfil.Name = "picProfil"
        Me.picProfil.Size = New System.Drawing.Size(536, 200)
        Me.picProfil.TabIndex = 6
        Me.picProfil.TabStop = False
        '
        'prgNmt
        '
        Me.prgNmt.Location = New System.Drawing.Point(100, 338)
        Me.prgNmt.Name = "prgNmt"
        Me.prgNmt.Size = New System.Drawing.Size(250, 20)
        Me.prgNmt.TabIndex = 7
        Me.prgNmt.Visible = False
        '
        'btnZamknij
        '
        Me.btnZamknij.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnZamknij.Location = New System.Drawing.Point(468, 335)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(80, 26)
        Me.btnZamknij.TabIndex = 8
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'dlgPlik
        '
        Me.dlgPlik.Filter = "Trasy (*.gpx;*.kml;*.kmz)|*.gpx;*.kml;*.kmz|Wszystkie pliki (*.*)|*.*"
        Me.dlgPlik.Title = "Plik trasy"
        '
        'dlgZapiszGpx
        '
        Me.dlgZapiszGpx.Filter = "Ślad GPX (*.gpx)|*.gpx|Wszystkie pliki (*.*)|*.*"
        Me.dlgZapiszGpx.Title = "Zapisz ślad z wysokościami NMT"
        '
        'FormProfilNmt
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnZamknij
        Me.ClientSize = New System.Drawing.Size(560, 373)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.prgNmt)
        Me.Controls.Add(Me.picProfil)
        Me.Controls.Add(Me.btnZapiszGpx)
        Me.Controls.Add(Me.btnNmt)
        Me.Controls.Add(Me.lblOpis)
        Me.Controls.Add(Me.btnPlik)
        Me.Controls.Add(Me.txtPlik)
        Me.Controls.Add(Me.lblPlik)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormProfilNmt"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Profil wysokościowy trasy"
        CType(Me.picProfil, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblPlik As System.Windows.Forms.Label
    Friend WithEvents txtPlik As System.Windows.Forms.TextBox
    Friend WithEvents btnPlik As System.Windows.Forms.Button
    Friend WithEvents lblOpis As System.Windows.Forms.Label
    Friend WithEvents btnNmt As System.Windows.Forms.Button
    Friend WithEvents btnZapiszGpx As System.Windows.Forms.Button
    Friend WithEvents picProfil As System.Windows.Forms.PictureBox
    Friend WithEvents prgNmt As System.Windows.Forms.ProgressBar
    Friend WithEvents btnZamknij As System.Windows.Forms.Button
    Friend WithEvents dlgPlik As System.Windows.Forms.OpenFileDialog
    Friend WithEvents dlgZapiszGpx As System.Windows.Forms.SaveFileDialog
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
