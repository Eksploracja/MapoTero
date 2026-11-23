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

Public NotInheritable Class AboutBox1

    Private Sub AboutBox1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        'okno miało angielskie napisy szablonu Visual Studio („About”, „Version”)
        Me.Text = "O programie " & ApplicationTitle
        ' Initialize all of the text displayed on the About Box.
        ' TODO: Customize the application's assembly information in the "Application" pane of the project 
        '    properties dialog (under the "Project" menu).
        Me.LabelProductName.Text = My.Application.Info.ProductName
        Me.LabelVersion.Text = "Wersja " & Program.Wersja
        Me.LabelCopyright.Text = My.Application.Info.Copyright
        Me.LabelCompanyName.Text = My.Application.Info.CompanyName
        Me.TextBoxDescription.Text = My.Application.Info.Description
        DodajPrzyciskWsparcia()
    End Sub

    ''' <summary>
    ''' Przycisk na lewo od OK, w tym samym wierszu. Leży w komórce przycisku OK, bo lewą kolumnę tabeli zasłania logo.
    ''' Okno otwierane jest jako domyślna instancja (AboutBox1.ShowDialog()), więc Load zachodzi przy każdym
    ''' otwarciu - bez sprawdzenia przyciski by się mnożyły.
    ''' </summary>
    Private Sub DodajPrzyciskWsparcia()
        If TableLayoutPanel.Controls.ContainsKey("panelPrzyciskow") Then Return
        Dim btn As New Button() With {
            .Name = "btnWesprzyj",
            .Text = "Wesprzyj rozwój programu",
            .Margin = OKButton.Margin,
            .UseVisualStyleBackColor = True
        }
        'szerokość według napisu, wysokość jak OK - przy AutoSize przycisk był o piksel niższy i nierówny z OK
        btn.Size = New Drawing.Size(btn.PreferredSize.Width + 12, OKButton.Height)
        AddHandler btn.Click, Sub() OtworzStrone(AdresWsparcia, "Wesprzyj rozwój programu")
        'OK i nowy przycisk w jednym panelu od prawej - położenie nie zależy od skalowania czcionki ekranu
        Dim panel As New FlowLayoutPanel() With {
            .Name = "panelPrzyciskow",
            .FlowDirection = FlowDirection.RightToLeft,
            .AutoSize = True,
            .WrapContents = False,
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .Margin = New Padding(0)
        }
        TableLayoutPanel.Controls.Remove(OKButton)
        panel.Controls.Add(OKButton)
        panel.Controls.Add(btn)
        TableLayoutPanel.Controls.Add(panel, 1, 5)
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub

End Class
