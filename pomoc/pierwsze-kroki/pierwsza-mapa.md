---
title: Pierwsza mapa krok po kroku
nav_order: 3
parent: Pierwsze kroki
---

# Pierwsza mapa krok po kroku
{: .no_toc }

Praktyczny samouczek: pobierzesz fragment mapy topograficznej 1:25 000, a następnie przygotujesz z niego plik dla telefonu oraz arkusz GeoTIFF dla programu GIS.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }} · **Czas wykonania:** ok. 10 minut

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

---

## Przed rozpoczęciem

Upewnij się, że masz przygotowane:
* uruchomiony program MapoTero i aktywne połączenie z internetem,
* domyślne ustawienia programu (nie zmieniaj jeszcze żadnych opcji w oknie *Ustawienia*).

W tym samouczku pobierzemy obszar o wymiarach około 6 × 6 km, złożony z siatki 2 × 2 segmenty.

---

## Krok 1: Wybór mapy i warstwy

1. W prawym panelu, w drzewie **Wybierz mapy**, rozwiń grupę:  
   **Mapy topograficzne** > **Skany map topograficznych GUGiK**.
2. Zaznacz pole wyboru przy pozycji **1:25 000, układ 1965** (nazwa techniczna na serwerze: *Raster_25_1965*).
3. Wybrana warstwa pojawi się na liście *Wybrane warstwy oraz ich kolejność* z numerem `1)`. W polu *Piksel [m/pix]* program automatycznie podpowie optymalną wartość: **2**.

{: .wskazowka }
Jeśli przez pomyłkę zaznaczysz dodatkową warstwę, odznacz ją w drzewie albo zaznacz na liście *Wybrane warstwy* i kliknij przycisk **✕** (lub naciśnij klawisz <kbd>Delete</kbd>).

---

## Krok 2: Ustalenie parametrów siatki

Na górnym pasku pozostaw wartości domyślne:
* **Segment [pix]:** `2000` (jeden segment to plik o boku 2000 pikseli),
* **Piksel [m/pix]:** `2` (jeden piksel w obrazie odpowiada 2 metrom w terenie, czyli pojedynczy segment pokrywa 4 × 4 km),
* **Układ współrzędnych:** `PL-1992`.

---

## Krok 3: Zaznaczenie obszaru na podglądzie

1. Odszukaj interesujący Cię teren na mapie podglądu (przesuwaj widok lewym przyciskiem myszy, przybliżaj kółkiem).
2. Wciśnij **prawy przycisk myszy** w jednym narożniku planowanego obszaru, przeciągnij prostokąt i puść przycisk.
3. Na mapie pojawi się niebieska siatka z uchwytami w narożnikach i na bokach. W polu *Wynikowy rozmiar siatki* zobaczysz wymiar `2 x 2`.

![Okno główne po wybraniu warstwy i zaznaczeniu obszaru]({{ '/pomoc/obrazy/01-pierwsza-mapa-wybor.png' | relative_url }})

*Rysunek 1. Okno główne po wybraniu warstwy i wyznaczeniu siatki 2 × 2 segmenty.*

{: .informacja }
Siatka zaokrągla zasięg do pełnych segmentów, więc może być nieco większa od narysowanego prostokąta. Jej granice możesz precyzyjnie dopasować, przeciągając niebieskie uchwyty lewym przyciskiem myszy.

![Szczegół siatki segmentów z widocznymi uchwytami regulacji]({{ '/pomoc/obrazy/01-pierwsza-mapa-siatka.png' | relative_url }})

*Rysunek 2. Niebieskie uchwyty pozwalają przesuwać krawędzie i narożniki siatki.*

---

## Krok 4: Pobranie mapy

1. Kliknij zielony przycisk **Pobierz** na górnym pasku.
2. Na dolnym pasku stanu obserwuj postęp pobierania kolejnych segmentów.
3. Po kilkunastu sekundach w polu *Komunikaty* pojawi się zielona informacja o zakończeniu pobierania.
4. Kliknij żółtą ikonę folderu obok przycisku pobierania. W otwartym folderze sesji (`download\`) zobaczysz cztery pliki segmentów `_01_01.jpg` do `_02_02.jpg`, plik parametrów sesji `conf.txt` oraz wykaz współrzędnych `koordynaty.txt`.

{: .uwaga }
Gdy komunikat wyświetli się na czerwono (*Nie udało się ściągnąć wszystkich segmentów…*), przyczyny znajdziesz w pliku `error.txt`. Zwykle wystarczy odczekać chwilę i kliknąć **Pobierz** jeszcze raz: program uzupełni tylko brakujące segmenty.

---

## Krok 5: Przygotowanie pliku wynikowego

Pobrane segmenty możesz od razu przygotować do użycia w wybranej aplikacji:

### Wariant A: Mapa do telefonu lub nawigacji GPS (MBTiles)
1. Wybierz menu *Narzędzia > Eksport do KMZ, MBTiles, SQLiteDB, OruxMaps* (<kbd>Ctrl</kbd>+<kbd>E</kbd>).
2. Program otworzy okno z automatycznie wczytaną sesją.
3. W sekcji formatów zaznacz **MBTiles** (dla Locus Map, OsmAnd lub OruxMaps).
4. Kliknij **Eksportuj**. W folderze sesji powstanie gotowy plik `_eksport_2x2.mbtiles`.

### Wariant B: Scalony arkusz do programu GIS (GeoTIFF)
1. Wybierz menu *Narzędzia > Łączenie segmentów* (<kbd>Ctrl</kbd>+<kbd>L</kbd>).
2. Jako format wybierz **GeoTIFF (bezstratny)** i kliknij **Złącz segmenty**.
3. Powstanie plik `_scalone_segmenty_2x2.tif` (jeden arkusz 4000 × 4000 pikseli z georeferencją w układzie PL-1992), który możesz przeciągnąć wprost do okna programu QGIS lub ArcGIS.

---

## Co dalej

| Jeśli chcesz... | Przejdź do rozdziału |
|---|---|
| dowiedzieć się, gdzie bezpiecznie przechowywać pobrane sesje | [Gdzie program zapisuje dane](../dane-programu/) |
| poznać zasady doboru rozdzielczości i piksela | [Pojęcia podstawowe](../../pojecia/) |
| poznać wszystkie przyciski i pola okna głównego | [Okno główne](../../okno-glowne/) |
| pobrać mapę wzdłuż trasy wycieczki GPX | [Obszar wzdłuż trasy](../../obszar/korytarz/) |
| przenieść plik do aplikacji w telefonie | [Mapa w telefonie i odbiorniku GPS](../../eksport/aplikacje/) |
| wstawić podkład do rysunku CAD | [Eksport do CAD](../../cad/) |

---

➡️ **Kolejny krok w przewodniku:** [Gdzie program zapisuje dane (dane programu) →](../dane-programu/)
