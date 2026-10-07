'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports MapoTero.Core

''' <summary>
''' Ustawienia programu (zapisywane w lastsettings.txt) oraz parametry bieżącej sesji pobierania
''' (zapisywane w conf.txt). Dawniej zmienne publiczne Module1.
''' </summary>
Public Class UstawieniaProgramu

#Region "Pliki georeferencyjne segmentów"
    ''' <summary>Plik .map (OziExplorer).</summary>
    Public Property PlikMap As Boolean
    ''' <summary>Plik .gmi (GPSTuner).</summary>
    Public Property PlikGmi As Boolean
    ''' <summary>Pliki .wld i .points.</summary>
    Public Property PlikWldPoints As Boolean
    ''' <summary>World file (.jpgw, .pngw ...) i .prj (QGIS, ArcGIS).</summary>
    Public Property PlikWorldFile As Boolean
    ''' <summary>Plik .kml (Google Earth).</summary>
    Public Property PlikKml As Boolean
    ''' <summary>Plik .tab (MapInfo).</summary>
    Public Property PlikTab As Boolean

    Public Function OpcjeGeoreferencji() As OpcjeGeoreferencji
        Return New OpcjeGeoreferencji With {.Map = PlikMap, .Gmi = PlikGmi, .WldPoints = PlikWldPoints,
                                            .WorldFile = PlikWorldFile, .Kml = PlikKml, .Tab = PlikTab}
    End Function
#End Region

#Region "Pobieranie"
    ''' <summary>Liczba prób pobrania segmentów.</summary>
    Public Property IloscProbPobrania As Integer = 3
    ''' <summary>Przerwa między kolejnymi próbami [s].</summary>
    Public Property PrzerwaMiedzyProbami As Integer = 5
    ''' <summary>Liczba segmentów pobieranych jednocześnie.</summary>
    Public Property LiczbaWatkow As Integer = 4
    ''' <summary>Zamiana kolejności X i Y w zapytaniu (serwery niezgodne ze standardem).</summary>
    Public Property ZamienXY As Boolean
    ''' <summary>Przy powtórnym pobieraniu tylko segmenty o numerze wyższym niż ostatni pobrany.</summary>
    Public Property PobierajPowyzejOstatniego As Boolean
    ''' <summary>Limit czasu oczekiwania na odpowiedź serwera [s].</summary>
    Public Property LimitCzasuSekundy As Integer = 60
    ''' <summary>Tworzenie mapy TrekBuddy / Locus Map.</summary>
    Public Property TrekBuddy As Boolean
    ''' <summary>Dopisek do przedrostka w nazwie paczki TrekBuddy (wg stylu StylNazwyTrekBuddy).</summary>
    Public Property NazwaTrekBuddy As String = ""
    ''' <summary>Styl nazwy paczki TrekBuddy: 0 - sam przedrostek, 1 - zbiór map, 2 - warstwa, 3 - zbiór map i warstwa.</summary>
    Public Property StylNazwyTrekBuddy As Integer
    ''' <summary>Bok segmentu sprzed włączenia trybu TrekBuddy (przywracany po jego wyłączeniu); tylko w pamięci.</summary>
    Public Property BokSegmentuPrzedTrekBuddy As String = ""
    ''' <summary>Format obrazu WMS: jpeg, png, tiff ... (jeden z FormatySegmentow).</summary>
    Public Property Format As String = "jpeg"
    ''' <summary>Przedrostek nazw segmentów.</summary>
    Public Property Prefiks As String = "_"
    ''' <summary>Styl numeracji segmentów (tekst jak w plikach: NrWiersza_NrKolumny, 01_02_03, 1_2_3).</summary>
    Public Property Numeracja As String = "NrWiersza_NrKolumny"
    ''' <summary>Układ współrzędnych pobierania.</summary>
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    ''' <summary>Układ współrzędnych nowej sesji (gdy w folderze pobierania nie ma pliku conf.txt).</summary>
    Public Property UkladDomyslny As UkladWspolrzednych = UkladWspolrzednych.PL1992
#End Region

#Region "Usługi WMTS"
    ''' <summary>Pozostawianie pobranych kafli WMTS w folderze sesji (ponowne pobieranie tego obszaru bez łączenia z serwerem).</summary>
    Public Property ZachowajKafleWmts As Boolean
    ''' <summary>Jakość JPEG segmentów składanych z kafli WMTS (50-100).</summary>
    Public Property JakoscJpegWmts As Integer = 90
#End Region

    ''' <summary>
    ''' Formaty segmentów obsługiwane przez program (parametr FORMAT zapytania WMS bez "image/").
    ''' Format svg+xml z poprzednich wersji usunięto - obraz wektorowy nie może być skalibrowany ani scalony.
    ''' </summary>
    Public Shared ReadOnly Property FormatySegmentow As String()
        Get
            Return {"jpeg", "png", "png8", "png24", "png32", "gif", "tiff"}
        End Get
    End Property

    ''' <summary>Format z listy obsługiwanych; nieznany (np. svg+xml z dawnego conf.txt) - jpeg.</summary>
    Public Shared Function NormalizujFormat(format As String) As String
        Dim f As String = If(format, "").Trim().ToLowerInvariant()
        Return If(Array.IndexOf(FormatySegmentow, f) >= 0, f, "jpeg")
    End Function

    ''' <summary>Style numeracji segmentów (tekst zapisywany w plikach).</summary>
    Public Shared ReadOnly Property StyleNumeracji As String()
        Get
            Return {"NrWiersza_NrKolumny", "01_02_03", "1_2_3"}
        End Get
    End Property

#Region "Foldery"
    Public Property FolderSegmentow As String = ""
    Public Property FolderWarstwy1 As String = ""
    Public Property FolderWarstwy2 As String = ""
    Public Property FolderWynikowy As String = ""
#End Region

#Region "Widok okna głównego"
    Public Property EdycjaXY As Boolean
    Public Property KursorWgs84 As Boolean
    Public Property ZaznaczenieWgs84 As Boolean
    Public Property KursorISrodekMapy As Boolean = True
    Public Property SrodekMapySzerokosc As String = "52.3"
    Public Property SrodekMapyDlugosc As String = "19.2"
    Public Property SkalaMapy As String = "6"
#End Region

#Region "Scalanie"
    Public Property ScalanieWorldFile As Boolean
    Public Property ScalanieKml As Boolean
    Public Property ScalanieMap As Boolean
    Public Property ScalanieTab As Boolean
    ''' <summary>Format scalonego arkusza.</summary>
    Public Property FormatArkusza As FormatArkusza = FormatArkusza.GeoTiff
