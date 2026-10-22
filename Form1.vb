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
Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers
Imports GMap.NET.WindowsForms.ToolTips
Imports MapoTero.Core

''' <summary>Główne okno programu.</summary>
Public Class Form1

    ''' <summary>Wskazuje, że okno zostało już załadowane (wczytanie sesji z menu pyta wtedy o folder).</summary>
    Private _zaladowany As Boolean

    ''' <summary>Wybrane warstwy mapy (maksymalnie 12).</summary>
    Private ReadOnly _warstwy As New List(Of String)
    Private Const MaksLiczbaWarstw As Integer = 12

    ''' <summary>Bieżący zbiór map (adres serwera i lista warstw).</summary>
    Private _plikWarstw As New PlikWarstw()

    ''' <summary>Przerywanie trwającego pobierania.</summary>
    Private _przerwanie As CancellationTokenSource

    ''' <summary>Programowe ustawianie listy układów (bez przeliczania zasięgu).</summary>
    Private _ustawianieUkladu As Boolean

    ''' <summary>Szerokość prawego panelu (bez paska przewijania) i jego najmniejsza wysokość, przy której nie jest przewijany.</summary>
    Private _szerokoscPanelu As Integer
    Private _minWysokoscPanelu As Integer

    ''' <summary>Liczba wierszy listy warstw WMS widocznych w najmniejszym oknie.</summary>
    Private Const MinWierszyListyWarstw As Integer = 5

    ''' <summary>Obszar pobierania wzdłuż trasy (zapisywany w folderze sesji w korytarz.txt); Nothing - cały prostokąt.</summary>
    Private _korytarz As Korytarz

    ''' <summary>Numeracja segmentów z conf.txt wczytanej sesji (pusta - brak conf.txt lub plik z bardzo starej wersji).</summary>
    Private _numeracjaSesji As String = ""

    ''' <summary>Data ostatniego pobierania sesji (rrrr-mm-dd) - z conf.txt, ustawiana na nowo przy rozpoczęciu pobierania.</summary>
    Private _dataPobrania As String = ""

    ''' <summary>Najwięcej segmentów korytarza rysowanych na mapie (przy większej liczbie - tylko linia trasy).</summary>
    Private Const MaksSegmentowNaMapie As Integer = 3000

    ''' <summary>
    ''' Najwięcej (szacunkowo) segmentów korytarza wyznaczanych od razu po zmianie parametrów - przy większej liczbie opis
    ''' podaje szacunek, a na mapie rysowana jest sama trasa. Dokładna liczba liczona jest przy rozpoczęciu pobierania.
    ''' </summary>
    Friend Const MaksSegmentowKorytarzaNaBiezaco As Double = 200000

    ''' <summary>Najwięcej segmentów jednego pobierania (wykaz segmentów, pliki koordynaty.txt i error.txt w pamięci).</summary>
    Friend Const MaksSegmentowPobierania As Long = 10000000
    Private _siatkaWidoczna As Boolean

    ''' <summary>Uchwyt siatki pod kursorem myszy (Nothing - kursor poza uchwytami).</summary>
    Private _uchwytPodKursorem As UchwytSiatki

    ''' <summary>Uchwyt przeciągany lewym przyciskiem myszy (Nothing - brak przeciągania).</summary>
    Private _przeciaganyUchwyt As UchwytSiatki

    ''' <summary>Zasięg siatki na początku przeciągania, bieżący zasięg po przyciągnięciu do segmentów i bok segmentu.</summary>
    Private _zasiegPrzedPrzeciaganiem As Zasieg
    Private _zasiegPrzeciagany As Zasieg
    Private _bokPrzeciagania As Double

    ''' <summary>Punkt chwycenia uchwytu środka (w układzie pobierania) - przesunięcie siatki liczone jest względem niego.</summary>
    Private _punktChwytu As PunktXY

    ''' <summary>Pola zasięgu XY sprzed przeciągania - przywracane po anulowaniu klawiszem Esc.</summary>
    Private _polaPrzedPrzeciaganiem As String()

    ''' <summary>Najwięcej kolumn (wierszy) siatki ustawianych uchwytem - ochrona przed przepełnieniem liczby segmentów.</summary>
    Private Const MaksKolumnUchwytem As Integer = 20000

#Region "Dostęp dla innych okien"

    ''' <summary>Folder segmentów bieżącej sesji (wyświetlany na pasku stanu).</summary>
    Public Property FolderSesji As String
        Get
            Return stFolder.Text
        End Get
        Set(value As String)
            stFolder.Text = value
            Ustawienia.FolderSegmentow = value
        End Set
    End Property

    ''' <summary>Nazwa bieżącego zbioru map.</summary>
    Public ReadOnly Property ZbiorMap As String
        Get
            Return cmbZbiorMap.Text
        End Get
    End Property

    ''' <summary>Nazwa pierwszej wybranej warstwy.</summary>
    Public ReadOnly Property PierwszaWarstwa As String
        Get
            Return If(_warstwy.Count > 0, _warstwy(0), "")
        End Get
    End Property

    ''' <summary>Czy na mapie wyświetlona jest siatka segmentów lub korytarz.</summary>
    Public ReadOnly Property SiatkaWidoczna As Boolean
        Get
            Return _siatkaWidoczna
        End Get
    End Property

    ''' <summary>Bok segmentu w pikselach (pole formularza).</summary>
    Public Property BokSegmentu As String
        Get
            Return txtBokSegmentu.Text
        End Get
        Set(value As String)
            txtBokSegmentu.Text = value
        End Set
    End Property

    ''' <summary>Rozmiar piksela (pole formularza).</summary>
    Public Property RozmiarPiksela As String
        Get
            Return txtRozmiarPiksela.Text
        End Get
        Set(value As String)
            txtRozmiarPiksela.Text = value
        End Set
    End Property

    ''' <summary>Wyświetla komunikat w polu komunikatów okna głównego.</summary>
    Public Sub Komunikat(tekst As String, kolor As Color)
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
    End Sub

    ''' <summary>Włącza lub wyłącza ręczną edycję współrzędnych zasięgu.</summary>
    Public Sub UstawEdycjeXY(wlaczona As Boolean)
        For Each c As Control In New Control() {txtXDol, txtYLewy, txtXGora, txtYPrawy, lblOpisXDol, lblOpisYLewy, lblOpisXGora, lblOpisYPrawy}
            c.Enabled = wlaczona
        Next
        'przy wpisywaniu z klawiatury narożniki siatki są zwykłymi pinezkami, inaczej - uchwytami zmiany zasięgu
        If _siatkaWidoczna Then OdswiezSiatke()
    End Sub

    ''' <summary>Pokazuje lub ukrywa współrzędne WGS84 zaznaczenia.</summary>
    Public Sub UstawWidokZaznaczeniaWgs(widoczne As Boolean)
        For Each c As Control In New Control() {lblZaznSzerGora, lblZaznSzerDol, lblZaznDlugPrawa, lblZaznDlugLewa}
            c.Visible = widoczne
        Next
    End Sub

    ''' <summary>Pokazuje lub ukrywa współrzędne kursora i środka mapy.</summary>
    Public Sub UstawWidokWspolrzednych(widoczne As Boolean)
        For Each c As Control In New Control() {lblKursorTytul, lblKursorUkladOpis, lblKursorUklad, lblSrodekSzer, lblSrodekOpisSzer,
                                                lblSrodekTytul, lblSrodekDlug, lblSrodekOpisDlug}
            c.Visible = widoczne
        Next
    End Sub

    ''' <summary>Wybiera zbiór map z listy.</summary>
    Public Sub UstawZbiorMap(nazwa As String)
        cmbZbiorMap.Text = nazwa
    End Sub

    ''' <summary>Przywraca domyślny widok mapy (cała Polska).</summary>
    Public Sub DomyslnyWidokMapy()
        WyczyscZnaczniki()
        btnZnaczniki.Enabled = True
        lblSrodekSzer.Text = "52.3"
        lblSrodekDlug.Text = "19.2"
        lblZoom.Text = "6"
        mapa.Zoom = 6
        mapa.Position = New PointLatLng(52.3, 19.2)
        mapa.Refresh()
    End Sub

    ''' <summary>Zapisuje aktualne położenie mapy w ustawieniach.</summary>
    Private Sub ZapamietajPolozenieMapy()
        Ustawienia.SrodekMapySzerokosc = lblSrodekSzer.Text
        Ustawienia.SrodekMapyDlugosc = lblSrodekDlug.Text
        Ustawienia.SkalaMapy = lblZoom.Text
    End Sub

#End Region

#Region "Uruchomienie i zamknięcie"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RejestrujKodowania()

        'wyświetla nazwę i wersję
        Me.Text = My.Application.Info.Title & " " & My.Application.Info.Version.ToString

        'bez końcowego ukośnika (w .NET 8 StartupPath go zawiera - ścieżki miały postać ...\\download\)
        FolderProgramu = Application.StartupPath
        If String.IsNullOrEmpty(FolderProgramu) Then FolderProgramu = AppContext.BaseDirectory
        FolderProgramu = FolderProgramu.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        FolderDanych = UstalFolderDanych(FolderProgramu)
        Directory.CreateDirectory(FolderDownload)
        Ustawienia.FolderSegmentow = FolderDownload
        FolderSesji = FolderDownload

        'serwery WMS (np. Geoportal) wymagają TLS 1.2 - na starszych wersjach Windows nie jest on domyślnie włączony;
        'domyślnie .NET Framework ogranicza też liczbę jednoczesnych połączeń z serwerem do 2
        Net.ServicePointManager.SecurityProtocol = Net.ServicePointManager.SecurityProtocol Or Net.SecurityProtocolType.Tls12
#If Not NETCOREAPP Then
        Net.ServicePointManager.DefaultConnectionLimit = 16
#End If

        Me.SetDesktopLocation(0, 0)
        UstawRozmiarOkna()

        'ustawienia startowe okna mapy
        mapa.Manager.Mode = AccessMode.ServerAndCache
        mapa.DragButton = MouseButtons.Left
        mapa.MapScaleInfoEnabled = False
        mapa.DisableAltForSelection = True
        mapa.Zoom = Wartosc(lblZoom.Text, 6)

        UstawEdycjeXY(False)
        UstawWidokZaznaczeniaWgs(False)

        _ustawianieUkladu = True
        For Each u In UkladWspolrzednych.Wszystkie
            cmbUklad.Items.Add(u)
        Next
        _ustawianieUkladu = False

        'wczytywanie ustawień: najpierw conf.txt z folderu domyślnego, potem lastsettings.txt
        Dim nowaSesja As Boolean = File.Exists(FolderDownload & "conf.txt") = False
        If nowaSesja Then
            btnScalanie.Enabled = False
            mapa.Position = New PointLatLng(52.3, 19.2)
            mapa.Zoom = 6
        End If

        WczytajConf(False)
        WczytajLastsettings(nowaSesja)

        'nowa sesja (brak conf.txt) - układ współrzędnych wybrany w ustawieniach jako domyślny
        If nowaSesja Then PrzelaczUklad(Ustawienia.UkladDomyslny, False)

        'widok okna głównego zapamiętany w lastsettings.txt
        UstawWidokWspolrzednych(Ustawienia.KursorISrodekMapy)
        UstawWidokZaznaczeniaWgs(Ustawienia.ZaznaczenieWgs84)
        UstawEdycjeXY(Ustawienia.EdycjaXY)

        'lista zbiorów map - zbiór i warstwy wczytane z conf.txt zostają (wybór zbioru na liście czyści wybrane warstwy;
        'dawniej po ponownym uruchomieniu warstwy sesji znikały i do wznowienia pobierania trzeba je było wybrać od nowa)
        Dim zbiorSesji As String = cmbZbiorMap.Text
        Dim warstwySesji As New List(Of String)(_warstwy)
        WczytajListeZbiorowMap(zbiorSesji)
        WczytajWarstwyZbioru()
        If warstwySesji.Count > 0 AndAlso String.Equals(cmbZbiorMap.Text, zbiorSesji, StringComparison.OrdinalIgnoreCase) Then
            _warstwy.Clear()
            _warstwy.AddRange(warstwySesji)
            Komunikat("Wczytano sesję pobierania z pliku conf.txt: " & zbiorSesji & ", warstwy: " & String.Join(", ", _warstwy) & ".", Color.Green)
        End If
        PokazWarstwy()

        If Ustawienia.Format = "" Then Ustawienia.Format = "jpeg"
        stFormat.Text = "." & Ustawienia.Format
        stSegment.Text = ""
        OdswiezOpisUkladu()
        PrzeliczSiatke()

        _zaladowany = True
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _przerwanie?.Cancel()
        WyczyscZnaczniki()
        ZapamietajPolozenieMapy()
        Try
            Ustawienia.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
    End Sub

