'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.IO
Imports System.Threading
Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers
Imports GMap.NET.WindowsForms.ToolTips
Imports MapoTero.Core

''' <summary>Główne okno programu.</summary>
Public Class Form1

    ''' <summary>Wskazuje, że okno zostało już załadowane (wczytanie sesji z menu pyta wtedy o folder).</summary>
    Private _zaladowany As Boolean

    ''' <summary>Wybrane warstwy mapy (maksymalnie 12).</summary>
    Private ReadOnly _warstwy As New List(Of String)
    Private Const MaksLiczbaWarstw As Integer = 12

    ''' <summary>Bieżący zbiór map (adres serwera i lista warstw).</summary>
    Private _plikWarstw As New PlikWarstw()

    ''' <summary>Przerywanie trwającego pobierania.</summary>
    Private _przerwanie As CancellationTokenSource

    ''' <summary>Programowe ustawianie listy układów (bez przeliczania zasięgu).</summary>
    Private _ustawianieUkladu As Boolean

#Region "Dostęp dla innych okien"

    ''' <summary>Folder segmentów bieżącej sesji (wyświetlany na pasku stanu).</summary>
    Public Property FolderSesji As String
        Get
            Return stFolder.Text
        End Get
        Set(value As String)
            stFolder.Text = value
            Ustawienia.FolderSegmentow = value
        End Set
    End Property

    ''' <summary>Nazwa bieżącego zbioru map.</summary>
    Public ReadOnly Property ZbiorMap As String
        Get
            Return cmbZbiorMap.Text
        End Get
    End Property

    ''' <summary>Nazwa pierwszej wybranej warstwy.</summary>
    Public ReadOnly Property PierwszaWarstwa As String
        Get
            Return If(_warstwy.Count > 0, _warstwy(0), "")
        End Get
    End Property

    ''' <summary>Bok segmentu w pikselach (pole formularza).</summary>
    Public Property BokSegmentu As String
        Get
            Return txtBokSegmentu.Text
        End Get
        Set(value As String)
            txtBokSegmentu.Text = value
        End Set
    End Property

    ''' <summary>Rozmiar piksela (pole formularza).</summary>
    Public Property RozmiarPiksela As String
        Get
            Return txtRozmiarPiksela.Text
        End Get
        Set(value As String)
            txtRozmiarPiksela.Text = value
        End Set
    End Property

    ''' <summary>Wyświetla komunikat w polu komunikatów okna głównego.</summary>
    Public Sub Komunikat(tekst As String, kolor As Color)
        txtKomunikaty.ForeColor = kolor
        txtKomunikaty.Text = tekst
    End Sub

    ''' <summary>Włącza lub wyłącza ręczną edycję współrzędnych zasięgu.</summary>
    Public Sub UstawEdycjeXY(wlaczona As Boolean)
        For Each c As Control In New Control() {txtXDol, txtYLewy, txtXGora, txtYPrawy, lblOpisXDol, lblOpisYLewy, lblOpisXGora, lblOpisYPrawy}
            c.Enabled = wlaczona
        Next
    End Sub

    ''' <summary>Pokazuje lub ukrywa współrzędne WGS84 zaznaczenia.</summary>
    Public Sub UstawWidokZaznaczeniaWgs(widoczne As Boolean)
        For Each c As Control In New Control() {lblZaznSzerGora, lblZaznSzerDol, lblZaznDlugPrawa, lblZaznDlugLewa}
            c.Visible = widoczne
        Next
    End Sub

    ''' <summary>Pokazuje lub ukrywa współrzędne kursora i środka mapy.</summary>
    Public Sub UstawWidokWspolrzednych(widoczne As Boolean)
        For Each c As Control In New Control() {lblKursorTytul, lblKursorUkladOpis, lblKursorUklad, lblSrodekSzer, lblSrodekOpisSzer,
                                                lblSrodekTytul, lblSrodekDlug, lblSrodekOpisDlug}
            c.Visible = widoczne
        Next
    End Sub

    ''' <summary>Wybiera zbiór map z listy.</summary>
    Public Sub UstawZbiorMap(nazwa As String)
        cmbZbiorMap.Text = nazwa
    End Sub

    ''' <summary>Przywraca domyślny widok mapy (cała Polska).</summary>
    Public Sub DomyslnyWidokMapy()
        mapa.Overlays.Clear()
        lblSrodekSzer.Text = "52.3"
        lblSrodekDlug.Text = "19.2"
        lblZoom.Text = "6"
        mapa.Zoom = 6
        mapa.Position = New PointLatLng(52.3, 19.2)
        mapa.Refresh()
    End Sub

    ''' <summary>Zapisuje aktualne położenie mapy w ustawieniach.</summary>
    Private Sub ZapamietajPolozenieMapy()
        Ustawienia.SrodekMapySzerokosc = lblSrodekSzer.Text
        Ustawienia.SrodekMapyDlugosc = lblSrodekDlug.Text
        Ustawienia.SkalaMapy = lblZoom.Text
    End Sub

#End Region

