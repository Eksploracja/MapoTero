'Copyright (C) <2015>  pajakt
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

'Copyright (C) <2015>  pajakt
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

Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Tematy pomocy dedykowanej dla poszczególnych narzędzi programu.</summary>
Public Enum TematPomocy
    Mbtiles
    Scalanie
    Eksport
    EksportCad
    DodajSerwer
    Szukaj
    ImportOzi
    Korytarz
    NakladanieMap
    UsuwaniePustych
    Ustawienia
    KorektaFolderu
End Enum

''' <summary>
''' Okno pomocy i instrukcji dedykowanej dla narzędzi MapoTero z aktywnym odnośnikiem do podręcznika online.
''' </summary>
Public Class FormPomocMbtiles

    Private _temat As TematPomocy = TematPomocy.Mbtiles
    Private _urlOnline As String = "https://eksploracja.github.io/MapoTero/pomoc/"

    Public Sub New()
        InitializeComponent()
        _temat = TematPomocy.Mbtiles
    End Sub

    Public Sub New(temat As TematPomocy)
        InitializeComponent()
        _temat = temat
    End Sub

    ''' <summary>Otwiera modalne okno pomocy dla wskazanego narzędzia.</summary>
    Public Shared Sub Pokaz(wlasciciel As IWin32Window, temat As TematPomocy)
        Using dlg As New FormPomocMbtiles(temat)
            dlg.ShowDialog(wlasciciel)
        End Using
    End Sub

    Private Sub FormPomocMbtiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        rtbTresc.Clear()
        GenerujTrescPomocy()
        rtbTresc.SelectionStart = 0
        rtbTresc.SelectionLength = 0
        rtbTresc.ScrollToCaret()
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        Close()
    End Sub

    Private Sub btnOnline_Click(sender As Object, e As EventArgs) Handles btnOnline.Click
        OtworzUrl(_urlOnline)
    End Sub

    Private Sub rtbTresc_LinkClicked(sender As Object, e As LinkClickedEventArgs) Handles rtbTresc.LinkClicked
        If Not String.IsNullOrEmpty(e.LinkText) Then
            OtworzUrl(e.LinkText)
        End If
    End Sub

    Private Sub OtworzUrl(url As String)
        Try
            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show("Nie udało się otworzyć adresu w przeglądarce: " & Global.MapoTero.Core.OpisBledu.Z(ex),
                            "Pomoc online", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub GenerujTrescPomocy()
        Select Case _temat
            Case TematPomocy.Mbtiles : PomocMbtiles()
            Case TematPomocy.Scalanie : PomocScalanie()
            Case TematPomocy.Eksport : PomocEksport()
            Case TematPomocy.EksportCad : PomocEksportCad()
            Case TematPomocy.DodajSerwer : PomocDodajSerwer()
            Case TematPomocy.Szukaj : PomocSzukaj()
            Case TematPomocy.ImportOzi : PomocImportOzi()
            Case TematPomocy.Korytarz : PomocKorytarz()
            Case TematPomocy.NakladanieMap : PomocNakladanieMap()
            Case TematPomocy.UsuwaniePustych : PomocUsuwaniePustych()
            Case TematPomocy.Ustawienia : PomocUstawienia()
            Case TematPomocy.KorektaFolderu : PomocKorektaFolderu()
            Case Else : PomocMbtiles()
        End Select
    End Sub

#Region "Treści pomocy dla poszczególnych narzędzi"

    Private Sub PomocScalanie()
        Me.Text = "Pomoc i instrukcja - Scalanie segmentów"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/scalanie/"

        DodajTytul("Scalanie pobranych segmentów w jeden arkusz mapy")
        DodajAkapit("Narzędzie łączy pobrane kafelki i segmenty rastrowe z folderu sesji w jeden jednolity, wielki arkusz mapy o wysokiej rozdzielczości. " &
                    "Wszelkie parametry zasięgu i siatki są wczytywane automatycznie z pliku konfiguracyjnego conf.txt znajdującego się w folderze segmentów.")

        DodajNaglowek1("1. Formaty wynikowe arkusza")
        DodajPunktor("GeoTIFF / BigTIFF (bezstratny Deflate): branżowy standard GIS (QGIS, ArcGIS, CAD). Posiada bezpośrednio zaszytą georeferencję w nagłówkach pliku.")
        DodajPunktor("GeoTIFF (kompresja JPEG): wysoka jakość wizualna przy wielokrotnie mniejszym rozmiarze pliku na dysku (zalecane dla dużych arkuszy ortofotomapy).")
        DodajPunktor("JPEG: uniwersalny obraz z osobnymi plikami kalibracji (World File .jgw, KML, MAP, TAB). Maksymalny wymiar boku to 65 500 pikseli.")
        DodajPunktor("PNG: format z obsługą pełnej przezroczystości (kanał alfa). Idealny, gdy pobrano warstwy nakładkowe (np. same granice, rzeki lub cieniowanie rzeźby).")

        DodajNaglowek1("2. Pliki georeferencji (kalibracji)")
        DodajPunktor("World File (.jgw / .pgw / .tfw): uniwersalny plik współrzędnych piksela dla systemów CAD i GIS.")
        DodajPunktor("KML (Google Earth): plik nakładki naziemnej do natychmiastowego wyświetlenia arkusza w trójwymiarowym globusie Google Earth.")
        DodajPunktor("MAP (OziExplorer / TrekBuddy): kalibracja punktowa dla tradycyjnych programów nawigacyjnych.")
        DodajPunktor("TAB (MapInfo Professional): plik tabeli georeferencyjnej dla środowiska MapInfo.")

        DodajNaglowek1("3. Wskazówki i rozwiązywanie problemów")
        DodajPunktor("Brak pliku conf.txt: jeśli plik konfiguracyjny uległ utracie, zaznacz opcję 'ręczne wprowadzanie parametrów' i uzupełnij współrzędne zasięgu oraz rozmiar segmentu.")
        DodajPunktor("Wykryto error.txt: obecność pliku error.txt oznacza, że nie pobrano wszystkich segmentów. Uzupełnij brakujące segmenty w oknie głównym przed scalaniem.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocEksport()
        Me.Text = "Pomoc i instrukcja - Eksport mapy (Garmin, MBTiles, Locus, Orux)"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/eksport/"

        DodajTytul("Eksport mapy do formatów urządzeń GPS i aplikacji mobilnych")
        DodajAkapit("Narzędzie przetwarza pobrane segmenty do gotowych do wgrania pakietów mapowych dla nawigacji turystycznych Garmin oraz aplikacji na smartfony (Locus Map, OruxMaps, OsmAnd).")

        DodajNaglowek1("1. Obsługiwane formaty")
        DodajPunktor("Garmin BirdsEye (JNX v4): wielopoziomowy raster z piramidą powiększeń bez limitu 100 kafli (zalecany dla odbiorników Garmin z odblokowanym firmware). Plik .jnx kopiuje się do folderu Garmin/BirdsEye.")
        DodajPunktor("Garmin Custom Maps (KMZ): standardowy format Garmina. Posiada ograniczenie do 100 kafli na plik - w razie potrzeby włącz opcję 'automatyczny podział na tomy', a program utworzy spójne części mieszczące się w limicie urządzenia.")
        DodajPunktor("MBTiles / SQLiteDB: kafelkowa baza SQLite w standardzie Web Mercator, dedykowana dla aplikacji mobilnych (Locus Map, OsmAnd, OruxMaps).")
        DodajPunktor("OruxMaps: folder z bazą kafli i plikiem XML gotowy do wrzucenia do katalogu map OruxMaps.")
        DodajPunktor("GeoTIFF bezpośredni: pojedynczy georeferencjonowany plik TIF dla Avenza Maps i aplikacji GIS.")

        DodajNaglowek1("2. Piramida powiększeń (Zoom Od - Zoom Do)")
        DodajPunktor("Dla formatów kafelkowych (JNX, MBTiles, SQLiteDB) program generuje automatyczną piramidę powiększeń od poziomu przeglądowego do szczegółowego.")
        DodajPunktor("Suwakiem jakości możesz dobrać optymalny kompromis między ostrością a rozmiarem pliku (zalecane 75-85%).")

        DodajNaglowek1("3. Filtr MIP dla ekranów Garmin")
        DodajPunktor("Opcja korekcji MIP optymalizuje kontrast, nasycenie i ostrość splotową dla ekranów transflektywnych Garmina, zapewniając doskonałą czytelność w pełnym słońcu.")
        DodajPunktor("Przycisk 'Podgląd MIP...' w oknie głównym pozwala obejrzeć efekt wyostrzenia i korekty na żywo na próbce mapy przed eksportem.")

        DodajNaglowek1("4. Dostosowanie do ekranu")
        DodajPunktor("Okno eksportu automatycznie dopasowuje swoją wysokość do rozdzielczości ekranu, włączając w razie potrzeby pionowy pasek przewijania.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocEksportCad()
        Me.Text = "Pomoc i instrukcja - Eksport do programów CAD"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/cad/"

        DodajTytul("Eksport podkładów rastrowych i siatek do środowisk CAD")
        DodajAkapit("Narzędzie przygotowuje kompletny projekt dla programów inżynierskich (AutoCAD, BricsCAD, ZWCAD, GstarCAD itp.). " &
                    "Generuje pliki wymiany DXF z warstwami wektorowymi, skrypty SCR oraz program LISP automatyzujący wstawianie rastrów.")

        DodajNaglowek1("1. Odwrócona konwencja osi w CAD")
        DodajPunktor("WAŻNE: W programach geodezyjnych i w MapoTero X oznacza oś północną (pionową), a Y oś wschodnią (poziomą).")
        DodajPunktor("W środowiskach CAD konwencja matematyczna jest odwrotna: X to oś pozioma (wschód), a Y to oś pionowa (północ). Program automatycznie przelicza i zamienia osie przy eksporcie do CAD!")

        DodajNaglowek1("2. Typy eksportu")
        DodajPunktor("Scalony arkusz (zalecany): wstawia pojedynczy połączony arkusz GeoTIFF/JPEG wrysowany precyzyjnie w punkcie wstawienia. Najbardziej wydajny dla CAD.")
        DodajPunktor("Siatka segmentów: wstawia poszczególne segmenty kafli na dedykowanej warstwie CAD.")
        DodajPunktor("Tylko siatka wektorowa: rysuje same ramki arkuszy, numery segmentów i zasięg bez dołączania plików rastrowych.")

        DodajNaglowek1("3. Uruchomienie w programie CAD")
        DodajPunktor("W AutoCAD / BricsCAD wpisz polecenie SCRIPT (lub _SCRIPT) i wskaż wygenerowany plik .scr.")
        DodajPunktor("Alternatywnie załaduj plik .lsp poleceniem APPLOAD i wpisz polecenie MAPOTERO_WSTAW.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocDodajSerwer()
        Me.Text = "Pomoc i instrukcja - Kreator dodawania serwerów WMS/WMTS"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/serwery/kreator/"

        DodajTytul("Kreator dodawania serwerów WMS, WMTS, XYZ i TMS")
        DodajAkapit("Kreator pozwala rozszerzyć bazę map o dowolne publiczne lub prywatne serwery mapowe. " &
                    "Zawiera wbudowany katalog publicznych usług infrastruktury informacji przestrzennej (Geoportal, EZiUDP, powiaty, gminy).")

        DodajNaglowek1("1. Wybór z katalogu usług lub własny adres URL")
        DodajPunktor("Kliknij 'Wybierz z listy usług...', aby przeszukać oficjalny wykaz usług GUGiK, e-mapa oraz portali powiatowych i gminnych.")
        DodajPunktor("Możesz także wkleić bezpośredni adres zapytania GetCapabilities serwera WMS/WMTS lub szablon kafelków XYZ (np. https://serwer/{z}/{x}/{y}.png).")

        DodajNaglowek1("2. Konfiguracja warstwy")
        DodajPunktor("Kreator automatycznie pobierze metadane, sprawdzi dostępne formaty rastra (PNG z przezroczystością, JPEG) oraz obsługiwane układy współrzędnych.")
        DodajPunktor("Zalecany piksel [m/pix]: optymalna rozdzielczość terenowa, przy której mapa zachowuje idealną ostrość bez interpolacji i rozmycia.")
        DodajPunktor("Zapis do zbioru: nowa usługa zostanie zapisana w folderze 'warstwy/' i pojawi się w drzewie 'Wybierz mapy' w oknie głównym.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocSzukaj()
        Me.Text = "Pomoc i instrukcja - Wyszukiwarka miejscowości i działek"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/obszar/zaznaczanie/"

        DodajTytul("Wyszukiwarka miejscowości, adresów i działek katastralnych")
        DodajAkapit("Narzędzie pozwala szybko zlokalizować dowolny punkt w Polsce i wycentrować na nim mapę główną, " &
                    "bądź precyzyjnie wyznaczyć obszar pobierania na podstawie urzędowych granic działki ewidencyjnej.")

        DodajNaglowek1("1. Wyszukiwanie miejscowości i adresów")
        DodajPunktor("Wpisz nazwę miasta, wsi, gminy lub ulicy (np. 'Kraków', 'Zakopane Krupówki', 'Gdańsk Długa').")
        DodajPunktor("Program odnajdzie współrzędne geograficzne i automatycznie przesunie mapę, ustawiając optymalny poziom zbliżenia.")

        DodajNaglowek1("2. Wyszukiwanie działek katastralnych (ULDK GUGiK)")
        DodajPunktor("Wpisz pełny identyfikator działki TERYT (np. 022503_5.0003.134) albo nazwę obrębu i numer działki (np. 'Krzewina 134').")
        DodajPunktor("Usługa ULDK pobierze dokładną geometrię granic działki z państwowego rejestru EGiB.")
        DodajPunktor("Margines [m]: bezpieczny bufor terenu wokół granic działki, aby na pobranej mapie widoczne było również najbliższe otoczenie i dojazd.")
        DodajPunktor("Automatyczne włączenie warstw: zaznaczenie opcji automatycznie aktywuje ortofotomapę wysokiej rozdzielczości oraz urzędową siatkę granic działek ewidencyjnych.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocImportOzi()
        Me.Text = "Pomoc i instrukcja - Konwerter kalibracji OziExplorer (.map)"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/narzedzia/"

        DodajTytul("Konwerter skalibrowanych map OziExplorer (.map)")
        DodajAkapit("Narzędzie konwertuje kalibrowane skany papierowych map turystycznych i archiwalnych (pliki .map wraz z obrazem) " &
                    "do nowoczesnych formatów mobilnych Garmin BirdsEye (JNX v4), Garmin KMZ oraz baz MBTiles.")

        DodajNaglowek1("1. Automatyczna transformacja historycznych układów (Datum)")
        DodajPunktor("Program automatycznie wykrywa i poprawnie transformuje historyczne polskie i europejskie układy odniesienia:" & vbCrLf &
                     "  • Borowa Góra (historyczne mapy WIG 1:100k i 1:25k)," & vbCrLf &
                     "  • Potsdam / Rauenberg (mapy niemieckie Messtischblatt 1:25k i Großblatt)," & vbCrLf &
                     "  • Pulkovo 1942 / Układ 1965 / Układ 1942 (mapy topograficzne z okresu PRL).")
        DodajPunktor("Współrzędne są precyzyjnie przeliczane do elipsoidy WGS84, dzięki czemu mapa nie jest przesunięta względem śladów GPS.")

        DodajNaglowek1("2. Tryb pojedynczy i wsadowy (Batch)")
        DodajPunktor("Możesz wybrać pojedynczy plik .map albo cały folder zawierający dziesiątki arkuszy.")
        DodajPunktor("W trybie wsadowym konwerter przetworzy całą serię arkuszy jeden po drugim.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocKorytarz()
        Me.Text = "Pomoc i instrukcja - Obszar wzdłuż trasy (korytarz)"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/obszar/korytarz/"

        DodajTytul("Obszar pobierania wzdłuż trasy (Korytarz GPX/KML)")
        DodajAkapit("Zamiast pobierać wielki prostokątny obszar terenu (z którego większość to niepotrzebne połacie), " &
                    "korytarz ogranicza pobieranie mapy wyłącznie do wąskiego pasa terenu wzdłuż planowanej trasy wędrówki lub spływu.")

        DodajNaglowek1("1. Obsługiwane pliki tras")
        DodajPunktor("Pliki GPX (.gpx): ślady (trk), trasy (rte) oraz punkty trasy (wpt).")
        DodajPunktor("Pliki Google Earth (.kml / .kmz): ścieżki LineString oraz gx:Track.")

        DodajNaglowek1("2. Szerokość bufora korytarza")
        DodajPunktor("Wpisz szerokość korytarza w metrach (np. 500 m, 1000 m lub 2000 m).")
        DodajPunktor("Program wyznaczy pas o tej szerokości po obu stronach śladu i wybierze tylko te segmenty siatki, przez które przebiega bufor.")
        DodajPunktor("Oszczędność czasu i pamięci: korytarz pozwala zredukować liczbę pobieranych kafelków nawet o 80-90%!")

        DodajNaglowek1("3. Profil wysokościowy i rzędne NMT / LIDAR")
        DodajPunktor("Przycisk 'Wzbogać ślad o wysokości' pobiera rzeczywiste rzędne terenu z lotniczego skaningu laserowego (NMT GUGiK) dla wszystkich punktów trasy.")
        DodajPunktor("W oknie wyświetlany jest interaktywny wykres profilu z obliczoną sumą przewyższeń (podejścia, zejścia, min/max).")
        DodajPunktor("Przycisk 'Zapisz wzbogacony GPX' eksportuje plik GPX z tagami <ele> dla odbiorników i zegarków turystycznych.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocNakladanieMap()
        Me.Text = "Pomoc i instrukcja - Nakładanie dwóch map na siebie"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/narzedzia/"

        DodajTytul("Nakładanie dwóch warstw mapowych na siebie")
        DodajAkapit("Narzędzie umożliwia cyfrowe zmiksowanie dwóch wcześniej pobranych zestawów segmentów o tym samym zasięgu i wymiarach w jeden wspólny zestaw.")

        DodajNaglowek1("1. Warstwa dolna i górna")
        DodajPunktor("Warstwa dolna (podkład): zazwyczaj współczesna ortofotomapa satelitarna lub mapa hipsometryczna.")
        DodajPunktor("Warstwa górna (nakładka): mapa archiwalna, skan topograficzny lub siatka granic.")
        DodajPunktor("Krycie suwakiem: określa stopień przezroczystości warstwy górnej (np. 50% pozwala idealnie porównać dawny przebieg dróg i lasów ze stanem obecnym).")

        DodajNaglowek1("2. Wymagania")
        DodajPunktor("Oba foldery muszą zawierać segmenty o identycznych wymiarach pikselowych i nazwach odpowiadających tej samej siatce.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocUsuwaniePustych()
        Me.Text = "Pomoc i instrukcja - Usuwanie pustych segmentów"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/narzedzia/"

        DodajTytul("Wykrywanie i usuwanie pustych (jednolitych) segmentów")
        DodajAkapit("Narzędzie analizuje pliki w folderze pobierania i wykrywa segmenty jednolicie białe lub puste, " &
                    "które serwery WMS zwracają dla obszarów leżących poza zasięgiem danych (np. poza granicami kraju lub na morzu).")

        DodajNaglowek1("1. Bezpieczeństwo danych")
        DodajPunktor("Wykryte puste pliki są bezpiecznie przenoszone do Kosza systemowego Windows, skąd w razie potrzeby można je łatwo przywrócić.")
        DodajPunktor("Usunięcie pustych segmentów zapobiega ich scalaniu do gotowego arkusza i drastycznie zmniejsza wagę baz kafelków MBTiles.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocKorektaFolderu()
        Me.Text = "Pomoc i instrukcja - Korekta obrazów w folderze"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/narzedzia/"

        DodajTytul("Korekta i optymalizacja obrazów w folderze")
        DodajAkapit("Narzędzie poprawia naraz wszystkie obrazy PNG, JPEG i TIFF z wybranego folderu (np. segmenty sesji) " &
                    "według wybranego profilu: kontrast, korekcja gamma, nasycenie barw i wyostrzanie krawędzi. " &
                    "Oryginały zostają bez zmian - poprawione kopie trafiają do folderu wynikowego (domyślnie podfolder ""korekta"").")

        DodajNaglowek1("1. Profil korekty")
        DodajPunktor("Przycisk ""Dostosuj"" otwiera podgląd na próbce z folderu. Możesz wybrać gotowy profil (np. Garmin MIP, Podkład CAD, Skan CAD) albo ustawić własne wartości suwaków.")
        DodajPunktor("Profil ""Brak"" nie zmienia obrazów, więc w tym oknie nie można go wybrać.")

        DodajNaglowek1("2. Georeferencja")
        DodajPunktor("Opcja ""Kopiuj pliki georeferencji"" kopiuje obok poprawionych obrazów pliki .jpgw, .pngw, .tifw, .wld, .prj, .map, .gmi, .tab, .kml i inne oraz conf.txt sesji.")
        DodajPunktor("GeoTIFF zachowuje georeferencję zapisaną w pliku, gdy program zna jego układ współrzędnych.")

        DodajNaglowek1("3. Przerwanie i błędy")
        DodajPunktor("Przycisk ""Przerwij"" albo zamknięcie okna przerywa przetwarzanie; już zapisane kopie zostają w folderze wynikowym.")
        DodajPunktor("Plik, którego nie da się odczytać, jest pomijany, a jego nazwę program podaje po zakończeniu pracy.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocUstawienia()
        Me.Text = "Pomoc i instrukcja - Ustawienia programu"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/ustawienia/"

        DodajTytul("Ustawienia programu MapoTero (lastsettings.txt)")
        DodajAkapit("W oknie ustawień konfigurujesz globalne parametry programu, zachowanie interfejsu oraz domyślne właściwości nowych sesji pobierania.")

        DodajNaglowek1("1. Układ współrzędnych nowej sesji")
        DodajPunktor("PL-1992 (EPSG:2180): standardowy układ państwowy dla całej Polski (najlepszy dla ortofotomapy i danych ogólnokrajowych).")
        DodajPunktor("PL-2000 (strefy 5-8): państwowy układ wielkoskalowy o minimalnych zniekształceniach odwzorowawczych (zalecany do prac geodezyjnych i działek EGiB).")
        DodajPunktor("UTM (strefy 33N-35N) i WGS84: układy międzynarodowe i nawigacyjne.")

        DodajNaglowek1("2. Widok okna głównego i kursor")
        DodajPunktor("Współrzędne kursora i środka mapy: wyświetla na bieżąco pozycję kursora myszy w układzie pobierania oraz w WGS84.")
        DodajPunktor("Edycja pól XY: pozwala ręcznie korygować liczby w polach zasięgu z klawiatury.")
        DodajPunktor("Siatka segmentów na mapie: włącza rysowanie siatki kafli bezpośrednio na podkładzie mapowym.")

        DodajNaglowek1("3. Pobieranie i serwery WMS/WMTS")
        DodajPunktor("Odstęp zapytań i ochrona serwerów: ogranicza tempo pobierania, zapobiegając blokadom IP ze strony serwerów rządowych.")
        DodajPunktor("Zachowuj pobrane kafle WMTS: pozostawia surowe kafelki w pamięci podręcznej, co umożliwia błyskawiczne ponowne generowanie mapy offline.")

        DodajStopkeOnline(_urlOnline)
    End Sub

    Private Sub PomocMbtiles()
        Me.Text = "Pomoc i instrukcja - Kreator wielowarstwowego MBTiles"
        _urlOnline = "https://eksploracja.github.io/MapoTero/pomoc/mbtiles-wielowarstwowy/"

        DodajTytul("Kreator wielowarstwowego i wieloskalowego MBTiles")
        DodajAkapit("Narzędzie umożliwia skompilowanie wielu wcześniej pobranych sesji mapowych do jednego, uniwersalnego pliku bazy kafelków MBTiles. " &
                    "Dzięki niemu w aplikacjach mobilnych (OruxMaps, Locus Map, OsmAnd i innych) możesz w zależności od skali powiększenia oglądać zupełnie inne mapy " &
                    "(np. z daleka przeglądową topo 1:100k, w zbliżeniu szczegółową topo 1:10k, a w maksymalnym powiększeniu ortofotomapę) lub łączyć je ze sobą.")

        DodajNaglowek1("1. Przygotowanie danych przed uruchomieniem kreatora")
        DodajPunktor("Każdą mapę (warstwę) pobierz wcześniej w głównym oknie MapoTero do osobnego folderu (np. 'sesja_topo25', 'sesja_topo10', 'sesja_orto').")
        DodajPunktor("Pobierane foldery muszą obejmować ten sam rejon geograficzny i zawierać plik konfiguracyjny conf.txt oraz pobrane kafelki.")
        DodajPunktor("Kreator nie pobiera kafelków z internetu - kompiluje lokalnie pobrane sesje w jedną zoptymalizowaną bazę SQLite MBTiles.")

        DodajNaglowek1("2. Dodawanie warstw do projektu")
        DodajPunktor("Kliknij 'Dodaj warstwę...' i wskaż folder z pobraną warstwą mapy.")
        DodajPunktor("Możesz także wskazać folder nadrzędny (np. 'download') - program wykryje wszystkie podfoldery z sesjami i zaproponuje dodanie ich wszystkich naraz.")
        DodajPunktor("Program automatycznie odczyta rozdzielczość piksela i zaproponuje optymalne poziomy powiększeń (Zoom Od - Zoom Do).")

        DodajNaglowek1("3. Zakresy wyświetlania (Zoom Od – Zoom Do)")
        DodajPunktor("W kolumnach 'Zoom Od' i 'Zoom Do' precyzyjnie wskaż, w jakim przedziale powiększeń dana warstwa ma być widoczna.")
        DodajPunktor("Przykładowa konfiguracja wieloskalowa:" & vbCrLf &
                     "  • Poziom 11 - 13: Mapa topograficzna ogólna 1:100 000" & vbCrLf &
                     "  • Poziom 14 - 15: Mapa topograficzna 1:25 000" & vbCrLf &
                     "  • Poziom 16 - 17: Mapa topograficzna 1:10 000" & vbCrLf &
                     "  • Poziom 18 - 19: Ortofotomapa o wysokiej rozdzielczości")
        DodajPunktor("Dwie lub więcej warstw mogą mieć ten sam zakres zoom - zostaną wtedy zblendowane (nałożone na siebie).")

        DodajNaglowek1("4. Hierarchia i kolejność warstw (Stos warstw)")
        DodajPunktor("GÓRA TABELI = WIERZCH MAPY (tak samo jak w programach GIS, np. QGIS czy Photoshopie).")
        DodajPunktor("DÓŁ TABELI = SPÓD / PODKŁAD (warstwa bazowa, tło).")
        DodajPunktor("Użyj przycisków 'W górę' i 'W dół':" & vbCrLf &
                     "  • 'W górę' - wynosi warstwę wyżej (przesuwa na wierzch stosu)," & vbCrLf &
                     "  • 'W dół' - spycha warstwę niżej (pod spód, jako podkład).")

        DodajNaglowek1("5. Przezroczystość, odcienie szarości i tryby mieszania (Styl QGIS)")
        DodajPunktor("Krycie %: określa krycie warstwy od 0% (niewidoczna) do 100% (pełne krycie).")
        DodajPunktor("Szarość: konwertuje kolorową warstwę do odcieni szarości. Świetne jako neutralny podkład satelitarny dla kolorowych szlaków lub do stonowania pstrej mapy topo.")
        DodajPunktor("Białe przezr.: zamienia białe tło (np. skanów papierowych lub WMS bez obsługi kanału alfa) w przezroczyste.")
        DodajPunktor("Tryb mieszania (kolumna 'Mieszanie'):" & vbCrLf &
                     "  • 1 Zwykły (Normal) - standardowe nakładanie z kryciem." & vbCrLf &
                     "  • 2 Mnożenie (Multiply) - tryb znany z QGIS i Photoshopa. Mnoży kolory warstwy przez podkład. " &
                     "Białe tło staje się całkowicie niewidoczne, a czarne napisy, rzędne wysokościowe i poziomice zachowują maksymalny kontrast na tle ortofotomapy (brak wypłowienia i 'mlecznej mgły')!" & vbCrLf &
                     "  • 3 Ciemniej (Darken) - wybiera ciemniejszy odcień z obu warstw." & vbCrLf &
                     "  • 4 Jaśniej (Lighten) - wybiera jaśniejszy kolor z obu warstw (np. jasne drogi/cieki wodne na ciemnym tle)." & vbCrLf &
                     "  • 5 Ekran (Screen) - rozjaśnia tło, czerń znika (odwrotność mnożenia, np. dla nocnych świateł czy radarów)." & vbCrLf &
                     "  • 6 Nakładka (Overlay) - łączy mnożenie i ekran, podbijając kontrast (idealne dla cieniowania rzeźby terenu / hillshade).")

        DodajNaglowek1("6. Symulator podglądu na żywo")
        DodajPunktor("Przesuwaj suwak 'Poziom zoom' w prawym panelu, aby podejrzeć kafel podglądu dla każdego powiększenia.")
        DodajPunktor("Podgląd uwzględnia kolejność warstw, przezroczystości, krycie oraz tryb mnożenia w czasie rzeczywistym.")

        DodajNaglowek1("7. Parametry kompresji i zapis pliku")
        DodajPunktor("Format Smart Hybrid (zalecany): kafle bez przezroczystości zapisuje jako mały JPEG, a kafle z przezroczystością (lub na krawędziach) jako PNG.")
        DodajPunktor("Szybka piramida w pasmach: generuje niższe powiększenia przez szybkie zmniejszanie (downscale 2x), skracając czas zapisu.")

        DodajStopkeOnline(_urlOnline)
    End Sub

#End Region

#Region "Formatowanie tekstu RichTextBox"

    Private Sub DodajTytul(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        rtbTresc.SelectionColor = Color.DarkBlue
        rtbTresc.AppendText(tekst & vbCrLf & vbCrLf)
    End Sub

    Private Sub DodajNaglowek1(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 10.5!, FontStyle.Bold)
        rtbTresc.SelectionColor = Color.FromArgb(0, 70, 140)
        rtbTresc.AppendText(vbCrLf & tekst & vbCrLf)
    End Sub

    Private Sub DodajAkapit(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 9.5!, FontStyle.Regular)
        rtbTresc.SelectionColor = Color.FromArgb(30, 30, 30)
        rtbTresc.AppendText(tekst & vbCrLf)
    End Sub

    Private Sub DodajPunktor(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 9.5!, FontStyle.Regular)
        rtbTresc.SelectionColor = Color.Black
        rtbTresc.AppendText(" • " & tekst & vbCrLf)
    End Sub

    Private Sub DodajStopkeOnline(url As String)
        rtbTresc.AppendText(vbCrLf)
        rtbTresc.SelectionFont = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        rtbTresc.SelectionColor = Color.FromArgb(0, 100, 0)
        rtbTresc.AppendText("Szczegółowy podręcznik online:" & vbCrLf)

        rtbTresc.SelectionFont = New Font("Segoe UI", 9.5!, FontStyle.Underline)
        rtbTresc.SelectionColor = Color.Blue
        rtbTresc.AppendText(url & vbCrLf & vbCrLf)

        rtbTresc.SelectionFont = New Font("Segoe UI", 8.5!, FontStyle.Italic)
        rtbTresc.SelectionColor = Color.Gray
        rtbTresc.AppendText("Kliknij powyższy odnośnik lub przycisk 'Pomoc online' na dole okna, aby otworzyć rozdział w przeglądarce." & vbCrLf)
    End Sub

#End Region

End Class

''' <summary>
''' Narzędzia integracji pomocy kontekstowej dla okien programu MapoTero.
''' </summary>
Public Module PomocProgramu

    ''' <summary>
    ''' Dodaje standardowy przycisk „Pomoc” w dolnym lewym rogu okna narzędzia i podpina zdarzenie otwarcia pomocy.
    ''' </summary>
    ''' <summary>Wymiary czcionki, w których zaprojektowano okna (AutoScaleDimensions w plikach *.Designer.vb).</summary>
    Friend ReadOnly WymiarProjektanta As New SizeF(6.0!, 13.0!)

    ''' <summary>
    ''' Skala, o jaką WinForms przeskalował okno względem projektu (AutoScaleMode.Font: bieżąca czcionka / 6 x 13).
    ''' Już przy 100% DPI domyślna czcionka Segoe UI 9 daje ok. 1,17 x 1,15; przy skalowaniu ekranu - więcej.
    ''' </summary>
    Friend Function SkalaProjektanta(form As Form) As SizeF
        If form.AutoScaleMode = AutoScaleMode.Font Then
            Dim biezace As SizeF = form.CurrentAutoScaleDimensions
            If biezace.Width > 0 AndAlso biezace.Height > 0 Then
                Return New SizeF(biezace.Width / WymiarProjektanta.Width, biezace.Height / WymiarProjektanta.Height)
            End If
        End If
        Return New SizeF(1.0!, 1.0!)
    End Function

    Public Function DodajPrzyciskPomocy(form As Form, temat As TematPomocy, Optional x As Integer = 12, Optional y As Integer = -1) As Button
        'okna otwierane jako domyślna instancja (np. Form3.ShowDialog()) nie są niszczone po zamknięciu, a zdarzenie
        'Load zachodzi przy każdym otwarciu - bez tego sprawdzenia każde otwarcie dokładało kolejny przycisk
        Dim istniejacy = TryCast(form.Controls("btnPomoc"), Button)
        If istniejacy IsNot Nothing Then Return istniejacy
        Dim btn As New Button()
        btn.Name = "btnPomoc"
        btn.Text = "Pomoc"
        'położenie i rozmiar podawane są w jednostkach projektanta (czcionka 6 x 13); okno jest już przeskalowane do
        'bieżącej czcionki (Segoe UI 9 albo większej przy skalowaniu ekranu), więc przycisk skalowany jest tak samo -
        'dawniej trafiał w położenie z projektu, a więc w środek przeskalowanego okna
        Dim skala As SizeF = SkalaProjektanta(form)
        Dim sx As Single = skala.Width, sy As Single = skala.Height
        btn.Size = New Size(CInt(Math.Round(75 * sx)), CInt(Math.Round(26 * sy)))
        btn.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        btn.UseVisualStyleBackColor = True
        x = CInt(Math.Round(x * sx))
        If y < 0 Then
            btn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
            y = form.ClientSize.Height - btn.Height - CInt(Math.Round(12 * sy))
        Else
            'położenie z projektu - kotwica jak u sąsiednich przycisków z projektu (Top, Left); z kotwicą do dołu
            'przycisk w oknie eksportu po włączeniu przewijania na niskim ekranie wypadał poniżej zawartości okna
            btn.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            y = CInt(Math.Round(y * sy))
        End If
        btn.Location = New Point(x, y)
        AddHandler btn.Click, Sub() FormPomocMbtiles.Pokaz(form, temat)
        'klawisz F1 - pomoc online opisywała go w oknach narzędzi, a działał tylko w wielowarstwowym MBTiles
        form.KeyPreview = True
        AddHandler form.KeyDown, Sub(s As Object, e As KeyEventArgs)
                                     If e.KeyCode = Keys.F1 AndAlso e.Modifiers = Keys.None AndAlso Not e.Handled Then
                                         e.Handled = True
                                         FormPomocMbtiles.Pokaz(form, temat)
                                     End If
                                 End Sub
        form.Controls.Add(btn)
        btn.BringToFront()
        Return btn
    End Function

    ''' <summary>
    ''' Okno o stałym rozmiarze, wyższe lub szersze niż ekran roboczy (np. laptop 1366 x 768 - 728 px nad paskiem zadań),
    ''' zmniejszane do ekranu z paskami przewijania - dawniej przyciski u dołu okna były poza ekranem.
    ''' Tylko dla okien bez kontrolek zakotwiczonych do dołu lub prawej (przy przewijaniu przesuwałyby się na zawartość).
    ''' </summary>
    Public Sub DopasujDoEkranu(form As Form)
        Dim ekran As Rectangle = Screen.FromControl(form).WorkingArea
        If form.Height <= ekran.Height AndAlso form.Width <= ekran.Width Then Return
        Dim klient As Size = form.ClientSize
        form.AutoScroll = True
        form.AutoScrollMinSize = klient
        Dim zaWysokie As Boolean = form.Height > ekran.Height
        Dim zaSzerokie As Boolean = form.Width > ekran.Width
        Dim w As Integer = Math.Min(form.Width + If(zaWysokie, SystemInformation.VerticalScrollBarWidth, 0), ekran.Width)
        Dim h As Integer = Math.Min(form.Height + If(zaSzerokie, SystemInformation.HorizontalScrollBarHeight, 0), ekran.Height)
        form.StartPosition = FormStartPosition.Manual
        form.Bounds = New Rectangle(ekran.Left + (ekran.Width - w) \ 2, ekran.Top + (ekran.Height - h) \ 2, w, h)
    End Sub

    Private ReadOnly _oknaZEsc As New Runtime.CompilerServices.ConditionalWeakTable(Of Form, Object)()

    ''' <summary>
    ''' Klawisz Esc zamyka okno narzędzia (jak przycisk zamykania okna) - w 8 oknach bez przycisku Anuluj/Zamknij nie działał.
    ''' Okna z pracą w tle same pytają w FormClosing, czy przerwać. Esc przy rozwiniętej liście zwija tylko listę.
    ''' Wywoływane w Load - okna otwierane jako domyślna instancja dostają obsługę tylko raz.
    ''' </summary>
    Public Sub ZamykanieKlawiszemEsc(form As Form, Optional rozmiarMinimalny As Boolean = False)
        'tylko przy pierwszym otwarciu - dawniej rozmiar minimalny brany był przy każdym otwarciu z rozmiaru, do którego
        'użytkownik powiększył okno poprzednio, więc okna nie dało się już zmniejszyć
        Dim znacznik As Object = Nothing
        If _oknaZEsc.TryGetValue(form, znacznik) Then Return
        _oknaZEsc.Add(form, New Object())
        If rozmiarMinimalny Then form.MinimumSize = form.Size
        If form.CancelButton IsNot Nothing Then Return
        form.KeyPreview = True
        AddHandler form.KeyDown, Sub(s As Object, e As KeyEventArgs)
                                     If e.KeyCode <> Keys.Escape OrElse e.Modifiers <> Keys.None OrElse e.Handled Then Return
                                     Dim lista = TryCast(form.ActiveControl, ComboBox)
                                     If lista IsNot Nothing AndAlso lista.DroppedDown Then Return
                                     e.Handled = True
                                     form.Close()
                                 End Sub
    End Sub

End Module
