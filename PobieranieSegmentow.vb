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

    Public ReadOnly Property Rozszerzenie As String
        Get
            Return Wms.RozszerzeniePliku(Format)
        End Get
    End Property
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

    Friend Shared ReadOnly Klient As HttpClient = UtworzKlienta()

    Private Shared Function UtworzKlienta() As HttpClient
        Dim obsluga As New HttpClientHandler() With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate}
        'limit czasu ustawiany osobno dla każdego zapytania (LimitCzasu) - wartość z ustawień może się zmieniać
        Dim k As New HttpClient(obsluga) With {.Timeout = Threading.Timeout.InfiniteTimeSpan}
        k.DefaultRequestHeaders.UserAgent.ParseAdd("MapoTero/" & My.Application.Info.Version.ToString())
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

    ''' <summary>Opis segmentu siatki.</summary>
    Private Class OpisSegmentu
        Public Indeks As Integer
        Public Wiersz As Integer
        Public Kolumna As Integer
        Public Numer As Integer
        Public Nazwa As String = ""
        Public Zasieg As Zasieg
        Public Adres As String = ""
    End Class

    Public Shared Async Function PobierzAsync(z As ZadaniePobierania, postep As IProgress(Of PostepPobierania),
                                              token As CancellationToken) As Task(Of WynikPobierania)
        Dim s As Siatka = z.Siatka
        Dim ext As String = z.Rozszerzenie
        Dim wynik As New WynikPobierania()

        'usługa WMTS (adres w pliku warstw wskazuje WMTS) - segmenty składane z kafli usługi
        Dim kafleWmts As ZrodloWmts = Nothing
        If Wmts.CzyAdresWmts(z.AdresSerwera) Then
            Try
                kafleWmts = Await ZrodloWmts.PrzygotujAsync(z, token)
            Catch ex As OperationCanceledException
                wynik.Przerwano = True
                Return wynik
            End Try
            postep?.Report(New PostepPobierania With {.Komunikat = "Trwa pobieranie segmentów (" & s.LiczbaSegmentow & ") z usługi " & kafleWmts.Opis})
        End If

        'lista segmentów w kolejności wierszami od góry
        Dim segmenty As New List(Of OpisSegmentu)
        For w = 1 To s.LiczbaWierszy
            For k = 1 To s.LiczbaKolumn
                Dim sg As New OpisSegmentu With {.Indeks = segmenty.Count, .Wiersz = w, .Kolumna = k, .Numer = s.NumerKolejny(w, k),
                                                 .Zasieg = s.Segment(w, k)}
                sg.Nazwa = If(z.TrekBuddy, s.NazwaSegmentuTrekBuddy(z.Prefiks, w, k), s.NazwaSegmentu(z.Prefiks, z.Numeracja, w, k))
                If kafleWmts Is Nothing Then
                    sg.Adres = Wms.ZapytanieGetMap(z.AdresSerwera, z.Warstwy, z.Uklad, sg.Zasieg, z.Format, s.BokSegmentuPx, s.BokSegmentuPx, z.ZamienOsie)
                Else
                    sg.Adres = kafleWmts.AdresSegmentu(sg.Zasieg)
                End If
                segmenty.Add(sg)
            Next
        Next
        wynik.Wszystkie = segmenty.Count

        'foldery mapy TrekBuddy / Locus Map
        Dim folderDocelowy As String = z.Folder
        Dim tb As MapaTrekBuddy = Nothing
        If z.TrekBuddy Then
            tb = New MapaTrekBuddy(z)
            tb.PrzygotujFoldery()
            folderDocelowy = tb.FolderSet
        End If

        ZapiszKoordynaty(z, segmenty)

        'przy powtórnym pobieraniu - tylko segmenty o numerze wyższym niż ostatni istniejący
        Dim doPobrania As List(Of OpisSegmentu) = segmenty
        If z.PobierajPowyzejOstatniego AndAlso Not z.TrekBuddy Then
            Dim ostatni As Integer = 0
            For Each sg In segmenty
                If File.Exists(folderDocelowy & sg.Nazwa & "." & ext) Then ostatni = Math.Max(ostatni, sg.Numer)
            Next
            doPobrania = segmenty.FindAll(Function(sg) sg.Numer > ostatni)
        End If

        Dim plikBledow As String = z.Folder & "error.txt"
        Dim przyczyny As New ConcurrentDictionary(Of Integer, String)

        Try
            For proba = 1 To Math.Max(1, z.IloscProb)
                'plik error.txt istnieje w trakcie pobierania - sygnał niekompletnego zestawu dla narzędzia scalania
                File.WriteAllText(plikBledow, "")
                przyczyny.Clear()
                Dim licznik As Integer = 0
                Dim biezacaProba As Integer = proba

                Using semafor As New SemaphoreSlim(Math.Max(1, z.LiczbaWatkow))
                    Dim zadania As New List(Of Task)
                    For Each sg In doPobrania
                        Dim segment As OpisSegmentu = sg
                        zadania.Add(Task.Run(
                            Async Function()
                                Await semafor.WaitAsync(token).ConfigureAwait(False)
                                Try
                                    Dim plik As String = folderDocelowy & segment.Nazwa & "." & ext
                                    Dim blad As String = ""
                                    If Not File.Exists(plik) Then
                                        If kafleWmts Is Nothing Then
                                            blad = Await PobierzSegmentAsync(segment.Adres, plik, z.LimitCzasuSekundy, token).ConfigureAwait(False)
                                        Else
                                            blad = Await kafleWmts.PobierzSegmentAsync(segment.Zasieg, s.BokSegmentuPx, z.Format, plik, token).ConfigureAwait(False)
                                        End If
                                    End If
                                    If File.Exists(plik) Then
                                        If Not z.TrekBuddy AndAlso z.Georeferencja.Dowolna Then
                                            ZapisGeoreferencji.ZapiszPliki(ZapisGeoreferencji.Obraz(z.Uklad, segment.Zasieg, s.BokSegmentuPx, s.BokSegmentuPx,
                                                                                                   folderDocelowy, segment.Nazwa & "." & ext),
                                                                          folderDocelowy, segment.Nazwa, ext, z.Georeferencja)
                                        End If
                                    Else
                                        przyczyny(segment.Indeks) = blad
                                    End If
                                Finally
                                    semafor.Release()
                                    Dim n As Integer = Interlocked.Increment(licznik)
                                    postep?.Report(New PostepPobierania With {.Pobrane = n, .Wszystkie = doPobrania.Count, .Proba = biezacaProba, .Segment = segment.Nazwa})
                                End Try
                            End Function, token))
                    Next
                    Await Task.WhenAll(zadania)
                End Using

                If przyczyny.IsEmpty OrElse proba >= z.IloscProb Then Exit For

                postep?.Report(New PostepPobierania With {.Pobrane = 0, .Wszystkie = doPobrania.Count, .Proba = proba,
                    .Komunikat = "Nie pobrano " & przyczyny.Count & " segmentów. Kolejna próba (" & (proba + 1) & " z " & z.IloscProb & ") za " & z.PrzerwaSekundy & " s"})
                Await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, z.PrzerwaSekundy)), token)
            Next
        Catch ex As OperationCanceledException
            wynik.Przerwano = True
        End Try

        'wykaz brakujących segmentów (także po przerwaniu - scalanie ostrzeże o niekompletnym zestawie)
        Dim brakujace As New List(Of OpisSegmentu)
        For Each sg In doPobrania
            If Not File.Exists(folderDocelowy & sg.Nazwa & "." & ext) Then brakujace.Add(sg)
        Next
        wynik.Nieudane = brakujace.Count
        If brakujace.Count = 0 Then
            If File.Exists(plikBledow) Then File.Delete(plikBledow)
            'komplet segmentów - pobrane kafle WMTS nie są już potrzebne
            If kafleWmts IsNot Nothing AndAlso Not wynik.Przerwano AndAlso Not z.ZachowajKafleWmts Then kafleWmts.UsunKafle()
        Else
            Dim tekst As New Text.StringBuilder()
            For Each sg In brakujace
                tekst.AppendLine(sg.Nazwa)
                tekst.AppendLine(sg.Adres)
                Dim przyczyna As String = Nothing
                If przyczyny.TryGetValue(sg.Indeks, przyczyna) AndAlso przyczyna <> "" Then
                    tekst.AppendLine("przyczyna: " & przyczyna.Replace(""""c, "'"c))
                ElseIf wynik.Przerwano Then
                    tekst.AppendLine("przyczyna: przerwano pobieranie")
                End If
            Next
            File.WriteAllText(plikBledow, tekst.ToString(), KodowanieSystemowe())
        End If

        If tb IsNot Nothing Then
            tb.ZapiszListeSegmentow(segmenty.ConvertAll(Function(sg) sg.Nazwa & "." & ext))
            If Not wynik.Przerwano Then tb.UtworzArchiwumTar()
        End If

        Return wynik
    End Function

    ''' <summary>
    ''' Pobiera jeden segment i zapisuje go w pliku. Zwraca pusty tekst, gdy się udało, albo opis błędu
    ''' (np. komunikat serwera WMS), gdy segmentu nie pobrano.
    ''' </summary>
    Public Shared Async Function PobierzSegmentAsync(adres As String, plikDocelowy As String, limitCzasuSekundy As Integer,
                                                     token As CancellationToken) As Task(Of String)
        Try
            Using limit = LimitCzasu(limitCzasuSekundy, token),
                  odpowiedz = Await Klient.GetAsync(adres, HttpCompletionOption.ResponseContentRead, limit.Token).ConfigureAwait(False)
                Dim dane() As Byte = Await odpowiedz.Content.ReadAsByteArrayAsync().ConfigureAwait(False)
                Dim typ As String = If(odpowiedz.Content.Headers.ContentType?.MediaType, "")

                If Not CzyObraz(dane) Then
                    'serwer WMS w razie błędu zwraca zwykle dokument XML (ServiceException) zamiast obrazka
                    Dim tekst As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 500)).Replace(vbCr, " ").Replace(vbLf, " ")
                    If Not odpowiedz.IsSuccessStatusCode Then
                        Return "HTTP " & CInt(odpowiedz.StatusCode) & " " & odpowiedz.ReasonPhrase & ": " & tekst
                    End If
                    Return "serwer nie zwrócił obrazka (" & typ & "): " & tekst
                End If

                'zapisuje dokładnie to, co przysłał serwer - bez ponownej kompresji obrazka
                Dim tymczasowy As String = plikDocelowy & ".tmp"
                File.WriteAllBytes(tymczasowy, dane)
                If File.Exists(plikDocelowy) Then File.Delete(plikDocelowy)
                File.Move(tymczasowy, plikDocelowy)
                Return ""
            End Using
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As OperationCanceledException
            Return OpisLimituCzasu(limitCzasuSekundy)
        Catch ex As Exception
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return opis.Replace(vbCr, " ").Replace(vbLf, " ")
        End Try
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
    Private Shared Sub ZapiszKoordynaty(z As ZadaniePobierania, segmenty As List(Of OpisSegmentu))
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("""" & z.Folder & """")
        sb.AppendLine(z.Siatka.BokSegmentuPx.ToString(Globalization.CultureInfo.InvariantCulture))
        For Each sg In segmenty
            sb.AppendLine(sg.Nazwa)
            sb.AppendLine(sg.Adres)
            sb.AppendLine(Liczba(sg.Zasieg.YLewy))
            sb.AppendLine(Liczba(sg.Zasieg.XDol))
            sb.AppendLine(Liczba(sg.Zasieg.YPrawy))
            sb.AppendLine(Liczba(sg.Zasieg.XGora))
        Next
        File.WriteAllText(z.Folder & "koordynaty.txt", sb.ToString(), KodowanieSystemowe())
    End Sub

End Class
