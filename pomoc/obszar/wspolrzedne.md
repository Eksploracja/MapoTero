---
title: Wpisywanie współrzędnych
nav_order: 2
parent: Wybór obszaru
description: "Dokładny zasięg można wpisać z klawiatury, np. narożniki arkusza mapy, działki albo obszaru opracowania."
---

# Wpisywanie współrzędnych
{: .no_toc }

Dokładny zasięg można wpisać z klawiatury, np. narożniki arkusza mapy, działki albo obszaru opracowania.
{: .fs-5 .fw-300 }

**Gdzie w programie:** karta *Obszar pobierania > Zmień ▾ > Wpisz współrzędne…* lub menu *Sesja pobierania*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Okno precyzyjnego wpisywania współrzędnych

Najwygodniejszym sposobem wpisania dokładnych granic obszaru jest dedykowane okno współrzędnych:

1. W karcie **Obszar pobierania** pod mapą kliknij odnośnik **Zmień ▾** i wybierz **Wpisz współrzędne…** (polecenie dostępne także w menu *Sesja pobierania*).
2. W oknie wpisz wartości krawędzi w wybranym układzie:
   * **Południe (X dolny / S)** i **Północ (X górny / N)**,
   * **Zachód (Y lewy / W)** i **Wschód (Y prawy / E)**.
3. Dla układów w metrach (PL-1992, PL-2000, UTM) możesz użyć przycisku **Zaokrąglij do km**. Program rozszerzy krawędzie na zewnątrz do pełnych kilometrów, co ułatwia wpasowanie siatki w równe moduły kilometrowe.
4. Kliknij **Zastosuj**.

Program wyznaczy siatkę segmentów, zaktualizuje kartę *Obszar pobierania* i wycentruje widok mapy na podanym obszarze.

---

## Wklejanie współrzędnych w wyszukiwarce

Współrzędne pojedynczego punktu możesz także wkleić wprost do pola **Szukaj miejsca lub działki…** na górnym pasku narzędzi (<kbd>Ctrl</kbd>+<kbd>F</kbd>):
* stopnie dziesiętne WGS84: np. `50.0614, 19.9366`,
* stopnie, minuty i sekundy: np. `50°03'41"N 19°56'12"E`,
* współrzędne płaskie X, Y w metrach (np. z operatu geodezyjnego w układzie PL-1992 lub PL-2000).

Po naciśnięciu <kbd>Enter</kbd> mapa natychmiast przesuwa się w zadane miejsce.

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

Liczby dziesiętne można wpisywać z kropką lub przecinkiem. Po kliknięciu **Zastosuj** program przelicza rozmiar siatki i przybliża widok na wyznaczony obszar. Do siatki w dowolnym momencie wrócisz także przez *Obszar pobierania > Zmień ▾ > Pokaż na mapie*.

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
