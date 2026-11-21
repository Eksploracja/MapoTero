'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq

''' <summary>Rodzaj eksportu mapy (format pliku docelowego).</summary>
Public Enum RodzajEksportu
    ''' <summary>JNX dla odbiorników Garmin (BirdsEye): wielopoziomowy raster bez limitu 100 kafli.</summary>
    Jnx
    ''' <summary>KMZ dla odbiorników Garmin (Custom Maps): kafle JPEG do 1024 px, limit liczby kafli.</summary>
    KmzGarmin
    ''' <summary>KMZ bez limitu liczby kafli (Locus Map, OruxMaps, Google Earth).</summary>
    Kmz
    ''' <summary>MBTiles: kafle Web Mercator (Locus Map, OsmAnd, OruxMaps i inne).</summary>
    MBTiles
    ''' <summary>SQLiteDB (format RMaps): kafle Web Mercator dla OsmAnd i Locus Map.</summary>
    SQLiteDB
    ''' <summary>Mapa offline OruxMaps: folder z bazą OruxMapsImages.db i kalibracją .otrk2.xml.</summary>
    OruxMaps
    ''' <summary>GeoTIFF z georeferencją (Avenza Maps, QField, GIS, CAD).</summary>
    GeoTiff
    ''' <summary>Mapa TrekBuddy / Locus Map: archiwum .tar z kaflami 512 px arkusza, plikiem .map i listą .set.</summary>
    TrekBuddy
End Enum

''' <summary>Główne przeznaczenie (urządzenie docelowe) mapy w kreatorze.</summary>
Public Enum CelMapy
    ''' <summary>Aplikacje mobilne na smartfony i tablety (Locus Map, OsmAnd, OruxMaps).</summary>
    TelefonTablet
    ''' <summary>Turystyczne odbiorniki i zegarki Garmin (GPSMAP, eTrex, Fenix itp.).</summary>
    OdbiornikGarmin
    ''' <summary>Arkusz graficzny do druku, plakatu lub przeglądania na komputerze.</summary>
    WydrukKomputer
    ''' <summary>Podkład do programów projektowych CAD (AutoCAD, MicroStation) i systemów GIS (QGIS).</summary>
    ProjektCadGis
End Enum

''' <summary>Definicja pojedynczej warstwy wchodzącej w skład gotowego zestawu (szablonu).</summary>
Public Class WarstwaSzablonu
    ''' <summary>Nazwa pliku zbioru map z folderu warstwy (np. TOPO_WMTS, ortofotomapa_WMTS).</summary>
    Public Property Zbior As String = ""
    ''' <summary>Nazwa warstwy na serwerze (np. MAPA TOPOGRAFICZNA, ISOK_Cien, hiking).</summary>
    Public Property Warstwa As String = ""
    ''' <summary>Krycie warstwy w procentach (0-100).</summary>
    Public Property Krycie As Integer = 100
    ''' <summary>Tryb mieszania pikseli z podkładem.</summary>
    Public Property TrybMieszania As TrybMieszania = TrybMieszania.Zwykly
    ''' <summary>Czy warstwa jest traktowana jako przezroczysta nakładka.</summary>
    Public Property Nakladka As Boolean = False
    ''' <summary>Opcjonalna czytelna etykieta warstwy wyświetlana w kreatorze.</summary>
    Public Property NazwaWyswietlana As String = ""

    Public Sub New()
    End Sub

    Public Sub New(zbior As String, warstwa As String, krycie As Integer,
                   Optional nakladka As Boolean = False,
                   Optional trybMieszania As TrybMieszania = TrybMieszania.Zwykly,
                   Optional nazwaWyswietlana As String = "")
        Me.Zbior = zbior
        Me.Warstwa = warstwa
        Me.Krycie = Math.Max(0, Math.Min(100, krycie))
        Me.Nakladka = nakladka
        Me.TrybMieszania = trybMieszania
        Me.NazwaWyswietlana = nazwaWyswietlana
    End Sub
End Class

''' <summary>
''' Stopień skali mapy wieloskalowej: warstwy pobierane z jednym rozmiarem piksela (np. mapa 1:10 000 przy 2 m,
''' 1:50 000 przy 10 m). W pliku MBTiles każdy stopień wypełnia swoje poziomy powiększenia, więc przy oddalaniu
''' mapa przełącza się na mapę w mniejszej skali zamiast pokazywać pomniejszoną, nieczytelną mapę szczegółową.
''' </summary>
Public Class StopienSkali
    ''' <summary>Opis stopnia dla użytkownika (np. „1:50 000”).</summary>
    Public Property Opis As String = ""
    ''' <summary>Rozmiar piksela, przy którym serwer podaje mapę tego stopnia.</summary>
    Public Property Piksel As Double = 1.0
    ''' <summary>Warstwy stopnia (od podkładu w górę).</summary>
    Public ReadOnly Property Warstwy As New List(Of WarstwaSzablonu)()

    Public Overrides Function ToString() As String
        Return Opis
    End Function
End Class

''' <summary>Stopień skali z pikselem i zakresem poziomów powiększenia wyliczonymi dla konkretnej mapy.</summary>
Public Class PlanStopnia
    Public Property Stopien As StopienSkali
    Public Property Piksel As Double
    Public Property PoziomMin As Integer
    Public Property PoziomMax As Integer

    Public Overrides Function ToString() As String
        Return $"{Stopien?.Opis} ({Piksel:0.##} m, zoom {PoziomMin}-{PoziomMax})"
    End Function
End Class

