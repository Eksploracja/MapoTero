---
title: Kreator serwerów WMS, WMTS, XYZ i TMS
nav_order: 1
parent: Dodawanie serwerów map
---

# Kreator serwerów WMS, WMTS, XYZ i TMS
{: .no_toc }

Kreator pobiera opis usługi (`GetCapabilities`, `TileJSON`, `tilemapresource.xml` lub szablon kafelków), pozwala sprawdzić
próbkę obrazu i zapisuje nowy zbiór map na liście programu.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Narzędzia > Dodaj nowy serwer WMS / WMTS...* lub przycisk **`[+]`** w oknie głównym  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

![Okno Kreator dodawania serwera mapowego]({{ '/pomoc/obrazy/13-kreator-serwerow.png' | relative_url }})

*Rysunek 1. Okno kreatora dodawania serwerów WMS, WMTS, XYZ i TMS.*

## Jak działa kreator (dwa widoki)

Okno kreatora działa dwuetapowo:

1. **Krok 1: Wybór usługi z katalogu lub wpisanie adresu URL**: domyślny widok po otwarciu.
2. **Krok 2: Analiza usługi, wybór warstw i podgląd próbki**: widok po wybraniu usługi lub pobraniu metadanych. W lewym górnym rogu znajduje się przycisk **← Wróć do listy usług**, który pozwala wrócić do katalogu bez zamykania okna.

---

## Krok 1: Wybór z katalogu usług lub własny URL

Okno otwiera się od razu na **Katalogu publicznych usług mapowych** z czterema źródłami do wyboru:

1. **Polecane usługi publiczne (wbudowane)**: ponad 100 sprawdzonych usług dodatkowych bez skorowidzów:
   * **GUGiK:** archiwalne ortofotomapy (od 1996 r.), kompozycje BDOT10k, BDOO, TOPO WMS, NMT cieniowanie i hipsometria WMS, K-GESUT, osnowy podstawowe i specjalne, usługi INSPIRE, KIMP, KIBDOT, wskaźnik NDVI,
   * **PGW Wody Polskie:** mapy zagrożenia powodziowego i hydrografia,
   * **PIG-PIB:** osuwiska SOPO, jaskinie, złoża kopalin MIDAS, geologia 1:200 000 / 1:500 000 (2022), geostanowiska, podtopienia, wody mineralne i lecznicze,
   * **GIOŚ, Ekoportal, IMGW, GUS:** pożary lasów, mapy hałasu, stacje pomiarowe,
   * **Kafelki XYZ z katalogu NextGIS QuickMapServices:** ESRI Imagery, Topo, Street i Hillshade, EOX Sentinel-2 (2024), OpenTopoMap, CartoDB Positron/Dark, OpenStreetMap DE / HOT / FR, CyclOSM, Freemap.sk, OpenRailwayMap, OpenSeaMap.
2. **Oficjalny wykaz Geoportal.gov.pl (online)**: aktualny wykaz usług publikowany przez GUGiK.
3. **EZiUDP: usługi centralne i wojewódzkie (online)**: rejestr usług urzędów centralnych i marszałkowskich (WODGiK).
4. **EZiUDP: powiaty i gminy (szukaj po nazwie / TERYT)**: wyszukiwarka lokalnych serwerów powiatowych (ODGiK) i gminnych (np. wpisz `Kraków`, `poznański` lub kod TERYT `1261`, `3021` i naciśnij Enter).

![Okno Katalog publicznych usług mapowych]({{ '/pomoc/obrazy/13-katalog-uslug.png' | relative_url }})

*Rysunek 2. Wbudowany katalog publicznych usług mapowych.*

Możesz filtrować listę po kategorii, ukryć usługi już obecne w programie (*Ukryj już w MapoTero*) oraz odświeżyć
zapamiętany wykaz przyciskiem **Odśwież z internetu**.

### Własny adres URL (ręczny)

Jeśli chcesz dodać serwer spoza katalogu:

* kliknij przycisk **+ Wpisz własny adres URL…** pod listą katalogu, **albo**
* po prostu **wklej adres** zaczynający się od `http://` lub `https://` w pole wyszukiwania katalogu. Program automatycznie przełączy widok na formularz adresu.

W polu adresu możesz podać:

