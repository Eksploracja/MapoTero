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
    ''' <summary>Liczba zapisanych dotąd wierszy.</summary>
    Public Property Zapisane As Integer

    Protected Sub New(szerokosc As Integer, wysokosc As Integer)
        If szerokosc <= 0 OrElse wysokosc <= 0 Then Throw New ArgumentException("Niepoprawny rozmiar obrazu")
        Me.Szerokosc = szerokosc
        Me.Wysokosc = wysokosc
    End Sub

    ''' <summary>Zapisuje kolejne wiersze: bufor RGB o długości liczbaWierszy * Szerokosc * 3, wierszami od góry.</summary>
    Public Sub ZapiszWiersze(rgb() As Byte, liczbaWierszy As Integer)
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

    ''' <summary>Tworzy zapis obrazu w wybranym formacie.</summary>
    Public Shared Function Utworz(format As FormatArkusza, sciezka As String, szerokosc As Integer, wysokosc As Integer, jakoscJpeg As Integer,
                                  uklad As UkladWspolrzednych, zasieg As Zasieg) As ZapisRastra
        Select Case format
            Case FormatArkusza.Jpeg : Return New ZapisJpeg(sciezka, szerokosc, wysokosc, jakoscJpeg)
            Case FormatArkusza.Png : Return New ZapisPng(sciezka, szerokosc, wysokosc)
            Case FormatArkusza.GeoTiffJpeg : Return New ZapisGeoTiff(sciezka, szerokosc, wysokosc, True, jakoscJpeg, uklad, zasieg)
            Case Else : Return New ZapisGeoTiff(sciezka, szerokosc, wysokosc, False, jakoscJpeg, uklad, zasieg)
        End Select
    End Function

End Class

''' <summary>Strumieniowy zapis PNG (kolor RGB 8 bitów, filtr Up, kompresja Deflate).</summary>
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

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer)
        MyBase.New(szerokosc, wysokosc)
        _plik = New FileStream(sciezka, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16)
        _plik.Write(New Byte() {137, 80, 78, 71, 13, 10, 26, 10}, 0, 8)
        Dim ihdr(12) As Byte
        ZapiszBE(ihdr, 0, CUInt(szerokosc))
        ZapiszBE(ihdr, 4, CUInt(wysokosc))
        ihdr(8) = 8   'głębia bitowa
        ihdr(9) = 2   'RGB
        ihdr(10) = 0 : ihdr(11) = 0 : ihdr(12) = 0
        ZapiszFragment(_plik, "IHDR", ihdr, ihdr.Length)

        _idat = New StrumienFragmentow(_plik, "IDAT")
        'nagłówek zlib (deflate, okno 32 kB)
        _idat.Write(New Byte() {&H78, &H9C}, 0, 2)
        _deflate = New DeflateStream(_idat, CompressionLevel.Optimal, True)
        _poprzedni = New Byte(szerokosc * 3 - 1) {}
        _wiersz = New Byte(szerokosc * 3) {}
    End Sub

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim dl As Integer = Szerokosc * 3
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
    Private _zakonczony As Boolean

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer, jakosc As Integer)
        MyBase.New(szerokosc, wysokosc)
        If szerokosc > 65500 OrElse wysokosc > 65500 Then
            Throw New ArgumentException("Format JPEG pozwala na obraz o boku najwyżej 65500 pikseli - wybierz GeoTIFF lub PNG")
        End If
        _plik = New FileStream(sciezka, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16)
        _jpeg = New jpeg_compress_struct(New jpeg_error_mgr()) With {
            .Image_width = szerokosc, .Image_height = wysokosc, .Input_components = 3, .In_color_space = J_COLOR_SPACE.JCS_RGB}
        _jpeg.jpeg_set_defaults()
        _jpeg.jpeg_set_quality(Math.Max(1, Math.Min(100, jakosc)), True)
        _jpeg.jpeg_stdio_dest(_plik)
        _jpeg.jpeg_start_compress(True)
    End Sub

    Protected Overrides Sub ZapiszWierszeWewn(rgb() As Byte, liczbaWierszy As Integer)
        Dim dl As Integer = Szerokosc * 3
        Dim wiersz(0)() As Byte
        wiersz(0) = New Byte(dl - 1) {}
        For w = 0 To liczbaWierszy - 1
            Buffer.BlockCopy(rgb, w * dl, wiersz(0), 0, dl)
            _jpeg.jpeg_write_scanlines(wiersz, 1)
        Next
    End Sub

    Public Overrides Sub Zakoncz()
        If Zapisane <> Wysokosc Then Throw New InvalidOperationException("Nie zapisano wszystkich wierszy obrazu")
        _jpeg.jpeg_finish_compress()
        _plik.Flush()
        _zakonczony = True
    End Sub

    Public Overrides Sub Dispose()
        If Not _zakonczony Then
            Try
                _jpeg.jpeg_abort_compress()
            Catch
            End Try
        End If
        _plik.Dispose()
    End Sub

End Class

