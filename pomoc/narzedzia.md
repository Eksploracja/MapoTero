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

1. **Cel** - telefon lub tablet (MBTiles, GeoPackage albo SQLiteDB), aplikacje terenowe GIS (GeoPackage dla QField, Mergin Maps, QGIS), zgodny odbiornik Garmin (KMZ dla Custom Maps albo JNX dla BirdsEye), wydruk (arkusz GeoTIFF) lub podkład CAD/GIS (GeoTIFF w PL-1992).
2. **Obszar** - miejscowość z promieniem, prostokąt zaznaczony na mapie albo pas wzdłuż trasy GPX/KML/KMZ. Obszar wybiera się przed stylem, żeby podgląd stylu pokazywał Twój teren. Jeśli przed otwarciem kreatora zaznaczysz obszar na mapie prawym przyciskiem myszy (i ewentualnie dopasujesz go uchwytami), kreator pominie ten krok i przejdzie od celu do stylu - inny obszar wybierzesz, klikając *Obszar* na pasku kroków. Obszar z poprzedniego uruchomienia programu nie jest pomijany. W polu wyszukiwania można wkleić współrzędne punktu (np. `50.0614, 19.9366` z Google Maps, `50°03'41"N 19°56'12"E` albo X Y w PL-1992 lub PL-2000, X - północ) - obszar wyznacza wtedy promień wokół punktu. Zamiast promienia można wybrać *wg granic* - obszarem jest wtedy prostokąt otaczający miejscowość z wyszukiwarki (dla adresu co najmniej 1 km). Identyfikator działki TERYT (np. `121901_1.0001.123/4`) albo zapis „działka Wieliczka 123/4” wyszukuje działkę w usłudze ULDK GUGiK, a obszarem jest działka z pasem 150 m. Plik trasy można też przeciągnąć myszą na okno kreatora, a przycisk *Profil…* pokazuje profil wysokościowy wczytanej trasy. Podgląd obszaru mieści cały obszar i pokazuje jego obrys albo pas trasy.
3. **Styl** - gotowy zestaw warstw z podglądem: turystyka piesza (mapa topograficzna GUGiK z cieniowaniem i szlakami), góry z poziomicami OpenTopoMap z cieniowaniem (także po czeskiej i słowackiej stronie), trasy rowerowe i MTB, dziedzictwo kulturowe (rzeźba terenu LiDAR z prześwitującą mapą topograficzną 1:10 000 i zabytkami NID - rejestr i ewidencja zabytków nieruchomych oraz stanowisk archeologicznych), mapy WIG 1918–1939, mapy niemieckie do 1945 (Messtischblatt, Karte des Deutschen Reiches), działki EGiB albo własne warstwy z okna głównego. Suwaki regulują krycie cieniowania, szlaków, mapy topograficznej i zabytków (krycie ustawione w oknie *Warstwy - zaawansowane* jest nadrzędne - suwak zmienia je proporcjonalnie), w dziedzictwie kulturowym pola wyboru włączają zabytki archeologiczne i nieruchome, a podgląd pokazuje środek wybranego obszaru - można go przesuwać myszką i oddalać - w stylu wieloskalowym pokazuje wtedy mapę w mniejszej skali. Przycisk w rogu podglądu (albo podwójne kliknięcie) otwiera duży podgląd w skali 1:1, w którym widać czytelność napisów. Przy mapach historycznych pod treścią kroku widać ograniczenia pokrycia arkuszami. Przycisk *Zapisz jako mój styl…* zapisuje bieżący skład warstw z kryciem, trybami mieszania i skalami pod własną nazwą - styl pojawia się na liście w kategorii *Moje style* także po ponownym uruchomieniu programu (plik `szablony_wlasne.json` w folderze programu albo w `%LocalAppData%\MapoTero`), a *Usuń mój styl* go usuwa.
4. **Zapis** - format (zalecany jest zaznaczony), szacowany rozmiar i czas, rozmiar piksela z opisem szczegółowości. W opcjach zaawansowanych pole *Korekta obrazu* i przycisk *Profil / Podgląd…* (to samo okno korekty co w oknie warstw); korekta jest domyślnie wyłączona, a okno korekty podpowiada profil (np. Garmin MIP dla odbiorników Garmin) i ma przycisk *Resetuj korektę*. Przy wydruku (GeoTIFF) kreator podaje rozmiar arkusza w centymetrach i najmniejszy format papieru A, na którym się mieści. Przycisk *Generuj mapę* pobiera dane i zapisuje plik, a po zakończeniu *Otwórz mapę* otwiera ją w programie przypisanym w Windows (np. GeoTIFF w QGIS, KMZ w Google Earth).

**Mapy wieloskalowe.** Turystyka piesza, góry z poziomicami, trasy rowerowe i MTB oraz dwa style historyczne mogą przełączać mapy zależnie od poziomu powiększenia w MBTiles, SQLiteDB (OsmAnd, Locus Map) oraz w JNX dla odbiorników Garmin (BirdsEye).

| Styl | Skale (od zbliżenia do oddalenia) |
|---|---|
| Turystyka piesza | 1:10 000 → 1:25 000 → 1:50 000 → 1:100 000 → mapa przeglądowa |
| Mapy WIG 1918–1939 | WIG 1:25 000, a gdzie jej brak - Messtischblatt 1:25 000 lub WIG 1:100 000 → 1:100 000 → 1:300 000 |
| Mapy niemieckie do 1945 | Messtischblatt 1:25 000 (gdzie indziej Karte des Deutschen Reiches) → KdR 1:100 000 → 1:300 000 |
| Góry z poziomicami, rower i MTB | mapa szczegółowa → średnia → przeglądowa |

