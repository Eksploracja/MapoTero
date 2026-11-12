'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Globalization
Imports System.Linq
Imports System.Text

''' <summary>
''' Wyszukiwanie warstw w drzewie zbiorów map. Wielkość liter, polskie znaki i podkreślenia nie mają znaczenia
''' („dzialki” znajduje „działki”, „mapy historyczne” - zbiór mapy_historyczne_IHPAN), a każde słowo frazy musi
''' wystąpić w nazwie warstwy albo w nazwie jej zbioru (np. „orto wmts” - warstwy zbioru ortofotomapa_WMTS).
''' </summary>
Public Module FiltrWarstw

    ''' <summary>Tekst do porównań: małe litery bez polskich znaków, podkreślenia i myślniki jako spacje.</summary>
    Public Function Uprosc(tekst As String) As String
        If String.IsNullOrEmpty(tekst) Then Return ""
        Dim rozlozony As String = tekst.ToLowerInvariant().Replace("ł"c, "l"c).Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(rozlozony.Length)
        For Each c In rozlozony
            If CharUnicodeInfo.GetUnicodeCategory(c) = UnicodeCategory.NonSpacingMark Then Continue For
            sb.Append(If(c = "_"c OrElse c = "-"c, " "c, c))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Słowa frazy wyszukiwania (puste - fraza nic nie ogranicza).</summary>
    Public Function Slowa(fraza As String) As String()
        Return Uprosc(fraza).Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
    End Function

    ''' <summary>Czy warstwa pasuje do frazy (każde słowo w nazwie warstwy lub zbioru).</summary>
    Public Function Pasuje(nazwaZbioru As String, nazwaWarstwy As String, fraza As String) As Boolean
        Dim tekst As String = Uprosc(nazwaZbioru) & " " & Uprosc(nazwaWarstwy)
        Return Slowa(fraza).All(Function(s) tekst.Contains(s))
    End Function

End Module
