Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Linq
Imports System.Windows.Forms
Imports MapoTero.Core

    ''' <summary>
    ''' Okno podglądu wczytanego poligonu z naniesionym obrysem na schematyczną mapę Polski,
    ''' wyborem układu CRS i przyciskiem szybkiej zamiany osi [X, Y] ↔ [Y, X].
    ''' </summary>
    Public Class FormPodgladPoligonu
        Inherits Form

        Private _zestaw As ZestawPoligonow
        Private ReadOnly _konwertujDoWgs84 As Func(Of RodzinaUkladuPoligonu, Punkt2D, Punkt2D)

        Private ReadOnly _cmbUklad As New ComboBox()
        Private ReadOnly _btnZamienOsie As New Button()
        Private ReadOnly _lblInfo As New Label()
        Private ReadOnly _panelMapa As New Panel()
        Private ReadOnly _btnOk As New Button()
        Private ReadOnly _btnAnuluj As New Button()

        Private Shared ReadOnly _konturPolski As Punkt2D() = {
            New Punkt2D(14.22, 53.92), New Punkt2D(15.57, 54.18), New Punkt2D(17.56, 54.76),
            New Punkt2D(18.81, 54.61), New Punkt2D(19.65, 54.45), New Punkt2D(22.79, 54.36),
            New Punkt2D(23.51, 53.95), New Punkt2D(23.95, 52.70), New Punkt2D(23.62, 52.08),
            New Punkt2D(24.15, 50.87), New Punkt2D(22.80, 49.02), New Punkt2D(20.90, 49.40),
            New Punkt2D(19.95, 49.18), New Punkt2D(18.85, 49.52), New Punkt2D(17.65, 50.28),
            New Punkt2D(16.65, 50.15), New Punkt2D(14.82, 50.87), New Punkt2D(14.53, 52.35),
            New Punkt2D(14.12, 52.84), New Punkt2D(14.22, 53.92)
        }

        Public ReadOnly Property ZatwierdzonyZestaw As ZestawPoligonow
            Get
                Return _zestaw
            End Get
        End Property

        Public Sub New(zestaw As ZestawPoligonow, konwertujDoWgs84 As Func(Of RodzinaUkladuPoligonu, Punkt2D, Punkt2D))
            _zestaw = zestaw
            _konwertujDoWgs84 = konwertujDoWgs84

            ZbudujInterfejs()
            OdswiezWidok()
        End Sub

        Private Sub ZbudujInterfejs()
            Me.Text = "Podgląd poligonu i weryfikacja układu współrzędnych"
            Me.AutoScaleMode = AutoScaleMode.Dpi
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.ClientSize = New Size(560, 520)
            Me.Font = New Font("Segoe UI", 9.0F)

            Dim lblUklad As New Label() With {.Text = "Układ współrzędnych pliku:", .Location = New Point(16, 16), .Size = New Size(170, 20)}
            _cmbUklad.SetBounds(190, 12, 200, 24)
            _cmbUklad.DropDownStyle = ComboBoxStyle.DropDownList
            For Each u As RodzinaUkladuPoligonu In [Enum].GetValues(Of RodzinaUkladuPoligonu)()
                _cmbUklad.Items.Add(u)
            Next
            _cmbUklad.SelectedItem = _zestaw.WykrytyUklad
            AddHandler _cmbUklad.SelectedIndexChanged, Sub(s, e)
                                                           If _cmbUklad.SelectedItem IsNot Nothing Then
                                                               _zestaw.WykrytyUklad = DirectCast(_cmbUklad.SelectedItem, RodzinaUkladuPoligonu)
                                                               OdswiezWidok()
                                                           End If
                                                       End Sub

            _btnZamienOsie.Text = "Zamień osie [X, Y] ↔ [Y, X]"
            _btnZamienOsie.SetBounds(398, 11, 146, 26)
            AddHandler _btnZamienOsie.Click, Sub(s, e)
                                                 _zestaw = _zestaw.ZamienOsie()
                                                 OdswiezWidok()
                                             End Sub

            _lblInfo.SetBounds(16, 44, 528, 36)

            _panelMapa.SetBounds(16, 86, 528, 376)
            _panelMapa.BackColor = Color.White
            _panelMapa.BorderStyle = BorderStyle.FixedSingle
            AddHandler _panelMapa.Paint, AddressOf PanelMapa_Paint

            _btnOk.Text = "Użyj tego poligonu"
            _btnOk.DialogResult = DialogResult.OK
            _btnOk.SetBounds(316, 474, 134, 30)

            _btnAnuluj.Text = "Anuluj"
            _btnAnuluj.DialogResult = DialogResult.Cancel
            _btnAnuluj.SetBounds(458, 474, 86, 30)

            Me.AcceptButton = _btnOk
            Me.CancelButton = _btnAnuluj
            Me.Controls.AddRange(New Control() {lblUklad, _cmbUklad, _btnZamienOsie, _lblInfo, _panelMapa, _btnOk, _btnAnuluj})
        End Sub

        Private Sub OdswiezWidok()
            Dim stanOsi As String = If(_zestaw.OsieZamienione, "osie ZAMIENIONE [Y, X]", "osie oryginalne [X, Y]")
            _lblInfo.Text = String.Format("Wierzchołków: {0:N0} | {1} | Stan: {2}", _zestaw.LiczbaWierzcholkow, _zestaw.ZrodloWykryciaUkladu, stanOsi)
            _lblInfo.ForeColor = If(_zestaw.NiejednoznaczneOsie, Color.DarkOrange, Color.DimGray)
            _panelMapa.Invalidate()
        End Sub

        Private Sub PanelMapa_Paint(sender As Object, e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Color.FromArgb(248, 250, 252))

            Const minLon As Double = 13.5
            Const maxLon As Double = 24.8
            Const minLat As Double = 48.7
            Const maxLat As Double = 55.3

            Dim rzutuj As Func(Of Punkt2D, PointF) =
                Function(wgs As Punkt2D) As PointF
                    Dim px As Single = CSng(((wgs.Wschod - minLon) / (maxLon - minLon)) * (_panelMapa.Width - 24) + 12)
                    Dim py As Single = CSng((1.0 - (wgs.Polnoc - minLat) / (maxLat - minLat)) * (_panelMapa.Height - 24) + 12)
                    Return New PointF(px, py)
                End Function

            Dim punktyPolski As PointF() = _konturPolski.Select(Function(p) rzutuj(p)).ToArray()
            Using pedzelPolska As New SolidBrush(Color.FromArgb(234, 241, 232)),
                  pioroPolska As New Pen(Color.FromArgb(140, 165, 140), 1.5F)
                g.FillPolygon(pedzelPolska, punktyPolski)
                g.DrawPolygon(pioroPolska, punktyPolski)
            End Using

            Dim krok As Integer = Math.Max(1, _zestaw.LiczbaWierzcholkow \ 2500)
            Using pedzelPoli As New SolidBrush(Color.FromArgb(95, 220, 45, 45)),
                  pioroPoli As New Pen(Color.FromArgb(210, 25, 25), 2.0F)
                For Each w As Wielokat In _zestaw.Wielokaty
                    Dim wierzch As Punkt2D() = w.ObrysZewnetrzny.Wierzcholki
                    Dim pktEkran As New List(Of PointF)()
                    For i As Integer = 0 To wierzch.Length - 1 Step krok
                        Try
                            Dim wgs As Punkt2D = _konwertujDoWgs84(_zestaw.WykrytyUklad, wierzch(i))
                            pktEkran.Add(rzutuj(wgs))
                        Catch
                        End Try
                    Next

                    If pktEkran.Count >= 3 Then
                        Dim arr As PointF() = pktEkran.ToArray()
                        g.FillPolygon(pedzelPoli, arr)
                        g.DrawPolygon(pioroPoli, arr)
                    End If
                Next
            End Using
        End Sub

        Public Shared Function PokazDlaPliku(rodzic As IWin32Window, sciezkaPliku As String) As ZestawPoligonow
            If Not System.IO.File.Exists(sciezkaPliku) Then Return Nothing
            Dim zestaw As ZestawPoligonow
            Try
                zestaw = ParserPoligonu.WczytajZPliku(sciezkaPliku)
            Catch ex As Exception
                MessageBox.Show(rodzic, "Nie udało się odczytać poligonu: " & OpisBledu.Z(ex), "Błąd odczytu poligonu", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return Nothing
            End Try

            If zestaw.Wielokaty.Count = 0 Then
                MessageBox.Show(rodzic, "Plik " & System.IO.Path.GetFileName(sciezkaPliku) & " nie zawiera poprawnych poligonów.", "Podgląd poligonu", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return Nothing
            End If

            Dim konwerter As Func(Of RodzinaUkladuPoligonu, Punkt2D, Punkt2D) = AddressOf ZestawPoligonow.PunktDoWgs84

            Using dlg As New FormPodgladPoligonu(zestaw, konwerter)
                If dlg.ShowDialog(rodzic) = DialogResult.OK Then
                    Return dlg.ZatwierdzonyZestaw
                End If
            End Using
            Return Nothing
        End Function
    End Class
