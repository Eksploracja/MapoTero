---
title: Katalog zbiorów map
nav_order: 17
---

# Katalog zbiorów map
{: .no_toc }

Opis 71 zbiorów map dostarczanych z programem: skąd pochodzą, w jakich grupach drzewa je znaleźć, jakiego są typu (WMS / WMTS / XYZ) i jaki mają zalecany rozmiar piksela.
{: .fs-5 .fw-300 }

**Gdzie w programie:** karta *Warstwy do pobrania* w oknie głównym (zobacz też [Galerię warstw online](../galeria-warstw/))  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Jak czytać katalog

W drzewie *Warstwy do pobrania* warstwy nie są ułożone według surowych nazw plików, lecz pogrupowane tematycznie przez plik `warstwy\katalog.txt` (UTF-8) i opatrzone czytelnymi nazwami. Wiersz `> Grupa` otwiera grupę, `>> Podgrupa` otwiera podgrupę, a wiersz `zbiór | warstwa | nazwa` dodaje do niej warstwę ze zbioru map, np.:

```text
> Przyroda i środowisko
>> Lasy (Bank Danych o Lasach)
Lasy_Panstwowe_BDL | 5 | Nadleśnictwa
```

Zbiory i warstwy niewymienione w `katalog.txt` (np. dodane [kreatorem serwerów](../serwery/kreator/)) pojawiają się na końcu drzewa w grupie **Dodane serwery**.

## Przegląd wszystkich zbiorów

