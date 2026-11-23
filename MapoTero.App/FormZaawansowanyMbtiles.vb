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
    Private _ctsOszacuj As CancellationTokenSource
    Private _srodekGeo As New PunktGeo(52.0, 19.0) ' Domyślnie środek Polski
    Private _wTrakcieLadowania As Boolean = True
    Private _opcjeKorekty As New OpcjeKorekcjiMip()

    Private _trybKreatora As Boolean = False
    Private _zestawKreatora As ZestawWarstw
    Private _silnikKreatora As PodgladWarstwSilnik = Nothing
    Private _zasiegKreatora As Zasieg
    Private _probkaBmpKreatora As Bitmap
    Private _wybranyFormat As RodzajEksportu = RodzajEksportu.MBTiles

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property WybranyFormat As RodzajEksportu
        Get
            Return _wybranyFormat
        End Get
        Set(value As RodzajEksportu)
            _wybranyFormat = If(value = RodzajEksportu.GeoPackage, RodzajEksportu.GeoPackage, RodzajEksportu.MBTiles)
            If cmbTypBazy IsNot Nothing AndAlso cmbTypBazy.Items.Count >= 2 Then
                cmbTypBazy.SelectedIndex = If(_wybranyFormat = RodzajEksportu.GeoPackage, 1, 0)
            End If
            AktualizujTytulOkna()
        End Set
    End Property

    Private Sub AktualizujTytulOkna()
        If Not _trybKreatora Then
            Dim nazwa = If(_wybranyFormat = RodzajEksportu.GeoPackage, "GeoPackage", "MBTiles")
            Me.Text = $"Zaawansowany eksport wielowarstwowy ({nazwa})"
            If grpUstawienia IsNot Nothing Then grpUstawienia.Text = $"Parametry pliku {nazwa} i kompresji"
        End If
    End Sub

    Private Sub FormZaawansowanyMbtiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CancelButton = btnZamknij
        PomocProgramu.DopasujDoEkranu(Me)
        _wTrakcieLadowania = True
        Me.AllowDrop = True
        dgvWarstwy.AllowDrop = True
        grpWarstwy.AllowDrop = True
        cmbFormat.SelectedIndex = 0 ' SmartHybrid
        cmbTypBazy.SelectedIndex = If(_wybranyFormat = RodzajEksportu.GeoPackage, 1, 0)
        AktualizujTytulOkna()

        If Not _trybKreatora Then
            _opcjeKorekty = Ustawienia.OpcjeKorekty(ProfilKorektyRastra.GarminMip)
            chkKorekta.Checked = _opcjeKorekty.Wlaczona
            AktualizujPrzyciskKorekty()

            ' Domyślna ścieżka pliku wynikowego
            Dim ext As String = If(_wybranyFormat = RodzajEksportu.GeoPackage, ".gpkg", ".mbtiles")
            Dim domyslnaNazwa As String = "mapa_wielowarstwowa_" & DateTime.Now.ToString("yyyyMMdd_HHmm") & ext
            txtPlikWynikowy.Text = Path.Combine(FolderDownload, domyslnaNazwa)
            txtNazwaMapy.Text = "Mapa wielowarstwowa"

            ' Na wstępie wczytaj wszystkie katalogi z mapami w download (oraz bieżącą sesję)
            WczytajWszystkieKatalogiDownload()

            ' Domyślna lokalizacja środka mapy (po wczytaniu warstw wyznacza środek z ich zasięgu)
            InicjalizujSrodekZMapyGlownej()
        Else
            Me.Text = "MBTiles wieloskalowy - dopasowanie warstw i powiększeń kreatora"
            btnZastosujKreator.Visible = True
            btnZamknij.Text = "Anuluj"
            ' Pobieranie i zapis robi kreator - przycisk tylko wracał do kreatora, dublował „Zastosuj i wróć” i miał ucięty
            ' napis („Pobierz i”)
            btnKompiluj.Visible = False
            btnZastosujKreator.Left = btnPomoc.Left - btnZastosujKreator.Width - 8
            AktualizujPrzyciskKorekty()
            PrzeliczKolejnoscWarstw()
            OdswiezTabeleWarstw()
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

    ''' <summary>Wczytuje bieżącą sesję programu (oraz katalogi z download tylko przy pustej liście).</summary>
    Private Sub WczytajWszystkieKatalogiDownload()
        ' 1. Jeśli jest aktywna sesja w oknie głównym, dodaj ją w pierwszej kolejności
        If Not String.IsNullOrEmpty(Form1.FolderSesji) AndAlso File.Exists(Path.Combine(Form1.FolderSesji, "conf.txt")) Then
            If Not _warstwy.Any(Function(item) TenSamFolder(item.Folder, Form1.FolderSesji)) Then
                DodajFolderSesji(Form1.FolderSesji)
            End If
            Return
        End If

        ' 2. Jeśli lista warstw jest już zapełniona, nie dosypuj automatycznie starych katalogów
        If _warstwy.Count > 0 Then Exit Sub

        Dim foldery = ZnajdzWszystkieKatalogiSesji(FolderDownload)
        If foldery.Count = 0 Then Exit Sub

        For Each folder In foldery
            If _warstwy.Any(Function(item) TenSamFolder(item.Folder, folder)) Then Continue For
            Dim nowa As New WarstwaWielowarstwowa With {.Folder = folder}
            Dim blad As String = ""
            If nowa.WczytajSesje(blad) Then
                ' Segmenty pobrane z TRANSPARENT=TRUE mają już kanał alfa; wypalanie bieli (ProgBieli=248) niszczyłoby jasne detale.
                _warstwy.Add(nowa)
            End If
        Next

        If _warstwy.Count > 0 Then
            PosortujWarstwyIDopasujRozlaczneSkale()
            PrzeliczKolejnoscWarstw()
            OdswiezTabeleWarstw()
            Loguj("Zaczytano " & _warstwy.Count & " sesji z folderu download z rozłącznymi zakresami powiększeń.", Color.DarkGreen)
        End If
    End Sub

    ''' <summary>Sortuje listę warstw wg rozdzielczości (naturalnego zoomu) i oblicza dla nich rozłączne zakresy skal.</summary>
    Private Sub PosortujWarstwyIDopasujRozlaczneSkale()
        If _warstwy.Count = 0 Then Exit Sub
        Dim posortowane = _warstwy.OrderBy(Function(x) KompilatorMbtiles.ObliczNaturalnyZoom(x)).ToList()
        _warstwy.Clear()
        _warstwy.AddRange(posortowane)
        KompilatorMbtiles.PrzeliczRozlaczneZakresyZoom(_warstwy)
    End Sub

    ''' <summary>Zwraca listę wszystkich katalogów zawierających plik conf.txt w folderze download i jego podfolderach.</summary>
    Private Function ZnajdzWszystkieKatalogiSesji(katalogDownload As String) As List(Of String)
        Dim foldery As New List(Of String)()
        If String.IsNullOrWhiteSpace(katalogDownload) OrElse Not Directory.Exists(katalogDownload) Then Return foldery

        If File.Exists(Path.Combine(katalogDownload, "conf.txt")) Then
            foldery.Add(katalogDownload)
        End If

        Try
            For Each pod In Directory.GetDirectories(katalogDownload)
                If File.Exists(Path.Combine(pod, "conf.txt")) Then
                    foldery.Add(pod)
                Else
                    Try
                        For Each pod2 In Directory.GetDirectories(pod)
                            If File.Exists(Path.Combine(pod2, "conf.txt")) Then
                                foldery.Add(pod2)
                            End If
                        Next
                    Catch
                    End Try
                End If
            Next
        Catch
        End Try

        Return foldery
    End Function

    Private Sub btnRozlaczneZakresy_Click(sender As Object, e As EventArgs) Handles btnRozlaczneZakresy.Click
        If _warstwy.Count = 0 Then Exit Sub
        PosortujWarstwyIDopasujRozlaczneSkale()
        PrzeliczKolejnoscWarstw()
        OdswiezTabeleWarstw()
        AktualizujZakresZoom()
        OdswiezPodgladAsync()
        Loguj("Dopasowano rozłączne zakresy powiększeń dla " & _warstwy.Count & " warstw.", Color.DarkGreen)
    End Sub

    ''' <summary>Dodaje sesję z podanego folderu i oblicza sugerowane poziomy zoom.</summary>
    Private Sub DodajFolderSesji(folder As String)
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

        _warstwy.Add(w)
        If _warstwy.Count > 1 Then
            PosortujWarstwyIDopasujRozlaczneSkale()
        End If
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
            Dim row = dgvWarstwy.Rows(idx)
            row.Tag = w
            If Not String.IsNullOrWhiteSpace(w.OpisPelny) Then
                row.Cells(colTytul.Index).ToolTipText = w.OpisPelny
            End If
            row.Cells(colFolder.Index).ToolTipText = w.Folder
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

    Private Sub btnWyczyscSesje_Click(sender As Object, e As EventArgs) Handles btnWyczyscSesje.Click
        If _warstwy.Count = 0 Then Exit Sub
        If MessageBox.Show("Czy na pewno chcesz usunąć wszystkie warstwy z listy?", "Czyszczenie listy",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            _warstwy.Clear()
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
        _ctsPodglad?.Dispose()
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
        If _trybKreatora Then
            lblPodgladInfo.Text = "Próbka stylu z kreatora (przed pobraniem)" & vbCrLf & opisWarstw
        Else
            lblPodgladInfo.Text = "Poziom " & z & " (piksel ~" & pikselMetry.ToString("0.00") & " m)" & vbCrLf & opisWarstw
        End If

        Dim opcje = PobierzOpcjeZFormularza()
        Dim warstwy = KopiaWarstw()
        Dim srodek = _srodekGeo

        Try
            Await Task.Delay(40, token)
            Dim bmp As Bitmap = Nothing
            If Not _trybKreatora Then
                bmp = Await Task.Run(Function()
                                         Return KompilatorMbtiles.RenderujPodgladKafla(warstwy, srodek.Szerokosc, srodek.Dlugosc, z, opcje)
                                     End Function, token)
            End If
            If token.IsCancellationRequested OrElse Zamkniete Then
                bmp?.Dispose()
                Exit Sub
            End If

            Dim staryObraz = picPodglad.Image
            If bmp IsNot Nothing Then
                picPodglad.Image = bmp
            ElseIf _trybKreatora Then
                Dim aktywneWiz = KompilatorMbtiles.WarstwyDlaPoziomu(_warstwy, z)
                If aktywneWiz.Count > 0 AndAlso _silnikKreatora IsNot Nothing AndAlso _zestawKreatora IsNot Nothing Then
                    Dim aktywneWarstwy As New List(Of WarstwaWybrana)()
                    Dim uzyte As New HashSet(Of WarstwaWielowarstwowa)()
                    For idx = 0 To _zestawKreatora.Warstwy.Count - 1
                        Dim wz = _zestawKreatora.Warstwy(idx)
                        Dim znakPozycji = "(Kreator: warstwa #" & (idx + 1) & ")"
                        Dim pasujaca = aktywneWiz.FirstOrDefault(Function(w) Not uzyte.Contains(w) AndAlso String.Equals(w.Folder, znakPozycji, StringComparison.OrdinalIgnoreCase))
                        If pasujaca Is Nothing AndAlso Not String.IsNullOrEmpty(wz.Adres) Then
                            pasujaca = aktywneWiz.FirstOrDefault(Function(w) Not uzyte.Contains(w) AndAlso w.OpisPelny.IndexOf(wz.Adres, StringComparison.OrdinalIgnoreCase) >= 0 AndAlso String.Equals(w.Tytul, wz.Nazwa, StringComparison.OrdinalIgnoreCase))
                        End If
                        If pasujaca Is Nothing Then
                            pasujaca = aktywneWiz.FirstOrDefault(Function(w) Not uzyte.Contains(w) AndAlso String.Equals(w.Tytul, wz.Nazwa, StringComparison.OrdinalIgnoreCase))
                        End If
                        If pasujaca IsNot Nothing Then
                            uzyte.Add(pasujaca)
                            Dim kopia As New WarstwaWybrana With {
                                .Nazwa = If(wz.Nazwa, ""),
                                .Adres = If(wz.Adres, ""),
                                .Zbior = If(wz.Zbior, ""),
                                .ZalecanyPiksel = wz.ZalecanyPiksel,
                                .TypZrodla = wz.TypZrodla,
                                .Nakladka = wz.Nakladka,
                                .IgnorujSsl = wz.IgnorujSsl,
                                .Krycie = CInt(Math.Round(pasujaca.Krycie * 100)),
                                .TrybMieszania = pasujaca.TrybMieszania
                            }
                            aktywneWarstwy.Add(kopia)
                        End If
                    Next
                    Dim zlozony = _silnikKreatora.ZlozObrazZWarstw(aktywneWarstwy)
                    If zlozony IsNot Nothing Then
                        If _opcjeKorekty IsNot Nothing AndAlso _opcjeKorekty.Wlaczona AndAlso chkKorekta.Checked Then
                            Dim pix = NakladanieObrazow.Piksele(zlozony, zlozony.Width, zlozony.Height)
                            KorekcjaMip.Zastosuj(pix, zlozony.Width, zlozony.Height, _opcjeKorekty)
                            Dim stara = zlozony
                            zlozony = NakladanieObrazow.DoBitmapy(pix, stara.Width, stara.Height)
                            stara.Dispose()
                        End If
                        picPodglad.Image = zlozony
                    ElseIf _probkaBmpKreatora IsNot Nothing Then
                        Try
                            picPodglad.Image = New Bitmap(_probkaBmpKreatora)
                        Catch
                            picPodglad.Image = RysujPustyKafel("Brak danych na tym powiększeniu")
                        End Try
                    Else
                        picPodglad.Image = RysujPustyKafel("Brak danych na tym powiększeniu")
                    End If
                ElseIf _probkaBmpKreatora IsNot Nothing AndAlso aktywneWiz.Count > 0 Then
                    Try
                        picPodglad.Image = New Bitmap(_probkaBmpKreatora)
                    Catch
                        picPodglad.Image = RysujPustyKafel("Brak danych na tym powiększeniu")
                    End Try
                Else
                    picPodglad.Image = RysujPustyKafel("Wszystkie warstwy wyłączone")
                End If
            Else
                picPodglad.Image = RysujPustyKafel("Brak danych na tym powiększeniu")
            End If
            staryObraz?.Dispose()
        Catch ex As OperationCanceledException
            ' Anulowanie jest oczekiwane przy szybkiej zmianie parametrów
        Catch ex As Exception
            Loguj("Błąd renderowania podglądu: " & OpisBledu.Z(ex), Color.Red)
        End Try
    End Sub

    Private Function PobierzOpcjeZFormularza() As OpcjeKompilatoraMbtiles
        Dim opcje As New OpcjeKompilatoraMbtiles With {
            .PlikDocelowy = txtPlikWynikowy.Text.Trim(),
            .Format = _wybranyFormat,
            .NazwaMapy = txtNazwaMapy.Text.Trim(),
            .JakoscJpeg = CInt(nudJakosc.Value),
            .SzybkaPiramidaWPasmach = chkPiramida.Checked,
            .WypelnijLukiWarstwaNizsza = chkWypelnijLuki.Checked,
            .PoziomMin = tbZoom.Minimum,
            .PoziomMax = tbZoom.Maximum,
            .Korekta = If(chkKorekta.Checked AndAlso _opcjeKorekty IsNot Nothing, _opcjeKorekty.Kopia(), New OpcjeKorekcjiMip With {.Wlaczona = False})
        }

        Select Case cmbFormat.SelectedIndex
            Case 1 : opcje.FormatKafli = FormatKafliMbtiles.TylkoJpeg
            Case 2 : opcje.FormatKafli = FormatKafliMbtiles.TylkoPng
            Case Else : opcje.FormatKafli = FormatKafliMbtiles.SmartHybrid
        End Select

        Return opcje
    End Function

    Private Sub chkKorekta_CheckedChanged(sender As Object, e As EventArgs) Handles chkKorekta.CheckedChanged
        _opcjeKorekty.Wlaczona = chkKorekta.Checked
        'po wyborze „Brak” w podglądzie zaznaczenie pola stosowało ukryte wartości suwaków poprzedniego profilu
        If chkKorekta.Checked AndAlso _opcjeKorekty.Profil = ProfilKorektyRastra.Brak Then
            _opcjeKorekty.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
        End If
        AktualizujPrzyciskKorekty()
        If Not _wTrakcieLadowania Then OdswiezPodgladAsync()
    End Sub

    Private Async Sub btnKorekta_Click(sender As Object, e As EventArgs) Handles btnKorekta.Click
        'próbka - kafel podglądu wyrenderowany BEZ korekty: obraz z picPodglad jest już poprawiony, więc podgląd MIP
        'pokazywał jako „oryginał” obraz po korekcie, a jako wynik - korektę nałożoną dwukrotnie
        Dim opcje = PobierzOpcjeZFormularza()
        opcje.Korekta = New OpcjeKorekcjiMip With {.Wlaczona = False}
        Dim warstwy = KopiaWarstw()
        Dim srodek = _srodekGeo
        Dim z = tbZoom.Value
        Dim probka As Bitmap = Nothing
        btnKorekta.Enabled = False
        UseWaitCursor = True
        Try
            Try
                If _trybKreatora AndAlso _probkaBmpKreatora IsNot Nothing Then
                    probka = New Bitmap(_probkaBmpKreatora)
                Else
                    probka = Await Task.Run(Function() KompilatorMbtiles.RenderujPodgladKafla(warstwy, srodek.Szerokosc, srodek.Dlugosc, z, opcje))
                End If
                If probka Is Nothing Then
                    For Each w In warstwy
                        If w.Wlaczona AndAlso w.PoziomMin <= w.PoziomMax Then
                            probka = Await Task.Run(Function() KompilatorMbtiles.RenderujPodgladKafla(warstwy, srodek.Szerokosc, srodek.Dlugosc, w.PoziomMax, opcje))
                            If probka IsNot Nothing Then Exit For
                        End If
                    Next
                End If
                If probka Is Nothing Then
                    For Each w In warstwy
                        If w.Zrodlo IsNot Nothing Then
                            Dim plik = w.Zrodlo.ZnajdzIstniejacySegmentSrodkowy()
                            If Not String.IsNullOrEmpty(plik) AndAlso File.Exists(plik) Then
                                probka = New Bitmap(plik)
                                Exit For
                            End If
                        End If
                    Next
                End If
            Catch ex As Exception
                probka?.Dispose()
                probka = Nothing
            Finally
                If Not Zamkniete AndAlso Not IsDisposed Then
                    btnKorekta.Enabled = (_cts Is Nothing) AndAlso chkKorekta.Checked
                    UseWaitCursor = False
                End If
            End Try
            If Zamkniete OrElse IsDisposed Then Exit Sub
            Using dlg As New FormPodgladMip(_opcjeKorekty, probka)
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    _opcjeKorekty = dlg.Opcje.Kopia()
                    chkKorekta.Checked = _opcjeKorekty.Wlaczona
                    AktualizujPrzyciskKorekty()
                    OdswiezPodgladAsync()
                End If
            End Using
        Finally
            probka?.Dispose()
        End Try
    End Sub

    'format mm:ss pokazywał czas powyżej godziny bez godzin (1 h 05 min jako 05:00)
    Friend Shared Function CzasTrwania(t As TimeSpan) As String
        Dim ci = Globalization.CultureInfo.InvariantCulture
        Return If(t.TotalHours >= 1, CInt(Math.Floor(t.TotalHours)).ToString(ci) & ":" & t.ToString("mm\:ss", ci), t.ToString("mm\:ss", ci))
    End Function

    Private Sub AktualizujPrzyciskKorekty()
        btnKorekta.Text = "Korekta obrazu..."
        btnKorekta.Enabled = (_cts Is Nothing) AndAlso chkKorekta.Checked
    End Sub

    Private Sub cmbTypBazy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTypBazy.SelectedIndexChanged
        If _wTrakcieLadowania Then Exit Sub
        Dim nowy = If(cmbTypBazy.SelectedIndex = 1, RodzajEksportu.GeoPackage, RodzajEksportu.MBTiles)
        If nowy <> _wybranyFormat Then
            _wybranyFormat = nowy
            Dim stara = txtPlikWynikowy.Text.Trim()
            If _wybranyFormat = RodzajEksportu.GeoPackage Then
                If stara.EndsWith(".mbtiles", StringComparison.OrdinalIgnoreCase) Then
                    txtPlikWynikowy.Text = stara.Substring(0, stara.Length - 8) & ".gpkg"
                End If
            Else
                If stara.EndsWith(".gpkg", StringComparison.OrdinalIgnoreCase) Then
                    txtPlikWynikowy.Text = stara.Substring(0, stara.Length - 5) & ".mbtiles"
                End If
            End If
            AktualizujTytulOkna()
        End If
    End Sub

    Private Sub btnWybierzPlik_Click(sender As Object, e As EventArgs) Handles btnWybierzPlik.Click
        Dim gpkg = (_wybranyFormat = RodzajEksportu.GeoPackage)
        dlgSave.Filter = If(gpkg, "Pliki GeoPackage (*.gpkg)|*.gpkg|Wszystkie pliki (*.*)|*.*",
                                  "Pliki MBTiles (*.mbtiles)|*.mbtiles|Wszystkie pliki (*.*)|*.*")
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

        _ctsOszacuj?.Cancel()
        _ctsOszacuj?.Dispose()
        Dim ctsOsz As New CancellationTokenSource()
        _ctsOszacuj = ctsOsz
        Dim tokenOsz = ctsOsz.Token

        Dim opcje = PobierzOpcjeZFormularza()
        Try
            If _trybKreatora AndAlso (_zasiegKreatora.Poprawny OrElse _srodekGeo.Szerokosc <> 0) Then
                Dim minZ As Integer = Math.Max(0, Math.Min(opcje.PoziomMin, opcje.PoziomMax))
                Dim maxZ As Integer = Math.Min(22, Math.Max(opcje.PoziomMin, opcje.PoziomMax))
                lvwPoziomy.Items.Clear()

                Dim lacznieKafli As Long = 0
                Dim lacznieBajtow As Long = 0

                Dim pGeo As ProstokatGeo
                If _zasiegKreatora.Poprawny Then
                    Dim ukladWarstwy = If(_warstwy.FirstOrDefault(Function(w) w.Wlaczona AndAlso w.Uklad IsNot Nothing)?.Uklad, UkladWspolrzednych.PL1992)
                    pGeo = ProstokatGeo.Opisany(ukladWarstwy, _zasiegKreatora)
                Else
                    pGeo = New ProstokatGeo(_srodekGeo.Szerokosc + 0.05, _srodekGeo.Szerokosc - 0.05, _srodekGeo.Dlugosc + 0.05, _srodekGeo.Dlugosc - 0.05)
                End If

                For poziom = minZ To maxZ
                    tokenOsz.ThrowIfCancellationRequested()
                    Dim aktywne = KompilatorMbtiles.WarstwyDlaPoziomu(_warstwy, poziom)
                    If aktywne.Count = 0 AndAlso Not opcje.WypelnijLukiWarstwaNizsza Then Continue For

                    Dim liczbaKafliPoziomu = WebMercator.LiczbaKafli(pGeo, poziom)
                    Dim rozmiarKafleSzacunek = CLng(liczbaKafliPoziomu) * 28000L ' orientacyjnie ~28 KB/kafel
                    lacznieKafli += liczbaKafliPoziomu
                    lacznieBajtow += rozmiarKafleSzacunek

                    Dim itm As New ListViewItem("Z" & poziom)
                    itm.SubItems.Add(liczbaKafliPoziomu.ToString("N0"))
                    itm.SubItems.Add(SzacunekRozmiaru.Tekst(rozmiarKafleSzacunek))
                    Dim nazwyAktywnych = If(aktywne.Count > 0, String.Join(", ", aktywne.Select(Function(w) w.Tytul)), "(wypełnienie)")
                    itm.SubItems.Add(nazwyAktywnych)
                    lvwPoziomy.Items.Add(itm)
                Next

                lblSzacunekPodsumowanie.Text = "Łącznie kafli: " & lacznieKafli.ToString("N0") & vbCrLf &
                                               "Szacowany rozmiar: ~" & SzacunekRozmiaru.Tekst(lacznieBajtow) & " (zostanie pobrane w kreatorze)"

                If lacznieBajtow > 4294967295L Then
                    lblStatusFat32.ForeColor = Color.Red
                    lblStatusFat32.Text = "UWAGA: Plik > 4 GB! Nie zmieści się na kartach SD w formacie FAT32."
                ElseIf lacznieBajtow > 2147483648L Then
                    lblStatusFat32.ForeColor = Color.DarkOrange
                    lblStatusFat32.Text = "Rozmiar powyżej 2 GB. Zgodny z OruxMaps i Locus Map."
                Else
                    lblStatusFat32.ForeColor = Color.DarkGreen
                    lblStatusFat32.Text = "Rozmiar optymalny dla urządzeń mobilnych (Locus Map, OruxMaps)."
                End If
                Exit Sub
            End If

            Dim szacunek = Await KompilatorMbtiles.OszacujRozmiarAsync(KopiaWarstw(), opcje, tokenOsz)
            If Zamkniete OrElse tokenOsz.IsCancellationRequested Then Exit Sub
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
        Catch ex As OperationCanceledException
        Catch ex As Exception
            If Not Zamkniete Then Loguj("Błąd szacowania rozmiaru: " & OpisBledu.Z(ex), Color.Red)
        Finally
            If _ctsOszacuj Is ctsOsz Then _ctsOszacuj = Nothing
            ctsOsz.Dispose()
            If Not Zamkniete AndAlso Not IsDisposed AndAlso _ctsOszacuj Is Nothing Then btnOszacuj.Enabled = (_cts Is Nothing)
        End Try
    End Sub

    Private Sub btnZastosujKreator_Click(sender As Object, e As EventArgs) Handles btnZastosujKreator.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Async Sub btnKompiluj_Click(sender As Object, e As EventArgs) Handles btnKompiluj.Click
        If _trybKreatora AndAlso _warstwy.Any(Function(w) w.Wlaczona AndAlso (w.Zrodlo Is Nothing OrElse w.Folder.StartsWith("(Kreator", StringComparison.OrdinalIgnoreCase))) Then
            Dim odp = MessageBox.Show("Warstwy pochodzą z szybkiego kreatora i nie zostały jeszcze pobrane z serwerów." & vbCrLf & vbCrLf &
                                      "Czy chcesz zatwierdzić wprowadzone ustawienia i powrócić do kreatora, aby pobrać segmenty i utworzyć plik MBTiles?",
                                      "Kreator mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If odp = DialogResult.Yes Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
            Exit Sub
        End If

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

        Loguj("Rozpoczęto kompilację MBTiles wieloskalowego do: " & opcje.PlikDocelowy, Color.DarkGreen)
        Dim st = Diagnostics.Stopwatch.StartNew()

        Try
            Dim plik As String = Await KompilatorMbtiles.KompilujAsync(KopiaWarstw(), opcje, postep, token)
            st.Stop()
            If Zamkniete Then Exit Sub
            FormEksport.OstatniPlik = plik
            Loguj("SUKCES: Utworzono plik " & If(_wybranyFormat = RodzajEksportu.GeoPackage, "GeoPackage", "MBTiles") & " w czasie: " & CzasTrwania(st.Elapsed), Color.DarkGreen)
            'plik docelowy otwarty w innym programie - wynik zapisany pod inną nazwą
            Dim innaNazwa As Boolean = Not String.Equals(plik, opcje.PlikDocelowy, StringComparison.OrdinalIgnoreCase)
            If innaNazwa Then txtPlikWynikowy.Text = plik

            Dim finInfo = New FileInfo(plik)
            Dim rozmiarMb = finInfo.Length / 1024.0 / 1024.0
            Dim odp = MessageBox.Show("Plik MBTiles został pomyślnie utworzony!" & vbCrLf & vbCrLf &
                                      If(innaNazwa, "Plik " & opcje.PlikDocelowy & " jest otwarty w innym programie, dlatego mapę zapisano pod inną nazwą." & vbCrLf, "") &
                                      "Plik: " & plik & vbCrLf &
                                      "Rozmiar: " & rozmiarMb.ToString("0.0") & " MB" & vbCrLf &
                                      "Czas: " & CzasTrwania(st.Elapsed) & vbCrLf & vbCrLf &
                                      "Czy chcesz otworzyć folder i zaznaczyć gotowy plik w Eksploratorze Windows?",
                                      "Kompilacja zakończona sukcesem", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
            If odp = DialogResult.Yes Then
                OtworzIZaznaczPlik(plik)
            End If
        Catch ex As OperationCanceledException
            Loguj("Kompilacja została przerwana przez użytkownika.", Color.DarkOrange)
        Catch ex As Exception
            If Not Zamkniete Then
                Loguj("BŁĄD podczas kompilacji: " & OpisBledu.Z(ex), Color.Red)
                MessageBox.Show("Wystąpił błąd podczas tworzenia MBTiles:" & vbCrLf & OpisBledu.Z(ex), "Błąd kompilacji", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        btnRozlaczneZakresy.Enabled = Not wTrakcie AndAlso _warstwy.Count > 0
        btnGora.Enabled = Not wTrakcie
        btnDol.Enabled = Not wTrakcie
        dgvWarstwy.Enabled = Not wTrakcie
        grpUstawienia.Enabled = Not wTrakcie
        btnOszacuj.Enabled = Not wTrakcie
        btnZamknij.Enabled = Not wTrakcie
        btnKorekta.Enabled = Not wTrakcie AndAlso chkKorekta.Checked
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
        _ctsPodglad?.Dispose()
        _ctsPodglad = Nothing
        _ctsOszacuj?.Cancel()
        _ctsOszacuj?.Dispose()
        _ctsOszacuj = Nothing
        picPodglad.Image?.Dispose()
        picPodglad.Image = Nothing
        _probkaBmpKreatora?.Dispose()
        _probkaBmpKreatora = Nothing
        If Not _trybKreatora Then
            _warstwy.Clear()
        End If
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
                Dim nowaDl = WebMercator.Dlugosc(pikselX, z)
                Dim nowaSz = WebMercator.Szerokosc(pikselY, z)
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

#Region "Integracja z szybkim kreatorem map"

    ''' <summary>
    ''' Otwiera narzędzie wieloskalowe dla zestawu warstw i parametrów przekazanych z szybkiego kreatora mapy.
    ''' Usuwa stare sesje z pamięci i inicjalizuje tabelę warstwami wybranymi przez użytkownika w kreatorze.
    ''' </summary>
    Public Function OtworzDlaKreatora(kreatorZestaw As ZestawWarstw,
                                      zasieg As Zasieg,
                                      uklad As UkladWspolrzednych,
                                      sciezkaDocelowa As String,
                                      zoomMin As Integer,
                                      zoomMax As Integer,
                                      opcjeKorekty As OpcjeKorekcjiMip,
                                      probkaBmp As Bitmap,
                                      parent As IWin32Window,
                                      Optional silnikPodgladu As PodgladWarstwSilnik = Nothing) As Boolean
        _trybKreatora = True
        _warstwy.Clear()
        dgvWarstwy.Rows.Clear()
        _zestawKreatora = kreatorZestaw
        _zasiegKreatora = zasieg
        _silnikKreatora = silnikPodgladu

        If _probkaBmpKreatora IsNot Nothing Then
            _probkaBmpKreatora.Dispose()
            _probkaBmpKreatora = Nothing
        End If
        If probkaBmp IsNot Nothing Then
            Try
                _probkaBmpKreatora = New Bitmap(probkaBmp)
            Catch
            End Try
        End If

        _opcjeKorekty = If(opcjeKorekty IsNot Nothing, opcjeKorekty.Kopia(), New OpcjeKorekcjiMip())
        chkKorekta.Checked = _opcjeKorekty.Wlaczona
        txtPlikWynikowy.Text = sciezkaDocelowa
        txtNazwaMapy.Text = If(Not String.IsNullOrEmpty(sciezkaDocelowa), Path.GetFileNameWithoutExtension(sciezkaDocelowa), "Mapa")

        If zasieg.Poprawny AndAlso uklad IsNot Nothing Then
            Dim pt = zasieg.Srodek
            _srodekGeo = uklad.DoWgs84(pt)
        End If

        ' Załaduj warstwy z zestawu kreatora
        If kreatorZestaw IsNot Nothing Then
            For i = 0 To kreatorZestaw.Warstwy.Count - 1
                Dim wz = kreatorZestaw.Warstwy(i)
                Dim nowa As New WarstwaWielowarstwowa With {
                    .Tytul = wz.Nazwa,
                    .OpisPelny = If(Not String.IsNullOrEmpty(wz.Zbior), "Zbiór: " & wz.Zbior & " | " & wz.Adres, wz.Nazwa),
                    .PoziomMin = zoomMin,
                    .PoziomMax = zoomMax,
                    .Krycie = Math.Max(0.0, Math.Min(1.0, wz.Krycie / 100.0)),
                    .TrybMieszania = wz.TrybMieszania,
                    .Wlaczona = True,
                    .BialePrzezroczyste = False,
                    .OdcienieSzarosci = False,
                    .Uklad = If(uklad, UkladWspolrzednych.PL1992),
                    .Folder = "(Kreator: warstwa #" & (i + 1) & ")"
                }
                _warstwy.Add(nowa)
            Next
        End If

        Dim res = Me.ShowDialog(parent)
        Return (res = DialogResult.OK)
    End Function

    ''' <summary>
    ''' Przenosi zmodyfikowane parametry warstw, zakresów zoom i korekty z powrotem do kreatora.
    ''' </summary>
    Public Sub PobierzUstawieniaDlaKreatora(ByRef zestaw As ZestawWarstw,
                                           ByRef zoomMin As Integer,
                                           ByRef zoomMax As Integer,
                                           ByRef opcjeKorekty As OpcjeKorekcjiMip,
                                           ByRef sciezkaDocelowa As String)
        sciezkaDocelowa = txtPlikWynikowy.Text.Trim()
        opcjeKorekty = _opcjeKorekty.Kopia()
        opcjeKorekty.Wlaczona = chkKorekta.Checked

        Dim aktywne = _warstwy.Where(Function(w) w.Wlaczona).ToList()
        If aktywne.Count > 0 Then
            zoomMin = aktywne.Min(Function(w) w.PoziomMin)
            zoomMax = aktywne.Max(Function(w) w.PoziomMax)
        End If

        ' Zaktualizuj parametry w warstwach zestawu (kolejność, krycie, tryb mieszania)
        If zestaw IsNot Nothing Then
            Dim stareWarstwy = zestaw.Warstwy.ToList()
            Dim uzyteStare As New HashSet(Of Integer)()
            zestaw.Wyczysc()
            For Each w In _warstwy
                If w.Wlaczona Then
                    Dim pasujacyIndeks As Integer = -1
                    Dim folderStr = If(w.Folder, "")
                    If folderStr.StartsWith("(Kreator: warstwa #", StringComparison.OrdinalIgnoreCase) AndAlso folderStr.EndsWith(")") Then
                        Dim nrTekst = folderStr.Substring("(Kreator: warstwa #".Length, folderStr.Length - "(Kreator: warstwa #".Length - 1)
                        Dim nr As Integer
                        If Integer.TryParse(nrTekst, nr) AndAlso nr >= 1 AndAlso nr <= stareWarstwy.Count AndAlso Not uzyteStare.Contains(nr - 1) Then
                            pasujacyIndeks = nr - 1
                        End If
                    End If
                    If pasujacyIndeks < 0 Then
                        For idx = 0 To stareWarstwy.Count - 1
                            If uzyteStare.Contains(idx) Then Continue For
                            Dim x = stareWarstwy(idx)
                            If Not String.IsNullOrEmpty(x.Adres) AndAlso Not String.IsNullOrEmpty(w.OpisPelny) AndAlso
                               w.OpisPelny.IndexOf(x.Adres, StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                               String.Equals(x.Nazwa, w.Tytul, StringComparison.OrdinalIgnoreCase) Then
                                pasujacyIndeks = idx
                                Exit For
                            End If
                        Next
                    End If
                    If pasujacyIndeks < 0 Then
                        For idx = 0 To stareWarstwy.Count - 1
                            If uzyteStare.Contains(idx) Then Continue For
                            If String.Equals(stareWarstwy(idx).Nazwa, w.Tytul, StringComparison.OrdinalIgnoreCase) Then
                                pasujacyIndeks = idx
                                Exit For
                            End If
                        Next
                    End If
                    If pasujacyIndeks >= 0 Then
                        uzyteStare.Add(pasujacyIndeks)
                        Dim pasujaca = stareWarstwy(pasujacyIndeks)
                        zestaw.Dodaj(pasujaca.Nazwa, CInt(Math.Round(w.Krycie * 100)), pasujaca.Adres, pasujaca.Zbior,
                                     pasujaca.ZalecanyPiksel, pasujaca.TypZrodla, pasujaca.Nakladka, False, pasujaca.IgnorujSsl, w.TrybMieszania)
                    Else
                        zestaw.Dodaj(w.Tytul, CInt(Math.Round(w.Krycie * 100)), w.Folder, "", 0, TypUslugiMapowej.Nieznana, False, False, False, w.TrybMieszania)
                    End If
                End If
            Next
        End If
    End Sub

#End Region


    Private Shared Function RysujPustyKafel(tekst As String) As Bitmap
        Dim pusta As New Bitmap(256, 256)
        Using g = Graphics.FromImage(pusta)
            g.Clear(Color.WhiteSmoke)
            Using p As New Pen(Color.LightGray)
                g.DrawLine(p, 0, 0, 256, 256)
                g.DrawLine(p, 0, 256, 256, 0)
            End Using
            Using f As New Font("Microsoft Sans Serif", 9.0!, FontStyle.Regular)
                g.DrawString(tekst, f, Brushes.Gray, New RectangleF(10, 110, 236, 40))
            End Using
        End Using
        Return pusta
    End Function

End Class
