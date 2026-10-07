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
Imports System.Threading
Imports MapoTero.Core

''' <summary>Okno scalania pobranych segmentów w jeden arkusz.</summary>
Public Class Form3

    Private _folderScalania As String = ""
    Private _uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _folderScalania = Form1.FolderSesji
        If _folderScalania = "" OrElse File.Exists(_folderScalania & "conf.txt") = False Then _folderScalania = FolderDownload
        txtFolder.Text = _folderScalania
        txtFolder.Enabled = False
        grpParametry.Enabled = False
        chkWorldFile.Checked = Ustawienia.ScalanieWorldFile
        chkKml.Checked = Ustawienia.ScalanieKml
        chkMap.Checked = Ustawienia.ScalanieMap
        chkTab.Checked = Ustawienia.ScalanieTab
        cmbFormatArkusza.SelectedIndex = CInt(Ustawienia.FormatArkusza)
        trkJakosc.Enabled = Ustawienia.FormatArkusza = FormatArkusza.Jpeg OrElse Ustawienia.FormatArkusza = FormatArkusza.GeoTiffJpeg
        WczytajParametry()
    End Sub

    ''' <summary>Parametry zbioru segmentów z pliku conf.txt w folderze scalania.</summary>
    Private Sub WczytajParametry()
        Dim plikConf As String = _folderScalania & "conf.txt"
        If File.Exists(plikConf) Then
            Try
                Dim c = KonfiguracjaSesji.Wczytaj(plikConf, KodowanieSystemowe())
                txtXDol.Text = c.XDol
                txtYLewy.Text = c.YLewy
                txtXGora.Text = c.XGora
                txtYPrawy.Text = c.YPrawy
                txtBokSegmentu.Text = c.BokSegmentuPx
                txtRozmiarPiksela.Text = c.RozmiarPiksela
                txtPrefiks.Text = If(c.Prefiks = "", "\", c.Prefiks)
                cmbFormat.Text = Wms.RozszerzeniePliku(c.Format)
                cmbNumeracja.Text = If(c.Numeracja = "", Ustawienia.Numeracja, c.Numeracja)
                _uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
            Catch ex As Exception
                Komunikat("Nie udało się odczytać pliku conf.txt: " & ex.Message, Color.Red)
            End Try
        End If
        PrzeliczLiczbeSegmentow()

        If File.Exists(_folderScalania & "error.txt") Then
            Komunikat("W katalogu segmentów wykryto obecność pliku error.txt co świadczy o niekompletnym zestawie segmentów. Uzupełnij je i usuń plik error.txt", Color.Red)
        ElseIf File.Exists(plikConf) = False Then
            chkRecznie.Enabled = True
            Komunikat("We wskazanym katalogu segmentów nie odnaleziono pliku konfiguracyjnego conf.txt. Jesli został on bezpowrotnie utracony, istnieje możliwość samodzielnego zdefiniowania parametrów segmentów. W tym celu zaznacz opcję 'ręczne wprowadzanie parametrów'", Color.Red)
        Else
            chkRecznie.Enabled = False
            Komunikat("Segmenty gotowe do złączenia. Wszelkie parametry scalania zostały załadowane automatycznie z pliku conf.txt", Color.Black)
        End If
    End Sub

    Private Sub Komunikat(tekst As String, kolor As Color)
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
    End Sub

    ''' <summary>Siatka segmentów wg pól okna.</summary>
    Private Function BiezacaSiatka() As Siatka
        Return New Siatka(New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text)),
                          Wartosc(txtRozmiarPiksela.Text), WartoscCalkowita(txtBokSegmentu.Text))
    End Function

    Private Sub PrzeliczLiczbeSegmentow()
        Dim s = BiezacaSiatka()
        txtLiczbaKolumn.Text = If(s.Poprawna, s.LiczbaKolumn.ToString(), "")
        txtLiczbaWierszy.Text = If(s.Poprawna, s.LiczbaWierszy.ToString(), "")
    End Sub

    Private Sub ParametryZmienione(sender As Object, e As EventArgs) Handles txtXDol.TextChanged, txtYLewy.TextChanged, txtXGora.TextChanged,
                                                                         txtYPrawy.TextChanged, txtBokSegmentu.TextChanged, txtRozmiarPiksela.TextChanged
        PrzeliczLiczbeSegmentow()
    End Sub

    Private Sub btnZmienFolder_Click(sender As Object, e As EventArgs) Handles btnZmienFolder.Click
        dlgFolder.SelectedPath = FolderDownload
        If dlgFolder.ShowDialog = DialogResult.OK Then
            _folderScalania = dlgFolder.SelectedPath & "\"
            txtFolder.Text = _folderScalania
            txtFolder.Enabled = True
            If File.Exists(_folderScalania & "conf.txt") = False Then
                MsgBox("Brak pliku konfiguracji 'conf.txt' w podanej lokalizacji.")
            End If
            WczytajParametry()
        End If
    End Sub

    ''' <summary>Format arkusza wybrany na liście (kolejność pozycji jak w wyliczeniu FormatArkusza).</summary>
    Private ReadOnly Property WybranyFormat As FormatArkusza
        Get
            Return CType(Math.Max(0, cmbFormatArkusza.SelectedIndex), FormatArkusza)
        End Get
    End Property

    Private Sub cmbFormatArkusza_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFormatArkusza.SelectedIndexChanged
        'jakość kompresji dotyczy formatów z kompresją JPEG
        trkJakosc.Enabled = WybranyFormat = FormatArkusza.Jpeg OrElse WybranyFormat = FormatArkusza.GeoTiffJpeg
        Ustawienia.FormatArkusza = WybranyFormat
    End Sub

    Private Async Sub btnScal_Click(sender As Object, e As EventArgs) Handles btnScal.Click
        If File.Exists(_folderScalania & "error.txt") Then
            Komunikat("W katalogu segmentów wykryto obecność pliku error.txt co świadczy o niekompletnym zestawie segmentów. Uzupełnij je i usuń plik error.txt", Color.Red)
            Exit Sub
        End If

        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then
            Komunikat("Niepoprawne parametry segmentów - uzupełnij je ręcznie (opcja 'ręczne wprowadzanie parametrów').", Color.Red)
            Exit Sub
        End If

        Dim zadanie As New ZadanieScalania With {
            .Folder = _folderScalania, .Prefiks = If(txtPrefiks.Text = "\", "", txtPrefiks.Text),
            .Numeracja = Siatka.StylZTekstu(cmbNumeracja.Text), .RozszerzenieSegmentow = Wms.RozszerzeniePliku(cmbFormat.Text),
            .Siatka = s, .Uklad = _uklad, .Format = WybranyFormat, .JakoscJpeg = trkJakosc.Value,
            .NazwaArkusza = "_scalone_segmenty_" & s.LiczbaKolumn & "x" & s.LiczbaWierszy,
            .Georeferencja = New OpcjeGeoreferencji With {.WorldFile = chkWorldFile.Checked, .Kml = chkKml.Checked, .Map = chkMap.Checked, .Tab = chkTab.Checked}}

        btnScal.Enabled = False
        Me.UseWaitCursor = True
        prgScalanie.Value = 0
        Komunikat("Trwa scalanie segmentów (" & s.SzerokoscPx & " x " & s.WysokoscPx & " pikseli)...", Color.Black)

        Dim postep As New Progress(Of Integer)(Sub(p) prgScalanie.Value = Math.Max(0, Math.Min(100, p)))
        Dim wynik As WynikScalania
        Try
            wynik = Await Task.Run(Function() ScalanieSegmentow.Scal(zadanie, postep, CancellationToken.None))
        Catch ex As Exception
            Komunikat("Błąd. Segmenty nie zostały scalone: " & ex.Message, Color.Red)
            Exit Sub
        Finally
            btnScal.Enabled = True
            Me.UseWaitCursor = False
        End Try

        Dim nazwa As String = Path.GetFileName(wynik.Plik)
        If wynik.Brakujace.Count > 0 Then
            Komunikat("Arkusz " & nazwa & " zapisano, ale brakowało " & wynik.Brakujace.Count & " segmentów (białe pola), m.in.: " &
                      String.Join(", ", wynik.Brakujace.Take(5)), Color.Red)
            Form1.Komunikat("Scalono segmenty do pliku " & nazwa & " - brakowało " & wynik.Brakujace.Count & " segmentów", Color.Red)
        Else
            Form1.Komunikat("Segmenty zostały prawidłowo scalone i zapisane do pliku " & nazwa, Color.Green)
            Me.Close()
        End If
    End Sub

    Private Sub chkRecznie_CheckedChanged(sender As Object, e As EventArgs) Handles chkRecznie.CheckedChanged
        grpParametry.Enabled = chkRecznie.Checked
    End Sub

    Private Sub trkJakosc_Scroll(sender As Object, e As EventArgs) Handles trkJakosc.Scroll
        lblJakosc.Text = trkJakosc.Value & "%"
    End Sub

    Private Sub Form3_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Ustawienia.ScalanieWorldFile = chkWorldFile.Checked
        Ustawienia.ScalanieKml = chkKml.Checked
        Ustawienia.ScalanieMap = chkMap.Checked
        Ustawienia.ScalanieTab = chkTab.Checked
    End Sub

End Class
