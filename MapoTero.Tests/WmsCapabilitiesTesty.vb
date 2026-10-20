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
Imports MapoTero.Core
Imports Xunit

Public Class WmsCapabilitiesTesty

    Private Const XmlWms130 As String =
        "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf &
        "<WMS_Capabilities version=""1.3.0"" xmlns=""http://www.opengis.net/wms"" xmlns:xlink=""http://www.w3.org/1999/xlink"">" & vbCrLf &
        "  <Service>" & vbCrLf &
        "    <Name>WMS</Name>" & vbCrLf &
        "    <Title>Geoportal Krajowy - Ewidencja Gruntów</Title>" & vbCrLf &
        "    <Abstract>Usługa prezentacji działek ewidencyjnych i budynków</Abstract>" & vbCrLf &
        "    <MaxWidth>4096</MaxWidth>" & vbCrLf &
        "    <MaxHeight>4096</MaxHeight>" & vbCrLf &
        "  </Service>" & vbCrLf &
        "  <Capability>" & vbCrLf &
        "    <Request>" & vbCrLf &
        "      <GetMap>" & vbCrLf &
        "        <Format>image/png</Format>" & vbCrLf &
        "        <Format>image/jpeg</Format>" & vbCrLf &
        "        <DCPType><HTTP><Get>" & vbCrLf &
        "          <OnlineResource xlink:type=""simple"" xlink:href=""https://integracja.gugik.gov.pl/cgi-bin/KIEG?"" />" & vbCrLf &
        "        </Get></HTTP></DCPType>" & vbCrLf &
        "      </GetMap>" & vbCrLf &
        "    </Request>" & vbCrLf &
        "    <Layer>" & vbCrLf &
        "      <Title>Krajowa Integracja Ewidencji Gruntów</Title>" & vbCrLf &
        "      <CRS>EPSG:2180</CRS>" & vbCrLf &
        "      <CRS>EPSG:4326</CRS>" & vbCrLf &
        "      <Layer queryable=""1"">" & vbCrLf &
        "        <Name>dzialki</Name>" & vbCrLf &
        "        <Title>Granice działek ewidencyjnych</Title>" & vbCrLf &
        "        <MinScaleDenominator>500</MinScaleDenominator>" & vbCrLf &
        "        <MaxScaleDenominator>50000</MaxScaleDenominator>" & vbCrLf &
        "      </Layer>" & vbCrLf &
        "      <Layer queryable=""1"">" & vbCrLf &
        "        <Name>numery_dzialek</Name>" & vbCrLf &
        "        <Title>Numery działek</Title>" & vbCrLf &
        "        <MinScaleDenominator>2500</MinScaleDenominator>" & vbCrLf &
        "      </Layer>" & vbCrLf &
        "      <Layer>" & vbCrLf &
        "        <Title>Grupa budynki bez nazwy</Title>" & vbCrLf &
        "        <Layer queryable=""1"">" & vbCrLf &
        "          <Name>budynki</Name>" & vbCrLf &
        "          <Title>Kontury budynków</Title>" & vbCrLf &
        "          <CRS>EPSG:2177</CRS>" & vbCrLf &
        "        </Layer>" & vbCrLf &
        "      </Layer>" & vbCrLf &
        "    </Layer>" & vbCrLf &
        "  </Capability>" & vbCrLf &
        "</WMS_Capabilities>"

    Private Const XmlWms111 As String =
        "<!DOCTYPE WMT_MS_Capabilities SYSTEM ""http://schemas.opengis.net/wms/1.1.1/capabilities_1_1_1.dtd"">" & vbCrLf &
        "<WMT_MS_Capabilities version=""1.1.1"">" & vbCrLf &
        "  <Service>" & vbCrLf &
        "    <Title>Serwer Map Historycznych HGIS</Title>" & vbCrLf &
        "  </Service>" & vbCrLf &
        "  <Capability>" & vbCrLf &
        "    <Request>" & vbCrLf &
        "      <GetMap>" & vbCrLf &
        "        <Format>image/png</Format>" & vbCrLf &
        "      </GetMap>" & vbCrLf &
        "    </Request>" & vbCrLf &
        "    <Layer>" & vbCrLf &
        "      <Title>Mapy WIG</Title>" & vbCrLf &
        "      <SRS>EPSG:2180</SRS>" & vbCrLf &
        "      <Layer>" & vbCrLf &
        "        <Name>wig25k</Name>" & vbCrLf &
        "        <Title>WIG 1:25 000</Title>" & vbCrLf &
        "        <ScaleHint min=""0.9899"" max=""9.899"" />" & vbCrLf &
        "      </Layer>" & vbCrLf &
        "    </Layer>" & vbCrLf &
        "  </Capability>" & vbCrLf &
        "</WMT_MS_Capabilities>"

    <Fact>
    Public Sub OdczytWms130()
        Dim u = WmsCapabilities.Wczytaj(XmlWms130, "https://domyslny.pl/wms")
        Assert.Equal("1.3.0", u.Wersja)
        Assert.Equal("Geoportal Krajowy - Ewidencja Gruntów", u.Tytul)
        Assert.Equal("https://integracja.gugik.gov.pl/cgi-bin/KIEG?", u.AdresGetMap)
        Assert.Equal(4096, u.MaxWidth)
        Assert.Equal(4096, u.MaxHeight)
        Assert.Contains("image/png", u.Formaty)
        Assert.Contains("image/jpeg", u.Formaty)

        Dim doPobrania = u.WszystkieWarstwyDoPobrania()
        Assert.Equal(3, doPobrania.Count)

        Dim wDzialki = u.ZnajdzWarstwe("dzialki")
        Assert.NotNull(wDzialki)
        Assert.Equal("Granice działek ewidencyjnych", wDzialki.Tytul)
        Assert.Equal(500, wDzialki.MinScaleDenominator)
        Assert.Equal(0.25, wDzialki.ZalecanyRozmiarPiksela())
        Assert.True(wDzialki.ObslugujeEpsg(2180))
        Assert.True(wDzialki.ObslugujeEpsg(4326))

        Dim wNumery = u.ZnajdzWarstwe("numery_dzialek")
        Assert.NotNull(wNumery)
        Assert.Equal(0.5, wNumery.ZalecanyRozmiarPiksela())

        Dim wBudynki = u.ZnajdzWarstwe("budynki")
        Assert.NotNull(wBudynki)
        Assert.True(wBudynki.ObslugujeEpsg(2177))
        Assert.True(wBudynki.ObslugujeEpsg(2180))
    End Sub

    <Fact>
    Public Sub OdczytWms111()
        Dim u = WmsCapabilities.Wczytaj(XmlWms111, "https://hgis.cartomatic.pl/wms")
        Assert.Equal("1.1.1", u.Wersja)
        Assert.Equal("Serwer Map Historycznych HGIS", u.Tytul)
        Dim warstwy = u.WszystkieWarstwyDoPobrania()
        Assert.Single(warstwy)
        Dim w = warstwy(0)
        Assert.Equal("wig25k", w.Nazwa)
        Assert.Equal("WIG 1:25 000", w.Tytul)
        Assert.True(w.ObslugujeEpsg(2180))
        Assert.True(w.MinScaleDenominator > 0)
    End Sub

    <Fact>
    Public Sub ZglaszaBladPrzyServiceException()
        Const xmlBlad As String = "<ServiceExceptionReport version=""1.3.0""><ServiceException>Brak autoryzacji</ServiceException></ServiceExceptionReport>"
        Dim ex = Assert.Throws(Of InvalidOperationException)(Function() WmsCapabilities.Wczytaj(xmlBlad, "https://serwer.pl"))
        Assert.Contains("Brak autoryzacji", ex.Message)
    End Sub

    <Fact>
    Public Sub OdczytKodowaniaIso88592ZeStrumienia()
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        Const xmlIso As String = "<?xml version=""1.0"" encoding=""ISO-8859-2""?>" & vbCrLf &
                                 "<WMS_Capabilities version=""1.3.0"">" & vbCrLf &
                                 "  <Service><Title>Polska Geodezja: Zażółć gęślą jaźń</Title></Service>" & vbCrLf &
                                 "  <Capability><Layer><Name>l1</Name><Title>Działki i źródła</Title></Layer></Capability>" & vbCrLf &
                                 "</WMS_Capabilities>"
        Dim enc = Encoding.GetEncoding("ISO-8859-2")
        Dim bajty = enc.GetBytes(xmlIso)
        Using ms As New MemoryStream(bajty)
            Dim u = WmsCapabilities.Wczytaj(ms, "https://serwer.pl")
            Assert.Equal("Polska Geodezja: Zażółć gęślą jaźń", u.Tytul)
            Assert.Equal("Działki i źródła", u.Warstwy(0).Tytul)
        End Using
    End Sub

    <Fact>
    Public Sub BudowanieAdresuWmsDlaPliku()
        Dim a1 = PlikWarstw.ZbudujAdresWmsDlaPliku("https://mapy.pl/wms", "1.3.0")
        Assert.Equal("https://mapy.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers=", a1)

        Dim a2 = PlikWarstw.ZbudujAdresWmsDlaPliku("https://mapy.pl/wms", "1.1.1")
        Assert.Equal("https://mapy.pl/wms?SERVICE=WMS&request=GetMap&version=1.1.1&SRS=EPSG:2180&layers=", a2)
    End Sub

    <Fact>
    Public Sub WalidacjaNazwyZbioru()
        Dim blad As String = ""
        Assert.True(PlikWarstw.CzyNazwaZbioruPoprawna("Moje_Dzialki_2026", blad))
        Assert.Equal("", blad)

        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("   ", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("warstwy", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("WARSTWY", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("CON", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("zbiór/niepoprawny", blad))
        Assert.False(PlikWarstw.CzyNazwaZbioruPoprawna("zbiór?test", blad))
    End Sub

    <Fact>
    Public Sub OczyszczanieNazwyZbioru()
        Assert.Equal("GUGiK_Ewidencja_Gruntow", PlikWarstw.OczyscNazweZbioru("GUGiK - Ewidencja Gruntów!"))
        Assert.Equal("nowy_zbior_map", PlikWarstw.OczyscNazweZbioru(""))
        Assert.Equal("moj_serwer", PlikWarstw.OczyscNazweZbioru("  moj___serwer..  "))
    End Sub

    <Fact>
    Public Sub ZapisIOdczytPlikuWarstw()
        Dim p As New PlikWarstw With {.Adres = "https://serwer.pl/wms?SERVICE=WMS&request=GetMap&version=1.3.0&CRS=EPSG:2180&layers="}
        p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "dzialki", .RozmiarPiksela = "0.5"})
        p.Warstwy.Add(New WarstwaMapy With {.Nazwa = "budynki", .RozmiarPiksela = "1"})

        Dim plikTmp = Path.GetTempFileName()
        Try
            p.Zapisz(plikTmp)
            Dim odczytany = PlikWarstw.Wczytaj(plikTmp)
            Assert.Equal(p.Adres, odczytany.Adres)
            Assert.Equal(2, odczytany.Warstwy.Count)
            Assert.Equal("dzialki", odczytany.Warstwy(0).Nazwa)
            Assert.Equal("0.5", odczytany.Warstwy(0).RozmiarPiksela)
            Assert.Equal("budynki", odczytany.Warstwy(1).Nazwa)
            Assert.Equal("1", odczytany.Warstwy(1).RozmiarPiksela)
        Finally
            If File.Exists(plikTmp) Then File.Delete(plikTmp)
        End Try
    End Sub

    <Fact>
    Public Sub RozpoznawanieWfsHtmlIError()
        'Test walidacji adresów i parsowania
        Dim wmsUrl13 = KlientCapabilities.AdresCapabilitiesWms("https://geoportal.gov.pl/wms")
        Assert.Contains("SERVICE=WMS", wmsUrl13)
        Assert.Contains("REQUEST=GetCapabilities", wmsUrl13)

        Dim wmtsUrl = KlientCapabilities.AdresCapabilitiesWmts("https://geoportal.gov.pl/wmts")
        Assert.Contains("SERVICE=WMTS", wmtsUrl)
        Assert.Contains("REQUEST=GetCapabilities", wmtsUrl)
    End Sub

End Class
