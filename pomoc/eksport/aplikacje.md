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

* **Bezpośrednio przez Wi-Fi i kod QR (najwygodniej):** po wygenerowaniu mapy w oknie *Eksport mapy* kliknij przycisk **Wyślij na telefon...** (funkcja dostępna także w menu *Narzędzia > Prześlij mapę na telefon (Wi-Fi)...*). Program uruchamia lokalny serwer i wyświetla na ekranie kod QR. Skieruj aparat telefonu na ekran, otwórz wyświetlony link i pobierz plik bezpośrednio do pamięci urządzenia przez domową sieć Wi-Fi, bez kabla USB, bez logowania i bez chmury (zob. [opis narzędzia](../../narzedzia/#przesyłanie-mapy-na-telefon-przez-wi-fi-i-kod-qr)).
* **Kabel USB:** podłącz telefon do komputera, wybierz w telefonie tryb *Przesyłanie plików (MTP)* i skopiuj plik do wskazanego folderu aplikacji.
* **Chmura lub komunikator:** prześlij plik na telefon przez własną chmurę (Dysk Google, OneDrive, Dropbox, Nextcloud) albo komunikator i otwórz go w aplikacji poleceniem *Otwórz za pomocą* / *Udostępnij*.

![Okno bezpośredniego transferu mapy na telefon przez Wi-Fi i kod QR]({{ '/pomoc/obrazy/16-wyslij-telefon.png' | relative_url }})

*Rysunek 1. Bezpośredni transfer mapy na telefon: program wyświetla kod QR w lokalnej sieci Wi-Fi.*

---

## Locus Map

| Format | Jak wgrać |
|---|---|
| **MBTiles** | otwórz plik w Locus Map albo skopiuj do folderu map Locusa (`Locus/maps`) |
| **GeoPackage (.gpkg)** | skopiuj do folderu `Locus/maps` albo otwórz plik w aplikacji |
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
| **GeoPackage (.gpkg)** | skopiuj plik do folderu `oruxmaps/mapfiles` |
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

## QField i Mergin Maps (Android, iOS)

Mobilne programy terenowe współpracujące ze środowiskiem QGIS, powszechnie używane w leśnictwie, ochronie przyrody, geodezji, inspekcjach infrastruktury i pracach badawczych.

| Format | Jak wgrać |
|---|---|
| **GeoPackage (.gpkg)** | dodaj plik `.gpkg` jako warstwę kafelkową w QGIS i zsynchronizuj projekt z telefonem (kabel, QFieldCloud lub Mergin Maps Cloud), albo skopiuj plik `.gpkg` bezpośrednio do pamięci urządzenia |
| **MBTiles** | dodaj plik `.mbtiles` do projektu QGIS na komputerze jako warstwę rastrową, a następnie przenieś folder projektu do telefonu |

Kafelkowe podkłady rastrowe GeoPackage i MBTiles nie obciążają pamięci telefonu i pozwalają błyskawicznie rejestrować obiekty przestrzenne oraz wypełniać formularze terenowe bez dostępu do sieci komórkowej.

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

Dla odbiorników turystycznych Garmin (seria GPSMAP, Oregon, Montana, eTrex, Alpha) MapoTero oferuje dwa dedykowane formaty: **JNX (Garmin BirdsEye)** oraz **KMZ (Garmin Custom Maps)**.

### Kopiowanie bezpośrednie przez USB w programie

Po wyeksportowaniu pliku `.jnx` lub `.kmz` w oknie *Eksport mapy* kliknij **Wyślij na telefon...** (dla `.jnx` otwiera od razu kopiowanie na Garmina, a dla `.kmz` przy podłączonym urządzeniu proponuje wysyłkę na Garmina lub przez Wi-Fi) albo otwórz w dowolnym momencie menu *Narzędzia > Kopiuj mapę do Garmina (USB)...*. Program:
1. Automatycznie wykrywa podłączone urządzenia Garmin oraz włożone do nich karty microSD.
2. Sprawdza dostępną pamięć urządzenia.
3. Kopiuje plik do właściwego folderu docelowego (`Garmin/CustomMaps` dla plików KMZ lub `Garmin/BirdsEye` dla plików JNX) z zabezpieczeniem przed zapełnieniem dysku.

### Formaty i wgrywanie ręczne

| Format | Folder docelowy w urządzeniu / karcie SD | Cechy i ograniczenia |
|---|---|---|
| **JNX (Garmin BirdsEye)** | `Garmin/BirdsEye` | **Brak limitu 100 kafli**, więc jedna mapa może obejmować cały region lub województwo. Wyświetla się pod nazwą produktu w menu *Ustawienia mapy > Informacje o mapie*. Wymaga odbiornika z obsługą BirdsEye (w nowszych modelach z oprogramowaniem patched firmware dla niepodpisanych plików JNX). |
| **KMZ dla Garmin** | `Garmin/CustomMaps` | Standardowy format Garmin Custom Maps obsługiwany fabrycznie przez wszystkie modele. Gdy mapa przekracza limit 100 kafli, program automatycznie dzieli ją na tomy (`…_tom1.kmz`, `…_tom2.kmz` itd.). |

Jeśli kopiujesz plik ręcznie:
1. Podłącz odbiornik kablem USB do komputera (urządzenie zgłosi się jako dysk wymienny).
2. Skopiuj plik `.kmz` do `Garmin/CustomMaps` lub plik `.jnx` do `Garmin/BirdsEye` (na pamięci wewnętrznej lub karcie microSD).
3. Bezpiecznie odłącz urządzenie i włącz mapę w menu mapy odbiornika (*Ustawienia mapy > Wybierz mapę*).

{: .wskazowka }
Dla odbiorników Garmin z ekranami transreflektywnymi warto w oknie eksportu włączyć opcję **Korekta obrazu (filtr MIP, ostrość, kontrast)** i wybrać profil *Ekran w słońcu / Garmin MIP*. Zwiększa ona kontrast i nasycenie barw, co wyraźnie poprawia czytelność podkładu w ostrym słońcu. Zob. [Podgląd MIP](../../narzedzia/#podgląd-mip-ekrany-garmin-memory-in-pixel).

---

## Google Earth

Otwórz plik **KMZ** w Google Earth (Pro), np. dwuklikiem. Mapa pojawi się w panelu *Miejsca* jako nakładka obrazu.
Jej przezroczystość reguluje się suwakiem we właściwościach nakładki.

Do Google Earth nadają się też pojedyncze segmenty z plikami `.kml` (opcja w [ustawieniach](../../ustawienia/))
i scalony arkusz z plikiem `.kml` ([Scalanie segmentów](../../scalanie/)).

---

## TrekBuddy

Format *TrekBuddy / Locus Map* w oknie eksportu tworzy archiwum `.tar` z dowolnej pobranej sesji: arkusz mapy (z nakładkami
i korektą obrazu) pocięty na kafle `512 × 512 px` w podfolderze `set`, plik kalibracji `.map` całego obszaru i listę kafli `.set`.
Plik `_trekbuddy_<kol>x<wier>.tar` powstaje w folderze sesji. Locus Map: skopiuj go do folderu `Locus/maps`.

Do wersji 3.18 paczkę tworzyło pobieranie w trybie TrekBuddy włączanym w oknie *Ustawienia*, przed pobieraniem.

---

➡️ **Kolejny krok:** [MBTiles wieloskalowy →](../../mbtiles-wielowarstwowy/)

*Zobacz też: [Wybór formatu](../formaty/) · [Poziomy powiększenia i rozmiar pliku](../powiekszenia/) · [Problemy i najczęstsze pytania](../../problemy/)*
