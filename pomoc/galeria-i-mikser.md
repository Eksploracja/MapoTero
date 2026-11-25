---
layout: default
title: Galeria i Mikser warstw
nav_order: 17.6
---

# Galeria i Mikser warstw
{: .no_toc }

**Galeria warstw** to przeglądarkowy katalog wszystkich zbiorów map dostępnych w MapoTero. Pozwala obejrzeć próbki map na żywo, sprawdzić ich zasięg geograficzny, a następnie wysłać wybraną warstwę albo całą wielowarstwową kompozycję wprost do otwartego programu.
{: .fs-6 .fw-300 }

---

## Spis treści
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Jak otworzyć Galerię warstw

Galerię możesz uruchomić na dwa sposoby:

* **Z programu MapoTero:** kliknij ikonę galerii (stos kart) w nagłówku karty *Warstwy do pobrania* albo wybierz z menu **Narzędzia > Galeria warstw map online**. Program otworzy stronę w przeglądarce z aktywnym połączeniem do działającego okna MapoTero.
* **Bezpośrednio w przeglądarce:** wejdź na stronę [Galeria warstw map](../galeria-warstw/). W tym trybie możesz przeglądać katalog, testować kompozycje w Mikserze oraz zapisywać przepisy do pliku `.mapotero` i schowka, a po uruchomieniu MapoTero jednym kliknięciem przesłać gotową kanapkę do programu.

{: .uwaga }
Gdy pierwszy raz wyślesz warstwę lub zestaw z przeglądarki do MapoTero, przeglądarka (Chrome, Edge, Opera) może wyświetlić pytanie o dostęp strony do sieci lokalnej lub adresu `127.0.0.1`. Kliknij **Zezwól**, aby strona mogła przekazywać wybrane warstwy bezpośrednio do uruchomionego programu.

---

## Przeglądanie katalogu i szczegóły warstwy

Każda karta w galerii prezentuje jedną warstwę mapową wraz z jej miniaturą, źródłem danych, rodzajem usługi (`WMS`, `WMTS` lub `XYZ`) oraz informacją o rozdzielczości.

![Widok Galerii warstw w przeglądarce z kartami map i paskiem Miksera na dole](../obrazy/17-galeria-warstw.png)

*Rysunek 1. Galeria warstw online: wyszukiwarka, filtry grup tematycznych, karty warstw i dolny pasek Miksera.*

* **Filtrowanie i wyszukiwanie:** wpisz fragment nazwy, temat albo nazwę instytucji w pole wyszukiwania u góry strony, albo wybierz jedną z grup tematycznych (np. *Ortofotomapy*, *Mapy topograficzne*, *Ewidencja gruntów i budynków*, *Rzeźba terenu*).
* **Podgląd i zasięg:** kliknij kartę warstwy (albo przycisk szczegółów), aby otworzyć okno z dwiema zakładkami:
  * **Próbka mapy** pokazuje rzeczywisty fragment mapy pobrany z serwera w wybranym miejscu i skali.
  * **Zasięg na mapie** wyświetla obszar pokrycia warstwy (np. całą Polskę, pojedyncze województwo lub pas przygraniczny) na czytelnej mapie wektorowej.
* **Szybkie wysłanie jednej warstwy:** przycisk **Wyślij** na karcie od razu dodaje tę warstwę do listy *Wybrane warstwy* w otwartym programie MapoTero.

---

## Mikser warstw: budowanie kompozycji w przeglądarce

Jeśli chcesz połączyć kilka map w jedną kompozycję (na przykład nałożyć ewidencję działek i cieniowanie terenu na ortofotomapę), nie musisz składać ich od zera po omacku. **Mikser warstw** pozwala ułożyć warstwy, ustawić ich przezroczystość oraz tryby mieszania i od razu zobaczyć wynikowy obraz w przeglądarce.

Aby dodać warstwę do kompozycji, kliknij **Dodaj do miksu** na jej karcie w galerii. Na dole ekranu pojawi się pasek roboczego miksu z miniaturami dodanych warstw i licznikiem (maksymalnie 12 warstw, tak samo jak w programie). Kliknij przycisk **Otwórz mikser**, aby przejść do pełnego widoku roboczego.

### Prawa kolumna: kolejność, krycie i tryby mieszania

![Okno Miksera warstw z podglądem na żywo oraz stosem warstw i trybami mieszania po prawej stronie](../obrazy/17-mikser-podglad.png)

*Rysunek 2. Mikser warstw: po lewej podgląd kompozycji na żywo, po prawej stos warstw z suwakami krycia i trybami mieszania.*

