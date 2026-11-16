'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks
Imports MapoTero.Core
Imports Xunit

Public Class KorekcjaMipTesty

    <Fact>
    Public Sub KorekcjaMip_Wylaczona_NieModyfikujePikseli()
        Dim piksele() As Integer = {&HFF102030, &HFF405060}
        Dim kopia() As Integer = CType(piksele.Clone(), Integer())

        Dim opcje As New OpcjeKorekcjiMip With {.Wlaczona = False}
        KorekcjaMip.Zastosuj(piksele, 2, 1, opcje)

        Assert.Equal(kopia(0), piksele(0))
        Assert.Equal(kopia(1), piksele(1))
    End Sub

    <Fact>
    Public Sub KorekcjaMip_RozjasniaCiemneOdcienie_Gamma()
        ' Ciemny piksel lasu: R=40, G=60, B=30
        Dim c = &HFF000000 Or (40 << 16) Or (60 << 8) Or 30
        Dim piksele() As Integer = {c}

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 0.75,
            .Kontrast = 1.0,
            .Nasycenie = 1.0,
            .Wyostrzanie = 0.0
        }
        KorekcjaMip.Zastosuj(piksele, 1, 1, opcje)

        Dim outG = (piksele(0) >> 8) And &HFF
        ' Dla gamma 0.75 wartość 60/255=0.235 po podniesieniu do 0.75 wynosi ok. 0.337 (~86)
        Assert.True(outG > 60, "Gamma 0.75 powinna rozjaśnić ciemne partie zieleni")
    End Sub

    <Fact>
    Public Sub KorekcjaMip_PodbijaNasycenie()
        ' Przygaszony błękit rzeki: R=50, G=80, B=120
        Dim c = &HFF000000 Or (50 << 16) Or (80 << 8) Or 120
        Dim piksele() As Integer = {c}

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 1.0,
            .Kontrast = 1.0,
            .Nasycenie = 1.5,
            .Wyostrzanie = 0.0
        }
        KorekcjaMip.Zastosuj(piksele, 1, 1, opcje)

        Dim outB = piksele(0) And &HFF
        Assert.True(outB > 120, "Zwiększenie nasycenia powinno podbić wiodący kanał niebieski")
    End Sub

    <Fact>
    Public Sub KorekcjaMip_WyostrzaKrawedzie_Laplacian()
        ' Obraz 3x3 z ciemną kropką w środku: tło jasnoszare, środek ciemniejszy
        Dim jasny = &HFF808080
        Dim ciemny = &HFF404040
        Dim piksele(8) As Integer
        For i = 0 To 8
            piksele(i) = jasny
        Next
        piksele(4) = ciemny ' środek

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 1.0,
            .Kontrast = 1.0,
            .Nasycenie = 1.0,
            .Wyostrzanie = 0.8
        }
        KorekcjaMip.Zastosuj(piksele, 3, 3, opcje)

        Dim centerVal = (piksele(4) >> 16) And &HFF
        ' Wyostrzanie powinno pogłębić kontrast krawędzi (środek ciemniejszy niż początkowe 64)
        Assert.True(centerVal < 64, "Wyostrzanie krawędzi powinno pogłębić różnicę na krawędzi")
    End Sub

    <Fact>
    Public Async Function KorekcjaMip_RealnyObrazZInternetu_GenerujePorownanieAsync() As Task
        ' Pobieramy autentyczny kafelek topograficzny OpenTopoMap z internetu
        Dim url = "https://a.tile.opentopomap.org/14/9070/5688.png"
        Dim dane() As Byte = Nothing

        Using http As New HttpClient()
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 MapoTeroTest/1.0")
            Try
                dane = Await http.GetByteArrayAsync(url).ConfigureAwait(False)
            Catch
            End Try
        End Using

        Dim origBmp As Bitmap
        If dane IsNot Nothing AndAlso dane.Length > 0 Then
            Using ms As New MemoryStream(dane)
                origBmp = New Bitmap(Image.FromStream(ms))
            End Using
        Else
            ' Fallback syntetyczny
            origBmp = New Bitmap(256, 256, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(origBmp)
                g.Clear(Color.FromArgb(70, 105, 75))
            End Using
        End If

        Dim szer = origBmp.Width
        Dim wys = origBmp.Height

        ' Pobieramy piksele do tablicy ARGB
        Dim piksele(szer * wys - 1) As Integer
        Dim bd = origBmp.LockBits(New Rectangle(0, 0, szer, wys), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wys - 1
                System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(bd.Scan0, y * bd.Stride), piksele, y * szer, szer)
            Next
        Finally
            origBmp.UnlockBits(bd)
        End Try

        ' Kopia pikseli do przetworzenia
        Dim pikseleMip(piksele.Length - 1) As Integer
        Array.Copy(piksele, pikseleMip, piksele.Length)

        Dim opcje As New OpcjeKorekcjiMip With {
            .Wlaczona = True,
            .Gamma = 0.75,
            .Kontrast = 1.25,
            .Nasycenie = 1.35,
            .Wyostrzanie = 0.6
        }
        KorekcjaMip.Zastosuj(pikseleMip, szer, wys, opcje)

        ' Tworzymy zmodyfikowaną bitmapę
        Dim mipBmp As New Bitmap(szer, wys, PixelFormat.Format32bppArgb)
        Dim bdMip = mipBmp.LockBits(New Rectangle(0, 0, szer, wys), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wys - 1
                System.Runtime.InteropServices.Marshal.Copy(pikseleMip, y * szer, IntPtr.Add(bdMip.Scan0, y * bdMip.Stride), szer)
            Next
        Finally
            mipBmp.UnlockBits(bdMip)
        End Try

        ' Tworzymy planszę porównawczą side-by-side
        Dim padding = 20
        Dim naglowekWys = 40
        Dim wynikSzer = szer * 2 + padding * 3
        Dim wynikWys = wys + naglowekWys + padding * 2

        Using kompozycja As New Bitmap(wynikSzer, wynikWys, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(kompozycja)
                g.Clear(Color.FromArgb(245, 247, 250))

                Using fTytul As New Font("Segoe UI", 11.0!, FontStyle.Bold)
                Using fPodpis As New Font("Segoe UI", 9.0!, FontStyle.Regular)
                Using bTytul As New SolidBrush(Color.FromArgb(30, 41, 59))
                Using bRamka As New Pen(Color.FromArgb(203, 213, 225), 1.0!)

                    ' Etykiety
                    g.DrawString("Oryginalny raster WMS/XYZ (surowy)", fTytul, bTytul, padding, 12)
                    g.DrawString("Po optymalizacji MIP (Garmin / pełne słońce)", fTytul, bTytul, szer + padding * 2, 12)

                    ' Rysowanie obrazów
                    g.DrawImage(origBmp, padding, naglowekWys, szer, wys)
                    g.DrawRectangle(bRamka, padding, naglowekWys, szer, wys)

                    g.DrawImage(mipBmp, szer + padding * 2, naglowekWys, szer, wys)
                    g.DrawRectangle(bRamka, szer + padding * 2, naglowekWys, szer, wys)

                    Dim parametry = $"Parametry MIP: Gamma={opcje.Gamma:F2}, Kontrast={opcje.Kontrast:F2}, Nasycenie={opcje.Nasycenie:F2}, Wyostrzanie={opcje.Wyostrzanie:F2}"
                    g.DrawString(parametry, fPodpis, Brushes.Gray, padding, wys + naglowekWys + 5)
                End Using
                End Using
                End Using
                End Using
            End Using

            ' Zapis do katalogu artefaktów sesji
            Dim folderArtefaktow = "C:\Users\KN\.gemini\antigravity\brain\6226a50c-ad35-435f-a04d-74ae97526ccf"
            If Directory.Exists(folderArtefaktow) Then
                Dim sciezkaWyniku = Path.Combine(folderArtefaktow, "korekcja_mip_porownanie.png")
                kompozycja.Save(sciezkaWyniku, ImageFormat.Png)
            End If
        End Using

        origBmp.Dispose()
        mipBmp.Dispose()

        ' Weryfikacja że piksele uległy zmianie
        Dim roznice = 0
        For i = 0 To piksele.Length - 1
            If piksele(i) <> pikseleMip(i) Then roznice += 1
        Next
        Assert.True(roznice > piksele.Length * 0.5, "Większość pikseli powinna ulec zoptymalizowaniu")
    End Function

End Class
