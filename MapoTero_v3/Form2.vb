Imports System.IO

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
        CheckBox10.Checked = Module1.kursorWGS84
        CheckBox11.Checked = Module1.zaznaczenieWGS84
        CheckBox12.Checked = Module1.kursor_i_srodekmapy
        CheckBox13.Checked = Module1.CheckTab

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


        If Module1.editXY = True Then
            Form1.TextBox1.Enabled = True
            Form1.TextBox2.Enabled = True
            Form1.TextBox3.Enabled = True
            Form1.TextBox4.Enabled = True
            Form1.Label3.Enabled = True
            Form1.Label4.Enabled = True
            Form1.Label5.Enabled = True
            Form1.Label6.Enabled = True


            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
            Form1.RichTextBox1.Text = "Wyłączono zaznaczanie na mapie zasięgu pobierania prawym przyciskiem myszy. Określ samodzielnie współrzędne XY zasięgu mapy i wpisz je w odpowiednie pola"

            'Form1.GMapControl1.Enabled = False
        Else
            Form1.TextBox1.Enabled = False
            Form1.TextBox2.Enabled = False
            Form1.TextBox3.Enabled = False
            Form1.TextBox4.Enabled = False
            Form1.Label3.Enabled = False
            Form1.Label4.Enabled = False
            Form1.Label5.Enabled = False
            Form1.Label6.Enabled = False
            'Form1.GMapControl1.Enabled = True
            Form1.RichTextBox1.Text = ""

        End If

    End Sub

    Private Sub CheckBox10_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox10.CheckedChanged
        Module1.kursorWGS84 = CheckBox10.CheckState



    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged
        Module1.zaznaczenieWGS84 = CheckBox11.CheckState


        If Module1.zaznaczenieWGS84 = True Then
            Form1.Label31.Visible = True
            Form1.Label32.Visible = True
            Form1.Label33.Visible = True
            Form1.Label34.Visible = True
        Else
            Form1.Label31.Visible = False
            Form1.Label32.Visible = False
            Form1.Label33.Visible = False
            Form1.Label34.Visible = False
        End If


    End Sub

    Private Sub CheckBox12_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox12.CheckedChanged
        Module1.kursor_i_srodekmapy = CheckBox12.CheckState

        If Module1.kursor_i_srodekmapy = False Then
            Form1.Label39.Visible = False
            Form1.Label40.Visible = False
            Form1.Label41.Visible = False
            Form1.Label35.Visible = False
            Form1.Label37.Visible = False
            Form1.Label38.Visible = False
            Form1.Label63.Visible = False
            Form1.Label66.Visible = False
        Else
            Form1.Label39.Visible = True
            Form1.Label40.Visible = True
            Form1.Label41.Visible = True
            Form1.Label35.Visible = True
            Form1.Label37.Visible = True
            Form1.Label38.Visible = True
            Form1.Label63.Visible = True
            Form1.Label66.Visible = True
        End If


    End Sub


    Private Sub CheckBox13_Click(sender As Object, e As EventArgs) Handles CheckBox13.Click
        Select Case CheckBox13.CheckState
            Case CheckState.Checked
                Module1.CheckTab = True
            Case CheckState.Unchecked
                Module1.CheckTab = False
        End Select
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click


        'File.Delete(folderSegmentow & "conf.txt")
        File.Delete(myPath & "\lastsettings.txt")
        Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
        Form1.RichTextBox1.Text = "Usunięto plik conf.txt oraz lastsetting.txt"
        
        Form1.Label35.Text = "52.3"
        Form1.Label63.Text = "19.2"
        Form1.Label65.Text = "6"

        'odświeżenie widoku okna mapy po wprowadzeniu nowych ustawień conf.txt
        Module1.x_start = 52.3
        Module1.y_start = 19.2
        Module1.zoom_start = 6
        

        Form1.GMapControl1.Refresh()
        Form1.GMapControl1.ReloadMap()
        Form1.GMapControl1.Zoom = 6
        Form1.GMapControl1.Position = New GMap.NET.PointLatLng(52.3, 19.2)
     
        Form1.Refresh()
        'domyślne ustawienia
        TextBox6.Text = "" 'wspolna nazwa kwadratu
        ComboBox1.Text = "jpeg"  'format
        Module1.format = "jpeg"
        CheckBox5.Checked = False 'powtorz pobieranie od ostatniego segm
        Module1.pobierajPowyzejOstatniego = False
        CheckBox9.Checked = False 'edytuj pola XY zaznaczenia
        Module1.editXY = False
        CheckBox10.Checked = False 'wyswietl wspolrz kursora WGS
        CheckBox11.Checked = False 'wyswietl wspolrz zaznaczenia WGS
        CheckBox12.Checked = True 'wyswietl wspolrzedne kursora mapy
        CheckBox1.Checked = False 'map
        Module1.CheckMap = False
        CheckBox8.Checked = False 'kml
        Module1.CheckKml = False
        CheckBox7.Checked = False 'jpgw
        Module1.CheckJpgw = False
        CheckBox13.Checked = False 'tab
        Module1.CheckTab = False
        CheckBox4.Checked = False 'wld
        Module1.CheckWldPoints = False
        CheckBox2.Checked = False 'gmi
        Module1.CheckGmi = False
        ComboBox2.Text = "NrWiersza_NrKolumny" 'numeracja
        Module1.numeracja = "NrWiersza_NrKolumny"
        TextBox1.Text = "3" 'ilosc prob
        Module1.iloscProbPobrania = 3
        TextBox2.Text = "5" 'sekund
        Module1.przerwaMiedzyProbami = 5
        CheckBox6.Checked = False 'tb
        Module1.CheckTB = False
        CheckBox3.Checked = False 'zamiana XY
        Module1.XYswitched = False
        'Module1.plik_lastsettings()
        'Form1.wczytaj_lastsettings()
        If x_start = 52.3 Then
            If File.Exists(folderSegmentow & "conf.txt") = True Then
                
               
                'tworzy plik z danymi sesji
                FileOpen(1, folderSegmentow & "\conf.txt", OpenMode.Output)
                WriteLine(1, folderSegmentow)
                WriteLine(1, Form1.ComboBox3.Text)
                WriteLine(1, Form1.TextBox1.Text)
                WriteLine(1, Form1.TextBox2.Text)
                WriteLine(1, Form1.TextBox3.Text)
                WriteLine(1, Form1.TextBox4.Text)
                WriteLine(1, Form1.TextBox9.Text)
                WriteLine(1, Form1.TextBox10.Text)
                WriteLine(1, warstwy(0))
                WriteLine(1, warstwy(1))
                WriteLine(1, warstwy(2))
                WriteLine(1, warstwy(3))
                WriteLine(1, warstwy(4))
                WriteLine(1, warstwy(5))
                WriteLine(1, warstwy(6))
                WriteLine(1, warstwy(7))
                WriteLine(1, warstwy(8))
                WriteLine(1, warstwy(9))
                WriteLine(1, warstwy(10))
                WriteLine(1, warstwy(11))
                WriteLine(1, nrWarstwy)
                WriteLine(1, format)
                WriteLine(1, wspolnaNazwaKwadratu)
                WriteLine(1, pobierajPowyzejOstatniego)
                WriteLine(1, "52.3")
                WriteLine(1, "19.2")
                WriteLine(1, "6")
                WriteLine(1, Module1.numeracja)
                FileClose(1)
            End If



           

            FileClose(1) 'w razie gyby był otwarty

            FileOpen(1, myPath & "\lastsettings.txt", OpenMode.Output)

            PrintLine(1, "folder segmentow")        'zapisuje ostatni folder segmentów, z niego wczytany zostanie plik conf
            If folderSegmentow = "" Then
                PrintLine(1, myPath & "\download\")
            Else
                PrintLine(1, folderSegmentow)
            End If

            PrintLine(1, "chkgmi")
            PrintLine(1, CheckGmi)          'zapisuje czy tworzyć gmi
            PrintLine(1, "chkmap")
            PrintLine(1, CheckMap)          'zapisuje czy tworzyć map
            PrintLine(1, "chkwldpoints")
            PrintLine(1, CheckWldPoints)    'zapisuje czy tworzyć wld i points
            PrintLine(1, "chkjpgw")
            PrintLine(1, CheckJpgw)          'zapisuje czy tworzyć jpgw
            PrintLine(1, "chkkml")
            PrintLine(1, CheckKml)          'zapisuje czy tworzyć kml
            PrintLine(1, "chktab")
            PrintLine(1, CheckTab)          'zapisuje czy tworzyć kml
            PrintLine(1, "dolna")           'zapisuje foldery łączonych warstw
            If folderWarstwa1 = "" Then
                PrintLine(1, myPath & "\download\dolna\")
            Else
                PrintLine(1, folderWarstwa1)
            End If

            PrintLine(1, "gorna")
            If folderWarstwa2 = "" Then
                PrintLine(1, myPath & "\download\gorna\")
            Else
                PrintLine(1, folderWarstwa2)
            End If

            PrintLine(1, "polaczone")
            If folderWynikowy = "" Then
                PrintLine(1, myPath & "\download\polaczone\")
            Else
                PrintLine(1, folderWynikowy)
            End If

            PrintLine(1, "XYswitched")
            PrintLine(1, XYswitched)

            PrintLine(1, "numeracja_")
            PrintLine(1, numeracja)
            PrintLine(1, "iloscProbPobrania")
            PrintLine(1, iloscProbPobrania)
            PrintLine(1, "przerwaMiedzyProbami")
            PrintLine(1, przerwaMiedzyProbami)

            PrintLine(1, "x_start")
            PrintLine(1, "52.3")
            PrintLine(1, "y_start")
            PrintLine(1, "19.2")
            PrintLine(1, "zoom_start")
            PrintLine(1, "6")
            FileClose(1)



        End If
    End Sub

    
End Class