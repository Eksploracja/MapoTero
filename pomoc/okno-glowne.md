---
title: Okno główne
nav_order: 5
---

# Okno główne
{: .no_toc }

Opis każdego elementu nowoczesnego okna głównego: paska narzędziowego, mapy podglądu, kart obszaru i parametrów pod mapą, katalogu map, warstw oraz panelu podglądu z lupą.
{: .fs-5 .fw-300 }

**Gdzie w programie:** okno, które pojawia się po uruchomieniu programu  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Widok całego okna

![Okno główne programu MapoTero z ponumerowanymi elementami]({{ '/pomoc/obrazy/03-okno-glowne.png' | relative_url }})

*Rysunek 1. Nowy, czytelny układ okna głównego programu MapoTero z ponumerowanymi sekcjami.*

| Nr | Element okna | Do czego służy |
|---|---|---|
| **1** | Pasek menu | menu *Sesja pobierania* (nowa sesja, zapis/odczyt `conf.txt`, obszar wzdłuż trasy i z poligonu, odświeżanie mapy), menu *Narzędzia* (szybki kreator mapy, wyszukiwarka miejscowości i działek ULDK, konwerter OziExplorer, konwerter GeoTIFF, profil wysokościowy NMT, scalanie i eksport mapy, eksport do CAD, korekta folderu, transfer Wi-Fi i USB Garmin, okno Ustawienia) oraz menu *Pomoc* (podręcznik online, galeria warstw map online, wbudowana instrukcja, zgłaszanie błędów, folder danych) |
| **2** | Pasek narzędziowy nad mapą | trzy logiczne grupy: **Nowa sesja** i **🪄 Szybki kreator...** (<kbd>Ctrl</kbd>+<kbd>K</kbd>), przełącznik **Zaznacz obszar** z zintegrowaną wyszukiwarką **🔍 Szukaj miejsca lub działki...** (<kbd>Ctrl</kbd>+<kbd>F</kbd> / <kbd>Ctrl</kbd>+<kbd>D</kbd>), a po prawej przycisk **Galeria warstw**, zwijanie panelu oraz **📁 Folder pobrań** |
| **3** | Mapa podglądu | interaktywny podkład (OpenStreetMap / Google / Bing) do wyboru obszaru, siatka segmentów z uchwytami regulacji, powiększone przyciski **+** i **-** w narożniku oraz podziałka liniowa w prawym dolnym rogu mapy |
| **4** | Drzewo *Wybierz mapy* | tematyczny katalog zbiorów i warstw, szybki filtr *szukaj warstwy…* (działa bez polskich znaków i wielkości liter), przyciski rozwijania `⊞`/`⊟` oraz przycisk `+ Serwer…` otwierający kreator usług WMS/WMTS/XYZ/TMS |
| **5** | *Wybrane warstwy oraz ich kolejność* | stos wybranych warstw (do 12) z dopiskiem *↑ na wierzchu*; regulacja `Krycie %` suwakiem bezpośrednio w wierszu zaznaczonej warstwy, szybkie usuwanie `✕`, zmiana kolejności przeciąganiem myszą lub <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd> oraz przycisk **Mikser…** |
| **6** | Karta *Obszar* pod mapą | wymiary terenu (np. 8 × 8 km), powierzchnia w km² i liczba segmentów siatki; kliknięcie wymiaru pokazuje obszar na mapie, a odnośnik **Zmień ▾** otwiera menu: *Pokaż na mapie*, *Zaznacz na mapie*, *Wpisz współrzędne…*, *Obszar wzdłuż trasy…* oraz *Obszar z poligonu…* |
| **7** | Karta *Parametry segmentów* | podsumowanie parametrów pobierania (układ współrzędnych np. PL-1992, bok segmentu np. 2000 px, format JPEG, terenowy rozmiar piksela np. 2,00 m/pix); odnośnik **Zmień…** pozwala szybko dostosować te wartości |
| **8** | Karta *Stan sesji* i pobieranie | dynamiczny przycisk **Pobierz segmenty** (wyróżniony kolorem, gdy sesja jest gotowa do startu), w trakcie pracy **Przerwij pobieranie**, a po skompletowaniu segmentów **Eksportuj pobraną mapę…**; ikona stanu pokazuje strzałkę, łuk postępu, zielony znak ✓ lub czerwony wykrzyknik |
| **9** | Zakładki prawego panelu | trzy zakładki: **Podgląd** (próbka złożenia warstw na żywo ze środka siatki, powiększenie %, przyciski `1:1`, `-`, `+`, `Dopasuj` oraz `Mikser warstw i krycia...`), **Komunikaty** (dziennik zdarzeń; czerwona z licznikiem przy błędach) oraz **Sesja** (szczegóły techniczne i ścieżki) |
| **10** | Pasek stanu | układ współrzędnych sesji, dokładne współrzędne kursora w tym układzie (X = północ, Y = wschód), przybliżona skala mapy na ekranie oraz nazwa sesji z kolorową diodą stanu (kliknięcie otwiera zakładkę komunikatów) |

