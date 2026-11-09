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

Imports MapoTero.Core

''' <summary>
''' Wybór obszaru pobierania wzdłuż trasy: plik GPX / KML / KMZ i szerokość pasa. Okno pokazuje, ile segmentów
''' zostanie pobranych w porównaniu z prostokątem obejmującym trasę (przy bieżącym rozmiarze piksela i segmentu).
''' </summary>
Public Class FormKorytarz

    Private _korytarz As Korytarz

    ''' <summary>Wybrany korytarz (po kliknięciu OK).</summary>
    Public ReadOnly Property Wynik As Korytarz

    ''' <param name="biezacy">Korytarz bieżącej sesji (zmiana szerokości pasa bez ponownego wskazywania pliku) albo Nothing.</param>
    Public Sub New(biezacy As Korytarz)
        InitializeComponent()
        If biezacy IsNot Nothing Then
            'kopia - Anuluj nie może zmienić korytarza sesji
            _korytarz = Korytarz.ZTekstuZapisu(biezacy.DoTekstu())
            txtPlik.Text = biezacy.Nazwa
            nudSzerokosc.Value = Math.Max(nudSzerokosc.Minimum, Math.Min(nudSzerokosc.Maximum, CDec(biezacy.SzerokoscMetry)))
        End If
        OdswiezOpis()
    End Sub

    Private Sub btnPlik_Click(sender As Object, e As EventArgs) Handles btnPlik.Click
        If dlgPlik.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Try
            Dim k = Korytarz.WczytajTrase(dlgPlik.FileName)
            If Not k.Poprawny Then
                _korytarz = Nothing
                txtPlik.Text = dlgPlik.FileName
                Opis("Plik nie zawiera śladu, trasy ani linii (GPX: trk, rte, wpt; KML: LineString, gx:Track).", Color.Red)
                btnOK.Enabled = False
                Exit Sub
            End If
            _korytarz = k
            txtPlik.Text = dlgPlik.FileName
        Catch ex As Exception
            _korytarz = Nothing
            txtPlik.Text = dlgPlik.FileName
            Opis("Nie udało się odczytać trasy: " & ex.Message, Color.Red)
            btnOK.Enabled = False
            Exit Sub
        End Try
        OdswiezOpis()
    End Sub

    Private Sub nudSzerokosc_ValueChanged(sender As Object, e As EventArgs) Handles nudSzerokosc.ValueChanged
        OdswiezOpis()
    End Sub

    Private Sub Opis(tekst As String, kolor As Color)
        lblOpis.ForeColor = kolor
        lblOpis.Text = tekst
    End Sub

    ''' <summary>Długość trasy i liczba segmentów korytarza.</summary>
    Private Sub OdswiezOpis()
        If _korytarz Is Nothing Then
            btnOK.Enabled = False
            Exit Sub
        End If
        _korytarz.SzerokoscMetry = CDbl(nudSzerokosc.Value)
        Dim uklad = Ustawienia.Uklad
        Dim siatka = Form1.SiatkaDlaZasiegu(_korytarz.Zasieg(uklad))
        Dim tekst As String = "Trasa: " & If(_korytarz.Nazwa <> "", _korytarz.Nazwa & ", ", "") & _korytarz.DlugoscKm.ToString("0.0") & " km, " &
                              _korytarz.LiczbaPunktow & " punktów."
        Dim szacunek As Double = _korytarz.SzacowanaLiczbaSegmentow(siatka, uklad)
        If Not siatka.Poprawna Then
            Opis(tekst & vbCrLf & "Wpisz w oknie głównym rozmiar piksela i segmentu, aby zobaczyć liczbę segmentów.", Color.Black)
        ElseIf szacunek > Form1.MaksSegmentowKorytarzaNaBiezaco Then
            'bez wyznaczania milionów segmentów przy każdej zmianie szerokości pasa
            Opis(tekst & vbCrLf & "Pobranych zostanie ok. " & Math.Round(szacunek).ToString("N0") & " segmentów (szacunek). " &
                 "Liczba dotyczy bieżącego rozmiaru piksela (" & Form1.RozmiarPiksela & ") i segmentu (" & Form1.BokSegmentu & " px).", Color.Black)
        Else
            Dim n As Integer = _korytarz.Segmenty(siatka, uklad).Count
            Dim wszystkie As Long = siatka.LiczbaSegmentow
            Opis(tekst & vbCrLf & "Pobranych zostanie " & n & " segmentów zamiast " & wszystkie & " w prostokącie obejmującym trasę" &
                 If(wszystkie > 0, " (" & Math.Round(100.0 * (wszystkie - n) / wszystkie) & "% mniej danych).", ".") & vbCrLf &
                 "Liczba dotyczy bieżącego rozmiaru piksela (" & Form1.RozmiarPiksela & ") i segmentu (" & Form1.BokSegmentu & " px).", Color.Black)
        End If
        btnOK.Enabled = True
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If _korytarz Is Nothing Then Exit Sub
        _korytarz.SzerokoscMetry = CDbl(nudSzerokosc.Value)
        _Wynik = _korytarz
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

End Class