''' <summary>Predefiniowany zestaw mapowy (preset) dla konkretnego zastosowania.</summary>
Public Class SzablonMapy
    Public Property Id As String = ""
    Public Property Nazwa As String = ""
    Public Property Kategoria As String = ""
    Public Property Opis As String = ""
    Public Property Piktogram As String = ""
    ''' <summary>Sugerowany rozmiar piksela w metrach - dobrany do rozdzielczości warstw zestawu (bez rozmywania i zbędnego powiększania pliku).</summary>
    Public Property DomyslnyPiksel As Double = 1.0
    ''' <summary>Profil korekty obrazu proponowany dla tego zestawu (poza odbiornikami Garmin, które zawsze dostają profil MIP).</summary>
    Public Property ProfilKorekty As ProfilKorektyRastra = ProfilKorektyRastra.Brak
    ''' <summary>Szerokość geograficzna reprezentatywnego punktu próbki podglądu (miejsce, w którym zestaw dobrze wygląda).</summary>
    Public Property ProbkaLat As Double = 52.0
    ''' <summary>Długość geograficzna reprezentatywnego punktu próbki podglądu.</summary>
    Public Property ProbkaLon As Double = 19.0
    ''' <summary>Rozmiar piksela dla próbki podglądu.</summary>
    Public Property ProbkaPiksel As Double = 1.0
    ''' <summary>Warstwy składające się na tę mozaikę mapową (od podkładu w górę); w mapie wieloskalowej - najdokładniejszy stopień.</summary>
    Public ReadOnly Property Warstwy As New List(Of WarstwaSzablonu)()
    ''' <summary>Stopnie skali od najdokładniejszego (pusta lista - mapa jednoskalowa z warstw <see cref="Warstwy"/>).</summary>
    Public ReadOnly Property StopnieSkali As New List(Of StopienSkali)()
    ''' <summary>Obsługiwane przeznaczenia (cele) dla tego szablonu.</summary>
    Public ReadOnly Property DozwoloneCele As New List(Of CelMapy)()

    Public Sub New()
    End Sub

    Public Sub New(id As String, nazwa As String, kategoria As String, opis As String, piktogram As String,
                   domyslnyPiksel As Double, profilKorekty As ProfilKorektyRastra, ParamArray cele() As CelMapy)
        Me.Id = id
        Me.Nazwa = nazwa
        Me.Kategoria = kategoria
        Me.Opis = opis
        Me.Piktogram = piktogram
        Me.DomyslnyPiksel = domyslnyPiksel
        Me.ProbkaPiksel = domyslnyPiksel
        Me.ProfilKorekty = profilKorekty
        If cele IsNot Nothing Then Me.DozwoloneCele.AddRange(cele)
    End Sub

    ''' <summary>Czytelna reprezentacja szablonu (używana w kontrolkach i podglądzie).</summary>
    Public Overrides Function ToString() As String
        Return If(Not String.IsNullOrEmpty(Piktogram), Piktogram & " ", "") & Nazwa
    End Function

    ''' <summary>Czy dany szablon pasuje do wskazanego celu.</summary>
    Public Function PasujeDoCelu(cel As CelMapy) As Boolean
        Return DozwoloneCele.Count = 0 OrElse DozwoloneCele.Contains(cel)
    End Function

    ''' <summary>Czy szablon ma kilka stopni skali (mapa wieloskalowa).</summary>
    Public ReadOnly Property Wieloskalowy As Boolean
        Get
            Return StopnieSkali.Count > 1
        End Get
    End Property

    ''' <summary>Dodaje stopień skali; pierwszy dodany (najdokładniejszy) wypełnia też <see cref="Warstwy"/>.</summary>
    Public Function DodajStopien(opis As String, piksel As Double, ParamArray warstwy() As WarstwaSzablonu) As StopienSkali
        Dim st As New StopienSkali With {.Opis = opis, .Piksel = piksel}
        st.Warstwy.AddRange(warstwy)
        If StopnieSkali.Count = 0 Then
            Me.Warstwy.Clear()
            Me.Warstwy.AddRange(warstwy)
        End If
        StopnieSkali.Add(st)
        Return st
    End Function

    ''' <summary>Stopień skali najbliższy (w skali logarytmicznej) podanemu pikselowi; Nothing dla mapy jednoskalowej.</summary>
    Public Function StopienDlaPiksela(piksel As Double) As StopienSkali
        If StopnieSkali.Count = 0 Then Return Nothing
        If piksel <= 0 Then Return StopnieSkali(0)
        Return StopnieSkali.OrderBy(Function(st) Math.Abs(Math.Log(piksel / st.Piksel))).First()
    End Function

    ''' <summary>Warstwy do pobrania przy danym pikselu: w mapie wieloskalowej - stopnia najbliższego pikselowi.</summary>
    Public Function WarstwyDlaPiksela(piksel As Double) As IList(Of WarstwaSzablonu)
        Dim st = StopienDlaPiksela(piksel)
        Return If(st Is Nothing, CType(Warstwy, IList(Of WarstwaSzablonu)), st.Warstwy)
    End Function

    ''' <summary>Tworzy i zwraca nowy zestaw warstw dla tego szablonu (piksel > 0 - warstwy stopnia skali najbliższego pikselowi).</summary>
    Public Function UtworzZestaw(zrodlaMap As IDictionary(Of String, PlikWarstw), Optional piksel As Double = 0) As ZestawWarstw
        Dim zestaw As New ZestawWarstw()
        ZarzadcaSzablonow.ZastosujWarstwy(Me, WarstwyDlaPiksela(piksel), zestaw, zrodlaMap)
        Return zestaw
    End Function

    ''' <summary>Zestaw warstw wskazanego stopnia skali.</summary>
    Public Function UtworzZestaw(zrodlaMap As IDictionary(Of String, PlikWarstw), stopien As StopienSkali) As ZestawWarstw
        Dim zestaw As New ZestawWarstw()
        ZarzadcaSzablonow.ZastosujWarstwy(Me, If(stopien IsNot Nothing, stopien.Warstwy, CType(Warstwy, IList(Of WarstwaSzablonu))), zestaw, zrodlaMap)
        Return zestaw
    End Function

    ''' <summary>Opis stopni skali (np. „1:10 000 → 1:50 000 → 1:100 000”); pusty dla mapy jednoskalowej.</summary>
    Public Function OpisSkal() As String
        If Not Wieloskalowy Then Return ""
        Return String.Join(" → ", StopnieSkali.Select(Function(st) st.Opis))
    End Function

    ''' <summary>Zwięzłe podsumowanie składników szablonu dla interfejsu użytkownika.</summary>
    Public Function OpisSkladnikow() As String
        Dim pozycje As New List(Of String)()
        For Each w In Warstwy
            Dim etykieta = If(Not String.IsNullOrWhiteSpace(w.NazwaWyswietlana), w.NazwaWyswietlana, w.Warstwa)
            If w.Krycie < 100 Then
                pozycje.Add(etykieta & " (" & w.Krycie & "%)")
            Else
                pozycje.Add(etykieta)
            End If
        Next
        Return String.Join(" + ", pozycje)
    End Function
End Class

''' <summary>Wynik kalkulacji i szacunków dla wskazanego obszaru i parametrów w kreatorze.</summary>
Public Class SzacunekKreatora
    ''' <summary>Liczba segmentów bazowych do pobrania.</summary>
    Public Property LiczbaSegmentow As Long = 0
    ''' <summary>Liczba wszystkich segmentów pełnego prostokąta siatki (przed przycięciem do korytarza).</summary>
    Public Property LiczbaSegmentowPelnych As Long = 0
    ''' <summary>Liczba kolumn siatki segmentów.</summary>
    Public Property SiatkaKolumny As Integer = 0
    ''' <summary>Liczba wierszy siatki segmentów.</summary>
    Public Property SiatkaWiersze As Integer = 0
    ''' <summary>Szerokość mapy w pikselach.</summary>
    Public Property SzerokoscPx As Long = 0
    ''' <summary>Wysokość mapy w pikselach.</summary>
    Public Property WysokoscPx As Long = 0
    ''' <summary>Czy kalkulacja dotyczy korytarza wzdłuż trasy.</summary>
    Public Property CzyKorytarz As Boolean = False
    ''' <summary>Oszczędność liczby segmentów i danych dzięki korytarzowi (w procentach 0-100%).</summary>
    Public Property OszczednoscProcent As Integer = 0

    ''' <summary>Dolna granica szacowanego rozmiaru pliku wynikowego w MB.</summary>
    Public Property SzacowanyRozmiarMbMin As Double = 0
    ''' <summary>Górna granica szacowanego rozmiaru pliku wynikowego w MB.</summary>
    Public Property SzacowanyRozmiarMbMax As Double = 0
    ''' <summary>Średni przybliżony rozmiar pobranych danych i pliku na dysku w MB.</summary>
    Public Property SzacowanyRozmiarMb As Double = 0
    ''' <summary>Przewidywany czas operacji (pobieranie + kompilacja) w sekundach przy stabilnym łączu.</summary>
    Public Property SzacowanyCzasSekundy As Integer = 0
    ''' <summary>Ostrzeżenie tekstowe dla użytkownika, gdy obszar jest zbyt duży.</summary>
    Public Property Ostrzezenie As String = ""
    ''' <summary>0 = bezpieczny (zielony), 1 = umiarkowany (żółty), 2 = bardzo duży (czerwony).</summary>
    Public Property PoziomOstrzezenia As Integer = 0

    ''' <summary>Czy przed generowaniem trzeba zapytać użytkownika o zgodę (duży obszar albo przekroczony limit formatu).</summary>
    Public ReadOnly Property WymagaPotwierdzenia As Boolean
        Get
            Return PoziomOstrzezenia >= 1
        End Get
    End Property

    ''' <summary>Czytelny opis czasu pobierania (np. 15 s, 2 min 30 s, 1 godz. 5 min).</summary>
    Public Function SformatowanyCzas() As String
        If SzacowanyCzasSekundy < 60 Then
            Return SzacowanyCzasSekundy & " s"
        ElseIf SzacowanyCzasSekundy < 3600 Then
            Dim min = SzacowanyCzasSekundy \ 60
            Dim sek = SzacowanyCzasSekundy Mod 60
            Return If(sek > 0, min & " min " & sek & " s", min & " min")
        Else
            Dim godz = SzacowanyCzasSekundy \ 3600
            Dim min = (SzacowanyCzasSekundy Mod 3600) \ 60
            Return If(min > 0, godz & " godz. " & min & " min", godz & " godz.")
        End If
    End Function

    ''' <summary>Czytelny opis rozmiaru (np. 14,5 MB, 1,2 GB) - z separatorem dziesiętnym ustawień regionalnych, jak przedział.</summary>
    Public Function SformatowanyRozmiar() As String
        If SzacowanyRozmiarMb >= 1024 Then
            Return (SzacowanyRozmiarMb / 1024.0).ToString("0.0", CultureInfo.CurrentCulture) & " GB"
        Else
            Return SzacowanyRozmiarMb.ToString("0.0", CultureInfo.CurrentCulture) & " MB"
        End If
    End Function

    ''' <summary>Czytelny przedział rozmiaru pliku (np. ~15–25 MB, ~1,2–1,8 GB, ~350–600 KB).</summary>
    Public Function SformatowanyPrzedzialRozmiaru() As String
        If SzacowanyRozmiarMbMax <= 0 Then Return "~0 MB"
        If SzacowanyRozmiarMbMax < 1.0 Then
            Dim minKb = Math.Max(1, CInt(Math.Round(SzacowanyRozmiarMbMin * 1024)))
            Dim maxKb = Math.Max(minKb, CInt(Math.Round(SzacowanyRozmiarMbMax * 1024)))
            Return If(minKb = maxKb, $"~{minKb} KB", $"~{minKb}–{maxKb} KB")
        ElseIf SzacowanyRozmiarMbMax >= 1024.0 Then
            'dawniej kropka (InvariantCulture), a pojedyncza wartość z przecinkiem - niespójne napisy w jednym oknie
            Dim minGb = (SzacowanyRozmiarMbMin / 1024.0).ToString("0.0", CultureInfo.CurrentCulture)
            Dim maxGb = (SzacowanyRozmiarMbMax / 1024.0).ToString("0.0", CultureInfo.CurrentCulture)
            Return $"~{minGb}–{maxGb} GB"
        Else
            Dim minMb = Math.Max(1, CInt(Math.Round(SzacowanyRozmiarMbMin)))
            Dim maxMb = Math.Max(minMb, CInt(Math.Round(SzacowanyRozmiarMbMax)))
            Return If(minMb = maxMb, $"~{minMb} MB", $"~{minMb}–{maxMb} MB")
        End If
    End Function
