---
title: Przebieg pobierania
nav_order: 1
parent: Pobieranie map
---

# Przebieg pobierania
{: .no_toc }

Program pobiera kilka segmentów jednocześnie i zapisuje je w folderze sesji. Pobieranie można w każdej chwili
przerwać i później wznowić.
{: .fs-5 .fw-300 }

**Gdzie w programie:** przyciski *Pobierz* i *Przerwij pobieranie* w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Rozpoczęcie pobierania

Po kliknięciu **Pobierz** program najpierw sprawdza parametry. Jeśli coś się nie zgadza, pobieranie nie rusza,
a czerwony komunikat mówi, co poprawić:

| Komunikat | Co zrobić |
|---|---|
| *Nie wybrano żadnej warstwy…* | zaznacz warstwę w drzewie *Wybierz mapy* |
| *Nie podano rozmiaru pojedynczego piksela…* / *…musi być większy od zera* | wpisz piksel, np. `1` |
| *Długość boku segmentu nie może przekraczać 2048 px.* | zmniejsz *Segment [pix]* |
| *Niepoprawne współrzędne X, Y.* | zaznacz obszar albo popraw pola XY (dolne muszą być mniejsze od górnych) |
| *Zbyt wiele segmentów do pobrania…* | zmniejsz obszar albo zwiększ piksel lub segment (limit 10 mln) |
| *Trasa nie przecina obszaru pobierania…* | wybierz trasę ponownie albo usuń obszar wzdłuż trasy |
| błąd przedrostka nazw segmentów | popraw przedrostek w oknie *Ustawienia* (bez znaków niedozwolonych w nazwach plików) |

Potem program sprawdza folder sesji. Jeśli są w nim segmenty **innej** sesji (inny zasięg, segment, układ, serwer
lub warstwy albo pliki bez `conf.txt`), pyta, co zrobić:

