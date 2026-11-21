Imports System.Drawing
Imports MapoTero.Core
Imports Xunit

Public Class PodgladWarstwSilnikTesty

    <Fact>
    Public Sub Inicjalizacja_BuforJestPusty()
        Using silnik As New PodgladWarstwSilnik()
            Assert.Equal(0, silnik.LiczbaWarstwWBuforze)
            Assert.False(silnik.PunktProbki.HasValue)
        End Using
    End Sub

    <Fact>
    Public Sub SprawdzIZaktualizujParametry_WykrywaZmianePunktuISkali()
        Using silnik As New PodgladWarstwSilnik()
            Dim uklad = UkladWspolrzednych.PL1992
            Dim punkt1 As New PunktXY(500000, 600000)

            ' Pierwsze wywołanie - parametry zostały zainicjalizowane (zmiana)
            Dim zmieniono1 = silnik.SprawdzIZaktualizujParametry(punkt1, 1.0, uklad)
            Assert.True(zmieniono1)
            Assert.True(silnik.PunktProbki.HasValue)
            Assert.Equal(punkt1.X, silnik.PunktProbki.Value.X)

            ' Identyczne wywołanie - brak zmian
            Dim zmieniono2 = silnik.SprawdzIZaktualizujParametry(punkt1, 1.0, uklad)
            Assert.False(zmieniono2)

            ' Zmiana rozdzielczości piksela - wykrywa zmianę
            Dim zmienionoPiksel = silnik.SprawdzIZaktualizujParametry(punkt1, 2.0, uklad)
            Assert.True(zmienionoPiksel)

            ' Zmiana punktu próbki o więcej niż 2 piksele - wykrywa zmianę
            Dim punkt2 As New PunktXY(500100, 600100)
            Dim zmienionoPunkt = silnik.SprawdzIZaktualizujParametry(punkt2, 2.0, uklad)
            Assert.True(zmienionoPunkt)

            ' Zmiana układu współrzędnych - wykrywa zmianę
            Dim zmienionoUklad = silnik.SprawdzIZaktualizujParametry(punkt2, 2.0, UkladWspolrzednych.Wgs84)
            Assert.True(zmienionoUklad)
        End Using
    End Sub

    <Fact>
    Public Sub ZlozObraz_PustyZestaw_ZwracaNothing()
        Using silnik As New PodgladWarstwSilnik()
            Dim zestaw As New ZestawWarstw()
            Dim wynik = silnik.ZlozObraz(zestaw)
            Assert.Null(wynik)
        End Using
    End Sub

    <Fact>
    Public Sub ZlozObraz_ZwracaBitmapeOWymiarachProbki()
        Using silnik As New PodgladWarstwSilnik()
            Dim zestaw As New ZestawWarstw()
            zestaw.Dodaj("warstwa1", adres:="http://serwer.test/wms", zbior:="zbior1")

            Dim bmp = silnik.ZlozObraz(zestaw)
            Assert.NotNull(bmp)
            Assert.Equal(PodgladWarstwSilnik.BokProbki, bmp.Width)
            Assert.Equal(PodgladWarstwSilnik.BokProbki, bmp.Height)
            bmp.Dispose()
        End Using
    End Sub

    <Fact>
    Public Sub WyczyscBufor_ZerujeParametryIStan()
        Using silnik As New PodgladWarstwSilnik()
            silnik.SprawdzIZaktualizujParametry(New PunktXY(100, 200), 1.0, UkladWspolrzednych.PL1992)
            Assert.True(silnik.PunktProbki.HasValue)

            silnik.WyczyscBufor()
            Assert.False(silnik.PunktProbki.HasValue)
            Assert.Equal(0, silnik.LiczbaWarstwWBuforze)
        End Using
    End Sub

    <Fact>
    Public Sub ZastosujZoom_SkalujePrawidlowoIWymiarachDocelowych()
        Using bmp As New Bitmap(100, 100)
            Using z1 = PodgladWarstwSilnik.ZastosujZoom(bmp, 1.0, 100, 100)
                Assert.NotNull(z1)
                Assert.Equal(100, z1.Width)
                Assert.Equal(100, z1.Height)
            End Using

            Using z2 = PodgladWarstwSilnik.ZastosujZoom(bmp, 2.0, 116, 116)
                Assert.NotNull(z2)
                Assert.Equal(116, z2.Width)
                Assert.Equal(116, z2.Height)
            End Using

            Using z05 = PodgladWarstwSilnik.ZastosujZoom(bmp, 0.5, 116, 116)
                Assert.NotNull(z05)
                Assert.Equal(116, z05.Width)
                Assert.Equal(116, z05.Height)
            End Using

            ' Dla Nothing zwraca Nothing
            Dim zNothing = PodgladWarstwSilnik.ZastosujZoom(Nothing, 2.0, 116, 116)
            Assert.Null(zNothing)
        End Using
    End Sub

    <Fact>
    Public Sub ZasiegProbki_WiekszyOdPaneluWokolPunktu()
        Using silnik As New PodgladWarstwSilnik()
            Assert.False(silnik.ZasiegProbki().HasValue)
            silnik.SprawdzIZaktualizujParametry(New PunktXY(500000, 600000), 2.0, UkladWspolrzednych.PL1992)
            Dim z = silnik.ZasiegProbki().Value
            Assert.Equal(PodgladWarstwSilnik.BokProbki * 2.0, z.XGora - z.XDol, 6)
            Assert.Equal(500000, (z.XDol + z.XGora) / 2, 6)
            Assert.Equal(600000, (z.YLewy + z.YPrawy) / 2, 6)
            'oddalenie panelu x0,5 pokazuje 2 x BokPanelu pikseli próbki - muszą się w niej mieścić
            Assert.True(PodgladWarstwSilnik.BokProbki >= 2 * PodgladWarstwSilnik.BokPanelu)
        End Using
    End Sub

    <Fact>
    Public Sub BrakujaceWarstwy_PustyBuforZwracaWszystkie()
        Using silnik As New PodgladWarstwSilnik()
            Dim zestaw As New ZestawWarstw()
            zestaw.Dodaj("warstwa1", adres:="http://serwer.test/wms", zbior:="zbior1")
            zestaw.Dodaj("warstwa2", adres:="http://serwer.test/wms", zbior:="zbior1")
            Assert.Equal(2, silnik.BrakujaceWarstwy(zestaw).Count)
        End Using
    End Sub

    <Fact>
    Public Sub PobierzPodgladAsync_BezParametrowProbkiNicNiePobiera()
        Using silnik As New PodgladWarstwSilnik()
            Dim zestaw As New ZestawWarstw()
            zestaw.Dodaj("warstwa1", adres:="http://serwer.test/wms", zbior:="zbior1")
            Dim wynik = silnik.PobierzPodgladAsync(New ZadaniePobierania(), zestaw, Threading.CancellationToken.None).Result
            Assert.Null(wynik.Item1)
            Assert.Empty(wynik.Item2)
        End Using
    End Sub

    <Fact>
    Public Sub ZastosujZoom_ProbkaWiekszaOdPanelu_OddaleniePokazujeWiekszyObszar()
        'próbka 300 px: biała, z czerwonym pasem przy lewej krawędzi (x < 80) - poza środkowymi 116 px
        Using bmp As New Bitmap(PodgladWarstwSilnik.BokProbki, PodgladWarstwSilnik.BokProbki)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                g.FillRectangle(Brushes.Red, 0, 0, 80, bmp.Height)
            End Using
            Dim bok = PodgladWarstwSilnik.BokPanelu
            Using z1 = PodgladWarstwSilnik.ZastosujZoom(bmp, 1.0, bok, bok)
                'skala 1:1 - środek próbki, bez pasa
                Assert.Equal(255, CInt(z1.GetPixel(0, bok \ 2).G))
            End Using
            Using z05 = PodgladWarstwSilnik.ZastosujZoom(bmp, 0.5, bok, bok)
                'oddalenie - 232 px próbki, lewa krawędź obejmuje pas
                Dim px = z05.GetPixel(2, bok \ 2)
                Assert.True(px.R > 200 AndAlso px.G < 80, px.ToString())
                Assert.Equal(255, CInt(z05.GetPixel(bok - 3, bok \ 2).G))
            End Using
        End Using
    End Sub


    <Fact>
    Public Sub ZastosujZoom_ZDopasowaniemWypelnienia_WypelniaCalyDocelowyObszar()
        ' Testujemy, że przy dopasujWypelnienie:=True pomniejszenie (0.3x) małej próbki (100x100)
        ' do dużego okna (374x196) wypełnia cały obszar bez marginesów szachownicy
        Using bmp As New Bitmap(100, 100)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.CornflowerBlue)
            End Using

            Using wynik = PodgladWarstwSilnik.ZastosujZoom(bmp, 0.3, 374, 196, dopasujWypelnienie:=True)
                Assert.NotNull(wynik)
                Assert.Equal(374, wynik.Width)
                Assert.Equal(196, wynik.Height)

                ' Wszystkie rogi i krawędzie muszą być wypełnione kolorem próbki (brak pustej szachownicy)
                Dim pNaro = wynik.GetPixel(0, 0)
                Assert.True(pNaro.B > 200, "Lewy górny róg powinien być wypełniony treścią mapy")

                Dim pPrawy = wynik.GetPixel(373, 195)
                Assert.True(pPrawy.B > 200, "Prawy dolny róg powinien być wypełniony treścią mapy")
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub PodgladWarstwSilnik_BokWlasciwosc_DostosowujeZasiegIProbke()
        Using silnik As New PodgladWarstwSilnik() With {.Bok = 512}
            Assert.Equal(512, silnik.Bok)
            silnik.SprawdzIZaktualizujParametry(New PunktXY(500000, 600000), 1.0, UkladWspolrzednych.PL1992)
            Dim z = silnik.ZasiegProbki().Value
            Assert.Equal(512.0, z.Szerokosc, 3)
            Assert.Equal(512.0, z.Wysokosc, 3)
        End Using
    End Sub

    <Fact>
    Public Sub ZastosujZoom_ZPrzesunieciem_PrzesuwaTrescWMiejscuDocelowym()
        Using zrodlo As New Bitmap(100, 100, Imaging.PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(zrodlo)
                g.Clear(Color.Blue)
            End Using
            zrodlo.SetPixel(50, 50, Color.FromArgb(255, 255, 0, 0)) ' czerwony punkt w centrum

            Using wynik = PodgladWarstwSilnik.ZastosujZoom(zrodlo, 1.0, 100, 100, dopasujWypelnienie:=False, przesuniecieX:=10, przesuniecieY:=20)
                Assert.NotNull(wynik)
                ' Punkt z (50,50) powinien przesunąć się o (+10, +20) na (60, 70)
                Dim px = wynik.GetPixel(60, 70)
                Assert.Equal(255, px.R)
                Assert.Equal(0, px.G)
                Assert.Equal(0, px.B)
            End Using
        End Using
    End Sub

End Class
