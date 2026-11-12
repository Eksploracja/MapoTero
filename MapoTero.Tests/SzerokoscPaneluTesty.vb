'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>Regulowana szerokość prawego panelu okna głównego (pasek splPanel).</summary>
Public Class SzerokoscPaneluTesty

    ''' <summary>
    ''' Przeciągnięcie paska w zmaksymalizowanym oknie poszerza panel i zapamiętuje szerokość; w najmniejszym oknie
    ''' panel się zwęża, a po ponownej maksymalizacji wraca do wybranej szerokości. (Błąd: obsługa zmiany rozmiaru
    ''' panelu przywracała poprzednią szerokość: Splitter zgłasza MouseUp przed zmianą szerokości, a formularz
    ''' z AutoScroll pokazywał po poszerzeniu panelu poziomy pasek przewijania i jego Resize cofał szerokość.)
    ''' </summary>
    <Fact>
    Public Sub PrzeciagnieciePaskaPoszerzaPanel()
        Dim blad As Exception = Nothing
        Dim t As New Thread(
            Sub()
                Try
                    Using f As New MapoTero.Form1()
                        f.Show()
                        Application.DoEvents()
                        Dim spl = DirectCast(f.Controls("splPanel"), Splitter)
                        Dim panel = f.Controls("Panel2")
                        Dim najmniejsza As Integer = panel.Width
                        Dim lewaIkonek As Integer = panel.Controls("btnOtworzFolder").Left
                        f.WindowState = FormWindowState.Maximized
                        Application.DoEvents()
                        'ekran za wąski, żeby poszerzyć panel o 100 px - nie ma czego sprawdzać
                        If spl.MinExtra + spl.Width + najmniejsza + 100 > f.ClientSize.Width Then Exit Sub

                        Dim zdarzenie = Sub(nazwa As String, x As Integer)
                                            GetType(Splitter).GetMethod(nazwa, BindingFlags.Instance Or BindingFlags.NonPublic).
                                                Invoke(spl, {New MouseEventArgs(MouseButtons.Left, 1, x, 200, 0)})
                                        End Sub
                        zdarzenie("OnMouseDown", 2)
                        zdarzenie("OnMouseMove", -98)
                        zdarzenie("OnMouseUp", -98)
                        Application.DoEvents()
                        Dim poszerzona As Integer = panel.Width
                        Assert.InRange(poszerzona, najmniejsza + 90, najmniejsza + 110)
                        Assert.True(MapoTero.Ustawienia.SzerokoscPanelu >= najmniejsza + 90)
                        'małe przyciski pod panelem trzymają się lewej krawędzi (były zakotwiczone do prawej i zostawały na środku)
                        Assert.Equal(lewaIkonek, panel.Controls("btnOtworzFolder").Left)

                        f.WindowState = FormWindowState.Normal
                        f.Size = f.MinimumSize
                        Application.DoEvents()
                        Assert.Equal(najmniejsza, panel.Width)

                        f.WindowState = FormWindowState.Maximized
                        Application.DoEvents()
                        Assert.Equal(poszerzona, panel.Width)
                        MapoTero.Ustawienia.SzerokoscPanelu = 0
                    End Using
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If blad IsNot Nothing Then Throw New Exception(blad.Message, blad)
    End Sub

End Class
