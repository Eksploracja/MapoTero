'Copyright (C) <2015>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports MapoTero.Core

''' <summary>
''' Silnik bieżącego podglądu zestawu warstw (kanapki): pobiera próbki warstw w tle,
''' buforuje pobrane obrazy i składa wynikowy obraz z uwzględnieniem krycia i trybów mieszania.
''' Jeden silnik obsługuje panel podglądu w oknie głównym i okno „Warstwy - zaawansowane”, więc okno zaawansowane
''' otwiera się z obrazami już pobranymi przez panel (wcześniej każde okno pobierało własną kopię próbki).
''' Próbka (BokProbki) jest większa od panelu (BokPanelu): przy oddaleniu panel pokazuje większy obszar
''' z prawdziwych danych zamiast tego samego wycinka zmniejszonego i otoczonego szachownicą.
''' </summary>
Public Class PodgladWarstwSilnik
    Implements IDisposable

    ''' <summary>Bok pobieranej próbki w pikselach (rozmiar piksela pobierania, bez skalowania).</summary>
    Public Const BokProbki As Integer = 300

    ''' <summary>Bok miniatury podglądu w panelu okna głównego.</summary>
    Public Const BokPanelu As Integer = 116

    ''' <summary>Bok pobieranej próbki dla tej instancji silnika (domyślnie BokProbki = 300).</summary>
    Public Property Bok As Integer = BokProbki

    ' Pobrane obrazy warstw próbki (piksele ARGB) - klucz to AdresSerwera|NazwaWarstwy
    Private ReadOnly _buforWarstw As New Dictionary(Of String, Integer())(StringComparer.Ordinal)

    ' Usługi WMTS podglądu (wg adresu)
    Private ReadOnly _uslugiWmts As New Dictionary(Of String, ZrodloWmts)(StringComparer.OrdinalIgnoreCase)

    ' Usługi WMTS z poprzednich parametrów podglądu (ich kafle usuwane przy zamknięciu)
    Private ReadOnly _uslugiDoUsuniecia As New List(Of ZrodloWmts)

    Private _punktProbki As PunktXY? = Nothing
    Private _ostatniPiksel As Double = 0
    Private _ostatniEpsg As Integer = 0

    ' Zwiększane przy każdej zmianie parametrów próbki. Pobieranie rozpoczęte dla starych parametrów nie zapisuje
    ' wyników: wcześniej obraz warstwy z poprzedniego miejsca trafiał do bufora nowej próbki, bo zmiana parametrów
    ' czyściła bufor, zanim przerwano trwające pobieranie.
    Private _pokolenie As Integer = 0

    Public Sub New()
    End Sub

    ''' <summary>Klucz warstwy w buforze pamięci podręcznej.</summary>
    Public Shared Function Klucz(w As WarstwaWybrana, Optional przezroczysty As Boolean = False) As String
        If w Is Nothing Then Return ""
        Return If(w.Adres, "").ToLowerInvariant() & "|" & If(w.Nazwa, "") & If(przezroczysty, "|1", "|0")
    End Function

    ''' <summary>Zwraca informację, czy dana warstwa znajduje się już w buforze pamięci podręcznej.</summary>
    Public Function CzyWarstwaWBuforze(w As WarstwaWybrana, Optional przezroczysty As Boolean = False) As Boolean
        Return _buforWarstw.ContainsKey(Klucz(w, przezroczysty))
    End Function

    ''' <summary>Liczba warstw znajdujących się w buforze.</summary>
    Public ReadOnly Property LiczbaWarstwWBuforze As Integer
        Get
            Return _buforWarstw.Count
        End Get
    End Property

    ''' <summary>Zapamiętany punkt próbki.</summary>
    Public ReadOnly Property PunktProbki As PunktXY?
        Get
            Return _punktProbki
        End Get
    End Property

    ''' <summary>Czyści bufor pobranych pikseli.</summary>
    Public Sub WyczyscBufor()
        _buforWarstw.Clear()
        _punktProbki = Nothing
        _ostatniPiksel = 0
        _ostatniEpsg = 0
        _pokolenie += 1
    End Sub

    ''' <summary>Zasięg próbki o boku BokProbki pikseli wokół zapamiętanego punktu; Nothing - brak parametrów.</summary>
    Public Function ZasiegProbki() As Zasieg?
        If Not _punktProbki.HasValue OrElse _ostatniPiksel <= 0 Then Return Nothing
        Dim p = _punktProbki.Value
        Dim pol As Double = Bok * _ostatniPiksel / 2
        Return New Zasieg(p.X - pol, p.Y - pol, p.X + pol, p.Y + pol)
    End Function

    ''' <summary>Warstwy zestawu, których obrazu nie ma w buforze (np. serwer zwrócił błąd).</summary>
    Public Function BrakujaceWarstwy(zestaw As ZestawWarstw) As List(Of WarstwaWybrana)
        Dim wynik As New List(Of WarstwaWybrana)()
        If zestaw Is Nothing Then Return wynik
        For i = 0 To zestaw.Liczba - 1
            Dim w = zestaw(i)
            Dim czyTransparent As Boolean = (i > 0 AndAlso Not w.CzyKafelkowa)
            If Not _buforWarstw.ContainsKey(Klucz(w, czyTransparent)) AndAlso (i > 0 OrElse Not _buforWarstw.ContainsKey(Klucz(w, True))) Then
                wynik.Add(w)
            End If
        Next
        Return wynik
    End Function

    ''' <summary>Czerwona ramka obrazu podglądu: kanapka złożona bez części warstw nie wygląda na kompletną.</summary>
    Public Shared Sub RysujRamkeBraku(obraz As Bitmap)
        If obraz Is Nothing Then Exit Sub
        Using g = Graphics.FromImage(obraz), pioro As New Pen(Color.Red, 2)
            pioro.Alignment = Drawing2D.PenAlignment.Inset
            g.DrawRectangle(pioro, 0, 0, obraz.Width - 1, obraz.Height - 1)
        End Using
    End Sub

    ''' <summary>Usuwa pliki tymczasowe pobrane przez usługi WMTS.</summary>
    Public Sub UsunKafleTymczasowe()
        For Each u In _uslugiWmts.Values.Concat(_uslugiDoUsuniecia)
            u.UsunKafle()
        Next
        _uslugiWmts.Clear()
        _uslugiDoUsuniecia.Clear()
    End Sub

    ''' <summary>
    ''' Sprawdza, czy zmieniły się parametry geograficzne (punkt, piksel, układ).
    ''' Jeśli tak, czyści bufor pamięci podręcznej i zapisuje nowe parametry.
    ''' </summary>
    Public Function SprawdzIZaktualizujParametry(punkt As PunktXY, piksel As Double, uklad As UkladWspolrzednych,
                                                Optional wymus As Boolean = False) As Boolean
        Dim zmienionoGeo As Boolean = wymus OrElse Not _punktProbki.HasValue OrElse _ostatniEpsg <> uklad.Epsg OrElse Math.Abs(_ostatniPiksel - piksel) > 0.0001
        Dim zmieniono As Boolean = zmienionoGeo
        If Not zmieniono Then
            Dim dx = Math.Abs(_punktProbki.Value.X - punkt.X)
            Dim dy = Math.Abs(_punktProbki.Value.Y - punkt.Y)
            If dx > (piksel * 2) OrElse dy > (piksel * 2) Then
                zmieniono = True
            End If
        End If

        If zmieniono Then
            _buforWarstw.Clear()
            ' Plan kafli WMTS zależy od układu i rozmiaru piksela, nie od samego przesunięcia punktu próbki -
            ' zachowanie _uslugiWmts przy przesunięciu oszczędza ponowne pobieranie GetCapabilities.
            If zmienionoGeo Then
                _uslugiDoUsuniecia.AddRange(_uslugiWmts.Values)
                _uslugiWmts.Clear()
            End If
            _punktProbki = punkt
            _ostatniPiksel = piksel
            _ostatniEpsg = uklad.Epsg
            _pokolenie += 1
        End If
        Return zmieniono
    End Function

    ''' <summary>
    ''' Rysuje subtelną szachownicę ilustrującą przezroczystość w podanym kontekście graficznym.
    ''' </summary>
    Public Shared Sub RysujSzachownice(g As Graphics, szer As Integer, wys As Integer, Optional rozmiarKratki As Integer = 8)
        Using bJasny As New SolidBrush(Color.White), bCiemny As New SolidBrush(Color.FromArgb(242, 242, 242))
            For y = 0 To wys - 1 Step rozmiarKratki
                For x = 0 To szer - 1 Step rozmiarKratki
                    Dim jasny = ((x \ rozmiarKratki) + (y \ rozmiarKratki)) Mod 2 = 0
                    Dim wKratki = Math.Min(rozmiarKratki, szer - x)
                    Dim hKratki = Math.Min(rozmiarKratki, wys - y)
                    g.FillRectangle(If(jasny, bJasny, bCiemny), x, y, wKratki, hKratki)
                Next
            Next
        End Using
    End Sub

    ''' <summary>
    ''' Skaluje obraz podglądu jako lupę rastrową (centrowanie w docelowym obszarze; środek większej próbki
    ''' wypełnia mniejszy obszar docelowy, a nadmiar jest obcinany).
    ''' Dla powiększenia >= 1.0 stosuje NearestNeighbor i PixelOffsetMode.Half, aby użytkownik widział rzeczywiste piksele pobierania.
    ''' Dla pomniejszenia &lt; 1.0 stosuje wygładzanie HighQualityBicubic.
    ''' </summary>
    Public Shared Function ZastosujZoom(zrodlo As Bitmap, zoom As Double, celSzer As Integer, celWys As Integer,
                                        Optional dopasujWypelnienie As Boolean = False,
                                        Optional przesuniecieX As Integer = 0,
                                        Optional przesuniecieY As Integer = 0) As Bitmap
        If zrodlo Is Nothing Then Return Nothing
        If Not dopasujWypelnienie AndAlso przesuniecieX = 0 AndAlso przesuniecieY = 0 AndAlso Math.Abs(zoom - 1.0) < 0.001 AndAlso zrodlo.Width = celSzer AndAlso zrodlo.Height = celWys Then
            Return New Bitmap(zrodlo)
        End If

        Dim wynik As New Bitmap(celSzer, celWys, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(wynik)
            RysujSzachownice(g, celSzer, celWys)

            If zoom >= 1.0 Then
                g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
            Else
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            End If

            Dim scaledW = CInt(Math.Round(zrodlo.Width * zoom))
            Dim scaledH = CInt(Math.Round(zrodlo.Height * zoom))

            If dopasujWypelnienie AndAlso (scaledW < celSzer OrElse scaledH < celWys) Then
                Dim scaleMnoznik = Math.Max(celSzer / CDbl(Math.Max(1, scaledW)), celWys / CDbl(Math.Max(1, scaledH)))
                scaledW = CInt(Math.Ceiling(scaledW * scaleMnoznik))
                scaledH = CInt(Math.Ceiling(scaledH * scaleMnoznik))
            End If

            Dim destX = (celSzer - scaledW) \ 2 + przesuniecieX
            Dim destY = (celWys - scaledH) \ 2 + przesuniecieY

            g.DrawImage(zrodlo, New Rectangle(destX, destY, scaledW, scaledH))
        End Using
        Return wynik
    End Function

    ''' <summary>
    ''' Składa wynikową bitmapę z warstw znajdujących się w buforze, uwzględniając krycie i tryby mieszania.
    ''' Tło stanowi subtelna szachownica ilustrująca przezroczystość.
    ''' </summary>
    Public Function ZlozObraz(zestaw As ZestawWarstw) As Bitmap
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return Nothing
        Return ZlozObrazZWarstw(zestaw.Warstwy)
    End Function

    ''' <summary>
    ''' Składa wynikową bitmapę z podanych warstw znajdujących się w buforze, uwzględniając ich krycie i tryb mieszania.
    ''' </summary>
    Public Function ZlozObrazZWarstw(warstwy As IEnumerable(Of WarstwaWybrana)) As Bitmap
        If warstwy Is Nothing Then Return Nothing
        Dim lista = warstwy.ToList()
        If lista.Count = 0 Then Return Nothing

        Dim piksele(Bok * Bok - 1) As Integer
        ' Tło szachownicy 8x8 px dla wizualizacji przezroczystości
        For y = 0 To Bok - 1
            For x = 0 To Bok - 1
                Dim jasny = ((x \ 8) + (y \ 8)) Mod 2 = 0
                piksele(y * Bok + x) = If(jasny, &HFFFFFFFF, &HFFF2F2F2)
            Next
        Next

        For i = 0 To lista.Count - 1
            Dim w = lista(i)
            Dim czyTransparent As Boolean = (i > 0 AndAlso Not w.CzyKafelkowa)
            Dim obraz() As Integer = Nothing
            If _buforWarstw.TryGetValue(Klucz(w, czyTransparent), obraz) OrElse (i = 0 AndAlso _buforWarstw.TryGetValue(Klucz(w, True), obraz)) Then
                NakladanieWarstw.Naloz(piksele, obraz, w.Krycie, w.TrybMieszania)
            End If
        Next

        Return NakladanieObrazow.DoBitmapy(piksele, Bok, Bok)
    End Function

    ''' <summary>
    ''' Pobiera asynchronicznie brakujące warstwy zestawu w zasięgu próbki (ZasiegProbki) i zwraca złożoną bitmapę
    ''' oraz listę błędów. Gdy w trakcie zmienią się parametry próbki, pobieranie kończy się bez zapisu wyników.
    ''' </summary>
    Public Async Function PobierzPodgladAsync(zadanie As ZadaniePobierania, zestaw As ZestawWarstw,
                                             token As CancellationToken, Optional postep As IProgress(Of String) = Nothing) As Task(Of Tuple(Of Bitmap, List(Of String)))
        Dim bledy As New List(Of String)()
        Dim zasieg = ZasiegProbki()
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 OrElse Not zasieg.HasValue Then
            Return Tuple.Create(CType(Nothing, Bitmap), bledy)
        End If
        Dim pokolenie As Integer = _pokolenie

        Dim listaWarstw = zestaw.Warstwy.ToList()
        Dim brakujace As New List(Of (Indeks As Integer, Warstwa As WarstwaWybrana, KluczBufora As String))()
        For i = 0 To listaWarstw.Count - 1
            Dim w = listaWarstw(i)
            Dim czyTransparent As Boolean = (Not w.CzyKafelkowa) AndAlso (i > 0 OrElse zadanie.Przezroczystosc)
            Dim kluczBufora As String = Klucz(w, czyTransparent)
            If Not _buforWarstw.ContainsKey(kluczBufora) Then
                brakujace.Add((i, w, kluczBufora))
            End If
        Next

        If brakujace.Count = 0 Then
            Return Tuple.Create(ZlozObraz(zestaw), bledy)
        End If

        ' Przygotowanie brakujących usług WMTS/XYZ równolegle (po jednej na unikalny adres usługi)
        Dim doPrzygotowania = brakujace.Where(Function(b) b.Warstwa.CzyKafelkowa).
            GroupBy(Function(b) b.Warstwa.Adres, StringComparer.OrdinalIgnoreCase).
            ToList()
        Dim bledyUslug As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        If doPrzygotowania.Count > 0 Then
            postep?.Report("Łączenie z usługą kafli…")
            Dim zadaniaUslug As New List(Of Task(Of (Adres As String, Usluga As ZrodloWmts, Blad As String)))()
            For Each grupa In doPrzygotowania
                Dim pierwszy = grupa.First().Warstwa
                Dim adresUslugi = pierwszy.Adres
                Dim usluga As ZrodloWmts = Nothing
                Dim maUsluge As Boolean = _uslugiWmts.TryGetValue(adresUslugi, usluga)
                Dim brakujeWarstwy As Boolean = Not maUsluge OrElse usluga Is Nothing OrElse
                    grupa.Any(Function(b) Not usluga.CzyMaWarstwe(b.Warstwa.Nazwa))
                If Not brakujeWarstwy Then Continue For

                Dim istniejaceNazwy As IEnumerable(Of String) = If(maUsluge AndAlso usluga IsNot Nothing,
                                                                   usluga.Plany.Select(Function(p) p.Warstwa.Identyfikator),
                                                                   Enumerable.Empty(Of String)())
                Dim nazwyUslugi = zestaw.Warstwy.Where(Function(x) String.Equals(x.Adres, adresUslugi, StringComparison.OrdinalIgnoreCase)).
                                                 Select(Function(x) x.Nazwa).
                                                 Concat(istniejaceNazwy).
                                                 Distinct(StringComparer.Ordinal).
                                                 ToList()
                Dim zrodloPliku As PlikWarstw = Nothing
                If Not String.IsNullOrEmpty(pierwszy.Zbior) AndAlso zadanie.ZrodlaMap IsNot Nothing Then
                    zadanie.ZrodlaMap.TryGetValue(pierwszy.Zbior, zrodloPliku)
                End If
                If zrodloPliku Is Nothing Then zrodloPliku = zadanie.ZnajdzZrodlo(-1, adresUslugi)
                Dim typUslugi = pierwszy.EfektywnyTyp
                zadaniaUslug.Add(Task.Run(Async Function() As Task(Of (String, ZrodloWmts, String))
                                              Try
                                                  Dim nowaUsluga = Await ZrodloWmts.PrzygotujAsync(zadanie, adresUslugi, nazwyUslugi, token,
                                                                                                      typUslugi:=typUslugi, plikWarstw:=zrodloPliku).ConfigureAwait(False)
                                                  Return (adresUslugi, nowaUsluga, "")
                                              Catch ex As OperationCanceledException
                                                  Throw
                                              Catch ex As Exception
                                                  Return (adresUslugi, CType(Nothing, ZrodloWmts), OpisBledu.Z(ex))
                                              End Try
                                          End Function, token))
            Next

            If zadaniaUslug.Count > 0 Then
                Dim wynikiUslug = Await Task.WhenAll(zadaniaUslug).ConfigureAwait(True)
                If pokolenie <> _pokolenie Then
                    For Each wu In wynikiUslug
                        wu.Usluga?.UsunKafle()
                    Next
                    Throw New OperationCanceledException()
                End If
                For Each wu In wynikiUslug
                    If wu.Usluga IsNot Nothing Then
                        Dim stara As ZrodloWmts = Nothing
                        If _uslugiWmts.TryGetValue(wu.Adres, stara) AndAlso stara IsNot Nothing Then _uslugiDoUsuniecia.Add(stara)
                        _uslugiWmts(wu.Adres) = wu.Usluga
                    ElseIf wu.Blad <> "" Then
                        bledyUslug(wu.Adres) = wu.Blad
                    End If
                Next
            End If
        End If

        token.ThrowIfCancellationRequested()
        If pokolenie <> _pokolenie Then Throw New OperationCanceledException()

        postep?.Report(If(brakujace.Count = 1, "Pobieranie " & brakujace(0).Warstwa.Nazwa & "…", "Pobieranie " & brakujace.Count & " warstw…"))

        ' Pobieranie brakujących warstw równolegle
        Dim zadaniaWarstw = brakujace.Select(
            Function(poz)
                Return Task.Run(Async Function() As Task(Of (KluczBufora As String, Nazwa As String, Piksele As Integer(), Blad As String))
                                    Dim w = poz.Warstwa
                                    Dim nazwa = w.Nazwa
                                    Dim piksele() As Integer = Nothing
                                    Dim blad As String = ""
                                    If w.CzyKafelkowa Then
                                        Dim usluga As ZrodloWmts = Nothing
                                        If _uslugiWmts.TryGetValue(w.Adres, usluga) AndAlso usluga IsNot Nothing Then
                                            Try
                                                Dim wynikWmts = Await usluga.PikseleAsync({nazwa}, zasieg.Value, Bok, token).ConfigureAwait(False)
                                                piksele = wynikWmts.Item1
                                                blad = wynikWmts.Item2
                                            Catch ex As OperationCanceledException
                                                Throw
                                            Catch ex As Exception
                                                blad = OpisBledu.Z(ex)
                                            End Try
                                        Else
                                            bledyUslug.TryGetValue(w.Adres, blad)
                                            If String.IsNullOrEmpty(blad) Then blad = "nie udało się połączyć z usługą kafli"
                                        End If
                                    Else
                                        Dim czystyAdres As String = w.Adres.Trim(""""c, " "c)
                                        Dim ignorujSsl As Boolean = w.IgnorujSsl OrElse zadanie.IgnorujSsl
                                        Dim przezroczysty As Boolean = poz.Indeks > 0 OrElse zadanie.Przezroczystosc
                                        Dim formatWms As String = If(przezroczysty, "png", zadanie.Format)
                                        Dim adres As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, zadanie.Uklad, zasieg.Value, formatWms,
                                                                                 Bok, Bok, zadanie.ZamienOsie, przezroczysty)
                                        Dim wynik = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adres, Math.Max(5, zadanie.LimitCzasuSekundy), token, ignorujSsl).ConfigureAwait(False)

                                        ' Jeśli serwer odrzucił przezroczyste png, fallback na jpeg
                                        If wynik.Item1 Is Nothing AndAlso przezroczysty Then
                                            Dim adresJpg As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, zadanie.Uklad, zasieg.Value, "jpeg",
                                                                                         Bok, Bok, zadanie.ZamienOsie, False)
                                            Dim wynikJpg = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adresJpg, Math.Max(5, zadanie.LimitCzasuSekundy), token, ignorujSsl).ConfigureAwait(False)
                                            If wynikJpg.Item1 IsNot Nothing Then
                                                wynik = wynikJpg
                                            End If
                                        End If

                                        blad = wynik.Item2
                                        piksele = If(blad = "", NakladanieObrazow.Piksele(wynik.Item1, Bok, Bok), Nothing)
                                    End If
                                    Return (poz.KluczBufora, nazwa, piksele, blad)
                                End Function, token)
            End Function).ToList()

        Dim wynikiWarstw = Await Task.WhenAll(zadaniaWarstw).ConfigureAwait(True)
        If pokolenie <> _pokolenie Then Throw New OperationCanceledException()

        For Each res In wynikiWarstw
            If res.Blad <> "" Then
                bledy.Add(res.Nazwa & ": " & res.Blad)
            ElseIf res.Piksele IsNot Nothing Then
                _buforWarstw(res.KluczBufora) = res.Piksele
            End If
        Next

        Dim wynikBmp = ZlozObraz(zestaw)
        Return Tuple.Create(wynikBmp, bledy)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        _buforWarstw.Clear()
        UsunKafleTymczasowe()
    End Sub

End Class
