'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Runtime.CompilerServices

''' <summary>
''' Pilnuje, żeby nowo otwarte okno mieściło się na ekranie. Okna wyśrodkowane nad oknem głównym (np. szybki kreator)
''' wystawały na niskim ekranie poza dolną krawędź, a przyciski na dole były niedostępne. Każde okno jest sprawdzane raz,
''' przy pierwszym pokazaniu - potem użytkownik może je przesuwać dowolnie.
''' Sprawdzenie działa w Application.Idle, które WinForms zgłasza także w pętli okien modalnych (ShowDialog), więc
''' obejmuje wszystkie okna programu bez zmian w każdym z nich.
''' </summary>
Friend Module OknaNaEkranie

    Private ReadOnly Sprawdzone As New ConditionalWeakTable(Of Form, Object)
    Private _zarejestrowane As Boolean

    Public Sub Zarejestruj()
        If _zarejestrowane Then Return
        _zarejestrowane = True
        AddHandler Application.Idle, AddressOf SprawdzOkna
    End Sub

    Private Sub SprawdzOkna(sender As Object, e As EventArgs)
        For Each okno As Form In Application.OpenForms.Cast(Of Form)().ToList()
            If Not okno.Visible OrElse okno.TopLevel = False OrElse okno.WindowState <> FormWindowState.Normal Then Continue For
            Dim znacznik As Object = Nothing
            If Sprawdzone.TryGetValue(okno, znacznik) Then Continue For
            Sprawdzone.Add(okno, Nothing)
            okno.Bounds = DopasujDoEkranu(okno.Bounds, Screen.FromControl(okno).WorkingArea,
                                          okno.FormBorderStyle = FormBorderStyle.Sizable OrElse
                                          okno.FormBorderStyle = FormBorderStyle.SizableToolWindow)
        Next
    End Sub

    ''' <summary>
    ''' Przesuwa prostokąt okna do obszaru roboczego ekranu (bez paska zadań); okno wyższe lub szersze od ekranu
    ''' zmniejsza, gdy da się zmieniać jego rozmiar, a w przeciwnym razie ustawia jego górną krawędź na górze ekranu,
    ''' żeby pasek tytułu pozostał dostępny.
    ''' </summary>
    Friend Function DopasujDoEkranu(okno As Rectangle, ekran As Rectangle, moznaZmniejszyc As Boolean) As Rectangle
        Dim r = okno
        If moznaZmniejszyc Then
            r.Width = Math.Min(r.Width, ekran.Width)
            r.Height = Math.Min(r.Height, ekran.Height)
        End If
        If r.Bottom > ekran.Bottom Then r.Y = ekran.Bottom - r.Height
        If r.Right > ekran.Right Then r.X = ekran.Right - r.Width
        If r.Top < ekran.Top Then r.Y = ekran.Top
        If r.Left < ekran.Left Then r.X = ekran.Left
        Return r
    End Function
End Module
