'Copyright (C) <2026>  pajakt
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
Imports System.Windows.Forms
Imports MapoTero.Core

Public Class FormWyslijNaTelefon

    Private ReadOnly _sciezkaPliku As String
    Private _serwer As SerwerWysylkiWifi

    Public Sub New(sciezkaPliku As String)
        InitializeComponent()
        _sciezkaPliku = sciezkaPliku
    End Sub

    ''' <summary>
    ''' Otwiera modalne okno transferu Wi-Fi z kodem QR dla wskazanego pliku mapy.
    ''' </summary>
    Public Shared Sub PokazDlaPliku(wlasciciel As IWin32Window, sciezkaPliku As String)
        If String.IsNullOrWhiteSpace(sciezkaPliku) OrElse Not File.Exists(sciezkaPliku) Then
            MessageBox.Show("Wskazany plik mapy nie istnieje.", "Błąd pliku", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using frm As New FormWyslijNaTelefon(sciezkaPliku)
            frm.ShowDialog(wlasciciel)
        End Using
    End Sub

    Private Sub FormWyslijNaTelefon_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim fi As New FileInfo(_sciezkaPliku)
            lblPlikInfo.Text = $"Plik: {fi.Name} ({SzacunekRozmiaru.Tekst(fi.Length)})"

            _serwer = New SerwerWysylkiWifi(_sciezkaPliku)
            AddHandler _serwer.ZmianaStatusu, AddressOf OnZmianaStatusu
            _serwer.Uruchom(0)

            Dim listaIp = SerwerWysylkiWifi.PobierzWszystkieAdresyLan()
            Dim domyslneIp = If(listaIp.Count > 0, listaIp(0), SerwerWysylkiWifi.WykryjDomyslneIpLan())

            cmbInterfejsy.Items.Clear()
            For Each ip In listaIp
                cmbInterfejsy.Items.Add(ip)
            Next

            If Not cmbInterfejsy.Items.Contains(domyslneIp) Then
                cmbInterfejsy.Items.Insert(0, domyslneIp)
            End If

            If cmbInterfejsy.Items.Count > 0 Then
                Dim idx = cmbInterfejsy.Items.IndexOf(domyslneIp)
                cmbInterfejsy.SelectedIndex = If(idx >= 0, idx, 0)
            Else
                cmbInterfejsy.Items.Add("127.0.0.1")
                cmbInterfejsy.SelectedIndex = 0
            End If

            If picQr.Image Is Nothing Then OdswiezKodQr()
            PokazStanGotowy()
        Catch ex As Exception
            MessageBox.Show("Nie udało się uruchomić serwera transferu Wi-Fi: " & OpisBledu.Z(ex),
                            "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Close()
        End Try
    End Sub

    Private Sub OdswiezKodQr()
        If _serwer Is Nothing OrElse cmbInterfejsy.SelectedItem Is Nothing Then Return

        Dim ip = cmbInterfejsy.SelectedItem.ToString()
        Dim urlStrony = _serwer.PobierzAdresStrony(ip)
        txtAdres.Text = urlStrony

        Try
            Dim staryObraz = picQr.Image
            ' Generujemy ostrą bitmapę dopasowaną do PictureBox
            picQr.Image = GeneratorQr.GenerujBitmapeDopasowana(urlStrony, picQr.Width - 10, marginesModuly:=4)
            staryObraz?.Dispose()
        Catch ex As Exception
            lblStatus.Text = "Błąd generowania kodu QR: " & OpisBledu.Z(ex)
        End Try
    End Sub

    Private Sub cmbInterfejsy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbInterfejsy.SelectedIndexChanged
        OdswiezKodQr()
    End Sub

    Private Sub btnKopiuj_Click(sender As Object, e As EventArgs) Handles btnKopiuj.Click
        If Not String.IsNullOrEmpty(txtAdres.Text) Then
            Try
                Clipboard.SetText(txtAdres.Text)
                ToolTip1.Show("Skopiowano link do schowka!", btnKopiuj, 0, -25, 1500)
            Catch ex As Exception
                lblStatus.Text = "Nie udało się skopiować linku do schowka: " & OpisBledu.Z(ex)
            End Try
        End If
    End Sub

    Private Sub OnZmianaStatusu(aktywne As Integer, komunikat As String)
        If IsDisposed OrElse Disposing Then Return
        If InvokeRequired Then
            If IsHandleCreated Then
                Try
                    BeginInvoke(New Action(Sub() OnZmianaStatusu(aktywne, komunikat)))
                Catch ex As ObjectDisposedException
                Catch ex As InvalidOperationException
                End Try
            End If
            Return
        End If

        If aktywne > 0 Then
            lblStatus.Text = $"Pobieranie w toku ({aktywne} {If(aktywne = 1, "połączenie", "połączenia")}). Nie zamykaj tego okna."
            lblStatus.ForeColor = Color.Blue
        Else
            PokazStanGotowy()
        End If
    End Sub

    ''' <summary>Stan oczekiwania z listą najczęstszych przyczyn, dla których telefon nie otwiera strony.</summary>
    Private Sub PokazStanGotowy()
        lblStatus.Text = "Gotowy do pobrania (oczekiwanie na telefon...). Strona się nie otwiera? Telefon musi być w tej samej " &
                         "sieci Wi-Fi (nie gościnnej), Zapora Windows musi zezwalać programowi na ruch, a sieć powinna być prywatna. " &
                         "Przy włączonym VPN wybierz inny adres z listy."
        lblStatus.ForeColor = Color.DarkSlateGray
    End Sub

    Private Sub FormWyslijNaTelefon_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _serwer IsNot Nothing AndAlso _serwer.AktywneTransfery > 0 Then
            If MessageBox.Show("Trwa pobieranie mapy przez telefon. Przerwać transfer i zamknąć serwer?",
                               "Pobieranie w toku",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question) <> DialogResult.Yes Then
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub FormWyslijNaTelefon_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _serwer IsNot Nothing Then
            RemoveHandler _serwer.ZmianaStatusu, AddressOf OnZmianaStatusu
            _serwer.Dispose()
            _serwer = Nothing
        End If
        picQr.Image?.Dispose()
        picQr.Image = Nothing
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        Close()
    End Sub


End Class
