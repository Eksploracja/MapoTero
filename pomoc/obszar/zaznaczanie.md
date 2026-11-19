---
title: Zaznaczanie na mapie
nav_order: 1
parent: Wybór obszaru
---

# Zaznaczanie na mapie
{: .no_toc }

Prostokąt zaznaczony prawym przyciskiem myszy, siatka segmentów na mapie i zmiana zasięgu uchwytami.
{: .fs-5 .fw-300 }

**Gdzie w programie:** mapa podglądu w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Zaznaczanie prawym przyciskiem myszy

1. Ustaw najpierw zbiór map, warstwę, segment i piksel (od nich zależy siatka).
2. Przybliż mapę tak, by widzieć cały obszar.
3. Wciśnij **prawy** przycisk myszy w jednym narożniku obszaru, przeciągnij do przeciwległego i puść.

Program:

* wpisze zasięg w pola XY (w układzie pobierania, zaokrąglony do pełnych metrów na zewnątrz zaznaczenia),
* narysuje siatkę segmentów i uchwyty,
* wyświetli w polu *Komunikaty* powierzchnię obszaru, a w polu *Wynikowy rozmiar siatki* liczbę segmentów.

Nowe zaznaczenie zastępuje poprzednie, w tym również aktywny [obszar wzdłuż trasy](../korytarz/).

{: .informacja }
Zaznaczanie myszą jest zablokowane w trakcie aktywnego pobierania oraz gdy zaznaczone jest pole wpisywania zasięgu XY z klawiatury (obok pola `x` górnego w schemacie siatki).

Zaznaczony prostokąt jest prostokątem na mapie podglądu (w stopniach geograficznych). W płaskim układzie pobierania (np. PL-1992) tworzy lekko obrócony czworokąt, więc program wyznacza prostokąt opisany obejmujący wszystkie jego narożniki.

---

## Wyszukiwarka miejscowości, adresów i działek katastralnych

Oprócz ręcznego zaznaczania myszą możesz szybko zlokalizować interesujący punkt w Polsce i wyznaczyć obszar pobierania za pomocą wbudowanej wyszukiwarki:

* **Jak uruchomić:** kliknij przycisk z ikoną lupy 🔍 na pasku pod powiększeniem mapy (pod przyciskiem `-`), wybierz polecenie *Narzędzia > Wyszukaj miejscowość lub działkę...* albo użyj skrótów klawiaturowych:
  * <kbd>Ctrl</kbd>+<kbd>F</kbd> – otwiera wyszukiwarkę na zakładce *Miejscowość / Adres*,
  * <kbd>Ctrl</kbd>+<kbd>D</kbd> – otwiera wyszukiwarkę na zakładce *Działka katastralna (ULDK)*.

### Zakładka 1: Miejscowość / Adres

Korzysta z geokodera OpenStreetMap Nominatim. Pozwala znaleźć dowolne miasto, wieś, osadę, ulicę, szczyt górski lub konkretny adres:

1. Wpisz poszukiwaną nazwę (np. `Kraków`, `Zakopane Krupówki`, `Gdańsk Długa 1`, `Babia Góra`) i kliknij **Szukaj** (lub naciśnij <kbd>Enter</kbd>).
2. Wybierz odpowiedni wpis z listy wyników (lista podaje pełną nazwę, typ obiektu oraz współrzędne geograficzne).
3. Kliknij **Pokaż na mapie** (lub dwukrotnie kliknij wynik na liście).

Program automatycznie przesunie mapę podglądu i ustawi optymalny poziom powiększenia (dla miast i gmin dopasuje widok do całego ich obszaru).

### Zakładka 2: Działka katastralna (ULDK GUGiK)

Łączy się bezpośrednio z urzędową Usługą Lokalizacji Działek Katastralnych (ULDK) Głównego Urzędu Geodezji i Kartografii:

1. Wpisz pełny identyfikator TERYT działki (np. `022503_5.0003.134`) albo nazwę obrębu/miejscowości i numer (np. `Krzewina 134`) i kliknij **Szukaj**.
2. Wybierz działkę z listy wyników (zostanie podany numer działki, obręb, gmina i kod TERYT).
3. Ustaw **Margines wokół działki na mapie [m]** (domyślnie `50 m`) – bufor terenu wokół granic parceli, by na pobranej mapie widoczne było również najbliższe otoczenie i dojazd.
4. Opcja **Włącz warstwy: Ortofotomapa i Działki ewidencyjne (EGiB GUGiK)** automatycznie zaznacza w drzewie map fotomapę lotniczą oraz urzędowe granice i numery działek.
5. Kliknij **Zaznacz na mapie**.

Program pobierze geometrię granic działki z państwowego rejestru, wycentruje widok, narysuje żółto-czerwony kontur działki z wierzchołkami oraz automatycznie wyznaczy siatkę segmentów pobierania obejmującą działkę wraz z zadanym marginesem.

---

## Siatka segmentów na mapie

Po zaznaczeniu mapa pokazuje:

* **niebieskie kwadraty**: segmenty, które zostaną pobrane,
* **uchwyty**: kwadratowe znaczniki w narożnikach, na środkach boków i w centrum siatki.

Siatka zaczyna się w lewym dolnym narożniku zaznaczenia i obejmuje pełne segmenty, więc jej prawa i górna
krawędź wychodzą zwykle poza zaznaczenie. Najechanie kursorem na uchwyt narożnika pokazuje współrzędne tego
narożnika z uwzględnieniem pełnych segmentów (rzeczywisty zasięg pobieranej mapy).

* Przy więcej niż **3000 segmentów** program rysuje wyłącznie zewnętrzny obrys siatki i wyświetla pomarańczowy komunikat. Przyspiesza to przesuwanie mapy.
* Siatkę można wyłączyć w oknie *Ustawienia* (*siatka segmentów na mapie*). Na mapie pozostają wtedy same znaczniki narożników.
* Zmiana boku segmentu, piksela lub układu natychmiast odświeża siatkę.
* Po przesunięciu mapy przycisk z pinezką (*wyświetl rzeczywisty zasięg…*) wraca widokiem do siatki.

---

## Uchwyty

Uchwyty zmieniają zasięg o **pełne segmenty**, co pozwala łatwo dodać brakujący rząd bez ponownego zaznaczania całego obszaru.

![Siatka segmentów na mapie z uchwytami narożników, boków i środka]({{ '/pomoc/obrazy/04-uchwyty-siatki.png' | relative_url }})

*Rysunek 1. Uchwyty siatki segmentów: narożniki, środki boków oraz uchwyt centralny do przesuwania całego obszaru.*

| Uchwyt | Przeciągnięcie lewym przyciskiem myszy |
|---|---|
| narożnik | przesuwa dwie krawędzie stykające się w tym narożniku |
| środek boku | przesuwa jedną krawędź (zmienia liczbę wierszy w pionie lub kolumn w poziomie) |
| środek siatki | przesuwa całą siatkę bez zmiany jej wymiarów |

Podczas przeciągania widać zarys nowego zasięgu, a po puszczeniu przycisku myszy pojawia się przeliczona siatka. Komunikat podaje nowy zasięg i liczbę segmentów.

Uchwyty działają tylko dla prostokątnego obszaru zaznaczanego myszą. Przy obszarze wzdłuż trasy, przy wpisywaniu
XY z klawiatury i w trakcie pobierania narożniki oznaczone są zwykłymi pinezkami.

---

## Cofnięcie zmiany

Naciśnięcie <kbd>Esc</kbd> **w trakcie** przeciągania uchwytu anuluje zmianę: zasięg wraca do stanu sprzed
przeciągania. Po puszczeniu przycisku myszy zmianę cofa się, przeciągając uchwyt z powrotem albo zaznaczając
obszar od nowa.

---

➡️ **Kolejny krok:** [Wpisywanie współrzędnych z klawiatury →](../wspolrzedne/)

*Zobacz też: [Obszar wzdłuż trasy](../korytarz/) · [Jak duży obszar pobrać](../wielkosc/)*
