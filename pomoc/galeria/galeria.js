(function (global) {
  "use strict";

  const MAKS_WARSTW_KANAPKI = 12;
  const MIN_KRYCIE = 5;
  const PELNE_KRYCIE = 100;
  const MAKS_PIKSEL_PODGLADU = 25;
  const MAKS_PIKSEL_DOPASOWANIA_BBOX = 15;
  const STOPNIE_SKALI_PODGLADU = [0.5, 1, 2, 4, 10, 25];
  const KLUCZ_LOCAL_STORAGE = "mapotero_galeria_kanapka_v1";

  const TRYBY_MIESZANIA = [
    { kod: "Zwykly", numer: 0, nazwa: "1 Zwykły", css: "source-over" },
    { kod: "Mnozenie", numer: 1, nazwa: "2 Mnożenie", css: "multiply" },
    { kod: "Ciemniej", numer: 2, nazwa: "3 Ciemniej", css: "darken" },
    { kod: "Jasniej", numer: 3, nazwa: "4 Jaśniej", css: "lighten" },
    { kod: "Ekran", numer: 4, nazwa: "5 Ekran", css: "screen" },
    { kod: "Nakladka", numer: 5, nazwa: "6 Nakładka", css: "overlay" },
    { kod: "RzezbaTerenu", numer: 6, nazwa: "7 Rzeźba terenu", css: null }
  ];

  const SKROTY_MIEJSC = [
    { nazwa: "Warszawa", lat: 52.2297, lon: 21.0122 },
    { nazwa: "Tatry", lat: 49.275, lon: 19.975 },
    { nazwa: "Kraków", lat: 50.0614, lon: 19.9366 },
    { nazwa: "Gdańsk", lat: 54.352, lon: 18.6466 },
    { nazwa: "Wrocław", lat: 51.1079, lon: 17.0385 }
  ];

  function poprawneKrycie(k) {
    const num = Math.round(Number(k));
    if (!Number.isFinite(num)) return PELNE_KRYCIE;
    return Math.max(MIN_KRYCIE, Math.min(PELNE_KRYCIE, num));
  }

  function znajdzTryb(kodLubNumer) {
    if (kodLubNumer === undefined || kodLubNumer === null) return TRYBY_MIESZANIA[0];
    const s = String(kodLubNumer).trim().toLowerCase();
    for (let i = 0; i < TRYBY_MIESZANIA.length; i++) {
      const t = TRYBY_MIESZANIA[i];
      if (String(t.numer) === s || t.kod.toLowerCase() === s || t.nazwa.toLowerCase() === s) {
        return t;
      }
    }
    return TRYBY_MIESZANIA[0];
  }

  function czyWarstwaCieniowania(w) {
    if (!w) return false;
    const tekst = ((w.zbior || "") + " " + (w.warstwa || "") + " " + (w.nazwa || "")).toLowerCase();
    return tekst.includes("cieniow") || tekst.includes("hillshade") || tekst.includes("shaded");
  }

  function utworzPozycjeKanapki(w, krycie, tryb) {
    return {
      id: w.id || (w.zbior + "__" + w.warstwa),
      zbior: w.zbior,
      warstwa: w.warstwa,
      nazwa: w.nazwa || w.warstwa,
      typ: w.typ || "WMS",
      nakladka: Boolean(w.nakladka),
      zalecany_piksel: Number(w.zalecany_piksel) || 0,
      zasieg: w.zasieg || { typ: "kraj", kod: "PL", etykieta: "Cała Polska" },
      probka_miksu: w.probka_miksu || null,
      obrazek: w.obrazek || null,
      krycie: poprawneKrycie(krycie === undefined ? PELNE_KRYCIE : krycie),
      tryb: znajdzTryb(tryb).kod
    };
  }

  function znajdzPrzykrytaNakladke(kanapka) {
    for (let i = 0; i < kanapka.length - 1; i++) {
      if (!kanapka[i].nakladka) continue;
      for (let j = i + 1; j < kanapka.length; j++) {
        if (!kanapka[j].nakladka && kanapka[j].krycie >= PELNE_KRYCIE && kanapka[j].tryb === "Zwykly") {
          return kanapka[i];
        }
      }
    }
    return null;
  }

  function najdokladniejszyPikselKanapki(kanapka) {
    let minPx = 0;
    for (let i = 0; i < kanapka.length; i++) {
      const px = Number(kanapka[i].zalecany_piksel) || 0;
      if (px > 0 && (minPx === 0 || px < minPx)) minPx = px;
    }
    return minPx;
  }

  function ostrzezeniaKanapki(kanapka) {
    const lista = [];
    if (!kanapka || kanapka.length === 0) return lista;

    const przykryta = znajdzPrzykrytaNakladke(kanapka);
    if (przykryta) {
      lista.push("Uwaga: nakładka „" + przykryta.nazwa + "” leży pod pełnokryjącą warstwą i może zostać zasłonięta.");
    } else {
      // Sprawdź także czy cieniowanie rzeźby nie zostało przykryte pełnokryjącym podkładem powyżej
      for (let i = 0; i < kanapka.length - 1; i++) {
        if (!czyWarstwaCieniowania(kanapka[i])) continue;
        let zaslonieta = false;
        for (let j = i + 1; j < kanapka.length; j++) {
          if (!kanapka[j].nakladka && kanapka[j].krycie >= PELNE_KRYCIE && kanapka[j].tryb === "Zwykly") {
            zaslonieta = true;
            break;
          }
        }
        if (zaslonieta) {
          lista.push("Uwaga: cieniowanie „" + kanapka[i].nazwa + "” leży pod pełnokryjącą warstwą i może zostać zasłonięte (użyj „⚡ Ułóż automatycznie”).");
          break;
        }
      }
    }

    if (kanapka.every(function (x) { return x.nakladka; })) {
      lista.push("Wybrano same przezroczyste nakładki — warto dodać podkład na dół kanapki (np. topo lub ortofoto).");
    }

    const kodyZagraniczne = new Set();
    let maPl = false;
    kanapka.forEach(function (x) {
      const z = x.zasieg || {};
      if (z.typ === "zagranica" && z.kod) kodyZagraniczne.add(z.kod);
      else if (z.typ === "kraj" || z.typ === "regionalny") maPl = true;
    });
    if (kodyZagraniczne.size > 1 || (kodyZagraniczne.size > 0 && maPl)) {
      lista.push("Warstwy w kanapce mają różne zasięgi geograficzne (np. Polska i zagranica) — upewnij się, że pokrywają Twój obszar.");
    }

    return lista;
  }

  function ulozKanapkeAutomatycznie(kanapka) {
    const podklady = [];
    const cieniowanie = [];
    const nakladki = [];
    kanapka.forEach(function (item) {
      const kopia = Object.assign({}, item);
      if (czyWarstwaCieniowania(kopia)) {
        if (kopia.tryb === "Zwykly") kopia.tryb = "RzezbaTerenu";
        if (kopia.krycie === PELNE_KRYCIE) kopia.krycie = 70;
        cieniowanie.push(kopia);
      } else if (kopia.nakladka) {
        nakladki.push(kopia);
      } else {
        podklady.push(kopia);
      }
    });
    return podklady.concat(cieniowanie, nakladki);
  }

  function kodujKanapkeUrl(kanapka) {
    if (!kanapka || kanapka.length === 0) return "";
    return kanapka.map(function (item) {
      const t = znajdzTryb(item.tryb).numer;
      return item.id + ":" + poprawneKrycie(item.krycie) + ":" + t;
    }).join(";");
  }

  function dekodujKanapkeUrl(tekst, wszystkieWarstwy) {
    if (!tekst || !Array.isArray(wszystkieWarstwy)) return [];
    const poId = new Map();
    const poKluczu = new Map();
    wszystkieWarstwy.forEach(function (w) {
      if (w.id) poId.set(w.id.toLowerCase(), w);
      poKluczu.set((w.zbior + "~" + w.warstwa).toLowerCase(), w);
    });

    const wynik = [];
    const uzyte = new Set();
    tekst.split(";").forEach(function (czesc) {
      if (wynik.length >= MAKS_WARSTW_KANAPKI) return;
      const seg = czesc.trim();
      if (!seg) return;
      const pola = seg.split(":");
      const klucz = (pola[0] || "").trim().toLowerCase();
      const warstwa = poId.get(klucz) || poKluczu.get(klucz);
      if (!warstwa) return;
      const kluczUnikalny = (warstwa.zbior + "|" + warstwa.warstwa).toLowerCase();
      if (uzyte.has(kluczUnikalny)) return;
      uzyte.add(kluczUnikalny);
      const krycie = pola.length > 1 ? parseInt(pola[1], 10) : PELNE_KRYCIE;
      const tryb = pola.length > 2 ? pola[2] : "0";
      wynik.push(utworzPozycjeKanapki(warstwa, krycie, tryb));
    });
    return wynik;
  }

  function budujPrzepisJson(kanapka, opcje) {
    const o = opcje || {};
    const wynik = {
      format: "mapotero-zestaw",
      wersja: 1,
      nazwa: (o.nazwa || "").trim(),
      cel: o.cel || "okno_glowne",
      zapiszStyl: Boolean(o.zapiszStyl),
      warstwy: (kanapka || []).map(function (item) {
        return {
          zbior: item.zbior,
          warstwa: item.warstwa,
          krycie: poprawneKrycie(item.krycie),
          tryb: znajdzTryb(item.tryb).kod
        };
      })
    };
    if (Array.isArray(o.obszarWgs84) && o.obszarWgs84.length === 4) {
      wynik.obszarWgs84 = o.obszarWgs84;
    }
    return wynik;
  }

  function wspolczynnikRzezby(szarosc01) {
    const neutralna = 180.0 / 255.0;
    return Math.max(0.0, Math.min(1.2, szarosc01 / neutralna));
  }

  function nalozRzezbeTerenuNaPiksele(bazaData, warstwaData, krycieProcent) {
    const alphaWsp = poprawneKrycie(krycieProcent) / 100.0;
    const len = bazaData.length;
    for (let i = 0; i < len; i += 4) {
      const wa = warstwaData[i + 3] / 255.0;
      const eff = wa * alphaWsp;
      if (eff <= 0) continue;

      const szarosc = (0.299 * warstwaData[i] + 0.587 * warstwaData[i + 1] + 0.114 * warstwaData[i + 2]) / 255.0;
      const wsp = wspolczynnikRzezby(szarosc);

      const br = bazaData[i];
      const bg = bazaData[i + 1];
      const bb = bazaData[i + 2];
      const ba = bazaData[i + 3];

      const docR = Math.min(255, Math.round(br * wsp));
      const docG = Math.min(255, Math.round(bg * wsp));
      const docB = Math.min(255, Math.round(bb * wsp));

      if (ba === 0) {
        bazaData[i] = warstwaData[i];
        bazaData[i + 1] = warstwaData[i + 1];
        bazaData[i + 2] = warstwaData[i + 2];
        bazaData[i + 3] = Math.round(255 * eff);
      } else {
        bazaData[i] = Math.round(br * (1 - eff) + docR * eff);
        bazaData[i + 1] = Math.round(bg * (1 - eff) + docG * eff);
        bazaData[i + 2] = Math.round(bb * (1 - eff) + docB * eff);
      }
    }
  }

  // Konwersja kafelków Web Mercator dla lekkiej mini-mapy wyboru miejsca próbki
  function latLonDoKafelka(lat, lon, zoom) {
    const n = Math.pow(2, zoom);
    const x = ((lon + 180) / 360) * n;
    const latRad = (lat * Math.PI) / 180;
    const y = ((1 - Math.log(Math.tan(latRad) + 1 / Math.cos(latRad)) / Math.PI) / 2) * n;
    return { x: x, y: y };
  }

  function kafelekDoLatLon(x, y, zoom) {
    const n = Math.pow(2, zoom);
    const lon = (x / n) * 360 - 180;
    const latRad = Math.atan(Math.sinh(Math.PI * (1 - (2 * y) / n)));
    const lat = (latRad * 180) / Math.PI;
    return { lat: lat, lon: lon };
  }

  function obliczPikselDlaBbox(bbox, bokPx) {
    if (!Array.isArray(bbox) || bbox.length !== 4) return 2.0;
    const minLon = Math.min(bbox[0], bbox[2]);
    const maxLon = Math.max(bbox[0], bbox[2]);
    const minLat = Math.min(bbox[1], bbox[3]);
    const maxLat = Math.max(bbox[1], bbox[3]);
    const srLat = (minLat + maxLat) / 2;
    const szerM = Math.abs(maxLon - minLon) * 111320 * Math.cos((srLat * Math.PI) / 180);
    const wysM = Math.abs(maxLat - minLat) * 111320;
    const maksM = Math.max(szerM, wysM, 180);
    return Math.max(0.5, Math.round((maksM / (bokPx || 360)) * 100) / 100);
  }

  function bboxZCentrumIPiksela(lat, lon, pikselMetry, bokPx) {
    const polM = ((bokPx || 360) * Math.max(0.25, pikselMetry || 2.0)) / 2;
    const dLat = polM / 111320;
    const cosLat = Math.max(0.2, Math.cos((lat * Math.PI) / 180));
    const dLon = polM / (111320 * cosLat);
    return [lon - dLon, lat - dLat, lon + dLon, lat + dLat];
  }

  function dopasujZoomDoBbox(bbox, szerPx, wysPx) {
    if (!Array.isArray(bbox) || bbox.length !== 4) return 11;
    const minLon = Math.min(bbox[0], bbox[2]);
    const maxLon = Math.max(bbox[0], bbox[2]);
    const minLat = Math.min(bbox[1], bbox[3]);
    const maxLat = Math.max(bbox[1], bbox[3]);
    for (let z = 15; z >= 5; z--) {
      const p1 = latLonDoKafelka(maxLat, minLon, z);
      const p2 = latLonDoKafelka(minLat, maxLon, z);
      const dx = Math.abs(p2.x - p1.x) * 256;
      const dy = Math.abs(p2.y - p1.y) * 256;
      if (dx <= szerPx * 0.8 && dy <= wysPx * 0.8) return z;
    }
    return 5;
  }

  // Dla dużego zaznaczonego obszaru nie rozciągamy próbki 360x360 na setki metrów/piksel
  // (serwery WMTS/WMS blokują zbyt grube skale) — zostajemy przy skali roboczej ze środka obszaru.
  function wybierzPikselPoZaznaczeniuBbox(bbox, aktualnyPiksel) {
    const obszarPiksel = obliczPikselDlaBbox(bbox, 360);
    if (obszarPiksel <= MAKS_PIKSEL_DOPASOWANIA_BBOX) {
      return {
        piksel: obszarPiksel,
        obszarPiksel: obszarPiksel,
        rozdzielony: false
      };
    }
    const bezpieczny = Number(aktualnyPiksel) > 0 && Number(aktualnyPiksel) <= MAKS_PIKSEL_PODGLADU
      ? Number(aktualnyPiksel)
      : 0;
    return {
      piksel: bezpieczny,
      obszarPiksel: obszarPiksel,
      rozdzielony: true
    };
  }

  // Zwraca kolejny stopień rozdzielczości podglądu: kierunek > 0 przybliża (mniejszy m/px), < 0 oddala (większy m/px).
  function wybierzNastepnyStopienSkali(aktualnyPiksel, ostatniPikselRenderu, kierunek) {
    const baza = Number(aktualnyPiksel) > 0
      ? Number(aktualnyPiksel)
      : (Number(ostatniPikselRenderu) > 0 ? Number(ostatniPikselRenderu) : 2.0);
    if (kierunek > 0) {
      for (let i = STOPNIE_SKALI_PODGLADU.length - 1; i >= 0; i--) {
        if (STOPNIE_SKALI_PODGLADU[i] < baza * 0.95) return STOPNIE_SKALI_PODGLADU[i];
      }
      return STOPNIE_SKALI_PODGLADU[0];
    }
    for (let i = 0; i < STOPNIE_SKALI_PODGLADU.length; i++) {
      if (STOPNIE_SKALI_PODGLADU[i] > baza * 1.05) return STOPNIE_SKALI_PODGLADU[i];
    }
    return STOPNIE_SKALI_PODGLADU[STOPNIE_SKALI_PODGLADU.length - 1];
  }

  // Grupuje sąsiednie elementy o tej samej podgrupie w kolejności z katalogu (bez wypychania pustej podgrupy na początek).
  function zgrupujPodgrupyWKolejnosci(elementy) {
    const bloki = [];
    if (!Array.isArray(elementy)) return bloki;
    elementy.forEach(function (pozycja) {
      const warstwa = pozycja && pozycja.warstwa && typeof pozycja.warstwa === "object" ? pozycja.warstwa : pozycja;
      const nazwaPod = (warstwa && warstwa.podgrupa) || "";
      const ostatni = bloki.length > 0 ? bloki[bloki.length - 1] : null;
      if (ostatni && ostatni.nazwa === nazwaPod) {
        ostatni.elementy.push(pozycja);
      } else {
        bloki.push({ nazwa: nazwaPod, elementy: [pozycja] });
      }
    });
    return bloki;
  }

  if (typeof module !== "undefined" && module.exports) {
    module.exports = {
      MAKS_WARSTW_KANAPKI,
      MAKS_PIKSEL_PODGLADU,
      MAKS_PIKSEL_DOPASOWANIA_BBOX,
      STOPNIE_SKALI_PODGLADU,
      TRYBY_MIESZANIA,
      poprawneKrycie,
      znajdzTryb,
      utworzPozycjeKanapki,
      znajdzPrzykrytaNakladke,
      najdokladniejszyPikselKanapki,
      ostrzezeniaKanapki,
      ulozKanapkeAutomatycznie,
      kodujKanapkeUrl,
      dekodujKanapkeUrl,
      budujPrzepisJson,
      wspolczynnikRzezby,
      nalozRzezbeTerenuNaPiksele,
      latLonDoKafelka,
      kafelekDoLatLon,
      obliczPikselDlaBbox,
      bboxZCentrumIPiksela,
      dopasujZoomDoBbox,
      wybierzPikselPoZaznaczeniuBbox,
      wybierzNastepnyStopienSkali,
      zgrupujPodgrupyWKolejnosci
    };
  }

  if (typeof document === "undefined") return;

  const kontener = document.getElementById("galeria-warstw-app");
  if (!kontener) return;

  const jsonUrl = kontener.getAttribute("data-json-url") || "galeria/warstwy.json";
  const imgBase = kontener.getAttribute("data-img-base") || "";
  const zbioryUrl = kontener.getAttribute("data-zbiory-url") || "../zbiory-map/";

  const stan = {
    dane: null,
    warstwy: [],
    widoczne: [],
    szukaj: "",
    grupa: "WSZYSTKIE",
    typ: "WSZYSTKIE",
    rola: "WSZYSTKIE",
    rozmiar: "STANDARD",
    indeksModala: -1,
    widokModala: "probka",
    port: 0,
    token: "",
    funkcjeProgramu: [],
    wersjaProgramu: "",
    kanapka: [],
    kanapkaUrlStart: "",
    nazwaKanapki: "",
    zapiszJakoStyl: false,
    mikserOtwarty: false,
    obszarProgramu: null,
    podgladLat: 52.2297,
    podgladLon: 21.0122,
    podgladPiksel: 0,
    ostatniPikselRenderu: 2.0,
    zaznaczonyBbox: null,
    przekazObszarDoMapoTero: true,
    dokZwiniety: false,
    zakladkaMiksera: "podglad",
    mapaZoom: 11,
    mapaCenterLat: 52.2297,
    mapaCenterLon: 21.0122,
    mapaTryb: "przesun",
    cacheKafelkowOsm: new Map(),
    abortPodglad: null,
    timerPodgladu: null,
    tokenPodgladuOffline: 0,
    cacheObrazow: new Map()
  };

  let ref = {};

  function escapeHtml(tekst) {
    if (!tekst) return "";
    return String(tekst)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function normalizuj(tekst) {
    if (!tekst) return "";
    return String(tekst)
      .toLowerCase()
      .replace(/ł/g, "l")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "");
  }

  function el(tag, klasa, tekst) {
    const node = document.createElement(tag);
    if (klasa) node.className = klasa;
    if (tekst !== undefined && tekst !== null) node.textContent = tekst;
    return node;
  }

  function utworzIkoneMapySvg() {
    const ns = "http://www.w3.org/2000/svg";
    const svg = document.createElementNS(ns, "svg");
    svg.setAttribute("viewBox", "0 0 24 24");
    svg.setAttribute("fill", "none");
    svg.setAttribute("stroke", "currentColor");
    svg.setAttribute("stroke-width", "1.6");
    svg.setAttribute("stroke-linecap", "round");
    svg.setAttribute("stroke-linejoin", "round");
    svg.setAttribute("class", "gw-placeholder-ikona");
    svg.setAttribute("aria-hidden", "true");

    const poly = document.createElementNS(ns, "polygon");
    poly.setAttribute("points", "1 6 1 22 8 18 16 22 23 18 23 2 16 6 8 2 1 6");
    const l1 = document.createElementNS(ns, "line");
    l1.setAttribute("x1", "8");
    l1.setAttribute("y1", "2");
    l1.setAttribute("x2", "8");
    l1.setAttribute("y2", "18");
    const l2 = document.createElementNS(ns, "line");
    l2.setAttribute("x1", "16");
    l2.setAttribute("y1", "6");
    l2.setAttribute("x2", "16");
    l2.setAttribute("y2", "22");

    svg.appendChild(poly);
    svg.appendChild(l1);
    svg.appendChild(l2);
    return svg;
  }

  function utworzPlaceholder(zbior) {
    const box = el("div", "gw-placeholder");
    box.appendChild(utworzIkoneMapySvg());
    box.appendChild(el("span", null, "Miniatura oczekuje na wygenerowanie (" + zbior + ")"));
    return box;
  }

  function formatujPiksel(wartosc) {
    if (wartosc === null || wartosc === undefined || wartosc === "" || Number(wartosc) <= 0) return "—";
    return String(wartosc).replace(".", ",") + " m/px";
  }

  function sciezkaObrazka(sciezka) {
    if (!sciezka) return "";
    if (/^https?:\/\//i.test(sciezka) || sciezka.startsWith("/")) return sciezka;
    return imgBase + sciezka;
  }

  function klasaTypu(typ) {
    const t = (typ || "").toUpperCase();
    if (t === "WMTS") return "gw-odznaka gw-odznaka-wmts";
    if (t === "WMS") return "gw-odznaka gw-odznaka-wms";
    if (t === "XYZ" || t === "TMS") return "gw-odznaka gw-odznaka-xyz";
    return "gw-odznaka";
  }

  function czyPolaczonoZProgramem() {
    return stan.port > 0 && stan.token !== "";
  }

  function maFunkcjeProgramu(nazwa) {
    return Array.isArray(stan.funkcjeProgramu) && stan.funkcjeProgramu.indexOf(nazwa) >= 0;
  }

  function odmienWarstwy(n) {
    const abs = Math.abs(Math.floor(n));
    if (abs === 1) return abs + " warstwa";
    const mod100 = abs % 100;
    const mod10 = abs % 10;
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return abs + " warstwy";
    return abs + " warstw";
  }

  function kopiujTekstDoSchowka(tekst) {
    if (navigator.clipboard && typeof navigator.clipboard.writeText === "function") {
      return navigator.clipboard.writeText(tekst).catch(function () {
        return kopiujPrzezTextarea(tekst);
      });
    }
    return kopiujPrzezTextarea(tekst);
  }

  function kopiujPrzezTextarea(tekst) {
    try {
      const ta = document.createElement("textarea");
      ta.value = tekst;
      ta.setAttribute("readonly", "");
      ta.style.position = "fixed";
      ta.style.left = "-9999px";
      document.body.appendChild(ta);
      ta.select();
      const ok = document.execCommand("copy");
      document.body.removeChild(ta);
      return ok ? Promise.resolve() : Promise.reject(new Error("Brak dostępu do schowka"));
    } catch (e) {
      return Promise.reject(e);
    }
  }

  function wczytajParametryUrl() {
    const params = new URLSearchParams(window.location.search);
    if (params.has("q")) stan.szukaj = params.get("q") || "";
    if (params.has("grupa")) stan.grupa = params.get("grupa") || "WSZYSTKIE";
    if (params.has("typ")) stan.typ = (params.get("typ") || "WSZYSTKIE").toUpperCase();
    if (params.has("rola")) stan.rola = (params.get("rola") || "WSZYSTKIE").toUpperCase();
    if (params.has("kanapka")) stan.kanapkaUrlStart = (params.get("kanapka") || "").trim();
    const p = parseInt(params.get("port") || "0", 10);
    const t = (params.get("token") || "").trim();
    if (p > 0 && p <= 65535 && t) {
      stan.port = p;
      stan.token = t;
    }
  }

  function zapiszParametryUrl() {
    const params = new URLSearchParams();
    if (stan.szukaj.trim()) params.set("q", stan.szukaj.trim());
    if (stan.grupa !== "WSZYSTKIE") params.set("grupa", stan.grupa);
    if (stan.typ !== "WSZYSTKIE") params.set("typ", stan.typ);
    if (stan.rola !== "WSZYSTKIE") params.set("rola", stan.rola);
    if (stan.kanapkaUrlStart && stan.kanapka.length > 0) {
      params.set("kanapka", kodujKanapkeUrl(stan.kanapka));
    }
    if (czyPolaczonoZProgramem()) {
      params.set("port", String(stan.port));
      params.set("token", stan.token);
    }
    const ciag = params.toString();
    const nowyUrl = window.location.pathname + (ciag ? "?" + ciag : "") + window.location.hash;
    window.history.replaceState(null, "", nowyUrl);
  }

  function zbudujLinkUdostepniania() {
    const params = new URLSearchParams();
    const kod = kodujKanapkeUrl(stan.kanapka);
    if (kod) params.set("kanapka", kod);
    return window.location.origin + window.location.pathname + (params.toString() ? "?" + params.toString() : "");
  }

  function zapiszKanapkeLocalStorage() {
    try {
      if (stan.kanapka.length === 0 && !stan.nazwaKanapki.trim()) {
        global.localStorage.removeItem(KLUCZ_LOCAL_STORAGE);
        return;
      }
      const payload = stan.kanapka.map(function (item) {
        return { zbior: item.zbior, warstwa: item.warstwa, krycie: item.krycie, tryb: item.tryb };
      });
      global.localStorage.setItem(KLUCZ_LOCAL_STORAGE, JSON.stringify({
        nazwa: stan.nazwaKanapki,
        warstwy: payload
      }));
    } catch (e) {
      // Brak dostępu do localStorage ignorujemy po cichu
    }
  }

  function wczytajKanapkePoczatkowa() {
    if (stan.kanapkaUrlStart) {
      const zUrl = dekodujKanapkeUrl(stan.kanapkaUrlStart, stan.warstwy);
      if (zUrl.length > 0) {
        stan.kanapka = zUrl;
        stan.mikserOtwarty = true;
        return;
      }
    }
    try {
      const raw = global.localStorage.getItem(KLUCZ_LOCAL_STORAGE);
      if (!raw) return;
      const parsed = JSON.parse(raw);
      if (!parsed || !Array.isArray(parsed.warstwy)) return;
      if (parsed.nazwa) stan.nazwaKanapki = parsed.nazwa;
      stan.kanapka = zmapujWarstwyZPrzepisu(parsed.warstwy);
    } catch (e) {
      // Ignorujemy uszkodzony zapis roboczy
    }
  }

  function zmapujWarstwyZPrzepisu(pozycje) {
    const mapa = new Map();
    stan.warstwy.forEach(function (w) {
      mapa.set((w.zbior + "|" + w.warstwa).toLowerCase(), w);
    });
    const wynik = [];
    const uzyte = new Set();
    (pozycje || []).forEach(function (p) {
      if (wynik.length >= MAKS_WARSTW_KANAPKI) return;
      const klucz = ((p.zbior || "") + "|" + (p.warstwa || "")).toLowerCase();
      if (uzyte.has(klucz)) return;
      const def = mapa.get(klucz);
      if (!def) return;
      uzyte.add(klucz);
      wynik.push(utworzPozycjeKanapki(def, p.krycie, p.tryb));
    });
    return wynik;
  }

  function indeksWKanapce(w) {
    for (let i = 0; i < stan.kanapka.length; i++) {
      if (
        stan.kanapka[i].zbior.toLowerCase() === (w.zbior || "").toLowerCase() &&
        stan.kanapka[i].warstwa === w.warstwa
      ) {
        return i;
      }
    }
    return -1;
  }

  function przelaczWarstweWKanapce(w) {
    const idx = indeksWKanapce(w);
    if (idx >= 0) {
      stan.kanapka.splice(idx, 1);
    } else {
      if (stan.kanapka.length >= MAKS_WARSTW_KANAPKI) {
        pokazStatusMiksera("Kanapka może mieć najwyżej " + MAKS_WARSTW_KANAPKI + " warstw. Usuń jedną przed dodaniem kolejnej.", true);
        return;
      }
      const poz = utworzPozycjeKanapki(w, PELNE_KRYCIE, "Zwykly");
      if (czyWarstwaCieniowania(poz)) {
        poz.tryb = "RzezbaTerenu";
        poz.krycie = 70;
      }
      if (!poz.nakladka && !czyWarstwaCieniowania(poz) && stan.kanapka.length > 0) {
        // Nowy podkład wsuwamy pod końcowe nakładki i warstwy cieniowania, tak jak w MapoTero
        let wstaw = stan.kanapka.length;
        while (wstaw > 0 && (stan.kanapka[wstaw - 1].nakladka || czyWarstwaCieniowania(stan.kanapka[wstaw - 1]))) wstaw--;
        stan.kanapka.splice(wstaw, 0, poz);
      } else {
        stan.kanapka.push(poz);
      }
    }
    poZmianieKanapki();
  }

  function poZmianieKanapki(bezPodgladu) {
    zapiszKanapkeLocalStorage();
    if (stan.kanapkaUrlStart) zapiszParametryUrl();
    odswiezZnacznikiKart();
    odswiezPrzyciskModalaKanapki();
    odswiezDokKanapki();
    if (stan.mikserOtwarty) {
      renderujListeMiksera();
      if (!bezPodgladu) zglosOdswiezeniePodgladuMiksera();
    }
  }

  function wyslijDoMapoTero(w, przycisk) {
    if (!czyPolaczonoZProgramem()) return;
    const poprzedniTekst = przycisk.textContent;
    przycisk.disabled = true;
    przycisk.textContent = "Wysyłanie…";

    const url =
      "http://127.0.0.1:" +
      stan.port +
      "/dodaj?zbior=" +
      encodeURIComponent(w.zbior) +
      "&warstwa=" +
      encodeURIComponent(w.warstwa) +
      "&token=" +
      encodeURIComponent(stan.token);

    fetch(url)
      .then(function (resp) {
        return resp.json().catch(function () {
          return { ok: resp.ok, status: resp.ok ? "dodano" : "blad", komunikat: "" };
        });
      })
      .then(function (wynik) {
        if (wynik.ok) {
          przycisk.classList.add("gw-dodano");
          przycisk.textContent = wynik.status === "juz_wybrana" ? "Już wybrana ✓" : "Dodano ✓";
          if (ref.pasekMapoTero) {
            ref.pasekMapoTero.classList.remove("gw-pasek-blad");
            ref.pasekMapoTero.textContent = wynik.komunikat || "Dodano warstwę do MapoTero.";
          }
        } else {
          przycisk.textContent = "Nie udało się";
          if (ref.pasekMapoTero) {
            ref.pasekMapoTero.classList.add("gw-pasek-blad");
            ref.pasekMapoTero.textContent = wynik.komunikat || "Program MapoTero nie mógł dodać warstwy.";
          }
        }
        setTimeout(function () {
          przycisk.disabled = false;
          przycisk.classList.remove("gw-dodano");
          przycisk.textContent = poprzedniTekst;
        }, 2000);
      })
      .catch(function () {
        przycisk.disabled = false;
        przycisk.textContent = poprzedniTekst;
        if (ref.pasekMapoTero) {
          ref.pasekMapoTero.classList.add("gw-pasek-blad");
          ref.pasekMapoTero.textContent =
            "Brak połączenia z MapoTero — sprawdź, czy program nadal jest otwarty.";
        }
      });
  }

  function pobierzStanZMapoTero(nadpiszKanapkeJesliPusta, wymusNadpisanieKanapki) {
    if (!czyPolaczonoZProgramem() || !maFunkcjeProgramu("stan")) return Promise.resolve(null);
    return fetch("http://127.0.0.1:" + stan.port + "/stan?token=" + encodeURIComponent(stan.token))
      .then(function (r) { return r.json(); })
      .then(function (dane) {
        if (!dane || !dane.ok) return null;
        if (dane.obszar) {
          stan.obszarProgramu = dane.obszar;
          if (Number.isFinite(dane.obszar.lat) && Number.isFinite(dane.obszar.lon) && (dane.obszar.lat !== 0 || dane.obszar.lon !== 0)) {
            stan.podgladLat = dane.obszar.lat;
            stan.podgladLon = dane.obszar.lon;
            stan.mapaCenterLat = dane.obszar.lat;
            stan.mapaCenterLon = dane.obszar.lon;
          }
          if (Array.isArray(dane.obszar.bboxWgs84) && dane.obszar.bboxWgs84.length === 4) {
            stan.mapaZoom = dopasujZoomDoBbox(dane.obszar.bboxWgs84, 356, 258);
          } else if (!dane.obszar.maSiatke) {
            stan.mapaZoom = 6;
          } else if (dane.obszar.piksel > 0) {
            stan.mapaZoom = dane.obszar.piksel <= 1 ? 14 : (dane.obszar.piksel <= 5 ? 12 : 10);
          }
        }
        if (dane.zestaw && Array.isArray(dane.zestaw.warstwy) && dane.zestaw.warstwy.length > 0) {
          if (wymusNadpisanieKanapki || (nadpiszKanapkeJesliPusta && stan.kanapka.length === 0)) {
            stan.kanapka = zmapujWarstwyZPrzepisu(dane.zestaw.warstwy);
            if (dane.zestaw.nazwa) stan.nazwaKanapki = dane.zestaw.nazwa;
            poZmianieKanapki(true);
          }
        }
        if (stan.mikserOtwarty) {
          rysujMinimape();
          zglosOdswiezeniePodgladuMiksera(true);
        }
        return dane;
      })
      .catch(function () { return null; });
  }

  function wyslijKanapkeDoMapoTero(cel, zapiszStyl) {
    if (!czyPolaczonoZProgramem()) return;
    if (stan.kanapka.length === 0) {
      pokazStatusMiksera("Kanapka jest pusta — dodaj przynajmniej jedną warstwę.", true);
      return;
    }

    if (!maFunkcjeProgramu("zestaw")) {
      pokazStatusMiksera("Ta wersja MapoTero nie obsługuje jeszcze wysyłania całej kanapki. Zaktualizuj program lub pobierz plik .mapotero.", true);
      return;
    }

    const przepis = budujPrzepisJson(stan.kanapka, {
      nazwa: stan.nazwaKanapki,
      cel: cel || "okno_glowne",
      zapiszStyl: Boolean(zapiszStyl),
      obszarWgs84: stan.przekazObszarDoMapoTero && stan.zaznaczonyBbox ? stan.zaznaczonyBbox : null
    });

    pokazStatusMiksera("Wysyłanie kanapki do programu MapoTero…", false);
    fetch("http://127.0.0.1:" + stan.port + "/zestaw?token=" + encodeURIComponent(stan.token), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(przepis)
    })
      .then(function (r) {
        return r.json().catch(function () { return { ok: r.ok, komunikat: "" }; });
      })
      .then(function (wynik) {
        if (wynik && wynik.ok) {
          pokazStatusMiksera(wynik.komunikat || "Przekazano kanapkę do MapoTero!", false);
          if (ref.pasekMapoTero) {
            ref.pasekMapoTero.classList.remove("gw-pasek-blad");
            ref.pasekMapoTero.textContent = wynik.komunikat || "Przekazano kanapkę do MapoTero.";
          }
        } else {
          pokazStatusMiksera((wynik && wynik.komunikat) || "Program nie mógł ustawić kanapki.", true);
        }
      })
      .catch(function () {
        pokazStatusMiksera("Brak połączenia z MapoTero — sprawdź, czy program nadal jest otwarty.", true);
      });
  }

  function utworzObrazLubPlaceholder(w, opis) {
    const ramka = el("div", "gw-miniatura-ramka" + (w.nakladka ? " gw-nakladka-tlo" : ""));
    const odznaki = el("div", "gw-odznaki");
    odznaki.appendChild(el("span", klasaTypu(w.typ), w.typ || "WMS"));
    if (w.nakladka) {
      odznaki.appendChild(el("span", "gw-odznaka gw-odznaka-nakladka", "Nakładka"));
    }
    if (w.zasieg && w.zasieg.typ && w.zasieg.typ !== "kraj") {
      let tekstOdznaki = "";
      let klasaOdznaki = "gw-odznaka gw-odznaka-zasieg";
      if (w.zasieg.typ === "regionalny") {
        tekstOdznaki = w.zasieg.procent_pl ? "Region " + w.zasieg.procent_pl + "%" : "Region";
        klasaOdznaki += " gw-zasieg-region";
      } else if (w.zasieg.typ === "czesciowy") {
        tekstOdznaki = w.zasieg.procent_pl ? w.zasieg.procent_pl + "%" : "Część";
        klasaOdznaki += " gw-zasieg-czesciowy";
      } else if (w.zasieg.typ === "zagranica") {
        const k = w.zasieg.kod;
        tekstOdznaki = k === "DE-BB" ? "Brandenburgia" : (k === "DE-SN" ? "Saksonia" : (k === "DE" ? "Niemcy" : (k === "CZ" ? "Czechy" : (k === "SK" ? "Słowacja" : (k === "LT" ? "Litwa" : "Zagranica")))));
        klasaOdznaki += " gw-zasieg-kraj";
      } else if (w.zasieg.typ === "morski") {
        tekstOdznaki = "Bałtyk";
        klasaOdznaki += " gw-zasieg-morze";
      } else if (w.zasieg.typ === "globalny") {
        tekstOdznaki = "Świat";
        klasaOdznaki += " gw-zasieg-swiat";
      }
      if (tekstOdznaki) {
        const odznakaZasieg = el("span", klasaOdznaki, tekstOdznaki);
        odznakaZasieg.title = "Zasięg: " + (w.zasieg.etykieta || tekstOdznaki);
        odznaki.appendChild(odznakaZasieg);
      }
    }
    ramka.appendChild(odznaki);

    const brak = utworzPlaceholder(w.zbior);
    brak.hidden = true;

    if (w.obrazek) {
      const img = document.createElement("img");
      img.src = sciezkaObrazka(w.obrazek);
      img.alt = opis;
      img.loading = "lazy";
      img.addEventListener("error", function () {
        img.hidden = true;
        brak.hidden = false;
      });
      ramka.appendChild(img);
    } else {
      brak.hidden = false;
    }
    ramka.appendChild(brak);
    return ramka;
  }

  function filtrujWarstwy() {
    const fraza = normalizuj(stan.szukaj.trim());
    stan.widoczne = stan.warstwy.filter(function (w) {
      if (stan.grupa !== "WSZYSTKIE" && w.grupa !== stan.grupa) return false;
      if (stan.typ !== "WSZYSTKIE" && (w.typ || "").toUpperCase() !== stan.typ) return false;
      if (stan.rola === "PODKLAD" && w.nakladka) return false;
      if (stan.rola === "NAKLADKA" && !w.nakladka) return false;
      if (fraza) {
        const czesci = [w.nazwa, w.zbior, w.warstwa, w.identyfikator, w.grupa, w.podgrupa, w.zrodlo];
        if (w.zasieg) {
          if (w.zasieg.etykieta) czesci.push(w.zasieg.etykieta);
          if (w.zasieg.kod) czesci.push(w.zasieg.kod);
          if (w.zasieg.typ) czesci.push(w.zasieg.typ);
        }
        const tekst = normalizuj(czesci.join(" "));
        if (!tekst.includes(fraza)) return false;
      }
      return true;
    });
  }

  function czyAktywneFiltry() {
    return (
      stan.szukaj.trim() !== "" ||
      stan.grupa !== "WSZYSTKIE" ||
      stan.typ !== "WSZYSTKIE" ||
      stan.rola !== "WSZYSTKIE"
    );
  }

  function odswiezZnacznikiKart() {
    if (!ref.wyniki) return;
    const karty = ref.wyniki.querySelectorAll(".gw-karta");
    karty.forEach(function (karta) {
      const zbior = karta.getAttribute("data-zbior") || "";
      const warstwa = karta.getAttribute("data-warstwa") || "";
      const idx = indeksWKanapce({ zbior: zbior, warstwa: warstwa });
      const btn = karta.querySelector(".gw-btn-kanapka");
      const btnWyslij = karta.querySelector(".gw-btn-dodaj");
      if (idx >= 0) {
        karta.classList.add("gw-karta-w-kanapce");
        if (btn) {
          btn.classList.add("aktywny");
          btn.textContent = "✓ W miksie (#" + (idx + 1) + ")";
          btn.title = "Usuń tę warstwę z kompozycji w Mikserze warstw";
        }
        if (btnWyslij) btnWyslij.hidden = true;
      } else {
        karta.classList.remove("gw-karta-w-kanapce");
        if (btn) {
          btn.classList.remove("aktywny");
          btn.textContent = "☐ Dodaj do miksu";
          btn.title = "Dodaj warstwę do kompozycji w Mikserze warstw (ułożysz tam kolejność, krycie i tryb nakładania)";
        }
        if (btnWyslij) btnWyslij.hidden = false;
      }
    });
  }

  function utworzKartySiatki(elementy) {
    const siatka = el("div", "gw-siatka" + (stan.rozmiar === "DUZE" ? " gw-siatka-duza" : ""));
    elementy.forEach(function (pozycja) {
      const w = pozycja.warstwa;
      const idxWKanapce = indeksWKanapce(w);
      const karta = el("div", "gw-karta" + (idxWKanapce >= 0 ? " gw-karta-w-kanapce" : ""));
      karta.setAttribute("role", "button");
      karta.setAttribute("data-zbior", w.zbior);
      karta.setAttribute("data-warstwa", w.warstwa);
      karta.tabIndex = 0;
      karta.addEventListener("click", function () {
        otworzModal(pozycja.indeks);
      });
      karta.addEventListener("keydown", function (e) {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          otworzModal(pozycja.indeks);
        }
      });

      karta.appendChild(utworzObrazLubPlaceholder(w, "Podgląd warstwy: " + w.nazwa));

      const tresc = el("div", "gw-tresc-karty");
      tresc.appendChild(el("div", "gw-nazwa-warstwy", w.nazwa));
      const podtytul = [w.zbior, formatujPiksel(w.zalecany_piksel)].join(" · ");
      tresc.appendChild(el("div", "gw-meta-karty", podtytul));

      const akcje = el("div", "gw-akcje-karty");
      const btnKanapka = el(
        "button",
        "gw-btn-kanapka" + (idxWKanapce >= 0 ? " aktywny" : ""),
        idxWKanapce >= 0 ? "✓ W miksie (#" + (idxWKanapce + 1) + ")" : "☐ Dodaj do miksu"
      );
      btnKanapka.type = "button";
      btnKanapka.title = idxWKanapce >= 0
        ? "Usuń tę warstwę z kompozycji w Mikserze warstw"
        : "Dodaj warstwę do kompozycji w Mikserze warstw (ułożysz tam kolejność, krycie i tryb nakładania)";
      btnKanapka.addEventListener("click", function (e) {
        e.stopPropagation();
        przelaczWarstweWKanapce(w);
      });
      akcje.appendChild(btnKanapka);

      if (czyPolaczonoZProgramem()) {
        const btnDodaj = el("button", "gw-btn-dodaj", "Wyślij");
        btnDodaj.type = "button";
        btnDodaj.hidden = idxWKanapce >= 0;
        btnDodaj.title = "Pomiń mikser i dodaj od razu tylko tę pojedynczą warstwę do MapoTero";
        btnDodaj.addEventListener("click", function (e) {
          e.stopPropagation();
          wyslijDoMapoTero(w, btnDodaj);
        });
        akcje.appendChild(btnDodaj);
      }

      tresc.appendChild(akcje);
      karta.appendChild(tresc);
      siatka.appendChild(karta);
    });
    return siatka;
  }

  function renderujSiatke() {
    filtrujWarstwy();
    zapiszParametryUrl();

    ref.licznik.textContent =
      "Pokazano " + stan.widoczne.length + " z " + stan.warstwy.length + " warstw";
    ref.btnReset.hidden = !czyAktywneFiltry();

    ref.wyniki.innerHTML = "";
    if (stan.widoczne.length === 0) {
      ref.wyniki.appendChild(
        el("div", "gw-brak-wynikow", "Brak warstw pasujących do wybranych filtrów.")
      );
      return;
    }

    const grupy = new Map();
    stan.widoczne.forEach(function (w, idx) {
      const klucz = w.grupa || "Pozostałe";
      if (!grupy.has(klucz)) grupy.set(klucz, []);
      grupy.get(klucz).push({ warstwa: w, indeks: idx });
    });

    grupy.forEach(function (elementy, nazwaGrupy) {
      const sekcja = el("section", "gw-sekcja-grupy");
      const naglowek = el("div", "gw-naglowek-grupy");
      naglowek.appendChild(el("h2", null, nazwaGrupy));
      naglowek.appendChild(el("span", "gw-liczba-w-grupie", odmienWarstwy(elementy.length)));
      sekcja.appendChild(naglowek);

      const bloki = zgrupujPodgrupyWKolejnosci(elementy);
      bloki.forEach(function (blok) {
        if (blok.nazwa) {
          sekcja.appendChild(el("h3", "gw-podgrupa-tytul", blok.nazwa));
        }
        sekcja.appendChild(utworzKartySiatki(blok.elementy));
      });

      ref.wyniki.appendChild(sekcja);
    });
  }

  const MAPA_GEOM = {"scale":30.9524,"offX":42.46,"offY":10,"cosLat0":0.61291,"lonMin":11,"latMax":56.4,"paths":{"PL":"M 281.6,161.1 L 283.0,164.9 L 282.7,167.5 L 286.5,173.2 L 288.8,179.0 L 290.9,181.1 L 291.1,181.9 L 288.7,183.8 L 290.0,185.7 L 290.9,189.0 L 290.8,191.7 L 288.6,195.4 L 283.6,196.4 L 270.8,211.2 L 264.5,220.3 L 263.5,222.4 L 265.0,229.9 L 264.5,233.7 L 267.2,236.5 L 266.5,238.4 L 262.1,236.7 L 260.1,236.5 L 251.5,232.6 L 250.5,229.8 L 249.1,228.4 L 244.3,226.3 L 238.8,225.8 L 233.7,226.1 L 232.2,228.5 L 229.7,229.3 L 224.9,226.9 L 220.1,227.1 L 216.3,229.3 L 214.3,233.4 L 211.6,232.2 L 209.4,233.1 L 208.6,232.7 L 209.2,230.7 L 208.9,227.5 L 206.2,226.5 L 202.6,220.5 L 199.0,223.2 L 197.1,226.7 L 193.6,226.8 L 193.1,223.6 L 191.0,223.2 L 190.6,220.1 L 186.5,215.6 L 185.9,211.8 L 181.9,210.3 L 181.0,210.8 L 175.8,207.0 L 174.9,208.1 L 172.9,209.0 L 172.1,208.6 L 169.2,205.0 L 168.2,204.5 L 167.5,203.2 L 169.7,202.1 L 170.2,201.0 L 169.6,198.6 L 166.8,199.9 L 164.2,200.2 L 159.2,196.4 L 154.0,194.9 L 156.1,200.8 L 153.3,202.3 L 150.2,205.1 L 149.4,204.9 L 143.7,196.7 L 141.7,195.9 L 141.3,195.0 L 142.7,193.1 L 144.1,192.6 L 145.3,190.3 L 142.7,187.8 L 137.5,189.2 L 136.3,187.3 L 132.2,185.2 L 125.1,183.0 L 123.6,180.8 L 123.2,178.4 L 118.2,176.7 L 118.0,180.7 L 114.7,181.5 L 117.7,174.2 L 118.7,169.3 L 116.6,162.8 L 113.1,160.9 L 113.1,156.7 L 110.8,151.4 L 112.2,149.1 L 113.6,143.7 L 112.7,142.8 L 112.3,138.5 L 111.1,137.6 L 109.9,135.1 L 111.1,129.8 L 109.1,126.2 L 104.2,122.0 L 101.8,119.0 L 102.0,117.3 L 104.9,114.4 L 106.4,112.0 L 107.2,109.1 L 107.2,106.5 L 104.3,92.7 L 110.4,95.4 L 109.8,93.3 L 110.0,89.8 L 103.4,88.3 L 103.4,85.8 L 106.7,86.6 L 113.0,83.7 L 123.8,80.0 L 140.9,75.3 L 147.9,67.1 L 154.1,65.8 L 161.3,61.7 L 176.9,58.4 L 181.4,58.3 L 189.7,63.1 L 190.4,64.7 L 184.8,61.3 L 183.5,61.2 L 188.0,70.9 L 191.1,72.8 L 193.8,73.5 L 202.0,72.3 L 205.7,70.1 L 217.1,71.3 L 254.3,73.1 L 265.0,73.4 L 266.8,72.0 L 268.1,72.2 L 270.4,73.5 L 270.9,74.9 L 275.5,76.8 L 278.7,79.8 L 279.2,85.6 L 281.5,96.7 L 285.1,106.9 L 286.9,114.4 L 287.2,124.4 L 286.1,125.6 L 277.9,130.2 L 273.4,137.3 L 273.8,138.2 L 281.5,143.0 L 282.5,144.9 L 281.6,149.9 L 282.0,152.1 L 280.4,155.2 L 280.4,158.0 L 281.6,161.1 Z","BALTYK":"M 25.0,0.0 L 340.0,0.0 L 340.0,102.0 L 25.0,102.0 Z","CZ":"M 191.0,223.2 L 186.6,223.8 L 183.1,226.9 L 182.2,228.6 L 178.3,231.1 L 176.9,237.0 L 173.6,239.1 L 170.7,242.5 L 168.2,243.9 L 165.4,244.4 L 159.9,243.4 L 158.9,244.0 L 157.5,245.8 L 155.4,251.5 L 154.1,248.2 L 150.8,247.3 L 147.6,245.4 L 146.4,245.2 L 144.3,247.1 L 138.4,246.6 L 134.0,243.2 L 131.6,243.4 L 124.2,239.9 L 121.4,240.7 L 119.6,239.1 L 118.2,239.0 L 116.9,246.1 L 115.0,246.0 L 114.3,246.9 L 112.8,249.2 L 112.5,251.5 L 108.6,250.6 L 106.3,252.2 L 103.0,252.1 L 100.3,251.4 L 99.2,248.6 L 95.9,246.3 L 90.8,240.3 L 88.8,240.4 L 85.9,236.0 L 83.1,234.1 L 78.8,228.8 L 76.9,228.9 L 74.4,226.2 L 68.8,216.2 L 71.1,211.3 L 70.1,209.5 L 65.4,205.1 L 63.1,198.8 L 64.0,198.5 L 64.7,199.2 L 66.7,202.5 L 70.0,197.3 L 71.8,195.9 L 76.0,194.8 L 79.3,195.5 L 80.7,192.9 L 83.8,192.3 L 84.9,190.2 L 87.5,188.9 L 88.7,189.5 L 91.0,186.3 L 97.4,184.5 L 106.4,180.3 L 104.2,177.1 L 104.7,176.2 L 105.4,176.0 L 109.7,177.3 L 110.7,179.7 L 111.2,179.8 L 111.0,181.6 L 113.1,182.9 L 113.9,182.8 L 114.7,181.5 L 118.0,180.7 L 118.2,176.7 L 123.2,178.4 L 123.6,180.8 L 125.1,183.0 L 132.2,185.2 L 136.3,187.3 L 137.5,189.2 L 142.7,187.8 L 144.1,188.9 L 145.3,190.3 L 144.5,192.1 L 142.7,193.1 L 141.3,195.0 L 141.7,195.9 L 143.7,196.7 L 149.4,204.9 L 150.2,205.1 L 153.3,202.3 L 156.1,200.8 L 154.0,194.9 L 159.2,196.4 L 164.2,200.2 L 166.8,199.9 L 169.6,198.6 L 170.2,201.0 L 169.7,202.1 L 167.5,203.2 L 168.2,204.5 L 169.2,205.0 L 172.1,208.6 L 172.9,209.0 L 174.9,208.1 L 175.8,207.0 L 181.0,210.8 L 181.9,210.3 L 185.9,211.8 L 186.5,215.6 L 190.6,220.1 L 191.0,223.2 Z","SK":"M 261.4,236.8 L 258.5,243.0 L 256.7,248.8 L 253.9,252.4 L 253.3,257.8 L 246.7,259.5 L 245.9,259.3 L 242.9,254.7 L 240.7,252.9 L 233.4,254.3 L 229.6,253.1 L 225.4,253.0 L 222.5,253.7 L 219.5,260.9 L 211.3,265.9 L 206.1,263.1 L 204.9,263.4 L 203.1,266.6 L 199.3,267.7 L 192.6,268.4 L 190.3,270.0 L 189.5,271.9 L 189.8,275.0 L 167.3,275.0 L 162.3,270.3 L 159.1,269.8 L 157.9,268.8 L 153.7,258.0 L 153.7,256.3 L 155.2,252.9 L 156.0,249.0 L 158.9,244.0 L 159.9,243.4 L 165.4,244.4 L 168.2,243.9 L 170.7,242.5 L 173.6,239.1 L 176.9,237.0 L 178.3,231.1 L 182.2,228.6 L 183.1,226.9 L 186.6,223.8 L 191.0,223.2 L 193.1,223.6 L 193.6,226.8 L 197.1,226.7 L 199.0,223.2 L 202.6,220.5 L 206.2,226.5 L 208.9,227.5 L 209.2,230.7 L 208.6,232.7 L 209.4,233.1 L 211.6,232.2 L 214.3,233.4 L 216.3,229.3 L 220.1,227.1 L 224.9,226.9 L 227.0,227.6 L 228.4,228.9 L 231.2,229.2 L 233.7,226.1 L 238.8,225.8 L 244.3,226.3 L 249.1,228.4 L 250.5,229.8 L 251.5,232.6 L 261.4,236.8 Z","DE_REST":"M 25.0,275.0 L 25.0,69.3 L 26.2,69.2 L 27.5,71.6 L 30.3,70.7 L 37.4,74.5 L 42.7,72.6 L 43.7,75.6 L 42.6,78.7 L 38.9,82.0 L 39.7,84.0 L 40.9,84.4 L 44.4,84.0 L 50.0,86.0 L 57.6,79.8 L 63.5,79.1 L 64.6,77.3 L 72.3,69.8 L 80.9,71.6 L 83.2,75.5 L 88.9,79.9 L 94.1,79.5 L 96.0,83.7 L 96.8,88.8 L 99.8,91.5 L 104.3,92.7 L 107.2,106.5 L 107.2,109.1 L 106.4,112.0 L 104.9,114.4 L 102.0,117.3 L 101.8,119.0 L 104.2,122.0 L 109.1,126.2 L 111.1,129.8 L 109.9,135.1 L 111.1,137.6 L 112.3,138.5 L 112.7,142.8 L 113.6,143.7 L 112.2,149.1 L 110.8,151.4 L 113.1,156.7 L 113.1,160.9 L 116.6,162.8 L 118.7,169.3 L 116.8,176.9 L 114.5,182.0 L 113.1,182.9 L 111.0,181.6 L 111.2,179.8 L 110.7,179.7 L 109.7,177.3 L 105.4,176.0 L 104.7,176.2 L 104.2,177.1 L 106.4,180.3 L 97.4,184.5 L 91.0,186.3 L 88.7,189.5 L 87.5,188.9 L 84.9,190.2 L 83.8,192.3 L 80.7,192.9 L 79.3,195.5 L 76.0,194.8 L 71.8,195.9 L 70.0,197.3 L 66.7,202.5 L 64.7,199.2 L 64.0,198.5 L 63.1,198.8 L 65.4,205.1 L 70.1,209.5 L 71.1,211.3 L 68.8,216.2 L 74.4,226.2 L 76.9,228.9 L 78.8,228.8 L 83.1,234.1 L 85.9,236.0 L 88.0,239.7 L 88.8,240.4 L 90.8,240.3 L 95.9,246.3 L 95.3,251.8 L 94.1,253.2 L 93.2,253.8 L 89.6,252.0 L 87.5,258.8 L 78.5,263.7 L 75.9,266.7 L 75.9,267.6 L 79.5,273.4 L 79.5,275.0 Z M 93.9,72.4 L 94.3,74.5 L 93.8,75.6 L 91.7,73.8 L 89.5,73.8 L 88.3,76.6 L 87.3,76.7 L 84.0,74.2 L 83.4,72.0 L 83.8,67.4 L 84.8,66.2 L 85.0,64.5 L 86.8,62.7 L 88.4,62.6 L 89.7,65.2 L 92.9,67.0 L 93.1,67.7 L 91.4,69.9 L 91.8,71.1 L 93.9,72.4 Z M 103.4,85.8 L 103.4,88.3 L 98.0,88.0 L 97.5,86.2 L 97.9,84.4 L 96.1,82.4 L 96.1,80.3 L 103.4,85.8 Z M 47.8,71.3 L 44.9,71.4 L 42.7,69.9 L 43.3,68.3 L 44.1,67.8 L 46.9,68.8 L 47.8,71.3 Z","DE_BB":"M 97.1,99.7 L 97.8,102.2 L 99.4,101.8 L 100.4,102.5 L 101.7,101.6 L 103.9,102.0 L 103.8,103.8 L 101.3,107.1 L 104.3,107.2 L 105.4,105.6 L 107.0,104.9 L 107.8,106.7 L 106.3,109.7 L 106.7,111.0 L 106.2,113.2 L 102.1,116.4 L 102.3,119.0 L 101.7,120.2 L 105.8,122.7 L 108.2,125.8 L 111.5,128.2 L 110.8,129.7 L 111.3,130.7 L 109.7,132.6 L 109.4,133.9 L 110.5,136.6 L 110.2,137.2 L 112.6,138.7 L 112.1,142.4 L 113.4,143.9 L 112.3,149.1 L 110.5,151.2 L 110.7,152.2 L 111.7,152.5 L 111.8,154.5 L 113.1,155.8 L 113.5,157.9 L 112.9,159.0 L 112.4,158.6 L 112.0,160.1 L 111.0,160.3 L 110.2,159.4 L 109.4,160.2 L 106.9,160.5 L 105.9,161.6 L 104.8,160.7 L 102.3,160.3 L 102.4,160.9 L 101.1,161.8 L 101.4,162.3 L 100.3,162.3 L 100.9,163.3 L 99.7,165.6 L 98.8,164.8 L 95.2,165.9 L 92.2,165.7 L 92.0,165.1 L 91.0,165.7 L 90.6,164.6 L 88.4,163.1 L 87.9,164.0 L 86.8,163.5 L 86.2,164.8 L 85.6,164.7 L 85.8,165.2 L 84.8,164.8 L 84.6,163.8 L 84.1,163.9 L 84.7,160.9 L 84.3,159.9 L 83.5,159.8 L 83.6,158.6 L 83.0,158.0 L 82.3,158.4 L 81.6,157.1 L 84.2,155.0 L 83.0,150.9 L 83.5,150.1 L 81.3,150.2 L 81.5,149.3 L 80.1,149.3 L 79.9,148.2 L 77.8,148.3 L 77.7,147.3 L 76.6,147.5 L 74.4,145.9 L 73.4,146.8 L 71.9,146.8 L 70.5,145.2 L 69.7,145.7 L 66.8,143.0 L 65.6,141.1 L 67.2,139.4 L 66.2,138.4 L 67.5,135.6 L 67.0,135.1 L 67.4,133.8 L 66.8,133.4 L 67.9,131.5 L 66.7,131.2 L 66.0,130.1 L 65.3,131.0 L 64.8,130.6 L 65.1,129.8 L 64.3,130.0 L 65.0,128.6 L 64.8,127.0 L 66.1,126.9 L 66.0,125.0 L 65.3,124.2 L 65.6,122.4 L 66.4,121.7 L 66.0,119.7 L 65.2,119.2 L 63.9,119.9 L 63.9,118.8 L 63.6,119.2 L 62.0,118.8 L 61.1,119.2 L 58.5,118.2 L 58.7,116.9 L 56.7,115.7 L 55.6,116.0 L 55.5,115.1 L 54.5,115.0 L 54.6,114.0 L 52.4,113.9 L 51.2,112.8 L 49.1,113.6 L 47.6,111.5 L 50.0,111.9 L 50.0,111.0 L 52.7,111.5 L 53.4,110.2 L 53.2,108.7 L 54.5,107.8 L 55.8,107.8 L 56.4,108.6 L 58.3,108.3 L 57.7,107.5 L 59.0,107.7 L 59.5,106.7 L 60.5,107.0 L 62.0,106.0 L 62.5,103.9 L 63.6,104.7 L 64.5,104.2 L 64.7,104.9 L 65.9,104.3 L 66.5,105.4 L 69.0,106.0 L 69.9,107.6 L 74.2,107.5 L 74.3,108.3 L 75.9,108.4 L 75.5,109.2 L 76.0,109.6 L 77.6,109.2 L 78.2,109.9 L 80.0,109.2 L 79.4,110.0 L 80.1,110.4 L 82.4,108.7 L 82.7,107.9 L 83.9,107.6 L 84.8,108.6 L 85.2,107.4 L 86.1,106.7 L 87.3,106.7 L 88.1,107.7 L 88.7,106.1 L 89.5,106.4 L 89.9,105.2 L 90.4,105.5 L 90.8,102.9 L 92.2,102.6 L 92.5,101.5 L 93.9,100.4 L 95.1,100.7 L 96.1,99.9 L 95.2,99.4 L 95.4,98.0 L 95.9,99.3 L 97.1,99.7 Z","DE_SN":"M 78.1,156.3 L 78.2,156.9 L 78.7,156.6 L 78.6,157.1 L 79.5,157.4 L 80.7,156.5 L 82.3,158.4 L 83.0,158.0 L 83.6,158.6 L 83.5,159.8 L 84.3,159.9 L 84.7,160.9 L 84.1,163.9 L 84.6,163.8 L 84.8,164.8 L 85.8,165.2 L 85.6,164.7 L 86.2,164.8 L 86.8,163.5 L 87.9,164.0 L 88.4,163.1 L 90.6,164.6 L 91.0,165.7 L 92.0,165.1 L 92.2,165.7 L 95.2,165.9 L 98.8,164.8 L 99.7,165.6 L 100.9,163.3 L 100.3,162.3 L 101.4,162.3 L 101.1,161.8 L 102.4,160.9 L 102.3,160.3 L 104.8,160.7 L 105.9,161.6 L 106.9,160.5 L 109.4,160.2 L 110.2,159.4 L 111.0,160.3 L 112.0,160.1 L 112.5,158.6 L 113.4,160.9 L 116.6,161.9 L 117.7,163.1 L 117.7,166.2 L 118.9,167.6 L 119.1,169.7 L 118.4,170.6 L 117.9,174.7 L 116.5,178.6 L 114.9,180.5 L 114.6,182.4 L 112.9,182.5 L 112.9,182.1 L 111.0,181.6 L 111.7,179.2 L 110.0,179.5 L 110.7,177.5 L 108.9,176.6 L 108.9,175.5 L 108.7,176.5 L 107.9,176.2 L 106.9,176.9 L 106.1,176.1 L 105.4,176.4 L 105.0,175.5 L 104.1,178.0 L 107.0,178.9 L 106.4,180.6 L 104.2,180.7 L 103.4,181.9 L 100.5,183.3 L 98.5,183.2 L 98.4,183.8 L 97.4,183.8 L 97.5,185.1 L 96.5,185.9 L 93.5,185.9 L 93.1,185.4 L 90.8,186.0 L 90.4,188.4 L 89.2,189.6 L 87.5,188.2 L 86.6,190.1 L 85.1,189.8 L 84.1,192.6 L 81.0,192.4 L 80.9,194.2 L 80.0,195.3 L 79.2,195.5 L 77.2,193.9 L 76.8,194.7 L 75.9,194.5 L 74.8,195.8 L 73.1,195.1 L 71.2,195.7 L 70.8,197.3 L 69.1,198.2 L 69.2,198.9 L 68.3,199.5 L 67.6,202.5 L 66.8,202.4 L 66.8,201.1 L 65.9,200.3 L 66.2,199.6 L 65.1,199.5 L 65.1,198.1 L 63.7,198.4 L 61.0,197.1 L 61.1,195.9 L 59.4,194.3 L 60.2,193.6 L 60.6,194.0 L 60.9,192.9 L 60.1,191.7 L 59.3,192.0 L 59.1,191.2 L 60.2,190.7 L 60.2,189.8 L 61.8,188.3 L 62.2,190.8 L 63.0,190.7 L 63.1,190.0 L 64.6,190.1 L 64.4,188.5 L 65.3,188.9 L 65.4,188.1 L 66.8,188.3 L 67.8,186.8 L 66.9,186.6 L 66.5,185.7 L 66.9,185.3 L 66.0,185.0 L 66.3,184.1 L 67.2,183.9 L 66.4,182.6 L 68.3,182.4 L 68.8,181.5 L 70.0,181.9 L 70.8,180.3 L 73.8,180.1 L 74.1,179.4 L 73.2,177.1 L 72.7,177.5 L 71.6,177.0 L 71.9,176.3 L 71.3,176.5 L 71.2,175.1 L 70.6,175.1 L 70.3,174.4 L 69.4,174.8 L 68.1,173.9 L 67.1,174.2 L 65.8,173.6 L 66.2,173.1 L 65.4,172.7 L 65.6,171.5 L 65.0,171.4 L 65.5,170.4 L 64.4,167.2 L 65.2,166.9 L 65.3,164.0 L 64.4,162.8 L 65.4,162.1 L 65.2,161.0 L 66.1,159.6 L 69.7,159.1 L 70.0,158.3 L 70.8,158.8 L 72.7,157.8 L 73.7,158.1 L 74.7,156.7 L 76.4,157.2 L 77.4,156.1 L 78.1,156.3 Z","LT":"M 231.4,44.7 L 230.3,44.5 L 232.4,40.9 L 234.3,34.3 L 234.1,38.2 L 232.8,42.5 L 231.4,44.7 Z M 265.7,73.2 L 264.0,69.0 L 264.1,66.9 L 266.8,57.3 L 263.0,54.2 L 261.9,51.5 L 252.5,51.4 L 242.7,47.3 L 239.6,44.8 L 236.6,45.2 L 236.0,42.7 L 236.7,39.2 L 233.3,28.2 L 233.0,20.2 L 244.6,12.6 L 252.7,9.8 L 262.3,10.8 L 267.8,10.1 L 269.5,10.6 L 270.9,12.4 L 272.4,12.1 L 273.8,11.0 L 285.5,12.2 L 291.4,14.2 L 298.1,13.6 L 302.4,10.6 L 305.0,9.7 L 306.2,10.1 L 309.4,16.2 L 320.6,19.1 L 328.6,25.6 L 332.4,30.1 L 334.6,31.5 L 338.3,32.7 L 337.8,36.4 L 335.7,42.7 L 336.4,43.5 L 340.0,43.9 L 340.0,46.3 L 338.4,49.3 L 332.6,49.0 L 331.8,49.5 L 330.3,53.2 L 328.8,54.5 L 324.4,55.8 L 322.9,58.5 L 321.8,62.1 L 321.8,66.8 L 319.8,70.0 L 318.4,74.0 L 318.6,74.7 L 321.4,75.2 L 322.2,76.2 L 322.3,79.4 L 318.9,80.0 L 317.7,79.3 L 318.1,77.6 L 316.8,75.2 L 315.1,76.5 L 313.4,76.5 L 311.5,77.7 L 308.9,80.2 L 305.6,79.8 L 304.7,80.6 L 303.7,85.1 L 300.9,84.9 L 295.1,87.6 L 292.7,85.8 L 284.0,87.0 L 280.7,86.8 L 279.3,86.1 L 278.7,79.8 L 277.1,78.1 L 273.3,75.6 L 270.9,74.9 L 270.4,73.5 L 268.1,72.2 L 266.8,72.0 L 265.7,73.2 Z","BY":"M 340.0,152.9 L 335.6,152.0 L 334.5,151.0 L 323.0,148.5 L 313.1,148.1 L 298.5,149.8 L 295.9,150.3 L 294.4,153.2 L 288.7,158.8 L 285.1,157.4 L 282.4,157.7 L 281.7,158.2 L 281.6,161.1 L 280.4,158.0 L 280.4,155.2 L 282.0,152.1 L 281.6,149.9 L 282.5,144.9 L 281.5,143.0 L 273.8,138.2 L 273.4,137.3 L 277.9,130.2 L 286.1,125.6 L 287.2,124.4 L 287.5,122.3 L 286.4,111.8 L 281.5,96.7 L 279.3,86.1 L 284.0,87.0 L 292.7,85.8 L 295.1,87.6 L 300.9,84.9 L 303.7,85.1 L 304.7,80.6 L 305.6,79.8 L 308.9,80.2 L 311.5,77.7 L 313.4,76.5 L 315.1,76.5 L 316.8,75.2 L 318.1,77.6 L 317.7,79.3 L 318.9,80.0 L 322.3,79.4 L 322.2,76.2 L 321.4,75.2 L 318.6,74.7 L 318.4,74.0 L 319.8,70.0 L 321.8,66.8 L 321.8,62.1 L 322.9,58.5 L 324.4,55.8 L 328.8,54.5 L 330.3,53.2 L 331.8,49.5 L 332.6,49.0 L 338.4,49.3 L 340.0,46.3 L 340.0,43.9 L 336.4,43.5 L 335.7,42.7 L 337.8,36.4 L 338.3,32.7 L 340.0,32.1 Z","UA":"M 340.0,260.3 L 338.8,262.0 L 332.8,263.7 L 332.3,266.5 L 330.1,270.2 L 316.9,272.8 L 312.3,275.0 L 302.6,275.0 L 300.1,272.1 L 298.3,271.6 L 292.5,272.9 L 290.2,271.7 L 289.1,272.0 L 282.8,270.2 L 277.9,270.3 L 274.0,267.4 L 272.8,267.3 L 271.2,269.8 L 267.8,271.6 L 267.4,269.1 L 265.7,266.6 L 264.0,266.8 L 262.2,265.9 L 261.0,263.6 L 257.8,262.1 L 256.0,257.4 L 253.6,257.5 L 253.9,252.4 L 256.7,248.8 L 258.5,243.0 L 261.4,236.8 L 266.5,238.4 L 267.3,237.1 L 264.5,233.7 L 265.0,229.9 L 263.5,222.4 L 264.5,220.3 L 270.8,211.2 L 283.6,196.4 L 288.6,195.4 L 290.8,191.7 L 290.9,189.0 L 290.0,185.7 L 288.7,183.8 L 291.1,181.9 L 290.9,181.1 L 288.8,179.0 L 286.5,173.2 L 282.7,167.5 L 283.0,164.9 L 281.6,161.1 L 281.7,158.2 L 283.5,157.3 L 285.1,157.4 L 288.2,159.0 L 291.5,156.6 L 295.9,150.3 L 305.5,149.3 L 309.3,148.3 L 325.6,148.9 L 340.0,152.9 Z","RU":"M 231.4,44.7 L 224.5,53.9 L 230.0,56.1 L 232.1,56.3 L 235.8,55.3 L 236.6,45.2 L 239.6,44.8 L 242.7,47.3 L 252.5,51.4 L 261.9,51.5 L 263.0,54.2 L 266.8,57.3 L 264.1,66.9 L 264.0,69.0 L 265.7,73.2 L 217.1,71.3 L 205.7,70.1 L 208.6,67.4 L 210.5,64.7 L 212.1,61.1 L 212.7,55.8 L 215.2,54.7 L 220.7,54.8 L 223.1,53.5 L 230.3,44.5 L 231.4,44.7 Z","SE":"M 147.3,13.4 L 146.4,14.9 L 145.5,14.8 L 144.9,12.8 L 145.1,4.8 L 147.2,0.0 L 151.3,0.0 L 147.3,13.4 Z M 144.1,0.0 L 141.4,4.1 L 137.3,15.5 L 135.8,17.2 L 134.0,18.5 L 130.2,16.6 L 124.5,17.7 L 119.3,17.1 L 114.2,17.4 L 112.9,18.2 L 113.7,21.4 L 111.8,21.8 L 110.0,20.9 L 107.0,23.1 L 103.5,27.6 L 103.2,30.8 L 105.9,37.0 L 102.7,41.1 L 95.7,40.1 L 86.5,42.6 L 78.2,40.6 L 79.3,38.4 L 79.9,30.2 L 70.4,13.4 L 74.8,15.1 L 76.6,14.2 L 73.9,8.7 L 77.7,8.4 L 78.9,6.4 L 78.2,3.3 L 75.0,1.9 L 74.0,0.0 Z","DK":"M 72.2,29.0 L 71.8,33.0 L 67.5,35.1 L 66.0,36.7 L 65.5,38.9 L 66.7,40.5 L 68.7,41.4 L 69.3,44.5 L 67.5,46.0 L 63.1,47.5 L 62.4,59.0 L 58.8,60.4 L 56.5,56.0 L 54.9,47.5 L 51.5,46.8 L 47.9,47.0 L 45.7,43.2 L 46.1,38.9 L 44.9,36.8 L 44.8,34.7 L 42.6,33.4 L 42.1,31.0 L 48.6,30.0 L 51.3,26.1 L 51.5,24.1 L 54.4,23.7 L 55.7,25.2 L 55.6,30.8 L 58.0,31.7 L 60.0,27.7 L 60.2,25.6 L 58.9,23.4 L 65.6,18.7 L 67.6,18.6 L 72.4,20.4 L 73.0,21.4 L 71.4,24.9 L 72.2,29.0 Z M 25.0,0.0 L 29.0,0.0 L 28.9,3.2 L 30.8,5.2 L 32.8,6.3 L 39.5,6.2 L 41.1,8.7 L 39.7,13.2 L 37.8,14.9 L 35.3,16.1 L 33.7,16.2 L 31.6,13.8 L 30.6,14.6 L 29.5,15.8 L 27.0,26.6 L 26.5,26.9 L 25.0,26.3 Z M 35.7,34.5 L 39.0,43.4 L 38.4,45.0 L 38.8,47.0 L 38.4,49.2 L 35.3,51.7 L 31.9,51.8 L 25.0,49.1 L 25.0,36.1 L 28.9,34.4 L 31.5,36.0 L 33.1,36.1 L 35.7,34.5 Z M 49.3,56.7 L 52.7,58.6 L 54.9,58.5 L 56.5,59.3 L 57.0,63.3 L 51.1,64.8 L 43.1,60.4 L 43.6,55.2 L 47.4,54.8 L 49.3,56.7 Z M 37.4,61.0 L 36.6,61.2 L 35.3,57.9 L 39.7,51.7 L 41.0,48.4 L 41.5,48.5 L 37.4,61.0 Z M 71.9,54.4 L 71.1,54.9 L 68.2,54.5 L 64.9,56.7 L 63.7,56.0 L 64.5,54.1 L 66.3,52.7 L 66.6,51.3 L 67.3,52.1 L 70.3,52.8 L 71.9,54.4 Z M 74.1,34.9 L 71.9,36.1 L 71.3,34.3 L 72.8,32.3 L 74.1,34.9 Z M 32.7,58.1 L 31.4,58.4 L 29.9,57.7 L 27.3,54.5 L 33.1,57.6 L 32.7,58.1 Z M 35.0,29.1 L 34.7,29.7 L 33.5,29.1 L 33.8,25.3 L 33.3,23.7 L 33.9,22.6 L 36.0,26.2 L 35.0,29.1 Z M 120.0,52.7 L 119.3,53.2 L 116.2,52.3 L 112.4,50.2 L 112.9,46.0 L 113.9,44.1 L 120.9,48.9 L 120.9,50.6 L 120.0,52.7 Z","WISLA":"M 190.6,210.4 L 193.0,210.2 M 191.9,219.1 L 190.2,213.7 L 189.9,211.6 L 190.6,210.4 M 193.0,210.2 L 195.5,209.6 L 198.0,206.8 L 200.8,206.7 L 205.4,208.1 L 210.4,207.9 L 223.1,203.8 L 225.4,202.3 L 227.2,200.0 L 230.0,198.6 L 233.7,198.0 L 237.7,196.2 L 241.8,193.1 L 244.9,190.0 L 247.0,186.8 L 247.8,181.9 L 247.0,171.3 L 247.4,169.7 L 249.9,166.8 L 250.4,165.2 L 249.9,163.4 L 247.9,161.6 L 247.8,160.4 L 246.1,158.9 L 242.7,157.1 L 240.7,155.3 L 240.3,153.5 L 236.7,148.7 L 237.2,147.9 L 237.1,146.4 L 236.4,144.1 L 231.7,136.8 L 229.2,134.3 L 227.1,133.3 L 223.1,133.2 L 217.4,134.1 L 212.5,133.2 L 206.4,129.2 L 198.1,126.3 L 193.5,122.7 L 189.8,117.5 L 185.5,114.1 L 180.5,112.5 L 179.0,109.9 L 181.2,106.2 L 184.0,103.4 L 187.5,101.5 L 189.3,99.2 L 190.0,94.4 L 191.1,93.1 L 191.8,91.2 L 191.8,85.9 L 191.2,83.0 L 191.4,80.3 L 192.6,78.0 L 193.8,73.5","ODRA":"M 168.0,216.2 L 172.4,218.8 L 174.2,219.3 L 177.9,216.5 L 181.1,211.6 L 181.3,208.9 L 180.0,206.4 L 179.6,203.8 L 180.0,201.3 L 178.8,198.2 L 175.9,194.4 L 174.5,191.6 L 174.1,187.8 L 173.0,185.8 L 170.7,183.9 L 164.8,180.8 L 163.1,179.4 L 161.4,176.4 L 160.0,175.0 L 156.6,173.1 L 153.8,170.1 L 151.3,169.3 L 148.2,169.8 L 146.3,169.6 L 145.7,167.4 L 146.2,166.2 L 146.1,161.1 L 145.0,158.7 L 143.2,156.9 L 141.5,156.2 L 140.1,156.4 L 134.4,154.1 L 132.9,152.0 L 133.3,149.5 L 132.7,147.4 L 131.3,145.9 L 126.4,144.8 L 113.8,143.8 L 112.7,142.8 L 112.8,139.8 L 112.3,138.5 L 110.3,136.5 L 109.9,135.1 L 111.1,129.8 L 109.1,126.2 L 104.2,122.0 L 101.8,119.0 L 102.0,117.3 L 103.0,115.8 L 104.9,114.4 L 106.4,112.0 L 107.8,106.7 L 110.4,101.5 L 111.1,99.0 L 110.4,95.4"},"miasta":[{"n":"Warszawa","x":232.4,"y":139.1},{"n":"Kraków","x":212,"y":206.2},{"n":"Gdańsk","x":187.5,"y":73.4},{"n":"Wrocław","x":157,"y":173.8},{"n":"Poznań","x":154.9,"y":133.6}],"miastaTrueOrtho":[{"n":"Warszawa","x":232.4,"y":139.1},{"n":"Kraków","x":212,"y":206.2},{"n":"Trójmiasto","x":187.5,"y":73.4},{"n":"Wrocław","x":157,"y":173.8},{"n":"Poznań","x":154.9,"y":133.6},{"n":"Łódź","x":202.9,"y":153.6},{"n":"GOP / Katowice","x":194.7,"y":199.9}]};

  function geoDoSvg(lon, lat) {
    return {
      x: MAPA_GEOM.offX + (lon - MAPA_GEOM.lonMin) * MAPA_GEOM.cosLat0 * MAPA_GEOM.scale,
      y: MAPA_GEOM.offY + (MAPA_GEOM.latMax - lat) * MAPA_GEOM.scale
    };
  }

  function dekodujMaskeHex(maskaHex) {
    if (!maskaHex || typeof maskaHex !== "string" || maskaHex.length !== 140) return null;
    const komorki = [];
    const cols = 28;
    const rows = 20;
    const dLon = 10.2 / cols;
    const dLat = 6.0 / rows;
    for (let b = 0; b < 70; b++) {
      const bajt = parseInt(maskaHex.substr(b * 2, 2), 16);
      if (isNaN(bajt)) continue;
      for (let bit = 0; bit < 8; bit++) {
        if ((bajt & (1 << (7 - bit))) !== 0) {
          const idx = b * 8 + bit;
          const r = Math.floor(idx / cols);
          const c = idx % cols;
          const lon1 = 14.0 + c * dLon;
          const lon2 = lon1 + dLon;
          const latTop = 55.0 - r * dLat;
          const latBottom = latTop - dLat;
          const p1 = geoDoSvg(lon1, latTop);
          const p2 = geoDoSvg(lon2, latBottom);
          komorki.push({
            x: Math.min(p1.x, p2.x),
            y: Math.min(p1.y, p2.y),
            w: Math.abs(p2.x - p1.x) + 0.35,
            h: Math.abs(p2.y - p1.y) + 0.35
          });
        }
      }
    }
    return komorki;
  }

  function opisZasieguDlaTabeli(w) {
    const z = w.zasieg || { typ: "kraj", etykieta: "Cała Polska" };
    if (z.typ === "kraj") return "Cała Polska (zasięg ogólnokrajowy 100%)";
    if (z.typ === "zagranica") return z.etykieta || "Zasięg zagraniczny";
    if (z.typ === "morski") return z.etykieta || "Morze Bałtyckie";
    if (z.typ === "globalny") return "Globalny (cały świat)";
    if (z.typ === "czesciowy") {
      let tekst = z.etykieta || "Zasięg częściowy";
      if (z.procent_pl) tekst += " · Pokrycie Polski: ok. " + z.procent_pl + "%";
      return tekst;
    }
    if (z.typ === "regionalny") {
      let tekst = z.etykieta || "Zasięg regionalny";
      if (z.procent_pl) tekst += " · Obszar w Polsce: ok. " + z.procent_pl + "%";
      if (z.bbox && Array.isArray(z.bbox) && z.bbox.length === 4) {
        tekst += " · BBOX: [" + z.bbox.join(", ") + "]";
      }
      return tekst;
    }
    return z.etykieta || "Cała Polska";
  }

  function utworzWidokZasieguSvg(w) {
    const z = w.zasieg || { typ: "kraj", etykieta: "Cała Polska" };
    const typ = z.typ || "kraj";
    const kod = z.kod || "";
    const komorki = z.maska_hex ? dekodujMaskeHex(z.maska_hex) : null;
    const maKomorki = komorki && komorki.length > 0;

    const wrap = el("div", "gw-widok-zasiegu-kontener");

    let klasaPL = "gw-mapa-kraj gw-mapa-polska";
    if (typ === "kraj" && !maKomorki) klasaPL += " gw-mapa-aktywny";
    else if (typ === "regionalny" || typ === "czesciowy" || maKomorki) klasaPL += " gw-mapa-baza-regionalna";

    let klasaBaltyk = "gw-mapa-akwen gw-mapa-baltyk";
    if (typ === "morski") klasaBaltyk += " gw-mapa-aktywny-morze";

    let klasaCZ = "gw-mapa-kraj gw-mapa-sasiad";
    if (typ === "zagranica" && kod === "CZ") klasaCZ += " gw-mapa-aktywny-zagranica";

    let klasaSK = "gw-mapa-kraj gw-mapa-sasiad";
    if (typ === "zagranica" && kod === "SK") klasaSK += " gw-mapa-aktywny-zagranica";

    let klasaLT = "gw-mapa-kraj gw-mapa-sasiad";
    if (typ === "zagranica" && kod === "LT") klasaLT += " gw-mapa-aktywny-zagranica";

    let klasaDE_BB = "gw-mapa-kraj gw-mapa-sasiad";
    let klasaDE_SN = "gw-mapa-kraj gw-mapa-sasiad";
    let klasaDE_REST = "gw-mapa-kraj gw-mapa-sasiad";
    if (typ === "zagranica") {
      if (kod === "DE-BB") klasaDE_BB += " gw-mapa-aktywny-zagranica";
      else if (kod === "DE-SN") klasaDE_SN += " gw-mapa-aktywny-zagranica";
      else if (kod === "DE") {
        klasaDE_BB += " gw-mapa-aktywny-zagranica";
        klasaDE_SN += " gw-mapa-aktywny-zagranica";
        klasaDE_REST += " gw-mapa-aktywny-zagranica";
      }
    }

    let svgHtml = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="25 0 315 275" class="gw-zasieg-svg" role="img" aria-label="Mapa zasięgu warstwy">';
    svgHtml += '<defs>';
    svgHtml += '<clipPath id="gw-clip-polska">';
    svgHtml += '<path d="' + MAPA_GEOM.paths.PL + '" />';
    svgHtml += '</clipPath>';
    svgHtml += '</defs>';

    svgHtml += '<path d="' + MAPA_GEOM.paths.BALTYK + '" class="' + klasaBaltyk + '" />';
    if (MAPA_GEOM.paths.SE) svgHtml += '<path d="' + MAPA_GEOM.paths.SE + '" class="gw-mapa-kraj gw-mapa-sasiad" />';
    if (MAPA_GEOM.paths.DK) svgHtml += '<path d="' + MAPA_GEOM.paths.DK + '" class="gw-mapa-kraj gw-mapa-sasiad" />';
    if (MAPA_GEOM.paths.DE_REST) svgHtml += '<path d="' + MAPA_GEOM.paths.DE_REST + '" class="' + klasaDE_REST + '" />';
    if (MAPA_GEOM.paths.DE_BB) svgHtml += '<path d="' + MAPA_GEOM.paths.DE_BB + '" class="' + klasaDE_BB + '" />';
    if (MAPA_GEOM.paths.DE_SN) svgHtml += '<path d="' + MAPA_GEOM.paths.DE_SN + '" class="' + klasaDE_SN + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.CZ + '" class="' + klasaCZ + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.SK + '" class="' + klasaSK + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.LT + '" class="' + klasaLT + '" />';
    if (MAPA_GEOM.paths.BY) svgHtml += '<path d="' + MAPA_GEOM.paths.BY + '" class="gw-mapa-kraj gw-mapa-sasiad" />';
    if (MAPA_GEOM.paths.UA) svgHtml += '<path d="' + MAPA_GEOM.paths.UA + '" class="gw-mapa-kraj gw-mapa-sasiad" />';
    if (MAPA_GEOM.paths.RU) svgHtml += '<path d="' + MAPA_GEOM.paths.RU + '" class="gw-mapa-kraj gw-mapa-sasiad" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.PL + '" class="' + klasaPL + '" />';

    if (maKomorki) {
      const uzyjClip = z.clip_polska !== false;
      svgHtml += '<g class="gw-mapa-komorki-grupa"' + (uzyjClip ? ' clip-path="url(#gw-clip-polska)"' : '') + '>';
      komorki.forEach(function (k) {
        svgHtml += '<rect x="' + k.x.toFixed(1) + '" y="' + k.y.toFixed(1) + '" width="' + k.w.toFixed(1) + '" height="' + k.h.toFixed(1) + '" class="gw-mapa-komorka" />';
      });
      svgHtml += '</g>';
      svgHtml += '<path d="' + MAPA_GEOM.paths.PL + '" fill="none" stroke="#475569" stroke-width="1.2" />';
    }

    svgHtml += '<path d="' + MAPA_GEOM.paths.WISLA + '" class="gw-mapa-rzeka" fill="none" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.ODRA + '" class="gw-mapa-rzeka" fill="none" />';
    svgHtml += '<text x="180" y="45" class="gw-etykieta-kraju gw-etykieta-baltyk" text-anchor="middle">BA\u0141TYK</text>';
    svgHtml += '<text x="48" y="142" class="gw-etykieta-kraju" text-anchor="middle">NIEMCY</text>';
    svgHtml += '<text x="125" y="220" class="gw-etykieta-kraju" text-anchor="middle">CZECHY</text>';
    svgHtml += '<text x="210" y="250" class="gw-etykieta-kraju" text-anchor="middle">S\u0141OWACJA</text>';
    svgHtml += '<text x="285" y="42" class="gw-etykieta-kraju" text-anchor="middle">LITWA</text>';
    svgHtml += '<text x="198" y="158" class="gw-etykieta-kraju gw-etykieta-polska" text-anchor="middle">POLSKA</text>';

    if (typ !== "globalny") {
      MAPA_GEOM.miasta.forEach(function (m) {
        svgHtml += '<g class="gw-mapa-miasto" transform="translate(' + m.x.toFixed(1) + ',' + m.y.toFixed(1) + ')">';
        svgHtml += '<circle r="2.2" class="gw-miasto-kropka" />';
        svgHtml += '<text dx="4" dy="3" class="gw-miasto-nazwa">' + m.n + '</text>';
        svgHtml += '</g>';
      });
    }

    if ((typ === "regionalny" || typ === "morski") && !maKomorki) {
      if (z.bbox && Array.isArray(z.bbox) && z.bbox.length === 4) {
        const p1 = geoDoSvg(z.bbox[0], z.bbox[3]);
        const p2 = geoDoSvg(z.bbox[2], z.bbox[1]);
        const x = Math.min(p1.x, p2.x);
        const y = Math.min(p1.y, p2.y);
        const wRect = Math.max(Math.abs(p2.x - p1.x), 6);
        const hRect = Math.max(Math.abs(p2.y - p1.y), 6);
        svgHtml += '<rect x="' + x.toFixed(1) + '" y="' + y.toFixed(1) + '" width="' + wRect.toFixed(1) + '" height="' + hRect.toFixed(1) + '" class="gw-mapa-bbox" rx="3" ry="3">';
        svgHtml += '<title>' + escapeHtml(z.etykieta || "Obszar warstwy") + '</title>';
        svgHtml += '</rect>';
      } else if (typ === "regionalny" && ((w.zbior && w.zbior.indexOf("TrueOrtho") >= 0) || (w.warstwa && w.warstwa.indexOf("PrawdziwaOrtofotomapa") >= 0))) {
        MAPA_GEOM.miastaTrueOrtho.forEach(function (m) {
          svgHtml += '<g class="gw-mapa-punkt-trueortho" transform="translate(' + m.x.toFixed(1) + ',' + m.y.toFixed(1) + ')">';
          svgHtml += '<circle r="6" class="gw-punkt-pierscien" />';
          svgHtml += '<circle r="3" class="gw-punkt-kropka" />';
          svgHtml += '<text y="-8" text-anchor="middle" class="gw-punkt-etykieta">' + m.n + '</text>';
          svgHtml += '</g>';
        });
      }
    }

    if (typ === "globalny") {
      svgHtml += '<rect x="25" y="0" width="315" height="275" class="gw-mapa-nakladka-globalna" />';
      svgHtml += '<g class="gw-mapa-globus" transform="translate(182, 137)">';
      svgHtml += '<circle r="46" class="gw-globus-tlo" />';
      svgHtml += '<ellipse rx="46" ry="23" class="gw-globus-siatka" fill="none" />';
      svgHtml += '<ellipse rx="23" ry="46" class="gw-globus-siatka" fill="none" />';
      svgHtml += '<line x1="-46" y1="0" x2="46" y2="0" class="gw-globus-siatka" />';
      svgHtml += '<line x1="0" y1="-46" x2="0" y2="46" class="gw-globus-siatka" />';
      svgHtml += '<text y="4" text-anchor="middle" class="gw-globus-napis">CAŁY ŚWIAT</text>';
      svgHtml += '</g>';
    }

    svgHtml += '</svg>';
    wrap.innerHTML = svgHtml;

    const info = el("div", "gw-zasieg-info-karta");
    const ikona = typ === "morski" ? "🌊" : (typ === "zagranica" ? "🌐" : (typ === "czesciowy" ? "📊" : (typ === "regionalny" ? "📍" : (typ === "globalny" ? "🌍" : "🇵🇱"))));

    const infoTytul = el("div", "gw-zasieg-info-tytul");
    infoTytul.appendChild(el("span", "gw-zasieg-info-ikona", ikona));
    infoTytul.appendChild(el("strong", null, z.etykieta || "Cała Polska"));
    info.appendChild(infoTytul);

    if (z.procent_pl !== undefined) {
      const badgePkt = el("div", "gw-zasieg-procent-wiersz");
      const klasaBadge = z.procent_pl >= 85 ? "gw-zasieg-procent-pelne" : (z.procent_pl >= 30 ? "gw-zasieg-procent-czesciowe" : "gw-zasieg-procent-regionalne");
      const badge = el("span", "gw-zasieg-procent-odznaka " + klasaBadge, "Rzeczywiste pokrycie Polski: ok. " + z.procent_pl + "%");
      badgePkt.appendChild(badge);
      info.appendChild(badgePkt);
    }

    let opisSzczegolowy = z.opis || "";
    if (!opisSzczegolowy) {
      if (typ === "kraj") {
        opisSzczegolowy = "Warstwa obejmuje obszar całej Polski (pokrycie ogólnokrajowe 100%).";
      } else if (typ === "zagranica") {
        opisSzczegolowy = "Urzędowa warstwa zagraniczna (" + (z.kod || "sąsiedzi") + "). Przydatna przy planowaniu tras i pobieraniu map pogranicza.";
      } else if (typ === "morski") {
        opisSzczegolowy = "Polskie obszary morskie, wyłączna strefa ekonomiczna (WZE) lub międzynarodowe dane batymetryczne Bałtyku.";
      } else if (typ === "czesciowy") {
        opisSzczegolowy = "Pokrycie częściowe — mapa dostępna dla wydanych arkuszy.";
      } else if (typ === "regionalny") {
        opisSzczegolowy = z.bbox
          ? "Zasięg regionalny ograniczony do wybranego obszaru historycznego lub geograficznego."
          : "Zasięg regionalny obejmujący wybrane duże miasta i aglomeracje.";
      } else if (typ === "globalny") {
        opisSzczegolowy = "Warstwa o zasięgu globalnym (pokrycie całego świata z serwerów otwartych / międzynarodowych).";
      }
    }
    info.appendChild(el("div", "gw-zasieg-info-opis", opisSzczegolowy));
    wrap.appendChild(info);
    return wrap;
  }

  function odswiezPrzyciskModalaKanapki() {
    if (!ref.modalKanapka || stan.indeksModala < 0 || stan.indeksModala >= stan.widoczne.length) return;
    const w = stan.widoczne[stan.indeksModala];
    const idx = indeksWKanapce(w);
    if (idx >= 0) {
      ref.modalKanapka.classList.add("aktywny");
      ref.modalKanapka.textContent = "✓ W miksie (#" + (idx + 1) + ")";
      ref.modalKanapka.title = "Usuń tę warstwę z kompozycji w Mikserze warstw";
    } else {
      ref.modalKanapka.classList.remove("aktywny");
      ref.modalKanapka.textContent = "☐ Dodaj do miksu";
      ref.modalKanapka.title = "Dodaj warstwę do kompozycji w Mikserze warstw";
    }
    if (ref.modalDodaj) {
      ref.modalDodaj.hidden = idx >= 0;
    }
    if (ref.modalOtworzMikser) {
      ref.modalOtworzMikser.hidden = stan.kanapka.length === 0;
      ref.modalOtworzMikser.textContent = "🧩 Mikser (" + stan.kanapka.length + ")";
    }
  }

  function otworzModal(indeks) {
    if (indeks < 0 || indeks >= stan.widoczne.length) return;
    stan.indeksModala = indeks;
    const w = stan.widoczne[indeks];

    if (ref.modalObrazKol) ref.modalObrazKol.scrollTop = 0;

    ref.modalPanelProbka.innerHTML = "";
    if (w.nakladka) {
      ref.modalPanelProbka.classList.add("gw-nakladka-tlo");
    } else {
      ref.modalPanelProbka.classList.remove("gw-nakladka-tlo");
    }

    const brak = utworzPlaceholder(w.zbior);
    brak.hidden = true;
    if (w.obrazek) {
      const img = document.createElement("img");
      img.src = sciezkaObrazka(w.obrazek);
      img.alt = "Powiększony podgląd warstwy: " + w.nazwa;
      img.addEventListener("error", function () {
        img.hidden = true;
        brak.hidden = false;
      });
      ref.modalPanelProbka.appendChild(img);
    } else {
      brak.hidden = false;
    }
    ref.modalPanelProbka.appendChild(brak);

    ref.modalPanelZasieg.innerHTML = "";
    ref.modalPanelZasieg.appendChild(utworzWidokZasieguSvg(w));

    if (ref.przelaczWidokModala) {
      ref.przelaczWidokModala(stan.widokModala || "probka");
    }

    ref.modalTytul.textContent = w.nazwa;
    const sciezkaCzesci = ["Wybierz mapy", w.grupa];
    if (w.podgrupa) sciezkaCzesci.push(w.podgrupa);
    sciezkaCzesci.push(w.nazwa);
    ref.modalSciezka.textContent = sciezkaCzesci.join(" › ");

    ref.modalTabela.innerHTML = "";
    const wiersze = [
      ["Plik zbioru", w.zbior + ".txt"],
      ["Nazwa warstwy", w.warstwa],
      ["Identyfikator", w.identyfikator || "—"],
      ["Typ usługi", (w.typ || "WMS") + (w.nakladka ? " (nakładka przezroczysta)" : " (podkład)")],
      ["Zalecany piksel", formatujPiksel(w.zalecany_piksel)],
      ["Zasięg przestrzenny", opisZasieguDlaTabeli(w), true],
      ["Źródło", w.zrodlo || "—"],
      ["Miejsce próbki", w.miejsce_probki || "—"]
    ];

    wiersze.forEach(function (para) {
      const tr = document.createElement("tr");
      tr.appendChild(el("th", null, para[0]));
      const td = el("td");
      if (para[2]) {
        const spanTekst = el("span", null, para[1]);
        const btnPokaz = el("button", "gw-link-pokaz-zasieg", "Pokaż na mapie ↗");
        btnPokaz.type = "button";
        btnPokaz.title = "Przełącz widok na mapę zasięgu przestrzennego";
        btnPokaz.addEventListener("click", function () {
          if (ref.przelaczWidokModala) ref.przelaczWidokModala("zasieg");
        });
        td.appendChild(spanTekst);
        td.appendChild(document.createTextNode(" "));
        td.appendChild(btnPokaz);
      } else {
        td.textContent = para[1] || "—";
      }
      tr.appendChild(td);
      ref.modalTabela.appendChild(tr);
    });

    odswiezPrzyciskModalaKanapki();
    ref.modalKopiuj.textContent = "Kopiuj nazwę";
    ref.modalPoprz.disabled = indeks <= 0;
    ref.modalNast.disabled = indeks >= stan.widoczne.length - 1;
    ref.modalTlo.hidden = false;
    ref.modalZamknij.focus();
  }

  function zamknijModal() {
    ref.modalTlo.hidden = true;
    stan.indeksModala = -1;
  }

  // --- Mikser kanapki (UI, podgląd na żywo i eksport) ---

  function czyJestCoResetowac() {
    return stan.kanapka.length > 0 || stan.nazwaKanapki.trim() !== "" || Array.isArray(stan.zaznaczonyBbox);
  }

  function resetujMikser() {
    stan.kanapka = [];
    stan.kanapkaUrlStart = "";
    stan.nazwaKanapki = "";
    stan.zaznaczonyBbox = null;
    stan.podgladPiksel = 0;
    stan.zapiszJakoStyl = false;
    if (ref.mikserInputNazwa) ref.mikserInputNazwa.value = "";
    if (ref.mikserSelSkala) ref.mikserSelSkala.value = "0";
    if (ref.mikserChkStyl) ref.mikserChkStyl.checked = false;
    pokazStatusMiksera("", false);
    poZmianieKanapki();
    rysujMinimape();
  }

  function odswiezDokKanapki() {
    if (!ref.mikserDok) return;
    const n = stan.kanapka.length;
    ref.mikserDokLicznik.textContent = n + " / " + MAKS_WARSTW_KANAPKI + " warstw";
    ref.mikserDok.classList.toggle("zwiniety", stan.dokZwiniety);
    if (ref.mikserDokZwin) {
      ref.mikserDokZwin.textContent = stan.dokZwiniety ? "▴ Rozwiń" : "▾ Zwiń";
      ref.mikserDokZwin.title = stan.dokZwiniety
        ? "Rozwiń dolny pasek Miksera warstw"
        : "Zwiń pasek do małej pigułki w rogu ekranu, aby nie zasłaniał kart";
    }
    if (ref.mikserDokReset) {
      ref.mikserDokReset.hidden = !czyJestCoResetowac();
    }
    if (n === 0) {
      ref.mikserDokOpis.textContent = "Wybierz warstwy przyciskiem „☐ Dodaj do miksu”, aby ułożyć własny miks.";
      ref.mikserDokOtworz.disabled = false;
      if (ref.mikserDokWyslij) ref.mikserDokWyslij.disabled = true;
    } else {
      const minPx = najdokladniejszyPikselKanapki(stan.kanapka);
      const podglad = stan.kanapka.map(function (x) { return x.nazwa; }).slice(0, 3).join(" + ");
      ref.mikserDokOpis.textContent =
        podglad + (n > 3 ? " (+" + (n - 3) + ")" : "") +
        (minPx > 0 ? " · zalecany piksel: " + formatujPiksel(minPx) : "");
      ref.mikserDokOtworz.disabled = false;
      if (ref.mikserDokWyslij) ref.mikserDokWyslij.disabled = false;
    }
  }

  function otworzMikser() {
    stan.mikserOtwarty = true;
    ref.mikserTlo.hidden = false;
    if (ref.przelaczZakladkeMiksera) ref.przelaczZakladkeMiksera(stan.zakladkaMiksera || "podglad");
    renderujListeMiksera();
    rysujMinimape();
    zglosOdswiezeniePodgladuMiksera(true);
    ref.mikserZamknij.focus();
  }

  function zamknijMikser() {
    stan.mikserOtwarty = false;
    ref.mikserTlo.hidden = true;
    if (stan.abortPodglad) {
      stan.abortPodglad.abort();
      stan.abortPodglad = null;
    }
  }

  function pokazStatusMiksera(komunikat, czyBlad) {
    if (!ref.mikserStatusWysylki) return;
    if (!komunikat) {
      ref.mikserStatusWysylki.hidden = true;
      return;
    }
    ref.mikserStatusWysylki.hidden = false;
    ref.mikserStatusWysylki.textContent = komunikat;
    if (czyBlad) ref.mikserStatusWysylki.classList.add("blad");
    else ref.mikserStatusWysylki.classList.remove("blad");
  }

  function czyJestWybranyObszar() {
    return Array.isArray(stan.zaznaczonyBbox) ||
      Boolean(stan.obszarProgramu && stan.obszarProgramu.maSiatke && Array.isArray(stan.obszarProgramu.bboxWgs84));
  }

  function odswiezOstrzezeniaMiksera() {
    if (!ref.mikserOstrzezenia) return;
    const ostrz = ostrzezeniaKanapki(stan.kanapka);
    if (!czyJestWybranyObszar()) {
      ostrz.push(
        czyPolaczonoZProgramem()
          ? "⚠️ Nie zaznaczono jeszcze obszaru na mapie (ani w MapoTero, ani tutaj w mikserze). Przejdź do „📍 Miejsce i obszar” i zaznacz teren prawym przyciskiem myszy (PPM), albo zaznacz obszar w MapoTero."
          : "⚠️ Nie zaznaczono jeszcze obszaru dla pliku .mapotero — możesz zaznaczyć teren prawym przyciskiem myszy (PPM) w zakładce „📍 Miejsce i obszar”."
      );
    }
    if (ostrz.length > 0) {
      ref.mikserOstrzezenia.hidden = false;
      ref.mikserOstrzezenia.textContent = ostrz.join(" ");
    } else {
      ref.mikserOstrzezenia.hidden = true;
    }
  }

  function renderujListeMiksera() {
    if (!ref.mikserWarstwy) return;
    ref.mikserWarstwy.innerHTML = "";

    odswiezOstrzezeniaMiksera();

    if (ref.mikserBtnAuto) ref.mikserBtnAuto.disabled = stan.kanapka.length < 2;
    if (ref.mikserBtnWyczysc) ref.mikserBtnWyczysc.disabled = !czyJestCoResetowac();
    if (ref.mikserBtnWyslijGlowne) ref.mikserBtnWyslijGlowne.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnWyslijKreator) ref.mikserBtnWyslijKreator.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnPobierzPlik) ref.mikserBtnPobierzPlik.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnKopiujLink) ref.mikserBtnKopiujLink.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnKopiujJson) ref.mikserBtnKopiujJson.disabled = stan.kanapka.length === 0;

    if (stan.kanapka.length === 0) {
      ref.mikserWarstwy.appendChild(
        el("div", "gw-brak-wynikow", "Mikser warstw jest pusty. Wybierz warstwy z galerii przyciskiem „☐ Dodaj do miksu”.")
      );
      return;
    }

    // Wyświetlamy od wierzchu (indeks n-1) do podkładu na dole (indeks 0), jak w FormWarstwyZaawansowane
    for (let idx = stan.kanapka.length - 1; idx >= 0; idx--) {
      (function (pozycjaIndeks) {
        const poz = stan.kanapka[pozycjaIndeks];
        const wiersz = el("div", "gw-mikser-wiersz");

        const gora = el("div", "gw-mikser-wiersz-gora");
        const nazwaWrap = el("div", "gw-mikser-poz-nazwa");
        const nr = el("span", "gw-mikser-numer", String(pozycjaIndeks + 1));
        nr.title = pozycjaIndeks === 0 ? "Podkład (na spodzie kanapki)" : "Warstwa #" + (pozycjaIndeks + 1);
        nazwaWrap.appendChild(nr);

        const tytul = el("span", "gw-mikser-tytul-warstwy", poz.nazwa);
        tytul.title = poz.nazwa + " (" + poz.zbior + " / " + poz.warstwa + ")";
        nazwaWrap.appendChild(tytul);

        nazwaWrap.appendChild(
          el("span", "gw-mikser-zbior-etykieta", pozycjaIndeks === 0 ? "Podkład" : (poz.nakladka ? "Nakładka" : poz.zbior))
        );

        const przyciski = el("div", "gw-mikser-przyciski-kolejnosci");
        const btnWyzej = el("button", null, "▲");
        btnWyzej.type = "button";
        btnWyzej.title = "Przesuń warstwę wyżej (bliżej wierzchu)";
        btnWyzej.disabled = pozycjaIndeks >= stan.kanapka.length - 1;
        btnWyzej.addEventListener("click", function () {
          const tmp = stan.kanapka[pozycjaIndeks + 1];
          stan.kanapka[pozycjaIndeks + 1] = stan.kanapka[pozycjaIndeks];
          stan.kanapka[pozycjaIndeks] = tmp;
          poZmianieKanapki();
        });

        const btnNizej = el("button", null, "▼");
        btnNizej.type = "button";
        btnNizej.title = "Przesuń warstwę niżej (bliżej podkładu)";
        btnNizej.disabled = pozycjaIndeks <= 0;
        btnNizej.addEventListener("click", function () {
          const tmp = stan.kanapka[pozycjaIndeks - 1];
          stan.kanapka[pozycjaIndeks - 1] = stan.kanapka[pozycjaIndeks];
          stan.kanapka[pozycjaIndeks] = tmp;
          poZmianieKanapki();
        });

        const btnUsun = el("button", null, "✕");
        btnUsun.type = "button";
        btnUsun.title = "Usuń warstwę z kanapki";
        btnUsun.addEventListener("click", function () {
          stan.kanapka.splice(pozycjaIndeks, 1);
          poZmianieKanapki();
        });

        przyciski.appendChild(btnWyzej);
        przyciski.appendChild(btnNizej);
        przyciski.appendChild(btnUsun);
        gora.appendChild(nazwaWrap);
        gora.appendChild(przyciski);

        const dol = el("div", "gw-mikser-wiersz-dol");

        const suwakGrupa = el("label", "gw-mikser-suwak-grupa");
        suwakGrupa.appendChild(el("span", null, "Krycie:"));
        const suwak = document.createElement("input");
        suwak.type = "range";
        suwak.min = String(MIN_KRYCIE);
        suwak.max = String(PELNE_KRYCIE);
        suwak.step = "5";
        suwak.value = String(poz.krycie);
        const wartoscSpan = el("span", "gw-mikser-krycie-wartosc", poz.krycie + "%");
        suwak.addEventListener("input", function () {
          poz.krycie = poprawneKrycie(suwak.value);
          wartoscSpan.textContent = poz.krycie + "%";
          zapiszKanapkeLocalStorage();
          if (stan.kanapkaUrlStart) zapiszParametryUrl();
          odswiezOstrzezeniaMiksera();
          zglosOdswiezeniePodgladuMiksera();
        });
        suwakGrupa.appendChild(suwak);
        suwakGrupa.appendChild(wartoscSpan);

        dol.appendChild(suwakGrupa);
        if (pozycjaIndeks === 0) {
          const bazaOpis = el("span", "gw-mikser-baza-etykieta", "Podkład bazowy (spód)");
          bazaOpis.title = "Warstwa leży na samym spodzie kanapki — jej tryb mieszania nie ma warstwy poniżej";
          dol.appendChild(bazaOpis);
        } else {
          const selectTryb = document.createElement("select");
          selectTryb.className = "gw-mikser-tryb-select";
          selectTryb.setAttribute("aria-label", "Tryb mieszania warstwy " + poz.nazwa);
          TRYBY_MIESZANIA.forEach(function (t) {
            const opt = document.createElement("option");
            opt.value = t.kod;
            opt.textContent = t.nazwa;
            if (t.kod === poz.tryb) opt.selected = true;
            selectTryb.appendChild(opt);
          });
          selectTryb.addEventListener("change", function () {
            poz.tryb = selectTryb.value;
            poZmianieKanapki();
          });
          dol.appendChild(selectTryb);
        }

        wiersz.appendChild(gora);
        wiersz.appendChild(dol);
        ref.mikserWarstwy.appendChild(wiersz);
      })(idx);
    }
  }

  function zglosOdswiezeniePodgladuMiksera(natychmiast) {
    if (!stan.mikserOtwarty) return;
    if (stan.timerPodgladu) clearTimeout(stan.timerPodgladu);
    if (natychmiast) {
      odswiezPodgladMiksera();
      return;
    }
    const opoznienie = czyPolaczonoZProgramem() && maFunkcjeProgramu("podglad") ? 180 : 30;
    stan.timerPodgladu = setTimeout(odswiezPodgladMiksera, opoznienie);
  }

  function ladujObrazekZCache(url) {
    if (stan.cacheObrazow.has(url)) {
      return Promise.resolve(stan.cacheObrazow.get(url));
    }
    return new Promise(function (resolve) {
      const img = new Image();
      img.onload = function () {
        stan.cacheObrazow.set(url, img);
        resolve(img);
      };
      img.onerror = function () {
        resolve(null);
      };
      img.src = url;
    });
  }

  function odswiezPodgladMiksera() {
    if (!stan.mikserOtwarty || !ref.mikserCanvas) return;
    if (stan.kanapka.length === 0) {
      const ctxPusty = ref.mikserCanvas.getContext("2d");
      ctxPusty.clearRect(0, 0, ref.mikserCanvas.width, ref.mikserCanvas.height);
      ref.mikserCanvas.hidden = false;
      ref.mikserImgLive.hidden = true;
      ref.mikserLadowanie.hidden = true;
      ref.mikserStanPodgladu.classList.remove("ostrzezenie");
      ref.mikserStanPodgladu.textContent = "Dodaj warstwy do kanapki, aby zobaczyć złożony podgląd.";
      return;
    }

    if (czyPolaczonoZProgramem() && maFunkcjeProgramu("podglad")) {
      odswiezPodgladZMapoTero();
    } else {
      odswiezPodgladCanvasOffline();
    }
  }

  function odswiezPodgladZMapoTero() {
    if (stan.abortPodglad) stan.abortPodglad.abort();
    const ctrl = new AbortController();
    stan.abortPodglad = ctrl;

    ref.mikserLadowanie.textContent = "Pobieranie podglądu z MapoTero…";
    ref.mikserLadowanie.hidden = false;
    ref.mikserStanPodgladu.classList.remove("ostrzezenie");
    ref.mikserStanPodgladu.textContent = "Pobieranie podglądu z MapoTero dla wybranego miejsca…";
    const pikselDoWyslania = stan.podgladPiksel > 0
      ? Math.min(MAKS_PIKSEL_PODGLADU, stan.podgladPiksel)
      : null;
    const payload = {
      warstwy: stan.kanapka.map(function (x) {
        return { zbior: x.zbior, warstwa: x.warstwa, krycie: x.krycie, tryb: x.tryb };
      }),
      lat: stan.podgladLat,
      lon: stan.podgladLon,
      piksel: pikselDoWyslania
    };

    fetch("http://127.0.0.1:" + stan.port + "/podglad?token=" + encodeURIComponent(stan.token), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
      signal: ctrl.signal
    })
      .then(function (resp) {
        if (!resp.ok) {
          return resp.json().catch(function () { return { komunikat: "Błąd podglądu" }; }).then(function (j) {
            throw new Error(j.komunikat || "Błąd podglądu");
          });
        }
        const latH = resp.headers.get("X-MapoTero-Lat");
        const lonH = resp.headers.get("X-MapoTero-Lon");
        const pxH = resp.headers.get("X-MapoTero-Piksel");
        const brakH = resp.headers.get("X-MapoTero-Brakujace");
        return resp.blob().then(function (blob) {
          return { blob: blob, latH: latH, lonH: lonH, pxH: pxH, brakH: brakH };
        });
      })
      .then(function (wynik) {
        if (ctrl.signal.aborted) return;
        const staryUrl = ref.mikserImgLive.getAttribute("data-blob-url");
        if (staryUrl) URL.revokeObjectURL(staryUrl);
        const nowyUrl = URL.createObjectURL(wynik.blob);
        ref.mikserImgLive.setAttribute("data-blob-url", nowyUrl);
        ref.mikserImgLive.src = nowyUrl;
        ref.mikserImgLive.hidden = false;
        ref.mikserCanvas.hidden = true;
        ref.mikserLadowanie.hidden = true;

        const opisPunktu = (wynik.latH || stan.podgladLat.toFixed(4)) + ", " + (wynik.lonH || stan.podgladLon.toFixed(4));
        const pxLiczba = parseFloat(wynik.pxH);
        if (Number.isFinite(pxLiczba) && pxLiczba > 0) {
          stan.ostatniPikselRenderu = pxLiczba;
          rysujMinimape();
        }
        const opisPx = wynik.pxH ? " · " + wynik.pxH.replace(".", ",") + " m/px (" + Math.round((pxLiczba || 2) * 360) + " m)" : "";
        let info = czyJestWybranyObszar()
          ? "Podgląd z MapoTero (" + opisPunktu + opisPx + ")"
          : "Podgląd środka widoku mapy (" + opisPunktu + opisPx + ") — brak zaznaczonego obszaru";
        if (stan.zaznaczonyBbox) {
          const pxObszar = obliczPikselDlaBbox(stan.zaznaczonyBbox, 360);
          const bokObszaruKm = ((pxObszar * 360) / 1000).toFixed(1).replace(".", ",");
          info += " · Obszar eksportu ~" + bokObszaruKm + " km";
        }
        info += ".";
        if (wynik.brakH) {
          const brakujace = decodeURIComponent(wynik.brakH).split("|").join(", ");
          info += " Brak obrazu dla: " + brakujace;
          if (pxLiczba >= 10) {
            info += " (część szczegółowych warstw nie wyświetla się przy tej skali — zmień na 1–4 m/px)";
          }
          info += ".";
          ref.mikserStanPodgladu.classList.add("ostrzezenie");
        } else if (!czyJestWybranyObszar()) {
          ref.mikserStanPodgladu.classList.add("ostrzezenie");
        } else {
          ref.mikserStanPodgladu.classList.remove("ostrzezenie");
        }
        ref.mikserStanPodgladu.textContent = info;
      })
      .catch(function (err) {
        if (ctrl.signal.aborted) return;
        ref.mikserLadowanie.hidden = true;
        ref.mikserStanPodgladu.classList.add("ostrzezenie");
        ref.mikserStanPodgladu.textContent = "Podgląd z MapoTero: " + (err && err.message ? err.message : "brak odpowiedzi.");
      });
  }

  function odswiezPodgladCanvasOffline() {
    const mojToken = ++stan.tokenPodgladuOffline;
    ref.mikserCanvas.hidden = false;
    ref.mikserImgLive.hidden = true;
    ref.mikserLadowanie.hidden = false;

    const zProbka = [];
    const bezProbki = [];
    stan.kanapka.forEach(function (item, idx) {
      if (item.probka_miksu) zProbka.push({ item: item, jestSpodem: idx === 0 });
      else bezProbki.push(item);
    });

    const ctx = ref.mikserCanvas.getContext("2d");
    const szer = ref.mikserCanvas.width;
    const wys = ref.mikserCanvas.height;

    if (zProbka.length === 0) {
      const zMiniatura = stan.kanapka.find(function (x) { return Boolean(x.obrazek); });
      if (!zMiniatura) {
        ctx.clearRect(0, 0, szer, wys);
        ref.mikserLadowanie.hidden = true;
        ref.mikserStanPodgladu.classList.add("ostrzezenie");
        ref.mikserStanPodgladu.textContent =
          "Wybrane warstwy nie mają wspólnej próbki statycznej offline. Otwórz galerię z programu MapoTero, aby zobaczyć podgląd na żywo z serwerów.";
        return;
      }
      ladujObrazekZCache(sciezkaObrazka(zMiniatura.obrazek)).then(function (img) {
        if (mojToken !== stan.tokenPodgladuOffline || !stan.mikserOtwarty) return;
        ctx.clearRect(0, 0, szer, wys);
        if (img) ctx.drawImage(img, 0, 0, szer, wys);
        ref.mikserLadowanie.hidden = true;
        ref.mikserStanPodgladu.classList.add("ostrzezenie");
        ref.mikserStanPodgladu.textContent =
          "Podgląd pojedynczej miniatury („" + zMiniatura.nazwa + "”) — wspólny miks offline nie obejmuje jeszcze tej warstwy. Pełny miks dowolnych warstw działa po otwarciu galerii z MapoTero.";
      });
      return;
    }

    const obietnice = zProbka.map(function (wpis) {
      return ladujObrazekZCache(sciezkaObrazka(wpis.item.probka_miksu)).then(function (img) {
        return { item: wpis.item, jestSpodem: wpis.jestSpodem, img: img };
      });
    });

    Promise.all(obietnice).then(function (zaladowane) {
      if (mojToken !== stan.tokenPodgladuOffline || !stan.mikserOtwarty) return;
      ctx.clearRect(0, 0, szer, wys);

      const tempCanvas = document.createElement("canvas");
      tempCanvas.width = szer;
      tempCanvas.height = wys;
      const tempCtx = tempCanvas.getContext("2d");

      const bledne = [];
      let czyCosNarysowano = false;
      zaladowane.forEach(function (para) {
        if (!para.img) {
          bledne.push(para.item.nazwa);
          return;
        }
        const trybInfo = (para.jestSpodem || !czyCosNarysowano)
          ? TRYBY_MIESZANIA[0]
          : znajdzTryb(para.item.tryb);
        if (trybInfo.kod === "RzezbaTerenu") {
          tempCtx.clearRect(0, 0, szer, wys);
          tempCtx.drawImage(para.img, 0, 0, szer, wys);
          const bazaImgData = ctx.getImageData(0, 0, szer, wys);
          const warstwaImgData = tempCtx.getImageData(0, 0, szer, wys);
          nalozRzezbeTerenuNaPiksele(bazaImgData.data, warstwaImgData.data, para.item.krycie);
          ctx.putImageData(bazaImgData, 0, 0);
        } else {
          ctx.save();
          ctx.globalAlpha = poprawneKrycie(para.item.krycie) / 100.0;
          ctx.globalCompositeOperation = trybInfo.css || "source-over";
          ctx.drawImage(para.img, 0, 0, szer, wys);
          ctx.restore();
        }
        czyCosNarysowano = true;
      });

      ref.mikserLadowanie.hidden = true;
      const miejsce = (stan.dane && stan.dane.mikser_miejsce) || "wspólny kadr testowy";
      const dopisekObszaru = stan.zaznaczonyBbox
        ? " Zaznaczony obszar zapisze się w pliku .mapotero (podgląd na żywo tego miejsca wymaga otwarcia galerii z MapoTero)."
        : "";
      if (bezProbki.length > 0 || bledne.length > 0) {
        const pomin = bezProbki.map(function (x) { return x.nazwa; }).concat(bledne);
        ref.mikserStanPodgladu.classList.add("ostrzezenie");
        ref.mikserStanPodgladu.textContent =
          "Tryb przeglądarki — stała próbka (" + miejsce + "). Bez wspólnej próbki (" + pomin.join(", ") + ")." + dopisekObszaru;
      } else {
        ref.mikserStanPodgladu.classList.remove("ostrzezenie");
        ref.mikserStanPodgladu.textContent =
          "Tryb przeglądarki — stała próbka: " + miejsce + "." + dopisekObszaru;
      }
    });
  }

  function pobierzKafelekOsm(z, tx, ty) {
    const n = Math.pow(2, z);
    if (ty < 0 || ty >= n) return null;
    const xWrap = ((tx % n) + n) % n;
    const klucz = z + "/" + xWrap + "/" + ty;
    if (stan.cacheKafelkowOsm.has(klucz)) {
      return stan.cacheKafelkowOsm.get(klucz);
    }
    const wpis = { status: "loading", img: new Image() };
    stan.cacheKafelkowOsm.set(klucz, wpis);
    wpis.img.onload = function () {
      wpis.status = "ok";
      if (stan.mikserOtwarty && stan.zakladkaMiksera === "mapa") rysujMinimape();
    };
    wpis.img.onerror = function () {
      wpis.status = "err";
    };
    wpis.img.src = "https://tile.openstreetmap.org/" + z + "/" + xWrap + "/" + ty + ".png";
    return wpis;
  }

  function rysujBboxNaMapie(ctx, bbox, cX, cY, z, szer, wys, kolorObrysu, kolorWypelnienia, przerywana) {
    if (!Array.isArray(bbox) || bbox.length !== 4) return;
    const minLon = Math.min(bbox[0], bbox[2]);
    const maxLon = Math.max(bbox[0], bbox[2]);
    const minLat = Math.min(bbox[1], bbox[3]);
    const maxLat = Math.max(bbox[1], bbox[3]);
    const p1 = latLonDoKafelka(maxLat, minLon, z);
    const p2 = latLonDoKafelka(minLat, maxLon, z);
    const x1 = szer / 2 + (p1.x - cX) * 256;
    const y1 = wys / 2 + (p1.y - cY) * 256;
    const x2 = szer / 2 + (p2.x - cX) * 256;
    const y2 = wys / 2 + (p2.y - cY) * 256;
    const rw = Math.max(4, x2 - x1);
    const rh = Math.max(4, y2 - y1);
    ctx.save();
    if (kolorWypelnienia) {
      ctx.fillStyle = kolorWypelnienia;
      ctx.fillRect(x1, y1, rw, rh);
    }
    ctx.strokeStyle = kolorObrysu;
    ctx.lineWidth = 2;
    if (przerywana) ctx.setLineDash([5, 4]);
    ctx.strokeRect(x1, y1, rw, rh);
    ctx.restore();
  }

  function rysujMinimape(tymczasowyProstokat) {
    if (ref.minimapaBtnWyczyscZazn) {
      ref.minimapaBtnWyczyscZazn.hidden = !stan.zaznaczonyBbox;
    }
    if (ref.mikserBtnWyczysc) {
      ref.mikserBtnWyczysc.disabled = !czyJestCoResetowac();
    }
    if (ref.mikserDokReset) {
      ref.mikserDokReset.hidden = !czyJestCoResetowac();
    }
    if (ref.mikserChkObszarWrap) {
      ref.mikserChkObszarWrap.hidden = !stan.zaznaczonyBbox;
    }
    if (ref.minimapaBtnDopasujPodglad) {
      const malyObszar = Array.isArray(stan.zaznaczonyBbox) &&
        obliczPikselDlaBbox(stan.zaznaczonyBbox, 360) <= MAKS_PIKSEL_PODGLADU;
      ref.minimapaBtnDopasujPodglad.hidden = !malyObszar;
    }
    if (ref.mikserZakladkaMapa) {
      ref.mikserZakladkaMapa.textContent = czyJestWybranyObszar()
        ? "📍 Miejsce i obszar ✓"
        : "📍 Miejsce i obszar ⚠️";
    }
    odswiezOstrzezeniaMiksera();
    if (ref.minimapaInfo) {
      let txt = czyPolaczonoZProgramem()
        ? (czyJestWybranyObszar()
            ? "Próbka (czerwona): " + stan.podgladLat.toFixed(4) + ", " + stan.podgladLon.toFixed(4)
            : "⚠️ Brak zaznaczonego obszaru · Środek: " + stan.podgladLat.toFixed(4) + ", " + stan.podgladLon.toFixed(4))
        : "Obszar do pliku .mapotero";
      if (stan.zaznaczonyBbox) {
        const pxAuto = obliczPikselDlaBbox(stan.zaznaczonyBbox, 360);
        const bokKm = ((pxAuto * 360) / 1000).toFixed(1).replace(".", ",");
        txt += (czyPolaczonoZProgramem() ? " · Obszar dla MapoTero: ~" : ": ~") + bokKm + " km";
        if (!czyPolaczonoZProgramem()) {
          txt += " (podgląd na żywo tego miejsca wymaga otwarcia galerii z MapoTero; tu widać stałą próbkę Tatr)";
        } else if (pxAuto > MAKS_PIKSEL_DOPASOWANIA_BBOX) {
          txt += " (duży obszar — podgląd pokazuje detal w skali roboczej)";
        }
      } else if (stan.obszarProgramu && stan.obszarProgramu.maSiatke) {
        txt += " · Niebieska ramka: obszar z MapoTero";
      } else {
        txt += czyPolaczonoZProgramem()
          ? " · Zaznacz prawym przyciskiem myszy (PPM) lub wybierz obszar w MapoTero"
          : " · Brak obszaru — zaznacz prawym przyciskiem (PPM) lub przyciskiem „Zaznacz obszar”";
      }
      ref.minimapaInfo.textContent = txt;
    }

    if (!ref.minimapaCanvas || stan.zakladkaMiksera !== "mapa") return;
    const canvas = ref.minimapaCanvas;
    const ctx = canvas.getContext("2d");
    const szer = canvas.width;
    const wys = canvas.height;
    const z = Math.max(5, Math.min(16, stan.mapaZoom));

    const centerTile = latLonDoKafelka(stan.mapaCenterLat, stan.mapaCenterLon, z);
    const cX = centerTile.x;
    const cY = centerTile.y;

    // Tło + siatka + miasta orientacyjne w rzucie Web Mercator (zawsze widoczne nawet przed wczytaniem kafli)
    ctx.fillStyle = "#eef2f6";
    ctx.fillRect(0, 0, szer, wys);

    const txStart = Math.floor(cX - szer / 512) - 1;
    const txEnd = Math.ceil(cX + szer / 512) + 1;
    const tyStart = Math.floor(cY - wys / 512) - 1;
    const tyEnd = Math.ceil(cY + wys / 512) + 1;

    let brakujaceKafle = false;
    for (let ty = tyStart; ty <= tyEnd; ty++) {
      for (let tx = txStart; tx <= txEnd; tx++) {
        const px = Math.round(szer / 2 + (tx - cX) * 256);
        const py = Math.round(wys / 2 + (ty - cY) * 256);
        const wpis = pobierzKafelekOsm(z, tx, ty);
        if (wpis && wpis.status === "ok") {
          ctx.drawImage(wpis.img, px, py, 256, 256);
        } else {
          brakujaceKafle = true;
        }
      }
    }

    if (brakujaceKafle) {
      ctx.save();
      ctx.fillStyle = "#475569";
      ctx.font = "11px sans-serif";
      SKROTY_MIEJSC.forEach(function (m) {
        const pt = latLonDoKafelka(m.lat, m.lon, z);
        const mx = szer / 2 + (pt.x - cX) * 256;
        const my = wys / 2 + (pt.y - cY) * 256;
        if (mx >= -30 && mx <= szer + 30 && my >= -20 && my <= wys + 20) {
          ctx.beginPath();
          ctx.arc(mx, my, 3, 0, Math.PI * 2);
          ctx.fill();
          ctx.fillText(m.nazwa, mx + 5, my + 4);
        }
      });
      ctx.restore();
    }

    // 1. Obszar zaznaczony w programie MapoTero (niebieska przerywana ramka)
    if (stan.obszarProgramu && Array.isArray(stan.obszarProgramu.bboxWgs84)) {
      rysujBboxNaMapie(ctx, stan.obszarProgramu.bboxWgs84, cX, cY, z, szer, wys, "#0284c7", "rgba(2, 132, 199, 0.12)", true);
    }

    // 2. Obszar zaznaczony na mapce w galerii (fioletowa ramka)
    if (Array.isArray(stan.zaznaczonyBbox)) {
      rysujBboxNaMapie(ctx, stan.zaznaczonyBbox, cX, cY, z, szer, wys, "#7253ed", "rgba(114, 83, 237, 0.18)", false);
    }

    // 3. Aktualny obszar próbki podglądu 360x360 px (czerwona ramka + celownik)
    const bboxProbki = bboxZCentrumIPiksela(stan.podgladLat, stan.podgladLon, stan.podgladPiksel > 0 ? stan.podgladPiksel : stan.ostatniPikselRenderu, 360);
    rysujBboxNaMapie(ctx, bboxProbki, cX, cY, z, szer, wys, "#dc2626", "rgba(220, 38, 38, 0.08)", false);

    const ptSrodek = latLonDoKafelka(stan.podgladLat, stan.podgladLon, z);
    const sx = szer / 2 + (ptSrodek.x - cX) * 256;
    const sy = wys / 2 + (ptSrodek.y - cY) * 256;
    ctx.save();
    ctx.strokeStyle = "#dc2626";
    ctx.lineWidth = 1.6;
    ctx.beginPath();
    ctx.moveTo(sx - 8, sy);
    ctx.lineTo(sx + 8, sy);
    ctx.moveTo(sx, sy - 8);
    ctx.lineTo(sx, sy + 8);
    ctx.stroke();
    ctx.restore();

    // 4. Prostokąt w trakcie zaznaczania myszą
    if (tymczasowyProstokat) {
      const rx = Math.min(tymczasowyProstokat.x1, tymczasowyProstokat.x2);
      const ry = Math.min(tymczasowyProstokat.y1, tymczasowyProstokat.y2);
      const rw = Math.abs(tymczasowyProstokat.x2 - tymczasowyProstokat.x1);
      const rh = Math.abs(tymczasowyProstokat.y2 - tymczasowyProstokat.y1);
      ctx.save();
      ctx.fillStyle = "rgba(114, 83, 237, 0.22)";
      ctx.strokeStyle = "#5b3fd4";
      ctx.lineWidth = 2;
      ctx.setLineDash([4, 3]);
      ctx.fillRect(rx, ry, rw, rh);
      ctx.strokeRect(rx, ry, rw, rh);
      ctx.restore();
    }

    if (ref.minimapaZoomTekst) {
      ref.minimapaZoomTekst.textContent = "Zoom: " + z;
    }
  }

  function utworzGrupePigulek(etykieta, opcje, pobierzAktualna, onZmiana) {
    const grupa = el("div", "gw-pigulki");
    grupa.appendChild(el("span", "gw-etykieta-filtru", etykieta));
    const przyciski = [];

    function odswiez() {
      const akt = pobierzAktualna();
      przyciski.forEach(function (para) {
        if (para.wartosc === akt) para.btn.classList.add("aktywny");
        else para.btn.classList.remove("aktywny");
      });
    }

    opcje.forEach(function (opcja) {
      const btn = el("button", "gw-btn-filtr", opcja.tekst);
      btn.type = "button";
      btn.addEventListener("click", function () {
        onZmiana(opcja.wartosc);
        odswiez();
        renderujSiatke();
      });
      przyciski.push({ btn: btn, wartosc: opcja.wartosc });
      grupa.appendChild(btn);
    });

    odswiez();
    return { element: grupa, odswiez: odswiez };
  }

  function zbudujMikserUi() {
    // Dok u dołu ekranu
    const mikserDok = el("div", "gw-mikser-dok");
    const dokInfo = el("div", "gw-mikser-dok-info");
    const dokLicznik = el("span", "gw-mikser-badge", "0 / " + MAKS_WARSTW_KANAPKI + " warstw");
    const dokOpis = el("span", "gw-mikser-dok-opis", "");
    dokInfo.appendChild(el("strong", null, "🧩 Mikser warstw:"));
    dokInfo.appendChild(dokLicznik);
    dokInfo.appendChild(dokOpis);

    const dokAkcje = el("div", "gw-mikser-dok-akcje");
    const dokOtworz = el("button", "gw-btn-glowny", "Otwórz mikser");
    dokOtworz.type = "button";
    dokOtworz.title = "Otwórz okno Miksera warstw z podglądem i ustawieniami krycia";
    dokOtworz.addEventListener("click", otworzMikser);
    dokAkcje.appendChild(dokOtworz);

    const dokAkcjePelne = el("div", "gw-mikser-dok-akcje-pelne");
    let dokWyslij = null;
    if (czyPolaczonoZProgramem()) {
      dokWyslij = el("button", "gw-btn-zielony", "Wyślij do MapoTero");
      dokWyslij.type = "button";
      dokWyslij.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("okno_glowne", false);
      });
      dokAkcjePelne.appendChild(dokWyslij);
    }

    const dokReset = el("button", "gw-btn-prosty", "↺ Resetuj");
    dokReset.type = "button";
    dokReset.hidden = true;
    dokReset.title = "Usuwa wszystkie warstwy z miksu i czyści zapamiętane ustawienia";
    dokReset.addEventListener("click", resetujMikser);
    dokAkcjePelne.appendChild(dokReset);
    dokAkcje.appendChild(dokAkcjePelne);

    const dokZwin = el("button", "gw-btn-prosty", "▾ Zwiń");
    dokZwin.type = "button";
    dokZwin.addEventListener("click", function () {
      stan.dokZwiniety = !stan.dokZwiniety;
      odswiezDokKanapki();
    });
    dokAkcje.appendChild(dokZwin);

    mikserDok.appendChild(dokInfo);
    mikserDok.appendChild(dokAkcje);

    // Okno modalne miksera
    const mikserTlo = el("div", "gw-mikser-modal-tlo");
    mikserTlo.hidden = true;
    mikserTlo.setAttribute("role", "dialog");
    mikserTlo.setAttribute("aria-modal", "true");
    mikserTlo.addEventListener("click", function (e) {
      if (e.target === mikserTlo) zamknijMikser();
    });

    const mikserModal = el("div", "gw-mikser-modal");
    const mikserNaglowek = el("div", "gw-mikser-naglowek");
    const mikserNaglowekTekst = el("div", "gw-mikser-naglowek-tekst");
    mikserNaglowekTekst.appendChild(el("h3", null, "🧩 Mikser warstw"));
    mikserNaglowekTekst.appendChild(
      el("span", "gw-mikser-podtytul", "Ułóż kolejność, przeźroczystość i tryb mieszania warstw, a potem wyślij zestaw do MapoTero")
    );
    mikserNaglowek.appendChild(mikserNaglowekTekst);
    const mikserZamknij = el("button", "gw-modal-zamknij gw-btn-zamknij-tekst", "Zamknij ×");
    mikserZamknij.type = "button";
    mikserZamknij.setAttribute("aria-label", "Zamknij mikser");
    mikserZamknij.title = "Zamknij okno miksera (Esc)";
    mikserZamknij.addEventListener("click", zamknijMikser);
    mikserNaglowek.appendChild(mikserZamknij);

    const mikserCialo = el("div", "gw-mikser-cialo");

    // Lewa kolumna: przełączane zakładki [Podgląd | Miejsce i obszar]
    const kolPodglad = el("div", "gw-mikser-kol-podglad");

    const mikserTabs = el("div", "gw-mikser-tabs");
    mikserTabs.setAttribute("role", "tablist");
    const tabPodglad = el("button", "gw-mikser-tab aktywny", "👁 Podgląd wyniku");
    tabPodglad.type = "button";
    tabPodglad.setAttribute("role", "tab");
    tabPodglad.setAttribute("aria-selected", "true");

    const tabMapa = el("button", "gw-mikser-tab", "📍 Miejsce i obszar");
    tabMapa.type = "button";
    tabMapa.setAttribute("role", "tab");
    tabMapa.setAttribute("aria-selected", "false");
    mikserTabs.appendChild(tabPodglad);
    mikserTabs.appendChild(tabMapa);

    const panelPodglad = el("div", "gw-mikser-panel");
    const ramkaPodgladu = el("div", "gw-mikser-ramka-podgladu");
    const mikserCanvas = document.createElement("canvas");
    mikserCanvas.width = 360;
    mikserCanvas.height = 360;
    const mikserImgLive = document.createElement("img");
    mikserImgLive.alt = "Podgląd złożonej kanapki z MapoTero";
    mikserImgLive.hidden = true;
    const mikserLadowanie = el("div", "gw-mikser-ladowanie", "Składanie podglądu…");
    mikserLadowanie.hidden = true;
    ramkaPodgladu.appendChild(mikserCanvas);
    ramkaPodgladu.appendChild(mikserImgLive);
    ramkaPodgladu.appendChild(mikserLadowanie);

    const sterowaniePodgladu = el("div", "gw-mikser-sterowanie-podgladu");
    const selSkala = document.createElement("select");
    selSkala.setAttribute("aria-label", "Rozdzielczość podglądu");
    [
      { val: "0", txt: "Skala: Auto" },
      { val: "0.5", txt: "0,5 m/px" },
      { val: "1", txt: "1 m/px" },
      { val: "2", txt: "2 m/px" },
      { val: "4", txt: "4 m/px" },
      { val: "10", txt: "10 m/px" },
      { val: "25", txt: "25 m/px" }
    ].forEach(function (op) {
      const opt = document.createElement("option");
      opt.value = op.val;
      opt.textContent = op.txt;
      selSkala.appendChild(opt);
    });

    function zsynchronizujSelectSkali(piksel) {
      const staraCustom = selSkala.querySelector("option[data-custom='1']");
      if (staraCustom) staraCustom.remove();
      const val = Number(piksel) > 0 ? Math.min(MAKS_PIKSEL_PODGLADU, Number(piksel)) : 0;
      if (val <= 0) {
        selSkala.value = "0";
        return;
      }
      const kandydat = String(val);
      const maOpcje = Array.prototype.some.call(selSkala.options, function (o) { return o.value === kandydat; });
      if (maOpcje) {
        selSkala.value = kandydat;
      } else {
        const opt = document.createElement("option");
        opt.value = kandydat;
        opt.textContent = (Math.round(val * 10) / 10).toString().replace(".", ",") + " m/px (obszar)";
        opt.setAttribute("data-custom", "1");
        selSkala.appendChild(opt);
        selSkala.value = kandydat;
      }
    }

    function ustawSkalePodgladu(nowyPiksel) {
      const wartosc = Number(nowyPiksel) > 0 ? Math.min(MAKS_PIKSEL_PODGLADU, Number(nowyPiksel)) : 0;
      stan.podgladPiksel = wartosc;
      zsynchronizujSelectSkali(wartosc);
      rysujMinimape();
      zglosOdswiezeniePodgladuMiksera(true);
    }

    selSkala.addEventListener("change", function () {
      ustawSkalePodgladu(parseFloat(selSkala.value) || 0);
    });

    if (czyPolaczonoZProgramem()) {
      const skalaGrupa = el("div", "gw-mikser-skala-grupa");
      const btnPodgladOddal = el("button", "gw-mikser-btn-zoom", "−");
      btnPodgladOddal.type = "button";
      btnPodgladOddal.title = "Oddal podgląd (większy obszar w m/px)";
      btnPodgladOddal.addEventListener("click", function () {
        ustawSkalePodgladu(wybierzNastepnyStopienSkali(stan.podgladPiksel, stan.ostatniPikselRenderu, -1));
      });

      const btnPodgladPrzybliz = el("button", "gw-mikser-btn-zoom", "+");
      btnPodgladPrzybliz.type = "button";
      btnPodgladPrzybliz.title = "Przybliż podgląd (dokładniejsza skala w m/px)";
      btnPodgladPrzybliz.addEventListener("click", function () {
        ustawSkalePodgladu(wybierzNastepnyStopienSkali(stan.podgladPiksel, stan.ostatniPikselRenderu, 1));
      });

      skalaGrupa.appendChild(btnPodgladOddal);
      skalaGrupa.appendChild(selSkala);
      skalaGrupa.appendChild(btnPodgladPrzybliz);
      sterowaniePodgladu.appendChild(skalaGrupa);

      ramkaPodgladu.title = "Użyj przycisków − / + lub kółka myszy, aby przybliżyć lub oddalić podgląd";
      ramkaPodgladu.addEventListener("wheel", function (e) {
        if (stan.kanapka.length === 0) return;
        e.preventDefault();
        const kierunek = e.deltaY < 0 ? 1 : -1;
        ustawSkalePodgladu(wybierzNastepnyStopienSkali(stan.podgladPiksel, stan.ostatniPikselRenderu, kierunek));
      }, { passive: false });

      const btnOdswiezPodglad = el("button", "gw-btn-prosty", "↻ Odśwież");
      btnOdswiezPodglad.type = "button";
      btnOdswiezPodglad.title = "Pobierz ponownie podgląd z MapoTero";
      btnOdswiezPodglad.addEventListener("click", function () {
        zglosOdswiezeniePodgladuMiksera(true);
      });
      sterowaniePodgladu.appendChild(btnOdswiezPodglad);
    }

    const btnZmienMiejsce = el("button", "gw-btn-prosty", "📍 Zmień miejsce");
    btnZmienMiejsce.type = "button";
    btnZmienMiejsce.title = "Otwórz mapę wyboru miejsca próbki oraz obszaru dla MapoTero";
    btnZmienMiejsce.addEventListener("click", function () {
      przelaczZakladkeMiksera("mapa");
    });
    sterowaniePodgladu.appendChild(btnZmienMiejsce);

    const mikserStanPodgladu = el("div", "gw-mikser-stan-podgladu", "");

    panelPodglad.appendChild(ramkaPodgladu);
    panelPodglad.appendChild(sterowaniePodgladu);
    panelPodglad.appendChild(mikserStanPodgladu);

    const minimapaBox = el("div", "gw-mikser-panel gw-mikser-minimapa-box");
    minimapaBox.hidden = true;

    const minimapaNarzedzia = el("div", "gw-minimapa-narzedzia");
    const grupaTryby = el("div", "gw-minimapa-grupa-btn");
    const btnTrybPrzesun = el("button", "aktywny", "🖐 Przesuń / Punkt");
    btnTrybPrzesun.type = "button";
    btnTrybPrzesun.title = "Lewy: przesuwaj mapę lub kliknij punkt próbki. Prawy przycisk (PPM): od razu zaznacz obszar";
    const btnTrybZaznacz = el("button", null, "⬜ Zaznacz obszar");
    btnTrybZaznacz.type = "button";
    btnTrybZaznacz.title = "Zaznacz obszar lewym przyciskiem myszy (prawym przyciskiem zaznaczasz zawsze od razu)";
    const btnWyczyscZazn = el("button", null, "✕ Wyczyść");
    btnWyczyscZazn.type = "button";
    btnWyczyscZazn.title = "Usuń zaznaczony obszar";
    btnWyczyscZazn.hidden = true;

    function ustawTrybMapy(tryb) {
      stan.mapaTryb = tryb;
      if (tryb === "zaznacz") {
        btnTrybZaznacz.classList.add("aktywny");
        btnTrybPrzesun.classList.remove("aktywny");
        minimapaCanvas.classList.remove("tryb-przesun");
        minimapaCanvas.classList.add("tryb-zaznacz");
      } else {
        btnTrybPrzesun.classList.add("aktywny");
        btnTrybZaznacz.classList.remove("aktywny");
        minimapaCanvas.classList.remove("tryb-zaznacz");
        minimapaCanvas.classList.add("tryb-przesun");
      }
    }

    btnTrybPrzesun.addEventListener("click", function () { ustawTrybMapy("przesun"); });
    btnTrybZaznacz.addEventListener("click", function () { ustawTrybMapy("zaznacz"); });
    btnWyczyscZazn.addEventListener("click", function () {
      stan.zaznaczonyBbox = null;
      stan.podgladPiksel = 0;
      zsynchronizujSelectSkali(0);
      rysujMinimape();
      zglosOdswiezeniePodgladuMiksera(true);
    });

    grupaTryby.appendChild(btnTrybPrzesun);
    grupaTryby.appendChild(btnTrybZaznacz);
    grupaTryby.appendChild(btnWyczyscZazn);

    const grupaZoom = el("div", "gw-minimapa-grupa-btn");
    const btnZoomMinus = el("button", null, "−");
    btnZoomMinus.type = "button";
    btnZoomMinus.title = "Oddal mapę";
    btnZoomMinus.addEventListener("click", function () {
      stan.mapaZoom = Math.max(5, stan.mapaZoom - 1);
      rysujMinimape();
    });
    const zoomTekst = el("span", "gw-minimapa-zoom-tekst", "Zoom: " + stan.mapaZoom);
    const btnZoomPlus = el("button", null, "+");
    btnZoomPlus.type = "button";
    btnZoomPlus.title = "Przybliż mapę";
    btnZoomPlus.addEventListener("click", function () {
      stan.mapaZoom = Math.min(16, stan.mapaZoom + 1);
      rysujMinimape();
    });
    grupaZoom.appendChild(btnZoomMinus);
    grupaZoom.appendChild(zoomTekst);
    grupaZoom.appendChild(btnZoomPlus);

    minimapaNarzedzia.appendChild(grupaTryby);
    minimapaNarzedzia.appendChild(grupaZoom);

    const minimapaWrap = el("div", "gw-minimapa-canvas-wrap");
    const minimapaCanvas = document.createElement("canvas");
    minimapaCanvas.width = 360;
    minimapaCanvas.height = 290;
    minimapaCanvas.className = "gw-minimapa-canvas tryb-przesun";
    minimapaWrap.appendChild(minimapaCanvas);

    const minimapaInfo = el("div", "gw-minimapa-info", "");

    function wspolrzedneCanvasaMapy(e) {
      const rect = minimapaCanvas.getBoundingClientRect();
      return {
        x: ((e.clientX - rect.left) / rect.width) * minimapaCanvas.width,
        y: ((e.clientY - rect.top) / rect.height) * minimapaCanvas.height
      };
    }

    function pikselCanvasaNaGeo(pxX, pxY) {
      const c = latLonDoKafelka(stan.mapaCenterLat, stan.mapaCenterLon, stan.mapaZoom);
      const tx = c.x + (pxX - minimapaCanvas.width / 2) / 256;
      const ty = c.y + (pxY - minimapaCanvas.height / 2) / 256;
      return kafelekDoLatLon(tx, ty, stan.mapaZoom);
    }

    let gest = null;
    minimapaCanvas.addEventListener("contextmenu", function (e) {
      e.preventDefault();
    });
    minimapaCanvas.addEventListener("pointerdown", function (e) {
      if (e.button !== 0 && e.button !== 2) return;
      e.preventDefault();
      minimapaCanvas.setPointerCapture(e.pointerId);
      const pt = wspolrzedneCanvasaMapy(e);
      const czyZaznaczanie = e.button === 2 || stan.mapaTryb === "zaznacz" || e.shiftKey;
      gest = {
        id: e.pointerId,
        przycisk: e.button,
        tryb: czyZaznaczanie ? "zaznacz" : "przesun",
        startX: pt.x,
        startY: pt.y,
        aktX: pt.x,
        aktY: pt.y,
        przeciagano: false,
        startCenterTile: latLonDoKafelka(stan.mapaCenterLat, stan.mapaCenterLon, stan.mapaZoom)
      };
      if (gest.tryb === "przesun") minimapaCanvas.classList.add("przeciaga");
    });

    minimapaCanvas.addEventListener("pointermove", function (e) {
      if (!gest || gest.id !== e.pointerId) return;
      const pt = wspolrzedneCanvasaMapy(e);
      const dx = pt.x - gest.startX;
      const dy = pt.y - gest.startY;
      if (Math.hypot(dx, dy) > 4) gest.przeciagano = true;

      if (gest.tryb === "przesun") {
        if (gest.przeciagano) {
          const nowyCenter = kafelekDoLatLon(
            gest.startCenterTile.x - dx / 256,
            gest.startCenterTile.y - dy / 256,
            stan.mapaZoom
          );
          stan.mapaCenterLat = Math.max(-80, Math.min(80, nowyCenter.lat));
          stan.mapaCenterLon = Math.max(-179, Math.min(179, nowyCenter.lon));
          rysujMinimape();
        }
      } else {
        gest.aktX = pt.x;
        gest.aktY = pt.y;
        rysujMinimape({ x1: gest.startX, y1: gest.startY, x2: gest.aktX, y2: gest.aktY });
      }
    });

    function zakonczGestMapy(e) {
      if (!gest || gest.id !== e.pointerId) return;
      const aktGest = gest;
      gest = null;
      minimapaCanvas.classList.remove("przeciaga");

      const pt = wspolrzedneCanvasaMapy(e);
      if (aktGest.tryb === "zaznacz" && aktGest.przeciagano) {
        const x1 = Math.min(aktGest.startX, pt.x);
        const x2 = Math.max(aktGest.startX, pt.x);
        const y1 = Math.min(aktGest.startY, pt.y);
        const y2 = Math.max(aktGest.startY, pt.y);
        if (x2 - x1 >= 8 && y2 - y1 >= 8) {
          const nw = pikselCanvasaNaGeo(x1, y1);
          const se = pikselCanvasaNaGeo(x2, y2);
          stan.zaznaczonyBbox = [
            Math.round(nw.lon * 100000) / 100000,
            Math.round(se.lat * 100000) / 100000,
            Math.round(se.lon * 100000) / 100000,
            Math.round(nw.lat * 100000) / 100000
          ];
          stan.podgladLat = Math.round(((nw.lat + se.lat) / 2) * 100000) / 100000;
          stan.podgladLon = Math.round(((nw.lon + se.lon) / 2) * 100000) / 100000;

          const wyborPx = wybierzPikselPoZaznaczeniuBbox(stan.zaznaczonyBbox, parseFloat(selSkala.value) || 0);
          if (czyPolaczonoZProgramem()) {
            if (!wyborPx.rozdzielony) {
              stan.podgladPiksel = wyborPx.piksel;
              stan.ostatniPikselRenderu = wyborPx.piksel;
              zsynchronizujSelectSkali(wyborPx.piksel);
            } else {
              stan.podgladPiksel = wyborPx.piksel;
              zsynchronizujSelectSkali(wyborPx.piksel);
            }
          }
          rysujMinimape();
          zglosOdswiezeniePodgladuMiksera(true);
          return;
        }
      }

      if (!aktGest.przeciagano) {
        const geo = pikselCanvasaNaGeo(pt.x, pt.y);
        stan.podgladLat = Math.round(geo.lat * 10000) / 10000;
        stan.podgladLon = Math.round(geo.lon * 10000) / 10000;
        rysujMinimape();
        zglosOdswiezeniePodgladuMiksera(true);
      } else {
        rysujMinimape();
      }
    }

    minimapaCanvas.addEventListener("pointerup", zakonczGestMapy);
    minimapaCanvas.addEventListener("pointercancel", zakonczGestMapy);
    minimapaCanvas.addEventListener("wheel", function (e) {
      e.preventDefault();
      const delta = e.deltaY < 0 ? 1 : -1;
      const nowyZoom = Math.max(5, Math.min(16, stan.mapaZoom + delta));
      if (nowyZoom === stan.mapaZoom) return;
      const pt = wspolrzedneCanvasaMapy(e);
      const podKursorem = pikselCanvasaNaGeo(pt.x, pt.y);
      stan.mapaZoom = nowyZoom;
      const poZoom = latLonDoKafelka(podKursorem.lat, podKursorem.lon, nowyZoom);
      const nowySrodek = kafelekDoLatLon(
        poZoom.x - (pt.x - minimapaCanvas.width / 2) / 256,
        poZoom.y - (pt.y - minimapaCanvas.height / 2) / 256,
        nowyZoom
      );
      stan.mapaCenterLat = Math.max(-80, Math.min(80, nowySrodek.lat));
      stan.mapaCenterLon = Math.max(-179, Math.min(179, nowySrodek.lon));
      rysujMinimape();
    }, { passive: false });

    const skrotyBox = el("div", "gw-minimapa-skroty");
    if (czyPolaczonoZProgramem()) {
      const btnZMapoTero = el("button", null, "↺ Obszar z MapoTero");
      btnZMapoTero.type = "button";
      btnZMapoTero.title = "Pobierz ponownie środek i obszar zaznaczenia z okna MapoTero";
      btnZMapoTero.addEventListener("click", function () {
        stan.zaznaczonyBbox = null;
        stan.podgladPiksel = 0;
        zsynchronizujSelectSkali(0);
        pobierzStanZMapoTero(false, false);
      });
      skrotyBox.appendChild(btnZMapoTero);
    }

    const btnPolska = el("button", null, "Polska");
    btnPolska.type = "button";
    btnPolska.addEventListener("click", function () {
      stan.mapaCenterLat = 52.0;
      stan.mapaCenterLon = 19.2;
      stan.mapaZoom = 6;
      rysujMinimape();
    });
    skrotyBox.appendChild(btnPolska);

    SKROTY_MIEJSC.forEach(function (m) {
      const b = el("button", null, m.nazwa);
      b.type = "button";
      b.addEventListener("click", function () {
        stan.podgladLat = m.lat;
        stan.podgladLon = m.lon;
        stan.mapaCenterLat = m.lat;
        stan.mapaCenterLon = m.lon;
        if (stan.mapaZoom < 11) stan.mapaZoom = 12;
        rysujMinimape();
        zglosOdswiezeniePodgladuMiksera(true);
      });
      skrotyBox.appendChild(b);
    });

    const akcjeMapyDol = el("div", "gw-mikser-sterowanie-podgladu");
    const btnGotoweMapa = el("button", "gw-btn-glowny", "✓ Gotowe — pokaż podgląd");
    btnGotoweMapa.type = "button";
    btnGotoweMapa.addEventListener("click", function () {
      przelaczZakladkeMiksera("podglad");
    });
    akcjeMapyDol.appendChild(btnGotoweMapa);

    let btnDopasujPodglad = null;
    if (czyPolaczonoZProgramem()) {
      btnDopasujPodglad = el("button", "gw-btn-prosty", "🔍 Dopasuj podgląd do obszaru");
      btnDopasujPodglad.type = "button";
      btnDopasujPodglad.hidden = true;
      btnDopasujPodglad.title = "Ustawia skalę próbki tak, aby cały mały zaznaczony obszar zmieścił się w podglądzie";
      btnDopasujPodglad.addEventListener("click", function () {
        if (!Array.isArray(stan.zaznaczonyBbox)) return;
        const px = Math.min(MAKS_PIKSEL_PODGLADU, obliczPikselDlaBbox(stan.zaznaczonyBbox, 360));
        stan.podgladPiksel = px;
        stan.ostatniPikselRenderu = px;
        zsynchronizujSelectSkali(px);
        przelaczZakladkeMiksera("podglad");
        zglosOdswiezeniePodgladuMiksera(true);
      });
      akcjeMapyDol.appendChild(btnDopasujPodglad);
    }

    minimapaBox.appendChild(minimapaNarzedzia);
    minimapaBox.appendChild(minimapaWrap);
    minimapaBox.appendChild(minimapaInfo);
    minimapaBox.appendChild(skrotyBox);
    minimapaBox.appendChild(akcjeMapyDol);

    function przelaczZakladkeMiksera(nowa) {
      stan.zakladkaMiksera = nowa === "mapa" ? "mapa" : "podglad";
      const czyMapa = stan.zakladkaMiksera === "mapa";
      tabPodglad.classList.toggle("aktywny", !czyMapa);
      tabPodglad.setAttribute("aria-selected", czyMapa ? "false" : "true");
      tabMapa.classList.toggle("aktywny", czyMapa);
      tabMapa.setAttribute("aria-selected", czyMapa ? "true" : "false");
      panelPodglad.hidden = czyMapa;
      minimapaBox.hidden = !czyMapa;
      if (czyMapa) rysujMinimape();
    }

    tabPodglad.addEventListener("click", function () { przelaczZakladkeMiksera("podglad"); });
    tabMapa.addEventListener("click", function () { przelaczZakladkeMiksera("mapa"); });

    kolPodglad.appendChild(mikserTabs);
    kolPodglad.appendChild(panelPodglad);
    kolPodglad.appendChild(minimapaBox);

    // Prawa kolumna: warstwy, suwaki i eksport
    const kolLista = el("div", "gw-mikser-kol-lista");
    const pasekListy = el("div", "gw-mikser-pasek-listy");
    pasekListy.appendChild(el("span", null, "Kolejność warstw (góra zasłania dół, spód = podkład 1.):"));

    const przyciskiListy = el("div", "gw-modal-linki");
    const btnAuto = el("button", "gw-btn-prosty", "⚡ Ułóż automatycznie");
    btnAuto.type = "button";
    btnAuto.title = "Przesuwa podkłady na spód, nakładki na górę i włącza tryb Rzeźba terenu dla cieniowania";
    btnAuto.addEventListener("click", function () {
      stan.kanapka = ulozKanapkeAutomatycznie(stan.kanapka);
      poZmianieKanapki();
    });

    const btnWyczysc = el("button", "gw-btn-prosty", "↺ Resetuj miks");
    btnWyczysc.type = "button";
    btnWyczysc.title = "Usuwa warstwy, nazwę stylu, zaznaczony obszar i wybraną skalę";
    btnWyczysc.addEventListener("click", resetujMikser);

    przyciskiListy.appendChild(btnAuto);
    if (czyPolaczonoZProgramem()) {
      const btnWczytajZProg = el("button", "gw-btn-prosty", "↺ Z programu");
      btnWczytajZProg.type = "button";
      btnWczytajZProg.title = "Zastępuje listę aktualną kanapką z okna MapoTero";
      btnWczytajZProg.addEventListener("click", function () {
        pobierzStanZMapoTero(false, true).then(function (dane) {
          if (!dane || !dane.zestaw || !dane.zestaw.warstwy || dane.zestaw.warstwy.length === 0) {
            pokazStatusMiksera("W oknie MapoTero nie ma obecnie wybranych warstw.", true);
          } else {
            pokazStatusMiksera("Wczytano bieżące warstwy z MapoTero.", false);
          }
        });
      });
      przyciskiListy.appendChild(btnWczytajZProg);
    }
    przyciskiListy.appendChild(btnWyczysc);
    pasekListy.appendChild(przyciskiListy);

    const mikserWarstwy = el("div", "gw-mikser-warstwy");
    const mikserOstrzezenia = el("div", "gw-mikser-ostrzezenia");
    mikserOstrzezenia.hidden = true;

    const sekcjaEksport = el("div", "gw-mikser-eksport");
    const nazwaWiersz = el("div", "gw-mikser-nazwa-wiersz");
    nazwaWiersz.appendChild(el("label", null, "Nazwa stylu / pliku:"));
    const inputNazwa = document.createElement("input");
    inputNazwa.type = "text";
    inputNazwa.className = "gw-mikser-nazwa-input";
    inputNazwa.placeholder = "np. Orto + rzeźba + szlaki (opcjonalnie)";
    inputNazwa.value = stan.nazwaKanapki;
    inputNazwa.addEventListener("input", function () {
      stan.nazwaKanapki = inputNazwa.value;
      zapiszKanapkeLocalStorage();
      odswiezDokKanapki();
    });
    nazwaWiersz.appendChild(inputNazwa);

    const akcjeGlowne = el("div", "gw-mikser-akcje-glowne");
    const opcjeWysylki = el("div", "gw-mikser-opcje-wiersz");
    let btnWyslijGlowne = null;
    let btnWyslijKreator = null;
    let chkStyl = null;
    if (czyPolaczonoZProgramem()) {
      btnWyslijGlowne = el("button", "gw-btn-zielony", "▶ Wyślij do MapoTero");
      btnWyslijGlowne.type = "button";
      btnWyslijGlowne.title = "Ustawia ten miks warstw w głównym oknie programu MapoTero";
      btnWyslijGlowne.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("okno_glowne", stan.zapiszJakoStyl);
      });

      btnWyslijKreator = el("button", "gw-btn-glowny", "✨ Otwórz w Szybkim Kreatorze");
      btnWyslijKreator.type = "button";
      btnWyslijKreator.title = "Przekazuje miks jako własny styl do Szybkiego Kreatora Mapy";
      btnWyslijKreator.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("kreator", true);
      });

      akcjeGlowne.appendChild(btnWyslijGlowne);
      akcjeGlowne.appendChild(btnWyslijKreator);

      chkStyl = document.createElement("input");
      chkStyl.type = "checkbox";
      chkStyl.checked = stan.zapiszJakoStyl;
      chkStyl.addEventListener("change", function () {
        stan.zapiszJakoStyl = chkStyl.checked;
      });
      const lblChk = el("label", "gw-mikser-opcja-chk", " zapisz też jako własny styl kreatora");
      lblChk.prepend(chkStyl);
      opcjeWysylki.appendChild(lblChk);
    }

    const chkObszar = document.createElement("input");
    chkObszar.type = "checkbox";
    chkObszar.checked = stan.przekazObszarDoMapoTero;
    chkObszar.addEventListener("change", function () {
      stan.przekazObszarDoMapoTero = chkObszar.checked;
    });
    const chkObszarWrap = el("label", "gw-mikser-opcja-chk", " ustaw zaznaczony obszar w MapoTero");
    chkObszarWrap.hidden = !stan.zaznaczonyBbox;
    chkObszarWrap.prepend(chkObszar);
    if (czyPolaczonoZProgramem()) {
      opcjeWysylki.appendChild(chkObszarWrap);
    }

    const akcjeZapasowe = el("div", "gw-mikser-akcje-zapasowe");
    const btnPobierzPlik = el("button", czyPolaczonoZProgramem() ? "gw-btn-prosty" : "gw-btn-zielony", "💾 Pobierz plik (.mapotero)");
    btnPobierzPlik.type = "button";
    btnPobierzPlik.title = "Plik możesz przeciągnąć bezpośrednio na okno MapoTero";
    btnPobierzPlik.addEventListener("click", function () {
      const przepis = budujPrzepisJson(stan.kanapka, {
        nazwa: stan.nazwaKanapki,
        cel: "okno_glowne",
        zapiszStyl: stan.zapiszJakoStyl,
        obszarWgs84: stan.zaznaczonyBbox
      });
      const blob = new Blob([JSON.stringify(przepis, null, 2)], { type: "application/json;charset=utf-8" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      const nazwaPliku = (stan.nazwaKanapki || "kanapka_mapotero")
        .trim()
        .toLowerCase()
        .replace(/[^a-z0-9_-]+/g, "_")
        .replace(/^_+|_+$/g, "") || "kanapka_mapotero";
      a.href = url;
      a.download = nazwaPliku + ".mapotero";
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      pokazStatusMiksera("Zapisano plik " + a.download + " — przeciągnij go na okno MapoTero lub wczytaj z menu listy warstw.", false);
    });

    const btnKopiujLink = el("button", "gw-btn-prosty", "🔗 Kopiuj link");
    btnKopiujLink.type = "button";
    btnKopiujLink.addEventListener("click", function () {
      const link = zbudujLinkUdostepniania();
      kopiujTekstDoSchowka(link)
        .then(function () {
          pokazStatusMiksera("Skopiowano link do udostępnienia tego miksu.", false);
        })
        .catch(function () {
          pokazStatusMiksera("Nie udało się automatycznie skopiować linku do schowka.", true);
        });
    });

    const btnKopiujJson = el("button", "gw-btn-prosty", "📋 Kopiuj JSON");
    btnKopiujJson.type = "button";
    btnKopiujJson.addEventListener("click", function () {
      const przepis = budujPrzepisJson(stan.kanapka, {
        nazwa: stan.nazwaKanapki,
        cel: "okno_glowne",
        zapiszStyl: stan.zapiszJakoStyl,
        obszarWgs84: stan.przekazObszarDoMapoTero && stan.zaznaczonyBbox ? stan.zaznaczonyBbox : null
      });
      kopiujTekstDoSchowka(JSON.stringify(przepis, null, 2))
        .then(function () {
          pokazStatusMiksera("Skopiowano przepis JSON do schowka — kliknij prawym na liście warstw w MapoTero i wybierz „Wklej kanapkę ze schowka”.", false);
        })
        .catch(function () {
          pokazStatusMiksera("Nie udało się automatycznie skopiować przepisu JSON — użyj przycisku „Pobierz plik (.mapotero)”.", true);
        });
    });

    akcjeZapasowe.appendChild(btnPobierzPlik);
    akcjeZapasowe.appendChild(btnKopiujLink);
    akcjeZapasowe.appendChild(btnKopiujJson);

    const statusWysylki = el("div", "gw-mikser-status-wysylki", "");
    statusWysylki.hidden = true;

    sekcjaEksport.appendChild(nazwaWiersz);
    if (akcjeGlowne.childNodes.length > 0) sekcjaEksport.appendChild(akcjeGlowne);
    if (opcjeWysylki.childNodes.length > 0) sekcjaEksport.appendChild(opcjeWysylki);
    if (czyPolaczonoZProgramem()) {
      const wiecejEksport = document.createElement("details");
      wiecejEksport.className = "gw-mikser-wiecej";
      const sumEksport = document.createElement("summary");
      sumEksport.textContent = "Plik .mapotero, link i udostępnianie";
      wiecejEksport.appendChild(sumEksport);
      wiecejEksport.appendChild(akcjeZapasowe);
      sekcjaEksport.appendChild(wiecejEksport);
    } else {
      sekcjaEksport.appendChild(akcjeZapasowe);
      sekcjaEksport.appendChild(
        el("p", "gw-mikser-podpowiedz-offline", "Przeciągnij pobrany plik .mapotero bezpośrednio na okno programu MapoTero.")
      );
    }
    sekcjaEksport.appendChild(statusWysylki);

    kolLista.appendChild(pasekListy);
    kolLista.appendChild(mikserWarstwy);
    kolLista.appendChild(mikserOstrzezenia);
    kolLista.appendChild(sekcjaEksport);

    mikserCialo.appendChild(kolPodglad);
    mikserCialo.appendChild(kolLista);
    mikserModal.appendChild(mikserNaglowek);
    if (!czyPolaczonoZProgramem()) {
      const wskazowkaPolaczenia = el("div", "gw-mikser-wskazowka-polaczenie");
      wskazowkaPolaczenia.innerHTML =
        "💡 <strong>Wskazówka:</strong> uruchom <strong>MapoTero</strong> i otwórz Galerię warstw z okna programu (ikona siatki obok wyszukiwarki warstw) — odblokujesz wtedy <strong>podgląd na żywo dla dowolnego zaznaczonego obszaru</strong> oraz <strong>wysyłanie zestawu 1 klikiem</strong> prosto do programu.";
      mikserModal.appendChild(wskazowkaPolaczenia);
    }
    mikserModal.appendChild(mikserCialo);
    mikserTlo.appendChild(mikserModal);

    return {
      mikserDok: mikserDok,
      mikserDokLicznik: dokLicznik,
      mikserDokOpis: dokOpis,
      mikserDokOtworz: dokOtworz,
      mikserDokWyslij: dokWyslij,
      mikserDokReset: dokReset,
      mikserDokZwin: dokZwin,
      mikserTlo: mikserTlo,
      mikserZamknij: mikserZamknij,
      mikserInputNazwa: inputNazwa,
      mikserSelSkala: selSkala,
      mikserChkStyl: chkStyl,
      mikserCanvas: mikserCanvas,
      mikserImgLive: mikserImgLive,
      mikserLadowanie: mikserLadowanie,
      mikserStanPodgladu: mikserStanPodgladu,
      mikserZakladkaMapa: tabMapa,
      przelaczZakladkeMiksera: przelaczZakladkeMiksera,
      minimapaCanvas: minimapaCanvas,
      minimapaZoomTekst: zoomTekst,
      minimapaInfo: minimapaInfo,
      minimapaBtnWyczyscZazn: btnWyczyscZazn,
      minimapaBtnDopasujPodglad: btnDopasujPodglad,
      mikserChkObszarWrap: chkObszarWrap,
      mikserWarstwy: mikserWarstwy,
      mikserOstrzezenia: mikserOstrzezenia,
      mikserBtnAuto: btnAuto,
      mikserBtnWyczysc: btnWyczysc,
      mikserBtnWyslijGlowne: btnWyslijGlowne,
      mikserBtnWyslijKreator: btnWyslijKreator,
      mikserBtnPobierzPlik: btnPobierzPlik,
      mikserBtnKopiujLink: btnKopiujLink,
      mikserBtnKopiujJson: btnKopiujJson,
      mikserStatusWysylki: statusWysylki
    };
  }

  function zbudujInterfejs() {
    kontener.innerHTML = "";

    const panel = el("div", "gw-panel-filtrow");
    const wierszGorny = el("div", "gw-wiersz-gorny");

    const szukaj = document.createElement("input");
    szukaj.type = "search";
    szukaj.className = "gw-szukaj";
    szukaj.placeholder = "Szukaj warstwy, zbioru lub źródła (np. orto, BDOT, WIG)…";
    szukaj.value = stan.szukaj;
    szukaj.setAttribute("aria-label", "Szukaj warstwy");
    szukaj.addEventListener("input", function () {
      stan.szukaj = szukaj.value;
      renderujSiatke();
    });

    const selectGrupa = document.createElement("select");
    selectGrupa.className = "gw-select-grupa";
    selectGrupa.setAttribute("aria-label", "Filtruj według grupy tematycznej");
    const opcjaWszystkie = document.createElement("option");
    opcjaWszystkie.value = "WSZYSTKIE";
    opcjaWszystkie.textContent = "Wszystkie grupy tematyczne";
    selectGrupa.appendChild(opcjaWszystkie);

    const unikalneGrupy = [];
    stan.warstwy.forEach(function (w) {
      if (w.grupa && !unikalneGrupy.includes(w.grupa)) unikalneGrupy.push(w.grupa);
    });
    unikalneGrupy.forEach(function (g) {
      const opt = document.createElement("option");
      opt.value = g;
      opt.textContent = g;
      if (g === stan.grupa) opt.selected = true;
      selectGrupa.appendChild(opt);
    });
    selectGrupa.addEventListener("change", function () {
      stan.grupa = selectGrupa.value;
      renderujSiatke();
    });

    wierszGorny.appendChild(szukaj);
    wierszGorny.appendChild(selectGrupa);
    panel.appendChild(wierszGorny);

    const wierszDolny = el("div", "gw-wiersz-dolny");
    const pigulkiKontener = el("div", "gw-grupy-pigulek");

    const grTyp = utworzGrupePigulek(
      "Typ:",
      [
        { wartosc: "WSZYSTKIE", tekst: "Wszystkie" },
        { wartosc: "WMS", tekst: "WMS" },
        { wartosc: "WMTS", tekst: "WMTS" },
        { wartosc: "XYZ", tekst: "XYZ" }
      ],
      function () { return stan.typ; },
      function (val) { stan.typ = val; }
    );

    const grRola = utworzGrupePigulek(
      "Rola:",
      [
        { wartosc: "WSZYSTKIE", tekst: "Wszystkie" },
        { wartosc: "PODKLAD", tekst: "Podkład" },
        { wartosc: "NAKLADKA", tekst: "Nakładka" }
      ],
      function () { return stan.rola; },
      function (val) { stan.rola = val; }
    );

    const grWidok = utworzGrupePigulek(
      "Karty:",
      [
        { wartosc: "STANDARD", tekst: "Standardowe" },
        { wartosc: "DUZE", tekst: "Większe" }
      ],
      function () { return stan.rozmiar; },
      function (val) { stan.rozmiar = val; }
    );

    pigulkiKontener.appendChild(grTyp.element);
    pigulkiKontener.appendChild(grRola.element);
    pigulkiKontener.appendChild(grWidok.element);

    const prawaNarzedzia = el("div", "gw-prawa-narzedzia");
    const btnReset = el("button", "gw-btn-reset", "Wyczyść filtry");
    btnReset.type = "button";
    btnReset.hidden = true;
    btnReset.addEventListener("click", function () {
      stan.szukaj = "";
      stan.grupa = "WSZYSTKIE";
      stan.typ = "WSZYSTKIE";
      stan.rola = "WSZYSTKIE";
      szukaj.value = "";
      selectGrupa.value = "WSZYSTKIE";
      grTyp.odswiez();
      grRola.odswiez();
      renderujSiatke();
    });

    const licznik = el("p", "gw-licznik");
    prawaNarzedzia.appendChild(btnReset);
    prawaNarzedzia.appendChild(licznik);

    wierszDolny.appendChild(pigulkiKontener);
    wierszDolny.appendChild(prawaNarzedzia);
    panel.appendChild(wierszDolny);

    const wyniki = el("div");

    // Modal pojedynczej warstwy
    const modalTlo = el("div", "gw-modal-tlo");
    modalTlo.hidden = true;
    modalTlo.setAttribute("role", "dialog");
    modalTlo.setAttribute("aria-modal", "true");
    modalTlo.addEventListener("click", function (e) {
      if (e.target === modalTlo) zamknijModal();
    });

    const modal = el("div", "gw-modal");
    const modalObrazKol = el("div", "gw-modal-obraz-kol");

    const modalTabs = el("div", "gw-modal-tabs");
    modalTabs.setAttribute("role", "tablist");
    modalTabs.setAttribute("aria-label", "Widok podglądu");

    const tabProbka = el("button", "gw-modal-tab aktywny", "🖼️ Próbka mapy");
    tabProbka.type = "button";
    tabProbka.setAttribute("role", "tab");
    tabProbka.setAttribute("aria-selected", "true");

    const tabZasieg = el("button", "gw-modal-tab", "🗺️ Zasięg na mapie");
    tabZasieg.type = "button";
    tabZasieg.setAttribute("role", "tab");
    tabZasieg.setAttribute("aria-selected", "false");

    modalTabs.appendChild(tabProbka);
    modalTabs.appendChild(tabZasieg);

    const modalPanelProbka = el("div", "gw-modal-panel-probka");
    const modalPanelZasieg = el("div", "gw-modal-panel-zasieg");
    modalPanelZasieg.hidden = true;

    function przelaczWidokModala(nowyWidok) {
      stan.widokModala = nowyWidok;
      if (nowyWidok === "zasieg") {
        tabProbka.classList.remove("aktywny");
        tabProbka.setAttribute("aria-selected", "false");
        tabZasieg.classList.add("aktywny");
        tabZasieg.setAttribute("aria-selected", "true");
        modalPanelProbka.hidden = true;
        modalPanelProbka.style.display = "none";
        modalPanelZasieg.hidden = false;
        modalPanelZasieg.style.display = "flex";
      } else {
        tabZasieg.classList.remove("aktywny");
        tabZasieg.setAttribute("aria-selected", "false");
        tabProbka.classList.add("aktywny");
        tabProbka.setAttribute("aria-selected", "true");
        modalPanelZasieg.hidden = true;
        modalPanelZasieg.style.display = "none";
        modalPanelProbka.hidden = false;
        modalPanelProbka.style.display = "flex";
      }
    }

    tabProbka.addEventListener("click", function () { przelaczWidokModala("probka"); });
    tabZasieg.addEventListener("click", function () { przelaczWidokModala("zasieg"); });

    modalObrazKol.appendChild(modalTabs);
    modalObrazKol.appendChild(modalPanelProbka);
    modalObrazKol.appendChild(modalPanelZasieg);

    const modalInfoKol = el("div", "gw-modal-info-kol");
    const modalNaglowek = el("div", "gw-modal-naglowek");
    const modalTytul = el("h3", "gw-modal-tytul");
    const modalZamknij = el("button", "gw-modal-zamknij", "×");
    modalZamknij.type = "button";
    modalZamknij.setAttribute("aria-label", "Zamknij podgląd");
    modalZamknij.addEventListener("click", zamknijModal);
    modalNaglowek.appendChild(modalTytul);
    modalNaglowek.appendChild(modalZamknij);

    const modalSciezka = el("div", "gw-modal-sciezka");
    const modalTabela = document.createElement("table");
    modalTabela.className = "gw-modal-tabela";

    const modalAkcje = el("div", "gw-modal-akcje");
    const modalLinki = el("div", "gw-modal-linki");

    const modalKanapka = el("button", "gw-btn-kanapka", "☐ Dodaj do miksu");
    modalKanapka.type = "button";
    modalKanapka.title = "Dodaj warstwę do kompozycji w Mikserze warstw";
    modalKanapka.addEventListener("click", function () {
      const w = stan.widoczne[stan.indeksModala];
      if (w) przelaczWarstweWKanapce(w);
    });
    modalLinki.appendChild(modalKanapka);

    const modalOtworzMikser = el("button", "gw-btn-prosty", "🧩 Mikser");
    modalOtworzMikser.type = "button";
    modalOtworzMikser.hidden = stan.kanapka.length === 0;
    modalOtworzMikser.title = "Przejdź bezpośrednio do Miksera warstw, aby ułożyć kolejność i krycie";
    modalOtworzMikser.addEventListener("click", function () {
      zamknijModal();
      otworzMikser();
    });
    modalLinki.appendChild(modalOtworzMikser);

    let modalDodaj = null;
    if (czyPolaczonoZProgramem()) {
      modalDodaj = el("button", "gw-btn-dodaj-modal", "Wyślij");
      modalDodaj.type = "button";
      modalDodaj.title = "Pomiń mikser i dodaj od razu tylko tę pojedynczą warstwę do MapoTero";
      modalDodaj.addEventListener("click", function () {
        const w = stan.widoczne[stan.indeksModala];
        if (w) wyslijDoMapoTero(w, modalDodaj);
      });
      modalLinki.appendChild(modalDodaj);
    }

    const modalKopiuj = el("button", "gw-btn-prosty", "Kopiuj nazwę");
    modalKopiuj.type = "button";
    modalKopiuj.addEventListener("click", function () {
      const w = stan.widoczne[stan.indeksModala];
      if (!w) return;
      kopiujTekstDoSchowka(w.nazwa)
        .then(function () {
          modalKopiuj.textContent = "Skopiowano!";
          setTimeout(function () {
            modalKopiuj.textContent = "Kopiuj nazwę";
          }, 1400);
        })
        .catch(function () {
          modalKopiuj.textContent = "Błąd schowka";
          setTimeout(function () {
            modalKopiuj.textContent = "Kopiuj nazwę";
          }, 1400);
        });
    });

    const linkKatalog = el("a", "gw-btn-prosty", "Katalog zbiorów");
    linkKatalog.href = zbioryUrl;
    modalLinki.appendChild(modalKopiuj);
    modalLinki.appendChild(linkKatalog);

    const nawigacja = el("div", "gw-modal-nawigacja");
    const modalPoprz = el("button", "gw-btn-prosty", "← Poprzednia");
    modalPoprz.type = "button";
    modalPoprz.addEventListener("click", function () {
      otworzModal(stan.indeksModala - 1);
    });
    const modalNast = el("button", "gw-btn-prosty", "Następna →");
    modalNast.type = "button";
    modalNast.addEventListener("click", function () {
      otworzModal(stan.indeksModala + 1);
    });
    nawigacja.appendChild(modalPoprz);
    nawigacja.appendChild(modalNast);

    modalAkcje.appendChild(modalLinki);
    modalAkcje.appendChild(nawigacja);

    modalInfoKol.appendChild(modalNaglowek);
    modalInfoKol.appendChild(modalSciezka);
    modalInfoKol.appendChild(modalTabela);
    modalInfoKol.appendChild(modalAkcje);

    modal.appendChild(modalObrazKol);
    modal.appendChild(modalInfoKol);
    modalTlo.appendChild(modal);

    const mikserUi = zbudujMikserUi();

    document.addEventListener("keydown", function (e) {
      if (stan.mikserOtwarty && e.key === "Escape") {
        zamknijMikser();
        return;
      }
      if (modalTlo.hidden) return;
      if (e.key === "Escape") zamknijModal();
      else if (e.key === "ArrowLeft" && stan.indeksModala > 0) otworzModal(stan.indeksModala - 1);
      else if (e.key === "ArrowRight" && stan.indeksModala < stan.widoczne.length - 1) otworzModal(stan.indeksModala + 1);
    });

    let pasekMapoTero = null;
    if (czyPolaczonoZProgramem()) {
      pasekMapoTero = el(
        "div",
        "gw-pasek-mapotero",
        "Łączenie z działającym programem MapoTero…"
      );
      kontener.appendChild(pasekMapoTero);
      fetch("http://127.0.0.1:" + stan.port + "/ping?token=" + encodeURIComponent(stan.token))
        .then(function (r) {
          if (!r.ok) throw new Error();
          return r.json().catch(function () { return { ok: true }; });
        })
        .then(function (pingDane) {
          stan.funkcjeProgramu = Array.isArray(pingDane.funkcje) ? pingDane.funkcje : ["dodaj"];
          stan.wersjaProgramu = pingDane.wersja || "";
          pasekMapoTero.classList.remove("gw-pasek-blad");
          pasekMapoTero.textContent =
            "Połączono z MapoTero" +
            (stan.wersjaProgramu ? " " + stan.wersjaProgramu : "") +
            " — dodawaj warstwy przyciskiem „Dodaj do miksu” i ułóż je w Mikserze warstw, albo kliknij „Wyślij”, aby dodać warstwę pojedynczo.";
          return pobierzStanZMapoTero(!stan.kanapkaUrlStart, false);
        })
        .catch(function () {
          pasekMapoTero.classList.add("gw-pasek-blad");
          pasekMapoTero.textContent =
            "Brak odpowiedzi z MapoTero — upewnij się, że program nadal jest uruchomiony i że w wyskakującym oknie przeglądarki wyraziłeś zgodę na dostęp do sieci lokalnej (127.0.0.1).";
        });
    }

    kontener.appendChild(panel);
    kontener.appendChild(wyniki);
    kontener.appendChild(mikserUi.mikserDok);
    kontener.appendChild(modalTlo);
    kontener.appendChild(mikserUi.mikserTlo);

    if (stan.dane && stan.dane.wygenerowano) {
      kontener.appendChild(
        el(
          "p",
          "gw-stopka-danych",
          "Stan spisu warstw: " +
            stan.dane.wygenerowano +
            (stan.dane.wersja_programu ? " (MapoTero " + stan.dane.wersja_programu + ")" : "")
        )
      );
    }

    ref = Object.assign({
      pasekMapoTero: pasekMapoTero,
      licznik: licznik,
      btnReset: btnReset,
      wyniki: wyniki,
      modalTlo: modalTlo,
      modalObrazKol: modalObrazKol,
      modalPanelProbka: modalPanelProbka,
      modalPanelZasieg: modalPanelZasieg,
      tabProbka: tabProbka,
      tabZasieg: tabZasieg,
      przelaczWidokModala: przelaczWidokModala,
      modalTytul: modalTytul,
      modalSciezka: modalSciezka,
      modalTabela: modalTabela,
      modalKanapka: modalKanapka,
      modalOtworzMikser: modalOtworzMikser,
      modalDodaj: modalDodaj,
      modalKopiuj: modalKopiuj,
      modalPoprz: modalPoprz,
      modalNast: modalNast,
      modalZamknij: modalZamknij
    }, mikserUi);

    renderujSiatke();
    odswiezDokKanapki();
    if (stan.mikserOtwarty) {
      otworzMikser();
    }
  }

  wczytajParametryUrl();

  fetch(jsonUrl)
    .then(function (resp) {
      if (!resp.ok) throw new Error("HTTP " + resp.status);
      return resp.json();
    })
    .then(function (dane) {
      stan.dane = dane;
      stan.warstwy = Array.isArray(dane.warstwy) ? dane.warstwy : [];
      wczytajKanapkePoczatkowa();
      zbudujInterfejs();
    })
    .catch(function () {
      kontener.innerHTML = "";
      kontener.appendChild(
        el(
          "p",
          "gw-brak-wynikow",
          "Nie udało się wczytać spisu warstw (warstwy.json). Spróbuj odświeżyć stronę."
        )
      );
    });
})(typeof window !== "undefined" ? window : globalThis);