#Region "Uruchomienie i zamknięcie"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'wyświetla nazwę i wersję
        Me.Text = My.Application.Info.Title & " " & My.Application.Info.Version.ToString

        FolderProgramu = My.Application.Info.DirectoryPath
        FolderDanych = UstalFolderDanych(FolderProgramu)
        Directory.CreateDirectory(FolderDownload)
        Ustawienia.FolderSegmentow = FolderDownload
        FolderSesji = FolderDownload

        'serwery WMS (np. Geoportal) wymagają TLS 1.2 - na starszych wersjach Windows nie jest on domyślnie włączony;
        'domyślnie .NET Framework ogranicza też liczbę jednoczesnych połączeń z serwerem do 2
        Net.ServicePointManager.SecurityProtocol = Net.ServicePointManager.SecurityProtocol Or Net.SecurityProtocolType.Tls12
#If Not NETCOREAPP Then
        Net.ServicePointManager.DefaultConnectionLimit = 16
#End If

        Me.SetDesktopLocation(0, 0)

        'ustawienia startowe okna mapy
        mapa.Manager.Mode = AccessMode.ServerAndCache
        mapa.DragButton = MouseButtons.Left
        mapa.MapScaleInfoEnabled = False
        mapa.DisableAltForSelection = True
        mapa.Zoom = Wartosc(lblZoom.Text, 6)

        UstawEdycjeXY(False)
        UstawWidokZaznaczeniaWgs(False)

        _ustawianieUkladu = True
        For Each u In UkladWspolrzednych.Wszystkie
            cmbUklad.Items.Add(u)
        Next
        _ustawianieUkladu = False

        'wczytywanie ustawień: najpierw conf.txt z folderu domyślnego, potem lastsettings.txt
        If File.Exists(FolderDownload & "conf.txt") = False Then
            btnScalanie.Enabled = False
            mapa.Position = New PointLatLng(52.3, 19.2)
            mapa.Zoom = 6
        End If

        WczytajConf(False)
        WczytajLastsettings()

        'lista zbiorów map
        WczytajListeZbiorowMap()
        WczytajWarstwyZbioru()
        PokazWarstwy()

        If Ustawienia.Format = "" Then Ustawienia.Format = "jpeg"
        stFormat.Text = "." & Ustawienia.Format
        stSegment.Text = ""
        OdswiezOpisUkladu()
        PrzeliczSiatke()

        _zaladowany = True
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _przerwanie?.Cancel()
        ZapamietajPolozenieMapy()
        Try
            Ustawienia.Zapisz(PlikLastsettings)
        Catch ex As Exception
            MsgBox("Nie udało się zapisać ustawień: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try
    End Sub

#End Region

#Region "Ustawienia programu i sesji"

    ''' <summary>Odczyt ostatnich ustawień programu z lastsettings.txt.</summary>
    Public Sub WczytajLastsettings()
        If File.Exists(PlikLastsettings) = False Then Exit Sub
        Try
            Ustawienia.Wczytaj(PlikLastsettings)
        Catch
            Exit Sub
        End Try
        lblSrodekSzer.Text = Ustawienia.SrodekMapySzerokosc
        lblSrodekDlug.Text = Ustawienia.SrodekMapyDlugosc
        lblZoom.Text = Ustawienia.SkalaMapy

        'ostatnio zapisana pozycja okna mapy
        mapa.Zoom = Wartosc(lblZoom.Text, 6)
        mapa.Position = New PointLatLng(Wartosc(lblSrodekSzer.Text, 52.3), Wartosc(lblSrodekDlug.Text, 19.2))

        Komunikat("Wczytano ostatnio zapisane ustawienia programu z lastsettings.txt. Styl numerowania segmentów to: " & Ustawienia.Numeracja, Color.Green)
    End Sub

    ''' <summary>
    ''' Odczyt parametrów sesji z conf.txt. Gdy pytajOFolder - użytkownik wskazuje folder sesji.
    ''' Cały plik jest najpierw wczytywany, a dopiero potem przypisywany do kontrolek (dawniej zmiana rodzaju mapy
    ''' w trakcie czytania pliku zamykała go i reszta parametrów nie była wczytywana).
    ''' </summary>
    Private Sub WczytajConf(pytajOFolder As Boolean)
        Dim folder As String = FolderSesji
        If pytajOFolder Then
            dlgFolder.SelectedPath = FolderDownload
            If dlgFolder.ShowDialog = DialogResult.OK Then folder = dlgFolder.SelectedPath & "\"
            If File.Exists(folder & "conf.txt") = False Then
                MsgBox("Brak pliku conf.txt w podanej lokalizacji.")
                Exit Sub
            End If
        End If

        FolderSesji = folder
        If File.Exists(folder & "conf.txt") = False Then Exit Sub

        Dim c As KonfiguracjaSesji
        Try
            c = KonfiguracjaSesji.Wczytaj(folder & "conf.txt", KodowanieSystemowe())
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku conf.txt: " & ex.Message, Color.Red)
            Exit Sub
        End Try

        'zbiór map - zmiana wywołuje wczytanie listy warstw i wyczyszczenie wybranych
        cmbZbiorMap.Text = c.RodzajMapy
        Ustawienia.Uklad = UkladWspolrzednych.ZKodu(c.UkladEpsg)
        txtXDol.Text = c.XDol
        txtYLewy.Text = c.YLewy
        txtXGora.Text = c.XGora
        txtYPrawy.Text = c.YPrawy
        txtBokSegmentu.Text = c.BokSegmentuPx
        txtRozmiarPiksela.Text = c.RozmiarPiksela

        _warstwy.Clear()
        For i = 0 To Math.Min(c.LiczbaWarstw, MaksLiczbaWarstw) - 1
            If c.Warstwy(i) <> "" Then _warstwy.Add(c.Warstwy(i))
        Next
        PokazWarstwy()

        If c.Format <> "" Then Ustawienia.Format = c.Format
        Ustawienia.Prefiks = c.Prefiks
        Ustawienia.PobierajPowyzejOstatniego = c.PobierajPowyzejOstatniego
        If c.Numeracja <> "" Then Ustawienia.Numeracja = c.Numeracja
        lblSrodekSzer.Text = c.SrodekMapySzerokosc
        lblSrodekDlug.Text = c.SrodekMapyDlugosc
        lblZoom.Text = c.SkalaMapy

        stFormat.Text = "." & Ustawienia.Format
        OdswiezOpisUkladu()

        'odświeżenie widoku okna mapy
        mapa.Refresh()
        mapa.ReloadMap()
        mapa.Zoom = Wartosc(lblZoom.Text, 6)
        mapa.Position = New PointLatLng(Wartosc(lblSrodekSzer.Text, 52.3), Wartosc(lblSrodekDlug.Text, 19.2))
        ZapamietajPolozenieMapy()

        Komunikat("Wczytano ustawienia sesji z pliku conf.txt. Format graficzny pobieranych segmentów to " & Ustawienia.Format &
                  ". Styl ich numerowania to: " & Ustawienia.Numeracja, Color.Green)
    End Sub

    ''' <summary>Zapis parametrów sesji do conf.txt. Gdy pytajOFolder - użytkownik wskazuje folder.</summary>
    Public Sub ZapiszConf(pytajOFolder As Boolean)
        If pytajOFolder Then
            dlgFolder.SelectedPath = FolderSesji
            If dlgFolder.ShowDialog = DialogResult.OK Then FolderSesji = dlgFolder.SelectedPath & "\"
        End If

        Dim c As New KonfiguracjaSesji With {
            .Folder = FolderSesji, .RodzajMapy = cmbZbiorMap.Text,
            .XDol = txtXDol.Text, .YLewy = txtYLewy.Text, .XGora = txtXGora.Text, .YPrawy = txtYPrawy.Text,
            .BokSegmentuPx = txtBokSegmentu.Text, .RozmiarPiksela = txtRozmiarPiksela.Text,
            .LiczbaWarstw = _warstwy.Count, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .SrodekMapySzerokosc = lblSrodekSzer.Text.Replace(","c, "."c), .SrodekMapyDlugosc = lblSrodekDlug.Text.Replace(","c, "."c),
            .SkalaMapy = lblZoom.Text.Replace(","c, "."c), .Numeracja = Ustawienia.Numeracja, .UkladEpsg = Ustawienia.Uklad.Epsg}
        For i = 0 To _warstwy.Count - 1
            c.Warstwy(i) = _warstwy(i)
        Next
        Try
            Directory.CreateDirectory(FolderSesji)
            c.Zapisz(FolderSesji & "conf.txt", KodowanieSystemowe())
        Catch ex As Exception
            Komunikat("Nie udało się zapisać pliku conf.txt: " & ex.Message, Color.Red)
            Exit Sub
        End Try

        If pytajOFolder Then Komunikat("Zapisano ustawienia sesji do pliku conf.txt w folderze " & FolderSesji, Color.Green)
    End Sub

    Private Sub ZapiszToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZapiszToolStripMenuItem.Click
        ZapiszConf(True)
    End Sub

    Private Sub WczytajToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles WczytajToolStripMenuItem1.Click
        WczytajConf(True)
    End Sub

#End Region

#Region "Zbiory map i warstwy"

    ''' <summary>Lista zbiorów map z pliku warstwy\warstwy.txt.</summary>
    Private Sub WczytajListeZbiorowMap()
        Dim plik As String = FolderProgramu & "\warstwy\warstwy.txt"
        If File.Exists(plik) = False Then
            Komunikat("Brak pliku " & plik & " z listą zbiorów map.", Color.Red)
            Exit Sub
        End If
        For Each linia In File.ReadAllLines(plik, PlikWarstw.Kodowanie())
            Dim nazwa As String = linia.Trim().Trim(""""c)
            If nazwa <> "" Then cmbZbiorMap.Items.Add(nazwa)
        Next
    End Sub

    ''' <summary>Warstwy bieżącego zbioru map do listy wyboru.</summary>
    Private Sub WczytajWarstwyZbioru()
        lstWarstwy.Items.Clear()
        _plikWarstw = New PlikWarstw()

        'brak pliku (np. zbiór map zapisany w conf.txt, a usunięty w nowszej wersji programu) - lista pozostaje pusta
        Dim plik As String = FolderProgramu & "\warstwy\" & cmbZbiorMap.Text & ".txt"
        If File.Exists(plik) = False Then Exit Sub
        Try
            _plikWarstw = PlikWarstw.Wczytaj(plik)
        Catch ex As Exception
            Komunikat("Nie udało się odczytać pliku zbioru map: " & ex.Message, Color.Red)
            Exit Sub
        End Try
        For Each w In _plikWarstw.Warstwy
            lstWarstwy.Items.Add(w.Nazwa)
        Next
    End Sub

    ''' <summary>Wyświetla wybrane warstwy w etykietach 1) ... 12).</summary>
    Private Sub PokazWarstwy()
        Dim etykiety() As Label = {lblWarstwa01, lblWarstwa02, lblWarstwa03, lblWarstwa04, lblWarstwa05, lblWarstwa06,
                                   lblWarstwa07, lblWarstwa08, lblWarstwa09, lblWarstwa10, lblWarstwa11, lblWarstwa12}
        For i = 0 To etykiety.Length - 1
            etykiety(i).Text = (i + 1).ToString() & ") " & If(i < _warstwy.Count, _warstwy(i), "")
        Next
        btnResetujWarstwy.Enabled = _warstwy.Count > 0
    End Sub

    ''' <summary>Czyści listę wybranych warstw.</summary>
    Private Sub WyczyscWarstwy()
        _warstwy.Clear()
        PokazWarstwy()
    End Sub

    Private Sub cmbZbiorMap_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbZbiorMap.SelectedIndexChanged
        PrzerwijPobieranie()
        WczytajWarstwyZbioru()
        WyczyscWarstwy()
        Komunikat("Zmieniono rodzaj mapy. Wybierz która dokładnie jej warstwa ma zostać pobrana, klikając na nią myszką", Color.Red)
    End Sub

    'dodawanie warstw
    Private Sub lstWarstwy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstWarstwy.SelectedIndexChanged
        If lstWarstwy.SelectedItem Is Nothing Then Exit Sub
        Dim nazwa As String = lstWarstwy.SelectedItem.ToString()

        If _warstwy.Count >= MaksLiczbaWarstw Then
            MsgBox("TEJ WARSTWY NIE MOŻNA JUŻ DODAĆ.")
            Exit Sub
        End If

        _warstwy.Add(nazwa)
        PokazWarstwy()
        Form2.OdswiezNazweTrekBuddy()

        'rozmiar piksela przypisany do warstwy w pliku zbioru map
        Dim w = _plikWarstw.Znajdz(nazwa)
        If w IsNot Nothing AndAlso w.RozmiarPiksela <> "" Then txtRozmiarPiksela.Text = RozmiarPikselaWarstwy(w.RozmiarPiksela)

        Komunikat("Wskazano warstwę: " & nazwa, Color.Green)

        'komunikaty o warstwach WMS HGIS, które ograniczają maksymalny rozmiar segmentu do 256 px
        Dim opis As String = ""
        Select Case nazwa
            Case "m25k"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:25 000 Messtischblatt. Pokrywa ona swoim zasięgiem terytorium Zaboru Pruskiego. Wskazany WMS portalu hgis.cartomatic.pl, który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "wig25k"
                opis = "Wybrałeś warstwę polskiej mapy topograficznej 1:25 000 Wojskowego Instytutu Geograficznego. Pokrywa ona głównie środkową i północną część terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "wig100k"
                opis = "Wybrałeś warstwę polskiej mapy topograficznej 1:100 000 Wojskowego Instytutu Geograficznego. Pokrywa ona terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Karte des Deutschen Reiches. Obejmuje ona terytorium Zaboru Pruskiego. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr_gb"
                opis = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Grossblatt. Obejmuje ona większość terytorium IIIRP. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
        End Select
        If opis <> "" Then
            txtBokSegmentu.Text = "250"
            Komunikat(opis, Color.Blue)
        End If
    End Sub

    Private Sub btnResetujWarstwy_Click(sender As Object, e As EventArgs) Handles btnResetujWarstwy.Click
        WyczyscWarstwy()
        ZapiszConf(False)
        btnPobierz.Enabled = True
        btnPrzerwij.Enabled = False
        Komunikat("Zresetowano listę wprowadzonych warstw mapy wskazanych do pobrania", Color.Green)
    End Sub

#End Region

#Region "Siatka segmentów"

    ''' <summary>Siatka segmentów wg pól formularza.</summary>
    Private Function BiezacaSiatka() As Siatka
        Dim obszar As New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text))
        Return New Siatka(obszar, Wartosc(txtRozmiarPiksela.Text), WartoscCalkowita(txtBokSegmentu.Text))
    End Function

    ''' <summary>
    ''' Przelicza wynikowy rozmiar siatki segmentów (dawniej te same obliczenia powtarzało sześć procedur,
    ''' z których część korzystała z nieaktualnych jeszcze wartości).
    ''' </summary>
    Private Sub PrzeliczSiatke()
        Dim s = BiezacaSiatka()
        Dim km As Double = 1000
        If Ustawienia.Uklad.Geograficzny Then
            'w WGS84 rozmiary w km są przybliżone - liczone dla kierunku północ-południe w środku obszaru
            Dim mSz As Double, mDl As Double
            Georeferencja.MetrowNaStopien((s.Obszar.XDol + s.Obszar.XGora) / 2, mSz, mDl)
            km = If(mSz > 0, 1000 / mSz, Double.NaN)
        End If
        If s.Poprawna Then
            txtLiczbaKolumn.Text = s.LiczbaKolumn.ToString()
            txtLiczbaWierszy.Text = s.LiczbaWierszy.ToString()
            txtSzerokoscPx.Text = s.SzerokoscPx.ToString()
            txtWysokoscPx.Text = s.WysokoscPx.ToString()
            txtSzerokoscKm.Text = If(Double.IsNaN(km), "", Liczba(Math.Round(s.LiczbaKolumn * s.BokSegmentu / km, 3)))
            txtWysokoscKm.Text = If(Double.IsNaN(km), "", Liczba(Math.Round(s.LiczbaWierszy * s.BokSegmentu / km, 3)))
        Else
            For Each t In New TextBox() {txtLiczbaKolumn, txtLiczbaWierszy, txtSzerokoscPx, txtWysokoscPx, txtSzerokoscKm, txtWysokoscKm}
                t.Text = ""
            Next
        End If
        Dim bok As Double = Wartosc(txtRozmiarPiksela.Text) * WartoscCalkowita(txtBokSegmentu.Text)
        txtZasiegSegmentuKm.Text = If(Double.IsNaN(km) OrElse bok <= 0, "", Liczba(Math.Round(bok / km, 3)))
    End Sub

    Private Sub PolaZasieguZmienione(sender As Object, e As EventArgs) Handles txtXDol.TextChanged, txtYLewy.TextChanged, txtXGora.TextChanged, txtYPrawy.TextChanged
        PrzeliczSiatke()
    End Sub

    Private Sub ParametrySegmentuZmienione(sender As Object, e As EventArgs) Handles txtBokSegmentu.TextChanged, txtRozmiarPiksela.TextChanged
        PrzeliczSiatke()
        If mapa.SelectedArea.IsEmpty = False Then
            mapa.Overlays.Clear()
            PokazZnaczniki()
        End If
    End Sub

    'zamiana przecinka na kropkę w polu rozmiaru piksela
    Private Sub txtRozmiarPiksela_KeyUp(sender As Object, e As KeyEventArgs) Handles txtRozmiarPiksela.KeyUp
        If txtRozmiarPiksela.Text.Contains(",") Then
            txtRozmiarPiksela.Text = txtRozmiarPiksela.Text.Replace(","c, "."c)
            txtRozmiarPiksela.SelectionStart = txtRozmiarPiksela.Text.Length
        End If
    End Sub

    ''' <summary>Opis układu współrzędnych przy współrzędnych kursora.</summary>
    Public Sub OdswiezOpisUkladu()
        Dim u = Ustawienia.Uklad
        Select Case u.Epsg
            Case 2180 : lblKursorUkladOpis.Text = "1992"
            Case 2176 To 2179 : lblKursorUkladOpis.Text = "2000/" & (u.Epsg - 2171).ToString()
            Case 4326 : lblKursorUkladOpis.Text = "WGS84"
            Case Else : lblKursorUkladOpis.Text = "UTM " & (u.Epsg - 32600).ToString() & "N"
        End Select
        ToolTip1.SetToolTip(lblKursorUkladOpis, u.Nazwa)
        lblRozmiarPikselaOpis.Text = If(u.Geograficzny, "Rozmiar piksela [°/pix]", "Rozmiar piksela [m/pix]")

        _ustawianieUkladu = True
        cmbUklad.SelectedItem = u
        _ustawianieUkladu = False
        PrzeliczSiatke()
    End Sub

    ''' <summary>
    ''' Zmiana układu współrzędnych pobierania: wpisany zasięg i rozmiar piksela są przeliczane do nowego układu.
    ''' </summary>
    Private Sub cmbUklad_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbUklad.SelectedIndexChanged
        If _ustawianieUkladu Then Exit Sub
        Dim nowy = TryCast(cmbUklad.SelectedItem, UkladWspolrzednych)
        Dim stary = Ustawienia.Uklad
        If nowy Is Nothing OrElse nowy Is stary Then Exit Sub

        Dim obszar As New Zasieg(Wartosc(txtXDol.Text), Wartosc(txtYLewy.Text), Wartosc(txtXGora.Text), Wartosc(txtYPrawy.Text))
        Ustawienia.Uklad = nowy
        If obszar.Szerokosc > 0 AndAlso obszar.Wysokosc > 0 Then
            Dim srodek = stary.DoWgs84(obszar.Srodek)
            Dim piksel As Double = Wartosc(txtRozmiarPiksela.Text)
            Dim z = stary.PrzeliczZasieg(nowy, obszar)
            txtXDol.Text = Liczba(z.XDol)
            txtYLewy.Text = Liczba(z.YLewy)
            txtXGora.Text = Liczba(z.XGora)
            txtYPrawy.Text = Liczba(z.YPrawy)
            If piksel > 0 Then txtRozmiarPiksela.Text = Liczba(stary.PrzeliczRozmiarPiksela(nowy, piksel, srodek))
        End If
        OdswiezOpisUkladu()
        Komunikat("Wybrano układ " & nowy.Nazwa & ". Zasięg pobierania przeliczono do nowego układu. Upewnij się, że serwer WMS obsługuje ten układ.", Color.Green)
    End Sub

    ''' <summary>Rozmiar piksela z pliku zbioru map (w metrach) w bieżącym układzie.</summary>
    Private Function RozmiarPikselaWarstwy(tekst As String) As String
        Dim u = Ustawienia.Uklad
        If Not u.Geograficzny Then Return tekst
        Dim metry As Double = Wartosc(tekst)
        If metry <= 0 Then Return tekst
        Return Liczba(UkladWspolrzednych.PL1992.PrzeliczRozmiarPiksela(u, metry, New PunktGeo(mapa.Position.Lat, mapa.Position.Lng)))
    End Function

#End Region

#Region "Pobieranie"

    Private Async Sub btnPobierz_Click(sender As Object, e As EventArgs) Handles btnPobierz.Click
        If Ustawienia.Format = "svg+xml" Then
            MsgBox("Ten format jeszcze nie działa :o(", , "Zmień format.")
            Form2.ShowDialog()
            Exit Sub
        End If

        'kontrola parametrów
        Dim piksel As Double = Wartosc(txtRozmiarPiksela.Text)
        Dim bok As Integer = WartoscCalkowita(txtBokSegmentu.Text)
        If txtRozmiarPiksela.Text.Trim() = "" Then
            Komunikat("Nie podano rozmiaru pojedynczego piksela segmentu", Color.Red) : Exit Sub
        ElseIf piksel <= 0 Then
            Komunikat("Rozmiar pojedynczego piksela segmentu musi być większy od zera", Color.Red) : Exit Sub
        End If
        If txtBokSegmentu.Text.Trim() = "" Then
            Komunikat("Nie podano długości boku segmentu.", Color.Red) : Exit Sub
        ElseIf bok < 1 Then
            Komunikat("Długość boku segmentu musi być większa od zera.", Color.Red) : Exit Sub
        ElseIf bok > 2048 Then
            Komunikat("Długość boku segmentu musi być mniejsza od 2048px.", Color.Red) : Exit Sub
        End If
        Dim siatka = BiezacaSiatka()
        If Not siatka.Poprawna Then
            Komunikat("Niepoprawne współrzędne X, Y.", Color.Red) : Exit Sub
        End If
        If _warstwy.Count = 0 Then
            Komunikat("Nie wybrano żadnej warstwy. Aby wybrać warstwę kliknij w jej nazwę.", Color.Red) : Exit Sub
        End If
        If _plikWarstw.Adres = "" Then
            Komunikat("Nie wybrano zbioru map (serwera WMS).", Color.Red) : Exit Sub
        End If

        'zapis parametrów sesji - na wszelki wypadek, żeby nie okazało się, że zapisuje w folderze głównym programu
        If FolderSesji = "" Then
            MsgBox("folder segmentów jest pusty")
            Exit Sub
        End If
        ZapiszConf(False)

        Dim zadanie As New ZadaniePobierania With {
            .AdresSerwera = _plikWarstw.Adres, .Warstwy = New List(Of String)(_warstwy), .Uklad = Ustawienia.Uklad,
            .Siatka = siatka, .Format = Ustawienia.Format, .Prefiks = Ustawienia.Prefiks,
            .Numeracja = Siatka.StylZTekstu(Ustawienia.Numeracja), .Folder = FolderSesji, .ZamienOsie = Ustawienia.ZamienXY,
            .IloscProb = Math.Max(1, Ustawienia.IloscProbPobrania), .PrzerwaSekundy = Ustawienia.PrzerwaMiedzyProbami,
            .LiczbaWatkow = Ustawienia.LiczbaWatkow, .PobierajPowyzejOstatniego = Ustawienia.PobierajPowyzejOstatniego,
            .Georeferencja = Ustawienia.OpcjeGeoreferencji(), .TrekBuddy = Ustawienia.TrekBuddy, .NazwaTrekBuddy = Ustawienia.NazwaTrekBuddy}

        btnPobierz.Enabled = False
        btnPrzerwij.Enabled = True
        cmbZbiorMap.Enabled = False
        lstWarstwy.Enabled = False
        btnScalanie.Enabled = False
        Komunikat("Trwa pobieranie segmentów (" & siatka.LiczbaSegmentow & ")", Color.Black)

        _przerwanie = New CancellationTokenSource()
        Dim postep As New Progress(Of PostepPobierania)(AddressOf PokazPostep)
        Dim wynik As WynikPobierania = Nothing
        Try
            wynik = Await PobieranieSegmentow.PobierzAsync(zadanie, postep, _przerwanie.Token)
        Catch ex As Exception
            Komunikat("Błąd pobierania: " & ex.Message, Color.Red)
        Finally
            _przerwanie.Dispose()
            _przerwanie = Nothing
            btnPobierz.Enabled = True
            btnPrzerwij.Enabled = False
            cmbZbiorMap.Enabled = True
            lstWarstwy.Enabled = True
            stPostep.Value = 0
            stSegment.Text = ""
        End Try

        If wynik Is Nothing Then Exit Sub
        If wynik.Przerwano Then
            Komunikat("Przerwano pobieranie segmentów", Color.Red)
        ElseIf wynik.Nieudane > 0 Then
            Komunikat("Nie udało się ściągnąć wszystkich segmentów (" & wynik.Nieudane & " z " & wynik.Wszystkie &
                      "). Wykaz tych segmentów i przyczyny błędów w pliku error.txt", Color.Red)
        Else
            Komunikat("Zakończono pobieranie. Mapa znajduje się w katalogu " & FolderSesji, Color.Green)
        End If
        btnScalanie.Enabled = File.Exists(FolderSesji & "error.txt") = False
    End Sub

    Private Sub PokazPostep(p As PostepPobierania)
        If p.Komunikat <> "" Then Komunikat(p.Komunikat, Color.Black)
        If p.Wszystkie > 0 Then stPostep.Value = Math.Min(100, CInt(p.Pobrane * 100L \ p.Wszystkie))
        stSegment.Text = p.Segment
    End Sub

    ''' <summary>Przerywa trwające pobieranie.</summary>
    Public Sub PrzerwijPobieranie()
        _przerwanie?.Cancel()
    End Sub

    Private Sub btnPrzerwij_Click(sender As Object, e As EventArgs) Handles btnPrzerwij.Click
        'komunikat przed przerwaniem - zakończenie pobierania może nastąpić od razu, wewnątrz Cancel(),
        'i wtedy jego komunikat końcowy nie może zostać nadpisany
        Komunikat("Przerywanie pobierania segmentów...", Color.Red)
        btnPrzerwij.Enabled = False
        PrzerwijPobieranie()
    End Sub

#End Region

#Region "Mapa"

    'zdarzenie CheckedChanged zachodzi także przy ODZNACZANIU przycisku - reaguje tylko zaznaczony
    Private Sub rbOsm_CheckedChanged(sender As Object, e As EventArgs) Handles rbOsm.CheckedChanged
        If rbOsm.Checked = False Then Exit Sub
        GMapProvider.UserAgent = "MapoTero/" & My.Application.Info.Version.ToString
        mapa.MapProvider = GMapProviders.OpenStreetMap
    End Sub

    Private Sub rbGoogle_CheckedChanged(sender As Object, e As EventArgs) Handles rbGoogle.CheckedChanged
        If rbGoogle.Checked = False Then Exit Sub
        mapa.MapProvider = GMapProviders.GoogleMap
    End Sub

    Private Sub rbBing_CheckedChanged(sender As Object, e As EventArgs) Handles rbBing.CheckedChanged
        If rbBing.Checked = False Then Exit Sub
        mapa.MapProvider = GMapProviders.BingSatelliteMap
    End Sub

    Private Sub mapa_MouseMove(sender As Object, e As MouseEventArgs) Handles mapa.MouseMove
        Dim u = Ustawienia.Uklad

        'współrzędne zasięgu podczas przeciągania zaznaczenia
        If Ustawienia.EdycjaXY = False AndAlso mapa.SelectedArea.IsEmpty = False Then
            Dim a = mapa.SelectedArea
            Dim gora As Double = a.Lat, dol As Double = a.Lat - a.HeightLat
            Dim lewa As Double = a.Lng, prawa As Double = a.Lng + a.WidthLng

            'prostokąt WGS84 w innym układzie jest lekko obróconym czworokątem - zasięg obejmuje wszystkie jego narożniki
            Dim narozniki() As PunktXY = {u.ZWgs84(gora, lewa), u.ZWgs84(gora, prawa), u.ZWgs84(dol, lewa), u.ZWgs84(dol, prawa)}
            Dim xMin As Double = Double.MaxValue, xMax As Double = Double.MinValue, yMin As Double = Double.MaxValue, yMax As Double = Double.MinValue
            For Each p In narozniki
                xMin = Math.Min(xMin, p.X) : xMax = Math.Max(xMax, p.X)
                yMin = Math.Min(yMin, p.Y) : yMax = Math.Max(yMax, p.Y)
            Next
            If u.Geograficzny Then
                txtXDol.Text = Liczba(Math.Round(xMin, 6)) : txtYLewy.Text = Liczba(Math.Round(yMin, 6))
                txtXGora.Text = Liczba(Math.Round(xMax, 6)) : txtYPrawy.Text = Liczba(Math.Round(yMax, 6))
            Else
                txtXDol.Text = Liczba(Math.Floor(xMin)) : txtYLewy.Text = Liczba(Math.Floor(yMin))
                txtXGora.Text = Liczba(Math.Ceiling(xMax)) : txtYPrawy.Text = Liczba(Math.Ceiling(yMax))
            End If

            lblZaznSzerGora.Text = "lat= " & Liczba(Math.Round(gora, 4))
            lblZaznSzerDol.Text = "lat= " & Liczba(Math.Round(dol, 4))
            lblZaznDlugPrawa.Text = "lng= " & Liczba(Math.Round(prawa, 4))
            lblZaznDlugLewa.Text = "lng= " & Liczba(Math.Round(lewa, 4))

            Dim s = BiezacaSiatka()
            If s.Poprawna AndAlso Not u.Geograficzny Then
                Dim km2 As Double = s.LiczbaKolumn * s.BokSegmentu / 1000 * s.LiczbaWierszy * s.BokSegmentu / 1000
                If s.SzerokoscPx * s.WysokoscPx < 5000000000L Then
                    Komunikat("Zaznaczyłeś obszar do pobrania o powierzchni " & Liczba(Math.Round(km2, 3)) & " km2", Color.Green)
                Else
                    Komunikat("Zaznaczyłeś obszar do pobrania o powierzchni " & Liczba(Math.Round(km2, 3)) & " km2. To dużo. Poradzę sobie. Jednak uzbroj się lepiej w kubek gorącej kawy :)", Color.Red)
                End If
            End If
        End If

        'współrzędne środka mapy w WGS84
        lblSrodekSzer.Text = Liczba(Math.Round(mapa.Position.Lat, 4))
        lblSrodekDlug.Text = Liczba(Math.Round(mapa.Position.Lng, 4))

        'współrzędne kursora
        Dim kursor = mapa.FromLocalToLatLng(e.X, e.Y)
        Dim k = u.ZWgs84(kursor.Lat, kursor.Lng)
        If u.Geograficzny Then
            lblKursorUklad.Text = "x= " & Liczba(Math.Round(k.X, 5)) & "   y= " & Liczba(Math.Round(k.Y, 5))
        Else
            lblKursorUklad.Text = "x= " & Liczba(Math.Round(k.X)) & "   y= " & Liczba(Math.Round(k.Y))
        End If

        lblKursorWgs.BackColor = Color.Transparent
        lblKursorWgs.Location = New Point(e.X - 140, e.Y - 10)
        If Ustawienia.KursorWgs84 Then
            lblKursorWgs.Text = "lat= " & Liczba(Math.Round(kursor.Lat, 4)) & "   lng= " & Liczba(Math.Round(kursor.Lng, 4))
            lblKursorWgs.Visible = True
        Else
            lblKursorWgs.Visible = False
        End If
    End Sub

    Private Sub btnPowieksz_Click(sender As Object, e As EventArgs) Handles btnPowieksz.Click
        mapa.Zoom += 1
    End Sub

    Private Sub btnPomniejsz_Click(sender As Object, e As EventArgs) Handles btnPomniejsz.Click
        mapa.Zoom -= 1
    End Sub

    Private Sub mapa_OnMapZoomChanged() Handles mapa.OnMapZoomChanged
        lblZoom.Text = mapa.Zoom.ToString()
    End Sub

    Private Sub mapa_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles mapa.MouseDoubleClick
        If e.Button = MouseButtons.Right Then
            mapa.Zoom -= 1
        Else
            mapa.Zoom += 1
        End If
    End Sub

    'znaczniki rzeczywistego zasięgu pobieranej mapy
    Private Sub btnZnaczniki_Click(sender As Object, e As EventArgs) Handles btnZnaczniki.Click
        mapa.Overlays.Clear()
        PokazZnaczniki()
    End Sub

    'znaczniki wyświetlane po zaznaczeniu obszaru pobierania
    Private Sub mapa_MouseClick(sender As Object, e As MouseEventArgs) Handles mapa.MouseClick
        If mapa.SelectedArea.IsEmpty = False AndAlso e.Button = MouseButtons.Right Then
            mapa.Overlays.Clear()
            PokazZnaczniki()
        End If
    End Sub

    ''' <summary>Znaczniki narożników siatki segmentów (zasięg po uwzględnieniu rozmiaru segmentów).</summary>
    Private Sub PokazZnaczniki()
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then Exit Sub
        Dim u = Ustawienia.Uklad
        Dim z = s.ZasiegSiatki

        btnZnaczniki.Enabled = False
        Dim warstwa As New GMapOverlay("markers")
        Dim narozniki = {Tuple.Create(z.LewyGorny, "Lewy górny narożnik"), Tuple.Create(z.PrawyGorny, "Prawy górny narożnik"),
                         Tuple.Create(z.PrawyDolny, "Prawy dolny narożnik"), Tuple.Create(z.LewyDolny, "Lewy dolny narożnik")}
        For Each n In narozniki
            Dim g = u.DoWgs84(n.Item1)
            Dim znacznik As New GMarkerGoogle(New PointLatLng(g.Szerokosc, g.Dlugosc), GMarkerGoogleType.red_small)
            znacznik.ToolTip = New GMapRoundedToolTip(znacznik)
            znacznik.ToolTipText = n.Item2 & vbCrLf & "po uwzględnieniu" & vbCrLf & "rozmiaru segmentów " & vbCrLf &
                                   "X: " & Liczba(n.Item1.X) & ", Y: " & Liczba(n.Item1.Y)
            warstwa.Markers.Add(znacznik)
        Next
        mapa.Overlays.Add(warstwa)

        'centrowanie obrazu po wyświetleniu znaczników
        Dim srodek = u.DoWgs84(z.Srodek)
        mapa.Position = New PointLatLng(srodek.Szerokosc, srodek.Dlugosc)
    End Sub

#End Region

#Region "Przyciski i menu"

    Private Sub btnOtworzFolder_Click(sender As Object, e As EventArgs) Handles btnOtworzFolder.Click
        Directory.CreateDirectory(FolderDownload)
        OtworzFolder(FolderDownload)
    End Sub

    Private Sub btnUsunPobrane_Click(sender As Object, e As EventArgs) Handles btnUsunPobrane.Click
        Dim odp As DialogResult = MessageBox.Show("Czy na pewno usunąć cały katalog 'download' z pobranymi mapami?", "Usuwanie zawartości katalogu download",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If odp = DialogResult.Yes Then
            Try
                If Directory.Exists(FolderDownload) Then Directory.Delete(FolderDownload, True)
                Directory.CreateDirectory(FolderDownload)
                Komunikat("Usunięto całą zawartość katalogu download", Color.Green)
            Catch ex As Exception
                Komunikat("Nie udało się usunąć zawartości katalogu download: " & ex.Message, Color.Red)
            End Try
        End If
    End Sub

    Private Sub btnScalanie_Click(sender As Object, e As EventArgs) Handles btnScalanie.Click
        Form3.ShowDialog()
    End Sub

    Private Sub ScalanieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScalanieToolStripMenuItem.Click
        Form3.ShowDialog()
    End Sub

    Private Sub EksportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EksportToolStripMenuItem.Click
        FormEksport.ShowDialog()
    End Sub

    Private Sub UstawieniaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UstawieniaToolStripMenuItem.Click
        Form2.ShowDialog()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.ShowDialog()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        pomoc_pfe.ShowDialog()
    End Sub

    Private Sub InstrukcjaObslugiToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_Obslugi.ShowDialog()
    End Sub

    Private Sub UsuwaniePustychSegmentowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsuwaniePustychSegmentówToolStripMenuItem.Click
        Usuwanie_p_seg.ShowDialog()
    End Sub

    Private Sub NakladanieWarstwToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NakładanieWarstwNaSiebieToolStripMenuItem.Click
        Nakladanie_Map.ShowDialog()
    End Sub

#End Region

End Class
