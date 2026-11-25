---
title: Kompozycje warstw i plastyczna rzeźba
nav_order: 2.5
parent: Pobieranie map
description: "Łączenie wielu warstw w jeden gotowy arkusz: plastyczne cieniowanie NMT, dynamiczna hipsometria, ortofotomapa 3D oraz granice działek katastralnych."
---

# Kompozycje warstw i plastyczna rzeźba
{: .no_toc }

Łączenie wielu warstw w jeden gotowy arkusz: plastyczne cieniowanie NMT, dynamiczna hipsometria, ortofotomapa 3D oraz granice działek katastralnych.
{: .fs-5 .fw-300 }

**Gdzie w programie:** karta *Wybrane warstwy* w prawym panelu oraz okno miksera warstw (przycisk **Mikser…** w nagłówku karty lub kliknięcie próbki w zakładce *Podgląd*)  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Zasada działania kanapki warstw

W MapoTero możesz wybrać do 12 warstw jednocześnie i złożyć z nich spójną mapę już na etapie pobierania. Warstwy układają się w stos (kanapkę) na karcie *Wybrane warstwy*:

* **Warstwa nr 1 (na samym dole):** to nieprzezroczysty podkład bazowy. Najczęściej jest to ortofotomapa, mapa topograficzna (np. BDOT10k lub skan 1:10 000) albo mapa archiwalna.
* **Warstwy wyższe (2, 3…):** nakładają się na podkład. Mogą to być przezroczyste wektory (działki ewidencyjne, szlaki turystyczne) lub rastry modelujące teren (cieniowanie rzeźby NMT, hipsometria).
* **Kolejność warstw:** zmieniasz, przeciągając pozycję myszą, skrótem <kbd>Ctrl</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd> albo z menu kontekstowego prawego przycisku myszy. Górna warstwa na liście leży na wierzchu stosu.
* **Krycie bezpośrednio na liście:** zaznaczona warstwa na liście ma pole liczbowe krycia (`%`) bezpośrednio w swoim wierszu, a krzyżyk **✕** na końcu wiersza ją usuwa.

Gdy wszystkie warstwy pochodzą z tego samego serwera WMS i mają krycie 100%, łączy je bezpośrednio serwer w jednym zapytaniu. Jeśli warstwy pochodzą z różnych źródeł (np. podkład z kafelków WMTS, a nakładka z serwera WMS) lub mają zmniejszone krycie, program pobiera każdą warstwę osobno z przezroczystym tłem i precyzyjnie nakłada je w segment wynikowy.

---

## Tryby nakładania i dobór krycia

Do dokładnego zestrojenia kompozycji służy okno **Mikser warstw**, otwierane przyciskiem **Mikser…** w nagłówku karty *Wybrane warstwy* (albo kliknięciem próbki podglądu). Znajdziesz w nim suwaki krycia dla każdej warstwy, wybór trybu mieszania, przyciski **➕ Dodaj warstwę...** oraz **⚡ Ułóż automatycznie**, podgląd na żywo na rzeczywistej próbce terenu (300 × 300 px) z lupą rastrową (`-`, `1:1`, `+` lub kółko myszy), przycisk *Korekta obrazu...* oraz podręczną instrukcję (przycisk **Pomoc** i klawisz <kbd>F1</kbd>).

Jeśli wolisz układać i sprawdzać warstwy na dużym ekranie przed dodaniem ich do programu, skorzystaj z przeglądarkowego [Miksera warstw w Galerii online](../../galeria-i-mikser/). Gotową kanapkę prześlesz jednym kliknięciem do MapoTero albo zapiszesz jako plik `.mapotero`, który wczytasz przeciągając go na okno programu.

| Tryb mieszania | Jak działa | Kiedy stosować |
|---|---|---|
| **7 Rzeźba terenu (Hillshade)** | Dedykowany tryb dla numerycznego modelu terenu: jasność 180 (płaski teren przy słońcu 45° nad horyzontem) jest neutralna. Płaski teren zachowuje w 100% oryginalne barwy, stoki w cieniu ciemnieją, a oświetlone lekko jaśnieją. | **Zalecany dla cieniowania rzeźby NMT** na ortofotomapie i mapach topograficznych. Rozwiązuje problem trybu Mnożenie, który kładł na całej mapie szarą zasłonę. |
| **1 Zwykły (Normal)** | Standardowe przenikanie przezroczystości (alfa). | Nakładki wektorowe z przezroczystym tłem (granice działek, numery, szlaki). W przypadku cieniowania NMT na ortofotomapie wymaga niskiego krycia (35–50%), inaczej tworzy szare zamglenie. |
| **2 Mnożenie (Multiply)** | Piksele warstwy górnej mnożą się z tłem. Biel staje się całkowicie niewidoczna, a cienie i ciemne linie przenikają w podkład. | Mapy topograficzne ze skanów papierowych i BDOT10k łączone z cieniowaniem NMT lub hipsometrią. Zachowuje ostrość czarnych napisów i siatki ulic. |
| **3 Ciemniej (Darken)** | Wybiera ciemniejszy kolor z obu warstw dla każdego kanału. | Nakładanie ciemnych linii, siatek lub napisów na jasny podkład bez rozjaśniania tła. |
| **4 Jaśniej (Lighten)** | Wybiera jaśniejszy kolor z obu warstw dla każdego kanału. | Wydobywanie jasnych linii, napisów lub warstwic na ciemniejszym podkładzie. |
| **5 Ekran (Screen)** | Rozjaśnia tło, czerń staje się niewidoczna (odwrotność mnożenia). | Warstwy nocne, radary lub rozświetlenia. |
| **6 Nakładka (Overlay)** | Wzmacnia kontrasty: rozjaśnia partie jasne i przyciemnia cienie, nie zamulając barw pośrednich szarością. | Plastyczna ortofotomapa 3D z cieniowaniem NMT. Pozwala uwypuklić zbocza i doliny bez efektu przymglenia zdjęcia lotniczego. |