| Plik zbioru | Źródło / instytucja | Typ | Warstw | Zalecany piksel |
|---|---|---|---|---|
| `administracyjna` | GUGiK / Geoportal | WMS | 1 | 16 m/px |
| `baltyk_batymetria_wms` | EMODnet (Batymetria Bałtyku) | WMS | 4 | 10 m/px |
| `BDOO_WMTS` | GUGiK / Geoportal | WMTS | 1 | 25 m/px |
| `BDOT10k_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `czechy_orto_WMTS` | Czechy (ČÚZK) | WMTS | 1 | 0,5 m/px |
| `czechy_orto_wms` | Czechy (ČÚZK) | WMS | 1 | 0,5 m/px |
| `czechy_topo_WMTS` | Czechy (ČÚZK) | WMTS | 1 | 2 m/px |
| `czechy_ztm10_wms` | Czechy (ČÚZK) | WMS | 1 | 1 m/px |
| `czechy_ztm25_wms` | Czechy (ČÚZK) | WMS | 1 | 2,5 m/px |
| `czechy_ztm50_wms` | Czechy (ČÚZK) | WMS | 1 | 5 m/px |
| `dzialki_ewid_EGIB` | GUGiK / Geoportal | WMS | 6 | 0,5–4 m/px |
| `geologiczna_500` | PIG-PIB (Geologia) | WMS | 1 | 70 m/px |
| `geologiczna_50_litogenetyczna` | PIG-PIB (Geologia) | WMS | 1 | 8 m/px |
| `geologiczna_50_SMGP` | PIG-PIB (Geologia) | WMS | 1 | 4 m/px |
| `hydrograficzna_50` | GUGiK / Geoportal | WMS | 2 | 4 m/px |
| `krajobrazowa` | GUGiK / Geoportal | WMS | 1 | 16 m/px |
| `Lasy_Panstwowe_BDL` | Lasy Państwowe (BDL) | WMS | 7 | 2–32 m/px |
| `Lasy_Panstwowe_Lowiectwo` | Lasy Państwowe (BDL) | WMS | 1 | 8 m/px |
| `Lasy_Panstwowe_Ochronnosc` | Lasy Państwowe (BDL) | WMS | 9 | 2–4 m/px |
| `Lasy_Panstwowe_Wlasnosc` | Lasy Państwowe (BDL) | WMS | 2 | 2 m/px |
| `Lasy_Panstwowe_Siedliska` | Lasy Państwowe (BDL) | WMS | 1 | 4 m/px |
| `Lasy_Panstwowe_Drzewostany` | Lasy Państwowe (BDL) | WMS | 6 | 2 m/px |
| `Lasy_Panstwowe_Dojazdy_Ppoz` | Lasy Państwowe (BDL) | WMS | 3 | 2–4 m/px |
| `Lasy_Panstwowe_Turystyka` | Lasy Państwowe (BDL) | WMS | 6 | 2–4 m/px |
| `litwa_inspire_wms` | Litwa (INSPIRE Geoportal) | WMS | 4 | 1–2 m/px |
| `mapa_glebowo_rolnicza` | GUGiK / Geoportal | WMS | 2 | 1 m/px |
| `mapy_historyczne_IHPAN` | IHPAN / Pastmaps | WMS | 22 | 2–40 m/px |
| `niemcy_basemapde_WMTS` | Niemcy (BKG basemap.de) | WMTS | 2 | 2 m/px |
| `niemcy_brandenburgia_dgm_wms` | Niemcy (Brandenburgia LGB) | WMS | 1 | 2 m/px |
| `niemcy_brandenburgia_dtk10_wms` | Niemcy (Brandenburgia LGB) | WMS | 1 | 1 m/px |
| `niemcy_brandenburgia_dtk25_wms` | Niemcy (Brandenburgia LGB) | WMS | 1 | 2 m/px |
| `niemcy_brandenburgia_dtk50_wms` | Niemcy (Brandenburgia LGB) | WMS | 1 | 5 m/px |
| `niemcy_brandenburgia_sdop_wms` | Niemcy (Brandenburgia LGB) | WMS | 1 | 0,5 m/px |
| `niemcy_saksonia_dop_wms` | Niemcy (Saksonia GeoSN) | WMS | 1 | 0,5 m/px |
| `niemcy_saksonia_dtk_wms` | Niemcy (Saksonia GeoSN) | WMS | 2 | 2,5–5 m/px |
| `niemcy_saksonia_hoehe_wms` | Niemcy (Saksonia GeoSN) | WMS | 2 | 2 m/px |
| `numer_mod_terenu_cieniowanie_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `numer_mod_terenu_hipsometria_WMTS` | GUGiK / Geoportal | WMTS | 1 | 1 m/px |
| `numer_mod_terenu_WMTS` | GUGiK / Geoportal | WMTS | 1 | 2 m/px |
| `obszary_chronione_GDOS` | GDOŚ | WMS | 10 | 1–4 m/px |
| `OpenStreetMap_Terrestris` | terrestris (OSM, znak wodny) | WMS | 1 | 10 m/px |
| `OpenTopoMap` | OpenTopoMap / OSM | XYZ | 1 | 2 m/px |
| `ortofotomapa` | GUGiK / Geoportal | WMS | 1 | 1 m/px |
| `ortofotomapa_HighResolution_WMTS` | GUGiK / Geoportal | WMTS | 1 | 0,1 m/px |
| `ortofotomapa_TrueOrtho` | GUGiK / Geoportal | WMS | 1 | 0,1 m/px |
| `ortofotomapa_WMTS` | GUGiK / Geoportal | WMTS | 1 | 0,25 m/px |
| `OSMapa_PL` | OSMapa.pl / OSM | XYZ | 1 | 2 m/px |
| `osnowy_geodezyjne` | GUGiK / Geoportal | WMS | 3 | 1 m/px |
| `regiony_fizycznogeograficzne` | PIG-PIB (Geologia) | WMS | 2 | 25–50 m/px |
| `rejestr_granic_jednostki_teryt` | GUGiK / Geoportal | WMS | 22 | 1–16 m/px |
| `rejestr_granic_punkty_adresowe` | GUGiK / Geoportal | WMS | 1 | 1 m/px |
| `rejestr_nazw_geograficznych` | GUGiK / Geoportal | WMS | 5 | 2–4 m/px |
| `skany_map_topograficznych` | GUGiK / Geoportal | WMS | 9 | 1–4 m/px |
| `slowacja_dmr_wms` | Słowacja (ÚGKK / ZBGIS) | WMS | 1 | 2 m/px |
| `slowacja_freemap` | Słowacja (Freemap Outdoor) | XYZ | 1 | 2 m/px |
| `slowacja_orto_wms` | Słowacja (ÚGKK / ZBGIS) | WMS | 1 | 0,5 m/px |
| `slowacja_zmsr_wms` | Słowacja (ÚGKK / ZBGIS) | WMS | 4 | 2–15 m/px |
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

## Mapy topograficzne

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
| Skala automatyczna | `TOPO_WMTS` | `MAPA TOPOGRAFICZNA` | WMTS | 2 m/px |

### Mapy społecznościowe i turystyczne (kafle XYZ)

