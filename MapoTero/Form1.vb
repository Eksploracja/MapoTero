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

Imports System
Imports System.Threading
Imports System.Diagnostics
Imports System.Net
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

Public Class Form1

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


    Public form1loaded As Boolean = False   'wskazuje, że form1 została już załadowana

    Sub SetFormOrder(ByVal Form1 As Form, Pozycja As Long)      'hierarchia okna
        SetWindowPos( _
             Form1.Handle, New IntPtr(Pozycja), Form1.Left, Form1.Top, _
             Form1.Width, Form1.Height, SWP_SHOWWINDOW Or SWP_FRAMECHANGED Or SWP_NOACTIVATE)
    End Sub


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        '// ustawiamy prawdziwy TOP MOST
        Call SetFormOrder(Me, HWND_TOP)

        'wyświetla nazwę i wersję 
        Me.Text = My.Application.Info.Title & " " & My.Application.Info.Version.ToString

        'program działa do daty CzasMinal
        ' Dim teraz As Date
        ' Dim CzasMinal As New System.DateTime(2011, 6, 30)
        ' teraz = Now
        ' If teraz > CzasMinal Then
        ' MsgBox("Czas minął. Pobierz nową wersję programu.")
        ' End
        ' End If

        TBseg = 22
        TBbok = 512


        myPath = My.Application.Info.DirectoryPath.ToString()

        Module1.folderSegmentow = myPath & "\download\"

        Me.SetDesktopLocation(0, 0)


        'przypisuje wartość zmiennej publicznej nrWarstwy
        nrWarstwy = 0

        TextBox7.Text = (Val(TextBox6.Text) / Val(TextBox10.Text)) * 1000
        TextBox8.Text = (Val(TextBox5.Text) / Val(TextBox10.Text)) * 1000





        'rozszerzenie = "jpg"
        'format = "jpeg"

        wczytaj_lastsettings()

        wczytajConf()

        'tworzy combobox z listą dostępnych warstw
        wczytaj_warstwyTxt()
        wczytaj_warstwy_z_pliku()

        ToolStripStatusLabel2.Text = "." & format
        ToolStripStatusLabel3.Text = ""

        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))



        form1loaded = True

    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Module1.plik_lastsettings()

    End Sub

    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click


        'w zależności od formatu obrazka odpowiednie rozszerzenie pliku
        Select Case Module1.format
            Case "jpeg"
                Module1.rozszerzenie = "jpg"
            Case "tiff"
                Module1.rozszerzenie = "tiff"
            Case "png"
                Module1.rozszerzenie = "png"
            Case "png8"
                Module1.rozszerzenie = "png"
            Case "png24"
                Module1.rozszerzenie = "png"
            Case "png32"
                Module1.rozszerzenie = "png"
            Case "gif"
                Module1.rozszerzenie = "gif"
            Case "svg+xml"
                'Module1.rozszerzenie = "svg"
                '==> dopóki svg nie działa będzie tak:
                MsgBox("Ten format jeszcze nie działa :o(", , "Zmień format.")
                Form2.Show()
                GoTo errorhandler

        End Select

        pobierz = True

        Button3.Enabled = True


        Module1.utworzPlikConf() 'na wszelki wypadek - żeby nie okazało się, że zapisuje w folderze głównym programu


        If folderSegmentow = "" Then
            GoTo errorhandler
        End If

        'tworzy plik koordynaty, kolejne procedury dodadzą do niego dane
        FileOpen(1, folderSegmentow & "koordynaty.txt", OpenMode.Output)
        WriteLine(1, folderSegmentow)           'zapisuje ścieżkę dostępu do folderu
        WriteLine(1, Val(TextBox9.Text))        'długość boku segmentu
        FileClose(1)

        Module1.proceduraGlowna()

