---
title: Formaty plików
nav_order: 20
---

# Formaty plików
{: .no_toc }

Dla zaawansowanych: budowa plików tekstowych i konfiguracyjnych zapisywanych oraz odczytywanych przez MapoTero.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## conf.txt

Plik `conf.txt` powstaje w folderze każdej sesji pobierania. Zapisuje wartości w osobnych wierszach (teksty w cudzysłowach,
wartości logiczne `#TRUE#` / `#FALSE#`, liczby z kropką dziesiętną) w ustalonej kolejności zgodnej ze starszymi wersjami:

1. Folder sesji, nazwa zbioru bazowego, zasięg (`XDol`, `YLewy`, `XGora`, `YPrawy`), bok segmentu `[px]`, rozmiar piksela.
2. 12 pól nazw warstw, liczba warstw, format obrazu, przedrostek, flaga *pobieraj powyżej ostatniego*, środek i zoom podglądu.
3. Pola dopisane w nowszych wersjach (opcjonalne dla zgodności wstecznej): styl numeracji, kod EPSG układu (`2180`, `2176–2179`, `32633–32635`, `4326`), flaga przezroczystości, data pobrania (`RRRR-MM-DD`), adres serwera głównego, krycie warstw (np. `100;50;100`), a dla kanapek wieloserwerowych adresy serwerów oraz zbiory warstw rozdzielone znakiem `|`.

---

## lastsettings.txt

Plik ustawień programu leży obok `MapoTero.exe` (lub w `%LocalAppData%\MapoTero`). Składa się z naprzemiennych par wierszy:
**nazwa parametru** w jednym wierszu i **wartość** w następnym (odczyt odbywa się po nazwach, więc brakujące pozycje
przyjmują wartości domyślne). Przechowuje ustawienia z okna *Ustawienia*, ostatni widok mapy, szerokość prawego panelu
oraz zapamiętane wybory z okien scalania, nakładania i eksportu do CAD.

---

## korytarz.txt

Zapisywany w folderze sesji (UTF-8 bez BOM, liczby z kropką dziesiętną) obok `conf.txt`, gdy obszar wyznaczono wzdłuż trasy:

```text
MapoTero korytarz trasy
szerokosc_m=1000
nazwa=Przykładowa trasa
linia
53.6536000 15.8058000
53.6591000 15.8142000
```

Blok `linia` może powtarzać się wielokrotnie dla tras wieloodcinkowych (współrzędne WGS84: szerokość i długość geograficzna).

---

## koordynaty.txt i error.txt

* **`koordynaty.txt`**: tworzony na początku pobierania. Dla każdego segmentu sesji zawiera nazwę pliku, dokładny adres
  zapytania wysłanego do serwera oraz współrzędne narożników segmentu.
* **`error.txt`**: istnieje podczas pobierania oraz po zakończeniu, jeśli brakuje części segmentów. Każdy brakujący segment
  opisany jest trzema wierszami: nazwa segmentu, adres zapytania, `przyczyna: …`. Wrażliwe parametry (tokeny, klucze) są maskowane jako `***`.

---

## Folder bufora _kafle_wmts

Gdy włączona jest opcja zachowywania kafli w sesji, program gromadzi pobrane kafelki w folderze `_kafle_wmts`.
Obok plików graficznych zapisywane są pliki `*.meta` (UTF-8 bez BOM):
1. wiersz 1: czas ostatniego sprawdzenia na serwerze (UTC ticks),
2. wiersz 2: nagłówek HTTP `ETag`,
3. wiersz 3: nagłówek HTTP `Last-Modified`.

Pozwala to sprawdzić aktualność kafli przy ponownym pobieraniu bez przesyłania całego obrazu od nowa (odpowiedź HTTP 304).

---

## Pliki zbiorów map i katalog.txt

* **`warstwy\*.txt`**: pojedyncze zbiory map w formacie klasycznym (Windows-1250) lub JSON (UTF-8), opisane szczegółowo
  w rozdziale [Plik zbioru map](../serwery/plik-zbioru-map/).
* **`warstwy\warstwy.txt`**: indeks zbiorów wczytywanych do programu (jedna nazwa pliku bez `.txt` w wierszu).
* **`warstwy\katalog.txt`** (UTF-8): buduje grupy drzewa *Wybierz mapy* i przypisuje warstwom czytelne nazwy:
  `> Grupa`, `>> Podgrupa`, `zbiór | warstwa | nazwa wyświetlana` (wiersze zaczynające się od `#` to komentarze).

---

➡️ **Kolejny krok:** [Słownik pojęć programu MapoTero →](../slownik/)

*Zobacz też: [Sesje pobierania](../sesje/) · [Plik zbioru map](../serwery/plik-zbioru-map/) · [Gdzie program zapisuje dane](../pierwsze-kroki/dane-programu/)*
