---
title: Pojęcia podstawowe
nav_order: 4
---

# Pojęcia podstawowe
{: .no_toc }

Kilka pojęć, bez których trudno świadomie dobrać ustawienia: segment, piksel, siatka, sesja i układ współrzędnych.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Zbiór map i warstwa

**Zbiór map** to plik tekstowy w folderze `warstwy`, który opisuje jeden serwer map: jego adres i listę dostępnych warstw. Zbiory to np. *skany_map_topograficznych*, *ortofotomapa*, *geologiczna_50_SMGP*. W drzewie *Wybierz mapy* warstwy są pogrupowane tematycznie i mają czytelne nazwy (np. *Mapy topograficzne > Skany map topograficznych GUGiK > 1:25 000, układ 1965*).

**Warstwa** to jedna mapa udostępniana przez serwer, np. *Raster_25_1965* (mapa topograficzna 1:25 000) w zbiorze skanów map topograficznych. Przy każdej warstwie zbiór podaje zalecany rozmiar piksela.

W oknie głównym możesz wybrać **kilka warstw naraz** (do 12 pozycji) i ułożyć z nich kanapkę w panelu *Wybrane warstwy oraz ich kolejność*:

* **Warstwa nr 1** jest podkładem na samym spodzie, a warstwy o wyższych numerach (2, 3…) nakładają się na nią.
* Możesz łączyć warstwy z tego samego serwera albo z zupełnie różnych źródeł (np. ortofotomapę WMTS, granice działek z WMS i szlaki turystyczne z kafli XYZ).
* Każdej warstwie możesz nadać częściowe **krycie (5–100%)** oraz zmienić jej pozycję w stosie.

![Schemat kanapki warstw: podkład na dole i warstwy nakładane powyżej]({{ '/pomoc/obrazy/02-schemat-kanapka.svg' | relative_url }})

*Rysunek 1. Zasada działania kanapki warstw: warstwa 1 tworzy podkład na samym spodzie, wyższe warstwy leżą nad nią.*

Gdy wszystkie warstwy pochodzą z jednego serwera WMS i mają krycie 100%, łączy je bezpośrednio serwer. Jeśli łączysz różne serwery, usługi kafelkowe lub ustawisz krycie poniżej 100%, program pobiera warstwy osobno i nakłada je w gotowy segment. Pobrane wcześniej osobne sesje można też połączyć później przy eksporcie (zob. [Nakładki i łączenie warstw](../eksport/nakladki/)).

Zbiory map dostarczane z programem opisuje [Katalog zbiorów map](../zbiory-map/). Własne dodasz za pomocą [kreatora serwerów](../serwery/kreator/).

---

## Serwery WMS, WMTS i kafelkowe XYZ / TMS

Program pobiera mapy z trzech rodzajów usług:

| | WMS | WMTS | Kafelkowe XYZ / TMS |
|---|---|---|---|
| Jak działa | serwer generuje obraz na zamówienie dla wskazanego prostokąta, rozmiaru i układu | oficjalny standard OGC z gotowymi kaflami w stałych skalach (zwykle 256 × 256 px) | proste adresy kafli `{z}/{x}/{y}` lub `{-y}` (TMS) w układzie Web Mercator |
| Co robi program | zamawia od razu cały segment | pobiera kafle z odpowiedniego poziomu, składa segment i w razie potrzeby przelicza układ | pobiera kafle według szablonu, składa segment i przelicza do układu sesji |
| Kilka warstw | przy kryciu 100% składa serwer, inaczej składa program | program pobiera i nakłada kafle kolejnych warstw | program pobiera i nakłada kafle kolejnych warstw |
| Przykłady | skany map topograficznych, mapy geologiczne, działki EGiB | *Ortofotomapa standardowa (WMTS)*, *Mapa topograficzna GUGiK (WMTS)*, *BDOT10k_WMTS* | *OSMapa.pl*, *UMP-pcPL*, szlaki *Waymarked Trails* |

Dla użytkownika obsługa w oknie głównym jest taka sama: w każdym przypadku powstają identyczne segmenty z wybranymi plikami kalibracji. Szczegóły: [Pobieranie z usług kafelkowych](../pobieranie/wmts/).

---

## Segment

Segment to pojedynczy kwadratowy plik graficzny (np. `_01_02.jpg`), na który program dzieli wybrany obszar.