#End Region

#Region "Rozmiar okna"

    ''' <summary>
    ''' Najmniejszy rozmiar okna i rozmiar startowy dopasowany do ekranu. Lista warstw WMS rozciąga się razem z oknem
    ''' (kotwice w projekcie formularza), więc okno można zmniejszyć o tyle, o ile da się skrócić tę listę; na ekranie
    ''' niższym nawet od tego prawy panel jest przewijany.
    ''' </summary>
    Private Sub UstawRozmiarOkna()
        Dim minWysokoscListy As Integer = lstWarstwy.ItemHeight * MinWierszyListyWarstw + (lstWarstwy.Height - lstWarstwy.ClientSize.Height)
        Dim zapasListy As Integer = Math.Max(0, lstWarstwy.Height - minWysokoscListy)
        _szerokoscPanelu = Panel2.Width
        _minWysokoscPanelu = Panel2.ClientSize.Height - zapasListy
        Panel2.AutoScrollMinSize = New Size(0, _minWysokoscPanelu)

        Dim ekran As Rectangle = Screen.FromControl(Me).WorkingArea
        Me.MinimumSize = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height - zapasListy, ekran.Height))
        Me.Size = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height, ekran.Height))
    End Sub

    ''' <summary>Przewijany prawy panel (ekran niższy niż najmniejsze okno) poszerza się o pasek przewijania.</summary>
    Private Sub Panel2_SizeChanged(sender As Object, e As EventArgs) Handles Panel2.SizeChanged
        If _szerokoscPanelu = 0 Then Exit Sub
        Dim szerokosc As Integer = _szerokoscPanelu + If(Panel2.Height < _minWysokoscPanelu, SystemInformation.VerticalScrollBarWidth, 0)
        If Panel2.Width <> szerokosc Then Panel2.Width = szerokosc
    End Sub

#End Region