Układ okna dzieli się na dwie główne kolumny:
* **po lewej**: mapa podglądu z górnym paskiem narzędzi oraz trzema ergonomicznymi kartami pod mapą (*Obszar*, *Parametry segmentów*, *Stan sesji*),
* **po prawej**: regulowany panel *Dane źródłowe* z katalogiem map, listą wybranych warstw i zakładkami podglądu/komunikatów. Wysokość sekcji w prawym panelu można płynnie regulować, przeciągając uchwyt `═` między kartami (dwuklik przywraca domyślny podział).

Program uruchamia się domyślnie w pełnym oknie zmaksymalizowanym, zapewniając optymalny obszar roboczy.

---

## Pasek menu

| Menu | Polecenie | Skrót | Opis |
|---|---|---|---|
| **Sesja pobierania** | Nowa sesja | | czyści wybrane warstwy, zasięg, zaznaczenie i siatkę, obszar wzdłuż trasy i kontur działki, przywracając domyślny stan programu (zob. [Sesje pobierania](../sesje/)) |
| | Zapisz ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>Z</kbd> | zapisuje parametry bieżącej sesji we wskazanym folderze. Wybrany folder staje się folderem sesji |
| | Wczytaj ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>W</kbd> | wczytuje sesję ze wskazanego folderu (wymaga pliku `conf.txt`) |
| | Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)... | | [Obszar wzdłuż trasy](../obszar/korytarz/) – pobieranie segmentów tylko w zadanym pasie wzdłuż śladu wycieczki |
| | Obszar pobierania z poligonu (KML, GeoJSON, GPKG)... | | [Obszar z poligonu](../obszar/zaznaczanie/#obszar-z-poligonu-kml-geojson-geopackage) – automatyczny prostokąt wokół wektora |
| | Usuń obszar wzdłuż trasy | | usuwa korytarz i wraca do pobierania pełnego prostokąta |
| | Odśwież mapę (pobierz ponownie wszystkie segmenty) | | [Odświeżanie mapy](../pobieranie/odswiezanie/) – bezpieczne dociąganie nowych kafelków |
| **Narzędzia** | Szybki kreator mapy... | <kbd>Ctrl</kbd>+<kbd>K</kbd> | [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy) – przygotowanie gotowej mapy w 4 krokach od celu do pliku |
| | Dodaj nowy serwer WMS / WMTS... | | [Kreator serwerów](../serwery/kreator/) – dodawanie usług WMS, WMTS oraz kafelków XYZ/TMS |
| | Wyszukaj miejscowość lub działkę... | <kbd>Ctrl</kbd>+<kbd>F</kbd> | [Wyszukiwarka miejscowości i działek ULDK](../obszar/zaznaczanie/#wyszukiwarka-miejscowości-adresów-i-działek-katastralnych) (<kbd>Ctrl</kbd>+<kbd>D</kbd> dla działek) |
| | Konwertuj mapę OziExplorer (.map)... | | [Konwerter map OziExplorer](../narzedzia/#konwerter-map-oziexplorer-map) – kalibrowane skany do JNX, KMZ, MBTiles i GeoPackage |
| | Konwertuj mapę GeoTIFF... | | [Konwerter GeoTIFF](../narzedzia/#konwerter-geotiff-kafelkowanie-i-piramidy) – kafelkowanie wewnętrzne 256×256 i generowanie piramid |
| | Profil wysokościowy trasy (GPX, KML)... | | [Profil wysokościowy trasy](../narzedzia/#profil-wysokościowy-trasy) – rzędne z laserowego skaningu NMT/LIDAR i wzbogacanie GPX |
| | Scalanie segmentów | <kbd>Ctrl</kbd>+<kbd>L</kbd> | okno [Eksport mapy](../eksport/) z wybranym formatem *Arkusz* – [Scalanie segmentów](../scalanie/) |
| | Eksport mapy (JNX, KMZ, MBTiles, GeoPackage, SQLiteDB, OruxMaps, arkusz, TrekBuddy) | <kbd>Ctrl</kbd>+<kbd>E</kbd> | [Eksport mapy](../eksport/) do formatów mobilnych i arkuszy |
| | Eksport do CAD (DXF, skrypt SCR, LISP)... | | [Eksport do CAD](../cad/) – rysunek DXF, skrypt SCR i program LISP dla programów CAD |
| | Korekta obrazów w folderze... | | [Korekta obrazów w folderze](../narzedzia/#korekta-obrazów-w-folderze) – wsadowa optymalizacja kontrastu, gamma i filtr MIP |
| | Prześlij mapę na telefon (Wi-Fi)... | | [Transfer mapy na telefon](../narzedzia/#przesyłanie-mapy-na-telefon-przez-wi-fi-i-kod-qr) – lokalny serwer Wi-Fi i kod QR |
| | Kopiuj mapę do Garmina (USB)... | | [Kopiowanie do Garmina](../narzedzia/#kopiowanie-mapy-do-odbiornika-garmin-usb) – wykrywanie odbiornika w trybie pamięci masowej |
| | Ustawienia | | okno [Ustawienia programu](../ustawienia/) |
| **Pomoc** | Podręcznik użytkownika (online) | <kbd>F1</kbd> | otwiera ten podręcznik w przeglądarce internetowej |
| | Galeria warstw map (podgląd online) | | [Galeria warstw map online](../galeria-warstw/) z bezpośrednim dodawaniem warstw do programu |
| | Krótka instrukcja (bez internetu) | | skrócona instrukcja wbudowana w plik programu |
| | Zadaj pytanie na forum (PFE) | | oficjalny dział programu na Pomorskim Forum Eksploracyjnym |
| | Zgłoś błąd lub pomysł (GitHub) | | strona zgłoszeń błędów i propozycji nowych funkcji na GitHubie |
| | Otwórz folder danych programu | | folder z `lastsettings.txt`, pobranymi mapami i plikiem `awarie.log` |
| | Co nowego w programie | | historia wydań i nowości |
| | Wesprzyj rozwój programu | | postaw symboliczną kawę autorowi na [buycoffee.to/mapy](https://buycoffee.to/mapy) |
| | O programie | | informacja o licencji GPLv3, wersji i autorach |

Każde samodzielne okno narzędziowe w programie posiada dedykowany przycisk **Pomoc** (oraz klawisz <kbd>F1</kbd>), otwierający podręczną instrukcję z aktywnym odnośnikiem do odpowiedniego rozdziału podręcznika online.

---

## Pasek narzędziowy nad mapą

Pasek nad mapą grupuje najważniejsze funkcje w trzech przejrzystych blokach:

1. **Sesja i kreator:**
   * **Nowa sesja** (kartka z zielonym plusem): jednym kliknięciem czyści obszar roboczy i przygotowuje program do pobrania nowej mapy w nowym podfolderze.
   * **🪄 Szybki kreator...** (<kbd>Ctrl</kbd>+<kbd>K</kbd>): prowadzi użytkownika krok po kroku przez przygotowanie mapy (wybór urządzenia, obszaru, stylu i zapisu).
2. **Wybór obszaru i wyszukiwarka:**
   * **Zaznacz obszar**: włącza tryb zaznaczania prostokąta na mapie.
   * **🔍 Szukaj miejsca lub działki...**: pole wyszukiwania z autouzupełnianiem miejscowości i adresów (OpenStreetMap Nominatim) oraz działek katastralnych ULDK (GUGiK). Wpisz np. *Drawsko Pomorskie*, *Łabędzie 1* albo identyfikator działki (np. *działka Wieliczka 123/4*) i naciśnij <kbd>Enter</kbd> – mapa wycentruje się, a program automatycznie ustawi siatkę pobierania. Skrót <kbd>Ctrl</kbd>+<kbd>F</kbd> przenosi kursor wprost do tego pola.
3. **Akcje pomocnicze:**
   * **Galeria warstw**: otwiera w przeglądarce [galerię warstw online](../galeria-warstw/), pozwalającą jednym kliknięciem dodawać sprawdzone warstwy bezpośrednio do działającego MapoTero.
   * **Zwiń / Rozwiń**: ukrywa lub przywraca prawy panel warstw, maksymalizując widok mapy.
   * **📁 Folder pobrań**: otwiera folder `download` w Eksploratorze Windows.

---

## Mapa podglądu

Interaktywna mapa podglądu służy do lokalizacji w terenie i precyzyjnego wyznaczania zasięgu siatki segmentów:

| Czynność | Sposób wykonania |
|---|---|
| przesuwanie mapy | przeciągnij lewym przyciskiem myszy |
| przybliżanie i oddalanie | kółko myszy, duże przyciski **+** i **-** w narożniku mapy, dwuklik lewym (przybliża) lub prawym (oddala) |
| zaznaczanie obszaru | przeciągnij **prawym** przyciskiem myszy – zobacz [Zaznaczanie na mapie](../obszar/zaznaczanie/) |
| regulacja zasięgu siatki | przeciągnij uchwyty w narożnikach, na środkach boków lub uchwyt centralny siatki |
| anulowanie zaznaczania | naciśnij klawisz <kbd>Esc</kbd> |

**Wybór podkładu:** przełączniki radiowe w górnej części mapy pozwalają przełączać podkłady orientacyjne: *OpenStreetMap* (domyślny), *GoogleMap* oraz *SatelliteBingsMap* (zdjęcia satelitarne). Program pamięta Twój wybór po ponownym uruchomieniu.

**Podziałka liniowa:** w prawym dolnym rogu mapy widoczna jest czytelna podziałka metryczna ułatwiająca ocenę odległości w terenie.

---

## Karty pod mapą

Pod mapą znajdują się trzy karty w jednolitym, przejrzystym stylu:

### 1. Karta *Obszar*
* Pokazuje rzeczywisty wymiar terenowy siatki (np. *8,0 × 8,0 km*), powierzchnię (np. *64,0 km²*) oraz liczbę segmentów (*4 kolumny × 4 wiersze = 16 segmentów*).
* Kliknięcie wymiaru obszaru centruje mapę na siatce pobierania.
* Odnośnik **Zmień ▾** rozwija menu szybkiej modyfikacji obszaru:
  * **Pokaż na mapie**: centruje widok na obszarze,
  * **Zaznacz na mapie**: uaktywnia zaznaczanie prostokąta,
  * **Wpisz współrzędne…**: otwiera okno ręcznego wprowadzania współrzędnych X/Y,
  * **Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)…**: wyznacza segmenty wzdłuż śladu wycieczki,
  * **Obszar pobierania z poligonu (KML, GeoJSON, GPKG)…**: dopasowuje siatkę do obrysu wektora.

### 2. Karta *Parametry segmentów*
* Przedstawia kluczowe parametry techniczne: **układ współrzędnych** (np. *PL-1992*, *PL-2000*, *UTM*, *WGS84*), **bok segmentu** (np. *2000 px*), **format pliku** (np. *JPEG*, *PNG*) oraz **terenowy rozmiar piksela** (np. *2,00 m/pix*).
* Odnośnik **Zmień…** rozwija panel parametrów, umożliwiając zmianę układu, rozmiaru segmentu i rozdzielczości piksela. Po wybraniu warstwy program automatycznie podpowiada najmniejszy zalecany piksel spośród warstw kanapki.

### 3. Karta *Stan sesji*
* Wyświetla duży, czytelny przycisk akcji:
  * **Pobierz segmenty**: podświetlony kolorem, gdy wybrano warstwę i zaznaczono obszar (przy braku warstwy lub obszaru jest bezpiecznie przygaszony),
  * **Przerwij pobieranie**: pojawia się w trakcie pobierania,
  * **Eksportuj pobraną mapę…**: wyświetla się automatycznie po zakończeniu pobierania, prowadząc wprost do generowania plików MBTiles, GeoPackage, KMZ, JNX lub scalonego arkusza.
* Ikona po prawej stronie sygnalizuje stan sesji: strzałka pobierania, animowany łuk postępu w trakcie pracy, zielony znak ✓ po pomyślnym zakończeniu lub czerwony wykrzyknik w razie błędów.

---

## Katalog map i lista warstw

Prawy panel programu organizuje źródła danych i kompozycję wielowarstwową:

* **Wybierz mapy (katalog tematyczny):**
  * Grupy tematyczne: *Ortofotomapy*, *Mapy topograficzne*, *Rzeźba terenu (cieniowanie, hipsometria)*, *Mapy historyczne i zabytki*, *Ewidencja, geodezja i uzbrojenie (EGiB, GESUT)*, *Granice i adresy (PRG, PRNG)*, *Przyroda i środowisko (GDOŚ, BDL)*, *Kraje sąsiednie i strefa przygraniczna (Czechy, Słowacja, Niemcy, Litwa, Bałtyk)* oraz serwery dodane przez użytkownika.
  * Zaznaczenie pola wyboru dodaje warstwę do listy wybranych. Odznaczenie ją usuwa.
  * Pole **szukaj warstwy…** błyskawicznie filtruje całe drzewo według dowolnych słów kluczowych. Klawisz <kbd>Esc</kbd> czyści filtr.
  * Przycisk **+ Serwer…** otwiera kreator usług WMS/WMTS/XYZ.

* **Wybrane warstwy oraz ich kolejność:**
  * Lista mieści do 12 warstw ułożonych w kanapkę (warstwa na dole listy to podkład, warstwy wyżej leżą na wierzchu).
  * **Krycie bezpośrednio w wierszu:** kliknięcie wartości krycia w wierszu warstwy pozwala błyskawicznie zmienić przezroczystość (5–100%).
  * **Usuwanie i kolejność:** przycisk **✕** na końcu wiersza natychmiast usuwa warstwę. Kolejność zmienia się przeciągając wiersz myszą lub skrótem <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd>.
  * Przycisk **Mikser…** w nagłówku karty otwiera zaawansowane okno miksera warstw.

### Okno *Warstwy - zaawansowane* (Mikser)

![Okno Warstwy - zaawansowane z suwakami krycia i podglądem kanapki]({{ '/pomoc/obrazy/03-warstwy-zaawansowane.png' | relative_url }})

*Rysunek 2. Okno zaawansowanego miksera warstw z suwakami krycia, trybami mieszania i podglądem na żywo.*

* Pozwala przypisać każdej warstwie niezależny tryb mieszania: *Zwykły*, *Mnożenie (Multiply)*, *Ciemniej*, *Jaśniej*, *Ekran*, *Nakładka (Overlay)* oraz innowacyjny tryb **7 Rzeźba terenu**.
* **Tryb „Rzeźba terenu”:** eliminuje problem szarej zasłony przy nakładaniu cieniowania NMT LiDAR. Płaski teren pozostaje neutralny, stoki w cieniu ciemnieją, a zbocza oświetlone delikatnie jaśnieją.
* Wyświetla podgląd złożenia warstw na żywo z lupą powiększającą oraz przycisk *Korekta obrazu...* (filtr MIP, wyostrzanie krawędzi, rozjaśnienie lasów).

---

## Zakładki dolne prawego panelu

Dolna część prawego panelu zawiera trzy funkcjonalne zakładki:

1. **Podgląd:**
   * Renderuje w tle rzeczywistą próbkę złożonych warstw ze środka siatki w docelowej rozdzielczości piksela.
   * Pokazuje bieżące powiększenie w procentach. Przyciski **1:1** (piksel w piksel), **-**, **+** oraz **Dopasuj** pozwalają precyzyjnie ocenić czytelność napisów i dróg.
   * Przycisk **Mikser warstw i krycia...** (lub kliknięcie podglądu) otwiera okno zaawansowanych parametrów kompozycji.
2. **Komunikaty:**
   * Dziennik zdarzeń informujący o postępie pobierania, liczbie pobranych segmentów, transferze danych i ewentualnych błędach serwerów.
   * Przy wystąpieniu błędów zakładka staje się czerwona i wyświetla liczbę problematycznych fragmentów (np. *Komunikaty (2)*).
3. **Sesja:**
   * Szczegółowe metadane bieżącej sesji: dokładne ścieżki na dysku, data utworzenia, plik `conf.txt`, podsumowanie siatki i format plików.

---

## Pasek stanu

Na dolnej krawędzi okna głównego pasek stanu prezentuje najważniejsze parametry robocze:
* **Układ współrzędnych sesji** (np. *PL-1992*),
* **Pozycja kursora myszy** w wybranym układzie: współrzędne `x` (północna) i `y` (wschodnia) w metrach lub stopniach,
* **Przybliżona skala mapy** na ekranie (np. *1 : 25 000*),
* **Nazwa sesji i wskaźnik stanu** z kolorową diodą: zielona dioda oznacza pomyślne ukończenie pobierania, czerwona – błędy serwerów (kliknięcie natychmiast przełącza do zakładki *Komunikaty*).

---

➡️ **Kolejny krok:** [Wybór obszaru pobierania mapy →](../obszar/)

*Zobacz też: [Pierwsza mapa krok po kroku](../pierwsze-kroki/pierwsza-mapa/) · [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy) · [Ustawienia programu](../ustawienia/)*
