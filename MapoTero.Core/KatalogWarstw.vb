'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Linq
Imports System.Text

''' <summary>
''' Węzeł katalogu warstw: grupa tematyczna (ma dzieci) albo warstwa - odnośnik do warstwy z pliku zbioru map
''' (<see cref="Zbior"/>, <see cref="Warstwa"/>) ze zrozumiałą nazwą (<see cref="Nazwa"/>).
''' </summary>
Public Class WezelKatalogu
    ''' <summary>Nazwa wyświetlana: nazwa grupy albo zrozumiała nazwa (alias) warstwy.</summary>
    Public Property Nazwa As String = ""
    ''' <summary>Warstwa: nazwa pliku zbioru map (bez .txt); grupa - pusty tekst.</summary>
    Public Property Zbior As String = ""
    ''' <summary>Warstwa: nazwa warstwy na serwerze; grupa - pusty tekst.</summary>
    Public Property Warstwa As String = ""
    Public ReadOnly Property Dzieci As New List(Of WezelKatalogu)

    Public ReadOnly Property CzyWarstwa As Boolean
        Get
            Return Zbior <> ""
        End Get
    End Property

    ''' <summary>Wszystkie warstwy w tym węźle i jego podgrupach.</summary>
    Public Function WszystkieWarstwy() As IEnumerable(Of WezelKatalogu)
        If CzyWarstwa Then Return {Me}
        Return Dzieci.SelectMany(Function(d) d.WszystkieWarstwy())
    End Function
End Class

