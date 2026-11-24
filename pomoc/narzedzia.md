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

**Gdzie:** menu *Narzędzia > Szybki kreator mapy...*, skrót <kbd>Ctrl</kbd>+<kbd>K</kbd> albo przycisk **Szybki kreator…** na pasku nad mapą.

![Szybki kreator generowania mapy w 4 krokach]({{ '/pomoc/obrazy/16-szybki-kreator.png' | relative_url }})

*Rysunek 1. Szybki kreator generowania mapy prowadzi krok po kroku od wyboru celu i obszaru przez dobór szablonu po gotowy plik.*

Kreator prowadzi w czterech krokach od pomysłu do gotowego pliku mapy, bez ręcznego ustawiania układu współrzędnych, siatki segmentów i parametrów eksportu:

1. **Cel:** wybierz przeznaczenie mapy:
   * **Smartfon lub tablet (Locus Map, OsmAnd, OruxMaps):** plik MBTiles (albo SQLiteDB dla OsmAnd) z piramidą powiększeń do płynnego przybliżania i pracy offline,
   * **Odbiornik turystyczny lub zegarek Garmin (Fenix, GPSMAP, eTrex):** plik KMZ (Garmin Custom Maps) albo JNX (Garmin BirdsEye bez limitu 100 kafli),
   * **Arkusz graficzny do druku lub przeglądania na komputerze:** scalony arkusz GeoTIFF z georeferencją, ze wskazaniem rozmiaru wydruku (np. A4–A0),
   * **Systemy GIS i aplikacje mobilne (QGIS, QField, Avenza Maps):** arkusz GeoTIFF w układzie PL-1992 (albo baza kafelkowa GeoPackage `.gpkg`),
   * **Projekt CAD (AutoCAD, ZWCAD, BricsCAD):** podkład rastrowy w wybranym pasie PL-2000 (strefy 5–8) lub PL-1992 ze skryptem wpasowania (`.scr`).
2. **Obszar:** wskaż interesujący Cię teren:
   * Prostokąt zaznaczony bezpośrednio na mapie (jeśli zaznaczyłeś obszar na mapie przed otwarciem kreatora, ten krok jest pomijany automatycznie, a w każdej chwili wrócisz do niego klikając *Obszar* na pasku kroków),
   * Miejscowość, adres, współrzędne (WGS84, PL-1992 lub PL-2000) albo działka ewidencyjna z promieniem (w km) lub prostokątem według granic z rejestru ULDK GUGiK (z bezpiecznym marginesem),
   * Pas wzdłuż trasy wycieczki (plik GPX, KML, KMZ albo wektor GeoPackage `.gpkg` wybrany przyciskiem lub przeciągnięty na okno). Przycisk **Profil…** pozwala od razu obejrzeć profil wysokościowy wczytanego śladu.
3. **Szablon:** wybierz gotową kompozycję dopasowaną do celu i aktywności:
   * **Turystyka piesza:** mapa topograficzna GUGiK z cieniowaniem rzeźby terenu i szlakami pieszymi,
   * **Góry z poziomicami:** warstwice i cieniowanie OpenTopoMap (także dla Czech i Słowacji),
   * **Trasy rowerowe i MTB:** sieć szlaków rowerowych na podkładzie OSMapa z cieniowaniem rzeźby terenu,
   * **Dziedzictwo kulturowe:** rzeźba terenu LiDAR z prześwitującą mapą topograficzną i zabytkami Narodowego Instytutu Dziedzictwa (rejestr i ewidencja zabytków nieruchomych oraz stanowisk archeologicznych),
   * **Mapy WIG 1918–1939:** archiwalne arkusze Wojskowego Instytutu Geograficznego uzupełnione mapami 1:25 000 tam, gdzie brakuje arkuszy WIG,
   * **Mapy niemieckie do 1945:** *Messtischblatt* 1:25 000 i *Karte des Deutschen Reiches* 1:100 000,
   * **Działka i granice katastralne:** ortofotomapa z granicami i numerami działek EGiB (oraz warianty dla CAD/GIS),
   * **Moje warstwy z okna głównego:** kompozycja wybrana aktualnie w oknie głównym programu.
   * Suwaki w panelu po prawej regulują krycie warstw (np. cieniowania i szlaków), przycisk **Dostosuj warstwy...** otwiera pełny mikser, a przycisk **Zapisz jako szablon...** utrwala własny zestaw na liście szablonów (**Usuń szablon** go usuwa).
