'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit
Imports Xunit.Abstractions

''' <summary>
''' Audyt etap 0 ('kliknij wszystko'): otwiera każde okno programu, wpisuje w pola zestawy danych skrajnych
''' i klika każdy dostępny przycisk i polecenie menu. Zapisuje nieobsłużone wyjątki, treść komunikatów
''' (MessageBox) i kliknięcia, które blokowały okno dłużej niż 2 s. Test nie kończy się błędem - wynik jest
''' raportem w %TEMP%\audyt\klik.txt. Uruchamianie: zmienna MAPOTERO_AUDYT=1 i --filter "Kategoria=Audyt" (klik.txt bez sesji, klik_sesja.txt z sesją testową).
''' </summary>
<Trait("Kategoria", "Audyt")>
Public Class AudytKliknijWszystkoTesty

    Private ReadOnly _wyjscie As ITestOutputHelper
    Public Sub New(wyjscie As ITestOutputHelper)
        _wyjscie = wyjscie
    End Sub

    'polecenia otwierające przeglądarkę, Eksplorator lub pobierające dane z sieci w dużej ilości - pomijane
    Private Shared ReadOnly Pomijane As New Regex("pomoc|help|^\?$|podręcznik|forum|przegl|pokaż plik|pokaz|otwórz folder|otworz folder|^pobierz$|odśwież mapę|zakończ|wyjście|btnPobierz$|btnPokazPlik|mnuOtworzPrzegladarka|ToolStripMenuItemPomoc",
                                                  RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private Shared ReadOnly ZestawyDanych As String() = {Nothing, "", "abc", "-1", "0", "99999999999", "1,5"}

    Private ReadOnly _raport As New StringBuilder()
    Private _biezacaAkcja As String = ""
    Private _biezaceOkno As String = ""
    Private ReadOnly _komunikaty As New List(Of String)()

#Region "WinAPI - zamykanie okien dialogowych"
    Private Delegate Function EnumProc(hWnd As IntPtr, lParam As IntPtr) As Boolean
    <DllImport("user32.dll")> Private Shared Function EnumThreadWindows(id As Integer, cb As EnumProc, lParam As IntPtr) As Boolean
    End Function
    <DllImport("kernel32.dll")> Private Shared Function GetCurrentThreadId() As Integer
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Unicode)> Private Shared Function GetClassName(h As IntPtr, s As StringBuilder, n As Integer) As Integer
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Unicode)> Private Shared Function GetWindowText(h As IntPtr, s As StringBuilder, n As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Private Shared Function IsWindowVisible(h As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function GetDlgItem(h As IntPtr, id As Integer) As IntPtr
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Unicode)> Private Shared Function GetDlgItemText(h As IntPtr, id As Integer, s As StringBuilder, n As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Private Shared Function PostMessage(h As IntPtr, msg As Integer, w As IntPtr, l As IntPtr) As Boolean
    End Function
    Private Const WM_COMMAND As Integer = &H111
    Private Const WM_CLOSE As Integer = &H10
    Private ReadOnly _zamkniete As New HashSet(Of IntPtr)()

    ''' <summary>Zamyka okna dialogowe Windows (MessageBox, wybór pliku/folderu) - wybiera Anuluj, Nie albo OK.</summary>
    Private Sub ZamknijDialogi()
        Dim watek = GetCurrentThreadId()
        EnumThreadWindows(watek,
            Function(h, l)
                If Not IsWindowVisible(h) Then Return True
                Dim kl As New StringBuilder(64)
                GetClassName(h, kl, 64)
                If kl.ToString() <> "#32770" Then Return True
                If _zamkniete.Contains(h) Then Return True
                _zamkniete.Add(h)
                Dim tytul As New StringBuilder(256), tresc As New StringBuilder(2048)
                GetWindowText(h, tytul, 256)
                GetDlgItemText(h, &HFFFF, tresc, 2048)
                Dim wpis = $"[{_biezaceOkno}] {_biezacaAkcja} -> okno '{tytul}': {tresc.ToString().Replace(vbCr, " ").Replace(vbLf, " ")}"
                _komunikaty.Add(wpis)
                For Each id In {2, 7, 1}
                    If GetDlgItem(h, id) <> IntPtr.Zero Then
                        PostMessage(h, WM_COMMAND, New IntPtr(id), IntPtr.Zero)
                        Return True
                    End If
                Next
                PostMessage(h, WM_CLOSE, IntPtr.Zero, IntPtr.Zero)
                Return True
            End Function, IntPtr.Zero)
        'okna programu otwarte modalnie (ShowDialog) z badanego okna
        For Each f As Form In New List(Of Form)(Application.OpenForms.Cast(Of Form)())
            If f.Modal AndAlso f.Name <> _biezaceOkno Then
                _komunikaty.Add($"[{_biezaceOkno}] {_biezacaAkcja} -> otwarte okno programu {f.GetType().Name} '{f.Text}' (zamknięte)")
                Try
                    f.DialogResult = DialogResult.Cancel
                    f.Close()
                Catch ex As Exception
                    Zapisz("WYJĄTEK przy zamykaniu " & f.GetType().Name, ex)
                End Try
            End If
        Next
    End Sub
#End Region

    Private Sub Zapisz(opis As String, ex As Exception)
        Dim e = If(TypeOf ex Is Reflection.TargetInvocationException AndAlso ex.InnerException IsNot Nothing, ex.InnerException, ex)
        Dim stos = If(e.StackTrace, "").Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim miejsce = stos.FirstOrDefault(Function(s) s.Contains("MapoTero") AndAlso Not s.Contains("AudytKliknij"))
        _raport.AppendLine($"WYJĄTEK [{_biezaceOkno}] {opis}: {e.GetType().Name}: {e.Message}")
        If miejsce IsNot Nothing Then _raport.AppendLine("    " & miejsce.Trim())
    End Sub

    ''' <summary>Pompuje komunikaty okna przez podany czas - uruchamia kontynuacje Async Sub; ich wyjątki przypisuje bieżącej akcji.</summary>
    Private Sub Pompuj(ms As Integer)
        Dim sw = Stopwatch.StartNew()
        Do
            Try
                Application.DoEvents()
            Catch ex As Exception
                Zapisz(_biezacaAkcja & " (wyjątek opóźniony, np. Async Sub)", ex)
            End Try
            Thread.Sleep(10)
        Loop While sw.ElapsedMilliseconds < ms
    End Sub

    Private Shared Function Opis(c As Object) As String
        If TypeOf c Is ToolStripItem Then
            Dim t = DirectCast(c, ToolStripItem)
            Return $"menu {t.Name} '{t.Text}'"
        End If
        Dim k = DirectCast(c, Control)
        Return $"{k.GetType().Name} {k.Name} '{k.Text}'"
    End Function

    Private Shared Function Pominac(nazwa As String, tekst As String) As Boolean
        Return Pomijane.IsMatch(If(nazwa, "")) OrElse Pomijane.IsMatch(If(tekst, "").Trim())
    End Function

    ''' <summary>Wszystkie kontrolki okna; przy TabControl wybiera kolejno każdą kartę, żeby jej przyciski były widoczne.</summary>
    Private Iterator Function Kontrolki(rodzic As Control) As IEnumerable(Of Control)
        For Each c As Control In rodzic.Controls
            Yield c
            If TypeOf c Is TabControl Then
                Dim tc = DirectCast(c, TabControl)
                For Each strona As TabPage In tc.TabPages
                    Try
                        tc.SelectedTab = strona
                    Catch ex As Exception
                        Zapisz("wybór karty " & strona.Text, ex)
                    End Try
                    For Each d In Kontrolki(strona)
                        Yield d
                    Next
                Next
            ElseIf c.HasChildren Then
                For Each d In Kontrolki(c)
                    Yield d
                Next
            End If
        Next
    End Function

    Private Iterator Function PozycjeMenu(elementy As ToolStripItemCollection) As IEnumerable(Of ToolStripItem)
        For Each it As ToolStripItem In elementy
            Yield it
            If TypeOf it Is ToolStripDropDownItem Then
                For Each d In PozycjeMenu(DirectCast(it, ToolStripDropDownItem).DropDownItems)
                    Yield d
                Next
            End If
        Next
    End Function

    Private Sub UstawDane(okno As Form, zestaw As String, nr As Integer)
        For Each c In Kontrolki(okno).ToList()
            If Not c.Enabled Then Continue For
            _biezacaAkcja = $"wpis '{zestaw}' w {Opis(c)}"
            Try
                If TypeOf c Is TextBox AndAlso Not DirectCast(c, TextBox).ReadOnly Then
                    c.Text = zestaw
                ElseIf TypeOf c Is NumericUpDown Then
                    Dim n = DirectCast(c, NumericUpDown)
                    n.Value = If(nr Mod 2 = 0, n.Minimum, n.Maximum)
                ElseIf TypeOf c Is ComboBox Then
                    Dim cb = DirectCast(c, ComboBox)
                    If cb.Items.Count > 0 Then cb.SelectedIndex = nr Mod cb.Items.Count
                ElseIf TypeOf c Is CheckBox Then
                    DirectCast(c, CheckBox).Checked = (nr Mod 2 = 1)
                ElseIf TypeOf c Is RadioButton AndAlso nr Mod 3 = 1 Then
                    DirectCast(c, RadioButton).Checked = True
                End If
            Catch ex As Exception
                Zapisz(_biezacaAkcja, ex)
            End Try
        Next
        Pompuj(50)
    End Sub

    Private Sub Kliknij(okno As Form, element As Object, kliknij As Action)
        _biezacaAkcja = "klik " & Opis(element)
        Dim sw = Stopwatch.StartNew()
        Try
            kliknij()
        Catch ex As Exception
            Zapisz(_biezacaAkcja, ex)
        End Try
        sw.Stop()
        If sw.ElapsedMilliseconds > 2000 Then
            _raport.AppendLine($"WOLNO [{_biezaceOkno}] {_biezacaAkcja}: {sw.ElapsedMilliseconds} ms (okno zablokowane)")
        End If
        Pompuj(120)
    End Sub

    Private Sub ZbadajOkno(nazwa As String, utworz As Func(Of Form))
        _biezaceOkno = nazwa
        _biezacaAkcja = "otwarcie okna"
        Dim okno As Form = Nothing
        Dim sw = Stopwatch.StartNew()
        Try
            okno = utworz()
            okno.Name = nazwa
            okno.Show()
            Pompuj(300)
        Catch ex As Exception
            Zapisz("otwarcie okna", ex)
            Return
        End Try
        _raport.AppendLine($"--- {nazwa}: otwarte w {sw.ElapsedMilliseconds} ms")

        Dim klikniete As New HashSet(Of String)(), pominiete As New HashSet(Of String)(), nieaktywne As New HashSet(Of String)()
        For nr = 0 To ZestawyDanych.Length - 1
            If okno.IsDisposed Then Exit For
            If ZestawyDanych(nr) IsNot Nothing Then UstawDane(okno, ZestawyDanych(nr), nr)

            For Each c In Kontrolki(okno).ToList()
                If okno.IsDisposed Then Exit For
                If Not (TypeOf c Is Button OrElse TypeOf c Is LinkLabel) Then Continue For
                Dim o = Opis(c)
                If Pominac(c.Name, c.Text) Then pominiete.Add(o) : Continue For
                If Not c.Enabled OrElse Not c.Visible Then nieaktywne.Add(o) : Continue For
                klikniete.Add(o)
                If TypeOf c Is Button Then
                    Kliknij(okno, c, Sub() DirectCast(c, Button).PerformClick())
                Else
                    Kliknij(okno, c, Sub() GetType(LinkLabel).GetMethod("OnLinkClicked", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).
                                        Invoke(c, {New LinkLabelLinkClickedEventArgs(DirectCast(c, LinkLabel).Links.Cast(Of LinkLabel.Link)().FirstOrDefault())}))
                End If
            Next

            'menu okna i menu kontekstowe kontrolek - tylko przy danych domyślnych i pierwszym zestawie skrajnym
            If nr > 1 OrElse okno.IsDisposed Then Continue For
            Dim menu As New List(Of ToolStripItem)()
            For Each c In Kontrolki(okno).ToList()
                If TypeOf c Is ToolStrip Then menu.AddRange(PozycjeMenu(DirectCast(c, ToolStrip).Items))
                If c.ContextMenuStrip IsNot Nothing Then menu.AddRange(PozycjeMenu(c.ContextMenuStrip.Items))
            Next
            For Each it In menu
                If okno.IsDisposed Then Exit For
                If TypeOf it Is ToolStripSeparator Then Continue For
                If TypeOf it Is ToolStripDropDownItem AndAlso DirectCast(it, ToolStripDropDownItem).DropDownItems.Count > 0 Then Continue For
                Dim o = Opis(it)
                If Pominac(it.Name, it.Text) Then pominiete.Add(o) : Continue For
                If Not it.Enabled Then nieaktywne.Add(o) : Continue For
                klikniete.Add(o)
                Kliknij(okno, it, Sub() it.PerformClick())
            Next
        Next

        _biezacaAkcja = "zamknięcie okna"
        Try
            If Not okno.IsDisposed Then okno.Close() : okno.Dispose()
        Catch ex As Exception
            Zapisz("zamknięcie okna", ex)
        End Try
        Pompuj(100)
        'okna otwarte niemodalnie przez badane okno
        For Each f As Form In New List(Of Form)(Application.OpenForms.Cast(Of Form)())
            If f.Name <> "Form1" Then
                _raport.AppendLine($"    pozostało otwarte okno {f.GetType().Name} '{f.Text}'")
                Try : f.Close() : Catch : End Try
            End If
        Next
        _raport.AppendLine($"    kliknięte: {klikniete.Count}, nieaktywne: {nieaktywne.Count}, pominięte: {String.Join("; ", pominiete)}")
        If nieaktywne.Count > 0 Then _raport.AppendLine($"    nieaktywne: {String.Join("; ", nieaktywne)}")
    End Sub

    <FaktAudytu>
    Public Sub KliknijWszystkieOkna()
        Przebieg("klik.txt")
    End Sub

    ''' <summary>
    ''' Ten sam przebieg z sesją testową (conf.txt i 2 x 2 segmenty PNG w download\audyt_sesja) - okno główne wczytuje ją
    ''' przy starcie, więc w oknach scalania, eksportu i CAD aktywne są przyciski wymagające pobranych segmentów.
    ''' </summary>
    <FaktAudytu>
    Public Sub KliknijWszystkieOknaZSesja()
        Dim baza = AppContext.BaseDirectory.TrimEnd("\"c)
        Dim folder = Path.Combine(baza, "download", "audyt_sesja")
        If Directory.Exists(folder) Then Directory.Delete(folder, True)
        Directory.CreateDirectory(folder)
        Dim lastsettings = Path.Combine(baza, "lastsettings.txt")
        If File.Exists(lastsettings) Then File.Delete(lastsettings)
        Dim c As New KonfiguracjaSesji With {
            .Folder = folder & "\", .RodzajMapy = "Testowa", .XDol = "487000", .YLewy = "637000", .XGora = "487512", .YPrawy = "637512",
            .RozmiarPiksela = "1", .BokSegmentuPx = "256", .UkladEpsg = 2180, .Format = "png", .Prefiks = "_", .Numeracja = "1", .Przezroczystosc = True}
        c.Warstwy(0) = "W1"
        c.LiczbaWarstw = 1
        c.Zapisz(Path.Combine(folder, "conf.txt"), Encoding.UTF8)
        For w = 1 To 2
            For k = 1 To 2
                Using bmp As New Drawing.Bitmap(256, 256)
                    Using g = Drawing.Graphics.FromImage(bmp)
                        g.Clear(If((w + k) Mod 2 = 0, Drawing.Color.ForestGreen, Drawing.Color.SteelBlue))
                    End Using
                    bmp.Save(Path.Combine(folder, "_" & w.ToString("D2") & "_" & k.ToString("D2") & ".png"), Drawing.Imaging.ImageFormat.Png)
                End Using
            Next
        Next
        Try
            Przebieg("klik_sesja.txt")
        Finally
            Try : Directory.Delete(folder, True) : Catch : End Try
            If File.Exists(lastsettings) Then File.Delete(lastsettings)
        End Try
    End Sub

    Private Sub Przebieg(plikRaportu As String)
        Dim wyjatek As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException)
                    Dim straz As New System.Windows.Forms.Timer() With {.Interval = 100}
                    AddHandler straz.Tick, Sub() ZamknijDialogi()
                    straz.Start()

                    Using glowne As New Global.MapoTero.Form1()
                        _biezaceOkno = "Form1"
                        glowne.Name = "Form1"
                        glowne.Show()
                        Pompuj(1500)
                        ZbadajOkno("Form2", Function() New Global.MapoTero.Form2())
                        ZbadajOkno("Form3", Function() New Global.MapoTero.Form3())
                        ZbadajOkno("FormEksport", Function() New Global.MapoTero.FormEksport())
                        ZbadajOkno("FormEksportCad", Function() New Global.MapoTero.FormEksportCad())
                        ZbadajOkno("FormZaawansowanyMbtiles", Function() New Global.MapoTero.FormZaawansowanyMbtiles())
                        ZbadajOkno("FormPomocMbtiles", Function() New Global.MapoTero.FormPomocMbtiles())
                        ZbadajOkno("FormDodajSerwer", Function() New Global.MapoTero.FormDodajSerwer())
                        ZbadajOkno("FormImportOzi", Function() New Global.MapoTero.FormImportOzi())
                        ZbadajOkno("FormKorektaFolderu", Function() New Global.MapoTero.FormKorektaFolderu())
                        ZbadajOkno("FormKorytarz", Function() New Global.MapoTero.FormKorytarz(New Korytarz()))
                        ZbadajOkno("FormProfilNmt", Function() New Global.MapoTero.FormProfilNmt())
                        ZbadajOkno("FormPodgladMip", Function() New Global.MapoTero.FormPodgladMip(New OpcjeKorekcjiMip()))
                        ZbadajOkno("FormSzukajDzialki", Function() New Global.MapoTero.FormSzukajDzialki())
                        ZbadajOkno("AboutBox1", Function() New Global.MapoTero.AboutBox1())
                        ZbadajOkno("Instrukcja_Obslugi", Function() New Global.MapoTero.Instrukcja_Obslugi())
                        'okno główne na końcu - jego polecenia otwierają okna narzędzi (zamykane przez strażnika)
                        _raport.AppendLine("=== Okno główne")
                        ZbadajOknoGlowne(glowne)
                    End Using
                    straz.Stop()
                Catch ex As Exception
                    wyjatek = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()

        Dim folder = Path.Combine(Path.GetTempPath(), "audyt")
        Directory.CreateDirectory(folder)
        Dim tekst = _raport.ToString() & vbCrLf & "=== Komunikaty" & vbCrLf & String.Join(vbCrLf, _komunikaty)
        If wyjatek IsNot Nothing Then tekst &= vbCrLf & "PRZERWANE: " & wyjatek.ToString()
        File.WriteAllText(Path.Combine(folder, plikRaportu), tekst, Encoding.UTF8)
        _wyjscie.WriteLine(tekst)
    End Sub

    ''' <summary>Okno główne bada się bez tworzenia nowego (jest już otwarte) - te same kroki co ZbadajOkno.</summary>
    Private Sub ZbadajOknoGlowne(glowne As Form)
        ZbadajOkno("Form1", Function() glowne)
    End Sub

End Class

''' <summary>
''' Test audytu uruchamiany tylko na żądanie (zmienna środowiskowa MAPOTERO_AUDYT=1) - otwiera wszystkie okna programu
''' i trwa kilka minut, więc nie może działać w zwykłym „dotnet test” ani w GitHub Actions.
''' </summary>
Public NotInheritable Class FaktAudytuAttribute
    Inherits FactAttribute
    Public Sub New()
        If Environment.GetEnvironmentVariable("MAPOTERO_AUDYT") <> "1" Then
            Skip = "Test audytu - uruchom z MAPOTERO_AUDYT=1"
        End If
    End Sub
End Class