errorhandler:
    End Sub


    'dodawanie warstw
    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged

        Dim linia As String = ""    'przechowuje linię pliku

        If nrWarstwy > 11 Then       'jeśli zechcesz wyznaczyć więcej warstw niż miejsca w tablicy
            MsgBox("TEJ WARSTWY NIE MOŻNA JUŻ DODAĆ.", , )
            GoTo errororhandler
        End If

        'zapisuje w tablicy dane z zaznaczonego wiersza
        warstwy(nrWarstwy) = ListBox1.SelectedItem

        nrWarstwy = nrWarstwy + 1

        Label11.Text = "1) " & warstwy(0)
        Label12.Text = "2) " & warstwy(1)
        Label13.Text = "3) " & warstwy(2)
        Label14.Text = "4) " & warstwy(3)
        Label15.Text = "5) " & warstwy(4)
        Label16.Text = "6) " & warstwy(5)
        Label27.Text = "7) " & warstwy(6)
        Label24.Text = "8) " & warstwy(7)
        Label28.Text = "9) " & warstwy(8)
        Label26.Text = "10) " & warstwy(9)
        Label25.Text = "11) " & warstwy(10)
        Label23.Text = "12) " & warstwy(11)


        'wpisuje do TextBox10 wartość m/pix przypisaną do danej warstwy
        FileOpen(1, myPath & "\warstwy\" & ComboBox3.Text & ".txt", OpenMode.Input)

        Input(1, adresSerwera)

        'przetważa plik, aż znajdzie linię zgodną z zaznaczeniem listbox1
        Do Until linia = ListBox1.SelectedItem
            Input(1, linia)
        Loop
        'wtedy przechodzi linię niżej i pobiera ją jako ilość m/piksel
        Input(1, TextBox10.Text)

        FileClose(1)






errororhandler:
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged

        TextBox5.Text = (Val(TextBox4.Text) - Val(TextBox2.Text)) / 1000
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))

    End Sub
    Private Sub TextBox4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox4.TextChanged

        TextBox5.Text = (Val(TextBox4.Text) - Val(TextBox2.Text)) / 1000
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
    End Sub
    Private Sub TextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged

        TextBox6.Text = (Val(TextBox3.Text) - Val(TextBox1.Text)) / 1000
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
    End Sub
    Private Sub TextBox3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged

        TextBox6.Text = (Val(TextBox3.Text) - Val(TextBox1.Text)) / 1000
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
    End Sub
    Private Sub TextBox5_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox5.TextChanged

        TextBox8.Text = (Val(TextBox5.Text) / Val(TextBox10.Text)) * 1000

    End Sub
    Private Sub TextBox6_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox6.TextChanged

        TextBox7.Text = (Val(TextBox6.Text) / Val(TextBox10.Text)) * 1000

    End Sub


    Private Sub TextBox10_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox10.TextChanged

        TextBox7.Text = (Val(TextBox6.Text) / Val(TextBox10.Text)) * 1000
        TextBox8.Text = (Val(TextBox5.Text) / Val(TextBox10.Text)) * 1000
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))

    End Sub
    Private Sub TextBox9_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox9.TextChanged


        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text)) / Val(TextBox10.Text)) / Val(TextBox9.Text))

    End Sub


    Private Sub ZapiszToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZapiszToolStripMenuItem.Click
        Module1.utworzPlikConf()
    End Sub

    Private Sub WczytajToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WczytajToolStripMenuItem1.Click
        Me.wczytajConf()
    End Sub

    Private Sub OtwórzOknoUstawieńToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OtwórzOknoUstawieńToolStripMenuItem.Click
        Form2.Show()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        'zmiana rozmiarów mapy przyciskami
        If XYswitched = False Then

            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) + 1000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - 1000
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) + 1000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - 1000
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) - 1000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) + 1000
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) - 1000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) + 1000
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox2.Text = Val(TextBox2.Text) + 1000
                            TextBox4.Text = Val(TextBox4.Text) + 1000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - 1000
                            TextBox4.Text = Val(TextBox4.Text) - 1000
                        Case "N"
                            TextBox1.Text = Val(TextBox1.Text) + 1000
                            TextBox3.Text = Val(TextBox3.Text) + 1000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - 1000
                            TextBox3.Text = Val(TextBox3.Text) - 1000
                    End Select

            End Select

        ElseIf XYswitched = True Then

            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + 1000
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) - 1000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + 1000
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) - 1000
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) - 1000
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) + 1000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) - 1000
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) + 1000
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + 1000
                            TextBox1.Text = Val(TextBox1.Text) + 1000
                        Case "W"
                            TextBox3.Text = Val(TextBox3.Text) - 1000
                            TextBox1.Text = Val(TextBox1.Text) - 1000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + 1000
                            TextBox2.Text = Val(TextBox2.Text) + 1000
                        Case "S"
                            TextBox4.Text = Val(TextBox4.Text) - 1000
                            TextBox2.Text = Val(TextBox2.Text) - 1000
                    End Select

            End Select

        End If
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click

        'zmiana rozmiarów mapy przyciskami
        If XYswitched = False Then

            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) + 10000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - 10000
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) + 10000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - 10000
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) - 10000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) + 10000
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) - 10000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) + 10000
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox2.Text = Val(TextBox2.Text) + 10000
                            TextBox4.Text = Val(TextBox4.Text) + 10000
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - 10000
                            TextBox4.Text = Val(TextBox4.Text) - 10000
                        Case "N"
                            TextBox1.Text = Val(TextBox1.Text) + 10000
                            TextBox3.Text = Val(TextBox3.Text) + 10000
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - 10000
                            TextBox3.Text = Val(TextBox3.Text) - 10000
                    End Select

            End Select

        ElseIf XYswitched = True Then

            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + 10000
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) - 10000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + 10000
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) - 10000
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) - 10000
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) + 10000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) - 10000
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) + 10000
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + 10000
                            TextBox1.Text = Val(TextBox1.Text) + 10000
                        Case "W"
                            TextBox3.Text = Val(TextBox3.Text) - 10000
                            TextBox1.Text = Val(TextBox1.Text) - 10000
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + 10000
                            TextBox2.Text = Val(TextBox2.Text) + 10000
                        Case "S"
                            TextBox4.Text = Val(TextBox4.Text) - 10000
                            TextBox2.Text = Val(TextBox2.Text) - 10000
                    End Select

            End Select

        End If
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click

        'zmiana rozmiarów mapy przyciskami
        Dim bokY As Long
        Dim bokX As Long

        bokY = Val(TextBox5.Text) * 1000
        bokX = Val(TextBox6.Text) * 1000


        If XYswitched = False Then
            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) + bokY
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - bokY
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) + bokX
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - bokX
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox4.Text = Val(TextBox4.Text) - bokY
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) + bokY
                        Case "N"
                            TextBox3.Text = Val(TextBox3.Text) - bokX
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) + bokX
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox2.Text = Val(TextBox2.Text) + bokY
                            TextBox4.Text = Val(TextBox4.Text) + bokY
                        Case "W"
                            TextBox2.Text = Val(TextBox2.Text) - bokY
                            TextBox4.Text = Val(TextBox4.Text) - bokY
                        Case "N"
                            TextBox1.Text = Val(TextBox1.Text) + bokX
                            TextBox3.Text = Val(TextBox3.Text) + bokX
                        Case "S"
                            TextBox1.Text = Val(TextBox1.Text) - bokX
                            TextBox3.Text = Val(TextBox3.Text) - bokX
                    End Select

            End Select



        ElseIf XYswitched = True Then
            Select Case ComboBox1.Text
                Case "powiększ"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + bokX
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) - bokX
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + bokY
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) - bokY
                    End Select

                Case "pomniejsz"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) - bokX
                        Case "W"
                            TextBox1.Text = Val(TextBox1.Text) + bokX
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) - bokY
                        Case "S"
                            TextBox2.Text = Val(TextBox2.Text) + bokY
                    End Select

                Case "przesuń"

                    Select Case ComboBox2.Text
                        Case "E"
                            TextBox3.Text = Val(TextBox3.Text) + bokX
                            TextBox1.Text = Val(TextBox1.Text) + bokX
                        Case "W"
                            TextBox3.Text = Val(TextBox3.Text) - bokX
                            TextBox1.Text = Val(TextBox1.Text) - bokX
                        Case "N"
                            TextBox4.Text = Val(TextBox4.Text) + bokY
                            TextBox2.Text = Val(TextBox2.Text) + bokY
                        Case "S"
                            TextBox4.Text = Val(TextBox4.Text) - bokY
                            TextBox2.Text = Val(TextBox2.Text) - bokY
                    End Select

            End Select
        End If
    End Sub


    Private Sub SKŁADAJWARSTWYToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SKŁADAJWARSTWYToolStripMenuItem.Click

        Dim PB As New PictureBox

        rozszerzenie = "png"

        'tworzy tablice z plików w folderach dolnej i górnej warstwy
        Dim files1() As String = Directory.GetFiles(folderWarstwa1)
        Dim files2() As String = Directory.GetFiles(folderWarstwa2)
        Dim ileZnakowW1 As Integer  'ile znaków w ścieżkach dostępu do folderów dolnej i górnej warstwy
        Dim ileZnakowW2 As Integer
        Dim ileZnakowFiles1 As Integer 'ile znaków w w nazwie pliku (ale tylko plik, bez ścieżki dostępu)
        Dim png As String               'zmienna służąca do przechowywania ostatnich znaków nazwy pliku w celu sprawdzenia czy to plik PNG
        Dim nazwaPliku As String        'służy do przechowania samej nazwy pliku - bez ścieżki dostępu

        If folderWarstwa1 = folderWarstwa2 Or folderWarstwa1 = folderWynikowy Or folderWarstwa2 = folderWynikowy Then
            MsgBox("Folder warstwy dolnej:" & Chr(13) & Chr(10) & _
                    folderWarstwa1 & Chr(13) & Chr(10) & _
                    "Folder warstwy górnej:" & Chr(13) & Chr(10) & _
                    folderWarstwa2 & Chr(13) & Chr(10) & _
                    "Folder warstw połączonych:" & Chr(13) & Chr(10) & _
                    folderWynikowy & Chr(13) & Chr(10) & _
                                      Chr(13) & Chr(10) & _
                    "Wybierz trzy różne foldery.")
            GoTo errorhandler
        End If

        'określa ilość znaków w ścieżkach dostępu do górnego i dolnego folderu
        ileZnakowW1 = Len(folderWarstwa1)
        ileZnakowW2 = Len(folderWarstwa2)



        For Each Plik In files1
            png = Microsoft.VisualBasic.Right(Plik, 4)
            If png = ".png" Then
                'sprawdza czy drugim katalogu istnieje odpowiedni plik
                'najpierw z wyodrębnia z nazwy pliku samą nazwę - bez ścieżki dostępu
                ileZnakowFiles1 = Len(Plik) - ileZnakowW1
                nazwaPliku = Microsoft.VisualBasic.Right(Plik, ileZnakowFiles1)
                'i sprawdza czy taki sam plik istnieje w drugim katalogu
                If File.Exists(folderWarstwa2 & nazwaPliku) Then




                    'jeśli tak to nakłada obrazki na siebie

                    PB.BorderStyle = BorderStyle.FixedSingle
                    PB.Size = New Size(GroupBox3.Width, GroupBox3.Width)
                    PB.Location = New Point(Panel1.Location.X, Panel1.Location.Y)
                    PB.SizeMode = PictureBoxSizeMode.StretchImage
                    PB.Visible = False
                    Me.Panel1.Controls.Add(PB)



                    Dim g As Object
                    Dim t As New Rectangle(0, 0, Val(TextBox9.Text), Val(TextBox9.Text))
                    Dim warstwa1 As New Bitmap(folderWarstwa1 & nazwaPliku)  'dolna warstwa
                    Dim warstwa2 As New Bitmap(folderWarstwa2 & nazwaPliku)  'górna warstwa
                    Dim warstwa1copy As Object


                    '   Dim warstwa1Tmp As New Bitmap(warstwa1.Width, warstwa1.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                    '   warstwa1Tmp = Image.FromFile(folderWarstwa1 & nazwaPliku)
                    '   warstwa1 = warstwa1Tmp
                    '   Dim warstwa2Tmp As New Bitmap(warstwa2.Width, warstwa1.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                    '   warstwa2Tmp = Image.FromFile(folderWarstwa2 & nazwaPliku)
                    '   warstwa2 = warstwa2Tmp
                    '   MsgBox("1: " & warstwa1.PixelFormat & " 1tmp: " & warstwa1Tmp.PixelFormat)
                    '   MsgBox("2: " & warstwa2.PixelFormat & " 2tmp: " & warstwa2Tmp.PixelFormat)


                    warstwa1copy = warstwa1.Clone()
                    warstwa2.MakeTransparent(Color.White)
                    g = Graphics.FromImage(warstwa1)
                    On Error Resume Next
                    g.DrawImageUnscaled(warstwa2, t)
                    PB.Image = warstwa1
                    PB.SizeMode = PictureBoxSizeMode.StretchImage
                    PB.Image.Save(folderWynikowy & nazwaPliku)

                    '  ToolStripProgressBar1.Value = (i / ileW1) * 100




                    '        Dim g As Object
                    '        Dim t As New Rectangle(0, 0, Val(TextBox9.Text), Val(TextBox9.Text))
                    '        Dim warstwa1 As New Bitmap(folderWarstwa2 & nazwa & i & "." & rozszerzenie)  'dolna warstwa
                    '        Dim warstwa2 As New Bitmap(folderWarstwa1 & nazwa & i & "." & rozszerzenie)  'górna warstwa
                    '        Dim warstwa2copy As Object
                    '
                    '                   warstwa2copy = warstwa2.Clone()
                    '                   warstwa1.MakeTransparent(Color.White)
                    '                   g = Graphics.FromImage(warstwa2)
                    '                   g.DrawImageUnscaled(warstwa1, t)
                    '                   PB.Image = warstwa2
                    '                   PB.SizeMode = PictureBoxSizeMode.StretchImage
                    '                  PB.Image.Save(folderWynikowy & nazwa & i & "." & rozszerzenie


                End If

            End If
        Next










        MsgBox("Pliki zostały zapisane", MsgBoxStyle.Information, "Infromacja")

        ToolStripProgressBar1.Value = 0
        Process.Start(folderWynikowy)

