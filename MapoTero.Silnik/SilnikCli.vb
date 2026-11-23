Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Standardowe kody wyjścia zwracane przez MapoTero.exe w trybie wiersza poleceń (CLI).
''' </summary>
Public Enum KodWyjsciaCli
    Sukces = 0
    BledneArgumenty = 1
    BrakPlikuSesji = 2
    BladPobierania = 3
    BladEksportu = 4
    NieoczekiwanyBlad = 99
    AnulowanoPrzezUzytkownika = 130
End Enum

''' <summary>
''' Sparsowane argumenty wiersza poleceń dla wywołania bezokienkowego.
''' </summary>
Public NotInheritable Class OpcjeCli
    Public Property SciezkaSesji As String = String.Empty
    Public Property FormatEksportu As String = String.Empty ' np. kmz, jnx, mbtiles, sqlitedb, orux, geotiff, gpkg
    Public Property SciezkaWyjsciowa As String = String.Empty
    Public Property SciezkaPoligonu As String = String.Empty
    Public Property TylkoEksport As Boolean = False
    Public Property NadpiszIstniejacy As Boolean = False
    Public Property TrybCichy As Boolean = False
    Public Property PokazPomoc As Boolean = False
End Class

''' <summary>
''' Raport postępu przekazywany z silnika pobierania / eksportu do konsoli CLI.
''' </summary>
Public Structure PostepCli
    Public Etap As String
    Public Wykonano As Integer
    Public Razem As Integer
    Public Komunikat As String

    Public ReadOnly Property Procent As Double
        Get
            If Razem <= 0 Then Return 0.0
            Return Math.Min(100.0, (Wykonano * 100.0) / Razem)
        End Get
    End Property
End Structure

''' <summary>
''' Kontrakt podpinający moduły MapoTero (PobieranieSegmentow, EksportMapy)
''' bez tworzenia i bez zależności od okna Form1.
''' </summary>
Public Interface IWykonawcaSesjiCli
    Function PobierzSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task
    Function EksportujSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task(Of String)
End Interface

''' <summary>
''' Bezpośredni, wątkowo bezpieczny reporter postępu dla konsoli CLI.
''' Celowo nie dziedziczy po System.Progress(Of T), aby nie przechwytywać WindowsFormsSynchronizationContext.
''' </summary>
Friend NotInheritable Class ReporterPostepuKonsoli
    Implements IProgress(Of PostepCli)

    Private ReadOnly _lockKonsola As New Object()
    Private ReadOnly _trybCichy As Boolean
    Private ReadOnly _czyInteraktywna As Boolean
    Private _ostatniProcent As Integer = -1
    Private _ostatniEtap As String = String.Empty

    Public Sub New(trybCichy As Boolean, czyInteraktywna As Boolean)
        _trybCichy = trybCichy
        _czyInteraktywna = czyInteraktywna
    End Sub

    Public Sub ResetujEtap()
        SyncLock _lockKonsola
            _ostatniProcent = -1
            _ostatniEtap = String.Empty
        End SyncLock
    End Sub

    Public Sub Report(value As PostepCli) Implements IProgress(Of PostepCli).Report
        If _trybCichy Then Return

        SyncLock _lockKonsola
            Dim procInt As Integer = CInt(Math.Floor(value.Procent))
            If procInt = _ostatniProcent AndAlso value.Etap = _ostatniEtap Then Return
            _ostatniProcent = procInt
            _ostatniEtap = value.Etap

            Dim linia As String = $"[{value.Etap}] {procInt,3}% ({value.Wykonano}/{value.Razem}) {value.Komunikat}"
            If _czyInteraktywna Then
                Console.Write(ControlChars.Cr & linia.PadRight(78))
            Else
                Console.WriteLine(linia)
            End If
        End SyncLock
    End Sub
End Class

''' <summary>
''' Główny silnik trybu konsolowego CLI dla aplikacji WinForms (.NET 10).
''' Podpina się do konsoli rodzica (cmd / PowerShell), raportuje postęp i ustawia Environment.ExitCode.
''' </summary>
Public NotInheritable Class SilnikCli

    Private Const AttachParentProcess As Integer = -1
    Private Const STD_OUTPUT_HANDLE As Integer = -11
    Private Const FILE_TYPE_UNKNOWN As UInteger = 0UI

    Private Shared ReadOnly DozwoloneFormatyEksportu As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "kmz", "jnx", "mbtiles", "sqlitedb", "orux", "oruxmaps", "geotiff", "tif", "tiff", "arkusz", "gpkg", "geopackage", "trekbuddy", "tar"
    }

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function AttachConsole(dwProcessId As Integer) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function FreeConsole() As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetStdHandle(nStdHandle As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetFileType(hFile As IntPtr) As UInteger
    End Function

    ''' <summary>
    ''' Sprawdza, czy lista argumentów wskazuje na uruchomienie w trybie konsolowym CLI.
    ''' </summary>
    Public Shared Function CzyTrybCli(argumenty As IEnumerable(Of String)) As Boolean
        If argumenty Is Nothing Then Return False
        For Each arg As String In argumenty
            Dim a As String = arg.Trim().ToLowerInvariant()
            If a = "--sesja" OrElse a = "-s" OrElse a = "--pomoc" OrElse a = "--help" OrElse a = "-h" Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Punkt wejścia wywoływany z MyApplication_Startup.
    ''' Zwraca kod wyjścia dla systemu operacyjnego.
    ''' </summary>
    Public Shared Function Uruchom(argumenty As String(), wykonawca As IWykonawcaSesjiCli) As KodWyjsciaCli
        Dim podpietoKonsole As Boolean = False

        ' Jeśli stdout nie jest podpięty / jest nieznany (typowe dla aplikacji WinExe) i nie jest przekierowany, podpinamy konsolę rodzica
        Dim hOut As IntPtr = GetStdHandle(STD_OUTPUT_HANDLE)
        Dim brakUchwytu As Boolean = (hOut = IntPtr.Zero OrElse hOut = New IntPtr(-1) OrElse GetFileType(hOut) = FILE_TYPE_UNKNOWN)
        If brakUchwytu OrElse Not Console.IsOutputRedirected Then
            podpietoKonsole = AttachConsole(AttachParentProcess)
            If podpietoKonsole Then
                Try
                    Dim swOut As New StreamWriter(Console.OpenStandardOutput()) With {.AutoFlush = True}
                    Dim swErr As New StreamWriter(Console.OpenStandardError()) With {.AutoFlush = True}
                    Console.SetOut(swOut)
                    Console.SetError(swErr)
                Catch
                End Try
            End If
        End If

        Try
            Dim opcje As OpcjeCli = Nothing
            Dim bladParsowania As String = String.Empty
            If Not ParsujArgumenty(argumenty, opcje, bladParsowania) Then
                Console.Error.WriteLine($"Błąd argumentów: {bladParsowania}")
                WypiszPomoc()
                Return KodWyjsciaCli.BledneArgumenty
            End If

            If opcje.PokazPomoc Then
                WypiszPomoc()
                Return KodWyjsciaCli.Sukces
            End If

            If Not File.Exists(opcje.SciezkaSesji) AndAlso Not Directory.Exists(opcje.SciezkaSesji) Then
                Console.Error.WriteLine($"Nie znaleziono pliku ani folderu sesji: {opcje.SciezkaSesji}")
                Return KodWyjsciaCli.BrakPlikuSesji
            End If

            ' Obsługa Ctrl+C w konsoli
            Using cts As New CancellationTokenSource()
                Dim zakonczono As Integer = 0
                Dim handlerAnulowania As ConsoleCancelEventHandler =
                    Sub(sender, e)
                        e.Cancel = True
                        If Threading.Volatile.Read(zakonczono) <> 0 Then Return
                        Try
                            cts.Cancel()
                        Catch ex As ObjectDisposedException
                            Return
                        End Try
                        Console.Error.WriteLine()
                        Console.Error.WriteLine("Otrzymano sygnał przerwania (Ctrl+C) – bezpieczne kończenie...")
                    End Sub

                AddHandler Console.CancelKeyPress, handlerAnulowania
                Try
                    Return WykonajZadanieAsync(opcje, wykonawca, cts.Token).GetAwaiter().GetResult()
                Finally
                    Threading.Volatile.Write(zakonczono, 1)
                    RemoveHandler Console.CancelKeyPress, handlerAnulowania
                End Try
            End Using
        Catch ex As OperationCanceledException
            Return KodWyjsciaCli.AnulowanoPrzezUzytkownika
        Catch ex As Exception
            Console.Error.WriteLine($"Nieoczekiwany błąd krytyczny: {ex.Message}")
            Return KodWyjsciaCli.NieoczekiwanyBlad
        Finally
            If podpietoKonsole Then
                Console.Out.Flush()
                Console.Error.Flush()
                FreeConsole()
            End If
        End Try
    End Function

    Private Shared Async Function WykonajZadanieAsync(
        opcje As OpcjeCli,
        wykonawca As IWykonawcaSesjiCli,
        ct As CancellationToken) As Task(Of KodWyjsciaCli)

        Dim czyInteraktywna As Boolean = Not Console.IsOutputRedirected
        Dim reporter As New ReporterPostepuKonsoli(opcje.TrybCichy, czyInteraktywna)

        ' Etap 1: Pobieranie brakujących segmentów sesji
        If Not opcje.TylkoEksport Then
            Try
                If Not opcje.TrybCichy Then
                    Console.WriteLine($"Start sesji: {opcje.SciezkaSesji}")
                End If
                Await wykonawca.PobierzSesjeAsync(opcje, reporter, ct).ConfigureAwait(False)
                If czyInteraktywna AndAlso Not opcje.TrybCichy Then Console.WriteLine()
            Catch ex As OperationCanceledException
                Return KodWyjsciaCli.AnulowanoPrzezUzytkownika
            Catch ex As Exception
                If czyInteraktywna Then Console.WriteLine()
                Console.Error.WriteLine($"Błąd pobierania sesji: {ex.Message}")
                Return KodWyjsciaCli.BladPobierania
            End Try
        End If

        ' Etap 2: Opcjonalny eksport mapy po pobraniu
        If Not String.IsNullOrWhiteSpace(opcje.FormatEksportu) Then
            Try
                reporter.ResetujEtap()
                Dim plikWynikowy As String = Await wykonawca.EksportujSesjeAsync(opcje, reporter, ct).ConfigureAwait(False)
                If czyInteraktywna AndAlso Not opcje.TrybCichy Then Console.WriteLine()
                If Not opcje.TrybCichy Then
                    Console.WriteLine($"Eksport zakończony pomyślnie: {plikWynikowy}")
                End If
            Catch ex As OperationCanceledException
                Return KodWyjsciaCli.AnulowanoPrzezUzytkownika
            Catch ex As Exception
                If czyInteraktywna Then Console.WriteLine()
                Console.Error.WriteLine($"Błąd eksportu ({opcje.FormatEksportu}): {ex.Message}")
                Return KodWyjsciaCli.BladEksportu
            End Try
        End If

        Return KodWyjsciaCli.Sukces
    End Function

    ''' <summary>
    ''' Parsuje flagi wiersza poleceń.
    ''' </summary>
    Public Shared Function ParsujArgumenty(args As String(), ByRef wynik As OpcjeCli, ByRef blad As String) As Boolean
        wynik = New OpcjeCli()
        blad = String.Empty

        If args Is Nothing OrElse args.Length = 0 Then
            blad = "Brak argumentów."
            Return False
        End If

        Dim i As Integer = 0
        While i < args.Length
            Dim arg As String = args(i).Trim()
            Select Case arg.ToLowerInvariant()
                Case "--pomoc", "--help", "-h"
                    wynik.PokazPomoc = True
                    Return True

                Case "--sesja", "-s"
                    If i + 1 >= args.Length Then
                        blad = "Opcja --sesja wymaga podania ścieżki do pliku conf.txt lub katalogu sesji."
                        Return False
                    End If
                    i += 1
                    wynik.SciezkaSesji = args(i)

                Case "--eksport", "-e"
                    If i + 1 >= args.Length Then
                        blad = "Opcja --eksport wymaga podania formatu (kmz, jnx, mbtiles, sqlitedb, orux, geotiff, gpkg)."
                        Return False
                    End If
                    i += 1
                    Dim fmt As String = args(i).Trim().ToLowerInvariant()
                    If Not DozwoloneFormatyEksportu.Contains(fmt) Then
                        blad = $"Nieznany format eksportu: {args(i)}. Dozwolone: kmz, jnx, mbtiles, sqlitedb, orux, geotiff, gpkg."
                        Return False
                    End If
                    wynik.FormatEksportu = fmt

                Case "--wyjscie", "-o"
                    If i + 1 >= args.Length Then
                        blad = "Opcja --wyjscie wymaga podania ścieżki pliku docelowego."
                        Return False
                    End If
                    i += 1
                    wynik.SciezkaWyjsciowa = args(i)

                Case "--poligon", "-p"
                    If i + 1 >= args.Length Then
                        blad = "Opcja --poligon wymaga podania ścieżki do pliku KML lub GeoJSON."
                        Return False
                    End If
                    i += 1
                    wynik.SciezkaPoligonu = args(i)

                Case "--bez-pobierania"
                    wynik.TylkoEksport = True

                Case "--nadpisz"
                    wynik.NadpiszIstniejacy = True

                Case "--cicho", "-q"
                    wynik.TrybCichy = True

                Case Else
                    blad = $"Nieznany parametr: {arg}"
                    Return False
            End Select
            i += 1
        End While

        If String.IsNullOrWhiteSpace(wynik.SciezkaSesji) Then
            blad = "Wymagany jest parametr --sesja <ścieżka>."
            Return False
        End If

        Return True
    End Function

    Private Shared Sub WypiszPomoc()
        Console.WriteLine("MapoTero – tryb wiersza poleceń (CLI)")
        Console.WriteLine("Użycie:")
        Console.WriteLine("  MapoTero.exe --sesja <ścieżka_conf_lub_folder> [opcje]")
        Console.WriteLine()
        Console.WriteLine("Opcje:")
        Console.WriteLine("  --sesja, -s <ścieżka>       Ścieżka do pliku conf.txt lub folderu sesji (wymagane)")
        Console.WriteLine("  --eksport, -e <format>      Format eksportu: kmz, jnx, mbtiles, sqlitedb, orux, geotiff, gpkg")
        Console.WriteLine("  --wyjscie, -o <plik>        Docelowa ścieżka pliku wynikowego")
        Console.WriteLine("  --poligon, -p <plik>        Opcjonalny plik poligonu KML / GeoJSON")
        Console.WriteLine("  --bez-pobierania            Pomiń pobieranie segmentów, wykonaj wyłącznie eksport")
        Console.WriteLine("  --nadpisz                   Nadpisz istniejący plik wynikowy")
        Console.WriteLine("  --cicho, -q                 Ogranicz komunikaty postępu (zostawia tylko błędy)")
        Console.WriteLine("  --pomoc, -h                 Wyświetl tę pomoc")
    End Sub

End Class
