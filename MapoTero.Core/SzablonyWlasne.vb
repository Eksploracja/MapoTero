'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>
''' Style mapy zapisane przez użytkownika w kreatorze (plik <see cref="NazwaPliku"/> w folderze danych programu, JSON).
''' Styl zapamiętuje skład warstw z kryciem i trybem mieszania - w mapie wieloskalowej osobno dla każdej skali.
''' </summary>
Public Module SzablonyWlasne

    Public Const NazwaPliku As String = "szablony_wlasne.json"
    Public Const PrefiksId As String = "styl_uzytkownika_"
    Public Const KategoriaWlasna As String = "Moje style"

    ''' <summary>Czy styl zapisał użytkownik (można go usunąć).</summary>
    Public Function CzyWlasny(s As SzablonMapy) As Boolean
        Return s IsNot Nothing AndAlso s.Id.StartsWith(PrefiksId, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function NoweId() As String
        Return PrefiksId & Guid.NewGuid().ToString("N").Substring(0, 12)
    End Function

    ''' <summary>Style z pliku; brak pliku - pusta lista. Uszkodzony plik zgłasza wyjątek z opisem (nie gubimy cudzej pracy po cichu).</summary>
    Public Function Wczytaj(sciezka As String) As List(Of SzablonMapy)
        Dim wynik As New List(Of SzablonMapy)()
        If String.IsNullOrEmpty(sciezka) OrElse Not File.Exists(sciezka) Then Return wynik
        Dim plik = JsonSerializer.Deserialize(Of PlikStylow)(File.ReadAllText(sciezka, Encoding.UTF8), Opcje)
        If plik?.Style Is Nothing Then Return wynik
        For Each d In plik.Style
            If String.IsNullOrWhiteSpace(d.Nazwa) Then Continue For
            wynik.Add(ZDanych(d))
        Next
        Return wynik
    End Function

    ''' <summary>Zapisuje style przez plik tymczasowy - przerwany zapis nie niszczy poprzedniej listy.</summary>
    Public Sub Zapisz(sciezka As String, style As IEnumerable(Of SzablonMapy))
        Dim plik As New PlikStylow With {.Wersja = 1, .Style = style.Select(AddressOf DoDanych).ToList()}
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(sciezka))
        If Not String.IsNullOrEmpty(folder) Then Directory.CreateDirectory(folder)
        Dim tmp = sciezka & ".tmp"
        File.WriteAllText(tmp, JsonSerializer.Serialize(plik, Opcje), New UTF8Encoding(False))
        File.Move(tmp, sciezka, overwrite:=True)
    End Sub

    Private ReadOnly Opcje As JsonSerializerOptions = UtworzOpcje()

    Private Function UtworzOpcje() As JsonSerializerOptions
        ' Nazwy wyliczeń tekstem (czytelny plik), polskie znaki bez kodowania
        Dim o As New JsonSerializerOptions With {
            .WriteIndented = True,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}
        o.Converters.Add(New JsonStringEnumConverter())
        Return o
    End Function

    Private Function DoDanych(s As SzablonMapy) As DaneStylu
        Dim d As New DaneStylu With {
            .Id = s.Id, .Nazwa = s.Nazwa, .Opis = s.Opis, .Piktogram = s.Piktogram, .DomyslnyPiksel = s.DomyslnyPiksel,
            .ProfilKorekty = s.ProfilKorekty, .ProbkaLat = s.ProbkaLat, .ProbkaLon = s.ProbkaLon, .ProbkaPiksel = s.ProbkaPiksel,
            .UwagaPokrycia = s.UwagaPokrycia, .Cele = s.DozwoloneCele.ToList()}
        If s.Wieloskalowy Then
            d.Skale = s.StopnieSkali.Select(Function(st) New DaneSkali With {.Opis = st.Opis, .Piksel = st.Piksel,
                                                                              .Opcjonalny = st.Opcjonalny, .Warstwy = st.Warstwy.ToList()}).ToList()
        Else
            d.Warstwy = s.Warstwy.ToList()
        End If
        Return d
    End Function

    Private Function ZDanych(d As DaneStylu) As SzablonMapy
        Dim s As New SzablonMapy(If(String.IsNullOrWhiteSpace(d.Id) OrElse Not d.Id.StartsWith(PrefiksId, StringComparison.OrdinalIgnoreCase), NoweId(), d.Id),
                                 d.Nazwa, KategoriaWlasna, If(d.Opis, ""), If(String.IsNullOrEmpty(d.Piktogram), "⭐", d.Piktogram),
                                 If(d.DomyslnyPiksel > 0, d.DomyslnyPiksel, 1.0), d.ProfilKorekty,
                                 If(d.Cele, New List(Of CelMapy)()).ToArray()) With {
            .ProbkaLat = If(d.ProbkaLat <> 0, d.ProbkaLat, 52.0), .ProbkaLon = If(d.ProbkaLon <> 0, d.ProbkaLon, 19.0),
            .ProbkaPiksel = If(d.ProbkaPiksel > 0, d.ProbkaPiksel, If(d.DomyslnyPiksel > 0, d.DomyslnyPiksel, 1.0)),
            .UwagaPokrycia = If(d.UwagaPokrycia, "")}
        If d.Skale IsNot Nothing AndAlso d.Skale.Count > 0 Then
            For Each sk In d.Skale.Where(Function(x) x.Warstwy IsNot Nothing AndAlso x.Warstwy.Count > 0)
                If sk.Opcjonalny Then
                    s.DodajStopienOpcjonalny(If(sk.Opis, ""), If(sk.Piksel > 0, sk.Piksel, 1.0), sk.Warstwy.ToArray())
                Else
                    s.DodajStopien(If(sk.Opis, ""), If(sk.Piksel > 0, sk.Piksel, 1.0), sk.Warstwy.ToArray())
                End If
            Next
        ElseIf d.Warstwy IsNot Nothing Then
            s.Warstwy.AddRange(d.Warstwy)
        End If
        Return s
    End Function

    Private Class PlikStylow
        Public Property Wersja As Integer
        Public Property Style As List(Of DaneStylu)
    End Class

    Private Class DaneStylu
        Public Property Id As String
        Public Property Nazwa As String
        Public Property Opis As String
        Public Property Piktogram As String
        Public Property DomyslnyPiksel As Double
        Public Property ProfilKorekty As ProfilKorektyRastra
        Public Property ProbkaLat As Double
        Public Property ProbkaLon As Double
        Public Property ProbkaPiksel As Double
        Public Property UwagaPokrycia As String
        Public Property Cele As List(Of CelMapy)
        Public Property Warstwy As List(Of WarstwaSzablonu)
        Public Property Skale As List(Of DaneSkali)
    End Class

    Private Class DaneSkali
        Public Property Opis As String
        Public Property Piksel As Double
        ''' <summary>Stopień opcjonalny (np. ortofotomapa przy zbliżeniu); pomijany w pliku, gdy False - starsze pliki bez zmian.</summary>
        Public Property Opcjonalny As Boolean
        Public Property Warstwy As List(Of WarstwaSzablonu)
    End Class

End Module
