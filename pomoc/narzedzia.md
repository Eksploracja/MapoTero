---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Szybki kreator mapy, konwerter map OziExplorer, profil wysokościowy trasy i korekta obrazów w folderze.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Szybki kreator mapy

**Gdzie:** menu *Narzędzia > Szybki kreator mapy...*, skrót **Ctrl+K** albo przycisk z różdżką pod mapą.

Kreator prowadzi początkujących w czterech krokach od pomysłu do gotowego pliku mapy - bez ręcznego ustawiania układu, siatki i eksportu.

1. **Cel** - telefon lub tablet (MBTiles albo SQLiteDB), odbiornik Garmin (KMZ, który działa na każdym modelu, albo JNX), wydruk (arkusz GeoTIFF) lub podkład CAD/GIS (GeoTIFF w PL-1992).
2. **Styl** - gotowy zestaw warstw z podglądem: turystyka piesza (mapa topograficzna GUGiK z cieniowaniem i szlakami), góry z poziomicami OpenTopoMap (także po czeskiej i słowackiej stronie), trasy rowerowe i MTB, eksploracja terenu LiDAR, plastyczna rzeźba terenu, mapy WIG 1918–1939, mapy niemieckie do 1945 (Messtischblatt, Karte des Deutschen Reiches), działki EGiB albo własne warstwy z okna głównego. Suwak reguluje siłę cieniowania, a podgląd można przesuwać myszką i oddalać - w stylu wieloskalowym pokazuje wtedy mapę w mniejszej skali.
3. **Obszar** - miejscowość z promieniem, prostokąt zaznaczony na mapie albo pas wzdłuż trasy GPX/KML/KMZ.
4. **Zapis** - format (zalecany jest zaznaczony), szacowany rozmiar i czas, rozmiar piksela z opisem szczegółowości. Przycisk *Generuj mapę* pobiera dane i zapisuje plik.

**Mapy wieloskalowe.** Style topograficzne, historyczne, rowerowe, górskie i rzeźby terenu zapisane jako MBTiles przełączają się przy oddalaniu na mapy w mniejszej skali, zamiast pokazywać pomniejszoną, nieczytelną mapę szczegółową:

| Styl | Skale (od zbliżenia do oddalenia) |
|---|---|
| Turystyka piesza | 1:10 000 → 1:50 000 → 1:100 000 → mapa przeglądowa |
| Mapy WIG 1918–1939 | 1:25 000 (gdzie się zachowała, gdzie indziej 1:100 000) → 1:100 000 → 1:300 000 |
| Mapy niemieckie do 1945 | Messtischblatt 1:25 000 (gdzie indziej Karte des Deutschen Reiches) → KdR 1:100 000 → 1:300 000 |
| Góry z poziomicami, rower i MTB, rzeźba terenu | mapa szczegółowa → średnia → przeglądowa |

Kreator pobiera każdą skalę osobno (podfoldery `_skala2`, `_skala3`… obok sesji) i sam dzieli poziomy powiększeń między skale. Pole *Mapa wieloskalowa* w kroku 4 można odznaczyć - wtedy cały plik powstaje z jednej skali.

Wskazówki:

- Cieniowanie rzeźby terenu w stylach używa trybu mieszania *Rzeźba terenu*: płaski teren zachowuje barwy mapy, a stoki w cieniu ciemnieją. Ten sam tryb można wybrać w oknie *Warstwy - zaawansowane*.
- Rozmiar piksela i zakres powiększeń są dobrane do rozdzielczości danych stylu. Gdy piksel jest mniejszy niż dane, kreator to pokazuje i proponuje właściwą wartość.
- Odbiorniki Garmin wyświetlają łącznie do 100 kafli KMZ (nowsze modele 500); przy większym obszarze kreator ostrzega.
- Gdy części fragmentów nie uda się pobrać, *Ponów pobieranie* pobiera tylko brakujące i zapisuje plik ponownie; *Wstecz* wraca do ustawień.
- Segmenty trafiają do folderu `download` (podfolder `Kreator_...`), a po zakończeniu kreator podpowiada, gdzie skopiować plik w telefonie lub odbiorniku.

## Konwerter map OziExplorer (.map)

**Gdzie:** menu *Narzędzia > Konwertuj mapę OziExplorer (.map)...* albo przeciągnięcie pliku `.map` lub całego folderu na okno programu (Drag &amp; Drop).

Narzędzie przetwarza kalibrowane skany papierowych map turystycznych i archiwalnych (pliki `.map` wraz ze skanem w formatach JPG, PNG, TIF lub BMP) do formatów mobilnych: **Garmin BirdsEye (JNX v4)**, **Garmin Custom Maps (KMZ)**, **MBTiles**, **SQLiteDB** (OsmAnd, Locus Map) oraz **OruxMaps** (folder mapy):

* **Automatyczna transformacja Datum (elipsoid historycznych):** program automatycznie wykrywa i przelicza historyczne polskie i europejskie układy odniesienia do współczesnej elipsoidy WGS84:
  * **Borowa Góra** (mapy Wojskowego Instytutu Geograficznego WIG 1:100 000 i 1:25 000),
  * **Potsdam / Rauenberg** (niemieckie mapy Messtischblatt 1:25 000 i Grossblatt),
  * **Pulkovo 1942 / Układ 1965 / Układ 1942** (mapy topograficzne z okresu PRL).
  Dzięki precyzyjnemu przeliczaniu współrzędnych arkusz nie wykazuje typowego przesunięcia o 100–200 metrów względem śladów GPS w odbiornikach.
