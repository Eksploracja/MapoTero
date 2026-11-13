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

## Guru Maps (iPhone, iPad, Android)

Dla użytkowników urządzeń Apple (iPhone, iPad) to najwygodniejszy sposób na korzystanie z map Geoportalu w terenie. Aplikacja ma szybki silnik kafelkowy, a import własnych map jest bezpłatny.

| Format | Jak wgrać |
|---|---|
| **MBTiles** | na iPhonie prześlij plik przez AirDrop, aplikację Pliki lub chmurę i wybierz *Otwórz w Guru Maps*. Na Androidzie użyj opcji importu w menu map |
| **SQLiteDB** | otwórz plik w aplikacji lub zaimportuj przez menu map |

W aplikacji mapa pojawia się w sekcji map własnych. Może działać jako mapa główna lub jako nakładka nad inną mapą, z regulacją przezroczystości.

---

## QField (Android, iOS)

Mobilny program terenowy połączony z QGIS-em, używany w leśnictwie, ochronie przyrody, geodezji i pracach badawczych.

| Format | Jak wgrać |
|---|---|
| **MBTiles** | dodaj plik `.mbtiles` do projektu QGIS na komputerze jako warstwę rastrową, a następnie przenieś folder projektu do telefonu lub zsynchronizuj przez QFieldCloud |

Podkład kafelkowy MBTiles w QFieldzie nie obciąża pamięci telefonu i pozwala płynnie zbierać punkty, linie oraz wypełniać formularze terenowe bez dostępu do sieci.

---

## AlpineQuest Off-Road Explorer (Android)

Popularna aplikacja do nawigacji terenowej dla wypraw 4x4, quadów, motocykli enduro i turystyki kwalifikowanej.

| Format | Jak wgrać |
|---|---|
| **MBTiles** | skopiuj plik do folderu `alpinequest/maps` w telefonie, a w aplikacji wybierz *Mapy > Dostępne mapy > Dodaj mapę z pliku* |
| **SQLiteDB** | wskaż plik w menu dodawania mapy |

AlpineQuest umożliwia łączenie kilku warstw rastrowych jednocześnie i płynne sterowanie ich kryciem nad cieniowaniem terenu.

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
