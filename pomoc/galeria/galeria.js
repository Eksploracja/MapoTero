(function (global) {
  "use strict";

  const MAKS_WARSTW_KANAPKI = 12;
  const MIN_KRYCIE = 5;
  const PELNE_KRYCIE = 100;
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
    return {
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

  if (typeof module !== "undefined" && module.exports) {
    module.exports = {
      MAKS_WARSTW_KANAPKI,
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
      kafelekDoLatLon
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
    minimapaOtwarta: false,
    minimapaBazaKafelki: null,
    abortPodglad: null,
    timerPodgladu: null,
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

  function wczytajParametryUrl() {
    const params = new URLSearchParams(window.location.search);
    if (params.has("q")) stan.szukaj = params.get("q") || "";
    if (params.has("grupa")) stan.grupa = params.get("grupa") || "WSZYSTKIE";
    if (params.has("typ")) stan.typ = (params.get("typ") || "WSZYSTKIE").toUpperCase();
    if (params.has("rola")) stan.rola = params.get("rola") || "WSZYSTKIE";
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
      if (!poz.nakladka && stan.kanapka.length > 0) {
        // Nowy podkład wsuwamy pod końcowe nakładki, tak jak w MapoTero
        let wstaw = stan.kanapka.length;
        while (wstaw > 0 && stan.kanapka[wstaw - 1].nakladka) wstaw--;
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
          }
          if (stan.podgladPiksel === 0 && dane.obszar.piksel > 0) {
            stan.podgladPiksel = 0; // Auto wg wybranej kanapki lub obszaru
          }
        }
        if (dane.zestaw && Array.isArray(dane.zestaw.warstwy) && dane.zestaw.warstwy.length > 0) {
          if (wymusNadpisanieKanapki || (nadpiszKanapkeJesliPusta && stan.kanapka.length === 0)) {
            stan.kanapka = zmapujWarstwyZPrzepisu(dane.zestaw.warstwy);
            if (dane.zestaw.nazwa) stan.nazwaKanapki = dane.zestaw.nazwa;
            poZmianieKanapki();
          }
        }
        if (stan.mikserOtwarty) {
          odswiezMinimape();
          zglosOdswiezeniePodgladuMiksera();
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
      zapiszStyl: Boolean(zapiszStyl)
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
        tekstOdznaki = "Region";
        klasaOdznaki += " gw-zasieg-region";
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
      if (idx >= 0) {
        karta.classList.add("gw-karta-w-kanapce");
        if (btn) {
          btn.classList.add("aktywny");
          btn.textContent = "✓ W kanapce (#" + (idx + 1) + ")";
        }
      } else {
        karta.classList.remove("gw-karta-w-kanapce");
        if (btn) {
          btn.classList.remove("aktywny");
          btn.textContent = "☐ Do kanapki";
        }
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
        idxWKanapce >= 0 ? "✓ W kanapce (#" + (idxWKanapce + 1) + ")" : "☐ Do kanapki"
      );
      btnKanapka.type = "button";
      btnKanapka.title = "Dodaj lub usuń warstwę z miksera kanapki";
      btnKanapka.addEventListener("click", function (e) {
        e.stopPropagation();
        przelaczWarstweWKanapce(w);
      });
      akcje.appendChild(btnKanapka);

      if (czyPolaczonoZProgramem()) {
        const btnDodaj = el("button", "gw-btn-dodaj", "+ MapoTero");
        btnDodaj.type = "button";
        btnDodaj.title = "Dodaj od razu jako pojedynczą warstwę do MapoTero";
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
      naglowek.appendChild(el("span", "gw-liczba-w-grupie", elementy.length + " warstw"));
      sekcja.appendChild(naglowek);

      const podgrupy = new Map();
      elementy.forEach(function (pozycja) {
        const kluczPod = pozycja.warstwa.podgrupa || "";
        if (!podgrupy.has(kluczPod)) podgrupy.set(kluczPod, []);
        podgrupy.get(kluczPod).push(pozycja);
      });

      if (podgrupy.has("")) {
        sekcja.appendChild(utworzKartySiatki(podgrupy.get("")));
      }

      podgrupy.forEach(function (podElementy, nazwaPodgrupy) {
        if (!nazwaPodgrupy) return;
        sekcja.appendChild(el("h3", "gw-podgrupa-tytul", nazwaPodgrupy));
        sekcja.appendChild(utworzKartySiatki(podElementy));
      });

      ref.wyniki.appendChild(sekcja);
    });
  }

  const MAPA_GEOM = {"scale": 30.9524, "offX": 42.46, "offY": 10.0, "cosLat0": 0.61291, "lonMin": 11.0, "latMax": 56.4, "paths": {"PL": "M 104.1,86.5 L 108.9,88.9 L 110.8,84.3 L 128.8,78.7 L 140.7,71.3 L 153.4,65.7 L 166.7,59.5 L 180.9,58.3 L 190.4,60.1 L 186.6,65.7 L 184.7,68.8 L 188.2,72.8 L 194.2,73.5 L 206.6,70.7 L 210.4,70.4 L 222.7,73.1 L 241.7,74.1 L 266.3,73.1 L 277.7,78.1 L 281.5,85.8 L 288.1,90.5 L 283.4,106.0 L 287.2,121.4 L 281.5,140.0 L 279.6,161.7 L 291.9,181.2 L 289.7,194.2 L 279.6,200.4 L 270.1,205.0 L 264.4,220.5 L 265.4,228.2 L 267.3,239.0 L 260.6,234.4 L 247.3,229.8 L 237.9,225.1 L 226.5,226.7 L 213.2,232.9 L 209.4,232.2 L 199.9,223.6 L 193.3,222.0 L 191.0,223.0 L 184.7,212.7 L 180.9,208.1 L 178.1,205.0 L 170.5,200.4 L 165.8,200.4 L 160.1,197.3 L 154.4,194.2 L 144.9,201.9 L 141.1,194.2 L 135.4,186.4 L 127.8,183.3 L 124.0,181.8 L 118.3,181.2 L 114.9,177.8 L 118.0,169.4 L 117.4,158.6 L 113.6,147.7 L 110.8,143.1 L 109.8,133.8 L 107.9,124.5 L 101.7,120.2 L 106.0,107.5 L 108.9,102.9 L 105.1,96.7 L 104.1,86.5 Z", "BALTYK": "M 42.5,10.0 L 317.5,10.0 L 317.5,68.8 L 266.3,73.1 L 222.7,73.1 L 210.4,70.4 L 194.2,73.5 L 188.2,72.8 L 184.7,68.8 L 186.6,65.7 L 190.4,60.1 L 180.9,58.3 L 166.7,59.5 L 153.4,65.7 L 140.7,71.3 L 128.8,78.7 L 110.8,84.3 L 104.1,86.5 L 42.5,84.3 Z", "CZ": "M 114.9,177.8 L 118.3,181.2 L 124.0,181.8 L 127.8,183.3 L 135.4,186.4 L 141.1,194.2 L 144.9,201.9 L 154.4,194.2 L 160.1,197.3 L 165.8,200.4 L 170.5,200.4 L 178.1,205.0 L 180.9,208.1 L 184.7,212.7 L 191.0,223.0 L 186.6,232.9 L 165.8,245.2 L 154.4,251.4 L 118.3,251.4 L 99.4,251.4 L 80.4,239.0 L 63.3,205.0 L 67.1,198.8 L 80.4,192.6 L 103.2,183.3 L 114.9,177.8 Z", "SK": "M 191.0,223.0 L 193.3,222.0 L 199.9,223.6 L 209.4,232.2 L 213.2,232.9 L 226.5,226.7 L 237.9,225.1 L 247.3,229.8 L 260.6,234.4 L 267.3,239.0 L 260.6,257.6 L 241.7,257.6 L 194.2,266.9 L 165.8,270.0 L 154.4,251.4 L 165.8,245.2 L 186.6,232.9 L 191.0,223.0 Z", "DE_BB": "M 101.7,120.2 L 107.9,124.5 L 109.8,133.8 L 110.8,143.1 L 113.6,147.7 L 117.4,158.6 L 108.9,164.8 L 89.9,164.8 L 80.4,155.5 L 65.2,146.2 L 57.6,121.4 L 57.6,109.0 L 76.6,99.8 L 105.1,96.7 L 108.9,102.9 L 106.0,107.5 L 101.7,120.2 Z", "DE_SN": "M 117.4,158.6 L 118.0,169.4 L 114.9,177.8 L 103.2,183.3 L 80.4,192.6 L 67.1,198.8 L 63.3,205.0 L 65.2,183.3 L 67.1,167.9 L 80.4,158.6 L 89.9,164.8 L 108.9,164.8 L 117.4,158.6 Z", "DE_REST": "M 42.5,84.3 L 104.1,86.5 L 105.1,96.7 L 76.6,99.8 L 57.6,109.0 L 57.6,121.4 L 65.2,146.2 L 80.4,155.5 L 89.9,164.8 L 80.4,158.6 L 67.1,167.9 L 65.2,183.3 L 63.3,205.0 L 42.5,201.9 L 42.5,84.3 Z", "LT": "M 266.3,73.1 L 277.7,78.1 L 281.5,85.8 L 317.5,84.3 L 317.5,10.0 L 232.2,10.0 L 230.3,44.0 L 266.3,73.1 Z", "WISLA": "M 194.2,220.5 L 198.0,211.2 L 211.3,206.5 L 224.6,201.9 L 247.3,186.4 L 249.2,164.8 L 236.0,143.1 L 232.2,136.9 L 220.8,133.8 L 194.2,127.6 L 186.6,112.1 L 184.7,99.8 L 188.5,75.0 L 193.3,73.5", "ODRA": "M 167.7,208.1 L 173.4,198.8 L 173.4,186.4 L 156.3,174.0 L 141.1,158.6 L 127.8,146.2 L 110.8,143.1 L 109.8,133.8 L 107.9,124.5 L 101.7,120.2 L 106.0,107.5 L 108.9,102.9 L 110.8,90.5"}, "miasta": [{"n": "Warszawa", "x": 232.4, "y": 139.1}, {"n": "Kraków", "x": 212.1, "y": 206.2}, {"n": "Gdańsk", "x": 187.6, "y": 73.5}, {"n": "Wrocław", "x": 156.9, "y": 173.7}, {"n": "Poznań", "x": 155.0, "y": 133.5}], "miastaTrueOrtho": [{"n": "Warszawa", "x": 232.4, "y": 139.1}, {"n": "Kraków", "x": 212.1, "y": 206.2}, {"n": "Trójmiasto", "x": 187.6, "y": 73.5}, {"n": "Wrocław", "x": 156.9, "y": 173.7}, {"n": "Poznań", "x": 155.0, "y": 133.5}, {"n": "Łódź", "x": 207.7, "y": 151.7}, {"n": "GOP / Katowice", "x": 202.9, "y": 196.9}]};

  function geoDoSvg(lon, lat) {
    return {
      x: MAPA_GEOM.offX + (lon - MAPA_GEOM.lonMin) * MAPA_GEOM.cosLat0 * MAPA_GEOM.scale,
      y: MAPA_GEOM.offY + (MAPA_GEOM.latMax - lat) * MAPA_GEOM.scale
    };
  }

  function opisZasieguDlaTabeli(w) {
    const z = w.zasieg || { typ: "kraj", etykieta: "Cała Polska" };
    if (z.typ === "kraj") return "Cała Polska (zasięg ogólnokrajowy)";
    if (z.typ === "zagranica") return z.etykieta || "Zasięg zagraniczny";
    if (z.typ === "morski") return z.etykieta || "Morze Bałtyckie";
    if (z.typ === "globalny") return "Globalny (cały świat)";
    if (z.typ === "regionalny") {
      let tekst = z.etykieta || "Zasięg regionalny";
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

    const wrap = el("div", "gw-widok-zasiegu-kontener");

    let klasaPL = "gw-mapa-kraj gw-mapa-polska";
    if (typ === "kraj") klasaPL += " gw-mapa-aktywny";
    else if (typ === "regionalny") klasaPL += " gw-mapa-baza-regionalna";

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
    svgHtml += '<path d="' + MAPA_GEOM.paths.BALTYK + '" class="' + klasaBaltyk + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_REST + '" class="' + klasaDE_REST + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_BB + '" class="' + klasaDE_BB + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_SN + '" class="' + klasaDE_SN + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.CZ + '" class="' + klasaCZ + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.SK + '" class="' + klasaSK + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.LT + '" class="' + klasaLT + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.PL + '" class="' + klasaPL + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.WISLA + '" class="gw-mapa-rzeka" fill="none" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.ODRA + '" class="gw-mapa-rzeka" fill="none" />';
    svgHtml += '<text x="180" y="48" class="gw-etykieta-kraju gw-etykieta-baltyk" text-anchor="middle">BAŁTYK</text>';
    svgHtml += '<text x="82" y="130" class="gw-etykieta-kraju" text-anchor="middle">NIEMCY</text>';
    svgHtml += '<text x="135" y="215" class="gw-etykieta-kraju" text-anchor="middle">CZECHY</text>';
    svgHtml += '<text x="210" y="250" class="gw-etykieta-kraju" text-anchor="middle">SŁOWACJA</text>';
    svgHtml += '<text x="278" y="55" class="gw-etykieta-kraju" text-anchor="middle">LITWA</text>';
    svgHtml += '<text x="195" y="145" class="gw-etykieta-kraju gw-etykieta-polska" text-anchor="middle">POLSKA</text>';

    if (typ !== "globalny") {
      MAPA_GEOM.miasta.forEach(function (m) {
        svgHtml += '<g class="gw-mapa-miasto" transform="translate(' + m.x.toFixed(1) + ',' + m.y.toFixed(1) + ')">';
        svgHtml += '<circle r="2.2" class="gw-miasto-kropka" />';
        svgHtml += '<text dx="4" dy="3" class="gw-miasto-nazwa">' + m.n + '</text>';
        svgHtml += '</g>';
      });
    }

    if (typ === "regionalny") {
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
      } else if (w.zbior && w.zbior.indexOf("PrawdziwaOrtofotomapa") >= 0) {
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
    const ikona = typ === "morski" ? "🌊" : (typ === "zagranica" ? "🌐" : (typ === "regionalny" ? "📍" : (typ === "globalny" ? "🌍" : "🇵🇱")));

    const infoTytul = el("div", "gw-zasieg-info-tytul");
    infoTytul.appendChild(el("span", "gw-zasieg-info-ikona", ikona));
    infoTytul.appendChild(el("strong", null, z.etykieta || "Cała Polska"));
    info.appendChild(infoTytul);

    let opisSzczegolowy = "";
    if (typ === "kraj") {
      opisSzczegolowy = "Warstwa obejmuje obszar całej Polski (pokrycie ogólnokrajowe).";
    } else if (typ === "zagranica") {
      opisSzczegolowy = "Urzędowa warstwa zagraniczna (" + (z.kod || "sąsiedzi") + "). Przydatna przy planowaniu tras i pobieraniu map pogranicza.";
    } else if (typ === "morski") {
      opisSzczegolowy = "Polskie obszary morskie, wyłączna strefa ekonomiczna (WZE) lub międzynarodowe dane batymetryczne Bałtyku.";
    } else if (typ === "regionalny") {
      opisSzczegolowy = z.bbox
        ? "Zasięg regionalny ograniczony do wybranego obszaru historycznego lub geograficznego."
        : "Zasięg regionalny obejmujący wybrane duże miasta i aglomeracje.";
    } else if (typ === "globalny") {
      opisSzczegolowy = "Warstwa o zasięgu globalnym (pokrycie całego świata z serwerów otwartych / międzynarodowych).";
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
      ref.modalKanapka.textContent = "✓ W kanapce (#" + (idx + 1) + ")";
    } else {
      ref.modalKanapka.classList.remove("aktywny");
      ref.modalKanapka.textContent = "+ Do kanapki";
    }
  }

  function otworzModal(indeks) {
    if (indeks < 0 || indeks >= stan.widoczne.length) return;
    stan.indeksModala = indeks;
    const w = stan.widoczne[indeks];

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

  function odswiezDokKanapki() {
    if (!ref.mikserDok) return;
    const n = stan.kanapka.length;
    ref.mikserDokLicznik.textContent = n + " / " + MAKS_WARSTW_KANAPKI + " warstw";
    if (n === 0) {
      ref.mikserDokOpis.textContent = "Zaznacz warstwy haczykiem „☐ Do kanapki”, aby ułożyć własny miks.";
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
    renderujListeMiksera();
    odswiezMinimape();
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

  function renderujListeMiksera() {
    if (!ref.mikserWarstwy) return;
    ref.mikserWarstwy.innerHTML = "";

    const ostrz = ostrzezeniaKanapki(stan.kanapka);
    if (ostrz.length > 0) {
      ref.mikserOstrzezenia.hidden = false;
      ref.mikserOstrzezenia.textContent = ostrz.join(" ");
    } else {
      ref.mikserOstrzezenia.hidden = true;
    }

    if (ref.mikserBtnAuto) ref.mikserBtnAuto.disabled = stan.kanapka.length < 2;
    if (ref.mikserBtnWyczysc) ref.mikserBtnWyczysc.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnWyslijGlowne) ref.mikserBtnWyslijGlowne.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnWyslijKreator) ref.mikserBtnWyslijKreator.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnPobierzPlik) ref.mikserBtnPobierzPlik.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnKopiujLink) ref.mikserBtnKopiujLink.disabled = stan.kanapka.length === 0;
    if (ref.mikserBtnKopiujJson) ref.mikserBtnKopiujJson.disabled = stan.kanapka.length === 0;

    if (stan.kanapka.length === 0) {
      ref.mikserWarstwy.appendChild(
        el("div", "gw-brak-wynikow", "Kanapka jest pusta. Wybierz warstwy z galerii przyciskiem „☐ Do kanapki”.")
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
          zglosOdswiezeniePodgladuMiksera();
        });
        suwakGrupa.appendChild(suwak);
        suwakGrupa.appendChild(wartoscSpan);

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

        dol.appendChild(suwakGrupa);
        dol.appendChild(selectTryb);

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

    ref.mikserLadowanie.hidden = false;
    const payload = {
      warstwy: stan.kanapka.map(function (x) {
        return { zbior: x.zbior, warstwa: x.warstwa, krycie: x.krycie, tryb: x.tryb };
      }),
      lat: stan.podgladLat,
      lon: stan.podgladLon,
      piksel: stan.podgladPiksel > 0 ? stan.podgladPiksel : null
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
        const opisPx = wynik.pxH ? " · " + wynik.pxH.replace(".", ",") + " m/px" : "";
        let info = "Podgląd na żywo z MapoTero (" + opisPunktu + opisPx + ").";
        if (wynik.brakH) {
          const brakujace = decodeURIComponent(wynik.brakH).split("|").join(", ");
          info += " Pominięto / brak odpowiedzi: " + brakujace + ".";
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
    ref.mikserCanvas.hidden = false;
    ref.mikserImgLive.hidden = true;
    ref.mikserLadowanie.hidden = false;

    const zProbka = [];
    const bezProbki = [];
    stan.kanapka.forEach(function (item) {
      if (item.probka_miksu) zProbka.push(item);
      else bezProbki.push(item);
    });

    const ctx = ref.mikserCanvas.getContext("2d");
    const szer = ref.mikserCanvas.width;
    const wys = ref.mikserCanvas.height;

    if (zProbka.length === 0) {
      ctx.clearRect(0, 0, szer, wys);
      ref.mikserLadowanie.hidden = true;
      ref.mikserStanPodgladu.classList.add("ostrzezenie");
      ref.mikserStanPodgladu.textContent =
        "Wybrane warstwy nie mają wspólnej próbki statycznej offline. Otwórz galerię z programu MapoTero, aby zobaczyć podgląd na żywo z serwerów.";
      return;
    }

    const obietnice = zProbka.map(function (item) {
      return ladujObrazekZCache(sciezkaObrazka(item.probka_miksu)).then(function (img) {
        return { item: item, img: img };
      });
    });

    Promise.all(obietnice).then(function (zaladowane) {
      ctx.clearRect(0, 0, szer, wys);

      const tempCanvas = document.createElement("canvas");
      tempCanvas.width = szer;
      tempCanvas.height = wys;
      const tempCtx = tempCanvas.getContext("2d");

      const bledne = [];
      zaladowane.forEach(function (para) {
        if (!para.img) {
          bledne.push(para.item.nazwa);
          return;
        }
        const trybInfo = znajdzTryb(para.item.tryb);
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
      });

      ref.mikserLadowanie.hidden = true;
      const miejsce = (stan.dane && stan.dane.mikser_miejsce) || "wspólny kadr testowy";
      if (bezProbki.length > 0 || bledne.length > 0) {
        const pomin = bezProbki.map(function (x) { return x.nazwa; }).concat(bledne);
        ref.mikserStanPodgladu.classList.add("ostrzezenie");
        ref.mikserStanPodgladu.textContent =
          "Podgląd offline (" + miejsce + "). Bez wspólnej próbki (" + pomin.join(", ") + ") — pełny podgląd działa po otwarciu galerii z MapoTero.";
      } else {
        ref.mikserStanPodgladu.classList.remove("ostrzezenie");
        ref.mikserStanPodgladu.textContent = "Podgląd miksera na wspólnej próbce: " + miejsce + ".";
      }
    });
  }

  function odswiezMinimape() {
    if (!ref.minimapaSiatka || !stan.minimapaOtwarta) return;
    const zoom = stan.podgladPiksel > 0 && stan.podgladPiksel <= 1.5 ? 14 : (stan.podgladPiksel >= 10 ? 11 : 13);
    const pt = latLonDoKafelka(stan.podgladLat, stan.podgladLon, zoom);
    const txCenter = Math.floor(pt.x);
    const tyCenter = Math.floor(pt.y);
    const fracX = pt.x - txCenter;
    const fracY = pt.y - tyCenter;

    stan.minimapaBazaKafelki = { zoom: zoom, txMin: txCenter - 1, tyMin: tyCenter - 1 };

    ref.minimapaSiatka.innerHTML = "";
    for (let dy = -1; dy <= 1; dy++) {
      for (let dx = -1; dx <= 1; dx++) {
        const img = document.createElement("img");
        img.alt = "";
        img.src = "https://tile.openstreetmap.org/" + zoom + "/" + (txCenter + dx) + "/" + (tyCenter + dy) + ".png";
        ref.minimapaSiatka.appendChild(img);
      }
    }
    const celownik = el("div", "gw-minimapa-celownik");
    celownik.style.left = (((1 + fracX) / 3) * 100).toFixed(2) + "%";
    celownik.style.top = (((1 + fracY) / 3) * 100).toFixed(2) + "%";
    ref.minimapaSiatka.appendChild(celownik);
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
    const dokOpis = el("span", null, "");
    dokInfo.appendChild(el("strong", null, "🧩 Mikser kanapki:"));
    dokInfo.appendChild(dokLicznik);
    dokInfo.appendChild(dokOpis);

    const dokAkcje = el("div", "gw-mikser-dok-akcje");
    const dokOtworz = el("button", "gw-btn-glowny", "Otwórz mikser i podgląd");
    dokOtworz.type = "button";
    dokOtworz.addEventListener("click", otworzMikser);
    dokAkcje.appendChild(dokOtworz);

    let dokWyslij = null;
    if (czyPolaczonoZProgramem()) {
      dokWyslij = el("button", "gw-btn-zielony", "Wyślij do MapoTero");
      dokWyslij.type = "button";
      dokWyslij.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("okno_glowne", false);
      });
      dokAkcje.appendChild(dokWyslij);
    }

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
    mikserNaglowek.appendChild(el("h3", null, "🧩 Mikser kanapki warstw"));
    const mikserZamknij = el("button", "gw-modal-zamknij", "×");
    mikserZamknij.type = "button";
    mikserZamknij.setAttribute("aria-label", "Zamknij mikser");
    mikserZamknij.addEventListener("click", zamknijMikser);
    mikserNaglowek.appendChild(mikserZamknij);

    const mikserCialo = el("div", "gw-mikser-cialo");

    // Lewa kolumna: podgląd + wybór miejsca
    const kolPodglad = el("div", "gw-mikser-kol-podglad");
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
    if (czyPolaczonoZProgramem()) {
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
      selSkala.addEventListener("change", function () {
        stan.podgladPiksel = parseFloat(selSkala.value) || 0;
        odswiezMinimape();
        zglosOdswiezeniePodgladuMiksera(true);
      });

      const btnZMapoTero = el("button", "gw-btn-prosty", "↺ Obszar z MapoTero");
      btnZMapoTero.type = "button";
      btnZMapoTero.title = "Pobierz ponownie środek zaznaczenia z okna MapoTero";
      btnZMapoTero.addEventListener("click", function () {
        pobierzStanZMapoTero(false, false);
      });

      const btnMinimapa = el("button", "gw-btn-prosty", "🗺️ Wybierz miejsce");
      btnMinimapa.type = "button";
      btnMinimapa.addEventListener("click", function () {
        stan.minimapaOtwarta = !stan.minimapaOtwarta;
        minimapaBox.hidden = !stan.minimapaOtwarta;
        if (stan.minimapaOtwarta) odswiezMinimape();
      });

      sterowaniePodgladu.appendChild(selSkala);
      sterowaniePodgladu.appendChild(btnZMapoTero);
      sterowaniePodgladu.appendChild(btnMinimapa);
    }

    const minimapaBox = el("div", "gw-mikser-minimapa-box");
    minimapaBox.hidden = true;
    minimapaBox.appendChild(el("div", null, "Kliknij na mapie miejsce do podglądu w MapoTero:"));
    const minimapaSiatka = el("div", "gw-minimapa-siatka");
    minimapaSiatka.addEventListener("click", function (e) {
      if (!stan.minimapaBazaKafelki) return;
      const rect = minimapaSiatka.getBoundingClientRect();
      const rx = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
      const ry = Math.max(0, Math.min(1, (e.clientY - rect.top) / rect.height));
      const tx = stan.minimapaBazaKafelki.txMin + rx * 3;
      const ty = stan.minimapaBazaKafelki.tyMin + ry * 3;
      const geo = kafelekDoLatLon(tx, ty, stan.minimapaBazaKafelki.zoom);
      stan.podgladLat = Math.round(geo.lat * 10000) / 10000;
      stan.podgladLon = Math.round(geo.lon * 10000) / 10000;
      odswiezMinimape();
      zglosOdswiezeniePodgladuMiksera(true);
    });
    const skrotyBox = el("div", "gw-minimapa-skroty");
    SKROTY_MIEJSC.forEach(function (m) {
      const b = el("button", null, m.nazwa);
      b.type = "button";
      b.addEventListener("click", function () {
        stan.podgladLat = m.lat;
        stan.podgladLon = m.lon;
        odswiezMinimape();
        zglosOdswiezeniePodgladuMiksera(true);
      });
      skrotyBox.appendChild(b);
    });
    minimapaBox.appendChild(minimapaSiatka);
    minimapaBox.appendChild(skrotyBox);

    const mikserStanPodgladu = el("div", "gw-mikser-stan-podgladu", "");

    kolPodglad.appendChild(ramkaPodgladu);
    kolPodglad.appendChild(sterowaniePodgladu);
    kolPodglad.appendChild(minimapaBox);
    kolPodglad.appendChild(mikserStanPodgladu);

    // Prawa kolumna: warstwy, suwaki i eksport
    const kolLista = el("div", "gw-mikser-kol-lista");
    const pasekListy = el("div", "gw-mikser-pasek-listy");
    pasekListy.appendChild(el("span", null, "Kolejność kanapki: na górze warstwa najwyższa, na dole podkład (1)."));

    const przyciskiListy = el("div", "gw-modal-linki");
    const btnAuto = el("button", "gw-btn-prosty", "⚡ Ułóż automatycznie");
    btnAuto.type = "button";
    btnAuto.title = "Przesuwa podkłady na spód, nakładki na górę i włącza tryb Rzeźba terenu dla cieniowania";
    btnAuto.addEventListener("click", function () {
      stan.kanapka = ulozKanapkeAutomatycznie(stan.kanapka);
      poZmianieKanapki();
    });

    const btnWyczysc = el("button", "gw-btn-prosty", "Wyczyść");
    btnWyczysc.type = "button";
    btnWyczysc.addEventListener("click", function () {
      stan.kanapka = [];
      pokazStatusMiksera("", false);
      poZmianieKanapki();
    });

    przyciskiListy.appendChild(btnAuto);
    if (czyPolaczonoZProgramem()) {
      const btnWczytajZProg = el("button", "gw-btn-prosty", "↺ Wczytaj z MapoTero");
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
    });
    nazwaWiersz.appendChild(inputNazwa);

    const akcjeGlowne = el("div", "gw-mikser-akcje-glowne");
    let btnWyslijGlowne = null;
    let btnWyslijKreator = null;
    if (czyPolaczonoZProgramem()) {
      const chkStyl = document.createElement("input");
      chkStyl.type = "checkbox";
      chkStyl.checked = stan.zapiszJakoStyl;
      chkStyl.addEventListener("change", function () {
        stan.zapiszJakoStyl = chkStyl.checked;
      });
      const lblChk = el("label", null, " zapisz też jako własny styl kreatora");
      lblChk.style.fontSize = "0.8rem";
      lblChk.style.display = "inline-flex";
      lblChk.style.alignItems = "center";
      lblChk.style.gap = "0.25rem";
      lblChk.prepend(chkStyl);

      btnWyslijGlowne = el("button", "gw-btn-zielony", "▶ Wyślij do MapoTero (Okno główne)");
      btnWyslijGlowne.type = "button";
      btnWyslijGlowne.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("okno_glowne", stan.zapiszJakoStyl);
      });

      btnWyslijKreator = el("button", "gw-btn-glowny", "✨ Otwórz w Szybkim Kreatorze");
      btnWyslijKreator.type = "button";
      btnWyslijKreator.addEventListener("click", function () {
        wyslijKanapkeDoMapoTero("kreator", true);
      });

      akcjeGlowne.appendChild(btnWyslijGlowne);
      akcjeGlowne.appendChild(btnWyslijKreator);
      akcjeGlowne.appendChild(lblChk);
    }

    const akcjeZapasowe = el("div", "gw-mikser-akcje-zapasowe");
    const btnPobierzPlik = el("button", "gw-btn-prosty", "💾 Pobierz plik (.mapotero)");
    btnPobierzPlik.type = "button";
    btnPobierzPlik.title = "Plik możesz przeciągnąć bezpośrednio na okno MapoTero";
    btnPobierzPlik.addEventListener("click", function () {
      const przepis = budujPrzepisJson(stan.kanapka, {
        nazwa: stan.nazwaKanapki,
        cel: "okno_glowne",
        zapiszStyl: stan.zapiszJakoStyl
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
      if (!navigator.clipboard) return;
      navigator.clipboard.writeText(link).then(function () {
        pokazStatusMiksera("Skopiowano link do udostępnienia tej kanapki.", false);
      });
    });

    const btnKopiujJson = el("button", "gw-btn-prosty", "📋 Kopiuj JSON");
    btnKopiujJson.type = "button";
    btnKopiujJson.addEventListener("click", function () {
      const przepis = budujPrzepisJson(stan.kanapka, {
        nazwa: stan.nazwaKanapki,
        cel: "okno_glowne",
        zapiszStyl: stan.zapiszJakoStyl
      });
      if (!navigator.clipboard) return;
      navigator.clipboard.writeText(JSON.stringify(przepis, null, 2)).then(function () {
        pokazStatusMiksera("Skopiowano przepis JSON do schowka — kliknij prawym na liście warstw w MapoTero i wybierz „Wklej kanapkę ze schowka”.", false);
      });
    });

    akcjeZapasowe.appendChild(btnPobierzPlik);
    akcjeZapasowe.appendChild(btnKopiujLink);
    akcjeZapasowe.appendChild(btnKopiujJson);

    const statusWysylki = el("div", "gw-mikser-status-wysylki", "");
    statusWysylki.hidden = true;

    sekcjaEksport.appendChild(nazwaWiersz);
    if (akcjeGlowne.childNodes.length > 0) sekcjaEksport.appendChild(akcjeGlowne);
    sekcjaEksport.appendChild(akcjeZapasowe);
    sekcjaEksport.appendChild(statusWysylki);

    kolLista.appendChild(pasekListy);
    kolLista.appendChild(mikserWarstwy);
    kolLista.appendChild(mikserOstrzezenia);
    kolLista.appendChild(sekcjaEksport);

    mikserCialo.appendChild(kolPodglad);
    mikserCialo.appendChild(kolLista);
    mikserModal.appendChild(mikserNaglowek);
    mikserModal.appendChild(mikserCialo);
    mikserTlo.appendChild(mikserModal);

    return {
      mikserDok: mikserDok,
      mikserDokLicznik: dokLicznik,
      mikserDokOpis: dokOpis,
      mikserDokOtworz: dokOtworz,
      mikserDokWyslij: dokWyslij,
      mikserTlo: mikserTlo,
      mikserZamknij: mikserZamknij,
      mikserCanvas: mikserCanvas,
      mikserImgLive: mikserImgLive,
      mikserLadowanie: mikserLadowanie,
      mikserStanPodgladu: mikserStanPodgladu,
      minimapaSiatka: minimapaSiatka,
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
        modalPanelZasieg.hidden = false;
      } else {
        tabZasieg.classList.remove("aktywny");
        tabZasieg.setAttribute("aria-selected", "false");
        tabProbka.classList.add("aktywny");
        tabProbka.setAttribute("aria-selected", "true");
        modalPanelZasieg.hidden = true;
        modalPanelProbka.hidden = false;
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

    const modalKanapka = el("button", "gw-btn-kanapka", "+ Do kanapki");
    modalKanapka.type = "button";
    modalKanapka.addEventListener("click", function () {
      const w = stan.widoczne[stan.indeksModala];
      if (w) przelaczWarstweWKanapce(w);
    });
    modalLinki.appendChild(modalKanapka);

    let modalDodaj = null;
    if (czyPolaczonoZProgramem()) {
      modalDodaj = el("button", "gw-btn-dodaj-modal", "+ Dodaj do MapoTero");
      modalDodaj.type = "button";
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
      if (!w || !navigator.clipboard) return;
      navigator.clipboard.writeText(w.nazwa).then(function () {
        modalKopiuj.textContent = "Skopiowano!";
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
            " — zaznacz warstwy haczykami „Do kanapki” i otwórz Mikser na dole ekranu albo dodawaj pojedynczo.";
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