* Bok segmentu podaje się w pikselach w polu *Segment [pix]*: od 1 do **2048** px (domyślnie 2000). Serwery Geoportalu nie generują obrazów większych niż 2048 px.
* Niektóre serwery mają niższy limit. Gdy serwer odrzuca duże zapytania, zmniejsz bok segmentu.
* W [trybie TrekBuddy](../ustawienia/#trekbuddy-i-locus-map) segment ma zawsze sztywny bok 512 px.

Większe segmenty oznaczają mniej plików na dysku i mniej zapytań do serwera. Mniejsze segmenty przydają się przy wolnym łączu lub niestabilnym serwerze, bo ewentualny błąd pobrania dotyczy wtedy mniejszego fragmentu.

---

## Rozmiar piksela a skala mapy

**Rozmiar piksela** (pole *Piksel [m/pix]*) określa, ile metrów w terenie przypada na jeden piksel obrazu. To główne ustawienie: decyduje o szczegółowości mapy i o objętości plików.

* **Za duży piksel.** Obraz będzie rozmyty, a drobne napisy i symbole nieczytelne.
* **Za mały piksel.** Obraz nie zyska na szczegółowości ponad swój oryginał, za to objętość danych gwałtownie wzrośnie. Zmniejszenie piksela o połowę daje **czterokrotnie** więcej pikseli i segmentów.

Po zaznaczeniu warstwy program automatycznie ustawia **najmniejszy** zalecany rozmiar piksela spośród wszystkich wybranych warstw (żeby dokładniejsza warstwa zachowała ostrość). Wartości wpisanej ręcznie program nie nadpisuje.
Orientacyjne wartości dla skanów map papierowych:

| Skala mapy źródłowej | Zalecany piksel | Teren w jednym segmencie 2000 px | Pikseli na 1 km² |
|---|---|---|---|
| 1:10 000 | 1 m | 2 × 2 km | 1 000 000 |
| 1:25 000 | 2 m | 4 × 4 km | 250 000 |
| 1:50 000 | 4 m | 8 × 8 km | 62 500 |
| 1:100 000 | 8 m | 16 × 16 km | 15 625 |
| ortofotomapa (standardowa rozdzielczość) | 0,25 m | 0,5 × 0,5 km | 16 000 000 |

Zasada przybliżona: **piksel w metrach ≈ mianownik skali ÷ 12 500**. Dla map z dużą ilością drobnego tekstu można wybrać piksel o stopień mniejszy (np. 1:25 000 z pikselem 1,5 m).

Teren obejmowany przez jeden segment to bok segmentu × piksel, np. 2000 px × 2 m = 4 km. Wartość tę widać w polu *Rozmiar pojedynczego segmentu*.

{: .informacja }
W układzie WGS84 piksel podaje się w stopniach (*Piksel [°/pix]*). Przy zmianie układu program sam przelicza wpisany piksel, a po kliknięciu warstwy przelicza zalecaną wartość z metrów na stopnie.

---

## Siatka segmentów

Zaznaczony obszar jest pokrywany siatką segmentów:

* Zaczyna się w lewym dolnym narożniku zaznaczenia i rośnie w górę oraz w prawo do pełnej liczby segmentów. Z tego powodu zazwyczaj jest nieco większa od samego zaznaczenia.
* Ma wiersze numerowane od góry (wiersz 1 leży najbardziej na północ) i kolumny od lewej.
* Wyświetla się na mapie jako niebieskie kwadraty (do 3000 segmentów, a przy większej liczbie widać tylko obrys zewnętrzny).

![Schemat siatki segmentów: początek w lewym dolnym narożniku, wiersze od góry, kolumny od lewej]({{ '/pomoc/obrazy/02-schemat-siatka.svg' | relative_url }})

*Rysunek 2. Schemat siatki segmentów: początek w lewym dolnym narożniku, wiersze numerowane od góry, kolumny od lewej.*

Pole *Wynikowy rozmiar siatki* podaje liczbę kolumn i wierszy oraz wymiary całej mapy w pikselach i w kilometrach.
Nazwy plików segmentów wynikają z ich pozycji w siatce: np. `_03_12` to wiersz 3, kolumna 12 (inne style numeracji opisuje rozdział [Sesje pobierania](../sesje/#nazwy-segmentów)).

---

## Sesja pobierania

**Sesja** to jedna pobrana mapa: folder z segmentami i plikiem `conf.txt`, w którym zapisane są wszystkie parametry pobierania (zbiór map, warstwy, układ, zasięg, piksel, bok segmentu, format, numeracja, data pobrania).

Dzięki plikowi `conf.txt` można w każdej chwili:

* pobrać brakujące segmenty lub odświeżyć mapę,
* scalić segmenty w arkusz albo wyeksportować mapę do telefonu lub CAD bez ponownego konfigurowania parametrów,
* sprawdzić po czasie źródło danych i datę pobrania.

Szczegóły: [Sesje pobierania](../sesje/).

---

## Układy współrzędnych

Segmenty pobierane są w układzie wybranym w oknie głównym (*Układ współrzędnych pobierania*). W tym samym układzie powstają pliki kalibracji i scalone arkusze.

| Układ | Kod EPSG | Kiedy go używać |
|---|---|---|
| **PL-1992** | 2180 | domyślny, jeden układ dla całej Polski: mapy topograficzne, ortofotomapy, większość serwerów Geoportalu |
| **PL-2000** (strefy 5, 6, 7, 8) | 2176, 2177, 2178, 2179 | mapy wielkoskalowe, dane geodezyjne i projekty CAD. Strefę dobiera się według długości geograficznej (strefa 5: 15°E, strefa 6: 18°E, strefa 7: 21°E, strefa 8: 24°E) |
| **UTM** (strefy 33N, 34N, 35N) | 32633, 32634, 32635 | odbiorniki GPS, mapy wojskowe, dane międzynarodowe |
| **WGS84** | 4326 | współrzędne geograficzne w stopniach: programy i aplikacje, które nie obsługują układów polskich |

{: .uwaga }
Serwer musi obsługiwać wybrany układ. Wiele serwerów obsługuje kilka układów, ale niektóre tylko jeden lub dwa. Gdy segmenty są puste albo serwer zgłasza błąd, wróć do PL-1992.
Obsługiwane układy warstwy pokazuje [kreator serwerów](../serwery/kreator/).

Przy zmianie układu program przelicza wpisany zasięg i rozmiar piksela. Obszar w nowym układzie to prostokąt opisany na poprzednim, więc może być nieco większy.

---

## Konwencja X i Y

W programie obowiązuje **konwencja geodezyjna**:

* **X**: współrzędna północna (rośnie na północ),
* **Y**: współrzędna wschodnia (rośnie na wschód).

Dlatego w polach zasięgu *X* oznacza dół i górę obszaru, a *Y* lewą i prawą krawędź. W PL-1992 wartości X dla Polski mieszczą się w przedziale 130 000–790 000 m, a Y w przedziale 140 000–880 000 m.

Programy GIS i CAD stosują najczęściej konwencję matematyczną: **X = wschód, Y = północ**. Pliki kalibracji i rysunki CAD generowane przez MapoTero są już zapisane w konwencji właściwej dla danego oprogramowania. O odwróconej kolejności osi trzeba pamiętać tylko przy ręcznym wpisywaniu współrzędnych.

![Porównanie osi X i Y: konwencja geodezyjna w MapoTero vs konwencja matematyczna w GIS i CAD]({{ '/pomoc/obrazy/02-schemat-osie.svg' | relative_url }})

*Rysunek 3. Konwencja geodezyjna w MapoTero (X na północ, Y na wschód) w zestawieniu z konwencją matematyczną w CAD i GIS.*

| Oś | W MapoTero (geodezja) | W AutoCAD, QGIS, ArcGIS (matematyka) |
|---|---|---|
| **X** | Północ (kierunek góra / dół) | Wschód (kierunek lewo / prawo) |
| **Y** | Wschód (kierunek lewo / prawo) | Północ (kierunek góra / dół) |

---

## Georeferencja

Segment to zwykły plik graficzny. Programy zewnętrzne rozpoznają jego położenie w terenie dzięki **georeferencji**:

* zapisanej bezpośrednio w nagłówku pliku (GeoTIFF dla scalonych arkuszy),
* zapisanej w osobnym **pliku kalibracji** obok obrazu (np. `.jgw` i `.prj` dla QGIS, `.map` dla OziExplorera, `.kml` dla Google Earth).

Tworzenie plików kalibracji włącza się w oknie *Ustawienia*. Domyślnie opcja jest wyłączona, ponieważ moduły scalania i eksportu korzystają bezpośrednio z pliku `conf.txt`. Zastosowanie poszczególnych formatów opisuje rozdział [Pliki kalibracji](../pliki-kalibracji/).

---

➡️ **Kolejny krok:** [Elementy okna głównego →](../okno-glowne/)

*Zobacz też: [Jak duży obszar pobrać](../obszar/wielkosc/) · [Słownik pojęć](../slownik/)*
