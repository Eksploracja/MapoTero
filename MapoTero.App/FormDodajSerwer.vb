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
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Okno kreatora dodawania i analizy serwerów WMS / WMTS (GetCapabilities Parser).
''' Automatycznie pobiera metadane, buduje hierarchię warstw, wylicza optymalny rozmiar piksela
''' i zapisuje zbiór map do formatu .txt MapoTero.
''' </summary>
Public Class FormDodajSerwer

    ''' <summary>Nazwa nowo dodanego zbioru map (plik bez rozszerzenia .txt) po zatwierdzeniu formularza.</summary>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property WynikNazwaZbioru As String = ""

    Private _wynikCap As WynikCapabilities
    Private _blokadaCheck As Boolean = False
    Private ReadOnly _rozmiaryPiksela As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
    Private _wybranaWarstwaWms As WarstwaWmsInfo
    Private _wybranaWarstwaWmts As WarstwaWmts
    Private _nazwaZbioruZmienionaRecznie As Boolean = False
    Private _nazwaZbioruZKatalogu As Boolean = False
    Private ReadOnly _srodekMapy As PunktGeo?
    Private ReadOnly _istniejaceZbiory As New List(Of KeyValuePair(Of String, String))
    Private _wybranyKatalogowyTytul As String = ""
    Private _wybranyKatalogowyAdres As String = ""
    Private _opcjeKafli As OpcjeKafliXyz
    ''' <summary>Przerwanie pobierania próbki lub listy warstw przy zamknięciu okna.</summary>
    Private _przerwanieProbki As CancellationTokenSource
    Private _przerwaniePolaczenia As CancellationTokenSource

    Public Sub New(Optional domyslnyAdres As String = "",
                   Optional srodekMapy As PunktGeo? = Nothing,
                   Optional istniejaceZbiory As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing)
        InitializeComponent()
        _srodekMapy = srodekMapy
        If istniejaceZbiory IsNot Nothing Then
            _istniejaceZbiory.AddRange(istniejaceZbiory)
        End If
        pnlKatalog.UstawIstniejaceZbiory(_istniejaceZbiory)
        If Not String.IsNullOrWhiteSpace(domyslnyAdres) Then
            txtAdres.Text = domyslnyAdres.Trim()
            PokazKrokWarstwy()
        Else
            PokazKrokKatalog()
        End If
    End Sub

    Private Sub PokazKrokKatalog()
        pnlKatalog.Visible = True
        pnlKrokWarstwy.Visible = False
        Me.CancelButton = pnlKatalog.PrzyciskAnuluj
        pnlKatalog.FokusujSzukaj()
    End Sub

    Private Sub PokazKrokWarstwy()
        pnlKatalog.Visible = False
        pnlKrokWarstwy.Visible = True
        Me.CancelButton = btnAnuluj
    End Sub

    Private Sub FormDodajSerwer_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If pnlKatalog.Visible Then
            pnlKatalog.FokusujSzukaj()
        End If
    End Sub

    Private Sub UstawWidocznoscOpcjiXyz(widoczne As Boolean)
        lblZoomZakres.Visible = widoczne
        nudZMin.Visible = widoczne
        lblZoomDo.Visible = widoczne
        nudZMax.Visible = widoczne
        chkTms.Visible = widoczne
        chkNakladka.Visible = widoczne
    End Sub

    Private Sub WypelnijOpcjeXyzZResultatu()
        If _wynikCap Is Nothing OrElse (_wynikCap.Typ <> TypUslugiMapowej.Xyz AndAlso _wynikCap.Typ <> TypUslugiMapowej.Tms) Then
            _opcjeKafli = Nothing
            UstawWidocznoscOpcjiXyz(False)
            Exit Sub
        End If
        _opcjeKafli = If(_wynikCap.OpcjeKafli, New OpcjeKafliXyz With {.Typ = _wynikCap.Typ}).Kopia()
        nudZMin.Value = Math.Max(nudZMin.Minimum, Math.Min(nudZMin.Maximum, CDec(_opcjeKafli.ZMin)))
        nudZMax.Value = Math.Max(nudZMin.Value, Math.Min(nudZMax.Maximum, CDec(_opcjeKafli.ZMax)))
        chkTms.Checked = (_opcjeKafli.Typ = TypUslugiMapowej.Tms)
        chkNakladka.Checked = _opcjeKafli.Nakladka
        UstawWidocznoscOpcjiXyz(True)
    End Sub

    Private Sub OdswiezOpcjeKafliZPol()
        If _opcjeKafli Is Nothing Then Exit Sub
        Dim z0 As Integer = CInt(nudZMin.Value)
        Dim z1 As Integer = Math.Max(z0, CInt(nudZMax.Value))
        If CInt(nudZMax.Value) < z0 Then nudZMax.Value = CDec(z0)
        _opcjeKafli.ZMin = z0
        _opcjeKafli.ZMax = z1
        _opcjeKafli.Typ = If(chkTms.Checked, TypUslugiMapowej.Tms, TypUslugiMapowej.Xyz)
        _opcjeKafli.Nakladka = chkNakladka.Checked
        If _wynikCap IsNot Nothing AndAlso _wynikCap.SzablonKafli <> "" Then
            Dim nazwaWarstwy = If(_wybranaWarstwaWmts IsNot Nothing, _wybranaWarstwaWmts.Identyfikator, _wynikCap.Tytul)
            _wynikCap.UslugaWmts = KafleXyz.UtworzUsluge(_wynikCap.SzablonKafli, {nazwaWarstwy}, _opcjeKafli)
            If _wynikCap.UslugaWmts.Warstwy.Count > 0 Then _wybranaWarstwaWmts = _wynikCap.UslugaWmts.Warstwy(0)
        End If
    End Sub

    Private Async Sub pnlKatalog_UslugaWybrana(wpis As WpisKataloguSerwerow) Handles pnlKatalog.UslugaWybrana
        If wpis Is Nothing Then Exit Sub
        _wybranyKatalogowyTytul = If(wpis.Nazwa, "").Trim()
        _wybranyKatalogowyAdres = If(wpis.Adres, "").Trim()
        txtAdres.Text = wpis.Adres
        chkIgnorujSsl.Checked = wpis.IgnorujSsl
        txtFiltr.Clear()
        Dim sugerowana = If(Not String.IsNullOrWhiteSpace(wpis.SugerowanaNazwaZbioru),
                            wpis.SugerowanaNazwaZbioru,
                            PlikWarstw.OczyscNazweZbioru(wpis.Nazwa))
        If Not String.IsNullOrWhiteSpace(sugerowana) Then
            txtNazwaZbioru.Text = sugerowana
            _nazwaZbioruZKatalogu = True
            _nazwaZbioruZmienionaRecznie = False
        Else
            _nazwaZbioruZKatalogu = False
            _nazwaZbioruZmienionaRecznie = False
        End If
        PokazKrokWarstwy()
        Await PolaczIZaladujWarstwyAsync()
    End Sub

    Private Async Sub pnlKatalog_WlasnyAdresKlikniety(wstepnyAdres As String) Handles pnlKatalog.WlasnyAdresKlikniety
        _wybranyKatalogowyTytul = ""
        _wybranyKatalogowyAdres = ""
        _nazwaZbioruZKatalogu = False
        _nazwaZbioruZmienionaRecznie = False
        txtFiltr.Clear()

        If Not String.IsNullOrWhiteSpace(wstepnyAdres) Then
            txtAdres.Text = wstepnyAdres.Trim()
            chkIgnorujSsl.Checked = False
            txtNazwaZbioru.Clear()
            PokazKrokWarstwy()
            Await PolaczIZaladujWarstwyAsync()
        Else
            If _wynikCap IsNot Nothing Then
                _wynikCap = Nothing
                _wybranaWarstwaWms = Nothing
                _wybranaWarstwaWmts = Nothing
                _rozmiaryPiksela.Clear()
                txtAdres.Clear()
                txtNazwaZbioru.Clear()
                tvWarstwy.Nodes.Clear()
                WyczyscSzczegoly()
                UstawWidocznoscOpcjiXyz(False)
                AktualizujLiczbeZaznaczonych()
                lblStatus.ForeColor = Color.Black
                lblStatus.Text = "Wpisz lub wklej adres serwera WMS / WMTS lub szablon kafelków XYZ/TMS i kliknij ""Pobierz listę warstw""."
            End If
            PokazKrokWarstwy()
            txtAdres.Focus()
            txtAdres.SelectAll()
        End If
    End Sub

    Private Sub pnlKatalog_AnulujKlikniete() Handles pnlKatalog.AnulujKlikniete
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnWrocDoKatalogu_Click(sender As Object, e As EventArgs) Handles btnWrocDoKatalogu.Click
        _przerwaniePolaczenia?.Cancel()
        _przerwanieProbki?.Cancel()
        UstawStanPolaczenia(trwa:=False)
        PokazKrokKatalog()
    End Sub

    Private Async Sub txtAdres_KeyDown(sender As Object, e As KeyEventArgs) Handles txtAdres.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            e.SuppressKeyPress = True
            Await PolaczIZaladujWarstwyAsync()
        End If
    End Sub

    Private Async Sub btnPolacz_Click(sender As Object, e As EventArgs) Handles btnPolacz.Click
        Await PolaczIZaladujWarstwyAsync()
    End Sub

    Private Async Function PolaczIZaladujWarstwyAsync() As Task
        Dim adres As String = txtAdres.Text.Trim()
        If Not String.Equals(adres, _wybranyKatalogowyAdres, StringComparison.OrdinalIgnoreCase) Then
            _wybranyKatalogowyTytul = ""
            _wybranyKatalogowyAdres = ""
            _nazwaZbioruZKatalogu = False
        End If
        If String.IsNullOrWhiteSpace(adres) Then
            lblStatus.ForeColor = Color.Red
            lblStatus.Text = "Wpisz lub wklej adres serwera WMS / WMTS lub szablon kafelków XYZ/TMS."
            txtAdres.Focus()
            Exit Function
        End If

        _przerwaniePolaczenia?.Cancel()
        _przerwaniePolaczenia?.Dispose()
        _przerwaniePolaczenia = New CancellationTokenSource()
        Dim token = _przerwaniePolaczenia.Token

        _wynikCap = Nothing
        _wybranaWarstwaWms = Nothing
        _wybranaWarstwaWmts = Nothing
        _rozmiaryPiksela.Clear()
        tvWarstwy.Nodes.Clear()
        WyczyscSzczegoly()
        UstawWidocznoscOpcjiXyz(False)
        AktualizujLiczbeZaznaczonych()

        UstawStanPolaczenia(trwa:=True)
        lblStatus.ForeColor = Color.Black
        lblStatus.Text = If(KafleXyz.CzyAdresXyz(adres),
                            "Sprawdzanie szablonu kafelków XYZ/TMS...",
                            "Łączenie z serwerem i pobieranie opisu usługi...")

        Try
            _wynikCap = Await KlientCapabilities.PobierzAsync(adres, chkIgnorujSsl.Checked, timeoutSekundy:=25, token:=token, srodekTestowy:=_srodekMapy)
            If token.IsCancellationRequested OrElse IsDisposed Then Exit Function

            If Not _wynikCap.Sukces Then
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Błąd: " & MetadaneMapy.MaskujSekrety(_wynikCap.KomunikatBledu)
                UstawWidocznoscOpcjiXyz(False)
                Exit Function
            End If

            'sukces - wypełnienie danych usługi
            WypelnijOpcjeXyzZResultatu()
            lblStatus.ForeColor = Color.DarkGreen
            Dim typOpis As String
            If _wynikCap.Typ = TypUslugiMapowej.Wms Then
                typOpis = "Serwer WMS " & _wynikCap.UslugaWms.Wersja
            ElseIf _wynikCap.Typ = TypUslugiMapowej.Xyz OrElse _wynikCap.Typ = TypUslugiMapowej.Tms Then
                typOpis = "Usługa kafelkowa " & KafleXyz.NazwaTypu(_wynikCap.Typ) &
                          " (z=" & _opcjeKafli.ZMin & ".." & _opcjeKafli.ZMax & ", " & _opcjeKafli.RozmiarKafla & " px)"
            Else
                typOpis = "Usługa kafelkowa WMTS"
            End If
            lblStatus.Text = "Połączono pomyślnie: " & typOpis & " - " & _wynikCap.Tytul
            If _wynikCap.Ostrzezenia.Count > 0 Then
                lblStatus.Text &= " (" & String.Join("; ", _wynikCap.Ostrzezenia) & ")"
            End If

            'propozycja nazwy pliku zbioru
            If (Not _nazwaZbioruZmienionaRecznie AndAlso Not _nazwaZbioruZKatalogu) OrElse String.IsNullOrWhiteSpace(txtNazwaZbioru.Text) Then
                txtNazwaZbioru.Text = PlikWarstw.OczyscNazweZbioru(_wynikCap.Tytul)
                _nazwaZbioruZmienionaRecznie = False
                _nazwaZbioruZKatalogu = False
            End If

            If _wybranyKatalogowyTytul <> "" AndAlso
               (_wynikCap.Typ = TypUslugiMapowej.Xyz OrElse _wynikCap.Typ = TypUslugiMapowej.Tms) AndAlso
               _wynikCap.UslugaWmts IsNot Nothing AndAlso
               _wynikCap.UslugaWmts.Warstwy.Count = 1 AndAlso
               String.IsNullOrWhiteSpace(_wynikCap.UslugaWmts.Warstwy(0).Tytul) Then
                _wynikCap.UslugaWmts.Warstwy(0).Tytul = _wybranyKatalogowyTytul
            End If

            WypelnijDrzewoWarstw()

        Catch ex As OperationCanceledException When token.IsCancellationRequested
            'anulowane przez zamknięcie okna lub nowe połączenie
        Catch ex As Exception
            If Not IsDisposed Then
                lblStatus.ForeColor = Color.Red
                lblStatus.Text = "Nieoczekiwany błąd: " & MetadaneMapy.MaskujSekrety(ex.Message)
            End If
        Finally
            If Not IsDisposed Then UstawStanPolaczenia(trwa:=False)
        End Try
    End Function

    Private Sub UstawStanPolaczenia(trwa As Boolean)
        btnPolacz.Enabled = Not trwa
        txtAdres.Enabled = Not trwa
        chkIgnorujSsl.Enabled = Not trwa
        If trwa Then btnZapisz.Enabled = False
        prgPostep.Visible = trwa
        Cursor = If(trwa, Cursors.WaitCursor, Cursors.Default)
    End Sub

    ''' <summary>Wypełnia kontrolkę TreeView warstwami usługi (WMS lub WMTS).</summary>
    Private Sub WypelnijDrzewoWarstw()
        tvWarstwy.BeginUpdate()
        tvWarstwy.Nodes.Clear()
        _wybranaWarstwaWms = Nothing
        _wybranaWarstwaWmts = Nothing
        WyczyscSzczegoly()

        Dim filtr As String = txtFiltr.Text.Trim()

        If _wynikCap.Typ = TypUslugiMapowej.Wms AndAlso _wynikCap.UslugaWms IsNot Nothing Then
            For Each w In _wynikCap.UslugaWms.Warstwy
                Dim node = UtworzWezelWms(w, filtr)
                If node IsNot Nothing Then tvWarstwy.Nodes.Add(node)
            Next
        ElseIf KafleXyz.CzyKafelkowy(_wynikCap.Typ) AndAlso _wynikCap.UslugaWmts IsNot Nothing Then
            For Each w In _wynikCap.UslugaWmts.Warstwy
                If PasujeDoFiltra(w.Identyfikator, w.Tytul, filtr) Then
                    Dim node As New TreeNode(If(w.Tytul <> "" AndAlso w.Tytul <> w.Identyfikator, w.Tytul & " (" & w.Identyfikator & ")", w.Identyfikator)) With {
                        .Tag = w
                    }
                    tvWarstwy.Nodes.Add(node)
                End If
            Next
        End If

        tvWarstwy.ExpandAll()
        tvWarstwy.EndUpdate()

        'jeśli na serwerze jest tylko 1 warstwa do pobrania, zaznacz ją automatycznie
        Dim wszystkie = PobierzZaznaczoneWarstwyZDrzewa()
        If tvWarstwy.Nodes.Count = 1 AndAlso tvWarstwy.Nodes(0).Tag IsNot Nothing Then
            tvWarstwy.Nodes(0).Checked = True
            tvWarstwy.SelectedNode = tvWarstwy.Nodes(0)
        End If
        AktualizujLiczbeZaznaczonych()
    End Sub

    Private Function UtworzWezelWms(w As WarstwaWmsInfo, filtr As String) As TreeNode
        Dim maPasujaceDziecko As Boolean = False
        Dim dzieciNode As New List(Of TreeNode)

        For Each d In w.Dzieci
            Dim dn = UtworzWezelWms(d, filtr)
            If dn IsNot Nothing Then
                dzieciNode.Add(dn)
                maPasujaceDziecko = True
            End If
        Next

        Dim pasujeBezposrednio As Boolean = PasujeDoFiltra(w.Nazwa, w.Tytul, filtr)
        If Not pasujeBezposrednio AndAlso Not maPasujaceDziecko Then Return Nothing

        Dim tekstWezla As String
        If Not String.IsNullOrEmpty(w.Tytul) AndAlso Not String.IsNullOrEmpty(w.Nazwa) AndAlso w.Tytul <> w.Nazwa Then
            tekstWezla = w.Tytul & " (" & w.Nazwa & ")"
        ElseIf Not String.IsNullOrEmpty(w.Tytul) Then
            tekstWezla = w.Tytul
        Else
            tekstWezla = w.Nazwa
        End If

        Dim node As New TreeNode(tekstWezla)
        node.Tag = w
        For Each dn In dzieciNode
            node.Nodes.Add(dn)
        Next
        Return node
    End Function

    Private Shared Function PasujeDoFiltra(nazwa As String, tytul As String, filtr As String) As Boolean
        If String.IsNullOrWhiteSpace(filtr) Then Return True
        Dim f = filtr.ToLowerInvariant()
        If Not String.IsNullOrEmpty(nazwa) AndAlso nazwa.ToLowerInvariant().Contains(f) Then Return True
        If Not String.IsNullOrEmpty(tytul) AndAlso tytul.ToLowerInvariant().Contains(f) Then Return True
        Return False
    End Function

    Private Sub txtFiltr_TextChanged(sender As Object, e As EventArgs) Handles txtFiltr.TextChanged
        If _wynikCap IsNot Nothing AndAlso _wynikCap.Sukces Then
            WypelnijDrzewoWarstw()
        End If
    End Sub

    Private Sub tvWarstwy_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvWarstwy.AfterCheck
        If _blokadaCheck Then Exit Sub
        _blokadaCheck = True
        Try
            'kaskadowe zaznaczanie podwęzłów
            UstawCheckDlaDzieci(e.Node, e.Node.Checked)
        Finally
            _blokadaCheck = False
        End Try
        AktualizujLiczbeZaznaczonych()
    End Sub

    Private Sub UstawCheckDlaDzieci(rodzic As TreeNode, stan As Boolean)
        For Each n As TreeNode In rodzic.Nodes
            n.Checked = stan
            UstawCheckDlaDzieci(n, stan)
        Next
    End Sub

    Private Sub AktualizujLiczbeZaznaczonych()
        Dim ile As Integer = 0
        ZliczZaznaczone(tvWarstwy.Nodes, ile)
        lblLiczbaZaznaczonych.Text = "Zaznaczono warstw do pobrania: " & ile
        btnZapisz.Enabled = (ile > 0)
    End Sub

    Private Sub ZliczZaznaczone(nodes As TreeNodeCollection, ByRef ile As Integer)
        For Each n As TreeNode In nodes
            If n.Checked Then
                If TypeOf n.Tag Is WarstwaWmsInfo Then
                    Dim w = DirectCast(n.Tag, WarstwaWmsInfo)
                    If w.CzyWarstwaDoPobrania Then ile += 1
                ElseIf TypeOf n.Tag Is WarstwaWmts Then
                    ile += 1
                End If
            End If
            ZliczZaznaczone(n.Nodes, ile)
        Next
    End Sub

    Private Sub tvWarstwy_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvWarstwy.AfterSelect
        If e.Node Is Nothing OrElse e.Node.Tag Is Nothing Then
            WyczyscSzczegoly()
            Exit Sub
        End If

        If TypeOf e.Node.Tag Is WarstwaWmsInfo Then
            _wybranaWarstwaWms = DirectCast(e.Node.Tag, WarstwaWmsInfo)
            _wybranaWarstwaWmts = Nothing
            PokazSzczegolyWms(_wybranaWarstwaWms)
        ElseIf TypeOf e.Node.Tag Is WarstwaWmts Then
            _wybranaWarstwaWmts = DirectCast(e.Node.Tag, WarstwaWmts)
            _wybranaWarstwaWms = Nothing
            PokazSzczegolyWmts(_wybranaWarstwaWmts)
        End If
    End Sub

    Private Sub PokazSzczegolyWms(w As WarstwaWmsInfo)
        lblNazwaWartosc.Text = If(w.Nazwa <> "", w.Nazwa, "(brak nazwy - grupa warstw)")
        lblTytulWartosc.Text = If(w.Tytul <> "", w.Tytul, "-")
        txtAbstrakt.Text = If(w.Abstrakt <> "", w.Abstrakt, "(brak opisu)")

        If w.ObslugiwaneCrs.Count > 0 Then
            lblUkladyWartosc.Text = String.Join(", ", w.ObslugiwaneCrs.Take(8))
            If w.ObslugiwaneCrs.Count > 8 Then lblUkladyWartosc.Text &= "..."
        Else
            lblUkladyWartosc.Text = "dziedziczone z serwera"
        End If

        If w.MinScaleDenominator > 0 OrElse w.MaxScaleDenominator > 0 Then
            lblSkalaWartosc.Text = "od 1:" & w.MinScaleDenominator.ToString("0", CultureInfo.InvariantCulture) &
                                   If(w.MaxScaleDenominator > 0, " do 1:" & w.MaxScaleDenominator.ToString("0", CultureInfo.InvariantCulture), "")
        Else
            lblSkalaWartosc.Text = "widoczna w każdej skali"
        End If

        Dim px As Double
        If _rozmiaryPiksela.TryGetValue(w.Nazwa, px) Then
            UstawPolePiksela(px)
        Else
            px = w.ZalecanyRozmiarPiksela()
            UstawPolePiksela(px)
        End If
        nudRozmiarPiksela.Enabled = w.CzyWarstwaDoPobrania
        btnPobierzProbke.Enabled = w.CzyWarstwaDoPobrania
        picPodglad.Image = Nothing
    End Sub

    Private Sub PokazSzczegolyWmts(w As WarstwaWmts)
        lblNazwaWartosc.Text = w.Identyfikator
        lblTytulWartosc.Text = If(w.Tytul <> "", w.Tytul, "-")
        Dim opisDodatkowy As String = ""
        If _opcjeKafli IsNot Nothing Then
            opisDodatkowy = ", zakres zoom " & _opcjeKafli.ZMin & ".." & _opcjeKafli.ZMax &
                            ", kafel " & _opcjeKafli.RozmiarKafla & " px" &
                            If(_opcjeKafli.Subdomeny.Count > 0, ", subdomeny: " & String.Join(",", _opcjeKafli.Subdomeny), "")
        End If
        txtAbstrakt.Text = "Warstwa kafelkowa " & KafleXyz.NazwaTypu(_wynikCap.Typ) &
                           ". Formaty: " & String.Join(", ", w.Formaty) & opisDodatkowy

        Dim uklady As New List(Of String)
        For Each p In w.Powiazania
            Dim z = _wynikCap.UslugaWmts.Zestaw(p.Zestaw)
            If z IsNot Nothing AndAlso Not String.IsNullOrEmpty(z.Crs) Then uklady.Add(z.Crs)
        Next
        lblUkladyWartosc.Text = If(uklady.Count > 0, String.Join(", ", uklady.Take(5)), "-")
        lblSkalaWartosc.Text = If(_opcjeKafli IsNot Nothing, "poziomy z=" & _opcjeKafli.ZMin & ".." & _opcjeKafli.ZMax, "zestawy kafli: " & w.Powiazania.Count)

        Dim px As Double
        If _rozmiaryPiksela.TryGetValue(w.Identyfikator, px) Then
            UstawPolePiksela(px)
        Else
            Dim sugerowany As Double = WyznaczSugerowanyRozmiarPikselaWmts(w)
            _rozmiaryPiksela(w.Identyfikator) = sugerowany
            UstawPolePiksela(sugerowany)
        End If
        nudRozmiarPiksela.Enabled = True
        btnPobierzProbke.Enabled = True
        If _wynikCap IsNot Nothing AndAlso _wynikCap.PrzykladowyKafel IsNot Nothing AndAlso _wynikCap.PrzykladowyKafel.Length > 0 Then
            Try
                Using ms As New MemoryStream(_wynikCap.PrzykladowyKafel)
                    Using staryImg = picPodglad.Image
                        picPodglad.Image = New Bitmap(Image.FromStream(ms))
                    End Using
                End Using
            Catch
                picPodglad.Image = Nothing
            End Try
        Else
            picPodglad.Image = Nothing
        End If
    End Sub

    ''' <summary>Wyznacza sugerowany rozmiar piksela dla warstwy WMTS/XYZ/TMS na podstawie jej nazwy i dostępnych macierzy.</summary>
    Private Function WyznaczSugerowanyRozmiarPikselaWmts(w As WarstwaWmts) As Double
        Return KafleXyz.SugerowanyRozmiarPiksela(_wynikCap?.UslugaWmts, w)
    End Function

    ''' <summary>
    ''' Rozmiar piksela w polu ograniczony do jego zakresu (0,01-500) - zalecany rozmiar z mianownika skali warstwy
    ''' przeglądowej może być większy, a przypisanie spoza zakresu kończyło się wyjątkiem przy wyborze warstwy.
    ''' </summary>
    Private Sub UstawPolePiksela(px As Double)
        If Double.IsNaN(px) OrElse Double.IsInfinity(px) Then px = 1.0
        nudRozmiarPiksela.Value = Math.Max(nudRozmiarPiksela.Minimum, Math.Min(nudRozmiarPiksela.Maximum, CDec(px)))
    End Sub

    Private Sub WyczyscSzczegoly()
        lblNazwaWartosc.Text = "-"
        lblTytulWartosc.Text = "-"
        txtAbstrakt.Text = ""
        lblUkladyWartosc.Text = "-"
        lblSkalaWartosc.Text = "-"
        nudRozmiarPiksela.Value = 1.0D
        nudRozmiarPiksela.Enabled = False
        btnPobierzProbke.Enabled = False
        picPodglad.Image = Nothing
    End Sub

    Private Sub nudRozmiarPiksela_ValueChanged(sender As Object, e As EventArgs) Handles nudRozmiarPiksela.ValueChanged
        If _wybranaWarstwaWms IsNot Nothing AndAlso _wybranaWarstwaWms.CzyWarstwaDoPobrania Then
            _rozmiaryPiksela(_wybranaWarstwaWms.Nazwa) = CDbl(nudRozmiarPiksela.Value)
        ElseIf _wybranaWarstwaWmts IsNot Nothing Then
            _rozmiaryPiksela(_wybranaWarstwaWmts.Identyfikator) = CDbl(nudRozmiarPiksela.Value)
        End If
    End Sub

    Private Sub btnZaznaczWszystkie_Click(sender As Object, e As EventArgs) Handles btnZaznaczWszystkie.Click
        UstawZaznaczenieWszystkich(tvWarstwy.Nodes, True)
        AktualizujLiczbeZaznaczonych()
    End Sub

    Private Sub btnOdznaczWszystkie_Click(sender As Object, e As EventArgs) Handles btnOdznaczWszystkie.Click
        UstawZaznaczenieWszystkich(tvWarstwy.Nodes, False)
        AktualizujLiczbeZaznaczonych()
    End Sub

    Private Sub UstawZaznaczenieWszystkich(nodes As TreeNodeCollection, stan As Boolean)
        _blokadaCheck = True
        Try
            For Each n As TreeNode In nodes
                n.Checked = stan
                UstawZaznaczenieWszystkich(n.Nodes, stan)
            Next
        Finally
            _blokadaCheck = False
        End Try
    End Sub

    ''' <summary>Pobiera próbkę graficzną kafelka (256x256 px) dla wybranej warstwy z mechanizmem automatycznego dopasowania skali.</summary>
    Private Async Sub btnPobierzProbke_Click(sender As Object, e As EventArgs) Handles btnPobierzProbke.Click
        If _wynikCap Is Nothing Then Exit Sub

        btnPobierzProbke.Enabled = False
        _przerwanieProbki = New CancellationTokenSource()
        Dim token = _przerwanieProbki.Token
        'pobieranie próbki trwa nawet kilkadziesiąt sekund - widoczny stan zamiast samego wyszarzonego przycisku
        Dim tekstPrzycisku As String = btnPobierzProbke.Text
        btnPobierzProbke.Text = "Pobieranie próbki..."
        Using staryPodglad = picPodglad.Image
            picPodglad.Image = Nothing
        End Using
        Cursor = Cursors.WaitCursor
        Try
            Dim sukcessPobrania As Boolean = False
            Dim znalezionyPx As Double = 0
            Dim ostatnieBajty As Byte() = Nothing
            Dim ostatniStatus As Integer = 200

            Using handler As New HttpClientHandler()
                If chkIgnorujSsl.Checked Then
                    handler.ServerCertificateCustomValidationCallback = Function(m, c, ch, err) True
                End If
                Using klient As New HttpClient(handler) With {.Timeout = TimeSpan.FromSeconds(LimitCzasuProbkiSekundy)}
                    klient.DefaultRequestHeaders.UserAgent.ParseAdd(Program.UserAgentProgramu)

                    If _wybranaWarstwaWms IsNot Nothing Then
                        Dim format As String = If(_wynikCap.UslugaWms.Formaty.Contains("image/png"), "image/png", "image/jpeg")
                        Dim px As Double = If(_rozmiaryPiksela.ContainsKey(_wybranaWarstwaWms.Nazwa),
                                              _rozmiaryPiksela(_wybranaWarstwaWms.Nazwa),
                                              _wybranaWarstwaWms.ZalecanyRozmiarPiksela())
                        Dim adresWms As String = PlikWarstw.ZbudujAdresWmsDlaPliku(_wynikCap.UslugaWms.AdresGetMap, _wynikCap.UslugaWms.Wersja)

                        'określenie środka obszaru próbkowania (z BoundingBox warstwy lub centrum Polski)
                        Dim srodekX As Double = 500000.0
                        Dim srodekY As Double = 500000.0
                        If _wybranaWarstwaWms.Zasieg2180.HasValue Then
                            Dim s = _wybranaWarstwaWms.Zasieg2180.Value.Srodek
                            If s.X >= 100000.0 AndAlso s.X <= 900000.0 AndAlso s.Y >= 100000.0 AndAlso s.Y <= 900000.0 Then
                                srodekX = s.X
                                srodekY = s.Y
                            End If
                        End If

                        'kandydaci nazw warstw (nazwa z XML, ID z LegendURL, a w razie błędu LayerNotDefined serwerów ArcGIS - indeksy numeryczne 0 i 1)
                        Dim kandydujaceNazwy As New List(Of String)
                        If Not String.IsNullOrEmpty(_wybranaWarstwaWms.Nazwa) Then
                            kandydujaceNazwy.Add(_wybranaWarstwaWms.Nazwa)
                        End If
                        If Not String.IsNullOrEmpty(_wybranaWarstwaWms.IdentyfikatorNumeryczny) AndAlso
                           Not kandydujaceNazwy.Contains(_wybranaWarstwaWms.IdentyfikatorNumeryczny) Then
                            kandydujaceNazwy.Add(_wybranaWarstwaWms.IdentyfikatorNumeryczny)
                        End If
                        For Each altId In {"0", "1"}
                            If Not kandydujaceNazwy.Contains(altId) Then kandydujaceNazwy.Add(altId)
                        Next

                        'próby skali: najpierw wybrany piksel, a w razie potrzeby skale przeglądowe
                        Dim kandydujacePx As New List(Of Double) From {px}
                        If px < 1.0 Then kandydujacePx.Add(1.0)
                        If px < 2.0 AndAlso Not kandydujacePx.Contains(2.0) Then kandydujacePx.Add(2.0)

                        Dim znalezionaNazwa As String = ""

                        For Each kandNazwa In kandydujaceNazwy
                            For Each testPx In kandydujacePx
                                Dim bok As Double = Math.Max(50.0, 256.0 * testPx)
                                Dim srodekPL As New Zasieg(srodekX, srodekY, srodekX + bok, srodekY + bok)
                                Dim url = Wms.ZapytanieGetMap(adresWms, {kandNazwa},
                                                              UkladWspolrzednych.PL1992, srodekPL, format, 256, 256,
                                                              zamienOsie:=False, przezroczystosc:=True)

                                Using odp = Await klient.GetAsync(url, token).ConfigureAwait(True)
                                    ostatniStatus = CInt(odp.StatusCode)
                                    Dim bajty = Await odp.Content.ReadAsByteArrayAsync(token).ConfigureAwait(True)
                                    If bajty IsNot Nothing AndAlso bajty.Length > 0 AndAlso PobieranieSegmentow.CzyObraz(bajty) Then
                                        Using ms As New MemoryStream(bajty)
                                            Using staryImg = picPodglad.Image
                                                picPodglad.Image = New Bitmap(Image.FromStream(ms))
                                            End Using
                                        End Using
                                        sukcessPobrania = True
                                        znalezionyPx = testPx
                                        znalezionaNazwa = kandNazwa
                                        Exit For
                                    Else
                                        ostatnieBajty = bajty
                                    End If
                                End Using
                            Next
                            If sukcessPobrania Then Exit For
                        Next

                        If sukcessPobrania Then
                            If Not String.IsNullOrEmpty(znalezionaNazwa) AndAlso znalezionaNazwa <> _wybranaWarstwaWms.Nazwa Then
                                Dim staraNazwa As String = _wybranaWarstwaWms.Nazwa
                                _wybranaWarstwaWms.Nazwa = znalezionaNazwa
                                lblNazwaWartosc.Text = znalezionaNazwa & " (" & staraNazwa & ")"
                                MessageBox.Show("Serwer odrzucił nazwę '" & staraNazwa & "' (błąd konfiguracji serwera LayerNotDefined), ale zaakceptował identyfikator '" & znalezionaNazwa & "'." & vbCrLf &
                                                "Identyfikator warstwy został zaktualizowany i z nim zostanie zapisany do pliku zbioru.",
                                                "Dopasowano identyfikator warstwy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            End If
                        Else
                            Dim podgladBledu As String = ""
                            If ostatnieBajty IsNot Nothing AndAlso ostatnieBajty.Length > 0 Then
                                podgladBledu = vbCrLf & "Odpowiedź serwera: " &
                                    System.Text.Encoding.UTF8.GetString(ostatnieBajty, 0, Math.Min(ostatnieBajty.Length, 250)).Replace(vbCr, " ").Replace(vbLf, " ").Trim()
                            Else
                                podgladBledu = vbCrLf & "Serwer zwrócił pustą odpowiedź (0 bajtów). Możliwe, że dane nie pokrywają centralnej Polski lub wybranej skali."
                            End If
                            MessageBox.Show("Serwer nie zwrócił poprawnego obrazka dla testowego zapytania (HTTP " & ostatniStatus & ")." & podgladBledu,
                                            "Podgląd próbki WMS", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If

                    ElseIf _wybranaWarstwaWmts IsNot Nothing AndAlso _wynikCap.UslugaWmts IsNot Nothing Then
                        OdswiezOpcjeKafliZPol()
                        Dim px As Double = If(_rozmiaryPiksela.ContainsKey(_wybranaWarstwaWmts.Identyfikator),
                                              _rozmiaryPiksela(_wybranaWarstwaWmts.Identyfikator),
                                              WyznaczSugerowanyRozmiarPikselaWmts(_wybranaWarstwaWmts))

                        'kolejka prób: najpierw wybrany piksel, a jeśli kafelek jest pusty (0 bajtów) -
                        'automatyczna próba poziomów przeglądowych (np. dla BDOO, NMT itp.)
                        Dim kandydujacePx As New List(Of Double) From {px}
                        For Each altPx In {25.0, 10.0, 5.0, 2.0, 1.0, 0.5, 0.25, 50.0}
                            If Math.Abs(altPx - px) > 0.05 AndAlso Not kandydujacePx.Contains(altPx) Then
                                kandydujacePx.Add(altPx)
                            End If
                        Next
                        Dim srodekPlProbka As PunktXY = If(_opcjeKafli IsNot Nothing AndAlso _srodekMapy.HasValue,
                                                           UkladWspolrzednych.PL1992.ZWgs84(_srodekMapy.Value),
                                                           New PunktXY(500000, 500000))

                        For Each testPx In kandydujacePx
                            Dim bok As Double = Math.Max(50.0, 256.0 * testPx)
                            Dim srodekPL As New Zasieg(srodekPlProbka.X, srodekPlProbka.Y, srodekPlProbka.X + bok, srodekPlProbka.Y + bok)
                            Try
                                Dim plan As New PlanWmts(_wynikCap.UslugaWmts, _wybranaWarstwaWmts.Identyfikator, UkladWspolrzednych.PL1992, srodekPL, testPx, "png")
                                Dim kol As Double, wie As Double
                                plan.PikselMacierzy(plan.DoUkladuKafli(New PunktXY(srodekPL.XGora, srodekPL.YLewy)), kol, wie)
                                Dim url = plan.AdresKafla(CInt(Math.Floor(wie / plan.Macierz.WysokoscKafla)), CInt(Math.Floor(kol / plan.Macierz.SzerokoscKafla)))

                                Using odp = Await klient.GetAsync(url, token).ConfigureAwait(True)
                                    ostatniStatus = CInt(odp.StatusCode)
                                    Dim bajty = Await odp.Content.ReadAsByteArrayAsync(token).ConfigureAwait(True)
                                    Dim ct As String = If(odp.Content.Headers.ContentType IsNot Nothing, If(odp.Content.Headers.ContentType.MediaType, ""), "")
                                    If odp.IsSuccessStatusCode AndAlso bajty IsNot Nothing AndAlso bajty.Length > 0 AndAlso
                                       KafleXyz.DiagnozujOdpowiedzKafla(bajty, ct, adres:=url) = "" Then
                                        If _opcjeKafli IsNot Nothing Then
                                            Dim wykrytyMime = KafleXyz.WykryjMimeZObrazu(bajty, ct)
                                            If wykrytyMime <> "" Then
                                                _opcjeKafli.FormatObrazu = wykrytyMime
                                                If _wybranaWarstwaWmts.Formaty.Count = 0 Then
                                                    _wybranaWarstwaWmts.Formaty.Add(wykrytyMime)
                                                Else
                                                    _wybranaWarstwaWmts.Formaty(0) = wykrytyMime
                                                End If
                                            End If
                                        End If
                                        Using ms As New MemoryStream(bajty)
                                            Using staryImg = picPodglad.Image
                                                picPodglad.Image = New Bitmap(Image.FromStream(ms))
                                            End Using
                                        End Using
                                        sukcessPobrania = True
                                        znalezionyPx = testPx
                                        Exit For
                                    Else
                                        ostatnieBajty = bajty
                                    End If
                                End Using
                            Catch
                                'błąd generowania planu dla danego poziomu - próba kolejnego
                            End Try
                        Next

                        If sukcessPobrania Then
                            If Math.Abs(znalezionyPx - px) > 0.05 Then
                                _rozmiaryPiksela(_wybranaWarstwaWmts.Identyfikator) = znalezionyPx
                                UstawPolePiksela(znalezionyPx)
                                MessageBox.Show("Pobrano próbkę dla rozdzielczości " & znalezionyPx.ToString("0.##") & " m/pix." & vbCrLf &
                                                "Serwer nie udostępnia kafelków dla wybranej dużej szczegółowości (" & px.ToString("0.##") & " m/pix) – automatycznie zaktualizowano zalecany rozmiar piksela.",
                                                "Podgląd próbki WMTS", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            End If
                        Else
                            Dim podgladBledu As String = ""
                            If ostatnieBajty IsNot Nothing AndAlso ostatnieBajty.Length > 0 Then
                                podgladBledu = vbCrLf & "Odpowiedź serwera: " &
                                    System.Text.Encoding.UTF8.GetString(ostatnieBajty, 0, Math.Min(ostatnieBajty.Length, 250)).Replace(vbCr, " ").Replace(vbLf, " ").Trim()
                            Else
                                podgladBledu = vbCrLf & "Serwer zwrócił pustą odpowiedź (0 bajtów). Kafelki w tym obszarze mogą nie być wygenerowane w pamięci podręcznej serwera."
                            End If
                            MessageBox.Show("Serwer nie zwrócił poprawnego kafelka dla testowego zapytania (HTTP " & ostatniStatus & ")." & podgladBledu,
                                            "Podgląd próbki WMTS", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                    Else
                        MessageBox.Show("Wybierz warstwę, aby pobrać próbkę.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            End Using

        Catch ex As OperationCanceledException When token.IsCancellationRequested
            'okno zamknięte w trakcie pobierania próbki
        Catch ex As TaskCanceledException
            If Not IsDisposed Then
                MessageBox.Show("Serwer nie odpowiedział w ciągu " & LimitCzasuProbkiSekundy & " s.", "Błąd podglądu", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            If Not IsDisposed Then
                MessageBox.Show("Nie udało się pobrać próbki kafelka: " & ex.Message, "Błąd podglądu", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Finally
            _przerwanieProbki?.Dispose()
            _przerwanieProbki = Nothing
            If Not IsDisposed Then
                btnPobierzProbke.Text = tekstPrzycisku
                btnPobierzProbke.Enabled = True
                Cursor = Cursors.Default
            End If
        End Try
    End Sub

    ''' <summary>Limit czasu jednego zapytania o próbkę [s].</summary>
    Private Const LimitCzasuProbkiSekundy As Integer = 30

    Private Sub FormDodajSerwer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _przerwaniePolaczenia?.Cancel()
        _przerwanieProbki?.Cancel()
        pnlKatalog.AnulujAktywneZapytanie()
    End Sub

    Private Sub txtNazwaZbioru_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtNazwaZbioru.KeyPress
        _nazwaZbioruZmienionaRecznie = True
        _nazwaZbioruZKatalogu = False
    End Sub

    Private Sub btnZapisz_Click(sender As Object, e As EventArgs) Handles btnZapisz.Click
        Dim nazwaZbioru As String = txtNazwaZbioru.Text.Trim()
        Dim komunikatBledu As String = ""
        If Not PlikWarstw.CzyNazwaZbioruPoprawna(nazwaZbioru, komunikatBledu) Then
            MessageBox.Show(komunikatBledu, "Niepoprawna nazwa zbioru", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNazwaZbioru.Focus()
            Exit Sub
        End If

        Dim zaznaczone = PobierzZaznaczoneWarstwyZDrzewa()
        If zaznaczone.Count = 0 Then
            MessageBox.Show("Zaznacz co najmniej jedną warstwę do pobrania.", "Brak wybranych warstw", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim folderZapisu As String = FolderZapisuWarstw()
        Dim plikZbioru As String = Path.Combine(folderZapisu, nazwaZbioru & ".txt")
        Dim plikIndeksu As String = Path.Combine(folderZapisu, "warstwy.txt")

        If File.Exists(plikZbioru) Then
            Dim odp = MessageBox.Show("Zbiór map o nazwie '" & nazwaZbioru & "' już istnieje." & vbCrLf &
                                      "Czy chcesz go zastąpić?", "Zbiór map istnieje",
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If odp <> DialogResult.Yes Then Exit Sub
        End If

        Try
            Dim czyZKatalogu = Not String.IsNullOrWhiteSpace(_wybranyKatalogowyTytul) AndAlso
                               String.Equals(txtAdres.Text.Trim(), _wybranyKatalogowyAdres, StringComparison.OrdinalIgnoreCase)
            Dim czytelnyTytul = If(czyZKatalogu, _wybranyKatalogowyTytul, If(_wynikCap?.Tytul, "").Trim())
            Dim p As New PlikWarstw With {.IgnorujSsl = chkIgnorujSsl.Checked, .Tytul = czytelnyTytul}

            If _wynikCap.Typ = TypUslugiMapowej.Wms Then
                If p.IgnorujSsl Then p.Typ = TypUslugiMapowej.Wms
                p.Adres = PlikWarstw.ZbudujAdresWmsDlaPliku(_wynikCap.UslugaWms.AdresGetMap, _wynikCap.UslugaWms.Wersja)
                For Each item In zaznaczone
                    If TypeOf item Is WarstwaWmsInfo Then
                        Dim w = DirectCast(item, WarstwaWmsInfo)
                        Dim px As Double = w.ZalecanyRozmiarPiksela()
                        If _rozmiaryPiksela.ContainsKey(w.Nazwa) Then px = _rozmiaryPiksela(w.Nazwa)
                        Dim tytulW = If(w.Tytul, "").Trim()
                        p.Warstwy.Add(New WarstwaMapy With {
                            .Nazwa = w.Nazwa,
                            .Tytul = If(tytulW <> "" AndAlso Not String.Equals(tytulW, w.Nazwa, StringComparison.Ordinal), tytulW, ""),
                            .RozmiarPiksela = px.ToString("0.##", CultureInfo.InvariantCulture)
                        })
                    End If
                Next
            ElseIf KafleXyz.CzyKafelkowy(_wynikCap.Typ) Then
                Dim czyXyzTms As Boolean = (_wynikCap.Typ = TypUslugiMapowej.Xyz OrElse _wynikCap.Typ = TypUslugiMapowej.Tms)
                If Not czyXyzTms AndAlso p.IgnorujSsl Then p.Typ = TypUslugiMapowej.Wmts
                p.Adres = If(czyXyzTms AndAlso Not String.IsNullOrEmpty(_wynikCap.SzablonKafli), _wynikCap.SzablonKafli, _wynikCap.AdresCapabilities)
                If czyXyzTms Then
                    OdswiezOpcjeKafliZPol()
                    If _opcjeKafli IsNot Nothing Then
                        p.Typ = _opcjeKafli.Typ
                        p.ZMin = _opcjeKafli.ZMin
                        p.ZMax = _opcjeKafli.ZMax
                        p.RozmiarKafla = _opcjeKafli.RozmiarKafla
                        p.Nakladka = _opcjeKafli.Nakladka
                        p.OdstepMs = _opcjeKafli.OdstepMs
                        p.MaksWatkow = _opcjeKafli.MaksWatkow
                        p.Referer = _opcjeKafli.Referer
                        p.Atrybucja = _opcjeKafli.Atrybucja
                        p.FormatObrazu = _opcjeKafli.FormatObrazu
                        p.ZasiegGeo = _opcjeKafli.ZasiegGeo
                        p.Subdomeny.AddRange(_opcjeKafli.Subdomeny)
                    End If
                End If
                For Each item In zaznaczone
                    If TypeOf item Is WarstwaWmts Then
                        Dim w = DirectCast(item, WarstwaWmts)
                        Dim px As Double = 1.0
                        If _rozmiaryPiksela.ContainsKey(w.Identyfikator) Then px = _rozmiaryPiksela(w.Identyfikator)
                        Dim tytulW = If(w.Tytul, "").Trim()
                        If tytulW = "" AndAlso czyXyzTms AndAlso zaznaczone.Count = 1 AndAlso czytelnyTytul <> "" Then
                            tytulW = czytelnyTytul
                        End If
                        p.Warstwy.Add(New WarstwaMapy With {
                            .Nazwa = w.Identyfikator,
                            .Tytul = If(tytulW <> "" AndAlso Not String.Equals(tytulW, w.Identyfikator, StringComparison.Ordinal), tytulW, ""),
                            .RozmiarPiksela = px.ToString("0.##", CultureInfo.InvariantCulture)
                        })
                    End If
                Next
            End If

            p.Zapisz(plikZbioru)
            PlikWarstw.DodajDoIndeksu(plikIndeksu, nazwaZbioru)

            WynikNazwaZbioru = nazwaZbioru
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Błąd podczas zapisywania zbioru map: " & ex.Message, "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function PobierzZaznaczoneWarstwyZDrzewa() As List(Of Object)
        Dim wynik As New List(Of Object)
        ZbierzZaznaczone(tvWarstwy.Nodes, wynik)
        Return wynik
    End Function

    Private Sub ZbierzZaznaczone(nodes As TreeNodeCollection, cel As List(Of Object))
        For Each n As TreeNode In nodes
            If n.Checked AndAlso n.Tag IsNot Nothing Then
                If TypeOf n.Tag Is WarstwaWmsInfo Then
                    Dim w = DirectCast(n.Tag, WarstwaWmsInfo)
                    If w.CzyWarstwaDoPobrania Then cel.Add(w)
                ElseIf TypeOf n.Tag Is WarstwaWmts Then
                    cel.Add(n.Tag)
                End If
            End If
            ZbierzZaznaczone(n.Nodes, cel)
        Next
    End Sub

End Class
