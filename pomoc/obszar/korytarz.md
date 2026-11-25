---
title: Obszar wzdłuż trasy
nav_order: 3
parent: Wybór obszaru
description: "Zamiast całego prostokąta program pobiera tylko segmenty leżące w pasie wzdłuż trasy. Przy trasie ukośnej lub krętej to wielokrotnie mniej danych."
---

# Obszar wzdłuż trasy
{: .no_toc }

Zamiast całego prostokąta program pobiera tylko segmenty leżące w pasie wzdłuż trasy. Przy trasie ukośnej lub
krętej to wielokrotnie mniej danych.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Sesja pobierania > Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)...*, odnośnik **Zmień ▾** w karcie *Obszar pobierania* pod mapą albo przeciągnięcie pliku trasy na mapę (Drag & Drop)  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najkrótsza droga

1. Ustaw zbiór map, warstwę, bok segmentu i piksel.
2. Wybierz *Sesja pobierania > Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)...* albo przeciągnij plik trasy z Eksploratora Windows na okno programu lub mapę (Drag &amp; Drop).
3. Kliknij **Wybierz...** i wskaż plik trasy (jeśli nie przeciągnięto go myszą).
4. Ustaw *Szerokość pasa po każdej stronie trasy [m]* i kliknij **OK**.
5. Sprawdź na mapie przebieg trasy (czerwona linia) oraz siatkę segmentów (kolor niebieski). W oknie zobaczysz liczbę wyznaczonych segmentów i oszczędność danych.
6. Kliknij **Pobierz segmenty**.

---

## Przygotowanie trasy

Program odczytuje następujące formaty:

| Plik | Co jest brane jako trasa |
|---|---|
| **GPX** | ślady (*track*) i trasy (*route*). Gdy plik zawiera tylko punkty (*waypoint*), program łączy je kolejno linią |
| **KML** | linie (również w grupach), ślady `gx:Track`. Obrys wielokąta traktowany jest jak linia |
| **KMZ** | pierwszy dokument KML znaleziony w archiwum |
| **GeoPackage (.gpkg)** | warstwy wektorowe zawierające linie (`LineString`, `MultiLineString`), np. trasy lub warianty dróg wyrysowane w QGIS (gdy plik ma kilka warstw, program pyta o wybór) |

Takie pliki generują serwisy i aplikacje turystyczne (np. Mapy.com, Komoot, Locus Map, OsmAnd i Google Earth). Plik może zawierać wiele odcinków. Program uwzględni je wszystkie.

---

## Okno Obszar pobierania wzdłuż trasy

![Okno Obszar pobierania wzdłuż trasy i wyznaczone segmenty na mapie]({{ '/pomoc/obrazy/04-korytarz.png' | relative_url }}){: loading="lazy" width="1090" height="504"}

*Rysunek 1. Okno wyboru trasy i szerokości pasa z niebieskimi segmentami wyznaczonymi wzdłuż trasy.*

| Pole | Opis |
|---|---|
| *Plik trasy (GPX, KML, KMZ, GPKG)* | przycisk **Wybierz...** wskazuje plik |
| *Szerokość pasa po każdej stronie trasy [m]* | od 10 do 50 000 m (domyślnie 1000 m). Pobrane zostaną segmenty, które choć częściowo leżą w zadanej odległości od trasy |

Pas mierzony jest symetrycznie od osi trasy, więc szerokość 1000 m daje korytarz o łącznej rozpiętości 2 km.
Ponieważ pobierane są pełne kwadratowe segmenty, rzeczywisty zasięg danych jest nieco szerszy.
W lewym dolnym rogu okna przycisk **Pomoc** (oraz klawisz <kbd>F1</kbd>) wyświetla podręczny opis doboru szerokości pasa i formatów trasy.

{: .wskazowka }
Przy wąskim korytarzu najlepiej sprawdza się mniejszy bok segmentu (np. 500–1000 px). Do pobrania trafi wtedy mniej zbędnego terenu poza pasem. Liczba pojedynczych plików wzrośnie, ale łączna objętość danych wyraźnie spadnie.

---

## Trasa i segmenty na mapie

Po kliknięciu **OK** program:

* ustawia zasięg pobierania na prostokąt obejmujący cały pas trasy (pola XY),
* rysuje trasę czerwoną linią, a segmenty do pobrania zaznacza na niebiesko,
* centruje i przybliża widok mapy do pełnej trasy,
* podaje w oknie długość trasy oraz liczbę segmentów (np. na powyższym zrzucie przy segmencie `1000 px`, pikselu `1 m` i pasie `1000 m` wyznaczono `31` z `36` segmentów, czyli `14%` mniej danych, a dla dłuższych lub ukośnych tras oszczędność przekracza `70%`).

Narożniki prostokąta są oznaczone pinezkami. Uchwytów nie ma, ponieważ zasięg wynika bezpośrednio z geometrii trasy. Zmiana boku segmentu lub piksela automatycznie przelicza segmenty korytarza.

Gdy trasa nie przecina obszaru pobierania (np. po ręcznej edycji współrzędnych XY), program nie rozpocznie pobierania i poprosi o ponowny wybór trasy lub wyczyszczenie korytarza.

---

## Plik korytarz.txt

Korytarz jest integralną częścią sesji. Program zapisuje go w folderze sesji jako `korytarz.txt` (obok `conf.txt`) przy starcie pobierania oraz przy ręcznym zapisie sesji. Po [wczytaniu sesji](../../sesje/) trasa i segmenty wracają na mapę, a wznawianie pobierania dotyczy wyłącznie segmentów korytarza.

Narzędzia korzystające z sesji uwzględniają korytarz:

* [Scalanie segmentów](../../scalanie/): poza korytarzem arkusz ma puste (białe) pola.
* [Eksport mapy](../../eksport/): poza korytarzem kafle są pomijane (przezroczyste w formatach PNG).
* [Eksport do CAD](../../cad/): linia trasy może trafić do rysunku jako osobna warstwa wektorowa `MAPOTERO_TRASA`.

---

## Powrót do pobierania prostokąta

* Menu *Sesja pobierania > Usuń obszar wzdłuż trasy (pobieranie całego prostokąta)*: przywraca standardowy prostokąt z siatką i uchwytami.
* Zaznaczenie nowego obszaru prawym przyciskiem myszy również usuwa korytarz trasy.

---

## Profil wysokościowy trasy

Profil wysokościowy z NMT (wysokości terenu dla punktów trasy, wykres i zapis GPX z wysokościami) jest od wersji 3.19
osobnym narzędziem dostępnym w menu *Narzędzia > Profil wysokościowy trasy (GPX, KML)...*. Szczegóły znajdziesz w rozdziale [Narzędzia dodatkowe](../../narzedzia/#profil-wysokościowy-trasy).

---

➡️ **Kolejny krok:** [Jak ocenić wielkość pobieranego obszaru →](../wielkosc/)

*Zobacz też: [Zaznaczanie na mapie](../zaznaczanie/) · [Sesje pobierania](../../sesje/)*
