'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>Testy poprawek z drugiego przebiegu audytu (etap 2, znaleziska E2-xx w docs-ai/audyt/rejestr.md).</summary>
Public Class AudytEtap2Testy

    Private Shared Sub WWatkuSta(akcja As Action)
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    akcja()
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_E2_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(f)
        Return f
    End Function

    Private Shared Sub UsunFolder(f As String)
        Try
            Directory.Delete(f, True)
        Catch
        End Try
    End Sub

    Private Shared Function Pole(Of T)(obiekt As Object, nazwa As String) As T
        Dim fi = obiekt.GetType().GetField(nazwa, BindingFlags.Instance Or BindingFlags.NonPublic)
        Assert.NotNull(fi)
        Return CType(fi.GetValue(obiekt), T)
    End Function

    Private Shared Sub UstawPole(obiekt As Object, nazwa As String, wartosc As Object)
        Dim fi = obiekt.GetType().GetField(nazwa, BindingFlags.Instance Or BindingFlags.NonPublic)
        Assert.NotNull(fi)
        fi.SetValue(obiekt, wartosc)
    End Sub

    Private Shared Function Kontrolka(Of T As Control)(f As Control, nazwa As String) As T
        Return DirectCast(f.Controls.Find(nazwa, True)(0), T)
    End Function

    Private Shared Sub ZapiszSegment(plik As String, bok As Integer)
        Using b As New Bitmap(bok, bok)
            Using g = Graphics.FromImage(b)
                g.Clear(Color.SteelBlue)
            End Using
            b.Save(plik, ImageFormat.Png)
        End Using
    End Sub

    Private Shared Function ZadanieScalania2x2(folder As String) As ZadanieScalania
        Dim bok = 50
        For w = 1 To 2
            For k = 1 To 2
                ZapiszSegment(folder & "_0" & w & "_0" & k & ".png", bok)
            Next
        Next
        Return New ZadanieScalania With {
            .Folder = folder, .Prefiks = "_", .Numeracja = StylNumeracji.WierszKolumna, .RozszerzenieSegmentow = "png",
            .Siatka = New Siatka(New Zasieg(500000, 600000, 500100, 600100), 1, bok), .Uklad = UkladWspolrzednych.PL1992,
            .Format = FormatArkusza.Png, .NazwaArkusza = "arkusz"}
    End Function

    ''' <summary>E2-01: przerwane ponowne scalanie nie usuwa poprzedniego arkusza i nie zostawia pliku tymczasowego.</summary>
    <Fact>
    Public Sub Scalanie_PrzerwanieNieUsuwaPoprzedniegoArkusza()
        Dim folder = NowyFolder()
        Try
            Dim z = ZadanieScalania2x2(folder)
            Dim pierwszy = ScalanieSegmentow.Scal(z, Nothing, CancellationToken.None)
            Dim zawartosc = File.ReadAllBytes(pierwszy.Plik)

            Using cts As New CancellationTokenSource()
                cts.Cancel()
                Assert.ThrowsAny(Of OperationCanceledException)(Sub() ScalanieSegmentow.Scal(z, Nothing, cts.Token))
            End Using

            Assert.True(File.Exists(pierwszy.Plik), "przerwanie usunęło poprzedni arkusz")
            Assert.Equal(zawartosc, File.ReadAllBytes(pierwszy.Plik))
            Assert.Empty(Directory.GetFiles(folder, "*.tmp"))

            'udane ponowne scalanie zastępuje arkusz i też nie zostawia pliku tymczasowego
            Dim drugi = ScalanieSegmentow.Scal(z, Nothing, CancellationToken.None)
            Assert.True(File.Exists(drugi.Plik))
            Assert.Empty(Directory.GetFiles(folder, "*.tmp"))
        Finally
            UsunFolder(folder)
        End Try
    End Sub

    ''' <summary>Odpowiedź serwera bez sieci - dokument WMS Capabilities.</summary>
    Private Class SerwerTestowy
        Inherits HttpMessageHandler
        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Threading.Tasks.Task(Of HttpResponseMessage)
            cancellationToken.ThrowIfCancellationRequested()
            Dim xml = "<?xml version=""1.0""?><WMS_Capabilities version=""1.3.0""><Service><Title>T</Title></Service><Capability><Layer><Name>a</Name></Layer></Capability></WMS_Capabilities>"
            Return Threading.Tasks.Task.FromResult(New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent(xml, Encoding.UTF8, "text/xml")})
        End Function
    End Class

    ''' <summary>E2-07: przerwane połączenie kreatora kończy się wyjątkiem przerwania, a nie wynikiem „błąd serwera”.</summary>
    <Fact>
    Public Async Function KlientCapabilities_PrzerwanieNieJestBledemSerwera() As Threading.Tasks.Task
        Using cts As New CancellationTokenSource()
            cts.Cancel()
            Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                Function() KlientCapabilities.PobierzAsync("https://serwer.example/wms", token:=cts.Token, handler:=New SerwerTestowy()))
        End Using
        'bez przerwania ten sam serwer działa
        Dim wynik = Await KlientCapabilities.PobierzAsync("https://serwer.example/wms", handler:=New SerwerTestowy())
        Assert.True(wynik.Sukces, wynik.KomunikatBledu)
    End Function

    ''' <summary>
    ''' Scalanie segmentów otwiera okno Eksport mapy z formatem Arkusz i folderem wskazanym przez okno CAD (dawne okno
    ''' scalania Form3 usunięte); folder bez conf.txt nie pozwala eksportować.
    ''' </summary>
    <Fact>
    Public Sub OknoEksportu_ScalanieOtwieraArkuszWeWskazanymFolderze()
        Dim zConf = NowyFolder(), bezConf = NowyFolder()
        Try
            Dim c As New KonfiguracjaSesji With {.Folder = zConf, .XDol = "500000", .YLewy = "600000", .XGora = "500100", .YPrawy = "600100",
                                                  .BokSegmentuPx = "50", .RozmiarPiksela = "1", .Format = "png", .Prefiks = "_", .UkladEpsg = 2180}
            c.Zapisz(zConf & "conf.txt", Encoding.UTF8)
            WWatkuSta(
                Sub()
                    Using f As New MapoTero.FormEksport() With {.FolderPoczatkowy = zConf, .ArkuszDomyslnie = True}
                        f.Show()
                        Application.DoEvents()
                        Assert.True(Kontrolka(Of RadioButton)(f, "rbGeoTiff").Checked)
                        Assert.True(Kontrolka(Of ComboBox)(f, "cmbFormatArkusza").Enabled)
                        Assert.True(Kontrolka(Of CheckBox)(f, "chkArkuszWorldFile").Enabled)
                        Assert.Contains(zConf.TrimEnd("\"c), Kontrolka(Of TextBox)(f, "txtFolder").Text)
                        Assert.True(Kontrolka(Of Button)(f, "btnEksportuj").Enabled)
                        Assert.Equal("", f.PlikArkusza)
                    End Using
                    Using f As New MapoTero.FormEksport() With {.FolderPoczatkowy = bezConf, .ArkuszDomyslnie = True}
                        f.Show()
                        Application.DoEvents()
                        Assert.False(Kontrolka(Of Button)(f, "btnEksportuj").Enabled)
                    End Using
                End Sub)
        Finally
            UsunFolder(zConf)
            UsunFolder(bezConf)
        End Try
    End Sub

    ''' <summary>E2-03: wskazanie nowej trasy usuwa profil wysokości poprzedniej (zapis GPX nie łączy tras).
    ''' Profil przeniesiony z okna korytarza do osobnego narzędzia FormProfilNmt.</summary>
    <Fact>
    Public Sub OknoProfiluNmt_NowaTrasaUsuwaProfilPoprzedniej()
        Dim folder = NowyFolder()
        Try
            Dim plik = folder & "pusta.gpx"
            File.WriteAllText(plik, "<?xml version=""1.0""?><gpx version=""1.1"" xmlns=""http://www.topografix.com/GPX/1/1""></gpx>")
            WWatkuSta(
                Sub()
                    Using f As New MapoTero.FormProfilNmt()
                        f.Show()
                        Application.DoEvents()
                        Dim linie = Pole(Of List(Of IList(Of Double)))(f, "_linieWysokosci")
                        linie.Add(New List(Of Double) From {100.0, 101.0})
                        UstawPole(f, "_statystyki", New StatystykiProfilu())
                        Dim btnZapiszGpx = Kontrolka(Of Button)(f, "btnZapiszGpx")
                        btnZapiszGpx.Enabled = True

                        f.WczytajPlikTrasy(plik)

                        Assert.Empty(linie)
                        Assert.Null(Pole(Of Object)(f, "_statystyki"))
                        Assert.False(btnZapiszGpx.Enabled)
                        'pusty plik - brak trasy, więc nie ma czego pobierać
                        Assert.False(Kontrolka(Of Button)(f, "btnNmt").Enabled)
                    End Using
                End Sub)
        Finally
            UsunFolder(folder)
        End Try
    End Sub

    ''' <summary>E2-04: zamknięcie okna importu OziExplorer w trakcie konwersji przerywa ją; okno zamyka się po jej zakończeniu.</summary>
    <Fact>
    Public Sub OknoOzi_ZamkniecieWTrakcieKonwersjiPrzerywaJa()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.FormImportOzi()
                    f.Show()
                    Application.DoEvents()
                    Using cts As New CancellationTokenSource()
                        UstawPole(f, "_anulowanie", cts)
                        f.Close()
                        Assert.True(cts.IsCancellationRequested, "zamknięcie nie przerwało konwersji")
                        Assert.True(f.Visible, "okno zamknęło się przed zakończeniem konwersji")
                        Assert.True(Pole(Of Boolean)(f, "_zamknijPoZakonczeniu"))
                    End Using
                    UstawPole(f, "_anulowanie", Nothing)
                    f.Close()
                    Assert.False(f.Visible)
                End Using
            End Sub)
    End Sub

    ''' <summary>E2-05: puste lub względne pole folderu docelowego korekty obrazów daje komunikat zamiast wyjątku.</summary>
    <Fact>
    Public Sub KorektaFolderu_SprawdzenieFolderow()
        Dim zrodlo = NowyFolder()
        Try
            Assert.Contains("Wskaż folder docelowy", MapoTero.FormKorektaFolderu.BladFolderow(zrodlo, ""))
            Assert.Contains("Wskaż folder docelowy", MapoTero.FormKorektaFolderu.BladFolderow(zrodlo, "   "))
            Assert.Contains("pełną ścieżką", MapoTero.FormKorektaFolderu.BladFolderow(zrodlo, "korekta"))
            Assert.Contains("inny niż źródłowy", MapoTero.FormKorektaFolderu.BladFolderow(zrodlo, zrodlo.TrimEnd("\"c)))
            Assert.Contains("źródłowy nie istnieje", MapoTero.FormKorektaFolderu.BladFolderow("", zrodlo & "korekta"))
            Assert.Contains("źródłowy nie istnieje", MapoTero.FormKorektaFolderu.BladFolderow("abc", zrodlo & "korekta"))
            Assert.Equal("", MapoTero.FormKorektaFolderu.BladFolderow(zrodlo, zrodlo & "korekta"))
        Finally
            UsunFolder(zrodlo)
        End Try
    End Sub

    ''' <summary>E2-08: rozmiar minimalny okna narzędzia brany jest z pierwszego otwarcia, a nie z rozmiaru powiększonego okna.</summary>
    <Fact>
    Public Sub RozmiarMinimalny_TylkoPrzyPierwszymOtwarciu()
        WWatkuSta(
            Sub()
                Using f As New Form() With {.Size = New Size(400, 300)}
                    MapoTero.PomocProgramu.ZamykanieKlawiszemEsc(f, rozmiarMinimalny:=True)
                    Assert.Equal(New Size(400, 300), f.MinimumSize)
                    f.Size = New Size(700, 500)
                    MapoTero.PomocProgramu.ZamykanieKlawiszemEsc(f, rozmiarMinimalny:=True)
                    Assert.Equal(New Size(400, 300), f.MinimumSize)
                End Using
            End Sub)
    End Sub

    ''' <summary>E2-10: uszkodzony conf.txt wskazanej sesji nie zmienia folderu bieżącej sesji.</summary>
    <Fact>
    Public Sub WczytanieSesji_UszkodzonyConfNieZmieniaFolderuSesji()
        Dim uszkodzony = NowyFolder()
        Try
            File.WriteAllBytes(uszkodzony & "conf.txt", Array.Empty(Of Byte)())
            WWatkuSta(
                Sub()
                    Using f As New MapoTero.Form1()
                        f.Show()
                        Application.DoEvents()
                        Dim przed As String = f.FolderSesji
                        f.WczytajConfZFolderu(uszkodzony, wybranyPrzezUzytkownika:=True)
                        Assert.Equal(przed, f.FolderSesji)
                    End Using
                End Sub)
        Finally
            UsunFolder(uszkodzony)
        End Try
    End Sub

End Class
