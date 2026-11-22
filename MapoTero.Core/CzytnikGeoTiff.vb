Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports BitMiracle.LibTiff.Classic

''' <summary>
''' Wyjątek zgłaszany przy próbie wczytania numerycznego modelu terenu (DEM/NMT) zamiast obrazu mapy.
''' </summary>
Public Class GeoTiffWysokosciowyException
    Inherits IOException

    Public Sub New(komunikat As String)
        MyBase.New(komunikat)
    End Sub
End Class

''' <summary>
''' Georeferencja afiniczna rastra (zgodna z world file / ModelTransformationTag):
'''   X_mapy = A * kolumna + B * wiersz + C
'''   Y_mapy = D * kolumna + E * wiersz + F
''' </summary>
Public NotInheritable Class GeoreferencjaGeoTiff
    Public Property A As Double
    Public Property B As Double
    Public Property C As Double
    Public Property D As Double
    Public Property E As Double
    Public Property F As Double
    Public Property KodEpsg As Integer?
    Public Property ZrodloGeoreferencji As String = String.Empty
    Public Property ZrodloCrs As String = String.Empty

    Public Function PikselNaWspolrzedne(px As Double, py As Double) As (X As Double, Y As Double)
        Return (A * px + B * py + C, D * px + E * py + F)
    End Function
End Class

