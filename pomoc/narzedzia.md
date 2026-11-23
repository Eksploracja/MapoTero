---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Szybki kreator mapy, wyszukiwarka działek i miejscowości ULDK, transfer Wi-Fi z kodem QR, konwertery OziExplorer i GeoTIFF, profil wysokościowy NMT oraz korekta obrazów i podgląd Garmin MIP.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Szybki kreator mapy

**Gdzie:** menu *Narzędzia > Szybki kreator mapy...*, skrót **Ctrl+K** albo przycisk z różdżką **🪄 Szybki kreator...** na pasku nad mapą.

![Szybki kreator generowania mapy w 4 krokach]({{ '/pomoc/obrazy/16-szybki-kreator.png' | relative_url }})

*Rysunek 1. Szybki kreator generowania mapy prowadzi krok po kroku od wyboru celu po gotowy plik.*

Kreator prowadzi początkujących w czterech intuicyjnych krokach od pomysłu do gotowego pliku mapy – bez konieczności ręcznego ustawiania układu współrzędnych, siatki segmentów i parametrów eksportu:

1. **Cel** – wybierz przeznaczenie mapy:
   * **Smartfon lub tablet**: uniwersalny format MBTiles (dla Locus Map, OsmAnd, OruxMaps), nowoczesna baza OGC GeoPackage (.gpkg) lub SQLiteDB,
   * **Aplikacje terenowe GIS**: format OGC GeoPackage (.gpkg) dla QField, Mergin Maps oraz QGIS,
   * **Odbiornik Garmin**: format KMZ (dla Garmin Custom Maps) albo wielopoziomowy JNX (dla Garmin BirdsEye bez limitu 100 kafli),
   * **Wydruk na papierze**: scalony arkusz GeoTIFF ze wskazaniem wymiaru w centymetrach i najmniejszego formatu arkusza A (A4, A3, A2, A1, A0),
   * **Podkład CAD / GIS**: arkusz GeoTIFF w państwowym układzie PL-1992 do bezpośredniego wstawienia do projektu.
2. **Obszar** – wskaż interesujący Cię teren:
   * Miejscowość z promieniem w kilometrach,
   * Prostokąt otaczający miejscowość (*wg granic*),
   * Wklejone współrzędne punktu (np. `53.6536, 15.8058` z Google Maps, `53°39'13"N 15°48'21"E` lub metry w PL-1992/PL-2000),
   * Działka katastralna: identyfikator TERYT (np. `320302_5.0001.123/4`) lub nazwa „działka Drawsko Pomorskie 123/4” – program pobiera urzędowy obrys z bazy ULDK GUGiK i dodaje bezpieczny margines 150 m,
   * Pas wzdłuż trasy wycieczki (plik GPX, KML, KMZ lub wektor GeoPackage .gpkg przeciągnięty na okno),
   * Jeśli przed otwarciem kreatora zaznaczysz obszar na mapie prawym przyciskiem myszy, kreator automatycznie pominie ten krok. Przycisk **Profil…** pozwala od razu obejrzeć profil wysokościowy wczytanego śladu.
3. **Styl** – gotowa kompozycja kartograficzna dopasowana do aktywności:
   * **Turystyka piesza**: szczegółowa mapa topograficzna GUGiK z cieniowaniem LiDAR i szlakami turystycznymi,
   * **Góry z poziomicami**: warstwice i cieniowanie OpenTopoMap z pokryciem Polski, Czech i Słowacji,
   * **Trasy rowerowe i MTB**: dedykowana sieć szlaków rowerowych z rzeźbą terenu,
   * **Dziedzictwo kulturowe**: rzeźba terenu LiDAR z prześwitującą mapą topograficzną i zabytkami Narodowego Instytutu Dziedzictwa (rejestr i ewidencja zabytków nieruchomych oraz stanowisk archeologicznych),
   * **Mapy WIG 1918–1939**: archiwalne arkusze Wojskowego Instytutu Geograficznego uzupełnione Messtischblattami tam, gdzie brakowało map 1:25 000,
   * **Mapy niemieckie do 1945**: Messtischblatt 1:25 000 i Karte des Deutschen Reiches 1:100 000,
   * **Działki ewidencyjne**: granice i numery działek z ewidencji gruntów i budynków (EGiB) na ortofotomapie,
   * **Własne warstwy**: kompozycja warstw aktualnie wybrana w oknie głównym programu.
   * Suwaki pozwalają regulować krycie podkładu, szlaków i cieniowania. Przycisk **Zapisz jako mój styl…** umożliwia utrwalenie własnej kompozycji na stałe w kategorii *Moje style*.
