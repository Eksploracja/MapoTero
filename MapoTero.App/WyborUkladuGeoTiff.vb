Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Pomocniczy zestaw pozycji i inicjalizacja listy wyboru CRS dla okna konwertera GeoTIFF,
''' działający jako zabezpieczenie w przypadku brakujących lub nietypowych tagów EPSG.
''' </summary>
Public NotInheritable Class WyborUkladuGeoTiff

    Public NotInheritable Class PozycjaCrs
        Public ReadOnly Property Epsg As Integer
        Public ReadOnly Property Nazwa As String

        Public Sub New(epsg As Integer, nazwa As String)
            Me.Epsg = epsg
            Me.Nazwa = nazwa
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} (EPSG:{1})", Nazwa, Epsg)
        End Function
    End Class

    Public Shared ReadOnly ObslugiwaneUklady As PozycjaCrs() = {
        New PozycjaCrs(2180, "PL-1992 / PUWG 1992"),
        New PozycjaCrs(2176, "PL-2000 strefa 5 (15°E)"),
        New PozycjaCrs(2177, "PL-2000 strefa 6 (18°E)"),
        New PozycjaCrs(2178, "PL-2000 strefa 7 (21°E)"),
        New PozycjaCrs(2179, "PL-2000 strefa 8 (24°E)"),
        New PozycjaCrs(32633, "WGS 84 / UTM 33N"),
        New PozycjaCrs(32634, "WGS 84 / UTM 34N"),
        New PozycjaCrs(32635, "WGS 84 / UTM 35N"),
        New PozycjaCrs(3857, "Web Mercator"),
        New PozycjaCrs(4326, "WGS 84 (geograficzny)")
    }

    Public Shared Sub WypelnijListeCrs(cmb As ComboBox, wykrytyEpsg As Integer?, lblOstrzezenie As Label)
        cmb.DropDownStyle = ComboBoxStyle.DropDownList
        cmb.Items.Clear()
        cmb.Items.AddRange(ObslugiwaneUklady)

        Dim indeks As Integer = -1
        If wykrytyEpsg.HasValue Then
            For i As Integer = 0 To ObslugiwaneUklady.Length - 1
                If ObslugiwaneUklady(i).Epsg = wykrytyEpsg.Value Then
                    indeks = i
                    Exit For
                End If
            Next
        End If

        If indeks >= 0 Then
            cmb.SelectedIndex = indeks
            If lblOstrzezenie IsNot Nothing Then
                lblOstrzezenie.ForeColor = Color.DarkGreen
                lblOstrzezenie.Text = String.Format("Układ rozpoznany automatycznie (EPSG:{0}).", wykrytyEpsg.Value)
            End If
        Else
            cmb.SelectedIndex = 0
            If lblOstrzezenie IsNot Nothing Then
                lblOstrzezenie.ForeColor = Color.DarkOrange
                lblOstrzezenie.Text = "Brak rozpoznanego tagu CRS w GeoTIFF/.prj – wybierz układ ręcznie z listy."
            End If
        End If
    End Sub
End Class
