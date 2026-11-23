'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Testy zmodernizowanej dolnej belki (kokpit: obszar i szacunek, rozdzielczość i format, podgląd).
''' </summary>
<Collection("OknaKreatora")>
Public Class KokpitDolnaBelkaTesty

    Private Shared Sub WWatkuSta(akcja As Action)
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    akcja()
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    Private Shared Function Kontrolka(Of T As Control)(f As Form, nazwa As String) As T
        Dim znalezione = f.Controls.Find(nazwa, True)
        Assert.NotEmpty(znalezione)
        Return DirectCast(znalezione(0), T)
    End Function

    <Fact>
    Public Sub KontrolkiNowegoKokpituIstnieja()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    ' Belka akcji (Opcja 3)
                    Assert.NotNull(Kontrolka(Of Panel)(f, "pnlBelkaAkcji"))
                    Dim btnKreator = Kontrolka(Of Button)(f, "btnKreatorDolny")
                    Assert.NotNull(btnKreator)
                    Assert.Contains("kreator", btnKreator.Text, StringComparison.OrdinalIgnoreCase)

                    ' Karta 1: Obszar i szacunek
                    Assert.NotNull(Kontrolka(Of Panel)(f, "grpObszar"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblBadgeSegmenty"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblWymiaryTerenu"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblPowierzchniaTerenu"))
                    Assert.NotNull(Kontrolka(Of Panel)(f, "pnlSzacunek"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblSzacowanyRozmiar"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblSzacowanyCzas"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblUkladZasiegSkrot"))
                    Assert.NotNull(Kontrolka(Of Button)(f, "btnWpiszWspolrzedne"))

                    ' Karta 2: Rozdzielczość i format
                    Assert.NotNull(Kontrolka(Of Panel)(f, "grpRozdzielczoscFormat"))
                    Assert.NotNull(Kontrolka(Of TextBox)(f, "txtRozmiarPiksela"))
                    Assert.NotNull(Kontrolka(Of ComboBox)(f, "cmbRozmiarPiksela"))
                    Assert.NotNull(Kontrolka(Of TextBox)(f, "txtBokSegmentu"))
                    Assert.NotNull(Kontrolka(Of Label)(f, "lblBokKm"))
                    Assert.NotNull(Kontrolka(Of ComboBox)(f, "cmbFormatDocelowy"))
                    Assert.NotNull(Kontrolka(Of Button)(f, "btnPrzejdzDoEksportu"))

                    ' Karta 3: Podgląd
                    Assert.NotNull(Kontrolka(Of Panel)(f, "grpPodglad"))
                    Assert.NotNull(Kontrolka(Of PictureBox)(f, "picPodglad"))
                    Dim btnMikser = Kontrolka(Of Button)(f, "btnPodgladZaawansowane")
                    Assert.Contains("Mikser", btnMikser.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub ZakladkiPracy_PokazujaHistorieIZachowujaListeWarstw()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim zakladki = Kontrolka(Of TabControl)(f, "zakladkiPracy")
                    Assert.Equal("Podgląd", zakladki.TabPages(0).Text)
                    Assert.StartsWith("Komunikaty", zakladki.TabPages(1).Text)
                    Assert.Equal("Sesja", zakladki.TabPages(2).Text)
                    Assert.Same(zakladki.TabPages(0), Kontrolka(Of Panel)(f, "grpPodglad").Parent)

                    f.Komunikat("Rozpoczęto pobieranie próbne", Drawing.Color.Black)
                    f.Komunikat("Próba: brak odpowiedzi serwera", Drawing.Color.Red)
                    zakladki.SelectedIndex = 1
                    Dim historia = Kontrolka(Of RichTextBox)(f, "dziennikSesji")
                    Assert.Contains("Rozpoczęto pobieranie próbne", historia.Text)
                    Assert.Contains("Próba: brak odpowiedzi serwera", historia.Text)
                    Assert.True(historia.ReadOnly)
                    Assert.True(Kontrolka(Of GroupBox)(f, "grpWybraneWarstwy").Visible)
                    Assert.True(Kontrolka(Of TreeView)(f, "tvWarstwy").Visible)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub ZaznaczenieDwochNaroznikow_ZatwierdzaZasiegPoDrugimKliknieciu()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Kontrolka(Of CheckBox)(f, "chkEdycjaXY").Checked = False
                    Dim zaznacz = Kontrolka(Of Button)(f, "btnZaznaczDwaNarozniki")
                    Dim mapa = Kontrolka(Of GMap.NET.WindowsForms.GMapControl)(f, "mapa")
                    Dim xDol = Kontrolka(Of TextBox)(f, "txtXDol")
                    Dim przed = xDol.Text
                    zaznacz.PerformClick()
                    Assert.False(mapa.CanDragMap)

                    Dim ruch = GetType(MapoTero.Form1).GetMethod("mapa_MouseUp", BindingFlags.NonPublic Or BindingFlags.Instance)
                    ruch.Invoke(f, New Object() {mapa, New MouseEventArgs(MouseButtons.Left, 1, 120, 120, 0)})
                    Assert.Equal(przed, xDol.Text)
                    ruch.Invoke(f, New Object() {mapa, New MouseEventArgs(MouseButtons.Left, 1, 170, 160, 0)})
                    Assert.NotEqual(przed, xDol.Text)
                    Assert.True(mapa.CanDragMap)
                    Assert.Equal("Zaznacz obszar", zaznacz.Text)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub PolaZasieguZgodnoscWsteczna()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    ' Pula pól tekstowych, do których odwołują się inne procedury i testy automatyczne
                    For Each nazwa In {"txtXDol", "txtYLewy", "txtXGora", "txtYPrawy",
                                       "txtLiczbaKolumn", "txtLiczbaWierszy", "txtSzerokoscKm", "txtWysokoscKm",
                                       "txtSzerokoscPx", "txtWysokoscPx", "chkEdycjaXY"}
                        Dim kontrolki = f.Controls.Find(nazwa, True)
                        Assert.Single(kontrolki)
                        Assert.NotNull(kontrolki(0))
                    Next

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub AktualizacjaSzacunkuIWymiarow()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim txtXDol = Kontrolka(Of TextBox)(f, "txtXDol")
                    Dim txtXGora = Kontrolka(Of TextBox)(f, "txtXGora")
                    Dim txtYLewy = Kontrolka(Of TextBox)(f, "txtYLewy")
                    Dim txtYPrawy = Kontrolka(Of TextBox)(f, "txtYPrawy")
                    Dim txtRozm = Kontrolka(Of TextBox)(f, "txtRozmiarPiksela")
                    Dim txtBok = Kontrolka(Of TextBox)(f, "txtBokSegmentu")

                    txtRozm.Text = "1"
                    txtBok.Text = "2000"
                    txtXDol.Text = "680000"
                    txtXGora.Text = "686000" ' 6 km wysokość
                    txtYLewy.Text = "568000"
                    txtYPrawy.Text = "572000" ' 4 km szerokość = 6 segmentów (3 wiersze x 2 kolumny)

                    Application.DoEvents()

                    Dim lblWymiary = Kontrolka(Of Label)(f, "lblWymiaryTerenu")
                    Dim lblPow = Kontrolka(Of Label)(f, "lblPowierzchniaTerenu")
                    Dim lblBadge = Kontrolka(Of Label)(f, "lblBadgeSegmenty")
                    Dim lblRozmiar = Kontrolka(Of Label)(f, "lblSzacowanyRozmiar")
                    Dim lblCzas = Kontrolka(Of Label)(f, "lblSzacowanyCzas")

                    Assert.Contains("4", lblWymiary.Text)
                    Assert.Contains("6", lblWymiary.Text)
                    Assert.Contains("24", lblPow.Text)
                    Assert.Contains("6 segment", lblBadge.Text)

                    Assert.False(String.IsNullOrWhiteSpace(lblRozmiar.Text))
                    Assert.NotEqual("-", lblRozmiar.Text)
                    Assert.False(String.IsNullOrWhiteSpace(lblCzas.Text))
                    Assert.NotEqual("-", lblCzas.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub PresetPikselaSynchronizacja()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim cmbPiksel = Kontrolka(Of ComboBox)(f, "cmbRozmiarPiksela")
                    Dim txtPiksel = Kontrolka(Of TextBox)(f, "txtRozmiarPiksela")

                    ' Wybór z listy aktualizuje pole tekstowe
                    cmbPiksel.SelectedIndex = 1 ' 0.25 m/px
                    Application.DoEvents()
                    Assert.Equal("0.25", txtPiksel.Text)

                    ' Wpisanie wartości aktualizuje indeks listy
                    txtPiksel.Text = "2"
                    Application.DoEvents()
                    Assert.Equal(4, cmbPiksel.SelectedIndex)

                    ' Wartość niestandardowa
                    txtPiksel.Text = "1.5"
                    Application.DoEvents()
                    Assert.Equal(7, cmbPiksel.SelectedIndex)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub FormZasiegXYZaokraglenie()
        WWatkuSta(
            Sub()
                Using okno As New MapoTero.FormZasiegXY()
                    okno.XDol = "680250"
                    okno.XGora = "686750"
                    okno.YLewy = "568100"
                    okno.YPrawy = "572900"

                    okno.ZaokraglijDoKm()

                    Assert.Equal("680000", okno.XDol)
                    Assert.Equal("687000", okno.XGora)
                    Assert.Equal("568000", okno.YLewy)
                    Assert.Equal("573000", okno.YPrawy)
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub BelkaIZakladkiMieszczaSiePoPrzywroceniuOkna()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    ' Symulacja "Przywróć w dół" (stan normalny okna)
                    f.WindowState = FormWindowState.Normal
                    Application.DoEvents()

                    Dim pobierz = Kontrolka(Of Button)(f, "btnPobierz")
                    Dim eksport = Kontrolka(Of Button)(f, "btnPrzejdzDoEksportu")
                    Dim belka = Kontrolka(Of Panel)(f, "Panel3")
                    Dim prawyPanel = Kontrolka(Of Panel)(f, "Panel2")
                    Dim zakladki = Kontrolka(Of TabControl)(f, "zakladkiPracy")
                    Dim mapa = Kontrolka(Of Control)(f, "mapa")
                    Dim pasekStanu = Kontrolka(Of StatusStrip)(f, "StatusStrip1")

                    Assert.Equal(mapa.Width, belka.Width)
                    Assert.Equal(pasekStanu.Top, prawyPanel.Bottom)
                    Assert.True(belka.Right <= prawyPanel.Left)

                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {.Wszystkie = 1}
                    f.AktualizujStanBelki()
                    Application.DoEvents()
                    Dim pobierzNaEkranie = pobierz.RectangleToScreen(pobierz.ClientRectangle)
                    Dim eksportNaEkranie = eksport.RectangleToScreen(eksport.ClientRectangle)
                    Dim belkaNaEkranie = belka.RectangleToScreen(belka.ClientRectangle)
                    Assert.True(belkaNaEkranie.Contains(pobierzNaEkranie), $"Pobieranie: {pobierzNaEkranie}, belka: {belkaNaEkranie}")
                    Assert.True(belkaNaEkranie.Contains(eksportNaEkranie), $"Eksport: {eksportNaEkranie}, belka: {belkaNaEkranie}")
                    'przyciski jeden pod drugim w karcie „Stan sesji”
                    Assert.False(pobierzNaEkranie.IntersectsWith(eksportNaEkranie), $"Pobieranie: {pobierzNaEkranie}, eksport: {eksportNaEkranie}")
                    Assert.True(zakladki.Right <= prawyPanel.ClientSize.Width, $"Zakładki: {zakladki.Bounds}, panel: {prawyPanel.ClientSize}")
                    Dim prawaZakladka = zakladki.GetTabRect(zakladki.TabCount - 1)
                    Assert.InRange(prawaZakladka.Right, zakladki.ClientSize.Width - 12, zakladki.ClientSize.Width)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub NoweKontrolkiBelkiAkcjiIstnieja()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim btnFolder = Kontrolka(Of Button)(f, "btnOtworzFolderDolny")
                    Assert.NotNull(btnFolder)
                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    Assert.NotNull(lblStatus)
                    Dim lnk = Kontrolka(Of LinkLabel)(f, "lnkSzczegoly")
                    Assert.NotNull(lnk)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_DomyslnyZasiegISiatka()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    f.AktualizujStanBelki()
                    ' Stan sesji przed pobraniem jest czysty (informacje o siatce są w bloku ZAZNACZONY OBSZAR)
                    Assert.Equal("", lblStatus.Text)

                    Dim lblBadge = Kontrolka(Of Label)(f, "lblBadgeSegmenty")
                    Assert.Contains("Siatka:", lblBadge.Text)
                    Assert.Contains("segment", lblBadge.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_ObszarKorytarza()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim k As New Korytarz With {.SzerokoscMetry = 25}
                    Dim u = UkladWspolrzednych.PL1992
                    k.DodajLinie({u.DoWgs84(500000, 500000), u.DoWgs84(500100, 500200)})
                    f._korytarz = k
                    f.PrzeliczSiatke()
                    f.AktualizujStanBelki()

                    Dim lblBadge = Kontrolka(Of Label)(f, "lblBadgeSegmenty")
                    Assert.Contains("wzdłuż trasy", lblBadge.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_WynikPobrania_SukcesWyrozniaEksport()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    Dim btnEksport = Kontrolka(Of Button)(f, "btnPrzejdzDoEksportu")
                    Assert.False(btnEksport.Visible)
                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {
                        .Wszystkie = 24, .Nieudane = 0, .Przerwano = False, .Odswiez = False, .Folder = "C:\dummy"
                    }
                    f.AktualizujStanBelki()

                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    Dim btnPobierz = Kontrolka(Of Button)(f, "btnPobierz")
                    Dim lnkSzczegoly = Kontrolka(Of LinkLabel)(f, "lnkSzczegoly")

                    Assert.Equal("24 z 24 segmentów gotowe", lblStatus.Text)
                    Assert.Equal("Pobierz segmenty", btnPobierz.Text)
                    Assert.False(lnkSzczegoly.Visible)
                    Assert.True(f._podswietlEksport)
                    Assert.True(btnEksport.Visible)
                    Assert.Equal("Eksportuj pobraną mapę…", btnEksport.Text)
                    Assert.Equal(Drawing.Color.White, btnEksport.ForeColor)
                    Assert.NotEqual(btnPobierz.BackColor, btnEksport.BackColor)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Podglad_MoznaOddalicIWrocicDoDopasowania()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    f.WybranyZestaw().Dodaj("Próbka")
                    Dim typ = GetType(MapoTero.Form1)
                    Dim prywatne = BindingFlags.NonPublic Or BindingFlags.Instance
                    typ.GetMethod("OdswiezPrzyciskiWarstw", prywatne).Invoke(f, Nothing)
                    typ.GetField("_bazaPodgladu", prywatne).SetValue(f, New Drawing.Bitmap(300, 300))
                    Dim zoom = typ.GetMethod("PowiekszeniePodgladu", prywatne)
                    typ.GetMethod("ZastosujZoomPodgladu", prywatne).Invoke(f, Nothing)
                    Dim plus = Kontrolka(Of Button)(f, "btnZoomPlus")
                    Dim minus = Kontrolka(Of Button)(f, "btnZoomMinus")
                    Dim dopasuj = Kontrolka(Of Button)(f, "btnZoomReset")
                    Dim poczatek = CDbl(zoom.Invoke(f, Nothing))
                    plus.PerformClick()
                    Dim przyblizenie = CDbl(zoom.Invoke(f, Nothing))
                    Assert.True(przyblizenie > poczatek)
                    minus.PerformClick()
                    Assert.True(CDbl(zoom.Invoke(f, Nothing)) < przyblizenie)
                    dopasuj.PerformClick()
                    Assert.Equal(poczatek, CDbl(zoom.Invoke(f, Nothing)), precision:=3)
                    Assert.False(dopasuj.Enabled)
                    Assert.StartsWith("Podgląd ", Kontrolka(Of Label)(f, "lblPodgladSkala").Text)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_WynikPobrania_BrakiSegmentow()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {
                        .Wszystkie = 24, .Nieudane = 5, .Przerwano = False, .Odswiez = False, .Folder = "C:\dummy"
                    }
                    f.AktualizujStanBelki()

                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    Dim btnPobierz = Kontrolka(Of Button)(f, "btnPobierz")

                    Assert.Equal("Brakuje 5 segmentów", lblStatus.Text)
                    Assert.Equal("Pobierz brakujące", btnPobierz.Text)
                    Assert.False(f._podswietlEksport)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_WynikPobrania_NieudaneOdswiezenie()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {
                        .Wszystkie = 24, .Nieudane = 3, .Przerwano = False, .Odswiez = True, .Folder = "C:\dummy"
                    }
                    f.AktualizujStanBelki()

                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    Dim btnPobierz = Kontrolka(Of Button)(f, "btnPobierz")

                    Assert.Equal("Nie udało się odświeżyć 3 segmentów", lblStatus.Text)
                    Assert.Equal("Ponów odświeżanie", btnPobierz.Text)
                    Assert.False(f._podswietlEksport)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_UniewaznieniePoZmianieZasiegu()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()

                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {
                        .Wszystkie = 24, .Nieudane = 0, .Przerwano = False, .Odswiez = False, .Folder = "C:\dummy"
                    }
                    f.AktualizujStanBelki()
                    Assert.True(f._ostatniWynikPobierania.HasValue)

                    Dim txtXDol = Kontrolka(Of TextBox)(f, "txtXDol")
                    txtXDol.Text = "670000"
                    Application.DoEvents()

                    Assert.False(f._ostatniWynikPobierania.HasValue)
                    Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                    Assert.Equal("", lblStatus.Text)
                    Dim lblBadge = Kontrolka(Of Label)(f, "lblBadgeSegmenty")
                    Assert.Contains("Siatka:", lblBadge.Text)

                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub StanBelki_ZmienioneWarstwyWznowienie()
        WWatkuSta(
            Sub()
                Dim tempDir = Path.Combine(Path.GetTempPath(), "MapoTero_Belka_" & Guid.NewGuid().ToString("N"))
                Directory.CreateDirectory(tempDir)
                Try
                    Using f As New MapoTero.Form1()
                        f.Show()
                        Application.DoEvents()

                        Dim txtXDol = Kontrolka(Of TextBox)(f, "txtXDol")
                        Dim txtXGora = Kontrolka(Of TextBox)(f, "txtXGora")
                        Dim txtYLewy = Kontrolka(Of TextBox)(f, "txtYLewy")
                        Dim txtYPrawy = Kontrolka(Of TextBox)(f, "txtYPrawy")
                        Dim txtRozm = Kontrolka(Of TextBox)(f, "txtRozmiarPiksela")
                        Dim txtBok = Kontrolka(Of TextBox)(f, "txtBokSegmentu")
                        txtRozm.Text = "1"
                        txtBok.Text = "2000"
                        txtXDol.Text = "680000"
                        txtXGora.Text = "686000"
                        txtYLewy.Text = "568000"
                        txtYPrawy.Text = "572000"
                        Application.DoEvents()

                        Dim cfg As New KonfiguracjaSesji With {
                            .XDol = "680000", .XGora = "686000", .YLewy = "568000", .YPrawy = "572000",
                            .BokSegmentuPx = "2000", .RozmiarPiksela = "1", .UkladEpsg = 2180,
                            .LiczbaWarstw = 1
                        }
                        cfg.Warstwy(0) = "ZupelnieInnaWarstwa"
                        cfg.Zapisz(Path.Combine(tempDir, "conf.txt"), Encoding.Default)

                        f.FolderSesji = tempDir
                        f._ostatniWynikPobierania = Nothing
                        f.AktualizujStanBelki()

                        Dim lblStatus = Kontrolka(Of Label)(f, "lblStatusSesji")
                        Assert.Equal("Zmieniono warstwy. Folder sesji zawiera segmenty poprzedniej mapy", lblStatus.Text)
                        Assert.False(f._podswietlEksport)

                        f.Close()
                    End Using
                Finally
                    If Directory.Exists(tempDir) Then Directory.Delete(tempDir, True)
                End Try
            End Sub)
    End Sub

End Class