* adres bazowy usługi **WMS** lub **WMTS** albo pełny link `GetCapabilities`,
* szablon kafelków **XYZ / TMS** zawierający `{z}`, `{x}`, `{y}` albo `{-y}` (także ze znacznikami subdomen `{a|b|c}`, `{switch:a,b,c}` lub `{quadkey}`),
* adres pliku **TileJSON** (`.json`) albo dokumentu OSGeo TMS (`tilemapresource.xml`).

Dla starszych serwerów powiatowych i gminnych z nieaktualnym certyfikatem zaznacz **Ignoruj błędy certyfikatów SSL (starsze serwery)** przed kliknięciem **Pobierz listę warstw**.

---

## Krok 2: Wybór warstw, parametry i próbka

Po kliknięciu **Połącz i wybierz warstwy** w katalogu (lub **Pobierz listę warstw** w widoku adresu) kreator analizuje usługę i otwiera panel konfiguracji warstw. Przycisk **← Wróć do listy usług** pozwala w każdej chwili wrócić do katalogu.

* pole **Filtruj warstwy** zawęża listę według nazwy technicznej lub tytułu warstwy,
* zaznacz wybrane warstwy (albo użyj przycisków **Zaznacz wszystkie** / **Odznacz wszystkie**).

{: .wskazowka }
Nie zaznaczaj warstw oznaczonych jako skorowidze lub siatki arkuszy, jeśli chcesz pobierać tylko treść mapy.

---

## Szczegóły warstwy i parametry XYZ/TMS

Kliknięcie dowolnej warstwy na drzewie wypełnia prawy panel szczegółów:

* **Nazwa techniczna**, **Tytuł** i **Opis warstwy** pobrane z metadanych usługi,
* **Obsługiwane układy** (sprawdź, czy jest `EPSG:2180` dla PL-1992 lub odpowiedni PL-2000),
* **Skala min/max (NMT)**: podpowiada, w jakiej skali warstwa w ogóle rysuje treść (poza nią serwer zwraca puste kafle),
* **Zalecany piksel [m/pix]** (`0,01–500`): wyliczany automatycznie ze skali minimalnej warstwy (możesz go poprawić ręcznie).

Dla usług **XYZ / TMS** pojawiają się dodatkowe pola:

* **Zoom (`zMin` do `zMax`)**: zakres poziomów powiększeń (`0–22`) udostępniany przez serwer,
* **Oś Y TMS (-y)**: włącz, jeśli serwer numeruje wiersze kafli od południa (standard OSGeo TMS),
* **Nakładka (szlaki)**: zaznacz dla warstw przezroczystych bez pełnego pokrycia. Odpowiedź `HTTP 404` oznacza wtedy pusty kafel zamiast błędu.

---

## Sprawdzenie warstwy

Przycisk **Sprawdź warstwę...** pobiera na próbę pojedynczy kafelek `256 × 256 px` (z okolicy aktualnego środka mapy
w oknie głównym) i wyświetla go w ramce *Podgląd próbki*. To najszybszy sposób, aby upewnić się przed zapisem, że serwer działa i rysuje treść.

---

## Zapis zbioru map

1. Wpisz **Nazwę nowego zbioru map** (będzie to nazwa pliku `.txt` bez polskich znaków i spacji, np. `powiat_ewidencja`).
2. Kliknij **Dodaj zbiór do MapoTero**.

Program zapisze plik zbioru w folderze `warstwy\` (obok `MapoTero.exe`, a przy braku uprawnień do zapisu w folderze `%LocalAppData%\MapoTero\warstwy`),
dopisze go do `warstwy.txt` i od razu pokaże w drzewie *Wybierz mapy* w grupie **Dodane serwery**.
Warstwy zapisane kreatorem zachowują w grupie *Dodane serwery* oraz na liście wybranych warstw swoje czytelne nazwy
pobrane z metadanych serwera. Jeśli usługa wymagała dodatkowych parametrów (XYZ/TMS, subdomeny, limit zapytań, nagłówek referer),
plik zbioru zostanie automatycznie sformatowany jako UTF-8 JSON.

---

➡️ **Kolejny krok:** [Budowa pliku zbioru map →](../plik-zbioru-map/)

*Zobacz też: [Katalog zbiorów map](../../zbiory-map/) · [Usługi kafelkowe](../../pobieranie/wmts/)*
