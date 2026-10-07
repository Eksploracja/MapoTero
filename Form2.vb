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

''' <summary>Okno ustawień programu i sesji pobierania.</summary>
Public Class Form2

    ''' <summary>Blokuje reakcję na zdarzenia kontrolek podczas ich wypełniania.</summary>
    Private _wypelnianie As Boolean

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Location = New Point(200, 150)
        WypelnijKontrolki()
    End Sub

    ''' <summary>Przenosi ustawienia do kontrolek okna.</summary>
    Private Sub WypelnijKontrolki()
        _wypelnianie = True
        Try
            Dim u = Ustawienia
            txtIloscProb.Text = u.IloscProbPobrania.ToString()
            txtPrzerwa.Text = u.PrzerwaMiedzyProbami.ToString()
            nudWatki.Value = Math.Max(nudWatki.Minimum, Math.Min(nudWatki.Maximum, CDec(u.LiczbaWatkow)))
            chkMap.Checked = u.PlikMap
            chkGmi.Checked = u.PlikGmi
            chkWldPoints.Checked = u.PlikWldPoints
            chkZamienXY.Checked = u.ZamienXY
            chkPowyzejOstatniego.Checked = u.PobierajPowyzejOstatniego
            chkTrekBuddy.Checked = u.TrekBuddy
            chkWorldFile.Checked = u.PlikWorldFile
            chkKml.Checked = u.PlikKml
            chkEdycjaXY.Checked = u.EdycjaXY
            chkKursorWgs.Checked = u.KursorWgs84
            chkZaznaczenieWgs.Checked = u.ZaznaczenieWgs84
            chkKursorISrodek.Checked = u.KursorISrodekMapy
            chkTab.Checked = u.PlikTab
            txtPrefiks.Text = u.Prefiks
            cmbNumeracja.Text = u.Numeracja
            cmbFormat.Text = u.Format
            UstawTrybTrekBuddy(u.TrekBuddy, False)
        Finally
            _wypelnianie = False
        End Try
        OdswiezNazweTrekBuddy()
    End Sub

    Private Sub cmbFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFormat.SelectedIndexChanged
        If _wypelnianie Then Exit Sub
        Ustawienia.Format = cmbFormat.Text
        Form1.stFormat.Text = "." & Ustawienia.Format
    End Sub

    Private Sub cmbNumeracja_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbNumeracja.SelectedIndexChanged
        If _wypelnianie Then Exit Sub
        Ustawienia.Numeracja = cmbNumeracja.Text
    End Sub

#Region "Pliki georeferencyjne i opcje pobierania"

    Private Sub chkMap_Click(sender As Object, e As EventArgs) Handles chkMap.Click
        Ustawienia.PlikMap = chkMap.Checked
    End Sub

    Private Sub chkGmi_Click(sender As Object, e As EventArgs) Handles chkGmi.Click
        Ustawienia.PlikGmi = chkGmi.Checked
    End Sub

    Private Sub chkWldPoints_Click(sender As Object, e As EventArgs) Handles chkWldPoints.Click
        Ustawienia.PlikWldPoints = chkWldPoints.Checked
    End Sub

    Private Sub chkWorldFile_Click(sender As Object, e As EventArgs) Handles chkWorldFile.Click
        Ustawienia.PlikWorldFile = chkWorldFile.Checked
    End Sub

    Private Sub chkKml_Click(sender As Object, e As EventArgs) Handles chkKml.Click
        Ustawienia.PlikKml = chkKml.Checked
    End Sub

    Private Sub chkTab_Click(sender As Object, e As EventArgs) Handles chkTab.Click
        Ustawienia.PlikTab = chkTab.Checked
    End Sub

    Private Sub chkZamienXY_Click(sender As Object, e As EventArgs) Handles chkZamienXY.Click
        Ustawienia.ZamienXY = chkZamienXY.Checked
    End Sub

    Private Sub chkPowyzejOstatniego_Click(sender As Object, e As EventArgs) Handles chkPowyzejOstatniego.Click
        Ustawienia.PobierajPowyzejOstatniego = chkPowyzejOstatniego.Checked
    End Sub

#End Region

#Region "TrekBuddy i Locus Map"

    Private Sub chkTrekBuddy_CheckedChanged(sender As Object, e As EventArgs) Handles chkTrekBuddy.CheckedChanged
        If _wypelnianie Then Exit Sub
        UstawTrybTrekBuddy(chkTrekBuddy.Checked, True)
    End Sub

    ''' <summary>
    ''' Tryb mapy TrekBuddy / Locus Map: wyłącza pliki kalibracyjne segmentów i numerację (segmenty nazywane
    ''' wg położenia w pikselach), ustawia segment 512 px.
    ''' </summary>
    Private Sub UstawTrybTrekBuddy(wlaczony As Boolean, zmienBokSegmentu As Boolean)
        Ustawienia.TrekBuddy = wlaczony
        If wlaczony Then
            chkMap.Checked = False : Ustawienia.PlikMap = False
            chkGmi.Checked = False : Ustawienia.PlikGmi = False
            chkZamienXY.Checked = False : Ustawienia.ZamienXY = False
            chkEdycjaXY.Checked = False
            chkPowyzejOstatniego.Checked = False : Ustawienia.PobierajPowyzejOstatniego = False
            Ustawienia.PlikWldPoints = False
            Ustawienia.PlikWorldFile = False
            If zmienBokSegmentu Then Form1.BokSegmentu = MapaTrekBuddy.BokSegmentu.ToString()
        ElseIf zmienBokSegmentu Then
            Form1.BokSegmentu = "2000"
        End If
        chkPowyzejOstatniego.Enabled = Not wlaczony
        grpKalibracja.Enabled = Not wlaczony
        cmbNumeracja.Enabled = Not wlaczony
        cmbNazwaTB.Enabled = wlaczony
        lblNazwaTB.Enabled = wlaczony
        lblPodgladNazwyTB.Visible = wlaczony
        lblPodgladTB.Visible = wlaczony
    End Sub

    Private Sub cmbNazwaTB_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbNazwaTB.SelectedIndexChanged
        OdswiezNazweTrekBuddy()
    End Sub

    Private Sub txtPrefiks_TextChanged(sender As Object, e As EventArgs) Handles txtPrefiks.TextChanged
        OdswiezNazweTrekBuddy()
    End Sub

    ''' <summary>Nazwa paczki TrekBuddy / Locus Map wg wybranego stylu (przedrostek + zbiór map i/lub warstwa).</summary>
    Public Sub OdswiezNazweTrekBuddy()
        Dim warstwa As String = Form1.PierwszaWarstwa
        Select Case cmbNazwaTB.SelectedIndex
            Case 1 : Ustawienia.NazwaTrekBuddy = Form1.ZbiorMap
            Case 2 : Ustawienia.NazwaTrekBuddy = warstwa
            Case 3 : Ustawienia.NazwaTrekBuddy = Form1.ZbiorMap & "_" & warstwa
            Case Else : Ustawienia.NazwaTrekBuddy = ""
        End Select
        lblPodgladNazwyTB.Text = txtPrefiks.Text & Ustawienia.NazwaTrekBuddy
    End Sub

