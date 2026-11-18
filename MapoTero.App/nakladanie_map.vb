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

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Nakładanie dwóch zbiorów segmentów PNG (np. kontury działek na ortofotomapę): białe piksele warstwy górnej
''' stają się przezroczyste.
''' </summary>
Public Class Nakladanie_Map

    Private _opcjeKorekty As New OpcjeKorekcjiMip()

    Private Sub Nakladanie_Map_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.ZamykanieKlawiszemEsc(Me, rozmiarMinimalny:=True)
        Me.Location = New Point(200, 150)
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.NakladanieMap, 15, 455)

        _opcjeKorekty.UstawWgProfilu(ProfilKorektyRastra.RozjasnienieLasow)
        chkKorekta.Checked = False
        'UstawWgProfilu włącza korektę, a pole jest odznaczone (zdarzenie CheckedChanged nie zachodzi bez zmiany stanu)
        _opcjeKorekty.Wlaczona = False
        AktualizujPrzyciskKorekty()

        'foldery zapamiętane w lastsettings.txt, a gdy ich brak - domyślne podkatalogi folderu download
        txtFolderDolna.Text = If(Ustawienia.FolderWarstwy1 <> "", Ustawienia.FolderWarstwy1, FolderDownload & "dolna\")
        txtFolderGorna.Text = If(Ustawienia.FolderWarstwy2 <> "", Ustawienia.FolderWarstwy2, FolderDownload & "gorna\")
        txtFolderWynik.Text = If(Ustawienia.FolderWynikowy <> "", Ustawienia.FolderWynikowy, FolderDownload & "polaczone\")
    End Sub

    Private Sub chkKorekta_CheckedChanged(sender As Object, e As EventArgs) Handles chkKorekta.CheckedChanged
        _opcjeKorekty.Wlaczona = chkKorekta.Checked
        'po wyborze „Brak” w podglądzie zaznaczenie pola stosowało ukryte wartości suwaków poprzedniego profilu
        If chkKorekta.Checked AndAlso _opcjeKorekty.Profil = ProfilKorektyRastra.Brak Then
            _opcjeKorekty.UstawWgProfilu(ProfilKorektyRastra.RozjasnienieLasow)
        End If
        AktualizujPrzyciskKorekty()
    End Sub

    Private Sub btnKorekta_Click(sender As Object, e As EventArgs) Handles btnKorekta.Click
        Dim probka As Bitmap = PrzygotujProbkeDoPodgladu()
        Using dlg As New FormPodgladMip(_opcjeKorekty, probka)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeKorekty = dlg.Opcje.Kopia()
                chkKorekta.Checked = _opcjeKorekty.Wlaczona
                AktualizujPrzyciskKorekty()
            End If
        End Using
        probka?.Dispose()
    End Sub

    Private Function PrzygotujProbkeDoPodgladu() As Bitmap
        Try
            Dim dolna As String = KoniecUkosnik(txtFolderDolna.Text)
            Dim gorna As String = KoniecUkosnik(txtFolderGorna.Text)
            If Directory.Exists(dolna) AndAlso Directory.Exists(gorna) Then
                For Each plikD In Directory.GetFiles(dolna, "*.png")
                    Dim nazwa = Path.GetFileName(plikD)
                    Dim plikG = Path.Combine(gorna, nazwa)
                    If File.Exists(plikG) Then
                        Using w1P As New Bitmap(plikD), w2P As New Bitmap(plikG)
                            Dim res As New Bitmap(w1P.Width, w1P.Height, PixelFormat.Format32bppArgb)
                            Using w2 As New Bitmap(w2P.Width, w2P.Height, PixelFormat.Format32bppArgb)
                                Using g2 = Graphics.FromImage(w2)
                                    g2.DrawImage(w2P, 0, 0, w2P.Width, w2P.Height)
                                End Using
                                w2.MakeTransparent(Color.White)
                                Using g = Graphics.FromImage(res)
                                    g.DrawImage(w1P, 0, 0, w1P.Width, w1P.Height)
                                    g.DrawImage(w2, 0, 0, w2.Width, w2.Height)
                                End Using
                            End Using
                            Return res
                        End Using
                    End If
                Next
            End If
        Catch
        End Try
        Return Nothing
    End Function

    Private Sub AktualizujPrzyciskKorekty()
        Dim nazwa = OpcjeKorekcjiMip.KrotkaNazwaProfilu(_opcjeKorekty.Profil)
        btnKorekta.Text = "Dostosuj (" & nazwa & ")..."
    End Sub

    'foldery zapisywane przy zamykaniu programu w lastsettings.txt
    Private Sub txtFolderDolna_TextChanged(sender As Object, e As EventArgs) Handles txtFolderDolna.TextChanged
        Ustawienia.FolderWarstwy1 = txtFolderDolna.Text
    End Sub

    Private Sub txtFolderGorna_TextChanged(sender As Object, e As EventArgs) Handles txtFolderGorna.TextChanged
        Ustawienia.FolderWarstwy2 = txtFolderGorna.Text
    End Sub

    Private Sub txtFolderWynik_TextChanged(sender As Object, e As EventArgs) Handles txtFolderWynik.TextChanged
        Ustawienia.FolderWynikowy = txtFolderWynik.Text
    End Sub

    ''' <summary>
    ''' Wybór folderu. Folder warstwy dolnej lub górnej staje się też folderem sesji okna głównego - dzięki temu
    ''' kolejne pobieranie zapisze segmenty właśnie do niego (np. najpierw warstwa dolna, potem górna); zmiana jest
    ''' ogłaszana w oknie głównym. Folder wynikowy nie zmienia folderu sesji (dawniej zmieniał - kolejne pobieranie
    ''' trafiało do folderu z nałożonymi mapami, których pliki uznawano za pobrane segmenty).
    ''' </summary>
    Private Function WybierzFolder(biezacy As String, folderSesji As Boolean) As String
        dlgFolder.SelectedPath = If(Directory.Exists(biezacy), biezacy, Form1.FolderSesji)
        If dlgFolder.ShowDialog = DialogResult.OK Then
            Dim folder As String = dlgFolder.SelectedPath.TrimEnd("\"c) & "\"
            If folderSesji AndAlso Not String.Equals(folder, Form1.FolderSesji, StringComparison.OrdinalIgnoreCase) Then
                Form1.FolderSesji = folder
                Form1.Komunikat("Folder sesji pobierania zmieniono na " & folder & " - kolejne pobieranie zapisze segmenty do tego folderu.", Color.Green)
            End If
            Return folder
        End If
        Return biezacy
    End Function

    Private Sub ButtonDolna_Click(sender As Object, e As EventArgs) Handles ButtonDolna.Click
        txtFolderDolna.Text = WybierzFolder(txtFolderDolna.Text, True)
    End Sub

    Private Sub ButtonGorna_Click(sender As Object, e As EventArgs) Handles ButtonGorna.Click
        txtFolderGorna.Text = WybierzFolder(txtFolderGorna.Text, True)
    End Sub

    Private Sub ButtonPolaczone_Click(sender As Object, e As EventArgs) Handles ButtonPolaczone.Click
        txtFolderWynik.Text = WybierzFolder(txtFolderWynik.Text, False)
    End Sub

    Private Async Sub btnSkladaj_Click(sender As Object, e As EventArgs) Handles btnSkladaj.Click
        Dim dolna As String = KoniecUkosnik(txtFolderDolna.Text)
        Dim gorna As String = KoniecUkosnik(txtFolderGorna.Text)
        Dim wynik As String = KoniecUkosnik(txtFolderWynik.Text)

        'ścieżka względna (np. wpisane „abc”) wskazywałaby folder względem bieżącego katalogu procesu, a nie folderu programu
        If Not Path.IsPathRooted(dolna) OrElse Not Path.IsPathRooted(gorna) OrElse Not Path.IsPathRooted(wynik) Then
            MessageBox.Show("Podaj pełne ścieżki folderów (np. C:\Mapy\dolna\) albo wskaż je przyciskami ""Dolna"", ""Górna"" i ""Połączone"".",
                            "Nakładanie map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If
        If Not Directory.Exists(dolna) OrElse Not Directory.Exists(gorna) Then
            MessageBox.Show("Folder warstwy dolnej lub górnej nie istnieje:" & vbCrLf & dolna & vbCrLf & gorna & vbCrLf & vbCrLf &
                   "Pobierz do nich segmenty obu map (format png, te same ustawienia geometrii) i powtórz operację.",
                            "Nakładanie map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If
        If String.Equals(dolna, gorna, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(dolna, wynik, StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(gorna, wynik, StringComparison.OrdinalIgnoreCase) Then
            MsgBox("Folder warstwy dolnej:" & vbCrLf & dolna & vbCrLf & "Folder warstwy górnej:" & vbCrLf & gorna & vbCrLf &
                   "Folder warstw połączonych:" & vbCrLf & wynik & vbCrLf & vbCrLf & "Wybierz trzy różne foldery.", MsgBoxStyle.Exclamation, "Nakładanie map")
            Exit Sub
        End If
        If Directory.GetFiles(dolna, "*.png").Length = 0 Then
            MsgBox("Folder warstwy dolnej nie zawiera segmentów png:" & vbCrLf & dolna & vbCrLf & vbCrLf &
                   "Umieść w nim segmenty warstwy podkładowej w formacie png o tych samych nazwach i rozdzielczości co w folderze warstwy górnej.",
                   MsgBoxStyle.Exclamation, "Nakładanie map")
            Exit Sub
        End If
        If Directory.GetFiles(gorna, "*.png").Length = 0 Then
            MsgBox("Folder warstwy górnej nie zawiera segmentów png:" & vbCrLf & gorna & vbCrLf & vbCrLf &
                   "Umieść w nim segmenty nakładanej warstwy w formacie png o tych samych nazwach i rozdzielczości co w folderze warstwy dolnej.",
                   MsgBoxStyle.Exclamation, "Nakładanie map")
            Exit Sub
        End If

        'folder wynikowy tworzony w razie potrzeby (dawniej trzeba go było utworzyć samemu)
        Try
            Directory.CreateDirectory(wynik)
        Catch ex As Exception
            MsgBox("Nie udało się utworzyć folderu " & wynik & ": " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Nakładanie map")
            Exit Sub
        End Try

        btnSkladaj.Enabled = False
        Me.UseWaitCursor = True
        Dim liczba As Integer
        Dim opcjeKorekty = If(chkKorekta.Checked, _opcjeKorekty.Kopia(), Nothing)
        Try
            liczba = Await Task.Run(Function() NalozWarstwy(dolna, gorna, wynik, opcjeKorekty))
        Finally
            btnSkladaj.Enabled = True
            Me.UseWaitCursor = False
        End Try

        MsgBox("Pliki zostały zapisane (" & liczba & ")", MsgBoxStyle.Information, "Nakładanie map")
        OtworzFolder(wynik)
    End Sub

    Private Shared Function KoniecUkosnik(folder As String) As String
        Dim f As String = folder.Trim()
        If f <> "" AndAlso Not f.EndsWith("\") Then f &= "\"
        Return f
    End Function

    ''' <summary>Nakłada segmenty o tych samych nazwach; zwraca liczbę zapisanych plików.</summary>
    Private Shared Function NalozWarstwy(dolna As String, gorna As String, wynik As String, opcjeKorekty As OpcjeKorekcjiMip) As Integer
        Dim liczba As Integer = 0

        ' Kopiowanie conf.txt jeśli istnieje w warstwie dolnej
        Dim confDolna As String = Path.Combine(dolna, "conf.txt")
        If File.Exists(confDolna) Then
            Try
                File.Copy(confDolna, Path.Combine(wynik, "conf.txt"), True)
            Catch
            End Try
        End If

        For Each plikDolny In Directory.GetFiles(dolna, "*.png")
            Dim nazwaPliku As String = Path.GetFileName(plikDolny)
            Dim plikGorny As String = Path.Combine(gorna, nazwaPliku)
            If File.Exists(plikGorny) = False Then Continue For
            'Using zwalnia pamięć i blokady plików po każdym segmencie; kopia dolnej warstwy w formacie 32-bitowym,
            'bo na obrazach z paletą barw (np. png8) nie da się rysować
            Try
                Using warstwa1Plik As New Bitmap(plikDolny), warstwa2Plik As New Bitmap(plikGorny)
                    Using warstwa1 As New Bitmap(warstwa1Plik.Width, warstwa1Plik.Height, PixelFormat.Format32bppArgb),
                          warstwa2 As New Bitmap(warstwa2Plik.Width, warstwa2Plik.Height, PixelFormat.Format32bppArgb)
                        Using g2 As Graphics = Graphics.FromImage(warstwa2)
                            g2.DrawImage(warstwa2Plik, 0, 0, warstwa2Plik.Width, warstwa2Plik.Height)
                        End Using
                        warstwa2.MakeTransparent(Color.White)
                        Using g As Graphics = Graphics.FromImage(warstwa1)
                            g.DrawImage(warstwa1Plik, 0, 0, warstwa1Plik.Width, warstwa1Plik.Height)
                            g.DrawImage(warstwa2, 0, 0, warstwa2.Width, warstwa2.Height)
                        End Using

                        If opcjeKorekty IsNot Nothing AndAlso opcjeKorekty.Wlaczona Then
                            NakladanieObrazow.ZastosujKorekcjeMip(warstwa1, opcjeKorekty)
                        End If

                        Dim plikDocelowy As String = Path.Combine(wynik, nazwaPliku)
                        'przez plik tymczasowy - przerwanie (np. zamknięcie programu) nie zostawia uszkodzonego segmentu wynikowego
                        Dim tymczasowy As String = plikDocelowy & "." & Guid.NewGuid().ToString("N") & ".tmp"
                        Try
                            warstwa1.Save(tymczasowy, ImageFormat.Png)
                            File.Move(tymczasowy, plikDocelowy, overwrite:=True)
                        Finally
                            If File.Exists(tymczasowy) Then File.Delete(tymczasowy)
                        End Try
                        liczba += 1

                        ' Kopiowanie georeferencji z warstwy dolnej
                        Dim bezRozszerzenia As String = Path.ChangeExtension(plikDolny, Nothing)
                        For Each ext In {".pngw", ".wld", ".prj", ".map", ".gmi", ".tab", ".kml", ".png.points", ".pgw"}
                            Dim plikGeoDolny As String = bezRozszerzenia & ext
                            If File.Exists(plikGeoDolny) Then
                                Try
                                    File.Copy(plikGeoDolny, Path.Combine(wynik, Path.GetFileName(plikGeoDolny)), True)
                                Catch
                                End Try
                            End If
                        Next
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Błąd nakładania segmentu " & nazwaPliku & ": " & ex.Message)
            End Try
        Next
        Return liczba
    End Function

End Class
