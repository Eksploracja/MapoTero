---
title: Scalanie segmentów
nav_order: 11
---

# Scalanie segmentów
{: .no_toc }

Złączenie pobranych segmentów w jeden duży arkusz GeoTIFF, JPEG lub PNG z plikami kalibracji - format *Arkusz* w oknie *Eksport mapy*.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Scalanie segmentów* (<kbd>Ctrl</kbd>+<kbd>L</kbd>), ikona szachownicy w oknie głównym albo okno *Eksport mapy* z wybranym formatem *Arkusz*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

{: .informacja }
Od wersji 3.19 scalanie nie ma osobnego okna. Polecenie *Scalanie segmentów* otwiera okno [*Eksport mapy*](../eksport/)
z zaznaczonym formatem *Arkusz*. Arkusz ma wszystkie formaty i pliki kalibracji dawnego okna scalania, a dodatkowo
nakładki z innych folderów i korektę obrazu.

## Najkrótsza droga

1. Pobierz wszystkie segmenty mapy.
2. Naciśnij <kbd>Ctrl</kbd>+<kbd>L</kbd> albo kliknij ikonę szachownicy w oknie głównym - otworzy się okno *Eksport mapy* z zaznaczonym formatem *Arkusz*.
3. Wybierz format arkusza na liście obok (domyślnie **GeoTIFF (kompresja JPEG)**) i zaznacz pliki georeferencji: *world file i .prj*, *.map*, *.tab*, *.kml*.
4. Kliknij **Eksportuj**.
5. Gotowy plik `_scalone_segmenty_<kol>x<wier>.tif` (lub `.jpg`/`.png`) powstanie w folderze sesji.

---

## Folder sesji

Okno pokazuje folder bieżącej sesji pobierania (podfolder `download\sesja_<zbior>_<timestamp>\` z plikiem `conf.txt`)
i wczytuje z `conf.txt` siatkę, format segmentów, przedrostek, styl numeracji i układ współrzędnych. Gdy folder bieżącej
sesji nie ma pliku `conf.txt`, okno otwiera najnowszą sesję z `download\` i pisze o tym w polu komunikatów.
Przycisk **Zmień folder** wskazuje inną sesję.

* **error.txt**: plik oznacza niedokończone pobieranie. Okno ostrzega o nim po otwarciu - w arkuszu brakujące segmenty będą białymi polami. Dopobierz je przyciskiem *Pobierz* w oknie głównym.
* **Brakujące segmenty**: po zapisaniu arkusza okno podaje, ilu segmentów brakowało, i wymienia kilka z nich.

{: .informacja }
Jeśli sesja była pobierana wzdłuż trasy (korytarz GPX/KML), brak segmentów poza wyznaczonym pasem jest zamierzony i okno go nie zgłasza. Teren poza korytarzem zostaje biały (albo przezroczysty w formatach PNG i GeoTIFF bezstratny, gdy zaznaczono kafle PNG lub nakładkę).

---

## Format arkusza

| Format | Zastosowanie | Ograniczenia i cechy |
|---|---|---|
| **GeoTIFF (bezstratny)** | QGIS, ArcGIS, Global Mapper, archiwum bez utraty jakości | kompresja Deflate, wbudowana georeferencja, może zachować przezroczyste tło, automatyczny BigTIFF powyżej 4 GB |
| **GeoTIFF (kompresja JPEG)** *(domyślny)* | mniejsze pliki GeoTIFF dla ortofotomap, Avenza Maps | wbudowana georeferencja, kompresja stratna (pole *Jakość JPEG*), białe tło zamiast przezroczystości |
| **JPEG** | szybki podgląd, programy CAD (z plikiem `.jpgw`), zdjęcia | maksymalny bok obrazu **65 500 px**, białe tło zamiast przezroczystości |
| **PNG** | nakładki, mapy z przezroczystym tłem do grafiki lub CAD | bezstratny, może zachować przezroczystość, wymaga pliku kalibracji obok obrazu (`.pngw`, `.map` lub `.tab`) |

Jakość kompresji JPEG ustawia pole *Jakość JPEG* w dolnej części okna eksportu.

---

## Pliki georeferencji arkusza

Pola pod formatem *Arkusz* są zapamiętywane między uruchomieniami programu:

* *world file i .prj* - world file z rozszerzeniem dobranym do formatu (`.tifw`, `.jpgw`, `.pngw`; przy GeoTIFF także `.tfw` dla programów CAD) i plik `.prj` z definicją układu,
* *.map* - plik kalibracji OziExplorer,
* *.tab* - plik MapInfo,
* *.kml* - nakładka Google Earth.

{: .wskazowka }
Przycisk **Scal segmenty...** w oknie *Eksport do CAD* otwiera okno eksportu z formatem *Arkusz* dla sesji z okna CAD.
Po zapisaniu arkusza i zamknięciu okna eksportu okno CAD wybiera ten arkusz do wstawienia. Zaznacz *world file i .prj*,
jeśli arkusz ma wstawiać program CAD bez obsługi GeoTIFF.

---

## Nakładki i korekta obrazu

Na arkusz można nałożyć warstwy z innych folderów sesji (grupa *Warstwy i przezroczystość*, np. granice działek na
ortofotomapę) - także o innej siatce i w innym układzie. Pole *Korekta obrazu* z przyciskiem *Profil / Podgląd...*
poprawia kontrast, nasycenie i ostrość całego arkusza. Zob. [Nakładki](../eksport/nakladki/).

---

## Bardzo duże arkusze

MapoTero zapisuje arkusz pasami wierszy. Zużycie pamięci RAM zależy od szerokości arkusza, a nie od jego całkowitej powierzchni. Arkusz o boku kilkudziesięciu tysięcy pikseli i wadze powyżej 4 GB (zapisywany automatycznie w standardzie BigTIFF) wymaga tylko wolnego miejsca na dysku.

Program zapisuje arkusz najpierw do pliku tymczasowego. Przycisk **Przerwij** zatrzymuje zapis w dowolnej chwili, a poprzedni arkusz o tej samej nazwie zostaje na dysku.

---

➡️ **Kolejny krok:** [Eksport mapy dla aplikacji mobilnych i GPS →](../eksport/)

*Zobacz też: [Pliki kalibracji](../pliki-kalibracji/) · [Eksport do CAD](../cad/) · [Współpraca z innymi programami](../inne-programy/)*
