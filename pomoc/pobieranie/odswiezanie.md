---
title: Odświeżanie mapy
nav_order: 2
parent: Pobieranie map
---

# Odświeżanie mapy
{: .no_toc }

Ponowne pobranie wszystkich segmentów sesji, np. gdy serwer udostępnił nowszą ortofotomapę albo zaktualizowane dane.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Sesja pobierania > odśwież mapę (pobierz ponownie wszystkie segmenty)*  
**Dotyczy wersji:** {{ site.wersja_programu }}

## Kiedy odświeżać

* Serwer opublikował nowszą wersję mapy (nowa ortofotomapa, aktualizacja działek, nowa edycja mapy tematycznej).
* Część segmentów jest uszkodzona albo pochodzi z czasu awarii serwera, ale nie wiadomo które.

Do zwykłego dokończenia przerwanego pobierania odświeżanie nie jest potrzebne. Wystarczy kliknąć *Pobierz*
([Wznawianie](../przebieg/#wznawianie)).

---

## Jak odświeżyć

1. Wczytaj sesję, którą chcesz odświeżyć (*Sesja pobierania > wczytaj ustawienia sesji (conf.txt)*), jeśli nie jest
   bieżącą sesją.
2. Wybierz *Sesja pobierania > odśwież mapę (pobierz ponownie wszystkie segmenty)*.
3. Potwierdź pytanie *Pobrać ponownie wszystkie segmenty mapy do folderu…?*

Program pobiera wszystkie segmenty sesji od nowa, także te, które już są w folderze. Przy obszarze wzdłuż trasy
odświeża tylko segmenty korytarza.

---

## Bezpieczeństwo starych segmentów

Stary plik segmentu jest zastępowany **dopiero po udanym pobraniu nowego**. Jeśli serwer nie odpowie albo
pobieranie zostanie przerwane, w folderze zostają stare segmenty, a mapa nadal jest kompletna.
Segmenty, których nie udało się odświeżyć, trafiają do wykazu w `error.txt`.

Przy usługach kafelkowych (WMTS / XYZ / TMS) kafle zachowane z poprzedniego pobierania (folder `_kafle_wmts`) są przed odświeżeniem usuwane,
bo mogłyby być nieaktualne.

---

## Data pobrania w opisie mapy

Odświeżenie zapisuje w `conf.txt` nową datę pobrania. Data trafia potem do opisu mapy w eksporcie
(nazwa mapy, opis i atrybucja w MBTiles, opis w KMZ) i do opisu w rysunku CAD. Po odświeżeniu warto więc
ponownie wyeksportować mapę.

---

➡️ **Kolejny krok:** [Usługi kafelkowe (WMTS, XYZ, TMS) →](../wmts/)

*Zobacz też: [Przebieg pobierania](../przebieg/) · [Sesje pobierania](../../sesje/)*
