---
title: Wpisywanie współrzędnych
nav_order: 2
parent: Wybór obszaru
---

# Wpisywanie współrzędnych
{: .no_toc }

Dokładny zasięg można wpisać z klawiatury, np. narożniki arkusza mapy, działki albo obszaru opracowania.
{: .fs-5 .fw-300 }

**Gdzie w programie:** pola zasięgu XY w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Włączenie wpisywania z klawiatury

1. Otwórz menu **Ustawienia**.
2. W grupie *Widok okna głównego* zaznacz **wpisywanie zasięgu XY z klawiatury**.
3. Kliknij **OK**.

Pola zasięgu stają się edytowalne, a zaznaczanie prawym przyciskiem myszy i uchwyty na mapie są wyłączone
(żeby przypadkowe kliknięcie nie zmieniło wpisanych wartości). Aby wrócić do zaznaczania myszą, wyłącz tę opcję.

---

## Pola XY

Współrzędne wpisuje się w **układzie pobierania** (wybranym w oknie głównym) w metrach (w układzie WGS84 w stopniach):

| Pole | Znaczenie |
|---|---|
| x dolne | południowa krawędź obszaru (najmniejsza współrzędna północna) |
| x górne | północna krawędź (największa współrzędna północna) |
| y lewe | zachodnia krawędź (najmniejsza współrzędna wschodnia) |
| y prawe | wschodnia krawędź (największa współrzędna wschodnia) |

{: .uwaga }
W programie **X oznacza północ, a Y wschód** (konwencja geodezyjna). Współrzędne z programu GIS lub CAD, gdzie
zwykle X = wschód, trzeba wpisać odwrotnie: wartość „X” z GIS do pola *y*, a „Y” do pola *x*.

Liczby dziesiętne można wpisywać z kropką lub przecinkiem. Po każdej zmianie program przelicza rozmiar siatki.
Siatkę na mapie dla wpisanego zasięgu pokazuje przycisk z pinezką (*wyświetl rzeczywisty zasięg pobieranego obszaru…*).
Program centruje wtedy mapę na siatce, a gdy siatka się nie mieści, oddala widok.

Siatka zaczyna się w punkcie (*x dolne*, *y lewe*) i obejmuje pełne segmenty w górę oraz w prawo. Żeby siatka
pokryła obszar bez naddatku, dobierz zasięg jako wielokrotność terenowego boku segmentu (bok × piksel), np. przy
segmencie 2000 px i pikselu 1 m: wielokrotność 2000 m.

---

## Przykład w układzie PL-1992

Przykładowy obszar o zasięgu 646 000–650 000 m (północ) i 286 000–292 000 m (wschód) w układzie PL-1992:

* x dolne = `646000`, x górne = `650000` (wysokość 4000 m),
* y lewe = `286000`, y prawe = `292000` (szerokość 6000 m).

Przy segmencie 2000 px i pikselu 1 m (segment = 2 km) siatka ma 3 kolumny i 2 wiersze i dokładnie pokrywa zadany prostokąt.

---

## Zmiana układu współrzędnych

Zmiana układu na liście *Układ współrzędnych pobierania* przelicza wpisany zasięg do nowego układu. Nowy zasięg
to prostokąt opisany na poprzednim, więc jest nieco większy. Dlatego najpierw wybierz układ, a potem wpisz współrzędne.

Dostępne układy i ich zastosowania: [Układy współrzędnych](../../pojecia/#układy-współrzędnych).

---

➡️ **Kolejny krok:** [Pobieranie wzdłuż trasy (korytarz) →](../korytarz/)

*Zobacz też: [Zaznaczanie na mapie](../zaznaczanie/) · [Konwencja X i Y](../../pojecia/#konwencja-x-i-y)*
