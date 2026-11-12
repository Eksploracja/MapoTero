---
title: Mapa w telefonie i odbiorniku GPS
nav_order: 4
parent: Eksport mapy
---

# Mapa w telefonie i odbiorniku GPS
{: .no_toc }

Jak wgrać wyeksportowaną mapę do najpopularniejszych aplikacji i urządzeń.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

{: .informacja }
Foldery i nazwy poleceń w aplikacjach zmieniają się między wersjami aplikacji i systemu telefonu. Poniżej są sposoby
typowe. Jeśli coś wygląda inaczej, sprawdź pomoc aplikacji, hasło „mapy offline” albo „import map”.

## Przeniesienie pliku do telefonu

* **Kabel USB:** podłącz telefon, wybierz w nim tryb przesyłania plików i skopiuj plik do wskazanego folderu.
* **Bez kabla:** prześlij plik na telefon (np. przez chmurę lub komunikator) i otwórz go w aplikacji poleceniem
  *Otwórz za pomocą* / *Udostępnij*. Większość aplikacji sama skopiuje mapę do swojego folderu.

Duże pliki (setki MB i więcej) lepiej kopiować kablem.

---

## Locus Map

| Format | Jak wgrać |
|---|---|
| **MBTiles** | otwórz plik w Locus Map albo skopiuj do folderu map Locusa (`Locus/maps`) |
| **SQLiteDB** | skopiuj do folderu `Locus/mapsOnline` albo otwórz plik w aplikacji |
| **KMZ** | otwórz plik w aplikacji (import mapy KMZ) |

Mapa pojawi się na liście map offline. [Nakładkę](../nakladki/) włącza się jako warstwę nad mapą podkładową,
z regulacją przezroczystości.

---

## OsmAnd

| Format | Jak wgrać |
|---|---|
| **SQLiteDB** | skopiuj do folderu `osmand/tiles` (natywny format map rastrowych OsmAnd) |
| **MBTiles** | otwórz plik w aplikacji (import) |

Mapę rastrową włącza się w ustawieniach mapy jako *mapę podkładową* albo *nakładkę* / *podkład*
(w OsmAnd wymaga to zwykle włączonej wtyczki map online / rastrowych).

---

## OruxMaps

| Format | Jak wgrać |
|---|---|
| **OruxMaps** (folder) | skopiuj **cały folder** `_eksport_…_oruxmaps` do folderu `oruxmaps/mapfiles` w telefonie |
| **MBTiles** | skopiuj plik do folderu `oruxmaps/mapfiles` |
| **KMZ** | skopiuj do folderu map albo otwórz w aplikacji |

Po skopiowaniu odśwież listę map w aplikacji.

---

## Garmin

Plik **KMZ dla Garmin** (`…_garmin.kmz`):

1. Podłącz odbiornik do komputera kablem USB. Pojawi się jako dysk.
2. Skopiuj plik do folderu `Garmin/CustomMaps` (utwórz go, jeśli nie istnieje). Odbiorniki z kartą pamięci
   odczytują też ten folder na karcie.
3. Odłącz odbiornik i włącz mapy niestandardowe w ustawieniach map urządzenia.

Mapy niestandardowe obsługuje część odbiorników Garmin, a limit kafli zależy od modelu
(zob. [KMZ dla Garmin Custom Maps](../formaty/#kmz-dla-garmin-custom-maps)).

---

## Google Earth

Otwórz plik **KMZ** w Google Earth (Pro), np. dwuklikiem. Mapa pojawi się w panelu *Miejsca* jako nakładka obrazu.
Jej przezroczystość reguluje się suwakiem we właściwościach nakładki.

Do Google Earth nadają się też pojedyncze segmenty z plikami `.kml` (opcja w [ustawieniach](../../ustawienia/))
i scalony arkusz z plikiem `.kml` ([Scalanie segmentów](../../scalanie/)).

---

## TrekBuddy

Mapy TrekBuddy (archiwum `.tar` z kalibracją) program tworzy już **przy pobieraniu**, w trybie TrekBuddy
włączanym w oknie *Ustawienia* (poza oknem eksportu). Zob. [Ustawienia: TrekBuddy i Locus Map](../../ustawienia/#trekbuddy-i-locus-map).

---

➡️ **Kolejny krok:** [Zaawansowany wielowarstwowy MBTiles →](../../mbtiles-wielowarstwowy/)

*Zobacz też: [Wybór formatu](../formaty/) · [Poziomy powiększenia i rozmiar pliku](../powiekszenia/) · [Problemy i najczęstsze pytania](../../problemy/)*
