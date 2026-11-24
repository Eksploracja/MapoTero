'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>Okno Ustawienia (Form2): blokada w trakcie pobierania i układ kontrolek.</summary>
<Collection("OknaKreatora")>
<Trait("Kategoria", "UI")>
Public Class UstawieniaOknoTesty

    Private Shared Sub WOknie(trwaPobieranie As Boolean, akcja As Action(Of Form))
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form2()
                    f.TrwaPobieranie = trwaPobieranie
                    f.StartPosition = FormStartPosition.Manual
                    f.Location = Point.Empty
                    OknoTestowe.Pokaz(f)
                    Application.DoEvents()
                    akcja(f)
                    f.Close()
                End Using
            End Sub)
    End Sub

    ''' <summary>Obszar kontrolki z szerokością tekstu dla kontrolek o rozmiarze automatycznym.</summary>
    Private Shared Function Obszar(c As Control) As Rectangle
        Dim r As Rectangle = If(c.AutoSize, New Rectangle(c.Location, c.PreferredSize), c.Bounds)
        'tolerancja 2 px - napis nad polem może dotykać jego krawędzi
        r.Inflate(-2, -2)
        Return r
    End Function

    Private Shared Function K(f As Form, nazwa As String) As Control
        Return f.Controls.Find(nazwa, True)(0)
    End Function

    ''' <summary>
    ''' W trakcie pobierania okno otwiera się (dawniej - tylko komunikat „Trwa pobieranie”), ale ustawienia sesji,
    ''' pobierania i siatki są wyłączone; widok współrzędnych i układ domyślny można zmienić.
    ''' </summary>
    <Fact>
    Public Sub WTrakciePobieraniaDostepneTylkoUstawieniaWidoku()
        WOknie(True,
            Sub(f)
                For Each nazwa In {"grpSesja", "grpKalibracja", "grpPobieranie", "grpWmts",
                                   "chkSiatkaSegmentow", "btnPrzywroc"}
                    Assert.False(K(f, nazwa).Enabled, nazwa & " powinno być wyłączone w trakcie pobierania")
                Next
                For Each nazwa In {"chkKursorISrodek", "chkKursorWgs", "chkZaznaczenieWgs", "cmbUkladDomyslny", "btnOK", "btnAnuluj"}
                    Assert.True(K(f, nazwa).Enabled, nazwa & " powinno być dostępne w trakcie pobierania")
                Next
                Assert.Contains("trwa pobieranie", f.Text)
            End Sub)
        WOknie(False,
            Sub(f)
                For Each nazwa In {"grpSesja", "grpPobieranie", "chkSiatkaSegmentow", "btnPrzywroc"}
                    Assert.True(K(f, nazwa).Enabled, nazwa)
                Next
                Assert.Equal("Ustawienia", f.Text)
            End Sub)
    End Sub

    ''' <summary>
    ''' Jakość JPEG dotyczy też segmentów składanych z kilku serwerów WMS, więc leży w grupie Pobieranie, a nie w grupie
    ''' usług kafelkowych; ramki się nie nakładają, a kontrolki mieszczą się w ramkach (po przesunięciu pól sesji).
    ''' </summary>
    <Fact>
    Public Sub UkladKontrolekBezNakladania()
        WOknie(False,
            Sub(f)
                Assert.Equal("grpPobieranie", K(f, "nudJakoscWmts").Parent.Name)
                Assert.Equal("grpPobieranie", K(f, "lblJakoscWmts").Parent.Name)
                Dim bledy As New List(Of String)
                Dim ramki As New List(Of Control)
                For Each c As Control In f.Controls
                    If Not f.ClientRectangle.Contains(c.Bounds) Then bledy.Add($"{c.Name} poza oknem: {c.Bounds}")
                    If TypeOf c Is GroupBox OrElse TypeOf c Is Button Then ramki.Add(c)
                    If TypeOf c Is GroupBox Then
                        For Each d As Control In c.Controls
                            Dim szer As Integer = If(d.AutoSize, d.PreferredSize.Width, d.Width)
                            If d.Left + szer > c.Width - 2 OrElse d.Bottom > c.Height - 2 Then
                                bledy.Add($"{c.Name}/{d.Name} wystaje poza ramkę ({d.Left + szer}x{d.Bottom} przy {c.Size})")
                            End If
                        Next
                        'napis nie może wchodzić pod sąsiednią kontrolkę (błąd: „Jakość JPEG składanych segmentów”
                        'chowała się pod polem liczbowym)
                        Dim dzieci = c.Controls.Cast(Of Control)().ToList()
                        For i = 0 To dzieci.Count - 1
                            For j = i + 1 To dzieci.Count - 1
                                If Obszar(dzieci(i)).IntersectsWith(Obszar(dzieci(j))) Then
                                    bledy.Add($"{c.Name}: {dzieci(i).Name} nakłada się na {dzieci(j).Name}")
                                End If
                            Next
                        Next
                    End If
                Next
                For i = 0 To ramki.Count - 1
                    For j = i + 1 To ramki.Count - 1
                        If ramki(i).Bounds.IntersectsWith(ramki(j).Bounds) Then bledy.Add($"{ramki(i).Name} nakłada się na {ramki(j).Name}")
                    Next
                Next
                Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
            End Sub)
    End Sub

    ''' <summary>
    ''' Usunięte z okna: „pobieraj tylko powyżej ostatniego” (wznawianie działa bez niej), „zamień X i Y” (działała na
    ''' wszystkie serwery) i „wpisywanie XY z klawiatury” (przeniesione do pola przy współrzędnych w oknie głównym).
    ''' </summary>
    <Fact>
    Public Sub UsunieteOpcjeNieWystepujaWOknie()
        WOknie(False,
            Sub(f)
                For Each nazwa In {"chkPowyzejOstatniego", "chkZamienXY", "grpZaawansowane", "chkEdycjaXY"}
                    Assert.Empty(f.Controls.Find(nazwa, True))
                Next
            End Sub)
    End Sub

    ''' <summary>
    ''' Zamiana X i Y nie jest odczytywana z lastsettings.txt (pozostawiona włączona psuła pobieranie z Geoportalu,
    ''' a po usunięciu pola nie dałoby się jej wyłączyć); wpis zostaje zapisany jako wyłączony dla starszych wersji.
    ''' Podkład mapy jest zapamiętywany (dawniej po każdym uruchomieniu wracał OpenStreetMap).
    ''' </summary>
    <Fact>
    Public Sub LastsettingsPodkladMapyIZamianaXY()
        Dim plik = IO.Path.GetTempFileName()
        Try
            IO.File.WriteAllText(plik, "XYswitched" & vbCrLf & "True" & vbCrLf & "podklad_mapy" & vbCrLf & "bing" & vbCrLf)
            Dim u As New MapoTero.UstawieniaProgramu()
            u.Wczytaj(plik)
            Assert.Equal("bing", u.PodkladMapy)
            u.PodkladMapy = "google"
            u.Zapisz(plik)
            Dim tekst = IO.File.ReadAllText(plik)
            Assert.Contains("XYswitched" & vbCrLf & "False", tekst)
            Dim w As New MapoTero.UstawieniaProgramu()
            w.Wczytaj(plik)
            Assert.Equal("google", w.PodkladMapy)
            'nieznana wartość - OpenStreetMap
            IO.File.WriteAllText(plik, "podklad_mapy" & vbCrLf & "yandex" & vbCrLf)
            Dim n As New MapoTero.UstawieniaProgramu()
            n.Wczytaj(plik)
            Assert.Equal("osm", n.PodkladMapy)
        Finally
            IO.File.Delete(plik)
        End Try
    End Sub

End Class
