'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports MapoTero.Core
Imports Xunit

Public Class ProfilNmtTesty

    <Fact>
    Public Sub ObliczStatystykiProfilu()
        Dim punkty = New List(Of PunktGeo) From {
            New PunktGeo(49.2, 20.0),
            New PunktGeo(49.21, 20.0),
            New PunktGeo(49.22, 20.0),
            New PunktGeo(49.23, 20.0)
        }
        Dim wys = New List(Of Double) From {500.0, 550.0, 530.0, 600.0}
        Dim stat = ProfilNmt.ObliczStatystyki(punkty, wys)

        Assert.Equal(500.0, stat.MinWysokosc)
        Assert.Equal(600.0, stat.MaxWysokosc)
        Assert.Equal(120.0, stat.Podejscia) ' +50 oraz +70 = 120
        Assert.Equal(20.0, stat.Zejscia)     ' -20 = 20
        Assert.True(stat.DlugoscKm > 0)
        Assert.Equal(4, stat.Punkty.Count)
    End Sub

    <Fact>
    Public Sub ZapisGpxZWysokosciami()
        Dim plik = Path.GetTempFileName()
        Try
            Dim linie = New List(Of List(Of PunktGeo)) From {
                New List(Of PunktGeo) From {
                    New PunktGeo(49.195, 19.981),
                    New PunktGeo(49.200, 19.985)
                }
            }
            Dim hLinie = New List(Of IList(Of Double)) From {
                New List(Of Double) From {1250.5, 1310.2}
            }
            Korytarz.ZapiszGpxZWysokosciami(plik, "Tatry Test", linie, hLinie)

            Assert.True(File.Exists(plik))
            Dim doc = XDocument.Load(plik)
            Dim ns As XNamespace = "http://www.topografix.com/GPX/1/1"
            Dim trkpts = doc.Descendants(ns + "trkpt").ToList()
            Assert.Equal(2, trkpts.Count)
            Assert.Equal("1250.5", trkpts(0).Element(ns + "ele").Value)
            Assert.Equal("1310.2", trkpts(1).Element(ns + "ele").Value)
            Assert.Equal("Tatry Test", doc.Descendants(ns + "trk").First().Element(ns + "name").Value)
        Finally
            If File.Exists(plik) Then File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Async Function PobierzWysokosciLiveIRysujWykresAsync() As Task
        ' Trasa Kuźnice (1000m) -> Kasprowy Wierch (1987m)
        Dim n = 20
        Dim punkty As New List(Of PunktGeo)()
        Dim startLat = 49.269, startLon = 19.981
        Dim endLat = 49.232, endLon = 19.981
        For i = 0 To n - 1
            Dim t = i / CDbl(n - 1)
            punkty.Add(New PunktGeo(startLat + t * (endLat - startLat), startLon + t * (endLon - startLon)))
        Next

        Dim wys = Await ProfilNmt.PobierzWysokosciAsync(punkty)
        Assert.Equal(n, wys.Length)
        Assert.True(wys(0) > 900.0 AndAlso wys(0) < 1100.0) ' Kuźnice ~1000 m
        Assert.True(wys(n - 1) > 1900.0 AndAlso wys(n - 1) < 2050.0) ' Kasprowy ~1987 m

        Dim stat = ProfilNmt.ObliczStatystyki(punkty, wys)
        Assert.True(stat.Podejscia > 800.0)

        ' Wygenerowanie wykresu profilu wysokościowego do weryfikacji wizualnej
        Dim sciezkaWykresu = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf\profil_nmt_kasprowy.png"
        Using bmp As New Bitmap(600, 300)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.FromArgb(248, 250, 252))
                g.SmoothingMode = SmoothingMode.AntiAlias

                ' Tytuł i statystyki
                Using fntTytul As New Font("Segoe UI", 11.0F, FontStyle.Bold),
                      fntOpis As New Font("Segoe UI", 8.5F, FontStyle.Regular),
                      pismo As New SolidBrush(Color.FromArgb(30, 41, 59))
                    g.DrawString("Profil wysokościowy NMT: Kuźnice -> Kasprowy Wierch (Tatry)", fntTytul, pismo, 15, 12)
                    g.DrawString($"Min: {stat.MinWysokosc:0} m  |  Max: {stat.MaxWysokosc:0} m  |  Podejścia: +{stat.Podejscia:0} m  |  Dystans: {stat.DlugoscKm:0.0} km", fntOpis, pismo, 15, 34)
                End Using

                Dim marginesLewy = 50, marginesDol = 40, marginesGora = 60, marginesPrawy = 20
                Dim szerWykresu = bmp.Width - marginesLewy - marginesPrawy
                Dim wysWykresu = bmp.Height - marginesGora - marginesDol

                Dim minH = Math.Floor(stat.MinWysokosc / 100.0) * 100.0
                Dim maxH = Math.Ceiling(stat.MaxWysokosc / 100.0) * 100.0
                Dim deltaH = Math.Max(100.0, maxH - minH)

                ' Siatka pozioma
                Using penSiatka As New Pen(Color.FromArgb(226, 232, 240), 1),
                      fntOs As New Font("Segoe UI", 7.5F),
                      pedzelOs As New SolidBrush(Color.FromArgb(100, 116, 139))
                    For h = minH To maxH Step 200.0
                        Dim y = marginesGora + wysWykresu - CSng((h - minH) / deltaH * wysWykresu)
                        g.DrawLine(penSiatka, marginesLewy, y, marginesLewy + szerWykresu, y)
                        g.DrawString($"{h:0}m", fntOs, pedzelOs, 5, y - 6)
                    Next
                End Using

                ' Rysowanie obszaru i linii profilu
                Dim pts(stat.Punkty.Count - 1) As PointF
                Dim ptsArea(stat.Punkty.Count + 1) As PointF
                Dim maxDystans = Math.Max(1.0, stat.Punkty.Last().DystansMetry)

                For i = 0 To stat.Punkty.Count - 1
                    Dim p = stat.Punkty(i)
                    Dim x = marginesLewy + CSng(p.DystansMetry / maxDystans * szerWykresu)
                    Dim y = marginesGora + wysWykresu - CSng((p.Wysokosc - minH) / deltaH * wysWykresu)
                    pts(i) = New PointF(x, y)
                    ptsArea(i) = New PointF(x, y)
                Next
                ptsArea(stat.Punkty.Count) = New PointF(marginesLewy + szerWykresu, marginesGora + wysWykresu)
                ptsArea(stat.Punkty.Count + 1) = New PointF(marginesLewy, marginesGora + wysWykresu)

                Using gradBrush As New LinearGradientBrush(New Point(0, marginesGora), New Point(0, marginesGora + wysWykresu),
                                                          Color.FromArgb(160, 59, 130, 246), Color.FromArgb(20, 59, 130, 246))
                    g.FillPolygon(gradBrush, ptsArea)
                End Using

                Using penProfil As New Pen(Color.FromArgb(37, 99, 235), 2.5F)
                    g.DrawLines(penProfil, pts)
                End Using
            End Using
            bmp.Save(sciezkaWykresu, Imaging.ImageFormat.Png)
        End Using

        Assert.True(File.Exists(sciezkaWykresu))
        Assert.True(New FileInfo(sciezkaWykresu).Length > 5000)
    End Function

End Class
