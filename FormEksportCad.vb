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
Imports System.Text
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Eksport mapy sesji do programów CAD: rysunek DXF z odnośnikami do rastrów na właściwych współrzędnych
''' (segmenty albo scalony arkusz), obrysem, siatką i numerami segmentów oraz skrypt SCR wstawiający mapę
''' do rysunku otwartego w AutoCAD, AutoCAD LT, BricsCAD lub ZWCAD.
''' </summary>
Public Class FormEksportCad

    Private _folder As String = ""
    Private _konfiguracja As KonfiguracjaSesji
    Private _maTrase As Boolean
    ''' <summary>Plik rysunku wybrany przez użytkownika (pusty - nazwa domyślna w folderze sesji).</summary>
    Private _plik As String = ""

    Private Sub FormEksportCad_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _folder = Form1.FolderSesji
        If _folder = "" OrElse File.Exists(_folder & "conf.txt") = False Then _folder = FolderDownload
        WczytajSesje()
    End Sub

    Private Sub Komunikat(tekst As String, kolor As Color)
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
    End Sub

    ''' <summary>Parametry sesji z conf.txt i opis segmentów.</summary>
    Private Sub WczytajSesje()
        txtFolder.Text = _folder
        lblInfo.Text = ""
        _konfiguracja = Nothing
        _plik = ""
        txtArkusz.Text = ""
        Dim plik As String = _folder & "conf.txt"
        If Not File.Exists(plik) Then
            Komunikat("W wybranym folderze nie ma pliku conf.txt z parametrami segmentów. Wskaż folder z pobranymi segmentami.", Color.Red)
            UstawWidok()
            Exit Sub
        End If
        Try
            _konfiguracja = KonfiguracjaSesji.Wczytaj(plik, KodowanieSystemowe())
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku conf.txt: " & ex.Message, Color.Red)
            UstawWidok()
            Exit Sub
        End Try
        Dim s = _konfiguracja.SiatkaSesji()
        If Not s.Poprawna Then
            _konfiguracja = Nothing
            Komunikat("Plik conf.txt zawiera niepoprawne parametry segmentów.", Color.Red)
            UstawWidok()
            Exit Sub
        End If
        Dim uklad = UkladWspolrzednych.ZKodu(_konfiguracja.UkladEpsg)
        Dim istniejace As Integer = _konfiguracja.LiczbaPlikowSegmentow(_folder)
        _maTrase = Korytarz.ZFolderu(_folder) IsNot Nothing
        lblInfo.Text = "Segmenty: " & s.LiczbaKolumn & " x " & s.LiczbaWierszy & " po " & s.BokSegmentuPx & " px (pliki w folderze: " & istniejace &
                       "), piksel " & _konfiguracja.RozmiarPiksela & If(uklad.Geograficzny, "°", " m") & ", układ " & uklad.Nazwa & vbCrLf &
                       "Mapa: " & OpisMapy() & If(_maTrase, vbCrLf & "Segmenty pobrane wzdłuż trasy (korytarz).", "")

        If uklad.Geograficzny Then
            Komunikat("Sesja pobrana w układzie WGS84 - współrzędne rysunku będą w stopniach. Do programu CAD lepiej pobrać mapę " &
                      "w układzie PL-2000 lub PL-1992 (jednostką rysunku jest wtedy metr).", Color.Red)
        ElseIf istniejace = 0 Then
            Komunikat("W folderze nie ma jeszcze pobranych segmentów - można wyeksportować sam obrys i siatkę segmentów.", Color.Black)
        ElseIf istniejace > EksportCad.ZalecanaLiczbaRastrow Then
            Komunikat("Segmentów jest " & istniejace & " - program CAD może działać wolno. Rozważ scalenie segmentów w jeden arkusz " &
                      "(Narzędzia > Łączenie segmentów) i wstawienie arkusza.", Color.Black)
        Else
            Komunikat("Wybierz elementy mapy i kliknij Eksportuj.", Color.Black)
        End If
        'po zmianie folderu - segmenty, gdy są (wcześniej mogła zostać wybrana opcja "bez rastrów" dla pustego folderu)
        If istniejace > 0 Then rbSegmenty.Checked = True Else rbBezRastrow.Checked = True
        UstawWidok()
    End Sub

    ''' <summary>Zbiór map i warstwy, np. "ortofotomapa Raster".</summary>
    Private Function OpisMapy() As String
        If _konfiguracja Is Nothing Then Return ""
        Return (_konfiguracja.RodzajMapy & " " & String.Join(", ", _konfiguracja.WybraneWarstwy())).Trim()
    End Function

    ''' <summary>Adres serwera: z conf.txt, a w sesjach z poprzednich wersji programu - z pliku zbioru map.</summary>
    Private Function AdresSerwera() As String
        If _konfiguracja.AdresSerwera <> "" Then Return _konfiguracja.AdresSerwera
        Try
            Dim plik As String = Path.Combine(FolderProgramu, "warstwy", _konfiguracja.RodzajMapy & ".txt")
            If File.Exists(plik) Then Return PlikWarstw.Wczytaj(plik).Adres
        Catch
        End Try
        Return ""
    End Function

    ''' <summary>Opis mapy wpisywany w rysunku: mapa, źródło danych, data pobrania i układ współrzędnych.</summary>
    Private Function OpisRysunku(uklad As UkladWspolrzednych) As String
        Dim zrodlo As String = MetadaneMapy.ZrodloDanych(AdresSerwera())
        Return "Mapa: " & OpisMapy() & If(zrodlo <> "", ", źródło: " & zrodlo, "") & vbLf &
               "Pobrano: " & If(_konfiguracja.DataPobrania <> "", _konfiguracja.DataPobrania, "data nieznana") &
               ", układ: " & uklad.Nazwa & " (MapoTero)"
    End Function

    Private Function DomyslnyPlik() As String
        Dim s = _konfiguracja.SiatkaSesji()
        Return _folder & "_cad_" & s.LiczbaKolumn & "x" & s.LiczbaWierszy & ".dxf"
    End Function

    ''' <summary>Dostępność opcji.</summary>
    Private Sub UstawWidok()
        Dim jest As Boolean = _konfiguracja IsNot Nothing
        grpRastry.Enabled = jest
        grpWektory.Enabled = jest
        grpPliki.Enabled = jest
        btnPlik.Enabled = jest
        txtArkusz.Enabled = rbArkusz.Checked
        btnArkusz.Enabled = rbArkusz.Checked
        chkTrasa.Enabled = _maTrase
        chkSciezkiWzgledne.Enabled = chkDxf.Checked
        txtPlik.Text = If(Not jest, "", If(_plik <> "", _plik, DomyslnyPlik()))
        'bez rastrów i bez elementów wektorowych plik byłby pusty
        Dim cosDoZapisu As Boolean = Not rbBezRastrow.Checked OrElse chkObrys.Checked OrElse chkSiatka.Checked OrElse
                                     (chkNumery.Checked AndAlso chkDxf.Checked) OrElse (chkTrasa.Checked AndAlso _maTrase)
        btnEksportuj.Enabled = jest AndAlso (chkDxf.Checked OrElse chkScr.Checked) AndAlso (Not rbArkusz.Checked OrElse txtArkusz.Text <> "") AndAlso cosDoZapisu
    End Sub

    Private Sub OpcjeZmienione(sender As Object, e As EventArgs) Handles rbSegmenty.CheckedChanged, rbArkusz.CheckedChanged, rbBezRastrow.CheckedChanged,
                                                                       chkDxf.CheckedChanged, chkScr.CheckedChanged, chkObrys.CheckedChanged,
                                                                       chkSiatka.CheckedChanged, chkNumery.CheckedChanged, chkTrasa.CheckedChanged
        UstawWidok()
    End Sub

    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        dlgFolder.SelectedPath = _folder
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            _folder = dlgFolder.SelectedPath.TrimEnd("\"c) & "\"
            WczytajSesje()
        End If
    End Sub

    Private Sub btnArkusz_Click(sender As Object, e As EventArgs) Handles btnArkusz.Click
        dlgArkusz.InitialDirectory = _folder
        If dlgArkusz.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Try
            Dim uwagi As New List(Of String)
            Dim a = EksportCad.Arkusz(dlgArkusz.FileName, UkladWspolrzednych.ZKodu(_konfiguracja.UkladEpsg), uwagi)
            'arkusz z innej sesji (inny obszar albo inny układ współrzędnych) nie pokryje obrysu i siatki tej sesji
            Dim zs = _konfiguracja.SiatkaSesji().ZasiegSiatki
            Dim za = a.Zasieg
            If za.YPrawy <= zs.YLewy OrElse za.YLewy >= zs.YPrawy OrElse za.XGora <= zs.XDol OrElse za.XDol >= zs.XGora Then
                uwagi.Insert(0, "Uwaga: arkusz leży poza obszarem tej sesji - może pochodzić z innej sesji lub być zapisany w innym układzie " &
                                "współrzędnych niż " & UkladWspolrzednych.ZKodu(_konfiguracja.UkladEpsg).Nazwa & ".")
            End If
            txtArkusz.Text = dlgArkusz.FileName
            Komunikat("Arkusz " & a.SzerokoscPx & " x " & a.WysokoscPx & " px." & If(uwagi.Count > 0, vbCrLf & String.Join(vbCrLf, uwagi), ""),
                      If(uwagi.Any(Function(u) u.StartsWith("Uwaga")), Color.Red, Color.Black))
        Catch ex As Exception
            txtArkusz.Text = ""
            Komunikat("Nie można wstawić arkusza " & Path.GetFileName(dlgArkusz.FileName) & ": " & ex.Message, Color.Red)
        End Try
        UstawWidok()
    End Sub

    Private Sub btnPlik_Click(sender As Object, e As EventArgs) Handles btnPlik.Click
        Dim biezacy As String = txtPlik.Text
        dlgPlik.InitialDirectory = Path.GetDirectoryName(biezacy)
        dlgPlik.FileName = Path.GetFileName(biezacy)
        If dlgPlik.ShowDialog(Me) = DialogResult.OK Then
            _plik = dlgPlik.FileName
            UstawWidok()
        End If
    End Sub

    Private Async Sub btnEksportuj_Click(sender As Object, e As EventArgs) Handles btnEksportuj.Click
        If _konfiguracja Is Nothing Then Exit Sub
        Dim plikDxf As String = txtPlik.Text
        Dim plikScr As String = Path.ChangeExtension(plikDxf, ".scr")
        Dim folder As String = _folder
        Dim konfiguracja = _konfiguracja
        Dim numeracja = Siatka.StylZTekstu(If(konfiguracja.Numeracja = "", Ustawienia.Numeracja, konfiguracja.Numeracja))
        Dim segmenty As Boolean = rbSegmenty.Checked
        Dim arkusz As String = If(rbArkusz.Checked, txtArkusz.Text, "")
        Dim dxf As Boolean = chkDxf.Checked, scr As Boolean = chkScr.Checked
        Dim wzgledne As Boolean = chkSciezkiWzgledne.Checked
        Dim obrys As Boolean = chkObrys.Checked, siatkaSegmentow As Boolean = chkSiatka.Checked, numery As Boolean = chkNumery.Checked
        Dim trasa As Boolean = chkTrasa.Checked AndAlso _maTrase
        Dim opis As String = If(chkOpis.Checked, OpisRysunku(UkladWspolrzednych.ZKodu(konfiguracja.UkladEpsg)), "")

        Komunikat("Tworzenie plików...", Color.Black)
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            Dim z = Await Task.Run(
                Function()
                    Dim zad = EksportCad.ZSesji(folder, konfiguracja, numeracja, segmenty)
                    If arkusz <> "" Then zad.Rastry.Add(EksportCad.Arkusz(arkusz, zad.Uklad, zad.Uwagi))
                    zad.RysujObrys = obrys
                    zad.RysujSiatke = siatkaSegmentow
                    zad.RysujNumery = numery
                    zad.RysujTrase = trasa
                    zad.Opis = opis
                    zad.FolderRysunku = If(wzgledne, Path.GetDirectoryName(plikDxf), "")
                    'rysunek i skrypt w kodowaniu systemu (ANSI, bez BOM) - tak jak odczytują je programy CAD w tym samym systemie
                    Dim kod As Encoding = EksportCad.KodowanieRysunku(KodowanieSystemowe())
                    If dxf Then File.WriteAllText(plikDxf, EksportCad.Dxf(zad, kod), kod)
                    If scr Then
                        Dim skrypt As String = EksportCad.Skrypt(zad)
                        'w skrypcie ścieżki rastrów muszą być zapisane dosłownie (rysunek DXF zapisuje inne znaki jako \U+XXXX)
                        If EksportCad.ZnakiSpozaKodowania(skrypt, kod) Then
                            zad.Uwagi.Add("Ścieżka rastrów zawiera znaki spoza strony kodowej " & kod.WebName & " - skrypt SCR nie znajdzie tych plików. " &
                                          "Przenieś folder z mapą do ścieżki bez takich znaków albo użyj rysunku DXF.")
                        End If
                        File.WriteAllText(plikScr, skrypt, kod)
                    End If
                    Return zad
                End Function)

            Dim sb As New StringBuilder()
            sb.Append("Zapisano (rastrów: ").Append(z.Rastry.Count).Append("):").Append(vbCrLf)
            If dxf Then
                sb.Append(plikDxf).Append(" - otwórz w programie CAD. Do istniejącego rysunku obiekty można przenieść ze schowka " &
                          "poleceniem PASTEORIG (wkleja na tych samych współrzędnych).").Append(vbCrLf)
            End If
            If scr Then sb.Append(plikScr).Append(" - w otwartym rysunku (jednostki: metry) polecenie SCRIPT i wskaż ten plik.").Append(vbCrLf)
            sb.Append("Oś X rysunku to wschód, oś Y - północ. Warstwy: ").Append(EksportCad.WarstwaRastrow).Append(", ").
               Append(EksportCad.WarstwaObrysu).Append(", ").Append(EksportCad.WarstwaSiatki).Append(", ").Append(EksportCad.WarstwaNumerow).Append(".")
            If dxf AndAlso z.Rastry.Count > 0 Then
                sb.Append(vbCrLf).Append("Ramki rastrów w rysunku DXF są ukryte - aby zaznaczać rastry kliknięciem, ustaw IMAGEFRAME na 1 lub 2.")
            End If
            For Each u In z.Uwagi
                sb.Append(vbCrLf).Append(u)
            Next
            Komunikat(sb.ToString(), Color.Green)
        Catch ex As Exception
            Komunikat("Eksport nie powiódł się: " & ex.Message, Color.Red)
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
    End Sub

End Class
