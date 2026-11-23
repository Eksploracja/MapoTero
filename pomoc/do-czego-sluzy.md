---
title: Do czego służy MapoTero
nav_order: 2.5
---

# Do czego służy MapoTero
{: .no_toc }

Przeglądarki internetowe pozwalają oglądać mapy z państwowych serwerów tylko na ekranie komputera. MapoTero służy do tego, żeby te same mapy pobrać na dysk, połączyć w jeden plik i zabrać ze sobą w teren albo wstawić do projektu technicznego.
{: .fs-6 }

**Dotyczy wersji:** {{ site.wersja_programu }}

---

## Dwie drogi pracy: kreator lub pełna kontrola

Program oferuje dwie wygodne ścieżki przygotowania mapy:

* **Szybki kreator mapy (<kbd>Ctrl</kbd>+<kbd>K</kbd>, przycisk z różdżką pod mapą):** najprostszy sposób dla każdego. Prowadzi w czterech krokach: wybór urządzenia docelowego (telefon, Garmin, wydruk, CAD/GIS), wskazanie obszaru (miejscowość z promieniem, działka, trasa GPX lub zaznaczenie), wybór gotowego stylu z podglądem na żywo i automatyczne wygenerowanie gotowego pliku. Sam dobiera optymalną rozdzielczość piksela, format oraz poziomy powiększeń.
* **Klasyczny tryb okna głównego:** pełna ręczna kontrola nad każdym parametrem. Samodzielny dobór do 12 warstw w kanapce z precyzyjną regulacją krycia, ręczne wyznaczanie siatki segmentów na mapie oraz zaawansowane opcje eksportu i scalania arkuszy.

---

## Główne zastosowania

### 1. Eksploracja terenu i poszukiwanie śladów historii
To z tej potrzeby narodził się program na Pomorskim Forum Eksploracyjnym. W lesie albo na bagnach nie ma zasięgu sieci komórkowej, a odczytanie terenu wymaga porównania kilku niezależnych źródeł:
* **Numeryczny model terenu (cieniowanie LiDAR):** rzeźba terenu pobrana z Geoportalu odsłania ukryte pod koronami drzew transzeje, okopy, kurhany, grodziska i dawne wały graniczne. Nowy **tryb mieszania „Rzeźba terenu”** pozwala nałożyć światłocień na mapę lub zdjęcie lotnicze bez szarej zasłony na płaskich obszarach.
* **Styl Dziedzictwo kulturowe:** gotowa kompozycja łącząca plastyczną rzeźbę terenu LiDAR z czytelnym podkładem mapy topograficznej oraz urzędowymi danymi Narodowego Instytutu Dziedzictwa (NID) – rejestrem i ewidencją zabytków nieruchomych oraz stanowisk archeologicznych.
* **Nakładanie map archiwalnych na współczesne:** dawne mapy topograficzne (WIG 1918–1939, pruskie *Messtischblatty* i *Grossblatty* do 1945 r., mapy zaborcze) nałożone na dzisiejsze zdjęcia lotnicze ujawniają zatarte dukty, fundamenty dawnych osad, leśniczówek, młynów czy zapomnianych cmentarzy. Program sam uzupełnia brakujące arkusze szczegółowe mapami w mniejszej skali.
* **Konwerter map OziExplorer (.map):** przetwarza kalibrowane skany papierowych map turystycznych i archiwalnych do nowoczesnych formatów mobilnych (MBTiles, SQLiteDB, OruxMaps, Garmin JNX/KMZ). Automatycznie przelicza historyczne elipsoidy (Borowa Góra, Potsdam, Pulkovo 1942) do WGS84, likwidując typowe przesunięcie o 100–200 m względem wskazań GPS.
* **Mapa zawsze pod ręką:** gotowy plik w telefonie lub odbiorniku turystycznym działa w 100% offline, bez internetu i bez opóźnień.

