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

    Private ReadOnly _plikiBiezace As New List(Of FileInfo)()

    Private Sub Usuwanie_p_seg_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbRozszerzenie_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    ''' <summary>Folder segmentów bieżącej sesji.</summary>
    Private ReadOnly Property Folder As String
        Get
            Return Form1.FolderSesji
        End Get
    End Property

    ''' <summary>Odświeża listę plików segmentów w pamięci podręcznej.</summary>
    Private Sub OdswiezPlikiSegmentow()
        _plikiBiezace.Clear()
        Dim rozszerzenie As String = cmbRozszerzenie.Text.TrimStart("."c)
        If String.IsNullOrWhiteSpace(rozszerzenie) OrElse Not Directory.Exists(Folder) Then Exit Sub
        Try
            For Each plik In Directory.GetFiles(Folder)
                Dim ext As String = Path.GetExtension(plik).TrimStart("."c)
                If ext.Equals(rozszerzenie, StringComparison.OrdinalIgnoreCase) Then
                    _plikiBiezace.Add(New FileInfo(plik))
                End If
            Next
        Catch
        End Try
    End Sub

    Private Sub cmbRozszerzenie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbRozszerzenie.SelectedIndexChanged
        txtRozmiarMin.Text = ""
        txtRozmiarMax.Text = ""
        txtRozmiarSredni.Text = ""
        txtDoUsuniecia.Text = ""
        txtLiczbaPlikow.Text = ""
        lblLiczbaPlikow.Text = "Liczba wszystkich plików " & cmbRozszerzenie.Text

        OdswiezPlikiSegmentow()
        If _plikiBiezace.Count = 0 Then Exit Sub

        Dim rozmiarMin As Long = Long.MaxValue, rozmiarMax As Long = 0, suma As Double = 0
        For Each fi In _plikiBiezace
            Dim rozmiar As Long = fi.Length
            suma += rozmiar
            rozmiarMin = Math.Min(rozmiarMin, rozmiar)
            rozmiarMax = Math.Max(rozmiarMax, rozmiar)
        Next

        txtRozmiarMin.Text = (rozmiarMin \ 1024).ToString() & " kB"
        txtRozmiarMax.Text = (rozmiarMax \ 1024).ToString() & " kB"
        txtRozmiarSredni.Text = Math.Floor(suma / _plikiBiezace.Count / 1024).ToString() & " kB"
        txtLiczbaPlikow.Text = _plikiBiezace.Count.ToString()
        txtDoUsuniecia.Text = PlikiPonizejRozmiaru().Count.ToString()
    End Sub

    Private Sub btnUsun_Click(sender As Object, e As EventArgs) Handles btnUsun.Click
        Dim doUsuniecia As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        btnUsun.Enabled = False

        For Each plik As String In PlikiPonizejRozmiaru()
            doUsuniecia.Add(plik)
            'pliki georeferencyjne towarzyszące segmentowi (jeśli istnieją)
            Dim bezRozszerzenia As String = Path.ChangeExtension(plik, Nothing)
            Dim ext As String = Path.GetExtension(plik).TrimStart("."c)
            For Each towarzyszacy As String In {".map", ".gmi", ".wld", ".tab", ".kml", ".prj",
                                                "." & ext & ".points", ".jpgw", ".pngw", ".tifw", ".gifw", ".tfw", ".jgw", ".pgw"}
                Dim sciezkaTowarzyszaca As String = bezRozszerzenia & towarzyszacy
                If File.Exists(sciezkaTowarzyszaca) Then
                    doUsuniecia.Add(sciezkaTowarzyszaca)
                End If
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
        cmbRozszerzenie_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub txtProgKB_TextChanged(sender As Object, e As EventArgs) Handles txtProgKB.TextChanged
        lblLiczbaPlikow.Text = "Liczba wszystkich plików " & cmbRozszerzenie.Text
        txtDoUsuniecia.Text = PlikiPonizejRozmiaru().Count.ToString()
    End Sub

    ''' <summary>Segmenty o wybranym rozszerzeniu, mniejsze niż próg podany w kB.</summary>
    Private Function PlikiPonizejRozmiaru() As List(Of String)
        Dim progKB As Double = Wartosc(txtProgKB.Text)
        If progKB <= 0 Then Return New List(Of String)()
        Dim progBajtow As Long = CLng(Math.Round(progKB * 1024.0))
        Dim wynik As New List(Of String)()
        For Each fi In _plikiBiezace
            If fi.Length < progBajtow Then
                wynik.Add(fi.FullName)
            End If
        Next
        Return wynik
    End Function

End Class
