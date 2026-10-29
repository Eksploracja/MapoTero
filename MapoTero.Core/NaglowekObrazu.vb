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

''' <summary>
''' Wymiary obrazu i georeferencja zapisana w pliku GeoTIFF odczytane z samego nagłówka pliku (PNG, JPEG, GIF,
''' TIFF i BigTIFF) - bez dekodowania obrazu, więc także dla arkuszy, które nie zmieściłyby się w pamięci.
''' </summary>
Public Class NaglowekObrazu

    Public Property SzerokoscPx As Long
    Public Property WysokoscPx As Long
    ''' <summary>Plik TIFF w formacie BigTIFF (powyżej 4 GB) - nie obsługują go programy CAD.</summary>
    Public Property BigTiff As Boolean
    ''' <summary>Znacznik GeoTIFF ModelPixelScale (rozmiar piksela w poziomie, w pionie, 0) albo Nothing.</summary>
    Public Property SkalaPiksela As Double()
    ''' <summary>Znacznik GeoTIFF ModelTiepoint (piksel i, j, k i jego współrzędne x, y, z) albo Nothing.</summary>
    Public Property PunktDowiazania As Double()

    Private Const TagSzerokosc As Integer = 256
    Private Const TagWysokosc As Integer = 257
    Private Const TagSkalaPiksela As Integer = 33550
    Private Const TagPunktDowiazania As Integer = 33922
    Private Const TagKluczeGeo As Integer = 34735

    ''' <summary>Kod EPSG układu współrzędnych zapisany w GeoTIFF (0 - brak lub układ niestandardowy).</summary>
    Public Property Epsg As Integer
    ''' <summary>
    ''' GTRasterTypeGeoKey = PixelIsPoint: punkt dowiązania wskazuje środek piksela, a nie jego narożnik (PixelIsArea,
    ''' wartość domyślna - tak zapisują MapoTero i GDAL).
    ''' </summary>
    Public Property PikselJakoPunkt As Boolean

    ''' <summary>Czy plik GeoTIFF zawiera położenie obrazu (rozmiar piksela i punkt dowiązania).</summary>
    Public ReadOnly Property MaGeoreferencje As Boolean
        Get
            Return SkalaPiksela IsNot Nothing AndAlso SkalaPiksela.Length >= 2 AndAlso
                   PunktDowiazania IsNot Nothing AndAlso PunktDowiazania.Length >= 6 AndAlso
                   SkalaPiksela(0) > 0 AndAlso SkalaPiksela(1) > 0
        End Get
    End Property

    ''' <summary>
    ''' Zasięg obrazu z georeferencji GeoTIFF (zewnętrzne krawędzie skrajnych pikseli). Przy PixelIsPoint punkt
    ''' dowiązania jest środkiem piksela - narożnik obrazu leży pół piksela dalej. X - północ, Y - wschód.
    ''' </summary>
    Public Function ZasiegGeoTiff() As Zasieg
        Dim t = PunktDowiazania, s = SkalaPiksela
        Dim polowa As Double = If(PikselJakoPunkt, 0.5, 0)
        Dim yLewy As Double = t(3) - (t(0) + polowa) * s(0)
        Dim xGora As Double = t(4) + (t(1) + polowa) * s(1)
        Return New Zasieg(xGora - WysokoscPx * s(1), yLewy, xGora, yLewy + SzerokoscPx * s(0))
    End Function

    ''' <summary>Odczytuje nagłówek pliku obrazu; wyjątek InvalidDataException, gdy format jest nieobsługiwany lub plik uszkodzony.</summary>
    Public Shared Function Odczytaj(sciezka As String) As NaglowekObrazu
        Using s As New FileStream(sciezka, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Return Odczytaj(s)
        End Using
    End Function

    Public Shared Function Odczytaj(s As Stream) As NaglowekObrazu
        Dim sygnatura(7) As Byte
        Dim n As Integer = CzytajPelne(s, sygnatura, 0, 8)
        If n >= 8 AndAlso sygnatura(0) = &H89 AndAlso sygnatura(1) = AscW("P"c) AndAlso sygnatura(2) = AscW("N"c) AndAlso sygnatura(3) = AscW("G"c) Then
            Return Png(s)
        End If
        If n >= 3 AndAlso sygnatura(0) = &HFF AndAlso sygnatura(1) = &HD8 Then Return Jpeg(s)
        If n >= 6 AndAlso sygnatura(0) = AscW("G"c) AndAlso sygnatura(1) = AscW("I"c) AndAlso sygnatura(2) = AscW("F"c) Then
            s.Position = 6
            Dim b(3) As Byte
            If CzytajPelne(s, b, 0, 4) < 4 Then Throw New InvalidDataException("Uszkodzony plik GIF")
            Return New NaglowekObrazu With {.SzerokoscPx = b(0) Or (CInt(b(1)) << 8), .WysokoscPx = b(2) Or (CInt(b(3)) << 8)}
        End If
        If n >= 4 AndAlso ((sygnatura(0) = AscW("I"c) AndAlso sygnatura(1) = AscW("I"c)) OrElse (sygnatura(0) = AscW("M"c) AndAlso sygnatura(1) = AscW("M"c))) Then
            Return Tiff(s)
        End If
        Throw New InvalidDataException("Nieobsługiwany format obrazu (oczekiwano TIFF, JPEG, PNG lub GIF)")
    End Function

    Private Shared Function Png(s As Stream) As NaglowekObrazu
        'pierwszy fragment: długość (4), "IHDR" (4), szerokość i wysokość (big-endian)
        s.Position = 8
        Dim b(15) As Byte
        If CzytajPelne(s, b, 0, 16) < 16 OrElse b(4) <> AscW("I"c) OrElse b(5) <> AscW("H"c) Then Throw New InvalidDataException("Uszkodzony plik PNG")
        Return New NaglowekObrazu With {.SzerokoscPx = LiczbaBE(b, 8, 4), .WysokoscPx = LiczbaBE(b, 12, 4)}
    End Function

    Private Shared Function Jpeg(s As Stream) As NaglowekObrazu
        s.Position = 2
        Dim b(7) As Byte
        Do
            'znacznik: FF xx (z ewentualnymi bajtami wypełnienia FF)
            Dim z As Integer = s.ReadByte()
            If z < 0 Then Exit Do
            If z <> &HFF Then Continue Do
            Dim typ As Integer
            Do
                typ = s.ReadByte()
            Loop While typ = &HFF
            If typ < 0 Then Exit Do
            If typ = &HD8 OrElse typ = &H1 OrElse (typ >= &HD0 AndAlso typ <= &HD7) Then Continue Do
            If typ = &HD9 OrElse typ = &HDA Then Exit Do
            If CzytajPelne(s, b, 0, 2) < 2 Then Exit Do
            Dim dlugosc As Integer = CInt(LiczbaBE(b, 0, 2))
            'SOF0-SOF15 z wyjątkiem DHT (C4), JPG (C8) i DAC (CC): precyzja, wysokość, szerokość
            If typ >= &HC0 AndAlso typ <= &HCF AndAlso typ <> &HC4 AndAlso typ <> &HC8 AndAlso typ <> &HCC Then
                If CzytajPelne(s, b, 0, 5) < 5 Then Exit Do
                Return New NaglowekObrazu With {.WysokoscPx = LiczbaBE(b, 1, 2), .SzerokoscPx = LiczbaBE(b, 3, 2)}
            End If
            If dlugosc < 2 Then Exit Do
            s.Seek(dlugosc - 2, SeekOrigin.Current)
        Loop
        Throw New InvalidDataException("Nie znaleziono wymiarów obrazu w pliku JPEG")
    End Function

    ''' <summary>TIFF i BigTIFF: znaczniki pierwszego katalogu (IFD) - wymiary i georeferencja GeoTIFF.</summary>
    Private Shared Function Tiff(s As Stream) As NaglowekObrazu
        s.Position = 0
        Dim nagl(15) As Byte
        If CzytajPelne(s, nagl, 0, 16) < 8 Then Throw New InvalidDataException("Uszkodzony plik TIFF")
        Dim le As Boolean = nagl(0) = AscW("I"c)
        Dim wersja As Long = Liczba(nagl, 2, 2, le)
        Dim big As Boolean
        Dim ifd As Long
        If wersja = 42 Then
            ifd = Liczba(nagl, 4, 4, le)
        ElseIf wersja = 43 Then
            big = True
            ifd = Liczba(nagl, 8, 8, le)
        Else
            Throw New InvalidDataException("Uszkodzony plik TIFF")
        End If

        Dim wynik As New NaglowekObrazu With {.BigTiff = big}
        Dim rozmiarWpisu As Integer = If(big, 20, 12)
        Dim b(7) As Byte
        s.Position = ifd
        If CzytajPelne(s, b, 0, If(big, 8, 2)) < If(big, 8, 2) Then Throw New InvalidDataException("Uszkodzony plik TIFF")
        Dim liczbaWpisow As Long = Liczba(b, 0, If(big, 8, 2), le)
        If liczbaWpisow <= 0 OrElse liczbaWpisow > 10000 Then Throw New InvalidDataException("Uszkodzony plik TIFF")
        Dim wpisy(CInt(liczbaWpisow) * rozmiarWpisu - 1) As Byte
        If CzytajPelne(s, wpisy, 0, wpisy.Length) < wpisy.Length Then Throw New InvalidDataException("Uszkodzony plik TIFF")

        For i = 0 To CInt(liczbaWpisow) - 1
            Dim p As Integer = i * rozmiarWpisu
            Dim tag As Integer = CInt(Liczba(wpisy, p, 2, le))
            Dim typ As Integer = CInt(Liczba(wpisy, p + 2, 2, le))
            Dim ile As Long = Liczba(wpisy, p + 4, If(big, 8, 4), le)
            Dim pWartosci As Integer = p + If(big, 12, 8)
            Select Case tag
                Case TagSzerokosc, TagWysokosc
                    'SHORT (3), LONG (4) albo LONG8 (16)
                    Dim bajty As Integer = If(typ = 3, 2, If(typ = 16, 8, 4))
                    Dim v As Long = Liczba(wpisy, pWartosci, bajty, le)
                    If tag = TagSzerokosc Then wynik.SzerokoscPx = v Else wynik.WysokoscPx = v
                Case TagSkalaPiksela, TagPunktDowiazania
                    If typ <> 12 OrElse ile <= 0 OrElse ile > 600 Then Continue For    'DOUBLE
                    Dim dane = DaneZnacznika(s, wpisy, pWartosci, CInt(ile) * 8, big, le)
                    Dim wartosci(CInt(ile) - 1) As Double
                    For j = 0 To wartosci.Length - 1
                        Dim kawalek(7) As Byte
                        Array.Copy(dane, j * 8, kawalek, 0, 8)
                        If BitConverter.IsLittleEndian <> le Then Array.Reverse(kawalek)
                        wartosci(j) = BitConverter.ToDouble(kawalek, 0)
                    Next
                    If tag = TagSkalaPiksela Then wynik.SkalaPiksela = wartosci Else wynik.PunktDowiazania = wartosci
                Case TagKluczeGeo
                    'GeoKeyDirectory: nagłówek i wpisy po 4 liczby SHORT (klucz, położenie, liczba, wartość)
                    If typ <> 3 OrElse ile < 4 OrElse ile > 4000 Then Continue For
                    Dim dane = DaneZnacznika(s, wpisy, pWartosci, CInt(ile) * 2, big, le)
                    For j = 4 To CInt(ile) - 4 Step 4
                        Dim klucz As Long = Liczba(dane, j * 2, 2, le)
                        Dim polozenie As Long = Liczba(dane, j * 2 + 2, 2, le)
                        Dim wartosc As Long = Liczba(dane, j * 2 + 6, 2, le)
                        'ProjectedCSTypeGeoKey albo GeographicTypeGeoKey zapisany bezpośrednio w katalogu
                        If polozenie = 0 AndAlso wartosc > 0 AndAlso wartosc < 32767 AndAlso (klucz = 3072 OrElse (klucz = 2048 AndAlso wynik.Epsg = 0)) Then
                            wynik.Epsg = CInt(wartosc)
                        End If
                        'GTRasterTypeGeoKey: 1 - PixelIsArea, 2 - PixelIsPoint
                        If klucz = 1025 AndAlso polozenie = 0 Then wynik.PikselJakoPunkt = (wartosc = 2)
                    Next
            End Select
        Next
        If wynik.SzerokoscPx <= 0 OrElse wynik.WysokoscPx <= 0 Then Throw New InvalidDataException("Plik TIFF nie zawiera wymiarów obrazu")
        Return wynik
    End Function

    ''' <summary>Wartości znacznika TIFF: zapisane w samym wpisie katalogu (gdy się mieszczą) albo pod wskazanym przesunięciem.</summary>
    Private Shared Function DaneZnacznika(s As Stream, wpisy() As Byte, pWartosci As Integer, rozmiar As Integer, big As Boolean, le As Boolean) As Byte()
        Dim dane(rozmiar - 1) As Byte
        Dim wPolu As Integer = If(big, 8, 4)
        If rozmiar <= wPolu Then
            Array.Copy(wpisy, pWartosci, dane, 0, rozmiar)
        Else
            Dim poz As Long = s.Position
            s.Position = Liczba(wpisy, pWartosci, wPolu, le)
            If CzytajPelne(s, dane, 0, rozmiar) < rozmiar Then Throw New InvalidDataException("Uszkodzony plik TIFF")
            s.Position = poz
        End If
        Return dane
    End Function

    Private Shared Function CzytajPelne(s As Stream, bufor() As Byte, poz As Integer, ile As Integer) As Integer
        Dim razem As Integer = 0
        While razem < ile
            Dim n As Integer = s.Read(bufor, poz + razem, ile - razem)
            If n <= 0 Then Exit While
            razem += n
        End While
        Return razem
    End Function

    Private Shared Function LiczbaBE(b() As Byte, poz As Integer, ile As Integer) As Long
        Return Liczba(b, poz, ile, False)
    End Function

    Private Shared Function Liczba(b() As Byte, poz As Integer, ile As Integer, le As Boolean) As Long
        Dim v As Long = 0
        For i = 0 To ile - 1
            Dim bajt As Long = b(poz + If(le, ile - 1 - i, i))
            v = (v << 8) Or bajt
        Next
        Return v
    End Function

End Class

''' <summary>Odczyt plików world file (.jgw, .jpgw, .tfw, .tifw, .pgw, .pngw).</summary>
Public NotInheritable Class WorldFile

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Zasięg obrazu (zewnętrzne krawędzie skrajnych pikseli) z treści world file: rozmiar piksela, dwa wyrazy obrotu
    ''' i współrzędne środka lewego górnego piksela (wschód, północ). Wyjątek InvalidDataException dla obrazu obróconego.
    ''' </summary>
    Public Shared Function Zasieg(tresc As String, szerokoscPx As Long, wysokoscPx As Long) As Zasieg
        Dim wiersze = tresc.Split({ChrW(13), ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
        Dim v As New List(Of Double)
        For Each w In wiersze
            If w.Trim() = "" Then Continue For
            Dim d As Double
            If Not Double.TryParse(w.Trim().Replace(","c, "."c), NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                Throw New InvalidDataException("Niepoprawny wiersz pliku world file: " & w.Trim())
            End If
            v.Add(d)
        Next
        If v.Count < 6 Then Throw New InvalidDataException("Plik world file powinien zawierać 6 liczb")
        Dim a As Double = v(0), d1 As Double = v(1), b As Double = v(2), e As Double = v(3), c As Double = v(4), f As Double = v(5)
        If d1 <> 0 OrElse b <> 0 Then Throw New InvalidDataException("Obraz jest obrócony (wyrazy obrotu w pliku world file) - nie można go wstawić jako prostokąta")
        If a <= 0 OrElse e >= 0 Then Throw New InvalidDataException("Niepoprawny rozmiar piksela w pliku world file")
        Dim yLewy As Double = c - a / 2
        Dim xGora As Double = f - e / 2
        Return New Zasieg(xGora + e * wysokoscPx, yLewy, xGora, yLewy + a * szerokoscPx)
    End Function

    ''' <summary>Ścieżka pliku world file towarzyszącego obrazowi albo pusty tekst, gdy go nie ma.</summary>
    Public Shared Function ZnajdzPlik(sciezkaObrazu As String) As String
        Dim rozszerzenie As String = Path.GetExtension(sciezkaObrazu).TrimStart("."c).ToLowerInvariant()
        Dim baza As String = Path.Combine(Path.GetDirectoryName(sciezkaObrazu), Path.GetFileNameWithoutExtension(sciezkaObrazu))
        Dim kandydaci As New List(Of String)
        kandydaci.Add(Georeferencja.RozszerzenieWorldFile(rozszerzenie))
        If rozszerzenie.Length >= 3 Then kandydaci.Add(rozszerzenie.Substring(0, 1) & rozszerzenie.Substring(rozszerzenie.Length - 1) & "w")
        kandydaci.Add(rozszerzenie & "w")
        'plik .wld programu MapoTero zawiera współrzędne WGS84 (dla .points), a nie układu obrazu - pomijany
        For Each k In kandydaci
            Dim plik As String = baza & "." & k
            If File.Exists(plik) Then Return plik
        Next
        Return ""
    End Function

End Class
