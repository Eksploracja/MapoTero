Option Strict On
Option Explicit On

Imports MapoTero
Imports Xunit

    Public Class GarminUsbTesty

        <Theory>
        <InlineData("Gorce_Lubań.kmz", "Gorce_Luban.kmz")>
        <InlineData("Ślężańska_Ścieżka.kmz", "Slezanska_Sciezka.kmz")>
        <InlineData("Żółty szlak - Dolina Będkowska.jnx", "Zolty_szlak_-_Dolina_Bedkowska.jnx")>
        <InlineData("Łódź_i_Okolice.kmz", "Lodz_i_Okolice.kmz")>
        <InlineData("mapa 123 test.kmz", "mapa_123_test.kmz")>
        Public Sub SanityzujNazwePlikuGarmina_UsuwaPolskieZnakiISpecjalne(wejscie As String, oczekiwana As String)
            Dim wynik As String = MenadzerGarminUsb.SanityzujNazwePlikuGarmina(wejscie)
            Assert.Equal(oczekiwana, wynik)
        End Sub

        <Fact>
        Public Sub PobierzPodfolderDocelowy_RozpoznajeKmzIJnx()
            Assert.Equal("CustomMaps", MenadzerGarminUsb.PobierzPodfolderDocelowy("trasa.kmz"))
            Assert.Equal("BirdsEye", MenadzerGarminUsb.PobierzPodfolderDocelowy("trasa.jnx"))
            Assert.Throws(Of NotSupportedException)(Sub() MenadzerGarminUsb.PobierzPodfolderDocelowy("trasa.mbtiles"))
        End Sub

        <Fact>
        Public Sub DyskGarmina_FormatujRozmiar_DzialaPoprawnie()
            Assert.Equal("500 B", DyskGarmina.FormatujRozmiar(500L))
            Assert.Equal("1.5 KB", DyskGarmina.FormatujRozmiar(1536L))
            Assert.Equal("10.0 MB", DyskGarmina.FormatujRozmiar(10L * 1024L * 1024L))
            Assert.Equal("1.50 GB", DyskGarmina.FormatujRozmiar(CLng(1.5 * 1024.0 * 1024.0 * 1024.0)))
        End Sub

        <Fact>
        Public Sub WolnaNazwaPliku_NieNadpisujeIstniejacejMapy()
            Dim katalog = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MapoTero_garmin_" & Guid.NewGuid().ToString("N"))
            System.IO.Directory.CreateDirectory(katalog)
            Try
                Assert.Equal(System.IO.Path.Combine(katalog, "mapa.kmz"), MenadzerGarminUsb.WolnaNazwaPliku(katalog, "mapa.kmz"))
                System.IO.File.WriteAllText(System.IO.Path.Combine(katalog, "mapa.kmz"), "x")
                Assert.Equal(System.IO.Path.Combine(katalog, "mapa_2.kmz"), MenadzerGarminUsb.WolnaNazwaPliku(katalog, "mapa.kmz"))
                System.IO.File.WriteAllText(System.IO.Path.Combine(katalog, "mapa_2.kmz"), "x")
                Assert.Equal(System.IO.Path.Combine(katalog, "mapa_3.kmz"), MenadzerGarminUsb.WolnaNazwaPliku(katalog, "mapa.kmz"))
            Finally
                System.IO.Directory.Delete(katalog, True)
            End Try
        End Sub

    End Class