{: .wskazowka }
Warstwy *Waymarked Trails* są przezroczystymi nakładkami. Po dodaniu mapy podkładowej (np. *OSMapa.pl* lub skanów GUGiK) program automatycznie umieszcza podkład pod szlakami.

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| OSMapa.pl – mapa topograficzno-turystyczna (OpenStreetMap) | `OSMapa_PL` | `osmapa` | XYZ | 2 m/px |
| UMP-pcPL – Uzupełniająca Mapa Polski | `UMP_pcPL` | `ump_pcPL` | XYZ | 2 m/px |
| OpenTopoMap – mapa topograficzna Europy (poziomice i cieniowanie) | `OpenTopoMap` | `opentopomap` | XYZ | 2 m/px |
| Szlaki piesze – nakładka (Waymarked Trails) | `Waymarked_Trails` | `hiking` | XYZ | 2 m/px |
| Szlaki rowerowe – nakładka (Waymarked Trails) | `Waymarked_Trails` | `cycling` | XYZ | 2 m/px |
| Szlaki MTB – nakładka (Waymarked Trails) | `Waymarked_Trails` | `mtb` | XYZ | 2 m/px |
| Szlaki konne – nakładka (Waymarked Trails) | `Waymarked_Trails` | `riding` | XYZ | 2 m/px |
| Trasy rolkowe – nakładka (Waymarked Trails) | `Waymarked_Trails` | `skating` | XYZ | 2 m/px |
| Trasy narciarskie – nakładka (Waymarked Trails) | `Waymarked_Trails` | `slopes` | XYZ | 2 m/px |

## Mapy topograficzne

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Baza danych obiektów topograficznych BDOT10k (kafle WMTS) | `BDOT10k_WMTS` | `BDOT10k` | WMTS | 1 m/px |
| Mapa ogólnogeograficzna BDOO z cieniowaniem (kafle WMTS) | `BDOO_WMTS` | `BDOOCieniowanie` | WMTS | 25 m/px |
| Mapa krajobrazowa GUGiK (przeglądowa) | `krajobrazowa` | `Krajobrazowa` | WMS | 16 m/px |

## Rzeźba terenu

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Cieniowanie rzeźby NMT 1 m (kafle WMTS) | `numer_mod_terenu_cieniowanie_WMTS` | `ISOK_Cien` | WMTS | 1 m/px |
| Hipsometria NMT 1 m – kolory wysokości (kafle WMTS) | `numer_mod_terenu_hipsometria_WMTS` | `Hipsometria` | WMTS | 1 m/px |
| Hipsometria z cieniowaniem NMT 1 m (kafle WMTS) | `numer_mod_terenu_WMTS` | `HipsoCien` | WMTS | 2 m/px |
| Poziomice SRTM 30 m (terrestris, ze znakiem wodnym) | `OpenStreetMap_Terrestris` | `SRTM30-Contour` | WMS | 10 m/px |
| Szczyty górskie i wzniesienia (GUGiK) | `szczyty` | `szczyty` | WMS | 4 m/px |

## Ortofotomapy

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Ortofotomapa standardowa 25 cm (kafle WMTS) | `ortofotomapa_WMTS` | `ORTOFOTOMAPA` | WMTS | 0,25 m/px |
| Ortofotomapa wysokiej rozdzielczości do 10 cm (kafle WMTS) | `ortofotomapa_HighResolution_WMTS` | `ORTOFOTOMAPA` | WMTS | 0,1 m/px |
| Ortofotomapa wysokiej rozdzielczości (WMS) | `ortofotomapa` | `Raster` | WMS | 1 m/px |
| Prawdziwa ortofotomapa (TrueOrtho) | `ortofotomapa_TrueOrtho` | `PrawdziwaOrtofotomapa` | WMS | 0,1 m/px |

## Mapy historyczne i zabytki

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
| Mapa taktyczna WIG 1:100 000 (1918–1939) – zasięg rozszerzony | `mapy_historyczne_IHPAN` | `wig100k_4326_extended` | WMS | 8 m/px |
| Mapa Polski i krajów ościennych WIG 1:500 000 (1939) | `mapy_historyczne_IHPAN` | `wig500k1939_3857` | WMS | 40 m/px |
| Mapa WIG 1:100 000 w układzie „Borowa Góra” (1947–1954) | `mapy_historyczne_IHPAN` | `wig_1947_1952_3857` | WMS | 8 m/px |

