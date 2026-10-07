Imports System.IO

Public Class Form3
    Dim folderScalania As String = ""

    Dim styl_numeracji As String
    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load






        folderScalania = folderDanych & "\download\"
        TextBox1.Text = folderScalania
        TextBox1.Enabled = False
        GroupBox1.Enabled = False
        CheckBox2.Checked = Module1.georef_scalanie_qgis
        CheckBox3.Checked = Module1.georef_scalanie_kml
        CheckBox4.Checked = Module1.georef_scalanie_map
        CheckBox5.Checked = Module1.georef_scalanie_tab
        Dim zm_jpg As String = ""

        'procedura wczytywania parametrów macierzy z pliku conf.txt
        On Error GoTo errorhandler
        FileClose(1) 'w razie gyby był otwarty

        FileOpen(1, folderScalania & "\conf.txt", OpenMode.Input)
        Input(1, "folderSegmentow")
        Input(1, "rodzaj mapy")
        Input(1, TextBox4.Text)
        Input(1, TextBox5.Text)
        Input(1, TextBox2.Text)
        Input(1, TextBox3.Text)
        Input(1, TextBox6.Text)
        Input(1, TextBox7.Text) ' rozdzielczosc 1 segmentu
        Input(1, "TextBox10.Text")
        Input(1, "null") 'Input(1, warstwy(0))
        Input(1, "null") 'Input(1, warstwy(1))
        Input(1, "null") 'Input(1, warstwy(2))
        Input(1, "null") 'Input(1, warstwy(3))
        Input(1, "null") 'Input(1, warstwy(4))
        Input(1, "null") 'Input(1, warstwy(5))
        Input(1, "null") 'Input(1, warstwy(6))
        Input(1, "null") 'Input(1, warstwy(7))
        Input(1, "null") 'Input(1, warstwy(8))
        Input(1, "null") 'Input(1, warstwy(9))
        Input(1, "null") 'Input(1, warstwy(10))
        Input(1, "null") 'Input(1, warstwy(11))

        Input(1, zm_jpg) 'format

        If wspolnaNazwaKwadratu = "" Then
            Input(1, "null")
            TextBox11.Text = "\"

        Else
            Input(1, TextBox11.Text)
        End If
        Input(1, "null") 'Input(1, pobierajPowyzejOstatniego)
        Input(1, "null") 'Input(1, Label35.Text)
        Input(1, "null") 'Input(1, Label63.Text)
        Input(1, "null") 'Input(1, Label65.Text)
        Input(1, ComboBox2.Text)
        FileClose(1)

