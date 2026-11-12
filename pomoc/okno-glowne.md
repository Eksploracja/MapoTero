---
title: Okno główne
nav_order: 5
---

# Okno główne
{: .no_toc }

Opis każdego elementu okna głównego: mapy podglądu, list zbiorów map i warstw, parametrów siatki i pól współrzędnych.
{: .fs-5 .fw-300 }

**Gdzie w programie:** okno, które pojawia się po uruchomieniu programu  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Widok całego okna

![Okno główne programu MapoTero z ponumerowanymi elementami]({{ '/pomoc/obrazy/03-okno-glowne.png' | relative_url }})

*Rysunek 1. Okno główne programu MapoTero z ponumerowanymi sekcjami.*

| Nr | Element okna | Do czego służy |
|---|---|---|
| **1** | Pasek menu | sesja pobierania (zapis/wczytanie `conf.txt`, korytarz GPX/KML, odświeżanie), narzędzia (kreator serwerów, scalanie, eksport, CAD, wielowarstwowy MBTiles, nakładanie, usuwanie pustych), ustawienia i pomoc |
| **2** | Mapa podglądu | podkład orientacyjny (OSM / Google / Bing), zaznaczanie obszaru prawym przyciskiem myszy oraz siatka segmentów z uchwytami |
| **3** | Drzewo *Wybierz mapy* | wszystkie zbiory i warstwy pogrupowane tematycznie, wyszukiwarka *szukaj warstwy…*, rozwijanie/zwijanie (`⊞`/`⊟`) oraz przycisk `[+]` (kreator serwerów) |
| **4** | *Wybrane warstwy oraz ich kolejność* | „kanapka” do 12 warstw (1 = podkład), zmiana kolejności (`▲`/`▼` lub przeciąganie), usuwanie (`✕`), ustawienie `Krycie %`, przycisk `…` (*Warstwy - zaawansowane*) oraz ikona kosza |
| **5** | Układ, segment i piksel | wybór układu współrzędnych pobierania, bok segmentu `[pix]` i rozmiar piksela `[m/pix]` (lub `[°/pix]` w WGS84) |
| **6** | *Wynikowy rozmiar siatki* | liczba kolumn × wierszy segmentów, całkowite wymiary w pikselach i kilometrach |
| **7** | Zasięg XY i pinezka | współrzędne granic zasięgu na schemacie (pola edycji po włączeniu wpisywania XY w *Ustawieniach*) oraz przycisk centrowania mapy na rzeczywistym zasięgu siatki |
| **8** | Przyciski akcji | **Pobierz**, **Przerwij pobieranie**, scalanie (ikona szachownicy), otwarcie folderu sesji (żółty folder) i *usuń wszystkie pobrane mapy* |
| **9** | *Komunikaty* | komunikaty o postępie, powierzchni zaznaczenia i ewentualnych błędach (kolor oznacza wagę komunikatu) |
| **10** | Pasek stanu | postęp w procentach, format segmentów, bieżący folder sesji i nazwa ostatniego segmentu |

Okno dzieli się na trzy części:

* **po lewej**: mapa podglądu do zaznaczania obszaru i podglądu siatki segmentów,
* **po prawej**: panel *Dane źródłowe* z drzewem map, listą wybranych warstw, ustawieniami układu, segmentu i piksela oraz współrzędnymi zasięgu,
* **na dole**: komunikaty oraz pasek stanu.

Tytuł okna podaje nazwę i numer wersji programu (np. `MapoTero {{ site.wersja_programu }}`). Pasek oddzielający mapę od panelu *Dane źródłowe* można przeciągać myszą, żeby poszerzyć listę warstw.

---

## Menu

