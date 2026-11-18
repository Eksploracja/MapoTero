'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>Testy opóźnienia i zachowania dymków pomocy (ToolTip) w panelach warstw i danych źródłowych.</summary>
Public Class PodpowiedziTesty

    <Fact>
    Public Sub Form1ToolTip1UstawieniaOpoznienia()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using f As New MapoTero.Form1()
                        Dim wlasnosc = GetType(MapoTero.Form1).GetProperty("ToolTip1", BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
                        Assert.NotNull(wlasnosc)
                        Dim tt = DirectCast(wlasnosc.GetValue(f), ToolTip)
                        Assert.NotNull(tt)
                        Assert.Equal(500, tt.InitialDelay)
                        Assert.Equal(500, tt.ReshowDelay)
                        Assert.True(tt.AutoPopDelay >= 10000)
                        Assert.True(tt.IsBalloon)
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub DrzewoWarstwUstawieniaPodpowiedzi()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using tv As New MapoTero.DrzewoWarstw()
                        Assert.True(tv.ShowNodeToolTips)
                        'Tworzymy uchwyt okna, aby wywołać OnHandleCreated i UstawCzasyPodpowiedzi
                        tv.CreateControl()
                        Assert.True(tv.IsHandleCreated)
                        tv.UstawCzasyPodpowiedzi()
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub ListaWarstwRuchKursoraNieRzucaWyjatkow()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using lst As New MapoTero.ListaWarstw()
                        Using tt As New ToolTip()
                            lst.Podpowiedz = tt
                            lst.OpisListy = "Opis listy"
                            Dim zestaw As New ZestawWarstw()
                            zestaw.Dodaj("orto", 100, "", "Ortofotomapa")
                            zestaw.Dodaj("topo", 80, "", "Topograficzna")
                            lst.Zestaw = zestaw
                            lst.Size = New Drawing.Size(200, 200)
                            lst.CreateControl()

                            Dim onMouseMove = GetType(MapoTero.ListaWarstw).GetMethod("OnMouseMove", BindingFlags.Instance Or BindingFlags.NonPublic)
                            Dim onMouseLeave = GetType(MapoTero.ListaWarstw).GetMethod("OnMouseLeave", BindingFlags.Instance Or BindingFlags.NonPublic)

                            'Symulacja ruchu kursora po wierszach
                            onMouseMove.Invoke(lst, {New MouseEventArgs(MouseButtons.None, 0, 50, 180, 0)})
                            onMouseMove.Invoke(lst, {New MouseEventArgs(MouseButtons.None, 0, 50, 150, 0)})
                            onMouseMove.Invoke(lst, {New MouseEventArgs(MouseButtons.None, 0, 50, 50, 0)})
                            onMouseLeave.Invoke(lst, {EventArgs.Empty})
                        End Using
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub TestBtnSzukajDzialkiWForm1()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Application.EnableVisualStyles()
                    Using f As New MapoTero.Form1()
                        f.Show()
                        Application.DoEvents()
                        Dim btn = DirectCast(f.Controls("btnSzukajDzialki"), Button)
                        Assert.NotNull(btn)
                        Assert.Equal("", btn.Text)
                        Assert.NotNull(btn.Image)
                        Assert.Equal(16, btn.Image.Width)
                        Assert.Equal(16, btn.Image.Height)

                        Dim wlasnosc = GetType(MapoTero.Form1).GetProperty("ToolTip1", BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
                        Dim tt = DirectCast(wlasnosc.GetValue(f), ToolTip)
                        Dim tip = tt.GetToolTip(btn)
                        Assert.Contains("Wyszukaj miejscowość", tip)
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub FolderSesjiDlaNarzedzia_BiezacaSesjaAlboNajnowszaZUwaga()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_Narzedzia_" & Guid.NewGuid().ToString("N")) & "\"
        Directory.CreateDirectory(folder)
        Try
            'folder bez conf.txt - okno przechodzi na najnowszą sesję i mówi o tym; gdy żadnej sesji nie ma - download bez uwagi
            Dim uwaga As String = Nothing
            Dim wynik = MapoTero.FolderSesjiDlaNarzedzia(folder, uwaga)
            If wynik = MapoTero.FolderDownload Then
                Assert.Equal("", uwaga)
            Else
                Assert.Contains("nie zawiera pliku conf.txt", uwaga)
                Assert.Contains(wynik, uwaga)
            End If

            'bieżąca sesja z conf.txt - bez zmiany folderu i bez uwagi
            File.WriteAllText(folder & "conf.txt", "test")
            wynik = MapoTero.FolderSesjiDlaNarzedzia(folder, uwaga)
            Assert.Equal(folder, wynik)
            Assert.Equal("", uwaga)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub UsuwaniePustych_TylkoBiezacaSesja()
        Dim folder = Path.Combine(Path.GetTempPath(), "MapoTero_Usuwanie_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            'istniejący folder (także bez conf.txt, np. folder segmentów spoza programu) - bieżąca sesja
            Assert.Equal(folder & "\", MapoTero.Usuwanie_p_seg.FolderBiezacejSesji(folder))
            'sam download, brak folderu albo folder nieistniejący - brak bieżącej sesji (bez przejścia na najnowszą)
            Assert.Equal("", MapoTero.Usuwanie_p_seg.FolderBiezacejSesji(MapoTero.FolderDownload))
            Assert.Equal("", MapoTero.Usuwanie_p_seg.FolderBiezacejSesji(""))
            Assert.Equal("", MapoTero.Usuwanie_p_seg.FolderBiezacejSesji(folder & "_nie_ma"))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub DodajPrzyciskPomocy_JedenPrzyciskPrzyPonownymOtwarciu()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using f As New Form()
                        Dim b1 = PomocProgramu.DodajPrzyciskPomocy(f, TematPomocy.Scalanie)
                        Dim b2 = PomocProgramu.DodajPrzyciskPomocy(f, TematPomocy.Scalanie)
                        Assert.Same(b1, b2)
                        Assert.Equal(1, f.Controls.Find("btnPomoc", False).Length)
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub FormEksport_RozmiarBazowyObejmujeKontrolkiPoSkalowaniu()
        'okno jest skalowane do bieżącej czcionki już w konstruktorze; rozmiar bazowy, do którego DopasujDoWysokosciEkranu
        'ustawia okno, musi obejmować wszystkie kontrolki (dawniej 510 x 788 z projektu obcinało prawą i dolną część)
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using frm As New MapoTero.FormEksport()
                        Dim typ = GetType(MapoTero.FormEksport)
                        Dim flagi = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
                        Dim szer = CInt(typ.GetProperty("BazowaSzerokoscKlienta", flagi).GetValue(frm))
                        Dim wys = CInt(typ.GetProperty("BazowaWysokoscKlienta", flagi).GetValue(frm))
                        Dim skala = PomocProgramu.SkalaProjektanta(frm)
                        Assert.Equal(CInt(Math.Round(510 * skala.Width)), szer)
                        For Each c As Control In frm.Controls
                            Assert.True(c.Right <= szer + 1, c.Name & " wychodzi poza szerokość " & szer & ": " & c.Right)
                        Next
                        Assert.True(wys >= 788)
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

    <Fact>
    Public Sub TestFormSzukajDzialkiZakladkiIPomoc()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    MapoTero.FormSzukajDzialki.OstatniaZakladka = 0
                    Using frm As New MapoTero.FormSzukajDzialki()
                        frm.Show()
                        Application.DoEvents()

                        Dim tab = DirectCast(frm.Controls("tabSzukaj"), TabControl)
                        Assert.NotNull(tab)
                        Assert.Equal(2, tab.TabCount)
                        Assert.Equal("Miejscowość / Adres", tab.TabPages(0).Text)
                        Assert.Equal("Działka katastralna (ULDK)", tab.TabPages(1).Text)

                        Dim btnPomoc = frm.Controls("btnPomoc")
                        Assert.NotNull(btnPomoc)
                        Assert.Equal("Pomoc", btnPomoc.Text)
                        'położenie z projektu skalowane razem z oknem - przycisk w oknie i nie zasłania innych kontrolek
                        Assert.True(frm.ClientRectangle.Contains(btnPomoc.Bounds), "przycisk Pomoc poza oknem: " & btnPomoc.Bounds.ToString())
                        For Each c As Control In frm.Controls
                            If c Is btnPomoc OrElse Not c.Visible Then Continue For
                            Assert.False(c.Bounds.IntersectsWith(btnPomoc.Bounds), "przycisk Pomoc zasłania " & c.Name)
                        Next

                        Dim btnWybierz = frm.Controls("btnWybierz")
                        Assert.NotNull(btnWybierz)
                        Assert.Equal("Pokaż na mapie", btnWybierz.Text)
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw blad
    End Sub

End Class
