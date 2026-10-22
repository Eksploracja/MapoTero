'Copyright (C) <2015>  pajakt
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
Partial Class FormPomocMbtiles
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
        Me.rtbTresc = New System.Windows.Forms.RichTextBox()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.pnlDol = New System.Windows.Forms.Panel()
        Me.pnlDol.SuspendLayout()
        Me.SuspendLayout()
        '
        'rtbTresc
        '
        Me.rtbTresc.BackColor = System.Drawing.Color.White
        Me.rtbTresc.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.rtbTresc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rtbTresc.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.rtbTresc.Location = New System.Drawing.Point(12, 12)
        Me.rtbTresc.Name = "rtbTresc"
        Me.rtbTresc.ReadOnly = True
        Me.rtbTresc.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        Me.rtbTresc.Size = New System.Drawing.Size(716, 506)
        Me.rtbTresc.TabIndex = 0
        Me.rtbTresc.Text = ""
        '
        'btnZamknij
        '
        Me.btnZamknij.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZamknij.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnZamknij.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.btnZamknij.Location = New System.Drawing.Point(626, 8)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(90, 28)
        Me.btnZamknij.TabIndex = 1
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'pnlDol
        '
        Me.pnlDol.Controls.Add(Me.btnZamknij)
        Me.pnlDol.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlDol.Location = New System.Drawing.Point(12, 518)
        Me.pnlDol.Name = "pnlDol"
        Me.pnlDol.Size = New System.Drawing.Size(716, 42)
        Me.pnlDol.TabIndex = 2
        '
        'FormPomocMbtiles
        '
        Me.AcceptButton = Me.btnZamknij
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnZamknij
        Me.ClientSize = New System.Drawing.Size(740, 572)
        Me.Controls.Add(Me.rtbTresc)
        Me.Controls.Add(Me.pnlDol)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(600, 450)
        Me.Name = "FormPomocMbtiles"
        Me.Padding = New System.Windows.Forms.Padding(12)
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Pomoc i instrukcja - Kreator wielowarstwowego MBTiles"
        Me.pnlDol.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents rtbTresc As RichTextBox
    Friend WithEvents btnZamknij As Button
    Friend WithEvents pnlDol As Panel
End Class
