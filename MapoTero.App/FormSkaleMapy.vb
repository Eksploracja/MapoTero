'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
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
    Private ReadOnly _zrodlaMap As IDictionary(Of String, PlikWarstw)
    Private ReadOnly _punktProbki As PunktXY?
    ' Jeden wiersz na każdy stopień skali szablonu (także nieużywany)
    Private ReadOnly _wiersze As New List(Of PlanStopnia)()
    Private ReadOnly _uzyte As New List(Of Boolean)()

    Private ReadOnly dgvSkale As New DataGridView()
    Private ReadOnly btnDodajSkale As New Button()
    Private ReadOnly btnUsunSkale As New Button()
    Private ReadOnly btnEdytujWarstwy As New Button()
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

    Public Sub New(szablon As SzablonMapy, planBiezacy As List(Of PlanStopnia), planAuto As List(Of PlanStopnia), szerokoscGeo As Double,
                   Optional zrodlaMap As IDictionary(Of String, PlikWarstw) = Nothing, Optional punktProbki As PunktXY? = Nothing)
        _szablon = szablon
        _planAuto = planAuto
        _szerokoscGeo = szerokoscGeo
        _zrodlaMap = zrodlaMap
        _punktProbki = punktProbki
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
        dgvSkale.Size = New Size(616, 156)
        dgvSkale.AllowUserToAddRows = False
        dgvSkale.AllowUserToDeleteRows = False
        dgvSkale.AllowUserToResizeRows = False
        dgvSkale.RowHeadersVisible = False
        dgvSkale.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvSkale.MultiSelect = False
        dgvSkale.BackgroundColor = Color.White
        dgvSkale.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvSkale.Columns.Add(New DataGridViewCheckBoxColumn With {.Name = "Uzyj", .HeaderText = "Użyj", .FillWeight = 40})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Skala", .HeaderText = "Skala", .ReadOnly = False, .FillWeight = 110})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Piksel", .HeaderText = "Piksel [m]", .FillWeight = 70})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "ZoomOd", .HeaderText = "Zoom od", .FillWeight = 60})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "ZoomDo", .HeaderText = "Zoom do", .FillWeight = 60})
        dgvSkale.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Warstwy", .HeaderText = "Wł. warstwy", .ReadOnly = True, .FillWeight = 75})
        AddHandler dgvSkale.SelectionChanged, Sub()
                                                  PokazWarstwy()
                                                  AktualizujPrzyciskiSkal()
                                              End Sub
        AddHandler dgvSkale.CellEndEdit, AddressOf dgvSkale_CellEndEdit
        AddHandler dgvSkale.CurrentCellDirtyStateChanged, Sub()
                                                               If dgvSkale.IsCurrentCellDirty AndAlso TypeOf dgvSkale.CurrentCell Is DataGridViewCheckBoxCell Then
                                                                   dgvSkale.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                                               End If
                                                           End Sub

        btnDodajSkale.Text = "Dodaj skalę"
        btnDodajSkale.Location = New Point(12, 224)
        btnDodajSkale.Size = New Size(110, 27)
        AddHandler btnDodajSkale.Click, AddressOf btnDodajSkale_Click

        btnUsunSkale.Text = "Usuń skalę"
        btnUsunSkale.Location = New Point(128, 224)
        btnUsunSkale.Size = New Size(105, 27)
        AddHandler btnUsunSkale.Click, AddressOf btnUsunSkale_Click

        btnEdytujWarstwy.Text = "Edytuj warstwy skali..."
        btnEdytujWarstwy.Location = New Point(239, 224)
        btnEdytujWarstwy.Size = New Size(165, 27)
        AddHandler btnEdytujWarstwy.Click, AddressOf btnEdytujWarstwy_Click

        lblWarstwy.Location = New Point(12, 258)
        lblWarstwy.Size = New Size(616, 18)
        lblWarstwy.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)

        clbWarstwy.Location = New Point(12, 278)
        clbWarstwy.Size = New Size(616, 138)
        clbWarstwy.CheckOnClick = True
        AddHandler clbWarstwy.ItemCheck, AddressOf clbWarstwy_ItemCheck

        btnAuto.Text = "Dobierz automatycznie"
        btnAuto.Location = New Point(12, 428)
        btnAuto.Size = New Size(170, 30)
        AddHandler btnAuto.Click, AddressOf btnAuto_Click

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

        Controls.AddRange({lblInfo, dgvSkale, btnDodajSkale, btnUsunSkale, btnEdytujWarstwy, lblWarstwy, clbWarstwy, btnAuto, btnPomoc, btnOk, btnAnuluj})
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

    Private Shared Function KlonujWarstwe(ws As WarstwaSzablonu) As WarstwaSzablonu
        Return New WarstwaSzablonu(ws.Zbior, ws.Warstwa, ws.Krycie, ws.Nakladka, ws.TrybMieszania, ws.NazwaWyswietlana) With {
            .Regulacja = ws.Regulacja, .Grupa = ws.Grupa, .Adres = ws.Adres, .TypZrodla = ws.TypZrodla}
    End Function

    Private Shared Function KlonujStopien(st As StopienSkali, Optional warstwyNadpisane As IList(Of WarstwaSzablonu) = Nothing) As StopienSkali
        Dim klon As New StopienSkali With {
            .Opis = If(st?.Opis, "podstawowa"),
            .Piksel = If(st IsNot Nothing AndAlso st.Piksel > 0, st.Piksel, 1.0),
            .Opcjonalny = If(st?.Opcjonalny, False)
        }
        Dim zrodlo = If(warstwyNadpisane, CType(st?.Warstwy, IList(Of WarstwaSzablonu)))
        If zrodlo IsNot Nothing Then
            For Each ws In zrodlo
                klon.Warstwy.Add(KlonujWarstwe(ws))
            Next
        End If
        Return klon
    End Function

    ''' <summary>Wypełnia tabelę: skale z planu są zaznaczone, pozostałe stopnie szablonu mają piksel i zakres naturalny.</summary>
    Private Sub Wczytaj(plan As List(Of PlanStopnia))
        _wypelnianie = True
        _wiersze.Clear()
        _uzyte.Clear()
        dgvSkale.Rows.Clear()
        Dim cos = Math.Max(0.1, Math.Cos(_szerokoscGeo * Math.PI / 180.0))
        Dim dodaneSzablonu As New HashSet(Of StopienSkali)()

        If plan IsNot Nothing Then
            For Each ps In plan
                Dim stOrig = ps.Stopien
                If stOrig IsNot Nothing Then dodaneSzablonu.Add(stOrig)
                Dim klonStopnia = KlonujStopien(stOrig, If(ps.Warstwy IsNot Nothing AndAlso ps.Warstwy.Count > 0, ps.Warstwy, Nothing))
                If stOrig IsNot Nothing AndAlso ps.Warstwy IsNot Nothing AndAlso ps.Warstwy.Count < stOrig.Warstwy.Count Then
                    ' Zachowaj pełną listę warstw stopnia w klonie, aby użytkownik mógł je ponownie włączyć w clbWarstwy
                    klonStopnia = KlonujStopien(stOrig)
                    Dim wybrane = klonStopnia.Warstwy.Where(Function(x) ps.Warstwy.Any(Function(y) String.Equals(y.Zbior, x.Zbior, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(y.Warstwa, x.Warstwa, StringComparison.OrdinalIgnoreCase))).ToList()
                    Dim w = New PlanStopnia With {.Stopien = klonStopnia, .Piksel = ps.Piksel, .PoziomMin = ps.PoziomMin, .PoziomMax = ps.PoziomMax, .Warstwy = wybrane}
                    DodajWierszDoTabeli(w, True)
                Else
                    Dim w = New PlanStopnia With {.Stopien = klonStopnia, .Piksel = ps.Piksel, .PoziomMin = ps.PoziomMin, .PoziomMax = ps.PoziomMax, .Warstwy = New List(Of WarstwaSzablonu)(klonStopnia.Warstwy)}
                    DodajWierszDoTabeli(w, True)
                End If
            Next
        End If

        For Each st In _szablon.StopnieSkali
            If dodaneSzablonu.Contains(st) Then Continue For
            If plan IsNot Nothing AndAlso plan.Any(Function(ps) ps.Stopien IsNot Nothing AndAlso String.Equals(ps.Stopien.Opis, st.Opis, StringComparison.OrdinalIgnoreCase)) Then Continue For
            Dim opcjonalnyNadPlanem = If(st.Opcjonalny, ZarzadcaSzablonow.PlanStopniaOpcjonalnego(_szablon, plan, _szerokoscGeo), Nothing)
            Dim w As PlanStopnia
            Dim klonStopnia = KlonujStopien(st)
            If opcjonalnyNadPlanem IsNot Nothing Then
                w = New PlanStopnia With {.Stopien = klonStopnia, .Piksel = opcjonalnyNadPlanem.Piksel, .PoziomMin = opcjonalnyNadPlanem.PoziomMin, .PoziomMax = opcjonalnyNadPlanem.PoziomMax, .Warstwy = New List(Of WarstwaSzablonu)(klonStopnia.Warstwy)}
            Else
                Dim zNat = Math.Max(5, Math.Min(20, CInt(Math.Round(Math.Log(156543.034 * cos / st.Piksel, 2)))))
                w = New PlanStopnia With {.Stopien = klonStopnia, .Piksel = st.Piksel, .PoziomMin = zNat, .PoziomMax = zNat, .Warstwy = New List(Of WarstwaSzablonu)(klonStopnia.Warstwy)}
            End If
            DodajWierszDoTabeli(w, False)
        Next

        _wypelnianie = False
        If dgvSkale.Rows.Count > 0 Then dgvSkale.Rows(0).Selected = True
        PokazWarstwy()
        AktualizujPrzyciskiSkal()
    End Sub

    Private Sub DodajWierszDoTabeli(w As PlanStopnia, uzyj As Boolean)
        _wiersze.Add(w)
        _uzyte.Add(uzyj)
        Dim idx = dgvSkale.Rows.Add(uzyj, w.Stopien.Opis, w.Piksel.ToString("0.##", CultureInfo.CurrentCulture),
                                    w.PoziomMin, w.PoziomMax, OpisWarstw(w))
        dgvSkale.Rows(idx).Cells("Warstwy").ToolTipText = TooltipWarstw(w)
    End Sub

    Private Sub AktualizujPrzyciskiSkal()
        Dim idx = WybranyIndeks()
        btnUsunSkale.Enabled = (_wiersze.Count > 1 AndAlso idx >= 0)
        btnEdytujWarstwy.Enabled = (idx >= 0)
    End Sub

    Private Sub dgvSkale_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs)
        If _wypelnianie OrElse e.RowIndex < 0 OrElse e.RowIndex >= _wiersze.Count Then Exit Sub
        Dim r = dgvSkale.Rows(e.RowIndex)
        Dim w = _wiersze(e.RowIndex)
        Dim nazwa = Convert.ToString(r.Cells("Skala").Value, CultureInfo.CurrentCulture)?.Trim()
        If Not String.IsNullOrWhiteSpace(nazwa) Then
            w.Stopien.Opis = nazwa
            If e.RowIndex = WybranyIndeks() Then lblWarstwy.Text = "Warstwy skali " & w.Stopien.Opis & ":"
        End If
        Dim px As Double
        If CzytajLiczbe(r.Cells("Piksel").Value, px) AndAlso Double.IsFinite(px) AndAlso px > 0 Then
            w.Piksel = px
            w.Stopien.Piksel = px
        End If
    End Sub

    Private Sub btnDodajSkale_Click(sender As Object, e As EventArgs)
        dgvSkale.EndEdit()
        Dim wzor = If(WybranyIndeks() >= 0, _wiersze(WybranyIndeks()), _wiersze.LastOrDefault())
        Dim maksPx = If(_wiersze.Count > 0, _wiersze.Max(Function(x) x.Piksel), 1.0)
        Dim nowyPiksel As Double
        Dim nowyOpis As String
        If _wiersze.Count = 0 Then
            nowyPiksel = 2.0
            nowyOpis = "szczegółowa"
        ElseIf maksPx <= 4.0 Then
            nowyPiksel = 10.0
            nowyOpis = "średnia (1:50 000)"
        ElseIf maksPx <= 12.0 Then
            nowyPiksel = 25.0
            nowyOpis = "przeglądowa (1:100 000)"
        Else
            nowyPiksel = Math.Round(maksPx * 2.5)
            nowyOpis = "przeglądowa"
        End If

        Dim nowyStopien = KlonujStopien(wzor?.Stopien, wzor?.WarstwyEfektywne)
        nowyStopien.Opis = nowyOpis
        nowyStopien.Piksel = nowyPiksel
        nowyStopien.Opcjonalny = False

        Dim zakres = ZarzadcaSzablonow.ZalecanyZakresPowiekszen(nowyPiksel, _szerokoscGeo)
        Dim nowyPlan As New PlanStopnia With {
            .Stopien = nowyStopien,
            .Piksel = nowyPiksel,
            .PoziomMin = zakres.Item1,
            .PoziomMax = zakres.Item2,
            .Warstwy = New List(Of WarstwaSzablonu)(nowyStopien.Warstwy)
        }

        _wypelnianie = True
        DodajWierszDoTabeli(nowyPlan, True)
        _wypelnianie = False
        PrzeliczZoomyAutomatycznie()
        dgvSkale.Rows(dgvSkale.Rows.Count - 1).Selected = True
        AktualizujPrzyciskiSkal()
    End Sub

    Private Sub btnUsunSkale_Click(sender As Object, e As EventArgs)
        Dim idx = WybranyIndeks()
        If _wiersze.Count <= 1 OrElse idx < 0 Then Exit Sub
        _wypelnianie = True
        _wiersze.RemoveAt(idx)
        _uzyte.RemoveAt(idx)
        dgvSkale.Rows.RemoveAt(idx)
        _wypelnianie = False
        If dgvSkale.Rows.Count > 0 Then
            dgvSkale.Rows(Math.Min(idx, dgvSkale.Rows.Count - 1)).Selected = True
        End If
        PokazWarstwy()
        AktualizujPrzyciskiSkal()
    End Sub

    Private Sub btnAuto_Click(sender As Object, e As EventArgs)
        If _planAuto IsNot Nothing AndAlso _planAuto.Count > 0 AndAlso _szablon.Wieloskalowy Then
            Wczytaj(_planAuto)
        Else
            PrzeliczZoomyAutomatycznie()
        End If
    End Sub

    Private Sub PrzeliczZoomyAutomatycznie()
        dgvSkale.EndEdit()
        Dim aktywne As New List(Of (Idx As Integer, Plan As PlanStopnia))()
        For i = 0 To _wiersze.Count - 1
            If CBool(If(dgvSkale.Rows(i).Cells("Uzyj").Value, False)) Then
                Dim px As Double
                If CzytajLiczbe(dgvSkale.Rows(i).Cells("Piksel").Value, px) AndAlso Double.IsFinite(px) AndAlso px > 0 Then
                    _wiersze(i).Piksel = px
                End If
                aktywne.Add((i, _wiersze(i)))
            End If
        Next
        If aktywne.Count = 0 Then Exit Sub

        Dim posortowane = aktywne.OrderBy(Function(x) x.Plan.Piksel).ToList()
        Dim cos = Math.Max(0.1, Math.Cos(_szerokoscGeo * Math.PI / 180.0))
        Dim naturalny = Function(p As Double) CInt(Math.Round(Math.Log(156543.034 * cos / p, 2)))
        Dim zGora = Math.Max(6, Math.Min(20, naturalny(posortowane(0).Plan.Piksel)))
        Dim zDol = Math.Max(5, Math.Min(zGora, naturalny(posortowane.Last().Plan.Piksel) - 3))
        For Each item In posortowane
            item.Plan.PoziomMin = Integer.MaxValue
            item.Plan.PoziomMax = Integer.MinValue
        Next
        For z = zDol To zGora
            Dim res = ZarzadcaSzablonow.PikselPoziomu(z, _szerokoscGeo)
            Dim najblizszy = posortowane.OrderBy(Function(x) Math.Abs(Math.Log(res / Math.Max(0.001, x.Plan.Piksel)))).First().Plan
            najblizszy.PoziomMin = Math.Min(najblizszy.PoziomMin, z)
            najblizszy.PoziomMax = Math.Max(najblizszy.PoziomMax, z)
        Next
        ' Jeśli dwa wiersze miały zbliżony piksel i jeden nie dostał żadnego poziomu, daj mu jego poziom naturalny
        For Each item In posortowane
            If item.Plan.PoziomMin > item.Plan.PoziomMax Then
                Dim zNat = Math.Max(5, Math.Min(20, naturalny(item.Plan.Piksel)))
                item.Plan.PoziomMin = zNat
                item.Plan.PoziomMax = zNat
            End If
            dgvSkale.Rows(item.Idx).Cells("ZoomOd").Value = item.Plan.PoziomMin
            dgvSkale.Rows(item.Idx).Cells("ZoomDo").Value = item.Plan.PoziomMax
        Next
    End Sub

    Private Sub btnEdytujWarstwy_Click(sender As Object, e As EventArgs)
        Dim idx = WybranyIndeks()
        If idx < 0 Then Exit Sub
        dgvSkale.EndEdit()
        Dim w = _wiersze(idx)
        Dim px As Double = w.Piksel
        CzytajLiczbe(dgvSkale.Rows(idx).Cells("Piksel").Value, px)
        If px <= 0 OrElse Not Double.IsFinite(px) Then px = 1.0

        Dim zrodla As IDictionary(Of String, PlikWarstw) = _zrodlaMap
        If zrodla Is Nothing Then
            zrodla = New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
        End If
        Dim zestaw = _szablon.UtworzZestaw(zrodla, w.WarstwyEfektywne)
        Dim przed = zestaw.Kopia()

        Dim punkt = If(_punktProbki.HasValue, _punktProbki.Value, UkladWspolrzednych.PL1992.ZWgs84(_szablon.ProbkaLat, _szablon.ProbkaLon))
        Dim pol = PodgladWarstwSilnik.BokProbki * px / 2.0
        Dim zasiegProbki As New Zasieg(punkt.X - pol, punkt.Y - pol, punkt.X + pol, punkt.Y + pol)
        Dim siatkaProbki As New Siatka(zasiegProbki, px, PodgladWarstwSilnik.BokProbki)

        Dim zadanie As New ZadaniePobierania With {
            .Uklad = UkladWspolrzednych.PL1992,
            .Siatka = siatkaProbki,
            .Zbiory = zestaw.Zbiory(),
            .ZrodlaMap = New Dictionary(Of String, PlikWarstw)(zrodla, StringComparer.OrdinalIgnoreCase),
            .Format = "png",
            .LimitCzasuSekundy = 15,
            .Folder = Path.Combine(Path.GetTempPath(), "MapoTero_kreator_skale") & "\"
        }

        Using silnik As New PodgladWarstwSilnik()
            silnik.SprawdzIZaktualizujParametry(punkt, px, UkladWspolrzednych.PL1992, wymus:=True)
            Using okno As New FormWarstwyZaawansowane(zestaw, zadanie, 0, silnik, New OpcjeKorekcjiMip With {.Wlaczona = False})
                okno.Text &= " - skala " & w.Stopien.Opis
                If okno.ShowDialog(Me) = DialogResult.OK Then
                    Dim noweWarstwy = okno.Zestaw.Warstwy.Select(
                        Function(wz) WarstwaSzablonu.ZWarstwyWybranej(wz, w.Stopien.Warstwy.FirstOrDefault(
                            Function(x) String.Equals(x.Zbior, wz.Zbior, StringComparison.OrdinalIgnoreCase) AndAlso
                                        String.Equals(x.Warstwa, wz.Nazwa, StringComparison.OrdinalIgnoreCase)))).ToList()
                    w.Stopien.Warstwy.Clear()
                    w.Stopien.Warstwy.AddRange(noweWarstwy)
                    w.Warstwy = New List(Of WarstwaSzablonu)(noweWarstwy)
                    dgvSkale.Rows(idx).Cells("Warstwy").Value = OpisWarstw(w)
                    dgvSkale.Rows(idx).Cells("Warstwy").ToolTipText = TooltipWarstw(w)
                    PokazWarstwy()
                End If
            End Using
        End Using
    End Sub

    Private Shared Function OpisWarstw(w As PlanStopnia) As String
        Return $"{w.WarstwyEfektywne.Count} / {w.Stopien.Warstwy.Count} wł."
    End Function

    Private Shared Function TooltipWarstw(w As PlanStopnia) As String
        Dim aktywne = w.WarstwyEfektywne
        If aktywne.Count = 0 Then Return "Brak włączonych warstw"
        Return "Włączone: " & String.Join(", ", aktywne.Select(Function(ws) ws.Etykieta))
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
        dgvSkale.Rows(i).Cells("Warstwy").ToolTipText = TooltipWarstw(w)
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
            Dim opisSkali = Convert.ToString(r.Cells("Skala").Value, CultureInfo.CurrentCulture)?.Trim()
            If String.IsNullOrWhiteSpace(opisSkali) Then opisSkali = "Skala " & (i + 1)
            w.Stopien.Opis = opisSkali
            w.Stopien.Piksel = piksel
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
