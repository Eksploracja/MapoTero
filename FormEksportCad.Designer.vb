<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormEksportCad
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
        Me.lblFolder = New System.Windows.Forms.Label()
        Me.txtFolder = New System.Windows.Forms.TextBox()
        Me.btnFolder = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.grpRastry = New System.Windows.Forms.GroupBox()
        Me.rbSegmenty = New System.Windows.Forms.RadioButton()
        Me.rbArkusz = New System.Windows.Forms.RadioButton()
        Me.txtArkusz = New System.Windows.Forms.TextBox()
        Me.btnArkusz = New System.Windows.Forms.Button()
        Me.rbBezRastrow = New System.Windows.Forms.RadioButton()
        Me.grpWektory = New System.Windows.Forms.GroupBox()
        Me.chkObrys = New System.Windows.Forms.CheckBox()
        Me.chkSiatka = New System.Windows.Forms.CheckBox()
        Me.chkNumery = New System.Windows.Forms.CheckBox()
        Me.chkTrasa = New System.Windows.Forms.CheckBox()
        Me.chkOpis = New System.Windows.Forms.CheckBox()
        Me.grpPliki = New System.Windows.Forms.GroupBox()
        Me.chkDxf = New System.Windows.Forms.CheckBox()
        Me.chkSciezkiWzgledne = New System.Windows.Forms.CheckBox()
        Me.chkScr = New System.Windows.Forms.CheckBox()
        Me.lblPlik = New System.Windows.Forms.Label()
        Me.txtPlik = New System.Windows.Forms.TextBox()
        Me.btnPlik = New System.Windows.Forms.Button()
        Me.txtKomunikaty = New System.Windows.Forms.TextBox()
        Me.btnEksportuj = New System.Windows.Forms.Button()
        Me.btnZamknij = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.dlgArkusz = New System.Windows.Forms.OpenFileDialog()
        Me.dlgPlik = New System.Windows.Forms.SaveFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpRastry.SuspendLayout()
        Me.grpWektory.SuspendLayout()
        Me.grpPliki.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblFolder
        '
        Me.lblFolder.AutoSize = True
        Me.lblFolder.Location = New System.Drawing.Point(12, 12)
        Me.lblFolder.Name = "lblFolder"
        Me.lblFolder.Size = New System.Drawing.Size(190, 13)
        Me.lblFolder.TabIndex = 0
        Me.lblFolder.Text = "Folder sesji (segmenty i plik conf.txt):"
        '
        'txtFolder
        '
        Me.txtFolder.Location = New System.Drawing.Point(12, 30)
        Me.txtFolder.Name = "txtFolder"
        Me.txtFolder.ReadOnly = True
        Me.txtFolder.Size = New System.Drawing.Size(400, 20)
        Me.txtFolder.TabIndex = 1
        '
        'btnFolder
        '
        Me.btnFolder.Location = New System.Drawing.Point(418, 28)
        Me.btnFolder.Name = "btnFolder"
        Me.btnFolder.Size = New System.Drawing.Size(90, 23)
        Me.btnFolder.TabIndex = 2
        Me.btnFolder.Text = "zmień..."
        Me.btnFolder.UseVisualStyleBackColor = True
        '
        'lblInfo
        '
        Me.lblInfo.Location = New System.Drawing.Point(12, 58)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(496, 42)
        Me.lblInfo.TabIndex = 3
        '
        'grpRastry
        '
        Me.grpRastry.Controls.Add(Me.rbSegmenty)
        Me.grpRastry.Controls.Add(Me.rbArkusz)
        Me.grpRastry.Controls.Add(Me.txtArkusz)
        Me.grpRastry.Controls.Add(Me.btnArkusz)
        Me.grpRastry.Controls.Add(Me.rbBezRastrow)
        Me.grpRastry.Location = New System.Drawing.Point(12, 104)
        Me.grpRastry.Name = "grpRastry"
        Me.grpRastry.Size = New System.Drawing.Size(496, 96)
        Me.grpRastry.TabIndex = 4
        Me.grpRastry.TabStop = False
        Me.grpRastry.Text = "Rastry wstawiane do rysunku"
        '
        'rbSegmenty
        '
        Me.rbSegmenty.AutoSize = True
        Me.rbSegmenty.Checked = True
        Me.rbSegmenty.Location = New System.Drawing.Point(10, 20)
        Me.rbSegmenty.Name = "rbSegmenty"
        Me.rbSegmenty.Size = New System.Drawing.Size(300, 17)
        Me.rbSegmenty.TabIndex = 0
        Me.rbSegmenty.TabStop = True
        Me.rbSegmenty.Text = "pobrane segmenty (osobny obraz dla każdego segmentu)"
        Me.ToolTip1.SetToolTip(Me.rbSegmenty, "Każdy plik segmentu wstawiany jest jako osobny obraz na swoim miejscu (przy setkach segmentów lepiej wstawić scalony arkusz).")
        Me.rbSegmenty.UseVisualStyleBackColor = True
        '
        'rbArkusz
        '
        Me.rbArkusz.AutoSize = True
        Me.rbArkusz.Location = New System.Drawing.Point(10, 44)
        Me.rbArkusz.Name = "rbArkusz"
        Me.rbArkusz.Size = New System.Drawing.Size(100, 17)
        Me.rbArkusz.TabIndex = 1
        Me.rbArkusz.Text = "scalony arkusz:"
        Me.ToolTip1.SetToolTip(Me.rbArkusz, "Arkusz utworzony poleceniem Łączenie segmentów (GeoTIFF albo JPEG / PNG z plikiem world file).")
        Me.rbArkusz.UseVisualStyleBackColor = True
        '
        'txtArkusz
        '
        Me.txtArkusz.Location = New System.Drawing.Point(116, 43)
        Me.txtArkusz.Name = "txtArkusz"
        Me.txtArkusz.ReadOnly = True
        Me.txtArkusz.Size = New System.Drawing.Size(274, 20)
        Me.txtArkusz.TabIndex = 2
        '
        'btnArkusz
        '
        Me.btnArkusz.Location = New System.Drawing.Point(396, 41)
        Me.btnArkusz.Name = "btnArkusz"
        Me.btnArkusz.Size = New System.Drawing.Size(90, 23)
        Me.btnArkusz.TabIndex = 3
        Me.btnArkusz.Text = "wybierz..."
        Me.btnArkusz.UseVisualStyleBackColor = True
        '
        'rbBezRastrow
        '
        Me.rbBezRastrow.AutoSize = True
        Me.rbBezRastrow.Location = New System.Drawing.Point(10, 70)
        Me.rbBezRastrow.Name = "rbBezRastrow"
        Me.rbBezRastrow.Size = New System.Drawing.Size(330, 17)
        Me.rbBezRastrow.TabIndex = 4
        Me.rbBezRastrow.Text = "bez rastrów - tylko obrys i siatka segmentów (np. przed pobraniem)"
        Me.rbBezRastrow.UseVisualStyleBackColor = True
        '
        'grpWektory
        '
        Me.grpWektory.Controls.Add(Me.chkObrys)
        Me.grpWektory.Controls.Add(Me.chkSiatka)
        Me.grpWektory.Controls.Add(Me.chkNumery)
        Me.grpWektory.Controls.Add(Me.chkTrasa)
        Me.grpWektory.Controls.Add(Me.chkOpis)
        Me.grpWektory.Location = New System.Drawing.Point(12, 206)
        Me.grpWektory.Name = "grpWektory"
        Me.grpWektory.Size = New System.Drawing.Size(496, 70)
        Me.grpWektory.TabIndex = 5
        Me.grpWektory.TabStop = False
        Me.grpWektory.Text = "Elementy wektorowe (osobne warstwy MAPOTERO_...)"
        '
        'chkObrys
        '
        Me.chkObrys.AutoSize = True
        Me.chkObrys.Checked = True
        Me.chkObrys.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkObrys.Location = New System.Drawing.Point(10, 20)
        Me.chkObrys.Name = "chkObrys"
        Me.chkObrys.Size = New System.Drawing.Size(92, 17)
        Me.chkObrys.TabIndex = 0
        Me.chkObrys.Text = "obrys zasięgu"
        Me.chkObrys.UseVisualStyleBackColor = True
        '
        'chkSiatka
        '
        Me.chkSiatka.AutoSize = True
        Me.chkSiatka.Checked = True
        Me.chkSiatka.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSiatka.Location = New System.Drawing.Point(130, 20)
        Me.chkSiatka.Name = "chkSiatka"
        Me.chkSiatka.Size = New System.Drawing.Size(108, 17)
        Me.chkSiatka.TabIndex = 1
        Me.chkSiatka.Text = "siatka segmentów"
        Me.chkSiatka.UseVisualStyleBackColor = True
        '
        'chkNumery
        '
        Me.chkNumery.AutoSize = True
        Me.chkNumery.Checked = True
        Me.chkNumery.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkNumery.Location = New System.Drawing.Point(270, 20)
        Me.chkNumery.Name = "chkNumery"
        Me.chkNumery.Size = New System.Drawing.Size(170, 17)
        Me.chkNumery.TabIndex = 2
        Me.chkNumery.Text = "numery segmentów (tylko DXF)"
        Me.chkNumery.UseVisualStyleBackColor = True
        '
        'chkTrasa
        '
        Me.chkTrasa.AutoSize = True
        Me.chkTrasa.Checked = True
        Me.chkTrasa.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTrasa.Location = New System.Drawing.Point(10, 44)
        Me.chkTrasa.Name = "chkTrasa"
        Me.chkTrasa.Size = New System.Drawing.Size(103, 17)
        Me.chkTrasa.TabIndex = 3
        Me.chkTrasa.Text = "trasa (korytarz)"
        Me.ToolTip1.SetToolTip(Me.chkTrasa, "Linia trasy GPX / KML sesji pobranej wzdłuż trasy.")
        Me.chkTrasa.UseVisualStyleBackColor = True
        '
        'chkOpis
        '
        Me.chkOpis.AutoSize = True
        Me.chkOpis.Checked = True
        Me.chkOpis.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkOpis.Location = New System.Drawing.Point(130, 44)
        Me.chkOpis.Name = "chkOpis"
        Me.chkOpis.Size = New System.Drawing.Size(270, 17)
        Me.chkOpis.TabIndex = 4
        Me.chkOpis.Text = "opis mapy: źródło, data pobrania, układ (tylko DXF)"
        Me.chkOpis.UseVisualStyleBackColor = True
        '
        'grpPliki
        '
        Me.grpPliki.Controls.Add(Me.chkDxf)
        Me.grpPliki.Controls.Add(Me.chkSciezkiWzgledne)
        Me.grpPliki.Controls.Add(Me.chkScr)
        Me.grpPliki.Location = New System.Drawing.Point(12, 282)
        Me.grpPliki.Name = "grpPliki"
        Me.grpPliki.Size = New System.Drawing.Size(496, 92)
        Me.grpPliki.TabIndex = 6
        Me.grpPliki.TabStop = False
        Me.grpPliki.Text = "Pliki wynikowe"
        '
        'chkDxf
        '
        Me.chkDxf.AutoSize = True
        Me.chkDxf.Checked = True
        Me.chkDxf.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkDxf.Location = New System.Drawing.Point(10, 20)
        Me.chkDxf.Name = "chkDxf"
        Me.chkDxf.Size = New System.Drawing.Size(330, 17)
        Me.chkDxf.TabIndex = 0
        Me.chkDxf.Text = "rysunek DXF (otwórz w programie CAD)"
        Me.ToolTip1.SetToolTip(Me.chkDxf, "DXF R2000 - AutoCAD 2000 i nowsze, AutoCAD LT, BricsCAD, ZWCAD, GstarCAD, progeCAD i inne.")
        Me.chkDxf.UseVisualStyleBackColor = True
        '
        'chkSciezkiWzgledne
        '
        Me.chkSciezkiWzgledne.AutoSize = True
        Me.chkSciezkiWzgledne.Checked = True
        Me.chkSciezkiWzgledne.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSciezkiWzgledne.Location = New System.Drawing.Point(28, 42)
        Me.chkSciezkiWzgledne.Name = "chkSciezkiWzgledne"
        Me.chkSciezkiWzgledne.Size = New System.Drawing.Size(390, 17)
        Me.chkSciezkiWzgledne.TabIndex = 1
        Me.chkSciezkiWzgledne.Text = "ścieżki rastrów względem pliku DXF (folder z mapą można przenieść)"
        Me.ToolTip1.SetToolTip(Me.chkSciezkiWzgledne, "Rastry leżące w folderze pliku DXF zapisywane są jako .\nazwa_pliku - rysunek działa po skopiowaniu całego folderu.")
        Me.chkSciezkiWzgledne.UseVisualStyleBackColor = True
        '
        'chkScr
        '
        Me.chkScr.AutoSize = True
        Me.chkScr.Checked = True
        Me.chkScr.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkScr.Location = New System.Drawing.Point(10, 66)
        Me.chkScr.Name = "chkScr"
        Me.chkScr.Size = New System.Drawing.Size(420, 17)
        Me.chkScr.TabIndex = 2
        Me.chkScr.Text = "skrypt SCR (wstawia mapę do otwartego rysunku poleceniem SCRIPT)"
        Me.ToolTip1.SetToolTip(Me.chkScr, "AutoCAD, AutoCAD LT, BricsCAD, ZWCAD - rastry, obrys, siatka i trasa na właściwych współrzędnych w bieżącym rysunku.")
        Me.chkScr.UseVisualStyleBackColor = True
        '
        'lblPlik
        '
        Me.lblPlik.AutoSize = True
        Me.lblPlik.Location = New System.Drawing.Point(12, 382)
        Me.lblPlik.Name = "lblPlik"
        Me.lblPlik.Size = New System.Drawing.Size(260, 13)
        Me.lblPlik.TabIndex = 7
        Me.lblPlik.Text = "Plik rysunku (skrypt .scr otrzyma tę samą nazwę):"
        '
        'txtPlik
        '
        Me.txtPlik.Location = New System.Drawing.Point(12, 400)
        Me.txtPlik.Name = "txtPlik"
        Me.txtPlik.ReadOnly = True
        Me.txtPlik.Size = New System.Drawing.Size(400, 20)
        Me.txtPlik.TabIndex = 8
        '
        'btnPlik
        '
        Me.btnPlik.Location = New System.Drawing.Point(418, 398)
        Me.btnPlik.Name = "btnPlik"
        Me.btnPlik.Size = New System.Drawing.Size(90, 23)
        Me.btnPlik.TabIndex = 9
        Me.btnPlik.Text = "zmień..."
        Me.btnPlik.UseVisualStyleBackColor = True
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.Location = New System.Drawing.Point(12, 430)
        Me.txtKomunikaty.Multiline = True
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.ReadOnly = True
        Me.txtKomunikaty.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtKomunikaty.Size = New System.Drawing.Size(496, 96)
        Me.txtKomunikaty.TabIndex = 10
        '
        'btnEksportuj
        '
        Me.btnEksportuj.Location = New System.Drawing.Point(330, 534)
        Me.btnEksportuj.Name = "btnEksportuj"
        Me.btnEksportuj.Size = New System.Drawing.Size(86, 26)
        Me.btnEksportuj.TabIndex = 11
        Me.btnEksportuj.Text = "Eksportuj"
        Me.btnEksportuj.UseVisualStyleBackColor = True
        '
        'btnZamknij
        '
        Me.btnZamknij.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnZamknij.Location = New System.Drawing.Point(422, 534)
        Me.btnZamknij.Name = "btnZamknij"
        Me.btnZamknij.Size = New System.Drawing.Size(86, 26)
        Me.btnZamknij.TabIndex = 12
        Me.btnZamknij.Text = "Zamknij"
        Me.btnZamknij.UseVisualStyleBackColor = True
        '
        'dlgFolder
        '
        Me.dlgFolder.Description = "Folder z pobranymi segmentami (plik conf.txt)"
        '
        'dlgArkusz
        '
        Me.dlgArkusz.Filter = "Arkusze map (*.tif;*.tiff;*.jpg;*.jpeg;*.png)|*.tif;*.tiff;*.jpg;*.jpeg;*.png|Wszystkie pliki (*.*)|*.*"
        Me.dlgArkusz.Title = "Scalony arkusz mapy"
        '
        'dlgPlik
        '
        Me.dlgPlik.DefaultExt = "dxf"
        Me.dlgPlik.Filter = "Rysunek DXF (*.dxf)|*.dxf"
        Me.dlgPlik.Title = "Plik rysunku DXF"
        '
        'FormEksportCad
        '
        Me.AcceptButton = Me.btnEksportuj
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnZamknij
        Me.ClientSize = New System.Drawing.Size(520, 572)
        Me.Controls.Add(Me.btnZamknij)
        Me.Controls.Add(Me.btnEksportuj)
        Me.Controls.Add(Me.txtKomunikaty)
        Me.Controls.Add(Me.btnPlik)
        Me.Controls.Add(Me.txtPlik)
        Me.Controls.Add(Me.lblPlik)
        Me.Controls.Add(Me.grpPliki)
        Me.Controls.Add(Me.grpWektory)
        Me.Controls.Add(Me.grpRastry)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.btnFolder)
        Me.Controls.Add(Me.txtFolder)
        Me.Controls.Add(Me.lblFolder)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormEksportCad"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Eksport do CAD (DXF, skrypt SCR)"
        Me.grpRastry.ResumeLayout(False)
        Me.grpRastry.PerformLayout()
        Me.grpWektory.ResumeLayout(False)
        Me.grpWektory.PerformLayout()
        Me.grpPliki.ResumeLayout(False)
        Me.grpPliki.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblFolder As System.Windows.Forms.Label
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnFolder As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents grpRastry As System.Windows.Forms.GroupBox
    Friend WithEvents rbSegmenty As System.Windows.Forms.RadioButton
    Friend WithEvents rbArkusz As System.Windows.Forms.RadioButton
    Friend WithEvents txtArkusz As System.Windows.Forms.TextBox
    Friend WithEvents btnArkusz As System.Windows.Forms.Button
    Friend WithEvents rbBezRastrow As System.Windows.Forms.RadioButton
    Friend WithEvents grpWektory As System.Windows.Forms.GroupBox
    Friend WithEvents chkObrys As System.Windows.Forms.CheckBox
    Friend WithEvents chkSiatka As System.Windows.Forms.CheckBox
    Friend WithEvents chkNumery As System.Windows.Forms.CheckBox
    Friend WithEvents chkTrasa As System.Windows.Forms.CheckBox
    Friend WithEvents chkOpis As System.Windows.Forms.CheckBox
    Friend WithEvents grpPliki As System.Windows.Forms.GroupBox
    Friend WithEvents chkDxf As System.Windows.Forms.CheckBox
    Friend WithEvents chkSciezkiWzgledne As System.Windows.Forms.CheckBox
    Friend WithEvents chkScr As System.Windows.Forms.CheckBox
    Friend WithEvents lblPlik As System.Windows.Forms.Label
    Friend WithEvents txtPlik As System.Windows.Forms.TextBox
    Friend WithEvents btnPlik As System.Windows.Forms.Button
    Friend WithEvents txtKomunikaty As System.Windows.Forms.TextBox
    Friend WithEvents btnEksportuj As System.Windows.Forms.Button
    Friend WithEvents btnZamknij As System.Windows.Forms.Button
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents dlgArkusz As System.Windows.Forms.OpenFileDialog
    Friend WithEvents dlgPlik As System.Windows.Forms.SaveFileDialog
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
