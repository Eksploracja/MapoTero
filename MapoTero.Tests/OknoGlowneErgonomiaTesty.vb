'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports GMap.NET
Imports Xunit

''' <summary>
''' Ergonomia okna głównego: wyróżniony przycisk pobierania, akcje w kartach belki, pole wyszukiwania na pasku,
''' pokazywanie obszaru pobierania na mapie.
''' </summary>
<Collection("OknaKreatora")>
<Trait("Kategoria", "UI")>
Public Class OknoGlowneErgonomiaTesty

    Private Shared ReadOnly Prywatne As BindingFlags = BindingFlags.NonPublic Or BindingFlags.Instance

    Private Shared Sub WWatkuSta(akcja As Action(Of MapoTero.Form1))
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    akcja(f)
                    f.Close()
                End Using
            End Sub)
    End Sub

    Private Shared Function Kontrolka(Of T As Control)(f As Form, nazwa As String) As T
        Return DirectCast(Assert.Single(f.Controls.Find(nazwa, True)), T)
    End Function

    Private Shared Sub UstawObszar(f As Form, xDol As String, yLewy As String, xGora As String, yPrawy As String)
        Kontrolka(Of TextBox)(f, "txtXDol").Text = xDol
        Kontrolka(Of TextBox)(f, "txtYLewy").Text = yLewy
        Kontrolka(Of TextBox)(f, "txtXGora").Text = xGora
        Kontrolka(Of TextBox)(f, "txtYPrawy").Text = yPrawy
        Application.DoEvents()
    End Sub

    <Fact>
    Public Sub PobierzSegmenty_BezWarstwPrzygaszony_ZWarstwaIObszaremWyrozniony()
        WWatkuSta(
            Sub(f)
                f.NowaSesja(pytaj:=False)
                Application.DoEvents()
                Dim pobierz = Kontrolka(Of Button)(f, "btnPobierz")
                Assert.False(pobierz.Enabled)
                Assert.NotEqual(MapoTero.Form1.KolorAkcjiOkna, pobierz.BackColor)

                UstawObszar(f, "680000", "568000", "682000", "570000")
                f.WybranyZestaw().Dodaj("Próbka")
                GetType(MapoTero.Form1).GetMethod("OdswiezPrzyciskiWarstw", Prywatne).Invoke(f, Nothing)
                f.AktualizujStanBelki()
                Application.DoEvents()
                Assert.True(pobierz.Enabled)
                Assert.Equal(MapoTero.Form1.KolorAkcjiOkna, pobierz.BackColor)
                Assert.Equal(Drawing.Color.White, pobierz.ForeColor)
                Assert.True(pobierz.Font.Bold)
            End Sub)
    End Sub

    <Fact>
    Public Sub KartaParametrow_ZmienRozwijaIZwijaParametry()
        WWatkuSta(
            Sub(f)
                Dim link = Kontrolka(Of LinkLabel)(f, "lnkParametry")
                Dim parametry = Kontrolka(Of Panel)(f, "grpRozdzielczoscFormat")
                Assert.False(parametry.Visible)
                Dim kliknij = Sub() GetType(LinkLabel).GetMethod("OnLinkClicked", Prywatne).
                                        Invoke(link, {New LinkLabelLinkClickedEventArgs(link.Links(0))})
                kliknij()
                Application.DoEvents()
                Assert.True(parametry.Visible)
                Assert.Equal("Zwiń", link.Text)
                kliknij()
                Application.DoEvents()
                Assert.False(parametry.Visible)
                Assert.Equal("Zmień…", link.Text)
            End Sub)
    End Sub

    <Fact>
    Public Sub PokazNaMapie_PrzyblizaDoMalegoObszaru()
        WWatkuSta(
            Sub(f)
                UstawObszar(f, "680000", "568000", "682000", "570000")
                Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                mapa.Position = New PointLatLng(54.5, 23.5)
                mapa.Zoom = 6
                Application.DoEvents()
                Dim czytelny = GetType(MapoTero.Form1).GetMethod("ObszarCzytelnyNaMapie", Prywatne)
                Assert.False(CBool(czytelny.Invoke(f, Nothing)))
                Assert.True(Kontrolka(Of LinkLabel)(f, "lnkZmienObszar").Visible)

                ' „Pokaż na mapie” z menu „Zmień ▾” karty obszaru
                Dim menu = DirectCast(GetType(MapoTero.Form1).GetField("_menuObszaru", Prywatne).GetValue(f), ContextMenuStrip)
                Dim pokaz = DirectCast(menu.Items("mnuPokazObszar"), ToolStripMenuItem)
                pokaz.PerformClick()
                Application.DoEvents()
                Assert.True(mapa.Zoom >= 11, $"Powiększenie: {mapa.Zoom}")
                Assert.True(CBool(czytelny.Invoke(f, Nothing)))
            End Sub)
    End Sub

    <Fact>
    Public Sub PoleSzukania_WspolrzednePrzesuwajaMapeBezOkna()
        WWatkuSta(
            Sub(f)
                Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                Dim pole = DirectCast(Assert.Single(f.Controls.Find("poleSzukaniaMiejsca", True)), MapoTero.PoleWyszukiwaniaMiejsca)
                pole.Pole.Text = "52.2297, 21.0122"
                pole.Szukaj(automatyczne:=False)
                Application.DoEvents()
                Assert.False(pole.Lista.Visible)
                Assert.InRange(mapa.Position.Lat, 52.22, 52.24)
                Assert.InRange(mapa.Position.Lng, 21.0, 21.02)
            End Sub)
    End Sub

    <Fact>
    Public Sub PoleSzukania_PodpowiedzWspolrzednychNaLiscie_EnterWybiera()
        WWatkuSta(
            Sub(f)
                Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                Dim pole = DirectCast(Assert.Single(f.Controls.Find("poleSzukaniaMiejsca", True)), MapoTero.PoleWyszukiwaniaMiejsca)
                pole.Pole.Text = "54.5189, 18.5305"
                pole.Szukaj(automatyczne:=True)
                Application.DoEvents()
                Assert.True(pole.Lista.Visible)
                Assert.Equal(1, pole.Lista.Items.Count)
                Assert.Same(f, pole.Lista.Parent)
                Assert.True(pole.Lista.Top >= pole.PointToScreen(Drawing.Point.Empty).Y - f.PointToScreen(Drawing.Point.Empty).Y + pole.Height)
                GetType(MapoTero.PoleWyszukiwaniaMiejsca).GetMethod("PoleKeyDown", Prywatne).
                    Invoke(pole, {pole.Pole, New KeyEventArgs(Keys.Enter)})
                Application.DoEvents()
                Assert.False(pole.Lista.Visible)
                Assert.InRange(mapa.Position.Lat, 54.5, 54.53)
            End Sub)
    End Sub

    <Fact>
    Public Sub PrzyciskiZoomu_SaDuzeIRozsuniete()
        WWatkuSta(
            Sub(f)
                Dim plus = Kontrolka(Of Button)(f, "btnPowieksz")
                Dim minus = Kontrolka(Of Button)(f, "btnPomniejsz")
                Assert.True(plus.Font.Size >= 14 AndAlso plus.Font.Bold)
                Assert.True(minus.Top - plus.Bottom >= 6, $"Odstęp: {minus.Top - plus.Bottom}")
                Assert.True(plus.Width >= 34)
            End Sub)
    End Sub

    <Theory>
    <InlineData(100, 500, 400, 400, False, 100, 340)>
    <InlineData(100, 500, 400, 900, True, 100, 0)>
    <InlineData(-50, -20, 400, 300, False, 0, 0)>
    <InlineData(1200, 100, 400, 300, False, 1000, 100)>
    Public Sub OknoNaEkranie_PrzesuwaDoObszaruRoboczego(x As Integer, y As Integer, w As Integer, h As Integer, sizable As Boolean,
                                                        oczekiwaneX As Integer, oczekiwaneY As Integer)
        Dim ekran As New Drawing.Rectangle(0, 0, 1400, 740)
        Dim wynik = MapoTero.OknaNaEkranie.DopasujDoEkranu(New Drawing.Rectangle(x, y, w, h), ekran, sizable)
        Assert.Equal(oczekiwaneX, wynik.X)
        Assert.Equal(oczekiwaneY, wynik.Y)
        Assert.True(wynik.Bottom <= ekran.Bottom OrElse Not sizable AndAlso h > ekran.Height)
        If sizable Then Assert.True(wynik.Height <= ekran.Height)
    End Sub

    <Fact>
    Public Sub Podglad_PrzyciskiNadProbka_JedenPrzyciskMiksera()
        WWatkuSta(
            Sub(f)
                f.WybranyZestaw().Dodaj("Próbka")
                GetType(MapoTero.Form1).GetMethod("OdswiezPrzyciskiWarstw", Prywatne).Invoke(f, Nothing)
                Application.DoEvents()
                Dim probka = Kontrolka(Of PictureBox)(f, "picPodglad")
                Dim ekranProbki = probka.RectangleToScreen(probka.ClientRectangle)
                For Each nazwa In {"btnZoomMinus", "btnZoomPlus", "btnZoomReset", "btnPodgladOdswiez"}
                    Dim b = Kontrolka(Of Button)(f, nazwa)
                    Assert.True(b.Visible, nazwa)
                    Dim ekran = b.RectangleToScreen(b.ClientRectangle)
                    Assert.True(ekran.Bottom <= ekranProbki.Top + 2, $"{nazwa} nie leży nad próbką: {ekran} / {ekranProbki}")
                Next
                Assert.False(Kontrolka(Of Button)(f, "btnPodgladZaawansowane").Visible)
                Assert.True(Kontrolka(Of Button)(f, "btnWarstwyZaawansowane").Visible)
            End Sub)
    End Sub

    <Fact>
    Public Sub PasekNarzedzi_KreatorZaznaczWyszukiwarkaFolder_ZoomKolkiemBezCentrowania()
        WWatkuSta(
            Sub(f)
                Dim kreator = Kontrolka(Of Button)(f, "btnKreatorDolny")
                Dim zaznacz = Kontrolka(Of Button)(f, "btnZaznaczDwaNarozniki")
                Dim folder = Kontrolka(Of Button)(f, "btnFolderPobran")
                Dim pole = Assert.Single(f.Controls.Find("poleSzukaniaMiejsca", True))
                Dim pasek = zaznacz.Parent
                Dim kolejnosc = {kreator, zaznacz, pole, folder}.Select(Function(c) pasek.Controls.GetChildIndex(c)).ToArray()
                Assert.Equal({0, 1, 2}.Select(Function(i) kolejnosc(i) + 1), kolejnosc.Skip(1))
                ' przycisk „Współrzędne…” zastąpiło menu karty obszaru
                Assert.False(Kontrolka(Of Button)(f, "btnWpiszWspolrzedne").Visible)
                ' odstępy: 4 px w grupie, 16 px między grupami
                Assert.True(pole.Margin.Right > zaznacz.Margin.Right * 3, $"{pole.Margin.Right} / {zaznacz.Margin.Right}")
                Assert.Equal(pole.Margin.Right, kreator.Margin.Right)
                Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                Assert.Equal(MouseWheelZoomType.MousePositionWithoutCenter, mapa.MouseWheelZoomType)
                ' przełącznik podkładu podglądu leży w prawym górnym rogu mapy, oddzielony od paska narzędzi
                Dim podklad = Assert.Single(f.Controls.Find("cmbPodkladMapy", True))
                Assert.Same(f, podklad.Parent)
                Assert.True(podklad.Right <= mapa.Right)
                Assert.True(podklad.Top >= mapa.Top)
            End Sub)
    End Sub

    <Fact>
    Public Sub KartaObszaru_MenuZmienZawieraWszystkieSposobyWskazania()
        WWatkuSta(
            Sub(f)
                Dim link = Kontrolka(Of LinkLabel)(f, "lnkZmienObszar")
                Assert.StartsWith("Zmień", link.Text)
                Dim menu = DirectCast(GetType(MapoTero.Form1).GetField("_menuObszaru", Prywatne).GetValue(f), ContextMenuStrip)
                Dim teksty = menu.Items.OfType(Of ToolStripMenuItem)().Select(Function(p) p.Text).ToList()
                Assert.Contains("Pokaż na mapie", teksty)
                Assert.Contains("Zaznacz na mapie", teksty)
                Assert.Contains("Wpisz współrzędne…", teksty)
                Assert.Contains(teksty, Function(t) t.Contains("trasy"))
                Assert.Contains(teksty, Function(t) t.Contains("poligonu"))
                ' bez obszaru „Pokaż na mapie” jest wyłączone, a menu nadal pozwala wskazać obszar
                f.NowaSesja(pytaj:=False)
                UstawObszar(f, "", "", "", "")
                GetType(ContextMenuStrip).GetMethod("OnOpening", Prywatne).Invoke(menu, {New System.ComponentModel.CancelEventArgs()})
                Assert.False(menu.Items("mnuPokazObszar").Enabled)
                Assert.True(menu.Items("mnuWpiszWspolrzedne").Enabled)
            End Sub)
    End Sub

    <Fact>
    Public Sub ListaWarstw_KrycieWWierszuZaznaczonejWarstwy_BezWierszaPrzyciskow()
        WWatkuSta(
            Sub(f)
                f.WybranyZestaw().Dodaj("Ortofotomapa")
                f.WybranyZestaw().Dodaj("Działki")
                GetType(MapoTero.Form1).GetMethod("PokazWarstwy", Prywatne).Invoke(f, Nothing)
                Dim lista = Kontrolka(Of MapoTero.ListaWarstw)(f, "lstWybraneWarstwy")
                lista.Zaznaczona = 1
                Application.DoEvents()
                Dim krycie = Kontrolka(Of NumericUpDown)(f, "numKrycie")
                Assert.Same(lista, krycie.Parent)
                Assert.True(krycie.Visible)
                ' zaznaczona warstwa 1 (na wierzchu) jest w górnym wierszu listy
                Assert.True(krycie.Bottom <= lista.WysokoscWiersza + 4, $"Pole krycia nie w górnym wierszu: {krycie.Bounds}")
                lista.Zaznaczona = 0
                Application.DoEvents()
                Assert.True(krycie.Top >= lista.WysokoscWiersza, $"Pole krycia nie przeszło do dolnego wiersza: {krycie.Bounds}")
                For Each nazwa In {"btnWarstwaWyzej", "btnWarstwaNizej", "btnUsunWarstwe", "lblKrycie"}
                    Assert.False(Kontrolka(Of Control)(f, nazwa).Visible, nazwa)
                Next
                ' „Mikser…” w nagłówku karty, nad listą
                Dim mikser = Kontrolka(Of Button)(f, "btnWarstwyZaawansowane")
                Assert.Same(Kontrolka(Of GroupBox)(f, "grpWybraneWarstwy"), mikser.Parent)
                Assert.True(mikser.Bottom <= lista.Parent.Top + lista.Top, "Mikser nie jest w nagłówku karty")
            End Sub)
    End Sub

    <Fact>
    Public Sub PasekStanu_UkladKursorSkalaISesja()
        WWatkuSta(
            Sub(f)
                Dim pasek = Kontrolka(Of StatusStrip)(f, "StatusStrip1")
                Assert.StartsWith("Układ: PL-1992", pasek.Items("stUklad").Text)
                Assert.Equal("Kursor  —", pasek.Items("stKursor").Text)
                Assert.Matches("^Skala ≈ 1:\d", pasek.Items("stSkala").Text)
                f.NowaSesja(pytaj:=False)
                f.AktualizujStanBelki()
                Assert.StartsWith("Sesja: bez nazwy", pasek.Items("stSesja").Text)
            End Sub)
    End Sub

    <Fact>
    Public Sub PoleSzukania_TekstNaSrodkuWysokosci_PoligonWMenuSesji_UkladWKarcieParametrow()
        WWatkuSta(
            Sub(f)
                Dim pole = DirectCast(Assert.Single(f.Controls.Find("poleSzukaniaMiejsca", True)), MapoTero.PoleWyszukiwaniaMiejsca)
                Dim gora = pole.Pole.Top, dol = pole.ClientSize.Height - pole.Pole.Bottom
                Assert.True(Math.Abs(gora - dol) <= 1, $"Pole tekstu nie jest na środku: góra {gora}, dół {dol}")

                Dim sesja = DirectCast(Assert.Single(f.MainMenuStrip.Items.Find("SesjaToolStripMenuItem", False)), ToolStripMenuItem)
                Dim trasa = sesja.DropDownItems.IndexOfKey("KorytarzToolStripMenuItem")
                Assert.Equal(trasa + 1, sesja.DropDownItems.IndexOfKey("PoligonToolStripMenuItem"))

                UstawObszar(f, "680000", "568000", "682000", "570000")
                f.AktualizujStanBelki()
                Dim etykiety = Kontrolka(Of Control)(f, "kartaStanuSesji").Parent.Controls.Cast(Of Control)().
                    SelectMany(Function(k) k.Controls.Cast(Of Control)()).OfType(Of Label)().Select(Function(l) l.Text).ToList()
                Assert.Contains(etykiety, Function(t) t.StartsWith("PL-1992 · bok", StringComparison.Ordinal))
                Assert.DoesNotContain(etykiety, Function(t) t.Contains("EPSG:2180) ·"))
            End Sub)
    End Sub

    <Theory>
    <InlineData(48321.0, 48000)>
    <InlineData(1234.0, 1200)>
    <InlineData(95.0, 95)>
    <InlineData(0.0, 0)>
    Public Sub Skala_ZaokraglonaDoDwochCyfrZnaczacych(mianownik As Double, oczekiwany As Integer)
        Assert.Equal(oczekiwany, MapoTero.Form1.ZaokraglijSkale(mianownik))
    End Sub

    <Fact>
    Public Sub OpisKursora_KolejnoscGeodezyjnaIPolskiZapis()
        Assert.Equal("Kursor  —", MapoTero.Form1.OpisKursora(Nothing, False))
        Dim opis = MapoTero.Form1.OpisKursora((335482.34, 291076.81), False)
        Assert.Matches("^Kursor  X: 335\s482,3  Y: 291\s076,8 m$", opis)
        Assert.Equal("Kursor  X: 54,51892°  Y: 18,53051°", MapoTero.Form1.OpisKursora((54.518921, 18.530512), True))
    End Sub

    ''' <summary>
    ''' Układ okna powstaje w konstruktorze bez tworzenia okien kontrolek - budowa w Load (z gotowymi uchwytami)
    ''' wydłużała start programu o ok. 1 s. Odwołanie np. do Screen.FromControl w trakcie budowy utworzyłoby uchwyty.
    ''' </summary>
    <Fact>
    Public Sub Konstruktor_BudujeUkladBezTworzeniaUchwytow()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    Assert.False(f.IsHandleCreated)
                    Assert.False(Kontrolka(Of Control)(f, "tvWarstwy").IsHandleCreated)
                    Assert.NotNull(Assert.Single(f.Controls.Find("zakladkiPracy", True)))
                    Assert.NotNull(Assert.Single(f.Controls.Find("poleSzukaniaMiejsca", True)))
                End Using
            End Sub)
    End Sub

    ''' <summary>Arkusz JPEG lub PNG: pole „piramidy w GeoTIFF” odznaczone i wyłączone; po powrocie do GeoTIFF wraca wybór.</summary>
    <Fact>
    Public Sub Eksport_PiramidyGeoTiffTylkoDlaFormatuTiff()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.FormEksport() With {.ArkuszDomyslnie = True}
                    OknoTestowe.Pokaz(f)
                    Application.DoEvents()
                    Dim format = DirectCast(Assert.Single(f.Controls.Find("cmbFormatArkusza", True)), ComboBox)
                    Dim piramidy = DirectCast(Assert.Single(f.Controls.Find("chkGeoTiffPiramidy", True)), CheckBox)
                    DirectCast(Assert.Single(f.Controls.Find("rbGeoTiff", True)), RadioButton).Checked = True
                    format.SelectedIndex = 0
                    Application.DoEvents()
                    Assert.True(piramidy.Checked)
                    format.SelectedIndex = 2 ' JPEG
                    Application.DoEvents()
                    Assert.False(piramidy.Checked)
                    Assert.False(piramidy.Enabled)
                    format.SelectedIndex = 1 ' GeoTIFF z kompresją JPEG
                    Application.DoEvents()
                    Assert.True(piramidy.Checked)
                    Assert.True(piramidy.Enabled)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub PrzyciskiKatalogu_MajaNapisyZamiastSymboli()
        WWatkuSta(
            Sub(f)
                Assert.Contains(Kontrolka(Of Button)(f, "btnZwinRozwin").Text, {"Zwiń", "Rozwiń"})
                Dim serwer = Kontrolka(Of Button)(f, "btnDodajSerwer")
                Assert.Contains("Serwer", serwer.Text)
                ' „+ Serwer…” w nagłówku karty katalogu, nad polem filtru
                Assert.Same(Kontrolka(Of GroupBox)(f, "grpDaneZrodlowe"), serwer.Parent)
                Dim filtr = Kontrolka(Of TextBox)(f, "txtSzukajWarstwy")
                Assert.True(serwer.RectangleToScreen(serwer.ClientRectangle).Bottom <= filtr.RectangleToScreen(filtr.ClientRectangle).Top)
                Dim drzewo = Kontrolka(Of TreeView)(f, "tvWarstwy")
                For Each wezel As TreeNode In drzewo.Nodes
                    Assert.False(String.IsNullOrEmpty(wezel.ToolTipText), $"Grupa bez podpowiedzi: {wezel.Text}")
                Next
            End Sub)
    End Sub

    <Fact>
    Public Sub ZaznaczObszar_PodswietlaPrzyciskIPokazujeBanerNadMapa_EscZamyka()
        WWatkuSta(
            Sub(f)
                Kontrolka(Of CheckBox)(f, "chkEdycjaXY").Checked = False
                Dim zaznacz = Kontrolka(Of Button)(f, "btnZaznaczDwaNarozniki")
                Dim baner = Kontrolka(Of Label)(f, "lblBanerZaznaczania")
                Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                Dim plus = Kontrolka(Of Button)(f, "btnPowieksz")
                Assert.False(baner.Visible)

                zaznacz.PerformClick()
                Application.DoEvents()

                Assert.Equal(MapoTero.Form1.KolorAkcjiOkna, zaznacz.BackColor)
                Assert.Equal(Drawing.Color.White, zaznacz.ForeColor)
                Assert.True(baner.Visible)
                Dim mapaEkran = mapa.RectangleToScreen(mapa.ClientRectangle)
                Dim banerEkran = baner.RectangleToScreen(baner.ClientRectangle)
                Dim plusEkran = plus.RectangleToScreen(plus.ClientRectangle)
                Assert.True(mapaEkran.Contains(banerEkran), $"Baner poza mapą: {banerEkran} / {mapaEkran}")
                Assert.True(banerEkran.Left >= plusEkran.Right, $"Baner nachodzi na zoom: {banerEkran} / {plusEkran}")

                Dim args As Object() = {New Message(), Keys.Escape}
                GetType(MapoTero.Form1).GetMethod("ProcessCmdKey", Prywatne).Invoke(f, args)
                Application.DoEvents()

                Assert.False(baner.Visible)
                Assert.Equal(Drawing.Color.White, zaznacz.BackColor)
                Assert.Equal("Zaznacz obszar", zaznacz.Text)
            End Sub)
    End Sub

    <Fact>
    Public Sub Komunikaty_LicznikBledowZerujeSiePoOtwarciuZakladkiINowejSesji_PrzyciskiKopiujIWyczysc()
        WWatkuSta(
            Sub(f)
                Dim zakladki = Kontrolka(Of TabControl)(f, "zakladkiPracy")
                Dim kopiuj = Kontrolka(Of Button)(f, "btnKopiujKomunikaty")
                Dim wyczysc = Kontrolka(Of Button)(f, "btnWyczyscKomunikaty")
                Dim dziennik = Kontrolka(Of RichTextBox)(f, "dziennikSesji")
                zakladki.SelectedIndex = 0
                Application.DoEvents()

                f.Komunikat("Błąd próbny", Drawing.Color.Red)
                Application.DoEvents()
                Assert.Equal("Komunikaty (1)", zakladki.TabPages(1).Text)
                Assert.True(kopiuj.Enabled)
                Assert.True(wyczysc.Enabled)

                zakladki.SelectedIndex = 1
                Application.DoEvents()
                Assert.Equal("Komunikaty", zakladki.TabPages(1).Text)

                wyczysc.PerformClick()
                Application.DoEvents()
                Assert.Equal("", dziennik.Text)
                Assert.False(kopiuj.Enabled)
                Assert.False(wyczysc.Enabled)

                zakladki.SelectedIndex = 0
                Application.DoEvents()
                f.Komunikat("Drugi błąd", Drawing.Color.Red)
                Application.DoEvents()
                Assert.Equal("Komunikaty (1)", zakladki.TabPages(1).Text)

                f.NowaSesja(pytaj:=False)
                Application.DoEvents()
                Assert.Equal("Komunikaty", zakladki.TabPages(1).Text)
            End Sub)
    End Sub

    <Fact>
    Public Sub SzukajWarstwy_PrzyciskCzyszczeniaCzysciFiltr()
        WWatkuSta(
            Sub(f)
                Dim txt = Kontrolka(Of TextBox)(f, "txtSzukajWarstwy")
                Dim ikona = Kontrolka(Of Label)(f, "btnWyczyscSzukajWarstwy")
                Assert.Equal("", ikona.Text)
                Assert.NotNull(ikona.Image)

                txt.Text = "orto"
                Application.DoEvents()
                Assert.Equal("✕", ikona.Text)
                Assert.Null(ikona.Image)

                GetType(Label).GetMethod("OnClick", Prywatne).Invoke(ikona, {EventArgs.Empty})
                Application.DoEvents()
                Assert.Equal("", txt.Text)
                Assert.Equal("", ikona.Text)
                Assert.NotNull(ikona.Image)
            End Sub)
    End Sub

    <Fact>
    Public Sub ZakladkaSesja_PrzyciskiMajaPlaskiStylIPodpisUsunPobrane()
        WWatkuSta(
            Sub(f)
                Dim zakladki = Kontrolka(Of TabControl)(f, "zakladkiPracy")
                zakladki.SelectedIndex = 2
                Application.DoEvents()

                Dim otworz = Kontrolka(Of Button)(f, "btnOtworzFolderDolny")
                Dim usun = Kontrolka(Of Button)(f, "btnUsunPobrane")
                Assert.Equal(FlatStyle.Flat, otworz.FlatStyle)
                Assert.Equal(FlatStyle.Flat, usun.FlatStyle)
                Assert.Equal("Usuń pobrane", usun.Text)
                Assert.NotNull(usun.Image)
            End Sub)
    End Sub

    <Fact>
    Public Sub ListaWarstw_SkrotTrybuMieszania()
        Assert.Equal("", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Zwykly))
        Assert.Equal("Mnoż.", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Mnozenie))
        Assert.Equal("Ciemn.", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Ciemniej))
        Assert.Equal("Jaśn.", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Jasniej))
        Assert.Equal("Ekran", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Ekran))
        Assert.Equal("Nakł.", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.Nakladka))
        Assert.Equal("Rzeźba", MapoTero.ListaWarstw.SkrotTrybuMieszania(MapoTero.Core.TrybMieszania.RzezbaTerenu))
    End Sub

    <Fact>
    Public Sub KartaParametrow_EscZwijaRozwinietyPanel()
        WWatkuSta(
            Sub(f)
                Dim link = Kontrolka(Of LinkLabel)(f, "lnkParametry")
                Dim parametry = Kontrolka(Of Panel)(f, "grpRozdzielczoscFormat")
                GetType(LinkLabel).GetMethod("OnLinkClicked", Prywatne).
                    Invoke(link, {New LinkLabelLinkClickedEventArgs(link.Links(0))})
                Application.DoEvents()
                Assert.True(parametry.Visible)

                parametry.Focus()
                Dim args As Object() = {New Message(), Keys.Escape}
                Dim obsluzony = CBool(GetType(MapoTero.Form1).GetMethod("ProcessCmdKey", Prywatne).Invoke(f, args))
                Application.DoEvents()
                Assert.True(obsluzony)
                Assert.False(parametry.Visible)
                Assert.Equal("Zmień…", link.Text)
            End Sub)
    End Sub

End Class