| Menu | Polecenie | Skrót | Opis |
|---|---|---|---|
| **Sesja pobierania** | zapisz ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>Z</kbd> | zapisuje parametry bieżącej sesji we wskazanym folderze. Wybrany folder staje się folderem sesji (zob. [Sesje pobierania](../sesje/)) |
| | wczytaj ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>W</kbd> | wczytuje sesję ze wskazanego folderu (wymaga pliku `conf.txt`) |
| | obszar pobierania wzdłuż trasy (GPX, KML, KMZ)... | | [Obszar wzdłuż trasy](../obszar/korytarz/) |
| | usuń obszar wzdłuż trasy | | wraca do pobierania pełnego prostokąta |
| | odśwież mapę (pobierz ponownie wszystkie segmenty) | | [Odświeżanie mapy](../pobieranie/odswiezanie/) |
| **Narzędzia** | Dodaj nowy serwer WMS / WMTS... | | [Kreator serwerów](../serwery/kreator/) |
| | Łączenie segmentów | <kbd>Ctrl</kbd>+<kbd>L</kbd> | [Scalanie segmentów](../scalanie/) |
| | Eksport do KMZ, MBTiles, SQLiteDB, OruxMaps | <kbd>Ctrl</kbd>+<kbd>E</kbd> | [Eksport mapy](../eksport/) |
| | Eksport do CAD (DXF, skrypt SCR, LISP)... | | [Eksport do CAD](../cad/) |
| | Zaawansowany wielowarstwowy MBTiles... | | [Wielowarstwowy MBTiles](../mbtiles-wielowarstwowy/) |
| | Nakładanie dwóch map png na siebie | <kbd>Ctrl</kbd>+<kbd>N</kbd> | [Narzędzia dodatkowe](../narzedzia/) |
| | Usuwanie pustych segmentów | <kbd>Ctrl</kbd>+<kbd>U</kbd> | [Narzędzia dodatkowe](../narzedzia/) |
| **Ustawienia** | | | okno [Ustawienia](../ustawienia/) |
| **Pomoc** | O programie | <kbd>F1</kbd> | wersja i autorzy |
| | Podstawowa instrukcja obsługi | <kbd>F2</kbd> | skrócona instrukcja wbudowana w program |
| | Pomoc na Pomorskim Forum Eksploracyjnym | <kbd>F3</kbd> | otwiera forum w przeglądarce |

Menu *Sesja pobierania* otwiera też <kbd>Alt</kbd>+<kbd>S</kbd>.

---

## Mapa podglądu

Mapa służy do wyboru obszaru i pokazuje siatkę segmentów. Nie jest to pobierana mapa, tylko podkład
do orientacji w terenie.

| Czynność | Jak |
|---|---|
| przesuwanie mapy | przeciągnij lewym przyciskiem myszy |
| przybliżanie i oddalanie | kółko myszy, przyciski **+** i **-** na mapie, dwuklik lewym (przybliża) lub prawym (oddala) przyciskiem |
| zaznaczanie obszaru | przeciągnij **prawym** przyciskiem myszy - [Zaznaczanie na mapie](../obszar/zaznaczanie/) |
| zmiana zasięgu siatki | przeciągnij uchwyt lewym przyciskiem myszy |

**Podkład mapy** wybiera się przełącznikami na mapie: *OpenStreetMap* (domyślny), *GoogleMap* albo
*SatelliteBingsMap* (zdjęcia satelitarne).

**Współrzędne pod mapą** (widoczne po włączeniu opcji *współrzędne kursora i środka mapy* w *Ustawieniach*):

* *Współrzędne kursora*: pozycja kursora w wybranym układzie pobierania (`x=` północ, `y=` wschód) z oznaczeniem strefy (np. *1992*, *2000/6*, *UTM 34N*, *WGS84*).
* *Współrzędne środka okna mapy*: szerokość (*Lat*) i długość (*Lng*) geograficzna WGS84 dla centrum widoku oraz bieżący poziom powiększenia.

Opcja *współrzędne kursora w WGS84* w *Ustawieniach* wyświetla dodatkowo stopnie geograficzne bezpośrednio przy wskaźniku myszy.

Program zapamiętuje położenie i przybliżenie mapy. Po ponownym uruchomieniu mapa wraca do tego samego miejsca.

---

## Zbiory map i warstwy

* **Wybierz mapy (zbiory map i ich warstwy).** Drzewo map pogrupowanych tematycznie: *Ortofotomapy*, *Mapy topograficzne*, *Rzeźba terenu*, *Mapy historyczne i zabytki*, *Ewidencja, geodezja i uzbrojenie*, *Granice i adresy*, *Przyroda i środowisko*, *Geologia (PIG-PIB)* oraz podgrupy (np. *Działki ewidencyjne (KIEG)*, *Mapy społecznościowe i turystyczne*).
  Kliknięcie nazwy grupy rozwija ją lub zwija. Warstwy mają czytelne nazwy i pola wyboru: zaznaczenie pola dodaje warstwę do listy wybranych, a odznaczenie ją usuwa. Możesz łączyć warstwy z różnych grup i serwerów (WMS, WMTS i XYZ/TMS). Przy nazwie grupy w nawiasie widać liczbę aktywnych warstw.
  Gdy na liście znajduje się przezroczysta nakładka (np. szlaki *Waymarked Trails*), a zaznaczysz mapę podkładową, program automatycznie wstawi podkład pod nakładkę. Podpowiedź dymkowa podaje nazwę techniczną na serwerze, zbiór map i zalecany piksel. Zbiory dodane [kreatorem](../serwery/kreator/) trafiają do grupy *Dodane serwery*. Strukturę i nazwy warstw określa plik `warstwy\katalog.txt` (zob. [Katalog zbiorów map](../zbiory-map/)).
