'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Narzędzie wsadowego przetwarzania obrazów rastrowych w wybranym folderze
''' (korekcja kontrastu, gamma, nasycenia i wyostrzanie krawędzi według wybranego profilu).
''' </summary>
Public Class FormKorektaFolderu

    Private _opcjeKorekty As New OpcjeKorekcjiMip()
    Private _cts As CancellationTokenSource
    'zamknięcie okna w trakcie przetwarzania: najpierw przerwanie, a okno zamyka się po zakończeniu zadania
    Private _zamknijPoZakonczeniu As Boolean

    ''' <summary>
    ''' Folder źródłowy zamiast folderu bieżącej sesji (okno otwarte z eksportu do CAD). Ustawiany przed ShowDialog -
    ''' pole tekstowe wypełnione przed otwarciem nadpisywało zdarzenie Load folderem sesji.
    ''' </summary>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property FolderPoczatkowy As String = ""

    Private Sub FormKorektaFolderu_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CancelButton = btnZamknij
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.KorektaFolderu, 15, 287)
        _opcjeKorekty = Ustawienia.OpcjeKorekty(ProfilKorektyRastra.GarminMip)
        'w tym oknie korekta jest celem - zawsze włączona (profil „Brak” dałby kopie bez zmian)
        If _opcjeKorekty.Profil = ProfilKorektyRastra.Brak Then _opcjeKorekty.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
        _opcjeKorekty.Wlaczona = True
        AktualizujEtykieteProfilu()

        ' Inicjalizacja domyślnego folderu źródłowego
        Dim zrodlo = If(Not String.IsNullOrEmpty(FolderPoczatkowy) AndAlso Directory.Exists(FolderPoczatkowy), FolderPoczatkowy,
                        If(Not String.IsNullOrEmpty(Form1.FolderSesji) AndAlso Directory.Exists(Form1.FolderSesji), Form1.FolderSesji, FolderDownload))
        txtFolderZrodlo.Text = zrodlo
        UstawDomyslnyFolderCel(zrodlo)
    End Sub

    Private Sub UstawDomyslnyFolderCel(zrodlo As String)
        If Not String.IsNullOrEmpty(zrodlo) Then
            txtFolderCel.Text = Path.Combine(zrodlo.TrimEnd("\"c, "/"c), "korekta")
        End If
    End Sub

    Private Sub txtFolderZrodlo_TextChanged(sender As Object, e As EventArgs) Handles txtFolderZrodlo.TextChanged
        If Directory.Exists(txtFolderZrodlo.Text) Then
            UstawDomyslnyFolderCel(txtFolderZrodlo.Text)
        End If
    End Sub

    Private Sub btnWybierzZrodlo_Click(sender As Object, e As EventArgs) Handles btnWybierzZrodlo.Click
        dlgFolder.SelectedPath = txtFolderZrodlo.Text
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            txtFolderZrodlo.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnWybierzCel_Click(sender As Object, e As EventArgs) Handles btnWybierzCel.Click
        dlgFolder.SelectedPath = txtFolderCel.Text
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            txtFolderCel.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnDostosuj_Click(sender As Object, e As EventArgs) Handles btnDostosuj.Click
        Dim probka = PobierzProbkeZFolderu(txtFolderZrodlo.Text)
        Using dlg As New FormPodgladMip(_opcjeKorekty, probka)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                If dlg.Opcje.Profil = ProfilKorektyRastra.Brak OrElse Not dlg.Opcje.Wlaczona Then
                    'profil „Brak” dawał kopie bez korekty, a pliki JPEG traciły jakość przez ponowną kompresję
                    MessageBox.Show("Profil ""Brak"" nie zmienia obrazów, więc w tym oknie nie można go wybrać. Pozostaje profil: " &
                                    OpcjeKorekcjiMip.NazwaProfilu(_opcjeKorekty.Profil) & ".", "Korekta obrazów",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    _opcjeKorekty = dlg.Opcje.Kopia()
                    AktualizujEtykieteProfilu()
                End If
            End If
        End Using
        probka?.Dispose()
    End Sub

    Private Function PobierzProbkeZFolderu(folder As String) As Bitmap
        Try
            If Directory.Exists(folder) Then
                For Each plik In Directory.GetFiles(folder)
                    Dim ext = Path.GetExtension(plik).ToLowerInvariant()
                    If {".png", ".jpg", ".jpeg", ".tif", ".tiff"}.Contains(ext) Then
                        Using bmp = Image.FromFile(plik)
                            Return New Bitmap(bmp)
                        End Using
                    End If
                Next
            End If
        Catch
        End Try
        Return Nothing
    End Function

    Private Sub AktualizujEtykieteProfilu()
        lblProfilOpis.Text = "Profil: " & OpcjeKorekcjiMip.NazwaProfilu(_opcjeKorekty.Profil)
        btnDostosuj.Text = "Profil: " & OpcjeKorekcjiMip.KrotkaNazwaProfilu(_opcjeKorekty.Profil) & "..."
    End Sub

    ''' <summary>
    ''' Opis błędu w folderach źródłowym i docelowym ("" - poprawne). Puste pole folderu docelowego dawało wyjątek
    ''' Path.GetFullPath w obsłudze kliknięcia (awaria programu), a ścieżka względna wskazywała folder bieżący procesu.
    ''' </summary>
    Friend Shared Function BladFolderow(zrodlo As String, cel As String) As String
        If String.IsNullOrWhiteSpace(zrodlo) OrElse Not Path.IsPathFullyQualified(zrodlo) OrElse Not Directory.Exists(zrodlo) Then
            Return "Folder źródłowy nie istnieje: " & zrodlo
        End If
        If String.IsNullOrWhiteSpace(cel) Then Return "Wskaż folder docelowy, w którym zostaną zapisane poprawione obrazy."
        If Not Path.IsPathFullyQualified(cel) Then
            Return "Folder docelowy musi być podany pełną ścieżką (np. C:\Mapy\korekta): " & cel
        End If
        Try
            If String.Equals(Path.GetFullPath(zrodlo).TrimEnd("\"c), Path.GetFullPath(cel).TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
                Return "Folder docelowy musi być inny niż źródłowy, aby uniknąć nadpisania oryginalnych plików."
            End If
        Catch ex As Exception
            Return "Niepoprawna ścieżka folderu docelowego " & cel & ": " & OpisBledu.Z(ex)
        End Try
        Return ""
    End Function

    Private Async Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Dim zrodlo = txtFolderZrodlo.Text.Trim()
        Dim cel = txtFolderCel.Text.Trim()

        Dim uwagaFolderow As String = BladFolderow(zrodlo, cel)
        If uwagaFolderow <> "" Then
            MessageBox.Show(uwagaFolderow, "Korekta obrazów", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim pominieteArkusze As Integer = 0
        Dim plikiObrazow = PlikiDoKorekty(zrodlo, pominieteArkusze)

        If Not _opcjeKorekty.Wlaczona OrElse _opcjeKorekty.Profil = ProfilKorektyRastra.Brak Then
            MessageBox.Show("Wybierz profil korekty przyciskiem ""Profil / Podgląd"".", "Korekta obrazów", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If plikiObrazow.Count = 0 Then
            MessageBox.Show("W folderze źródłowym nie znaleziono obsługiwanych plików graficznych (.png, .jpg, .tif)" &
                            If(pominieteArkusze > 0, " poza scalonymi arkuszami", "") & ".", "Brak plików", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            If Not Directory.Exists(cel) Then Directory.CreateDirectory(cel)
        Catch ex As Exception
            MessageBox.Show("Nie udało się utworzyć folderu docelowego: " & OpisBledu.Z(ex), "Korekta obrazów", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        ' Przygotowanie UI
        _cts = New CancellationTokenSource()
        btnStart.Enabled = False
        btnWybierzZrodlo.Enabled = False
        btnWybierzCel.Enabled = False
        btnDostosuj.Enabled = False
        btnPrzerwij.Enabled = True
        prgPostep.Value = 0
        prgPostep.Maximum = plikiObrazow.Count

        Dim kopiujGeo = chkKopiujGeo.Checked
        Dim opcje = _opcjeKorekty.Kopia()
        Dim token = _cts.Token

        Dim postep As New Progress(Of Integer)(
            Sub(licznik)
                If IsDisposed Then Exit Sub
                prgPostep.Value = Math.Min(prgPostep.Maximum, licznik)
                lblStatus.Text = String.Format("Przetworzono: {0} / {1} ({2}%)", licznik, plikiObrazow.Count, CInt(licznik * 100 \ plikiObrazow.Count))
            End Sub)

        Try
            Dim wynik = Await Task.Run(
                Function()
                    Return WykonajKorekteFolderu(plikiObrazow, cel, opcje, kopiujGeo, postep, token)
                End Function, token)

            If Not _zamknijPoZakonczeniu Then
                Dim bledy = wynik.Bledy
                If bledy.Count = 0 Then
                    lblStatus.Text = String.Format("Zakończono pomyślnie. Zapisano {0} obrazów w: {1}", wynik.Zapisane, cel)
                Else
                    lblStatus.Text = String.Format("Zapisano {0} obrazów, {1} plików nie udało się przetworzyć.", wynik.Zapisane, bledy.Count)
                End If
                Dim tresc = String.Format("Przetworzono {0} obrazów.", wynik.Zapisane)
                If bledy.Count > 0 Then
                    tresc &= vbCrLf & vbCrLf & String.Format("Nie udało się przetworzyć {0} plików:", bledy.Count) & vbCrLf &
                             String.Join(vbCrLf, bledy.Take(10)) & If(bledy.Count > 10, vbCrLf & "…", "")
                End If
                If MessageBox.Show(tresc & vbCrLf & vbCrLf & "Czy chcesz otworzyć folder docelowy?",
                                   "Korekta zakończona", MessageBoxButtons.YesNo,
                                   If(bledy.Count = 0, MessageBoxIcon.Information, MessageBoxIcon.Warning)) = DialogResult.Yes Then
                    OtworzFolder(cel)
                End If
            End If
        Catch ex As OperationCanceledException
            lblStatus.Text = "Przetwarzanie zostało przerwane przez użytkownika."
        Catch ex As Exception
            lblStatus.Text = "Błąd: " & OpisBledu.Z(ex)
            If Not _zamknijPoZakonczeniu Then
                MessageBox.Show("Wystąpił błąd podczas przetwarzania: " & OpisBledu.Z(ex), "Korekta obrazów", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            _cts.Dispose()
            _cts = Nothing
            btnStart.Enabled = True
            btnWybierzZrodlo.Enabled = True
            btnWybierzCel.Enabled = True
            btnDostosuj.Enabled = True
            btnPrzerwij.Enabled = False
        End Try
        If _zamknijPoZakonczeniu Then Close()
    End Sub

    ''' <summary>Wynik korekty folderu: liczba zapisanych obrazów i pliki, których nie udało się przetworzyć (nazwa: przyczyna).</summary>
    Private NotInheritable Class WynikKorekty
        Public Property Zapisane As Integer
        Public Property Bledy As New List(Of String)
    End Class

    ''' <summary>
    ''' Pliki georeferencji kopiowane obok poprawionych obrazów. Program zapisuje world file jako .jpgw/.pngw/.tifw
    ''' (Georeferencja.RozszerzenieWorldFile) - dawniej brakowało .jpgw i .tifw, więc segmenty JPEG i TIFF traciły położenie.
    ''' </summary>
    Friend Shared ReadOnly RozszerzeniaGeoreferencji As String() =
        {".jpgw", ".pngw", ".tifw", ".gifw", ".jgw", ".pgw", ".tfw", ".wld", ".prj", ".map", ".gmi", ".tab", ".kml"}

    Private Shared Function WykonajKorekteFolderu(pliki As List(Of String), folderCel As String,
                                                  opcje As OpcjeKorekcjiMip, kopiujGeo As Boolean,
                                                  postep As IProgress(Of Integer), token As CancellationToken) As WynikKorekty
        Dim licznik As Integer = 0
        Dim bledy As New System.Collections.Concurrent.ConcurrentBag(Of String)
        Dim rozszerzeniaGeo = RozszerzeniaGeoreferencji

        ' Kopiowanie conf.txt jeśli istnieje
        If pliki.Count > 0 Then
            Dim confZrodlo = Path.Combine(Path.GetDirectoryName(pliki(0)), "conf.txt")
            If File.Exists(confZrodlo) Then
                Try
                    File.Copy(confZrodlo, Path.Combine(folderCel, "conf.txt"), True)
                Catch
                End Try
            End If
        End If

        Dim po As New ParallelOptions With {
            .CancellationToken = token,
            .MaxDegreeOfParallelism = LiczbaWatkow(pliki)
        }

        Parallel.ForEach(pliki, po,
            Sub(plik)
                token.ThrowIfCancellationRequested()
                Dim nazwa = Path.GetFileName(plik)
                Dim sciezkaCel = Path.Combine(folderCel, nazwa)

                'uszkodzony lub nieobsługiwany plik nie przerywa całej partii - dawniej jeden zły plik kończył
                'przetwarzanie komunikatem „One or more errors occurred” bez nazwy pliku
                Try
                    KorygujPlik(plik, sciezkaCel, opcje)
                Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                    bledy.Add(nazwa & ": " & OpisBledu.Z(ex))
                    Return
                End Try

                If kopiujGeo Then
                    Dim rdzen = Path.ChangeExtension(plik, Nothing)
                    'plik .points georeferencera QGIS ma w nazwie rozszerzenie obrazu (segment.jpg.points) - dawniej
                    'szukany jako segment.points, więc nie był kopiowany
                    For Each plikGeo In rozszerzeniaGeo.Select(Function(r) rdzen & r).Append(plik & ".points")
                        If File.Exists(plikGeo) Then
                            Try
                                File.Copy(plikGeo, Path.Combine(folderCel, Path.GetFileName(plikGeo)), True)
                            Catch
                            End Try
                        End If
                    Next
                End If

                Dim l = Interlocked.Increment(licznik)
                postep?.Report(l)
            End Sub)

        Return New WynikKorekty With {.Zapisane = licznik, .Bledy = bledy.OrderBy(Function(b) b, StringComparer.OrdinalIgnoreCase).ToList()}
    End Function

    ''' <summary>Wczytuje obraz, poprawia go i zapisuje w formacie wynikającym z rozszerzenia.</summary>
    Private Shared Sub KorygujPlik(plik As String, sciezkaCel As String, opcje As OpcjeKorekcjiMip)
        Using zrodloBmp As New Bitmap(plik)
            Using kopiaBmp As New Bitmap(zrodloBmp.Width, zrodloBmp.Height, PixelFormat.Format32bppArgb)
                Using g = Graphics.FromImage(kopiaBmp)
                    g.DrawImage(zrodloBmp, 0, 0, zrodloBmp.Width, zrodloBmp.Height)
                End Using

                If opcje.Wlaczona Then
                    NakladanieObrazow.ZastosujKorekcjeMip(kopiaBmp, opcje)
                End If

                ' Zapis w odpowiednim formacie
                Select Case Path.GetExtension(plik).ToLowerInvariant()
                    Case ".jpg", ".jpeg"
                        'jakość 95 - domyślna jakość GDI+ (ok. 75) wyraźnie pogarszała segmenty
                        File.WriteAllBytes(sciezkaCel, EksportMapy.Koduj(kopiaBmp, False, JakoscJpeg))
                    Case ".tif", ".tiff"
                        If Not ZapiszJakoGeoTiff(plik, kopiaBmp, sciezkaCel) Then kopiaBmp.Save(sciezkaCel, ImageFormat.Tiff)
                    Case Else
                        kopiaBmp.Save(sciezkaCel, ImageFormat.Png)
                End Select
            End Using
        End Using
    End Sub

    Private Const JakoscJpeg As Integer = 95
    'pamięć na obrazy przetwarzane równocześnie
    Private Const BudzetPamieci As Long = 1024L * 1024 * 1024

    ''' <summary>
    ''' Liczba obrazów przetwarzanych równocześnie: każdy zajmuje ok. 20 bajtów na piksel (obraz źródłowy, kopia 32-bitowa,
    ''' tablice pikseli korekty) - dawniej tyle obrazów, ile rdzeni, więc duże segmenty wyczerpywały pamięć.
    ''' Liczona dla największego obrazu partii (dawniej dla pierwszego pliku - duży obraz wśród małych segmentów
    ''' przetwarzany był równolegle z innymi ponad budżet pamięci).
    ''' </summary>
    Friend Shared Function LiczbaWatkow(pliki As IList(Of String)) As Integer
        Dim piksele As Long = 1
        For Each plik In pliki
            Try
                Dim n = NaglowekObrazu.Odczytaj(plik)
                piksele = Math.Max(piksele, n.SzerokoscPx * n.WysokoscPx)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException OrElse TypeOf ex Is UnauthorizedAccessException
                piksele = Math.Max(piksele, 4000L * 4000)
            End Try
        Next
        Return CInt(Math.Max(1L, Math.Min(Environment.ProcessorCount, BudzetPamieci \ (piksele * 20))))
    End Function

    ''' <summary>Przedrostek arkusza zapisywanego przez scalanie segmentów (okno Eksport mapy, format Arkusz).</summary>
    Friend Const PrzedrostekArkusza As String = "_scalone_segmenty_"

    ''' <summary>
    ''' Obrazy folderu do korekty: PNG, JPEG i TIFF bez scalonych arkuszy. Arkusz ze scalania leży w folderze sesji
    ''' obok segmentów, jest wielokrotnie większy od segmentu i był poprawiany razem z nimi (groźba braku pamięci).
    ''' </summary>
    Friend Shared Function PlikiDoKorekty(folder As String, ByRef pominieteArkusze As Integer) As List(Of String)
        pominieteArkusze = 0
        Dim wynik As New List(Of String)()
        For Each plik In Directory.GetFiles(folder)
            Dim ext = Path.GetExtension(plik).ToLowerInvariant()
            If Not {".png", ".jpg", ".jpeg", ".tif", ".tiff"}.Contains(ext) Then Continue For
            If Path.GetFileName(plik).StartsWith(PrzedrostekArkusza, StringComparison.OrdinalIgnoreCase) Then
                pominieteArkusze += 1
                Continue For
            End If
            wynik.Add(plik)
        Next
        Return wynik
    End Function

    ''' <summary>
    ''' Zapisuje poprawiony obraz jako GeoTIFF z georeferencją pliku źródłowego. Bitmap.Save zapisuje TIFF bez znaczników
    ''' GeoTIFF, więc poprawiony segment tracił położenie. False - źródło nie ma georeferencji GeoTIFF albo układ jest
    ''' nieznany programowi (wtedy zwykły zapis TIFF; pliki georeferencji obok obrazu kopiuje opcja „kopiuj georeferencję”).
    ''' </summary>
    Private Shared Function ZapiszJakoGeoTiff(zrodlo As String, bmp As Bitmap, cel As String) As Boolean
        Dim naglowek As NaglowekObrazu
        Try
            naglowek = NaglowekObrazu.Odczytaj(zrodlo)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException
            Return False
        End Try
        If Not naglowek.MaGeoreferencje OrElse naglowek.Epsg = 0 Then Return False
        Dim uklad = UkladWspolrzednych.ZKodu(naglowek.Epsg)
        If uklad.Epsg <> naglowek.Epsg Then Return False 'ZKodu zwraca PL-1992 dla nieznanego kodu

        Dim szer = bmp.Width, wys = bmp.Height
        Dim piksele = NakladanieObrazow.PikseleBitmapy(bmp)
        Dim kanaly As Integer = If(piksele.Any(Function(p) ((p >> 24) And &HFF) < 255), 4, 3)
        Using zapis As New ZapisGeoTiff(cel, szer, wys, False, 0, uklad, naglowek.ZasiegGeoTiff(), kanaly)
            Dim wiersz(szer * kanaly - 1) As Byte
            For y = 0 To wys - 1
                For x = 0 To szer - 1
                    Dim c = piksele(y * szer + x), i = x * kanaly
                    wiersz(i) = CByte((c >> 16) And &HFF)
                    wiersz(i + 1) = CByte((c >> 8) And &HFF)
                    wiersz(i + 2) = CByte(c And &HFF)
                    If kanaly = 4 Then wiersz(i + 3) = CByte((c >> 24) And &HFF)
                Next
                zapis.ZapiszWiersze(wiersz, 1)
            Next
            zapis.Zakoncz()
        End Using
        Return True
    End Function

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        _cts?.Cancel()
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        Me.Close()
    End Sub

    ''' <summary>
    ''' Zamknięcie w trakcie przetwarzania (także krzyżykiem) przerywa je, a okno zamyka się po zakończeniu zadania.
    ''' Dawniej krzyżyk nie przerywał pracy: przetwarzanie trwało w tle, a na końcu pojawiał się komunikat zamkniętego okna.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _cts IsNot Nothing Then
            _cts.Cancel()
            _zamknijPoZakonczeniu = True
            lblStatus.Text = "Przerywanie przetwarzania…"
            e.Cancel = True
            Return
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class