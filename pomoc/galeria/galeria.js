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
    port: 0,
    token: ""
  };

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
        const tekst = normalizuj(
          [w.nazwa, w.zbior, w.warstwa, w.identyfikator, w.grupa, w.podgrupa, w.zrodlo].join(" ")
        );
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

  function otworzModal(indeks) {
    if (indeks < 0 || indeks >= stan.widoczne.length) return;
    stan.indeksModala = indeks;
    const w = stan.widoczne[indeks];

    ref.modalObrazKol.innerHTML = "";
    if (w.nakladka) {
      ref.modalObrazKol.classList.add("gw-nakladka-tlo");
    } else {
      ref.modalObrazKol.classList.remove("gw-nakladka-tlo");
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
      ref.modalObrazKol.appendChild(img);
    } else {
      brak.hidden = false;
    }
    ref.modalObrazKol.appendChild(brak);

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
      ["Źródło", w.zrodlo || "—"],
      ["Miejsce próbki", w.miejsce_probki || "—"]
    ];

    wiersze.forEach(function (para) {
      const tr = document.createElement("tr");
      tr.appendChild(el("th", null, para[0]));
      tr.appendChild(el("td", null, para[1] || "—"));
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