* **szukaj warstwy…** Filtruje widoczne pozycje według wpisanych słów (nazwa wyświetlana, nazwa grupy lub kod techniczny, np. `orto wmts`). Wielkość liter i polskie znaki nie mają znaczenia. Klawisz <kbd>Esc</kbd> czyści pole wyszukiwania.
* **⊞ / ⊟** rozwija lub zwija wszystkie grupy w drzewie, a **[+]** uruchamia [kreator dodawania serwera](../serwery/kreator/).
* **Wybrane warstwy oraz ich kolejność.** Lista mieści do 12 warstw ułożonych w stos (ta sama warstwa nie może powtarzać się na liście):
  * **Warstwa 1** to podkład na samym spodzie. Warstwy o wyższych numerach leżą na niej. Widać spod nich podkład tam, gdzie mają przezroczyste tło, niepełne pokrycie terenu lub zmniejszone krycie.
  * Przy warstwach z różnych serwerów widać szarym tekstem ich źródło.
  * **Kolejność i usuwanie.** Zaznaczoną warstwę przesuniesz przyciskami **▲** / **▼**, skrótem <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd> albo przeciągając myszą. Usuniesz ją przyciskiem **✕**, klawiszem <kbd>Delete</kbd> lub odznaczając w drzewie. Przycisk z ikoną kosza czyści całą listę. Menu pod prawym przyciskiem myszy zawiera polecenia *Przesuń w górę / w dół*, *Ustaw jako podkład*, *Krycie* oraz *Usuń*.
  * **Krycie % (5–100%).** Ustawia przezroczystość zaznaczonej warstwy. Pozwala to nałożyć np. cieniowanie rzeźby terenu na mapę topograficzną albo granice działek na ortofotomapę.
  * **Przycisk `…`** otwiera okno *Warstwy - zaawansowane*.
* **Szerokość panelu.** Pionowy pasek oddzielający mapę od panelu bocznego możesz przeciągnąć w lewo, żeby odsłonić długie nazwy warstw. W powiększonym oknie panel zachowuje zadaną szerokość, a przy zwężaniu okna dopasowuje się automatycznie, żeby zmieścić podgląd mapy. Program zapisuje szerokość panelu w `lastsettings.txt`.

{: .informacja }
Sąsiednie warstwy z tego samego serwera WMS o kryciu 100% serwer łączy w jednym zapytaniu. Warstwy z innych serwerów, usługi kafelkowe (WMTS / XYZ / TMS) oraz warstwy o kryciu poniżej 100% program pobiera osobno z przezroczystym tłem i nakłada przy tworzeniu segmentu. Pobrane wcześniej osobne sesje połączysz również przy eksporcie (zob. [Nakładki i łączenie warstw](../eksport/nakladki/)).

### Okno *Warstwy - zaawansowane*

![Okno Warstwy - zaawansowane z suwakami krycia i podglądem kanapki]({{ '/pomoc/obrazy/03-warstwy-zaawansowane.png' | relative_url }})

*Rysunek 2. Okno zaawansowanej kompozycji warstw z suwakami krycia i podglądem na żywo.*

Okno otwierane przyciskiem **…** ułatwia zestrojenie wielowarstwowej kompozycji:

* Pokazuje osobny suwak krycia (5–100%), przyciski kolejności **▲** / **▼** oraz usuwania **✕** dla każdej warstwy.
* Informuje o liczbie zapytań na jeden segment (ile zapytań WMS i ile warstw kafelkowych) oraz ostrzega, gdy nieprzezroczysta warstwa na górze całkowicie zasłania warstwę pod spodem.
* Po prawej stronie wyświetla **próbkę podglądu (300 × 300 px)** ze środka zaznaczonej siatki. Każda warstwa pobierana jest z sieci tylko raz. Przesuwanie suwaków krycia i zmiana kolejności natychmiast przeliczają podgląd bez ponownego pobierania danych.

Kliknięcie **OK** przenosi ustawienia do okna głównego. Przycisk **Anuluj** odrzuca zmiany.

## Układ współrzędnych pobierania

Lista układów: PL-1992, PL-2000 (strefy 5–8), UTM (strefy 33N–35N), WGS84. Zmiana układu:

* przelicza wpisany zasięg i rozmiar piksela do nowego układu,
* zmienia jednostkę piksela na *[°/pix]* dla WGS84,
* dotyczy bieżącej sesji (domyślny układ nowych sesji wybiera się w *Ustawieniach*).

