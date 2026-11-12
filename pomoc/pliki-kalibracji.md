---
title: Pliki kalibracji
nav_order: 10
---

# Pliki kalibracji
{: .no_toc }

Pliki georeferencji zapisywane obok segmentów i scalonych arkuszy: który program czyta dany format.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Ustawienia > Pliki kalibracji segmentów* oraz okno *Łączenie segmentów*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Przegląd

Domyślnie MapoTero nie tworzy plików kalibracji obok pojedynczych segmentów, bo do scalania i eksportu w zupełności
wystarczy plik sesji `conf.txt`. Pliki kalibracji włącza się wtedy, gdy obrazy chcesz otworzyć bezpośrednio w programach
GIS, CAD lub nawigacyjnych.

| Plik | Rozszerzenie | Do jakich programów | Co zawiera |
|---|---|---|---|
| **World file + PRJ** | `.jpgw`, `.pngw`, `.tifw`, `.gifw` + `.prj` | QGIS, ArcGIS, AutoCAD Map / Civil 3D, Global Mapper | 6 parametrów położenia i skali piksela oraz definicja układu (WKT) w pliku `.prj` |
| **GeoTIFF** *(tylko scalony arkusz)* | `.tif` | QGIS, ArcGIS, Global Mapper, CAD z obsługą GeoTIFF | georeferencja zapisana wewnątrz samego pliku obrazu (tagi GeoTIFF) |
| **Google Earth** | `.kml` | Google Earth Pro | nakładka obrazu z dokładnymi narożnikami `gx:LatLonQuad` oraz obróconym `LatLonBox` |
| **OziExplorer** | `.map` | OziExplorer, starsze narzędzia GPS | punkty kalibracyjne narożników, układ odwzorowania, ścieżka obrazu |
| **MapInfo** | `.tab` | MapInfo Pro | tabela definicji rastra z 4 narożnikami i zapisem `CoordSys` |
| **WLD + Points** | `.wld` + `.<ext>.points` | starsze wersje QGIS (wtyczka georeferencera), narzędzia GDAL | world file (`.wld`) oraz tabela punktów kontrolnych CSV |
| **GPS Tuner** | `.gmi` | GPS Tuner 5.x | punkty kontrolne w formacie tekstowym GPS Tunera |
| **TrekBuddy / Locus** | `.tar` (z `.map` i `.set`) | TrekBuddy, starsze wersje Locus Map | archiwum z kafelkami `512 × 512 px` w podfolderze `set` i kalibracją |

## World file i .prj (QGIS, ArcGIS)

Podstawowy wybór dla programów GIS oraz CAD. Dla każdego obrazu program generuje dwa pliki:

* **world file**: rozszerzenie zależy od formatu obrazu (`.jpgw` dla JPEG, `.pngw` dla PNG, `.tifw` dla TIFF, `.gifw` dla GIF). Zawiera rozmiar piksela w poziomie i pionie oraz współrzędne **środka lewego górnego piksela** w układzie pobierania.
* **`.prj`**: definicja układu współrzędnych w formacie WKT (EPSG:2180, PL-2000, UTM lub WGS84). Programy QGIS i ArcGIS odczytują układ automatycznie bez pytania użytkownika o parametry.

---

## Google Earth (.kml)

Plik `.kml` otwiera segment lub scalony arkusz bezpośrednio w Google Earth Pro jako nakładkę terenu.
Program zapisuje cztery narożniki w WGS84 (`gx:LatLonQuad`) oraz kąt obrotu dla `LatLonBox`. Obrazy pobrane w PL-1992 lub PL-2000 są precyzyjnie dopasowane również na krańcach Polski (uwzględniają zbieżność południków).

---

## OziExplorer (.map)

Klasyczny plik kalibracji programu OziExplorer. Zawiera współrzędne geograficzne oraz współrzędne siatki czterech
narożników obrazu, nazwę elipsoidy/układu i odwołanie do pliku graficznego.

---

## MapInfo (.tab)

Plik tekstowy w kodowaniu `WindowsLatin2` opisujący raster jako `Definition Table` z czterema punktami kontrolnymi
(lewy górny, prawy górny, prawy dolny, lewy dolny) oraz definicją układu `CoordSys` dla MapInfo.

---

## Pliki .wld i .points

Zestaw dla starszych narzędzi GIS oraz wtyczki georeferencji: plik `.wld` (world file o stałym rozszerzeniu niezależnym
od formatu obrazu) wraz z plikiem `.jpg.points` / `.png.points` (lista punktów narożnych w układzie sesji).

---

## GPS Tuner (.gmi)

Prosty plik tekstowy zapisujący wymiary obrazu w pikselach i dwie pary współrzędnych narożników dla programu GPS Tuner 5.x.

---

## TrekBuddy i Locus Map (.tar)

W trybie TrekBuddy (grupa *TrekBuddy i Locus Map* w oknie *Ustawienia*) program dzieli pobieraną mapę na segmenty `512 × 512 px` w podfolderze `set`, tworzy plik kalibracji `.map` oraz skorowidz `.set` i pakuje całość do jednego archiwum `.tar`.

---

## Dokładność kalibracji

* **Liczby z kropką dziesiętną.** Wszystkie pliki kalibracji stosują zapis liczb z kropką dziesiętną (`InvariantCulture`), co zapewnia bezbłędny odczyt w środowiskach GIS i CAD.
* **Środek piksela w world file.** MapoTero zapisuje współrzędne środka lewego górnego piksela zgodnie ze standardem ESRI i GDAL.
* **Zgodność z układem sesji.** Poza plikami KML (które zawsze używają stopni WGS84) pliki kalibracji powstają w układzie wybranym w oknie głównym. Opcja zamiany osi X i Y z ustawień nie wpływa na współrzędne w plikach kalibracji.

---

➡️ **Kolejny krok:** [Scalanie pobranych segmentów w jeden arkusz →](../scalanie/)

*Zobacz też: [Współpraca z innymi programami](../inne-programy/) · [Ustawienia](../ustawienia/#pliki-kalibracji-segmentów)*
