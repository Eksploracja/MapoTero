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

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text

''' <summary>
''' Mapa wstawiana do rysunku CAD: rastry (segmenty albo scalony arkusz) z położeniem w układzie współrzędnych
''' oraz elementy wektorowe - obrys zasięgu, siatka i numery segmentów, trasa korytarza, opis mapy.
''' </summary>
Public Class ZadanieCad

    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    ''' <summary>Obrazy wstawiane do rysunku (SciezkaPliku - pełna ścieżka pliku).</summary>
    Public ReadOnly Property Rastry As New List(Of ObrazGeoreferencyjny)
    ''' <summary>Siatka segmentów sesji (obrys, siatka i numery segmentów) albo Nothing.</summary>
    Public Property Siatka As Siatka
    ''' <summary>
    ''' Segmenty korytarza trasy (klucze jak w Korytarz.Segmenty) - siatka rysowana jest wtedy jako prostokąty tych
    ''' segmentów; Nothing - cała prostokątna siatka.
    ''' </summary>
    Public Property SegmentyKorytarza As HashSet(Of Integer)
    Public Property Prefiks As String = "_"
    Public Property Numeracja As StylNumeracji = StylNumeracji.WierszKolumna
    ''' <summary>Linie trasy korytarza w układzie zadania.</summary>
    Public ReadOnly Property Trasa As New List(Of List(Of PunktXY))

    Public Property RysujObrys As Boolean = True
    Public Property RysujSiatke As Boolean = True
    Public Property RysujNumery As Boolean = True
    Public Property RysujTrase As Boolean = True
    ''' <summary>Opis mapy (źródło, data pobrania, układ) wpisywany w rysunku pod obrysem; pusty - bez opisu.</summary>
    Public Property Opis As String = ""
    ''' <summary>
    ''' Obszar zaznaczony na mapie, gdy jest mniejszy od siatki powiększonej do pełnych segmentów - rysowany razem z obrysem
    ''' na osobnej warstwie; Nothing - pokrywa się z siatką (albo obszar wyznacza korytarz trasy).
    ''' </summary>
    Public Property ObszarZaznaczenia As Zasieg?

    ''' <summary>Folder pliku DXF: rastry w nim lub w jego podfolderach zapisywane są ze ścieżką względną; pusty - ścieżki pełne.</summary>
    Public Property FolderRysunku As String = ""
    ''' <summary>Początek nazw definicji obrazów - różny dla różnych sesji, by nazwy obrazów w rysunku się nie powtarzały.</summary>
    Public Property PrzedrostekNazwObrazow As String = "MT"

    ''' <summary>Uwagi zebrane podczas tworzenia plików (np. pominięte numery segmentów).</summary>
    Public ReadOnly Property Uwagi As New List(Of String)

End Class

''' <summary>
''' Eksport mapy do programów CAD:
'''  - rysunek DXF (wersja R2000, AutoCAD 2000 i nowsze, BricsCAD, ZWCAD, GstarCAD, progeCAD i inne) z odnośnikami
'''    do rastrów (encje IMAGE) na właściwych współrzędnych i z warstwami wektorowymi,
'''  - skrypt SCR, który wstawia rastry, obrys, siatkę i trasę do rysunku otwartego w programie CAD (polecenie SCRIPT;
'''    działa także w AutoCAD LT, który nie obsługuje dodatków).
''' Rysunek CAD ma oś X skierowaną na wschód i oś Y na północ - odwrotnie niż geodezyjne X / Y programu,
''' dlatego punkt (X, Y) zapisywany jest jako (Y, X). Jednostka rysunku to jednostka układu (metr, w WGS84 stopień).
''' </summary>
Public NotInheritable Class EksportCad

    Private Sub New()
    End Sub

    Public Const WarstwaRastrow As String = "MAPOTERO_RASTER"
    Public Const WarstwaObrysu As String = "MAPOTERO_ZASIEG"
    Public Const WarstwaSiatki As String = "MAPOTERO_SIATKA"
    Public Const WarstwaNumerow As String = "MAPOTERO_NUMERY"
    Public Const WarstwaTrasy As String = "MAPOTERO_TRASA"
    Public Const WarstwaOpisu As String = "MAPOTERO_OPIS"
    Public Const WarstwaZaznaczenia As String = "MAPOTERO_ZAZNACZENIE"

    ''' <summary>Najwięcej rastrów w jednym rysunku - przy większej liczbie lepiej wstawić scalony arkusz.</summary>
    Public Const MaksRastrow As Integer = 10000
    ''' <summary>Liczba rastrów, powyżej której program CAD może działać wolno.</summary>
    Public Const ZalecanaLiczbaRastrow As Integer = 1000
    ''' <summary>Najwięcej numerów segmentów (tekstów) w rysunku.</summary>
    Public Const MaksNumerow As Integer = 20000
    ''' <summary>Najwięcej prostokątów segmentów korytarza - przy większej liczbie rysowana jest cała siatka liniami.</summary>
    Public Const MaksProstokatow As Integer = 50000

    Private Shared ReadOnly Ci As CultureInfo = CultureInfo.InvariantCulture
    Private Shared ReadOnly Nl As String = (ChrW(13) & ChrW(10))

#Region "Zadanie z sesji pobierania"

    ''' <summary>
    ''' Zadanie dla sesji pobierania: segmenty istniejące w folderze sesji, siatka i korytarz trasy (jeśli jest).
    ''' Wyjątek InvalidDataException, gdy parametry siatki są niepoprawne lub segmentów jest więcej niż MaksRastrow.
    ''' </summary>
    ''' <param name="folder">Folder sesji zakończony separatorem.</param>
    ''' <param name="wstawSegmenty">False - bez rastrów segmentów (np. gdy wstawiany jest scalony arkusz).</param>
    Public Shared Function ZSesji(folder As String, konfiguracja As KonfiguracjaSesji, numeracja As StylNumeracji, wstawSegmenty As Boolean) As ZadanieCad
        Dim siatka = konfiguracja.SiatkaSesji()
        If Not siatka.Poprawna OrElse siatka.ZbytDuza Then Throw New InvalidDataException("Plik conf.txt zawiera niepoprawne parametry segmentów.")
        Dim z As New ZadanieCad With {
            .Uklad = UkladWspolrzednych.ZKodu(konfiguracja.UkladEpsg), .Siatka = siatka,
            .Prefiks = If(konfiguracja.Prefiks, ""), .Numeracja = numeracja,
            .PrzedrostekNazwObrazow = PrzedrostekDlaFolderu(folder)}

        Dim trasaSesji = Korytarz.ZFolderu(folder)
        If trasaSesji IsNot Nothing Then
            z.SegmentyKorytarza = trasaSesji.Segmenty(siatka, z.Uklad)
            For Each odcinek In trasaSesji.Linie
                Dim linia As New List(Of PunktXY)(odcinek.Count)
                For Each p In odcinek
                    linia.Add(z.Uklad.ZWgs84(p))
                Next
                If linia.Count >= 2 Then z.Trasa.Add(linia)
            Next
        Else
            'zaznaczenie z mapy: siatka rośnie w górę i w prawo do pełnych segmentów - dokładny zasięg zaznaczenia osobno
            Dim zs = siatka.ZasiegSiatki
            Dim tolerancja As Double = siatka.RozmiarPiksela / 2
            If zs.XGora - siatka.Obszar.XGora > tolerancja OrElse zs.YPrawy - siatka.Obszar.YPrawy > tolerancja Then
                z.ObszarZaznaczenia = siatka.Obszar
            End If
        End If

        If wstawSegmenty Then
            Dim rozszerzenie As String = Wms.RozszerzeniePliku(konfiguracja.Format)
            'jedno odczytanie listy plików zamiast sprawdzania każdego pola siatki (siatka może mieć miliony segmentów)
            Dim pliki As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            If Directory.Exists(folder) Then
                For Each p In Directory.EnumerateFiles(folder, z.Prefiks & "*." & rozszerzenie)
                    pliki.Add(Path.GetFileName(p))
                Next
            End If
            Dim nieczytelne As New List(Of String)
            For w = 1 To siatka.LiczbaWierszy
                If pliki.Count = 0 Then Exit For
                For k = 1 To siatka.LiczbaKolumn
                    If z.SegmentyKorytarza IsNot Nothing AndAlso Not z.SegmentyKorytarza.Contains(siatka.NumerKolejny(w, k) - 1) Then Continue For
                    Dim nazwa As String = siatka.NazwaSegmentu(z.Prefiks, numeracja, w, k) & "." & rozszerzenie
                    If Not pliki.Contains(nazwa) Then Continue For
                    If z.Rastry.Count >= MaksRastrow Then
                        Throw New InvalidDataException("Sesja zawiera ponad " & MaksRastrow.ToString("N0", CultureInfo.CurrentCulture) &
                                                       " segmentów - takiej liczby obrazów programy CAD nie wyświetlą sprawnie. " &
                                                       "Scal segmenty w jeden arkusz (Narzędzia > Łączenie segmentów) i wstaw arkusz.")
                    End If
                    'wymiary z nagłówka pliku: rysunek CAD musi podać rzeczywistą liczbę pikseli obrazu, a plik uszkodzony
                    '(np. odpowiedź serwera z błędem zapisana jako obraz) zostałby w programie CAD pokazany jako brakujący
                    Dim sciezka As String = folder & nazwa
                    Dim naglowek As NaglowekObrazu
                    Try
                        naglowek = NaglowekObrazu.Odczytaj(sciezka)
                    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException OrElse TypeOf ex Is UnauthorizedAccessException
                        nieczytelne.Add(nazwa)
                        Continue For
                    End Try
                    z.Rastry.Add(New ObrazGeoreferencyjny With {
                        .Uklad = z.Uklad, .Zasieg = siatka.Segment(w, k), .SzerokoscPx = naglowek.SzerokoscPx,
                        .WysokoscPx = naglowek.WysokoscPx, .NazwaPliku = nazwa, .SciezkaPliku = sciezka})
                Next
            Next
            If nieczytelne.Count > 0 Then
                z.Uwagi.Add("Pominięto " & nieczytelne.Count & " uszkodzonych lub nieczytelnych plików segmentów (np. " &
                            String.Join(", ", nieczytelne.Take(3)) & ") - pobierz je ponownie.")
            End If
        End If
        Return z
    End Function

    ''' <summary>
    ''' Kodowanie rysunku DXF i skryptu SCR: strona kodowa ANSI systemu, a gdy system używa innej (np. UTF-8) - 1250.
    ''' Pliki zapisywane są bez znacznika BOM, którego programy CAD nie rozpoznają w pierwszym wierszu pliku.
    ''' </summary>
    Public Shared Function KodowanieRysunku(systemowe As Encoding) As Encoding
        Dim strona As Integer = If(systemowe Is Nothing, 1250, systemowe.CodePage)
        Select Case strona
            Case 874, 932, 936, 949, 950, 1250 To 1258 : Return Encoding.GetEncoding(strona)
            Case Else : Return Encoding.GetEncoding(1250)
        End Select
    End Function

    ''' <summary>Czy tekst (np. ścieżka rastra w skrypcie SCR) zawiera znaki, których nie da się zapisać w kodowaniu.</summary>
    Public Shared Function ZnakiSpozaKodowania(tekst As String, kodowanie As Encoding) As Boolean
        Dim t As String = If(tekst, "")
        Return kodowanie.GetString(kodowanie.GetBytes(t)) <> t
    End Function

    ''' <summary>
    ''' Scalony arkusz (lub inny obraz) z położeniem odczytanym z pliku world file albo z georeferencji GeoTIFF.
    ''' Wyjątek InvalidDataException, gdy położenia nie da się ustalić.
    ''' </summary>
    Public Shared Function Arkusz(sciezka As String, uklad As UkladWspolrzednych, uwagi As List(Of String)) As ObrazGeoreferencyjny
        Dim n = NaglowekObrazu.Odczytaj(sciezka)
        Dim zasieg As Zasieg
        Dim plikWorld As String = WorldFile.ZnajdzPlik(sciezka)
        If plikWorld <> "" Then
            zasieg = WorldFile.Zasieg(File.ReadAllText(plikWorld), n.SzerokoscPx, n.WysokoscPx)
        ElseIf n.MaGeoreferencje Then
            zasieg = n.ZasiegGeoTiff()
        Else
            Throw New InvalidDataException("Obok obrazu nie ma pliku world file (np. ." & Georeferencja.RozszerzenieWorldFile(Path.GetExtension(sciezka)) &
                                           "), a plik nie zawiera georeferencji GeoTIFF. Scal segmenty z zaznaczoną opcją pliku world file albo do GeoTIFF.")
        End If
        If uwagi IsNot Nothing Then
            If n.Epsg > 0 AndAlso n.Epsg <> uklad.Epsg Then
                uwagi.Add("Uwaga: arkusz GeoTIFF zapisany jest w układzie EPSG:" & n.Epsg.ToString(Ci) & ", a sesja w układzie " & uklad.Nazwa &
                          " - arkusz nie pokryje obrysu i siatki sesji. Scal segmenty tej sesji albo wybierz folder sesji, z której pochodzi arkusz.")
            End If
            If n.BigTiff Then uwagi.Add("Arkusz zapisany jest w formacie BigTIFF, którego programy CAD zwykle nie otwierają - scal segmenty do mniejszego arkusza lub do JPEG.")
            Dim rozszerzenie As String = Path.GetExtension(sciezka).ToLowerInvariant()
            If rozszerzenie = ".tif" OrElse rozszerzenie = ".tiff" Then
                uwagi.Add("Jeśli program CAD nie wyświetli arkusza TIFF (kompresja Deflate lub JPEG w TIFF), scal segmenty do arkusza JPEG.")
            End If
        End If
        Return New ObrazGeoreferencyjny With {.Uklad = uklad, .Zasieg = zasieg, .SzerokoscPx = n.SzerokoscPx, .WysokoscPx = n.WysokoscPx,
                                              .NazwaPliku = Path.GetFileName(sciezka), .SciezkaPliku = sciezka}
    End Function

    ''' <summary>
    ''' Przedrostek nazw obrazów dla folderu sesji: nazwa folderu i skrót jego ścieżki - te same segmenty (np. _01_01)
    ''' z różnych sesji mają w rysunku różne nazwy, a ponowne wstawienie tej samej sesji używa tych samych nazw.
    ''' </summary>
    Public Shared Function PrzedrostekDlaFolderu(folder As String) As String
        Dim f As String = If(folder, "").TrimEnd("\"c, "/"c)
        Dim nazwa As String = NazwaObiektu(Path.GetFileName(f))
        If nazwa.Length > 30 Then nazwa = nazwa.Substring(0, 30)
        'FNV-1a ze ścieżki (bez rozróżniania wielkości liter) - stały między uruchomieniami programu
        Dim h As UInteger = 2166136261UI
        For Each c In f.ToLowerInvariant()
            h = h Xor CUInt(AscW(c) And &HFFFF)
            h = CUInt((CULng(h) * 16777619UL) And &HFFFFFFFFUL)
        Next
        Return "MT_" & If(nazwa <> "", nazwa & "_", "") & (h And &HFFFFFFUI).ToString("x6", Ci)
    End Function

    ''' <summary>
    ''' Nazwa obiektu rysunku (definicji obrazu): litery bez polskich znaków, cyfry, podkreślenie i minus -
    ''' dozwolone we wszystkich programach CAD.
    ''' </summary>
    Public Shared Function NazwaObiektu(tekst As String) As String
        Dim sb As New StringBuilder()
        For Each c In If(tekst, "").Replace("ł"c, "l"c).Replace("Ł"c, "L"c).Normalize(NormalizationForm.FormD)
            If CharUnicodeInfo.GetUnicodeCategory(c) = UnicodeCategory.NonSpacingMark Then Continue For
            If (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "a"c AndAlso c <= "z"c) OrElse (c >= "0"c AndAlso c <= "9"c) OrElse c = "-"c OrElse c = "_"c Then
                sb.Append(c)
            Else
                sb.Append("_"c)
            End If
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Nazwy definicji obrazów zadania (unikalne w rysunku, w kolejności rastrów).</summary>
    Public Shared Function NazwyObrazow(z As ZadanieCad) As List(Of String)
        Dim wynik As New List(Of String)
        Dim uzyte As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each r In z.Rastry
            Dim baza As String = z.PrzedrostekNazwObrazow & "_" & NazwaObiektu(Path.GetFileNameWithoutExtension(r.NazwaPliku))
            If baza.Length > 200 Then baza = baza.Substring(0, 200)
            Dim nazwa As String = baza
            Dim i As Integer = 2
            While Not uzyte.Add(nazwa)
                nazwa = baza & "_" & i.ToString(Ci)
                i += 1
            End While
            wynik.Add(nazwa)
        Next
        Return wynik
    End Function

#End Region

#Region "Geometria wspólna dla DXF i SCR"

    ''' <summary>Zasięg rysunku: obrys siatki i rastry (X - północ, Y - wschód).</summary>
    Private Shared Function ZasiegRysunku(z As ZadanieCad) As Zasieg
        Dim xMin = Double.MaxValue, yMin = Double.MaxValue, xMax = Double.MinValue, yMax = Double.MinValue
        Dim dodaj = Sub(q As Zasieg)
                        xMin = Math.Min(xMin, q.XDol) : yMin = Math.Min(yMin, q.YLewy)
                        xMax = Math.Max(xMax, q.XGora) : yMax = Math.Max(yMax, q.YPrawy)
                    End Sub
        If z.Siatka IsNot Nothing AndAlso z.Siatka.Poprawna Then dodaj(z.Siatka.ZasiegSiatki)
        For Each r In z.Rastry
            dodaj(r.Zasieg)
        Next
        If xMin > xMax Then Return New Zasieg(0, 0, 1, 1)
        Return New Zasieg(xMin, yMin, xMax, yMax)
    End Function

    ''' <summary>Czy siatka rysowana jest jako prostokąty segmentów korytarza (a nie liniami całej siatki).</summary>
    Private Shared Function SiatkaKorytarza(z As ZadanieCad) As Boolean
        Return z.SegmentyKorytarza IsNot Nothing AndAlso z.SegmentyKorytarza.Count <= MaksProstokatow
    End Function

    ''' <summary>
    ''' Linie całej siatki: pionowe (granice kolumn) i poziome (granice wierszy) - pary punktów. Bez krawędzi zewnętrznych,
    ''' gdy rysowany jest obrys: pokrywałyby się z nim, a programy CAD różnie rozstrzygają, która z dwóch linii jest na wierzchu.
    ''' </summary>
    Private Shared Iterator Function LinieSiatki(s As Siatka, bezKrawedzi As Boolean) As IEnumerable(Of PunktXY())
        Dim zs = s.ZasiegSiatki
        Dim od As Integer = If(bezKrawedzi, 1, 0)
        For k = od To s.LiczbaKolumn - od
            Dim y As Double = s.Obszar.YLewy + k * s.BokSegmentu
            Yield {New PunktXY(zs.XDol, y), New PunktXY(zs.XGora, y)}
        Next
        For w = od To s.LiczbaWierszy - od
            Dim x As Double = s.Obszar.XDol + w * s.BokSegmentu
            Yield {New PunktXY(x, zs.YLewy), New PunktXY(x, zs.YPrawy)}
        Next
    End Function

    ''' <summary>Segmenty siatki (wiersz, kolumna) - wszystkie albo tylko segmenty korytarza.</summary>
    Private Shared Iterator Function SegmentySiatki(z As ZadanieCad) As IEnumerable(Of Tuple(Of Integer, Integer))
        Dim s = z.Siatka
        For w = 1 To s.LiczbaWierszy
            For k = 1 To s.LiczbaKolumn
                If z.SegmentyKorytarza IsNot Nothing AndAlso Not z.SegmentyKorytarza.Contains(s.NumerKolejny(w, k) - 1) Then Continue For
                Yield Tuple.Create(w, k)
            Next
        Next
    End Function

    Private Shared Function LiczbaSegmentowSiatki(z As ZadanieCad) As Long
        If z.SegmentyKorytarza IsNot Nothing Then Return z.SegmentyKorytarza.Count
        Return z.Siatka.LiczbaSegmentow
    End Function

    Private Shared Function MaSiatke(z As ZadanieCad) As Boolean
        Return z.Siatka IsNot Nothing AndAlso z.Siatka.Poprawna AndAlso Not z.Siatka.ZbytDuza
    End Function

    ''' <summary>Liczba z kropką dziesiętną (do 10 miejsc po przecinku).</summary>
    Private Shared Function L(v As Double) As String
        Return Georeferencja.Liczba(v)
    End Function

    ''' <summary>Punkt rysunku CAD "wschód,północ".</summary>
    Private Shared Function PunktCad(p As PunktXY) As String
        Return L(p.Y) & "," & L(p.X)
    End Function

    Private Shared Function Przezroczysty(r As ObrazGeoreferencyjny) As Boolean
        Dim e As String = Path.GetExtension(r.NazwaPliku).ToLowerInvariant()
        Return e = ".png" OrElse e = ".gif" OrElse e = ".tif" OrElse e = ".tiff"
    End Function

#End Region

#Region "DXF"

    ''' <summary>
    ''' Treść rysunku DXF R2000 (AC1015). Teksty i ścieżki zapisywane są w stronie kodowej <paramref name="kodowanie"/>
    ''' (zmienna $DWGCODEPAGE; strona ANSI, np. 1250), a znaki spoza niej - jako \U+XXXX; plik trzeba zapisać w tym samym
    ''' kodowaniu. Dla kodowania innego niż ANSI (np. UTF-8) wszystkie znaki spoza ASCII zapisywane są jako \U+XXXX.
    ''' </summary>
    Public Shared Function Dxf(z As ZadanieCad, kodowanie As Encoding) As String
        Dim d As New PisarzDxf(kodowanie)
        Dim nazwy = NazwyObrazow(z)
        Dim zr = ZasiegRysunku(z)
        Dim metry As Boolean = Not z.Uklad.Geograficzny

        'stałe uchwyty tablic i słowników; pozostałe obiekty dostają kolejne numery od 0x20
        Const hBlockRecord As String = "1", hLayer As String = "2", hStyle As String = "3", hLtype As String = "5", hView As String = "6",
              hUcs As String = "7", hVport As String = "8", hAppid As String = "9", hDimstyle As String = "A", hSlownik As String = "C",
              hGrupy As String = "D"
        Dim hModel = d.Uchwyt(), hPapier = d.Uchwyt()
        Dim hStyleStandard = d.Uchwyt()
        Dim hSlownikObrazow = d.Uchwyt(), hZmienneRastrow = d.Uchwyt()

        Dim tresc As New PisarzDxf(kodowanie, d)

        '--- TABLES
        tresc.Sekcja("TABLES")
        'VPORT - widok rysunku na całą mapę
        tresc.Tablica("VPORT", hVport, 1)
        Dim wysokoscWidoku As Double = Math.Max(zr.Wysokosc, zr.Szerokosc / 1.6) * 1.1
        tresc.K(0, "VPORT").K(5, tresc.Uchwyt()).K(330, hVport).K(100, "AcDbSymbolTableRecord").K(100, "AcDbViewportTableRecord")
        tresc.K(2, "*ACTIVE").K(70, "0").K(10, "0").K(20, "0").K(11, "1").K(21, "1")
        tresc.K(12, L(zr.Srodek.Y)).K(22, L(zr.Srodek.X)).K(13, "0").K(23, "0").K(14, "1").K(24, "1").K(15, "1").K(25, "1")
        tresc.K(16, "0").K(26, "0").K(36, "1").K(17, "0").K(27, "0").K(37, "0")
        tresc.K(40, L(If(wysokoscWidoku > 0, wysokoscWidoku, 1))).K(41, "1.6").K(42, "50").K(43, "0").K(44, "0").K(50, "0").K(51, "0")
        tresc.K(71, "0").K(72, "100").K(73, "1").K(74, "3").K(75, "0").K(76, "0").K(77, "0").K(78, "0")
        tresc.K(281, "0").K(65, "1").K(110, "0").K(120, "0").K(130, "0").K(111, "1").K(121, "0").K(131, "0")
        tresc.K(112, "0").K(122, "1").K(132, "0").K(79, "0").K(146, "0")
        tresc.K(0, "ENDTAB")

        tresc.Tablica("LTYPE", hLtype, 3)
        For Each lt In {"ByBlock", "ByLayer", "Continuous"}
            tresc.K(0, "LTYPE").K(5, tresc.Uchwyt()).K(330, hLtype).K(100, "AcDbSymbolTableRecord").K(100, "AcDbLinetypeTableRecord")
            tresc.K(2, lt).K(70, "0").K(3, If(lt = "Continuous", "Solid line", "")).K(72, "65").K(73, "0").K(40, "0")
        Next
        tresc.K(0, "ENDTAB")

        Dim warstwy As New List(Of Tuple(Of String, Integer)) From {
            Tuple.Create("0", 7), Tuple.Create(WarstwaRastrow, 7), Tuple.Create(WarstwaObrysu, 1), Tuple.Create(WarstwaSiatki, 8),
            Tuple.Create(WarstwaNumerow, 8), Tuple.Create(WarstwaTrasy, 6), Tuple.Create(WarstwaOpisu, 7),
            Tuple.Create(WarstwaZaznaczenia, 3)}
        tresc.Tablica("LAYER", hLayer, warstwy.Count)
        For Each w In warstwy
            tresc.K(0, "LAYER").K(5, tresc.Uchwyt()).K(330, hLayer).K(100, "AcDbSymbolTableRecord").K(100, "AcDbLayerTableRecord")
            tresc.K(2, w.Item1).K(70, "0").K(62, w.Item2.ToString(Ci)).K(6, "Continuous")
        Next
        tresc.K(0, "ENDTAB")

        tresc.Tablica("STYLE", hStyle, 1)
        tresc.K(0, "STYLE").K(5, hStyleStandard).K(330, hStyle).K(100, "AcDbSymbolTableRecord").K(100, "AcDbTextStyleTableRecord")
        tresc.K(2, "Standard").K(70, "0").K(40, "0").K(41, "1").K(50, "0").K(71, "0").K(42, "2.5").K(3, "arial.ttf").K(4, "")
        tresc.K(0, "ENDTAB")

        tresc.Tablica("VIEW", hView, 0)
        tresc.K(0, "ENDTAB")
        tresc.Tablica("UCS", hUcs, 0)
        tresc.K(0, "ENDTAB")

        tresc.Tablica("APPID", hAppid, 1)
        tresc.K(0, "APPID").K(5, tresc.Uchwyt()).K(330, hAppid).K(100, "AcDbSymbolTableRecord").K(100, "AcDbRegAppTableRecord")
        tresc.K(2, "ACAD").K(70, "0")
        tresc.K(0, "ENDTAB")

        Dim hDimStandard = tresc.Uchwyt()
        tresc.K(0, "TABLE").K(2, "DIMSTYLE").K(5, hDimstyle).K(330, "0").K(100, "AcDbSymbolTable").K(70, "1")
        tresc.K(100, "AcDbDimStyleTable").K(71, "1").K(340, hDimStandard)
        tresc.K(0, "DIMSTYLE").K(105, hDimStandard).K(330, hDimstyle).K(100, "AcDbSymbolTableRecord").K(100, "AcDbDimStyleTableRecord")
        tresc.K(2, "Standard").K(70, "0").K(40, "1").K(41, "2.5").K(42, "0.625").K(43, "3.75").K(44, "1.25").K(45, "0").K(46, "0")
        tresc.K(47, "0").K(48, "0").K(140, "2.5").K(141, "2.5").K(142, "0").K(143, "0.03937007874").K(144, "1").K(145, "0").K(146, "1")
        tresc.K(147, "0.625").K(71, "0").K(72, "0").K(73, "0").K(74, "0").K(75, "0").K(76, "0").K(77, "1").K(78, "8")
        tresc.K(170, "0").K(171, "3").K(172, "1").K(173, "0").K(174, "0").K(175, "0").K(176, "0").K(177, "0").K(178, "0")
        tresc.K(270, "2").K(271, "2").K(272, "2").K(273, "2").K(274, "3").K(275, "0").K(276, "0").K(277, "2").K(278, "44")
        tresc.K(279, "0").K(280, "0").K(281, "0").K(282, "0").K(283, "0").K(284, "8").K(285, "0").K(286, "0").K(288, "0")
        tresc.K(340, hStyleStandard).K(371, "-2").K(372, "-2")
        tresc.K(0, "ENDTAB")

        tresc.Tablica("BLOCK_RECORD", hBlockRecord, 2)
        tresc.K(0, "BLOCK_RECORD").K(5, hModel).K(330, hBlockRecord).K(100, "AcDbSymbolTableRecord").K(100, "AcDbBlockTableRecord")
        tresc.K(2, "*Model_Space")
        tresc.K(0, "BLOCK_RECORD").K(5, hPapier).K(330, hBlockRecord).K(100, "AcDbSymbolTableRecord").K(100, "AcDbBlockTableRecord")
        tresc.K(2, "*Paper_Space")
        tresc.K(0, "ENDTAB")
        tresc.K(0, "ENDSEC")

        '--- BLOCKS
        tresc.Sekcja("BLOCKS")
        For Each b In {Tuple.Create("*Model_Space", hModel), Tuple.Create("*Paper_Space", hPapier)}
            tresc.K(0, "BLOCK").K(5, tresc.Uchwyt()).K(330, b.Item2).K(100, "AcDbEntity")
            If b.Item1 = "*Paper_Space" Then tresc.K(67, "1")
            tresc.K(8, "0").K(100, "AcDbBlockBegin")
            tresc.K(2, b.Item1).K(70, "0").K(10, "0").K(20, "0").K(30, "0").K(3, b.Item1).K(1, "")
            tresc.K(0, "ENDBLK").K(5, tresc.Uchwyt()).K(330, b.Item2).K(100, "AcDbEntity").K(8, "0").K(100, "AcDbBlockEnd")
        Next
        tresc.K(0, "ENDSEC")

        '--- ENTITIES: najpierw rastry (pod spodem), potem elementy wektorowe
        tresc.Sekcja("ENTITIES")
        Dim definicje As New List(Of Tuple(Of String, String, String, ObrazGeoreferencyjny, String))   'nazwa, uchwyt definicji, uchwyt reaktora, raster, uchwyt obrazu
        For i = 0 To z.Rastry.Count - 1
            Dim r = z.Rastry(i)
            Dim hObraz = tresc.Uchwyt(), hDef = tresc.Uchwyt(), hReaktor = tresc.Uchwyt()
            definicje.Add(Tuple.Create(nazwy(i), hDef, hReaktor, r, hObraz))
            Dim flagi As Integer = 1 Or 2 Or 4 Or If(Przezroczysty(r), 8, 0)
            tresc.K(0, "IMAGE").K(5, hObraz).K(330, hModel).K(100, "AcDbEntity").K(8, WarstwaRastrow).K(100, "AcDbRasterImage").K(90, "0")
            'punkt wstawienia - lewy dolny narożnik obrazu; wektory U i V - bok piksela w prawo i w górę
            tresc.K(10, L(r.Zasieg.YLewy)).K(20, L(r.Zasieg.XDol)).K(30, "0")
            tresc.K(11, L(r.PikselY)).K(21, "0").K(31, "0")
            tresc.K(12, "0").K(22, L(r.PikselX)).K(32, "0")
            tresc.K(13, r.SzerokoscPx.ToString(Ci)).K(23, r.WysokoscPx.ToString(Ci))
            tresc.K(340, hDef).K(70, flagi.ToString(Ci)).K(280, "0").K(281, "50").K(282, "50").K(283, "0").K(360, hReaktor)
            tresc.K(71, "1").K(91, "2").K(14, "-0.5").K(24, "-0.5").K(14, L(r.SzerokoscPx - 0.5)).K(24, L(r.WysokoscPx - 0.5))
        Next

        If MaSiatke(z) Then
            Dim s = z.Siatka
            If z.RysujSiatke Then
                If SiatkaKorytarza(z) Then
                    For Each wk In SegmentySiatki(z)
                        Dim q = s.Segment(wk.Item1, wk.Item2)
                        tresc.Polilinia(hModel, WarstwaSiatki, {q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny}, True)
                    Next
                Else
                    For Each odc In LinieSiatki(s, z.RysujObrys)
                        tresc.K(0, "LINE").K(5, tresc.Uchwyt()).K(330, hModel).K(100, "AcDbEntity").K(8, WarstwaSiatki).K(100, "AcDbLine")
                        tresc.K(10, L(odc(0).Y)).K(20, L(odc(0).X)).K(30, "0").K(11, L(odc(1).Y)).K(21, L(odc(1).X)).K(31, "0")
                    Next
                End If
            End If
            If z.RysujNumery Then
                If LiczbaSegmentowSiatki(z) > MaksNumerow Then
                    z.Uwagi.Add("Pominięto numery segmentów - segmentów jest więcej niż " & MaksNumerow.ToString("N0", CultureInfo.CurrentCulture) & ".")
                Else
                    Dim wysokosc As Double = s.BokSegmentu / 8
                    For Each wk In SegmentySiatki(z)
                        Dim c = s.Segment(wk.Item1, wk.Item2).Srodek
                        tresc.Tekst(hModel, WarstwaNumerow, c, wysokosc, s.NazwaSegmentu(z.Prefiks, z.Numeracja, wk.Item1, wk.Item2), True)
                    Next
                End If
            End If
            If z.RysujObrys Then
                Dim q = s.ZasiegSiatki
                tresc.Polilinia(hModel, WarstwaObrysu, {q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny}, True)
                If z.ObszarZaznaczenia.HasValue Then
                    Dim o = z.ObszarZaznaczenia.Value
                    tresc.Polilinia(hModel, WarstwaZaznaczenia, {o.LewyDolny, o.PrawyDolny, o.PrawyGorny, o.LewyGorny}, True)
                End If
            End If
        End If
        If z.RysujTrase Then
            For Each linia In z.Trasa
                tresc.Polilinia(hModel, WarstwaTrasy, linia, False)
            Next
        End If
        If If(z.Opis, "").Trim() <> "" Then
            'opis pod lewym dolnym narożnikiem mapy
            Dim wys As Double = Math.Max(zr.Szerokosc, zr.Wysokosc) / 150
            Dim wiersze = z.Opis.Replace(vbCr, "").Split(ChrW(10))
            For i = 0 To wiersze.Length - 1
                tresc.Tekst(hModel, WarstwaOpisu, New PunktXY(zr.XDol - wys * 1.8 * (i + 1), zr.YLewy), wys, wiersze(i), False)
            Next
        End If
        tresc.K(0, "ENDSEC")

        '--- OBJECTS
        tresc.Sekcja("OBJECTS")
        tresc.K(0, "DICTIONARY").K(5, hSlownik).K(330, "0").K(100, "AcDbDictionary").K(281, "1")
        tresc.K(3, "ACAD_GROUP").K(350, hGrupy)
        tresc.K(3, "ACAD_IMAGE_DICT").K(350, hSlownikObrazow)
        tresc.K(3, "ACAD_IMAGE_VARS").K(350, hZmienneRastrow)
        tresc.K(0, "DICTIONARY").K(5, hGrupy).K(102, "{ACAD_REACTORS").K(330, hSlownik).K(102, "}").K(330, hSlownik)
        tresc.K(100, "AcDbDictionary").K(281, "1")
        tresc.K(0, "DICTIONARY").K(5, hSlownikObrazow).K(102, "{ACAD_REACTORS").K(330, hSlownik).K(102, "}").K(330, hSlownik)
        tresc.K(100, "AcDbDictionary").K(281, "1")
        For Each df In definicje
            tresc.K(3, df.Item1).K(350, df.Item2)
        Next
        'ramki obrazów wyłączone (siatka segmentów jest na osobnej warstwie); jednostki obrazów - metry
        tresc.K(0, "RASTERVARIABLES").K(5, hZmienneRastrow).K(102, "{ACAD_REACTORS").K(330, hSlownik).K(102, "}").K(330, hSlownik)
        tresc.K(100, "AcDbRasterVariables").K(90, "0").K(70, "0").K(71, "1").K(72, If(metry, "3", "0"))
        For Each df In definicje
            Dim r = df.Item4
            tresc.K(0, "IMAGEDEF").K(5, df.Item2).K(102, "{ACAD_REACTORS").K(330, hSlownikObrazow).K(330, df.Item3).K(102, "}")
            tresc.K(330, hSlownikObrazow).K(100, "AcDbRasterImageDef").K(90, "0").K(1, SciezkaWRysunku(z, r.SciezkaPliku), True)
            tresc.K(10, r.SzerokoscPx.ToString(Ci)).K(20, r.WysokoscPx.ToString(Ci)).K(11, "1").K(21, "1").K(280, "1").K(281, "0")
        Next
        For Each df In definicje
            tresc.K(0, "IMAGEDEF_REACTOR").K(5, df.Item3).K(330, df.Item5).K(100, "AcDbRasterImageDefReactor").K(90, "2").K(330, df.Item5)
        Next
        tresc.K(0, "ENDSEC")

        '--- HEADER i CLASSES (na końcu - $HANDSEED musi być większy od wszystkich uchwytów)
        d.Sekcja("HEADER")
        d.K(9, "$ACADVER").K(1, "AC1015")
        d.K(9, "$DWGCODEPAGE").K(3, d.StronaKodowa)
        d.K(9, "$INSBASE").K(10, "0").K(20, "0").K(30, "0")
        d.K(9, "$EXTMIN").K(10, L(zr.YLewy)).K(20, L(zr.XDol)).K(30, "0")
        d.K(9, "$EXTMAX").K(10, L(zr.YPrawy)).K(20, L(zr.XGora)).K(30, "0")
        d.K(9, "$LUNITS").K(70, "2")
        d.K(9, "$LUPREC").K(70, If(metry, "3", "8"))
        d.K(9, "$MEASUREMENT").K(70, "1")
        d.K(9, "$INSUNITS").K(70, If(metry, "6", "0"))
        d.K(9, "$HANDSEED").K(5, d.Uchwyt())
        d.K(0, "ENDSEC")
        d.Sekcja("CLASSES")
        d.Klasa("RASTERVARIABLES", "AcDbRasterVariables", 0, False)
        d.Klasa("IMAGEDEF", "AcDbRasterImageDef", 0, False)
        d.Klasa("IMAGEDEF_REACTOR", "AcDbRasterImageDefReactor", 1, False)
        d.Klasa("IMAGE", "AcDbRasterImage", 2175, True)
        d.K(0, "ENDSEC")

        d.Dopisz(tresc)
        d.K(0, "EOF")
        Return d.ToString()
    End Function

    ''' <summary>Ścieżka rastra zapisywana w rysunku: względna (.\nazwa), gdy raster leży w folderze rysunku, inaczej pełna.</summary>
    Private Shared Function SciezkaWRysunku(z As ZadanieCad, sciezka As String) As String
        Dim folder As String = If(z.FolderRysunku, "")
        If folder = "" Then Return sciezka
        If Not folder.EndsWith("\") AndAlso Not folder.EndsWith("/") Then folder &= "\"
        If sciezka.StartsWith(folder, StringComparison.OrdinalIgnoreCase) Then Return ".\" & sciezka.Substring(folder.Length)
        Return sciezka
    End Function

    ''' <summary>Zapis kodów grupowych DXF z kolejnymi uchwytami obiektów.</summary>
    Private Class PisarzDxf

        Private ReadOnly _sb As New StringBuilder()
        'kodowanie strony ANSI rysunku; Nothing - wszystkie znaki spoza ASCII zapisywane jako \U+XXXX
        Private ReadOnly _koder As Encoding
        Private ReadOnly _licznik As LicznikUchwytow

        Private Class LicznikUchwytow
            Public Nastepny As Long = &H20
        End Class

        Public Sub New(kodowanie As Encoding)
            If StronaAnsi(kodowanie.CodePage) Then
                _koder = Encoding.GetEncoding(kodowanie.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback)
            End If
            _licznik = New LicznikUchwytow()
        End Sub

        ''' <summary>Pisarz dzielący licznik uchwytów z innym (treść zapisywana przed nagłówkiem).</summary>
        Public Sub New(kodowanie As Encoding, wspolny As PisarzDxf)
            _koder = wspolny._koder
            _licznik = wspolny._licznik
        End Sub

        Public Function Uchwyt() As String
            Dim h As Long = _licznik.Nastepny
            _licznik.Nastepny += 1
            Return h.ToString("X", Ci)
        End Function

        ''' <summary>Nazwa strony kodowej rysunku ($DWGCODEPAGE).</summary>
        Public ReadOnly Property StronaKodowa As String
            Get
                Return If(_koder IsNot Nothing, "ANSI_" & _koder.CodePage.ToString(Ci), "ANSI_1252")
            End Get
        End Property

        Private Shared Function StronaAnsi(strona As Integer) As Boolean
            Select Case strona
                Case 874, 932, 936, 949, 950, 1250 To 1258 : Return True
                Case Else : Return False
            End Select
        End Function

        Public Function K(kod As Integer, wartosc As String) As PisarzDxf
            _sb.Append(kod.ToString(Ci).PadLeft(3)).Append(Nl).Append(wartosc).Append(Nl)
            Return Me
        End Function

        Public Sub Sekcja(nazwa As String)
            K(0, "SECTION").K(2, nazwa)
        End Sub

        Public Sub Tablica(nazwa As String, uchwyt As String, liczba As Integer)
            K(0, "TABLE").K(2, nazwa).K(5, uchwyt).K(330, "0").K(100, "AcDbSymbolTable").K(70, liczba.ToString(Ci))
        End Sub

        Public Sub Klasa(nazwa As String, klasa As String, flagi As Integer, encja As Boolean)
            K(0, "CLASS").K(1, nazwa).K(2, klasa).K(3, "ISM").K(90, flagi.ToString(Ci)).K(280, "0").K(281, If(encja, "1", "0"))
        End Sub

        Public Sub Polilinia(wlasciciel As String, warstwa As String, punkty As IEnumerable(Of PunktXY), zamknieta As Boolean)
            Dim lista = punkty.ToList()
            K(0, "LWPOLYLINE").K(5, Uchwyt()).K(330, wlasciciel).K(100, "AcDbEntity").K(8, warstwa).K(100, "AcDbPolyline")
            K(90, lista.Count.ToString(Ci)).K(70, If(zamknieta, "1", "0")).K(43, "0")
            For Each p In lista
                K(10, L(p.Y)).K(20, L(p.X))
            Next
        End Sub

        ''' <summary>Tekst; srodek - wyrównanie do środka (w poziomie i w pionie), inaczej do lewego dolnego narożnika.</summary>
        Public Sub Tekst(wlasciciel As String, warstwa As String, p As PunktXY, wysokosc As Double, tekst As String, srodek As Boolean)
            K(0, "TEXT").K(5, Uchwyt()).K(330, wlasciciel).K(100, "AcDbEntity").K(8, warstwa).K(100, "AcDbText")
            K(10, L(p.Y)).K(20, L(p.X)).K(30, "0").K(40, L(wysokosc)).K(1, TekstDxf(tekst))
            If srodek Then K(72, "1").K(11, L(p.Y)).K(21, L(p.X)).K(31, "0")
            K(100, "AcDbText")
            If srodek Then K(73, "2")
        End Sub

        ''' <summary>
        ''' Tekst w stronie kodowej rysunku: znaki spoza niej jako \U+XXXX, znak ^ jako "^ " (^ rozpoczyna w DXF znak sterujący),
        ''' znaki sterujące - spacja.
        ''' </summary>
        Public Function TekstDxf(tekst As String) As String
            Dim sb As New StringBuilder()
            Dim jeden(0) As Char
            For Each c In If(tekst, "")
                If c = "^"c Then
                    sb.Append("^ ")
                ElseIf Char.IsControl(c) Then
                    sb.Append(" "c)
                ElseIf AscW(c) < 128 Then
                    sb.Append(c)
                Else
                    jeden(0) = c
                    Dim kodowalny As Boolean = False
                    If _koder IsNot Nothing AndAlso Not Char.IsSurrogate(c) Then
                        Try
                            _koder.GetBytes(jeden)
                            kodowalny = True
                        Catch ex As EncoderFallbackException
                        End Try
                    End If
                    sb.Append(If(kodowalny, c.ToString(), "\U+" & (AscW(c) And &HFFFF).ToString("X4", Ci)))
                End If
            Next
            Return sb.ToString()
        End Function

        Public Function K(kod As Integer, wartosc As String, tekst As Boolean) As PisarzDxf
            Return K(kod, If(tekst, TekstDxf(wartosc), wartosc))
        End Function

        Public Sub Dopisz(inny As PisarzDxf)
            _sb.Append(inny._sb)
        End Sub

        Public Overrides Function ToString() As String
            Return _sb.ToString()
        End Function

    End Class

#End Region

#Region "Skrypt SCR"

    ''' <summary>
    ''' Skrypt SCR dla AutoCAD, AutoCAD LT, BricsCAD i ZWCAD (polecenie SCRIPT w otwartym rysunku): wstawia rastry
    ''' poleceniem -IMAGE oraz rysuje obrys, siatkę segmentów i trasę na osobnych warstwach. Numery segmentów i opis
    ''' mapy zawiera tylko rysunek DXF (polecenie tekstu ma w różnych programach różne pytania).
    ''' Polecenia i opcje mają przedrostek "_" (wersje językowe programów), a punkty - "_non" (bez przyciągania
    ''' do obiektów). Skrypt zapisuje się w kodowaniu systemu (ANSI).
    ''' </summary>
    Public Shared Function Skrypt(z As ZadanieCad) As String
        Dim sb As New StringBuilder()
        Dim w = Sub(tekst As String) sb.Append(tekst).Append(Nl)
        Dim metry As Boolean = Not z.Uklad.Geograficzny
        Dim zr = ZasiegRysunku(z)
        Dim nazwy = NazwyObrazow(z)

        w("; MapoTero - wstawienie mapy do rysunku CAD (AutoCAD, AutoCAD LT, BricsCAD, ZWCAD)")
        w("; Uruchomienie: polecenie SCRIPT (_SCRIPT) w otwartym rysunku i wskazanie tego pliku.")
        w("; Uklad wspolrzednych: " & BezPolskichZnakow(z.Uklad.Nazwa) & If(metry, ", jednostka rysunku: metr.", ", jednostka rysunku: stopien."))
        w("; Os X rysunku - wschod (Y geodezyjne), os Y rysunku - polnoc (X geodezyjne).")
        w("; Na czas wstawiania rastrow INSUNITS = 0 (skala obrazu = jego szerokosc w jednostkach rysunku),")
        w("; na koniec INSUNITS = " & If(metry, "6 (metry)", "0") & " i FILEDIA = 1. Wspolrzedne podawane sa w ukladzie WCS")
        w("; (na czas skryptu UCS = World, potem poprzedni). Rastry trafiaja pod istniejace obiekty rysunku (DRAWORDER),")
        w("; obiekty na warstwy MAPOTERO_..., a po zakonczeniu biezaca jest warstwa 0.")
        w("; Calosc mozna cofnac jednym poleceniem U (cofnij).")
        w("_UNDO _BEGIN")
        w("_SETVAR FILEDIA 0")
        w("_SETVAR INSUNITS 0")
        'punkty i kąt obrotu obrazów liczone są w bieżącym układzie UCS - przy obróconym UCS mapa trafiłaby w złe miejsce
        w("_UCS _WORLD")

        If z.Rastry.Count > 0 Then
            'polecenie -IMAGE przyjmuje tylko szerokość obrazu - wysokość wynika z liczby pikseli, czyli piksel kwadratowy
            Dim niekwadratowe = z.Rastry.Where(Function(r) Math.Abs(r.PikselX - r.PikselY) > 0.000001 * Math.Max(r.PikselX, r.PikselY)).ToList()
            If niekwadratowe.Count > 0 Then
                z.Uwagi.Add("Raster " & niekwadratowe(0).NazwaPliku & " ma piksele niekwadratowe (" & L(niekwadratowe(0).PikselY) & " x " &
                            L(niekwadratowe(0).PikselX) & ") - skrypt SCR wstawi go z niewłaściwą wysokością; użyj rysunku DXF.")
            End If
            Warstwa(sb, WarstwaRastrow, 7)
            For i = 0 To z.Rastry.Count - 1
                Dim r = z.Rastry(i)
                w("_-IMAGE")
                w("_ATTACH")
                w(nazwy(i) & "=""" & r.SciezkaPliku & """")
                w("_non")
                w(PunktCad(r.Zasieg.LewyDolny))
                w(L(r.Zasieg.Szerokosc))
                'kąt obrotu wskazany punktem na wschód od punktu wstawienia (prawy dolny narożnik): liczba "0" oznacza
                'kierunek ANGBASE, a przy ANGBASE = 90 (azymuty od północy) raster zostałby obrócony o 90°
                w("_non")
                w(PunktCad(r.Zasieg.PrawyDolny))
                'tło nakładek PNG / GIF (np. działki pobrane z przezroczystym tłem) bez tego zakrywa podkład
                If Przezroczysty(r) Then
                    w("_TRANSPARENCY")
                    w("_LAST")
                    w("")
                    w("_ON")
                End If
                'nowe obiekty rysowane są na wierzchu - raster przesłoniłby rysunek, do którego jest wstawiany
                w("_DRAWORDER")
                w("_LAST")
                w("")
                w("_BACK")
            Next
        End If

        If MaSiatke(z) Then
            Dim s = z.Siatka
            If z.RysujSiatke Then
                Warstwa(sb, WarstwaSiatki, 8)
                If SiatkaKorytarza(z) Then
                    For Each wk In SegmentySiatki(z)
                        Dim q = s.Segment(wk.Item1, wk.Item2)
                        PoliliniaScr(sb, {q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny}, True)
                    Next
                Else
                    For Each odc In LinieSiatki(s, z.RysujObrys)
                        w("_LINE")
                        w("_non") : w(PunktCad(odc(0)))
                        w("_non") : w(PunktCad(odc(1)))
                        w("")
                    Next
                End If
            End If
            If z.RysujObrys Then
                Warstwa(sb, WarstwaObrysu, 1)
                Dim q = s.ZasiegSiatki
                PoliliniaScr(sb, {q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny}, True)
                If z.ObszarZaznaczenia.HasValue Then
                    Warstwa(sb, WarstwaZaznaczenia, 3)
                    Dim o = z.ObszarZaznaczenia.Value
                    PoliliniaScr(sb, {o.LewyDolny, o.PrawyDolny, o.PrawyGorny, o.LewyGorny}, True)
                End If
            End If
        End If
        If z.RysujTrase AndAlso z.Trasa.Count > 0 Then
            Warstwa(sb, WarstwaTrasy, 6)
            For Each linia In z.Trasa
                PoliliniaScr(sb, linia, False)
            Next
        End If

        w("_-LAYER")
        w("_SET")
        w("0")
        w("")
        w("_SETVAR INSUNITS " & If(metry, "6", "0"))
        w("_SETVAR FILEDIA 1")
        w("_UCS _PREVIOUS")
        w("_ZOOM")
        w("_WINDOW")
        w("_non") : w(L(zr.YLewy) & "," & L(zr.XDol))
        w("_non") : w(L(zr.YPrawy) & "," & L(zr.XGora))
        w("_UNDO _END")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Tworzy warstwę o podanym kolorze i ustawia ją jako bieżącą; warstwę już istniejącą (np. z poprzedniego wstawienia)
    ''' odmraża i włącza - zamrożonej warstwy nie da się ustawić jako bieżącej, a obiekty na wyłączonej byłyby niewidoczne.
    ''' </summary>
    Private Shared Sub Warstwa(sb As StringBuilder, nazwa As String, kolor As Integer)
        sb.Append("_-LAYER").Append(Nl)
        sb.Append("_NEW").Append(Nl).Append(nazwa).Append(Nl)
        sb.Append("_THAW").Append(Nl).Append(nazwa).Append(Nl)
        sb.Append("_ON").Append(Nl).Append(nazwa).Append(Nl)
        'na zablokowanej warstwie DRAWORDER i TRANSPARENCY nie zmienią wstawionego rastra
        sb.Append("_UNLOCK").Append(Nl).Append(nazwa).Append(Nl)
        sb.Append("_SET").Append(Nl).Append(nazwa).Append(Nl)
        sb.Append("_COLOR").Append(Nl).Append(kolor.ToString(Ci)).Append(Nl).Append(nazwa).Append(Nl)
        sb.Append(Nl)
    End Sub

    ''' <summary>Polilinia o zerowej szerokości (niezależnie od zmiennej PLINEWID).</summary>
    Private Shared Sub PoliliniaScr(sb As StringBuilder, punkty As IEnumerable(Of PunktXY), zamknieta As Boolean)
        Dim pierwszy As Boolean = True
        sb.Append("_PLINE").Append(Nl)
        For Each p In punkty
            sb.Append("_non").Append(Nl).Append(PunktCad(p)).Append(Nl)
            If pierwszy Then
                sb.Append("_WIDTH").Append(Nl).Append("0").Append(Nl).Append("0").Append(Nl)
                pierwszy = False
            End If
        Next
        sb.Append(If(zamknieta, "_CLOSE", "")).Append(Nl)
    End Sub

    Private Shared Function BezPolskichZnakow(tekst As String) As String
        Dim sb As New StringBuilder()
        For Each c In If(tekst, "").Replace("ł"c, "l"c).Replace("Ł"c, "L"c).Normalize(NormalizationForm.FormD)
            If CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark Then sb.Append(c)
        Next
        Return sb.ToString()
    End Function

#End Region

#Region "Program LISP"

    ''' <summary>
    ''' Program AutoLISP (.lsp) dla AutoCAD, AutoCAD LT 2024 i nowszych, BricsCAD i ZWCAD - wczytany poleceniem APPLOAD
    ''' od razu wstawia mapę do otwartego rysunku (ponownie: polecenie MAPOTERO_WSTAW). W odróżnieniu od skryptu SCR:
    '''  - zapamiętuje i przywraca bieżącą warstwę, INSUNITS, FILEDIA i CMDECHO (także po przerwaniu klawiszem Esc),
    '''  - nie zmienia układu UCS - punkty przeliczane są z WCS funkcją trans,
    '''  - linie, polilinie i teksty tworzy funkcją entmake (bez pytań poleceń), więc wstawia także numery segmentów i opis,
    '''  - pomija rastry, których plików nie ma na dysku (skrypt SCR zatrzymałby się na pytaniu o plik).
    ''' Teksty spoza ASCII zapisywane są jako \U+XXXX (kody tekstu CAD), ścieżki - w kodowaniu pliku (ANSI).
    ''' </summary>
    Public Shared Function Lisp(z As ZadanieCad) As String
        Dim sb As New StringBuilder()
        Dim w = Sub(tekst As String) sb.Append(tekst).Append(Nl)
        Dim metry As Boolean = Not z.Uklad.Geograficzny
        Dim zr = ZasiegRysunku(z)
        Dim nazwy = NazwyObrazow(z)

        w(";;; MapoTero - wstawienie mapy do rysunku CAD (AutoCAD, AutoCAD LT 2024 i nowsze, BricsCAD, ZWCAD)")
        w(";;; Wczytanie: polecenie APPLOAD albo przeciagniecie pliku do okna rysunku - mapa jest wstawiana od razu.")
        w(";;; Ponowne wstawienie po wczytaniu: polecenie MAPOTERO_WSTAW.")
        w(";;; Uklad wspolrzednych: " & BezPolskichZnakow(z.Uklad.Nazwa) & If(metry, ", jednostka rysunku: metr.", ", jednostka rysunku: stopien."))
        w(";;; Os X rysunku - wschod (Y geodezyjne), os Y - polnoc (X geodezyjne). Wspolrzedne w ukladzie WCS.")
        w(";;; Biezaca warstwa, INSUNITS, FILEDIA i CMDECHO sa po zakonczeniu przywracane. Calosc cofa polecenie U.")
        w("")

        'dane mapy
        w("(setq mt:rastry '(")
        For i = 0 To z.Rastry.Count - 1
            Dim r = z.Rastry(i)
            w(" (" & TekstLisp(nazwy(i), False) & " " & TekstLisp(r.SciezkaPliku, False) & " " & Rz(r.Zasieg.YLewy) & " " & Rz(r.Zasieg.XDol) & " " &
              Rz(r.Zasieg.Szerokosc) & If(Przezroczysty(r), " T", " nil") & ")")
        Next
        w("))")

        Dim linie As New List(Of PunktXY())
        Dim prostokaty As New List(Of PunktXY())
        Dim numery As New List(Of Tuple(Of PunktXY, String))
        Dim wysokoscNumerow As Double = 0
        If MaSiatke(z) Then
            Dim s = z.Siatka
            If z.RysujSiatke Then
                If SiatkaKorytarza(z) Then
                    For Each wk In SegmentySiatki(z)
                        Dim q = s.Segment(wk.Item1, wk.Item2)
                        prostokaty.Add({q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny})
                    Next
                Else
                    linie.AddRange(LinieSiatki(s, z.RysujObrys))
                End If
            End If
            If z.RysujNumery Then
                If LiczbaSegmentowSiatki(z) > MaksNumerow Then
                    If Not z.Uwagi.Any(Function(u) u.StartsWith("Pominięto numery")) Then
                        z.Uwagi.Add("Pominięto numery segmentów - segmentów jest więcej niż " & MaksNumerow.ToString("N0", CultureInfo.CurrentCulture) & ".")
                    End If
                Else
                    wysokoscNumerow = s.BokSegmentu / 8
                    For Each wk In SegmentySiatki(z)
                        numery.Add(Tuple.Create(s.Segment(wk.Item1, wk.Item2).Srodek, s.NazwaSegmentu(z.Prefiks, z.Numeracja, wk.Item1, wk.Item2)))
                    Next
                End If
            End If
        End If
        w("(setq mt:siatka '(")
        For Each odc In linie
            w(" ((" & Rz(odc(0).Y) & " " & Rz(odc(0).X) & " 0.0) (" & Rz(odc(1).Y) & " " & Rz(odc(1).X) & " 0.0))")
        Next
        w("))")
        w("(setq mt:prostokaty '(")
        For Each p In prostokaty
            w(" " & PunktyLisp(p))
        Next
        w("))")
        w("(setq mt:numery '(")
        For Each n In numery
            w(" ((" & Rz(n.Item1.Y) & " " & Rz(n.Item1.X) & " 0.0) " & TekstLisp(n.Item2, True) & ")")
        Next
        w("))")
        w("(setq mt:wysokosc-numerow " & Rz(wysokoscNumerow) & ")")

        'obrysy: zasięg siatki i obszar zaznaczenia
        w("(setq mt:obrysy '(")
        If MaSiatke(z) AndAlso z.RysujObrys Then
            Dim q = z.Siatka.ZasiegSiatki
            w(" (" & TekstLisp(WarstwaObrysu, False) & " 1 " & PunktyLisp({q.LewyDolny, q.PrawyDolny, q.PrawyGorny, q.LewyGorny}) & ")")
            If z.ObszarZaznaczenia.HasValue Then
                Dim o = z.ObszarZaznaczenia.Value
                w(" (" & TekstLisp(WarstwaZaznaczenia, False) & " 3 " & PunktyLisp({o.LewyDolny, o.PrawyDolny, o.PrawyGorny, o.LewyGorny}) & ")")
            End If
        End If
        w("))")
        w("(setq mt:trasa '(")
        If z.RysujTrase Then
            For Each linia In z.Trasa
                w(" " & PunktyLisp(linia))
            Next
        End If
        w("))")

        'opis pod lewym dolnym narożnikiem mapy (jak w rysunku DXF)
        w("(setq mt:opis '(")
        Dim wysOpisu As Double = Math.Max(zr.Szerokosc, zr.Wysokosc) / 150
        If If(z.Opis, "").Trim() <> "" Then
            Dim wiersze = z.Opis.Replace(vbCr, "").Split(ChrW(10))
            For i = 0 To wiersze.Length - 1
                w(" ((" & Rz(zr.YLewy) & " " & Rz(zr.XDol - wysOpisu * 1.8 * (i + 1)) & " 0.0) " & TekstLisp(wiersze(i), True) & ")")
            Next
        End If
        w("))")
        w("(setq mt:wysokosc-opisu " & Rz(wysOpisu) & ")")
        w("(setq mt:okno '((" & Rz(zr.YLewy) & " " & Rz(zr.XDol) & " 0.0) (" & Rz(zr.YPrawy) & " " & Rz(zr.XGora) & " 0.0)))")
        w("")

        'stała część programu
        For Each wiersz In ProgramLisp.Split({vbLf}, StringSplitOptions.None)
            w(wiersz.TrimEnd(ChrW(13)))
        Next
        w("(c:MAPOTERO_WSTAW)")

        'ścieżki ze znakami spoza ASCII zależą od kodowania odczytu pliku .lsp (w AutoCAD 2021+ - zmienna LISPSYS)
        If z.Rastry.Any(Function(r) r.SciezkaPliku.Any(Function(c) AscW(c) > 127)) Then
            z.Uwagi.Add("Ścieżka rastrów zawiera znaki spoza ASCII (np. polskie litery). Jeśli program LISP nie znajdzie plików " &
                        "(AutoCAD 2021 i nowsze odczytują pliki .lsp zależnie od zmiennej LISPSYS), przenieś folder z mapą albo użyj rysunku DXF.")
        End If
        Return sb.ToString()
    End Function

    ''' <summary>Liczba rzeczywista AutoLISP (zawsze z kropką - liczba całkowita przekroczyłaby zakres w obliczeniach).</summary>
    Private Shared Function Rz(v As Double) As String
        Dim t As String = L(v)
        If t.IndexOf("."c) < 0 AndAlso t.IndexOf("E"c) < 0 Then t &= ".0"
        Return t
    End Function

    Private Shared Function PunktyLisp(punkty As IEnumerable(Of PunktXY)) As String
        Dim sb As New StringBuilder("(")
        For Each p In punkty
            If sb.Length > 1 Then sb.Append(" ")
            sb.Append("(").Append(Rz(p.Y)).Append(" ").Append(Rz(p.X)).Append(")")
        Next
        Return sb.Append(")").ToString()
    End Function

    ''' <summary>
    ''' Tekst AutoLISP w cudzysłowie: \ i " poprzedzone znakiem \. Teksty rysunku (kody) - znaki spoza ASCII jako \U+XXXX,
    ''' które programy CAD wyświetlają jako właściwe litery niezależnie od kodowania pliku .lsp.
    ''' </summary>
    Private Shared Function TekstLisp(tekst As String, kodyTekstu As Boolean) As String
        Dim sb As New StringBuilder("""")
        For Each c In If(tekst, "")
            If c = "\"c Then
                sb.Append("\\")
            ElseIf c = """"c Then
                sb.Append("\""")
            ElseIf Char.IsControl(c) Then
                sb.Append(" "c)
            ElseIf kodyTekstu AndAlso AscW(c) > 127 Then
                sb.Append("\\U+").Append((AscW(c) And &HFFFF).ToString("X4", Ci))
            Else
                sb.Append(c)
            End If
        Next
        Return sb.Append("""").ToString()
    End Function

    ''' <summary>Funkcje programu LISP - korzystają z danych mt:... zapisanych przed nimi.</summary>
    Private Const ProgramLisp As String =
";; punkt WCS w biezacym ukladzie UCS (polecenia przyjmuja punkty w UCS)
(defun mt:ucs (p) (trans p 0 1))

;; warstwa: utworzona albo odmrozona, wlaczona i odblokowana
(defun mt:warstwa (nazwa kolor / e)
  (if (setq e (tblobjname ""LAYER"" nazwa))
    (progn
      (setq e (entget e))
      (setq e (subst (cons 70 (logand (cdr (assoc 70 e)) (~ 5))) (assoc 70 e) e))
      (setq e (subst (cons 62 (abs (cdr (assoc 62 e)))) (assoc 62 e) e))
      (entmod e))
    (entmake (list '(0 . ""LAYER"") '(100 . ""AcDbSymbolTableRecord"") '(100 . ""AcDbLayerTableRecord"")
                   (cons 2 nazwa) '(70 . 0) (cons 62 kolor) '(6 . ""Continuous""))))
  nazwa)

(defun mt:polilinia (warstwa punkty zamknieta)
  (entmake (append (list '(0 . ""LWPOLYLINE"") '(100 . ""AcDbEntity"") (cons 8 warstwa) '(100 . ""AcDbPolyline"")
                         (cons 90 (length punkty)) (cons 70 (if zamknieta 1 0)))
                   (mapcar '(lambda (p) (cons 10 p)) punkty))))

(defun mt:tekst (warstwa p wys tekst srodek)
  (entmake (append (list '(0 . ""TEXT"") (cons 8 warstwa) '(7 . ""Standard"") (cons 10 p) (cons 40 wys) (cons 1 tekst))
                   (if srodek (list '(72 . 1) (cons 11 p) '(73 . 2))))))

;; raster: punkt wstawienia - lewy dolny naroznik, skala - szerokosc (przy INSUNITS = 0), obrot wskazany punktem
;; na wschod (niezaleznie od ANGBASE / ANGDIR); potem przezroczystosc i przesuniecie pod istniejace obiekty
(defun mt:obraz (nazwa plik x y szer przezr / ostatni)
  (if (findfile plik)
    (progn
      (setq ostatni (entlast))
      (command ""_.-IMAGE"" ""_Attach"" (strcat nazwa ""=\"""" plik ""\"""")
               ""_non"" (mt:ucs (list x y 0.0)) szer ""_non"" (mt:ucs (list (+ x szer) y 0.0)))
      (if (not (equal ostatni (entlast)))
        (progn
          (if przezr (command ""_.TRANSPARENCY"" (entlast) """" ""_ON""))
          (command ""_.DRAWORDER"" (entlast) """" ""_Back"")
          (setq mt:wstawione (1+ mt:wstawione)))
        (setq mt:brak (1+ mt:brak))))
    (setq mt:brak (1+ mt:brak))))

(defun mt:przywroc ()
  (foreach z mt:zapisane (setvar (car z) (cdr z)))
  (setq *error* mt:stary-error))

(defun mt:blad (msg)
  (if (and msg (not (wcmatch (strcase msg) ""*CANCEL*,*QUIT*,*EXIT*,*ANULOW*,*PRZERW*"")))
    (princ (strcat ""\nMapoTero - blad: "" msg)))
  (if command-s (command-s ""_.UNDO"" ""_End""))
  (mt:przywroc)
  (princ))

(defun c:MAPOTERO_WSTAW ()
  (setq mt:zapisane (mapcar '(lambda (v) (cons v (getvar v))) '(""CLAYER"" ""INSUNITS"" ""FILEDIA"" ""CMDECHO"")))
  (setq mt:stary-error *error* *error* mt:blad)
  (setq mt:wstawione 0 mt:brak 0)
  (setvar ""CMDECHO"" 0)
  (command ""_.UNDO"" ""_BEgin"")
  (setvar ""FILEDIA"" 0)
  (setvar ""INSUNITS"" 0)
  (if mt:rastry
    (progn
      (setvar ""CLAYER"" (mt:warstwa ""MAPOTERO_RASTER"" 7))
      (foreach r mt:rastry (apply 'mt:obraz r))))
  (if (or mt:siatka mt:prostokaty)
    (progn
      (mt:warstwa ""MAPOTERO_SIATKA"" 8)
      (foreach l mt:siatka
        (entmake (list '(0 . ""LINE"") '(8 . ""MAPOTERO_SIATKA"") (cons 10 (car l)) (cons 11 (cadr l)))))
      (foreach p mt:prostokaty (mt:polilinia ""MAPOTERO_SIATKA"" p T))))
  (if mt:numery
    (progn
      (mt:warstwa ""MAPOTERO_NUMERY"" 8)
      (foreach n mt:numery (mt:tekst ""MAPOTERO_NUMERY"" (car n) mt:wysokosc-numerow (cadr n) T))))
  (foreach o mt:obrysy
    (mt:warstwa (car o) (cadr o))
    (mt:polilinia (car o) (caddr o) T))
  (if mt:trasa
    (progn
      (mt:warstwa ""MAPOTERO_TRASA"" 6)
      (foreach t1 mt:trasa (mt:polilinia ""MAPOTERO_TRASA"" t1 nil))))
  (if mt:opis
    (progn
      (mt:warstwa ""MAPOTERO_OPIS"" 7)
      (foreach o mt:opis (mt:tekst ""MAPOTERO_OPIS"" (car o) mt:wysokosc-opisu (cadr o) nil))))
  (command ""_.UNDO"" ""_End"")
  (mt:przywroc)
  (command ""_.ZOOM"" ""_Window"" (mt:ucs (car mt:okno)) (mt:ucs (cadr mt:okno)))
  (princ (strcat ""\nMapoTero: wstawiono rastrow: "" (itoa mt:wstawione)
                 (if (> mt:brak 0) (strcat "", pominieto (brak pliku): "" (itoa mt:brak)) """")))
  (princ))
"

#End Region

End Class
