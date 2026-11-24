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

![Okno główne programu MapoTero]({{ '/pomoc/obrazy/03-okno-glowne.png' | relative_url }})

*Rysunek 1. Okno główne programu MapoTero — mapa podglądu z siatką segmentów, katalog warstw, kompozycja warstw z podglądem na żywo oraz karty obszaru, parametrów i stanu sesji.*

| Nr | Element okna | Do czego służy |
|---|---|---|
| **1** | Pasek menu | menu *Sesja pobierania* (<kbd>Alt</kbd>+<kbd>S</kbd>), menu *Narzędzia* oraz menu *Pomoc* (pełna lista poleceń i skrótów poniżej) |
| **2** | Górny pasek narzędzi | trzy grupy: **Nowa sesja** i **Szybki kreator…** (<kbd>Ctrl</kbd>+<kbd>K</kbd>), **Zaznacz obszar** i wyszukiwarka **Szukaj miejsca lub działki…** (<kbd>Ctrl</kbd>+<kbd>F</kbd>), **Folder pobrań**, a po prawej lista **Podkład** (OpenStreetMap, Google Maps, Bing) |
| **3** | Mapa podglądu | interaktywny podkład orientacyjny do wyboru obszaru, siatka segmentów z białymi uchwytami regulacji, przyciski **+** / **-** oraz podziałka liniowa w prawym dolnym rogu |
| **4** | Karta *Warstwy do pobrania* | tematyczny katalog zbiorów i warstw, filtr *Szukaj warstwy…*, przycisk **Zwiń / Rozwiń**, ikona galerii warstw online oraz przycisk **+ Serwer…** |
| **5** | Karta *Wybrane warstwy* | stos wybranych warstw (do 12), pole liczbowe krycia (`%`) w wierszu wybranej warstwy, krzyżyk `✕` do usuwania, zmiana kolejności myszą lub <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd> oraz przycisk **Mikser…** |
| **6** | Karta *Obszar pobierania* | wymiary terenu (np. `8 × 8 km`) i liczba segmentów. Kliknięcie wymiarów wraca do siatki na mapie, a **Zmień ▾** otwiera menu metod wyznaczania obszaru |
| **7** | Karta *Parametry segmentów* | rozmiar piksela (np. `2 m/px`), układ współrzędnych, bok segmentu i format plików. Odnośnik **Zmień…** rozwija panel edycji parametrów |
| **8** | Karta *Stan sesji* | przycisk **Pobierz segmenty** (w trakcie pracy **Przerwij pobieranie**), a po skompletowaniu segmentów **Eksportuj pobraną mapę…**. Ikona i kolor sygnalizują stan sesji |
| **9** | Zakładki prawego panelu | trzy zakładki: **Podgląd** (próbka kompozycji z przyciskami `-`, `+`, `Dopasuj`, odświeżaniem i powiększeniem w %), **Komunikaty** (dziennik zdarzeń i błędów) oraz **Sesja** (szczegóły folderu sesji i przycisk *Usuń pobrane*) |
| **10** | Pasek stanu | układ współrzędnych sesji, pozycja kursora (`X`, `Y`), przybliżona skala mapy oraz aktywna sesja z kolorową diodą stanu |

<details markdown="block">
<summary><b>Pokaż schemat okna z ponumerowanymi sekcjami (1–10)</b></summary>

![Okno główne programu MapoTero z ponumerowanymi sekcjami]({{ '/pomoc/obrazy/03-okno-glowne-sekcje.png' | relative_url }})

*Schemat rozmieszczenia sekcji okna głównego opisanych w tabeli powyżej (1–10).*

</details>

Układ okna dzieli się na dwie główne kolumny:
* **po lewej**: mapa podglądu z górnym paskiem narzędzi oraz trzema kartami pod mapą (*Obszar pobierania*, *Parametry segmentów*, *Stan sesji*),
* **po prawej**: regulowany panel z kartami *Warstwy do pobrania*, *Wybrane warstwy* i dolnymi zakładkami. Szerokość prawego panelu i wysokość jego sekcji można płynnie regulować, przeciągając dzielniki (dwuklik przywraca domyślny podział).

