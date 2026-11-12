---
title: Nakładki i łączenie warstw
nav_order: 3
parent: Eksport mapy
---

# Nakładki i łączenie warstw
{: .no_toc }

Przezroczysta nakładka nad mapą aplikacji oraz łączenie kilku pobranych map w jedną, z regulacją krycia.
{: .fs-5 .fw-300 }

**Gdzie w programie:** okno eksportu, grupa *Warstwy i przezroczystość*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Nakładka

Opcja **nakładka (przezroczysta warstwa wyświetlana nad mapą podkładową aplikacji)** tworzy mapę,
która wyświetla się bezpośrednio na mapie w aplikacji: na przykład granice działek, obszary chronione albo mapa
historyczna nałożona na OpenStreetMap w Locus Map, OsmAnd czy OruxMaps.

* Kafle zapisywane są jako PNG z przezroczystością. W MBTiles mapa ma typ `overlay`.
* Opcja jest zaznaczona automatycznie, gdy sesja została pobrana w formacie PNG lub GIF z opcją *przezroczyste tło*.
* Niedostępna dla KMZ dla Garmin.

Żeby nakładka miała przezroczyste tło, mapa musi je mieć. Są dwa sposoby:

1. **Przy pobieraniu:** format segmentów PNG i opcja *przezroczyste tło* w oknie *Ustawienia* (serwer dostaje parametr
   `TRANSPARENT=TRUE`). Najlepsza jakość, ale nie każdy serwer to obsługuje.
2. **Przy eksporcie:** opcja **białe tło mapy z tego folderu przezroczyste** zamienia białe piksele na przezroczyste.
   Działa z każdym serwerem i z segmentami JPEG, ale może też „wyciąć” białe elementy treści mapy.

---

## Warstwy z innych folderów

Na eksportowaną mapę (mapę podstawową z folderu sesji) można nałożyć **mapy z innych sesji pobierania**.
Lista *Warstwy z innych folderów nakładane na mapę (od dołu do góry)*:

1. Kliknij **dodaj warstwę...** i wskaż folder innej sesji (z plikiem `conf.txt`).
2. Warstwa pojawi się na liście. Kolejne warstwy kładzione są wyżej.
3. Zaznacz warstwę na liście, żeby ustawić jej opcje:
   * **Krycie [%]**: od 5 do 100 (domyślnie 100). Wartość 100% zakrywa mapę pod spodem, a 50% daje półprzezroczystość.
   * **Białe tło zaznaczonej warstwy przezroczyste**: dla warstw bez przezroczystego tła (włączane automatycznie
     dla segmentów JPEG i PNG pobranych bez przezroczystości).
4. **usuń warstwę** usuwa zaznaczoną pozycję.

Warunki:

* warstwa musi pokrywać przynajmniej część obszaru mapy podstawowej (obszar eksportu wyznacza zawsze mapa podstawowa),
* może być pobrana **w innym układzie współrzędnych** i z innym pikselem (program przelicza ją przy eksporcie),
* nie może to być folder mapy podstawowej.

Po zmianie mapy podstawowej (*zmień folder*) program usuwa z listy warstwy, które jej nie pokrywają.

Najwyższy poziom powiększenia program dobiera do najdokładniejszej z warstw, a opis mapy w pliku wymienia
wszystkie warstwy i ich źródła.

![Sekcja Warstwy i przezroczystość w oknie eksportu]({{ '/pomoc/obrazy/10-eksport-nakladki.png' | relative_url }})

*Rysunek 1. Sekcja warstw i przezroczystości w oknie eksportu.*

---

## Przykłady

**Działki na ortofotomapie** (jedna mapa do telefonu):

1. Pobierz ortofotomapę (np. *Ortofotomapa standardowa 25 cm (kafle WMTS)*) do jednej sesji.
2. Pobierz działki (*Ewidencja, geodezja i uzbrojenie > Działki ewidencyjne (KIEG) > Działki*) do drugiej sesji, w PNG z przezroczystym tłem, ten sam obszar.
3. Otwórz eksport z folderem ortofotomapy, wybierz **dodaj warstwę...** i wskaż folder działek, krycie 100%.
4. Eksportuj do MBTiles.

**Mapa topograficzna półprzezroczysta na ortofotomapie:**

1. Mapa podstawowa: ortofotomapa (*Ortofotomapa standardowa 25 cm (kafle WMTS)*).
2. Dodaj warstwę z innego folderu: mapę topograficzną (np. *1:25 000, układ 1965*), ustaw krycie ok. `45–60%`
   i zaznacz *białe tło zaznaczonej warstwy przezroczyste*, żeby z mapy topo zostały na zdjęciu drogi, napisy i poziomice.

**Mapa historyczna nad mapą aplikacji:**

1. Pobierz mapę historyczną (np. *Messtischblätter 1:25 000* wybranego obszaru).
2. W eksporcie zaznacz **nakładka** i **białe tło mapy z tego folderu przezroczyste**.
3. W aplikacji włącz ją jako nakładkę nad OpenStreetMap. Przezroczystość nakładki zwykle reguluje się w aplikacji.

{: .informacja }
Kilka warstw z jednego albo z **różnych** serwerów (WMS, WMTS, XYZ/TMS) możesz też połączyć od razu **podczas pobierania**
w oknie głównym ([Wybrane warstwy i ich kolejność](../../okno-glowne/#zbiory-map-i-warstwy) oraz okno warstw zaawansowanych).
Łączenie w oknie eksportu przydaje się wtedy, gdy sesje były już pobrane osobno, powstały w różnych układach
współrzędnych albo wymagają usunięcia białego tła z gotowych segmentów JPEG.
Mapę, która przy różnych przybliżeniach pokazuje różne warstwy, tworzy
[kreator wielowarstwowego MBTiles](../../mbtiles-wielowarstwowy/).

---

➡️ **Kolejny krok:** [Mapa w telefonie i odbiorniku GPS →](../aplikacje/)

*Zobacz też: [Wybór formatu](../formaty/) · [Ustawienia: przezroczyste tło](../../ustawienia/#sesja-pobierania) · [Nakładanie dwóch map PNG](../../narzedzia/)*
