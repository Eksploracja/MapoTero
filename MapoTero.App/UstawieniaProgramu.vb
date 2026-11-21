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
    ''' <summary>Limit czasu oczekiwania na odpowiedź serwera [s].</summary>
    Public Property LimitCzasuSekundy As Integer = 120
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
    ''' <summary>Przezroczyste tło segmentów PNG / GIF (TRANSPARENT=TRUE) - warstwy nakładane na inne mapy.</summary>
    Public Property Przezroczystosc As Boolean
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
    'foldery usuniętego narzędzia nakładania dwóch map PNG - zapisywane dalej, żeby lastsettings.txt miał te same
    'wpisy co w starszych wersjach programu (wersje sprzed 3.12 czytały plik po kolei, bez nazw)
    Public Property FolderWarstwy1 As String = ""
    Public Property FolderWarstwy2 As String = ""
    Public Property FolderWynikowy As String = ""
#End Region

#Region "Widok okna głównego"
    Public Property EdycjaXY As Boolean
    Public Property KursorWgs84 As Boolean
    Public Property ZaznaczenieWgs84 As Boolean
    Public Property KursorISrodekMapy As Boolean = True
    Public Property SiatkaSegmentowNaMapie As Boolean = True
    ''' <summary>Szerokość prawego panelu ustawiona przez użytkownika w pikselach; 0 - szerokość domyślna.</summary>
    Public Property SzerokoscPanelu As Integer
    ''' <summary>Podkład mapy w oknie głównym: osm, google albo bing (dawniej po każdym uruchomieniu wracał OpenStreetMap).</summary>
    Public Property PodkladMapy As String = "osm"

    ''' <summary>Podkład z listy obsługiwanych; nieznany - osm.</summary>
    Public Shared Function NormalizujPodklad(podklad As String) As String
        Dim p As String = If(podklad, "").Trim().ToLowerInvariant()
        Return If(p = "google" OrElse p = "bing", p, "osm")
    End Function

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

#Region "Eksport do CAD"
    ''' <summary>Elementy wektorowe rysunku CAD: obrys zasięgu, siatka i numery segmentów, trasa korytarza, opis mapy.</summary>
    Public Property CadObrys As Boolean = True
    Public Property CadSiatka As Boolean = True
    Public Property CadNumery As Boolean = True
    Public Property CadTrasa As Boolean = True
    Public Property CadOpis As Boolean = True
    ''' <summary>Pliki wynikowe: rysunek DXF (ze ścieżkami rastrów względem pliku DXF) i skrypt SCR.</summary>
    Public Property CadDxf As Boolean = True
    Public Property CadSciezkiWzgledne As Boolean = True
    Public Property CadScr As Boolean = True
    ''' <summary>Program LISP (.lsp) - wariant skryptu przywracający ustawienia rysunku (AutoCAD, BricsCAD, ZWCAD, LT 2024+).</summary>
    Public Property CadLsp As Boolean
#End Region

#Region "Korekta obrazu"
    ''' <summary>Czy optymalizacja czytelności obrazu jest domyślnie włączona.</summary>
    Public Property KorektaWlaczona As Boolean = False
    ''' <summary>Domyślny profil korekty obrazu.</summary>
    Public Property KorektaProfil As ProfilKorektyRastra = ProfilKorektyRastra.Brak
    ''' <summary>Wartości suwaków profilu „Własny” (dawniej niezapisywane - po zamknięciu okna wracały wartości Garmin MIP).</summary>
    Public Property KorektaGamma As Double = 0.75
    Public Property KorektaKontrast As Double = 1.25
    Public Property KorektaNasycenie As Double = 1.35
    Public Property KorektaWyostrzanie As Double = 0.6

    ''' <summary>
    ''' Zapamiętana korekta obrazu. profilGdyBrak - profil podstawiany, gdy żaden nie był wybrany (okna, w których
    ''' korekta jest celem, np. korekta folderu); stan włączenia pozostaje wtedy zapamiętany.
    ''' </summary>
    Public Function OpcjeKorekty(Optional profilGdyBrak As ProfilKorektyRastra = ProfilKorektyRastra.Brak) As OpcjeKorekcjiMip
        If KorektaProfil = ProfilKorektyRastra.Brak AndAlso profilGdyBrak <> ProfilKorektyRastra.Brak Then
            Dim o = OpcjeKorekcjiMip.Odtworz(True, profilGdyBrak, KorektaGamma, KorektaKontrast, KorektaNasycenie, KorektaWyostrzanie)
            o.Wlaczona = KorektaWlaczona
            Return o
        End If
        Return OpcjeKorekcjiMip.Odtworz(KorektaWlaczona, KorektaProfil, KorektaGamma, KorektaKontrast, KorektaNasycenie, KorektaWyostrzanie)
    End Function

    ''' <summary>Zapamiętuje korektę obrazu (włączenie, profil i wartości suwaków).</summary>
    Public Sub ZapamietajKorekte(o As OpcjeKorekcjiMip)
        If o Is Nothing Then Exit Sub
        KorektaWlaczona = o.Wlaczona
        KorektaProfil = o.Profil
        KorektaGamma = o.Gamma
        KorektaKontrast = o.Kontrast
        KorektaNasycenie = o.Nasycenie
        KorektaWyostrzanie = o.Wyostrzanie
    End Sub
