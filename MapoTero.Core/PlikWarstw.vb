'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Collections.Generic
Imports System.IO
Imports System.Text

''' <summary>Warstwa mapy z pliku zbioru map: nazwa warstwy na serwerze i zalecany rozmiar piksela.</summary>
Public Class WarstwaMapy
    Public Property Nazwa As String = ""
    ''' <summary>Rozmiar piksela zapisany w pliku (tekst, np. "2" lub "0.25"); może być pusty.</summary>
    Public Property RozmiarPiksela As String = ""

    Public Overrides Function ToString() As String
        Return Nazwa
    End Function
End Class

''' <summary>
''' Plik zbioru map z katalogu "warstwy": pierwsze pole - adres serwera, dalej pary pól: nazwa warstwy, rozmiar piksela.
''' Pola mogą być w cudzysłowach i rozdzielone końcami wierszy lub przecinkami (format instrukcji Write/Input).
''' </summary>
Public Class PlikWarstw

    Public Property Adres As String = ""
    Public ReadOnly Property Warstwy As New List(Of WarstwaMapy)

    ''' <summary>Pliki warstw są zapisane w kodowaniu Windows-1250 (polskie znaki w nazwach warstw).</summary>
    Public Shared Function Kodowanie() As Encoding
        Try
            Return Encoding.GetEncoding(1250)
        Catch ex As Exception
            'platforma bez stron kodowych Windows - teksty ASCII odczytane zostaną poprawnie
            Return Encoding.UTF8
        End Try
    End Function

    Public Shared Function Wczytaj(sciezka As String) As PlikWarstw
        Return ZTekstu(File.ReadAllText(sciezka, Kodowanie()))
    End Function

    Public Shared Function ZTekstu(tekst As String) As PlikWarstw
        Dim p As New PlikWarstw()
        Dim r As New CzytnikPlikuVb(tekst)
        If r.KoniecPliku Then Return p
        p.Adres = r.Nastepne()
        While Not r.KoniecPliku
            Dim w As New WarstwaMapy With {.Nazwa = r.Nastepne()}
            If Not r.KoniecPliku Then w.RozmiarPiksela = r.Nastepne()
            If w.Nazwa <> "" Then p.Warstwy.Add(w)
        End While
        Return p
    End Function

    ''' <summary>Warstwa o podanej nazwie albo Nothing.</summary>
    Public Function Znajdz(nazwa As String) As WarstwaMapy
        For Each w In Warstwy
            If w.Nazwa = nazwa Then Return w
        Next
        Return Nothing
    End Function

End Class
