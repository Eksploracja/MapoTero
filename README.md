# MapoTero

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/)

**MapoTero** to darmowy program służący do pobierania rastrowych map z internetu, opublikowanych za pośrednictwem serwerów WMS (takich jak Geoportal2, PIG, GDOŚ). 

Program pobiera mapy, dzieląc wybrany obszar na siatkę kwadratowych segmentów (do 2048×2048 px). Pobrane segmenty map można następnie:
* **scalić w jeden duży plik rastrowy** przy użyciu wbudowanej funkcji *"złącz pobrane segmenty w jeden arkusz"*,
* **wgrać do urządzeń GPS** i aplikacji turystycznych (np. [TrekBuddy](http://www.trekbuddy.net/forum/index.php), [OziExplorer](http://www.oziexplorer.com/)),
* **wyświetlić jako podkład georeferencyjny** w programach GIS i GPS: [QGIS](https://www.qgis.org/), [Google Earth](https://www.google.pl/intl/pl/earth/), ArcGIS, GPS Tuner, MapInfo.

* **Obsługiwane formaty rastrowe:** JPEG, TIFF, PNG, GIF.
* **Obsługiwane formaty georeferencji:** MAP, KML, TAB, JPGW, WLD, GMI.

---

## Pobieranie

Najnowsze wydania programu można pobrać z zakładki **[Releases](https://github.com/Eksploracja/MapoTero/releases)**:
* 📥 [Pobierz najnowszą wersję MapoTero z GitHub Releases](https://github.com/Eksploracja/MapoTero/releases)

---

## Aktualności

* **Wersja 3.12:**
  * Aktualizacja bibliotek zależnych (.NET Framework 4.7.2, GMap.NET, SQLite, Newtonsoft.Json),
  * Uporządkowanie struktury projektu i usunięcie przestarzałego kodu v2,
  * Poprawki konfiguracji kompilacji i wsparcia dla nowoczesnych środowisk Visual Studio.
* **19.10.2021 – Wersja 3.11:**
  * Naprawa niedziałającej warstwy Ortofotomapa,
  * Aktualizacja adresów warstw WMS.
* **20.10.2020 – Wersja 3.10:**
  * Usunięcie usterki związanej z niewyświetlaniem podkładowej warstwy OpenStreetMap w systemie Windows 10,
  * Aktualizacja warstw WMS,
  * Zmiana platformy programistycznej .NET Framework z wersji 4.5 na 4.7.2.
* **09.09.2019 – Wersja 3.0.0.9:**
  * Aktualizacja warstw WMS,
  * Zmiana platformy programistycznej .NET Framework z wersji 3.5 na 4.5,
  * Naprawa błędu wyświetlania podkładowej warstwy OpenStreetMap.

---

## Skrócona instrukcja obsługi

1. **Wybór zbioru map:** Wybierz z listy po prawej stronie okna programu zbiór map, z którego chcesz pobrać dane (lub pozostaw domyślny zbiór *"skany map topograficznych"*).
2. **Wybór warstwy:** Kliknij jednokrotnie lewym przyciskiem myszy na interesującą Cię warstwę (np. *"Raster_25_1965"* – mapa 1:25 000 w układzie 1965, lub *"Raster_10_1965"*). Nazwa zaznaczonej warstwy wyświetli się w polu *"Wybrane warstwy"*.
3. **Zaznaczenie obszaru:** Na podglądzie mapy zaznacz prawym przyciskiem myszy zasięg pobieranego obszaru.
4. **Pobieranie:** Kliknij przycisk **"Pobierz"**. Rozpocznie się pobieranie segmentów do katalogu `download`.
5. **Przeglądanie i scalanie:**
   * Kliknij żółty folder, aby otworzyć katalog z pobranymi plikami.
   * Opcjonalnie scal pobrane segmenty w jeden arkusz ikoną szachownicy (*"scal pobrane segmenty..."*). Wydajność zależy od ilości pamięci RAM (narzędzie zalecane do arkuszy do 10 000 × 10 000 px; dla większych panoram można użyć np. IrfanView).

![Uproszczona instrukcja obsługi](instrukcja.jpg)

---

## Wygląd okien programu

### Główne okno programu
![Główne okno programu](przod.jpg)

### Okno ustawień
![Okno ustawień](tyl.jpg)

---

## Obsługiwane serwery WMS

* **Geoportal2** (Główny Urząd Geodezji i Kartografii)
* **Państwowy Instytut Geologiczny (PIG)**
* **Generalna Dyrekcja Ochrony Środowiska (GDOŚ)**

---

## Źródła informacji i społeczność

* Dyskusja i wsparcie: dział *"Programy GPS/GIS wspierające eksplorację"* na [Pomorskim Forum Eksploracyjnym](https://forum.eksploracja.pl/viewforum.php?f=205)
* Fanpage Facebook: [facebook.com/mapotero.opensource](https://www.facebook.com/mapotero.opensource)

---

## Zespół projektu

* **Twórca programu:** Pajakt (2009–2014)
* **Rozwój projektu:** [Kazimierz Niecikowski](http://labgis.pl/) (od 2015)
* **Współpraca programistyczna:**
  * Paweł_gdn
  * AAA222 (moduł NoTo)
  * Edward Zadorski (kod modułu przeliczania współrzędnych)

---

## Alternatywne rozwiązania

* **Kafelkarz** (autor: AAA222) – program umożliwiający pobieranie danych zarówno z serwerów WMS, jak i WMTS.

---

## Licencja

Program jest wolnym oprogramowaniem udostępnianym na warunkach licencji **GNU General Public License v3 (GPLv3)**. Szczegóły licencji znajdują się w pliku [LICENSE](LICENSE).