#Region "Ustawienia programu i sesji"

    ''' <summary>Odczyt ostatnich ustawień programu z lastsettings.txt.</summary>
    Public Sub WczytajLastsettings(Optional nowaSesja As Boolean = False)
        If File.Exists(PlikLastsettings) = False Then Exit Sub
        'numeracja segmentów jest parametrem sesji - lastsettings.txt (np. z numeracją innej, ostatnio wczytanej sesji)
        'nie może jej zmienić, bo nazwy segmentów przestałyby pasować do plików w folderze sesji
        Dim numeracjaSesji As String = If(nowaSesja, "", _numeracjaSesji)
        Try
            Ustawienia.Wczytaj(PlikLastsettings, nowaSesja)
        Catch
            Exit Sub
        Finally
            If numeracjaSesji <> "" Then Ustawienia.Numeracja = numeracjaSesji
        End Try
        lblSrodekSzer.Text = Ustawienia.SrodekMapySzerokosc
        lblSrodekDlug.Text = Ustawienia.SrodekMapyDlugosc
        lblZoom.Text = Ustawienia.SkalaMapy

        'ostatnio zapisana pozycja okna mapy
        mapa.Zoom = Wartosc(lblZoom.Text, 6)
        mapa.Position = New PointLatLng(Wartosc(lblSrodekSzer.Text, 52.3), Wartosc(lblSrodekDlug.Text, 19.2))

        Komunikat("Wczytano ostatnio zapisane ustawienia programu z lastsettings.txt. Styl numerowania segmentów to: " & Ustawienia.Numeracja, Color.Green)
    End Sub

    ''' <summary>
    ''' Odczyt parametrów sesji z conf.txt. Gdy pytajOFolder - użytkownik wskazuje folder sesji.
    ''' Cały plik jest najpierw wczytywany, a dopiero potem przypisywany do kontrolek (dawniej zmiana rodzaju mapy
    ''' w trakcie czytania pliku zamykała go i reszta parametrów nie była wczytywana).
    ''' </summary>
    Private Sub WczytajConf(pytajOFolder As Boolean)
        Dim folder As String = FolderSesji
        If pytajOFolder Then
            'Anuluj - bez zmian (dawniej ponownie wczytywany był conf.txt bieżącej sesji, co kasowało niezapisane zmiany)
            dlgFolder.SelectedPath = FolderDownload
            If dlgFolder.ShowDialog <> DialogResult.OK Then Exit Sub
            folder = FolderZUkosnikiem(dlgFolder.SelectedPath)
            If File.Exists(folder & "conf.txt") = False Then
                MsgBox("Brak pliku conf.txt w podanej lokalizacji.")
                Exit Sub
            End If
        End If

        FolderSesji = folder
        If File.Exists(folder & "conf.txt") = False Then Exit Sub

        Dim c As KonfiguracjaSesji
        Try
            c = KonfiguracjaSesji.Wczytaj(folder & "conf.txt", KodowanieSystemowe())
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku conf.txt: " & ex.Message, Color.Red)
            Exit Sub
        End Try

        'zbiór map - zmiana wywołuje wczytanie listy warstw i wyczyszczenie wybranych
        cmbZbiorMap.Text = c.RodzajMapy
        Ustawienia.Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
        txtXDol.Text = c.XDol
        txtYLewy.Text = c.YLewy
        txtXGora.Text = c.XGora
        txtYPrawy.Text = c.YPrawy
        txtBokSegmentu.Text = c.BokSegmentuPx
        txtRozmiarPiksela.Text = c.RozmiarPiksela

        _warstwy.Clear()
        For i = 0 To Math.Min(c.LiczbaWarstw, MaksLiczbaWarstw) - 1
            If c.Warstwy(i) <> "" Then _warstwy.Add(c.Warstwy(i))
        Next
        PokazWarstwy()

        'format spoza listy obsługiwanych (np. svg+xml z dawnych wersji) - jpeg
        If c.Format <> "" Then Ustawienia.Format = UstawieniaProgramu.NormalizujFormat(c.Format)
        Ustawienia.Prefiks = c.Prefiks
        Ustawienia.Przezroczystosc = c.Przezroczystosc
        _dataPobrania = c.DataPobrania
        Ustawienia.PobierajPowyzejOstatniego = c.PobierajPowyzejOstatniego
        If c.Numeracja <> "" Then Ustawienia.Numeracja = c.Numeracja
        _numeracjaSesji = c.Numeracja
        lblSrodekSzer.Text = c.SrodekMapySzerokosc
        lblSrodekDlug.Text = c.SrodekMapyDlugosc
        lblZoom.Text = c.SkalaMapy

        stFormat.Text = "." & Ustawienia.Format
        OdswiezOpisUkladu()

        'odświeżenie widoku okna mapy
        mapa.Refresh()
        mapa.ReloadMap()
        mapa.Zoom = Wartosc(lblZoom.Text, 6)
        mapa.Position = New PointLatLng(Wartosc(lblSrodekSzer.Text, 52.3), Wartosc(lblSrodekDlug.Text, 19.2))
        ZapamietajPolozenieMapy()

        'obszar wzdłuż trasy zapisany w folderze sesji
        _korytarz = Korytarz.ZFolderu(folder)
        UsunKorytarzToolStripMenuItem.Enabled = _korytarz IsNot Nothing
        WyczyscZnaczniki()
        If _korytarz IsNot Nothing Then
            PokazKorytarz()
        ElseIf Ustawienia.SiatkaSegmentowNaMapie Then
            Dim s = BiezacaSiatka()
            If s.Poprawna Then PokazZnaczniki(centruj:=False)
        End If

        Komunikat("Wczytano ustawienia sesji z pliku conf.txt. Format graficzny pobieranych segmentów to " & Ustawienia.Format &
                  ". Styl ich numerowania to: " & Ustawienia.Numeracja & If(_korytarz IsNot Nothing, ". " & OpisKorytarza(), ""), Color.Green)
    End Sub

    ''' <summary>Zapis parametrów sesji do conf.txt. Gdy pytajOFolder - użytkownik wskazuje folder.</summary>
    Public Sub ZapiszConf(pytajOFolder As Boolean)
        If pytajOFolder Then
            'Anuluj - nic nie jest zapisywane (dawniej conf.txt bieżącego folderu był mimo to nadpisywany)
            dlgFolder.SelectedPath = FolderSesji
            If dlgFolder.ShowDialog <> DialogResult.OK Then Exit Sub
            Dim folder As String = FolderZUkosnikiem(dlgFolder.SelectedPath)
            If Not PotwierdzFolderInnejSesji(folder, "Zapisanie ustawień nadpisze jej plik conf.txt, a jej segmenty zostaną uznane za segmenty bieżącej sesji.") Then Exit Sub
            FolderSesji = folder
        End If

        Dim c = BiezacaKonfiguracja()
        Try
            Directory.CreateDirectory(FolderSesji)
            c.Zapisz(FolderSesji & "conf.txt", KodowanieSystemowe())
            'obszar wzdłuż trasy - plik korytarz.txt obok conf.txt (bez korytarza - usunięcie nieaktualnego pliku)
            Dim plikKorytarza As String = FolderSesji & Korytarz.NazwaPliku
            If _korytarz IsNot Nothing Then
                _korytarz.Zapisz(plikKorytarza)
            ElseIf File.Exists(plikKorytarza) Then
                File.Delete(plikKorytarza)
            End If
        Catch ex As Exception
            Komunikat("Nie udało się zapisać pliku conf.txt: " & ex.Message, Color.Red)
            Exit Sub
        End Try

        If pytajOFolder Then Komunikat("Zapisano ustawienia sesji do pliku conf.txt w folderze " & FolderSesji, Color.Green)
    End Sub

    ''' <summary>Ścieżka folderu zakończona jednym ukośnikiem.</summary>
    Private Shared Function FolderZUkosnikiem(folder As String) As String
        Return folder.TrimEnd("\"c, "/"c) & "\"
    End Function

    ''' <summary>Parametry bieżącej sesji (pola formularza i ustawienia) w postaci zapisywanej w conf.txt.</summary>
    Private Function BiezacaKonfiguracja() As KonfiguracjaSesji
        Dim c As New KonfiguracjaSesji With {
            .Folder = FolderSesji, .RodzajMapy = cmbZbiorMap.Text,
            .XDol = txtXDol.Text, .YLewy = txtYLewy.Text, .XGora = txtXGora.Text, .YPrawy = txtYPrawy.Text,
            .BokSegmentuPx = txtBokSegmentu.Text, .RozmiarPiksela = txtRozmiarPiksela.Text,
            .LiczbaWarstw = _warstwy.Count, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .SrodekMapySzerokosc = lblSrodekSzer.Text.Replace(","c, "."c), .SrodekMapyDlugosc = lblSrodekDlug.Text.Replace(","c, "."c),
            .SkalaMapy = lblZoom.Text.Replace(","c, "."c), .Numeracja = Ustawienia.Numeracja, .UkladEpsg = Ustawienia.Uklad.Epsg,
            .Przezroczystosc = Ustawienia.Przezroczystosc, .DataPobrania = _dataPobrania, .AdresSerwera = _plikWarstw.Adres}
        For i = 0 To _warstwy.Count - 1
            c.Warstwy(i) = _warstwy(i)
        Next
        Return c
    End Function

    ''' <summary>
    ''' Segmenty innej sesji w folderze: conf.txt z inną siatką, układem, serwerem lub warstwami albo brak conf.txt
    ''' (sesja nieznana - np. folder wynikowy nakładania map) przy plikach o nazwach segmentów bieżącej sesji.
    ''' Zwraca liczbę takich plików (0 - folder pusty, nowy albo z tą samą sesją, czyli wznowienie) i opis przyczyny.
    ''' </summary>
    Private Function SegmentyInnejSesji(folder As String, ByRef opis As String) As Integer
        opis = ""
        Dim biezaca = BiezacaKonfiguracja()
        Dim plikConf As String = folder & "conf.txt"
        If File.Exists(plikConf) Then
            Dim zapisana As KonfiguracjaSesji
            Try
                zapisana = KonfiguracjaSesji.Wczytaj(plikConf, KodowanieSystemowe())
            Catch
                zapisana = Nothing
            End Try
            If zapisana IsNot Nothing AndAlso biezaca.TaSamaSiatka(zapisana) Then Return 0
            opis = If(zapisana Is Nothing, "z nieczytelnym plikiem conf.txt",
                      "z innym zasięgiem siatki, rozmiarem segmentu, układem, serwerem lub warstwami")
        Else
            'bez conf.txt nie wiadomo, czym są pliki o nazwach segmentów - dawniej uznawano je za pobrane segmenty
            opis = "bez pliku conf.txt - nie wiadomo, z jakiej sesji pochodzą"
        End If
        Dim liczba As Integer = biezaca.LiczbaPlikowSegmentow(folder)
        Dim tekst As String = Odmiana(liczba, "plik", "pliki", "plików")
        opis = liczba & " " & tekst & " " & opis
        Return liczba
    End Function

    ''' <summary>Pyta o zgodę, gdy folder zawiera segmenty innej sesji pobierania. True - folder można użyć.</summary>
    Private Function PotwierdzFolderInnejSesji(folder As String, skutek As String) As Boolean
        Dim opis As String = ""
        Dim liczba As Integer = SegmentyInnejSesji(folder, opis)
        If liczba = 0 Then Return True
        Return MessageBox.Show("Folder " & folder & " zawiera segmenty innej sesji pobierania (" & opis & ")." & vbCrLf & vbCrLf &
                               skutek & vbCrLf & vbCrLf & "Kontynuować?", "Folder innej sesji",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    ''' <summary>
    ''' Przed pobieraniem: folder z segmentami innej sesji. Segmenty o tych samych nazwach zostałyby uznane za pobrane
    ''' i trafiły do scalonej mapy, choć przedstawiają inny teren - nowa sesja zapisywana jest wtedy w nowym podfolderze.
    ''' False - pobieranie anulowane.
    ''' </summary>
    Private Function PrzygotujFolderSesji() As Boolean
        Dim opis As String = ""
        Dim liczba As Integer = SegmentyInnejSesji(FolderSesji, opis)
        If liczba = 0 Then Return True
        Dim nowy As String = FolderDownload & "sesja_" & Date.Now.ToString("yyyy-MM-dd_HHmmss", Globalization.CultureInfo.InvariantCulture) & "\"
        Dim odp = MessageBox.Show("Folder " & FolderSesji & " zawiera segmenty innej sesji pobierania (" & opis & ")." & vbCrLf &
                                  "Segmenty o tych samych nazwach zostałyby uznane za już pobrane i trafiłyby do scalonej mapy." & vbCrLf & vbCrLf &
                                  "Tak - pobierz nową sesję do folderu:" & vbCrLf & nowy & vbCrLf & vbCrLf &
                                  "Nie - anuluj pobieranie (aby wznowić poprzednią sesję, wczytaj jej ustawienia z menu Sesja pobierania).",
                                  "Folder innej sesji", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If odp <> DialogResult.Yes Then
            Komunikat("Anulowano pobieranie - folder " & FolderSesji & " zawiera segmenty innej sesji.", Color.Red)
            Return False
        End If
        Try
            Directory.CreateDirectory(nowy)
        Catch ex As Exception
            Komunikat("Nie udało się utworzyć folderu " & nowy & ": " & ex.Message, Color.Red)
            Return False
        End Try
        FolderSesji = nowy
        Return True
    End Function
    Private Sub ZapiszToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZapiszToolStripMenuItem.Click
        If PobieranieTrwa("Zapisanie ustawień sesji w innym folderze") Then Exit Sub
        ZapiszConf(True)
    End Sub

    Private Sub WczytajToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles WczytajToolStripMenuItem1.Click
        If PobieranieTrwa("Wczytanie ustawień innej sesji") Then Exit Sub
        WczytajConf(True)
    End Sub

    ''' <summary>
    ''' Czy trwa pobieranie - wtedy (z komunikatem) nie wolno zmieniać sesji: pobieranie korzysta z kopii parametrów,
    ''' a zmiana conf.txt, warstw lub folderu w trakcie rozjeżdżała zapisane parametry z pobieranymi segmentami.
    ''' </summary>
    Private Function PobieranieTrwa(czynnosc As String) As Boolean
        If _przerwanie Is Nothing Then Return False
        MessageBox.Show(czynnosc & " będzie możliwe po zakończeniu lub przerwaniu pobierania segmentów.", "Trwa pobieranie",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
        Return True
    End Function

#End Region

#Region "Zbiory map i warstwy"

    ''' <summary>Lista zbiorów map z pliku warstwy\warstwy.txt.</summary>
    Public Sub WczytajListeZbiorowMap(Optional wybranyZbior As String = "")
        cmbZbiorMap.Items.Clear()
        Dim znalezione As New List(Of String)()
        Dim dodane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        '1. zbiory w folderze programu
        DodajZbioryZIndeksu(Path.Combine(FolderWarstwProgramu, "warstwy.txt"), znalezione, dodane)
        '2. zbiory w folderze danych użytkownika (jeśli inny niż programu)
        If Not String.Equals(FolderWarstwProgramu, FolderWarstwDanych, StringComparison.OrdinalIgnoreCase) Then
            DodajZbioryZIndeksu(Path.Combine(FolderWarstwDanych, "warstwy.txt"), znalezione, dodane)
        End If

        For Each nazwa In znalezione
            cmbZbiorMap.Items.Add(nazwa)
        Next

        If cmbZbiorMap.Items.Count = 0 Then
            Komunikat("Brak plików z listą zbiorów map.", Color.Red)
        ElseIf Not String.IsNullOrEmpty(wybranyZbior) AndAlso cmbZbiorMap.Items.Contains(wybranyZbior) Then
            cmbZbiorMap.SelectedItem = wybranyZbior
        ElseIf cmbZbiorMap.Items.Contains("skany_map_topograficznych") Then
            cmbZbiorMap.SelectedItem = "skany_map_topograficznych"
        ElseIf cmbZbiorMap.Items.Count > 0 Then
            cmbZbiorMap.SelectedIndex = 0
        End If
    End Sub

    Private Sub DodajZbioryZIndeksu(plik As String, lista As List(Of String), dodane As HashSet(Of String))
        If Not File.Exists(plik) Then Exit Sub
        For Each linia In File.ReadAllLines(plik, PlikWarstw.Kodowanie())
            Dim nazwa As String = linia.Trim().Trim(""""c)
            If nazwa <> "" AndAlso dodane.Add(nazwa) Then lista.Add(nazwa)
        Next
    End Sub

    ''' <summary>Warstwy bieżącego zbioru map do listy wyboru.</summary>
    Private Sub WczytajWarstwyZbioru()
        lstWarstwy.Items.Clear()
        _plikWarstw = New PlikWarstw()

        Dim nazwaPliku As String = cmbZbiorMap.Text & ".txt"
        Dim plik As String = Path.Combine(FolderWarstwProgramu, nazwaPliku)
        If Not File.Exists(plik) Then
            plik = Path.Combine(FolderWarstwDanych, nazwaPliku)
        End If
        If File.Exists(plik) = False Then Exit Sub
        Try
            _plikWarstw = PlikWarstw.Wczytaj(plik)
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku zbioru map: " & ex.Message, Color.Red)
            Exit Sub
        End Try
        For Each w In _plikWarstw.Warstwy
            lstWarstwy.Items.Add(w.Nazwa)
        Next
    End Sub

    ''' <summary>Wyświetla wybrane warstwy w etykietach 1) ... 12).</summary>
    Private Sub PokazWarstwy()
        Dim etykiety() As Label = {lblWarstwa01, lblWarstwa02, lblWarstwa03, lblWarstwa04, lblWarstwa05, lblWarstwa06,
                                   lblWarstwa07, lblWarstwa08, lblWarstwa09, lblWarstwa10, lblWarstwa11, lblWarstwa12}
        For i = 0 To etykiety.Length - 1
            etykiety(i).Text = (i + 1).ToString() & ") " & If(i < _warstwy.Count, _warstwy(i), "")
        Next
        btnResetujWarstwy.Enabled = _warstwy.Count > 0
    End Sub

    ''' <summary>Czyści listę wybranych warstw.</summary>
    Private Sub WyczyscWarstwy()
        _warstwy.Clear()
        PokazWarstwy()
    End Sub

    Private Sub cmbZbiorMap_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbZbiorMap.SelectedIndexChanged
        PrzerwijPobieranie()
        WczytajWarstwyZbioru()
        WyczyscWarstwy()
        Komunikat("Zmieniono rodzaj mapy. Wybierz która dokładnie jej warstwa ma zostać pobrana, klikając na nią myszką", Color.Red)
    End Sub

    'dodawanie warstw
    Private Sub lstWarstwy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstWarstwy.SelectedIndexChanged
        If lstWarstwy.SelectedItem Is Nothing Then Exit Sub
        Dim nazwa As String = lstWarstwy.SelectedItem.ToString()

        If _warstwy.Count >= MaksLiczbaWarstw Then
            MsgBox("TEJ WARSTWY NIE MOŻNA JUŻ DODAĆ.")
            Exit Sub
        End If

        _warstwy.Add(nazwa)
        PokazWarstwy()
        Form2.OdswiezNazweTrekBuddy()

        'rozmiar piksela przypisany do warstwy w pliku zbioru map
        Dim w = _plikWarstw.Znajdz(nazwa)
        If w IsNot Nothing AndAlso w.RozmiarPiksela <> "" Then txtRozmiarPiksela.Text = RozmiarPikselaWarstwy(w.RozmiarPiksela)

        Komunikat("Wskazano warstwę: " & nazwa, Color.Green)

        'komunikaty o warstwach WMS HGIS, które ograniczają maksymalny rozmiar segmentu do 256 px
        Dim opis As String = ""
        Select Case nazwa
            Case "m25k"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:25 000 Messtischblatt. Pokrywa ona swoim zasięgiem terytorium Zaboru Pruskiego. Wskazany WMS portalu hgis.cartomatic.pl, który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "wig25k"
                opis = "Wybrałeś warstwę polskiej mapy topograficznej 1:25 000 Wojskowego Instytutu Geograficznego. Pokrywa ona głównie środkową i północną część terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "wig100k"
                opis = "Wybrałeś warstwę polskiej mapy topograficznej 1:100 000 Wojskowego Instytutu Geograficznego. Pokrywa ona terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Karte des Deutschen Reiches. Obejmuje ona terytorium Zaboru Pruskiego. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr_gb"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Grossblatt. Obejmuje ona większość terytorium IIIRP. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
        End Select
        If opis <> "" Then
            txtBokSegmentu.Text = "250"
            Komunikat(opis, Color.Blue)
        End If
    End Sub

    Private Sub btnResetujWarstwy_Click(sender As Object, e As EventArgs) Handles btnResetujWarstwy.Click
        'w trakcie pobierania reset zapisywał conf.txt pobieranej sesji bez warstw i włączał przycisk Pobierz
        If PobieranieTrwa("Zmiana listy warstw") Then Exit Sub
        WyczyscWarstwy()
        ZapiszConf(False)
        btnPobierz.Enabled = True
        btnPrzerwij.Enabled = False
        Komunikat("Zresetowano listę wprowadzonych warstw mapy wskazanych do pobrania", Color.Green)
    End Sub

#End Region

#Region "Siatka segmentów"

    ''' <summary>Siatka segmentów wg pól formularza.</summary>
    Friend Function BiezacaSiatka() As Siatka
        Dim obszar As New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text))
        Return New Siatka(obszar, Wartosc(txtRozmiarPiksela.Text), WartoscCalkowita(txtBokSegmentu.Text))
    End Function

    ''' <summary>
    ''' Przelicza wynikowy rozmiar siatki segmentów (dawniej te same obliczenia powtarzało sześć procedur,
    ''' z których część korzystała z nieaktualnych jeszcze wartości).
    ''' </summary>
    Private Sub PrzeliczSiatke()
        Dim s = BiezacaSiatka()
        'jednostki układu na 1 km w kierunku północ-południe i wschód-zachód
        Dim kmSz As Double = 1000, kmDl As Double = 1000
        Dim geograficzny As Boolean = Ustawienia.Uklad.Geograficzny
        If geograficzny Then
            'w WGS84 rozmiary w km są przybliżone - liczone w środku obszaru; stopień długości jest krótszy
            'od stopnia szerokości (dawniej szerokość liczono jak wysokość - na 52°N zawyżoną ok. 1,6 raza)
            Dim mSz As Double, mDl As Double
            Georeferencja.MetrowNaStopien((s.Obszar.XDol + s.Obszar.XGora) / 2, mSz, mDl)
            kmSz = If(mSz > 0, 1000 / mSz, Double.NaN)
            kmDl = If(mDl > 0, 1000 / mDl, Double.NaN)
        End If
        If s.Poprawna Then
            txtLiczbaKolumn.Text = s.LiczbaKolumn.ToString()
            txtLiczbaWierszy.Text = s.LiczbaWierszy.ToString()
            txtSzerokoscPx.Text = s.SzerokoscPx.ToString()
            txtWysokoscPx.Text = s.WysokoscPx.ToString()
            txtSzerokoscKm.Text = If(Double.IsNaN(kmDl), "", Liczba(Math.Round(s.LiczbaKolumn * s.BokSegmentu / kmDl, 3)))
            txtWysokoscKm.Text = If(Double.IsNaN(kmSz), "", Liczba(Math.Round(s.LiczbaWierszy * s.BokSegmentu / kmSz, 3)))
        Else
            For Each t In New TextBox() {txtLiczbaKolumn, txtLiczbaWierszy, txtSzerokoscPx, txtWysokoscPx, txtSzerokoscKm, txtWysokoscKm}
                t.Text = ""
            Next
        End If
        Dim bok As Double = Wartosc(txtRozmiarPiksela.Text) * WartoscCalkowita(txtBokSegmentu.Text)
        If Double.IsNaN(kmSz) OrElse Double.IsNaN(kmDl) OrElse bok <= 0 Then
            txtZasiegSegmentuKm.Text = ""
        ElseIf geograficzny Then
            'segment w stopniach nie jest kwadratem w terenie - szerokość × wysokość
            txtZasiegSegmentuKm.Text = Liczba(Math.Round(bok / kmDl, 2)) & "×" & Liczba(Math.Round(bok / kmSz, 2))
        Else
            txtZasiegSegmentuKm.Text = Liczba(Math.Round(bok / kmSz, 3))
        End If
    End Sub

    Private Sub PolaZasieguZmienione(sender As Object, e As EventArgs) Handles txtXDol.TextChanged, txtYLewy.TextChanged, txtXGora.TextChanged, txtYPrawy.TextChanged
        btnZnaczniki.Enabled = True
        PrzeliczSiatke()
    End Sub

    Private Sub ParametrySegmentuZmienione(sender As Object, e As EventArgs) Handles txtBokSegmentu.TextChanged, txtRozmiarPiksela.TextChanged
        btnZnaczniki.Enabled = True
        PrzeliczSiatke()
        If _siatkaWidoczna OrElse mapa.SelectedArea.IsEmpty = False OrElse _korytarz IsNot Nothing Then
            Dim s = BiezacaSiatka()
            If s.Poprawna Then
                WyczyscZnaczniki()
                PokazZnaczniki(centruj:=False)
            End If
        End If
    End Sub

    'zamiana przecinka na kropkę w polu rozmiaru piksela
    Private Sub txtRozmiarPiksela_KeyUp(sender As Object, e As KeyEventArgs) Handles txtRozmiarPiksela.KeyUp
        If txtRozmiarPiksela.Text.Contains(",") Then
            txtRozmiarPiksela.Text = txtRozmiarPiksela.Text.Replace(","c, "."c)
            txtRozmiarPiksela.SelectionStart = txtRozmiarPiksela.Text.Length
        End If
    End Sub

    ''' <summary>Opis układu współrzędnych przy współrzędnych kursora.</summary>
    Public Sub OdswiezOpisUkladu()
        Dim u = Ustawienia.Uklad
        Select Case u.Epsg
            Case 2180 : lblKursorUkladOpis.Text = "1992"
            Case 2176 To 2179 : lblKursorUkladOpis.Text = "2000/" & (u.Epsg - 2171).ToString()
            Case 4326 : lblKursorUkladOpis.Text = "WGS84"
            Case Else : lblKursorUkladOpis.Text = "UTM " & (u.Epsg - 32600).ToString() & "N"
        End Select
        ToolTip1.SetToolTip(lblKursorUkladOpis, u.Nazwa)
        'współrzędne kursora tuż za opisem układu (dłuższy opis, np. "WGS84", nachodził na nie)
        lblKursorUklad.Left = lblKursorUkladOpis.Right + 2
        lblRozmiarPikselaOpis.Text = If(u.Geograficzny, "Piksel [°/pix]", "Piksel [m/pix]")

        _ustawianieUkladu = True
        cmbUklad.SelectedItem = u
        _ustawianieUkladu = False
        PrzeliczSiatke()
    End Sub

    ''' <summary>
    ''' Zmiana układu współrzędnych pobierania: wpisany zasięg i rozmiar piksela są przeliczane do nowego układu.
    ''' </summary>
    Private Sub cmbUklad_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbUklad.SelectedIndexChanged
        If _ustawianieUkladu Then Exit Sub
        Dim nowy = TryCast(cmbUklad.SelectedItem, UkladWspolrzednych)
        If nowy IsNot Nothing Then PrzelaczUklad(nowy, True)
    End Sub

    ''' <summary>
    ''' Zmienia układ współrzędnych pobierania; wpisany zasięg i rozmiar piksela są przeliczane do nowego układu.
    ''' </summary>
    Public Sub PrzelaczUklad(nowy As UkladWspolrzednych, pokazKomunikat As Boolean)
        Dim stary = Ustawienia.Uklad
        If nowy Is Nothing OrElse nowy Is stary Then Exit Sub

        Dim obszar As New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text))
        Ustawienia.Uklad = nowy
        If obszar.Szerokosc > 0 AndAlso obszar.Wysokosc > 0 Then
            Dim srodek = stary.DoWgs84(obszar.Srodek)
            Dim piksel As Double = Wartosc(txtRozmiarPiksela.Text)
            Dim z = stary.PrzeliczZasieg(nowy, obszar)
            txtXDol.Text = Liczba(z.XDol)
            txtYLewy.Text = Liczba(z.YLewy)
            txtXGora.Text = Liczba(z.XGora)
            txtYPrawy.Text = Liczba(z.YPrawy)
            If piksel > 0 Then txtRozmiarPiksela.Text = Liczba(stary.PrzeliczRozmiarPiksela(nowy, piksel, srodek))
        End If
        OdswiezOpisUkladu()
        If pokazKomunikat Then
            Komunikat("Wybrano układ " & nowy.Nazwa & ". Zasięg pobierania przeliczono do nowego układu. Upewnij się, że serwer WMS obsługuje ten układ.", Color.Green)
        End If
        If _siatkaWidoczna Then
            Dim s = BiezacaSiatka()
            If s.Poprawna Then
                WyczyscZnaczniki()
                PokazZnaczniki(centruj:=False)
            End If
        End If
    End Sub

    ''' <summary>Odświeża wyświetlanie siatki na mapie (np. po zmianie ustawień widoku).</summary>
    Public Sub OdswiezSiatke()
        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            WyczyscZnaczniki()
            PokazZnaczniki(centruj:=False)
        End If
    End Sub

    ''' <summary>Rozmiar piksela z pliku zbioru map (w metrach) w bieżącym układzie.</summary>
    Private Function RozmiarPikselaWarstwy(tekst As String) As String
        Dim u = Ustawienia.Uklad
        If Not u.Geograficzny Then Return tekst
        Dim metry As Double = Wartosc(tekst)
        If metry <= 0 Then Return tekst
        Return Liczba(UkladWspolrzednych.PL1992.PrzeliczRozmiarPiksela(u, metry, New PunktGeo(mapa.Position.Lat, mapa.Position.Lng)))
    End Function

#End Region

#Region "Pobieranie"

    Private Sub btnPobierz_Click(sender As Object, e As EventArgs) Handles btnPobierz.Click
        RozpocznijPobieranie(False)
    End Sub

    ''' <summary>Ponowne pobranie wszystkich segmentów bieżącej sesji (np. po aktualizacji ortofotomapy).</summary>
    Private Sub OdswiezToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OdswiezToolStripMenuItem.Click
        If _przerwanie IsNot Nothing Then Exit Sub
        If MessageBox.Show("Pobrać ponownie wszystkie segmenty mapy do folderu " & FolderSesji & "?" & vbCrLf & vbCrLf &
                           "Istniejące pliki segmentów zostaną zastąpione nowymi - każdy dopiero po udanym pobraniu nowego segmentu.",
                           "Odświeżenie mapy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        RozpocznijPobieranie(True)
    End Sub

    ''' <summary>Pobieranie segmentów; odswiez - ponownie także tych, które już są w folderze sesji.</summary>
    Private Async Sub RozpocznijPobieranie(odswiez As Boolean)
        If _przerwanie IsNot Nothing Then Exit Sub
        'kontrola parametrów
        Dim piksel As Double = Wartosc(txtRozmiarPiksela.Text)
        Dim bok As Integer = WartoscCalkowita(txtBokSegmentu.Text)
        If txtRozmiarPiksela.Text.Trim() = "" Then
            Komunikat("Nie podano rozmiaru pojedynczego piksela segmentu", Color.Red) : Exit Sub
        ElseIf piksel <= 0 Then
            Komunikat("Rozmiar pojedynczego piksela segmentu musi być większy od zera", Color.Red) : Exit Sub
        End If
        If txtBokSegmentu.Text.Trim() = "" Then
            Komunikat("Nie podano długości boku segmentu.", Color.Red) : Exit Sub
        ElseIf bok < 1 Then
            Komunikat("Długość boku segmentu musi być większa od zera.", Color.Red) : Exit Sub
        ElseIf bok > 2048 Then
            Komunikat("Długość boku segmentu nie może przekraczać 2048 px.", Color.Red) : Exit Sub
        End If
        Dim siatka = BiezacaSiatka()
        If Not siatka.Poprawna Then
            Komunikat("Niepoprawne współrzędne X, Y.", Color.Red) : Exit Sub
        End If
        If _warstwy.Count = 0 Then
            Komunikat("Nie wybrano żadnej warstwy. Aby wybrać warstwę kliknij w jej nazwę.", Color.Red) : Exit Sub
        End If
        If _plikWarstw.Adres = "" Then
            Komunikat("Nie wybrano zbioru map (serwera WMS).", Color.Red) : Exit Sub
        End If
        'przedrostek także z wczytanego conf.txt - sprawdzany przed pobraniem pierwszego segmentu
        Dim bladPrzedrostka As String = KonfiguracjaSesji.BladPrzedrostka(Ustawienia.Prefiks)
        If bladPrzedrostka <> "" Then
            Komunikat(bladPrzedrostka & " Popraw go w oknie Ustawienia.", Color.Red) : Exit Sub
        End If

        'limit wielkości pobierania - przy kilkudziesięciu milionach segmentów brakowało pamięci na ich wykaz
        Dim liczbaSegmentow As Long = siatka.LiczbaSegmentow
        If _korytarz IsNot Nothing Then liczbaSegmentow = CLng(Math.Min(liczbaSegmentow, _korytarz.SzacowanaLiczbaSegmentow(siatka, Ustawienia.Uklad)))
        If siatka.ZbytDuza OrElse liczbaSegmentow > MaksSegmentowPobierania Then
            Komunikat("Zbyt wiele segmentów do pobrania (" & If(_korytarz IsNot Nothing, "ok. ", "") & liczbaSegmentow.ToString("N0") &
                      ", najwięcej " & MaksSegmentowPobierania.ToString("N0") &
                      ") - zmniejsz obszar albo zwiększ rozmiar piksela lub bok segmentu.", Color.Red) : Exit Sub
        End If
        If _korytarz IsNot Nothing Then
            liczbaSegmentow = _korytarz.Segmenty(siatka, Ustawienia.Uklad).Count
            If liczbaSegmentow = 0 Then
                Komunikat("Trasa nie przecina obszaru pobierania - wybierz trasę ponownie albo usuń obszar wzdłuż trasy (menu Sesja pobierania).", Color.Red) : Exit Sub
            End If
        End If

        'zapis parametrów sesji - na wszelki wypadek, żeby nie okazało się, że zapisuje w folderze głównym programu
        If FolderSesji = "" Then
            MsgBox("folder segmentów jest pusty")
            Exit Sub
        End If
        If Not PrzygotujFolderSesji() Then Exit Sub
        _dataPobrania = MetadaneMapy.TekstDaty(Date.Today)
        ZapiszConf(False)

        Dim zadanie As New ZadaniePobierania With {
            .AdresSerwera = _plikWarstw.Adres, .Warstwy = New List(Of String)(_warstwy), .Uklad = Ustawienia.Uklad,
            .Siatka = siatka, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .Numeracja = Siatka.StylZTekstu(Ustawienia.Numeracja), .Folder = FolderSesji, .ZamienOsie = Ustawienia.ZamienXY,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .PrzerwaSekundy = Ustawienia.PrzerwaMiedzyProbami,
            .LiczbaWatkow = Ustawienia.LiczbaWatkow, .LimitCzasuSekundy = Ustawienia.LimitCzasuSekundy,
            .ZachowajKafleWmts = Ustawienia.ZachowajKafleWmts, .JakoscJpegWmts = Ustawienia.JakoscJpegWmts,
            .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .Georeferencja = Ustawienia.OpcjeGeoreferencji(), .TrekBuddy = Ustawienia.TrekBuddy, .NazwaTrekBuddy = Ustawienia.NazwaTrekBuddy,
            .Przezroczystosc = Ustawienia.Przezroczystosc, .Korytarz = _korytarz, .Odswiez = odswiez}

        btnPobierz.Enabled = False
        btnPrzerwij.Enabled = True
        cmbZbiorMap.Enabled = False
        lstWarstwy.Enabled = False
        btnScalanie.Enabled = False
        btnResetujWarstwy.Enabled = False
        btnUsunPobrane.Enabled = False
        OdswiezToolStripMenuItem.Enabled = False
        KorytarzToolStripMenuItem.Enabled = False
        Komunikat(If(odswiez, "Trwa odświeżanie segmentów (", "Trwa pobieranie segmentów (") & liczbaSegmentow & ")", Color.Black)

        'własna kopia - zwalniane jest zawsze źródło tego pobierania, nawet gdyby pole wskazywało już inne
        Dim przerwanie As New CancellationTokenSource()
        _przerwanie = przerwanie
        Dim postep As New Progress(Of PostepPobierania)(AddressOf PokazPostep)
        Dim wynik As WynikPobierania = Nothing
        Try
            wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, postep, przerwanie.Token)
        Catch ex As Exception
            Komunikat("Błąd pobierania: " & ex.Message, Color.Red)
        Finally
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            przerwanie.Dispose()
            btnPobierz.Enabled = True
            btnPrzerwij.Enabled = False
            cmbZbiorMap.Enabled = True
            lstWarstwy.Enabled = True
            btnResetujWarstwy.Enabled = _warstwy.Count > 0
            btnUsunPobrane.Enabled = True
            OdswiezToolStripMenuItem.Enabled = True
            KorytarzToolStripMenuItem.Enabled = True
            stPostep.Value = 0
            stSegment.Text = ""
        End Try

        If wynik Is Nothing Then Exit Sub
        If wynik.Przerwano Then
            Komunikat("Przerwano pobieranie segmentów", Color.Red)
        ElseIf wynik.Nieudane > 0 Then
            Komunikat("Nie udało się ściągnąć wszystkich segmentów (" & wynik.Nieudane & " z " & wynik.Wszystkie &
                      "). Wykaz tych segmentów i przyczyny błędów w pliku error.txt", Color.Red)
        Else
            Komunikat("Zakończono pobieranie. Mapa znajduje się w katalogu " & zadanie.Folder, Color.Green)
        End If
        'folder pobierania (FolderSesji mógł się w międzyczasie zmienić)
        btnScalanie.Enabled = File.Exists(zadanie.Folder & "error.txt") = False
    End Sub

    Private Sub PokazPostep(p As PostepPobierania)
        If p.Komunikat <> "" Then Komunikat(p.Komunikat, Color.Black)
        If p.Wszystkie > 0 Then stPostep.Value = Math.Min(100, CInt(p.Pobrane * 100L \ p.Wszystkie))
        stSegment.Text = p.Segment
    End Sub

    ''' <summary>Przerywa trwające pobieranie.</summary>
    Public Sub PrzerwijPobieranie()
        _przerwanie?.Cancel()
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        'komunikat przed przerwaniem - zakończenie pobierania może nastąpić od razu, wewnątrz Cancel(),
        'i wtedy jego komunikat końcowy nie może zostać nadpisany
        Komunikat("Przerywanie pobierania segmentów...", Color.Red)
        btnPrzerwij.Enabled = False
        PrzerwijPobieranie()
    End Sub

#End Region

#Region "Mapa"

    'zdarzenie CheckedChanged zachodzi także przy ODZNACZANIU przycisku - reaguje tylko zaznaczony
    Private Sub rbOsm_CheckedChanged(sender As Object, e As EventArgs) Handles rbOsm.CheckedChanged
        If rbOsm.Checked = False Then Exit Sub
        GMapProvider.UserAgent = "MapoTero/" & My.Application.Info.Version.ToString
        ZmienPodkladMapy(GMapProviders.OpenStreetMap)
    End Sub

    Private Sub rbGoogle_CheckedChanged(sender As Object, e As EventArgs) Handles rbGoogle.CheckedChanged
        If rbGoogle.Checked = False Then Exit Sub
        ZmienPodkladMapy(GMapProviders.GoogleMap)
    End Sub

    Private Sub rbBing_CheckedChanged(sender As Object, e As EventArgs) Handles rbBing.CheckedChanged
        If rbBing.Checked = False Then Exit Sub
        ZmienPodkladMapy(GMapProviders.BingSatelliteMap)
    End Sub

    ''' <summary>Po zmianie podkładu mapy kafle wczytywane są ponownie, gdy skończy się wczytywanie kafli w toku.</summary>
    Private _przeladujPoZmianiePodkladu As Boolean

    ''' <summary>
    ''' Zmiana podkładu mapy. Kafle poprzedniego podkładu, które GMap.NET pobierał jeszcze w chwili zmiany, trafiały
    ''' do widoku nowego podkładu i zostawały na nim (np. prostokąt zdjęcia satelitarnego na mapie OSM) - po zakończeniu
    ''' wczytywania widok jest więc przeładowywany.
    ''' </summary>
    Private Sub ZmienPodkladMapy(dostawca As GMapProvider)
        _przeladujPoZmianiePodkladu = True
        mapa.MapProvider = dostawca
    End Sub

    'zdarzenie wywoływane w wątku wczytywania kafli
    Private Sub mapa_OnTileLoadComplete(czasMs As Long) Handles mapa.OnTileLoadComplete
        If Not _przeladujPoZmianiePodkladu Then Exit Sub
        _przeladujPoZmianiePodkladu = False
        If IsDisposed OrElse Not IsHandleCreated Then Exit Sub
        BeginInvoke(New Action(Sub() If Not mapa.IsDisposed Then mapa.ReloadMap()))
    End Sub

    ''' <summary>Aktualizuje pola współrzędnych i komunikaty na podstawie zaznaczonego prostokąta na mapie.</summary>
    Private Sub UstawObszarZZaznaczenia(a As RectLatLng)
        If a.IsEmpty OrElse a.WidthLng <= 0.00001 OrElse a.HeightLat <= 0.00001 Then Exit Sub
        Dim u = Ustawienia.Uklad
        Dim gora As Double = a.Lat, dol As Double = a.Lat - a.HeightLat
        Dim lewa As Double = a.Lng, prawa As Double = a.Lng + a.WidthLng

        'prostokąt WGS84 w innym układzie jest lekko obróconym czworokątem - zasięg obejmuje wszystkie jego narożniki
        Dim narozniki() As PunktXY = {u.ZWgs84(gora, lewa), u.ZWgs84(gora, prawa), u.ZWgs84(dol, lewa), u.ZWgs84(dol, prawa)}
        Dim xMin As Double = Double.MaxValue, xMax As Double = Double.MinValue, yMin As Double = Double.MaxValue, yMax As Double = Double.MinValue
        For Each p In narozniki
            xMin = Math.Min(xMin, p.X) : xMax = Math.Max(xMax, p.X)
            yMin = Math.Min(yMin, p.Y) : yMax = Math.Max(yMax, p.Y)
        Next
        If u.Geograficzny Then
            txtXDol.Text = Liczba(Math.Round(xMin, 6)) : txtYLewy.Text = Liczba(Math.Round(yMin, 6))
            txtXGora.Text = Liczba(Math.Round(xMax, 6)) : txtYPrawy.Text = Liczba(Math.Round(yMax, 6))
        Else
            txtXDol.Text = Liczba(Math.Floor(xMin)) : txtYLewy.Text = Liczba(Math.Floor(yMin))
            txtXGora.Text = Liczba(Math.Ceiling(xMax)) : txtYPrawy.Text = Liczba(Math.Ceiling(yMax))
        End If

        lblZaznSzerGora.Text = "lat= " & Liczba(Math.Round(gora, 4))
        lblZaznSzerDol.Text = "lat= " & Liczba(Math.Round(dol, 4))
        lblZaznDlugPrawa.Text = "lng= " & Liczba(Math.Round(prawa, 4))
        lblZaznDlugLewa.Text = "lng= " & Liczba(Math.Round(lewa, 4))

        Dim s = BiezacaSiatka()
        If s.Poprawna AndAlso Not u.Geograficzny Then
            Dim km2 As Double = s.LiczbaKolumn * s.BokSegmentu / 1000.0 * s.LiczbaWierszy * s.BokSegmentu / 1000.0
            If CDbl(s.SzerokoscPx) * s.WysokoscPx < 5000000000.0 Then
                Komunikat("Zaznaczyłeś obszar do pobrania o powierzchni " & Liczba(Math.Round(km2, 3)) & " km2", Color.Green)
            Else
                Komunikat("Zaznaczyłeś obszar do pobrania o powierzchni " & Liczba(Math.Round(km2, 3)) & " km2. To dużo. Poradzę sobie. Jednak uzbroj się lepiej w kubek gorącej kawy :)", Color.Red)
            End If
        End If
    End Sub

    Private Sub mapa_MouseMove(sender As Object, e As MouseEventArgs) Handles mapa.MouseMove
        Dim u = Ustawienia.Uklad

        'zmiana zasięgu siatki uchwytem; brak wciśniętego przycisku - mysz puszczona poza oknem programu
        If _przeciaganyUchwyt IsNot Nothing Then
            If e.Button = MouseButtons.Left Then PrzeciagnijUchwyt(e) Else ZakonczPrzeciaganie(anuluj:=False)
        End If

        'współrzędne zasięgu podczas przeciągania zaznaczenia (nie w trakcie pobierania - zmieniałoby zasięg pobieranej sesji)
        If Ustawienia.EdycjaXY = False AndAlso _przerwanie Is Nothing AndAlso mapa.SelectedArea.IsEmpty = False Then
            'nowe zaznaczenie prostokąta zastępuje obszar wzdłuż trasy
            If _korytarz IsNot Nothing Then UsunKorytarz(False)
            If _siatkaWidoczna Then WyczyscZnaczniki()
            UstawObszarZZaznaczenia(mapa.SelectedArea)
        End If

        'współrzędne środka mapy w WGS84
        lblSrodekSzer.Text = Liczba(Math.Round(mapa.Position.Lat, 4))
        lblSrodekDlug.Text = Liczba(Math.Round(mapa.Position.Lng, 4))

        'współrzędne kursora
        Dim kursor = mapa.FromLocalToLatLng(e.X, e.Y)
        Dim k = u.ZWgs84(kursor.Lat, kursor.Lng)
        If u.Geograficzny Then
            lblKursorUklad.Text = "x= " & Liczba(Math.Round(k.X, 5)) & "   y= " & Liczba(Math.Round(k.Y, 5))
        Else
            lblKursorUklad.Text = "x= " & Liczba(Math.Round(k.X)) & "   y= " & Liczba(Math.Round(k.Y))
        End If

        lblKursorWgs.BackColor = Color.Transparent
        lblKursorWgs.Location = New Point(e.X - 140, e.Y - 10)
        If Ustawienia.KursorWgs84 Then
            lblKursorWgs.Text = "lat= " & Liczba(Math.Round(kursor.Lat, 4)) & "   lng= " & Liczba(Math.Round(kursor.Lng, 4))
            lblKursorWgs.Visible = True
        Else
            lblKursorWgs.Visible = False
        End If
    End Sub

    Private Sub btnPowieksz_Click(sender As Object, e As EventArgs) Handles btnPowieksz.Click
        mapa.Zoom += 1
    End Sub

    Private Sub btnPomniejsz_Click(sender As Object, e As EventArgs) Handles btnPomniejsz.Click
        mapa.Zoom -= 1
    End Sub

    Private Sub mapa_OnMapZoomChanged() Handles mapa.OnMapZoomChanged
        lblZoom.Text = mapa.Zoom.ToString()
    End Sub

    Private Sub mapa_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles mapa.MouseDoubleClick
        If e.Button = MouseButtons.Right Then
            mapa.Zoom -= 1
        Else
            mapa.Zoom += 1
        End If
    End Sub

    ''' <summary>Czyści i poprawnie zwalnia zasoby GDI+ wszystkich znaczników na mapie.</summary>
    Private Sub WyczyscZnaczniki()
        _siatkaWidoczna = False
        _uchwytPodKursorem = Nothing
        'Markers.Clear() przy warstwie podpiętej pod mapę zeruje też flagę najechania myszy na znacznik
        For Each overlay In mapa.Overlays
            For Each marker In overlay.Markers
                marker.Dispose()
            Next
            overlay.Markers.Clear()
            For Each trasa In overlay.Routes
                trasa.Dispose()
            Next
            overlay.Routes.Clear()
            For Each wielokat In overlay.Polygons
                wielokat.Dispose()
            Next
            overlay.Polygons.Clear()
        Next
        mapa.Overlays.Clear()
    End Sub

    'znaczniki rzeczywistego zasięgu pobieranej mapy
    Private Sub btnZnaczniki_Click(sender As Object, e As EventArgs) Handles btnZnaczniki.Click
        WyczyscZnaczniki()
        PokazZnaczniki(centruj:=True)
    End Sub

    Private Sub mapa_MouseDown(sender As Object, e As MouseEventArgs) Handles mapa.MouseDown
        If _przerwanie IsNot Nothing OrElse Ustawienia.EdycjaXY Then Exit Sub
        'GMap.NET nie przesuwa mapy, gdy lewy przycisk wciśnięto nad znacznikiem - przeciąganie uchwytu zmienia zasięg siatki
        If e.Button = MouseButtons.Left AndAlso _uchwytPodKursorem IsNot Nothing AndAlso mapa.IsMouseOverMarker AndAlso UchwytyAktywne() Then
            RozpocznijPrzeciaganie(_uchwytPodKursorem, e.Location)
            Exit Sub
        End If
        If e.Button = MouseButtons.Right Then
            If _siatkaWidoczna Then
                WyczyscZnaczniki()
                btnZnaczniki.Enabled = True
            End If
        End If
    End Sub

    ''' <summary>Znaczniki i siatka segmentów po zakończeniu zaznaczania obszaru pobierania (puszczenie prawego przycisku).</summary>
    Private Sub mapa_OnSelectionChange(Selection As RectLatLng, ZoomToFit As Boolean) Handles mapa.OnSelectionChange
        If _przerwanie IsNot Nothing OrElse Ustawienia.EdycjaXY Then
            'zaznaczenie bez skutku - pozostawione na mapie nadpisywało potem pola XY przy każdym ruchu myszy
            If Not Selection.IsEmpty Then mapa.SelectedArea = RectLatLng.Empty
            Exit Sub
        End If
        If Selection.IsEmpty OrElse Selection.WidthLng <= 0.00001 OrElse Selection.HeightLat <= 0.00001 Then Exit Sub
        If _korytarz IsNot Nothing Then UsunKorytarz(False)
        UstawObszarZZaznaczenia(Selection)
        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            WyczyscZnaczniki()
            PokazZnaczniki(centruj:=False)
        End If
        mapa.SelectedArea = RectLatLng.Empty
    End Sub