### Zabytki i dziedzictwo (NID)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Rejestr zabytków nieruchomych – zasięgi i typy | `zabytki_zasiegi_NID` | `Zabytki_nieruchome_I` | WMS | 1 m/px |
| Rejestr zabytków nieruchomych (INSPIRE) | `zabytki_NID` | `Immovable_Monuments` | WMS | 1 m/px |
| Rejestr zabytków archeologicznych | `zabytki_archeo_NID` | `Archaeological_Monuments` | WMS | 2 m/px |
| Ewidencja zabytków nieruchomych | `zabytki_ewidencja_nieruchomych_NID` | `Ewidencja_zabytkow_nieruchomych_I` | WMS | 1 m/px |
| Ewidencja zabytków archeologicznych | `zabytki_ewidencja_archeo_NID` | `Ewidencja_zabytków_archeologicznych` | WMS | 2 m/px |
| Pomniki Historii | `zabytki_pomniki_historii_NID` | `Monuments_of_History` | WMS | 4 m/px |
| Obiekty światowego dziedzictwa UNESCO | `zabytki_unesco_NID` | `World_Heritage_Properties_List` | WMS | 4 m/px |
| Groby i cmentarze wojenne | `zabytki_groby_wojenne_NID` | `Groby_i_cmentarze_wojenne_I` | WMS | 1 m/px |

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
| Osnowa pozioma – etykiety punktów | `osnowy_geodezyjne` | `osnowa-XYH` | WMS | 1 m/px |

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

## Przyroda i środowisko

### Formy ochrony przyrody (GDOŚ)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Parki narodowe | `obszary_chronione_GDOS` | `GDOS:ParkiNarodowe` | WMS | 4 m/px |
| Parki krajobrazowe | `obszary_chronione_GDOS` | `GDOS:ParkiKrajobrazowe` | WMS | 4 m/px |
| Rezerwaty przyrody | `obszary_chronione_GDOS` | `GDOS:Rezerwaty` | WMS | 2 m/px |
| Natura 2000 – obszary ptasie (OSO) | `obszary_chronione_GDOS` | `GDOS:ObszarySpecjalnejOchrony` | WMS | 4 m/px |
| Natura 2000 – obszary siedliskowe (SOO) | `obszary_chronione_GDOS` | `GDOS:SpecjalneObszaryOchrony` | WMS | 4 m/px |
| Obszary chronionego krajobrazu | `obszary_chronione_GDOS` | `GDOS:ObszaryChronionegoKrajobrazu` | WMS | 4 m/px |
| Zespoły przyrodniczo-krajobrazowe | `obszary_chronione_GDOS` | `GDOS:ZespolyPrzyrodniczoKrajobrazowe` | WMS | 2 m/px |
| Użytki ekologiczne | `obszary_chronione_GDOS` | `GDOS:UzytkiEkologiczne` | WMS | 2 m/px |
| Stanowiska dokumentacyjne | `obszary_chronione_GDOS` | `GDOS:StanowiskaDokumentacyjne` | WMS | 2 m/px |
| Pomniki przyrody | `obszary_chronione_GDOS` | `GDOS:PomnikiPrzyrody` | WMS | 1 m/px |

### Lasy Państwowe (BDL)

### > Leśny podział terytorialny (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Regionalne dyrekcje Lasów Państwowych (RDLP) | `Lasy_Panstwowe_BDL` | `6` | WMS | 32 m/px |
| Nadleśnictwa | `Lasy_Panstwowe_BDL` | `5` | WMS | 16 m/px |
| Leśnictwa | `Lasy_Panstwowe_BDL` | `4` | WMS | 8 m/px |
| Oddziały leśne (Lasy Państwowe) | `Lasy_Panstwowe_BDL` | `3` | WMS | 2 m/px |
| Wydzielenia leśne (Lasy Państwowe) | `Lasy_Panstwowe_BDL` | `1` | WMS | 2 m/px |
| Oddziały poza Lasami Państwowymi | `Lasy_Panstwowe_BDL` | `2` | WMS | 4 m/px |
| Wydzielenia poza Lasami Państwowymi | `Lasy_Panstwowe_BDL` | `0` | WMS | 4 m/px |

