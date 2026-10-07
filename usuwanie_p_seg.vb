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

Public Class Usuwanie_p_seg
    Private Property SredniRozmiar As String
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
        TextBox5.Text = ""
        TextBox6.Text = ""
        Label9.Text = "Liczba wszystkich plików " & ComboBox1.Text
        'przegląda wszystkie pliki w wybranym folderze i poddaje je odpowiednim czynnościom
        Dim files() As String = Directory.GetFiles(folderSegmentow)
        For Each Plik In files

            Application.DoEvents()

            'sprawdza czy rozszerzenie pliku jest zgodne z zadanym
            TestRozszerzenia = Microsoft.VisualBasic.Right(Plik, Len(ComboBox1.Text))

            If TestRozszerzenia = ComboBox1.Text Then

                ilePlikow += 1                               'zlicza pliki

                Rozmiar = FileLen(Plik)                                 'sprawdza rozmiar pliku
                rozmiarSum += Rozmiar                       'sumuje rozmiary wszystkich plików


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
        SredniRozmiar = Int((rozmiarSum / ilePlikow) / 1024)      'oblicza średni rozmiar pliku w kB


        TextBox1.Text = rozmiarMin & " kB"
        TextBox2.Text = rozmiarMax & " kB"
        TextBox3.Text = SredniRozmiar & " kB"
        TextBox6.Text = ilePlikow


errorhandler:
        ComboBox1.Enabled = True
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim toDelete As New List(Of String)          'pliki do usunięcia (lista - dawniej tablica ograniczona do 10 001 plików)

        Button1.Enabled = False

        'przegląda wszystkie pliki w wybranym folderze i poddaje je odpowiednim czynnościom
        For Each Plik As String In PlikiPonizejRozmiaru()

            toDelete.Add(Plik)

            'pliki georeferencyjne towarzyszące segmentowi (jeśli istnieją)
            Dim bezRozszerzenia As String = Plik.Substring(0, Plik.Length - Len(ComboBox1.Text) - 1)
            For Each towarzyszacy As String In {".map", ".gmi", ".wld", ".tab", ".kml",
                                                "." & ComboBox1.Text & ".points", ".jpgw", ".pngw", ".tifw", ".gifw"}
                If File.Exists(bezRozszerzenia & towarzyszacy) Then toDelete.Add(bezRozszerzenia & towarzyszacy)
            Next

        Next


        For Each plikDoUsuniecia As String In toDelete

            File.Delete(plikDoUsuniecia)

        Next


        TextBox4.Text = 0
        TextBox5.Text = toDelete.Count
        Button1.Enabled = True


    End Sub




    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged
        Label9.Text = "Liczba wszystkich plików " & ComboBox1.Text
        Button1.Enabled = False

        TextBox5.Text = PlikiPonizejRozmiaru().Count
        Button1.Enabled = True


    End Sub

    'segmenty o wybranym rozszerzeniu, mniejsze niż rozmiar podany w TextBox4 (w kB)
    Private Function PlikiPonizejRozmiaru() As List(Of String)

        Dim wynik As New List(Of String)
        Dim Rozmiar As Single
        Dim progKB As Double = Val(TextBox4.Text)   'Val - puste lub błędne pole nie powoduje już błędu programu

        If ComboBox1.Text = "" Or Directory.Exists(folderSegmentow) = False Then Return wynik

        'przegląda wszystkie pliki w wybranym folderze i poddaje je odpowiednim czynnościom
        Dim files() As String = Directory.GetFiles(folderSegmentow)
        For Each Plik As String In files

            Application.DoEvents()

            'sprawdza czy rozszerzenie pliku jest zgodne z zadanym
            TestRozszerzenia = Microsoft.VisualBasic.Right(Plik, Len(ComboBox1.Text))

            Rozmiar = Int(FileLen(Plik) / 1024)                                'sprawdza rozmiar pliku

            If TestRozszerzenia = ComboBox1.Text And Rozmiar < progKB Then
                wynik.Add(Plik)
            End If

        Next

        Return wynik
    End Function
End Class