Serwer musi obsługiwać wybrany układ (zob. [Układy współrzędnych](../pojecia/#układy-współrzędnych)).

## Segment i piksel

* **Segment [pix]**: bok kwadratowego segmentu w pikselach, od 1 do 2048 (domyślnie 2000).
* **Piksel [m/pix]**: terenowy rozmiar piksela. Po zaznaczeniu warstwy program ustawia najmniejszy zalecany piksel spośród wszystkich wybranych warstw (np. łącząc mapę topo 2 m z granicami 16 m, zachowa ostrość 2 m). Jeśli wpiszesz wartość ręcznie, program jej nie nadpisze.
* **Rozmiar pojedynczego segmentu**: wymiary segmentu w terenie [km]. W układzie WGS84 podawana jest szerokość × wysokość, ponieważ segment w stopniach nie tworzy w terenie idealnego kwadratu.

Zmiana segmentu lub piksela natychmiast przelicza siatkę i aktualizuje podgląd na mapie. Wskazówki doboru parametrów znajdziesz w rozdziale [Pojęcia podstawowe](../pojecia/#rozmiar-piksela-a-skala-mapy).

## Wynikowy rozmiar siatki segmentów

| Pole | Znaczenie |
|---|---|
| [segm] | liczba kolumn × liczba wierszy segmentów |
| [pix] | szerokość × wysokość całej mapy w pikselach (wymiar scalonego arkusza) |
| Szerokość / Wysokość siatki [km] | zasięg całej siatki w terenie |

Po zaznaczeniu obszaru w polu *Komunikaty* wyświetla się również jego powierzchnia w km².

## Zasięg XY

Grupa *Schemat siatki segmentów pobieranej mapy i zasięgu zaznaczenia* zawiera cztery pola współrzędnych granicznych:

* **x dolne i górne**: południowa i północna krawędź (X = północ),
* **y lewe i prawe**: zachodnia i wschodnia krawędź (Y = wschód).

Pola wypełniają się automatycznie przy zaznaczaniu myszą na mapie. Ręczna edycja jest dostępna po włączeniu opcji *wpisywanie zasięgu XY z klawiatury* w *Ustawieniach* (zob. [Wpisywanie współrzędnych](../obszar/wspolrzedne/)). Przy aktywnej opcji *współrzędne zaznaczenia w WGS84* obok pól widać wartości w stopniach geograficznych.

Przycisk z pinezką (*pokaż na mapie siatkę segmentów…*) centruje mapę na zasięgu wpisanym w polach XY i rysuje narożniki siatki.

## Przyciski

| Przycisk | Działanie |
|---|---|
| **Pobierz** | rozpoczyna pobieranie segmentów (zob. [Przebieg pobierania](../pobieranie/przebieg/)) |
| **Przerwij pobieranie** | zatrzymuje pobieranie. Już pobrane pliki pozostają w folderze |
| szachownica (*złącz pobrane segmenty w jeden arkusz*) | otwiera okno [scalania segmentów](../scalanie/). Przycisk jest nieaktywny przed pierwszym pobraniem oraz gdy w sesji wystąpiły błędy i istnieje plik `error.txt` |
| żółty folder (*wyświetl folder z segmentami pobranej mapy*) | otwiera w Eksploratorze Windows bieżący katalog sesji (domyślnie `download`) |
| *usuń wszystkie pobrane mapy* | usuwa **cały folder `download`** po potwierdzeniu |

{: .uwaga }
Polecenie *Usuń wszystkie pobrane mapy* kasuje cały katalog `download` wraz z podfolderami `sesja_…`. Pliki nie trafiają do Kosza systemowego. Używaj go tylko przy chęci całkowitego wyczyszczenia dysku.

## Komunikaty i pasek stanu

Pole **Komunikaty** u dołu okna wyświetla informacje o przebiegu pracy:

* **zielony**: czynność zakończona pomyślnie (np. wczytano sesję, zakończono pobieranie),
* **czarny**: bieżący postęp,
* **czerwony**: błąd lub ostrzeżenie (np. brak warstwy, niekompletne segmenty, przerwanie pracy),
* **niebieski**: podpowiedź do wybranej warstwy,
* **pomarańczowy**: siatka zbyt gęsta, by wyświetlić wszystkie linie na mapie.

**Pasek stanu** pokazuje: procentowy postęp pobierania, format plików (np. `.jpeg`), ścieżkę bieżącej sesji oraz nazwę ostatnio zapisanego segmentu.

## Czynności zablokowane podczas pobierania

Podczas aktywnego pobierania segmentów zablokowana jest zmiana warstw, wczytywanie i zapis sesji, modyfikacja obszaru, wejście do ustawień oraz scalanie. Program zapobiega w ten sposób rozbieżnościom między plikiem `conf.txt` a pobieranymi danymi. Eksport innych, wcześniej ukończonych sesji pozostaje dostępny.

---

➡️ **Kolejny krok:** [Wybór obszaru pobierania mapy →](../obszar/)

*Zobacz też: [Pierwsza mapa krok po kroku](../pierwsze-kroki/pierwsza-mapa/) · [Ustawienia programu](../ustawienia/)*
