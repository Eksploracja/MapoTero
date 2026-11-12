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
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Źródło segmentów z usługi WMTS. Usługa udostępnia gotowe kafle o stałych poziomach szczegółowości, więc segment
''' składany jest z kafli wybranego poziomu i przeliczany na siatkę segmentów (rozmiar piksela i układ współrzędnych
''' wybrane przez użytkownika). Dzięki temu segmenty WMTS mają te same pliki georeferencji co segmenty WMS i działają
''' z nimi scalanie, eksport i paczki TrekBuddy. Pobrane kafle przechowywane są w folderze _kafle_wmts sesji
''' (wspólne kafle sąsiednich segmentów pobierane są raz, ponowienie po błędzie nie pobiera ich ponownie);
''' folder jest usuwany po pobraniu wszystkich segmentów, chyba że w ustawieniach wybrano zachowywanie kafli.
''' </summary>
Public Class ZrodloWmts

    ''' <summary>Plany kolejnych warstw (kolejność rysowania jak w WMS - pierwsza warstwa na spodzie).</summary>
    Public ReadOnly Property Plany As New List(Of PlanWmts)
    'plany wg nazwy warstwy podanej w zbiorze map
    Private ReadOnly _planyWgNazwy As New Dictionary(Of String, PlanWmts)(StringComparer.Ordinal)
    Public ReadOnly Property FolderKafli As String

    'liczba kafli pobieranych jednocześnie dla jednego segmentu
    Private Const WatkiKafli As Integer = 4
    'największa mozaika kafli jednej warstwy segmentu (piksele)
    Private Const MaksMozaika As Long = 64L * 1024 * 1024

    'kafle pobierane w tej chwili - sąsiednie segmenty, pobierane równolegle, czekają na ten sam kafel zamiast pobierać go ponownie
    Private ReadOnly _wTrakcie As New ConcurrentDictionary(Of String, Lazy(Of Task(Of Tuple(Of Byte(), String))))
    'kafle puste (HTTP 404/204) zapamiętane wyłącznie na czas bieżącego pobierania, bez utrwalania 0 B na dysku dla XYZ/TMS
    Private ReadOnly _pusteKafle As New ConcurrentDictionary(Of String, Boolean)(StringComparer.Ordinal)
    'kafle z folderu sesji już zweryfikowane (lub pobrane) w bieżącym przebiegu - bez ponownej walidacji między sąsiednimi segmentami
    Private ReadOnly _zweryfikowaneWSesji As New ConcurrentDictionary(Of String, Boolean)(StringComparer.Ordinal)
    Private ReadOnly _opcjeWarstw As New Dictionary(Of String, OpcjeKafliXyz)(StringComparer.Ordinal)

    Private Const MaksRundUzupelniania As Integer = 2
    'kafle podkładu XYZ/TMS, których serwer nie ma (HTTP 404) - zastępowane powiększonym fragmentem kafla z poziomu wyżej
    Private ReadOnly _kafle404 As New ConcurrentDictionary(Of String, Boolean)(StringComparer.Ordinal)
    'ile poziomów wyżej szukać kafla zastępczego; dalej brak kafli oznacza obszar bez danych
    Private Const MaksPoziomowZastepstwa As Integer = 4

    Private ReadOnly _limitCzasu As Integer
    Private ReadOnly _jakoscJpeg As Integer
    Private ReadOnly _czyKafleXyz As Boolean
    Private ReadOnly _ignorujSsl As Boolean
    Private ReadOnly _progWalidacjiCache As TimeSpan
    Private ReadOnly _kluczSerwera As String = ""
    Private ReadOnly _referer As String = ""
    Private _udaneObrazki As Integer

    Private NotInheritable Class DaneCacheMeta
        Public CheckedUtc As DateTime
        Public ETag As String = ""
        Public LastModified As String = ""
    End Class

    Private Sub New(folderKafli As String, limitCzasuSekundy As Integer, jakoscJpeg As Integer,
                    Optional czyKafleXyz As Boolean = False, Optional kluczSerwera As String = "",
                    Optional referer As String = "", Optional ignorujSsl As Boolean = False,
                    Optional progWalidacjiCache As TimeSpan? = Nothing)
        Me.FolderKafli = folderKafli
        _limitCzasu = limitCzasuSekundy
        _jakoscJpeg = jakoscJpeg
        _czyKafleXyz = czyKafleXyz
        _kluczSerwera = If(kluczSerwera, "")
        _referer = If(referer, "")
        _ignorujSsl = ignorujSsl
        _progWalidacjiCache = If(progWalidacjiCache, TimeSpan.FromMinutes(15))
    End Sub

    ''' <summary>Odczyt dokumentu GetCapabilities (WMTS) albo budowa syntetycznej usługi XYZ/TMS i wybór poziomu kafli.</summary>
    Public Shared Async Function PrzygotujAsync(z As ZadaniePobierania, adresUslugi As String, nazwy As IEnumerable(Of String),
                                                token As CancellationToken, Optional folderKafli As String = Nothing,
                                                Optional typUslugi As TypUslugiMapowej = TypUslugiMapowej.Nieznana,
                                                Optional plikWarstw As PlikWarstw = Nothing) As Task(Of ZrodloWmts)
        Dim folderDocelowy As String = If(folderKafli, z.Folder & "_kafle_wmts\")
        Dim czyIgnorujSsl As Boolean = z.IgnorujSsl OrElse If(plikWarstw IsNot Nothing, plikWarstw.IgnorujSsl, False)
        Dim progCache As TimeSpan = TimeSpan.FromSeconds(Math.Max(0, z.WiekWalidacjiCacheSekundy))
        Dim efektywnyTyp As TypUslugiMapowej = KafleXyz.WybierzTyp(adresUslugi, typUslugi)
        If efektywnyTyp = TypUslugiMapowej.Xyz OrElse efektywnyTyp = TypUslugiMapowej.Tms Then
            Return Await PrzygotujXyzTmsAsync(z, adresUslugi, nazwy, efektywnyTyp, plikWarstw, czyIgnorujSsl, progCache, folderDocelowy, token).ConfigureAwait(False)
        End If

        Dim adres As String = Wmts.AdresCapabilities(adresUslugi)
        Dim dane() As Byte = Nothing
        For proba = 1 To Math.Max(1, z.IloscProb)
            Try
                Using limit = PobieranieSegmentow.LimitCzasu(z.LimitCzasuSekundy, token),
                      odpowiedz = Await PobieranieSegmentow.WybierzKlienta(czyIgnorujSsl).GetAsync(adres, limit.Token).ConfigureAwait(False)
                    odpowiedz.EnsureSuccessStatusCode()
                    dane = Await odpowiedz.Content.ReadAsByteArrayAsync(limit.Token).ConfigureAwait(False)
                End Using
                Exit For
            Catch ex As OperationCanceledException When token.IsCancellationRequested
                Throw
            Catch ex As OperationCanceledException When proba >= z.IloscProb
                Throw New InvalidOperationException("nie udało się pobrać opisu usługi WMTS (" & MetadaneMapy.MaskujSekrety(adres) & "): " &
                                                    PobieranieSegmentow.OpisLimituCzasu(z.LimitCzasuSekundy), ex)
            Catch ex As Exception When proba < z.IloscProb
            Catch ex As Exception
                Throw New InvalidOperationException("nie udało się pobrać opisu usługi WMTS (" & MetadaneMapy.MaskujSekrety(adres) & "): " & ex.Message, ex)
            End Try
            Await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, z.PrzerwaSekundy)), token).ConfigureAwait(False)
        Next

        Dim usluga As UslugaWmts
        Try
            'dokument odczytywany wg deklaracji encoding w XML (ReadAsStringAsync brał kodowanie z nagłówka HTTP albo UTF-8,
            'co przekłamywało polskie znaki w dokumentach ISO-8859-2 - także w identyfikatorach warstw)
            Dim tekstXml As String
            Using ms As New MemoryStream(If(dane, New Byte() {}))
                tekstXml = System.Xml.Linq.XDocument.Load(ms).ToString(System.Xml.Linq.SaveOptions.DisableFormatting)
            End Using
            usluga = UslugaWmts.Wczytaj(tekstXml, adres)
        Catch ex As Exception
            Throw New InvalidOperationException("niepoprawny opis usługi WMTS (" & MetadaneMapy.MaskujSekrety(adres) & "): " & ex.Message, ex)
        End Try
        'wspólny folder kafli wszystkich usług sesji - nazwa kafla to skrót jego adresu
        Dim zrodlo As New ZrodloWmts(folderDocelowy, z.LimitCzasuSekundy, Math.Max(1, Math.Min(100, z.JakoscJpegWmts)),
                                     ignorujSsl:=czyIgnorujSsl, progWalidacjiCache:=progCache)
        For Each nazwa In nazwy
            If zrodlo._planyWgNazwy.ContainsKey(nazwa) Then Continue For
            Dim plan As New PlanWmts(usluga, nazwa, z.Uklad, z.Siatka.ZasiegSiatki, z.Siatka.RozmiarPiksela, z.Format)
            zrodlo.Plany.Add(plan)
            zrodlo._planyWgNazwy(nazwa) = plan
        Next
        If zrodlo.Plany.Count = 0 Then Throw New InvalidOperationException("nie wybrano warstwy WMTS")
        Directory.CreateDirectory(zrodlo.FolderKafli)
        Return zrodlo
    End Function

    Private Shared Async Function PrzygotujXyzTmsAsync(z As ZadaniePobierania, adresUslugi As String, nazwy As IEnumerable(Of String),
                                                       efektywnyTyp As TypUslugiMapowej, plikWarstw As PlikWarstw,
                                                       czyIgnorujSsl As Boolean, progCache As TimeSpan,
                                                       folderDocelowy As String, token As CancellationToken) As Task(Of ZrodloWmts)
        Dim listaNazw = If(nazwy, Array.Empty(Of String)()).Where(Function(n) Not String.IsNullOrEmpty(n)).Distinct(StringComparer.Ordinal).ToList()
        If listaNazw.Count = 0 Then Throw New InvalidOperationException("nie wybrano warstwy kafelkowej XYZ/TMS")

        Dim uslugaZbioru = KafleXyz.UtworzUslugeZbioru(adresUslugi, listaNazw, plikWarstw, efektywnyTyp)
        Dim opcjeBazowe As OpcjeKafliXyz = uslugaZbioru.OpcjeBazowe
        Dim opcjeWarstw = uslugaZbioru.OpcjeWarstw
        Dim usluga As UslugaWmts = uslugaZbioru.Usluga
        Dim kluczSerwera As String = KafleXyz.KluczSerwera(adresUslugi)
        Dim zrodlo As New ZrodloWmts(folderDocelowy, z.LimitCzasuSekundy, Math.Max(1, Math.Min(100, z.JakoscJpegWmts)),
                                     czyKafleXyz:=True, kluczSerwera:=kluczSerwera, referer:=opcjeBazowe.Referer,
                                     ignorujSsl:=czyIgnorujSsl, progWalidacjiCache:=progCache)
        For Each nazwa In listaNazw
            Dim plan As New PlanWmts(usluga, nazwa, z.Uklad, z.Siatka.ZasiegSiatki, z.Siatka.RozmiarPiksela, z.Format)
            zrodlo.Plany.Add(plan)
            zrodlo._planyWgNazwy(nazwa) = plan
            zrodlo._opcjeWarstw(nazwa) = opcjeWarstw(nazwa)
        Next
        Directory.CreateDirectory(zrodlo.FolderKafli)

        'dla map podkładowych XYZ/TMS wykonaj szybki test środkowego kafla obszaru przed rozpoczęciem pobierania segmentów,
        'żeby martwy serwer albo zły szablon zawiódł od razu zamiast tworzyć białe segmenty
        For Each plan In zrodlo.Plany
            Dim opcjePlanu = opcjeWarstw(plan.Warstwa.Identyfikator)
            If opcjePlanu.Nakladka Then Continue For
            Dim testowe = WybierzTestoweKafle(plan, z.Siatka)
            If testowe.Count = 0 Then Continue For
            Dim pierwszyBlad As String = ""
            Dim trafiono As Boolean = False
            For Each wk In testowe
                Dim adresTest = plan.AdresKafla(wk.Item1, wk.Item2)
                Dim kluczTest = plan.KluczKafla(wk.Item1, wk.Item2)
                Dim wynikTest = Await zrodlo.PobierzKafelAsync(adresTest, kluczTest, opcjePlanu, token).ConfigureAwait(False)
                If wynikTest.Item2 = "" Then
                    trafiono = True
                    Exit For
                End If
                If pierwszyBlad = "" Then pierwszyBlad = wynikTest.Item2
            Next
            If Not trafiono AndAlso pierwszyBlad <> "" Then
                Throw New InvalidOperationException("nie można pobrać mapy kafelkowej (" & MetadaneMapy.MaskujSekrety(adresUslugi) & "): " & pierwszyBlad)
            End If
        Next

        Return zrodlo
    End Function

    Private Shared Function WybierzTestoweKafle(plan As PlanWmts, siatka As Siatka) As List(Of Tuple(Of Integer, Integer))
        Dim wynik As New List(Of Tuple(Of Integer, Integer))
        If siatka Is Nothing OrElse siatka.LiczbaWierszy <= 0 OrElse siatka.LiczbaKolumn <= 0 Then Return wynik
        Dim sw As Integer = Math.Max(1, (siatka.LiczbaWierszy + 1) \ 2)
        Dim sk As Integer = Math.Max(1, (siatka.LiczbaKolumn + 1) \ 2)
        Dim m = PrzygotujMozaike(plan, siatka.Segment(sw, sk), siatka.BokSegmentuPx)
        If m.Szerokosc <= 0 Then Return wynik
        Dim wMid As Integer = (m.Wiersz0 + m.Wiersz1) \ 2
        Dim kMid As Integer = (m.Kolumna0 + m.Kolumna1) \ 2
        Dim kandydaci = {
            Tuple.Create(wMid, kMid),
            Tuple.Create(m.Wiersz0, m.Kolumna0),
            Tuple.Create(m.Wiersz1, m.Kolumna1)
        }
        For Each cand In kandydaci
            If cand.Item1 < m.Wiersz0 OrElse cand.Item1 > m.Wiersz1 OrElse cand.Item2 < m.Kolumna0 OrElse cand.Item2 > m.Kolumna1 Then Continue For
            If Not plan.KafelDostepny(cand.Item1, cand.Item2) Then Continue For
            If Not wynik.Any(Function(p) p.Item1 = cand.Item1 AndAlso p.Item2 = cand.Item2) Then wynik.Add(cand)
        Next
        Return wynik
    End Function

    ''' <summary>Opis wybranych poziomów kafli.</summary>
    Public ReadOnly Property Opis As String
        Get
            Return String.Join("; ", Plany.ConvertAll(Function(p) p.Opis))
        End Get
    End Property

    ''' <summary>Adres kafla w lewym górnym narożniku segmentu (do wykazów koordynaty.txt i error.txt).</summary>
    Public Function AdresSegmentu(zasieg As Zasieg) As String
        Dim p = Plany(0)
        Dim kol As Double, wie As Double
        p.PikselMacierzy(p.DoUkladuKafli(New PunktXY(zasieg.XGora, zasieg.YLewy)), kol, wie)
        Return p.AdresKafla(CInt(Math.Floor(wie / p.Macierz.WysokoscKafla)), CInt(Math.Floor(kol / p.Macierz.SzerokoscKafla)))
    End Function

    ''' <summary>Usuwa folder pobranych kafli.</summary>
    Public Sub UsunKafle()
        Try
            If Directory.Exists(FolderKafli) Then Directory.Delete(FolderKafli, True)
        Catch
            'folder może być chwilowo zablokowany (np. przez program antywirusowy) - pozostaje do usunięcia ręcznie
        End Try
    End Sub

    ''' <summary>Mozaika kafli jednej warstwy obejmująca segment.</summary>
    Private Class Mozaika
        Public Plan As PlanWmts
        ''' <summary>Krycie warstwy (0-1).</summary>
        Public Krycie As Double = 1
        Public Przeliczenia As SiatkaPrzeliczen
        Public Kolumna0 As Integer
        Public Wiersz0 As Integer
        Public Kolumna1 As Integer
        Public Wiersz1 As Integer
        Public Szerokosc As Integer
        Public Wysokosc As Integer
        ''' <summary>Piksele ARGB (wierszami); Nothing - segment poza zasięgiem kafli.</summary>
        Public Piksele() As Integer
    End Class

    ''' <summary>
    ''' Piksele segmentu (ARGB wierszami) złożone z kafli wskazanych warstw usługi, nałożonych w podanej kolejności.
    ''' Wynik: (piksele, pusty tekst) albo (Nothing, opis błędu).
    ''' </summary>
    Public Async Function PikseleAsync(nazwy As IEnumerable(Of String), zasieg As Zasieg, bok As Integer,
                                       token As CancellationToken) As Task(Of Tuple(Of Integer(), String))
        Try
            Dim mozaiki As New List(Of Mozaika)
            For Each nazwa In nazwy
                Dim plan As PlanWmts = Nothing
                If Not _planyWgNazwy.TryGetValue(nazwa, plan) Then Return Tuple.Create(CType(Nothing, Integer()), "brak warstwy " & nazwa & " w usłudze WMTS")
                Dim m = PrzygotujMozaike(plan, zasieg, bok)
                Dim blad As String = Await WypelnijMozaikeAsync(m, token).ConfigureAwait(False)
                If blad <> "" Then Return Tuple.Create(CType(Nothing, Integer()), blad)
                mozaiki.Add(m)
            Next
            token.ThrowIfCancellationRequested()
            Dim piksele() As Integer = Await Task.Run(Function() RenderujPiksele(mozaiki, bok), token).ConfigureAwait(False)
            Return Tuple.Create(piksele, "")
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            Throw
        Catch ex As Exception
            Dim opis As String = ex.Message
            If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
            Return Tuple.Create(CType(Nothing, Integer()), opis.Replace(vbCr, " ").Replace(vbLf, " "))
        End Try
    End Function

    ''' <summary>Zakres kafli potrzebnych dla segmentu (z marginesem piksela na interpolację).</summary>
    Private Shared Function PrzygotujMozaike(plan As PlanWmts, zasieg As Zasieg, bok As Integer) As Mozaika
        Dim px As Double = (zasieg.YPrawy - zasieg.YLewy) / bok
        Dim py As Double = (zasieg.XGora - zasieg.XDol) / bok
        'siatka przeliczeń dla narożników pikseli segmentu: (0, 0) - lewy górny narożnik segmentu
        Dim siatka As New SiatkaPrzeliczen(bok, bok, 16,
            Function(x As Double, y As Double)
                Dim p = plan.DoUkladuKafli(New PunktXY(zasieg.XGora - y * py, zasieg.YLewy + x * px))
                Dim kol As Double, wie As Double
                plan.PikselMacierzy(p, kol, wie)
                Return New PunktXY(wie, kol)
            End Function)

        'skrajne położenia w macierzy - wzdłuż krawędzi segmentu
        Dim minK As Double = Double.MaxValue, maksK As Double = Double.MinValue
        Dim minW As Double = Double.MaxValue, maksW As Double = Double.MinValue
        Dim krawedz = Sub(x As Double, y As Double)
                          Dim p = siatka.Punkt(x, y)
                          minW = Math.Min(minW, p.X) : maksW = Math.Max(maksW, p.X)
                          minK = Math.Min(minK, p.Y) : maksK = Math.Max(maksK, p.Y)
                      End Sub
        For i = 0 To bok Step 16
            krawedz(i, 0) : krawedz(i, bok) : krawedz(0, i) : krawedz(bok, i)
        Next
        krawedz(bok, bok)

        Dim mc = plan.Macierz
        Dim m As New Mozaika With {.Plan = plan, .Przeliczenia = siatka}
        m.Kolumna0 = CInt(Math.Max(0, Math.Floor((minK - 2) / mc.SzerokoscKafla)))
        m.Kolumna1 = CInt(Math.Min(mc.LiczbaKolumn - 1, Math.Floor((maksK + 2) / mc.SzerokoscKafla)))
        m.Wiersz0 = CInt(Math.Max(0, Math.Floor((minW - 2) / mc.WysokoscKafla)))
        m.Wiersz1 = CInt(Math.Min(mc.LiczbaWierszy - 1, Math.Floor((maksW + 2) / mc.WysokoscKafla)))
        If m.Kolumna1 >= m.Kolumna0 AndAlso m.Wiersz1 >= m.Wiersz0 Then
            m.Szerokosc = (m.Kolumna1 - m.Kolumna0 + 1) * mc.SzerokoscKafla
            m.Wysokosc = (m.Wiersz1 - m.Wiersz0 + 1) * mc.WysokoscKafla
            If CLng(m.Szerokosc) * m.Wysokosc > MaksMozaika Then
                Throw New InvalidOperationException("segment wymaga zbyt wielu kafli WMTS - zmniejsz bok segmentu")
            End If
        End If
        Return m
    End Function

    Private Shared Function KluczPozycji(wiersz As Integer, kolumna As Integer) As Long
        Return CLng(wiersz) * 100000000L + kolumna
    End Function

    Private Shared Function CzyBladNiepowtarzalnyFormatu(opis As String) As Boolean
        If String.IsNullOrEmpty(opis) Then Return False
        Return opis.Contains("MVT/PBF") OrElse opis.Contains("WebP")
    End Function

    ''' <summary>Pobiera wszystkie wymagane kafle cząstkowe (z ponownymi próbami uzupełnienia braków) i dopiero po weryfikacji kompletności składa je w mozaikę.</summary>
    Private Async Function WypelnijMozaikeAsync(m As Mozaika, token As CancellationToken) As Task(Of String)
        If m.Szerokosc = 0 Then Return ""
        Dim mc = m.Plan.Macierz
        Dim wymagane As New List(Of Tuple(Of Integer, Integer))
        For w = m.Wiersz0 To m.Wiersz1
            For k = m.Kolumna0 To m.Kolumna1
                If m.Plan.KafelDostepny(w, k) Then wymagane.Add(Tuple.Create(w, k))
            Next
        Next
        If wymagane.Count = 0 Then Return ""

        Dim opcje As OpcjeKafliXyz = Nothing
        _opcjeWarstw.TryGetValue(m.Plan.Warstwa.Identyfikator, opcje)
        Dim kafle As New ConcurrentDictionary(Of Long, Byte())
        Dim bledy As New ConcurrentDictionary(Of Long, String)

        For runda = 1 To MaksRundUzupelniania
            Dim doPobrania = wymagane.Where(Function(wk) Not kafle.ContainsKey(KluczPozycji(wk.Item1, wk.Item2))).ToList()
            If doPobrania.Count = 0 Then Exit For
            If runda > 1 Then
                Await Task.Delay(TimeSpan.FromMilliseconds(350 * (runda - 1)), token).ConfigureAwait(False)
            End If

            Using semafor As New SemaphoreSlim(WatkiKafli)
                Dim zadania As New List(Of Task)
                For Each wk In doPobrania
                    Dim wiersz As Integer = wk.Item1, kolumna As Integer = wk.Item2
                    zadania.Add(Task.Run(
                        Async Function()
                            Await semafor.WaitAsync(token).ConfigureAwait(False)
                            Try
                                Dim adres As String = m.Plan.AdresKafla(wiersz, kolumna)
                                Dim klucz As String = m.Plan.KluczKafla(wiersz, kolumna)
                                Dim poz As Long = KluczPozycji(wiersz, kolumna)
                                Dim wynik = Await PobierzKafelAsync(adres, klucz, opcje, token).ConfigureAwait(False)
                                If wynik.Item2 <> "" Then
                                    bledy(poz) = "kafel " & MetadaneMapy.MaskujSekrety(adres) & ": " & wynik.Item2
                                ElseIf wynik.Item1 IsNot Nothing AndAlso wynik.Item1.Length > 0 AndAlso Not PobieranieSegmentow.CzyObraz(wynik.Item1) Then
                                    'uszkodzony kafel z dysku lub niepełna odpowiedź - usuń z dysku i ponów w kolejnej rundzie uzupełniania
                                    Dim plikUszk = Path.Combine(FolderKafli, NazwaKafla(klucz))
                                    UsunPlikSafely(plikUszk)
                                    UsunPlikSafely(plikUszk & ".meta")
                                    Dim usuniete As Boolean = False
                                    _zweryfikowaneWSesji.TryRemove(klucz, usuniete)
                                    bledy(poz) = "kafel " & MetadaneMapy.MaskujSekrety(adres) & ": uszkodzony obraz kafla"
                                Else
                                    kafle(poz) = If(wynik.Item1, New Byte() {})
                                    Dim staryBlad As String = Nothing
                                    bledy.TryRemove(poz, staryBlad)
                                End If
                            Finally
                                semafor.Release()
                            End Try
                        End Function, token))
                Next
                Await Task.WhenAll(zadania).ConfigureAwait(False)
            End Using

            Dim brakPoRundzie = wymagane.Where(Function(wk) Not kafle.ContainsKey(KluczPozycji(wk.Item1, wk.Item2))).ToList()
            If brakPoRundzie.Count = 0 Then Exit For
            If bledy.Count > 0 AndAlso bledy.Values.All(AddressOf CzyBladNiepowtarzalnyFormatu) Then Exit For
        Next

        'bramka kompletności przed scaleniem kafli cząstkowych w finalny segment
        Dim brakujace = wymagane.Where(Function(wk) Not kafle.ContainsKey(KluczPozycji(wk.Item1, wk.Item2))).ToList()
        If brakujace.Count > 0 Then
            Dim listaBledow = bledy.Values.Where(Function(x) Not String.IsNullOrEmpty(x)).ToList()
            If listaBledow.Count > 0 Then
                Return "nie pobrano wszystkich kafli cząstkowych (" & brakujace.Count & " z " & wymagane.Count & "): " &
                       listaBledow(0) & If(listaBledow.Count > 1, " (oraz " & (listaBledow.Count - 1) & " innych kafli)", "")
            End If
            Return "niekompletny zestaw kafli cząstkowych (" & brakujace.Count & " brakujących z " & wymagane.Count & ")"
        End If

        'podkład XYZ/TMS: kafle, których serwer nie ma (HTTP 404, zwykle dziury na najwyższym poziomie powiększenia),
        'zastąp powiększonym fragmentem kafla z poziomu wyżej zamiast zostawiać białe pola
        Dim zastepcze As New Dictionary(Of Long, Tuple(Of Byte(), KafelNadrzednyWmts))
        If _czyKafleXyz AndAlso (opcje Is Nothing OrElse Not opcje.Nakladka) Then
            For Each para In kafle.ToList()
                If para.Value IsNot Nothing AndAlso para.Value.Length > 0 Then Continue For
                Dim w As Integer = CInt(para.Key \ 100000000L), k As Integer = CInt(para.Key Mod 100000000L)
                If Not _kafle404.ContainsKey(m.Plan.KluczKafla(w, k)) Then Continue For
                For poziomy = 1 To MaksPoziomowZastepstwa
                    Dim nad = m.Plan.KafelNadrzedny(w, k, poziomy)
                    If nad Is Nothing Then Exit For
                    Dim wynikNad = Await PobierzKafelAsync(nad.Adres, nad.Klucz, opcje, token).ConfigureAwait(False)
                    If wynikNad.Item2 <> "" Then
                        If _kafle404.ContainsKey(nad.Klucz) Then Continue For
                        Return "kafel zastępczy " & MetadaneMapy.MaskujSekrety(nad.Adres) & ": " & wynikNad.Item2
                    End If
                    If wynikNad.Item1 IsNot Nothing AndAlso wynikNad.Item1.Length > 0 AndAlso PobieranieSegmentow.CzyObraz(wynikNad.Item1) Then
                        zastepcze(para.Key) = Tuple.Create(wynikNad.Item1, nad)
                        Exit For
                    End If
                Next
            Next
        End If

        Using obraz As New Bitmap(m.Szerokosc, m.Wysokosc, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(obraz)
                g.Clear(Color.Transparent)
                g.CompositingMode = CompositingMode.SourceCopy
                g.PixelOffsetMode = PixelOffsetMode.Half
                For Each para In kafle
                    Dim w As Integer = CInt(para.Key \ 100000000L), k As Integer = CInt(para.Key Mod 100000000L)
                    Dim zast As Tuple(Of Byte(), KafelNadrzednyWmts) = Nothing
                    If (para.Value Is Nothing OrElse para.Value.Length = 0) AndAlso zastepcze.TryGetValue(para.Key, zast) Then
                        RysujFragmentKafla(g, zast.Item1, zast.Item2, New Rectangle((k - m.Kolumna0) * mc.SzerokoscKafla, (w - m.Wiersz0) * mc.WysokoscKafla,
                                                                                    mc.SzerokoscKafla, mc.WysokoscKafla))
                        Continue For
                    End If
                    If para.Value Is Nothing OrElse para.Value.Length = 0 Then Continue For
                    Using ms As New MemoryStream(para.Value)
                        Using kafel As Image = Image.FromStream(ms)
                            g.InterpolationMode = If(kafel.Width = mc.SzerokoscKafla AndAlso kafel.Height = mc.WysokoscKafla,
                                                     InterpolationMode.NearestNeighbor, InterpolationMode.HighQualityBicubic)
                            g.DrawImage(kafel, New Rectangle((k - m.Kolumna0) * mc.SzerokoscKafla, (w - m.Wiersz0) * mc.WysokoscKafla,
                                                             mc.SzerokoscKafla, mc.WysokoscKafla), 0, 0, kafel.Width, kafel.Height, GraphicsUnit.Pixel)
                        End Using
                    End Using
                Next
            End Using
            ReDim m.Piksele(m.Szerokosc * m.Wysokosc - 1)
            Dim d = obraz.LockBits(New Rectangle(0, 0, m.Szerokosc, m.Wysokosc), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            Try
                For y = 0 To m.Wysokosc - 1
                    Marshal.Copy(IntPtr.Add(d.Scan0, y * d.Stride), m.Piksele, y * m.Szerokosc, m.Szerokosc)
                Next
            Finally
                obraz.UnlockBits(d)
            End Try
        End Using
        Return ""
    End Function

    ''' <summary>Rysuje powiększoną część kafla nadrzędnego w miejscu brakującego kafla.</summary>
    Private Shared Sub RysujFragmentKafla(g As Graphics, dane As Byte(), nad As KafelNadrzednyWmts, cel As Rectangle)
        Using ms As New MemoryStream(dane)
            Using kafel As Image = Image.FromStream(ms)
                Dim czesci As Integer = 1 << nad.Poziomy
                Dim bokW As Single = CSng(kafel.Width) / czesci
                Dim bokH As Single = CSng(kafel.Height) / czesci
                'TileFlipXY: interpolacja na brzegu wycinka nie miesza pikseli spoza niego (bez ciemnej obwódki)
                Using atr As New ImageAttributes()
                    atr.SetWrapMode(WrapMode.TileFlipXY)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.DrawImage(kafel, cel, nad.CzescKolumna * bokW, nad.CzescWiersz * bokH, bokW, bokH, GraphicsUnit.Pixel, atr)
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Kafel z folderu kafli albo z serwera. Wynik: (dane, błąd); dane puste - kafla nie ma
    ''' (serwery WMTS zgłaszają w ten sposób kafle poza obszarem danych: HTTP 404 lub 204).
    ''' </summary>
    Private Async Function PobierzKafelAsync(adres As String, klucz As String, opcje As OpcjeKafliXyz,
                                             token As CancellationToken) As Task(Of Tuple(Of Byte(), String))
        If _pusteKafle.ContainsKey(klucz) Then Return Tuple.Create(New Byte() {}, "")
        Dim plik As String = Path.Combine(FolderKafli, NazwaKafla(klucz))
        Dim plikMeta As String = plik & ".meta"
        Dim metaWalidacji As DaneCacheMeta = Nothing
        Dim bajtyWalidacji() As Byte = Nothing

        If File.Exists(plik) Then
            Dim bajty = File.ReadAllBytes(plik)
            If bajty.Length > 0 Then
                If Not PobieranieSegmentow.CzyObraz(bajty) Then
                    'uszkodzony plik w folderze kafli - usuń i pobierz na nowo
                    UsunPlikSafely(plik)
                    UsunPlikSafely(plikMeta)
                    Dim usuniety As Boolean = False
                    _zweryfikowaneWSesji.TryRemove(klucz, usuniety)
                ElseIf _zweryfikowaneWSesji.ContainsKey(klucz) Then
                    Interlocked.Increment(_udaneObrazki)
                    Return Tuple.Create(bajty, "")
                Else
                    Dim meta = OdczytajMeta(plikMeta)
                    Dim maWalidator As Boolean = meta IsNot Nothing AndAlso (meta.ETag <> "" OrElse meta.LastModified <> "")
                    Dim wygasl As Boolean = maWalidator AndAlso (DateTime.UtcNow - meta.CheckedUtc >= _progWalidacjiCache)
                    If Not wygasl Then
                        _zweryfikowaneWSesji(klucz) = True
                        Interlocked.Increment(_udaneObrazki)
                        Return Tuple.Create(bajty, "")
                    End If
                    metaWalidacji = meta
                    bajtyWalidacji = bajty
                End If
            Else
                'dla XYZ/TMS ignoruj ewentualny pusty plik 0 B z poprzednich uruchomień dla podkładu
                If Not _czyKafleXyz OrElse (opcje IsNot Nothing AndAlso opcje.Nakladka) Then
                    Return Tuple.Create(bajty, "")
                End If
                UsunPlikSafely(plik)
                UsunPlikSafely(plikMeta)
            End If
        End If
        'token pobierania (wspólny dla wszystkich segmentów) przerywa tylko oczekiwanie na przepustkę regulatora
        Dim zadanie = _wTrakcie.GetOrAdd(klucz, Function(k) New Lazy(Of Task(Of Tuple(Of Byte(), String)))(
                                             Function() PobierzKafelZSerweraAsync(adres, klucz, plik, opcje, token, metaWalidacji, bajtyWalidacji)))
        Try
            Return Await zadanie.Value.WaitAsync(token).ConfigureAwait(False)
        Finally
            'kafel pobrany jest już w folderze kafli; nieudany - przy kolejnej próbie pobierany od nowa
            Dim usuniete As Lazy(Of Task(Of Tuple(Of Byte(), String))) = Nothing
            _wTrakcie.TryRemove(klucz, usuniete)
        End Try
    End Function

    Private Async Function PobierzKafelZSerweraAsync(adres As String, klucz As String, plik As String,
                                                      opcje As OpcjeKafliXyz, token As CancellationToken,
                                                      Optional metaCache As DaneCacheMeta = Nothing,
                                                      Optional bajtyCache() As Byte = Nothing) As Task(Of Tuple(Of Byte(), String))
        Dim maksProb As Integer = If(_czyKafleXyz, 3, 1)
        Dim maksWatkow As Integer = If(opcje IsNot Nothing, opcje.MaksWatkow, 0)
        Dim odstepMs As Integer = If(opcje IsNot Nothing, opcje.OdstepMs, 0)
        Dim referer As String = If(opcje IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(opcje.Referer), opcje.Referer, _referer)
        Dim jestNakladka As Boolean = opcje IsNot Nothing AndAlso opcje.Nakladka
        Dim kluczRegulatora As String = If(_czyKafleXyz AndAlso _kluczSerwera <> "", _kluczSerwera, Nothing)
        Dim plikMeta As String = plik & ".meta"
        Dim klientHttp = PobieranieSegmentow.WybierzKlienta(_ignorujSsl)

        For proba = 1 To maksProb
            'regulacja obciążenia serwera: łącznie z kaflami innych segmentów najwyżej tyle zapytań naraz, ile wątków pobierania
            Dim przepustka As RegulatorZapytan.Przepustka = Nothing
            Dim ponowOpoznienieMs As Integer = 0
            Try
                przepustka = Await RegulatorZapytan.Pobieranie.WejdzAsync(adres, token, kluczRegulatora, maksWatkow, odstepMs).ConfigureAwait(False)
                Using req As New HttpRequestMessage(HttpMethod.Get, adres)
                    Dim refUri As Uri = Nothing
                    If Not String.IsNullOrWhiteSpace(referer) AndAlso Uri.TryCreate(referer, UriKind.Absolute, refUri) Then
                        req.Headers.Referrer = refUri
                    End If
                    If metaCache IsNot Nothing Then
                        If metaCache.ETag <> "" Then req.Headers.TryAddWithoutValidation("If-None-Match", metaCache.ETag)
                        If metaCache.LastModified <> "" Then req.Headers.TryAddWithoutValidation("If-Modified-Since", metaCache.LastModified)
                    End If
                    Using limit = PobieranieSegmentow.LimitCzasu(_limitCzasu, CancellationToken.None),
                          odpowiedz = Await klientHttp.SendAsync(req, HttpCompletionOption.ResponseContentRead, limit.Token).ConfigureAwait(False)
                        przepustka.Odpowiedz(odpowiedz)

                        If odpowiedz.StatusCode = HttpStatusCode.NotModified AndAlso bajtyCache IsNot Nothing AndAlso bajtyCache.Length > 0 Then
                            przepustka.Dispose()
                            Dim nowyEtag = If(odpowiedz.Headers.ETag IsNot Nothing, odpowiedz.Headers.ETag.ToString(), If(metaCache?.ETag, ""))
                            Dim nowyLm = PobierzLastModified(odpowiedz, If(metaCache?.LastModified, ""))
                            ZapiszMetaAtomowo(plikMeta, New DaneCacheMeta With {.CheckedUtc = DateTime.UtcNow, .ETag = nowyEtag, .LastModified = nowyLm})
                            _zweryfikowaneWSesji(klucz) = True
                            Interlocked.Increment(_udaneObrazki)
                            Return Tuple.Create(bajtyCache, "")
                        End If

                        Dim dane() As Byte = Await odpowiedz.Content.ReadAsByteArrayAsync(limit.Token).ConfigureAwait(False)
                        Dim ct As String = If(odpowiedz.Content.Headers.ContentType IsNot Nothing, If(odpowiedz.Content.Headers.ContentType.MediaType, ""), "")
                        przepustka.Dispose()

                        If odpowiedz.StatusCode = HttpStatusCode.NoContent Then
                            _pusteKafle(klucz) = True
                            If Not _czyKafleXyz Then
                                ZapiszKafelAtomowo(plik, New Byte() {})
                            End If
                            Return Tuple.Create(New Byte() {}, "")
                        End If

                        If odpowiedz.StatusCode = HttpStatusCode.NotFound Then
                            If Not _czyKafleXyz Then
                                ZapiszKafelAtomowo(plik, New Byte() {})
                                Return Tuple.Create(New Byte() {}, "")
                            End If
                            'dla XYZ/TMS nie zapisuj pliku 0 B na dysku; 404 na nakładce albo na obrzeżu mapy po innych udanych kaflach traktuj jako pusty kafel
                            '(na podkładzie WypelnijMozaikeAsync próbuje go zastąpić kaflem z poziomu wyżej)
                            If Not jestNakladka Then _kafle404(klucz) = True
                            If jestNakladka OrElse Threading.Volatile.Read(_udaneObrazki) > 0 Then
                                _pusteKafle(klucz) = True
                                Return Tuple.Create(New Byte() {}, "")
                            End If
                            Return Tuple.Create(CType(Nothing, Byte()), "HTTP 404 (brak kafelka pod adresem " & MetadaneMapy.MaskujSekrety(adres) & ")")
                        End If

                        Dim kod As Integer = CInt(odpowiedz.StatusCode)
                        'HTTP 403 nie jest ponawiany: serwer odrzuca klienta (Referer, User-Agent, klucz), kolejna próba tego nie zmieni
                        If _czyKafleXyz AndAlso kod = 403 Then
                            Return Tuple.Create(CType(Nothing, Byte()), KafleXyz.OpisOdmowyDostepu(referer))
                        End If
                        If (kod = 418 OrElse kod = 429 OrElse kod = 500 OrElse kod = 502 OrElse kod = 503 OrElse kod = 504) AndAlso proba < maksProb Then
                            Await Task.Delay(TimeSpan.FromMilliseconds(400 * proba), token).ConfigureAwait(False)
                            Continue For
                        End If

                        If Not odpowiedz.IsSuccessStatusCode Then
                            If bajtyCache IsNot Nothing AndAlso bajtyCache.Length > 0 AndAlso (kod >= 500 OrElse kod = 429) Then
                                _zweryfikowaneWSesji(klucz) = True
                                Interlocked.Increment(_udaneObrazki)
                                Return Tuple.Create(bajtyCache, "")
                            End If
                            Dim tekstBledu As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 300)).Replace(vbCr, " ").Replace(vbLf, " ")
                            Return Tuple.Create(CType(Nothing, Byte()), "HTTP " & kod & " " & odpowiedz.ReasonPhrase & ": " & tekstBledu)
                        End If

                        If _czyKafleXyz Then
                            Dim diag As String = KafleXyz.DiagnozujOdpowiedzKafla(dane, ct, adres:=adres)
                            If diag <> "" Then Return Tuple.Create(CType(Nothing, Byte()), diag)
                        ElseIf Not PobieranieSegmentow.CzyObraz(dane) Then
                            Dim diag As String = KafleXyz.DiagnozujOdpowiedzKafla(dane, ct, adres:=adres)
                            If diag <> "" Then Return Tuple.Create(CType(Nothing, Byte()), diag)
                            Dim tekst As String = Text.Encoding.UTF8.GetString(dane, 0, Math.Min(dane.Length, 300)).Replace(vbCr, " ").Replace(vbLf, " ")
                            Return Tuple.Create(CType(Nothing, Byte()), "serwer nie zwrócił obrazka: " & tekst)
                        End If

                        ZapiszKafelAtomowo(plik, dane)
                        Dim etag = If(odpowiedz.Headers.ETag IsNot Nothing, odpowiedz.Headers.ETag.ToString(), "")
                        Dim lastMod = PobierzLastModified(odpowiedz, "")
                        If etag <> "" OrElse lastMod <> "" Then
                            ZapiszMetaAtomowo(plikMeta, New DaneCacheMeta With {.CheckedUtc = DateTime.UtcNow, .ETag = etag, .LastModified = lastMod})
                        Else
                            UsunPlikSafely(plikMeta)
                        End If
                        _zweryfikowaneWSesji(klucz) = True
                        Interlocked.Increment(_udaneObrazki)
                        Return Tuple.Create(dane, "")
                    End Using
                End Using
            Catch ex As OperationCanceledException When token.IsCancellationRequested
                Return Tuple.Create(CType(Nothing, Byte()), "przerwano pobieranie")
            Catch ex As OperationCanceledException When przepustka Is Nothing
                'pobieranie przerwane w trakcie oczekiwania na przepustkę - zapytania nie wysłano
                Return Tuple.Create(CType(Nothing, Byte()), "przerwano pobieranie")
            Catch ex As OperationCanceledException
                przepustka.Blad()
                przepustka.Dispose()
                If proba < maksProb Then
                    ponowOpoznienieMs = 400 * proba
                ElseIf bajtyCache IsNot Nothing AndAlso bajtyCache.Length > 0 Then
                    _zweryfikowaneWSesji(klucz) = True
                    Interlocked.Increment(_udaneObrazki)
                    Return Tuple.Create(bajtyCache, "")
                Else
                    Return Tuple.Create(CType(Nothing, Byte()), PobieranieSegmentow.OpisLimituCzasu(_limitCzasu))
                End If
            Catch ex As Exception
                If TypeOf ex Is HttpRequestException Then przepustka?.Blad()
                przepustka?.Dispose()
                If proba < maksProb AndAlso TypeOf ex Is HttpRequestException Then
                    ponowOpoznienieMs = 400 * proba
                ElseIf bajtyCache IsNot Nothing AndAlso bajtyCache.Length > 0 Then
                    _zweryfikowaneWSesji(klucz) = True
                    Interlocked.Increment(_udaneObrazki)
                    Return Tuple.Create(bajtyCache, "")
                Else
                    Dim opis As String = ex.Message
                    If ex.InnerException IsNot Nothing Then opis &= " (" & ex.InnerException.Message & ")"
                    Return Tuple.Create(CType(Nothing, Byte()), MetadaneMapy.MaskujSekrety(opis).Replace(vbCr, " ").Replace(vbLf, " "))
                End If
            Finally
                przepustka?.Dispose()
            End Try
            If ponowOpoznienieMs > 0 Then
                Await Task.Delay(TimeSpan.FromMilliseconds(ponowOpoznienieMs), token).ConfigureAwait(False)
            End If
        Next
        Return Tuple.Create(CType(Nothing, Byte()), "nie udało się pobrać kafelka")
    End Function

    Private Shared Function PobierzLastModified(odpowiedz As HttpResponseMessage, domyslna As String) As String
        If odpowiedz.Content.Headers.LastModified.HasValue Then
            Return odpowiedz.Content.Headers.LastModified.Value.UtcDateTime.ToString("R", Globalization.CultureInfo.InvariantCulture)
        End If
        Dim vals As IEnumerable(Of String) = Nothing
        If odpowiedz.Content.Headers.TryGetValues("Last-Modified", vals) Then
            Dim s = vals.FirstOrDefault()
            If Not String.IsNullOrWhiteSpace(s) Then Return s.Trim()
        End If
        Return domyslna
    End Function

    Private Shared Function OdczytajMeta(plikMeta As String) As DaneCacheMeta
        Try
            If Not File.Exists(plikMeta) Then Return Nothing
            Dim linie = File.ReadAllLines(plikMeta, Text.Encoding.UTF8)
            If linie.Length = 0 Then Return Nothing
            Dim ticks As Long
            Dim czas As DateTime = DateTime.MinValue
            If Long.TryParse(linie(0).Trim(), Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, ticks) Then
                czas = New DateTime(ticks, DateTimeKind.Utc)
            End If
            Return New DaneCacheMeta With {
                .CheckedUtc = czas,
                .ETag = If(linie.Length > 1, linie(1).Trim(), ""),
                .LastModified = If(linie.Length > 2, linie(2).Trim(), "")
            }
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Sub ZapiszMetaAtomowo(plikMeta As String, meta As DaneCacheMeta)
        Try
            Dim tresc = meta.CheckedUtc.Ticks.ToString(Globalization.CultureInfo.InvariantCulture) & vbLf &
                        If(meta.ETag, "") & vbLf &
                        If(meta.LastModified, "") & vbLf
            Dim tymcz As String = plikMeta & "." & Guid.NewGuid().ToString("N") & ".tmp"
            File.WriteAllText(tymcz, tresc, New Text.UTF8Encoding(False))
            File.Move(tymcz, plikMeta, overwrite:=True)
        Catch
        End Try
    End Sub

    Private Shared Sub UsunPlikSafely(sciezka As String)
        Try
            If File.Exists(sciezka) Then File.Delete(sciezka)
        Catch
        End Try
    End Sub

    Private Shared Sub ZapiszKafelAtomowo(plik As String, dane() As Byte)
        'zapis przez plik tymczasowy o unikalnej nazwie - ten sam kafel mogą pobierać jednocześnie dwa sąsiednie segmenty
        Dim tymczasowy As String = plik & "." & Guid.NewGuid().ToString("N") & ".tmp"
        File.WriteAllBytes(tymczasowy, dane)
        Try
            File.Move(tymczasowy, plik, overwrite:=True)
        Catch ex As IOException
            'kafel zapisywany jednocześnie przez inny segment
        End Try
        If File.Exists(tymczasowy) Then File.Delete(tymczasowy)
    End Sub

    ''' <summary>Nazwa pliku kafla - skrót adresu (adres jednoznacznie określa warstwę, poziom i położenie kafla).</summary>
    Private Shared Function NazwaKafla(adres As String) As String
        Using sha As SHA1 = SHA1.Create()
            Dim skrot = sha.ComputeHash(Text.Encoding.UTF8.GetBytes(adres))
            Return BitConverter.ToString(skrot).Replace("-", "").ToLowerInvariant() & ".kafel"
        End Using
    End Function

    ''' <summary>
    ''' Piksele segmentu: interpolacja dwuliniowa kafli (z nadpróbkowaniem, gdy kafle są dokładniejsze od segmentu),
    ''' warstwy nakładane kolejno z uwzględnieniem przezroczystości. Wynik - piksele ARGB wierszami.
    ''' </summary>
    Private Shared Function RenderujPiksele(mozaiki As List(Of Mozaika), bok As Integer) As Integer()
        Dim wynik(bok * bok - 1) As Integer
        For py = 0 To bok - 1
            For px = 0 To bok - 1
                'kolor z premnożoną przezroczystością (0-1)
                Dim r As Double = 0, g As Double = 0, b As Double = 0, a As Double = 0
                For Each m In mozaiki
                    If m.Piksele Is Nothing Then Continue For
                    Dim n As Integer = m.Plan.Nadprobkowanie
                    Dim sr As Double = 0, sg As Double = 0, sb As Double = 0, sa As Double = 0
                    For j = 0 To n - 1
                        For i = 0 To n - 1
                            Dim p = m.Przeliczenia.Punkt(px + (i + 0.5) / n, py + (j + 0.5) / n)
                            Probka(m, p.Y, p.X, sr, sg, sb, sa)
                        Next
                    Next
                    'krycie warstwy mnoży kolor premnożony razem z przezroczystością
                    Dim nn As Double = n * n / m.Krycie
                    sr /= nn : sg /= nn : sb /= nn : sa /= nn
                    r = sr + r * (1 - sa) : g = sg + g * (1 - sa) : b = sb + b * (1 - sa) : a = sa + a * (1 - sa)
                Next
                If a > 0.000001 Then
                    Dim alfa As Integer = CInt(Math.Round(a * 255))
                    wynik(py * bok + px) = (alfa << 24) Or (Bajt(r / a) << 16) Or (Bajt(g / a) << 8) Or Bajt(b / a)
                End If
            Next
        Next
        Return wynik
    End Function

    Private Shared Function Bajt(v As Double) As Integer
        Return Math.Max(0, Math.Min(255, CInt(Math.Round(v * 255))))
    End Function

    ''' <summary>
    ''' Dodaje próbkę mozaiki w punkcie (kolumna, wiersz) całej macierzy (narożniki pikseli w liczbach całkowitych):
    ''' interpolacja dwuliniowa kolorów premnożonych przez przezroczystość. Poza mozaiką - próbka przezroczysta.
    ''' </summary>
    Private Shared Sub Probka(m As Mozaika, kolumna As Double, wiersz As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double, ByRef a As Double)
        Dim u As Double = kolumna - m.Kolumna0 * m.Plan.Macierz.SzerokoscKafla - 0.5
        Dim v As Double = wiersz - m.Wiersz0 * m.Plan.Macierz.WysokoscKafla - 0.5
        If u < -0.5 OrElse v < -0.5 OrElse u > m.Szerokosc - 0.5 OrElse v > m.Wysokosc - 0.5 Then Exit Sub
        Dim k0 As Integer = CInt(Math.Floor(u)), w0 As Integer = CInt(Math.Floor(v))
        Dim tx As Double = u - k0, ty As Double = v - w0
        Dim k1 As Integer = Math.Min(k0 + 1, m.Szerokosc - 1), w1 As Integer = Math.Min(w0 + 1, m.Wysokosc - 1)
        k0 = Math.Max(0, k0) : w0 = Math.Max(0, w0)
        Dodaj(m.Piksele(w0 * m.Szerokosc + k0), (1 - tx) * (1 - ty), r, g, b, a)
        Dodaj(m.Piksele(w0 * m.Szerokosc + k1), tx * (1 - ty), r, g, b, a)
        Dodaj(m.Piksele(w1 * m.Szerokosc + k0), (1 - tx) * ty, r, g, b, a)
        Dodaj(m.Piksele(w1 * m.Szerokosc + k1), tx * ty, r, g, b, a)
    End Sub

    Private Shared Sub Dodaj(argb As Integer, waga As Double, ByRef r As Double, ByRef g As Double, ByRef b As Double, ByRef a As Double)
        Dim alfa As Double = ((argb >> 24) And &HFF) / 255.0 * waga
        If alfa = 0 Then Exit Sub
        r += ((argb >> 16) And &HFF) / 255.0 * alfa
        g += ((argb >> 8) And &HFF) / 255.0 * alfa
        b += (argb And &HFF) / 255.0 * alfa
        a += alfa
    End Sub

    ''' <summary>Zapis segmentu w formacie wybranym w ustawieniach (JPEG na białym tle, PNG z przezroczystością).</summary>
    Friend Shared Function Koduj(bmp As Bitmap, format As String, jakoscJpeg As Integer) As Byte()
        Using bmp
            Select Case Wms.RozszerzeniePliku(format)
                Case "png"
                    Return EksportMapy.Koduj(bmp, True, 0)
                Case "tif", "gif"
                    Using ms As New MemoryStream()
                        bmp.Save(ms, If(Wms.RozszerzeniePliku(format) = "tif", ImageFormat.Tiff, ImageFormat.Gif))
                        Return ms.ToArray()
                    End Using
                Case Else
                    Return EksportMapy.Koduj(bmp, False, jakoscJpeg)
            End Select
        End Using
    End Function

End Class
