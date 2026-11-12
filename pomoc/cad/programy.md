---
title: Wstawianie w programach CAD
nav_order: 1
parent: Eksport do CAD
---

# Wstawianie w programach CAD
{: .no_toc }

Krok po kroku: jak wstawić wyeksportowaną mapę jako podkład do konkretnego programu CAD.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Przed wstawieniem: kolejność warstw i jednostki

* Pracuj w rysunku o jednostkach ustawionych na **metry** (`INSUNITS` = `6` lub bez jednostki `0`, jeśli współrzędne geodezyjne masz już w metrach).
* Najszybszy sposób przeniesienia obiektów z otwartego pliku `_cad_*.dxf` do własnego rysunku projektowego to **Kopiuj** (<kbd>Ctrl</kbd>+<kbd>C</kbd>) i w docelowym rysunku polecenie **`PASTEORIG`** (*Wklej do oryginalnych współrzędnych*).
* Jeśli nie możesz zaznaczyć wstawionych rastrów myszą, wpisz w linii poleceń **`IMAGEFRAME`** i ustaw wartość **`1`** (ramki widoczne i drukowane) albo **`2`** (ramki widoczne tylko na ekranie, niedrukowane).

---

## AutoCAD

* **Plik LISP (zalecane):** otwórz swój rysunek, wpisz polecenie **`APPLOAD`**, wskaż plik `.lsp` (w razie pytania o zaufanie wybierz *Zawsze wczytuj* / *Wczytaj raz*). Mapa wstawi się od razu pod istniejące obiekty i ustawienia rysunku zostaną przywrócone.
* **Plik SCR:** wpisz **`SCRIPT`** i wskaż plik `.scr` (uwaga: skrypt przestawia rysunek na warstwę `MAPOTERO_...`, więc po wykonaniu przywróć swoją warstwę bieżącą).
* **Plik DXF:** otwórz plik `.dxf` i przenieś mapę przez `PASTEORIG` lub podepnij jako odnośnik zewnętrzny (`XREF`).

---

## AutoCAD LT

* **Starsze wersje (do 2023):** AutoCAD LT nie obsługiwał LISP-a. Użyj polecenia **`SCRIPT`** i wskaż plik `.scr` albo otwórz plik `.dxf` i wklej przez **`PASTEORIG`**.
* **AutoCAD LT 2024 i nowsze:** obsługuje AutoLISP, więc możesz wczytać plik `.lsp` poleceniem **`APPLOAD`** dokładnie tak jak w pełnym AutoCAD.

---

## BricsCAD

Wszystkie trzy metody działają bezpośrednio:

* **`APPLOAD`** dla pliku `.lsp` (automatyczne wstawienie oraz ponowne uruchomienie poleceniem `MAPOTERO_WSTAW`),
* **`SCRIPT`** dla pliku `.scr`,
* bezpośrednie otwarcie `.dxf` ze ścieżkami względnymi.

---

## ZWCAD

W ZWCAD (oraz GstarCAD / progeCAD):

* wczytaj `.lsp` przez **`APPLOAD`** (albo przeciągnij plik `.lsp` w obszar rysunku) lub uruchom `.scr` poleceniem **`SCRIPT`**,
* jeśli rastry zasłaniają istniejący rysunek, zaznacz warstwę `MAPOTERO_RASTER` i użyj **`DRAWORDER`** → *Na spód* (*Send to Back*). Plik LISP robi to automatycznie.

---

## Setki segmentów

Gdy obszar jest duży i w folderze znajduje się ponad kilkaset segmentów (program ostrzega powyżej **1000 rastrów**, a twardy limit eksportu wynosi **10 000**):

1. W oknie *Eksport do CAD* w grupie *Rastry wstawiane do rysunku* kliknij przycisk **scal teraz...**.
2. W otwartym oknie scalania wybierz format **GeoTIFF (bezstratny)** albo **JPEG** (pole world file zaznaczy się samo) i kliknij **Złącz segmenty**.
3. Po powrocie do okna eksportu CAD program automatycznie wybierze opcję **scalony arkusz** ze wskazaniem powstałego pliku.

Zamiast setek lub tysięcy osobnych plików program CAD ładuje wtedy **jeden szybki obraz**. Rysunek otwiera się sprawnie i nie zacina przy przesuwaniu.

---

➡️ **Kolejny krok:** [Dodawanie własnych serwerów map →](../../serwery/)

*Zobacz też: [Eksport do CAD](../) · [Scalanie segmentów](../../scalanie/) · [Układy współrzędnych](../../pojecia/#układy-współrzędnych)*
