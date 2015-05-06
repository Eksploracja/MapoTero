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

Public Class Form2


    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
      
        Me.Location = New Point(200, 150)


        TextBox1.Text = Module1.iloscProbPobrania
        TextBox2.Text = Module1.przerwaMiedzyProbami
        CheckBox1.Checked = Module1.CheckMap 'wyprowadza stan checkboxów do zmiennych
        CheckBox2.Checked = Module1.CheckGmi
        CheckBox4.Checked = Module1.CheckWldPoints
        CheckBox3.Checked = Module1.XYswitched
        CheckBox5.Checked = Module1.pobierajPowyzejOstatniego
        CheckBox6.Checked = Module1.CheckTB
        CheckBox7.Checked = Module1.CheckJpgw
        CheckBox8.Checked = Module1.CheckKml
        CheckBox9.Checked = Module1.editXY
        'ustawienie odpowiedniej warosci Combobox1 odbywa sie przy jego inicjalizacji

        TextBox6.Text = Module1.wspolnaNazwaKwadratu
        ComboBox2.Text = Module1.numeracja

            ComboBox1.Text = Module1.format


    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        Module1.format = ComboBox1.Text
        Form1.ToolStripStatusLabel2.Text = Module1.format


    End Sub


    Private Sub CheckBox1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox1.Click
        Select Case CheckBox1.CheckState
            Case CheckState.Checked
                Module1.CheckMap = True
            Case CheckState.Unchecked
                Module1.CheckMap = False
        End Select
    End Sub

    Private Sub CheckBox2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox2.Click
        Select Case CheckBox2.CheckState
            Case CheckState.Checked
                Module1.CheckGmi = True
            Case CheckState.Unchecked
                Module1.CheckGmi = False
        End Select
    End Sub

    Private Sub CheckBox4_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox4.Click
        Select Case CheckBox4.CheckState
            Case CheckState.Checked
                Module1.CheckWldPoints = True
            Case CheckState.Unchecked
                Module1.CheckWldPoints = False
        End Select
    End Sub



    Private Sub CheckBox3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox3.Click
        Select Case CheckBox3.CheckState
            Case CheckState.Checked
                Module1.XYswitched = True
            Case CheckState.Unchecked
                Module1.XYswitched = False
        End Select
    End Sub
    Private Sub CheckBox5_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox5.Click
        Select Case CheckBox5.CheckState
            Case CheckState.Checked
                Module1.pobierajPowyzejOstatniego = True
            Case CheckState.Unchecked
                Module1.pobierajPowyzejOstatniego = False
        End Select
    End Sub
    Private Sub CheckBox7_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox7.Click
        Select Case CheckBox7.CheckState
            Case CheckState.Checked
                Module1.CheckJpgw = True
            Case CheckState.Unchecked
                Module1.CheckJpgw = False
        End Select
    End Sub

    Private Sub CheckBox6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox6.CheckedChanged
        Dim bok As Integer = Val(Form1.TextBox9.Text)
        Select Case CheckBox6.CheckState
            Case CheckState.Checked
                CheckBox1.Checked = False
                Module1.CheckMap = False
                CheckBox2.Checked = False
                Module1.CheckGmi = False
                CheckBox3.Checked = False
                Module1.XYswitched = False
                CheckBox9.Checked = False
                'Module1.editXY = False
                CheckBox5.Checked = False
                CheckBox5.Enabled = False
                Module1.pobierajPowyzejOstatniego = False
                Module1.CheckWldPoints = False
                GroupBox1.Enabled = False
                Module1.CheckJpgw = False
                
                'Module1.CheckNrSeg = False
                Module1.CheckTB = True
                Form1.TextBox9.Text = TBbok
                ComboBox2.Enabled = False


            Case CheckState.Unchecked
                GroupBox1.Enabled = True

                Module1.CheckTB = False
                Form1.TextBox9.Text = bok
                CheckBox5.Enabled = True

                ComboBox2.Enabled = True


        End Select
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        wspolnaNazwaKwadratu = TextBox6.Text
        iloscProbPobrania = Val(TextBox1.Text)
        przerwaMiedzyProbami = Val(TextBox2.Text)
        'editXY = CheckBox9.CheckState
        Module1.plik_lastsettings()
        Me.Close()
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged
        Module1.numeracja = ComboBox2.Text
    End Sub


    
    

    Private Sub CheckBox8_Click(sender As Object, e As EventArgs) Handles CheckBox8.Click

        Select Case CheckBox8.CheckState
            Case CheckState.Checked
                Module1.CheckKml = True
            Case CheckState.Unchecked
                Module1.CheckKml = False
        End Select



    End Sub

    


    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged
        Module1.editXY = CheckBox9.CheckState


        If editXY = True Then
            Form1.GroupBox1.Enabled = True
        Else
            Form1.GroupBox1.Enabled = False
        End If

    End Sub
End Class