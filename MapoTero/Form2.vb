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





    '// API DLA USTAWIANIA POZYCJI OKNA
    Public Declare Function SetWindowPos Lib "user32.dll" ( _
       ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Int32, _
       ByVal Y As Int32, ByVal cx As Int32, ByVal cy As Int32, ByVal uFlags As Int32) As Boolean

    Public Const HWND_BOTTOM As Long = 1     '// POD WSZYSTKIMI OKNAMI
    Public Const HWND_NOTOPMOST As Long = -2 '// POD PIERWSZYM OKNEM
    Public Const HWND_TOP As Long = 0        '// NAD PIERWSZYM OKNEM
    Public Const HWND_TOPMOST As Long = -1   '// NAD WSZYSTKIMI OKNAMI
    Public Const SWP_NOSIZE As Long = 1
    Public Const SWP_NOMOVE As Long = 2
    Public Const SWP_NOZORDER As Long = 4
    Public Const SWP_NOREDRAW As Long = 8
    Public Const SWP_NOACTIVATE As Long = 16
    Public Const SWP_FRAMECHANGED As Long = 32
    Public Const SWP_SHOWWINDOW As Long = 64
    Public Const SWP_HIDEWINDOW As Long = 128
    Public Const SWP_NOCOPYBITS As Long = 256
    Public Const SWP_NOOWNERZORDER As Long = 512
    Public Const SWP_NOSENDCHANGING As Long = 1024


    Sub SetFormOrder(ByVal MojaForma As Form, Pozycja As Long)
        SetWindowPos( _
             MojaForma.Handle, New IntPtr(Pozycja), MojaForma.Left, MojaForma.Top, _
             MojaForma.Width, MojaForma.Height, SWP_SHOWWINDOW Or SWP_FRAMECHANGED Or SWP_NOACTIVATE)
    End Sub





    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '// ustawiamy prawdziwy TOP MOST
        Call SetFormOrder(Me, HWND_TOP)

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
        CheckBox8.Checked = Module1.CheckNrSeg
        ComboBox1.Text = Module1.format
        TextBox3.Text = Module1.folderWarstwa1
        TextBox4.Text = Module1.folderWarstwa2
        TextBox5.Text = Module1.folderWynikowy
        TextBox6.Text = Module1.wspolnaNazwaKwadratu

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
    Private Sub CheckBox8_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox8.Click
        Select Case CheckBox8.CheckState
            Case CheckState.Checked
                Module1.CheckNrSeg = True
            Case CheckState.Unchecked
                Module1.CheckNrSeg = False
        End Select
    End Sub

    Private Sub CheckBox6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox6.CheckedChanged
        Select Case CheckBox6.CheckState
            Case CheckState.Checked
                CheckBox1.Checked = False
                Module1.CheckMap = False
                CheckBox2.Checked = False
                Module1.CheckGmi = False
                CheckBox3.Checked = False
                Module1.XYswitched = False
                CheckBox5.Checked = False
                CheckBox5.Enabled = False
                Module1.pobierajPowyzejOstatniego = False
                Module1.CheckWldPoints = False
                GroupBox1.Enabled = False
                Module1.CheckJpgw = False
                CheckBox8.Checked = False
                CheckBox8.Enabled = False
                Module1.CheckNrSeg = False
                CheckBox8.Checked = False
                Module1.CheckTB = True
                Form1.TextBox9.Text = TBbok



            Case CheckState.Unchecked
                GroupBox1.Enabled = True

                Module1.CheckTB = False
                Form1.TextBox9.Text = "2000"
                CheckBox5.Enabled = True
                CheckBox8.Enabled = True


        End Select
    End Sub
    Private Sub ButtonDolna_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDolna.Click
        If Form1.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Module1.folderWarstwa1 = Form1.FolderBrowserDialog1.SelectedPath & "\"
        End If


        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = Module1.folderWarstwa1
        TextBox3.Text = Module1.folderWarstwa1

    End Sub

    Private Sub ButtonGorna_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGorna.Click
        If Form1.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Module1.folderWarstwa2 = Form1.FolderBrowserDialog1.SelectedPath & "\"
        End If


        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = Module1.folderWarstwa2
        TextBox4.Text = Module1.folderWarstwa2
    End Sub

    Private Sub ButtonPolaczone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPolaczone.Click
        If Form1.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Module1.folderWynikowy = Form1.FolderBrowserDialog1.SelectedPath & "\"
        End If



        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = Module1.folderWynikowy
        TextBox5.Text = Module1.folderWynikowy
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        wspolnaNazwaKwadratu = TextBox6.Text
        iloscProbPobrania = Val(TextBox1.Text)
        przerwaMiedzyProbami = Val(TextBox2.Text)
        Module1.plik_lastsettings()

        Me.Close()
    End Sub


End Class