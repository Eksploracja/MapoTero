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

Imports GMap.NET
Imports GMap.NET.WindowsForms

''' <summary>
''' Uchwyt zmiany zasięgu siatki segmentów. Kwadrat w narożniku albo na środku boku siatki - przeciągnięcie lewym
''' przyciskiem myszy przesuwa wskazane krawędzie siatki o pełne segmenty; koło w środku siatki - przesuwa całą siatkę.
''' </summary>
<CLSCompliant(False)>
Public Class UchwytSiatki
    Inherits GMapMarker

    Private Const Bok As Integer = 10
    Private Const SrednicaSrodka As Integer = 14

    ''' <summary>Krawędź północ-południe przesuwana uchwytem: 1 - górna (XGora), -1 - dolna (XDol), 0 - żadna.</summary>
    Public ReadOnly Property KierunekX As Integer

    ''' <summary>Krawędź wschód-zachód przesuwana uchwytem: 1 - prawa (YPrawy), -1 - lewa (YLewy), 0 - żadna.</summary>
    Public ReadOnly Property KierunekY As Integer

    Public Sub New(polozenie As PointLatLng, kierunekX As Integer, kierunekY As Integer)
        MyBase.New(polozenie)
        Me.KierunekX = kierunekX
        Me.KierunekY = kierunekY
        Dim rozmiar As Integer = If(Przesuwanie, SrednicaSrodka, Bok)
        Size = New Size(rozmiar, rozmiar)
        'środek uchwytu dokładnie w punkcie siatki (narożnik, środek boku, środek siatki)
        Offset = New Point(-rozmiar \ 2, -rozmiar \ 2)
    End Sub

    ''' <summary>Uchwyt w środku siatki - przesuwa całą siatkę bez zmiany liczby segmentów.</summary>
    Public ReadOnly Property Przesuwanie As Boolean
        Get
            Return KierunekX = 0 AndAlso KierunekY = 0
        End Get
    End Property

    ''' <summary>Kursor odpowiadający kierunkowi przesuwania.</summary>
    Public ReadOnly Property Kursor As Cursor
        Get
            If Przesuwanie Then Return Cursors.SizeAll
            If KierunekX = 0 Then Return Cursors.SizeWE
            If KierunekY = 0 Then Return Cursors.SizeNS
            'prawy górny i lewy dolny narożnik - przekątna "/", pozostałe - "\"
            Return If(KierunekX = KierunekY, Cursors.SizeNESW, Cursors.SizeNWSE)
        End Get
    End Property

    Public Overrides Sub OnRender(g As Graphics)
        Dim r As New Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width - 1, Size.Height - 1)
        Using pioro As New Pen(Color.RoyalBlue, 2)
            If Przesuwanie Then
                'koło z krzyżykiem
                Dim tryb = g.SmoothingMode
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.FillEllipse(Brushes.White, r)
                g.DrawEllipse(pioro, r)
                g.SmoothingMode = tryb
                Dim sx As Integer = r.X + r.Width \ 2, sy As Integer = r.Y + r.Height \ 2
                Using krzyz As New Pen(Color.RoyalBlue, 1)
                    g.DrawLine(krzyz, sx - 3, sy, sx + 3, sy)
                    g.DrawLine(krzyz, sx, sy - 3, sx, sy + 3)
                End Using
            Else
                g.FillRectangle(Brushes.White, r)
                g.DrawRectangle(pioro, r)
            End If
        End Using
    End Sub

End Class
