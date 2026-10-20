<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormKorytarz
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
        Me.lblSzerokosc = New System.Windows.Forms.Label()
        Me.nudSzerokosc = New System.Windows.Forms.NumericUpDown()
        Me.lblOpis = New System.Windows.Forms.Label()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.dlgPlik = New System.Windows.Forms.OpenFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.nudSzerokosc, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.txtPlik.Size = New System.Drawing.Size(340, 20)
        Me.txtPlik.TabIndex = 1
        '
        'btnPlik
        '
        Me.btnPlik.Location = New System.Drawing.Point(358, 28)
        Me.btnPlik.Name = "btnPlik"
        Me.btnPlik.Size = New System.Drawing.Size(90, 23)
        Me.btnPlik.TabIndex = 2
        Me.btnPlik.Text = "wybierz..."
        Me.ToolTip1.SetToolTip(Me.btnPlik, "Ślad lub trasa zapisana np. w Mapy.com, Komoot, Locus Map, OsmAnd albo Google Earth.")
        Me.btnPlik.UseVisualStyleBackColor = True
        '
        'lblSzerokosc
        '
        Me.lblSzerokosc.AutoSize = True
        Me.lblSzerokosc.Location = New System.Drawing.Point(12, 66)
        Me.lblSzerokosc.Name = "lblSzerokosc"
        Me.lblSzerokosc.Size = New System.Drawing.Size(240, 13)
        Me.lblSzerokosc.TabIndex = 3
        Me.lblSzerokosc.Text = "Szerokość pasa po każdej stronie trasy [m]:"
        '
        'nudSzerokosc
        '
        Me.nudSzerokosc.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudSzerokosc.Location = New System.Drawing.Point(268, 64)
        Me.nudSzerokosc.Maximum = New Decimal(New Integer() {50000, 0, 0, 0})
        Me.nudSzerokosc.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudSzerokosc.Name = "nudSzerokosc"
        Me.nudSzerokosc.Size = New System.Drawing.Size(84, 20)
        Me.nudSzerokosc.TabIndex = 4
        Me.nudSzerokosc.Value = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.ToolTip1.SetToolTip(Me.nudSzerokosc, "Pobrane zostaną segmenty, które choć częściowo leżą nie dalej od trasy niż ta odległość.")
        '
        'lblOpis
        '
        Me.lblOpis.Location = New System.Drawing.Point(12, 96)
        Me.lblOpis.Name = "lblOpis"
        Me.lblOpis.Size = New System.Drawing.Size(436, 80)
        Me.lblOpis.TabIndex = 5
        Me.lblOpis.Text = "Wskaż plik z trasą. Pobierane będą tylko segmenty w pasie wzdłuż trasy - zasięg pobierania zostanie ustawiony na prostokąt obejmujący cały pas."
        '
        'btnOK
        '
        Me.btnOK.Enabled = False
        Me.btnOK.Location = New System.Drawing.Point(282, 186)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(80, 26)
        Me.btnOK.TabIndex = 6
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(368, 186)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(80, 26)
        Me.btnAnuluj.TabIndex = 7
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'dlgPlik
        '
        Me.dlgPlik.Filter = "Trasy (*.gpx;*.kml;*.kmz)|*.gpx;*.kml;*.kmz|Wszystkie pliki (*.*)|*.*"
        Me.dlgPlik.Title = "Plik trasy"
        '
        'FormKorytarz
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(460, 224)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.lblOpis)
        Me.Controls.Add(Me.nudSzerokosc)
        Me.Controls.Add(Me.lblSzerokosc)
        Me.Controls.Add(Me.btnPlik)
        Me.Controls.Add(Me.txtPlik)
        Me.Controls.Add(Me.lblPlik)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormKorytarz"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Obszar pobierania wzdłuż trasy"
        CType(Me.nudSzerokosc, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblPlik As System.Windows.Forms.Label
    Friend WithEvents txtPlik As System.Windows.Forms.TextBox
    Friend WithEvents btnPlik As System.Windows.Forms.Button
    Friend WithEvents lblSzerokosc As System.Windows.Forms.Label
    Friend WithEvents nudSzerokosc As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblOpis As System.Windows.Forms.Label
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents dlgPlik As System.Windows.Forms.OpenFileDialog
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