* **Tak**: nowa sesja zostanie pobrana do nowego folderu `download\sesja_RRRR-MM-DD_GGMMSS\`.
* **Nie**: pobieranie zostaje anulowane. Aby wznowić poprzednią sesję, wczytaj ją z menu *Sesja pobierania*.

Zapobiega to przypadkowemu uznaniu starych segmentów o tych samych nazwach za pobrane części nowej mapy.

Na koniec program zapisuje w folderze sesji `conf.txt` (z bieżącą datą) oraz ewentualny plik `korytarz.txt`, po czym rozpoczyna pobieranie.

---

## Postęp i komunikaty

* Segmenty pobierane są **wierszami od góry**, kilka naraz (liczba z ustawienia *Równoczesne pobrania*, domyślnie 4).
* Pasek stanu pokazuje postęp w procentach i nazwę ostatniego segmentu.
* Komunikat podaje liczbę segmentów do pobrania. Gdy serwer jest przeciążony, program informuje o przerwie
  i zmniejszeniu tempa ([Ochrona serwerów map](../serwery/)).
* Okno programu przez cały czas reaguje: można przesuwać mapę albo eksportować inne, już pobrane mapy.

Jeśli wszystkie warstwy pochodzą z jednego serwera WMS i mają krycie 100%, każdy segment zapisywany jest dokładnie
tak, jak przysłał go serwer, bez ponownej kompresji. W przypadku usług kafelkowych (WMTS / XYZ / TMS), warstw
z kilku serwerów lub krycia poniżej 100% program pobiera składniki osobno i składa gotowy segment. Obok segmentów
powstają pliki kalibracji wybrane w ustawieniach (domyślnie żadne).

## Zakończenie

Komunikat końcowy:

| Komunikat | Znaczenie |
|---|---|
| zielony *Zakończono pobieranie. Mapa znajduje się w katalogu…* | wszystkie segmenty są w folderze |
| czerwony *Nie udało się ściągnąć wszystkich segmentów (n z N). Wykaz… w pliku error.txt* | części segmentów brakuje (zob. [Błędy pobierania](../bledy/)) |
| czerwony *Przerwano pobieranie segmentów* | pobieranie przerwano przyciskiem |

Program automatycznie ponawia nieudane segmenty: po każdej rundzie czeka czas określony w polu *Przerwa między próbami* i pobiera brakujące pliki jeszcze raz, łącznie tyle razy, ile wynosi *Liczba prób pobrania* (domyślnie 3 próby co 5 s). Komunikat *Kolejna próba (2 z 3) za 5 s* informuje o tym na bieżąco.

---

## Przerywanie

Przycisk **Przerwij pobieranie** zatrzymuje zadanie. Segmenty już zapisane na dysku zostają zachowane, a plik pobierany w danym momencie zostaje usunięty, aby nie pozostawić uszkodzonego obrazu. W folderze powstaje `error.txt` z wykazem brakujących segmentów i przyczyną *przerwano pobieranie*.

Pobieranie przerywa też zamknięcie programu. Brakujące segmenty można dopobrać później (niżej).

---

## Wznawianie

Aby dokończyć przerwane lub niepełne pobieranie:

1. Jeśli program był zamknięty, uruchom go. Sesja z folderu `download` wczyta się sama. Sesję z innego folderu
   wczytaj z menu *Sesja pobierania > wczytaj ustawienia sesji (conf.txt)*.
2. Nie zmieniaj zasięgu, segmentu, piksela, układu ani warstw.
3. Kliknij **Pobierz**.

Program pominie segmenty, które już są w folderze, i pobierze tylko brakujące, także te z luk w środku mapy.
Pozostałości po zapisie przerwanym w połowie (pliki `.tmp`) usuwa sam.

**Opcja *przy ponownym pobieraniu pobieraj tylko segmenty o numerze wyższym niż ostatni pobrany***
(okno *Ustawienia*, grupa *Sesja pobierania*): program zaczyna od segmentu następnego po ostatnim istniejącym
i pomija luki przed nim. Przydaje się, gdy luki są zamierzone, np. po [usunięciu pustych segmentów](../../narzedzia/).
Nie działa w trybie TrekBuddy ani przy odświeżaniu mapy.

---

## Czego nie można robić w trakcie pobierania

W trakcie pobierania program blokuje zmiany, które rozjechałyby parametry sesji z pobieranymi segmentami:
zmianę zbioru map i warstw, zaznaczanie obszaru, wczytywanie i zapis sesji, okno *Ustawienia*, scalanie,
usuwanie map, dodawanie serwera, nakładanie map i usuwanie pustych segmentów. Przy próbie program wyświetla
*„… będzie możliwe po zakończeniu lub przerwaniu pobierania segmentów”*.

---

## Pliki w folderze sesji po pobraniu

| Plik | Zawartość |
|---|---|
| `_01_01.jpg`, `_01_02.jpg` … | segmenty (przedrostek, numer, rozszerzenie wg ustawień) |
| pliki kalibracji (`.jgw`, `.prj`, `.map` …) | gdy włączone w ustawieniach |
| `conf.txt` | parametry sesji |
| `koordynaty.txt` | wykaz segmentów z adresami zapytań i współrzędnymi |
| `error.txt` | tylko gdy brakuje segmentów: nazwa, adres zapytania i przyczyna dla każdego brakującego |
| `korytarz.txt` | tylko przy obszarze wzdłuż trasy |
| `_kafle_wmts\` | tylko usługi kafelkowe (WMTS / XYZ / TMS), w trakcie pobierania albo gdy włączono zachowywanie kafli |

Więcej: [Sesje pobierania](../../sesje/).

---

➡️ **Kolejny krok:** [Odświeżanie pobranej wcześniej mapy →](../odswiezanie/)

*Zobacz też: [Błędy pobierania](../bledy/) · [Usługi kafelkowe (WMTS, XYZ, TMS)](../wmts/)*
