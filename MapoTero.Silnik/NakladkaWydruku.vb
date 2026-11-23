Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Xml.Linq

''' <summary>
''' Typ układu siatki kilometrowej nanoszonej wyłącznie na scalone arkusze do druku / GeoTIFF.
''' </summary>
Public Enum UkladSiatkiWydruku
    Puwg1992 = 2180
    Utm33N = 32633
    Utm34N = 32634
    Utm35N = 32635
    Pl2000Strefa5 = 2176
    Pl2000Strefa6 = 2177
    Pl2000Strefa7 = 2178
    Pl2000Strefa8 = 2179
End Enum

''' <summary>
''' Konfiguracja rysowania siatki kilometrowej z opisem współrzędnych na marginesach arkusza.
''' </summary>
Public NotInheritable Class ParametrySiatkiKilometrowej
    Public Property UkladSiatki As UkladSiatkiWydruku = UkladSiatkiWydruku.Puwg1992
    Public Property KrokMetry As Integer = 1000
    Public Property KolorLinii As Color = Color.FromArgb(165, 25, 65, 130)
    Public Property GruboscLiniiPx As Single = 1.5F
    Public Property RozmiarCzcionkiPt As Single = 11.0F
    Public Property OpisujPelneMetry As Boolean = False
    Public Property MarginesEtykietPx As Integer = 10
End Class

''' <summary>
''' Konfiguracja stylu trasy GPX/KML nanoszonej na papierowy arkusz do druku.
''' </summary>
Public NotInheritable Class StylSladuWydruku
    Public Property KolorWypelnienia As Color = Color.FromArgb(225, 215, 25, 40)
    Public Property KolorObwiedni As Color = Color.FromArgb(235, 255, 255, 255)
    Public Property GruboscLiniiPx As Single = 4.0F
    Public Property RysujZnacznikiStartKoniec As Boolean = True
End Class

''' <summary>
''' Opis prostokąta terenowego scalonego arkusza w jednostkach arkusza (lewy-górny narożnik i rozmiar piksela w poziomie/pionie).
''' </summary>
Public NotInheritable Class ZakresArkusza
    Public Property SzerokoscPx As Integer
    Public Property WysokoscPx As Integer
    Public Property LewyEastingM As Double
    Public Property GornyNorthingM As Double
    Public Property RozmiarPikselaXM As Double
    Public Property RozmiarPikselaYM As Double

    Public Property RozmiarPikselaM As Double
        Get
            Return If(RozmiarPikselaXM > 0, RozmiarPikselaXM, RozmiarPikselaYM)
        End Get
        Set(value As Double)
            RozmiarPikselaXM = value
            RozmiarPikselaYM = value
        End Set
    End Property

    Public ReadOnly Property PrawyEastingM As Double
        Get
            Return LewyEastingM + (SzerokoscPx * RozmiarPikselaXM)
        End Get
    End Property

    Public ReadOnly Property DolnyNorthingM As Double
        Get
            Return GornyNorthingM - (WysokoscPx * RozmiarPikselaYM)
        End Get
    End Property

    Public Function MetryNaPiksel(eastingM As Double, northingM As Double) As PointF
        Dim rx As Double = If(RozmiarPikselaXM > 0, RozmiarPikselaXM, 1.0)
        Dim ry As Double = If(RozmiarPikselaYM > 0, RozmiarPikselaYM, rx)
        Dim px As Single = CSng((eastingM - LewyEastingM) / rx)
        Dim py As Single = CSng((GornyNorthingM - northingM) / ry)
        Return New PointF(px, py)
    End Function
End Class