#Region "Zmiana zasięgu siatki uchwytami"

    ''' <summary>Czy zasięg siatki można zmieniać uchwytami: prostokątny obszar zaznaczany myszą, bez trwającego pobierania.</summary>
    Private Function UchwytyAktywne() As Boolean
        Return _korytarz Is Nothing AndAlso Not Ustawienia.EdycjaXY AndAlso _przerwanie Is Nothing
    End Function

    Private Sub mapa_OnMarkerEnter(item As GMapMarker) Handles mapa.OnMarkerEnter
        _uchwytPodKursorem = TryCast(item, UchwytSiatki)
        If _uchwytPodKursorem IsNot Nothing AndAlso UchwytyAktywne() Then mapa.Cursor = _uchwytPodKursorem.Kursor
    End Sub

    Private Sub mapa_OnMarkerLeave(item As GMapMarker) Handles mapa.OnMarkerLeave
        If item Is _uchwytPodKursorem Then _uchwytPodKursorem = Nothing
    End Sub

    Private Sub mapa_MouseUp(sender As Object, e As MouseEventArgs) Handles mapa.MouseUp
        If _przeciaganyUchwyt IsNot Nothing AndAlso e.Button = MouseButtons.Left Then ZakonczPrzeciaganie(anuluj:=False)
    End Sub

    ''' <summary>Esc podczas przeciągania uchwytu przywraca zasięg sprzed przeciągania.</summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape AndAlso _przeciaganyUchwyt IsNot Nothing Then
            ZakonczPrzeciaganie(anuluj:=True)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ''' <summary>Początek przeciągania: siatka i uchwyty ustępują miejsca samemu obrysowi zasięgu.</summary>
    Private Sub RozpocznijPrzeciaganie(uchwyt As UchwytSiatki, kursor As Point)
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then Exit Sub
        _przeciaganyUchwyt = uchwyt
        Dim chwyt = mapa.FromLocalToLatLng(kursor.X, kursor.Y)
        _punktChwytu = Ustawienia.Uklad.ZWgs84(chwyt.Lat, chwyt.Lng)
        _zasiegPrzedPrzeciaganiem = s.ZasiegSiatki
        _zasiegPrzeciagany = s.ZasiegSiatki
        _bokPrzeciagania = s.BokSegmentu
        _polaPrzedPrzeciaganiem = {txtXDol.Text, txtYLewy.Text, txtXGora.Text, txtYPrawy.Text}
        mapa.Cursor = uchwyt.Kursor
        KomunikatPrzeciagania()
        'GMap.NET wywołuje zdarzenie MouseDown przed sprawdzeniem, czy kursor jest nad znacznikiem - usunięcie uchwytów
        'już teraz rozpoczęłoby przesuwanie mapy; obrys zastępuje siatkę dopiero po zakończeniu obsługi wciśnięcia przycisku
        BeginInvoke(New Action(Sub()
                                   If _przeciaganyUchwyt IsNot Nothing Then PokazObrysPrzeciagania()
                               End Sub))
    End Sub

    ''' <summary>
    ''' Przesuwa krawędzie wskazane uchwytem do linii siatki najbliższej kursorowi. Siatka rośnie od lewego dolnego narożnika,
    ''' więc górna i prawa krawędź dodają lub odejmują segmenty, a dolna i lewa przesuwają ten narożnik o pełne segmenty -
    ''' pozostałe segmenty zostają na swoich miejscach. Przeciwległe krawędzie się nie zmieniają; najmniejsza siatka to 1 x 1.
    ''' </summary>
    Private Sub PrzeciagnijUchwyt(e As MouseEventArgs)
        mapa.Cursor = _przeciaganyUchwyt.Kursor
        Dim kursor = mapa.FromLocalToLatLng(e.X, e.Y)
        Dim p = Ustawienia.Uklad.ZWgs84(kursor.Lat, kursor.Lng)
        Dim z0 = _zasiegPrzedPrzeciaganiem, bok = _bokPrzeciagania
        Dim z = z0
        If _przeciaganyUchwyt.Przesuwanie Then
            'cała siatka przesuwana swobodnie za kursorem (liczba segmentów bez zmian) - o pełne metry,
            'a w WGS84 z dokładnością 6 miejsc po przecinku, jak przy zaznaczaniu obszaru myszą
            Dim miejsca As Integer = If(Ustawienia.Uklad.Geograficzny, 6, 0)
            Dim dx As Double = Math.Round(p.X - _punktChwytu.X, miejsca)
            Dim dy As Double = Math.Round(p.Y - _punktChwytu.Y, miejsca)
            z = New Zasieg(z0.XDol + dx, z0.YLewy + dy, z0.XGora + dx, z0.YPrawy + dy)
        End If
        Select Case _przeciaganyUchwyt.KierunekX
            Case 1 : z.XGora = z0.XDol + LiczbaSegmentowDo(p.X - z0.XDol, bok) * bok
            Case -1 : z.XDol = z0.XGora - LiczbaSegmentowDo(z0.XGora - p.X, bok) * bok
        End Select
        Select Case _przeciaganyUchwyt.KierunekY
            Case 1 : z.YPrawy = z0.YLewy + LiczbaSegmentowDo(p.Y - z0.YLewy, bok) * bok
            Case -1 : z.YLewy = z0.YPrawy - LiczbaSegmentowDo(z0.YPrawy - p.Y, bok) * bok
        End Select
        If RowneZasiegi(z, _zasiegPrzeciagany) Then Exit Sub

        _zasiegPrzeciagany = z
        UstawPolaZasiegu(z)
        PokazObrysPrzeciagania()
        KomunikatPrzeciagania()
    End Sub

    ''' <summary>Liczba pełnych segmentów najbliższa odległości (co najmniej 1).</summary>
    Private Shared Function LiczbaSegmentowDo(odleglosc As Double, bok As Double) As Integer
        Return CInt(Math.Max(1, Math.Min(MaksKolumnUchwytem, Math.Round(odleglosc / bok))))
    End Function

    Private Shared Function RowneZasiegi(a As Zasieg, b As Zasieg) As Boolean
        Return a.XDol = b.XDol AndAlso a.YLewy = b.YLewy AndAlso a.XGora = b.XGora AndAlso a.YPrawy = b.YPrawy
    End Function

    Private Sub UstawPolaZasiegu(z As Zasieg)
        txtXDol.Text = Liczba(z.XDol)
        txtYLewy.Text = Liczba(z.YLewy)
        txtXGora.Text = Liczba(z.XGora)
        txtYPrawy.Text = Liczba(z.YPrawy)
    End Sub

    ''' <summary>Podgląd podczas przeciągania - sam obrys zasięgu (przerysowanie całej siatki przy każdym ruchu myszy byłoby zbyt wolne).</summary>
    Private Sub PokazObrysPrzeciagania()
        WyczyscZnaczniki()
        Dim u = Ustawienia.Uklad
        Dim z = _zasiegPrzeciagany
        Dim warstwa As New GMapOverlay("podglad")
        mapa.Overlays.Add(warstwa)
        Dim punkty As New List(Of PointLatLng)(4)
        For Each n In {z.LewyGorny, z.PrawyGorny, z.PrawyDolny, z.LewyDolny}
            Dim g = u.DoWgs84(n)
            punkty.Add(New PointLatLng(g.Szerokosc, g.Dlugosc))
        Next
        warstwa.Polygons.Add(New GMapPolygon(punkty, "obrys") With {
            .Fill = New SolidBrush(Color.FromArgb(25, Color.RoyalBlue)),
            .Stroke = New Pen(Color.RoyalBlue, 2) With {.DashStyle = Drawing2D.DashStyle.Dash}})
    End Sub

    Private Sub KomunikatPrzeciagania()
        Komunikat(OpisZasieguSiatki(_zasiegPrzeciagany) & ". Puść przycisk myszy, aby zatwierdzić zasięg; Esc - anuluj.", Color.Blue)
    End Sub

    ''' <summary>Opis siatki o zasięgu z pełnych segmentów: kolumny x wiersze = segmenty (i powierzchnia w układach metrycznych).</summary>
    Private Function OpisZasieguSiatki(z As Zasieg) As String
        Dim bok = _bokPrzeciagania
        Dim kolumny As Long = CLng(Math.Round(z.Szerokosc / bok))
        Dim wiersze As Long = CLng(Math.Round(z.Wysokosc / bok))
        Dim opis As String = "Zasięg siatki: " & kolumny & " kol. × " & wiersze & " wier. = " & (kolumny * wiersze) & " " &
                             Odmiana(kolumny * wiersze, "segment", "segmenty", "segmentów")
        If Not Ustawienia.Uklad.Geograficzny Then
            opis &= " (" & Liczba(Math.Round(z.Szerokosc / 1000.0 * z.Wysokosc / 1000.0, 3)) & " km2)"
        End If
        Return opis
    End Function

    ''' <summary>Koniec przeciągania: zatwierdzenie nowego zasięgu albo (Esc, brak zmiany) przywrócenie pól sprzed przeciągania.</summary>
    Private Sub ZakonczPrzeciaganie(anuluj As Boolean)
        If _przeciaganyUchwyt Is Nothing Then Exit Sub
        Dim przesunieto As Boolean = _przeciaganyUchwyt.Przesuwanie
        _przeciaganyUchwyt = Nothing
        mapa.Cursor = Cursors.Default
        Dim zmieniono As Boolean = Not anuluj AndAlso Not RowneZasiegi(_zasiegPrzeciagany, _zasiegPrzedPrzeciaganiem)
        If zmieniono Then
            UstawPolaZasiegu(_zasiegPrzeciagany)
            'współrzędne WGS84 zaznaczenia - prostokąt opisany na nowym zasięgu
            Dim g = ProstokatGeo.Opisany(Ustawienia.Uklad, _zasiegPrzeciagany)
            lblZaznSzerGora.Text = "lat= " & Liczba(Math.Round(g.Polnoc, 4))
            lblZaznSzerDol.Text = "lat= " & Liczba(Math.Round(g.Poludnie, 4))
            lblZaznDlugPrawa.Text = "lng= " & Liczba(Math.Round(g.Wschod, 4))
            lblZaznDlugLewa.Text = "lng= " & Liczba(Math.Round(g.Zachod, 4))
        Else
            txtXDol.Text = _polaPrzedPrzeciaganiem(0)
            txtYLewy.Text = _polaPrzedPrzeciaganiem(1)
            txtXGora.Text = _polaPrzedPrzeciaganiem(2)
            txtYPrawy.Text = _polaPrzedPrzeciaganiem(3)
        End If
        WyczyscZnaczniki()
        PokazZnaczniki(centruj:=False)
        If zmieniono Then
            Komunikat(If(przesunieto, "Przesunięto siatkę segmentów. ", "Zmieniono zasięg pobierania. ") & OpisZasieguSiatki(_zasiegPrzeciagany), Color.Green)
        Else
            Komunikat(If(anuluj, "Anulowano zmianę zasięgu siatki.", "Zasięg siatki bez zmian."), Color.Green)
        End If
    End Sub

