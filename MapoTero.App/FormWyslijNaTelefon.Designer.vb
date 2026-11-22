'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormWyslijNaTelefon
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
        Me.lblTytul = New System.Windows.Forms.Label()
        Me.lblInstrukcja = New System.Windows.Forms.Label()
        Me.lblPlikInfo = New System.Windows.Forms.Label()
        Me.picQr = New System.Windows.Forms.PictureBox()
        Me.lblSiec = New System.Windows.Forms.Label()
        Me.cmbInterfejsy = New System.Windows.Forms.ComboBox()
        Me.txtAdres = New System.Windows.Forms.TextBox()
        Me.btnKopiuj = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.picQr, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblTytul
        '
        Me.lblTytul.AutoSize = True
        Me.lblTytul.Font = New System.Drawing.Font("Segoe UI", 10.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.lblTytul.Location = New System.Drawing.Point(12, 9)
        Me.lblTytul.Name = "lblTytul"
        Me.lblTytul.Size = New System.Drawing.Size(256, 19)
        Me.lblTytul.TabIndex = 0
        Me.lblTytul.Text = "Prześlij mapę na telefon przez Wi-Fi"
        '
        'lblInstrukcja
        '
        Me.lblInstrukcja.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.lblInstrukcja.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblInstrukcja.Location = New System.Drawing.Point(13, 33)
        Me.lblInstrukcja.Name = "lblInstrukcja"
        Me.lblInstrukcja.Size = New System.Drawing.Size(434, 38)
        Me.lblInstrukcja.TabIndex = 1
        Me.lblInstrukcja.Text = "Zeskanuj aparatem telefonu kod QR, aby pobrać mapę przez sieć Wi-Fi (oba urządzen" &
    "ia w tej samej sieci). Przy pierwszym użyciu zaakceptuj alert Zapory Windows (De" &
    "fender), jeśli się pojawi."
        '
        'lblPlikInfo
        '
        Me.lblPlikInfo.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.lblPlikInfo.Location = New System.Drawing.Point(13, 70)
        Me.lblPlikInfo.Name = "lblPlikInfo"
        Me.lblPlikInfo.Size = New System.Drawing.Size(434, 20)
        Me.lblPlikInfo.TabIndex = 2
        Me.lblPlikInfo.Text = "Plik: ..."
        '
        'picQr
        '
        Me.picQr.BackColor = System.Drawing.Color.White
        Me.picQr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picQr.Location = New System.Drawing.Point(100, 95)
        Me.picQr.Name = "picQr"
        Me.picQr.Size = New System.Drawing.Size(260, 260)
        Me.picQr.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.picQr.TabIndex = 3
        Me.picQr.TabStop = False
        '
        'lblSiec
        '
        Me.lblSiec.AutoSize = True
        Me.lblSiec.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.lblSiec.Location = New System.Drawing.Point(13, 365)
        Me.lblSiec.Name = "lblSiec"
        Me.lblSiec.Size = New System.Drawing.Size(146, 15)
        Me.lblSiec.TabIndex = 4
        Me.lblSiec.Text = "Adres sieciowy komputera:"
        '
        'cmbInterfejsy
        '
        Me.cmbInterfejsy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbInterfejsy.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.cmbInterfejsy.FormattingEnabled = True
        Me.cmbInterfejsy.Location = New System.Drawing.Point(165, 362)
        Me.cmbInterfejsy.Name = "cmbInterfejsy"
        Me.cmbInterfejsy.Size = New System.Drawing.Size(282, 23)
        Me.cmbInterfejsy.TabIndex = 5
        Me.ToolTip1.SetToolTip(Me.cmbInterfejsy, "Wybierz adres IP karty Wi-Fi lub karty sieciowej, do której podłączony jest telefon.")
        '
        'txtAdres
        '
        Me.txtAdres.BackColor = System.Drawing.Color.White
        Me.txtAdres.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.txtAdres.Location = New System.Drawing.Point(16, 393)
        Me.txtAdres.Name = "txtAdres"
        Me.txtAdres.ReadOnly = True
        Me.txtAdres.Size = New System.Drawing.Size(344, 23)
        Me.txtAdres.TabIndex = 6
        '
        'btnKopiuj
        '
        Me.btnKopiuj.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.btnKopiuj.Location = New System.Drawing.Point(366, 392)
        Me.btnKopiuj.Name = "btnKopiuj"
        Me.btnKopiuj.Size = New System.Drawing.Size(81, 25)
        Me.btnKopiuj.TabIndex = 7
        Me.btnKopiuj.Text = "Kopiuj link"
        Me.ToolTip1.SetToolTip(Me.btnKopiuj, "Kopiuje bezpośredni link do pobrania mapy do schowka.")
        Me.btnKopiuj.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.lblStatus.ForeColor = System.Drawing.Color.DarkSlateGray
        Me.lblStatus.Location = New System.Drawing.Point(13, 424)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(434, 56)
        Me.lblStatus.TabIndex = 8
        Me.lblStatus.Text = "Gotowy do pobrania (oczekiwanie na telefon...)"
        '
        'btnZamknij
        '
        Me.btnZamknij.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnZamknij.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.btnZamknij.Location = New System.Drawing.Point(357, 485)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(90, 28)
        Me.btnZamknij.TabIndex = 9
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'FormWyslijNaTelefon
        '
        Me.AcceptButton = Me.btnZamknij
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnZamknij
        Me.ClientSize = New System.Drawing.Size(460, 525)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnKopiuj)
        Me.Controls.Add(Me.txtAdres)
        Me.Controls.Add(Me.cmbInterfejsy)
        Me.Controls.Add(Me.lblSiec)
        Me.Controls.Add(Me.picQr)
        Me.Controls.Add(Me.lblPlikInfo)
        Me.Controls.Add(Me.lblInstrukcja)
        Me.Controls.Add(Me.lblTytul)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormWyslijNaTelefon"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Prześlij mapę na telefon (Wi-Fi)"
        CType(Me.picQr, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblTytul As System.Windows.Forms.Label
    Friend WithEvents lblInstrukcja As System.Windows.Forms.Label
    Friend WithEvents lblPlikInfo As System.Windows.Forms.Label
    Friend WithEvents picQr As System.Windows.Forms.PictureBox
    Friend WithEvents lblSiec As System.Windows.Forms.Label
    Friend WithEvents cmbInterfejsy As System.Windows.Forms.ComboBox
    Friend WithEvents txtAdres As System.Windows.Forms.TextBox
    Friend WithEvents btnKopiuj As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnZamknij As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip

End Class
