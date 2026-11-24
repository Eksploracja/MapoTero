'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks
Imports MapoTero.Core
Imports Xunit

Public Class KorekcjaMipTesty

    <Fact>
    Public Sub KorekcjaMip_Wylaczona_NieModyfikujePikseli()
        Dim piksele() As Integer = {&HFF102030, &HFF405060}
        Dim kopia() As Integer = CType(piksele.Clone(), Integer())

        Dim opcje As New OpcjeKorekcjiMip With {.Wlaczona = False}
        KorekcjaMip.Zastosuj(piksele, 2, 1, opcje)

        Assert.Equal(kopia(0), piksele(0))
        Assert.Equal(kopia(1), piksele(1))
    End Sub

    <Fact>
    Public Sub KorekcjaMip_RozjasniaCiemneOdcienie_Gamma()
        ' Ciemny piksel lasu: R=40, G=60, B=30
        Dim c = &HFF000000 Or (40 << 16) Or (60 << 8) Or 30
        Dim piksele() As Integer = {c}

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 0.75,
            .Kontrast = 1.0,
            .Nasycenie = 1.0,
            .Wyostrzanie = 0.0
        }
        KorekcjaMip.Zastosuj(piksele, 1, 1, opcje)

        Dim outG = (piksele(0) >> 8) And &HFF
        ' Dla gamma 0.75 wartość 60/255=0.235 po podniesieniu do 0.75 wynosi ok. 0.337 (~86)
        Assert.True(outG > 60, "Gamma 0.75 powinna rozjaśnić ciemne partie zieleni")
    End Sub

    <Fact>
    Public Sub KorekcjaMip_PodbijaNasycenie()
        ' Przygaszony błękit rzeki: R=50, G=80, B=120
        Dim c = &HFF000000 Or (50 << 16) Or (80 << 8) Or 120
        Dim piksele() As Integer = {c}

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 1.0,
            .Kontrast = 1.0,
            .Nasycenie = 1.5,
            .Wyostrzanie = 0.0
        }
        KorekcjaMip.Zastosuj(piksele, 1, 1, opcje)

        Dim outB = piksele(0) And &HFF
        Assert.True(outB > 120, "Zwiększenie nasycenia powinno podbić wiodący kanał niebieski")
    End Sub

    <Fact>
    Public Sub KorekcjaMip_WyostrzaKrawedzie_Laplacian()
        ' Obraz 3x3 z ciemną kropką w środku: tło jasnoszare, środek ciemniejszy
        Dim jasny = &HFF808080
        Dim ciemny = &HFF404040
        Dim piksele(8) As Integer
        For i = 0 To 8
            piksele(i) = jasny
        Next
        piksele(4) = ciemny ' środek

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 1.0,
            .Kontrast = 1.0,
            .Nasycenie = 1.0,
            .Wyostrzanie = 0.8
        }
        KorekcjaMip.Zastosuj(piksele, 3, 3, opcje)

        Dim centerVal = (piksele(4) >> 16) And &HFF
        ' Wyostrzanie powinno pogłębić kontrast krawędzi (środek ciemniejszy niż początkowe 64)
        Assert.True(centerVal < 64, "Wyostrzanie krawędzi powinno pogłębić różnicę na krawędzi")
    End Sub

    <Fact>
    Public Async Function KorekcjaMip_RealnyObrazZInternetu_GenerujePorownanieAsync() As Task
        ' Pobieramy autentyczny kafelek topograficzny OpenTopoMap z internetu
        Dim url = "https://a.tile.opentopomap.org/14/9070/5688.png"
        Dim dane() As Byte = Nothing

        Using http As New HttpClient()
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 MapoTeroTest/1.0")
            Try
                dane = Await http.GetByteArrayAsync(url).ConfigureAwait(False)
            Catch
            End Try
        End Using

        Dim origBmp As Bitmap
        If dane IsNot Nothing AndAlso dane.Length > 0 Then
            Using ms As New MemoryStream(dane)
                origBmp = New Bitmap(Image.FromStream(ms))
            End Using
        Else
            ' Fallback syntetyczny
            origBmp = New Bitmap(256, 256, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(origBmp)
                g.Clear(Color.FromArgb(70, 105, 75))
            End Using
        End If

        Dim szer = origBmp.Width
        Dim wys = origBmp.Height

        ' Pobieramy piksele do tablicy ARGB
        Dim piksele(szer * wys - 1) As Integer
        Dim bd = origBmp.LockBits(New Rectangle(0, 0, szer, wys), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wys - 1
                System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(bd.Scan0, y * bd.Stride), piksele, y * szer, szer)
            Next
        Finally
            origBmp.UnlockBits(bd)
        End Try

        ' Kopia pikseli do przetworzenia
        Dim pikseleMip(piksele.Length - 1) As Integer
        Array.Copy(piksele, pikseleMip, piksele.Length)

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 0.75,
            .Kontrast = 1.25,
            .Nasycenie = 1.35,
            .Wyostrzanie = 0.6
        }
        KorekcjaMip.Zastosuj(pikseleMip, szer, wys, opcje)

        ' Tworzymy zmodyfikowaną bitmapę
        Dim mipBmp As New Bitmap(szer, wys, PixelFormat.Format32bppArgb)
        Dim bdMip = mipBmp.LockBits(New Rectangle(0, 0, szer, wys), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wys - 1
                System.Runtime.InteropServices.Marshal.Copy(pikseleMip, y * szer, IntPtr.Add(bdMip.Scan0, y * bdMip.Stride), szer)
            Next
        Finally
            mipBmp.UnlockBits(bdMip)
        End Try

        ' Tworzymy planszę porównawczą side-by-side
        Dim padding = 20
        Dim naglowekWys = 40
        Dim wynikSzer = szer * 2 + padding * 3
        Dim wynikWys = wys + naglowekWys + padding * 2

        Using kompozycja As New Bitmap(wynikSzer, wynikWys, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(kompozycja)
                g.Clear(Color.FromArgb(245, 247, 250))

                Using fTytul As New Font("Segoe UI", 11.0!, FontStyle.Bold)
                Using fPodpis As New Font("Segoe UI", 9.0!, FontStyle.Regular)
                Using bTytul As New SolidBrush(Color.FromArgb(30, 41, 59))
                Using bRamka As New Pen(Color.FromArgb(203, 213, 225), 1.0!)

                    ' Etykiety
                    g.DrawString("Oryginalny raster WMS/XYZ (surowy)", fTytul, bTytul, padding, 12)
                    g.DrawString("Po optymalizacji MIP (Garmin / pełne słońce)", fTytul, bTytul, szer + padding * 2, 12)

                    ' Rysowanie obrazów
                    g.DrawImage(origBmp, padding, naglowekWys, szer, wys)
                    g.DrawRectangle(bRamka, padding, naglowekWys, szer, wys)

                    g.DrawImage(mipBmp, szer + padding * 2, naglowekWys, szer, wys)
                    g.DrawRectangle(bRamka, szer + padding * 2, naglowekWys, szer, wys)

                    Dim parametry = $"Parametry MIP: Gamma={opcje.Gamma:F2}, Kontrast={opcje.Kontrast:F2}, Nasycenie={opcje.Nasycenie:F2}, Wyostrzanie={opcje.Wyostrzanie:F2}"
                    g.DrawString(parametry, fPodpis, Brushes.Gray, padding, wys + naglowekWys + 5)
                End Using
                End Using
                End Using
                End Using
            End Using

            ' Zapis do katalogu artefaktów sesji
            Dim folderArtefaktow = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf"
            If Directory.Exists(folderArtefaktow) Then
                Dim sciezkaWyniku = Path.Combine(folderArtefaktow, "korekcja_mip_porownanie.png")
                kompozycja.Save(sciezkaWyniku, ImageFormat.Png)
            End If
        End Using

        origBmp.Dispose()
        mipBmp.Dispose()

        ' Weryfikacja że piksele uległy zmianie
        Dim roznice = 0
        For i = 0 To piksele.Length - 1
            If piksele(i) <> pikseleMip(i) Then roznice += 1
        Next
        Assert.True(roznice > piksele.Length * 0.5, "Większość pikseli powinna ulec zoptymalizowaniu")
    End Function

    <Fact>
    Public Sub ProfileKorekty_UstawiajaPrawidloweWartosci()
        Dim opcje As New OpcjeKorekcjiMip()
        Assert.Equal(ProfilKorektyRastra.Brak, opcje.Profil)
        Assert.False(opcje.Wlaczona)

        opcje.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
        Assert.True(opcje.Wlaczona)
        Assert.Equal(0.75, opcje.Gamma)
        Assert.Equal(1.25, opcje.Kontrast)
        Assert.Equal(1.35, opcje.Nasycenie)
        Assert.Equal(0.6, opcje.Wyostrzanie)

        opcje.UstawWgProfilu(ProfilKorektyRastra.WyostrzenieKrawedzi)
        Assert.True(opcje.Wlaczona)
        Assert.Equal(1.0, opcje.Gamma)
        Assert.Equal(1.1, opcje.Kontrast)
        Assert.Equal(0.5, opcje.Wyostrzanie)

        opcje.UstawWgProfilu(ProfilKorektyRastra.StareMapyKontrast)
        Assert.True(opcje.Wlaczona)
        Assert.Equal(1.4, opcje.Kontrast)
        Assert.Equal(0.75, opcje.Wyostrzanie)

        opcje.UstawWgProfilu(ProfilKorektyRastra.Brak)
        Assert.False(opcje.Wlaczona)
    End Sub

    <Fact>
    Public Sub KorekcjaMip_ZastosujDoBuforaRgb_ModyfikujePikseleIChroniAlfa()
        Dim bufor() As Byte = {
            40, 60, 30, 255,   ' Ciemna zieleń, nieprzezroczysta
            40, 60, 30, 0      ' W pełni przezroczysta - nie powinna ulec zmianie
        }

        Dim opcje As New OpcjeKorekcjiMip()
        opcje.UstawWgProfilu(ProfilKorektyRastra.GarminMip)

        KorekcjaMip.ZastosujDoBuforaRgb(bufor, 2, 1, 4, opcje)

        ' Piksel 1 (nieprzezroczysty) powinien zostać rozjaśniony przez Gamma i Kontrast
        Assert.True(bufor(1) > 60, "Zielony kanał piksela nieprzezroczystego powinien zostać rozjaśniony")
        Assert.Equal(CByte(255), bufor(3))

        ' Piksel 2 (przezroczysty) powinien pozostać bez zmian
        Assert.Equal(CByte(40), bufor(4))
        Assert.Equal(CByte(60), bufor(5))
        Assert.Equal(CByte(30), bufor(6))
        Assert.Equal(CByte(0), bufor(7))
    End Sub

    <Fact>
    Public Sub KorekcjaMip_ZastosujDoBuforaRgb_WyostrzaKrawedzie()
        ' Siatka 3x3 RGB, w środku ciemny piksel (40), dookoła jaśniejszy (120)
        Dim bufor(3 * 3 * 3 - 1) As Byte
        For i = 0 To 8
            Dim idx = i * 3
            bufor(idx) = 120
            bufor(idx + 1) = 120
            bufor(idx + 2) = 120
        Next
        ' Środek (indeks 4) ciemniejszy
        bufor(4 * 3) = 40
        bufor(4 * 3 + 1) = 40
        bufor(4 * 3 + 2) = 40

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 1.0,
            .Kontrast = 1.0,
            .Nasycenie = 1.0,
            .Wyostrzanie = 0.8
        }

        KorekcjaMip.ZastosujDoBuforaRgb(bufor, 3, 3, 3, opcje)

        ' Środkowy piksel pod wpływem wyostrzania krawędzi powinien pogłębić kontrast (stać się ciemniejszy niż 40)
        Assert.True(bufor(4 * 3) < 40, "Wyostrzanie w buforze RGB powinno pogłębić kontrast środkowego piksela krawędzi")
    End Sub

    <Fact>
    Public Sub ProfileKorekty_Cad_UstawiajaPrawidloweWartosci()
        Dim opcjeCad As New OpcjeKorekcjiMip()
        opcjeCad.UstawWgProfilu(ProfilKorektyRastra.PodkladCadStonowany)
        Assert.True(opcjeCad.Wlaczona)
        Assert.Equal(0.85, opcjeCad.Gamma)
        Assert.Equal(0.6, opcjeCad.Kontrast)
        Assert.Equal(0.6, opcjeCad.Nasycenie)

        Dim opcjeSkan As New OpcjeKorekcjiMip()
        opcjeSkan.UstawWgProfilu(ProfilKorektyRastra.SkanCadCzarnoBialy)
        Assert.True(opcjeSkan.Wlaczona)
        Assert.Equal(0.85, opcjeSkan.Gamma)
        Assert.Equal(1.5, opcjeSkan.Kontrast)
        Assert.Equal(0.0, opcjeSkan.Nasycenie)
    End Sub

    Private Shared Function Szary(v As Integer) As Integer
        Return CInt(&HFF000000) Or (v << 16) Or (v << 8) Or v
    End Function

    ''' <summary>Profil „Podkład CAD (rozjaśniony)” rozjaśnia obraz i zachowuje biel (dawniej biel 255 -> 236, szarość 128 -> 111).</summary>
    <Fact>
    Public Sub PodkladCad_RozjasniaIZachowujeBiel()
        Dim opcje As New OpcjeKorekcjiMip()
        opcje.UstawWgProfilu(ProfilKorektyRastra.PodkladCadStonowany)
        Dim piksele() As Integer = {Szary(255), Szary(128), Szary(0)}
        KorekcjaMip.Zastosuj(piksele, 3, 1, opcje)
        Assert.Equal(255, piksele(0) And &HFF)
        Assert.True((piksele(1) And &HFF) > 160, "szarość 128 powinna wyraźnie się rozjaśnić: " & (piksele(1) And &HFF))
        Assert.True((piksele(2) And &HFF) > 80, "czerń powinna się rozjaśnić (podkład stonowany): " & (piksele(2) And &HFF))
    End Sub

    ''' <summary>Kontrast poniżej 1 spłaszcza tony w stronę bieli; powyżej 1 - rozciąga wokół środka jak dawniej.</summary>
    <Fact>
    Public Sub Kontrast_PonizejJeden_ZachowujeBiel_PowyzejJeden_WokolSrodka()
        Dim slaby As New OpcjeKorekcjiMip With {.Wlaczona = True, .Gamma = 1.0, .Kontrast = 0.5, .Nasycenie = 1.0, .Wyostrzanie = 0}
        Dim p() As Integer = {Szary(255), Szary(0)}
        KorekcjaMip.Zastosuj(p, 2, 1, slaby)
        Assert.Equal(255, p(0) And &HFF)
        Assert.Equal(128, p(1) And &HFF)

        Dim mocny As New OpcjeKorekcjiMip With {.Wlaczona = True, .Gamma = 1.0, .Kontrast = 2.0, .Nasycenie = 1.0, .Wyostrzanie = 0}
        Dim q() As Integer = {Szary(64), Szary(192)}
        KorekcjaMip.Zastosuj(q, 2, 1, mocny)
        Assert.Equal(0, q(0) And &HFF)
        Assert.Equal(255, q(1) And &HFF)
    End Sub

    Private Shared Function LosowyBufor(szer As Integer, wys As Integer, kanaly As Integer, ziarno As Integer) As Byte()
        Dim los As New Random(ziarno)
        Dim b(szer * wys * kanaly - 1) As Byte
        los.NextBytes(b)
        If kanaly = 4 Then
            For i = 3 To b.Length - 1 Step 4
                b(i) = If(b(i) < 30, CByte(0), CByte(255)) 'część pikseli przezroczysta
            Next
        End If
        Return b
    End Function

    ''' <summary>
    ''' Obraz poprawiany pasami (GeoTIFF, scalanie) jest identyczny jak poprawiony w całości - także pasy po 1 wierszu.
    ''' Dawniej dwa wiersze na każdym styku pasów miały o ok. 11% mniejszy kontrast lokalny.
    ''' </summary>
    <Theory>
    <InlineData(3)>
    <InlineData(4)>
    Public Sub KorekcjaPasow_WynikJakBezPodzialuNaPasy(kanaly As Integer)
        Const Szer As Integer = 40, Wys As Integer = 50
        Dim opcje As New OpcjeKorekcjiMip()
        opcje.UstawWgProfilu(ProfilKorektyRastra.GarminMip)
        Dim zrodlo = LosowyBufor(Szer, Wys, kanaly, 11 + kanaly)
        Dim cale = CType(zrodlo.Clone(), Byte())
        KorekcjaMip.ZastosujDoBuforaRgb(cale, Szer, Wys, kanaly, opcje)

        Dim bajtyWiersza = Szer * kanaly
        Dim wynik As New List(Of Byte)
        Dim zapis As Action(Of Byte(), Integer) = Sub(b, n) wynik.AddRange(b.Take(n * bajtyWiersza))
        Dim kp As New KorekcjaPasow(Szer, kanaly, opcje)
        Dim y As Integer = 0
        For Each n In {7, 1, 13, 2, 20, 7}
            Dim pas(n * bajtyWiersza - 1) As Byte
            Array.Copy(zrodlo, y * bajtyWiersza, pas, 0, pas.Length)
            kp.Dodaj(pas, n, zapis)
            y += n
        Next
        Assert.Equal(Wys, y)
        kp.Zakoncz(zapis)
        Assert.Equal(cale, wynik.ToArray())
    End Sub

    <Fact>
    Public Sub Wytnij_ZwracaSrodekAlboNothingGdyPrzezroczysty()
        Dim p(4 * 3 - 1) As Integer
        p(1 * 4 + 1) = Szary(10)
        p(1 * 4 + 2) = Szary(20)
        Dim srodek = KorekcjaMip.Wytnij(p, 4, 3, 1)
        Assert.Equal({Szary(10), Szary(20)}, srodek)
        'dane tylko w marginesie - kafel pusty
        Dim tylkoMargines(4 * 3 - 1) As Integer
        tylkoMargines(0) = Szary(10)
        Assert.Null(KorekcjaMip.Wytnij(tylkoMargines, 4, 3, 1))
    End Sub

    ''' <summary>
    ''' Odtworzenie zapamiętanych opcji: własne wartości suwaków zostają, odznaczona korekta zostaje odznaczona
    ''' (dawniej UstawWgProfilu ponownie ją włączało), profil „Brak” nigdy nie włącza korekty.
    ''' </summary>
    <Fact>
    Public Sub OpcjeKorekcji_Odtworz()
        Dim wlasne = OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.Wlasny, 1.1, 0.7, 0.4, 1.2)
        Assert.True(wlasne.Wlaczona)
        Assert.Equal(ProfilKorektyRastra.Wlasny, wlasne.Profil)
        Assert.Equal(1.1, wlasne.Gamma)
        Assert.Equal(0.7, wlasne.Kontrast)
        Assert.Equal(0.4, wlasne.Nasycenie)
        Assert.Equal(1.2, wlasne.Wyostrzanie)

        Dim odznaczona = OpcjeKorekcjiMip.Odtworz(False, ProfilKorektyRastra.GarminMip, 0, 0, 0, 0)
        Assert.False(odznaczona.Wlaczona)
        Assert.Equal(1.25, odznaczona.Kontrast)

        Assert.False(OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.Brak, 1, 1, 1, 1).Wlaczona)
        Assert.Equal(ProfilKorektyRastra.Brak, OpcjeKorekcjiMip.Odtworz(True, CType(99, ProfilKorektyRastra), 1, 1, 1, 1).Profil)
        'wartości spoza zakresu suwaków ograniczone
        Dim poza = OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.Wlasny, 9, Double.NaN, -1, 5)
        Assert.Equal(1.5, poza.Gamma)
        Assert.Equal(1.25, poza.Kontrast)
        Assert.Equal(0.0, poza.Nasycenie)
        Assert.Equal(1.5, poza.Wyostrzanie)
    End Sub

    ''' <summary>Wartości suwaków profilu „Własny” w lastsettings.txt (dawniej zapisywany był tylko numer profilu).</summary>
    <Fact>
    Public Sub UstawieniaKorekty_ZapisIOdczytLastsettings()
        Dim plik = Path.GetTempFileName()
        Dim kultura = Globalization.CultureInfo.CurrentCulture
        Try
            Globalization.CultureInfo.CurrentCulture = New Globalization.CultureInfo("pl-PL")
            Dim u As New MapoTero.UstawieniaProgramu()
            u.ZapamietajKorekte(OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.Wlasny, 1.1, 0.7, 0.45, 1.2))
            u.Zapisz(plik)
            Assert.Contains("0.45", File.ReadAllText(plik))
            Dim w As New MapoTero.UstawieniaProgramu()
            w.Wczytaj(plik)
            Dim o = w.OpcjeKorekty()
            Assert.True(o.Wlaczona)
            Assert.Equal(ProfilKorektyRastra.Wlasny, o.Profil)
            Assert.Equal(1.1, o.Gamma)
            Assert.Equal(0.7, o.Kontrast)
            Assert.Equal(0.45, o.Nasycenie)
            Assert.Equal(1.2, o.Wyostrzanie)

            'odznaczona korekta z wybranym profilem zostaje odznaczona po ponownym otwarciu okna
            Dim odz = OpcjeKorekcjiMip.Odtworz(True, ProfilKorektyRastra.GarminMip, 0, 0, 0, 0)
            odz.Wlaczona = False
            w.ZapamietajKorekte(odz)
            w.Zapisz(plik)
            Dim w2 As New MapoTero.UstawieniaProgramu()
            w2.Wczytaj(plik)
            Assert.False(w2.OpcjeKorekty().Wlaczona)
            Assert.Equal(ProfilKorektyRastra.GarminMip, w2.OpcjeKorekty().Profil)

            'plik starszej wersji (bez wartości suwaków) - wartości domyślne
            File.WriteAllText(plik, "korekta_profil" & vbCrLf & "7" & vbCrLf & "korekta_wlaczona" & vbCrLf & "True" & vbCrLf)
            Dim w3 As New MapoTero.UstawieniaProgramu()
            w3.Wczytaj(plik)
            Assert.Equal(0.75, w3.OpcjeKorekty().Gamma)
        Finally
            Globalization.CultureInfo.CurrentCulture = kultura
            File.Delete(plik)
        End Try
    End Sub

    ''' <summary>
    ''' Wartości każdego profilu mieszczą się w zakresach suwaków okna podglądu. Dawniej suwaki obcinały wartości
    ''' profili CAD (nasycenie 0 -> 1), więc „Skan CAD (czarno-biały)” wybrany w podglądzie dawał obraz kolorowy.
    ''' Okno: wybór profilu „Brak” wyłącza korektę także w podglądzie.
    ''' </summary>
    <Fact>
    <Trait("Kategoria", "UI")>
    Public Sub OknoPodgladu_ProfileMieszczaSieWZakresachSuwakow_BrakWylaczaKorekte()
        Dim bledy As New List(Of String)
        OknoTestowe.WykonywajNaSta(
            Sub()
                For Each p In DirectCast([Enum].GetValues(GetType(ProfilKorektyRastra)), ProfilKorektyRastra())
                    Dim o As New OpcjeKorekcjiMip()
                    o.UstawWgProfilu(p)
                    Using f As New MapoTero.FormPodgladMip(o, CType(Nothing, Bitmap))
                        OknoTestowe.Pokaz(f)
                        System.Windows.Forms.Application.DoEvents()
                        Dim w = f.Opcje
                        If Math.Abs(w.Gamma - o.Gamma) > 0.001 OrElse Math.Abs(w.Kontrast - o.Kontrast) > 0.001 OrElse
                           Math.Abs(w.Nasycenie - o.Nasycenie) > 0.001 OrElse Math.Abs(w.Wyostrzanie - o.Wyostrzanie) > 0.001 Then
                            bledy.Add($"{p}: suwaki zmieniły wartości profilu ({w.Gamma}, {w.Kontrast}, {w.Nasycenie}, {w.Wyostrzanie})")
                        End If
                        f.cmbProfil.SelectedIndex = CInt(ProfilKorektyRastra.Brak)
                        System.Windows.Forms.Application.DoEvents()
                        If f.Opcje.Wlaczona Then bledy.Add($"{p}: po wyborze profilu Brak korekta w podglądzie nadal włączona")
                        f.Close()
                    End Using
                Next
            End Sub)
        Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
    End Sub

    Private Shared Function KafelPng(piksele() As Integer, x0 As Integer, y0 As Integer, szerCalosci As Integer) As Byte()
        Dim kafel(256 * 256 - 1) As Integer
        For y = 0 To 255
            Array.Copy(piksele, (y0 + y) * szerCalosci + x0, kafel, y * 256, 256)
        Next
        Using bmp = MapoTero.NakladanieObrazow.DoBitmapy(kafel, 256, 256), ms As New MemoryStream()
            bmp.Save(ms, ImageFormat.Png)
            Return ms.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' Kafel niższego poziomu piramidy z wyostrzaniem jest identyczny z wycinkiem całego obrazu pomniejszonego
    ''' i wyostrzonego - brzegi kafla wyostrzone jak środek (dawniej niższe poziomy traciły wyostrzenie).
    ''' </summary>
    <Fact>
    Public Sub KafelZDzieciWyostrzony_JakWycinekWyostrzonegoObrazu()
        Dim los As New Random(5)
        Dim caly(1024 * 1024 - 1) As Integer
        For i = 0 To caly.Length - 1
            caly(i) = Szary(los.Next(60, 200))
        Next
        Dim dzieci(15)() As Byte
        For i = 0 To 15
            dzieci(i) = KafelPng(caly, (i Mod 4) * 256, (i \ 4) * 256, 1024)
        Next
        Dim opcje As New OpcjeKorekcjiMip()
        opcje.UstawWgProfilu(ProfilKorektyRastra.GarminMip)

        Dim oczekiwany() As Integer
        Using duzy = MapoTero.NakladanieObrazow.DoBitmapy(caly, 1024, 1024), sredni As New Bitmap(512, 512, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(sredni)
                g.Clear(Color.Transparent)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
                g.DrawImage(duzy, New Rectangle(0, 0, 512, 512), 0, 0, 1024, 1024, GraphicsUnit.Pixel)
            End Using
            Dim p = MapoTero.NakladanieObrazow.PikseleBitmapy(sredni)
            KorekcjaMip.Zastosuj(p, 512, 512, opcje.TylkoWyostrzanie())
            oczekiwany = KorekcjaMip.Wytnij(p, 512, 512, 128)
        End Using

        Using wynik = MapoTero.EksportMapy.KafelZDzieciWyostrzony(dzieci, opcje)
            Assert.Equal(oczekiwany, MapoTero.NakladanieObrazow.PikseleBitmapy(wynik))
        End Using
        'bez danych w środkowych kaflach - kafel pusty
        Dim tylkoSasiedzi = CType(dzieci.Clone(), Byte()())
        For Each i In {5, 6, 9, 10}
            tylkoSasiedzi(i) = Nothing
        Next
        Assert.Null(MapoTero.EksportMapy.KafelZDzieciWyostrzony(tylkoSasiedzi, opcje))
    End Sub

End Class
