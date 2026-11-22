Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
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
''' Opis prostokąta terenowego scalonego arkusza w metrach (lewy-górny narożnik i rozmiar piksela).
''' </summary>
Public NotInheritable Class ZakresArkusza
    Public Property SzerokoscPx As Integer
    Public Property WysokoscPx As Integer
    Public Property LewyEastingM As Double
    Public Property GornyNorthingM As Double
    Public Property RozmiarPikselaM As Double

    Public ReadOnly Property PrawyEastingM As Double
        Get
            Return LewyEastingM + (SzerokoscPx * RozmiarPikselaM)
        End Get
    End Property

    Public ReadOnly Property DolnyNorthingM As Double
        Get
            Return GornyNorthingM - (WysokoscPx * RozmiarPikselaM)
        End Get
    End Property

    Public Function MetryNaPiksel(eastingM As Double, northingM As Double) As PointF
        Dim px As Single = CSng((eastingM - LewyEastingM) / RozmiarPikselaM)
        Dim py As Single = CSng((GornyNorthingM - northingM) / RozmiarPikselaM)
        Return New PointF(px, py)
    End Function
End Class

''' <summary>
''' Moduł rysowania wektorowej siatki kilometrowej (PUWG-92 / UTM) oraz opcjonalnego śladu GPX/KML
''' wyłącznie na scalonych arkuszach do druku lub plikach GeoTIFF (.NET 10).
''' Wspiera przesunięcie pionowe (offsetPasaY) dla strumieniowego scalania pasami.
''' </summary>
Public NotInheritable Class RendererNakladekWydruku

    Public Shared Function DobierzKrokSiatki(arkusz As ZakresArkusza) As Integer
        Dim szerokoscM As Double = arkusz.SzerokoscPx * arkusz.RozmiarPikselaM
        Dim docelowy As Double = szerokoscM / 8.0
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
        Optional przeliczZUkSiatkiNaArkusz As Func(Of Double, Double, (E As Double, N As Double)) = Nothing)

        Dim krok As Integer = If(parametry.KrokMetry > 0, parametry.KrokMetry, DobierzKrokSiatki(arkusz))
        Dim stan As GraphicsState = g.Save()

        Try
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit

            If offsetPasaY <> 0 Then
                g.TranslateTransform(0.0F, -CSng(offsetPasaY))
            End If

            Dim minE As Double = Math.Floor(arkusz.LewyEastingM / krok) * krok
            Dim maxE As Double = Math.Ceiling(arkusz.PrawyEastingM / krok) * krok
            Dim minN As Double = Math.Floor(arkusz.DolnyNorthingM / krok) * krok
            Dim maxN As Double = Math.Ceiling(arkusz.GornyNorthingM / krok) * krok

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
                        RysujKrzywaSiatki(g, pioro, arkusz, eVal, minN, maxN, True, przeliczZUkSiatkiNaArkusz)
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
                        RysujKrzywaSiatki(g, pioro, arkusz, nVal, minE, maxE, False, przeliczZUkSiatkiNaArkusz)
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
        arkusz As ZakresArkusza,
        stala As Double,
        zakresOd As Double,
        zakresDo As Double,
        czyPionowa As Boolean,
        transformacja As Func(Of Double, Double, (E As Double, N As Double)))

        Const probki As Integer = 16
        Dim punkty(probki) As PointF
        For i As Integer = 0 To probki
            Dim t As Double = zakresOd + ((zakresDo - zakresOd) * i) / probki
            Dim m = If(czyPionowa, transformacja(stala, t), transformacja(t, stala))
            punkty(i) = arkusz.MetryNaPiksel(m.E, m.N)
        Next
        g.DrawLines(pioro, punkty)
    End Sub

    Public Shared Function FormatujEtykieteWspolrzednej(wartoscMetry As Double, krok As Integer, pelneMetry As Boolean) As String
        If pelneMetry Then
            Return String.Format(CultureInfo.InvariantCulture, "{0:N0}", Math.Round(wartoscMetry)).Replace(","c, " "c)
        End If
        If wartoscMetry < 1000.0 Then
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
            Throw New FileNotFoundException("Nie znaleziono pliku śladu GPX/KML.", sciezkaPliku)
        End If

        Dim wynik As New List(Of List(Of (Lon As Double, Lat As Double)))()
        Dim doc As XDocument = XDocument.Load(sciezkaPliku)
        Dim ext As String = Path.GetExtension(sciezkaPliku).ToLowerInvariant()

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
            Throw New NotSupportedException(String.Format("Nieobsługiwany format śladu: {0} (obsługiwane: .gpx, .kml).", ext))
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
                    Dim punkty(seg.Count - 1) As PointF
                    For i As Integer = 0 To seg.Count - 1
                        punkty(i) = przeliczWgs84NaPiksel(seg(i).Lon, seg(i).Lat)
                    Next

                    g.DrawLines(pioroObwiedni, punkty)
                    g.DrawLines(pioroWnetrza, punkty)

                    If styl.RysujZnacznikiStartKoniec Then
                        RysujZnacznikPunktu(g, punkty(0), Color.ForestGreen)
                        RysujZnacznikPunktu(g, punkty(punkty.Length - 1), Color.Crimson)
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
