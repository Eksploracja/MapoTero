---
title: Pierwsza mapa krok po kroku
nav_order: 3
parent: Pierwsze kroki
description: "Praktyczny samouczek: pobierzesz fragment mapy topograficznej 1:25 000, a następnie przygotujesz z niego plik dla telefonu oraz arkusz GeoTIFF dla programu GIS."
---

# Pierwsza mapa krok po kroku
{: .no_toc }

Praktyczny samouczek: pobierzesz fragment mapy topograficznej 1:25 000, a następnie przygotujesz z niego plik dla telefonu oraz arkusz GeoTIFF dla programu GIS.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }} · **Czas wykonania:** ok. 10 minut

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

{: .wskazowka }
**Szukasz najprostszej drogi bez ręcznych ustawień?** Wciśnij w programie <kbd>Ctrl</kbd>+<kbd>K</kbd> (lub kliknij przycisk **Szybki kreator…** na pasku nad mapą), aby otworzyć [Szybki kreator mapy](../../narzedzia/#szybki-kreator-mapy). Kreator prowadzi w 4 krokach od wyboru urządzenia docelowego po gotowy plik. Poniższy poradnik pokazuje pełną, klasyczną ścieżkę pracy w oknie głównym programu z ręczną kontrolą wszystkich parametrów.

---

## Przed rozpoczęciem

Upewnij się, że masz przygotowane:
* uruchomiony program MapoTero i aktywne połączenie z internetem,
* domyślne ustawienia programu (nie zmieniaj jeszcze żadnych opcji w oknie *Ustawienia*).

W tym samouczku pobierzemy obszar o wymiarach około 8 × 8 km, złożony z siatki 2 × 2 segmenty.

---

## Krok 1: Wybór mapy i warstwy

1. W prawym panelu, w karcie **Katalog warstw**, rozwiń grupę:  
   **Mapy topograficzne** > **Skany map topograficznych GUGiK**.
2. Zaznacz pole wyboru przy pozycji **1:25 000, układ 1965** (nazwa techniczna na serwerze: *Raster_25_1965*).
3. Wybrana warstwa pojawi się na liście *Wybrane warstwy* z numerem `1)`. W karcie *Parametry segmentów* program automatycznie podpowie optymalną wartość piksela: **2,00 m/px**.

{: .wskazowka }
Jeśli przez pomyłkę zaznaczysz dodatkową warstwę, odznacz ją w drzewie albo kliknij krzyżyk **✕** na końcu jej wiersza na liście *Wybrane warstwy* (lub naciśnij klawisz <kbd>Delete</kbd>).

---

## Krok 2: Sprawdzenie parametrów w karcie pod mapą

W karcie **Parametry segmentów** pod mapą sprawdź wartości (są ustawione domyślnie):
* **Układ:** `PL-1992` (państwowy układ współrzędnych dla całej Polski),
* **Bok segmentu:** `2000 px` (jeden segment to plik kwadratowy o boku 2000 pikseli),
* **Piksel:** `2 m/px` (jeden piksel obrazu odpowiada 2 metrom w terenie, więc pojedynczy segment pokrywa 4 × 4 km terenu).

Jeśli chcesz zmienić te wartości, wystarczy kliknąć odnośnik **Zmień…** w tej karcie.

---

## Krok 3: Zaznaczenie obszaru na podglądzie

1. Odszukaj interesujący Cię teren na mapie podglądu (przesuwaj widok lewym przyciskiem myszy, przybliżaj kółkiem myszy lub przyciskami **+** / **-**). Możesz też wpisać miejscowość w polu **Szukaj miejsca lub działki…** nad mapą (<kbd>Ctrl</kbd>+<kbd>F</kbd>) i wcisnąć <kbd>Enter</kbd>.
2. Wciśnij **prawy przycisk myszy** w jednym narożniku planowanego obszaru, przeciągnij prostokąt i puść przycisk (albo kliknij przycisk **Zaznacz obszar** na pasku narzędzi i kliknij dwa przeciwległe narożniki).
3. Na mapie pojawi się siatka segmentów z uchwytami regulacji. W karcie *Obszar pobierania* pod mapą zobaczysz wymiar `8 × 8 km` oraz `4 segmenty`.

![Okno główne po wybraniu warstwy i zaznaczeniu obszaru]({{ '/pomoc/obrazy/01-pierwsza-mapa-wybor.png' | relative_url }}){: loading="lazy" width="1266" height="721"}

*Rysunek 1. Okno główne po wybraniu warstwy i wyznaczeniu siatki segmentów na mapie.*

{: .informacja }
Siatka zaokrągla zasięg do pełnych segmentów, więc może być nieco większa od narysowanego prostokąta. Jej granice możesz precyzyjnie dopasować, przeciągając białe uchwyty na krawędziach i narożnikach lewym przyciskiem myszy.

![Szczegół siatki segmentów z widocznymi uchwytami regulacji]({{ '/pomoc/obrazy/01-pierwsza-mapa-siatka.png' | relative_url }}){: loading="lazy" width="1266" height="721"}

*Rysunek 2. Uchwyty pozwalają przesuwać krawędzie, narożniki i całą siatkę.*

---

## Krok 4: Pobranie segmentów

1. W karcie **Stan sesji** pod mapą kliknij niebieski przycisk **Pobierz segmenty**.
2. Obserwuj postęp na tej samej karcie oraz na pasku stanu u dołu okna.
3. Po kilkunastu sekundach karta zmieni stan na zielone **4 z 4 segmentów gotowe**, a pod **Pobierz segmenty** pojawi się przycisk **Eksportuj pobraną mapę…**.

{: .informacja }
W prawym dolnym rogu na zakładce **Podgląd** możesz obejrzeć rzeczywistą próbkę mapy w skali 1:1 lub w powiększeniu.

{: .uwaga }
Gdy na karcie *Stan sesji* pojawi się komunikat o brakujących segmentach (*Nie udało się pobrać wszystkich…*), przełącz na zakładkę **Komunikaty**, gdzie widać szczegóły błędu. Wystarczy odczekać chwilę i kliknąć **Pobierz segmenty** jeszcze raz: program bez problemu uzupełni tylko brakujące segmenty.

---

## Krok 5: Przygotowanie pliku wynikowego

Pobrane segmenty możesz od razu wyeksportować i przesłać do wybranej aplikacji:

### Wariant A: Mapa do telefonu (MBTiles lub GeoPackage) z transferem przez Wi-Fi
1. Kliknij przycisk **Eksportuj pobraną mapę…** w karcie *Stan sesji* (lub wciśnij <kbd>Ctrl</kbd>+<kbd>E</kbd>).
2. W oknie *Eksport mapy* w sekcji formatów wybierz **MBTiles** (dla Locus Map, OsmAnd, OruxMaps) lub **GeoPackage (.gpkg)** (dla QField, Locus Map, QGIS).
3. Kliknij **Eksportuj**. Po wygenerowaniu pliku kliknij przycisk **Wyślij na telefon...**.
4. Na ekranie pojawi się kod QR wygenerowany przez wbudowany serwer Wi-Fi. Zeskanuj go aparatem smartfona, aby bezpośrednio pobrać mapę do pamięci telefonu bez kabla USB i bez chmury.

### Wariant B: Kopia do odbiornika Garmin (KMZ lub JNX)
1. W oknie *Eksport mapy* wybierz **KMZ (Garmin Custom Maps)** lub wielopoziomowy **JNX (Garmin BirdsEye)**.
2. Po wygenerowaniu pliku kliknij **Wyślij na telefon...** (dla `.jnx` od razu uruchamia kopiowanie na Garmina, a dla `.kmz` wykrywa podłączony odbiornik USB) albo wybierz z menu *Narzędzia > Kopiuj mapę do Garmina (USB)...*, żeby zapisać plik w folderze `Garmin/CustomMaps` lub `Garmin/BirdsEye`.

### Wariant C: Kafelkowany arkusz do programu GIS (GeoTIFF)
1. W oknie *Eksport mapy* wybierz format **Arkusz**, a obok format **GeoTIFF (bezstratny)** lub **GeoTIFF (kompresja JPEG)** (opcjonalnie z zaznaczonym polem *piramidy w GeoTIFF*).
2. Kliknij **Eksportuj**. Program utworzy kafelkowany plik GeoTIFF 256×256 z piramidami pomniejszeń, który szybko otwiera się w programach QGIS, ArcGIS czy Avenza Maps.

---

## Co dalej

| Jeśli chcesz... | Przejdź do rozdziału |
|---|---|
| dowiedzieć się, gdzie bezpiecznie przechowywać pobrane sesje | [Gdzie program zapisuje dane](../dane-programu/) |
| poznać zasady doboru rozdzielczości i piksela | [Pojęcia podstawowe](../../pojecia/) |
| poznać wszystkie elementy i menu okna głównego | [Okno główne](../../okno-glowne/) |
| wygenerować mapę turystyczną lub historyczną w 4 krokach | [Szybki kreator mapy](../../narzedzia/#szybki-kreator-mapy) |
| pobrać mapę wzdłuż trasy wycieczki GPX | [Obszar wzdłuż trasy](../../obszar/korytarz/) |
| przenieść plik do aplikacji w telefonie | [Mapa w telefonie i odbiorniku GPS](../../eksport/aplikacje/) |
| wstawić podkład do rysunku CAD | [Eksport do CAD](../../cad/) |

---

➡️ **Kolejny krok w przewodniku:** [Gdzie program zapisuje dane (dane programu) →](../dane-programu/)
