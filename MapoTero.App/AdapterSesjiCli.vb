Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Rzeczywisty adapter sesji dla trybu wiersza poleceń (CLI).
''' Realizuje bezokienkowe pobieranie segmentów i eksport map na podstawie pliku sesji (conf.txt).
''' </summary>
Friend NotInheritable Class AdapterSesjiCli
    Implements IWykonawcaSesjiCli

    Private NotInheritable Class BezposredniPostep(Of T)
        Implements IProgress(Of T)

        Private ReadOnly _akcja As Action(Of T)

        Public Sub New(akcja As Action(Of T))
            _akcja = akcja
        End Sub

        Public Sub Report(value As T) Implements IProgress(Of T).Report
            _akcja?(value)
        End Sub
    End Class

    Private Shared Function WyznaczFolderSesji(sciezka As String) As String
        Dim f = Path.GetFullPath(sciezka.Trim())
        If File.Exists(f) Then
            f = Path.GetDirectoryName(f)
        End If
        If Not f.EndsWith(Path.DirectorySeparatorChar) AndAlso Not f.EndsWith(Path.AltDirectorySeparatorChar) Then
            f &= Path.DirectorySeparatorChar
        End If
        Return f
    End Function

    Private Shared Function WczytajSesje(folderSesji As String) As (Conf As KonfiguracjaSesji, Siatka As Siatka, Uklad As UkladWspolrzednych)
        Dim plikConf = Path.Combine(folderSesji, "conf.txt")
        If Not File.Exists(plikConf) Then
            Throw New FileNotFoundException("W folderze sesji nie znaleziono pliku conf.txt.", plikConf)
        End If

        Dim conf = KonfiguracjaSesji.Wczytaj(plikConf, NarzedziaTekstu.KodowanieSystemowe())
        Dim uklad = UkladWspolrzednych.ZKodu(conf.UkladEpsg)
        Dim zasieg As New Zasieg(NarzedziaTekstu.Wartosc(conf.XDol), NarzedziaTekstu.Wartosc(conf.YLewy),
                                 NarzedziaTekstu.Wartosc(conf.XGora), NarzedziaTekstu.Wartosc(conf.YPrawy))
        Dim siatka As New Siatka(zasieg, NarzedziaTekstu.Wartosc(conf.RozmiarPiksela), NarzedziaTekstu.WartoscCalkowita(conf.BokSegmentuPx))
        If Not siatka.Poprawna Then
            Throw New InvalidDataException("Plik conf.txt zawiera niepoprawne parametry siatki.")
        End If
        Return (conf, siatka, uklad)
    End Function

    Private Shared Function PrzygotujZrodloMap(conf As KonfiguracjaSesji) As Dictionary(Of String, PlikWarstw)
        Dim zrodla As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        Dim folderyWarstw As New List(Of String) From {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "warstwy"),
            Program.FolderWarstwProgramu,
            If(Not String.IsNullOrEmpty(Program.FolderDanych), Program.FolderWarstwDanych, Nothing),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MapoTero", "warstwy")
        }

        Dim zbioryList = conf.WybranyZestaw().Zbiory()
        For Each zb In zbioryList
            If String.IsNullOrWhiteSpace(zb) OrElse zrodla.ContainsKey(zb) Then Continue For
            For Each folderWarstw In folderyWarstw
                If String.IsNullOrWhiteSpace(folderWarstw) Then Continue For
                Dim plik = Path.Combine(folderWarstw, zb & ".txt")
                If File.Exists(plik) Then
                    Try
                        zrodla(zb) = PlikWarstw.Wczytaj(plik)
                        Exit For
                    Catch
                    End Try
                End If
            Next
        Next

        If zrodla.Count = 0 AndAlso Not String.IsNullOrEmpty(conf.AdresSerwera) Then
            Dim pw As New PlikWarstw With {
                .Typ = TypUslugiMapowej.Wms,
                .Adres = conf.AdresSerwera,
                .FormatObrazu = conf.Format
            }
            For Each w In conf.WybraneWarstwy()
                pw.Warstwy.Add(New WarstwaMapy With {.Nazwa = w})
            Next
            Dim nazwaZbioru = If(String.IsNullOrEmpty(conf.RodzajMapy), "sesja", conf.RodzajMapy)
            zrodla(nazwaZbioru) = pw
        End If

        Return zrodla
    End Function

    Public Async Function PobierzSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task _
        Implements IWykonawcaSesjiCli.PobierzSesjeAsync

        Dim folder = WyznaczFolderSesji(opcje.SciezkaSesji)
        Dim sesja = WczytajSesje(folder)
        Dim zrodla = PrzygotujZrodloMap(sesja.Conf)
        Dim zestaw = sesja.Conf.WybranyZestaw()

        Dim zadanie As New ZadaniePobierania With {
            .Folder = folder,
            .Uklad = sesja.Uklad,
            .Siatka = sesja.Siatka,
            .Zbiory = zestaw.Zbiory(),
            .Adresy = zestaw.Adresy(),
            .Warstwy = sesja.Conf.WybraneWarstwy(),
            .Krycia = sesja.Conf.KryciaWarstw(),
            .TrybyMieszania = sesja.Conf.TrybyMieszaniaWarstw,
            .Przezroczystosc = sesja.Conf.Przezroczystosc,
            .Korytarz = Korytarz.ZFolderu(folder),
            .ZrodlaMap = zrodla,
            .AdresSerwera = sesja.Conf.AdresSerwera,
            .Format = sesja.Conf.Format,
            .Prefiks = sesja.Conf.Prefiks,
            .Numeracja = Siatka.StylZTekstu(If(String.IsNullOrEmpty(sesja.Conf.Numeracja), "y_x", sesja.Conf.Numeracja)),
            .LiczbaWatkow = 4,
            .LimitCzasuSekundy = 30,
            .IloscProb = 3,
            .PrzerwaSekundy = 1
        }

        Dim postepPobierania As New BezposredniPostep(Of PostepPobierania)(
            Sub(p)
                postep?.Report(New PostepCli With {
                    .Etap = "Pobieranie",
                    .Wykonano = p.Pobrane,
                    .Razem = p.Wszystkie,
                    .Komunikat = p.Komunikat
                })
            End Sub)

        Dim wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, postepPobierania, ct).ConfigureAwait(False)
        If wynik.Nieudane > 0 Then
            Throw New IOException($"Nie pobrano wszystkich segmentów ({wynik.Nieudane} błędów z {wynik.Wszystkie}).")
        End If
    End Function

    Public Async Function EksportujSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task(Of String) _
        Implements IWykonawcaSesjiCli.EksportujSesjeAsync

        Dim folder = WyznaczFolderSesji(opcje.SciezkaSesji)
        Dim sesja = WczytajSesje(folder)

        Dim rodzaj As RodzajEksportu = RodzajEksportu.MBTiles
        Dim rozszerzenie As String = ".mbtiles"
        Dim fmt = If(String.IsNullOrEmpty(opcje.FormatEksportu), "mbtiles", opcje.FormatEksportu.ToLowerInvariant())

        Select Case fmt
            Case "kmz"
                rodzaj = RodzajEksportu.Kmz
                rozszerzenie = ".kmz"
            Case "jnx"
                rodzaj = RodzajEksportu.Jnx
                rozszerzenie = ".jnx"
            Case "gpkg", "geopackage"
                rodzaj = RodzajEksportu.GeoPackage
                rozszerzenie = ".gpkg"
            Case "sqlitedb"
                rodzaj = RodzajEksportu.SQLiteDB
                rozszerzenie = ".sqlitedb"
            Case "orux", "oruxmaps"
                rodzaj = RodzajEksportu.OruxMaps
                rozszerzenie = "_oruxmaps"
            Case "geotiff", "tif", "tiff", "arkusz"
                rodzaj = RodzajEksportu.GeoTiff
                rozszerzenie = ".tif"
            Case "trekbuddy", "tar"
                rodzaj = RodzajEksportu.TrekBuddy
                rozszerzenie = ".tar"
            Case Else
                rodzaj = RodzajEksportu.MBTiles
                rozszerzenie = ".mbtiles"
        End Select

        Dim plikWyjsciowy = opcje.SciezkaWyjsciowa
        If String.IsNullOrEmpty(plikWyjsciowy) Then
            Dim nazwa = "_eksport_" & sesja.Siatka.LiczbaKolumn & "x" & sesja.Siatka.LiczbaWierszy & rozszerzenie
            plikWyjsciowy = Path.Combine(folder, nazwa)
        End If

        If Not opcje.NadpiszIstniejacy Then
            Dim istnieje As Boolean = If(rodzaj = RodzajEksportu.OruxMaps, Directory.Exists(plikWyjsciowy), File.Exists(plikWyjsciowy))
            If istnieje Then
                Throw New IOException($"Plik wynikowy już istnieje: {plikWyjsciowy}. Użyj --nadpisz, aby nadpisać.")
            End If
        End If

        Dim przytnij As Boolean = False
        Dim maska As Func(Of Double, Double, Boolean) = Nothing
        If Not String.IsNullOrWhiteSpace(opcje.SciezkaPoligonu) Then
            Dim poli = ParserPoligonu.WczytajZPliku(opcje.SciezkaPoligonu)
            Dim poliWUkladzie = poli.PrzeliczWspolrzedne(
                Function(p)
                    Dim wgs = ZestawPoligonow.PunktDoWgs84(poli.WykrytyUklad, p)
                    Dim xy = sesja.Uklad.ZWgs84(wgs.Polnoc, wgs.Wschod)
                    Return New Punkt2D(xy.Y, xy.X)
                End Function)
            przytnij = True
            maska = Function(x, y) poliWUkladzie.ZawieraPunkt(New Punkt2D(y, x))
        Else
            Dim korytarzSesji = Korytarz.ZFolderu(folder)
            If korytarzSesji IsNot Nothing Then
                przytnij = True
                maska = korytarzSesji.UtworzMaske(sesja.Siatka.ZasiegSiatki, sesja.Uklad)
            End If
        End If

        Dim zrodlo As New ZrodloSegmentow(folder, sesja.Conf.Prefiks,
                                          Siatka.StylZTekstu(If(String.IsNullOrEmpty(sesja.Conf.Numeracja), "y_x", sesja.Conf.Numeracja)),
                                          Wms.RozszerzeniePliku(sesja.Conf.Format), sesja.Siatka, przytnij, maska)

        Dim zadanie As New ZadanieEksportu With {
            .Rodzaj = rodzaj,
            .Zrodlo = zrodlo,
            .Uklad = sesja.Uklad,
            .Plik = plikWyjsciowy,
            .JakoscJpeg = 75,
            .FormatArkusza = FormatArkusza.GeoTiff,
            .PiramidyGeoTiff = True,
            .Nazwa = Path.GetFileNameWithoutExtension(plikWyjsciowy)
        }

        If zadanie.Piramida Then
            Dim srodek = sesja.Uklad.DoWgs84(sesja.Siatka.ZasiegSiatki.Srodek)
            Dim poziomBazowy = WebMercator.PoziomDlaRozdzielczosci(EksportMapy.PikselWMetrach(zrodlo, sesja.Uklad), srodek.Szerokosc)
            zadanie.PoziomMax = Math.Max(0, poziomBazowy)
            zadanie.PoziomMin = Math.Max(0, poziomBazowy - 4)
        End If

        Dim postepEksportu As New BezposredniPostep(Of Integer)(
            Sub(procent)
                postep?.Report(New PostepCli With {
                    .Etap = "Eksport",
                    .Wykonano = procent,
                    .Razem = 100,
                    .Komunikat = $"{procent}%"
                })
            End Sub)

        Await Task.Run(Sub() EksportMapy.Eksportuj(zadanie, postepEksportu, ct), ct).ConfigureAwait(False)
        Return plikWyjsciowy
    End Function

End Class