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
Imports MapoTero.Core

''' <summary>Usuwanie pustych (małych) segmentów wraz z ich plikami georeferencyjnymi.</summary>
Public Class Usuwanie_p_seg

    Private ReadOnly _plikiBiezace As New List(Of FileInfo)()

    ''' <summary>Tytuł okna z projektu (do niego dopisywany jest folder segmentów).</summary>
    Private _tytul As String = ""

    Private Sub Usuwanie_p_seg_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.ZamykanieKlawiszemEsc(Me, rozmiarMinimalny:=True)
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.UsuwaniePustych, 140, 287)
        DodajPrzyciskFolderu()
        If _tytul = "" Then _tytul = Me.Text
        'bez bieżącej sesji okno nie przechodzi na inną (np. najnowszą) - kasuje pliki, więc folder wskazuje użytkownik
        'przyciskiem „Folder…” (dawniej okno działało bez ostrzeżenia na najnowszej sesji w download)
        UstawFolder(FolderBiezacejSesji(Form1.FolderSesji))
    End Sub

    ''' <summary>Ustawia folder segmentów (pusty - brak bieżącej sesji: przycisk Usuń nieaktywny) i odświeża statystykę.</summary>
    Private Sub UstawFolder(folder As String)
        _folder = folder
        btnUsun.Enabled = _folder <> ""
        Me.Text = _tytul & " - " & If(_folder <> "", _folder, "brak bieżącej sesji, wskaż folder przyciskiem Folder…")
        ToolTip1.SetToolTip(_btnFolder, If(_folder <> "", "folder segmentów: " & _folder & " - kliknij, aby wskazać inny",
                                           "Nie ma bieżącej sesji pobierania - wskaż folder z segmentami"))
        cmbRozszerzenie_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    ''' <summary>Przycisk wyboru folderu segmentów (obok listy rozszerzeń, położenie z projektu skalowane jak okno).</summary>
    Private _btnFolder As Button

    Private Sub DodajPrzyciskFolderu()
        If _btnFolder IsNot Nothing Then Exit Sub
        Dim skala = PomocProgramu.SkalaProjektanta(Me)
        _btnFolder = New Button With {
            .Name = "btnFolder", .Text = "Folder…", .UseVisualStyleBackColor = True,
            .Location = New Point(CInt(229 * skala.Width), cmbRozszerzenie.Top - 1),
            .Size = New Size(CInt(84 * skala.Width), cmbRozszerzenie.Height + 2)}
        Controls.Add(_btnFolder)
        AddHandler _btnFolder.Click, AddressOf btnFolder_Click
    End Sub

    Private Sub btnFolder_Click(sender As Object, e As EventArgs)
        dlgFolder.Description = "Folder z segmentami do sprawdzenia"
        dlgFolder.SelectedPath = If(_folder <> "" AndAlso Directory.Exists(_folder), _folder, FolderDownload)
        If dlgFolder.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        UstawFolder(dlgFolder.SelectedPath.TrimEnd("\"c, "/"c) & "\")
    End Sub

    ''' <summary>Folder segmentów wybrany przy otwarciu okna (bieżąca sesja albo folder wskazany przez użytkownika).</summary>
    Private _folder As String = ""

    ''' <summary>
    ''' Folder bieżącej sesji, z którego okno może usuwać pliki: istniejący folder inny niż sam download (folder sesji
    ''' albo np. folder segmentów bez conf.txt); pusty tekst - brak bieżącej sesji. Dawniej okno przechodziło wtedy
    ''' bez ostrzeżenia na najnowszą sesję w download i usuwało jej pliki.
    ''' </summary>
    Friend Shared Function FolderBiezacejSesji(folderSesji As String) As String
        If String.IsNullOrWhiteSpace(folderSesji) OrElse Not Directory.Exists(folderSesji) Then Return ""
        If String.Equals(Path.GetFullPath(folderSesji).TrimEnd("\"c, "/"c), Path.GetFullPath(FolderDownload).TrimEnd("\"c, "/"c),
                         StringComparison.OrdinalIgnoreCase) Then Return ""
        Return folderSesji.TrimEnd("\"c, "/"c) & "\"
    End Function

    ''' <summary>Folder segmentów, na którym działa okno.</summary>
    Private ReadOnly Property Folder As String
        Get
            Return _folder
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

        'do Kosza - pomyłkę przy wpisywaniu progu można cofnąć; paczkami (pojedynczo ok. 70 ms na plik)
        Cursor = Cursors.WaitCursor
        Try
            PrzeniesDoKosza(doUsuniecia)
        Finally
            Cursor = Cursors.Default
        End Try
        Dim bledy As Integer = doUsuniecia.Where(Function(p) File.Exists(p)).Count()
        Dim usunieteSegmenty As Integer = segmenty.Where(Function(p) Not File.Exists(p)).Count()
        Dim usuniete As Integer = doUsuniecia.Count - bledy

        txtProgKB.Text = "0"
        btnUsun.Enabled = True
        cmbRozszerzenie_SelectedIndexChanged(Nothing, Nothing)

        Dim opis As String = "Przeniesiono do Kosza " & usunieteSegmenty & " " & Odmiana(usunieteSegmenty, "segment", "segmenty", "segmentów")
        If usuniete > usunieteSegmenty Then opis &= " i " & (usuniete - usunieteSegmenty) & " " & Odmiana(usuniete - usunieteSegmenty, "plik", "pliki", "plików") & " georeferencyjnych"
        opis &= "."
        If doUsuniecia.Count > MaksPlikowPojedynczo Then opis &= " W Koszu są w folderze """ & NazwaFolderuKosza & " (data i godzina)""."
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