#End Region

    ''' <summary>Rozszerzenie plików segmentów dla bieżącego formatu.</summary>
    Public ReadOnly Property Rozszerzenie As String
        Get
            Return Wms.RozszerzeniePliku(Format)
        End Get
    End Property

    ''' <summary>Odczyt lastsettings.txt (brakujące ustawienia pozostają bez zmian).</summary>
    Public Sub Wczytaj(sciezka As String)
        Dim u = PlikUstawien.Wczytaj(sciezka, KodowanieSystemowe())
        FolderSegmentow = u.Tekst("folder segmentow", FolderSegmentow)
        PlikGmi = u.Logiczna("chkgmi", PlikGmi)
        PlikMap = u.Logiczna("chkmap", PlikMap)
        PlikWldPoints = u.Logiczna("chkwldpoints", PlikWldPoints)
        PlikWorldFile = u.Logiczna("chkjpgw", PlikWorldFile)
        PlikKml = u.Logiczna("chkkml", PlikKml)
        PlikTab = u.Logiczna("chktab", PlikTab)
        FolderWarstwy1 = u.Tekst("dolna", FolderWarstwy1)
        FolderWarstwy2 = u.Tekst("gorna", FolderWarstwy2)
        FolderWynikowy = u.Tekst("polaczone", FolderWynikowy)
        ZamienXY = u.Logiczna("XYswitched", ZamienXY)
        'zapisywane jako "numeracja_" (dawny plik domyślny zawierał "numeracja")
        Numeracja = u.Tekst("numeracja_", u.Tekst("numeracja", Numeracja))
        IloscProbPobrania = u.Calkowita("iloscProbPobrania", IloscProbPobrania)
        PrzerwaMiedzyProbami = u.Calkowita("przerwaMiedzyProbami", PrzerwaMiedzyProbami)
        SrodekMapySzerokosc = u.Tekst("x_start", SrodekMapySzerokosc)
        SrodekMapyDlugosc = u.Tekst("y_start", SrodekMapyDlugosc)
        SkalaMapy = u.Tekst("zoom_start", SkalaMapy)
        'ustawienie dodane w wersji 3.12 (układ współrzędnych jest parametrem sesji - zapisywany w conf.txt)
        LiczbaWatkow = Math.Max(1, Math.Min(16, u.Calkowita("watki", LiczbaWatkow)))
        LimitCzasuSekundy = Math.Max(10, Math.Min(600, u.Calkowita("limit_czasu", LimitCzasuSekundy)))
        ZachowajKafleWmts = u.Logiczna("kafle_wmts_zachowaj", ZachowajKafleWmts)
        JakoscJpegWmts = Math.Max(50, Math.Min(100, u.Calkowita("jakosc_jpeg_wmts", JakoscJpegWmts)))
        StylNazwyTrekBuddy = Math.Max(0, Math.Min(3, u.Calkowita("styl_nazwy_tb", StylNazwyTrekBuddy)))
        Dim epsg As Integer = u.Calkowita("uklad_domyslny", UkladDomyslny.Epsg)
        UkladDomyslny = UkladWspolrzednych.ZKodu(epsg)
    End Sub

    ''' <summary>Zapis lastsettings.txt - kolejność i nazwy jak w poprzednich wersjach, nowe ustawienia na końcu.</summary>
    Public Sub Zapisz(sciezka As String)
        Dim u As New PlikUstawien()
        u.Ustaw("folder segmentow", If(FolderSegmentow = "", FolderDownload, FolderSegmentow))
        u.Ustaw("chkgmi", PlikGmi)
        u.Ustaw("chkmap", PlikMap)
        u.Ustaw("chkwldpoints", PlikWldPoints)
        u.Ustaw("chkjpgw", PlikWorldFile)
        u.Ustaw("chkkml", PlikKml)
        u.Ustaw("chktab", PlikTab)
        u.Ustaw("dolna", If(FolderWarstwy1 = "", FolderDownload & "dolna\", FolderWarstwy1))
        u.Ustaw("gorna", If(FolderWarstwy2 = "", FolderDownload & "gorna\", FolderWarstwy2))
        u.Ustaw("polaczone", If(FolderWynikowy = "", FolderDownload & "polaczone\", FolderWynikowy))
        u.Ustaw("XYswitched", ZamienXY)
        u.Ustaw("numeracja_", Numeracja)
        u.Ustaw("iloscProbPobrania", IloscProbPobrania)
        u.Ustaw("przerwaMiedzyProbami", PrzerwaMiedzyProbami)
        u.Ustaw("x_start", SrodekMapySzerokosc.Replace(","c, "."c))
        u.Ustaw("y_start", SrodekMapyDlugosc.Replace(","c, "."c))
        u.Ustaw("zoom_start", SkalaMapy.Replace(","c, "."c))
        u.Ustaw("watki", LiczbaWatkow)
        u.Ustaw("limit_czasu", LimitCzasuSekundy)
        u.Ustaw("kafle_wmts_zachowaj", ZachowajKafleWmts)
        u.Ustaw("jakosc_jpeg_wmts", JakoscJpegWmts)
        u.Ustaw("styl_nazwy_tb", StylNazwyTrekBuddy)
        u.Ustaw("uklad_domyslny", UkladDomyslny.Epsg)
        u.Zapisz(sciezka, KodowanieSystemowe())
    End Sub

    ''' <summary>Przywraca ustawienia domyślne (przycisk "Resetuj ustawienia").</summary>
    Public Sub PrzywrocDomyslne()
        Prefiks = "_"
        Format = "jpeg"
        PobierajPowyzejOstatniego = False
        EdycjaXY = False
        KursorWgs84 = False
        ZaznaczenieWgs84 = False
        KursorISrodekMapy = True
        PlikMap = False
        PlikKml = False
        PlikWorldFile = False
        PlikTab = False
        PlikWldPoints = False
        PlikGmi = False
        Numeracja = "NrWiersza_NrKolumny"
        IloscProbPobrania = 3
        PrzerwaMiedzyProbami = 5
        LiczbaWatkow = 4
        LimitCzasuSekundy = 60
        ZachowajKafleWmts = False
        JakoscJpegWmts = 90
        TrekBuddy = False
        StylNazwyTrekBuddy = 0
        BokSegmentuPrzedTrekBuddy = ""
        ZamienXY = False
        'układ bieżącej sesji zmienia okno ustawień (Form1.PrzelaczUklad) - z przeliczeniem wpisanego zasięgu
        UkladDomyslny = UkladWspolrzednych.PL1992
        SrodekMapySzerokosc = "52.3"
        SrodekMapyDlugosc = "19.2"
        SkalaMapy = "6"
    End Sub

End Class

''' <summary>Rodzaje plików georeferencyjnych tworzonych dla obrazu.</summary>
Public Class OpcjeGeoreferencji
    Public Property Map As Boolean
    Public Property Gmi As Boolean
    Public Property WldPoints As Boolean
    Public Property WorldFile As Boolean
    Public Property Kml As Boolean
    Public Property Tab As Boolean

    Public ReadOnly Property Dowolna As Boolean
        Get
            Return Map OrElse Gmi OrElse WldPoints OrElse WorldFile OrElse Kml OrElse Tab
        End Get
    End Property
End Class
