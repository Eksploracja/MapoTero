---
title: Narzędzia dodatkowe
nav_order: 16
---

# Narzędzia dodatkowe
{: .no_toc }

Konwerter map OziExplorer, profil wysokościowy trasy i korekta obrazów w folderze.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

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
* **Korekta obrazu:** pole *Korekta obrazu* i przycisk *Profil / Podgląd...* działają jak w oknie eksportu (próbka ze skanu); profil jest wspólny z eksportem.
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
