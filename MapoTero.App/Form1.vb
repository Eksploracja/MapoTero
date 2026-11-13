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

    ''' <summary>Wybrane warstwy mapy z kryciem (maksymalnie 12) - „kanapka” wyświetlana w lstWybraneWarstwy.</summary>
    Private _zestaw As New ZestawWarstw()

    ''' <summary>Programowe ustawianie pola krycia (bez zmiany krycia warstwy).</summary>
    Private _ustawianieKrycia As Boolean

    ''' <summary>Menu warstwy na liście wybranych warstw (prawy przycisk myszy).</summary>
    Private _menuWarstwy As ContextMenuStrip

    ''' <summary>
    ''' Rozmiar piksela wpisany ostatnio przez program wg najdokładniejszej warstwy kanapki: w metrach i jako tekst pola
    ''' (0 / pusty - brak). Gdy pole nadal go zawiera, program przelicza je po zmianie warstw; wartości wpisanej ręcznie nie zmienia.
    ''' </summary>
    Private _automatycznyPikselMetry As Double
    Private _automatycznyPikselTekst As String = ""

    ''' <summary>Katalog warstw: grupy tematyczne drzewa i nazwy wyświetlane warstw (warstwy\katalog.txt).</summary>
    Private _katalog As New KatalogWarstw()

    ''' <summary>Nazwy wyświetlane i grupy warstw według serwera i nazwy warstwy (<see cref="KatalogWarstw.MapaNazw"/>).</summary>
    Private _nazwyWarstw As New Dictionary(Of String, (Nazwa As String, Grupa As String))

    ''' <summary>Czytelne nazwy warstw i zbiorów zapisane w plikach zbiorów dodanych przez użytkownika.</summary>
    Private _tytulyDodanychWarstw As New Dictionary(Of String, (Nazwa As String, Grupa As String))(StringComparer.Ordinal)

    Private Const GrupaDodanychSerwerow As String = "Dodane serwery"

    ''' <summary>Zbiory map z warstwy.txt w kolejności drzewa warstw.</summary>
    Private ReadOnly _zbiory As New List(Of ZbiorMapy)

    ''' <summary>
    ''' Ostatnio wskazany zbiór map (kliknięty w drzewie, z conf.txt) - nazwa sesji, gdy kanapka jest pusta
    ''' (dawniej wybrany na liście zbiorów map).
    ''' </summary>
    Private _biezacyZbior As String = DomyslnyZbiorMap
    Private Const DomyslnyZbiorMap As String = "skany_map_topograficznych"

    ''' <summary>Grupy drzewa rozwinięte przez użytkownika - klucze węzłów (zachowane przy przebudowie drzewa i po wyczyszczeniu wyszukiwania).</summary>
    Private ReadOnly _rozwinieteZbiory As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Programowe ustawianie pól wyboru i rozwinięć drzewa (bez dodawania/usuwania warstw).</summary>
    Private _synchronizacjaDrzewa As Boolean

    ''' <summary>Przerywanie trwającego pobierania.</summary>
    Private _przerwanie As CancellationTokenSource

    ''' <summary>Programowe ustawianie listy układów (bez przeliczania zasięgu).</summary>
    Private _ustawianieUkladu As Boolean

    ''' <summary>Szerokość prawego panelu wybrana przez użytkownika (bez paska przewijania) i jego najmniejsza wysokość, przy której nie jest przewijany.</summary>
    Private _szerokoscPanelu As Integer
    Private _minWysokoscPanelu As Integer

    ''' <summary>Najmniejsza szerokość prawego panelu (z projektu formularza) i obszaru mapy z dolnym panelem siatki.</summary>
    Private _minSzerokoscPanelu As Integer
    Private _minSzerokoscMapy As Integer

    ''' <summary>Elementy leżące na mapie przy jej prawej krawędzi i ich odległość od tej krawędzi (<see cref="PrzypnijNakladkiDoMapy"/>).</summary>
    Private ReadOnly _nakladkiMapy As New List(Of (Kontrolka As Control, OdPrawejMapy As Integer))

    ''' <summary>Wysokość prawego panelu przy ostatnim dopasowaniu jego szerokości.</summary>
    Private _wysokoscPanelu As Integer

    ''' <summary>Liczba wierszy drzewa warstw widocznych w najmniejszym oknie.</summary>
    Private Const MinWierszyListyWarstw As Integer = 8

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
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
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
            If _zestaw.Liczba > 0 AndAlso _zestaw(0).Zbior <> "" Then Return _zestaw(0).Zbior
            Return _biezacyZbior
        End Get
    End Property

    ''' <summary>Nazwa pierwszej wybranej warstwy.</summary>
    Public ReadOnly Property PierwszaWarstwa As String
        Get
            Return If(_zestaw.Liczba > 0, _zestaw(0).Nazwa, "")
        End Get
    End Property

    ''' <summary>Czy na mapie wyświetlona jest siatka segmentów lub korytarz.</summary>
    Public ReadOnly Property SiatkaWidoczna As Boolean
        Get
            Return _siatkaWidoczna
        End Get
    End Property

    ''' <summary>Bok segmentu w pikselach (pole formularza).</summary>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BokSegmentu As String
        Get
            Return txtBokSegmentu.Text
        End Get
        Set(value As String)
            txtBokSegmentu.Text = value
        End Set
    End Property

    ''' <summary>Rozmiar piksela (pole formularza).</summary>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
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

    ''' <summary>Wskazuje zbiór map: rozwija go w drzewie warstw i zaznacza.</summary>
    Public Sub UstawZbiorMap(nazwa As String)
        _biezacyZbior = nazwa
        RozwinZbioryWybranych(nazwa)
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
        Me.Text = My.Application.Info.Title & " " & Program.Wersja

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
        PrzygotujListeWarstw()

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
        UstawSzerokoscPanelu(Ustawienia.SzerokoscPanelu)

        'drzewo zbiorów map - warstwy wczytane z conf.txt zostają (każda pamięta swój serwer, więc zbiór sesji na liście
        'ich nie czyści; dawniej po ponownym uruchomieniu warstwy sesji znikały i trzeba je było wybrać od nowa)
        WczytajListeZbiorowMap(_biezacyZbior)
        If _zestaw.Liczba > 0 Then
            Komunikat("Wczytano sesję pobierania z pliku conf.txt, warstwy: " & OpisWarstw() & ".", Color.Green)
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
        Dim minWysokoscListy As Integer = tvWarstwy.ItemHeight * MinWierszyListyWarstw + (tvWarstwy.Height - tvWarstwy.ClientSize.Height)
        Dim zapasListy As Integer = Math.Max(0, tvWarstwy.Height - minWysokoscListy)
        _minSzerokoscPanelu = Panel2.Width
        _minSzerokoscMapy = ClientSize.Width - Panel2.Width - splPanel.Width
        _szerokoscPanelu = _minSzerokoscPanelu
        PrzypnijNakladkiDoMapy()
        _minWysokoscPanelu = Panel2.ClientSize.Height - zapasListy
        Panel2.AutoScrollMinSize = New Size(0, _minWysokoscPanelu)
        splPanel.MinSize = _minSzerokoscPanelu
        splPanel.MinExtra = _minSzerokoscMapy
        ToolTip1.SetToolTip(splPanel, "Przeciągnij, aby zmienić szerokość prawego panelu")

        Dim ekran As Rectangle = Screen.FromControl(Me).WorkingArea
        Me.MinimumSize = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height - zapasListy, ekran.Height))
        Me.Size = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height, ekran.Height))
    End Sub

    ''' <summary>Szerokość prawego panelu zapamiętana w lastsettings.txt (0 - domyślna).</summary>
    Private Sub UstawSzerokoscPanelu(szerokosc As Integer)
        _szerokoscPanelu = Math.Max(_minSzerokoscPanelu, szerokosc)
        DopasujSzerokoscPanelu()
    End Sub

    ''' <summary>
    ''' Szerokość prawego panelu: wybrana przez użytkownika, ale w węższym oknie zmniejszona tak, żeby mapa z panelem
    ''' siatki pod nią się zmieściła (po poszerzeniu okna panel wraca do wybranej szerokości). Przewijany panel
    ''' (ekran niższy niż najmniejsze okno) poszerza się o pasek przewijania.
    ''' </summary>
    Private Sub DopasujSzerokoscPanelu()
        If _szerokoscPanelu = 0 Then Exit Sub
        Dim pasek As Integer = If(Panel2.Height < _minWysokoscPanelu, SystemInformation.VerticalScrollBarWidth, 0)
        Dim dostepna As Integer = ClientSize.Width - splPanel.Width - _minSzerokoscMapy - pasek
        Dim szerokosc As Integer = Math.Max(_minSzerokoscPanelu, Math.Min(_szerokoscPanelu, dostepna)) + pasek
        If Panel2.Width <> szerokosc Then Panel2.Width = szerokosc
    End Sub

    ''' <summary>
    ''' Tylko zmiana wysokości panelu (pasek przewijania). Szerokość zmienia Splitter, a wybraną szerokość podaje
    ''' dopiero po niej w SplitterMoved - dopasowanie przy zmianie szerokości przywracałoby poprzednią. (Znacznik
    ''' przeciągania kasowany w MouseUp paska nie wystarcza: Splitter zgłasza MouseUp przed zmianą szerokości.)
    ''' </summary>
    Private Sub Panel2_SizeChanged(sender As Object, e As EventArgs) Handles Panel2.SizeChanged
        If Panel2.Height = _wysokoscPanelu Then Exit Sub
        _wysokoscPanelu = Panel2.Height
        DopasujSzerokoscPanelu()
    End Sub

    ''' <summary>
    ''' Przełączniki podkładu i współrzędne środka mapy leżą na formularzu nad mapą, w projekcie zakotwiczone do prawej
    ''' krawędzi okna. Przy szerszym prawym panelu zasłaniałyby go, więc trzymają się prawej krawędzi mapy: kotwica
    ''' zmieniona na lewą, a położenie ustawiane po każdym ułożeniu okna. (Przesuwanie ich z kotwicą do prawej w czasie
    ''' zmiany rozmiaru okna nie działa - WinForms przelicza je potem jeszcze raz według kotwicy.)
    ''' </summary>
    Private Sub PrzypnijNakladkiDoMapy()
        For Each c As Control In Me.Controls
            If c.Dock = DockStyle.None AndAlso (c.Anchor And AnchorStyles.Right) <> 0 AndAlso (c.Anchor And AnchorStyles.Left) = 0 Then
                _nakladkiMapy.Add((c, mapa.Right - c.Left))
                c.Anchor = (c.Anchor And Not AnchorStyles.Right) Or AnchorStyles.Left
            End If
        Next
    End Sub

    Private Sub Form1_Layout(sender As Object, e As LayoutEventArgs) Handles MyBase.Layout
        For Each n In _nakladkiMapy
            Dim lewa As Integer = mapa.Right - n.OdPrawejMapy
            If n.Kontrolka.Left <> lewa Then n.Kontrolka.Left = lewa
        Next
    End Sub

    ''' <summary>
    ''' Zmiana rozmiaru okna. Formularz nie może mieć AutoScroll: po poszerzeniu panelu pokazywał poziomy pasek
    ''' przewijania, a zgłoszony przez to Resize cofał szerokość panelu (na niskim ekranie przewija się sam panel).
    ''' Szerokość panelu jest dopasowywana przed ułożeniem okna (MyBase.OnResize): dopasowana dopiero w zdarzeniu
    ''' Resize, po przywróceniu zmaksymalizowanego okna panel był najpierw układany w starej szerokości, więc przez
    ''' chwilę było widać jego prawą część przesuniętą w lewo, zanim się przerysował.
    ''' </summary>
    Protected Overrides Sub OnResize(e As EventArgs)
        If WindowState <> FormWindowState.Minimized Then DopasujSzerokoscPanelu()
        MyBase.OnResize(e)
    End Sub

    ''' <summary>Kursor nad paskiem między mapą a prawym panelem (pasek podświetlony).</summary>
    Private _kursorNadPaskiem As Boolean

    ''' <summary>
    ''' Pasek między mapą a prawym panelem: krawędzie i uchwyt z kropek na środku, żeby było widać, że panel można
    ''' poszerzyć; pod kursorem pasek jest podświetlony.
    ''' </summary>
    Private Sub splPanel_Paint(sender As Object, e As PaintEventArgs) Handles splPanel.Paint
        Dim g As Graphics = e.Graphics
        Dim w As Integer = splPanel.Width, h As Integer = splPanel.Height
        g.Clear(If(_kursorNadPaskiem, SystemColors.Highlight, ControlPaint.Dark(SystemColors.Control, 0.05F)))
        Using krawedz As New Pen(SystemColors.ControlDark)
            g.DrawLine(krawedz, 0, 0, 0, h)
            g.DrawLine(krawedz, w - 1, 0, w - 1, h)
        End Using
        'uchwyt: dwie kolumny kropek na przemian, jak w uchwytach pasków narzędzi
        Dim kropka As Integer = Math.Max(1, (w - 2) \ 2)
        Dim wiersze As Integer = 12
        Dim y As Integer = (h - wiersze * kropka * 2) \ 2
        Using pedzel As New SolidBrush(If(_kursorNadPaskiem, SystemColors.HighlightText, SystemColors.ControlDarkDark))
            For i = 0 To wiersze - 1
                g.FillRectangle(pedzel, 1 + (i Mod 2) * kropka, y + i * kropka * 2, kropka, kropka)
            Next
        End Using
    End Sub

    Private Sub splPanel_MouseEnter(sender As Object, e As EventArgs) Handles splPanel.MouseEnter
        _kursorNadPaskiem = True
        splPanel.Invalidate()
    End Sub

    Private Sub splPanel_MouseLeave(sender As Object, e As EventArgs) Handles splPanel.MouseLeave
        _kursorNadPaskiem = False
        splPanel.Invalidate()
    End Sub

    ''' <summary>Szerokość wybrana przeciągnięciem paska - zapamiętana w lastsettings.txt.</summary>
    Private Sub splPanel_SplitterMoved(sender As Object, e As SplitterEventArgs) Handles splPanel.SplitterMoved
        Dim pasek As Integer = If(Panel2.Height < _minWysokoscPanelu, SystemInformation.VerticalScrollBarWidth, 0)
        _szerokoscPanelu = Math.Max(_minSzerokoscPanelu, Panel2.Width - pasek)
        Ustawienia.SzerokoscPanelu = If(_szerokoscPanelu = _minSzerokoscPanelu, 0, _szerokoscPanelu)
        DopasujSzerokoscPanelu()
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

        'zbiór map sesji - nazwa sesji przy pustej kanapce, rozwijany w drzewie warstw
        If c.RodzajMapy <> "" Then _biezacyZbior = c.RodzajMapy
        Ustawienia.Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
        txtXDol.Text = c.XDol
        txtYLewy.Text = c.YLewy
        txtXGora.Text = c.XGora
        txtYPrawy.Text = c.YPrawy
        txtBokSegmentu.Text = c.BokSegmentuPx
        txtRozmiarPiksela.Text = c.RozmiarPiksela

        _zestaw = c.WybranyZestaw()
        'rozmiar piksela sesji równy zalecanemu dla najdokładniejszej warstwy - nadal dobierany automatycznie
        UzupelnijZalecanePiksele()
        Dim najdokladniejsza = _zestaw.NajdokladniejszaWarstwa()
        _automatycznyPikselMetry = If(najdokladniejsza IsNot Nothing, najdokladniejsza.ZalecanyPiksel, 0)
        _automatycznyPikselTekst = If(najdokladniejsza IsNot Nothing, RozmiarPikselaWarstwy(Liczba(najdokladniejsza.ZalecanyPiksel)), "")
        PokazWarstwy()
        If _zestaw.Liczba > 0 AndAlso lstWybraneWarstwy.Zaznaczona < 0 Then
            lstWybraneWarstwy.Zaznaczona = 0
        End If
        RozwinZbioryWybranych(_biezacyZbior)

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
            .Folder = FolderSesji, .RodzajMapy = If(_zestaw.Liczba > 0 AndAlso _zestaw(0).Zbior <> "", _zestaw(0).Zbior, _biezacyZbior),
            .XDol = txtXDol.Text, .YLewy = txtYLewy.Text, .XGora = txtXGora.Text, .YPrawy = txtYPrawy.Text,
            .BokSegmentuPx = txtBokSegmentu.Text, .RozmiarPiksela = txtRozmiarPiksela.Text,
            .LiczbaWarstw = _zestaw.Liczba, .KrycieWarstw = _zestaw.TekstKrycia(), .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .SrodekMapySzerokosc = lblSrodekSzer.Text.Replace(","c, "."c), .SrodekMapyDlugosc = lblSrodekDlug.Text.Replace(","c, "."c),
            .SkalaMapy = lblZoom.Text.Replace(","c, "."c), .Numeracja = Ustawienia.Numeracja, .UkladEpsg = Ustawienia.Uklad.Epsg,
            .Przezroczystosc = EfektywnaPrzezroczystosc(), .DataPobrania = _dataPobrania,
            .AdresSerwera = If(_zestaw.Liczba > 0, _zestaw(0).Adres, If(ZnajdzZbior(_biezacyZbior)?.Plik?.Adres, "")),
            .AdresyWarstw = ZestawWarstw.Polacz(_zestaw.Adresy()), .ZbioryWarstw = ZestawWarstw.Polacz(_zestaw.Zbiory()),
            .TrybyMieszaniaWarstw = _zestaw.TekstTrybowMieszania()}
        For i = 0 To _zestaw.Liczba - 1
            c.Warstwy(i) = _zestaw(i).Nazwa
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
    Friend Function PobieranieTrwa(czynnosc As String) As Boolean
        If _przerwanie Is Nothing Then Return False
        MessageBox.Show(czynnosc & " będzie możliwe po zakończeniu lub przerwaniu pobierania segmentów.", "Trwa pobieranie",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
        Return True
    End Function

#End Region

#Region "Zbiory map i warstwy"

    ''' <summary>
    ''' Wczytuje zbiory map z plików warstwy\warstwy.txt (folder programu i folder danych), katalog grup i nazw warstw
    ''' (warstwy\katalog.txt) i buduje drzewo warstw. Zbiór <paramref name="pokazZbior"/> (np. dodany kreatorem) jest
    ''' rozwijany i wskazywany w drzewie.
    ''' </summary>
    Public Sub WczytajListeZbiorowMap(Optional pokazZbior As String = "")
        Dim znalezione As New List(Of String)()
        Dim dodane As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        '1. zbiory w folderze programu
        DodajZbioryZIndeksu(Path.Combine(FolderWarstwProgramu, "warstwy.txt"), znalezione, dodane)
        '2. zbiory w folderze danych użytkownika (jeśli inny niż programu)
        If Not String.Equals(FolderWarstwProgramu, FolderWarstwDanych, StringComparison.OrdinalIgnoreCase) Then
            DodajZbioryZIndeksu(Path.Combine(FolderWarstwDanych, "warstwy.txt"), znalezione, dodane)
        End If

        'wszystkie pliki od razu - są małe (kilkadziesiąt plików po kilkadziesiąt wierszy), a wyszukiwanie warstw,
        'liczniki wybranych warstw i „rozwiń wszystkie” potrzebują zawartości każdego zbioru
        _zbiory.Clear()
        For Each nazwa In znalezione
            Dim z As New ZbiorMapy With {.Nazwa = nazwa}
            Dim plik As String = SciezkaZbioruMap(nazwa)
            If plik = "" Then
                z.Blad = "brak pliku " & nazwa & ".txt w folderze warstwy"
            Else
                Try
                    z.Plik = PlikWarstw.Wczytaj(plik)
                Catch ex As Exception
                    z.Blad = ex.Message
                End Try
            End If
            _zbiory.Add(z)
        Next
        WczytajKatalog()

        If _zbiory.Count = 0 Then
            Komunikat("Brak plików z listą zbiorów map.", Color.Red)
        ElseIf ZnajdzZbior(_biezacyZbior) Is Nothing Then
            _biezacyZbior = If(ZnajdzZbior(DomyslnyZbiorMap) IsNot Nothing, DomyslnyZbiorMap, _zbiory(0).Nazwa)
        End If
        If pokazZbior <> "" AndAlso ZnajdzZbior(pokazZbior) IsNot Nothing Then _biezacyZbior = pokazZbior

        BudujDrzewo()
        RozwinZbioryWybranych(_biezacyZbior)
        PokazWarstwy()
    End Sub

    ''' <summary>
    ''' Katalog grup i nazw warstw (folder programu, potem folder danych) oraz nazwy wyświetlane warstw kanapki.
    ''' Brak katalogu - drzewo pokazuje zbiory map z nazwami z plików (grupa „Dodane serwery”).
    ''' </summary>
    Private Sub WczytajKatalog()
        _katalog = New KatalogWarstw()
        Dim plik As String = Path.Combine(FolderWarstwProgramu, KatalogWarstw.NazwaPliku)
        If Not File.Exists(plik) Then plik = Path.Combine(FolderWarstwDanych, KatalogWarstw.NazwaPliku)
        If File.Exists(plik) Then
            Try
                _katalog = KatalogWarstw.Wczytaj(plik)
                If _katalog.Bledy.Count > 0 Then
                    Komunikat("Pominięto błędne wiersze katalogu warstw (" & _katalog.Bledy.Count & "): " & _katalog.Bledy(0), Color.Red)
                End If
            Catch ex As Exception
                Komunikat("Nie udało się odczytać katalogu warstw: " & ex.Message, Color.Red)
            End Try
        End If
        Dim pliki As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        _tytulyDodanychWarstw.Clear()
        For Each z In _zbiory
            If z.Plik IsNot Nothing Then
                pliki(z.Nazwa) = z.Plik
                Dim maCzytelnyZbior = Not String.IsNullOrWhiteSpace(z.Plik.Tytul)
                Dim nazwaZbioru As String = If(maCzytelnyZbior, z.Plik.Tytul, z.Nazwa)
                For Each wm In z.Plik.Warstwy
                    Dim maCzytelnaWarstwe = Not String.IsNullOrWhiteSpace(wm.Tytul)
                    If Not maCzytelnyZbior AndAlso Not maCzytelnaWarstwe Then Continue For
                    Dim klucz = KatalogWarstw.KluczAliasu(z.Plik.Adres, wm.Nazwa)
                    Dim nazwaW = If(maCzytelnaWarstwe, wm.Tytul, wm.Nazwa)
                    If Not _tytulyDodanychWarstw.ContainsKey(klucz) Then
                        _tytulyDodanychWarstw(klucz) = (nazwaW, nazwaZbioru)
                    End If
                Next
            End If
        Next
        _nazwyWarstw = _katalog.MapaNazw(pliki)
    End Sub

    ''' <summary>Nazwa warstwy kanapki do wyświetlania: z katalogu warstw albo z tytułu w pliku zbioru, inaczej nazwa na serwerze.</summary>
    Friend Function NazwaWarstwy(w As WarstwaWybrana) As String
        Dim n As (Nazwa As String, Grupa As String) = Nothing
        Dim klucz = KatalogWarstw.KluczAliasu(w.Adres, w.Nazwa)
        If _nazwyWarstw.TryGetValue(klucz, n) Then Return n.Nazwa
        If _tytulyDodanychWarstw.TryGetValue(klucz, n) Then Return n.Nazwa
        Return w.Nazwa
    End Function

    ''' <summary>Źródło warstwy kanapki do wyświetlania: grupa z katalogu warstw albo tytuł zbioru, inaczej nazwa pliku zbioru.</summary>
    Friend Function ZrodloWarstwy(w As WarstwaWybrana) As String
        Dim n As (Nazwa As String, Grupa As String) = Nothing
        Dim klucz = KatalogWarstw.KluczAliasu(w.Adres, w.Nazwa)
        If _nazwyWarstw.TryGetValue(klucz, n) Then Return n.Grupa
        If _tytulyDodanychWarstw.TryGetValue(klucz, n) Then Return n.Grupa
        Return w.Zbior
    End Function

    Private Sub DodajZbioryZIndeksu(plik As String, lista As List(Of String), dodane As HashSet(Of String))
        If Not File.Exists(plik) Then Exit Sub
        For Each linia In File.ReadAllLines(plik, PlikWarstw.Kodowanie())
            Dim nazwa As String = linia.Trim().Trim(""""c)
            If nazwa <> "" AndAlso dodane.Add(nazwa) Then lista.Add(nazwa)
        Next
    End Sub

    ''' <summary>Ścieżka pliku zbioru map (folder programu, potem folder danych); pusty tekst - brak pliku.</summary>
    Private Function SciezkaZbioruMap(zbior As String) As String
        Dim plik As String = Path.Combine(FolderWarstwProgramu, zbior & ".txt")
        If File.Exists(plik) Then Return plik
        plik = Path.Combine(FolderWarstwDanych, zbior & ".txt")
        Return If(File.Exists(plik), plik, "")
    End Function

    ''' <summary>Zbiór map wczytany do drzewa (Nothing - brak).</summary>
    Private Function ZnajdzZbior(nazwa As String) As ZbiorMapy
        Return _zbiory.FirstOrDefault(Function(z) String.Equals(z.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>Czy wyszukiwanie warstw jest włączone (pole wyszukiwania zawiera słowa).</summary>
    Private ReadOnly Property SzukanieWarstw As Boolean
        Get
            Return FiltrWarstw.Slowa(txtSzukajWarstwy.Text).Length > 0
        End Get
    End Property

    ''' <summary>
    ''' Drzewo warstw: grupy i podgrupy z katalogu warstw, w nich warstwy z polami wyboru (nazwy z katalogu), na końcu
    ''' grupa „Dodane serwery” ze zbiorami i warstwami spoza katalogu. Przy wyszukiwaniu - tylko pasujące warstwy
    ''' (do frazy pasują nazwy grup, nazwy warstw i nazwy techniczne), wszystkie grupy rozwinięte. Pola wyboru
    ''' odpowiadają warstwom kanapki (<see cref="OdswiezZaznaczeniaDrzewa"/>). Klucz węzła (Name) - ścieżka grup.
    ''' </summary>
    Private Sub BudujDrzewo()
        Dim fraza As String = txtSzukajWarstwy.Text
        Dim szukanie As Boolean = SzukanieWarstw
        Dim kluczZaznaczonego As String = If(tvWarstwy.SelectedNode?.Name, "")

        _synchronizacjaDrzewa = True
        tvWarstwy.BeginUpdate()
        Try
            tvWarstwy.Nodes.Clear()
            For Each g In _katalog.Grupy
                Dim wezel = WezelGrupy(g, "", "", fraza, szukanie)
                If wezel IsNot Nothing Then tvWarstwy.Nodes.Add(wezel)
            Next
            Dim dodane = WezelDodanychSerwerow(fraza, szukanie)
            If dodane IsNot Nothing Then tvWarstwy.Nodes.Add(dodane)

            For Each wezel In tvWarstwy.WszystkieWezly().Where(Function(w) DrzewoWarstw.CzyGrupa(w)).ToList()
                If szukanie OrElse _rozwinieteZbiory.Contains(wezel.Name) Then wezel.Expand()
            Next

            'zaznaczenie sprzed przebudowy (np. po wpisaniu kolejnej litery w polu wyszukiwania)
            If kluczZaznaczonego <> "" Then
                Dim znalezione = tvWarstwy.Nodes.Find(kluczZaznaczonego, True)
                If znalezione.Length > 0 Then tvWarstwy.SelectedNode = znalezione(0)
            End If
        Finally
            tvWarstwy.EndUpdate()
            _synchronizacjaDrzewa = False
        End Try
        tvWarstwy.UkryjPolaGrup()
        OdswiezZaznaczeniaDrzewa()
        OdswiezPrzyciskZwijania()
    End Sub

    ''' <summary>Węzeł grupy katalogu z podgrupami i warstwami (Nothing - grupa pusta, np. nic nie pasuje do wyszukiwania).</summary>
    Private Function WezelGrupy(g As WezelKatalogu, kluczRodzica As String, nazwyRodzicow As String, fraza As String, szukanie As Boolean) As TreeNode
        Dim klucz As String = kluczRodzica & "/" & g.Nazwa
        Dim nazwy As String = nazwyRodzicow & " " & g.Nazwa
        Dim wezel As New TreeNode(g.Nazwa) With {.Name = klucz, .Tag = g.Nazwa}
        For Each d In g.Dzieci
            If d.CzyWarstwa Then
                Dim z = ZnajdzZbior(d.Zbior)
                If z Is Nothing OrElse z.Plik Is Nothing Then Continue For
                If szukanie AndAlso Not FiltrWarstw.Pasuje(nazwy & " " & z.Nazwa, d.Nazwa & " " & d.Warstwa, fraza) Then Continue For
                wezel.Nodes.Add(WezelWarstwy(z, d.Warstwa, d.Nazwa, klucz))
            Else
                Dim podgrupa = WezelGrupy(d, klucz, nazwy, fraza, szukanie)
                If podgrupa IsNot Nothing Then wezel.Nodes.Add(podgrupa)
            End If
        Next
        Return If(wezel.Nodes.Count > 0, wezel, Nothing)
    End Function

    ''' <summary>
    ''' Grupa „Dodane serwery”: zbiory map spoza katalogu (np. dodane kreatorem lub w folderze użytkownika) i warstwy
    ''' zbiorów z katalogu, których katalog nie wymienia - z nazwami z plików; zbiory, których nie udało się odczytać, szare.
    ''' </summary>
    Private Function WezelDodanychSerwerow(fraza As String, szukanie As Boolean) As TreeNode
        Dim klucz As String = "/" & GrupaDodanychSerwerow
        Dim grupa As New TreeNode(GrupaDodanychSerwerow) With {.Name = klucz, .Tag = GrupaDodanychSerwerow,
            .ToolTipText = "Zbiory map i warstwy spoza katalogu warstw (warstwy\katalog.txt), np. dodane kreatorem serwerów."}
        For Each z In _zbiory
            Dim wyswietlanyZbior As String = If(z.Plik IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(z.Plik.Tytul), z.Plik.Tytul, z.Nazwa)
            Dim wezel As New TreeNode(wyswietlanyZbior) With {.Name = klucz & "/" & z.Nazwa, .Tag = wyswietlanyZbior}
            If z.Plik Is Nothing Then
                If szukanie AndAlso Not FiltrWarstw.Pasuje(GrupaDodanychSerwerow, wyswietlanyZbior & " " & z.Nazwa, fraza) Then Continue For
                wezel.ForeColor = SystemColors.GrayText
                wezel.ToolTipText = "Nie udało się odczytać zbioru map: " & z.Blad
                grupa.Nodes.Add(wezel)
                Continue For
            End If
            wezel.ToolTipText = wyswietlanyZbior & If(wyswietlanyZbior <> z.Nazwa, " (" & z.Nazwa & ")", "") &
                                vbCrLf & If(z.Plik.Adres <> "", MetadaneMapy.MaskujSekrety(z.Plik.Adres), "Brak adresu serwera w pliku zbioru map.")
            For Each w In _katalog.NiewymienioneWarstwy(z.Nazwa, z.Plik)
                Dim wyswietlanaNazwa As String = If(Not String.IsNullOrWhiteSpace(w.Tytul), w.Tytul, w.Nazwa)
                If szukanie AndAlso Not FiltrWarstw.Pasuje(GrupaDodanychSerwerow & " " & wyswietlanyZbior & " " & z.Nazwa, wyswietlanaNazwa & " " & w.Nazwa, fraza) Then Continue For
                wezel.Nodes.Add(WezelWarstwy(z, w.Nazwa, wyswietlanaNazwa, wezel.Name))
            Next
            If wezel.Nodes.Count > 0 Then grupa.Nodes.Add(wezel)
        Next
        Return If(grupa.Nodes.Count > 0, grupa, Nothing)
    End Function

    ''' <summary>Węzeł warstwy z podpowiedzią: nazwa warstwy na serwerze, zbiór map, typ, zakres powiększeń, zalecany rozmiar piksela.</summary>
    Private Function WezelWarstwy(z As ZbiorMapy, warstwa As String, nazwa As String, kluczRodzica As String) As TreeNode
        Dim wm = z.Plik.Znajdz(warstwa)
        Dim opisTechniczny As String = KafleXyz.OpisWarstwy(z.Plik.EfektywnyTyp, z.Plik.OpcjeKafli(warstwa))
        Dim opis As String = nazwa & vbCrLf & "Warstwa " & warstwa & " zbioru map " & z.Nazwa &
                             If(opisTechniczny <> "", vbCrLf & opisTechniczny, "") &
                             If(wm IsNot Nothing AndAlso Wartosc(wm.RozmiarPiksela) > 0, vbCrLf & "Zalecany rozmiar piksela: " & wm.RozmiarPiksela & " m", "")
        Return New TreeNode(nazwa) With {.Name = kluczRodzica & "/" & z.Nazwa & "|" & warstwa, .ToolTipText = opis,
                                         .Tag = New WarstwaWDrzewie With {.Zbior = z, .Warstwa = warstwa, .Nazwa = nazwa}}
    End Function

    ''' <summary>
    ''' Pola wyboru warstw według kanapki (warstwa zaznaczona = wybrana z tego samego serwera, więc ta sama warstwa
    ''' w kilku grupach jest zaznaczona wszędzie) i liczba wybranych warstw przy nazwach grup. Wywoływane po każdej
    ''' zmianie kanapki, także gdy dodanie warstwy się nie udało.
    ''' </summary>
    Private Sub OdswiezZaznaczeniaDrzewa()
        If tvWarstwy.Nodes.Count = 0 Then Exit Sub
        _synchronizacjaDrzewa = True
        Try
            For Each wezel As TreeNode In tvWarstwy.Nodes
                OdswiezWezel(wezel)
            Next
        Finally
            _synchronizacjaDrzewa = False
        End Try
    End Sub

    ''' <summary>Zaznaczenie warstwy albo licznik grupy; zwraca liczbę wybranych warstw w węźle.</summary>
    Private Function OdswiezWezel(wezel As TreeNode) As Integer
        Dim w = DrzewoWarstw.WarstwaWezla(wezel)
        If w IsNot Nothing Then
            Dim wybrana As Boolean = _zestaw.Zawiera(w.Zbior.Plik.Adres, w.Warstwa)
            If wezel.Checked <> wybrana Then wezel.Checked = wybrana
            Return If(wybrana, 1, 0)
        End If
        Dim suma As Integer = 0
        For Each d As TreeNode In wezel.Nodes
            suma += OdswiezWezel(d)
        Next
        Dim tekst As String = CStr(wezel.Tag) & If(suma > 0, " (" & suma & ")", "")
        If wezel.Text <> tekst Then wezel.Text = tekst
        Return suma
    End Function

    ''' <summary>
    ''' Rozwija grupy z wybranymi warstwami oraz grupę pierwszej warstwy zbioru <paramref name="pokazZbior"/>, którą
    ''' wskazuje w drzewie.
    ''' </summary>
    Private Sub RozwinZbioryWybranych(Optional pokazZbior As String = "")
        If tvWarstwy.Nodes.Count = 0 Then Exit Sub
        Dim cel As TreeNode = Nothing
        For Each wezel In tvWarstwy.WszystkieWezly().ToList()
            Dim w = DrzewoWarstw.WarstwaWezla(wezel)
            If w Is Nothing Then Continue For
            Dim zeZbioru As Boolean = String.Equals(w.Zbior.Nazwa, pokazZbior, StringComparison.OrdinalIgnoreCase)
            If zeZbioru AndAlso cel Is Nothing Then cel = wezel.Parent
            If wezel.Checked OrElse (zeZbioru AndAlso cel Is wezel.Parent) Then RozwinNadrzedne(wezel)
        Next
        If cel IsNot Nothing Then
            tvWarstwy.SelectedNode = cel
            tvWarstwy.PokazWezel(cel)
        End If
        OdswiezPrzyciskZwijania()
    End Sub

    Private Shared Sub RozwinNadrzedne(wezel As TreeNode)
        Dim rodzic As TreeNode = wezel.Parent
        While rodzic IsNot Nothing
            If Not rodzic.IsExpanded Then rodzic.Expand()
            rodzic = rodzic.Parent
        End While
    End Sub

    ''' <summary>Przycisk zwijania: zwija wszystko, gdy któraś grupa jest rozwinięta, w przeciwnym razie rozwija wszystkie.</summary>
    Private Sub OdswiezPrzyciskZwijania()
        Dim zwin As Boolean = tvWarstwy.Nodes.Cast(Of TreeNode)().Any(Function(n) n.IsExpanded)
        btnZwinRozwin.Text = If(zwin, "⊟", "⊞")
        ToolTip1.SetToolTip(btnZwinRozwin, If(zwin, "Zwiń wszystkie grupy", "Rozwiń wszystkie grupy i podgrupy"))
    End Sub

    Private Sub btnZwinRozwin_Click(sender As Object, e As EventArgs) Handles btnZwinRozwin.Click
        Dim zwin As Boolean = tvWarstwy.Nodes.Cast(Of TreeNode)().Any(Function(n) n.IsExpanded)
        tvWarstwy.BeginUpdate()
        Try
            If zwin Then tvWarstwy.CollapseAll() Else tvWarstwy.ExpandAll()
        Finally
            tvWarstwy.EndUpdate()
        End Try
        If zwin AndAlso tvWarstwy.Nodes.Count > 0 Then
            tvWarstwy.PokazWezel(tvWarstwy.Nodes(0))
        ElseIf tvWarstwy.SelectedNode IsNot Nothing Then
            tvWarstwy.PokazWezel(tvWarstwy.SelectedNode)
        End If
        OdswiezPrzyciskZwijania()
    End Sub

    Private Sub tvWarstwy_AfterExpandCollapse(sender As Object, e As TreeViewEventArgs) Handles tvWarstwy.AfterExpand, tvWarstwy.AfterCollapse
        'przy wyszukiwaniu grupy rozwija program - zapamiętywane są tylko rozwinięcia bez wyszukiwania
        If Not _synchronizacjaDrzewa AndAlso Not SzukanieWarstw AndAlso DrzewoWarstw.CzyGrupa(e.Node) Then
            If e.Node.IsExpanded Then _rozwinieteZbiory.Add(e.Node.Name) Else _rozwinieteZbiory.Remove(e.Node.Name)
        End If
        If Not _synchronizacjaDrzewa Then OdswiezPrzyciskZwijania()
    End Sub

    Private Sub tvWarstwy_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvWarstwy.AfterSelect
        If _synchronizacjaDrzewa Then Exit Sub
        Dim w = DrzewoWarstw.WarstwaWezla(e.Node)
        If w IsNot Nothing Then _biezacyZbior = w.Zbior.Nazwa
    End Sub

    ''' <summary>Kliknięcie nazwy warstwy zaznacza ją lub odznacza (jak pole wyboru), kliknięcie nazwy grupy rozwija ją lub zwija.</summary>
    Private Sub tvWarstwy_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvWarstwy.NodeMouseClick
        If e.Button <> MouseButtons.Left OrElse tvWarstwy.HitTest(e.Location).Location <> TreeViewHitTestLocations.Label Then Exit Sub
        If DrzewoWarstw.CzyGrupa(e.Node) Then
            e.Node.Toggle()
        Else
            e.Node.Checked = Not e.Node.Checked
        End If
    End Sub

    ''' <summary>Zaznaczenie warstwy dodaje ją na wierzch kanapki, odznaczenie - usuwa z kanapki.</summary>
    Private Sub tvWarstwy_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvWarstwy.AfterCheck
        Dim w = DrzewoWarstw.WarstwaWezla(e.Node)
        If _synchronizacjaDrzewa OrElse w Is Nothing Then Exit Sub
        _biezacyZbior = w.Zbior.Nazwa
        If e.Node.Checked Then DodajWarstwe(w) Else UsunWarstweZbioru(w)
        'pola wyboru zgodne z kanapką także wtedy, gdy zmiana się nie udała (pobieranie, 12 warstw, brak adresu),
        'i przy tej samej warstwie w innych grupach
        OdswiezZaznaczeniaDrzewa()
    End Sub

    Private Sub txtSzukajWarstwy_TextChanged(sender As Object, e As EventArgs) Handles txtSzukajWarstwy.TextChanged
        BudujDrzewo()
    End Sub

    ''' <summary>Esc czyści wyszukiwanie, strzałka w dół lub Enter przechodzą do drzewa warstw.</summary>
    Private Sub txtSzukajWarstwy_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSzukajWarstwy.KeyDown
        Select Case e.KeyCode
            Case Keys.Escape
                txtSzukajWarstwy.Clear()
            Case Keys.Down, Keys.Enter
                If tvWarstwy.Nodes.Count = 0 Then Exit Sub
                If tvWarstwy.SelectedNode Is Nothing Then
                    'przy wyszukiwaniu - pierwsza znaleziona warstwa
                    tvWarstwy.SelectedNode = If(tvWarstwy.WszystkieWezly().FirstOrDefault(Function(w) Not DrzewoWarstw.CzyGrupa(w) AndAlso w.IsVisible),
                                                tvWarstwy.Nodes(0))
                End If
                tvWarstwy.Focus()
            Case Else
                Exit Sub
        End Select
        e.Handled = True
        e.SuppressKeyPress = True
    End Sub

    ''' <summary>Blokuje drzewo warstw w trakcie pobierania (kanapki nie można wtedy zmieniać).</summary>
    Private Sub UstawDostepnoscDrzewa(dostepne As Boolean)
        tvWarstwy.Enabled = dostepne
        txtSzukajWarstwy.Enabled = dostepne
        btnZwinRozwin.Enabled = dostepne
    End Sub

    ''' <summary>Lista wybranych warstw: obraz kanapki w tle, menu warstwy, podpowiedzi.</summary>
    Private Sub PrzygotujListeWarstw()
        lstWybraneWarstwy.ObrazTla = My.Resources.kanapka_gis
        lstWybraneWarstwy.Podpowiedz = ToolTip1
        lstWybraneWarstwy.OpisListy = ToolTip1.GetToolTip(grpWybraneWarstwy)
        lstWybraneWarstwy.Zestaw = _zestaw
        lstWybraneWarstwy.NazwaWarstwy = AddressOf NazwaWarstwy
        lstWybraneWarstwy.ZrodloWarstwy = AddressOf ZrodloWarstwy

        _menuWarstwy = New ContextMenuStrip(components)
        _menuWarstwy.Items.Add("Przesuń wyżej", Nothing, Sub() PrzesunZaznaczonaWarstwe(1))
        _menuWarstwy.Items.Add("Przesuń niżej", Nothing, Sub() PrzesunZaznaczonaWarstwe(-1))
        _menuWarstwy.Items.Add(New ToolStripSeparator())
        For Each k In {100, 75, 50, 25}
            Dim krycie As Integer = k
            Dim poz As New ToolStripMenuItem("Krycie " & krycie & " %", Nothing, Sub() UstawKrycieZaznaczonej(krycie)) With {.Tag = krycie}
            _menuWarstwy.Items.Add(poz)
        Next
        _menuWarstwy.Items.Add("Krycie - zaawansowane…", Nothing, Sub() OtworzWarstwyZaawansowane())
        _menuWarstwy.Items.Add(New ToolStripSeparator())
        _menuWarstwy.Items.Add("Usuń warstwę", My.Resources.kosz, Sub() UsunZaznaczonaWarstwe())
        AddHandler _menuWarstwy.Opening,
            Sub(s, e)
                Dim i As Integer = lstWybraneWarstwy.Zaznaczona
                If i < 0 Then e.Cancel = True : Exit Sub
                _menuWarstwy.Items(0).Enabled = i < _zestaw.Liczba - 1
                _menuWarstwy.Items(1).Enabled = i > 0
                For Each poz In _menuWarstwy.Items.OfType(Of ToolStripMenuItem)()
                    If TypeOf poz.Tag Is Integer Then poz.Checked = CInt(poz.Tag) = _zestaw(i).Krycie
                Next
            End Sub
        lstWybraneWarstwy.MenuWarstwy = _menuWarstwy
    End Sub

    ''' <summary>Wyświetla wybrane warstwy (lista z kanapką, pola wyboru w drzewie warstw) i stan przycisków panelu.</summary>
    Private Sub PokazWarstwy()
        If lstWybraneWarstwy.Zestaw IsNot _zestaw Then lstWybraneWarstwy.Zestaw = _zestaw Else lstWybraneWarstwy.Odswiez()
        OdswiezZaznaczeniaDrzewa()
        OdswiezPrzyciskiWarstw()
    End Sub

    ''' <summary>Przyciski i pole krycia wg zaznaczonej warstwy.</summary>
    Private Sub OdswiezPrzyciskiWarstw()
        Dim i As Integer = lstWybraneWarstwy.Zaznaczona
        Dim pobieranie As Boolean = _przerwanie IsNot Nothing
        btnResetujWarstwy.Enabled = (i >= 0 OrElse _zestaw.Liczba = 1) AndAlso _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnUsunWarstwe.Enabled = (i >= 0 OrElse _zestaw.Liczba = 1) AndAlso _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnWarstwaWyzej.Enabled = i >= 0 AndAlso i < _zestaw.Liczba - 1 AndAlso Not pobieranie
        btnWarstwaNizej.Enabled = i > 0 AndAlso Not pobieranie
        numKrycie.Enabled = i >= 0 AndAlso Not pobieranie
        lblKrycie.Enabled = numKrycie.Enabled
        btnWarstwyZaawansowane.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        _ustawianieKrycia = True
        numKrycie.Value = If(i >= 0, _zestaw(i).Krycie, ZestawWarstw.PelneKrycie)
        _ustawianieKrycia = False
    End Sub

    ''' <summary>Opis warstw do komunikatów, np. "orto, dzialki (krycie 60 %)".</summary>
    Private Function OpisWarstw() As String
        Dim zbiory As Boolean = _zestaw.WieleZrodel
        Return String.Join(", ", _zestaw.Warstwy.Select(Function(w) NazwaWarstwy(w) & If(zbiory AndAlso ZrodloWarstwy(w) <> "", " [" & ZrodloWarstwy(w) & "]", "") &
                                                           If(w.Krycie < ZestawWarstw.PelneKrycie, " (krycie " & w.Krycie & " %)", "")))
    End Function

    ''' <summary>Po zmianie listy warstw: widok, nazwa paczki TrekBuddy.</summary>
    Private Sub PoZmianieWarstw(zaznacz As Integer)
        PokazWarstwy()
        lstWybraneWarstwy.Zaznaczona = zaznacz
        OdswiezPrzyciskiWarstw()
        Form2.OdswiezNazweTrekBuddy()
    End Sub

    ''' <summary>Czyści listę wybranych warstw.</summary>
    Private Sub WyczyscWarstwy()
        _zestaw.Wyczysc()
        PokazWarstwy()
    End Sub

    ''' <summary>Usuwa z kanapki warstwę (odznaczenie warstwy w drzewie).</summary>
    Private Sub UsunWarstweZbioru(w As WarstwaWDrzewie)
        For i = 0 To _zestaw.Liczba - 1
            If _zestaw(i).TaSama(w.Zbior.Plik.Adres, w.Warstwa) Then
                lstWybraneWarstwy.Zaznaczona = i
                UsunZaznaczonaWarstwe()
                Exit Sub
            End If
        Next
    End Sub

    ''' <summary>
    ''' Dodaje warstwę na wierzch kanapki. Wybrane już warstwy innych zbiorów zostają - każda pamięta swój serwer,
    ''' więc na mapę topograficzną z jednego serwera można nałożyć np. cieniowanie z innego.
    ''' </summary>
    Private Sub DodajWarstwe(w As WarstwaWDrzewie)
        If PobieranieTrwa("Zmiana listy warstw") Then Exit Sub
        Dim zbior As ZbiorMapy = w.Zbior
        If zbior.Plik.Adres = "" Then
            Komunikat("Zbiór map " & zbior.Nazwa & " nie ma adresu serwera.", Color.Red) : Exit Sub
        End If
        Dim zWarstwy = zbior.Plik.Znajdz(w.Warstwa)
        Dim zalecany As Double = If(zWarstwy IsNot Nothing, Wartosc(zWarstwy.RozmiarPiksela), 0)
        Dim bylaPusta As Boolean = _zestaw.Liczba = 0
        Dim typZrodla As TypUslugiMapowej = zbior.Plik.EfektywnyTyp
        Dim jestNakladka As Boolean = zbior.Plik.OpcjeKafli(w.Warstwa).Nakladka
        Select Case _zestaw.Dodaj(w.Warstwa, adres:=zbior.Plik.Adres, zbior:=zbior.Nazwa, zalecanyPiksel:=zalecany,
                                   typZrodla:=typZrodla, nakladka:=jestNakladka, umiescPodNakladkami:=True,
                                   ignorujSsl:=zbior.Plik.IgnorujSsl)
            Case WynikDodania.BrakMiejsca
                MsgBox("TEJ WARSTWY NIE MOŻNA JUŻ DODAĆ - wybrano już " & ZestawWarstw.MaksLiczba & " warstw.")
                Exit Sub
            Case WynikDodania.JuzWybrana
                Komunikat("Warstwa " & w.Nazwa & " jest już na liście wybranych warstw.", Color.Blue)
                Exit Sub
        End Select
        Dim indeksNowej As Integer = Enumerable.Range(0, _zestaw.Liczba).FirstOrDefault(
            Function(ix) _zestaw(ix).TaSama(zbior.Plik.Adres, w.Warstwa), _zestaw.Liczba - 1)
        PoZmianieWarstw(indeksNowej)

        'rozmiar piksela wg najdokładniejszej warstwy kanapki (dawniej - wg ostatnio klikniętej)
        'dawne komunikaty o limicie 256 px serwera hgis.cartomatic.pl usunięte - serwer nie działa, a jego następca
        '(pastmaps.cenagis.edu.pl, IHPAN) zwraca obrazy 2000 px; nazwy warstw (m25k_2180…) i tak już ich nie wywoływały
        Dim opisPiksela As String = DobierzRozmiarPiksela(bylaPusta)
        Dim infoWstawienia As String = If(indeksNowej < _zestaw.Liczba - 1, " (umieszczono pod nakładką)", "")
        Dim ostrzezenieFormat As String = If(_zestaw.CzySameNakladki() AndAlso Not Wms.CzyFormatZPrzezroczystoscia(Ustawienia.Format),
                                             " Uwaga: wybrano same nakładki, a format pobierania to ." & Ustawienia.Format & " (bez przezroczystości).", "")
        Komunikat("Wskazano warstwę: " & w.Nazwa & infoWstawienia & "." & If(opisPiksela <> "", " " & opisPiksela, "") & ostrzezenieFormat, Color.Green)
    End Sub

    Private Sub btnResetujWarstwy_Click(sender As Object, e As EventArgs) Handles btnResetujWarstwy.Click
        If PobieranieTrwa("Zmiana listy warstw") Then Exit Sub
        UsunZaznaczonaWarstwe()
    End Sub

    Private Sub btnUsunWarstwe_Click(sender As Object, e As EventArgs) Handles btnUsunWarstwe.Click
        UsunZaznaczonaWarstwe()
    End Sub

    Private Sub btnWarstwaWyzej_Click(sender As Object, e As EventArgs) Handles btnWarstwaWyzej.Click
        PrzesunZaznaczonaWarstwe(1)
    End Sub

    Private Sub btnWarstwaNizej_Click(sender As Object, e As EventArgs) Handles btnWarstwaNizej.Click
        PrzesunZaznaczonaWarstwe(-1)
    End Sub

    Private Sub btnWarstwyZaawansowane_Click(sender As Object, e As EventArgs) Handles btnWarstwyZaawansowane.Click
        OtworzWarstwyZaawansowane()
    End Sub

    Private Sub lstWybraneWarstwy_ZaznaczenieZmienione(sender As Object, e As EventArgs) Handles lstWybraneWarstwy.ZaznaczenieZmienione
        OdswiezPrzyciskiWarstw()
    End Sub

    Private Sub lstWybraneWarstwy_UsuniecieZadane(sender As Object, e As EventArgs) Handles lstWybraneWarstwy.UsuniecieZadane
        UsunZaznaczonaWarstwe()
    End Sub

    Private Sub lstWybraneWarstwy_PrzeniesienieZadane(sender As Object, e As PrzeniesienieWarstwyEventArgs) Handles lstWybraneWarstwy.PrzeniesienieZadane
        PrzeniesWarstwe(e.ZIndeksu, e.NaIndeks)
    End Sub

    Private Sub numKrycie_ValueChanged(sender As Object, e As EventArgs) Handles numKrycie.ValueChanged
        If _ustawianieKrycia Then Exit Sub
        UstawKrycieZaznaczonej(CInt(numKrycie.Value))
    End Sub

    Private Sub UsunZaznaczonaWarstwe()
        If _zestaw.Liczba = 1 AndAlso lstWybraneWarstwy.Zaznaczona < 0 Then
            lstWybraneWarstwy.Zaznaczona = 0
        End If
        Dim i As Integer = lstWybraneWarstwy.Zaznaczona
        If i < 0 OrElse PobieranieTrwa("Zmiana listy warstw") Then Exit Sub
        Dim nazwa As String = NazwaWarstwy(_zestaw(i))
        _zestaw.Usun(i)
        PoZmianieWarstw(Math.Min(i, _zestaw.Liczba - 1))
        If _zestaw.Liczba = 0 Then
            _automatycznyPikselMetry = 0
            _automatycznyPikselTekst = ""
        End If
        Dim opisPiksela As String = If(_zestaw.Liczba > 0, DobierzRozmiarPiksela(False), "")
        Komunikat("Usunięto warstwę " & nazwa & " z listy wybranych warstw." & If(opisPiksela <> "", " " & opisPiksela, ""), Color.Green)
    End Sub

    ''' <summary>Przesuwa zaznaczoną warstwę o jedno miejsce: 1 - wyżej w kanapce, -1 - niżej.</summary>
    Private Sub PrzesunZaznaczonaWarstwe(kierunek As Integer)
        Dim i As Integer = lstWybraneWarstwy.Zaznaczona
        If i >= 0 Then PrzeniesWarstwe(i, i + kierunek)
    End Sub

    Private Sub PrzeniesWarstwe(zIndeksu As Integer, naIndeks As Integer)
        If zIndeksu < 0 OrElse zIndeksu >= _zestaw.Liczba OrElse naIndeks < 0 OrElse naIndeks >= _zestaw.Liczba OrElse naIndeks = zIndeksu Then Exit Sub
        If PobieranieTrwa("Zmiana kolejności warstw") Then Exit Sub
        Dim nowa As Integer = _zestaw.Przenies(zIndeksu, naIndeks)
        PoZmianieWarstw(nowa)
        Dim przykryta = _zestaw.ZnajdzPrzykrytaNakladke()
        If przykryta IsNot Nothing Then
            Komunikat("Uwaga: nakładka " & NazwaWarstwy(przykryta) & " może zostać zasłonięta przez warstwę nad nią.", Color.DarkOrange)
        Else
            Komunikat("Warstwa " & NazwaWarstwy(_zestaw(nowa)) & " jest teraz na miejscu " & (nowa + 1) & If(nowa = 0, " (podkład)", "") & ".", Color.Green)
        End If
    End Sub

    Private Sub UstawKrycieZaznaczonej(krycie As Integer)
        Dim i As Integer = lstWybraneWarstwy.Zaznaczona
        If i < 0 Then Exit Sub
        If PobieranieTrwa("Zmiana krycia warstwy") Then
            OdswiezPrzyciskiWarstw()
            Exit Sub
        End If
        _zestaw.UstawKrycie(i, krycie)
        PoZmianieWarstw(i)
        Komunikat("Krycie warstwy " & NazwaWarstwy(_zestaw(i)) & ": " & _zestaw(i).Krycie & " %." &
                  If(_zestaw(i).CzyKafelkowa OrElse _zestaw(i).Krycie >= ZestawWarstw.PelneKrycie, "",
                     " Warstwy o kryciu mniejszym niż 100% pobierane są osobnymi zapytaniami (" & _zestaw.GrupyZapytan().Count & " grup na segment)."),
                  Color.Green)
    End Sub

    ''' <summary>Okno „Warstwy - zaawansowane”: krycie suwakami, kolejność i podgląd kanapki; zmiany przyjmowane po OK.</summary>
    Private Sub OtworzWarstwyZaawansowane()
        If _zestaw.Liczba = 0 OrElse PobieranieTrwa("Zmiana listy warstw") Then Exit Sub
        Using okno As New FormWarstwyZaawansowane(_zestaw.Kopia(), ZadanieDoPodgladu(), lstWybraneWarstwy.Zaznaczona)
            If okno.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
            _zestaw = okno.Zestaw
            PoZmianieWarstw(Math.Min(okno.Zaznaczona, _zestaw.Liczba - 1))
            Dim opisPiksela As String = DobierzRozmiarPiksela(False)
            Dim przykryta = _zestaw.ZnajdzPrzykrytaNakladke()
            If przykryta IsNot Nothing Then
                Komunikat("Uwaga: nakładka " & NazwaWarstwy(przykryta) & " może zostać zasłonięta przez warstwę nad nią.", Color.DarkOrange)
            Else
                Komunikat("Wybrane warstwy: " & OpisWarstw() & "." & If(opisPiksela <> "", " " & opisPiksela, ""), Color.Green)
            End If
        End Using
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

    ''' <summary>
    ''' Rozmiar piksela sesji wg najdokładniejszej warstwy kanapki (najmniejszy zalecany): żadna warstwa nie traci szczegółów,
    ''' mniej dokładne są przeskalowane. Pole zmieniane jest, gdy kanapka była pusta albo zawiera wartość wpisaną wcześniej
    ''' przez program; wartość wpisaną ręcznie zostawia. Zwraca opis do komunikatu (pusty - żadna warstwa nie ma zalecanego rozmiaru).
    ''' </summary>
    Private Function DobierzRozmiarPiksela(bylaPusta As Boolean) As String
        UzupelnijZalecanePiksele()
        Dim najdokladniejsza = _zestaw.NajdokladniejszaWarstwa()
        If najdokladniejsza Is Nothing Then Return ""
        Dim jednostka As String = If(Ustawienia.Uklad.Geograficzny, "°", " m")
        Dim zalecany As String = RozmiarPikselaWarstwy(Liczba(najdokladniejsza.ZalecanyPiksel))
        Dim zrodlo As String = najdokladniejsza.Nazwa & If(_zestaw.WieleZrodel AndAlso najdokladniejsza.Zbior <> "", " (" & najdokladniejsza.Zbior & ")", "")
        Dim opis As String
        If bylaPusta OrElse PikselAutomatyczny() Then
            If txtRozmiarPiksela.Text.Trim() <> zalecany Then txtRozmiarPiksela.Text = zalecany
            _automatycznyPikselMetry = najdokladniejsza.ZalecanyPiksel
            _automatycznyPikselTekst = zalecany
            opis = "Rozmiar piksela " & zalecany & jednostka & If(_zestaw.Liczba > 1, " - wg najdokładniejszej warstwy " & zrodlo & ".", ".")
        Else
            opis = "Rozmiar piksela wpisany ręcznie (" & txtRozmiarPiksela.Text.Trim() & jednostka & ") pozostaje; zalecany dla wybranych warstw: " &
                   zalecany & jednostka & " (warstwa " & zrodlo & ")."
        End If
        Dim najmniej = _zestaw.NajmniejDokladnaWarstwa()
        If najmniej IsNot Nothing AndAlso najmniej.ZalecanyPiksel >= 4 * najdokladniejsza.ZalecanyPiksel Then
            opis &= " Warstwa " & najmniej.Nazwa & " (zalecany " & Liczba(najmniej.ZalecanyPiksel) & " m) zostanie przeskalowana; mały piksel to więcej segmentów i danych."
        End If
        Return opis
    End Function

    ''' <summary>Czy pole rozmiaru piksela zawiera wartość wpisaną przez program (nie przez użytkownika).</summary>
    Private Function PikselAutomatyczny() As Boolean
        If _automatycznyPikselMetry <= 0 Then Return False
        Dim tekst As String = txtRozmiarPiksela.Text.Trim()
        If tekst = _automatycznyPikselTekst Then Return True
        'po zmianie układu metrycznego (np. PL-1992 na PL-2000) wartość w metrach pozostaje ta sama
        If Ustawienia.Uklad.Geograficzny Then Return False
        Return Math.Abs(Wartosc(tekst) - _automatycznyPikselMetry) <= _automatycznyPikselMetry * 0.000001
    End Function

    ''' <summary>Zalecany rozmiar piksela, typ źródła i flaga nakładki warstw sesji (np. po wczytaniu conf.txt) - z plików zbiorów map.</summary>
    Private Sub UzupelnijZalecanePiksele()
        Dim pliki As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        For Each w In _zestaw.Warstwy
            If w.Zbior = "" Then Continue For
            Dim plik As PlikWarstw = Nothing
            If Not pliki.TryGetValue(w.Zbior, plik) Then
                Dim z = ZnajdzZbior(w.Zbior)
                plik = If(z?.Plik, PlikZbioruMap(w.Zbior))
                pliki(w.Zbior) = plik
            End If
            If plik Is Nothing Then Continue For
            w.TypZrodla = plik.EfektywnyTyp
            w.Nakladka = plik.OpcjeKafli(w.Nazwa).Nakladka
            w.IgnorujSsl = plik.IgnorujSsl
            If w.ZalecanyPiksel <= 0 Then
                Dim wm = plik.Znajdz(w.Nazwa)
                If wm IsNot Nothing Then w.ZalecanyPiksel = Math.Max(0, Wartosc(wm.RozmiarPiksela))
            End If
        Next
    End Sub

    Private Function ZrodlaMapSesji() As Dictionary(Of String, PlikWarstw)
        Dim zrodla As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        For Each z In _zbiory
            If z.Plik IsNot Nothing AndAlso Not zrodla.ContainsKey(z.Nazwa) Then
                zrodla(z.Nazwa) = z.Plik
            End If
        Next
        For Each w In _zestaw.Warstwy
            If String.IsNullOrEmpty(w.Zbior) OrElse zrodla.ContainsKey(w.Zbior) Then Continue For
            Dim plik = PlikZbioruMap(w.Zbior)
            If plik IsNot Nothing Then zrodla(w.Zbior) = plik
        Next
        Return zrodla
    End Function

    Friend Function AtrybucjaZbioru(zbior As String, Optional adres As String = "") As String
        If Not String.IsNullOrEmpty(zbior) Then
            Dim z = ZnajdzZbior(zbior)
            Dim p = If(z?.Plik, PlikZbioruMap(zbior))
            If p IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(p.Atrybucja) Then Return p.Atrybucja
        End If
        If Not String.IsNullOrEmpty(adres) Then
            Dim wgAdresu = _zbiory.FirstOrDefault(Function(x) x.Plik IsNot Nothing AndAlso String.Equals(x.Plik.Adres, adres, StringComparison.OrdinalIgnoreCase))
            If wgAdresu IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(wgAdresu.Plik.Atrybucja) Then Return wgAdresu.Plik.Atrybucja
        End If
        Return ""
    End Function

    Private Function EfektywnaPrzezroczystosc() As Boolean
        Dim maPrzezroczystosc As Boolean = _zestaw.CzySameNakladki() OrElse
                                           (_zestaw.Liczba > 0 AndAlso (_zestaw(0).Krycie < 100 OrElse _zestaw(0).Nakladka)) OrElse
                                           _zestaw.Warstwy.Any(Function(w) w.Krycie < 100 OrElse w.Nakladka)
        Return Ustawienia.Przezroczystosc OrElse (maPrzezroczystosc AndAlso Wms.CzyFormatZPrzezroczystoscia(Ustawienia.Format))
    End Function

    ''' <summary>Plik zbioru map (folder programu, potem folder danych); Nothing - brak pliku lub błąd odczytu.</summary>
    Private Function PlikZbioruMap(zbior As String) As PlikWarstw
        Dim plik As String = SciezkaZbioruMap(zbior)
        If plik = "" Then Return Nothing
        Try
            Return PlikWarstw.Wczytaj(plik)
        Catch
            Return Nothing
        End Try
    End Function

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
    ''' <summary>
    ''' Parametry podglądu kanapki w oknie „Warstwy - zaawansowane”: serwer, układ, format i siatka jak przy pobieraniu
    ''' (siatka Nothing - niepoprawne pola zasięgu).
    ''' </summary>
    Private Function ZadanieDoPodgladu() As ZadaniePobierania
        Dim siatka = BiezacaSiatka()
        Return New ZadaniePobierania With {
            .Uklad = Ustawienia.Uklad, .Siatka = If(siatka.Poprawna, siatka, Nothing),
            .Zbiory = _zestaw.Zbiory(), .ZrodlaMap = ZrodlaMapSesji(),
            .Format = Ustawienia.Format, .ZamienOsie = Ustawienia.ZamienXY, .LimitCzasuSekundy = Ustawienia.LimitCzasuSekundy,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .JakoscJpegWmts = Ustawienia.JakoscJpegWmts,
            .Przezroczystosc = EfektywnaPrzezroczystosc(),
            .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_podglad") & "\"}
    End Function

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
        If _zestaw.Liczba = 0 Then
            Komunikat("Nie wybrano żadnej warstwy. Aby wybrać warstwę kliknij w jej nazwę.", Color.Red) : Exit Sub
        End If
        If _zestaw.Warstwy.Any(Function(w) w.Adres = "") Then
            Komunikat("Nie znam serwera którejś z wybranych warstw - usuń ją i wybierz ponownie ze zbioru map.", Color.Red) : Exit Sub
        End If
        Dim przykryta = _zestaw.ZnajdzPrzykrytaNakladke()
        If przykryta IsNot Nothing Then
            If MessageBox.Show("Nakładka """ & NazwaWarstwy(przykryta) & """ znajduje się pod nieprzezroczystą warstwą (krycie 100%) i może być niewidoczna." & vbCrLf & vbCrLf &
                               "Kontynuować pobieranie?", "Kolejność warstw", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                Exit Sub
            End If
        End If
        Dim maPrzezroczystosc As Boolean = _zestaw.CzySameNakladki() OrElse
                                           (_zestaw.Liczba > 0 AndAlso (_zestaw(0).Krycie < 100 OrElse _zestaw(0).Nakladka)) OrElse
                                           _zestaw.Warstwy.Any(Function(w) w.Krycie < 100 OrElse w.Nakladka)
        If maPrzezroczystosc AndAlso Not Wms.CzyFormatZPrzezroczystoscia(Ustawienia.Format) Then
            Dim odpFormat = MessageBox.Show("Wybrane warstwy zawierają nakładki lub krycie mniejsze niż 100%, ale obecny format segmentów to ." & Ustawienia.Format &
                                            " (tło i przezroczystości zostaną wypalone bez kanału alfa)." & vbCrLf & vbCrLf &
                                            "Przełączyć na format PNG z przezroczystym tłem?", "Format z przezroczystością",
                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If odpFormat = DialogResult.Cancel Then Exit Sub
            If odpFormat = DialogResult.Yes Then
                Ustawienia.Format = "png"
                Ustawienia.Przezroczystosc = True
                stFormat.Text = ".png"
                Ustawienia.Zapisz(PlikLastsettings)
            End If
        End If

        Dim zrodlaSesji = ZrodlaMapSesji()
        Dim grupyKafelkowe = _zestaw.GrupyZapytan().Where(Function(g) g.EfektywnyTyp = TypUslugiMapowej.Xyz OrElse g.EfektywnyTyp = TypUslugiMapowej.Tms).ToList()
        If grupyKafelkowe.Count > 0 Then
            Dim planySzacunku As New List(Of PlanWmts)
            Dim warstwyPozaZasiegiem As New List(Of String)
            For Each g In grupyKafelkowe
                Dim pierwszaWarstwa = _zestaw.Warstwy.FirstOrDefault(Function(x) String.Equals(x.Adres, g.Adres, StringComparison.OrdinalIgnoreCase))
                Dim src As PlikWarstw = Nothing
                If pierwszaWarstwa IsNot Nothing AndAlso Not String.IsNullOrEmpty(pierwszaWarstwa.Zbior) Then zrodlaSesji.TryGetValue(pierwszaWarstwa.Zbior, src)
                If src Is Nothing Then src = zrodlaSesji.Values.FirstOrDefault(Function(v) String.Equals(v.Adres, g.Adres, StringComparison.OrdinalIgnoreCase))
                Dim usl = KafleXyz.UtworzUslugeZbioru(g.Adres, g.Warstwy, src, g.EfektywnyTyp)
                For Each n In g.Warstwy
                    planySzacunku.Add(New PlanWmts(usl.Usluga, n, Ustawienia.Uklad, siatka.ZasiegSiatki, siatka.RozmiarPiksela, Ustawienia.Format))
                    If KafleXyz.ObszarPozaZasiegiem(usl.OpcjeWarstw(n), Ustawienia.Uklad, siatka.ZasiegSiatki) Then
                        warstwyPozaZasiegiem.Add(If(String.IsNullOrEmpty(pierwszaWarstwa?.Zbior), n, pierwszaWarstwa.Zbior & ": " & n))
                    End If
                Next
            Next
            'kafle poza polem "zasieg" zbioru map nie są pobierane - bez ostrzeżenia użytkownik dostałby puste segmenty
            If warstwyPozaZasiegiem.Count > 0 Then
                If MessageBox.Show("Wybrany obszar leży w całości poza zasięgiem danych map:" & vbCrLf & vbCrLf &
                                   String.Join(vbCrLf, warstwyPozaZasiegiem) & vbCrLf & vbCrLf &
                                   "Segmenty tych map będą puste. Kontynuować pobieranie?", "Obszar poza zasięgiem mapy",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                    Exit Sub
                End If
            End If
            Const ProgOstrzezeniaKafli As Long = 4000
            If KafleXyz.SzacujLiczbeKafli(planySzacunku, siatka, Ustawienia.Uklad, _korytarz, ProgOstrzezeniaKafli + 1) > ProgOstrzezeniaKafli Then
                If MessageBox.Show("Wybrany obszar wymaga pobrania ponad " & ProgOstrzezeniaKafli.ToString("N0") &
                                   " kafelków z serwera XYZ/TMS. Może to zająć dużo czasu lub obciążyć serwer." & vbCrLf & vbCrLf &
                                   "Kontynuować pobieranie?", "Duża liczba kafelków", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                    Exit Sub
                End If
            End If
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
            .AdresSerwera = _zestaw(0).Adres, .Warstwy = _zestaw.Nazwy(), .Krycia = _zestaw.Krycia(), .Adresy = _zestaw.Adresy(),
            .Zbiory = _zestaw.Zbiory(), .ZrodlaMap = zrodlaSesji, .Uklad = Ustawienia.Uklad,
            .Siatka = siatka, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .Numeracja = Siatka.StylZTekstu(Ustawienia.Numeracja), .Folder = FolderSesji, .ZamienOsie = Ustawienia.ZamienXY,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .PrzerwaSekundy = Ustawienia.PrzerwaMiedzyProbami,
            .LiczbaWatkow = Ustawienia.LiczbaWatkow, .LimitCzasuSekundy = Ustawienia.LimitCzasuSekundy,
            .ZachowajKafleWmts = Ustawienia.ZachowajKafleWmts, .JakoscJpegWmts = Ustawienia.JakoscJpegWmts,
            .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .Georeferencja = Ustawienia.OpcjeGeoreferencji(), .TrekBuddy = Ustawienia.TrekBuddy, .NazwaTrekBuddy = Ustawienia.NazwaTrekBuddy,
            .Przezroczystosc = EfektywnaPrzezroczystosc(), .Korytarz = _korytarz, .Odswiez = odswiez}

        btnPobierz.Enabled = False
        btnPrzerwij.Enabled = True
        UstawDostepnoscDrzewa(False)
        btnScalanie.Enabled = False
        btnUsunPobrane.Enabled = False
        OdswiezToolStripMenuItem.Enabled = False
        KorytarzToolStripMenuItem.Enabled = False
        Komunikat(If(odswiez, "Trwa odświeżanie segmentów (", "Trwa pobieranie segmentów (") & liczbaSegmentow & ")", Color.Black)

        'własna kopia - zwalniane jest zawsze źródło tego pobierania, nawet gdyby pole wskazywało już inne
        Dim przerwanie As New CancellationTokenSource()
        _przerwanie = przerwanie
        OdswiezPrzyciskiWarstw()
        Dim postep As New Progress(Of PostepPobierania)(AddressOf PokazPostep)
        Dim wynik As WynikPobierania = Nothing
        Try
            wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, postep, przerwanie.Token)
        Catch ex As Exception
            Komunikat("Błąd pobierania: " & MetadaneMapy.MaskujSekrety(ex.Message), Color.Red)
        Finally
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            przerwanie.Dispose()
            btnPobierz.Enabled = True
            btnPrzerwij.Enabled = False
            UstawDostepnoscDrzewa(True)
            btnUsunPobrane.Enabled = True
            OdswiezToolStripMenuItem.Enabled = True
            KorytarzToolStripMenuItem.Enabled = True
            stPostep.Value = 0
            stSegment.Text = ""
            OdswiezPrzyciskiWarstw()
        End Try

        If wynik Is Nothing Then Exit Sub
        If wynik.Przerwano Then
            Komunikat("Przerwano pobieranie segmentów", Color.Red)
        ElseIf wynik.Nieudane > 0 Then
            Dim plikErr As String = zadanie.Folder & "error.txt"
            Dim trescErr As String = If(File.Exists(plikErr), File.ReadAllText(plikErr), "")
            If trescErr.IndexOf("InvalidCRS", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               trescErr.IndexOf("układu współrzędnych", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               trescErr.IndexOf("CRS not supported", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Komunikat("Nie udało się ściągnąć segmentów (" & wynik.Nieudane & " z " & wynik.Wszystkie &
                          "). Serwer może nie obsługiwać układu " & Ustawienia.Uklad.Nazwa & " - zmień układ na PL-1992 lub sprawdź error.txt.", Color.Red)
            Else
                Komunikat("Nie udało się ściągnąć wszystkich segmentów (" & wynik.Nieudane & " z " & wynik.Wszystkie &
                          "). Wykaz tych segmentów i przyczyny błędów w pliku error.txt", Color.Red)
            End If
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
        GMapProvider.UserAgent = Program.UserAgentProgramu
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
        Dim folder = If(Not String.IsNullOrWhiteSpace(FolderSesji), FolderSesji, FolderDownload)
        Directory.CreateDirectory(folder)
        OtworzFolder(folder)
    End Sub

    Private Sub btnUsunPobrane_Click(sender As Object, e As EventArgs) Handles btnUsunPobrane.Click
        If PobieranieTrwa("Usunięcie pobranych map") Then Exit Sub
        Dim folder = If(Not String.IsNullOrWhiteSpace(FolderSesji), FolderSesji, FolderDownload)
        If Not Directory.Exists(folder) Then
            Komunikat("Folder bieżącej sesji nie istnieje: " & folder, Color.Red)
            Exit Sub
        End If

        Dim czyGlownyDownload As Boolean = String.Equals(Path.GetFullPath(folder).TrimEnd("\"c, "/"c),
                                                         Path.GetFullPath(FolderDownload).TrimEnd("\"c, "/"c),
                                                         StringComparison.OrdinalIgnoreCase)

        Dim komunikatPytania As String
        If czyGlownyDownload Then
            komunikatPytania = "Czy na pewno przenieść do Kosza pliki bieżącej sesji z folderu download?" & vbCrLf & vbCrLf &
                               "Pliki segmentów i konfiguracji zostaną przeniesione do Kosza (podkatalogi innych sesji pozostaną nienaruszone)."
        Else
            komunikatPytania = "Czy na pewno przenieść do Kosza folder bieżącej sesji?" & vbCrLf & vbCrLf &
                               folder & vbCrLf & vbCrLf &
                               "Zawartość folderu zostanie bezpiecznie przeniesiona do Kosza."
        End If

        Dim odp As DialogResult = MessageBox.Show(komunikatPytania, "Usuwanie plików bieżącej sesji",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
        If odp = DialogResult.Yes Then
            Try
                If czyGlownyDownload Then
                    Dim usuniete As Integer = 0
                    For Each plik In Directory.GetFiles(folder)
                        FileIO.FileSystem.DeleteFile(plik, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
                        usuniete += 1
                    Next
                    Komunikat("Przeniesiono do Kosza pliki bieżącej sesji (" & usuniete & " plików) z folderu download", Color.Green)
                Else
                    FileIO.FileSystem.DeleteDirectory(folder, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
                    FolderSesji = FolderDownload
                    Komunikat("Przeniesiono do Kosza folder sesji: " & folder, Color.Green)
                End If
            Catch ex As Exception
                Komunikat("Nie udało się przenieść sesji do Kosza: " & ex.Message, Color.Red)
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
        Dim istniejace = _zbiory.Where(Function(z) z.Plik IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(z.Plik.Adres)).
            Select(Function(z) New KeyValuePair(Of String, String)(z.Nazwa, z.Plik.Adres)).ToList()
        Using okno As New FormDodajSerwer(srodekMapy:=New PunktGeo(mapa.Position.Lat, mapa.Position.Lng), istniejaceZbiory:=istniejace)
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

    Private Sub EksportCadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EksportCadToolStripMenuItem.Click
        Using f As New FormEksportCad()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub ZaawansowanyMbtilesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZaawansowanyMbtilesToolStripMenuItem.Click
        FormZaawansowanyMbtiles.ShowDialog()
    End Sub

    Private Sub UstawieniaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UstawieniaToolStripMenuItem.Click
        'okno ustawień zmienia parametry sesji (przedrostek, format, numeracja, bok segmentu TrekBuddy)
        If PobieranieTrwa("Zmiana ustawień") Then Exit Sub
        Form2.ShowDialog()
    End Sub

    ''' <summary>Adres oficjalnej strony pomocy i podręcznika użytkownika MapoTero w serwisie GitHub Pages.</summary>
    Public Const UrlPomocyOnline As String = "https://eksploracja.github.io/MapoTero/pomoc/"

    Private Sub PomocOnlineToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PomocOnlineToolStripMenuItem.Click
        OtworzPomocOnline()
    End Sub

    ''' <summary>Otwiera podręcznik użytkownika programu w domyślnej przeglądarce internetowej.</summary>
    Public Shared Sub OtworzPomocOnline(Optional podstrona As String = "")
        Dim url = UrlPomocyOnline
        If Not String.IsNullOrWhiteSpace(podstrona) Then
            url = UrlPomocyOnline.TrimEnd("/"c) & "/" & podstrona.TrimStart("/"c)
        End If
        Try
            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć strony pomocy: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
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