Program uruchamia się domyślnie w pełnym oknie zmaksymalizowanym.

---

## Pasek menu

| Menu | Polecenie | Skrót | Opis |
|---|---|---|---|
| **Sesja pobierania** | Nowa sesja | | czyści wybrane warstwy, zasięg, zaznaczenie i siatkę, obszar wzdłuż trasy i kontur działki, przywracając domyślny stan programu (zob. [Sesje pobierania](../sesje/)) |
| | Otwórz folder pobrań | | otwiera w Eksploratorze Windows bieżący folder sesji z pobranymi segmentami |
| | Pokaż siatkę na mapie | | włącza lub ukrywa siatkę segmentów na mapie podglądu |
| | Zapisz ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>Z</kbd> | zapisuje parametry bieżącej sesji we wskazanym folderze. Wybrany folder staje się folderem sesji |
| | Wczytaj ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>W</kbd> | wczytuje sesję ze wskazanego folderu (wymaga pliku `conf.txt`) |
| | Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)... | | [Obszar wzdłuż trasy](../obszar/korytarz/), pobieranie segmentów tylko w zadanym pasie wzdłuż śladu trasy |
| | Obszar pobierania z poligonu (KML, GeoJSON, GPKG)... | | [Obszar z poligonu](../obszar/zaznaczanie/#obszar-z-poligonu-kml-geojson-geopackage), automatyczny prostokąt wokół wektora |
| | Usuń obszar wzdłuż trasy (pobieranie całego prostokąta) | | usuwa korytarz i wraca do pobierania pełnego prostokąta |
| | Odśwież mapę (pobierz ponownie wszystkie segmenty) | | [Odświeżanie mapy](../pobieranie/odswiezanie/), bezpieczne dociąganie nowych kafelków |
| | Usuń pobrane pliki bieżącej sesji... (do Kosza) | | przenosi pobrane segmenty bieżącej sesji do Kosza (zob. [Dane programu](../pierwsze-kroki/dane-programu/)) |
| **Narzędzia** | Szybki kreator mapy... | <kbd>Ctrl</kbd>+<kbd>K</kbd> | [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy), przygotowanie gotowej mapy w 4 krokach od celu do pliku |
| | Dodaj nowy serwer WMS / WMTS... | | [Kreator serwerów](../serwery/kreator/), dodawanie usług WMS, WMTS oraz kafelków XYZ/TMS |
| | Otwórz folder własnych warstw... | | otwiera folder `warstwy` w katalogu danych programu |
| | Wyszukaj miejscowość lub działkę... | <kbd>Ctrl</kbd>+<kbd>F</kbd> | [Wyszukiwarka miejscowości i działek ULDK](../obszar/zaznaczanie/#wyszukiwarka-miejscowości-adresów-i-działek-katastralnych) (<kbd>Ctrl</kbd>+<kbd>D</kbd> dla działek) |
| | Konwertuj mapę OziExplorer (.map)... | | [Konwerter map OziExplorer](../narzedzia/#konwerter-map-oziexplorer-map), kalibrowane skany do JNX, KMZ, MBTiles i GeoPackage |
| | Konwertuj mapę GeoTIFF... | | [Konwerter map GeoTIFF](../narzedzia/#konwerter-map-geotiff), konwersja pliku GeoTIFF do MBTiles, GeoPackage, KMZ, JNX, SQLiteDB lub OruxMaps |
| | Profil wysokościowy trasy (GPX, KML)... | | [Profil wysokościowy trasy](../narzedzia/#profil-wysokościowy-trasy), rzędne z laserowego skaningu NMT/LIDAR i wzbogacanie GPX |
| | Scalanie segmentów | <kbd>Ctrl</kbd>+<kbd>L</kbd> | okno [Eksport mapy](../eksport/) z wybranym formatem *Arkusz*, zobacz [Scalanie segmentów](../scalanie/) |
| | Eksport z pobranych segmentów… | <kbd>Ctrl</kbd>+<kbd>E</kbd> | [Eksport mapy](../eksport/) do formatów mobilnych (JNX, KMZ, MBTiles, GeoPackage, SQLiteDB, OruxMaps, TrekBuddy) i arkuszy |
| | Eksport do CAD (DXF, skrypt SCR, LISP)... | | [Eksport do CAD](../cad/), rysunek DXF, skrypt SCR i program LISP dla programów CAD |
| | Korekta obrazów w folderze... | | [Korekta obrazów w folderze](../narzedzia/#korekta-obrazów-w-folderze), wsadowa optymalizacja kontrastu, gamma i filtr MIP |
| | Prześlij mapę na telefon (Wi-Fi)... | | [Transfer mapy na telefon](../narzedzia/#przesyłanie-mapy-na-telefon-przez-wi-fi-i-kod-qr), lokalny serwer Wi-Fi i kod QR |
| | Kopiuj mapę do Garmina (USB)... | | [Kopiowanie do Garmina](../narzedzia/#kopiowanie-mapy-do-odbiornika-garmin-usb), wykrywanie odbiornika w trybie pamięci masowej |
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

Pasek nad mapą grupuje główne polecenia w trzech blokach po lewej oraz wybór podkładu po prawej:

1. **Sesja i kreator:**
   * **Nowa sesja**: czyści obszar roboczy i przygotowuje program do pobrania nowej mapy do nowego podfolderu.
   * **Szybki kreator…** (<kbd>Ctrl</kbd>+<kbd>K</kbd>): otwiera 4-etapowy kreator przygotowania mapy (wybór celu, obszaru, szablonu i zapisu).
2. **Wybór obszaru i wyszukiwarka:**
   * **Zaznacz obszar**: włącza tryb wskazywania prostokąta na mapie dwoma kliknięciami w przeciwległych narożnikach.
   * **Szukaj miejsca lub działki…**: pole wyszukiwania miejscowości i adresów, działek katastralnych ULDK (np. `działka Wieliczka 123/4`) oraz współrzędnych (WGS84, PL-1992, PL-2000). Skrót <kbd>Ctrl</kbd>+<kbd>F</kbd> przenosi kursor wprost do tego pola.
3. **Folder pobrań i podkład:**
   * **Folder pobrań**: otwiera katalog `download\` w Eksploratorze Windows.
   * **Podkład** *(lista rozwijana po prawej)*: przełącza mapę orientacyjną podglądu między **OpenStreetMap** (domyślny), **Google Maps** oraz **Bing (zdjęcia lotnicze)**.

---

## Mapa podglądu

Interaktywna mapa podglądu służy do lokalizacji w terenie i precyzyjnego wyznaczania zasięgu siatki segmentów:

| Czynność | Sposób wykonania |
|---|---|
| przesuwanie mapy | przeciągnij lewym przyciskiem myszy |
| przybliżanie i oddalanie | kółko myszy, przyciski **+** i **-** w lewym górnym rogu mapy, dwuklik lewym (przybliża) lub prawym (oddala) |
| zaznaczanie obszaru | przeciągnij **prawym** przyciskiem myszy albo użyj przycisku **Zaznacz obszar** (zob. [Zaznaczanie na mapie](../obszar/zaznaczanie/)) |
| regulacja zasięgu siatki | przeciągnij białe uchwyty w narożnikach, na środkach boków lub uchwyt centralny siatki |
| anulowanie zaznaczania | naciśnij klawisz <kbd>Esc</kbd> |

**Podziałka liniowa:** w prawym dolnym rogu mapy widoczna jest podziałka metryczna ułatwiająca ocenę odległości w terenie.

---

## Karty pod mapą

Pod mapą znajdują się trzy karty w jednolitym, przejrzystym stylu:

### 1. Karta *Obszar pobierania*
* Pokazuje rzeczywisty wymiar terenowy siatki (np. `8 × 8 km`) i liczbę segmentów (np. `4 segmenty`), a przy obszarze wzdłuż trasy informację o korytarzu.
* Kliknięcie wymiaru obszaru centruje mapę na siatce pobierania.
* Odnośnik **Zmień ▾** rozwija menu modyfikacji obszaru:
  * **Pokaż na mapie**: centruje widok na wyznaczonym obszarze,
  * **Zaznacz na mapie**: uruchamia zaznaczanie prostokąta na mapie,
  * **Wpisz współrzędne…**: otwiera okno ręcznego wpisywania granic zasięgu,
  * **Obszar wzdłuż trasy (GPX, KML, KMZ, GPKG)…**: wyznacza segmenty w pasie wzdłuż śladu,
  * **Obszar z poligonu (KML, GeoJSON, GPKG)…**: dopasowuje siatkę do obrysu obiektu wektorowego.

### 2. Karta *Parametry segmentów*
* Przedstawia **terenowy rozmiar piksela** (np. `2 m/px`), **układ współrzędnych** (np. `PL-1992`), **bok segmentu** (np. `2000 px`) oraz **format plików** (`JPEG`, `PNG`…).
* Odnośnik **Zmień…** rozwija panel parametrów, gdzie można zmienić piksel, bok segmentu i układ współrzędnych. Po wybraniu warstwy program automatycznie dobiera najmniejszy zalecany piksel spośród warstw kanapki.

### 3. Karta *Stan sesji*
* Wyświetla duży, czytelny przycisk akcji:
  * **Pobierz segmenty**: podświetlony kolorem, gdy wybrano warstwę i zaznaczono obszar (przy braku warstwy lub obszaru jest bezpiecznie przygaszony),
  * **Przerwij pobieranie**: pojawia się w trakcie pobierania,
  * **Eksportuj pobraną mapę…**: wyświetla się automatycznie po zakończeniu pobierania, prowadząc wprost do generowania plików MBTiles, GeoPackage, KMZ, JNX lub scalonego arkusza.
* Ikona po prawej stronie sygnalizuje stan sesji: strzałka pobierania, animowany łuk postępu w trakcie pracy, zielony znak ✓ po pomyślnym zakończeniu lub czerwony wykrzyknik w razie błędów.

---

## Katalog map i lista warstw

Prawy panel programu organizuje źródła danych i kompozycję wielowarstwową:

* **Warstwy do pobrania (katalog tematyczny):**
  * Grupy tematyczne: *Mapy topograficzne*, *Rzeźba terenu (cieniowanie, hipsometria)*, *Ortofotomapy*, *Mapy historyczne i zabytki*, *Ewidencja, geodezja i uzbrojenie (EGiB, GESUT)*, *Granice i adresy (PRG, PRNG)*, *Przyroda i środowisko (GDOŚ, BDL)*, *Geologia (PIG-PIB)*, *Kraje sąsiednie i strefa przygraniczna (Czechy, Słowacja, Niemcy, Litwa, Bałtyk)* oraz serwery dodane przez użytkownika.
  * Zaznaczenie pola wyboru dodaje warstwę do listy wybranych. Odznaczenie ją usuwa.
  * Pole wyszukiwarki pozwala przefiltrować drzewo według dowolnych słów kluczowych. Klawisz <kbd>Esc</kbd> czyści filtr.
  * Przycisk **Zwiń / Rozwiń** zwija lub rozwija wszystkie grupy w katalogu, ikona galerii otwiera [Galerię warstw map online](../galeria-warstw/), a przycisk **+ Serwer…** w nagłówku karty uruchamia kreator usług WMS, WMTS i kafelkowych XYZ/TMS.

* **Wybrane warstwy:**
  * Lista mieści do 12 warstw ułożonych w kanapkę (warstwa na dole listy to podkład, warstwy wyżej leżą na wierzchu).
  * **Krycie bezpośrednio w wierszu:** zaznaczenie warstwy wyświetla pole liczbowe do zmiany przezroczystości (5–100%).
  * **Usuwanie i kolejność:** przycisk **✕** w wierszu usuwa warstwę. Kolejność zmienisz, przeciągając wiersz myszą lub skrótem <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd>.
  * Przycisk **Mikser…** w nagłówku karty otwiera okno *Warstwy - zaawansowane*.

### Okno *Warstwy - zaawansowane* (Mikser)

![Okno Warstwy - zaawansowane: mapa topograficzna 1:10 000 z cieniowaniem LiDAR w trybie mnożenia]({{ '/pomoc/obrazy/03-warstwy-zaawansowane.png' | relative_url }})

*Rysunek 2. Okno zaawansowanego miksera warstw: mapa topograficzna 1:10 000 połączona z cieniowaniem NMT LiDAR w trybie mnożenia (pełne krycie 100%).*

* Pozwala przypisać każdej warstwie niezależny tryb mieszania: *Zwykły*, *Nakładka*, *Mnożenie*, *Ekran*, *Ciemniej*, *Jaśniej*, *Różnica* oraz **Rzeźba terenu**.
* **Tryb „Rzeźba terenu”:** eliminuje problem szarej zasłony przy nakładaniu cieniowania NMT LiDAR. Płaski teren pozostaje neutralny, stoki w cieniu ciemnieją, a zbocza oświetlone delikatnie jaśnieją.
* Wyświetla podgląd złożenia warstw na żywo z lupą powiększającą oraz przycisk *Korekta obrazu...* (filtr MIP, wyostrzanie krawędzi, rozjaśnienie lasów).

---

## Zakładki dolne prawego panelu

Dolna część prawego panelu zawiera trzy funkcjonalne zakładki:

1. **Podgląd:**
   * Renderuje w tle rzeczywistą próbkę złożonych warstw ze środka siatki w docelowej rozdzielczości piksela.
   * Pokazuje bieżące powiększenie w procentach, przyciski **−**, **+**, **Dopasuj** oraz przycisk odświeżania próbki z serwerów.
   * Kliknięcie próbki podglądu otwiera okno zaawansowanego miksera warstw.
2. **Komunikaty:**
   * Dziennik zdarzeń informujący o postępie pobierania, liczbie pobranych segmentów, transferze danych i ewentualnych błędach serwerów.
   * Przy wystąpieniu błędów zakładka staje się czerwona i wyświetla liczbę problematycznych fragmentów (np. *Komunikaty (2)*).
3. **Sesja:**
   * Szczegółowe dane bieżącej sesji: dokładne ścieżki na dysku, plik `conf.txt`, podsumowanie siatki, format plików oraz przycisk **Usuń pobrane** (przenosi pobrane segmenty bieżącej sesji do Kosza).

---

## Pasek stanu

Na dolnej krawędzi okna głównego pasek stanu prezentuje najważniejsze parametry robocze:
* **Układ współrzędnych sesji** (np. *PL-1992*),
* **Pozycja kursora myszy** w wybranym układzie: współrzędne `x` (północna) i `y` (wschodnia) w metrach lub stopniach,
* **Przybliżona skala mapy** na ekranie (np. *1 : 25 000*),
* **Nazwa sesji i wskaźnik stanu** z kolorową diodą: zielona dioda oznacza pomyślne ukończenie pobierania, czerwona oznacza błędy serwerów (kliknięcie przełącza do zakładki *Komunikaty*).

---

➡️ **Kolejny krok:** [Wybór obszaru pobierania mapy →](../obszar/)

*Zobacz też: [Pierwsza mapa krok po kroku](../pierwsze-kroki/pierwsza-mapa/) · [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy) · [Ustawienia programu](../ustawienia/)*
