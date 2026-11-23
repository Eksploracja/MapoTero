'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports MapoTero.Core
Imports Xunit

''' <summary>Testy błędów znalezionych w przeglądzie spójności programu (2026-11-17).</summary>
Public Class PrzegladSpojnosciTesty

    ''' <summary>
    ''' Nowa sesja wczytuje z lastsettings.txt tylko parametry sesji. (Błąd: wczytywała cały plik, a program zapisuje go
    ''' dopiero przy zamknięciu, więc ustawienia zmienione od uruchomienia - np. korekta obrazu - wracały do starych.)
    ''' </summary>
    <Fact>
    Public Sub NowaSesja_NieCofaUstawienProgramu()
        Dim plik = Path.GetTempFileName()
        Try
            Dim zapisane As New MapoTero.UstawieniaProgramu()
            zapisane.Prefiks = "plik_"
            zapisane.Format = "png"
            zapisane.Przezroczystosc = True
            zapisane.ScalanieKml = False
            zapisane.ZapamietajKorekte(OpcjeKorekcjiMip.Odtworz(False, ProfilKorektyRastra.Brak, 1, 1, 1, 0))
            zapisane.Zapisz(plik)

            Dim biezace As New MapoTero.UstawieniaProgramu()
            biezace.Prefiks = "inny_"
            biezace.Format = "jpeg"
            biezace.ScalanieKml = True
            biezace.ZapamietajKorekte(OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.SkanCadCzarnoBialy, 1, 1, 1, 0))
            biezace.WczytajParametryNowejSesji(plik)

            Assert.Equal("plik_", biezace.Prefiks)
            Assert.Equal("png", biezace.Format)
            Assert.True(biezace.Przezroczystosc)
            Assert.True(biezace.ScalanieKml)
            Assert.Equal(ProfilKorektyRastra.SkanCadCzarnoBialy, biezace.OpcjeKorekty().Profil)
            Assert.True(biezace.OpcjeKorekty().Wlaczona)
        Finally
            File.Delete(plik)
        End Try
    End Sub

    Private Const GpxZCzasami As String =
        "<?xml version=""1.0"" encoding=""UTF-8""?>" &
        "<gpx version=""1.1"" creator=""Zegarek"" xmlns=""http://www.topografix.com/GPX/1/1"">" &
        "<wpt lat=""50.1"" lon=""19.1""><name>Schronisko</name></wpt>" &
        "<trk><name>Wycieczka</name><trkseg>" &
        "<trkpt lat=""50.0"" lon=""19.0""><time>2026-11-01T08:00:00Z</time></trkpt>" &
        "<trkpt lat=""abc"" lon=""19.0""><time>2026-11-01T08:00:30Z</time></trkpt>" &
        "<trkpt lat=""50.001"" lon=""19.001""><ele>999.0</ele><time>2026-11-01T08:01:00Z</time></trkpt>" &
        "<trkpt lat=""50.002"" lon=""19.002""><ele>321.0</ele><time>2026-11-01T08:02:00Z</time></trkpt>" &
        "</trkseg></trk></gpx>"

    ''' <summary>
    ''' Wzbogacony GPX zachowuje czasy punktów, punkty wpt i nazwy, a wysokości trafiają do właściwych punktów (punkt
    ''' z błędnym lat pominięty jak przy odczycie trasy). (Błąd: zapis tworzył nowy plik z samymi współrzędnymi.)
    ''' </summary>
    <Fact>
    Public Sub WzbogaconyGpx_ZachowujeCzasyIPunkty()
        Dim k = Korytarz.ZTekstu(GpxZCzasami)
        Assert.NotNull(k.DokumentGpx)
        Assert.Equal(3, k.LiczbaPunktow)
        Dim wynik = Korytarz.WzbogacGpx(k.DokumentGpx, {210.4, 211.5, Double.NaN})
        Dim ns As XNamespace = "http://www.topografix.com/GPX/1/1"
        Dim pkt = wynik.Descendants(ns + "trkpt").ToList()
        Assert.Equal(4, pkt.Count)
        Assert.Equal("210.4", pkt(0).Element(ns + "ele").Value)
        Assert.Same(pkt(0).Element(ns + "ele"), pkt(0).Elements().First()) 'ele przed time
        Assert.Null(pkt(1).Element(ns + "ele"))
        Assert.Equal("211.5", pkt(2).Element(ns + "ele").Value)
        Assert.Equal("321.0", pkt(3).Element(ns + "ele").Value) 'brak danych NMT - dawna wysokość zostaje
        Assert.Equal("2026-11-01T08:01:00Z", pkt(2).Element(ns + "time").Value)
        Assert.Equal("Schronisko", wynik.Descendants(ns + "wpt").Single().Element(ns + "name").Value)
        'oryginał bez zmian
        Assert.Equal("999.0", k.DokumentGpx.Descendants(ns + "trkpt").ElementAt(2).Element(ns + "ele").Value)

        Dim plik = Path.GetTempFileName()
        Try
            Korytarz.ZapiszGpx(wynik, plik)
            Dim ponownie = Korytarz.WczytajTrase(plik)
            Assert.Equal(3, ponownie.LiczbaPunktow)
            Assert.Contains("<time>2026-11-01T08:00:00Z</time>", File.ReadAllText(plik))
        Finally
            File.Delete(plik)
        End Try
    End Sub

    ''' <summary>Trasa z KML nie ma dokumentu GPX - zapis wzbogaconego śladu korzysta wtedy z dawnej metody.</summary>
    <Fact>
    Public Sub TrasaKml_BezDokumentuGpx()
        Dim k = Korytarz.ZTekstu("<kml xmlns=""http://www.opengis.net/kml/2.2""><Placemark><LineString><coordinates>19,50 19.1,50.1</coordinates></LineString></Placemark></kml>")
        Assert.Null(k.DokumentGpx)
    End Sub

    ''' <summary>
    ''' Korekta folderu pomija scalony arkusz i dobiera liczbę wątków do największego obrazu. (Błąd: arkusz był
    ''' poprawiany razem z segmentami, a liczbę wątków liczono z pierwszego pliku.)
    ''' </summary>
    <Fact>
    Public Sub KorektaFolderu_PomijaArkuszIWatkiWgNajwiekszego()
        Dim folder = Path.Combine(Path.GetTempPath(), "mapotero_spojnosc_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            For Each n In {"a_01_01.png", "a_01_02.png"}
                Using b As New Bitmap(8, 8)
                    b.Save(Path.Combine(folder, n), ImageFormat.Png)
                End Using
            Next
            Using b As New Bitmap(12000, 12000, PixelFormat.Format24bppRgb)
                b.Save(Path.Combine(folder, "_scalone_segmenty_2x1.png"), ImageFormat.Png)
            End Using
            Dim pominiete As Integer
            Dim pliki = MapoTero.FormKorektaFolderu.PlikiDoKorekty(folder, pominiete)
            Assert.Equal(2, pliki.Count)
            Assert.Equal(1, pominiete)

            'duży obraz na końcu listy - liczba wątków jak dla niego (12000 x 12000 x 20 B > 1 GB -> 1 wątek)
            Dim wszystkie = pliki.Concat({Path.Combine(folder, "_scalone_segmenty_2x1.png")}).ToList()
            Assert.Equal(1, MapoTero.FormKorektaFolderu.LiczbaWatkow(wszystkie))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    ''' <summary>Czas kompilacji MBTiles powyżej godziny zawiera godziny. (Błąd: format mm:ss pokazywał 1 h 05 min jako 05:00.)</summary>
    <Fact>
    Public Sub CzasKompilacji_ZGodzinami()
        Assert.Equal("04:07", MapoTero.FormZaawansowanyMbtiles.CzasTrwania(New TimeSpan(0, 4, 7)))
        Assert.Equal("1:05:00", MapoTero.FormZaawansowanyMbtiles.CzasTrwania(New TimeSpan(1, 5, 0)))
        Assert.Equal("26:00:09", MapoTero.FormZaawansowanyMbtiles.CzasTrwania(New TimeSpan(1, 2, 0, 9)))
    End Sub

    ''' <summary>
    ''' Klawisz F1 otwiera pomoc w oknach z przyciskiem Pomoc - okno przechwytuje klawisze (KeyPreview). (Błąd: pomoc
    ''' online opisywała F1 w oknach Scalanie i Ustawienia, a działał tylko w wielowarstwowym MBTiles.)
    ''' </summary>
    <Fact>
    Public Sub F1_WOknachZPrzyciskiemPomocy()
        Dim bledy As New List(Of String)
        Dim t As New Thread(
            Sub()
                For Each f As Form In {CType(New MapoTero.FormEksport(), Form), New MapoTero.Form2(), New MapoTero.FormKorektaFolderu()}
                    Try
                        f.StartPosition = FormStartPosition.Manual
                        f.Show()
                        Application.DoEvents()
                        If Not f.KeyPreview Then bledy.Add(f.Name & ": KeyPreview wyłączone")
                        f.Close()
                    Catch ex As Exception
                        bledy.Add(f.Name & ": " & ex.Message)
                    Finally
                        f.Dispose()
                    End Try
                Next
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
    End Sub

End Class
