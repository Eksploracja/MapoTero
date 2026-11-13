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

Imports System.Collections.Concurrent
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>Parametry jednego pobierania segmentów mapy z serwera WMS.</summary>
Public Class ZadaniePobierania
    Public Property AdresSerwera As String = ""
    Public Property Warstwy As New List(Of String)
    ''' <summary>Krycie kolejnych warstw (procent); brak wartości - warstwa w pełni kryjąca.</summary>
    Public Property Krycia As New List(Of Integer)
    ''' <summary>Adresy serwerów WMS / usług WMTS kolejnych warstw; brak wartości - AdresSerwera.</summary>
    Public Property Adresy As New List(Of String)
    ''' <summary>Nazwy zbiorów map kolejnych warstw (indeksy w <see cref="ZrodlaMap"/>).</summary>
    Public Property Zbiory As New List(Of String)
    ''' <summary>Odczytane zbiory map sesji (np. metadane XYZ/TMS: zakres powiększeń, nakładka, subdomeny, odstęp zapytań).</summary>
    Public Property ZrodlaMap As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Public Property Siatka As Siatka
    Public Property Format As String = "jpeg"
    Public Property Prefiks As String = "_"
    Public Property Numeracja As StylNumeracji = StylNumeracji.WierszKolumna
    ''' <summary>Folder segmentów (zakończony "\").</summary>
    Public Property Folder As String = ""
    Public Property ZamienOsie As Boolean
    Public Property IloscProb As Integer = 3
    Public Property PrzerwaSekundy As Integer = 5
    Public Property LiczbaWatkow As Integer = 4
    ''' <summary>Limit czasu oczekiwania na odpowiedź serwera [s].</summary>
    Public Property LimitCzasuSekundy As Integer = 60
    ''' <summary>WMTS: pozostawienie pobranych kafli w folderze sesji po pobraniu wszystkich segmentów.</summary>
    Public Property ZachowajKafleWmts As Boolean
    ''' <summary>WMTS: jakość JPEG segmentów składanych z kafli.</summary>
    Public Property JakoscJpegWmts As Integer = 90
    Public Property PobierajPowyzejOstatniego As Boolean
    Public Property Georeferencja As New OpcjeGeoreferencji()
    Public Property TrekBuddy As Boolean
    ''' <summary>Dopisek do nazwy paczki TrekBuddy / Locus Map.</summary>
    Public Property NazwaTrekBuddy As String = ""
    ''' <summary>Przezroczyste tło segmentów PNG / GIF (parametr TRANSPARENT=TRUE zapytań WMS).</summary>
    Public Property Przezroczystosc As Boolean
    ''' <summary>Obszar wzdłuż trasy: pobierane są tylko segmenty przecinające korytarz; Nothing - cały prostokąt.</summary>
    Public Property Korytarz As Korytarz
    ''' <summary>
    ''' Odświeżenie mapy: wszystkie segmenty pobierane są ponownie (istniejący plik zastępowany jest dopiero po
    ''' udanym pobraniu nowego), a kafle WMTS zachowane w folderze sesji - usuwane przed pobieraniem.
    ''' </summary>
    Public Property Odswiez As Boolean
    ''' <summary>Pobieranie segmentów bez weryfikacji certyfikatu SSL (dla serwerów dodanych z opcją ignorowania błędów SSL).</summary>
    Public Property IgnorujSsl As Boolean
    ''' <summary>Po ilu sekundach buforowany na dysku kafel sprawdza na serwerze aktualizację przez ETag / Last-Modified (domyślnie 15 min).</summary>
    Public Property WiekWalidacjiCacheSekundy As Integer = 900

    Public ReadOnly Property Rozszerzenie As String
        Get
            Return Wms.RozszerzeniePliku(Format)
        End Get
    End Property

    Friend Function ZnajdzZrodlo(indeks As Integer, adres As String) As PlikWarstw
        If ZrodlaMap Is Nothing OrElse ZrodlaMap.Count = 0 Then Return Nothing
        If indeks >= 0 AndAlso indeks < Zbiory.Count AndAlso Not String.IsNullOrEmpty(Zbiory(indeks)) Then
            Dim zb As PlikWarstw = Nothing
            If ZrodlaMap.TryGetValue(Zbiory(indeks), zb) AndAlso zb IsNot Nothing Then Return zb
        End If
        If Not String.IsNullOrEmpty(adres) Then
            For Each zb In ZrodlaMap.Values
                If zb IsNot Nothing AndAlso String.Equals(zb.Adres, adres, StringComparison.OrdinalIgnoreCase) Then Return zb
            Next
        End If
        Return Nothing
    End Function

    ''' <summary>Warstwy z kryciem, adresem serwera i typem źródła.</summary>
    Public Function WybranyZestaw() As ZestawWarstw
        Dim z As New ZestawWarstw()
        For i = 0 To Warstwy.Count - 1
            Dim adres As String = If(i < Adresy.Count AndAlso Not String.IsNullOrEmpty(Adresy(i)), Adresy(i), AdresSerwera)
            Dim zbior As String = If(i < Zbiory.Count, Zbiory(i), "")
            Dim src = ZnajdzZrodlo(i, adres)
            Dim typZrodla As TypUslugiMapowej = If(src IsNot Nothing, src.EfektywnyTyp, TypUslugiMapowej.Nieznana)
            Dim nakladka As Boolean = If(src IsNot Nothing, src.OpcjeKafli(Warstwy(i)).Nakladka, False)
            Dim bezSsl As Boolean = IgnorujSsl OrElse If(src IsNot Nothing, src.IgnorujSsl, False)
            z.Dodaj(Warstwy(i), If(i < Krycia.Count, Krycia(i), ZestawWarstw.PelneKrycie), adres, zbior, 0, typZrodla, nakladka, False, bezSsl)
        Next
        Return z
    End Function
