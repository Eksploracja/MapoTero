'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core

''' <summary>
''' Wczytanie poligonów lub linii z pliku GeoPackage: wybór warstwy (gdy plik ma kilka pasujących), sprawdzenie
''' układu współrzędnych i odczyt geometrii. Komunikaty o problemach pokazuje sam, więc wynik Nothing oznacza
''' „nic do zrobienia”.
''' </summary>
Friend Module WyborWarstwyGeoPackage

    ''' <summary>
    ''' Geometria wybranej warstwy pliku. <paramref name="linie"/> - szukane linie (trasa korytarza), inaczej poligony;
    ''' gdy plik nie ma warstw tego rodzaju, a ma drugiego - proponowane są one.
    ''' </summary>
    Friend Function Wczytaj(rodzic As IWin32Window, plik As String, linie As Boolean) As GeometriaWektorowa
        Dim tytul = If(linie, "Trasa z GeoPackage", "Obszar z GeoPackage")
        Dim warstwy As List(Of WarstwaGeoPackage)
        Try
            warstwy = CzytnikGeoPackage.Warstwy(plik).Where(Function(w) w.MozePoligony OrElse w.MozeLinie).ToList()
        Catch ex As Exception
            MessageBox.Show(rodzic, "Nie udało się otworzyć pliku GeoPackage: " & OpisBledu.Z(ex), tytul, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
        If warstwy.Count = 0 Then
            MessageBox.Show(rodzic, "Plik " & IO.Path.GetFileName(plik) & " nie zawiera warstw z poligonami ani liniami " &
                            "(może to być GeoPackage z kafelkami mapy).", tytul, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return Nothing
        End If
        ' Najpierw warstwy szukanego rodzaju, potem pozostałe.
        warstwy = warstwy.OrderByDescending(Function(w) If(linie, w.MozeLinie, w.MozePoligony)).ToList()
        Dim warstwa = If(warstwy.Count = 1, warstwy(0), WybierzWarstwe(rodzic, plik, warstwy, tytul))
        If warstwa Is Nothing Then Return Nothing
        If Not GeometriaWektorowa.ObslugiwanyUklad(warstwa.Epsg) Then
            MessageBox.Show(rodzic, GeometriaWektorowa.OpisNieobslugiwanegoUkladu(warstwa.Epsg), tytul, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return Nothing
        End If
        Try
            Dim geometria = CzytnikGeoPackage.Wczytaj(plik, warstwa)
            If geometria.Poligony.Count = 0 AndAlso geometria.Linie.Count = 0 Then
                MessageBox.Show(rodzic, "Warstwa " & warstwa.Nazwa & " nie zawiera poligonów ani linii.", tytul, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return Nothing
            End If
            Return geometria
        Catch ex As Exception
            MessageBox.Show(rodzic, "Nie udało się odczytać warstwy " & warstwa.Nazwa & ": " & OpisBledu.Z(ex), tytul, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    Private Function WybierzWarstwe(rodzic As IWin32Window, plik As String, warstwy As List(Of WarstwaGeoPackage), tytul As String) As WarstwaGeoPackage
        Using okno As New Form With {.Text = tytul, .FormBorderStyle = FormBorderStyle.FixedDialog, .MaximizeBox = False, .MinimizeBox = False,
                                     .ShowInTaskbar = False, .StartPosition = FormStartPosition.CenterParent, .ClientSize = New Size(460, 260),
                                     .Font = New Font("Segoe UI", 9.0F), .AutoScaleMode = AutoScaleMode.Font}
            Dim opis As New Label With {.Text = "Plik " & IO.Path.GetFileName(plik) & " ma kilka warstw. Wybierz warstwę, która wyznaczy obszar pobierania:",
                                        .Location = New Point(12, 10), .Size = New Size(436, 36)}
            Dim lista As New ListBox With {.Location = New Point(12, 50), .Size = New Size(436, 160), .IntegralHeight = False, .Name = "lstWarstwyGpkg"}
            lista.Items.AddRange(warstwy.Cast(Of Object)().ToArray())
            lista.SelectedIndex = 0
            Dim ok As New Button With {.Text = "OK", .DialogResult = DialogResult.OK, .Size = New Size(80, 28), .Location = New Point(282, 220)}
            Dim anuluj As New Button With {.Text = "Anuluj", .DialogResult = DialogResult.Cancel, .Size = New Size(80, 28), .Location = New Point(368, 220)}
            AddHandler lista.DoubleClick, Sub() If lista.SelectedIndex >= 0 Then okno.DialogResult = DialogResult.OK
            okno.Controls.AddRange({opis, lista, ok, anuluj})
            okno.AcceptButton = ok
            okno.CancelButton = anuluj
            If okno.ShowDialog(rodzic) <> DialogResult.OK Then Return Nothing
            Return DirectCast(lista.SelectedItem, WarstwaGeoPackage)
        End Using
    End Function
End Module