---

## Cztery sprawdzone kompozycje

Poniższe przykłady przedstawiają przetestowane zestawienia map z Geoportalu na obszarze krawędzi Trójmiejskiego Parku Krajobrazowego w Gdyni (rejon ul. Łęczyckiej i wąwozów morenowych Małego Kacka).

### 1. Plastyczna ortofotomapa 3D

Połączenie aktualnego zdjęcia lotniczego z numerycznym modelem terenu (NMT 1 m). Cieniowanie wydobywa krawędzie wzniesień, zbocza leśne i wąwozy, które na płaskiej fotografii z góry zlewają się z otoczeniem.

![Plastyczna ortofotomapa 3D z cieniowaniem NMT]({{ '/pomoc/obrazy/kompozycja-1-plastyczna-orto.jpg' | relative_url }}){: loading="lazy" width="836" height="365"}

*Rysunek 1. Plastyczna ortofotomapa 3D: podkład lotniczy 25 cm z nałożonym cieniowaniem NMT w trybie Nakładka.*

* **Warstwa 1 (podkład):** *Ortofotomapy > Ortofotomapa standardowa (kafle WMTS)*, krycie 100%.
* **Warstwa 2 (wierzch):** *Rzeźba terenu > Cieniowanie rzeźby NMT 1 m (kafle WMTS)*, tryb **Rzeźba terenu** z kryciem **70%** (lub tryb **Nakładka** z kryciem 75%).
* **Zalecany piksel:** 0,25 m/pix (lub 0,5 m/pix dla mniejszego rozmiaru plików).
* **Format zapisu:** JPEG (jakość 90) lub PNG.

{: .wskazowka }
Tryb **Rzeźba terenu** (oraz Nakładka) zachowuje naturalne nasycenie zieleni lasów i dachów zabudowy, wzmacniając jedynie światłocień na zboczach bez szarej zasłony na płaskim terenie. W trybie Zwykłym zbyt wysokie krycie cieniowania pokrywa ortofotomapę szarym nalotem.

---

### 2. Rzeźba turystyczna (Topo + cieniowanie NMT)

Klasyczny styl wojskowych i turystycznych map sztabowych. Plastyczny światłocień z rzeźby terenu układa się pod siatką ulic, poziomicami i obrysami budynków.

![Mapa topograficzna 1:10 000 z cieniowaniem rzeźby NMT w trybie Mnożenie]({{ '/pomoc/obrazy/kompozycja-2-topo-cieniowanie.jpg' | relative_url }}){: loading="lazy" width="836" height="365"}

*Rysunek 2. Mapa topograficzna 1:10 000 (układ 1965) z cieniowaniem NMT 1 m w trybie Mnożenie (krycie 90%).*

* **Warstwa 1 (podkład):** *Mapy topograficzne > Skany map topograficznych GUGiK > 1:10 000, układ 1965* (lub *BDOT10k_WMTS*), krycie 100%.
* **Warstwa 2 (wierzch):** *Rzeźba terenu > Cieniowanie rzeźby NMT 1 m (kafle WMTS)*, tryb **Mnożenie**, krycie **90%**.
* **Zalecany piksel:** 1 m/pix dla mapy 1:10 000 (lub 2 m/pix dla 1:25 000).
* **Format zapisu:** JPEG lub PNG.

{: .informacja }
Dzięki zastosowaniu trybu Mnożenie białe tło papierowej mapy staje się całkowicie przezroczyste dla światłocienia, a czarne napisy, siatka dróg i brązowe warstwice pozostają idealnie ostre i kontrastowe.

---

### 3. Trójwymiarowa rzeźba z dynamiczną hipsometrią ISOK

Wielowarstwowa kompozycja krajobrazowa. Łączy treść mapy topograficznej z głębokim cieniem zboczy oraz barwną skalą wysokościową generowaną na żywo przez usługę ISOK.

![Kompozycja Topo 1965 z cieniowaniem 90% i dynamiczną hipsometrią ISOK 50%]({{ '/pomoc/obrazy/kompozycja-2a-topo-dynamiczna-hipsometria.jpg' | relative_url }}){: loading="lazy" width="836" height="365"}

