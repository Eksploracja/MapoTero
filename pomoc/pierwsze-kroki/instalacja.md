---
title: Instalacja i uruchomienie
nav_order: 1
parent: Pierwsze kroki
---

# Instalacja i uruchomienie
{: .no_toc }

MapoTero nie wymaga instalatora: to jeden plik `MapoTero.exe` dla 64-bitowego Windows, dostarczany w paczce ZIP
razem z folderem `warstwy`.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Wymagania

| Co | Wymaganie |
|---|---|
| System | Windows 64-bitowy (x64) |
| Środowisko | [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) w wersji x64 (bezpłatne, instalowane jednorazowo) |
| Internet | potrzebny do pobierania map i wyświetlania podkładu mapy w oknie programu |
| Dysk | od kilkunastu megabajtów do kilkunastu gigabajtów, zależnie od obszaru i dokładności pobieranych map (zob. [Jak duży obszar pobrać](../../obszar/wielkosc/)) |

{: .informacja }
Program wymaga zainstalowania środowiska .NET osobno, więc sam plik exe zajmuje mało miejsca na dysku. Jeśli .NET Desktop Runtime 10 nie jest
zainstalowany, Windows przy uruchomieniu wyświetli komunikat z odnośnikiem do pobrania. Wybierz wersję
**Desktop Runtime** dla **x64**.

---

## Pobranie i rozpakowanie

1. Pobierz paczkę `MapoTero_v{{ site.wersja_programu }}.zip` ze strony
   [najnowszego wydania](https://github.com/Eksploracja/MapoTero/releases/latest).
2. Rozpakuj ją do wybranego folderu, np. `C:\MapoTero` albo `D:\Mapy\MapoTero`.
3. Sprawdź, czy obok `MapoTero.exe` leży folder `warstwy` z definicjami usług. Bez niego lista map w programie będzie pusta.

Zawartość paczki:

```
MapoTero.exe     program
warstwy\         zbiory map (pliki .txt)
README.md        opis programu
```

{: .wskazowka }
Najwygodniej rozpakować program do folderu, w którym możesz zapisywać pliki (np. na dysku `D:` albo w swoich
dokumentach). Program trzyma wtedy pobrane mapy i ustawienia obok siebie. Jeśli rozpakujesz go do `Program Files`,
też zadziała, ale dane trafią do folderu użytkownika (zob. [Gdzie program zapisuje dane](../dane-programu/)).

## Pierwsze uruchomienie i komunikat SmartScreen

Program jest otwartoźródłowy, ale nie ma płatnego certyfikatu podpisu cyfrowego. Dlatego przy pierwszym uruchomieniu
Windows może wyświetlić niebieskie okno *„System Windows ochronił ten komputer”*.

1. Kliknij odnośnik **Więcej informacji**.
2. Kliknij przycisk **Uruchom mimo to**.

![Okno SmartScreen: najpierw „Więcej informacji”, potem „Uruchom mimo to”]({{ '/obrazy/smartscreen.png' | relative_url }})

*Rysunek 1. Okno Windows SmartScreen: kliknij „Więcej informacji”, a następnie „Uruchom mimo to”.*

Okienko nie pojawi się, jeśli przed rozpakowaniem odblokujesz paczkę: kliknij plik ZIP prawym przyciskiem myszy,
wybierz **Właściwości**, zaznacz na dole **Odblokuj** i kliknij **OK**.

---

## Sprawdzenie pliku

W opisie każdego [wydania](https://github.com/Eksploracja/MapoTero/releases/latest) są:

* sumy kontrolne SHA-256 paczki ZIP i pliku `MapoTero.exe`,
* odnośnik do wyniku skanu w serwisie VirusTotal.

Sumę kontrolną pobranego pliku sprawdzisz w PowerShell:

```powershell
Get-FileHash .\MapoTero_v{{ site.wersja_programu }}.zip -Algorithm SHA256
```

Wynik musi być identyczny z sumą podaną w opisie wydania.

---

## Aktualizacja do nowej wersji

1. Zamknij program.
2. Zastąp plik `MapoTero.exe` nową wersją.
3. Zastąp folder `warstwy` nowym katalogiem. Zawiera on zaktualizowane adresy serwerów i nowe zbiory map.

Zachowaj własne pliki:

* plik `lastsettings.txt` (Twoje ustawienia),
* folder `download` (pobrane mapy),
* zbiory map dodane samodzielnie. Jeśli leżą w folderze `warstwy` programu, skopiuj je przed nadpisaniem folderu i dopisz ich nazwy z powrotem do pliku `warstwy\warstwy.txt` (zob. [Plik zbioru map](../../serwery/plik-zbioru-map/)).

{: .informacja }
Nowe wersje odczytują pliki sesji (`conf.txt`) i ustawienia zapisane przez starsze wydania. Możesz bez przeszkód wracać do dawniej pobranych map.

---

➡️ **Kolejny krok:** [Pierwsza mapa krok po kroku →](../pierwsza-mapa/)

*Zobacz też: [Gdzie program zapisuje dane](../dane-programu/) · [Problemy i najczęstsze pytania](../../problemy/)*