Kreator pobiera każdą skalę osobno (podfoldery `_skala2`, `_skala3`… obok sesji) i sam dzieli poziomy powiększeń między skale. Przycisk *Ustawienia skal…* w kroku 4 pozwala wyłączyć skalę, zmienić jej piksel i zakres powiększeń oraz wybrać jej warstwy (zakresy nie mogą się nakładać). Pole *Mapa wieloskalowa* można odznaczyć - wtedy cały plik powstaje z jednej skali.

**Ortofotomapa przy zbliżeniu.** W stylach *Turystyka piesza* i *Góry z poziomicami* pole *Ortofotomapa przy zbliżeniu* w kroku 4 dodaje nad najdokładniejszą mapą jeszcze jedną skalę: zdjęcie lotnicze ze szlakami na powiększeniu 17 (piksel 0,75 m). Mapa 1:10 000 jest na tym powiększeniu już tylko powiększonymi pikselami, a na zdjęciu widać polany, piargi, ścieżki i budynki. Pole jest domyślnie wyłączone, bo plik i czas pobierania rosną kilkakrotnie (szacunek w kroku 4 to uwzględnia). W turystyce pieszej ortofotomapa obejmuje tylko Polskę - za granicą zbliżenie jest czarne. Styl górski łączy ortofotomapy Polski, Czech, Słowacji i Saksonii. Przy własnym planie skal ortofotomapę włącza się w oknie *Ustawienia skal…*.

Podgląd stylu pokazuje obrys zapisu oraz pozwala wybrać konkretną skalę. Zmiana samego krycia, mieszania albo korekty obrazu zachowuje plan skal. Przy zmianie składu lub kolejności warstw kreator pyta o użycie własnej kompozycji. Wszystkie powiększenia pokazują ten sam wybrany obszar. Na brzegach map MBTiles i SQLiteDB tło poza obszarem jest przezroczyste. Arkusz GeoTIFF ma dokładny zasięg w PL-1992, a kafle JPEG dla Garmina mieszczą się w prostokącie wpisanym w obszar w WGS84.

**Pokrycie map historycznych.** WIG 1:25 000 zachowała się tylko dla części II RP, Messtischblatt obejmuje ziemie dawnych Niemiec, a WIG 1:100 000 - II RP i pas sąsiednich terenów. Dlatego w szczegółowej skali stylu WIG na dole leży WIG 1:100 000, na nim Messtischblatt, a na wierzchu WIG 1:25 000. Mapy niemieckie w centralnej i wschodniej Polsce są puste.

Wskazówki:

- Cieniowanie rzeźby terenu w stylach używa trybu mieszania *Rzeźba terenu*: płaski teren zachowuje barwy mapy, a stoki w cieniu ciemnieją. Ten sam tryb można wybrać w oknie *Warstwy - zaawansowane*.
- Rozmiar piksela i zakres powiększeń są dobrane do rozdzielczości danych stylu. Gdy piksel jest mniejszy niż dane, kreator to pokazuje i proponuje właściwą wartość.
- Większość odbiorników z obsługą Garmin Custom Maps przyjmuje do 100 kafli JPEG. Wybrane modele obsługują do 500. Przy większym obszarze kreator ostrzega.
- Podczas pobierania kreator pokazuje pozostały czas - po kilku sekundach liczy go z rzeczywistej prędkości pobierania i liczby segmentów wszystkich skal.
- Gdy części fragmentów nie uda się pobrać, *Ponów pobieranie* pobiera tylko brakujące i zapisuje plik ponownie; *Wstecz* wraca do ustawień.
- Mały obszar korzysta z mniejszych segmentów przy tej samej rozdzielczości. Na trasach obraz w pliku jest przycięty do szerokości pasa, z przezroczystym tłem w MBTiles, SQLiteDB i GeoTIFF oraz białym w formatach JPEG Garmina.
- Kreator wskazuje skale z brakami i zachowuje poprzednią mapę, dopóki nowa nie zostanie poprawnie pobrana i zapisana.
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
* **Pomoc w oknie:** przycisk **Pomoc** w lewym dolnym rogu (oraz klawisz <kbd>F1</kbd>) wyświetla podręczną instrukcję parametrów konwersji i transformacji elipsoid.

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
Przycisk **Pomoc** (oraz klawisz <kbd>F1</kbd>) otwiera podręczną instrukcję z opisem profili rastrowych.

---

## Profil wysokościowy trasy

**Gdzie:** menu *Narzędzia > Profil wysokościowy trasy (GPX, KML)...*

1. Przyciskiem **Wybierz...** wskaż plik trasy GPX, KML albo KMZ. Okno pokazuje długość trasy i liczbę punktów.
2. Kliknij **Pobierz wysokości z NMT**. Program pobiera z usługi wysokościowej GUGiK (lotniczy skaning laserowy LIDAR)
   wysokości terenu dla wszystkich punktów trasy. Dane obejmują tylko Polskę - punkty za granicą zostają bez wysokości.
3. Wykres pokazuje profil terenu wzdłuż trasy z wysokością najniższą i najwyższą oraz sumą podejść i zejść.
4. **Zapisz GPX z wysokościami...** zapisuje ślad ze znacznikami `<ele>` dla zegarków i odbiorników GPS. Z pliku GPX
   zostają czasy punktów, punkty `wpt` i nazwy.

W dolnym rogu okna przycisk **Pomoc** (oraz klawisz <kbd>F1</kbd>) otwiera opis działania usługi wysokościowej NMT.
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
