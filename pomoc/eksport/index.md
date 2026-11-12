---
title: Eksport mapy
nav_order: 12
has_children: true
---

# Eksport mapy
{: .no_toc }

Zapis pobranej mapy w formatach dla odbiorników GPS i aplikacji mobilnych, także jako przezroczystej nakładki
albo z kilkoma mapami nałożonymi na siebie.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Eksport do KMZ, MBTiles, SQLiteDB, OruxMaps* (<kbd>Ctrl</kbd>+<kbd>E</kbd>)  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Najkrótsza droga

1. Pobierz mapę (sesja musi mieć plik `conf.txt`).
2. Naciśnij <kbd>Ctrl</kbd>+<kbd>E</kbd>. Okno otworzy się z folderem bieżącej sesji.
3. Wybierz format (zob. [Wybór formatu](formaty/)).
4. Kliknij **Eksportuj**.
5. Plik pojawi się w folderze sesji. Skopiuj go do telefonu lub odbiornika
   ([Mapa w telefonie i odbiorniku GPS](aplikacje/)).

![Okno eksportu mapy z ponumerowanymi sekcjami]({{ '/pomoc/obrazy/10-eksport-mapy.png' | relative_url }})

*Rysunek 1. Okno eksportu mapy z ponumerowanymi sekcjami parametrów.*

| Nr | Sekcja okna | Opis |
|---|---|---|
| **1** | *Folder segmentów* | folder sesji (`conf.txt`) oraz podsumowanie: liczba segmentów, piksel, układ, opis mapy i data |
| **2** | *Format* | wybór formatu wynikowego (KMZ dla Garmin, KMZ, MBTiles, SQLiteDB, OruxMaps) lub kreatora *Wielowarstwowy...* |
| **3** | *Kafle Web Mercator* | zakres poziomów powiększenia i opcja przezroczystych kafli PNG |
| **4** | *Warstwy i przezroczystość* | nakładka, przezroczyste białe tło oraz nakładanie warstw z innych folderów sesji |
| **5** | *Rozmiar pliku* i *Jakość JPEG* | szacowanie wagi pliku, dopasowanie do limitu MB oraz stopień kompresji JPEG |
| **6** | Plan eksportu i przyciski | podgląd liczby kafli i poziomów oraz przyciski **Eksportuj** / **Przerwij** |

---

## Okno eksportu krok po kroku

| Grupa | Zawartość |
|---|---|
| *Folder segmentów (z plikiem conf.txt)* | folder eksportowanej sesji. Opcja **zmień folder** wskazuje inną sesję. Pod spodem: liczba segmentów (i ile z nich istnieje), piksel, układ, opis mapy, źródło danych i data pobrania |
| *Format* | KMZ dla Garmin (z limitem kafli), KMZ, MBTiles, SQLiteDB, OruxMaps. Przycisk **Wielowarstwowy...** otwiera [kreator wielowarstwowego MBTiles](../mbtiles-wielowarstwowy/) |
| *Kafle Web Mercator (MBTiles, SQLiteDB, OruxMaps)* | zakres poziomów powiększenia, kafle PNG. Zobacz: [Poziomy powiększenia i rozmiar pliku](powiekszenia/) |
| *Warstwy i przezroczystość* | nakładka, białe tło przezroczyste, warstwy z innych folderów z kryciem. Zobacz: [Nakładki i łączenie warstw](nakladki/) |
| *Rozmiar pliku* | szacowanie rozmiaru, limit MB i dopasowanie poziomu |
| *Jakość JPEG* | 10-100, domyślnie 85 |
| przyciski | **Eksportuj**, **Przerwij** |

Pod opcjami program na bieżąco pokazuje **plan eksportu**: liczbę kafli i poziomy powiększenia, a dla formatu KMZ siatkę
kafli i informację, czy rozdzielczość trzeba zmniejszyć ze względu na limit kafli.

---

## Komunikaty przy otwieraniu sesji

* *Segmenty gotowe do eksportu.* Wszystko w porządku.
* *W folderze jest plik error.txt: zestaw segmentów jest niekompletny…* Brakujące segmenty będą pustymi miejscami
  na mapie. Najpierw [dopobierz je](../pobieranie/przebieg/#wznawianie).
* *Segmenty pobrane wzdłuż trasy (korytarz)…* Poza korytarzem mapa będzie pusta (przezroczysta w kaflach PNG).

---

## Plik wynikowy

Wynik zapisywany jest **w folderze sesji**. Nazwa zawiera rozmiar siatki segmentów:

| Format | Plik |
|---|---|
| KMZ dla Garmin | `_eksport_10x8_garmin.kmz` |
| KMZ | `_eksport_10x8.kmz` |
| MBTiles | `_eksport_10x8.mbtiles` |
| SQLiteDB | `_eksport_10x8.sqlitedb` |
| OruxMaps | folder `_eksport_10x8_oruxmaps` |

Kolejny eksport tego samego formatu **zastępuje** poprzedni plik. Jeśli chcesz zachować kilka wersji (np. z różnymi
poziomami), zmień nazwę pliku po eksporcie.

Mapa w aplikacji ma nazwę złożoną z opisu mapy (zbiór map i warstwa) i daty pobrania. MBTiles i KMZ zawierają też
opis ze źródłem danych (atrybucję).

---

## Czas eksportu

Eksport może trwać od kilku sekund do wielu godzin, zależnie od liczby kafli. Gdy plan przekracza 2 mln kafli,
program pyta, czy kontynuować. Kafle bez danych (np. poza korytarzem) nie są zapisywane. Przerwany lub nieudany
eksport nie zostawia niedokończonego pliku.

---

➡️ **Kolejny krok:** [Wybór formatu eksportu →](formaty/)

*Zobacz też: [Wielowarstwowy MBTiles](../mbtiles-wielowarstwowy/) · [Eksport do CAD](../cad/) · [Scalanie segmentów](../scalanie/)*
