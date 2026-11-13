---
title: Katalog zbiorów map
nav_order: 17
---

# Katalog zbiorów map
{: .no_toc }

Opis 43 zbiorów map dostarczanych z programem: skąd pochodzą, w jakich grupach drzewa je znaleźć, jakiego są typu (WMS / WMTS / XYZ) i jaki mają zalecany rozmiar piksela.
{: .fs-5 .fw-300 }

**Gdzie w programie:** drzewo *Wybierz mapy* w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Jak czytać katalog

W drzewie *Wybierz mapy* warstwy są pogrupowane tematycznie przez plik katalogu `warstwy\katalog.txt` (UTF-8) i opatrzone czytelnymi nazwami zamiast surowych nazw plików. Wiersz `> Grupa` otwiera grupę, `>> Podgrupa` tworzy podgrupę, a wiersz `zbiór | warstwa | nazwa` dodaje do niej warstwę ze zbioru map, np.:

```text
> Przyroda i środowisko
>> Lasy (Bank Danych o Lasach)
Lasy_Panstwowe_BDL | 5 | Nadleśnictwa
```

Zbiory i warstwy niewymienione w `katalog.txt` (np. dodane [kreatorem serwerów](../serwery/kreator/)) pojawiają się na końcu drzewa w grupie **Dodane serwery**.

---

## Przegląd wszystkich zbiorów

