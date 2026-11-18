'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>Układ menu Pomoc i przycisk wsparcia w oknie O programie.</summary>
Public Class MenuPomocTesty

    Private Shared Sub WWatkuSta(akcja As Action)
        Dim blad As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    akcja()
                                Catch ex As Exception
                                    blad = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw New Exception(blad.Message, blad)
    End Sub

    ''' <summary>
    ''' Kolejność pozycji menu Pomoc; tylko podręcznik ma skrót (F1). Dawniej F2 i F3 otwierały instrukcję i forum,
    ''' a w Windows służą do zmiany nazwy i wyszukiwania.
    ''' </summary>
    <Fact>
    Public Sub MenuPomoc_KolejnoscPozycjiITylkoF1()
        WWatkuSta(Sub()
                      Using f As New MapoTero.Form1()
                          Dim pozycje = f.HelpToolStripMenuItem.DropDownItems.Cast(Of ToolStripItem)().
                              Select(Function(p) If(TypeOf p Is ToolStripSeparator, "-", p.Text)).ToArray()
                          Assert.Equal({"Podręcznik użytkownika (online)", "Krótka instrukcja (bez internetu)", "-",
                                        "Zadaj pytanie na forum (PFE)", "Zgłoś błąd lub pomysł (GitHub)",
                                        "Otwórz folder danych programu", "-",
                                        "Co nowego w programie", "Wesprzyj rozwój programu", "O programie"}, pozycje)
                          Dim skroty = f.HelpToolStripMenuItem.DropDownItems.OfType(Of ToolStripMenuItem)().
                              Where(Function(p) p.ShortcutKeys <> Keys.None).Select(Function(p) p.ShortcutKeys).ToArray()
                          Assert.Equal({Keys.F1}, skroty)
                      End Using
                  End Sub)
    End Sub

    ''' <summary>Adres wsparcia prowadzi do strony autora przez HTTPS.</summary>
    <Fact>
    Public Sub AdresWsparcia_Https()
        Assert.Equal("https://buycoffee.to/mapy", MapoTero.AdresWsparcia)
    End Sub

    ''' <summary>Okno O programie ma jeden przycisk „Wesprzyj rozwój programu” w obrębie okna, obok przycisku OK.</summary>
    <Fact>
    Public Sub OknoOProgramie_PrzyciskWsparciaObokOk()
        WWatkuSta(Sub()
                      Using f As New MapoTero.AboutBox1()
                          f.Show()
                          Application.DoEvents()
                          Dim btn = f.Controls.Find("btnWesprzyj", True)
                          Assert.Single(btn)
                          Dim r = f.RectangleToClient(btn(0).Parent.RectangleToScreen(btn(0).Bounds))
                          Assert.True(f.ClientRectangle.Contains(r), "przycisk poza oknem: " & r.ToString())
                          Dim ok = f.RectangleToClient(f.OKButton.Parent.RectangleToScreen(f.OKButton.Bounds))
                          Assert.True(r.Right <= ok.Left, "przycisk zachodzi na OK")
                          Assert.Equal(ok.Top, r.Top)
                      End Using
                  End Sub)
    End Sub

End Class