''' <summary>
''' Strumieniowy czytnik zewnętrznych plików GeoTIFF oparty na BitMiracle.LibTiff.NET (.NET 10).
''' Wykorzystuje ReadRGBAStrip / ReadRGBATile z buforem LRU, poprawnie obsługuje dolną krawędź pasów/kafli
''' oraz odrzuca siatki wysokościowe DEM/NMT (16-bit INT i 32-bit float).
''' </summary>
Public NotInheritable Class CzytnikGeoTiff
    Implements IDisposable

    Private Const TagModelPixelScale As TiffTag = CType(33550, TiffTag)
    Private Const TagModelTiepoint As TiffTag = CType(33922, TiffTag)
    Private Const TagModelTransformation As TiffTag = CType(34264, TiffTag)
    Private Const TagGeoKeyDirectory As TiffTag = CType(34735, TiffTag)

    Private Const KeyGeographicType As Integer = 2048
    Private Const KeyProjectedCSType As Integer = 3072

    Private Shared ReadOnly _lockExtender As New Object()
    Private Shared _extenderZarejestrowany As Boolean = False
    Private Shared _poprzedniExtender As Tiff.TiffExtendProc = Nothing

    Private _tiff As Tiff
    Private _disposed As Boolean = False

    Public ReadOnly Property Szerokosc As Integer
    Public ReadOnly Property Wysokosc As Integer
    Public ReadOnly Property CzyKafelkowany As Boolean
    Public ReadOnly Property SzerokoscKafla As Integer
    Public ReadOnly Property WysokoscKafla As Integer
    Public ReadOnly Property WierszyWPasie As Integer
    Public ReadOnly Property Georeferencja As GeoreferencjaGeoTiff

    Private ReadOnly _maksBlokowWCache As Integer
    Private ReadOnly _cacheSlownik As New Dictionary(Of Long, LinkedListNode(Of WpisBloku))()
    Private ReadOnly _cacheLru As New LinkedList(Of WpisBloku)()
    Private ReadOnly _lockOdczyt As New Object()

    Private NotInheritable Class WpisBloku
        Public Klucz As Long
        Public BuforAbgr As Integer()
        Public RealnaWysokosc As Integer
    End Class

    Public Sub New(sciezkaPliku As String, Optional maksBlokowWCache As Integer = 24)
        If Not File.Exists(sciezkaPliku) Then
            Throw New FileNotFoundException("Nie znaleziono pliku GeoTIFF.", sciezkaPliku)
        End If

        ZarejestrujTagiGeoTiff()
        _maksBlokowWCache = Math.Max(4, maksBlokowWCache)

        _tiff = Tiff.Open(sciezkaPliku, "r")
        If _tiff Is Nothing Then
            Throw New InvalidDataException(String.Format("Nie można otworzyć pliku jako prawidłowego TIFF/GeoTIFF: {0}", sciezkaPliku))
        End If

        Try
            WalidujZeToObrazRastrowy()

            Szerokosc = PobierzIntTag(TiffTag.IMAGEWIDTH, 0)
            Wysokosc = PobierzIntTag(TiffTag.IMAGELENGTH, 0)
            If Szerokosc <= 0 OrElse Wysokosc <= 0 Then
                Throw New InvalidDataException("Plik GeoTIFF ma nieprawidłowe wymiary obrazu.")
            End If

            CzyKafelkowany = _tiff.IsTiled()
            If CzyKafelkowany Then
                SzerokoscKafla = PobierzIntTag(TiffTag.TILEWIDTH, 256)
                WysokoscKafla = PobierzIntTag(TiffTag.TILELENGTH, 256)
            Else
                WierszyWPasie = PobierzIntTag(TiffTag.ROWSPERSTRIP, Wysokosc)
                If WierszyWPasie <= 0 Then WierszyWPasie = Wysokosc
            End If

            Georeferencja = OdczytajGeoreferencje(sciezkaPliku)
        Catch
            _tiff.Dispose()
            _tiff = Nothing
            Throw
        End Try
    End Sub

    Private Shared Sub ZarejestrujTagiGeoTiff()
        SyncLock _lockExtender
            If _extenderZarejestrowany Then Return
            _poprzedniExtender = Tiff.SetTagExtender(AddressOf RozszerzTagiGeoTiff)
            _extenderZarejestrowany = True
        End SyncLock
    End Sub

    Private Shared Sub RozszerzTagiGeoTiff(tif As Tiff)
        Dim pola As TiffFieldInfo() = {
            New TiffFieldInfo(TagModelPixelScale, -1, -1, TiffType.DOUBLE, FieldBit.Custom, True, True, "ModelPixelScaleTag"),
            New TiffFieldInfo(TagModelTiepoint, -1, -1, TiffType.DOUBLE, FieldBit.Custom, True, True, "ModelTiepointTag"),
            New TiffFieldInfo(TagModelTransformation, -1, -1, TiffType.DOUBLE, FieldBit.Custom, True, True, "ModelTransformationTag"),
            New TiffFieldInfo(TagGeoKeyDirectory, -1, -1, TiffType.SHORT, FieldBit.Custom, True, True, "GeoKeyDirectoryTag")
        }
        tif.MergeFieldInfo(pola, pola.Length)
        If _poprzedniExtender IsNot Nothing Then
            _poprzedniExtender.Invoke(tif)
        End If
    End Sub

    Private Sub WalidujZeToObrazRastrowy()
        Dim sampleFormat As Integer = PobierzIntTag(TiffTag.SAMPLEFORMAT, CInt(BitMiracle.LibTiff.Classic.SampleFormat.UINT))
        Dim bitsPerSample As Integer = PobierzIntTag(TiffTag.BITSPERSAMPLE, 8)
        Dim samplesPerPixel As Integer = PobierzIntTag(TiffTag.SAMPLESPERPIXEL, 1)

        If sampleFormat = CInt(BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP) Then
            Throw New GeoTiffWysokosciowyException(
                "Wskazany plik GeoTIFF zawiera numeryczny model terenu / dane zmiennoprzecinkowe (32-bit float DEM/NMT), a nie obraz mapy.")
        End If

        If sampleFormat = CInt(BitMiracle.LibTiff.Classic.SampleFormat.INT) AndAlso samplesPerPixel = 1 AndAlso bitsPerSample >= 16 Then
            Throw New GeoTiffWysokosciowyException(
                "Wskazany plik GeoTIFF zawiera 16-bitową siatkę wysokościową (signed INT DEM/NMT), a nie obraz mapy.")
        End If
    End Sub

    Public Function WytnijFragmentBgra(x As Integer, y As Integer, szer As Integer, wys As Integer) As Byte()
        If szer <= 0 OrElse wys <= 0 Then
            Return Array.Empty(Of Byte)()
        End If

        Dim wynik((szer * wys * 4) - 1) As Byte

        SyncLock _lockOdczyt
            For dy As Integer = 0 To wys - 1
                Dim currY As Integer = y + dy
                If currY < 0 OrElse currY >= Wysokosc Then Continue For

                Dim bazaWierszaDst As Integer = dy * szer * 4

                If CzyKafelkowany Then
                    KopiujWierszZKafli(currY, x, szer, wynik, bazaWierszaDst)
                Else
                    KopiujWierszZPasa(currY, x, szer, wynik, bazaWierszaDst)
                End If
            Next
        End SyncLock

        Return wynik
    End Function

    Private Sub KopiujWierszZPasa(currY As Integer, startX As Integer, szer As Integer, dst As Byte(), bazaDst As Integer)
        Dim pasStartY As Integer = (currY \ WierszyWPasie) * WierszyWPasie
        Dim blok As WpisBloku = PobierzPasZLru(pasStartY)

        ' Ostatni niepełny pas: odwracanie względem realneWiersze - 1 - y
        Dim lokalneY As Integer = currY - pasStartY
        Dim odwroconeY As Integer = blok.RealnaWysokosc - 1 - lokalneY
        Dim bazaSrc As Integer = odwroconeY * Szerokosc

        For dx As Integer = 0 To szer - 1
            Dim px As Integer = startX + dx
            If px < 0 OrElse px >= Szerokosc Then Continue For

            Dim abgr As Integer = blok.BuforAbgr(bazaSrc + px)
            Dim di As Integer = bazaDst + (dx * 4)
            dst(di) = CByte(Tiff.GetB(abgr))
            dst(di + 1) = CByte(Tiff.GetG(abgr))
            dst(di + 2) = CByte(Tiff.GetR(abgr))
            dst(di + 3) = CByte(Tiff.GetA(abgr))
        Next
    End Sub

    Private Sub KopiujWierszZKafli(currY As Integer, startX As Integer, szer As Integer, dst As Byte(), bazaDst As Integer)
        Dim kafelStartY As Integer = (currY \ WysokoscKafla) * WysokoscKafla
        Dim lokalneY As Integer = currY - kafelStartY

        ' Kafel (także niepełny dolny): odwracanie zawsze względem tileLength - 1 - y ze stałym krokiem tileWidth
        Dim odwroconeY As Integer = WysokoscKafla - 1 - lokalneY
        Dim bazaWierszaKafla As Integer = odwroconeY * SzerokoscKafla

        Dim dx As Integer = 0
        While dx < szer
            Dim px As Integer = startX + dx
            If px < 0 OrElse px >= Szerokosc Then
                dx += 1
                Continue While
            End If

            Dim kafelStartX As Integer = (px \ SzerokoscKafla) * SzerokoscKafla
            Dim blok As WpisBloku = PobierzKafelZLru(kafelStartX, kafelStartY)
            Dim lokalneX As Integer = px - kafelStartX
            Dim doSkopiowania As Integer = Math.Min(SzerokoscKafla - lokalneX, Math.Min(szer - dx, Szerokosc - px))

            For k As Integer = 0 To doSkopiowania - 1
                Dim abgr As Integer = blok.BuforAbgr(bazaWierszaKafla + lokalneX + k)
                Dim di As Integer = bazaDst + ((dx + k) * 4)
                dst(di) = CByte(Tiff.GetB(abgr))
                dst(di + 1) = CByte(Tiff.GetG(abgr))
                dst(di + 2) = CByte(Tiff.GetR(abgr))
                dst(di + 3) = CByte(Tiff.GetA(abgr))
            Next

            dx += doSkopiowania
        End While
    End Sub

    Private Function PobierzPasZLru(pasStartY As Integer) As WpisBloku
        Dim klucz As Long = pasStartY
        Dim wezel As LinkedListNode(Of WpisBloku) = Nothing
        If _cacheSlownik.TryGetValue(klucz, wezel) Then
            _cacheLru.Remove(wezel)
            _cacheLru.AddFirst(wezel)
            Return wezel.Value
        End If

        Dim realnaWys As Integer = Math.Min(WierszyWPasie, Wysokosc - pasStartY)
        Dim bufor((Szerokosc * WierszyWPasie) - 1) As Integer
        If Not _tiff.ReadRGBAStrip(pasStartY, bufor) Then
            Throw New IOException(String.Format("Błąd dekodowania pasa GeoTIFF (wiersz {0}).", pasStartY))
        End If

        Return DodajDoLru(klucz, bufor, realnaWys)
    End Function

    Private Function PobierzKafelZLru(kafelStartX As Integer, kafelStartY As Integer) As WpisBloku
        Dim klucz As Long = (CLng(kafelStartY) << 32) Or CLng(kafelStartX)
        Dim wezel As LinkedListNode(Of WpisBloku) = Nothing
        If _cacheSlownik.TryGetValue(klucz, wezel) Then
            _cacheLru.Remove(wezel)
            _cacheLru.AddFirst(wezel)
            Return wezel.Value
        End If

        Dim bufor((SzerokoscKafla * WysokoscKafla) - 1) As Integer
        If Not _tiff.ReadRGBATile(kafelStartX, kafelStartY, bufor) Then
            Throw New IOException(String.Format("Błąd dekodowania kafla GeoTIFF ({0}, {1}).", kafelStartX, kafelStartY))
        End If

        Return DodajDoLru(klucz, bufor, WysokoscKafla)
    End Function

    Private Function DodajDoLru(klucz As Long, bufor As Integer(), realnaWys As Integer) As WpisBloku
        Dim wpis As New WpisBloku() With {
            .Klucz = klucz,
            .BuforAbgr = bufor,
            .RealnaWysokosc = realnaWys
        }
        Dim nowyWezel As New LinkedListNode(Of WpisBloku)(wpis)
        _cacheLru.AddFirst(nowyWezel)
        _cacheSlownik(klucz) = nowyWezel

        While _cacheLru.Count > _maksBlokowWCache
            Dim najstarszy As LinkedListNode(Of WpisBloku) = _cacheLru.Last
            _cacheLru.RemoveLast()
            _cacheSlownik.Remove(najstarszy.Value.Klucz)
        End While

        Return wpis
    End Function

    Private Function OdczytajGeoreferencje(sciezkaTiff As String) As GeoreferencjaGeoTiff
        Dim geo As New GeoreferencjaGeoTiff()

        Dim trans As Double() = PobierzDoubleArrayTag(TagModelTransformation)
        If trans IsNot Nothing AndAlso trans.Length >= 16 Then
            geo.A = trans(0)
            geo.B = trans(1)
            geo.C = trans(3)
            geo.D = trans(4)
            geo.E = trans(5)
            geo.F = trans(7)
            geo.ZrodloGeoreferencji = "GeoTIFF (ModelTransformationTag)"
        Else
            Dim skala As Double() = PobierzDoubleArrayTag(TagModelPixelScale)
            Dim punkty As Double() = PobierzDoubleArrayTag(TagModelTiepoint)
            If skala IsNot Nothing AndAlso skala.Length >= 2 AndAlso punkty IsNot Nothing AndAlso punkty.Length >= 6 Then
                Dim iPx As Double = punkty(0)
                Dim jPy As Double = punkty(1)
                Dim xMap As Double = punkty(3)
                Dim yMap As Double = punkty(4)
                geo.A = skala(0)
                geo.B = 0.0
                geo.C = xMap - (iPx * skala(0))
                geo.D = 0.0
                geo.E = -Math.Abs(skala(1))
                geo.F = yMap + (jPy * Math.Abs(skala(1)))
                geo.ZrodloGeoreferencji = "GeoTIFF (ModelPixelScale + ModelTiepoint)"
            Else
                WczytajWorldFile(sciezkaTiff, geo)
            End If
        End If

        Dim epsgTag As Integer? = OdczytajEpsgZGeoKeys()
        If epsgTag.HasValue Then
            geo.KodEpsg = epsgTag
            geo.ZrodloCrs = String.Format("GeoTIFF GeoKey (EPSG:{0})", epsgTag.Value)
        Else
            Dim sciezkaPrj As String = Path.ChangeExtension(sciezkaTiff, ".prj")
            If File.Exists(sciezkaPrj) Then
                Dim epsgPrj As Integer? = RozpoznajEpsgZPlikuPrj(File.ReadAllText(sciezkaPrj))
                If epsgPrj.HasValue Then
                    geo.KodEpsg = epsgPrj
                    geo.ZrodloCrs = String.Format("Plik .prj (EPSG:{0})", epsgPrj.Value)
                End If
            End If
        End If

        Return geo
    End Function

    Private Function OdczytajEpsgZGeoKeys() As Integer?
        Dim pole As FieldValue() = _tiff.GetField(TagGeoKeyDirectory)
        If pole Is Nothing OrElse pole.Length < 2 Then Return Nothing

        Dim klucze As Short() = TryCast(pole(1).Value, Short())
        If klucze Is Nothing OrElse klucze.Length < 4 Then Return Nothing

        ' Bezalokacyjne rzutowanie bez znaku (obsługuje kody EPSG >= 32768, np. UTM 32633-32635)
        Dim liczbaKluczy As Integer = CInt(klucze(3)) And &HFFFF
        Dim epsgProj As Integer? = Nothing
        Dim epsgGeog As Integer? = Nothing

        For i As Integer = 0 To liczbaKluczy - 1
            Dim baza As Integer = 4 + (i * 4)
            If baza + 3 >= klucze.Length Then Exit For

            Dim keyId As Integer = CInt(klucze(baza)) And &HFFFF
            Dim tiffTagLoc As Integer = CInt(klucze(baza + 1)) And &HFFFF
            Dim wartosc As Integer = CInt(klucze(baza + 3)) And &HFFFF

            If tiffTagLoc = 0 AndAlso wartosc > 0 AndAlso wartosc <> 32767 Then
                If keyId = KeyProjectedCSType Then epsgProj = wartosc
                If keyId = KeyGeographicType Then epsgGeog = wartosc
            End If
        Next

        Return If(epsgProj, epsgGeog)
    End Function

    Private Shared Sub WczytajWorldFile(sciezkaTiff As String, geo As GeoreferencjaGeoTiff)
        Dim katalog As String = If(Path.GetDirectoryName(sciezkaTiff), String.Empty)
        Dim baza As String = Path.Combine(katalog, Path.GetFileNameWithoutExtension(sciezkaTiff))
        Dim kandydaci As String() = {baza & ".tfw", baza & ".tifw", baza & ".wld", sciezkaTiff & "w"}

        For Each plik As String In kandydaci
            If Not File.Exists(plik) Then Continue For
            Dim linie As String() = File.ReadAllLines(plik) _
                .Where(Function(l) Not String.IsNullOrWhiteSpace(l)) _
                .ToArray()
            If linie.Length < 6 Then Continue For

            Dim v(5) As Double
            Dim ok As Boolean = True
            For i As Integer = 0 To 5
                If Not Double.TryParse(linie(i).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, v(i)) Then
                    ok = False
                    Exit For
                End If
            Next
            If Not ok Then Continue For

            geo.A = v(0)
            geo.D = v(1)
            geo.B = v(2)
            geo.E = v(3)
            geo.C = v(4) - (0.5 * v(0)) - (0.5 * v(2))
            geo.F = v(5) - (0.5 * v(1)) - (0.5 * v(3))
            geo.ZrodloGeoreferencji = String.Format("World file ({0})", Path.GetFileName(plik))
            Return
        Next

        Throw New InvalidDataException(
            "Plik TIFF nie posiada tagów georeferencji GeoTIFF ani pliku towarzyszącego (.tfw / .tifw / .wld).")
    End Sub

    Public Shared Function RozpoznajEpsgZPlikuPrj(wkt As String) As Integer?
        If String.IsNullOrWhiteSpace(wkt) Then Return Nothing

        Dim dopasowania As MatchCollection = Regex.Matches(wkt, "AUTHORITY\s*\[\s*""EPSG""\s*,\s*""(\d{4,5})""\s*\]", RegexOptions.IgnoreCase)
        If dopasowania.Count > 0 Then
            Return Integer.Parse(dopasowania(dopasowania.Count - 1).Groups(1).Value, CultureInfo.InvariantCulture)
        End If

        Dim u As String = wkt.ToUpperInvariant()
        If u.Contains("POLAND_CS92") OrElse u.Contains("PUWG_92") OrElse u.Contains("ETRF2000_PL_CS92") Then Return 2180
        If u.Contains("CS2000_ZONE_5") OrElse u.Contains("2000_STREF_5") OrElse u.Contains("POLAND_CS2000_ZONE_5") Then Return 2176
        If u.Contains("CS2000_ZONE_6") OrElse u.Contains("2000_STREF_6") OrElse u.Contains("POLAND_CS2000_ZONE_6") Then Return 2177
        If u.Contains("CS2000_ZONE_7") OrElse u.Contains("2000_STREF_7") OrElse u.Contains("POLAND_CS2000_ZONE_7") Then Return 2178
        If u.Contains("CS2000_ZONE_8") OrElse u.Contains("2000_STREF_8") OrElse u.Contains("POLAND_CS2000_ZONE_8") Then Return 2179
        If u.Contains("UTM_ZONE_33N") Then Return 32633
        If u.Contains("UTM_ZONE_34N") Then Return 32634
        If u.Contains("UTM_ZONE_35N") Then Return 32635
        If u.Contains("WEB_MERCATOR") OrElse u.Contains("PSEUDO_MERCATOR") Then Return 3857

        Dim mCm As Match = Regex.Match(wkt, "Central_Meridian""\s*,\s*([0-9\.\-]+)", RegexOptions.IgnoreCase)
        Dim mFe As Match = Regex.Match(wkt, "False_Easting""\s*,\s*([0-9\.\-]+)", RegexOptions.IgnoreCase)
        If mCm.Success AndAlso mFe.Success Then
            Dim cm As Double = Double.Parse(mCm.Groups(1).Value, CultureInfo.InvariantCulture)
            Dim fe As Double = Double.Parse(mFe.Groups(1).Value, CultureInfo.InvariantCulture)
            If Math.Abs(cm - 19.0) < 0.01 AndAlso Math.Abs(fe - 500000.0) < 1.0 Then Return 2180
            If Math.Abs(cm - 15.0) < 0.01 AndAlso Math.Abs(fe - 5500000.0) < 1.0 Then Return 2176
            If Math.Abs(cm - 18.0) < 0.01 AndAlso Math.Abs(fe - 6500000.0) < 1.0 Then Return 2177
            If Math.Abs(cm - 21.0) < 0.01 AndAlso Math.Abs(fe - 7500000.0) < 1.0 Then Return 2178
            If Math.Abs(cm - 24.0) < 0.01 AndAlso Math.Abs(fe - 8500000.0) < 1.0 Then Return 2179
            If Math.Abs(cm - 15.0) < 0.01 AndAlso Math.Abs(fe - 500000.0) < 1.0 Then Return 32633
            If Math.Abs(cm - 21.0) < 0.01 AndAlso Math.Abs(fe - 500000.0) < 1.0 Then Return 32634
            If Math.Abs(cm - 27.0) < 0.01 AndAlso Math.Abs(fe - 500000.0) < 1.0 Then Return 32635
        End If

        If u.StartsWith("GEOGCS") AndAlso (u.Contains("WGS_1984") OrElse u.Contains("WGS 84")) Then Return 4326
        Return Nothing
    End Function

    Private Function PobierzIntTag(tag As TiffTag, domyslna As Integer) As Integer
        Dim pole As FieldValue() = _tiff.GetField(tag)
        If pole Is Nothing OrElse pole.Length = 0 Then Return domyslna
        Return pole(0).ToInt()
    End Function

    Private Function PobierzDoubleArrayTag(tag As TiffTag) As Double()
        Dim pole As FieldValue() = _tiff.GetField(tag)
        If pole Is Nothing OrElse pole.Length < 2 Then Return Nothing
        Return TryCast(pole(1).Value, Double())
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        SyncLock _lockOdczyt
            _cacheSlownik.Clear()
            _cacheLru.Clear()
            If _tiff IsNot Nothing Then
                _tiff.Dispose()
                _tiff = Nothing
            End If
        End SyncLock
    End Sub
End Class
