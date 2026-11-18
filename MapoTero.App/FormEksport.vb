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
    Private _ostatniPlikEksportu As String = ""
    Private _opcjeMip As New OpcjeKorekcjiMip()
    ''' <summary>Programowe ustawianie kontrolek warstwy (bez reakcji na zdarzenia).</summary>
    Private _ustawianie As Boolean

    ''' <summary>Rozmiar obszaru roboczego formularza w projekcie (jednostki projektanta, przed skalowaniem czcionki).</summary>
    Private Const ProjektSzerokoscKlienta As Integer = 510
    Private Const ProjektWysokoscKlienta As Integer = 788

    ''' <summary>
    ''' Rozmiar obszaru roboczego po skalowaniu okna do bieżącej czcionki. Dawniej używano wprost wartości z projektu,
    ''' a okno było już powiększone (Segoe UI 9: ok. 595 x 909 px przy 100% DPI) - ustawienie 510 x 788 obcinało
    ''' prawą i dolną część okna razem z przyciskami.
    ''' </summary>
    Private ReadOnly Property BazowaSzerokoscKlienta As Integer
        Get
            Return CInt(Math.Round(ProjektSzerokoscKlienta * PomocProgramu.SkalaProjektanta(Me).Width))
        End Get
    End Property

    Private ReadOnly Property BazowaWysokoscKlienta As Integer
        Get
            Return CInt(Math.Round(ProjektWysokoscKlienta * PomocProgramu.SkalaProjektanta(Me).Height))
        End Get
    End Property

    Private Sub FormEksport_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.ZamykanieKlawiszemEsc(Me)
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.Eksport, 12, 748)
        DopasujDoWysokosciEkranu()
        If cboKolejnoscGarmin.Items.Count > 0 AndAlso cboKolejnoscGarmin.SelectedIndex < 0 Then
            cboKolejnoscGarmin.SelectedIndex = 0
        End If
        Dim uwagaFolderu As String = ""
        _folder = FolderSesjiDlaNarzedzia(Form1.FolderSesji, uwagaFolderu)
        WczytajSesje()
        UsunNiepasujaceNakladki()
        'UstawWgProfilu włącza korektę, więc dawniej po odczycie stanu „wyłączona” korekta znów była zaznaczona
        _opcjeMip = Ustawienia.OpcjeKorekty()
        chkKorekcjaMip.Checked = _opcjeMip.Wlaczona
        AktualizujPrzyciskMip()
        UstawWidok()
        If uwagaFolderu <> "" Then Komunikat(uwagaFolderu & vbCrLf & txtKomunikaty.Text, Color.OrangeRed)
    End Sub

    ''' <summary>
    ''' Dopasowuje wysokość okna eksportu do wysokości ekranu roboczego (ekrany o niskiej rozdzielczości lub
    ''' przy skalowaniu DPI 125-150%). Gdy ekran jest za niski, włącza pionowy pasek przewijania (AutoScroll)
    ''' i poszerza okno o szerokość paska, aby zawartość i przyciski były w pełni dostępne.
    ''' </summary>
    Public Sub DopasujDoWysokosciEkranu()
        Dim ekran As Rectangle = Screen.FromControl(Me).WorkingArea
        Dim ramkaPionowa As Integer = Math.Max(0, Me.Height - Me.ClientSize.Height)
        Dim wymaganaWysokoscOkna As Integer = BazowaWysokoscKlienta + ramkaPionowa
        Dim maksDozwolonaWysokoscOkna As Integer = Math.Max(320, ekran.Height - 30)

        If wymaganaWysokoscOkna > maksDozwolonaWysokoscOkna Then
            Me.AutoScroll = True
            Me.AutoScrollMinSize = New Size(0, BazowaWysokoscKlienta)
            Dim nowaWysokoscKlienta As Integer = Math.Max(260, maksDozwolonaWysokoscOkna - ramkaPionowa)
            Dim pasek As Integer = SystemInformation.VerticalScrollBarWidth
            Me.ClientSize = New Size(BazowaSzerokoscKlienta + pasek, nowaWysokoscKlienta)
        Else
            Me.AutoScroll = False
            Me.AutoScrollMinSize = Size.Empty
            Me.ClientSize = New Size(BazowaSzerokoscKlienta, BazowaWysokoscKlienta)
        End If

        Me.StartPosition = FormStartPosition.Manual
        Me.Top = Math.Max(ekran.Top + 5, ekran.Top + (ekran.Height - Me.Height) \ 2)
        Me.Left = Math.Max(ekran.Left + 5, ekran.Left + (ekran.Width - Me.Width) \ 2)
        Me.AutoScrollPosition = New Point(0, 0)
    End Sub

    ''' <summary>
    ''' Usuwa z listy nakładki, które nie pasują do bieżącej mapy podstawowej: folder mapy podstawowej albo segmenty poza
    ''' jej obszarem. Zamknięte okno modalne nie jest niszczone, więc nakładki zostawały z poprzedniego otwarcia okna,
    ''' także po zmianie mapy podstawowej.
    ''' </summary>
    Private Sub UsunNiepasujaceNakladki()
        For i = lstNakladki.Items.Count - 1 To 0 Step -1
            Dim p = TryCast(lstNakladki.Items(i), PozycjaNakladki)
            If p Is Nothing OrElse _sesja Is Nothing OrElse String.Equals(p.Sesja.Folder, _folder, StringComparison.OrdinalIgnoreCase) OrElse
               Not Pokrywa(_sesja, p.Sesja) Then
                lstNakladki.Items.RemoveAt(i)
            End If
        Next
    End Sub

    ''' <summary>Czy segmenty sesji b pokrywają choć część obszaru sesji a.</summary>
    Private Shared Function Pokrywa(a As Sesja, b As Sesja) As Boolean
        Dim ga = ProstokatGeo.Opisany(a.Uklad, a.Siatka.ZasiegSiatki)
        Dim gb = ProstokatGeo.Opisany(b.Uklad, b.Siatka.ZasiegSiatki)
        Return Not (gb.Zachod >= ga.Wschod OrElse gb.Wschod <= ga.Zachod OrElse gb.Poludnie >= ga.Polnoc OrElse gb.Polnoc <= ga.Poludnie)
    End Function

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
            blad = "Nie udało się odczytać pliku conf.txt: " & OpisBledu.Z(ex)
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

        Dim zrodlo As String = MetadaneMapy.ZrodloDanych(s.AdresSerwera, Form1.AtrybucjaZbioru(c.RodzajMapy, s.AdresSerwera))
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
            If rbJnx.Checked Then Return RodzajEksportu.Jnx
            If rbMBTiles.Checked Then Return RodzajEksportu.MBTiles
            If rbSQLiteDB.Checked Then Return RodzajEksportu.SQLiteDB
            If rbOruxMaps.Checked Then Return RodzajEksportu.OruxMaps
            If rbKmz.Checked Then Return RodzajEksportu.Kmz
            If rbGeoTiff.Checked Then Return RodzajEksportu.GeoTiff
            Return RodzajEksportu.KmzGarmin
        End Get
    End Property

    ''' <summary>Opis mapy: tytuł, data pobrania, warstwy i źródła danych.</summary>
    Private Function UtworzMetadane() As MetadaneMapy
        Dim m As New MetadaneMapy With {.DataPobrania = MetadaneMapy.DataZTekstu(_sesja.Konfiguracja.DataPobrania)}
        Dim tytuly As New List(Of String) From {_sesja.Opis}
        m.Warstwy.Add(_sesja.Opis)
        m.DodajZrodlo(MetadaneMapy.ZrodloDanych(_sesja.AdresSerwera, Form1.AtrybucjaZbioru(_sesja.Konfiguracja.RodzajMapy, _sesja.AdresSerwera)))
        For Each p In lstNakladki.Items.Cast(Of PozycjaNakladki)()
            tytuly.Add(p.Warstwa.Opis)
            m.Warstwy.Add(p.Warstwa.Opis)
            m.DodajZrodlo(MetadaneMapy.ZrodloDanych(p.Warstwa.AdresSerwera, Form1.AtrybucjaZbioru("", p.Warstwa.AdresSerwera)))
        Next
        m.Tytul = String.Join(" + ", tytuly)
        Return m
    End Function

    Private Function UtworzZadanie() As ZadanieEksportu
        Dim rodzaj As RodzajEksportu = WybranyRodzaj
        Dim nazwa As String = "_eksport_" & _sesja.Siatka.LiczbaKolumn & "x" & _sesja.Siatka.LiczbaWierszy
        Dim rozszerzenie As String
        Select Case rodzaj
            Case RodzajEksportu.Jnx : rozszerzenie = ".jnx"
            Case RodzajEksportu.MBTiles : rozszerzenie = ".mbtiles"
            Case RodzajEksportu.SQLiteDB : rozszerzenie = ".sqlitedb"
            Case RodzajEksportu.OruxMaps : rozszerzenie = "_oruxmaps"
            Case RodzajEksportu.GeoTiff : rozszerzenie = ".tif"
            Case RodzajEksportu.KmzGarmin : rozszerzenie = "_garmin.kmz"
            Case Else : rozszerzenie = ".kmz"
        End Select
        _sesja.Zrodlo.BialePrzezroczyste = chkBialePodstawa.Checked
        Dim kOrder As Integer = 50
        If cboKolejnoscGarmin.SelectedIndex = 1 Then
            kOrder = 25
        ElseIf cboKolejnoscGarmin.SelectedIndex = 2 Then
            kOrder = 75
        End If
        Dim z As New ZadanieEksportu With {
            .Rodzaj = rodzaj, .Zrodlo = _sesja.Zrodlo, .Uklad = _sesja.Uklad, .Plik = _folder & nazwa & rozszerzenie,
            .JakoscJpeg = CInt(nudJakosc.Value), .MaksKafli = CInt(nudMaksKafli.Value), .PodzialNaTomy = chkPodzialNaTomy.Checked AndAlso rodzaj = RodzajEksportu.KmzGarmin,
            .KolejnoscRysowania = kOrder,
            .PoziomMin = CInt(Math.Min(nudPoziomMin.Value, nudPoziomMax.Value)), .PoziomMax = CInt(nudPoziomMax.Value), .KaflePng = chkPng.Checked,
            .Nakladka = chkNakladka.Checked AndAlso rodzaj <> RodzajEksportu.Jnx, .Metadane = UtworzMetadane()}
        Dim mip = _opcjeMip.Kopia()
        mip.Wlaczona = chkKorekcjaMip.Checked
        z.KorekcjaMip = mip
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
        Dim piramida As Boolean = rodzaj = RodzajEksportu.MBTiles OrElse rodzaj = RodzajEksportu.SQLiteDB OrElse rodzaj = RodzajEksportu.OruxMaps OrElse rodzaj = RodzajEksportu.Jnx
        Dim garmin As Boolean = rodzaj = RodzajEksportu.KmzGarmin OrElse rodzaj = RodzajEksportu.Jnx
        Dim geotiff As Boolean = rodzaj = RodzajEksportu.GeoTiff
        chkGeoTiffJpeg.Enabled = wolny AndAlso geotiff
        chkGeoTiffCad.Enabled = wolny AndAlso geotiff
        chkGeoTiffJpeg.Visible = geotiff
        chkGeoTiffCad.Visible = geotiff
        chkKorekcjaMip.Enabled = wolny
        btnPodgladMip.Enabled = wolny
        grpFormat.Enabled = wolny
        grpWarstwy.Enabled = wolny
        grpRozmiar.Enabled = wolny AndAlso _sesja IsNot Nothing
        nudJakosc.Enabled = wolny
        btnFolder.Enabled = wolny
        nudMaksKafli.Enabled = rbKmzGarmin.Checked
        chkPodzialNaTomy.Enabled = wolny AndAlso rbKmzGarmin.Checked
        chkPodzialNaTomy.Visible = rbKmzGarmin.Checked
        lblKolejnoscGarmin.Enabled = garmin
        cboKolejnoscGarmin.Enabled = garmin AndAlso wolny
        grpMBTiles.Enabled = wolny AndAlso piramida
        chkPng.Enabled = piramida AndAlso rodzaj <> RodzajEksportu.Jnx AndAlso Not (chkNakladka.Checked AndAlso chkNakladka.Enabled)
        chkNakladka.Enabled = rodzaj <> RodzajEksportu.Jnx
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
            If z.PodzialNaTomy Then
                Dim tomy = plan.WyznaczTomy(z.MaksKafli)
                lblPlan.Text = "Kafle: " & plan.LiczbaKafli & " (podział na " & tomy.Count & " tomów po maks. " & z.MaksKafli & " kafli, pełna rozdzielczość)" &
                               If(z.Png, ", kafle PNG", ", kafle JPEG")
            Else
                lblPlan.Text = "Kafle: " & plan.Kolumny & " x " & plan.Wiersze & " = " & plan.LiczbaKafli &
                               If(plan.Pomniejszenie > 1.001, ", rozdzielczość zmniejszona " & plan.Pomniejszenie.ToString("0.0") & "x (limit kafli)", ", pełna rozdzielczość") &
                               If(z.Png, ", kafle PNG", ", kafle JPEG")
            End If
        End If
    End Sub

    Private Sub OpcjeZmienione(sender As Object, e As EventArgs) Handles rbKmzGarmin.CheckedChanged, rbKmz.CheckedChanged, rbMBTiles.CheckedChanged,
                                                                    rbSQLiteDB.CheckedChanged, rbOruxMaps.CheckedChanged, rbJnx.CheckedChanged, rbGeoTiff.CheckedChanged, chkGeoTiffJpeg.CheckedChanged, chkGeoTiffCad.CheckedChanged, chkKorekcjaMip.CheckedChanged,
                                                                    cboKolejnoscGarmin.SelectedIndexChanged, chkPodzialNaTomy.CheckedChanged, nudMaksKafli.ValueChanged,
                                                                    nudPoziomMin.ValueChanged, nudPoziomMax.ValueChanged, chkPng.CheckedChanged,
                                                                    chkNakladka.CheckedChanged, chkBialePodstawa.CheckedChanged, nudJakosc.ValueChanged
        If chkKorekcjaMip.Checked <> _opcjeMip.Wlaczona Then
            _opcjeMip.Wlaczona = chkKorekcjaMip.Checked
            If chkKorekcjaMip.Checked AndAlso _opcjeMip.Profil = ProfilKorektyRastra.Brak Then
                _opcjeMip.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
            End If
            AktualizujPrzyciskMip()
        End If
        lblRozmiar.Text = ""
        UstawWidok()
    End Sub

    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        dlgFolder.SelectedPath = _folder
        If dlgFolder.ShowDialog = DialogResult.OK Then
            _folder = dlgFolder.SelectedPath & "\"
            WczytajSesje()
            UsunNiepasujaceNakladki()
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
            MsgBox("Wskaż folder innej sesji pobierania niż folder mapy podstawowej.", MsgBoxStyle.Exclamation, "Eksport mapy")
            Exit Sub
        End If
        Dim blad As String = ""
        Dim s = OdczytajSesje(folder, blad)
        If s Is Nothing Then
            MsgBox(blad, MsgBoxStyle.Exclamation, "Eksport mapy")
            Exit Sub
        End If
        If s.Zrodlo.LiczbaSegmentow = 0 Then
            MsgBox("W folderze nie ma pobranych segmentów.", MsgBoxStyle.Exclamation, "Eksport mapy")
            Exit Sub
        End If
        'warstwa musi pokrywać choć część mapy podstawowej
        If Not Pokrywa(_sesja, s) Then
            MsgBox("Segmenty z tego folderu nie pokrywają obszaru mapy podstawowej.", MsgBoxStyle.Exclamation, "Eksport mapy")
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
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Nie udało się oszacować rozmiaru: " & OpisBledu.Z(ex), Color.Red)
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
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then Komunikat("Nie udało się oszacować rozmiaru: " & OpisBledu.Z(ex), Color.Red)
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
        _trwaEksport = True
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
                _ostatniPlikEksportu = z.Plik
                btnPokazPlik.Enabled = True
                Select Case z.Rodzaj
                    Case RodzajEksportu.GeoTiff
                        Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                        Komunikat("Zapisano plik GeoTIFF: " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & "). Plik z georeferencją gotowy do użycia w Avenza Maps, QField, QGIS lub programach CAD.", Color.Green)
                    Case RodzajEksportu.Jnx
                        Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                        Komunikat("Zapisano mapę BirdsEye: " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & "). Skopiuj plik .jnx do folderu Garmin/BirdsEye w urządzeniu lub na karcie SD.", Color.Green)
                    Case RodzajEksportu.KmzGarmin
                        If z.PodzialNaTomy AndAlso Not File.Exists(z.Plik) Then
                            Komunikat("Zapisano tomy mapy Garmin Custom Maps w folderze " & Path.GetDirectoryName(z.Plik) & ". W menu odbiornika włączaj pojedyncze tomy w zależności od rejonu wędrówki.", Color.Green)
                        Else
                            Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                            Komunikat("Zapisano mapę Garmin Custom Maps: " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & "). Skopiuj plik .kmz do folderu Garmin/CustomMaps w urządzeniu lub na karcie SD.", Color.Green)
                        End If
                    Case RodzajEksportu.MBTiles
                        Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                        Komunikat("Zapisano plik MBTiles: " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & "). Locus Map: skopiuj do Locus/maps; OsmAnd/Orux: wskaż plik w menedżerze map.", Color.Green)
                    Case RodzajEksportu.SQLiteDB
                        Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                        Komunikat("Zapisano plik SQLiteDB: " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & "). OsmAnd: skopiuj do osmand/tiles; Locus Map: do Locus/maps.", Color.Green)
                    Case RodzajEksportu.OruxMaps
                        Komunikat("Zapisano mapę OruxMaps w folderze " & z.Plik & ". Skopiuj cały ten folder do folderu oruxmaps/mapfiles w telefonie.", Color.Green)
                    Case Else
                        Dim rozmiar As Long = If(File.Exists(z.Plik), New FileInfo(z.Plik).Length, 0)
                        Komunikat("Zapisano plik " & z.Plik & " (" & SzacunekRozmiaru.Tekst(rozmiar) & ")", Color.Green)
                End Select
            End If
            Form1.Komunikat("Wyeksportowano mapę do " & Path.GetFileName(z.Plik), Color.Green)
        Catch ex As OperationCanceledException
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                Komunikat("Przerwano eksport", Color.Red)
            End If
        Catch ex As Exception
            If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                Komunikat("Błąd eksportu: " & OpisBledu.Z(ex), Color.Red)
            End If
        Finally
            _trwaEksport = False
            ZakonczPrace()
        End Try
    End Sub

    Private Sub btnPokazPlik_Click(sender As Object, e As EventArgs) Handles btnPokazPlik.Click
        If Not String.IsNullOrEmpty(_ostatniPlikEksportu) Then
            Try
                If Directory.Exists(_ostatniPlikEksportu) Then
                    Process.Start(New ProcessStartInfo("explorer.exe", """" & _ostatniPlikEksportu & """") With {.UseShellExecute = True})
                ElseIf File.Exists(_ostatniPlikEksportu) Then
                    Process.Start(New ProcessStartInfo("explorer.exe", "/select,""" & _ostatniPlikEksportu & """") With {.UseShellExecute = True})
                End If
            Catch ex As Exception
                MsgBox("Nie udało się otworzyć folderu: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Eksport mapy")
            End Try
        End If
    End Sub

    Private Sub btnPodgladMip_Click(sender As Object, e As EventArgs) Handles btnPodgladMip.Click
        Dim probka As String = ""
        If _sesja IsNot Nothing AndAlso Directory.Exists(_folder) Then
            Dim k = _sesja.Konfiguracja
            Dim wzorzec = If(String.IsNullOrEmpty(k.Prefiks), "*", k.Prefiks & "*") & "." & Wms.RozszerzeniePliku(k.Format)
            Dim pliki = Directory.GetFiles(_folder, wzorzec)
            'segment ze środka listy - pierwszy (narożny) bywa pusty lub częściowo poza mapą
            If pliki.Length > 0 Then probka = pliki.OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase).ElementAt(pliki.Length \ 2)
        End If
        Using dlg As New FormPodgladMip(_opcjeMip, probka)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _opcjeMip = dlg.Opcje
                chkKorekcjaMip.Checked = _opcjeMip.Wlaczona
                AktualizujPrzyciskMip()
            End If
        End Using
    End Sub

    Private Sub AktualizujPrzyciskMip()
        If _opcjeMip IsNot Nothing AndAlso _opcjeMip.Wlaczona Then
            btnPodgladMip.Text = "Profil: " & OpcjeKorekcjiMip.KrotkaNazwaProfilu(_opcjeMip.Profil) & "..."
        Else
            btnPodgladMip.Text = "Profil / Podgląd..."
        End If
    End Sub

    Private Sub btnWielowarstwowyMbtiles_Click(sender As Object, e As EventArgs) Handles btnWielowarstwowyMbtiles.Click
        FormZaawansowanyMbtiles.ShowDialog(Me)
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        btnPrzerwij.Enabled = False
        _przerwanie?.Cancel()
    End Sub

    ''' <summary>Trwa eksport (nie szacowanie rozmiaru) - zamknięcie okna wymaga potwierdzenia.</summary>
    Private _trwaEksport As Boolean

    Private Sub FormEksport_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        'Esc albo krzyżyk przerywały dawniej bez pytania eksport trwający nawet godzinę (okna scalania i MBTiles pytają)
        If _trwaEksport AndAlso _przerwanie IsNot Nothing AndAlso e.CloseReason = CloseReason.UserClosing Then
            If MessageBox.Show("Trwa eksport mapy. Przerwać go i zamknąć okno? Niedokończony plik zostanie usunięty.", "Zamykanie okna",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
                e.Cancel = True
                Exit Sub
            End If
        End If
        _przerwanie?.Cancel()
    End Sub

    Private Sub FormEksport_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Ustawienia.ZapamietajKorekte(_opcjeMip)
    End Sub

End Class