errorhandler:
        ' wersja oryginalna:
        '       Dim g As Object
        '       Dim t As New Rectangle(0, 0, Val(TextBox9.Text), Val(TextBox9.Text))
        '       Dim warstwa1 As New Bitmap("C:\Documents and Settings\tomek\Moje dokumenty\mapy\geoportal\bieszczady\PictureBox2_2.jpg")
        '       Dim warstwa2 As New Bitmap("C:\Documents and Settings\tomek\Moje dokumenty\mapy\geoportal\bieszczady\PictureBox2_1.jpg")
        '       Dim warstwa2copy As Object

        '        warstwa2copy = warstwa2.Clone()
        '        warstwa1.MakeTransparent(Color.White)
        '        g = Graphics.FromImage(warstwa2)
        '        g.DrawImageUnscaled(warstwa1, t)
        '        PictureBox1.Image = warstwa2
        '        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        '        PictureBox1.Image.Save("C:\Documents and Settings\tomek\Moje dokumenty\mapy\geoportal\bieszczady\PictureBox2.jpg")


        '       MsgBox("Plik został zapisany", MsgBoxStyle.Information, "Infromacja")


    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Module1.plikGeoreferencyjny_map()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.Show()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        Form3.Show()
    End Sub

    Private Sub GeoportalToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GeoportalToolStripMenuItem.Click
        Form4.Show()
    End Sub

    Private Sub WybierzFolderyWarstwDoZłożeniaToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WybierzFolderyWarstwDoZłożeniaToolStripMenuItem.Click
        Form2.Show()
    End Sub
    Private Sub Instrukcja_obslugi_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_obslugi.Show()
    End Sub

    Private Sub wczytaj_lastsettings()
        On Error GoTo brakpliku

        'wpisuje ostatnie ustawienia z pliku lastsettings.txt i conf.txt
        FileOpen(1, myPath & "\lastsettings.txt", OpenMode.Input)

        Dim zmiennaNaSztuke As String = ""      'do przechowania opisu zmiennej znajdującej sią poniżej opisu
        Input(1, zmiennaNaSztuke)
        Input(1, folderSegmentow)   'wczytuje ostatni folder segmentów, z niego wczytany zostanie plik conf
        If File.Exists(myPath & "\download\conf.txt") = False Then