### 2. Wyprawy, turystyka i nawigacja bez internetu
Wędrówki z plecakiem, wyprawy rowerowe, kajakowe, narciarskie i trasy offroad z dala od głównych dróg i zasięgu sieci:
* **Mapy wieloskalowe w jednym pliku:** w formatach MBTiles, GeoPackage, SQLiteDB oraz Garmin JNX mapa automatycznie zmienia się wraz ze stopniem przybliżenia ekranu. Z daleka widzisz ogólny zarys okolicy (np. mapa przeglądowa lub 1:100 000), a po zbliżeniu włącza się szczegółowa mapa topograficzna (1:25 000, 1:10 000), leśna lub ortofotomapa.
* **Dedykowane style turystyczne:** gotowe zestawy warstw dla turystyki pieszej, *Góry z poziomicami OpenTopoMap* (z cieniowaniem rzeźby, działające również po czeskiej i słowackiej stronie granicy) oraz *Trasy rowerowe i MTB*.
* **Pobieranie tylko wzdłuż trasy (korytarz):** narzędzie korytarza wycina z mapy pas o wybranej szerokości wzdłuż śladu GPX, KML lub KMZ. Zamiast pobierać całe województwo, dostajesz dokładnie ten fragment terenu, przez który przebiega szlak (oszczędność do 90% danych).
* **Profil wysokościowy trasy:** narzędzie pobiera z państwowego lotniczego skaningu laserowego (LiDAR NMT GUGiK) dokładne wysokości terenu dla punktów śladu, rysuje profil z sumą podejść i zejść oraz zapisuje plik GPX wzbogacony o znaczniki wysokości `<ele>`.
* **Współpraca ze sprzętem turystycznym:**
  * Odbiorniki Garmin: format Garmin BirdsEye (JNX v4) bez limitu kafli oraz standardowy Garmin Custom Maps (KMZ) z opcją automatycznego podziału na tomy (gdy mapa przekracza limit 100 kafli urządzenia) i filtrem MIP optymalizującym kontrast w pełnym słońcu.
  * Smartfony i tablety: formaty MBTiles, GeoPackage, SQLiteDB i OruxMaps dla aplikacji Locus Map, OsmAnd, Guru Maps (na iPhone, iPad i Androida), OruxMaps czy AlpineQuest.

### 3. Podkłady do projektowania w CAD (AutoCAD, BricsCAD, ZWCAD)
Projektanci dróg, sieci uzbrojenia terenu, architekci krajobrazu i geodeci potrzebują aktualnego tła fotograficznego lub ewidencyjnego:
* **Automatyczne wpasowanie w układzie współrzędnych:** program generuje plik wymiany DXF, skrypt wsadowy SCR albo program LISP, który wstawia podkłady rastrowe precyzyjnie we właściwe miejsce rysunku (np. w układzie PL-2000 strefy 5–8 lub PL-1992).
* **Automatyczna zamiana osi X/Y:** w geodezji i MapoTero X oznacza oś pionową (północ), a Y poziomą (wschód). Środowiska CAD stosują odwrotną konwencję matematyczną. Program automatycznie dokonuje właściwej transformacji osi.
* **Profile korekty dla CAD:** dedykowane profile obrazu (podkład CAD stonowany, skan czarno-biały) pozwalają przygasić podkład, aby rysowane wektory i osie projektowe pozostały idealnie czytelne.
* **Bez żmudnej ręcznej kalibracji:** raster siada dokładnie na współrzędnych projektowych, w metrach, bez konieczności kupowania płatnych wtyczek geodezyjnych.

### 4. Płynna praca w programach GIS i pomiary w terenie (QGIS, ArcGIS, QField)
Podłączanie serwerów WMS bezpośrednio w programie QGIS bywa uciążliwe przy wolnym łączu lub przeciążeniu serwerów zewnętrznych:
* **Wyszukiwarka działek katastralnych ULDK (<kbd>Ctrl</kbd>+<kbd>D</kbd>):** błyskawicznie odnajduje działkę ewidencyjną po identyfikatorze TERYT lub nazwie obrębu i numerze, pobiera dokładną geometrię granic z rejestru EGiB GUGiK, wyznacza obszar z bezpiecznym buforem i automatycznie włącza ortofotomapę z granicami katastralnymi.
* **Jeden duży arkusz GeoTIFF na dysku:** MapoTero pobiera wybrany obszar, scala segmenty i zapisuje plik GeoTIFF / BigTIFF z bezpośrednio zaszytą georeferencją oraz kompresją Deflate lub JPEG.
* **Błyskawiczne działanie:** praca na lokalnym pliku z dysku jest natychmiastowa, bez czekania na dociąganie kafelków przy każdym przesunięciu czy powiększeniu widoku.
* **Pomiary i podkłady w terenie (QField, Mergin Maps, Avenza Maps):** wyeksportowane pliki MBTiles oraz kafelkowe bazy GeoPackage (.gpkg) działają w mobilnym QFieldzie, Mergin Maps czy Locus Map na telefonie lub tablecie, służąc leśnikom, przyrodnikom i geodetom jako szybki, bezobsługowy podkład pod rejestrowane obiekty przestrzenne.

---

## Udogodnienia ułatwiające pracę

