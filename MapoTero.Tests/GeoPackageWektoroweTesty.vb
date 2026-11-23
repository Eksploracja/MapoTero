'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Data.SQLite
Imports System.IO
Imports MapoTero
Imports MapoTero.Core
Imports Xunit

''' <summary>Odczyt poligonów i linii z GeoPackage: WKB (obie kolejności bajtów, Z/M), nagłówek GP i warstwy pliku.</summary>
Public Class GeoPackageWektoroweTesty

#Region "Budowa WKB i plików testowych"

    Private Shared Function Wkb(Optional malyEndian As Boolean = True) As BinaryWriterWkb
        Return New BinaryWriterWkb(malyEndian)
    End Function

    ''' <summary>Prosty zapis WKB do testów (kolejność bajtów do wyboru).</summary>
    Private NotInheritable Class BinaryWriterWkb
        Private ReadOnly _ms As New MemoryStream()
        Private ReadOnly _maly As Boolean
        Public Sub New(maly As Boolean)
            _maly = maly
        End Sub
        Public Function Naglowek(typ As UInteger) As BinaryWriterWkb
            _ms.WriteByte(If(_maly, CByte(1), CByte(0)))
            Return UInt(typ)
        End Function
        Public Function UInt(v As UInteger) As BinaryWriterWkb
            Dim b = BitConverter.GetBytes(v)
            If BitConverter.IsLittleEndian <> _maly Then Array.Reverse(b)
            _ms.Write(b, 0, 4)
            Return Me
        End Function
        Public Function Liczby(ParamArray v() As Double) As BinaryWriterWkb
            For Each d In v
                Dim b = BitConverter.GetBytes(d)
                If BitConverter.IsLittleEndian <> _maly Then Array.Reverse(b)
                _ms.Write(b, 0, 8)
            Next
            Return Me
        End Function
        Public Function Bajty() As Byte()
            Return _ms.ToArray()
        End Function
    End Class

    ''' <summary>Pierścień prostokąta (zamknięty, 5 punktów) jako lista współrzędnych x, y.</summary>
    Private Shared Function Prostokat(x0 As Double, y0 As Double, x1 As Double, y1 As Double) As Double()
        Return {x0, y0, x1, y0, x1, y1, x0, y1, x0, y0}
    End Function

    Private Shared Function PoligonWkb(ParamArray pierscienie()() As Double) As Byte()
        Dim w = Wkb().Naglowek(3).UInt(CUInt(pierscienie.Length))
        For Each p In pierscienie
            w.UInt(CUInt(p.Length \ 2)).Liczby(p)
        Next
        Return w.Bajty()
    End Function

    Private Shared Function LiniaWkb(ParamArray xy() As Double) As Byte()
        Return Wkb().Naglowek(2).UInt(CUInt(xy.Length \ 2)).Liczby(xy).Bajty()
    End Function

    ''' <summary>Geometria GeoPackage: „GP”, wersja 0, flagi (mały endian, opcjonalna koperta XY), srs_id, WKB.</summary>
    Private Shared Function BlobGp(wkb() As Byte, srs As Integer, Optional koperta As Boolean = False) As Byte()
        Using ms As New MemoryStream()
            ms.Write({AscW("G"c), AscW("P"c), 0, If(koperta, CByte(&H3), CByte(&H1))}.Select(Function(b) CByte(b)).ToArray(), 0, 4)
            ms.Write(BitConverter.GetBytes(srs), 0, 4)
            If koperta Then
                For Each d In {0.0, 1.0, 0.0, 1.0}
                    ms.Write(BitConverter.GetBytes(d), 0, 8)
                Next
            End If
            ms.Write(wkb, 0, wkb.Length)
            Return ms.ToArray()
        End Using
    End Function

    Private Shared Function NowyPlik() As String
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_gpkg_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Return Path.Combine(folder, "obszary.gpkg")
    End Function

    ''' <summary>GeoPackage z warstwami: „dzialki” (poligony PL-1992, srs_id 100 → EPSG:2180) i „trasa” (linia WGS84).</summary>
    Private Shared Sub UtworzPlik(plik As String, poligony As IEnumerable(Of Byte()), linie As IEnumerable(Of Byte()))
        Using p As New SQLiteConnection("Data Source=" & plik & ";Version=3;Pooling=False;")
            p.Open()
            Dim sql = Sub(tekst As String)
                          Using k As New SQLiteCommand(tekst, p)
                              k.ExecuteNonQuery()
                          End Using
                      End Sub
            sql("CREATE TABLE gpkg_spatial_ref_sys (srs_name TEXT, srs_id INTEGER PRIMARY KEY, organization TEXT, organization_coordsys_id INTEGER, definition TEXT)")
            sql("INSERT INTO gpkg_spatial_ref_sys VALUES ('ETRF2000-PL / CS92', 100, 'EPSG', 2180, ''), ('WGS 84', 4326, 'EPSG', 4326, '')")
            sql("CREATE TABLE gpkg_contents (table_name TEXT PRIMARY KEY, data_type TEXT, identifier TEXT, srs_id INTEGER)")
            sql("INSERT INTO gpkg_contents VALUES ('dzialki', 'features', 'Działki ewidencyjne', 100), ('trasa', 'features', 'trasa', 4326), ('kafle', 'tiles', 'kafle', 3857)")
            sql("CREATE TABLE gpkg_geometry_columns (table_name TEXT, column_name TEXT, geometry_type_name TEXT, srs_id INTEGER, z INTEGER, m INTEGER)")
            sql("INSERT INTO gpkg_geometry_columns VALUES ('dzialki', 'geom', 'MULTIPOLYGON', 100, 0, 0), ('trasa', 'shape', 'LINESTRING', 4326, 0, 0)")
            sql("CREATE TABLE dzialki (fid INTEGER PRIMARY KEY, geom BLOB)")
            sql("CREATE TABLE trasa (fid INTEGER PRIMARY KEY, shape BLOB)")
            For Each tabela In {("dzialki", "geom", poligony, 100), ("trasa", "shape", linie, 4326)}
                For Each g In tabela.Item3
                    Using k As New SQLiteCommand($"INSERT INTO {tabela.Item1} ({tabela.Item2}) VALUES (@g)", p)
                        k.Parameters.AddWithValue("@g", BlobGp(g, tabela.Item4))
                        k.ExecuteNonQuery()
                    End Using
                Next
            Next
            Using k As New SQLiteCommand("INSERT INTO dzialki (geom) VALUES (NULL)", p)
                k.ExecuteNonQuery()
            End Using
        End Using
    End Sub

#End Region

    <Fact>
    Public Sub Wkb_PoligonZDziura()
        Dim g = CzytnikGeometriiGpkg.ZWkb(PoligonWkb(Prostokat(0, 0, 10, 10), Prostokat(2, 2, 4, 4)))
        Dim poligon = Assert.Single(g.Poligony)
        Assert.Equal(2, poligon.Count)
        Assert.Equal(5, poligon(0).Count)
        Assert.Equal(10.0, poligon(0)(1).Wschod)
        Assert.Equal(0.0, poligon(0)(1).Polnoc)
        Assert.Empty(g.Linie)
    End Sub

    <Fact>
    Public Sub Wkb_LiniaZWysokosciaWKolejnosciBigEndian()
        ' ISO WKB LineString Z (1002): trzy liczby na punkt, wysokość pomijana
        Dim dane = Wkb(malyEndian:=False).Naglowek(1002).UInt(2).Liczby(18.5, 54.4, 120.0, 18.6, 54.5, 130.0).Bajty()
        Dim linia = Assert.Single(CzytnikGeometriiGpkg.ZWkb(dane).Linie)
        Assert.Equal(New Punkt2D(18.5, 54.4), linia(0))
        Assert.Equal(New Punkt2D(18.6, 54.5), linia(1))
    End Sub

    <Fact>
    Public Sub Wkb_EwkbZFlagaZ_IKolekcjaGeometrii()
        Dim punkt = Wkb().Naglowek(1).Liczby(1, 2).Bajty()
        Dim liniaZ = Wkb().Naglowek(&H80000002UI).UInt(2).Liczby(0, 0, 5, 1, 1, 5).Bajty()
        Dim kolekcja = Wkb().Naglowek(7).UInt(3).Bajty().Concat(punkt).Concat(liniaZ).Concat(PoligonWkb(Prostokat(0, 0, 1, 1))).ToArray()
        Dim g = CzytnikGeometriiGpkg.ZWkb(kolekcja)
        Assert.Single(g.Poligony)
        Assert.Equal(New Punkt2D(1, 1), Assert.Single(g.Linie)(1))
    End Sub

    <Fact>
    Public Sub BlobGp_ZKoperta_IPustaGeometria()
        Dim multi = Wkb().Naglowek(6).UInt(2).Bajty().Concat(PoligonWkb(Prostokat(0, 0, 1, 1))).Concat(PoligonWkb(Prostokat(5, 5, 6, 6))).ToArray()
        Assert.Equal(2, CzytnikGeometriiGpkg.ZBlobu(BlobGp(multi, 4326, koperta:=True)).Poligony.Count)
        Dim pusta = BlobGp(PoligonWkb(Prostokat(0, 0, 1, 1)), 4326)
        pusta(3) = pusta(3) Or CByte(&H10)
        Assert.Empty(CzytnikGeometriiGpkg.ZBlobu(pusta).Poligony)
        Assert.Throws(Of InvalidDataException)(Function() CzytnikGeometriiGpkg.ZWkb(Wkb().Naglowek(2).UInt(1000000).Bajty()))
    End Sub

    <Fact>
    Public Sub Plik_WarstwyPoligonowILinii_ZasiegWUkladzieSesji()
        Dim plik = NowyPlik()
        Try
            UtworzPlik(plik, {PoligonWkb(Prostokat(568000, 680000, 570000, 682000)), PoligonWkb(Prostokat(571000, 683000, 572000, 684000))},
                       {LiniaWkb(18.5, 54.4, 18.6, 54.45, 18.7, 54.5)})
            Dim warstwy = CzytnikGeoPackage.Warstwy(plik)
            Assert.Equal(2, warstwy.Count) ' warstwa kafli pominięta
            Dim dzialki = warstwy.Single(Function(w) w.Tabela = "dzialki")
            Assert.Equal(2180, dzialki.Epsg) ' srs_id 100 przeliczony na kod EPSG
            Assert.Equal("Działki ewidencyjne", dzialki.Nazwa)
            Assert.Equal(3, dzialki.LiczbaObiektow)
            Assert.True(dzialki.MozePoligony)
            Assert.False(dzialki.MozeLinie)

            Dim geometria = CzytnikGeoPackage.Wczytaj(plik, dzialki)
            Assert.Equal(2, geometria.Poligony.Count) ' pusta geometria (NULL) pominięta
            Dim z = geometria.ZestawPoligonowWgs84().ZasiegWUkladzie(UkladWspolrzednych.PL1992)
            Assert.InRange(z.YLewy, 567999.0, 568001.0)
            Assert.InRange(z.XDol, 679999.0, 680001.0)
            Assert.InRange(z.YPrawy, 571999.0, 572001.0)
            Assert.InRange(z.XGora, 683999.0, 684001.0)

            Dim trasa = warstwy.Single(Function(w) w.Tabela = "trasa")
            Assert.True(trasa.MozeLinie)
            Dim k = CzytnikGeoPackage.Wczytaj(plik, trasa).TrasaWgs84("trasa")
            Assert.True(k.Poprawny)
            Assert.Equal(3, k.LiczbaPunktow)
            Assert.Equal(54.4, k.Linie(0)(0).Szerokosc, 6)
            Assert.Equal(18.5, k.Linie(0)(0).Dlugosc, 6)
        Finally
            Directory.Delete(Path.GetDirectoryName(plik), True)
        End Try
    End Sub

    <Theory>
    <InlineData(2180, True)>
    <InlineData(2178, True)>
    <InlineData(4326, True)>
    <InlineData(3857, True)>
    <InlineData(32634, True)>
    <InlineData(31467, False)>
    <InlineData(0, False)>
    Public Sub ObslugiwaneUklady(epsg As Integer, obslugiwany As Boolean)
        Assert.Equal(obslugiwany, GeometriaWektorowa.ObslugiwanyUklad(epsg))
        If Not obslugiwany Then
            Assert.Throws(Of NotSupportedException)(Function() GeometriaWektorowa.DoWgs84(epsg, New Punkt2D(1, 1)))
        End If
    End Sub
End Class