#End Region

    ''' <summary>Rozszerzenie plików segmentów dla bieżącego formatu.</summary>
    Public ReadOnly Property Rozszerzenie As String
        Get
            Return Wms.RozszerzeniePliku(Format)
        End Get
    End Property

    ''' <summary>
    ''' Odczyt lastsettings.txt (brakujące ustawienia pozostają bez zmian). Przedrostek, format i przezroczystość
    ''' segmentów są parametrami sesji (conf.txt) - z lastsettings.txt wczytywane tylko dla nowej sesji.
    ''' </summary>
    Public Sub Wczytaj(sciezka As String, Optional parametryNowejSesji As Boolean = False)
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
        'XYswitched (zamiana X i Y w zapytaniu) nie jest już odczytywane - opcja działała na wszystkie serwery i pozostawiona
        'włączona psuła pobieranie z Geoportalu; serwer niezgodny z WMS 1.3.0 obsługuje VERSION=1.1.1 w adresie
        PodkladMapy = NormalizujPodklad(u.Tekst("podklad_mapy", PodkladMapy))
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
        SiatkaSegmentowNaMapie = u.Logiczna("siatka_segmentow", SiatkaSegmentowNaMapie)
        'widok okna głównego (dawniej niezapisywany - po ponownym uruchomieniu wracały wartości domyślne)
        KursorISrodekMapy = u.Logiczna("kursor_i_srodek", KursorISrodekMapy)
        KursorWgs84 = u.Logiczna("kursor_wgs84", KursorWgs84)
        ZaznaczenieWgs84 = u.Logiczna("zaznaczenie_wgs84", ZaznaczenieWgs84)
        EdycjaXY = u.Logiczna("edycja_xy", EdycjaXY)
        SzerokoscPanelu = Math.Max(0, u.Calkowita("szerokosc_panelu", SzerokoscPanelu))
        'opcje okna eksportu do CAD
        CadObrys = u.Logiczna("cad_obrys", CadObrys)
        CadSiatka = u.Logiczna("cad_siatka", CadSiatka)
        CadNumery = u.Logiczna("cad_numery", CadNumery)
        CadTrasa = u.Logiczna("cad_trasa", CadTrasa)
        CadOpis = u.Logiczna("cad_opis", CadOpis)
        CadDxf = u.Logiczna("cad_dxf", CadDxf)
        CadSciezkiWzgledne = u.Logiczna("cad_sciezki_wzgledne", CadSciezkiWzgledne)
        CadScr = u.Logiczna("cad_scr", CadScr)
        CadLsp = u.Logiczna("cad_lsp", CadLsp)
        KorektaWlaczona = u.Logiczna("korekta_wlaczona", KorektaWlaczona)
        KorektaProfil = CType(u.Calkowita("korekta_profil", CInt(KorektaProfil)), ProfilKorektyRastra)
        KorektaGamma = u.Liczba("korekta_gamma", KorektaGamma)
        KorektaKontrast = u.Liczba("korekta_kontrast", KorektaKontrast)
        KorektaNasycenie = u.Liczba("korekta_nasycenie", KorektaNasycenie)
        KorektaWyostrzanie = u.Liczba("korekta_wyostrzanie", KorektaWyostrzanie)
        'co najmniej jeden plik wynikowy - inaczej przycisk Eksportuj byłby nieaktywny bez widocznej przyczyny
        If Not CadDxf AndAlso Not CadScr AndAlso Not CadLsp Then CadDxf = True
        If parametryNowejSesji Then
            'pusty przedrostek jest dozwolony - odczyt wprost, gdy ustawienie istnieje
            If u.Zawiera("przedrostek") Then Prefiks = u.Tekst("przedrostek", "")
            Format = NormalizujFormat(u.Tekst("format", Format))
            Przezroczystosc = u.Logiczna("przezroczystosc", Przezroczystosc)
        End If
    End Sub

    ''' <summary>
    ''' Tylko parametry nowej sesji z lastsettings.txt: przedrostek, format, przezroczystość i numeracja segmentów.
    ''' Nowa sesja wczytywała cały plik, a program zapisuje go dopiero przy zamknięciu - ustawienia zmienione od
    ''' uruchomienia (korekta obrazu, opcje scalania i eksportu do CAD, foldery dawnego nakładania map) wracały do starych wartości.
    ''' </summary>
    Public Sub WczytajParametryNowejSesji(sciezka As String)
        Dim u = PlikUstawien.Wczytaj(sciezka, KodowanieSystemowe())
        Numeracja = u.Tekst("numeracja_", u.Tekst("numeracja", Numeracja))
        If u.Zawiera("przedrostek") Then Prefiks = u.Tekst("przedrostek", "")
        Format = NormalizujFormat(u.Tekst("format", Format))
        Przezroczystosc = u.Logiczna("przezroczystosc", Przezroczystosc)
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
        'wpis zostaje (zawsze wyłączony), żeby starsze wersje programu nie włączały zamiany z dawnej wartości
        u.Ustaw("XYswitched", False)
        u.Ustaw("podklad_mapy", PodkladMapy)
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
        u.Ustaw("siatka_segmentow", SiatkaSegmentowNaMapie)
        u.Ustaw("kursor_i_srodek", KursorISrodekMapy)
        u.Ustaw("kursor_wgs84", KursorWgs84)
        u.Ustaw("zaznaczenie_wgs84", ZaznaczenieWgs84)
        u.Ustaw("edycja_xy", EdycjaXY)
        u.Ustaw("przedrostek", Prefiks)
        u.Ustaw("format", Format)
        u.Ustaw("przezroczystosc", Przezroczystosc)
        u.Ustaw("cad_obrys", CadObrys)
        u.Ustaw("cad_siatka", CadSiatka)
        u.Ustaw("cad_numery", CadNumery)
        u.Ustaw("cad_trasa", CadTrasa)
        u.Ustaw("cad_opis", CadOpis)
        u.Ustaw("cad_dxf", CadDxf)
        u.Ustaw("cad_sciezki_wzgledne", CadSciezkiWzgledne)
        u.Ustaw("cad_scr", CadScr)
        u.Ustaw("cad_lsp", CadLsp)
        u.Ustaw("korekta_wlaczona", KorektaWlaczona)
        u.Ustaw("korekta_profil", CInt(KorektaProfil))
        u.Ustaw("szerokosc_panelu", SzerokoscPanelu)
        u.Ustaw("korekta_gamma", KorektaGamma)
        u.Ustaw("korekta_kontrast", KorektaKontrast)
        u.Ustaw("korekta_nasycenie", KorektaNasycenie)
        u.Ustaw("korekta_wyostrzanie", KorektaWyostrzanie)
        u.Zapisz(sciezka, KodowanieSystemowe())
    End Sub

    ''' <summary>Przywraca ustawienia domyślne (przycisk "Resetuj ustawienia").</summary>
    Public Sub PrzywrocDomyslne()
        Prefiks = "_"
        Format = "jpeg"
        Przezroczystosc = False
        EdycjaXY = False
        KursorWgs84 = False
        ZaznaczenieWgs84 = False
        KursorISrodekMapy = True
        SiatkaSegmentowNaMapie = True
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
        LimitCzasuSekundy = 120
        ZachowajKafleWmts = False
        JakoscJpegWmts = 90
        TrekBuddy = False
        StylNazwyTrekBuddy = 0
        BokSegmentuPrzedTrekBuddy = ""
        PodkladMapy = "osm"
        'układ bieżącej sesji zmienia okno ustawień (Form1.PrzelaczUklad) - z przeliczeniem wpisanego zasięgu
        UkladDomyslny = UkladWspolrzednych.PL1992
        SrodekMapySzerokosc = "52.3"
        SrodekMapyDlugosc = "19.2"
        SkalaMapy = "6"
        CadObrys = True
        CadSiatka = True
        CadNumery = True
        CadTrasa = True
        CadOpis = True
        CadDxf = True
        CadSciezkiWzgledne = True
        CadScr = True
        CadLsp = False
    End Sub

End Class
