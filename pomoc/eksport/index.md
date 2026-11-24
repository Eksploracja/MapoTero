---
title: Eksport mapy
nav_order: 12
has_children: true
---

# Eksport mapy
{: .no_toc }

Zapis pobranej mapy w formatach dla odbiorników GPS i aplikacji mobilnych, kafelkowanego GeoTIFF, a także bezpośredni transfer przez Wi-Fi z kodem QR lub USB.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Eksport z pobranych segmentów…* (<kbd>Ctrl</kbd>+<kbd>E</kbd>) albo przycisk **Eksportuj pobraną mapę…** w karcie *Stan sesji* okna głównego  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najkrótsza droga

1. Pobierz segmenty mapy (sesja musi mieć plik `conf.txt`).
2. Naciśnij <kbd>Ctrl</kbd>+<kbd>E</kbd> (lub kliknij przycisk **Eksportuj pobraną mapę…**). Okno otworzy się z automatycznie wczytaną sesją.
3. Wybierz format docelowy (zob. [Wybór formatu](formaty/)).
4. Kliknij **Eksportuj**.
5. Po zakończeniu kliknij **Wyślij na telefon...** (dla kodów QR przez Wi-Fi albo wysyłki na Garmina przez USB). Pliki dla Garmina skopiujesz też przez *Narzędzia > Kopiuj mapę do Garmina (USB)...*.

![Okno eksportu mapy z ponumerowanymi sekcjami]({{ '/pomoc/obrazy/10-eksport-mapy.png' | relative_url }})

*Rysunek 1. Okno eksportu mapy programu MapoTero z ponumerowanymi sekcjami parametrów.*

| Nr | Sekcja okna | Opis |
|---|---|---|
| **1** | *Folder sesji* | folder eksportowanej sesji (`conf.txt`) oraz zestawienie: liczba segmentów, rozdzielczość piksela, układ, opis mapy i data pobrania |
| **2** | *Format* | wybór formatu: **Garmin BirdsEye (JNX)**, **KMZ dla Garmin** (z automatycznym podziałem na tomy), uniwersalny **KMZ**, **MBTiles** (i przycisk **Wieloskalowy...**), **GeoPackage (.gpkg)**, **SQLiteDB**, **OruxMaps**, **Arkusz** (scalanie z opcjami kalibracji i piramid w GeoTIFF) oraz **TrekBuddy / Locus Map (.tar)** |
| **3** | *Kafle Web Mercator* | zakres poziomów powiększenia (zoom min–max) oraz opcja przezroczystych kafli PNG (przy formacie *Arkusz* tutaj pojawiają się *Opcje arkusza*: siatka km i ślad GPX/KML) |
| **4** | *Warstwy, przezroczystość i korekta obrazu* | nakładka, zamiana białego tła na przezroczyste, łączenie z warstwami innych sesji oraz korekta obrazu (m.in. filtr MIP dla ekranów Garmin) |
| **5** | *Rozmiar pliku* i *Jakość JPEG* | dynamiczne szacowanie wagi pliku w MB, dopasowanie poziomu do zadanego limitu oraz suwak jakości kompresji JPEG |
| **6** | Plan eksportu i przyciski | podsumowanie liczby generowanych kafelków, przycisk **Pomoc** (<kbd>F1</kbd>), **Eksportuj**, a po zakończeniu: **Pokaż w folderze** i **Wyślij na telefon...** |

---

## Nowe funkcje eksportu

