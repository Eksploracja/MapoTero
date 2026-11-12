---
title: Plik zbioru map
nav_order: 2
parent: Dodawanie serwerów map
---

# Plik zbioru map
{: .no_toc }

Zbiór map to plik `.txt` w folderze `warstwy\`. W zależności od typu usługi ma klasyczny format tekstowy albo rozszerzony format JSON.
{: .fs-5 .fw-300 }

**Gdzie w programie:** folder `warstwy\` obok `MapoTero.exe` (lub `%LocalAppData%\MapoTero\warstwy`)  
**Dotyczy wersji:** {{ site.wersja_programu }}

<details open markdown="block">
<summary>Na tej stronie</summary>
{: .text-delta }
- TOC
{:toc}
</details>

## Dwa formaty plików

Program automatycznie rozpoznaje dwa formaty plików `.txt`:

1. **Format klasyczny (Windows-1250)**: prosty plik tekstowy kompatybilny ze starszymi wersjami MapoTero (dla zwykłych usług WMS i WMTS bez dodatkowych opcji).
2. **Format JSON (UTF-8)**: rozpoznawany po nawiasie `{` na początku pliku. Obsługuje usługi **XYZ / TMS**, subdomeny, zakresy zoom, nagłówek `referer`, atrybucję, ignorowanie błędów SSL oraz czytelne tytuły.

---

## Format klasyczny (WMS i WMTS)

Pierwsze pole to adres bazowy serwera, a kolejne to pary: **nazwa warstwy na serwerze** i **zalecany rozmiar piksela** (z kropką dziesiętną).

* Dla **WMS** adres powinien prowadzić do operacji `GetMap` (z parametrami `version=`, `CRS=`/`SRS=` i pustym `layers=` na końcu).
* Dla **WMTS** program rozpoznaje usługę po obecności `SERVICE=WMTS` lub `/WMTS` w adresie, a nazwą warstwy jest identyfikator `Identifier` z dokumentu `GetCapabilities`.

```text
https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolution?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=
Raster
0.25
```

---

## Format JSON (XYZ, TMS i rozszerzone metadane)

Zalecany przy własnych serwerach kafelkowych `{z}/{x}/{y}` lub `{-y}` oraz wtedy, gdy chcesz ustawić czytelny tytuł,
limit wątków lub nagłówek `referer`:

```json
{
  "typ": "xyz",
  "tytul": "Moja mapa turystyczna",
  "adres": "https://{s}.example.org/tiles/{z}/{x}/{y}.png",
  "subdomeny": ["a", "b", "c"],
  "zMin": 6,
  "zMax": 17,
  "rozmiarKafla": 256,
  "nakladka": false,
  "maksWatkow": 2,
  "odstepMs": 100,
  "referer": "https://example.org/",
  "atrybucja": "© Moje źródło map",
  "ignorujSsl": false,
  "zasieg": [14.0, 49.0, 24.2, 55.0],
  "warstwy": [
    {
      "nazwa": "szlaki",
      "tytul": "Szlaki turystyczne",
      "piksel": "1",
      "zMin": 8,
      "zMax": 17,
      "nakladka": true
    }
  ]
}
```

| Pole | Znaczenie |
|---|---|
| `typ` | `"wms"`, `"wmts"`, `"xyz"` albo `"tms"` |
| `adres` | adres WMS/WMTS lub szablon kafli (`{z}`, `{x}`, `{y}`, `{-y}`, `{quadkey}`, `{s}`) |
| `subdomeny` | lista podstawiana za `{s}` (np. `["a", "b", "c"]`) |
| `zMin` / `zMax` | minimalny i maksymalny poziom powiększeń (`0–22`) |
| `nakladka` | `true` dla warstw przezroczystych (odpowiedź `HTTP 404` daje pusty kafel) |
| `maksWatkow`, `odstepMs` | ograniczenie zapytań do serwera kafli (`ms` odstępu między żądaniami) |
| `referer`, `atrybucja`, `format` | opcjonalny nagłówek HTTP `Referer`, tekst źródła do metadanych oraz jawny typ MIME |
| `zasieg` | opcjonalny prostokąt w WGS84 `[zachód, południe, wschód, północ]` do ostrzeżeń poza obszarem |
| `warstwy` | tablica warstw (`nazwa` techniczna, opcjonalny `tytul`, zalecany `piksel`, opcjonalne nadpisania `zMin`/`zMax`/`nakladka`) |

---

## Rejestracja i udostępnianie zbiorów

Aby ręcznie utworzony plik `<moj_zbior>.txt` pojawił się w programie:

1. Zapisz go w folderze `warstwy\` (albo `%LocalAppData%\MapoTero\warstwy\`).
2. Dopisz jego nazwę (bez `.txt`) jako nowy wiersz w pliku `warstwy\warstwy.txt`.
3. Po ponownym uruchomieniu MapoTero zbiór pojawi się w drzewie *Wybierz mapy* w grupie **Dodane serwery** (chyba że dopiszesz go także do grup w pliku `warstwy\katalog.txt`, kodowanie UTF-8).

---

➡️ **Kolejny krok:** [Narzędzia dodatkowe →](../../narzedzia/)

*Zobacz też: [Kreator serwerów](../kreator/) · [Formaty plików](../../formaty-plikow/) · [Katalog zbiorów map](../../zbiory-map/)*
