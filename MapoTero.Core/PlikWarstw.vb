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
            Dim providerType = Type.GetType("System.Text.CodePagesEncodingProvider, System.Text.Encoding.CodePages")
            If providerType IsNot Nothing Then
                Dim prop = providerType.GetProperty("Instance")
                Dim instance = TryCast(prop?.GetValue(Nothing), EncodingProvider)
                If instance IsNot Nothing Then Encoding.RegisterProvider(instance)
            End If
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
            If String.Equals(w.Nazwa, nazwa, StringComparison.OrdinalIgnoreCase) Then Return w
        Next
        Return Nothing
    End Function

    ''' <summary>Zapisuje zbiór map do pliku tekstowego z zachowaniem kodowania programu.</summary>
    Public Sub Zapisz(sciezka As String)
        Dim sb As New ZapisPlikuVb()
        sb.Tekst(Adres)
        For Each w In Warstwy
            sb.Tekst(w.Nazwa)
            sb.Tekst(If(Not String.IsNullOrEmpty(w.RozmiarPiksela), w.RozmiarPiksela, "1"))
        Next
        sb.Zapisz(sciezka, Kodowanie())
    End Sub

    ''' <summary>Sprawdza, czy nazwa zbioru map jest poprawna dla systemu plików i nie koliduje z nazwami zarezerwowanymi.</summary>
    Public Shared Function CzyNazwaZbioruPoprawna(nazwa As String, ByRef komunikatBledu As String) As Boolean
        komunikatBledu = ""
        If String.IsNullOrWhiteSpace(nazwa) Then
            komunikatBledu = "Nazwa zbioru map nie może być pusta."
            Return False
        End If
        Dim czysta = nazwa.Trim()
        For Each c In Path.GetInvalidFileNameChars()
            If czysta.Contains(c) Then
                komunikatBledu = "Nazwa zawiera niedozwolony znak: '" & c & "'."
                Return False
            End If
        Next
        Dim male = czysta.ToLowerInvariant()
        If male = "warstwy" OrElse male = "_archiwalne" OrElse male = "download" OrElse male = "conf" Then
            komunikatBledu = "Nazwa '" & czysta & "' jest zarezerwowana dla plików programu."
            Return False
        End If
        Dim zarezerwowaneDos As String() = {"con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6",
                                            "com7", "com8", "com9", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"}
        If Array.IndexOf(zarezerwowaneDos, male) >= 0 Then
            komunikatBledu = "Nazwa '" & czysta & "' jest nazwą zarezerwowaną systemu Windows."
            Return False
        End If
        Return True
    End Function

    ''' <summary>Tworzy bezpieczną nazwę pliku z tytułu usługi lub warstwy (usuwa znaki specjalne, zamienia polskie litery i spacje na podkreślenia).</summary>
    Public Shared Function OczyscNazweZbioru(sugerowana As String) As String
        If String.IsNullOrWhiteSpace(sugerowana) Then Return "nowy_zbior_map"
        Dim s As String = sugerowana.Trim()
        'zamiana polskich znaków
        Dim plZrodlo As String = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ"
        Dim plCel As String = "acelnoszzACELNOSZZ"
        For i = 0 To plZrodlo.Length - 1
            s = s.Replace(plZrodlo(i), plCel(i))
        Next
        Dim sb As New StringBuilder()
        For Each c In s
            If (c >= "a"c AndAlso c <= "z"c) OrElse (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "0"c AndAlso c <= "9"c) OrElse c = "_"c Then
                sb.Append(c)
            Else
                sb.Append("_"c)
            End If
        Next
        s = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "_+", "_").Trim("_"c)
        If s.Length > 60 Then s = s.Substring(0, 60).TrimEnd("_"c)
        If s = "" Then s = "nowy_zbior_map"
        Return s
    End Function

    ''' <summary>Tworzy poprawny adres zapytania GetMap dla WMS z parametrami CRS/SRS i layers=.</summary>
    Public Shared Function ZbudujAdresWmsDlaPliku(adresGetMap As String, wersja As String) As String
        Dim a As String = Wms.UzupelnijAdres(adresGetMap.Trim())
        Dim czyWersja11 As Boolean = wersja.StartsWith("1.1", StringComparison.Ordinal)
        Dim ukladParam As String = If(czyWersja11, "SRS=EPSG:2180", "CRS=EPSG:2180")
        Dim verParam As String = If(czyWersja11, "version=1.1.1", "version=1.3.0")

        'inny parametr REQUEST (np. GetCapabilities z adresu zgłoszenia) - serwer odczytałby go zamiast dopisanego GetMap
        a = System.Text.RegularExpressions.Regex.Replace(a, "(?<=[?&])request=(?!getmap(&|$))[^&]*", "request=GetMap",
                                                         System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If a.IndexOf("request=getmap", StringComparison.OrdinalIgnoreCase) < 0 Then
            Dim sep As String = If(a.Contains("?"), "&", "?")
            If a.EndsWith("?", StringComparison.Ordinal) OrElse a.EndsWith("&", StringComparison.Ordinal) Then sep = ""
            a &= sep & "request=GetMap"
        End If

        If a.IndexOf("version=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&" & verParam
        End If

        If a.IndexOf("crs=", StringComparison.OrdinalIgnoreCase) < 0 AndAlso a.IndexOf("srs=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&" & ukladParam
        End If

        If a.IndexOf("layers=", StringComparison.OrdinalIgnoreCase) < 0 Then
            a &= "&layers="
        End If

        Return a
    End Function

    ''' <summary>Dopisuje nazwę zbioru do pliku indeksu warstwy.txt, jeśli jeszcze w nim nie istnieje.</summary>
    Public Shared Sub DodajDoIndeksu(plikIndeksu As String, nazwaZbioru As String)
        Dim linie As New List(Of String)
        If File.Exists(plikIndeksu) Then
            linie.AddRange(File.ReadAllLines(plikIndeksu, Kodowanie()).Select(Function(l) l.Trim()).Where(Function(l) l <> ""))
        End If
        If Not linie.Any(Function(l) String.Equals(l, nazwaZbioru, StringComparison.OrdinalIgnoreCase)) Then
            linie.Add(nazwaZbioru)
            File.WriteAllLines(plikIndeksu, linie, Kodowanie())
        End If
    End Sub

End Class
