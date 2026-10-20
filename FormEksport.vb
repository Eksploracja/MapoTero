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
''' KMZ (Garmin Custom Maps, Locus Map, OruxMaps), MBTiles (Locus Map, OsmAnd, OruxMaps), SQLiteDB (OsmAnd, Locus Map)
''' i mapy offline OruxMaps. Na mapę można nałożyć warstwy z innych folderów sesji (np. granice działek na ortofotomapę),
''' a mapę z przezroczystym tłem wyeksportować jako nakładkę wyświetlaną nad mapą podkładową aplikacji.
''' </summary>
Public Class FormEksport

    ''' <summary>Segmenty i parametry sesji pobierania odczytane z folderu z plikiem conf.txt.</summary>
    Private Class Sesja
        Public Folder As String = ""
        Public Konfiguracja As KonfiguracjaSesji
        Public Siatka As Siatka
        Public Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
        Public Zrodlo As ZrodloSegmentow

        ''' <summary>Zbiór map i pierwsza warstwa, np. "ortofotomapa Raster".</summary>
        Public ReadOnly Property Opis As String
            Get
                Return (Konfiguracja.RodzajMapy & " " & Konfiguracja.Warstwy(0)).Trim()
            End Get
        End Property

        ''' <summary>
        ''' Adres serwera: z conf.txt, a w sesjach z poprzednich wersji programu - z pliku zbioru map w katalogu "warstwy".
        ''' </summary>
        Public ReadOnly Property AdresSerwera As String
            Get
                If Konfiguracja.AdresSerwera <> "" Then Return Konfiguracja.AdresSerwera
                Try
                    Dim plik As String = Path.Combine(FolderProgramu, "warstwy", Konfiguracja.RodzajMapy & ".txt")
                    If File.Exists(plik) Then Return PlikWarstw.Wczytaj(plik).Adres
                Catch
                End Try
                Return ""
            End Get
        End Property
    End Class

    ''' <summary>Warstwa nakładana na mapę - pozycja listy.</summary>
    Private Class PozycjaNakladki
        Public Sesja As Sesja
        Public Warstwa As WarstwaEksportu

        Public Overrides Function ToString() As String
            Return Warstwa.Opis & " - krycie " & CInt(Warstwa.Krycie * 100) & "%" &
                   If(Warstwa.Zrodlo.BialePrzezroczyste, ", białe tło przezroczyste", "") & " (" & Sesja.Folder & ")"
        End Function
    End Class

    Private _folder As String = ""
    Private _sesja As Sesja
    Private _przerwanie As CancellationTokenSource
    ''' <summary>Programowe ustawianie kontrolek warstwy (bez reakcji na zdarzenia).</summary>
    Private _ustawianie As Boolean

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

    ''' <summary>Odczyt sesji z folderu: conf.txt i lista istniejących segmentów. Nothing i opis błędu, gdy się nie udało.</summary>
    Private Shared Function OdczytajSesje(folder As String, ByRef blad As String) As Sesja
        blad = ""
        Dim plik As String = folder & "conf.txt"
        If File.Exists(plik) = False Then
            blad = "W wybranym folderze nie ma pliku conf.txt z parametrami segmentów. Wskaż folder z pobranymi segmentami."
            Return Nothing
        End If
        Dim s As New Sesja With {.Folder = folder}
        Try
            s.Konfiguracja = KonfiguracjaSesji.Wczytaj(plik, KodowanieSystemowe())
            Dim c = s.Konfiguracja
            s.Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
            s.Siatka = New Siatka(New Zasieg(Wartosc(c.XDol), Wartosc(c.YLewy), Wartosc(c.XGora), Wartosc(c.YPrawy)),
                                  Wartosc(c.RozmiarPiksela), WartoscCalkowita(c.BokSegmentuPx))
        Catch ex As Exception
            blad = "Nie udało się odczytać pliku conf.txt: " & ex.Message
            Return Nothing
        End Try
        If Not s.Siatka.Poprawna Then
            blad = "Plik conf.txt zawiera niepoprawne parametry segmentów."
            Return Nothing
        End If
        Dim k = s.Konfiguracja
        s.Zrodlo = New ZrodloSegmentow(folder, k.Prefiks, Siatka.StylZTekstu(If(k.Numeracja = "", Ustawienia.Numeracja, k.Numeracja)),
                                       Wms.RozszerzeniePliku(k.Format), s.Siatka)
        Return s
    End Function

    ''' <summary>Parametry segmentów z conf.txt.</summary>
    Private Sub WczytajSesje()
        txtFolder.Text = _folder
        _sesja = Nothing
        lblInfo.Text = ""
        lblRozmiar.Text = ""
        Dim blad As String = ""
        Dim s = OdczytajSesje(_folder, blad)
        If s Is Nothing Then
            Komunikat(blad, Color.Red)
            Exit Sub
        End If
        _sesja = s
        Dim c = s.Konfiguracja

        Dim zrodlo As String = MetadaneMapy.ZrodloDanych(s.AdresSerwera)
        lblInfo.Text = "Segmenty: " & s.Siatka.LiczbaKolumn & " x " & s.Siatka.LiczbaWierszy & " po " & s.Siatka.BokSegmentuPx & " px (istniejące: " &
                       s.Zrodlo.LiczbaSegmentow & "), piksel " & c.RozmiarPiksela & If(s.Uklad.Geograficzny, "°", " m") & ", układ " & s.Uklad.Nazwa & vbCrLf &
                       "Mapa: " & s.Opis & vbCrLf &
                       If(zrodlo <> "", "Źródło: " & zrodlo & ", ", "") & "pobrano: " & If(c.DataPobrania <> "", c.DataPobrania, "data nieznana")

        'mapa pobrana z przezroczystym tłem - domyślnie eksport jako nakładka
        chkNakladka.Checked = c.Przezroczystosc AndAlso Wms.CzyFormatZPrzezroczystoscia(c.Format)
        chkBialePodstawa.Checked = False

        'poziomy powiększenia kafli Web Mercator: najwyższy - pełna szczegółowość segmentów
        Dim zmax As Integer = PoziomNaturalny()
        nudPoziomMax.Value = Math.Min(nudPoziomMax.Maximum, zmax)
        nudPoziomMin.Value = Math.Max(nudPoziomMin.Minimum, Math.Min(zmax, Math.Max(8, zmax - 6)))
        If File.Exists(_folder & "error.txt") Then
            Komunikat("W folderze jest plik error.txt - zestaw segmentów jest niekompletny (brakujące fragmenty pozostaną puste).", Color.Red)
        ElseIf Korytarz.ZFolderu(_folder) IsNot Nothing Then
            Komunikat("Segmenty pobrane wzdłuż trasy (korytarz) - obszar poza korytarzem pozostanie pusty (przezroczysty w kaflach PNG).", Color.Black)
        Else
            Komunikat("Segmenty gotowe do eksportu.", Color.Black)
        End If
    End Sub

    ''' <summary>Najniższy poziom powiększenia z pełną szczegółowością najdokładniejszej z warstw.</summary>
    Private Function PoziomNaturalny() As Integer
        Dim z = UtworzZadanie()
        Dim srodek = _sesja.Uklad.DoWgs84(_sesja.Siatka.ZasiegSiatki.Srodek)
        Dim wynik As Integer = 0
        For Each w In z.WarstwyRenderowania()
            wynik = Math.Max(wynik, WebMercator.PoziomDlaRozdzielczosci(EksportMapy.PikselWMetrach(w.Zrodlo, w.Uklad), srodek.Szerokosc))
        Next
        Return wynik
    End Function

    Private ReadOnly Property WybranyRodzaj As RodzajEksportu
        Get
            If rbMBTiles.Checked Then Return RodzajEksportu.MBTiles
            If rbSQLiteDB.Checked Then Return RodzajEksportu.SQLiteDB
            If rbOruxMaps.Checked Then Return RodzajEksportu.OruxMaps
            If rbKmz.Checked Then Return RodzajEksportu.Kmz
            Return RodzajEksportu.KmzGarmin
        End Get
    End Property

    ''' <summary>Opis mapy: tytuł, data pobrania, warstwy i źródła danych.</summary>
    Private Function UtworzMetadane() As MetadaneMapy
        Dim m As New MetadaneMapy With {.DataPobrania = MetadaneMapy.DataZTekstu(_sesja.Konfiguracja.DataPobrania)}
        Dim tytuly As New List(Of String) From {_sesja.Opis}
        m.Warstwy.Add(_sesja.Opis)
        m.DodajZrodlo(MetadaneMapy.ZrodloDanych(_sesja.AdresSerwera))
        For Each p In lstNakladki.Items.Cast(Of PozycjaNakladki)()
            tytuly.Add(p.Warstwa.Opis)
            m.Warstwy.Add(p.Warstwa.Opis)
            m.DodajZrodlo(MetadaneMapy.ZrodloDanych(p.Warstwa.AdresSerwera))
        Next
        m.Tytul = String.Join(" + ", tytuly)
        Return m
    End Function

    Private Function UtworzZadanie() As ZadanieEksportu
        Dim rodzaj As RodzajEksportu = WybranyRodzaj
        Dim nazwa As String = "_eksport_" & _sesja.Siatka.LiczbaKolumn & "x" & _sesja.Siatka.LiczbaWierszy
        Dim rozszerzenie As String
        Select Case rodzaj
            Case RodzajEksportu.MBTiles : rozszerzenie = ".mbtiles"
            Case RodzajEksportu.SQLiteDB : rozszerzenie = ".sqlitedb"
            Case RodzajEksportu.OruxMaps : rozszerzenie = "_oruxmaps"
            Case RodzajEksportu.KmzGarmin : rozszerzenie = "_garmin.kmz"
            Case Else : rozszerzenie = ".kmz"
        End Select
        _sesja.Zrodlo.BialePrzezroczyste = chkBialePodstawa.Checked
        Dim z As New ZadanieEksportu With {
            .Rodzaj = rodzaj, .Zrodlo = _sesja.Zrodlo, .Uklad = _sesja.Uklad, .Plik = _folder & nazwa & rozszerzenie,
            .JakoscJpeg = CInt(nudJakosc.Value), .MaksKafli = CInt(nudMaksKafli.Value),
            .PoziomMin = CInt(Math.Min(nudPoziomMin.Value, nudPoziomMax.Value)), .PoziomMax = CInt(nudPoziomMax.Value), .KaflePng = chkPng.Checked,
            .Nakladka = chkNakladka.Checked AndAlso rodzaj <> RodzajEksportu.KmzGarmin, .Metadane = UtworzMetadane()}
        z.Nazwa = z.Metadane.Nazwa
        For Each p In lstNakladki.Items.Cast(Of PozycjaNakladki)()
            z.Nakladki.Add(p.Warstwa)
        Next
        Return z
    End Function

    ''' <summary>Dostępność opcji i podsumowanie planu eksportu.</summary>
    Private Sub UstawWidok()
        Dim wolny As Boolean = _przerwanie Is Nothing
        Dim rodzaj = WybranyRodzaj
        Dim piramida As Boolean = rodzaj = RodzajEksportu.MBTiles OrElse rodzaj = RodzajEksportu.SQLiteDB OrElse rodzaj = RodzajEksportu.OruxMaps
        grpFormat.Enabled = wolny
        grpWarstwy.Enabled = wolny
        grpRozmiar.Enabled = wolny AndAlso _sesja IsNot Nothing
        nudJakosc.Enabled = wolny
        btnFolder.Enabled = wolny
        nudMaksKafli.Enabled = rbKmzGarmin.Checked
        grpMBTiles.Enabled = wolny AndAlso piramida
        chkNakladka.Enabled = rodzaj <> RodzajEksportu.KmzGarmin
        chkPng.Enabled = Not (chkNakladka.Checked AndAlso chkNakladka.Enabled)
        btnDopasuj.Enabled = piramida
        nudLimit.Enabled = piramida
        Dim zaznaczona = TryCast(lstNakladki.SelectedItem, PozycjaNakladki)
        btnUsunNakladke.Enabled = zaznaczona IsNot Nothing
        nudKrycie.Enabled = zaznaczona IsNot Nothing
        chkBialeNakladka.Enabled = zaznaczona IsNot Nothing
        btnEksportuj.Enabled = _sesja IsNot Nothing AndAlso wolny
        lblPlan.Text = ""
        If _sesja Is Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        If z.Piramida Then
            lblPlan.Text = "Liczba kafli: do " & EksportMapy.LiczbaKafliMBTiles(z).ToString("N0") & " (poziomy " & z.PoziomMin & "-" & z.PoziomMax &
                           "), kafle " & If(z.Png, "PNG", "JPEG")
        Else
            Dim plan = EksportMapy.PlanKmz(z)
            lblPlan.Text = "Kafle: " & plan.Kolumny & " x " & plan.Wiersze & " = " & plan.LiczbaKafli &
                           If(plan.Pomniejszenie > 1.001, ", rozdzielczość zmniejszona " & plan.Pomniejszenie.ToString("0.0") & "x (limit kafli)", ", pełna rozdzielczość")
        End If
    End Sub

    Private Sub OpcjeZmienione(sender As Object, e As EventArgs) Handles rbKmzGarmin.CheckedChanged, rbKmz.CheckedChanged, rbMBTiles.CheckedChanged,
                                                                    rbSQLiteDB.CheckedChanged, rbOruxMaps.CheckedChanged, nudMaksKafli.ValueChanged,
                                                                    nudPoziomMin.ValueChanged, nudPoziomMax.ValueChanged, chkPng.CheckedChanged,
                                                                    chkNakladka.CheckedChanged, chkBialePodstawa.CheckedChanged, nudJakosc.ValueChanged
        lblRozmiar.Text = ""
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

