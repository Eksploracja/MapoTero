'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.ComponentModel
Imports System.Data
Imports System.Data.SQLite
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

Public Enum FormatKonwersjiOzi
    Jnx
    Kmz
    Mbtiles
End Enum

Public Class RaportPostepuWsadowego
    Public Property IndeksPliku As Integer
    Public Property LaczniePlikow As Integer
    Public Property NazwaPliku As String = ""
    Public Property ProcentPliku As Integer
    Public Property ProcentCalkowity As Integer
    Public Property StatusTekst As String = ""
End Class

Public Class WynikKonwersjiWsadowej
    Public Property LaczniePlikow As Integer
    Public Property Sukcesy As Integer
    Public Property Pominiete As Integer
    Public Property Bledy As New List(Of String)()
    Public Property WygenerowanePliki As New List(Of String)()
End Class

Public Class FormImportOzi

    Private _ozi As PlikKalibracjiOzi
    Private _anulowanie As CancellationTokenSource
    Private _plikiWsadowe As New List(Of String)()
    Private _czyTrybWsadowy As Boolean = False
    ''' <summary>Okno zamknięte w trakcie konwersji - zamyka się po jej przerwaniu, bez komunikatów o wyniku.</summary>
    Private _zamknijPoZakonczeniu As Boolean

    Public Sub New()
        InitializeComponent()
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.ImportOzi)
        Me.AllowDrop = True
    End Sub

    ''' <summary>Otwiera formularz i ładuje wskazany plik .map.</summary>
    Public Sub OtworzZPlikiem(sciezkaPlikuMap As String)
        ZaladujPlikMap(sciezkaPlikuMap)
    End Sub

    ''' <summary>Otwiera formularz w trybie wsadowym (hurtowym) z podaną listą plików .map.</summary>
    Public Sub OtworzWsadowo(plikiMap As IEnumerable(Of String))
        ZaladujWsadowo(plikiMap)
    End Sub

    ''' <summary>Wyszukuje wszystkie pliki kalibracji .map w podanym folderze (w bieżącym lub podfolderach).</summary>
    Public Shared Function ZnajdzPlikiMapWFolderze(sciezkaFolderu As String) As List(Of String)
        If String.IsNullOrWhiteSpace(sciezkaFolderu) OrElse Not Directory.Exists(sciezkaFolderu) Then
            Return New List(Of String)()
        End If
        'podfoldery bez dostępu (np. skróty "Moja muzyka" w Dokumentach, "Dane aplikacji") są pomijane -
        'wcześniej wyjątek UnauthorizedAccessException przerywał wyszukiwanie po upuszczeniu takiego folderu na okno
        Dim opcje As New EnumerationOptions With {.IgnoreInaccessible = True, .RecurseSubdirectories = False, .MatchCasing = MatchCasing.CaseInsensitive}
        Dim pliki = Directory.GetFiles(sciezkaFolderu, "*.map", opcje).Where(AddressOf CzyPlikKalibracjiOzi).OrderBy(Function(f) f).ToList()
        If pliki.Count = 0 Then
            opcje.RecurseSubdirectories = True
            pliki = Directory.GetFiles(sciezkaFolderu, "*.map", opcje).Where(AddressOf CzyPlikKalibracjiOzi).OrderBy(Function(f) f).ToList()
        End If
        Return pliki
    End Function

    ''' <summary>
    ''' Czy plik .map jest kalibracją OziExplorera (pierwszy wiersz "OziExplorer Map Data File"). Rozszerzenie .map mają też
    ''' mapy źródeł JavaScript (*.js.map), pliki MapServera i MapInfo - w folderze Dokumenty było ich kilkaset.
    ''' </summary>
    Public Shared Function CzyPlikKalibracjiOzi(sciezka As String) As Boolean
        Try
            Dim bufor(63) As Byte
            Dim n As Integer
            Using fs As New FileStream(sciezka, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                n = fs.Read(bufor, 0, bufor.Length)
            End Using
            Dim poczatek = System.Text.Encoding.UTF8.GetString(bufor, 0, n).TrimStart(ChrW(&HFEFF), " "c)
            Return poczatek.StartsWith("OziExplorer Map Data File", StringComparison.OrdinalIgnoreCase)
        Catch
            Return False
        End Try
    End Function

    Protected Overrides Sub OnDragEnter(drgevent As DragEventArgs)
        MyBase.OnDragEnter(drgevent)
        If drgevent.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = DirectCast(drgevent.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                drgevent.Effect = DragDropEffects.Copy
                Exit Sub
            End If
        End If
        drgevent.Effect = DragDropEffects.None
    End Sub

    Protected Overrides Sub OnDragDrop(drgevent As DragEventArgs)
        MyBase.OnDragDrop(drgevent)
        'w trakcie konwersji przyciski wyboru plików są zablokowane - upuszczenie pliku podmieniało opis konwertowanej mapy
        If _anulowanie IsNot Nothing Then Exit Sub
        If drgevent.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = DirectCast(drgevent.Data.GetData(DataFormats.FileDrop), String())
            If pliki Is Nothing OrElse pliki.Length = 0 Then Exit Sub

            Dim znalezioneMapy As New List(Of String)()
            For Each p In pliki
                If Directory.Exists(p) Then
                    znalezioneMapy.AddRange(ZnajdzPlikiMapWFolderze(p))
                ElseIf String.Equals(Path.GetExtension(p), ".map", StringComparison.OrdinalIgnoreCase) Then
                    znalezioneMapy.Add(p)
                End If
            Next

            If znalezioneMapy.Count > 1 Then
                ZaladujWsadowo(znalezioneMapy)
            ElseIf znalezioneMapy.Count = 1 Then
                ZaladujPlikMap(znalezioneMapy(0))
            ElseIf _ozi IsNot Nothing AndAlso Not _czyTrybWsadowy Then
                Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
                If ext = ".jpg" OrElse ext = ".png" OrElse ext = ".tif" OrElse ext = ".bmp" OrElse ext = ".jpeg" Then
                    UstawPlikObrazu(pliki(0))
                End If
            End If
        End If
    End Sub

    Private Sub btnWybierzMap_Click(sender As Object, e As EventArgs) Handles btnWybierzMap.Click
        If dlgWybierzMap.ShowDialog(Me) = DialogResult.OK Then
            If dlgWybierzMap.FileNames.Length > 1 Then
                ZaladujWsadowo(dlgWybierzMap.FileNames)
            ElseIf dlgWybierzMap.FileNames.Length = 1 Then
                ZaladujPlikMap(dlgWybierzMap.FileNames(0))
            End If
        End If
    End Sub

    Private Sub btnWybierzFolder_Click(sender As Object, e As EventArgs) Handles btnWybierzFolder.Click
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            Dim pliki = ZnajdzPlikiMapWFolderze(dlgFolder.SelectedPath)
            If pliki.Count = 0 Then
                MessageBox.Show("W wybranym folderze nie znaleziono żadnych plików kalibracji OziExplorer (*.map).",
                                "Brak plików kalibracji", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ElseIf pliki.Count = 1 Then
                ZaladujPlikMap(pliki(0))
            Else
                ZaladujWsadowo(pliki)
            End If
        End If
    End Sub

    Private Sub btnWybierzObraz_Click(sender As Object, e As EventArgs) Handles btnWybierzObraz.Click
        If dlgWybierzObraz.ShowDialog(Me) = DialogResult.OK Then
            UstawPlikObrazu(dlgWybierzObraz.FileName)
        End If
    End Sub

    Public Sub ZaladujWsadowo(pliki As IEnumerable(Of String))
        _plikiWsadowe = pliki.Where(Function(p) Not String.IsNullOrWhiteSpace(p)).Distinct().ToList()
        If _plikiWsadowe.Count = 0 Then Exit Sub
        If _plikiWsadowe.Count = 1 Then
            ZaladujPlikMap(_plikiWsadowe(0))
            Exit Sub
        End If

        _czyTrybWsadowy = True
        _ozi = Nothing

        Dim zObrazem = 0
        Dim datums As New HashSet(Of String)()
        For Each p In _plikiWsadowe
            Try
                Dim o = PlikKalibracjiOzi.Wczytaj(p)
                If o.PlikObrazuIstnieje Then zObrazem += 1
                If Not String.IsNullOrEmpty(o.Datum) Then datums.Add(o.Datum)
            Catch
            End Try
        Next

        Dim folderGlowny = Path.GetDirectoryName(_plikiWsadowe(0))
        txtPlikMap.Text = $"Kolejka wsadowa: {_plikiWsadowe.Count} plików .map ({folderGlowny})"
        lblPlikObrazu.Text = "Dopasowanie skanów rastrowych:"
        txtPlikObrazu.Text = $"{zObrazem} z {_plikiWsadowe.Count} plików posiada dopasowany obraz w folderze"
        txtPlikObrazu.ForeColor = If(zObrazem = _plikiWsadowe.Count, SystemColors.WindowText, Color.DarkOrange)
        btnWybierzObraz.Enabled = False

        lblInfoTytul.Text = $"Tryb hurtowy: wybrano {_plikiWsadowe.Count} plików kalibracji .map"
        lblInfoDatum.Text = $"Wykryte układy / datum: {If(datums.Count > 0, String.Join(", ", datums), "WGS 84")}"
        lblInfoWymiary.Text = $"Mapy gotowe do przetworzenia: {zObrazem} (brakujące obrazy: {_plikiWsadowe.Count - zObrazem})"
        lblInfoPunkty.Text = "Każda mapa zostanie automatycznie przekonwertowana z georeferencją"
        lblInfoZasieg.Text = "Folder zapisu zostanie wskazany przed rozpoczęciem konwersji"

        btnKonwertuj.Text = $"Konwertuj hurtowo ({zObrazem})..."
        btnKonwertuj.Enabled = (zObrazem > 0)
        lblStatus.Text = $"Gotowy do konwersji hurtowej {zObrazem} map."
    End Sub

    Public Sub ZaladujPlikMap(sciezka As String)
        _czyTrybWsadowy = False
        _plikiWsadowe.Clear()
        _plikiWsadowe.Add(sciezka)
        lblPlikObrazu.Text = "Plik obrazu (skan mapy JPG, PNG):"
        btnWybierzObraz.Enabled = True
        btnKonwertuj.Text = "Konwertuj i zapisz..."
        Try
            _ozi = PlikKalibracjiOzi.Wczytaj(sciezka)
            txtPlikMap.Text = sciezka
            txtPlikObrazu.Text = _ozi.PlikObrazu
            OdswiezInformacje()
        Catch ex As Exception
            MessageBox.Show("Nie udało się odczytać pliku kalibracji: " & OpisBledu.Z(ex),
                            "Błąd pliku kalibracji", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UstawPlikObrazu(sciezka As String)
        If _ozi Is Nothing Then Exit Sub
        Try
            Dim oziNowy = PlikKalibracjiOzi.Wczytaj(_ozi.SciezkaMap, sciezka)
            _ozi = oziNowy
            txtPlikObrazu.Text = sciezka
            OdswiezInformacje()
        Catch ex As Exception
            MessageBox.Show("Błąd odczytu pliku obrazu: " & OpisBledu.Z(ex), "Błąd obrazu", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OdswiezInformacje()
        If _ozi Is Nothing Then
            lblInfoTytul.Text = "Tytuł:"
            lblInfoDatum.Text = "Datum:"
            lblInfoWymiary.Text = "Wymiary:"
            lblInfoPunkty.Text = "Punkty:"
            lblInfoZasieg.Text = "Zasięg:"
            btnKonwertuj.Enabled = False
            Exit Sub
        End If

        Dim ci = CultureInfo.InvariantCulture
        lblInfoTytul.Text = "Tytuł: " & If(String.IsNullOrEmpty(_ozi.Tytul), Path.GetFileNameWithoutExtension(_ozi.SciezkaMap), _ozi.Tytul)

        Dim korektaTekst = ""
        If Not _ozi.DatumObslugiwane Then
            korektaTekst = " - NIEZNANY: brak korekty, mapa może być przesunięta nawet o kilkaset metrów"
        ElseIf Not _ozi.Datum.ToUpperInvariant().Contains("WGS") Then
            korektaTekst = " (automatyczna korekta Datum do WGS84)"
        End If
        lblInfoDatum.Text = "Układ / Datum: " & _ozi.Datum & korektaTekst
        lblInfoDatum.ForeColor = If(_ozi.DatumObslugiwane, SystemColors.ControlText, Color.DarkRed)

        If _ozi.PlikObrazuIstnieje Then
            txtPlikObrazu.ForeColor = SystemColors.WindowText
            lblInfoWymiary.Text = String.Format(ci, "Wymiary rastra: {0} x {1} px (~{2:0.0} m/pix)",
                                                _ozi.SzerokoscPx, _ozi.WysokoscPx, _ozi.RozdzielczoscMetrowNaPiksel)
            btnKonwertuj.Enabled = True
            lblStatus.Text = If(_ozi.DuzyBladKalibracji,
                                "Uwaga: punkty kalibracyjne nie pasują do siebie - mapa może być przesunięta (duży błąd RMS).",
                                "Kalibracja poprawna. Gotowy do konwersji.")
        Else
            txtPlikObrazu.ForeColor = Color.Red
            lblInfoWymiary.Text = "Wymiary: brak pliku obrazu na dysku (wskaż plik ręcznie przyciskiem Przeglądaj)"
            btnKonwertuj.Enabled = False
            lblStatus.Text = "Ostrzeżenie: wskaż plik obrazu mapy na dysku."
        End If

        lblInfoPunkty.Text = String.Format(ci, "Punkty kalibracyjne: {0} pkt{1}, błąd RMS: {2:0.0} m ({3})",
                                           _ozi.Punkty.Count, If(_ozi.PunktyWSiatce > 0, " (w siatce: " & _ozi.PunktyWSiatce & ")", ""),
                                           _ozi.Transformacja.BladRmsMetry, OpisModelu(_ozi.Transformacja.Model))
        lblInfoPunkty.ForeColor = If(_ozi.DuzyBladKalibracji, Color.DarkRed, SystemColors.ControlText)

        lblInfoZasieg.Text = String.Format(ci, "Zasięg WGS84: N {0:0.0000}° .. {1:0.0000}°, E {2:0.0000}° .. {3:0.0000}°",
                                           _ozi.ZasiegWgs84.Poludnie, _ozi.ZasiegWgs84.Polnoc,
                                           _ozi.ZasiegWgs84.Zachod, _ozi.ZasiegWgs84.Wschod)
    End Sub

    Private Shared Function OpisModelu(m As ModelKalibracji) As String
        Select Case m
            Case ModelKalibracji.Rzutowy : Return "model rzutowy"
            Case ModelKalibracji.Wielomian2 : Return "wielomian 2. stopnia"
            Case Else : Return "model afiniczny"
        End Select
    End Function

    Private Sub rbFormat_CheckedChanged(sender As Object, e As EventArgs) Handles rbKmz.CheckedChanged, rbJnx.CheckedChanged, rbMbtiles.CheckedChanged
        chkPodzialKmz.Enabled = rbKmz.Checked
    End Sub

    Private Async Sub btnKonwertuj_Click(sender As Object, e As EventArgs) Handles btnKonwertuj.Click
        If _czyTrybWsadowy Then
            Await WykonajKonwersjeWsadowaAsync()
        Else
            Await WykonajKonwersjePojedynczaAsync()
        End If
    End Sub

    ''' <summary>
    ''' Zamknięcie krzyżykiem w trakcie konwersji przerywa ją, a okno zamyka się po zakończeniu zadania (jak w oknie
    ''' korekty obrazów). Dawniej konwersja trwała w tle po zamknięciu okna, a na końcu pojawiał się jej komunikat.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _anulowanie IsNot Nothing Then
            _anulowanie.Cancel()
            _zamknijPoZakonczeniu = True
            lblStatus.Text = "Trwa przerywanie operacji..."
            e.Cancel = True
            Return
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        If _anulowanie IsNot Nothing Then
            _anulowanie.Cancel()
            lblStatus.Text = "Trwa przerywanie operacji..."
        Else
            Me.Close()
        End If
    End Sub

    Private Async Function WykonajKonwersjePojedynczaAsync() As Task
        If _ozi Is Nothing OrElse Not _ozi.PlikObrazuIstnieje Then Exit Function

        Dim domyslnaNazwa = Path.GetFileNameWithoutExtension(_ozi.SciezkaMap)
        If rbJnx.Checked Then
            dlgZapisz.Filter = "Garmin BirdsEye (*.jnx)|*.jnx"
            dlgZapisz.DefaultExt = "jnx"
            dlgZapisz.FileName = domyslnaNazwa & ".jnx"
        ElseIf rbKmz.Checked Then
            dlgZapisz.Filter = "Garmin Custom Maps (*.kmz)|*.kmz"
            dlgZapisz.DefaultExt = "kmz"
            dlgZapisz.FileName = domyslnaNazwa & ".kmz"
        Else
            dlgZapisz.Filter = "Baza MBTiles (*.mbtiles)|*.mbtiles"
            dlgZapisz.DefaultExt = "mbtiles"
            dlgZapisz.FileName = domyslnaNazwa & ".mbtiles"
        End If

        If dlgZapisz.ShowDialog(Me) <> DialogResult.OK Then Exit Function

        Dim sciezkaWyjsciowa = dlgZapisz.FileName
        Dim formatJnx = rbJnx.Checked
        Dim formatKmz = rbKmz.Checked
        Dim kolejnosc = CInt(nudKolejnosc.Value)
        Dim jakosc = CInt(nudJakosc.Value)
        Dim podzial = chkPodzialKmz.Checked

        btnKonwertuj.Enabled = False
        btnWybierzMap.Enabled = False
        btnWybierzFolder.Enabled = False
        btnWybierzObraz.Enabled = False
        grpFormat.Enabled = False
        grpOpcje.Enabled = False
        btnZamknij.Text = "Anuluj"
        prgPostep.Value = 0

        _anulowanie = New CancellationTokenSource()
        Dim postep = New Progress(Of Integer)(Sub(pct)
                                                  prgPostep.Value = Math.Max(0, Math.Min(100, pct))
                                                  lblStatus.Text = $"Trwa konwersja kafli... {pct}%"
                                              End Sub)

        Try
            Await Task.Run(Sub()
                               If formatKmz Then
                                   KonwertujDoKmz(_ozi, sciezkaWyjsciowa, kolejnosc, jakosc, podzial, postep, _anulowanie.Token)
                               ElseIf formatJnx Then
                                   KonwertujDoJnx(_ozi, sciezkaWyjsciowa, kolejnosc, jakosc, postep, _anulowanie.Token)
                               Else
                                   KonwertujDoMbtiles(_ozi, sciezkaWyjsciowa, jakosc, postep, _anulowanie.Token)
                               End If
                           End Sub, _anulowanie.Token)

            lblStatus.Text = "Konwersja zakończona pomyślnie!"
            prgPostep.Value = 100
            If Not _zamknijPoZakonczeniu Then MessageBox.Show("Pomyślnie utworzono mapę:" & vbCrLf & sciezkaWyjsciowa,
                            "Konwersja zakończona", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As OperationCanceledException
            lblStatus.Text = "Konwersja została anulowana."
            prgPostep.Value = 0
            If Not _zamknijPoZakonczeniu Then MessageBox.Show("Konwersja została przerwana przez użytkownika.",
                            "Anulowano", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            lblStatus.Text = "Błąd: " & OpisBledu.Z(ex)
            If Not _zamknijPoZakonczeniu Then MessageBox.Show("Wystąpił błąd podczas konwersji:" & vbCrLf & OpisBledu.Z(ex),
                            "Błąd konwersji", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnKonwertuj.Enabled = True
            btnWybierzMap.Enabled = True
            btnWybierzFolder.Enabled = True
            btnWybierzObraz.Enabled = True
            grpFormat.Enabled = True
            grpOpcje.Enabled = True
            btnZamknij.Text = "Zamknij"
            If _anulowanie IsNot Nothing Then
                _anulowanie.Dispose()
                _anulowanie = Nothing
            End If
            If _zamknijPoZakonczeniu Then Close()
        End Try
    End Function

    Private Async Function WykonajKonwersjeWsadowaAsync() As Task
        If _plikiWsadowe Is Nothing OrElse _plikiWsadowe.Count = 0 Then Exit Function

        If _plikiWsadowe.Count > 0 Then
            dlgFolderWyjsciowy.SelectedPath = Path.GetDirectoryName(_plikiWsadowe(0))
        End If
        If dlgFolderWyjsciowy.ShowDialog(Me) <> DialogResult.OK Then Exit Function

        Dim folderWyjsciowy = dlgFolderWyjsciowy.SelectedPath
        Dim format = If(rbJnx.Checked, FormatKonwersjiOzi.Jnx, If(rbKmz.Checked, FormatKonwersjiOzi.Kmz, FormatKonwersjiOzi.Mbtiles))
        Dim kolejnosc = CInt(nudKolejnosc.Value)
        Dim jakosc = CInt(nudJakosc.Value)
        Dim podzial = chkPodzialKmz.Checked

        btnKonwertuj.Enabled = False
        btnWybierzMap.Enabled = False
        btnWybierzFolder.Enabled = False
        btnWybierzObraz.Enabled = False
        grpFormat.Enabled = False
        grpOpcje.Enabled = False
        btnZamknij.Text = "Anuluj"
        prgPostep.Value = 0

        _anulowanie = New CancellationTokenSource()
        Dim postep = New Progress(Of RaportPostepuWsadowego)(Sub(r)
                                                                 prgPostep.Value = Math.Max(0, Math.Min(100, r.ProcentCalkowity))
                                                                 lblStatus.Text = r.StatusTekst
                                                             End Sub)

        Try
            Dim wynik As WynikKonwersjiWsadowej = Nothing
            Await Task.Run(Function()
                               wynik = KonwertujWsadowo(_plikiWsadowe, folderWyjsciowy, format, kolejnosc, jakosc, podzial, postep, _anulowanie.Token)
                               Return wynik
                           End Function, _anulowanie.Token)

            prgPostep.Value = 100
            lblStatus.Text = $"Konwersja wsadowa zakończona! Pomyślnie: {wynik.Sukcesy}/{wynik.LaczniePlikow}."

            Dim msg = $"Konwersja wsadowa zakończona!" & vbCrLf & vbCrLf &
                      $"Przetworzono pomyślnie: {wynik.Sukcesy} z {wynik.LaczniePlikow} map." & vbCrLf &
                      $"Folder docelowy:" & vbCrLf & folderWyjsciowy
            If wynik.Bledy.Count > 0 Then
                msg &= vbCrLf & vbCrLf & $"Uwagi i pominięte pliki ({wynik.Bledy.Count}):" & vbCrLf &
                       String.Join(vbCrLf, wynik.Bledy.Take(10))
                If wynik.Bledy.Count > 10 Then
                    msg &= vbCrLf & $"...oraz {wynik.Bledy.Count - 10} kolejnych."
                End If
            End If

            If Not _zamknijPoZakonczeniu Then MessageBox.Show(msg, "Konwersja wsadowa zakończona", MessageBoxButtons.OK,
                            If(wynik.Bledy.Count > 0, MessageBoxIcon.Warning, MessageBoxIcon.Information))
        Catch ex As OperationCanceledException
            lblStatus.Text = "Konwersja wsadowa została anulowana."
            prgPostep.Value = 0
            If Not _zamknijPoZakonczeniu Then MessageBox.Show("Konwersja wsadowa została przerwana przez użytkownika.",
                            "Anulowano", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            lblStatus.Text = "Błąd: " & OpisBledu.Z(ex)
            If Not _zamknijPoZakonczeniu Then MessageBox.Show("Wystąpił błąd podczas konwersji wsadowej:" & vbCrLf & OpisBledu.Z(ex),
                            "Błąd konwersji", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnKonwertuj.Enabled = True
            btnWybierzMap.Enabled = True
            btnWybierzFolder.Enabled = True
            btnWybierzObraz.Enabled = Not _czyTrybWsadowy
            grpFormat.Enabled = True
            grpOpcje.Enabled = True
            btnZamknij.Text = "Zamknij"
            If _anulowanie IsNot Nothing Then
                _anulowanie.Dispose()
                _anulowanie = Nothing
            End If
            If _zamknijPoZakonczeniu Then Close()
        End Try
    End Function

    ''' <summary>Liczba poziomów przeglądowych piramidy JNX/MBTiles poniżej poziomu szczegółowego.</summary>
    Friend Const PoziomyPrzegladowe As Integer = 4
    ''' <summary>Limit kafli pliku JNX w odbiornikach Garmin.</summary>
    Friend Const MaksKafliJnx As Integer = 50000

    ''' <summary>Poziom Web Mercator odpowiadający rozdzielczości skanu (10-18).</summary>
    Friend Shared Function PoziomSzczegolowy(ozi As PlikKalibracjiOzi) As Integer
        Dim mPerPx = ozi.RozdzielczoscMetrowNaPiksel
        If mPerPx <= 0 Then mPerPx = 2.5
        Dim srodekSz = (ozi.ZasiegWgs84.Polnoc + ozi.ZasiegWgs84.Poludnie) / 2.0
        Dim z = CInt(Math.Round(Math.Log(156543.03392 * Math.Cos(srodekSz * Math.PI / 180.0) / mPerPx) / Math.Log(2)))
        Return Math.Max(10, Math.Min(18, z))
    End Function

    ''' <summary>
    ''' Rysuje na kaflu fragment skanu. Kafel opisuje funkcja geoDoUv (WGS84 -> piksel kafla). Fragment skanu pod kaflem
    ''' jest wycinany i rysowany przekształceniem afinicznym wyznaczonym lokalnie z kalibracji - kafel uwzględnia obrót
    ''' mapy i nieliniowe modele kalibracji, a GDI+ nie przetwarza całego skanu przy każdym kaflu (wcześniej
    ''' ok. 1 s na kafel dla obróconych map). Duże wycinki (poziomy przeglądowe) są dzielone na ćwiartki.
    ''' </summary>
    ''' <returns>False, gdy kafel nie zachodzi na skan.</returns>
    Friend Shared Function RysujFragment(g As Graphics, src As Bitmap, ozi As PlikKalibracjiOzi,
                                         polnoc As Double, poludnie As Double, wschod As Double, zachod As Double,
                                         geoDoUv As Func(Of Double, Double, PointF), Optional glebokosc As Integer = 0) As Boolean
        Dim t = ozi.Transformacja
        Dim minX = Double.MaxValue, maxX = Double.MinValue, minY = Double.MaxValue, maxY = Double.MinValue
        For i = 0 To 4
            Dim f = i / 4.0
            Dim szer = poludnie + (polnoc - poludnie) * f
            Dim dl = zachod + (wschod - zachod) * f
            For Each p In {t.GeoDoPiksel(polnoc, dl), t.GeoDoPiksel(poludnie, dl), t.GeoDoPiksel(szer, zachod), t.GeoDoPiksel(szer, wschod)}
                minX = Math.Min(minX, p.Item1) : maxX = Math.Max(maxX, p.Item1)
                minY = Math.Min(minY, p.Item2) : maxY = Math.Max(maxY, p.Item2)
            Next
        Next
        Dim w = src.Width, h = src.Height
        If maxX < 0 OrElse maxY < 0 OrElse minX > w OrElse minY > h Then Return False

        If glebokosc < 3 AndAlso (maxX - minX > 1500 OrElse maxY - minY > 1500) Then
            Dim srSz = (polnoc + poludnie) / 2, srDl = (wschod + zachod) / 2
            Dim cwiartki = {
                New Double() {polnoc, srSz, srDl, zachod}, New Double() {polnoc, srSz, wschod, srDl},
                New Double() {srSz, poludnie, srDl, zachod}, New Double() {srSz, poludnie, wschod, srDl}}
            Dim narysowano = False
            For Each c In cwiartki
                Dim lg = geoDoUv(c(0), c(3)), pd = geoDoUv(c(1), c(2))
                Dim stan = g.Save()
                g.SetClip(RectangleF.FromLTRB(Math.Min(lg.X, pd.X), Math.Min(lg.Y, pd.Y), Math.Max(lg.X, pd.X), Math.Max(lg.Y, pd.Y)))
                If RysujFragment(g, src, ozi, c(0), c(1), c(2), c(3), geoDoUv, glebokosc + 1) Then narysowano = True
                g.Restore(stan)
            Next
            Return narysowano
        End If

        Dim x0 = Math.Max(0, CInt(Math.Floor(minX)) - 2)
        Dim y0 = Math.Max(0, CInt(Math.Floor(minY)) - 2)
        Dim x1 = Math.Min(w, CInt(Math.Ceiling(maxX)) + 2)
        Dim y1 = Math.Min(h, CInt(Math.Ceiling(maxY)) + 2)
        If x1 <= x0 OrElse y1 <= y0 Then Return False

        Dim gLG = t.PikselDoGeo(x0, y0), gPG = t.PikselDoGeo(x1, y0), gLD = t.PikselDoGeo(x0, y1)
        Dim punkty = {geoDoUv(gLG.Szerokosc, gLG.Dlugosc), geoDoUv(gPG.Szerokosc, gPG.Dlugosc), geoDoUv(gLD.Szerokosc, gLD.Dlugosc)}
        Using atr As New ImageAttributes()
            'odbicie krawędzi zamiast półprzezroczystej obwódki interpolacji na brzegu wycinka
            atr.SetWrapMode(Drawing2D.WrapMode.TileFlipXY)
            g.DrawImage(src, punkty, New RectangleF(x0, y0, x1 - x0, y1 - y0), GraphicsUnit.Pixel, atr)
        End Using
        Return True
    End Function

    ''' <summary>Kafel Web Mercator 256 x 256 ze skanu; Nothing - kafel poza mapą.</summary>
    Friend Shared Function KafelMercatora(src As Bitmap, ozi As PlikKalibracjiOzi, x As Integer, y As Integer, z As Integer,
                                          jakosc As Integer) As Byte()
        Dim polnoc = WebMercator.Szerokosc(y * 256.0, z)
        Dim poludnie = WebMercator.Szerokosc((y + 1) * 256.0, z)
        Dim zachod = WebMercator.Dlugosc(x * 256.0, z)
        Dim wschod = WebMercator.Dlugosc((x + 1) * 256.0, z)
        Dim n = 256.0 * (1L << z)
        Dim geoDoUv = Function(lat As Double, lon As Double) As PointF
                          Dim fi = lat * Math.PI / 180.0
                          Dim u = (lon + 180.0) / 360.0 * n - x * 256.0
                          Dim v = (1.0 - Math.Log(Math.Tan(fi) + 1.0 / Math.Cos(fi)) / Math.PI) / 2.0 * n - y * 256.0
                          Return New PointF(CSng(u), CSng(v))
                      End Function
        Using kafelBmp As New Bitmap(256, 256, PixelFormat.Format24bppRgb)
            Using g = Graphics.FromImage(kafelBmp)
                g.Clear(Color.White)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                If Not RysujFragment(g, src, ozi, polnoc, poludnie, wschod, zachod, geoDoUv) Then Return Nothing
            End Using
            Return EksportMapy.Koduj(kafelBmp, False, jakosc)
        End Using
    End Function

    ''' <summary>Kafle piramidy (od poziomu przeglądowego do szczegółowego) pokrywające zasięg mapy.</summary>
    Friend Shared Function KafleDoWygenerowania(ozi As PlikKalibracjiOzi) As List(Of KafelXYZ)
        Dim zMax = PoziomSzczegolowy(ozi)
        Dim zMin = Math.Max(8, zMax - PoziomyPrzegladowe)
        Dim lista As New List(Of KafelXYZ)
        For z = zMin To zMax
            Dim zakres = WebMercator.ZakresKafli(ozi.ZasiegWgs84, z)
            For y = zakres.Item2 To zakres.Item4
                For x = zakres.Item1 To zakres.Item3
                    lista.Add(New KafelXYZ(x, y, z))
                Next
            Next
        Next
        Return lista
    End Function

    Friend Shared Sub KonwertujDoKmz(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                      kolejnosc As Integer, jakosc As Integer, podzial As Boolean,
                                      postep As IProgress(Of Integer), token As CancellationToken)
        'KMZ: kafle w siatce szerokości i długości geograficznej (Garmin nie obraca obrazu w LatLonBox) - skan jest
        'przerysowywany do tej siatki; wcześniej prostokąty zasięgu kafli skanu przesuwały obrócone mapy
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim zasieg = ozi.ZasiegWgs84
            Dim mPerPx = If(ozi.RozdzielczoscMetrowNaPiksel > 0, ozi.RozdzielczoscMetrowNaPiksel, 2.5)
            Dim srodekSz = (zasieg.Polnoc + zasieg.Poludnie) / 2.0
            Dim stopnieSzer = mPerPx / 111132.95
            Dim stopnieDl = mPerPx / (111412.84 * Math.Cos(srodekSz * Math.PI / 180.0))
            Const kRozmiar As Integer = 1024
            Dim w = CInt(Math.Ceiling((zasieg.Wschod - zasieg.Zachod) / stopnieDl))
            Dim h = CInt(Math.Ceiling((zasieg.Polnoc - zasieg.Poludnie) / stopnieSzer))
            Dim cols = CInt(Math.Ceiling(w / CDbl(kRozmiar)))
            Dim rows = CInt(Math.Ceiling(h / CDbl(kRozmiar)))
            Dim lacznieKafli = cols * rows

            Dim maxKafliNaTom = If(podzial, 100, Integer.MaxValue)
            Dim liczbaTomow = CInt(Math.Ceiling(lacznieKafli / CDbl(maxKafliNaTom)))

            Dim biezacyTom = 1
            Dim kafliWTomie = 0
            Dim sciezkaTomu = If(liczbaTomow > 1, EksportMapy.NazwaPlikuTomu(plikWyjsciowy, biezacyTom, liczbaTomow), plikWyjsciowy)
            Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
            Dim zapisKmz As New ZapisKmz(sciezkaTomu, tytul, kolejnosc)
            Dim utworzone As New List(Of String) From {sciezkaTomu}
            Dim zapisane = 0

            Try
                Dim licznik = 0
                For r = 0 To rows - 1
                    For c = 0 To cols - 1
                        token.ThrowIfCancellationRequested()
                        Dim kSzer = Math.Min(kRozmiar, w - c * kRozmiar)
                        Dim kWys = Math.Min(kRozmiar, h - r * kRozmiar)
                        Dim n = zasieg.Polnoc - r * kRozmiar * stopnieSzer
                        Dim s = n - kWys * stopnieSzer
                        Dim we = zasieg.Zachod + c * kRozmiar * stopnieDl
                        Dim ea = we + kSzer * stopnieDl
                        Dim geoDoUv = Function(lat As Double, lon As Double) New PointF(CSng((lon - we) / stopnieDl), CSng((n - lat) / stopnieSzer))

                        Using kafelBmp As New Bitmap(kSzer, kWys, PixelFormat.Format24bppRgb)
                            Dim jestTresc As Boolean
                            Using g = Graphics.FromImage(kafelBmp)
                                g.Clear(Color.White)
                                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                                jestTresc = RysujFragment(g, srcBmp, ozi, n, s, ea, we, geoDoUv)
                            End Using
                            If jestTresc Then
                                Dim jpeg = EksportMapy.Koduj(kafelBmp, False, jakosc)
                                If kafliWTomie >= maxKafliNaTom Then
                                    zapisKmz.Zakoncz()
                                    zapisKmz.Dispose()
                                    biezacyTom += 1
                                    kafliWTomie = 0
                                    sciezkaTomu = EksportMapy.NazwaPlikuTomu(plikWyjsciowy, biezacyTom, liczbaTomow)
                                    zapisKmz = New ZapisKmz(sciezkaTomu, tytul, kolejnosc)
                                    utworzone.Add(sciezkaTomu)
                                End If
                                Dim nazwaKafla = String.Format(CultureInfo.InvariantCulture, "tile_{0:00}_{1:00}.jpg", r, c)
                                zapisKmz.DodajKafel(nazwaKafla, jpeg, New ProstokatGeo(n, s, ea, we))
                                kafliWTomie += 1
                                zapisane += 1
                            End If
                        End Using

                        licznik += 1
                        postep?.Report(CInt(licznik * 100L / lacznieKafli))
                    Next
                Next
                If zapisane = 0 Then Throw New InvalidOperationException("Nie wygenerowano żadnych kafli - kalibracja nie pokrywa obrazu mapy.")
                zapisKmz.Zakoncz()
                zapisKmz.Dispose()
            Catch
                'anulowanie lub błąd: niedokończone tomy KMZ nie mogą zostać na dysku (wyglądałyby na gotową mapę)
                zapisKmz.Dispose()
                For Each plik In utworzone
                    UsunPlikBezpiecznie(plik)
                Next
                Throw
            End Try
        End Using
    End Sub

    Friend Shared Sub KonwertujDoJnx(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                      kolejnosc As Integer, jakosc As Integer,
                                      postep As IProgress(Of Integer), token As CancellationToken)
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim kafle = KafleDoWygenerowania(ozi)
            If kafle.Count > MaksKafliJnx Then
                Throw New InvalidOperationException($"Mapa wymaga {kafle.Count} kafli, a plik JNX może mieć najwyżej {MaksKafliJnx}. Podziel skan na mniejsze arkusze.")
            End If
            Dim srodekSz = (ozi.ZasiegWgs84.Polnoc + ozi.ZasiegWgs84.Poludnie) / 2.0
            'poziomy od przeglądowego do szczegółowego - jak w eksporcie JNX z okna eksportu
            Dim poziomy As New SortedDictionary(Of Integer, PoziomJnx)
            Dim licznik = 0
            For Each k In kafle
                token.ThrowIfCancellationRequested()
                Dim dane = KafelMercatora(srcBmp, ozi, k.X, k.Y, k.Z, jakosc)
                If dane IsNot Nothing Then
                    Dim poziom As PoziomJnx = Nothing
                    If Not poziomy.TryGetValue(k.Z, poziom) Then
                        poziom = New PoziomJnx(ZapisJnx.SkalaDlaPoziomu(k.Z, srodekSz))
                        poziomy(k.Z) = poziom
                    End If
                    poziom.Kafle.Add(New KafelJnx(New ProstokatGeo(WebMercator.Szerokosc(k.Y * 256.0, k.Z), WebMercator.Szerokosc((k.Y + 1) * 256.0, k.Z),
                                                                   WebMercator.Dlugosc((k.X + 1) * 256.0, k.Z), WebMercator.Dlugosc(k.X * 256.0, k.Z)), dane))
                End If
                licznik += 1
                postep?.Report(CInt(licznik * 100L / kafle.Count))
            Next

            If poziomy.Count = 0 Then
                Throw New InvalidOperationException("Nie wygenerowano żadnych kafli - kalibracja nie pokrywa obrazu mapy.")
            End If

            Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
            Try
                ZapisJnx.Zapisz(plikWyjsciowy, tytul, kolejnosc, poziomy.Values.ToList())
            Catch
                UsunPlikBezpiecznie(plikWyjsciowy)
                Throw
            End Try
        End Using
    End Sub

    Friend Shared Sub KonwertujDoMbtiles(ozi As PlikKalibracjiOzi, plikWyjsciowy As String,
                                          jakosc As Integer, postep As IProgress(Of Integer), token As CancellationToken)
        Using srcBmp As New Bitmap(ozi.PlikObrazu)
            Dim kafle = KafleDoWygenerowania(ozi)
            Dim zMax = kafle.Max(Function(k) k.Z)
            Dim zMin = kafle.Min(Function(k) k.Z)
            Dim srodekSz = (ozi.ZasiegWgs84.Polnoc + ozi.ZasiegWgs84.Poludnie) / 2.0

            EksportMapy.UsunPlikBazy(plikWyjsciowy)
            Try
            Using polaczenie As New SQLiteConnection(EksportMapy.ParametryPolaczenia(plikWyjsciowy, "Wal"))
                polaczenie.Open()
                Using cmdInit As New SQLiteCommand("PRAGMA synchronous = OFF; PRAGMA journal_mode = WAL; CREATE TABLE metadata (name text, value text); CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob); CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);", polaczenie)
                    cmdInit.ExecuteNonQuery()
                End Using

                Dim ci = CultureInfo.InvariantCulture
                Dim tytul = If(String.IsNullOrEmpty(ozi.Tytul), Path.GetFileNameWithoutExtension(plikWyjsciowy), ozi.Tytul)
                Dim meta As New Dictionary(Of String, String) From {
                    {"name", tytul},
                    {"type", "baselayer"},
                    {"version", "1.1"},
                    {"description", "Konwersja z mapy OziExplorer (.map)"},
                    {"format", "jpg"},
                    {"minzoom", zMin.ToString(ci)},
                    {"maxzoom", zMax.ToString(ci)},
                    {"bounds", String.Format(ci, "{0},{1},{2},{3}", ozi.ZasiegWgs84.Zachod, ozi.ZasiegWgs84.Poludnie, ozi.ZasiegWgs84.Wschod, ozi.ZasiegWgs84.Polnoc)},
                    {"center", String.Format(ci, "{0},{1},{2}", (ozi.ZasiegWgs84.Zachod + ozi.ZasiegWgs84.Wschod) / 2.0, srodekSz, zMax)}
                }

                Using cmdMeta As New SQLiteCommand("INSERT INTO metadata (name, value) VALUES (@n, @v);", polaczenie)
                    Dim pN = cmdMeta.Parameters.Add("@n", DbType.String)
                    Dim pV = cmdMeta.Parameters.Add("@v", DbType.String)
                    For Each m In meta
                        pN.Value = m.Key
                        pV.Value = m.Value
                        cmdMeta.ExecuteNonQuery()
                    Next
                End Using

                Dim licznik = 0
                Dim zapisane = 0
                Using trans = polaczenie.BeginTransaction()
                    Using cmdTile As New SQLiteCommand("INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);", polaczenie, trans)
                        Dim pZ = cmdTile.Parameters.Add("@z", DbType.Int32)
                        Dim pX = cmdTile.Parameters.Add("@x", DbType.Int32)
                        Dim pY = cmdTile.Parameters.Add("@y", DbType.Int32)
                        Dim pD = cmdTile.Parameters.Add("@d", DbType.Binary)
                        For Each k In kafle
                            token.ThrowIfCancellationRequested()
                            Dim dane = KafelMercatora(srcBmp, ozi, k.X, k.Y, k.Z, jakosc)
                            If dane IsNot Nothing Then
                                ' W MBTiles wiersze liczone są od dołu (TMS)
                                pZ.Value = k.Z
                                pX.Value = k.X
                                pY.Value = k.WierszTms
                                pD.Value = dane
                                cmdTile.ExecuteNonQuery()
                                zapisane += 1
                            End If
                            licznik += 1
                            postep?.Report(CInt(licznik * 100L / kafle.Count))
                        Next
                        trans.Commit()
                    End Using
                End Using
                If zapisane = 0 Then Throw New InvalidOperationException("Nie wygenerowano żadnych kafli - kalibracja nie pokrywa obrazu mapy.")

                Using cmdDel As New SQLiteCommand("PRAGMA journal_mode = DELETE;", polaczenie)
                    cmdDel.ExecuteNonQuery()
                End Using
            End Using
            Catch
                'anulowanie lub błąd: niedokończona baza MBTiles (z plikami -wal, -shm) usuwana z dysku
                SQLiteConnection.ClearAllPools()
                Try
                    EksportMapy.UsunPlikBazy(plikWyjsciowy)
                Catch
                End Try
                Throw
            End Try
        End Using
    End Sub

    Private Shared Sub UsunPlikBezpiecznie(plik As String)
        Try
            If File.Exists(plik) Then File.Delete(plik)
        Catch
        End Try
    End Sub

    ''' <summary>Nazwa pliku wynikowego: nazwa pliku .map, przy powtórce poprzedzona nazwą folderu, a w ostateczności numerem.</summary>
    Friend Shared Function UnikalnaNazwaWyniku(plikMap As String, uzyte As HashSet(Of String)) As String
        Dim rdzen = Path.GetFileNameWithoutExtension(plikMap)
        Dim nazwa = rdzen
        If uzyte.Contains(nazwa) Then
            Dim folder = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(plikMap)))
            If Not String.IsNullOrEmpty(folder) Then nazwa = folder & "_" & rdzen
        End If
        Dim numer = 2
        Dim kandydat = nazwa
        While uzyte.Contains(kandydat)
            kandydat = nazwa & "_" & numer.ToString(CultureInfo.InvariantCulture)
            numer += 1
        End While
        uzyte.Add(kandydat)
        Return kandydat
    End Function

    Friend Shared Function KonwertujWsadowo(plikiMap As IEnumerable(Of String),
                                            folderWyjsciowy As String,
                                            format As FormatKonwersjiOzi,
                                            kolejnosc As Integer,
                                            jakosc As Integer,
                                            podzialKmz As Boolean,
                                            postep As IProgress(Of RaportPostepuWsadowego),
                                            token As CancellationToken) As WynikKonwersjiWsadowej
        If Not Directory.Exists(folderWyjsciowy) Then
            Directory.CreateDirectory(folderWyjsciowy)
        End If

        Dim lista = plikiMap.Where(Function(p) Not String.IsNullOrWhiteSpace(p)).ToList()
        Dim wynik As New WynikKonwersjiWsadowej With {.LaczniePlikow = lista.Count}
        Dim lacznie = lista.Count
        'mapy o tej samej nazwie z różnych podfolderów nie mogą nadpisać jednego pliku wynikowego
        Dim uzyteNazwy As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For i = 0 To lacznie - 1
            token.ThrowIfCancellationRequested()
            Dim plikMap = lista(i)
            Dim nazwaPliku = Path.GetFileName(plikMap)
            Dim rdzen = Path.GetFileNameWithoutExtension(plikMap)
            Dim nrPliku = i + 1

            postep?.Report(New RaportPostepuWsadowego With {
                .IndeksPliku = nrPliku,
                .LaczniePlikow = lacznie,
                .NazwaPliku = nazwaPliku,
                .ProcentPliku = 0,
                .ProcentCalkowity = CInt(Math.Round((i / CDbl(Math.Max(1, lacznie))) * 100.0)),
                .StatusTekst = $"Wczytywanie [{nrPliku}/{lacznie}]: {nazwaPliku}..."
            })

            Try
                Dim ozi = PlikKalibracjiOzi.Wczytaj(plikMap)
                If Not ozi.PlikObrazuIstnieje Then
                    wynik.Pominiete += 1
                    wynik.Bledy.Add($"{nazwaPliku}: brak pliku obrazu rastrowego na dysku.")
                    Continue For
                End If

                Dim rozszerzenie = If(format = FormatKonwersjiOzi.Jnx, ".jnx", If(format = FormatKonwersjiOzi.Kmz, ".kmz", ".mbtiles"))
                Dim nazwaWyniku = UnikalnaNazwaWyniku(plikMap, uzyteNazwy)
                Dim sciezkaDocelowa = Path.Combine(folderWyjsciowy, nazwaWyniku & rozszerzenie)
                If ozi.DuzyBladKalibracji Then
                    wynik.Bledy.Add($"{nazwaPliku}: uwaga - duży błąd kalibracji (RMS {ozi.Transformacja.BladRmsMetry:0} m), mapa może być przesunięta.")
                End If
                If Not ozi.DatumObslugiwane Then
                    wynik.Bledy.Add($"{nazwaPliku}: uwaga - nieznany datum ({ozi.Datum}), współrzędne użyte bez korekty.")
                End If
                Dim biezacyIndeks = i

                Dim postepPliku = New Progress(Of Integer)(Sub(pct)
                                                              Dim calkowity = CInt(Math.Round(((biezacyIndeks + (pct / 100.0)) / CDbl(Math.Max(1, lacznie))) * 100.0))
                                                              postep?.Report(New RaportPostepuWsadowego With {
                                                                  .IndeksPliku = nrPliku,
                                                                  .LaczniePlikow = lacznie,
                                                                  .NazwaPliku = nazwaPliku,
                                                                  .ProcentPliku = pct,
                                                                  .ProcentCalkowity = Math.Min(99, calkowity),
                                                                  .StatusTekst = $"Konwersja [{nrPliku}/{lacznie}]: {nazwaPliku} ({pct}%)"
                                                              })
                                                          End Sub)

                If format = FormatKonwersjiOzi.Kmz Then
                    KonwertujDoKmz(ozi, sciezkaDocelowa, kolejnosc, jakosc, podzialKmz, postepPliku, token)
                ElseIf format = FormatKonwersjiOzi.Jnx Then
                    KonwertujDoJnx(ozi, sciezkaDocelowa, kolejnosc, jakosc, postepPliku, token)
                Else
                    KonwertujDoMbtiles(ozi, sciezkaDocelowa, jakosc, postepPliku, token)
                End If

                wynik.Sukcesy += 1
                wynik.WygenerowanePliki.Add(sciezkaDocelowa)
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                wynik.Pominiete += 1
                wynik.Bledy.Add($"{nazwaPliku}: {OpisBledu.Z(ex)}")
            End Try
        Next

        postep?.Report(New RaportPostepuWsadowego With {
            .IndeksPliku = lacznie,
            .LaczniePlikow = lacznie,
            .NazwaPliku = "",
            .ProcentPliku = 100,
            .ProcentCalkowity = 100,
            .StatusTekst = $"Zakończono. Pomyślnie przetworzono {wynik.Sukcesy} z {lacznie} map."
        })

        Return wynik
    End Function

End Class
