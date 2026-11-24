'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports MapoTero.Core
Imports MapoTero.Core.GeometriaZasieguGalerii

''' <summary>
''' Renderuje obraz mapy zasięgu warstwy na podstawie danych z <c>warstwy.json</c> (<see cref="PozycjaGaleriiWarstw"/>),
''' w tym dokładnej siatki pokrycia <c>maska_hex</c> z widocznymi „dziurami” wewnątrz Polski.
''' </summary>
Public NotInheritable Class RendererZasieguGalerii

    Private Sub New()
    End Sub

    Private Shared Function NaPointF(punkty As PunktMapyGalerii()) As PointF()
        Dim wynik(punkty.Length - 1) As PointF
        For i As Integer = 0 To punkty.Length - 1
            wynik(i) = New PointF(punkty(i).X, punkty(i).Y)
        Next
        Return wynik
    End Function

    ''' <summary>Tworzy bitmapę zasięgu warstwy o zadanej szerokości i wysokości (domyślnie 330×288 px).</summary>
    Public Shared Function Renderuj(pozycja As PozycjaGaleriiWarstw, Optional szerokosc As Integer = 330, Optional wysokosc As Integer = 288) As Bitmap
        If szerokosc < 120 Then szerokosc = 120
        If wysokosc < 100 Then wysokosc = 100

        Dim bmp As New Bitmap(szerokosc, wysokosc, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
            g.Clear(Color.FromArgb(248, 250, 252))

            Dim z = If(pozycja?.Zasieg, New ZasiegWarstwyGalerii())
            Dim typ = If(z.Typ, "kraj").ToLowerInvariant()
            Dim kod = If(z.Kod, "").ToUpperInvariant()
            Dim komorki = DekodujMaskeHex(z.MaskaHex)
            Dim maKomorki = komorki.Count > 0

            'Skalowanie viewBox (25..340 × 0..275 -> 315 × 275) w obszar bitmapy
            Dim margines As Single = 4.0F
            Dim skalaX = (szerokosc - 2 * margines) / 315.0F
            Dim skalaY = (wysokosc - 2 * margines) / 275.0F
            Dim skala = Math.Min(skalaX, skalaY)
            Dim offX = (szerokosc - 315.0F * skala) / 2.0F - 25.0F * skala
            Dim offY = (wysokosc - 275.0F * skala) / 2.0F

            Dim stan = g.Save()
            g.TranslateTransform(offX, offY)
            g.ScaleTransform(skala, skala)

            Dim kolorTlaKraju = Color.FromArgb(226, 232, 240)
            Dim kolorObramowaniaKraju = Color.FromArgb(148, 163, 184)
            Dim kolorPolski = Color.FromArgb(238, 242, 246)
            Dim kolorAktywny = Color.FromArgb(59, 130, 246)
            Dim kolorAktywnyWyp = Color.FromArgb(147, 197, 253)
            Dim kolorBaltyk = Color.FromArgb(219, 234, 254)
            Dim kolorBaltykAktywny = Color.FromArgb(96, 165, 250)
            Dim kolorZagranicaAktywny = Color.FromArgb(110, 175, 245)

            'Bałtyk
            Using pedzel As New SolidBrush(If(typ = "morski", kolorBaltykAktywny, kolorBaltyk))
                g.FillPolygon(pedzel, NaPointF(Baltyk))
            End Using

            'Sąsiedzi
            RysujKraj(g, NiemcyReszta, If(typ = "zagranica" AndAlso kod = "DE", kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)
            RysujKraj(g, NiemcyBrandenburgia, If(typ = "zagranica" AndAlso (kod = "DE-BB" OrElse kod = "DE"), kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)
            RysujKraj(g, NiemcySaksonia, If(typ = "zagranica" AndAlso (kod = "DE-SN" OrElse kod = "DE"), kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)
            RysujKraj(g, Czechy, If(typ = "zagranica" AndAlso kod = "CZ", kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)
            RysujKraj(g, Slowacja, If(typ = "zagranica" AndAlso kod = "SK", kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)
            RysujKraj(g, Litwa, If(typ = "zagranica" AndAlso kod = "LT", kolorZagranicaAktywny, kolorTlaKraju), kolorObramowaniaKraju)

            'Polska (tło bazowe)
            Dim polskaWyp As Color = kolorPolski
            If typ = "kraj" AndAlso Not maKomorki Then
                polskaWyp = kolorAktywnyWyp
            ElseIf typ = "regionalny" OrElse typ = "czesciowy" OrElse maKomorki Then
                polskaWyp = Color.FromArgb(241, 245, 249)
            End If
            RysujKraj(g, Polska, polskaWyp, Color.FromArgb(71, 85, 105), 1.2F)

            'Komórki rzeczywistego zasięgu z maska_hex (pokazuje dziury wewnątrz Polski)
            If maKomorki Then
                Dim uzyjClip = Not z.ClipPolska.HasValue OrElse z.ClipPolska.Value
                Dim stanClip = g.Save()
                If uzyjClip Then
                    Using sciezkaPolski As New GraphicsPath()
                        sciezkaPolski.AddPolygon(NaPointF(Polska))
                        g.SetClip(sciezkaPolski, CombineMode.Intersect)
                    End Using
                End If
                Using pedzelKomorki As New SolidBrush(Color.FromArgb(185, 37, 99, 235))
                    For Each k In komorki
                        g.FillRectangle(pedzelKomorki, k.X, k.Y, k.Szerokosc, k.Wysokosc)
                    Next
                End Using
                g.Restore(stanClip)
                Using pioroGranicy As New Pen(Color.FromArgb(71, 85, 105), 1.2F)
                    g.DrawPolygon(pioroGranicy, NaPointF(Polska))
                End Using
            End If

            'Rzeki główne
            Using pioroRzeki As New Pen(Color.FromArgb(147, 197, 253), 1.1F)
                g.DrawLines(pioroRzeki, NaPointF(Wisla))
                g.DrawLines(pioroRzeki, NaPointF(Odra))
            End Using

            'Etykiety państw
            Using fntKraj As New Font("Segoe UI", 7.0F, FontStyle.Bold)
                Using fntPl As New Font("Segoe UI", 8.0F, FontStyle.Bold)
                    RysujEtykiete(g, "BAŁTYK", 180.0F, 44.0F, fntKraj, Color.FromArgb(59, 130, 246))
                    RysujEtykiete(g, "NIEMCY", 82.0F, 126.0F, fntKraj, Color.FromArgb(100, 116, 139))
                    RysujEtykiete(g, "CZECHY", 135.0F, 211.0F, fntKraj, Color.FromArgb(100, 116, 139))
                    RysujEtykiete(g, "SŁOWACJA", 210.0F, 246.0F, fntKraj, Color.FromArgb(100, 116, 139))
                    RysujEtykiete(g, "LITWA", 278.0F, 51.0F, fntKraj, Color.FromArgb(100, 116, 139))
                    RysujEtykiete(g, "POLSKA", 195.0F, 141.0F, fntPl, If(typ = "kraj" OrElse maKomorki, Color.FromArgb(30, 58, 138), Color.FromArgb(71, 85, 105)))
                End Using
            End Using

            'Miasta orientacyjne
            If typ <> "globalny" Then
                Using fntMiasto As New Font("Segoe UI", 6.2F, FontStyle.Regular)
                    Using pedzelKropki As New SolidBrush(Color.FromArgb(51, 65, 85))
                        Using pedzelNapisu As New SolidBrush(Color.FromArgb(51, 65, 85))
                            For Each m In Miasta
                                g.FillEllipse(pedzelKropki, m.X - 2.0F, m.Y - 2.0F, 4.0F, 4.0F)
                                g.DrawString(m.Nazwa, fntMiasto, pedzelNapisu, m.X + 3.5F, m.Y - 5.0F)
                            Next
                        End Using
                    End Using
                End Using
            End If

            'Prostokąt BBOX (regionalne bez maski lub morskie bez maski)
            If (typ = "regionalny" OrElse typ = "morski") AndAlso Not maKomorki Then
                If z.Bbox IsNot Nothing AndAlso z.Bbox.Length = 4 Then
                    Dim p1 = GeoDoMapy(z.Bbox(0), z.Bbox(3))
                    Dim p2 = GeoDoMapy(z.Bbox(2), z.Bbox(1))
                    Dim rx = Math.Min(p1.X, p2.X)
                    Dim ry = Math.Min(p1.Y, p2.Y)
                    Dim rw = Math.Max(Math.Abs(p2.X - p1.X), 6.0F)
                    Dim rh = Math.Max(Math.Abs(p2.Y - p1.Y), 6.0F)
                    Using pedzelBox As New SolidBrush(Color.FromArgb(70, 37, 99, 235))
                        Using pioroBox As New Pen(Color.FromArgb(220, 37, 99, 235), 1.6F)
                            g.FillRectangle(pedzelBox, rx, ry, rw, rh)
                            g.DrawRectangle(pioroBox, rx, ry, rw, rh)
                        End Using
                    End Using
                ElseIf typ = "regionalny" AndAlso pozycja IsNot Nothing AndAlso pozycja.Zbior.IndexOf("PrawdziwaOrtofotomapa", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Using fntMiasto As New Font("Segoe UI", 6.4F, FontStyle.Bold)
                        Using pedzelPunkt As New SolidBrush(Color.FromArgb(37, 99, 235))
                            Using pioroPierscien As New Pen(Color.FromArgb(140, 37, 99, 235), 1.5F)
                                For Each m In MiastaTrueOrtho
                                    g.DrawEllipse(pioroPierscien, m.X - 6.0F, m.Y - 6.0F, 12.0F, 12.0F)
                                    g.FillEllipse(pedzelPunkt, m.X - 3.0F, m.Y - 3.0F, 6.0F, 6.0F)
                                    Dim sz = g.MeasureString(m.Nazwa, fntMiasto)
                                    g.DrawString(m.Nazwa, fntMiasto, pedzelPunkt, m.X - sz.Width / 2.0F, m.Y - 17.0F)
                                Next
                            End Using
                        End Using
                    End Using
                End If
            End If

            'Nakładka globalna
            If typ = "globalny" Then
                Using pedzelNakladka As New SolidBrush(Color.FromArgb(145, 219, 234, 254))
                    g.FillRectangle(pedzelNakladka, 25.0F, 0.0F, 315.0F, 275.0F)
                End Using
                Using pedzelGlob As New SolidBrush(Color.FromArgb(220, 239, 246, 255))
                    Using pioroGlob As New Pen(kolorAktywny, 1.8F)
                        g.FillEllipse(pedzelGlob, 182.0F - 46.0F, 137.0F - 46.0F, 92.0F, 92.0F)
                        g.DrawEllipse(pioroGlob, 182.0F - 46.0F, 137.0F - 46.0F, 92.0F, 92.0F)
                        g.DrawEllipse(pioroGlob, 182.0F - 46.0F, 137.0F - 23.0F, 92.0F, 46.0F)
                        g.DrawEllipse(pioroGlob, 182.0F - 23.0F, 137.0F - 46.0F, 46.0F, 92.0F)
                        g.DrawLine(pioroGlob, 182.0F - 46.0F, 137.0F, 182.0F + 46.0F, 137.0F)
                        g.DrawLine(pioroGlob, 182.0F, 137.0F - 46.0F, 182.0F, 137.0F + 46.0F)
                    End Using
                End Using
                Using fntSwiat As New Font("Segoe UI", 8.0F, FontStyle.Bold)
                    RysujEtykiete(g, "CAŁY ŚWIAT", 182.0F, 131.0F, fntSwiat, Color.FromArgb(30, 64, 175))
                End Using
            End If

            g.Restore(stan)
        End Using
        Return bmp
    End Function

    Private Shared Sub RysujKraj(g As Graphics, punkty As PunktMapyGalerii(), wyp As Color, obramowanie As Color, Optional grubosc As Single = 0.8F)
        Dim pts = NaPointF(punkty)
        Using pedzel As New SolidBrush(wyp)
            g.FillPolygon(pedzel, pts)
        End Using
        Using pioro As New Pen(obramowanie, grubosc)
            g.DrawPolygon(pioro, pts)
        End Using
    End Sub

    Private Shared Sub RysujEtykiete(g As Graphics, tekst As String, x As Single, y As Single, czcionka As Font, kolor As Color)
        Using pedzel As New SolidBrush(kolor)
            Dim sz = g.MeasureString(tekst, czcionka)
            g.DrawString(tekst, czcionka, pedzel, x - sz.Width / 2.0F, y)
        End Using
    End Sub
End Class
