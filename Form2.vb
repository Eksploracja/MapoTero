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

    ''' <summary>Opisy stylów nazwy paczki TrekBuddy (indeks = UstawieniaProgramu.StylNazwyTrekBuddy).</summary>
    Private Shared ReadOnly StyleNazwyTB As String() = {"sam przedrostek", "nazwa zbioru map", "nazwa warstwy", "zbiór map + warstwa"}

    ''' <summary>Blokuje reakcję na zdarzenia kontrolek podczas ich wypełniania.</summary>
    Private _wypelnianie As Boolean

    Private Sub Form2_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Me.Visible Then WypelnijKontrolki()
    End Sub

    ''' <summary>Listy wyboru (wypełniane raz).</summary>
    Private Sub PrzygotujListy()
        If cmbFormat.Items.Count > 0 Then Exit Sub
        cmbFormat.Items.AddRange(UstawieniaProgramu.FormatySegmentow)
        cmbNumeracja.Items.AddRange(UstawieniaProgramu.StyleNumeracji)
        cmbNazwaTB.Items.AddRange(StyleNazwyTB)
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
            txtPrefiks.Text = u.Prefiks
            cmbNumeracja.SelectedItem = u.Numeracja
            If cmbNumeracja.SelectedIndex < 0 Then cmbNumeracja.SelectedIndex = 0
            chkPowyzejOstatniego.Checked = u.PobierajPowyzejOstatniego
            cmbUkladDomyslny.SelectedItem = u.UkladDomyslny
            'widok okna głównego
            chkKursorISrodek.Checked = u.KursorISrodekMapy
            chkKursorWgs.Checked = u.KursorWgs84
            chkZaznaczenieWgs.Checked = u.ZaznaczenieWgs84
            chkEdycjaXY.Checked = u.EdycjaXY
            'pliki kalibracji
            chkMap.Checked = u.PlikMap
            chkKml.Checked = u.PlikKml
            chkWorldFile.Checked = u.PlikWorldFile
            chkTab.Checked = u.PlikTab
            chkWldPoints.Checked = u.PlikWldPoints
            chkGmi.Checked = u.PlikGmi
            'TrekBuddy
            chkTrekBuddy.Checked = u.TrekBuddy
            cmbNazwaTB.SelectedIndex = Math.Max(0, Math.Min(StyleNazwyTB.Length - 1, u.StylNazwyTrekBuddy))
            'pobieranie
            Ustaw(nudIloscProb, u.IloscProbPobrania)
            Ustaw(nudPrzerwa, u.PrzerwaMiedzyProbami)
            Ustaw(nudWatki, u.LiczbaWatkow)
            Ustaw(nudLimitCzasu, u.LimitCzasuSekundy)
            chkZachowajKafle.Checked = u.ZachowajKafleWmts
            Ustaw(nudJakoscWmts, u.JakoscJpegWmts)
            chkZamienXY.Checked = u.ZamienXY
        Finally
            _wypelnianie = False
        End Try
        OdswiezDostepnosc()
        OdswiezPodgladTB()
    End Sub

    ''' <summary>Wartość pola liczbowego ograniczona do jego zakresu.</summary>
    Private Shared Sub Ustaw(pole As NumericUpDown, wartosc As Integer)
        pole.Value = Math.Max(pole.Minimum, Math.Min(pole.Maximum, CDec(wartosc)))
    End Sub

#Region "TrekBuddy i Locus Map"

    ''' <summary>
    ''' W trybie TrekBuddy / Locus Map segmenty nazywane są wg położenia w pikselach, a kalibracja zapisywana jest w pliku .map
    ''' paczki - numeracja, pliki kalibracji segmentów i pobieranie powyżej ostatniego segmentu nie są wtedy używane.
    ''' Wartości tych ustawień pozostają bez zmian i wracają do użytku po wyłączeniu trybu.
    ''' </summary>
    Private Sub OdswiezDostepnosc()
        Dim tb As Boolean = chkTrekBuddy.Checked
        grpKalibracja.Enabled = Not tb
        cmbNumeracja.Enabled = Not tb
        lblNumeracja.Enabled = Not tb
        chkPowyzejOstatniego.Enabled = Not tb
        lblNazwaTB.Enabled = tb
        cmbNazwaTB.Enabled = tb
        lblPodgladTB.Enabled = tb
        lblPodgladNazwyTB.Enabled = tb
    End Sub

    Private Sub chkTrekBuddy_CheckedChanged(sender As Object, e As EventArgs) Handles chkTrekBuddy.CheckedChanged
        If _wypelnianie Then Exit Sub
        OdswiezDostepnosc()
    End Sub

    Private Sub cmbNazwaTB_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbNazwaTB.SelectedIndexChanged
        If _wypelnianie Then Exit Sub
        OdswiezPodgladTB()
    End Sub

    Private Sub txtPrefiks_TextChanged(sender As Object, e As EventArgs) Handles txtPrefiks.TextChanged
        If _wypelnianie Then Exit Sub
        OdswiezPodgladTB()
    End Sub

    ''' <summary>Podgląd nazwy paczki dla wartości wpisanych w oknie.</summary>
    Private Sub OdswiezPodgladTB()
        lblPodgladNazwyTB.Text = txtPrefiks.Text & DopisekTrekBuddy(cmbNazwaTB.SelectedIndex)
    End Sub

    ''' <summary>Dopisek do przedrostka w nazwie paczki TrekBuddy / Locus Map wg stylu.</summary>
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

    ''' <summary>
    ''' Nazwa paczki TrekBuddy / Locus Map wg zapisanego stylu - wywoływane także z okna głównego po zmianie warstw.
    ''' </summary>
    Public Sub OdswiezNazweTrekBuddy()
        Ustawienia.NazwaTrekBuddy = DopisekTrekBuddy(Ustawienia.StylNazwyTrekBuddy)
        If Me.Visible Then OdswiezPodgladTB()
    End Sub

