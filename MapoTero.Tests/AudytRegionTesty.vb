'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Audyt (paczka 0): działanie programu przy formacie regionalnym innym niż polski (Windows: Ustawienia > Czas i język >
''' Region > Format regionalny). Polscy użytkownicy często mają format „angielski (USA)” przy polskiej stronie kodowej
''' systemu (1250). Testy opisują oczekiwane zachowanie - niepowodzenie oznacza znalezisko audytu.
''' </summary>
<Trait("Kategoria", "Audyt")>
Public Class AudytRegionTesty

    <DllImport("kernel32.dll")>
    Private Shared Function GetACP() As Integer
    End Function

    Private Shared Sub WKulturze(nazwa As String, akcja As Action)
        Dim stara = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = New CultureInfo(nazwa)
            akcja()
        Finally
            CultureInfo.CurrentCulture = stara
        End Try
    End Sub

    ''' <summary>Starsze wersje (FileOpen/Print, Encoding.Default w .NET Framework) zapisywały pliki w stronie kodowej systemu (GetACP).</summary>
    <FaktAudytu>
    Public Sub KodowanieSystemowe_NieZalezyOdFormatuRegionalnego()
        Dim systemowa = GetACP()
        For Each k In {"pl-PL", "en-US", "de-DE", "en-GB"}
            Dim cp As Integer = 0
            WKulturze(k, Sub() cp = Global.MapoTero.Program.KodowanieSystemowe().CodePage)
            Assert.True(cp = systemowa, $"format {k}: KodowanieSystemowe() = {cp}, strona kodowa systemu = {systemowa}")
        Next
    End Sub

    ''' <summary>Ścieżka folderu sesji z polskimi literami (np. nazwa użytkownika Windows) musi przetrwać zapis i odczyt lastsettings.txt.</summary>
    <FaktAudytu>
    Public Sub Lastsettings_PolskieLiteryWSciezce_PrzyFormacieAngielskim()
        Dim plik = Path.Combine(Path.GetTempPath(), "audyt_lastsettings_" & Guid.NewGuid().ToString("N") & ".txt")
        Dim folder = "C:\Users\Łukasz Żółć\MapoTero\download\sesja_ąęśń\"
        Try
            Dim odczytany As String = ""
            WKulturze("en-US",
                Sub()
                    Dim u As New Global.MapoTero.UstawieniaProgramu() With {.FolderSegmentow = folder}
                    u.Zapisz(plik)
                    Dim v As New Global.MapoTero.UstawieniaProgramu()
                    v.Wczytaj(plik)
                    odczytany = v.FolderSegmentow
                End Sub)
            Assert.Equal(folder, odczytany)
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Sub

    ''' <summary>conf.txt zapisany przy formacie polskim (starsza wersja, inny komputer) czytany przy formacie angielskim.</summary>
    <FaktAudytu>
    Public Sub Conf_ZapisanyPrzyPolskim_CzytanyPrzyAngielskim()
        Dim plik = Path.Combine(Path.GetTempPath(), "audyt_conf_" & Guid.NewGuid().ToString("N") & ".txt")
        Dim folder = "C:\Mapy\Łódź_północ\"
        Try
            WKulturze("pl-PL",
                Sub()
                    Dim c As New KonfiguracjaSesji With {.Folder = folder, .RodzajMapy = "Ortofotomapa - źródło GUGiK", .XDol = "1", .YLewy = "1", .XGora = "2", .YPrawy = "2",
                                                         .RozmiarPiksela = "0.5", .BokSegmentuPx = "2000", .UkladEpsg = 2180, .Format = "jpeg", .Prefiks = "_", .Numeracja = "1"}
                    c.Zapisz(plik, Global.MapoTero.Program.KodowanieSystemowe())
                End Sub)
            Dim odczytana As KonfiguracjaSesji = Nothing
            WKulturze("en-US", Sub() odczytana = KonfiguracjaSesji.Wczytaj(plik, Global.MapoTero.Program.KodowanieSystemowe()))
            Assert.Equal(folder, odczytana.Folder)
            Assert.Equal("Ortofotomapa - źródło GUGiK", odczytana.RodzajMapy)
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Sub

    ''' <summary>Liczby wpisywane przez użytkownika - niezależnie od formatu regionalnego.</summary>
    <FaktAudytu>
    Public Sub Wartosc_NiezaleznaOdFormatu()
        For Each k In {"pl-PL", "en-US", "de-DE", "fr-FR"}
            WKulturze(k,
                Sub()
                    Assert.Equal(0.5, Global.MapoTero.Program.Wartosc("0,5"))
                    Assert.Equal(0.5, Global.MapoTero.Program.Wartosc("0.5"))
                    Assert.Equal(1234.5, Global.MapoTero.Program.Wartosc(" 1234.5 "))
                End Sub)
        Next
    End Sub

End Class
