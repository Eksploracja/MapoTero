'Copyright (C) <2015-2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Mały pływający pasek narzędziowy wyświetlany nad mapą podczas zaznaczania obszaru w kreatorze.
''' </summary>
Public Class FormPasekZaznaczania
    Inherits Form

    Public Event Zatwierdzono As EventHandler
    Public Event Anulowano As EventHandler

    Private ReadOnly _lblInfo As Label
    Private ReadOnly _btnZatwierdz As Button
    Private ReadOnly _btnAnuluj As Button

    Public Sub New()
        Me.New(Nothing)
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            If Form1.TrybOkienBezAktywacji Then Return True
            Return MyBase.ShowWithoutActivation
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp = MyBase.CreateParams
            If Form1.TrybOkienBezAktywacji Then
                cp.ExStyle = cp.ExStyle Or &H8000000 Or &H80 Or &H20
            End If
            Return cp
        End Get
    End Property

    Public Sub New(parent As Form)
        If parent IsNot Nothing Then
            Me.Owner = parent
        End If

        Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
        Me.StartPosition = FormStartPosition.Manual
        Me.TopMost = Not Form1.TrybOkienBezAktywacji
        Me.ShowInTaskbar = False
        If Form1.TrybOkienBezAktywacji Then Me.Opacity = 0
        Me.Text = "Kreator mapy - zaznaczanie obszaru na mapie"
        Me.ClientSize = New Size(640, 52)
        Me.BackColor = Color.FromArgb(245, 245, 248)
        Me.KeyPreview = True

        _lblInfo = New Label With {
            .Text = "Zaznacz obszar: przeciągnij prawym przyciskiem myszy",
            .Location = New Point(12, 16),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(30, 30, 30)}
        Me.Controls.Add(_lblInfo)

        _btnZatwierdz = New Button With {
            .Text = "✓ Gotowe (wróć)",
            .Location = New Point(380, 10),
            .Size = New Size(130, 32),
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 120, 215),
            .ForeColor = Color.White,
            .UseVisualStyleBackColor = False}
        AddHandler _btnZatwierdz.Click, Sub()
                                           RaiseEvent Zatwierdzono(Me, EventArgs.Empty)
                                           Me.Close()
                                       End Sub
        Me.Controls.Add(_btnZatwierdz)

        _btnAnuluj = New Button With {
            .Text = "Anuluj",
            .Location = New Point(520, 10),
            .Size = New Size(95, 32),
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)}
        AddHandler _btnAnuluj.Click, Sub()
                                         RaiseEvent Anulowano(Me, EventArgs.Empty)
                                         Me.Close()
                                     End Sub
        Me.Controls.Add(_btnAnuluj)

        Me.AcceptButton = _btnZatwierdz
        Me.CancelButton = _btnAnuluj

        AddHandler KeyDown, Sub(s, e)
                                If e.KeyCode = Keys.Escape Then
                                    RaiseEvent Anulowano(Me, EventArgs.Empty)
                                    Me.Close()
                                End If
                            End Sub
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If Me.Owner IsNot Nothing Then
        Dim ekran = If(Me.Owner IsNot Nothing, Screen.FromControl(Me.Owner), Screen.PrimaryScreen)
        Dim desiredX = If(Me.Owner IsNot Nothing, Me.Owner.Left + Math.Max(0, (Me.Owner.Width - Me.Width) \ 2), (ekran.WorkingArea.Width - Me.Width) \ 2)
        Dim desiredY = If(Me.Owner IsNot Nothing, Me.Owner.Top + 60, ekran.WorkingArea.Top + 60)
        desiredX = Math.Max(ekran.WorkingArea.Left + 10, Math.Min(desiredX, ekran.WorkingArea.Right - Me.Width - 10))
        desiredY = Math.Max(ekran.WorkingArea.Top + 10, Math.Min(desiredY, ekran.WorkingArea.Bottom - Me.Height - 10))
        Me.Location = New Point(desiredX, desiredY)
            FormKreatorMapy.PrzywrocZMinimalizacji(Me.Owner)
            Me.Owner.Activate()
            Me.Owner.BringToFront()
        Else
            Me.CenterToScreen()
        End If
    End Sub

End Class