End Class

''' <summary>Katalog szablonów mapowych i logika pomocnicza kreatora generowania map.</summary>
Public Module ZarzadcaSzablonow

    ''' <summary>Limit kafli KMZ, które odbiorniki Garmin wyświetlają łącznie (nowsze modele: 500).</summary>
    Public Const LimitKafliGarmin As Integer = 100

    Private ReadOnly _domyslneSzablony As List(Of SzablonMapy) = UtworzDomyslneSzablony()

    ''' <summary>Pobiera pełną listę predefiniowanych szablonów mapowych.</summary>
    Public Function PobierzDomyslne() As IReadOnlyList(Of SzablonMapy)
        Return _domyslneSzablony.AsReadOnly()
    End Function

    ''' <summary>Filtruje szablony dopasowane do wskazanego urządzenia lub celu.</summary>
    Public Function PobierzDlaCelu(cel As CelMapy) As List(Of SzablonMapy)
        Return _domyslneSzablony.Where(Function(s) s.PasujeDoCelu(cel)).ToList()
    End Function

    ''' <summary>Zwraca szablon o podanym identyfikatorze.</summary>
    Public Function ZnajdzSzablon(id As String) As SzablonMapy
        Return _domyslneSzablony.FirstOrDefault(Function(s) String.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' Aplikuje wybrany szablon do zestawu warstw, pobierając metadane z załadowanych zbiorów map.
    ''' Zalecany piksel warstwy pochodzi z pliku zbioru (rozdzielczość serwera), a nie z szablonu - na jego podstawie
    ''' kreator ostrzega przed pikselem dokładniejszym niż dane.
    ''' </summary>
    Public Sub ZastosujSzablon(szablon As SzablonMapy, zestaw As ZestawWarstw, zrodlaMap As IDictionary(Of String, PlikWarstw))
        If szablon Is Nothing Then Exit Sub
        ZastosujWarstwy(szablon, szablon.Warstwy, zestaw, zrodlaMap)
    End Sub

    ''' <summary>Wypełnia zestaw podanymi warstwami szablonu (np. jednego stopnia skali).</summary>
    Public Sub ZastosujWarstwy(szablon As SzablonMapy, warstwy As IEnumerable(Of WarstwaSzablonu), zestaw As ZestawWarstw, zrodlaMap As IDictionary(Of String, PlikWarstw))
        If szablon Is Nothing OrElse zestaw Is Nothing OrElse warstwy Is Nothing Then Exit Sub
        zestaw.Wyczysc()
        For Each w In warstwy
            Dim adres As String = ""
            Dim typZrodla As TypUslugiMapowej = TypUslugiMapowej.Nieznana
            Dim ignorujSsl As Boolean = False
            Dim plik As PlikWarstw = Nothing

            If zrodlaMap Is Nothing OrElse Not zrodlaMap.TryGetValue(w.Zbior, plik) Then
                plik = WczytajPlikZbioru(w.Zbior)
                If plik IsNot Nothing AndAlso zrodlaMap IsNot Nothing Then zrodlaMap(w.Zbior) = plik
            End If
            Dim zalecany As Double = szablon.DomyslnyPiksel
            If plik IsNot Nothing Then
                adres = plik.Adres
                typZrodla = plik.EfektywnyTyp
                ignorujSsl = plik.IgnorujSsl
                Dim wm = plik.Znajdz(w.Warstwa)
                Dim p As Double
                If wm IsNot Nothing AndAlso Double.TryParse(wm.RozmiarPiksela, NumberStyles.Float, CultureInfo.InvariantCulture, p) AndAlso p > 0 Then
                    zalecany = p
                End If
            End If

            zestaw.Dodaj(w.Warstwa, krycie:=w.Krycie, adres:=adres, zbior:=w.Zbior,
                         zalecanyPiksel:=zalecany, typZrodla:=typZrodla,
                         nakladka:=w.Nakladka, ignorujSsl:=ignorujSsl, trybMieszania:=w.TrybMieszania)
        Next
    End Sub

    ''' <summary>Wczytuje plik zbioru map z folderu warstwy obok programu (albo w folderze bieżącym); Nothing, gdy brak.</summary>
    Public Function WczytajPlikZbioru(zbior As String) As PlikWarstw
        If String.IsNullOrWhiteSpace(zbior) Then Return Nothing
        For Each folder In {AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory}
            Dim sciezka = Path.Combine(folder, "warstwy", zbior & ".txt")
            If File.Exists(sciezka) Then
                Try
                    Return PlikWarstw.Wczytaj(sciezka)
                Catch
                    'uszkodzony plik zbioru - warstwa zostanie dodana bez adresu, a pobieranie zgłosi błąd
                End Try
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Warstwa cieniowania rzeźby terenu, której krycie reguluje suwak kreatora: nakładka (nie najniższa warstwa)
    ''' o nazwie zawierającej „cien”, „shaded” lub „hillshade”. Najniższa warstwa jest pomijana, bo np. HipsoCien
    ''' (hipsometria z cieniowaniem) jest podkładem - zmniejszenie jej krycia odsłoniłoby puste tło.
    ''' </summary>
    Public Function ZnajdzWarstweCieniowania(zestaw As ZestawWarstw) As WarstwaWybrana
        If zestaw Is Nothing Then Return Nothing
        For i = 1 To zestaw.Liczba - 1
            If CzyNazwaCieniowania(zestaw(i).Nazwa) Then Return zestaw(i)
        Next
        Return Nothing
    End Function

    ''' <summary>Indeks warstwy cieniowania w szablonie (jak <see cref="ZnajdzWarstweCieniowania"/>); -1, gdy brak.</summary>
    Public Function IndeksCieniowania(szablon As SzablonMapy) As Integer
        If szablon Is Nothing Then Return -1
        For i = 1 To szablon.Warstwy.Count - 1
            If CzyNazwaCieniowania(szablon.Warstwy(i).Warstwa) Then Return i
        Next
        Return -1
    End Function

    Private Function CzyNazwaCieniowania(nazwa As String) As Boolean
        If String.IsNullOrEmpty(nazwa) Then Return False
        Return nazwa.IndexOf("cien", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               nazwa.IndexOf("shaded", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               nazwa.IndexOf("hillshade", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>
    ''' Liczba osobnych źródeł (serwerów) w zestawie - każde to osobne zapytanie na segment, więc czas pobierania
    ''' rośnie proporcjonalnie (warstwy jednego serwera WMS idą w jednym zapytaniu).
    ''' </summary>
    Public Function LiczbaZrodel(zestaw As ZestawWarstw) As Integer
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return 1
        Return Math.Max(1, zestaw.GrupyZapytan().Count)
    End Function

    ''' <summary>Rozmiar piksela poziomu powiększenia Web Mercator na danej szerokości geograficznej.</summary>
    Public Function PikselPoziomu(poziom As Integer, szerokoscGeo As Double) As Double
        Return 156543.034 * Math.Max(0.1, Math.Cos(szerokoscGeo * Math.PI / 180.0)) / Math.Pow(2, poziom)
    End Function

    ''' <summary>
    ''' Plan mapy wieloskalowej: piksel i zakres powiększeń każdego stopnia skali. Najdokładniejszy stopień dostaje piksel
    ''' wybrany przez użytkownika; stopnie, których piksel nie jest wyraźnie większy, są pomijane. Każdy poziom powiększenia
    ''' trafia do stopnia o pikselu najbliższym rozdzielczości poziomu (w skali logarytmicznej), a najmniej dokładny
    ''' stopień obejmuje jeszcze 3 poziomy poniżej swojej rozdzielczości.
    ''' </summary>
    Public Function PlanStopni(szablon As SzablonMapy, pikselNajdokladniejszy As Double, szerokoscGeo As Double) As List(Of PlanStopnia)
        Dim plan As New List(Of PlanStopnia)()
        If szablon Is Nothing OrElse szablon.StopnieSkali.Count = 0 OrElse pikselNajdokladniejszy <= 0 Then Return plan
        plan.Add(New PlanStopnia With {.Stopien = szablon.StopnieSkali(0), .Piksel = pikselNajdokladniejszy})
        For i = 1 To szablon.StopnieSkali.Count - 1
            Dim st = szablon.StopnieSkali(i)
            If st.Piksel > plan.Last().Piksel * 1.8 Then plan.Add(New PlanStopnia With {.Stopien = st, .Piksel = st.Piksel})
        Next
        Dim cos = Math.Max(0.1, Math.Cos(szerokoscGeo * Math.PI / 180.0))
        Dim naturalny = Function(p As Double) CInt(Math.Round(Math.Log(156543.034 * cos / p, 2)))
        Dim zGora = Math.Max(6, Math.Min(20, naturalny(plan(0).Piksel)))
        Dim zDol = Math.Max(5, Math.Min(zGora, naturalny(plan.Last().Piksel) - 3))
        For Each ps In plan
            ps.PoziomMin = Integer.MaxValue
            ps.PoziomMax = Integer.MinValue
        Next
        For z = zDol To zGora
            Dim res = PikselPoziomu(z, szerokoscGeo)
            Dim najblizszy = plan.OrderBy(Function(ps) Math.Abs(Math.Log(res / ps.Piksel))).First()
            najblizszy.PoziomMin = Math.Min(najblizszy.PoziomMin, z)
            najblizszy.PoziomMax = Math.Max(najblizszy.PoziomMax, z)
        Next
        'stopień bez żadnego poziomu (np. piksel użytkownika zbliżony do następnego stopnia) nie jest pobierany
        plan.RemoveAll(Function(ps) ps.PoziomMin > ps.PoziomMax)
        Return plan
    End Function

    ''' <summary>Łączny szacunek mapy wieloskalowej: suma segmentów, rozmiaru i czasu wszystkich stopni planu.</summary>
    Public Function SzacujWieloskalowo(plan As IList(Of PlanStopnia), zasieg As Zasieg, Optional korytarz As Korytarz = Nothing,
                                       Optional uklad As UkladWspolrzednych = Nothing, Optional liczbaZrodel As Integer = 1) As SzacunekKreatora
        If plan Is Nothing OrElse plan.Count = 0 Then Return New SzacunekKreatora()
        Dim wynik = SzacujParametry(zasieg, plan(0).Piksel, 1024, korytarz, uklad, RodzajEksportu.MBTiles, True, liczbaZrodel)
        For i = 1 To plan.Count - 1
            Dim s = SzacujParametry(zasieg, plan(i).Piksel, 1024, korytarz, uklad, RodzajEksportu.MBTiles, True, liczbaZrodel)
            wynik.LiczbaSegmentow += s.LiczbaSegmentow
            wynik.LiczbaSegmentowPelnych += s.LiczbaSegmentowPelnych
            wynik.SzacowanyRozmiarMbMin += s.SzacowanyRozmiarMbMin
            wynik.SzacowanyRozmiarMbMax += s.SzacowanyRozmiarMbMax
            wynik.SzacowanyRozmiarMb += s.SzacowanyRozmiarMb
            wynik.SzacowanyCzasSekundy += s.SzacowanyCzasSekundy
        Next
        Return wynik
    End Function

    ''' <summary>
    ''' Oblicza szacunkową liczbę segmentów, wielkość pliku i czas trwania operacji z uwzględnieniem formatu i korytarza.
    ''' </summary>
    Public Function SzacujParametry(zasieg As Zasieg, piksel As Double, Optional bok As Integer = 1024,
                                   Optional korytarz As Korytarz = Nothing,
                                   Optional uklad As UkladWspolrzednych = Nothing,
                                   Optional formatDocelowy As RodzajEksportu = RodzajEksportu.MBTiles,
                                   Optional wielopoziomowy As Boolean = True,
                                   Optional liczbaZrodel As Integer = 1) As SzacunekKreatora
        Dim wynik As New SzacunekKreatora()
        If Not zasieg.Poprawny OrElse piksel <= 0 OrElse bok <= 0 Then
            Return wynik
        End If

        Dim siatka As New Siatka(zasieg, piksel, bok)
        If Not siatka.Poprawna Then Return wynik

        wynik.SiatkaKolumny = siatka.LiczbaKolumn
        wynik.SiatkaWiersze = siatka.LiczbaWierszy
        wynik.SzerokoscPx = CLng(siatka.LiczbaKolumn) * bok
        wynik.WysokoscPx = CLng(siatka.LiczbaWierszy) * bok
        wynik.LiczbaSegmentowPelnych = siatka.LiczbaSegmentow

        If korytarz IsNot Nothing AndAlso uklad IsNot Nothing Then
            Dim segm = korytarz.Segmenty(siatka, uklad)
            wynik.LiczbaSegmentow = segm.Count
            wynik.CzyKorytarz = True
            If wynik.LiczbaSegmentowPelnych > 0 Then
                wynik.OszczednoscProcent = Math.Max(0, CInt(Math.Round((1.0 - (CDbl(wynik.LiczbaSegmentow) / CDbl(wynik.LiczbaSegmentowPelnych))) * 100.0)))
            End If
        Else
            wynik.LiczbaSegmentow = siatka.LiczbaSegmentow
            wynik.CzyKorytarz = False
        End If

        ' Współczynniki rozmiaru w MB na segment w zależności od formatu
        Dim mbMin As Double
        Dim mbMax As Double

        Select Case formatDocelowy
            Case RodzajEksportu.MBTiles, RodzajEksportu.SQLiteDB, RodzajEksportu.OruxMaps
                If wielopoziomowy Then
                    ' Piramida powiększeń dodaje ok. 33% kafelków z niższych zoomów
                    mbMin = 0.35
                    mbMax = 0.6
                Else
                    mbMin = 0.25
                    mbMax = 0.45
                End If
            Case RodzajEksportu.KmzGarmin
                ' JPEG 1024x1024, jakość ~80%
                mbMin = 0.15
                mbMax = 0.3
            Case RodzajEksportu.Jnx
                ' BirdsEye JPEG z poziomami
                mbMin = 0.2
                mbMax = 0.4
            Case RodzajEksportu.GeoTiff
                ' Scalony arkusz z kompresją JPEG/Deflate
                mbMin = 0.25
                mbMax = 0.5
            Case Else
                mbMin = 0.25
                mbMax = 0.45
        End Select

        wynik.SzacowanyRozmiarMbMin = Math.Round(wynik.LiczbaSegmentow * mbMin, 1)
        wynik.SzacowanyRozmiarMbMax = Math.Round(wynik.LiczbaSegmentow * mbMax, 1)
        wynik.SzacowanyRozmiarMb = Math.Round((wynik.SzacowanyRozmiarMbMin + wynik.SzacowanyRozmiarMbMax) / 2.0, 1)

        ' Czas: pobieranie 4-wątkowe (ok. 4 zapytania/s), każde źródło zestawu to osobne zapytanie na segment
        Dim zapytania = wynik.LiczbaSegmentow * Math.Max(1, liczbaZrodel)
        Dim czasPobierania = CInt(Math.Min(Integer.MaxValue \ 2, Math.Max(1L, CLng(Math.Ceiling(zapytania / 4.0)))))
        Dim czasKompilacji = 0
        Select Case formatDocelowy
            Case RodzajEksportu.MBTiles, RodzajEksportu.SQLiteDB, RodzajEksportu.Jnx
                czasKompilacji = CInt(Math.Ceiling(wynik.LiczbaSegmentow / 15.0))
            Case RodzajEksportu.GeoTiff, RodzajEksportu.KmzGarmin
                czasKompilacji = CInt(Math.Ceiling(wynik.LiczbaSegmentow / 25.0))
        End Select
        wynik.SzacowanyCzasSekundy = czasPobierania + czasKompilacji

        ' Progi ostrzeżeń są te same, co próg pytania przed generowaniem (WymagaPotwierdzenia = poziom >= 1)
        If formatDocelowy = RodzajEksportu.KmzGarmin AndAlso wynik.LiczbaSegmentow > LimitKafliGarmin Then
            wynik.PoziomOstrzezenia = If(wynik.LiczbaSegmentow > 5 * LimitKafliGarmin, 2, 1)
            wynik.Ostrzezenie = $"Odbiorniki Garmin wyświetlają łącznie {LimitKafliGarmin} kafli KMZ (nowsze modele 500), a ten obszar wymaga {wynik.LiczbaSegmentow:N0}. " &
                                "Zwiększ rozmiar piksela, zmniejsz obszar albo wybierz format JNX (BirdsEye)."
        ElseIf wynik.LiczbaSegmentow <= 500 Then
            wynik.PoziomOstrzezenia = 0
            wynik.Ostrzezenie = "Mały obszar: szybkie pobieranie i poręczny plik mapy."
        ElseIf wynik.LiczbaSegmentow <= 2500 Then
            wynik.PoziomOstrzezenia = 0
            wynik.Ostrzezenie = "Średni obszar: pobieranie potrwa kilka minut."
        ElseIf wynik.LiczbaSegmentow <= 6000 Then
            wynik.PoziomOstrzezenia = 1
            wynik.Ostrzezenie = "Duży obszar: potrzebne stabilne łącze i wolne miejsce na dysku. Większy rozmiar piksela zmniejszy plik."
        Else
            wynik.PoziomOstrzezenia = 2
            wynik.Ostrzezenie = "Bardzo duży obszar (ponad 6 000 segmentów): pobieranie potrwa długo. Zwiększ rozmiar piksela albo zmniejsz obszar."
        End If

        Return wynik
    End Function

    ''' <summary>
    ''' Format pliku dla celu i wariantu wybranego w kreatorze. Wariant zawsze decyduje o formacie - dawniej dla telefonu,
    ''' wydruku i CAD był ignorowany, a dla Garmina KMZ powyżej 100 kafli był po cichu zamieniany na JNX.
    ''' </summary>
    ''' <param name="wielopoziomowy">True - wariant kafli wieloskalowych (MBTiles, JNX); False - wariant pojedynczego pliku
    ''' (telefon: SQLiteDB, Garmin: KMZ, wydruk i CAD/GIS: GeoTIFF).</param>
    Public Function DopasujFormatDocelowy(cel As CelMapy, Optional wielopoziomowy As Boolean = True) As RodzajEksportu
        Select Case cel
            Case CelMapy.TelefonTablet
                Return If(wielopoziomowy, RodzajEksportu.MBTiles, RodzajEksportu.SQLiteDB)
            Case CelMapy.OdbiornikGarmin
                Return If(wielopoziomowy, RodzajEksportu.Jnx, RodzajEksportu.KmzGarmin)
            Case CelMapy.WydrukKomputer, CelMapy.ProjektCadGis
                Return If(wielopoziomowy, RodzajEksportu.MBTiles, RodzajEksportu.GeoTiff)
            Case Else
                Return RodzajEksportu.MBTiles
        End Select
    End Function

    ''' <summary>Wariant zalecany początkującym dla danego celu (True - wielopoziomowy).</summary>
    Public Function ZalecanyWariantWielopoziomowy(cel As CelMapy) As Boolean
        'Garmin: KMZ działa na każdym odbiorniku bez dodatkowych kroków, JNX wymaga obsługi BirdsEye;
        'wydruk i CAD/GIS: jeden arkusz GeoTIFF
        Return cel = CelMapy.TelefonTablet
    End Function

    ''' <summary>Czy format składa się z kafli na wielu poziomach powiększenia (ma sens zakres zoom).</summary>
    Public Function CzyFormatWielopoziomowy(rodzaj As RodzajEksportu) As Boolean
        Select Case rodzaj
            Case RodzajEksportu.MBTiles, RodzajEksportu.SQLiteDB, RodzajEksportu.OruxMaps, RodzajEksportu.Jnx
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>Czytelna nazwa formatu dla użytkownika (zamiast nazwy wartości wyliczenia, np. KMZGARMIN).</summary>
    Public Function NazwaFormatu(rodzaj As RodzajEksportu) As String
        Select Case rodzaj
            Case RodzajEksportu.MBTiles : Return "MBTiles"
            Case RodzajEksportu.SQLiteDB : Return "SQLiteDB (RMaps)"
            Case RodzajEksportu.KmzGarmin : Return "Garmin KMZ (Custom Maps)"
            Case RodzajEksportu.Kmz : Return "KMZ"
            Case RodzajEksportu.Jnx : Return "Garmin JNX (BirdsEye)"
            Case RodzajEksportu.GeoTiff : Return "GeoTIFF"
            Case RodzajEksportu.OruxMaps : Return "OruxMaps"
            Case RodzajEksportu.TrekBuddy : Return "TrekBuddy (TAR)"
            Case Else : Return rodzaj.ToString()
        End Select
    End Function

    ''' <summary>Filtr okna zapisu pliku dla formatu (pierwsza pozycja - ten format, druga - wszystkie pliki).</summary>
    Public Function FiltrPliku(rodzaj As RodzajEksportu) As String
        Dim ext = RozszerzenieDlaRodzaju(rodzaj)
        Return $"{NazwaFormatu(rodzaj)} (*{ext})|*{ext}|Wszystkie pliki (*.*)|*.*"
    End Function

    ''' <summary>Krótka wskazówka, gdzie skopiować gotowy plik, żeby mapa pojawiła się w urządzeniu lub programie.</summary>
    Public Function WskazowkaWgrania(rodzaj As RodzajEksportu) As String
        Select Case rodzaj
            Case RodzajEksportu.MBTiles
                Return "Skopiuj plik do telefonu: Locus Map - folder Locus/mapsOnline lub Locus/maps, OruxMaps - oruxmaps/mapfiles; w QGIS przeciągnij plik na mapę."
            Case RodzajEksportu.SQLiteDB
                Return "Skopiuj plik do telefonu: OsmAnd - folder osmand/tiles, Locus Map - folder Locus/maps. Mapa pojawi się na liście map offline."
            Case RodzajEksportu.KmzGarmin, RodzajEksportu.Kmz
                Return "Podłącz odbiornik kablem USB i skopiuj plik do folderu Garmin/CustomMaps (przy karcie pamięci: Garmin/CustomMaps na karcie)."
            Case RodzajEksportu.Jnx
                Return "Podłącz odbiornik kablem USB i skopiuj plik do folderu Garmin/BirdsEye. Mapę włączysz w ustawieniach mapy odbiornika."
            Case RodzajEksportu.GeoTiff
                Return "Plik GeoTIFF otworzysz w QGIS, AutoCAD Map 3D, Avenza Maps lub QField; obraz zawiera georeferencję w układzie PL-1992."
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Zakres poziomów powiększenia Web Mercator dopasowany do rozmiaru piksela: najwyższy poziom ma piksel najbliższy
    ''' pikselowi mapy (dawniej stałe 10-16 - ortofotomapa 0,25 m traciła szczegóły, a mapa 4 m była rozdmuchana).
    ''' </summary>
    Public Function ZalecanyZakresPowiekszen(piksel As Double, szerokoscGeo As Double) As Tuple(Of Integer, Integer)
        If piksel <= 0 Then Return Tuple.Create(10, 16)
        Dim cos = Math.Max(0.1, Math.Cos(szerokoscGeo * Math.PI / 180.0))
        Dim zMax = CInt(Math.Round(Math.Log(156543.034 * cos / piksel, 2)))
        zMax = Math.Max(6, Math.Min(20, zMax))
        Dim zMin = Math.Max(5, zMax - 6)
        Return Tuple.Create(zMin, zMax)
    End Function

    ''' <summary>
    ''' Najmniejszy rozmiar piksela, który jeszcze dodaje szczegółów: połowa zalecanego piksela najdokładniejszej warstwy
    ''' (0, gdy warstwy nie mają zalecanych rozmiarów). Mniejszy piksel tylko powiększa plik i rozmywa obraz.
    ''' </summary>
    Public Function NajmniejszyPrzydatnyPiksel(zestaw As ZestawWarstw) As Double
        If zestaw Is Nothing Then Return 0
        Dim w = zestaw.NajdokladniejszaWarstwa()
        Return If(w Is Nothing, 0, w.ZalecanyPiksel / 2.0)
    End Function

    ''' <summary>Skala wydruku odpowiadająca pikselowi przy 254 dpi (0,1 mm na piksel) - np. 1 m -> 10 000.</summary>
    Public Function SkalaWydruku(piksel As Double) As Integer
        If piksel <= 0 Then Return 0
        Dim skala = piksel / 0.0001
        'zaokrąglenie do „okrągłych” wartości ułatwia porównanie z mapami papierowymi
        Dim rzad = Math.Pow(10, Math.Floor(Math.Log10(skala)))
        Return CInt(Math.Round(skala / (rzad / 2)) * (rzad / 2))
    End Function

    ''' <summary>Opis, co widać na mapie o danym rozmiarze piksela - zrozumiały dla początkujących.</summary>
    Public Function OpisSzczegolowosci(piksel As Double) As String
        If piksel <= 0 Then Return ""
        If piksel <= 0.3 Then Return "widać pojedyncze drzewa, samochody i granice działek"
        If piksel <= 0.7 Then Return "widać budynki, ogrody i wąskie ścieżki"
        If piksel <= 1.5 Then Return "widać ścieżki, budynki i drobne szczegóły terenu"
        If piksel <= 3.0 Then Return "dobra mapa turystyczna: szlaki, drogi i poziomice"
        If piksel <= 6.0 Then Return "mapa przeglądowa regionu"
        Return "mapa przeglądowa dużego obszaru"
    End Function

    ''' <summary>Polska forma liczebnika: Odmiana(1, "segment", "segmenty", "segmentów") = "segment", 3 - "segmenty", 5 i 12 - "segmentów".</summary>
    Public Function Odmiana(n As Long, jeden As String, kilka As String, wiele As String) As String
        Dim a = Math.Abs(n)
        If a = 1 Then Return jeden
        Dim j = a Mod 10, d = a Mod 100
        If j >= 2 AndAlso j <= 4 AndAlso (d < 12 OrElse d > 14) Then Return kilka
        Return wiele
    End Function

    ''' <summary>Zwraca domyślne rozszerzenie pliku dla danego rodzaju eksportu.</summary>
    Public Function RozszerzenieDlaRodzaju(rodzaj As RodzajEksportu) As String
        Select Case rodzaj
            Case RodzajEksportu.MBTiles : Return ".mbtiles"
            Case RodzajEksportu.KmzGarmin, RodzajEksportu.Kmz : Return ".kmz"
            Case RodzajEksportu.Jnx : Return ".jnx"
            Case RodzajEksportu.SQLiteDB : Return ".sqlitedb"
            Case RodzajEksportu.GeoTiff : Return ".tif"
            Case RodzajEksportu.TrekBuddy : Return ".tar"
            Case Else : Return ".mbtiles"
        End Select
    End Function

    ''' <summary>
    ''' Tworzy zestaw fabrycznych szablonów MapoTero. Piksele, krycia i punkty próbek dobrane na podstawie pobranych
    ''' próbek (audyt 2026-11): piksel równy rozdzielczości warstwy podkładu, cieniowanie w trybie Rzeźba terenu
    ''' (mnożenie kładło szarą zasłonę na płaskim terenie, a przy kryciu 30-45% rzeźby prawie nie było widać),
    ''' mapy topograficzne i historyczne wieloskalowe - serwer TOPO GUGiK sam podaje 1:10 000 przy 2 m, 1:50 000
    ''' przy 10 m, 1:100 000 przy 25 m i mapę przeglądową przy 60 m. Osobnego zestawu skanów map topograficznych
    ''' nie ma: skany z 1992 r. to te same mapy, które podaje TOPO, a wydania z 1965 r. są czarno-białe i nieczytelne.
    ''' </summary>
    Private Function UtworzDomyslneSzablony() As List(Of SzablonMapy)
        Dim lista As New List(Of SzablonMapy)()
        Const ZbiorCieniowania = "numer_mod_terenu_cieniowanie_WMTS"
        Const ZbiorHistoryczny = "mapy_historyczne_IHPAN"
        Dim cien = Function(krycie As Integer) New WarstwaSzablonu(ZbiorCieniowania, "ISOK_Cien", krycie, True, TrybMieszania.RzezbaTerenu, "Cieniowanie rzeźby terenu")
        Dim topo = Function() New WarstwaSzablonu("TOPO_WMTS", "MAPA TOPOGRAFICZNA", 100, False, TrybMieszania.Zwykly, "Mapa topograficzna GUGiK")
        Dim szlakiPiesze = Function() New WarstwaSzablonu("Waymarked_Trails", "hiking", 100, True, TrybMieszania.Zwykly, "Szlaki piesze")

        ' 1. Turystyka piesza (próbka: Hala Gąsienicowa)
        Dim sPiesza As New SzablonMapy(
            "piesza_gory", "Turystyka piesza", "Turystyka",
            "Urzędowa mapa topograficzna GUGiK z cieniowaniem rzeźby terenu i szlakami pieszymi (OpenStreetMap). Przy oddalaniu 1:10 000 przechodzi w 1:50 000, 1:100 000 i mapę przeglądową.",
            "🥾", 2.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 49.244, .ProbkaLon = 20.005}
        sPiesza.DodajStopien("1:10 000", 2.0, topo(), cien(70), szlakiPiesze())
        sPiesza.DodajStopien("1:50 000", 10.0, topo(), cien(70), szlakiPiesze())
        ' 1:100 000 bez szlaków (przy 25 m linie szlaków zlewały się w gąszcz) i ze słabszym cieniowaniem (Tatry czerniały)
        sPiesza.DodajStopien("1:100 000", 25.0, topo(), cien(45))
        sPiesza.DodajStopien("przeglądowa", 60.0, topo())
        lista.Add(sPiesza)

        ' 2. Góry po obu stronach granicy (próbka: Śnieżka) - OpenTopoMap obejmuje też Czechy i Słowację
        Dim otm = Function() New WarstwaSzablonu("OpenTopoMap", "opentopomap", 100, False, TrybMieszania.Zwykly, "OpenTopoMap")
        Dim sGory As New SzablonMapy(
            "gory_poziomice", "Góry z poziomicami", "Turystyka",
            "OpenTopoMap z poziomicami, cieniowaniem i nazwami szczytów oraz szlakami pieszymi. Obejmuje też czeską, słowacką i niemiecką stronę gór, gdzie mapy GUGiK są puste.",
            "⛰️", 2.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 50.736, .ProbkaLon = 15.74}
        sGory.DodajStopien("szczegółowa", 2.0, otm(), szlakiPiesze())
        sGory.DodajStopien("średnia", 8.0, otm(), szlakiPiesze())
        sGory.DodajStopien("przeglądowa", 30.0, otm())
        lista.Add(sGory)

        ' 3. Trasy rowerowe i MTB (próbka: Góry Izerskie, Świeradów-Zdrój - trasa rowerowa 13)
        Dim osm = Function() New WarstwaSzablonu("OSMapa_PL", "osmapa", 100, False, TrybMieszania.Zwykly, "OSMapa")
        Dim rower = Function() New WarstwaSzablonu("Waymarked_Trails", "cycling", 100, True, TrybMieszania.Zwykly, "Szlaki rowerowe")
        Dim mtb = Function() New WarstwaSzablonu("Waymarked_Trails", "mtb", 100, True, TrybMieszania.Zwykly, "Trasy MTB")
        Dim sRower As New SzablonMapy(
            "rower_mtb", "Trasy rowerowe i MTB", "Turystyka",
            "Przejrzysta mapa dróg i ścieżek OSMapa ze szlakami rowerowymi, trasami MTB i cieniowaniem, które pokazuje podjazdy. Przy oddalaniu zostają czytelne główne drogi i trasy.",
            "🚲", 2.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 50.873, .ProbkaLon = 15.334}
        sRower.DodajStopien("szczegółowa", 2.0, osm(), cien(65), rower(), mtb())
        sRower.DodajStopien("średnia", 8.0, osm(), cien(65), rower(), mtb())
        ' przeglądowa bez nakładki szlaków - OSMapa sama rysuje główne trasy, a z nakładką mapa była przeładowana
        sRower.DodajStopien("przeglądowa", 30.0, osm())
        lista.Add(sRower)

        ' 4. Eksploracja terenu (próbka: Bieszczady, wąwozy nad Cisną) - jednoskalowa, bo jej sensem są szczegóły
        ' Cieniowanie w trybie zwykłym 60%: na ciemnej ortofotomapie lasu żaden tryb mieszania nie pokazuje rzeźby,
        ' więc rzeźba LiDAR częściowo zastępuje obraz lotniczy.
        Dim sLas As New SzablonMapy(
            "las_eksploracja", "Eksploracja terenu (LiDAR)", "Teren",
            "Rzeźba terenu ze skanowania laserowego (LiDAR, NMT 1 m) przenikająca się z ortofotomapą. Widać wąwozy, rowy, nasypy, grodziska i okopy ukryte pod koronami drzew.",
            "🌲", 1.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer, CelMapy.ProjektCadGis) With {
            .ProbkaLat = 49.19, .ProbkaLon = 22.42}
        sLas.Warstwy.Add(New WarstwaSzablonu("ortofotomapa_WMTS", "ORTOFOTOMAPA", 100, False, TrybMieszania.Zwykly, "Ortofotomapa"))
        sLas.Warstwy.Add(New WarstwaSzablonu(ZbiorCieniowania, "ISOK_Cien", 60, True, TrybMieszania.Zwykly, "Rzeźba terenu LiDAR"))
        lista.Add(sLas)

        ' 5. Plastyczna mapa rzeźby terenu (próbka: Tarnica, Bieszczady)
        Dim hipso = Function() New WarstwaSzablonu("numer_mod_terenu_WMTS", "HipsoCien", 100, False, TrybMieszania.Zwykly, "Hipsometria z cieniowaniem")
        Dim szczyty = Function() New WarstwaSzablonu("szczyty", "szczyty", 100, True, TrybMieszania.Zwykly, "Szczyty")
        Dim sRzezba As New SzablonMapy(
            "rzezba_terenu", "Plastyczna rzeźba terenu", "Teren",
            "Barwna mapa wysokości (hipsometria) z cieniowaniem rzeźby terenu i nazwami szczytów. Dobrze wygląda na wydruku, plakacie i jako tło do planowania wycieczek.",
            "🏔️", 4.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.WydrukKomputer, CelMapy.ProjektCadGis) With {
            .ProbkaLat = 49.078, .ProbkaLon = 22.72}
        sRzezba.DodajStopien("szczegółowa", 4.0, hipso(), szczyty())
        sRzezba.DodajStopien("średnia", 15.0, hipso(), szczyty())
        sRzezba.DodajStopien("przeglądowa", 50.0, hipso())
        lista.Add(sRzezba)

        ' 6. Mapy WIG (próbka: Twierdza Modlin); 1:25 000 zachowała się tylko dla części kraju - pod nią 1:100 000,
        ' żeby przy zbliżeniu nie było pustych miejsc. Podgląd przy 6 m pokazuje barwną mapę 1:100 000.
        Dim wig100 = Function() New WarstwaSzablonu(ZbiorHistoryczny, "wig100k_2180", 100, False, TrybMieszania.Zwykly, "Mapa taktyczna WIG 1:100 000")
        Dim sWig As New SzablonMapy(
            "historia_wig", "Mapy WIG 1918–1939", "Historia",
            "Przedwojenne mapy WIG z cieniowaniem rzeźby, obszar II RP. Przy zbliżeniu 1:25 000 (gdzie się zachowała, gdzie indziej 1:100 000), przy oddalaniu barwne 1:100 000 i 1:300 000.",
            "📜", 2.0, ProfilKorektyRastra.StareMapyKontrast, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 52.435, .ProbkaLon = 20.685, .ProbkaPiksel = 6.0}
        sWig.DodajStopien("1:25 000", 2.0, wig100(), New WarstwaSzablonu(ZbiorHistoryczny, "wig25k_2180", 100, True, TrybMieszania.Zwykly, "Mapa szczegółowa WIG 1:25 000"), cien(50))
        sWig.DodajStopien("1:100 000", 6.0, wig100(), cien(50))
        sWig.DodajStopien("1:300 000", 20.0, New WarstwaSzablonu(ZbiorHistoryczny, "wig300k_3857", 100, False, TrybMieszania.Zwykly, "Mapa operacyjna WIG 1:300 000"))
        lista.Add(sWig)

        ' 7. Mapy niemieckie (próbka: Olsztyn); Messtischblatt tylko dla ziem dawnych Niemiec - pod nim Karte des
        ' Deutschen Reiches. Podgląd przy 6 m pokazuje kremową KdR z kreskowaniem rzeźby.
        Dim kdr = Function() New WarstwaSzablonu(ZbiorHistoryczny, "kdr_2180", 100, False, TrybMieszania.Zwykly, "Karte des Deutschen Reiches 1:100 000")
        Dim sNiem As New SzablonMapy(
            "historia_niemieckie", "Mapy niemieckie do 1945", "Historia",
            "Pomorze, Śląsk, Prusy, Wielkopolska. Przy zbliżeniu Messtischblatt 1:25 000 (gdzie indziej Karte des Deutschen Reiches), przy oddalaniu KdR 1:100 000 (ok. 1900) i mapa 1:300 000.",
            "🏰", 2.0, ProfilKorektyRastra.StareMapyKontrast, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 53.778, .ProbkaLon = 20.48, .ProbkaPiksel = 6.0}
        sNiem.DodajStopien("1:25 000", 2.0, kdr(), New WarstwaSzablonu(ZbiorHistoryczny, "m25k_2180", 100, True, TrybMieszania.Zwykly, "Messtischblatt 1:25 000"), cien(50))
        sNiem.DodajStopien("1:100 000", 6.0, kdr(), cien(50))
        sNiem.DodajStopien("1:300 000", 20.0, New WarstwaSzablonu(ZbiorHistoryczny, "ukvme_2180", 100, False, TrybMieszania.Zwykly, "Übersichtskarte von Mitteleuropa 1:300 000"))
        lista.Add(sNiem)

        ' 8. Działka i granice katastralne (próbka: Wieliczka)
        Dim sDzialka As New SzablonMapy(
            "dzialka_kataster", "Działka i granice katastralne", "Geodezja",
            "Ortofotomapa lotnicza z granicami i numerami działek ewidencyjnych (EGiB, Krajowa Integracja Ewidencji Gruntów GUGiK).",
            "📐", 0.25, ProfilKorektyRastra.Brak, CelMapy.ProjektCadGis, CelMapy.TelefonTablet, CelMapy.WydrukKomputer) With {
            .ProbkaLat = 49.987, .ProbkaLon = 20.065}
        sDzialka.Warstwy.Add(New WarstwaSzablonu("ortofotomapa_WMTS", "ORTOFOTOMAPA", 100, False, TrybMieszania.Zwykly, "Ortofotomapa"))
        sDzialka.Warstwy.Add(New WarstwaSzablonu("dzialki_ewid_EGIB", "dzialki", 100, True, TrybMieszania.Zwykly, "Granice działek"))
        sDzialka.Warstwy.Add(New WarstwaSzablonu("dzialki_ewid_EGIB", "numery_dzialek", 100, True, TrybMieszania.Zwykly, "Numery działek"))
        lista.Add(sDzialka)

        ' 9. Własny zestaw z okna głównego
        Dim sWlasny As New SzablonMapy(
            "wlasny_zestaw", "Moje warstwy z okna głównego", "Zaawansowane",
            "Mapa z warstw, krycia i trybów mieszania wybranych w oknie głównym programu (lista „Wybrane warstwy””).",
            "⚙️", 1.0, ProfilKorektyRastra.Brak, CelMapy.TelefonTablet, CelMapy.OdbiornikGarmin, CelMapy.WydrukKomputer, CelMapy.ProjektCadGis)
        lista.Add(sWlasny)

        Return lista
    End Function

End Module
