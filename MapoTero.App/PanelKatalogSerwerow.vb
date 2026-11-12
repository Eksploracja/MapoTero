'Copyright (C) <2026>  MapoTero
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

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Krok 1 kreatora dodawania serwera: wybór usługi z wbudowanego katalogu
''' oraz oficjalnych rejestrów online (Geoportal.gov.pl i EZiUDP GUGiK),
''' albo przejście do wpisania własnego adresu URL.
''' </summary>
Public Class PanelKatalogSerwerow

    Private Const TrybWbudowany As Integer = 0
    Private Const TrybGeoportal As Integer = 1
    Private Const TrybEziudpCentralne As Integer = 2
    Private Const TrybEziudpLokalne As Integer = 3

    Private ReadOnly _istniejaceZbiory As New List(Of KeyValuePair(Of String, String))
    Private ReadOnly _cacheSesji As New Dictionary(Of Integer, List(Of WpisKataloguSerwerow))
    Private ReadOnly _cachePodgladuCap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private _biezaceWpisy As New List(Of WpisKataloguSerwerow)
    Private _blokadaZdarzen As Boolean = False
    Private _zaladowano As Boolean = False
    Private _poprzedniTryb As Integer = TrybWbudowany
    Private _filtrListy As String = ""
    Private _frazaLokalna As String = ""
    Private _ctsZapytania As CancellationTokenSource
    Private _ctsPodgladuCap As CancellationTokenSource
    Private _kolumnaSortowania As Integer = -1
    Private _sortowanieMalejaco As Boolean = False

    Public Event UslugaWybrana(wpis As WpisKataloguSerwerow)
    Public Event WlasnyAdresKlikniety(wstepnyAdres As String)
    Public Event AnulujKlikniete()

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PrzyciskAnuluj As Button
        Get
            Return btnAnuluj
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
        'włączenie podwójnego buforowania ListView ogranicza migotanie podczas filtrowania
        GetType(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)?.SetValue(lvUslugi, True)
        InicjalizujFiltry()
    End Sub

    Public Sub UstawIstniejaceZbiory(istniejaceZbiory As IEnumerable(Of KeyValuePair(Of String, String)))
        _istniejaceZbiory.Clear()
        If istniejaceZbiory IsNot Nothing Then
            _istniejaceZbiory.AddRange(istniejaceZbiory)
        End If
        UpewnijIstniejaceZbiory()
        If _biezaceWpisy.Count > 0 Then
            KatalogSerwerow.DopasujIstniejace(_biezaceWpisy, _istniejaceZbiory)
            WypelnijListe(bezFiltraFrazy:=(cmbZrodlo.SelectedIndex = TrybEziudpLokalne))
        End If
    End Sub

    ''' <summary>Zapewnia obecność listy zbiorów map do porównania (fallback z dysku, gdy nie przekazano listy z Form1).</summary>
    Private Sub UpewnijIstniejaceZbiory()
        If _istniejaceZbiory.Count > 0 Then Exit Sub
        Dim foldery As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(FolderWarstwProgramu) AndAlso Directory.Exists(FolderWarstwProgramu) Then
            foldery.Add(FolderWarstwProgramu)
        End If
        If Not String.IsNullOrWhiteSpace(FolderWarstwDanych) AndAlso Directory.Exists(FolderWarstwDanych) Then
            foldery.Add(FolderWarstwDanych)
        End If

        Dim dodaneNazwy As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each kat In foldery
            Dim pliki = Directory.GetFiles(kat, "*.txt")
            For Each p In pliki
                Dim fn = Path.GetFileNameWithoutExtension(p)
                If String.Equals(fn, "katalog", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(fn, "warstwy", StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not dodaneNazwy.Add(fn) Then Continue For
                Try
                    Dim pw = PlikWarstw.Wczytaj(p)
                    If pw IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(pw.Adres) Then
                        _istniejaceZbiory.Add(New KeyValuePair(Of String, String)(fn, pw.Adres))
                    End If
                Catch
                End Try
            Next
        Next
    End Sub

    Public Sub FokusujSzukaj()
        If txtSzukaj.CanFocus Then txtSzukaj.Focus()
    End Sub

    Public Sub AnulujAktywneZapytanie()
        AnulujAktywnyPodgladCap()
        If _ctsZapytania IsNot Nothing Then
            _ctsZapytania.Cancel()
            _ctsZapytania.Dispose()
            _ctsZapytania = Nothing
        End If
    End Sub

    Private Sub InicjalizujFiltry()
        _blokadaZdarzen = True
        Try
            cmbZrodlo.Items.AddRange({
                "Polecane usługi publiczne (wbudowane)",
                "Oficjalny wykaz Geoportal.gov.pl (online)",
                "EZiUDP – usługi centralne i wojewódzkie (online)",
                "EZiUDP – powiaty i gminy (szukaj po nazwie / TERYT)"
            })
            cmbZakresLokalny.Items.AddRange({"Powiaty i gminy", "Tylko powiaty", "Tylko gminy"})
            cmbZakresLokalny.SelectedIndex = 0
            cmbKategoria.Items.Add("Wszystkie")
            cmbKategoria.SelectedIndex = 0
        Finally
            _blokadaZdarzen = False
        End Try
    End Sub

    Private Sub PanelKatalogSerwerow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode OrElse _zaladowano Then Exit Sub
        _zaladowano = True
        cmbZrodlo.SelectedIndex = TrybWbudowany
    End Sub

    Private Function SciezkaCache(nazwaPliku As String) As String
        If String.IsNullOrWhiteSpace(FolderDanych) Then Return ""
        Return Path.Combine(FolderDanych, "cache_katalogu", nazwaPliku)
    End Function

    Private Shared Function CzyWpisanoAdresUrl(tekst As String) As Boolean
        Dim t = If(tekst, "").Trim()
        If t.Length < 12 Then Return False
        Return t.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
               t.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub AktualizujPrzyciskWlasnegoAdresu()
        If CzyWpisanoAdresUrl(txtSzukaj.Text) Then
            btnWlasnyAdres.Text = "Użyj wpisanego adresu URL →"
            lblStatus.ForeColor = Color.DarkBlue
            lblStatus.Text = "Wykryto adres URL — naciśnij Enter lub kliknij przycisk po lewej, aby połączyć się bezpośrednio z tym serwerem."
        Else
            btnWlasnyAdres.Text = "+ Wpisz własny adres URL (WMS / WMTS / XYZ)..."
        End If
    End Sub

    Private Async Sub cmbZrodlo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbZrodlo.SelectedIndexChanged
        If _blokadaZdarzen Then Exit Sub
        Dim tryb = cmbZrodlo.SelectedIndex
        If _poprzedniTryb = TrybEziudpLokalne Then
            _frazaLokalna = txtSzukaj.Text
        Else
            _filtrListy = txtSzukaj.Text
        End If
        _poprzedniTryb = tryb

        _blokadaZdarzen = True
        Try
            txtSzukaj.Text = If(tryb = TrybEziudpLokalne, _frazaLokalna, _filtrListy)
        Finally
            _blokadaZdarzen = False
        End Try

        AktualizujKontrolkiTrybu(tryb)
        Await ZaladujZrodloAsync(tryb, wymusOdswiezenie:=False)
        AktualizujPrzyciskWlasnegoAdresu()
    End Sub

    Private Sub AktualizujKontrolkiTrybu(tryb As Integer)
        Dim czyLokalne = (tryb = TrybEziudpLokalne)
        cmbZakresLokalny.Visible = czyLokalne
        btnSzukajLokalne.Visible = czyLokalne
        btnOdswiez.Visible = (tryb <> TrybWbudowany)
        btnOdswiez.Text = If(czyLokalne, "Odśwież powiaty", "Odśwież z internetu")

        If czyLokalne Then
            txtSzukaj.PlaceholderText = "np. Kraków, Zielonki, 1206 (Enter)"
        Else
            txtSzukaj.PlaceholderText = "szukaj po nazwie, instytucji lub wklej własny URL..."
        End If
    End Sub

    Private Async Function ZaladujZrodloAsync(tryb As Integer, wymusOdswiezenie As Boolean) As Task
        AnulujAktywneZapytanie()
        WyczyscSzczegoly()

        If tryb = TrybWbudowany Then
            UpewnijIstniejaceZbiory()
            Dim sciezkaZewn = If(String.IsNullOrWhiteSpace(FolderWarstwProgramu), "", Path.Combine(FolderWarstwProgramu, "serwery_dodatkowe.json"))
            Dim wpisy = KatalogSerwerow.WczytajWbudowany(sciezkaZewn)
            KatalogSerwerow.DopasujIstniejace(wpisy, _istniejaceZbiory)
            _biezaceWpisy = wpisy
            OdswiezKategorie(wpisy)
            WypelnijListe()
            lblStatus.ForeColor = Color.DarkGreen
            lblStatus.Text = "Wczytano wbudowaną listę sprawdzonych usług (" & wpisy.Count & "). Wybierz usługę lub wpisz własny adres URL poniżej."
            Exit Function
        End If

        If tryb = TrybEziudpLokalne Then
            If _cacheSesji.ContainsKey(TrybEziudpLokalne) AndAlso _cacheSesji(TrybEziudpLokalne).Count > 0 Then
                _biezaceWpisy = _cacheSesji(TrybEziudpLokalne)
                OdswiezKategorie(_biezaceWpisy)
                WypelnijListe(bezFiltraFrazy:=True)
                lblStatus.ForeColor = Color.DarkGreen
                lblStatus.Text = "Znaleziono usług lokalnych: " & _biezaceWpisy.Count & "."
                Exit Function
            End If
            _biezaceWpisy = New List(Of WpisKataloguSerwerow)()
            OdswiezKategorie(_biezaceWpisy)
            WypelnijListe(bezFiltraFrazy:=True)
            lblStatus.ForeColor = Color.Black
            lblStatus.Text = "Wpisz nazwę gminy, powiatu albo kod TERYT (np. Kraków, Zielonki, 1206) i kliknij ""Szukaj""."
            txtSzukaj.Focus()
            Exit Function
        End If

        If Not wymusOdswiezenie AndAlso _cacheSesji.ContainsKey(tryb) Then
            _biezaceWpisy = _cacheSesji(tryb)
            OdswiezKategorie(_biezaceWpisy)
            WypelnijListe()
            lblStatus.ForeColor = Color.DarkGreen
            lblStatus.Text = "Wczytano " & _biezaceWpisy.Count & " usług."
            Exit Function
        End If

        _ctsZapytania = New CancellationTokenSource()
        Dim token = _ctsZapytania.Token
        UstawStanLadowania(True)
        lblStatus.ForeColor = Color.Black
        lblStatus.Text = If(tryb = TrybGeoportal,
                            "Pobieranie oficjalnego wykazu usług z Geoportal.gov.pl...",
                            "Pobieranie wykazu usług z rejestru EZiUDP...")

        Try
            Dim res As WynikPobraniaKatalogu
            If tryb = TrybGeoportal Then
                res = Await KlientKataloguSerwerow.PobierzGeoportalAsync(
                    sciezkaCache:=SciezkaCache("geoportal.json"),
                    wymusOdswiezenie:=wymusOdswiezenie,
                    token:=token)
            Else
                res = Await KlientKataloguSerwerow.PobierzEziudpCentralneIWojewodzkieAsync(
                    sciezkaCache:=SciezkaCache("eziudp_centralne.json"),
                    wymusOdswiezenie:=wymusOdswiezenie,
                    token:=token)
            End If

            If token.IsCancellationRequested OrElse IsDisposed Then Exit Function

            If Not res.Sukces Then
                _biezaceWpisy = New List(Of WpisKataloguSerwerow)()
                OdswiezKategorie(_biezaceWpisy)
                WypelnijListe()
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = res.KomunikatBledu
                Exit Function
            End If

            KatalogSerwerow.DopasujIstniejace(res.Wpisy, _istniejaceZbiory)
            _cacheSesji(tryb) = res.Wpisy
            _biezaceWpisy = res.Wpisy
            OdswiezKategorie(_biezaceWpisy)
            WypelnijListe(bezFiltraFrazy:=(tryb = TrybEziudpLokalne))

            If res.ZCacheStary Then
                lblStatus.ForeColor = Color.DarkOrange
                lblStatus.Text = "Brak połączenia - wczytano " & res.Wpisy.Count & " usług ze starszej pamięci podręcznej offline."
            Else
                lblStatus.ForeColor = Color.DarkGreen
                lblStatus.Text = "Pobrano " & res.Wpisy.Count & " usług" &
                                 If(res.ZCache, " (z lokalnej pamięci podręcznej).", " z internetu.")
            End If
        Catch ex As OperationCanceledException
            'anulowane przez zmianę źródła lub zamknięcie okna
        Catch ex As Exception
            If Not IsDisposed Then
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Błąd pobierania wykazu: " & MetadaneMapy.MaskujSekrety(ex.Message)
            End If
        Finally
            If Not IsDisposed Then UstawStanLadowania(False)
        End Try
    End Function

    Private Async Function WykonajSzukanieLokalneAsync(Optional wymusOdswiezeniePowiatow As Boolean = False) As Task
        Dim fraza = txtSzukaj.Text.Trim()
        If fraza.Length < 2 Then
            lblStatus.ForeColor = Color.Red
            lblStatus.Text = "Wpisz co najmniej 2 znaki nazwy gminy, powiatu albo kodu TERYT (np. Kraków, Zielonki, 1206)."
            txtSzukaj.Focus()
            Exit Function
        End If

        Dim zakres As String = "oba"
        If cmbZakresLokalny.SelectedIndex = 1 Then zakres = "powiaty"
        If cmbZakresLokalny.SelectedIndex = 2 Then zakres = "gminy"

        AnulujAktywneZapytanie()
        _ctsZapytania = New CancellationTokenSource()
        Dim token = _ctsZapytania.Token

        WyczyscSzczegoly()
        UstawStanLadowania(True)
        lblStatus.ForeColor = Color.Black
        lblStatus.Text = "Wyszukiwanie w rejestrze EZiUDP dla """ & fraza & """..."

        Try
            Dim res = Await KlientKataloguSerwerow.SzukajEziudpLokalneAsync(
                fraza:=fraza,
                zakres:=zakres,
                sciezkaCachePowiatow:=SciezkaCache("eziudp_powiaty.json"),
                wymusOdswiezeniePowiatow:=wymusOdswiezeniePowiatow,
                token:=token)

            If token.IsCancellationRequested OrElse IsDisposed Then Exit Function

            If Not res.Sukces Then
                _biezaceWpisy = New List(Of WpisKataloguSerwerow)()
                OdswiezKategorie(_biezaceWpisy)
                WypelnijListe(bezFiltraFrazy:=True)
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = res.KomunikatBledu
                Exit Function
            End If

            KatalogSerwerow.DopasujIstniejace(res.Wpisy, _istniejaceZbiory)
            _cacheSesji(TrybEziudpLokalne) = res.Wpisy
            _biezaceWpisy = res.Wpisy
            OdswiezKategorie(_biezaceWpisy)
            WypelnijListe(bezFiltraFrazy:=True)

            lblStatus.ForeColor = If(res.Wpisy.Count > 0, Color.DarkGreen, Color.DarkOrange)
            lblStatus.Text = If(res.Wpisy.Count > 0,
                                "Znaleziono " & res.Wpisy.Count & " usług lokalnych dla """ & fraza & """.",
                                "Brak usług przeglądania w EZiUDP dla """ & fraza & """. Spróbuj krótszej nazwy lub kodu TERYT.")
        Catch ex As OperationCanceledException
            'anulowane
        Catch ex As Exception
            If Not IsDisposed Then
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Błąd wyszukiwania w EZiUDP: " & MetadaneMapy.MaskujSekrety(ex.Message)
            End If
        Finally
            If Not IsDisposed Then UstawStanLadowania(False)
        End Try
    End Function

    Private Sub UstawStanLadowania(trwa As Boolean)
        prgPostep.Visible = trwa
        cmbZrodlo.Enabled = Not trwa
        cmbZakresLokalny.Enabled = Not trwa
        btnOdswiez.Enabled = Not trwa
        btnSzukajLokalne.Enabled = Not trwa
        Cursor = If(trwa, Cursors.WaitCursor, Cursors.Default)
    End Sub

    Private Sub OdswiezKategorie(wpisy As IEnumerable(Of WpisKataloguSerwerow))
        Dim poprzednia = If(cmbKategoria.SelectedItem IsNot Nothing, cmbKategoria.SelectedItem.ToString(), "Wszystkie")
        Dim unikalne = wpisy.Select(Function(w) w.Kategoria).
            Where(Function(k) Not String.IsNullOrWhiteSpace(k)).
            Distinct(StringComparer.OrdinalIgnoreCase).
            OrderBy(Function(k) k, StringComparer.CurrentCultureIgnoreCase).
            ToList()

        _blokadaZdarzen = True
        Try
            cmbKategoria.Items.Clear()
            cmbKategoria.Items.Add("Wszystkie")
            For Each k In unikalne
                cmbKategoria.Items.Add(k)
            Next
            Dim idx = cmbKategoria.Items.IndexOf(poprzednia)
            cmbKategoria.SelectedIndex = If(idx >= 0, idx, 0)
        Finally
            _blokadaZdarzen = False
        End Try
    End Sub

    Private Sub WypelnijListe(Optional bezFiltraFrazy As Boolean = False)
        Dim fraza = If(bezFiltraFrazy, "", txtSzukaj.Text)
        Dim kategoria = If(cmbKategoria.SelectedItem IsNot Nothing, cmbKategoria.SelectedItem.ToString(), "Wszystkie")
        Dim widoczne = KatalogSerwerow.Filtruj(_biezaceWpisy, fraza, kategoria, chkUkryjDodane.Checked, chkUkryjSkorowidze.Checked)

        'dynamiczny licznik i dostępność filtrów ukrywania
        Dim liczbaDodanych = _biezaceWpisy.Where(Function(w) w.CzyJuzDodany).Count()
        Dim liczbaSkorowidzow = _biezaceWpisy.Where(Function(w) w.CzySkorowidz).Count()
        _blokadaZdarzen = True
        Try
            chkUkryjDodane.Text = "Ukryj już w MapoTero (" & liczbaDodanych & ")"
            chkUkryjDodane.Enabled = (liczbaDodanych > 0)
            chkUkryjSkorowidze.Text = "Ukryj skorowidze (" & liczbaSkorowidzow & ")"
            chkUkryjSkorowidze.Enabled = (liczbaSkorowidzow > 0)
        Finally
            _blokadaZdarzen = False
        End Try

        lvUslugi.BeginUpdate()
        Try
            lvUslugi.ListViewItemSorter = Nothing
            lvUslugi.Items.Clear()
            For Each w In widoczne
                Dim stan = KatalogSerwerow.TekstStanuWMapoTero(w)
                Dim zrodloTekst = If(Not String.IsNullOrWhiteSpace(w.Obszar) AndAlso
                                     Not String.Equals(w.Obszar, "Polska", StringComparison.OrdinalIgnoreCase) AndAlso
                                     w.Dostawca <> "",
                                     w.Dostawca & " (" & w.Obszar & ")",
                                     If(w.Dostawca <> "", w.Dostawca, w.Obszar))

                Dim item As New ListViewItem(w.Nazwa) With {.Tag = w}
                item.UseItemStyleForSubItems = False
                item.SubItems.Add(w.Kategoria)
                item.SubItems.Add(zrodloTekst)
                item.SubItems.Add(w.NazwaTypu)
                Dim subStan = item.SubItems.Add(stan)
                If w.CzyJuzDodany Then
                    item.ForeColor = SystemColors.GrayText
                    subStan.ForeColor = Color.DimGray
                End If
                lvUslugi.Items.Add(item)
            Next

            If _kolumnaSortowania >= 0 Then
                lvUslugi.ListViewItemSorter = New PorownywaczKolumnListView(_kolumnaSortowania, _sortowanieMalejaco)
                lvUslugi.Sort()
            End If
        Finally
            lvUslugi.EndUpdate()
        End Try

        WyczyscSzczegoly()
        If _biezaceWpisy.Count > 0 Then
            If widoczne.Count = 0 Then
                lblStatus.ForeColor = Color.DarkOrange
                lblStatus.Text = "Brak usług pasujących do filtrów (naciśnij Esc w polu Szukaj, aby zresetować filtr)."
            Else
                lblStatus.ForeColor = Color.DarkGreen
                lblStatus.Text = "Wyświetlono " & widoczne.Count & " z " & _biezaceWpisy.Count & " usług."
            End If
        End If
    End Sub

    Private Sub txtSzukaj_TextChanged(sender As Object, e As EventArgs) Handles txtSzukaj.TextChanged
        If _blokadaZdarzen Then Exit Sub
        If cmbZrodlo.SelectedIndex <> TrybEziudpLokalne Then
            WypelnijListe()
        ElseIf _biezaceWpisy.Count = 0 AndAlso Not CzyWpisanoAdresUrl(txtSzukaj.Text) Then
            lblStatus.ForeColor = Color.Black
            lblStatus.Text = "Wpisz nazwę gminy, powiatu albo kod TERYT (np. Kraków, Zielonki, 1206) i kliknij ""Szukaj""."
        End If
        AktualizujPrzyciskWlasnegoAdresu()
    End Sub

    Private Async Sub txtSzukaj_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSzukaj.KeyDown
        If e.KeyCode = Keys.Escape Then
            If txtSzukaj.Text <> "" Then
                txtSzukaj.Clear()
                e.Handled = True
                e.SuppressKeyPress = True
            End If
        ElseIf e.KeyCode = Keys.Enter Then
            e.Handled = True
            e.SuppressKeyPress = True
            If CzyWpisanoAdresUrl(txtSzukaj.Text) Then
                RaiseEvent WlasnyAdresKlikniety(txtSzukaj.Text.Trim())
            ElseIf cmbZrodlo.SelectedIndex = TrybEziudpLokalne Then
                Await WykonajSzukanieLokalneAsync()
            ElseIf lvUslugi.Items.Count > 0 Then
                lvUslugi.Focus()
                If lvUslugi.SelectedItems.Count = 0 Then lvUslugi.Items(0).Selected = True
            End If
        ElseIf e.KeyCode = Keys.Down AndAlso lvUslugi.Items.Count > 0 Then
            lvUslugi.Focus()
            If lvUslugi.SelectedItems.Count = 0 Then lvUslugi.Items(0).Selected = True
            e.Handled = True
        End If
    End Sub

    Private Async Sub cmbZakresLokalny_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbZakresLokalny.SelectedIndexChanged
        If _blokadaZdarzen OrElse cmbZrodlo.SelectedIndex <> TrybEziudpLokalne Then Exit Sub
        If txtSzukaj.Text.Trim().Length >= 2 Then
            Await WykonajSzukanieLokalneAsync()
        End If
    End Sub

    Private Async Sub btnSzukajLokalne_Click(sender As Object, e As EventArgs) Handles btnSzukajLokalne.Click
        Await WykonajSzukanieLokalneAsync()
    End Sub

    Private Async Sub btnOdswiez_Click(sender As Object, e As EventArgs) Handles btnOdswiez.Click
        Dim tryb = cmbZrodlo.SelectedIndex
        If tryb = TrybEziudpLokalne Then
            Await WykonajSzukanieLokalneAsync(wymusOdswiezeniePowiatow:=True)
        Else
            Await ZaladujZrodloAsync(tryb, wymusOdswiezenie:=True)
        End If
    End Sub

    Private Sub cmbKategoria_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbKategoria.SelectedIndexChanged
        If _blokadaZdarzen Then Exit Sub
        WypelnijListe(bezFiltraFrazy:=(cmbZrodlo.SelectedIndex = TrybEziudpLokalne))
    End Sub

    Private Sub chkUkryjDodane_CheckedChanged(sender As Object, e As EventArgs) Handles chkUkryjDodane.CheckedChanged
        If _blokadaZdarzen Then Exit Sub
        WypelnijListe(bezFiltraFrazy:=(cmbZrodlo.SelectedIndex = TrybEziudpLokalne))
    End Sub

    Private Sub chkUkryjSkorowidze_CheckedChanged(sender As Object, e As EventArgs) Handles chkUkryjSkorowidze.CheckedChanged
        If _blokadaZdarzen Then Exit Sub
        WypelnijListe(bezFiltraFrazy:=(cmbZrodlo.SelectedIndex = TrybEziudpLokalne))
    End Sub

    Private Sub lvUslugi_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvUslugi.SelectedIndexChanged
        AnulujAktywnyPodgladCap()

        If lvUslugi.SelectedItems.Count = 0 Then
            WyczyscSzczegoly()
            Exit Sub
        End If

        Dim w = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
        If w Is Nothing Then
            WyczyscSzczegoly()
            Exit Sub
        End If

        txtAdresPodglad.Text = w.Adres
        Dim linie As New List(Of String)()
        If w.CzyJuzDodany Then
            linie.Add("ℹ Ta usługa jest już w MapoTero jako zbiór """ & w.JuzDodanyZbior & """. Możesz kliknąć ""Dalej"", aby wybrać inne warstwy tego serwera (np. punkty, linie) i zapisać je pod nową nazwą.")
        End If
        If w.CzySkorowidz Then
            linie.Add("Skorowidz / siatka arkuszy (pokazuje zasięgi lub numery arkuszy danych).")
        End If
        Dim opisTypu = KatalogSerwerow.OpisTypuUslugi(w.Typ)
        If opisTypu <> "" Then linie.Add(opisTypu)
        If Not String.IsNullOrWhiteSpace(w.Opis) Then linie.Add(w.Opis)
        If Not String.IsNullOrWhiteSpace(w.Dostawca) OrElse Not String.IsNullOrWhiteSpace(w.Obszar) Then
            linie.Add("Dostawca: " & If(w.Dostawca <> "", w.Dostawca, "-") & " | Obszar: " & If(w.Obszar <> "", w.Obszar, "-"))
        End If

        Dim kluczCap = KatalogSerwerow.NormalizujAdresDoPorownania(w.Adres)
        Dim cachedCap As String = Nothing
        If kluczCap <> "" AndAlso _cachePodgladuCap.TryGetValue(kluczCap, cachedCap) AndAlso Not String.IsNullOrWhiteSpace(cachedCap) Then
            linie.Add(cachedCap)
        End If

        txtOpisPodglad.Text = String.Join(vbCrLf, linie)
        btnWybierz.Enabled = True
        btnKopiujUrl.Enabled = True

        'leniwy podgląd GetCapabilities z serwera (z buforowaniem w sesji)
        If kluczCap <> "" AndAlso Not _cachePodgladuCap.ContainsKey(kluczCap) AndAlso Not KafleXyz.CzyAdresXyz(w.Adres) Then
            UruchomLeniwyPodgladCap(w, kluczCap)
        End If
    End Sub

    Private Sub AnulujAktywnyPodgladCap()
        If _ctsPodgladuCap IsNot Nothing Then
            _ctsPodgladuCap.Cancel()
            _ctsPodgladuCap.Dispose()
            _ctsPodgladuCap = Nothing
        End If
    End Sub

    Private Sub UruchomLeniwyPodgladCap(w As WpisKataloguSerwerow, kluczCap As String)
        _ctsPodgladuCap = New CancellationTokenSource()
        Dim token = _ctsPodgladuCap.Token
        Dim adres = w.Adres
        Dim ignorujSsl = w.IgnorujSsl

        Task.Run(Async Function() As Task
            Try
                Await Task.Delay(250, token).ConfigureAwait(False)
                If token.IsCancellationRequested Then Return

                Dim res = Await KlientCapabilities.PobierzAsync(adres, ignorujSsl, timeoutSekundy:=4, token:=token).ConfigureAwait(False)
                If token.IsCancellationRequested OrElse Not res.Sukces Then Return

                Dim tekstPodgladu = KatalogSerwerow.FormatujPodgladCapabilities(res)
                If String.IsNullOrWhiteSpace(tekstPodgladu) Then Return

                BeginInvoke(Sub()
                    If IsDisposed OrElse token.IsCancellationRequested Then Return
                    _cachePodgladuCap(kluczCap) = tekstPodgladu
                    If lvUslugi.SelectedItems.Count > 0 Then
                        Dim sel = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
                        If sel IsNot Nothing AndAlso String.Equals(KatalogSerwerow.NormalizujAdresDoPorownania(sel.Adres), kluczCap, StringComparison.OrdinalIgnoreCase) Then
                            Dim noweLinie As New List(Of String)(txtOpisPodglad.Lines)
                            If Not noweLinie.Any(Function(l) l.StartsWith("Tytuł z serwera:", StringComparison.OrdinalIgnoreCase) OrElse l.StartsWith("Warstwy", StringComparison.OrdinalIgnoreCase)) Then
                                noweLinie.Add(tekstPodgladu)
                                txtOpisPodglad.Text = String.Join(vbCrLf, noweLinie)
                            End If
                        End If
                    End If
                End Sub)
            Catch
                'błędy leniwego podglądu w tle są ignorowane
            End Try
        End Function, token)
    End Sub

    Private Sub WyczyscSzczegoly()
        AnulujAktywnyPodgladCap()
        txtAdresPodglad.Text = ""
        txtOpisPodglad.Text = ""
        btnWybierz.Enabled = False
        btnKopiujUrl.Enabled = False
    End Sub

    Private Sub lvUslugi_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles lvUslugi.MouseDoubleClick
        If lvUslugi.HitTest(e.Location).Item IsNot Nothing Then
            ZatwierdzWybor()
        End If
    End Sub

    Private Sub lvUslugi_KeyDown(sender As Object, e As KeyEventArgs) Handles lvUslugi.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            ZatwierdzWybor()
        ElseIf e.Control AndAlso e.KeyCode = Keys.C Then
            KopiujBiezacyUrl()
            e.Handled = True
        End If
    End Sub

    Private Sub KopiujBiezacyUrl()
        Dim url = txtAdresPodglad.Text.Trim()
        If String.IsNullOrWhiteSpace(url) AndAlso lvUslugi.SelectedItems.Count > 0 Then
            Dim w = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
            If w IsNot Nothing Then url = w.Adres
        End If
        If Not String.IsNullOrWhiteSpace(url) Then
            Try
                Clipboard.SetText(url)
                lblStatus.ForeColor = Color.DarkGreen
                lblStatus.Text = "Skopiowano adres URL usługi do schowka: " & url
            Catch ex As Exception
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Nie udało się skopiować adresu do schowka: " & ex.Message
            End Try
        End If
    End Sub

    Private Sub btnKopiujUrl_Click(sender As Object, e As EventArgs) Handles btnKopiujUrl.Click
        KopiujBiezacyUrl()
    End Sub

    Private Sub mnuDalej_Click(sender As Object, e As EventArgs) Handles mnuDalej.Click
        ZatwierdzWybor()
    End Sub

    Private Sub mnuKopiujUrl_Click(sender As Object, e As EventArgs) Handles mnuKopiujUrl.Click
        KopiujBiezacyUrl()
    End Sub

    Private Sub mnuKopiujNazwe_Click(sender As Object, e As EventArgs) Handles mnuKopiujNazwe.Click
        If lvUslugi.SelectedItems.Count > 0 Then
            Dim w = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
            If w IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(w.Nazwa) Then
                Try
                    Clipboard.SetText(w.Nazwa)
                    lblStatus.ForeColor = Color.DarkGreen
                    lblStatus.Text = "Skopiowano nazwę usługi do schowka: """ & w.Nazwa & """."
                Catch ex As Exception
                    lblStatus.ForeColor = Color.Red
                    lblStatus.Text = "Nie udało się skopiować nazwy: " & ex.Message
                End Try
            End If
        End If
    End Sub

    Private Sub mnuOtworzPrzegladarka_Click(sender As Object, e As EventArgs) Handles mnuOtworzPrzegladarka.Click
        Dim url = txtAdresPodglad.Text.Trim()
        If String.IsNullOrWhiteSpace(url) AndAlso lvUslugi.SelectedItems.Count > 0 Then
            Dim w = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
            If w IsNot Nothing Then url = w.Adres
        End If
        If Not String.IsNullOrWhiteSpace(url) Then
            Try
                Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
            Catch ex As Exception
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Nie udało się otworzyć adresu w przeglądarce: " & ex.Message
            End Try
        End If
    End Sub

    Private Sub ctxMenuUslugi_Opening(sender As Object, e As CancelEventArgs) Handles ctxMenuUslugi.Opening
        Dim maZaznaczenie = (lvUslugi.SelectedItems.Count > 0)
        mnuDalej.Enabled = maZaznaczenie
        mnuKopiujUrl.Enabled = maZaznaczenie AndAlso Not String.IsNullOrWhiteSpace(txtAdresPodglad.Text)
        mnuKopiujNazwe.Enabled = maZaznaczenie
        mnuOtworzPrzegladarka.Enabled = maZaznaczenie AndAlso Not String.IsNullOrWhiteSpace(txtAdresPodglad.Text)
    End Sub

    Private Sub btnWybierz_Click(sender As Object, e As EventArgs) Handles btnWybierz.Click
        ZatwierdzWybor()
    End Sub

    Private Sub btnWlasnyAdres_Click(sender As Object, e As EventArgs) Handles btnWlasnyAdres.Click
        Dim wstepny = If(CzyWpisanoAdresUrl(txtSzukaj.Text), txtSzukaj.Text.Trim(), "")
        RaiseEvent WlasnyAdresKlikniety(wstepny)
    End Sub

    Private Sub btnAnuluj_Click(sender As Object, e As EventArgs) Handles btnAnuluj.Click
        RaiseEvent AnulujKlikniete()
    End Sub

    Private Sub ZatwierdzWybor()
        If lvUslugi.SelectedItems.Count = 0 Then Exit Sub
        Dim w = TryCast(lvUslugi.SelectedItems(0).Tag, WpisKataloguSerwerow)
        If w Is Nothing Then Exit Sub
        RaiseEvent UslugaWybrana(w)
    End Sub

    Private Sub lvUslugi_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles lvUslugi.ColumnClick
        If e.Column = _kolumnaSortowania Then
            _sortowanieMalejaco = Not _sortowanieMalejaco
        Else
            _kolumnaSortowania = e.Column
            _sortowanieMalejaco = False
        End If
        lvUslugi.ListViewItemSorter = New PorownywaczKolumnListView(_kolumnaSortowania, _sortowanieMalejaco)
        lvUslugi.Sort()
    End Sub

    Private NotInheritable Class PorownywaczKolumnListView
        Implements IComparer

        Private ReadOnly _kolumna As Integer
        Private ReadOnly _malejaco As Boolean

        Public Sub New(kolumna As Integer, malejaco As Boolean)
            _kolumna = kolumna
            _malejaco = malejaco
        End Sub

        Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim lx = DirectCast(x, ListViewItem)
            Dim ly = DirectCast(y, ListViewItem)
            Dim tx = If(_kolumna < lx.SubItems.Count, lx.SubItems(_kolumna).Text, "")
            Dim ty = If(_kolumna < ly.SubItems.Count, ly.SubItems(_kolumna).Text, "")
            Dim wynik = String.Compare(tx, ty, StringComparison.CurrentCultureIgnoreCase)
            Return If(_malejaco, -wynik, wynik)
        End Function
    End Class

End Class
