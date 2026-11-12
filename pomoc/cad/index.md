---
title: Eksport do CAD
nav_order: 14
has_children: true
---

# Eksport do CAD
{: .no_toc }

Mapa jako podkład w rysunku CAD: rysunek DXF, skrypt SCR albo program LISP, z rastrami umieszczonymi na właściwych współrzędnych.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Eksport do CAD (DXF, skrypt SCR, LISP)...*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najkrótsza droga

1. Pobierz mapę w układzie **PL-2000** (strefa 5–8 dopasowana do obszaru) albo **PL-1992**, aby jednostką rysunku był **metr**.
2. Wybierz menu *Narzędzia > Eksport do CAD (DXF, skrypt SCR, LISP)...*.
3. Zostaw wybrane *pobrane segmenty* (albo użyj przycisku **scal teraz...** przy dużej liczbie segmentów).
4. Zaznacz pliki wynikowe (`rysunek DXF`, `skrypt SCR`, opcjonalnie `program LISP`) i kliknij **Eksportuj**.
5. Otwórz utworzony plik `_cad_<kol>x<wier>.dxf` bezpośrednio w programie CAD albo wstaw mapę do istniejącego rysunku poleceniem `SCRIPT` / `APPLOAD` ([Wstawianie w programach CAD](programy/)).

![Okno Eksport do CAD (DXF, skrypt SCR, LISP)]({{ '/pomoc/obrazy/12-eksport-cad.png' | relative_url }})

*Rysunek 1. Okno eksportu do CAD: wybór rastrów, elementów wektorowych i formatów.*

---

## Rastry

* **Pobrane segmenty (osobny obraz dla każdego segmentu)**: każdy plik segmentu trafia do rysunku jako osobna encja rastra.
  Limit w jednym rysunku to **10 000 rastrów** (powyżej 1000 program CAD może działać wolno, o czym program ostrzega komunikatem).
* **Scalony arkusz**: wstawia jeden plik arkusza utworzony przez *Łączenie segmentów* (GeoTIFF albo JPEG/PNG z plikiem world file).
  Przycisk **wybierz...** wskazuje istniejący arkusz, a **scal teraz...** uruchamia od razu okno scalania dla bieżącej sesji
  i automatycznie wybiera powstały arkusz.
* **Bez rastrów: tylko obrys i siatka segmentów**: przydatne do wstawienia samego skorowidza przed pobraniem mapy.

---

## Elementy wektorowe

Wszystkie elementy trafiają na osobne warstwy o nazwach zaczynających się od `MAPOTERO_`. Można je jednym kliknięciem wyłączać lub ukrywać:

| Warstwa | Zawartość | Uwagi |
|---|---|---|
| `MAPOTERO_RASTER` | wstawione obrazy (segmenty lub scalony arkusz) | w DXF ramki rastrów są ukryte, a do zaznaczania ich kliknięciem służy zmienna `IMAGEFRAME` (wartość `1` lub `2`) |
| `MAPOTERO_ZASIEG` | zewnętrzny obrys siatki (pełne segmenty) | kolor czerwony |
| `MAPOTERO_ZAZNACZENIE` | rzeczywisty prostokąt zaznaczony na mapie | rysowany na zielono, jeśli jest mniejszy od siatki pełnych segmentów |
| `MAPOTERO_SIATKA` | linie lub prostokąty siatki poszczególnych segmentów | przy korytarzu ponad 50 000 segmentów rysowana jest pełna siatka liniami |
| `MAPOTERO_NUMERY` | opisy/numery segmentów (`DXF`, `LISP`) | limit do **20 000** numerów w jednym rysunku |
| `MAPOTERO_TRASA` | linia trasy GPX / KML | aktywne tylko dla sesji pobranej wzdłuż trasy |
| `MAPOTERO_OPIS` | tekst opisu mapy pod obrysem (`DXF`, `LISP`) | zawiera nazwę mapy, źródło danych, datę pobrania i układ współrzędnych |

---

## Pliki wynikowe

Domyślnie pliki zapisują się w folderze sesji pod nazwą `_cad_<kol>x<wier>` (możesz ją zmienić przyciskiem **zmień...**).
Ustawienia pól wyboru są zapamiętywane między uruchomieniami w `lastsettings.txt`.

* **Rysunek DXF (DXF R2000)**: otwiera się w AutoCAD 2000+, AutoCAD LT, BricsCAD, ZWCAD, GstarCAD, progeCAD i innych.
  Opcja *ścieżki rastrów względem pliku DXF* zapisuje odnośniki w postaci `.\nazwa_pliku`, co pozwala przenieść cały folder na inny komputer.
* **Skrypt SCR**: do uruchomienia w otwartym rysunku poleceniem `SCRIPT` (działa również w starszym AutoCAD LT bez obsługi LISP).
* **Program LISP (.lsp)**: wczytywany poleceniem `APPLOAD`. Wstawia mapę pod istniejące obiekty, dodaje numery i opis,
  przywraca bieżącą warstwę oraz zmienne `INSUNITS` i `FILEDIA`, nie zmienia UCS i pomija brakujące pliki. Ponowne wywołanie w tym samym rysunku wykonuje polecenie `MAPOTERO_WSTAW`.

{: .uwaga }
Skrypt `.scr` i program `.lsp` zapisują ścieżki plików w kodowaniu systemowym Windows (ANSI). Jeśli nazwa folderu sesji
zawiera znaki spoza polskiej strony kodowej, program ostrzeże o tym. Warto wtedy przenieść mapę do prostszej ścieżki albo użyć pliku DXF.

---

## Osie i jednostki rysunku

![Konwencja osi X i Y w MapoTero w porównaniu z rysunkiem CAD]({{ '/pomoc/obrazy/12-osie-cad.svg' | relative_url }})

*Rysunek 2. Konwencja osi w MapoTero i w rysunku CAD: program automatycznie zamienia kolejność.*

W rysunku CAD obowiązuje układ matematyczny: **oś X skierowana jest na wschód**, a **oś Y na północ**, czyli odwrotnie niż w polach XY okna głównego MapoTero. Moduł eksportu przelicza kolejność automatycznie, więc punkt `(X, Y)` z programu trafia na współrzędne `(Y, X)` w rysunku CAD.

* Dla układów **PL-2000** i **PL-1992** jednostką rysunku jest **1 metr**.
* Dla **WGS84** jednostką są **stopnie**. Do prac projektowych w CAD unikaj WGS84 (program wyświetli ostrzeżenie).

---

➡️ **Kolejny krok:** [Wstawianie mapy w programach CAD →](programy/)

*Zobacz też: [Scalanie segmentów](../scalanie/) · [Układy współrzędnych](../pojecia/#układy-współrzędnych)*
