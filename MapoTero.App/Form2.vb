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

''' <summary>
''' Okno ustawień programu i sesji pobierania. Zmiany w oknie są stosowane dopiero po kliknięciu OK
''' (Anuluj lub zamknięcie okna - ustawienia bez zmian). Okno jest wypełniane przy każdym otwarciu,
''' bo zamknięte okno modalne nie jest niszczone, a ustawienia mogą się zmienić w innym miejscu (np. wczytanie conf.txt).
''' </summary>
Public Class Form2

    ''' <summary>Blokuje reakcję na zdarzenia kontrolek podczas ich wypełniania.</summary>
    Private _wypelnianie As Boolean

    ''' <summary>
    ''' Ustawiane przez okno główne przed otwarciem okna: trwa pobieranie segmentów (część ustawień wyłączona).
    ''' Przekazywane z zewnątrz, a nie odczytywane z Form1, bo testy tworzą to okno bez okna głównego.
    ''' </summary>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TrwaPobieranie As Boolean

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.Ustawienia, 202, 387)
    End Sub

    Private Sub Form2_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Me.Visible Then WypelnijKontrolki()
    End Sub

    ''' <summary>Listy wyboru (wypełniane raz).</summary>
    Private Sub PrzygotujListy()
        If cmbFormat.Items.Count > 0 Then Exit Sub
        cmbFormat.Items.AddRange(UstawieniaProgramu.FormatySegmentow)
        cmbNumeracja.Items.AddRange(UstawieniaProgramu.StyleNumeracji)
        For Each u In UkladWspolrzednych.Wszystkie
            cmbUkladDomyslny.Items.Add(u)
        Next
    End Sub

    ''' <summary>Przenosi ustawienia do kontrolek okna.</summary>
    Private Sub WypelnijKontrolki()
        PrzygotujListy()
        _wypelnianie = True
        Try
            Dim u = Ustawienia
            'sesja
            cmbFormat.SelectedItem = UstawieniaProgramu.NormalizujFormat(u.Format)
            chkPrzezroczystosc.Checked = u.Przezroczystosc
            txtPrefiks.Text = u.Prefiks
            cmbNumeracja.SelectedItem = u.Numeracja
            If cmbNumeracja.SelectedIndex < 0 Then cmbNumeracja.SelectedIndex = 0
            cmbUkladDomyslny.SelectedItem = u.UkladDomyslny
            'widok okna głównego
            chkKursorISrodek.Checked = u.KursorISrodekMapy
            chkKursorWgs.Checked = u.KursorWgs84
            chkZaznaczenieWgs.Checked = u.ZaznaczenieWgs84
            chkSiatkaSegmentow.Checked = u.SiatkaSegmentowNaMapie
            'pliki kalibracji
            chkMap.Checked = u.PlikMap
            chkKml.Checked = u.PlikKml
            chkWorldFile.Checked = u.PlikWorldFile
            chkTab.Checked = u.PlikTab
            chkWldPoints.Checked = u.PlikWldPoints
            chkGmi.Checked = u.PlikGmi
            'pobieranie
            Ustaw(nudIloscProb, u.IloscProbPobrania)
            Ustaw(nudPrzerwa, u.PrzerwaMiedzyProbami)
            Ustaw(nudWatki, u.LiczbaWatkow)
            Ustaw(nudLimitCzasu, u.LimitCzasuSekundy)
            chkZachowajKafle.Checked = u.ZachowajKafleWmts
            Ustaw(nudJakoscWmts, u.JakoscJpegWmts)
        Finally
            _wypelnianie = False
        End Try
        OdswiezDostepnosc()
        UstawBlokadePobierania()
    End Sub

    ''' <summary>
    ''' W trakcie pobierania można zmienić tylko widok współrzędnych i układ nowej sesji. Pozostałe ustawienia zmieniają
    ''' parametry trwającej sesji (format, przedrostek, numeracja, conf.txt) albo siatkę na mapie,
    ''' dlatego są wyłączone; dawniej okno w ogóle nie otwierało się w trakcie pobierania.
    ''' </summary>
    Private Sub UstawBlokadePobierania()
        Dim pobieranie As Boolean = TrwaPobieranie
        For Each c As Control In New Control() {grpSesja, grpKalibracja, grpPobieranie, grpWmts,
                                                chkSiatkaSegmentow, btnPrzywroc}
            c.Enabled = Not pobieranie
        Next
        Me.Text = If(pobieranie, "Ustawienia (trwa pobieranie - część ustawień można zmienić po jego zakończeniu)", "Ustawienia")
    End Sub

    ''' <summary>Wartość pola liczbowego ograniczona do jego zakresu.</summary>
    Private Shared Sub Ustaw(pole As NumericUpDown, wartosc As Integer)
        pole.Value = Math.Max(pole.Minimum, Math.Min(pole.Maximum, CDec(wartosc)))
    End Sub

    ''' <summary>Przezroczyste tło tylko dla formatów, które je zapisują (PNG, GIF).</summary>
    Private Sub OdswiezDostepnosc()
        chkPrzezroczystosc.Enabled = Wms.CzyFormatZPrzezroczystoscia(CStr(cmbFormat.SelectedItem))
    End Sub

    Private Sub cmbFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFormat.SelectedIndexChanged
        If _wypelnianie Then Exit Sub
        OdswiezDostepnosc()
    End Sub

#Region "TrekBuddy (tylko wznowienie sesji z dawnych wersji)"

    ''' <summary>
    ''' Mapę TrekBuddy / Locus Map tworzy od wersji 3.19 okno Eksport mapy z dowolnej sesji, więc okno Ustawienia nie ma
    ''' już trybu TrekBuddy. Tryb zostaje tylko dla wznawianej sesji z dawnej wersji (pole w conf.txt) - jej segmenty
    ''' mają nazwy i folder paczki TrekBuddy, a nazwa paczki powstaje wg zapisanego stylu (lastsettings.txt).
    ''' </summary>
    Private Shared Function DopisekTrekBuddy(styl As Integer) As String
        Dim s As String = ""
        Select Case styl
            Case 1 : s = Form1.ZbiorMap
            Case 2 : s = Form1.PierwszaWarstwa
            Case 3 : s = Form1.ZbiorMap & "_" & Form1.PierwszaWarstwa
            Case Else : s = ""
        End Select
        Return MapaTrekBuddy.BezpiecznaNazwa(s)
    End Function

    ''' <summary>Nazwa paczki TrekBuddy wg zapisanego stylu - wywoływane z okna głównego po zmianie warstw.</summary>
    Public Sub OdswiezNazweTrekBuddy()
        Ustawienia.NazwaTrekBuddy = DopisekTrekBuddy(Ustawienia.StylNazwyTrekBuddy)
    End Sub

#End Region

#Region "OK, Anuluj, przywracanie domyślnych"

    ''' <summary>Stosuje wszystkie ustawienia z okna i zapisuje je w lastsettings.txt.</summary>
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Dim u = Ustawienia

        'przedrostek jest początkiem nazwy pliku segmentu - niedozwolone znaki kierowały zapis poza folder sesji
        Dim bladPrzedrostka As String = KonfiguracjaSesji.BladPrzedrostka(txtPrefiks.Text)
        If bladPrzedrostka <> "" Then
            MsgBox(bladPrzedrostka, MsgBoxStyle.Exclamation, "Ustawienia")
            txtPrefiks.Focus()
            txtPrefiks.SelectAll()
            Exit Sub
        End If

        'sesja (zapisywana w conf.txt przy rozpoczęciu pobierania)
        u.Format = UstawieniaProgramu.NormalizujFormat(CStr(cmbFormat.SelectedItem))
        u.Przezroczystosc = chkPrzezroczystosc.Checked
        Form1.stFormat.Text = "." & u.Format
        u.Prefiks = txtPrefiks.Text
        If cmbNumeracja.SelectedItem IsNot Nothing Then u.Numeracja = CStr(cmbNumeracja.SelectedItem)
        Dim uklad = TryCast(cmbUkladDomyslny.SelectedItem, UkladWspolrzednych)
        If uklad IsNot Nothing Then u.UkladDomyslny = uklad

        'widok okna głównego
        u.KursorISrodekMapy = chkKursorISrodek.Checked
        u.KursorWgs84 = chkKursorWgs.Checked
        u.ZaznaczenieWgs84 = chkZaznaczenieWgs.Checked
        Dim bylaSiatka As Boolean = u.SiatkaSegmentowNaMapie
        u.SiatkaSegmentowNaMapie = chkSiatkaSegmentow.Checked
        If bylaSiatka <> u.SiatkaSegmentowNaMapie AndAlso Form1.SiatkaWidoczna Then
            Form1.OdswiezSiatke()
        End If
        Form1.UstawWidokWspolrzednych(u.KursorISrodekMapy)
        Form1.UstawWidokZaznaczeniaWgs(u.ZaznaczenieWgs84)

        'pliki kalibracji segmentów
        u.PlikMap = chkMap.Checked
        u.PlikKml = chkKml.Checked
        u.PlikWorldFile = chkWorldFile.Checked
        u.PlikTab = chkTab.Checked
        u.PlikWldPoints = chkWldPoints.Checked
        u.PlikGmi = chkGmi.Checked

        OdswiezNazweTrekBuddy()

        'pobieranie
        u.IloscProbPobrania = CInt(nudIloscProb.Value)
        u.PrzerwaMiedzyProbami = CInt(nudPrzerwa.Value)
        u.LiczbaWatkow = CInt(nudWatki.Value)
        u.LimitCzasuSekundy = CInt(nudLimitCzasu.Value)
        u.ZachowajKafleWmts = chkZachowajKafle.Checked
        u.JakoscJpegWmts = CInt(nudJakoscWmts.Value)


        Try
            u.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Ustawienia")
        End Try
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    ''' <summary>
    ''' Przywraca ustawienia domyślne (po potwierdzeniu) - od razu, łącznie z widokiem mapy, zbiorem map, bokiem segmentu,
    ''' rozmiarem piksela i układem współrzędnych w oknie głównym.
    ''' </summary>
    Private Sub btnPrzywroc_Click(sender As Object, e As EventArgs) Handles btnPrzywroc.Click
        If MessageBox.Show("Przywrócić domyślne ustawienia programu?" & vbCrLf & vbCrLf &
                           "Zmiana obejmie także widok mapy, zbiór map, bok segmentu, rozmiar piksela i układ współrzędnych " &
                           "w oknie głównym i nie można jej cofnąć przyciskiem Anuluj.",
                           "Przywracanie ustawień domyślnych", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                           MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Exit Sub

        Ustawienia.PrzywrocDomyslne()

        'układ PL-1992 - z przeliczeniem wpisanego zasięgu; potem domyślne parametry segmentów
        Form1.PrzelaczUklad(UkladWspolrzednych.PL1992, False)
        Form1.DomyslnyWidokMapy()
        Form1.UstawEdycjeXY(False)
        Form1.UstawWidokZaznaczeniaWgs(False)
        Form1.UstawWidokWspolrzednych(True)
        Form1.UstawZbiorMap("skany_map_topograficznych")
        Form1.BokSegmentu = "2000"
        Form1.RozmiarPiksela = "2"
        Form1.stFormat.Text = "." & Ustawienia.Format
        Form1.OdswiezOpisUkladu()
        OdswiezNazweTrekBuddy()

        WypelnijKontrolki()

        Try
            If File.Exists(PlikLastsettings) Then File.Delete(PlikLastsettings)
            Ustawienia.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Ustawienia")
        End Try
        Form1.Komunikat("Przywrócono ustawienia domyślne programu (lastsettings.txt)", Color.Green)
    End Sub

#End Region

End Class
