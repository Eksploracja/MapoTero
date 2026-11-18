'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.IO
Imports System.Net.Http
Imports System.Runtime.InteropServices

''' <summary>
''' Opis błędu dla użytkownika. .NET podaje treść typowych wyjątków po angielsku („Access to the path … is denied.”),
''' więc komunikaty programu („Nie udało się zapisać ustawień: ” &amp; ex.Message) mieszały dwa języki i nie mówiły,
''' co zrobić. Dla typowych błędów plików, sieci i obrazów - opis po polsku ze wskazówką; dla pozostałych - treść wyjątku.
''' </summary>
Public NotInheritable Class OpisBledu

    Private Sub New()
    End Sub

    'kody HRESULT błędów Windows zgłaszanych jako IOException
    Private Const PlikUzywany As Integer = &H80070020        'ERROR_SHARING_VIOLATION
    Private Const PlikZablokowany As Integer = &H80070021    'ERROR_LOCK_VIOLATION
    Private Const DyskPelny As Integer = &H80070070          'ERROR_DISK_FULL
    Private Const DyskPelnyHandle As Integer = &H80070027    'ERROR_HANDLE_DISK_FULL
    Private Const UrzadzenieNieGotowe As Integer = &H80070015 'ERROR_NOT_READY (np. wyjęty pendrive)

    ''' <summary>Opis błędu po polsku (bez kropki na końcu, gdy pochodzi z programu).</summary>
    Public Shared Function Z(ex As Exception) As String
        If ex Is Nothing Then Return "nieznany błąd"
        If TypeOf ex Is AggregateException AndAlso ex.InnerException IsNot Nothing Then Return Z(ex.InnerException)

        If TypeOf ex Is UnauthorizedAccessException Then
            Return "brak uprawnień do pliku lub folderu (plik tylko do odczytu albo folder chroniony, np. Program Files)" & Sciezka(ex.Message)
        End If
        If TypeOf ex Is PathTooLongException Then
            Return "ścieżka pliku jest za długa - wybierz folder o krótszej ścieżce"
        End If
        If TypeOf ex Is FileNotFoundException Then
            Dim f = DirectCast(ex, FileNotFoundException).FileName
            Return "plik nie istnieje" & If(String.IsNullOrEmpty(f), Sciezka(ex.Message), ": " & f)
        End If
        If TypeOf ex Is DirectoryNotFoundException Then
            Return "folder nie istnieje" & Sciezka(ex.Message)
        End If
        If TypeOf ex Is DriveNotFoundException Then
            Return "dysk nie istnieje lub jest niedostępny" & Sciezka(ex.Message)
        End If
        If TypeOf ex Is IOException Then
            Select Case ex.HResult
                Case PlikUzywany, PlikZablokowany
                    Return "plik jest otwarty w innym programie - zamknij go i spróbuj ponownie" & Sciezka(ex.Message)
                Case DyskPelny, DyskPelnyHandle
                    Return "brak miejsca na dysku"
                Case UrzadzenieNieGotowe
                    Return "dysk jest niedostępny (np. odłączony dysk zewnętrzny)"
            End Select
        End If
        If TypeOf ex Is TimeoutException OrElse
           (TypeOf ex Is TaskCanceledException AndAlso TypeOf ex.InnerException Is TimeoutException) Then
            Return "serwer nie odpowiedział w wyznaczonym czasie - spróbuj ponownie lub wydłuż limit czasu w Ustawieniach"
        End If
        If TypeOf ex Is HttpRequestException Then
            Dim h = DirectCast(ex, HttpRequestException)
            If h.StatusCode.HasValue Then
                Return "serwer zwrócił błąd HTTP " & CInt(h.StatusCode.Value) & " (" & h.StatusCode.Value.ToString() & ")"
            End If
            'brak sieci, nieznany host - treść pochodzi z Windows (po polsku)
            Return "błąd połączenia z serwerem: " & If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
        End If
        If TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is InsufficientMemoryException Then
            Return "za mało pamięci - zmniejsz obszar lub rozmiar arkusza, zamknij inne programy"
        End If
        If TypeOf ex Is ExternalException AndAlso ex.Message.Contains("GDI+") Then
            Return "błąd odczytu lub zapisu obrazu (uszkodzony plik, brak miejsca na dysku albo obraz za duży)"
        End If
        If TypeOf ex Is ArgumentException AndAlso ex.Message = "Parameter is not valid." Then
            Return "plik nie jest poprawnym obrazem lub obraz jest za duży"
        End If
        Return ex.Message
    End Function

    ''' <summary>Ścieżka z angielskiej treści wyjątku (w apostrofach), np. „Access to the path 'C:\x' is denied.” → „: C:\x”.</summary>
    Private Shared Function Sciezka(tresc As String) As String
        If String.IsNullOrEmpty(tresc) Then Return ""
        Dim a = tresc.IndexOf("'"c)
        If a < 0 Then Return ""
        Dim b = tresc.IndexOf("'"c, a + 1)
        If b <= a + 1 Then Return ""
        Return ": " & tresc.Substring(a + 1, b - a - 1)
    End Function

End Class
