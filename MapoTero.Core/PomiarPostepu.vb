'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Diagnostics

''' <summary>
''' Szacowanie pozostałego czasu pobierania z rzeczywistej prędkości: przez pierwsze sekundy tylko „szacowanie czasu…”,
''' potem prędkość z segmentów pobranych od pierwszego odczytu (pomija segmenty już istniejące przy ponowieniu).
''' </summary>
Public Class PomiarPostepu
    Private ReadOnly _wszystkie As Long
    Private ReadOnly _zegar As Func(Of Double)
    Private _startSek As Double = -1
    Private _startPobrane As Long

    Public Sub New(wszystkie As Long)
        Me.New(wszystkie, Nothing)
    End Sub

    ''' <param name="zegar">Sekundy od początku pomiaru (Nothing - zegar systemowy; testy podają własny).</param>
    Public Sub New(wszystkie As Long, zegar As Func(Of Double))
        _wszystkie = Math.Max(1, wszystkie)
        If zegar Is Nothing Then
            Dim stoper = Stopwatch.StartNew()
            zegar = Function() stoper.Elapsed.TotalSeconds
        End If
        _zegar = zegar
    End Sub

    Public ReadOnly Property Uplynelo As Double
        Get
            Return _zegar()
        End Get
    End Property

    ''' <summary>Dopisek do opisu postępu, np. „ · pozostało ok. 2 min 10 s (4,3 segm./s)”.</summary>
    Public Function Opis(pobrane As Long) As String
        Dim teraz = _zegar()
        If _startSek < 0 Then
            _startSek = teraz
            _startPobrane = pobrane
        End If
        Dim trwanie = teraz - _startSek
        Dim przyrost = pobrane - _startPobrane
        If trwanie < 3 OrElse przyrost < 2 Then Return " · szacowanie czasu..."
        Dim predkosc = przyrost / trwanie
        Dim pozostalo = Math.Max(0, _wszystkie - pobrane) / predkosc
        Return $" · pozostało ok. {Czas(pozostalo)} ({predkosc:0.0} segm./s, łącznie {_wszystkie:N0} segm.)"
    End Function

    ''' <summary>Dopisek dla etapu z postępem w procentach (zapis pliku).</summary>
    Public Shared Function OpisZProcentu(procent As Integer, uplynelo As Double) As String
        If procent < 3 OrElse uplynelo < 2 Then Return "..."
        Return " · pozostało ok. " & Czas(uplynelo * (100 - procent) / procent)
    End Function

    Public Shared Function Czas(sekundy As Double) As String
        Return New SzacunekKreatora With {.SzacowanyCzasSekundy = CInt(Math.Min(Integer.MaxValue \ 2, Math.Ceiling(Math.Max(0, sekundy))))}.SformatowanyCzas()
    End Function
End Class