''' <summary>
''' Katalog warstw (<c>warstwy\katalog.txt</c>): grupy tematyczne i podgrupy drzewa „Wybierz mapy” oraz zrozumiałe
''' nazwy warstw. Pliki zbiorów map się nie zmieniają (adres serwera i nazwy warstw), więc starsze wersje programu,
''' kreator serwerów i pliki sesji działają bez zmian. Składnia (UTF-8):
''' <code>
''' # komentarz
''' > Grupa
''' >> Podgrupa
''' zbiór | warstwa | nazwa wyświetlana      (nazwa pominięta - nazwa warstwy na serwerze)
''' </code>
''' Warstwa należy do ostatnio otwartej grupy. Ta sama warstwa może wystąpić w kilku grupach.
''' </summary>
Public Class KatalogWarstw

    Public Const NazwaPliku As String = "katalog.txt"

    ''' <summary>Grupy pierwszego poziomu w kolejności z pliku.</summary>
    Public ReadOnly Property Grupy As New List(Of WezelKatalogu)

    ''' <summary>Opisy błędnych wierszy (numer wiersza i powód) - wiersze te są pomijane.</summary>
    Public ReadOnly Property Bledy As New List(Of String)

    Public Shared Function Wczytaj(sciezka As String) As KatalogWarstw
        Return ZTekstu(File.ReadAllText(sciezka, Encoding.UTF8))
    End Function

    Public Shared Function ZTekstu(tekst As String) As KatalogWarstw
        Dim k As New KatalogWarstw()
        'ścieżka otwartych grup: indeks 0 - grupa pierwszego poziomu
        Dim otwarte As New List(Of WezelKatalogu)
        Dim nr As Integer = 0
        For Each surowy In If(tekst, "").Replace(vbCr, "").Split({vbLf}, StringSplitOptions.None)
            nr += 1
            Dim linia As String = surowy.Trim()
            If linia = "" OrElse linia.StartsWith("#") Then Continue For

            If linia.StartsWith(">") Then
                Dim poziom As Integer = linia.TakeWhile(Function(c) c = ">"c).Count()
                Dim nazwa As String = linia.Substring(poziom).Trim()
                If nazwa = "" Then k.Bledy.Add("wiersz " & nr & ": grupa bez nazwy") : Continue For
                If poziom > otwarte.Count + 1 Then
                    k.Bledy.Add("wiersz " & nr & ": podgrupa poziomu " & poziom & " bez grupy nadrzędnej")
                    Continue For
                End If
                Dim grupa As New WezelKatalogu With {.Nazwa = nazwa}
                If poziom = 1 Then k.Grupy.Add(grupa) Else otwarte(poziom - 2).Dzieci.Add(grupa)
                otwarte.RemoveRange(poziom - 1, otwarte.Count - (poziom - 1))
                otwarte.Add(grupa)
                Continue For
            End If

            Dim pola = linia.Split("|"c).Select(Function(p) p.Trim()).ToArray()
            If pola.Length < 2 OrElse pola(0) = "" OrElse pola(1) = "" Then
                k.Bledy.Add("wiersz " & nr & ": oczekiwano: zbiór | warstwa | nazwa") : Continue For
            End If
            If otwarte.Count = 0 Then k.Bledy.Add("wiersz " & nr & ": warstwa przed pierwszą grupą") : Continue For
            Dim nazwaWarstwy As String = If(pola.Length >= 3 AndAlso pola(2) <> "", pola(2), pola(1))
            otwarte(otwarte.Count - 1).Dzieci.Add(New WezelKatalogu With {.Zbior = pola(0), .Warstwa = pola(1), .Nazwa = nazwaWarstwy})
        Next
        Return k
    End Function

    ''' <summary>Wszystkie warstwy katalogu (z powtórzeniami, jeśli warstwa jest w kilku grupach).</summary>
    Public Function Warstwy() As IEnumerable(Of WezelKatalogu)
        Return Grupy.SelectMany(Function(g) g.WszystkieWarstwy())
    End Function

    ''' <summary>Czy warstwa zbioru map występuje w katalogu.</summary>
    Public Function Zawiera(zbior As String, warstwa As String) As Boolean
        Return Warstwy().Any(Function(w) TaSamaWarstwa(w, zbior, warstwa))
    End Function

    ''' <summary>Warstwy pliku zbioru map, których katalog nie wymienia (np. zbiory dodane kreatorem) - w kolejności pliku.</summary>
    Public Function NiewymienioneWarstwy(zbior As String, plik As PlikWarstw) As List(Of WarstwaMapy)
        If plik Is Nothing Then Return New List(Of WarstwaMapy)()
        Return plik.Warstwy.Where(Function(w) Not Zawiera(zbior, w.Nazwa)).ToList()
    End Function

    ''' <summary>Czy katalog wymienia jakąkolwiek warstwę zbioru map.</summary>
    Public Function ZawieraZbior(zbior As String) As Boolean
        Return Warstwy().Any(Function(w) String.Equals(w.Zbior, zbior, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' Nazwy wyświetlane warstw według adresu serwera i nazwy warstwy (klucz <see cref="KluczAliasu"/>) - tak rozpoznawane
    ''' są też warstwy sesji zapisanych z innym zbiorem map tego samego serwera (np. dawne zbiory Hgis_*).
    ''' Wartość: nazwa warstwy i nazwa grupy, w której jest (pierwsze wystąpienie w katalogu).
    ''' </summary>
    Public Function MapaNazw(zbiory As IDictionary(Of String, PlikWarstw)) As Dictionary(Of String, (Nazwa As String, Grupa As String))
        Dim wynik As New Dictionary(Of String, (Nazwa As String, Grupa As String))(StringComparer.Ordinal)
        For Each g In Grupy
            DodajNazwy(g, zbiory, wynik)
        Next
        Return wynik
    End Function

    Private Shared Sub DodajNazwy(wezel As WezelKatalogu, zbiory As IDictionary(Of String, PlikWarstw),
                                  wynik As Dictionary(Of String, (Nazwa As String, Grupa As String)))
        For Each d In wezel.Dzieci
            If Not d.CzyWarstwa Then DodajNazwy(d, zbiory, wynik) : Continue For
            Dim plik As PlikWarstw = Nothing
            If Not zbiory.TryGetValue(d.Zbior, plik) OrElse plik Is Nothing Then Continue For
            Dim klucz As String = KluczAliasu(plik.Adres, d.Warstwa)
            If Not wynik.ContainsKey(klucz) Then wynik(klucz) = (d.Nazwa, wezel.Nazwa)
        Next
    End Sub

    ''' <summary>Klucz warstwy niezależny od zbioru map: adres serwera (bez wielkości liter i cudzysłowów) i nazwa warstwy.</summary>
    Public Shared Function KluczAliasu(adres As String, warstwa As String) As String
        Return If(adres, "").Trim().Trim(""""c).ToLowerInvariant() & vbTab & If(warstwa, "")
    End Function

    Private Shared Function TaSamaWarstwa(w As WezelKatalogu, zbior As String, warstwa As String) As Boolean
        Return String.Equals(w.Zbior, zbior, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(w.Warstwa, warstwa, StringComparison.Ordinal)
    End Function

End Class