4. **Zapis** – generowanie mapy:
   * Program automatycznie dobiera właściwy rozmiar piksela, format oraz szacuje objętość pliku w MB i czas pobierania,
   * W opcjach zaawansowanych dostępne jest okno korekty obrazu z gotowymi profilami (np. *Garmin MIP* dla zegarków i nawigacji),
   * Kliknięcie **Generuj mapę** pobiera dane w tle ze wskaźnikiem rzeczywistego czasu do końca, a po ukończeniu przycisk **Otwórz mapę** natychmiast wyświetla wynik w domyślnym programie.

### Mapy wieloskalowe (Multi-Scale)
W stylach *Turystyka piesza*, *Góry z poziomicami*, *Rower i MTB* oraz stylach historycznych formaty MBTiles, GeoPackage, SQLiteDB i JNX tworzone są jako **mapy wieloskalowe**:
* Przy dużym zbliżeniu widoczna jest mapa szczegółowa 1:10 000,
* W miarę oddalania widok płynnie przełącza się na mapy 1:25 000, 1:50 000, 1:100 000 aż po mapę przeglądową, eliminując nieczytelne, zlane napisy.
* Opcja **Ortofotomapa przy zbliżeniu** w kroku 4 dodaje na najwyższym poziomie zoomu (powiększenie 17, piksel 0,75 m) ortofotomapę lotniczą ze szlakami, odsłaniając ścieżki i polany niewidoczne na mapach topograficznych.

---

## Wyszukiwarka miejscowości, adresów i działek ULDK

**Gdzie:** menu *Narzędzia > Wyszukaj miejscowość lub działkę...*, skróty **Ctrl+F** (miejscowość/adres) i **Ctrl+D** (działka) lub pole wyszukiwania na pasku nad mapą.

![Wyszukiwarka miejscowości i działek katastralnych ULDK]({{ '/pomoc/obrazy/16-szukaj-dzialki.png' | relative_url }})

*Rysunek 2. Okno zintegrowanej wyszukiwarki miejscowości, adresów i działek ewidencyjnych ULDK.*

* **Miejscowość / Adres**: korzysta z geokodera OpenStreetMap Nominatim. Wpisz dowolne miasto, wieś, osadę lub konkretny adres z numerem domu. Program natychmiast centruje mapę podglądu z optymalnym powiększeniem.
* **Działka katastralna (ULDK)**: bezpośrednia integracja z krajową usługą Usługi Lokalizacji Działek Katastralnych (GUGiK).
  * Wyszukiwanie po pełnym identyfikatorze TERYT (np. `320302_5.0001.123/4`) lub po nazwie województwa, powiatu, gminy, obrębu i numerze działki,
  * Program pobiera oficjalną geometrię wektorową działki z państwowego rejestru, rysuje jej kontur na mapie i ustawia siatkę pobierania z regulowanym buforem (np. 50 m, 150 m),
  * Przycisk jednym kliknięciem aktywuje ortofotomapę wysokiej rozdzielczości oraz warstwę granic i numerów działek EGiB.

---

## Przesyłanie mapy na telefon przez Wi-Fi i kod QR

**Gdzie:** menu *Narzędzia > Prześlij mapę na telefon (Wi-Fi)...* oraz przycisk **Wyślij na telefon...** w oknie eksportu mapy.

![Bezpośredni transfer mapy na telefon przez sieć Wi-Fi i kod QR]({{ '/pomoc/obrazy/16-wyslij-telefon.png' | relative_url }})

*Rysunek 3. Wbudowany serwer Wi-Fi wyświetla kod QR umożliwiający natychmiastowe pobranie mapy na smartfon.*

Narzędzie rozwiązuje problem niewygodnego podłączania kabli USB i wysyłania wielogigabajtowych map do chmury:
1. Po wygenerowaniu pliku `.mbtiles`, `.gpkg`, `.kmz`, `.tif`, `.sqlitedb` lub `.tar` program uruchamia lekki serwer w lokalnej sieci Wi-Fi i wyświetla na ekranie duży kod QR (własny generator bez zewnętrznych zależności).
2. Wystarczy skierować aparat telefonu na ekran komputera i dotknąć powiadomienia, aby otworzyć bezpieczną stronę transferu.
3. Jedno kliknięcie na telefonie pobiera mapę wprost do pamięci urządzenia (Locus Map, QField, OsmAnd, OruxMaps, Avenza Maps).
4. Serwer obsługuje wznawianie przerwanych transferów (nagłówek HTTP Range), automatycznie wybiera kartę Wi-Fi przed połączeniami VPN i zabezpiecza transmisję jednorazowym tokenem kryptograficznym.

---

## Konwerter map OziExplorer (.map)

**Gdzie:** menu *Narzędzia > Konwertuj mapę OziExplorer (.map)...* albo przeciągnięcie pliku `.map` lub całego folderu na okno programu (Drag & Drop).

