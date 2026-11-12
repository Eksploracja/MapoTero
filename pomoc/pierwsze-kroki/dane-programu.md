---
title: Gdzie program zapisuje dane
nav_order: 2
parent: Pierwsze kroki
---

# Gdzie program zapisuje dane
{: .no_toc }

Program zapisuje ustawienia i pobrane mapy obok siebie, a gdy tam nie może, w folderze użytkownika.
{: .fs-5 .fw-300 }

**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Folder danych

Przy każdym uruchomieniu program sprawdza możliwość zapisu plików w swoim folderze (tam, gdzie leży `MapoTero.exe`).

* **Tak** (np. `D:\MapoTero`): folderem danych jest bezpośrednio folder programu.
* **Nie** (np. program umieszczony w `C:\Program Files\MapoTero`): folderem danych staje się `%LocalAppData%\MapoTero` (ścieżka `C:\Users\<nazwa użytkownika>\AppData\Local\MapoTero`).

{: .wskazowka }
Aby otworzyć folder użytkownika, wpisz `%LocalAppData%\MapoTero` w pasku adresu Eksploratora plików i naciśnij Enter.

Folder bieżącej sesji jest widoczny na pasku stanu u dołu okna głównego.

---

## Pliki i foldery programu

| Plik lub folder | Gdzie | Zawartość |
|---|---|---|
| `MapoTero.exe` | folder programu | program |
| `warstwy\` | folder programu | zbiory map dostarczone z programem: plik `warstwy.txt` z indeksem zbiorów, `katalog.txt` ze strukturą grup i czytelnymi nazwami oraz pliki `.txt` poszczególnych zbiorów |
| `warstwy\` | folder danych (gdy inny niż folder programu) | zbiory map dodane [kreatorem](../../serwery/kreator/), gdy folder programu ma zablokowany zapis |
| `lastsettings.txt` | folder danych | konfiguracja programu: opcje z okna *Ustawienia*, widok mapy, szerokość panelu i parametry eksportu do CAD. Zapisywany przy zamykaniu programu i po kliknięciu *OK* w *Ustawieniach* |
| `download\` | folder danych | domyślny folder sesji oraz podfoldery kolejnych pobrań (zob. [Sesje pobierania](../../sesje/)) |
| `awarie.log` | folder danych | rejestr nieoczekiwanych błędów programu, pomocny przy zgłaszaniu usterek (zob. [Problemy](../../problemy/)) |

Zawartość folderu pojedynczej sesji pobierania opisuje rozdział [Sesje pobierania](../../sesje/).

{: .uwaga }
Przycisk *usuń wszystkie pobrane mapy* w oknie głównym kasuje **cały folder `download`** wraz ze wszystkimi sesjami. Mapy, które chcesz zachować, przenieś wcześniej poza folder `download` lub wykonaj ich kopię zapasową.

---

## Przenoszenie programu i danych

* **Na inny dysk lub komputer:** skopiuj cały folder programu (razem z plikiem `lastsettings.txt` i katalogiem `download`, jeśli tam się znajdują). Gdy dane były zapisywane w `%LocalAppData%\MapoTero`, skopiuj również ten katalog.
* **Kopia zapasowa map:** wystarczy skopiować podfoldery sesji z katalogu `download`. Każdy z nich zawiera kompletny plik `conf.txt`, co pozwala w dowolnej chwili [wczytać sesję](../../sesje/) i scalić arkusz lub wyeksportować mapę.
* **Własne zbiory map:** skopiuj ich pliki `.txt` z folderu `warstwy` i dopisz nazwy do `warstwy.txt` w docelowej instalacji.

---

➡️ **Kolejny krok:** [Pierwsza mapa krok po kroku →](../pierwsza-mapa/)

*Zobacz też: [Instalacja i uruchomienie](../instalacja/) · [Sesje pobierania](../../sesje/) · [Formaty plików](../../formaty-plikow/)*
