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

Public Class usuwanie_p_seg
    Private Property sredniRozmiar As String
    Private Property TestRozszerzenia As String


    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged

        Dim Plik As String
        Dim Rozmiar As Single
        Dim rozmiarMin As Single
        Dim rozmiarMax As Single
        Dim rozmiarSum As Double
        Dim ilePlikow As Long

        ComboBox1.Enabled = False

        TextBox1.Text = ""
        TextBox2.Text = ""
        TextBox3.Text = ""
        TextBox6.Text = ""

        'przegląda wszystkie pliki w wybranym folderze i poddaje je odpowiednim czynnościom
        Dim files() As String = Directory.GetFiles(folderSegmentow)
        For Each Plik In files

            Application.DoEvents()

            'sprawdza czy rozszerzenie pliku jest zgodne z zadanym
            TestRozszerzenia = Microsoft.VisualBasic.Right(Plik, Len(ComboBox1.Text))

            If TestRozszerzenia = ComboBox1.Text Then

                ilePlikow = ilePlikow + 1                               'zlicza pliki

                Rozmiar = FileLen(Plik)                                 'sprawdza rozmiar pliku
                rozmiarSum = rozmiarSum + Rozmiar                       'sumuje rozmiary wszystkich plików


                Select Case ilePlikow
                    Case 1
                        rozmiarMax = Rozmiar                                'jeśli to pierwszy plik wtedy jego wielkość przypisywana jest też do rozmiaru Min i Max
                        rozmiarMin = Rozmiar
                    Case Else
                        If Rozmiar > rozmiarMax Then rozmiarMax = Rozmiar 'potem Min i Max przypisywane są po porównaniu z bieżącym plikiem
                        If Rozmiar < rozmiarMin Then rozmiarMin = Rozmiar
                End Select

            End If

        Next

        If ilePlikow = 0 Then GoTo errorhandler

        rozmiarMin = Int(rozmiarMin / 1024)
        rozmiarMax = Int(rozmiarMax / 1024)
        sredniRozmiar = Int((rozmiarSum / ilePlikow) / 1024)      'oblicza średni rozmiar pliku w kB


        TextBox1.Text = rozmiarMin & " kB"
        TextBox2.Text = rozmiarMax & " kB"
        TextBox3.Text = sredniRozmiar & " kB"
        TextBox6.Text = ilePlikow


errorhandler:
        ComboBox1.Enabled = True
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim Rozmiar As Single
        Dim Plik As String
        Dim toDelete(10000) As String          'pliki do usunięcie
        Dim ileDelete As Long                       'ile plików do usunięcia

        Button1.Enabled = False

        'przegląda wszystkie pliki w wybranym folderze i poddaje je odpowiednim czynnościom
        Dim files() As String = Directory.GetFiles(folderSegmentow)
        For Each Plik In files

            Application.DoEvents()

            'sprawdza czy rozszerzenie pliku jest zgodne z zadanym
            TestRozszerzenia = Microsoft.VisualBasic.Right(Plik, Len(ComboBox1.Text))

            Rozmiar = Int(FileLen(Plik) / 1024)                                'sprawdza rozmiar pliku

            If TestRozszerzenia = ComboBox1.Text And Rozmiar < TextBox4.Text Then

                ileDelete = ileDelete + 1           'jeśli warunek spełniony to zwiększa się ileDelete
                '  ReDim toDelete(ileDelete)           'i zwiększa się rozmiar tablicy na pliki do usunięcia
                toDelete(ileDelete - 1) = Plik      'po czym plik jest umieszczany w tablicy



                'to samo co powyżej, ale dla pliku .map jeśli istnieje
                Plik = (Replace(Plik, "." & TestRozszerzenia, ".map"))
                If Dir(Plik) <> "" Then
                    ileDelete = ileDelete + 1
                    '         ReDim toDelete(ileDelete)
                    toDelete(ileDelete - 1) = Plik
                End If


                'to samo co powyżej, ale dla pliku .gmi jeśli istnieje
                Plik = (Replace(Plik, ".map", ".gmi"))
                If Dir(Plik) <> "" Then
                    ileDelete = ileDelete + 1
                    '         ReDim toDelete(ileDelete)
                    toDelete(ileDelete - 1) = Plik
                End If


                'to samo co powyżej, ale dla pliku .wld jeśli istnieje
                Plik = (Replace(Plik, ".gmi", ".wld"))
                If Dir(Plik) <> "" Then
                    ileDelete = ileDelete + 1
                    '         ReDim toDelete(ileDelete)
                    toDelete(ileDelete - 1) = Plik
                End If


                'to samo co powyżej, ale dla pliku .jpg.points jeśli istnieje
                Plik = (Replace(Plik, ".wld", ".jpg.points"))
                If Dir(Plik) <> "" Then
                    ileDelete = ileDelete + 1
                    '          ReDim toDelete(ileDelete)
                    toDelete(ileDelete - 1) = Plik
                End If


                'to samo co powyżej, ale dla pliku .jpgw jeśli istnieje
                Plik = (Replace(Plik, ".jpg.points", ".jpgw"))
                If Dir(Plik) <> "" Then
                    ileDelete = ileDelete + 1
                    '          ReDim toDelete(ileDelete)
                    toDelete(ileDelete - 1) = Plik
                End If
            End If

        Next


        For i = 0 To ileDelete - 1

            File.Delete(toDelete(i))

        Next


        TextBox4.Text = 0

        Button1.Enabled = True


    End Sub
End Class