* **Bezpośredni transfer Wi-Fi z kodem QR:** po wygenerowaniu mapy przycisk **Wyślij na telefon...** uruchamia lokalny serwer i wyświetla kod QR. Skieruj aparat smartfona na ekran, aby pobrać mapę do telefonu bez kabla USB i bez wysyłania plików do chmury.
* **Wykrywanie odbiorników Garmin przez USB:** dla wyeksportowanych plików `.jnx` i `.kmz` przycisk **Wyślij na telefon...** (oraz polecenie *Narzędzia > Kopiuj mapę do Garmina (USB)...*) wykrywa podłączony odbiornik w trybie pamięci masowej USB i kopiuje plik do odpowiedniego folderu (`Garmin/CustomMaps` lub `Garmin/BirdsEye`).
* **Format OGC GeoPackage (.gpkg):** standard otwartej bazy danych kafelków Web Mercator (EPSG:3857), obsługiwany bezpośrednio przez aplikacje mobilne GIS (QField, Mergin Maps) oraz turystyczne (Locus Map, OruxMaps, QGIS).
* **Kafelkowany GeoTIFF z piramidami (Cloud-Optimized GeoTIFF):** po wybraniu *Arkusz > GeoTIFF* i zaznaczeniu *piramidy w GeoTIFF* powstaje plik z kafelkowaniem 256×256 i wewnętrznym łańcuchem pomniejszeń (IFD reduced overviews), który w QGIS, ArcGIS czy Avenza Maps otwiera się bez wczytywania całego obrazu do RAM.
* **Automatyczny podział KMZ na tomy:** przy eksporcie map przekraczających limit 100 kafli odbiornika Garmin program dzieli wynik na tomy (`_tom1.kmz`, `_tom2.kmz`) bez obniżania rozdzielczości.
* **Dopasowanie do mniejszych ekranów:** na mniejszych ekranach laptopów okno eksportu wyświetla pionowy pasek przewijania z pełnym dostępem do wszystkich opcji i przycisków.

---

## Komunikaty przy otwieraniu sesji

* *Segmenty gotowe do eksportu.* Wszystkie pliki sesji są kompletne.
* *W folderze jest plik error.txt: zestaw segmentów jest niekompletny…* Brakujące segmenty będą pustymi miejscami na mapie. Warto najpierw [dopobrać je w oknie głównym](../pobieranie/przebieg/#wznawianie).
* *Segmenty pobrane wzdłuż trasy (korytarz)…* Poza korytarzem trasy mapa będzie pusta (przezroczysta w kaflach PNG lub biała w JPEG).

---

## Pliki wynikowe

Plik mapy zapisywany jest bezpośrednio **w folderze bieżącej sesji**:

| Format | Przykładowa nazwa pliku |
|---|---|
| KMZ dla Garmin | `_eksport_10x8_garmin.kmz` (lub `..._tom1.kmz`) |
| KMZ | `_eksport_10x8.kmz` |
| Garmin BirdsEye | `_eksport_10x8.jnx` |
| MBTiles | `_eksport_10x8.mbtiles` |
| OGC GeoPackage | `_eksport_10x8.gpkg` |
| SQLiteDB | `_eksport_10x8.sqlitedb` |
| OruxMaps | folder `_eksport_10x8_oruxmaps` |
| Arkusz GeoTIFF | `_scalone_segmenty_10x8.tif` |
| Paczka TrekBuddy | `_eksport_10x8.tar` |

Kolejny eksport tego samego formatu w tym samym folderze **zastępuje** poprzedni plik. Jeśli chcesz zachować kilka wariantów, zmień nazwę pliku po zakończeniu eksportu.

---

## Czas trwania eksportu

Eksport trwa zazwyczaj od kilkunastu sekund do kilku minut, w zależności od liczby generowanych kafelków i wybranego formatu kompresji. Gdy plan przekracza 2 miliony kafli, program wyświetla ostrzeżenie z pytaniem o potwierdzenie. Puste kafle (np. poza wyznaczonym korytarzem trasy) są automatycznie pomijane, co drastycznie oszczędza czas i miejsce na dysku.

---

➡️ **Kolejny krok:** [Szczegółowy przegląd formatów eksportu →](formaty/)

*Zobacz też: [Wieloskalowy MBTiles](../mbtiles-wielowarstwowy/) · [Eksport do CAD](../cad/) · [Scalanie segmentów w arkusz](../scalanie/)*
