---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Konwerter map OziExplorer, nakładanie dwóch pobranych map PNG, usuwanie pustych segmentów oraz tworzenie paczek TrekBuddy (.tar).
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia* oraz menu *Ustawienia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Konwerter map OziExplorer (.map)

**Gdzie:** menu *Narzędzia > Konwertuj mapę OziExplorer (.map)...* albo przeciągnięcie pliku `.map` lub całego folderu na okno programu (Drag &amp; Drop).

Narzędzie przetwarza kalibrowane skany papierowych map turystycznych i archiwalnych (pliki `.map` wraz ze skanem w formatach JPG, PNG, TIF lub BMP) do nowoczesnych formatów mobilnych: **Garmin BirdsEye (JNX v4)**, **Garmin Custom Maps (KMZ)** oraz **MBTiles**:

* **Automatyczna transformacja Datum (elipsoid historycznych):** program automatycznie wykrywa i przelicza historyczne polskie i europejskie układy odniesienia do współczesnej elipsoidy WGS84:
  * **Borowa Góra** (mapy Wojskowego Instytutu Geograficznego WIG 1:100 000 i 1:25 000),
  * **Potsdam / Rauenberg** (niemieckie mapy Messtischblatt 1:25 000 i Grossblatt),
  * **Pulkovo 1942 / Układ 1965 / Układ 1942** (mapy topograficzne z okresu PRL).
  Dzięki precyzyjnemu przeliczaniu współrzędnych arkusz nie wykazuje typowego przesunięcia o 100–200 metrów względem śladów GPS w odbiornikach.
* **Modele kalibracji nieliniowej:** dla dużych arkuszy program stosuje model rzutowy (4–6 punktów) lub wielomian 2. stopnia (7+ punktów), redukując zniekształcenia skanera papieru. Punkty kalibracji mogą być podane w stopniach lub w siatce UTM.
* **Piramida powiększeń:** formaty JNX i MBTiles generowane są automatycznie z kompletną piramidą 5 poziomów skali.
* **Praca pojedyncza lub wsadowa (Batch):** możesz wskazać pojedynczy plik `.map` albo cały katalog zawierający dziesiątki arkuszy – program przetworzy je kolejno w tle.

---

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

## Korekta i optymalizacja obrazów w folderze

**Gdzie:** menu *Narzędzia > Korekta i optymalizacja obrazów w folderze...* albo przycisk w oknie [Eksport do CAD](../cad/).

Narzędzie poprawia naraz wszystkie obrazy PNG, JPEG i TIFF leżące w jednym folderze (np. segmenty sesji):

1. Wskaż **folder z obrazami**. Program podpowiada folder bieżącej sesji (a z okna eksportu do CAD - folder tego eksportu).
2. Wskaż **folder wynikowy**, inny niż źródłowy. Domyślnie to podfolder `korekta`. Oryginały zostają bez zmian.
3. Przyciskiem **Dostosuj** wybierz profil korekty i sprawdź efekt na próbce z folderu: *Garmin MIP*, *Wyostrzenie krawędzi*,
   *Stare mapy i skany*, *Rozjaśnienie lasów*, *Podkład CAD*, *Skan CAD* albo własne wartości suwaków. Profilu *Brak* nie można
   tu wybrać, bo nie zmienia obrazów.
4. Kliknij **Rozpocznij korektę**.

Opcja *Kopiuj pliki georeferencji* kopiuje obok poprawionych obrazów pliki `.jpgw`, `.pngw`, `.tifw`, `.wld`, `.prj`, `.map`,
`.gmi`, `.tab`, `.kml` i inne oraz `conf.txt` sesji. GeoTIFF zachowuje georeferencję zapisaną w pliku, gdy program zna jego układ.
Zamknięcie okna w trakcie pracy przerywa przetwarzanie. Pliki, których nie da się odczytać, program pomija i wymienia po zakończeniu.

---

## Usuwanie pustych segmentów

**Gdzie:** menu *Narzędzia > Usuwanie pustych segmentów* (<kbd>Ctrl</kbd>+<kbd>U</kbd>).

![Okno Usuwanie pustych segmentów]({{ '/pomoc/obrazy/14-usuwanie-pustych.png' | relative_url }})

*Rysunek 2. Okno usuwania pustych segmentów na podstawie progu rozmiaru pliku.*

Na brzegach map historycznych lub tematycznych o ograniczonym zasięgu powstają białe albo jednokolorowe segmenty, które
mają wyraźnie mniejszy rozmiar pliku od segmentów z treścią.

Okno działa na folderze bieżącej sesji, który widać w jego tytule. Inny folder wskażesz przyciskiem **Folder…** obok listy
rozszerzeń. Gdy bieżącej sesji nie ma (np. po usunięciu pobranych map albo przed pierwszym pobraniem), okno nie przechodzi
samo na inną sesję: przycisk **Usuń** jest nieaktywny, dopóki nie wskażesz folderu przyciskiem **Folder…**.

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
