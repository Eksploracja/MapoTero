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
''' buforuje pobrane obrazy i składa wynikową miniaturę z uwzględnieniem krycia i trybów mieszania.
''' </summary>
Public Class PodgladWarstwSilnik
    Implements IDisposable

    ''' <summary>Bok miniatury podglądu w pikselach.</summary>
    Public Const BokPodgladu As Integer = 116

    ' Pobrane obrazy warstw próbki (piksele ARGB) - klucz to AdresSerwera|NazwaWarstwy
    Private ReadOnly _buforWarstw As New Dictionary(Of String, Integer())(StringComparer.Ordinal)

    ' Usługi WMTS podglądu (wg adresu)
    Private ReadOnly _uslugiWmts As New Dictionary(Of String, ZrodloWmts)(StringComparer.OrdinalIgnoreCase)

    ' Usługi WMTS z poprzednich parametrów podglądu (ich kafle usuwane przy zamknięciu)
    Private ReadOnly _uslugiDoUsuniecia As New List(Of ZrodloWmts)

    Private _punktProbki As PunktXY? = Nothing
    Private _ostatniPiksel As Double = 0
    Private _ostatniEpsg As Integer = 0

    Public Sub New()
    End Sub

    ''' <summary>Klucz warstwy w buforze pamięci podręcznej.</summary>
    Public Shared Function Klucz(w As WarstwaWybrana) As String
        Return w.Adres.ToLowerInvariant() & "|" & w.Nazwa
    End Function

    ''' <summary>Zwraca informację, czy dana warstwa znajduje się już w buforze pamięci podręcznej.</summary>
    Public Function CzyWarstwaWBuforze(w As WarstwaWybrana) As Boolean
        Return _buforWarstw.ContainsKey(Klucz(w))
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
    Public Function SprawdzIZaktualizujParametry(punkt As PunktXY, piksel As Double, uklad As UkladWspolrzednych) As Boolean
        Dim zmieniono As Boolean = False
        If Not _punktProbki.HasValue OrElse _ostatniEpsg <> uklad.Epsg OrElse Math.Abs(_ostatniPiksel - piksel) > 0.0001 Then
            zmieniono = True
        Else
            Dim dx = Math.Abs(_punktProbki.Value.X - punkt.X)
            Dim dy = Math.Abs(_punktProbki.Value.Y - punkt.Y)
            If dx > (piksel * 2) OrElse dy > (piksel * 2) Then
                zmieniono = True
            End If
        End If

        If zmieniono Then
            _buforWarstw.Clear()
            'usługa WMTS przygotowana dla poprzedniego układu lub piksela (plan kafli) dawała pusty podgląd
            'po zmianie układu - np. ortofotomapa znikała z podglądu w WGS84 bez komunikatu o błędzie.
            'Pliki kafli usuwane są dopiero przy zamknięciu - poprzednie pobieranie podglądu może jeszcze z nich korzystać.
            _uslugiDoUsuniecia.AddRange(_uslugiWmts.Values)
            _uslugiWmts.Clear()
            _punktProbki = punkt
            _ostatniPiksel = piksel
            _ostatniEpsg = uklad.Epsg
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
    ''' Skaluje obraz podglądu jako lupę rastrową (centrowanie w docelowym obszarze).
    ''' Dla powiększenia >= 1.0 stosuje NearestNeighbor i PixelOffsetMode.Half, aby użytkownik widział rzeczywiste piksele pobierania.
    ''' Dla pomniejszenia &lt; 1.0 stosuje wygładzanie HighQualityBicubic.
    ''' </summary>
    Public Shared Function ZastosujZoom(zrodlo As Bitmap, zoom As Double, celSzer As Integer, celWys As Integer) As Bitmap
        If zrodlo Is Nothing Then Return Nothing
        If Math.Abs(zoom - 1.0) < 0.001 AndAlso zrodlo.Width = celSzer AndAlso zrodlo.Height = celWys Then
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
            Dim destX = (celSzer - scaledW) \ 2
            Dim destY = (celWys - scaledH) \ 2

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

        Dim piksele(BokPodgladu * BokPodgladu - 1) As Integer
        ' Tło szachownicy 8x8 px dla wizualizacji przezroczystości
        For y = 0 To BokPodgladu - 1
            For x = 0 To BokPodgladu - 1
                Dim jasny = ((x \ 8) + (y \ 8)) Mod 2 = 0
                piksele(y * BokPodgladu + x) = If(jasny, &HFFFFFFFF, &HFFF2F2F2)
            Next
        Next

        For Each w In zestaw.Warstwy
            Dim obraz() As Integer = Nothing
            If _buforWarstw.TryGetValue(Klucz(w), obraz) Then
                NakladanieWarstw.Naloz(piksele, obraz, w.Krycie, w.TrybMieszania)
            End If
        Next

        Return NakladanieObrazow.DoBitmapy(piksele, BokPodgladu, BokPodgladu)
    End Function

    ''' <summary>
    ''' Pobiera asynchronicznie brakujące warstwy zestawu i zwraca złożoną bitmapę oraz listę błędów.
    ''' </summary>
    Public Async Function PobierzPodgladAsync(zadanie As ZadaniePobierania, zestaw As ZestawWarstw, zasieg As Zasieg,
                                             token As CancellationToken, Optional postep As IProgress(Of String) = Nothing) As Task(Of Tuple(Of Bitmap, List(Of String)))
        Dim bledy As New List(Of String)()
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then
            Return Tuple.Create(CType(Nothing, Bitmap), bledy)
        End If

        Dim listaWarstw = zestaw.Warstwy.ToList()
        For i = 0 To listaWarstw.Count - 1
            token.ThrowIfCancellationRequested()
            Dim w = listaWarstw(i)
            Dim nazwa = w.Nazwa
            If _buforWarstw.ContainsKey(Klucz(w)) Then Continue For

            postep?.Report("Pobieranie " & nazwa & "…")
            Dim piksele() As Integer = Nothing
            Dim blad As String = ""

            If w.CzyKafelkowa Then
                Dim usluga As ZrodloWmts = Nothing
                If Not _uslugiWmts.TryGetValue(w.Adres, usluga) Then
                    Dim nazwyUslugi = zestaw.Warstwy.Where(Function(x) String.Equals(x.Adres, w.Adres, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Nazwa).ToList()
                    Dim zrodloPliku As PlikWarstw = Nothing
                    If Not String.IsNullOrEmpty(w.Zbior) AndAlso zadanie.ZrodlaMap IsNot Nothing Then
                        zadanie.ZrodlaMap.TryGetValue(w.Zbior, zrodloPliku)
                    End If
                    If zrodloPliku Is Nothing Then zrodloPliku = zadanie.ZnajdzZrodlo(-1, w.Adres)
                    usluga = Await ZrodloWmts.PrzygotujAsync(zadanie, w.Adres, nazwyUslugi, token,
                                                             typUslugi:=w.EfektywnyTyp, plikWarstw:=zrodloPliku)
                    _uslugiWmts(w.Adres) = usluga
                End If
                Dim wynikWmts = Await usluga.PikseleAsync({nazwa}, zasieg, BokPodgladu, token)
                piksele = wynikWmts.Item1
                blad = wynikWmts.Item2
            Else
                Dim czystyAdres As String = w.Adres.Trim(""""c, " "c)
                Dim ignorujSsl As Boolean = w.IgnorujSsl OrElse zadanie.IgnorujSsl
                Dim przezroczysty As Boolean = i > 0 OrElse zadanie.Przezroczystosc
                Dim formatWms As String = If(przezroczysty, "png", zadanie.Format)
                Dim adres As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, zadanie.Uklad, zasieg, formatWms,
                                                         BokPodgladu, BokPodgladu, zadanie.ZamienOsie, przezroczysty)
                Dim wynik = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adres, Math.Max(5, zadanie.LimitCzasuSekundy), token, ignorujSsl)

                ' Jeśli serwer odrzucił przezroczyste png, fallback na jpeg
                If wynik.Item1 Is Nothing AndAlso przezroczysty Then
                    Dim adresJpg As String = Wms.ZapytanieGetMap(czystyAdres, {nazwa}, zadanie.Uklad, zasieg, "jpeg",
                                                                 BokPodgladu, BokPodgladu, zadanie.ZamienOsie, False)
                    Dim wynikJpg = Await PobieranieSegmentow.PobierzObrazBezRegulatoraAsync(adresJpg, Math.Max(5, zadanie.LimitCzasuSekundy), token, ignorujSsl)
                    If wynikJpg.Item1 IsNot Nothing Then
                        wynik = wynikJpg
                    End If
                End If

                blad = wynik.Item2
                piksele = If(blad = "", NakladanieObrazow.Piksele(wynik.Item1, BokPodgladu, BokPodgladu), Nothing)
            End If

            If blad <> "" Then
                bledy.Add(nazwa & ": " & blad)
            ElseIf piksele IsNot Nothing Then
                _buforWarstw(Klucz(w)) = piksele
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