#End Region

    ''' <summary>Znaczniki narożników siatki segmentów oraz siatka kafli do pobrania.</summary>
    Private Sub PokazZnaczniki(Optional centruj As Boolean = False)
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then Exit Sub
        Dim u = Ustawienia.Uklad
        Dim z = s.ZasiegSiatki

        'przycisk zostaje aktywny - po przesunięciu mapy wraca do siatki (dawniej był wyłączany do następnej zmiany zasięgu)

        'rysowanie siatki segmentów lub korytarza pod pinezkami
        If _korytarz IsNot Nothing Then
            PokazKorytarz()
        ElseIf Ustawienia.SiatkaSegmentowNaMapie Then
            PokazSiatkeSegmentow()
        End If

        'warstwa dodawana do mapy przed znacznikami - GMap.NET wylicza położenie na ekranie tylko obiektom
        'dodawanym do warstwy już podpiętej pod mapę (inaczej dopiero po przesunięciu lub zmianie skali mapy)
        Dim warstwa As New GMapOverlay("markers")
        mapa.Overlays.Add(warstwa)
        Dim narozniki = {Tuple.Create(z.LewyGorny, "Lewy górny narożnik", 1, -1), Tuple.Create(z.PrawyGorny, "Prawy górny narożnik", 1, 1),
                         Tuple.Create(z.PrawyDolny, "Prawy dolny narożnik", -1, 1), Tuple.Create(z.LewyDolny, "Lewy dolny narożnik", -1, -1)}
        Dim edycja As Boolean = UchwytyAktywne()
        If edycja Then
            'uchwyt środka dodawany pierwszy - przy małej siatce na ekranie uchwyty narożników i boków leżą nad nim
            Dim g = u.DoWgs84(z.Srodek)
            Dim srodek As New UchwytSiatki(New PointLatLng(g.Szerokosc, g.Dlugosc), 0, 0)
            srodek.ToolTip = New GMapRoundedToolTip(srodek)
            srodek.ToolTipText = "Środek siatki" & vbCrLf & "przeciągnij, aby przesunąć" & vbCrLf & "całą siatkę"
            warstwa.Markers.Add(srodek)
        End If
        For Each n In narozniki
            Dim g = u.DoWgs84(n.Item1)
            Dim polozenie As New PointLatLng(g.Szerokosc, g.Dlugosc)
            'prostokątny obszar można poszerzać i zawężać kwadratowymi uchwytami; obszar wzdłuż trasy - tylko pinezki
            Dim znacznik As GMapMarker = If(edycja, DirectCast(New UchwytSiatki(polozenie, n.Item3, n.Item4), GMapMarker),
                                                     New GMarkerGoogle(polozenie, GMarkerGoogleType.red_small))
            znacznik.ToolTip = New GMapRoundedToolTip(znacznik)
            znacznik.ToolTipText = n.Item2 & vbCrLf & "po uwzględnieniu" & vbCrLf & "rozmiaru segmentów " & vbCrLf &
                                   "X: " & Liczba(n.Item1.X) & ", Y: " & Liczba(n.Item1.Y) &
                                   If(edycja, vbCrLf & "przeciągnij, aby zmienić zasięg", "")
            warstwa.Markers.Add(znacznik)
        Next
        If edycja Then
            Dim xSr As Double = (z.XDol + z.XGora) / 2, ySr As Double = (z.YLewy + z.YPrawy) / 2
            Dim boki = {Tuple.Create(New PunktXY(z.XGora, ySr), "Górna krawędź", 1, 0), Tuple.Create(New PunktXY(z.XDol, ySr), "Dolna krawędź", -1, 0),
                        Tuple.Create(New PunktXY(xSr, z.YLewy), "Lewa krawędź", 0, -1), Tuple.Create(New PunktXY(xSr, z.YPrawy), "Prawa krawędź", 0, 1)}
            For Each b In boki
                Dim g = u.DoWgs84(b.Item1)
                Dim uchwyt As New UchwytSiatki(New PointLatLng(g.Szerokosc, g.Dlugosc), b.Item3, b.Item4)
                uchwyt.ToolTip = New GMapRoundedToolTip(uchwyt)
                uchwyt.ToolTipText = b.Item2 & vbCrLf & "przeciągnij, aby zmienić" & vbCrLf &
                                     If(b.Item3 = 0, "liczbę kolumn segmentów", "liczbę wierszy segmentów")
                warstwa.Markers.Add(uchwyt)
            Next
        End If
        _siatkaWidoczna = True

        If centruj Then
            'centrowanie obrazu po wyświetleniu znaczników
            Dim srodek = u.DoWgs84(z.Srodek)
            mapa.Position = New PointLatLng(srodek.Szerokosc, srodek.Dlugosc)

            'siatka rośnie od lewego dolnego narożnika zaznaczenia w górę i w prawo - przy bardzo małym zaznaczeniu
            'pojedynczy segment jest znacznie większy od niego i znaczniki wypadają poza okno mapy; wtedy mapa jest
            'oddalana tak, by objąć cały zasięg siatki
            If Not ZnacznikiWidoczne(warstwa) Then
                Dim gora As Double = Double.MinValue, dol As Double = Double.MaxValue
                Dim lewa As Double = Double.MaxValue, prawa As Double = Double.MinValue
                For Each m In warstwa.Markers
                    gora = Math.Max(gora, m.Position.Lat) : dol = Math.Min(dol, m.Position.Lat)
                    lewa = Math.Min(lewa, m.Position.Lng) : prawa = Math.Max(prawa, m.Position.Lng)
                Next
                'margines wokół znaczników; u góry większy, bo ikona pinezki leży nad wskazywanym punktem
                Dim dSz As Double = gora - dol, dDl As Double = prawa - lewa
                mapa.SetZoomToFitRect(RectLatLng.FromLTRB(lewa - 0.15 * dDl, gora + 0.25 * dSz, prawa + 0.15 * dDl, dol - 0.15 * dSz))
            End If
        End If
    End Sub

    ''' <summary>Rysuje na mapie siatkę segmentów (kafli) dla zaznaczonego prostokątnego obszaru pobierania.</summary>
    Private Sub PokazSiatkeSegmentow()
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then Exit Sub
        Dim u = Ustawienia.Uklad
        Dim zZew = s.ZasiegSiatki
        Dim warstwa As New GMapOverlay("siatka")
        mapa.Overlays.Add(warstwa)

        If s.LiczbaSegmentow > MaksSegmentowNaMapie Then
            Komunikat("Liczba segmentów (" & s.LiczbaSegmentow & ") przekracza limit wyświetlania siatki na mapie (" & MaksSegmentowNaMapie & "). Pokazano zewnętrzny zasięg.", Color.DarkOrange)
            Dim punktyZew As New List(Of PointLatLng)(4)
            For Each n In {zZew.LewyGorny, zZew.PrawyGorny, zZew.PrawyDolny, zZew.LewyDolny}
                Dim g = u.DoWgs84(n)
                punktyZew.Add(New PointLatLng(g.Szerokosc, g.Dlugosc))
            Next
            warstwa.Polygons.Add(New GMapPolygon(punktyZew, "zasieg_zewnetrzny") With {
                .Fill = New SolidBrush(Color.FromArgb(40, Color.RoyalBlue)),
                .Stroke = New Pen(Color.FromArgb(140, Color.RoyalBlue), 2)})
            Exit Sub
        End If
        'nieaktualny komunikat o przekroczeniu limitu (np. po zwiększeniu rozmiaru segmentu)
        If txtKomunikaty.Text.StartsWith("Liczba segmentów (", StringComparison.Ordinal) AndAlso txtKomunikaty.Text.Contains("przekracza limit") Then
            Komunikat("Pokazano pełną siatkę segmentów (" & s.LiczbaSegmentow & " " & Odmiana(s.LiczbaSegmentow, "segment", "segmenty", "segmentów") & ").", Color.Green)
        End If

        For wiersz As Integer = 1 To s.LiczbaWierszy
            For kolumna As Integer = 1 To s.LiczbaKolumn
                Dim z = s.Segment(wiersz, kolumna)
                Dim punkty As New List(Of PointLatLng)(4)
                For Each n In {z.LewyGorny, z.PrawyGorny, z.PrawyDolny, z.LewyDolny}
                    Dim g = u.DoWgs84(n)
                    punkty.Add(New PointLatLng(g.Szerokosc, g.Dlugosc))
                Next
                warstwa.Polygons.Add(New GMapPolygon(punkty, "segment") With {
                    .Fill = New SolidBrush(Color.FromArgb(40, Color.RoyalBlue)),
                    .Stroke = New Pen(Color.FromArgb(140, Color.RoyalBlue), 1)})
            Next
        Next
    End Sub

    ''' <summary>
    ''' Czy wszystkie znaczniki warstwy (wraz z ikoną pinezki) mieszczą się w oknie mapy i nie zasłaniają ich
    ''' kontrolki leżące na mapie (przyciski skali, wybór podkładu, współrzędne kursora i środka mapy).
    ''' </summary>
    Private Function ZnacznikiWidoczne(warstwa As GMapOverlay) As Boolean
        Const margines As Integer = 10
        Dim zaslony As New List(Of Rectangle)
        For Each c As Control In New Control() {btnPowieksz, btnPomniejsz, lblZoom, rbGoogle, rbBing, rbOsm,
                                                lblKursorTytul, lblKursorUkladOpis, lblKursorUklad, lblSrodekTytul,
                                                lblSrodekOpisSzer, lblSrodekSzer, lblSrodekOpisDlug, lblSrodekDlug}
            If c.Visible Then zaslony.Add(mapa.RectangleToClient(c.RectangleToScreen(c.ClientRectangle)))
        Next
        For Each m In warstwa.Markers
            Dim p = mapa.FromLatLngToLocal(m.Position)
            If p.X < margines OrElse p.X > mapa.Width - margines OrElse
               p.Y < m.Size.Height + margines OrElse p.Y > mapa.Height - margines Then Return False
            'obszar ikony: pinezka leży nad wskazywanym punktem, uchwyt - wokół niego
            Dim ikona As Rectangle = If(TypeOf m Is UchwytSiatki,
                                        New Rectangle(CInt(p.X) - m.Size.Width \ 2, CInt(p.Y) - m.Size.Height \ 2, m.Size.Width, m.Size.Height),
                                        New Rectangle(CInt(p.X) - m.Size.Width \ 2, CInt(p.Y) - m.Size.Height, m.Size.Width, m.Size.Height))
            For Each z In zaslony
                If z.IntersectsWith(ikona) Then Return False
            Next
        Next
        Return True
    End Function