### > Turystyka w lasach (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Obszar „Zanocuj w lesie” (biwaki) | `Lasy_Panstwowe_Turystyka` | `0` | WMS | 4 m/px |
| Miejsca postoju, wiaty i ogniska | `Lasy_Panstwowe_Turystyka` | `15` | WMS | 2 m/px |
| Powierzchniowe obiekty rekreacyjne | `Lasy_Panstwowe_Turystyka` | `17` | WMS | 4 m/px |
| Ścieżki dydaktyczne | `Lasy_Panstwowe_Turystyka` | `1` | WMS | 2 m/px |
| Ścieżki konne | `Lasy_Panstwowe_Turystyka` | `4` | WMS | 4 m/px |
| Ośrodki edukacyjne i izby leśne | `Lasy_Panstwowe_Turystyka` | `13` | WMS | 2 m/px |

### > Drogi leśne i dojazdy pożarowe (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Dojazdy pożarowe (drogi leśne z numeracją) | `Lasy_Panstwowe_Dojazdy_Ppoz` | `0` | WMS | 2 m/px |
| Punkty czerpania wody | `Lasy_Panstwowe_Dojazdy_Ppoz` | `1` | WMS | 4 m/px |
| Lokalizacje pożarów | `Lasy_Panstwowe_Dojazdy_Ppoz` | `2` | WMS | 4 m/px |

### > Drzewostany i siedliska leśne (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Skład gatunkowy drzewostanów (LP) | `Lasy_Panstwowe_Drzewostany` | `1` | WMS | 2 m/px |
| Skład gatunkowy (poza LP wg PUL) | `Lasy_Panstwowe_Drzewostany` | `0` | WMS | 2 m/px |
| Klasy odnowienia KO i KDO (LP) | `Lasy_Panstwowe_Drzewostany` | `3` | WMS | 2 m/px |
| Klasy odnowienia KO i KDO (poza LP) | `Lasy_Panstwowe_Drzewostany` | `2` | WMS | 2 m/px |
| Wydzielenia niezalesione (LP) | `Lasy_Panstwowe_Drzewostany` | `5` | WMS | 2 m/px |
| Wydzielenia niezalesione (poza LP) | `Lasy_Panstwowe_Drzewostany` | `4` | WMS | 2 m/px |
| Siedliska leśne (typy TSL) | `Lasy_Panstwowe_Siedliska` | `0` | WMS | 4 m/px |

### > Własność i ochrona lasów (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Lasy Państwowe (PGL LP) | `Lasy_Panstwowe_Wlasnosc` | `1` | WMS | 2 m/px |
| Lasy pozostałe (prywatne, PN, inne) | `Lasy_Panstwowe_Wlasnosc` | `0` | WMS | 2 m/px |
| Lasy wodochronne | `Lasy_Panstwowe_Ochronnosc` | `1` | WMS | 4 m/px |
| Lasy podmiejskie (wokół miast) | `Lasy_Panstwowe_Ochronnosc` | `2` | WMS | 4 m/px |
| Cenne fragmenty rodzimej przyrody | `Lasy_Panstwowe_Ochronnosc` | `3` | WMS | 4 m/px |
| Uszkodzone przez przemysł | `Lasy_Panstwowe_Ochronnosc` | `4` | WMS | 4 m/px |
| Lasy glebochronne | `Lasy_Panstwowe_Ochronnosc` | `5` | WMS | 4 m/px |
| Lasy obronne | `Lasy_Panstwowe_Ochronnosc` | `6` | WMS | 4 m/px |
| Lasy uzdrowiskowe | `Lasy_Panstwowe_Ochronnosc` | `7` | WMS | 4 m/px |
| Powierzchnie badawcze i doświadczalne | `Lasy_Panstwowe_Ochronnosc` | `8` | WMS | 4 m/px |
| Drzewostany nasienne (WDN/GDN) | `Lasy_Panstwowe_Ochronnosc` | `9` | WMS | 2 m/px |

### > Obwody łowieckie (BDL)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Obwody łowieckie | `Lasy_Panstwowe_Lowiectwo` | `0` | WMS | 8 m/px |

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

## Geologia (PIG-PIB)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Szczegółowa mapa geologiczna Polski 1:50 000 | `geologiczna_50_SMGP` | `0` | WMS | 4 m/px |
| Mapa litogenetyczna Polski 1:50 000 | `geologiczna_50_litogenetyczna` | `1` | WMS | 8 m/px |
| Mapa geologiczna Polski 1:500 000 | `geologiczna_500` | `0` | WMS | 70 m/px |

## Kraje sąsiednie i strefa przygraniczna

