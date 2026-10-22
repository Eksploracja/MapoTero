'Copyright (C) <2015>  pajakt
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
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

Public Class FormZaawansowanyMbtiles

    Private ReadOnly _warstwy As New List(Of WarstwaWielowarstwowa)
    Private _cts As CancellationTokenSource
    Private _ctsPodglad As CancellationTokenSource
    Private _srodekGeo As New PunktGeo(52.0, 19.0) ' Domyślnie środek Polski
    Private _wTrakcieLadowania As Boolean = True

    Private Sub FormZaawansowanyMbtiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _wTrakcieLadowania = True
        Me.AllowDrop = True
        dgvWarstwy.AllowDrop = True
        grpWarstwy.AllowDrop = True
        cmbFormat.SelectedIndex = 0 ' SmartHybrid

        ' Domyślna lokalizacja środka mapy
        InicjalizujSrodekZMapyGlownej()

        ' Domyślna ścieżka pliku wynikowego
        Dim domyslnaNazwa As String = "mapa_wielowarstwowa_" & DateTime.Now.ToString("yyyyMMdd_HHmm") & ".mbtiles"
        txtPlikWynikowy.Text = Path.Combine(FolderDownload, domyslnaNazwa)
        txtNazwaMapy.Text = "Mapa wielowarstwowa"

        ' Jeśli w programie jest aktywna sesja pobierania, dodaj ją jako pierwszą warstwę - o ile nie ma jej już w projekcie
        ' (zamknięte okno modalne nie jest niszczone: lista warstw zostaje, a Load wykonuje się przy każdym otwarciu)
        If Not String.IsNullOrEmpty(Form1.FolderSesji) AndAlso File.Exists(Path.Combine(Form1.FolderSesji, "conf.txt")) AndAlso
           Not _warstwy.Any(Function(w) TenSamFolder(w.Folder, Form1.FolderSesji)) Then
            DodajFolderSesji(Form1.FolderSesji)
        End If
        'sortowanie tabeli zmieniało kolejność wierszy bez zmiany kolejności warstw - przyciski Góra/Dół przesuwały złą warstwę
        For Each kolumna As DataGridViewColumn In dgvWarstwy.Columns
            kolumna.SortMode = DataGridViewColumnSortMode.NotSortable
        Next

        _wTrakcieLadowania = False
        AktualizujZakresZoom()
        OdswiezPodgladAsync()
    End Sub

    Private Sub InicjalizujSrodekZMapyGlownej()
        Try
            ' 1. Jeśli są już dodane warstwy z poprawnym zasięgiem, wycentruj na ich środku
            Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(_warstwy)
            If obszar.Poprawny Then
                _srodekGeo = New PunktGeo((obszar.Polnoc + obszar.Poludnie) / 2, (obszar.Zachod + obszar.Wschod) / 2)
                Return
            End If

            ' 2. W przeciwnym razie pobierz środek z bieżącej siatki lub widoku mapy głównej
            Dim form1Instance = TryCast(Application.OpenForms("Form1"), Form1)
            If form1Instance IsNot Nothing Then
                Dim s = form1Instance.BiezacaSiatka
                If s.Poprawna Then
                    Dim srodekUkladu = s.ZasiegSiatki.Srodek
                    _srodekGeo = Ustawienia.Uklad.DoWgs84(srodekUkladu)
                    Return
                End If
            End If
        Catch
        End Try
        _srodekGeo = New PunktGeo(52.065, 19.252)
    End Sub

    ''' <summary>Czy okno zostało zamknięte (praca w tle mogła się zakończyć już po zamknięciu okna).</summary>
    Private ReadOnly Property Zamkniete As Boolean
        Get
            Return IsDisposed OrElse Disposing
        End Get
    End Property

    ''' <summary>Czy ścieżki wskazują ten sam folder (bez względu na wielkość liter i końcowy ukośnik).</summary>
    Private Shared Function TenSamFolder(a As String, b As String) As Boolean
        Try
            Return String.Equals(Path.GetFullPath(a).TrimEnd("\"c, "/"c), Path.GetFullPath(b).TrimEnd("\"c, "/"c), StringComparison.OrdinalIgnoreCase)
        Catch
            Return False
        End Try
    End Function

    ''' <summary>Warstwa zaznaczonego wiersza tabeli i jej położenie na liście warstw (-1 - brak).</summary>
    Private Function IndeksZaznaczonejWarstwy() As Integer
        If dgvWarstwy.CurrentRow Is Nothing Then Return -1
        Return _warstwy.IndexOf(TryCast(dgvWarstwy.CurrentRow.Tag, WarstwaWielowarstwowa))
    End Function

    ''' <summary>Kopia listy warstw dla pracy w tle - lista okna może się w tym czasie zmienić.</summary>
    Private Function KopiaWarstw() As List(Of WarstwaWielowarstwowa)
        Return New List(Of WarstwaWielowarstwowa)(_warstwy)
    End Function

    Private Sub Loguj(tekst As String, Optional kolor As Color = Nothing)
        If Zamkniete Then Exit Sub
        If kolor = Nothing Then kolor = Color.Black
        txtLog.SelectionStart = txtLog.TextLength
        txtLog.SelectionLength = 0
        txtLog.SelectionColor = kolor
        txtLog.AppendText("[" & DateTime.Now.ToString("HH:mm:ss") & "] " & tekst & vbCrLf)
        txtLog.ScrollToCaret()
    End Sub

    ''' <summary>Dodaje sesję z podanego folderu i oblicza sugerowane poziomy zoom.</summary>
    Private Sub DodajFolderSesji(folder As String)
        If Not File.Exists(Path.Combine(folder, "conf.txt")) Then
            Dim podkatalogi As New List(Of String)
            Try
                For Each pod In Directory.GetDirectories(folder)
                    If File.Exists(Path.Combine(pod, "conf.txt")) Then podkatalogi.Add(pod)
                Next
            Catch
            End Try
            If podkatalogi.Count > 0 Then
                Dim odp = MessageBox.Show("W wybranym folderze znaleziono " & podkatalogi.Count & " warstw mapowych:" & vbCrLf &
                                          String.Join(vbCrLf, podkatalogi.Select(Function(p) "• " & Path.GetFileName(p))) & vbCrLf & vbCrLf &
                                          "Czy chcesz dodać je wszystkie do projektu wielowarstwowego?",
                                          "Wykryto sesje w podfolderach", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If odp = DialogResult.Yes Then
                    For Each pod In podkatalogi
                        DodajFolderSesji(pod)
                    Next
                    Return
                End If
            End If
        End If

        Dim w As New WarstwaWielowarstwowa With {.Folder = folder}
        Dim blad As String = ""
        If Not w.WczytajSesje(blad) Then
            Loguj("Nie udało się załadować sesji z folderu " & folder & ": " & blad, Color.Red)
            MessageBox.Show(blad, "Błąd warstwy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Sugerowane poziomy zoom na podstawie rozmiaru piksela
        Dim srodek = w.ZasiegWgs84
        Dim srodekSz As Double = (srodek.Polnoc + srodek.Poludnie) / 2
        Dim mX As Double, mY As Double
        Georeferencja.MetrowNaStopien(srodekSz, mX, mY)
        Dim pikselMetry As Double = If(w.Uklad.Geograficzny, w.Siatka.RozmiarPiksela * mX, w.Siatka.RozmiarPiksela)

        Dim zoomNat As Integer = WebMercator.PoziomDlaRozdzielczosci(pikselMetry, srodekSz)
        w.PoziomMax = Math.Min(21, Math.Max(1, zoomNat))
        w.PoziomMin = Math.Max(1, w.PoziomMax - 3)
        w.Kolejnosc = _warstwy.Count + 1

        ' Domyślne przezroczyste białe tło dla warstw z przezroczystością lub nakładek
        If w.Konfiguracja IsNot Nothing AndAlso w.Konfiguracja.Przezroczystosc Then
            w.BialePrzezroczyste = True
        End If

        _warstwy.Add(w)
        PrzeliczKolejnoscWarstw()
        OdswiezTabeleWarstw()
        AktualizujZakresZoom()
        Loguj("Dodano warstwę: " & w.Tytul & " (poziomy zoom " & w.PoziomMin & " - " & w.PoziomMax & ")", Color.DarkGreen)
        OdswiezPodgladAsync()
    End Sub

    ''' <summary>
    ''' W standardzie GIS (QGIS, Photoshop) góra tabeli (indeks 0) to wierzch stosu (rysowany jako ostatni, na wierzchu),
    ''' a dół tabeli to spód / podkład (rysowany jako pierwszy).
    ''' Im wyższa wartość Kolejnosc, tym później (wyżej) warstwa jest rysowana.
    ''' </summary>
    Private Sub PrzeliczKolejnoscWarstw()
        For i = 0 To _warstwy.Count - 1
            _warstwy(i).Kolejnosc = _warstwy.Count - i
        Next
    End Sub

    Private Sub OdswiezTabeleWarstw()
        dgvWarstwy.Rows.Clear()
        For Each w In _warstwy
            Dim trybStr = TrybMieszaniaRozszerzenia.NazwaWyswietlana(w.TrybMieszania)
            Dim idx = dgvWarstwy.Rows.Add(w.Wlaczona, w.Tytul, w.PoziomMin, w.PoziomMax, CInt(w.Krycie * 100), trybStr, w.OdcienieSzarosci, w.BialePrzezroczyste, w.Folder)
            dgvWarstwy.Rows(idx).Tag = w
        Next
    End Sub

    Private Sub AktualizujZakresZoom()
        Dim minZ As Integer = 22
        Dim maxZ As Integer = 0
        Dim maWlaczone As Boolean = False

        For Each w In _warstwy
            If w.Wlaczona Then
                minZ = Math.Min(minZ, w.PoziomMin)
                maxZ = Math.Max(maxZ, w.PoziomMax)
                maWlaczone = True
            End If
        Next

        If Not maWlaczone Then
            minZ = 8
            maxZ = 18
        End If

        tbZoom.Minimum = Math.Max(1, minZ)
        tbZoom.Maximum = Math.Min(22, Math.Max(tbZoom.Minimum, maxZ))
        If tbZoom.Value < tbZoom.Minimum Then tbZoom.Value = tbZoom.Minimum
        If tbZoom.Value > tbZoom.Maximum Then tbZoom.Value = tbZoom.Maximum
        lblZoomWartosc.Text = tbZoom.Value.ToString()
    End Sub

    Private Sub btnDodajSesje_Click(sender As Object, e As EventArgs) Handles btnDodajSesje.Click
        dlgFolder.SelectedPath = FolderDownload
        dlgFolder.Description = "Wskaż folder z pobraną warstwą mapy (zawierający plik conf.txt i segmenty):"
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            DodajFolderSesji(dlgFolder.SelectedPath)
        End If
    End Sub

    Private Sub btnUsunSesje_Click(sender As Object, e As EventArgs) Handles btnUsunSesje.Click
        If dgvWarstwy.CurrentRow Is Nothing Then Exit Sub
        Dim w = TryCast(dgvWarstwy.CurrentRow.Tag, WarstwaWielowarstwowa)
        If w IsNot Nothing Then
            _warstwy.Remove(w)
            PrzeliczKolejnoscWarstw()
            OdswiezTabeleWarstw()
            AktualizujZakresZoom()
            OdswiezPodgladAsync()
        End If
    End Sub

    Private Sub btnGora_Click(sender As Object, e As EventArgs) Handles btnGora.Click
        Dim idx = IndeksZaznaczonejWarstwy()
        If idx > 0 Then
            Dim w = _warstwy(idx)
            _warstwy.RemoveAt(idx)
            _warstwy.Insert(idx - 1, w)
            PrzeliczKolejnoscWarstw()
            OdswiezTabeleWarstw()
            dgvWarstwy.CurrentCell = dgvWarstwy.Rows(idx - 1).Cells(0)
            dgvWarstwy.Rows(idx - 1).Selected = True
            OdswiezPodgladAsync()
        End If
    End Sub

    Private Sub btnDol_Click(sender As Object, e As EventArgs) Handles btnDol.Click
        Dim idx = IndeksZaznaczonejWarstwy()
        If idx >= 0 AndAlso idx < _warstwy.Count - 1 Then
            Dim w = _warstwy(idx)
            _warstwy.RemoveAt(idx)
            _warstwy.Insert(idx + 1, w)
            PrzeliczKolejnoscWarstw()
            OdswiezTabeleWarstw()
            dgvWarstwy.CurrentCell = dgvWarstwy.Rows(idx + 1).Cells(0)
            dgvWarstwy.Rows(idx + 1).Selected = True
            OdswiezPodgladAsync()
        End If
    End Sub

    Private Sub dgvWarstwy_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvWarstwy.CellValueChanged
        If _wTrakcieLadowania OrElse e.RowIndex < 0 Then Exit Sub
        Dim row = dgvWarstwy.Rows(e.RowIndex)
        Dim w = TryCast(row.Tag, WarstwaWielowarstwowa)
        If w Is Nothing Then Exit Sub

        Try
            w.Wlaczona = CBool(row.Cells(colWlaczona.Index).Value)
            w.Tytul = CStr(row.Cells(colTytul.Index).Value)
            w.PoziomMin = Math.Max(0, Math.Min(22, CInt(row.Cells(colPoziomMin.Index).Value)))
            w.PoziomMax = Math.Max(w.PoziomMin, Math.Min(22, CInt(row.Cells(colPoziomMax.Index).Value)))
            Dim krycieProc = Math.Max(0, Math.Min(100, CInt(row.Cells(colKrycie.Index).Value)))
            w.Krycie = krycieProc / 100.0
            Dim trybVal = TryCast(row.Cells(colTryb.Index).Value, String)
            If Not String.IsNullOrEmpty(trybVal) Then
                w.TrybMieszania = TrybMieszaniaRozszerzenia.ZTekstu(trybVal)
            End If
            w.OdcienieSzarosci = CBool(row.Cells(colSzare.Index).Value)
            w.BialePrzezroczyste = CBool(row.Cells(colBiale.Index).Value)
            If w.Zrodlo IsNot Nothing Then
                w.Zrodlo.OdcienieSzarosci = w.OdcienieSzarosci
                w.Zrodlo.BialePrzezroczyste = w.BialePrzezroczyste
            End If
        Catch
        End Try

        AktualizujZakresZoom()
        OdswiezPodgladAsync()
    End Sub

    Private Sub dgvWarstwy_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles dgvWarstwy.CurrentCellDirtyStateChanged
        If dgvWarstwy.IsCurrentCellDirty Then
            dgvWarstwy.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub tbZoom_Scroll(sender As Object, e As EventArgs) Handles tbZoom.Scroll
        lblZoomWartosc.Text = tbZoom.Value.ToString()
        OdswiezPodgladAsync()
    End Sub

    Private Sub btnCentrumMapy_Click(sender As Object, e As EventArgs) Handles btnCentrumMapy.Click
        InicjalizujSrodekZMapyGlownej()
        Loguj("Zaktualizowano punkt podglądu do: " & _srodekGeo.Szerokosc.ToString("0.0000") & "° N, " &
              _srodekGeo.Dlugosc.ToString("0.0000") & "° E", Color.Blue)
        OdswiezPodgladAsync()
    End Sub

    ''' <summary>Renderuje kafel podglądu w tle i aktualizuje PictureBox.</summary>
    Private Async Sub OdswiezPodgladAsync()
        If _wTrakcieLadowania Then Exit Sub
        _ctsPodglad?.Cancel()
        _ctsPodglad = New CancellationTokenSource()
        Dim token = _ctsPodglad.Token

        Dim z = tbZoom.Value
        lblZoomWartosc.Text = z.ToString()

        Dim aktywne = KompilatorMbtiles.WarstwyDlaPoziomu(_warstwy, z)
        Dim opisWarstw As String = ""
        If aktywne.Count > 1 Then
            Dim podklad = aktywne(0)
            Dim nakladki = aktywne.Skip(1).ToList()
            Dim opisPodkladu = podklad.Tytul & " (" & CInt(podklad.Krycie * 100) & "%" & If(podklad.OdcienieSzarosci, ", szarość", "") & ")"
            Dim opisNakladek = String.Join(", ", nakladki.Select(Function(w)
                                                                     Dim cechy As New List(Of String) From {CInt(w.Krycie * 100) & "%"}
                                                                     If w.TrybMieszania <> TrybMieszania.Zwykly Then cechy.Add(TrybMieszaniaRozszerzenia.NazwaWyswietlana(w.TrybMieszania))
                                                                     If w.OdcienieSzarosci Then cechy.Add("szarość")
                                                                     Return w.Tytul & " (" & String.Join(", ", cechy) & ")"
                                                                 End Function))
            opisWarstw = "Podkład: " & opisPodkladu & " → Wierzch: " & opisNakladek
        ElseIf aktywne.Count = 1 Then
            Dim w0 = aktywne(0)
            Dim cechy As New List(Of String) From {CInt(w0.Krycie * 100) & "%"}
            If w0.TrybMieszania <> TrybMieszania.Zwykly Then cechy.Add(TrybMieszaniaRozszerzenia.NazwaWyswietlana(w0.TrybMieszania))
            If w0.OdcienieSzarosci Then cechy.Add("szarość")
            opisWarstw = w0.Tytul & " (" & String.Join(", ", cechy) & ")"
        ElseIf chkWypelnijLuki.Checked Then
            opisWarstw = "(Wypełnienie przeskalowaną warstwą niższą)"
        Else
            opisWarstw = "(Brak warstwy na poziomie Zoom " & z & " - kafel pusty)"
        End If

        Dim pikselMetry = WebMercator.RozmiarPiksela(_srodekGeo.Szerokosc, z)
        lblPodgladInfo.Text = "Poziom " & z & " (piksel ~" & pikselMetry.ToString("0.00") & " m)" & vbCrLf & opisWarstw

        Dim opcje = PobierzOpcjeZFormularza()
        Dim warstwy = KopiaWarstw()
        Dim srodek = _srodekGeo

        Try
            Await Task.Delay(40, token)
            Dim bmp As Bitmap = Await Task.Run(Function()
                                                   Return KompilatorMbtiles.RenderujPodgladKafla(warstwy, srodek.Szerokosc, srodek.Dlugosc, z, opcje)
                                               End Function, token)
            If token.IsCancellationRequested OrElse Zamkniete Then
                bmp?.Dispose()
                Exit Sub
            End If

            Dim staryObraz = picPodglad.Image
            If bmp IsNot Nothing Then
                picPodglad.Image = bmp
            Else
                Dim pusta As New Bitmap(256, 256)
                Using g = Graphics.FromImage(pusta)
                    g.Clear(Color.WhiteSmoke)
                    Using p As New Pen(Color.LightGray)
                        g.DrawLine(p, 0, 0, 256, 256)
                        g.DrawLine(p, 0, 256, 256, 0)
                    End Using
                    Using f As New Font("Microsoft Sans Serif", 9.0!, FontStyle.Regular)
                        g.DrawString("Brak danych na tym powiększeniu", f, Brushes.Gray, New RectangleF(10, 110, 236, 40))
                    End Using
                End Using
                picPodglad.Image = pusta
            End If
            staryObraz?.Dispose()
        Catch ex As OperationCanceledException
            ' Anulowanie jest oczekiwane przy szybkiej zmianie parametrów
        Catch ex As Exception
            Loguj("Błąd renderowania podglądu: " & ex.Message, Color.Red)
        End Try
    End Sub

    Private Function PobierzOpcjeZFormularza() As OpcjeKompilatoraMbtiles
        Dim opcje As New OpcjeKompilatoraMbtiles With {
            .PlikDocelowy = txtPlikWynikowy.Text.Trim(),
            .NazwaMapy = txtNazwaMapy.Text.Trim(),
            .JakoscJpeg = CInt(nudJakosc.Value),
            .SzybkaPiramidaWPasmach = chkPiramida.Checked,
            .WypelnijLukiWarstwaNizsza = chkWypelnijLuki.Checked,
            .PoziomMin = tbZoom.Minimum,
            .PoziomMax = tbZoom.Maximum
        }

        Select Case cmbFormat.SelectedIndex
            Case 1 : opcje.FormatKafli = FormatKafliMbtiles.TylkoJpeg
            Case 2 : opcje.FormatKafli = FormatKafliMbtiles.TylkoPng
            Case Else : opcje.FormatKafli = FormatKafliMbtiles.SmartHybrid
        End Select

        Return opcje
    End Function

    Private Sub btnWybierzPlik_Click(sender As Object, e As EventArgs) Handles btnWybierzPlik.Click
        dlgSave.Filter = "Pliki MBTiles (*.mbtiles)|*.mbtiles|Wszystkie pliki (*.*)|*.*"
        dlgSave.FileName = Path.GetFileName(txtPlikWynikowy.Text)
        dlgSave.InitialDirectory = Path.GetDirectoryName(txtPlikWynikowy.Text)
        If dlgSave.ShowDialog(Me) = DialogResult.OK Then
            txtPlikWynikowy.Text = dlgSave.FileName
        End If
    End Sub

    Private Async Sub btnOszacuj_Click(sender As Object, e As EventArgs) Handles btnOszacuj.Click
        If _warstwy.Where(Function(w) w.Wlaczona).Count() = 0 Then
            MessageBox.Show("Dodaj i włącz co najmniej jedną warstwę do projektu.", "Brak warstw", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        btnOszacuj.Enabled = False
        lblSzacunekPodsumowanie.Text = "Trwa szacowanie rozmiaru..."
        lblStatusFat32.Text = "Próbkowanie poziomów powiększeń..."

        Dim opcje = PobierzOpcjeZFormularza()
        Try
            Dim szacunek = Await KompilatorMbtiles.OszacujRozmiarAsync(KopiaWarstw(), opcje, CancellationToken.None)
            If Zamkniete Then Exit Sub
            lvwPoziomy.Items.Clear()

            For Each kv In szacunek.Poziomy
                Dim p = kv.Value
                Dim itm As New ListViewItem("Z" & p.Poziom)
                itm.SubItems.Add(p.LiczbaKafliZDanymi.ToString("N0"))
                itm.SubItems.Add(SzacunekRozmiaru.Tekst(p.RozmiarPoziomuBajty))
                itm.SubItems.Add(p.AktywneWarstwyTekst)
                lvwPoziomy.Items.Add(itm)
            Next

            lblSzacunekPodsumowanie.Text = "Łącznie kafli: " & szacunek.LacznaLiczbaKafli.ToString("N0") & vbCrLf &
                                           "Szacowany rozmiar: " & szacunek.TekstRozmiaru()

            If szacunek.CzyPrzekraczaFat32 Then
                lblStatusFat32.ForeColor = Color.Red
                lblStatusFat32.Text = "UWAGA: Plik > 4 GB! Nie zmieści się na kartach SD w formacie FAT32."
            ElseIf szacunek.LacznyRozmiarBajty > 2147483648L Then
                lblStatusFat32.ForeColor = Color.DarkOrange
                lblStatusFat32.Text = "Rozmiar powyżej 2 GB. Zgodny z OruxMaps i Locus Map."
            Else
                lblStatusFat32.ForeColor = Color.DarkGreen
                lblStatusFat32.Text = "Rozmiar optymalny dla urządzeń mobilnych (Locus Map, OruxMaps)."
            End If

            Loguj("Oszacowano parametry pliku: " & szacunek.LacznaLiczbaKafli.ToString("N0") & " kafli, " &
                  szacunek.TekstRozmiaru(), Color.Blue)
        Catch ex As Exception
            Loguj("Błąd szacowania rozmiaru: " & ex.Message, Color.Red)
        Finally
            If Not Zamkniete Then btnOszacuj.Enabled = _cts Is Nothing
        End Try
    End Sub

    Private Async Sub btnKompiluj_Click(sender As Object, e As EventArgs) Handles btnKompiluj.Click
        Dim opcje = PobierzOpcjeZFormularza()

        If String.IsNullOrWhiteSpace(opcje.PlikDocelowy) Then
            MessageBox.Show("Podaj ścieżkę pliku wynikowego .mbtiles.", "Brak pliku docelowego", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPlikWynikowy.Focus()
            Exit Sub
        End If

        If _warstwy.Where(Function(w) w.Wlaczona).Count() = 0 Then
            MessageBox.Show("Włącz co najmniej jedną warstwę do kompilacji.", "Brak aktywnych warstw", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        ' Sprawdzenie luk w zoomie
        Dim luki = KompilatorMbtiles.SprawdzLukiPoziomow(_warstwy, opcje.PoziomMin, opcje.PoziomMax)
        If luki.Count > 0 AndAlso Not opcje.WypelnijLukiWarstwaNizsza Then
            Dim odp = MessageBox.Show("W projekcie wykryto poziomy zoom bez przypisanej warstwy: " &
                                      String.Join(", ", luki) & "." & vbCrLf & vbCrLf &
                                      "Czy chcesz automatycznie wypełnić te luki przeskalowaną warstwą niższą?" & vbCrLf &
                                      "(Kliknij 'Tak', aby wypełnić luki, lub 'Nie', aby pozostawić te poziomy puste).",
                                      "Wykryto luki w zoomie", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If odp = DialogResult.Cancel Then Exit Sub
            If odp = DialogResult.Yes Then
                opcje.WypelnijLukiWarstwaNizsza = True
                chkWypelnijLuki.Checked = True
            End If
        End If

        ' Sprawdzenie wolnego miejsca na dysku docelowym
        Dim ostrzezenieMiejsca As String = ""
        If Not SprawdzMiejsceNaDysku(opcje.PlikDocelowy, ostrzezenieMiejsca) Then
            MessageBox.Show(ostrzezenieMiejsca, "Brak miejsca na dysku", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        ElseIf Not String.IsNullOrEmpty(ostrzezenieMiejsca) Then
            Dim odpM = MessageBox.Show(ostrzezenieMiejsca, "Ostrzeżenie o małej ilości miejsca", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If odpM <> DialogResult.Yes Then Exit Sub
        End If

        UstawStanKompilacji(True)
        _cts = New CancellationTokenSource()
        Dim token = _cts.Token

        Dim postep As New Progress(Of RaportPostepuKompilacji)(Sub(r)
                                                                   If Zamkniete Then Exit Sub
                                                                   prgPostep.Value = Math.Max(0, Math.Min(100, r.Procent))
                                                                   If Not String.IsNullOrEmpty(r.Komunikat) Then
                                                                       Loguj(r.Komunikat)
                                                                   End If
                                                               End Sub)

        Loguj("Rozpoczęto kompilację wielowarstwowego MBTiles do: " & opcje.PlikDocelowy, Color.DarkGreen)
        Dim st = Diagnostics.Stopwatch.StartNew()

        Try
            Dim plik As String = Await KompilatorMbtiles.KompilujAsync(KopiaWarstw(), opcje, postep, token)
            st.Stop()
            If Zamkniete Then Exit Sub
            Loguj("SUKCES: Utworzono plik MBTiles w czasie: " & st.Elapsed.ToString("mm\:ss"), Color.DarkGreen)
            'plik docelowy otwarty w innym programie - wynik zapisany pod inną nazwą
            Dim innaNazwa As Boolean = Not String.Equals(plik, opcje.PlikDocelowy, StringComparison.OrdinalIgnoreCase)
            If innaNazwa Then txtPlikWynikowy.Text = plik

            Dim finInfo = New FileInfo(plik)
            Dim rozmiarMb = finInfo.Length / 1024.0 / 1024.0
            Dim odp = MessageBox.Show("Plik MBTiles został pomyślnie utworzony!" & vbCrLf & vbCrLf &
                                      If(innaNazwa, "Plik " & opcje.PlikDocelowy & " jest otwarty w innym programie, dlatego mapę zapisano pod inną nazwą." & vbCrLf, "") &
                                      "Plik: " & plik & vbCrLf &
                                      "Rozmiar: " & rozmiarMb.ToString("0.0") & " MB" & vbCrLf &
                                      "Czas: " & st.Elapsed.ToString("mm\:ss") & vbCrLf & vbCrLf &
                                      "Czy chcesz otworzyć folder i zaznaczyć gotowy plik w Eksploratorze Windows?",
                                      "Kompilacja zakończona sukcesem", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
            If odp = DialogResult.Yes Then
                OtworzIZaznaczPlik(plik)
            End If
        Catch ex As OperationCanceledException
            Loguj("Kompilacja została przerwana przez użytkownika.", Color.DarkOrange)
        Catch ex As Exception
            If Not Zamkniete Then
                Loguj("BŁĄD podczas kompilacji: " & ex.Message, Color.Red)
                MessageBox.Show("Wystąpił błąd podczas tworzenia MBTiles:" & vbCrLf & ex.Message, "Błąd kompilacji", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            _cts?.Dispose()
            _cts = Nothing
            'okno mogło zostać zamknięte w trakcie kompilacji (przerwanej przy zamykaniu)
            If Not Zamkniete Then UstawStanKompilacji(False)
        End Try
    End Sub

    Private Sub UstawStanKompilacji(wTrakcie As Boolean)
        btnKompiluj.Enabled = Not wTrakcie
        btnPrzerwij.Enabled = wTrakcie
        btnDodajSesje.Enabled = Not wTrakcie
        btnUsunSesje.Enabled = Not wTrakcie
        btnGora.Enabled = Not wTrakcie
        btnDol.Enabled = Not wTrakcie
        dgvWarstwy.Enabled = Not wTrakcie
        grpUstawienia.Enabled = Not wTrakcie
        btnOszacuj.Enabled = Not wTrakcie
        btnZamknij.Enabled = Not wTrakcie
        If Not wTrakcie Then
            prgPostep.Value = 0
        End If
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        If _cts IsNot Nothing AndAlso Not _cts.IsCancellationRequested Then
            btnPrzerwij.Enabled = False
            _cts.Cancel()
            Loguj("Żądanie przerwania kompilacji...")
        End If
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        Close()
    End Sub

    Private Sub FormZaawansowanyMbtiles_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _cts IsNot Nothing Then
            Dim odp = MessageBox.Show("Kompilacja jest w toku. Czy na pewno chcesz przerwać i zamknąć okno?", "Trwa kompilacja",
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If odp = DialogResult.Yes Then
                _cts.Cancel()
            Else
                e.Cancel = True
                Exit Sub
            End If
        End If
        _ctsPodglad?.Cancel()
    End Sub

    Private Sub btnPomoc_Click(sender As Object, e As EventArgs) Handles btnPomoc.Click
        PokazPomoc()
    End Sub

    Private Sub PokazPomoc()
        Using dlg As New FormPomocMbtiles()
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub FormZaawansowanyMbtiles_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F1 Then
            PokazPomoc()
            e.Handled = True
        End If
    End Sub


    Private Sub dgvWarstwy_KeyDown(sender As Object, e As KeyEventArgs) Handles dgvWarstwy.KeyDown
        If e.KeyCode = Keys.Delete Then
            btnUsunSesje.PerformClick()
            e.Handled = True
        End If
    End Sub

    Private Sub chkWypelnijLuki_CheckedChanged(sender As Object, e As EventArgs) Handles chkWypelnijLuki.CheckedChanged
        If Not _wTrakcieLadowania Then OdswiezPodgladAsync()
    End Sub

    Private Sub chkPiramida_CheckedChanged(sender As Object, e As EventArgs) Handles chkPiramida.CheckedChanged
        If Not _wTrakcieLadowania Then OdswiezPodgladAsync()
    End Sub

    Private Sub cmbFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFormat.SelectedIndexChanged
        If Not _wTrakcieLadowania Then OdswiezPodgladAsync()
    End Sub

    Private Sub picPodglad_MouseEnter(sender As Object, e As EventArgs) Handles picPodglad.MouseEnter
        picPodglad.Focus()
    End Sub

    Private Sub picPodglad_MouseWheel(sender As Object, e As MouseEventArgs) Handles picPodglad.MouseWheel
        If e.Delta > 0 Then
            If tbZoom.Value < tbZoom.Maximum Then
                tbZoom.Value += 1
                tbZoom_Scroll(sender, EventArgs.Empty)
            End If
        ElseIf e.Delta < 0 Then
            If tbZoom.Value > tbZoom.Minimum Then
                tbZoom.Value -= 1
                tbZoom_Scroll(sender, EventArgs.Empty)
            End If
        End If
    End Sub

    Private _przeciaganiePodgladu As Boolean = False
    Private _punktPoczatkowy As Point

    Private Sub picPodglad_MouseDown(sender As Object, e As MouseEventArgs) Handles picPodglad.MouseDown
        If e.Button = MouseButtons.Left Then
            _przeciaganiePodgladu = True
            _punktPoczatkowy = e.Location
            picPodglad.Cursor = Cursors.SizeAll
        End If
    End Sub

    Private Sub picPodglad_MouseUp(sender As Object, e As MouseEventArgs) Handles picPodglad.MouseUp
        If _przeciaganiePodgladu Then
            _przeciaganiePodgladu = False
            picPodglad.Cursor = Cursors.Default
            Dim dx = e.X - _punktPoczatkowy.X
            Dim dy = e.Y - _punktPoczatkowy.Y
            If dx <> 0 OrElse dy <> 0 Then
                Dim z = tbZoom.Value
                Dim pikselX = WebMercator.PikselX(_srodekGeo.Dlugosc, z) - dx
                Dim pikselY = WebMercator.PikselY(_srodekGeo.Szerokosc, z) - dy
                Dim nowaDl = (pikselX / (256.0 * (1 << z))) * 360.0 - 180.0
                Dim nowaSzRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * pikselY / (256.0 * (1 << z)))))
                Dim nowaSz = nowaSzRad * 180.0 / Math.PI
                _srodekGeo = New PunktGeo(nowaSz, nowaDl)
                OdswiezPodgladAsync()
            End If
        End If
    End Sub


    Private Sub FormZaawansowanyMbtiles_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter, dgvWarstwy.DragEnter, grpWarstwy.DragEnter
        'w trakcie kompilacji lista warstw nie może się zmieniać
        If _cts Is Nothing AndAlso e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub FormZaawansowanyMbtiles_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop, dgvWarstwy.DragDrop, grpWarstwy.DragDrop
        If _cts Is Nothing AndAlso e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim sciezki = TryCast(e.Data.GetData(DataFormats.FileDrop), String())
            If sciezki IsNot Nothing Then
                For Each s In sciezki
                    If Directory.Exists(s) Then
                        DodajFolderSesji(s)
                    ElseIf File.Exists(s) AndAlso Path.GetFileName(s).Equals("conf.txt", StringComparison.OrdinalIgnoreCase) Then
                        DodajFolderSesji(Path.GetDirectoryName(s))
                    End If
                Next
            End If
        End If
    End Sub

    Private Sub btnPokazWFolderze_Click(sender As Object, e As EventArgs) Handles btnPokazWFolderze.Click
        Dim plik = txtPlikWynikowy.Text.Trim()
        OtworzIZaznaczPlik(plik)
    End Sub

    ''' <summary>Sprawdza czy na dysku docelowym jest wystarczająca ilość wolnego miejsca.</summary>
    Private Function SprawdzMiejsceNaDysku(sciezkaPliku As String, ByRef ostrzezenie As String) As Boolean
        ostrzezenie = ""
        Try
            Dim pelnaSciezka = Path.GetFullPath(sciezkaPliku)
            Dim folder = Path.GetDirectoryName(pelnaSciezka)
            Dim literaDysku = Path.GetPathRoot(folder)
            If String.IsNullOrEmpty(literaDysku) Then Return True

            Dim di As New DriveInfo(literaDysku)
            If Not di.IsReady Then Return True

            Dim wolneBajt = di.AvailableFreeSpace

            Dim lacznieKafli As Long = 0
            Dim minZ As Integer = tbZoom.Minimum
            Dim maxZ As Integer = tbZoom.Maximum
            Dim obszar = KompilatorMbtiles.WyznaczObszarWgs84(_warstwy)
            If obszar.Poprawny Then
                For p = minZ To maxZ
                    Dim aktywne = KompilatorMbtiles.WarstwyDlaPoziomu(_warstwy, p)
                    If aktywne.Count > 0 OrElse chkWypelnijLuki.Checked Then
                        lacznieKafli += WebMercator.LiczbaKafli(obszar, p)
                    End If
                Next
            End If

            Dim szacowanyRozmiar As Long = Math.Max(10485760L, lacznieKafli * 40000L)
            Dim wymaganeMiejsce As Long = CLng(szacowanyRozmiar * 1.5)

            If wolneBajt < 52428800L Then ' Poniżej 50 MB
                ostrzezenie = "Na dysku " & literaDysku & " pozostało skrajnie mało wolnego miejsca: " &
                              SzacunekRozmiaru.Tekst(wolneBajt) & "!" & vbCrLf &
                              "Kompilacja nie może zostać uruchomiona ze względu na ryzyko awarii systemu." & vbCrLf &
                              "Zwolnij miejsce na dysku lub wskaż inny dysk docelowy."
                Return False
            End If

            If wolneBajt < wymaganeMiejsce Then
                ostrzezenie = "Na dysku docelowym (" & literaDysku & ") wykryto mało wolnego miejsca:" & vbCrLf &
                              "• Dostępne na dysku: " & SzacunekRozmiaru.Tekst(wolneBajt) & vbCrLf &
                              "• Szacowany rozmiar pliku: ~" & SzacunekRozmiaru.Tekst(szacowanyRozmiar) & vbCrLf &
                              "• Bezpieczny zapas z bazą tymczasową: ~" & SzacunekRozmiaru.Tekst(wymaganeMiejsce) & vbCrLf & vbCrLf &
                              "Czy mimo to chcesz kontynuować kompilację?"
                Return True
            End If
        Catch
        End Try
        Return True
    End Function

    Private Shared Sub OtworzIZaznaczPlik(sciezka As String)
        Try
            If File.Exists(sciezka) Then
                Process.Start(New ProcessStartInfo("explorer.exe", "/select,""" & sciezka & """") With {.UseShellExecute = True})
            Else
                Dim folder = Path.GetDirectoryName(sciezka)
                If Directory.Exists(folder) Then
                    Process.Start(New ProcessStartInfo("explorer.exe", folder) With {.UseShellExecute = True})
                Else
                    MessageBox.Show("Folder ani plik docelowy jeszcze nie istnieje.", "Folder pliku", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

End Class
