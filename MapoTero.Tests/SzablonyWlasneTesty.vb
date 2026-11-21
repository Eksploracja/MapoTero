'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'Własne style kreatora: zapis i odczyt pliku, kolejność na liście, zapis kompozycji z kreatora.
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

<Collection("OknaKreatora")>
Public Class SzablonyWlasneTesty

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_style_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(f)
        Return f
    End Function

    <Fact>
    Public Sub ZapisIOdczyt_ZachowujeSkaleWarstwyITryby()
        Dim folder = NowyFolder()
        Try
            Dim plik = Path.Combine(folder, SzablonyWlasne.NazwaPliku)
            Dim wielo As New SzablonMapy(SzablonyWlasne.NoweId(), "Moje WIG – łąki", SzablonyWlasne.KategoriaWlasna, "opis ąę", "⭐", 2.0,
                                         ProfilKorektyRastra.StareMapyKontrast, CelMapy.OdbiornikGarmin) With {.ProbkaLat = 52.4, .ProbkaLon = 20.7, .ProbkaPiksel = 6}
            wielo.DodajStopien("1:25 000", 2.0, New WarstwaSzablonu("mapy_historyczne_IHPAN", "wig25k_2180", 80, True, TrybMieszania.Zwykly, "WIG 25k").ZGrupa("Arkusze"),
                               New WarstwaSzablonu("cien", "ISOK_Cien", 40, False, TrybMieszania.RzezbaTerenu).ZRegulacja(RodzajRegulacji.Cieniowanie))
            wielo.DodajStopien("1:100 000", 6.0, New WarstwaSzablonu("mapy_historyczne_IHPAN", "wig100k_2180", 100))
            Dim jedno As New SzablonMapy(SzablonyWlasne.NoweId(), "Serwer własny", SzablonyWlasne.KategoriaWlasna, "", "⭐", 1.0, ProfilKorektyRastra.Brak)
            jedno.Warstwy.Add(New WarstwaSzablonu("", "warstwa_x", 70) With {.Adres = "https://example.invalid/wms?", .TypZrodla = TypUslugiMapowej.Wms})

            SzablonyWlasne.Zapisz(plik, {wielo, jedno})
            Dim tekst = File.ReadAllText(plik)
            Assert.Contains("łąki", tekst)
            Assert.Contains("RzezbaTerenu", tekst)

            Dim wczytane = SzablonyWlasne.Wczytaj(plik)
            Assert.Equal(2, wczytane.Count)
            Dim w = wczytane(0)
            Assert.Equal(wielo.Id, w.Id)
            Assert.True(SzablonyWlasne.CzyWlasny(w))
            Assert.Equal(SzablonyWlasne.KategoriaWlasna, w.Kategoria)
            Assert.True(w.Wieloskalowy)
            Assert.Equal({"1:25 000", "1:100 000"}, w.StopnieSkali.Select(Function(st) st.Opis).ToArray())
            Assert.Equal(6.0, w.StopnieSkali(1).Piksel)
            Dim cien = w.StopnieSkali(0).Warstwy(1)
            Assert.Equal(40, cien.Krycie)
            Assert.Equal(TrybMieszania.RzezbaTerenu, cien.TrybMieszania)
            Assert.Equal(RodzajRegulacji.Cieniowanie, cien.Regulacja)
            Assert.Equal("Arkusze", w.StopnieSkali(0).Warstwy(0).Grupa)
            Assert.True(w.StopnieSkali(0).Warstwy(0).Nakladka)
            Assert.Equal({CelMapy.OdbiornikGarmin}, w.DozwoloneCele.ToArray())
            Assert.Equal(ProfilKorektyRastra.StareMapyKontrast, w.ProfilKorekty)
            Assert.Equal(52.4, w.ProbkaLat)
            ' warstwa spoza folderu warstwy - adres zapamiętany w stylu trafia do zestawu
            Dim zestaw = wczytane(1).UtworzZestaw(New Dictionary(Of String, PlikWarstw)(), 0.0)
            Assert.Equal("https://example.invalid/wms?", zestaw(0).Adres)
            Assert.Equal(70, zestaw(0).Krycie)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub BrakPliku_PustaLista_UszkodzonyPlik_Wyjatek()
        Dim folder = NowyFolder()
        Try
            Dim plik = Path.Combine(folder, SzablonyWlasne.NazwaPliku)
            Assert.Empty(SzablonyWlasne.Wczytaj(plik))
            File.WriteAllText(plik, "{ to nie jest json")
            Assert.ThrowsAny(Of Exception)(Function() SzablonyWlasne.Wczytaj(plik))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ListaStylow_WlasnePrzedWarstwamiOknaGlownego()
        Dim styl As New SzablonMapy(SzablonyWlasne.NoweId(), "Mój", SzablonyWlasne.KategoriaWlasna, "", "⭐", 1.0, ProfilKorektyRastra.Brak)
        styl.Warstwy.Add(New WarstwaSzablonu("TOPO_WMTS", "MAPA TOPOGRAFICZNA", 100))
        Try
            ZarzadcaSzablonow.UstawWlasne({styl})
            Dim lista = ZarzadcaSzablonow.PobierzDlaCelu(CelMapy.TelefonTablet)
            Dim iWlasny = lista.FindIndex(Function(s) s.Id = styl.Id)
            Dim iOkna = lista.FindIndex(Function(s) s.Id = "wlasny_zestaw")
            Assert.True(iWlasny >= 0)
            If iOkna >= 0 Then Assert.Equal(iOkna - 1, iWlasny)
            Assert.Same(styl, ZarzadcaSzablonow.ZnajdzSzablon(styl.Id))
            Assert.False(SzablonyWlasne.CzyWlasny(ZarzadcaSzablonow.ZnajdzSzablon("wlasny_zestaw")))
        Finally
            ZarzadcaSzablonow.UstawWlasne(Nothing)
        End Try
    End Sub

    ''' <summary>Kreator: mapy WIG z suwakiem cieniowania na 20% zapisane jako styl - trzy skale, krycie z suwaka, styl zaznaczony na liście.</summary>
    <Fact>
    Public Sub Kreator_ZapisujeKompozycjeIUsuwaStyl()
        Dim folder = NowyFolder()
        Dim staryFolder = Global.MapoTero.Program.FolderDanych
        Dim wyjatek As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    Global.MapoTero.Program.FolderDanych = folder
                                    Using f As New Global.MapoTero.Form1()
                                        Using k As New Global.MapoTero.FormKreatorMapy(f)
                                            k.Show() : Application.DoEvents()
                                            ' cel zapamiętany z poprzedniego testu mógłby nie mieć stylu WIG na liście
                                            DirectCast(k.Controls.Find("rbCelTelefon", True)(0), RadioButton).Checked = True
                                            Dim dalej = DirectCast(k.Controls.Find("btnDalej", True)(0), Button)
                                            dalej.PerformClick() : Application.DoEvents()
                                            dalej.PerformClick() : Application.DoEvents()
                                            Dim lst = DirectCast(k.Controls.Find("lstSzablony", True)(0), ListBox)
                                            For i = 0 To lst.Items.Count - 1
                                                If DirectCast(lst.Items(i), SzablonMapy).Id = "historia_wig" Then lst.SelectedIndex = i
                                            Next
                                            Application.DoEvents()
                                            Dim tb = DirectCast(k.Controls.Find("tbCieniowanie", True)(0), TrackBar)
                                            tb.Value = 20
                                            k.GetType().GetMethod("ZmienRegulacje", BindingFlags.NonPublic Or BindingFlags.Instance).Invoke(k, New Object() {0, 20})

                                            Dim styl = k.ZapiszStylWlasny("Test WIG", Function(p) True)
                                            Assert.NotNull(styl)
                                            Assert.True(File.Exists(Path.Combine(folder, SzablonyWlasne.NazwaPliku)))
                                            Assert.True(3 = styl.StopnieSkali.Count, $"skale {styl.StopnieSkali.Count}, warstwy {styl.Warstwy.Count}, opis {styl.Opis}")
                                            Dim cien = styl.StopnieSkali(0).Warstwy.First(Function(w) w.Warstwa = "ISOK_Cien")
                                            Assert.Equal(20, cien.Krycie)
                                            Assert.Equal(styl.Id, DirectCast(lst.SelectedItem, SzablonMapy).Id)
                                            Assert.True(DirectCast(k.Controls.Find("btnUsunStyl", True)(0), Button).Enabled)

                                            ' ponowne wczytanie z pliku (np. po uruchomieniu programu)
                                            Dim zPliku = SzablonyWlasne.Wczytaj(Path.Combine(folder, SzablonyWlasne.NazwaPliku))
                                            Assert.Equal("Test WIG", zPliku.Single().Nazwa)

                                            k.UsunStylWlasny(styl)
                                            Assert.Empty(SzablonyWlasne.Wczytaj(Path.Combine(folder, SzablonyWlasne.NazwaPliku)))
                                            Assert.DoesNotContain(lst.Items.Cast(Of SzablonMapy)(), Function(s) s.Id = styl.Id)
                                        End Using
                                    End Using
                                Catch ex As Exception
                                    wyjatek = ex
                                Finally
                                    Global.MapoTero.Program.FolderDanych = staryFolder
                                    ZarzadcaSzablonow.UstawWlasne(Nothing)
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        Directory.Delete(folder, True)
        If wyjatek IsNot Nothing Then Throw New Exception(wyjatek.Message, wyjatek)
    End Sub

End Class