### Czechy (ČÚZK)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa topograficzna wieloskalowa (ČÚZK, kafle WMTS – każdy układ) | `czechy_topo_WMTS` | `ZTM_WM` | WMTS | 2 m/px |
| Skan mapy topograficznej 1:10 000 (ZTM 10, WMS – zalecany układ WGS84/UTM) | `czechy_ztm10_wms` | `0` | WMS | 1 m/px |
| Skan mapy topograficznej 1:25 000 (ZTM 25, WMS – zalecany układ WGS84/UTM) | `czechy_ztm25_wms` | `0` | WMS | 2,5 m/px |
| Skan mapy topograficznej 1:50 000 (ZTM 50, WMS – zalecany układ WGS84/UTM) | `czechy_ztm50_wms` | `0` | WMS | 5 m/px |
| Ortofotomapa lotnicza (Ortofoto ČR, kafle WMTS – każdy układ) | `czechy_orto_WMTS` | `ORTOFOTO_WM` | WMTS | 0,5 m/px |
| Ortofotomapa lotnicza (Ortofoto ČR, WMS – każdy układ, poza Czechami przezroczysta) | `czechy_orto_wms` | `0` | WMS | 0,5 m/px |

### Słowacja (ZBGIS i Freemap)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa turystyczna Karpat – szlaki i rzeźba (Freemap Slovakia, kafle XYZ – każdy układ) | `slowacja_freemap` | `outdoor` | XYZ | 2 m/px |
| Skan mapy topograficznej 1:10 000 (ZM10, WMS – zalecany układ WGS84/UTM) | `slowacja_zmsr_wms` | `1` | WMS | 2 m/px |
| Skan mapy topograficznej 1:25 000 (ZM25, WMS – tylko środkowa Słowacja, przy granicy z Polską puste; zalecany układ WGS84/UTM) | `slowacja_zmsr_wms` | `5` | WMS | 4 m/px |
| Skan mapy topograficznej 1:50 000 (ZM50, WMS – zalecany układ WGS84/UTM) | `slowacja_zmsr_wms` | `9` | WMS | 8 m/px |
| Skan mapy topograficznej 1:100 000 (ZM100, WMS – zalecany układ WGS84/UTM) | `slowacja_zmsr_wms` | `13` | WMS | 15 m/px |
| Ortofotomozaika SR (ZBGIS, WMS – zalecany układ WGS84/UTM) | `slowacja_orto_wms` | `1` | WMS | 0,5 m/px |
| Cieniowanie rzeźby terenu (DMR 5.0, WMS – zalecany układ WGS84/UTM) | `slowacja_dmr_wms` | `0` | WMS | 2 m/px |

### Niemcy – mapa ogólnokrajowa (BKG)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Mapa urzędowa basemap.de – kolorowa (BKG, kafle WMTS – każdy układ) | `niemcy_basemapde_WMTS` | `de_basemapde_web_raster_farbe` | WMTS | 2 m/px |
| Mapa urzędowa basemap.de – szara (BKG, kafle WMTS – każdy układ) | `niemcy_basemapde_WMTS` | `de_basemapde_web_raster_grau` | WMTS | 2 m/px |

### Niemcy – Brandenburgia (strefa Odry, LGB)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Skan mapy topograficznej 1:10 000 (DTK10, WMS – tylko układ WGS84, serwer odrzuca UTM i PL-1992) | `niemcy_brandenburgia_dtk10_wms` | `bb_dtk10_farbe` | WMS | 1 m/px |
| Skan mapy topograficznej 1:25 000 (DTK25, WMS – tylko układ WGS84, serwer odrzuca UTM i PL-1992) | `niemcy_brandenburgia_dtk25_wms` | `bb_dtk25_farbe` | WMS | 2 m/px |
| Skan mapy topograficznej 1:50 000 (DTK50, WMS – tylko układ WGS84, serwer odrzuca UTM i PL-1992) | `niemcy_brandenburgia_dtk50_wms` | `bb_dtk50_farbe` | WMS | 5 m/px |
| Ortofotomapa lotnicza 20 cm (SDOP, WMS – tylko układ WGS84, serwer odrzuca UTM i PL-1992) | `niemcy_brandenburgia_sdop_wms` | `bb_sdop` | WMS | 0,5 m/px |
| Cieniowanie reliefu NMT 1 m (DGMSHADE, WMS – tylko układ WGS84, serwer odrzuca UTM i PL-1992) | `niemcy_brandenburgia_dgm_wms` | `dgmshade` | WMS | 2 m/px |

