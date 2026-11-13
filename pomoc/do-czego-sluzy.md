---
title: Do czego służy MapoTero
nav_order: 2.5
---

# Do czego służy MapoTero
{: .no_toc }

Przeglądarki internetowe pozwalają oglądać mapy z państwowych serwerów tylko na ekranie komputera. MapoTero służy do tego, żeby te same mapy pobrać na dysk, połączyć w jeden plik i zabrać ze sobą w teren albo wstawić do projektu technicznego.
{: .fs-6 }

**Dotyczy wersji:** {{ site.wersja_programu }}

---

## Cztery główne zastosowania

### 1. Eksploracja terenu i poszukiwanie śladów historii
To z tej potrzeby narodził się program na Pomorskim Forum Eksploracyjnym. W lesie albo na bagnach nie ma zasięgu sieci komórkowej, a odczytanie terenu wymaga porównania kilku źródeł:
* **Nakładanie map archiwalnych na współczesne:** dawne mapy topograficzne (WIG, pruskie *Messtischblatty*, mapy zaborcze) nałożone na dzisiejsze zdjęcia lotnicze ujawniają zatarte dukty, fundamenty dawnych osad, leśniczówek, młynów czy zapomnianych cmentarzy.
* **Numeryczny model terenu (cieniowanie LiDAR):** rzeźba terenu pobrana z Geoportalu pokazuje ukryte pod koronami drzew transzeje, okopy, kurhany i dawne wały graniczne.
* **Mapa zawsze pod ręką:** gotowy plik w telefonie działa bez internetu i bez opóźnień.

### 2. Wyprawy, turystyka i nawigacja bez internetu
Wędrówki z plecakiem, wyprawy rowerowe, kajakowe i trasy offroad z dala od głównych dróg:
* **Wielowarstwowa mapa w jednym pliku:** w formacie MBTiles mapa zmienia się razem z powiększeniem ekranu. Z daleka widać ogólny zarys okolicy, a po przybliżeniu włącza się szczegółowa mapa leśna, topograficzna lub ortofotomapa.
* **Pobieranie tylko wzdłuż trasy:** narzędzie korytarza wycina z mapy pas o wybranej szerokości wzdłuż śladu GPX. Zamiast pobierać całe województwo, dostajesz dokładnie ten fragment terenu, przez który idzie szlak.
* **Współpraca ze sprzętem turystycznym:** pliki KMZ trafiają bezpośrednio do odbiorników Garmin, a formaty MBTiles, SQLiteDB i OruxMaps do aplikacji Locus Map, OsmAnd, Guru Maps (na iPhone i Androida), OruxMaps czy AlpineQuest na telefonie.

### 3. Podkłady do projektowania w CAD (AutoCAD, BricsCAD, ZWCAD)
Projektanci dróg, sieci uzbrojenia terenu, architekci krajobrazu i geodeci potrzebują aktualnego tła fotograficznego lub ewidencyjnego:
* **Automatyczne wpasowanie w układzie współrzędnych:** program generuje plik DXF, skrypt SCR albo program LISP, który wstawia wycinki mapy we właściwe miejsce rysunku (np. w układzie PL-2000 lub PL-1992).
* **Bez żmudnej ręcznej kalibracji:** raster siada dokładnie na współrzędnych projektowych, w metrach, bez konieczności kupowania płatnych wtyczek geodezyjnych.

### 4. Płynna praca w programach GIS (QGIS, ArcGIS, QField)
Podłączanie serwerów WMS bezpośrednio w programie QGIS bywa uciążliwe przy wolnym łączu albo przeciążeniu serwera:
* **Jeden duży arkusz GeoTIFF na dysku:** MapoTero pobiera wybrany obszar, scala segmenty i zapisuje plik z georeferencją oraz kompresją.
* **Błyskawiczne działanie:** praca na lokalnym pliku z dysku jest natychmiastowa, bez czekania na dociąganie kafelków przy każdym przesunięciu widoku.
* **Podkłady w terenie (QField):** pliki MBTiles wyeksportowane z MapoTero działają w mobilnym QFieldzie na telefonie lub tablecie, służąc leśnikom, przyrodnikom i geodetom jako szybki podkład pod zbierane obiekty.

---

## Jakie mapy można pobrać

W programie jest gotowy katalog ponad 30 polskich serwisów mapowych, a kreator pozwala dodać dowolny inny serwer WMS lub WMTS:
* **Ortofotomapy:** zdjęcia lotnicze i satelitarne o wysokiej rozdzielczości (teren całego kraju).
* **Mapy topograficzne:** od map przeglądowych po szczegółowe skale 1:10 000.
* **Cieniowanie rzeźby terenu:** numeryczny model terenu oparty o skanowanie laserowe LiDAR.
* **Ewidencja gruntów i budynków:** granice i numery działek z Krajowej Integracji Ewidencji Gruntów (KIEG).
* **Przyroda i lasy:** Bank Danych o Lasach (BDL), granice form ochrony przyrody (GDOŚ), mapy hydrograficzne i sozologiczne.
* **Geologia:** szczegółowe mapy geologiczne i osuwiskowe Państwowego Instytutu Geologicznego.
* **Serwisy powiatowe i miejskie:** bazy uzbrojenia terenu (GESUT) i obiekty topograficzne (BDOT500) z lokalnych portali mapowych.

---

➡️ **Kolejny krok:** [Pierwsze kroki w MapoTero →](../pierwsze-kroki/)