* **Bezpośredni transfer map na telefon przez Wi-Fi i kod QR:** błyskawiczne przesyłanie wyeksportowanej mapy (.mbtiles, .gpkg, .tif, .kmz, .sqlitedb, .tar) prosto do pamięci telefonu przez domowe Wi-Fi — bez szukania kabla USB, bez logowania i bez chmury (zob. [Przesyłanie przez Wi-Fi](../narzedzia/#przesyłanie-mapy-na-telefon-przez-wi-fi-i-kod-qr)).
* **Bezpośrednie kopiowanie na odbiornik Garmin przez USB:** program automatycznie wykrywa podłączony odbiornik Garmin oraz karty microSD, sprawdza wolną pamięć i jednym kliknięciem kopiuje pliki KMZ lub JNX do właściwych folderów (`Garmin/CustomMaps` lub `Garmin/BirdsEye`).
* **Wyszukiwarka z autouzupełnianiem (<kbd>Ctrl</kbd>+<kbd>F</kbd>):** podpowiedzi miejscowości i adresów w trakcie pisania (geokodery Photon i OSM Nominatim) z priorytetem odległości od centrum mapy. Obsługuje także bezpośrednie wklejanie współrzędnych (WGS84, PL-1992, PL-2000).
* **Podgląd zestawu warstw na żywo:** zakładka *Podgląd* w prawym panelu renderuje próbkę miksowania warstw ze środka siatki w skali 1:1, z lupą ×2 i widokiem oddalonym ×½, od razu uwzględniając krycie i tryby mieszania.
* **Korekta obrazów w folderze:** narzędzie do wsadowej optymalizacji kontrastu, ostrości i nasycenia dla całych folderów map lub skanów z zachowaniem plików georeferencji.
* **Konwerter kafelkowanego GeoTIFF:** automatyczne kafelkowanie 256×256 i piramida podkatalogów pomniejszeń (Cloud-Optimized GeoTIFF) dla natychmiastowego otwierania wielkich arkuszy.
* **Dedykowany przycisk „Pomoc” w każdym oknie:** każde niezależne narzędzie programu posiada przycisk *Pomoc* (oraz skrót klawiszowy <kbd>F1</kbd>), który otwiera podręczną instrukcję z bezpośrednim odnośnikiem do odpowiedniego rozdziału podręcznika.

---

## Jakie mapy można pobrać

W programie jest wbudowany katalog kilkudziesięciu polskich i europejskich serwisów mapowych, a kreator serwerów pozwala dodać dowolną inną usługę WMS, WMTS lub kafelki XYZ/TMS:
* **Ortofotomapy:** zdjęcia lotnicze i satelitarne o wysokiej rozdzielczości (do 5–25 cm/pix, teren całego kraju).
* **Mapy topograficzne:** urzędowa Baza Danych Obiektów Topograficznych (BDOT10k) oraz historyczne skany map sztabowych GUGiK (1:10 000, 1:25 000, 1:50 000, 1:100 000).
* **Cieniowanie rzeźby terenu:** numeryczny model terenu oparty o skanowanie laserowe LiDAR NMT (siatka 1 m).
* **Ewidencja gruntów i budynków:** granice i numery działek z Krajowej Integracji Ewidencji Gruntów (KIEG / EGiB).
* **Dziedzictwo kulturowe i zabytki:** oficjalne bazy Narodowego Instytutu Dziedzictwa (zabytki nieruchome i archeologiczne).
* **Mapy historyczne:** zbiory Wojskowego Instytutu Geograficznego (WIG), mapy pruskie (*Messtischblatt*, *Grossblatt*), austro-węgierskie i rosyjskie.
* **Przyroda i lasy:** Bank Danych o Lasach (BDL), granice form ochrony przyrody (GDOŚ), mapy hydrograficzne i sozologiczne.
* **Geologia:** szczegółowe mapy geologiczne i osuwiskowe Państwowego Instytutu Geologicznego (PIG-PIB).
* **Serwisy powiatowe i miejskie:** bazy uzbrojenia terenu (GESUT) i obiekty topograficzne (BDOT500) z portali powiatowych i gminnych (dostępne z poziomu wyszukiwarki rejestru EZiUDP w kreatorze serwerów).
* **Strefa przygraniczna:** zintegrowane usługi państwowe Czech (ČÚZK), Słowacji (GKÚ), Niemiec (Brandenburgia, Saksonia) i Litwy, pozwalające pobierać spójne mapy w rejonach granicznych.

---

➡️ **Kolejny krok:** [Pierwsze kroki w MapoTero →](../pierwsze-kroki/)
