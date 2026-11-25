---
title: Ustawienia
nav_order: 9
description: "Wszystkie opcje okna Ustawienia: co robi każda, jaka jest wartość domyślna i kiedy warto ją zmienić."
---

# Ustawienia
{: .no_toc }

Wszystkie opcje okna *Ustawienia*: co robi każda, jaka jest wartość domyślna i kiedy warto ją zmienić.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Ustawienia*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Jak działa okno

![Okno Ustawienia z ponumerowanymi grupami opcji]({{ '/pomoc/obrazy/07-ustawienia.png' | relative_url }}){: loading="lazy" width="782" height="458"}

*Rysunek 1. Okno Ustawienia programu MapoTero z ponumerowanymi grupami opcji.*

| Nr | Sekcja okna | Zawartość |
|---|---|---|
| **1** | [Sesja pobierania](#sesja-pobierania) | format, przezroczyste tło, przedrostek i numeracja segmentów (`conf.txt`) |
| **2** | [Nowa sesja](#nowa-sesja) | domyślny układ współrzędnych dla czystego startu programu (`lastsettings.txt`) |
| **3** | [Widok okna głównego](#widok-okna-głównego) | etykiety współrzędnych i wyświetlanie siatki segmentów na mapie |
| **4** | [Pliki kalibracji segmentów](#pliki-kalibracji-segmentów) | dodatkowe pliki georeferencji: `.map`, `.kml`, world file + `.prj`, `.tab`, `.wld` + `.points`, `.gmi` |
| **5** | [Pobieranie](#pobieranie) | liczba prób, przerwy, równoczesne pobrania i limit czasu odpowiedzi |
| **6** | [Usługi kafelkowe](#usługi-kafelkowe) | zachowywanie kafli w podfolderze `_kafle_wmts` i jakość JPEG przy składaniu |

Okienko grupuje zarówno parametry zapisywane dla konkretnej sesji pobierania (`conf.txt`), jak i ogólne opcje pracy programu (`lastsettings.txt`).

* **OK**: zatwierdza parametry, stosuje zmiany od razu i zapisuje je w plikach konfiguracyjnych.
* **Anuluj** (lub naciśnięcie klawisza <kbd>Esc</kbd>): odrzuca wszystkie zmiany wprowadzone od otwarcia okna.
* **Przywróć domyślne...**: po potwierdzeniu przywraca ustawienia fabryczne (widok mapy, domyślny zbiór *skany_map_topograficznych*, bok `2000 px`, piksel `2,00 m/pix` oraz układ `PL-1992`). Tej operacji nie można cofnąć przyciskiem **Anuluj**.
* **Pomoc** (oraz klawisz <kbd>F1</kbd>): otwiera okno podręcznej pomocy ze zwięzłym opisem wszystkich sekcji i bezpośrednim odnośnikiem do tego rozdziału.

{: .informacja }
W trakcie pobierania okno *Ustawienia* pozwala zmienić tylko opcje widoku współrzędnych oraz domyślny układ nowej sesji. Pozostałe opcje są zablokowane, aby nie zmienić formatu, numeracji ani siatki w trakcie trwania aktywnej sesji.

---

## Sesja pobierania

Te ustawienia opisują sposób zapisu segmentów bieżącej sesji i trafiają do jej pliku `conf.txt`:

| Opcja | Domyślnie | Zakres / wartości | Co robi i kiedy zmienić |
|---|---|---|---|
| *Format segmentów* | `jpeg` | `jpeg`, `png`, `png8`, `png24`, `png32`, `gif`, `tiff` | Format zapisywanych plików segmentów. Wybierz `png` lub `png32`, gdy potrzebujesz przezroczystego tła albo bezstratnej jakości (np. granice działek lub szlaki). |
| *przezroczyste tło (tylko PNG i GIF)* | wyłączone | PNG / GIF | Przesyła do serwera parametr `TRANSPARENT=TRUE` i zapisuje przezroczysty kanał alfa. Aktywne tylko dla formatów `png*` i `gif`. |
| *Przedrostek nazw segmentów* | `_` | do 50 znaków (może być pusty) | Początek nazwy każdego pliku segmentu (np. `_01_02.jpg`). Niedozwolone są znaki systemowe (`\ / : * ? " < > |`). |
| *Numeracja segmentów* | `NrWiersza_NrKolumny` | `NrWiersza_NrKolumny`, `01_02_03`, `1_2_3` | Styl numerowania segmentów w nazwach plików. Szczegóły i przykłady opisuje rozdział [Sesje pobierania](../sesje/#nazwy-segmentów). |

---

## Nowa sesja

| Opcja | Domyślnie | Zakres / wartości | Co robi |
|---|---|---|---|
| *Domyślny układ współrzędnych* | `PL-1992` | `PL-1992`, `PL-2000` (5–8), `UTM` (33N–35N), `WGS84` | Układ wybierany automatycznie przy starcie czystego programu oraz po kliknięciu ikony nowej sesji. |

---

## Widok okna głównego

| Opcja | Domyślnie | Co robi |
|---|---|---|
| *współrzędne kursora i środka mapy* | włączone | Pokazuje na pasku stanu bieżącą pozycję kursora w wybranym układzie sesji (`x=` północ, `y=` wschód). |
| *współrzędne kursora w WGS84* | wyłączone | Wyświetla dodatkowo stopnie geograficzne bezpośrednio przy wskaźniku myszy. |
| *współrzędne zaznaczenia w WGS84* | wyłączone | Pokazuje granice zaznaczonego obszaru w stopniach WGS84. |
| *siatka segmentów na mapie* | włączone | Rysuje niebieską siatkę poszczególnych segmentów na mapie (do 3000 segmentów). Po wyłączeniu widać same znaczniki narożników. |

---

## Pliki kalibracji segmentów

Domyślnie **wszystkie pola są wyłączone**, ponieważ wbudowane scalanie i eksport korzystają bezpośrednio z pliku `conf.txt`.
Włącz wybrane formaty tylko wtedy, gdy chcesz otwierać pojedyncze, niescalone segmenty w zewnętrznym oprogramowaniu (QGIS, ArcGIS,
OziExplorer, MapInfo, Google Earth, GPS Tuner). Opis każdego formatu znajdziesz w rozdziale [Pliki kalibracji](../pliki-kalibracji/).

---

## Pobieranie

| Opcja | Domyślnie | Zakres | Co robi i kiedy zmienić |
|---|---|---|---|
| *Liczba prób pobrania* | `3` | `1–20` | Łączna liczba rund pobierania. W każdej kolejnej rundzie program ponawia tylko brakujące segmenty. Zwiększ przy niestabilnym łączu lub wolnym serwerze. |
| *Przerwa między próbami [s]* | `5` | `0–300` | Czas oczekiwania między kolejnymi rundami prób. Przy błędach przeciążenia serwera (HTTP 429/503) warto wydłużyć np. do `30–60 s`. |
| *Równoczesne pobrania* | `4` | `1–16` | Maksymalna liczba jednoczesnych zapytań do jednego serwera (obejmuje też pojedyncze kafle WMTS/XYZ/TMS). Dla wolnych serwerów (np. map historycznych IHPAN) zmniejsz do `1–2`. |
| *Limit czasu odpowiedzi [s]* | `120` | `10–600` (krok 10) | Maksymalny czas oczekiwania na odpowiedź na pojedyncze zapytanie. Zwiększ np. do `240–300 s`, jeśli serwer długo generuje duże segmenty. |
| *Jakość JPEG przy składaniu* | `90` | `50–100` | Jakość kompresji segmentów JPEG, które program składa z kilku obrazów (z kafli WMTS/XYZ/TMS, z kilku serwerów WMS albo z warstw o kryciu poniżej 100% lub innym trybem mieszania). Segment z jednego zapytania WMS zapisuje bez ponownej kompresji. |

---

## Usługi kafelkowe

Dotyczy usług kafelkowych (WMTS, XYZ i TMS):

| Opcja | Domyślnie | Zakres | Co robi |
|---|---|---|---|
| *zachowuj pobrane kafle w folderze sesji* | wyłączone | brak | Pozostawia podfolder `_kafle_wmts` po zakończeniu pobierania. Ułatwia powtórne złożenie tego samego obszaru z innym bokiem segmentu bez ponownego pobierania z sieci, ale zajmuje miejsce na dysku. |

---

## Przeniesione i usunięte opcje

* **Ignoruj błędy certyfikatu SSL** (dla starszych serwerów gminnych i uczelnianych z samopodpisanymi lub niekompletnymi certyfikatami): opcja została przeniesiona do [kreatora dodawania serwerów](../serwery/kreator/), gdzie ustawia się ją osobno dla konkretnej usługi.
* **Tryb TrekBuddy**: paczki `.tar` generuje bezpośrednio okno [Eksport mapy](../eksport/aplikacje/#trekbuddy) z dowolnej sesji (stare sesje z trybem w `conf.txt` nadal wznawiają się poprawnie).
* **Zamień kolejność X i Y w zapytaniu**: usunięta, ponieważ żaden ze współczesnych serwerów jej nie wymaga (a pozostawiona włączona psuła pobieranie z Geoportalu).
* **Pobieraj tylko segmenty o numerze wyższym niż ostatni**: usunięta, ponieważ program automatycznie pomija istniejące już pliki na dysku i bezbłędnie uzupełnia brakujące luki.

---

➡️ **Kolejny krok:** [Pliki kalibracji segmentów i georeferencja →](../pliki-kalibracji/)

*Zobacz też: [Okno główne](../okno-glowne/) · [Ochrona serwerów map](../pobieranie/serwery/)*
