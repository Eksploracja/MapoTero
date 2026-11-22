---
title: Wybór formatu
nav_order: 1
parent: Eksport mapy
---

# Wybór formatu
{: .no_toc }

Który format wybrać dla której aplikacji lub urządzenia.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Porównanie formatów

| Format | Dla | Budowa | Uwagi |
|---|---|---|---|
| **KMZ: Garmin Custom Maps** | odbiorniki Garmin | kafle JPEG do 1024 × 1024 px w WGS84 | opcjonalny automatyczny podział na tomy przy ponad 100 kaflach |
| **Garmin BirdsEye (JNX)** | odbiorniki Garmin (firmware z obsługą JNX) | wielopoziomowy raster z piramidą powiększeń | brak limitu 100 kafli, kopiowany do folderu `Garmin/BirdsEye` |
| **Arkusz** (scalanie segmentów) | Avenza Maps, QField, ArcGIS, CAD, QGIS | jeden plik GeoTIFF, JPEG albo PNG z georeferencją | pliki world file i `.prj`, `.map`, `.tab`, `.kml`; nakładki z innych folderów |
| **TrekBuddy / Locus Map** | Locus Map, TrekBuddy | archiwum `.tar` z kaflami 512 px, `.map` i `.set` | z dowolnej pobranej sesji, z nakładkami |
| **KMZ** | Locus Map, OruxMaps, Google Earth | kafle do 1024 × 1024 px w WGS84 | brak limitu kafli, może być nakładką (PNG) |
| **MBTiles** | Locus Map, OsmAnd, OruxMaps i wiele innych | baza SQLite z piramidą kafli Web Mercator 256 px | najbardziej uniwersalny, z poziomami powiększenia |
| **GeoPackage (.gpkg)** | QField, Mergin Maps, QGIS, Locus Map, OruxMaps | baza SQLite w standardzie OGC z piramidą kafli Web Mercator 256 px | standard OGC dla mobilnego GIS, natywnie czytany przez QField |
| **SQLiteDB** | OsmAnd, Locus Map | baza w formacie RMaps, kafle Web Mercator | natywny format map rastrowych OsmAnd |
| **OruxMaps** | OruxMaps | folder z bazą `OruxMapsImages.db` i kalibracją `.otrk2.xml` | natywny format map offline OruxMaps |

{: .wskazowka }
Nie wiesz, co wybrać? Do aplikacji w telefonie wybierz **MBTiles**, do mobilnego GIS (QField, Mergin Maps) **GeoPackage**, do Garmina z odblokowanym firmware **Garmin BirdsEye (JNX)** lub **KMZ dla Garmin**, do aplikacji Avenza Maps **Arkusz** w formacie GeoTIFF, a do obejrzenia w Google Earth zwykły **KMZ**.

---

## KMZ dla Garmin Custom Maps

Odbiorniki Garmin z funkcją *Custom Maps* (mapy niestandardowe) wyświetlają obrazy z pliku KMZ, ale mają ograniczenia:
kafle JPEG o boku najwyżej 1024 px i ograniczoną liczbę kafli (zazwyczaj do 100 kafli na urządzenie).

* *Maksymalna liczba kafli*: od 1 do 1000, domyślnie **100**.
* **Automatyczny podział na tomy:** jeśli mapa w pełnej rozdzielczości wymaga więcej niż 100 kafli, włącz opcję podziału na tomy. Program utworzy spójne części (`_tom1.kmz`, `_tom2.kmz` itd.) bez konieczności sztucznego obniżania rozdzielczości czy obcinania terenu.
* **Kolejność rysowania (z-order):** możesz określić warstwę wyświetlania mapy w odbiorniku (np. pod wektorową siatką dróg lub na wierzchu).
* **Filtr MIP dla Garmina:** opcja korekcji kontrastu, nasycenia i ostrości splotowej Laplaciana 3×3, specjalnie zoptymalizowana pod kątem ekranów Memory-In-Pixel, dzięki czemu mapa zachowuje maksymalną czytelność w ostrym słońcu.

---

## Garmin BirdsEye (JNX)

Format dedykowany dla odbiorników turystycznych Garmin (np. GPSMAP, eTrex, Montana, Oregon) z odblokowaną obsługą BirdsEye (tzw. patchowany firmware):

