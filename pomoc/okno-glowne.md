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
| **1** | Pasek menu | sesja pobierania (zapis/wczytanie `conf.txt`, korytarz GPX/KML, odświeżanie), narzędzia (kreator serwerów, wyszukiwarka miejscowości i działek, konwerter Ozi, profil wysokościowy trasy, scalanie i eksport mapy, CAD, korekta obrazów), ustawienia i pomoc |
| **2** | Mapa podglądu | podkład orientacyjny (OSM / Google / Bing), zaznaczanie obszaru prawym przyciskiem myszy, siatka segmentów z uchwytami oraz przycisk lupy 🔍 (wyszukiwarka) |
| **3** | Drzewo *Wybierz mapy* | wszystkie zbiory i warstwy pogrupowane tematycznie, wyszukiwarka *szukaj warstwy…*, rozwijanie/zwijanie (`⊞`/`⊟`) oraz przycisk `[+]` (kreator serwerów) |
| **4** | *Wybrane warstwy oraz ich kolejność* | „kanapka” do 12 warstw (1 = podkład), zmiana kolejności (`▲`/`▼` lub przeciąganie), usuwanie (`✕`), ustawienie `Krycie %`, przycisk `…` (*Warstwy - zaawansowane*) oraz ikona kosza |
| **5** | Układ, segment i piksel | wybór układu współrzędnych pobierania, bok segmentu `[pix]` i rozmiar piksela `[m/pix]` (lub `[°/pix]` w WGS84) |
| **6** | *Wynikowy rozmiar siatki* | liczba kolumn × wierszy segmentów, całkowite wymiary w pikselach i kilometrach |
| **7** | Zasięg XY i pinezka | współrzędne granic zasięgu na schemacie (pola edycji po włączeniu wpisywania XY w *Ustawieniach*) oraz przycisk centrowania mapy na rzeczywistym zasięgu siatki |
| **8** | Podgląd na żywo i lupa | miniatura miksowania warstw ze środka siatki: skala 1:1, lupa ×2 i większy obszar ×½, przycisk *Zaawansowane* (okno *Warstwy - zaawansowane*) |
| **9** | Przyciski akcji | **Pobierz**, **Przerwij pobieranie**, scalanie (ikona szachownicy), otwarcie folderu sesji (żółty folder), *nowa sesja* (kartka z zielonym plusem) i *usuń pobrane mapy bieżącej sesji* |
| **10** | *Komunikaty* | komunikaty o postępie, powierzchni zaznaczenia i ewentualnych błędach (kolor oznacza wagę komunikatu) |
| **11** | Pasek stanu | postęp w procentach, format segmentów, bieżący podfolder sesji i nazwa ostatniego segmentu |

Okno dzieli się na trzy części:

* **po lewej**: mapa podglądu do zaznaczania obszaru i podglądu siatki segmentów, wraz z przyciskami powiększenia i lupą wyszukiwania,
* **po prawej**: panel *Dane źródłowe* z drzewem map, listą wybranych warstw, ustawieniami układu, segmentu i piksela, współrzędnymi zasięgu oraz panelem podglądu warstw z lupą powiększającą,
* **na dole**: komunikaty oraz pasek stanu.

Tytuł okna podaje nazwę i numer wersji programu (np. `MapoTero {{ site.wersja_programu }}`). Program uruchamia się domyślnie w pełnym oknie zmaksymalizowanym, zapewniając optymalny obszar roboczy. Pasek oddzielający mapę od panelu *Dane źródłowe* można przeciągać myszą, żeby poszerzyć listę warstw.

---

## Menu

