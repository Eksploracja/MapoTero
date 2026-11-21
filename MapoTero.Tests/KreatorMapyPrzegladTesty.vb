'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'Przegląd kreatora mapy: wszystkie cele, style i warianty formatu w kroku 4 oraz kompletność zestawów warstw.
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

<Collection("OknaKreatora")>
Public Class KreatorMapyPrzegladTesty

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

    Private Shared Function Metoda(nazwa As String) As MethodInfo
        Return GetType(Global.MapoTero.FormKreatorMapy).GetMethod(nazwa, BindingFlags.NonPublic Or BindingFlags.Instance)
    End Function

    <Fact>
    Public Sub WszystkieCeleIStyle_Krok4GotowyDoGenerowania()
        Dim bledy As New List(Of String)()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    For Each rbCel In {"rbCelTelefon", "rbCelGarmin", "rbCelWydruk", "rbCelCadGis"}
                        Using k As New Global.MapoTero.FormKreatorMapy(f)
                            k.Show() : Application.DoEvents()
                            Kt(Of RadioButton)(k, rbCel).Checked = True
                            Dim dalej = Kt(Of Button)(k, "btnDalej")
                            dalej.PerformClick() : Application.DoEvents()
                            dalej.PerformClick() : Application.DoEvents()
                            Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                            Dim ids = lst.Items.Cast(Of SzablonMapy)().Select(Function(s) s.Id).Where(Function(id) id <> "wlasny_zestaw").ToList()
                            For Each id In ids
                                For i = 0 To lst.Items.Count - 1
                                    If DirectCast(lst.Items(i), SzablonMapy).Id = id Then lst.SelectedIndex = i
                                Next
                                Application.DoEvents()
                                dalej.PerformClick() : Application.DoEvents()
                                For Each wariant In {"rbWariantPojedynczy", "rbWariantWielopoziomowy"}
                                    Kt(Of RadioButton)(k, wariant).Checked = True
                                    Application.DoEvents()
                                    Dim opis = $"{rbCel}/{id}/{wariant}"
                                    If Not Kt(Of Button)(k, "btnGeneruj").Enabled Then bledy.Add(opis & ": przycisk Generuj wyłączony - " & Kt(Of Label)(k, "lblSzacunekKomunikat").Text)
                                    If String.IsNullOrWhiteSpace(Kt(Of TextBox)(k, "txtSciezkaDocelowa").Text) Then bledy.Add(opis & ": pusta ścieżka")
                                    Dim format = DirectCast(GetType(Global.MapoTero.FormKreatorMapy).GetProperty("FormatDocelowy", BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(k), RodzajEksportu)
                                    Dim rozszerzenie = ZarzadcaSzablonow.RozszerzenieDlaRodzaju(format)
                                    If Not Kt(Of TextBox)(k, "txtSciezkaDocelowa").Text.EndsWith(rozszerzenie, StringComparison.OrdinalIgnoreCase) Then bledy.Add(opis & ": rozszerzenie ścieżki nie pasuje do " & format.ToString())
                                    If Kt(Of NumericUpDown)(k, "numZoomMin").Value > Kt(Of NumericUpDown)(k, "numZoomMax").Value Then bledy.Add(opis & ": zoom od > do")
                                Next
                                Kt(Of Button)(k, "btnWstecz").PerformClick() : Application.DoEvents()
                            Next
                        End Using
                    Next
                End Using
            End Sub)
        Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
    End Sub

    ''' <summary>Każda warstwa każdej skali stylu ma swój zbiór map - inaczej warstwa znika z mapy bez komunikatu.</summary>
    <Fact>
    Public Sub WszystkieStyle_ZestawyMajaWszystkieWarstwyKazdejSkali()
        Dim bledy As New List(Of String)()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        Dim zrodla = DirectCast(Metoda("PobierzZrodlaKreatora").Invoke(k, Nothing), Dictionary(Of String, PlikWarstw))
                        For Each s In ZarzadcaSzablonow.PobierzDomyslne()
                            Dim zestawyWarstw = New List(Of Tuple(Of String, IList(Of WarstwaSzablonu))) From {Tuple.Create("podstawowe", CType(s.Warstwy, IList(Of WarstwaSzablonu)))}
                            For Each st In s.StopnieSkali
                                zestawyWarstw.Add(Tuple.Create(st.Opis, CType(st.Warstwy, IList(Of WarstwaSzablonu))))
                            Next
                            For Each para In zestawyWarstw
                                Dim zestaw = s.UtworzZestaw(zrodla, para.Item2)
                                If zestaw.Liczba <> para.Item2.Count Then
                                    bledy.Add($"{s.Id} / {para.Item1}: {zestaw.Liczba} z {para.Item2.Count} warstw ({String.Join(", ", para.Item2.Select(Function(w) w.Zbior & ":" & w.Warstwa))})")
                                End If
                            Next
                        Next
                    End Using
                End Using
            End Sub)
        Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
    End Sub

    ''' <summary>Podgląd obszaru w kroku 2 mieści cały wybrany obszar w ramce (dawniej było widać tylko jego środek).</summary>
    <Theory>
    <InlineData(10000.0, 4000.0)>
    <InlineData(3000.0, 25000.0)>
    <InlineData(800.0, 600.0)>
    Public Sub PodgladObszaru_CalyObszarMiesciSieWRamce(szerokosc As Double, wysokosc As Double)
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        Dim pic = Kt(Of PictureBox)(k, "picPodgladObszaru")
                        Dim z As New Zasieg(500000, 600000, 500000 + wysokosc, 600000 + szerokosc)
                        Dim piksel = CDbl(Metoda("PikselPodgladuObszaru").Invoke(k, New Object() {z}))
                        Assert.True(z.Szerokosc / piksel <= pic.Width, $"szerokość {z.Szerokosc / piksel:0} px > ramka {pic.Width} px")
                        Assert.True(z.Wysokosc / piksel <= pic.Height, $"wysokość {z.Wysokosc / piksel:0} px > ramka {pic.Height} px")
                        ' obszar wypełnia znaczną część ramki, a nie jest kropką
                        Assert.True(Math.Max(z.Szerokosc / piksel / pic.Width, z.Wysokosc / piksel / pic.Height) > 0.5 OrElse
                                    piksel <= DirectCast(k.GetType().GetField("_wybranySzablon", BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(k), SzablonMapy).DomyslnyPiksel + 0.0001)
                    End Using
                End Using
            End Sub)
    End Sub

    ''' <summary>Współrzędne wpisane w polu wyszukiwania dają punkt bez zapytania do serwera i obszar o wybranym promieniu.</summary>
    <Fact>
    Public Sub Wyszukiwanie_WspolrzedneDajaObszarWokolPunktu()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Kt(Of ComboBox)(k, "cboPromien").SelectedItem = "2 km"
                        Kt(Of TextBox)(k, "txtSzukajMiejsca").Text = "50.0614, 19.9366"
                        Kt(Of Button)(k, "btnSzukajMiejsca").PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstWynikiMiejscowosci")
                        Assert.Equal(1, lst.Items.Count)
                        Dim z = DirectCast(k.GetType().GetField("_wybranyZasieg", BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(k), Zasieg)
                        Assert.True(z.Poprawny)
                        Assert.Equal(4000, z.Szerokosc, 0)
                        Dim srodek = UkladWspolrzednych.PL1992.DoWgs84(z.Srodek)
                        Assert.Equal(50.0614, srodek.Szerokosc, 4)
                        Assert.Equal(19.9366, srodek.Dlugosc, 4)
                        Assert.StartsWith("Punkt", Kt(Of Label)(k, "lblWynikMiejscowosci").Text.Replace("Wybrano: ", ""))
                    End Using
                End Using
            End Sub)
    End Sub

    ''' <summary>
    ''' Obszar zaznaczony prawym przyciskiem w tej sesji - „Dalej” w kroku 1 przechodzi od razu do stylu; obszar z poprzedniej
    ''' sesji (pola XY bez zaznaczenia) albo zmieniony potem w polach XY - krok 2 jak dotąd.
    ''' </summary>
    <Theory>
    <InlineData("brak", 2)>
    <InlineData("zaznaczony", 3)>
    <InlineData("zmieniony_potem", 2)>
    Public Sub ZaznaczenieMysza_PomijaKrokObszaru(stan As String, oczekiwanyKrok As Integer)
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    If stan <> "brak" Then
                        f.GetType().GetField("_zasiegZaznaczonyMysza", BindingFlags.NonPublic Or BindingFlags.Instance).SetValue(f, CType(f.ZasiegPolXY(), Zasieg?))
                    End If
                    If stan = "zmieniony_potem" Then
                        Dim txt = DirectCast(f.Controls.Find("txtXDol", True)(0), TextBox)
                        txt.Text = (Double.Parse(txt.Text, Globalization.CultureInfo.InvariantCulture) - 500).ToString(Globalization.CultureInfo.InvariantCulture)
                    End If
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Assert.Equal(oczekiwanyKrok, k.BiezacyKrok)
                        If oczekiwanyKrok = 3 Then
                            Assert.StartsWith("Obszar zaznaczony na mapie", Kt(Of Label)(k, "lblPodtytulKroku").Text)
                            ' powrót do kroku obszaru nadal działa
                            Kt(Of Button)(k, "btnWstecz").PerformClick() : Application.DoEvents()
                            Assert.Equal(2, k.BiezacyKrok)
                        End If
                    End Using
                End Using
            End Sub)
    End Sub

    ''' <summary>„wg granic” - prostokąt miejscowości z wyszukiwarki; promień - kwadrat; działka - zasięg z pasem 150 m.</summary>
    <Fact>
    Public Sub Wyszukiwanie_GraniceMiejscowosciIDzialka()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Dim cbo = Kt(Of ComboBox)(k, "cboPromien")
                        Dim zakopane As New WynikGeokodowania With {.Nazwa = "Zakopane", .Szerokosc = 49.29, .Dlugosc = 19.96,
                            .MinSzerokosc = 49.25, .MaxSzerokosc = 49.33, .MinDlugosc = 19.88, .MaxDlugosc = 20.05}
                        cbo.SelectedItem = "wg granic"
                        Dim zGranic = k.ZasiegWyniku(zakopane)
                        Assert.Equal(ObszarKreatora.ZasiegZGranic(49.25, 49.33, 19.88, 20.05), zGranic)
                        cbo.SelectedItem = "2 km"
                        Assert.Equal(4000, k.ZasiegWyniku(zakopane).Szerokosc, 0)
                        ' wynik bez prostokąta (wklejone współrzędne) przy „wg granic” - 5 km
                        cbo.SelectedItem = "wg granic"
                        Dim punkt As New WynikGeokodowania With {.Szerokosc = 50.06, .Dlugosc = 19.93}
                        Assert.Equal(10000, k.ZasiegWyniku(punkt).Szerokosc, 0)
                        Dim dz As New Global.MapoTero.WynikDzialki With {.Zasieg1992 = New Zasieg(500000, 600000, 500040, 600030)}
                        Assert.Equal(330, k.ZasiegWyniku(dz).Szerokosc, 6)
                    End Using
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub Trasa_PrzyciskProfiluPoWczytaniu()
        Dim plik = IO.Path.Combine(IO.Path.GetTempPath(), "mt_profil_" & Guid.NewGuid().ToString("N") & ".gpx")
        IO.File.WriteAllText(plik, "<?xml version=""1.0""?><gpx version=""1.1"" xmlns=""http://www.topografix.com/GPX/1/1""><trk><trkseg>" &
                             "<trkpt lat=""50.06"" lon=""19.93""/><trkpt lat=""50.07"" lon=""19.95""/></trkseg></trk></gpx>")
        Try
            Sta(Sub()
                    Using f As New Global.MapoTero.Form1()
                        Using k As New Global.MapoTero.FormKreatorMapy(f)
                            k.Show() : Application.DoEvents()
                            Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                            Dim profil = Kt(Of Button)(k, "btnProfilTrasy")
                            Assert.False(profil.Enabled)
                            k.WczytajPlikTrasy(plik)
                            Assert.True(profil.Enabled)
                        End Using
                    End Using
                End Sub)
        Finally
            IO.File.Delete(plik)
        End Try
    End Sub

    ''' <summary>Telefon, SQLiteDB z turystyką pieszą - mapa wieloskalowa (dawniej tylko MBTiles).</summary>
    <Fact>
    Public Sub TelefonSQLiteDB_Wieloskalowe()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of RadioButton)(k, "rbCelTelefon").Checked = True
                        Dim dalej = Kt(Of Button)(k, "btnDalej")
                        dalej.PerformClick() : Application.DoEvents()
                        dalej.PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "piesza_gory" Then lst.SelectedIndex = i
                        Next
                        dalej.PerformClick() : Application.DoEvents()
                        Kt(Of RadioButton)(k, "rbWariantPojedynczy").Checked = True
                        Application.DoEvents()
                        Assert.Equal(RodzajEksportu.SQLiteDB, k.FormatDocelowy)
                        Assert.True(Kt(Of CheckBox)(k, "chkWieloskalowa").Visible)
                        Assert.Contains("wieloskalowy", Kt(Of Label)(k, "lblFormatInfo").Text)
                    End Using
                End Using
            End Sub)
    End Sub

    ''' <summary>Garmin JNX z mapami WIG to mapa wieloskalowa (dawniej tylko MBTiles).</summary>
    <Fact>
    Public Sub GarminJnx_MapyWig_Wieloskalowe()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of RadioButton)(k, "rbCelGarmin").Checked = True
                        Dim dalej = Kt(Of Button)(k, "btnDalej")
                        dalej.PerformClick() : Application.DoEvents()
                        dalej.PerformClick() : Application.DoEvents()
                        Dim lst = Kt(Of ListBox)(k, "lstSzablony")
                        For i = 0 To lst.Items.Count - 1
                            If DirectCast(lst.Items(i), SzablonMapy).Id = "historia_wig" Then lst.SelectedIndex = i
                        Next
                        dalej.PerformClick() : Application.DoEvents()
                        Kt(Of RadioButton)(k, "rbWariantWielopoziomowy").Checked = True
                        Application.DoEvents()
                        Assert.Equal(RodzajEksportu.Jnx, k.FormatDocelowy)
                        Assert.True(Kt(Of CheckBox)(k, "chkWieloskalowa").Visible)
                        Assert.Contains("wieloskalowy", Kt(Of Label)(k, "lblFormatInfo").Text)
                        Assert.Contains("skal", Kt(Of Label)(k, "lblSzacunekSegmenty").Text)
                        ' KMZ nie ma poziomów - mapa jednoskalowa
                        Kt(Of RadioButton)(k, "rbWariantPojedynczy").Checked = True
                        Application.DoEvents()
                        Assert.False(Kt(Of CheckBox)(k, "chkWieloskalowa").Visible)
                    End Using
                End Using
            End Sub)
    End Sub

    <Theory>
    <InlineData(2000.0, 2900.0, 1.0, "A4")>
    <InlineData(4200.0, 2970.0, 1.0, "A3")>
    <InlineData(8000.0, 5000.0, 1.0, "A1")>
    <InlineData(20000.0, 20000.0, 1.0, "większy niż A0")>
    Public Sub ArkuszWydruku_NajmniejszyFormatA(szer As Double, wys As Double, piksel As Double, oczekiwany As String)
        Dim opis = ZarzadcaSzablonow.OpisArkuszaWydruku(szer, wys, piksel)
        Assert.Contains(oczekiwany, opis)
    End Sub

    <Fact>
    Public Sub OtworzMape_TylkoGdyWindowsMaProgram()
        Assert.True(Global.MapoTero.FormKreatorMapy.MaProgramDoOtwarcia(".txt"))
        Assert.False(Global.MapoTero.FormKreatorMapy.MaProgramDoOtwarcia(".mapotero_nieznane_rozszerzenie"))
    End Sub

    <Fact>
    Public Sub Upuszczenie_TylkoJedenPlikTrasy()
        Dim dane = Function(pliki As String()) As IDataObject
                       Dim d As New DataObject()
                       d.SetData(DataFormats.FileDrop, pliki)
                       Return d
                   End Function
        Assert.Equal("C:\t\trasa.GPX", Global.MapoTero.FormKreatorMapy.PlikTrasyZUpuszczenia(dane({"C:\t\trasa.GPX"})))
        Assert.NotNull(Global.MapoTero.FormKreatorMapy.PlikTrasyZUpuszczenia(dane({"C:\t\a.kmz"})))
        Assert.Null(Global.MapoTero.FormKreatorMapy.PlikTrasyZUpuszczenia(dane({"C:\t\mapa.mbtiles"})))
        Assert.Null(Global.MapoTero.FormKreatorMapy.PlikTrasyZUpuszczenia(dane({"C:\t\a.gpx", "C:\t\b.gpx"})))
        Assert.Null(Global.MapoTero.FormKreatorMapy.PlikTrasyZUpuszczenia(New DataObject("tekst")))
    End Sub

    ''' <summary>Najechanie myszą na podgląd nie zabiera fokusu z pola wyszukiwania miejscowości.</summary>
    <Fact>
    Public Sub PodgladObszaru_NajechanieNieZabieraFokusuZPolaWyszukiwania()
        Sta(Sub()
                Using f As New Global.MapoTero.Form1()
                    Using k As New Global.MapoTero.FormKreatorMapy(f)
                        k.Show() : Application.DoEvents()
                        Kt(Of Button)(k, "btnDalej").PerformClick() : Application.DoEvents()
                        Dim txt = Kt(Of TextBox)(k, "txtSzukajMiejsca")
                        k.ActiveControl = txt
                        Dim pic = Kt(Of PictureBox)(k, "picPodgladObszaru")
                        Metoda("PrzejmijFokusDlaKolka").Invoke(k, New Object() {pic})
                        Assert.Same(txt, k.ActiveControl)
                    End Using
                End Using
            End Sub)
    End Sub

End Class