4. **Zapis:** sprawdzenie szacunków i generowanie mapy:
   * Wybierasz szczegółowość mapy, strukturę pliku (wielopoziomowa piramida powiększeń albo pojedynczy raster), folder docelowy oraz ewentualne opcje zaawansowane (korektę obrazu, własny zakres powiększeń, rozmiar piksela, układ CAD czy podział KMZ na tomy),
   * Program szacuje liczbę segmentów, rozmiar i czas,
   * Kliknięcie **Generuj mapę** pobiera dane i zapisuje wynik z szacowanym czasem do końca, a po ukończeniu udostępnia przyciski **Pokaż w folderze** oraz **Przejdź do mapy w oknie głównym**.

### Mapy wieloskalowe (Multi-Scale)
W szablonach turystycznych i historycznych formaty MBTiles, GeoPackage, SQLiteDB i JNX tworzone są jako **mapy wieloskalowe**:
* Przy dużym zbliżeniu widoczna jest mapa szczegółowa 1:10 000,
* W miarę oddalania widok płynnie przełącza się na mapy 1:25 000, 1:50 000, 1:100 000 aż po mapę przeglądową, bez zlewających się napisów.
* Opcja w kroku 4 pozwala dołączyć na dużym zbliżeniu ortofotomapę ze szlakami albo otworzyć pełne okno **MBTiles wieloskalowy**, jeśli chcesz ręcznie zmienić zakresy poziomów powiększeń.

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

Narzędzie przetwarza kalibrowane skany papierowych map turystycznych i archiwalnych (pliki `.map` wraz z rastrami JPG, PNG, TIF lub BMP) do nowoczesnych formatów mobilnych: **Garmin BirdsEye (JNX v4)**, **Garmin Custom Maps (KMZ)**, **MBTiles**, **SQLiteDB** oraz **OruxMaps**:
* **Automatyczna transformacja Datum (elipsoid historycznych):** program automatycznie wykrywa i przelicza historyczne elipsoidy odniesienia do współczesnej WGS84:
  * **Borowa Góra** (mapy Wojskowego Instytutu Geograficznego WIG 1:100 000 i 1:25 000),
  * **Potsdam / Rauenberg** (niemieckie mapy Messtischblatt 1:25 000 i Grossblatt),
  * **Pulkovo 1942 / Układ 1965 / Układ 1942** (mapy topograficzne z okresu PRL).
  Eliminuje to uciążliwe przesunięcie arkusza o 100–200 metrów względem śladu GPS.
* **Modele kalibracji nieliniowej:** dla dużych arkuszy stosowany jest model rzutowy (4–6 punktów) lub wielomian 2. stopnia (7+ punktów), korygujący zniekształcenia skanera papieru.
* **Pełna piramida powiększeń:** bazy JNX i MBTiles tworzone są z kompletem 5 poziomów skali.
* **Praca wsadowa (Batch):** po wskazaniu całego folderu program automatycznie przetwarza dziesiątki arkuszy jeden po drugim.

---

## Konwerter map GeoTIFF

**Gdzie:** menu *Narzędzia > Konwertuj mapę GeoTIFF...*

![Okno konwertera map GeoTIFF]({{ '/pomoc/obrazy/16-konwerter-geotiff.png' | relative_url }})

*Rysunek 5. Okno konwersji pliku GeoTIFF do formatów map mobilnych i odbiorników Garmin.*

Przekształca pojedynczy plik GeoTIFF (`.tif` lub `.tiff`, na przykład arkusz wyeksportowany wcześniej z MapoTero albo wyrenderowany w QGIS) w gotową mapę mobilną lub paczkę dla odbiornika Garmin:

* **Wczytywanie georeferencji:** program automatycznie odczytuje zasięg, rozdzielczość oraz układ współrzędnych z nagłówka GeoTIFF (albo z plików `.tfw` / `.prj`), a gdy brakuje kodu EPSG, pozwala wybrać układ z listy.
* **Obsługiwane formaty wynikowe:** **MBTiles** (Locus Map, OsmAnd), **GeoPackage (`.gpkg`)** (QField, Mergin Maps), **KMZ** (Garmin Custom Maps, z opcją podziału na tomy po 100 kafli), **JNX** (Garmin BirdsEye), **SQLiteDB** (OsmAnd) oraz **OruxMaps**.
* **Parametry konwersji:** ustawisz tu zakres poziomów powiększeń (**Zoom od–do**) i **jakość JPEG**.

{: .wskazowka }
Aby zapisać kafelkowany plik GeoTIFF z wewnętrznymi piramidami powiększeń (Cloud-Optimized GeoTIFF), wybierz w oknie [Eksport mapy]({{ '/pomoc/eksport/' | relative_url }}) format **Arkusz** i zaznacz opcję *piramidy w GeoTIFF*.

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

**Gdzie:** menu *Narzędzia > Kopiuj mapę do Garmina (USB)...* oraz przycisk **Wyślij na telefon...** w oknie eksportu po wygenerowaniu `.jnx` lub `.kmz`.

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
