---
title: Galeria warstw map
nav_order: 17.5
description: "Przegląd wyglądu warstw WMS, WMTS i XYZ dostępnych w MapoTero z podziałem na grupy tematyczne, filtrowaniem oraz parametrami przydatnymi przed pobraniem."
---

# Galeria warstw map
{: .no_toc }

Przegląd wyglądu warstw WMS, WMTS i XYZ dostępnych w MapoTero z podziałem na grupy tematyczne, filtrowaniem oraz parametrami przydatnymi przed pobraniem.
{: .fs-5 .fw-300 }

**Gdzie w programie:** karta *Katalog warstw* w oknie głównym (ikona galerii po prawej stronie nagłówka) albo menu *Pomoc > Galeria warstw map (podgląd online)*  
**Zobacz też:** [Jak działa Galeria i Mikser warstw (poradnik) →]({{ '/pomoc/galeria-i-mikser/' | relative_url }}) · [Katalog zbiorów map]({{ '/pomoc/zbiory-map/' | relative_url }}) · [Dotyczy wersji]({{ '/pomoc/' | relative_url }}): {{ site.wersja_programu }}

{: .informacja }
Próbki nakładek przezroczystych (np. działek ewidencyjnych lub szlaków) są oznaczone etykietą **Nakładka** i prezentowane na stonowanym podkładzie, aby zachować czytelność linii oraz napisów.

{: .wskazowka }
**Układanie warstw i dodawanie do programu:** kliknij **Dodaj do miksu**, ułóż kolejność, krycie i tryb mieszania w **Mikserze warstw** u dołu ekranu, a potem wyślij cały zestaw jednym kliknięciem do `MapoTero.exe` (albo kliknij **Wyślij**, aby dodać pojedynczą warstwę od razu). Jeśli otworzyłeś tę stronę z okna MapoTero, przeglądarka (np. Chrome, Edge) może wyświetlić jednorazowe okienko z pytaniem o zgodę na połączenie z siecią lokalną (`127.0.0.1`). Wyraź zgodę, ponieważ służy to wyłącznie do lokalnej komunikacji z programem i nie wysyła żadnych danych na zewnątrz.

<link rel="stylesheet" href="{{ '/pomoc/galeria/galeria.css' | relative_url }}?v={{ site.time | date: '%s' }}">

<div id="galeria-warstw-app"
  data-json-url="{{ '/pomoc/galeria/warstwy.json' | relative_url }}?v={{ site.time | date: '%s' }}"
  data-img-base="{{ '/pomoc/' | relative_url }}"
  data-zbiory-url="{{ '/pomoc/zbiory-map/' | relative_url }}">
<p class="gw-status">Wczytywanie galerii warstw…</p>
</div>

<noscript>
<p><strong>Uwaga:</strong> Przeglądarka ma wyłączoną obsługę JavaScript. Spis wszystkich zbiorów i warstw w formie tekstowej znajdziesz w rozdziale <a href="{{ '/pomoc/zbiory-map/' | relative_url }}">Katalog zbiorów map</a>.</p>
</noscript>

<script src="{{ '/pomoc/galeria/galeria.js' | relative_url }}?v={{ site.time | date: '%s' }}" defer></script>
