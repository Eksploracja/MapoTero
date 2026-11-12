---
title: Błędy pobierania
nav_order: 5
parent: Pobieranie map
---

# Błędy pobierania
{: .no_toc }

Co oznaczają nieudane, puste lub białe segmenty i jak je naprawić.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najpierw: kliknij Pobierz jeszcze raz

Większość błędów jest chwilowa: serwer był przeciążony, połączenie się zerwało. Po komunikacie
*Nie udało się ściągnąć wszystkich segmentów…* zwykle wystarczy po chwili kliknąć **Pobierz** jeszcze raz.
Program pobierze tylko brakujące segmenty.

---

## Plik error.txt

Gdy brakuje segmentów, w folderze sesji powstaje plik `error.txt`. Dla każdego brakującego segmentu zawiera:

```
_03_07
https://…adres zapytania do serwera…
przyczyna: HTTP 503 Service Unavailable: …
```

* pierwsza linia: nazwa segmentu,
* druga linia: pełny adres zapytania (możesz wkleić go do przeglądarki, by sprawdzić odpowiedź serwera),
* trzecia linia: przyczyna błędu, jeśli została zwrócona przez serwer.

Najczęstsze przyczyny:

| Przyczyna w error.txt | Znaczenie | Co zrobić |
|---|---|---|
| `HTTP 429`, `HTTP 503`, `HTTP 502`, `HTTP 504` | serwer przeciążony lub chwilowo niedostępny | odczekaj chwilę i ponów próbę. Zmniejsz *Równoczesne pobrania* |
| `przekroczono czas oczekiwania na odpowiedź serwera (120 s)` | serwer nie odpowiedział w zadanym czasie | zwiększ *Limit czasu odpowiedzi* (np. do 300 s) lub zmniejsz bok segmentu |
| `serwer nie zwrócił obrazka (text/xml…): …ServiceException…` | serwer odrzucił zapytanie z błędem XML | przeczytaj treść błędu (często nieobsługiwany układ, zbyt duży bok obrazu lub niewłaściwa nazwa warstwy) |
| `HTTP 403` (odmowa dostępu) | serwer wymaga nagłówka `referer` lub klucza API | pobieranie zatrzymuje się od razu. Ustaw pole `referer` w pliku JSON lub skontaktuj się z dostawcą mapy |
| `HTTP 404`, `HTTP 400` | błędny adres, parametry lub brak kafelka | sprawdź definicję zbioru. W podkładach XYZ/TMS brakujący kafel `404` program zastępuje automatycznie poziomem wyżej |
| Błąd SSL / certyfikatu | wygasły lub niezaufany certyfikat SSL na starszym serwerze lokalnym | włącz *Ignoruj błędy certyfikatów SSL* w [kreatorze serwerów](../../serwery/kreator/) lub wpisz `"ignorujSsl": true` w pliku zbioru JSON |
| `przerwano pobieranie` | pobieranie przerwano przyciskiem lub zamknięciem programu | kliknij *Pobierz* |
| błąd zapisu pliku | brak miejsca na dysku albo brak uprawnień | zwolnij miejsce na dysku lub wybierz inny folder zapisu |

{: .informacja }
Program automatycznie maskuje w pliku `error.txt` oraz logach awarii wrażliwe parametry (takie jak klucze API, tokeny w adresach URL czy dane autoryzacji), zastępując je ciągiem `***`. Przesłanie pliku `error.txt` na forum pomocy nie ujawnia prywatnych kluczy dostępowych.

{: .informacja }
Plik `error.txt` istnieje również w trakcie trwania pobierania i znika automatycznie po zapisaniu kompletu segmentów. Dopóki plik znajduje się w folderze, okno scalania blokuje łączenie niekompletnego arkusza, a moduł eksportu ostrzega o brakujących kaflach. Jeśli brak segmentów jest zamierzony (np. po usunięciu pustych krawędzi), usuń `error.txt` ręcznie.

---

## Ponawianie prób

Program automatycznie ponawia nieudane segmenty w ramach jednej sesji pobierania. Trzy parametry w oknie *Ustawienia* (grupa *Pobieranie*):

| Ustawienie | Domyślnie | Zakres | Działanie |
|---|---|---|---|
| *Liczba prób pobrania* | 3 | 1–20 | łączna liczba rund pobierania (każda kolejna dotyczy tylko brakujących plików) |
| *Przerwa między próbami [s]* | 5 | 0–300 | czas oczekiwania przed kolejną rundą |
| *Limit czasu odpowiedzi [s]* | 120 | 10–600 | maksymalny czas oczekiwania na odpowiedź na pojedyncze zapytanie |

Przy niestabilnym serwerze pomaga ustawienie większej liczby prób z dłuższą przerwą (np. 5 prób co 60 s).

---

## Puste i białe segmenty

Segment pobrany bez błędów sieciowych, ale pozbawiony treści graficznej, wynika zazwyczaj z konfiguracji usługi po stronie serwera:

* **Obszar poza zasięgiem danych.** Mapy historyczne obejmują określone regiony, a krajowe kafelki XYZ (np. *OSMapa.pl*) obejmują terytorium Polski. Poza zasięgiem serwer zwraca biały lub przezroczysty obraz (dla wybranych zbiorów XYZ program wyświetla ostrzeżenie przed startem pobierania).
* **Warstwa niewidoczna w danej skali.** Część warstw WMS (np. ewidencja gruntów, uzbrojenie) uaktywnia się dopiero przy dużym powiększeniu. Przy zbyt dużym pikselu serwer zwraca pusty obraz. Zmniejsz piksel do wartości zalecanej (wskazuje ją [kreator serwerów](../../serwery/kreator/)).
* **Układ nieobsługiwany przez serwer.** Część serwerów zamiast błędu zwraca pusty obraz. Wróć wtedy do układu PL-1992.
* **Zamieniona kolejność osi.** Serwery niezgodne ze standardem WMS 1.3.0 interpretują osie odwrotnie i generują przesunięty, często pusty wycinek. W takim wypadku pomaga opcja *zamień kolejność X i Y w zapytaniu* w oknie *Ustawienia* (grupa *Nietypowe serwery WMS*). Dla Geoportalu opcja musi pozostać wyłączona.

---

## Usuwanie pustych segmentów

Puste segmenty z krawędzi mapy zajmują miejsce na dysku i spowalniają późniejszy eksport. Narzędzie *Narzędzia > Usuwanie pustych segmentów* kasuje pliki mniejsze od zadanego progu bajtów (zob. [Narzędzia dodatkowe](../../narzedzia/)).
Aby przy kolejnym kliknięciu *Pobierz* program nie pobierał ich ponownie, włącz opcję *przy ponownym pobieraniu pobieraj tylko segmenty o numerze wyższym niż ostatni pobrany* ([Wznawianie](../przebieg/#wznawianie)).

---

➡️ **Kolejny krok:** [Georeferencja i pliki kalibracji segmentów →](../../pliki-kalibracji/)

*Zobacz też: [Ochrona serwerów map](../serwery/) · [Problemy i najczęstsze pytania](../../problemy/)*
