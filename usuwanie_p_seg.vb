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

''' <summary>Usuwanie pustych (małych) segmentów wraz z ich plikami georeferencyjnymi.</summary>
Public Class Usuwanie_p_seg

    ''' <summary>Folder segmentów bieżącej sesji.</summary>
    Private ReadOnly Property Folder As String
        Get
            Return Form1.FolderSesji
        End Get
    End Property

    ''' <summary>Pliki segmentów o wybranym rozszerzeniu.</summary>
    Private Function PlikiSegmentow() As List(Of String)
        Dim wynik As New List(Of String)
        Dim rozszerzenie As String = cmbRozszerzenie.Text
        If rozszerzenie = "" OrElse Directory.Exists(Folder) = False Then Return wynik
        For Each plik In Directory.GetFiles(Folder)
            If plik.EndsWith(rozszerzenie, StringComparison.OrdinalIgnoreCase) Then wynik.Add(plik)
        Next
        Return wynik
    End Function

    Private Sub cmbRozszerzenie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbRozszerzenie.SelectedIndexChanged
        txtRozmiarMin.Text = ""
        txtRozmiarMax.Text = ""
        txtRozmiarSredni.Text = ""
        txtDoUsuniecia.Text = ""
        txtLiczbaPlikow.Text = ""
        lblLiczbaPlikow.Text = "Liczba wszystkich plików " & cmbRozszerzenie.Text

        Dim pliki = PlikiSegmentow()
        If pliki.Count = 0 Then Exit Sub

        Dim rozmiarMin As Long = Long.MaxValue, rozmiarMax As Long = 0, suma As Double = 0
        For Each plik In pliki
            Dim rozmiar As Long = New FileInfo(plik).Length
            suma += rozmiar
            rozmiarMin = Math.Min(rozmiarMin, rozmiar)
            rozmiarMax = Math.Max(rozmiarMax, rozmiar)
        Next

        txtRozmiarMin.Text = (rozmiarMin \ 1024).ToString() & " kB"
        txtRozmiarMax.Text = (rozmiarMax \ 1024).ToString() & " kB"
        txtRozmiarSredni.Text = Math.Floor(suma / pliki.Count / 1024).ToString() & " kB"
        txtLiczbaPlikow.Text = pliki.Count.ToString()
        txtDoUsuniecia.Text = PlikiPonizejRozmiaru().Count.ToString()
    End Sub

    Private Sub btnUsun_Click(sender As Object, e As EventArgs) Handles btnUsun.Click
        Dim doUsuniecia As New List(Of String)
        btnUsun.Enabled = False

        For Each plik As String In PlikiPonizejRozmiaru()
            doUsuniecia.Add(plik)
            'pliki georeferencyjne towarzyszące segmentowi (jeśli istnieją)
            Dim bezRozszerzenia As String = plik.Substring(0, plik.Length - cmbRozszerzenie.Text.Length - 1)
            For Each towarzyszacy As String In {".map", ".gmi", ".wld", ".tab", ".kml", ".prj",
                                                "." & cmbRozszerzenie.Text & ".points", ".jpgw", ".pngw", ".tifw", ".gifw"}
                If File.Exists(bezRozszerzenia & towarzyszacy) Then doUsuniecia.Add(bezRozszerzenia & towarzyszacy)
            Next
        Next

        Dim usuniete As Integer = 0
        For Each plik As String In doUsuniecia
            Try
                File.Delete(plik)
                usuniete += 1
            Catch
            End Try
        Next

        txtProgKB.Text = "0"
        txtDoUsuniecia.Text = usuniete.ToString()
        btnUsun.Enabled = True
    End Sub

    Private Sub txtProgKB_TextChanged(sender As Object, e As EventArgs) Handles txtProgKB.TextChanged
        lblLiczbaPlikow.Text = "Liczba wszystkich plików " & cmbRozszerzenie.Text
        txtDoUsuniecia.Text = PlikiPonizejRozmiaru().Count.ToString()
    End Sub

    ''' <summary>Segmenty o wybranym rozszerzeniu, mniejsze niż próg podany w kB.</summary>
    Private Function PlikiPonizejRozmiaru() As List(Of String)
        Dim progKB As Double = Wartosc(txtProgKB.Text)
        Return PlikiSegmentow().FindAll(Function(plik) Math.Floor(New FileInfo(plik).Length / 1024) < progKB)
    End Function

End Class
