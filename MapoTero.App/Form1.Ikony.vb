Imports System.Drawing.Drawing2D

''' <summary>
''' Ikony okna głównego rysowane kreską w kolorze akcji, żeby wyglądały jednakowo w kartach belki, nagłówkach
''' prawego panelu i zakładkach oraz nie rozmywały się przy powiększonym ekranie (rysunek w rozmiarze docelowym).
''' Ta sama rzecz ma wszędzie tę samą ikonę: przerywany prostokąt to obszar pobierania (także na przycisku „Zaznacz obszar”).
''' </summary>
Partial Public Class Form1

    Friend Enum IkonaOkna
        Obszar
        Segmenty
        Katalog
        Warstwy
        Podglad
        Galeria
        Komunikaty
        Sesja
        Kosz
        Odswiez
        ''' <summary>Folder pobrań ze wszystkimi sesjami - inna ikona niż folder bieżącej sesji (audyt ER-14).</summary>
        Pobrane
    End Enum

    ''' <summary>Stan sesji pokazywany ikoną karty „Stan sesji” i kropką na pasku stanu.</summary>
    Friend Enum StanIkonySesji
        Nieaktywna
        Gotowa
        Pobieranie
        Pobrana
        Bledy
    End Enum

    Private Shared ReadOnly KolorSukcesu As Color = Color.FromArgb(46, 125, 50)
    Private Shared ReadOnly KolorPostepu As Color = Color.FromArgb(214, 120, 0)

    Friend Shared Sub RysujIkone(g As Graphics, ikona As IkonaOkna, r As RectangleF, kolor As Color)
        Dim tryb = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim grubosc = Math.Max(1.2F, r.Width / 13.0F)
        Using pioro As New Pen(kolor, grubosc) With {.LineJoin = LineJoin.Round}, pedzel As New SolidBrush(kolor)
            Dim x = r.X, y = r.Y, s = r.Width
            Select Case ikona
                Case IkonaOkna.Obszar
                    Using przerywane As New Pen(kolor, grubosc) With {.DashStyle = DashStyle.Dash}
                        g.DrawRectangle(przerywane, x + s * 0.14F, y + s * 0.2F, s * 0.72F, s * 0.6F)
                    End Using
                    Dim b = s * 0.2F
                    For Each p In {New PointF(x + s * 0.14F, y + s * 0.2F), New PointF(x + s * 0.86F, y + s * 0.2F),
                                   New PointF(x + s * 0.14F, y + s * 0.8F), New PointF(x + s * 0.86F, y + s * 0.8F)}
                        g.FillRectangle(pedzel, p.X - b / 2, p.Y - b / 2, b, b)
                    Next
                Case IkonaOkna.Segmenty
                    Dim bok = s * 0.38F, odstep = s * 0.1F, x0 = x + (s - 2 * bok - odstep) / 2, y0 = y + (s - 2 * bok - odstep) / 2
                    For Each k In {0, 1}
                        For Each w In {0, 1}
                            g.DrawRectangle(pioro, x0 + k * (bok + odstep), y0 + w * (bok + odstep), bok, bok)
                        Next
                    Next
                Case IkonaOkna.Katalog
                    'lista warstw w drzewie: punkty i linie
                    For Each w In {0.22F, 0.5F, 0.78F}
                        g.FillEllipse(pedzel, x + s * 0.08F, y + s * w - s * 0.08F, s * 0.16F, s * 0.16F)
                        g.DrawLine(pioro, x + s * 0.36F, y + s * w, x + s * 0.94F, y + s * w)
                    Next
                Case IkonaOkna.Warstwy
                    'trzy arkusze mapy jeden nad drugim
                    For Each przesuniecie In {0.56F, 0.36F, 0.16F}
                        Dim yy = y + s * przesuniecie
                        Dim pkt = {New PointF(x + s * 0.5F, yy), New PointF(x + s * 0.96F, yy + s * 0.15F),
                                   New PointF(x + s * 0.5F, yy + s * 0.3F), New PointF(x + s * 0.04F, yy + s * 0.15F)}
                        Using tlo As New SolidBrush(Color.White)
                            g.FillPolygon(tlo, pkt)
                        End Using
                        g.DrawPolygon(pioro, pkt)
                    Next
                Case IkonaOkna.Podglad
                    g.DrawRectangle(pioro, x + s * 0.06F, y + s * 0.16F, s * 0.88F, s * 0.68F)
                    g.DrawLines(pioro, {New PointF(x + s * 0.14F, y + s * 0.76F), New PointF(x + s * 0.4F, y + s * 0.44F),
                                        New PointF(x + s * 0.58F, y + s * 0.62F), New PointF(x + s * 0.7F, y + s * 0.5F),
                                        New PointF(x + s * 0.88F, y + s * 0.76F)})
                    g.FillEllipse(pedzel, x + s * 0.64F, y + s * 0.26F, s * 0.13F, s * 0.13F)
                Case IkonaOkna.Galeria
                    'stos kart podglądu: tylna karta lekko wysunięta w prawo-górę, przednia z krajobrazem
                    Dim rNaroznika = s * 0.09F
                    Using tylna = SciezkaZaokraglonegoProstokata(x + s * 0.26F, y + s * 0.09F, s * 0.65F, s * 0.65F, rNaroznika),
                          pioroTylne As New Pen(Color.FromArgb(165, kolor), grubosc) With {.LineJoin = LineJoin.Round},
                          przednia = SciezkaZaokraglonegoProstokata(x + s * 0.09F, y + s * 0.26F, s * 0.65F, s * 0.65F, rNaroznika),
                          tloKarty As New SolidBrush(Color.FromArgb(238, 244, 250))
                        g.DrawPath(pioroTylne, tylna)
                        g.FillPath(tloKarty, przednia)
                        g.DrawPath(pioro, przednia)
                    End Using
                    Using pioroKrajobraz As New Pen(kolor, grubosc) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round, .LineJoin = LineJoin.Round}
                        g.DrawLines(pioroKrajobraz, {New PointF(x + s * 0.17F, y + s * 0.79F), New PointF(x + s * 0.34F, y + s * 0.56F),
                                                    New PointF(x + s * 0.48F, y + s * 0.7F), New PointF(x + s * 0.56F, y + s * 0.61F),
                                                    New PointF(x + s * 0.67F, y + s * 0.79F)})
                    End Using
                    g.FillEllipse(pedzel, x + s * 0.49F, y + s * 0.35F, s * 0.13F, s * 0.13F)
                Case IkonaOkna.Komunikaty
                    Using sciezka As New GraphicsPath()
                        sciezka.AddArc(x + s * 0.06F, y + s * 0.12F, s * 0.25F, s * 0.25F, 180, 90)
                        sciezka.AddArc(x + s * 0.69F, y + s * 0.12F, s * 0.25F, s * 0.25F, 270, 90)
                        sciezka.AddArc(x + s * 0.69F, y + s * 0.47F, s * 0.25F, s * 0.25F, 0, 90)
                        sciezka.AddLine(x + s * 0.4F, y + s * 0.72F, x + s * 0.2F, y + s * 0.9F)
                        sciezka.AddLine(x + s * 0.22F, y + s * 0.72F, x + s * 0.18F, y + s * 0.72F)
                        sciezka.AddArc(x + s * 0.06F, y + s * 0.47F, s * 0.25F, s * 0.25F, 90, 90)
                        sciezka.CloseFigure()
                        g.DrawPath(pioro, sciezka)
                    End Using
                Case IkonaOkna.Sesja
                    'folder sesji
                    g.DrawLines(pioro, {New PointF(x + s * 0.06F, y + s * 0.82F), New PointF(x + s * 0.06F, y + s * 0.18F),
                                        New PointF(x + s * 0.38F, y + s * 0.18F), New PointF(x + s * 0.48F, y + s * 0.3F),
                                        New PointF(x + s * 0.94F, y + s * 0.3F), New PointF(x + s * 0.94F, y + s * 0.82F),
                                        New PointF(x + s * 0.06F, y + s * 0.82F)})
                Case IkonaOkna.Kosz
                    'kosz na śmieci
                    g.DrawLines(pioro, {New PointF(x + s * 0.36F, y + s * 0.22F), New PointF(x + s * 0.36F, y + s * 0.1F),
                                        New PointF(x + s * 0.64F, y + s * 0.1F), New PointF(x + s * 0.64F, y + s * 0.22F)})
                    g.DrawLine(pioro, x + s * 0.14F, y + s * 0.22F, x + s * 0.86F, y + s * 0.22F)
                    g.DrawLines(pioro, {New PointF(x + s * 0.22F, y + s * 0.22F), New PointF(x + s * 0.28F, y + s * 0.88F),
                                        New PointF(x + s * 0.72F, y + s * 0.88F), New PointF(x + s * 0.78F, y + s * 0.22F)})
                    g.DrawLine(pioro, x + s * 0.42F, y + s * 0.37F, x + s * 0.44F, y + s * 0.73F)
                    g.DrawLine(pioro, x + s * 0.58F, y + s * 0.37F, x + s * 0.56F, y + s * 0.73F)
                Case IkonaOkna.Odswiez
                    'okrągła strzałka odświeżania
                    Dim ramka As New RectangleF(x + s * 0.14F, y + s * 0.14F, s * 0.72F, s * 0.72F)
                    g.DrawArc(pioro, ramka, 30.0F, 290.0F)
                    g.DrawLines(pioro, {New PointF(x + s * 0.57F, y + s * 0.3F), New PointF(x + s * 0.78F, y + s * 0.27F),
                                        New PointF(x + s * 0.79F, y + s * 0.08F)})
                Case IkonaOkna.Pobrane
                    'dwa foldery jeden za drugim i strzałka w dół (pobrane mapy)
                    g.DrawLines(pioro, {New PointF(x + s * 0.18F, y + s * 0.2F), New PointF(x + s * 0.42F, y + s * 0.2F),
                                        New PointF(x + s * 0.5F, y + s * 0.3F), New PointF(x + s * 0.94F, y + s * 0.3F),
                                        New PointF(x + s * 0.94F, y + s * 0.72F)})
                    g.DrawLines(pioro, {New PointF(x + s * 0.06F, y + s * 0.92F), New PointF(x + s * 0.06F, y + s * 0.36F),
                                        New PointF(x + s * 0.32F, y + s * 0.36F), New PointF(x + s * 0.4F, y + s * 0.46F),
                                        New PointF(x + s * 0.82F, y + s * 0.46F), New PointF(x + s * 0.82F, y + s * 0.92F),
                                        New PointF(x + s * 0.06F, y + s * 0.92F)})
                    g.DrawLine(pioro, x + s * 0.44F, y + s * 0.56F, x + s * 0.44F, y + s * 0.84F)
                    g.DrawLines(pioro, {New PointF(x + s * 0.34F, y + s * 0.74F), New PointF(x + s * 0.44F, y + s * 0.84F),
                                        New PointF(x + s * 0.54F, y + s * 0.74F)})
            End Select
        End Using
        g.SmoothingMode = tryb
    End Sub

    Friend Shared Function UtworzIkoneBitmapy(ikona As IkonaOkna, rozmiar As Integer, kolor As Color) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            RysujIkone(g, ikona, New RectangleF(0, 0, rozmiar, rozmiar), kolor)
        End Using
        Return bmp
    End Function

    Private Shared Function SciezkaZaokraglonegoProstokata(x As Single, y As Single, szerokosc As Single, wysokosc As Single, promien As Single) As GraphicsPath
        Dim sciezka As New GraphicsPath()
        Dim srednica = Math.Min(promien * 2.0F, Math.Min(szerokosc, wysokosc))
        sciezka.AddArc(x, y, srednica, srednica, 180, 90)
        sciezka.AddArc(x + szerokosc - srednica, y, srednica, srednica, 270, 90)
        sciezka.AddArc(x + szerokosc - srednica, y + wysokosc - srednica, srednica, srednica, 0, 90)
        sciezka.AddArc(x, y + wysokosc - srednica, srednica, srednica, 90, 90)
        sciezka.CloseFigure()
        Return sciezka
    End Function

    ''' <summary>Ikona stanu sesji: strzałka pobierania, łuk postępu, znak ✓ albo wykrzyknik.</summary>
    Friend Shared Sub RysujIkoneStanu(g As Graphics, stan As StanIkonySesji, r As RectangleF, Optional postep As Double = 0)
        Dim tryb = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim s = r.Width, x = r.X, y = r.Y
        Dim grubosc = Math.Max(1.4F, s / 11.0F)
        Select Case stan
            Case StanIkonySesji.Nieaktywna, StanIkonySesji.Gotowa
                Dim kolor = If(stan = StanIkonySesji.Gotowa, KolorAkcjiOkna, Color.FromArgb(150, 165, 182))
                Using pioro As New Pen(kolor, grubosc) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round, .LineJoin = LineJoin.Round}
                    g.DrawLine(pioro, x + s / 2, y + s * 0.1F, x + s / 2, y + s * 0.66F)
                    g.DrawLines(pioro, {New PointF(x + s * 0.24F, y + s * 0.42F), New PointF(x + s / 2, y + s * 0.68F),
                                        New PointF(x + s * 0.76F, y + s * 0.42F)})
                    g.DrawLine(pioro, x + s * 0.12F, y + s * 0.88F, x + s * 0.88F, y + s * 0.88F)
                End Using
            Case StanIkonySesji.Pobieranie
                Using tlo As New Pen(Color.FromArgb(232, 236, 241), grubosc), pioro As New Pen(KolorPostepu, grubosc)
                    Dim o As New RectangleF(x + grubosc, y + grubosc, s - 2 * grubosc, s - 2 * grubosc)
                    g.DrawEllipse(tlo, o)
                    g.DrawArc(pioro, o, -90, CSng(Math.Max(12, 360 * Math.Clamp(postep, 0, 1))))
                End Using
            Case StanIkonySesji.Pobrana, StanIkonySesji.Bledy
                Dim kolor = If(stan = StanIkonySesji.Pobrana, KolorSukcesu, Color.Firebrick)
                Using pedzel As New SolidBrush(kolor), pioro As New Pen(Color.White, grubosc) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round}
                    g.FillEllipse(pedzel, x + s * 0.04F, y + s * 0.04F, s * 0.92F, s * 0.92F)
                    If stan = StanIkonySesji.Pobrana Then
                        g.DrawLines(pioro, {New PointF(x + s * 0.28F, y + s * 0.52F), New PointF(x + s * 0.44F, y + s * 0.68F),
                                            New PointF(x + s * 0.74F, y + s * 0.36F)})
                    Else
                        g.DrawLine(pioro, x + s / 2, y + s * 0.26F, x + s / 2, y + s * 0.56F)
                        Using kropka As New SolidBrush(Color.White)
                            g.FillEllipse(kropka, x + s / 2 - grubosc * 0.7F, y + s * 0.68F, grubosc * 1.4F, grubosc * 1.4F)
                        End Using
                    End If
                End Using
        End Select
        g.SmoothingMode = tryb
    End Sub

    ''' <summary>Kolorowa kropka stanu sesji na pasku stanu.</summary>
    Friend Shared Function KolorStanu(stan As StanIkonySesji) As Color
        Select Case stan
            Case StanIkonySesji.Gotowa : Return KolorAkcjiOkna
            Case StanIkonySesji.Pobieranie : Return KolorPostepu
            Case StanIkonySesji.Pobrana : Return KolorSukcesu
            Case StanIkonySesji.Bledy : Return Color.Firebrick
            Case Else : Return Color.FromArgb(150, 165, 182)
        End Select
    End Function
End Class
