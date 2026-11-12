---
title: Usługi kafelkowe (WMTS, XYZ, TMS)
nav_order: 3
parent: Pobieranie map
---

# Usługi kafelkowe (WMTS, XYZ, TMS)
{: .no_toc }

Z usług kafelkowych WMTS, XYZ i TMS program sam wybiera poziom kafli, składa z nich segmenty i zapisuje je
z takimi samymi plikami kalibracji jak przy WMS.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Czym charakteryzują się usługi kafelkowe

Serwer kafelkowy udostępnia gotowe, przygotowane wcześniej **kafle** (zwykle 256 × 256 px) w stałych poziomach szczegółowości:

* **WMTS** (*Web Map Tile Service*): oficjalny standard OGC opisany dokumentem `GetCapabilities` (np. *Ortofotomapa standardowa 25 cm (kafle WMTS)*, *Mapa topograficzna GUGiK (kafle WMTS)*, *BDOT10k_WMTS*). Program rozpoznaje usługę po adresie zawierającym `SERVICE=WMTS` lub `/WMTS`.
* **XYZ / TMS**: proste adresy kafelkowe w układzie Web Mercator oparte na szablonie `{z}/{x}/{y}` (np. *OSMapa.pl*, *UMP-pcPL*, *Waymarked Trails*), wariancie `{-y}` (standard OSGeo TMS o odwróconej osi pionowej) albo kodzie `{quadkey}`.

Obsługa w oknie głównym wygląda tak samo jak przy WMS: wybierasz warstwę, piksel, bok segmentu i obszar. Możesz swobodnie łączyć w jednej kompozycji warstwy z różnych usług (np. podkład WMTS/XYZ z granicami działek WMS lub szlakami turystycznymi XYZ). Program pobiera poszczególne kafle lub obrazy i składa z nich gotowy segment.

---

## Usługi XYZ i TMS: podkłady i nakładki

Przy obsłudze kafelków XYZ i TMS działają dodatkowe reguły:

* **Podkład pod nakładką.** Jeśli dodasz najpierw przezroczystą nakładkę (np. szlaki turystyczne *Waymarked Trails*), a później wybierzesz mapę podkładową (np. *OSMapa.pl*), podkład zostanie automatycznie umieszczony pod nakładką.
* **Pobieranie samej nakładki.** Zaznaczenie samych nakładek zachowuje przezroczyste tło PNG (puste kafle `HTTP 404` pozostają przezroczyste, zamiast zgłaszać błąd pobierania).
* **Brakujące kafle na najwyższym zbliżeniu.** Gdy na serwerze podkładu brakuje pojedynczego kafla na maksymalnym poziomie (`HTTP 404`), program pobiera kafel z poziomu wyżej i powiększa jego fragment, zapobiegając powstawaniu białych plam.
* **Obszar poza zasięgiem mapy.** Dla zbiorów z określonym zasięgiem (np. *OSMapa.pl* poza Polską) przed pobieraniem pojawia się ostrzeżenie o braku danych na zaznaczonym terenie.
* **Zakres powiększeń, subdomeny i referer.** W kreatorze oraz plikach JSON można zdefiniować zakres poziomów (`zMin`–`zMax`), odwróconą oś `{-y}`, rotację subdomen (`{s}`, `{a|b|c}` lub `{switch:a,b,c}`), format `{quadkey}` (serwery w stylu Bing) oraz nagłówek `referer`. W szablonach dopuszczalny jest również przyrostek `{r}` (oznaczenie kafli retina z bibliotek Leaflet i OpenLayers, pomijane automatycznie).

---

## Dobór poziomu kafli

Program porównuje zadany rozmiar piksela segmentu z poziomami usługi i wybiera najmniej szczegółowy poziom, który zapewnia wymaganą ostrość (dopuszcza piksel kafla większy maksymalnie o 10%). Gdy żaden poziom nie jest tak dokładny, pobiera najszczegółowszy dostępny zestaw kafli.

W praktyce:

* piksel mniejszy niż maksymalny poziom usługi nie polepszy czytelności, za to powiększy pliki,
* zalecany piksel z pliku zbioru map (np. 0,25 m dla standardowej ortofotomapy) wskazuje najdokładniejszy sensowny poziom.

Gdy kafle są wyraźnie dokładniejsze niż segment, program uśrednia piksele kafla, więc obraz zachowuje gładkie przejścia.

---

## Przeliczanie do wybranego układu

Jeśli usługa udostępnia kafle w wybranym układzie pobierania, segmenty powstają bez przeliczania współrzędnych. W przeciwnym razie program pobiera kafle w układzie źródłowym (np. Web Mercator) i transformuje obraz do układu docelowego. Pozwala to pobrać mapę z usługi WMTS w układzie PL-1992 bezpośrednio do układu PL-2000 albo UTM.

Gdy warstwa nie oferuje kafli w żadnym układzie obsługiwanym przez program, pobieranie kończy się komunikatem z listą dostępnych układów.

---

## Zachowywanie pobranych kafli

Kafle (WMTS, XYZ i TMS) trafiają do wspólnego podfolderu `_kafle_wmts` w katalogu sesji. Kafel wspólny dla sąsiednich segmentów pobierany jest raz, a wznowienie po błędzie nie powtarza pobranych już plików.

* Domyślnie folder `_kafle_wmts` jest usuwany po skompletowaniu wszystkich segmentów.
* Opcja *zachowuj pobrane kafle w folderze sesji* (okno *Ustawienia*, grupa *Usługi kafelkowe (WMTS / XYZ)*) pozostawia pobrane pliki. Ponowne złożenie tego samego obszaru z innym bokiem segmentu nie wymaga wtedy łączenia z internetem.
* **Weryfikacja bufora:** dla zachowanych kafli program zapisuje pliki metadanych (`.meta`) z nagłówkami `ETag` i `Last-Modified`. Przy powtórnym uruchomieniu sprawdza z serwerem aktualność danych (odpowiedź `HTTP 304 Not Modified`), co eliminuje zbędny transfer bez ryzyka pozostania z nieaktualną mapą.
* Przy [odświeżaniu mapy](../odswiezanie/) zachowane kafle są usuwane przed startem pobierania.

---

## Jakość JPEG segmentów

Segmenty składane z kafli trzeba zapisać od nowa, więc przy formacie JPEG są ponownie kompresowane. Jakość ustawia
się w oknie *Ustawienia* (*Jakość JPEG segmentów*, od 50 do 100, domyślnie 90). Segmenty z pojedynczego serwera WMS
o kryciu 100% zapisywane są bez ponownej kompresji i to ustawienie ich nie dotyczy.

---

➡️ **Kolejny krok:** [Zasady ochrony serwerów i tempo zapytań →](../serwery/)

*Zobacz też: [Serwery WMS, WMTS i kafelkowe XYZ / TMS](../../pojecia/#serwery-wms-wmts-i-kafelkowe-xyz--tms) · [Kreator serwerów](../../serwery/kreator/)*
