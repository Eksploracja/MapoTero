---
title: Słownik pojęć
nav_order: 21
---

# Słownik pojęć
{: .no_toc }

Krótkie objaśnienia najważniejszych terminów używanych w programie MapoTero i w tej pomocy.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

| Pojęcie | Wyjaśnienie |
|---|---|
| **CRS / EPSG** | Oznaczenie układu współrzędnych (np. `EPSG:2180` dla układu PL-1992, `EPSG:4326` dla WGS84). |
| **EZiUDP** | Ewidencja Zbiorów i Usług Danych Przestrzennych: oficjalny państwowy rejestr usług mapowych (krajowych, wojewódzkich, powiatowych i gminnych), przeszukiwany bezpośrednio przez kreator serwerów MapoTero. |
| **Georeferencja (kalibracja)** | Informacja zapisana w pliku obrazu (GeoTIFF) lub w osobnym pliku obok niego (`.jpgw`, `.prj`, `.map`, `.kml`, `.tab`), pozwalająca innym programom ustalić położenie mapy w terenie. |
| **GetCapabilities** | Dokument XML udostępniany przez serwer WMS lub WMTS z listą wszystkich dostępnych warstw, obsługiwanych układów i zakresów skal. |
| **Kafel (tile)** | Mały fragment mapy (zwykle `256 × 256 px`) udostępniany w gotowej formie przez usługi kafelkowe (WMTS, XYZ, TMS) lub zapisywany wewnątrz baz MBTiles/SQLiteDB. |
| **Kanapka warstw** | Kilka warstw nałożonych jedna na drugą w panelu *Wybrane warstwy oraz ich kolejność* (warstwa 1 to podkład na spodzie, wyższe numery leżą nad nią). |
| **Katalog warstw (`katalog.txt`)** | Plik konfiguracyjny w folderze `warstwy\`, który tworzy logiczne grupy tematyczne w drzewie *Wybierz mapy* i przypisuje warstwom czytelne, zrozumiałe nazwy zamiast kodów technicznych serwerów. |
| **Korytarz** | Pobieranie ograniczone do pasa o zadanej szerokości wokół wczytanej trasy GPX, KML lub KMZ (zamiast całego prostokąta). |
| **Krycie (%)** | Stopień nieprzezroczystości warstwy (`5–100%`). Mniejsze krycie pozwala prześwitywać warstwie znajdującej się niżej. |
| **LISP (`.lsp`)** | Program automatyzacji w środowisku CAD (AutoCAD, BricsCAD, ZWCAD itp.) wczytywany poleceniem `APPLOAD`, który wstawia podkłady rastrowe MapoTero pod istniejące obiekty rysunku poleceniem `MAPOTERO_WSTAW`. |
| **MBTiles / SQLiteDB** | Jednoplikowe bazy danych SQLite przechowujące piramidę kafelków mapy dla aplikacji mobilnych (Locus Map, OsmAnd, OruxMaps). |
| **Piksel terenowy (`[m/pix]`)** | Wielkość jednego piksela obrazu w terenie. Decyduje o ostrości mapy i objętości pobieranych danych. |
| **Segment** | Pojedynczy kwadratowy plik obrazu pobierany przez MapoTero (do `2048 × 2048 px`). Cały obszar dzielony jest na siatkę segmentów. |
| **Sesja pobierania** | Folder z pobranymi segmentami i plikiem `conf.txt`, który przechowuje komplet parametrów danej mapy. |
| **Siatka segmentów** | Układ wierszy (od góry) i kolumn (od lewej) segmentów pokrywających zaznaczony obszar. |
| **TileJSON** | Otwarty format metadanych JSON opisujący serwery kafli (adresy URL, zakresy powiększeń, atrybucję i zasięg geograficzny), rozpoznawany przez kreator serwerów. |
| **TMS** | Standard kafelków sieciowych (OSGeo), w którym oś pionowa `{-y}` liczona jest od południa (odwrotnie niż w zwykłym XYZ). |
| **WMS** | Usługa sieciowa (*Web Map Service*), w której serwer rysuje obraz dokładnie dla zamówionego przez program wycinka, rozmiaru i układu. |
| **WMTS** | Usługa sieciowa (*Web Map Tile Service*) udostępniająca gotowe, wcześniej wyrenderowane kafle w stałych poziomach powiększeń. |
| **World file** | Krótki 6-wierszowy plik tekstowy (`.jpgw`, `.pngw`, `.tifw`, `.wld`) podający położenie środka lewego górnego piksela i skalę obrazu dla programów GIS/CAD. |
| **XYZ** | Prosty schemat adresowania kafelków w układzie Web Mercator (`{z}/{x}/{y}`), popularny m.in. w serwisach OpenStreetMap. |

---

➡️ **Powrót do początku:** [Wprowadzenie do pomocy MapoTero →](../)

*Zobacz też: [Pojęcia podstawowe](../pojecia/) · [Okno główne](../okno-glowne/) · [Problemy i najczęstsze pytania](../problemy/)*