### Niemcy – Saksonia (strefa Nysy Łużyckiej, GeoSN)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Skan mapy topograficznej 1:25 000 (DTK25, WMS – układ PL-1992, WGS84, UTM) | `niemcy_saksonia_dtk_wms` | `sn_dtk25_p_color` | WMS | 2,5 m/px |
| Skan mapy topograficznej 1:50 000 (DTK50, WMS – układ PL-1992, WGS84, UTM) | `niemcy_saksonia_dtk_wms` | `sn_dtk50_p_color` | WMS | 5 m/px |
| Ortofotomapa lotnicza 20 cm (DOP, WMS – układ PL-1992, WGS84, UTM) | `niemcy_saksonia_dop_wms` | `sn_dop_020` | WMS | 0,5 m/px |
| Cieniowanie rzeźby terenu (DGM, WMS – układ PL-1992, WGS84, UTM) | `niemcy_saksonia_hoehe_wms` | `relief_standard` | WMS | 2 m/px |
| Nachylenie stoków (GeoSN, WMS – układ PL-1992, WGS84, UTM) | `niemcy_saksonia_hoehe_wms` | `hangneigung` | WMS | 2 m/px |

### Litwa (INSPIRE)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Ewidencja gruntów – granice działek (INSPIRE, WMS – zalecany układ WGS84/UTM) | `litwa_inspire_wms` | `cp:CP.CadastralParcel` | WMS | 1 m/px |
| Warstwice poziomicowe terenu (INSPIRE, WMS – zalecany układ WGS84/UTM) | `litwa_inspire_wms` | `el:EL.ContourLine` | WMS | 2 m/px |
| Budynki i zabudowa (INSPIRE, WMS – zalecany układ WGS84/UTM) | `litwa_inspire_wms` | `bu:BU.Building_polygon` | WMS | 1 m/px |
| Sieć dróg i szlaków komunikacyjnych (INSPIRE, WMS – zalecany układ WGS84/UTM) | `litwa_inspire_wms` | `tn:TN.RoadTransportNetwork.RoadLink` | WMS | 2 m/px |

### Morze Bałtyckie (EMODnet Bathymetry)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| Rzeźba i głębokości dna Bałtyku – pełne pokrycie (WMS – zalecany układ WGS84/UTM) | `baltyk_batymetria_wms` | `emodnet:mean_atlas_land` | WMS | 10 m/px |
| Głębokość dna morskiego – paleta barwna (WMS – zalecany układ WGS84/UTM) | `baltyk_batymetria_wms` | `emodnet:mean_rainbowcolour` | WMS | 10 m/px |
| Izobaty – poziomice głębokości morskiej (WMS – zalecany układ WGS84/UTM) | `baltyk_batymetria_wms` | `emodnet:contours` | WMS | 10 m/px |
| Linia brzegowa Morza Bałtyckiego (WMS – zalecany układ WGS84/UTM) | `baltyk_batymetria_wms` | `coastlines` | WMS | 10 m/px |

### Europa Środkowa i Wschodnia (OpenTopoMap)

| Nazwa w drzewie | Zbiór map | Warstwa na serwerze | Typ | Piksel |
|---|---|---|---|---|
| OpenTopoMap – mapa topograficzna z poziomicami i cieniowaniem (Litwa, Ukraina, Białoruś, kafle XYZ – każdy układ) | `OpenTopoMap` | `opentopomap` | XYZ | 2 m/px |

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
* `rejestr_nazw_geograficznych`
* `sieci_transportowe`
* `stacje_refer`

## Warunki korzystania i atrybucja

Mapy pochodzą z publicznych rejestrów i projektów społecznościowych (GUGiK, PIG-PIB, GDOŚ, NID, IHPAN, Bank Danych o Lasach, OpenStreetMap, OSMapa.pl, UMP-pcPL, Waymarked Trails). Przed publikacją lub komercyjnym wykorzystaniem pobranych map sprawdź licencję dostawcy danych i zachowaj informację o źródle (MapoTero automatycznie zapisuje ją w `conf.txt`, metadanych MBTiles/KMZ oraz opisie CAD).

---

**Zobacz też:** [Dodawanie serwerów map](../serwery/) · [Ochrona serwerów map](../pobieranie/serwery/) · [Okno główne](../okno-glowne/#katalog-map-i-lista-warstw)
