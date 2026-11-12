---
title: Scalanie segmentów
nav_order: 11
---

# Scalanie segmentów
{: .no_toc }

Złączenie pobranych segmentów w jeden duży arkusz GeoTIFF, JPEG lub PNG z plikami kalibracji.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Łączenie segmentów* (<kbd>Ctrl</kbd>+<kbd>L</kbd>) lub ikona szachownicy w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najkrótsza droga

1. Pobierz wszystkie segmenty mapy (w folderze nie może być pliku `error.txt`).
2. Naciśnij <kbd>Ctrl</kbd>+<kbd>L</kbd> albo kliknij ikonę szachownicy w oknie głównym.
3. Sprawdź wybrany *Format scalonego arkusza* (domyślnie **GeoTIFF (bezstratny)**) i zaznacz ewentualne pliki referencyjne.
4. Kliknij **Złącz segmenty**.
5. Gotowy plik `_scalone_segmenty_<kol>x<wier>.tif` (lub `.jpg`/`.png`) powstanie w folderze sesji.

![Okno Złączanie segmentów w jeden plik]({{ '/pomoc/obrazy/09-scalanie.png' | relative_url }})

*Rysunek 1. Okno łączenia pobranych segmentów w jeden duży arkusz.*

---

## Folder segmentów i conf.txt

Po otwarciu okna program automatycznie wskazuje folder bieżącej sesji (gdy zawiera plik `conf.txt`, w przeciwnym razie `download\`)
i wczytuje parametry siatki, format segmentów, przedrostek, styl numeracji oraz informację o przezroczystości.

* **zmień folder**: pozwala wskazać dowolny inny katalog z wcześniej pobraną mapą.
* **error.txt blokuje scalanie**: dopóki w folderze istnieje plik `error.txt` (oznaczający niekompletne pobranie), scalanie nie zostanie uruchomione. Należy dopobrać brakujące segmenty albo, jeśli brak jest zamierzony, usunąć plik `error.txt` ręcznie.
* **ręczne wprowadzanie parametrów w razie braku conf.txt**: opcja aktywna tylko wtedy, gdy w folderze brakuje pliku `conf.txt`. Pozwala ręcznie podać zasięg XY, liczbę kolumn i wierszy, bok segmentu, piksel, przedrostek, format i styl numeracji.

{: .informacja }
Jeśli sesja była pobierana wzdłuż trasy (korytarz GPX/KML), brak segmentów poza wyznaczonym pasem jest zamierzony. Scalony arkusz powstanie bez przeszkód, a teren poza korytarzem zostanie wypełniony kolorem białym (lub przezroczystością w formatach PNG i GeoTIFF).

---

## Format arkusza

| Format | Zastosowanie | Ograniczenia i cechy |
|---|---|---|
| **GeoTIFF (bezstratny)** *(domyślny)* | QGIS, ArcGIS, Global Mapper, archiwum bez utraty jakości | kompresja Deflate, wbudowana georeferencja, **zachowuje przezroczyste tło** z PNG/GIF, automatyczny BigTIFF powyżej 4 GB |
| **GeoTIFF (kompresja JPEG)** | mniejsze pliki GeoTIFF dla ortofotomap | wbudowana georeferencja, kompresja stratna z regulacją jakości, białe tło zamiast przezroczystości |
| **JPEG** | szybki podgląd, programy CAD (z plikiem `.jpgw`), zdjęcia | maksymalny bok obrazu **65 500 px**, białe tło zamiast przezroczystości |
| **PNG** | nakładki, mapy z przezroczystym tłem do grafiki lub CAD | bezstratny, **zachowuje przezroczystość**, wymaga pliku kalibracji obok obrazu (`.pngw`, `.map` lub `.tab`) |

---

## Jakość kompresji JPEG

Suwak **Jakość kompresji JPEG scalonego pliku** (`0–100%`, domyślnie `75%`) jest aktywny wyłącznie wtedy, gdy wybrano
format **JPEG** albo **GeoTIFF (kompresja JPEG)**. Wyższa wartość daje ostrzejsze napisy kosztem większego rozmiaru pliku.

---

## Pliki referencyjne arkusza

Wybrane pola są zapamiętywane między uruchomieniami programu:

* *twórz plik referencyjny OziExplorer (.map)*
* *twórz plik referencyjny GoogleEarth (.kml)*
* *twórz plik referencyjny QGIS, ArcGis (.tifw / .jpgw / .pngw)*: program automatycznie dobiera rozszerzenie world file do wybranego formatu arkusza i dopisuje obok plik `.prj` z definicją układu.
* *twórz plik referencyjny MapInfo (.tab)*

{: .wskazowka }
Jeśli uruchamiasz scalanie bezpośrednio przyciskiem **scal teraz...** z okna *Eksport do CAD*, program automatycznie
zaznacza tworzenie pliku world file, aby CAD mógł od razu wstawić arkusz na właściwe współrzędne.

---

## Bardzo duże arkusze

MapoTero scala segmenty strumieniowo, pasami wierszy. Zużycie pamięci RAM zależy tylko od szerokości pojedynczego rzędu segmentów, a nie od całkowitej powierzchni mapy. Nawet arkusz o boku kilkudziesięciu tysięcy pikseli i wadze powyżej 4 GB (zapisywany automatycznie w standardzie BigTIFF) nie przepełni pamięci RAM, wymagając jedynie wolnego miejsca na dysku.

W trakcie scalania przycisk **Złącz segmenty** zmienia się na **Przerwij**. Operację można bezpiecznie zatrzymać w dowolnej chwili.

---

➡️ **Kolejny krok:** [Eksport mapy dla aplikacji mobilnych i GPS →](../eksport/)

*Zobacz też: [Pliki kalibracji](../pliki-kalibracji/) · [Eksport do CAD](../cad/) · [Współpraca z innymi programami](../inne-programy/)*
