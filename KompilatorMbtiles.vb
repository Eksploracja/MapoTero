'Copyright (C) <2015>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Data
Imports System.Data.SQLite
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>Format zapisu kafli rastrowych w pliku MBTiles.</summary>
Public Enum FormatKafliMbtiles
    ''' <summary>JPEG dla kafli w pełni kryjących; PNG dla kafli z przezroczystością (krawędzie, nakładki).</summary>
    SmartHybrid = 0
    ''' <summary>Wszystkie kafle jako JPEG (przezroczystość zastąpiona kolorem tła).</summary>
    TylkoJpeg = 1
    ''' <summary>Wszystkie kafle jako PNG (pełna bezstratność i kanał alfa).</summary>
    TylkoPng = 2
End Enum

''' <summary>Definicja jednej warstwy / źródła danych w projekcie wielowarstwowego MBTiles.</summary>
Public Class WarstwaWielowarstwowa
    Public Property Folder As String = ""
    Public Property Tytul As String = ""
    Public Property PoziomMin As Integer = 10
    Public Property PoziomMax As Integer = 16
    Public Property Krycie As Double = 1.0
    Public Property BialePrzezroczyste As Boolean = False
    Public Property OdcienieSzarosci As Boolean = False
    Public Property Kolejnosc As Integer = 0
    Public Property Wlaczona As Boolean = True
    Public Property TrybMieszania As TrybMieszania = TrybMieszania.Zwykly

    Public Property Konfiguracja As KonfiguracjaSesji
    Public Property Siatka As Siatka
    Public Property Uklad As UkladWspolrzednych = UkladWspolrzednych.PL1992
    Public Property Zrodlo As ZrodloSegmentow

    Public ReadOnly Property Poprawna As Boolean
        Get
            Return Zrodlo IsNot Nothing AndAlso Siatka IsNot Nothing AndAlso Siatka.Poprawna
        End Get
    End Property

    Public ReadOnly Property ZasiegWgs84 As ProstokatGeo
        Get
            If Siatka Is Nothing OrElse Not Siatka.Poprawna Then Return New ProstokatGeo()
            Return ProstokatGeo.Opisany(Uklad, Siatka.ZasiegSiatki)
        End Get
    End Property

    ''' <summary>Wczytuje parametry sesji z pliku conf.txt w folderze.</summary>
    Public Function WczytajSesje(ByRef blad As String) As Boolean
        blad = ""
        If String.IsNullOrWhiteSpace(Folder) Then
            blad = "Nie określono folderu sesji."
            Return False
        End If
        Dim plikConf As String = Path.Combine(Folder, "conf.txt")
        If Not File.Exists(plikConf) Then
            blad = "W folderze brak pliku conf.txt."
            Return False
        End If

        Try
            Konfiguracja = KonfiguracjaSesji.Wczytaj(plikConf, KodowanieSystemowe())
            Dim c = Konfiguracja
            Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
            Siatka = New Siatka(New Zasieg(Wartosc(c.XDol), Wartosc(c.YLewy),
                                           Wartosc(c.XGora), Wartosc(c.YPrawy)),
                                Wartosc(c.RozmiarPiksela), WartoscCalkowita(c.BokSegmentuPx))
            If Not Siatka.Poprawna Then
                blad = "Niepoprawne parametry siatki w pliku conf.txt."
                Return False
            End If

            Dim styl = Siatka.StylZTekstu(If(c.Numeracja = "", Ustawienia.Numeracja, c.Numeracja))
            Dim rozszerzenie = Wms.RozszerzeniePliku(c.Format)
            Zrodlo = New ZrodloSegmentow(Folder.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar,
                                         c.Prefiks, styl, rozszerzenie, Siatka)
            Zrodlo.BialePrzezroczyste = BialePrzezroczyste

            If String.IsNullOrWhiteSpace(Tytul) Then
                Tytul = (c.RodzajMapy & " " & If(c.Warstwy.Count > 0, c.Warstwy(0), "")).Trim()
                If String.IsNullOrWhiteSpace(Tytul) Then Tytul = Path.GetFileName(Folder.TrimEnd(Path.DirectorySeparatorChar))
            End If
            Return True
        Catch ex As Exception
            blad = "Błąd odczytu sesji: " & ex.Message
            Return False
        End Try
    End Function

    Public Function DoWarstwyEksportu() As WarstwaEksportu
        If Zrodlo IsNot Nothing Then
            Zrodlo.BialePrzezroczyste = BialePrzezroczyste
            Zrodlo.OdcienieSzarosci = OdcienieSzarosci
        End If
        Return New WarstwaEksportu With {
            .Zrodlo = Zrodlo,
            .Uklad = Uklad,
            .Krycie = Math.Max(0.0, Math.Min(1.0, Krycie)),
            .Opis = Tytul,
            .AdresSerwera = If(Konfiguracja IsNot Nothing, Konfiguracja.AdresSerwera, ""),
            .TrybMieszania = TrybMieszania
        }
    End Function
End Class

''' <summary>Parametry zaawansowanego zadania tworzenia wielowarstwowego MBTiles.</summary>
Public Class OpcjeKompilatoraMbtiles
    Public Property PlikDocelowy As String = ""
    Public Property NazwaMapy As String = "Mapa wielowarstwowa"
    Public Property OpisMapy As String = "Mapa skompilowana z wielu warstw i poziomów zoom w MapoTero"
    Public Property Atrybucja As String = "MapoTero"
    Public Property FormatKafli As FormatKafliMbtiles = FormatKafliMbtiles.SmartHybrid
    Public Property JakoscJpeg As Integer = 85
    Public Property KolorTlaJpeg As Color = Color.White
    Public Property SzybkaPiramidaWPasmach As Boolean = True
    Public Property WypelnijLukiWarstwaNizsza As Boolean = False
    Public Property PoziomMin As Integer = 8
    Public Property PoziomMax As Integer = 18

    Public Function Klonuj() As OpcjeKompilatoraMbtiles
        Return DirectCast(Me.MemberwiseClone(), OpcjeKompilatoraMbtiles)
    End Function
