---
title: Wielowarstwowy MBTiles
nav_order: 13
---

# Wielowarstwowy MBTiles
{: .no_toc }

Kompilacja kilku pobranych sesji do jednego pliku MBTiles: każda warstwa może być widoczna w swoim zakresie
powiększeń albo mieszać się z warstwami leżącymi pod nią.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Zaawansowany wielowarstwowy MBTiles...* lub przycisk *Wielowarstwowy...* w oknie eksportu  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Do czego służy

Zamiast wozić w telefonie osobne pliki map dla różnych skal, tworzysz **jedną bazę `.mbtiles`**, która automatycznie
przełącza mapę wraz z przybliżaniem (np. w Locus Map, OruxMaps, OsmAnd, Guru Maps na iPhone czy QFieldzie):

* z daleka (zoom `11–13`): mapa przeglądowa lub topograficzna `1:100 000`,
* przy zbliżeniu (zoom `14–16`): szczegółowa mapa topograficzna `1:25 000` lub `1:10 000`,
* z bliska (zoom `17–19`): ortofotomapa o wysokiej rozdzielczości (opcjonalnie z nałożoną półprzezroczystą mapą topo
  lub granicami działek w trybie *Mnożenie*).

Kreator łączy wyłącznie **wcześniej pobrane sesje** (foldery z `conf.txt`), bez pobierania nowych danych z sieci.

![Zaawansowany kreator wielowarstwowego / wieloskalowego MBTiles]({{ '/pomoc/obrazy/11-mbtiles-wielowarstwowy.png' | relative_url }})

*Rysunek 1. Kreator wielowarstwowego i wieloskalowego pliku MBTiles.*

---

## Lista warstw i zakresy powiększeń

1. Kliknij **Dodaj warstwę...** i wskaż folder pobranej sesji (możesz też wskazać nadrzędny folder `download`,
   z którego program sam wykryje wszystkie podfoldery sesji, albo przeciągnąć foldery myszą wprost do listy).
2. Program odczyta `conf.txt` każdej warstwy i automatycznie podpowie zakres **Zoom Od** do **Zoom Do**.

{: .uwaga }
Kolejność w tym kreatorze działa jak w programach GIS (QGIS, Photoshop): **góra tabeli = wierzch mapy**, a **dół tabeli = podkład**.
Przycisk **W górę** wynosi warstwę na wierzch stosu, a **W dół** spycha ją niżej.

| Kolumna | Znaczenie |
|---|---|
| *Wł.* | włącza lub tymczasowo wyłącza warstwę bez usuwania jej z projektu |
| *Tytuł / Mapa* | nazwa warstwy (z `conf.txt`, można ją edytować) |
| *Zoom Od* / *Zoom Do* | zakres powiększeń (`0–22`), w którym warstwa jest rysowana |
| *Krycie %* | przezroczystość warstwy (`0–100%`) |
| *Mieszanie* | tryb łączenia z warstwą pod spodem (tabela poniżej) |
| *Szarość* | konwersja warstwy do odcieni szarości (świetne np. do stonowania ortofotomapy pod kolorowe szlaki) |
| *Białe przezr.* | zamienia białe tło warstwy (np. skanów papierowych lub JPEG) na przezroczyste |

### Tryby mieszania

| Tryb | Działanie i kiedy użyć |
|---|---|
| *1 Zwykły* | standardowe nakładanie z zadanym kryciem % |
| *2 Mnożenie* | białe tło staje się niewidoczne, a czarne napisy, poziomice i drogi zachowują pełny kontrast na tle ortofotomapy (bez „mlecznej mgły”) |
| *3 Ciemniej* | z obu warstw wybiera ciemniejszy odcień piksela |
| *4 Jaśniej* | wybiera jaśniejszy kolor (np. jasne linie na ciemnym podkładzie) |
| *5 Ekran* | rozjaśnia podkład, czerń znika |
| *6 Nakładka* | podbija kontrast (szczególnie przydatne do cieniowania rzeźby NMT nad mapą lub ortofotomapą) |

---

## Podgląd poziomu powiększenia

Po prawej stronie okna znajduje się panel **Symulacja i podgląd zoomu**:

* przesuwaj suwak **Poziom zoom** (`6–20`), aby na żywo obejrzeć kafel `256 × 256 px` dokładnie tak, jak powstanie w pliku wynikowym
  (z uwzględnieniem kolejności, zakresów zoom, mieszania, szarości i przezroczystości),
* przycisk **Pobierz środek z mapy głównej** przenosi punkt podglądu w miejsce widoczne aktualnie w oknie głównym MapoTero.

---

## Parametry pliku

* **Plik wynikowy** (domyślnie `mapa_wielowarstwowa_RRRRMMDD_GGMM.mbtiles` w `download\`) i przycisk **Pokaż...** otwierający plik w Eksploratorze.
* **Nazwa mapy**: nazwa widoczna na liście map w aplikacji mobilnej.
* **Format kafli:**
  * *Smart Hybrid (JPG + PNG przezroczyste)* *(zalecany)*: kafle pełne zapisuje jako lekki JPEG, a kafle z przezroczystością lub na brzegach jako PNG.
  * *Tylko JPEG*: najmniejszy plik, ale z białym tłem w miejscach bez danych.
  * *Tylko PNG*: bezstratny zapis z pełną przezroczystością i znacznie większym rozmiarem.
* **Jakość JPEG (%)**: od `10` do `100` (domyślnie `85`).
* **Szybka piramida w pasmach warstw** *(domyślnie włączone)*: przyspiesza zapis nawet o ok. 70%, generując mniejsze poziomy
  przez szybkie dwukrotne zmniejszanie tam, gdzie aktywna jest ta sama warstwa.
* **Wypełnij brakujące poziomy powiększeń przeskalowaną warstwą niższą**: zapobiega „dziurom” w zoomie, jeśli między zakresami warstw została luka.

---

## Szacowanie rozmiaru

Przed kliknięciem **Kompiluj MBTiles** użyj przycisku **Oszacuj rozmiar i liczbę kafli**. Program obliczy przewidywaną
wagę każdego poziomu zoom oraz sprawdzi zgodność z limitem **4 GB** (maksymalny rozmiar pojedynczego pliku na kartach pamięci
sformatowanych w systemie plików FAT32 w starszych telefonach i odbiornikach GPS).

W oknie dostępna jest też wbudowana pomoc pod przyciskiem **Pomoc (?)** (klawisz <kbd>F1</kbd>).

---

## Zgodność z aplikacjami

Wygenerowany plik `.mbtiles` wystarczy skopiować do telefonu:

* **Locus Map:** `Locus/maps/` albo otworzyć plik w aplikacji (*Wczytaj mapę zewnętrzną*),
* **OruxMaps:** `oruxmaps/mapfiles/`,
* **OsmAnd:** otworzyć plik (import mapy rastrowej).

{: .wskazowka }
Aby uzyskać bardzo czytelną mapę topograficzną na tle ortofotomapy, połóż mapę topo na górze tabeli,
ustaw tryb **2 Mnożenie**, zaznacz **Białe przezr.** i ustaw krycie na `75–85%`.

---

➡️ **Kolejny krok:** [Eksport mapy do programów CAD →](../cad/)

*Zobacz też: [Eksport mapy](../eksport/) · [Mapa w telefonie i odbiorniku GPS](../eksport/aplikacje/) · [Nakładki i łączenie warstw](../eksport/nakladki/)*
