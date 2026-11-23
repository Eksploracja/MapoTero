---
title: Eksport mapy
nav_order: 12
has_children: true
---

# Eksport mapy
{: .no_toc }

Zapis pobranej mapy w formatach dla odbiorników GPS i aplikacji mobilnych, kafelkowanego GeoTIFF, a także bezpośredni transfer przez Wi-Fi z kodem QR lub USB.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Eksport mapy (KMZ, JNX, MBTiles, GeoPackage, SQLiteDB, OruxMaps, GeoTIFF)* (<kbd>Ctrl</kbd>+<kbd>E</kbd>) albo przycisk **Eksportuj pobraną mapę…** w karcie stanu okna głównego  
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
5. Po zakończeniu kliknij **Wyślij na telefon...** (aby zeskanować kod QR przez Wi-Fi) albo **Kopiuj na Garmina...** (kopiowanie na dysk USB).

![Okno eksportu mapy z ponumerowanymi sekcjami]({{ '/pomoc/obrazy/10-eksport-mapy.png' | relative_url }})

*Rysunek 1. Okno eksportu mapy programu MapoTero z ponumerowanymi sekcjami parametrów.*

| Nr | Sekcja okna | Opis |
|---|---|---|
| **1** | *Folder sesji* | folder eksportowanej sesji (`conf.txt`) oraz zestawienie: liczba segmentów, rozdzielczość piksela, układ, opis mapy i data pobrania |
| **2** | *Format* | wybór formatu: **Garmin BirdsEye (JNX)**, **KMZ dla Garmin** (z automatycznym podziałem na tomy), uniwersalny **KMZ**, **MBTiles**, nowoczesny **GeoPackage (.gpkg)**, **SQLiteDB**, **OruxMaps**, **Arkusz** (scalanie: kafelkowany GeoTIFF z piramidami, JPEG, PNG) oraz **TrekBuddy / Locus Map (.tar)** |
| **3** | *Kafle Web Mercator* | zakres poziomów powiększenia (zoom min–max) oraz opcja przezroczystych kafli PNG |
| **4** | *Warstwy, przezroczystość i filtr MIP* | nakładka, zamiana białego tła na przezroczyste, łączenie z warstwami innych sesji oraz profil korekty obrazu / filtr MIP dla ekranów Garmin |
| **5** | *Rozmiar pliku* i *Jakość JPEG* | dynamiczne szacowanie wagi pliku w MB, dopasowanie poziomu do zadanego limitu oraz suwak jakości kompresji JPEG |
| **6** | Plan eksportu i przyciski | podsumowanie liczby generowanych kafelków, przycisk **Pomoc** (<kbd>F1</kbd>), **Eksportuj**, a po zakończeniu: **Pokaż w folderze**, **Wyślij na telefon...** i **Kopiuj na Garmina...** |

---

## Nowe funkcje eksportu

* **Bezpośredni transfer Wi-Fi z kodem QR:** po wygenerowaniu mapy przycisk **Wyślij na telefon...** uruchamia wbudowany serwer lokalny i wyświetla kod QR. Wystarczy skierować aparat smartfona na ekran, aby pobrać mapę do telefonu bez podłączania kabla USB i bez wysyłania plików do chmury internetowej.
* **Wykrywanie odbiorników Garmin przez USB:** przycisk **Kopiuj na Garmina...** automatycznie wykrywa podłączony odbiornik w trybie pamięci masowej USB i kopiuje plik do odpowiedniego folderu (`Garmin/CustomMaps` dla KMZ lub `Garmin/BirdsEye` dla JNX).
* **Format OGC GeoPackage (.gpkg):** standard otwartej bazy danych kafelków Web Mercator (EPSG:3857), bezpośrednio obsługiwany przez aplikacje mobilne GIS (QField, Mergin Maps) oraz turystyczne (Locus Map, OruxMaps, QGIS).
* **Kafelkowany GeoTIFF z piramidami (Cloud-Optimized GeoTIFF):** format *Arkusz > GeoTIFF* tworzy kafelkowanie 256×256 i wewnętrzny łańcuch pomniejszeń (IFD reduced overviews), dzięki czemu wielogigabajtowe arkusze otwierają się w programach QGIS, ArcGIS czy Avenza Maps natychmiastowo.
* **Automatyczny podział KMZ na tomy:** przy eksporcie map przekraczających limit 100 kafli odbiornika Garmin program oferuje inteligentny podział na tomy (`_tom1.kmz`, `_tom2.kmz`) bez konieczności sztucznego obniżania rozdzielczości.
* **Płynne skalowanie okna:** na mniejszych ekranach laptopów okno eksportu wyświetla pionowy pasek przewijania, gwarantując pełny dostęp do wszystkich opcji i przycisków.

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
