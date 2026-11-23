<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormZasiegXY
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblUkladInfo = New System.Windows.Forms.Label()
        Me.grpWspolrzedne = New System.Windows.Forms.GroupBox()
        Me.btnZaokraglijKm = New System.Windows.Forms.Button()
        Me.txtYPrawy = New System.Windows.Forms.TextBox()
        Me.lblYPrawy = New System.Windows.Forms.Label()
        Me.txtYLewy = New System.Windows.Forms.TextBox()
        Me.lblYLewy = New System.Windows.Forms.Label()
        Me.txtXDol = New System.Windows.Forms.TextBox()
        Me.lblXDol = New System.Windows.Forms.Label()
        Me.txtXGora = New System.Windows.Forms.TextBox()
        Me.lblXGora = New System.Windows.Forms.Label()
        Me.btnZastosuj = New System.Windows.Forms.Button()
        Me.btnAnuluj = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpWspolrzedne.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblUkladInfo
        '
        Me.lblUkladInfo.AutoSize = True
        Me.lblUkladInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblUkladInfo.ForeColor = System.Drawing.Color.Navy
        Me.lblUkladInfo.Location = New System.Drawing.Point(12, 12)
        Me.lblUkladInfo.Name = "lblUkladInfo"
        Me.lblUkladInfo.Size = New System.Drawing.Size(184, 15)
        Me.lblUkladInfo.TabIndex = 0
        Me.lblUkladInfo.Text = "Układ współrzędnych: PL-1992"
        '
        'grpWspolrzedne
        '
        Me.grpWspolrzedne.Controls.Add(Me.btnZaokraglijKm)
        Me.grpWspolrzedne.Controls.Add(Me.txtYPrawy)
        Me.grpWspolrzedne.Controls.Add(Me.lblYPrawy)
        Me.grpWspolrzedne.Controls.Add(Me.txtYLewy)
        Me.grpWspolrzedne.Controls.Add(Me.lblYLewy)
        Me.grpWspolrzedne.Controls.Add(Me.txtXDol)
        Me.grpWspolrzedne.Controls.Add(Me.lblXDol)
        Me.grpWspolrzedne.Controls.Add(Me.txtXGora)
        Me.grpWspolrzedne.Controls.Add(Me.lblXGora)
        Me.grpWspolrzedne.Location = New System.Drawing.Point(12, 34)
        Me.grpWspolrzedne.Name = "grpWspolrzedne"
        Me.grpWspolrzedne.Size = New System.Drawing.Size(340, 160)
        Me.grpWspolrzedne.TabIndex = 1
        Me.grpWspolrzedne.TabStop = False
        Me.grpWspolrzedne.Text = "Współrzędne granic zasięgu"
        '
        'btnZaokraglijKm
        '
        Me.btnZaokraglijKm.Location = New System.Drawing.Point(180, 122)
        Me.btnZaokraglijKm.Name = "btnZaokraglijKm"
        Me.btnZaokraglijKm.Size = New System.Drawing.Size(145, 26)
        Me.btnZaokraglijKm.TabIndex = 8
        Me.btnZaokraglijKm.Text = "Zaokrąglij do km"
        Me.ToolTip1.SetToolTip(Me.btnZaokraglijKm, "Zaokrągla współrzędne do pełnych kilometrów (wielokrotności 1000 m).")
        Me.btnZaokraglijKm.UseVisualStyleBackColor = True
        '
        'txtYPrawy
        '
        Me.txtYPrawy.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtYPrawy.Location = New System.Drawing.Point(180, 88)
        Me.txtYPrawy.Name = "txtYPrawy"
        Me.txtYPrawy.Size = New System.Drawing.Size(145, 23)
        Me.txtYPrawy.TabIndex = 7
        '
        'lblYPrawy
        '
        Me.lblYPrawy.AutoSize = True
        Me.lblYPrawy.Location = New System.Drawing.Point(180, 72)
        Me.lblYPrawy.Name = "lblYPrawy"
        Me.lblYPrawy.Size = New System.Drawing.Size(117, 13)
        Me.lblYPrawy.TabIndex = 6
        Me.lblYPrawy.Text = "Wschód (Y prawy / E):"
        '
        'txtYLewy
        '
        Me.txtYLewy.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtYLewy.Location = New System.Drawing.Point(15, 88)
        Me.txtYLewy.Name = "txtYLewy"
        Me.txtYLewy.Size = New System.Drawing.Size(145, 23)
        Me.txtYLewy.TabIndex = 5
        '
        'lblYLewy
        '
        Me.lblYLewy.AutoSize = True
        Me.lblYLewy.Location = New System.Drawing.Point(15, 72)
        Me.lblYLewy.Name = "lblYLewy"
        Me.lblYLewy.Size = New System.Drawing.Size(115, 13)
        Me.lblYLewy.TabIndex = 4
        Me.lblYLewy.Text = "Zachód (Y lewy / W):"
        '
        'txtXDol
        '
        Me.txtXDol.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtXDol.Location = New System.Drawing.Point(180, 38)
        Me.txtXDol.Name = "txtXDol"
        Me.txtXDol.Size = New System.Drawing.Size(145, 23)
        Me.txtXDol.TabIndex = 3
        '
        'lblXDol
        '
        Me.lblXDol.AutoSize = True
        Me.lblXDol.Location = New System.Drawing.Point(180, 22)
        Me.lblXDol.Name = "lblXDol"
        Me.lblXDol.Size = New System.Drawing.Size(118, 13)
        Me.lblXDol.TabIndex = 2
        Me.lblXDol.Text = "Południe (X dolny / S):"
        '
        'txtXGora
        '
        Me.txtXGora.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtXGora.Location = New System.Drawing.Point(15, 38)
        Me.txtXGora.Name = "txtXGora"
        Me.txtXGora.Size = New System.Drawing.Size(145, 23)
        Me.txtXGora.TabIndex = 1
        '
        'lblXGora
        '
        Me.lblXGora.AutoSize = True
        Me.lblXGora.Location = New System.Drawing.Point(15, 22)
        Me.lblXGora.Name = "lblXGora"
        Me.lblXGora.Size = New System.Drawing.Size(114, 13)
        Me.lblXGora.TabIndex = 0
        Me.lblXGora.Text = "Północ (X górny / N):"
        '
        'btnZastosuj
        '
        Me.btnZastosuj.Location = New System.Drawing.Point(176, 204)
        Me.btnZastosuj.Name = "btnZastosuj"
        Me.btnZastosuj.Size = New System.Drawing.Size(85, 26)
        Me.btnZastosuj.TabIndex = 2
        Me.btnZastosuj.Text = "Zastosuj"
        Me.btnZastosuj.UseVisualStyleBackColor = True
        '
        'btnAnuluj
        '
        Me.btnAnuluj.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnAnuluj.Location = New System.Drawing.Point(267, 204)
        Me.btnAnuluj.Name = "btnAnuluj"
        Me.btnAnuluj.Size = New System.Drawing.Size(85, 26)
        Me.btnAnuluj.TabIndex = 3
        Me.btnAnuluj.Text = "Anuluj"
        Me.btnAnuluj.UseVisualStyleBackColor = True
        '
        'FormZasiegXY
        '
        Me.AcceptButton = Me.btnZastosuj
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnAnuluj
        Me.ClientSize = New System.Drawing.Size(364, 240)
        Me.Controls.Add(Me.btnAnuluj)
        Me.Controls.Add(Me.btnZastosuj)
        Me.Controls.Add(Me.grpWspolrzedne)
        Me.Controls.Add(Me.lblUkladInfo)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormZasiegXY"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Współrzędne zasięgu pobierania"
        Me.grpWspolrzedne.ResumeLayout(False)
        Me.grpWspolrzedne.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblUkladInfo As System.Windows.Forms.Label
    Friend WithEvents grpWspolrzedne As System.Windows.Forms.GroupBox
    Friend WithEvents txtYPrawy As System.Windows.Forms.TextBox
    Friend WithEvents lblYPrawy As System.Windows.Forms.Label
    Friend WithEvents txtYLewy As System.Windows.Forms.TextBox
    Friend WithEvents lblYLewy As System.Windows.Forms.Label
    Friend WithEvents txtXDol As System.Windows.Forms.TextBox
    Friend WithEvents lblXDol As System.Windows.Forms.Label
    Friend WithEvents txtXGora As System.Windows.Forms.TextBox
    Friend WithEvents lblXGora As System.Windows.Forms.Label
    Friend WithEvents btnZaokraglijKm As System.Windows.Forms.Button
    Friend WithEvents btnZastosuj As System.Windows.Forms.Button
    Friend WithEvents btnAnuluj As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
