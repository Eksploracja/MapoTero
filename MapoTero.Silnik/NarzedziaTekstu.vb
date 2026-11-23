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

Imports System.Globalization
Imports System.Reflection
Imports System.Text
Imports MapoTero.Core

''' <summary>
''' Kodowanie plików tekstowych, odczyt liczb wpisanych przez użytkownika i odmiana słów - wspólne dla silnika
''' i okien programu. Przeniesione z modułu Program (MapoTero.App), bo korzystają z nich pobieranie i eksport map.
''' </summary>
Public Module NarzedziaTekstu

#If NETCOREAPP Then
    ''' <summary>Strona kodowa ANSI systemu Windows (w .NET Framework zwracana przez Encoding.Default).</summary>
    Private Declare Function GetACP Lib "kernel32" () As Integer
#End If

    ''' <summary>
    ''' Kodowanie plików tekstowych programu (conf.txt, lastsettings.txt, pliki georeferencyjne): systemowa strona
    ''' kodowa ANSI - takie samo, jakiego używały instrukcje FileOpen/Print poprzednich wersji.
    ''' </summary>
    Public Function KodowanieSystemowe() As Encoding
#If NETCOREAPP Then
        RejestrujKodowania()
        'strona kodowa systemu (GetACP, „Język dla programów nieobsługujących Unicode”), a nie formatu regionalnego
        '(CultureInfo.CurrentCulture): przy formacie „angielski (USA)” na polskim Windows dawało to 1252 - polskie
        'litery w ścieżkach lastsettings.txt i conf.txt były tracone („Łukasz” -> „Lukasz”), a pliki zapisane przy
        'formacie polskim czytane jako „£ódŸ”
        Try
            Dim cp As Integer = GetACP()
            'Windows z opcją „Użyj UTF-8 dla obsługi języków na całym świecie” - UTF-8 bez znacznika BOM, jak w .NET Framework
            If cp = 65001 Then Return New UTF8Encoding(False)
            If cp > 0 Then Return Encoding.GetEncoding(cp)
        Catch
        End Try
        Return Encoding.GetEncoding(1250)
#Else
        Return Encoding.Default
#End If
    End Function

    ''' <summary>
    ''' W .NET 8 strony kodowe Windows (np. 1250 plików warstw) są dostępne dopiero po rejestracji dostawcy kodowań -
    ''' wywoływane na starcie programu, przed odczytem jakiegokolwiek pliku. W .NET Framework nic nie robi.
    ''' </summary>
    Public Sub RejestrujKodowania()
#If NETCOREAPP Then
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
#End If
    End Sub

    ''' <summary>
    ''' Liczba wpisana przez użytkownika - akceptuje kropkę i przecinek dziesiętny (Val() przerywał odczyt na przecinku,
    ''' więc "0,5" dawało 0). Gdy tekst nie jest liczbą - wartość domyślna.
    ''' </summary>
    Public Function Wartosc(tekst As String, Optional domyslna As Double = 0) As Double
        If String.IsNullOrWhiteSpace(tekst) Then Return domyslna
        Dim wynik As Double
        'NaN i nieskończoność ("nan", "1e999") nie są liczbami dla użytkownika - dawniej kończyły się wyjątkiem przy zaokrąglaniu
        If Double.TryParse(tekst.Trim().Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture, wynik) AndAlso
           Not Double.IsNaN(wynik) AndAlso Not Double.IsInfinity(wynik) Then Return wynik
        Return domyslna
    End Function

    ''' <summary>Liczba całkowita wpisana przez użytkownika.</summary>
    Public Function WartoscCalkowita(tekst As String, Optional domyslna As Integer = 0) As Integer
        Dim w As Double = Wartosc(tekst, domyslna)
        If w > Integer.MaxValue OrElse w < Integer.MinValue Then Return domyslna
        Return CInt(Math.Round(w))
    End Function

    ''' <summary>Liczba z kropką dziesiętną, bez zbędnych zer.</summary>
    Public Function Liczba(wartosc As Double) As String
        Return MapoTero.Core.Georeferencja.Liczba(wartosc)
    End Function

    ''' <summary>Polska odmiana rzeczownika po liczbie: 1 segment, 2-4 segmenty (bez 12-14), 5 segmentów.</summary>
    Public Function Odmiana(liczba As Long, pojedyncza As String, mnoga24 As String, mnoga As String) As String
        Dim n As Long = Math.Abs(liczba)
        If n = 1 Then Return pojedyncza
        If n Mod 10 >= 2 AndAlso n Mod 10 <= 4 AndAlso (n Mod 100 < 12 OrElse n Mod 100 > 14) Then Return mnoga24
        Return mnoga
    End Function

End Module

''' <summary>
''' Ustawienia silnika, które ustala program (MapoTero.App). Silnik nie zna okien ani ustawień programu,
''' dlatego program przekazuje mu potrzebne wartości tutaj.
''' </summary>
Public NotInheritable Class UstawieniaSilnika

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Nagłówek User-Agent zapytań HTTP pobierania (np. „MapoTero/3.19”). Domyślnie z wersji uruchomionego programu
    ''' (zestaw startowy MapoTero.exe) - tak samo jak Program.UserAgentProgramu. Klient HTTP pobierania powstaje przy
    ''' pierwszym użyciu PobieranieSegmentow, więc późniejsza zmiana nie dotyczy już utworzonego klienta.
    ''' </summary>
    Public Shared Property UserAgent As String =
        WersjaProgramu.UserAgent(If(Assembly.GetEntryAssembly(), GetType(UstawieniaSilnika).Assembly))

    ''' <summary>
    ''' Styl numeracji segmentów dla sesji, której conf.txt go nie zapisał (starsze wersje programu). Program podaje
    ''' bieżące ustawienie (Ustawienia.Numeracja); funkcja jest wywoływana przy każdym odczycie sesji, więc zmiana
    ''' ustawień działa od razu.
    ''' </summary>
    Public Shared Property NumeracjaDomyslna As Func(Of String) = Function() "NrWiersza_NrKolumny"

End Class
