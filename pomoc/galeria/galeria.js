(function () {
  "use strict";

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
    token: ""
  };

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
    if (wartosc === null || wartosc === undefined || wartosc === "") return "—";
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

  function wczytajParametryUrl() {
    const params = new URLSearchParams(window.location.search);
    if (params.has("q")) stan.szukaj = params.get("q") || "";
    if (params.has("grupa")) stan.grupa = params.get("grupa") || "WSZYSTKIE";
    if (params.has("typ")) stan.typ = (params.get("typ") || "WSZYSTKIE").toUpperCase();
    if (params.has("rola")) stan.rola = params.get("rola") || "WSZYSTKIE";
    const p = parseInt(params.get("port") || "0", 10);
    const t = (params.get("token") || "").trim();
    if (p > 0 && p <= 65535 && t) {
      stan.port = p;
      stan.token = t;
    }
  }

  function czyPolaczonoZProgramem() {
    return stan.port > 0 && stan.token !== "";
  }

  function zapiszParametryUrl() {
    const params = new URLSearchParams();
    if (stan.szukaj.trim()) params.set("q", stan.szukaj.trim());
    if (stan.grupa !== "WSZYSTKIE") params.set("grupa", stan.grupa);
    if (stan.typ !== "WSZYSTKIE") params.set("typ", stan.typ);
    if (stan.rola !== "WSZYSTKIE") params.set("rola", stan.rola);
    if (czyPolaczonoZProgramem()) {
      params.set("port", String(stan.port));
      params.set("token", stan.token);
    }
    const ciag = params.toString();
    const nowyUrl = window.location.pathname + (ciag ? "?" + ciag : "") + window.location.hash;
    window.history.replaceState(null, "", nowyUrl);
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

  let ref = {};

  function utworzKartySiatki(elementy) {
    const siatka = el("div", "gw-siatka" + (stan.rozmiar === "DUZE" ? " gw-siatka-duza" : ""));
    elementy.forEach(function (pozycja) {
      const w = pozycja.warstwa;
      const karta = el("div", "gw-karta");
      karta.setAttribute("role", "button");
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

      if (czyPolaczonoZProgramem()) {
        const btnDodaj = el("button", "gw-btn-dodaj", "+ Dodaj do MapoTero");
        btnDodaj.type = "button";
        btnDodaj.addEventListener("click", function (e) {
          e.stopPropagation();
          wyslijDoMapoTero(w, btnDodaj);
        });
        tresc.appendChild(btnDodaj);
      }

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

    // Tło akwenu Bałtyku
    svgHtml += '<path d="' + MAPA_GEOM.paths.BALTYK + '" class="' + klasaBaltyk + '" />';

    // Kraje sąsiednie
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_REST + '" class="' + klasaDE_REST + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_BB + '" class="' + klasaDE_BB + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.DE_SN + '" class="' + klasaDE_SN + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.CZ + '" class="' + klasaCZ + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.SK + '" class="' + klasaSK + '" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.LT + '" class="' + klasaLT + '" />';

    // Polska
    svgHtml += '<path d="' + MAPA_GEOM.paths.PL + '" class="' + klasaPL + '" />';

    // Rzeki główne
    svgHtml += '<path d="' + MAPA_GEOM.paths.WISLA + '" class="gw-mapa-rzeka" fill="none" />';
    svgHtml += '<path d="' + MAPA_GEOM.paths.ODRA + '" class="gw-mapa-rzeka" fill="none" />';

    // Etykiety państw i mórz
    svgHtml += '<text x="180" y="48" class="gw-etykieta-kraju gw-etykieta-baltyk" text-anchor="middle">BAŁTYK</text>';
    svgHtml += '<text x="82" y="130" class="gw-etykieta-kraju" text-anchor="middle">NIEMCY</text>';
    svgHtml += '<text x="135" y="215" class="gw-etykieta-kraju" text-anchor="middle">CZECHY</text>';
    svgHtml += '<text x="210" y="250" class="gw-etykieta-kraju" text-anchor="middle">SŁOWACJA</text>';
    svgHtml += '<text x="278" y="55" class="gw-etykieta-kraju" text-anchor="middle">LITWA</text>';
    svgHtml += '<text x="195" y="145" class="gw-etykieta-kraju gw-etykieta-polska" text-anchor="middle">POLSKA</text>';

    // Punkty orientacyjne miast (dla map nieglobalnych)
    if (typ !== "globalny") {
      MAPA_GEOM.miasta.forEach(function (m) {
        svgHtml += '<g class="gw-mapa-miasto" transform="translate(' + m.x.toFixed(1) + ',' + m.y.toFixed(1) + ')">';
        svgHtml += '<circle r="2.2" class="gw-miasto-kropka" />';
        svgHtml += '<text dx="4" dy="3" class="gw-miasto-nazwa">' + m.n + '</text>';
        svgHtml += '</g>';
      });
    }

    // Zasięg regionalny: prostokąt BBOX lub wyznaczone miasta TrueOrtho
    if (typ === "regionalny") {
      if (z.bbox && Array.isArray(z.bbox) && z.bbox.length === 4) {
        const p1 = geoDoSvg(z.bbox[0], z.bbox[3]); // minLon, maxLat
        const p2 = geoDoSvg(z.bbox[2], z.bbox[1]); // maxLon, minLat
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

    // Nakładka dla warstw globalnych
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

    // Karta informacyjna pod mapą
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
      if (z.bbox) {
        opisSzczegolowy = "Zasięg regionalny ograniczony do wybranego obszaru historycznego lub geograficznego.";
      } else {
        opisSzczegolowy = "Zasięg regionalny obejmujący wybrane duże miasta i aglomeracje.";
      }
    } else if (typ === "globalny") {
      opisSzczegolowy = "Warstwa o zasięgu globalnym (pokrycie całego świata z serwerów otwartych / międzynarodowych).";
    }
    info.appendChild(el("div", "gw-zasieg-info-opis", opisSzczegolowy));

    wrap.appendChild(info);
    return wrap;
  }

  function otworzModal(indeks) {
    if (indeks < 0 || indeks >= stan.widoczne.length) return;
    stan.indeksModala = indeks;
    const w = stan.widoczne[indeks];

    // 1. Obsługa próbki mapy
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

    // 2. Obsługa mapy zasięgu
    ref.modalPanelZasieg.innerHTML = "";
    ref.modalPanelZasieg.appendChild(utworzWidokZasieguSvg(w));

    // Utrzymanie aktualnie wybranej zakładki (domyślnie probka)
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

    // Modal
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

    document.addEventListener("keydown", function (e) {
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
          pasekMapoTero.textContent =
            "Połączono z programem MapoTero — kliknij „+ Dodaj do MapoTero” przy dowolnej warstwie.";
        })
        .catch(function () {
          pasekMapoTero.classList.add("gw-pasek-blad");
          pasekMapoTero.textContent =
            "Brak odpowiedzi z MapoTero — upewnij się, że program nadal jest uruchomiony i że w wyskakującym oknie przeglądarki wyraziłeś zgodę na dostęp do sieci lokalnej (127.0.0.1).";
        });
    }

    kontener.appendChild(panel);
    kontener.appendChild(wyniki);
    kontener.appendChild(modalTlo);

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

    ref = {
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
      modalKopiuj: modalKopiuj,
      modalPoprz: modalPoprz,
      modalNast: modalNast,
      modalZamknij: modalZamknij
    };

    renderujSiatke();
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
})();
