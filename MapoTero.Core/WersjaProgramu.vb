'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Reflection

''' <summary>
''' Formatowanie numeru wersji programu bez zbędnych końcowych zer
''' (np. 3.16.0.0 -&gt; „3.16”, 3.16.1.0 -&gt; „3.16.1”, 3.16.0.2 -&gt; „3.16.0.2”).
''' </summary>
Public NotInheritable Class WersjaProgramu

    Private Sub New()
    End Sub

    ''' <summary>Zwraca numer wersji bez końcowych składowych równych 0 ponad dwie pierwsze liczby (Major.Minor).</summary>
    Public Shared Function Formatuj(wersja As Version) As String
        If wersja Is Nothing Then Return ""
        If wersja.Revision > 0 Then Return wersja.ToString(4)
        If wersja.Build > 0 Then Return wersja.ToString(3)
        Return wersja.ToString(2)
    End Function

    ''' <summary>Zwraca sformatowaną wersję podanego zestawu.</summary>
    Public Shared Function Z(zestaw As Assembly) As String
        If zestaw Is Nothing Then Return ""
        Return Formatuj(zestaw.GetName().Version)
    End Function

    ''' <summary>Nagłówek User-Agent programu dla zapytań HTTP (np. „MapoTero/3.16”).</summary>
    Public Shared Function UserAgent(zestaw As Assembly) As String
        Dim v As String = Z(zestaw)
        Return If(String.IsNullOrEmpty(v), "MapoTero", "MapoTero/" & v)
    End Function

End Class
