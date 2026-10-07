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

Imports System.Drawing.Imaging
Imports System.IO

''' <summary>
''' Nakładanie dwóch zbiorów segmentów PNG (np. kontury działek na ortofotomapę): białe piksele warstwy górnej
''' stają się przezroczyste.
''' </summary>
Public Class Nakladanie_Map

    Private Sub Nakladanie_Map_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Location = New Point(200, 150)

        'foldery zapamiętane w lastsettings.txt, a gdy ich brak - domyślne podkatalogi folderu download
        txtFolderDolna.Text = If(Ustawienia.FolderWarstwy1 <> "", Ustawienia.FolderWarstwy1, FolderDownload & "dolna\")
        txtFolderGorna.Text = If(Ustawienia.FolderWarstwy2 <> "", Ustawienia.FolderWarstwy2, FolderDownload & "gorna\")
        txtFolderWynik.Text = If(Ustawienia.FolderWynikowy <> "", Ustawienia.FolderWynikowy, FolderDownload & "polaczone\")
    End Sub

    'foldery zapisywane przy zamykaniu programu w lastsettings.txt
    Private Sub txtFolderDolna_TextChanged(sender As Object, e As EventArgs) Handles txtFolderDolna.TextChanged
        Ustawienia.FolderWarstwy1 = txtFolderDolna.Text
    End Sub

    Private Sub txtFolderGorna_TextChanged(sender As Object, e As EventArgs) Handles txtFolderGorna.TextChanged
        Ustawienia.FolderWarstwy2 = txtFolderGorna.Text
    End Sub

    Private Sub txtFolderWynik_TextChanged(sender As Object, e As EventArgs) Handles txtFolderWynik.TextChanged
        Ustawienia.FolderWynikowy = txtFolderWynik.Text
    End Sub

    ''' <summary>
    ''' Wybór folderu warstwy. Wskazany folder staje się też folderem sesji okna głównego - dzięki temu
    ''' kolejne pobieranie zapisze segmenty właśnie do niego (np. najpierw warstwa dolna, potem górna).
    ''' </summary>
    Private Function WybierzFolder(biezacy As String) As String
        dlgFolder.SelectedPath = If(Directory.Exists(biezacy), biezacy, Form1.FolderSesji)
        If dlgFolder.ShowDialog = DialogResult.OK Then
            Dim folder As String = dlgFolder.SelectedPath & "\"
            Form1.FolderSesji = folder
            Return folder
        End If
        Return biezacy
    End Function

    Private Sub ButtonDolna_Click(sender As Object, e As EventArgs) Handles ButtonDolna.Click
        txtFolderDolna.Text = WybierzFolder(txtFolderDolna.Text)
    End Sub

    Private Sub ButtonGorna_Click(sender As Object, e As EventArgs) Handles ButtonGorna.Click
        txtFolderGorna.Text = WybierzFolder(txtFolderGorna.Text)
    End Sub

    Private Sub ButtonPolaczone_Click(sender As Object, e As EventArgs) Handles ButtonPolaczone.Click
        txtFolderWynik.Text = WybierzFolder(txtFolderWynik.Text)
    End Sub

    Private Async Sub btnSkladaj_Click(sender As Object, e As EventArgs) Handles btnSkladaj.Click
        Dim dolna As String = KoniecUkosnik(txtFolderDolna.Text)
        Dim gorna As String = KoniecUkosnik(txtFolderGorna.Text)
        Dim wynik As String = KoniecUkosnik(txtFolderWynik.Text)

        If Not Directory.Exists(dolna) OrElse Not Directory.Exists(gorna) OrElse Not Directory.Exists(wynik) Then
            MsgBox("Błąd. Upewnij się, czy wskazane wcześniej trzy podkatalogi istnieją i powtórz jeszcze raz operację")
            Exit Sub
        End If
        If String.Equals(dolna, gorna, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(dolna, wynik, StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(gorna, wynik, StringComparison.OrdinalIgnoreCase) Then
            MsgBox("Folder warstwy dolnej:" & vbCrLf & dolna & vbCrLf & "Folder warstwy górnej:" & vbCrLf & gorna & vbCrLf &
                   "Folder warstw połączonych:" & vbCrLf & wynik & vbCrLf & vbCrLf & "Wybierz trzy różne foldery.")
            Exit Sub
        End If
        If Directory.GetFiles(dolna, "*.png").Length = 0 Then
            MsgBox("Błąd. Katalog warstwy dolnej nie zawiera plików do połączenia" & vbCrLf &
                   "Umieść w nim segmenty warstwy podkładowej w formacie png o identycznej nazwie oraz rozdzielczości jak katalogu warstwy górnej")
            Exit Sub
        End If
        If Directory.GetFiles(gorna, "*.png").Length = 0 Then
            MsgBox("Błąd. Katalog warstwy górnej nie zawiera plików do połączenia" & vbCrLf &
                   "Umieść w nich segmenty nakładanej wartstwy w formacie png o identycznej nazwie oraz rozdzielczości jak katalogu warstwy dolnej")
            Exit Sub
        End If

        btnSkladaj.Enabled = False
        Me.UseWaitCursor = True
        Dim liczba As Integer
        Try
            liczba = Await Task.Run(Function() NalozWarstwy(dolna, gorna, wynik))
        Finally
            btnSkladaj.Enabled = True
            Me.UseWaitCursor = False
        End Try

        MsgBox("Pliki zostały zapisane (" & liczba & ")", MsgBoxStyle.Information, "Informacja")
        OtworzFolder(wynik)
    End Sub

    Private Shared Function KoniecUkosnik(folder As String) As String
        Dim f As String = folder.Trim()
        If f <> "" AndAlso Not f.EndsWith("\") Then f &= "\"
        Return f
    End Function

    ''' <summary>Nakłada segmenty o tych samych nazwach; zwraca liczbę zapisanych plików.</summary>
    Private Shared Function NalozWarstwy(dolna As String, gorna As String, wynik As String) As Integer
        Dim liczba As Integer = 0
        For Each plikDolny In Directory.GetFiles(dolna, "*.png")
            Dim nazwaPliku As String = Path.GetFileName(plikDolny)
            If File.Exists(gorna & nazwaPliku) = False Then Continue For
            'Using zwalnia pamięć i blokady plików po każdym segmencie; kopia dolnej warstwy w formacie 32-bitowym,
            'bo na obrazach z paletą barw (np. png8) nie da się rysować
            Try
                Using warstwa1Plik As New Bitmap(plikDolny), warstwa2 As New Bitmap(gorna & nazwaPliku)
                    Using warstwa1 As New Bitmap(warstwa1Plik.Width, warstwa1Plik.Height, PixelFormat.Format32bppArgb)
                        warstwa2.MakeTransparent(Color.White)
                        Using g As Graphics = Graphics.FromImage(warstwa1)
                            g.DrawImage(warstwa1Plik, 0, 0, warstwa1Plik.Width, warstwa1Plik.Height)
                            g.DrawImage(warstwa2, 0, 0, warstwa2.Width, warstwa2.Height)
                        End Using
                        warstwa1.Save(wynik & nazwaPliku, ImageFormat.Png)
                        liczba += 1
                    End Using
                End Using
            Catch
                'segment, którego nie da się odczytać, jest pomijany
            End Try
        Next
        Return liczba
    End Function

End Class
