Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

    ''' <summary>
    ''' Typ wykrytego nośnika w odbiorniku Garmin pracującym w trybie USB Mass Storage.
    ''' </summary>
    Public Enum TypNosnikaGarmina
        PamiecWewnetrzna = 0
        KartaMicroSd = 1
    End Enum

    ''' <summary>
    ''' Informacje o pojedynczym woluminie zawierającym katalog \Garmin\.
    ''' </summary>
    Public NotInheritable Class DyskGarmina
        Public Property LiteraDysku As String = String.Empty
        Public Property EtykietaWoluminu As String = String.Empty
        Public Property TypNosnika As TypNosnikaGarmina
        Public Property WolneMiejsceBajty As Long
        Public Property CalkowityRozmiarBajty As Long
        Public Property SciezkaKataloguGarmin As String = String.Empty

        ''' <summary>
        ''' Czytelny opis nośnika do wyświetlenia na liście wyboru w interfejsie.
        ''' </summary>
        Public Overrides Function ToString() As String
            Dim opisTypu As String = If(TypNosnika = TypNosnikaGarmina.PamiecWewnetrzna,
                                        "Pamięć wewnętrzna",
                                        "Karta microSD")
            Dim etykieta As String = If(String.IsNullOrWhiteSpace(EtykietaWoluminu), "Garmin", EtykietaWoluminu.Trim())
            Dim dyskBezSlasha As String = LiteraDysku.TrimEnd("\"c)
            Return String.Format("{0} ({1}, {2}) – {3} wolne", etykieta, opisTypu, dyskBezSlasha, FormatujRozmiar(WolneMiejsceBajty))
        End Function

        Public Shared Function FormatujRozmiar(bajty As Long) As String
            If bajty < 1024L Then Return String.Format("{0} B", bajty)
            Dim kb As Double = bajty / 1024.0
            If kb < 1024.0 Then Return String.Format(CultureInfo.InvariantCulture, "{0:0.0} KB", kb)
            Dim mb As Double = kb / 1024.0
            If mb < 1024.0 Then Return String.Format(CultureInfo.InvariantCulture, "{0:0.0} MB", mb)
            Dim gb As Double = mb / 1024.0
            Return String.Format(CultureInfo.InvariantCulture, "{0:0.00} GB", gb)
        End Function
    End Class

    ''' <summary>
    ''' Logika wykrywania odbiorników Garmin w trybie pamięci masowej USB,
    ''' sanitacji nazw plików oraz bezpiecznego kopiowania z kontrolą wolnego miejsca.
    ''' </summary>
    Public NotInheritable Class MenadzerGarminUsb

        ''' <summary>
        ''' Minimalny zapas wolnego miejsca (16 MB), aby nie zapchać pamięci systemowej urządzenia.
        ''' </summary>
        Public Const MarginesWolnegoMiejscaBajty As Long = 16L * 1024L * 1024L

        ''' <summary>
        ''' Skanuje podłączone woluminy i zwraca dyski zawierające folder \Garmin\.
        ''' Rozróżnia pamięć wewnętrzną odbiornika od karty microSD.
        ''' </summary>
        Public Shared Function WykryjDyskiGarmina() As List(Of DyskGarmina)
            Dim wynik As New List(Of DyskGarmina)()

            For Each drive As DriveInfo In DriveInfo.GetDrives()
                Try
                    If drive.DriveType <> DriveType.Removable AndAlso drive.DriveType <> DriveType.Fixed Then
                        Continue For
                    End If

                    If Not drive.IsReady Then Continue For

                    Dim sciezkaGarmin As String = Path.Combine(drive.RootDirectory.FullName, "Garmin")
                    If Not Directory.Exists(sciezkaGarmin) Then Continue For

                    ' Pomijamy partycję systemową Windows, gdyby użytkownik miał tam lokalny folder C:\Garmin
                    Dim dyskSystemowy As String = Path.GetPathRoot(Environment.SystemDirectory)
                    If String.Equals(drive.RootDirectory.FullName, dyskSystemowy, StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Dim typNosnika As TypNosnikaGarmina = RozpoznajTypNosnika(drive, sciezkaGarmin)

                    wynik.Add(New DyskGarmina() With {
                        .LiteraDysku = drive.RootDirectory.FullName,
                        .EtykietaWoluminu = drive.VolumeLabel,
                        .TypNosnika = typNosnika,
                        .WolneMiejsceBajty = drive.AvailableFreeSpace,
                        .CalkowityRozmiarBajty = drive.TotalSize,
                        .SciezkaKataloguGarmin = sciezkaGarmin
                    })
                Catch ex As IOException
                    ' Dysk odłączony w trakcie skanowania – pomijamy
                Catch ex As UnauthorizedAccessException
                    ' Brak uprawnień do woluminu – pomijamy
                End Try
            Next

            ' Preferujemy kartę microSD na górze listy (bezpieczniejsza dla dużych plików JNX/KMZ)
            wynik.Sort(Function(a, b)
                           Dim cmpTyp As Integer = b.TypNosnika.CompareTo(a.TypNosnika)
                           If cmpTyp <> 0 Then Return cmpTyp
                           Return b.WolneMiejsceBajty.CompareTo(a.WolneMiejsceBajty)
                       End Function)

            Return wynik
        End Function

        ''' <summary>
        ''' Rozróżnia pamięć wewnętrzną Garmina od karty microSD na podstawie plików systemowych urządzenia.
        ''' </summary>
        Private Shared Function RozpoznajTypNosnika(drive As DriveInfo, sciezkaGarmin As String) As TypNosnikaGarmina
            Dim plikUrzadzenia As String = Path.Combine(sciezkaGarmin, "GarminDevice.xml")
            Dim mapaBazowa As String = Path.Combine(sciezkaGarmin, "gmapbmap.img")

            If File.Exists(plikUrzadzenia) OrElse File.Exists(mapaBazowa) Then
                Return TypNosnikaGarmina.PamiecWewnetrzna
            End If

            Dim etykieta As String = If(drive.VolumeLabel, String.Empty).ToUpperInvariant()
            If etykieta.Contains("GARMIN") AndAlso Not etykieta.Contains("SD") Then
                Return TypNosnikaGarmina.PamiecWewnetrzna
            End If

            Return TypNosnikaGarmina.KartaMicroSd
        End Function

        ''' <summary>
        ''' Przygotowuje bezpieczną nazwę pliku dla starszych odbiorników Garmin:
        ''' 1) jawnie zamienia ł/Ł na l/L (Normalize FormD nie rozkłada tych liter),
        ''' 2) usuwa pozostałe znaki diakrytyczne (ą, ć, ę, ń, ó, ś, ź, ż itd.),
        ''' 3) usuwa spacje i znaki spoza bezpiecznego podzbioru ASCII.
        ''' </summary>
        Public Shared Function SanityzujNazwePlikuGarmina(oryginalnaNazwa As String) As String
            If String.IsNullOrWhiteSpace(oryginalnaNazwa) Then
                Return "mapa.kmz"
            End If

            Dim nazwaBezRozszerzenia As String = Path.GetFileNameWithoutExtension(oryginalnaNazwa)
            Dim rozszerzenie As String = Path.GetExtension(oryginalnaNazwa).ToLowerInvariant()

            ' Krok 1: jawna zamiana znaków niepoddających się dekompozycji Unicode FormD
            Dim wstepna As String = nazwaBezRozszerzenia _
                .Replace("ł", "l") _
                .Replace("Ł", "L") _
                .Replace("ß", "ss")

            ' Krok 2: rozkład Unicode (FormD) i odrzucenie znaków diakrytycznych
            Dim znormalizowana As String = wstepna.Normalize(NormalizationForm.FormD)
            Dim sb As New StringBuilder(znormalizowana.Length)
            Dim poprzedniPodkreslnik As Boolean = False

            For Each znak As Char In znormalizowana
                Dim kategoria As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(znak)
                If kategoria = UnicodeCategory.NonSpacingMark Then
                    Continue For
                End If

                ' Krok 3: akceptujemy wyłącznie bezpieczne znaki ASCII; spacje i separatory zamieniamy na '_'
                If (znak >= "a"c AndAlso znak <= "z"c) OrElse
                   (znak >= "A"c AndAlso znak <= "Z"c) OrElse
                   (znak >= "0"c AndAlso znak <= "9"c) OrElse
                   znak = "-"c Then
                    sb.Append(znak)
                    poprzedniPodkreslnik = False
                ElseIf Char.IsWhiteSpace(znak) OrElse znak = "_"c OrElse znak = "."c Then
                    If Not poprzedniPodkreslnik AndAlso sb.Length > 0 Then
                        sb.Append("_"c)
                        poprzedniPodkreslnik = True
                    End If
                End If
            Next

            Dim wynikBazowy As String = sb.ToString().Trim("_"c, "-"c)
            If String.IsNullOrEmpty(wynikBazowy) Then
                wynikBazowy = "mapa"
            End If

            Const maksDlugosc As Integer = 48
            If wynikBazowy.Length > maksDlugosc Then
                wynikBazowy = wynikBazowy.Substring(0, maksDlugosc).TrimEnd("_"c, "-"c)
            End If

            Return wynikBazowy & rozszerzenie
        End Function

        ''' <summary>
        ''' Wyznacza docelowy podfolder na odbiorniku Garmin na podstawie rozszerzenia pliku mapy.
        ''' </summary>
        Public Shared Function PobierzPodfolderDocelowy(sciezkaZrodlowa As String) As String
            Dim ext As String = Path.GetExtension(sciezkaZrodlowa).ToLowerInvariant()
            Select Case ext
                Case ".kmz"
                    Return "CustomMaps"
                Case ".jnx"
                    Return "BirdsEye"
                Case Else
                    Throw New NotSupportedException(String.Format("Format '{0}' nie jest obsługiwany przez automatyczne kopiowanie na Garmina (obsługiwane: .kmz, .jnx).", ext))
            End Select
        End Function

        Friend Shared Function NazwaDocelowaPliku(sciezkaZrodlowa As String, nazwaDocelowa As String) As String
            Dim extZrodla As String = Path.GetExtension(sciezkaZrodlowa).ToLowerInvariant()
            Dim surowaNazwa As String
            If String.IsNullOrWhiteSpace(nazwaDocelowa) Then
                surowaNazwa = Path.GetFileName(sciezkaZrodlowa)
            Else
                Dim przycieta = nazwaDocelowa.Trim()
                If przycieta.EndsWith(extZrodla, StringComparison.OrdinalIgnoreCase) AndAlso przycieta.Length > extZrodla.Length Then
                    surowaNazwa = przycieta
                Else
                    surowaNazwa = przycieta & extZrodla
                End If
            End If
            Return SanityzujNazwePlikuGarmina(surowaNazwa)
        End Function

        ''' <summary>
        ''' Kopiuje plik mapy na wybrany nośnik Garmina ze sprawdzeniem wolnego miejsca i raportowaniem postępu.
        ''' </summary>
        Public Shared Async Function KopiujNaGarminaAsync(
            sciezkaZrodlowa As String,
            dysk As DyskGarmina,
            Optional nazwaDocelowa As String = Nothing,
            Optional postep As IProgress(Of Double) = Nothing,
            Optional ct As CancellationToken = Nothing) As Task(Of String)

            If Not File.Exists(sciezkaZrodlowa) Then
                Throw New FileNotFoundException("Nie znaleziono pliku źródłowego mapy.", sciezkaZrodlowa)
            End If

            Dim infoPliku As New FileInfo(sciezkaZrodlowa)
            Dim aktualnyDysk As New DriveInfo(dysk.LiteraDysku)
            If Not aktualnyDysk.IsReady Then
                Throw New IOException(String.Format("Nośnik {0} przestał być dostępny.", dysk.LiteraDysku))
            End If

            Dim wymaganeMiejsce As Long = infoPliku.Length + MarginesWolnegoMiejscaBajty
            If aktualnyDysk.AvailableFreeSpace < wymaganeMiejsce Then
                Throw New IOException(
                    String.Format("Za mało wolnego miejsca na nośniku {0}. Wymagane: {1} (+16 MB zapasu), dostępne: {2}.",
                                  dysk.LiteraDysku, DyskGarmina.FormatujRozmiar(infoPliku.Length), DyskGarmina.FormatujRozmiar(aktualnyDysk.AvailableFreeSpace)))
            End If

            Dim podfolder As String = PobierzPodfolderDocelowy(sciezkaZrodlowa)
            Dim katalogDocelowy As String = Path.Combine(dysk.SciezkaKataloguGarmin, podfolder)
            Directory.CreateDirectory(katalogDocelowy)

            Dim bezpiecznaNazwa As String = NazwaDocelowaPliku(sciezkaZrodlowa, nazwaDocelowa)
            'nie nadpisujemy mapy w odbiorniku: po zamianie polskich liter różne pliki („Łódź”, „Lodz”) mają tę samą nazwę
            Dim sciezkaDocelowa As String = WolnaNazwaPliku(katalogDocelowy, bezpiecznaNazwa)
            Dim sciezkaTmp As String = sciezkaDocelowa & ".tmp"

            Const rozmiarBufora As Integer = 256 * 1024
            Dim bufor(rozmiarBufora - 1) As Byte
            Dim memBufor As New Memory(Of Byte)(bufor)
            Dim skopiowano As Long = 0

            Try
                Using fsZrodlo As New FileStream(sciezkaZrodlowa, FileMode.Open, FileAccess.Read, FileShare.Read, rozmiarBufora, useAsync:=True),
                      fsCel As New FileStream(sciezkaTmp, FileMode.Create, FileAccess.Write, FileShare.None, rozmiarBufora, useAsync:=True)

                    While True
                        ct.ThrowIfCancellationRequested()
                        Dim odczytano As Integer = Await fsZrodlo.ReadAsync(memBufor, ct).ConfigureAwait(False)
                        If odczytano = 0 Then Exit While

                        Await fsCel.WriteAsync(New ReadOnlyMemory(Of Byte)(bufor, 0, odczytano), ct).ConfigureAwait(False)
                        skopiowano += odczytano

                        If infoPliku.Length > 0 AndAlso postep IsNot Nothing Then
                            postep.Report(Math.Min(100.0, (skopiowano * 100.0) / infoPliku.Length))
                        End If
                    End While

                    Await fsCel.FlushAsync(ct).ConfigureAwait(False)
                End Using

                File.Move(sciezkaTmp, sciezkaDocelowa, overwrite:=False)
                Return sciezkaDocelowa
            Catch
                Try
                    If File.Exists(sciezkaTmp) Then File.Delete(sciezkaTmp)
                Catch
                End Try
                Throw
            End Try
        End Function

        ''' <summary>Ścieżka pliku w katalogu, która jeszcze nie istnieje: nazwa, nazwa_2, nazwa_3…</summary>
        Public Shared Function WolnaNazwaPliku(katalog As String, nazwa As String) As String
            Dim sciezka As String = Path.Combine(katalog, nazwa)
            Dim rdzen As String = Path.GetFileNameWithoutExtension(nazwa)
            Dim rozszerzenie As String = Path.GetExtension(nazwa)
            Dim numer As Integer = 2
            While File.Exists(sciezka)
                sciezka = Path.Combine(katalog, rdzen & "_" & numer.ToString(CultureInfo.InvariantCulture) & rozszerzenie)
                numer += 1
            End While
            Return sciezka
        End Function

        ''' <summary>
        ''' Otwiera Eksplorator Windows z zaznaczonym plikiem mapy (ścieżka zalecana dla nowszych Garminów MTP).
        ''' </summary>
        Public Shared Sub PokazWFolderze(sciezkaPliku As String)
            If String.IsNullOrWhiteSpace(sciezkaPliku) OrElse Not File.Exists(sciezkaPliku) Then
                Return
            End If

            Dim psi As New ProcessStartInfo() With {
                .FileName = "explorer.exe",
                .Arguments = String.Format("/select,""{0}""", sciezkaPliku),
                .UseShellExecute = True
            }
            Using p = Process.Start(psi)
            End Using
        End Sub
    End Class
