---
title: Współpraca z innymi programami
nav_order: 18
---

# Współpraca z innymi programami
{: .no_toc }

Jak otworzyć pobrane segmenty i scalone arkusze w popularnych programach GIS i nawigacyjnych.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## QGIS

Najwygodniejszym formatem dla QGIS jest scalony arkusz **GeoTIFF** (zapisuje układ współrzędnych i położenie wewnątrz pliku):

1. Scal segmenty poleceniem *Narzędzia > Łączenie segmentów* (<kbd>Ctrl</kbd>+<kbd>L</kbd>) z formatem **GeoTIFF (bezstratny)**.
2. Przeciągnij powstały plik `_scalone_segmenty_….tif` z Eksploratora prosto do okna mapy QGIS.
3. Jeśli eksportujesz w formacie **JPEG** lub **PNG** (albo chcesz wczytać pojedyncze segmenty), zaznacz tworzenie **world file i `.prj`**. Obok obrazu powstaną pliki np. `.jpgw` i `.prj`, a QGIS wczyta układ automatycznie.

{: .wskazowka }
Jeśli chcesz w QGIS otworzyć od razu całą mozaikę kilkudziesięciu pojedynczych segmentów (z plikami `.jpgw` i `.prj`),
użyj w QGIS narzędzia *Raster > Różne > Buduj wirtualny raster (katalog)*, zamiast przeciągać każdy plik osobno.

### Przeniesienie podkładu w teren (QField)

Jeśli przygotowujesz projekt do pracy w terenie w mobilnej aplikacji **QField** (na telefonie lub tablecie):

1. W oknie [Eksport mapy](../eksport/) wyeksportuj pobraną mapę do formatu **MBTiles**.
2. Dodaj plik `.mbtiles` do projektu QGIS na komputerze jako warstwę rastrową.
3. Zsynchronizuj projekt z urządzeniem mobilnym (przez kabel lub QFieldCloud). Podkład MBTiles działa w terenie w 100% offline i nie obciąża pamięci telefonu przy zbieraniu punktów i pomiarów. Zobacz też: [Mapa w telefonie i odbiorniku GPS: QField](../eksport/aplikacje/#qfield-android-ios).

---

## ArcGIS

ArcGIS Pro i ArcMap odczytują bezpośrednio:

* **GeoTIFF** ze scalania (wbudowany układ EPSG:2180, PL-2000 lub UTM),
* **JPEG / PNG** z plikiem world file (`.jpgw` / `.pngw`) oraz towarzyszącym plikiem definicji układu `.prj`.

---

## Google Earth

* **Cała mapa:** w oknie [Eksport mapy](../eksport/) wybierz format **KMZ (Locus Map, OruxMaps, Google Earth)** (bez limitu kafli)
  i otwórz plik `_eksport_….kmz` w Google Earth Pro.
* **Scalony arkusz lub pojedyncze segmenty:** podczas scalania (albo w ustawieniach segmentów) zaznacz plik referencyjny `.kml`.
  MapoTero zapisuje dokładne narożniki `gx:LatLonQuad` oraz obrót `LatLonBox`, więc mapa pasuje dokładnie na całym obszarze Polski.

---

## OziExplorer

Włącz generowanie pliku **`.map`** w *Ustawieniach* (dla segmentów) lub w oknie *Łączenie segmentów* (dla całego arkusza).
Otwórz plik `.map` w OziExplorerze (*File > Load from File > Load Map File*). Pamiętaj, aby nie przenosić pliku obrazu
bez towarzyszącego mu `.map` do innego katalogu (w pliku `.map` zapisana jest ścieżka pliku graficznego).

---

## MapInfo

Zaznacz tworzenie pliku **`.tab`** podczas scalania lub w ustawieniach segmentów. W MapInfo Pro otwórz bezpośrednio
plik `.tab` (*Open Table*). Plik zawiera cztery narożniki oraz zapis `CoordSys` w kodowaniu `WindowsLatin2`.

---

## GPS Tuner

W starszych urządzeniach i oprogramowaniu GPS Tuner 5.x zaznacz w *Ustawieniach* opcję **plik .gmi / GPS Tuner 5.x**
i wskaż wygenerowane pliki kalibracji `.gmi` leżące obok segmentów.

---

➡️ **Kolejny krok:** [Rozwiązywanie problemów i najczęstsze pytania →](../problemy/)

*Zobacz też: [Pliki kalibracji](../pliki-kalibracji/) · [Scalanie segmentów](../scalanie/) · [Mapa w telefonie i odbiorniku GPS](../eksport/aplikacje/)*
