'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Text.RegularExpressions

''' <summary>
''' Nazwa pliku wynikowego okna Eksport mapy. Dawniej każda mapa nazywała się „_eksport_6x6.mbtiles”, więc w telefonie
''' i w Garminie wszystkie mapy miały podobne, nic niemówiące nazwy, a ponowny eksport nadpisywał poprzedni plik
''' bez pytania (audyt ER-10). Nazwa domyślna pochodzi teraz od folderu sesji (warstwy i data pobrania).
''' </summary>
Public NotInheritable Class NazwaPlikuEksportu
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Końcówka nazwy dla formatu: rozszerzenie albo dopisek z rozszerzeniem (KMZ Garmin, TrekBuddy, folder OruxMaps).
    ''' </summary>
    Public Shared Function Przyrostek(rodzaj As RodzajEksportu, formatArkusza As FormatArkusza) As String
        Select Case rodzaj
            Case RodzajEksportu.Jnx : Return ".jnx"
            Case RodzajEksportu.MBTiles : Return ".mbtiles"
            Case RodzajEksportu.GeoPackage : Return ".gpkg"
            Case RodzajEksportu.SQLiteDB : Return ".sqlitedb"
            Case RodzajEksportu.OruxMaps : Return "_oruxmaps"
            Case RodzajEksportu.GeoTiff : Return "." & ZapisRastra.Rozszerzenie(formatArkusza)
            Case RodzajEksportu.TrekBuddy : Return "_trekbuddy.tar"
            Case RodzajEksportu.KmzGarmin : Return "_garmin.kmz"
            Case Else : Return ".kmz"
        End Select
    End Function

    ''' <summary>
    ''' Czytelna nazwa sesji z nazwy jej folderu: „sesja_ortofotomapa_2026-11-25_101233” → „ortofotomapa_2026-11-25_101233”.
    ''' Folder bez nazwy (sam folder pobrań) albo nazwa bez liter i cyfr - „mapa”.
    ''' </summary>
    Public Shared Function NazwaSesji(folderSesji As String) As String
        If String.IsNullOrWhiteSpace(folderSesji) Then Return "mapa"
        Dim nazwa = Path.GetFileName(folderSesji.TrimEnd("\"c, "/"c))
        If nazwa.StartsWith("sesja_", StringComparison.OrdinalIgnoreCase) Then nazwa = nazwa.Substring(6)
        nazwa = Regex.Replace(nazwa, "[\\/:*?""<>|]", "_").Trim(" "c, "."c, "_"c)
        If nazwa = "" OrElse String.Equals(nazwa, "download", StringComparison.OrdinalIgnoreCase) Then Return "mapa"
        Return nazwa
    End Function

    ''' <summary>
    ''' Domyślna nazwa pliku (bez folderu). Arkusz zachowuje przedrostek arkusza i wymiary siatki, bo po nim korekta obrazów
    ''' w folderze i eksport CAD rozpoznają arkusz; pozostałe formaty biorą nazwę sesji.
    ''' </summary>
    Public Shared Function NazwaDomyslna(folderSesji As String, rodzaj As RodzajEksportu, formatArkusza As FormatArkusza,
                                         kolumny As Integer, wiersze As Integer, przedrostekArkusza As String) As String
        Return PodstawaDomyslna(folderSesji, rodzaj, kolumny, wiersze, przedrostekArkusza) & Przyrostek(rodzaj, formatArkusza)
    End Function

    ''' <summary>Domyślna nazwa bez końcówki formatu.</summary>
    Public Shared Function PodstawaDomyslna(folderSesji As String, rodzaj As RodzajEksportu, kolumny As Integer, wiersze As Integer,
                                            przedrostekArkusza As String) As String
        If rodzaj = RodzajEksportu.GeoTiff Then Return przedrostekArkusza & kolumny & "x" & wiersze
        Return NazwaSesji(folderSesji)
    End Function

    ''' <summary>
    ''' Podstawa nazwy (bez końcówki formatu) z pełnej ścieżki wybranej przez użytkownika: odcina rozpoznaną końcówkę
    ''' dowolnego formatu, żeby po zmianie formatu nazwa nie miała dwóch rozszerzeń („mapa.mbtiles.jnx”).
    ''' </summary>
    Public Shared Function PodstawaZeSciezki(sciezka As String) As String
        Dim nazwa = Path.GetFileName(sciezka)
        For Each koncowka In {"_garmin.kmz", "_trekbuddy.tar", "_oruxmaps", ".mbtiles", ".gpkg", ".sqlitedb", ".jnx", ".kmz",
                              ".tif", ".tiff", ".jpg", ".jpeg", ".png", ".tar"}
            If nazwa.EndsWith(koncowka, StringComparison.OrdinalIgnoreCase) AndAlso nazwa.Length > koncowka.Length Then
                Return nazwa.Substring(0, nazwa.Length - koncowka.Length)
            End If
        Next
        Return nazwa
    End Function
End Class