| Plik zbioru | Źródło / instytucja | Typ | Warstw | Zalecany piksel |
|---|---|---|---|---|
| `administracyjna` | GUGiK / Geoportal | WMS | 1 | 16 m/px |
| `BDOO_WMTS` | GUGiK / Geoportal | WMTS | 1 | 25 m/px |
| `BDOT10k_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `dzialki_ewid_EGIB` | GUGiK / Geoportal | WMS | 6 | 0,5–4 m/px |
| `geologiczna_50_litogenetyczna` | PIG-PIB (Geologia) | WMS | 1 | 8 m/px |
| `geologiczna_50_SMGP` | PIG-PIB (Geologia) | WMS | 1 | 4 m/px |
| `geologiczna_500` | PIG-PIB (Geologia) | WMS | 1 | 70 m/px |
| `hydrograficzna_50` | GUGiK / Geoportal | WMS | 2 | 4 m/px |
| `krajobrazowa` | GUGiK / Geoportal | WMS | 1 | 16 m/px |
| `Lasy_Panstwowe_BDL` | Lasy Państwowe (BDL) | WMS | 7 | 2–32 m/px |
| `mapa_glebowo_rolnicza` | GUGiK / Geoportal | WMS | 2 | 1 m/px |
| `mapy_historyczne_IHPAN` | IHPAN / Pastmaps | WMS | 28 | 1–40 m/px |
| `numer_mod_terenu_cieniowanie_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `numer_mod_terenu_hipsometria_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `numer_mod_terenu_WMTS` | GUGiK / Geoportal | WMTS | 1 | 2 m/px |
| `obszary_chronione_GDOS` | GDOŚ | WMS | 10 | 1–4 m/px |
| `OpenStreetMap_Terrestris` | terrestris (OSM, znak wodny) | WMS | 6 | 2–10 m/px |
| `ortofotomapa` | GUGiK / Geoportal | WMS | 1 | 1 m/px |
| `ortofotomapa_HighResolution_WMTS` | GUGiK / Geoportal | WMTS | 1 | 0,1 m/px |
| `ortofotomapa_TrueOrtho` | GUGiK / Geoportal | WMS | 1 | 0,1 m/px |
| `ortofotomapa_WMTS` | GUGiK / Geoportal | WMTS | 1 | 0,25 m/px |
| `OSMapa_PL` | OSMapa.pl / OSM | XYZ | 1 | 2 m/px |
| `osnowy_geodezyjne` | GUGiK / Geoportal | WMS | 3 | 1 m/px |
| `plany_miast_1944_IHPAN` | IHPAN / Pastmaps | WMS | 20 | 2 m/px |
| `regiony_fizycznogeograficzne` | PIG-PIB (Geologia) | WMS | 2 | 25–50 m/px |
| `rejestr_granic_jednostki_teryt` | GUGiK / Geoportal | WMS | 52 | 1–16 m/px |
| `rejestr_granic_punkty_adresowe` | GUGiK / Geoportal | WMS | 1 | 1 m/px |
| `rejestr_nazw_geograficznych` | GUGiK / Geoportal | WMS | 5 | 2–4 m/px |
| `skany_map_topograficznych` | GUGiK / Geoportal | WMS | 9 | 1–4 m/px |
| `sozologiczna_50` | GUGiK / Geoportal | WMS | 1 | 4 m/px |
| `szczyty` | GUGiK / Geoportal | WMS | 1 | 4 m/px |
| `TOPO_WMTS` | GUGiK / Geoportal | WMTS | 1 | 2 m/px |
| `UMP_pcPL` | UMP-pcPL | XYZ | 1 | 2 m/px |
| `uzbrojenie_terenu_KIUT` | GUGiK / Geoportal | WMS | 9 | 0,25 m/px |
| `Waymarked_Trails` | Waymarked Trails / OSM | XYZ | 6 | 2 m/px |
| `zabytki_archeo_NID` | NID | WMS | 1 | 2 m/px |
| `zabytki_ewidencja_archeo_NID` | NID | WMS | 1 | 2 m/px |
| `zabytki_ewidencja_nieruchomych_NID` | NID | WMS | 1 | 1 m/px |
| `zabytki_groby_wojenne_NID` | NID | WMS | 1 | 1 m/px |
| `zabytki_NID` | NID | WMS | 1 | 1 m/px |
| `zabytki_pomniki_historii_NID` | NID | WMS | 1 | 4 m/px |
| `zabytki_unesco_NID` | NID | WMS | 1 | 4 m/px |
| `zabytki_zasiegi_NID` | NID | WMS | 1 | 1 m/px |

---

## Ortofotomapy

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Ortofotomapa standardowa 25 cm (kafle WMTS) | `ortofotomapa_WMTS` | `ORTOFOTOMAPA` | WMTS | 0,25 m/px |
| Ortofotomapa wysokiej rozdzielczości do 10 cm (kafle WMTS) | `ortofotomapa_HighResolution_WMTS` | `ORTOFOTOMAPA` | WMTS | 0,1 m/px |
| Ortofotomapa wysokiej rozdzielczości (WMS) | `ortofotomapa` | `Raster` | WMS | 1 m/px |
| Prawdziwa ortofotomapa (TrueOrtho) | `ortofotomapa_TrueOrtho` | `PrawdziwaOrtofotomapa` | WMS | 0,1 m/px |

---

## Mapy topograficzne

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa topograficzna GUGiK (kafle WMTS) | `TOPO_WMTS` | `MAPA TOPOGRAFICZNA` | WMTS | 2 m/px |
| Baza danych obiektów topograficznych BDOT10k (kafle WMTS) | `BDOT10k_WMTS` | `BDOT10k` | WMTS | 1 m/px |
| Mapa ogólnogeograficzna BDOO z cieniowaniem (kafle WMTS) | `BDOO_WMTS` | `BDOOCieniowanie` | WMTS | 25 m/px |
| Mapa krajobrazowa GUGiK (przeglądowa) | `krajobrazowa` | `Krajobrazowa` | WMS | 16 m/px |

### Skany map topograficznych GUGiK

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| 1:10 000, układ 1942 | `skany_map_topograficznych` | `Raster_10_1942` | WMS | 1 m/px |
| 1:10 000, układ 1965 | `skany_map_topograficznych` | `Raster_10_1965` | WMS | 1 m/px |
| 1:10 000, układ 1992 | `skany_map_topograficznych` | `Raster_10_1992` | WMS | 1 m/px |
| 1:25 000, układ 1965 | `skany_map_topograficznych` | `Raster_25_1965` | WMS | 2 m/px |
| 1:50 000, układ 1942 | `skany_map_topograficznych` | `Raster_50_1942` | WMS | 4 m/px |
| 1:50 000, układ 1965 | `skany_map_topograficznych` | `Raster_50_1965` | WMS | 4 m/px |
| 1:50 000, układ 1992 | `skany_map_topograficznych` | `Raster_50_1992` | WMS | 4 m/px |
| 1:50 000, układ UTM | `skany_map_topograficznych` | `Raster_50_UTM` | WMS | 4 m/px |
| 1:50 000, VMap L2 | `skany_map_topograficznych` | `Raster_50tys_VMap` | WMS | 4 m/px |

### Mapy społecznościowe i turystyczne (kafle XYZ)

{: .wskazowka }
Warstwy *Waymarked Trails* są przezroczystymi nakładkami. Po dodaniu mapy podkładowej (np. *OSMapa.pl* lub skanów GUGiK) program automatycznie umieszcza podkład pod szlakami.

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| OSMapa.pl (mapa topograficzno-turystyczna OpenStreetMap) | `OSMapa_PL` | `osmapa` | XYZ | 2 m/px |
| UMP-pcPL (Uzupełniająca Mapa Polski) | `UMP_pcPL` | `ump_pcPL` | XYZ | 2 m/px |
| Szlaki piesze: nakładka (Waymarked Trails) | `Waymarked_Trails` | `hiking` | XYZ | 2 m/px |
| Szlaki rowerowe: nakładka (Waymarked Trails) | `Waymarked_Trails` | `cycling` | XYZ | 2 m/px |
| Szlaki MTB: nakładka (Waymarked Trails) | `Waymarked_Trails` | `mtb` | XYZ | 2 m/px |
| Szlaki konne: nakładka (Waymarked Trails) | `Waymarked_Trails` | `riding` | XYZ | 2 m/px |
| Trasy rolkowe: nakładka (Waymarked Trails) | `Waymarked_Trails` | `skating` | XYZ | 2 m/px |
| Trasy narciarskie: nakładka (Waymarked Trails) | `Waymarked_Trails` | `slopes` | XYZ | 2 m/px |

### OpenStreetMap (terrestris.de, ze znakiem wodnym)

{: .informacja }
Darmowa usługa terrestris.de dokleja do każdego pobranego obrazu reklamowy znak wodny.

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa ogólna OpenStreetMap | `OpenStreetMap_Terrestris` | `OSM-WMS` | WMS | 2 m/px |
| Mapa topograficzna OpenStreetMap | `OpenStreetMap_Terrestris` | `TOPO-OSM-WMS` | WMS | 4 m/px |
| Mapa turystyczna OpenStreetMap | `OpenStreetMap_Terrestris` | `HikeMap` | WMS | 4 m/px |
| Nakładka: drogi i nazwy (przezroczysta) | `OpenStreetMap_Terrestris` | `OSM-Overlay-WMS` | WMS | 2 m/px |

---

## Rzeźba terenu

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Hipsometria z cieniowaniem NMT 1 m (kafle WMTS) | `numer_mod_terenu_WMTS` | `HipsoCien` | WMTS | 2 m/px |
| Cieniowanie rzeźby NMT 1 m (kafle WMTS) | `numer_mod_terenu_cieniowanie_WMTS` | `ISOK_Cien` | WMTS | 1 m/px |
| Hipsometria NMT 1 m: kolory wysokości (kafle WMTS) | `numer_mod_terenu_hipsometria_WMTS` | `Hipsometria` | WMTS | 1 m/px |
| Cieniowanie rzeźby SRTM 30 m (terrestris, ze znakiem wodnym) | `OpenStreetMap_Terrestris` | `SRTM30-Hillshade` | WMS | 10 m/px |
| Poziomice SRTM 30 m (terrestris, ze znakiem wodnym) | `OpenStreetMap_Terrestris` | `SRTM30-Contour` | WMS | 10 m/px |
| Szczyty górskie i wzniesienia (GUGiK) | `szczyty` | `szczyty` | WMS | 4 m/px |

---

## Mapy historyczne i zabytki

### Zabytki i dziedzictwo (NID)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Rejestr zabytków nieruchomych: zasięgi i typy | `zabytki_zasiegi_NID` | `Zabytki_nieruchome_I` | WMS | 1 m/px |
| Rejestr zabytków nieruchomych (INSPIRE) | `zabytki_NID` | `Immovable_Monuments` | WMS | 1 m/px |
| Rejestr zabytków archeologicznych | `zabytki_archeo_NID` | `Archaeological_Monuments` | WMS | 2 m/px |
| Ewidencja zabytków nieruchomych | `zabytki_ewidencja_nieruchomych_NID` | `Ewidencja_zabytkow_nieruchomych_I` | WMS | 1 m/px |
| Ewidencja zabytków archeologicznych | `zabytki_ewidencja_archeo_NID` | `Ewidencja_zabytków_archeologicznych` | WMS | 2 m/px |
| Pomniki Historii | `zabytki_pomniki_historii_NID` | `Monuments_of_History` | WMS | 4 m/px |
| Obiekty światowego dziedzictwa UNESCO | `zabytki_unesco_NID` | `World_Heritage_Properties_List` | WMS | 4 m/px |
| Groby i cmentarze wojenne | `zabytki_groby_wojenne_NID` | `Groby_i_cmentarze_wojenne_I` | WMS | 1 m/px |

### Mapy historyczne (IHPAN)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Messtischblätter 1:25 000 (1870–1945) | `mapy_historyczne_IHPAN` | `m25k_2180` | WMS | 2 m/px |
| Mapa szczegółowa WIG 1:25 000 (1918–1939) | `mapy_historyczne_IHPAN` | `wig25k_2180` | WMS | 2 m/px |
| Mapa taktyczna WIG 1:100 000 (1918–1939) | `mapy_historyczne_IHPAN` | `wig100k_2180` | WMS | 8 m/px |
| Mapa operacyjna WIG 1:300 000 (1918–1939) | `mapy_historyczne_IHPAN` | `wig300k_3857` | WMS | 25 m/px |
| Mapa Polski i krajów ościennych WIG 1:500 000 (1947) | `mapy_historyczne_IHPAN` | `wig500k_2180` | WMS | 40 m/px |
| Karte des Deutschen Reiches 1:100 000 (ok. 1900) | `mapy_historyczne_IHPAN` | `kdr_2180` | WMS | 8 m/px |
| Karte des Deutschen Reiches 1:100 000 (ok. 1944) | `mapy_historyczne_IHPAN` | `kdr_gb_2180` | WMS | 8 m/px |
| Karte des Westlichen Russlands 1:100 000 (1914–1921) | `mapy_historyczne_IHPAN` | `kdwr_2180` | WMS | 8 m/px |
| Übersichtskarte von Mitteleuropa 1:300 000 (ok. 1900) | `mapy_historyczne_IHPAN` | `ukvme_2180` | WMS | 25 m/px |
| Mapa administracyjna Galicji i Lodomerii 1:115 200 (1855) | `mapy_historyczne_IHPAN` | `kummersberg_2180` | WMS | 8 m/px |
| Śląsk w końcu XVIII wieku | `mapy_historyczne_IHPAN` | `slask18w_2180` | WMS | 16 m/px |
| Mapa Schroettera 1:150 000 (1796–1802) | `mapy_historyczne_IHPAN` | `schroetter_3857` | WMS | 16 m/px |
| Mapa Gilly'ego 1:150 000 (1802–1803) | `mapy_historyczne_IHPAN` | `gilly_3857` | WMS | 16 m/px |
| Mapa Textora-Sotzmanna 1:150 000 (1806–1808) | `mapy_historyczne_IHPAN` | `textor_3857` | WMS | 16 m/px |
| Mapa Gaula/Raczyńskiego 1:125 000 (1807–1812) | `mapy_historyczne_IHPAN` | `gaul_3857` | WMS | 12 m/px |
| Topograficzna Karta Królestwa Polskiego 1:126 000 (1839–1843) | `mapy_historyczne_IHPAN` | `TKKP_126k_3857` | WMS | 12 m/px |
| Karta Dawnej Polski Chrzanowskiego 1:300 000 (1859) | `mapy_historyczne_IHPAN` | `chrzanowski_3857` | WMS | 30 m/px |
| Mapa Reymanna 1:200 000 (druga połowa XIX w.) | `mapy_historyczne_IHPAN` | `reymann_3857` | WMS | 20 m/px |
| Dwuwiorstówka 1:84 000 (1908–1936) | `mapy_historyczne_IHPAN` | `dwuwiorstowka_3857` | WMS | 8 m/px |
| Mapa taktyczna WIG 1:100 000 (1918–1939): zasięg rozszerzony | `mapy_historyczne_IHPAN` | `wig100k_4326_extended` | WMS | 8 m/px |
| Mapa Polski i krajów ościennych WIG 1:500 000 (1939) | `mapy_historyczne_IHPAN` | `wig500k1939_3857` | WMS | 40 m/px |
| Mapa WIG 1:100 000 w układzie „Borowa Góra” (1947–1954) | `mapy_historyczne_IHPAN` | `wig_1947_1952_3857` | WMS | 8 m/px |

### Ziemie polskie Korony w drugiej połowie XVI w. (IHPAN)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Ziemie polskie Korony 1:250 000 | `mapy_historyczne_IHPAN` | `ahp_ziemie_polskie_3857` | WMS | 20 m/px |
| Podziały kościelne 1:500 000 | `mapy_historyczne_IHPAN` | `ahp_koscielne_3857` | WMS | 40 m/px |
| Rozmieszczenie własności ziemskiej 1:500 000 | `mapy_historyczne_IHPAN` | `ahp_wlasnosc_3857` | WMS | 40 m/px |

### Plany miast (IHPAN)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Białystok (1944) | `plany_miast_1944_IHPAN` | `bialystok_2180` | WMS | 2 m/px |
| Bydgoszcz (1944) | `plany_miast_1944_IHPAN` | `bydgoszcz_2180` | WMS | 2 m/px |
| Częstochowa (1944) | `plany_miast_1944_IHPAN` | `czestochowa_2180` | WMS | 2 m/px |
| Gdańsk (1944) | `plany_miast_1944_IHPAN` | `gdansk_2180` | WMS | 2 m/px |
| Grudziądz (1944) | `plany_miast_1944_IHPAN` | `grudziadz_2180` | WMS | 2 m/px |
| Inowrocław (1943) | `plany_miast_1944_IHPAN` | `inowroclaw_2180` | WMS | 2 m/px |
| Jasło (1944) | `plany_miast_1944_IHPAN` | `jaslo_2180` | WMS | 2 m/px |
| Katowice (1944) | `plany_miast_1944_IHPAN` | `katowice_2180` | WMS | 2 m/px |
| Kielce (1944) | `plany_miast_1944_IHPAN` | `kielce_2180` | WMS | 2 m/px |
| Kraków (1944) | `plany_miast_1944_IHPAN` | `krakow_2180` | WMS | 2 m/px |
| Lublin (1944) | `plany_miast_1944_IHPAN` | `lublin_2180` | WMS | 2 m/px |
| Łódź (1944) | `plany_miast_1944_IHPAN` | `lodz_2180` | WMS | 2 m/px |
| Poznań (1944) | `plany_miast_1944_IHPAN` | `poznan_2180` | WMS | 2 m/px |
| Radom (1944) | `plany_miast_1944_IHPAN` | `radom_2180` | WMS | 2 m/px |
| Rzeszów (1944) | `plany_miast_1944_IHPAN` | `rzeszow_2180` | WMS | 2 m/px |
| Siedlce (1944) | `plany_miast_1944_IHPAN` | `siedlce_2180` | WMS | 2 m/px |
| Tarnów (1944) | `plany_miast_1944_IHPAN` | `tarnow_2180` | WMS | 2 m/px |
| Toruń (1944) | `plany_miast_1944_IHPAN` | `torun_2180` | WMS | 2 m/px |
| Warszawa (1944) | `plany_miast_1944_IHPAN` | `warszawa_2180` | WMS | 2 m/px |
| Warszawa (1822) | `mapy_historyczne_IHPAN` | `warszawaOKI_2180` | WMS | 2 m/px |
| Warszawa: plan Lindleyów 1:2500 (1897–1901) | `mapy_historyczne_IHPAN` | `lindley_3857` | WMS | 1 m/px |
| Warszawa-Praga (1796) | `mapy_historyczne_IHPAN` | `rauch_2180` | WMS | 2 m/px |
| Wrocław (1944) | `plany_miast_1944_IHPAN` | `wroclaw_3857` | WMS | 2 m/px |

---

## Ewidencja, geodezja i uzbrojenie

### Działki ewidencyjne (KIEG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Działki | `dzialki_ewid_EGIB` | `dzialki` | WMS | 0,5 m/px |
| Numery działek | `dzialki_ewid_EGIB` | `numery_dzialek` | WMS | 0,5 m/px |
| Budynki | `dzialki_ewid_EGIB` | `budynki` | WMS | 0,5 m/px |
| Użytki gruntowe | `dzialki_ewid_EGIB` | `uzytki` | WMS | 0,5 m/px |
| Kontury klasyfikacyjne | `dzialki_ewid_EGIB` | `kontury` | WMS | 0,5 m/px |
| Obręby ewidencyjne | `dzialki_ewid_EGIB` | `obreby` | WMS | 4 m/px |

### Uzbrojenie terenu (KIUT)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Sieć wodociągowa | `uzbrojenie_terenu_KIUT` | `przewod_wodociagowy` | WMS | 0,25 m/px |
| Sieć kanalizacyjna | `uzbrojenie_terenu_KIUT` | `przewod_kanalizacyjny` | WMS | 0,25 m/px |
| Sieć elektroenergetyczna | `uzbrojenie_terenu_KIUT` | `przewod_elektroenergetyczny` | WMS | 0,25 m/px |
| Sieć gazowa | `uzbrojenie_terenu_KIUT` | `przewod_gazowy` | WMS | 0,25 m/px |
| Sieć ciepłownicza | `uzbrojenie_terenu_KIUT` | `przewod_cieplowniczy` | WMS | 0,25 m/px |
| Sieć telekomunikacyjna | `uzbrojenie_terenu_KIUT` | `przewod_telekomunikacyjny` | WMS | 0,25 m/px |
| Sieć specjalna | `uzbrojenie_terenu_KIUT` | `przewod_specjalny` | WMS | 0,25 m/px |
| Sieci niezidentyfikowane | `uzbrojenie_terenu_KIUT` | `przewod_niezidentyfikowany` | WMS | 0,25 m/px |
| Urządzenia | `uzbrojenie_terenu_KIUT` | `przewod_urzadzenia` | WMS | 0,25 m/px |

### Osnowy geodezyjne

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Osnowa pozioma | `osnowy_geodezyjne` | `osnowa-XY` | WMS | 1 m/px |
| Osnowa wysokościowa | `osnowy_geodezyjne` | `osnowa-H` | WMS | 1 m/px |
| Osnowa pozioma: etykiety punktów | `osnowy_geodezyjne` | `osnowa-XYH` | WMS | 1 m/px |

---

## Granice i adresy

### Jednostki podziału kraju (PRG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Granica państwa | `rejestr_granic_jednostki_teryt` | `A00_Granice_panstwa` | WMS | 16 m/px |
| Granice województw | `rejestr_granic_jednostki_teryt` | `A01_Granice_wojewodztw` | WMS | 16 m/px |
| Granice powiatów | `rejestr_granic_jednostki_teryt` | `A02_Granice_powiatow` | WMS | 8 m/px |
| Granice gmin | `rejestr_granic_jednostki_teryt` | `A03_Granice_gmin` | WMS | 4 m/px |
| Granice miast | `rejestr_granic_jednostki_teryt` | `A04_Granice_miast` | WMS | 2 m/px |
| Granice jednostek ewidencyjnych | `rejestr_granic_jednostki_teryt` | `A05_Granice_jednostek_ewidencyjnych` | WMS | 2 m/px |
| Granice obrębów ewidencyjnych | `rejestr_granic_jednostki_teryt` | `A06_Granice_obrebow_ewidencyjnych` | WMS | 1 m/px |
| Granice rejonów statystycznych | `rejestr_granic_jednostki_teryt` | `R01_Granice_rejonow_statystycznych` | WMS | 2 m/px |
| Granice obwodów spisowych | `rejestr_granic_jednostki_teryt` | `R02_Granice_obwodow_spisowych` | WMS | 1 m/px |
| Mapa administracyjna GUGiK (przeglądowa) | `administracyjna` | `Administracyjna` | WMS | 16 m/px |

### Adresy i nazwy geograficzne (GUGiK)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Punkty adresowe (INSPIRE) | `rejestr_granic_punkty_adresowe` | `AD.Address` | WMS | 1 m/px |
| Urzędowe nazwy miejscowości (PRNG) | `rejestr_nazw_geograficznych` | `M1_UrzedoweNazwyMiejscowosci` | WMS | 4 m/px |
| Pozostałe nazwy miejscowości (PRNG) | `rejestr_nazw_geograficznych` | `M2_PozostaleNazwyMiejscowosci` | WMS | 2 m/px |
| Urzędowe nazwy obiektów fizjograficznych (PRNG) | `rejestr_nazw_geograficznych` | `O1_UrzedoweNazwyObiektowFizjograficznych` | WMS | 4 m/px |
| Zestandaryzowane nazwy obiektów fizjograficznych (PRNG) | `rejestr_nazw_geograficznych` | `O2_ZestandaryzowaneNazwyObiektowFizjograficznych` | WMS | 4 m/px |
| Pozostałe nazwy obiektów fizjograficznych (PRNG) | `rejestr_nazw_geograficznych` | `O3_PozostaleNazwyObiektowFizjograficznych` | WMS | 2 m/px |

### Sądy i prokuratury (PRG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Sądy apelacyjne | `rejestr_granic_jednostki_teryt` | `S01_Sad_apelacyjny` | WMS | 8 m/px |
| Sądy okręgowe | `rejestr_granic_jednostki_teryt` | `S02_Sad_okregowy` | WMS | 8 m/px |
| Sądy rejonowe | `rejestr_granic_jednostki_teryt` | `S03_Sad_rejonowy` | WMS | 8 m/px |
| Wojewódzkie sądy administracyjne | `rejestr_granic_jednostki_teryt` | `S04_Wojewodzki_sad_administracyjny` | WMS | 8 m/px |
| Prokuratury regionalne | `rejestr_granic_jednostki_teryt` | `P01_Prokuratura_regionalna` | WMS | 8 m/px |
| Prokuratury okręgowe | `rejestr_granic_jednostki_teryt` | `P02_Prokuratura_okregowa` | WMS | 8 m/px |
| Prokuratury rejonowe | `rejestr_granic_jednostki_teryt` | `P03_Prokuratura_rejonowa` | WMS | 8 m/px |

### Policja, straże i obrona cywilna (PRG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Komendy wojewódzkie policji | `rejestr_granic_jednostki_teryt` | `K01_Komenda_wojewodzka_policji` | WMS | 8 m/px |
| Komenda stołeczna policji | `rejestr_granic_jednostki_teryt` | `K03_Komenda_stoleczna_policji` | WMS | 4 m/px |
| Komendy powiatowe policji | `rejestr_granic_jednostki_teryt` | `K02_Komenda_powiatowa_policji` | WMS | 8 m/px |
| Komendy rejonowe policji | `rejestr_granic_jednostki_teryt` | `K04_Komenda_rejonowa_policji` | WMS | 4 m/px |
| Komisariaty policji | `rejestr_granic_jednostki_teryt` | `K05_Komisariat_policji` | WMS | 2 m/px |
| Komendy wojewódzkie straży pożarnej | `rejestr_granic_jednostki_teryt` | `K06_Komenda_wojewodzka_strazy_pozarnej` | WMS | 8 m/px |
| Komendy powiatowe straży pożarnej | `rejestr_granic_jednostki_teryt` | `K07_Komenda_powiatowa_strazy_pozarnej` | WMS | 8 m/px |
| Oddziały straży granicznej | `rejestr_granic_jednostki_teryt` | `K08_Oddzial_strazy_granicznej` | WMS | 8 m/px |
| Placówki straży granicznej | `rejestr_granic_jednostki_teryt` | `K09_Placowka_strazy_granicznej` | WMS | 4 m/px |
| Dywizjony straży granicznej | `rejestr_granic_jednostki_teryt` | `K10_Dywizjon_strazy_granicznej` | WMS | 8 m/px |
| Obrona cywilna: województwa | `rejestr_granic_jednostki_teryt` | `K11_Obszar_dzialania_szefa_obrony_cywilnej_wojewodztwa` | WMS | 8 m/px |
| Obrona cywilna: powiaty | `rejestr_granic_jednostki_teryt` | `K12_Obszar_dzialania_szefa_obrony_cywilnej_powiatu` | WMS | 8 m/px |
| Obrona cywilna: gminy | `rejestr_granic_jednostki_teryt` | `K13_Obszar_dzialania_szefa_obrony_cywilnej_gminy` | WMS | 4 m/px |

### Urzędy, lasy i wody (PRG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Izby administracji skarbowej | `rejestr_granic_jednostki_teryt` | `U05_Izba_administracji_skarbowej` | WMS | 8 m/px |
| Urzędy skarbowe | `rejestr_granic_jednostki_teryt` | `U02_Urzad_skarbowy` | WMS | 8 m/px |
| Wyspecjalizowane urzędy skarbowe | `rejestr_granic_jednostki_teryt` | `U03_Wyspecjalizowany_urzad_skarbowy` | WMS | 8 m/px |
| Urzędy celno-skarbowe | `rejestr_granic_jednostki_teryt` | `U04_Urzad_celno-skarbowy` | WMS | 8 m/px |
| Archiwa państwowe | `rejestr_granic_jednostki_teryt` | `U01_Archiwum_panstwowe` | WMS | 8 m/px |
| Regionalne dyrekcje Lasów Państwowych | `rejestr_granic_jednostki_teryt` | `U07_Regionalna_dyrekcja_lasow_panstwowych` | WMS | 8 m/px |
| Nadleśnictwa | `rejestr_granic_jednostki_teryt` | `U06_Nadlesnictwo` | WMS | 4 m/px |
| Regionalne zarządy gospodarki wodnej | `rejestr_granic_jednostki_teryt` | `U09_Regionalny_zarzad_gospodarki_wodnej_PGWWP` | WMS | 8 m/px |
| Zarządy zlewni (Wody Polskie) | `rejestr_granic_jednostki_teryt` | `U08_Zarzad_zlewni_PGWWP` | WMS | 4 m/px |
| Urzędy żeglugi śródlądowej | `rejestr_granic_jednostki_teryt` | `U11_Urzad_zeglugi_srodladowej` | WMS | 8 m/px |

### Obszary morskie (PRG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Morskie wody wewnętrzne | `rejestr_granic_jednostki_teryt` | `W03_Morskie_wody_wewnetrzne` | WMS | 4 m/px |
| Morze terytorialne RP | `rejestr_granic_jednostki_teryt` | `W02_Morze_terytorialne_RP` | WMS | 8 m/px |
| Linia podstawowa morza terytorialnego | `rejestr_granic_jednostki_teryt` | `W01_Linia_podstawowa_morza_terytorialnego` | WMS | 4 m/px |
| Strefa przyległa | `rejestr_granic_jednostki_teryt` | `W05_Strefa_przylegla` | WMS | 8 m/px |
| Wyłączna strefa ekonomiczna | `rejestr_granic_jednostki_teryt` | `W04_Wylaczna_strefa_ekonomiczna` | WMS | 16 m/px |
| Morska linia brzegowa | `rejestr_granic_jednostki_teryt` | `W06_Morskie_linie_brzegowe` | WMS | 2 m/px |
| Pas techniczny | `rejestr_granic_jednostki_teryt` | `W09_Pas_techniczny` | WMS | 2 m/px |
| Pas ochronny | `rejestr_granic_jednostki_teryt` | `W08_Pas_ochronny` | WMS | 2 m/px |
| Pas nadbrzeżny | `rejestr_granic_jednostki_teryt` | `W07_Pas_nadbrzezny` | WMS | 2 m/px |
| Urzędy morskie | `rejestr_granic_jednostki_teryt` | `U10_Urzad_morski` | WMS | 8 m/px |
| Porty morskie | `rejestr_granic_jednostki_teryt` | `W11_Port_morski` | WMS | 2 m/px |
| Przystanie morskie | `rejestr_granic_jednostki_teryt` | `W12_Przystan_morska` | WMS | 2 m/px |
| Redy | `rejestr_granic_jednostki_teryt` | `W10_Reda` | WMS | 4 m/px |

---

## Przyroda i środowisko

### Formy ochrony przyrody (GDOŚ)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Parki narodowe | `obszary_chronione_GDOS` | `GDOS:ParkiNarodowe` | WMS | 4 m/px |
| Parki krajobrazowe | `obszary_chronione_GDOS` | `GDOS:ParkiKrajobrazowe` | WMS | 4 m/px |
| Rezerwaty przyrody | `obszary_chronione_GDOS` | `GDOS:Rezerwaty` | WMS | 2 m/px |
| Natura 2000: obszary ptasie (OSO) | `obszary_chronione_GDOS` | `GDOS:ObszarySpecjalnejOchrony` | WMS | 4 m/px |
| Natura 2000: obszary siedliskowe (SOO) | `obszary_chronione_GDOS` | `GDOS:SpecjalneObszaryOchrony` | WMS | 4 m/px |
| Obszary chronionego krajobrazu | `obszary_chronione_GDOS` | `GDOS:ObszaryChronionegoKrajobrazu` | WMS | 4 m/px |
| Zespoły przyrodniczo-krajobrazowe | `obszary_chronione_GDOS` | `GDOS:ZespolyPrzyrodniczoKrajobrazowe` | WMS | 2 m/px |
| Użytki ekologiczne | `obszary_chronione_GDOS` | `GDOS:UzytkiEkologiczne` | WMS | 2 m/px |
| Stanowiska dokumentacyjne | `obszary_chronione_GDOS` | `GDOS:StanowiskaDokumentacyjne` | WMS | 2 m/px |
| Pomniki przyrody | `obszary_chronione_GDOS` | `GDOS:PomnikiPrzyrody` | WMS | 1 m/px |

### Lasy (Bank Danych o Lasach)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Regionalne dyrekcje Lasów Państwowych (RDLP) | `Lasy_Panstwowe_BDL` | `6` | WMS | 32 m/px |
| Nadleśnictwa | `Lasy_Panstwowe_BDL` | `5` | WMS | 16 m/px |
| Leśnictwa | `Lasy_Panstwowe_BDL` | `4` | WMS | 8 m/px |
| Oddziały leśne (Lasy Państwowe) | `Lasy_Panstwowe_BDL` | `3` | WMS | 2 m/px |
| Wydzielenia leśne (Lasy Państwowe) | `Lasy_Panstwowe_BDL` | `1` | WMS | 2 m/px |
| Oddziały poza Lasami Państwowymi | `Lasy_Panstwowe_BDL` | `2` | WMS | 4 m/px |
| Wydzielenia poza Lasami Państwowymi | `Lasy_Panstwowe_BDL` | `0` | WMS | 4 m/px |

### Mapa hydrograficzna (GUGiK)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa hydrograficzna 1:50 000 | `hydrograficzna_50` | `Raster` | WMS | 4 m/px |
| Mapa hydrograficzna 1:50 000 (enviDMS) | `hydrograficzna_50` | `Raster_enviDMS_50k` | WMS | 4 m/px |

### Mapa sozologiczna (GUGiK)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa sozologiczna 1:50 000 (stan i ochrona środowiska) | `sozologiczna_50` | `0` | WMS | 4 m/px |

### Mapa glebowo-rolnicza 1:5000 (GUGiK)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Kontury glebowe | `mapa_glebowo_rolnicza` | `0` | WMS | 1 m/px |
| Odkrywki glebowe | `mapa_glebowo_rolnicza` | `2` | WMS | 1 m/px |

### Regiony fizycznogeograficzne (PIG-PIB)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Makroregiony fizycznogeograficzne | `regiony_fizycznogeograficzne` | `1` | WMS | 50 m/px |
| Mezoregiony fizycznogeograficzne | `regiony_fizycznogeograficzne` | `0` | WMS | 25 m/px |

---

## Geologia (PIG-PIB)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Szczegółowa mapa geologiczna Polski 1:50 000 | `geologiczna_50_SMGP` | `0` | WMS | 4 m/px |
| Mapa litogenetyczna Polski 1:50 000 | `geologiczna_50_litogenetyczna` | `1` | WMS | 8 m/px |
| Mapa geologiczna Polski 1:500 000 | `geologiczna_500` | `0` | WMS | 70 m/px |

---

## Zbiory archiwalne

W podfolderze `warstwy\_archiwalne\` przechowywane są wycofane lub zastąpione definicje serwerów (nie pojawiają się na liście w programie, aby nie zaśmiecać drzewa). Nadal jednak nazwy historycznych warstw działają przy otwieraniu dawnych plików `conf.txt`.

* `BDO_topograficzna`
* `BDOT_wszystkie_warstwy`
* `BDOT_wybrane_warstwy`
* `budynki_BDOT_2009`
* `budynki_BDOT_2010`
* `dzialki_ewid_LPIS`
* `Hgis_100_grossblatt`
* `Hgis_100_Karte_des_deutschen_reiches`
* `Hgis_100_WIG_taktyczna`
* `Hgis_25_messtishblatt`
* `Hgis_25_WIG_topograficzna`
* `hydrogeologiczna_50`
* `numer_mod_terenu_ISOK_cieniowanie`
* `numer_mod_terenu_LPIS_cieniowanie`
* `numer_mod_terenu_LPIS_hipsometria`
* `rejestr_nazw_geograficznych` (dawna wersja serwera WMS PRNG, w programie aktywny jest nowszy zbiór PRNG)
* `sieci_transportowe`
* `stacje_refer`

---

## Warunki korzystania i atrybucja

Mapy pochodzą z publicznych rejestrów i projektów społecznościowych (GUGiK, PIG-PIB, GDOŚ, NID, IHPAN, Bank Danych o Lasach, OpenStreetMap, OSMapa.pl, UMP-pcPL, Waymarked Trails). Przed publikacją lub komercyjnym wykorzystaniem pobranych map sprawdź licencję dostawcy danych i zachowaj informację o źródle (MapoTero automatycznie zapisuje ją w `conf.txt`, metadanych MBTiles/KMZ oraz opisie CAD).

---

➡️ **Kolejny krok:** [Współpraca z innymi programami GIS i CAD →](../inne-programy/)

*Zobacz też: [Dodawanie serwerów map](../serwery/) · [Ochrona serwerów map](../pobieranie/serwery/) · [Okno główne](../okno-glowne/#zbiory-map-i-warstwy)*