#End Region

#Region "Widok okna głównego"

    Private Sub chkEdycjaXY_CheckedChanged(sender As Object, e As EventArgs) Handles chkEdycjaXY.CheckedChanged
        Ustawienia.EdycjaXY = chkEdycjaXY.Checked
        Form1.UstawEdycjeXY(Ustawienia.EdycjaXY)
        If _wypelnianie Then Exit Sub
        If Ustawienia.EdycjaXY Then
            Form1.Komunikat("Wyłączono zaznaczanie na mapie zasięgu pobierania prawym przyciskiem myszy. Określ samodzielnie współrzędne XY zasięgu mapy i wpisz je w odpowiednie pola", Color.Green)
        Else
            Form1.Komunikat("", Color.Black)
        End If
    End Sub

    Private Sub chkKursorWgs_CheckedChanged(sender As Object, e As EventArgs) Handles chkKursorWgs.CheckedChanged
        Ustawienia.KursorWgs84 = chkKursorWgs.Checked
    End Sub

    Private Sub chkZaznaczenieWgs_CheckedChanged(sender As Object, e As EventArgs) Handles chkZaznaczenieWgs.CheckedChanged
        Ustawienia.ZaznaczenieWgs84 = chkZaznaczenieWgs.Checked
        Form1.UstawWidokZaznaczeniaWgs(Ustawienia.ZaznaczenieWgs84)
    End Sub

    Private Sub chkKursorISrodek_CheckedChanged(sender As Object, e As EventArgs) Handles chkKursorISrodek.CheckedChanged
        Ustawienia.KursorISrodekMapy = chkKursorISrodek.Checked
        Form1.UstawWidokWspolrzednych(Ustawienia.KursorISrodekMapy)
    End Sub

#End Region

#Region "Zapis i reset"

    Private Sub btnZapisz_Click(sender As Object, e As EventArgs) Handles btnZapisz.Click
        Ustawienia.Prefiks = txtPrefiks.Text
        Ustawienia.IloscProbPobrania = Math.Max(1, WartoscCalkowita(txtIloscProb.Text, 3))
        Ustawienia.PrzerwaMiedzyProbami = Math.Max(0, WartoscCalkowita(txtPrzerwa.Text, 5))
        Ustawienia.LiczbaWatkow = CInt(nudWatki.Value)
        Try
            Ustawienia.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
        Me.Close()
    End Sub

    'przywracanie domyślnych ustawień
    Private Sub btnResetuj_Click(sender As Object, e As EventArgs) Handles btnResetuj.Click
        Ustawienia.PrzywrocDomyslne()

        'czyszczenie znaczników i domyślny widok mapy
        Form1.DomyslnyWidokMapy()
        Form1.UstawEdycjeXY(False)
        Form1.UstawWidokZaznaczeniaWgs(False)
        Form1.UstawWidokWspolrzednych(True)
        Form1.UstawZbiorMap("skany_map_topograficznych")
        Form1.BokSegmentu = "2000"
        Form1.RozmiarPiksela = "2"
        Form1.stFormat.Text = "." & Ustawienia.Format
        Form1.OdswiezOpisUkladu()

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
