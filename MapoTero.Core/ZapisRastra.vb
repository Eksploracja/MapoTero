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

Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Threading
Imports BitMiracle.LibJpeg.Classic
Imports BitMiracle.LibTiff.Classic

''' <summary>Format pliku scalonego arkusza.</summary>
Public Enum FormatArkusza
    ''' <summary>GeoTIFF z kompresją bezstratną (Deflate) i georeferencją w pliku.</summary>
    GeoTiff
    ''' <summary>GeoTIFF z kompresją JPEG (mniejszy plik, kompresja stratna).</summary>
    GeoTiffJpeg
    ''' <summary>JPEG (maks. 65500 x 65500 pikseli).</summary>
    Jpeg
    ''' <summary>PNG (bezstratny).</summary>
    Png
End Enum

''' <summary>
''' Strumieniowy zapis dużego obrazu RGB (24 bity) - wiersze dopisywane kolejno od góry, bez trzymania całego
''' obrazu w pamięci. Zastępuje zewnętrzny moduł NoToCONS.exe, którego wydajność zależała od ilości pamięci RAM.
''' </summary>
Public MustInherit Class ZapisRastra
    Implements IDisposable

    Public ReadOnly Property Szerokosc As Integer
    Public ReadOnly Property Wysokosc As Integer
    ''' <summary>Liczba kanałów piksela: 3 - RGB, 4 - RGBA (kanał przezroczystości).</summary>
    Public ReadOnly Property Kanaly As Integer
    ''' <summary>Liczba zapisanych dotąd wierszy.</summary>
    Public Property Zapisane As Integer

    Protected Sub New(szerokosc As Integer, wysokosc As Integer, Optional kanaly As Integer = 3)
        If szerokosc <= 0 OrElse wysokosc <= 0 Then Throw New ArgumentException("Niepoprawny rozmiar obrazu")
        If kanaly <> 3 AndAlso kanaly <> 4 Then Throw New ArgumentException("Obsługiwane są obrazy RGB i RGBA")
        Me.Szerokosc = szerokosc
        Me.Wysokosc = wysokosc
        Me.Kanaly = kanaly
    End Sub

    ''' <summary>Czy format arkusza może zachować przezroczystość (kanał alfa).</summary>
    Public Shared Function ObslugujePrzezroczystosc(format As FormatArkusza) As Boolean
        Return format = FormatArkusza.Png OrElse format = FormatArkusza.GeoTiff
    End Function

    ''' <summary>Zapisuje kolejne wiersze: bufor RGB (RGBA) o długości liczbaWierszy * Szerokosc * Kanaly, wierszami od góry.</summary>
    Public Sub ZapiszWiersze(rgb() As Byte, liczbaWierszy As Integer)
        If liczbaWierszy < 0 Then Throw New ArgumentOutOfRangeException(NameOf(liczbaWierszy))
        If Zapisane + liczbaWierszy > Wysokosc Then Throw New InvalidOperationException("Za dużo wierszy obrazu")
        ZapiszWierszeWewn(rgb, liczbaWierszy)
        Zapisane += liczbaWierszy
    End Sub

    Protected MustOverride Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)

    ''' <summary>Kończy zapis pliku (wymaga zapisania wszystkich wierszy).</summary>
    Public MustOverride Sub Zakoncz()

    Public MustOverride Sub Dispose() Implements IDisposable.Dispose

    ''' <summary>Rozszerzenie pliku dla formatu.</summary>
    Public Shared Function Rozszerzenie(format As FormatArkusza) As String
        Select Case format
            Case FormatArkusza.Jpeg : Return "jpg"
            Case FormatArkusza.Png : Return "png"
            Case Else : Return "tif"
        End Select
    End Function

    ''' <summary>Maksymalny rozmiar obrazu w danym formacie (w pikselach, w każdym wymiarze).</summary>
    Public Shared Function MaksymalnyBok(format As FormatArkusza) As Integer
        Select Case format
            Case FormatArkusza.Jpeg : Return 65500
            Case Else : Return Integer.MaxValue
        End Select
    End Function

    ''' <summary>
    ''' Tworzy zapis obrazu w wybranym formacie. Przezroczystość (RGBA) zachowują PNG i GeoTIFF bezstratny;
    ''' formaty z kompresją JPEG zapisują zawsze RGB. Dla GeoTIFF opcja piramidy tworzy wewnętrzne katalogi
    ''' pomniejszeń (overviews) dla szybkiego wyświetlania w QGIS i Avenza Maps.
    ''' </summary>
    Public Shared Function Utworz(format As FormatArkusza, sciezka As String, szerokosc As Integer, wysokosc As Integer, jakoscJpeg As Integer,
                                  uklad As UkladWspolrzednych, zasieg As Zasieg, Optional przezroczystosc As Boolean = False,
                                  Optional piramidy As Boolean = True) As ZapisRastra
        Dim kanaly As Integer = If(przezroczystosc AndAlso ObslugujePrzezroczystosc(format), 4, 3)
        Select Case format
            Case FormatArkusza.Jpeg : Return New ZapisJpeg(sciezka, szerokosc, wysokosc, jakoscJpeg)
            Case FormatArkusza.Png : Return New ZapisPng(sciezka, szerokosc, wysokosc, kanaly)
            Case FormatArkusza.GeoTiffJpeg : Return New ZapisGeoTiff(sciezka, szerokosc, wysokosc, True, jakoscJpeg, uklad, zasieg, 3, piramidy)
            Case Else : Return New ZapisGeoTiff(sciezka, szerokosc, wysokosc, False, jakoscJpeg, uklad, zasieg, kanaly, piramidy)
        End Select
    End Function

End Class

''' <summary>Strumieniowy zapis PNG (kolor RGB lub RGBA 8 bitów, filtr Up, kompresja Deflate).</summary>
Public Class ZapisPng
    Inherits ZapisRastra

    Private ReadOnly _plik As FileStream
    Private ReadOnly _idat As StrumienFragmentow
    Private ReadOnly _deflate As DeflateStream
    Private _poprzedni() As Byte
    Private _wiersz() As Byte
    Private _adlerA As UInteger = 1
    Private _adlerB As UInteger = 0
    Private _zakonczony As Boolean

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer, Optional kanaly As Integer = 3)
        MyBase.New(szerokosc, wysokosc, kanaly)
        _plik = New FileStream(sciezka, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16)
        Try
            _plik.Write(New Byte() {137, 80, 78, 71, 13, 10, 26, 10}, 0, 8)
            Dim ihdr(12) As Byte
            ZapiszBE(ihdr, 0, CUInt(szerokosc))
            ZapiszBE(ihdr, 4, CUInt(wysokosc))
            ihdr(8) = 8   'głębia bitowa
            ihdr(9) = CByte(If(kanaly = 4, 6, 2))   'RGBA albo RGB
            ihdr(10) = 0 : ihdr(11) = 0 : ihdr(12) = 0
            ZapiszFragment(_plik, "IHDR", ihdr, ihdr.Length)

            _idat = New StrumienFragmentow(_plik, "IDAT")
            'nagłówek zlib (deflate, okno 32 kB)
            _idat.Write(New Byte() {&H78, &H9C}, 0, 2)
            _deflate = New DeflateStream(_idat, CompressionLevel.Optimal, True)
            _poprzedni = New Byte(szerokosc * kanaly - 1) {}
            _wiersz = New Byte(szerokosc * kanaly) {}
        Catch
            _deflate?.Dispose()
            _plik?.Dispose()
            Throw
        End Try
    End Sub

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim dl As Integer = Szerokosc * Kanaly
        For w = 0 To liczbaWierszy - 1
            Dim poz As Integer = w * dl
            _wiersz(0) = 2   'filtr Up - różnica względem wiersza powyżej (dobra kompresja map)
            For i = 0 To dl - 1
                Dim b As Byte = rgb(poz + i)
                _wiersz(i + 1) = CByte((CInt(b) - CInt(_poprzedni(i))) And &HFF)
                _poprzedni(i) = b
            Next
            Adler(_wiersz, _wiersz.Length)
            _deflate.Write(_wiersz, 0, _wiersz.Length)
        Next
    End Sub

    Private Sub Adler(dane() As Byte, n As Integer)
        Const modulo As UInteger = 65521
        Dim i As Integer = 0
        While i < n
            Dim k As Integer = Math.Min(n - i, 5552)
            For j = 0 To k - 1
                _adlerA += dane(i + j)
                _adlerB += _adlerA
            Next
            _adlerA = _adlerA Mod modulo
            _adlerB = _adlerB Mod modulo
            i += k
        End While
    End Sub

    Public Overrides Sub Zakoncz()
        If _zakonczony Then Return
        If Zapisane <> Wysokosc Then Throw New InvalidOperationException("Nie zapisano wszystkich wierszy obrazu")
        _deflate.Dispose()
        Dim adler(3) As Byte
        ZapiszBE(adler, 0, (_adlerB << 16) Or _adlerA)
        _idat.Write(adler, 0, 4)
        _idat.Flush()
        ZapiszFragment(_plik, "IEND", New Byte() {}, 0)
        _plik.Flush()
        _zakonczony = True
    End Sub

    Public Overrides Sub Dispose()
        If Not _zakonczony Then
            Try
                _deflate.Dispose()
            Catch
            End Try
        End If
        _plik.Dispose()
    End Sub

    Friend Shared Sub ZapiszBE(bufor() As Byte, poz As Integer, wartosc As UInteger)
        bufor(poz) = CByte((wartosc >> 24) And &HFFUI)
        bufor(poz + 1) = CByte((wartosc >> 16) And &HFFUI)
        bufor(poz + 2) = CByte((wartosc >> 8) And &HFFUI)
        bufor(poz + 3) = CByte(wartosc And &HFFUI)
    End Sub

    Friend Shared Sub ZapiszFragment(s As Stream, typ As String, dane() As Byte, n As Integer)
        Dim naglowek(7) As Byte
        ZapiszBE(naglowek, 0, CUInt(n))
        Dim t() As Byte = Encoding.ASCII.GetBytes(typ)
        Array.Copy(t, 0, naglowek, 4, 4)
        s.Write(naglowek, 0, 8)
        If n > 0 Then s.Write(dane, 0, n)
        Dim crc As UInteger = Crc32.Oblicz(Crc32.Oblicz(&HFFFFFFFFUI, t, 4), dane, n) Xor &HFFFFFFFFUI
        Dim c(3) As Byte
        ZapiszBE(c, 0, crc)
        s.Write(c, 0, 4)
    End Sub

    ''' <summary>Strumień dzielący skompresowane dane na fragmenty IDAT pliku PNG.</summary>
    Private Class StrumienFragmentow
        Inherits Stream

        Private ReadOnly _cel As Stream
        Private ReadOnly _typ As String
        Private ReadOnly _bufor(65535) As Byte
        Private _ile As Integer

        Public Sub New(cel As Stream, typ As String)
            _cel = cel
            _typ = typ
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            While count > 0
                Dim k As Integer = Math.Min(count, _bufor.Length - _ile)
                System.Buffer.BlockCopy(buffer, offset, _bufor, _ile, k)
                _ile += k : offset += k : count -= k
                If _ile = _bufor.Length Then Flush()
            End While
        End Sub

        Public Overrides Sub Flush()
            If _ile > 0 Then
                ZapiszFragment(_cel, _typ, _bufor, _ile)
                _ile = 0
            End If
        End Sub

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return False
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotSupportedException()
            End Get
        End Property
        Public Overrides Property Position As Long
            Get
                Throw New NotSupportedException()
            End Get
            Set(value As Long)
                Throw New NotSupportedException()
            End Set
        End Property
        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Throw New NotSupportedException()
        End Function
        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function
        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub
    End Class

End Class

''' <summary>Suma kontrolna CRC-32 (fragmenty plików PNG).</summary>
Friend NotInheritable Class Crc32

    Private Sub New()
    End Sub

    Private Shared ReadOnly Tablica As UInteger() = UtworzTablice()

    Private Shared Function UtworzTablice() As UInteger()
        Dim t(255) As UInteger
        For n = 0 To 255
            Dim c As UInteger = CUInt(n)
            For k = 0 To 7
                If (c And 1UI) <> 0 Then
                    c = &HEDB88320UI Xor (c >> 1)
                Else
                    c >>= 1
                End If
            Next
            t(n) = c
        Next
        Return t
    End Function

    ''' <summary>Aktualizuje CRC (bez końcowej negacji).</summary>
    Public Shared Function Oblicz(crc As UInteger, dane() As Byte, n As Integer) As UInteger
        Dim c As UInteger = crc
        For i = 0 To n - 1
            c = Tablica(CInt((c Xor dane(i)) And &HFFUI)) Xor (c >> 8)
        Next
        Return c
    End Function

End Class

''' <summary>Strumieniowy zapis JPEG (biblioteka LibJpeg.NET).</summary>
Public Class ZapisJpeg
    Inherits ZapisRastra

    Private ReadOnly _plik As FileStream
    Private ReadOnly _jpeg As jpeg_compress_struct
    Private ReadOnly _wierszBufor(0)() As Byte
    Private _zakonczony As Boolean

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer, jakosc As Integer)
        MyBase.New(szerokosc, wysokosc)
        If szerokosc > 65500 OrElse wysokosc > 65500 Then
            Throw New ArgumentException("Format JPEG pozwala na obraz o boku najwyżej 65500 pikseli - wybierz GeoTIFF lub PNG")
        End If
        _plik = New FileStream(sciezka, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16)
        Try
            _jpeg = New jpeg_compress_struct(New jpeg_error_mgr()) With {
                .Image_width = szerokosc, .Image_height = wysokosc, .Input_components = 3, .In_color_space = J_COLOR_SPACE.JCS_RGB}
            _jpeg.jpeg_set_defaults()
            _jpeg.jpeg_set_quality(Math.Max(1, Math.Min(100, jakosc)), True)
            _jpeg.jpeg_stdio_dest(_plik)
            _jpeg.jpeg_start_compress(True)
            _wierszBufor(0) = New Byte(szerokosc * 3 - 1) {}
        Catch
            _plik?.Dispose()
            Throw
        End Try
    End Sub

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim dl As Integer = Szerokosc * 3
        For w = 0 To liczbaWierszy - 1
            Buffer.BlockCopy(rgb, w * dl, _wierszBufor(0), 0, dl)
            _jpeg.jpeg_write_scanlines(_wierszBufor, 1)
        Next
    End Sub

    Public Overrides Sub Zakoncz()
        If _zakonczony Then Return
        If Zapisane <> Wysokosc Then Throw New InvalidOperationException("Nie zapisano wszystkich wierszy obrazu")
        _jpeg.jpeg_finish_compress()
        _plik.Flush()
        _zakonczony = True
    End Sub

    Public Overrides Sub Dispose()
        If Not _zakonczony AndAlso _jpeg IsNot Nothing Then
            Try
                _jpeg.jpeg_abort_compress()
            Catch
            End Try
        End If
        _plik?.Dispose()
    End Sub