![Konwerter kalibrowanych map OziExplorer]({{ '/pomoc/obrazy/16-konwerter-ozi.png' | relative_url }})

*Rysunek 4. Konwerter kalibrowanych map OziExplorer do formatów mobilnych i GPS.*

Narzędzie przetwarza kalibrowane skany papierowych map turystycznych i archiwalnych (pliki `.map` wraz z rastrami JPG, PNG, TIF lub BMP) do nowoczesnych formatów mobilnych: **Garmin BirdsEye (JNX v4)**, **Garmin Custom Maps (KMZ)**, **MBTiles**, **OGC GeoPackage (.gpkg)**, **SQLiteDB** oraz **OruxMaps**:
* **Automatyczna transformacja Datum (elipsoid historycznych):** program automatycznie wykrywa i przelicza historyczne elipsoidy odniesienia do współczesnej WGS84:
  * **Borowa Góra** (mapy Wojskowego Instytutu Geograficznego WIG 1:100 000 i 1:25 000),
  * **Potsdam / Rauenberg** (niemieckie mapy Messtischblatt 1:25 000 i Grossblatt),
  * **Pulkovo 1942 / Układ 1965 / Układ 1942** (mapy topograficzne z okresu PRL).
  Eliminuje to uciążliwe przesunięcie arkusza o 100–200 metrów względem śladu GPS.
* **Modele kalibracji nieliniowej:** dla dużych arkuszy stosowany jest model rzutowy (4–6 punktów) lub wielomian 2. stopnia (7+ punktów), korygujący zniekształcenia skanera papieru.
* **Pełna piramida powiększeń:** bazy JNX, MBTiles i GeoPackage tworzone są z kompletem 5 poziomów skali.
* **Praca wsadowa (Batch):** po wskazaniu całego folderu program automatycznie przetwarza dziesiątki arkuszy jeden po drugim.

---

## Konwerter GeoTIFF (kafelkowanie i piramidy)

**Gdzie:** menu *Narzędzia > Konwertuj mapę GeoTIFF...*

![Konwerter GeoTIFF]({{ '/pomoc/obrazy/16-konwerter-geotiff.png' | relative_url }})

*Rysunek 5. Narzędzie optymalizacji arkuszy GeoTIFF do standardu Cloud-Optimized GeoTIFF.*

Przekształca tradycyjne, ciężkie arkusze GeoTIFF zapisane wierszami (strip-based) w nowoczesne, kafelkowane pliki o strukturze **Cloud-Optimized GeoTIFF**:
* **Kafelkowanie wewnętrzne 256×256 px**: podział obrazu na miniaturowe kafle wewnątrz pliku TIFF,
* **Łańcuch piramid pomniejszeń (IFD reduced overviews)**: podkatalogi pomniejszeń umożliwiające błyskawiczne renderowanie mapy przy dowolnym oddaleniu widoku w QGIS, ArcGIS czy Avenza Maps bez konieczności dekompresowania wielogigabajtowego pliku z dysku,
* **Wybór kompresji**: bezstratna kompresja Deflate (zoptymalizowana pod rysunki CAD i mapy wektorowe) lub kompresja JPEG z regulacją jakości (dla ortofotomap i skanów papierowych).

---

## Profil wysokościowy trasy

**Gdzie:** menu *Narzędzia > Profil wysokościowy trasy (GPX, KML)...*

![Profil wysokościowy trasy z NMT LIDAR]({{ '/pomoc/obrazy/16-profil-nmt.png' | relative_url }})

*Rysunek 6. Profil wysokościowy trasy ze skaningu laserowego NMT LIDAR GUGiK.*

1. Przyciskiem **Wybierz...** wskaż plik trasy wycieczki (GPX, KML, KMZ lub warstwę liniową wektora GeoPackage `.gpkg`).
2. Kliknij **Pobierz wysokości z NMT**. Program łączy się z państwową usługą wysokościową NMT GUGiK (lotniczy skaning laserowy LIDAR) i pobiera rzeczywiste rzędne terenu dla każdego punktu trasy (dla obszaru Polski).
3. Wykres prezentuje profil podłużny terenu wraz z kluczowymi statystykami: wysokością minimalną i maksymalną, dystansem oraz sumą podejść i zejść.
4. Przycisk **Zapisz GPX z wysokościami...** tworzy wzbogacony plik GPX ze znacznikami `<ele>`, idealny do wgrania do zegarków sportowych (Garmin, Suunto, Coros) i odbiorników turystycznych.

---

## Korekta obrazów w folderze

**Gdzie:** menu *Narzędzia > Korekta obrazów w folderze...* albo przycisk w oknie [Eksport do CAD](../cad/).