#Region "Warstwy nakładane"

    Private Sub btnDodajNakladke_Click(sender As Object, e As EventArgs) Handles btnDodajNakladke.Click
        If _sesja Is Nothing Then Exit Sub
        dlgFolder.SelectedPath = _folder
        If dlgFolder.ShowDialog <> DialogResult.OK Then Exit Sub
        Dim folder As String = dlgFolder.SelectedPath & "\"
        If String.Equals(folder, _folder, StringComparison.OrdinalIgnoreCase) Then
            MsgBox("Wskaż folder innej sesji pobierania niż folder mapy podstawowej.", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        Dim blad As String = ""
        Dim s = OdczytajSesje(folder, blad)
        If s Is Nothing Then
            MsgBox(blad, MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        If s.Zrodlo.LiczbaSegmentow = 0 Then
            MsgBox("W folderze nie ma pobranych segmentów.", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        'warstwa musi pokrywać choć część mapy podstawowej
        Dim a = ProstokatGeo.Opisany(_sesja.Uklad, _sesja.Siatka.ZasiegSiatki)
        Dim b = ProstokatGeo.Opisany(s.Uklad, s.Siatka.ZasiegSiatki)
        If b.Zachod >= a.Wschod OrElse b.Wschod <= a.Zachod OrElse b.Poludnie >= a.Polnoc OrElse b.Polnoc <= a.Poludnie Then
            MsgBox("Segmenty z tego folderu nie pokrywają obszaru mapy podstawowej.", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        'segmenty bez przezroczystego tła (JPEG albo PNG pobrane bez TRANSPARENT=TRUE) - białe tło domyślnie przezroczyste
        Dim c = s.Konfiguracja
        s.Zrodlo.BialePrzezroczyste = Not (Wms.CzyFormatZPrzezroczystoscia(c.Format) AndAlso c.Przezroczystosc)
        Dim p As New PozycjaNakladki With {.Sesja = s, .Warstwa = New WarstwaEksportu With {
            .Zrodlo = s.Zrodlo, .Uklad = s.Uklad, .Krycie = 1, .Opis = s.Opis, .AdresSerwera = s.AdresSerwera}}
        lstNakladki.Items.Add(p)
        lstNakladki.SelectedItem = p
        lblRozmiar.Text = ""
        UstawWidok()
    End Sub

    Private Sub btnUsunNakladke_Click(sender As Object, e As EventArgs) Handles btnUsunNakladke.Click
        If lstNakladki.SelectedIndex < 0 Then Exit Sub
        lstNakladki.Items.RemoveAt(lstNakladki.SelectedIndex)
        lblRozmiar.Text = ""
        UstawWidok()
    End Sub

    Private Sub lstNakladki_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstNakladki.SelectedIndexChanged
        Dim p = TryCast(lstNakladki.SelectedItem, PozycjaNakladki)
        If p IsNot Nothing Then
            _ustawianie = True
            Try
                nudKrycie.Value = Math.Max(nudKrycie.Minimum, Math.Min(nudKrycie.Maximum, CDec(Math.Round(p.Warstwa.Krycie * 100))))
                chkBialeNakladka.Checked = p.Warstwa.Zrodlo.BialePrzezroczyste
            Finally
                _ustawianie = False
            End Try
        End If
        UstawWidok()
    End Sub

    Private Sub WarstwaZmieniona(sender As Object, e As EventArgs) Handles nudKrycie.ValueChanged, chkBialeNakladka.CheckedChanged
        If _ustawianie Then Exit Sub
        Dim i As Integer = lstNakladki.SelectedIndex
        Dim p = TryCast(lstNakladki.SelectedItem, PozycjaNakladki)
        If p Is Nothing Then Exit Sub
        p.Warstwa.Krycie = nudKrycie.Value / 100D
        p.Warstwa.Zrodlo.BialePrzezroczyste = chkBialeNakladka.Checked
        'odświeżenie opisu pozycji listy
        _ustawianie = True
        Try
            lstNakladki.Items(i) = p
            lstNakladki.SelectedIndex = i
        Finally
            _ustawianie = False
        End Try
        lblRozmiar.Text = ""
    End Sub

#End Region

#Region "Rozmiar pliku"

    ''' <summary>Blokuje opcje na czas pracy w tle; zwraca token przerwania.</summary>
    Private Function RozpocznijPrace() As CancellationToken
        _przerwanie = New CancellationTokenSource()
        btnPrzerwij.Enabled = True
        UstawWidok()
        Return _przerwanie.Token
    End Function

    Private Sub ZakonczPrace()
        If _przerwanie IsNot Nothing Then
            _przerwanie.Dispose()
            _przerwanie = Nothing
        End If
        If Not Me.IsDisposed AndAlso Not Me.Disposing Then
            btnPrzerwij.Enabled = False
            UstawWidok()
        End If
    End Sub

    Private Async Sub btnOszacuj_Click(sender As Object, e As EventArgs) Handles btnOszacuj.Click
        If _sesja Is Nothing OrElse _przerwanie IsNot Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        Dim token = RozpocznijPrace()
        Komunikat("Trwa szacowanie rozmiaru pliku (renderowanie próbnych kafli)...", Color.Black)
        Try
            Dim s = Await Task.Run(Function() EksportMapy.SzacujRozmiar(z, z.PoziomMax, token))
            If Me.IsDisposed OrElse Me.Disposing Then Exit Sub
            PokazSzacunek(s, z)
        Catch ex As OperationCanceledException
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Przerwano szacowanie rozmiaru", Color.Red)
        Catch ex As Exception
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Nie udało się oszacować rozmiaru: " & ex.Message, Color.Red)
        Finally
            ZakonczPrace()
        End Try
    End Sub

    Private Sub PokazSzacunek(s As SzacunekRozmiaru, z As ZadanieEksportu)
        If s.Probki = 0 Then
            lblRozmiar.Text = "brak danych do oszacowania"
            Komunikat("W obszarze mapy nie znaleziono segmentów z danymi.", Color.Red)
            Exit Sub
        End If
        lblRozmiar.Text = "ok. " & SzacunekRozmiaru.Tekst(s.Bajty) & " (" & s.LiczbaKafli.ToString("N0") & " kafli)"
        If s.Poziomy.Count > 0 Then
            Komunikat("Szacowany rozmiar poziomów: " & String.Join("; ", s.Poziomy.Select(Function(p) p.Key & ": " & SzacunekRozmiaru.Tekst(p.Value))) &
                      ". Szacunek na podstawie " & s.Probki & " próbnych kafli - rzeczywisty rozmiar może się różnić o kilkadziesiąt procent.", Color.Black)
        Else
            Komunikat("Szacunek na podstawie " & s.Probki & " próbnych kafli - rzeczywisty rozmiar może się różnić o kilkadziesiąt procent.", Color.Black)
        End If
    End Sub

    ''' <summary>Najwyższy poziom powiększenia, przy którym mapa mieści się w limicie rozmiaru.</summary>
    Private Async Sub btnDopasuj_Click(sender As Object, e As EventArgs) Handles btnDopasuj.Click
        If _sesja Is Nothing OrElse _przerwanie IsNot Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        If Not z.Piramida Then Exit Sub
        Dim gora As Integer = Math.Max(z.PoziomMin, Math.Min(CInt(nudPoziomMax.Maximum), PoziomNaturalny()))
        Dim limit As Long = CLng(nudLimit.Value) * 1024L * 1024L
        Dim token = RozpocznijPrace()
        Komunikat("Trwa szacowanie rozmiaru poziomów " & z.PoziomMin & "-" & gora & "...", Color.Black)
        Try
            Dim s = Await Task.Run(Function() EksportMapy.SzacujRozmiar(z, gora, token))
            If Me.IsDisposed OrElse Me.Disposing Then Exit Sub
            If s.Probki = 0 Then
                Komunikat("W obszarze mapy nie znaleziono segmentów z danymi.", Color.Red)
                Exit Sub
            End If
            Dim poziom As Integer = EksportMapy.PoziomDlaLimitu(s, limit)
            If poziom < 0 Then
                Komunikat("Już sam poziom " & z.PoziomMin & " przekracza limit (" & SzacunekRozmiaru.Tekst(s.Poziomy.First().Value) &
                          ") - zwiększ limit albo obniż najniższy poziom powiększenia.", Color.Red)
                Exit Sub
            End If
            nudPoziomMax.Value = poziom
            Dim bajty As Long = s.Poziomy.Where(Function(p) p.Key <= poziom).Sum(Function(p) p.Value)
            lblRozmiar.Text = "ok. " & SzacunekRozmiaru.Tekst(bajty)
            Komunikat("Najwyższy poziom powiększenia mieszczący się w limicie " & nudLimit.Value & " MB: " & poziom &
                      If(poziom >= gora, " (pełna szczegółowość segmentów)", " (pełna szczegółowość segmentów to poziom " & gora & ")") &
                      ". Szacowany rozmiar: " & SzacunekRozmiaru.Tekst(bajty) & ".", Color.Green)
        Catch ex As OperationCanceledException
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Przerwano szacowanie rozmiaru", Color.Red)
        Catch ex As Exception
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Nie udało się oszacować rozmiaru: " & ex.Message, Color.Red)
        Finally
            ZakonczPrace()
        End Try
    End Sub

#End Region

    Private Async Sub btnEksportuj_Click(sender As Object, e As EventArgs) Handles btnEksportuj.Click
        If _sesja Is Nothing OrElse _przerwanie IsNot Nothing Then Exit Sub
        Dim z = UtworzZadanie()
        If z.Piramida AndAlso EksportMapy.LiczbaKafliMBTiles(z) > 2000000 Then
            If MessageBox.Show("Eksport obejmie ponad 2 mln kafli i może trwać bardzo długo. Kontynuować?", "Eksport kafli",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        End If

        Dim token = RozpocznijPrace()
        prgEksport.Value = 0
        Komunikat("Trwa eksport do " & Path.GetFileName(z.Plik) & "...", Color.Black)

        Dim postep As New Progress(Of Integer)(Sub(p)
                                                   If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                                                       prgEksport.Value = Math.Max(0, Math.Min(100, p))
                                                   End If
                                               End Sub)
        Try
            Await Task.Run(Sub() EksportMapy.Eksportuj(z, postep, token))
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                prgEksport.Value = 100
                If z.Rodzaj = RodzajEksportu.OruxMaps Then
                    Komunikat("Zapisano mapę OruxMaps w folderze " & z.Plik & ". Skopiuj cały ten folder do folderu oruxmaps/mapfiles w telefonie.", Color.Green)
                Else
                    Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                    Komunikat("Zapisano plik " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & ")", Color.Green)
                End If
            End If
            Form1.Komunikat("Wyeksportowano mapę do " & Path.GetFileName(z.Plik), Color.Green)
        Catch ex As OperationCanceledException
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                Komunikat("Przerwano eksport", Color.Red)
            End If
        Catch ex As Exception
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                Komunikat("Błąd eksportu: " & ex.Message, Color.Red)
            End If
        Finally
            ZakonczPrace()
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
