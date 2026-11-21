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
    ''' <summary>Zwięzły identyfikator / skrót nazwy warstwy (maksymalnie 20 znaków, np. Topo_65_10k); grupa - pusty tekst.</summary>
    Public Property Identyfikator As String = ""
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
''' zbiór | warstwa | nazwa wyświetlana [| identyfikator]      (nazwa pominięta - nazwa warstwy na serwerze; identyfikator - skrót do 20 znaków)
''' </code>
''' Warstwa należy do ostatnio otwartej grupy. Ta sama warstwa może wystąpić w kilku grupach.
''' </summary>
Public Class KatalogWarstw

    Public Const NazwaPliku As String = "katalog.txt"

    Private Shared _instancja As KatalogWarstw
    Private Shared ReadOnly _blokada As New Object()

    ''' <summary>Współdzielona instancja katalogu warstw w aplikacji.</summary>
    Public Shared Property Instancja As KatalogWarstw
        Get
            SyncLock _blokada
                If _instancja Is Nothing Then
                    _instancja = PobierzDomyslny()
                End If
                Return _instancja
            End SyncLock
        End Get
        Set(value As KatalogWarstw)
            SyncLock _blokada
                _instancja = value
            End SyncLock
        End Set
    End Property

    ''' <summary>Zwraca domyślny katalog warstw wczytany z katalogu warstw programu lub standardowych ścieżek.</summary>
    Public Shared Function PobierzDomyslny(Optional folderWarstw As String = "") As KatalogWarstw
        Dim kandydaci As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(folderWarstw) Then
            kandydaci.Add(Path.Combine(folderWarstw, NazwaPliku))
        End If
        kandydaci.Add(Path.Combine(AppContext.BaseDirectory, "warstwy", NazwaPliku))
        kandydaci.Add(Path.Combine(AppContext.BaseDirectory, NazwaPliku))
        kandydaci.Add(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "warstwy", NazwaPliku))
        kandydaci.Add(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "warstwy", NazwaPliku))

        For Each sciezka In kandydaci
            Try
                If File.Exists(sciezka) Then
                    Return Wczytaj(sciezka)
                End If
            Catch
            End Try
        Next
        Return New KatalogWarstw()
    End Function

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
            Dim identyfikatorWarstwy As String = If(pola.Length >= 4, pola(3), "")
            otwarte(otwarte.Count - 1).Dzieci.Add(New WezelKatalogu With {
                .Zbior = pola(0),
                .Warstwa = pola(1),
                .Nazwa = nazwaWarstwy,
                .Identyfikator = identyfikatorWarstwy
            })
        Next
        Return k
    End Function

    ''' <summary>Wszystkie warstwy katalogu (z powtórzeniami, jeśli warstwa jest w kilku grupach).</summary>
    Public Function Warstwy() As IEnumerable(Of WezelKatalogu)
        Return Grupy.SelectMany(Function(g) g.WszystkieWarstwy())
    End Function

    ''' <summary>Zwięzły identyfikator warstwy (np. Topo_65_10k) z katalogu warstw; pusty tekst - brak w katalogu.</summary>
    Public Function ZnajdzIdentyfikator(zbior As String, warstwa As String) As String
        Dim w = Warstwy().FirstOrDefault(Function(x) TaSamaWarstwa(x, zbior, warstwa))
        Return If(w IsNot Nothing, w.Identyfikator, "")
    End Function

    ''' <summary>Wyszukuje identyfikator (skrót do 20 znaków) pasujący do nazwy warstwy z dowolnego zbioru.</summary>
    Public Function ZnajdzIdentyfikatorWgWarstwy(warstwa As String) As String
        If String.IsNullOrWhiteSpace(warstwa) Then Return ""
        Dim w = Warstwy().FirstOrDefault(Function(x) String.Equals(x.Warstwa, warstwa, StringComparison.OrdinalIgnoreCase))
        Return If(w IsNot Nothing, w.Identyfikator, "")
    End Function

    ''' <summary>Wyszukuje czytelną nazwę wyświetlaną warstwy z katalogu warstw.</summary>
    Public Function ZnajdzNazwe(zbior As String, warstwa As String) As String
        Dim w = Warstwy().FirstOrDefault(Function(x) TaSamaWarstwa(x, zbior, warstwa))
        If w IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(w.Nazwa) Then Return w.Nazwa
        w = Warstwy().FirstOrDefault(Function(x) String.Equals(x.Warstwa, warstwa, StringComparison.OrdinalIgnoreCase))
        Return If(w IsNot Nothing, w.Nazwa, "")
    End Function

    ''' <summary>
    ''' Wyznacza zwięzłą, skróconą nazwę warstwy dla sesji na podstawie jej zbioru i listy warstw (np. Topo_65_10k).
    ''' </summary>
    Public Function SkroconaNazwaSesji(c As KonfiguracjaSesji, Optional folder As String = "") As String
        If c Is Nothing Then
            Return If(Not String.IsNullOrWhiteSpace(folder), Path.GetFileName(folder.TrimEnd("\"c, "/"c)), "")
        End If

        Dim warstwy = If(c.Warstwy, Array.Empty(Of String)())
        Dim liczba As Integer = Math.Max(0, Math.Min(warstwy.Length, c.LiczbaWarstw))
        If liczba = 0 Then
            liczba = warstwy.TakeWhile(Function(s) Not String.IsNullOrWhiteSpace(s)).Count()
        End If

        If liczba > 0 Then
            Dim skroty As New List(Of String)()
            For i = 0 To liczba - 1
                Dim wName = warstwy(i)
                If String.IsNullOrWhiteSpace(wName) Then Continue For
                Dim id = ZnajdzIdentyfikator(c.RodzajMapy, wName)
                If String.IsNullOrWhiteSpace(id) Then id = ZnajdzIdentyfikatorWgWarstwy(wName)
                If String.IsNullOrWhiteSpace(id) Then id = wName
                skroty.Add(id)
            Next

            If skroty.Count = 1 Then
                Return skroty(0)
            ElseIf skroty.Count > 1 Then
                Dim polaczone = String.Join("+", skroty)
                If polaczone.Length > 30 Then
                    Return skroty(0) & "_mix"
                End If
                Return polaczone
            End If
        End If

        If Not String.IsNullOrWhiteSpace(c.RodzajMapy) Then
            Return c.RodzajMapy
        End If

        Return If(Not String.IsNullOrWhiteSpace(folder), Path.GetFileName(folder.TrimEnd("\"c, "/"c)), "")
    End Function

    ''' <summary>
    ''' Zwraca pełny, czytelny opis sesji (np. zrozumiała nazwa z katalogu oraz serwer i warstwa).
    ''' </summary>
    Public Function PelnyOpisSesji(c As KonfiguracjaSesji, Optional folder As String = "") As String
        If c Is Nothing Then Return If(Not String.IsNullOrWhiteSpace(folder), folder, "")
        Dim wName = If(c.Warstwy IsNot Nothing AndAlso c.Warstwy.Length > 0, c.Warstwy(0), "")
        Dim czytelna = ZnajdzNazwe(c.RodzajMapy, wName)
        Dim serwerIWarstwa = (c.RodzajMapy & " " & wName).Trim()
        If Not String.IsNullOrWhiteSpace(czytelna) AndAlso Not String.Equals(czytelna, serwerIWarstwa, StringComparison.OrdinalIgnoreCase) Then
            Return czytelna & " (" & serwerIWarstwa & ")"
        End If
        Return serwerIWarstwa
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
    ''' Nazwy wyświetlane i identyfikatory warstw według adresu serwera i nazwy warstwy (klucz <see cref="KluczAliasu"/>) - tak rozpoznawane
    ''' są też warstwy sesji zapisanych z innym zbiorem map tego samego serwera (np. dawne zbiory Hgis_*).
    ''' Wartość: nazwa warstwy, nazwa grupy i skrócony identyfikator (pierwsze wystąpienie w katalogu).
    ''' </summary>
    Public Function MapaNazw(zbiory As IDictionary(Of String, PlikWarstw)) As Dictionary(Of String, (Nazwa As String, Grupa As String, Identyfikator As String))
        Dim wynik As New Dictionary(Of String, (Nazwa As String, Grupa As String, Identyfikator As String))(StringComparer.Ordinal)
        For Each g In Grupy
            DodajNazwy(g, zbiory, wynik)
        Next
        Return wynik
    End Function

    Private Shared Sub DodajNazwy(wezel As WezelKatalogu, zbiory As IDictionary(Of String, PlikWarstw),
                                  wynik As Dictionary(Of String, (Nazwa As String, Grupa As String, Identyfikator As String)))
        For Each d In wezel.Dzieci
            If Not d.CzyWarstwa Then DodajNazwy(d, zbiory, wynik) : Continue For
            Dim plik As PlikWarstw = Nothing
            If Not zbiory.TryGetValue(d.Zbior, plik) OrElse plik Is Nothing Then Continue For
            Dim klucz As String = KluczAliasu(plik.Adres, d.Warstwa)
            If Not wynik.ContainsKey(klucz) Then wynik(klucz) = (d.Nazwa, wezel.Nazwa, d.Identyfikator)
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
