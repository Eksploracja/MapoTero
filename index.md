---
layout: default
title: Mapy z Geoportalu, LiDAR i mapy historyczne
description: "Darmowy program na Windows: pobieranie map z Geoportalu (WMS/WMTS), LiDAR i map historycznych. Gotowe mapy offline do MBTiles, Garmin, GIS i CAD."
nav_exclude: true
---

# MapoTero
{: .no_toc }

Darmowy, otwartoźródłowy program dla systemu Windows do pobierania map rastrowych z usług **WMS, WMTS oraz XYZ/TMS**. Przygotuj gotowe mapy offline na telefon, odbiornik Garmin, do programów GIS i projektów CAD.
{: .fs-6 .fw-300 }

[Pobierz MapoTero](https://github.com/Eksploracja/MapoTero/releases/latest){: .btn .btn-primary .mr-2 .mb-2 }
[Podręcznik użytkownika](pomoc/){: .btn .mb-2 }
{: .mb-2 }

Darmowy program na Windows (64-bit) · Bez instalacji (portable, pojedynczy plik .exe) · Licencja wolnego oprogramowania GPLv3
{: .fs-3 .text-grey-dk-000 .d-block .mb-4 }

![Główne okno programu MapoTero]({{ '/pomoc/obrazy/03-okno-glowne.png' | relative_url }})

---

## Co zrobisz w MapoTero?

| Zastosowanie | Formaty wyjściowe | Przykładowe aplikacje i urządzenia |
| --- | --- | --- |
| **Mapy offline w telefonie** | MBTiles (także wielowarstwowe i wieloskalowe), OGC GeoPackage (`.gpkg`), SQLiteDB, OruxMaps | Locus Map, OsmAnd, OruxMaps, QField, Mergin Maps, Avenza Maps |
| **Odbiorniki Garmin** | Garmin BirdsEye (`.jnx` bez limitu 100 kafli), Garmin Custom Maps (`.kmz` z podziałem na tomy i przezroczystością) | Urządzenia Garmin (bezpośredni zapis przez kabel USB) |
| **Praca w GIS i wydruki** | Kafelkowany GeoTIFF z piramidą pomniejszeń (COG), scalony JPEG / PNG oraz pojedyncze segmenty z georeferencją | QGIS, ArcGIS, Google Earth |
| **Podkłady do CAD** | Rysunek DXF (R2000) z obrysem, siatką i numeracją, skrypt SCR oraz program LISP (`.lsp`) | AutoCAD, AutoCAD LT, BricsCAD, ZWCAD |

---

## Najważniejsze możliwości

* **Szybki kreator map (`Ctrl+K`):** w 4 prostych krokach prowadzi od wyboru urządzenia i gotowego stylu (turystyka, góry, rower, cieniowanie LiDAR z zabytkami NID, mapy historyczne lub działki EGiB) do gotowego pliku offline.
* **Transfer na telefon przez Wi-Fi i kod QR:** bez kabla USB i bez chmury – wystarczy zeskanować kod QR aparatem telefonu w sieci lokalnej, aby pobrać mapę bezpośrednio na smartfon.
* **Własne kompozycje warstw:** łączenie warstw z różnych serwerów (WMS, WMTS i XYZ razem), regulacja krycia, tryby mieszania (w tym dedykowany tryb *7 Rzeźba terenu* dla cieniowania LiDAR) oraz podgląd efektu na żywo przed pobieraniem.
* **Pobieranie tylko potrzebnego obszaru:** siatka segmentów (do 2048×2048 px) z przesuwanymi uchwytami, korytarz wzdłuż trasy GPX/KML/KMZ, obrys z pliku `.gpkg` oraz wyszukiwarka miejscowości, adresów i działek ULDK (`Ctrl+F`).
* **Układy współrzędnych i konwertery:** precyzyjne przeliczenia w locie dla PL-1992, PL-2000 (strefy 5–8), UTM (33N–35N) i WGS84 oraz wbudowany konwerter starych map OziExplorer (`.map`) do formatów JNX, KMZ i MBTiles.

---

## Szybki start

Program działa w trybie **portable (bez instalacji)** – pobierz i rozpakuj archiwum ZIP, a następnie uruchom `MapoTero.exe` (katalog `warstwy` musi znajdować się obok pliku `.exe`).

1. **Wybierz obszar i styl:** naciśnij <kbd>Ctrl</kbd>+<kbd>K</kbd>, aby uruchomić [Szybki kreator mapy](pomoc/narzedzia/#szybki-kreator-mapy) i wybrać cel (smartfon, Garmin, wydruk, CAD).
2. **Pobierz dane:** program automatycznie pobierze segmenty map i przygotuje plik wynikowy.
3. **Prześlij mapę:** wyślij plik na telefon kodem QR przez Wi-Fi lub skopiuj na urządzenie Garmin przez USB.

Więcej informacji, tryb ręczny z pełną kontrolą warstw oraz szczegółowe poradniki znajdziesz w dokumentacji:

* 📖 **[Podręcznik użytkownika online](pomoc/):** 42 rozdziały instrukcji krok po kroku opisujące każde narzędzie i format.
* 🗺️ **[Galeria warstw i usług](pomoc/galeria-warstw/):** zestawienie polecanych serwisów (ortofotomapy, NMT/LiDAR, mapy archiwalne WIG i pruskie, zabytki NID, bazy EZiUDP).
* 💬 **[Społeczność i forum](https://forum.eksploracja.pl/viewforum.php?f=205):** dyskusje, pomoc i zgłaszanie propozycji na forum eksploracja.pl.
* 💻 **[Projekt na GitHubie](https://github.com/Eksploracja/MapoTero):** repozytorium kodu, historia wydań i zgłaszanie błędów (Issues).
