'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Okno wyboru dodatkowej warstwy do kompozycji mapy: prezentuje hierarchiczne drzewo
''' serwerów WMS/WMTS/XYZ z czytelnym, zrozumiałym nazewnictwem i możliwością wyszukiwania.
''' </summary>
Public Class FormWybierzDodatkowaWarstwe
    Inherits Form

    Public Class ElementWarstwy
        Public Property NazwaWyswietlana As String = ""
        Public Property NazwaTechniczna As String = ""
        Public Property ZbiorNazwa As String = ""
        Public Property ZbiorKlucz As String = ""
        Public Property AdresSerwera As String = ""
        Public Property TypUslugi As TypUslugiMapowej
        Public Property ZalecanyPiksel As Double = 0
        Public Property JestNakladka As Boolean
        Public Property IgnorujSsl As Boolean
        Public Property CzyJuzDodana As Boolean = False

        Public Overrides Function ToString() As String
            Return NazwaWyswietlana & " [" & ZbiorNazwa & "]"
        End Function
    End Class

    Private _czcionkaPogrubiona As Font

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _czcionkaPogrubiona?.Dispose()
            _czcionkaPogrubiona = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Shared ReadOnly _znaneNazwySerwerow As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
        {"BDOO_WMTS", "Geoportal GUGiK – Baza Danych Obiektów Ogólnogeograficznych (BDOO)"},
        {"BDOT10k_WMTS", "Geoportal GUGiK – Baza Danych Obiektów Topograficznych (BDOT10k)"},
        {"TOPO_WMTS", "Geoportal GUGiK – Mapa topograficzna Polski (WMTS)"},
        {"ortofotomapa_WMTS", "Geoportal GUGiK – Ortofotomapa standardowa (WMTS)"},
        {"ortofotomapa_HighResolution_WMTS", "Geoportal GUGiK – Ortofotomapa wysokiej rozdzielczości do 10 cm (WMTS)"},
        {"ortofotomapa", "Geoportal GUGiK – Ortofotomapa (WMS)"},
        {"ortofotomapa_TrueOrtho", "Geoportal GUGiK – Prawdziwa ortofotomapa (TrueOrtho)"},
        {"numer_mod_terenu_cieniowanie_WMTS", "Geoportal GUGiK – Cieniowanie rzeźby terenu NMT (ISOK LiDAR)"},
        {"numer_mod_terenu_hipsometria_WMTS", "Geoportal GUGiK – Hipsometria NMT (WMTS)"},
        {"numer_mod_terenu_WMTS", "Geoportal GUGiK – Numeryczny Model Terenu (NMT)"},
        {"skany_map_topograficznych", "Geoportal GUGiK – Skany map topograficznych (1942, 1965, 1992, UTM)"},
        {"krajobrazowa", "Geoportal GUGiK – Mapa krajobrazowa (przeglądowa)"},
        {"administracyjna", "Geoportal GUGiK – Podział administracyjny Polski"},
        {"dzialki_ewid_EGIB", "Geoportal GUGiK – Ewidencja gruntów i budynków (działki i budynki EGIB)"},
        {"uzbrojenie_terenu_KIUT", "Geoportal GUGiK – Sieci uzbrojenia terenu (KIUT)"},
        {"rejestr_granic_jednostki_teryt", "Geoportal GUGiK – Państwowy Rejestr Granic (TERYT)"},
        {"rejestr_granic_punkty_adresowe", "Geoportal GUGiK – Punkty adresowe (PRG)"},
        {"rejestr_nazw_geograficznych", "Geoportal GUGiK – Państwowy Rejestr Nazw Geograficznych (PRNG)"},
        {"osnowy_geodezyjne", "Geoportal GUGiK – Osnowy geodezyjne (KIO)"},
        {"hydrograficzna_50", "Geoportal GUGiK – Mapa hydrograficzna 1:50 000"},
        {"sozologiczna_50", "Geoportal GUGiK – Mapa sozologiczna 1:50 000"},
        {"mapa_glebowo_rolnicza", "Geoportal GUGiK – Mapa glebowo-rolnicza"},
        {"szczyty", "Geoportal GUGiK – Szczyty i formy terenu"},
        {"OSMapa_PL", "OpenStreetMap – OSMapa.pl (topograficzno-turystyczna)"},
        {"Waymarked_Trails", "Waymarked Trails – Szlaki turystyczne i nakładki (piesze, rowerowe, MTB)"},
        {"OpenTopoMap", "OpenTopoMap – Mapa topograficzna Europy"},
        {"UMP_pcPL", "UMP-pcPL – Uzupełniająca Mapa Polski"},
        {"OpenStreetMap_Terrestris", "Terrestris – Poziomice SRTM 30 m"},
        {"Lasy_Panstwowe_BDL", "Lasy Państwowe – Bank Danych o Lasach (oddziały, drzewostan BDL)"},
        {"Lasy_Panstwowe_Turystyka", "Lasy Państwowe – Zagospodarowanie turystyczne i program Zanocuj w lesie (BDL)"},
        {"Lasy_Panstwowe_Dojazdy_Ppoz", "Lasy Państwowe – Dojazdy pożarowe i drogi leśne (BDL)"},
        {"Lasy_Panstwowe_Drzewostany", "Lasy Państwowe – Mapa drzewostanów i klas odnowienia (BDL)"},
        {"Lasy_Panstwowe_Siedliska", "Lasy Państwowe – Mapa siedlisk leśnych TSL (BDL)"},
        {"Lasy_Panstwowe_Wlasnosc", "Lasy Państwowe – Kategorie i formy własności lasów (BDL)"},
        {"Lasy_Panstwowe_Ochronnosc", "Lasy Państwowe – Kategorie ochronności lasów (BDL)"},
        {"Lasy_Panstwowe_Lowiectwo", "Lasy Państwowe – Obwody łowieckie (BDL)"},
        {"obszary_chronione_GDOS", "GDOŚ – Parki narodowe, krajobrazowe i Natura 2000"},
        {"geologiczna_50_SMGP", "PIG-PIB – Szczegółowa Mapa Geologiczna Polski 1:50 000 (SMGP)"},
        {"geologiczna_50_litogenetyczna", "PIG-PIB – Mapa litogenetyczna Polski 1:50 000"},
        {"geologiczna_500", "PIG-PIB – Mapa geologiczna Polski 1:500 000"},
        {"regiony_fizycznogeograficzne", "PIG-PIB – Regiony fizycznogeograficzne Kondrackiego"},
        {"baltyk_batymetria_wms", "EMODnet – Batymetria Bałtyku i mórz europejskich"},
        {"zabytki_NID", "NID – Zabytki nieruchome (rejestr zabytków)"},
        {"zabytki_archeo_NID", "NID – Zabytki archeologiczne"},
        {"zabytki_pomniki_historii_NID", "NID – Pomniki Historii"},
        {"zabytki_unesco_NID", "NID – Obiekty UNESCO w Polsce"},
        {"zabytki_groby_wojenne_NID", "NID – Groby i cmentarze wojenne"},
        {"zabytki_ewidencja_nieruchomych_NID", "NID – Ewidencja zabytków nieruchomych"},
        {"zabytki_ewidencja_archeo_NID", "NID – Ewidencja zabytków archeologicznych"},
        {"zabytki_zasiegi_NID", "NID – Zasięgi ochrony konserwatorskiej"},
        {"mapy_historyczne_IHPAN", "IH PAN – Mapy historyczne (Cenagis)"},
        {"czechy_orto_WMTS", "ČÚZK Czechy – Ortofotomapa (WMTS)"},
        {"czechy_topo_WMTS", "ČÚZK Czechy – Mapa topograficzna (WMTS)"},
        {"czechy_ztm10_wms", "ČÚZK Czechy – Mapa ZTM 1:10 000"},
        {"czechy_ztm25_wms", "ČÚZK Czechy – Mapa ZTM 1:25 000"},
        {"czechy_ztm50_wms", "ČÚZK Czechy – Mapa ZTM 1:50 000"},
        {"slowacja_orto_wms", "ZBGIS Słowacja – Ortofotomapa"},
        {"slowacja_dmr_wms", "ZBGIS Słowacja – Cieniowanie rzeźby DMR"},
        {"slowacja_zmsr_wms", "ZBGIS Słowacja – Mapa topograficzna ZMSR"},
        {"slowacja_freemap", "Freemap Słowacja – Mapa turystyczna"},
        {"niemcy_basemapde_WMTS", "Niemcy – Basemap.de (WMTS)"},
        {"niemcy_brandenburgia_sdop_wms", "Niemcy Brandenburgia – Ortofotomapa"},
        {"niemcy_brandenburgia_dgm_wms", "Niemcy Brandenburgia – Model terenu DGM"},
        {"niemcy_brandenburgia_dtk10_wms", "Niemcy Brandenburgia – Mapa DTK10 (1:10 000)"},
        {"niemcy_brandenburgia_dtk25_wms", "Niemcy Brandenburgia – Mapa DTK25 (1:25 000)"},
        {"niemcy_brandenburgia_dtk50_wms", "Niemcy Brandenburgia – Mapa DTK50 (1:50 000)"},
        {"niemcy_saksonia_dop_wms", "Niemcy Saksonia – Ortofotomapa"},
        {"niemcy_saksonia_dtk_wms", "Niemcy Saksonia – Mapa topograficzna DTK"},
        {"niemcy_saksonia_hoehe_wms", "Niemcy Saksonia – Cieniowanie i rzeźba terenu"},
        {"litwa_inspire_wms", "Litwa – Geoportal INSPIRE"}
    }

    Private ReadOnly _zrodla As Dictionary(Of String, PlikWarstw)
    Private ReadOnly _juzDodane As ZestawWarstw
    Private ReadOnly _wszystkieElementy As New List(Of ElementWarstwy)()
    Private ReadOnly _txtSzukaj As New TextBox()
    Private ReadOnly _btnRozwin As New Button()
    Private ReadOnly _btnZwin As New Button()
    Private ReadOnly _tvWarstwy As New TreeView()
    Private ReadOnly _lblInfo As New Label()
    Private ReadOnly _btnDodaj As New Button()
    Private ReadOnly _btnAnuluj As New Button()
    Private _ikonyPodgladuDrzewa As PomocnikIkonPodgladuDrzewa

    Private _wybranaWarstwa As WarstwaWybrana = Nothing

    Public Sub New(zrodla As Dictionary(Of String, PlikWarstw), Optional juzDodane As ZestawWarstw = Nothing)
        _zrodla = zrodla
        _juzDodane = juzDodane
        Text = "Wybierz warstwę do dodania do kompozycji"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(640, 560)
        AutoScaleMode = AutoScaleMode.Font

        Dim lblInstrukcja As New Label With {
            .Location = New Point(12, 10),
            .Size = New Size(616, 26),
            .Text = "Wybierz serwer i warstwę mapy z drzewka. Możesz szybko przefiltrować listę wpisując szukaną frazę."
        }
        Controls.Add(lblInstrukcja)

        _txtSzukaj.Name = "txtSzukaj"
        _txtSzukaj.Location = New Point(12, 40)
        _txtSzukaj.Size = New Size(430, 24)
        _txtSzukaj.PlaceholderText = "Filtruj (np. cieniowanie, ortofotomapa, bdot, szlaki, działki)..."
        AddHandler _txtSzukaj.TextChanged, AddressOf txtSzukaj_TextChanged
        Controls.Add(_txtSzukaj)

        _btnRozwin.Name = "btnRozwinWszystkie"
        _btnRozwin.Location = New Point(448, 39)
        _btnRozwin.Size = New Size(88, 26)
        _btnRozwin.Text = "Rozwiń"
        AddHandler _btnRozwin.Click, AddressOf btnRozwin_Click
        Controls.Add(_btnRozwin)

        _btnZwin.Name = "btnZwinWszystkie"
        _btnZwin.Location = New Point(540, 39)
        _btnZwin.Size = New Size(88, 26)
        _btnZwin.Text = "Zwiń"
        AddHandler _btnZwin.Click, AddressOf btnZwin_Click
        Controls.Add(_btnZwin)

        _tvWarstwy.Name = "tvWarstwy"
        _tvWarstwy.Location = New Point(12, 72)
        _tvWarstwy.Size = New Size(616, 370)
        _tvWarstwy.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        _tvWarstwy.ShowLines = True
        _tvWarstwy.ShowPlusMinus = True
        _tvWarstwy.ShowRootLines = True
        _tvWarstwy.HideSelection = False
        _tvWarstwy.HotTracking = True
        AddHandler _tvWarstwy.AfterSelect, AddressOf tvWarstwy_AfterSelect
        AddHandler _tvWarstwy.NodeMouseDoubleClick, AddressOf tvWarstwy_NodeMouseDoubleClick
        Controls.Add(_tvWarstwy)

        _lblInfo.Name = "lblInfo"
        _lblInfo.Location = New Point(12, 448)
        _lblInfo.Size = New Size(616, 62)
        _lblInfo.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular)
        _lblInfo.ForeColor = Color.DimGray
        _lblInfo.Text = "Rozwiń gałąź serwera i zaznacz warstwę, którą chcesz dodać do kompozycji mapy."
        Controls.Add(_lblInfo)

        _btnDodaj.Name = "btnDodaj"
        _btnDodaj.Location = New Point(386, 518)
        _btnDodaj.Size = New Size(130, 30)
        _btnDodaj.Text = "Dodaj warstwę"
        _btnDodaj.Enabled = False
        _btnDodaj.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        AddHandler _btnDodaj.Click, AddressOf btnDodaj_Click
        Controls.Add(_btnDodaj)

        _btnAnuluj.Name = "btnAnuluj"
        _btnAnuluj.Location = New Point(522, 518)
        _btnAnuluj.Size = New Size(106, 30)
        _btnAnuluj.Text = "Anuluj"
        _btnAnuluj.DialogResult = DialogResult.Cancel
        Controls.Add(_btnAnuluj)

        AcceptButton = _btnDodaj
        CancelButton = _btnAnuluj

        ZaladujWarstwyZeZrodel()
        ZbudujDrzewo("")

        _ikonyPodgladuDrzewa = New PomocnikIkonPodgladuDrzewa(
            _tvWarstwy,
            Program.PobierzMagazynGalerii(Form1.TrybTestowyBezSieci),
            Function(wezel)
                Dim el = TryCast(wezel?.Tag, ElementWarstwy)
                If el Is Nothing Then Return ("", "")
                Return (Path.GetFileNameWithoutExtension(el.ZbiorKlucz), el.NazwaTechniczna)
            End Function)
    End Sub

    Public ReadOnly Property WybranaWarstwa As WarstwaWybrana
        Get
            Return _wybranaWarstwa
        End Get
    End Property

    Private Shared Function DajCzytelnaNazweSerwera(zbiorKlucz As String, plik As PlikWarstw) As String
        Dim czystyKlucz = Path.GetFileNameWithoutExtension(zbiorKlucz)

        Dim znana As String = Nothing
        If _znaneNazwySerwerow.TryGetValue(czystyKlucz, znana) Then
            Return znana
        End If

        If plik IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(plik.Tytul) AndAlso
           Not String.Equals(plik.Tytul, czystyKlucz, StringComparison.OrdinalIgnoreCase) Then
            Return plik.Tytul
        End If

        ' Formatowanie awaryjne: zamiana podkreśleń na spacje i czytelny zapis
        Dim nazwaLadna = czystyKlucz.Replace("_", " ").Trim()
        If plik IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(plik.Adres) Then
            Try
                Dim uri As New Uri(plik.Adres)
                nazwaLadna &= " (" & uri.Host & ")"
            Catch
            End Try
        End If
        Return nazwaLadna
    End Function

    Private Sub ZaladujWarstwyZeZrodel()
        _wszystkieElementy.Clear()
        If _zrodla Is Nothing Then Exit Sub

        Dim katalog = KatalogWarstw.PobierzDomyslny()
        For Each kv In _zrodla
            Dim zbiorKlucz = kv.Key
            Dim plik = kv.Value
            If plik Is Nothing OrElse String.IsNullOrWhiteSpace(plik.Adres) Then Continue For

            Dim czystyKlucz = Path.GetFileNameWithoutExtension(zbiorKlucz)
            Dim tytulSerwera = DajCzytelnaNazweSerwera(zbiorKlucz, plik)

            For Each w In plik.Warstwy
                If String.IsNullOrWhiteSpace(w.Nazwa) Then Continue For

                Dim nazwaWyswietlana = katalog.ZnajdzNazwe(czystyKlucz, w.Nazwa)
                If String.IsNullOrWhiteSpace(nazwaWyswietlana) Then
                    nazwaWyswietlana = If(Not String.IsNullOrWhiteSpace(w.Tytul), w.Tytul, w.Nazwa)
                End If

                Dim px As Double = 0
                If Not String.IsNullOrEmpty(w.RozmiarPiksela) Then
                    Double.TryParse(w.RozmiarPiksela, NumberStyles.Float, CultureInfo.InvariantCulture, px)
                End If

                Dim nakladka = plik.OpcjeKafli(w.Nazwa).Nakladka
                Dim juzDodana = (_juzDodane IsNot Nothing AndAlso _juzDodane.Warstwy.Any(Function(x) x.TaSama(plik.Adres, w.Nazwa)))

                _wszystkieElementy.Add(New ElementWarstwy With {
                    .NazwaWyswietlana = nazwaWyswietlana,
                    .NazwaTechniczna = w.Nazwa,
                    .ZbiorNazwa = tytulSerwera,
                    .ZbiorKlucz = zbiorKlucz,
                    .AdresSerwera = plik.Adres,
                    .TypUslugi = plik.EfektywnyTyp,
                    .ZalecanyPiksel = px,
                    .JestNakladka = nakladka,
                    .IgnorujSsl = plik.IgnorujSsl,
                    .CzyJuzDodana = juzDodana
                })
            Next
        Next
    End Sub

    Private Sub ZbudujDrzewo(fraza As String)
        _tvWarstwy.BeginUpdate()
        _tvWarstwy.Nodes.Clear()

        Dim czyFiltrowanie = Not String.IsNullOrWhiteSpace(fraza)
        Dim frazaTrim = If(czyFiltrowanie, fraza.Trim(), "")

        ' Grupowanie elementów według serwerów
        Dim grupySerwerow = _wszystkieElementy.
            GroupBy(Function(el) el.ZbiorNazwa).
            OrderBy(Function(g) g.Key)

        Dim pierwszyPasujacyWezelWarstwy As TreeNode = Nothing
        Dim liczbaDopasowanychWarstw As Integer = 0

        For Each g In grupySerwerow
            Dim serwerNazwa = g.Key
            Dim warstwySerwera = g.OrderBy(Function(x) x.NazwaWyswietlana).ToList()

            ' Filtrowanie warstw w serwerze
            Dim pasujaceWarstwy As List(Of ElementWarstwy)
            If czyFiltrowanie Then
                pasujaceWarstwy = warstwySerwera.Where(Function(el)
                                                           Return FiltrWarstw.Pasuje(el.ZbiorNazwa, el.NazwaWyswietlana & " " & el.NazwaTechniczna, frazaTrim)
                                                       End Function).ToList()
            Else
                pasujaceWarstwy = warstwySerwera
            End If

            If pasujaceWarstwy.Count = 0 Then Continue For

            ' Węzeł główny: serwer
            Dim opisLiczby = If(pasujaceWarstwy.Count = 1, "1 warstwa", pasujaceWarstwy.Count & " warstwy")
            If pasujaceWarstwy.Count >= 5 Then opisLiczby = pasujaceWarstwy.Count & " warstw"

            If _czcionkaPogrubiona Is Nothing Then
                _czcionkaPogrubiona = New Font(_tvWarstwy.Font, FontStyle.Bold)
            End If
            Dim nodeSerwer As New TreeNode(serwerNazwa & "  [" & opisLiczby & "]") With {
                .Tag = Nothing,
                .NodeFont = _czcionkaPogrubiona,
                .ForeColor = Color.FromArgb(15, 45, 95)
            }

            For Each el In pasujaceWarstwy
                Dim tekstWarstwy = el.NazwaWyswietlana
                If el.CzyJuzDodana Then
                    tekstWarstwy &= "  (już w kompozycji)"
                ElseIf el.JestNakladka Then
                    tekstWarstwy &= "  [nakładka]"
                End If

                Dim nodeWarstwa As New TreeNode(tekstWarstwy) With {
                    .Tag = el,
                    .ForeColor = If(el.CzyJuzDodana, Color.Gray, Color.Black)
                }

                nodeSerwer.Nodes.Add(nodeWarstwa)
                liczbaDopasowanychWarstw += 1

                If pierwszyPasujacyWezelWarstwy Is Nothing AndAlso Not el.CzyJuzDodana Then
                    pierwszyPasujacyWezelWarstwy = nodeWarstwa
                End If
            Next

            _tvWarstwy.Nodes.Add(nodeSerwer)

            ' Gdy użytkownik filtruje, automatycznie rozwiń dopasowane gałęzie
            If czyFiltrowanie Then
                nodeSerwer.Expand()
            End If
        Next

        _tvWarstwy.EndUpdate()

        If czyFiltrowanie Then
            If pierwszyPasujacyWezelWarstwy IsNot Nothing Then
                _tvWarstwy.SelectedNode = pierwszyPasujacyWezelWarstwy
                _tvWarstwy.SelectedNode.EnsureVisible()
                AktualizujStanWyboru(pierwszyPasujacyWezelWarstwy)
            Else
                _btnDodaj.Enabled = False
                _lblInfo.Text = "Brak warstw pasujących do wpisanej frazy '" & frazaTrim & "'."
                _lblInfo.ForeColor = Color.FromArgb(180, 50, 50)
            End If
        Else
            _btnDodaj.Enabled = False
            _lblInfo.Text = "Wybierz serwer (gałąź) i kliknij konkretną warstwę, którą chcesz dodać do kompozycji."
            _lblInfo.ForeColor = Color.DimGray
        End If
    End Sub

    Private Sub txtSzukaj_TextChanged(sender As Object, e As EventArgs)
        ZbudujDrzewo(_txtSzukaj.Text)
    End Sub

    Private Sub btnRozwin_Click(sender As Object, e As EventArgs)
        _tvWarstwy.BeginUpdate()
        _tvWarstwy.ExpandAll()
        _tvWarstwy.EndUpdate()
    End Sub

    Private Sub btnZwin_Click(sender As Object, e As EventArgs)
        _tvWarstwy.BeginUpdate()
        _tvWarstwy.CollapseAll()
        _tvWarstwy.EndUpdate()
    End Sub

    Private Sub tvWarstwy_AfterSelect(sender As Object, e As TreeViewEventArgs)
        AktualizujStanWyboru(e.Node)
    End Sub

    Private Sub AktualizujStanWyboru(node As TreeNode)
        Dim el = TryCast(node?.Tag, ElementWarstwy)
        If el IsNot Nothing Then
            If el.CzyJuzDodana Then
                _btnDodaj.Enabled = False
                _lblInfo.ForeColor = Color.FromArgb(160, 100, 20)
                _lblInfo.Text = "Warstwa: " & el.NazwaWyswietlana & " (" & el.NazwaTechniczna & ")" & vbCrLf &
                                "Ta warstwa znajduje się już w Twojej kompozycji mapy."
            Else
                _btnDodaj.Enabled = True
                _lblInfo.ForeColor = Color.FromArgb(10, 30, 60)
                Dim typStr = If(el.TypUslugi = TypUslugiMapowej.Wmts, "WMTS (kafle)", If(el.TypUslugi = TypUslugiMapowej.Xyz, "XYZ (kafle)", "WMS (obraz)"))
                Dim zalecanaRozdz = If(el.ZalecanyPiksel > 0, " | Zalecana rozdzielczość: " & el.ZalecanyPiksel.ToString("0.##", CultureInfo.InvariantCulture) & " m/px", "")
                Dim nakladkaInfo = If(el.JestNakladka, " [nakładka przezroczysta]", "")

                _lblInfo.Text = "Tytuł: " & el.NazwaWyswietlana & nakladkaInfo & vbCrLf &
                                "Identyfikator serwera: " & el.NazwaTechniczna & " (" & typStr & ")" & zalecanaRozdz & vbCrLf &
                                "Serwer: " & el.ZbiorNazwa & " | " & el.AdresSerwera
            End If
        Else
            _btnDodaj.Enabled = False
            _lblInfo.ForeColor = Color.DimGray
            _lblInfo.Text = If(node IsNot Nothing,
                               "Wybrano serwer: " & node.Text & vbCrLf & "Rozwiń gałąź klikając [+] i zaznacz warstwę, którą chcesz dodać do kompozycji.",
                               "Rozwiń gałąź serwera i zaznacz warstwę, którą chcesz dodać do kompozycji mapy.")
        End If
    End Sub

    Private Sub tvWarstwy_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs)
        If _ikonyPodgladuDrzewa IsNot Nothing AndAlso _ikonyPodgladuDrzewa.CzyPunktNaIkonie(e.Location) Then Exit Sub
        If e.Node IsNot Nothing AndAlso e.Node.Tag IsNot Nothing Then
            Dim el = TryCast(e.Node.Tag, ElementWarstwy)
            If el IsNot Nothing AndAlso Not el.CzyJuzDodana Then
                btnDodaj_Click(sender, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Sub btnDodaj_Click(sender As Object, e As EventArgs)
        Dim el = TryCast(_tvWarstwy.SelectedNode?.Tag, ElementWarstwy)
        If el Is Nothing OrElse el.CzyJuzDodana Then Exit Sub

        ' Domyślny tryb mieszania: Rzeźba terenu dla cieniowania (płaski teren bez szarej zasłony), Zwykły dla pozostałych
        Dim tryb = If(el.NazwaTechniczna.IndexOf("cien", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                      el.NazwaWyswietlana.IndexOf("cieniow", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                      el.NazwaTechniczna.IndexOf("shaded", StringComparison.OrdinalIgnoreCase) >= 0,
                      TrybMieszania.RzezbaTerenu, TrybMieszania.Zwykly)

        ' Domyślne krycie: 70% dla cieniowania, 100% dla reszty
        Dim krycie = If(tryb = TrybMieszania.RzezbaTerenu, 70, 100)

        _wybranaWarstwa = New WarstwaWybrana With {
            .Nazwa = el.NazwaTechniczna,
            .Adres = el.AdresSerwera,
            .Zbior = el.ZbiorKlucz,
            .ZalecanyPiksel = el.ZalecanyPiksel,
            .TypZrodla = el.TypUslugi,
            .Nakladka = el.JestNakladka,
            .IgnorujSsl = el.IgnorujSsl,
            .Krycie = krycie,
            .TrybMieszania = tryb
        }

        DialogResult = DialogResult.OK
        Close()
    End Sub

End Class
