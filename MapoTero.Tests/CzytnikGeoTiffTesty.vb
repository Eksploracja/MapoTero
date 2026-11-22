Option Strict On
Option Explicit On

Imports MapoTero.Core
Imports Xunit

Public Class CzytnikGeoTiffTesty

    <Theory>
    <InlineData("PROJCS[""Poland_CS92"",GEOGCS[""GCS_WGS_1984"",DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137,298.257223563]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",19],PARAMETER[""scale_factor"",0.9993],PARAMETER[""false_easting"",500000],PARAMETER[""false_northing"",-5300000],UNIT[""Meter"",1]]", 2180)>
    <InlineData("PROJCS[""ETRF2000-PL / CS92"",AUTHORITY[""EPSG"",""2180""]]", 2180)>
    <InlineData("PROJCS[""ETRF2000-PL / CS2000 zone 5"",AUTHORITY[""EPSG"",""2176""]]", 2176)>
    <InlineData("PROJCS[""ETRF2000-PL / CS2000 zone 7"",AUTHORITY[""EPSG"",""2178""]]", 2178)>
    <InlineData("PROJCS[""WGS 84 / UTM zone 33N"",AUTHORITY[""EPSG"",""32633""]]", 32633)>
    <InlineData("PROJCS[""WGS 84 / Pseudo-Mercator"",AUTHORITY[""EPSG"",""3857""]]", 3857)>
    Public Sub RozpoznajEpsgZPlikuPrj_RozpoznajeKody(wkt As String, oczekiwanyEpsg As Integer)
        Dim epsg As Integer? = CzytnikGeoTiff.RozpoznajEpsgZPlikuPrj(wkt)
        Assert.True(epsg.HasValue)
        Assert.Equal(oczekiwanyEpsg, epsg.Value)
    End Sub

    <Fact>
    Public Sub RozpoznajEpsgZPlikuPrj_RozpoznajeParametryCentralMeridian()
        ' Esri WKT bez tagu AUTHORITY dla strefy 6 (południk 18, false easting 6 500 000)
        Dim wkt As String = "PROJCS[""NieznanaNazwa"",PARAMETER[""Central_Meridian"", 18.0],PARAMETER[""False_Easting"", 6500000.0]]"
        Dim epsg As Integer? = CzytnikGeoTiff.RozpoznajEpsgZPlikuPrj(wkt)
        Assert.True(epsg.HasValue)
        Assert.Equal(2177, epsg.Value)
    End Sub

    <Fact>
    Public Sub GeoreferencjaGeoTiff_PikselNaWspolrzedne_LiczyPoprawnie()
        Dim geo As New GeoreferencjaGeoTiff() With {
            .A = 0.5,
            .B = 0.0,
            .C = 500000.0,
            .D = 0.0,
            .E = -0.5,
            .F = 400000.0
        }

        Dim wsp = geo.PikselNaWspolrzedne(100.0, 200.0)
        Assert.Equal(500050.0, wsp.X)
        Assert.Equal(399900.0, wsp.Y)
    End Sub

End Class
