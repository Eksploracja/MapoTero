'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System.ComponentModel
Imports System.Windows.Forms
Imports MapoTero.Core

Public Class FormSzukajDzialki

    Private Shared _ostatniaZakladka As Integer = 0
    Private ReadOnly _poczatkowyTryb As Integer = -1
    Private ReadOnly _srodekLat As Double?
    Private ReadOnly _srodekLon As Double?
    Private WithEvents _timerPodpowiedzi As New Timer With {.Interval = 350}
    Private _ctsBiezace As Threading.CancellationTokenSource

    Friend Shared Property OstatniaZakladka As Integer
        Get
            Return _ostatniaZakladka
        End Get
        Set(value As Integer)
            _ostatniaZakladka = value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WybranaDzialka As DzialkaKatastralna

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WybraneMiejsce As WynikGeokodowania

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property MarginesMetry As Double = 50.0

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WlaczWarstwy As Boolean = True

    ''' <summary>Fraza wpisana w pole wyszukiwania okna głównego - po otwarciu okna od razu wyszukiwana.</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PoczatkowaFraza As String = ""

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If String.IsNullOrWhiteSpace(PoczatkowaFraza) Then Return
        If tabSzukaj.SelectedIndex = 0 Then
            txtZapytanieMiejscowosc.Text = PoczatkowaFraza
            btnSzukajMiejscowosc.PerformClick()
        Else
            txtZapytanie.Text = PoczatkowaFraza
            btnSzukaj.PerformClick()
        End If
    End Sub

    ''' <summary>Przerwanie trwającego wyszukiwania przy zamknięciu okna.</summary>
    Private ReadOnly _przerwanie As New Threading.CancellationTokenSource()

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Zamknięcie okna przerywa wyszukiwanie - dawniej zapytanie trwało dalej (do 30 s), a jego błąd pokazywał się
    ''' w komunikacie już po zamknięciu okna.
    ''' </summary>
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _timerPodpowiedzi.Stop()
        _timerPodpowiedzi.Dispose()
        _ctsBiezace?.Cancel()
        _ctsBiezace?.Dispose()
        _ctsBiezace = Nothing
        _przerwanie.Cancel()
        _przerwanie.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

    Public Sub New(poczatkowyTryb As Integer, Optional srodekLat As Double? = Nothing, Optional srodekLon As Double? = Nothing)
        InitializeComponent()
        _poczatkowyTryb = poczatkowyTryb
        _srodekLat = srodekLat
        _srodekLon = srodekLon
    End Sub

    Private Sub FormSzukajDzialki_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.Szukaj, 12, 384)

        Dim tryb = If(_poczatkowyTryb >= 0, _poczatkowyTryb, _ostatniaZakladka)
        If tryb >= 0 AndAlso tryb < tabSzukaj.TabCount Then
            tabSzukaj.SelectedIndex = tryb
        End If
        AktualizujStanPrzyciskow()
    End Sub

    Private Sub tabSzukaj_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tabSzukaj.SelectedIndexChanged
        _ostatniaZakladka = tabSzukaj.SelectedIndex
        AktualizujStanPrzyciskow()
    End Sub

    Private Sub AktualizujStanPrzyciskow()
        If tabSzukaj.SelectedIndex = 0 Then
            Me.AcceptButton = btnSzukajMiejscowosc
            btnWybierz.Text = "Pokaż na mapie"
            btnWybierz.Enabled = (lvwMiejscowosci.SelectedItems.Count > 0)
        Else
            Me.AcceptButton = btnSzukaj
            btnWybierz.Text = "Zaznacz na mapie"
            btnWybierz.Enabled = (lvwWyniki.SelectedItems.Count > 0)
        End If
    End Sub