errorhandler:
        If zm_jpg = "jpeg" Then
            ComboBox1.Text = "jpg"

        Else
            If zm_jpg = "tiff" Then
                ComboBox1.Text = "tif"

            Else
                ComboBox1.Text = zm_jpg

            End If

        End If

        TextBox8.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox5.Text)) / Val(TextBox7.Text)) / Val(TextBox6.Text))  'ile seg horiz
        TextBox9.Text = Math.Ceiling(((Val(TextBox2.Text) - Val(TextBox4.Text)) / Val(TextBox7.Text)) / Val(TextBox6.Text))  'ile seg wert

        If ComboBox2.Text = "01_02_03" Then
            styl_numeracji = "0"
        ElseIf ComboBox2.Text = "1_2_3" Then
            styl_numeracji = "1"
        ElseIf ComboBox2.Text = "NrWiersza_NrKolumny" Then
            styl_numeracji = "2"
        End If

        If File.Exists(folderScalania & "\error.txt") = False Then

            If File.Exists(folderScalania & "\conf.txt") = False Then
                MsgBox("Brak pliku konfiguracji 'conf.txt' w podanej lokalizacji.")
                CheckBox1.Enabled = True
                RichTextBox1.ForeColor = System.Drawing.Color.Red
                RichTextBox1.Text = "We wskazanym katalogu segmentów nie odnaleziono pliku konfiguracyjnego conf.txt. Jesli został on bezpowrotnie utracony, istnieje możliwość samodzielnego zdefiniowania parametrów segmentów. W tym celu zaznacz opcję 'ręczne wprowadzanie parametrów'"
            Else
                CheckBox1.Enabled = False
                RichTextBox1.Text = "Segmenty gotowe do złączenia. Wszelkie parametry scalania zostały załadowane automatycznie z pliku conf.txt"
            End If
        Else
            RichTextBox1.ForeColor = System.Drawing.Color.Red
            RichTextBox1.Text = "W katalogu segmentów wykryto obecność pliku error.txt co świadczy o niekompletnym zestawie segmentów. Uzupełnij je i usuń plik error.txt"
        End If


    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.FolderBrowserDialog1.SelectedPath = folderDanych & "\download\"
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            folderScalania = FolderBrowserDialog1.SelectedPath & "\"
            If File.Exists(folderScalania & "\conf.txt") = False Then
                MsgBox("Brak pliku konfiguracji 'conf.txt' w podanej lokalizacji.")
            Else
                RichTextBox1.ForeColor = System.Drawing.Color.Green
                RichTextBox1.Text = "Plik conf.txt został pomyślnie załadowany z nowej lokalizacji. Segmenty gotowe do złączenia"
            End If
            TextBox1.Text = folderScalania
            TextBox1.Enabled = True
        End If

    End Sub

    Private Async Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        'Procedura skleja kafle w mapę
        'Wywolanie: ..\NoToCONS.exe [Npoziom] [Npion] [Px] [TypNazwy] [Qjpg] [Path] [Prefix] [NazwaMapy] [Rozszerzenie]

        Dim nazwa_sklejka As String = ""



        If File.Exists(folderScalania & "\error.txt") = True Then
            RichTextBox1.ForeColor = System.Drawing.Color.Red
            RichTextBox1.Text = "W katalogu segmentów wykryto obecność pliku error.txt co świadczy o niekompletnym zestawie segmentów. Uzupełnij je i usuń plik error.txt"
        Else
            nazwa_sklejka = "_scalone_segmenty_" & TextBox8.Text & "x" & TextBox9.Text

            'rozszerzenie plików segmentów i scalonego arkusza - formaty png8/png24/png32 zapisywane są jako .png, jpeg jako .jpg, tiff jako .tif
            Dim rozszerzenieArkusza As String = NormalizujRozszerzenie(ComboBox1.Text)

            'NoToCONS (Delphi) oczekuje ścieżki zakończonej "\"; prefiks i nazwa w cudzysłowach, aby pusty prefiks lub spacje nie przesuwały parametrów
            Dim folderArg As String = folderScalania.TrimEnd("\"c)
            Dim argumenty As String = TextBox8.Text & " " & TextBox9.Text & " " & TextBox6.Text & " " & styl_numeracji & " " & TrackBar1.Value & " " &
                Chr(34) & folderArg & "\" & Chr(34) & " " & Chr(34) & TextBox11.Text & Chr(34) & " " & Chr(34) & nazwa_sklejka & Chr(34) & " " & rozszerzenieArkusza

            Dim plikArkusza As String = folderArg & "\" & nazwa_sklejka & "." & rozszerzenieArkusza
            Dim kodWyjscia As Integer = -1
            Dim poczatekScalania As Date = Now.AddSeconds(-2)   'plik starszy niż ta chwila to pozostałość po wcześniejszym scalaniu

            Button4.Enabled = False
            Me.UseWaitCursor = True
            RichTextBox1.ForeColor = System.Drawing.Color.Black
            RichTextBox1.Text = "Trwa scalanie segmentów. Przy dużych arkuszach może to potrwać kilka minut..."

            Try
                Dim startInfo As New ProcessStartInfo(myPath & "\skrypty\NoToCONS.exe", argumenty)
                startInfo.UseShellExecute = False
                Using proces As Process = Process.Start(startInfo)
                    'czeka na zakończenie scalania bez blokowania okna programu
                    Await System.Threading.Tasks.Task.Run(Sub() proces.WaitForExit())
                    kodWyjscia = proces.ExitCode
                End Using
            Catch ex As Exception
                RichTextBox1.ForeColor = System.Drawing.Color.Red
                RichTextBox1.Text = "Nie udało się uruchomić modułu scalania NoToCONS.exe: " & ex.Message
                Button4.Enabled = True
                Me.UseWaitCursor = False
                Exit Sub
            End Try

            Button4.Enabled = True
            Me.UseWaitCursor = False

            'o powodzeniu świadczy dopiero istnienie pliku scalonego arkusza (dawniej sukces zgłaszano zaraz po uruchomieniu NoToCONS)
            If File.Exists(plikArkusza) AndAlso File.GetLastWriteTime(plikArkusza) >= poczatekScalania Then
                Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
                Form1.RichTextBox1.Text = "Segmenty zostały prawidłowo scalone i zapisane do pliku o nazwie" & " " & nazwa_sklejka & "." & rozszerzenieArkusza
                'pliki georeferencyjne scalonego arkusza powstają obok niego
                Module1.folderScalonych = folderArg & "\"
                Module1.rozszerzenieScalonych = rozszerzenieArkusza
                If Module1.georef_scalanie_qgis = True Then Module1.plikGeoreferencyjny_jpgw()
                If Module1.georef_scalanie_kml = True Then Module1.plikGeoreferencyjny_kml()
                If Module1.georef_scalanie_map = True Then Module1.plikGeoreferencyjny_map()
                If Module1.georef_scalanie_tab = True Then Module1.plikGeoreferencyjny_tab()
                Me.Close()
            Else
                RichTextBox1.ForeColor = System.Drawing.Color.Red
                RichTextBox1.Text = "Błąd. Segmenty nie zostały poprawnie scalone (kod zakończenia NoToCONS: " & kodWyjscia & "). Prawdopodobnie przygotowane wcześniej segmenty obszaru nie są kompletne, bądź po ich skompletowaniu nie został usunięty plik error.txt"
            End If
        End If




    End Sub

    'zamienia nazwę formatu WMS (jpeg, png24, tiff...) na rozszerzenie pliku
    Private Function NormalizujRozszerzenie(ByVal formatPliku As String) As String
        Select Case formatPliku.ToLower()
            Case "jpeg", "jpg"
                Return "jpg"
            Case "tiff", "tif"
                Return "tif"
            Case "png", "png8", "png24", "png32"
                Return "png"
            Case Else
                Return formatPliku
        End Select
    End Function

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            GroupBox1.Enabled = True
            TextBox2.Enabled = False
            TextBox3.Enabled = False
            TextBox4.Enabled = False
            TextBox5.Enabled = False

        Else
            GroupBox1.Enabled = False
        End If
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        Label20.Text = TrackBar1.Value & "%"
    End Sub


    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged

        Module1.georef_scalanie_qgis = CheckBox2.CheckState



    End Sub

    Private Sub CheckBox2_Click(sender As Object, e As EventArgs) Handles CheckBox2.Click
        Select Case CheckBox2.CheckState
            Case CheckState.Checked
                Module1.georef_scalanie_qgis = True

            Case CheckState.Unchecked
                Module1.georef_scalanie_qgis = False
        End Select
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged

        Module1.georef_scalanie_kml = CheckBox3.CheckState



    End Sub

    Private Sub CheckBox3_Click(sender As Object, e As EventArgs) Handles CheckBox3.Click
        Select Case CheckBox3.CheckState
            Case CheckState.Checked
                Module1.georef_scalanie_kml = True

            Case CheckState.Unchecked
                Module1.georef_scalanie_kml = False
        End Select
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged

        Module1.georef_scalanie_map = CheckBox4.CheckState



    End Sub

    Private Sub CheckBox4_Click(sender As Object, e As EventArgs) Handles CheckBox4.Click
        Select Case CheckBox4.CheckState
            Case CheckState.Checked
                Module1.georef_scalanie_map = True

            Case CheckState.Unchecked
                Module1.georef_scalanie_map = False
        End Select
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        Module1.georef_scalanie_tab = CheckBox5.CheckState
    End Sub

    Private Sub CheckBox5_Click(sender As Object, e As EventArgs) Handles CheckBox5.Click
        Select Case CheckBox5.CheckState
            Case CheckState.Checked
                Module1.georef_scalanie_tab = True

            Case CheckState.Unchecked
                Module1.georef_scalanie_tab = False
        End Select
    End Sub

    Private Sub Form3_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Module1.georef_scalanie_qgis = False
        Module1.georef_scalanie_kml = False
        Module1.georef_scalanie_map = False
        Module1.georef_scalanie_tab = False

    End Sub
End Class