End Class

''' <summary>Szacowane parametry jednego poziomu powiększenia w kompilacji MBTiles.</summary>
Public Class SzacunekPoziomuMbtiles
    Public Property Poziom As Integer
    Public Property LiczbaKafliSiatki As Long
    Public Property LiczbaKafliZDanymi As Long
    Public Property SredniRozmiarKafla As Long
    Public Property RozmiarPoziomuBajty As Long
    Public Property AktywneWarstwyTekst As String = ""
End Class

''' <summary>Całościowy szacunek parametrów wynikowego pliku MBTiles.</summary>
Public Class SzacunekKompilacjiMbtiles
    Public Property Poziomy As New SortedDictionary(Of Integer, SzacunekPoziomuMbtiles)
    Public Property LacznaLiczbaKafli As Long
    Public Property LacznyRozmiarBajty As Long

    Public ReadOnly Property CzyPrzekraczaFat32 As Boolean
        Get
            ' Limit FAT32 to 4 GB - 1 bajt (4 294 967 295 bajtów)
            Return LacznyRozmiarBajty >= 4294967295L
        End Get
    End Property

    Public Function TekstRozmiaru() As String
        Return SzacunekRozmiaru.Tekst(LacznyRozmiarBajty)
    End Function

    Public Function OpisStatusuFat32() As String
        If CzyPrzekraczaFat32 Then
            Return "UWAGA: Szacowany rozmiar przekracza 4 GB! Plik nie zmieści się na kartach microSD sformatowanych w FAT32 (wymagany exFAT lub zmniejszenie maksymalnego zoomu)."
        ElseIf LacznyRozmiarBajty > 2147483648L Then
            Return "Rozmiar powyżej 2 GB - upewnij się, że masz wystarczająco dużo wolnego miejsca w telefonie."
        Else
            Return "Rozmiar optymalny dla urządzeń mobilnych (Locus Map, OruxMaps)."
        End If
    End Function
End Class

