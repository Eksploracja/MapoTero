'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>
''' Audyt układu okien przy skalowaniu 100, 125 i 150 %. Skalowania Windows nie da się zmienić w teście, więc test
''' powiększa czcionki okna (okna mają AutoScaleMode.Font, więc położenie i rozmiar kontrolek rosną tak jak przy DPI)
''' i szuka: tekstów, które nie mieszczą się w kontrolce, kontrolek wychodzących poza obszar rodzica, nakładających się
''' kontrolek i okien większych od ekranu. Zrzuty okien: %TEMP%\audyt\uklad\, raport: %TEMP%\audyt\uklad.txt.
''' Uruchamianie osobno (czcionka domyślna programu musi być ustawiona przed pierwszym oknem w procesie):
''' MAPOTERO_AUDYT=1 i --filter "FullyQualifiedName~AudytUkladOkien".
''' </summary>
<Trait("Kategoria", "Audyt")>
Public Class AudytUkladOkienTesty

    Private ReadOnly _raport As New StringBuilder()
    Private _okno As String = ""
    Private _skala As Double

    Private Shared ReadOnly Skale As Double() = {1.0, 1.25, 1.5}

    'najmniejszy typowy ekran roboczy (bez paska zadań) w pikselach przy danym skalowaniu:
    '100 % - laptop 1366 x 768; 125 % i 150 % - ekran 1920 x 1080
    Private Shared Function Ekran(skala As Double) As Size
        Return If(skala = 1.0, New Size(1366, 728), New Size(1920, 1040))
    End Function

    Private Sub Zglos(tekst As String)
        _raport.AppendLine($"[{_okno} {_skala * 100:0}%] {tekst}")
    End Sub

    Private Shared Function Nazwa(c As Control) As String
        Dim t = If(c.Text, "").Replace(vbCr, " ").Replace(vbLf, " ")
        If t.Length > 40 Then t = t.Substring(0, 40) & "…"
        Return $"{c.GetType().Name} {c.Name} '{t}'"
    End Function

    ''' <summary>Kontrolki z jawnie ustawioną czcionką (inną niż rodzica) - przy zmianie czcionki okna jej nie dziedziczą.</summary>
    Private Shared Sub JawneCzcionki(rodzic As Control, wynik As List(Of Tuple(Of Control, Font)))
        For Each c As Control In rodzic.Controls
            If Not c.Font.Equals(rodzic.Font) Then wynik.Add(Tuple.Create(c, c.Font))
            JawneCzcionki(c, wynik)
        Next
    End Sub

    Private Sub SprawdzKontrolki(rodzic As Control)
        Dim widoczne = rodzic.Controls.Cast(Of Control)().Where(Function(c) c.Visible).ToList()
        Dim obszar = rodzic.ClientRectangle
        Dim przewijany = TypeOf rodzic Is ScrollableControl AndAlso DirectCast(rodzic, ScrollableControl).AutoScroll
        For Each c In widoczne
            'poza obszarem rodzica (o więcej niż 2 px), gdy rodzic nie ma paska przewijania
            If Not przewijany AndAlso Not TypeOf rodzic Is ToolStrip AndAlso c.Dock = DockStyle.None AndAlso
               (c.Right > obszar.Right + 2 OrElse c.Bottom > obszar.Bottom + 2 OrElse c.Left < -2 OrElse c.Top < -2) Then
                Zglos($"poza obszarem {Nazwa(rodzic)}: {Nazwa(c)} {c.Bounds} > {obszar.Size}")
            End If
            SprawdzTekst(c)
        Next
        'nakładanie się kontrolek tego samego rodzica (bez paneli i obrazów, które bywają tłem)
        Dim lista = widoczne.Where(Function(c) c.Dock = DockStyle.None AndAlso Not TypeOf c Is Panel AndAlso Not TypeOf c Is PictureBox AndAlso
                                                Not TypeOf c Is TabControl AndAlso c.Width > 0 AndAlso c.Height > 0).ToList()
        For i = 0 To lista.Count - 1
            For j = i + 1 To lista.Count - 1
                Dim r = Rectangle.Intersect(lista(i).Bounds, lista(j).Bounds)
                If r.Width > 3 AndAlso r.Height > 3 Then
                    Zglos($"nakładają się: {Nazwa(lista(i))} i {Nazwa(lista(j))} ({r.Width}x{r.Height} px)")
                End If
            Next
        Next
        For Each c In widoczne
            If TypeOf c Is TabControl Then
                Dim tc = DirectCast(c, TabControl)
                For Each s As TabPage In tc.TabPages
                    tc.SelectedTab = s
                    Application.DoEvents()
                    SprawdzKontrolki(s)
                Next
            ElseIf c.HasChildren AndAlso Not TypeOf c Is NumericUpDown Then
                SprawdzKontrolki(c)
            End If
        Next
    End Sub

    ''' <summary>Tekst etykiety, przycisku lub pola wyboru, który nie mieści się w kontrolce (z zawijaniem wierszy tam, gdzie kontrolka zawija).</summary>
    Private Sub SprawdzTekst(c As Control)
        Dim tekst = c.Text
        If String.IsNullOrWhiteSpace(tekst) Then Return
        Dim szer As Integer, wys As Integer, zawija As Boolean
        If TypeOf c Is Label Then
            Dim l = DirectCast(c, Label)
            If l.AutoSize Then Return
            szer = l.ClientSize.Width - l.Padding.Horizontal : wys = l.ClientSize.Height - l.Padding.Vertical : zawija = True
        ElseIf TypeOf c Is Button Then
            szer = c.Width - 8 : wys = c.Height - 4 : zawija = True
        ElseIf TypeOf c Is CheckBox OrElse TypeOf c Is RadioButton Then
            If DirectCast(c, ButtonBase).AutoSize Then Return
            szer = c.Width - 18 : wys = c.Height : zawija = True
        ElseIf TypeOf c Is GroupBox Then
            szer = c.Width - 14 : wys = c.Font.Height + 4 : zawija = False
        Else
            Return
        End If
        If szer <= 0 OrElse wys <= 0 Then Return
        Dim flagi = TextFormatFlags.NoPrefix Or If(zawija, TextFormatFlags.WordBreak, TextFormatFlags.SingleLine)
        Dim potrzebny = TextRenderer.MeasureText(tekst, c.Font, New Size(szer, Integer.MaxValue), flagi)
        'najdłuższe słowo musi mieścić się w szerokości (zawijanie nie dzieli słów)
        Dim najdluzsze = tekst.Split({" "c, vbLf(0), vbCr(0)}, StringSplitOptions.RemoveEmptyEntries).
                              Select(Function(s) TextRenderer.MeasureText(s, c.Font, Size.Empty, TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine).Width).DefaultIfEmpty(0).Max()
        If potrzebny.Height > wys + 2 OrElse najdluzsze > szer + 2 OrElse (Not zawija AndAlso potrzebny.Width > szer + 2) Then
            Zglos($"ucięty tekst: {Nazwa(c)} potrzeba {Math.Max(potrzebny.Width, najdluzsze)}x{potrzebny.Height}, jest {szer}x{wys}")
        End If
    End Sub

    Private Sub Zbadaj(nazwa As String, utworz As Func(Of Form))
        For Each skala In Skale
            _okno = nazwa : _skala = skala
            Dim f As Form = Nothing
            Try
                f = utworz()
                If skala <> 1.0 Then
                    'przy DPI rosną wszystkie czcionki, także ustawione jawnie w kontrolkach
                    Dim jawne As New List(Of Tuple(Of Control, Font))()
                    JawneCzcionki(f, jawne)
                    Dim przed = f.ClientSize
                    f.Font = New Font(f.Font.FontFamily, CSng(f.Font.Size * skala), f.Font.Style)
                    For Each j In jawne
                        j.Item1.Font = New Font(j.Item2.FontFamily, CSng(j.Item2.Size * skala), j.Item2.Style)
                    Next
                    'zmiana czcionki przed utworzeniem okna nie zawsze uruchamia AutoScale - wtedy układ skalowany jawnie
                    If f.ClientSize = przed Then f.Scale(New SizeF(CSng(skala), CSng(skala)))
                End If
                f.StartPosition = FormStartPosition.Manual
                f.Location = New Point(0, 0)
                f.Show()
                Application.DoEvents()
                Dim ekr = Ekran(skala)
                If f.Width > ekr.Width OrElse f.Height > ekr.Height Then
                    Zglos($"okno {f.Width}x{f.Height} większe od ekranu {ekr.Width}x{ekr.Height}")
                End If
                SprawdzKontrolki(f)
                Dim folder = Path.Combine(Path.GetTempPath(), "audyt", "uklad")
                Directory.CreateDirectory(folder)
                Using bmp As New Bitmap(f.Width, f.Height)
                    f.DrawToBitmap(bmp, New Rectangle(Point.Empty, f.Size))
                    bmp.Save(Path.Combine(folder, $"{nazwa}_{skala * 100:0}.png"), Imaging.ImageFormat.Png)
                End Using
                'okno o zmiennym rozmiarze bez rozmiaru minimalnego - zmniejszone do 60 %
                If skala = 1.0 AndAlso f.FormBorderStyle = FormBorderStyle.Sizable AndAlso f.MinimumSize.IsEmpty Then
                    _okno = nazwa & " zmniejszone do 60%"
                    f.Size = New Size(CInt(f.Width * 0.6), CInt(f.Height * 0.6))
                    Application.DoEvents()
                    SprawdzKontrolki(f)
                End If
            Catch ex As Exception
                Zglos("WYJĄTEK: " & ex.Message)
            Finally
                If f IsNot Nothing AndAlso Not f.IsDisposed Then f.Close() : f.Dispose()
            End Try
        Next
    End Sub

    <FaktAudytu>
    Public Sub UkladOkienPrzySkalowaniu()
        Dim wyjatek As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    'czcionka domyślna programu (ApplicationEvents.vb) - bez niej okna w teście są o ok. 20 % większe niż w programie
                    Try
                        Application.SetDefaultFont(New Font("Microsoft Sans Serif", 8.25!, FontStyle.Regular, GraphicsUnit.Point))
                    Catch ex As InvalidOperationException
                        _raport.AppendLine("UWAGA: nie ustawiono czcionki domyślnej programu (okno utworzone wcześniej w tym procesie) - wyniki zawyżone")
                    End Try
                    Using glowne As New Global.MapoTero.Form1()
                        glowne.Show()
                        Application.DoEvents()
                        glowne.Hide()
                        Zbadaj("Form2", Function() New Global.MapoTero.Form2())
                        Zbadaj("Form3", Function() New Global.MapoTero.Form3())
                        Zbadaj("FormEksport", Function() New Global.MapoTero.FormEksport())
                        Zbadaj("FormEksportCad", Function() New Global.MapoTero.FormEksportCad())
                        Zbadaj("FormZaawansowanyMbtiles", Function() New Global.MapoTero.FormZaawansowanyMbtiles())
                        Zbadaj("FormPomocMbtiles", Function() New Global.MapoTero.FormPomocMbtiles())
                        Zbadaj("FormDodajSerwer", Function() New Global.MapoTero.FormDodajSerwer())
                        Zbadaj("FormImportOzi", Function() New Global.MapoTero.FormImportOzi())
                        Zbadaj("FormKorektaFolderu", Function() New Global.MapoTero.FormKorektaFolderu())
                        Zbadaj("FormKorytarz", Function() New Global.MapoTero.FormKorytarz(New Korytarz()))
                        Zbadaj("FormPodgladMip", Function() New Global.MapoTero.FormPodgladMip(New OpcjeKorekcjiMip()))
                        Zbadaj("FormSzukajDzialki", Function() New Global.MapoTero.FormSzukajDzialki())
                        Zbadaj("Nakladanie_Map", Function() New Global.MapoTero.Nakladanie_Map())
                        Zbadaj("usuwanie_p_seg", Function() New Global.MapoTero.usuwanie_p_seg())
                        Zbadaj("AboutBox1", Function() New Global.MapoTero.AboutBox1())
                        Zbadaj("Instrukcja_Obslugi", Function() New Global.MapoTero.Instrukcja_Obslugi())
                        Zbadaj("pomoc_pfe", Function() New Global.MapoTero.pomoc_pfe())
                    End Using
                    Zbadaj("Form1", Function() New Global.MapoTero.Form1())
                Catch ex As Exception
                    wyjatek = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        Dim tekst = _raport.ToString()
        If wyjatek IsNot Nothing Then tekst &= vbCrLf & "PRZERWANE: " & wyjatek.ToString()
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "audyt", "uklad.txt"), tekst, Encoding.UTF8)
    End Sub

End Class