![Korekta i optymalizacja obrazów w folderze]({{ '/pomoc/obrazy/16-korekta-folderu.png' | relative_url }})

*Rysunek 7. Wsadowa korekta i optymalizacja kontrastu, gamma i wyostrzania dla całego katalogu obrazów.*

Narzędzie przetwarza wsadowo wszystkie pliki rastrowe (PNG, JPEG, TIFF) znajdujące się w wybranym folderze:
1. Wskaż folder źródłowy oraz docelowy (domyślnie podkatalog `korekta`),
2. Przyciskiem **Korekta obrazu...** wybierz dedykowany profil:
   * **Garmin MIP**: optymalizacja kontrastu i nasycenia pod ekrany transrefleksyjne,
   * **Wyostrzenie krawędzi**: filtr splotowy Laplaciana 3×3 podkreślający drogi, poziomice i napisy,
   * **Stare mapy i skany**: redukcja zażółceń papieru i podbicie czytelności tuszu,
   * **Rozjaśnienie lasów**: redukcja ciemnej zieleni utrudniającej odczyt warstwic,
   * **Podkład CAD / Skan CAD**: konwersja pod kątem czytelnego podkładu pod wektoryzację.
3. Opcja *Kopiuj pliki georeferencji* dba o to, by obok poprawionych obrazów znalazły się wszystkie towarzyszące pliki kalibracji (`.tfw`, `.jpgw`, `.pngw`, `.map`, `.prj`, `.tab`, `.kml`) oraz `conf.txt` sesji.

---

## Podgląd MIP (ekrany Garmin Memory-In-Pixel)

**Gdzie:** okno [Warstwy - zaawansowane](../okno-glowne/#okno-warstwy---zaawansowane-mikser), [Eksport mapy](../eksport/) oraz [Szybki kreator mapy](#szybki-kreator-mapy).

![Podgląd MIP dla odbiorników Garmin]({{ '/pomoc/obrazy/10-podglad-mip.png' | relative_url }})

*Rysunek 8. Moduł podglądu post-processingu MIP z symulacją czytelności w ostrym świetle słonecznym.*

Ekrany transrefleksyjne Garmin Memory-In-Pixel (seria Fenix, Epix, Forerunner, Edge, GPSMAP, Montana) doskonale spisują się w słońcu, lecz standardowe rastry komputerowe wyglądają na nich blado. Moduł MIP stosuje zoptymalizowaną krzywą gamma, podbicie nasycenia barw oraz filtr wyostrzający krawędzie bez artefaktów na brzegach kafelków, gwarantując maksymalną czytelność mapy w trudnych warunkach terenowych.

---

## Kopiowanie mapy do odbiornika Garmin (USB)

**Gdzie:** menu *Narzędzia > Kopiuj mapę do Garmina (USB)...* oraz przycisk w oknie eksportu mapy.

Automatycznie skanuje podłączone do komputera napędy wymienne, wykrywa odbiorniki turystyczne Garmin pracujące w trybie pamięci masowej USB (oraz karty microSD) i jednym kliknięciem kopiuje wygenerowany plik `.kmz` lub `.jnx` bezpośrednio do właściwego folderu systemowego:
* pliki KMZ trafiają automatycznie do `Garmin/CustomMaps`,
* pliki JNX (BirdsEye) trafiają do `Garmin/BirdsEye`.

Gdy odbiornik pracuje w nowszym trybie MTP (widocznym w Windows jako odtwarzacz przenośny), program wyświetla czytelną instrukcję, jak przełączyć urządzenie w tryb pamięci masowej (*Konfiguracja > System > Interfejs > Garmin Spanning / Pamięć masowa*).

---

## Usunięte narzędzia

W ramach porządkowania architektury programu w nowszych wersjach usunięto przestarzałe narzędzia, których funkcje przejęły nowocześniejsze moduły:
* **Usuwanie pustych segmentów**: silnik pobierania automatycznie wykrywa i ponawia białe segmenty, eksport pomija puste kafle, a korytarz trasy nie generuje segmentów poza pasem. Dawne narzędzie kasowało pliki po rozmiarze, co mogło uszkodzić jasne mapy ewidencyjne.
* **Nakładanie dwóch map PNG na siebie**: zastąpione wielowarstwową kompozycją w oknie głównym oraz obsługą nakładek w oknie eksportu.
* **Osobny tryb TrekBuddy w Ustawieniach**: paczki `.tar` generuje teraz bezpośrednio uniwersalne okno eksportu mapy.

---

➡️ **Kolejny krok:** [Katalog zbiorów map dostarczanych z programem →](../zbiory-map/)

*Zobacz też: [Błędy pobierania](../pobieranie/bledy/) · [Nakładki i łączenie warstw](../eksport/nakladki/) · [Sesje pobierania](../sesje/)*