End Class

''' <summary>Stan pobierania przekazywany do okna programu.</summary>
Public Class PostepPobierania
    Public Property Pobrane As Integer
    Public Property Wszystkie As Integer
    Public Property Proba As Integer
    Public Property Segment As String = ""
    ''' <summary>Komunikat dla użytkownika (np. o oczekiwaniu na kolejną próbę); pusty - bez zmian.</summary>
    Public Property Komunikat As String = ""
End Class

''' <summary>Wynik pobierania.</summary>
Public Class WynikPobierania
    Public Property Wszystkie As Integer
    ''' <summary>Liczba segmentów, których nie udało się pobrać (wykaz w error.txt).</summary>
    Public Property Nieudane As Integer
    Public Property Przerwano As Boolean
End Class

''' <summary>
''' Pobieranie segmentów mapy z serwera WMS (lub usługi WMTS - zob. ZrodloWmts). Dawniej odbywało się w wątku okna (Application.DoEvents, Sleep z kernel32),
''' po jednym segmencie i bez limitu czasu. Teraz: HttpClient, kilka segmentów jednocześnie, przerywanie
''' w dowolnej chwili, okno programu pozostaje w pełni responsywne.
''' </summary>
Public NotInheritable Class PobieranieSegmentow

    Private Sub New()
    End Sub

    Private Shared _klient As HttpClient = UtworzKlienta(Nothing, False)
    Private Shared _klientBezSsl As HttpClient = UtworzKlienta(Nothing, True)
    Private Shared _nadpisanyKlient As Boolean

    Friend Shared Property Klient As HttpClient
        Get
            Return _klient
        End Get
        Set(value As HttpClient)
            _nadpisanyKlient = (value IsNot Nothing)
            _klient = If(value, UtworzKlienta(Nothing, False))
        End Set
    End Property

    Friend Shared Function WybierzKlienta(ignorujSsl As Boolean) As HttpClient
        If _nadpisanyKlient OrElse Not ignorujSsl Then Return _klient
        Return _klientBezSsl
    End Function

    Friend Shared Function UtworzKlienta(handler As HttpMessageHandler, Optional ignorujSsl As Boolean = False) As HttpClient
        Dim obsluga As HttpMessageHandler = handler
        If obsluga Is Nothing Then
            Dim h As New HttpClientHandler() With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate}
            If ignorujSsl Then
                h.ServerCertificateCustomValidationCallback = Function(m, c, ch, err) True
            End If
            obsluga = h
        End If
        'limit czasu ustawiany osobno dla każdego zapytania (LimitCzasu) - wartość z ustawień może się zmieniać
        Dim k As New HttpClient(obsluga, disposeHandler:=handler Is Nothing) With {.Timeout = Threading.Timeout.InfiniteTimeSpan}
        k.DefaultRequestHeaders.UserAgent.ParseAdd(Program.UserAgentProgramu & " (+https://github.com/Eksploracja/MapoTero)")
        Return k
    End Function

    ''' <summary>
    ''' Token przerwania zapytania: przerwanie przez użytkownika albo przekroczenie limitu czasu.
    ''' Przekroczenie limitu rozpoznaje się po tym, że token użytkownika nie został przerwany.
    ''' </summary>
    Friend Shared Function LimitCzasu(sekundy As Integer, token As CancellationToken) As CancellationTokenSource
        Dim cts = CancellationTokenSource.CreateLinkedTokenSource(token)
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, sekundy)))
        Return cts
    End Function

    ''' <summary>Opis błędu przekroczenia limitu czasu.</summary>
    Friend Shared Function OpisLimituCzasu(sekundy As Integer) As String
        Return "przekroczono czas oczekiwania na odpowiedź serwera (" & sekundy & " s)"
    End Function

    ''' <summary>
    ''' Segment do pobrania: sam wiersz i kolumna siatki - nazwa, zasięg i adres wyznaczane są w razie potrzeby (dawniej
    ''' każdy segment przechowywał gotowy adres zapytania, co przy milionach segmentów wyczerpywało pamięć).
    ''' </summary>
    Private Structure OpisSegmentu
        Public Indeks As Integer
        Public Wiersz As Integer
        Public Kolumna As Integer
    End Structure

    ''' <summary>Odstęp raportów postępu [ms] - przy setkach segmentów na sekundę okno nie nadążało z ich obsługą.</summary>
    Private Const OdstepRaportuMs As Long = 100

    Public Shared Async Function PobierzAsync(z As ZadaniePobierania, postep As IProgress(Of PostepPobierania),
                                              token As CancellationToken) As Task(Of WynikPobierania)
        Dim s As Siatka = z.Siatka
        Dim ext As String = z.Rozszerzenie
        Dim wynik As New WynikPobierania()

        'łącznie (segmenty WMS i kafle WMTS) najwyżej tyle jednoczesnych zapytań do serwera, ile wątków; hamowanie przy przeciążeniu
        RegulatorZapytan.Pobieranie.Rozpocznij(z.LiczbaWatkow, Sub(tekst) postep?.Report(New PostepPobierania With {.Komunikat = tekst}))

        'grupy warstw: z jednego serwera i o pełnym kryciu - razem, pozostałe osobno (nakładane w programie)
        Dim grupy As List(Of GrupaZapytania) = z.WybranyZestaw().GrupyZapytan()
        If grupy.Count = 0 Then Throw New InvalidOperationException("nie wybrano żadnej warstwy")

        'usługi kafelkowe (WMTS, XYZ, TMS) - warstwy składane z kafli; wspólny folder kafli sesji
        Dim maKafelkowe As Boolean = grupy.Any(Function(g) g.CzyKafelkowa)
        Dim domyslnyFolderKafli As String = z.Folder & "_kafle_wmts\"
        If maKafelkowe AndAlso z.Odswiez Then
            Try
                If Directory.Exists(domyslnyFolderKafli) Then Directory.Delete(domyslnyFolderKafli, True)
            Catch
            End Try
        End If
        Dim uslugiWmts As New Dictionary(Of String, ZrodloWmts)(StringComparer.OrdinalIgnoreCase)
        For Each adresUslugi In grupy.Where(Function(g) g.CzyKafelkowa).Select(Function(g) g.Adres).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            Dim grupyAdresu = grupy.Where(Function(g) g.CzyKafelkowa AndAlso String.Equals(g.Adres, adresUslugi, StringComparison.OrdinalIgnoreCase)).ToList()
            Dim nazwyWarstw = grupyAdresu.SelectMany(Function(g) g.Warstwy).Distinct(StringComparer.Ordinal).ToList()
            Dim typUslugi = grupyAdresu(0).EfektywnyTyp
            Dim idx = Enumerable.Range(0, z.Warstwy.Count).FirstOrDefault(
                Function(ix) String.Equals(If(ix < z.Adresy.Count AndAlso Not String.IsNullOrEmpty(z.Adresy(ix)), z.Adresy(ix), z.AdresSerwera), adresUslugi, StringComparison.OrdinalIgnoreCase), -1)
            Dim zrodloPliku As PlikWarstw = z.ZnajdzZrodlo(idx, adresUslugi)
            Try
                uslugiWmts(adresUslugi) = Await ZrodloWmts.PrzygotujAsync(z, adresUslugi, nazwyWarstw, token,
                                                                           typUslugi:=typUslugi, plikWarstw:=zrodloPliku)
            Catch ex As OperationCanceledException
                wynik.Przerwano = True
                Return wynik
            End Try
        Next
        If uslugiWmts.Count > 0 Then
            Dim pierwsza = uslugiWmts.Values.First()
            UsunPlikiTymczasowe(pierwsza.FolderKafli, "*.tmp")
        End If

        'jedna grupa WMS o pełnym kryciu i zwykłym trybie mieszania - jedno zapytanie i zapis tego, co przysłał serwer (jak w starszych wersjach);
        'w pozostałych przypadkach segment składany jest z grup w programie
        Dim skladanie As Boolean = grupy.Count > 1 OrElse grupy(0).CzyKafelkowa OrElse grupy(0).Krycie < ZestawWarstw.PelneKrycie OrElse grupy(0).TrybMieszania <> TrybMieszania.Zwykly

        Dim nazwa As Func(Of OpisSegmentu, String) =
            Function(sg) If(z.TrekBuddy, s.NazwaSegmentuTrekBuddy(z.Prefiks, sg.Wiersz, sg.Kolumna), s.NazwaSegmentu(z.Prefiks, z.Numeracja, sg.Wiersz, sg.Kolumna))
        'adres do wykazów koordynaty.txt i error.txt - zapytanie (lub kafel) pierwszej grupy
        Dim adres As Func(Of OpisSegmentu, String) =
            Function(sg)
                Dim zasiegSegmentu = s.Segment(sg.Wiersz, sg.Kolumna)
                Dim g = grupy(0)
                If g.CzyKafelkowa Then Return uslugiWmts(g.Adres).AdresSegmentu(zasiegSegmentu)
                Return Wms.ZapytanieGetMap(g.Adres, g.Warstwy, z.Uklad, zasiegSegmentu, z.Format, s.BokSegmentuPx, s.BokSegmentuPx, z.ZamienOsie,
                                           z.Przezroczystosc)
            End Function

        'segmenty przecinające korytarz trasy (Nothing - wszystkie segmenty prostokąta)
        Dim wKorytarzu As HashSet(Of Integer) = Nothing
        If z.Korytarz IsNot Nothing AndAlso z.Korytarz.Poprawny Then wKorytarzu = z.Korytarz.Segmenty(s, z.Uklad)

        'lista segmentów w kolejności wierszami od góry
        Dim segmenty As New List(Of OpisSegmentu)
        For w = 1 To s.LiczbaWierszy
            For k = 1 To s.LiczbaKolumn
                If wKorytarzu IsNot Nothing AndAlso Not wKorytarzu.Contains(s.NumerKolejny(w, k) - 1) Then Continue For
                segmenty.Add(New OpisSegmentu With {.Indeks = segmenty.Count, .Wiersz = w, .Kolumna = k})
            Next
        Next
        wKorytarzu = Nothing
        wynik.Wszystkie = segmenty.Count
        If uslugiWmts.Count > 0 Then
            postep?.Report(New PostepPobierania With {.Komunikat = "Trwa pobieranie segmentów (" & segmenty.Count & "); kafle: " &
                                                                    String.Join("; ", uslugiWmts.Values.Select(Function(u) u.Opis))})
        End If

        'foldery mapy TrekBuddy / Locus Map
        Dim folderDocelowy As String = If(z.Folder.EndsWith("\") OrElse z.Folder.EndsWith("/"), z.Folder, z.Folder & Path.DirectorySeparatorChar)
        Dim tb As MapaTrekBuddy = Nothing
        If z.TrekBuddy Then
            tb = New MapaTrekBuddy(z)
            tb.PrzygotujFoldery()
            folderDocelowy = tb.FolderSet
        End If

        'pliki tymczasowe segmentów po przerwanym pobieraniu (np. zamknięciu programu w trakcie zapisu)
        UsunPlikiTymczasowe(folderDocelowy, z.Prefiks & "*." & ext & "*.tmp")
        ZapiszKoordynaty(z, segmenty, nazwa, adres)

        'brakujące segmenty są zawsze pobierane (istniejące pliki pomijane); opcja "powyżej ostatniego" pomija też luki
        'przed ostatnim pobranym segmentem (dawniej filtrowała tylko istniejące pliki, więc nic nie zmieniała)
        Dim doPobrania As List(Of OpisSegmentu) = segmenty
        If z.PobierajPowyzejOstatniego AndAlso Not z.TrekBuddy AndAlso Not z.Odswiez Then
            Dim ostatni As Integer = segmenty.FindLastIndex(Function(sg) File.Exists(Path.Combine(folderDocelowy, nazwa(sg) & "." & ext)))
            If ostatni >= 0 Then doPobrania = segmenty.GetRange(ostatni + 1, segmenty.Count - ostatni - 1)
        End If

        Dim plikBledow As String = Path.Combine(z.Folder, "error.txt")
        Dim przyczyny As New ConcurrentDictionary(Of Integer, String)
        'odświeżanie: segmenty już pobrane ponownie (kolejne próby pomijają je); każdy wątek zapisuje inne pozycje
        Dim odswiezone(Math.Max(0, segmenty.Count - 1)) As Boolean

        Try
            For proba = 1 To Math.Max(1, z.IloscProb)
                'plik error.txt istnieje w trakcie pobierania - sygnał niekompletnego zestawu dla narzędzia scalania
                File.WriteAllText(plikBledow, "")
                przyczyny.Clear()
                Dim licznik As Integer = 0
                Dim nastepny As Integer = -1
                Dim ostatniRaport As Long = 0
                Dim biezacaProba As Integer = proba

                Dim pobierzSegment =
                    Async Function(sg As OpisSegmentu) As Task
                        Dim nazwaSegmentu As String = nazwa(sg)
                        Dim plik As String = Path.Combine(folderDocelowy, nazwaSegmentu & "." & ext)
                        Try
                            Dim blad As String = ""
                            Dim zasieg = s.Segment(sg.Wiersz, sg.Kolumna)
                            Dim pobierz As Boolean = Not File.Exists(plik) OrElse (z.Odswiez AndAlso Not odswiezone(sg.Indeks))
                            If pobierz Then
                                If skladanie Then
                                    blad = Await ZlozSegmentAsync(z, grupy, uslugiWmts, zasieg, s.BokSegmentuPx, plik, token).ConfigureAwait(False)
                                Else
                                    blad = Await PobierzSegmentAsync(adres(sg), plik, z.LimitCzasuSekundy, token, z.IgnorujSsl OrElse grupy(0).IgnorujSsl).ConfigureAwait(False)
                                End If
                            End If
                            If pobierz AndAlso blad = "" Then odswiezone(sg.Indeks) = True
                            If File.Exists(plik) AndAlso blad = "" AndAlso Not z.TrekBuddy AndAlso z.Georeferencja.Dowolna Then
                                Try
                                    ZapisGeoreferencji.ZapiszPliki(ZapisGeoreferencji.Obraz(z.Uklad, zasieg, s.BokSegmentuPx, s.BokSegmentuPx,
                                                                                           folderDocelowy, nazwaSegmentu & "." & ext),
                                                                  folderDocelowy, nazwaSegmentu, ext, z.Georeferencja)
                                Catch ex As Exception
                                    blad = "nie udało się zapisać plików georeferencyjnych: " & ex.Message
                                End Try
                            End If
                            If blad <> "" OrElse Not File.Exists(plik) OrElse New FileInfo(plik).Length = 0 Then
                                przyczyny(sg.Indeks) = If(blad <> "", blad, "brak lub pusty plik segmentu po pobraniu")
                            End If
                        Catch ex As OperationCanceledException When token.IsCancellationRequested
                            Throw
                        Catch ex As Exception
                            'błąd jednego segmentu (np. zapisu na dysku) nie przerywa pobierania pozostałych - trafia do error.txt
                            przyczyny(sg.Indeks) = ex.Message.Replace(vbCr, " ").Replace(vbLf, " ")
                        Finally
                            Dim n As Integer = Interlocked.Increment(licznik)
                            Dim teraz As Long = Environment.TickCount64
                            If n = doPobrania.Count OrElse teraz - Interlocked.Read(ostatniRaport) >= OdstepRaportuMs Then
                                Interlocked.Exchange(ostatniRaport, teraz)
                                postep?.Report(New PostepPobierania With {.Pobrane = n, .Wszystkie = doPobrania.Count, .Proba = biezacaProba, .Segment = nazwaSegmentu})
                            End If
                        End Try
                    End Function

                'stała liczba wątków pobierających kolejne segmenty (dawniej osobne zadanie dla każdego segmentu naraz)
                Dim watki As New List(Of Task)
                For i = 1 To Math.Max(1, Math.Min(z.LiczbaWatkow, doPobrania.Count))
                    watki.Add(Task.Run(
                        Async Function()
                            Do
                                token.ThrowIfCancellationRequested()
                                Dim j As Integer = Interlocked.Increment(nastepny)
                                If j >= doPobrania.Count Then Exit Do
                                Await pobierzSegment(doPobrania(j)).ConfigureAwait(False)
                            Loop
                        End Function, token))
                Next
                Await Task.WhenAll(watki).ConfigureAwait(False)

                'przed zakończeniem pętli upewnij się, że wszystkie pliki segmentów faktycznie istnieją i nie są puste
                For Each sg In doPobrania
                    Dim sciezkaSeg As String = Path.Combine(folderDocelowy, nazwa(sg) & "." & ext)
                    If Not File.Exists(sciezkaSeg) OrElse New FileInfo(sciezkaSeg).Length = 0 Then
                        If Not przyczyny.ContainsKey(sg.Indeks) Then
                            przyczyny(sg.Indeks) = "brak kompletnego pliku segmentu"
                        End If
                    End If
                Next

                If przyczyny.IsEmpty OrElse proba >= z.IloscProb Then Exit For

                postep?.Report(New PostepPobierania With {.Pobrane = 0, .Wszystkie = doPobrania.Count, .Proba = proba,
                    .Komunikat = "Nie pobrano " & przyczyny.Count & If(przyczyny.Count = 1, " segmentu", " segmentów") & ". Kolejna próba (" & (proba + 1) & " z " & z.IloscProb & ") za " & z.PrzerwaSekundy & " s"})
                Await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, z.PrzerwaSekundy)), token).ConfigureAwait(False)
            Next
        Catch ex As OperationCanceledException
            wynik.Przerwano = True
        End Try

        'wykaz brakujących segmentów (także po przerwaniu - scalanie ostrzeże o niekompletnym zestawie); brakujący jest
        'też segment z błędem ostatniej próby (np. zapisu plików georeferencyjnych) i - przy odświeżaniu - segment,
        'którego nie udało się pobrać ponownie (pozostał stary plik)
        Dim brakujace As New List(Of OpisSegmentu)
        For Each sg In doPobrania
            If Not File.Exists(Path.Combine(folderDocelowy, nazwa(sg) & "." & ext)) OrElse przyczyny.ContainsKey(sg.Indeks) OrElse
               (z.Odswiez AndAlso Not odswiezone(sg.Indeks)) Then
                brakujace.Add(sg)
            End If
        Next
        wynik.Nieudane = brakujace.Count
        If brakujace.Count = 0 Then
            If File.Exists(plikBledow) Then File.Delete(plikBledow)
            'komplet segmentów - pobrane kafle WMTS nie są już potrzebne
            If uslugiWmts.Count > 0 AndAlso Not wynik.Przerwano AndAlso Not z.ZachowajKafleWmts Then uslugiWmts.Values.First().UsunKafle()
        Else
            Using plik As New StreamWriter(plikBledow, False, KodowanieSystemowe())
                For Each sg In brakujace
                    plik.WriteLine(nazwa(sg))
                    plik.WriteLine(MetadaneMapy.MaskujSekrety(adres(sg)))
                    Dim przyczyna As String = Nothing
                    If przyczyny.TryGetValue(sg.Indeks, przyczyna) AndAlso przyczyna <> "" Then
                        plik.WriteLine("przyczyna: " & MetadaneMapy.MaskujSekrety(przyczyna).Replace(""""c, "'"c))
                    ElseIf wynik.Przerwano Then
                        plik.WriteLine("przyczyna: przerwano pobieranie")
                    End If
                Next
            End Using
        End If

        If tb IsNot Nothing Then
            tb.ZapiszListeSegmentow(segmenty.Select(Function(sg) nazwa(sg) & "." & ext))
            If Not wynik.Przerwano Then
                Try
                    tb.UtworzArchiwumTar()
                Catch ex As Exception
                    Debug.WriteLine("Nie udało się utworzyć archiwum TAR: " & ex.Message)
                End Try
            End If
        End If

        Return wynik
    End Function

    ''' <summary>Usuwa pliki tymczasowe pasujące do wzorca (pozostałe po przerwanym zapisie); zablokowane pomija.</summary>
    Private Shared Sub UsunPlikiTymczasowe(folder As String, wzorzec As String)
        Try
            If Not Directory.Exists(folder) Then Exit Sub
            For Each plik In Directory.EnumerateFiles(folder, wzorzec)
                If Not plik.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) Then Continue For
                Try
                    File.Delete(plik)
                Catch
                End Try
            Next
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Pobiera jeden segment i zapisuje go w pliku. Zwraca pusty tekst, gdy się udało, albo opis błędu
    ''' (np. komunikat serwera WMS), gdy segmentu nie pobrano.
    ''' </summary>
    Public Shared Async Function PobierzSegmentAsync(adres As String, plikDocelowy As String, limitCzasuSekundy As Integer,
                                                     token As CancellationToken, Optional ignorujSsl As Boolean = False) As Task(Of String)
        Dim wynik = Await PobierzObrazAsync(adres, limitCzasuSekundy, token, ignorujSsl).ConfigureAwait(False)
        If wynik.Item2 <> "" Then Return wynik.Item2
        Try
            'zapisuje dokładnie to, co przysłał serwer - bez ponownej kompresji obrazka
            ZapiszPlik(plikDocelowy, wynik.Item1)
            Return ""
        Catch ex As Exception
            Return ex.Message.Replace(vbCr, " ").Replace(vbLf, " ")
        End Try
    End Function

    ''' <summary>Zapis przez plik tymczasowy - przerwany zapis nie zostawia uszkodzonego segmentu.</summary>
    Private Shared Sub ZapiszPlik(plikDocelowy As String, dane() As Byte)
        Dim tymczasowy As String = plikDocelowy & "." & Guid.NewGuid().ToString("N") & ".tmp"
        File.WriteAllBytes(tymczasowy, dane)
        File.Move(tymczasowy, plikDocelowy, overwrite:=True)
    End Sub

    ''' <summary>
    ''' Pobiera obraz z serwera WMS. Wynik: (dane obrazu, pusty tekst) albo (Nothing, opis błędu - np. komunikat serwera).
    ''' </summary>
    Friend Shared Async Function PobierzObrazAsync(adres As String, limitCzasuSekundy As Integer,
                                                   token As CancellationToken, Optional ignorujSsl As Boolean = False) As Task(Of Tuple(Of Byte(), String))
        'regulacja obciążenia serwera - limit czasu odpowiedzi liczony od wysłania zapytania, bez oczekiwania na przepustkę
        Dim przepustka As RegulatorZapytan.Przepustka = Nothing
        Try
            przepustka = Await RegulatorZapytan.Pobieranie.WejdzAsync(adres, token).ConfigureAwait(False)
            Using limit = LimitCzasu(limitCzasuSekundy, token),
                  odpowiedz = Await WybierzKlienta(ignorujSsl).GetAsync(adres, HttpCompletionOption.ResponseContentRead, limit.Token).ConfigureAwait(False)
                przepustka.Odpowiedz(odpowiedz)
                Dim dane() As Byte = Await odpowiedz.Content.ReadAsByteArrayAsync(limit.Token).ConfigureAwait(False)
                przepustka.Dispose()
                Dim typ As String = If(odpowiedz.Content.Headers.ContentType?.MediaType, "")

                If Not CzyObraz(dane) Then
                    'serwer WMS w razie błędu zwraca zwykle dokument XML (ServiceException) zamiast obrazka
                    Dim tekst As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 500)).Replace(vbCr, " ").Replace(vbLf, " ")
                    If Not odpowiedz.IsSuccessStatusCode Then
                        Return Blad("HTTP " & CInt(odpowiedz.StatusCode) & " " & odpowiedz.ReasonPhrase & ": " & tekst)
                    End If
                    If tekst.IndexOf("InvalidCRS", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       tekst.IndexOf("CRS not supported", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       tekst.IndexOf("SRS not supported", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        Return Blad("brak obsługi układu współrzędnych przez serwer WMS: " & tekst)
                    End If
                    Return Blad("serwer nie zwrócił obrazka (" & typ & "): " & tekst)
                End If
                Return Tuple.Create(dane, "")
            End Using
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As OperationCanceledException
            przepustka?.Blad()
            Return Blad(OpisLimituCzasu(limitCzasuSekundy))
        Catch ex As Exception
            If TypeOf ex Is HttpRequestException Then przepustka?.Blad()
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return Blad(opis.Replace(vbCr, " ").Replace(vbLf, " "))
        Finally
            przepustka?.Dispose()
        End Try
    End Function

    ''' <summary>
    ''' Pobiera obraz z serwera WMS bezpośrednio (bez kolejki regulatora pobierania). Używane dla interaktywnego podglądu.
    ''' </summary>
    Friend Shared Async Function PobierzObrazBezRegulatoraAsync(adres As String, limitCzasuSekundy As Integer,
                                                               token As CancellationToken, Optional ignorujSsl As Boolean = False) As Task(Of Tuple(Of Byte(), String))
        Try
            Using limit = LimitCzasu(limitCzasuSekundy, token),
                  odpowiedz = Await WybierzKlienta(ignorujSsl).GetAsync(adres, HttpCompletionOption.ResponseContentRead, limit.Token).ConfigureAwait(False)
                Dim dane() As Byte = Await odpowiedz.Content.ReadAsByteArrayAsync(limit.Token).ConfigureAwait(False)
                Dim typ As String = If(odpowiedz.Content.Headers.ContentType?.MediaType, "")

                If Not CzyObraz(dane) Then
                    Dim tekst As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 500)).Replace(vbCr, " ").Replace(vbLf, " ")
                    If Not odpowiedz.IsSuccessStatusCode Then
                        Return Blad("HTTP " & CInt(odpowiedz.StatusCode) & " " & odpowiedz.ReasonPhrase & ": " & tekst)
                    End If
                    If tekst.IndexOf("InvalidCRS", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       tekst.IndexOf("CRS not supported", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       tekst.IndexOf("SRS not supported", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        Return Blad("brak obsługi układu współrzędnych przez serwer WMS: " & tekst)
                    End If
                    Return Blad("serwer nie zwrócił obrazka (" & typ & "): " & tekst)
                End If
                Return Tuple.Create(dane, "")
            End Using
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As OperationCanceledException
            Return Blad(OpisLimituCzasu(limitCzasuSekundy))
        Catch ex As Exception
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return Blad(opis.Replace(vbCr, " ").Replace(vbLf, " "))
        End Try
    End Function

    Private Shared Function Blad(opis As String) As Tuple(Of Byte(), String)
        Return Tuple.Create(CType(Nothing, Byte()), opis)
    End Function

    ''' <summary>
    ''' Segment złożony z grup warstw (różne serwery WMS, usługi WMTS, krycie): każda grupa WMS pobierana osobnym
    ''' zapytaniem (nad podkładem - z przezroczystym tłem), grupa WMTS składana z kafli; wyniki nakładane z kryciem grup
    ''' i zapisywane w formacie sesji.
    ''' </summary>
    Private Shared Async Function ZlozSegmentAsync(z As ZadaniePobierania, grupy As List(Of GrupaZapytania),
                                                   uslugiWmts As Dictionary(Of String, ZrodloWmts), zasieg As Zasieg,
                                                   bok As Integer, plikDocelowy As String, token As CancellationToken) As Task(Of String)
        Dim wynik = Await ZlozWarstwyAsync(z, grupy, uslugiWmts, zasieg, bok, token).ConfigureAwait(False)
        If wynik.Item2 <> "" Then Return wynik.Item2
        Try
            Dim dane() As Byte = Await Task.Run(Function() ZrodloWmts.Koduj(NakladanieObrazow.DoBitmapy(wynik.Item1, bok, bok), z.Format, z.JakoscJpegWmts), token).ConfigureAwait(False)
            ZapiszPlik(plikDocelowy, dane)
            Return ""
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            Return ex.Message.Replace(vbCr, " ").Replace(vbLf, " ")
        End Try
    End Function

    ''' <summary>
    ''' Pobiera grupy warstw obszaru i nakłada je z kryciem. Wynik: (piksele ARGB wierszami, pusty tekst) albo (Nothing, błąd).
    ''' </summary>
    Friend Shared Async Function ZlozWarstwyAsync(z As ZadaniePobierania, grupy As List(Of GrupaZapytania),
                                                  uslugiWmts As Dictionary(Of String, ZrodloWmts), zasieg As Zasieg, bok As Integer,
                                                  token As CancellationToken) As Task(Of Tuple(Of Integer(), String))
        Dim szerokosc As Integer = bok, wysokosc As Integer = bok
        Dim piksele(szerokosc * wysokosc - 1) As Integer
        For i = 0 To grupy.Count - 1
            Dim g = grupy(i)
            If g.CzyKafelkowa Then
                'kafle WMTS/XYZ/TMS mają już przezroczystość pikseli - nakładane jak warstwy WMS
                Dim kafle = Await uslugiWmts(g.Adres).PikseleAsync(g.Warstwy, zasieg, bok, token).ConfigureAwait(False)
                If kafle.Item2 <> "" Then Return Tuple.Create(CType(Nothing, Integer()), "warstwa " & String.Join(",", g.Warstwy) & ": " & kafle.Item2)
                NakladanieWarstw.Naloz(piksele, kafle.Item1, g.Krycie, g.TrybMieszania)
                Continue For
            End If
            'warstwy nad podkładem, z innym trybem mieszania i warstwa częściowo przezroczysta - PNG z przezroczystym tłem (JPEG przykryłby niższe warstwy bielą)
            Dim przezroczyste As Boolean = i > 0 OrElse g.Krycie < ZestawWarstw.PelneKrycie OrElse g.TrybMieszania <> TrybMieszania.Zwykly OrElse z.Przezroczystosc
            Dim format As String = If(przezroczyste, "png", z.Format)
            Dim adres As String = Wms.ZapytanieGetMap(g.Adres, g.Warstwy, z.Uklad, zasieg, format, szerokosc, wysokosc, z.ZamienOsie, przezroczyste)
            Dim obraz = Await PobierzObrazAsync(adres, z.LimitCzasuSekundy, token, z.IgnorujSsl OrElse g.IgnorujSsl).ConfigureAwait(False)
            If obraz.Item2 <> "" Then Return Tuple.Create(CType(Nothing, Integer()), "warstwa " & String.Join(",", g.Warstwy) & ": " & obraz.Item2)
            NakladanieWarstw.Naloz(piksele, NakladanieObrazow.Piksele(obraz.Item1, szerokosc, wysokosc), g.Krycie, g.TrybMieszania)
        Next
        Return Tuple.Create(piksele, "")
    End Function

    ''' <summary>Czy dane są poprawnym obrazem (dekodowanie przez GDI+).</summary>
    Friend Shared Function CzyObraz(dane() As Byte) As Boolean
        If dane Is Nothing OrElse dane.Length = 0 Then Return False
        Try
            Using ms As New MemoryStream(dane)
                Using obraz As Image = Image.FromStream(ms)
                    Return obraz.Width > 0 AndAlso obraz.Height > 0
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    ''' <summary>Plik koordynaty.txt - wykaz segmentów z adresami i współrzędnymi (format jak w poprzednich wersjach).</summary>
    Private Shared Sub ZapiszKoordynaty(z As ZadaniePobierania, segmenty As List(Of OpisSegmentu),
                                        nazwa As Func(Of OpisSegmentu, String), adres As Func(Of OpisSegmentu, String))
        'zapis strumieniowy - przy milionach segmentów tekst całego pliku nie mieścił się w pamięci
        Using plik As New StreamWriter(z.Folder & "koordynaty.txt", False, KodowanieSystemowe())
            plik.WriteLine("""" & z.Folder & """")
            plik.WriteLine(z.Siatka.BokSegmentuPx.ToString(Globalization.CultureInfo.InvariantCulture))
            For Each sg In segmenty
                Dim zasieg = z.Siatka.Segment(sg.Wiersz, sg.Kolumna)
                plik.WriteLine(nazwa(sg))
                plik.WriteLine(MetadaneMapy.MaskujSekrety(adres(sg)))
                plik.WriteLine(Liczba(zasieg.YLewy))
                plik.WriteLine(Liczba(zasieg.XDol))
                plik.WriteLine(Liczba(zasieg.YPrawy))
                plik.WriteLine(Liczba(zasieg.XGora))
            Next
        End Using
    End Sub

End Class