* **Modele kalibracji nieliniowej:** dla dużych arkuszy program stosuje model rzutowy (4–6 punktów) lub wielomian 2. stopnia (7+ punktów), redukując zniekształcenia skanera papieru. Punkty kalibracji mogą być podane w stopniach lub w siatce UTM.
* **Piramida powiększeń:** formaty JNX, MBTiles, SQLiteDB i OruxMaps generowane są automatycznie z kompletną piramidą 5 poziomów skali.
* **Korekta obrazu:** pole *Korekta obrazu* i przycisk *Korekta obrazu...* działają jak w oknie eksportu (próbka ze skanu); profil jest wspólny z eksportem.
* **Praca pojedyncza lub wsadowa (Batch):** możesz wskazać pojedynczy plik `.map` albo cały katalog zawierający dziesiątki arkuszy – program przetworzy je kolejno w tle.

---

## Nakładanie dwóch pobranych map

Dawne narzędzie *Nakładanie dwóch map png na siebie* zostało usunięte - to samo, z regulacją krycia, dla map o różnej
siatce i w każdym formacie, robią:

* panel *Wybrane warstwy* okna głównego - gdy obie mapy dopiero pobierasz (warstwy łączone od razu przy pobieraniu),
* okno [Eksport mapy z nakładką](../eksport/nakladki/) - gdy obie mapy są już pobrane w osobnych sesjach
  (także scalony arkusz GeoTIFF z plikami dla programów CAD i GIS).

---

## Korekta obrazów w folderze

**Gdzie:** menu *Narzędzia > Korekta obrazów w folderze...* albo przycisk w oknie [Eksport do CAD](../cad/).

Narzędzie poprawia naraz wszystkie obrazy PNG, JPEG i TIFF leżące w jednym folderze (np. segmenty sesji):

1. Wskaż **folder z obrazami**. Program podpowiada folder bieżącej sesji (a z okna eksportu do CAD - folder tego eksportu).
2. Wskaż **folder wynikowy**, inny niż źródłowy. Domyślnie to podfolder `korekta`. Oryginały zostają bez zmian.
3. Przyciskiem **Korekta obrazu...** wybierz profil korekty i sprawdź efekt na próbce z folderu: *Garmin MIP*, *Wyostrzenie krawędzi*,
   *Stare mapy i skany*, *Rozjaśnienie lasów*, *Podkład CAD*, *Skan CAD* albo własne wartości suwaków. Profilu *Brak* nie można
   tu wybrać, bo nie zmienia obrazów.
4. Kliknij **Rozpocznij korektę**.

Opcja *Kopiuj pliki georeferencji* kopiuje obok poprawionych obrazów pliki `.jpgw`, `.pngw`, `.tifw`, `.wld`, `.prj`, `.map`,
`.gmi`, `.tab`, `.kml` i inne oraz `conf.txt` sesji. GeoTIFF zachowuje georeferencję zapisaną w pliku, gdy program zna jego układ.
Zamknięcie okna w trakcie pracy przerywa przetwarzanie. Pliki, których nie da się odczytać, program pomija i wymienia po zakończeniu.

---

## Profil wysokościowy trasy

**Gdzie:** menu *Narzędzia > Profil wysokościowy trasy (GPX, KML)...*

1. Przyciskiem **Wybierz...** wskaż plik trasy GPX, KML albo KMZ. Okno pokazuje długość trasy i liczbę punktów.
2. Kliknij **Pobierz wysokości z NMT**. Program pobiera z usługi wysokościowej GUGiK (lotniczy skaning laserowy LIDAR)
   wysokości terenu dla wszystkich punktów trasy. Dane obejmują tylko Polskę - punkty za granicą zostają bez wysokości.
3. Wykres pokazuje profil terenu wzdłuż trasy z wysokością najniższą i najwyższą oraz sumą podejść i zejść.
4. **Zapisz GPX z wysokościami...** zapisuje ślad ze znacznikami `<ele>` dla zegarków i odbiorników GPS. Z pliku GPX
   zostają czasy punktów, punkty `wpt` i nazwy.

Dawniej profil był częścią okna obszaru pobierania wzdłuż trasy, choć nie ma związku z wyznaczaniem obszaru.

---

## Usunięte narzędzia

* **Usuwanie pustych segmentów** - pobieranie samo ponawia białe segmenty, eksport nie zapisuje pustych kafli, a obszar
  wzdłuż trasy nie pobiera segmentów poza pasem. Narzędzie kasowało pliki według rozmiaru, więc mogło usunąć jasne
  fragmenty prawdziwej mapy.
* **Tryb TrekBuddy w Ustawieniach** - mapę TrekBuddy / Locus Map (`.tar`) tworzy teraz okno
  [Eksport mapy](../eksport/aplikacje/#trekbuddy) z dowolnej pobranej sesji.

---

➡️ **Kolejny krok:** [Katalog zbiorów map dostarczanych z programem →](../zbiory-map/)

*Zobacz też: [Błędy pobierania](../pobieranie/bledy/) · [Nakładki i łączenie warstw](../eksport/nakladki/) · [Sesje pobierania](../sesje/)*
