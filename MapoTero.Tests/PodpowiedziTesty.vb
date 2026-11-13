'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

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

End Class
