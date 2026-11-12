---
title: Jak duży obszar pobrać
nav_order: 4
parent: Wybór obszaru
---

# Jak duży obszar pobrać
{: .no_toc }

Liczba pikseli rośnie z kwadratem dokładności: dwa razy mniejszy piksel to cztery razy więcej danych. Warto
oszacować czas pobierania i miejsce na dysku przed kliknięciem *Pobierz*.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Szacowanie liczby segmentów

Po zaznaczeniu obszaru program pokazuje w polu *Wynikowy rozmiar siatki* liczbę kolumn i wierszy segmentów oraz wymiary mapy w pikselach i kilometrach. W komunikacie u dołu podaje powierzchnię w km².

Liczbę segmentów można oszacować ze wzoru:

> liczba segmentów ≈ powierzchnia obszaru ÷ (bok segmentu w px × piksel w m)²

Przykład: obszar 20 × 20 km, bok segmentu 2000 px, piksel 1 m. Segment obejmuje 2 × 2 km, więc siatka liczy 10 × 10 = 100 segmentów i 20 000 × 20 000 pikseli.

| Obszar | Piksel | Segment 2000 px | Liczba segmentów | Pikseli razem |
|---|---|---|---|---|
| 10 × 10 km | 2 m | 4 × 4 km | 3 × 3 = 9 | ok. 36 mln |
| 10 × 10 km | 1 m | 2 × 2 km | 5 × 5 = 25 | 100 mln |
| 10 × 10 km | 0,25 m | 0,5 × 0,5 km | 20 × 20 = 400 | 1,6 mld |
| 50 × 50 km | 2 m | 4 × 4 km | 13 × 13 = 169 | ok. 676 mln |
| powiat (ok. 1000 km²) | 0,25 m | 0,5 × 0,5 km | ok. 4000 | ok. 16 mld |

---

## Czas pobierania

Tempo pracy zależy w największym stopniu od serwera mapowego. Pojedynczy segment z Geoportalu pobiera się w ułamku sekundy, ale serwery map historycznych generują kafelki nawet kilkanaście sekund. Program wysyła kilka zapytań równolegle (wartość *Równoczesne pobrania* w [ustawieniach](../../ustawienia/), domyślnie 4).

Najprościej sprawdzić tempo na małym wycinku: pobierz kilka segmentów testowych i pomnóż uzyskany czas przez rozmiar planowanej siatki.

{: .uwaga }
Nie zwiększaj liczby równoczesnych pobrań bez potrzeby. Zbyt duże obciążenie spowalnia serwer albo skutkuje czasową blokadą adresu IP. Zasady automatycznych przerw opisuje rozdział [Ochrona serwerów map](../../pobieranie/serwery/).

---

## Miejsce na dysku

Rozmiar pliku segmentu zależy od formatu oraz zawartości mapy:

* **JPEG**: najmniejsze pliki. Dla map topograficznych i ortofotomap segment 2000 × 2000 px zajmuje od kilkuset kilobajtów do około 1–2 MB.
* **PNG**: bezstratna kompresja. Dla ortofotomap pliki są kilkukrotnie większe niż JPEG, ale dla rysunków o jednolitym tle (granice działek, sieci uzbrojenia) pozostają bardzo małe.
* **TIFF**: największa objętość danych na dysku.

Ortofotomapa o pikselu 0,25 m w formacie JPEG zajmuje orientacyjnie kilka megabajtów na km². Mapa topograficzna 1:25 000 o pikselu 2 m to ułamek megabajta na km². Scalony arkusz i wygenerowane bazy mobilne wymagają dodatkowego miejsca o zbliżonej wielkości.

{: .wskazowka }
Pobierz na próbę kilka segmentów i sprawdź ich wagę w folderze sesji. Pomnożenie tej wartości przez liczbę segmentów da rzetelny szacunek.

---

## Granice programu

* Do **10 mln segmentów** w pojedynczym zadaniu. Przy większej liczbie program poprosi o zawężenie obszaru lub zwiększenie piksela.
* Bok segmentu wynosi maksymalnie **2048 px**.
* W jednej kompozycji mieści się do 12 warstw.
* Gdy cała mapa przekracza 5 mld pikseli, w oknie pojawia się czerwone ostrzeżenie informujące o długim czasie generowania obrazu.

---

## Dzielenie dużych obszarów

Bardzo duży obszar warto podzielić na mniejsze części i pobierać każdą do osobnego folderu:

* ewentualne zerwanie połączenia dotyczy tylko bieżącego fragmentu,
* poszczególne fragmenty można przetwarzać niezależnie,
* łatwiej kontrolować wolną przestrzeń na dysku.

Zamiast prostokąta wzdłuż długiej trasy użyj [obszaru wzdłuż trasy](../korytarz/).

---

➡️ **Kolejny krok:** [Rozpoczęcie i przebieg pobierania mapy →](../../pobieranie/przebieg/)

*Zobacz też: [Rozmiar piksela a skala mapy](../../pojecia/#rozmiar-piksela-a-skala-mapy) · [Sesje pobierania](../../sesje/)*