''' <summary>
''' Moduł rysowania wektorowej siatki kilometrowej (PUWG-92 / UTM) oraz opcjonalnego śladu GPX/KML
''' wyłącznie na scalonych arkuszach do druku lub plikach GeoTIFF (.NET 10).
''' Wspiera przesunięcie pionowe (offsetPasaY) dla strumieniowego scalania pasami.
''' </summary>
Public NotInheritable Class RendererNakladekWydruku

    Public Shared Function DobierzKrokSiatki(arkusz As ZakresArkusza, Optional szerokoscM As Double = 0) As Integer
        Dim szerokosc As Double = If(szerokoscM > 0, szerokoscM, arkusz.SzerokoscPx * arkusz.RozmiarPikselaM)
        Dim docelowy As Double = szerokosc / 8.0
        Dim kroki As Integer() = {100, 250, 500, 1000, 2000, 5000, 10000}

        For Each k As Integer In kroki
            If k >= docelowy Then Return k
        Next
        Return 10000
    End Function

    Public Shared Sub RysujSiatkeKilometrowa(
        g As Graphics,
        arkusz As ZakresArkusza,
        parametry As ParametrySiatkiKilometrowej,
        Optional offsetPasaY As Integer = 0,
        Optional przeliczZUkSiatkiNaArkusz As Func(Of Double, Double, (E As Double, N As Double)) = Nothing,
        Optional zakresMetrowySiatki As Tuple(Of Double, Double, Double, Double) = Nothing)

        Dim szerokoscDoKroku As Double = If(zakresMetrowySiatki IsNot Nothing, Math.Abs(zakresMetrowySiatki.Item2 - zakresMetrowySiatki.Item1), 0)
        Dim krok As Integer = If(parametry.KrokMetry > 0, parametry.KrokMetry, DobierzKrokSiatki(arkusz, szerokoscDoKroku))
        Dim stan As GraphicsState = g.Save()

        Try
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit

            If offsetPasaY <> 0 Then
                g.TranslateTransform(0.0F, -CSng(offsetPasaY))
            End If

            Dim minE As Double
            Dim maxE As Double
            Dim minN As Double
            Dim maxN As Double

            If zakresMetrowySiatki IsNot Nothing Then
                minE = Math.Floor(zakresMetrowySiatki.Item1 / krok) * krok
                maxE = Math.Ceiling(zakresMetrowySiatki.Item2 / krok) * krok
                minN = Math.Floor(zakresMetrowySiatki.Item3 / krok) * krok
                maxN = Math.Ceiling(zakresMetrowySiatki.Item4 / krok) * krok
            Else
                minE = Math.Floor(arkusz.LewyEastingM / krok) * krok
                maxE = Math.Ceiling(arkusz.PrawyEastingM / krok) * krok
                minN = Math.Floor(arkusz.DolnyNorthingM / krok) * krok
                maxN = Math.Ceiling(arkusz.GornyNorthingM / krok) * krok
            End If

            Using pioro As New Pen(parametry.KolorLinii, parametry.GruboscLiniiPx),
                  czcionka As New Font("Segoe UI", parametry.RozmiarCzcionkiPt, FontStyle.Bold)

                Dim eVal As Double = minE
                While eVal <= maxE
                    If przeliczZUkSiatkiNaArkusz Is Nothing Then
                        Dim px As Single = arkusz.MetryNaPiksel(eVal, arkusz.GornyNorthingM).X
                        If px >= 0 AndAlso px <= arkusz.SzerokoscPx Then
                            g.DrawLine(pioro, px, 0.0F, px, CSng(arkusz.WysokoscPx))
                            Dim opis As String = FormatujEtykieteWspolrzednej(eVal, krok, parametry.OpisujPelneMetry)
                            RysujEtykieteZObwiednia(g, opis, czcionka, px + 4.0F, parametry.MarginesEtykietPx)
                            RysujEtykieteZObwiednia(g, opis, czcionka, px + 4.0F, CSng(arkusz.WysokoscPx - parametry.MarginesEtykietPx - czcionka.Height))
                        End If
                    Else
                        RysujKrzywaSiatki(g, pioro, czcionka, arkusz, eVal, minN, maxN, True, przeliczZUkSiatkiNaArkusz, krok, parametry.OpisujPelneMetry, parametry.MarginesEtykietPx)
                    End If
                    eVal += krok
                End While

                Dim nVal As Double = minN
                While nVal <= maxN
                    If przeliczZUkSiatkiNaArkusz Is Nothing Then
                        Dim py As Single = arkusz.MetryNaPiksel(arkusz.LewyEastingM, nVal).Y
                        If py >= 0 AndAlso py <= arkusz.WysokoscPx Then
                            g.DrawLine(pioro, 0.0F, py, CSng(arkusz.SzerokoscPx), py)
                            Dim opis As String = FormatujEtykieteWspolrzednej(nVal, krok, parametry.OpisujPelneMetry)
                            Dim szerTekstu As Single = g.MeasureString(opis, czcionka).Width
                            RysujEtykieteZObwiednia(g, opis, czcionka, parametry.MarginesEtykietPx, py - czcionka.Height - 2.0F)
                            RysujEtykieteZObwiednia(g, opis, czcionka, arkusz.SzerokoscPx - szerTekstu - parametry.MarginesEtykietPx, py - czcionka.Height - 2.0F)
                        End If
                    Else
                        RysujKrzywaSiatki(g, pioro, czcionka, arkusz, nVal, minE, maxE, False, przeliczZUkSiatkiNaArkusz, krok, parametry.OpisujPelneMetry, parametry.MarginesEtykietPx)
                    End If
                    nVal += krok
                End While
            End Using
        Finally
            g.Restore(stan)
        End Try
    End Sub

    Private Shared Sub RysujKrzywaSiatki(
        g As Graphics,
        pioro As Pen,
        czcionka As Font,
        arkusz As ZakresArkusza,
        stala As Double,
        zakresOd As Double,
        zakresDo As Double,
        czyPionowa As Boolean,
        transformacja As Func(Of Double, Double, (E As Double, N As Double)),
        krok As Integer,
        pelneMetry As Boolean,
        marginesEtykietPx As Integer)

        Const probki As Integer = 32
        Dim punkty As New List(Of PointF)()
        Dim widocznyPrzynajmniejJeden As Boolean = False

        For i As Integer = 0 To probki
            Dim t As Double = zakresOd + ((zakresDo - zakresOd) * i) / probki
            Dim m = If(czyPionowa, transformacja(stala, t), transformacja(t, stala))
            Dim pt = arkusz.MetryNaPiksel(m.E, m.N)
            punkty.Add(pt)
            If pt.X >= -50.0F AndAlso pt.X <= arkusz.SzerokoscPx + 50.0F AndAlso
               pt.Y >= -50.0F AndAlso pt.Y <= arkusz.WysokoscPx + 50.0F Then
                widocznyPrzynajmniejJeden = True
            End If
        Next

        If widocznyPrzynajmniejJeden AndAlso punkty.Count >= 2 Then
            g.DrawLines(pioro, punkty.ToArray())

            ' Etykiety na brzegach dla linii widocznych
            Dim opis As String = FormatujEtykieteWspolrzednej(stala, krok, pelneMetry)
            Dim ptPierwszy As PointF? = Nothing
            Dim ptOstatni As PointF? = Nothing
            For Each p In punkty
                If p.X >= 0 AndAlso p.X <= arkusz.SzerokoscPx AndAlso p.Y >= 0 AndAlso p.Y <= arkusz.WysokoscPx Then
                    If Not ptPierwszy.HasValue Then ptPierwszy = p
                    ptOstatni = p
                End If
            Next

            If ptPierwszy.HasValue Then
                Dim ex As Single = Math.Max(CSng(marginesEtykietPx), Math.Min(ptPierwszy.Value.X + 4.0F, CSng(arkusz.SzerokoscPx - 70)))
                Dim ey As Single = Math.Max(CSng(marginesEtykietPx), Math.Min(ptPierwszy.Value.Y - czcionka.Height - 2.0F, CSng(arkusz.WysokoscPx - 25)))
                RysujEtykieteZObwiednia(g, opis, czcionka, ex, ey)
            End If
            If ptOstatni.HasValue AndAlso ptPierwszy.HasValue AndAlso (Math.Abs(ptOstatni.Value.X - ptPierwszy.Value.X) > 100.0F OrElse Math.Abs(ptOstatni.Value.Y - ptPierwszy.Value.Y) > 100.0F) Then
                Dim ex As Single = Math.Max(CSng(marginesEtykietPx), Math.Min(ptOstatni.Value.X + 4.0F, CSng(arkusz.SzerokoscPx - 70)))
                Dim ey As Single = Math.Max(CSng(marginesEtykietPx), Math.Min(ptOstatni.Value.Y - czcionka.Height - 2.0F, CSng(arkusz.WysokoscPx - 25)))
                RysujEtykieteZObwiednia(g, opis, czcionka, ex, ey)
            End If
        End If
    End Sub

    Public Shared Function FormatujEtykieteWspolrzednej(wartoscMetry As Double, krok As Integer, pelneMetry As Boolean) As String
        If pelneMetry Then
            Return String.Format(CultureInfo.InvariantCulture, "{0:N0}", Math.Round(wartoscMetry)).Replace(","c, " "c)
        End If
        If Math.Abs(wartoscMetry) < 1000.0 Then
            Return String.Format(CultureInfo.InvariantCulture, "{0:0} m", Math.Round(wartoscMetry))
        End If
        Dim valKm As Double = wartoscMetry / 1000.0
        If (krok Mod 1000 <> 0) Then
            Return String.Format(CultureInfo.InvariantCulture, "{0:0.#}", valKm)
        End If
        Return String.Format(CultureInfo.InvariantCulture, "{0}", CLng(Math.Round(valKm)))
    End Function

    Public Shared Function WczytajPunktyGpx(sciezkaPliku As String) As List(Of (Lon As Double, Lat As Double))
        Dim segs = WczytajSegmentySladu(sciezkaPliku)
        Return If(segs.Count > 0, segs(0), New List(Of (Lon As Double, Lat As Double))())
    End Function

    Public Shared Function WczytajPunktyKml(sciezkaPliku As String) As List(Of (Lon As Double, Lat As Double))
        Dim segs = WczytajSegmentySladu(sciezkaPliku)
        Return If(segs.Count > 0, segs(0), New List(Of (Lon As Double, Lat As Double))())
    End Function

    Private Shared Sub RysujEtykieteZObwiednia(g As Graphics, tekst As String, czcionka As Font, x As Single, y As Single)
        Using sciezka As New GraphicsPath(),
              pioroTla As New Pen(Color.FromArgb(230, 255, 255, 255), 3.2F) With {.LineJoin = LineJoin.Round},
              pedzelTekstu As New SolidBrush(Color.FromArgb(235, 18, 45, 95))

            Dim rozmiarEm As Single = g.DpiY * czcionka.SizeInPoints / 72.0F
            sciezka.AddString(tekst, czcionka.FontFamily, CInt(czcionka.Style), rozmiarEm, New PointF(x, y), StringFormat.GenericDefault)
            g.DrawPath(pioroTla, sciezka)
            g.FillPath(pedzelTekstu, sciezka)
        End Using
    End Sub

    Public Shared Function WczytajSegmentySladu(sciezkaPliku As String) As List(Of List(Of (Lon As Double, Lat As Double)))
        If Not File.Exists(sciezkaPliku) Then
            Throw New FileNotFoundException("Nie znaleziono pliku śladu GPX/KML/KMZ.", sciezkaPliku)
        End If

        Dim wynik As New List(Of List(Of (Lon As Double, Lat As Double)))()
        Dim ext As String = Path.GetExtension(sciezkaPliku).ToLowerInvariant()
        Dim doc As XDocument
        If ext = ".kmz" Then
            Using arch = ZipFile.OpenRead(sciezkaPliku)
                Dim wpis = arch.Entries.FirstOrDefault(Function(e) e.FullName.EndsWith(".kml", StringComparison.OrdinalIgnoreCase))
                If wpis Is Nothing Then Throw New InvalidDataException("Archiwum KMZ nie zawiera pliku .kml ze śladem.")
                Using s = wpis.Open()
                    doc = XDocument.Load(s)
                End Using
            End Using
            ext = ".kml"
        Else
            doc = XDocument.Load(sciezkaPliku)
        End If

        If ext = ".gpx" Then
            For Each seg As XElement In doc.Descendants().Where(Function(e) e.Name.LocalName = "trkseg" OrElse e.Name.LocalName = "rte")
                Dim lista As New List(Of (Lon As Double, Lat As Double))()
                For Each pt As XElement In seg.Elements().Where(Function(e) e.Name.LocalName = "trkpt" OrElse e.Name.LocalName = "rtept")
                    Dim aLat As XAttribute = pt.Attribute("lat")
                    Dim aLon As XAttribute = pt.Attribute("lon")
                    If aLat Is Nothing OrElse aLon Is Nothing Then Continue For

                    Dim lat As Double
                    Dim lon As Double
                    If Double.TryParse(aLat.Value, NumberStyles.Float, CultureInfo.InvariantCulture, lat) AndAlso
                       Double.TryParse(aLon.Value, NumberStyles.Float, CultureInfo.InvariantCulture, lon) Then
                        lista.Add((lon, lat))
                    End If
                Next
                If lista.Count >= 2 Then wynik.Add(lista)
            Next
        ElseIf ext = ".kml" Then
            For Each ls As XElement In doc.Descendants().Where(Function(e) e.Name.LocalName = "LineString")
                Dim coords As XElement = ls.Elements().FirstOrDefault(Function(e) e.Name.LocalName = "coordinates")
                If coords Is Nothing Then Continue For

                Dim lista As New List(Of (Lon As Double, Lat As Double))()
                Dim tokeny As String() = coords.Value.Split(New Char() {" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf},
                                                            StringSplitOptions.RemoveEmptyEntries)
                For Each t As String In tokeny
                    Dim cz As String() = t.Split(","c)
                    If cz.Length < 2 Then Continue For
                    Dim lon As Double
                    Dim lat As Double
                    If Double.TryParse(cz(0), NumberStyles.Float, CultureInfo.InvariantCulture, lon) AndAlso
                       Double.TryParse(cz(1), NumberStyles.Float, CultureInfo.InvariantCulture, lat) Then
                        lista.Add((lon, lat))
                    End If
                Next
                If lista.Count >= 2 Then wynik.Add(lista)
            Next
        Else
            Throw New NotSupportedException(String.Format("Nieobsługiwany format śladu: {0} (obsługiwane: .gpx, .kml, .kmz).", Path.GetExtension(sciezkaPliku).ToLowerInvariant()))
        End If

        Return wynik
    End Function

    Public Shared Sub RysujSladNaArkuszu(
        g As Graphics,
        segmentyWgs84 As IEnumerable(Of List(Of (Lon As Double, Lat As Double))),
        przeliczWgs84NaPiksel As Func(Of Double, Double, PointF),
        styl As StylSladuWydruku,
        Optional offsetPasaY As Integer = 0)

        Dim stan As GraphicsState = g.Save()
        Try
            g.SmoothingMode = SmoothingMode.AntiAlias
            If offsetPasaY <> 0 Then
                g.TranslateTransform(0.0F, -CSng(offsetPasaY))
            End If

            Using pioroObwiedni As New Pen(styl.KolorObwiedni, styl.GruboscLiniiPx + 3.0F) With {
                      .LineJoin = LineJoin.Round,
                      .StartCap = LineCap.Round,
                      .EndCap = LineCap.Round
                  },
                  pioroWnetrza As New Pen(styl.KolorWypelnienia, styl.GruboscLiniiPx) With {
                      .LineJoin = LineJoin.Round,
                      .StartCap = LineCap.Round,
                      .EndCap = LineCap.Round
                  }

                For Each seg As List(Of (Lon As Double, Lat As Double)) In segmentyWgs84
                    If seg.Count < 2 Then Continue For
                    Dim bezpieczny = Function(p As PointF) As Boolean
                                         Return Single.IsFinite(p.X) AndAlso Single.IsFinite(p.Y) AndAlso
                                                Math.Abs(p.X) <= 100000.0F AndAlso Math.Abs(p.Y - offsetPasaY) <= 100000.0F
                                     End Function
                    Dim pierwszyPunkt As PointF? = Nothing
                    Dim ostatniPunkt As PointF? = Nothing
                    Dim podsegment As New List(Of PointF)(seg.Count)

                    For i As Integer = 0 To seg.Count - 1
                        Dim pt = przeliczWgs84NaPiksel(seg(i).Lon, seg(i).Lat)
                        If i = 0 Then pierwszyPunkt = pt
                        If i = seg.Count - 1 Then ostatniPunkt = pt
                        If bezpieczny(pt) Then
                            podsegment.Add(pt)
                        ElseIf podsegment.Count >= 2 Then
                            Dim arr = podsegment.ToArray()
                            g.DrawLines(pioroObwiedni, arr)
                            g.DrawLines(pioroWnetrza, arr)
                            podsegment.Clear()
                        Else
                            podsegment.Clear()
                        End If
                    Next

                    If podsegment.Count >= 2 Then
                        Dim arr = podsegment.ToArray()
                        g.DrawLines(pioroObwiedni, arr)
                        g.DrawLines(pioroWnetrza, arr)
                    End If

                    If styl.RysujZnacznikiStartKoniec Then
                        If pierwszyPunkt.HasValue AndAlso bezpieczny(pierwszyPunkt.Value) Then RysujZnacznikPunktu(g, pierwszyPunkt.Value, Color.ForestGreen)
                        If ostatniPunkt.HasValue AndAlso bezpieczny(ostatniPunkt.Value) Then RysujZnacznikPunktu(g, ostatniPunkt.Value, Color.Crimson)
                    End If
                Next
            End Using
        Finally
            g.Restore(stan)
        End Try
    End Sub

    Private Shared Sub RysujZnacznikPunktu(g As Graphics, p As PointF, kolor As Color)
        Const promien As Single = 6.0F
        Using pedzel As New SolidBrush(kolor),
              pioro As New Pen(Color.White, 2.0F)
            g.FillEllipse(pedzel, p.X - promien, p.Y - promien, promien * 2.0F, promien * 2.0F)
            g.DrawEllipse(pioro, p.X - promien, p.Y - promien, promien * 2.0F, promien * 2.0F)
        End Using
    End Sub
End Class