* **Brak limitu 100 kafli:** pozwala wyeksportować mapę obejmującą całe pasma górskie lub województwa w jednym pliku.
* **Wielopoziomowa piramida:** zawiera automatycznie wyliczone poziomy powiększenia od przeglądowego do szczegółowego.
* Gotowy plik `.jnx` wystarczy skopiować bezpośrednio do katalogu `Garmin/BirdsEye` w pamięci urządzenia lub na karcie microSD.

---

## Arkusz (scalanie segmentów)

Jeden plik obrazu z georeferencją w układzie sesji - ten sam format otwiera polecenie *Narzędzia > Scalanie segmentów*:

* **Formaty:** GeoTIFF bezstratny (Deflate), GeoTIFF z kompresją JPEG, JPEG (do 65 500 px na bok) albo PNG.
* **Zastosowanie:** Avenza Maps, QField, QGIS, ArcGIS i programy CAD.
* **Pliki towarzyszące:** world file i `.prj`, kalibracja OziExplorer `.map`, MapInfo `.tab`, Google Earth `.kml`.
* Szczegóły: [Scalanie segmentów](../../scalanie/).

---

## KMZ dla Locus Map, OruxMaps, Google Earth

Ten sam układ kafli co dla Garmina, ale bez limitu ich liczby, więc zawsze w pełnej rozdzielczości. Kafle są
w układzie WGS84. Program przelicza mapę z układu pobierania. Obszar KMZ to prostokąt w stopniach wpisany w zasięg siatki,
więc przy mapie w PL-1992 brzegi mogą być minimalnie przycięte.

Jako [nakładka](../nakladki/) KMZ zapisuje kafle PNG z przezroczystością.

---

## MBTiles

Standardowy format map kafelkowych: jedna baza SQLite z kaflami 256 × 256 px w odwzorowaniu Web Mercator,
na kilku poziomach powiększenia. Obsługuje go większość aplikacji mapowych.

* Zakres poziomów ustawia się w grupie *Kafle Web Mercator*. Szczegóły: [Poziomy powiększenia i rozmiar pliku](../powiekszenia/).
* Nakładka zapisuje typ mapy `overlay`, więc aplikacja wyświetla ją nad swoją mapą podkładową.
* Opis mapy (nazwa, opis, atrybucja ze źródłem danych) zapisywany jest w metadanych pliku.

Plik MBTiles z kilku pobranych map, każdej w innym zakresie powiększeń, tworzy
[MBTiles wieloskalowy](../../mbtiles-wielowarstwowy/).

---

## GeoPackage (.gpkg)

Międzynarodowy, otwarty standard Open Geospatial Consortium (OGC) dla danych geoprzestrzennych: jednoplikowa baza SQLite
zawierająca piramidę kafelków rastrowych Web Mercator (256 × 256 px).

* **Standard mobilnego GIS:** natywny format danych dla aplikacji terenowych **QField**, **Mergin Maps** i **SW Maps** oraz programu **QGIS**.
* **Kompatybilność:** pliki `.gpkg` odczytują również aplikacje turystyczne **Locus Map** i **OruxMaps**.
* **Struktura zgodna ze specyfikacją OGC:** kompletne tabele metadanych (`gpkg_contents`, `gpkg_spatial_ref_sys`, `gpkg_tile_matrix_set`, `gpkg_tile_matrix`) oraz siatka kafelków `tiles` numerowana w układzie Top-Left (XYZ).
* **Te same opcje co MBTiles:** wybór zakresu powiększeń, przezroczyste kafle PNG lub szybkie JPEG, szacowanie rozmiaru oraz filtr MIP dla Garmina.

---

## SQLiteDB

Baza w formacie RMaps (kafle Web Mercator), czytana przez OsmAnd i Locus Map. Te same opcje co MBTiles.
Wybierz SQLiteDB, gdy aplikacja nie obsługuje MBTiles albo gdy wolisz natywny format OsmAnd.

---

## OruxMaps: mapa offline

Natywny format OruxMaps: **folder** z bazą kafli `OruxMapsImages.db` i plikiem kalibracji `.otrk2.xml`.
Cały folder kopiuje się do telefonu. Te same opcje co MBTiles.

---

➡️ **Kolejny krok:** [Poziomy powiększenia i rozmiar pliku →](../powiekszenia/)

*Zobacz też: [Mapa w telefonie i odbiorniku GPS](../aplikacje/) · [Nakładki i łączenie warstw](../nakladki/) · [Zaawansowany MBTiles](../../mbtiles-wielowarstwowy/)*
