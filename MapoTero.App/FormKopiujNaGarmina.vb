Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core

    ''' <summary>
    ''' Samodzielne okno WinForms do kopiowania wyeksportowanej mapy (.kmz / .jnx)
    ''' na odbiornik Garmin w trybie pamięci masowej USB, z jasnym komunikatem o MTP.
    ''' </summary>
    Public Class FormKopiujNaGarmina
        Inherits Form

        Private ReadOnly _sciezkaZrodlowa As String
        Private ReadOnly _rozmiarPliku As Long
        Private _cts As CancellationTokenSource

        Private ReadOnly _lstDyski As New ListBox()
        Private ReadOnly _txtNazwaDocelowa As New TextBox()
        Private ReadOnly _lblInformacjaPlik As New Label()
        Private ReadOnly _lblMtpInfo As New Label()
        Private ReadOnly _lblStatus As New Label()
        Private ReadOnly _pasekPostepu As New ProgressBar()
        Private ReadOnly _btnOdswiez As New Button()
        Private ReadOnly _btnKopiuj As New Button()
        Private ReadOnly _btnPokazWFolderze As New Button()
        Private ReadOnly _btnZamknij As New Button()

        Public Sub New(sciezkaZrodlowa As String)
            _sciezkaZrodlowa = sciezkaZrodlowa
            _rozmiarPliku = If(File.Exists(sciezkaZrodlowa), New FileInfo(sciezkaZrodlowa).Length, 0L)

            ZbudujInterfejs()
            OdswiezListeDyskow()
        End Sub

        Private Sub ZbudujInterfejs()
            Me.Text = "Kopiuj do pamięci masowej Garmina (USB)"
            Me.AutoScaleMode = AutoScaleMode.Dpi
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.ClientSize = New Size(560, 390)
            Me.Font = New Font("Segoe UI", 9.0F)

            Dim podfolder As String = If(Path.GetExtension(_sciezkaZrodlowa).Equals(".jnx", StringComparison.OrdinalIgnoreCase),
                                         "\Garmin\BirdsEye\",
                                         "\Garmin\CustomMaps\")

            _lblInformacjaPlik.SetBounds(16, 14, 528, 22)
            _lblInformacjaPlik.Text = String.Format("Plik mapy: {0} ({1}) → {2}",
                                                    Path.GetFileName(_sciezkaZrodlowa),
                                                    DyskGarmina.FormatujRozmiar(_rozmiarPliku),
                                                    podfolder)

            Dim lblDyski As New Label() With {
                .Text = "Wykryte nośniki z folderem \Garmin\:",
                .Location = New Point(16, 44),
                .Size = New Size(380, 20)
            }

            _btnOdswiez.Text = "Odśwież"
            _btnOdswiez.SetBounds(454, 40, 90, 26)
            AddHandler _btnOdswiez.Click, Sub(s, e) OdswiezListeDyskow()

            _lstDyski.SetBounds(16, 70, 528, 96)
            AddHandler _lstDyski.SelectedIndexChanged, Sub(s, e) AktualizujStanPrzyciskuKopiuj()

            Dim lblNazwa As New Label() With {
                .Text = "Nazwa pliku na urządzeniu (bez polskich znaków i spacji):",
                .Location = New Point(16, 174),
                .Size = New Size(528, 20)
            }

            _txtNazwaDocelowa.SetBounds(16, 196, 528, 24)
            _txtNazwaDocelowa.Text = MenadzerGarminUsb.SanityzujNazwePlikuGarmina(Path.GetFileName(_sciezkaZrodlowa))

            Dim panelMtp As New Panel() With {
                .Location = New Point(16, 230),
                .Size = New Size(528, 66),
                .BackColor = Color.FromArgb(245, 248, 255),
                .BorderStyle = BorderStyle.FixedSingle
            }
            _lblMtpInfo.SetBounds(8, 6, 510, 54)
            _lblMtpInfo.Text = _
                "Uwaga: funkcja wykrywa starsze odbiorniki pracujące w trybie pamięci masowej USB " &
                "(widoczne jako dysk z literą, np. GPSMAP 62/64, eTrex 20/30, Oregon). " &
                "Dla nowszych modeli MTP (np. GPSMAP 67, Montana 700, zegarki Fenix/Epix) " &
                "użyj przycisku 'Pokaż w folderze' i przeciągnij plik ręcznie do pamięci urządzenia."
            panelMtp.Controls.Add(_lblMtpInfo)

            _pasekPostepu.SetBounds(16, 306, 528, 18)
            _pasekPostepu.Minimum = 0
            _pasekPostepu.Maximum = 100

            _lblStatus.SetBounds(16, 328, 528, 18)
            _lblStatus.ForeColor = Color.DimGray

            _btnPokazWFolderze.Text = "Pokaż w folderze"
            _btnPokazWFolderze.SetBounds(16, 352, 140, 28)
            AddHandler _btnPokazWFolderze.Click, Sub(s, e) MenadzerGarminUsb.PokazWFolderze(_sciezkaZrodlowa)

            _btnKopiuj.Text = "Kopiuj na Garmin"
            _btnKopiuj.SetBounds(324, 352, 130, 28)
            AddHandler _btnKopiuj.Click, AddressOf BtnKopiuj_Click

            _btnZamknij.Text = "Zamknij"
            _btnZamknij.SetBounds(462, 352, 82, 28)
            AddHandler _btnZamknij.Click, Sub(s, e)
                                              If _cts IsNot Nothing Then _cts.Cancel()
                                              Me.Close()
                                          End Sub
            Me.CancelButton = _btnZamknij

            Me.Controls.AddRange(New Control() {
                _lblInformacjaPlik, lblDyski, _btnOdswiez, _lstDyski,
                lblNazwa, _txtNazwaDocelowa, panelMtp,
                _pasekPostepu, _lblStatus, _btnPokazWFolderze, _btnKopiuj, _btnZamknij
            })
            'przycisk Pomoc i F1 (audyt ER-33); okno budowane w kodzie, więc położenie ustawiane wprost
            Dim pomoc = PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.PrzeslijMape)
            pomoc.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            pomoc.SetBounds(164, 352, 82, 28)
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            _cts?.Cancel()
            MyBase.OnFormClosing(e)
        End Sub

        Private Sub OdswiezListeDyskow(Optional zachowajStatus As Boolean = False)
            _lstDyski.Items.Clear()
            Dim dyski As List(Of DyskGarmina) = MenadzerGarminUsb.WykryjDyskiGarmina()

            For Each d As DyskGarmina In dyski
                _lstDyski.Items.Add(d)
            Next

            If _lstDyski.Items.Count > 0 Then
                _lstDyski.SelectedIndex = 0
                If Not zachowajStatus Then
                    _lblStatus.ForeColor = Color.DimGray
                    _lblStatus.Text = String.Format("Znaleziono nośników Garmina: {0}.", _lstDyski.Items.Count)
                End If
            ElseIf Not zachowajStatus Then
                _lblStatus.ForeColor = Color.DarkOrange
                _lblStatus.Text = "Nie wykryto odbiornika Garmin w trybie pamięci masowej USB."
            End If

            AktualizujStanPrzyciskuKopiuj()
        End Sub

        Private Sub AktualizujStanPrzyciskuKopiuj()
            Dim wybrany As DyskGarmina = TryCast(_lstDyski.SelectedItem, DyskGarmina)
            If wybrany Is Nothing Then
                _btnKopiuj.Enabled = False
                Return
            End If

            Dim wystarczyMiejsca As Boolean = wybrany.WolneMiejsceBajty >= (_rozmiarPliku + MenadzerGarminUsb.MarginesWolnegoMiejscaBajty)
            _btnKopiuj.Enabled = wystarczyMiejsca
            If Not wystarczyMiejsca Then
                _lblStatus.ForeColor = Color.Firebrick
                _lblStatus.Text = "Za mało wolnego miejsca na wybranym nośniku (wybierz kartę microSD)."
            End If
        End Sub

        Private Async Sub BtnKopiuj_Click(sender As Object, e As EventArgs)
            Dim wybrany As DyskGarmina = TryCast(_lstDyski.SelectedItem, DyskGarmina)
            If wybrany Is Nothing Then Return

            _btnKopiuj.Enabled = False
            _btnOdswiez.Enabled = False
            _pasekPostepu.Value = 0
            _lblStatus.ForeColor = Color.DimGray
            _lblStatus.Text = "Kopiowanie mapy na urządzenie..."

            _cts = New CancellationTokenSource()
            Dim postep As New Progress(Of Double)(Sub(procent)
                                                      If IsDisposed OrElse Disposing Then Return
                                                      _pasekPostepu.Value = Math.Max(0, Math.Min(100, CInt(Math.Round(procent))))
                                                  End Sub)

            Try
                Dim cel As String = Await MenadzerGarminUsb.KopiujNaGarminaAsync(
                    _sciezkaZrodlowa, wybrany, _txtNazwaDocelowa.Text, postep, _cts.Token)

                If IsDisposed OrElse Disposing Then Return
                _pasekPostepu.Value = 100
                _lblStatus.ForeColor = Color.DarkGreen
                _lblStatus.Text = String.Format("Zapisano pomyślnie: {0}", cel)
                OdswiezListeDyskow(zachowajStatus:=True)
            Catch ex As OperationCanceledException
                If Not IsDisposed AndAlso Not Disposing Then
                    _lblStatus.ForeColor = Color.DarkOrange
                    _lblStatus.Text = "Kopiowanie zostało przerwane."
                End If
            Catch ex As Exception
                If Not IsDisposed AndAlso Not Disposing Then
                    _lblStatus.ForeColor = Color.Firebrick
                    _lblStatus.Text = "Błąd kopiowania: " & OpisBledu.Z(ex)
                End If
            Finally
                _cts?.Dispose()
                _cts = Nothing
                If Not IsDisposed AndAlso Not Disposing Then
                    _btnOdswiez.Enabled = True
                    AktualizujStanPrzyciskuKopiuj()
                End If
            End Try
        End Sub

        Public Shared Sub PokazDlaPliku(rodzic As IWin32Window, sciezkaPliku As String)
            If Not File.Exists(sciezkaPliku) Then Exit Sub
            Using dlg As New FormKopiujNaGarmina(sciezkaPliku)
                dlg.ShowDialog(rodzic)
            End Using
        End Sub
    End Class
