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
Imports System.Linq

''' <summary>
''' Warstwa wybrana do pobrania: nazwa na serwerze, źródło (zbiór map i adres serwera WMS / usługi WMTS - warstwy
''' jednej kanapki mogą pochodzić z różnych serwerów) i krycie (procent, 100 - warstwa nieprzezroczysta).
''' </summary>
Public Class WarstwaWybrana
    Public Property Nazwa As String = ""
    ''' <summary>Nazwa zbioru map (pliku w folderze warstwy), z którego wybrano warstwę.</summary>
    Public Property Zbior As String = ""
    ''' <summary>Adres serwera WMS lub usługi WMTS z pliku zbioru map.</summary>
    Public Property Adres As String = ""
    Public Property Krycie As Integer = ZestawWarstw.PelneKrycie
    ''' <summary>Zalecany rozmiar piksela z pliku zbioru map [m]; 0 - nieznany.</summary>
    Public Property ZalecanyPiksel As Double
    ''' <summary>Typ usługi ze zbioru map (Nieznana - wykrywany z adresu).</summary>
    Public Property TypZrodla As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    ''' <summary>Czy warstwa jest przezroczystą nakładką (np. szlaki turystyczne).</summary>
    Public Property Nakladka As Boolean
    ''' <summary>Czy pomijać błędy certyfikatu SSL przy pobieraniu tej warstwy.</summary>
    Public Property IgnorujSsl As Boolean
    ''' <summary>Tryb mieszania warstwy z warstwami leżącymi poniżej (domyślnie Zwykły).</summary>
    Public Property TrybMieszania As TrybMieszania = TrybMieszania.Zwykly

    Public ReadOnly Property EfektywnyTyp As TypUslugiMapowej
        Get
            Return KafleXyz.WybierzTyp(Adres, TypZrodla)
        End Get
    End Property

    Public ReadOnly Property CzyKafelkowa As Boolean
        Get
            Return KafleXyz.CzyKafelkowy(EfektywnyTyp)
        End Get
    End Property

    Public ReadOnly Property CzyWmts As Boolean
        Get
            Return EfektywnyTyp = TypUslugiMapowej.Wmts
        End Get
    End Property

    Public Function Kopia() As WarstwaWybrana
        Return New WarstwaWybrana With {
            .Nazwa = Nazwa, .Zbior = Zbior, .Adres = Adres, .Krycie = Krycie,
            .ZalecanyPiksel = ZalecanyPiksel, .TypZrodla = TypZrodla, .Nakladka = Nakladka,
            .IgnorujSsl = IgnorujSsl, .TrybMieszania = TrybMieszania}
    End Function

    ''' <summary>Ta sama warstwa tego samego serwera.</summary>
    Public Function TaSama(adres As String, nazwa As String) As Boolean
        Return String.Equals(Me.Nazwa, nazwa, StringComparison.Ordinal) AndAlso
               String.Equals(Me.Adres, adres, StringComparison.OrdinalIgnoreCase)
    End Function
End Class

''' <summary>
''' Warstwy pobierane razem z jednego serwera (WMS - jednym zapytaniem, serwer składa je sam; WMTS/XYZ/TMS - z kafli tej
''' samej usługi) i krycie, z jakim wynik jest nakładany na niższe grupy.
''' </summary>
Public Class GrupaZapytania
    Public ReadOnly Property Warstwy As New List(Of String)
    Public Property Krycie As Integer = ZestawWarstw.PelneKrycie
    ''' <summary>Tryb mieszania grupy z warstwami leżącymi poniżej (domyślnie Zwykły).</summary>
    Public Property TrybMieszania As TrybMieszania = TrybMieszania.Zwykly
    Public Property Adres As String = ""
    Public Property TypZrodla As TypUslugiMapowej = TypUslugiMapowej.Nieznana
    Public Property Nakladka As Boolean
    Public Property IgnorujSsl As Boolean

    Public ReadOnly Property EfektywnyTyp As TypUslugiMapowej
        Get
            Return KafleXyz.WybierzTyp(Adres, TypZrodla)
        End Get
    End Property

    Public ReadOnly Property CzyKafelkowa As Boolean
        Get
            Return KafleXyz.CzyKafelkowy(EfektywnyTyp)
        End Get
    End Property

    Public ReadOnly Property CzyWmts As Boolean
        Get
            Return EfektywnyTyp = TypUslugiMapowej.Wmts
        End Get
    End Property
End Class

''' <summary>Wynik dodawania warstwy do zestawu.</summary>
Public Enum WynikDodania
    Dodano
    JuzWybrana
    BrakMiejsca
End Enum

''' <summary>
''' „Kanapka” wybranych warstw: kolejność nakładania (pierwsza - podkład na spodzie), źródło i krycie każdej warstwy.
''' Najwyżej 12 warstw - tyle miejsc ma plik conf.txt.
''' </summary>
Public Class ZestawWarstw

    Public Const MaksLiczba As Integer = 12
    Public Const PelneKrycie As Integer = 100
    Public Const MinKrycie As Integer = 5
    ''' <summary>Separator wartości kolejnych warstw w polach conf.txt (nie występuje w adresach i nazwach zbiorów).</summary>
    Public Const Separator As Char = "|"c

    Private ReadOnly _warstwy As New List(Of WarstwaWybrana)

    ''' <summary>Warstwy w kolejności nakładania (indeks 0 - podkład).</summary>
    Public ReadOnly Property Warstwy As IReadOnlyList(Of WarstwaWybrana)
        Get
            Return _warstwy
        End Get
    End Property

    Public ReadOnly Property Liczba As Integer
        Get
            Return _warstwy.Count
        End Get
    End Property

    Default Public ReadOnly Property Element(indeks As Integer) As WarstwaWybrana
        Get
            Return _warstwy(indeks)
        End Get
    End Property

    Public Function Nazwy() As List(Of String)
        Return _warstwy.Select(Function(w) w.Nazwa).ToList()
    End Function

    Public Function Krycia() As List(Of Integer)
        Return _warstwy.Select(Function(w) w.Krycie).ToList()
    End Function

    Public Function Adresy() As List(Of String)
        Return _warstwy.Select(Function(w) w.Adres).ToList()
    End Function

    Public Function Zbiory() As List(Of String)
        Return _warstwy.Select(Function(w) w.Zbior).ToList()
    End Function

    ''' <summary>Czy warstwy pochodzą z więcej niż jednego serwera.</summary>
    Public ReadOnly Property WieleZrodel As Boolean
        Get
            Return _warstwy.Select(Function(w) w.Adres.ToLowerInvariant()).Distinct().Count() > 1
        End Get
    End Property

    ''' <summary>Czy któraś warstwa ma krycie mniejsze niż 100% lub nietypowy tryb mieszania (pobieranie wymaga wtedy nakładania warstw w programie).</summary>
    Public ReadOnly Property CzesciowoPrzezroczysty As Boolean
        Get
            Return _warstwy.Any(Function(w) w.Krycie < PelneKrycie OrElse w.TrybMieszania <> TrybMieszania.Zwykly)
        End Get
    End Property

    Public Function Kopia() As ZestawWarstw
        Dim z As New ZestawWarstw()
        For Each w In _warstwy
            z._warstwy.Add(w.Kopia())
        Next
        Return z
    End Function

    Public Function Zawiera(adres As String, nazwa As String) As Boolean
        Return _warstwy.Any(Function(w) w.TaSama(adres, nazwa))
    End Function

    Public Function Dodaj(nazwa As String, Optional krycie As Integer = PelneKrycie, Optional adres As String = "",
                          Optional zbior As String = "", Optional zalecanyPiksel As Double = 0,
                          Optional typZrodla As TypUslugiMapowej = TypUslugiMapowej.Nieznana,
                          Optional nakladka As Boolean = False,
                          Optional umiescPodNakladkami As Boolean = False,
                          Optional ignorujSsl As Boolean = False,
                          Optional trybMieszania As TrybMieszania = TrybMieszania.Zwykly) As WynikDodania
        If String.IsNullOrEmpty(nazwa) Then Return WynikDodania.JuzWybrana
        'ta sama warstwa dwa razy - serwer narysowałby ją podwójnie, a pomyłkowe kliknięcie trudno było cofnąć
        If Zawiera(If(adres, ""), nazwa) Then Return WynikDodania.JuzWybrana
        If _warstwy.Count >= MaksLiczba Then Return WynikDodania.BrakMiejsca
        Dim nowa As New WarstwaWybrana With {
            .Nazwa = nazwa,
            .Adres = If(adres, ""),
            .Zbior = If(zbior, ""),
            .Krycie = PoprawneKrycie(krycie),
            .ZalecanyPiksel = If(zalecanyPiksel > 0 AndAlso Not Double.IsInfinity(zalecanyPiksel), zalecanyPiksel, 0),
            .TypZrodla = typZrodla,
            .Nakladka = nakladka,
            .IgnorujSsl = ignorujSsl,
            .TrybMieszania = trybMieszania
        }
        If umiescPodNakladkami AndAlso Not nakladka AndAlso _warstwy.Count > 0 Then
            'podkład dodany po wybraniu nakładki (np. szlaków) wsuwany jest pod końcowe nakładki, żeby nie zakrył ich sobą
            Dim indeksWstawienia As Integer = _warstwy.Count
            While indeksWstawienia > 0 AndAlso _warstwy(indeksWstawienia - 1).Nakladka
                indeksWstawienia -= 1
            End While
            _warstwy.Insert(indeksWstawienia, nowa)
        Else
            _warstwy.Add(nowa)
        End If
        Return WynikDodania.Dodano
    End Function

    ''' <summary>Czy wszystkie wybrane warstwy są przezroczystymi nakładkami (brak podkładu pod spodem).</summary>
    Public Function CzySameNakladki() As Boolean
        Return _warstwy.Count > 0 AndAlso _warstwy.All(Function(w) w.Nakladka)
    End Function

    ''' <summary>Zwraca pierwszą nakładkę przykrytą przez pełnokryjącą warstwę niebędącą nakładką leżącą nad nią, albo Nothing.</summary>
    Public Function ZnajdzPrzykrytaNakladke() As WarstwaWybrana
        For i = 0 To _warstwy.Count - 2
            If Not _warstwy(i).Nakladka Then Continue For
            For j = i + 1 To _warstwy.Count - 1
                If Not _warstwy(j).Nakladka AndAlso _warstwy(j).Krycie >= PelneKrycie Then
                    Return _warstwy(i)
                End If
            Next
        Next
        Return Nothing
    End Function

    Public Sub Usun(indeks As Integer)
        If indeks >= 0 AndAlso indeks < _warstwy.Count Then _warstwy.RemoveAt(indeks)
    End Sub

    Public Sub Wyczysc()
        _warstwy.Clear()
    End Sub

    ''' <summary>Przenosi warstwę na inną pozycję (indeksy w kolejności nakładania). Zwraca nową pozycję warstwy.</summary>
    Public Function Przenies(zIndeksu As Integer, naIndeks As Integer) As Integer
        If zIndeksu < 0 OrElse zIndeksu >= _warstwy.Count Then Return zIndeksu
        naIndeks = Math.Max(0, Math.Min(_warstwy.Count - 1, naIndeks))
        If naIndeks = zIndeksu Then Return zIndeksu
        Dim w = _warstwy(zIndeksu)
        _warstwy.RemoveAt(zIndeksu)
        _warstwy.Insert(naIndeks, w)
        Return naIndeks
    End Function

    ''' <summary>
    ''' Warstwa o najmniejszym zalecanym rozmiarze piksela (najdokładniejsza) - wg niej dobierany jest piksel sesji, żeby
    ''' żadna warstwa kanapki nie traciła szczegółów (mniej dokładne są przeskalowane). Nothing - żadna warstwa nie ma
    ''' zalecanego rozmiaru. Przy równych wartościach - niższa warstwa.
    ''' </summary>
    Public Function NajdokladniejszaWarstwa() As WarstwaWybrana
        Return _warstwy.Where(Function(w) w.ZalecanyPiksel > 0).OrderBy(Function(w) w.ZalecanyPiksel).FirstOrDefault()
    End Function

    ''' <summary>Warstwa o największym zalecanym rozmiarze piksela (Nothing - brak zalecanych rozmiarów).</summary>
    Public Function NajmniejDokladnaWarstwa() As WarstwaWybrana
        Return _warstwy.Where(Function(w) w.ZalecanyPiksel > 0).OrderByDescending(Function(w) w.ZalecanyPiksel).FirstOrDefault()
    End Function

    Public Sub UstawKrycie(indeks As Integer, krycie As Integer)
        If indeks >= 0 AndAlso indeks < _warstwy.Count Then _warstwy(indeks).Krycie = PoprawneKrycie(krycie)
    End Sub

    Public Sub UstawTrybMieszania(indeks As Integer, tryb As TrybMieszania)
        If indeks >= 0 AndAlso indeks < _warstwy.Count Then _warstwy(indeks).TrybMieszania = tryb
    End Sub

    Public Shared Function PoprawneKrycie(krycie As Integer) As Integer
        Return Math.Max(MinKrycie, Math.Min(PelneKrycie, krycie))
    End Function

    ''' <summary>
    ''' Grupy warstw pobieranych razem dla jednego segmentu. Sąsiednie warstwy tego samego serwera o pełnym kryciu
    ''' i zwykłym trybie mieszania pobierane są razem (WMS - jednym zapytaniem, serwer nakłada je sam), a osobno:
    ''' każda warstwa o kryciu mniejszym niż 100%, z innym trybem mieszania i każda zmiana serwera.
    ''' </summary>
    Public Function GrupyZapytan() As List(Of GrupaZapytania)
        Dim grupy As New List(Of GrupaZapytania)
        Dim biezaca As GrupaZapytania = Nothing
        For Each w In _warstwy
            If w.Krycie >= PelneKrycie AndAlso w.TrybMieszania = TrybMieszania.Zwykly AndAlso
               biezaca IsNot Nothing AndAlso biezaca.Krycie >= PelneKrycie AndAlso biezaca.TrybMieszania = TrybMieszania.Zwykly AndAlso
               String.Equals(biezaca.Adres, w.Adres, StringComparison.OrdinalIgnoreCase) AndAlso
               biezaca.EfektywnyTyp = w.EfektywnyTyp AndAlso
               biezaca.IgnorujSsl = w.IgnorujSsl Then
                biezaca.Warstwy.Add(w.Nazwa)
            Else
                biezaca = New GrupaZapytania With {
                    .Krycie = w.Krycie,
                    .TrybMieszania = w.TrybMieszania,
                    .Adres = w.Adres,
                    .TypZrodla = w.TypZrodla,
                    .Nakladka = w.Nakladka,
                    .IgnorujSsl = w.IgnorujSsl
                }
                biezaca.Warstwy.Add(w.Nazwa)
                grupy.Add(biezaca)
            End If
        Next
        Return grupy
    End Function

    ''' <summary>Krycie warstw w polu conf.txt, np. "100;60;100"; same pełne krycia - pusty tekst.</summary>
    Public Function TekstKrycia() As String
        If Not _warstwy.Any(Function(w) w.Krycie < PelneKrycie) Then Return ""
        Return String.Join(";", _warstwy.Select(Function(w) w.Krycie.ToString(CultureInfo.InvariantCulture)))
    End Function

    ''' <summary>Tryby mieszania warstw w polu conf.txt, np. "0;1;0"; same zwykłe - pusty tekst.</summary>
    Public Function TekstTrybowMieszania() As String
        If Not _warstwy.Any(Function(w) w.TrybMieszania <> TrybMieszania.Zwykly) Then Return ""
        Return String.Join(";", _warstwy.Select(Function(w) CInt(w.TrybMieszania).ToString(CultureInfo.InvariantCulture)))
    End Function

    ''' <summary>Krycie kolejnych warstw z pola conf.txt; brakujące lub nieczytelne wartości - pełne krycie.</summary>
    Public Shared Function KryciaZTekstu(tekst As String, liczba As Integer) As List(Of Integer)
        Dim wynik As New List(Of Integer)
        Dim czesci = If(tekst, "").Split(";"c)
        For i = 0 To liczba - 1
            Dim k As Integer = PelneKrycie
            If i < czesci.Length Then
                Dim v As Integer
                If Integer.TryParse(czesci(i).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, v) Then k = PoprawneKrycie(v)
            End If
            wynik.Add(k)
        Next
        Return wynik
    End Function

    ''' <summary>Tryby mieszania kolejnych warstw z pola conf.txt; brakujące lub nieczytelne - tryb zwykły.</summary>
    Public Shared Function TrybyMieszaniaZTekstu(tekst As String, liczba As Integer) As List(Of TrybMieszania)
        Dim wynik As New List(Of TrybMieszania)
        Dim czesci = If(tekst, "").Split(";"c)
        For i = 0 To liczba - 1
            Dim t As TrybMieszania = TrybMieszania.Zwykly
            If i < czesci.Length AndAlso Not String.IsNullOrWhiteSpace(czesci(i)) Then
                Dim v As Integer
                If Integer.TryParse(czesci(i).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, v) AndAlso
                   [Enum].IsDefined(GetType(TrybMieszania), v) Then
                    t = CType(v, TrybMieszania)
                Else
                    t = TrybMieszaniaRozszerzenia.ZTekstu(czesci(i))
                End If
            End If
            wynik.Add(t)
        Next
        Return wynik
    End Function

    ''' <summary>Wartości kolejnych warstw połączone separatorem (pole conf.txt adresów lub zbiorów).</summary>
    Public Shared Function Polacz(wartosci As IEnumerable(Of String)) As String
        Return String.Join(Separator.ToString(), wartosci.Select(Function(w) If(w, "")))
    End Function

    ''' <summary>
    ''' Wartości kolejnych warstw z pola conf.txt; brakujące (pole puste - pliki starszych wersji, albo krótsze) -
    ''' wartość domyślna (adres serwera lub zbiór całej sesji).
    ''' </summary>
    Public Shared Function Rozdziel(tekst As String, liczba As Integer, domyslna As String) As List(Of String)
        Dim czesci = If(String.IsNullOrEmpty(tekst), New String() {}, tekst.Split(Separator))
        Dim wynik As New List(Of String)
        For i = 0 To liczba - 1
            wynik.Add(If(i < czesci.Length AndAlso czesci(i).Trim() <> "", czesci(i).Trim(), If(domyslna, "")))
        Next
        Return wynik
    End Function

    ''' <summary>Zestaw z nazw warstw i pól conf.txt (krycie, adresy, zbiory i tryby mieszania warstw).</summary>
    Public Shared Function ZNazw(nazwy As IEnumerable(Of String), Optional tekstKrycia As String = "",
                                 Optional adresy As IList(Of String) = Nothing, Optional zbiory As IList(Of String) = Nothing,
                                 Optional tekstTrybow As String = "") As ZestawWarstw
        Dim z As New ZestawWarstw()
        Dim lista = nazwy.Where(Function(n) Not String.IsNullOrEmpty(n)).ToList()
        Dim krycia = KryciaZTekstu(tekstKrycia, lista.Count)
        Dim tryby = TrybyMieszaniaZTekstu(tekstTrybow, lista.Count)
        For i = 0 To lista.Count - 1
            z.Dodaj(lista(i), krycia(i),
                    If(adresy IsNot Nothing AndAlso i < adresy.Count, adresy(i), ""),
                    If(zbiory IsNot Nothing AndAlso i < zbiory.Count, zbiory(i), ""),
                    trybMieszania:=tryby(i))
        Next
        Return z
    End Function

End Class

''' <summary>Nakładanie warstw rastrowych z kryciem i trybami mieszania (piksele ARGB 32-bit, Format32bppArgb).</summary>
Public NotInheritable Class NakladanieWarstw

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Nakłada <paramref name="warstwa"/> na <paramref name="cel"/> z kryciem w procentach i wybranym trybem mieszania.
    ''' Przezroczystość pikseli warstwy (np. tło PNG) jest uwzględniana razem z kryciem.
    ''' </summary>
    Public Shared Sub Naloz(cel() As Integer, warstwa() As Integer, krycie As Integer, Optional tryb As TrybMieszania = TrybMieszania.Zwykly)
        If cel Is Nothing OrElse warstwa Is Nothing Then Exit Sub
        Dim k As Double = Math.Max(0, Math.Min(100, krycie)) / 100.0
        Dim n As Integer = Math.Min(cel.Length, warstwa.Length)
        If tryb = TrybMieszania.Zwykly Then
            For i = 0 To n - 1
                Dim s As Integer = warstwa(i)
                Dim sa As Double = ((s >> 24) And &HFF) / 255.0 * k
                If sa <= 0 Then Continue For
                Dim d As Integer = cel(i)
                Dim da As Double = ((d >> 24) And &HFF) / 255.0
                Dim oa As Double = sa + da * (1 - sa)
                Dim r As Double = (((s >> 16) And &HFF) * sa + ((d >> 16) And &HFF) * da * (1 - sa)) / oa
                Dim g As Double = (((s >> 8) And &HFF) * sa + ((d >> 8) And &HFF) * da * (1 - sa)) / oa
                Dim b As Double = ((s And &HFF) * sa + (d And &HFF) * da * (1 - sa)) / oa
                cel(i) = (Bajt(oa * 255) << 24) Or (Bajt(r) << 16) Or (Bajt(g) << 8) Or Bajt(b)
            Next
        Else
            For i = 0 To n - 1
                Dim s As Integer = warstwa(i)
                Dim sa As Double = ((s >> 24) And &HFF) / 255.0 * k
                If sa <= 0 Then Continue For
                Dim d As Integer = cel(i)
                Dim da As Double = ((d >> 24) And &HFF) / 255.0
                If da <= 0 Then
                    cel(i) = (Bajt(sa * 255) << 24) Or (s And &HFFFFFF)
                    Continue For
                End If

                Dim br As Double = ((d >> 16) And &HFF) / 255.0
                Dim bg As Double = ((d >> 8) And &HFF) / 255.0
                Dim bb As Double = (d And &HFF) / 255.0
                Dim lr As Double = ((s >> 16) And &HFF) / 255.0
                Dim lg As Double = ((s >> 8) And &HFF) / 255.0
                Dim lb As Double = (s And &HFF) / 255.0

                Dim mr As Double, mg As Double, mb As Double
                Select Case tryb
                    Case TrybMieszania.Mnozenie
                        ' 2 Mnożenie: Cb * Cs
                        mr = br * lr
                        mg = bg * lg
                        mb = bb * lb
                    Case TrybMieszania.Ciemniej
                        ' 3 Ciemniej: min(Cb, Cs)
                        mr = Math.Min(br, lr)
                        mg = Math.Min(bg, lg)
                        mb = Math.Min(bb, lb)
                    Case TrybMieszania.Jasniej
                        ' 4 Jaśniej: max(Cb, Cs)
                        mr = Math.Max(br, lr)
                        mg = Math.Max(bg, lg)
                        mb = Math.Max(bb, lb)
                    Case TrybMieszania.Ekran
                        ' 5 Ekran: 1 - (1-Cb)*(1-Cs)
                        mr = 1.0 - (1.0 - br) * (1.0 - lr)
                        mg = 1.0 - (1.0 - bg) * (1.0 - lg)
                        mb = 1.0 - (1.0 - bb) * (1.0 - lb)
                    Case TrybMieszania.Nakladka
                        ' 6 Nakładka: mnożenie dla ciemnych, ekran dla jasnych
                        mr = If(br < 0.5, 2.0 * br * lr, 1.0 - 2.0 * (1.0 - br) * (1.0 - lr))
                        mg = If(bg < 0.5, 2.0 * bg * lg, 1.0 - 2.0 * (1.0 - bg) * (1.0 - lg))
                        mb = If(bb < 0.5, 2.0 * bb * lb, 1.0 - 2.0 * (1.0 - bb) * (1.0 - lb))
                    Case Else
                        mr = lr
                        mg = lg
                        mb = lb
                End Select

                Dim oa As Double = sa + da * (1.0 - sa)
                Dim wr As Double = mr * sa + br * da * (1.0 - sa)
                Dim wg As Double = mg * sa + bg * da * (1.0 - sa)
                Dim wb As Double = mb * sa + bb * da * (1.0 - sa)
                Dim r As Double = wr / oa * 255.0
                Dim g As Double = wg / oa * 255.0
                Dim b As Double = wb / oa * 255.0
                cel(i) = (Bajt(oa * 255) << 24) Or (Bajt(r) << 16) Or (Bajt(g) << 8) Or Bajt(b)
            Next
        End If
    End Sub

    Private Shared Function Bajt(v As Double) As Integer
        Return Math.Max(0, Math.Min(255, CInt(Math.Round(v))))
    End Function

End Class
