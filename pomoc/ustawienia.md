---
title: Ustawienia
nav_order: 9
---

# Ustawienia
{: .no_toc }

Wszystkie opcje okna *Ustawienia*: co robi każda, jaka jest wartość domyślna i kiedy warto ją zmienić.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Ustawienia*  
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
| **3** | [Widok okna głównego](#widok-okna-głównego) | etykiety współrzędnych, wpisywanie XY z klawiatury i wyświetlanie siatki |
| **4** | [Pliki kalibracji segmentów](#pliki-kalibracji-segmentów) | dodatkowe pliki `.map`, `.kml`, world file, `.tab`, `.wld`, `.gmi` |
| **5** | [TrekBuddy i Locus Map](#trekbuddy-i-locus-map) | zapis mapy w paczce `.tar` i wybór nazwy paczki |
| **6** | [Pobieranie](#pobieranie) | liczba prób, przerwy, równoczesne pobrania i limit czasu |
| **7** | [Usługi kafelkowe](#usługi-kafelkowe) | zachowywanie kafli w `_kafle_wmts` oraz jakość JPEG składanych segmentów |
| **8** | [Nietypowe serwery WMS](#nietypowe-serwery-wms) | zamiana osi X i Y w zapytaniach `BBOX` |

Okienko grupuje zarówno parametry zapisywane dla konkretnej sesji pobierania (`conf.txt`), jak i ogólne opcje pracy programu (`lastsettings.txt`).

* **OK**: zatwierdza przedrostek nazw segmentów, stosuje zmiany od razu i zapisuje je w `lastsettings.txt`.
* **Anuluj** (lub zamknięcie okna krzyżykiem): odrzuca wszystkie zmiany wprowadzone od otwarcia okna.
* **Przywróć domyślne...**: po potwierdzeniu przywraca ustawienia fabryczne (widok mapy w oknie głównym, domyślny zbiór *skany_map_topograficznych*, bok `2000 px`, piksel `2` oraz układ `PL-1992`). Tej operacji nie można cofnąć przyciskiem **Anuluj**.

{: .informacja }
W trakcie pobierania okno *Ustawienia* jest zablokowane, aby nie zmienić formatu ani numeracji w trakcie trwania sesji.

---

## Sesja pobierania

Te ustawienia opisują sposób zapisu segmentów bieżącej sesji i trafiają do jej pliku `conf.txt`:

| Opcja | Domyślnie | Zakres / wartości | Co robi i kiedy zmienić |
|---|---|---|---|
| *Format segmentów* | `jpeg` | `jpeg`, `png`, `png8`, `png24`, `png32`, `gif`, `tiff` | Format zapisywanych plików. Wybierz `png` lub `png32`, gdy potrzebujesz przezroczystego tła albo bezstratnej jakości (np. granice działek). |
| *przezroczyste tło* | wyłączone | PNG / GIF | Przesyła do serwera parametr `TRANSPARENT=TRUE` i zapisuje przezroczysty kanał alfa. Aktywne tylko dla formatów `png*` i `gif`. |
| *Przedrostek nazw segmentów* | `_` | do 50 znaków (może być pusty) | Początek nazwy każdego pliku segmentu (np. `_01_02.jpg`). Niedozwolone są znaki systemowe (`\ / : * ? " < > |`). |
| *Numeracja segmentów* | `NrWiersza_NrKolumny` | `NrWiersza_NrKolumny`, `01_02_03`, `1_2_3` | Styl numerowania segmentów w nazwach plików. Szczegóły i przykłady opisuje rozdział [Sesje pobierania](../sesje/#nazwy-segmentów). |
| *przy ponownym pobieraniu pobieraj tylko segmenty o numerze wyższym niż ostatni pobrany* | wyłączone | brak | Pomija wcześniejsze luki w numeracji przy wznawianiu pobierania (np. po usunięciu pustych segmentów na brzegach). Opcja nie działa w trybie TrekBuddy. |

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
| *wpisywanie zasięgu XY z klawiatury* | wyłączone | Odblokowuje ręczną edycję pól `x` / `y` w oknie głównym. Jednocześnie wyłącza zaznaczanie prawym przyciskiem myszy i uchwyty na mapie ([Wpisywanie współrzędnych](../obszar/wspolrzedne/)). |
| *siatka segmentów na mapie* | włączone | Rysuje niebieską siatkę poszczególnych segmentów na mapie (do 3000 segmentów). Po wyłączeniu widać same znaczniki narożników. |

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

W trybie TrekBuddy wyłączone są indywidualne pliki kalibracji segmentów, zmiana stylu numeracji oraz opcja
*pobieraj tylko segmenty o numerze wyższym…*.

---

## Pobieranie

| Opcja | Domyślnie | Zakres | Co robi i kiedy zmienić |
|---|---|---|---|
| *Liczba prób pobrania* | `3` | `1–20` | Łączna liczba rund pobierania. W każdej kolejnej rundzie program ponawia tylko brakujące segmenty. Zwiększ przy niestabilnym łączu lub wolnym serwerze. |
| *Przerwa między próbami [s]* | `5` | `0–300` | Czas oczekiwania między kolejnymi rundami prób. Przy błędach przeciążenia serwera warto wydłużyć np. do `30–60 s`. |
| *Równoczesne pobrania* | `4` | `1–16` | Maksymalna liczba jednoczesnych zapytań do jednego serwera (obejmuje też pojedyncze kafle WMTS/XYZ/TMS). Dla wolnych serwerów (np. map historycznych IHPAN) zmniejsz do `1–2`. |
| *Limit czasu odpowiedzi [s]* | `120` | `10–600` (krok 10) | Maksymalny czas oczekiwania na odpowiedź na pojedyncze zapytanie. Zwiększ np. do `240–300 s`, jeśli serwer długo generuje duże segmenty. |

---

## Usługi kafelkowe

W oknie grupa nosi tytuł *Usługi kafelkowe (WMTS / XYZ)* i dotyczy wszystkich usług kafelkowych (WMTS, XYZ i TMS):

| Opcja | Domyślnie | Zakres | Co robi |
|---|---|---|---|
| *zachowuj pobrane kafle w folderze sesji* | wyłączone | brak | Pozostawia podfolder `_kafle_wmts` po zakończeniu pobierania. Ułatwia powtórne złożenie tego samego obszaru z innym bokiem segmentu bez ponownego pobierania z sieci, ale zajmuje miejsce na dysku. |
| *Jakość JPEG segmentów* | `90` | `50–100` | Jakość kompresji segmentów JPEG składanych przez program z pobranych kafli. Nie dotyczy pojedynczego serwera WMS o pełnym kryciu. |

---

## Nietypowe serwery WMS

| Opcja | Domyślnie | Co robi i kiedy użyć |
|---|---|---|
| *zamień kolejność X i Y w zapytaniu* | wyłączone | Zamienia kolejność osi wyłącznie w parametrze `BBOX` zapytania WMS (nie zmienia współrzędnych w plikach kalibracji). Włącz tylko dla zewnętrznego serwera niezgodnego ze standardem WMS 1.3.0, który zwraca puste obrazy lub przesunięty teren. Dla Geoportalu musi pozostać **wyłączone**. |

---

➡️ **Kolejny krok:** [Pliki kalibracji segmentów i georeferencja →](../pliki-kalibracji/)

*Zobacz też: [Okno główne](../okno-glowne/) · [Ochrona serwerów map](../pobieranie/serwery/)*