#End Region

#Region "Obszar wzdłuż trasy"

    ''' <summary>Siatka segmentów dla zasięgu przy bieżącym rozmiarze piksela i boku segmentu (okno wyboru trasy).</summary>
    Public Function SiatkaDlaZasiegu(z As Zasieg) As Siatka
        Return New Siatka(ZaokraglonyZasieg(z), Wartosc(txtRozmiarPiksela.Text), WartoscCalkowita(txtBokSegmentu.Text))
    End Function

    ''' <summary>Zasięg zaokrąglony jak przy zaznaczaniu na mapie: metry do pełnych wartości na zewnątrz, stopnie do 6 miejsc.</summary>
    Private Shared Function ZaokraglonyZasieg(z As Zasieg) As Zasieg
        If Ustawienia.Uklad.Geograficzny Then
            Return New Zasieg(Math.Round(z.XDol, 6), Math.Round(z.YLewy, 6), Math.Round(z.XGora, 6), Math.Round(z.YPrawy, 6))
        End If
        Return New Zasieg(Math.Floor(z.XDol), Math.Floor(z.YLewy), Math.Ceiling(z.XGora), Math.Ceiling(z.YPrawy))
    End Function

    Private Sub KorytarzToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KorytarzToolStripMenuItem.Click
        If _przerwanie IsNot Nothing Then Exit Sub
        Using okno As New FormKorytarz(_korytarz)
            If okno.ShowDialog(Me) = DialogResult.OK AndAlso okno.Wynik IsNot Nothing Then UstawKorytarz(okno.Wynik)
        End Using
    End Sub

    Private Sub UsunKorytarzToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsunKorytarzToolStripMenuItem.Click
        If PobieranieTrwa("Usunięcie obszaru wzdłuż trasy") Then Exit Sub
        UsunKorytarz(True)
    End Sub

    ''' <summary>
    ''' Ustawia obszar pobierania wzdłuż trasy: zasięg pobierania obejmuje cały korytarz, a pobierane są tylko segmenty,
    ''' które go przecinają.
    ''' </summary>
    Public Sub UstawKorytarz(k As Korytarz)
        _korytarz = k
        UsunKorytarzToolStripMenuItem.Enabled = True
        mapa.SelectedArea = RectLatLng.Empty
        Dim z = ZaokraglonyZasieg(k.Zasieg(Ustawienia.Uklad))
        txtXDol.Text = Liczba(z.XDol)
        txtYLewy.Text = Liczba(z.YLewy)
        txtXGora.Text = Liczba(z.XGora)
        txtYPrawy.Text = Liczba(z.YPrawy)
        WyczyscZnaczniki()
        PokazZnaczniki()
        'widok mapy obejmujący całą trasę
        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            Dim g = ProstokatGeo.Opisany(Ustawienia.Uklad, s.ZasiegSiatki)
            mapa.SetZoomToFitRect(New RectLatLng(g.Polnoc, g.Zachod, g.Wschod - g.Zachod, g.Polnoc - g.Poludnie))
        End If
        Komunikat(OpisKorytarza(), Color.Green)
    End Sub

    ''' <summary>Usuwa obszar wzdłuż trasy - pobierany będzie cały prostokąt.</summary>
    Private Sub UsunKorytarz(komunikatOUsunieciu As Boolean)
        If _korytarz Is Nothing Then Exit Sub
        _korytarz = Nothing
        UsunKorytarzToolStripMenuItem.Enabled = False
        WyczyscZnaczniki()
        If komunikatOUsunieciu Then
            PokazZnaczniki()
            Komunikat("Usunięto obszar wzdłuż trasy - pobierany będzie cały prostokąt.", Color.Green)
        End If
    End Sub

    ''' <summary>Opis korytarza: trasa, szerokość pasa i liczba pobieranych segmentów.</summary>
    Private Function OpisKorytarza() As String
        If _korytarz Is Nothing Then Return ""
        Dim s = BiezacaSiatka()
        Dim opis As String = "Obszar wzdłuż trasy " & If(_korytarz.Nazwa <> "", "'" & _korytarz.Nazwa & "' ", "") &
                             "(" & _korytarz.DlugoscKm.ToString("0.0") & " km, pas " & Liczba(_korytarz.SzerokoscMetry) & " m po obu stronach trasy)"
        If Not s.Poprawna Then Return opis
        Dim szacunek As Double = _korytarz.SzacowanaLiczbaSegmentow(s, Ustawienia.Uklad)
        If szacunek > MaksSegmentowKorytarzaNaBiezaco Then
            Return opis & ": ok. " & Math.Round(szacunek).ToString("N0") & " segmentów (szacunek - przy tak małym segmencie dokładna liczba nie jest liczona)"
        End If
        Dim n As Integer = _korytarz.Segmenty(s, Ustawienia.Uklad).Count
        Dim wszystkie As Long = s.LiczbaSegmentow
        Return opis & ": pobranych zostanie " & n & " z " & wszystkie & " segmentów prostokąta" &
               If(wszystkie > 0, " (" & Math.Round(100.0 * (wszystkie - n) / wszystkie) & "% mniej danych)", "")
    End Function

    ''' <summary>Rysuje na mapie trasę i segmenty korytarza.</summary>
    Private Sub PokazKorytarz()
        If _korytarz Is Nothing Then Exit Sub
        Dim warstwa As New GMapOverlay("korytarz")
        mapa.Overlays.Add(warstwa)
        Dim s = BiezacaSiatka()
        'segmenty rysowane, gdy jest ich niewiele - wyznaczanie milionów segmentów (np. w trakcie wpisywania boku
        'segmentu, gdy po pierwszej cyfrze segment ma 2 piksele) zawieszało program na dziesiątki sekund
        If s.Poprawna AndAlso _korytarz.SzacowanaLiczbaSegmentow(s, Ustawienia.Uklad) <= MaksSegmentowKorytarzaNaBiezaco Then
            Dim u = Ustawienia.Uklad
            Dim zbior = _korytarz.Segmenty(s, u)
            If zbior.Count <= MaksSegmentowNaMapie Then
                For Each klucz In zbior
                    Dim z = s.Segment(klucz \ s.LiczbaKolumn + 1, klucz Mod s.LiczbaKolumn + 1)
                    Dim punkty As New List(Of PointLatLng)
                    For Each n In {z.LewyGorny, z.PrawyGorny, z.PrawyDolny, z.LewyDolny}
                        Dim g = u.DoWgs84(n)
                        punkty.Add(New PointLatLng(g.Szerokosc, g.Dlugosc))
                    Next
                    warstwa.Polygons.Add(New GMapPolygon(punkty, "segment") With {
                        .Fill = New SolidBrush(Color.FromArgb(40, Color.RoyalBlue)), .Stroke = New Pen(Color.FromArgb(140, Color.RoyalBlue), 1)})
                Next
            End If
        End If
        For Each l In _korytarz.Linie
            Dim punkty = l.ConvertAll(Function(p) New PointLatLng(p.Szerokosc, p.Dlugosc))
            warstwa.Routes.Add(New GMapRoute(punkty, "trasa") With {.Stroke = New Pen(Color.Red, 2)})
        Next
    End Sub

#End Region

#Region "Przyciski i menu"

    Private Sub btnOtworzFolder_Click(sender As Object, e As EventArgs) Handles btnOtworzFolder.Click
        Directory.CreateDirectory(FolderDownload)
        OtworzFolder(FolderDownload)
    End Sub

    Private Sub btnUsunPobrane_Click(sender As Object, e As EventArgs) Handles btnUsunPobrane.Click
        If PobieranieTrwa("Usunięcie pobranych map") Then Exit Sub
        Dim odp As DialogResult = MessageBox.Show("Czy na pewno usunąć cały katalog 'download' z pobranymi mapami?", "Usuwanie zawartości katalogu download",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If odp = DialogResult.Yes Then
            Try
                If Directory.Exists(FolderDownload) Then Directory.Delete(FolderDownload, True)
                Directory.CreateDirectory(FolderDownload)
                Komunikat("Usunięto całą zawartość katalogu download", Color.Green)
            Catch ex As Exception
                Komunikat("Nie udało się usunąć zawartości katalogu download: " & ex.Message, Color.Red)
            End Try
        End If
    End Sub

    Private Sub btnScalanie_Click(sender As Object, e As EventArgs) Handles btnScalanie.Click
        If PobieranieTrwa("Scalanie segmentów") Then Exit Sub
        Form3.ShowDialog()
    End Sub

    Private Sub DodajSerwerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DodajSerwerToolStripMenuItem.Click, btnDodajSerwer.Click
        'wczytanie listy zbiorów map po dodaniu serwera zmienia wybrany zbiór - to przerywało pobieranie i czyściło warstwy
        If PobieranieTrwa("Dodanie serwera") Then Exit Sub
        Using okno As New FormDodajSerwer()
            If okno.ShowDialog(Me) = DialogResult.OK AndAlso Not String.IsNullOrEmpty(okno.WynikNazwaZbioru) Then
                WczytajListeZbiorowMap(okno.WynikNazwaZbioru)
                Komunikat("Dodano zbiór map: " & okno.WynikNazwaZbioru, Color.Green)
            End If
        End Using
    End Sub

    Private Sub ScalanieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScalanieToolStripMenuItem.Click
        If PobieranieTrwa("Scalanie segmentów") Then Exit Sub
        Form3.ShowDialog()
    End Sub

    Private Sub EksportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EksportToolStripMenuItem.Click
        FormEksport.ShowDialog()
    End Sub

    Private Sub ZaawansowanyMbtilesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZaawansowanyMbtilesToolStripMenuItem.Click
        FormZaawansowanyMbtiles.ShowDialog()
    End Sub

    Private Sub UstawieniaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UstawieniaToolStripMenuItem.Click
        'okno ustawień zmienia parametry sesji (przedrostek, format, numeracja, bok segmentu TrekBuddy)
        If PobieranieTrwa("Zmiana ustawień") Then Exit Sub
        Form2.ShowDialog()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.ShowDialog()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        pomoc_pfe.OtworzWPrzegladarce()
    End Sub

    Private Sub InstrukcjaObslugiToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_Obslugi.ShowDialog()
    End Sub

    Private Sub UsuwaniePustychSegmentowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsuwaniePustychSegmentówToolStripMenuItem.Click
        If PobieranieTrwa("Usuwanie pustych segmentów") Then Exit Sub
        Usuwanie_p_seg.ShowDialog()
    End Sub

    Private Sub NakladanieWarstwToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NakładanieWarstwNaSiebieToolStripMenuItem.Click
        'okno nakładania zmienia folder sesji pobierania
        If PobieranieTrwa("Nakładanie map") Then Exit Sub
        Nakladanie_Map.ShowDialog()
    End Sub

#End Region

End Class
