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

Imports System.IO

Public Class Nakladanie_Map


    'foldery zapamiętane w lastsettings.txt (Module1), a gdy ich brak - domyślne podkatalogi folderu download
    Dim folderWarstwa1 As String = If(Module1.folderWarstwa1 <> "", Module1.folderWarstwa1, folderDanych & "\download\dolna\")           'folder warstwy dolnej
    Dim folderWarstwa2 As String = If(Module1.folderWarstwa2 <> "", Module1.folderWarstwa2, folderDanych & "\download\gorna\")          'folder warstwy górnej
    Dim folderWynikowy As String = If(Module1.folderWynikowy <> "", Module1.folderWynikowy, folderDanych & "\download\polaczone\")


    Private Sub TextBox3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged

        folderWarstwa1 = TextBox3.Text
        Module1.folderWarstwa1 = folderWarstwa1   'zapisywany przy zamykaniu programu w lastsettings.txt

    End Sub

    Private Sub TextBox4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox4.TextChanged

        folderWarstwa2 = TextBox4.Text
        Module1.folderWarstwa2 = folderWarstwa2

    End Sub

    Private Sub TextBox5_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox5.TextChanged

        folderWynikowy = TextBox5.Text
        Module1.folderWynikowy = folderWynikowy

    End Sub

    Private Sub Form6_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Me.Location = New Point(200, 150)



        TextBox3.Text = folderWarstwa1
        TextBox4.Text = folderWarstwa2
        TextBox5.Text = folderWynikowy

    End Sub


    Private Sub ButtonDolna_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDolna.Click
        Me.FolderBrowserDialog1.SelectedPath = folderSegmentow
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            folderWarstwa1 = FolderBrowserDialog1.SelectedPath & "\"
        End If


        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = folderWarstwa1
        TextBox3.Text = folderWarstwa1

    End Sub

    Private Sub ButtonGorna_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGorna.Click
        Me.FolderBrowserDialog1.SelectedPath = folderSegmentow
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            folderWarstwa2 = FolderBrowserDialog1.SelectedPath & "\"
        End If


        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = folderWarstwa2
        TextBox4.Text = folderWarstwa2
    End Sub

    Private Sub ButtonPolaczone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPolaczone.Click
        Me.FolderBrowserDialog1.SelectedPath = folderSegmentow
        If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            folderWynikowy = FolderBrowserDialog1.SelectedPath & "\"
        End If



        'wyświetla nazwę kwadratu na pasku stanu i textboxie
        Form1.ToolStripStatusLabel1.Text = folderWynikowy
        TextBox5.Text = folderWynikowy
    End Sub





    Private Sub SKŁADAJWARSTWYToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Directory.Exists(folderWarstwa1) = False Or Directory.Exists(folderWarstwa2) = False Or Directory.Exists(folderWynikowy) = False Then

            MsgBox("Błąd. Upewnij się, czy wskazane wcześniej trzy podkatalogi istnieją i powtórz jeszcze raz operację")
            GoTo errorhandler
        Else

            rozszerzenie = "png"

            'tworzy tablice z plików w folderach dolnej i górnej warstwy
            Dim files1() As String = Directory.GetFiles(folderWarstwa1)

            Dim ileZnakowW1 As Integer  'ile znaków w ścieżkach dostępu do folderów dolnej i górnej warstwy

            Dim ileZnakowFiles1 As Integer 'ile znaków w w nazwie pliku (ale tylko plik, bez ścieżki dostępu)
            Dim png As String               'zmienna służąca do przechowywania ostatnich znaków nazwy pliku w celu sprawdzenia czy to plik PNG
            Dim nazwaPliku As String        'służy do przechowania samej nazwy pliku - bez ścieżki dostępu



            If Dir(folderWarstwa1) = "" Then
                MsgBox("Błąd. Katalog warstwy dolnej nie zawiera plików do połączenia" & Chr(13) & Chr(10) & _
                "Umieść w nim segmenty warstwy podkładowej w formacie png o identycznej nazwie oraz rozdzielczości jak katalogu warstwy górnej"
)
                GoTo errorhandler
            End If
            If Dir(folderWarstwa2) = "" Then
                MsgBox("Błąd. Katalog warstwy górnej nie zawiera plików do połączenia" & Chr(13) & Chr(10) & _
                "Umieść w nich segmenty nakładanej wartstwy w formacie png o identycznej nazwie oraz rozdzielczości jak katalogu warstwy dolnej"
)
                GoTo errorhandler
            End If
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
                        'Using zwalnia pamięć i blokady plików po każdym segmencie (dawniej obrazy nie były zwalniane,
                        'co przy większej liczbie segmentów kończyło się brakiem pamięci). Dawny, niewidoczny PictureBox
                        'służył jedynie do zapisu obrazu - zapis odbywa się teraz bezpośrednio.
                        Try
                            Using warstwa1Plik As New Bitmap(folderWarstwa1 & nazwaPliku), warstwa2 As New Bitmap(folderWarstwa2 & nazwaPliku)  'dolna i górna warstwa
                                'kopia dolnej warstwy w formacie 32-bitowym - na obrazach z paletą barw (np. png8) nie da się rysować
                                Using warstwa1 As New Bitmap(warstwa1Plik.Width, warstwa1Plik.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                                    warstwa2.MakeTransparent(Color.White)
                                    Using g As Graphics = Graphics.FromImage(warstwa1)
                                        g.DrawImage(warstwa1Plik, 0, 0, warstwa1Plik.Width, warstwa1Plik.Height)
                                        g.DrawImage(warstwa2, 0, 0, warstwa2.Width, warstwa2.Height)
                                    End Using
                                    warstwa1.Save(folderWynikowy & nazwaPliku, System.Drawing.Imaging.ImageFormat.Png)
                                End Using
                            End Using
                        Catch
                            'segment, którego nie da się odczytać, jest pomijany (jak dawniej przy On Error Resume Next)
                        End Try



                    End If

                End If
            Next


            MsgBox("Pliki zostały zapisane", MsgBoxStyle.Information, "Infromacja")

            Form1.ToolStripProgressBar1.Value = 0
            Process.Start(folderWynikowy)

errorhandler:
        End If
    End Sub


End Class