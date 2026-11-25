'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>Wynik pobrania obrazu podglądu warstwy lub jej zasięgu.</summary>
Public Class WynikPodgladuWarstwy
    Public Property Sukces As Boolean
    Public Property Obraz As Bitmap
    Public Property Komunikat As String = ""
    Public Property BrakKodekaWebp As Boolean
    Public Property Pozycja As PozycjaGaleriiWarstw
End Class

''' <summary>
''' Zarządza spisem galerii (<c>warstwy.json</c>), pobieraniem próbek <c>.webp</c> z GitHub Pages,
''' lokalnym cache na dysku oraz renderowaniem map zasięgu z <c>maska_hex</c>.
''' </summary>
Public Class MagazynPodgladuGalerii
    Implements IDisposable

    Public Const DomyslnyAdresPomocy As String = "https://eksploracja.github.io/MapoTero/pomoc/"

    Private ReadOnly _folderCache As String
    Private ReadOnly _adresBazowyPomocy As String
    Private ReadOnly _folderLokalnyPomoc As String
    Private ReadOnly _wylaczSiec As Boolean
    Private ReadOnly _http As HttpClient
    Private ReadOnly _maWlasnyHttp As Boolean
    Private ReadOnly _blokada As New Object()
    Private ReadOnly _blokadaLadowaniaKatalogu As New SemaphoreSlim(1, 1)

    Private ReadOnly _cacheProbek As New Dictionary(Of String, Bitmap)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _kolejnoscProbek As New Queue(Of String)()
    Private ReadOnly _cacheZasiegow As New Dictionary(Of String, Bitmap)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _kolejnoscZasiegow As New Queue(Of String)()
    Private Const MaksRozmiarCachePamieci As Integer = 40

    Private _katalog As KatalogGaleriiWarstw
    Private _ostatnieOdswiezenieUtc As DateTime = DateTime.MinValue
    Private _zakonczonoPierwszeLadowanie As Boolean

    Public Event KatalogZaladowany As EventHandler

    Public Sub New(folderDanych As String,
                   Optional adresBazowyPomocy As String = DomyslnyAdresPomocy,
                   Optional folderLokalnyPomoc As String = Nothing,
                   Optional httpClient As HttpClient = Nothing,
                   Optional wylaczSiec As Boolean = False)
        Dim bazowyFolder = If(String.IsNullOrWhiteSpace(folderDanych), Path.GetTempPath(), folderDanych)
        _folderCache = Path.Combine(bazowyFolder, "galeria_cache")
        _wylaczSiec = wylaczSiec
        Dim envUrl = Environment.GetEnvironmentVariable("MAPOTERO_POMOC_URL")
        Dim url = If(Not String.IsNullOrWhiteSpace(envUrl), envUrl, adresBazowyPomocy)
        If String.IsNullOrWhiteSpace(url) Then url = DomyslnyAdresPomocy
        _adresBazowyPomocy = url.TrimEnd("/"c) & "/"

        If folderLokalnyPomoc IsNot Nothing Then
            _folderLokalnyPomoc = folderLokalnyPomoc
        Else
            _folderLokalnyPomoc = ZnajdzLokalnyFolderPomocy()
        End If

        If httpClient IsNot Nothing Then
            _http = httpClient
            _maWlasnyHttp = False
        Else
            _http = New HttpClient() With {.Timeout = TimeSpan.FromSeconds(12)}
            Try
                _http.DefaultRequestHeaders.UserAgent.ParseAdd(UstawieniaSilnika.UserAgent)
            Catch
            End Try
            _maWlasnyHttp = True
        End If

        WczytajKatalogZCacheLubLokalnie()
    End Sub

    Public ReadOnly Property Katalog As KatalogGaleriiWarstw
        Get
            SyncLock _blokada
                Return _katalog
            End SyncLock
        End Get
    End Property

    ''' <summary>Szuka warstwy w aktualnie załadowanym katalogu galerii.</summary>
    Public Function ZnajdzWarstwe(zbior As String, warstwa As String) As PozycjaGaleriiWarstw
        Dim kat = Katalog
        If kat Is Nothing Then Return Nothing
        Return kat.Znajdz(zbior, warstwa)
    End Function

    ''' <summary>Czy warstwa jest dostępna w katalogu galerii (wtedy warto pokazywać ikonki podglądu).</summary>
    Public Function CzyDostepna(zbior As String, warstwa As String) As Boolean
        Return ZnajdzWarstwe(zbior, warstwa) IsNot Nothing
    End Function

    Private Shared Function ZnajdzLokalnyFolderPomocy() As String
        Dim kandydaci = {
            Path.Combine(AppContext.BaseDirectory, "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "pomoc"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pomoc")
        }
        For Each sciezka In kandydaci
            Try
                Dim pelna = Path.GetFullPath(sciezka)
                If File.Exists(Path.Combine(pelna, "galeria", "warstwy.json")) Then
                    Return pelna
                End If
            Catch
            End Try
        Next
        Return ""
    End Function

    Private ReadOnly Property SciezkaCacheKatalogu As String
        Get
            Return Path.Combine(_folderCache, "warstwy.json")
        End Get
    End Property

    Private Sub WczytajKatalogZCacheLubLokalnie()
        Try
            If File.Exists(SciezkaCacheKatalogu) Then
                Dim json = File.ReadAllText(SciezkaCacheKatalogu, Encoding.UTF8)
                Dim kat = KatalogGaleriiWarstw.ZTekstu(json)
                If kat.Pozycje.Count > 0 Then
                    SyncLock _blokada
                        _katalog = kat
                        _ostatnieOdswiezenieUtc = File.GetLastWriteTimeUtc(SciezkaCacheKatalogu)
                    End SyncLock
                    Return
                End If
            End If
        Catch
        End Try

        If Not String.IsNullOrWhiteSpace(_folderLokalnyPomoc) Then
            Try
                Dim lokalny = Path.Combine(_folderLokalnyPomoc, "galeria", "warstwy.json")
                If File.Exists(lokalny) Then
                    Dim json = File.ReadAllText(lokalny, Encoding.UTF8)
                    Dim kat = KatalogGaleriiWarstw.ZTekstu(json)
                    If kat.Pozycje.Count > 0 Then
                        SyncLock _blokada
                            _katalog = kat
                        End SyncLock
                    End If
                End If
            Catch
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Upewnia się, że katalog <c>warstwy.json</c> jest wczytany i odświeżony (najpierw z cache,
    ''' w tle z GitHuba maksymalnie raz na 24 godziny, z fallbackiem do lokalnego folderu <c>pomoc/</c>).
    ''' </summary>
    Public Async Function ZapewnijKatalogAsync(Optional wymusOdswiezenie As Boolean = False,
                                              Optional ct As CancellationToken = Nothing) As Task(Of Boolean)
        Dim aktualny = Katalog
        Dim czySwiezy = aktualny IsNot Nothing AndAlso aktualny.Pozycje.Count > 0 AndAlso
                        Not wymusOdswiezenie AndAlso
                        (_ostatnieOdswiezenieUtc > DateTime.UtcNow.AddHours(-24) OrElse _zakonczonoPierwszeLadowanie)
        If czySwiezy Then Return True

        Await _blokadaLadowaniaKatalogu.WaitAsync(ct).ConfigureAwait(False)
        Try
            aktualny = Katalog
            If aktualny IsNot Nothing AndAlso aktualny.Pozycje.Count > 0 AndAlso
               Not wymusOdswiezenie AndAlso
               (_ostatnieOdswiezenieUtc > DateTime.UtcNow.AddHours(-24) OrElse _zakonczonoPierwszeLadowanie) Then
                Return True
            End If

            Dim zmieniono As Boolean = False
            If Not _wylaczSiec Then
                Try
                    Dim url = _adresBazowyPomocy & "galeria/warstwy.json"
                    Using resp = Await _http.GetAsync(url, ct).ConfigureAwait(False)
                        If resp.IsSuccessStatusCode Then
                            Dim json = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                            Dim nowy = KatalogGaleriiWarstw.ZTekstu(json)
                            If nowy.Pozycje.Count > 0 Then
                                ZapiszDoCache(SciezkaCacheKatalogu, Encoding.UTF8.GetBytes(json))
                                SyncLock _blokada
                                    _katalog = nowy
                                    _ostatnieOdswiezenieUtc = DateTime.UtcNow
                                    _cacheZasiegow.Clear()
                                    _kolejnoscZasiegow.Clear()
                                End SyncLock
                                zmieniono = True
                            End If
                        End If
                    End Using
                Catch
                End Try
            End If

            If Not zmieniono AndAlso (Katalog Is Nothing OrElse Katalog.Pozycje.Count = 0) AndAlso
               Not String.IsNullOrWhiteSpace(_folderLokalnyPomoc) Then
                Dim lokalny = Path.Combine(_folderLokalnyPomoc, "galeria", "warstwy.json")
                If File.Exists(lokalny) Then
                    Dim json = File.ReadAllText(lokalny, Encoding.UTF8)
                    Dim nowy = KatalogGaleriiWarstw.ZTekstu(json)
                    If nowy.Pozycje.Count > 0 Then
                        SyncLock _blokada
                            _katalog = nowy
                        End SyncLock
                        zmieniono = True
                    End If
                End If
            End If

            _zakonczonoPierwszeLadowanie = True
            If zmieniono Then RaiseEvent KatalogZaladowany(Me, EventArgs.Empty)
            Return Katalog IsNot Nothing AndAlso Katalog.Pozycje.Count > 0
        Finally
            _blokadaLadowaniaKatalogu.Release()
        End Try
    End Function

    ''' <summary>
    ''' Zwraca bitmapę próbki mapy dla podanej warstwy (własna kopia dla wywołującego — należy ją zwolnić).
    ''' Pobiera plik <c>.webp</c> z GitHuba i zapisuje w lokalnym cache, dekoduje przez WIC.
    ''' </summary>
    Public Async Function PobierzObrazProbkiAsync(zbior As String, warstwa As String,
                                                  Optional ct As CancellationToken = Nothing) As Task(Of WynikPodgladuWarstwy)
        Dim wynik As New WynikPodgladuWarstwy()
        Dim pozycja = ZnajdzWarstwe(zbior, warstwa)
        If pozycja Is Nothing Then
            Await ZapewnijKatalogAsync(False, ct).ConfigureAwait(False)
            pozycja = ZnajdzWarstwe(zbior, warstwa)
        End If
        wynik.Pozycja = pozycja
        If pozycja Is Nothing OrElse String.IsNullOrWhiteSpace(pozycja.Obrazek) Then
            wynik.Komunikat = "Brak próbki podglądu dla tej warstwy."
            Return wynik
        End If

        Dim klucz = KatalogGaleriiWarstw.Klucz(zbior, warstwa)
        Dim zPamieci = PobierzZKolejkiCache(_cacheProbek, klucz)
        If zPamieci IsNot Nothing Then
            wynik.Sukces = True
            wynik.Obraz = zPamieci
            Return wynik
        End If

        Dim wzgledna = pozycja.Obrazek.Replace("\"c, "/"c).TrimStart("/"c)
        Dim sciezkaCacheObrazu = SciezkaWewnatrz(_folderCache, wzgledna)
        If sciezkaCacheObrazu Is Nothing Then
            wynik.Komunikat = "Nieprawidłowa ścieżka próbki podglądu w spisie galerii."
            Return wynik
        End If
        Dim dane As Byte() = Nothing

        If File.Exists(sciezkaCacheObrazu) Then
            Try
                dane = File.ReadAllBytes(sciezkaCacheObrazu)
            Catch
                dane = Nothing
            End Try
        End If

        If (dane Is Nothing OrElse dane.Length = 0) AndAlso Not _wylaczSiec Then
            Try
                Dim url = _adresBazowyPomocy & wzgledna
                Using resp = Await _http.GetAsync(url, ct).ConfigureAwait(False)
                    If resp.IsSuccessStatusCode Then
                        dane = Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)
                        If dane IsNot Nothing AndAlso dane.Length > 0 Then
                            ZapiszDoCache(sciezkaCacheObrazu, dane)
                        End If
                    End If
                End Using
            Catch ex As OperationCanceledException
                Throw
            Catch
                dane = Nothing
            End Try
        End If

        If (dane Is Nothing OrElse dane.Length = 0) AndAlso Not String.IsNullOrWhiteSpace(_folderLokalnyPomoc) Then
            Dim lokalnyObraz = SciezkaWewnatrz(_folderLokalnyPomoc, wzgledna)
            If lokalnyObraz IsNot Nothing AndAlso File.Exists(lokalnyObraz) Then
                Try
                    dane = File.ReadAllBytes(lokalnyObraz)
                Catch
                    dane = Nothing
                End Try
            End If
        End If

        If dane Is Nothing OrElse dane.Length = 0 Then
            wynik.Komunikat = "Nie udało się pobrać próbki podglądu z GitHuba."
            Return wynik
        End If

        Dim bladKodeka As Boolean = False
        Dim bmp = DekoderWebp.DekodujZBajtow(dane, bladKodeka)
        If bmp Is Nothing Then
            wynik.BrakKodekaWebp = bladKodeka
            wynik.Komunikat = If(bladKodeka,
                                 "System Windows nie ma aktywnego kodeka WebP (WIC).",
                                 "Nie udało się odczytać pliku próbki podglądu.")
            Return wynik
        End If

        wynik.Sukces = True
        wynik.Obraz = KopiaIDodanieDoCache(_cacheProbek, _kolejnoscProbek, klucz, bmp)
        Return wynik
    End Function

    ''' <summary>
    ''' Ścieżka pliku wskazanego w spisie galerii, jeśli leży wewnątrz podanego folderu; inaczej Nothing. Spis
    ''' warstwy.json pobierany jest z internetu - wpis „obrazek” z „..\” zapisałby pobrany plik poza folderem podręcznym.
    ''' </summary>
    Friend Shared Function SciezkaWewnatrz(folder As String, wzgledna As String) As String
        If String.IsNullOrWhiteSpace(folder) OrElse String.IsNullOrWhiteSpace(wzgledna) Then Return Nothing
        Try
            Dim baza = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
            Dim pelna = Path.GetFullPath(Path.Combine(baza, wzgledna.Replace("/"c, Path.DirectorySeparatorChar)))
            Return If(pelna.StartsWith(baza, StringComparison.OrdinalIgnoreCase), pelna, Nothing)
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Kopia bitmapy dla wywołującego i zapis oryginału w pamięci podręcznej. Kopia powstaje, zanim oryginał trafi
    ''' do pamięci - dawniej Clone() po dodaniu mógł trafić na jednoczesne Clone() tej samej bitmapy w innym wątku
    ''' (GDI+: „obiekt jest używany w innym miejscu”), a bitmapa, której klucz był już w pamięci, nie była zwalniana.
    ''' </summary>
    Private Function KopiaIDodanieDoCache(slownik As Dictionary(Of String, Bitmap), kolejka As Queue(Of String), klucz As String, bmp As Bitmap) As Bitmap
        Dim kopia = CType(bmp.Clone(), Bitmap)
        If Not DodajDoKolejkiCache(slownik, kolejka, klucz, bmp) Then bmp.Dispose()
        Return kopia
    End Function

    ''' <summary>
    ''' Zwraca wyrenderowaną mapę zasięgu dla warstwy na podstawie <c>warstwy.json</c> (w tym <c>maska_hex</c>
    ''' z dziurami wewnątrz Polski). Wywołujący jest właścicielem zwróconej bitmapy.
    ''' </summary>
    Public Function PobierzObrazZasiegu(zbior As String, warstwa As String,
                                        Optional szerokosc As Integer = 330,
                                        Optional wysokosc As Integer = 288) As WynikPodgladuWarstwy
        Dim wynik As New WynikPodgladuWarstwy()
        Dim pozycja = ZnajdzWarstwe(zbior, warstwa)
        wynik.Pozycja = pozycja
        If pozycja Is Nothing Then
            wynik.Komunikat = "Brak danych o zasięgu tej warstwy."
            Return wynik
        End If

        Dim klucz = KatalogGaleriiWarstw.Klucz(zbior, warstwa) & "|" & szerokosc & "x" & wysokosc
        Dim zPamieci = PobierzZKolejkiCache(_cacheZasiegow, klucz)
        If zPamieci IsNot Nothing Then
            wynik.Sukces = True
            wynik.Obraz = zPamieci
            Return wynik
        End If

        Dim bmp = RendererZasieguGalerii.Renderuj(pozycja, szerokosc, wysokosc)
        wynik.Sukces = True
        wynik.Obraz = KopiaIDodanieDoCache(_cacheZasiegow, _kolejnoscZasiegow, klucz, bmp)
        Return wynik
    End Function

    Private Function PobierzZKolejkiCache(slownik As Dictionary(Of String, Bitmap), klucz As String) As Bitmap
        SyncLock _blokada
            Dim bmp As Bitmap = Nothing
            If slownik.TryGetValue(klucz, bmp) AndAlso bmp IsNot Nothing Then
                Return CType(bmp.Clone(), Bitmap)
            End If
        End SyncLock
        Return Nothing
    End Function

    ''' <summary>Dodaje bitmapę do pamięci podręcznej; False - klucz już był (bitmapa nie została zapamiętana).</summary>
    Private Function DodajDoKolejkiCache(slownik As Dictionary(Of String, Bitmap), kolejka As Queue(Of String), klucz As String, bmp As Bitmap) As Boolean
        If bmp Is Nothing Then Return False
        SyncLock _blokada
            If slownik.ContainsKey(klucz) Then Return False
            While kolejka.Count >= MaksRozmiarCachePamieci
                Dim najstarszy = kolejka.Dequeue()
                Dim stary As Bitmap = Nothing
                If slownik.TryGetValue(najstarszy, stary) Then
                    slownik.Remove(najstarszy)
                    stary?.Dispose()
                End If
            End While
            slownik(klucz) = bmp
            kolejka.Enqueue(klucz)
        End SyncLock
        Return True
    End Function

    Private Shared Sub ZapiszDoCache(sciezka As String, dane As Byte())
        Try
            Dim kat = Path.GetDirectoryName(sciezka)
            If Not String.IsNullOrEmpty(kat) Then Directory.CreateDirectory(kat)
            Dim tymczasowy = sciezka & ".tmp"
            File.WriteAllBytes(tymczasowy, dane)
            File.Move(tymczasowy, sciezka, True)
        Catch
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock _blokada
            For Each bmp In _cacheProbek.Values
                bmp?.Dispose()
            Next
            _cacheProbek.Clear()
            _kolejnoscProbek.Clear()
            For Each bmp In _cacheZasiegow.Values
                bmp?.Dispose()
            Next
            _cacheZasiegow.Clear()
            _kolejnoscZasiegow.Clear()
        End SyncLock
        _blokadaLadowaniaKatalogu.Dispose()
        If _maWlasnyHttp Then _http.Dispose()
    End Sub
End Class
