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
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Eksport pobranych segmentów do formatów map dla odbiorników GPS i aplikacji mobilnych:
''' KMZ (Garmin Custom Maps, Locus Map, OruxMaps) i MBTiles (Locus Map, OsmAnd, OruxMaps).
''' </summary>
Public Class FormEksport

    Private _folder As String = ""
    Private _konfiguracja As KonfiguracjaSesji
    Private _siatka As Siatka
    Private _uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Private _przerwanie As CancellationTokenSource

    Private Sub FormEksport_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _folder = Form1.FolderSesji
        If _folder = "" OrElse File.Exists(_folder & "conf.txt") = False Then _folder = FolderDownload
        WczytajSesje()
        UstawWidok()
    End Sub

    Private Sub Komunikat(tekst As String, kolor As Color)
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
    End Sub

    ''' <summary>Parametry segmentów z conf.txt.</summary>
    Private Sub WczytajSesje()
        txtFolder.Text = _folder
        _konfiguracja = Nothing
        _siatka = Nothing
        lblInfo.Text = ""
        Dim plik As String = _folder & "conf.txt"
        If File.Exists(plik) = False Then
            Komunikat("W wybranym folderze nie ma pliku conf.txt z parametrami segmentów. Wskaż folder z pobranymi segmentami.", Color.Red)
            Exit Sub
        End If
        Try
            _konfiguracja = KonfiguracjaSesji.Wczytaj(plik, KodowanieSystemowe())
            _uklad = UkladWspolrzednych.ZKodu(_konfiguracja.UkladEpsg)
            _siatka = New Siatka(New Zasieg(Wartosc(_konfiguracja.XDol), Wartosc(_konfiguracja.YLewy), Wartosc(_konfiguracja.XGora), Wartosc(_konfiguracja.YPrawy)),
                                 Wartosc(_konfiguracja.RozmiarPiksela), WartoscCalkowita(_konfiguracja.BokSegmentuPx))
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku conf.txt: " & ex.Message, Color.Red)
            Exit Sub
        End Try
        If Not _siatka.Poprawna Then
            Komunikat("Plik conf.txt zawiera niepoprawne parametry segmentów.", Color.Red)
            _siatka = Nothing
            Exit Sub
        End If

        lblInfo.Text = "Segmenty: " & _siatka.LiczbaKolumn & " x " & _siatka.LiczbaWierszy & " po " & _siatka.BokSegmentuPx & " px, piksel " &
                       _konfiguracja.RozmiarPiksela & If(_uklad.Geograficzny, "°", " m") & ", układ " & _uklad.Nazwa & vbCrLf &
                       "Mapa: " & _konfiguracja.RodzajMapy & " " & _konfiguracja.Warstwy(0)

        'poziomy powiększenia MBTiles: najwyższy - pełna szczegółowość segmentów
        Dim z = UtworzZadanie()
        Dim srodek = _uklad.DoWgs84(_siatka.ZasiegSiatki.Srodek)
        Dim zmax As Integer = WebMercator.PoziomDlaRozdzielczosci(EksportMapy.PikselWMetrach(z), srodek.Szerokosc)
        nudPoziomMax.Value = Math.Min(nudPoziomMax.Maximum, zmax)
        nudPoziomMin.Value = Math.Max(nudPoziomMin.Minimum, Math.Min(zmax, Math.Max(8, zmax - 6)))
        If File.Exists(_folder & "error.txt") Then
            Komunikat("W folderze jest plik error.txt - zestaw segmentów jest niekompletny (brakujące fragmenty pozostaną puste).", Color.Red)
        Else
            Komunikat("Segmenty gotowe do eksportu.", Color.Black)
        End If
    End Sub

    Private Function UtworzZadanie() As ZadanieEksportu
        Dim c = _konfiguracja
        Dim zrodlo As New ZrodloSegmentow(_folder, c.Prefiks, Siatka.StylZTekstu(If(c.Numeracja = "", Ustawienia.Numeracja, c.Numeracja)),
                                         Wms.RozszerzeniePliku(c.Format), _siatka)
        Dim rodzaj As RodzajEksportu = If(rbMBTiles.Checked, RodzajEksportu.MBTiles, If(rbKmz.Checked, RodzajEksportu.Kmz, RodzajEksportu.KmzGarmin))
        Dim nazwa As String = "_eksport_" & _siatka.LiczbaKolumn & "x" & _siatka.LiczbaWierszy
        Dim rozszerzenie As String = If(rodzaj = RodzajEksportu.MBTiles, ".mbtiles", If(rodzaj = RodzajEksportu.KmzGarmin, "_garmin.kmz", ".kmz"))
        Return New ZadanieEksportu With {
            .Rodzaj = rodzaj, .Zrodlo = zrodlo, .Uklad = _uklad, .Plik = _folder & nazwa & rozszerzenie,
            .Nazwa = (c.RodzajMapy & " " & c.Warstwy(0)).Trim(), .JakoscJpeg = CInt(nudJakosc.Value), .MaksKafli = CInt(nudMaksKafli.Value),
            .PoziomMin = CInt(Math.Min(nudPoziomMin.Value, nudPoziomMax.Value)), .PoziomMax = CInt(nudPoziomMax.Value), .KaflePng = chkPng.Checked}
    End Function

    ''' <summary>Dostępność opcji i podsumowanie planu eksportu.</summary>
    Private Sub UstawWidok()
        nudMaksKafli.Enabled = rbKmzGarmin.Checked
        grpMBTiles.Enabled = rbMBTiles.Checked
        btnEksportuj.Enabled = _siatka IsNot Nothing AndAlso _przerwanie Is Nothing
        lblPlan.Text = ""
        If _siatka Is Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        If z.Rodzaj = RodzajEksportu.MBTiles Then
            lblPlan.Text = "Liczba kafli: " & EksportMapy.LiczbaKafliMBTiles(z).ToString("N0") & " (poziomy " & z.PoziomMin & "-" & z.PoziomMax & ")"
        Else
            Dim plan = EksportMapy.PlanKmz(z)
            lblPlan.Text = "Kafle: " & plan.Kolumny & " x " & plan.Wiersze & " = " & plan.LiczbaKafli &
                           If(plan.Pomniejszenie > 1.001, ", rozdzielczość zmniejszona " & plan.Pomniejszenie.ToString("0.0") & "x (limit kafli)", ", pełna rozdzielczość")
        End If
    End Sub

    Private Sub OpcjeZmienione(sender As Object, e As EventArgs) Handles rbKmzGarmin.CheckedChanged, rbKmz.CheckedChanged, rbMBTiles.CheckedChanged,
                                                                    nudMaksKafli.ValueChanged, nudPoziomMin.ValueChanged, nudPoziomMax.ValueChanged
        UstawWidok()
    End Sub

    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        dlgFolder.SelectedPath = _folder
        If dlgFolder.ShowDialog = DialogResult.OK Then
            _folder = dlgFolder.SelectedPath & "\"
            WczytajSesje()
            UstawWidok()
        End If
    End Sub

    Private Async Sub btnEksportuj_Click(sender As Object, e As EventArgs) Handles btnEksportuj.Click
        If _siatka Is Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        If z.Rodzaj = RodzajEksportu.MBTiles AndAlso EksportMapy.LiczbaKafliMBTiles(z) > 2000000 Then
            If MessageBox.Show("Eksport obejmie ponad 2 mln kafli i może trwać bardzo długo. Kontynuować?", "Eksport MBTiles",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        End If

        _przerwanie = New CancellationTokenSource()
        btnEksportuj.Enabled = False
        btnPrzerwij.Enabled = True
        grpFormat.Enabled = False
        grpMBTiles.Enabled = False
        prgEksport.Value = 0
        Komunikat("Trwa eksport do pliku " & Path.GetFileName(z.Plik) & "...", Color.Black)

        Dim postep As New Progress(Of Integer)(Sub(p) prgEksport.Value = Math.Max(0, Math.Min(100, p)))
        Dim token = _przerwanie.Token
        Try
            Await Task.Run(Sub() EksportMapy.Eksportuj(z, postep, token))
            prgEksport.Value = 100
            Komunikat("Zapisano plik " & z.Plik, Color.Green)
            Form1.Komunikat("Wyeksportowano mapę do pliku " & Path.GetFileName(z.Plik), Color.Green)
        Catch ex As OperationCanceledException
            Komunikat("Przerwano eksport", Color.Red)
        Catch ex As Exception
            Komunikat("Błąd eksportu: " & ex.Message, Color.Red)
        Finally
            _przerwanie.Dispose()
            _przerwanie = Nothing
            btnPrzerwij.Enabled = False
            grpFormat.Enabled = True
            UstawWidok()
        End Try
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        btnPrzerwij.Enabled = False
        _przerwanie?.Cancel()
    End Sub

    Private Sub FormEksport_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _przerwanie?.Cancel()
    End Sub

End Class
