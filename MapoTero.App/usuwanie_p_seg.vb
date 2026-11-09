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
        Dim segmenty = PlikiPonizejRozmiaru()
        If segmenty.Count = 0 Then
            MsgBox("Brak segmentów mniejszych niż podany rozmiar - wpisz próg w polu ""Usuń kafle mniejsze niż"".", MsgBoxStyle.Information, "Usuwanie pustych segmentów")
            Exit Sub
        End If
        If MessageBox.Show("Usunąć " & segmenty.Count & " z " & _plikiBiezace.Count & " segmentów mniejszych niż " & txtProgKB.Text.Trim() &
                           " kB (wraz z ich plikami georeferencyjnymi) z folderu" & vbCrLf & Folder & "?" & vbCrLf & vbCrLf &
                           "Pliki zostaną przeniesione do Kosza.", "Usuwanie pustych segmentów",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Exit Sub
        btnUsun.Enabled = False

        For Each plik As String In segmenty
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

        'do Kosza - pomyłkę przy wpisywaniu progu można cofnąć
        Dim usuniete As Integer = 0, usunieteSegmenty As Integer = 0, bledy As Integer = 0
        Dim zbiorSegmentow As New HashSet(Of String)(segmenty, StringComparer.OrdinalIgnoreCase)
        For Each plik As String In doUsuniecia
            Try
                FileIO.FileSystem.DeleteFile(plik, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
                usuniete += 1
                If zbiorSegmentow.Contains(plik) Then usunieteSegmenty += 1
            Catch
                bledy += 1
            End Try
        Next

        txtProgKB.Text = "0"
        btnUsun.Enabled = True
        cmbRozszerzenie_SelectedIndexChanged(Nothing, Nothing)

        Dim opis As String = "Przeniesiono do Kosza " & usunieteSegmenty & " " & Odmiana(usunieteSegmenty, "segment", "segmenty", "segmentów")
        If usuniete > usunieteSegmenty Then opis &= " i " & (usuniete - usunieteSegmenty) & " " & Odmiana(usuniete - usunieteSegmenty, "plik", "pliki", "plików") & " georeferencyjnych"
        opis &= "."
        If bledy > 0 Then opis &= vbCrLf & "Nie udało się usunąć " & bledy & " " & Odmiana(bledy, "pliku", "plików", "plików") & "."
        opis &= vbCrLf & vbCrLf & "Brakujące segmenty uzupełnisz, wznawiając pobieranie przyciskiem Pobierz."
        MsgBox(opis, If(bledy > 0, MsgBoxStyle.Exclamation, MsgBoxStyle.Information), "Usuwanie pustych segmentów")
        Form1.Komunikat(opis.Replace(vbCrLf, " "), If(bledy > 0, Color.Red, Color.Green))
    End Sub

    Private Sub txtProgKB_TextChanged(sender As Object, e As EventArgs) Handles txtProgKB.TextChanged
        lblLiczbaPlikow.Text = "Liczba wszystkich plików " & cmbRozszerzenie.Text
        txtDoUsuniecia.Text = PlikiPonizejRozmiaru().Count.ToString()
    End Sub

    ''' <summary>Segmenty o wybranym rozszerzeniu, mniejsze niż próg podany w kB.</summary>
    Private Function PlikiPonizejRozmiaru() As List(Of String)
        Dim progKB As Double = Wartosc(txtProgKB.Text)
        If progKB <= 0 Then Return New List(Of String)()
        'bardzo duża wartość (np. 1e20) przekraczała zakres Long
        Dim progBajtow As Long = CLng(Math.Min(1.0E+18, Math.Round(progKB * 1024.0)))
        Dim wynik As New List(Of String)()
        For Each fi In _plikiBiezace
            If fi.Length < progBajtow Then
                wynik.Add(fi.FullName)
            End If
        Next
        Return wynik
    End Function

End Class