#End Region

#Region "OK, Anuluj, przywracanie domyślnych"

    ''' <summary>Stosuje wszystkie ustawienia z okna i zapisuje je w lastsettings.txt.</summary>
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Dim u = Ustawienia

        'sesja (zapisywana w conf.txt przy rozpoczęciu pobierania)
        u.Format = UstawieniaProgramu.NormalizujFormat(CStr(cmbFormat.SelectedItem))
        Form1.stFormat.Text = "." & u.Format
        u.Prefiks = txtPrefiks.Text
        If cmbNumeracja.SelectedItem IsNot Nothing Then u.Numeracja = CStr(cmbNumeracja.SelectedItem)
        u.PobierajPowyzejOstatniego = chkPowyzejOstatniego.Checked
        Dim uklad = TryCast(cmbUkladDomyslny.SelectedItem, UkladWspolrzednych)
        If uklad IsNot Nothing Then u.UkladDomyslny = uklad

        'widok okna głównego
        Dim bylaEdycjaXY As Boolean = u.EdycjaXY
        u.KursorISrodekMapy = chkKursorISrodek.Checked
        u.KursorWgs84 = chkKursorWgs.Checked
        u.ZaznaczenieWgs84 = chkZaznaczenieWgs.Checked
        u.EdycjaXY = chkEdycjaXY.Checked
        Form1.UstawWidokWspolrzednych(u.KursorISrodekMapy)
        Form1.UstawWidokZaznaczeniaWgs(u.ZaznaczenieWgs84)
        Form1.UstawEdycjeXY(u.EdycjaXY)
        If u.EdycjaXY AndAlso Not bylaEdycjaXY Then
            Form1.Komunikat("Wyłączono zaznaczanie na mapie zasięgu pobierania prawym przyciskiem myszy. Określ samodzielnie współrzędne XY zasięgu mapy i wpisz je w odpowiednie pola", Color.Green)
        ElseIf bylaEdycjaXY AndAlso Not u.EdycjaXY Then
            Form1.Komunikat("", Color.Black)
        End If

        'pliki kalibracji segmentów
        u.PlikMap = chkMap.Checked
        u.PlikKml = chkKml.Checked
        u.PlikWorldFile = chkWorldFile.Checked
        u.PlikTab = chkTab.Checked
        u.PlikWldPoints = chkWldPoints.Checked
        u.PlikGmi = chkGmi.Checked

        'TrekBuddy: segment 512 px, a po wyłączeniu - bok segmentu sprzed włączenia trybu
        Dim bylTrekBuddy As Boolean = u.TrekBuddy
        u.TrekBuddy = chkTrekBuddy.Checked
        If u.TrekBuddy AndAlso Not bylTrekBuddy Then
            u.BokSegmentuPrzedTrekBuddy = Form1.BokSegmentu
            Form1.BokSegmentu = MapaTrekBuddy.BokSegmentu.ToString()
        ElseIf bylTrekBuddy AndAlso Not u.TrekBuddy Then
            Form1.BokSegmentu = If(u.BokSegmentuPrzedTrekBuddy <> "", u.BokSegmentuPrzedTrekBuddy, "2000")
            u.BokSegmentuPrzedTrekBuddy = ""
        End If
        u.StylNazwyTrekBuddy = Math.Max(0, cmbNazwaTB.SelectedIndex)
        OdswiezNazweTrekBuddy()

        'pobieranie
        u.IloscProbPobrania = CInt(nudIloscProb.Value)
        u.PrzerwaMiedzyProbami = CInt(nudPrzerwa.Value)
        u.LiczbaWatkow = CInt(nudWatki.Value)
        u.LimitCzasuSekundy = CInt(nudLimitCzasu.Value)
        u.ZachowajKafleWmts = chkZachowajKafle.Checked
        u.JakoscJpegWmts = CInt(nudJakoscWmts.Value)
        u.ZamienXY = chkZamienXY.Checked

        Try
            u.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & ex.Message, MsgBoxStyle.Exclamation)
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
            MsgBox("Nie udało się zapisać ustawień: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
        Form1.Komunikat("Przywrócono ustawienia domyślne programu (lastsettings.txt)", Color.Green)
    End Sub

#End Region

End Class