End Class

''' <summary>
''' Strumieniowy zapis kafelkowanego GeoTIFF (256x256) z georeferencją w pliku, automatycznym BigTIFF
''' i wewnętrznymi piramidami (łańcuch IFD REDUCEDIMAGE).
''' Poziomy piramidy powstają w trakcie zapisu z nieskompresowanych wierszy (każdy poziom z poprzedniego, średnia 2x2)
''' i czekają w tymczasowych plikach TIFF z bezstratną kompresją Deflate; Zakoncz dopisuje je do pliku głównego.
''' Dlaczego tak: pierwsza wersja po zapisie czytała z pliku poprzedni poziom, więc przy kompresji JPEG każdy poziom
''' powstawał z już skompresowanego obrazu (strata jakości rosła z poziomem), a cały drugi przebieg szedł
''' bez postępu i bez możliwości przerwania.
''' </summary>
Public Class ZapisGeoTiff
    Inherits ZapisRastra

    Private Const ROZMIAR_KAFLA As Integer = 256
    Private Const PROG_BIGTIFF As Long = 3900000000L

    ' Znaczniki GeoTIFF
    Private Const TagModelPixelScale As Integer = 33550
    Private Const TagModelTiepoint As Integer = 33922
    Private Const TagGeoKeyDirectory As Integer = 34735
    Private Const TagGeoDoubleParams As Integer = 34736
    Private Const TagGeoAsciiParams As Integer = 34737

    Private Shared ReadOnly Blokada As New Object()
    Private Shared _rozszerzenieZarejestrowane As Boolean
    Private Shared _poprzednieRozszerzenie As Tiff.TiffExtendProc

    Private ReadOnly _sciezka As String
    Private ReadOnly _kompresjaJpeg As Boolean
    Private ReadOnly _jakosc As Integer
    Private ReadOnly _czyBigTiff As Boolean

    Private ReadOnly _pasBufor As Byte()
    Private ReadOnly _kafelBufor As Byte()
    Private _wierszyWPasie As Integer
    Private _tiff As Tiff
    Private _zakonczony As Boolean
    Private ReadOnly _poziomy As New List(Of PoziomPiramidy)

    ''' <summary>Postęp dopisywania piramid w Zakoncz, w procentach (0-100).</summary>
    Public Property PostepPiramid As Action(Of Integer)
    ''' <summary>Przerwanie dopisywania piramid w Zakoncz.</summary>
    Public Property Przerwanie As CancellationToken

    ''' <summary>Czy plik dostanie piramidy (wybrane i obraz większy niż jeden kafel).</summary>
    Public ReadOnly Property MaPiramidy As Boolean
        Get
            Return _poziomy.Count > 0
        End Get
    End Property

    ''' <summary>Jeden poziom piramidy: zbiera wiersze poprzedniego poziomu parami i zapisuje kafle do pliku tymczasowego.</summary>
    Private NotInheritable Class PoziomPiramidy
        Public SrcW As Integer
        Public SrcH As Integer
        Public W As Integer
        Public H As Integer
        Public Sciezka As String
        Public Tif As Tiff
        Public Pas As Byte()
        Public WierszyWPasie As Integer
        Public Zapisane As Integer
        Public Odebrane As Integer
        Public Oczekujacy As Byte()
        Public MaOczekujacy As Boolean
        Public Wiersz As Byte()
    End Class

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer, kompresjaJpeg As Boolean, jakosc As Integer,
                   uklad As UkladWspolrzednych, zasieg As Zasieg, Optional kanaly As Integer = 3, Optional piramidy As Boolean = True)
        MyBase.New(szerokosc, wysokosc, If(kompresjaJpeg, 3, kanaly))
        If String.IsNullOrWhiteSpace(sciezka) Then Throw New ArgumentException("Pusta ścieżka pliku.", NameOf(sciezka))
        If kompresjaJpeg AndAlso Me.Kanaly <> 3 Then
            Throw New ArgumentException("Kompresja JPEG wymaga 3 kanałów (RGB).", NameOf(kompresjaJpeg))
        End If

        ZarejestrujZnaczniki()

        _sciezka = sciezka
        _kompresjaJpeg = kompresjaJpeg
        _jakosc = Math.Clamp(jakosc, 10, 100)

        ' Decyzja BigTIFF z uwzględnieniem narzutu piramidy (ok. 1,35x względem bazy)
        Dim rozmiarBazowy As Long = CLng(szerokosc) * CLng(wysokosc) * CLng(Me.Kanaly)
        Dim rozmiarSzacowany As Long = If(piramidy, (rozmiarBazowy * 135L) \ 100L, rozmiarBazowy)
        _czyBigTiff = (rozmiarSzacowany > PROG_BIGTIFF)

        _pasBufor = New Byte(szerokosc * ROZMIAR_KAFLA * Me.Kanaly - 1) {}
        _kafelBufor = New Byte(ROZMIAR_KAFLA * ROZMIAR_KAFLA * Me.Kanaly - 1) {}

        Try
            _tiff = Tiff.Open(_sciezka, If(_czyBigTiff, "w8", "w"))
            If _tiff Is Nothing Then Throw New IOException("Nie można utworzyć pliku " & _sciezka)
            KonfigurujKatalog(_tiff, szerokosc, wysokosc, Me.Kanaly, kompresjaJpeg, _jakosc, czyPiramida:=False)
            ZapiszTagiGeoTiff(_tiff, szerokosc, wysokosc, uklad, zasieg)

            If piramidy Then
                Dim w = szerokosc, h = wysokosc, n = 1
                While Math.Max(w, h) > ROZMIAR_KAFLA
                    Dim p As New PoziomPiramidy With {
                        .SrcW = w, .SrcH = h, .W = Math.Max(1, (w + 1) \ 2), .H = Math.Max(1, (h + 1) \ 2),
                        .Sciezka = _sciezka & ".p" & n & ".tmp"}
                    p.Pas = New Byte(p.W * ROZMIAR_KAFLA * Me.Kanaly - 1) {}
                    p.Oczekujacy = New Byte(w * Me.Kanaly - 1) {}
                    p.Wiersz = New Byte(p.W * Me.Kanaly - 1) {}
                    _poziomy.Add(p)
                    p.Tif = Tiff.Open(p.Sciezka, If(_czyBigTiff, "w8", "w"))
                    If p.Tif Is Nothing Then Throw New IOException("Nie można utworzyć pliku tymczasowego " & p.Sciezka)
                    KonfigurujKatalog(p.Tif, p.W, p.H, Me.Kanaly, kompresjaJpeg:=False, jakosc:=_jakosc, czyPiramida:=False)
                    w = p.W : h = p.H : n += 1
                End While
            End If
        Catch
            Dispose()
            Throw
        End Try
    End Sub

    Private Shared Sub KonfigurujKatalog(tif As Tiff, szer As Integer, wys As Integer, kanaly As Integer,
                                         kompresjaJpeg As Boolean, jakosc As Integer, czyPiramida As Boolean)
        If czyPiramida Then
            tif.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE)
        End If
        tif.SetField(TiffTag.IMAGEWIDTH, szer)
        tif.SetField(TiffTag.IMAGELENGTH, wys)
        tif.SetField(TiffTag.BITSPERSAMPLE, 8)
        tif.SetField(TiffTag.SAMPLESPERPIXEL, kanaly)
        tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
        tif.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT)
        tif.SetField(TiffTag.TILEWIDTH, ROZMIAR_KAFLA)
        tif.SetField(TiffTag.TILELENGTH, ROZMIAR_KAFLA)
        tif.SetField(TiffTag.SOFTWARE, "MapoTero")

        If kompresjaJpeg Then
            tif.SetField(TiffTag.COMPRESSION, Compression.JPEG)
            tif.SetField(TiffTag.PHOTOMETRIC, Photometric.YCBCR)
            tif.SetField(TiffTag.JPEGQUALITY, jakosc)
            tif.SetField(TiffTag.JPEGCOLORMODE, JpegColorMode.RGB)
        Else
            tif.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
            tif.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL)
            tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
            If kanaly = 4 Then
                tif.SetField(TiffTag.EXTRASAMPLES, 1, New Short() {CShort(ExtraSample.UNASSALPHA)})
            End If
        End If
    End Sub

    Private Shared Sub ZapiszTagiGeoTiff(tif As Tiff, szer As Integer, wys As Integer, uklad As UkladWspolrzednych, zasieg As Zasieg)
        ' Georeferencja zapisywana wyłącznie w katalogu IFD 0
        Dim pikselY As Double = zasieg.Szerokosc / szer
        Dim pikselX As Double = zasieg.Wysokosc / wys
        tif.SetField(CType(TagModelPixelScale, TiffTag), 3, New Double() {pikselY, pikselX, 0.0})
        tif.SetField(CType(TagModelTiepoint, TiffTag), 6, New Double() {0.0, 0.0, 0.0, zasieg.YLewy, zasieg.XGora, 0.0})
        Dim klucze() As Short = PobierzKluczeGeo(uklad)
        tif.SetField(CType(TagGeoKeyDirectory, TiffTag), klucze.Length, klucze)
    End Sub

    Private Shared Function PobierzKluczeGeo(uklad As UkladWspolrzednych) As Short()
        If uklad.Geograficzny Then
            Return {1, 1, 0, 3,
                    1024, 0, 1, 2,                      'GTModelTypeGeoKey = geograficzny
                    1025, 0, 1, 1,                      'GTRasterTypeGeoKey = PixelIsArea
                    2048, 0, 1, JakoShort(uklad.Epsg)}  'GeographicTypeGeoKey
        Else
            Return {1, 1, 0, 3,
                    1024, 0, 1, 1,                      'GTModelTypeGeoKey = odwzorowanie
                    1025, 0, 1, 1,                      'GTRasterTypeGeoKey = PixelIsArea
                    3072, 0, 1, JakoShort(uklad.Epsg)}  'ProjectedCSTypeGeoKey
        End If
    End Function

    Private Shared Function JakoShort(kod As Integer) As Short
        Dim u As Integer = kod And &HFFFF
        Return CShort(If(u > 32767, u - 65536, u))
    End Function

    ''' <summary>Rejestruje w LibTiff.NET znaczniki GeoTIFF (raz na cały program).</summary>
    Private Shared Sub ZarejestrujZnaczniki()
        SyncLock Blokada
            If _rozszerzenieZarejestrowane Then Exit Sub
            _poprzednieRozszerzenie = Tiff.SetTagExtender(AddressOf RozszerzZnaczniki)
            _rozszerzenieZarejestrowane = True
        End SyncLock
    End Sub

    Private Shared Sub RozszerzZnaczniki(tif As Tiff)
        Dim info() As TiffFieldInfo = {
            New TiffFieldInfo(CType(TagModelPixelScale, TiffTag), -1, -1, TiffType.DOUBLE, FieldBit.Custom, False, True, "ModelPixelScaleTag"),
            New TiffFieldInfo(CType(TagModelTiepoint, TiffTag), -1, -1, TiffType.DOUBLE, FieldBit.Custom, False, True, "ModelTiepointTag"),
            New TiffFieldInfo(CType(TagGeoKeyDirectory, TiffTag), -1, -1, TiffType.SHORT, FieldBit.Custom, False, True, "GeoKeyDirectoryTag"),
            New TiffFieldInfo(CType(TagGeoDoubleParams, TiffTag), -1, -1, TiffType.DOUBLE, FieldBit.Custom, False, True, "GeoDoubleParamsTag"),
            New TiffFieldInfo(CType(TagGeoAsciiParams, TiffTag), -1, -1, TiffType.ASCII, FieldBit.Custom, False, False, "GeoAsciiParamsTag")}
        tif.MergeFieldInfo(info, info.Length)
        _poprzednieRozszerzenie?.Invoke(tif)
    End Sub

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim bajtowWWierszu = Szerokosc * Kanaly
        For w = 0 To liczbaWierszy - 1
            Buffer.BlockCopy(rgb, w * bajtowWWierszu, _pasBufor, _wierszyWPasie * bajtowWWierszu, bajtowWWierszu)
            _wierszyWPasie += 1

            If _wierszyWPasie = ROZMIAR_KAFLA OrElse (Zapisane + w + 1) = Wysokosc Then
                Dim yPoczatekPasa = (Zapisane + w + 1) - _wierszyWPasie
                ZapiszPasJakoKafle(_tiff, _pasBufor, Szerokosc, yPoczatekPasa, _wierszyWPasie, Kanaly, _kafelBufor)
                _wierszyWPasie = 0
            End If

            If _poziomy.Count > 0 Then PrzekazWiersz(0, rgb, w * bajtowWWierszu)
        Next
    End Sub

    ''' <summary>
    ''' Przekazuje wiersz do poziomu piramidy. Co drugi wiersz (i ostatni nieparzysty) daje wiersz poziomu,
    ''' który trafia do pasa kafli poziomu i dalej do następnego poziomu.
    ''' </summary>
    Private Sub PrzekazWiersz(i As Integer, zrodlo As Byte(), przesuniecie As Integer)
        Dim p = _poziomy(i)
        p.Odebrane += 1
        If Not p.MaOczekujacy Then
            Buffer.BlockCopy(zrodlo, przesuniecie, p.Oczekujacy, 0, p.SrcW * Kanaly)
            p.MaOczekujacy = True
            If p.Odebrane < p.SrcH Then Return
            Pomniejsz(p, p.Oczekujacy, 0, Nothing, 0)
        Else
            Pomniejsz(p, p.Oczekujacy, 0, zrodlo, przesuniecie)
        End If
        p.MaOczekujacy = False

        Dim bajtyWiersza = p.W * Kanaly
        Buffer.BlockCopy(p.Wiersz, 0, p.Pas, p.WierszyWPasie * bajtyWiersza, bajtyWiersza)
        p.WierszyWPasie += 1
        p.Zapisane += 1
        If p.WierszyWPasie = ROZMIAR_KAFLA OrElse p.Zapisane = p.H Then
            ZapiszPasJakoKafle(p.Tif, p.Pas, p.W, p.Zapisane - p.WierszyWPasie, p.WierszyWPasie, Kanaly, _kafelBufor)
            p.WierszyWPasie = 0
        End If
        If i + 1 < _poziomy.Count Then PrzekazWiersz(i + 1, p.Wiersz, 0)
    End Sub

    ''' <summary>
    ''' Średnia 2x2 dwóch wierszy (drugi może nie istnieć) do p.Wiersz. Przy kanale alfa kolor jest ważony
    ''' przezroczystością, żeby przezroczyste piksele nie przyciemniały krawędzi.
    ''' </summary>
    Private Sub Pomniejsz(p As PoziomPiramidy, wierszA As Byte(), offA As Integer, wierszB As Byte(), offB As Integer)
        Dim k = Kanaly
        For dx = 0 To p.W - 1
            Dim x0 = dx * 2
            Dim ileX = If(x0 + 1 < p.SrcW, 1, 0)
            Dim r As Integer = 0, g As Integer = 0, b As Integer = 0, sumaA As Integer = 0, n As Integer = 0
            For wiersz = 0 To 1
                Dim buf = If(wiersz = 0, wierszA, wierszB)
                If buf Is Nothing Then Continue For
                Dim off0 = If(wiersz = 0, offA, offB) + x0 * k
                For ox = 0 To ileX
                    Dim o = off0 + ox * k
                    If k = 3 Then
                        r += buf(o) : g += buf(o + 1) : b += buf(o + 2)
                    Else
                        Dim a = CInt(buf(o + 3))
                        r += buf(o) * a : g += buf(o + 1) * a : b += buf(o + 2) * a
                        sumaA += a
                    End If
                    n += 1
                Next
            Next
            Dim d = dx * k
            If k = 3 Then
                Dim pol = n \ 2
                p.Wiersz(d) = CByte((r + pol) \ n)
                p.Wiersz(d + 1) = CByte((g + pol) \ n)
                p.Wiersz(d + 2) = CByte((b + pol) \ n)
            ElseIf sumaA > 0 Then
                Dim polA = sumaA \ 2
                p.Wiersz(d) = CByte((r + polA) \ sumaA)
                p.Wiersz(d + 1) = CByte((g + polA) \ sumaA)
                p.Wiersz(d + 2) = CByte((b + polA) \ sumaA)
                p.Wiersz(d + 3) = CByte((sumaA + n \ 2) \ n)
            Else
                p.Wiersz(d) = 0 : p.Wiersz(d + 1) = 0 : p.Wiersz(d + 2) = 0 : p.Wiersz(d + 3) = 0
            End If
        Next
    End Sub

    ''' <summary>Dzieli pas wierszy na kafle 256x256; brzegi niepełnych kafli powiela (bez ciemnych obwódek).</summary>
    Private Shared Sub ZapiszPasJakoKafle(tif As Tiff, pas As Byte(), szerokosc As Integer, yPoczatekPasa As Integer,
                                          wysokoscPasa As Integer, kanaly As Integer, kafel As Byte())
        Dim wierszPasaBajty = szerokosc * kanaly
        Dim wierszKaflaBajty = ROZMIAR_KAFLA * kanaly
        Dim liczbaKafliX = (szerokosc + ROZMIAR_KAFLA - 1) \ ROZMIAR_KAFLA

        For tx = 0 To liczbaKafliX - 1
            Dim xStart = tx * ROZMIAR_KAFLA
            Dim waznaSzer = Math.Min(ROZMIAR_KAFLA, szerokosc - xStart)
            Dim wazneBajty = waznaSzer * kanaly

            For ly = 0 To wysokoscPasa - 1
                Dim dstOff = ly * wierszKaflaBajty
                Buffer.BlockCopy(pas, (ly * wierszPasaBajty) + (xStart * kanaly), kafel, dstOff, wazneBajty)
                If waznaSzer < ROZMIAR_KAFLA Then
                    Dim ostPikselOff = dstOff + ((waznaSzer - 1) * kanaly)
                    For lx = waznaSzer To ROZMIAR_KAFLA - 1
                        Buffer.BlockCopy(kafel, ostPikselOff, kafel, dstOff + (lx * kanaly), kanaly)
                    Next
                End If
            Next

            If wysokoscPasa < ROZMIAR_KAFLA Then
                Dim ostWierszOff = (wysokoscPasa - 1) * wierszKaflaBajty
                For ly = wysokoscPasa To ROZMIAR_KAFLA - 1
                    Buffer.BlockCopy(kafel, ostWierszOff, kafel, ly * wierszKaflaBajty, wierszKaflaBajty)
                Next
            End If

            If tif.WriteTile(kafel, 0, xStart, yPoczatekPasa, 0, 0) < 0 Then
                Throw New IOException($"Błąd zapisu kafelka ({tx}, {yPoczatekPasa \ ROZMIAR_KAFLA}).")
            End If
        Next
    End Sub

    Public Overrides Sub Zakoncz()
        If _zakonczony Then Return
        If Zapisane <> Wysokosc Then Throw New InvalidOperationException($"Nie zapisano wszystkich wierszy obrazu ({Zapisane}/{Wysokosc}).")
        _zakonczony = True

        If Not _tiff.WriteDirectory() Then Throw New IOException("Błąd zapisu katalogu głównego GeoTIFF.")
        _tiff.Close()
        _tiff.Dispose()
        _tiff = Nothing
        If _poziomy.Count = 0 Then Return

        For Each p In _poziomy
            p.Tif.Close()
            p.Tif.Dispose()
            p.Tif = Nothing
        Next
        Try
            DopiszPiramidy()
        Finally
            UsunPlikiPoziomow()
        End Try
    End Sub

    ''' <summary>Dopisuje poziomy z plików tymczasowych jako kolejne katalogi IFD (kompresja jak w obrazie głównym).</summary>
    Private Sub DopiszPiramidy()
        Dim wszystkie As Long = 0
        For Each p In _poziomy
            wszystkie += CLng((p.W + ROZMIAR_KAFLA - 1) \ ROZMIAR_KAFLA) * ((p.H + ROZMIAR_KAFLA - 1) \ ROZMIAR_KAFLA)
        Next
        Dim zrobione As Long = 0
        Dim ostatniProcent = -1

        Dim rawBufor As Byte() = Array.Empty(Of Byte)()
        Using fs As New FileStream(_sciezka, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 65536)
            Using writer As Tiff = Tiff.ClientOpen("GeoTiff", "a", fs, New TiffStream())
                If writer Is Nothing Then Throw New IOException("Nie udało się otworzyć pliku GeoTIFF do dopisania piramid.")
                For Each p In _poziomy
                    Using reader As Tiff = Tiff.Open(p.Sciezka, "r")
                        If reader Is Nothing Then Throw New IOException("Nie udało się odczytać pliku tymczasowego " & p.Sciezka)
                        KonfigurujKatalog(writer, p.W, p.H, Kanaly, _kompresjaJpeg, _jakosc, czyPiramida:=True)
                        For y = 0 To p.H - 1 Step ROZMIAR_KAFLA
                            For x = 0 To p.W - 1 Step ROZMIAR_KAFLA
                                Przerwanie.ThrowIfCancellationRequested()
                                Dim skopiowanoSurowo As Boolean = False
                                If Not _kompresjaJpeg Then
                                    Dim srcTile As Integer = reader.ComputeTile(x, y, 0, 0)
                                    Dim surowyRozmiarLong As Long = reader.RawTileSize(srcTile)
                                    If surowyRozmiarLong > 0 AndAlso surowyRozmiarLong <= Integer.MaxValue Then
                                        Dim surowyRozmiar As Integer = CInt(surowyRozmiarLong)
                                        If surowyRozmiar > rawBufor.Length Then ReDim rawBufor(surowyRozmiar - 1)
                                        Dim odczytano As Integer = reader.ReadRawTile(srcTile, rawBufor, 0, surowyRozmiar)
                                        If odczytano = surowyRozmiar AndAlso writer.WriteRawTile(writer.ComputeTile(x, y, 0, 0), rawBufor, 0, odczytano) >= 0 Then
                                            skopiowanoSurowo = True
                                        End If
                                    End If
                                End If
                                If Not skopiowanoSurowo Then
                                    If reader.ReadTile(_kafelBufor, 0, x, y, 0, 0) < 0 Then
                                        Throw New IOException($"Błąd odczytu kafelka piramidy ({x}, {y}).")
                                    End If
                                    If writer.WriteTile(_kafelBufor, 0, x, y, 0, 0) < 0 Then
                                        Throw New IOException($"Błąd zapisu kafelka piramidy ({x}, {y}).")
                                    End If
                                End If
                                zrobione += 1
                                Dim procent = CInt(zrobione * 100 \ wszystkie)
                                If procent <> ostatniProcent Then
                                    ostatniProcent = procent
                                    PostepPiramid?.Invoke(procent)
                                End If
                            Next
                        Next
                        If Not writer.WriteDirectory() Then Throw New IOException("Błąd zapisu katalogu piramidy GeoTIFF.")
                    End Using
                Next
            End Using
        End Using
    End Sub

    Private Sub UsunPlikiPoziomow()
        For Each p In _poziomy
            Try
                If File.Exists(p.Sciezka) Then File.Delete(p.Sciezka)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                'plik tymczasowy zostaje, gdy inny program go trzyma - zapis mapy się nie przerywa
            End Try
        Next
    End Sub

    Public Overrides Sub Dispose()
        _zakonczony = True
        ZamknijCicho(_tiff)
        _tiff = Nothing
        For Each p In _poziomy
            ZamknijCicho(p.Tif)
            p.Tif = Nothing
        Next
        UsunPlikiPoziomow()
    End Sub

    Private Shared Sub ZamknijCicho(tif As Tiff)
        If tif Is Nothing Then Return
        Try
            tif.Close()
        Catch
        End Try
        Try
            tif.Dispose()
        Catch
        End Try
    End Sub

End Class
