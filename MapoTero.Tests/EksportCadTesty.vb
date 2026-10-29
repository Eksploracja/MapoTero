'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports MapoTero.Core
Imports Xunit

Public Class EksportCadTesty

    Shared Sub New()
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    End Sub

    ''' <summary>Siatka 2 x 2 segmenty po 256 px, piksel 1 m (PL-1992).</summary>
    Private Shared Function Siatka2x2() As Siatka
        Return New Siatka(New Zasieg(487000, 637000, 487512, 637512), 1.0, 256)
    End Function

    Private Shared Function NowyFolder() As String
        Dim f = Path.Combine(Path.GetTempPath(), "MapoTero_Cad_" & Guid.NewGuid().ToString("N")) & Path.DirectorySeparatorChar
        Directory.CreateDirectory(f)
        Return f
    End Function

    ''' <summary>Zadanie z 4 segmentami siatki 2 x 2 w folderze C:\mapy\sesja\.</summary>
    Private Shared Function Zadanie() As ZadanieCad
        Dim s = Siatka2x2()
        Dim z As New ZadanieCad With {.Uklad = UkladWspolrzednych.PL1992, .Siatka = s, .PrzedrostekNazwObrazow = "MT_test"}
        For w = 1 To 2
            For k = 1 To 2
                Dim nazwa = s.NazwaSegmentu("_", StylNumeracji.WierszKolumna, w, k) & ".jpg"
                z.Rastry.Add(New ObrazGeoreferencyjny With {.Uklad = z.Uklad, .Zasieg = s.Segment(w, k), .SzerokoscPx = 256, .WysokoscPx = 256,
                                                            .NazwaPliku = nazwa, .SciezkaPliku = "C:\mapy\sesja\" & nazwa})
            Next
        Next
        Return z
    End Function

    ''' <summary>Pary (kod grupowy, wartość) pliku DXF.</summary>
    Private Shared Function Pary(dxf As String) As List(Of KeyValuePair(Of Integer, String))
        Dim wiersze = dxf.Split({vbCrLf}, StringSplitOptions.None)
        Dim wynik As New List(Of KeyValuePair(Of Integer, String))
        For i = 0 To wiersze.Length - 2 Step 2
            wynik.Add(New KeyValuePair(Of Integer, String)(Integer.Parse(wiersze(i).Trim(), CultureInfo.InvariantCulture), wiersze(i + 1)))
        Next
        Return wynik
    End Function

    ''' <summary>Obiekty DXF (od kodu 0 do następnego kodu 0): typ i lista par.</summary>
    Private Shared Function Obiekty(p As List(Of KeyValuePair(Of Integer, String))) As List(Of KeyValuePair(Of String, List(Of KeyValuePair(Of Integer, String))))
        Dim wynik As New List(Of KeyValuePair(Of String, List(Of KeyValuePair(Of Integer, String))))
        For Each para In p
            If para.Key = 0 Then
                wynik.Add(New KeyValuePair(Of String, List(Of KeyValuePair(Of Integer, String)))(para.Value, New List(Of KeyValuePair(Of Integer, String))))
            ElseIf wynik.Count > 0 Then
                wynik(wynik.Count - 1).Value.Add(para)
            End If
        Next
        Return wynik
    End Function

    Private Shared Function Wartosc(o As List(Of KeyValuePair(Of Integer, String)), kod As Integer) As String
        For Each p In o
            If p.Key = kod Then Return p.Value
        Next
        Return Nothing
    End Function

    Private Shared Function Liczba(t As String) As Double
        Return Double.Parse(t, CultureInfo.InvariantCulture)
    End Function

    <Fact>
    Public Sub DxfStrukturaIUchwyty()
        Dim dxf = EksportCad.Dxf(Zadanie(), Encoding.GetEncoding(1250))
        Dim p = Pary(dxf)
        Assert.Equal("SECTION", p(0).Value)
        Assert.Equal("EOF", p(p.Count - 1).Value)
        'kolejność sekcji
        Dim sekcje As New List(Of String)
        For i = 0 To p.Count - 2
            If p(i).Key = 0 AndAlso p(i).Value = "SECTION" Then sekcje.Add(p(i + 1).Value)
        Next
        Assert.Equal({"HEADER", "CLASSES", "TABLES", "BLOCKS", "ENTITIES", "OBJECTS"}, sekcje.ToArray())

        Dim ob = Obiekty(p)
        'uchwyty niepowtarzalne i mniejsze od $HANDSEED
        Dim uchwyty As New HashSet(Of String)
        Dim maks As Long = 0
        For Each o In ob
            For Each para In o.Value
                If (para.Key = 5 OrElse para.Key = 105) AndAlso o.Key <> "SECTION" Then
                    If o.Key = "SECTION" Then Continue For
                    'kod 5 w nagłówku to wartość $HANDSEED
                    If Wartosc(o.Value, 9) IsNot Nothing Then Continue For
                    Assert.True(uchwyty.Add(para.Value), "powtórzony uchwyt " & para.Value)
                    maks = Math.Max(maks, Convert.ToInt64(para.Value, 16))
                End If
            Next
        Next
        Dim iSeed = p.FindIndex(Function(x) x.Key = 9 AndAlso x.Value = "$HANDSEED")
        Assert.True(Convert.ToInt64(p(iSeed + 1).Value, 16) > maks)
        Assert.Equal("AC1015", p(p.FindIndex(Function(x) x.Value = "$ACADVER") + 1).Value)
        Assert.Equal("ANSI_1250", p(p.FindIndex(Function(x) x.Value = "$DWGCODEPAGE") + 1).Value)

        'każdy obraz wskazuje swoją definicję i reaktor, a reaktor - obraz
        Dim definicje = ob.Where(Function(o) o.Key = "IMAGEDEF").ToDictionary(Function(o) Wartosc(o.Value, 5))
        Dim reaktory = ob.Where(Function(o) o.Key = "IMAGEDEF_REACTOR").ToDictionary(Function(o) Wartosc(o.Value, 5))
        Dim obrazy = ob.Where(Function(o) o.Key = "IMAGE").ToList()
        Assert.Equal(4, obrazy.Count)
        Assert.Equal(4, definicje.Count)
        For Each o In obrazy
            Dim h = Wartosc(o.Value, 5)
            Assert.True(definicje.ContainsKey(Wartosc(o.Value, 340)))
            Dim r = reaktory(Wartosc(o.Value, 360))
            Assert.Equal(h, Wartosc(r.Value, 330))
            Assert.Equal(EksportCad.WarstwaRastrow, Wartosc(o.Value, 8))
        Next
        'słownik obrazów zawiera wszystkie definicje
        Dim slownik = ob.First(Function(o) o.Key = "DICTIONARY" AndAlso o.Value.Any(Function(x) x.Key = 3 AndAlso x.Value = "ACAD_IMAGE_DICT"))
        Dim hSlownikaObrazow = slownik.Value(slownik.Value.FindIndex(Function(x) x.Key = 3 AndAlso x.Value = "ACAD_IMAGE_DICT") + 1).Value
        Dim slownikObrazow = ob.First(Function(o) o.Key = "DICTIONARY" AndAlso Wartosc(o.Value, 5) = hSlownikaObrazow)
        Assert.Equal(4, slownikObrazow.Value.Where(Function(x) x.Key = 350).Count())
        'klasy obrazów
        Assert.Equal(4, ob.Where(Function(o) o.Key = "CLASS").Count())
    End Sub

    <Fact>
    Public Sub DxfPolozenieObrazuOsieCad()
        Dim z = Zadanie()
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Dim obraz = ob.First(Function(o) o.Key = "IMAGE").Value
        'segment (1, 1) - lewy górny: X 487256..487512 (północ), Y 637000..637256 (wschód); w CAD: x = wschód, y = północ
        Assert.Equal(637000.0, Liczba(Wartosc(obraz, 10)))
        Assert.Equal(487256.0, Liczba(Wartosc(obraz, 20)))
        Assert.Equal(1.0, Liczba(Wartosc(obraz, 11)))
        Assert.Equal(0.0, Liczba(Wartosc(obraz, 21)))
        Assert.Equal(0.0, Liczba(Wartosc(obraz, 12)))
        Assert.Equal(1.0, Liczba(Wartosc(obraz, 22)))
        Assert.Equal("256", Wartosc(obraz, 13))
        Assert.Equal("256", Wartosc(obraz, 23))
        Dim def = ob.First(Function(o) o.Key = "IMAGEDEF").Value
        Assert.Equal("C:\mapy\sesja\_01_01.jpg", Wartosc(def, 1))
    End Sub

    <Fact>
    Public Sub DxfSiatkaNumeryIObrys()
        Dim ob = Obiekty(Pary(EksportCad.Dxf(Zadanie(), Encoding.ASCII)))
        'siatka 2 x 2 z obrysem: tylko wewnętrzna linia pionowa i pozioma (zewnętrzne pokrywałyby się z obrysem), 4 numery, obrys
        Dim linie = ob.Where(Function(o) o.Key = "LINE").Select(Function(o) o.Value).ToList()
        Assert.Equal(2, linie.Count)
        Assert.Equal(637256.0, Liczba(Wartosc(linie(0), 10)))
        Assert.Equal(637256.0, Liczba(Wartosc(linie(0), 11)))
        Assert.Equal(487256.0, Liczba(Wartosc(linie(1), 20)))
        Dim teksty = ob.Where(Function(o) o.Key = "TEXT").Select(Function(o) Wartosc(o.Value, 1)).ToList()
        Assert.Equal({"_01_01", "_01_02", "_02_01", "_02_02"}, teksty.ToArray())
        Dim obrys = ob.Single(Function(o) o.Key = "LWPOLYLINE").Value
        Assert.Equal(EksportCad.WarstwaObrysu, Wartosc(obrys, 8))
        Assert.Equal("1", Wartosc(obrys, 70))
        Dim xs = obrys.Where(Function(x) x.Key = 10).Select(Function(x) Liczba(x.Value)).ToList()
        Dim ys = obrys.Where(Function(x) x.Key = 20).Select(Function(x) Liczba(x.Value)).ToList()
        Assert.Equal(637000.0, xs.Min())
        Assert.Equal(637512.0, xs.Max())
        Assert.Equal(487000.0, ys.Min())
        Assert.Equal(487512.0, ys.Max())
    End Sub

    <Fact>
    Public Sub OpcjeEksportuCadWLastsettings()
        Dim plik = Path.GetTempFileName()
        Try
            Dim u As New MapoTero.UstawieniaProgramu With {.CadObrys = False, .CadNumery = False, .CadScr = False, .CadSciezkiWzgledne = False}
            u.Zapisz(plik)
            Dim w As New MapoTero.UstawieniaProgramu()
            w.Wczytaj(plik)
            Assert.False(w.CadObrys)
            Assert.True(w.CadSiatka)
            Assert.False(w.CadNumery)
            Assert.True(w.CadDxf)
            Assert.False(w.CadScr)
            Assert.False(w.CadSciezkiWzgledne)

            'bez żadnego pliku wynikowego przycisk Eksportuj byłby nieaktywny - wraca rysunek DXF
            Dim bez As New MapoTero.UstawieniaProgramu With {.CadDxf = False, .CadScr = False}
            bez.Zapisz(plik)
            Dim w2 As New MapoTero.UstawieniaProgramu()
            w2.Wczytaj(plik)
            Assert.True(w2.CadDxf)

            'plik ze starszej wersji (bez opcji CAD) - wartości domyślne
            File.WriteAllText(plik, "watki" & vbCrLf & "4" & vbCrLf)
            Dim w3 As New MapoTero.UstawieniaProgramu()
            w3.Wczytaj(plik)
            Assert.True(w3.CadObrys AndAlso w3.CadNumery AndAlso w3.CadDxf AndAlso w3.CadScr)
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub SiatkaBezObrysuZKrawedziami()
        Dim z = Zadanie()
        z.RysujObrys = False
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        'bez obrysu siatka ma także krawędzie zewnętrzne: 3 linie pionowe i 3 poziome
        Assert.Equal(6, ob.Where(Function(o) o.Key = "LINE").Count())
        Assert.False(ob.Any(Function(o) o.Key = "LWPOLYLINE"))
        Assert.Equal(6, EksportCad.Skrypt(z).Split({vbCrLf}, StringSplitOptions.None).Where(Function(x) x = "_LINE").Count())
    End Sub

    <Fact>
    Public Sub DxfKorytarzProstokatySegmentow()
        Dim z = Zadanie()
        z.SegmentyKorytarza = New HashSet(Of Integer) From {0, 3}
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.Equal(0, ob.Where(Function(o) o.Key = "LINE").Count())
        Assert.Equal(2, ob.Where(Function(o) o.Key = "LWPOLYLINE" AndAlso Wartosc(o.Value, 8) = EksportCad.WarstwaSiatki).Count())
        Assert.Equal({"_01_01", "_02_02"}, ob.Where(Function(o) o.Key = "TEXT").Select(Function(o) Wartosc(o.Value, 1)).ToArray())
    End Sub

    <Fact>
    Public Sub DxfBezElementowWektorowych()
        Dim z = Zadanie()
        z.RysujObrys = False
        z.RysujSiatke = False
        z.RysujNumery = False
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.False(ob.Any(Function(o) o.Key = "LINE" OrElse o.Key = "LWPOLYLINE" OrElse o.Key = "TEXT"))
        Assert.Equal(4, ob.Where(Function(o) o.Key = "IMAGE").Count())
    End Sub

    <Fact>
    Public Sub DxfZnakiSpozaStronyKodowej()
        Dim z = Zadanie()
        z.Opis = "Mapa: Łódź β^x"
        Dim d1250 = EksportCad.Dxf(z, Encoding.GetEncoding(1250))
        Assert.Contains("Mapa: Łódź \U+03B2^ x", d1250)
        Dim dAscii = EksportCad.Dxf(z, Encoding.ASCII)
        Assert.Contains("Mapa: \U+0141\U+00F3d\U+017A \U+03B2^ x", dAscii)
        Assert.Contains("ANSI_1252", dAscii)
    End Sub

    <Fact>
    Public Sub KodowanieRysunkuBezBomIStronaAnsi()
        Assert.Equal(1250, EksportCad.KodowanieRysunku(Encoding.UTF8).CodePage)
        Assert.Equal(1250, EksportCad.KodowanieRysunku(Nothing).CodePage)
        Assert.Equal(1252, EksportCad.KodowanieRysunku(Encoding.GetEncoding(1252)).CodePage)
        Assert.Empty(EksportCad.KodowanieRysunku(Encoding.UTF8).GetPreamble())
        Dim k = Encoding.GetEncoding(1250)
        Assert.False(EksportCad.ZnakiSpozaKodowania("C:\mapy\Łódź\_01_01.jpg", k))
        Assert.True(EksportCad.ZnakiSpozaKodowania("C:\mapy\Ελλάδα\_01_01.jpg", k))
    End Sub

    <Fact>
    Public Sub DxfBezOpisuIPapierZFlaga()
        Dim z = Zadanie()
        z.Opis = Nothing
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.False(ob.Any(Function(o) o.Key = "TEXT" AndAlso Wartosc(o.Value, 8) = EksportCad.WarstwaOpisu))
        Dim papier = ob.First(Function(o) o.Key = "BLOCK" AndAlso Wartosc(o.Value, 2) = "*Paper_Space").Value
        Assert.Equal("1", Wartosc(papier, 67))
        Dim model = ob.First(Function(o) o.Key = "BLOCK" AndAlso Wartosc(o.Value, 2) = "*Model_Space").Value
        Assert.Null(Wartosc(model, 67))
    End Sub

    <Fact>
    Public Sub DxfSciezkiWzgledne()
        Dim z = Zadanie()
        z.FolderRysunku = "C:\mapy\sesja"
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.Equal(".\_01_01.jpg", Wartosc(ob.First(Function(o) o.Key = "IMAGEDEF").Value, 1))
        z.FolderRysunku = "C:\inny\"
        ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.Equal("C:\mapy\sesja\_01_01.jpg", Wartosc(ob.First(Function(o) o.Key = "IMAGEDEF").Value, 1))
    End Sub

    <Fact>
    Public Sub NazwyObrazowNiepowtarzalne()
        Dim z = Zadanie()
        Dim nazwy = EksportCad.NazwyObrazow(z)
        Assert.Equal("MT_test__01_01", nazwy(0))
        Assert.Equal(4, nazwy.Distinct().Count())
        Assert.Equal("Lodz_ae_x_y", EksportCad.NazwaObiektu("Łódź ąę x.y"))
        Dim a = EksportCad.PrzedrostekDlaFolderu("C:\mapy\sesja\")
        Assert.Equal(a, EksportCad.PrzedrostekDlaFolderu("c:\MAPY\sesja"))
        Assert.NotEqual(a, EksportCad.PrzedrostekDlaFolderu("C:\mapy2\sesja\"))
        Assert.StartsWith("MT_sesja_", a)
    End Sub

    <Fact>
    Public Sub SkryptWstawiaRastryNaWspolrzednych()
        Dim scr = EksportCad.Skrypt(Zadanie())
        Dim w = scr.Split({vbCrLf}, StringSplitOptions.None)
        'pierwszy raster: nazwa=ścieżka, punkt bez przyciągania, skala = szerokość w metrach, obrót 0
        Dim i = Array.IndexOf(w, "_-IMAGE")
        Assert.True(i > 0)
        Assert.Equal("_ATTACH", w(i + 1))
        Assert.Equal("MT_test__01_01=""C:\mapy\sesja\_01_01.jpg""", w(i + 2))
        Assert.Equal("_non", w(i + 3))
        Assert.Equal("637000,487256", w(i + 4))
        Assert.Equal("256", w(i + 5))
        'obrót wskazany punktem na wschód (prawy dolny narożnik) - niezależnie od ANGBASE / ANGDIR
        Assert.Equal("_non", w(i + 6))
        Assert.Equal("637256,487256", w(i + 7))
        'raster pod istniejące obiekty rysunku
        Assert.Equal({"_DRAWORDER", "_LAST", "", "_BACK"}, w.Skip(i + 8).Take(4).ToArray())
        Assert.Equal(4, w.Where(Function(x) x = "_-IMAGE").Count())
        Assert.Equal(4, w.Where(Function(x) x = "_DRAWORDER").Count())
        'INSUNITS = 0 przed wstawieniem rastrów, na koniec metry; współrzędne w WCS, na koniec poprzedni UCS
        Assert.True(Array.IndexOf(w, "_SETVAR INSUNITS 0") < i)
        Assert.True(Array.IndexOf(w, "_SETVAR INSUNITS 6") > i)
        Assert.True(Array.IndexOf(w, "_UCS _WORLD") < i)
        Assert.True(Array.IndexOf(w, "_UCS _PREVIOUS") > i)
        'warstwa rastrów: utworzona, odmrożona, włączona i bieżąca
        Dim iw = Array.IndexOf(w, "_-LAYER")
        Assert.Equal({"_NEW", EksportCad.WarstwaRastrow, "_THAW", EksportCad.WarstwaRastrow, "_ON", EksportCad.WarstwaRastrow,
                      "_UNLOCK", EksportCad.WarstwaRastrow, "_SET", EksportCad.WarstwaRastrow, "_COLOR", "7", EksportCad.WarstwaRastrow, ""},
                     w.Skip(iw + 1).Take(14).ToArray())
        Assert.Equal("_UNDO _BEGIN", w.First(Function(x) Not x.StartsWith(";")))
        Assert.Equal("_UNDO _END", w(w.Length - 2))
        Assert.Equal("", w(w.Length - 1))
        'siatka - 2 linie wewnętrzne (zewnętrzne pokrywa obrys), obrys - zamknięta polilinia
        Assert.Equal(2, w.Where(Function(x) x = "_LINE").Count())
        Assert.Equal(1, w.Where(Function(x) x = "_PLINE").Count())
        Assert.Contains("_CLOSE", w)
        'każdy punkt poprzedzony _non
        For j = 1 To w.Length - 1
            If w(j).Contains(",") AndAlso Not w(j).StartsWith(";") AndAlso Not w(j).Contains("=") Then Assert.Equal("_non", w(j - 1))
        Next
        'skrypt w ASCII (komentarze bez polskich znaków)
        Assert.True(scr.All(Function(c) AscW(c) < 128))
    End Sub

    <Fact>
    Public Sub NakladkaPngPrzezroczysta()
        Dim z = Zadanie()
        For Each r In z.Rastry
            r.NazwaPliku = Path.ChangeExtension(r.NazwaPliku, ".png")
            r.SciezkaPliku = Path.ChangeExtension(r.SciezkaPliku, ".png")
        Next
        Dim w = EksportCad.Skrypt(z).Split({vbCrLf}, StringSplitOptions.None)
        Dim i = Array.IndexOf(w, "_-IMAGE")
        Assert.Equal({"_TRANSPARENCY", "_LAST", "", "_ON", "_DRAWORDER"}, w.Skip(i + 8).Take(5).ToArray())
        Assert.Equal(4, w.Where(Function(x) x = "_TRANSPARENCY").Count())
        Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
        Assert.Equal("15", Wartosc(ob.First(Function(o) o.Key = "IMAGE").Value, 70))
        'segmenty JPEG - bez przezroczystości
        Assert.DoesNotContain("_TRANSPARENCY", EksportCad.Skrypt(Zadanie()))
        Assert.Equal("7", Wartosc(Obiekty(Pary(EksportCad.Dxf(Zadanie(), Encoding.ASCII))).First(Function(o) o.Key = "IMAGE").Value, 70))
    End Sub

    <Fact>
    Public Sub SkryptOstrzegaPrzyPikselachNiekwadratowych()
        Dim z = Zadanie()
        z.Rastry.Clear()
        z.Rastry.Add(New ObrazGeoreferencyjny With {.Zasieg = New Zasieg(487000, 637000, 487100, 637400), .SzerokoscPx = 200, .WysokoscPx = 100,
                                                    .NazwaPliku = "arkusz.jpg", .SciezkaPliku = "C:\mapy\arkusz.jpg"})
        EksportCad.Skrypt(z)
        Assert.Single(z.Uwagi)
        Assert.Contains("niekwadratowe", z.Uwagi(0))
        'DXF zapisuje oba wektory piksela
        Dim obraz = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII))).First(Function(o) o.Key = "IMAGE").Value
        Assert.Equal(2.0, Liczba(Wartosc(obraz, 11)))
        Assert.Equal(1.0, Liczba(Wartosc(obraz, 22)))
        'kwadratowe - bez uwag
        Dim z2 = Zadanie()
        EksportCad.Skrypt(z2)
        Assert.Empty(z2.Uwagi)
    End Sub

    ''' <summary>Nagłówek GeoTIFF 64 x 40 px (bez danych obrazu): piksel 0,5 m, punkt dowiązania (600000; 500020), typ rastra.</summary>
    Private Shared Function NaglowekGeoTiff(typRastra As Short) As Byte()
        Dim ms As New MemoryStream()
        Dim bw As New BinaryWriter(ms)
        bw.Write(New Byte() {AscW("I"c), AscW("I"c)}) : bw.Write(CShort(42)) : bw.Write(8)
        Dim wpis = Sub(tag As Integer, typ As Short, ile As Integer, wartosc As Integer, krotka As Boolean)
                       bw.Write(CUShort(tag)) : bw.Write(typ) : bw.Write(ile)
                       If krotka Then bw.Write(CShort(wartosc)) : bw.Write(CShort(0)) Else bw.Write(wartosc)
                   End Sub
        bw.Write(CShort(5))
        wpis(256, 3, 1, 64, True)
        wpis(257, 3, 1, 40, True)
        wpis(33550, 12, 3, 74, False)
        wpis(33922, 12, 6, 98, False)
        wpis(34735, 3, 8, 146, False)
        bw.Write(0)
        For Each d In {0.5, 0.5, 0.0, 0.0, 0.0, 0.0, 600000.0, 500020.0, 0.0}
            bw.Write(d)
        Next
        For Each k In New Short() {1, 1, 0, 1, 1025, 0, 1, typRastra}
            bw.Write(k)
        Next
        Return ms.ToArray()
    End Function

    <Theory>
    <InlineData(1, 600000.0, 500020.0)>
    <InlineData(2, 599999.75, 500020.25)>
    Public Sub GeoTiffPixelIsPointPolPiksela(typRastra As Integer, yLewy As Double, xGora As Double)
        Using ms As New MemoryStream(NaglowekGeoTiff(CShort(typRastra)))
            Dim n = NaglowekObrazu.Odczytaj(ms)
            Assert.Equal(typRastra = 2, n.PikselJakoPunkt)
            Dim z = n.ZasiegGeoTiff()
            Assert.Equal(yLewy, z.YLewy, 9)
            Assert.Equal(xGora, z.XGora, 9)
            Assert.Equal(xGora - 20, z.XDol, 9)
            Assert.Equal(yLewy + 32, z.YPrawy, 9)
        End Using
    End Sub

    <Fact>
    Public Sub ArkuszGeoTiffWInnymUkladzie()
        Dim folder = NowyFolder()
        Try
            Dim plik = folder & "arkusz.tif"
            Dim zasieg As New Zasieg(500000, 600000, 500020, 600032)
            Using z As New ZapisGeoTiff(plik, 64, 40, False, 90, UkladWspolrzednych.PL1992, zasieg)
                z.ZapiszWiersze(New Byte(64 * 40 * 3 - 1) {}, 40)
                z.Zakoncz()
            End Using
            Assert.Equal(2180, NaglowekObrazu.Odczytaj(plik).Epsg)
            Dim uwagi As New List(Of String)
            EksportCad.Arkusz(plik, UkladWspolrzednych.PL1992, uwagi)
            Assert.DoesNotContain(uwagi, Function(u) u.StartsWith("Uwaga"))
            uwagi.Clear()
            EksportCad.Arkusz(plik, UkladWspolrzednych.PL2000Strefa7, uwagi)
            Assert.Contains(uwagi, Function(u) u.StartsWith("Uwaga") AndAlso u.Contains("EPSG:2180"))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    ''' <summary>
    ''' Wyrażenia najwyższego poziomu programu AutoLISP (sprawdzenie składni: komentarze ;, teksty "..." ze znakami \,
    ''' domknięte nawiasy). Wyjątek przy błędzie składni.
    ''' </summary>
    Private Shared Function WyrazeniaLisp(program As String) As List(Of String)
        Dim wynik As New List(Of String)
        Dim glebokosc As Integer = 0, poczatek As Integer = -1
        Dim i As Integer = 0
        While i < program.Length
            Dim c = program(i)
            If c = ";"c Then
                While i < program.Length AndAlso program(i) <> ChrW(10)
                    i += 1
                End While
                Continue While
            End If
            If c = """"c Then
                i += 1
                While program(i) <> """"c
                    If program(i) = "\"c Then i += 1
                    If program(i) = ChrW(10) Then Throw New Exception("Tekst przechodzi do następnego wiersza na pozycji " & i)
                    i += 1
                End While
            ElseIf c = "("c Then
                If glebokosc = 0 Then poczatek = i
                glebokosc += 1
            ElseIf c = ")"c Then
                glebokosc -= 1
                If glebokosc < 0 Then Throw New Exception("Nadmiarowy nawias na pozycji " & i)
                If glebokosc = 0 Then wynik.Add(program.Substring(poczatek, i - poczatek + 1))
            End If
            i += 1
        End While
        If glebokosc <> 0 Then Throw New Exception("Niedomknięte nawiasy: " & glebokosc)
        Return wynik
    End Function

    <Fact>
    Public Sub LispSkladniaIDane()
        Dim z = Zadanie()
        z.Opis = "Mapa: Łódź ""test"" \ x"
        z.ObszarZaznaczenia = New Zasieg(487000, 637000, 487400, 637500)
        z.Trasa.Add(New List(Of PunktXY) From {New PunktXY(487050, 637050), New PunktXY(487300, 637400)})
        z.Rastry(1).SciezkaPliku = "C:\mapy z ""cudzysłowem""\_01_02.png"
        z.Rastry(1).NazwaPliku = "_01_02.png"
        Dim program = EksportCad.Lisp(z)
        Dim wyr = WyrazeniaLisp(program)
        'dane, funkcje i na końcu uruchomienie wstawiania
        Assert.Equal("(c:MAPOTERO_WSTAW)", wyr.Last())
        Assert.Contains(wyr, Function(x) x.StartsWith("(defun c:MAPOTERO_WSTAW"))
        Assert.Contains(wyr, Function(x) x.StartsWith("(defun mt:blad"))
        Dim rastry = wyr.First(Function(x) x.StartsWith("(setq mt:rastry"))
        Assert.Contains("(""MT_test__01_01"" ""C:\\mapy\\sesja\\_01_01.jpg"" 637000.0 487256.0 256.0 nil)", rastry)
        Assert.Contains("""C:\\mapy z \""cudzys", rastry)
        Assert.Contains("256.0 T)", rastry)
        'siatka 2 x 2 z obrysem: 2 linie wewnętrzne; obrys i zaznaczenie; numery; trasa; opis z kodami \U+
        Assert.Equal(2, wyr.First(Function(x) x.StartsWith("(setq mt:siatka")).Split({"0.0) ("}, StringSplitOptions.None).Length - 1)
        Dim obrysy = wyr.First(Function(x) x.StartsWith("(setq mt:obrysy"))
        Assert.Contains("""MAPOTERO_ZASIEG"" 1 ((637000.0 487000.0) (637512.0 487000.0)", obrysy)
        Assert.Contains("""MAPOTERO_ZAZNACZENIE"" 3", obrysy)
        Assert.Contains("""_02_02""", wyr.First(Function(x) x.StartsWith("(setq mt:numery")))
        Assert.Contains("((637050.0 487050.0) (637400.0 487300.0))", wyr.First(Function(x) x.StartsWith("(setq mt:trasa")))
        Assert.Contains("""Mapa: \\U+0141\\U+00F3d\\U+017A \""test\"" \\ x""", wyr.First(Function(x) x.StartsWith("(setq mt:opis")))
        'cały program w ASCII, gdy ścieżki są w ASCII (kodowanie odczytu .lsp zależy od programu CAD)
        Dim z2 = Zadanie()
        z2.Opis = "Mapa: Łódź"
        Assert.True(EksportCad.Lisp(z2).All(Function(c) AscW(c) < 128))
        Assert.Empty(z2.Uwagi)
        'ścieżka z polską literą - uwaga o kodowaniu
        z2.Rastry(0).SciezkaPliku = "C:\Łódź\_01_01.jpg"
        EksportCad.Lisp(z2)
        Assert.Single(z2.Uwagi)
    End Sub

    <Fact>
    Public Sub LispBezDanychIKorytarz()
        Dim z = Zadanie()
        z.Rastry.Clear()
        z.RysujNumery = False
        z.SegmentyKorytarza = New HashSet(Of Integer) From {0, 3}
        Dim wyr = WyrazeniaLisp(EksportCad.Lisp(z))
        Assert.Equal("(setq mt:rastry '(" & vbCrLf & "))", wyr.First(Function(x) x.StartsWith("(setq mt:rastry")))
        Assert.Equal("(setq mt:numery '(" & vbCrLf & "))", wyr.First(Function(x) x.StartsWith("(setq mt:numery")))
        'korytarz: prostokąty segmentów zamiast linii
        Assert.Equal("(setq mt:siatka '(" & vbCrLf & "))", wyr.First(Function(x) x.StartsWith("(setq mt:siatka")))
        Assert.Equal(2, wyr.First(Function(x) x.StartsWith("(setq mt:prostokaty")).Split({vbCrLf}, StringSplitOptions.None).Length - 2)
    End Sub

    <Fact>
    Public Sub SkryptWgs84BezMetrow()
        Dim z As New ZadanieCad With {.Uklad = UkladWspolrzednych.Wgs84,
                                      .Siatka = New Siatka(New Zasieg(52, 21, 52.01, 21.01), 0.00001, 500)}
        Dim scr = EksportCad.Skrypt(z)
        Assert.DoesNotContain("_SETVAR INSUNITS 6", scr)
        Assert.DoesNotContain("_-IMAGE", scr)
        Dim dxf = EksportCad.Dxf(z, Encoding.ASCII)
        Dim p = Pary(dxf)
        Assert.Equal("0", p(p.FindIndex(Function(x) x.Value = "$INSUNITS") + 1).Value)
    End Sub

    <Fact>
    Public Sub WorldFileZasiegZgodnyZZapisem()
        Dim o As New ObrazGeoreferencyjny With {.Zasieg = New Zasieg(487000, 637000, 487512.5, 637256.25), .SzerokoscPx = 1025, .WysokoscPx = 2050}
        Dim z = WorldFile.Zasieg(Georeferencja.WorldFile(o), o.SzerokoscPx, o.WysokoscPx)
        Assert.Equal(487000, z.XDol, 6)
        Assert.Equal(637000, z.YLewy, 6)
        Assert.Equal(487512.5, z.XGora, 6)
        Assert.Equal(637256.25, z.YPrawy, 6)
        Assert.Throws(Of InvalidDataException)(Function() WorldFile.Zasieg("1" & vbCrLf & "0.1" & vbCrLf & "0" & vbCrLf & "-1" & vbCrLf & "5" & vbCrLf & "6", 10, 10))
    End Sub

    <Theory>
    <InlineData("png")>
    <InlineData("jpg")>
    <InlineData("gif")>
    Public Sub NaglowekWymiaryObrazu(rozszerzenie As String)
        Dim folder = NowyFolder()
        Try
            Dim plik = folder & "obraz." & rozszerzenie
            Using bmp As New Bitmap(301, 123)
                Dim format = If(rozszerzenie = "png", ImageFormat.Png, If(rozszerzenie = "gif", ImageFormat.Gif, ImageFormat.Jpeg))
                bmp.Save(plik, format)
            End Using
            Dim n = NaglowekObrazu.Odczytaj(plik)
            Assert.Equal(301L, n.SzerokoscPx)
            Assert.Equal(123L, n.WysokoscPx)
            Assert.False(n.MaGeoreferencje)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Theory>
    <InlineData(False)>
    <InlineData(True)>
    Public Sub NaglowekGeoTiff(kompresjaJpeg As Boolean)
        Dim folder = NowyFolder()
        Try
            Dim plik = folder & "arkusz.tif"
            Dim zasieg As New Zasieg(500000, 600000, 500000 + 40 * 0.5, 600000 + 64 * 0.5)
            Using z As New ZapisGeoTiff(plik, 64, 40, kompresjaJpeg, 90, UkladWspolrzednych.PL1992, zasieg)
                z.ZapiszWiersze(New Byte(64 * 40 * 3 - 1) {}, 40)
                z.Zakoncz()
            End Using
            Dim n = NaglowekObrazu.Odczytaj(plik)
            Assert.Equal(64L, n.SzerokoscPx)
            Assert.Equal(40L, n.WysokoscPx)
            Assert.False(n.BigTiff)
            Assert.True(n.MaGeoreferencje)
            Dim zg = n.ZasiegGeoTiff()
            Assert.Equal(zasieg.XDol, zg.XDol, 6)
            Assert.Equal(zasieg.YLewy, zg.YLewy, 6)
            Assert.Equal(zasieg.XGora, zg.XGora, 6)
            Assert.Equal(zasieg.YPrawy, zg.YPrawy, 6)

            'arkusz GeoTIFF bez world file - położenie z nagłówka
            Dim a = EksportCad.Arkusz(plik, UkladWspolrzednych.PL1992, New List(Of String))
            Assert.Equal(zasieg.YLewy, a.Zasieg.YLewy, 6)
            Assert.Equal(plik, a.SciezkaPliku)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ArkuszJpegZWorldFile()
        Dim folder = NowyFolder()
        Try
            Dim plik = folder & "arkusz.jpg"
            Using bmp As New Bitmap(200, 100)
                bmp.Save(plik, ImageFormat.Jpeg)
            End Using
            Dim zasieg As New Zasieg(487000, 637000, 487050, 637100)
            Dim o As New ObrazGeoreferencyjny With {.Zasieg = zasieg, .SzerokoscPx = 200, .WysokoscPx = 100}
            File.WriteAllText(folder & "arkusz.jpgw", Georeferencja.WorldFile(o))
            Dim a = EksportCad.Arkusz(plik, UkladWspolrzednych.PL1992, Nothing)
            Assert.Equal(200L, a.SzerokoscPx)
            Assert.Equal(637000, a.Zasieg.YLewy, 6)
            Assert.Equal(487050, a.Zasieg.XGora, 6)

            'bez georeferencji - wyjątek z opisem
            File.Delete(folder & "arkusz.jpgw")
            Assert.Throws(Of InvalidDataException)(Function() EksportCad.Arkusz(plik, UkladWspolrzednych.PL1992, Nothing))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ObrysZaznaczeniaMniejszegoOdSiatki()
        Dim folder = NowyFolder()
        Try
            'zaznaczenie 500 x 400 m przy segmencie 256 m - siatka 2 x 2 (512 x 512 m)
            Dim c As New KonfiguracjaSesji With {.XDol = "487000", .YLewy = "637000", .XGora = "487400", .YPrawy = "637500",
                                                 .BokSegmentuPx = "256", .RozmiarPiksela = "1", .Format = "image/jpeg", .Prefiks = "_", .UkladEpsg = 2180}
            Dim z = EksportCad.ZSesji(folder, c, StylNumeracji.WierszKolumna, False)
            Assert.True(z.ObszarZaznaczenia.HasValue)
            Assert.Equal(487400.0, z.ObszarZaznaczenia.Value.XGora)
            Assert.Equal(637500.0, z.ObszarZaznaczenia.Value.YPrawy)
            Dim ob = Obiekty(Pary(EksportCad.Dxf(z, Encoding.ASCII)))
            Dim zazn = ob.Single(Function(o) o.Key = "LWPOLYLINE" AndAlso Wartosc(o.Value, 8) = EksportCad.WarstwaZaznaczenia).Value
            Assert.Equal(637500.0, zazn.Where(Function(x) x.Key = 10).Max(Function(x) Liczba(x.Value)))
            Assert.Equal(487400.0, zazn.Where(Function(x) x.Key = 20).Max(Function(x) Liczba(x.Value)))
            Assert.Contains(EksportCad.WarstwaZaznaczenia, EksportCad.Skrypt(z))
            'bez obrysu - bez zaznaczenia
            z.RysujObrys = False
            Assert.DoesNotContain(EksportCad.WarstwaZaznaczenia, EksportCad.Skrypt(z))

            'zaznaczenie równe siatce - tylko obrys
            c.XGora = "487512" : c.YPrawy = "637512"
            Assert.False(EksportCad.ZSesji(folder, c, StylNumeracji.WierszKolumna, False).ObszarZaznaczenia.HasValue)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <Fact>
    Public Sub ZadanieZSesjiIstniejaceSegmenty()
        Dim folder = NowyFolder()
        Try
            Dim c As New KonfiguracjaSesji With {.Folder = folder, .RodzajMapy = "ortofotomapa", .XDol = "487000", .YLewy = "637000",
                                                 .XGora = "487512", .YPrawy = "637512", .BokSegmentuPx = "256", .RozmiarPiksela = "1",
                                                 .Format = "image/jpeg", .Prefiks = "_", .UkladEpsg = 2180}
            For Each n In {"_01_01.jpg", "_01_02.jpg", "_02_02.jpg"}
                Using bmp As New Bitmap(256, 256)
                    bmp.Save(folder & n, ImageFormat.Jpeg)
                End Using
            Next
            'segment o innym wymiarze niż w conf.txt - w rysunku rzeczywista liczba pikseli, zasięg segmentu bez zmian
            Using bmp As New Bitmap(128, 128)
                bmp.Save(folder & "_02_01.jpg", ImageFormat.Jpeg)
            End Using
            'plik spoza siatki i plik uszkodzony (np. komunikat błędu serwera zapisany jako obraz)
            File.WriteAllBytes(folder & "_09_09.jpg", {1})
            File.WriteAllText(folder & "_01_02.jpgw", "1")
            Dim z = EksportCad.ZSesji(folder, c, StylNumeracji.WierszKolumna, True)
            Assert.Equal(4, z.Rastry.Count)
            Assert.Equal(folder & "_02_02.jpg", z.Rastry(3).SciezkaPliku)
            Assert.Equal(637256.0, z.Rastry(3).Zasieg.YLewy)
            Assert.Equal(487000.0, z.Rastry(3).Zasieg.XDol)
            Assert.Equal(128L, z.Rastry(2).SzerokoscPx)
            Assert.Equal(2.0, z.Rastry(2).PikselY)
            Assert.Null(z.SegmentyKorytarza)
            Assert.Empty(z.Uwagi)
            Assert.Empty(EksportCad.ZSesji(folder, c, StylNumeracji.WierszKolumna, False).Rastry)

            File.WriteAllText(folder & "_01_01.jpg", "<ServiceExceptionReport>")
            z = EksportCad.ZSesji(folder, c, StylNumeracji.WierszKolumna, True)
            Assert.Equal(3, z.Rastry.Count)
            Assert.Single(z.Uwagi)
            Assert.Contains("_01_01.jpg", z.Uwagi(0))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

End Class
