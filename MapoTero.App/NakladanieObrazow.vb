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

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices

''' <summary>
''' Zamiana obrazów na tablice pikseli ARGB i z powrotem - do nakładania warstw z kryciem
''' (obliczenia w <see cref="MapoTero.Core.NakladanieWarstw"/>).
''' </summary>
Friend NotInheritable Class NakladanieObrazow

    Private Sub New()
    End Sub

    ''' <summary>Piksele ARGB (wierszami) obrazu zapisanego w danych, przeskalowanego do podanego rozmiaru.</summary>
    Public Shared Function Piksele(dane() As Byte, szerokosc As Integer, wysokosc As Integer) As Integer()
        Using ms As New MemoryStream(dane)
            Using obraz As Image = Image.FromStream(ms)
                Return Piksele(obraz, szerokosc, wysokosc)
            End Using
        End Using
    End Function

    Public Shared Function Piksele(obraz As Image, szerokosc As Integer, wysokosc As Integer) As Integer()
        Using bmp As New Bitmap(szerokosc, wysokosc, PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.Transparent)
                g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                g.InterpolationMode = If(obraz.Width = szerokosc AndAlso obraz.Height = wysokosc,
                                         Drawing2D.InterpolationMode.NearestNeighbor, Drawing2D.InterpolationMode.HighQualityBicubic)
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
                g.DrawImage(obraz, New Rectangle(0, 0, szerokosc, wysokosc), 0, 0, obraz.Width, obraz.Height, GraphicsUnit.Pixel)
            End Using
            Dim wynik(szerokosc * wysokosc - 1) As Integer
            Dim d = bmp.LockBits(New Rectangle(0, 0, szerokosc, wysokosc), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            Try
                For y = 0 To wysokosc - 1
                    Marshal.Copy(IntPtr.Add(d.Scan0, y * d.Stride), wynik, y * szerokosc, szerokosc)
                Next
            Finally
                bmp.UnlockBits(d)
            End Try
            Return wynik
        End Using
    End Function

    ''' <summary>Bitmapa 32-bitowa z pikseli ARGB (wierszami).</summary>
    Public Shared Function DoBitmapy(piksele() As Integer, szerokosc As Integer, wysokosc As Integer) As Bitmap
        Dim bmp As New Bitmap(szerokosc, wysokosc, PixelFormat.Format32bppArgb)
        Dim d = bmp.LockBits(New Rectangle(0, 0, szerokosc, wysokosc), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        Try
            For y = 0 To wysokosc - 1
                Marshal.Copy(piksele, y * szerokosc, IntPtr.Add(d.Scan0, y * d.Stride), szerokosc)
            Next
        Finally
            bmp.UnlockBits(d)
        End Try
        Return bmp
    End Function

End Class
