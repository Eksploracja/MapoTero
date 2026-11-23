' =====================================================================
' Generator kodów QR (ISO/IEC 18004) dla MapoTero
' Wersje 1–10 (od 21x21 do 57x57 modułów), poziom korekcji M (~15%).
' Czysty kod VB.NET bez zewnętrznych bibliotek (brak zależności NuGet).
' =====================================================================

Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Text

Public NotInheritable Class GeneratorQr

    Private Structure ParametryWersji
        Public ReadOnly RazemBajtow As Integer
        Public ReadOnly EcNaBlok As Integer
        Public ReadOnly LiczbaBlokow As Integer
        Public ReadOnly PozycjeWzorcow As Integer()

        Public Sub New(razem As Integer, ec As Integer, bloki As Integer, wzorce As Integer())
            RazemBajtow = razem
            EcNaBlok = ec
            LiczbaBlokow = bloki
            PozycjeWzorcow = wzorce
        End Sub

        Public ReadOnly Property BajtowDanych As Integer
            Get
                Return RazemBajtow - (EcNaBlok * LiczbaBlokow)
            End Get
        End Property
    End Structure

    ' Tabela ISO/IEC 18004 dla poziomu korekcji M (wersje 1–10, indeks 0 nieużywany)
    Private Shared ReadOnly TabelaWersjiM As ParametryWersji() = {
        Nothing,
        New ParametryWersji(26, 10, 1, Array.Empty(Of Integer)()),      ' V1:  21x21, maks.  14 B
        New ParametryWersji(44, 16, 1, {6, 18}),                        ' V2:  25x25, maks.  26 B
        New ParametryWersji(70, 26, 1, {6, 22}),                        ' V3:  29x29, maks.  42 B
        New ParametryWersji(100, 18, 2, {6, 26}),                       ' V4:  33x33, maks.  62 B
        New ParametryWersji(134, 24, 2, {6, 30}),                       ' V5:  37x37, maks.  84 B
        New ParametryWersji(172, 16, 4, {6, 34}),                       ' V6:  41x41, maks. 106 B
        New ParametryWersji(196, 18, 4, {6, 22, 38}),                   ' V7:  45x45, maks. 122 B
        New ParametryWersji(242, 22, 4, {6, 24, 42}),                   ' V8:  49x49, maks. 152 B
        New ParametryWersji(292, 22, 5, {6, 26, 46}),                   ' V9:  53x53, maks. 180 B
        New ParametryWersji(346, 26, 5, {6, 28, 50})                    ' V10: 57x57, maks. 213 B
    }

    Private ReadOnly _wersja As Integer
    Private ReadOnly _rozmiar As Integer
    Private ReadOnly _moduly As Boolean(,)
    Private ReadOnly _funkcyjne As Boolean(,)

    Public ReadOnly Property Wersja As Integer
        Get
            Return _wersja
        End Get
    End Property

    Public ReadOnly Property Rozmiar As Integer
        Get
            Return _rozmiar
        End Get
    End Property

    Public Function CzyCiemny(x As Integer, y As Integer) As Boolean
        Return _moduly(y, x)
    End Function

    Private Sub New(wersja As Integer, wszystkieBajty As Byte())
        _wersja = wersja
        _rozmiar = 17 + (4 * wersja)
        _moduly = New Boolean(_rozmiar - 1, _rozmiar - 1) {}
        _funkcyjne = New Boolean(_rozmiar - 1, _rozmiar - 1) {}

        RysujWzorceFunkcyjne()
        WpiszDaneZygzakiem(wszystkieBajty)

        ' Wybór optymalnej maski spośród 8 zdefiniowanych w ISO/IEC 18004
        Dim najlepszaMaska As Integer = 0
        Dim najnizszaKara As Integer = Integer.MaxValue

        For maska = 0 To 7
            ZastosujMaske(maska)
            ZapiszBityFormatu(maska)
            Dim kara = ObliczKare()
            If kara < najnizszaKara Then
                najnizszaKara = kara
                najlepszaMaska = maska
            End If
            ZastosujMaske(maska) ' Cofnięcie maski przez ponowny XOR
        Next

        ZastosujMaske(najlepszaMaska)
        ZapiszBityFormatu(najlepszaMaska)
    End Sub

    ' =====================================================================
    ' PUBLICZNE API
    ' =====================================================================

    ''' <summary>
    ''' Tworzy macierz kodu QR (poziom korekcji M) dla podanego tekstu UTF-8.
    ''' </summary>
    Public Shared Function Utworz(tekst As String) As GeneratorQr
        If tekst Is Nothing Then Throw New ArgumentNullException(NameOf(tekst))

        Dim bajtyTekstu As Byte() = Encoding.UTF8.GetBytes(tekst)
        Dim wersja As Integer = DobierzWersje(bajtyTekstu.Length)
        Dim param As ParametryWersji = TabelaWersjiM(wersja)

        Dim daneZakodowane As Byte() = ZakodujDane(bajtyTekstu, wersja, param.BajtowDanych)
        Dim wszystkieBajty As Byte() = DodajKorekcjeIPrzeplot(daneZakodowane, param)

        Return New GeneratorQr(wersja, wszystkieBajty)
    End Function

    ''' <summary>
    ''' Renderuje ostrą bitmapę QR o stałym rozmiarze pojedynczego modułu (w pikselach).
    ''' </summary>
    Public Shared Function GenerujBitmape(
        tekst As String,
        Optional rozmiarModuluPx As Integer = 6,
        Optional marginesModuly As Integer = 4) As Bitmap

        Dim qr = Utworz(tekst)
        Return qr.RenderujBitmape(rozmiarModuluPx, marginesModuly)
    End Function

    ''' <summary>
    ''' Renderuje ostrą bitmapę QR dopasowaną całkowitym mnożnikiem modułu do rozmiaru PictureBox.
    ''' W PictureBox ustaw SizeMode = PictureBoxSizeMode.CenterImage (bez rozmycia skalowania).
    ''' </summary>
    Public Shared Function GenerujBitmapeDopasowana(
        tekst As String,
        docelowyBokPx As Integer,
        Optional marginesModuly As Integer = 4) As Bitmap

        Dim qr = Utworz(tekst)
        Dim calkowiteModuly = qr.Rozmiar + (2 * Math.Max(0, marginesModuly))
        Dim modulPx = Math.Max(1, docelowyBokPx \ calkowiteModuly)
        Return qr.RenderujBitmape(modulPx, marginesModuly)
    End Function

    Public Function RenderujBitmape(
        Optional rozmiarModuluPx As Integer = 6,
        Optional marginesModuly As Integer = 4) As Bitmap

        If rozmiarModuluPx <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(rozmiarModuluPx))
        If marginesModuly < 0 Then Throw New ArgumentOutOfRangeException(NameOf(marginesModuly))

        Dim bokPx = (_rozmiar + (2 * marginesModuly)) * rozmiarModuluPx
        Dim bmp As New Bitmap(bokPx, bokPx, PixelFormat.Format24bppRgb)

        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.None
            g.InterpolationMode = InterpolationMode.NearestNeighbor
            g.PixelOffsetMode = PixelOffsetMode.None
            g.Clear(Color.White)

            For y = 0 To _rozmiar - 1
                Dim py = (y + marginesModuly) * rozmiarModuluPx
                For x = 0 To _rozmiar - 1
                    If _moduly(y, x) Then
                        Dim px = (x + marginesModuly) * rozmiarModuluPx
                        g.FillRectangle(Brushes.Black, px, py, rozmiarModuluPx, rozmiarModuluPx)
                    End If
                Next
            Next
        End Using

        Return bmp
    End Function

    ' =====================================================================
    ' KODOWANIE DANYCH (TRYB BAJTOWY) I REED-SOLOMON GF(256)
    ' =====================================================================

    Private Shared Function DobierzWersje(dlugoscBajtow As Integer) As Integer
        For v = 1 To TabelaWersjiM.Length - 1
            Dim bityNaglowka = 4 + If(v <= 9, 8, 16)
            Dim dostepneBity = TabelaWersjiM(v).BajtowDanych * 8
            If (bityNaglowka + (dlugoscBajtow * 8)) <= dostepneBity Then
                Return v
            End If
        Next

        Throw New ArgumentException(
            $"Tekst ({dlugoscBajtow} B w UTF-8) przekracza pojemność obsługiwanych wersji QR 1–10 (maks. 213 B dla korekcji M).",
            NameOf(dlugoscBajtow))
    End Function

    Private Shared Function ZakodujDane(bajtyTekstu As Byte(), wersja As Integer, bajtowDanych As Integer) As Byte()
        Dim maxBitow = bajtowDanych * 8
        Dim bity As New List(Of Boolean)(maxBitow)

        ' Tryb bajtowy = 0100 (4)
        DopiszBity(bity, &H4, 4)
        ' Licznik bajtów: 8 bitów dla V1–V9, 16 bitów dla V10+
        DopiszBity(bity, bajtyTekstu.Length, If(wersja <= 9, 8, 16))

        For Each b In bajtyTekstu
            DopiszBity(bity, b, 8)
        Next

        ' Terminator (maks. 4 zera)
        Dim terminator = Math.Min(4, maxBitow - bity.Count)
        DopiszBity(bity, 0, terminator)

        ' Wyrównanie do pełnego bajtu
        Dim reszta = bity.Count Mod 8
        If reszta <> 0 Then
            DopiszBity(bity, 0, 8 - reszta)
        End If

        ' Dopełnienie naprzemienne 0xEC i 0x11
        Dim padByte As Integer = &HEC
        While bity.Count < maxBitow
            DopiszBity(bity, padByte, 8)
            padByte = If(padByte = &HEC, &H11, &HEC)
        End While

        Dim wynik(bajtowDanych - 1) As Byte
        For i = 0 To bity.Count - 1
            If bity(i) Then
                wynik(i >> 3) = wynik(i >> 3) Or CByte(1 << (7 - (i And 7)))
            End If
        Next
        Return wynik
    End Function

    Private Shared Sub DopiszBity(bufor As List(Of Boolean), wartosc As Integer, iloscBitow As Integer)
        For i = iloscBitow - 1 To 0 Step -1
            bufor.Add(((wartosc >> i) And 1) <> 0)
        Next
    End Sub

    Private Shared Function DodajKorekcjeIPrzeplot(dane As Byte(), param As ParametryWersji) As Byte()
        Dim liczbaBlokow = param.LiczbaBlokow
        Dim krotkiBlokDl = param.BajtowDanych \ liczbaBlokow
        Dim dlugichBlokow = param.BajtowDanych Mod liczbaBlokow
        Dim krotkichBlokow = liczbaBlokow - dlugichBlokow

        Dim wielomianRs = GenerujWielomianRs(param.EcNaBlok)
        Dim blokiDanych(liczbaBlokow - 1)() As Byte
        Dim blokiEc(liczbaBlokow - 1)() As Byte

        Dim offset As Integer = 0
        For b = 0 To liczbaBlokow - 1
            Dim dlBloku = krotkiBlokDl + If(b >= krotkichBlokow, 1, 0)
            Dim blok(dlBloku - 1) As Byte
            Array.Copy(dane, offset, blok, 0, dlBloku)
            offset += dlBloku

            blokiDanych(b) = blok
            blokiEc(b) = ObliczReszteRs(blok, wielomianRs)
        Next

        Dim wynik As New List(Of Byte)(param.RazemBajtow)

        ' Przeplot słów danych
        Dim maxDlBloku = krotkiBlokDl + If(dlugichBlokow > 0, 1, 0)
        For i = 0 To maxDlBloku - 1
            For b = 0 To liczbaBlokow - 1
                If i < blokiDanych(b).Length Then
                    wynik.Add(blokiDanych(b)(i))
                End If
            Next
        Next

        ' Przeplot słów korekcji błędów
        For i = 0 To param.EcNaBlok - 1
            For b = 0 To liczbaBlokow - 1
                wynik.Add(blokiEc(b)(i))
            Next
        Next

        Return wynik.ToArray()
    End Function

    Private Shared Function GfMnoz(a As Byte, b As Byte) As Byte
        ' Mnożenie w GF(256) modulo wielomian pierwotny x^8 + x^4 + x^3 + x^2 + 1 (0x11D)
        Dim z As Integer = 0
        Dim x As Integer = a
        Dim y As Integer = b
        For i = 7 To 0 Step -1
            z = (z << 1) Xor ((z >> 7) * &H11D)
            z = z Xor (((y >> i) And 1) * x)
        Next
        Return CByte(z)
    End Function

    Private Shared Function GenerujWielomianRs(stopien As Integer) As Byte()
        Dim poly(stopien - 1) As Byte
        poly(stopien - 1) = 1
        Dim pierwiastek As Byte = 1

        For i = 0 To stopien - 1
            For j = 0 To stopien - 1
                poly(j) = GfMnoz(poly(j), pierwiastek)
                If j + 1 < stopien Then
                    poly(j) = poly(j) Xor poly(j + 1)
                End If
            Next
            pierwiastek = GfMnoz(pierwiastek, 2)
        Next

        Return poly
    End Function

    Private Shared Function ObliczReszteRs(dane As Byte(), dzielnik As Byte()) As Byte()
        Dim d = dzielnik.Length
        Dim wynik(d - 1) As Byte

        For Each bajt In dane
            Dim czynnik As Byte = bajt Xor wynik(0)
            Array.Copy(wynik, 1, wynik, 0, d - 1)
            wynik(d - 1) = 0
            For j = 0 To d - 1
                wynik(j) = wynik(j) Xor GfMnoz(dzielnik(j), czynnik)
            Next
        Next

        Return wynik
    End Function

    ' =====================================================================
    ' WZORCE FUNKCYJNE, FORMAT, WERSJA I ZYGZAK
    ' =====================================================================

    Private Sub UstawFunkcyjny(x As Integer, y As Integer, ciemny As Boolean)
        _moduly(y, x) = ciemny
        _funkcyjne(y, x) = True
    End Sub

    Private Sub RysujWzorceFunkcyjne()
        ' 1. Paski synchronizacji (timing patterns)
        For i = 0 To _rozmiar - 1
            UstawFunkcyjny(6, i, (i Mod 2) = 0)
            UstawFunkcyjny(i, 6, (i Mod 2) = 0)
        Next

        ' 2. Trzy znaczniki narożne (finder patterns + białe separatory)
        RysujZnacznikNarozny(3, 3)
        RysujZnacznikNarozny(_rozmiar - 4, 3)
        RysujZnacznikNarozny(3, _rozmiar - 4)

        ' 3. Wzorce wyrównujące (alignment patterns) dla V2+
        Dim pozycje = TabelaWersjiM(_wersja).PozycjeWzorcow
        If pozycje.Length > 0 Then
            Dim ostatni = pozycje(pozycje.Length - 1)
            For Each cy In pozycje
                For Each cx In pozycje
                    If (cx = 6 AndAlso cy = 6) OrElse
                       (cx = 6 AndAlso cy = ostatni) OrElse
                       (cx = ostatni AndAlso cy = 6) Then
                        Continue For
                    End If
                    RysujWzorzecWyrownujacy(cx, cy)
                Next
            Next
        End If

        ' 4. Rezerwacja pól informacji o formacie (tymczasowo maska 0) i wersji (V7+)
        ZapiszBityFormatu(0)
        If _wersja >= 7 Then
            ZapiszBityWersji()
        End If
    End Sub

    Private Sub RysujZnacznikNarozny(cx As Integer, cy As Integer)
        For dy = -4 To 4
            Dim y = cy + dy
            If y < 0 OrElse y >= _rozmiar Then Continue For
            For dx = -4 To 4
                Dim x = cx + dx
                If x < 0 OrElse x >= _rozmiar Then Continue For
                Dim dystans = Math.Max(Math.Abs(dx), Math.Abs(dy))
                UstawFunkcyjny(x, y, dystans <> 2 AndAlso dystans <> 4)
            Next
        Next
    End Sub

    Private Sub RysujWzorzecWyrownujacy(cx As Integer, cy As Integer)
        For dy = -2 To 2
            For dx = -2 To 2
                Dim dystans = Math.Max(Math.Abs(dx), Math.Abs(dy))
                UstawFunkcyjny(cx + dx, cy + dy, dystans <> 1)
            Next
        Next
    End Sub

    Private Sub ZapiszBityFormatu(maska As Integer)
        ' Poziom korekcji M = 00 (0), BCH(15,5) modulo 0x537, maska XOR 0x5412
        Dim dane As Integer = (0 << 3) Or (maska And 7)
        Dim remBity As Integer = dane
        For i = 0 To 9
            remBity = (remBity << 1) Xor ((remBity >> 9) * &H537)
        Next
        Dim bity As Integer = ((dane << 10) Or remBity) Xor &H5412

        ' Pierwsza kopia (wokół lewego górnego znacznika)
        For i = 0 To 5
            UstawFunkcyjny(8, i, ((bity >> i) And 1) <> 0)
        Next
        UstawFunkcyjny(8, 7, ((bity >> 6) And 1) <> 0)
        UstawFunkcyjny(8, 8, ((bity >> 7) And 1) <> 0)
        UstawFunkcyjny(7, 8, ((bity >> 8) And 1) <> 0)
        For i = 9 To 14
            UstawFunkcyjny(14 - i, 8, ((bity >> i) And 1) <> 0)
        Next

        ' Druga kopia (prawy górny i lewy dolny znacznik)
        For i = 0 To 7
            UstawFunkcyjny(_rozmiar - 1 - i, 8, ((bity >> i) And 1) <> 0)
        Next
        For i = 8 To 14
            UstawFunkcyjny(8, _rozmiar - 15 + i, ((bity >> i) And 1) <> 0)
        Next

        ' Stały czarny moduł (dark module)
        UstawFunkcyjny(8, _rozmiar - 8, True)
    End Sub

    Private Sub ZapiszBityWersji()
        ' BCH(18,6) modulo 0x1F25 (tylko dla V7–V10)
        Dim remBity As Integer = _wersja
        For i = 0 To 11
            remBity = (remBity << 1) Xor ((remBity >> 11) * &H1F25)
        Next
        Dim bity As Integer = (_wersja << 12) Or remBity

        For i = 0 To 17
            Dim ciemny = ((bity >> i) And 1) <> 0
            Dim a = _rozmiar - 11 + (i Mod 3)
            Dim b = i \ 3
            UstawFunkcyjny(b, a, ciemny) ' Lewy dół
            UstawFunkcyjny(a, b, ciemny) ' Prawa góra
        Next
    End Sub

    Private Sub WpiszDaneZygzakiem(bajty As Byte())
        Dim indeksBitu As Integer = 0
        Dim razemBitow As Integer = bajty.Length * 8

        Dim prawa As Integer = _rozmiar - 1
        While prawa >= 1
            If prawa = 6 Then prawa = 5 ' Pominięcie pionowego paska synchronizacji

            Dim wGore As Boolean = ((prawa + 1) And 2) = 0
            For pion = 0 To _rozmiar - 1
                Dim y = If(wGore, _rozmiar - 1 - pion, pion)
                For j = 0 To 1
                    Dim x = prawa - j
                    If Not _funkcyjne(y, x) AndAlso indeksBitu < razemBitow Then
                        Dim bit = ((bajty(indeksBitu >> 3) >> (7 - (indeksBitu And 7))) And 1) <> 0
                        _moduly(y, x) = bit
                        indeksBitu += 1
                    End If
                Next
            Next

            prawa -= 2
        End While
    End Sub

    ' =====================================================================
    ' MASKOWANIE I OCENA JAKOŚCI (4 REGUŁY KAR ISO/IEC 18004)
    ' =====================================================================

    Private Sub ZastosujMaske(maska As Integer)
        For y = 0 To _rozmiar - 1
            For x = 0 To _rozmiar - 1
                If _funkcyjne(y, x) Then Continue For

                Dim odwroc As Boolean
                Select Case maska
                    Case 0 : odwroc = ((x + y) Mod 2) = 0
                    Case 1 : odwroc = (y Mod 2) = 0
                    Case 2 : odwroc = (x Mod 3) = 0
                    Case 3 : odwroc = ((x + y) Mod 3) = 0
                    Case 4 : odwroc = (((y \ 2) + (x \ 3)) Mod 2) = 0
                    Case 5 : odwroc = (((x * y) Mod 2) + ((x * y) Mod 3)) = 0
                    Case 6 : odwroc = ((((x * y) Mod 2) + ((x * y) Mod 3)) Mod 2) = 0
                    Case 7 : odwroc = ((((x + y) Mod 2) + ((x * y) Mod 3)) Mod 2) = 0
                    Case Else : odwroc = False
                End Select

                If odwroc Then _moduly(y, x) = Not _moduly(y, x)
            Next
        Next
    End Sub

    Private Function ObliczKare() As Integer
        Dim kara As Integer = 0

        ' Reguła 1 (serie >= 5) oraz Reguła 3 (wzorzec 1:1:3:1:1 z 4 białymi po boku) — wiersze i kolumny
        For y = 0 To _rozmiar - 1
            kara += KaraLinii(y, poWierszu:=True)
        Next
        For x = 0 To _rozmiar - 1
            kara += KaraLinii(x, poWierszu:=False)
        Next

        ' Reguła 2: bloki 2x2 tego samego koloru (+3 za każdy)
        For y = 0 To _rozmiar - 2
            For x = 0 To _rozmiar - 2
                Dim c = _moduly(y, x)
                If c = _moduly(y, x + 1) AndAlso c = _moduly(y + 1, x) AndAlso c = _moduly(y + 1, x + 1) Then
                    kara += 3
                End If
            Next
        Next

        ' Reguła 4: bilans czarnych i białych modułów (+10 za każde pełne 5% odchylenia od 50%)
        Dim ciemnych As Integer = 0
        For y = 0 To _rozmiar - 1
            For x = 0 To _rozmiar - 1
                If _moduly(y, x) Then ciemnych += 1
            Next
        Next
        Dim razem = _rozmiar * _rozmiar
        Dim odchylenie = Math.Abs((ciemnych * 20) - (razem * 10)) \ razem
        kara += odchylenie * 10

        Return kara
    End Function

    Private Function KaraLinii(indeks As Integer, poWierszu As Boolean) As Integer
        Dim kara As Integer = 0

        ' Reguła 1: ciągłe serie >= 5 tego samego koloru
        Dim poprzedni = If(poWierszu, _moduly(indeks, 0), _moduly(0, indeks))
        Dim seria As Integer = 1
        For i = 1 To _rozmiar - 1
            Dim obecny = If(poWierszu, _moduly(indeks, i), _moduly(i, indeks))
            If obecny = poprzedni Then
                seria += 1
                If seria = 5 Then
                    kara += 3
                ElseIf seria > 5 Then
                    kara += 1
                End If
            Else
                poprzedni = obecny
                seria = 1
            End If
        Next

        ' Reguła 3: 11-bitowe okno dla 10111010000 (0x5D0) i 00001011101 (0x05D) z 4 białymi marginesu
        Dim okno As Integer = 0
        For i = -4 To _rozmiar + 3
            Dim ciemny = (i >= 0 AndAlso i < _rozmiar) AndAlso
                         If(poWierszu, _moduly(indeks, i), _moduly(i, indeks))
            okno = ((okno << 1) And &H7FF) Or If(ciemny, 1, 0)
            If i >= 6 AndAlso (okno = &H5D0 OrElse okno = &H5D) Then
                kara += 40
            End If
        Next

        Return kara
    End Function

End Class
