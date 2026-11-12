---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Nakładanie dwóch pobranych map PNG, usuwanie pustych segmentów oraz tworzenie paczek TrekBuddy (.tar).
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia* oraz menu *Ustawienia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Nakładanie dwóch map PNG

**Gdzie:** menu *Narzędzia > Nakładanie dwóch map png na siebie* (<kbd>Ctrl</kbd>+<kbd>N</kbd>).

![Okno Nakładanie dwóch map na siebie]({{ '/pomoc/obrazy/14-nakladanie-map.png' | relative_url }})

*Rysunek 1. Okno nakładania dwóch map PNG o jednakowej siatce.*

Narzędzie łączy dwa wcześniej pobrane zestawy segmentów PNG o **identycznych nazwach plików, wymiarach i siatce**:

1. Wskaż folder **dolna (podkładowa)** i **górna (nakładana)**. Pamiętaj, że ich wskazanie przełącza bieżący folder sesji
   okna głównego (żeby ułatwić pobranie obu map po kolei).
2. Wskaż osobny folder **połączone** (zostanie utworzony automatycznie, musi być inny niż oba foldery źródłowe).
3. Kliknij **Składaj warstwy** (w instrukcji widocznej wewnątrz samego okna programu przycisk nazywany jest jeszcze „Połącz mapy”).

Program kopiuje `conf.txt` z warstwy dolnej, zamienia **białe piksele** warstwy górnej na przezroczyste i zapisuje
wynikowe segmenty PNG (obsługuje także 8-bitowe PNG z paletą barw).

{: .wskazowka }
Jeśli dopiero planujesz pobrać dwie mapy razem, wygodniej jest połączyć je od razu w panelu *Wybrane warstwy* okna
głównego (z regulacją krycia), a dla sesji z różnymi układami lub w JPEG w oknie [Eksport mapy](../eksport/nakladki/).

---

## Usuwanie pustych segmentów

**Gdzie:** menu *Narzędzia > Usuwanie pustych segmentów* (<kbd>Ctrl</kbd>+<kbd>U</kbd>).

![Okno Usuwanie pustych segmentów]({{ '/pomoc/obrazy/14-usuwanie-pustych.png' | relative_url }})

*Rysunek 2. Okno usuwania pustych segmentów na podstawie progu rozmiaru pliku.*

Na brzegach map historycznych lub tematycznych o ograniczonym zasięgu powstają białe albo jednokolorowe segmenty, które
mają wyraźnie mniejszy rozmiar pliku od segmentów z treścią.

1. Wybierz rozszerzenie plików z listy (`jpg`, `png`, `tif`, `gif`). Program od razu zlicza pliki w folderze bieżącej
   sesji i pokazuje ich najmniejszy, największy oraz średni rozmiar w `kB`.
2. Sprawdź w Eksploratorze rozmiar przykładowego „pustego” segmentu i wpisz w polu *Usuń kafle mniejsze niż* wartość
   większą o ok. 10%. Pole *Liczba plików zaznaczonych do usunięcia* na bieżąco podaje, ile segmentów spełnia warunek.
3. Kliknij **Usuń** i potwierdź.

Wybrane segmenty **wraz z ich plikami georeferencyjnymi** (`.map`, `.kml`, `.tab`, `.wld`, `.prj`, `.jpgw`, `.points` itp.)
zostają przeniesione do **Kosza** systemu Windows (pomyłkę można więc cofnąć).

{: .informacja }
Jeżeli później wznowisz pobieranie w tym samym folderze, włącz w *Ustawieniach* opcję *pobieraj tylko segmenty o numerze
wyższym niż ostatni pobrany*, inaczej program pobierze usunięte segmenty od nowa.

---

## Mapa TrekBuddy / Locus Map (.tar)

Tryb włączany w [Ustawieniach](../ustawienia/#trekbuddy-i-locus-map) (grupa *TrekBuddy i Locus Map*). Podczas pobierania
program ustawia stały bok segmentu `512 × 512 px`, zapisuje kafle w podfolderze `set\`, generuje plik kalibracji `.map`
i spis `.set`, a po zakończeniu pobierania pakuje mapę do jednego archiwum `.tar` w folderze sesji. Po wyłączeniu tego
trybu przywracany jest poprzedni rozmiar boku segmentu.

---

➡️ **Kolejny krok:** [Katalog zbiorów map dostarczanych z programem →](../zbiory-map/)

*Zobacz też: [Błędy i puste segmenty](../pobieranie/bledy/) · [Nakładki i łączenie warstw](../eksport/nakladki/) · [Sesje pobierania](../sesje/)*
