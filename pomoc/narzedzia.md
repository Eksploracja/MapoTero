---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Konwerter map OziExplorer, usuwanie pustych segmentów oraz tworzenie paczek TrekBuddy (.tar).
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

## Nakładanie dwóch pobranych map

Dawne narzędzie *Nakładanie dwóch map png na siebie* zostało usunięte - to samo, z regulacją krycia, dla map o różnej
siatce i w każdym formacie, robią:

* panel *Wybrane warstwy* okna głównego - gdy obie mapy dopiero pobierasz (warstwy łączone od razu przy pobieraniu),
* okno [Eksport mapy z nakładką](../eksport/nakladki/) - gdy obie mapy są już pobrane w osobnych sesjach
  (także scalony arkusz GeoTIFF z plikami dla programów CAD i GIS).

---

## Korekta i optymalizacja obrazów w folderze

**Gdzie:** menu *Narzędzia > Korekta i optymalizacja obrazów w folderze...* albo przycisk w oknie [Eksport do CAD](../cad/).

Narzędzie poprawia naraz wszystkie obrazy PNG, JPEG i TIFF leżące w jednym folderze (np. segmenty sesji):

1. Wskaż **folder z obrazami**. Program podpowiada folder bieżącej sesji (a z okna eksportu do CAD - folder tego eksportu).
2. Wskaż **folder wynikowy**, inny niż źródłowy. Domyślnie to podfolder `korekta`. Oryginały zostają bez zmian.
3. Przyciskiem **Profil / Podgląd** wybierz profil korekty i sprawdź efekt na próbce z folderu: *Garmin MIP*, *Wyostrzenie krawędzi*,
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
Program zapisuje nazwy usuniętych segmentów w pliku `usuniete_puste_segmenty.txt` w folderze sesji. Jeżeli później
wznowisz pobieranie w tym folderze, program ich nie pobierze; pobierze tylko segmenty brakujące z innego powodu.
Usunięte segmenty pobiera ponownie dopiero odświeżanie mapy (menu *Sesja pobierania*).

---

## Mapa TrekBuddy / Locus Map (.tar)

Tryb włączany w [Ustawieniach](../ustawienia/#trekbuddy-i-locus-map) (grupa *TrekBuddy i Locus Map*). Podczas pobierania
program ustawia stały bok segmentu `512 × 512 px`, zapisuje kafle w podfolderze `set\`, generuje plik kalibracji `.map`
i spis `.set`, a po zakończeniu pobierania pakuje mapę do jednego archiwum `.tar` w folderze sesji. Po wyłączeniu tego
trybu przywracany jest poprzedni rozmiar boku segmentu.

---

➡️ **Kolejny krok:** [Katalog zbiorów map dostarczanych z programem →](../zbiory-map/)

*Zobacz też: [Błędy i puste segmenty](../pobieranie/bledy/) · [Nakładki i łączenie warstw](../eksport/nakladki/) · [Sesje pobierania](../sesje/)*
