# Zasady współpracy i rozwoju projektu MapoTero

Dziękujemy za zainteresowanie rozwojem MapoTero! Ten dokument opisuje zasady tworzenia kodu, architekturę projektu oraz wymagania dotyczące zgłaszania zmian.

---

## 1. Architektura i technologie

* **Język i środowisko:** VB.NET, WinForms, **.NET 10 (x64)**.
* **Mapa:** GMap.NET.WinForms 2.1.7.
* **Tryb kompilatora:** `Option Strict On`, `Option Explicit On`, `Option Infer On`.
* **Język w kodzie:** komentarze, nazwy procedur/zmiennych i komunikaty dla użytkownika są **po polsku**.

### Podział na projekty
* **`MapoTero.Core`** (`netstandard2.0`, bez WinForms) — obliczenia matematyczne, geodezyjne (szereg Krügera), obsługa formatów plików (.conf, .txt, KML, DXF, GeoTIFF, MBTiles), zapytania WMS/WMTS. Nową logikę dodawaj tutaj.
* **`MapoTero.App`** (`net10.0-windows`, WinForms) — interfejs użytkownika, okna formularzy, kontrolka mapy, wątki pobierania w tle.
* **`MapoTero.Tests`** — testy jednostkowe xUnit weryfikujące poprawność `MapoTero.Core`.

---

## 2. Żelazna zasada: wyłącznie jeden plik `MapoTero.exe`

MapoTero jest rozwijane i dystrybuowane **wyłącznie jako jeden plik wykonywalny `MapoTero.exe`** w folderze `MapoTero_SingleFile` (obok niego znajduje się jedynie folder `warstwy/` oraz `README.md`).

* Publikację programu wykonuj poleceniem:
  ```bash
  dotnet publish MapoTero.App/MapoTero_v3.vbproj -c Release -o MapoTero_SingleFile
  ```
* **Nie zmieniaj** w pliku projektu `MapoTero_v3.vbproj` ustawień jednoplikowych (`PublishSingleFile`, `IncludeNativeLibrariesForSelfExtract`).
* **Nie twórz** wydań wieloplikowych, profili publikacji (`*.pubxml`) ani nowych folderów wyjściowych.

---

## 3. Kompilacja i testy

Przed zgłoszeniem zmian upewnij się, że cały projekt buduje się bez błędów i przechodzi testy jednostkowe:

* **Kompilacja:**
  ```bash
  dotnet build MapoTero.App/MapoTero_v3.vbproj -c Release
  ```
* **Testy jednostkowe:**
  ```bash
  dotnet test MapoTero.Tests/MapoTero.Tests.vbproj -c Release
  ```
Każdą nową logikę obliczeniową lub parser formatów należy pokryć nowymi testami w `MapoTero.Tests`.

---

## 4. Konwencje i reguły projektowe

* **Współrzędne:** zgodnie z polską konwencją geodezyjną **X oznacza współrzędną północną** (Northing), a **Y współrzędną wschodnią** (Easting). Zasada ta obowiązuje również w nazwach pól i zmiennych (`XGora`, `XDol`, `YLewy`, `YPrawy`).
* **Zapis liczb:** liczby zmiennoprzecinkowe w plikach konfiguracyjnych, plikach sesji i zapytaniach HTTP zapisujemy zawsze z **kropką dziesiętną** (`CultureInfo.InvariantCulture`), niezależnie od ustawień regionalnych systemu operacyjnego.
* **Zgodność wsteczna:** pliki sesji (`conf.txt`, `lastsettings.txt`, `korytarz.txt`) muszą zachować zgodność ze starszymi wersjami MapoTero.
* **Pliki projektanta WinForms (`*.Designer.vb`):** edytuj ze szczególną ostrożnością. Właściwości publiczne formularzy wymagają atrybutu `DesignerSerializationVisibility(Hidden)`, aby uniknąć problemów w .NET 10.

---

## 5. Dodawanie nowych warstw i serwerów WMS/WMTS

Nowe publiczne usługi mapowe można dodawać do plików tekstowych w folderze `MapoTero_SingleFile/warstwy/` oraz odpowiednich grup tematycznych w `warstwy/katalog.txt`. Zgłaszając nową warstwę, upewnij się, że:
1. Usługa działa stabilnie i jest dostępna publicznie.
2. Adres URL zapytania `GetCapabilities` jest aktualny.
3. Warstwa posiada zdefiniowane obsługiwane układy współrzędnych (np. EPSG:2180, EPSG:2176–2179, EPSG:4326).

---

## 6. Zgłaszanie błędów i Pull Requestów

* Błędy i propozycje zmian zgłaszaj poprzez [GitHub Issues](https://github.com/Eksploracja/MapoTero/issues) przy użyciu przygotowanych szablonów.
* W przypadku Pull Requestów opisz cel zmian, powołaj się na ewentualne zgłoszenie i upewnij się, że lista kontrolna w szablonie PR została spełniona.
