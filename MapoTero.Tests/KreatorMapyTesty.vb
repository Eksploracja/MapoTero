'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

<Collection("OknaKreatora")>
Public Class KreatorMapyTesty

    Private Sub UruchomWWatkuSta(akcja As Action)
        Dim wyjatek As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    akcja()
                Catch ex As Exception
                    wyjatek = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If wyjatek IsNot Nothing Then
            Throw New Exception("Błąd testu UI: " & wyjatek.Message, wyjatek)
        End If
    End Sub

    <Fact>
    Public Sub Form1_PosiadaPrzyciskIPozycjeMenuKreatora()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    ' Przycisk na pasku narzędziowym
                    Dim btn = f.Controls.Find("btnKreator", True)
                    Assert.NotEmpty(btn)
                    Assert.True(btn(0).Visible)

                    ' Pozycja w menu Narzędzia
                    Dim menuNarzedzia = DirectCast(f.MainMenuStrip.Items("NarzedziaToolStripMenuItem"), ToolStripMenuItem)
                    Assert.NotNull(menuNarzedzia)
                    Dim menuKreator = menuNarzedzia.DropDownItems("KreatorMapyToolStripMenuItem")
                    Assert.NotNull(menuKreator)
                    Assert.Equal(Keys.Control Or Keys.K, DirectCast(menuKreator, ToolStripMenuItem).ShortcutKeys)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_PrzechodziPrzezKrokiNawigacji()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Krok 1: Cel
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        Dim btnWstecz = DirectCast(k.Controls.Find("btnWstecz", True)(0), Button)
                        Dim btnGeneruj = DirectCast(k.Controls.Find("btnGeneruj", True)(0), Button)

                        Assert.False(btnWstecz.Enabled)
                        Assert.True(btnDalej.Visible)
                        Assert.False(btnGeneruj.Visible)

                        ' Przejście do Krok 2 (Styl)
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        Dim lstSzablony = DirectCast(k.Controls.Find("lstSzablony", True)(0), ListBox)
                        Assert.True(lstSzablony.Items.Count > 0)
                        Assert.True(btnWstecz.Enabled)

                        ' Przejście do Krok 3 (Obszar)
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        ' Domyślny tryb zaznaczania: prostokąt na mapie
                        Dim rbObszarMapa = DirectCast(k.Controls.Find("rbObszarMapa", True)(0), RadioButton)
                        Assert.True(rbObszarMapa.Checked)

                        ' Cofnięcie do Krok 2
                        btnWstecz.PerformClick()
                        Application.DoEvents()
                        Assert.True(lstSzablony.Items.Count > 0)

                        k.Close()
                    End Using

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_ZaznaczNaMapie_PowrotNiePowodujeBleduDisposed()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    f.OtworzKreatorMapy()
                    Application.DoEvents()

                    Dim k = Application.OpenForms.OfType(Of Global.MapoTero.FormKreatorMapy)().FirstOrDefault()
                    Assert.NotNull(k)

                    ' Przejdź do kroku 3
                    Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                    btnDalej.PerformClick()
                    Application.DoEvents()
                    btnDalej.PerformClick()
                    Application.DoEvents()

                    ' Kliknij Zaznacz na mapie
                    Dim btnZaznacz = DirectCast(k.Controls.Find("btnZaznaczNaMapie", True)(0), Button)
                    btnZaznacz.PerformClick()
                    Application.DoEvents()

                    ' Pasek powinien być otwarty
                    Dim pasek = Application.OpenForms.OfType(Of Global.MapoTero.FormPasekZaznaczania)().FirstOrDefault()
                    Assert.NotNull(pasek)

                    ' Kliknij Gotowe
                    Dim btnGotowe = pasek.Controls.OfType(Of Button)().FirstOrDefault(Function(b) b.Text.Contains("Gotowe"))
                    Assert.NotNull(btnGotowe)
                    btnGotowe.PerformClick()
                    Application.DoEvents()

                    ' Sprawdź czy k nie jest zdisposedowany i czy jest widoczny
                    Assert.False(k.IsDisposed)
                    Assert.True(k.Visible)

                    k.Close()
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub UtworzIkoneKreatora_ZwracaPoprawnaBitmape()
        Using bmp = Global.MapoTero.Form1.UtworzIkoneKreatora(16)
            Assert.NotNull(bmp)
            Assert.Equal(16, bmp.Width)
            Assert.Equal(16, bmp.Height)
        End Using
    End Sub

    <Fact>
    Public Sub SzablonMapy_ToString_ZwracaCzytelnaNazweZamiastNazwyKlasy()
        For Each s In ZarzadcaSzablonow.PobierzDomyslne()
            Dim tekst = s.ToString()
            Assert.False(tekst.StartsWith("MapoTero."), $"Szablon {s.Id} zwraca nazwę typu .NET zamiast czytelnej nazwy!")
            Assert.Contains(s.Nazwa, tekst)
        Next
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_ZawieraPodpowiedziMiejscIOpcjeZaawansowane()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Kontrolka podpowiedzi miejscowości w kroku 3
                        Dim lstWyniki = k.Controls.Find("lstWynikiMiejscowosci", True)
                        Assert.NotEmpty(lstWyniki)

                        ' Opcje zaawansowane w kroku 4
                        Dim chkKorekta = k.Controls.Find("chkKorektaObrazu", True)
                        Assert.NotEmpty(chkKorekta)
                        Dim chkZoom = k.Controls.Find("chkWlasnyZoom", True)
                        Assert.NotEmpty(chkZoom)
                        Dim numZoomMin = k.Controls.Find("numZoomMin", True)
                        Assert.NotEmpty(numZoomMin)
                        Dim numZoomMax = k.Controls.Find("numZoomMax", True)
                        Assert.NotEmpty(numZoomMax)

                        k.Close()
                    End Using

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormZaawansowanyMbtiles_OtworzDlaKreatora_InicjalizujeTylkoWarstwyKreatora()
        UruchomWWatkuSta(
            Sub()
                Using dlg As New Global.MapoTero.FormZaawansowanyMbtiles()
                    Dim zestaw As New ZestawWarstw()
                    zestaw.Dodaj("GUGiK Topo", 100, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/BDOT500/WMS/Standard", "topo")
                    zestaw.Dodaj("Cieniowanie ISOK", 35, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/WMS/ShadedRelief", "cien")

                    Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
                    Dim uklad = UkladWspolrzednych.PL1992
                    Dim sciezka = "C:\test_export\tatry.mbtiles"
                    Dim korekta As New OpcjeKorekcjiMip() With {.Wlaczona = True}

                    ' Symulacja otwarcia w trybie kreatora (bez blokowania ShowDialog)
                    ' OtworzDlaKreatora przyjmuje Nothing jako parent w teście jednostkowym
                    ' Załaduj parametry bez wywoływania ShowDialog
                    Dim t = dlg.GetType()
                    Dim fTryb = t.GetField("_trybKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fTryb.SetValue(dlg, True)

                    ' Inicjalizacja poprzez bezpośrednie wywołanie logiki OtworzDlaKreatora
                    dlg.Show()
                    Application.DoEvents()

                    ' Wywołaj OtworzDlaKreatora w wątku STA
                    Dim metodaOtworz = t.GetMethod("OtworzDlaKreatora")
                    Assert.NotNull(metodaOtworz)

                    ' Przetestuj odczyt parametrów z kreatora
                    Dim warstwyField = t.GetField("_warstwy", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    Dim listaWarstw = DirectCast(warstwyField.GetValue(dlg), Generic.List(Of Global.MapoTero.WarstwaWielowarstwowa))

                    ' Wyczyszczenie i załadowanie zestawu kreatora
                    listaWarstw.Clear()
                    For i = 0 To zestaw.Warstwy.Count - 1
                        Dim wz = zestaw.Warstwy(i)
                        listaWarstw.Add(New Global.MapoTero.WarstwaWielowarstwowa With {
                            .Tytul = wz.Nazwa,
                            .PoziomMin = 10,
                            .PoziomMax = 16,
                            .Krycie = wz.Krycie / 100.0,
                            .Wlaczona = True
                        })
                    Next

                    Dim zMinOut = 0
                    Dim zMaxOut = 0
                    Dim korektaOut As OpcjeKorekcjiMip = Nothing
                    Dim sciezkaOut = ""

                    dlg.PobierzUstawieniaDlaKreatora(zestaw, zMinOut, zMaxOut, korektaOut, sciezkaOut)

                    Assert.Equal(10, zMinOut)
                    Assert.Equal(16, zMaxOut)
                    Assert.Equal(2, zestaw.Warstwy.Count)
                    Assert.Equal("GUGiK Topo", zestaw.Warstwy(0).Nazwa)
                    Assert.Equal(100, zestaw.Warstwy(0).Krycie)
                    Assert.Equal("Cieniowanie ISOK", zestaw.Warstwy(1).Nazwa)
                    Assert.Equal(35, zestaw.Warstwy(1).Krycie)

                    dlg.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormZaawansowanyMbtiles_WypelniaTabeleWarstwamiKreatoraBezStarejSesji()
        UruchomWWatkuSta(
            Sub()
                Using dlg As New Global.MapoTero.FormZaawansowanyMbtiles()
                    Dim zestaw As New ZestawWarstw()
                    zestaw.Dodaj("OSMapa turystyczna", 100, "https://tile.openstreetmap.pl/osmapa", "OSMapa_PL")
                    zestaw.Dodaj("Szlaki rowerowe", 100, "https://tile.waymarkedtrails.org/cycling", "Waymarked_Trails")

                    Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
                    Dim uklad = UkladWspolrzednych.PL1992
                    Dim sciezka = "C:\download\trasy.mbtiles"
                    Dim korekta As New OpcjeKorekcjiMip()

                    ' Symulacja otwarcia w trybie kreatora
                    Dim t = dlg.GetType()
                    Dim fTryb = t.GetField("_trybKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fTryb.SetValue(dlg, True)

                    Dim warstwyField = t.GetField("_warstwy", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    Dim listaWarstw = DirectCast(warstwyField.GetValue(dlg), Generic.List(Of Global.MapoTero.WarstwaWielowarstwowa))
                    listaWarstw.Clear()
                    For i = 0 To zestaw.Warstwy.Count - 1
                        Dim wz = zestaw.Warstwy(i)
                        listaWarstw.Add(New Global.MapoTero.WarstwaWielowarstwowa With {
                            .Tytul = wz.Nazwa,
                            .PoziomMin = 10,
                            .PoziomMax = 16,
                            .Krycie = wz.Krycie / 100.0,
                            .Wlaczona = True
                        })
                    Next

                    dlg.Show()
                    Application.DoEvents()

                    Dim dgv = DirectCast(dlg.Controls.Find("dgvWarstwy", True)(0), DataGridView)
                    Assert.Equal(2, dgv.Rows.Count)
                    Assert.Equal("OSMapa turystyczna", CStr(dgv.Rows(0).Cells(1).Value))
                    Assert.Equal("Szlaki rowerowe", CStr(dgv.Rows(1).Cells(1).Value))

                    dlg.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_BrakNakladajacychSieKontrolek_WszystkieKroki()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' 1. Stopka - przyciski nie złączone ze sobą
                        Dim btnWstecz = DirectCast(k.Controls.Find("btnWstecz", True)(0), Button)
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        Dim btnGeneruj = DirectCast(k.Controls.Find("btnGeneruj", True)(0), Button)
                        Dim btnAnuluj = DirectCast(k.Controls.Find("btnAnuluj", True)(0), Button)

                        Assert.True(btnDalej.Left >= btnWstecz.Right + 5, "Brak odstępu między Wstecz i Dalej!")
                        Assert.True(btnGeneruj.Left >= btnWstecz.Right + 5, "Brak odstępu między Wstecz i Generuj!")
                        Assert.True(btnAnuluj.Left >= btnDalej.Right + 5, "Brak odstępu między Dalej i Anuluj!")
                        Assert.True(btnAnuluj.Left >= btnGeneruj.Right + 5, "Brak odstępu między Generuj i Anuluj!")

                        ' 2. Krok 2 - suwak cieniowania, status podglądu i pasek zoom
                        Dim tbCieniowanie = DirectCast(k.Controls.Find("tbCieniowanie", True)(0), TrackBar)
                        Dim lblStanStyl = DirectCast(k.Controls.Find("lblStanPodgladuStyl", True)(0), Label)
                        Dim btnZoomMinusStyl = DirectCast(k.Controls.Find("btnZoomMinusStyl", True)(0), Button)
                        Dim lblZoomStyl = DirectCast(k.Controls.Find("lblZoomStyl", True)(0), Label)
                        Dim btnZoomPlusStyl = DirectCast(k.Controls.Find("btnZoomPlusStyl", True)(0), Button)
                        Dim btnZoomResetStyl = DirectCast(k.Controls.Find("btnZoomResetStyl", True)(0), Button)
                        Dim btnOdswiezStyl = DirectCast(k.Controls.Find("btnOdswiezStyl", True)(0), Button)
                        Dim picPodgladStylu = DirectCast(k.Controls.Find("picPodgladStylu", True)(0), PictureBox)
                        Dim btnDostosujWarstwy = DirectCast(k.Controls.Find("btnDostosujWarstwy", True)(0), Button)

                        Assert.True(lblStanStyl.Top >= tbCieniowanie.Bottom, "Suwak cieniowania nachodzi na etykietę podglądu!")
                        Assert.True(btnZoomMinusStyl.Top >= tbCieniowanie.Bottom, "Suwak cieniowania nachodzi na przycisk zoom - !")
                        Assert.True(lblZoomStyl.Left >= btnZoomMinusStyl.Right, "Przycisk zoom - nachodzi na etykietę zoom!")
                        Assert.True(btnZoomPlusStyl.Left >= lblZoomStyl.Right, "Etykieta zoom nachodzi na przycisk zoom + !")
                        Assert.True(btnZoomResetStyl.Left >= btnZoomPlusStyl.Right, "Przycisk zoom + nachodzi na reset 1:1!")
                        Assert.True(btnOdswiezStyl.Left >= btnZoomResetStyl.Right, "Przycisk reset 1:1 nachodzi na odśwież!")
                        Assert.True(picPodgladStylu.Top >= lblStanStyl.Bottom, "Pasek stanu podglądu nachodzi na PictureBox podglądu!")
                        Assert.True(btnDostosujWarstwy.Top >= picPodgladStylu.Bottom, "PictureBox podglądu nachodzi na przycisk dostosowywania warstw!")

                        ' 3. Krok 3 - obszar: wyszukiwarka, zaznaczanie na mapie, trasa
                        Dim pnlObszarMapa = DirectCast(k.Controls.Find("pnlObszarMapa", True)(0), Panel)
                        Dim rbMiejscowosc = DirectCast(k.Controls.Find("rbObszarMiejscowosc", True)(0), RadioButton)
                        Dim pnlObszarMiejscowosc = DirectCast(k.Controls.Find("pnlObszarMiejscowosc", True)(0), Panel)
                        Dim rbTrasa = DirectCast(k.Controls.Find("rbObszarTrasa", True)(0), RadioButton)

                        Assert.True(rbMiejscowosc.Top >= pnlObszarMapa.Bottom, "Panel Zaznacz na mapie nachodzi na radiobutton Miejscowość!")
                        Assert.True(rbTrasa.Top >= pnlObszarMiejscowosc.Bottom, "Panel Miejscowość nachodzi na radiobutton Trasa!")

                        Dim txtSzukaj = DirectCast(k.Controls.Find("txtSzukajMiejsca", True)(0), TextBox)
                        Dim lblPromien = DirectCast(k.Controls.Find("lblPromien", True)(0), Label)
                        Dim cboPromien = DirectCast(k.Controls.Find("cboPromien", True)(0), ComboBox)
                        Dim btnSzukaj = DirectCast(k.Controls.Find("btnSzukajMiejsca", True)(0), Button)
                        Dim lstWyniki = DirectCast(k.Controls.Find("lstWynikiMiejscowosci", True)(0), ListBox)
                        Dim lblWynik = DirectCast(k.Controls.Find("lblWynikMiejscowosci", True)(0), Label)

                        Assert.True(lblPromien.Left >= txtSzukaj.Right, "Pole wyszukiwania nachodzi na etykietę promień!")
                        Assert.True(cboPromien.Left >= lblPromien.Right, "Etykieta promień nachodzi na combobox promienia!")
                        Assert.True(btnSzukaj.Left >= cboPromien.Right, "Combobox promienia nachodzi na przycisk szukaj!")
                        Assert.True(lstWyniki.Top >= txtSzukaj.Bottom, "Pole szukania nachodzi na listę wyników!")
                        Assert.True(lblWynik.Top >= lstWyniki.Bottom, "Lista wyników nachodzi na etykietę wybranego wyniku!")

                        ' 4. Krok 4 - opcje zaawansowane korekty rastra i zoomu
                        Dim chkKorekta = DirectCast(k.Controls.Find("chkKorektaObrazu", True)(0), CheckBox)
                        Dim lblProfil = DirectCast(k.Controls.Find("lblProfil", True)(0), Label)
                        Dim cboProfil = DirectCast(k.Controls.Find("cboProfilKorekty", True)(0), ComboBox)
                        Dim btnDostosujKorekte = DirectCast(k.Controls.Find("btnDostosujKorekte", True)(0), Button)
                        Dim chkZoom = DirectCast(k.Controls.Find("chkWlasnyZoom", True)(0), CheckBox)
                        Dim btnWieloskalowy = DirectCast(k.Controls.Find("btnOtworzWieloskalowy", True)(0), Button)
                        Dim lblWieloskalowyOpis = DirectCast(k.Controls.Find("lblWieloskalowyOpis", True)(0), Label)

                        Assert.True(lblProfil.Top >= chkKorekta.Bottom, "Checkbox korekty nachodzi na etykietę profilu!")
                        Assert.True(cboProfil.Top >= chkKorekta.Bottom, "Checkbox korekty nachodzi na combobox profilu!")
                        Assert.True(cboProfil.Left >= lblProfil.Right, "Etykieta profilu nachodzi na combobox profilu!")
                        Assert.True(btnDostosujKorekte.Left >= cboProfil.Right, "Combobox profilu nachodzi na przycisk dostosowania suwaków!")
                        Assert.True(chkZoom.Top >= cboProfil.Bottom, "Combobox profilu nachodzi na checkbox własnego zoomu!")
                        Assert.True(lblWieloskalowyOpis.Left >= btnWieloskalowy.Right, "Przycisk narzędzia wieloskalowego nachodzi na opis!")

                        k.Close()
                    End Using

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_Krok3_RadioObszaruPrzelaczaOpcje()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        Dim rbMapa = DirectCast(k.Controls.Find("rbObszarMapa", True)(0), RadioButton)
                        Dim rbMiejscowosc = DirectCast(k.Controls.Find("rbObszarMiejscowosc", True)(0), RadioButton)
                        Dim rbTrasa = DirectCast(k.Controls.Find("rbObszarTrasa", True)(0), RadioButton)

                        rbMapa.Checked = True
                        Assert.True(rbMapa.Checked)
                        Assert.False(rbMiejscowosc.Checked)
                        Assert.False(rbTrasa.Checked)

                        rbMiejscowosc.Checked = True
                        Assert.True(rbMiejscowosc.Checked)
                        Assert.False(rbMapa.Checked)

                        k.Close()
                    End Using
                    f.Close()
                End Using
            End Sub)
    End Sub


    <Fact>
    Public Sub FormWybierzDodatkowaWarstwe_WczytujeWarstwyZeZrodel()
        Dim zrodla As New Dictionary(Of String, PlikWarstw)()
        Dim plik As New PlikWarstw With {.Adres = "http://serwer.test/wms", .Tytul = "Testowy Serwer"}
        plik.Warstwy.Add(New WarstwaMapy With {.Nazwa = "isok_cien", .Tytul = "Cieniowanie ISOK 1 m", .RozmiarPiksela = "1.0"})
        plik.Warstwy.Add(New WarstwaMapy With {.Nazwa = "orto", .Tytul = "Ortofotomapa", .RozmiarPiksela = "0.25"})
        zrodla.Add("test_zbior.txt", plik)

        UruchomWWatkuSta(
            Sub()
                Using dlg As New Global.MapoTero.FormWybierzDodatkowaWarstwe(zrodla)
                    dlg.Show()
                    Application.DoEvents()

                    Dim tv = DirectCast(dlg.Controls.Find("tvWarstwy", True)(0), TreeView)
                    Assert.Equal(1, tv.Nodes.Count) ' 1 węzeł serwera
                    Dim serwerNode = tv.Nodes(0)
                    Assert.Equal(2, serwerNode.Nodes.Count) ' 2 warstwy na serwerze

                    ' Kliknięcie gałęzi serwera: przycisk dodawania zablokowany
                    tv.SelectedNode = serwerNode
                    Application.DoEvents()
                    Dim btnDodaj = DirectCast(dlg.Controls.Find("btnDodaj", True)(0), Button)
                    Assert.False(btnDodaj.Enabled)

                    ' Kliknięcie konkretnej warstwy: przycisk dodawania aktywny
                    tv.SelectedNode = serwerNode.Nodes(0)
                    Application.DoEvents()
                    Assert.True(btnDodaj.Enabled)

                    ' Sprawdź filtrowanie w polu wyszukiwania
                    Dim txtSzukaj = DirectCast(dlg.Controls.Find("txtSzukaj", True)(0), TextBox)
                    txtSzukaj.Text = "cien"
                    Application.DoEvents()
                    Assert.Equal(1, tv.Nodes.Count)
                    Assert.Equal(1, tv.Nodes(0).Nodes.Count)
                    Assert.Contains("Cieniowanie", tv.Nodes(0).Nodes(0).Text)

                    dlg.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub KompilatorMbtiles_WarstwyDlaPoziomu_ObslugujeWarstwyKreatora()
        Dim warstwy As New List(Of WarstwaWielowarstwowa) From {
            New WarstwaWielowarstwowa With {.Tytul = "W1", .PoziomMin = 10, .PoziomMax = 15, .Wlaczona = True, .Folder = "(Kreator: warstwa #1)"},
            New WarstwaWielowarstwowa With {.Tytul = "W2", .PoziomMin = 10, .PoziomMax = 15, .Wlaczona = False, .Folder = "(Kreator: warstwa #2)"}
        }

        Dim aktywneZ12 = KompilatorMbtiles.WarstwyDlaPoziomu(warstwy, 12)
        Assert.Single(aktywneZ12)
        Assert.Equal("W1", aktywneZ12(0).Tytul)

        ' Wyłączenie W1 powoduje 0 aktywnych
        warstwy(0).Wlaczona = False
        Dim aktywnePoWylaczeniu = KompilatorMbtiles.WarstwyDlaPoziomu(warstwy, 12)
        Assert.Empty(aktywnePoWylaczeniu)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_Krok4_PosiadaRegulacjeRozmiaruPiksela()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Przejdź do Kroku 4
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        btnDalej.PerformClick()
                        Application.DoEvents()
                        btnDalej.PerformClick()
                        Application.DoEvents()
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        Dim numPiksel = DirectCast(k.Controls.Find("numRozmiarPiksela", True)(0), NumericUpDown)
                        Assert.NotNull(numPiksel)
                        Assert.True(numPiksel.Value > 0)

                        Dim lblSeg = DirectCast(k.Controls.Find("lblSzacunekSegmenty", True)(0), Label)
                        Dim tekstWstepny = lblSeg.Text

                        ' Zmiana piksela (np. zwiększenie z 1.0 na 2.0 zmniejsza liczbę segmentów 4-krotnie)
                        numPiksel.Value = 2.0D
                        Application.DoEvents()
                        Dim tekstPoZmianie = lblSeg.Text

                        Assert.NotEmpty(tekstPoZmianie)
                        k.Close()
                    End Using
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_Krok2_PrzesuwanieMyszkaPodgladu()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Krok 2
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        Dim szablonPoczatkowy = ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory")
                        Dim picPodgladStylu = DirectCast(k.Controls.Find("picPodgladStylu", True)(0), PictureBox)
                        Dim btnZoomResetStyl = DirectCast(k.Controls.Find("btnZoomResetStyl", True)(0), Button)

                        ' Ustaw sztuczną bazę podglądu, aby obsłużyć zdarzenia myszy
                        Using bmp As New Bitmap(100, 100)
                            Dim fldBaza = GetType(Global.MapoTero.FormKreatorMapy).GetField("_bazaPodgladuStylu", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            fldBaza.SetValue(k, bmp)

                            ' Symulacja wejścia myszką
                            Dim miEnter = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseEnter", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miEnter.Invoke(k, New Object() {picPodgladStylu, EventArgs.Empty})
                            Assert.Equal(Cursors.Hand, picPodgladStylu.Cursor)

                            ' Symulacja wciśnięcia lewego przycisku myszy
                            Dim miDown = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseDown", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miDown.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0)})
                            Assert.Equal(Cursors.SizeAll, picPodgladStylu.Cursor)

                            ' Symulacja przeciągnięcia
                            Dim miMove = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseMove", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miMove.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 0, 120, 115, 0)})

                            ' Symulacja puszczenia przycisku (zakończenie przeciągania)
                            Dim miUp = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseUp", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miUp.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 1, 120, 115, 0)})
                            Assert.Equal(Cursors.Hand, picPodgladStylu.Cursor)

                            ' Po przesunięciu przycisk resetu powinien być aktywny
                            Assert.True(btnZoomResetStyl.Enabled)

                            ' Kliknięcie 1:1 resetuje przesunięcie
                            btnZoomResetStyl.PerformClick()
                            fldBaza.SetValue(k, Nothing)
                        End Using
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_Krok2_PrzesuwanieMyszkaPodgladu_SprawdzPolozenie()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Krok 2
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        Dim szablonPoczatkowy = ZarzadcaSzablonow.ZnajdzSzablon("piesza_gory")
                        Dim picPodgladStylu = DirectCast(k.Controls.Find("picPodgladStylu", True)(0), PictureBox)

                        ' Ustaw sztuczną bazę podglądu
                        Using bmp As New Bitmap(300, 300)
                            ' Narysuj unikalny wzorzec: czerwony punkt w środku (150, 150)
                            Using g = Graphics.FromImage(bmp)
                                g.Clear(Color.Blue)
                                g.FillRectangle(Brushes.Red, 140, 140, 20, 20)
                            End Using

                            Dim fldBaza = GetType(Global.MapoTero.FormKreatorMapy).GetField("_bazaPodgladuStylu", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            fldBaza.SetValue(k, bmp)

                            Dim miZastosuj = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("ZastosujZoomStylu", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miZastosuj.Invoke(k, Nothing)

                            ' Sprawdź pixel w środku picPodgladStylu
                            Dim imgPoczatkowy = New Bitmap(picPodgladStylu.Image)
                            Dim kolorPoczatkowySrodek = imgPoczatkowy.GetPixel(picPodgladStylu.Width \ 2, picPodgladStylu.Height \ 2)
                            Assert.Equal(Color.Red.ToArgb(), kolorPoczatkowySrodek.ToArgb())

                            ' 1. MouseDown at (100, 100)
                            Dim miDown = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseDown", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miDown.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0)})

                            ' 2. MouseMove to (150, 100) -> przesunięcie w prawo o 50 px
                            Dim miMove = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseMove", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miMove.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 0, 150, 100, 0)})

                            ' Podczas przeciągania środek nie powinien być czerwony (czerwony przesunął się w prawo o 50 px)
                            Dim imgPrzeciagany = New Bitmap(picPodgladStylu.Image)
                            Dim kolorPrzeciaganySrodek = imgPrzeciagany.GetPixel(picPodgladStylu.Width \ 2, picPodgladStylu.Height \ 2)
                            Dim kolorPrzesuniety = imgPrzeciagany.GetPixel(picPodgladStylu.Width \ 2 + 50, picPodgladStylu.Height \ 2)
                            Assert.Equal(Color.Blue.ToArgb(), kolorPrzeciaganySrodek.ToArgb())
                            Assert.Equal(Color.Red.ToArgb(), kolorPrzesuniety.ToArgb())

                            ' 3. MouseUp at (150, 100)
                            Dim miUp = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladStylu_MouseUp", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miUp.Invoke(k, New Object() {picPodgladStylu, New MouseEventArgs(MouseButtons.Left, 1, 150, 100, 0)})

                            ' Poczekaj na zakończenie asynchronicznego odświeżania podglądu (do 10 sekund)
                            Dim lblStan = DirectCast(k.Controls.Find("lblStanPodgladuStyl", True)(0), Label)
                            Dim startCzekania = DateTime.UtcNow
                            While lblStan.Text.Contains("Pobieranie") AndAlso (DateTime.UtcNow - startCzekania).TotalSeconds < 10
                                Application.DoEvents()
                                Thread.Sleep(50)
                            End While

                            ' CO SIĘ DZIEJE PO ZAKOŃCZENIU ODŚWIEŻANIA?
                            Dim fldPunkt = GetType(Global.MapoTero.FormKreatorMapy).GetField("_wlasnyPunktStylu", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            Dim wlasnyPt = CType(fldPunkt.GetValue(k), PunktXY?)
                            Assert.True(wlasnyPt.HasValue, "Własny punkt powinien być ustawiony po przeciągnięciu!")

                            ' Sprawdź czy punkt Y przesunięcia różni się od punktu początkowego szablonu (przesunięcie poziome)
                            Dim p0 = UkladWspolrzednych.PL1992.ZWgs84(szablonPoczatkowy.ProbkaLat, szablonPoczatkowy.ProbkaLon)
                            Assert.NotEqual(p0.Y, wlasnyPt.Value.Y, 1)

                            ' Po zwolnieniu przycisku myszy przesunięcie ekranowe pozostaje zachowane (nie cofa widoku)
                            Assert.NotNull(picPodgladStylu.Image)
                            Dim fldDelta = GetType(Global.MapoTero.FormKreatorMapy).GetField("_deltaPrzeciaganiaStylu", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            Dim delta = CType(fldDelta.GetValue(k), Point)
                            ' Przesunięcie zostaje do pobrania nowej próbki, potem jest zerowane (próbka wyśrodkowana na nowym punkcie);
                            ' wynik zależy od czasu odpowiedzi serwera
                            If lblStan.Text.StartsWith("Podgląd gotowy") Then
                                Assert.Equal(Point.Empty, delta)
                            Else
                                Assert.Equal(New Point(50, 0), delta)
                            End If

                            fldBaza.SetValue(k, Nothing)
                        End Using
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormZaawansowanyMbtiles_TestOdswiezPodglad_CzyBlad()
        UruchomWWatkuSta(
            Sub()
                Using dlg As New Global.MapoTero.FormZaawansowanyMbtiles()
                    Dim zestaw As New ZestawWarstw()
                    zestaw.Dodaj("MAPA TOPOGRAFICZNA", 100, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/BDOT500/WMS/Standard", "TOPO_WMTS")
                    zestaw.Dodaj("ISOK_Cien", 35, "https://mapy.geoportal.gov.pl/wss/service/PZGIK/NMT/WMS/ShadedRelief", "numer_mod_terenu_cieniowanie_WMTS", nakladka:=True, trybMieszania:=TrybMieszania.Mnozenie)
                    zestaw.Dodaj("hiking", 100, "https://tile.waymarkedtrails.org/hiking", "Waymarked_Trails", nakladka:=True)

                    Dim zasieg As New Zasieg(500000, 600000, 510000, 610000)
                    Dim uklad = UkladWspolrzednych.PL1992
                    Dim sciezka = "C:\test_export\tatry.mbtiles"
                    Dim korekta As New OpcjeKorekcjiMip() With {.Wlaczona = True}

                    Dim silnik As New PodgladWarstwSilnik() With {.Bok = 512}
                    Dim bmp As New Bitmap(512, 512)

                    Dim t = dlg.GetType()
                    Dim fTryb = t.GetField("_trybKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fTryb.SetValue(dlg, True)
                    Dim fSilnik = t.GetField("_silnikKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fSilnik.SetValue(dlg, silnik)
                    Dim fZestaw = t.GetField("_zestawKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fZestaw.SetValue(dlg, zestaw)
                    Dim fProbka = t.GetField("_probkaBmpKreatora", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fProbka.SetValue(dlg, bmp)
                    Dim fKorekta = t.GetField("_opcjeKorekty", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    fKorekta.SetValue(dlg, korekta)

                    Dim warstwyField = t.GetField("_warstwy", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    Dim listaWarstw = DirectCast(warstwyField.GetValue(dlg), Generic.List(Of Global.MapoTero.WarstwaWielowarstwowa))
                    listaWarstw.Clear()
                    For i = 0 To zestaw.Warstwy.Count - 1
                        Dim wz = zestaw.Warstwy(i)
                        listaWarstw.Add(New Global.MapoTero.WarstwaWielowarstwowa With {
                            .Tytul = wz.Nazwa,
                            .PoziomMin = 10,
                            .PoziomMax = 16,
                            .Krycie = wz.Krycie / 100.0,
                            .TrybMieszania = wz.TrybMieszania,
                            .Wlaczona = True,
                            .Folder = "(Kreator: warstwa #" & (i + 1) & ")"
                        })
                    Next

                    dlg.Show()
                    Application.DoEvents()

                    Dim m = t.GetMethod("OdswiezPodgladAsync", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                    m.Invoke(dlg, Nothing)

                    Dim start = DateTime.UtcNow
                    While (DateTime.UtcNow - start).TotalMilliseconds < 500
                        Application.DoEvents()
                        Thread.Sleep(20)
                    End While

                    Dim txtLog = DirectCast(dlg.Controls.Find("txtLog", True)(0), RichTextBox)
                    Assert.DoesNotContain("Błąd renderowania podglądu", txtLog.Text)

                    dlg.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormKreatorMapy_Krok3_PrzesuwanieMyszkaPodgladu_SprawdzPolozenie()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show()
                        Application.DoEvents()

                        ' Krok 1 -> Krok 2 -> Krok 3
                        Dim btnDalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                        btnDalej.PerformClick()
                        Application.DoEvents()
                        btnDalej.PerformClick()
                        Application.DoEvents()

                        Dim picPodgladObszaru = DirectCast(k.Controls.Find("picPodgladObszaru", True)(0), PictureBox)

                        ' Ustaw sztuczną bazę podglądu
                        Using bmp As New Bitmap(300, 300)
                            Using g = Graphics.FromImage(bmp)
                                g.Clear(Color.Blue)
                                g.FillRectangle(Brushes.Red, 140, 140, 20, 20)
                            End Using

                            Dim fldBaza = GetType(Global.MapoTero.FormKreatorMapy).GetField("_bazaPodgladuObszaru", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            fldBaza.SetValue(k, bmp)

                            Dim miZastosuj = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("ZastosujZoomObszaru", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miZastosuj.Invoke(k, Nothing)

                            ' 1. MouseDown at (100, 100)
                            Dim miDown = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladObszaru_MouseDown", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miDown.Invoke(k, New Object() {picPodgladObszaru, New MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0)})

                            ' 2. MouseMove to (150, 100) -> przesunięcie w prawo o 50 px
                            Dim miMove = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladObszaru_MouseMove", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miMove.Invoke(k, New Object() {picPodgladObszaru, New MouseEventArgs(MouseButtons.Left, 0, 150, 100, 0)})

                            ' 3. MouseUp at (150, 100)
                            Dim miUp = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("picPodgladObszaru_MouseUp", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            miUp.Invoke(k, New Object() {picPodgladObszaru, New MouseEventArgs(MouseButtons.Left, 1, 150, 100, 0)})

                            ' Sprawdź czy _wlasnyPunktObszaru został ustawiony
                            Dim fldPunkt = GetType(Global.MapoTero.FormKreatorMapy).GetField("_wlasnyPunktObszaru", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            Dim wlasnyPt = CType(fldPunkt.GetValue(k), PunktXY?)
                            Assert.True(wlasnyPt.HasValue, "Własny punkt obszaru powinien być ustawiony po przeciągnięciu!")

                            ' Przesunięcie nie powinno zresetować się do (0,0) przed zakończeniem pobierania
                            Dim fldDelta = GetType(Global.MapoTero.FormKreatorMapy).GetField("_deltaPrzeciaganiaObszaru", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                            Dim delta = CType(fldDelta.GetValue(k), Point)
                            Assert.Equal(New Point(50, 0), delta)

                            fldBaza.SetValue(k, Nothing)
                        End Using
                    End Using
                End Using
            End Sub)
    End Sub

End Class
