---
title: Ochrona serwerów map
nav_order: 4
parent: Pobieranie map
---

# Ochrona serwerów map
{: .no_toc }

Zbyt intensywne pobieranie może doprowadzić do czasowego zablokowania adresu IP przez dostawcę map. Odpowiednie limity zapytań zapobiegają błędom połączenia i odrzucaniu żądań.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Liczba jednoczesnych zapytań

Wartość *Równoczesne pobrania* (okno *Ustawienia*, grupa *Pobieranie*, od 1 do 16, domyślnie **4**) określa liczbę równoległych zapytań wysyłanych do pojedynczego serwera. Limit obejmuje wszystkie żądania w tym samym czasie, w tym pojedyncze kafle WMTS potrzebne do złożenia segmentu.

* Większa liczba wątków przyspiesza pobieranie tylko do pewnej granicy, po której serwer zaczyna zwalniać lub zwracać błędy.
* Dla serwerów Geoportalu wartość domyślna 4 stanowi optymalny kompromis.
* Dla wolnych serwerów (np. map historycznych) warto zmniejszyć liczbę wątków do 1 lub 2 i wydłużyć *Limit czasu odpowiedzi*.

---

## Automatyczne spowalnianie pobierania

Gdy serwer zwraca kody błędów przeciążenia HTTP **429** (*Too Many Requests*), **502**, **503**, **504** albo dochodzi do serii przekroczeń czasu połączenia, program automatycznie ogranicza tempo pracy:

1. Wstrzymuje kolejne zapytania na czas podany w nagłówku *Retry-After* lub wprowadza stopniowane pauzy (od 5 do 60 sekund).
2. Zmniejsza liczbę równoczesnych wątków o połowę.
3. Po ustabilizowaniu odpowiedzi stopniowo wraca do wyjściowej liczby połączeń z ustawień.

W polu *Komunikaty* widać informację o wstrzymaniu zapytań. Pobieranie jest kontynuowane automatycznie, a nieudane segmenty program ponawia w kolejnej rundzie.

---

## Dobre praktyki

* **Unikaj maksymalnej liczby wątków.** Ustawienie 16 pobrań rzadko przyspiesza pracę w porównaniu z wartościami 4–8, a znacznie częściej skutkuje blokadą IP.
* **Dostosuj obszar.** Wybór rozsądnego piksela i zawężenie zasięgu (lub [obszar wzdłuż trasy](../../obszar/korytarz/)) zmniejsza liczbę zapytań do serwera.
* **Duże pobierania dziel na części.** Zadania obejmujące całe regiony warto pobierać etapami.
* **Zachowuj kafle WMTS.** Gdy planujesz ponowne składanie tego samego obszaru z innym bokiem segmentu, włącz zachowywanie kafli w sesji ([Pobieranie z usług WMTS](../wmts/#zachowywanie-pobranych-kafli)).
* **Odświeżaj mapy tylko w razie potrzeby.** Funkcja [odświeżania](../odswiezanie/) pobiera wszystkie segmenty od nowa.

---

## Warunki korzystania z usług

Mapy pochodzą z publicznych rejestrów i serwisów internetowych podlegających licencjom ich dysponentów. Przed publikacją lub komercyjnym wykorzystaniem materiałów sprawdź warunki danej usługi i podaj źródło danych.

Program zapisuje w pliku `conf.txt` adres serwera oraz datę pobrania, a przy eksporcie automatycznie dołącza opis ze źródłem danych (atrybucja w MBTiles, opis w KMZ, metadane w rysunku CAD).

---

➡️ **Kolejny krok:** [Rozwiązywanie problemów i błędów pobierania →](../bledy/)

*Zobacz też: [Ustawienia pobierania](../../ustawienia/#pobieranie) · [Katalog zbiorów map](../../zbiory-map/)*
