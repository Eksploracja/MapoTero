---
title: Problemy i najczęstsze pytania
nav_order: 19
---

# Problemy i najczęstsze pytania
{: .no_toc }

Objaw, najczęstsza przyczyna i sprawdzony sposób rozwiązania.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Program się nie uruchamia

* **Komunikat o braku środowiska .NET:** MapoTero wymaga bezpłatnego środowiska
  **.NET Desktop Runtime 10 (x64)**. Pobierz je ze strony Microsoftu (w sekcji *Desktop Apps*, koniecznie wersję `x64`,
  nie `x86` ani samo `Runtime`) i uruchom program ponownie.
* **Niebieskie okno Windows SmartScreen (*System Windows ochronił ten komputer*):** kliknij **Więcej informacji**,
  a potem **Uruchom mimo to** (zob. [Instalacja i uruchomienie](../pierwsze-kroki/instalacja/#pierwsze-uruchomienie-i-komunikat-smartscreen)).
* **Pusta lista map w programie:** obok `MapoTero.exe` brakuje folderu `warstwy\` albo plik `warstwy.txt` w tym folderze
  jest pusty. Rozpakuj całą paczkę ZIP do jednego katalogu.

---

## Segmenty są białe lub puste

* **Za duży lub za mały rozmiar piksela:** wiele usług WMS (np. działki ewidencyjne, uzbrojenie terenu) ma ograniczony
  zakres skal i przy pikselu np. `20 m/pix` zwraca biały obraz. Ustaw zalecany piksel (zostaw wartość podpowiedzianą przez program).
* **Obszar poza zasięgiem mapy:** mapy historyczne i lokalne obejmują tylko część kraju, a krajowe kafelki XYZ (np. *OSMapa.pl*)
  obejmują tylko Polskę.
* **Wybrano układ nieobsługiwany przez serwer:** niektóre serwery zewnętrzne nie obsługują np. PL-2000 lub UTM i zwracają
  pusty obraz bez błędu. Przełącz układ pobierania z powrotem na **PL-1992**.
* **Przykryta nakładka w kanapce:** jeśli na karcie *Wybrane warstwy* nieprzezroczysta warstwa leży wyżej (bliżej góry listy)
  niż nakładka lub mapa pod spodem, zakryje ją w całości. Sprawdź kolejność w oknie głównym albo przyciskiem **Mikser…**.
* **Zamieniona kolejność osi X i Y (zewnętrzne serwery WMS):** dla serwera niezgodnego ze standardem WMS 1.3.0 zmień
  w jego adresie w pliku zbioru map `VERSION=1.3.0` na `VERSION=1.1.1` (zob. [Błędy pobierania](../pobieranie/bledy/)).

---

## Mapa jest przesunięta

* **Sprawdź układ współrzędnych w docelowym programie (QGIS, CAD, ArcGIS):** mapa została pobrana np. w `PL-1992` (EPSG:2180),
  a projekt jest otwarty w innym układzie (np. PL-2000) bez automatycznej transformacji w locie.
* **Zamienione współrzędne przy ręcznym wpisywaniu:** w MapoTero **X = północ, Y = wschód**, a w GIS i CAD jest odwrotnie
  (zob. [Konwencja X i Y](../pojecia/#konwencja-x-i-y)).
* **Stare pliki ze starszej wersji programu (< 3.12):** przed wersją 3.12 pliki KML (`LatLonBox`) i world file mogły mieć
  niewielkie przesunięcie (ok. pół piksela lub obrót na krańcach Polski). Odśwież lub wyeksportuj mapę w nowej wersji.

---

## Serwer odrzuca zapytania

* **Błędy `HTTP 429`, `502`, `503`, `504` lub przekroczenia czasu:** serwer jest przeciążony. Program sam zwalnia i robi
  przerwy ([Ochrona serwerów map](../pobieranie/serwery/)). Zmniejsz w *Ustawieniach* liczbę *Równoczesnych pobrań* (np. do `1–2`)
  i zwiększ *Limit czasu odpowiedzi*.
* **Błąd `HTTP 403` na serwerze XYZ/TMS:** serwer wymaga odpowiedniego nagłówka `referer` (pobieranie zatrzymuje się od razu).
* **Błąd certyfikatu SSL w kreatorze:** włącz w oknie dodawania serwera opcję *Ignoruj błędy certyfikatów SSL (starsze serwery)*.
* **Brakujące segmenty po zakończeniu:** sprawdź przyczynę w pliku `error.txt` w folderze sesji i kliknij **Pobierz segmenty** ponownie,
  aby dopobrać braki.

---

## Brak prawa zapisu

Jeśli umieścisz program w folderze systemowym (np. `C:\Program Files\MapoTero`), Windows nie pozwala programom zapisywać
tam plików. MapoTero automatycznie przełącza wtedy zapis `lastsettings.txt`, `download\`, `awarie.log` i nowo dodanych
zbiorów map do folderu użytkownika **`%LocalAppData%\MapoTero`** (zob. [Gdzie program zapisuje dane](../pierwsze-kroki/dane-programu/)).

---

## Nieoczekiwany błąd programu

W razie nieoczekiwanego wyjątku program zapisuje szczegóły techniczne (wersję, system, datę i stos wywołań) w pliku
**`awarie.log`** w folderze danych i wyświetla komunikat z możliwością kontynuowania pracy.
Jeśli błąd się powtarza, zgłoś go na [forum](https://forum.eksploracja.pl/viewforum.php?f=205) lub w dziale
[Issues na GitHubie](https://github.com/Eksploracja/MapoTero/issues), dołączając ostatni wpis z `awarie.log`.

---

## Najczęstsze pytania

**Dlaczego przycisk Eksportuj pobraną mapę… jest ukryty albo polecenie Scalanie segmentów jest nieaktywne?**  
Ponieważ bieżący folder sesji nie ma jeszcze `conf.txt` (nowa sesja przed pierwszym pobraniem) albo w folderze brakuje pobranych segmentów. Jeśli wcześniejsze pobieranie nie było pełne (istnieje `error.txt`), okno eksportu otwiera się z ostrzeżeniem o brakach.

**Jak pobrać nową mapę, nie nadpisując poprzedniej?**  
Zapisz sesję w nowym folderze (<kbd>Alt</kbd>+<kbd>Z</kbd>) przed pobieraniem albo zgódź się na utworzenie nowego podfolderu
`download\sesja_<nazwa_zbioru>_<RRRR-MM-DD_GGMMSS>\`, gdy program zapyta o to po kliknięciu **Pobierz segmenty**.

**Czy mogę połączyć dwie mapy z różnych serwerów?**  
Tak, bezpośrednio na karcie *Wybrane warstwy* w oknie głównym (z regulacją krycia i podglądem w oknie **Mikser…**)
albo później z gotowych sesji w oknie [Eksport mapy](../eksport/nakladki/) czy [Wielowarstwowy MBTiles](../mbtiles-wielowarstwowy/).

**Jak najszybciej przygotować gotową mapę bez ręcznego ustawiania siatki i układu?**  
Wciśnij skrót <kbd>Ctrl</kbd>+<kbd>K</kbd> albo kliknij przycisk **Szybki kreator…** na górnym pasku nad mapą. [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy) poprowadzi Cię w 4 krokach: wybierzesz cel (telefon, Garmin, wydruk, GIS, CAD), wskażesz obszar (miejscowość, działkę lub trasę), wybierzesz szablon i jednym kliknięciem wygenerujesz gotowy plik.

---

➡️ **Kolejny krok:** [Szczegółowa struktura formatów plików →](../formaty-plikow/)

*Zobacz też: [Błędy pobierania](../pobieranie/bledy/) · [Ochrona serwerów map](../pobieranie/serwery/) · [Gdzie program zapisuje dane](../pierwsze-kroki/dane-programu/)*
