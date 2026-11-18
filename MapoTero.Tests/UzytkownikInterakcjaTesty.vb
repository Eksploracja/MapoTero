'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Wnikliwe testy interakcji użytkownika (User Interaction / UI Fuzzing).
''' Symulują klikanie w poszczególne opcje, menu, przyciski i wprowadzanie skrajnych danych
''' we wszystkich formularzach aplikacji w poszukiwaniu nieobsłużonych wyjątków i błędów.
''' </summary>
<Trait("Kategoria", "Ciezkie")>
Public Class UzytkownikInterakcjaTesty

    ''' <summary>Pomocnicza metoda uruchamiająca akcję w dedykowanym wątku STA Windows Forms.</summary>
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
            Throw New Exception("Błąd podczas interakcji UI: " & wyjatek.Message, wyjatek)
        End If
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form1_WprowadzanieSkrajnychWspolrzednych()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim txtXDol = DirectCast(f.Controls.Find("txtXDol", True)(0), TextBox)
                    Dim txtYLewy = DirectCast(f.Controls.Find("txtYLewy", True)(0), TextBox)
                    Dim txtXGora = DirectCast(f.Controls.Find("txtXGora", True)(0), TextBox)
                    Dim txtYPrawy = DirectCast(f.Controls.Find("txtYPrawy", True)(0), TextBox)
                    Dim txtBok = DirectCast(f.Controls.Find("txtBokSegmentu", True)(0), TextBox)
                    Dim txtPiksel = DirectCast(f.Controls.Find("txtRozmiarPiksela", True)(0), TextBox)
                    Dim cmbUklad = DirectCast(f.Controls.Find("cmbUklad", True)(0), ComboBox)

                    ' 1. Wprowadzanie pustych wartości
                    txtXDol.Text = ""
                    txtYLewy.Text = ""
                    txtXGora.Text = ""
                    txtYPrawy.Text = ""
                    txtBok.Text = ""
                    txtPiksel.Text = ""
                    Application.DoEvents()

                    ' 2. Wprowadzanie wartości nieliczbowych i znaków specjalnych
                    txtXDol.Text = "abc"
                    txtYLewy.Text = "!@#$%"
                    txtXGora.Text = "   "
                    txtYPrawy.Text = "12.34.56"
                    txtBok.Text = "zer0"
                    txtPiksel.Text = "dwa metry"
                    Application.DoEvents()

                    ' 3. Przełączanie układów współrzędnych z błędnymi danymi
                    For i As Integer = 0 To cmbUklad.Items.Count - 1
                        cmbUklad.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 4. Wprowadzanie wartości ujemnych i odwróconych granic (XDol > XGora)
                    txtXDol.Text = "500000"
                    txtXGora.Text = "400000"
                    txtYLewy.Text = "600000"
                    txtYPrawy.Text = "300000"
                    txtBok.Text = "-1000"
                    txtPiksel.Text = "-2.5"
                    Application.DoEvents()

                    ' 5. Wprowadzanie ułamków z przecinkiem i kropką
                    txtXDol.Text = "200000,5"
                    txtXGora.Text = "210000.75"
                    txtYLewy.Text = "500000,25"
                    txtYPrawy.Text = "510000.5"
                    txtBok.Text = "1024"
                    txtPiksel.Text = "1,25"
                    Application.DoEvents()

                    ' 6. Wprowadzanie bardzo dużych liczb (potencjalne przepełnienie)
                    txtXDol.Text = "999999999999999999"
                    txtXGora.Text = "1000000000000000000"
                    txtBok.Text = "99999999"
                    txtPiksel.Text = "99999999"
                    Application.DoEvents()

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form1_DrzewoWarstwIFiltrowanie()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim txtSzukaj = DirectCast(f.Controls.Find("txtSzukajWarstwy", True)(0), TextBox)
                    Dim tvWarstwy = DirectCast(f.Controls.Find("tvWarstwy", True)(0), TreeView)
                    Dim btnZwinRozwin = DirectCast(f.Controls.Find("btnZwinRozwin", True)(0), Button)

                    ' 1. Wyszukiwanie z polskimi znakami diakrytycznymi
                    txtSzukaj.Text = "działki"
                    Application.DoEvents()
                    Assert.NotEmpty(txtSzukaj.Text)

                    ' 2. Wyszukiwanie bez polskich znaków
                    txtSzukaj.Text = "dzialki"
                    Application.DoEvents()

                    ' 3. Wyszukiwanie wielkimi literami
                    txtSzukaj.Text = "ORTOFOTOMAPA"
                    Application.DoEvents()

                    ' 4. Wyszukiwanie frazy nieistniejącej
                    txtSzukaj.Text = "nieistniejaca_warstwa_xyz_123"
                    Application.DoEvents()

                    ' 5. Czyszczenie pola wyszukiwania
                    txtSzukaj.Text = ""
                    Application.DoEvents()

                    ' 6. Klikanie przycisku rozwijania / zwijania wszystkich grup
                    btnZwinRozwin.PerformClick()
                    Application.DoEvents()
                    btnZwinRozwin.PerformClick()
                    Application.DoEvents()

                    ' 7. Interakcja z węzłami drzewa jeśli istnieją
                    If tvWarstwy.Nodes.Count > 0 Then
                        Dim pierwszyWezel = tvWarstwy.Nodes(0)
                        pierwszyWezel.Expand()
                        Application.DoEvents()
                        tvWarstwy.SelectedNode = pierwszyWezel
                        Application.DoEvents()

                        ' Kliknięcie węzła dziecka
                        If pierwszyWezel.Nodes.Count > 0 Then
                            Dim dziecko = pierwszyWezel.Nodes(0)
                            tvWarstwy.SelectedNode = dziecko
                            dziecko.Checked = True
                            Application.DoEvents()
                            dziecko.Checked = False
                            Application.DoEvents()
                        End If
                    End If

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form1_ListaWybranychWarstwIOpcje()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim lstWarstwy = DirectCast(f.Controls.Find("lstWybraneWarstwy", True)(0), Global.MapoTero.ListaWarstw)
                    Dim numKrycie = DirectCast(f.Controls.Find("numKrycie", True)(0), NumericUpDown)
                    Dim btnGora = DirectCast(f.Controls.Find("btnWarstwaWyzej", True)(0), Button)
                    Dim btnDol = DirectCast(f.Controls.Find("btnWarstwaNizej", True)(0), Button)
                    Dim btnUsun = DirectCast(f.Controls.Find("btnUsunWarstwe", True)(0), Button)

                    ' 1. Klikanie góra/dół/usuń przy pustej liście
                    btnGora.PerformClick()
                    btnDol.PerformClick()
                    btnUsun.PerformClick()
                    Application.DoEvents()

                    ' 2. Dodanie warstw testowych do zestawu Form1
                    Dim zestaw As New ZestawWarstw()
                    zestaw.Dodaj("W1", 100, "", "ortofotomapa")
                    zestaw.Dodaj("W2", 80, "", "ortofotomapa")
                    zestaw.Dodaj("W3", 50, "", "topo")

                    Dim poleZestaw = GetType(Global.MapoTero.Form1).GetField("_zestaw", BindingFlags.NonPublic Or BindingFlags.Instance)
                    poleZestaw.SetValue(f, zestaw)
                    Dim metodaPoZmianie = GetType(Global.MapoTero.Form1).GetMethod("PoZmianieWarstw", BindingFlags.NonPublic Or BindingFlags.Instance)
                    metodaPoZmianie.Invoke(f, New Object() {1})
                    Application.DoEvents()
                    Assert.Equal(3, lstWarstwy.Zestaw.Liczba)

                    ' 3. Zmiana zaznaczenia i manipulacja kryciem
                    lstWarstwy.Zaznaczona = 1
                    Application.DoEvents()
                    numKrycie.Value = 35
                    Application.DoEvents()
                    Assert.Equal(35, lstWarstwy.Zestaw.Warstwy(1).Krycie)

                    ' 4. Zmiana kolejności przyciskami
                    btnGora.PerformClick()
                    Application.DoEvents()

                    btnDol.PerformClick()
                    Application.DoEvents()

                    ' 5. Usuwanie warstwy przyciskiem
                    btnUsun.PerformClick()
                    Application.DoEvents()
                    Assert.Equal(2, lstWarstwy.Zestaw.Liczba)

                    ' 6. Wyczyszczenie pozostałych warstw
                    Dim metodaWyczysc = GetType(Global.MapoTero.Form1).GetMethod("WyczyscWarstwy", BindingFlags.NonPublic Or BindingFlags.Instance)
                    metodaWyczysc.Invoke(f, Nothing)
                    Application.DoEvents()
                    Assert.Equal(0, lstWarstwy.Zestaw.Liczba)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form2_Ustawienia_PrzelaczanieWszystkichKontrolek()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form2()
                    f.Show()
                    Application.DoEvents()

                    Dim chkSiatka = DirectCast(f.Controls.Find("chkSiatkaSegmentow", True)(0), CheckBox)
                    Dim chkEdycjaXY = DirectCast(f.Controls.Find("chkEdycjaXY", True)(0), CheckBox)
                    Dim chkKursorWgs = DirectCast(f.Controls.Find("chkKursorWgs", True)(0), CheckBox)
                    Dim chkZaznaczenieWgs = DirectCast(f.Controls.Find("chkZaznaczenieWgs", True)(0), CheckBox)
                    Dim chkTrekBuddy = DirectCast(f.Controls.Find("chkTrekBuddy", True)(0), CheckBox)
                    Dim chkPrzezroczystosc = DirectCast(f.Controls.Find("chkPrzezroczystosc", True)(0), CheckBox)
                    Dim chkPowyzejOstatniego = DirectCast(f.Controls.Find("chkPowyzejOstatniego", True)(0), CheckBox)
                    Dim chkZachowajKafle = DirectCast(f.Controls.Find("chkZachowajKafle", True)(0), CheckBox)
                    Dim chkZamienXY = DirectCast(f.Controls.Find("chkZamienXY", True)(0), CheckBox)

                    Dim cmbFormat = DirectCast(f.Controls.Find("cmbFormat", True)(0), ComboBox)
                    Dim cmbNumeracja = DirectCast(f.Controls.Find("cmbNumeracja", True)(0), ComboBox)
                    Dim cmbUkladDomyslny = DirectCast(f.Controls.Find("cmbUkladDomyslny", True)(0), ComboBox)
                    Dim txtPrefiks = DirectCast(f.Controls.Find("txtPrefiks", True)(0), TextBox)

                    Dim nudWatki = DirectCast(f.Controls.Find("nudWatki", True)(0), NumericUpDown)
                    Dim nudLimitCzasu = DirectCast(f.Controls.Find("nudLimitCzasu", True)(0), NumericUpDown)
                    Dim nudIloscProb = DirectCast(f.Controls.Find("nudIloscProb", True)(0), NumericUpDown)
                    Dim nudPrzerwa = DirectCast(f.Controls.Find("nudPrzerwa", True)(0), NumericUpDown)

                    ' 1. Przełączanie wszystkich checkboxów
                    For Each chk In {chkSiatka, chkEdycjaXY, chkKursorWgs, chkZaznaczenieWgs,
                                     chkPrzezroczystosc, chkPowyzejOstatniego, chkZachowajKafle, chkZamienXY}
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                    Next

                    ' 2. Tryb TrekBuddy (włączenie i wyłączenie)
                    chkTrekBuddy.Checked = True
                    Application.DoEvents()
                    Assert.True(chkTrekBuddy.Checked)
                    chkTrekBuddy.Checked = False
                    Application.DoEvents()
                    Assert.False(chkTrekBuddy.Checked)

                    ' 3. Zmiana formatu segmentów
                    For i As Integer = 0 To cmbFormat.Items.Count - 1
                        cmbFormat.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 4. Zmiana stylu numeracji
                    For i As Integer = 0 To cmbNumeracja.Items.Count - 1
                        cmbNumeracja.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 5. Zmiana układu domyślnego
                    For i As Integer = 0 To cmbUkladDomyslny.Items.Count - 1
                        cmbUkladDomyslny.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 6. Wprowadzanie dozwolonych znaków w przedrostku
                    txtPrefiks.Text = "arkusz_testowy_1"
                    Application.DoEvents()

                    ' 7. Wartości liczbowe skrajne
                    nudWatki.Value = nudWatki.Maximum
                    nudLimitCzasu.Value = nudLimitCzasu.Minimum
                    nudIloscProb.Value = 5
                    nudPrzerwa.Value = 200
                    Application.DoEvents()

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form3_Scalanie_ReczneParametryIFormaty()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form3()
                    f.Show()
                    Application.DoEvents()

                    Dim chkRecznie = DirectCast(f.Controls.Find("chkRecznie", True)(0), CheckBox)
                    Dim cmbFormatArkusza = DirectCast(f.Controls.Find("cmbFormatArkusza", True)(0), ComboBox)
                    Dim trkJakosc = DirectCast(f.Controls.Find("trkJakosc", True)(0), TrackBar)
                    Dim txtXDol = DirectCast(f.Controls.Find("txtXDol", True)(0), TextBox)
                    Dim txtYLewy = DirectCast(f.Controls.Find("txtYLewy", True)(0), TextBox)
                    Dim txtXGora = DirectCast(f.Controls.Find("txtXGora", True)(0), TextBox)
                    Dim txtYPrawy = DirectCast(f.Controls.Find("txtYPrawy", True)(0), TextBox)
                    Dim txtBok = DirectCast(f.Controls.Find("txtBokSegmentu", True)(0), TextBox)
                    Dim txtPiksel = DirectCast(f.Controls.Find("txtRozmiarPiksela", True)(0), TextBox)

                    Dim chkWorldFile = DirectCast(f.Controls.Find("chkWorldFile", True)(0), CheckBox)
                    Dim chkKml = DirectCast(f.Controls.Find("chkKml", True)(0), CheckBox)
                    Dim chkMap = DirectCast(f.Controls.Find("chkMap", True)(0), CheckBox)
                    Dim chkTab = DirectCast(f.Controls.Find("chkTab", True)(0), CheckBox)

                    ' 1. Włączenie ręcznego wprowadzania parametrów
                    chkRecznie.Enabled = True
                    chkRecznie.Checked = True
                    Application.DoEvents()

                    ' 2. Zmiana formatu arkusza scalonego
                    For i As Integer = 0 To cmbFormatArkusza.Items.Count - 1
                        cmbFormatArkusza.SelectedIndex = i
                        Application.DoEvents()
                        ' Sprawdzenie suwaka jakości JPEG
                        trkJakosc.Value = 75
                        Application.DoEvents()
                    Next

                    ' 3. Przełączanie formatów georeferencji
                    For Each chk In {chkWorldFile, chkKml, chkMap, chkTab}
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                    Next

                    ' 4. Wprowadzanie poprawnych parametrów siatki 2x2
                    txtXDol.Text = "200000"
                    txtYLewy.Text = "500000"
                    txtXGora.Text = "202048"
                    txtYPrawy.Text = "502048"
                    txtBok.Text = "1024"
                    txtPiksel.Text = "1.0"
                    Application.DoEvents()

                    Dim txtKol = DirectCast(f.Controls.Find("txtLiczbaKolumn", True)(0), TextBox)
                    Dim txtWie = DirectCast(f.Controls.Find("txtLiczbaWierszy", True)(0), TextBox)
                    Assert.Equal("2", txtKol.Text)
                    Assert.Equal("2", txtWie.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormEksport_PrzelaczanieTypowEksportu()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.FormEksport()
                    f.Show()
                    Application.DoEvents()

                    Dim rbGarmin = DirectCast(f.Controls.Find("rbKmzGarmin", True)(0), RadioButton)
                    Dim rbKmz = DirectCast(f.Controls.Find("rbKmz", True)(0), RadioButton)
                    Dim rbMBTiles = DirectCast(f.Controls.Find("rbMBTiles", True)(0), RadioButton)
                    Dim rbSQLiteDB = DirectCast(f.Controls.Find("rbSQLiteDB", True)(0), RadioButton)
                    Dim rbOruxMaps = DirectCast(f.Controls.Find("rbOruxMaps", True)(0), RadioButton)

                    Dim nudJakosc = DirectCast(f.Controls.Find("nudJakosc", True)(0), NumericUpDown)
                    Dim nudMaksKafli = DirectCast(f.Controls.Find("nudMaksKafli", True)(0), NumericUpDown)
                    Dim chkPng = DirectCast(f.Controls.Find("chkPng", True)(0), CheckBox)
                    Dim chkNakladka = DirectCast(f.Controls.Find("chkNakladka", True)(0), CheckBox)
                    Dim chkBialePodstawa = DirectCast(f.Controls.Find("chkBialePodstawa", True)(0), CheckBox)

                    ' 1. Przełączanie wszystkich typów eksportu
                    For Each rb In {rbGarmin, rbKmz, rbMBTiles, rbSQLiteDB, rbOruxMaps}
                        rb.Checked = True
                        Application.DoEvents()
                        Assert.True(rb.Checked)
                    Next

                    ' 2. Manipulacja opcjami jakości i kafli
                    nudJakosc.Value = 85
                    nudMaksKafli.Value = 100
                    chkPng.Checked = True
                    chkNakladka.Checked = True
                    chkBialePodstawa.Checked = True
                    Application.DoEvents()

                    chkPng.Checked = False
                    chkNakladka.Checked = False
                    Application.DoEvents()

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormEksport_DopasowanieWysokosciIAutoScroll()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.FormEksport()
                    f.Show()
                    Application.DoEvents()

                    ' Wysokość okna nie może przekraczać wysokości obszaru roboczego ekranu
                    Dim ekran = Screen.FromControl(f).WorkingArea
                    Assert.True(f.Height <= ekran.Height)

                    ' Jeśli okno zostało zmniejszone w pionie, AutoScroll musi być aktywny z bazową wysokością 788 z projektu,
                    ' przeskalowaną tak jak okno (czcionka Segoe UI 9 już przy 100% DPI)
                    Dim bazowa As Integer = CInt(Math.Round(788 * Global.MapoTero.PomocProgramu.SkalaProjektanta(f).Height))
                    If f.ClientSize.Height < bazowa Then
                        Assert.True(f.AutoScroll)
                        Assert.Equal(bazowa, f.AutoScrollMinSize.Height)
                    End If

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormEksportCad_OpcjeIWarstwy()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.FormEksportCad()
                    f.Show()
                    Application.DoEvents()

                    Dim rbSegmenty = DirectCast(f.Controls.Find("rbSegmenty", True)(0), RadioButton)
                    Dim rbArkusz = DirectCast(f.Controls.Find("rbArkusz", True)(0), RadioButton)
                    Dim rbBezRastrow = DirectCast(f.Controls.Find("rbBezRastrow", True)(0), RadioButton)

                    Dim chkSiatka = DirectCast(f.Controls.Find("chkSiatka", True)(0), CheckBox)
                    Dim chkObrys = DirectCast(f.Controls.Find("chkObrys", True)(0), CheckBox)
                    Dim chkNumery = DirectCast(f.Controls.Find("chkNumery", True)(0), CheckBox)
                    Dim chkTrasa = DirectCast(f.Controls.Find("chkTrasa", True)(0), CheckBox)
                    Dim chkOpis = DirectCast(f.Controls.Find("chkOpis", True)(0), CheckBox)

                    Dim chkDxf = DirectCast(f.Controls.Find("chkDxf", True)(0), CheckBox)
                    Dim chkSciezkiWzgledne = DirectCast(f.Controls.Find("chkSciezkiWzgledne", True)(0), CheckBox)
                    Dim chkScr = DirectCast(f.Controls.Find("chkScr", True)(0), CheckBox)
                    Dim chkLsp = DirectCast(f.Controls.Find("chkLsp", True)(0), CheckBox)

                    ' 1. Przełączanie segmenty vs arkusz vs bez rastrów
                    rbArkusz.Checked = True
                    Application.DoEvents()
                    Assert.True(rbArkusz.Checked)

                    rbBezRastrow.Checked = True
                    Application.DoEvents()
                    Assert.True(rbBezRastrow.Checked)

                    rbSegmenty.Checked = True
                    Application.DoEvents()
                    Assert.True(rbSegmenty.Checked)

                    ' 2. Zmiana formatu eksportu CAD (DXF, SCR, LISP, ścieżki względne)
                    For Each chk In {chkDxf, chkSciezkiWzgledne, chkScr, chkLsp}
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                    Next

                    ' 3. Przełączanie warstw wektorowych CAD
                    For Each chk In {chkSiatka, chkObrys, chkNumery, chkTrasa, chkOpis}
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                        chk.Checked = Not chk.Checked
                        Application.DoEvents()
                    Next

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormZaawansowanyMbtiles_Opcje()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.FormZaawansowanyMbtiles()
                    f.Show()
                    Application.DoEvents()

                    Dim cmbFormat = DirectCast(f.Controls.Find("cmbFormat", True)(0), ComboBox)
                    Dim tbZoom = DirectCast(f.Controls.Find("tbZoom", True)(0), TrackBar)
                    Dim chkWypelnijLuki = DirectCast(f.Controls.Find("chkWypelnijLuki", True)(0), CheckBox)
                    Dim chkPiramida = DirectCast(f.Controls.Find("chkPiramida", True)(0), CheckBox)
                    Dim btnGora = DirectCast(f.Controls.Find("btnGora", True)(0), Button)
                    Dim btnDol = DirectCast(f.Controls.Find("btnDol", True)(0), Button)
                    Dim btnUsun = DirectCast(f.Controls.Find("btnUsunSesje", True)(0), Button)

                    ' 1. Zmiana formatu kafelków
                    For i As Integer = 0 To cmbFormat.Items.Count - 1
                        cmbFormat.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 2. Zmiana zoomu podglądu
                    tbZoom.Value = Math.Min(tbZoom.Maximum, tbZoom.Value + 1)
                    Application.DoEvents()

                    ' 3. Checkboxy opcji
                    chkWypelnijLuki.Checked = Not chkWypelnijLuki.Checked
                    chkPiramida.Checked = Not chkPiramida.Checked
                    Application.DoEvents()

                    ' 4. Przyciski góra/dół/usuń przy pustej liście sesji
                    btnGora.PerformClick()
                    btnDol.PerformClick()
                    btnUsun.PerformClick()
                    Application.DoEvents()

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Narzedzia_UsuwaniePustychSegmentow()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.usuwanie_p_seg()
                    f.Show()
                    Application.DoEvents()

                    Dim txtProg = DirectCast(f.Controls.Find("txtProgKB", True)(0), TextBox)
                    Dim cmbRozszerzenie = DirectCast(f.Controls.Find("cmbRozszerzenie", True)(0), ComboBox)
                    Dim btnUsun = DirectCast(f.Controls.Find("btnUsun", True)(0), Button)

                    ' 1. Wprowadzanie wartości progu
                    txtProg.Text = "0"
                    Application.DoEvents()
                    txtProg.Text = "2.5"
                    Application.DoEvents()
                    txtProg.Text = "abc"
                    Application.DoEvents()
                    txtProg.Text = "-5"
                    Application.DoEvents()
                    txtProg.Text = "999999"
                    Application.DoEvents()

                    ' 2. Zmiana filtra rozszerzeń
                    For i As Integer = 0 To cmbRozszerzenie.Items.Count - 1
                        cmbRozszerzenie.SelectedIndex = i
                        Application.DoEvents()
                    Next

                    ' 3. Walidacja wartości progu i kontrolek
                    txtProg.Text = "10"
                    Application.DoEvents()
                    Assert.Equal("10", txtProg.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormDodajSerwer_KatalogIFiltry()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.FormDodajSerwer()
                    f.Show()
                    Application.DoEvents()

                    ' Wyszukiwanie w katalogu usług
                    Dim pnlKatalog = DirectCast(f.Controls.Find("pnlKatalog", True)(0), Global.MapoTero.PanelKatalogSerwerow)
                    Dim txtSzukaj = DirectCast(pnlKatalog.Controls.Find("txtSzukaj", True)(0), TextBox)
                    Dim chkUkryj = DirectCast(pnlKatalog.Controls.Find("chkUkryjDodane", True)(0), CheckBox)

                    ' 1. Wyszukiwanie różnych fraz
                    txtSzukaj.Text = "orto"
                    Application.DoEvents()
                    txtSzukaj.Text = "zabytki"
                    Application.DoEvents()
                    txtSzukaj.Text = "nieistniejacy_serwer_xyz"
                    Application.DoEvents()
                    txtSzukaj.Text = ""
                    Application.DoEvents()

                    ' 2. Ukrywanie już dodanych
                    chkUkryj.Checked = Not chkUkryj.Checked
                    Application.DoEvents()
                    chkUkryj.Checked = Not chkUkryj.Checked
                    Application.DoEvents()

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_Form1_UkladPolaKryciaWarstwy()
        UruchomWWatkuSta(
            Sub()
                Using f As New Global.MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim grp = DirectCast(f.Controls.Find("grpWybraneWarstwy", True)(0), GroupBox)
                    Dim numKrycie = DirectCast(f.Controls.Find("numKrycie", True)(0), NumericUpDown)
                    Dim lblKrycie = DirectCast(f.Controls.Find("lblKrycie", True)(0), Label)
                    Dim btnZaaw = DirectCast(f.Controls.Find("btnWarstwyZaawansowane", True)(0), Button)
                    Dim btnKosz = DirectCast(f.Controls.Find("btnResetujWarstwy", True)(0), Button)

                    ' Pole krycia musi mieć wystarczającą szerokość na liczbę 100 i strzałki (min 48 px)
                    Assert.True(numKrycie.Width >= 48, $"Szerokość numKrycie ({numKrycie.Width}) powinna wynosić co najmniej 48 px, aby nie przycinać liczby 100.")
                    ' Elementy nie mogą się nakładać
                    Assert.True(numKrycie.Left >= lblKrycie.Right - 2, $"Etykieta lblKrycie (Right={lblKrycie.Right}) nakłada się na numKrycie (Left={numKrycie.Left}).")
                    Assert.True(btnZaaw.Left >= numKrycie.Right - 1, $"numKrycie (Right={numKrycie.Right}) nakłada się na btnWarstwyZaawansowane (Left={btnZaaw.Left}).")

                    ' Wyrenderuj grpWybraneWarstwy do bitmapy i zapisz do artefaktów
                    Using bmp As New Bitmap(grp.Width, grp.Height)
                        grp.DrawToBitmap(bmp, New Rectangle(0, 0, grp.Width, grp.Height))
                        Dim sciezkaBmp = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\podglad_krycie_warstwy.png"
                        bmp.Save(sciezkaBmp, System.Drawing.Imaging.ImageFormat.Png)
                    End Using

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub TestUzytkownik_FormImportOzi_TrybWsadowyUI()
        UruchomWWatkuSta(
            Sub()
                Dim folderTestu = Path.Combine(Path.GetTempPath(), "MapoTero_OziUiBatch_" & Guid.NewGuid().ToString("N"))
                Directory.CreateDirectory(folderTestu)
                Try
                    ' Przygotuj 2 testowe pliki map
                    Dim p1 = Path.Combine(folderTestu, "mapa1.map")
                    Dim p2 = Path.Combine(folderTestu, "mapa2.map")
                    Dim img1 = Path.Combine(folderTestu, "mapa1.jpg")
                    Dim img2 = Path.Combine(folderTestu, "mapa2.jpg")
                    Using bmp As New Bitmap(32, 32)
                        bmp.Save(img1, System.Drawing.Imaging.ImageFormat.Jpeg)
                        bmp.Save(img2, System.Drawing.Imaging.ImageFormat.Jpeg)
                    End Using
                    Dim linie = {
                        "OziExplorer Map Data File Version 2.2", "Mapa", "mapa1.jpg", "1 ,Map Code,", "WGS 84,, 0, 0,WGS 84",
                        "Reserved 1", "Reserved 2", "Magnetic Variation,,,E", "Map Projection,Latitude/Longitude,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No",
                        "Point01,xy, 0, 0,in, deg, 50, 00,N, 20, 00,E, grid,,,,N",
                        "Point02,xy, 32, 32,in, deg, 49, 59,N, 20, 01,E, grid,,,,N"
                    }
                    File.WriteAllLines(p1, linie)
                    linie(1) = "Mapa 2"
                    linie(2) = "mapa2.jpg"
                    File.WriteAllLines(p2, linie)

                    Using f As New Global.MapoTero.FormImportOzi()
                        f.Show()
                        Application.DoEvents()

                        f.OtworzWsadowo({p1, p2})
                        Application.DoEvents()

                        Dim txtPlikMap = DirectCast(f.Controls.Find("txtPlikMap", True)(0), TextBox)
                        Dim btnKonwertuj = DirectCast(f.Controls.Find("btnKonwertuj", True)(0), Button)
                        Dim btnWybierzObraz = DirectCast(f.Controls.Find("btnWybierzObraz", True)(0), Button)
                        Dim btnWybierzFolder = DirectCast(f.Controls.Find("btnWybierzFolder", True)(0), Button)

                        Assert.Contains("2", txtPlikMap.Text)
                        Assert.Contains("hurtowo", btnKonwertuj.Text, StringComparison.OrdinalIgnoreCase)
                        Assert.False(btnWybierzObraz.Enabled)
                        Assert.True(btnKonwertuj.Enabled)
                        Assert.True(btnWybierzFolder.Enabled)

                        ' Renderowanie do bitmapy i zapis do podglądu
                        Using bmp As New Bitmap(f.Width, f.Height)
                            f.DrawToBitmap(bmp, New Rectangle(0, 0, f.Width, f.Height))
                            Dim sciezkaBmp = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\podglad_ozi_tryb_wsadowy.png"
                            bmp.Save(sciezkaBmp, System.Drawing.Imaging.ImageFormat.Png)
                        End Using

                        f.Close()
                    End Using
                Finally
                    If Directory.Exists(folderTestu) Then Directory.Delete(folderTestu, True)
                End Try
            End Sub)
    End Sub

End Class
