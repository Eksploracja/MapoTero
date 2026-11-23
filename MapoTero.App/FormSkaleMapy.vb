'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Ustawienia skal mapy wieloskalowej z kreatora: które skale wchodzą do pliku, z jakim pikselem, na jakich poziomach
''' powiększenia i z jakimi warstwami. Plan sprawdza ZarzadcaSzablonow.SprawdzPlan (zakresy nie mogą się nakładać).
''' </summary>
Public Class FormSkaleMapy
    Inherits Form

    Private ReadOnly _szablon As SzablonMapy
    Private ReadOnly _planAuto As List(Of PlanStopnia)
    Private ReadOnly _szerokoscGeo As Double
    ' Jeden wiersz na każdy stopień skali szablonu (także nieużywany)
    Private ReadOnly _wiersze As New List(Of PlanStopnia)()
    Private ReadOnly _uzyte As New List(Of Boolean)()

    Private ReadOnly dgvSkale As New DataGridView()
    Private ReadOnly clbWarstwy As New CheckedListBox()
    Private ReadOnly lblWarstwy As New Label()
    Private ReadOnly lblInfo As New Label()
    Private ReadOnly btnAuto As New Button()
    Private ReadOnly btnPomoc As New Button()
    Private ReadOnly btnOk As New Button()
    Private ReadOnly btnAnuluj As New Button()
    Private _wypelnianie As Boolean

    ''' <summary>Plan zatwierdzony przyciskiem OK (tylko zaznaczone skale, od najdokładniejszej).</summary>
    Public ReadOnly Property Wynik As List(Of PlanStopnia) = New List(Of PlanStopnia)()

    Public Sub New(szablon As SzablonMapy, planBiezacy As List(Of PlanStopnia), planAuto As List(Of PlanStopnia), szerokoscGeo As Double)
        _szablon = szablon
        _planAuto = planAuto
        _szerokoscGeo = szerokoscGeo
        Text = "Ustawienia skal mapy wieloskalowej - " & szablon.Nazwa
        Font = New Font("Segoe UI", 9.0!)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(640, 470)
        UtworzKontrolki()
        Wczytaj(planBiezacy)
    End Sub

    Private Sub UtworzKontrolki()
        lblInfo.Location = New Point(12, 10)
        lblInfo.Size = New Size(616, 48)
        lblInfo.ForeColor = Color.FromArgb(60, 70, 90)
        lblInfo.Text = "Każda skala to osobno pobrana mapa, wyświetlana na swoich poziomach powiększenia (zoom). " &
                       "Większy zoom to zbliżenie. Zakresy skal nie mogą się nakładać; poziomy bez skali będą w pliku puste."

        dgvSkale.Location = New Point(12, 62)
        dgvSkale.Size = New Size(616, 160)
        dgvSkale.AllowUserToAddRows = False
        dgvSkale.AllowUserToDeleteRows = False
        dgvSkale.AllowUserToResizeRows = False
        dgvSkale.RowHeadersVisible = False
        dgvSkale.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvSkale.MultiSelect = False
        dgvSkale.BackgroundColor = Color.White
        dgvSkale.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvSkale.Columns.Add(New DataGridViewCheckBoxColumn With {.Name = "Uzyj", .HeaderText = "Użyj", .FillWeight = 40})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Skala", .HeaderText = "Skala", .ReadOnly = True, .FillWeight = 110})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Piksel", .HeaderText = "Piksel [m]", .FillWeight = 70})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "ZoomOd", .HeaderText = "Zoom od", .FillWeight = 60})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "ZoomDo", .HeaderText = "Zoom do", .FillWeight = 60})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Warstwy", .HeaderText = "Warstwy", .ReadOnly = True, .FillWeight = 70})
        AddHandler dgvSkale.SelectionChanged, Sub() PokazWarstwy()
        AddHandler dgvSkale.CurrentCellDirtyStateChanged, Sub()
                                                               If dgvSkale.IsCurrentCellDirty AndAlso TypeOf dgvSkale.CurrentCell Is DataGridViewCheckBoxCell Then
                                                                   dgvSkale.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                                               End If
                                                           End Sub

        lblWarstwy.Location = New Point(12, 230)
        lblWarstwy.Size = New Size(616, 18)
        lblWarstwy.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)

        clbWarstwy.Location = New Point(12, 250)
        clbWarstwy.Size = New Size(616, 160)
        clbWarstwy.CheckOnClick = True
        AddHandler clbWarstwy.ItemCheck, AddressOf clbWarstwy_ItemCheck

        btnAuto.Text = "Przywróć automatyczne"
        btnAuto.Location = New Point(12, 428)
        btnAuto.Size = New Size(170, 30)
        AddHandler btnAuto.Click, Sub() Wczytaj(_planAuto)

        btnPomoc.Text = "Pomoc"
        btnPomoc.Location = New Point(190, 428)
        btnPomoc.Size = New Size(80, 30)
        AddHandler btnPomoc.Click, Sub() FormPomocMbtiles.Pokaz(Me, TematPomocy.Kreator)

        btnOk.Text = "OK"
        btnOk.Location = New Point(442, 428)
        btnOk.Size = New Size(90, 30)
        AddHandler btnOk.Click, AddressOf btnOk_Click

        btnAnuluj.Text = "Anuluj"
        btnAnuluj.Location = New Point(538, 428)
        btnAnuluj.Size = New Size(90, 30)
        btnAnuluj.DialogResult = DialogResult.Cancel

        Controls.AddRange({lblInfo, dgvSkale, lblWarstwy, clbWarstwy, btnAuto, btnPomoc, btnOk, btnAnuluj})
        KeyPreview = True
        AddHandler KeyDown, Sub(s As Object, e As KeyEventArgs)
                                If e.KeyCode = Keys.F1 AndAlso e.Modifiers = Keys.None AndAlso Not e.Handled Then
                                    e.Handled = True
                                    FormPomocMbtiles.Pokaz(Me, TematPomocy.Kreator)
                                End If
                            End Sub
        AcceptButton = btnOk
        CancelButton = btnAnuluj
    End Sub

    ''' <summary>Wypełnia tabelę: skale z planu są zaznaczone, pozostałe stopnie szablonu mają piksel i zakres naturalny.</summary>
    Private Sub Wczytaj(plan As List(Of PlanStopnia))
        _wypelnianie = True
        _wiersze.Clear()
        _uzyte.Clear()
        dgvSkale.Rows.Clear()
        Dim cos = Math.Max(0.1, Math.Cos(_szerokoscGeo * Math.PI / 180.0))
        For Each st In _szablon.StopnieSkali
            Dim zPlanu = plan?.FirstOrDefault(Function(ps) ps.Stopien Is st)
            Dim w As PlanStopnia
            Dim opcjonalnyNadPlanem = If(st.Opcjonalny AndAlso zPlanu Is Nothing, ZarzadcaSzablonow.PlanStopniaOpcjonalnego(_szablon, plan, _szerokoscGeo), Nothing)
            If zPlanu IsNot Nothing Then
                w = zPlanu.Kopia()
            ElseIf opcjonalnyNadPlanem IsNot Nothing Then
                'ortofotomapa zaraz nad najdokładniejszą skalą planu - bez pustego poziomu między nimi
                w = opcjonalnyNadPlanem
            Else
                Dim zNat = Math.Max(5, Math.Min(20, CInt(Math.Round(Math.Log(156543.034 * cos / st.Piksel, 2)))))
                w = New PlanStopnia With {.Stopien = st, .Piksel = st.Piksel, .PoziomMin = zNat, .PoziomMax = zNat}
            End If
            If w.Warstwy Is Nothing Then w.Warstwy = New List(Of WarstwaSzablonu)(st.Warstwy)
            _wiersze.Add(w)
            _uzyte.Add(zPlanu IsNot Nothing)
            dgvSkale.Rows.Add(zPlanu IsNot Nothing, st.Opis, w.Piksel.ToString("0.##", CultureInfo.CurrentCulture),
                              w.PoziomMin, w.PoziomMax, OpisWarstw(w))
        Next
        _wypelnianie = False
        If dgvSkale.Rows.Count > 0 Then dgvSkale.Rows(0).Selected = True
        PokazWarstwy()
    End Sub

    Private Shared Function OpisWarstw(w As PlanStopnia) As String
        Return $"{w.WarstwyEfektywne.Count} z {w.Stopien.Warstwy.Count}"
    End Function

    Private Function WybranyIndeks() As Integer
        If dgvSkale.SelectedRows.Count = 0 Then Return -1
        Return dgvSkale.SelectedRows(0).Index
    End Function

    Private Sub PokazWarstwy()
        Dim i = WybranyIndeks()
        _wypelnianie = True
        clbWarstwy.Items.Clear()
        If i >= 0 AndAlso i < _wiersze.Count Then
            Dim w = _wiersze(i)
            lblWarstwy.Text = "Warstwy skali " & w.Stopien.Opis & ":"
            For Each ws In w.Stopien.Warstwy
                clbWarstwy.Items.Add(ws.Etykieta & If(ws.Krycie < 100, $" ({ws.Krycie}%)", ""), w.WarstwyEfektywne.Contains(ws))
            Next
        Else
            lblWarstwy.Text = ""
        End If
        _wypelnianie = False
    End Sub

    Private Sub clbWarstwy_ItemCheck(sender As Object, e As ItemCheckEventArgs)
        If _wypelnianie Then Exit Sub
        Dim i = WybranyIndeks()
        If i < 0 Then Exit Sub
        Dim w = _wiersze(i)
        Dim ws = w.Stopien.Warstwy(e.Index)
        ' kolejność warstw jak w szablonie (od podkładu w górę)
        Dim wybrane = w.Stopien.Warstwy.Where(Function(x, k) If(k = e.Index, e.NewValue = CheckState.Checked, w.WarstwyEfektywne.Contains(x))).ToList()
        w.Warstwy = wybrane
        dgvSkale.Rows(i).Cells("Warstwy").Value = OpisWarstw(w)
    End Sub

    Private Shared Function CzytajLiczbe(v As Object, ByRef wynik As Double) As Boolean
        Dim t = Convert.ToString(v, CultureInfo.CurrentCulture).Trim().Replace(","c, "."c)
        Return Double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, wynik)
    End Function

    Private Sub btnOk_Click(sender As Object, e As EventArgs)
        dgvSkale.EndEdit()
        Dim plan As New List(Of PlanStopnia)()
        For i = 0 To _wiersze.Count - 1
            Dim r = dgvSkale.Rows(i)
            If Not CBool(If(r.Cells("Uzyj").Value, False)) Then Continue For
            Dim piksel As Double, zOd As Double, zDo As Double
            If Not CzytajLiczbe(r.Cells("Piksel").Value, piksel) OrElse Not CzytajLiczbe(r.Cells("ZoomOd").Value, zOd) OrElse
               Not CzytajLiczbe(r.Cells("ZoomDo").Value, zDo) OrElse
               Not Double.IsFinite(piksel) OrElse Not Double.IsFinite(zOd) OrElse Not Double.IsFinite(zDo) OrElse
               zOd < 0 OrElse zOd > 22 OrElse zDo < 0 OrElse zDo > 22 Then
                MessageBox.Show($"Skala {_wiersze(i).Stopien.Opis}: wpisz poprawne liczby w polach piksela i powiększeń (0–22).", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If
            Dim w = _wiersze(i).Kopia()
            w.Piksel = piksel
            w.PoziomMin = CInt(Math.Round(zOd))
            w.PoziomMax = CInt(Math.Round(zDo))
            plan.Add(w)
        Next
        Dim blad = ZarzadcaSzablonow.SprawdzPlan(plan)
        If blad <> "" Then
            MessageBox.Show(blad, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim luki = ZarzadcaSzablonow.LukiPlanu(plan)
        If luki.Count > 0 AndAlso MessageBox.Show("Poziomy powiększenia " & String.Join(", ", luki) & " nie należą do żadnej skali - w pliku będą puste." & vbCrLf & vbCrLf &
                                                  "Czy zapisać mimo to?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Exit Sub
        End If
        Wynik.Clear()
        Wynik.AddRange(plan.OrderBy(Function(ps) ps.Piksel))
        DialogResult = DialogResult.OK
        Close()
    End Sub

End Class
