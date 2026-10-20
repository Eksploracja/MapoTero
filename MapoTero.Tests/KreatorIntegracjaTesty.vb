'Copyright (C) <2026>  MapoTero
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
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports MapoTero.Core
Imports Xunit

Public Class KreatorIntegracjaTesty

    Public Sub New()
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    End Sub

    <Fact>
    Public Async Function TestujPobieranieGeoportalWmtsLive() As Task
        'Pobranie i autodetekcja rzeczywistej usługi WMTS Geoportalu
        Const url As String = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMTS/StandardResolution"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=15)

        Assert.True(wynik.Sukces, "Nie udało się połączyć z Geoportalem WMTS: " & wynik.KomunikatBledu)
        Assert.Equal(TypUslugiMapowej.Wmts, wynik.Typ)
        Assert.NotNull(wynik.UslugaWmts)
        Assert.True(wynik.UslugaWmts.Warstwy.Count > 0)
        Assert.Contains(wynik.UslugaWmts.Warstwy, Function(w) w.Identyfikator.IndexOf("ORTOFOTOMAPA", StringComparison.OrdinalIgnoreCase) >= 0)
    End Function

    <Fact>
    Public Async Function TestujPobieranieKiegWmsLive() As Task
        'Pobranie i autodetekcja rzeczywistej usługi WMS Krajowej Integracji Ewidencji Gruntów
        Const url As String = "https://integracja.gugik.gov.pl/cgi-bin/KrajowaIntegracjaEwidencjiGruntow"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=15)

        Assert.True(wynik.Sukces, "Nie udało się połączyć z KIEG WMS: " & wynik.KomunikatBledu)
        Assert.Equal(TypUslugiMapowej.Wms, wynik.Typ)
        Assert.NotNull(wynik.UslugaWms)
        Assert.True(wynik.UslugaWms.Warstwy.Count > 0)

        Dim doPobrania = wynik.UslugaWms.WszystkieWarstwyDoPobrania()
        Assert.True(doPobrania.Count >= 5)

        Dim dzialki = wynik.UslugaWms.ZnajdzWarstwe("dzialki")
        Assert.NotNull(dzialki)
        Assert.True(dzialki.ObslugujeEpsg(2180))
        Assert.True(dzialki.ZalecanyRozmiarPiksela() > 0)
    End Function

    <Fact>
    Public Async Function TestujBladStronyWwwZamiastXml() As Task
        'Wklejenie strony HTML zamiast usługi WMS/WMTS
        Const url As String = "https://www.google.com"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=10)

        Assert.False(wynik.Sukces)
        Assert.Contains("stronę WWW", wynik.KomunikatBledu)
    End Function

    <Fact>
    Public Async Function TestujNieistniejacyAdres() As Task
        Const url As String = "https://mapy.geoportal.gov.pl/nieistniejacy_serwis_404_xyz"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=10)

        Assert.False(wynik.Sukces)
        Assert.NotEmpty(wynik.KomunikatBledu)
    End Function

    <Fact>
    Public Sub TestujCyklZapisuIOdczytuNowegoZbioruWms()
        'Symulacja dodania nowego zbioru z kreatora do katalogu i odczyt przez silnik
        Dim tempDir = Path.Combine(Path.GetTempPath(), "MapoTero_Test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir)

        Try
            Dim plikIndeksu = Path.Combine(tempDir, "warstwy.txt")
            Dim nazwaZbioru = "powiat_testowy_wms"
            Dim plikZbioru = Path.Combine(tempDir, nazwaZbioru & ".txt")

            '1. Tworzenie zbioru przez kreator
            Dim p As New PlikWarstw With {
                .Adres = PlikWarstw.ZbudujAdresWmsDlaPliku("https://powiat.pl/wms", "1.3.0")
            }
            p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "granice_gmin", .RozmiarPiksela = "2.0"})
            p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "dzialki", .RozmiarPiksela = "0.5"})
            p.Zapisz(plikZbioru)
            PlikWarstw.DodajDoIndeksu(plikIndeksu, nazwaZbioru)

            '2. Weryfikacja indeksu
            Assert.True(File.Exists(plikIndeksu))
            Dim linieIndeksu = File.ReadAllLines(plikIndeksu, PlikWarstw.Kodowanie())
            Assert.Contains(nazwaZbioru, linieIndeksu)

            '3. Weryfikacja ponownego dodania tego samego zbioru (unikalność w indeksie)
            PlikWarstw.DodajDoIndeksu(plikIndeksu, nazwaZbioru)
            Dim liniePo = File.ReadAllLines(plikIndeksu, PlikWarstw.Kodowanie())
            Assert.Single(liniePo, Function(l) l = nazwaZbioru)

            '4. Wczytanie przez silnik PlikWarstw
            Dim wczytany = PlikWarstw.Wczytaj(plikZbioru)
            Assert.Equal("https://powiat.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=", wczytany.Adres)
            Assert.Equal(2, wczytany.Warstwy.Count)
            Assert.Equal("granice_gmin", wczytany.Warstwy(0).Nazwa)
            Assert.Equal("2.0", wczytany.Warstwy(0).RozmiarPiksela)

            '5. Wygenerowanie zapytania GetMap dla wczytanej warstwy
            Dim zasiegTest As New Zasieg(500000, 600000, 501000, 601000)
            Dim getMapUrl = Wms.ZapytanieGetMap(wczytany.Adres, {wczytany.Warstwy(1).Nazwa},
                                                UkladWspolrzednych.PL1992, zasiegTest, "png", 1024, 1024, False, True)

            Assert.Contains("layers=dzialki", getMapUrl)
            Assert.Contains("CRS=EPSG:2180", getMapUrl)
            Assert.Contains("transparent=TRUE", getMapUrl)
            Assert.Contains("width=1024", getMapUrl)

        Finally
            If Directory.Exists(tempDir) Then
                Directory.Delete(tempDir, recursive:=True)
            End If
        End Try
    End Sub

    <Fact>
    Public Sub TestujBudowanieZapytaniaGetMapZAdresuBezRequest()
        'Test adresu bazowego z GetCapabilities (bez request=GetMap i bez layers=)
        Dim surowyAdres = "https://integracja.gugik.gov.pl/cgi-bin/KrajowaIntegracjaUzbrojeniaTerenu?"
        Dim z = New Zasieg(500000, 500000, 500100, 500100)
        Dim url = Wms.ZapytanieGetMap(surowyAdres, {"przewod_gazowy"}, UkladWspolrzednych.PL1992, z, "image/png", 256, 256, False, True)

        Assert.Contains("SERVICE=WMS", url)
        Assert.Contains("REQUEST=GetMap", url)
        Assert.Contains("VERSION=1.3.0", url)
        Assert.Contains("CRS=EPSG:2180", url)
        Assert.Contains("layers=przewod_gazowy", url)
        Assert.Contains("format=image/png", url)
    End Sub

    <Fact>
    Public Async Function TestujPobieranieKafliBdooWmtsLive() As Task
        'Test pobierania kafelka z usługi BDOO (baza ogólnogeograficzna w skali 1:250 000 z cieniowaniem)
        Const url As String = "https://mapy.geoportal.gov.pl/wss/service/PZGIK/BDOO/WMTS/BDOOzCieniowaniem"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=15)
        Assert.True(wynik.Sukces, "Blad pobierania: " & wynik.KomunikatBledu)
        Dim u = wynik.UslugaWmts
        Dim warstwa = u.Warstwy(0)

        'Dla BDOO odpowiednia rozdzielczość to skala przeglądowa (25 m/pix)
        Dim srodekPL As New Zasieg(500000, 500000, 500256, 500256)
        Dim plan As New PlanWmts(u, warstwa.Identyfikator, UkladWspolrzednych.PL1992, srodekPL, 25.0, "png")
        Dim kol As Double, wie As Double
        plan.PikselMacierzy(plan.DoUkladuKafli(New PunktXY(srodekPL.XGora, srodekPL.YLewy)), kol, wie)
        Dim wierszKafla = CInt(Math.Floor(wie / plan.Macierz.WysokoscKafla))
        Dim kolumnaKafla = CInt(Math.Floor(kol / plan.Macierz.SzerokoscKafla))
        Dim tileUrl = plan.AdresKafla(wierszKafla, kolumnaKafla)

        Using klient As New System.Net.Http.HttpClient()
            klient.DefaultRequestHeaders.UserAgent.ParseAdd("MapoTero/3.14")
            Dim odp = Await klient.GetAsync(tileUrl)
            Dim bajty = Await odp.Content.ReadAsByteArrayAsync()
            Assert.True(odp.IsSuccessStatusCode)
            Assert.True(bajty.Length > 1000, "Kafelek BDOO powinien zawierać dane obrazu (otrzymano " & bajty.Length & " bajtów)")
            'sprawdzenie sygnatury PNG (89 50 4E 47)
            Assert.Equal(CByte(&H89), bajty(0))
            Assert.Equal(CByte(&H50), bajty(1))
            Assert.Equal(CByte(&H4E), bajty(2))
            Assert.Equal(CByte(&H47), bajty(3))
        End Using
    End Function

    <Fact>
    Public Async Function TestujPobieranieKafliSozoWmsLive() As Task
        'Test usługi ArcGIS Server SOZO (wymagającej numerycznego ID zamiast nazwy 'Raster')
        Const url As String = "https://mapy.geoportal.gov.pl/wss/service/img/guest/SOZO/MapServer/WMSServer"
        Dim wynik = Await KlientCapabilities.PobierzAsync(url, ignorujSsl:=True, timeoutSekundy:=15)
        Assert.True(wynik.Sukces, "Blad pobierania: " & wynik.KomunikatBledu)
        Dim u = wynik.UslugaWms
        Dim warstwy = u.WszystkieWarstwyDoPobrania()
        Assert.True(warstwy.Count > 0)
        Dim raster = warstwy(0)
        Assert.Equal("1", raster.IdentyfikatorNumeryczny)

        'Pobranie próbki z użyciem wykrytego numerycznego ID
        Dim z = New Zasieg(500000, 500000, 501024, 501024)
        Dim adresWms = PlikWarstw.ZbudujAdresWmsDlaPliku(u.AdresGetMap, u.Wersja)
        Dim getMapUrl = Wms.ZapytanieGetMap(adresWms, {raster.IdentyfikatorNumeryczny}, UkladWspolrzednych.PL1992, z, "image/png", 256, 256, False, True)

        Using klient As New System.Net.Http.HttpClient()
            klient.DefaultRequestHeaders.UserAgent.ParseAdd("MapoTero/3.14")
            Dim odp = Await klient.GetAsync(getMapUrl)
            Dim bajty = Await odp.Content.ReadAsByteArrayAsync()
            Assert.True(odp.IsSuccessStatusCode)
            Assert.True(bajty.Length > 1000)
            Assert.Equal(CByte(&H89), bajty(0))
            Assert.Equal(CByte(&H50), bajty(1))
            Assert.Equal(CByte(&H4E), bajty(2))
            Assert.Equal(CByte(&H47), bajty(3))
        End Using
    End Function

End Class
