Option Strict On
Option Explicit On
Option Infer On

Imports System.ComponentModel
Imports System.Globalization
Imports System.Windows.Forms
Imports MapoTero.Core

''' <summary>
''' Okno modalne do precyzyjnego wpisywania współrzędnych zasięgu pobierania z klawiatury.
''' </summary>
Public Class FormZasiegXY

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property XDol As String
        Get
            Return txtXDol.Text.Trim()
        End Get
        Set(value As String)
            txtXDol.Text = value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property XGora As String
        Get
            Return txtXGora.Text.Trim()
        End Get
        Set(value As String)
            txtXGora.Text = value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property YLewy As String
        Get
            Return txtYLewy.Text.Trim()
        End Get
        Set(value As String)
            txtYLewy.Text = value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property YPrawy As String
        Get
            Return txtYPrawy.Text.Trim()
        End Get
        Set(value As String)
            txtYPrawy.Text = value
        End Set
    End Property

    Private _czyGeograficzny As Boolean

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CzyGeograficzny As Boolean
        Get
            Return _czyGeograficzny
        End Get
        Set(value As Boolean)
            _czyGeograficzny = value
            btnZaokraglijKm.Enabled = Not value
            btnZaokraglijKm.Visible = Not value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property NazwaUkladu As String
        Get
            Return lblUkladInfo.Text
        End Get
        Set(value As String)
            lblUkladInfo.Text = "Układ współrzędnych: " & value
        End Set
    End Property

    Private Sub FormZasiegXY_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PomocProgramu.DodajPrzyciskPomocy(Me, TematPomocy.Wspolrzedne, 12, 254)
        btnZaokraglijKm.Enabled = Not _czyGeograficzny
        btnZaokraglijKm.Visible = Not _czyGeograficzny
        txtXGora.SelectAll()
        txtXGora.Focus()
    End Sub

    Private Shared Function SprobujLiczbe(tekst As String, ByRef wynik As Double) As Boolean
        If String.IsNullOrWhiteSpace(tekst) Then Return False
        Dim ok = Double.TryParse(tekst.Trim().Replace(","c, "."c), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, wynik)
        Return ok AndAlso Double.IsFinite(wynik)
    End Function

    ''' <summary>Zaokrągla granice zasięgu na zewnątrz do pełnych kilometrów.</summary>
    Public Sub ZaokraglijDoKm()
        If _czyGeograficzny Then Exit Sub
        Dim xMin As Double, xMax As Double, yMin As Double, yMax As Double
        If Not SprobujLiczbe(txtXDol.Text, xMin) OrElse Not SprobujLiczbe(txtXGora.Text, xMax) OrElse
           Not SprobujLiczbe(txtYLewy.Text, yMin) OrElse Not SprobujLiczbe(txtYPrawy.Text, yMax) Then
            Exit Sub
        End If

        If xMin <> 0 OrElse xMax <> 0 OrElse yMin <> 0 OrElse yMax <> 0 Then
            txtXDol.Text = NarzedziaTekstu.Liczba(Math.Floor(xMin / 1000.0) * 1000.0)
            txtXGora.Text = NarzedziaTekstu.Liczba(Math.Ceiling(xMax / 1000.0) * 1000.0)
            txtYLewy.Text = NarzedziaTekstu.Liczba(Math.Floor(yMin / 1000.0) * 1000.0)
            txtYPrawy.Text = NarzedziaTekstu.Liczba(Math.Ceiling(yMax / 1000.0) * 1000.0)
        End If
    End Sub

    Private Sub btnZaokraglijKm_Click(sender As Object, e As EventArgs) Handles btnZaokraglijKm.Click
        ZaokraglijDoKm()
    End Sub

    Private Sub btnZastosuj_Click(sender As Object, e As EventArgs) Handles btnZastosuj.Click
        Dim xMin As Double, xMax As Double, yMin As Double, yMax As Double
        If Not SprobujLiczbe(txtXDol.Text, xMin) OrElse Not SprobujLiczbe(txtXGora.Text, xMax) OrElse
           Not SprobujLiczbe(txtYLewy.Text, yMin) OrElse Not SprobujLiczbe(txtYPrawy.Text, yMax) Then
            MessageBox.Show(Me, "Wprowadzone współrzędne zasięgu są niepoprawne.",
                            "Współrzędne zasięgu", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim zasieg As New Zasieg(xMin, yMin, xMax, yMax)
        Dim siatka As New Siatka(zasieg, 1.0, 2000)
        If Not siatka.Poprawna Then
            Dim powod As String = siatka.PowodNiepoprawnosci
            MessageBox.Show(Me, If(powod <> "", powod, "Wprowadzone współrzędne zasięgu są niepoprawne."),
                            "Współrzędne zasięgu", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

End Class