| Menu | Polecenie | Skrót | Opis |
|---|---|---|---|
| **Sesja pobierania** | Nowa sesja | | czyści warstwy, zasięg, obszar wzdłuż trasy i widok mapy, a następne pobieranie trafi do nowego podfolderu (zob. [Sesje pobierania](../sesje/)) |
| | Zapisz ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>Z</kbd> | zapisuje parametry bieżącej sesji we wskazanym folderze. Wybrany folder staje się folderem sesji (zob. [Sesje pobierania](../sesje/)) |
| | Wczytaj ustawienia sesji (conf.txt) | <kbd>Alt</kbd>+<kbd>W</kbd> | wczytuje sesję ze wskazanego folderu (wymaga pliku `conf.txt`) |
| | Obszar pobierania wzdłuż trasy (GPX, KML, KMZ)... | | [Obszar wzdłuż trasy](../obszar/korytarz/) |
| | Usuń obszar wzdłuż trasy | | wraca do pobierania pełnego prostokąta |
| | Odśwież mapę (pobierz ponownie wszystkie segmenty) | | [Odświeżanie mapy](../pobieranie/odswiezanie/) |
| **Narzędzia** | Dodaj nowy serwer WMS / WMTS... | | [Kreator serwerów](../serwery/kreator/) |
| | Wyszukaj miejscowość lub działkę... | <kbd>Ctrl</kbd>+<kbd>F</kbd> | [Wyszukiwarka miejscowości i działek](../obszar/zaznaczanie/#wyszukiwarka-miejscowości-adresów-i-działek-katastralnych) (<kbd>Ctrl</kbd>+<kbd>D</kbd> dla działek) |
| | Konwertuj mapę OziExplorer (.map)... | | [Konwerter map OziExplorer](../narzedzia/#konwerter-map-oziexplorer-map) |
| | Profil wysokościowy trasy (GPX, KML)... | | [Profil wysokościowy trasy](../narzedzia/#profil-wysokościowy-trasy) |
| | Scalanie segmentów | <kbd>Ctrl</kbd>+<kbd>L</kbd> | okno [Eksport mapy](../eksport/) z formatem *Arkusz* - [Scalanie segmentów](../scalanie/) |
| | Eksport mapy (JNX, KMZ, MBTiles, SQLiteDB, OruxMaps, arkusz, TrekBuddy) | <kbd>Ctrl</kbd>+<kbd>E</kbd> | [Eksport mapy](../eksport/) |
| | Eksport do CAD (DXF, skrypt SCR, LISP)... | | [Eksport do CAD](../cad/) |
| | Korekta obrazów w folderze... | | [Narzędzia dodatkowe](../narzedzia/#korekta-obrazów-w-folderze) |
| **Ustawienia** | | | okno [Ustawienia](../ustawienia/) |
| **Pomoc** | Podręcznik użytkownika (online) | <kbd>F1</kbd> | otwiera ten podręcznik w przeglądarce |
| | Krótka instrukcja (bez internetu) | | skrócona instrukcja wbudowana w program |
| | Zadaj pytanie na forum (PFE) | | otwiera dział MapoTero na Pomorskim Forum Eksploracyjnym |
| | Zgłoś błąd lub pomysł (GitHub) | | otwiera stronę zgłoszeń programu na GitHubie |
| | Otwórz folder danych programu | | folder z `lastsettings.txt`, folderem `download` i `awarie.log` ([Gdzie program zapisuje dane](../pierwsze-kroki/dane-programu/)) |
| | Co nowego w programie | | lista wydań z opisem zmian |
| | Wesprzyj rozwój programu | | strona [buycoffee.to/mapy](https://buycoffee.to/mapy) – postaw kawę autorowi |
| | O programie | | wersja i autorzy; przycisk *Wesprzyj rozwój programu* |

Menu *Sesja pobierania* otwiera też <kbd>Alt</kbd>+<kbd>S</kbd>. Każde samodzielne okno narzędziowe ma ponadto w dolnej części przycisk **Pomoc** (oraz klawisz <kbd>F1</kbd>), otwierający dedykowane okno instrukcji z bezpośrednim odnośnikiem do odpowiedniego rozdziału podręcznika online.

---

## Mapa podglądu

Mapa służy do wyboru obszaru i pokazuje siatkę segmentów. Nie jest to pobierana mapa, tylko podkład
do orientacji w terenie.

| Czynność | Jak |
|---|---|
| przesuwanie mapy | przeciągnij lewym przyciskiem myszy |
| przybliżanie i oddalanie | kółko myszy, przyciski **+** i **-** na mapie, dwuklik lewym (przybliża) lub prawym (oddala) przyciskiem |
| wyszukiwanie miejscowości / działki | przycisk z ikoną lupy 🔍 pod przyciskiem `-` (lub skróty <kbd>Ctrl</kbd>+<kbd>F</kbd> / <kbd>Ctrl</kbd>+<kbd>D</kbd>) |
| zaznaczanie obszaru | przeciągnij **prawym** przyciskiem myszy - [Zaznaczanie na mapie](../obszar/zaznaczanie/) |
| zmiana zasięgu siatki | przeciągnij uchwyt lewym przyciskiem myszy |

**Podkład mapy** wybiera się przełącznikami na mapie: *OpenStreetMap* (domyślny), *GoogleMap* albo
*SatelliteBingsMap* (zdjęcia satelitarne). Program zapamiętuje wybrany podkład do następnego uruchomienia.

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

*Szczegółowy poradnik doboru podkładów, krycia i trybów mieszania znajdziesz na stronie [Kompozycje warstw i plastyczna rzeźba](../pobieranie/kompozycje/).*

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

Pola wypełniają się automatycznie przy zaznaczaniu myszą na mapie. Ręczną edycję włącza małe pole wyboru obok pola `x` górnego w schemacie siatki (zob. [Wpisywanie współrzędnych](../obszar/wspolrzedne/)). Przy aktywnej opcji *współrzędne zaznaczenia w WGS84* obok pól widać wartości w stopniach geograficznych.

Przycisk z pinezką (*pokaż na mapie siatkę segmentów…*) centruje mapę na zasięgu wpisanym w polach XY i rysuje narożniki siatki.

## Podgląd zestawu warstw i lupa powiększająca

W prawym dolnym narożniku okna głównego (pod schematem siatki i polami współrzędnych) znajduje się **panel podglądu zestawu warstw na żywo**:

* **Próbka w czasie rzeczywistym:** silnik podglądu pobiera w tle wycinek wybranych warstw w rozmiarze piksela pobierania i nakłada je dokładnie według zadanego krycia oraz trybów mieszania (np. mnożenie, nakładka). Wycinek pochodzi ze środka siatki; przy obszarze wzdłuż trasy - z pobieranego segmentu najbliższego środkowi siatki, a bez siatki - ze środka widoku mapy (podgląd podąża za mapą).
* **Powiększenie:** przyciski `-`, `1:1` i `+` (albo kółko myszy) przełączają skalę **1:1** (piksel w piksel), **lupę ×2** i **widok ×½**. Lupa powiększa piksele pobierania, a widok ×½ pokazuje większy obszar pobrany z serwera - rozdzielczość pobierania się nie zmienia. Podpowiedź etykiety rozmiaru piksela podaje widoczny obszar w terenie.
* **Brakujące warstwy:** gdy serwer nie zwróci którejś warstwy, podgląd dostaje czerwoną ramkę, a napis stanu obok podglądu (np. *Brak 1 z 3 warstw*) wymienia je w podpowiedzi.
* **Okno *Warstwy - zaawansowane*:** pokazuje tę samą próbkę w oknie 300 px z lupą 1:1, ×2 i ×4; warstwy pobrane już przez panel nie są pobierane ponownie.
* **Przycisk *Zaawansowane*** (albo kliknięcie podglądu) otwiera okno *Warstwy - zaawansowane*.

## Przyciski

| Przycisk | Działanie |
|---|---|
| **Pobierz** | rozpoczyna pobieranie segmentów (zob. [Przebieg pobierania](../pobieranie/przebieg/)) |
| **Przerwij pobieranie** | zatrzymuje pobieranie. Już pobrane pliki pozostają w folderze |
| szachownica (*Scalanie segmentów*) | otwiera okno [Eksport mapy](../eksport/) z formatem *Arkusz* ([Scalanie segmentów](../scalanie/)). Przycisk jest nieaktywny przed pierwszym pobraniem oraz gdy w sesji wystąpiły błędy i istnieje plik `error.txt` |
| żółty folder (*Wyświetl folder z segmentami pobranej mapy*) | otwiera w Eksploratorze Windows katalog bieżącej sesji (dedykowany podfolder sesji w `download\`, np. `download\sesja_ortofotomapa_...`) |
| *nowa sesja* (kartka z zielonym plusem) | to samo co *Sesja pobierania > Nowa sesja*: czyści wybrane warstwy, zasięg, zaznaczenie i siatkę, obszar wzdłuż trasy i kontur działki, przywraca domyślny bok segmentu i piksel, zwija drzewo warstw i pokazuje całą Polskę. Pliki na dysku zostają |
| *Usuń pobrane mapy bieżącej sesji* | po potwierdzeniu przenosi do Kosza systemowego mapę bieżącej sesji |

{: .uwaga }
Polecenie *Usuń pobrane mapy bieżącej sesji* przenosi do Kosza cały podfolder sesji w `download\` (np. `download\sesja_ortofotomapa_...`). Gdy sesja leży w folderze wskazanym przez Ciebie poza `download\`, do Kosza trafiają tylko pliki leżące bezpośrednio w tym folderze, a jego podfoldery i sam folder zostają. Inne sesje nie są usuwane. Na dysku sieciowym lub pendrivie bez Kosza Windows może usunąć pliki trwale.

## Komunikaty i pasek stanu

Pole **Komunikaty** u dołu okna wyświetla informacje o przebiegu pracy:

* **zielony**: czynność zakończona pomyślnie (np. wczytano sesję, zakończono pobieranie),
* **czarny**: bieżący postęp,
* **czerwony**: błąd lub ostrzeżenie (np. brak warstwy, niekompletne segmenty, przerwanie pracy),
* **niebieski**: podpowiedź do wybranej warstwy,
* **pomarańczowy**: siatka zbyt gęsta, by wyświetlić wszystkie linie na mapie.

**Pasek stanu** pokazuje: procentowy postęp pobierania, format plików (np. `.jpeg`), ścieżkę bieżącej sesji oraz nazwę ostatnio zapisanego segmentu. Kliknięcie formatu otwiera okno [Ustawienia](../ustawienia/#sesja-pobierania), w którym go zmienisz.

## Czynności zablokowane podczas pobierania

Podczas aktywnego pobierania segmentów zablokowana jest zmiana warstw, wczytywanie i zapis sesji, modyfikacja obszaru, wejście do ustawień oraz scalanie. Program zapobiega w ten sposób rozbieżnościom między plikiem `conf.txt` a pobieranymi danymi. Eksport innych, wcześniej ukończonych sesji pozostaje dostępny.

---

➡️ **Kolejny krok:** [Wybór obszaru pobierania mapy →](../obszar/)

*Zobacz też: [Pierwsza mapa krok po kroku](../pierwsze-kroki/pierwsza-mapa/) · [Ustawienia programu](../ustawienia/)*
