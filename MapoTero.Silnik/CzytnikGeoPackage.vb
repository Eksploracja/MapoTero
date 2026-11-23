'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Data.SQLite
Imports MapoTero.Core

''' <summary>Warstwa wektorowa pliku GeoPackage (tabela z kolumną geometrii).</summary>
Public NotInheritable Class WarstwaGeoPackage
    Public Property Tabela As String = ""
    Public Property Kolumna As String = ""
    Public Property Nazwa As String = ""
    ''' <summary>Typ geometrii z gpkg_geometry_columns, np. MULTIPOLYGON, LINESTRING, GEOMETRY.</summary>
    Public Property TypGeometrii As String = ""
    Public Property Epsg As Integer
    Public Property LiczbaObiektow As Long

    ''' <summary>Czy warstwa może zawierać poligony (typ ogólny GEOMETRY też).</summary>
    Public ReadOnly Property MozePoligony As Boolean
        Get
            Return Not TypGeometrii.Contains("CURVE") AndAlso (TypGeometrii.Contains("POLYGON") OrElse TypGeometrii.Contains("GEOMETRY"))
        End Get
    End Property

    ''' <summary>Czy warstwa może zawierać linie.</summary>
    Public ReadOnly Property MozeLinie As Boolean
        Get
            Return TypGeometrii.Contains("LINESTRING") OrElse TypGeometrii.Contains("GEOMETRY")
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim opisTypu = If(TypGeometrii.Contains("POLYGON"), "poligony", If(TypGeometrii.Contains("LINESTRING"), "linie",
                          If(TypGeometrii.Contains("POINT"), "punkty", TypGeometrii.ToLowerInvariant())))
        Return $"{Nazwa} ({opisTypu}, {LiczbaObiektow} obiektów, EPSG:{Epsg})"
    End Function
End Class

''' <summary>
''' Odczyt warstw wektorowych z pliku GeoPackage (.gpkg, np. z QGIS) - poligony i linie do wyznaczenia obszaru
''' pobierania. Plik otwierany tylko do odczytu; geometrię rozkłada <see cref="CzytnikGeometriiGpkg"/>.
''' </summary>
Public NotInheritable Class CzytnikGeoPackage

    Private Shared Function Polacz(plik As String) As SQLiteConnection
        ' Bez puli połączeń - inaczej plik zostawałby zablokowany po zamknięciu okna i nie dałoby się go zmienić w QGIS.
        Dim b As New SQLiteConnectionStringBuilder With {.DataSource = plik, .Version = 3, .ReadOnly = True, .Pooling = False}
        Dim p As New SQLiteConnection(b.ToString())
        p.Open()
        Return p
    End Function

    ''' <summary>Warstwy wektorowe pliku (bez warstw kafli rastrowych). Pusta lista - plik bez warstw wektorowych.</summary>
    Public Shared Function Warstwy(plik As String) As List(Of WarstwaGeoPackage)
        Dim wynik As New List(Of WarstwaGeoPackage)
        Using p = Polacz(plik)
            If Not TabelaIstnieje(p, "gpkg_geometry_columns") Then Return wynik
            Using k As New SQLiteCommand(
                "SELECT g.table_name, g.column_name, g.geometry_type_name, g.srs_id, COALESCE(NULLIF(c.identifier, ''), g.table_name), " &
                "s.organization, s.organization_coordsys_id " &
                "FROM gpkg_geometry_columns g LEFT JOIN gpkg_contents c ON c.table_name = g.table_name " &
                "LEFT JOIN gpkg_spatial_ref_sys s ON s.srs_id = g.srs_id ORDER BY g.table_name", p)
                Using r = k.ExecuteReader()
                    While r.Read()
                        Dim srs = If(r.IsDBNull(3), 0, Convert.ToInt32(r.GetValue(3)))
                        Dim organizacja = If(r.IsDBNull(5), "", Convert.ToString(r.GetValue(5)))
                        ' srs_id w GeoPackage to numer wewnętrzny; zwykle równy EPSG, ale kod EPSG podaje gpkg_spatial_ref_sys
                        Dim epsg = If(organizacja.Equals("EPSG", StringComparison.OrdinalIgnoreCase) AndAlso Not r.IsDBNull(6),
                                      Convert.ToInt32(r.GetValue(6)), srs)
                        wynik.Add(New WarstwaGeoPackage With {
                            .Tabela = r.GetString(0), .Kolumna = r.GetString(1),
                            .TypGeometrii = Convert.ToString(r.GetValue(2)).ToUpperInvariant(), .Epsg = epsg,
                            .Nazwa = Convert.ToString(r.GetValue(4))})
                    End While
                End Using
            End Using
            For i = wynik.Count - 1 To 0 Step -1
                Dim w = wynik(i)
                Try
                    Using k As New SQLiteCommand($"SELECT COUNT(*) FROM {Cytuj(w.Tabela)}", p)
                        w.LiczbaObiektow = Convert.ToInt64(k.ExecuteScalar())
                    End Using
                Catch
                    wynik.RemoveAt(i)
                End Try
            Next
        End Using
        Return wynik
    End Function

    ''' <summary>Poligony i linie warstwy we współrzędnych pliku (z kodem EPSG warstwy).</summary>
    Public Shared Function Wczytaj(plik As String, warstwa As WarstwaGeoPackage) As GeometriaWektorowa
        Dim wynik As New GeometriaWektorowa With {.Epsg = warstwa.Epsg}
        Using p = Polacz(plik), k As New SQLiteCommand($"SELECT {Cytuj(warstwa.Kolumna)} FROM {Cytuj(warstwa.Tabela)}", p),
              r = k.ExecuteReader()
            While r.Read()
                If r.IsDBNull(0) Then Continue While
                Dim blob = TryCast(r.GetValue(0), Byte())
                If blob IsNot Nothing Then wynik.Dolacz(CzytnikGeometriiGpkg.ZBlobu(blob))
            End While
        End Using
        Return wynik
    End Function

    Private Shared Function TabelaIstnieje(p As SQLiteConnection, nazwa As String) As Boolean
        Using k As New SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type IN ('table','view') AND name = @n", p)
            k.Parameters.AddWithValue("@n", nazwa)
            Return Convert.ToInt64(k.ExecuteScalar()) > 0
        End Using
    End Function

    ''' <summary>Nazwa tabeli lub kolumny w cudzysłowie SQL (nazwy pochodzą z pliku, więc mogą zawierać spacje i cudzysłowy).</summary>
    Private Shared Function Cytuj(nazwa As String) As String
        Return """" & nazwa.Replace("""", """""") & """"
    End Function
End Class
