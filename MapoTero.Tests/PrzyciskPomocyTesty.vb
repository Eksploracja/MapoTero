'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>Położenie przycisku „Pomoc” dodawanego w kodzie do okien narzędzi (PomocProgramu.DodajPrzyciskPomocy).</summary>
Public Class PrzyciskPomocyTesty

    ''' <summary>Okna z przyciskiem Pomoc; okno dodawania serwera w obu krokach (katalog usług i lista warstw).</summary>
    Private Shared Function Okna() As List(Of (Nazwa As String, Utworz As Func(Of Form)))
        Return New List(Of (String, Func(Of Form))) From {
            ("Form2 (Ustawienia)", Function() New MapoTero.Form2()),
            ("Form3 (złączanie segmentów)", Function() New MapoTero.Form3()),
            ("FormEksport", Function() New MapoTero.FormEksport()),
            ("FormEksportCad", Function() New MapoTero.FormEksportCad()),
            ("FormDodajSerwer - katalog", Function() New MapoTero.FormDodajSerwer()),
            ("FormDodajSerwer - warstwy", Function() New MapoTero.FormDodajSerwer("https://example.invalid/wms")),
            ("FormKorytarz", Function() New MapoTero.FormKorytarz(Nothing)),
            ("FormImportOzi", Function() New MapoTero.FormImportOzi()),
            ("FormSzukajDzialki - miejscowości", Function() New MapoTero.FormSzukajDzialki()),
            ("FormSzukajDzialki - działki", Function() New MapoTero.FormSzukajDzialki(1)),
            ("usuwanie_p_seg", Function() New MapoTero.usuwanie_p_seg()),
            ("FormKorektaFolderu", Function() New MapoTero.FormKorektaFolderu())}
    End Function

    ''' <summary>Widoczne kontrolki okna bez pojemników (panele, kontrolki użytkownika), z ramkami GroupBox.</summary>
    Private Shared Sub ZbierzKontrolki(rodzic As Control, wynik As List(Of Control))
        For Each c As Control In rodzic.Controls
            If Not c.Visible Then Continue For
            Dim pojemnik = TypeOf c Is Panel OrElse TypeOf c Is UserControl OrElse TypeOf c Is SplitContainer OrElse
                           TypeOf c Is TabControl OrElse TypeOf c Is TabPage
            If Not pojemnik Then wynik.Add(c)
            If pojemnik OrElse TypeOf c Is GroupBox Then ZbierzKontrolki(c, wynik)
        Next
    End Sub

    ''' <summary>
    ''' Przycisk Pomoc leży w obszarze okna i nie zasłania innych kontrolek. (Błąd: przycisk w lewym dolnym rogu
    ''' zasłaniał „Wpisz własny adres URL…” i „Wróć do listy usług” w kreatorze serwerów, „otwórz folder” w eksporcie
    ''' do CAD, „ręczne wprowadzanie parametrów” w złączaniu segmentów, pole progu kB w usuwaniu pustych segmentów
    ''' i pasek postępu w eksporcie mapy; w eksporcie mapy z przewijaniem na niskim ekranie wypadał poza okno.)
    ''' </summary>
    <Fact>
    Public Sub PrzyciskPomocyNieZaslaniaKontrolek()
        Dim bledy As New List(Of String)
        Dim t As New Thread(
            Sub()
                For Each okno In Okna()
                    Try
                        Using f = okno.Utworz()
                            f.StartPosition = FormStartPosition.Manual
                            f.Location = Point.Empty
                            f.Show()
                            Application.DoEvents()
                            Dim pomoc = f.Controls("btnPomoc")
                            If pomoc Is Nothing OrElse Not pomoc.Visible Then
                                bledy.Add($"{okno.Nazwa}: brak przycisku Pomoc")
                                Continue For
                            End If
                            'obszar zawartości okna - z przewijaniem (AutoScroll) większy niż widoczna część
                            Dim obszar As Rectangle = If(f.AutoScroll, f.DisplayRectangle, f.ClientRectangle)
                            If Not obszar.Contains(pomoc.Bounds) Then
                                bledy.Add($"{okno.Nazwa}: przycisk Pomoc {pomoc.Bounds} poza obszarem okna {obszar}")
                            End If
                            Dim rPomoc = f.RectangleToScreen(pomoc.Bounds)
                            Dim kontrolki As New List(Of Control)
                            ZbierzKontrolki(f, kontrolki)
                            For Each c In kontrolki
                                If c Is pomoc OrElse TypeOf c Is GroupBox AndAlso c.Bounds.Contains(pomoc.Bounds) Then Continue For
                                Dim r = c.Parent.RectangleToScreen(c.Bounds)
                                If r.IntersectsWith(rPomoc) Then
                                    bledy.Add($"{okno.Nazwa}: przycisk Pomoc zasłania {c.Name} '{c.Text}'")
                                End If
                            Next
                            f.Close()
                        End Using
                    Catch ex As Exception
                        bledy.Add($"{okno.Nazwa}: {ex.GetType().Name}: {ex.Message}")
                    End Try
                Next
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        Assert.True(bledy.Count = 0, String.Join(Environment.NewLine, bledy))
    End Sub

End Class
