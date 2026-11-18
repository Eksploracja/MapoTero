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

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(poczatkowyTryb As Integer)
        InitializeComponent()
        _poczatkowyTryb = poczatkowyTryb
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

#Region "Wyszukiwanie miejscowości i adresów (Nominatim)"

    Private Async Sub btnSzukajMiejscowosc_Click(sender As Object, e As EventArgs) Handles btnSzukajMiejscowosc.Click
        Dim fraza = txtZapytanieMiejscowosc.Text.Trim()
        If String.IsNullOrEmpty(fraza) Then
            MessageBox.Show("Wpisz nazwę miejscowości, wsi, ulicę lub adres.",
                            "Wyszukiwarka", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        btnSzukajMiejscowosc.Enabled = False
        prgSzukajMiejscowosc.Visible = True
        lblStatusMiejscowosc.Text = "Trwa wyszukiwanie miejscowości / adresu..."
        lvwMiejscowosci.Items.Clear()
        btnWybierz.Enabled = False

        Try
            Dim wyniki = Await KlientGeokodowania.SzukajMiejscaAsync(fraza)
            If wyniki.Count = 0 Then
                lblStatusMiejscowosc.Text = "Nie znaleziono pasujących miejscowości ani adresów."
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
                    lvwMiejscowosci.Select()
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Wystąpił błąd podczas łączenia z geokoderem: " & OpisBledu.Z(ex),
                            "Błąd wyszukiwania", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblStatusMiejscowosc.Text = "Błąd wyszukiwania."
        Finally
            btnSzukajMiejscowosc.Enabled = True
            prgSzukajMiejscowosc.Visible = False
            AktualizujStanPrzyciskow()
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
            Dim wyniki = Await KlientUldk.SzukajDzialekAsync(fraza)
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
        Catch ex As Exception
            MessageBox.Show("Wystąpił błąd podczas łączenia z usługą ULDK: " & OpisBledu.Z(ex),
                            "Błąd wyszukiwania", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblStatus.Text = "Błąd wyszukiwania."
        Finally
            btnSzukaj.Enabled = True
            prgSzukaj.Visible = False
            AktualizujStanPrzyciskow()
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
