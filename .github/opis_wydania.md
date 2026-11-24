MapoTero 3.19 - szybki kreator map (Ctrl+K), transfer Wi-Fi z kodem QR, OGC GeoPackage (.gpkg), Cloud-Optimized GeoTIFF, kopiowanie USB Garmin i nowy układ okna głównego.

## Instalacja

1. Pobierz plik **[MapoTero_v3.19.zip](https://github.com/Eksploracja/MapoTero/releases/download/v3.19/MapoTero_v3.19.zip)** (bezpośrednie pobranie lub poniżej w sekcji *Assets*).
2. Przed rozpakowaniem kliknij plik prawym przyciskiem myszy, wybierz *Właściwości*, zaznacz **Odblokuj** i kliknij *OK* - wtedy Windows nie będzie blokował uruchomienia programu (program nie ma podpisu cyfrowego, więc filtr SmartScreen traktuje go jako nierozpoznaną aplikację; można też w oknie SmartScreen kliknąć *Więcej informacji* > *Uruchom mimo to*). Sumy kontrolne plików i wynik skanu antywirusowego - niżej, w sekcji *Sumy kontrolne SHA-256*.

   ![Okno SmartScreen: najpierw „Więcej informacji”, potem „Uruchom mimo to”](https://raw.githubusercontent.com/Eksploracja/MapoTero/master/obrazy/smartscreen.png)
3. Rozpakuj archiwum do dowolnego folderu i uruchom **MapoTero.exe** - program to jeden plik, obok którego musi leżeć folder `warstwy` (zbiory map).
4. Program wymaga środowiska [.NET Desktop Runtime 10 (x64)](https://dotnet.microsoft.com/download/dotnet/10.0) - jeśli nie jest zainstalowane, Windows zaproponuje jego pobranie przy pierwszym uruchomieniu.

Ustawienia i zbiory map z poprzednich wersji (plik `lastsettings.txt`, folder `warstwy`) są w pełni zgodne z wersją 3.19.

## Zmiany w wersji 3.19

* **Szybki kreator generowania map (skrót Ctrl+K, przycisk z różdżką na pasku):**
  * Prowadzi użytkownika krok po kroku przez cały proces przygotowania gotowej mapy w 4 prostych krokach: wybór urządzenia docelowego, wybór stylu mapy z podglądem na żywo, wskazanie obszaru oraz podsumowanie z kalkulacją parametrów i automatycznym wygenerowaniem pliku.
  * Obsługiwane platformy docelowe: smartfon (MBTiles, OGC GeoPackage, SQLiteDB dla Locus Map, OsmAnd, QField, OruxMaps), odbiornik turystyczny Garmin (Garmin Custom Maps KMZ oraz BirdsEye JNX v4), wydruk wielkoformatowy (arkusz GeoTIFF / JPEG / PNG w formatach A4–A0) oraz podkład CAD/GIS (georeferencjonowany GeoTIFF).
  * Gotowe style z automatycznym doborem warstw, krycia i trybów mieszania: turystyka piesza (mapa topograficzna GUGiK BDOT10k/BDOO), góry z poziomicami (OpenTopoMap z hipsometrią dla Polski, Czech i Słowacji), trasy rowerowe i MTB, dziedzictwo kulturowe (rzeźba terenu LiDAR z prześwitującą mapą i rejestrem zabytków NID oraz stanowiskami archeologicznymi), mapy historyczne WIG (1918–1939) z Messtischblattami na ziemiach zachodnich, archiwalne mapy niemieckie do 1945 r., działki katastralne EGiB oraz styl własny z warstw zdefiniowanych w oknie głównym.
  * **Mapy wieloskalowe:** automatyczne generowanie piramid skalowych w formatach MBTiles, GeoPackage, SQLiteDB i JNX — przy oddalaniu widoku mapa automatycznie przełącza się na mniejszą skalę (np. 1:10 000 → 1:25 000 → 1:50 000 → 1:100 000) zamiast pokazywać nieczytelną pomniejszoną mozaikę.
  * Opcjonalna warstwa zdjęć lotniczych (ortofotomapa przy zbliżeniu, zoom 17, piksel 0,75 m) dla największych powiększeń w stylach turystycznych.
  * Zaawansowany kalkulator: precyzyjne szacowanie liczby segmentów, rozmiaru pliku i czasu generowania z podziałem na serwery, ostrzeganie o limicie 100 kafli Garmina i braku miejsca na dysku, wskaźnik rzeczywistego postępu i pozostałego czasu pobierania.
  * Możliwość zapisu własnej kompozycji jako własnego szablonu (*Zapisz jako szablon...*) do wielokrotnego wykorzystania.
* **Bezpośredni transfer map na telefon przez Wi-Fi i kod QR:**
  * W oknie *Eksport mapy*, oknie zaawansowanego MBTiles oraz w menu *Narzędzia -> Prześlij mapę na telefon (Wi-Fi)...* przycisk *Wyślij na telefon...* uruchamia wbudowany, lekki serwer HTTP w sieci lokalnej i wyświetla na ekranie duży, czytelny kod QR (natywny generator bez zewnętrznych zależności).
  * Zeskanowanie kodu aparatem smartfona natychmiast otwiera stronę pobierania i przesyła plik (`.mbtiles`, `.gpkg`, `.tif`, `.kmz`, `.sqlitedb`, `.tar`) bezpośrednio do pamięci telefonu (Locus Map, QField, OsmAnd, OruxMaps, Avenza Maps) bez użycia kabla USB i bez wysyłania map do zewnętrznych chmur.
  * Pełne wsparcie wznawiania transferów (nagłówek HTTP `Range`), automatyczny wybór aktywnej karty sieciowej Wi-Fi/Ethernet oraz zabezpieczenie każdej sesji jednorazowym tokenem kryptograficznym.
* **Bezpośrednie kopiowanie na odbiornik Garmin (USB):**
  * W oknie eksportu program automatycznie wykrywa podłączone urządzenia GPS Garmin oraz czytniki kart pamięci microSD, prezentując dedykowaną opcję zapisu bezpośrednio do katalogu `Garmin/CustomMaps` (dla plików KMZ) lub `Garmin/BirdsEye` (dla plików JNX) z weryfikacją wolnego miejsca na dysku.
* **Obsługa formatu OGC GeoPackage (.gpkg):**
  * Eksport kafelków rastrowych w standardzie OGC GeoPackage 1.3 (układ Web Mercator EPSG:3857) dla nowoczesnych aplikacji terenowych GIS (QField, Mergin Maps, QGIS) oraz Locus Map i OruxMaps.
  * Wczytywanie poligonów i tras wektorowych z warstw GeoPackage (.gpkg) do definiowania obszaru pobierania (prostokąt otaczający z podglądem lub pas korytarza wzdłuż linii) z automatyczną transformacją układów współrzędnych (PL-1992, PL-2000, UTM, WGS84, Web Mercator).
* **Kafelkowany Cloud-Optimized GeoTIFF (256×256) z wewnętrzną piramidą pomniejszeń:**
  * Eksport arkuszy GeoTIFF i scalanie segmentów tworzy teraz zoptymalizowaną strukturę kafelkowaną z wewnętrznymi poziomami pomniejszeń (IFD reduced overview images).
  * Błyskawiczne otwieranie i płynne przeglądanie wielogigabajtowych map w Avenza Maps, QGIS, ArcGIS oraz oprogramowaniu CAD (AutoCAD, MicroStation) z dekompresją z dysku wyłącznie widocznych fragmentów rastra.
* **Nowy, ergonomiczny układ okna głównego i kart narzędziowych:**
  * Zintegrowany pasek wyszukiwania nad mapą (*Szukaj miejsca lub działki...*, skrót Ctrl+F) z podpowiedziami w trakcie pisania, obsługą adresów, miejscowości (geokoder Nominatim), identyfikatorów TERYT / numerów działek (ULDK) oraz bezpośrednim wklejaniem współrzędnych (WGS84, PL-1992, PL-2000).
  * Nowy podział na karty pod mapą z czytelnymi ikonami i obramowaniem: karta *Obszar pobierania* z rozwijanym menu *Zmień ▾* (dostęp do współrzędnych, korytarza GPX, poligonu KML/GeoPackage), karta *Parametry segmentów* z bezpośrednim linkiem do edycji oraz karta *Stan sesji* zawierająca wyróżniony przycisk *Pobierz segmenty* wraz z łukiem postępu i statusem.
  * Prawy panel boczny z regulacją wysokości sekcji za pomocą uchwytu, zoptymalizowana lista wybranych warstw z bezpośrednią edycją krycia w wierszu warstwy i usuwaniem przyciskiem ✕.
  * Odświeżony dolny pasek stanu z precyzyjną informacją o układzie współrzędnych sesji, aktualnych współrzędnych kursora (X, Y), przybliżonej skali mapy na ekranie oraz statusie sesji pobierania.
* **Nowy tryb mieszania „7 Rzeźba terenu”:**
  * Specjalistyczny algorytm mieszania warstw dla cieniowania NMT LiDAR traktujący kolor neutralnej szarości płaskich powierzchni jako przezroczysty, dzięki czemu cieniowanie nie przyciemnia mapy na równinach, a stoki i wzniesienia zyskują plastyczną, naturalną głębię (automatycznie aktywowany przy dodawaniu warstw cieniowania rzeźby).
* **Uporządkowanie architektury i usunięcie archaicznych modułów:**
  * Funkcja scalania segmentów została całkowicie zintegrowana w oknie *Eksport mapy* (format *Arkusz: GeoTIFF / JPEG / PNG*), zapewniając dostęp do nakładek, wielowątkowości i profili korekty obrazu; dawne osobne okno scalania usunięto.
  * Usunięto przestarzałe narzędzia *Nakładanie dwóch map png na siebie* oraz *Usuwanie pustych segmentów* (zastąpione przez automatyczną detekcję pustych segmentów WMS i zaawansowane nakładki w eksporcie).
  * Bezpieczne usuwanie plików sesji do systemowego Kosza (w przypadku dużej liczby plików trafiają one do jednego wspólnego folderu z datą i godziną).
  * Pełna obsługa polskich znaków diakrytycznych w ścieżkach przy dowolnych regionalnych ustawieniach systemowych (np. Windows z językiem angielskim).
* **Stabilność i jakość kodu:**
  * **1229 testów jednostkowych i integracyjnych** (.NET 10 x64) weryfikujących wszystkie operacje matematyczne, układy współrzędnych, silnik rastrowy, eksport i interfejs programu.

## Zmiany w wersji 3.18

* **Wyszukiwarka miejscowości, adresów i działek ULDK (`Ctrl+F`, `Ctrl+D`):** szukanie po adresie, kodzie TERYT lub nazwie obrębu i numerze działki, z obrysem działki na mapie i automatyczną siatką pobierania z buforem.
* **Podgląd zestawu warstw na żywo:** miniatura wybranych warstw z lupą w prawym dolnym rogu okna głównego, pokazująca krycie i mieszanie warstw przed pobieraniem.
* **Korekta obrazu i profile Garmin MIP:** poprawa kontrastu, nasycenia i ostrości z podglądem na żywo dla ekranów Garmin MIP, gotowe profile korekty oraz narzędzie do zbiorczej korekty obrazów w folderze.
* **Konwerter map OziExplorer (`.map` na JNX, KMZ i MBTiles):** konwersja pojedynczych plików i całych folderów (także przez przeciągnięcie na okno programu) z korektą historycznych układów współrzędnych.
* **Profil wysokościowy trasy (NMT LiDAR):** wykres wysokości i suma podejść dla śladu GPX w oknie korytarza oraz zapis pliku GPX z uzupełnionymi wysokościami.
* **Bezpośredni eksport do GeoTIFF:** zapis mapy wprost do GeoTIFF (dla Avenza Maps, QField, GIS i CAD) z georeferencją.
* **Automatyczne dzielenie dużych map KMZ na tomy:** podział map przekraczających limit 100 kafli Garmin na kolejne pliki bez obniżania rozdzielczości.
* **Wygoda i niezawodność:** podręczna pomoc (`F1`) w oknach narzędzi, przycisk *Nowa sesja* oraz wykrywanie i ponawianie pustych białych segmentów zwracanych przez serwery WMS.

## Zmiany w wersji 3.17

* **Kreator dodawania serwerów – wbudowany katalog gotowych usług publicznych i rejestr EZiUDP:**
  * Okno kreatora otwiera się od razu na katalogu z listą ponad 100 sprawdzonych usług dodatkowych bez uciążliwych skorowidzów (m.in. archiwalne ortofotomapy GUGiK, BDOT10k/BDOO/TOPO, NMT cieniowanie i hipsometria, K-GESUT, osnowy podstawowe i specjalne, INSPIRE, hydrografia i zagrożenie powodziowe PGW Wody Polskie, osuwiska SOPO, jaskinie, złoża MIDAS i geologia PIG-PIB, mapy hałasu i pożary lasów Ekoportalu, IMGW, GUS oraz kafelki XYZ z katalogu NextGIS QMS: ESRI, Sentinel-2 2024, OpenTopoMap, CyclOSM, Freemap.sk).
  * Bezpośredni dostęp online do oficjalnego wykazu Geoportal.gov.pl oraz rejestru EZiUDP z wyszukiwarką usług powiatowych i gminnych po nazwie lub kodzie TERYT.
  * Opcja wpisania własnego adresu URL serwera WMS/WMTS/XYZ (przycisk pod listą lub wklejenie linku w polu wyszukiwania) oraz możliwość powrotu z podglądu warstw do katalogu.
  * Warstwy zapisane kreatorem zachowują w grupie *Dodane serwery* i na liście wybranych warstw swoje czytelne nazwy.
* **Obsługa kafelków XYZ / TMS:**
  * Bezpośrednie pobieranie map z serwerów kafli `{z}/{x}/{y}` i TMS (`{-y}`), w tym wbudowane zbiory *OSMapa.pl*, *UMP-pcPL* i przezroczyste nakładki szlaków turystycznych *Waymarked Trails* (piesze, rowerowe, MTB, konne, rolkowe, narciarskie).
  * Rozpoznawanie szablonów `{z}/{x}/{y}`, parametrów `{quadkey}` i `{r}`, metadanych TileJSON oraz specyfikacji `tilemapresource.xml`.
  * Podkład dodany po nakładce automatycznie trafia na spód stosu warstw, a pobieranie samych nakładek w pełni obsługuje przezroczyste tło PNG.
  * Ostrzeżenie przed pobieraniem obszaru leżącego w całości poza zasięgiem mapy, inteligentne zastępowanie brakujących kafli (HTTP 404) powiększonym fragmentem z niższego poziomu powiększenia oraz natychmiastowe zatrzymanie z podpowiedzią przy odmowie dostępu (HTTP 403).
* **Drzewo warstw w grupach tematycznych i wyszukiwarka:**
  * Zbiory map z folderu `warstwy` pogrupowane w czytelnym drzewie według definicji w `warstwy/katalog.txt`.
  * Szybka wyszukiwarka warstw i grup tematycznych w panelu map (niewrażliwa na wielkość liter i polskie znaki diakrytyczne).
* **Nowy panel wybranych warstw i zaawansowana kompozycja:**
  * Warstwy z różnych serwerów WMS i WMTS można łączyć w jedną kompozycję („kanapkę”) – wybór kolejnej warstwy nie czyści już wcześniej zaznaczonych.
  * Nowe okno *Warstwy – zaawansowane* z suwakami krycia (0–100%), zmianą kolejności warstw, licznikiem zapytań i podglądem na żywo.
  * Automatyczne dobieranie rozmiaru piksela według najdokładniejszej z wybranych warstw.
  * Możliwość przeciągania myszą paska bocznego w celu poszerzenia panelu danych i wygodnego czytania długich nazw warstw.
  * **Bezpieczeństwo danych i ułatwienia sesji:**
    * Przycisk *Usuń pobrane* przenosi pliki do systemowego Kosza (zamiast bezpowrotnego kasowania) i dotyczy wyłącznie aktywnej sesji.
    * Żółta ikona folderu otwiera bezpośrednio folder bieżącej sesji.
    * Program ostrzega przed utratą przezroczystości w formacie JPEG przy warstwach o kryciu < 100% lub nakładkach i proponuje automatyczne przełączenie na format PNG.
    * Ochrona programów CAD przed zawieszeniem – ostrzeżenie i potwierdzenie przy próbie wygenerowania skryptu z ponad 1000 pojedynczych rastrów.
    * Diagnostyka WMS – czytelne wychwytywanie i raportowanie błędów serwera dotyczących nieobsługiwanego układu współrzędnych (CRS/SRS).
* **Pełna dokumentacja i pomoc online:**
  * 39 rozdziałów instrukcji obsługi w serwisie GitHub Pages z nowoczesnym motywem Just the Docs, ilustracjami, tabelami parametrów i wyszukiwarką pod adresem [eksploracja.github.io/MapoTero/pomoc/](https://eksploracja.github.io/MapoTero/pomoc/).
* **Testy jednostkowe:** 731 testów zakończonych sukcesem.

## Zmiany w wersji 3.16

* **Eksport do CAD** (menu *Narzędzia > Eksport do CAD*):
  * Rysunek DXF (R2000) z odnośnikami do rastrów na właściwych współrzędnych - segmenty sesji albo scalony arkusz (world file lub GeoTIFF); obrys, siatka i numery segmentów, trasa korytarza i opis mapy na warstwach `MAPOTERO_...`; ścieżki względne do rastrów.
  * Skrypt SCR dla AutoCAD, AutoCAD LT, BricsCAD i ZWCAD (polecenie `SCRIPT`) wstawiający mapę do otwartego rysunku pod istniejące obiekty, jako jedna operacja cofania.
  * Program LISP (polecenie `APPLOAD`; AutoCAD, BricsCAD, ZWCAD, AutoCAD LT od wersji 2024) - wstawia też numery segmentów i opis, nie zmienia układu UCS i przywraca ustawienia rysunku także po Esc.
  * Obrys samego zaznaczenia, gdy jest mniejsze od siatki; przycisk *scal teraz...* (złączenie segmentów w arkusz z plikiem world file) i *otwórz folder*; zapamiętywanie opcji eksportu.
* **Ochrona serwerów map przed przeciążeniem:**
  * Łączna liczba jednoczesnych zapytań do jednego serwera nie przekracza liczby wątków z ustawień - także przy pobieraniu kafli WMTS, które wysyłało dotąd do 4 zapytań na wątek.
  * Po odpowiedzi HTTP 429, 502, 503, 504 albo serii przekroczeń czasu program robi przerwę w zapytaniach do serwera (z uwzględnieniem nagłówka `Retry-After`) i zmniejsza liczbę jednoczesnych zapytań o połowę, a po udanych odpowiedziach stopniowo ją przywraca; informuje o tym w oknie programu.
  * Oczekiwanie w kolejce nie wlicza się do limitu czasu odpowiedzi serwera; przerwanie pobierania zatrzymuje też zapytania czekające w kolejce.
* **.NET 10:** program wymaga środowiska .NET Desktop Runtime 10 (x64) - wsparcie .NET 8 kończy się 10.11.2026. Z programu usunięto nieużywany klient MS SQL ze znanymi lukami bezpieczeństwa.
* **Obsługa awarii:** opis nieoczekiwanego błędu (wersja programu, system, stos wywołań) zapisywany jest w pliku `awarie.log` w folderze danych programu; okno komunikatu podaje adres zgłoszeń i pozwala kontynuować pracę.
* **Poprawki:** położenie arkuszy GeoTIFF zapisanych jako *PixelIsPoint* (przesunięcie o pół piksela).
* **Testy jednostkowe:** 638 testów zakończonych sukcesem.

## Zmiany w wersji 3.15

* **Kreator dodawania serwerów WMS i WMTS** (menu *Narzędzia* lub przycisk `[+]` przy liście map): analiza dokumentu `GetCapabilities` (WMS 1.3.0, WMS 1.1.1, WMTS 1.0.0), drzewo warstw z wyszukiwarką, zalecany rozmiar piksela ze skali minimalnej, sprawdzenie obsługiwanych układów, podgląd próbnego kafla i zapis nowego zbioru map do folderu `warstwy`; ponawianie chwilowych błędów HTTP i poprawne polskie znaki w dokumentach ISO-8859-2.
* **Siatka segmentów na mapie i zmiana zasięgu uchwytami:**
  * Po zaznaczeniu obszaru prawym przyciskiem myszy na mapie rysowana jest siatka segmentów (opcję można wyłączyć w ustawieniach).
  * Uchwyty w narożnikach i na środkach boków przesuwają krawędzie o pełne segmenty, uchwyt w środku przesuwa całą siatkę; Esc przywraca poprzedni zasięg.
* **Wielowarstwowy i wieloskalowy MBTiles** (menu *Narzędzia*): kompilacja kilku pobranych sesji do jednego pliku MBTiles - każda warstwa widoczna w wybranym zakresie powiększeń (np. z daleka topo 1:100k, w zbliżeniu topo 1:10k, najbliżej ortofotomapa) albo nałożona na inne z kryciem, trybem mieszania i odcieniami szarości; podgląd poziomu powiększenia i wbudowana pomoc.
* **Bezpieczniejsze pobieranie:**
  * W trakcie pobierania nie można zmienić warstw, dodać serwera, wczytać innej sesji, scalać ani nakładać map.
  * Błąd jednego segmentu (np. zapisu georeferencji) nie przerywa całego pobierania; obsługa obszarów do 10 mln segmentów, usuwanie starych plików `.tmp`.
  * Szybkie wyznaczanie korytarza trasy także przy małym segmencie.
  * Pobieranie innego obszaru do folderu z segmentami innej sesji proponuje nowy podfolder; przedrostek nazw segmentów nie może zawierać znaków niedozwolonych w nazwach plików.
* **Poprawki:**
  * Opcja *pobieraj tylko segmenty o numerze wyższym niż ostatni pobrany* działa zgodnie z opisem.
  * Scalanie zachowuje przezroczystość w PNG i GeoTIFF bezstratnym i ogranicza zużycie pamięci; poprawny plik `.wld` dla układu WGS84.
  * Usuwanie pustych segmentów z potwierdzeniem, do Kosza, z informacją o wyniku.
  * Zapamiętywanie opcji widoku okna głównego oraz przedrostka, formatu i przezroczystości nowej sesji; warstwy sesji z `conf.txt` nie znikają po ponownym uruchomieniu.
  * Eksport: średnik w ścieżce, stare pliki `-wal`; domyślny limit czasu odpowiedzi serwera 120 s.
* **Zbiory map:** zbiór *ortofotomapa* korzysta z usługi Geoportalu HighResolution, adresy zbiorów map z protokołem https.
* **Jeden plik programu:** program to pojedynczy plik `MapoTero.exe`, obok którego leży folder `warstwy`.
* **Testy jednostkowe:** 593 testy zakończone sukcesem.

## Zmiany w wersji 3.14

* **Obszar pobierania wzdłuż trasy (korytarz GPX/KML/KMZ):**
  * Nowa funkcja w menu *Sesja pobierania*: po wczytaniu pliku śladu (GPX, KML lub KMZ z Mapy.com, Komoot, Locus Map itp.) i zdefiniowaniu szerokości bufora, program pobiera wyłącznie segmenty przecinające korytarz trasy zamiast całego prostokąta otaczającego.
  * Wielokrotna oszczędność transferu i czasu przy trasach krętych lub ukośnych.
  * Wizualizacja trasy i wybranych segmentów korytarza bezpośrednio na mapie.
  * Zapis definicji korytarza w pliku sesji `korytarz.txt` i jego automatyczne wznawianie.
* **Przezroczyste nakładki i przezroczystość tła:**
  * Nowa opcja *przezroczyste tło* w ustawieniach (parametr `TRANSPARENT=TRUE` zapytań WMS dla formatów PNG/GIF).
  * Opcja *nakładka* w oknie eksportu (kafle PNG, typ `overlay` w MBTiles).
  * Opcja automatycznej zamiany jednolitego białego tła na przezroczyste dla serwerów WMS nieobsługujących kanału alfa.
* **Łączenie warstw podczas eksportu:**
  * Możliwość nakładania na eksportowaną mapę dodatkowych warstw z innych folderów sesji (np. cieniowanie rzeźby terenu lub granice działek nałożone na ortofotomapę) z płynną regulacją przezroczystości/krycia – także dla sesji pobranych w różnych układach współrzędnych.
* **Nowe formaty eksportu map mobilnych:**
  * **SQLiteDB:** format bazodanowy kafli RMaps (kompatybilny z aplikacjami Locus Map oraz OsmAnd).
  * **OruxMaps:** eksport do natywnego formatu offline OruxMaps (folder z bazą `OruxMapsImages.db` i plikiem kalibracji `.otrk2.xml`).
  * Automatyczne ustawianie flagi tylko do odczytu dla bezpiecznego otwierania baz kafli.
* **Rozmiar i opis mapy:**
  * Szacowanie wielkości wynikowego pliku mapy przed rozpoczęciem eksportu (próbne pobranie kafli) i dobór maksymalnego poziomu powiększenia mieszczącego się w zadanym limicie MB.
  * Zapis daty pobrania sesji i źródła danych w `conf.txt` oraz metadanych eksportu (nazwa, opis, atrybucja w MBTiles/KMZ).
  * Opcja *odśwież mapę*: ponowne pobranie wszystkich segmentów sesji z bezpiecznym zastąpieniem plików dopiero po pomyślnym pobraniu nowych.
* **Usprawnienia interfejsu (WinForms):**
  * Okno główne o zmiennym rozmiarze z automatycznym dopasowaniem do wysokości ekranu (rozciągana lista warstw WMS z przewijanym panelem).
  * Przywrócenie klasycznej czcionki Microsoft Sans Serif 8,25 pt (.NET Framework) dla maksymalnej czytelności na każdym ekranie.
  * Automatyczne oddalanie podglądu mapy do zasięgu siatki, gdy znaczniki narożników wykraczają poza okno.
* **Testy jednostkowe:**
  * Rozbudowa zestawu testów o weryfikację algorytmów korytarza GPX i nowych eksporterów (507 testów jednostkowych zakończonych sukcesem).

## Zmiany w wersji 3.13

* **Wydajność scalania segmentów:** eliminacja wielokrotnego odczytu i dekodowania plików kafli z dysku poprzez buforowanie wiersza segmentów w pamięci RAM; wielokrotne przyspieszenie łączenia arkuszy.
* **Eksport MBTiles:** naprawa powiązania transakcji SQLite z zapytaniami (rozwiązanie wyjątku `InvalidOperationException`), jednorazowa alokacja parametrów SQL, włączenie trybów `PRAGMA synchronous = OFF` oraz `WAL` dla szybkiego zapisu, buforowanie kodeka JPEG.
* **Odporność pobierania:** naprawa wznawiania sesji – brakujące segmenty są identyfikowane na podstawie fizycznego istnienia plików na dysku (brak pomijania kafli w przypadku restartu lub błędów sieci); atomowe przenoszenie plików tymczasowych.
* **Izolacja współbieżności WMTS:** token anulowania nie wpływa na współdzielone pobierania kafli w pamięci podręcznej.
* **Zarządzanie pamięcią i zasobami natywnymi:**
  * Eliminacja wycieków uchwytów plików KMZ (`FileStream`) w bloku `Try...Finally`.
  * Bezpieczne zamykanie i zwalnianie biblioteki LibTiff w eksporcie GeoTIFF.
  * Obsługa paletowych obrazów PNG (8-bit) przed nałożeniem przezroczystości `MakeTransparent`.
  * Poprawne zwalnianie zasobów GDI+ dla znaczników `GMarkerGoogle` przy odświeżaniu mapy i zamykaniu okna.
* **Stabilność interfejsu (WinForms):** zabezpieczenie asynchronicznych operacji w oknach scalania i eksportu przed `ObjectDisposedException` przy zamknięciu okna w trakcie pracy w tle.
* **Narzędzia i UI:**
  * Dokładne porównywanie rozmiaru plików w bajtach w oknie usuwania pustych segmentów (rozwiązanie problemu zaokrąglania przy ułamkowych progach kB) oraz eliminacja obciążenia UI przez buforowanie listy plików w RAM.
  * Nowe okno ustawień programu: bezpieczne stosowanie zmian przyciskiem OK, walidowane pola liczbowe, listy wyboru, ochrona przed utratą boku segmentu w trybie TrekBuddy.
  * Otwieranie forum pomocy w domyślnej przeglądarce internetowej systemu z protokołem HTTPS (zamiast przestarzałego silnika IE Trident).
* **Aktualizacja zbiorów map:** dodanie nowych warstw WMTS (m.in. BDOT10k, ortofotomapa wysokiej rozdzielczości, TrueOrtho) oraz uporządkowanie nieaktywnych źródeł w katalogu `_archiwalne`.
* **Testy jednostkowe:** rozszerzenie zestawu testów jednostkowych o testy scalania segmentów (482 testy zakończone sukcesem).

## Zmiany w wersji 3.12

* Przejście na platformę .NET 8 (program 64-bitowy) i aktualizacja bibliotek zależnych (GMap.NET, SQLite); automatyczna kompilacja i testy w GitHub Actions
* Pobieranie z usług WMTS: wystarczy podać w pliku zbioru map adres usługi WMTS (przykład: zbiór *ortofotomapa_WMTS*); program sam wybiera poziom kafli odpowiadający rozmiarowi piksela, a segmenty składa z kafli i w razie potrzeby przelicza do wybranego układu - mają te same pliki georeferencji co segmenty WMS
* Wybór układu współrzędnych segmentów: PL-1992, PL-2000 (strefy 5-8), UTM 33N-35N, WGS84 - współrzędne obszaru i rozmiar piksela przeliczane są przy zmianie układu, a pliki georeferencji zapisywane w wybranym układzie
* Wbudowane scalanie segmentów do GeoTIFF (także BigTIFF, kompresja Deflate lub JPEG), JPEG i PNG - zamiast zewnętrznego programu NoToCONS; arkusz zapisywany jest strumieniowo, więc jego wielkość nie jest ograniczona pamięcią RAM
* Eksport mapy (menu Narzędzia > Eksport mapy, Ctrl+E) do KMZ dla odbiorników Garmin (kafle do 1024 px, limit liczby kafli), KMZ dla Locus Map / OruxMaps i MBTiles z piramidą poziomów powiększenia
* Kilka segmentów pobieranych jednocześnie (liczba wątków w ustawieniach), przerywanie pobierania w dowolnej chwili, okno programu nie zamraża się podczas pobierania i scalania
* Parametr SERVICE=WMS dodawany do zapytań (wymagany przez serwery MapServer i GeoServer), kolejność osi w zapytaniach WMS 1.3.0 zgodna z definicją układu, a opcja zamiany X i Y dotyczy tylko zapytania - nie zamienia współrzędnych zapisywanych w plikach georeferencji
* Domyślny podkład mapy OpenStreetMap, ostre wyświetlanie okien na ekranach o dużej rozdzielczości (skalowanie DPI)
* Przeliczenia współrzędnych w jednej, dokładnej implementacji (szereg Krügera, zgodność z biblioteką PROJ do 1 mm) i testy jednostkowe biblioteki MapoTero.Core
* Uporządkowanie struktury projektu i usunięcie przestarzałego kodu v2
* Poprawki konfiguracji kompilacji i wsparcia dla nowoczesnych środowisk Visual Studio
* Naprawa ponawiania pobierania nieudanych segmentów (ustawienia "ilość prób" i "przerwa między próbami" nie były uwzględniane); okno programu nie zamraża się podczas oczekiwania na kolejną próbę
* Odporniejsze pobieranie segmentów: limit czasu połączenia, zapis obrazu bez ponownej kompresji JPEG, przyczyna błędu (np. komunikat serwera WMS) zapisywana w pliku error.txt
* Naprawa wznawiania pobierania przy numeracji segmentów NrWiersza_NrKolumny
* Naprawa wczytywania sesji z pliku conf.txt za jednym razem oraz zawieszania się programu przy starcie, gdy zapisany zbiór map nie istnieje
* Scalanie segmentów zgłasza sukces dopiero po faktycznym utworzeniu arkusza; pliki georeferencyjne scalonego arkusza zapisywane są obok niego, z poprawną nazwą i rozszerzeniem pliku
* Naprawa awarii przy tworzeniu plików .points dla formatu PNG oraz błędnych współrzędnych pikselowych w pliku .tab scalonego arkusza
* Nakładanie map: zwalnianie pamięci po każdym segmencie i obsługa plików PNG z paletą barw (png8)
* Usuwanie pustych segmentów: brak limitu 10 000 plików, usuwanie także plików .kml, .pngw i .png.points
* Gdy w katalogu programu nie można zapisywać (np. Program Files), ustawienia i pobrane mapy trafiają do %LocalAppData%\MapoTero
* Dokładna kalibracja plików KML: zapis dokładnych narożników obrazu (gx:LatLonQuad) oraz LatLonBox z obrotem uwzględniającym zbieżność południków - dawniej kafle na wschodzie i zachodzie Polski były w KML obrócone i przeskalowane (przesunięcie narożników kafla 1 km o ok. 40 m)
* Pliki world file (.jpgw, .pngw, ...) podają środek lewego górnego piksela zgodnie ze standardem (dawniej przesunięcie o pół piksela)
* Dokładny zasięg segmentów o niecałkowitym wymiarze terenowym (np. 0,1 m x 2048 px) i zapis liczb z kropką dziesiętną niezależnie od ustawień regionalnych systemu
* Usunięcie nieużywanych bibliotek (GMap.NET.WindowsPresentation, System.Text.Encoding.CodePages) i wersji testowych (preview) bibliotek
