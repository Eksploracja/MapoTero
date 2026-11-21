'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'Testy poprawek kreatora mapy po przeglądzie (2026-11): każdy test odpowiada znalezionemu błędowi.
Imports System
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

<Collection("OknaKreatora")>
Public Class KreatorMapyPoprawkiTesty

    Private Shared Sub Sta(akcja As Action)
        Dim wyjatek As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    akcja()
                                Catch ex As Exception
                                    wyjatek = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If wyjatek IsNot Nothing Then Throw New Exception(wyjatek.Message, wyjatek)
    End Sub

    Private Shared Function Kt(Of T As Control)(f As Form, nazwa As String) As T
        Return DirectCast(f.Controls.Find(nazwa, True)(0), T)
    End Function

    Private Shared Function Pole(Of T)(o As Object, nazwa As String) As T
        Return DirectCast(o.GetType().GetField(nazwa, BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(o), T)
    End Function

    ''' <summary>Przechodzi do kroku 4 z wybranym celem i szablonem.</summary>
    Private Shared Sub DoKroku4(k As Form, rbCel As String, idSzablonu As String)
        Kt(Of RadioButton)(k, rbCel).Checked = True
        Dim dalej = Kt(Of Button)(k, "btnDalej")
        dalej.PerformClick() : Application.DoEvents()
        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
        For i = 0 To lst.Items.Count - 1
            If DirectCast(lst.Items(i), SzablonMapy).Id = idSzablonu Then lst.SelectedIndex = i
        Next
        dalej.PerformClick() : Application.DoEvents()
        dalej.PerformClick() : Application.DoEvents()
    End Sub

    <Fact>
    Public Sub Telefon_WariantPojedynczy_DajeSQLiteDB()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelTelefon", "piesza_gory")
                        Kt(Of RadioButton)(k, "rbWariantPojedynczy").Checked = True
                        Application.DoEvents()
                        Assert.Contains("SQLiteDB", Kt(Of Label)(k, "lblFormatInfo").Text)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Wydruk_WariantWielopoziomowy_DajeMBTiles()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelWydruk", "piesza_gory")
                        Kt(Of RadioButton)(k, "rbWariantWielopoziomowy").Checked = True
                        Application.DoEvents()
                        Assert.Contains("MBTiles", Kt(Of Label)(k, "lblFormatInfo").Text)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Kataster_PikselZSzablonu()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelCadGis", "dzialka_kataster")
                        Assert.Equal(0.25D, Kt(Of NumericUpDown)(k, "numRozmiarPiksela").Value)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub WsteczZKroku3_ZachowujeWybranySzablon()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Dim dalej = Kt(Of Button)(k, "btnDalej")
                        dalej.PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        lst.SelectedIndex = 2
                        dalej.PerformClick() : Application.DoEvents()
                        Kt(Of Button)(k, "btnWstecz").PerformClick() : Application.DoEvents()
                        Assert.Equal(2, lst.SelectedIndex)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Garmin_WylaczonaKorekta_NieWracaPoZmianiePiksela()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelGarmin", "piesza_gory")
                        Dim chk = Kt(Of CheckBox)(k, "chkKorektaObrazu")
                        chk.Checked = False
                        Kt(Of NumericUpDown)(k, "numRozmiarPiksela").Value = 2D
                        Application.DoEvents()
                        Assert.False(chk.Checked)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Miejscowosc_BezWyniku_NieZostawiaStaregoZasiegu()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Dim dalej = Kt(Of Button)(k, "btnDalej")
                        dalej.PerformClick() : Application.DoEvents()
                        dalej.PerformClick() : Application.DoEvents()
                        Dim zasiegMapy = Pole(Of Zasieg)(k, "_wybranyZasieg")
                        Assert.True(zasiegMapy.Poprawny, "warunek wstępny: zasięg z mapy")
                        Kt(Of RadioButton)(k, "rbObszarMiejscowosc").Checked = True
                        Application.DoEvents()
                        Dim teraz = Pole(Of Zasieg)(k, "_wybranyZasieg")
                        Assert.False(teraz.Poprawny, "Przy wybranej 'miejscowości' bez wyniku kreator dalej używa zasięgu z mapy: " &
                                     Kt(Of Label)(k, "lblObszarWymiary").Text)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub DuzyObszarPikselMinimalny_SzacunekSzybki()
        Dim z As New Zasieg(450000, 600000, 550000, 700000) ' 100 x 100 km
        Dim sw = Diagnostics.Stopwatch.StartNew()
        Dim s = ZarzadcaSzablonow.SzacujParametry(z, 0.1, 1024)
        sw.Stop()
        Assert.True(s.LiczbaSegmentow > 900000)
        Assert.True(sw.ElapsedMilliseconds < 2000, "czas " & sw.ElapsedMilliseconds)
        ' Szacunek czasu przy 4 seg/s
        Assert.True(s.SzacowanyCzasSekundy > 0)
    End Sub

    <Fact>
    Public Sub FormatyRozmiaruSpojne()
        Dim s As New SzacunekKreatora With {.SzacowanyRozmiarMb = 1536, .SzacowanyRozmiarMbMin = 1024, .SzacowanyRozmiarMbMax = 2048}
        Dim a = s.SformatowanyRozmiar()
        Dim b = s.SformatowanyPrzedzialRozmiaru()
        Assert.Equal(a.Contains(","), b.Contains(","))
    End Sub

    <Fact>
    Public Sub Garmin_WariantKmz_PonadSto_ZostajeKmzZOstrzezeniem()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelGarmin", "piesza_gory")
                        Kt(Of RadioButton)(k, "rbWariantPojedynczy").Checked = True
                        Kt(Of NumericUpDown)(k, "numRozmiarPiksela").Value = 0.1D
                        Application.DoEvents()
                        Assert.Contains("KMZ", Kt(Of Label)(k, "lblFormatInfo").Text)
                        Dim kom = Kt(Of Label)(k, "lblSzacunekKomunikat").Text
                        Assert.True(kom.Contains("Garmin") OrElse kom.Contains("JNX"), kom)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub WlasnyZestaw_PustyPokazujeBrakWarstw()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    f.WybranyZestaw().Wyczysc()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "wlasny_zestaw" Then lst.SelectedIndex = i
                        Next
                        Assert.Contains("nie wybrano", Kt(Of Label)(k, "lblSkladnikiSzablonu").Text)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub OpcjeZaawansowane_UkladBezNakladania()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelTelefon", "piesza_gory")
                        Assert.True(Kt(Of NumericUpDown)(k, "numRozmiarPiksela").Visible)
                        Assert.True(Kt(Of GroupBox)(k, "grpSzczegolowosc").Bottom <= Kt(Of LinkLabel)(k, "lnkZaawansowane").Top)
                        Assert.True(Kt(Of LinkLabel)(k, "lnkZaawansowane").Bottom <= Kt(Of GroupBox)(k, "grpZaawansowane").Top + 4)
                        Assert.True(Kt(Of GroupBox)(k, "grpZaawansowane").Bottom <= Kt(Of Panel)(k, "pnlKrok4").Height)
                        Assert.True(Kt(Of GroupBox)(k, "grpZapis").Bottom <= Kt(Of GroupBox)(k, "grpSzczegolowosc").Top)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub UnikalnaSciezka_DopisujeNumer()
        Dim folder = IO.Path.Combine(IO.Path.GetTempPath(), "kreator_unikalna_" & Guid.NewGuid().ToString("N"))
        IO.Directory.CreateDirectory(folder)
        Try
            Dim p = IO.Path.Combine(folder, "Mapa.mbtiles")
            Assert.Equal(p, Global.MapoTero.FormKreatorMapy.UnikalnaSciezka(p))
            IO.File.WriteAllText(p, "x")
            Assert.Equal(IO.Path.Combine(folder, "Mapa_2.mbtiles"), Global.MapoTero.FormKreatorMapy.UnikalnaSciezka(p))
        Finally
            IO.Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub Korekta_DomyslnieWylaczona_TakzeDlaGarminaIWig()
        Sta(Sub()
                For Each para In {Tuple.Create("rbCelGarmin", "piesza_gory"), Tuple.Create("rbCelTelefon", "historia_wig")}
                    Using f As New Global.MapoTero.Form1()
                        Using k As New Global.MapoTero.FormKreatorMapy(f)
                            k.Show() : Application.DoEvents()
                            DoKroku4(k, para.Item1, para.Item2)
                            Assert.False(Kt(Of CheckBox)(k, "chkKorektaObrazu").Checked)
                            Assert.False(Pole(Of OpcjeKorekcjiMip)(k, "_opcjeKorekty").Wlaczona)
                            Assert.True(Kt(Of Label)(k, "lblProfilKorekty").Text.Contains("sugerowany"), para.Item2 & ": " & Kt(Of Label)(k, "lblProfilKorekty").Text)
                        End Using
                    End Using
                Next
            End Sub)
    End Sub

    <Fact>
    Public Sub Korekta_ZaznaczeniePolaWlaczaProfilSugerowany()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        DoKroku4(k, "rbCelGarmin", "piesza_gory")
                        Kt(Of CheckBox)(k, "chkKorektaObrazu").Checked = True
                        Dim o = Pole(Of OpcjeKorekcjiMip)(k, "_opcjeKorekty")
                        Assert.True(o.Wlaczona)
                        Assert.Equal(ProfilKorektyRastra.GarminMip, o.Profil)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub KolejnoscKrokow_ObszarPrzedStylem()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Assert.True(Kt(Of Panel)(k, "pnlKrok3").Visible, "krok 2 to wybór obszaru")
                        Assert.Contains("obszar", Kt(Of Label)(k, "lblTytulKroku").Text)
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Assert.True(Kt(Of Panel)(k, "pnlKrok2").Visible, "krok 3 to wybór stylu")
                        ' podgląd stylu pokazuje środek obszaru użytkownika, nie przykładowe miejsce stylu
                        Dim zasieg = Pole(Of Zasieg)(k, "_wybranyZasieg")
                        Dim mi = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("PobierzParametryProbkiStylu", BindingFlags.NonPublic Or BindingFlags.Instance)
                        Dim punkt = DirectCast(mi.Invoke(k, Nothing), Tuple(Of PunktXY, Double)).Item1
                        Assert.Equal(zasieg.Srodek.X, punkt.X, 3)
                        Assert.Equal(zasieg.Srodek.Y, punkt.Y, 3)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Dziedzictwo_PolaGrupWylaczajaZabytki()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "dziedzictwo" Then lst.SelectedIndex = i
                        Next
                        Application.DoEvents()
                        Dim pole = Kt(Of CheckBox)(k, "chkGrupa1")
                        Assert.Equal("Zabytki archeologiczne", pole.Text)
                        pole.Checked = False
                        Dim mi = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("PobierzZestawStylu", BindingFlags.NonPublic Or BindingFlags.Instance)
                        Dim zestaw = DirectCast(mi.Invoke(k, New Object() {1.0}), ZestawWarstw)
                        Assert.DoesNotContain(zestaw.Warstwy, Function(w) w.Zbior.Contains("archeo"))
                        Assert.Contains(zestaw.Warstwy, Function(w) w.Zbior = "zabytki_zasiegi_NID")
                        ' wiersz stanu podglądu pod polami grup
                        Assert.True(Kt(Of Label)(k, "lblStanPodgladuStyl").Top >= pole.Bottom)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub MapyWig_OknoWarstwPokazujeSkale25000()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "historia_wig" Then lst.SelectedIndex = i
                        Next
                        Application.DoEvents()
                        ' podgląd automatyczny (6 m) to skala 1:100 000, a okno warstw ma pokazać najdokładniejszą 1:25 000
                        Dim mi = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("StopienOknaWarstw", BindingFlags.NonPublic Or BindingFlags.Instance)
                        Dim st = DirectCast(mi.Invoke(k, Nothing), StopienSkali)
                        Assert.NotNull(st)
                        Assert.Equal("1:25 000", st.Opis)
                        Assert.Contains(st.Warstwy, Function(w) w.Warstwa = "wig25k_2180")
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub ZestawWlasny_KrycieZOknaZaawansowanegoNadrzedne()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "dziedzictwo" Then lst.SelectedIndex = i
                        Next
                        ' jak po oknie zaawansowanym: zabytki z różnym kryciem
                        Dim wlasny = DirectCast(GetType(Global.MapoTero.FormKreatorMapy).GetMethod("PobierzZestawStylu", BindingFlags.NonPublic Or BindingFlags.Instance).Invoke(k, New Object() {1.0}), ZestawWarstw)
                        wlasny.UstawKrycie(3, 40)
                        GetType(Global.MapoTero.FormKreatorMapy).GetField("_zestawNiestandardowy", BindingFlags.NonPublic Or BindingFlags.Instance).SetValue(k, wlasny)
                        Dim pokaz = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("PokazSzczegolySzablonu", BindingFlags.NonPublic Or BindingFlags.Instance)
                        pokaz.Invoke(k, New Object() {DirectCast(lst.SelectedItem, SzablonMapy), False})
                        Dim mi = GetType(Global.MapoTero.FormKreatorMapy).GetMethod("PobierzZestawStylu", BindingFlags.NonPublic Or BindingFlags.Instance)
                        Dim zestaw = DirectCast(mi.Invoke(k, New Object() {1.0}), ZestawWarstw)
                        Assert.Equal(100, zestaw(2).Krycie)
                        Assert.Equal(40, zestaw(3).Krycie)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub PomiarPostepu_SzacujePozostalyCzas()
        Dim pomiar As New Global.MapoTero.PomiarPostepu(1000)
        Assert.Contains("szacowanie", pomiar.Opis(0))
        Assert.Equal(" · pozostało ok. 10 s", Global.MapoTero.PomiarPostepu.OpisZProcentu(50, 10))
        Assert.Equal("...", Global.MapoTero.PomiarPostepu.OpisZProcentu(1, 10))
        Assert.Equal("2 min 5 s", Global.MapoTero.PomiarPostepu.Czas(124.2))
    End Sub

End Class
