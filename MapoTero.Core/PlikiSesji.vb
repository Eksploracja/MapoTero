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
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' Odczyt plików zapisanych instrukcją Write/WriteLine języka Visual Basic (format conf.txt):
''' teksty w cudzysłowach, wartości logiczne #TRUE#/#FALSE#, liczby bez cudzysłowów,
''' pola rozdzielone przecinkami lub końcami wierszy. Zastępuje FileOpen/Input z numerem pliku.
''' </summary>
Public Class CzytnikPlikuVb

    Private ReadOnly _tekst As String
    Private _pozycja As Integer

    Public Sub New(tekst As String)
        _tekst = If(tekst, "")
    End Sub

    Public Shared Function ZPliku(sciezka As String, kodowanie As Encoding) As CzytnikPlikuVb
        Return New CzytnikPlikuVb(File.ReadAllText(sciezka, kodowanie))
    End Function

    ''' <summary>Czy pozostały jeszcze pola do odczytu.</summary>
    Public ReadOnly Property KoniecPliku As Boolean
        Get
            Dim i As Integer = _pozycja
            While i < _tekst.Length AndAlso (Char.IsWhiteSpace(_tekst(i)) OrElse _tekst(i) = ","c)
                i += 1
            End While
            Return i >= _tekst.Length
        End Get
    End Property

    ''' <summary>Następne pole w postaci tekstu (cudzysłowy usunięte). Wyjątek EndOfStreamException na końcu pliku.</summary>
    Public Function Nastepne() As String
        'pomija spacje i tabulatory przed polem
        While _pozycja < _tekst.Length AndAlso (_tekst(_pozycja) = " "c OrElse _tekst(_pozycja) = ChrW(9))
            _pozycja += 1
        End While
        'pomija znak końca poprzedniego wiersza
        If _pozycja < _tekst.Length AndAlso (_tekst(_pozycja) = ChrW(13) OrElse _tekst(_pozycja) = ChrW(10)) Then
            If _tekst(_pozycja) = ChrW(13) AndAlso _pozycja + 1 < _tekst.Length AndAlso _tekst(_pozycja + 1) = ChrW(10) Then _pozycja += 1
            _pozycja += 1
            While _pozycja < _tekst.Length AndAlso (_tekst(_pozycja) = " "c OrElse _tekst(_pozycja) = ChrW(9))
                _pozycja += 1
            End While
        End If
        If _pozycja >= _tekst.Length Then Throw New EndOfStreamException("Koniec pliku")

        Dim wynik As String
        If _tekst(_pozycja) = """"c Then
            Dim koniec As Integer = _tekst.IndexOf(""""c, _pozycja + 1)
            If koniec < 0 Then koniec = _tekst.Length
            wynik = _tekst.Substring(_pozycja + 1, koniec - _pozycja - 1)
            _pozycja = Math.Min(koniec + 1, _tekst.Length)
            'pomija resztę do separatora
            While _pozycja < _tekst.Length AndAlso _tekst(_pozycja) <> ","c AndAlso _tekst(_pozycja) <> ChrW(13) AndAlso _tekst(_pozycja) <> ChrW(10)
                _pozycja += 1
            End While
        Else
            Dim start As Integer = _pozycja
            While _pozycja < _tekst.Length AndAlso _tekst(_pozycja) <> ","c AndAlso _tekst(_pozycja) <> ChrW(13) AndAlso _tekst(_pozycja) <> ChrW(10)
                _pozycja += 1
            End While
            wynik = _tekst.Substring(start, _pozycja - start).Trim()
        End If
        'przecinek kończący pole
        If _pozycja < _tekst.Length AndAlso _tekst(_pozycja) = ","c Then _pozycja += 1

        'wartości specjalne zapisywane przez Write dla pustych zmiennych
        If wynik = "#ERROR 448#" OrElse wynik = "#NULL#" Then wynik = ""
        Return wynik
    End Function

    Public Function NastepnyLogiczny() As Boolean
        Dim t As String = Nastepne().Trim("#"c)
        Return t.Equals("TRUE", StringComparison.OrdinalIgnoreCase) OrElse t = "1" OrElse t = "-1"
    End Function

    Public Function NastepnaLiczbaCalkowita() As Integer
        Dim wynik As Integer
        Integer.TryParse(Nastepne(), NumberStyles.Integer, CultureInfo.InvariantCulture, wynik)
        Return wynik
    End Function

End Class

''' <summary>Zapis w formacie instrukcji WriteLine języka Visual Basic (każde pole w osobnym wierszu).</summary>
Public Class ZapisPlikuVb

    Private ReadOnly _sb As New StringBuilder()

    Public Sub Tekst(wartosc As String)
        _sb.Append(""""c).Append(If(wartosc, "").Replace("""", "'")).Append(""""c).Append((ChrW(13) & ChrW(10)))
    End Sub

    Public Sub Logiczny(wartosc As Boolean)
        _sb.Append(If(wartosc, "#TRUE#", "#FALSE#")).Append((ChrW(13) & ChrW(10)))
    End Sub

    Public Sub LiczbaCalkowita(wartosc As Integer)
        _sb.Append(wartosc.ToString(CultureInfo.InvariantCulture)).Append((ChrW(13) & ChrW(10)))
    End Sub

    Public Overrides Function ToString() As String
        Return _sb.ToString()
    End Function

    Public Sub Zapisz(sciezka As String, kodowanie As Encoding)
        File.WriteAllText(sciezka, _sb.ToString(), kodowanie)
    End Sub

End Class

''' <summary>
''' Parametry sesji pobierania zapisywane w pliku conf.txt w folderze segmentów.
''' Kolejność pól jest zgodna z wcześniejszymi wersjami programu (pola 1-28);
''' nowe pola dopisywane są na końcu, dzięki czemu starsze wersje nadal odczytują plik.
''' Wartości pól formularza przechowywane są jako tekst - tak jak w nich wpisano.
''' </summary>
Public Class KonfiguracjaSesji

    Public Property Folder As String = ""
    Public Property RodzajMapy As String = ""
    ''' <summary>Lewy dolny narożnik obszaru (X - północ).</summary>
    Public Property XDol As String = ""
    ''' <summary>Lewy dolny narożnik obszaru (Y - wschód).</summary>
    Public Property YLewy As String = ""
    ''' <summary>Prawy górny narożnik obszaru (X - północ).</summary>
    Public Property XGora As String = ""
    ''' <summary>Prawy górny narożnik obszaru (Y - wschód).</summary>
    Public Property YPrawy As String = ""
    Public Property BokSegmentuPx As String = ""
    Public Property RozmiarPiksela As String = ""
    Public ReadOnly Property Warstwy As String() = New String(11) {}
    Public Property LiczbaWarstw As Integer
    ''' <summary>Format obrazu WMS, np. jpeg, png, tiff.</summary>
    Public Property Format As String = ""
    Public Property Prefiks As String = "_"
    Public Property PobierajPowyzejOstatniego As Boolean
    Public Property SrodekMapySzerokosc As String = ""
    Public Property SrodekMapyDlugosc As String = ""
    Public Property SkalaMapy As String = ""
    ''' <summary>Styl numeracji segmentów (puste w plikach z bardzo starych wersji).</summary>
    Public Property Numeracja As String = ""
    ''' <summary>Kod EPSG układu współrzędnych sesji (nowe pole; brak w starszych plikach = 2180).</summary>
    Public Property UkladEpsg As Integer = 2180

    Public Sub New()
        For i = 0 To 11
            Warstwy(i) = ""
        Next
    End Sub

    Public Shared Function Wczytaj(sciezka As String, kodowanie As Encoding) As KonfiguracjaSesji
        Dim c As New KonfiguracjaSesji()
        Dim r = CzytnikPlikuVb.ZPliku(sciezka, kodowanie)
        c.Folder = r.Nastepne()
        c.RodzajMapy = r.Nastepne()
        c.XDol = r.Nastepne()
        c.YLewy = r.Nastepne()
        c.XGora = r.Nastepne()
        c.YPrawy = r.Nastepne()
        c.BokSegmentuPx = r.Nastepne()
        c.RozmiarPiksela = r.Nastepne()
        For i = 0 To 11
            c.Warstwy(i) = r.Nastepne()
        Next
        c.LiczbaWarstw = r.NastepnaLiczbaCalkowita()
        c.Format = r.Nastepne()
        c.Prefiks = r.Nastepne()
        c.PobierajPowyzejOstatniego = r.NastepnyLogiczny()
        c.SrodekMapySzerokosc = r.Nastepne()
        c.SrodekMapyDlugosc = r.Nastepne()
        c.SkalaMapy = r.Nastepne()
        'pola dodane w późniejszych wersjach - mogą nie istnieć
        If Not r.KoniecPliku Then c.Numeracja = r.Nastepne()
        If Not r.KoniecPliku Then
            Dim epsg As Integer = r.NastepnaLiczbaCalkowita()
            If epsg > 0 Then c.UkladEpsg = epsg
        End If
        Return c
    End Function

    Public Function DoTekstu() As String
        Dim w As New ZapisPlikuVb()
        w.Tekst(Folder)
        w.Tekst(RodzajMapy)
        w.Tekst(XDol)
        w.Tekst(YLewy)
        w.Tekst(XGora)
        w.Tekst(YPrawy)
        w.Tekst(BokSegmentuPx)
        w.Tekst(RozmiarPiksela)
        For i = 0 To 11
            w.Tekst(Warstwy(i))
        Next
        w.LiczbaCalkowita(LiczbaWarstw)
        w.Tekst(Format)
        w.Tekst(Prefiks)
        w.Logiczny(PobierajPowyzejOstatniego)
        w.Tekst(SrodekMapySzerokosc)
        w.Tekst(SrodekMapyDlugosc)
        w.Tekst(SkalaMapy)
        w.Tekst(Numeracja)
        w.LiczbaCalkowita(UkladEpsg)
        Return w.ToString()
    End Function

    Public Sub Zapisz(sciezka As String, kodowanie As Encoding)
        File.WriteAllText(sciezka, DoTekstu(), kodowanie)
    End Sub

End Class

''' <summary>
''' Plik ustawień programu (lastsettings.txt): pary wierszy "nazwa ustawienia" / "wartość".
''' Odczyt po nazwach, a nie po kolejności - brak lub dodanie ustawienia nie przesuwa pozostałych wartości.
''' </summary>
Public Class PlikUstawien

    Private ReadOnly _wartosci As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _kolejnosc As New List(Of String)

    Public Shared Function Wczytaj(sciezka As String, kodowanie As Encoding) As PlikUstawien
        Dim u As New PlikUstawien()
        Dim linie() As String = File.ReadAllLines(sciezka, kodowanie)
        For i = 0 To linie.Length - 2 Step 2
            u.Ustaw(linie(i).Trim(), linie(i + 1).Trim())
        Next
        Return u
    End Function

    Public Sub Ustaw(nazwa As String, wartosc As String)
        If Not _wartosci.ContainsKey(nazwa) Then _kolejnosc.Add(nazwa)
        _wartosci(nazwa) = If(wartosc, "")
    End Sub

    Public Sub Ustaw(nazwa As String, wartosc As Boolean)
        Ustaw(nazwa, If(wartosc, "True", "False"))
    End Sub

    Public Sub Ustaw(nazwa As String, wartosc As Integer)
        Ustaw(nazwa, wartosc.ToString(CultureInfo.InvariantCulture))
    End Sub

    Public Function Zawiera(nazwa As String) As Boolean
        Return _wartosci.ContainsKey(nazwa)
    End Function

    ''' <summary>Wartość tekstowa; gdy brak lub pusta - wartość domyślna.</summary>
    Public Function Tekst(nazwa As String, domyslna As String) As String
        Dim w As String = Nothing
        If _wartosci.TryGetValue(nazwa, w) AndAlso w <> "" Then Return w
        Return domyslna
    End Function

    Public Function Logiczna(nazwa As String, domyslna As Boolean) As Boolean
        Dim w As String = Nothing
        Dim wynik As Boolean
        If _wartosci.TryGetValue(nazwa, w) AndAlso Boolean.TryParse(w, wynik) Then Return wynik
        Return domyslna
    End Function

    Public Function Calkowita(nazwa As String, domyslna As Integer) As Integer
        Dim w As String = Nothing
        Dim wynik As Integer
        If _wartosci.TryGetValue(nazwa, w) AndAlso Integer.TryParse(w, NumberStyles.Integer, CultureInfo.InvariantCulture, wynik) Then Return wynik
        Return domyslna
    End Function

    Public Function DoTekstu() As String
        Dim sb As New StringBuilder()
        For Each nazwa In _kolejnosc
            sb.Append(nazwa).Append((ChrW(13) & ChrW(10))).Append(_wartosci(nazwa)).Append((ChrW(13) & ChrW(10)))
        Next
        Return sb.ToString()
    End Function

    Public Sub Zapisz(sciezka As String, kodowanie As Encoding)
        File.WriteAllText(sciezka, DoTekstu(), kodowanie)
    End Sub

End Class
