Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero
Imports Xunit

Public Class CliTesty

    <Theory>
    <InlineData(New String() {"--sesja", "c:\test\conf.txt"}, True)>
    <InlineData(New String() {"-s", "c:\test\conf.txt"}, True)>
    <InlineData(New String() {"--pomoc"}, True)>
    <InlineData(New String() {"-h"}, True)>
    <InlineData(New String() {"--help"}, True)>
    <InlineData(New String() {"--inna-flaga"}, False)>
    <InlineData(New String() {}, False)>
    Public Sub CzyTrybCli_RozpoznajeFlagi(args As String(), oczekiwany As Boolean)
        Dim wynik = SilnikCli.CzyTrybCli(args)
        Assert.Equal(oczekiwany, wynik)
    End Sub

    <Fact>
    Public Sub ParsujArgumenty_KompletneParametry_UstawiaWlasciwosci()
        Dim args As String() = {
            "--sesja", "c:\dane\conf.txt",
            "--eksport", "kmz",
            "--wyjscie", "c:\out\mapa.kmz",
            "--poligon", "c:\dane\granica.kml",
            "--bez-pobierania",
            "--nadpisz",
            "--cicho"
        }

        Dim opcje As OpcjeCli = Nothing
        Dim blad As String = String.Empty
        Dim sukces = SilnikCli.ParsujArgumenty(args, opcje, blad)

        Assert.True(sukces)
        Assert.NotNull(opcje)
        Assert.Equal("c:\dane\conf.txt", opcje.SciezkaSesji)
        Assert.Equal("kmz", opcje.FormatEksportu)
        Assert.Equal("c:\out\mapa.kmz", opcje.SciezkaWyjsciowa)
        Assert.Equal("c:\dane\granica.kml", opcje.SciezkaPoligonu)
        Assert.True(opcje.TylkoEksport)
        Assert.True(opcje.NadpiszIstniejacy)
        Assert.True(opcje.TrybCichy)
    End Sub

    <Fact>
    Public Sub ParsujArgumenty_BrakSesji_ZwracaBlad()
        Dim args As String() = {"--eksport", "mbtiles"}
        Dim opcje As OpcjeCli = Nothing
        Dim blad As String = String.Empty

        Dim sukces = SilnikCli.ParsujArgumenty(args, opcje, blad)
        Assert.False(sukces)
        Assert.Contains("Wymagany jest parametr --sesja", blad)
    End Sub

    <Fact>
    Public Sub ParsujArgumenty_NieznanaFlaga_ZwracaBlad()
        Dim args As String() = {"--sesja", "c:\conf.txt", "--nieznany"}
        Dim opcje As OpcjeCli = Nothing
        Dim blad As String = String.Empty

        Dim sukces = SilnikCli.ParsujArgumenty(args, opcje, blad)
        Assert.False(sukces)
        Assert.Contains("Nieznany parametr", blad)
    End Sub

    <Fact>
    Public Sub ParsujArgumenty_FlagaPomocy_UstawiaPokazPomoc()
        Dim args As String() = {"--help"}
        Dim opcje As OpcjeCli = Nothing
        Dim blad As String = String.Empty

        Dim sukces = SilnikCli.ParsujArgumenty(args, opcje, blad)
        Assert.True(sukces)
        Assert.True(opcje.PokazPomoc)
    End Sub

    <Fact>
    Public Sub PostepCli_LiczProcent_PoprawneWartosci()
        Dim p1 As New PostepCli With {.Wykonano = 50, .Razem = 100}
        Assert.Equal(50.0, p1.Procent)

        Dim p2 As New PostepCli With {.Wykonano = 0, .Razem = 0}
        Assert.Equal(0.0, p2.Procent)

        Dim p3 As New PostepCli With {.Wykonano = 150, .Razem = 100}
        Assert.Equal(100.0, p3.Procent)
    End Sub

    <Fact>
    Public Sub Uruchom_FlagaPomocy_ZwracaSukces()
        Dim wykonawca As New MockWykonawcaSesji()
        Dim wynik = SilnikCli.Uruchom(New String() {"--pomoc"}, wykonawca)
        Assert.Equal(KodWyjsciaCli.Sukces, wynik)
    End Sub

    <Fact>
    Public Sub Uruchom_NieistniejacyPlikSesji_ZwracaBrakPlikuSesji()
        Dim wykonawca As New MockWykonawcaSesji()
        Dim wynik = SilnikCli.Uruchom(New String() {"--sesja", "c:\nie_ma_takiego_pliku_xyz_123.txt"}, wykonawca)
        Assert.Equal(KodWyjsciaCli.BrakPlikuSesji, wynik)
    End Sub

    <Fact>
    Public Sub Uruchom_PrawidlowaSesja_WywolujePobieranieIEksport()
        Dim tempFile As String = Path.Combine(Path.GetTempPath(), "sesja_" & Guid.NewGuid().ToString("N") & ".txt")
        Try
            File.WriteAllText(tempFile, "test")
            Dim wykonawca As New MockWykonawcaSesji()
            Dim args As String() = {"--sesja", tempFile, "--eksport", "geotiff", "--cicho"}
            Dim wynik = SilnikCli.Uruchom(args, wykonawca)

            Assert.Equal(KodWyjsciaCli.Sukces, wynik)
            Assert.True(wykonawca.CzyPobrano)
            Assert.True(wykonawca.CzyWyeksportowano)
        Finally
            If File.Exists(tempFile) Then File.Delete(tempFile)
        End Try
    End Sub

    Private Class MockWykonawcaSesji
        Implements IWykonawcaSesjiCli

        Public Property CzyPobrano As Boolean = False
        Public Property CzyWyeksportowano As Boolean = False

        Public Function PobierzSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task Implements IWykonawcaSesjiCli.PobierzSesjeAsync
            CzyPobrano = True
            postep.Report(New PostepCli With {.Etap = "Pobieranie", .Wykonano = 1, .Razem = 1, .Komunikat = "OK"})
            Return Task.CompletedTask
        End Function

        Public Function EksportujSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task(Of String) Implements IWykonawcaSesjiCli.EksportujSesjeAsync
            CzyWyeksportowano = True
            postep.Report(New PostepCli With {.Etap = "Eksport", .Wykonano = 1, .Razem = 1, .Komunikat = "OK"})
            Return Task.FromResult("wynik.tif")
        End Function
    End Class

End Class
