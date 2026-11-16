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

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WybranaDzialka As DzialkaKatastralna

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property MarginesMetry As Double = 50.0

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WlaczWarstwy As Boolean = True

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
            MessageBox.Show("Wystąpił błąd podczas łączenia z usługą ULDK: " & ex.Message,
                            "Błąd wyszukiwania", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblStatus.Text = "Błąd wyszukiwania."
        Finally
            btnSzukaj.Enabled = True
            prgSzukaj.Visible = False
        End Try
    End Sub

    Private Sub lvwWyniki_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvwWyniki.SelectedIndexChanged
        If lvwWyniki.SelectedItems.Count > 0 Then
            WybranaDzialka = TryCast(lvwWyniki.SelectedItems(0).Tag, DzialkaKatastralna)
            btnWybierz.Enabled = (WybranaDzialka IsNot Nothing)
        Else
            WybranaDzialka = Nothing
            btnWybierz.Enabled = False
        End If
    End Sub

    Private Sub lvwWyniki_DoubleClick(sender As Object, e As EventArgs) Handles lvwWyniki.DoubleClick
        If btnWybierz.Enabled Then
            btnWybierz.PerformClick()
        End If
    End Sub

    Private Sub btnWybierz_Click(sender As Object, e As EventArgs) Handles btnWybierz.Click
        If WybranaDzialka Is Nothing Then Exit Sub
        MarginesMetry = CDbl(nudMargines.Value)
        WlaczWarstwy = chkWarstwy.Checked
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

End Class