#Region "Wyszukiwanie miejscowości i adresów (Photon + Nominatim)"

    Private Sub txtZapytanieMiejscowosc_TextChanged(sender As Object, e As EventArgs) Handles txtZapytanieMiejscowosc.TextChanged
        If tabSzukaj.SelectedIndex <> 0 Then Exit Sub
        _timerPodpowiedzi.Stop()
        Dim fraza = txtZapytanieMiejscowosc.Text.Trim()
        If fraza.Length >= 3 Then
            _timerPodpowiedzi.Start()
        ElseIf fraza.Length = 0 Then
            _ctsBiezace?.Cancel()
            lvwMiejscowosci.Items.Clear()
            lblStatusMiejscowosc.Text = "Wpisz nazwę miejscowości, ulicę lub adres..."
            AktualizujStanPrzyciskow()
        End If
    End Sub

    Private Sub _timerPodpowiedzi_Tick(sender As Object, e As EventArgs) Handles _timerPodpowiedzi.Tick
        _timerPodpowiedzi.Stop()
        Dim fraza = txtZapytanieMiejscowosc.Text.Trim()
        If fraza.Length >= 3 Then
            WyszukajMiejscowosciAsync(fraza, automatyczne:=True)
        End If
    End Sub

    Private Sub txtZapytanieMiejscowosc_KeyDown(sender As Object, e As KeyEventArgs) Handles txtZapytanieMiejscowosc.KeyDown
        If e.KeyCode = Keys.Down AndAlso lvwMiejscowosci.Items.Count > 0 Then
            lvwMiejscowosci.Focus()
            If lvwMiejscowosci.SelectedIndices.Count = 0 Then
                lvwMiejscowosci.Items(0).Selected = True
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub lvwMiejscowosci_KeyDown(sender As Object, e As KeyEventArgs) Handles lvwMiejscowosci.KeyDown
        If e.KeyCode = Keys.Enter AndAlso btnWybierz.Enabled Then
            btnWybierz.PerformClick()
            e.Handled = True
        End If
    End Sub

    Private Sub btnSzukajMiejscowosc_Click(sender As Object, e As EventArgs) Handles btnSzukajMiejscowosc.Click
        _timerPodpowiedzi.Stop()
        Dim fraza = txtZapytanieMiejscowosc.Text.Trim()
        If String.IsNullOrEmpty(fraza) Then
            MessageBox.Show("Wpisz nazwę miejscowości, wsi, ulicę lub adres.",
                            "Wyszukiwarka miejscowości", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        WyszukajMiejscowosciAsync(fraza, automatyczne:=False)
    End Sub

    Private Async Sub WyszukajMiejscowosciAsync(fraza As String, automatyczne As Boolean)
        _ctsBiezace?.Cancel()
        _ctsBiezace?.Dispose()
        Dim cts = Threading.CancellationTokenSource.CreateLinkedTokenSource(_przerwanie.Token)
        _ctsBiezace = cts
        Dim token = cts.Token

        If Not automatyczne Then
            btnSzukajMiejscowosc.Enabled = False
        End If
        prgSzukajMiejscowosc.Visible = True
        lblStatusMiejscowosc.Text = If(automatyczne, "Szukanie podpowiedzi...", "Trwa wyszukiwanie miejscowości / adresu...")

        Try
            Dim wyniki = Await KlientGeokodowania.SzukajMiejscaAsync(fraza, token, _srodekLat, _srodekLon)
            If IsDisposed OrElse token.IsCancellationRequested Then Exit Sub

            lvwMiejscowosci.BeginUpdate()
            Try
                lvwMiejscowosci.Items.Clear()
                If wyniki.Count = 0 Then
                    lblStatusMiejscowosc.Text = If(automatyczne, "Brak podpowiedzi dla wpisanej frazy.", "Nie znaleziono pasujących miejscowości ani adresów.")
                Else
                    lblStatusMiejscowosc.Text = $"Znaleziono wyników: {wyniki.Count}"
                    For Each m In wyniki
                        Dim item As New ListViewItem(m.PelnaNazwa)
                        item.SubItems.Add(m.TypOpis)
                        item.SubItems.Add(m.WspolrzedneTekst)
                        item.Tag = m
                        lvwMiejscowosci.Items.Add(item)
                    Next
                    If lvwMiejscowosci.Items.Count > 0 Then
                        lvwMiejscowosci.Items(0).Selected = True
                    End If
                End If
            Finally
                lvwMiejscowosci.EndUpdate()
            End Try
        Catch ex As OperationCanceledException When token.IsCancellationRequested
            ' Użytkownik dopisał kolejną literę lub zamknięto okno
        Catch ex As Exception When Not IsDisposed AndAlso Not token.IsCancellationRequested
            If Not automatyczne Then
                MessageBox.Show("Wystąpił błąd podczas łączenia z geokoderem: " & OpisBledu.Z(ex),
                                "Błąd wyszukiwania", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            lblStatusMiejscowosc.Text = "Błąd wyszukiwania."
        Finally
            If _ctsBiezace Is cts Then _ctsBiezace = Nothing
            cts.Dispose()
            If Not IsDisposed AndAlso _ctsBiezace Is Nothing Then
                btnSzukajMiejscowosc.Enabled = True
                prgSzukajMiejscowosc.Visible = False
                AktualizujStanPrzyciskow()
            End If
        End Try
    End Sub

    Private Sub lvwMiejscowosci_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvwMiejscowosci.SelectedIndexChanged
        If tabSzukaj.SelectedIndex = 0 Then
            If lvwMiejscowosci.SelectedItems.Count > 0 Then
                WybraneMiejsce = TryCast(lvwMiejscowosci.SelectedItems(0).Tag, WynikGeokodowania)
                btnWybierz.Enabled = (WybraneMiejsce IsNot Nothing)
            Else
                WybraneMiejsce = Nothing
                btnWybierz.Enabled = False
            End If
        End If
    End Sub

    Private Sub lvwMiejscowosci_DoubleClick(sender As Object, e As EventArgs) Handles lvwMiejscowosci.DoubleClick
        If btnWybierz.Enabled Then
            btnWybierz.PerformClick()
        End If
    End Sub

#End Region

#Region "Wyszukiwanie działek katastralnych (ULDK)"

    Private Async Sub btnSzukaj_Click(sender As Object, e As EventArgs) Handles btnSzukaj.Click
        Dim fraza = txtZapytanie.Text.Trim()
        If String.IsNullOrEmpty(fraza) Then
            MessageBox.Show("Wpisz identyfikator TERYT lub nazwę obrębu i numer działki.",
                            "Wyszukiwarka działek", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        btnSzukaj.Enabled = False
        prgSzukaj.Visible = True
        lblStatus.Text = "Trwa wyszukiwanie w usłudze ULDK GUGiK..."
        lvwWyniki.Items.Clear()
        btnWybierz.Enabled = False

        Try
            Dim wyniki = Await KlientUldk.SzukajDzialekAsync(fraza, _przerwanie.Token)
            If IsDisposed OrElse _przerwanie.IsCancellationRequested Then Exit Sub
            If wyniki.Count = 0 Then
                lblStatus.Text = "Nie znaleziono działek odpowiadających kryteriom."
            Else
                lblStatus.Text = $"Znaleziono działek: {wyniki.Count}"
                For Each d In wyniki
                    Dim item As New ListViewItem(d.Numer)
                    item.SubItems.Add(d.Obreb)
                    item.SubItems.Add(d.Gmina)
                    item.SubItems.Add(d.Identyfikator)
                    item.Tag = d
                    lvwWyniki.Items.Add(item)
                Next
                If lvwWyniki.Items.Count > 0 Then
                    lvwWyniki.Items(0).Selected = True
                    lvwWyniki.Select()
                End If
            End If
        Catch ex As OperationCanceledException When _przerwanie.IsCancellationRequested
        Catch ex As Exception When Not IsDisposed AndAlso Not _przerwanie.IsCancellationRequested
            MessageBox.Show("Wystąpił błąd podczas łączenia z usługą ULDK: " & OpisBledu.Z(ex),
                            "Błąd wyszukiwania", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblStatus.Text = "Błąd wyszukiwania."
        Finally
            If Not IsDisposed Then
                btnSzukaj.Enabled = True
                prgSzukaj.Visible = False
                AktualizujStanPrzyciskow()
            End If
        End Try
    End Sub

    Private Sub lvwWyniki_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvwWyniki.SelectedIndexChanged
        If tabSzukaj.SelectedIndex = 1 Then
            If lvwWyniki.SelectedItems.Count > 0 Then
                WybranaDzialka = TryCast(lvwWyniki.SelectedItems(0).Tag, DzialkaKatastralna)
                btnWybierz.Enabled = (WybranaDzialka IsNot Nothing)
            Else
                WybranaDzialka = Nothing
                btnWybierz.Enabled = False
            End If
        End If
    End Sub

    Private Sub lvwWyniki_DoubleClick(sender As Object, e As EventArgs) Handles lvwWyniki.DoubleClick
        If btnWybierz.Enabled Then
            btnWybierz.PerformClick()
        End If
    End Sub

#End Region

    Private Sub btnWybierz_Click(sender As Object, e As EventArgs) Handles btnWybierz.Click
        If tabSzukaj.SelectedIndex = 0 Then
            If lvwMiejscowosci.SelectedItems.Count = 0 Then Exit Sub
            WybraneMiejsce = TryCast(lvwMiejscowosci.SelectedItems(0).Tag, WynikGeokodowania)
            If WybraneMiejsce Is Nothing Then Exit Sub
            WybranaDzialka = Nothing
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            If lvwWyniki.SelectedItems.Count = 0 Then Exit Sub
            WybranaDzialka = TryCast(lvwWyniki.SelectedItems(0).Tag, DzialkaKatastralna)
            If WybranaDzialka Is Nothing Then Exit Sub
            WybraneMiejsce = Nothing
            MarginesMetry = CDbl(nudMargines.Value)
            WlaczWarstwy = chkWarstwy.Checked
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

End Class
