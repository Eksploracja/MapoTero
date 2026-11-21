'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO

''' <summary>Podmienia gotowe pliki z katalogu roboczego na tym samym woluminie i cofa dokonane podmiany po błędzie.</summary>
Public NotInheritable Class WymianaPlikowMapy
    Private Sub New()
    End Sub

    Public Shared Sub Zatwierdz(pliki As IEnumerable(Of String), folderDocelowy As String, kopie As String,
                                 Optional usuwane As IEnumerable(Of String) = Nothing)
        Directory.CreateDirectory(kopie)
        Dim zakonczone As New List(Of (Cel As String, Kopia As String))
        Try
            For Each plik In pliki
                Dim nazwa = Path.GetFileName(plik)
                Dim cel = Path.Combine(folderDocelowy, nazwa)
                Dim kopia As String = Nothing
                If File.Exists(cel) Then
                    kopia = Path.Combine(kopie, nazwa)
                    File.Replace(plik, cel, kopia)
                Else
                    File.Move(plik, cel)
                End If
                zakonczone.Add((cel, kopia))
            Next
            For Each cel In If(usuwane, Enumerable.Empty(Of String)())
                Dim kopia = Path.Combine(kopie, Path.GetFileName(cel))
                File.Move(cel, kopia)
                zakonczone.Add((cel, kopia))
            Next
        Catch ex As Exception
            Dim bledyCofania As New List(Of Exception)
            For i = zakonczone.Count - 1 To 0 Step -1
                Dim p = zakonczone(i)
                Try
                    If p.Kopia Is Nothing Then
                        File.Delete(p.Cel)
                    Else
                        File.Move(p.Kopia, p.Cel, overwrite:=True)
                    End If
                Catch bladCofania As Exception
                    bledyCofania.Add(bladCofania)
                End Try
            Next
            If bledyCofania.Count > 0 Then
                bledyCofania.Insert(0, ex)
                Throw New AggregateException($"Nie udało się przywrócić wszystkich plików. Zachowane kopie są w folderze {kopie}.", bledyCofania)
            End If
            Throw
        End Try
    End Sub
End Class