brakConfTxt:
            MsgBox("Brak folderu, do którego ostatnio pobierano lub brak w nim pliku conf.txt.", , "Wybierz folder, do którego chcesz pobierać mapy.")
            If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                folderSegmentow = FolderBrowserDialog1.SelectedPath & "\"
                If File.Exists(myPath & "\download\conf.txt") = False Then
                    GoTo brakConfTxt
                End If
            Else
                MsgBox("Przed rozpoczęciem pobierania wskaż folder z plikiem conf.txt lub zapisz nową sesję.", , "Nie wskazano właściwego folderu pobierania.")
            End If
        End If
        Input(1, zmiennaNaSztuke)
        Input(1, CheckGmi)          'wczytuje czy tworzyć gmi
        Input(1, zmiennaNaSztuke)
        Input(1, CheckMap)          'wczytuje czy tworzyć map
        Input(1, zmiennaNaSztuke)
        Input(1, CheckWldPoints)    'wczytuje czy tworzyć wld i points
        Input(1, zmiennaNaSztuke)
        Input(1, CheckJpgw)          'wczytuje czy tworzyć jpgw
        Input(1, zmiennaNaSztuke)
        Input(1, folderWarstwa1)    'wczytuje foldery łączonych warstw
        Input(1, zmiennaNaSztuke)
        Input(1, folderWarstwa2)
        Input(1, zmiennaNaSztuke)
        Input(1, folderWynikowy)
        Input(1, zmiennaNaSztuke)
        Input(1, XYswitched)
        Input(1, zmiennaNaSztuke)
        Input(1, CheckTB)
        Input(1, zmiennaNaSztuke)
        Input(1, CheckNrSeg)
		Input(1, zmiennaNaSztuke)
        Input(1, iloscProbPobrania)
        Input(1, zmiennaNaSztuke)
        Input(1, przerwaMiedzyProbami)
        FileClose(1)