''' <summary>Raport postępu tworzenia wielowarstwowego MBTiles.</summary>
Public Class RaportPostepuKompilacji
    Public Property Procent As Integer
    Public Property AktualnyPoziom As Integer
    Public Property WykonaneKafle As Long
    Public Property WszystkieKafle As Long
    Public Property Komunikat As String = ""
    Public Property Gotowe As Boolean
End Class

''' <summary>
''' Silnik kompilacji wielowarstwowego i wieloskalowego pliku MBTiles.
''' Łączy kafelki z różnych sesji (o różnych poziomach zoomu i układach współrzędnych)
''' w jeden zoptymalizowany plik bazy SQLite MBTiles v1.3 kompatybilny z OruxMaps i Locus Map.
''' </summary>
Public NotInheritable Class KompilatorMbtiles

    Private Sub New()
    End Sub

    ''' <summary>Zasięg WGS84 opisany na wszystkich aktywnych warstwach.</summary>
    Public Shared Function WyznaczObszarWgs84(warstwy As IEnumerable(Of WarstwaWielowarstwowa)) As ProstokatGeo
        Dim p As New ProstokatGeo(Double.MinValue, Double.MaxValue, Double.MinValue, Double.MaxValue)
        Dim maJakikolwiek As Boolean = False
        For Each w In warstwy
            If Not w.Wlaczona OrElse Not w.Poprawna Then Continue For
            Dim z = w.ZasiegWgs84
            If Not z.Poprawny Then Continue For
            p.Polnoc = Math.Max(p.Polnoc, z.Polnoc)
            p.Poludnie = Math.Min(p.Poludnie, z.Poludnie)
            p.Wschod = Math.Max(p.Wschod, z.Wschod)
            p.Zachod = Math.Min(p.Zachod, z.Zachod)
            maJakikolwiek = True
        Next
        Return If(maJakikolwiek, p, New ProstokatGeo(55.0, 49.0, 24.5, 14.0))
    End Function

    ''' <summary>Zwraca listę warstw aktywnych dla wskazanego poziomu powiększenia (posortowanych wg kolejności rysowania).</summary>
    Public Shared Function WarstwyDlaPoziomu(warstwy As IEnumerable(Of WarstwaWielowarstwowa), poziom As Integer) As List(Of WarstwaWielowarstwowa)
        Dim wynik = warstwy.Where(Function(w) w.Wlaczona AndAlso w.Poprawna AndAlso w.PoziomMin <= poziom AndAlso w.PoziomMax >= poziom).
                            OrderBy(Function(w) w.Kolejnosc).ToList()
        Return wynik
    End Function

    ''' <summary>Wyszukuje luki w poziomach zoom (poziomy, na których żadna warstwa nie jest aktywna).</summary>
    Public Shared Function SprawdzLukiPoziomow(warstwy As IEnumerable(Of WarstwaWielowarstwowa), minZ As Integer, maxZ As Integer) As List(Of Integer)
        Dim luki As New List(Of Integer)
        For z = minZ To maxZ
            If WarstwyDlaPoziomu(warstwy, z).Count = 0 Then
                luki.Add(z)
            End If
        Next
        Return luki
    End Function

    ''' <summary>
    ''' Szybki szacunek rozmiaru pliku i liczby kafli metodą próbkowania statystycznego.
    ''' </summary>
    Public Shared Async Function OszacujRozmiarAsync(warstwy As List(Of WarstwaWielowarstwowa),
                                                     opcje As OpcjeKompilatoraMbtiles,
                                                     token As CancellationToken) As Task(Of SzacunekKompilacjiMbtiles)
        Return Await Task.Run(Function()
                                  Dim wynik As New SzacunekKompilacjiMbtiles()
                                  Dim los As New Random(54321)
                                  Dim minZ As Integer = Math.Max(0, Math.Min(opcje.PoziomMin, opcje.PoziomMax))
                                  Dim maxZ As Integer = Math.Min(22, Math.Max(opcje.PoziomMin, opcje.PoziomMax))

                                  For poziom = minZ To maxZ
                                      token.ThrowIfCancellationRequested()
                                      Dim aktywne = WarstwyDlaPoziomu(warstwy, poziom)
                                      If aktywne.Count = 0 AndAlso Not opcje.WypelnijLukiWarstwaNizsza Then
                                          Continue For
                                      End If

                                      Dim warstwyDoUzycia As List(Of WarstwaWielowarstwowa) = aktywne
                                      If warstwyDoUzycia.Count = 0 AndAlso opcje.WypelnijLukiWarstwaNizsza Then
                                          ' Pobierz najbliższą warstwę z niższego poziomu
                                          For p = poziom - 1 To 0 Step -1
                                              Dim nizsze = WarstwyDlaPoziomu(warstwy, p)
                                              If nizsze.Count > 0 Then
                                                  warstwyDoUzycia = nizsze
                                                  Exit For
                                              End If
                                          Next
                                      End If

                                      If warstwyDoUzycia.Count = 0 Then Continue For

                                      Dim obszarPoziomu = WyznaczObszarWgs84(warstwyDoUzycia)
                                      Dim zakresKafli = WebMercator.ZakresKafli(obszarPoziomu, poziom)
                                      Dim liczbaKafliPoziomu = WebMercator.LiczbaKafli(obszarPoziomu, poziom)

                                      Dim warstwyEksportu = warstwyDoUzycia.Select(Function(w) w.DoWarstwyEksportu()).ToList()

                                      ' Próbkowanie obecności danych
                                      Dim kandydaci As New List(Of Tuple(Of Integer, Integer))
                                      If liczbaKafliPoziomu <= 300 Then
                                          For ty = zakresKafli.Item2 To zakresKafli.Item4
                                              For tx = zakresKafli.Item1 To zakresKafli.Item3
                                                  kandydaci.Add(Tuple.Create(tx, ty))
                                              Next
                                          Next
                                      Else
                                          For i = 1 To 300
                                              kandydaci.Add(Tuple.Create(los.Next(zakresKafli.Item1, zakresKafli.Item3 + 1),
                                                                         los.Next(zakresKafli.Item2, zakresKafli.Item4 + 1)))
                                          Next
                                      End If

                                      Dim trafioneKafle As New List(Of Tuple(Of Integer, Integer))
                                      For Each k In kandydaci
                                          If EksportMapy.KafelMaDane(warstwyEksportu, k.Item1, k.Item2, poziom) Then
                                              trafioneKafle.Add(k)
                                          End If
                                      Next

                                      Dim wspolczynnikDanych As Double = If(kandydaci.Count > 0, trafioneKafle.Count / CDbl(kandydaci.Count), 0.0)
                                      Dim szacowanaLiczbaZDanymi As Long = CLng(Math.Round(liczbaKafliPoziomu * wspolczynnikDanych))

                                      ' Pomiar średniego rozmiaru kafla na próbkach rzeczywistych
                                      Dim probkiDoRenderu As New List(Of Tuple(Of Integer, Integer))
                                      If trafioneKafle.Count > 0 Then
                                          Dim ileProbek As Integer = Math.Min(6, trafioneKafle.Count)
                                          For i = 0 To ileProbek - 1
                                              probkiDoRenderu.Add(trafioneKafle(los.Next(trafioneKafle.Count)))
                                          Next
                                      End If

                                      Dim sumaBajtowProbek As Long = 0
                                      Dim zrenderowane As Integer = 0
                                      Dim srodekSz As Double = (obszarPoziomu.Polnoc + obszarPoziomu.Poludnie) / 2
                                      Dim nadprobkowanie As Integer = EksportMapy.LiczbaProbek(warstwyEksportu, WebMercator.RozmiarPiksela(srodekSz, poziom))

                                      For Each k In probkiDoRenderu
                                          token.ThrowIfCancellationRequested()
                                          Dim tx = k.Item1, ty = k.Item2
                                          Dim przelicz = EksportMapy.PrzeliczenieKafla(tx * 256.0, ty * 256.0, poziom)
                                          Using bmp = EksportMapy.RenderujKafel(warstwyEksportu, 256, 256, nadprobkowanie, przelicz)
                                              If bmp IsNot Nothing Then
                                                  Dim dane = KodujKafel(bmp, opcje)
                                                  If dane IsNot Nothing Then
                                                      sumaBajtowProbek += dane.Length
                                                      zrenderowane += 1
                                                  End If
                                              End If
                                          End Using
                                      Next

                                      Dim sredniRozmiar As Long = If(zrenderowane > 0, CLng(sumaBajtowProbek / zrenderowane), 35000L)
                                      Dim szacowaneBajtyPoziomu As Long = szacowanaLiczbaZDanymi * sredniRozmiar

                                      Dim sz As New SzacunekPoziomuMbtiles With {
                                          .Poziom = poziom,
                                          .LiczbaKafliSiatki = liczbaKafliPoziomu,
                                          .LiczbaKafliZDanymi = szacowanaLiczbaZDanymi,
                                          .SredniRozmiarKafla = sredniRozmiar,
                                          .RozmiarPoziomuBajty = szacowaneBajtyPoziomu,
                                          .AktywneWarstwyTekst = String.Join(" + ", warstwyDoUzycia.Select(Function(w) w.Tytul))
                                      }
                                      wynik.Poziomy(poziom) = sz
                                      wynik.LacznaLiczbaKafli += szacowanaLiczbaZDanymi
                                      wynik.LacznyRozmiarBajty += szacowaneBajtyPoziomu
                                  Next

                                  Return wynik
                              End Function, token)
    End Function

    ''' <summary>Koduje kafel według wybranych opcji kompresji.</summary>
    Private Shared Function KodujKafel(bmp As Bitmap, opcje As OpcjeKompilatoraMbtiles) As Byte()
        Select Case opcje.FormatKafli
            Case FormatKafliMbtiles.TylkoPng
                Return EksportMapy.Koduj(bmp, True, opcje.JakoscJpeg)
            Case FormatKafliMbtiles.TylkoJpeg
                Return EksportMapy.Koduj(bmp, False, opcje.JakoscJpeg, opcje.KolorTlaJpeg)
            Case Else
                ' SmartHybrid: jeśli ma przezroczystość -> PNG, w przeciwnym razie JPEG
                Dim maPrzezroczystosc As Boolean = EksportMapy.CzyPosiadaPrzezroczystosc(bmp)
                If maPrzezroczystosc Then
                    Return EksportMapy.Koduj(bmp, True, opcje.JakoscJpeg)
                Else
                    Return EksportMapy.Koduj(bmp, False, opcje.JakoscJpeg, opcje.KolorTlaJpeg)
                End If
        End Select
    End Function

    ''' <summary>
    ''' Renderuje kafel podglądu dla zadanych współrzędnych geograficznych i poziomu powiększenia.
    ''' </summary>
    Public Shared Function RenderujPodgladKafla(warstwy As List(Of WarstwaWielowarstwowa),
                                                szerokoscGeo As Double, dlugoscGeo As Double,
                                                poziom As Integer,
                                                opcje As OpcjeKompilatoraMbtiles) As Bitmap
        Dim aktywne = WarstwyDlaPoziomu(warstwy, poziom)
        If aktywne.Count = 0 AndAlso opcje.WypelnijLukiWarstwaNizsza Then
            For p = poziom - 1 To 0 Step -1
                Dim nizsze = WarstwyDlaPoziomu(warstwy, p)
                If nizsze.Count > 0 Then
                    aktywne = nizsze
                    Exit For
                End If
            Next
        End If
        If aktywne.Count = 0 Then Return Nothing

        Dim tx As Integer = CInt(Math.Floor(WebMercator.PikselX(dlugoscGeo, poziom) / WebMercator.RozmiarKafla))
        Dim ty As Integer = CInt(Math.Floor(WebMercator.PikselY(szerokoscGeo, poziom) / WebMercator.RozmiarKafla))
        Dim warstwyEksportu = aktywne.Select(Function(w) w.DoWarstwyEksportu()).ToList()

        Dim nadprobkowanie As Integer = EksportMapy.LiczbaProbek(warstwyEksportu, WebMercator.RozmiarPiksela(szerokoscGeo, poziom))
        Dim przelicz = EksportMapy.PrzeliczenieKafla(tx * 256.0, ty * 256.0, poziom)
        Return EksportMapy.RenderujKafel(warstwyEksportu, 256, 256, nadprobkowanie, przelicz)
    End Function

    ''' <summary>
    ''' Główna procedura kompilacji pliku MBTiles. Zwraca ścieżkę zapisanego pliku - zwykle PlikDocelowy, a gdy ten jest
    ''' otwarty w innym programie - plik obok niego z datą i godziną w nazwie.
    ''' </summary>
    Public Shared Async Function KompilujAsync(warstwy As List(Of WarstwaWielowarstwowa),
                                               opcje As OpcjeKompilatoraMbtiles,
                                               postep As IProgress(Of RaportPostepuKompilacji),
                                               token As CancellationToken) As Task(Of String)
        Dim plikTmp As String = opcje.PlikDocelowy & ".tmp"
        Return Await Task.Run(Function()
                                  Try
                                      KompilujSynchronie(warstwy, opcje, postep, token)
                                  Catch
                                      'po przerwaniu lub błędzie niedokończona baza (nawet kilka GB) nie może zostać na dysku
                                      UsunPlikTymczasowy(plikTmp)
                                      Throw
                                  End Try
                                  'podmiana pliku poza blokiem usuwającym bazę - gotowy wynik nie może zostać skasowany
                                  Dim zapisany As String = PrzeniesWynik(plikTmp, opcje.PlikDocelowy)
                                  postep?.Report(New RaportPostepuKompilacji With {
                                      .Procent = 100,
                                      .Gotowe = True,
                                      .Komunikat = If(zapisany = opcje.PlikDocelowy, "Kompilacja MBTiles zakończona pomyślnie!",
                                                      "Kompilacja zakończona. Plik docelowy jest otwarty w innym programie - mapę zapisano jako " & zapisany)
                                  })
                                  Return zapisany
                              End Function, token)
    End Function

    Private Shared Sub UsunPlikTymczasowy(plik As String)
        Try
            SQLiteConnection.ClearAllPools()
            EksportMapy.UsunPlikBazy(plik)
        Catch
            'plik chwilowo zablokowany - zostanie usunięty przy następnej kompilacji do tego pliku
        End Try
    End Sub

    Private Shared Sub KompilujSynchronie(warstwy As List(Of WarstwaWielowarstwowa),
                                          opcje As OpcjeKompilatoraMbtiles,
                                          postep As IProgress(Of RaportPostepuKompilacji),
                                          token As CancellationToken)
        Dim plikDocelowy = opcje.PlikDocelowy
        Dim plikTmp = plikDocelowy & ".tmp"
        'pozostałości poprzedniej, przerwanej kompilacji (także pliki dziennika bazy)
        EksportMapy.UsunPlikBazy(plikTmp)

        Dim folderDocelowy = Path.GetDirectoryName(plikDocelowy)
        If Not String.IsNullOrEmpty(folderDocelowy) AndAlso Not Directory.Exists(folderDocelowy) Then
            Directory.CreateDirectory(folderDocelowy)
        End If

        Dim minZ As Integer = Math.Max(0, Math.Min(opcje.PoziomMin, opcje.PoziomMax))
        Dim maxZ As Integer = Math.Min(22, Math.Max(opcje.PoziomMin, opcje.PoziomMax))
        Dim obszarGlobalny = WyznaczObszarWgs84(warstwy)

        ' Wyliczenie łącznej szacunkowej liczby kafli do paska postępu
        Dim lacznieKafli As Long = 0
        For p = minZ To maxZ
            Dim aktywne = WarstwyDlaPoziomu(warstwy, p)
            If aktywne.Count > 0 OrElse opcje.WypelnijLukiWarstwaNizsza Then
                lacznieKafli += WebMercator.LiczbaKafli(obszarGlobalny, p)
            End If
        Next
        lacznieKafli = Math.Max(1, lacznieKafli)
        Dim przetworzoneKafle As Long = 0

        ' Utworzenie bazy SQLite
        Dim connectionString = EksportMapy.ParametryPolaczenia(plikTmp, "Memory")
        Using conn As New SQLiteConnection(connectionString)
            conn.Open()
            Using cmd = conn.CreateCommand()
                cmd.CommandText = "PRAGMA page_size = 4096; PRAGMA cache_size = -64000; PRAGMA synchronous = OFF; PRAGMA journal_mode = MEMORY;"
                cmd.ExecuteNonQuery()

                ' Tworzenie tabel
                cmd.CommandText = "CREATE TABLE metadata (name text, value text);" &
                                  "CREATE TABLE tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob);" &
                                  "CREATE TABLE IF NOT EXISTS android_metadata (locale text);" &
                                  "INSERT INTO android_metadata VALUES ('pl_PL');"
                cmd.ExecuteNonQuery()
            End Using

            ' Wstawienie metadanych
            Dim ci = Globalization.CultureInfo.InvariantCulture
            Dim srodekSz As Double = (obszarGlobalny.Polnoc + obszarGlobalny.Poludnie) / 2
            Dim srodekDl As Double = (obszarGlobalny.Zachod + obszarGlobalny.Wschod) / 2
            Dim formatStr = If(opcje.FormatKafli = FormatKafliMbtiles.TylkoJpeg, "jpg", "png")

            Dim metadane As New Dictionary(Of String, String) From {
                {"name", opcje.NazwaMapy},
                {"type", "baselayer"},
                {"version", "1.3"},
                {"description", opcje.OpisMapy},
                {"format", formatStr},
                {"minzoom", minZ.ToString(ci)},
                {"maxzoom", maxZ.ToString(ci)},
                {"bounds", String.Format(ci, "{0},{1},{2},{3}", obszarGlobalny.Zachod, obszarGlobalny.Poludnie, obszarGlobalny.Wschod, obszarGlobalny.Polnoc)},
                {"center", String.Format(ci, "{0},{1},{2}", srodekDl, srodekSz, maxZ)},
                {"attribution", opcje.Atrybucja}
            }

            Using cmdMeta = conn.CreateCommand()
                cmdMeta.CommandText = "INSERT INTO metadata (name, value) VALUES (@n, @v);"
                Dim pN = cmdMeta.Parameters.Add("@n", DbType.String)
                Dim pV = cmdMeta.Parameters.Add("@v", DbType.String)
                For Each kv In metadane
                    pN.Value = kv.Key
                    pV.Value = kv.Value
                    cmdMeta.ExecuteNonQuery()
                Next
            End Using

            ' Polecenia wstawiania i odczytu kafli
            Using cmdInsert = conn.CreateCommand(), cmdSelect = conn.CreateCommand()
                cmdInsert.CommandText = "INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES (@z, @x, @y, @d);"
                Dim pZ = cmdInsert.Parameters.Add("@z", DbType.Int32)
                Dim pX = cmdInsert.Parameters.Add("@x", DbType.Int32)
                Dim pY = cmdInsert.Parameters.Add("@y", DbType.Int32)
                Dim pD = cmdInsert.Parameters.Add("@d", DbType.Binary)

                cmdSelect.CommandText = "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y;"
                Dim sZ = cmdSelect.Parameters.Add("@z", DbType.Int32)
                Dim sX = cmdSelect.Parameters.Add("@x", DbType.Int32)
                Dim sY = cmdSelect.Parameters.Add("@y", DbType.Int32)

                ' Funkcja odczytu kafla z bazy do piramidy
                Dim odczytajKafel = Function(k As KafelXYZ) As Byte()
                                        sZ.Value = k.Z
                                        sX.Value = k.X
                                        sY.Value = k.WierszTms
                                        Return TryCast(cmdSelect.ExecuteScalar(), Byte())
                                    End Function

                ' Generowanie poziomów: od najwyższego do najniższego
                ' Aby móc składać poziomy niższe z wyższych (piramida), idziemy w dół
                For poziom = maxZ To minZ Step -1
                    token.ThrowIfCancellationRequested()

                    Dim aktywne = WarstwyDlaPoziomu(warstwy, poziom)
                    If aktywne.Count = 0 AndAlso opcje.WypelnijLukiWarstwaNizsza Then
                        For p = poziom - 1 To 0 Step -1
                            Dim nizsze = WarstwyDlaPoziomu(warstwy, p)
                            If nizsze.Count > 0 Then
                                aktywne = nizsze
                                Exit For
                            End If
                        Next
                    End If

                    If aktywne.Count = 0 Then
                        Continue For
                    End If

                    Dim obszarPoziomu = WyznaczObszarWgs84(aktywne)
                    Dim zakres = WebMercator.ZakresKafli(obszarPoziomu, poziom)
                    Dim warstwyEksportu = aktywne.Select(Function(w) w.DoWarstwyEksportu()).ToList()

                    Dim srodekSzPoziomu As Double = (obszarPoziomu.Polnoc + obszarPoziomu.Poludnie) / 2
                    Dim nadprobkowanie As Integer = EksportMapy.LiczbaProbek(warstwyEksportu, WebMercator.RozmiarPiksela(srodekSzPoziomu, poziom))

                    ' Sprawdzenie, czy ten poziom można wygenerować szybką piramidą 2x z poziomu poziom+1
                    ' (Możliwe tylko wtedy, gdy poziom+1 istnieje w bazy i ma ten sam zestaw warstw!)
                    Dim moznaPiramida As Boolean = False
                    If opcje.SzybkaPiramidaWPasmach AndAlso poziom < maxZ Then
                        Dim warstwyWyzej = WarstwyDlaPoziomu(warstwy, poziom + 1)
                        ' Porównanie zestawu warstw
                        If warstwyWyzej.Count = aktywne.Count AndAlso
                           warstwyWyzej.All(Function(w1) aktywne.Any(Function(w2) w2.Folder = w1.Folder)) Then
                            moznaPiramida = True
                        End If
                    End If

                    ' Przygotowanie listy współrzędnych do przetworzenia na tym poziomie
                    Dim kaflePoziomu As New List(Of KafelXYZ)
                    For ty = zakres.Item2 To zakres.Item4
                        For tx = zakres.Item1 To zakres.Item3
                            kaflePoziomu.Add(New KafelXYZ(tx, ty, poziom))
                        Next
                    Next

                    Const RozmiarPaczki As Integer = 128
                    Dim po As New ParallelOptions With {
                        .CancellationToken = token,
                        .MaxDegreeOfParallelism = Environment.ProcessorCount
                    }

                    Using trans = conn.BeginTransaction()
                        cmdInsert.Transaction = trans
                        cmdSelect.Transaction = trans

                        For offset = 0 To kaflePoziomu.Count - 1 Step RozmiarPaczki
                            token.ThrowIfCancellationRequested()
                            Dim biezacyOffset = offset
                            Dim liczbaWPaczce = Math.Min(RozmiarPaczki, kaflePoziomu.Count - biezacyOffset)
                            Dim daneKafli As Byte()() = New Byte(liczbaWPaczce - 1)() {}

                            If moznaPiramida Then
                                Dim suroweDzieci As Byte()()() = New Byte(liczbaWPaczce - 1)()() {}
                                For i = 0 To liczbaWPaczce - 1
                                    Dim k = kaflePoziomu(biezacyOffset + i)
                                    suroweDzieci(i) = New Byte(3)() {}
                                    For dy = 0 To 1
                                        For dx = 0 To 1
                                            suroweDzieci(i)(dy * 2 + dx) = odczytajKafel(New KafelXYZ(k.X * 2 + dx, k.Y * 2 + dy, poziom + 1))
                                        Next
                                    Next
                                Next

                                Parallel.For(0, liczbaWPaczce, po, Sub(i)
                                                                       daneKafli(i) = KafelZPiramidyZDanych(suroweDzieci(i), opcje)
                                                                   End Sub)
                            Else
                                Dim biezacyPoziom = poziom
                                Dim lokalneWarstwy = warstwyEksportu
                                Dim lokalneNadprobkowanie = nadprobkowanie
                                Dim lokalneOpcje = opcje
                                Parallel.For(0, liczbaWPaczce, po, Sub(i)
                                                                       Dim k = kaflePoziomu(biezacyOffset + i)
                                                                       If EksportMapy.KafelMaDane(lokalneWarstwy, k.X, k.Y, biezacyPoziom) Then
                                                                           Dim przelicz = EksportMapy.PrzeliczenieKafla(k.X * 256.0, k.Y * 256.0, biezacyPoziom)
                                                                           Using bmp = EksportMapy.RenderujKafel(lokalneWarstwy, 256, 256, lokalneNadprobkowanie, przelicz)
                                                                               If bmp IsNot Nothing Then
                                                                                   daneKafli(i) = KodujKafel(bmp, lokalneOpcje)
                                                                               End If
                                                                           End Using
                                                                       End If
                                                                   End Sub)
                            End If

                            For i = 0 To liczbaWPaczce - 1
                                Dim dane = daneKafli(i)
                                If dane IsNot Nothing Then
                                    Dim k = kaflePoziomu(biezacyOffset + i)
                                    pZ.Value = k.Z
                                    pX.Value = k.X
                                    pY.Value = k.WierszTms
                                    pD.Value = dane
                                    cmdInsert.ExecuteNonQuery()
                                End If
                            Next

                            przetworzoneKafle += liczbaWPaczce
                            If postep IsNot Nothing Then
                                Dim proc As Integer = CInt(Math.Min(98, przetworzoneKafle * 98 \ lacznieKafli))
                                Dim ostKafel = kaflePoziomu(biezacyOffset + liczbaWPaczce - 1)
                                postep.Report(New RaportPostepuKompilacji With {
                                    .Procent = proc,
                                    .AktualnyPoziom = poziom,
                                    .WykonaneKafle = przetworzoneKafle,
                                    .WszystkieKafle = lacznieKafli,
                                    .Komunikat = "Poziom " & poziom & " (" & ostKafel.X & "," & ostKafel.Y & ") - " & proc & "% [wielowątkowo]"
                                })
                            End If
                        Next
                        trans.Commit()
                    End Using
                Next
            End Using

            ' Tworzenie unikalnego indeksu PO wstawieniu danych (szybki zapis masowy)
            If postep IsNot Nothing Then
                postep.Report(New RaportPostepuKompilacji With {
                    .Procent = 98,
                    .Komunikat = "Tworzenie indeksu kafli i optymalizacja bazy..."
                })
            End If

            Using cmdIndex = conn.CreateCommand()
                cmdIndex.CommandText = "CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);"
                cmdIndex.ExecuteNonQuery()
            End Using

            ' Zamknięcie w trybie DELETE (zgodność z Androidem i kartami microSD)
            Using cmdFinal = conn.CreateCommand()
                cmdFinal.CommandText = "PRAGMA journal_mode = DELETE; PRAGMA locking_mode = NORMAL;"
                cmdFinal.ExecuteNonQuery()
            End Using
        End Using

        ' Wyczyszczenie puli połączeń SQLite przed podmianą pliku
        SQLiteConnection.ClearAllPools()
    End Sub

    ''' <summary>
    ''' Zastępuje plik docelowy gotową bazą. Gdy starego pliku nie da się usunąć (np. jest otwarty w QGIS albo innym
    ''' programie), gotowa baza zapisywana jest pod nazwą z datą i godziną - dawniej błąd podmiany kasował cały wynik.
    ''' Zwraca ścieżkę zapisanego pliku.
    ''' </summary>
    Private Shared Function PrzeniesWynik(plikTmp As String, plikDocelowy As String) As String
        Try
            File.Move(plikTmp, plikDocelowy, overwrite:=True)
            Return plikDocelowy
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Dim zastepczy As String = Path.Combine(If(Path.GetDirectoryName(plikDocelowy), ""),
                                                   Path.GetFileNameWithoutExtension(plikDocelowy) & "_" &
                                                   DateTime.Now.ToString("yyyyMMdd_HHmmss", Globalization.CultureInfo.InvariantCulture) &
                                                   Path.GetExtension(plikDocelowy))
            File.Move(plikTmp, zastepczy)
            Return zastepczy
        End Try
    End Function

    ''' <summary>Tworzy kafel z 4 kafli wyższego poziomu przekazanych bezpośrednio jako tablica bajtów.</summary>
    Private Shared Function KafelZPiramidyZDanych(dzieci As Byte()(), opcje As OpcjeKompilatoraMbtiles) As Byte()
        If dzieci Is Nothing OrElse dzieci.All(Function(d) d Is Nothing) Then Return Nothing
        Using duzy As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
            Dim jest As Boolean = False
            Using g = Graphics.FromImage(duzy)
                g.Clear(Color.Transparent)
                For dy = 0 To 1
                    For dx = 0 To 1
                        Dim dane = dzieci(dy * 2 + dx)
                        If dane Is Nothing Then Continue For
                        Using ms As New MemoryStream(dane), obraz = Image.FromStream(ms)
                            g.DrawImage(obraz, New Rectangle(dx * 256, dy * 256, 256, 256))
                            jest = True
                        End Using
                    Next
                Next
            End Using
            If Not jest Then Return Nothing

            Using maly As New Bitmap(256, 256, PixelFormat.Format32bppArgb)
                Using g = Graphics.FromImage(maly)
                    g.Clear(Color.Transparent)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    g.DrawImage(duzy, New Rectangle(0, 0, 256, 256), 0, 0, 512, 512, GraphicsUnit.Pixel)
                End Using
                Return KodujKafel(maly, opcje)
            End Using
        End Using
    End Function

End Class