''' <summary>
''' Zapis GeoTIFF (biblioteka LibTiff.NET): obraz z georeferencją zapisaną w samym pliku - układ współrzędnych
''' (kod EPSG), położenie i rozmiar piksela. Dla obrazów powyżej 4 GB - format BigTIFF.
''' </summary>
Public Class ZapisGeoTiff
    Inherits ZapisRastra

    'znaczniki GeoTIFF
    Private Const TagModelPixelScale As Integer = 33550
    Private Const TagModelTiepoint As Integer = 33922
    Private Const TagGeoKeyDirectory As Integer = 34735
    Private Const TagGeoDoubleParams As Integer = 34736
    Private Const TagGeoAsciiParams As Integer = 34737

    Private Shared ReadOnly Blokada As New Object()
    Private Shared _rozszerzenieZarejestrowane As Boolean
    Private Shared _poprzednieRozszerzenie As Tiff.TiffExtendProc

    Private ReadOnly _tiff As Tiff
    Private ReadOnly _wiersz() As Byte
    Private _zakonczony As Boolean

    Public Sub New(sciezka As String, szerokosc As Integer, wysokosc As Integer, kompresjaJpeg As Boolean, jakosc As Integer,
                   uklad As UkladWspolrzednych, zasieg As Zasieg)
        MyBase.New(szerokosc, wysokosc)
        ZarejestrujZnaczniki()

        'BigTIFF, gdy nieskompresowany obraz przekracza ok. 4 GB
        Dim duzy As Boolean = CLng(szerokosc) * wysokosc * 3 > 3900000000L
        _tiff = Tiff.Open(sciezka, If(duzy, "w8", "w"))
        If _tiff Is Nothing Then Throw New IOException("Nie można utworzyć pliku " & sciezka)

        _tiff.SetField(TiffTag.IMAGEWIDTH, szerokosc)
        _tiff.SetField(TiffTag.IMAGELENGTH, wysokosc)
        _tiff.SetField(TiffTag.BITSPERSAMPLE, 8)
        _tiff.SetField(TiffTag.SAMPLESPERPIXEL, 3)
        _tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG)
        _tiff.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT)
        _tiff.SetField(TiffTag.ROWSPERSTRIP, 16)
        _tiff.SetField(TiffTag.SOFTWARE, "MapoTero")
        If kompresjaJpeg Then
            _tiff.SetField(TiffTag.COMPRESSION, Compression.JPEG)
            _tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.YCBCR)
            _tiff.SetField(TiffTag.JPEGQUALITY, Math.Max(1, Math.Min(100, jakosc)))
            _tiff.SetField(TiffTag.JPEGCOLORMODE, JpegColorMode.RGB)
        Else
            _tiff.SetField(TiffTag.COMPRESSION, Compression.ADOBE_DEFLATE)
            _tiff.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL)
            _tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB)
        End If

        'georeferencja: rozmiar piksela, punkt dowiązania (lewy górny narożnik obrazu), układ współrzędnych
        Dim pikselY As Double = zasieg.Szerokosc / szerokosc
        Dim pikselX As Double = zasieg.Wysokosc / wysokosc
        _tiff.SetField(CType(TagModelPixelScale, TiffTag), 3, New Double() {pikselY, pikselX, 0})
        _tiff.SetField(CType(TagModelTiepoint, TiffTag), 6, New Double() {0, 0, 0, zasieg.YLewy, zasieg.XGora, 0})
        Dim klucze() As Short
        If uklad.Geograficzny Then
            klucze = {1, 1, 0, 3,
                      1024, 0, 1, 2,              'GTModelTypeGeoKey = geograficzny
                      1025, 0, 1, 1,              'GTRasterTypeGeoKey = PixelIsArea
                      2048, 0, 1, JakoShort(uklad.Epsg)}  'GeographicTypeGeoKey
        Else
            klucze = {1, 1, 0, 3,
                      1024, 0, 1, 1,              'GTModelTypeGeoKey = odwzorowanie
                      1025, 0, 1, 1,              'GTRasterTypeGeoKey = PixelIsArea
                      3072, 0, 1, JakoShort(uklad.Epsg)}  'ProjectedCSTypeGeoKey
        End If
        _tiff.SetField(CType(TagGeoKeyDirectory, TiffTag), klucze.Length, klucze)

        _wiersz = New Byte(szerokosc * 3 - 1) {}
    End Sub

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
        Dim dl As Integer = Szerokosc * 3
        For w = 0 To liczbaWierszy - 1
            Buffer.BlockCopy(rgb, w * dl, _wiersz, 0, dl)
            If Not _tiff.WriteScanline(_wiersz, Zapisane + w) Then Throw New IOException("Błąd zapisu pliku TIFF")
        Next
    End Sub

    Public Overrides Sub Zakoncz()
        If Zapisane <> Wysokosc Then Throw New InvalidOperationException("Nie zapisano wszystkich wierszy obrazu")
        _tiff.FlushData()
        _tiff.Close()
        _zakonczony = True
    End Sub

    Public Overrides Sub Dispose()
        'po Zakoncz() plik jest już zamknięty (Close zwalnia zasoby)
        If Not _zakonczony Then
            _zakonczony = True
            Try
                _tiff?.Close()
            Catch
            Finally
                Try
                    _tiff?.Dispose()
                Catch
                End Try
            End Try
        End If
    End Sub

End Class
