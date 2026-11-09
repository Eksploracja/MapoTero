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

Imports System.Diagnostics

Public Class pomoc_pfe

    Private Const UrlPomocy As String = "https://forum.eksploracja.pl/viewforum.php?f=205"

    Private Sub pomoc_pfe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            WebBrowser1.ScriptErrorsSuppressed = True
            WebBrowser1.Navigate(UrlPomocy)
        Catch
        End Try
    End Sub

    ''' <summary>Otwiera forum w domyślnej przeglądarce internetowej systemu.</summary>
    Public Shared Sub OtworzWPrzegladarce()
        Try
            Process.Start(New ProcessStartInfo(UrlPomocy) With {.UseShellExecute = True})
        Catch ex As Exception
            MsgBox("Nie można otworzyć strony pomocy: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
    End Sub

End Class