*Rysunek 3. Kompozycja trójwarstwowa: Topo 1:10 000, cieniowanie NMT (90%) oraz dynamiczna hipsometria ISOK (50%).*

* **Warstwa 1 (podkład):** *Mapy topograficzne > Skany map topograficznych GUGiK > 1:10 000, układ 1965*, krycie 100%.
* **Warstwa 2:** *Rzeźba terenu > Cieniowanie rzeźby NMT 1 m (kafle WMTS)*, tryb **Mnożenie**, krycie **90%**.
* **Warstwa 3:** *Rzeźba terenu > Hipsometria NMT 1 m: kolory wysokości (kafle WMTS)*, tryb **Mnożenie**, krycie **50%**.
* **Zalecany piksel:** 1 m/pix lub 1,32 m/pix.
* **Format zapisu:** JPEG lub PNG.

{: .uwaga }
Sama dynamiczna hipsometria ma intensywne, nasycone barwy. Nałożenie jej w trybie Mnożenia z pełnym kryciem 100% spowodowałoby zbytnie przyciemnienie mapy. Ustawienie krycia hipsometrii na poziomie **50%** daje optymalną czytelność: rzeźba terenu zyskuje wyraziste kolory (od zieleni dolin po ciepłe żółcie i brązy grzbietów), a treść topograficzna pozostaje w pełni czytelna.

---

### 4. Ortofotomapa z granicami działek (podkład geodezyjny)

Zestawienie niezbędne przy pracach projektowych, analizach nieruchomości i lokalizacji granic w terenie. Na zdjęcie lotnicze nanoszone są wektorowe granice parceli oraz ich oficjalne numery ewidencyjne z Krajowej Integracji Ewidencji Gruntów (KIEG).

![Ortofotomapa z granicami i numerami działek ewidencyjnych]({{ '/pomoc/obrazy/kompozycja-3-orto-dzialki.jpg' | relative_url }}){: loading="lazy" width="836" height="365"}

*Rysunek 4. Podkład terenowy: ortofotomapa standardowa z nałożonymi działkami i numeracją EGiB.*

* **Warstwa 1 (podkład):** *Ortofotomapy > Ortofotomapa standardowa (kafle WMTS)*, krycie 100%.
* **Warstwa 2:** *Ewidencja, geodezja i uzbrojenie > Działki ewidencyjne (KIEG) > Działki*, krycie 100% (przezroczyste tło PNG).
* **Warstwa 3:** *Ewidencja, geodezja i uzbrojenie > Działki ewidencyjne (KIEG) > Numery działek*, krycie 100% (przezroczyste tło PNG).
* **Zalecany piksel:** 0,25 m/pix.
* **Format zapisu:** PNG lub JPEG (jakość 90).

{: .wskazowka }
Działki ewidencyjne i numery działek są udostępniane przez serwer KIEG w postaci przezroczystych nakładek PNG. Dodanie ich do listy wybranych warstw automatycznie zachowuje pełną widoczność leżącej pod nimi ortofotomapy.

---

## Wskazówki praktyczne

1. **Dobór piksela siatki:** Program po wybraniu kilku warstw automatycznie ustawia najmniejszy zalecany piksel spośród nich. Łącząc ortofotomapę (0,25 m) z cieniowaniem NMT (1 m), program przyjmie 0,25 m. Cieniowanie zostanie gładko przeskalowane, a zdjęcie lotnicze zachowa maksymalną ostrość.
2. **Kolejność w kanapce:** Podkład powinien znajdować się na samym dole listy. Jeśli przez pomyłkę dodasz nieprzezroczystą mapę na górę, zasłoni warstwy leżące pod nią. Przeciągnij ją wtedy w dół myszą, użyj skrótu <kbd>Ctrl</kbd>+<kbd>↓</kbd> albo kliknij prawym przyciskiem myszy i wybierz *Przesuń niżej*.
3. **Format segmentów przy nakładkach:** Jeśli przygotowujesz mapę, która ma służyć jako przezroczysta warstwa w telefonie (np. same granice działek nad mapą wektorową w Locus Map lub OsmAnd), w oknie *Ustawienia* wybierz format **PNG** z opcją *przezroczyste tło*. Format JPEG nie obsługuje przezroczystości i zamieni puste tło na białe.
4. **Zapis sesji:** Każdą udaną kompozycję warto zapisać poleceniem *Sesja pobierania > Zapisz sesję w folderze...*. Plik sesji zapamiętuje wszystkie wybrane warstwy wraz z ich kolejnością i wartościami krycia.

---

➡️ **Kolejny krok:** [Przebieg pobierania, przerywanie i wznawianie →](../przebieg/)

*Zobacz też: [Okno główne programu](../../okno-glowne/) · [Usługi kafelkowe WMTS](../wmts/) · [Nakładki i łączenie warstw przy eksporcie](../../eksport/nakladki/)*
