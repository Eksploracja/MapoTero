---
title: Sesje pobierania
nav_order: 8
---

# Sesje pobierania
{: .no_toc }

Każde pobieranie tworzy sesję: folder z segmentami, plikami kalibracji i plikiem `conf.txt`, z którego korzystają scalanie i eksport.
{: .fs-5 .fw-300 }

**Gdzie w programie:** menu *Sesja pobierania > zapisz / wczytaj ustawienia sesji (conf.txt)*  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Folder sesji i jego zawartość

Domyślnym folderem sesji jest `download\` w folderze danych programu. Kiedy zapiszesz sesję w wybranym przez siebie
miejscu lub pobierzesz kolejną mapę do osobnego podfolderu (`download\sesja_RRRR-MM-DD_GGMMSS\`), ten folder staje się
bieżącym folderem sesji (jego ścieżkę widać na pasku stanu u dołu okna).

| Plik lub podfolder | Kiedy powstaje | Zawartość |
|---|---|---|
| `_01_01.jpg`, `_01_02.jpg`… | podczas pobierania | pobrane obrazy segmentów |
| `.jgw`, `.prj`, `.map`, `.kml`, `.tab`… | gdy włączone w ustawieniach | pliki georeferencyjne obok każdego segmentu |
| `conf.txt` | przy zapisie sesji i starcie pobierania | pełna konfiguracja sesji (zasięg, warstwy, układ, format, data) |
| `koordynaty.txt` | przy starcie pobierania | lista wszystkich segmentów z adresami zapytań i narożnikami |
| `korytarz.txt` | gdy włączono obszar wzdłuż trasy | geometria trasy i szerokość pasa korytarza |
| `error.txt` | w trakcie lub po błędach pobierania | wykaz brakujących segmentów z przyczyną błędu (znika po kompletnym pobraniu) |
| `_kafle_wmts\` | usługi WMTS / XYZ / TMS | bufor pobranych kafli (domyślnie usuwany po komplecie segmentów) |
| `_scalone_segmenty_…`, `_eksport_…`, `_cad_…` | po scaleniu lub eksporcie | pliki wynikowe utworzone z tej sesji |

---

## Plik conf.txt

`conf.txt` jest wizytówką sesji pobierania. Przechowuje w formie tekstowej m.in.:

* wybrany zbiór map, listę maksymalnie 12 warstw, ich krycie (`%`) oraz indywidualne adresy serwerów i zbiory
  (gdy kanapka łączy warstwy z różnych usług),
* dokładny zasięg (`x dolne`, `y lewe`, `x górne`, `y prawe`), kod EPSG układu współrzędnych, bok segmentu `[px]` i piksel,
* format obrazu segmentów, przezroczystość tła (`TRANSPARENT`), przedrostek i styl numeracji,
* pozycję podglądu mapy, źródłowy adres serwera oraz datę ostatniego rozpoczęcia pobierania.

Pliki `conf.txt` zapisane w starszych wersjach MapoTero wczytują się poprawnie w nowej wersji.

---

## Zapisywanie i wczytywanie sesji

* **Automatycznie przy starcie:** jeśli w domyślnym folderze `download\` istnieje `conf.txt`, program wczytuje tę sesję
  samoczynnie po uruchomieniu.
* **Zapisz ustawienia sesji (`Alt`+`Z`):** *Sesja pobierania > zapisz ustawienia sesji (conf.txt)* pozwala wskazać dowolny
  folder (np. `D:\Mapy\Okolica_topo\`), zapisać tam `conf.txt` (i ewentualnie `korytarz.txt`) i od razu przestawić
  program na zapis kolejnych segmentów do tego folderu. Jeśli w wybranym folderze leżą segmenty innej sesji, program
  wyświetli ostrzeżenie.
* **Wczytaj ustawienia sesji (`Alt`+`W`):** *Sesja pobierania > wczytaj ustawienia sesji (conf.txt)* odtwarza cały stan
  z wybranego folderu (musi zawierać `conf.txt`): warstwy, krycia, zasięg, piksel, korytarz oraz rysuje siatkę na mapie.
  Przydaje się do dopobrania braków lub odświeżenia starej mapy.

---

## Nazwy segmentów

Nazwa pliku każdego segmentu składa się z **przedrostka** (ustawienie w oknie *Ustawienia*, domyślnie `_`), numeru segmentu
zależnego od **stylu numeracji** oraz rozszerzenia wybranego formatu. Wiersze liczone są **od góry** (wiersz 1 najbardziej
na północ), a kolumny **od lewej** (kolumna 1 najbardziej na zachód).

Przykład dla siatki o 4 kolumnach i 3 wierszach (segment w 2. wierszu i 3. kolumnie, tj. 7. kolejny segment), z domyślnym przedrostkiem `_` i formatem `jpeg`:

| Styl numeracji | Nazwa pliku | Zasada |
|---|---|---|
| `NrWiersza_NrKolumny` *(domyślny)* | `_02_03.jpg` | dwucyfrowy numer wiersza i dwucyfrowy numer kolumny (od 100 wzwyż liczba ma 3 cyfry) |
| `01_02_03` | `_07.jpg` | kolejny numer segmentu uzupełniony zerami (co najmniej dwie cyfry) |
| `1_2_3` | `_7.jpg` | kolejny numer segmentu bez wiodących zer |

{: .wskazowka }
Styl `NrWiersza_NrKolumny` jest najbardziej czytelny przy przeglądaniu plików w folderze, bo od razu wiadomo, w którym
rzędzie i kolumnie leży dany obraz.

---

## Opis mapy

Przy starcie pobierania (albo [odświeżaniu](../pobieranie/odswiezanie/)) program zapisuje w `conf.txt` aktualną datę oraz adres
serwera. Na tej podstawie podczas eksportu do MBTiles, KMZ i CAD automatycznie uzupełniana jest nazwa mapy, źródło
danych (np. Geoportal / GUGiK, IHPAN, PIG-PIB, OpenStreetMap) oraz data pobrania.

---

## Pobieranie innego obszaru do tego samego folderu

Jeśli w bieżącym folderze sesji znajdują się już segmenty innego obszaru, innej siatki, innych warstw, innego krycia lub innego
układu współrzędnych (albo pliki bez `conf.txt`), po kliknięciu **Pobierz** program zatrzymuje się i pyta:

* **Tak**: tworzy nowy podfolder `download\sesja_RRRR-MM-DD_GGMMSS\` i pobiera nową mapę do niego, pozostawiając poprzednie pliki nienaruszone.
* **Nie**: anuluje pobieranie, co pozwala wczytać poprzednią sesję lub zmienić folder ręcznie.

{: .informacja }
Jeżeli w folderze są tylko segmenty pobrane w dokładnie tej samej konfiguracji, kliknięcie **Pobierz** wznawia sesję
i dopobiera wyłącznie brakujące segmenty.

---

➡️ **Kolejny krok:** [Ustawienia programu →](../ustawienia/)

*Zobacz też: [Przebieg pobierania](../pobieranie/przebieg/) · [Scalanie segmentów](../scalanie/) · [Formaty plików](../formaty-plikow/)*
