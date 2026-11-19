---
title: Ustawienia
nav_order: 9
---

# Ustawienia
{: .no_toc }

Wszystkie opcje okna *Ustawienia*: co robi każda, jaka jest wartość domyślna i kiedy warto ją zmienić.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Ustawienia* albo kliknięcie formatu segmentów (np. `.jpeg`) na pasku stanu u dołu okna głównego  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Jak działa okno

![Okno Ustawienia z ponumerowanymi grupami opcji]({{ '/pomoc/obrazy/07-ustawienia.png' | relative_url }})

*Rysunek 1. Okno Ustawienia z ponumerowanymi grupami opcji.*

| Nr | Sekcja okna | Zawartość |
|---|---|---|
| **1** | [Sesja pobierania](#sesja-pobierania) | format, przezroczyste tło, przedrostek i numeracja segmentów (`conf.txt`) |
| **2** | [Nowa sesja](#nowa-sesja) | domyślny układ współrzędnych dla czystego startu programu |
| **3** | [Widok okna głównego](#widok-okna-głównego) | etykiety współrzędnych i wyświetlanie siatki |
| **4** | [Pliki kalibracji segmentów](#pliki-kalibracji-segmentów) | dodatkowe pliki `.map`, `.kml`, world file, `.tab`, `.wld`, `.gmi` |
| **5** | [TrekBuddy i Locus Map](#trekbuddy-i-locus-map) | zapis mapy w paczce `.tar` i wybór nazwy paczki |
| **6** | [Pobieranie](#pobieranie) | liczba prób, przerwy, równoczesne pobrania, limit czasu i jakość JPEG składanych segmentów |
| **7** | [Usługi kafelkowe](#usługi-kafelkowe) | zachowywanie kafli w `_kafle_wmts` |

Okienko grupuje zarówno parametry zapisywane dla konkretnej sesji pobierania (`conf.txt`), jak i ogólne opcje pracy programu (`lastsettings.txt`).

* **OK**: zatwierdza przedrostek nazw segmentów, stosuje zmiany od razu i zapisuje je w `lastsettings.txt`.
* **Anuluj** (lub zamknięcie okna krzyżykiem): odrzuca wszystkie zmiany wprowadzone od otwarcia okna.
* **Przywróć domyślne...**: po potwierdzeniu przywraca ustawienia fabryczne (widok mapy w oknie głównym, domyślny zbiór *skany_map_topograficznych*, bok `2000 px`, piksel `2` oraz układ `PL-1992`). Tej operacji nie można cofnąć przyciskiem **Anuluj**.
* **Pomoc** (oraz klawisz <kbd>F1</kbd>): otwiera okno podręcznej pomocy ze zwięzłym opisem wszystkich sekcji i bezpośrednim odnośnikiem do tego rozdziału.

{: .informacja }
W trakcie pobierania okno *Ustawienia* pozwala zmienić tylko współrzędne kursora, środka mapy i zaznaczenia oraz domyślny układ nowej sesji. Pozostałe opcje są wyłączone, aby nie zmienić formatu, numeracji ani siatki w trakcie trwania sesji.

---

## Sesja pobierania

Te ustawienia opisują sposób zapisu segmentów bieżącej sesji i trafiają do jej pliku `conf.txt`:

| Opcja | Domyślnie | Zakres / wartości | Co robi i kiedy zmienić |
|---|---|---|---|
| *Format segmentów* | `jpeg` | `jpeg`, `png`, `png8`, `png24`, `png32`, `gif`, `tiff` | Format zapisywanych plików. Wybierz `png` lub `png32`, gdy potrzebujesz przezroczystego tła albo bezstratnej jakości (np. granice działek). |
| *przezroczyste tło (tylko PNG i GIF)* | wyłączone | PNG / GIF | Przesyła do serwera parametr `TRANSPARENT=TRUE` i zapisuje przezroczysty kanał alfa. Aktywne tylko dla formatów `png*` i `gif`. |
| *Przedrostek nazw segmentów* | `_` | do 50 znaków (może być pusty) | Początek nazwy każdego pliku segmentu (np. `_01_02.jpg`). Niedozwolone są znaki systemowe (`\ / : * ? " < > |`). |
| *Numeracja segmentów* | `NrWiersza_NrKolumny` | `NrWiersza_NrKolumny`, `01_02_03`, `1_2_3` | Styl numerowania segmentów w nazwach plików. Szczegóły i przykłady opisuje rozdział [Sesje pobierania](../sesje/#nazwy-segmentów). |

---

## Nowa sesja

| Opcja | Domyślnie | Zakres / wartości | Co robi |
|---|---|---|---|
| *Domyślny układ współrzędnych* | `PL-1992` | `PL-1992`, `PL-2000` (5–8), `UTM` (33N–35N), `WGS84` | Układ wybierany automatycznie przy starcie czystego programu (gdy w `download\` nie ma pliku `conf.txt`). |

---

## Widok okna głównego

| Opcja | Domyślnie | Co robi |
|---|---|---|
| *współrzędne kursora i środka mapy* | włączone | Pokazuje pod mapą bieżącą pozycję kursora w układzie pobierania oraz współrzędne WGS84 i zoom środka mapy. |
| *współrzędne kursora w WGS84* | wyłączone | Wyświetla małą etykietę z długością i szerokością geograficzną tuż przy kursorze na mapie. |
| *współrzędne zaznaczenia w WGS84* | wyłączone | Pokazuje pod polami XY granice zaznaczonego obszaru w stopniach WGS84. |
| *siatka segmentów na mapie* | włączone | Rysuje niebieską siatkę poszczególnych segmentów na mapie (do 3000 segmentów). Po wyłączeniu widać same znaczniki narożników. |

Wpisywanie zasięgu XY z klawiatury włącza małe pole wyboru obok pola `x` górnego w schemacie siatki okna głównego (zob. [Wpisywanie współrzędnych](../obszar/wspolrzedne/)).

---

## Pliki kalibracji segmentów

Domyślnie **wszystkie pola są wyłączone**, ponieważ wbudowane scalanie i eksport korzystają bezpośrednio z pliku `conf.txt`.
Włącz wybrane formaty tylko wtedy, gdy chcesz otwierać pojedyncze pobrane segmenty w zewnętrznym programie (QGIS, ArcGIS,
OziExplorer, MapInfo, Google Earth, GPS Tuner). Opis każdego formatu znajdziesz w rozdziale [Pliki kalibracji](../pliki-kalibracji/).

---

## TrekBuddy i Locus Map

| Opcja | Domyślnie | Co robi |
|---|---|---|
| *twórz mapę TrekBuddy / Locus Map (.tar)* | wyłączone | Przełącza pobieranie w tryb paczki `.tar` z segmentami `512 × 512 px` i plikami `.map` / `.set`. Po wyłączeniu wraca poprzedni bok segmentu z okna głównego. |
| *nazwa paczki (po przedrostku)* | `sam przedrostek` | Dodaje do nazwy folderu i archiwum `.tar` nazwę zbioru map, nazwę warstwy albo oba elementy. Pod spodem widać podgląd nazwy. |

W trybie TrekBuddy wyłączone są indywidualne pliki kalibracji segmentów i zmiana stylu numeracji. Tryb jest parametrem
sesji: program zapisuje go w `conf.txt`, więc po ponownym uruchomieniu albo wczytaniu sesji wraca razem z nią.
*Nowa sesja* wyłącza tryb TrekBuddy, bo przywraca domyślny bok segmentu.

---

## Pobieranie

| Opcja | Domyślnie | Zakres | Co robi i kiedy zmienić |
|---|---|---|---|
| *Liczba prób pobrania* | `3` | `1–20` | Łączna liczba rund pobierania. W każdej kolejnej rundzie program ponawia tylko brakujące segmenty. Zwiększ przy niestabilnym łączu lub wolnym serwerze. |
| *Przerwa między próbami [s]* | `5` | `0–300` | Czas oczekiwania między kolejnymi rundami prób. Przy błędach przeciążenia serwera warto wydłużyć np. do `30–60 s`. |
| *Równoczesne pobrania* | `4` | `1–16` | Maksymalna liczba jednoczesnych zapytań do jednego serwera (obejmuje też pojedyncze kafle WMTS/XYZ/TMS). Dla wolnych serwerów (np. map historycznych IHPAN) zmniejsz do `1–2`. |
| *Limit czasu odpowiedzi [s]* | `120` | `10–600` (krok 10) | Maksymalny czas oczekiwania na odpowiedź na pojedyncze zapytanie. Zwiększ np. do `240–300 s`, jeśli serwer długo generuje duże segmenty. |
| *Jakość JPEG przy składaniu* | `90` | `50–100` | Jakość kompresji segmentów JPEG, które program składa z kilku obrazów: z kafli WMTS/XYZ/TMS, z kilku serwerów WMS albo z warstw o kryciu poniżej 100 % lub z innym trybem mieszania. Segment z jednego zapytania WMS program zapisuje bez ponownej kompresji. |

---

## Usługi kafelkowe

W oknie grupa nosi tytuł *Usługi kafelkowe (WMTS / XYZ)* i dotyczy wszystkich usług kafelkowych (WMTS, XYZ i TMS):

| Opcja | Domyślnie | Zakres | Co robi |
|---|---|---|---|
| *zachowuj pobrane kafle w folderze sesji* | wyłączone | brak | Pozostawia podfolder `_kafle_wmts` po zakończeniu pobierania. Ułatwia powtórne złożenie tego samego obszaru z innym bokiem segmentu bez ponownego pobierania z sieci, ale zajmuje miejsce na dysku. |


---

➡️ **Kolejny krok:** [Pliki kalibracji segmentów i georeferencja →](../pliki-kalibracji/)

*Zobacz też: [Okno główne](../okno-glowne/) · [Ochrona serwerów map](../pobieranie/serwery/)*
