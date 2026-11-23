Imports System.IO
Imports Xunit

''' <summary>Audyt: czas przenoszenia plików do Kosza paczkami (Program.PrzeniesDoKosza). Tylko przy MAPOTERO_AUDYT=1 - zostawia pliki w Koszu.</summary>
<Trait("Kategoria", "Audyt")>
Public Class AudytKoszTesty
    <FaktAudytu>
    Public Sub PrzeniesDoKosza_300Plikow()
        Dim folder = Path.Combine(Path.GetTempPath(), "audyt", "kosz_test2")
        Directory.CreateDirectory(folder)
        Dim pliki As New List(Of String)
        For i = 1 To 300
            Dim p = Path.Combine(folder, "mapotero_audyt_kosz_" & i & ".txt")
            File.WriteAllText(p, "x")
            pliki.Add(p)
        Next
        Dim sw = Diagnostics.Stopwatch.StartNew()
        Dim bledy = Global.MapoTero.Program.PrzeniesDoKosza(pliki)
        sw.Stop()
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "audyt", "kosz.txt"), $"300 plików: {sw.ElapsedMilliseconds} ms, błędy: {bledy}, pozostało: {pliki.Where(Function(p) File.Exists(p)).Count()}")
        Assert.Equal(0, bledy)
        Assert.All(pliki, Sub(p) Assert.False(File.Exists(p)))
    End Sub
End Class