brakpliku:
    End Sub

    Private Sub wczytajConf()

        Dim formatNaProbe As String 'służy do wczytania rozszerzenia obrazka z pliku i jeśli module1.format jest inny to następuje zamiana
        formatNaProbe = ""

        If form1loaded = True Then
            If Me.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                folderSegmentow = Me.FolderBrowserDialog1.SelectedPath & "\"
            End If
            If Dir(myPath & "\download\conf.txt") = "" Then
                MsgBox("Brak pliku w podanej lokalizacji.")
                GoTo errorhandler
            End If
        End If



        'wyświetla nazwę kwadratu na pasku stanu
        ToolStripStatusLabel1.Text = folderSegmentow

        'zazwyczaj brak conf.txt zostanie wychwycony kilka linijek wyżej, ale przypierwszym uruchomieniu conf.txt jeszcze nie ma
        On Error GoTo errorhandler
        FileOpen(1, myPath & "\download\conf.txt", OpenMode.Input)
        Input(1, folderSegmentow)
        Input(1, ComboBox3.Text)
        Input(1, TextBox1.Text)
        Input(1, TextBox2.Text)
        Input(1, TextBox3.Text)
        Input(1, TextBox4.Text)
        Input(1, TextBox9.Text)
        Input(1, TextBox10.Text)
        Input(1, warstwy(0))
        Input(1, warstwy(1))
        Input(1, warstwy(2))
        Input(1, warstwy(3))
        Input(1, warstwy(4))
        Input(1, warstwy(5))
        Input(1, warstwy(6))
        Input(1, warstwy(7))
        Input(1, warstwy(8))
        Input(1, warstwy(9))
        Input(1, warstwy(10))
        Input(1, warstwy(11))
        Input(1, nrWarstwy)
        Input(1, formatNaProbe)

        For i = 0 To 11
            If warstwy(i) = "#ERROR 448#" Then warstwy(i) = ""
        Next

        If EOF(1) Then
            'jeśli  jest to plik starego typu to progrma przyjmuje domyślne wartości zmiennych
            wspolnaNazwaKwadratu = "PictureBox"
        Else
        Input(1, wspolnaNazwaKwadratu)
        End If


        If EOF(1) Then
            'jeśli  jest to plik starego typu to progrma przyjmuje domyślne wartości zmiennych
            pobierajPowyzejOstatniego = False
        Else
            Input(1, pobierajPowyzejOstatniego)
        End If

        FileClose(1)


        If formatNaProbe <> Module1.format Then
            'MsgBox("Ustawiłeś rozszerzenie pobieranych plików na " & "." & Module1.format & Chr(13) & Chr(10) & _
            '"podczas gdy rozszerzenie z wczytanej sesji to " & "." & formatNaProbe & Chr(13) & Chr(10) & _
            'Chr(13) & Chr(10) & _
            '"Program zamienił rozszerzenie na ." & formatNaProbe, , "ZMIENIONO ROZSZERZENIE")
            Module1.format = formatNaProbe


            'Dim combobox1index As Integer
            'Dim last As Integer

            'combobox1index = 0

            'last = Form2.ComboBox1.Items.Count - 1
            'Do Until Form2.ComboBox1.Items.Item(combobox1index) = formatNaProbe Or combobox1index < last
            'combobox1index = combobox1index + 1
            'Loop

            'Form2.ComboBox1.SelectedIndex = combobox1index

        End If

        ToolStripStatusLabel2.Text = "." & rozszerzenie
        Label11.Text = "1) " & warstwy(0)
        Label12.Text = "2) " & warstwy(1)
        Label13.Text = "3) " & warstwy(2)
        Label14.Text = "4) " & warstwy(3)
        Label15.Text = "5) " & warstwy(4)
        Label16.Text = "6) " & warstwy(5)
        Label27.Text = "7) " & warstwy(6)
        Label24.Text = "8) " & warstwy(7)
        Label28.Text = "9) " & warstwy(8)
        Label26.Text = "10) " & warstwy(9)
        Label25.Text = "11) " & warstwy(10)
        Label23.Text = "12) " & warstwy(11)

        'wyłączyłem wyskakujące okienko folderu download (30. 03. 2015. Kazik)
        'If folderSegmentow <> "" Then
        'Process.Start(folderSegmentow)
        ' End If