* **Kolejność warstw:** warstwa na samym dole listy stanowi nieprzezroczysty podkład, a warstwy nad nią nakładają się od dołu do góry. Kolejność zmienisz przyciskami **▲** i **▼** w wierszu warstwy. Przycisk **✕** usuwa warstwę ze stosu.
* **Krycie (`5–100%`):** suwak i pole liczbowe decydują, jak mocno warstwa przykrywa mapy leżące pod nią.
* **Tryb mieszania:** lista rozwijana pozwala wybrać jeden z siedmiu trybów dostępnych w MapoTero (`Zwykły`, `Mnożenie`, `Ciemniej`, `Jaśniej`, `Ekran`, `Nakładka`, `Rzeźba terenu`). Szczegółowy opis działania trybów znajdziesz na stronie [Kompozycje i nakładanie warstw](../pobieranie/kompozycje/).
* **Ułóż automatycznie:** jednym kliknięciem porządkuje wybrane warstwy według typowych zasad kartograficznych (podkład na dół, cieniowanie i hipsometria z odpowiednim trybem mieszania, warstwy wektorowe i napisy na wierzch).
* **Z programu:** pobiera do Miksera warstwy, które masz obecnie wybrane w oknie MapoTero. Możesz w ten sposób szybko poprawić lub rozbudować już rozpoczętą kompozycję.
* **Resetuj miks:** czyści stos, jeśli chcesz zacząć budowę zestawu od nowa.
* **Ostrzeżenia w stosie:** Mikser automatycznie podpowiada, gdy układ warstw ma oczywistą usterkę, na przykład gdy warstwa na samej górze ma `100%` krycia w trybie *Zwykły* i całkowicie zasłania resztę kompozycji.

### Lewa kolumna: podgląd wyniku oraz miejsce i obszar

Lewa część okna Miksera ma dwie zakładki przełączane u góry:

1. **Podgląd wyniku:** pokazuje złożoną kompozycję w wybranym miejscu. Możesz przełączać źródło podglądu między rzeczywistymi kafelkami **na żywo** z serwerów a lekką symulacją offline (przydatną przy wolnym łączu), zmieniać skalę podglądu (`Auto` albo od `0,5` do `25 m/px`) oraz przybliżać i oddalać kółkiem myszy.
2. **Miejsce i obszar:** wyświetla interaktywną mini-mapę, na której sprawdzisz i zmienisz lokalizację próbki oraz zaznaczysz gotowy obszar do pobrania w programie.

![Zakładka Miejsce i obszar w Mikserze warstw z mini-mapą i zaznaczonym obszarem](../obrazy/17-mikser-mapa.png)

*Rysunek 3. Zakładka Miejsce i obszar w Mikserze warstw: wybór miejsca próbki oraz zaznaczanie prostokąta obszaru prawym przyciskiem myszy.*

* **Szybkie skróty miejsc:** przyciski nad mini-mapą pozwalają błyskawicznie przeskoczyć w charakterystyczne miejsca (np. Tatry, centrum miasta czy obszar leśny), aby przetestować kompozycję w odpowiednim terenie.
* **Zmiana punktu próbki:** przesuń krzyżyk na mini-mapie albo wpisz współrzędne, aby zobaczyć podgląd dokładnie w miejscu, które zamierzasz pobrać.
* **Zaznaczanie obszaru:** przytrzymaj **prawy przycisk myszy** i przeciągnij po mini-mapie, aby narysować prostokąt obszaru. Jeśli klikniesz **Dopasuj podgląd do obszaru**, podgląd kompozycji dostosuje się do wybranego zasięgu.

---

## Wysyłanie do MapoTero i zapisywanie przepisów

Gdy kompozycja wygląda tak, jak trzeba, możesz ją wykorzystać na kilka sposobów:

* **Wyślij do MapoTero:** przekazuje cały stos warstw (wraz z kolejnością, kryciem i trybami mieszania) wprost na listę *Wybrane warstwy* w otwartym programie. Jeśli w Mikserze zaznaczyłeś także obszar na mini-mapie, program przejmie również ten zasięg.
* **Otwórz w Szybkim Kreatorze:** uruchamia w MapoTero [Szybki kreator mapy](../narzedzia/#szybki-kreator-mapy) z przygotowanym już własnym zestawem warstw. Jeśli w miksie łączysz mapy o dużej rozpiętości skal (np. mapę przeglądową, topograficzną i ortofotomapę), MapoTero zaproponuje automatyczne rozdzielenie ich na osobne stopnie skali (mapę wieloskalową MBTiles). W kreatorze możesz zapisać taki zestaw jako swój stały szablon albo wyeksportować go (wraz z zakresami zoom) do pliku `.mapotero.json`.
* **Zapisz jako plik `.mapotero`:** pobiera niewielki plik przepisu w formacie JSON (`mapotero-zestaw`). Taki plik możesz zachować na dysku, wysłać znajomym albo przeciągnąć myszą z Eksploratora Windows wprost na okno MapoTero (Drag & Drop), aby natychmiast wczytać całą kompozycję.
* **Kopiuj link lub JSON:** kopiuje adres URL galerii z zakodowaną kompozycją (`?kanapka=...`) albo surowy tekst JSON do schowka. W programie MapoTero wystarczy kliknąć prawym przyciskiem myszy na liście *Wybrane warstwy* i wybrać **Wklej kanapkę ze schowka**.

{: .wskazowka }
Jeśli chcesz przenieść gotową kompozycję z okna MapoTero do Miksera lub zapisać ją dla kolegi, kliknij prawym przyciskiem myszy na liście *Wybrane warstwy* w programie i wybierz **Kopiuj kanapkę (JSON)**.
