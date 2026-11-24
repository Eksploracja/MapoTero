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

    ''' <summary>Bok segmentu wpisany ostatnio automatycznie przez program wg limitów serwerów kanapki.</summary>
    Private _automatycznyBokSegmentu As Integer = 2000
    Private _bokSegmentuZmienionyRecznie As Boolean = False
    Private _biezacyLimitSegmentu As Integer = 0
    Private _lnkOstrzezenieSegmentu As LinkLabel

    ''' <summary>Katalog warstw: grupy tematyczne drzewa i nazwy wyświetlane warstw (warstwy\katalog.txt).</summary>
    Private _katalog As New KatalogWarstw()

    ''' <summary>Nazwy wyświetlane i identyfikatory warstw według serwera i nazwy warstwy (<see cref="KatalogWarstw.MapaNazw"/>).</summary>
    Private _nazwyWarstw As New Dictionary(Of String, (Nazwa As String, Grupa As String, Identyfikator As String))

    ''' <summary>Czytelne nazwy warstw i zbiorów zapisane w plikach zbiorów dodanych przez użytkownika.</summary>
    Private _tytulyDodanychWarstw As New Dictionary(Of String, (Nazwa As String, Grupa As String))(StringComparer.Ordinal)

    Private Const GrupaDodanychSerwerow As String = "Dodane serwery"

    ''' <summary>Zbiory map z warstwy.txt w kolejności drzewa warstw.</summary>
    Private ReadOnly _zbiory As New List(Of ZbiorMapy)
    ''' <summary>Lokalny nasłuch HTTP pozwalający stronie galerii warstw dodać wybraną warstwę lub kanapkę do programu.</summary>
    Private _mostekGalerii As MostekGalerii
    Private _silnikPodgladuGalerii As PodgladWarstwSilnik
    Private _ikonyPodgladuDrzewa As PomocnikIkonPodgladuDrzewa
    Private ReadOnly _semaforPodgladuGalerii As New SemaphoreSlim(1, 1)

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

    ''' <summary>Silnik bieżącego podglądu zestawu warstw w dolnym panelu.</summary>
    Private ReadOnly _silnikPodgladu As New PodgladWarstwSilnik()

    ''' <summary>Timer opóźniający pobieranie podglądu (debouncing 350 ms).</summary>
    Private ReadOnly _timerPodgladu As New System.Windows.Forms.Timer With {.Interval = 350}

    ''' <summary>Żeton anulowania trwającego pobierania podglądu.</summary>
    Private _ctsPodglad As CancellationTokenSource

    ''' <summary>Trwające zadanie pobierania podglądu panelu (mikser może na nie poczekać zamiast przerywać).</summary>
    Private _zadaniePodgladuPanelu As Threading.Tasks.Task

    ''' <summary>Zapamiętany punkt próbki ze środka mapy, gdy nie ma jeszcze poprawnej siatki.</summary>
    Private _punktProbkiBezSiatki As PunktXY?

    ''' <summary>
    ''' Poziomy powiększenia podglądu (1.0 = skala 1:1, piksel w piksel). 0,5 pokazuje większy obszar pobranej próbki;
    ''' 4.0 w panelu 116 px pokazywało tylko 29 x 29 pikseli danych, więc go nie ma.
    ''' </summary>
    Private ReadOnly _poziomyZoomu As Double() = {0.15, 0.25, 0.33, 0.5, 0.75, 1.0, 1.5, 2.0, 3.0, 4.0}
    ' Brak wartości oznacza dopasowanie do aktualnej wielkości panelu.
    Private _zoomPodgladu As Double?

    ''' <summary>Bazowa złożona bitmapa próbki w skali 1:1 o wymiarach BokProbki x BokProbki.</summary>
    Private _bazaPodgladu As Bitmap

    ''' <summary>Podgląd złożony bez części warstw (błąd serwera) - obraz dostaje czerwoną ramkę.</summary>
    Private _brakWarstwPodgladu As Boolean

    ''' <summary>Zapamiętany punkt próbki w segmencie korytarza i parametry, dla których go wyznaczono.</summary>
    Private _punktKorytarza As PunktXY?
    Private _kluczPunktuKorytarza As String = ""

    ''' <summary>Obszar pobierania wzdłuż trasy (zapisywany w folderze sesji w korytarz.txt); Nothing - cały prostokąt.</summary>
    Friend _korytarz As Korytarz
    Private _dzialkaKatastralna As DzialkaKatastralna

    ''' <summary>Numeracja segmentów z conf.txt wczytanej sesji (pusta - brak conf.txt lub plik z bardzo starej wersji).</summary>
    Private _numeracjaSesji As String = ""

    ''' <summary>Data ostatniego pobierania sesji (rrrr-mm-dd) - z conf.txt, ustawiana na nowo przy rozpoczęciu pobierania.</summary>
    Private _dataPobrania As String = ""

    ''' <summary>Najwięcej segmentów korytarza rysowanych na mapie (przy większej liczbie - tylko linia trasy).</summary>
    Private Const MaksSegmentowNaMapie As Integer = 3000
    Private ReadOnly _pedzelZewnetrzny As New SolidBrush(Color.FromArgb(40, Color.RoyalBlue))
    Private ReadOnly _pioroZewnetrzne As New Pen(Color.FromArgb(140, Color.RoyalBlue), 2)
    Private ReadOnly _pedzelSiatki As New SolidBrush(Color.FromArgb(40, Color.RoyalBlue))
    Private ReadOnly _pioroSiatki As New Pen(Color.FromArgb(140, Color.RoyalBlue), 1)
    Private ReadOnly _pioroTrasy As New Pen(Color.Red, 2)
    Private ReadOnly _pedzelDzialki As New SolidBrush(Color.FromArgb(45, Color.OrangeRed))
    Private ReadOnly _pioroDzialki As New Pen(Color.FromArgb(200, Color.DarkRed), 2)
    Private _wstrzymajPolaZasiegu As Boolean
    Private _cacheConfSciezka As String = ""
    Private _cacheConfCzasUtc As DateTime = DateTime.MinValue
    Private _cacheConfObiekt As KonfiguracjaSesji

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

    ''' <summary>
    ''' Nazwy pól dla czytników ekranu - bez nich Windows brał nazwę z poprzedniej etykiety w kolejności kontrolek
    ''' (np. pole „Segment [pix]” czytane było jako „Brak warstw”, pole X dolne jako „=”).
    ''' </summary>
    Private Sub UstawNazwyDostepnosci()
        txtXDol.AccessibleName = "Zasięg: X dolne (północ) w metrach"
        txtXGora.AccessibleName = "Zasięg: X górne (północ) w metrach"
        txtYLewy.AccessibleName = "Zasięg: Y lewe (wschód) w metrach"
        txtYPrawy.AccessibleName = "Zasięg: Y prawe (wschód) w metrach"
        txtRozmiarPiksela.AccessibleName = "Rozmiar piksela w metrach"
        txtBokSegmentu.AccessibleName = "Bok segmentu w pikselach"
        txtZasiegSegmentuKm.AccessibleName = "Zasięg segmentu w terenie w kilometrach"
        txtSzerokoscKm.AccessibleName = "Szerokość siatki w kilometrach"
        txtWysokoscKm.AccessibleName = "Wysokość siatki w kilometrach"
        txtSzerokoscPx.AccessibleName = "Szerokość siatki w pikselach"
        txtWysokoscPx.AccessibleName = "Wysokość siatki w pikselach"
        txtLiczbaKolumn.AccessibleName = "Liczba kolumn segmentów"
        txtLiczbaWierszy.AccessibleName = "Liczba wierszy segmentów"
        txtSzukajWarstwy.AccessibleName = "Szukaj warstwy"
        txtKomunikaty.AccessibleName = "Komunikaty"
        btnWpiszWspolrzedne.AccessibleName = "Wpisz współrzędne zasięgu XY"
        cmbRozmiarPiksela.AccessibleName = "Typowe rozdzielczości piksela"
        cmbFormatDocelowy.AccessibleName = "Format docelowy mapy"
        btnPrzejdzDoEksportu.AccessibleName = "Otwórz eksport dla wybranego formatu"
        btnPodgladZaawansowane.AccessibleName = "Mikser warstw i krycia"
    End Sub

    ''' <summary>Wyświetla komunikat w polu komunikatów okna głównego.</summary>
    Public Sub Komunikat(tekst As String, kolor As Color)
        'w trakcie uruchamiania kolejne komunikaty (np. „Wczytano ostatnio zapisane ustawienia”) zastępowały wcześniejszy
        'błąd (uszkodzony conf.txt, brak zbiorów map) - błędy startu zostają widoczne, następne błędy są dopisywane
        If Not _zaladowany AndAlso txtKomunikaty.ForeColor = Color.Red AndAlso txtKomunikaty.Text <> "" Then
            If kolor <> Color.Red Then Exit Sub
            tekst = txtKomunikaty.Text & vbCrLf & tekst
        End If
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
        DopiszKomunikatSesji(tekst, kolor)
    End Sub

    ''' <summary>Włącza lub wyłącza ręczną edycję współrzędnych zasięgu.</summary>
    Public Sub UstawEdycjeXY(wlaczona As Boolean)
        Ustawienia.EdycjaXY = wlaczona
        If wlaczona Then ZakonczWyborNaroznikow()
        For Each c As Control In New Control() {txtXDol, txtYLewy, txtXGora, txtYPrawy, lblOpisXDol, lblOpisYLewy, lblOpisXGora, lblOpisYPrawy}
            c.Enabled = wlaczona
        Next
        _ustawianieEdycjiXY = True
        chkEdycjaXY.Checked = wlaczona
        _ustawianieEdycjiXY = False
        'przy wpisywaniu z klawiatury narożniki siatki są zwykłymi pinezkami, inaczej - uchwytami zmiany zasięgu
        If _siatkaWidoczna Then OdswiezSiatke()
    End Sub

    ''' <summary>Blokuje reakcję pola edycji XY na zaznaczenie ustawiane w kodzie (UstawEdycjeXY).</summary>
    Private _ustawianieEdycjiXY As Boolean

    ''' <summary>
    ''' Pole przy współrzędnych XY w schemacie siatki: wpisywanie zasięgu z klawiatury (dawniej opcja w oknie Ustawienia -
    ''' przełącza się ją przy samych polach, a nie w osobnym oknie). Stan zapamiętywany w lastsettings.txt.
    ''' </summary>
    Private Sub chkEdycjaXY_CheckedChanged(sender As Object, e As EventArgs) Handles chkEdycjaXY.CheckedChanged
        If _ustawianieEdycjiXY Then Exit Sub
        Dim wlaczona As Boolean = chkEdycjaXY.Checked
        If wlaczona = Ustawienia.EdycjaXY Then Exit Sub
        If _przeciaganyUchwyt IsNot Nothing Then ZakonczPrzeciaganie(anuluj:=True)
        Ustawienia.EdycjaXY = wlaczona
        UstawEdycjeXY(wlaczona)
        If wlaczona Then
            Komunikat("Wpisz współrzędne XY zasięgu pobierania w pola schematu siatki. Zaznaczanie obszaru prawym przyciskiem " &
                      "myszy i uchwyty na mapie są wyłączone, dopóki pole przy współrzędnych jest zaznaczone.", Color.Green)
        Else
            Komunikat("Zaznacz na mapie prawym przyciskiem myszy obszar do pobrania.", Color.Green)
        End If
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
            c.Visible = widoczne AndAlso _odczytKursora Is Nothing
        Next
        AktualizujWspolrzednePaska()
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

    ''' <summary>W trybie testowym okno nie łączy się z internetem (GMap.NET w trybie CacheOnly, brak pobierania podglądów).</summary>
    Friend Shared Property TrybTestowyBezSieci As Boolean = False

    Friend Shared ReadOnly Property TrybOkienBezAktywacji As Boolean
        Get
            Return TrybTestowyBezSieci AndAlso Environment.GetEnvironmentVariable("MAPOTERO_AUDYT") <> "1"
        End Get
    End Property

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            If TrybOkienBezAktywacji Then Return True
            Return MyBase.ShowWithoutActivation
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp = MyBase.CreateParams
            If TrybOkienBezAktywacji Then
                ' WS_EX_NOACTIVATE (&H08000000) | WS_EX_TOOLWINDOW (&H80) | WS_EX_TRANSPARENT (&H20):
                ' okno testowe nie kradnie fokusu, nie miesza w pasku zadań ani nie przechwytuje kliknięć myszy
                cp.ExStyle = cp.ExStyle Or &H8000000 Or &H80 Or &H20
            End If
            Return cp
        End Get
    End Property

    ''' <summary>
    ''' Układ okna (pasek narzędzi, panel warstw, belka, podgląd) budowany przed utworzeniem okien kontrolek. W zdarzeniu
    ''' Load okno i wszystkie kontrolki mają już uchwyty, więc każda dodana tam kontrolka od razu tworzyła swoje okno
    ''' i przeliczała układ - budowa trwała ok. 1 s; tutaj uchwyty powstają raz, przy pokazaniu okna.
    ''' Wymiary projektu potrzebne do najmniejszego rozmiaru okna są zapamiętywane przed przebudową układu.
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        If TrybOkienBezAktywacji Then
            ShowInTaskbar = False
            Opacity = 0
        End If
        Me.SetDesktopLocation(0, 0)
        ZapamietajWymiaryProjektu()
        InicjalizujPrzyciskiMapy()
        UstawNazwyDostepnosci()
        InicjalizujNowaSesje()
        InicjalizujKreator()
        InicjalizujKokpit()
        InicjalizujOstrzezenieSegmentu()
        UlozPanelePracy()
        PrzygotujListeWarstw()
        AddHandler _timerPodgladu.Tick, AddressOf TimerPodgladu_Tick
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RejestrujKodowania()
        btnGaleriaWarstw.Image = UtworzIkoneGalerii()
        'silnik (MapoTero.Silnik) nie zna ustawień programu - dla sesji bez numeracji w conf.txt pyta o bieżącą
        UstawieniaSilnika.NumeracjaDomyslna = Function() Ustawienia.Numeracja

        'wyświetla nazwę i wersję
        Me.Text = My.Application.Info.Title & " " & Program.Wersja

        'bez końcowego ukośnika (w .NET 8 StartupPath go zawiera - ścieżki miały postać ...\\download\)
        FolderProgramu = Application.StartupPath
        If String.IsNullOrEmpty(FolderProgramu) Then FolderProgramu = AppContext.BaseDirectory
        FolderProgramu = FolderProgramu.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        FolderDanych = UstalFolderDanych(FolderProgramu)
        Directory.CreateDirectory(FolderDownload)

        'wyszukanie folderu sesji: zapamiętany w lastsettings.txt (sam download - po Nowej sesji), najnowszy podkatalog w download/
        Dim zapamietany As String = ""
        If File.Exists(PlikLastsettings) Then
            Try
                zapamietany = PlikUstawien.Wczytaj(PlikLastsettings, KodowanieSystemowe()).Tekst("folder segmentow", "")
            Catch
            End Try
        End If
        Dim folderDoWczytania As String = KonfiguracjaSesji.FolderSesjiDoWczytania(zapamietany, FolderDownload)
        'zapamiętany folder sesji usunięto lub przeniesiono - dawniej program po cichu wczytywał inną (najnowszą) sesję
        If zapamietany <> "" AndAlso
           Not String.Equals(zapamietany.TrimEnd("\"c), FolderDownload.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) AndAlso
           Not String.Equals(zapamietany.TrimEnd("\"c), folderDoWczytania.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
            Komunikat("Nie znaleziono sesji z poprzedniego uruchomienia (folder " & zapamietany & " nie istnieje lub nie ma pliku conf.txt)" &
                      If(folderDoWczytania <> "", " - wczytano najnowszą sesję: " & folderDoWczytania, "") & ".", Color.Red)
        End If

        Dim nowaSesja As Boolean = String.IsNullOrEmpty(folderDoWczytania) OrElse File.Exists(Path.Combine(folderDoWczytania, "conf.txt")) = False
        If nowaSesja Then
            FolderSesji = FolderDownload
            btnScalanie.Enabled = False
            mapa.Position = New PointLatLng(52.3, 19.2)
            mapa.Zoom = 6
        Else
            FolderSesji = folderDoWczytania
            btnScalanie.Enabled = True
        End If

        'serwery WMS (np. Geoportal) wymagają TLS 1.2 - na starszych wersjach Windows nie jest on domyślnie włączony;
        'domyślnie .NET Framework ogranicza też liczbę jednoczesnych połączeń z serwerem do 2
        Net.ServicePointManager.SecurityProtocol = Net.ServicePointManager.SecurityProtocol Or Net.SecurityProtocolType.Tls12
#If Not NETCOREAPP Then
        Net.ServicePointManager.DefaultConnectionLimit = 16
#End If

        UstawRozmiarOkna()

        'ustawienia startowe okna mapy
        mapa.Manager.Mode = If(TrybTestowyBezSieci, AccessMode.CacheOnly, AccessMode.ServerAndCache)
        mapa.DragButton = MouseButtons.Left
        mapa.MapScaleInfoEnabled = False
        mapa.DisableAltForSelection = True
        mapa.Zoom = Wartosc(lblZoom.Text, 6)
        If Not TrybTestowyBezSieci Then
            Me.AllowDrop = True
            mapa.AllowDrop = True
        End If

        UstawEdycjeXY(False)
        UstawWidokZaznaczeniaWgs(False)

        _ustawianieUkladu = True
        For Each u In UkladWspolrzednych.Wszystkie
            cmbUklad.Items.Add(u)
        Next
        _ustawianieUkladu = False

        'wczytywanie parametrów aktywnej sesji z conf.txt, potem ustawień programu z lastsettings.txt
        WczytajConf(False)
        WczytajLastsettings(nowaSesja)

        'nowa sesja (brak conf.txt) - układ współrzędnych wybrany w ustawieniach jako domyślny
        If nowaSesja Then PrzelaczUklad(Ustawienia.UkladDomyslny, False)

        'widok okna głównego zapamiętany w lastsettings.txt
        UstawWidokWspolrzednych(Ustawienia.KursorISrodekMapy)
        UstawWidokZaznaczeniaWgs(Ustawienia.ZaznaczenieWgs84)
        UstawEdycjeXY(Ustawienia.EdycjaXY)
        UstawSzerokoscPanelu(Ustawienia.SzerokoscPanelu)
        UstawPodkladMapy(Ustawienia.PodkladMapy)

        'drzewo zbiorów map - warstwy wczytane z conf.txt zostają (każda pamięta swój serwer, więc zbiór sesji na liście
        'ich nie czyści; dawniej po ponownym uruchomieniu warstwy sesji znikały i trzeba je było wybrać od nowa)
        WczytajListeZbiorowMap(_biezacyZbior)
        _ikonyPodgladuDrzewa = New PomocnikIkonPodgladuDrzewa(
            tvWarstwy,
            Program.PobierzMagazynGalerii(TrybTestowyBezSieci),
            Function(wezel)
                Dim w = DrzewoWarstw.WarstwaWezla(wezel)
                If w Is Nothing OrElse w.Zbior Is Nothing Then Return ("", "")
                Return (w.Zbior.Nazwa, w.Warstwa)
            End Function)
        If _zestaw.Liczba > 0 Then
            Komunikat("Wczytano sesję pobierania z pliku conf.txt, warstwy: " & OpisWarstw() & ".", Color.Green)
        End If
        PokazWarstwy()

        If Ustawienia.Format = "" Then Ustawienia.Format = "jpeg"
        stFormat.Text = "." & Ustawienia.Format
        stSegment.Text = ""
        OdswiezOpisUkladu()
        PrzeliczSiatke()

        'wczytana sesja: gdy zapamiętany widok mapy nie pokazuje jej obszaru (poza oknem albo kilka pikseli),
        'mapa przybliża się do obszaru - dopiero po pokazaniu okna, bo wcześniej mapa nie ma docelowego rozmiaru
        If Not nowaSesja Then
            AddHandler Shown, Sub()
                                  If Not ObszarCzytelnyNaMapie() Then PokazObszarNaMapie()
                              End Sub
        End If
        AddHandler mapa.Paint, AddressOf RysujPodzialke

        _zaladowany = True
    End Sub


    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _timerPodgladu.Stop()
        _timerPodgladu.Dispose()
        _przerwanie?.Cancel()
        _ctsPodglad?.Cancel()
        _bazaPodgladu?.Dispose()
        _bazaPodgladu = Nothing
        _silnikPodgladu.Dispose()
        _mostekGalerii?.Dispose()
        _mostekGalerii = Nothing
        _silnikPodgladuGalerii?.Dispose()
        _silnikPodgladuGalerii = Nothing
        _ikonyPodgladuDrzewa?.Dispose()
        _ikonyPodgladuDrzewa = Nothing
        WyczyscZnaczniki()
        _pedzelZewnetrzny.Dispose()
        _pioroZewnetrzne.Dispose()
        _pedzelSiatki.Dispose()
        _pioroSiatki.Dispose()
        _pioroTrasy.Dispose()
        _pedzelDzialki.Dispose()
        _pioroDzialki.Dispose()
        ZapamietajPolozenieMapy()
        Try
            Ustawienia.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Ustawienia")
        End Try
    End Sub

#End Region

#Region "Rozmiar okna"

    ''' <summary>
    ''' Najmniejszy rozmiar okna i rozmiar startowy dopasowany do ekranu. Okno domyślnie otwiera się jako
    ''' zmaksymalizowane (Me.WindowState = FormWindowState.Maximized), a po przywróceniu zachowuje dopasowany
    ''' rozmiar bazowy. Lista warstw WMS rozciąga się razem z oknem (kotwice w projekcie formularza),
    ''' więc okno można zmniejszyć o tyle, o ile da się skrócić tę listę; na ekranie niższym nawet od tego
    ''' prawy panel jest przewijany.
    ''' </summary>
    ''' <summary>
    ''' Wymiary z projektu formularza, liczone w konstruktorze przed przebudową układu (UlozPanelePracy przenosi drzewo
    ''' warstw do nowego panelu i ustawia własną najmniejszą wysokość panelu). Bez odwołania do ekranu -
    ''' Screen.FromControl tworzyłby uchwyt okna przed zbudowaniem układu.
    ''' </summary>
    Private Sub ZapamietajWymiaryProjektu()
        Dim minWysokoscListy As Integer = tvWarstwy.ItemHeight * MinWierszyListyWarstw + (tvWarstwy.Height - tvWarstwy.ClientSize.Height)
        _zapasListyProjektu = Math.Max(0, tvWarstwy.Height - minWysokoscListy)
        _minSzerokoscPanelu = Panel2.Width
        _minSzerokoscMapy = Math.Min(ClientSize.Width - Panel2.Width - splPanel.Width, Px(560))
        _szerokoscPanelu = _minSzerokoscPanelu
        PrzypnijNakladkiDoMapy()
        _minWysokoscPanelu = Panel2.ClientSize.Height - _zapasListyProjektu
        Panel2.AutoScrollMinSize = New Size(0, _minWysokoscPanelu)
        splPanel.MinSize = _minSzerokoscPanelu
        splPanel.MinExtra = _minSzerokoscMapy
        ToolTip1.SetToolTip(splPanel, "Przeciągnij, aby zmienić szerokość prawego panelu")
    End Sub

    ''' <summary>O ile listę warstw z projektu formularza można skrócić (najmniejsza wysokość okna).</summary>
    Private _zapasListyProjektu As Integer

    Private Sub UstawRozmiarOkna()
        Dim zapasListy As Integer = _zapasListyProjektu
        Dim ekran As Rectangle = Screen.FromControl(Me).WorkingArea
        Me.MinimumSize = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height - zapasListy, ekran.Height))
        Me.Size = New Size(Math.Min(Me.Width, ekran.Width), Math.Min(Me.Height, ekran.Height))
        Me.WindowState = FormWindowState.Maximized
    End Sub

    ''' <summary>Szerokość prawego panelu zapamiętana w lastsettings.txt (0 - domyślna).</summary>
    Private Sub UstawSzerokoscPanelu(szerokosc As Integer)
        'domyślnie co najmniej 27% szerokości ekranu - przy węższym panelu nazwy warstw z katalogu były ucięte
        Dim domyslna = Math.Max(CInt(_minSzerokoscPanelu * 1.48), CInt(Screen.FromControl(Me).WorkingArea.Width * 0.27))
        _szerokoscPanelu = If(szerokosc > 0, Math.Max(_minSzerokoscPanelu, szerokosc), domyslna)
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
        'plik z danymi binarnymi albo bez wpisu „folder segmentow” (pierwszego we wszystkich wersjach programu, także sprzed
        '3.12) jest uszkodzony - dawniej program pisał „Wczytano ostatnio zapisane ustawienia”, choć używał domyślnych
        Dim poprawny As Boolean = False
        Try
            poprawny = Not PlikUstawien.WygladaNaUszkodzony(File.ReadAllText(PlikLastsettings, KodowanieSystemowe()))
        Catch
        End Try
        If Not poprawny Then
            Komunikat("Plik " & PlikLastsettings & " jest uszkodzony - użyto domyślnych ustawień programu (zostaną zapisane przy zamknięciu programu).", Color.Red)
            Exit Sub
        End If
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
            dlgFolder.SelectedPath = If(Directory.Exists(FolderSesji), FolderSesji, FolderDownload)
            If dlgFolder.ShowDialog <> DialogResult.OK Then Exit Sub
            folder = FolderZUkosnikiem(dlgFolder.SelectedPath)
            If File.Exists(folder & "conf.txt") = False Then
                MessageBox.Show("W folderze " & folder & " nie ma pliku conf.txt - to nie jest folder sesji pobierania.", "Wczytywanie sesji",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
        End If
        WczytajConfZFolderu(folder, pytajOFolder)
    End Sub

    ''' <summary>
    ''' Odczyt conf.txt z folderu. Folder sesji zmienia się dopiero po udanym odczycie - dawniej uszkodzony conf.txt
    ''' wskazanej sesji przełączał folder sesji, choć warstwy i zasięg zostawały z poprzedniej (okna scalania i eksportu
    ''' otwierały wtedy folder z nieczytelnym conf.txt).
    ''' </summary>
    Friend Sub WczytajConfZFolderu(folder As String, Optional wybranyPrzezUzytkownika As Boolean = False)
        ZakonczWyborNaroznikow()
        If File.Exists(folder & "conf.txt") = False Then
            FolderSesji = folder
            Exit Sub
        End If

        Dim c As KonfiguracjaSesji
        Try
            c = KonfiguracjaSesji.Wczytaj(folder & "conf.txt", KodowanieSystemowe())
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku " & folder & "conf.txt (" & OpisBledu.Z(ex) & "). Plik sesji jest uszkodzony - " &
                      If(wybranyPrzezUzytkownika, "bieżąca sesja nie została zmieniona.", "wybierz warstwy i zasięg od nowa albo wczytaj inną sesję."), Color.Red)
            If Not wybranyPrzezUzytkownika Then FolderSesji = folder
            Exit Sub
        End Try
        FolderSesji = folder

        'zbiór map sesji - nazwa sesji przy pustej kanapce, rozwijany w drzewie warstw
        If c.RodzajMapy <> "" Then _biezacyZbior = c.RodzajMapy
        Ustawienia.Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
        WpiszPolaZasiegu(c.XDol, c.YLewy, c.XGora, c.YPrawy)
        txtBokSegmentu.Text = c.BokSegmentuPx
        _bokSegmentuZmienionyRecznie = True
        _automatycznyBokSegmentu = WartoscCalkowita(c.BokSegmentuPx)
        UkryjOstrzezenieSegmentu()
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
        'tryb TrekBuddy jest parametrem sesji (nazwy segmentów, paczka .tar) - dawniej niezapisywany, więc po ponownym
        'uruchomieniu wznowione pobieranie sesji TrekBuddy zapisywało w niej zwykłe segmenty; bok segmentu jest w conf.txt
        Ustawienia.TrekBuddy = c.TrekBuddy
        Ustawienia.BokSegmentuPrzedTrekBuddy = ""
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
        _ostatniWynikPobierania = Nothing
        AktualizujStanBelki()
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
            _cacheConfSciezka = ""
            _cacheConfObiekt = Nothing
            'obszar wzdłuż trasy - plik korytarz.txt obok conf.txt (bez korytarza - usunięcie nieaktualnego pliku)
            Dim plikKorytarza As String = FolderSesji & Korytarz.NazwaPliku
            If _korytarz IsNot Nothing Then
                _korytarz.Zapisz(plikKorytarza)
            ElseIf File.Exists(plikKorytarza) Then
                File.Delete(plikKorytarza)
            End If
        Catch ex As Exception
            Komunikat("Nie udało się zapisać pliku conf.txt: " & OpisBledu.Z(ex), Color.Red)
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
            .TrekBuddy = Ustawienia.TrekBuddy,
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
    ''' (sesja nieznana - np. folder z segmentami spoza programu) przy plikach o nazwach segmentów bieżącej sesji.
    ''' Zwraca liczbę takich plików (0 - folder pusty, nowy albo z tą samą sesją, czyli wznowienie) i opis przyczyny.
    ''' </summary>
    Private Function SegmentyInnejSesji(folder As String, ByRef opis As String) As Integer
        opis = ""
        Dim biezaca = BiezacaKonfiguracja()
        Dim plikConf As String = folder & "conf.txt"
        If File.Exists(plikConf) Then
            Dim zapisana = PobierzZapisanaKonfiguracje(plikConf)
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
    ''' Przed pobieraniem: przygotowanie dedykowanego podfolderu sesji w katalogu download.
    ''' Każda nowa sesja pobierania otrzymuje od początku własny podkatalog sesji (np. download\sesja_ortofotomapa_...),
    ''' dzięki czemu pliki nie zaśmiecają głównego folderu download, a warstwy nie kolidują ze sobą.
    ''' Wznowienie tej samej sesji (identyczna siatka i parametry) kontynuuje zapis w jej podfolderze.
    ''' Gdy w bieżącym podfolderze znajdują się segmenty innej sesji, tworzony jest nowy podfolder.
    ''' False - pobieranie anulowane.
    ''' </summary>
    Private Function PrzygotujFolderSesji() As Boolean
        Dim czyGlownyDownload As Boolean = String.IsNullOrWhiteSpace(FolderSesji) OrElse
            String.Equals(Path.GetFullPath(FolderSesji).TrimEnd("\"c, "/"c),
                          Path.GetFullPath(FolderDownload).TrimEnd("\"c, "/"c),
                          StringComparison.OrdinalIgnoreCase)

        If czyGlownyDownload Then
            ' Nowa sesja startująca z folderu download - od początku tworzymy dedykowany podkatalog sesji
            Return UtworzPodfolderSesji()
        End If

        ' FolderSesji wskazuje podkatalog: sprawdzamy, czy wznawiamy tę samą sesję, czy parametry się zmieniły
        Dim opis As String = ""
        Dim liczba As Integer = SegmentyInnejSesji(FolderSesji, opis)
        If liczba = 0 Then
            'conf.txt innej mapy bez pobranych segmentów (np. pobieranie przerwane na starcie): nowa mapa dostaje własny
            'podfolder - dawniej trafiała do folderu nazwanego od poprzedniej mapy i nadpisywała jego conf.txt
            If opis <> "" AndAlso File.Exists(FolderSesji & "conf.txt") Then
                Return UtworzPodfolderSesji()
            End If
            ' Ta sama siatka / parametry (lub pusty podkatalog) - wznowienie pobierania w tym samym folderze
            Return True
        End If

        ' Inna siatka lub inne warstwy przy istniejących już plikach - automatyczne utworzenie nowego podfolderu sesji,
        ' aby nie pomieszać segmentów różnych map w jednym katalogu (Wariant 2: pełna automatyzacja bez uciążliwych pytań)
        If Not UtworzPodfolderSesji() Then Return False
        Komunikat("Wykryto zmianę mapy lub warstw - pobieranie do nowego folderu: " & Path.GetFileName(FolderSesji.TrimEnd("\"c, "/"c)), Color.Green)
        Return True
    End Function
    ''' <summary>Tworzy nowy podfolder sesji w download i ustawia go jako folder sesji. False - nie udało się.</summary>
    Private Function UtworzPodfolderSesji() As Boolean
        Dim skrot = IdentyfikatorBiezacychWarstw()
        Dim nazwaFolderu = KonfiguracjaSesji.ProponowanaNazwaFolderuSesji(If(Not String.IsNullOrWhiteSpace(skrot), skrot, ZbiorMap), Date.Now)
        Dim nowyFolder As String = Path.Combine(FolderDownload, nazwaFolderu) & "\"
        Try
            Directory.CreateDirectory(nowyFolder)
            FolderSesji = nowyFolder
            Return True
        Catch ex As Exception
            Komunikat("Nie udało się utworzyć folderu sesji " & nowyFolder & ": " & OpisBledu.Z(ex), Color.Red)
            Return False
        End Try
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

        '1. zbiory w folderze danych użytkownika (mają pierwszeństwo przed dystrybucją)
        If Not String.Equals(FolderWarstwProgramu, FolderWarstwDanych, StringComparison.OrdinalIgnoreCase) Then
            DodajZbioryZIndeksu(Path.Combine(FolderWarstwDanych, "warstwy.txt"), znalezione, dodane)
            SkanujNieindeksowaneZbiory(FolderWarstwDanych, znalezione, dodane)
        End If

        '2. zbiory w folderze programu
        DodajZbioryZIndeksu(Path.Combine(FolderWarstwProgramu, "warstwy.txt"), znalezione, dodane)

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
                    z.Blad = OpisBledu.Z(ex)
                End Try
            End If
            _zbiory.Add(z)
        Next
        WczytajKatalog()

        If _zbiory.Count = 0 Then
            Komunikat("Brak zbiorów map: nie znaleziono pliku " & Path.Combine(FolderWarstwProgramu, "warstwy.txt") & ". Skopiuj folder warstwy z archiwum programu obok pliku MapoTero.exe.", Color.Red)
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
                Komunikat("Nie udało się odczytać katalogu warstw: " & OpisBledu.Z(ex), Color.Red)
            End Try
        End If
        KatalogWarstw.Instancja = _katalog
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
        Dim n As (Nazwa As String, Grupa As String, Identyfikator As String) = Nothing
        Dim klucz = KatalogWarstw.KluczAliasu(w.Adres, w.Nazwa)
        If _nazwyWarstw.TryGetValue(klucz, n) Then Return n.Nazwa
        Dim nd As (Nazwa As String, Grupa As String) = Nothing
        If _tytulyDodanychWarstw.TryGetValue(klucz, nd) Then Return nd.Nazwa
        Return w.Nazwa
    End Function

    ''' <summary>Źródło warstwy kanapki do wyświetlania: grupa z katalogu warstw albo tytuł zbioru, inaczej nazwa pliku zbioru.</summary>
    Friend Function ZrodloWarstwy(w As WarstwaWybrana) As String
        Dim n As (Nazwa As String, Grupa As String, Identyfikator As String) = Nothing
        Dim klucz = KatalogWarstw.KluczAliasu(w.Adres, w.Nazwa)
        If _nazwyWarstw.TryGetValue(klucz, n) Then Return n.Grupa
        Dim nd As (Nazwa As String, Grupa As String) = Nothing
        If _tytulyDodanychWarstw.TryGetValue(klucz, nd) Then Return nd.Grupa
        Return w.Zbior
    End Function

    ''' <summary>Zwięzły identyfikator warstwy (maksymalnie 20 znaków, np. Topo_65_10k) z katalogu warstw.</summary>
    Friend Function IdentyfikatorWarstwy(w As WarstwaWybrana) As String
        If w Is Nothing Then Return ""
        Dim n As (Nazwa As String, Grupa As String, Identyfikator As String) = Nothing
        Dim klucz = KatalogWarstw.KluczAliasu(w.Adres, w.Nazwa)
        If _nazwyWarstw.TryGetValue(klucz, n) AndAlso Not String.IsNullOrWhiteSpace(n.Identyfikator) Then
            Return n.Identyfikator
        End If
        Return w.Nazwa
    End Function

    ''' <summary>Zwięzły identyfikator bieżącej konfiguracji warstw do nazwy folderu sesji lub eksportu.</summary>
    Friend Function IdentyfikatorBiezacychWarstw() As String
        If _zestaw Is Nothing OrElse _zestaw.Liczba = 0 Then Return ""
        If _zestaw.Liczba = 1 Then
            Dim id1 = IdentyfikatorWarstwy(_zestaw.Warstwy(0))
            Return If(Not String.IsNullOrWhiteSpace(id1), id1, ZbiorMap)
        Else
            Dim skroty = _zestaw.Warstwy.Select(Function(w) IdentyfikatorWarstwy(w)).Where(Function(s) Not String.IsNullOrWhiteSpace(s)).ToList()
            If skroty.Count = 0 Then Return ZbiorMap
            Dim polaczone = String.Join("+", skroty)
            If polaczone.Length > 30 Then
                Return skroty(0) & "_mix"
            End If
            Return polaczone
        End If
    End Function

    Private Sub DodajZbioryZIndeksu(plik As String, lista As List(Of String), dodane As HashSet(Of String))
        If Not File.Exists(plik) Then Exit Sub
        For Each linia In File.ReadAllLines(plik, PlikWarstw.Kodowanie())
            Dim nazwa As String = linia.Trim().Trim(""""c)
            If nazwa <> "" AndAlso dodane.Add(nazwa) Then lista.Add(nazwa)
        Next
    End Sub

    ''' <summary>Wyszukuje pliki *.txt w folderze danych, które nie są wymienione w indeksie warstwy.txt i dodaje poprawne zbiory.</summary>
    Private Sub SkanujNieindeksowaneZbiory(folder As String, lista As List(Of String), dodane As HashSet(Of String))
        If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then Exit Sub
        Try
            For Each sciezka In Directory.EnumerateFiles(folder, "*.txt")
                Dim plik = Path.GetFileName(sciezka)
                If String.Equals(plik, "warstwy.txt", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(plik, "katalog.txt", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If
                Dim nazwa = Path.GetFileNameWithoutExtension(plik)
                If dodane.Contains(nazwa) Then Continue For
                If PlikWarstw.CzyWygladaNaZbiorMap(sciezka) Then
                    If dodane.Add(nazwa) Then lista.Add(nazwa)
                End If
            Next
        Catch
        End Try
    End Sub

    ''' <summary>Ścieżka pliku zbioru map (folder danych użytkownika ma pierwszeństwo, potem folder programu); pusty tekst - brak pliku.</summary>
    Private Function SciezkaZbioruMap(zbior As String) As String
        Dim plik As String = Path.Combine(FolderWarstwDanych, zbior & ".txt")
        If File.Exists(plik) Then Return plik
        plik = Path.Combine(FolderWarstwProgramu, zbior & ".txt")
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
        Dim wezel As New TreeNode(g.Nazwa) With {.Name = klucz, .Tag = g.Nazwa, .ToolTipText = g.Nazwa}
        For Each d In g.Dzieci
            If d.CzyWarstwa Then
                Dim z = ZnajdzZbior(d.Zbior)
                If z Is Nothing OrElse z.Plik Is Nothing Then Continue For
                If szukanie AndAlso Not FiltrWarstw.Pasuje(nazwy & " " & z.Nazwa, d.Nazwa & " " & d.Warstwa & If(d.Identyfikator <> "", " " & d.Identyfikator, ""), fraza) Then Continue For
                wezel.Nodes.Add(WezelWarstwy(z, d.Warstwa, d.Nazwa, klucz, d.Identyfikator))
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
    Private Function WezelWarstwy(z As ZbiorMapy, warstwa As String, nazwa As String, kluczRodzica As String, Optional identyfikator As String = "") As TreeNode
        Dim wm = z.Plik.Znajdz(warstwa)
        Dim opisTechniczny As String = KafleXyz.OpisWarstwy(z.Plik.EfektywnyTyp, z.Plik.OpcjeKafli(warstwa))
        Dim naglowek As String = If(Not String.IsNullOrWhiteSpace(identyfikator), nazwa & " [" & identyfikator & "]", nazwa)
        Dim opis As String = naglowek & vbCrLf & "Warstwa " & warstwa & " zbioru map " & z.Nazwa &
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
        btnZwinRozwin.Text = If(zwin, "Zwiń", "Rozwiń")
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
        If _ikonyPodgladuDrzewa IsNot Nothing AndAlso _ikonyPodgladuDrzewa.CzyPunktNaIkonie(e.Location) Then Exit Sub
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

    ''' <summary>Lista wybranych warstw: menu, podpowiedzi i nazwy warstw.</summary>
    Private Sub PrzygotujListeWarstw()
        lstWybraneWarstwy.Podpowiedz = ToolTip1
        lstWybraneWarstwy.OpisListy = ToolTip1.GetToolTip(grpWybraneWarstwy)
        lstWybraneWarstwy.Zestaw = _zestaw
        lstWybraneWarstwy.NazwaWarstwy = AddressOf NazwaWarstwy
        lstWybraneWarstwy.ZrodloWarstwy = AddressOf ZrodloWarstwy

        _menuWarstwy = New ContextMenuStrip(components)
        Dim miWyzej = New ToolStripMenuItem("Przesuń wyżej", Nothing, Sub() PrzesunZaznaczonaWarstwe(1))
        Dim miNizej = New ToolStripMenuItem("Przesuń niżej", Nothing, Sub() PrzesunZaznaczonaWarstwe(-1))
        Dim sepKrycie As New ToolStripSeparator()
        Dim pozycjeKrycia As New List(Of ToolStripMenuItem)()
        _menuWarstwy.Items.Add(miWyzej)
        _menuWarstwy.Items.Add(miNizej)
        _menuWarstwy.Items.Add(sepKrycie)
        For Each k In {100, 75, 50, 25}
            Dim krycie As Integer = k
            Dim poz As New ToolStripMenuItem("Krycie " & krycie & " %", Nothing, Sub() UstawKrycieZaznaczonej(krycie)) With {.Tag = krycie}
            pozycjeKrycia.Add(poz)
            _menuWarstwy.Items.Add(poz)
        Next
        Dim miZaaw = New ToolStripMenuItem("Krycie - zaawansowane...", Nothing, Sub() OtworzWarstwyZaawansowane())
        Dim sepUsun As New ToolStripSeparator()
        Dim miUsun = New ToolStripMenuItem("Usuń warstwę", My.Resources.kosz, Sub() UsunZaznaczonaWarstwe())
        Dim sepKanapka As New ToolStripSeparator()
        Dim miKopiujKanapke = New ToolStripMenuItem("Kopiuj kanapkę (JSON)", Nothing, Sub() KopiujKanapkeDoSchowka())
        Dim miWklejKanapke = New ToolStripMenuItem("Wklej kanapkę ze schowka", Nothing, Sub() WklejKanapkeZeSchowka())
        Dim miWczytajKanapke = New ToolStripMenuItem("Wczytaj kanapkę z pliku...", Nothing, Sub() WczytajKanapkeZPliku())
        _menuWarstwy.Items.AddRange({miZaaw, sepUsun, miUsun, sepKanapka, miKopiujKanapke, miWklejKanapke, miWczytajKanapke})

        AddHandler _menuWarstwy.Opening,
            Sub(s, e)
                Dim i As Integer = lstWybraneWarstwy.Zaznaczona
                Dim maZaznaczona As Boolean = i >= 0 AndAlso i < _zestaw.Liczba
                miWyzej.Visible = maZaznaczona
                miNizej.Visible = maZaznaczona
                sepKrycie.Visible = maZaznaczona
                miZaaw.Visible = maZaznaczona
                sepUsun.Visible = maZaznaczona
                miUsun.Visible = maZaznaczona
                sepKanapka.Visible = maZaznaczona
                If maZaznaczona Then
                    miWyzej.Enabled = i < _zestaw.Liczba - 1
                    miNizej.Enabled = i > 0
                End If
                For Each poz In pozycjeKrycia
                    poz.Visible = maZaznaczona
                    If maZaznaczona Then poz.Checked = CInt(poz.Tag) = _zestaw(i).Krycie
                Next
                miKopiujKanapke.Enabled = _zestaw.Liczba > 0
                miWklejKanapke.Enabled = _przerwanie Is Nothing
                miWczytajKanapke.Enabled = _przerwanie Is Nothing
            End Sub
        lstWybraneWarstwy.MenuWarstwy = _menuWarstwy
    End Sub

    ''' <summary>Wyświetla wybrane warstwy (lista z kanapką, pola wyboru w drzewie warstw) i stan przycisków panelu.</summary>
    Private Sub PokazWarstwy()
        If lstWybraneWarstwy.Zestaw IsNot _zestaw Then lstWybraneWarstwy.Zestaw = _zestaw Else lstWybraneWarstwy.Odswiez()
        OdswiezZaznaczeniaDrzewa()
        OdswiezPrzyciskiWarstw()
        If _zaladowany Then PrzeliczSiatke()
        ZglosOdswiezeniePodgladu()
    End Sub

    ''' <summary>Przyciski i pole krycia wg zaznaczonej warstwy.</summary>
    Private Sub OdswiezPrzyciskiWarstw()
        DopasujSekcjeWarstw()
        Dim i As Integer = lstWybraneWarstwy.Zaznaczona
        Dim pobieranie As Boolean = _przerwanie IsNot Nothing
        btnUsunWarstwe.Enabled = (i >= 0 OrElse _zestaw.Liczba = 1) AndAlso _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnWarstwaWyzej.Enabled = i >= 0 AndAlso i < _zestaw.Liczba - 1 AndAlso Not pobieranie
        btnWarstwaNizej.Enabled = i > 0 AndAlso Not pobieranie
        numKrycie.Enabled = i >= 0 AndAlso Not pobieranie
        lblKrycie.Enabled = numKrycie.Enabled
        btnWarstwyZaawansowane.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnPodgladZaawansowane.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnPodgladOdswiez.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        AktualizujStanPrzyciskowZoomu()
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
        UniewaznijWynikPobierania()
    End Sub

    ''' <summary>Czyści listę wybranych warstw.</summary>
    Private Sub WyczyscWarstwy()
        _zestaw.Wyczysc()
        PokazWarstwy()
        UniewaznijWynikPobierania()
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
                MsgBox("Nie można dodać warstwy - wybrano już " & ZestawWarstw.MaksLiczba & " warstw (więcej program nie łączy). Usuń którąś z listy wybranych warstw.", MsgBoxStyle.Exclamation, "Wybrane warstwy")
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
        DobierzBokSegmentu(bylaPusta)
        Dim infoWstawienia As String = If(indeksNowej < _zestaw.Liczba - 1, " (umieszczono pod nakładką)", "")
        Dim ostrzezenieFormat As String = If(_zestaw.CzySameNakladki() AndAlso Not Wms.CzyFormatZPrzezroczystoscia(Ustawienia.Format),
                                             " Uwaga: wybrano same nakładki, a format pobierania to ." & Ustawienia.Format & " (bez przezroczystości). " &
                                             "Zmień go w oknie Ustawienia - kliknij format na pasku stanu u dołu okna.", "")
        Komunikat("Wskazano warstwę: " & w.Nazwa & infoWstawienia & "." & If(opisPiksela <> "", " " & opisPiksela, "") & ostrzezenieFormat, Color.Green)
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
        UniewaznijWynikPobierania()
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
        DobierzBokSegmentu(_zestaw.Liczba = 0)
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
        'okno korzysta z bufora podglądu panelu - jeśli parametry się nie zmieniły, nie przerywamy trwającego pobierania
        _timerPodgladu.Stop()
        Dim punkt As PunktXY, piksel As Double
        ParametryProbki(punkt, piksel)
        Dim zmieniono = _silnikPodgladu.SprawdzIZaktualizujParametry(punkt, piksel, Ustawienia.Uklad)
        If zmieniono Then _ctsPodglad?.Cancel()
        Dim aktywnePobieranie = If(zmieniono, Nothing, _zadaniePodgladuPanelu)
        Using okno As New FormWarstwyZaawansowane(_zestaw.Kopia(), ZadanieDoPodgladu(), lstWybraneWarstwy.Zaznaczona, _silnikPodgladu,
                                                  czekajNaPobieranie:=aktywnePobieranie)
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

#Region "Bieżący podgląd zestawu warstw"

    Private Sub btnPodgladZaawansowane_Click(sender As Object, e As EventArgs) Handles btnPodgladZaawansowane.Click, picPodglad.Click
        OtworzWarstwyZaawansowane()
    End Sub

    Private Sub btnPodgladOdswiez_Click(sender As Object, e As EventArgs) Handles btnPodgladOdswiez.Click
        WymusOdswiezeniePodgladu()
    End Sub

    Private Sub btnZoomPlus_Click(sender As Object, e As EventArgs) Handles btnZoomPlus.Click
        ZmienZoomPodgladu(1)
    End Sub

    Private Sub btnZoomMinus_Click(sender As Object, e As EventArgs) Handles btnZoomMinus.Click
        ZmienZoomPodgladu(-1)
    End Sub

    Private Sub btnZoomReset_Click(sender As Object, e As EventArgs) Handles btnZoomReset.Click
        _zoomPodgladu = Nothing
        ZastosujZoomPodgladu()
    End Sub

    Private Sub picPodglad_MouseEnter(sender As Object, e As EventArgs) Handles picPodglad.MouseEnter
        If _zestaw.Liczba > 0 Then picPodglad.Focus()
    End Sub

    Private Sub picPodglad_MouseWheel(sender As Object, e As MouseEventArgs) Handles picPodglad.MouseWheel
        If _zestaw.Liczba = 0 OrElse _bazaPodgladu Is Nothing Then Exit Sub
        ZmienZoomPodgladu(Math.Sign(e.Delta))
    End Sub

    Private Sub ZmienZoomPodgladu(kierunek As Integer)
        If _bazaPodgladu Is Nothing OrElse _przerwanie IsNot Nothing OrElse kierunek = 0 Then Return
        Dim teraz = PowiekszeniePodgladu()
        Dim nowy = If(kierunek > 0, _poziomyZoomu.FirstOrDefault(Function(z) z > teraz + 0.005),
                                   _poziomyZoomu.LastOrDefault(Function(z) z < teraz - 0.005))
        If nowy = 0 Then Return
        _zoomPodgladu = nowy
        ZastosujZoomPodgladu()
    End Sub

    Private Function PowiekszeniePodgladu() As Double
        If _zoomPodgladu.HasValue Then Return _zoomPodgladu.Value
        If _bazaPodgladu Is Nothing Then Return 1.0
        Dim obszar = picPodglad.ClientSize
        Dim dopasowane = Math.Min(obszar.Width / CDbl(_bazaPodgladu.Width), obszar.Height / CDbl(_bazaPodgladu.Height))
        Return Math.Clamp(dopasowane, 0.01, 1.0)
    End Function

    ''' <summary>
    ''' Bez poprawnej siatki podgląd podąża za mapą: po przesunięciu lub przybliżeniu mapy próbka pochodzi ze środka
    ''' nowego widoku. Wcześniej punkt próbki zmieniał się tylko przy zmianie listy warstw, więc dodanie warstwy po
    ''' przesunięciu mapy niespodziewanie przenosiło podgląd i pobierało od nowa wszystkie warstwy.
    ''' </summary>
    Private Sub mapa_OnPositionChanged(point As PointLatLng) Handles mapa.OnPositionChanged
        'środek mapy na pasku stanu także po przesunięciu z wyszukiwarki (dawniej odświeżany tylko ruchem myszy)
        If _zaladowany Then
            lblSrodekSzer.Text = Liczba(Math.Round(point.Lat, 4))
            lblSrodekDlug.Text = Liczba(Math.Round(point.Lng, 4))
        End If
        If Not _zaladowany OrElse _zestaw.Liczba = 0 OrElse _przerwanie IsNot Nothing Then Exit Sub
        If BiezacaSiatka().Poprawna Then Exit Sub
        ZglosOdswiezeniePodgladu()
    End Sub

    ''' <summary>
    ''' Skaluje w pamięci bazową bitmapę próbki do bieżącego powiększenia i wyświetla w picPodglad. Przy 100% panel
    ''' pokazuje środek próbki piksel w piksel, przy 50% większy obszar próbki, przy 200% lupę rastrową.
    ''' </summary>
    Private Sub ZastosujZoomPodgladu()
        If _bazaPodgladu Is Nothing Then
            Dim stary = picPodglad.Image
            picPodglad.Image = Nothing
            stary?.Dispose()
            AktualizujStanPrzyciskowZoomu()
            AktualizujEtykieteSkali()
            Exit Sub
        End If
        Dim obszar = picPodglad.ClientSize
        If obszar.Width < 2 OrElse obszar.Height < 2 Then Return
        Dim powiekszony = PodgladWarstwSilnik.ZastosujZoom(_bazaPodgladu, PowiekszeniePodgladu(), obszar.Width, obszar.Height)
        If _brakWarstwPodgladu Then PodgladWarstwSilnik.RysujRamkeBraku(powiekszony)
        Dim staryImg = picPodglad.Image
        picPodglad.Image = powiekszony
        staryImg?.Dispose()
        AktualizujEtykieteSkali()
        AktualizujStanPrzyciskowZoomu()
    End Sub

    ''' <summary>
    ''' Piksel podglądu bez poprawnej siatki: wpisany w polu (w jednostkach układu) albo zalecany piksel najdokładniejszej
    ''' warstwy. Zalecany piksel jest w metrach - w układzie WGS84 przeliczany na stopnie; wcześniej 0,25 m traktowano
    ''' jako 0,25°, więc podgląd obejmował obszar ok. 29° x 29° i był pusty.
    ''' </summary>
    Friend Function PikselPodgladuBezSiatki() As Double
        Dim wpisany = Wartosc(txtRozmiarPiksela.Text)
        If wpisany > 0 Then Return wpisany
        Dim zalecany = If(_zestaw.NajdokladniejszaWarstwa() IsNot Nothing, _zestaw.NajdokladniejszaWarstwa().ZalecanyPiksel, 0)
        If zalecany <= 0 Then zalecany = 1.0
        If Not Ustawienia.Uklad.Geograficzny Then Return zalecany
        Return UkladWspolrzednych.PL1992.PrzeliczRozmiarPiksela(Ustawienia.Uklad, zalecany, New PunktGeo(mapa.Position.Lat, mapa.Position.Lng))
    End Function

    ''' <summary>
    ''' Punkt i rozmiar piksela próbki. Z poprawną siatką: środek siatki, a przy obszarze wzdłuż trasy środek
    ''' pobieranego segmentu najbliższego środkowi siatki (środek prostokąta może leżeć daleko od trasy i podgląd
    ''' pokazywałby teren, którego program nie pobierze). Bez siatki: środek bieżącego widoku mapy.
    ''' </summary>
    Private Sub ParametryProbki(ByRef punkt As PunktXY, ByRef piksel As Double)
        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            piksel = If(s.RozmiarPiksela > 0, s.RozmiarPiksela, 1.0)
            punkt = s.ZasiegSiatki.Srodek
            If _korytarz IsNot Nothing Then
                Dim klucz As String = String.Join("|", Liczba(s.Obszar.XDol), Liczba(s.Obszar.YLewy), Liczba(s.Obszar.XGora),
                                                  Liczba(s.Obszar.YPrawy), Liczba(s.RozmiarPiksela), s.BokSegmentuPx,
                                                  Ustawienia.Uklad.Epsg, _korytarz.GetHashCode(), Liczba(_korytarz.SzerokoscMetry))
                'wyznaczanie segmentów korytarza trwa przy długiej trasie - pola zasięgu zmieniają się po kolei, więc wynik zapamiętywany
                If klucz <> _kluczPunktuKorytarza Then
                    _kluczPunktuKorytarza = klucz
                    _punktKorytarza = _korytarz.PunktProbki(s, Ustawienia.Uklad, MaksSegmentowKorytarzaNaBiezaco)
                End If
                If _punktKorytarza.HasValue Then punkt = _punktKorytarza.Value
            End If
            _punktProbkiBezSiatki = Nothing
        Else
            _punktProbkiBezSiatki = Ustawienia.Uklad.ZWgs84(mapa.Position.Lat, mapa.Position.Lng)
            punkt = _punktProbkiBezSiatki.Value
            piksel = PikselPodgladuBezSiatki()
        End If
    End Sub

    ''' <summary>
    ''' Oddziela powiększenie wyświetlanej próbki od rozdzielczości, z jaką pobierane są segmenty.
    ''' </summary>
    Private Sub AktualizujEtykieteSkali()
        If _zestaw.Liczba = 0 OrElse _bazaPodgladu Is Nothing Then
            lblPodgladSkala.Text = "-"
            ToolTip1.SetToolTip(lblPodgladSkala, "")
            Exit Sub
        End If
        Dim s = BiezacaSiatka()
        Dim bazowyPiksel As Double = If(s.Poprawna, If(s.RozmiarPiksela > 0, s.RozmiarPiksela, 1.0), PikselPodgladuBezSiatki())
        Dim zoom = PowiekszeniePodgladu()
        lblPodgladSkala.Text = $"Podgląd {CInt(Math.Round(zoom * 100))}%"
        ToolTip1.SetToolTip(lblPodgladSkala, "Powiększenie próbki na ekranie. Rozdzielczość pobierania: " &
                            Liczba(bazowyPiksel) & If(Ustawienia.Uklad.Geograficzny, " °/px", " m/px") & ".")
    End Sub

    ''' <summary>Aktualizuje dostępność przycisków powiększenia podglądu.</summary>
    Private Sub AktualizujStanPrzyciskowZoomu()
        Dim pobieranie As Boolean = _przerwanie IsNot Nothing
        Dim maObraz As Boolean = _zestaw.Liczba > 0 AndAlso _bazaPodgladu IsNot Nothing
        Dim zoom = PowiekszeniePodgladu()
        btnZoomMinus.Enabled = maObraz AndAlso zoom > _poziomyZoomu(0) + 0.005 AndAlso Not pobieranie
        btnZoomPlus.Enabled = maObraz AndAlso zoom < _poziomyZoomu(_poziomyZoomu.Length - 1) - 0.005 AndAlso Not pobieranie
        btnZoomReset.Enabled = maObraz AndAlso _zoomPodgladu.HasValue AndAlso Not pobieranie
    End Sub

    ''' <summary>Wymusza wyczyszczenie pamięci podręcznej i ponowne pobranie podglądu z serwerów.</summary>
    Private Sub WymusOdswiezeniePodgladu()
        If _przerwanie IsNot Nothing Then Exit Sub
        _silnikPodgladu.WyczyscBufor()
        _punktProbkiBezSiatki = Nothing
        Dim staraBaza = _bazaPodgladu
        _bazaPodgladu = Nothing
        staraBaza?.Dispose()
        ZglosOdswiezeniePodgladu(natychmiast:=True)
    End Sub

    ''' <summary>Wyświetla złożony obraz próbki i stan podglądu (brakujące warstwy: czerwona ramka i ich lista).</summary>
    Private Sub PokazObrazPodgladu(bmp As Bitmap, bledy As IList(Of String))
        If bmp IsNot Nothing Then
            Dim staraBaza = _bazaPodgladu
            _bazaPodgladu = bmp
            staraBaza?.Dispose()
        End If
        Dim brakujace = _silnikPodgladu.BrakujaceWarstwy(_zestaw)
        _brakWarstwPodgladu = brakujace.Count > 0
        ZastosujZoomPodgladu()
        If brakujace.Count = 0 Then
            lblPodgladStan.Text = ""
            ToolTip1.SetToolTip(lblPodgladStan, "")
        Else
            lblPodgladStan.Text = If(brakujace.Count = _zestaw.Liczba, "Błąd pobierania", "Brak " & brakujace.Count & " z " & _zestaw.Liczba & " warstw")
            Dim opis As String = "Podgląd bez warstw: " & String.Join(", ", brakujace.Select(Function(w) NazwaWarstwy(w))) & "."
            If bledy IsNot Nothing AndAlso bledy.Count > 0 Then opis &= vbCrLf & String.Join(vbCrLf, bledy)
            ToolTip1.SetToolTip(lblPodgladStan, opis)
        End If
        ToolTip1.SetToolTip(picPodglad, "Bieżący podgląd zestawu warstw (" & OpisWarstw() & ")." &
                            If(_brakWarstwPodgladu, " Czerwona ramka: części warstw nie udało się pobrać.", "") &
                            " Kółko myszy: przybliż/oddal. Kliknij, aby otworzyć okno Zaawansowane.")
    End Sub

    ''' <summary>
    ''' Zgłasza potrzebę odświeżenia podglądu.
    ''' Gdy natychmiast=False, uruchamia timer opóźniający (debouncing 350 ms).
    ''' </summary>
    Private Sub ZglosOdswiezeniePodgladu(Optional natychmiast As Boolean = False)
        Dim pobieranie As Boolean = _przerwanie IsNot Nothing
        btnPodgladZaawansowane.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        btnPodgladOdswiez.Enabled = _zestaw.Liczba > 0 AndAlso Not pobieranie
        AktualizujStanPrzyciskowZoomu()

        If _zestaw.Liczba = 0 Then
            _timerPodgladu.Stop()
            _ctsPodglad?.Cancel()
            _punktProbkiBezSiatki = Nothing
            _brakWarstwPodgladu = False
            Dim staraBaza = _bazaPodgladu
            _bazaPodgladu = Nothing
            staraBaza?.Dispose()
            Dim stary = picPodglad.Image
            picPodglad.Image = Nothing
            stary?.Dispose()
            lblPodgladLiczbaWarstw.Text = "Warstw: 0"
            lblPodgladSkala.Text = "-"
            lblPodgladStan.Text = "Brak warstw"
            ToolTip1.SetToolTip(picPodglad, "Brak wybranych warstw do pobrania.")
            ToolTip1.SetToolTip(lblPodgladStan, "")
            Exit Sub
        End If

        lblPodgladLiczbaWarstw.Text = "Warstw: " & _zestaw.Liczba

        If pobieranie Then
            lblPodgladStan.Text = "Pobieranie w toku"
            Exit Sub
        End If

        Dim punkt As PunktXY, bazowyPiksel As Double
        ParametryProbki(punkt, bazowyPiksel)
        AktualizujEtykieteSkali()

        Dim zmieniono = _silnikPodgladu.SprawdzIZaktualizujParametry(punkt, bazowyPiksel, Ustawienia.Uklad)

        ' Jeśli parametry próbki się nie zmieniły i wszystkie warstwy są już w buforze, składamy obraz natychmiast w RAM
        If Not zmieniono AndAlso _silnikPodgladu.BrakujaceWarstwy(_zestaw).Count = 0 Then
            _timerPodgladu.Stop()
            PokazObrazPodgladu(_silnikPodgladu.ZlozObraz(_zestaw), Nothing)
            Exit Sub
        End If

        _timerPodgladu.Stop()
        If natychmiast Then
            UruchomPobieraniePodgladuAsync()
        Else
            _timerPodgladu.Start()
        End If
    End Sub

    Private Sub TimerPodgladu_Tick(sender As Object, e As EventArgs)
        _timerPodgladu.Stop()
        UruchomPobieraniePodgladuAsync()
    End Sub

    ''' <summary>Pobiera asynchronicznie brakujące warstwy podglądu z serwerów WMS/WMTS.</summary>
    Private Async Sub UruchomPobieraniePodgladuAsync()
        If _zestaw.Liczba = 0 OrElse _przerwanie IsNot Nothing Then Exit Sub

        Dim punkt As PunktXY, bazowyPiksel As Double
        ParametryProbki(punkt, bazowyPiksel)
        _silnikPodgladu.SprawdzIZaktualizujParametry(punkt, bazowyPiksel, Ustawienia.Uklad)

        _ctsPodglad?.Cancel()
        Dim cts As New CancellationTokenSource()
        _ctsPodglad = cts

        lblPodgladStan.Text = "Pobieranie…"
        Dim postep As New Progress(Of String)(Sub(kom)
                                                  If IsDisposed OrElse Disposing Then Exit Sub
                                                  If Not cts.IsCancellationRequested Then lblPodgladStan.Text = kom
                                              End Sub)

        Try
            Dim zadanie = ZadanieDoPodgladu()
            Dim t = _silnikPodgladu.PobierzPodgladAsync(zadanie, _zestaw, cts.Token, postep)
            _zadaniePodgladuPanelu = t
            Dim wynik = Await t
            If cts.IsCancellationRequested OrElse IsDisposed OrElse Disposing Then
                wynik.Item1?.Dispose()
                Exit Sub
            End If
            PokazObrazPodgladu(wynik.Item1, wynik.Item2)
        Catch ex As OperationCanceledException
        Catch ex As Exception
            If IsDisposed OrElse Disposing Then Exit Sub
            lblPodgladStan.Text = "Błąd podglądu"
            ToolTip1.SetToolTip(lblPodgladStan, OpisBledu.Z(ex))
        Finally
            If _ctsPodglad Is cts Then
                _ctsPodglad = Nothing
                _zadaniePodgladuPanelu = Nothing
            End If
            cts.Dispose()
        End Try
    End Sub

#End Region

#Region "Siatka segmentów"

    ''' <summary>Siatka segmentów wg pól formularza.</summary>
    Friend Function BiezacaSiatka() As Siatka
        Dim obszar = ZasiegPolXY()
        Return New Siatka(obszar, Wartosc(txtRozmiarPiksela.Text), WartoscCalkowita(txtBokSegmentu.Text))
    End Function

    ''' <summary>Zasięg z pól współrzędnych X/Y okna głównego (niezależnie od rozmiaru piksela).</summary>
    Friend Function ZasiegPolXY() As Zasieg
        Return New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text))
    End Function

    ''' <summary>
    ''' Przelicza wynikowy rozmiar siatki segmentów (dawniej te same obliczenia powtarzało sześć procedur,
    ''' z których część korzystała z nieaktualnych jeszcze wartości).
    ''' </summary>
    Friend Sub PrzeliczSiatke()
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

        '--- Aktualizacja nowoczesnego kokpitu (Wariant 2C-2) ---
        If lblWymiaryTerenu Is Nothing Then Exit Sub
        If s.Poprawna Then
            Dim szerKm As Double = If(Double.IsNaN(kmDl), 0, (s.Obszar.YPrawy - s.Obszar.YLewy) / kmDl)
            Dim wysKm As Double = If(Double.IsNaN(kmSz), 0, (s.Obszar.XGora - s.Obszar.XDol) / kmSz)
            szerKm = Math.Abs(szerKm)
            wysKm = Math.Abs(wysKm)
            Dim powKm2 As Double = szerKm * wysKm
            Dim liczbaSegm As Long = CLng(s.LiczbaKolumn) * CLng(s.LiczbaWierszy)

            If lblBadgeSegmenty IsNot Nothing Then
                lblBadgeSegmenty.ForeColor = Color.FromArgb(15, 23, 42)
                If _korytarz IsNot Nothing Then
                    lblBadgeSegmenty.Text = $"Siatka: {liczbaSegm} {Odmiana(liczbaSegm, "segment", "segmenty", "segmentów")} (wzdłuż trasy)"
                Else
                    lblBadgeSegmenty.Text = $"Siatka: {liczbaSegm} {Odmiana(liczbaSegm, "segment", "segmenty", "segmentów")} ({s.LiczbaKolumn} × {s.LiczbaWierszy})"
                End If
            End If

            lblWymiaryTerenu.ForeColor = Color.FromArgb(51, 65, 85)
            lblWymiaryTerenu.Text = $"{Liczba(Math.Round(szerKm, 1))} × {Liczba(Math.Round(wysKm, 1))} km"
            If lblPowierzchniaTerenu IsNot Nothing Then
                lblPowierzchniaTerenu.Text = $"({Liczba(Math.Round(powKm2, 1))} km²)"
                lblPowierzchniaTerenu.Left = lblWymiaryTerenu.Right + 4
                lblPowierzchniaTerenu.Top = lblWymiaryTerenu.Top
                lblPowierzchniaTerenu.Visible = True
            End If

            ' Szacunek rozmiaru i czasu pobierania
            Dim formatEksportu As RodzajEksportu = WybranyRodzajEksportuKokpitu()
            Dim liczbaZrodel As Integer = If(_zestaw IsNot Nothing, ZarzadcaSzablonow.LiczbaZrodel(_zestaw), 1)
            Dim szacunek = ZarzadcaSzablonow.SzacujParametry(s.Obszar, s.RozmiarPiksela, s.BokSegmentuPx,
                                                            _korytarz, Ustawienia.Uklad, formatEksportu,
                                                            False, liczbaZrodel)
            lblSzacowanyRozmiar.Text = szacunek.SformatowanyPrzedzialRozmiaru()
            lblSzacowanyCzas.Text = szacunek.SformatowanyCzas()

            ' Skrót układu i współrzędnych
            If geograficzny Then
                lblUkladZasiegSkrot.Text = $"{Ustawienia.Uklad.Nazwa} ({Liczba(Math.Round(s.Obszar.XDol, 2))}°N, {Liczba(Math.Round(s.Obszar.YLewy, 2))}°E)"
            Else
                Dim xK = Math.Round(s.Obszar.XDol / 1000.0)
                Dim yK = Math.Round(s.Obszar.YLewy / 1000.0)
                lblUkladZasiegSkrot.Text = $"{Ustawienia.Uklad.Nazwa} (N: {xK:0}k, E: {yK:0}k)"
            End If
        Else
            If lblBadgeSegmenty IsNot Nothing Then
                lblBadgeSegmenty.ForeColor = Color.FromArgb(100, 116, 139)
                lblBadgeSegmenty.Text = "Brak zaznaczenia"
            End If
            lblWymiaryTerenu.ForeColor = Color.DimGray
            lblWymiaryTerenu.Text = "Zaznacz prawym przyciskiem myszy"
            If lblPowierzchniaTerenu IsNot Nothing Then
                lblPowierzchniaTerenu.Text = ""
                lblPowierzchniaTerenu.Visible = False
            End If
            lblSzacowanyRozmiar.Text = "-"
            lblSzacowanyCzas.Text = "-"
            lblUkladZasiegSkrot.Text = $"{Ustawienia.Uklad.Nazwa} (zaznacz obszar na mapie)"
        End If

        ' Bok segmentu w terenie
        If bok > 0 AndAlso Not Double.IsNaN(kmSz) Then
            Dim bokKmWartosc As Double = If(geograficzny, bok / kmSz, bok / 1000.0)
            lblBokKm.Text = $"= {Liczba(Math.Round(bokKmWartosc, 2))} km w terenie"
        Else
            lblBokKm.Text = ""
        End If

        AktualizujPresetPiksela(Wartosc(txtRozmiarPiksela.Text))
        OdswiezPodsumowaniePracy()
        AktualizujStanBelki()
    End Sub

    Private Function PobierzZapisanaKonfiguracje(plikConf As String) As KonfiguracjaSesji
        If String.IsNullOrEmpty(plikConf) OrElse Not File.Exists(plikConf) Then Return Nothing
        Try
            Dim czas = File.GetLastWriteTimeUtc(plikConf)
            If String.Equals(_cacheConfSciezka, plikConf, StringComparison.OrdinalIgnoreCase) AndAlso
               _cacheConfCzasUtc = czas AndAlso _cacheConfObiekt IsNot Nothing Then
                Return _cacheConfObiekt
            End If
            Dim c = KonfiguracjaSesji.Wczytaj(plikConf, KodowanieSystemowe())
            _cacheConfSciezka = plikConf
            _cacheConfCzasUtc = czas
            _cacheConfObiekt = c
            Return c
        Catch
            Return Nothing
        End Try
    End Function

    Private Sub WpiszPolaZasiegu(xDol As String, yLewy As String, xGora As String, yPrawy As String)
        _wstrzymajPolaZasiegu = True
        Try
            txtXDol.Text = xDol
            txtYLewy.Text = yLewy
            txtXGora.Text = xGora
            txtYPrawy.Text = yPrawy
        Finally
            _wstrzymajPolaZasiegu = False
        End Try
        PolaZasieguZmienione(Me, EventArgs.Empty)
    End Sub

    Private Sub PolaZasieguZmienione(sender As Object, e As EventArgs) Handles txtXDol.TextChanged, txtYLewy.TextChanged, txtXGora.TextChanged, txtYPrawy.TextChanged
        If _wstrzymajPolaZasiegu Then Exit Sub
        btnZnaczniki.Enabled = True
        UniewaznijWynikPobierania()
        PrzeliczSiatke()
        If _zaladowany Then ZglosOdswiezeniePodgladu()
    End Sub

    Private Sub ParametrySegmentuZmienione(sender As Object, e As EventArgs) Handles txtBokSegmentu.TextChanged, txtRozmiarPiksela.TextChanged
        btnZnaczniki.Enabled = True
        UniewaznijWynikPobierania()
        PrzeliczSiatke()
        If sender Is txtBokSegmentu Then
            Dim biezacy As Integer = WartoscCalkowita(txtBokSegmentu.Text)
            If _biezacyLimitSegmentu > 0 Then
                If biezacy > _biezacyLimitSegmentu Then
                    PokazOstrzezenieSegmentu(_biezacyLimitSegmentu)
                Else
                    UkryjOstrzezenieSegmentu()
                End If
            End If
        End If
        If _siatkaWidoczna OrElse mapa.SelectedArea.IsEmpty = False OrElse _korytarz IsNot Nothing Then
            Dim s = BiezacaSiatka()
            If s.Poprawna Then
                WyczyscZnaczniki()
                PokazZnaczniki(centruj:=False)
            End If
        End If
        If _zaladowany Then ZglosOdswiezeniePodgladu()
    End Sub

    Private Sub txtBokSegmentu_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtBokSegmentu.KeyPress
        If Not Char.IsControl(e.KeyChar) Then
            _bokSegmentuZmienionyRecznie = True
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
            WpiszPolaZasiegu(Liczba(z.XDol), Liczba(z.YLewy), Liczba(z.XGora), Liczba(z.YPrawy))
            If piksel > 0 Then txtRozmiarPiksela.Text = Liczba(stary.PrzeliczRozmiarPiksela(nowy, piksel, srodek))
        Else
            _punktProbkiBezSiatki = Nothing
            If _zaladowany Then ZglosOdswiezeniePodgladu()
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
            If w.MaksRozmiarSegmentu <= 0 Then
                If plik.MaksRozmiarSegmentu > 0 Then
                    w.MaksRozmiarSegmentu = plik.MaksRozmiarSegmentu
                Else
                    Dim wm = plik.Znajdz(w.Nazwa)
                    If wm IsNot Nothing AndAlso wm.MaksRozmiarSegmentu > 0 Then
                        w.MaksRozmiarSegmentu = wm.MaksRozmiarSegmentu
                    ElseIf (w.Adres IsNot Nothing AndAlso w.Adres.IndexOf("integracja.gugik.gov.pl/cgi-bin/KIEG", StringComparison.OrdinalIgnoreCase) >= 0) OrElse
                           (w.Zbior IsNot Nothing AndAlso w.Zbior.IndexOf("ewidencja_gruntow", StringComparison.OrdinalIgnoreCase) >= 0) Then
                        w.MaksRozmiarSegmentu = 1500
                    ElseIf w.Zbior IsNot Nothing AndAlso w.Zbior.IndexOf("ortofotomapa", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso Not w.CzyKafelkowa Then
                        w.MaksRozmiarSegmentu = 4000
                    End If
                End If
            End If
        Next
    End Sub

    ''' <summary>Dobiera bezpieczny bok segmentu w pikselach wg limitów serwerów wybranych warstw.</summary>
    Private Sub DobierzBokSegmentu(bylaPusta As Boolean)
        UzupelnijZalecanePiksele()
        Dim bezpieczny = _zestaw.BezpiecznyBokSegmentu(2000)
        _biezacyLimitSegmentu = If(bezpieczny < 2000, bezpieczny, 0)
        Dim biezacy As Integer = WartoscCalkowita(txtBokSegmentu.Text)
        If bylaPusta OrElse Not _bokSegmentuZmienionyRecznie Then
            If bezpieczny < 2000 Then
                txtBokSegmentu.Text = bezpieczny.ToString(System.Globalization.CultureInfo.InvariantCulture)
                _automatycznyBokSegmentu = bezpieczny
            ElseIf _automatycznyBokSegmentu < 2000 Then
                txtBokSegmentu.Text = "2000"
                _automatycznyBokSegmentu = 2000
            End If
            UkryjOstrzezenieSegmentu()
        Else
            If biezacy > bezpieczny AndAlso bezpieczny < 2000 Then
                PokazOstrzezenieSegmentu(bezpieczny)
            Else
                UkryjOstrzezenieSegmentu()
            End If
        End If
    End Sub

    Private Sub InicjalizujOstrzezenieSegmentu()
        _lnkOstrzezenieSegmentu = New LinkLabel With {
            .Name = "lnkOstrzezenieSegmentu",
            .Location = New Point(txtBokSegmentu.Left, txtBokSegmentu.Bottom + 1),
            .Size = New Size(160, 14),
            .Font = New Font(Font.FontFamily, 7.5!, FontStyle.Regular),
            .LinkColor = Color.FromArgb(180, 90, 0),
            .ActiveLinkColor = Color.FromArgb(200, 60, 0),
            .Visible = False,
            .AutoEllipsis = True
        }
        AddHandler _lnkOstrzezenieSegmentu.LinkClicked, AddressOf lnkOstrzezenieSegmentu_LinkClicked
        grpRozdzielczoscFormat.Controls.Add(_lnkOstrzezenieSegmentu)
        _lnkOstrzezenieSegmentu.BringToFront()
    End Sub

    Private Sub lnkOstrzezenieSegmentu_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If _biezacyLimitSegmentu > 0 Then
            txtBokSegmentu.Text = _biezacyLimitSegmentu.ToString(System.Globalization.CultureInfo.InvariantCulture)
            _automatycznyBokSegmentu = _biezacyLimitSegmentu
            _bokSegmentuZmienionyRecznie = False
            UkryjOstrzezenieSegmentu()
        End If
    End Sub

    Private Sub PokazOstrzezenieSegmentu(limit As Integer)
        _biezacyLimitSegmentu = limit
        If _lnkOstrzezenieSegmentu Is Nothing Then Exit Sub
        _lnkOstrzezenieSegmentu.Text = $"Zalecany maks. {limit} px [Zastosuj]"
        ToolTip1.SetToolTip(_lnkOstrzezenieSegmentu, $"Wybrana warstwa zaleca pobieranie segmentów o boku do {limit} px. Kliknij, aby zastosować.")
        _lnkOstrzezenieSegmentu.Visible = True
    End Sub

    Private Sub UkryjOstrzezenieSegmentu()
        If _lnkOstrzezenieSegmentu IsNot Nothing Then
            _lnkOstrzezenieSegmentu.Visible = False
        End If
    End Sub

#Region "Kokpit dolnej belki (Wariant 2C-2)"

    Private _synchronizacjaPiksela As Boolean = False

    ''' <summary>Inicjalizacja list wyboru (presety piksela, format docelowy) w nowoczesnym kokpicie.</summary>
    Private Sub InicjalizujKokpit()
        cmbRozmiarPiksela.Items.Clear()
        cmbRozmiarPiksela.Items.Add("0.1 m/px (bardzo wysoka, ~1:350)")
        cmbRozmiarPiksela.Items.Add("0.25 m/px (ortofoto wysoka, ~1:880)")
        cmbRozmiarPiksela.Items.Add("0.5 m/px (ortofoto standard, ~1:1 750)")
        cmbRozmiarPiksela.Items.Add("1.0 m/px (domyślny, topo ~1:3 500)")
        cmbRozmiarPiksela.Items.Add("2.0 m/px (ogólna, ~1:7 000)")
        cmbRozmiarPiksela.Items.Add("5.0 m/px (przeglądowa, ~1:17 500)")
        cmbRozmiarPiksela.Items.Add("10.0 m/px (bardzo niska, ~1:35 000)")
        cmbRozmiarPiksela.Items.Add("Własna wartość...")

        cmbFormatDocelowy.Items.Clear()
        cmbFormatDocelowy.Items.Add("GeoTIFF (arkusz scalony)")
        cmbFormatDocelowy.Items.Add("MBTiles (mobilne i GIS)")
        cmbFormatDocelowy.Items.Add("KMZ (Google Earth / Garmin)")
        cmbFormatDocelowy.Items.Add("JPEG + World file (.jgw)")
        cmbFormatDocelowy.Items.Add("PNG + World file (.pgw)")
        cmbFormatDocelowy.Items.Add("OruxMaps (baza SQLite)")
        cmbFormatDocelowy.Items.Add("CAD (DXF + SCR/LISP)")
        cmbFormatDocelowy.SelectedIndex = 0
    End Sub

    ''' <summary>Synchronizuje listę typowych rozdzielczości z wartością w polu tekstowym piksela.</summary>
    Private Sub AktualizujPresetPiksela(piksel As Double)
        If cmbRozmiarPiksela Is Nothing OrElse _synchronizacjaPiksela OrElse cmbRozmiarPiksela.Items.Count < 8 Then Exit Sub
        ' W trakcie wpisywania np. 1,5 wartość 1 po pierwszym znaku nie może zamykać pola edycji.
        If txtRozmiarPiksela.Focused AndAlso cmbRozmiarPiksela.SelectedIndex = 7 Then Return
        _synchronizacjaPiksela = True
        Try
            If Math.Abs(piksel - 0.1) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 0
            ElseIf Math.Abs(piksel - 0.25) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 1
            ElseIf Math.Abs(piksel - 0.5) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 2
            ElseIf Math.Abs(piksel - 1.0) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 3
            ElseIf Math.Abs(piksel - 2.0) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 4
            ElseIf Math.Abs(piksel - 5.0) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 5
            ElseIf Math.Abs(piksel - 10.0) < 0.001 Then
                cmbRozmiarPiksela.SelectedIndex = 6
            Else
                cmbRozmiarPiksela.SelectedIndex = 7 ' Własna wartość...
            End If
        Finally
            _synchronizacjaPiksela = False
        End Try
    End Sub

    Private Sub cmbRozmiarPiksela_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbRozmiarPiksela.SelectedIndexChanged
        If _synchronizacjaPiksela OrElse cmbRozmiarPiksela.SelectedIndex < 0 Then Exit Sub
        _synchronizacjaPiksela = True
        Try
            Select Case cmbRozmiarPiksela.SelectedIndex
                Case 0 : txtRozmiarPiksela.Text = "0.1"
                Case 1 : txtRozmiarPiksela.Text = "0.25"
                Case 2 : txtRozmiarPiksela.Text = "0.5"
                Case 3 : txtRozmiarPiksela.Text = "1"
                Case 4 : txtRozmiarPiksela.Text = "2"
                Case 5 : txtRozmiarPiksela.Text = "5"
                Case 6 : txtRozmiarPiksela.Text = "10"
                Case 7
                    OdswiezEdytorRozdzielczosci()
                    txtRozmiarPiksela.Focus()
                    txtRozmiarPiksela.SelectAll()
            End Select
        Finally
            _synchronizacjaPiksela = False
        End Try
    End Sub

    Private Sub cmbFormatDocelowy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFormatDocelowy.SelectedIndexChanged
        PrzeliczSiatke()
    End Sub

    Private Function WybranyRodzajEksportuKokpitu() As RodzajEksportu
        If cmbFormatDocelowy Is Nothing Then Return RodzajEksportu.GeoTiff
        Select Case cmbFormatDocelowy.SelectedIndex
            Case 0 ' GeoTIFF
                Return RodzajEksportu.GeoTiff
            Case 1 ' MBTiles
                Return RodzajEksportu.MBTiles
            Case 2 ' KMZ
                Return RodzajEksportu.KmzGarmin
            Case 3 ' JPEG
                Return RodzajEksportu.GeoTiff
            Case 4 ' PNG
                Return RodzajEksportu.GeoTiff
            Case 5 ' OruxMaps
                Return RodzajEksportu.OruxMaps
            Case 6 ' CAD
                Return RodzajEksportu.GeoTiff
            Case Else
                Return RodzajEksportu.GeoTiff
        End Select
    End Function

    ''' <summary>Otwiera okno ręcznego wpisywania i zaokrąglania współrzędnych obszaru pobierania.</summary>
    Private Sub btnWpiszWspolrzedne_Click(sender As Object, e As EventArgs) Handles btnWpiszWspolrzedne.Click
        If PobieranieTrwa("Wpisywanie współrzędnych") Then Return
        ZakonczWyborNaroznikow()
        Using okno As New FormZasiegXY()
            okno.NazwaUkladu = Ustawienia.Uklad.Nazwa
            okno.CzyGeograficzny = Ustawienia.Uklad.Geograficzny
            okno.XDol = txtXDol.Text
            okno.XGora = txtXGora.Text
            okno.YLewy = txtYLewy.Text
            okno.YPrawy = txtYPrawy.Text
            If okno.ShowDialog(Me) = DialogResult.OK Then
                _korytarz = Nothing
                UsunKorytarzToolStripMenuItem.Enabled = False
                WpiszPolaZasiegu(okno.XDol, okno.YLewy, okno.XGora, okno.YPrawy)
                WyczyscZnaczniki()
                PokazZnaczniki(centruj:=True)
            End If
        End Using
    End Sub

    Private Sub btnKreatorDolny_Click(sender As Object, e As EventArgs) Handles btnKreatorDolny.Click
        OtworzKreatorMapy()
    End Sub

    ''' <summary>Otwiera okno eksportu lub scalania z formatem wybranym w kokpicie.</summary>
    Private Sub btnPrzejdzDoEksportu_Click(sender As Object, e As EventArgs) Handles btnPrzejdzDoEksportu.Click
        If PobieranieTrwa("Eksport") Then Exit Sub
        Select Case cmbFormatDocelowy.SelectedIndex
            Case 0 ' GeoTIFF (arkusz scalony)
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.GeoTiff, .FormatArkuszaPoczatkowy = FormatArkusza.GeoTiff}
                    f.ShowDialog(Me)
                End Using
            Case 1 ' MBTiles (mobilne i GIS)
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.MBTiles}
                    f.ShowDialog(Me)
                End Using
            Case 2 ' KMZ (Google Earth / Garmin)
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.KmzGarmin}
                    f.ShowDialog(Me)
                End Using
            Case 3 ' JPEG + World file
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.GeoTiff, .FormatArkuszaPoczatkowy = FormatArkusza.Jpeg}
                    f.ShowDialog(Me)
                End Using
            Case 4 ' PNG + World file
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.GeoTiff, .FormatArkuszaPoczatkowy = FormatArkusza.Png}
                    f.ShowDialog(Me)
                End Using
            Case 5 ' OruxMaps (baza SQLite)
                Using f As New FormEksport() With {.FormatPoczatkowy = RodzajEksportu.OruxMaps}
                    f.ShowDialog(Me)
                End Using
            Case 6 ' CAD (DXF + SCR/LISP)
                EksportCadToolStripMenuItem.PerformClick()
            Case Else
                btnScalanie.PerformClick()
        End Select
    End Sub

    Friend Structure WynikPobieraniaInfo
        Public Nieudane As Integer
        Public Wszystkie As Integer
        Public Przerwano As Boolean
        Public Odswiez As Boolean
        Public Folder As String
    End Structure
    Friend _ostatniWynikPobierania As WynikPobieraniaInfo? = Nothing
    Friend _podswietlEksport As Boolean = False

    ''' <summary>Unieważnia zapamiętany stan pobrania po zmianie geometrii, warstw, parametrów lub folderu sesji.</summary>
    Friend Sub UniewaznijWynikPobierania()
        _ostatniWynikPobierania = Nothing
        AktualizujStanBelki()
    End Sub

    ''' <summary>Aktualizuje etykietę stanu sesji i przycisk eksportu na dolnej belce.</summary>
    Friend Sub AktualizujStanBelki()
        If lblStatusSesji Is Nothing Then Exit Sub

        ' 1. Trwające pobieranie
        If _przerwanie IsNot Nothing Then
            lblStatusSesji.Text = "Trwa pobieranie segmentów..."
            lblStatusSesji.ForeColor = Color.FromArgb(37, 99, 235)
            lnkSzczegoly.Visible = False
            btnPobierz.Text = "Pobierz segmenty"
            UstawWyroznienieEksportu(False)
            If btnOtworzFolderDolny IsNot Nothing Then btnOtworzFolderDolny.Enabled = False
            Exit Sub
        End If

        If btnOtworzFolderDolny IsNot Nothing Then
            btnOtworzFolderDolny.Enabled = True
        End If

        Dim s = BiezacaSiatka()
        Dim liczbaSegm As Long = If(s.Poprawna, CLng(s.LiczbaKolumn) * CLng(s.LiczbaWierszy), 0L)

        ' 2. Jeśli mamy wynik z bieżącego pobrania
        If _ostatniWynikPobierania.HasValue Then
            Dim w = _ostatniWynikPobierania.Value
            If w.Przerwano Then
                lblStatusSesji.Text = "Pobieranie przerwane"
                lblStatusSesji.ForeColor = Color.FromArgb(185, 28, 28)
                lnkSzczegoly.Visible = False
                btnPobierz.Text = "Pobierz segmenty"
                UstawWyroznienieEksportu(False)
                Exit Sub
            ElseIf w.Nieudane > 0 Then
                Dim plikErr = Path.Combine(FolderSesji, "error.txt")
                lnkSzczegoly.Visible = File.Exists(plikErr)
                If lnkSzczegoly.Visible Then
                    lnkSzczegoly.Left = lblStatusSesji.Left + Math.Min(lblStatusSesji.PreferredWidth + 8, lblStatusSesji.Width - 65)
                End If
                If w.Odswiez Then
                    lblStatusSesji.Text = $"Nie udało się odświeżyć {w.Nieudane} {Odmiana(w.Nieudane, "segmentu", "segmentów", "segmentów")}"
                    btnPobierz.Text = "Ponów odświeżanie"
                Else
                    lblStatusSesji.Text = $"Brakuje {w.Nieudane} {Odmiana(w.Nieudane, "segmentu", "segmentów", "segmentów")}"
                    btnPobierz.Text = "Pobierz brakujące"
                End If
                lblStatusSesji.ForeColor = Color.FromArgb(185, 28, 28)
                UstawWyroznienieEksportu(False)
                Exit Sub
            Else
                lblStatusSesji.Text = $"{w.Wszystkie} z {w.Wszystkie} segmentów gotowe"
                lblStatusSesji.ForeColor = Color.FromArgb(22, 101, 52)
                lnkSzczegoly.Visible = False
                btnPobierz.Text = "Pobierz segmenty"
                UstawWyroznienieEksportu(True)
                Exit Sub
            End If
        End If

        ' 3. Brak wyniku bieżącego pobrania - sprawdzamy stan folderu sesji
        lnkSzczegoly.Visible = False
        btnPobierz.Text = "Pobierz segmenty"

        Dim plikConf As String = If(FolderSesji <> "", Path.Combine(FolderSesji, "conf.txt"), "")
        If File.Exists(plikConf) Then
            Dim zapisana = PobierzZapisanaKonfiguracje(plikConf)

            If zapisana IsNot Nothing Then
                Dim biezaca = BiezacaKonfiguracja()
                Dim taSamaGeometria = GeometriaTakaSama(biezaca, zapisana)
                If taSamaGeometria Then
                    Dim teSameWarstwy = biezaca.WybraneWarstwy().SequenceEqual(zapisana.WybraneWarstwy(), StringComparer.Ordinal) AndAlso
                                        biezaca.KryciaWarstw().SequenceEqual(zapisana.KryciaWarstw()) AndAlso
                                        biezaca.TrybyMieszaniaWarstwLista().SequenceEqual(zapisana.TrybyMieszaniaWarstwLista())

                    Dim plikKorytarza = Path.Combine(FolderSesji, "korytarz.txt")
                    Dim korytarzZgodny = (_korytarz IsNot Nothing AndAlso File.Exists(plikKorytarza)) OrElse
                                         (_korytarz Is Nothing AndAlso Not File.Exists(plikKorytarza))

                    If Not teSameWarstwy Then
                        lblStatusSesji.Text = "Zmieniono warstwy. Folder sesji zawiera segmenty poprzedniej mapy"
                        lblStatusSesji.ForeColor = Color.FromArgb(180, 83, 9)
                        UstawWyroznienieEksportu(False)
                        Exit Sub
                    ElseIf korytarzZgodny Then
                        Dim liczbaPlikow = biezaca.LiczbaPlikowSegmentow(FolderSesji)
                        Dim oczekiwana As Long = liczbaSegm
                        If _korytarz IsNot Nothing Then
                            If Not s.Poprawna OrElse s.LiczbaSegmentow > MaksSegmentowKorytarzaNaBiezaco OrElse
                               _korytarz.SzacowanaLiczbaSegmentow(s, Ustawienia.Uklad) > MaksSegmentowKorytarzaNaBiezaco Then
                                oczekiwana = liczbaSegm
                            Else
                                oczekiwana = CLng(_korytarz.Segmenty(s, Ustawienia.Uklad).Count)
                            End If
                        End If
                        Dim plikErr = Path.Combine(FolderSesji, "error.txt")
                        If liczbaPlikow > 0 AndAlso liczbaPlikow >= oczekiwana AndAlso Not File.Exists(plikErr) Then
                            lblStatusSesji.Text = $"{liczbaPlikow} z {oczekiwana} segmentów gotowe"
                            lblStatusSesji.ForeColor = Color.FromArgb(22, 101, 52)
                            UstawWyroznienieEksportu(True)
                            Exit Sub
                        End If
                    End If
                End If
            End If
        End If

        ' 4. Domyślny stan - sesja niepobrana (informacje o siatce i obszarze są w całości w karcie "ZAZNACZONY OBSZAR")
        UstawWyroznienieEksportu(False)
        lblStatusSesji.Text = ""
    End Sub

    Private Shared Function GeometriaTakaSama(a As KonfiguracjaSesji, b As KonfiguracjaSesji) As Boolean
        If a Is Nothing OrElse b Is Nothing Then Return False
        Dim sa = a.SiatkaSesji(), sb = b.SiatkaSesji()
        If Not sa.Poprawna OrElse Not sb.Poprawna Then Return False
        If sa.BokSegmentuPx <> sb.BokSegmentuPx Then Return False
        If Math.Abs(sa.RozmiarPiksela - sb.RozmiarPiksela) > 0.000000001 Then Return False
        If sa.LiczbaKolumn <> sb.LiczbaKolumn OrElse sa.LiczbaWierszy <> sb.LiczbaWierszy Then Return False
        If Math.Abs(sa.Obszar.XDol - sb.Obszar.XDol) > 0.000000001 OrElse Math.Abs(sa.Obszar.YLewy - sb.Obszar.YLewy) > 0.000000001 Then Return False
        If a.UkladEpsg <> b.UkladEpsg Then Return False
        Return True
    End Function

    Private Sub UstawWyroznienieEksportu(podswietl As Boolean)
        _podswietlEksport = podswietl
        OdswiezPodpowiedzBelki()
        If btnPrzejdzDoEksportu Is Nothing Then Return
        StylPrzyciskuAkcji(btnPrzejdzDoEksportu, podswietl)
        OdswiezWygladPobierania()
    End Sub

    Private Sub lnkSzczegoly_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkSzczegoly.LinkClicked
        PokazSzczegolyPobierania()
    End Sub

#End Region

    Friend Function WybranyZestaw() As ZestawWarstw
        Return _zestaw
    End Function

    Friend Sub ZaladujSesjeZFolderu(folder As String)
        WczytajConfZFolderu(folder, True)
    End Sub

    Friend Function ZrodlaMapSesji() As Dictionary(Of String, PlikWarstw)
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
        Dim trybOdswiez As Boolean = False
        If _ostatniWynikPobierania.HasValue AndAlso _ostatniWynikPobierania.Value.Nieudane > 0 AndAlso _ostatniWynikPobierania.Value.Odswiez Then
            trybOdswiez = True
        End If
        RozpocznijPobieranie(trybOdswiez)
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
    Friend Function ZadanieDoPodgladu() As ZadaniePobierania
        Dim siatka = BiezacaSiatka()
        If Not siatka.Poprawna Then
            Dim srodek = If(_punktProbkiBezSiatki.HasValue, _punktProbkiBezSiatki.Value, Ustawienia.Uklad.ZWgs84(mapa.Position.Lat, mapa.Position.Lng))
            Dim piksel = PikselPodgladuBezSiatki()
            Dim bok = 2000
            Dim zasieg As New Zasieg(srodek.X - bok * piksel / 2, srodek.Y - bok * piksel / 2, srodek.X + bok * piksel / 2, srodek.Y + bok * piksel / 2)
            siatka = New Siatka(zasieg, piksel, bok)
        End If
        Return New ZadaniePobierania With {
            .Uklad = Ustawienia.Uklad, .Siatka = If(siatka.Poprawna, siatka, Nothing),
            .Zbiory = _zestaw.Zbiory(), .ZrodlaMap = ZrodlaMapSesji(),
            .Format = Ustawienia.Format, .LimitCzasuSekundy = Ustawienia.LimitCzasuSekundy,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .JakoscJpegWmts = Ustawienia.JakoscJpegWmts,
            .Przezroczystosc = EfektywnaPrzezroczystosc(),
            .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_podglad") & "\"}
    End Function

    Private Async Sub RozpocznijPobieranie(odswiez As Boolean)
        If _przerwanie IsNot Nothing Then Exit Sub
        ZakonczWyborNaroznikow()
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
            Komunikat("Niepoprawny zasięg pobierania: " & siatka.PowodNiepoprawnosci & ".", Color.Red) : Exit Sub
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
                Try
                    Ustawienia.Zapisz(PlikLastsettings)
                Catch
                End Try
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
                Komunikat("Trasa nie przecina obszaru pobierania - wybierz trasę ponownie albo usuń obszar wzdłuż trasy (Sesja pobierania > Usuń obszar wzdłuż trasy).", Color.Red) : Exit Sub
            End If
        End If

        'zapis parametrów sesji - na wszelki wypadek, żeby nie okazało się, że zapisuje w folderze głównym programu
        If FolderSesji = "" Then
            MsgBox("Nie ustalono folderu sesji - wybierz folder przyciskiem folderu w oknie głównym.", MsgBoxStyle.Exclamation, "Pobieranie")
            Exit Sub
        End If
        If Not PrzygotujFolderSesji() Then Exit Sub
        _dataPobrania = MetadaneMapy.TekstDaty(Date.Today)
        ZapiszConf(False)

        Dim zadanie As New ZadaniePobierania With {
            .AdresSerwera = _zestaw(0).Adres, .Warstwy = _zestaw.Nazwy(), .Krycia = _zestaw.Krycia(), .Adresy = _zestaw.Adresy(),
            .TrybyMieszania = _zestaw.TekstTrybowMieszania(),
            .Zbiory = _zestaw.Zbiory(), .ZrodlaMap = zrodlaSesji, .Uklad = Ustawienia.Uklad,
            .Siatka = siatka, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .Numeracja = Siatka.StylZTekstu(Ustawienia.Numeracja), .Folder = FolderSesji,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .PrzerwaSekundy = Ustawienia.PrzerwaMiedzyProbami,
            .LiczbaWatkow = Ustawienia.LiczbaWatkow, .LimitCzasuSekundy = Ustawienia.LimitCzasuSekundy,
            .ZachowajKafleWmts = Ustawienia.ZachowajKafleWmts, .JakoscJpegWmts = Ustawienia.JakoscJpegWmts,
            .Georeferencja = Ustawienia.OpcjeGeoreferencji(), .TrekBuddy = Ustawienia.TrekBuddy, .NazwaTrekBuddy = Ustawienia.NazwaTrekBuddy,
            .Przezroczystosc = EfektywnaPrzezroczystosc(), .Korytarz = _korytarz, .Odswiez = odswiez}

        btnPobierz.Enabled = False
        btnPrzerwij.Enabled = True
        UstawDostepnoscDrzewa(False)
        btnScalanie.Enabled = False
        btnUsunPobrane.Enabled = False
        _btnNowaSesja.Enabled = False
        If _btnKreator IsNot Nothing Then _btnKreator.Enabled = False
        If btnKreatorDolny IsNot Nothing Then btnKreatorDolny.Enabled = False
        If btnPrzejdzDoEksportu IsNot Nothing Then btnPrzejdzDoEksportu.Enabled = False
        OdswiezToolStripMenuItem.Enabled = False
        KorytarzToolStripMenuItem.Enabled = False
        chkEdycjaXY.Enabled = False
        Komunikat(If(odswiez, "Trwa odświeżanie segmentów (", "Trwa pobieranie segmentów (") & liczbaSegmentow & ")", Color.Black)

        'własna kopia - zwalniane jest zawsze źródło tego pobierania, nawet gdyby pole wskazywało już inne
        Dim przerwanie As New CancellationTokenSource()
        _przerwanie = przerwanie
        _ostatniWynikPobierania = Nothing
        WyzerujLicznikBledow()
        AktualizujStanBelki()
        OdswiezPrzyciskiWarstw()
        Dim postep As New Progress(Of PostepPobierania)(AddressOf PokazPostep)
        Dim wynik As WynikPobierania = Nothing
        Try
            wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, postep, przerwanie.Token)
        Catch ex As Exception
            If Not IsDisposed AndAlso Not Disposing Then
                Komunikat("Błąd pobierania: " & MetadaneMapy.MaskujSekrety(OpisBledu.Z(ex)), Color.Red)
            End If
        Finally
            _cacheConfSciezka = ""
            _cacheConfObiekt = Nothing
            If _przerwanie Is przerwanie Then _przerwanie = Nothing
            przerwanie.Dispose()
            If Not IsDisposed AndAlso Not Disposing Then
                btnPobierz.Enabled = True
                btnPrzerwij.Enabled = False
                UstawDostepnoscDrzewa(True)
                btnUsunPobrane.Enabled = True
                _btnNowaSesja.Enabled = True
                If _btnKreator IsNot Nothing Then _btnKreator.Enabled = True
                If btnKreatorDolny IsNot Nothing Then btnKreatorDolny.Enabled = True
                If btnPrzejdzDoEksportu IsNot Nothing Then btnPrzejdzDoEksportu.Enabled = True
                OdswiezToolStripMenuItem.Enabled = True
                KorytarzToolStripMenuItem.Enabled = True
                chkEdycjaXY.Enabled = True
                stPostep.Value = 0
                If _postepBelki IsNot Nothing Then _postepBelki.Value = 0
                stSegment.Text = ""
                OdswiezPrzyciskiWarstw()
                OdswiezPodpowiedzBelki()
            End If
        End Try

        If IsDisposed OrElse Disposing Then Exit Sub
        If wynik Is Nothing Then
            AktualizujStanBelki()
            Exit Sub
        End If
        If wynik.Przerwano Then
            Komunikat("Przerwano pobieranie segmentów", Color.Red)
        ElseIf wynik.Nieudane > 0 Then
            Dim plikErr As String = zadanie.Folder & "error.txt"
            Dim trescErr As String = ""
            Try
                If File.Exists(plikErr) Then trescErr = File.ReadAllText(plikErr)
            Catch
            End Try
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
        btnScalanie.Enabled = True
        _ostatniWynikPobierania = New WynikPobieraniaInfo With {
            .Nieudane = wynik.Nieudane,
            .Wszystkie = wynik.Wszystkie,
            .Przerwano = wynik.Przerwano,
            .Odswiez = odswiez,
            .Folder = zadanie.Folder
        }
        AktualizujStanBelki()
    End Sub


    Private Sub PokazPostep(p As PostepPobierania)
        If IsDisposed OrElse Disposing Then Exit Sub
        If p.Komunikat <> "" Then Komunikat(p.Komunikat, Color.Black)
        If p.Wszystkie > 0 Then
            stPostep.Value = Math.Min(100, CInt(p.Pobrane * 100L \ p.Wszystkie))
            If _postepBelki IsNot Nothing Then _postepBelki.Value = stPostep.Value
            OdswiezPasekSesji()
        End If
        stSegment.Text = p.Segment
        If _przerwanie IsNot Nothing AndAlso p.Wszystkie > 0 AndAlso _szczegolyStanuBelki IsNot Nothing Then
            _szczegolyStanuBelki.Text = $"Postęp {stPostep.Value}% · {p.Segment}"
        End If
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
        Ustawienia.PodkladMapy = "osm"
        GMapProvider.UserAgent = Program.UserAgentProgramu
        ZmienPodkladMapy(GMapProviders.OpenStreetMap)
    End Sub

    Private Sub rbGoogle_CheckedChanged(sender As Object, e As EventArgs) Handles rbGoogle.CheckedChanged
        If rbGoogle.Checked = False Then Exit Sub
        Ustawienia.PodkladMapy = "google"
        ZmienPodkladMapy(GMapProviders.GoogleMap)
    End Sub

    Private Sub rbBing_CheckedChanged(sender As Object, e As EventArgs) Handles rbBing.CheckedChanged
        If rbBing.Checked = False Then Exit Sub
        Ustawienia.PodkladMapy = "bing"
        ZmienPodkladMapy(GMapProviders.BingSatelliteMap)
    End Sub

    ''' <summary>Zaznacza podkład mapy zapamiętany w lastsettings.txt (zaznaczenie wywołuje zmianę podkładu).</summary>
    Private Sub UstawPodkladMapy(podklad As String)
        Select Case UstawieniaProgramu.NormalizujPodklad(podklad)
            Case "google" : rbGoogle.Checked = True
            Case "bing" : rbBing.Checked = True
            Case Else : rbOsm.Checked = True
        End Select
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
            WpiszPolaZasiegu(Liczba(Math.Round(xMin, 6)), Liczba(Math.Round(yMin, 6)), Liczba(Math.Round(xMax, 6)), Liczba(Math.Round(yMax, 6)))
        Else
            WpiszPolaZasiegu(Liczba(Math.Floor(xMin)), Liczba(Math.Floor(yMin)), Liczba(Math.Ceiling(xMax)), Liczba(Math.Ceiling(yMax)))
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
        ZapamietajKursor(k.X, k.Y)
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
        If _trybZaznaczania Then Exit Sub
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
                If trasa.Stroke Is _pioroTrasy Then trasa.Stroke = Nothing
                trasa.Dispose()
            Next
            overlay.Routes.Clear()
            For Each wielokat In overlay.Polygons
                If wielokat.Fill Is _pedzelZewnetrzny OrElse wielokat.Fill Is _pedzelSiatki OrElse wielokat.Fill Is _pedzelDzialki Then wielokat.Fill = Nothing
                If wielokat.Stroke Is _pioroZewnetrzne OrElse wielokat.Stroke Is _pioroSiatki OrElse wielokat.Stroke Is _pioroDzialki Then wielokat.Stroke = Nothing
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
        _poleSzukaniaMiejsca?.ZamknijListe()
        If _przerwanie IsNot Nothing OrElse Ustawienia.EdycjaXY Then Exit Sub
        If _trybZaznaczania Then
            If e.Button <> MouseButtons.Right Then Return
            ZakonczWyborNaroznikow()
        End If
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
    ' Zasięg zaznaczony prawym przyciskiem myszy w tej sesji programu (także dopasowany potem uchwytami) - kreator mapy
    ' pomija wtedy krok wyboru obszaru. Obszar z lastsettings.txt, wpisany w pola XY albo z wyszukiwarki się nie liczy.
    Private _zasiegZaznaczonyMysza As Zasieg?

    ''' <summary>Czy pola XY pokazują obszar zaznaczony w tej sesji prawym przyciskiem myszy (i ewentualnie dopasowany uchwytami).</summary>
    Friend Function CzyZasiegZaznaczonyMysza() As Boolean
        Return _zasiegZaznaczonyMysza.HasValue AndAlso _zasiegZaznaczonyMysza.Value.Equals(ZasiegPolXY())
    End Function

    Private Sub mapa_OnSelectionChange(Selection As RectLatLng, ZoomToFit As Boolean) Handles mapa.OnSelectionChange
        If _przerwanie IsNot Nothing OrElse Ustawienia.EdycjaXY Then
            'zaznaczenie bez skutku - pozostawione na mapie nadpisywało potem pola XY przy każdym ruchu myszy
            If Not Selection.IsEmpty Then mapa.SelectedArea = RectLatLng.Empty
            Exit Sub
        End If
        If Selection.IsEmpty OrElse Selection.WidthLng <= 0.00001 OrElse Selection.HeightLat <= 0.00001 Then Exit Sub
        ZakonczWyborNaroznikow()
        If _podpowiedzZaznaczenia IsNot Nothing Then _podpowiedzZaznaczenia.Text = ""
        If _korytarz IsNot Nothing Then UsunKorytarz(False)
        UstawObszarZZaznaczenia(Selection)
        _zasiegZaznaczonyMysza = ZasiegPolXY()
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
        Return _korytarz Is Nothing AndAlso Not Ustawienia.EdycjaXY AndAlso _przerwanie Is Nothing AndAlso Not _trybZaznaczania
    End Function

    Private Sub mapa_OnMarkerEnter(item As GMapMarker) Handles mapa.OnMarkerEnter
        _uchwytPodKursorem = TryCast(item, UchwytSiatki)
        If _uchwytPodKursorem IsNot Nothing AndAlso UchwytyAktywne() Then mapa.Cursor = _uchwytPodKursorem.Kursor
    End Sub

    Private Sub mapa_OnMarkerLeave(item As GMapMarker) Handles mapa.OnMarkerLeave
        If item Is _uchwytPodKursorem Then _uchwytPodKursorem = Nothing
    End Sub

    Private Sub mapa_MouseUp(sender As Object, e As MouseEventArgs) Handles mapa.MouseUp
        If _trybZaznaczania AndAlso e.Button = MouseButtons.Left Then
            PrzyjmijNaroznik(mapa.FromLocalToLatLng(e.X, e.Y))
            Return
        End If
        If _przeciaganyUchwyt IsNot Nothing AndAlso e.Button = MouseButtons.Left Then ZakonczPrzeciaganie(anuluj:=False)
    End Sub

    ''' <summary>Skróty klawiaturowe okna głównego (Esc dla uchwytu, Ctrl+F wyszukiwanie, Ctrl+D działki).</summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape AndAlso _trybZaznaczania Then
            ZakonczWyborNaroznikow()
            Return True
        End If
        If keyData = Keys.Escape AndAlso _przeciaganyUchwyt IsNot Nothing Then
            ZakonczPrzeciaganie(anuluj:=True)
            Return True
        End If
        If keyData = Keys.Escape AndAlso grpRozdzielczoscFormat.Visible AndAlso Not txtSzukajWarstwy.Focused AndAlso
           (_poleSzukaniaMiejsca Is Nothing OrElse Not _poleSzukaniaMiejsca.Pole.Focused) Then
            ZwinParametryBelki()
            Return True
        End If
        If keyData = (Keys.Control Or Keys.F) Then
            'pole wyszukiwania na pasku narzędzi; bez niego (np. w starszym układzie) - okno wyszukiwarki
            If _poleSzukaniaMiejsca IsNot Nothing AndAlso _poleSzukaniaMiejsca.Visible Then
                _poleSzukaniaMiejsca.Pole.Focus()
                _poleSzukaniaMiejsca.Pole.SelectAll()
            Else
                OtworzWyszukiwarke(0)
            End If
            Return True
        End If
        If keyData = (Keys.Control Or Keys.D) Then
            OtworzWyszukiwarke(1)
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
        WpiszPolaZasiegu(Liczba(z.XDol), Liczba(z.YLewy), Liczba(z.XGora), Liczba(z.YPrawy))
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
            Dim zaznaczonyMysza = _zasiegZaznaczonyMysza.HasValue AndAlso _zasiegZaznaczonyMysza.Value.Equals(
                New Zasieg(Wartosc(_polaPrzedPrzeciaganiem(0)), Wartosc(_polaPrzedPrzeciaganiem(1)), Wartosc(_polaPrzedPrzeciaganiem(2)), Wartosc(_polaPrzedPrzeciaganiem(3))))
            UstawPolaZasiegu(_zasiegPrzeciagany)
            ' dopasowanie uchwytami tego samego zaznaczenia zostaje zaznaczeniem myszą
            If zaznaczonyMysza Then _zasiegZaznaczonyMysza = ZasiegPolXY()
            'współrzędne WGS84 zaznaczenia - prostokąt opisany na nowym zasięgu
            Dim g = ProstokatGeo.Opisany(Ustawienia.Uklad, _zasiegPrzeciagany)
            lblZaznSzerGora.Text = "lat= " & Liczba(Math.Round(g.Polnoc, 4))
            lblZaznSzerDol.Text = "lat= " & Liczba(Math.Round(g.Poludnie, 4))
            lblZaznDlugPrawa.Text = "lng= " & Liczba(Math.Round(g.Wschod, 4))
            lblZaznDlugLewa.Text = "lng= " & Liczba(Math.Round(g.Zachod, 4))
        Else
            WpiszPolaZasiegu(_polaPrzedPrzeciaganiem(0), _polaPrzedPrzeciaganiem(1), _polaPrzedPrzeciaganiem(2), _polaPrzedPrzeciaganiem(3))
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
        If _dzialkaKatastralna IsNot Nothing Then
            PokazDzialke()
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
                .Fill = _pedzelZewnetrzny,
                .Stroke = _pioroZewnetrzne})
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
                    .Fill = _pedzelSiatki,
                    .Stroke = _pioroSiatki})
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
        For Each c As Control In New Control() {btnPowieksz, btnPomniejsz, lblZoom, btnSzukajDzialki, _polePodkladuMapy,
                                                rbGoogle, rbBing, rbOsm,
                                                lblKursorTytul, lblKursorUkladOpis, lblKursorUklad, lblSrodekTytul,
                                                lblSrodekOpisSzer, lblSrodekSzer, lblSrodekOpisDlug, lblSrodekDlug}
            If c IsNot Nothing AndAlso c.Visible Then zaslony.Add(mapa.RectangleToClient(c.RectangleToScreen(c.ClientRectangle)))
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

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles Me.DragEnter, mapa.DragEnter
        If e.Data IsNot Nothing AndAlso e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim pliki = TryCast(e.Data.GetData(DataFormats.FileDrop), String())
            If pliki IsNot Nothing AndAlso pliki.Length > 0 Then
                Dim ext = Path.GetExtension(pliki(0)).ToLowerInvariant()
                If ext = ".gpx" OrElse ext = ".kml" OrElse ext = ".kmz" OrElse ext = ".map" OrElse ext = ".geojson" OrElse ext = ".json" OrElse ext = ".mapotero" OrElse ext = ".gpkg" OrElse Directory.Exists(pliki(0)) Then
                    e.Effect = DragDropEffects.Copy
                    Return
                End If
            End If
        End If
        e.Effect = DragDropEffects.None
    End Sub

    ''' <summary>
    ''' Pokazuje podgląd poligonu z pliku KML/GeoJSON i ustawia zasięg siatki na jego obrys w układzie sesji.
    ''' </summary>
    Private Sub WczytajObszarPoligonu(plik As String)
        If Path.GetExtension(plik).Equals(".gpkg", StringComparison.OrdinalIgnoreCase) Then
            WczytajObszarZGeoPackage(plik, linie:=False)
            Return
        End If
        UstawObszarPoligonu(FormPodgladPoligonu.PokazDlaPliku(Me, plik), plik)
    End Sub

    ''' <summary>
    ''' Obszar z warstwy GeoPackage: poligony dają prostokąt obszaru, linie - obszar wzdłuż trasy (korytarz).
    ''' <paramref name="linie"/> - przy warstwie z poligonami i liniami wybierane są linie.
    ''' </summary>
    Private Sub WczytajObszarZGeoPackage(plik As String, linie As Boolean)
        Dim geometria = WyborWarstwyGeoPackage.Wczytaj(Me, plik, linie)
        If geometria Is Nothing Then Return
        If geometria.Poligony.Count > 0 AndAlso (Not linie OrElse geometria.Linie.Count = 0) Then
            UstawObszarPoligonu(FormPodgladPoligonu.PokazDlaZestawu(Me, geometria.ZestawPoligonowWgs84()), plik)
            Return
        End If
        Using okno As New FormKorytarz(geometria.TrasaWgs84(Path.GetFileNameWithoutExtension(plik)))
            If okno.ShowDialog(Me) = DialogResult.OK AndAlso okno.Wynik IsNot Nothing Then UstawKorytarz(okno.Wynik)
        End Using
    End Sub

    Private Sub UstawObszarPoligonu(zestaw As ZestawPoligonow, plik As String)
        If zestaw Is Nothing OrElse zestaw.Wielokaty.Count = 0 Then Return
        _korytarz = Nothing
        UsunKorytarzToolStripMenuItem.Enabled = False
        Dim z = ZaokraglonyZasieg(zestaw.ZasiegWUkladzie(Ustawienia.Uklad))
        WpiszPolaZasiegu(Liczba(z.XDol), Liczba(z.YLewy), Liczba(z.XGora), Liczba(z.YPrawy))
        WyczyscZnaczniki()
        PokazZnaczniki(centruj:=True)
        Komunikat("Wczytano obszar poligonu: " & Path.GetFileName(plik) & " (" & zestaw.Wielokaty.Count & " obiektów).", Color.Green)
    End Sub

    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles Me.DragDrop, mapa.DragDrop
        If PobieranieTrwa("Wczytanie pliku") Then Exit Sub
        If e.Data Is Nothing OrElse Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Exit Sub
        Dim pliki = TryCast(e.Data.GetData(DataFormats.FileDrop), String())
        If pliki Is Nothing OrElse pliki.Length = 0 Then Exit Sub

        ' Sprawdzenie czy upuszczono pliki kalibracji OziExplorer (.map) lub folder z takimi plikami
        Dim plikiMap As New List(Of String)()
        For Each p In pliki
            If Directory.Exists(p) Then
                plikiMap.AddRange(FormImportOzi.ZnajdzPlikiMapWFolderze(p))
            ElseIf String.Equals(Path.GetExtension(p), ".map", StringComparison.OrdinalIgnoreCase) Then
                plikiMap.Add(p)
            End If
        Next
        If plikiMap.Count > 0 Then
            Using okno As New FormImportOzi()
                If plikiMap.Count = 1 Then
                    okno.OtworzZPlikiem(plikiMap(0))
                Else
                    okno.OtworzWsadowo(plikiMap)
                End If
                okno.ShowDialog(Me)
            End Using
            Exit Sub
        End If

        Dim plik = pliki(0)
        Dim ext = Path.GetExtension(plik).ToLowerInvariant()
        If ext = ".mapotero" OrElse ext = ".json" Then
            Try
                Dim tresc = File.ReadAllText(plik, System.Text.Encoding.UTF8)
                If ext = ".mapotero" OrElse PrzekazZestawu.CzyWygladaJakZestawGalerii(tresc) Then
                    Dim dane = PrzekazZestawu.ZTekstu(tresc)
                    Dim wynik = ZastosujPrzepisZestawu(dane, "z pliku " & Path.GetFileName(plik))
                    If Not wynik.Ok Then
                        MsgBox(wynik.Komunikat, MsgBoxStyle.Exclamation, "Kanapka warstw")
                    End If
                    Exit Sub
                End If
            Catch ex As Exception
                MsgBox("Nie udało się wczytać kanapki warstw: " & OpisBledu.Z(ex), MsgBoxStyle.Critical, "Kanapka warstw")
                Exit Sub
            End Try
        End If
        If ext = ".geojson" OrElse ext = ".json" OrElse ext = ".gpkg" Then
            Try
                WczytajObszarPoligonu(plik)
            Catch ex As Exception
                MsgBox("Nie udało się odczytać poligonu: " & OpisBledu.Z(ex), MsgBoxStyle.Critical, "Obszar poligonu")
            End Try
            Exit Sub
        End If

        If ext = ".gpx" OrElse ext = ".kml" OrElse ext = ".kmz" Then
            Try
                Dim k = Korytarz.WczytajTrase(plik)
                If Not k.Poprawny Then
                    'Poligon czyta tylko parser KML (GPX i KMZ dałyby komunikat o nieobsługiwanym rozszerzeniu, a potem drugi)
                    Dim maPoligon = False
                    If ext = ".kml" Then
                        Try
                            maPoligon = ParserPoligonu.WczytajZPliku(plik).Wielokaty.Count > 0
                        Catch ex As InvalidDataException
                            'KML bez <Polygon> - zostaje komunikat poniżej
                        End Try
                    End If
                    If maPoligon Then
                        WczytajObszarPoligonu(plik)
                        Exit Sub
                    End If

                    MsgBox("Plik " & Path.GetFileName(plik) & " nie zawiera śladu, trasy, linii ani poligonu (GPX: trk, rte, wpt; KML/KMZ: LineString, gx:Track, Polygon).", MsgBoxStyle.Exclamation, "Obszar pobierania")
                    Exit Sub
                End If
                Using okno As New FormKorytarz(k)
                    If okno.ShowDialog(Me) = DialogResult.OK AndAlso okno.Wynik IsNot Nothing Then
                        UstawKorytarz(okno.Wynik)
                    End If
                End Using
            Catch ex As Exception
                MsgBox("Nie udało się odczytać pliku trasy: " & OpisBledu.Z(ex), MsgBoxStyle.Critical, "Obszar wzdłuż trasy")
            End Try
        End If
    End Sub

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
        WpiszPolaZasiegu(Liczba(z.XDol), Liczba(z.YLewy), Liczba(z.XGora), Liczba(z.YPrawy))
        WyczyscZnaczniki()
        PokazZnaczniki()
        'widok mapy obejmujący całą trasę
        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            Dim g = ProstokatGeo.Opisany(Ustawienia.Uklad, s.ZasiegSiatki)
            mapa.SetZoomToFitRect(New RectLatLng(g.Polnoc, g.Zachod, g.Wschod - g.Zachod, g.Polnoc - g.Poludnie))
        End If
        Komunikat(OpisKorytarza(), Color.Green)
        If _zaladowany Then ZglosOdswiezeniePodgladu()
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
        If _zaladowany Then ZglosOdswiezeniePodgladu()
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
                        .Fill = _pedzelSiatki, .Stroke = _pioroSiatki})
                Next
            End If
        End If
        For Each l In _korytarz.Linie
            Dim punkty = l.ConvertAll(Function(p) New PointLatLng(p.Szerokosc, p.Dlugosc))
            warstwa.Routes.Add(New GMapRoute(punkty, "trasa") With {.Stroke = _pioroTrasy})
        Next
    End Sub

#End Region

#Region "Przyciski i menu"

    Private Sub btnOtworzFolder_Click(sender As Object, e As EventArgs) Handles btnOtworzFolder.Click, btnOtworzFolderDolny.Click
        Dim folder = If(Not String.IsNullOrWhiteSpace(FolderSesji), FolderSesji, FolderDownload)
        Directory.CreateDirectory(folder)
        OtworzFolder(folder)
    End Sub

    Private Sub btnUsunPobrane_Click(sender As Object, e As EventArgs) Handles btnUsunPobrane.Click, UsunPobraneToolStripMenuItem.Click
        If PobieranieTrwa("Usunięcie pobranych map") Then Exit Sub
        Dim folder = If(Not String.IsNullOrWhiteSpace(FolderSesji), FolderSesji, FolderDownload)
        If Not Directory.Exists(folder) Then
            Komunikat("Folder bieżącej sesji nie istnieje: " & folder, Color.Red)
            Exit Sub
        End If

        Dim pelnyFolder As String = Path.GetFullPath(folder).TrimEnd("\"c, "/"c)
        Dim pelnyDownload As String = Path.GetFullPath(FolderDownload).TrimEnd("\"c, "/"c)
        Dim czyGlownyDownload As Boolean = String.Equals(pelnyFolder, pelnyDownload, StringComparison.OrdinalIgnoreCase)
        'cały folder tylko wtedy, gdy jest podfolderem download (folder utworzony przez program); folder sesji wskazany
        'przez użytkownika (np. D:\GIS\Projekt) mógł zawierać jego inne pliki i podfoldery - usuwane są tylko pliki
        Dim czyPodfolderDownload As Boolean = Not czyGlownyDownload AndAlso
            pelnyFolder.StartsWith(pelnyDownload & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        Dim tylkoPliki As Boolean = Not czyPodfolderDownload

        Dim komunikatPytania As String
        If czyGlownyDownload Then
            komunikatPytania = "Czy na pewno przenieść do Kosza pliki bieżącej sesji z folderu download?" & vbCrLf & vbCrLf &
                               "Pliki segmentów i konfiguracji zostaną przeniesione do Kosza (podkatalogi innych sesji pozostaną nienaruszone)."
        ElseIf tylkoPliki Then
            komunikatPytania = "Czy na pewno przenieść do Kosza pliki bieżącej sesji z folderu" & vbCrLf & vbCrLf &
                               folder & "?" & vbCrLf & vbCrLf &
                               "Folder leży poza folderem download, więc zostaną usunięte tylko pliki leżące bezpośrednio w nim; " &
                               "podfoldery i sam folder pozostaną."
        Else
            komunikatPytania = "Czy na pewno przenieść do Kosza folder bieżącej sesji?" & vbCrLf & vbCrLf &
                               folder & vbCrLf & vbCrLf &
                               "Zawartość folderu zostanie bezpiecznie przeniesiona do Kosza."
        End If

        Dim odp As DialogResult = MessageBox.Show(komunikatPytania, "Usuwanie plików bieżącej sesji",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
        If odp = DialogResult.Yes Then
            Try
                If tylkoPliki Then
                    'paczkami jedną operacją Windows - pojedynczo ok. 70 ms na plik (przy tysiącach segmentów okno stało minutami)
                    Dim pliki = Directory.GetFiles(folder)
                    Cursor = Cursors.WaitCursor
                    Dim nieprzeniesione As Integer
                    Try
                        nieprzeniesione = PrzeniesDoKosza(pliki)
                    Finally
                        Cursor = Cursors.Default
                    End Try
                    btnScalanie.Enabled = False
                    Komunikat("Przeniesiono do Kosza pliki bieżącej sesji (" & (pliki.Length - nieprzeniesione) & " plików) z folderu " & folder &
                              If(pliki.Length > MaksPlikowPojedynczo, " (w Koszu jako folder """ & NazwaFolderuKosza & " (data i godzina)"")", "") &
                              If(nieprzeniesione > 0, "; nie udało się przenieść " & nieprzeniesione & " plików (np. otwartych w innym programie).", ""),
                              If(nieprzeniesione > 0, Color.Red, Color.Green))
                Else
                    FileIO.FileSystem.DeleteDirectory(folder, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
                    FolderSesji = FolderDownload
                    btnScalanie.Enabled = False
                    Komunikat("Przeniesiono do Kosza folder sesji: " & folder, Color.Green)
                End If
            Catch ex As Exception
                Komunikat("Nie udało się przenieść sesji do Kosza: " & OpisBledu.Z(ex), Color.Red)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Scalanie segmentów (przycisk i menu): okno Eksport mapy z wybranym formatem Arkusz - dawne osobne okno scalania
    ''' robiło to samo co eksport GeoTIFF, bez nakładek.
    ''' </summary>
    Private Sub btnScalanie_Click(sender As Object, e As EventArgs) Handles btnScalanie.Click, ScalanieToolStripMenuItem.Click
        If PobieranieTrwa("Scalanie segmentów") Then Exit Sub
        Using f As New FormEksport() With {.ArkuszDomyslnie = True}
            f.ShowDialog(Me)
        End Using
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

    Private Sub FolderWlasnychWarstwToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FolderWlasnychWarstwToolStripMenuItem.Click
        Try
            Dim folder = FolderWarstwDanych
            Directory.CreateDirectory(folder)
            OtworzFolder(folder)
        Catch ex As Exception
            Komunikat("Nie udało się otworzyć folderu własnych warstw: " & OpisBledu.Z(ex), Color.Red)
        End Try
    End Sub

    Private Sub EksportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EksportToolStripMenuItem.Click
        If PobieranieTrwa("Eksport") Then Exit Sub
        Using f As New FormEksport() With {.ArkuszDomyslnie = False}
            f.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>Otwiera okno eksportu mapy z opcjonalnym domyślnym rodzajem eksportu.</summary>
    Friend Sub OtworzOknoEksportu(Optional rodzaj As RodzajEksportu = RodzajEksportu.MBTiles)
        If PobieranieTrwa("Eksport") Then Exit Sub
        Using f As New FormEksport() With {.ArkuszDomyslnie = (rodzaj = RodzajEksportu.GeoTiff)}
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub EksportCadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EksportCadToolStripMenuItem.Click
        Using f As New FormEksportCad()
            f.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>
    ''' Okno Ustawienia otwierane z menu Narzędzia.
    ''' W trakcie pobierania okno pozwala zmienić tylko widok współrzędnych i układ domyślny (Form2.WypelnijKontrolki).
    ''' </summary>
    Private Sub UstawieniaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UstawieniaToolStripMenuItem.Click, stFormat.Click
        Form2.TrwaPobieranie = _przerwanie IsNot Nothing
        Form2.ShowDialog()
    End Sub

    ''' <summary>Adres oficjalnej strony pomocy i podręcznika użytkownika MapoTero w serwisie GitHub Pages.</summary>
    Public Const UrlPomocyOnline As String = "https://eksploracja.github.io/MapoTero/pomoc/"

    Private Sub PomocOnlineToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PomocOnlineToolStripMenuItem.Click
        OtworzPomocOnline()
    End Sub

    Private Sub GaleriaWarstwToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GaleriaWarstwToolStripMenuItem.Click, btnGaleriaWarstw.Click
        OtworzGalerieWarstw()
    End Sub

    Private Shared Function UtworzIkoneGalerii() As Bitmap
        Dim bmp As New Bitmap(16, 16)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            Using p1 As New SolidBrush(Color.FromArgb(46, 125, 50)),
                  p2 As New SolidBrush(Color.FromArgb(29, 78, 216)),
                  p3 As New SolidBrush(Color.FromArgb(217, 119, 6)),
                  p4 As New SolidBrush(Color.FromArgb(114, 83, 237)),
                  ramka As New Pen(Color.FromArgb(80, 80, 90))
                g.FillRectangle(p1, 1, 1, 6, 6)
                g.FillRectangle(p2, 9, 1, 6, 6)
                g.FillRectangle(p3, 1, 9, 6, 6)
                g.FillRectangle(p4, 9, 9, 6, 6)
                g.DrawRectangle(ramka, 1, 1, 5, 5)
                g.DrawRectangle(ramka, 9, 1, 5, 5)
                g.DrawRectangle(ramka, 1, 9, 5, 5)
                g.DrawRectangle(ramka, 9, 9, 5, 5)
            End Using
        End Using
        Return bmp
    End Function

    ''' <summary>
    ''' Uruchamia lokalny nasłuch MostekGalerii (jeśli nie działa) i otwiera stronę galerii warstw w przeglądarce
    ''' z parametrami połączenia (port i token), aby użytkownik mógł jednym kliknięciem dodać warstwę lub całą kanapkę.
    ''' Zmienna środowiskowa MAPOTERO_GALERIA_URL pozwala wskazać lokalny serwer podglądu podczas testów.
    ''' </summary>
    Public Sub OtworzGalerieWarstw()
        Try
            If _mostekGalerii Is Nothing Then
                Dim opcje As New OpcjeMostkaGalerii With {
                    .WersjaProgramu = Program.Wersja,
                    .ObslugaDodaj = AddressOf ObsluzWarstweZGalerii,
                    .ObslugaPobierzStan = AddressOf PobierzStanDlaGalerii,
                    .ObslugaZastosujZestaw = AddressOf ObsluzZestawZGalerii,
                    .ObslugaPodglad = AddressOf ObsluzPodgladZGaleriiAsync
                }
                _mostekGalerii = New MostekGalerii(opcje)
            End If
        Catch ex As Exception
            Komunikat("Nie udało się uruchomić połączenia z galerią warstw: " & OpisBledu.Z(ex), Color.DarkOrange)
        End Try

        Dim adresBazowy = Environment.GetEnvironmentVariable("MAPOTERO_GALERIA_URL")
        If String.IsNullOrWhiteSpace(adresBazowy) Then
            adresBazowy = UrlPomocyOnline.TrimEnd("/"c) & "/galeria-warstw/"
        End If
        Dim url = If(_mostekGalerii IsNot Nothing, _mostekGalerii.ZbudujAdresGalerii(adresBazowy), adresBazowy)
        Try
            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć galerii warstw: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Galeria warstw")
        End Try
    End Sub

    Private Function ObsluzWarstweZGalerii(zbiorNazwa As String, warstwaNazwa As String) As WynikMostkaGalerii
        If Me.IsDisposed Then Return WynikMostkaGalerii.Blad("blad", "Okno programu zostało zamknięte.")
        If Me.InvokeRequired Then
            Return DirectCast(Me.Invoke(New Func(Of String, String, WynikMostkaGalerii)(AddressOf ObsluzWarstweZGalerii), zbiorNazwa, warstwaNazwa), WynikMostkaGalerii)
        End If

        If _przerwanie IsNot Nothing Then
            Return WynikMostkaGalerii.Blad("zajety", "Trwa pobieranie mapy - zatrzymaj je przed zmianą listy warstw.")
        End If

        Dim zbior = _zbiory.FirstOrDefault(Function(z) String.Equals(z.Nazwa, zbiorNazwa, StringComparison.OrdinalIgnoreCase))
        If zbior Is Nothing Then
            Return WynikMostkaGalerii.Blad("nie_znaleziono", $"Nie znaleziono zbioru map '{zbiorNazwa}' w tym programie.")
        End If
        Dim zWarstwy = zbior.Plik.Znajdz(warstwaNazwa)
        If zWarstwy Is Nothing Then
            Return WynikMostkaGalerii.Blad("nie_znaleziono", $"Nie znaleziono warstwy '{warstwaNazwa}' w zbiorze '{zbiorNazwa}'.")
        End If

        If _zestaw.Zawiera(zbior.Plik.Adres, zWarstwy.Nazwa) Then
            PrzywrocOknoNaWierzch()
            Return WynikMostkaGalerii.JuzWybrana($"Warstwa '{zWarstwy.Nazwa}' jest już wybrana.")
        End If

        Dim nazwaWyswietlana = If(_katalog IsNot Nothing, _katalog.ZnajdzNazwe(zbior.Nazwa, zWarstwy.Nazwa), "")
        If String.IsNullOrWhiteSpace(nazwaWyswietlana) Then nazwaWyswietlana = zWarstwy.ToString()

        Dim w As New WarstwaWDrzewie With {
            .Zbior = zbior,
            .Warstwa = zWarstwy.Nazwa,
            .Nazwa = nazwaWyswietlana
        }
        DodajWarstwe(w)
        PrzywrocOknoNaWierzch()
        Return WynikMostkaGalerii.Dodano($"Dodano warstwę '{nazwaWyswietlana}'.")
    End Function

    Private Function PobierzStanDlaGalerii() As StanMostkaGalerii
        If Me.IsDisposed Then Return New StanMostkaGalerii()
        If Me.InvokeRequired Then
            Return DirectCast(Me.Invoke(New Func(Of StanMostkaGalerii)(AddressOf PobierzStanDlaGalerii)), StanMostkaGalerii)
        End If

        Dim punkt As PunktXY, piksel As Double
        ParametryProbki(punkt, piksel)
        Dim geo = Ustawienia.Uklad.DoWgs84(punkt)
        Dim pikselMetry = If(Ustawienia.Uklad.Geograficzny,
                             Ustawienia.Uklad.PrzeliczRozmiarPiksela(UkladWspolrzednych.PL1992, piksel, geo),
                             piksel)

        Dim obszar As New DaneObszaruGalerii With {
            .Lat = Math.Round(geo.Szerokosc, 5),
            .Lon = Math.Round(geo.Dlugosc, 5),
            .Piksel = Math.Round(If(pikselMetry > 0, pikselMetry, 1.0), 3),
            .Uklad = "EPSG:" & Ustawienia.Uklad.Epsg
        }

        Dim s = BiezacaSiatka()
        If s.Poprawna Then
            obszar.MaSiatke = True
            Dim sw = Ustawienia.Uklad.DoWgs84(s.Obszar.XDol, s.Obszar.YLewy)
            Dim ne = Ustawienia.Uklad.DoWgs84(s.Obszar.XGora, s.Obszar.YPrawy)
            obszar.BboxWgs84 = {
                Math.Round(Math.Min(sw.Dlugosc, ne.Dlugosc), 5),
                Math.Round(Math.Min(sw.Szerokosc, ne.Szerokosc), 5),
                Math.Round(Math.Max(sw.Dlugosc, ne.Dlugosc), 5),
                Math.Round(Math.Max(sw.Szerokosc, ne.Szerokosc), 5)
            }
        End If

        Return New StanMostkaGalerii With {
            .Ok = True,
            .Zestaw = PrzekazZestawu.ZZestawuWarstw(_zestaw, OpisWarstw()),
            .Obszar = obszar
        }
    End Function

    Private Function ObsluzZestawZGalerii(dane As DaneZestawuGalerii) As WynikMostkaGalerii
        If Me.IsDisposed Then Return WynikMostkaGalerii.Blad("blad", "Okno programu zostało zamknięte.")
        If Me.InvokeRequired Then
            Return DirectCast(Me.Invoke(New Func(Of DaneZestawuGalerii, WynikMostkaGalerii)(AddressOf ObsluzZestawZGalerii), dane), WynikMostkaGalerii)
        End If
        If _przerwanie IsNot Nothing Then
            Return WynikMostkaGalerii.Blad("zajety", "Trwa pobieranie mapy - zatrzymaj je przed zmianą listy warstw.")
        End If

        Dim wynik = ZastosujPrzepisZestawu(dane, "z galerii")
        If wynik.Ok Then PrzywrocOknoNaWierzch()
        Return wynik
    End Function

    Private Function ZastosujPrzepisZestawu(dane As DaneZestawuGalerii, Optional zrodloOpis As String = "z galerii") As WynikMostkaGalerii
        If dane Is Nothing Then Return WynikMostkaGalerii.Blad("blad_parametrow", "Pusty przepis kanapki.")
        Dim wynikBudowy = PrzekazZestawu.ZbudujZestaw(dane, ZrodlaMapSesji(), _zestaw)
        If wynikBudowy.Zestaw.Liczba = 0 Then
            Dim szczegoly = If(wynikBudowy.Pominiete.Count > 0, " (" & String.Join(", ", wynikBudowy.Pominiete) & ")", "")
            Return WynikMostkaGalerii.Blad("nie_znaleziono", "Nie rozpoznano żadnej warstwy z przepisu" & szczegoly & ".")
        End If

        Dim opisPiksela As String = ""
        If wynikBudowy.Cel <> "kreator" Then
            Dim bylaPusta As Boolean = _zestaw.Liczba = 0
            _zestaw = wynikBudowy.Zestaw
            PoZmianieWarstw(Math.Max(0, _zestaw.Liczba - 1))
            opisPiksela = DobierzRozmiarPiksela(bylaPusta)
            DobierzBokSegmentu(bylaPusta)
        End If

        Dim zapisanyStyl As SzablonMapy = Nothing
        If wynikBudowy.ZapiszStyl OrElse wynikBudowy.Cel = "kreator" Then
            zapisanyStyl = ZapiszStylZPrzepisu(wynikBudowy.Zestaw, wynikBudowy.Nazwa)
        End If

        Dim czesci As New List(Of String)()
        If wynikBudowy.Cel = "kreator" Then
            czesci.Add("Zapisano kanapkę jako styl kreatora" & If(zapisanyStyl IsNot Nothing, " '" & zapisanyStyl.Nazwa & "'", "") & " (" & wynikBudowy.Zestaw.Liczba & " warstw)")
        Else
            czesci.Add("Wczytano kanapkę " & zrodloOpis & " (" & wynikBudowy.Zestaw.Liczba & " warstw)")
            If zapisanyStyl IsNot Nothing Then czesci.Add("zapisano też styl '" & zapisanyStyl.Nazwa & "'")
        End If
        If opisPiksela <> "" Then czesci.Add(opisPiksela)
        If wynikBudowy.Pominiete.Count > 0 Then czesci.Add("pominięto: " & String.Join(", ", wynikBudowy.Pominiete))

        Dim przykryta = wynikBudowy.Zestaw.ZnajdzPrzykrytaNakladke()
        Dim komunikatKoncowy = String.Join(". ", czesci) & "."
        If przykryta IsNot Nothing Then
            komunikatKoncowy &= " Uwaga: nakładka " & NazwaWarstwy(przykryta) & " może zostać zasłonięta przez warstwę nad nią."
            Komunikat(komunikatKoncowy, Color.DarkOrange)
        Else
            Komunikat(komunikatKoncowy, If(wynikBudowy.Pominiete.Count > 0, Color.DarkOrange, Color.Green))
        End If

        If wynikBudowy.Cel = "kreator" AndAlso zapisanyStyl IsNot Nothing Then
            FormKreatorMapy.WybierzSzablonNaStarcie(zapisanyStyl.Id)
            OtworzKreatorMapy()
        End If

        Return WynikMostkaGalerii.Ustawiono(komunikatKoncowy, wynikBudowy.Zestaw.Liczba, wynikBudowy.Pominiete)
    End Function

    Private Function ZapiszStylZPrzepisu(zestaw As ZestawWarstw, nazwa As String) As SzablonMapy
        Dim punkt As PunktXY, piksel As Double
        ParametryProbki(punkt, piksel)
        Dim geo = Ustawienia.Uklad.DoWgs84(punkt)
        Dim pikselMetry = If(Ustawienia.Uklad.Geograficzny,
                             Ustawienia.Uklad.PrzeliczRozmiarPiksela(UkladWspolrzednych.PL1992, piksel, geo),
                             piksel)

        Dim styl = PrzekazZestawu.UtworzStylSzablonu(zestaw, nazwa, geo.Szerokosc, geo.Dlugosc, pikselMetry)
        If styl Is Nothing Then Return Nothing

        Dim sciezka = Path.Combine(If(String.IsNullOrEmpty(FolderDanych), AppDomain.CurrentDomain.BaseDirectory, FolderDanych), SzablonyWlasne.NazwaPliku)
        Dim lista As List(Of SzablonMapy)
        Try
            lista = SzablonyWlasne.Wczytaj(sciezka)
        Catch
            lista = ZarzadcaSzablonow.PobierzWlasne().ToList()
        End Try

        Dim istniejacy = lista.FindIndex(Function(x) String.Equals(x.Nazwa, styl.Nazwa, StringComparison.CurrentCultureIgnoreCase))
        If istniejacy >= 0 Then
            lista(istniejacy) = styl
        Else
            lista.Add(styl)
        End If

        SzablonyWlasne.Zapisz(sciezka, lista)
        ZarzadcaSzablonow.UstawWlasne(lista)
        Return styl
    End Function

    Private Async Function ObsluzPodgladZGaleriiAsync(zadanie As ZadaniePodgladuGalerii, ct As CancellationToken) As Task(Of WynikPodgladuGalerii)
        If Me.IsDisposed Then Return WynikPodgladuGalerii.Blad("Okno programu zostało zamknięte.")

        Await _semaforPodgladuGalerii.WaitAsync(ct).ConfigureAwait(False)
        Try
            ct.ThrowIfCancellationRequested()

            Dim stanMigawki = CType(Me.Invoke(New Func(Of (
                Zrodla As Dictionary(Of String, PlikWarstw),
                Uklad As UkladWspolrzednych,
                DomyslnyPunkt As PunktXY,
                DomyslnyPiksel As Double,
                Format As String,
                LimitCzasu As Integer,
                IloscProb As Integer,
                JakoscJpegWmts As Integer
            ))(Function()
                   Dim p As PunktXY, px As Double
                   ParametryProbki(p, px)
                   Return (ZrodlaMapSesji(), Ustawienia.Uklad, p, px, Ustawienia.Format,
                           Ustawienia.LimitCzasuSekundy, Ustawienia.IloscProbPobrania, Ustawienia.JakoscJpegWmts)
               End Function)), (
                Zrodla As Dictionary(Of String, PlikWarstw),
                Uklad As UkladWspolrzednych,
                DomyslnyPunkt As PunktXY,
                DomyslnyPiksel As Double,
                Format As String,
                LimitCzasu As Integer,
                IloscProb As Integer,
                JakoscJpegWmts As Integer
            ))

            Dim wynikZestawu = PrzekazZestawu.ZbudujZestaw(
                New DaneZestawuGalerii With {.Warstwy = zadanie.Warstwy},
                stanMigawki.Zrodla)
            If wynikZestawu.Zestaw.Liczba = 0 Then
                Return WynikPodgladuGalerii.Blad("Nie rozpoznano żadnej warstwy do podglądu.")
            End If

            Dim uklad = stanMigawki.Uklad
            Dim punkt As PunktXY
            Dim latOut As Double, lonOut As Double
            If zadanie.Lat.HasValue AndAlso zadanie.Lon.HasValue Then
                latOut = zadanie.Lat.Value
                lonOut = zadanie.Lon.Value
                punkt = uklad.ZWgs84(latOut, lonOut)
            Else
                punkt = stanMigawki.DomyslnyPunkt
                Dim geo = uklad.DoWgs84(punkt)
                latOut = geo.Szerokosc
                lonOut = geo.Dlugosc
            End If

            Dim najdokladniejsza = wynikZestawu.Zestaw.NajdokladniejszaWarstwa()
            Dim pikselMetry As Double
            If zadanie.Piksel.HasValue AndAlso zadanie.Piksel.Value > 0 Then
                pikselMetry = zadanie.Piksel.Value
            ElseIf najdokladniejsza IsNot Nothing AndAlso najdokladniejsza.ZalecanyPiksel > 0 Then
                pikselMetry = najdokladniejsza.ZalecanyPiksel
            Else
                pikselMetry = If(uklad.Geograficzny,
                                 uklad.PrzeliczRozmiarPiksela(UkladWspolrzednych.PL1992, stanMigawki.DomyslnyPiksel, New PunktGeo(latOut, lonOut)),
                                 stanMigawki.DomyslnyPiksel)
            End If
            If pikselMetry <= 0 Then pikselMetry = 2.0

            Dim pikselUkladu = If(uklad.Geograficzny,
                                  UkladWspolrzednych.PL1992.PrzeliczRozmiarPiksela(uklad, pikselMetry, New PunktGeo(latOut, lonOut)),
                                  pikselMetry)

            If _silnikPodgladuGalerii Is Nothing Then
                _silnikPodgladuGalerii = New PodgladWarstwSilnik() With {.Bok = 360}
            End If
            _silnikPodgladuGalerii.SprawdzIZaktualizujParametry(punkt, pikselUkladu, uklad)

            Dim pol = _silnikPodgladuGalerii.Bok * pikselUkladu / 2.0
            Dim zasiegProbki As New Zasieg(punkt.X - pol, punkt.Y - pol, punkt.X + pol, punkt.Y + pol)
            Dim siatkaProbki As New Siatka(zasiegProbki, pikselUkladu, _silnikPodgladuGalerii.Bok)

            Dim zadaniePob As New ZadaniePobierania With {
                .Uklad = uklad,
                .Siatka = siatkaProbki,
                .Zbiory = wynikZestawu.Zestaw.Zbiory(),
                .ZrodlaMap = stanMigawki.Zrodla,
                .Format = stanMigawki.Format,
                .LimitCzasuSekundy = Math.Max(5, stanMigawki.LimitCzasu),
                .IloscProb = Math.Max(1, stanMigawki.IloscProb),
                .JakoscJpegWmts = stanMigawki.JakoscJpegWmts,
                .Przezroczystosc = True,
                .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_galeria_podglad") & Path.DirectorySeparatorChar
            }

            Dim wynik = Await _silnikPodgladuGalerii.PobierzPodgladAsync(zadaniePob, wynikZestawu.Zestaw, ct).ConfigureAwait(False)
            ct.ThrowIfCancellationRequested()

            Using bmp = wynik.Item1
                If bmp Is Nothing Then
                    Dim blad = If(wynik.Item2 IsNot Nothing AndAlso wynik.Item2.Count > 0,
                                  String.Join("; ", wynik.Item2),
                                  "Serwer mapowy nie zwrócił obrazu dla tego obszaru.")
                    Return WynikPodgladuGalerii.Blad(blad)
                End If
                Using ms As New MemoryStream()
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
                    Dim brakujace = _silnikPodgladuGalerii.BrakujaceWarstwy(wynikZestawu.Zestaw).
                        Select(Function(w) w.Zbior & "/" & w.Nazwa).
                        Concat(wynikZestawu.Pominiete).
                        ToList()
                    Return WynikPodgladuGalerii.Sukces(ms.ToArray(), latOut, lonOut, pikselMetry, brakujace)
                End Using
            End Using
        Finally
            _semaforPodgladuGalerii.Release()
        End Try
    End Function

    Private Sub KopiujKanapkeDoSchowka()
        If _zestaw.Liczba = 0 Then Exit Sub
        Try
            Dim tekst = PrzekazZestawu.DoTekstu(PrzekazZestawu.ZZestawuWarstw(_zestaw, OpisWarstw()))
            Clipboard.SetText(tekst)
            Komunikat("Skopiowano przepis kanapki (" & _zestaw.Liczba & " warstw) do schowka.", Color.Green)
        Catch ex As Exception
            MsgBox("Nie udało się skopiować kanapki do schowka: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Kanapka warstw")
        End Try
    End Sub

    Private Sub WklejKanapkeZeSchowka()
        If PobieranieTrwa("Wklejenie kanapki warstw") Then Exit Sub
        Try
            If Not Clipboard.ContainsText() Then
                MsgBox("W schowku nie ma tekstu z przepisem kanapki.", MsgBoxStyle.Information, "Kanapka warstw")
                Exit Sub
            End If
            Dim dane = PrzekazZestawu.ZTekstu(Clipboard.GetText())
            Dim wynik = ZastosujPrzepisZestawu(dane, "ze schowka")
            If Not wynik.Ok Then MsgBox(wynik.Komunikat, MsgBoxStyle.Exclamation, "Kanapka warstw")
        Catch ex As Exception
            MsgBox("Nie udało się wkleić kanapki ze schowka: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Kanapka warstw")
        End Try
    End Sub

    Private Sub WczytajKanapkeZPliku()
        If PobieranieTrwa("Wczytanie kanapki warstw") Then Exit Sub
        Using dlg As New OpenFileDialog With {
            .Title = "Wczytaj kanapkę warstw z galerii",
            .Filter = "Przepis kanapki MapoTero (*.mapotero;*.json)|*.mapotero;*.json|Wszystkie pliki (*.*)|*.*"
        }
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
            Try
                Dim dane = PrzekazZestawu.ZTekstu(File.ReadAllText(dlg.FileName, System.Text.Encoding.UTF8))
                Dim wynik = ZastosujPrzepisZestawu(dane, "z pliku " & Path.GetFileName(dlg.FileName))
                If Not wynik.Ok Then MsgBox(wynik.Komunikat, MsgBoxStyle.Exclamation, "Kanapka warstw")
            Catch ex As Exception
                MsgBox("Nie udało się wczytać pliku kanapki: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Kanapka warstw")
            End Try
        End Using
    End Sub

    Private Sub PrzywrocOknoNaWierzch()
        If Me.WindowState = FormWindowState.Minimized Then Me.WindowState = FormWindowState.Normal
        Me.Activate()
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
            MsgBox("Nie można otworzyć strony pomocy: " & OpisBledu.Z(ex), MsgBoxStyle.Exclamation, "Pomoc")
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.ShowDialog()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        OtworzStrone(AdresForumPfe, "Zadaj pytanie na forum")
    End Sub

    Private Sub InstrukcjaObslugiToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_Obslugi.ShowDialog()
    End Sub

    Private Sub ZglosBladToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZglosBladToolStripMenuItem.Click
        OtworzStrone(AdresZgloszenBledow, "Zgłoś błąd lub pomysł")
    End Sub

    ''' <summary>
    ''' Folder danych bywa w %LocalAppData%\MapoTero (program w Program Files), więc użytkownik nie znajduje
    ''' lastsettings.txt, pobranych map ani awarie.log obok exe.
    ''' </summary>
    Private Sub FolderDanychToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FolderDanychToolStripMenuItem.Click
        OtworzFolder(FolderDanych)
    End Sub

    Private Sub CoNowegoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CoNowegoToolStripMenuItem.Click
        OtworzStrone(AdresWydan, "Co nowego w programie")
    End Sub

    Private Sub WesprzyjToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WesprzyjToolStripMenuItem.Click
        OtworzStrone(AdresWsparcia, "Wesprzyj rozwój programu")
    End Sub

    Private Sub KorektaFolderuToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KorektaFolderuToolStripMenuItem.Click
        Using dlg As New FormKorektaFolderu()
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub PrzeslijNaTelefonToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PrzeslijNaTelefonToolStripMenuItem.Click
        Using ofd As New OpenFileDialog()
            ofd.Title = "Wybierz mapę do przesłania na telefon (Wi-Fi)"
            ofd.Filter = "Mapy mobilne (*.mbtiles;*.gpkg;*.tif;*.kmz;*.sqlitedb;*.tar)|*.mbtiles;*.gpkg;*.tif;*.kmz;*.sqlitedb;*.tar|Wszystkie pliki (*.*)|*.*"
            ofd.FilterIndex = 1
            ofd.RestoreDirectory = True
            If Not String.IsNullOrEmpty(FormEksport.OstatniPlik) AndAlso File.Exists(FormEksport.OstatniPlik) Then
                ofd.InitialDirectory = Path.GetDirectoryName(FormEksport.OstatniPlik)
                ofd.FileName = Path.GetFileName(FormEksport.OstatniPlik)
            ElseIf Not String.IsNullOrEmpty(Program.FolderDownload) AndAlso Directory.Exists(Program.FolderDownload) Then
                ofd.InitialDirectory = Program.FolderDownload
            ElseIf Not String.IsNullOrEmpty(Program.FolderDanych) AndAlso Directory.Exists(Program.FolderDanych) Then
                ofd.InitialDirectory = Program.FolderDanych
            End If

            If ofd.ShowDialog(Me) = DialogResult.OK AndAlso File.Exists(ofd.FileName) Then
                FormWyslijNaTelefon.PokazDlaPliku(Me, ofd.FileName)
            End If
        End Using
    End Sub

    Private Sub KopiujNaGarminaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KopiujNaGarminaToolStripMenuItem.Click
        Using ofd As New OpenFileDialog()
            ofd.Title = "Wybierz mapę do skopiowania na Garmina (USB)"
            ofd.Filter = "Mapy Garmin (*.kmz;*.jnx)|*.kmz;*.jnx|Wszystkie pliki (*.*)|*.*"
            ofd.FilterIndex = 1
            ofd.RestoreDirectory = True
            If Not String.IsNullOrEmpty(FormEksport.OstatniPlik) AndAlso File.Exists(FormEksport.OstatniPlik) Then
                Dim ext = Path.GetExtension(FormEksport.OstatniPlik).ToLowerInvariant()
                If ext = ".kmz" OrElse ext = ".jnx" Then
                    ofd.InitialDirectory = Path.GetDirectoryName(FormEksport.OstatniPlik)
                    ofd.FileName = Path.GetFileName(FormEksport.OstatniPlik)
                End If
            ElseIf Not String.IsNullOrEmpty(Program.FolderDownload) AndAlso Directory.Exists(Program.FolderDownload) Then
                ofd.InitialDirectory = Program.FolderDownload
            End If

            If ofd.ShowDialog(Me) = DialogResult.OK AndAlso File.Exists(ofd.FileName) Then
                FormKopiujNaGarmina.PokazDlaPliku(Me, ofd.FileName)
            End If
        End Using
    End Sub

    Private Sub ZnajdzDzialkeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZnajdzDzialkeToolStripMenuItem.Click
        OtworzWyszukiwarke()
    End Sub

    Private Sub btnSzukajDzialki_Click(sender As Object, e As EventArgs) Handles btnSzukajDzialki.Click
        OtworzWyszukiwarke()
    End Sub

    Private Sub KonwertujOziToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KonwertujOziToolStripMenuItem.Click
        Using frm As New FormImportOzi()
            frm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub KonwertujGeoTiffToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KonwertujGeoTiffToolStripMenuItem.Click
        Using frm As New FormKonwerterGeoTiff()
            frm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub ProfilNmtToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ProfilNmtToolStripMenuItem.Click
        Using frm As New FormProfilNmt()
            frm.ShowDialog(Me)
        End Using
    End Sub

#Region "Nowa sesja"

    ''' <summary>Przycisk „nowa sesja” obok przycisków folderu, siatki, scalania i usuwania (tworzony w kodzie).</summary>
    Private _btnNowaSesja As Button

    ''' <summary>Pozycja „Nowa sesja” w menu Sesja pobierania (tworzona w kodzie).</summary>
    Private _menuNowaSesja As ToolStripMenuItem

    ''' <summary>
    ''' Przycisk i pozycja menu „Nowa sesja”. Przycisk stoi w trzeciej kolumnie górnego rzędu siatki przycisków
    ''' (obok przycisku siatki, a nie obok „Usuń pobrane” - oba „czyszczą”, więc sąsiedztwo sprzyjałoby pomyłce);
    ''' położenie liczone jest z przeskalowanych już przycisków, więc zgadza się przy każdej czcionce i skalowaniu.
    ''' </summary>
    Private Sub InicjalizujNowaSesje()
        'kolumna przycisku scalania pod spodem - liczona z odstępu przycisków wychodziła o 1 px obok po skalowaniu
        _btnNowaSesja = New Button With {
            .Name = "btnNowaSesja", .Size = btnZnaczniki.Size, .Location = New Point(btnScalanie.Left, btnZnaczniki.Top),
            .Anchor = btnZnaczniki.Anchor, .Image = UtworzIkoneNowejSesji(), .ImageAlign = ContentAlignment.MiddleCenter,
            .UseVisualStyleBackColor = True, .TabIndex = btnZnaczniki.TabIndex}
        btnZnaczniki.Parent.Controls.Add(_btnNowaSesja)
        _btnNowaSesja.BringToFront()
        ToolTip1.SetToolTip(_btnNowaSesja, "Nowa sesja - wyczyść warstwy, zasięg i widok mapy (pobrane pliki zostają)")
        AddHandler _btnNowaSesja.Click, Sub() NowaSesja(pytaj:=True)

        _menuNowaSesja = New ToolStripMenuItem("Nowa sesja") With {.Name = "NowaSesjaToolStripMenuItem", .Image = UtworzIkoneNowejSesji()}
        AddHandler _menuNowaSesja.Click, Sub() NowaSesja(pytaj:=True)
        SesjaToolStripMenuItem.DropDownItems.Insert(0, _menuNowaSesja)
        SesjaToolStripMenuItem.DropDownItems.Insert(1, New ToolStripSeparator())
    End Sub

    ''' <summary>Ikona nowej sesji: pusta kartka z zagiętym rogiem i zielonym plusem (rysowana jak lupa wyszukiwarki).</summary>
    Public Shared Function UtworzIkoneNowejSesji(Optional rozmiar As Integer = 16) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.Clear(Color.Transparent)
            Dim kartka() As PointF = {New PointF(2.5!, 1.5!), New PointF(9.5!, 1.5!), New PointF(12.5!, 4.5!),
                                      New PointF(12.5!, 14.5!), New PointF(2.5!, 14.5!)}
            Using pedzel As New SolidBrush(Color.White)
                g.FillPolygon(pedzel, kartka)
            End Using
            Using pioro As New Pen(Color.FromArgb(70, 70, 70), 1.0!)
                g.DrawPolygon(pioro, kartka)
                g.DrawLines(pioro, {New PointF(9.5!, 1.5!), New PointF(9.5!, 4.5!), New PointF(12.5!, 4.5!)})
            End Using
            Using pedzel As New SolidBrush(Color.FromArgb(0, 150, 60))
                g.FillEllipse(pedzel, 8.0!, 8.0!, 8.0!, 8.0!)
            End Using
            Using pioro As New Pen(Color.White, 1.6!)
                g.DrawLine(pioro, 12.0!, 9.8!, 12.0!, 14.2!)
                g.DrawLine(pioro, 9.8!, 12.0!, 14.2!, 12.0!)
            End Using
        End Using
        Return bmp
    End Function

    ''' <summary>
    ''' Nowa sesja: czyści wybrane warstwy, zasięg, zaznaczenie i siatkę na mapie, obszar wzdłuż trasy i kontur działki,
    ''' przywraca domyślny bok segmentu i rozmiar piksela (dobierany ponownie wg pierwszej wybranej warstwy), układ
    ''' domyślny z ustawień, przedrostek, format i numerację nowej sesji z lastsettings.txt, zwija drzewo warstw
    ''' z rozwiniętymi skanami map topograficznych GUGiK i pokazuje całą Polskę. Ustawienia programu zostają, pliki
    ''' na dysku też - folder sesji wraca do download, więc następne pobieranie utworzy nowy podfolder.
    ''' </summary>
    ''' <param name="pytaj">Pytanie o potwierdzenie, gdy są wybrane warstwy lub zasięg (testy wywołują bez pytania).</param>
    Friend Sub NowaSesja(pytaj As Boolean)
        If PobieranieTrwa("Rozpoczęcie nowej sesji") Then Exit Sub
        If pytaj AndAlso (_zestaw.Liczba > 0 OrElse BiezacaSiatka().Poprawna OrElse _korytarz IsNot Nothing) Then
            If MessageBox.Show("Rozpocząć nową sesję?" & vbCrLf & vbCrLf &
                               "Wybrane warstwy, zasięg pobierania, obszar wzdłuż trasy i widok mapy zostaną wyczyszczone. " &
                               "Pobrane pliki zostają na dysku - do pobranej lub zapisanej sesji wrócisz poleceniem Sesja pobierania > Wczytaj.",
                               "Nowa sesja", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        End If

        ZakonczWyborNaroznikow()

        _ostatniWynikPobierania = Nothing
        WyzerujLicznikBledow()
        _ctsPodglad?.Cancel()
        If _przeciaganyUchwyt IsNot Nothing Then ZakonczPrzeciaganie(anuluj:=True)

        'parametry sesji z lastsettings.txt (jak przy starcie bez sesji) - przedrostek, format, przezroczystość, numeracja
        If File.Exists(PlikLastsettings) Then
            Try
                'tylko parametry sesji - pełny odczyt cofał ustawienia programu zmienione od uruchomienia
                Ustawienia.WczytajParametryNowejSesji(PlikLastsettings)
            Catch
            End Try
        End If
        If Ustawienia.Format = "" Then Ustawienia.Format = "jpeg"
        'nowa sesja ma domyślny bok segmentu (niżej), więc nie może zostać w trybie TrekBuddy (segmenty 512 px)
        Ustawienia.TrekBuddy = False
        Ustawienia.BokSegmentuPrzedTrekBuddy = ""
        stFormat.Text = "." & Ustawienia.Format
        FolderSesji = FolderDownload
        _numeracjaSesji = ""
        _dataPobrania = ""
        btnScalanie.Enabled = False

        'obszar: trasa, działka, zaznaczenie i siatka na mapie, pola zasięgu
        _korytarz = Nothing
        UsunKorytarzToolStripMenuItem.Enabled = False
        _dzialkaKatastralna = Nothing
        WyczyscZnaczniki()
        mapa.SelectedArea = RectLatLng.Empty
        _punktProbkiBezSiatki = Nothing
        WpiszPolaZasiegu("", "", "", "")
        For Each l In New Label() {lblZaznSzerGora, lblZaznSzerDol, lblZaznDlugLewa, lblZaznDlugPrawa}
            l.Text = "."
        Next
        PrzelaczUklad(Ustawienia.UkladDomyslny, False)
        txtBokSegmentu.Text = "2000"
        'piksel wpisany „przez program” - po wybraniu warstwy zastąpi go zalecany rozmiar piksela tej warstwy
        txtRozmiarPiksela.Text = "1"
        _automatycznyPikselTekst = "1"
        _automatycznyPikselMetry = 1

        'warstwy i drzewo: puste kanapka i wyszukiwanie, zwinięte grupy, rozwinięte skany map topograficznych GUGiK
        _zestaw.Wyczysc()
        _biezacyZbior = If(ZnajdzZbior(DomyslnyZbiorMap) IsNot Nothing, DomyslnyZbiorMap, _biezacyZbior)
        _rozwinieteZbiory.Clear()
        txtSzukajWarstwy.Clear()
        BudujDrzewo()
        RozwinZbioryWybranych(_biezacyZbior)
        PoZmianieWarstw(-1)

        'widok całej Polski
        mapa.Position = New PointLatLng(52.3, 19.2)
        mapa.Zoom = 6
        ZapamietajPolozenieMapy()
        PrzeliczSiatke()
        Komunikat("Rozpoczęto nową sesję. Wybierz mapę w drzewie warstw i zaznacz obszar prawym przyciskiem myszy; " &
                  "pobieranie utworzy nowy podfolder sesji w folderze download.", Color.Green)
    End Sub

#End Region

    Private Sub InicjalizujPrzyciskiMapy()
        InicjalizujPrzyciskSzukaj()
    End Sub

    Private Sub InicjalizujPrzyciskSzukaj()
        Try
            ' Estetyczny odstęp (12 px) pod blokiem zoomu, aby lupa stanowiła osobne narzędzie i nie zlewała się z przyciskiem pomniejszania
            btnSzukajDzialki.Left = btnPomniejsz.Left
            btnSzukajDzialki.Top = btnPomniejsz.Bottom + 12
            btnSzukajDzialki.Text = ""
            btnSzukajDzialki.Image = UtworzIkoneLupy()
            btnSzukajDzialki.ImageAlign = ContentAlignment.MiddleCenter
            ToolTip1.SetToolTip(btnSzukajDzialki, "Wyszukaj miejscowość, adres lub działkę katastralną (Ctrl+F - pole wyszukiwania, Ctrl+D - działka)")
        Catch
        End Try
    End Sub

    ''' <summary>Rysuje ostrą, wektorową ikonę lupy dla przycisku wyszukiwarki na pasku mapy.</summary>
    Public Shared Function UtworzIkoneLupy(Optional rozmiar As Integer = 16) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.Clear(Color.Transparent)

            ' Lekki błękit wewnątrz soczewki
            Using pedzelSzkla As New SolidBrush(Color.FromArgb(40, 0, 120, 215))
                g.FillEllipse(pedzelSzkla, 2.0!, 2.0!, 8.0!, 8.0!)
            End Using

            ' Metalowa obręcz soczewki
            Using pioroRamy As New Pen(Color.FromArgb(50, 50, 50), 1.6!)
                g.DrawEllipse(pioroRamy, 2.0!, 2.0!, 8.0!, 8.0!)
            End Using

            ' Błysk światła na szkle
            Using pioroBlysk As New Pen(Color.FromArgb(200, 255, 255, 255), 1.0!)
                g.DrawArc(pioroBlysk, 3.5!, 3.5!, 5.0!, 5.0!, 190.0!, 80.0!)
            End Using

            ' Rączka lupy
            Using pioroRaczki As New Pen(Color.FromArgb(50, 50, 50), 2.2!)
                pioroRaczki.StartCap = Drawing2D.LineCap.Round
                pioroRaczki.EndCap = Drawing2D.LineCap.Round
                g.DrawLine(pioroRaczki, 9.0!, 9.0!, 13.5!, 13.5!)
            End Using
        End Using
        Return bmp
    End Function

    Private Sub OtworzWyszukiwarke(Optional tryb As Integer = -1, Optional fraza As String = "")
        If PobieranieTrwa("Wyszukiwanie") Then Exit Sub
        Using frm As New FormSzukajDzialki(tryb, srodekLat:=mapa.Position.Lat, srodekLon:=mapa.Position.Lng)
            frm.PoczatkowaFraza = fraza
            If frm.ShowDialog(Me) = DialogResult.OK Then
                If frm.WybranaDzialka IsNot Nothing Then
                    UstawDzialke(frm.WybranaDzialka, frm.MarginesMetry, frm.WlaczWarstwy)
                ElseIf frm.WybraneMiejsce IsNot Nothing Then
                    UstawMiejsce(frm.WybraneMiejsce)
                End If
            End If
        End Using
    End Sub

    Private Sub UstawMiejsce(m As WynikGeokodowania)
        If m Is Nothing Then Exit Sub
        If m.MaObszar Then
            Dim dLat = Math.Max(0.001, m.MaxSzerokosc - m.MinSzerokosc)
            Dim dLon = Math.Max(0.001, m.MaxDlugosc - m.MinDlugosc)
            mapa.SetZoomToFitRect(RectLatLng.FromLTRB(m.MinDlugosc - dLon * 0.1, m.MaxSzerokosc + dLat * 0.1,
                                                     m.MaxDlugosc + dLon * 0.1, m.MinSzerokosc - dLat * 0.1))
        Else
            mapa.Position = New PointLatLng(m.Szerokosc, m.Dlugosc)
            If mapa.Zoom < 14 Then
                mapa.Zoom = 15
            End If
        End If
        Komunikat("Przejście do: " & m.KrotkaNazwa & " (" & m.WspolrzedneTekst & ")", Color.Green)
    End Sub

    Private Sub UstawDzialke(d As DzialkaKatastralna, margines As Double, wlaczWarstwy As Boolean)
        _korytarz = Nothing
        UsunKorytarzToolStripMenuItem.Enabled = False
        _dzialkaKatastralna = d
        Dim z1992 = d.Zasieg1992
        Dim zRozszerzony = New Zasieg(z1992.XDol - margines, z1992.YLewy - margines, z1992.XGora + margines, z1992.YPrawy + margines)
        Dim zDocelowy = UkladWspolrzednych.PL1992.PrzeliczZasieg(Ustawienia.Uklad, zRozszerzony)
        Dim z = ZaokraglonyZasieg(zDocelowy)

        WpiszPolaZasiegu(Liczba(z.XDol), Liczba(z.YLewy), Liczba(z.XGora), Liczba(z.YPrawy))

        WyczyscZnaczniki()
        PokazZnaczniki()

        Dim wszystkie = If(d.ObrysyWgs84.Count > 0, d.ObrysyWgs84.SelectMany(Function(o) o).ToList(), d.ObrysWgs84)
        If wszystkie.Count > 0 Then
            Dim minLat = wszystkie.Min(Function(p) p.Szerokosc)
            Dim maxLat = wszystkie.Max(Function(p) p.Szerokosc)
            Dim minLon = wszystkie.Min(Function(p) p.Dlugosc)
            Dim maxLon = wszystkie.Max(Function(p) p.Dlugosc)
            Dim dLat = Math.Max(0.0005, maxLat - minLat)
            Dim dLon = Math.Max(0.0005, maxLon - minLon)
            mapa.SetZoomToFitRect(New RectLatLng(maxLat + dLat * 0.3, minLon - dLon * 0.3, dLon * 1.6, dLat * 1.6))
        End If

        If wlaczWarstwy Then
            WlaczWarstwyDzialek()
        End If

        Komunikat("Ustawiono obszar pobierania dla: " & d.ToString(), Color.Green)
    End Sub

    ''' <summary>
    ''' Podkład dla działki: jedna ortofotomapa (standardowa WMTS, a gdy jej brak - pierwsza polska ortofotomapa)
    ''' oraz granice i numery działek EGiB. Wcześniej warunek „nazwa zawiera ortofoto/dzialki” zaznaczał 11 warstw,
    ''' w tym czeskie ortofoto, cztery polskie ortofotomapy, użytki i kontury klasyfikacyjne.
    ''' </summary>
    Private Sub WlaczWarstwyDzialek()
        Dim wezly = tvWarstwy.WszystkieWezly().
            Select(Function(w) New With {.Wezel = w, .Item = TryCast(w.Tag, WarstwaWDrzewie)}).
            Where(Function(x) x.Item IsNot Nothing AndAlso x.Item.Zbior IsNot Nothing).ToList()
        Dim orto = wezly.FirstOrDefault(Function(x) String.Equals(x.Item.Zbior.Nazwa, "ortofotomapa_WMTS", StringComparison.OrdinalIgnoreCase))
        If orto Is Nothing Then
            orto = wezly.FirstOrDefault(Function(x) x.Item.Zbior.Nazwa.StartsWith("ortofotomapa", StringComparison.OrdinalIgnoreCase))
        End If
        Dim doWlaczenia As New List(Of TreeNode)
        If orto IsNot Nothing Then doWlaczenia.Add(orto.Wezel)
        For Each warstwaEgib In {"dzialki", "numery_dzialek"}
            Dim egib = wezly.FirstOrDefault(Function(x) String.Equals(x.Item.Zbior.Nazwa, "dzialki_ewid_EGIB", StringComparison.OrdinalIgnoreCase) AndAlso
                                                        String.Equals(x.Item.Warstwa, warstwaEgib, StringComparison.OrdinalIgnoreCase))
            If egib IsNot Nothing Then doWlaczenia.Add(egib.Wezel)
        Next
        For Each w In doWlaczenia
            If Not w.Checked Then w.Checked = True
        Next
    End Sub

    Private Sub PokazDzialke()
        If _dzialkaKatastralna Is Nothing OrElse _dzialkaKatastralna.ObrysWgs84.Count = 0 Then Exit Sub
        Dim warstwa As New GMapOverlay("dzialka")
        mapa.Overlays.Add(warstwa)
        'działka wieloczęściowa (MULTIPOLYGON) - każda część osobnym wielokątem
        Dim obrysy = If(_dzialkaKatastralna.ObrysyWgs84.Count > 0, _dzialkaKatastralna.ObrysyWgs84,
                        New List(Of List(Of PunktGeo)) From {_dzialkaKatastralna.ObrysWgs84})
        For Each obrys In obrysy
            Dim punkty As New List(Of PointLatLng)()
            For Each p In obrys
                punkty.Add(New PointLatLng(p.Szerokosc, p.Dlugosc))
            Next
            warstwa.Polygons.Add(New GMapPolygon(punkty, "obrys_dzialki") With {
                .Fill = _pedzelDzialki,
                .Stroke = _pioroDzialki
            })
        Next
    End Sub

#End Region

#Region "Kreator mapy"

    Private _btnKreator As Button
    Private _menuKreator As ToolStripMenuItem

    ''' <summary>
    ''' Przycisk i pozycja menu kreatora mapy. Przycisk stoi w lewym górnym rogu siatki przycisków, bo od kreatora
    ''' zaczyna się pracę; górny rząd to przygotowanie mapy (kreator, siatka, nowa sesja), dolny - pobrane pliki
    ''' (folder, usuwanie, scalanie). Położenie liczone z przeskalowanych już przycisków.
    ''' </summary>
    Private Sub InicjalizujKreator()
        _btnKreator = New Button With {
            .Name = "btnKreator", .Size = btnZnaczniki.Size, .Location = New Point(btnOtworzFolder.Left, btnZnaczniki.Top),
            .Anchor = btnZnaczniki.Anchor, .Image = UtworzIkoneKreatora(), .ImageAlign = ContentAlignment.MiddleCenter,
            .UseVisualStyleBackColor = True, .TabIndex = btnZnaczniki.TabIndex - 1}
        btnZnaczniki.Parent.Controls.Add(_btnKreator)
        _btnKreator.BringToFront()
        ToolTip1.SetToolTip(_btnKreator, "Szybki kreator mapy (Ctrl+K) - prowadzi krok po kroku od celu do gotowej mapy")
        AddHandler _btnKreator.Click, Sub() OtworzKreatorMapy()

        _menuKreator = New ToolStripMenuItem("Szybki kreator mapy...", UtworzIkoneKreatora(), Sub() OtworzKreatorMapy()) With {
            .Name = "KreatorMapyToolStripMenuItem",
            .ShortcutKeys = Keys.Control Or Keys.K}
        NarzedziaToolStripMenuItem.DropDownItems.Insert(0, _menuKreator)
        NarzedziaToolStripMenuItem.DropDownItems.Insert(1, New ToolStripSeparator())

        If btnKreatorDolny IsNot Nothing Then
            btnKreatorDolny.Image = UtworzIkoneKreatora(16)
            btnKreatorDolny.ImageAlign = ContentAlignment.MiddleLeft
            btnKreatorDolny.TextAlign = ContentAlignment.MiddleCenter
            btnKreatorDolny.TextImageRelation = TextImageRelation.ImageBeforeText
            ToolTip1.SetToolTip(btnKreatorDolny, "Szybki kreator mapy (Ctrl+K) - prowadzi krok po kroku od celu do gotowej mapy")
        End If
    End Sub

    ''' <summary>
    ''' Ikona kreatora: różdżka z dużą żółtą gwiazdką i dwoma iskrami (powszechny znak kreatora). Poprzednia ikona
    ''' (mapa z fioletową kreską) w 16 px wyglądała jak ołówek na kartce, czyli jak „edytuj”.
    ''' </summary>
    Public Shared Function UtworzIkoneKreatora(Optional rozmiar As Integer = 16) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.Clear(Color.Transparent)
            g.ScaleTransform(rozmiar / 16.0!, rozmiar / 16.0!)
            'trzonek różdżki z jasną końcówką
            Using pioro As New Pen(Color.FromArgb(60, 60, 70), 2.4!) With {.StartCap = Drawing2D.LineCap.Round, .EndCap = Drawing2D.LineCap.Round}
                g.DrawLine(pioro, 2.0!, 14.0!, 8.6!, 7.4!)
            End Using
            Using pioro As New Pen(Color.FromArgb(235, 235, 240), 2.4!) With {.EndCap = Drawing2D.LineCap.Round}
                g.DrawLine(pioro, 8.6!, 7.4!, 9.8!, 6.2!)
            End Using
            'gwiazdki
            RysujGwiazdke(g, 11.6!, 4.4!, 4.3!, Color.FromArgb(255, 200, 0), Color.FromArgb(190, 120, 0))
            RysujGwiazdke(g, 4.2!, 4.0!, 2.4!, Color.FromArgb(255, 215, 60), Color.FromArgb(190, 120, 0))
            RysujGwiazdke(g, 13.6!, 11.2!, 2.2!, Color.FromArgb(255, 215, 60), Color.FromArgb(190, 120, 0))
        End Using
        Return bmp
    End Function

    ''' <summary>Czteroramienna gwiazdka (iskra) o środku (x, y) i promieniu r.</summary>
    Private Shared Sub RysujGwiazdke(g As Graphics, x As Single, y As Single, r As Single, wypelnienie As Color, obrys As Color)
        Dim wew As Single = r * 0.36!
        Dim pkt(7) As PointF
        For i = 0 To 7
            Dim kat = i * Math.PI / 4.0 - Math.PI / 2.0
            Dim promien = If(i Mod 2 = 0, r, wew)
            pkt(i) = New PointF(x + CSng(Math.Cos(kat) * promien), y + CSng(Math.Sin(kat) * promien))
        Next
        Using pedzel As New SolidBrush(wypelnienie)
            g.FillPolygon(pedzel, pkt)
        End Using
        Using pioro As New Pen(obrys, 0.8!) With {.LineJoin = Drawing2D.LineJoin.Round}
            g.DrawPolygon(pioro, pkt)
        End Using
    End Sub

    ''' <summary>Silnik podglądu zestawu warstw współdzielony z oknem kreatora.</summary>
    Friend Function SilnikPodgladu() As PodgladWarstwSilnik
        Return _silnikPodgladu
    End Function

    ''' <summary>Centruje mapę w oknie głównym na wskazanym zasięgu i aktualizuje znaczniki.</summary>
    Friend Sub CentrujNaZasiegu(z As Zasieg)
        If Not z.Poprawny Then Exit Sub
        Dim srodek1992X = (z.XDol + z.XGora) / 2.0
        Dim srodek1992Y = (z.YLewy + z.YPrawy) / 2.0
        Dim wgs = UkladWspolrzednych.PL1992.DoWgs84(srodek1992X, srodek1992Y)
        mapa.Position = New GMap.NET.PointLatLng(wgs.Szerokosc, wgs.Dlugosc)
        Dim zDocelowy = If(Ustawienia.Uklad.Epsg <> 2180, UkladWspolrzednych.PL1992.PrzeliczZasieg(Ustawienia.Uklad, z), z)
        WpiszPolaZasiegu(Liczba(zDocelowy.XDol), Liczba(zDocelowy.YLewy), Liczba(zDocelowy.XGora), Liczba(zDocelowy.YPrawy))
        WyczyscZnaczniki()
        PokazZnaczniki()
    End Sub

    Private _oknoKreatora As FormKreatorMapy

    ''' <summary>Otwiera okno szybkiego kreatora generowania mapy.</summary>
    Public Sub OtworzKreatorMapy()
        If PobieranieTrwa("Otwarcie kreatora generowania mapy") Then Exit Sub
        ZakonczWyborNaroznikow()
        If _oknoKreatora IsNot Nothing AndAlso Not _oknoKreatora.IsDisposed Then
            If _oknoKreatora.WindowState = FormWindowState.Minimized Then
                _oknoKreatora.WindowState = FormWindowState.Normal
            End If
            _oknoKreatora.Show()
            _oknoKreatora.Activate()
            _oknoKreatora.BringToFront()
            Exit Sub
        End If

        _oknoKreatora = New FormKreatorMapy(Me)
        AddHandler _oknoKreatora.FormClosed, Sub()
                                                 _oknoKreatora = Nothing
                                                 Enabled = True
                                                 FormKreatorMapy.PrzywrocZMinimalizacji(Me)
                                                 Show()
                                                 If Not TrybOkienBezAktywacji Then
                                                     Activate()
                                                     BringToFront()
                                                     FormKreatorMapy.SetForegroundWindow(Handle)
                                                 End If
                                             End Sub
        _oknoKreatora.Show(Me)
        Enabled = False
    End Sub

#End Region

End Class
