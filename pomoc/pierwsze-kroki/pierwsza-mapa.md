---
title: Pierwsza mapa krok po kroku
nav_order: 3
parent: Pierwsze kroki
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
**Szukasz najprostszej i najszybszej drogi bez ręcznych ustawień?** Wciśnij w programie <kbd>Ctrl</kbd>+<kbd>K</kbd> (lub kliknij przycisk **🪄 Szybki kreator...** na pasku nad mapą), aby otworzyć [Szybki kreator mapy](../../narzedzia/#szybki-kreator-mapy). Kreator prowadzi w 4 krokach od wyboru urządzenia docelowego po gotowy plik. Poniższy poradnik pokazuje pełną, klasyczną ścieżkę pracy w oknie głównym programu z ręczną kontrolą wszystkich parametrów.

---

## Przed rozpoczęciem

Upewnij się, że masz przygotowane:
* uruchomiony program MapoTero i aktywne połączenie z internetem,
* domyślne ustawienia programu (nie zmieniaj jeszcze żadnych opcji w oknie *Ustawienia*).

W tym samouczku pobierzemy obszar o wymiarach około 8 × 8 km, złożony z siatki 2 × 2 segmenty.

---

## Krok 1: Wybór mapy i warstwy

1. W prawym panelu, w drzewie **Wybierz mapy**, rozwiń grupę:  
   **Mapy topograficzne** > **Skany map topograficznych GUGiK**.
2. Zaznacz pole wyboru przy pozycji **1:25 000, układ 1965** (nazwa techniczna na serwerze: *Raster_25_1965*).
3. Wybrana warstwa pojawi się na liście *Wybrane warstwy oraz ich kolejność* z numerem `1)`. W karcie *Parametry segmentów* program automatycznie podpowie optymalną wartość piksela: **2,00 m/pix**.

{: .wskazowka }
Jeśli przez pomyłkę zaznaczysz dodatkową warstwę, odznacz ją w drzewie albo kliknij czerwony krzyżyk **✕** na końcu jej wiersza na liście *Wybrane warstwy* (lub naciśnij klawisz <kbd>Delete</kbd>).

---

## Krok 2: Sprawdzenie parametrów w karcie pod mapą

W karcie **Parametry segmentów** pod mapą sprawdź wartości (są ustawione domyślnie):
* **Układ:** `PL-1992` (państwowy układ współrzędnych dla całej Polski),
* **Bok segmentu:** `2000 px` (jeden segment to plik kwadratowy o boku 2000 pikseli),
* **Piksel:** `2,00 m/pix` (jeden piksel obrazu odpowiada 2 metrom w terenie; pojedynczy segment pokrywa 4 × 4 km terenu).

Jeśli chcesz zmienić te wartości, wystarczy kliknąć odnośnik **Zmień…** w tej karcie.

---

## Krok 3: Zaznaczenie obszaru na podglądzie

1. Odszukaj interesujący Cię teren na mapie podglądu (przesuwaj widok lewym przyciskiem myszy, przybliżaj kółkiem myszy lub przyciskami **+** / **-**). Możesz też wpisać miejscowość w polu **🔍 Szukaj miejsca lub działki...** nad mapą i wcisnąć <kbd>Enter</kbd>.
2. Wciśnij **prawy przycisk myszy** w jednym narożniku planowanego obszaru, przeciągnij prostokąt i puść przycisk.
3. Na mapie pojawi się siatka segmentów z uchwytami regulacji. W karcie *Obszar* pod mapą zobaczysz wymiar `8,0 × 8,0 km` oraz siatkę `2 × 2` (4 segmenty).

![Okno główne po wybraniu warstwy i zaznaczeniu obszaru]({{ '/pomoc/obrazy/01-pierwsza-mapa-wybor.png' | relative_url }})

*Rysunek 1. Okno główne po wybraniu warstwy i wyznaczeniu siatki segmentów na mapie.*

{: .informacja }
Siatka zaokrągla zasięg do pełnych segmentów, więc może być nieco większa od narysowanego prostokąta. Jej granice możesz precyzyjnie dopasować, przeciągając niebieskie uchwyty lewym przyciskiem myszy.

![Szczegół siatki segmentów z widocznymi uchwytami regulacji]({{ '/pomoc/obrazy/01-pierwsza-mapa-siatka.png' | relative_url }})

*Rysunek 2. Uchwyty pozwalają przesuwać krawędzie, narożniki i całą siatkę.*

---

## Krok 4: Pobranie segmentów

1. W karcie **Stan sesji** pod mapą kliknij wyróżniony kolorem przycisk **Pobierz segmenty**.
2. Obserwuj postęp: ikona stanu pokazuje animowany łuk postępu, a pasek stanu na dole wyświetla pobierane pliki.
3. Po kilkunastu sekundach ikona zmieni się w zielony znak ✓, a przycisk zmieni napis na **Eksportuj pobraną mapę…**.
4. W prawym dolnym rogu na zakładce **Podgląd** możesz obejrzeć rzeczywistą próbkę pobranej mapy w skali 1:1 lub w powiększeniu lupą.

{: .uwaga }
Gdy na ikonie stanu pojawi się czerwony wykrzyknik (*Nie udało się ściągnąć wszystkich segmentów…*), kliknięcie diody stanu przełączy do zakładki **Komunikaty**, gdzie widać szczegóły błędu. Wystarczy odczekać chwilę i kliknąć **Pobierz segmenty** jeszcze raz: program bez problemu uzupełni tylko brakujące segmenty.

---

## Krok 5: Przygotowanie pliku wynikowego

Pobrane segmenty możesz od razu wyeksportować i przesłać do wybranej aplikacji:

### Wariant A: Mapa do telefonu (MBTiles lub GeoPackage) z transferem przez Wi-Fi
1. Kliknij przycisk **Eksportuj pobraną mapę…** w karcie stanu (lub wciśnij <kbd>Ctrl</kbd>+<kbd>E</kbd>).
2. W oknie *Eksport mapy* w sekcji formatów wybierz **MBTiles** (dla Locus Map, OsmAnd, OruxMaps) lub **GeoPackage (.gpkg)** (dla QField, Locus Map, QGIS).
3. Kliknij **Eksportuj**. Po wygenerowaniu pliku kliknij przycisk **Wyślij na telefon...**.
4. Na ekranie pojawi się kod QR wygenerowany przez wbudowany serwer Wi-Fi. Zeskanuj go aparatem smartfona, aby bezpośrednio pobrać mapę do pamięci telefonu bez kabla USB i bez chmury!

### Wariant B: Kopia do odbiornika Garmin (KMZ lub JNX)
1. W oknie *Eksport mapy* wybierz **KMZ (Garmin Custom Maps)** lub wielopoziomowy **JNX (Garmin BirdsEye)**.
2. Po wygenerowaniu pliku kliknij przycisk **Kopiuj na Garmina...** – program sam wykryje podłączone urządzenie USB i skopiuje plik do właściwego katalogu (`Garmin/CustomMaps` lub `Garmin/BirdsEye`).

### Wariant C: Kafelkowany arkusz do programu GIS (GeoTIFF)
1. W oknie *Eksport mapy* wybierz format **Arkusz**, a obok format **GeoTIFF (bezstratny)**.
2. Kliknij **Eksportuj**. Program utworzy kafelkowany plik GeoTIFF 256×256 z wewnętrznymi piramidami pomniejszeń, który natychmiast otwiera się w programach QGIS, ArcGIS czy Avenza Maps.

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
