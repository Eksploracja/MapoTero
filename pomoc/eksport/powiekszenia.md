---
title: Poziomy powiększenia i rozmiar pliku
nav_order: 2
parent: Eksport mapy
---

# Poziomy powiększenia i rozmiar pliku
{: .no_toc }

Jak dobrać zakres powiększeń, format kafli i jakość, żeby mapa była czytelna, a plik nie był za duży.
Dotyczy MBTiles, SQLiteDB i OruxMaps.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Poziomy powiększenia

Mapy w telefonie składają się z kafli 256 × 256 px ułożonych w **piramidę**: na każdym kolejnym poziomie
powiększenia (*zoom*) kafle obejmują dwa razy mniejszy teren, więc jest ich cztery razy więcej. Aplikacja przy danym
przybliżeniu wyświetla kafle odpowiedniego poziomu.

Rozmiar piksela kafli na szerokości geograficznej Polski (ok. 52°N):

| Poziom | Piksel kafla | Odpowiada mniej więcej |
|---|---|---|
| 12 | ok. 24 m | przegląd regionu |
| 13 | ok. 12 m | |
| 14 | ok. 5,9 m | mapy 1:100 000 (piksel 8 m) |
| 15 | ok. 2,9 m | mapy 1:50 000 (piksel 4 m) |
| 16 | ok. 1,5 m | mapy 1:25 000 (piksel 2 m) |
| 17 | ok. 0,7 m | mapy 1:10 000 (piksel 1 m) |
| 18 | ok. 0,37 m | |
| 19 | ok. 0,18 m | ortofotomapa (piksel 0,25 m) |

Program sam ustawia zakres po otwarciu sesji:

* **Najwyższy poziom** (*do*): pierwszy poziom, na którym piksel kafla jest nie większy niż piksel pobranej mapy,
  czyli pełna szczegółowość (przy kilku warstwach: najdokładniejszej z nich).
* **Najniższy poziom** (*od*): o 6 poziomów niżej, ale nie mniej niż 8.

Pole poziomów przyjmuje wartości 0-22.

| Zmiana | Skutek |
|---|---|
| obniżenie poziomu *do* o 1 | plik ok. 4 razy mniejszy, mapa mniej szczegółowa przy największym przybliżeniu |
| podniesienie poziomu *do* ponad wartość programu | większy plik bez nowych szczegółów (obraz jest tylko powiększany) |
| obniżenie poziomu *od* | mapa widoczna przy większym oddaleniu, plik rośnie nieznacznie |
| podniesienie poziomu *od* | przy oddaleniu aplikacja nie pokaże mapy (albo pokaże własną mapę podkładową) |

{: .wskazowka }
Wiele aplikacji przy przybliżeniu ponad najwyższy poziom mapy powiększa jej ostatni poziom, więc obniżenie
poziomu *do* o jeden zwykle nie przeszkadza w terenie, a bardzo zmniejsza plik.

---

## Kafle PNG i JPEG

* **JPEG** (domyślnie): małe pliki, dobre dla ortofotomap i map skanowanych. Brak przezroczystości: poza obszarem
  mapy kafle mają białe tło.
* **PNG** (opcja *kafle PNG (przezroczyste brzegi)*): brzegi mapy i miejsca bez danych są przezroczyste, więc
  aplikacja pokaże tam swoją mapę podkładową. Pliki są większe, zwłaszcza dla zdjęć.

Przy [nakładce](../nakladki/) kafle są zawsze PNG.

---

## Jakość JPEG

Pole *Jakość JPEG* (10-100, domyślnie 85) dotyczy kafli JPEG. Wartości 75-90 to zwykle dobry kompromis.
Poniżej 70 widać artefakty kompresji na napisach map. Powyżej 90 plik szybko rośnie, a różnicy prawie nie widać.

---

## Szacowanie rozmiaru

Przycisk **oszacuj rozmiar** renderuje kilka próbnych kafli i na tej podstawie szacuje rozmiar pliku. Komunikat
podaje rozmiar poszczególnych poziomów i całości. Szacunek może się różnić od wyniku o kilkadziesiąt procent,
bo zależy od treści mapy w miejscach próbek.

---

## Dopasowanie do limitu

Gdy plik musi się zmieścić w określonym rozmiarze (karta pamięci, limit aplikacji, przesyłanie):

1. Wpisz *limit [MB]* (od 1 do 100 000, domyślnie 500).
2. Kliknij **dopasuj**.

Program szacuje rozmiar kolejnych poziomów i ustawia **najwyższy poziom**, przy którym mapa mieści się w limicie.
Jeśli limit przekracza już sam najniższy poziom, program o tym informuje. Zwiększ wtedy limit, podnieś poziom
*od* albo zmniejsz jakość JPEG.

---

➡️ **Kolejny krok:** [Nakładki i łączenie warstw w eksporcie →](../nakladki/)

*Zobacz też: [Wybór formatu](../formaty/) · [Mapa w telefonie i odbiorniku GPS](../aplikacje/) · [Wielowarstwowy MBTiles](../../mbtiles-wielowarstwowy/)*