errorhandler:
    End Sub
    'wczytanie listy plików z warstwami do combobox3
    Private Sub wczytaj_warstwyTxt()

        Dim pozycjaListy As String = ""

        FileClose(1) 'w razie gyby był otwarty

        FileOpen(1, myPath & "\warstwy\warstwy.txt", OpenMode.Input)

        Do Until EOF(1)
            Input(1, pozycjaListy)
            ComboBox3.Items.Add(pozycjaListy)
        Loop


        FileClose(1)

errorhandler:
    End Sub
    'wcztanie danych do listbox1 w zależności od wyboru w combobox3
    Private Sub wczytaj_warstwy_z_pliku()

        Dim pozycjaListy As String = ""
        Dim pusta As String = ""    'zmienna na linie, które nie będą użyte

        FileClose(1) 'w razie gyby był otwarty

        ListBox1.Items.Clear()

        On Error Resume Next
        FileOpen(1, myPath & "\warstwy\" & ComboBox3.Text & ".txt", OpenMode.Input)

        Input(1, adresSerwera)

        Do Until EOF(1)
            Input(1, pozycjaListy)
            ListBox1.Items.Add(pozycjaListy)
            Input(1, pusta)
        Loop

        FileClose(1)

errorhandler:

    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox3.SelectedIndexChanged
        'zeruje wybrane warstwy
        Module1.przerwij()
        'wczytuje nowe do wyboru
        wczytaj_warstwy_z_pliku()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Module1.przerwij()
        Button1.Enabled = True
        Button3.Enabled = False
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Module1.resetuj()
        Button1.Enabled = True
        Button3.Enabled = False

        FileOpen(1, myPath & "\download\conf.txt", OpenMode.Output)
        WriteLine(1, folderSegmentow)
        WriteLine(1, ComboBox3.Text)
        WriteLine(1, TextBox1.Text)
        WriteLine(1, TextBox2.Text)
        WriteLine(1, TextBox3.Text)
        WriteLine(1, TextBox4.Text)
        WriteLine(1, TextBox9.Text)
        WriteLine(1, TextBox10.Text)
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
        FileClose(1)




    End Sub

    Private Sub UsuńPusteSegmentyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UsuwaniePustychSegmentówToolStripMenuItem.Click
        Form5.Show()
    End Sub


End Class
