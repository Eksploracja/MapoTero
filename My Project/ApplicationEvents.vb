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

Namespace My

    Partial Friend Class MyApplication

#If NETCOREAPP Then
        ''' <summary>
        ''' Domyślna czcionka okien taka jak w .NET Framework (Microsoft Sans Serif 8,25 pt). W .NET 8 domyślną czcionką
        ''' jest Segoe UI 9 pt, a formularze skalują się według czcionki (AutoScaleMode.Font) - wszystkie okna były
        ''' o ok. 20% większe niż w projekcie, okno główne nie mieściło się na ekranie, a przyciski i pole komunikatów
        ''' zakotwiczone do dolnej krawędzi prawego panelu wjeżdżały pod grupę "Wybrane warstwy oraz ich kolejność".
        ''' </summary>
        Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As Global.Microsoft.VisualBasic.ApplicationServices.ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
            e.Font = New Font("Microsoft Sans Serif", 8.25!, FontStyle.Regular, GraphicsUnit.Point)
        End Sub
#End If

    End Class

End Namespace
