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
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers




Public Class Form1

    Private Property form1loaded As Boolean = False   'wskazuje, że form1 została już załadowana



    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


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

        ' pozostałe parametry
        myPath = My.Application.Info.DirectoryPath.ToString()

        Module1.folderSegmentow = myPath & "\download\"

        If Directory.Exists(myPath & "\download\") = False Then Directory.CreateDirectory(myPath & "\download\")


        Me.SetDesktopLocation(0, 0)



        'ustawienia startowe okna mapy

        'punkt startowy mapy głównej
        Me.GMapControl1.Manager.Mode = AccessMode.ServerAndCache
        Me.GMapControl1.DragButton = Windows.Forms.MouseButtons.Left
        Me.GMapControl1.MapScaleInfoEnabled = False
        Me.GMapControl1.DisableAltForSelection = True
        Me.GMapControl1.Zoom = Val(Label65.Text)

        TextBox1.Enabled = False
        TextBox2.Enabled = False
        TextBox3.Enabled = False
        TextBox4.Enabled = False
        Label3.Enabled = False
        Label4.Enabled = False
        Label5.Enabled = False
        Label6.Enabled = False


        Label31.Visible = False
        Label32.Visible = False
        Label33.Visible = False
        Label34.Visible = False




        'wczytywanie ustaleń okna z lastsetting. Jeśli go nie ma, to szuka conf. Gdy go zabraknie, to sięgamy po sztywny start

        If File.Exists(folderSegmentow & "\conf.txt") = False Then
            Button8.Enabled = False
        End If


        If File.Exists(myPath & "lastsettings.txt") = False Then
            wczytaj_lastsettings()

            Module1.x_start = Label35.Text
            Module1.y_start = Label63.Text
            Module1.zoom_start = Label65.Text
            Module1.x_start = Replace(Val(Label35.Text).ToString, ",", ".")
            Module1.y_start = Replace(Val(Label63.Text).ToString, ",", ".")
            Module1.zoom_start = Replace(Val(Label65.Text).ToString, ",", ".")

            Me.GMapControl1.Zoom = Val(Label65.Text)
            Me.GMapControl1.Position = New PointLatLng(Val(Label35.Text.ToString), Val(Label63.Text.ToString))
        ElseIf File.Exists(folderSegmentow & "conf.txt") = False Then
            wczytajConf()

            Module1.x_start = Label35.Text
            Module1.y_start = Label63.Text
            Module1.zoom_start = Label65.Text
            Module1.x_start = Replace(Val(Label35.Text).ToString, ",", ".")
            Module1.y_start = Replace(Val(Label63.Text).ToString, ",", ".")
            Module1.zoom_start = Replace(Val(Label65.Text).ToString, ",", ".")

            Me.GMapControl1.Zoom = Val(Label65.Text)
            Me.GMapControl1.Position = New PointLatLng(Val(Label35.Text.ToString), Val(Label63.Text.ToString))
        Else

            Me.GMapControl1.Position = New PointLatLng(52.3, 19.2)
            Me.GMapControl1.Zoom = 6
        End If


        'przypisuje wartość zmiennej publicznej nrWarstwy
        nrWarstwy = 0

        wczytaj_lastsettings()

        wczytajConf()

        'tworzy combobox z listą dostępnych warstw
        wczytaj_warstwyTxt()
        wczytaj_warstwy_z_pliku()

        ToolStripStatusLabel2.Text = "." & format
        If format = "" Then
            ToolStripStatusLabel2.Text = "jpeg"
            Form2.ComboBox1.Text = "jpeg"
        End If
        ToolStripStatusLabel3.Text = ""

     
        form1loaded = True


    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Module1.plik_lastsettings()

    End Sub

    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
        RichTextBox1.ForeColor = System.Drawing.Color.Green
        RichTextBox1.Text = "Trwa pobieranie segmentów"

        'w zależności od formatu obrazka odpowiednie rozszerzenie pliku
        Select Case Module1.format
            Case "jpeg"
                Module1.rozszerzenie = "jpg" 'format wysyłania zapytań do wms
            Case "tiff"
                Module1.rozszerzenie = "tif"
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

        folderSegmentow = ToolStripStatusLabel1.Text
        Module1.utworzPlikConf() 'na wszelki wypadek - żeby nie okazało się, że zapisuje w folderze głównym programu


        If folderSegmentow = "" Then
            MsgBox("folder segmentów jest pusty")
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

        If warstwy(0) <> "" Then
            RichTextBox1.ForeColor = System.Drawing.Color.Green
            RichTextBox1.Text = "Wskazano warstwę: " & ListBox1.SelectedItem
        End If


errororhandler:
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox8.Text = Val(TextBox11.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        End Sub
    Private Sub TextBox4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox4.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox8.Text = Val(TextBox11.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        End Sub
    Private Sub TextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Val(TextBox12.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
         End Sub
    Private Sub TextBox3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged
         TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Val(TextBox12.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        End Sub
    Private Sub TextBox9_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox9.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox8.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox13.Text = (Val(TextBox10.Text) * Val(TextBox9.Text)) / 1000
    End Sub
    Private Sub TextBox10_TextChanged(sender As Object, e As EventArgs) Handles TextBox10.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox8.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox13.Text = (Val(TextBox10.Text) * Val(TextBox9.Text)) / 1000
    End Sub
    
    Private Sub ZapiszToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZapiszToolStripMenuItem.Click
        Module1.utworzPlikConf()
    End Sub

    Private Sub WczytajToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WczytajToolStripMenuItem1.Click
        FileClose(1) ' na wypadek gdyby był otwarty
        For i = 1 To 2 'nierozwiązany bug - dopiero po dwukrotnym wczytaniu conf, aktualizują się parametry sesji
            wczytajConf()
        Next
    End Sub

    Private Sub OtwórzOknoUstawieńToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UstawieniaToolStripMenuItem.Click
        Form2.Show()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.Show()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        pomoc_pfe.Show()
    End Sub

    Private Sub Instrukcja_obslugi_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_Obslugi.Show()
    End Sub

    Private Sub wczytaj_lastsettings()
        On Error GoTo brakpliku
        
        FileClose(1) 'w razie gyby był otwarty

        'wpisuje ostatnie ustawienia z pliku lastsettings.txt i conf.txt
        FileOpen(1, myPath & "\lastsettings.txt", OpenMode.Input)

        Dim zmiennaNaSztuke As String = ""      'do przechowania opisu zmiennej znajdującej sią poniżej opisu
        Input(1, zmiennaNaSztuke)
        Input(1, folderSegmentow)   'wczytuje ostatni folder segmentów, z niego wczytany zostanie plik conf

        Input(1, zmiennaNaSztuke)
        Input(1, CheckGmi)          'wczytuje czy tworzyć gmi
        Input(1, zmiennaNaSztuke)
        Input(1, CheckMap)          'wczytuje czy tworzyć map
        Input(1, zmiennaNaSztuke)
        Input(1, CheckWldPoints)    'wczytuje czy tworzyć wld i points
        Input(1, zmiennaNaSztuke)
        Input(1, CheckJpgw)          'wczytuje czy tworzyć jpgw
        Input(1, zmiennaNaSztuke)
        Input(1, CheckKml)          'wczytuje czy tworzyć kml
        Input(1, zmiennaNaSztuke)
        Input(1, CheckTab)          'wczytuje czy tworzyć tab
        Input(1, zmiennaNaSztuke)
        Input(1, folderWarstwa1)    'wczytuje foldery łączonych warstw
        Input(1, zmiennaNaSztuke)
        Input(1, folderWarstwa2)
        Input(1, zmiennaNaSztuke)
        Input(1, folderWynikowy)
        Input(1, zmiennaNaSztuke)
        Input(1, XYswitched)
        'Input(1, zmiennaNaSztuke)
        'Input(1, CheckTB)
        Input(1, zmiennaNaSztuke)
        Input(1, numeracja)
        Input(1, zmiennaNaSztuke)
        Input(1, iloscProbPobrania)
        Input(1, zmiennaNaSztuke)
        Input(1, przerwaMiedzyProbami)
        Input(1, zmiennaNaSztuke)
        Input(1, Label35.Text)
        Input(1, zmiennaNaSztuke)
        Input(1, Label63.Text)
        Input(1, zmiennaNaSztuke)
        Input(1, Label65.Text)
        FileClose(1)

        RichTextBox1.ForeColor = System.Drawing.Color.Green
        RichTextBox1.Text = "Wczytano ostatnio zapisane ustawienia programu. Format graficzny pobieranych segmentów to " & rozszerzenie & " . Styl ich numerowania to: " & numeracja

brakpliku:
          End Sub

    Private Sub wczytajConf()

        Dim formatNaProbe As String 'służy do wczytania rozszerzenia obrazka z pliku i jeśli module1.format jest inny to następuje zamiana
        formatNaProbe = ""

        If form1loaded = True Then
            If Me.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                folderSegmentow = Me.FolderBrowserDialog1.SelectedPath & "\"
            End If
            If Dir(folderSegmentow & "\conf.txt") = "" Then
                MsgBox("Brak pliku w podanej lokalizacji.")
                GoTo errorhandler
            End If
        End If

        'wyświetla nazwę kwadratu na pasku stanu
        ToolStripStatusLabel1.Text = folderSegmentow

        'zazwyczaj brak conf.txt zostanie wychwycony kilka linijek wyżej, ale przypierwszym uruchomieniu conf.txt jeszcze nie ma
        On Error GoTo errorhandler

        FileOpen(1, folderSegmentow & "\conf.txt", OpenMode.Input)
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
        Input(1, wspolnaNazwaKwadratu)
        Input(1, pobierajPowyzejOstatniego)
        Input(1, Label35.Text)
        Input(1, Label63.Text)
        Input(1, Label65.Text)
        Input(1, Form2.ComboBox2.Text)
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

        ToolStripStatusLabel2.Text = "." & format
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

        'odświeżenie widoku okna mapy po wprowadzeniu nowych ustawień conf.txt
        Me.GMapControl1.Refresh()
        Me.GMapControl1.ReloadMap()
        Me.GMapControl1.Zoom = Val(Label65.Text)
        Me.GMapControl1.Position = New PointLatLng(Val(Label35.Text.ToString), Val(Label63.Text.ToString))
        x_start = Label35.Text
        y_start = Label63.Text
        zoom_start = Label65.Text
        'End If

        Me.Refresh()

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


        'czyszczenie listy dotychczasowych warstw
        nrWarstwy = 0
        For i = 0 To 11
            warstwy(i) = ""
        Next
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

        strUrlparts(1) = ""


        If warstwy(0) = "" Then
            RichTextBox1.ForeColor = System.Drawing.Color.Red
            RichTextBox1.Text = "Zmieniono rodzaj mapy. Wybierz która dokładnie jej warstwa ma zostać pobrana, klikając na nią myszką"
        End If

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Module1.przerwij()
        Button1.Enabled = True
        Button3.Enabled = False
        RichTextBox1.ForeColor = System.Drawing.Color.Red
        RichTextBox1.Text = "Przerwano pobieranie segmentów"
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Module1.resetuj()
        Module1.utworzPlikConf()
        Button1.Enabled = True
        Button3.Enabled = False

    End Sub

    Private Sub UsuńPusteSegmentyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UsuwaniePustychSegmentówToolStripMenuItem.Click
        usuwanie_p_seg.Show()
    End Sub
    Private Sub SkładajWarstwyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NakładanieWarstwNaSiebieToolStripMenuItem.Click
        nakladanie_map.Show()
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged

        Me.GMapControl1.MapProvider = GMapProviders.OpenStreetMap
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        Me.GMapControl1.MapProvider = GMapProviders.GoogleMap
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        Me.GMapControl1.MapProvider = GMapProviders.BingSatelliteMap
    End Sub

    Private Sub GMapControl1_MouseMove(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseMove

        'współrzędne bbox podczas przeciągania zaznaczenia
        If Module1.editXY = False Then
            If Me.GMapControl1.SelectedArea.IsEmpty = False Then
                Dim X0_92 As Integer
                Dim Y0_92 As Integer
                Dim X1_92 As Integer
                Dim Y1_92 As Integer
                Dim X0_84 = Me.GMapControl1.SelectedArea.Lng
                Dim Y1_84 = Me.GMapControl1.SelectedArea.Lat
                Dim X1_84 = Me.GMapControl1.SelectedArea.Lng + Me.GMapControl1.SelectedArea.WidthLng
                Dim Y0_84 = Me.GMapControl1.SelectedArea.Lat - Me.GMapControl1.SelectedArea.HeightLat


                X0_92 = Transform.GetXU92(Y1_84, X1_84)   'prawy gorny x
                TextBox3.Text = X0_92
                Y0_92 = Transform.GetYU92(Y1_84, X1_84)  'prawy gorny y
                TextBox4.Text = Y0_92
                X1_92 = Transform.GetXU92(Y0_84, X0_84)   'lewy dolny x
                TextBox1.Text = X1_92
                Y1_92 = Transform.GetYU92(Y0_84, X0_84)  'lewy gorny y
                TextBox2.Text = Y1_92


                Label31.Text = "lat= " + Convert.ToString(Round(Y1_84, 4))
                Label32.Text = "lat= " + Convert.ToString(Round(Y0_84, 4))
                Label33.Text = "lng= " + Convert.ToString(Round(X1_84, 4))
                Label34.Text = "lng= " + Convert.ToString(Round(X0_84, 4))

                If Val(TextBox8.Text) * Val(TextBox7.Text) < 5000000000 Then
                    RichTextBox1.ForeColor = System.Drawing.Color.Green
                    RichTextBox1.Text = "Zaznaczyłeś obszar do pobrania o powierzchni " & ((Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) * ((Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) & " km2"

                Else
                    RichTextBox1.ForeColor = System.Drawing.Color.Red
                    RichTextBox1.Text = "Zaznaczyłeś obszar do pobrania o łącznej rozdzielczości " & Val(TextBox8.Text) & "x" & Val(TextBox7.Text) & " px. To dużo. Poradzę sobie. Jednak uzbroj się lepiej w kubek gorącej kawy :)"
                End If
            End If
        End If

        'współrzędne środka mapy w wgs84
        Dim Lat_srodek As Double = Me.GMapControl1.Position.Lat
        Dim Lng_srodek As Double = Me.GMapControl1.Position.Lng
        Label35.Text = Convert.ToString(Round(Lat_srodek, 4))
        Label63.Text = Convert.ToString(Round(Lng_srodek, 4))
        'współrzędne podczas ruchu myszą
        Dim lat_mysz As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat
        Dim lng_mysz As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng

        Dim lng_92 As Integer = Transform.GetXU92(lat_mysz, lng_mysz)   'prawy gorny x
        Dim lat_92 As Integer = Transform.GetYU92(lat_mysz, lng_mysz)   'prawy gorny y
        Label41.Text = "x= " + Convert.ToString(lng_92) + "   y= " + Convert.ToString(lat_92)

        Dim mouseY As Double = e.Location.Y
        Dim mouseX As Double = e.Location.X
        Label36.BackColor = Color.Transparent
        Label36.Location = New Point(mouseX - 60, mouseY - 10)
        'Label43.Text = "lat= " + Convert.ToString(Round(lat_mysz, 4)) + "   lng= " + Convert.ToString(Round(lng_mysz, 4))
        If kursorWGS84 = True Then
            Label36.Text = "lat= " + Convert.ToString(Round(lat_mysz, 4)) + "   lng= " + Convert.ToString(Round(lng_mysz, 4))
            Label36.Visible = True
        Else
            Label36.Visible = False
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim zoom As Integer = Me.GMapControl1.Zoom
        Me.GMapControl1.Zoom = zoom + 1
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim zoom As Integer = Me.GMapControl1.Zoom
        Me.GMapControl1.Zoom = zoom - 1
    End Sub

    Private Sub GMapControl1_OnMapZoomChanged() Handles GMapControl1.OnMapZoomChanged
        Label65.Text = Me.GMapControl1.Zoom.ToString
    End Sub

    Private Sub ScalanieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScalanieToolStripMenuItem.Click
        Form3.Show()
    End Sub

    
    Private Sub Label11_TextChanged(sender As Object, e As EventArgs) Handles Label11.TextChanged
        If warstwy(0) <> "" Then
            Button6.Enabled = True
        Else
            Button6.Enabled = False
        End If
    End Sub


    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Process.Start(folderSegmentow)
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Form3.Show()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Module1.resetuj()
        Module1.utworzPlikConf()
        Button1.Enabled = True
        Button3.Enabled = False
    End Sub


End Class



      