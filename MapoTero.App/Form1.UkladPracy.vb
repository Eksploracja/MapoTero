Imports System.IO
Imports GMap.NET
Imports MapoTero.Core

Partial Public Class Form1
    Private _zakladkiPracy As TabControl
    Private _zakladkaKomunikatow As TabPage
    Private _zakladkaSesji As TabPage
    Private _dziennikSesji As RichTextBox
    Private _folderZakladki As TextBox
    Private _informacjeOPlikach As Label
    Private _szczegolyBledow As LinkLabel
    Private _liczbaBledowSesji As Integer
    Private _dopasowywanieZakladek As Boolean
    Private _podsumowanieObszaru As Label
    Private _wymiaryObszaru As Label
    Private _podsumowanieParametrow As Label
    Private _rozdzielczoscBelki As Label
    Private _postepBelki As ProgressBar
    Private _podpowiedzBelki As Label
    Private _szczegolyStanuBelki As Label
    Private _panelPobierania As Panel
    Private _prawyUklad As TableLayoutPanel
    Private _pustyPodglad As Label
    Private _ukladPodgladu As TableLayoutPanel
    Private _pasekPodgladu As Control
    Private _btnKopiujKomunikaty As Button
    Private _btnWyczyscKomunikaty As Button
    Private _ukladBelki As TableLayoutPanel
    Private _kartaParametrow As Control
    Private _wysokoscDanychBelki As Integer
    Private _wysokoscParametrowBelki As Integer
    Private _banerZaznaczania As Label
    Private _polePodkladuMapy As ComboBox
    Private _ikonaZaznaczeniaNormalna As Bitmap
    Private _ikonaZaznaczeniaAktywna As Bitmap
    Private ReadOnly _zasobyUkladu As New List(Of IDisposable)()
    Private _odczytKursora As ToolStripStatusLabel
    Private _odczytSrodka As ToolStripStatusLabel
    Private _podpowiedzZaznaczenia As ToolStripStatusLabel
    Private _przyciskZaznaczenia As Button
    Private _trybZaznaczania As Boolean
    Private _pierwszyNaroznik As PointLatLng?
    Private _ukladKompozycji As TableLayoutPanel
    Private _opcjeKompozycji As TableLayoutPanel
    Private _ukladRozdzielczosci As TableLayoutPanel
    Private _czcionkaRobocza As Font
    Private _czcionkaNaglowka As Font
    Private _czcionkaWartosci As Font
    Private _czcionkaAkcji As Font
    Private _lnkPokazObszar As LinkLabel
    Private _menuObszaru As ContextMenuStrip
    Private _lnkParametry As LinkLabel
    Private _poleSzukaniaMiejsca As PoleWyszukiwaniaMiejsca
    Private _kartaStanu As Control
    Private _stUklad As ToolStripStatusLabel
    Private _stSkala As ToolStripStatusLabel
    Private _stSesja As ToolStripStatusLabel
    Private _kursorUkladu As (X As Double, Y As Double)?
    'przeciąganie granicy między kartami prawego panelu: 1 - katalog/wybrane, 2 - wybrane/zakładki
    Private _przeciaganaGranica As Integer
    Private _poczatekPrzeciagania As Integer
    Private _wysokosciPrzedPrzeciaganiem As (Wybrane As Integer, Zakladki As Integer)
    Private Shared ReadOnly Atrament As Color = Color.FromArgb(30, 45, 61)
    Private Shared ReadOnly TloNieaktywne As Color = Color.FromArgb(228, 233, 239)
    ''' <summary>Odstęp zawartości sekcji panelu warstw od jej krawędzi (ten sam dla tytułu i kontrolek).</summary>
    Private ReadOnly Property WciecieSekcji As Integer
        Get
            Return Px(8)
        End Get
    End Property
    Private Shared ReadOnly KolorRamkiKarty As Color = Color.FromArgb(184, 198, 212)
    Private Shared ReadOnly TloBelki As Color = Color.FromArgb(225, 232, 240)
    Private Shared ReadOnly KolorPomocniczy As Color = Color.FromArgb(75, 94, 113)
    Private Shared ReadOnly KolorLinii As Color = Color.FromArgb(207, 218, 229)
    Private Shared ReadOnly TloPaneli As Color = Color.FromArgb(240, 245, 250)
    Friend Shared ReadOnly KolorAkcjiOkna As Color = Color.FromArgb(25, 78, 128)
    Private Shared ReadOnly KolorAkcjiHover As Color = Color.FromArgb(19, 62, 102)
    Private Shared ReadOnly KolorAkcjiDown As Color = Color.FromArgb(14, 48, 80)

    Private Function Px(wartosc As Integer) As Integer
        Return CInt(wartosc * DeviceDpi / 96.0)
    End Function

    Private Function ZarejestrujIkone(ikona As IkonaOkna, kolor As Color, Optional bok As Integer = 16) As Bitmap
        Dim bmp = UtworzIkoneBitmapy(ikona, Px(bok), kolor)
        _zasobyUkladu.Add(bmp)
        Return bmp
    End Function

    Private Sub UlozPanelePracy()
        SuspendLayout()
        Panel2.SuspendLayout()
        Panel3.SuspendLayout()
        ' Bez przeliczania układu po każdej dodanej kontrolce - przy starcie okno ma już uchwyty, więc każda zmiana kosztowała
        For Each sekcja As Control In New Control() {grpDaneZrodlowe, grpWybraneWarstwy, grpPodglad, pnlBelkaAkcji}
            sekcja.SuspendLayout()
        Next
        _czcionkaRobocza = New Font("Segoe UI", 9.25F)
        _czcionkaNaglowka = New Font("Segoe UI", 9.25F, FontStyle.Bold)
        _czcionkaWartosci = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        _czcionkaAkcji = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        AddHandler Disposed, Sub()
                                 _czcionkaRobocza.Dispose()
                                 _czcionkaNaglowka.Dispose()
                                 _czcionkaWartosci.Dispose()
                                 _czcionkaAkcji.Dispose()
                                 For Each zasob In _zasobyUkladu
                                     zasob.Dispose()
                                 Next
                             End Sub
        Dim renderer As New PlaskiRendererPaska()
        MenuStrip1.Renderer = renderer
        StatusStrip1.Renderer = renderer
        MenuStrip1.Font = _czcionkaRobocza
        MenuStrip1.BackColor = Color.White
        Panel2.BackColor = TloPaneli

        _zakladkiPracy = New PlaskieZakladki With {.Name = "zakladkiPracy", .Dock = DockStyle.Fill,
                                                   .Font = _czcionkaRobocza, .SizeMode = TabSizeMode.Fixed, .ShowToolTips = True}
        Dim podglad As New TabPage("Podgląd")
        _zakladkaKomunikatow = New TabPage("Komunikaty")
        _zakladkaSesji = New TabPage("Sesja")
        _zakladkiPracy.TabPages.AddRange({podglad, _zakladkaKomunikatow, _zakladkaSesji})
        AddHandler _zakladkiPracy.ClientSizeChanged, Sub() DopasujSzerokoscZakladek()
        UlozPodglad(podglad)
        UlozKomunikaty()
        UlozZakladkeSesji()

        _prawyUklad = New TableLayoutPanel With {
            .ColumnCount = 1, .RowCount = 3, .Dock = DockStyle.Top, .Padding = New Padding(Px(6), 0, Px(6), Px(5))
        }
        _prawyUklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        _prawyUklad.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        _prawyUklad.RowStyles.Add(New RowStyle(SizeType.Absolute, Px(96)))
        _prawyUklad.RowStyles.Add(New RowStyle(SizeType.Absolute, Px(156)))
        UlozKatalogWarstw()
        UlozWybraneWarstwy()
        RysujJakKarte(grpDaneZrodlowe, IkonaOkna.Katalog, btnDodajSerwer, Nothing)
        RysujJakKarte(grpWybraneWarstwy, IkonaOkna.Warstwy, btnWarstwyZaawansowane,
                      Function() If(_zestaw.Liczba > 1, "↑ na wierzchu", ""))
        UstawWygladZakladek()
        UstawUchwytyPanelu()
        DolozKomorke(_prawyUklad, grpDaneZrodlowe, 0, 0)
        DolozKomorke(_prawyUklad, grpWybraneWarstwy, 0, 1)
        DolozKomorke(_prawyUklad, _zakladkiPracy, 0, 2)
        ' Wspólne krawędzie sekcji panelu: góra pierwszej sekcji na wysokości górnej krawędzi mapy, dół zakładek na wysokości
        ' dolnej krawędzi kart belki, a lewe i prawe krawędzie sekcji i zakładek w jednej linii.
        _zakladkiPracy.Margin = Padding.Empty
        Panel2.Controls.Add(_prawyUklad)
        _prawyUklad.BringToFront()
        For Each staryPrzycisk As Control In {btnScalanie, btnOtworzFolder, _btnKreator, btnZnaczniki}
            staryPrzycisk.Visible = False
        Next
        _minWysokoscPanelu = Px(420)
        Panel2.AutoScrollMinSize = New Size(0, _minWysokoscPanelu)
        AddHandler Panel2.SizeChanged, Sub() DopasujSekcjeWarstw()
        DopasujSekcjeWarstw()

        UlozBelkePracy()
        UlozWejsciaMapy()
        UlozPasekWspolrzednych()
        UlozMenuPracy()
        ' Panel warstw zajmuje całą wysokość nad paskiem stanu. Belka korzysta z szerokości mapy.
        For Each sekcja As Control In New Control() {grpDaneZrodlowe, grpWybraneWarstwy, grpPodglad, pnlBelkaAkcji}
            sekcja.ResumeLayout(True)
        Next
        Panel3.ResumeLayout(True)
        Panel2.ResumeLayout(True)
        ResumeLayout(True)

        AddHandler stFolder.TextChanged, Sub() OdswiezZakladkeSesji()
        AddHandler stFormat.TextChanged, Sub()
                                           OdswiezPodsumowaniePracy()
                                           OdswiezZakladkeSesji()
                                       End Sub
        AddHandler _zakladkiPracy.SelectedIndexChanged,
            Sub()
                If _zakladkiPracy.SelectedTab Is _zakladkaKomunikatow Then WyzerujLicznikBledow()
                OdswiezZakladkeSesji()
            End Sub
        AddHandler lblStatusSesji.TextChanged, Sub() OdswiezPodpowiedzBelki()
        AddHandler lblStatusSesji.ForeColorChanged, Sub() OdswiezPodpowiedzBelki()
        OdswiezPodsumowaniePracy()
        OdswiezZakladkeSesji()
    End Sub

    Private _menuPobierz As ToolStripMenuItem
    Private _menuOstatnie As ToolStripMenuItem

    ''' <summary>
    ''' Menu „Sesja pobierania” w kolejności pracy: sesja (nowa, otwórz, ostatnie, zapisz) → obszar → pobieranie i eksport
    ''' → pliki. Dawniej menu nie miało pobierania, zaznaczania ani eksportu (eksport był w Narzędziach obok konwerterów),
    ''' a zapis i odczyt sesji miały skróty Alt+Z / Alt+W, które w Windows otwierają menu z klawiatury (audyt ER-06, ER-12).
    ''' </summary>
    Private Sub UlozMenuPracy()
        Dim sesja = SesjaToolStripMenuItem.DropDownItems
        ' Alt+S na pozycji głównej menu też kolidował z mnemonikami - menu otwiera Alt i klawisz podkreślonej litery.
        SesjaToolStripMenuItem.ShortcutKeys = Keys.None
        _menuNowaSesja.ShortcutKeys = Keys.Control Or Keys.N
        WczytajToolStripMenuItem1.Text = "Otwórz sesję..."
        WczytajToolStripMenuItem1.ToolTipText = "Wczytaj sesję z jej folderu (plik conf.txt): warstwy, obszar i parametry pobierania"
        WczytajToolStripMenuItem1.ShortcutKeys = Keys.Control Or Keys.O
        ZapiszToolStripMenuItem.Text = "Zapisz sesję w folderze..."
        ZapiszToolStripMenuItem.ToolTipText = "Zapisz warstwy, obszar i parametry bieżącej sesji (plik conf.txt) we wskazanym folderze"
        ZapiszToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.S
        _menuOstatnie = New ToolStripMenuItem("Ostatnie sesje") With {.Name = "OstatnieSesjeToolStripMenuItem"}
        _menuOstatnie.DropDownItems.Add(New ToolStripMenuItem("(brak)") With {.Enabled = False})
        AddHandler _menuOstatnie.DropDownOpening, Sub() WypelnijOstatnieSesje()

        Dim zaznacz As New ToolStripMenuItem("Zaznacz obszar na mapie", ZarejestrujIkone(IkonaOkna.Obszar, KolorAkcjiOkna),
                                             Sub() PrzelaczZaznaczanieObszaru()) With {.Name = "ZaznaczObszarToolStripMenuItem"}
        Dim wspolrzedne As New ToolStripMenuItem("Wpisz współrzędne obszaru...", Nothing,
                                                 Sub() btnWpiszWspolrzedne_Click(btnWpiszWspolrzedne, EventArgs.Empty)) With {.Name = "WspolrzedneToolStripMenuItem"}
        KorytarzToolStripMenuItem.Text = "Obszar pobierania wzdłuż trasy (GPX, KML, KMZ, GPKG)..."
        ' Poligon tuż pod trasą - oba wyznaczają obszar pobierania z pliku (tak samo jak w menu „Zmień ▾” karty obszaru).
        Dim poligon As New ToolStripMenuItem("Obszar pobierania z poligonu (KML, GeoJSON, GPKG)...", Nothing, Sub() WybierzPlikPoligonu()) With {
            .Name = "PoligonToolStripMenuItem",
            .ToolTipText = "Prostokąt obejmujący poligon z pliku, np. granicę gminy albo działki narysowaną w QGIS lub Google Earth"}
        Dim siatka As New ToolStripMenuItem("Pokaż obszar i siatkę na mapie", Nothing, Sub() PokazObszarNaMapie())

        _menuPobierz = New ToolStripMenuItem("Pobierz segmenty", Nothing, Sub() PobierzZMenu()) With {
            .Name = "PobierzToolStripMenuItem", .ShortcutKeys = Keys.F5}
        OdswiezToolStripMenuItem.Text = "Pobierz ponownie wszystkie segmenty..."
        ' Eksport należy do sesji (tworzy plik z jej segmentów), więc stoi tu, a nie w Narzędziach obok konwerterów.
        NarzedziaToolStripMenuItem.DropDownItems.Remove(EksportToolStripMenuItem)
        EksportToolStripMenuItem.Text = "Eksportuj mapę..."
        EksportToolStripMenuItem.ToolTipText = "Utwórz plik mapy (MBTiles, KMZ, JNX, GeoTIFF…) z segmentów pobranych do folderu sesji"

        Dim folderSesji As New ToolStripMenuItem("Otwórz folder sesji", ZarejestrujIkone(IkonaOkna.Sesja, KolorAkcjiOkna),
                                                 Sub() btnOtworzFolder_Click(btnOtworzFolderDolny, EventArgs.Empty))
        Dim pobrane As New ToolStripMenuItem("Otwórz folder pobrań (wszystkie sesje)", ZarejestrujIkone(IkonaOkna.Pobrane, KolorAkcjiOkna),
                                             Sub() OtworzFolder(FolderDownload))
        UsunPobraneToolStripMenuItem.Text = "Usuń pobrane pliki sesji... (do Kosza)"

        sesja.Clear()
        sesja.AddRange(New ToolStripItem() {
            _menuNowaSesja, WczytajToolStripMenuItem1, _menuOstatnie, ZapiszToolStripMenuItem, New ToolStripSeparator(),
            zaznacz, wspolrzedne, KorytarzToolStripMenuItem, poligon, UsunKorytarzToolStripMenuItem, siatka, New ToolStripSeparator(),
            _menuPobierz, OdswiezToolStripMenuItem, EksportToolStripMenuItem, New ToolStripSeparator(),
            folderSesji, pobrane, UsunPobraneToolStripMenuItem})
        AddHandler SesjaToolStripMenuItem.DropDownOpening,
            Sub()
                Dim trwa = _przerwanie IsNot Nothing
                _menuPobierz.Text = If(trwa, "Przerwij pobieranie", If(btnPobierz.Text <> "", btnPobierz.Text, "Pobierz segmenty"))
                zaznacz.Text = If(_trybZaznaczania, "Anuluj zaznaczanie", "Zaznacz obszar na mapie")
                siatka.Enabled = BiezacaSiatka().Poprawna
            End Sub

        ' Dwa konwertery robią to samo dla dwóch rodzajów kalibracji - jedno podmenu zamiast dwóch pozycji obok siebie (ER-28).
        Dim narzedzia = NarzedziaToolStripMenuItem.DropDownItems
        Dim miejsce = narzedzia.IndexOf(KonwertujOziToolStripMenuItem)
        narzedzia.Remove(KonwertujOziToolStripMenuItem)
        narzedzia.Remove(KonwertujGeoTiffToolStripMenuItem)
        KonwertujOziToolStripMenuItem.Text = "Skan OziExplorer z plikiem kalibracji (.map)..."
        KonwertujGeoTiffToolStripMenuItem.Text = "Mapa GeoTIFF (.tif)..."
        Dim konwertery As New ToolStripMenuItem("Konwertuj gotową mapę") With {
            .Name = "KonwertujMapeToolStripMenuItem",
            .ToolTipText = "Zamienia gotową, skalibrowaną mapę (OziExplorer .map albo GeoTIFF) na JNX, KMZ, MBTiles, GeoPackage, SQLiteDB lub OruxMaps"}
        konwertery.DropDownItems.AddRange({KonwertujOziToolStripMenuItem, KonwertujGeoTiffToolStripMenuItem})
        narzedzia.Insert(Math.Max(0, miejsce), konwertery)

        MenuStrip1.Items.Remove(UstawieniaToolStripMenuItem)
        UstawieniaToolStripMenuItem.Text = "Ustawienia..."
        UstawieniaToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.Oemcomma
        UstawieniaToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+,"
        NarzedziaToolStripMenuItem.DropDownItems.Add(New ToolStripSeparator())
        NarzedziaToolStripMenuItem.DropDownItems.Add(UstawieniaToolStripMenuItem)
    End Sub

    ''' <summary>„Pobierz segmenty” z menu (F5): to samo co przycisk w karcie „Stan sesji”, a w trakcie pobierania - przerwanie.</summary>
    Private Sub PobierzZMenu()
        If _przerwanie IsNot Nothing Then
            btnPrzerwij_Click(btnPrzerwij, EventArgs.Empty)
            Return
        End If
        If _zestaw.Liczba = 0 Then
            PokazBladSesji("Najpierw wybierz warstwę w katalogu warstw po prawej.", "Brak warstwy")
        ElseIf Not BiezacaSiatka().Poprawna Then
            PokazBladSesji("Najpierw zaznacz obszar na mapie albo wpisz współrzędne (Obszar pobierania → Zmień ▾).", "Brak obszaru")
        Else
            btnPobierz_Click(btnPobierz, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>Podmenu „Ostatnie sesje”: foldery z conf.txt w folderze pobrań, od najnowszej.</summary>
    Private Sub WypelnijOstatnieSesje()
        _menuOstatnie.DropDownItems.Clear()
        Dim foldery = KonfiguracjaSesji.OstatnieFolderySesji(FolderDownload, 10)
        If foldery.Count = 0 Then
            _menuOstatnie.DropDownItems.Add(New ToolStripMenuItem("(brak zapisanych sesji)") With {.Enabled = False})
            Return
        End If
        For Each folder In foldery
            Dim f = folder
            Dim nazwa = NazwaPlikuEksportu.NazwaSesji(f)
            Dim data = File.GetLastWriteTime(Path.Combine(f, "conf.txt")).ToString("d MMM HH:mm", _kulturaPl)
            Dim pozycja As New ToolStripMenuItem(nazwa & "   (" & data & ")", Nothing,
                                                 Sub()
                                                     If PobieranieTrwa("Wczytanie innej sesji") Then Return
                                                     WczytajConfZFolderu(f, wybranyPrzezUzytkownika:=True)
                                                 End Sub) With {.ToolTipText = f}
            If String.Equals(f.TrimEnd("\"c), If(FolderSesji, "").TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then pozycja.Checked = True
            _menuOstatnie.DropDownItems.Add(pozycja)
        Next
    End Sub

    ''' <summary>Wysokość nagłówka kart prawego panelu (pas z ikoną, tytułem i akcją karty).</summary>
    Private ReadOnly Property WysokoscNaglowka As Integer
        Get
            Return Px(32)
        End Get
    End Property

    Private Shared ReadOnly TloNaglowka As Color = Color.FromArgb(236, 241, 247)

    ''' <summary>
    ''' Sekcja panelu warstw jako karta: ramka jak w kartach pod mapą, nagłówek na przyciemnionym pasie z ikoną,
    ''' tytułem, opcjonalnym dopiskiem i akcją karty po prawej (dawniej osobne przyciski zajmowały wiersz w karcie).
    ''' Białe sekcje bez ramek i nagłówków słabo odróżniały się od siebie. Kontrolki wewnątrz są osobnymi oknami,
    ''' więc rysunek ich nie zasłania.
    ''' </summary>
    Private Sub RysujJakKarte(sekcja As GroupBox, ikona As IkonaOkna, akcja As Button, dopisek As Func(Of String))
        sekcja.Margin = New Padding(0, 0, 0, Px(6))
        ' GroupBox zaczyna zawartość pod wysokością czcionki tytułu - dopełnienie przesuwa ją pod pas nagłówka.
        sekcja.Padding = New Padding(1, Math.Max(0, WysokoscNaglowka + 1 - sekcja.Font.Height), 1, 1)
        If akcja IsNot Nothing Then
            StylPrzycisku(akcja)
            akcja.AutoSize = True
            akcja.MinimumSize = New Size(Px(29), Px(24))
            akcja.MaximumSize = New Size(0, Px(24))
            akcja.Padding = New Padding(Px(4), 0, Px(4), 0)
            akcja.Dock = DockStyle.None
            akcja.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            akcja.Margin = Padding.Empty
            If akcja.Parent IsNot sekcja Then
                akcja.Parent?.Controls.Remove(akcja)
                sekcja.Controls.Add(akcja)
            End If
            akcja.BringToFront()
            Dim ustaw As Action = Sub() akcja.Location = New Point(sekcja.Width - akcja.Width - WciecieSekcji, (WysokoscNaglowka - akcja.Height) \ 2 + 1)
            AddHandler sekcja.Resize, Sub() ustaw()
            AddHandler akcja.SizeChanged, Sub() ustaw()
            ustaw()
        End If
        AddHandler sekcja.Resize, Sub() sekcja.Invalidate()
        AddHandler sekcja.Paint,
            Sub(s, e)
                Dim g = e.Graphics
                g.Clear(Color.White)
                Using naglowek As New SolidBrush(TloNaglowka)
                    g.FillRectangle(naglowek, 0, 0, sekcja.Width, WysokoscNaglowka)
                End Using
                Using pioro As New Pen(KolorRamkiKarty)
                    g.DrawLine(pioro, 0, WysokoscNaglowka, sekcja.Width, WysokoscNaglowka)
                    g.DrawRectangle(pioro, 0, 0, sekcja.Width - 1, sekcja.Height - 1)
                End Using
                Dim bok = Px(16)
                RysujIkone(g, ikona, New RectangleF(WciecieSekcji + 1, (WysokoscNaglowka - bok) / 2.0F, bok, bok), KolorAkcjiOkna)
                Dim x = WciecieSekcji + bok + Px(8)
                Dim prawa = If(akcja IsNot Nothing AndAlso akcja.Visible, akcja.Left - Px(6), sekcja.Width - WciecieSekcji)
                Dim wysokosc = WysokoscNaglowka
                TextRenderer.DrawText(g, sekcja.Text, _czcionkaNaglowka, New Rectangle(x, 0, Math.Max(0, prawa - x), wysokosc), Atrament,
                                      TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine)
                Dim tekst = dopisek?.Invoke()
                If Not String.IsNullOrEmpty(tekst) Then
                    Dim szer = TextRenderer.MeasureText(g, sekcja.Text, _czcionkaNaglowka, Size.Empty, TextFormatFlags.NoPrefix).Width
                    Dim xd = x + szer + Px(4)
                    Dim tekstDopisku = "· " & tekst
                    ' Dopisek tylko w całości - ucięty „↑ na wie…” mówiłby mniej niż żaden.
                    If xd + TextRenderer.MeasureText(g, tekstDopisku, _czcionkaRobocza).Width <= prawa Then
                        TextRenderer.DrawText(g, tekstDopisku, _czcionkaRobocza, New Rectangle(xd, 0, prawa - xd, wysokosc), KolorPomocniczy,
                                              TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine)
                    End If
                End If
            End Sub
    End Sub

    ''' <summary>
    ''' Uchwyty „═” w odstępach między kartami prawego panelu: przeciągnięcie zmienia wysokość listy wybranych warstw
    ''' i zakładek kosztem katalogu (albo odwrotnie), dwuklik przywraca podział domyślny. Program zapamiętuje wysokości.
    ''' </summary>
    Private Sub UstawUchwytyPanelu()
        AddHandler _prawyUklad.Paint,
            Sub(s, e)
                For Each granica In {1, 2}
                    Dim y = SrodekGranicy(granica)
                    Dim x = _prawyUklad.Width \ 2
                    Using pioro As New Pen(Color.FromArgb(150, 165, 182))
                        e.Graphics.DrawLine(pioro, x - Px(12), y - 1, x + Px(12), y - 1)
                        e.Graphics.DrawLine(pioro, x - Px(12), y + 1, x + Px(12), y + 1)
                    End Using
                Next
            End Sub
        AddHandler _prawyUklad.MouseMove,
            Sub(s, e)
                If _przeciaganaGranica > 0 AndAlso e.Button = MouseButtons.Left Then
                    PrzesunGranice(e.Y - _poczatekPrzeciagania)
                    Return
                End If
                _prawyUklad.Cursor = If(GranicaPod(e.Y) > 0, Cursors.HSplit, Cursors.Default)
            End Sub
        AddHandler _prawyUklad.MouseDown,
            Sub(s, e)
                Dim granica = GranicaPod(e.Y)
                If e.Button <> MouseButtons.Left OrElse granica = 0 Then Return
                _przeciaganaGranica = granica
                _poczatekPrzeciagania = e.Y
                _wysokosciPrzedPrzeciaganiem = (CInt(_prawyUklad.RowStyles(1).Height), CInt(_prawyUklad.RowStyles(2).Height))
            End Sub
        AddHandler _prawyUklad.MouseUp, Sub() _przeciaganaGranica = 0
        AddHandler _prawyUklad.MouseLeave, Sub() If _przeciaganaGranica = 0 Then _prawyUklad.Cursor = Cursors.Default
        AddHandler _prawyUklad.MouseDoubleClick,
            Sub(s, e)
                If GranicaPod(e.Y) = 0 Then Return
                Ustawienia.WysokoscWybranych = 0
                Ustawienia.WysokoscZakladek = 0
                DopasujSekcjeWarstw()
            End Sub
        ToolTip1.SetToolTip(_prawyUklad, "Przeciągnij uchwyt, aby zmienić wysokość kart; dwuklik przywraca podział domyślny.")
    End Sub

    Private Function SrodekGranicy(granica As Integer) As Integer
        Dim gora = If(granica = 1, grpDaneZrodlowe.Bottom, grpWybraneWarstwy.Bottom)
        Dim dol = If(granica = 1, grpWybraneWarstwy.Top, _zakladkiPracy.Top)
        Return (gora + dol) \ 2
    End Function

    ''' <summary>Numer granicy między kartami pod punktem (odstęp między kartami z zapasem 2 px); 0 - poza granicami.</summary>
    Private Function GranicaPod(y As Integer) As Integer
        If grpDaneZrodlowe.Bottom - Px(2) <= y AndAlso y <= grpWybraneWarstwy.Top + Px(2) Then Return 1
        If grpWybraneWarstwy.Bottom - Px(2) <= y AndAlso y <= _zakladkiPracy.Top + Px(2) Then Return 2
        Return 0
    End Function

    Private Sub PrzesunGranice(przesuniecie As Integer)
        Dim wybrane = _wysokosciPrzedPrzeciaganiem.Wybrane, zakladki = _wysokosciPrzedPrzeciaganiem.Zakladki
        If _przeciaganaGranica = 1 Then
            wybrane -= przesuniecie
        Else
            wybrane += przesuniecie
            zakladki -= przesuniecie
        End If
        Dim wolne = _prawyUklad.ClientSize.Height - _prawyUklad.Padding.Vertical
        Dim minKatalog = Px(110), minWybrane = Px(70), minZakladki = Px(150)
        wybrane = Math.Max(minWybrane, wybrane)
        zakladki = Math.Max(minZakladki, zakladki)
        If wybrane + zakladki > wolne - minKatalog Then
            If _przeciaganaGranica = 1 Then wybrane = wolne - minKatalog - zakladki Else zakladki = wolne - minKatalog - wybrane
        End If
        If wybrane < minWybrane OrElse zakladki < minZakladki Then Return
        Ustawienia.WysokoscWybranych = CInt(wybrane / (DeviceDpi / 96.0))
        Ustawienia.WysokoscZakladek = CInt(zakladki / (DeviceDpi / 96.0))
        DopasujSekcjeWarstw()
    End Sub

    ''' <summary>Zakładki rysowane w stylu kart: wybrana biała z paskiem w kolorze akcji, pozostałe w kolorze tła panelu.</summary>
    Private Sub UstawWygladZakladek()
        For Each strona As TabPage In _zakladkiPracy.TabPages
            strona.UseVisualStyleBackColor = False
            strona.BackColor = Color.White
        Next
        DirectCast(_zakladkiPracy, PlaskieZakladki).RysujZakladke =
            Sub(g, indeks, r, wybrana, najechana)
                Dim strona = _zakladkiPracy.TabPages(indeks)
                Dim bledy = indeks = 1 AndAlso _liczbaBledowSesji > 0
                Dim kolor = If(bledy, Color.Firebrick, If(wybrana, Atrament, KolorPomocniczy))
                Dim czcionka = If(wybrana OrElse bledy, _czcionkaNaglowka, _czcionkaRobocza)
                ' Ikona przed napisem; razem wyśrodkowane w zakładce. Przy wąskiej zakładce zostaje sam napis.
                Dim bok = Px(15), odstep = Px(6)
                Dim szerNapisu = TextRenderer.MeasureText(g, strona.Text, czcionka, Size.Empty, TextFormatFlags.NoPrefix).Width
                Dim zIkona = szerNapisu + bok + odstep <= r.Width - Px(8)
                Dim x = r.Left + (r.Width - szerNapisu - If(zIkona, bok + odstep, 0)) \ 2
                If zIkona Then
                    Dim ikony = {IkonaOkna.Podglad, IkonaOkna.Komunikaty, IkonaOkna.Sesja}
                    RysujIkone(g, ikony(Math.Min(indeks, 2)), New RectangleF(x, r.Top + (r.Height - bok) / 2.0F + 1, bok, bok),
                               If(bledy, Color.Firebrick, If(wybrana, KolorAkcjiOkna, KolorPomocniczy)))
                    x += bok + odstep
                End If
                TextRenderer.DrawText(g, strona.Text, czcionka, New Rectangle(x, r.Top, r.Right - x, r.Height), kolor,
                                      TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine)
            End Sub
    End Sub

    Private Sub DopasujSzerokoscZakladek()
        If _dopasowywanieZakladek OrElse _zakladkiPracy.ClientSize.Width < Px(60) Then Return
        ' Ustawienie ItemSize chwilowo zmienia rozmiar samego TabControl i ponownie zgłasza ClientSizeChanged.
        _dopasowywanieZakladek = True
        Try
            Dim szerokosc = (_zakladkiPracy.ClientSize.Width - Px(6)) \ _zakladkiPracy.TabCount
            Dim rozmiar As New Size(szerokosc, Px(28))
            If _zakladkiPracy.ItemSize <> rozmiar Then _zakladkiPracy.ItemSize = rozmiar
            Dim napis = "Komunikaty"
            If _liczbaBledowSesji > 0 Then
                napis &= $" ({_liczbaBledowSesji})"
                _zakladkaKomunikatow.ToolTipText = napis
            Else
                _zakladkaKomunikatow.ToolTipText = ""
            End If
            _zakladkaKomunikatow.Text = If(TextRenderer.MeasureText(napis, _czcionkaNaglowka).Width < szerokosc, napis, "Komunikaty")
            _zakladkiPracy.Invalidate()
            OdswiezPasekSesji()
        Finally
            _dopasowywanieZakladek = False
        End Try
    End Sub

    Private Sub UlozKatalogWarstw()
        Dim uklad As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 2, .Dock = DockStyle.Fill, .Margin = Padding.Empty,
                                                .Padding = New Padding(WciecieSekcji, Px(7), WciecieSekcji, WciecieSekcji)}
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, Px(34)))
        uklad.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Dim filtr As New TableLayoutPanel With {.ColumnCount = 3, .RowCount = 1, .Margin = Padding.Empty}
        filtr.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        ' Przyciski z napisami zamiast samych symboli „⊟” i „+”, których znaczenia nie było widać bez podpowiedzi.
        ' „+ Serwer…” jest akcją karty w jej nagłówku, więc pole filtru zyskuje szerokość.
        filtr.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, Px(62)))
        filtr.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, Px(34)))
        filtr.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        btnDodajSerwer.Text = "+ Serwer…"

        grpDaneZrodlowe.Controls.Remove(txtSzukajWarstwy)
        Dim pnlSzukaj As New Panel With {.Name = "pnlSzukajWarstwy", .BackColor = Color.White, .Height = Px(28),
                                         .Padding = New Padding(Px(6), 1, Px(2), 1),
                                         .Margin = New Padding(0, 0, 0, Px(6))}
        Dim ikonaSzukaj As New Label With {.Name = "btnWyczyscSzukajWarstwy", .Dock = DockStyle.Right,
                                           .Width = Px(22), .TextAlign = ContentAlignment.MiddleCenter,
                                           .Cursor = Cursors.Hand, .BackColor = Color.White,
                                           .Font = _czcionkaRobocza, .AccessibleRole = AccessibleRole.PushButton}
        Dim lupaWarstw = UtworzIkoneLupy(16)
        _zasobyUkladu.Add(lupaWarstw)
        txtSzukajWarstwy.Font = _czcionkaRobocza
        txtSzukajWarstwy.PlaceholderText = "Szukaj warstwy…"
        txtSzukajWarstwy.BorderStyle = BorderStyle.None
        txtSzukajWarstwy.BackColor = Color.White
        txtSzukajWarstwy.Anchor = AnchorStyles.None
        txtSzukajWarstwy.Dock = DockStyle.None
        txtSzukajWarstwy.Margin = Padding.Empty
        pnlSzukaj.Controls.Add(txtSzukajWarstwy)
        pnlSzukaj.Controls.Add(ikonaSzukaj)
        Dim odswiezIkoneSzukania As Action =
            Sub()
                If txtSzukajWarstwy.TextLength = 0 Then
                    ikonaSzukaj.Image = lupaWarstw
                    ikonaSzukaj.Text = ""
                    ToolTip1.SetToolTip(ikonaSzukaj, "Wpisz fragment nazwy warstwy lub zbioru map")
                Else
                    ikonaSzukaj.Image = Nothing
                    ikonaSzukaj.Text = "✕"
                    ikonaSzukaj.ForeColor = KolorPomocniczy
                    ToolTip1.SetToolTip(ikonaSzukaj, "Wyczyść filtr (Esc)")
                End If
            End Sub
        odswiezIkoneSzukania()
        AddHandler txtSzukajWarstwy.TextChanged, Sub() odswiezIkoneSzukania()
        AddHandler txtSzukajWarstwy.GotFocus, Sub() pnlSzukaj.Invalidate()
        AddHandler txtSzukajWarstwy.LostFocus, Sub() pnlSzukaj.Invalidate()
        AddHandler txtSzukajWarstwy.EnabledChanged,
            Sub()
                Dim tlo = If(txtSzukajWarstwy.Enabled, Color.White, SystemColors.Control)
                pnlSzukaj.BackColor = tlo
                txtSzukajWarstwy.BackColor = tlo
                ikonaSzukaj.BackColor = tlo
                pnlSzukaj.Invalidate()
            End Sub
        AddHandler ikonaSzukaj.Click,
            Sub()
                If txtSzukajWarstwy.TextLength > 0 AndAlso txtSzukajWarstwy.Enabled Then txtSzukajWarstwy.Clear()
                If txtSzukajWarstwy.Enabled Then txtSzukajWarstwy.Focus()
            End Sub
        AddHandler pnlSzukaj.Click, Sub() If txtSzukajWarstwy.Enabled Then txtSzukajWarstwy.Focus()
        AddHandler pnlSzukaj.Layout,
            Sub()
                Dim lewa = pnlSzukaj.Padding.Left
                Dim prawa = ikonaSzukaj.Left
                txtSzukajWarstwy.SetBounds(lewa, Math.Max(0, (pnlSzukaj.ClientSize.Height - txtSzukajWarstwy.Height) \ 2),
                                           Math.Max(10, prawa - lewa - 2), txtSzukajWarstwy.Height)
            End Sub
        AddHandler pnlSzukaj.Resize, Sub() pnlSzukaj.Invalidate()
        AddHandler pnlSzukaj.Paint,
            Sub(s, e)
                Dim kolorRamki = If(txtSzukajWarstwy.Focused, KolorAkcjiOkna, Color.FromArgb(160, 172, 186))
                Using pioro As New Pen(kolorRamki)
                    e.Graphics.DrawRectangle(pioro, 0, 0, pnlSzukaj.Width - 1, pnlSzukaj.Height - 1)
                End Using
            End Sub

        tvWarstwy.Font = _czcionkaRobocza
        tvWarstwy.ItemHeight = _czcionkaRobocza.Height + Px(5)
        tvWarstwy.ShowLines = False
        tvWarstwy.BorderStyle = BorderStyle.None
        StylPrzycisku(btnZwinRozwin)
        btnZwinRozwin.MinimumSize = New Size(0, Px(28))
        btnZwinRozwin.Padding = New Padding(Px(2), 0, Px(2), 0)
        btnZwinRozwin.Margin = New Padding(Px(4), 0, 0, Px(6))
        StylPrzycisku(btnGaleriaWarstw)
        btnGaleriaWarstw.Image = ZarejestrujIkone(IkonaOkna.Galeria, KolorAkcjiOkna)
        btnGaleriaWarstw.MinimumSize = New Size(0, Px(28))
        btnGaleriaWarstw.Padding = Padding.Empty
        btnGaleriaWarstw.Margin = New Padding(Px(4), 0, 0, Px(6))
        DolozKomorke(filtr, pnlSzukaj, 0, 0)
        DolozKomorke(filtr, btnZwinRozwin, 1, 0)
        DolozKomorke(filtr, btnGaleriaWarstw, 2, 0)
        DolozKomorke(uklad, filtr, 0, 0)
        DolozKomorke(uklad, tvWarstwy, 0, 1)
        filtr.Margin = Padding.Empty
        tvWarstwy.Margin = Padding.Empty
        Label29.Visible = False
        ' „Katalog warstw”, a nie „Warstwy do pobrania” - pobierane są warstwy z karty „Wybrane warstwy” (audyt ER-08)
        grpDaneZrodlowe.Text = "Katalog warstw"
        grpDaneZrodlowe.Font = _czcionkaNaglowka
        grpDaneZrodlowe.ForeColor = Atrament
        grpDaneZrodlowe.BackColor = Color.White
        grpDaneZrodlowe.Controls.Add(uklad)
        uklad.BringToFront()
    End Sub

    Private Sub UlozWybraneWarstwy()
        Dim uklad As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 1, .Dock = DockStyle.Fill, .Margin = Padding.Empty,
                                                .Padding = New Padding(WciecieSekcji - Px(2), Px(3), WciecieSekcji - Px(2), Px(4))}
        _ukladKompozycji = uklad
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        uklad.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        ' Krycie i usuwanie są w wierszu warstwy, kolejność zmienia przeciąganie (także Ctrl+↑/↓ i menu warstwy),
        ' a mikser jest akcją karty w nagłówku - osobny wiersz ▲ ▼ ✕ „Krycie %” „Mikser…” pod listą zabierał miejsce,
        ' a krycie było widać dwa razy (przy warstwie i w polu pod listą).
        For Each zbedna As Control In New Control() {btnWarstwaWyzej, btnWarstwaNizej, btnUsunWarstwe, lblKrycie}
            zbedna.Visible = False
        Next
        numKrycie.Font = _czcionkaRobocza
        numKrycie.TextAlign = HorizontalAlignment.Right
        numKrycie.Width = Px(48)
        numKrycie.AccessibleName = "Krycie zaznaczonej warstwy [%]"
        lstWybraneWarstwy.Font = _czcionkaRobocza
        lstWybraneWarstwy.EdytorKrycia = numKrycie
        ' Napis zamiast samych kropek „…”, które nie mówiły, że przycisk otwiera mikser warstw.
        btnWarstwyZaawansowane.Text = "Mikser…"
        ToolTip1.SetToolTip(btnWarstwyZaawansowane, "Mikser warstw: krycie, kolejność i tryby mieszania z podglądem na żywo")
        grpWybraneWarstwy.Font = _czcionkaNaglowka
        grpWybraneWarstwy.ForeColor = Atrament
        grpWybraneWarstwy.BackColor = Color.White
        DolozKomorke(uklad, lstWybraneWarstwy, 0, 0)
        lstWybraneWarstwy.Margin = Padding.Empty
        grpWybraneWarstwy.Controls.Add(uklad)
        uklad.BringToFront()
    End Sub

    Private Sub DopasujSekcjeWarstw()
        If _prawyUklad Is Nothing Then Return
        Dim liczba = _zestaw.Liczba
        grpWybraneWarstwy.Text = If(liczba = 0, "Wybrane warstwy", $"Wybrane warstwy ({liczba})")
        grpWybraneWarstwy.Invalidate()
        Dim wysokosc = Math.Max(Panel2.ClientSize.Height, _minWysokoscPanelu)
        _prawyUklad.Height = wysokosc
        ' Lista wybranych warstw i zakładki z podglądem dostają ok. połowy wysokości panelu, a katalog resztę:
        ' przy niższym podziale podgląd był za mały, a lista mieściła tylko 2-4 warstwy.
        Dim wiersze = Math.Min(liczba, If(wysokosc < Px(485), 3, 6))
        Dim stalaCzesc = WysokoscNaglowka + Px(14) + _ukladKompozycji.Padding.Vertical
        Dim wybrane = If(liczba = 0, WysokoscNaglowka + Px(38), stalaCzesc + wiersze * lstWybraneWarstwy.WysokoscWiersza)
        Dim zakladki = Math.Clamp(CInt(wysokosc * 0.4), Px(190), Px(420))
        ' Wysokości ustawione uchwytami między kartami (zapisane bez skalowania ekranu); katalog zachowuje co najmniej 110 px.
        If Ustawienia.WysokoscWybranych > 0 AndAlso liczba > 0 Then wybrane = Px(Ustawienia.WysokoscWybranych)
        If Ustawienia.WysokoscZakladek > 0 Then zakladki = Px(Ustawienia.WysokoscZakladek)
        Dim wolne = wysokosc - _prawyUklad.Padding.Vertical - Px(110)
        If wybrane + zakladki > wolne Then
            zakladki = Math.Max(Px(150), wolne - wybrane)
            wybrane = Math.Max(Px(70), wolne - zakladki)
        End If
        _prawyUklad.RowStyles(1).Height = wybrane
        _prawyUklad.RowStyles(2).Height = zakladki
        _prawyUklad.Invalidate()
        If _pustyPodglad IsNot Nothing Then
            _pustyPodglad.Visible = liczba = 0
            _pasekPodgladu.Visible = liczba > 0
        End If
    End Sub

    Private Sub UlozWejsciaMapy()
        _przyciskZaznaczenia = New Button With {.Name = "btnZaznaczDwaNarozniki", .Text = "Zaznacz obszar"}
        AddHandler _przyciskZaznaczenia.Click, Sub() PrzelaczZaznaczanieObszaru()
        ' Jedno narzędzie wyszukiwania: pole z lupą i listą wyników pod nim. Osobny przycisk lupy otwierał drugie okno
        ' z tym samym wyszukiwaniem; to okno zostaje w menu Narzędzia (i pod Ctrl+D) dla wyszukiwania działek.
        Dim lupaPaska = UtworzIkoneLupy(Px(16))
        _zasobyUkladu.Add(lupaPaska)
        _poleSzukaniaMiejsca = New PoleWyszukiwaniaMiejsca(lupaPaska, _czcionkaRobocza) With {
            .Name = "poleSzukaniaMiejsca", .Size = New Size(Px(200), Px(28)),
            .SrodekMapy = Function() (mapa.Position.Lat, mapa.Position.Lng)}
        ToolTip1.SetToolTip(_poleSzukaniaMiejsca.Pole, "Wpisz miejscowość, adres, działkę (np. działka Wieliczka 123/4) " &
                            "albo współrzędne. Podpowiedzi pojawiają się w trakcie pisania, Enter wybiera wynik (Ctrl+F przenosi tu kursor).")
        AddHandler _poleSzukaniaMiejsca.Wybrano, AddressOf PokazWynikWyszukiwania
        btnSzukajDzialki.Visible = False
        ' Wpisywanie współrzędnych jest w menu „Zmień ▾” karty „Obszar pobierania”, obok innych sposobów wskazania obszaru.
        btnWpiszWspolrzedne.Visible = False
        _ikonaZaznaczeniaNormalna = ZarejestrujIkone(IkonaOkna.Obszar, KolorAkcjiOkna)
        _ikonaZaznaczeniaAktywna = ZarejestrujIkone(IkonaOkna.Obszar, Color.White)
        _przyciskZaznaczenia.Image = _ikonaZaznaczeniaNormalna
        _przyciskZaznaczenia.TextImageRelation = TextImageRelation.ImageBeforeText
        btnKreatorDolny.Text = "Szybki kreator…"
        Dim ikonaKreatora = UtworzIkoneKreatora(Px(16))
        _zasobyUkladu.Add(ikonaKreatora)
        btnKreatorDolny.Image = ikonaKreatora
        btnKreatorDolny.TextImageRelation = TextImageRelation.ImageBeforeText
        lblZoom.Visible = False
        btnPowieksz.Text = "+"
        btnPomniejsz.Text = "−"
        ' Kółko myszy przybliża w miejscu kursora bez przenoszenia kursora na środek mapy (tryb MousePositionAndCenter
        ' z projektu formularza przestawiał kursor na krzyżyk środka przy każdym obrocie kółka).
        mapa.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter
        btnPowieksz.AccessibleName = "Przybliż mapę"
        btnPomniejsz.AccessibleName = "Oddal mapę"
        _polePodkladuMapy = New ComboBox With {
            .Name = "cmbPodkladMapy",
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = Px(160),
            .Font = _czcionkaRobocza,
            .AccessibleName = "Podkład mapy podglądu"
        }
        _polePodkladuMapy.Items.AddRange({"OpenStreetMap", "Google Maps", "Bing — zdjęcia lotnicze"})
        ToolTip1.SetToolTip(_polePodkladuMapy, "Podkład mapy podglądu (OpenStreetMap, Google Maps, Bing). Nie jest pobierany jako warstwa sesji.")
        Dim poprzedniPodklad() As RadioButton = {rbOsm, rbGoogle, rbBing}
        AddHandler _polePodkladuMapy.SelectedIndexChanged,
            Sub()
                If _polePodkladuMapy.SelectedIndex >= 0 Then poprzedniPodklad(_polePodkladuMapy.SelectedIndex).Checked = True
            End Sub
        For i = 0 To poprzedniPodklad.Length - 1
            Dim indeks = i
            Dim opcja = poprzedniPodklad(indeks)
            opcja.Visible = False
            AddHandler opcja.CheckedChanged, Sub()
                                                If opcja.Checked Then _polePodkladuMapy.SelectedIndex = indeks
                                            End Sub
            If opcja.Checked Then _polePodkladuMapy.SelectedIndex = indeks
        Next
        Dim folderPobran As New Button With {.Name = "btnFolderPobran", .Text = "Folder pobrań",
                                            .Image = ZarejestrujIkone(IkonaOkna.Pobrane, KolorAkcjiOkna), .TextImageRelation = TextImageRelation.ImageBeforeText}
        AddHandler folderPobran.Click, Sub() OtworzFolder(FolderDownload)
        ToolTip1.SetToolTip(folderPobran, "Otwórz folder download ze wszystkimi pobranymi sesjami.")
        _btnNowaSesja.Text = "Nowa sesja"
        _btnNowaSesja.TextImageRelation = TextImageRelation.ImageBeforeText
        ' Wszystkie elementy paska mają wysokość przycisku i wspólną linię środka. Przełącznik podkładu podglądu leży
        ' w prawym górnym rogu mapy, oddzielony od paska narzędzi i panelu pobieranych warstw.
        Dim wysokosc = Px(30)
        For Each przycisk In {_btnNowaSesja, btnKreatorDolny, folderPobran, _przyciskZaznaczenia}
            StylPrzycisku(przycisk)
            przycisk.MinimumSize = New Size(Px(29), wysokosc)
            przycisk.MaximumSize = New Size(0, wysokosc)
            przycisk.Margin = New Padding(0, 0, OdstepWGrupie, 0)
        Next
        btnKreatorDolny.BackColor = TloNaglowka
        btnKreatorDolny.ForeColor = KolorAkcjiOkna
        btnKreatorDolny.Font = _czcionkaNaglowka
        btnKreatorDolny.FlatAppearance.BorderColor = Color.FromArgb(130, 165, 200)
        btnKreatorDolny.FlatAppearance.MouseOverBackColor = TloBelki
        btnKreatorDolny.FlatAppearance.MouseDownBackColor = Color.FromArgb(208, 220, 234)
        ' Trzy grupy z równymi odstępami: sesja (nowa, kreator), obszar (zaznaczenie, wyszukiwanie), folder pobrań na końcu
        ' - otwiera się go dopiero po pracy. Wewnątrz grupy 4 px, między grupami 16 px (dawniej odstępy od 2 do 14 px).
        btnKreatorDolny.Margin = New Padding(0, 0, OdstepGrup, 0)
        _poleSzukaniaMiejsca.Height = wysokosc
        _poleSzukaniaMiejsca.Margin = New Padding(0, 0, OdstepGrup, 0)
        Dim lewaCzesc = PasekPrzyciskow(_btnNowaSesja, btnKreatorDolny, _przyciskZaznaczenia, _poleSzukaniaMiejsca, folderPobran)
        lewaCzesc.WrapContents = True
        AddHandler lewaCzesc.Paint,
            Sub(s, e)
                Using pioro As New Pen(KolorRamkiKarty)
                    For Each para In {(DirectCast(btnKreatorDolny, Control), DirectCast(_przyciskZaznaczenia, Control)),
                                      (DirectCast(_poleSzukaniaMiejsca, Control), DirectCast(folderPobran, Control))}
                        Dim lewa = para.Item1, prawa = para.Item2
                        If Not lewa.Visible OrElse Not prawa.Visible Then Continue For
                        If Math.Abs(lewa.Top - prawa.Top) >= Px(6) OrElse prawa.Left <= lewa.Right Then Continue For
                        Dim x = (lewa.Right + prawa.Left) \ 2
                        e.Graphics.DrawLine(pioro, x, lewa.Top + Px(5), x, lewa.Bottom - Px(5))
                    Next
                End Using
            End Sub
        AddHandler lewaCzesc.Layout, Sub() lewaCzesc.Invalidate()
        Dim pasek As New TableLayoutPanel With {.Name = "pasekNarzedzi", .ColumnCount = 1, .RowCount = 1, .Dock = DockStyle.Top,
                                                .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .BackColor = TloPaneli}
        pasek.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        pasek.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        pasek.Controls.Add(lewaCzesc, 0, 0)
        lewaCzesc.Dock = DockStyle.Fill
        Dim dopasujKrawedzie As Action =
            Sub()
                Dim prawy = Math.Max(0, ClientSize.Width - Panel2.Right) + _prawyUklad.Padding.Right
                Dim nowy As New Padding(mapa.Left, Px(6), prawy, Px(6))
                If pasek.Padding <> nowy Then pasek.Padding = nowy
            End Sub
        AddHandler Panel2.SizeChanged, Sub() dopasujKrawedzie()
        AddHandler mapa.SizeChanged, Sub() dopasujKrawedzie()
        AddHandler mapa.LocationChanged, Sub() dopasujKrawedzie()
        dopasujKrawedzie()
        Dim przedMenu = Controls.GetChildIndex(MenuStrip1)
        Controls.Add(pasek)
        Controls.SetChildIndex(pasek, przedMenu)
        UlozZoomNaMapie()
        Controls.Add(_polePodkladuMapy)
        _polePodkladuMapy.BringToFront()
        AddHandler mapa.SizeChanged, Sub() UstawPozycjePodkladuMapy()
        AddHandler mapa.LocationChanged, Sub() UstawPozycjePodkladuMapy()
        UstawPozycjePodkladuMapy()
        _banerZaznaczania = New Label With {
            .Name = "lblBanerZaznaczania", .AutoSize = True, .Visible = False,
            .BackColor = KolorAkcjiOkna, .ForeColor = Color.White, .Font = _czcionkaNaglowka,
            .Padding = New Padding(Px(12), Px(6), Px(12), Px(6))
        }
        Controls.Add(_banerZaznaczania)
        _banerZaznaczania.BringToFront()
        AddHandler mapa.SizeChanged, Sub() UstawPozycjeBaneraZaznaczania()
        AddHandler mapa.LocationChanged, Sub() UstawPozycjeBaneraZaznaczania()
        AddHandler _banerZaznaczania.SizeChanged, Sub() UstawPozycjeBaneraZaznaczania()
        Label67.Visible = False
    End Sub

    Private Sub UstawPozycjePodkladuMapy()
        If _polePodkladuMapy Is Nothing OrElse mapa Is Nothing Then Return
        _polePodkladuMapy.Location = New Point(mapa.Right - _polePodkladuMapy.Width - Px(10), mapa.Top + Px(10))
    End Sub

    Private Sub UstawPozycjeBaneraZaznaczania()
        If _banerZaznaczania Is Nothing Then Return
        Dim lewaWolna = mapa.Left + Px(54)
        Dim prawaWolna = If(_polePodkladuMapy IsNot Nothing AndAlso _polePodkladuMapy.Visible, _polePodkladuMapy.Left - Px(8), mapa.Right - Px(10))
        Dim dostepna = prawaWolna - lewaWolna
        Dim x = If(dostepna > _banerZaznaczania.Width, lewaWolna + (dostepna - _banerZaznaczania.Width) \ 2,
                   mapa.Left + Math.Max(Px(54), (mapa.Width - _banerZaznaczania.Width) \ 2))
        Dim y = mapa.Top + Px(10)
        _banerZaznaczania.Location = New Point(x, y)
    End Sub

    Private ReadOnly Property OdstepWGrupie As Integer
        Get
            Return Px(4)
        End Get
    End Property

    Private ReadOnly Property OdstepGrup As Integer
        Get
            Return Px(16)
        End Get
    End Property

    ''' <summary>Włącza zaznaczanie obszaru dwoma kliknięciami na mapie albo je anuluje.</summary>
    Private Sub PrzelaczZaznaczanieObszaru()
        If PobieranieTrwa("Zaznaczanie obszaru") Then Return
        If _trybZaznaczania Then
            ZakonczWyborNaroznikow()
            Return
        End If
        If Ustawienia.EdycjaXY Then
            PokazBladSesji("Zaznaczanie jest wyłączone, bo włączono ręczne wpisywanie XY. Wpisz współrzędne: Obszar pobierania → Zmień ▾.")
            _podpowiedzZaznaczenia.Text = "Włączona edycja XY · Obszar pobierania → Zmień → Wpisz współrzędne…"
            Return
        End If
        _trybZaznaczania = True
        _przyciskZaznaczenia.Text = "Anuluj zaznaczanie"
        _przyciskZaznaczenia.Font = _czcionkaNaglowka
        _przyciskZaznaczenia.Image = _ikonaZaznaczeniaAktywna
        StylPrzyciskuAkcji(_przyciskZaznaczenia, True)
        _podpowiedzZaznaczenia.Text = "Kliknij pierwszy narożnik obszaru · Esc anuluje"
        If _banerZaznaczania IsNot Nothing Then
            _banerZaznaczania.Text = "Kliknij pierwszy narożnik obszaru  ·  Esc anuluje"
            _banerZaznaczania.Visible = True
            UstawPozycjeBaneraZaznaczania()
            _banerZaznaczania.BringToFront()
        End If
        mapa.CanDragMap = False
        mapa.Cursor = Cursors.Cross
        mapa.Focus()
    End Sub

    ''' <summary>
    ''' Menu „Zmień ▾” karty „Obszar pobierania”: wszystkie sposoby wskazania obszaru w jednym miejscu
    ''' (dawniej przycisk „Współrzędne…” na pasku, trasa w menu Sesja, poligon tylko przez przeciągnięcie pliku).
    ''' </summary>
    Private Function UtworzMenuObszaru() As ContextMenuStrip
        Dim menu As New ContextMenuStrip(components) With {.Font = _czcionkaRobocza, .ShowImageMargin = True}
        Dim pokaz As New ToolStripMenuItem("Pokaż na mapie", Nothing, Sub() PokazObszarNaMapie()) With {.Name = "mnuPokazObszar"}
        Dim zaznacz As New ToolStripMenuItem("Zaznacz na mapie", UtworzIkoneZaznaczenia(), Sub() PrzelaczZaznaczanieObszaru()) With {.Name = "mnuZaznaczObszar"}
        Dim wspolrzedne As New ToolStripMenuItem("Wpisz współrzędne…", UtworzIkoneWspolrzednych(),
                                                 Sub() btnWpiszWspolrzedne_Click(btnWpiszWspolrzedne, EventArgs.Empty)) With {.Name = "mnuWpiszWspolrzedne"}
        Dim trasa As New ToolStripMenuItem("Obszar wzdłuż trasy (GPX, KML, KMZ, GPKG)…", Nothing,
                                           Sub() KorytarzToolStripMenuItem_Click(KorytarzToolStripMenuItem, EventArgs.Empty))
        Dim poligon As New ToolStripMenuItem("Obszar z poligonu (KML, GeoJSON, GPKG)…", Nothing, Sub() WybierzPlikPoligonu())
        Dim bezTrasy As New ToolStripMenuItem("Usuń obszar wzdłuż trasy", Nothing,
                                              Sub() UsunKorytarzToolStripMenuItem_Click(UsunKorytarzToolStripMenuItem, EventArgs.Empty))
        menu.Items.AddRange({pokaz, New ToolStripSeparator(), zaznacz, wspolrzedne, New ToolStripSeparator(), trasa, poligon, bezTrasy})
        AddHandler menu.Opening,
            Sub()
                Dim pobieranie = _przerwanie IsNot Nothing
                pokaz.Enabled = BiezacaSiatka().Poprawna
                For Each poz In {zaznacz, wspolrzedne, trasa, poligon, bezTrasy}
                    poz.Enabled = Not pobieranie
                Next
                trasa.Enabled = trasa.Enabled AndAlso KorytarzToolStripMenuItem.Enabled
                bezTrasy.Visible = _korytarz IsNot Nothing
                zaznacz.Text = If(_trybZaznaczania, "Anuluj zaznaczanie", "Zaznacz na mapie")
            End Sub
        Return menu
    End Function

    Private Sub WybierzPlikPoligonu()
        If PobieranieTrwa("Wczytywanie obszaru") Then Return
        Using okno As New OpenFileDialog With {.Title = "Obszar pobierania z poligonu",
                                               .Filter = "Poligony (*.kml;*.geojson;*.json;*.gpkg)|*.kml;*.geojson;*.json;*.gpkg|Wszystkie pliki|*.*"}
            If okno.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                WczytajObszarPoligonu(okno.FileName)
            Catch ex As Exception
                MsgBox("Nie udało się odczytać poligonu: " & OpisBledu.Z(ex), MsgBoxStyle.Critical, "Obszar poligonu")
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Wynik z pola wyszukiwania: miejsce przesuwa mapę, a działka ustawia obszar pobierania wokół niej (z pasem 50 m,
    ''' jak w oknie wyszukiwarki) i rysuje jej obrys. Warstw działek pole nie włącza - to robi okno wyszukiwarki.
    ''' </summary>
    Private Sub PokazWynikWyszukiwania(wynik As WynikWyszukiwania)
        If PobieranieTrwa("Wyszukiwanie") Then Return
        If wynik.Dzialka IsNot Nothing Then
            UstawDzialke(wynik.Dzialka, 50.0, wlaczWarstwy:=False)
        Else
            UstawMiejsce(wynik.Miejsce)
        End If
        mapa.Focus()
    End Sub

    ''' <summary>
    ''' Podziałka w prawym dolnym rogu mapy (lewy dolny róg zajmuje atrybucja podkładu). Długość odcinka to okrągła
    ''' wartość 1, 2 lub 5 × 10^n metrów, około 100 px, liczona dla szerokości geograficznej środka mapy.
    ''' </summary>
    Private Sub RysujPodzialke(sender As Object, e As PaintEventArgs)
        If mapa.MapProvider Is Nothing OrElse mapa.Width < Px(200) Then Return
        Dim metryNaPiksel = mapa.MapProvider.Projection.GetGroundResolution(CInt(Math.Floor(mapa.Zoom)), mapa.Position.Lat)
        ' Ułamkowe powiększenie GMap.NET rysuje przeskalowanymi kaflami poziomu całkowitego.
        metryNaPiksel /= Math.Pow(2, mapa.Zoom - Math.Floor(mapa.Zoom))
        If metryNaPiksel <= 0 OrElse Double.IsNaN(metryNaPiksel) Then Return
        Dim cel = Px(100) * metryNaPiksel
        Dim rzad = Math.Pow(10, Math.Floor(Math.Log10(cel)))
        Dim dlugosc = rzad
        For Each krok In {2.0, 5.0, 10.0}
            If rzad * krok <= cel Then dlugosc = rzad * krok
        Next
        Dim piksele = CInt(dlugosc / metryNaPiksel)
        Dim napis = If(dlugosc >= 1000, (dlugosc / 1000).ToString("0.###", Globalization.CultureInfo.InvariantCulture) & " km",
                       dlugosc.ToString("0.###", Globalization.CultureInfo.InvariantCulture) & " m")
        Dim g = e.Graphics
        Dim rozmiarNapisu = TextRenderer.MeasureText(napis, _czcionkaRobocza)
        Dim szerokosc = Math.Max(piksele, rozmiarNapisu.Width) + Px(12)
        Dim ramka As New Rectangle(mapa.Width - szerokosc - Px(10), mapa.Height - rozmiarNapisu.Height - Px(18), szerokosc,
                                   rozmiarNapisu.Height + Px(10))
        Using tlo As New SolidBrush(Color.FromArgb(200, Color.White))
            g.FillRectangle(tlo, ramka)
        End Using
        Dim x0 = ramka.Left + (ramka.Width - piksele) \ 2, y = ramka.Bottom - Px(5)
        Using pioro As New Pen(Atrament, Math.Max(1.5F, Px(2)))
            g.DrawLine(pioro, x0, y, x0 + piksele, y)
            g.DrawLine(pioro, x0, y - Px(5), x0, y)
            g.DrawLine(pioro, x0 + piksele, y - Px(5), x0 + piksele, y)
        End Using
        TextRenderer.DrawText(g, napis, _czcionkaRobocza, New Rectangle(ramka.Left, ramka.Top + Px(1), ramka.Width, rozmiarNapisu.Height),
                              Atrament, TextFormatFlags.HorizontalCenter Or TextFormatFlags.NoPadding)
    End Sub

    ''' <summary>Ikona „Zaznacz obszar”: przerywany prostokąt z kwadratowymi uchwytami w narożnikach.</summary>
    Friend Shared Function UtworzIkoneZaznaczenia(Optional rozmiar As Integer = 16) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            Using pioro As New Pen(KolorAkcjiOkna, 1.0F) With {.DashStyle = Drawing2D.DashStyle.Dash}
                g.DrawRectangle(pioro, 2, 3, 11, 9)
            End Using
            Using pedzel As New SolidBrush(KolorAkcjiOkna)
                For Each p In {New Point(1, 2), New Point(12, 2), New Point(1, 11), New Point(12, 11)}
                    g.FillRectangle(pedzel, p.X, p.Y, 3, 3)
                Next
            End Using
        End Using
        Return bmp
    End Function

    ''' <summary>Ikona „Współrzędne”: celownik z punktem w środku.</summary>
    Friend Shared Function UtworzIkoneWspolrzednych(Optional rozmiar As Integer = 16) As Bitmap
        Dim bmp As New Bitmap(rozmiar, rozmiar, Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)
            Using pioro As New Pen(KolorAkcjiOkna, 1.4F)
                g.DrawEllipse(pioro, 3.0F, 3.0F, 10.0F, 10.0F)
                g.DrawLine(pioro, 8.0F, 0.5F, 8.0F, 4.5F)
                g.DrawLine(pioro, 8.0F, 11.5F, 8.0F, 15.5F)
                g.DrawLine(pioro, 0.5F, 8.0F, 4.5F, 8.0F)
                g.DrawLine(pioro, 11.5F, 8.0F, 15.5F, 8.0F)
            End Using
            Using pedzel As New SolidBrush(Color.Firebrick)
                g.FillEllipse(pedzel, 6.5F, 6.5F, 3.0F, 3.0F)
            End Using
        End Using
        Return bmp
    End Function

    Private Sub UlozZoomNaMapie()
        ' Duże, pogrubione znaki i odstęp między przyciskami - małe „+” i „−” stykające się ze sobą słabo było widać na mapie.
        ' Każdy przycisk leży osobno na formularzu nad mapą: kontener z odstępem zasłaniałby mapę paskiem tła formularza.
        Dim czcionkaZoomu As New Font("Segoe UI", 15.0F, FontStyle.Bold)
        AddHandler Disposed, Sub() czcionkaZoomu.Dispose()
        For Each przycisk In {btnPowieksz, btnPomniejsz}
            StylPrzycisku(przycisk)
            przycisk.Font = czcionkaZoomu
            przycisk.ForeColor = KolorAkcjiOkna
            przycisk.FlatAppearance.BorderColor = Color.FromArgb(150, 165, 182)
            przycisk.Padding = Padding.Empty
            przycisk.TextAlign = ContentAlignment.MiddleCenter
            przycisk.UseCompatibleTextRendering = False
            przycisk.AutoSize = False
            przycisk.MinimumSize = Size.Empty
            przycisk.Dock = DockStyle.None
            przycisk.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            przycisk.Size = New Size(Px(38), Px(38))
            If przycisk.Parent IsNot Me Then Controls.Add(przycisk)
            przycisk.BringToFront()
        Next
        ToolTip1.SetToolTip(btnPowieksz, "Przybliż mapę")
        ToolTip1.SetToolTip(btnPomniejsz, "Oddal mapę")
        Dim ustaw As Action = Sub()
                                  btnPowieksz.Location = New Point(mapa.Left + Px(10), mapa.Top + Px(10))
                                  btnPomniejsz.Location = New Point(mapa.Left + Px(10), btnPowieksz.Bottom + Px(8))
                              End Sub
        AddHandler mapa.LocationChanged, Sub() ustaw()
        ustaw()
    End Sub
    Private Sub ZakonczWyborNaroznikow()
        If Not _trybZaznaczania Then Return
        _trybZaznaczania = False
        _pierwszyNaroznik = Nothing
        _przyciskZaznaczenia.Text = "Zaznacz obszar"
        _przyciskZaznaczenia.Font = _czcionkaRobocza
        _przyciskZaznaczenia.Image = _ikonaZaznaczeniaNormalna
        StylPrzyciskuAkcji(_przyciskZaznaczenia, False)
        _podpowiedzZaznaczenia.Text = ""
        If _banerZaznaczania IsNot Nothing Then _banerZaznaczania.Visible = False
        mapa.CanDragMap = True
        mapa.Cursor = Cursors.Hand
    End Sub

    Private Sub PrzyjmijNaroznik(punkt As PointLatLng)
        If Not _pierwszyNaroznik.HasValue Then
            _pierwszyNaroznik = punkt
            _podpowiedzZaznaczenia.Text = "Kliknij przeciwległy narożnik · Esc anuluje"
            If _banerZaznaczania IsNot Nothing Then _banerZaznaczania.Text = "Kliknij przeciwległy narożnik  ·  Esc anuluje"
            Return
        End If
        Dim pierwszy = _pierwszyNaroznik.Value
        Dim prostokat = RectLatLng.FromLTRB(Math.Min(pierwszy.Lng, punkt.Lng), Math.Max(pierwszy.Lat, punkt.Lat),
                                            Math.Max(pierwszy.Lng, punkt.Lng), Math.Min(pierwszy.Lat, punkt.Lat))
        If prostokat.WidthLng <= 0.00001 OrElse prostokat.HeightLat <= 0.00001 Then
            _podpowiedzZaznaczenia.Text = "Kliknij inny narożnik, aby wyznaczyć prostokąt"
            If _banerZaznaczania IsNot Nothing Then _banerZaznaczania.Text = "Kliknij inny narożnik, aby wyznaczyć prostokąt  ·  Esc anuluje"
            Return
        End If
        ' Ten sam zapis zasięgu i rysowanie siatki co po zaznaczeniu prawym przyciskiem.
        ZakonczWyborNaroznikow()
        mapa_OnSelectionChange(prostokat, False)
    End Sub

    Private Sub UlozPasekWspolrzednych()
        StatusStrip1.BackColor = TloPaneli
        StatusStrip1.Font = _czcionkaRobocza
        StatusStrip1.ShowItemToolTips = True
        stPostep.Visible = False
        stFormat.Visible = False
        stFolder.Visible = False
        stSegment.Visible = False
        ' Sekcje oddzielone płaską kreską: układ sesji, kursor w tym układzie, przybliżona skala, a po prawej podpowiedź
        ' zaznaczania i stan sesji. Obszaru pobierania pasek nie powtarza - jest na karcie tuż nad nim.
        Dim sekcja = Function(nazwa As String) New ToolStripStatusLabel With {
            .Name = nazwa, .ForeColor = KolorPomocniczy, .BorderSides = ToolStripStatusLabelBorderSides.None,
            .Padding = New Padding(Px(4), 0, Px(8), 0)}
        _stUklad = sekcja("stUklad")
        _odczytKursora = sekcja("stKursor")
        _odczytKursora.AutoSize = False
        _odczytKursora.TextAlign = ContentAlignment.MiddleLeft
        _stSkala = sekcja("stSkala")
        _odczytSrodka = New ToolStripStatusLabel With {.Visible = False}
        _podpowiedzZaznaczenia = New ToolStripStatusLabel With {.Spring = True, .TextAlign = ContentAlignment.MiddleRight,
                                                               .ForeColor = KolorPomocniczy}
        _stSesja = New ToolStripStatusLabel With {.Name = "stSesja", .ForeColor = Atrament, .ImageScaling = ToolStripItemImageScaling.None,
                                                  .BorderSides = ToolStripStatusLabelBorderSides.None,
                                                  .Padding = New Padding(Px(8), 0, Px(4), 0), .IsLink = False}
        AddHandler StatusStrip1.Paint,
            Sub(s, e)
                Dim y0 = Px(4), y1 = StatusStrip1.Height - Px(4)
                If y1 <= y0 Then Return
                Using pioro As New Pen(KolorLinii)
                    For Each etyk In {_stUklad, _odczytKursora, _stSkala}
                        If etyk IsNot Nothing AndAlso etyk.Visible AndAlso etyk.Bounds.Width > 0 Then
                            Dim x = etyk.Bounds.Right - 1
                            e.Graphics.DrawLine(pioro, x, y0, x, y1)
                        End If
                    Next
                    If _stSesja IsNot Nothing AndAlso _stSesja.Visible AndAlso _stSesja.Bounds.Width > 0 Then
                        Dim x = _stSesja.Bounds.Left
                        e.Graphics.DrawLine(pioro, x, y0, x, y1)
                    End If
                End Using
            End Sub
        AddHandler _stSesja.Click,
            Sub()
                If StanSesji() = StanIkonySesji.Bledy OrElse _liczbaBledowSesji > 0 Then _zakladkiPracy.SelectedTab = _zakladkaKomunikatow
            End Sub
        StatusStrip1.Items.AddRange({_stUklad, _odczytKursora, _stSkala, _odczytSrodka, _podpowiedzZaznaczenia, _stSesja})
        AddHandler lblKursorUklad.TextChanged, Sub() AktualizujWspolrzednePaska()
        AddHandler lblSrodekDlug.TextChanged, Sub() AktualizujWspolrzednePaska()
        AddHandler mapa.OnMapZoomChanged, Sub() AktualizujWspolrzednePaska()
        AddHandler mapa.MouseLeave,
            Sub()
                _kursorUkladu = Nothing
                AktualizujWspolrzednePaska()
            End Sub
        AktualizujWspolrzednePaska()
    End Sub

    ''' <summary>Zapamiętuje współrzędne kursora w układzie sesji (wywołanie z ruchu myszy nad mapą).</summary>
    Private Sub ZapamietajKursor(x As Double, y As Double)
        _kursorUkladu = (x, y)
    End Sub

    Private Shared ReadOnly _kulturaPl As Globalization.CultureInfo = Globalization.CultureInfo.GetCultureInfo("pl-PL")
    Private _ostatniPomiarUkladuKursora As UkladWspolrzednych
    Private _ostatniaCzcionkaKursora As Font

    Private Sub AktualizujWspolrzednePaska()
        If _odczytKursora Is Nothing Then Return
        Dim u = Ustawienia.Uklad
        _stUklad.Text = "Układ: " & u.Nazwa
        _odczytKursora.Visible = Ustawienia.KursorISrodekMapy
        _odczytKursora.Text = OpisKursora(_kursorUkladu, u.Geograficzny)
        ' Stała szerokość sekcji kursora - inaczej sekcje po prawej przesuwały się przy każdym ruchu myszy.
        If u IsNot _ostatniPomiarUkladuKursora OrElse StatusStrip1.Font IsNot _ostatniaCzcionkaKursora Then
            Dim najszerszy As (X As Double, Y As Double)? = If(u.Geograficzny, (-99.99999, -999.99999), (9999999.9, 9999999.9))
            _odczytKursora.Width = TextRenderer.MeasureText(OpisKursora(najszerszy, u.Geograficzny), StatusStrip1.Font).Width +
                                   _odczytKursora.Padding.Horizontal + Px(6)
            _ostatniPomiarUkladuKursora = u
            _ostatniaCzcionkaKursora = StatusStrip1.Font
        End If
        Dim mianownik = MianownikSkali()
        _stSkala.Text = If(mianownik > 0, "Skala ≈ 1:" & mianownik.ToString("#,0", _kulturaPl), "")
        _stSkala.ToolTipText = "Skala przybliżona: program przyjmuje typową wielkość piksela ekranu (" & DeviceDpi & " dpi)."
    End Sub

    ''' <summary>
    ''' Współrzędne kursora w kolejności geodezyjnej (X na północ). Poza mapą „—”.
    ''' </summary>
    Friend Shared Function OpisKursora(kursor As (X As Double, Y As Double)?, geograficzny As Boolean) As String
        If Not kursor.HasValue Then Return "Kursor  —"
        Dim k = kursor.Value
        If geograficzny Then Return $"Kursor  X: {k.X.ToString("0.00000", _kulturaPl)}°  Y: {k.Y.ToString("0.00000", _kulturaPl)}°"
        Return $"Kursor  X: {k.X.ToString("#,0.0", _kulturaPl)}  Y: {k.Y.ToString("#,0.0", _kulturaPl)} m"
    End Function

    ''' <summary>
    ''' Mianownik skali mapy na ekranie zaokrąglony do dwóch cyfr znaczących (np. 48 000). Program nie zna rzeczywistej
    ''' wielkości piksela monitora, więc liczy z dpi systemu - dlatego na pasku stoi „≈”.
    ''' </summary>
    Private Function MianownikSkali() As Integer
        If mapa.MapProvider Is Nothing Then Return 0
        Dim metryNaPiksel = mapa.MapProvider.Projection.GetGroundResolution(CInt(Math.Floor(mapa.Zoom)), mapa.Position.Lat)
        metryNaPiksel /= Math.Pow(2, mapa.Zoom - Math.Floor(mapa.Zoom))
        Return ZaokraglijSkale(metryNaPiksel * DeviceDpi / 0.0254)
    End Function

    Friend Shared Function ZaokraglijSkale(mianownik As Double) As Integer
        If Double.IsNaN(mianownik) OrElse mianownik <= 0 OrElse Double.IsInfinity(mianownik) Then Return 0
        Dim rzad = Math.Pow(10, Math.Max(0, Math.Floor(Math.Log10(mianownik)) - 1))
        Return CInt(Math.Round(mianownik / rzad) * rzad)
    End Function

    ''' <summary>Stan sesji do ikony karty „Stan sesji” i kropki na pasku stanu.</summary>
    Friend Function StanSesji() As StanIkonySesji
        If _przerwanie IsNot Nothing Then Return StanIkonySesji.Pobieranie
        If _ostatniWynikPobierania.HasValue AndAlso _ostatniWynikPobierania.Value.Nieudane > 0 Then Return StanIkonySesji.Bledy
        If _podswietlEksport Then Return StanIkonySesji.Pobrana
        If _zestaw.Liczba > 0 AndAlso BiezacaSiatka().Poprawna Then Return StanIkonySesji.Gotowa
        Return StanIkonySesji.Nieaktywna
    End Function

    Private Function PostepPobierania() As Double
        If _postepBelki Is Nothing OrElse _postepBelki.Maximum <= _postepBelki.Minimum Then Return 0
        Return (_postepBelki.Value - _postepBelki.Minimum) / CDbl(_postepBelki.Maximum - _postepBelki.Minimum)
    End Function

    ''' <summary>Prawa sekcja paska stanu: kropka w kolorze stanu, nazwa sesji i krótki stan; przy błędach kliknięcie otwiera Komunikaty.</summary>
    Private Sub OdswiezPasekSesji()
        If _stSesja Is Nothing Then Return
        Dim stan = StanSesji()
        ' Nowa sesja bez zapisu ma za folder sam folder pobrań - wtedy „bez nazwy”, a nie „download”.
        Dim folder = If(FolderSesji, "").TrimEnd("\"c, "/"c)
        Dim nazwa = If(folder = "" OrElse String.Equals(folder, If(FolderDownload, "").TrimEnd("\"c, "/"c), StringComparison.OrdinalIgnoreCase),
                       "bez nazwy", Path.GetFileName(folder))
        Dim opis As String
        Select Case stan
            Case StanIkonySesji.Pobieranie : opis = $"pobieranie {PostepPobierania() * 100:0} %"
            Case StanIkonySesji.Bledy : opis = $"błędy ({_ostatniWynikPobierania.Value.Nieudane})"
            Case StanIkonySesji.Pobrana : opis = "pobrana"
            Case StanIkonySesji.Gotowa : opis = "gotowa do pobrania"
            Case Else : opis = If(_zestaw.Liczba = 0, "brak warstw", "brak obszaru")
        End Select
        Dim tekst = $"Sesja: {nazwa} · {opis}"
        If _stSesja.Text <> tekst Then _stSesja.Text = tekst
        Dim bledy = stan = StanIkonySesji.Bledy OrElse _liczbaBledowSesji > 0
        _stSesja.ToolTipText = If(bledy, "Kliknij, aby zobaczyć komunikaty o błędach.", FolderSesji)
        _stSesja.IsLink = False
        If _stSesja.Tag Is Nothing OrElse CType(_stSesja.Tag, StanIkonySesji) <> stan Then
            Dim bok = Px(10)
            Dim kropka As New Bitmap(bok, bok)
            Using g = Graphics.FromImage(kropka), pedzel As New SolidBrush(KolorStanu(stan))
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.FillEllipse(pedzel, 0.5F, 0.5F, bok - 1.5F, bok - 1.5F)
            End Using
            _stSesja.Image?.Dispose()
            _stSesja.Image = kropka
            _stSesja.Tag = stan
        End If
        _kartaStanu?.Invalidate()
    End Sub

    Private Sub StylPrzycisku(przycisk As Button)
        przycisk.Font = _czcionkaRobocza
        przycisk.AutoSize = True
        przycisk.AutoSizeMode = AutoSizeMode.GrowAndShrink
        przycisk.MinimumSize = New Size(Px(29), Px(30))
        przycisk.Padding = New Padding(Px(5), 0, Px(5), 0)
        przycisk.FlatStyle = FlatStyle.Flat
        przycisk.FlatAppearance.BorderColor = KolorLinii
        przycisk.FlatAppearance.MouseOverBackColor = TloNaglowka
        przycisk.FlatAppearance.MouseDownBackColor = TloBelki
        przycisk.BackColor = Color.White
        przycisk.ForeColor = Atrament
        przycisk.Cursor = Cursors.Hand
        przycisk.UseVisualStyleBackColor = False
    End Sub

    Private Sub StylPrzyciskuAkcji(przycisk As Button, aktywny As Boolean)
        przycisk.BackColor = If(aktywny, KolorAkcjiOkna, Color.White)
        przycisk.ForeColor = If(aktywny, Color.White, Atrament)
        przycisk.FlatAppearance.BorderColor = If(aktywny, KolorAkcjiOkna, KolorLinii)
        przycisk.FlatAppearance.MouseOverBackColor = If(aktywny, KolorAkcjiHover, TloNaglowka)
        przycisk.FlatAppearance.MouseDownBackColor = If(aktywny, KolorAkcjiDown, TloBelki)
    End Sub

    Private Shared Sub DolozKomorke(uklad As TableLayoutPanel, kontrolka As Control, kolumna As Integer, wiersz As Integer)
        kontrolka.Dock = DockStyle.Fill
        uklad.Controls.Add(kontrolka, kolumna, wiersz)
    End Sub

    Private Shared Function PasekPrzyciskow(ParamArray kontrolki() As Control) As FlowLayoutPanel
        Dim pasek As New FlowLayoutPanel With {
            .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .WrapContents = True, .Margin = Padding.Empty
        }
        For Each c In kontrolki
            c.Dock = DockStyle.None
            c.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            If TypeOf c Is Button Then
                DirectCast(c, Button).AutoSize = True
            End If
            pasek.Controls.Add(c)
        Next
        Return pasek
    End Function

    Private Sub UlozPodglad(strona As TabPage)
        grpPodglad.SuspendLayout()
        ' Pasek sterowania nad próbką - dawna kolumna obok próbki zabierała niemal połowę szerokości panelu.
        Dim uklad As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 2, .Dock = DockStyle.Fill, .Margin = Padding.Empty}
        _ukladPodgladu = uklad
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        uklad.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        uklad.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim pasek As New TableLayoutPanel With {.ColumnCount = 2, .RowCount = 1, .Dock = DockStyle.Top,
                                                .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                                .BackColor = TloPaneli, .Margin = Padding.Empty,
                                                .Padding = New Padding(Px(4), Px(3), Px(4), Px(4))}
        pasek.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        pasek.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        pasek.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        AddHandler pasek.Paint,
            Sub(s, e)
                Using pioro As New Pen(KolorLinii)
                    e.Graphics.DrawLine(pioro, 0, pasek.Height - 1, pasek.Width - 1, pasek.Height - 1)
                End Using
            End Sub

        btnZoomMinus.Text = "−"
        btnZoomReset.Text = "Dopasuj"
        ToolTip1.SetToolTip(btnZoomReset, "Pokaż całą próbkę w dostępnym miejscu.")
        For Each przycisk In {btnZoomMinus, btnZoomPlus}
            StylPrzycisku(przycisk)
            przycisk.AutoSize = False
            przycisk.MinimumSize = Size.Empty
            przycisk.Padding = Padding.Empty
            przycisk.Size = New Size(Px(24), Px(24))
            przycisk.Margin = New Padding(0, 0, Px(2), 0)
        Next
        StylPrzycisku(btnZoomReset)
        btnZoomReset.AutoSize = True
        btnZoomReset.AutoSizeMode = AutoSizeMode.GrowAndShrink
        btnZoomReset.MinimumSize = New Size(Px(52), Px(24))
        btnZoomReset.MaximumSize = Size.Empty
        btnZoomReset.Padding = New Padding(Px(4), 0, Px(4), 0)
        btnZoomReset.Margin = New Padding(0, 0, Px(2), 0)

        StylPrzycisku(btnPodgladOdswiez)
        btnPodgladOdswiez.AutoSize = False
        btnPodgladOdswiez.MinimumSize = Size.Empty
        btnPodgladOdswiez.Padding = Padding.Empty
        btnPodgladOdswiez.Text = ""
        btnPodgladOdswiez.Image = ZarejestrujIkone(IkonaOkna.Odswiez, KolorAkcjiOkna, 14)
        btnPodgladOdswiez.ImageAlign = ContentAlignment.MiddleCenter
        btnPodgladOdswiez.AccessibleName = "Odśwież podgląd"
        btnPodgladOdswiez.Size = New Size(Px(26), Px(24))
        btnPodgladOdswiez.Margin = Padding.Empty

        Dim przyciski As New FlowLayoutPanel With {.AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                                    .WrapContents = False, .Margin = Padding.Empty}
        przyciski.Controls.AddRange({btnZoomMinus, btnZoomPlus, btnZoomReset, btnPodgladOdswiez})
        pasek.Controls.Add(przyciski, 0, 0)

        lblPodgladSkala.AutoSize = False
        lblPodgladSkala.Dock = DockStyle.Fill
        lblPodgladSkala.Font = _czcionkaRobocza
        lblPodgladSkala.ForeColor = KolorPomocniczy
        lblPodgladSkala.TextAlign = ContentAlignment.MiddleRight
        lblPodgladSkala.AutoEllipsis = True
        lblPodgladSkala.Margin = New Padding(Px(4), 0, 0, 0)
        pasek.Controls.Add(lblPodgladSkala, 1, 0)
        _pasekPodgladu = pasek
        uklad.Controls.Add(pasek, 0, 0)

        Dim probka As New Panel With {.BackColor = TloPaneli, .Margin = Padding.Empty}
        picPodglad.Dock = DockStyle.Fill
        picPodglad.BorderStyle = BorderStyle.None
        picPodglad.Cursor = Cursors.Hand
        ToolTip1.SetToolTip(picPodglad, "Kółko myszy: przybliż lub oddal próbkę (tak jak − i + powyżej). Podwójne kliknięcie otwiera Mikser warstw.")
        ' Pusta próbka mówi, jak zacząć - pierwsze uruchomienie i nowa sesja nie miały żadnej wskazówki poza kartą stanu (ER-13).
        _pustyPodglad = New Label With {.Text = "Tu pojawi się próbka wybranych warstw." & vbCrLf & vbCrLf &
                                                "Jak zacząć:" & vbCrLf &
                                                "1. Wybierz warstwę w katalogu warstw powyżej." & vbCrLf &
                                                "2. Zaznacz obszar na mapie (przycisk Zaznacz obszar)." & vbCrLf &
                                                "3. Kliknij Pobierz segmenty, potem Eksportuj." & vbCrLf & vbCrLf &
                                                "Gotową mapę w kilku krokach utworzy Szybki kreator (Ctrl+K).",
                                        .Dock = DockStyle.Fill,
                                        .TextAlign = ContentAlignment.MiddleCenter, .Font = _czcionkaRobocza,
                                        .ForeColor = KolorPomocniczy, .BackColor = TloPaneli, .Padding = New Padding(Px(5))}
        lblPodgladStan.Dock = DockStyle.Bottom
        lblPodgladStan.AutoSize = False
        lblPodgladStan.Height = Px(22)
        lblPodgladStan.BackColor = TloNaglowka
        lblPodgladStan.ForeColor = KolorPomocniczy
        lblPodgladStan.Font = _czcionkaRobocza
        lblPodgladStan.TextAlign = ContentAlignment.MiddleLeft
        lblPodgladStan.Padding = New Padding(Px(6), 0, Px(6), 0)
        lblPodgladStan.AutoEllipsis = True
        lblPodgladStan.Visible = False
        AddHandler lblPodgladStan.TextChanged,
            Sub()
                Dim tekst = lblPodgladStan.Text
                lblPodgladStan.Visible = _zestaw.Liczba > 0 AndAlso Not String.IsNullOrWhiteSpace(tekst)
                lblPodgladStan.ForeColor = If(tekst.Contains("Błąd") OrElse tekst.StartsWith("Brak ", StringComparison.Ordinal),
                                              Color.Firebrick, KolorPomocniczy)
            End Sub

        probka.Controls.Add(picPodglad)
        probka.Controls.Add(lblPodgladStan)
        probka.Controls.Add(_pustyPodglad)
        _pustyPodglad.BringToFront()
        DolozKomorke(uklad, probka, 0, 1)

        AddHandler picPodglad.ClientSizeChanged,
            Sub()
                If _bazaPodgladu IsNot Nothing Then ZastosujZoomPodgladu()
            End Sub
        ' Jeden przycisk miksera - „Mikser…” przy kryciu warstw; drugi (pod próbką) zajmował osobny wiersz.
        btnPodgladZaawansowane.Text = "Mikser warstw…"
        btnPodgladZaawansowane.Visible = False
        lblTytulPodglad.Visible = False
        lblPodgladLiczbaWarstw.Visible = False
        grpPodglad.Controls.Add(uklad)
        grpPodglad.Dock = DockStyle.Fill
        strona.Controls.Add(grpPodglad)
        grpPodglad.ResumeLayout(True)
    End Sub

    Private Sub UlozKomunikaty()
        _dziennikSesji = New RichTextBox With {
            .Name = "dziennikSesji", .Dock = DockStyle.Fill, .ReadOnly = True, .DetectUrls = False,
            .AccessibleName = "Historia komunikatów", .BackColor = SystemColors.Window,
            .BorderStyle = BorderStyle.None, .Font = _czcionkaRobocza
        }
        _szczegolyBledow = New LinkLabel With {
            .Text = "Szczegóły pobierania…", .AutoSize = False, .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft, .LinkColor = KolorAkcjiOkna,
            .ActiveLinkColor = KolorAkcjiOkna, .LinkBehavior = LinkBehavior.HoverUnderline,
            .BackColor = TloPaneli, .AutoEllipsis = True, .Visible = False, .Margin = Padding.Empty
        }
        AddHandler _szczegolyBledow.LinkClicked, Sub() PokazSzczegolyPobierania()

        _btnKopiujKomunikaty = New Button With {.Name = "btnKopiujKomunikaty", .Text = "Kopiuj", .Enabled = False}
        _btnWyczyscKomunikaty = New Button With {.Name = "btnWyczyscKomunikaty", .Text = "Wyczyść", .Enabled = False}
        For Each przycisk In {_btnKopiujKomunikaty, _btnWyczyscKomunikaty}
            StylPrzycisku(przycisk)
            przycisk.MinimumSize = New Size(Px(48), Px(23))
            przycisk.MaximumSize = Size.Empty
            przycisk.Padding = New Padding(Px(6), 0, Px(6), 0)
        Next
        _btnKopiujKomunikaty.Margin = New Padding(Px(4), 0, Px(4), 0)
        _btnWyczyscKomunikaty.Margin = Padding.Empty
        ToolTip1.SetToolTip(_btnKopiujKomunikaty, "Kopiuj całą historię komunikatów do schowka")
        ToolTip1.SetToolTip(_btnWyczyscKomunikaty, "Wyczyść komunikaty i wyzeruj licznik błędów")

        AddHandler _btnKopiujKomunikaty.Click,
            Sub()
                If _dziennikSesji.TextLength = 0 Then Return
                Try
                    Clipboard.SetText(_dziennikSesji.Text)
                Catch
                End Try
            End Sub
        AddHandler _btnWyczyscKomunikaty.Click,
            Sub()
                _dziennikSesji.Clear()
                _btnKopiujKomunikaty.Enabled = False
                _btnWyczyscKomunikaty.Enabled = False
                WyzerujLicznikBledow()
            End Sub

        Dim pasekKomunikatow As New TableLayoutPanel With {
            .ColumnCount = 3, .RowCount = 1, .Dock = DockStyle.Bottom,
            .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = TloPaneli, .Padding = New Padding(Px(4), Px(4), Px(4), Px(3))
        }
        pasekKomunikatow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        pasekKomunikatow.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        pasekKomunikatow.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        pasekKomunikatow.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        AddHandler pasekKomunikatow.Paint,
            Sub(s, e)
                Using pioro As New Pen(KolorLinii)
                    e.Graphics.DrawLine(pioro, 0, 0, pasekKomunikatow.Width - 1, 0)
                End Using
            End Sub
        pasekKomunikatow.Controls.Add(_szczegolyBledow, 0, 0)
        pasekKomunikatow.Controls.Add(_btnKopiujKomunikaty, 1, 0)
        pasekKomunikatow.Controls.Add(_btnWyczyscKomunikaty, 2, 0)

        _zakladkaKomunikatow.Controls.Add(_dziennikSesji)
        _zakladkaKomunikatow.Controls.Add(pasekKomunikatow)
    End Sub

    Private Sub UlozZakladkeSesji()
        Dim uklad As New TableLayoutPanel With {
            .ColumnCount = 1, .Dock = DockStyle.Top, .AutoSize = True, .Padding = New Padding(Px(8), Px(6), Px(8), Px(6))
        }
        _zakladkaSesji.AutoScroll = True
        Dim tytulFolderu As New Label With {.Text = "Folder sesji", .AutoSize = True, .Font = _czcionkaNaglowka,
                                            .ForeColor = KolorPomocniczy, .Margin = New Padding(0, 0, 0, Px(3))}
        _folderZakladki = New TextBox With {.ReadOnly = True, .AccessibleName = "Folder sesji",
                                            .Font = _czcionkaRobocza, .BackColor = Color.White, .ForeColor = Atrament}
        _informacjeOPlikach = New Label With {.AutoSize = True, .Font = _czcionkaRobocza,
                                              .ForeColor = KolorPomocniczy, .Margin = New Padding(0, Px(5), 0, Px(6))}
        Dim zapisz As New Button With {.Text = "Zapisz sesję w folderze…"}
        AddHandler zapisz.Click, Sub() ZapiszToolStripMenuItem.PerformClick()

        btnOtworzFolderDolny.Text = "Otwórz folder sesji"
        btnOtworzFolderDolny.Image = ZarejestrujIkone(IkonaOkna.Sesja, KolorAkcjiOkna)
        btnOtworzFolderDolny.TextImageRelation = TextImageRelation.ImageBeforeText

        btnUsunPobrane.Text = "Usuń pobrane"
        btnUsunPobrane.Image = ZarejestrujIkone(IkonaOkna.Kosz, Color.Firebrick)
        btnUsunPobrane.TextImageRelation = TextImageRelation.ImageBeforeText
        btnUsunPobrane.AccessibleName = "Usuń pobrane segmenty"

        For Each przycisk In {btnOtworzFolderDolny, zapisz, btnUsunPobrane}
            StylPrzycisku(przycisk)
            przycisk.MinimumSize = New Size(Px(29), Px(28))
            przycisk.MaximumSize = Size.Empty
            przycisk.Margin = New Padding(0, 0, Px(6), Px(4))
        Next
        btnUsunPobrane.ForeColor = Color.Firebrick
        btnUsunPobrane.FlatAppearance.BorderColor = Color.FromArgb(225, 190, 190)
        btnUsunPobrane.FlatAppearance.MouseOverBackColor = Color.FromArgb(253, 237, 237)
        btnUsunPobrane.FlatAppearance.MouseDownBackColor = Color.FromArgb(248, 218, 218)

        uklad.Controls.Add(tytulFolderu)
        _folderZakladki.Dock = DockStyle.Fill
        uklad.Controls.Add(_folderZakladki)
        uklad.Controls.Add(_informacjeOPlikach)
        uklad.Controls.Add(PasekPrzyciskow(btnOtworzFolderDolny, zapisz, btnUsunPobrane))
        _zakladkaSesji.Controls.Add(uklad)
    End Sub

    Private Sub UlozBelkePracy()
        ' Przyciski pobierania i eksportu są w karcie „Stan sesji” - osobny wiersz akcji pod kartami był prawie pusty.
        Dim wysokoscDanych = Px(106)
        Dim wysokoscParametrow = Px(104)
        _wysokoscDanychBelki = wysokoscDanych
        _wysokoscParametrowBelki = wysokoscParametrow
        Dim uklad As New TableLayoutPanel With {
            .ColumnCount = 3, .RowCount = 3, .Dock = DockStyle.Fill, .Padding = Padding.Empty,
            .BackColor = TloBelki
        }
        _ukladBelki = uklad
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 28))
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 29))
        uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 43))
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, wysokoscDanych))
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))

        _wymiaryObszaru = EtykietaBelki(_czcionkaWartosci)
        _podsumowanieObszaru = EtykietaBelki(_czcionkaRobocza, pomocnicza:=True)
        _rozdzielczoscBelki = EtykietaBelki(_czcionkaWartosci)
        _podsumowanieParametrow = EtykietaBelki(_czcionkaRobocza, pomocnicza:=True)
        _podpowiedzBelki = EtykietaBelki(_czcionkaWartosci)
        _szczegolyStanuBelki = EtykietaBelki(_czcionkaRobocza, pomocnicza:=True)
        _postepBelki = New ProgressBar With {.Dock = DockStyle.Top, .Height = Px(4), .Margin = New Padding(0, Px(1), 0, Px(2)), .Visible = False}

        ' „Zmień ▾” zamiast „Pokaż na mapie”: menu ze wszystkimi sposobami wskazania obszaru (także pokazaniem go na mapie),
        ' tak jak „Zmień…” w karcie parametrów. Przycisk „Współrzędne…” na pasku stał się zbędny.
        _lnkPokazObszar = New LinkLabel With {.Name = "lnkZmienObszar", .Text = "Zmień ▾"}
        _menuObszaru = UtworzMenuObszaru()
        _lnkParametry = New LinkLabel With {.Name = "lnkParametry"}
        Dim kartaObszaru = SekcjaBelki("Obszar pobierania", _wymiaryObszaru, _podsumowanieObszaru, _lnkPokazObszar,
                                       Sub(g, r) RysujIkone(g, IkonaOkna.Obszar, r, KolorAkcjiOkna))
        Dim kartaParametrow = SekcjaBelki("Parametry segmentów", _rozdzielczoscBelki, _podsumowanieParametrow, _lnkParametry,
                                          Sub(g, r) RysujIkone(g, IkonaOkna.Segmenty, r, KolorAkcjiOkna))
        _kartaParametrow = kartaParametrow
        DolozKomorke(uklad, kartaObszaru, 0, 0)
        DolozKomorke(uklad, kartaParametrow, 1, 0)
        ObramujKarte(kartaObszaru)
        ObramujKarte(kartaParametrow)

        ' Edycja parametrów i podgląd obszaru są w kartach, których dotyczą (dawniej osobne przyciski pod kartami).
        _lnkParametry.Text = "Zmień…"
        AddHandler _lnkParametry.LinkClicked,
            Sub()
                If grpRozdzielczoscFormat.Visible Then
                    ZwinParametryBelki()
                Else
                    grpRozdzielczoscFormat.Visible = True
                    uklad.RowStyles(2).Height = wysokoscParametrow
                    Panel3.Height = wysokoscDanych + wysokoscParametrow
                    _lnkParametry.Text = "Zwiń"
                    _kartaParametrow?.Invalidate()
                    grpRozdzielczoscFormat.Invalidate()
                End If
            End Sub
        ToolTip1.SetToolTip(_lnkParametry, "Rozdzielczość, bok segmentu, układ współrzędnych i format pobieranych segmentów.")
        AddHandler _lnkPokazObszar.LinkClicked, Sub() _menuObszaru.Show(_lnkPokazObszar, New Point(0, _lnkPokazObszar.Height))
        ToolTip1.SetToolTip(_lnkPokazObszar, "Pokaż obszar na mapie, zaznacz go, wpisz współrzędne albo wczytaj z pliku trasy lub poligonu.")
        ' Kliknięcie wymiaru obszaru pokazuje obszar na mapie - skrót do pierwszej pozycji menu.
        _wymiaryObszaru.Cursor = Cursors.Hand
        AddHandler _wymiaryObszaru.Click, Sub() If BiezacaSiatka().Poprawna Then PokazObszarNaMapie()
        ToolTip1.SetToolTip(_wymiaryObszaru, "Kliknij, aby pokazać obszar pobierania na mapie.")
        _panelPobierania = New Panel With {.Width = Px(160), .Height = Px(28), .Margin = New Padding(0, 0, 0, Px(3))}
        For Each b In {btnPobierz, btnPrzerwij}
            StylPrzycisku(b)
            b.Dock = DockStyle.Fill
            b.AutoSize = False
            _panelPobierania.Controls.Add(b)
        Next
        btnPrzerwij.Visible = False
        StylPrzyciskuAkcji(btnPrzerwij, True)
        StylPrzycisku(btnPrzejdzDoEksportu)
        btnPrzejdzDoEksportu.Text = "Eksportuj pobraną mapę…"
        btnPrzejdzDoEksportu.AccessibleName = btnPrzejdzDoEksportu.Text
        ToolTip1.SetToolTip(btnPrzejdzDoEksportu, "Utwórz plik mapy z segmentów już pobranych do folderu sesji.")
        btnPrzejdzDoEksportu.AutoSize = False
        btnPrzejdzDoEksportu.MinimumSize = Size.Empty
        btnPrzejdzDoEksportu.Size = New Size(Px(160), Px(28))
        btnPrzejdzDoEksportu.Margin = Padding.Empty
        btnPrzejdzDoEksportu.FlatAppearance.BorderSize = 1
        StylPrzyciskuAkcji(btnPrzejdzDoEksportu, _podswietlEksport)

        Dim wysNaglowka = Math.Max(Px(25), _czcionkaNaglowka.Height + Px(8))
        Dim bokIkony = Px(15)
        Dim przyciski As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 2, .AutoSize = True,
                                                    .AutoSizeMode = AutoSizeMode.GrowAndShrink, .Margin = New Padding(Px(4), 0, Px(6), 0),
                                                    .Anchor = AnchorStyles.Right, .BackColor = Color.White}
        przyciski.Controls.Add(_panelPobierania, 0, 0)
        przyciski.Controls.Add(btnPrzejdzDoEksportu, 0, 1)

        Dim stan As New TableLayoutPanel With {.ColumnCount = 2, .RowCount = 2, .BackColor = Color.White,
                                               .Name = "kartaStanuSesji", .AccessibleName = "Stan sesji"}
        stan.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        stan.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        stan.RowStyles.Add(New RowStyle(SizeType.Absolute, wysNaglowka))
        stan.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        AddHandler stan.Paint,
            Sub(s, e)
                Dim wiersze = stan.GetRowHeights()
                Dim wys = If(wiersze.Length > 0 AndAlso wiersze(0) > 0, wiersze(0), wysNaglowka)
                Using naglowek As New SolidBrush(TloNaglowka)
                    e.Graphics.FillRectangle(naglowek, 1, 1, stan.Width - 2, wys)
                End Using
                Using linia As New Pen(KolorLinii)
                    e.Graphics.DrawLine(linia, 1, wys + 1, stan.Width - 2, wys + 1)
                End Using
                RysujIkoneStanu(e.Graphics, StanSesji(), New RectangleF(Px(8), 1 + (wys - bokIkony) / 2.0F, bokIkony, bokIkony), PostepPobierania())
                Dim lewaTekstu = Px(8) + bokIkony + Px(6)
                TextRenderer.DrawText(e.Graphics, "Stan sesji", _czcionkaNaglowka,
                                      New Rectangle(lewaTekstu, 1, Math.Max(0, stan.Width - Px(8) - lewaTekstu), wys), Atrament,
                                      TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine)
            End Sub

        Dim trescStanu As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 3, .Dock = DockStyle.Fill,
                                                    .BackColor = Color.White, .Margin = Padding.Empty,
                                                    .Padding = New Padding(Px(8), Px(3), Px(4), Px(1))}
        trescStanu.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        trescStanu.RowStyles.Add(New RowStyle(SizeType.Absolute, Math.Max(Px(24), _czcionkaWartosci.Height + Px(2))))
        trescStanu.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        trescStanu.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        DolozKomorke(trescStanu, _podpowiedzBelki, 0, 0)
        DolozKomorke(trescStanu, _szczegolyStanuBelki, 0, 1)
        trescStanu.Controls.Add(_postepBelki, 0, 2)
        DolozKomorke(stan, trescStanu, 0, 1)
        stan.Controls.Add(przyciski, 1, 1)
        _kartaStanu = stan
        DolozKomorke(uklad, stan, 2, 0)
        ObramujKarte(stan)
        ' Zewnętrzne krawędzie skrajnych kart w linii z lewą i prawą krawędzią mapy.
        kartaObszaru.Margin = New Padding(0, kartaObszaru.Margin.Top, kartaObszaru.Margin.Right, kartaObszaru.Margin.Bottom)
        stan.Margin = New Padding(stan.Margin.Left, stan.Margin.Top, 0, stan.Margin.Bottom)

        UlozUstawieniaBelki()
        grpRozdzielczoscFormat.Margin = New Padding(0, 0, 0, Px(5))
        grpRozdzielczoscFormat.Padding = New Padding(1, Px(3), 1, 1)
        grpRozdzielczoscFormat.BackColor = Color.White
        AddHandler grpRozdzielczoscFormat.Resize, Sub() grpRozdzielczoscFormat.Invalidate()
        AddHandler uklad.Resize, Sub() If grpRozdzielczoscFormat.Visible Then grpRozdzielczoscFormat.Invalidate()
        AddHandler grpRozdzielczoscFormat.Paint,
            Sub(s, e)
                Using pioro As New Pen(KolorRamkiKarty)
                    e.Graphics.DrawRectangle(pioro, 0, 0, grpRozdzielczoscFormat.Width - 1, grpRozdzielczoscFormat.Height - 1)
                End Using
                If _kartaParametrow IsNot Nothing AndAlso _kartaParametrow.Width > 2 Then
                    Dim x0 = Math.Max(0, _kartaParametrow.Left - grpRozdzielczoscFormat.Left)
                    Dim szer = Math.Min(grpRozdzielczoscFormat.Width - x0, _kartaParametrow.Width)
                    If szer > 0 Then
                        Using akcent As New SolidBrush(KolorAkcjiOkna)
                            e.Graphics.FillRectangle(akcent, x0, 0, szer, Math.Max(2, Px(2)))
                        End Using
                    End If
                End If
            End Sub

        DolozKomorke(uklad, grpRozdzielczoscFormat, 0, 2)
        uklad.SetColumnSpan(grpRozdzielczoscFormat, 3)
        grpRozdzielczoscFormat.Visible = False
        grpObszar.Visible = False
        lblSeparator1.Visible = False
        lblSeparator2.Visible = False
        pnlBelkaAkcji.Dock = DockStyle.Fill
        lblStatusSesji.Visible = False
        pnlBelkaAkcji.Controls.Add(uklad)
        uklad.BringToFront()
        Panel3.AutoScroll = False
        Panel3.Height = wysokoscDanych
    End Sub

    Friend Sub ZwinParametryBelki()
        If _ukladBelki Is Nothing OrElse Not grpRozdzielczoscFormat.Visible Then Return
        grpRozdzielczoscFormat.Visible = False
        _ukladBelki.RowStyles(2).Height = 0
        Panel3.Height = _wysokoscDanychBelki
        _lnkParametry.Text = "Zmień…"
        _kartaParametrow?.Invalidate()
    End Sub

    ''' <summary>
    ''' Karta belki odcięta od tła: ramka w kolorze linii i odstęp od sąsiednich kart. Same białe karty na jasnym tle
    ''' zlewały się ze sobą.
    ''' </summary>
    Private Sub ObramujKarte(karta As Control)
        karta.Margin = New Padding(Px(3), Px(5), Px(3), Px(5))
        karta.Padding = New Padding(karta.Padding.Left + 1, karta.Padding.Top + 1, karta.Padding.Right + 1, karta.Padding.Bottom + 1)
        AddHandler karta.Paint,
            Sub(s, e)
                Using pioro As New Pen(KolorRamkiKarty)
                    e.Graphics.DrawRectangle(pioro, 0, 0, karta.Width - 1, karta.Height - 1)
                End Using
                If karta Is _kartaParametrow AndAlso grpRozdzielczoscFormat.Visible Then
                    Dim h = Math.Max(2, Px(2))
                    Using akcent As New SolidBrush(KolorAkcjiOkna)
                        e.Graphics.FillRectangle(akcent, 0, karta.Height - h, karta.Width, h)
                    End Using
                End If
            End Sub
        AddHandler karta.Resize, Sub() karta.Invalidate()
    End Sub

    Private Shared Function EtykietaBelki(czcionka As Font, Optional pomocnicza As Boolean = False) As Label
        Return New Label With {.Font = czcionka, .ForeColor = If(pomocnicza, KolorPomocniczy, Atrament),
                               .TextAlign = ContentAlignment.MiddleLeft, .Margin = Padding.Empty, .AutoEllipsis = True}
    End Function

    ''' <summary>
    ''' Karta belki w jednym stylu z kartami prawego panelu: pasek nagłówka z ikoną, tytułem i akcją po prawej,
    ''' pod nimi wartość i szczegóły w białym polu.
    ''' </summary>
    Private Function SekcjaBelki(tytul As String, wartosc As Label, szczegoly As Label,
                                 akcja As LinkLabel, ikona As Action(Of Graphics, RectangleF)) As TableLayoutPanel
        Dim bok = Px(15)
        Dim wysNaglowka = Math.Max(Px(25), _czcionkaNaglowka.Height + Px(8))
        Dim sekcja As New TableLayoutPanel With {.ColumnCount = 2, .RowCount = 3, .BackColor = Color.White,
                                                 .Padding = Padding.Empty, .AccessibleName = tytul}
        AddHandler sekcja.Paint,
            Sub(s, e)
                Dim wiersze = sekcja.GetRowHeights()
                Dim wys = If(wiersze.Length > 0 AndAlso wiersze(0) > 0, wiersze(0), wysNaglowka)
                Using naglowek As New SolidBrush(TloNaglowka)
                    e.Graphics.FillRectangle(naglowek, 1, 1, sekcja.Width - 2, wys)
                End Using
                Using linia As New Pen(KolorLinii)
                    e.Graphics.DrawLine(linia, 1, wys + 1, sekcja.Width - 2, wys + 1)
                End Using
                ikona(e.Graphics, New RectangleF(Px(8), 1 + (wys - bok) / 2.0F, bok, bok))
                Dim prawa = If(akcja IsNot Nothing AndAlso akcja.Visible, akcja.Left - Px(4), sekcja.Width - Px(8))
                Dim lewaTekstu = Px(8) + bok + Px(6)
                TextRenderer.DrawText(e.Graphics, tytul, _czcionkaNaglowka,
                                      New Rectangle(lewaTekstu, 1, Math.Max(0, prawa - lewaTekstu), wys), Atrament,
                                      TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine)
            End Sub
        sekcja.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        sekcja.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        sekcja.RowStyles.Add(New RowStyle(SizeType.Absolute, wysNaglowka))
        sekcja.RowStyles.Add(New RowStyle(SizeType.Absolute, Math.Max(Px(26), _czcionkaWartosci.Height + Px(4))))
        sekcja.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        If akcja IsNot Nothing Then
            akcja.Font = _czcionkaRobocza
            akcja.AutoSize = True
            akcja.BackColor = TloNaglowka
            akcja.Margin = New Padding(0, 0, Px(8), 0)
            akcja.LinkColor = KolorAkcjiOkna
            akcja.ActiveLinkColor = KolorAkcjiOkna
            akcja.LinkBehavior = LinkBehavior.HoverUnderline
            akcja.Anchor = AnchorStyles.Right
            sekcja.Controls.Add(akcja, 1, 0)
        End If
        wartosc.Margin = New Padding(Px(8), Px(2), Px(8), 0)
        szczegoly.Margin = New Padding(Px(8), 0, Px(8), Px(3))
        DolozKomorke(sekcja, wartosc, 0, 1)
        DolozKomorke(sekcja, szczegoly, 0, 2)
        sekcja.SetColumnSpan(wartosc, 2)
        sekcja.SetColumnSpan(szczegoly, 2)
        Return sekcja
    End Function

    ''' <summary>
    ''' Przybliża mapę do obszaru pobierania i rysuje siatkę. Przy małym obszarze i oddalonej mapie siatka miała
    ''' kilka pikseli albo leżała poza widokiem, więc nie było widać, czego dotyczy karta „Obszar pobierania”.
    ''' </summary>
    Private Sub PokazObszarNaMapie()
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then
            PokazBladSesji("Nie ma obszaru pobierania. Kliknij Zaznacz obszar albo wybierz Obszar pobierania → Zmień ▾.", "Brak obszaru")
            Return
        End If
        WyczyscZnaczniki()
        PokazZnaczniki()
        Dim u = Ustawienia.Uklad
        Dim z = s.ZasiegSiatki
        Dim narozniki = {u.DoWgs84(z.LewyGorny), u.DoWgs84(z.PrawyGorny), u.DoWgs84(z.PrawyDolny), u.DoWgs84(z.LewyDolny)}
        Dim gora = narozniki.Max(Function(p) p.Szerokosc), dol = narozniki.Min(Function(p) p.Szerokosc)
        Dim lewa = narozniki.Min(Function(p) p.Dlugosc), prawa = narozniki.Max(Function(p) p.Dlugosc)
        Dim dSz = gora - dol, dDl = prawa - lewa
        mapa.SetZoomToFitRect(RectLatLng.FromLTRB(lewa - 0.2 * dDl, gora + 0.25 * dSz, prawa + 0.2 * dDl, dol - 0.2 * dSz))
    End Sub

    ''' <summary>Czy siatka obszaru jest na mapie widoczna i ma co najmniej kilkanaście pikseli.</summary>
    Private Function ObszarCzytelnyNaMapie() As Boolean
        Dim s = BiezacaSiatka()
        If Not s.Poprawna Then Return True
        Dim u = Ustawienia.Uklad
        Dim z = s.ZasiegSiatki
        Dim a = u.DoWgs84(z.LewyDolny), b = u.DoWgs84(z.PrawyGorny)
        Dim pa = mapa.FromLatLngToLocal(New PointLatLng(a.Szerokosc, a.Dlugosc))
        Dim pb = mapa.FromLatLngToLocal(New PointLatLng(b.Szerokosc, b.Dlugosc))
        Dim prostokat = Rectangle.FromLTRB(CInt(Math.Min(pa.X, pb.X)), CInt(Math.Min(pa.Y, pb.Y)),
                                           CInt(Math.Max(pa.X, pb.X)), CInt(Math.Max(pa.Y, pb.Y)))
        Return prostokat.Width >= Px(16) AndAlso prostokat.Height >= Px(16) AndAlso mapa.ClientRectangle.IntersectsWith(prostokat)
    End Function

    Private _poleFormatuSegmentow As ComboBox

    Private Sub UlozUstawieniaBelki()
        Dim uklad As New TableLayoutPanel With {.ColumnCount = 4, .RowCount = 3, .Dock = DockStyle.Fill, .BackColor = Color.White,
                                                .Padding = New Padding(Px(8), Px(3), Px(8), Px(3))}
        For Each udzial In {35, 16, 23, 26}
            uklad.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, udzial))
        Next
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, Px(22)))
        uklad.RowStyles.Add(New RowStyle(SizeType.Absolute, Px(32)))
        uklad.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        For Each para In {(lblOpisPiksela, "Rozdzielczość"), (lblOpisSegmentu, "Bok segmentu [px]"),
                          (lblUklad, "Układ współrzędnych"), (lblOpisFormatu, "Format segmentów")}
            para.Item1.Text = para.Item2
            para.Item1.Font = _czcionkaNaglowka
            para.Item1.ForeColor = KolorPomocniczy
        Next
        ' Format pobieranych segmentów obok pozostałych parametrów pobierania - dawniej był tylko w oknie Ustawienia,
        ' a w tym miejscu stał format eksportu (audyt ER-03, ER-04).
        _poleFormatuSegmentow = New ComboBox With {.Name = "cmbFormatSegmentow", .DropDownStyle = ComboBoxStyle.DropDownList,
                                                   .AccessibleName = "Format segmentów"}
        For Each f In OpisyFormatowSegmentow
            _poleFormatuSegmentow.Items.Add(f.Opis)
        Next
        ToolTip1.SetToolTip(_poleFormatuSegmentow, "Format plików pobieranych segmentów. JPEG daje mniejsze pliki; PNG zachowuje przezroczystość nakładek. " &
                            "Format pliku mapy (MBTiles, KMZ, GeoTIFF…) wybierasz w oknie eksportu.")
        AddHandler _poleFormatuSegmentow.SelectedIndexChanged, AddressOf PoleFormatuSegmentow_Zmiana
        AddHandler stFormat.TextChanged, Sub() PokazFormatSegmentow()
        PokazFormatSegmentow()
        For Each kontrolka As Control In {CType(txtRozmiarPiksela, Control), cmbRozmiarPiksela, txtBokSegmentu, cmbUklad, _poleFormatuSegmentow}
            kontrolka.Font = _czcionkaRobocza
        Next
        Dim rozdzielczosc As New TableLayoutPanel With {.ColumnCount = 2, .RowCount = 1}
        _ukladRozdzielczosci = rozdzielczosc
        rozdzielczosc.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        rozdzielczosc.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 0))
        DolozKomorke(rozdzielczosc, cmbRozmiarPiksela, 0, 0)
        DolozKomorke(rozdzielczosc, txtRozmiarPiksela, 1, 0)
        AddHandler cmbRozmiarPiksela.SelectedIndexChanged, Sub() OdswiezEdytorRozdzielczosci()
        AddHandler txtRozmiarPiksela.Validated, Sub() AktualizujPresetPiksela(Wartosc(txtRozmiarPiksela.Text))
        DolozKomorke(uklad, lblOpisPiksela, 0, 0)
        DolozKomorke(uklad, lblOpisSegmentu, 1, 0)
        DolozKomorke(uklad, lblUklad, 2, 0)
        DolozKomorke(uklad, lblOpisFormatu, 3, 0)
        DolozKomorke(uklad, rozdzielczosc, 0, 1)
        DolozKomorke(uklad, txtBokSegmentu, 1, 1)
        DolozKomorke(uklad, cmbUklad, 2, 1)
        DolozKomorke(uklad, _poleFormatuSegmentow, 3, 1)
        DolozKomorke(uklad, lblBokKm, 1, 2)
        DolozKomorke(uklad, _lnkOstrzezenieSegmentu, 2, 2)
        uklad.SetColumnSpan(_lnkOstrzezenieSegmentu, 2)
        lblTytulParametry.Visible = False
        lblJednostkaPiksela.Visible = False
        lblJednostkaSegmentu.Visible = False
        grpRozdzielczoscFormat.Controls.Add(uklad)
        uklad.BringToFront()
    End Sub

    Private Sub OdswiezEdytorRozdzielczosci()
        If _ukladRozdzielczosci Is Nothing Then Return
        Dim stopnie = Ustawienia.Uklad.Geograficzny
        Dim wlasna = stopnie OrElse cmbRozmiarPiksela.SelectedIndex = cmbRozmiarPiksela.Items.Count - 1
        lblOpisPiksela.Text = If(stopnie, "Rozdzielczość [°/px]", "Rozdzielczość [m/px]")
        cmbRozmiarPiksela.Visible = Not stopnie
        txtRozmiarPiksela.Visible = wlasna
        _ukladRozdzielczosci.ColumnStyles(0).SizeType = If(stopnie, SizeType.Absolute, SizeType.Percent)
        _ukladRozdzielczosci.ColumnStyles(0).Width = If(stopnie, 0, 100)
        _ukladRozdzielczosci.ColumnStyles(1).SizeType = If(stopnie, SizeType.Percent, SizeType.Absolute)
        _ukladRozdzielczosci.ColumnStyles(1).Width = If(stopnie, 100, If(wlasna, Px(70), 0))
    End Sub

    Private Sub OdswiezPodsumowaniePracy()
        If _podsumowanieObszaru Is Nothing Then Return
        Dim siatka = BiezacaSiatka()
        _wymiaryObszaru.Text = If(_korytarz IsNot Nothing, "Obszar wzdłuż trasy", lblWymiaryTerenu.Text)
        ' Układ współrzędnych jest w karcie parametrów, bo tam się go zmienia (dawniej stał w karcie obszaru).
        If siatka.Poprawna Then
            Dim liczba = siatka.LiczbaSegmentow
            Dim opis = $"{liczba} {Odmiana(liczba, "segment", "segmenty", "segmentów")}"
            If _korytarz IsNot Nothing Then opis = "Siatka: " & opis
            _podsumowanieObszaru.Text = opis
        Else
            _podsumowanieObszaru.Text = "Brak obszaru"
        End If
        Dim jednostka = If(Ustawienia.Uklad.Geograficzny, "°/px", "m/px")
        _rozdzielczoscBelki.Text = $"{txtRozmiarPiksela.Text} {jednostka}"
        _podsumowanieParametrow.Text = $"{KrotkaNazwaUkladu(Ustawienia.Uklad.Nazwa)} · bok {txtBokSegmentu.Text} px · {stFormat.Text.TrimStart("."c).ToUpperInvariant()}"
        ToolTip1.SetToolTip(_podsumowanieParametrow, $"{Ustawienia.Uklad.Nazwa}, bok segmentu {txtBokSegmentu.Text} px, format {stFormat.Text.TrimStart("."c).ToUpperInvariant()}")
        OdswiezEdytorRozdzielczosci()
        OdswiezPodpowiedzBelki()
        AktualizujWspolrzednePaska()
    End Sub

    ''' <summary>Nazwa układu bez kodu EPSG w nawiasie, np. „PL-1992” z „PL-1992 (EPSG:2180)” (pełna nazwa jest w podpowiedzi).</summary>
    Friend Shared Function KrotkaNazwaUkladu(nazwa As String) As String
        Dim nawias = nazwa.IndexOf(" (EPSG", StringComparison.Ordinal)
        Return If(nawias > 0, nazwa.Substring(0, nawias), nazwa)
    End Function

    Private Sub OdswiezZakladkeSesji()
        If _folderZakladki Is Nothing Then Return
        _folderZakladki.Text = FolderSesji
        If _zakladkiPracy.SelectedTab IsNot _zakladkaSesji Then Return
        Dim konfiguracja = BiezacaKonfiguracja()
        Try
            Dim plik = If(String.IsNullOrWhiteSpace(FolderSesji), "", Path.Combine(FolderSesji, "conf.txt"))
            If File.Exists(plik) Then konfiguracja = KonfiguracjaSesji.Wczytaj(plik, KodowanieSystemowe())
        Catch ex As IOException
            _informacjeOPlikach.Text = "Nie udało się odczytać ustawień sesji: " & OpisBledu.Z(ex)
            Return
        Catch ex As UnauthorizedAccessException
            _informacjeOPlikach.Text = "Nie udało się odczytać ustawień sesji: " & OpisBledu.Z(ex)
            Return
        End Try
        Dim styl = Siatka.StylZTekstu(konfiguracja.Numeracja)
        Dim nazwaNumeracji = If(styl = StylNumeracji.WierszKolumna, "Wiersz–kolumna",
                               If(styl = StylNumeracji.KolejnyDwucyfrowy, "Kolejna (01, 02…)", "Kolejna (1, 2…)"))
        Dim siatkaSesji = konfiguracja.SiatkaSesji()
        Dim nazwaPliku = If(konfiguracja.TrekBuddy, siatkaSesji.NazwaSegmentuTrekBuddy(konfiguracja.Prefiks, 1, 1),
                           siatkaSesji.NazwaSegmentu(konfiguracja.Prefiks, styl, 1, 1))
        _informacjeOPlikach.Text = "Numeracja: " & If(konfiguracja.TrekBuddy, "TrekBuddy (piksele X/Y)", nazwaNumeracji) & vbCrLf &
                                   "Przykładowy plik: " & nazwaPliku & "." & Wms.RozszerzeniePliku(konfiguracja.Format)
    End Sub

    Private Sub DopiszKomunikatSesji(tekst As String, kolor As Color)
        If IsDisposed OrElse Disposing OrElse _dziennikSesji Is Nothing OrElse _dziennikSesji.IsDisposed OrElse _przeciaganyUchwyt IsNot Nothing Then Return
        If Not mapa.SelectedArea.IsEmpty AndAlso Control.MouseButtons <> MouseButtons.None Then Return
        ' Na mapie i w postępie powtarzają się krótkie komunikaty. Ograniczamy rozmiar historii w pamięci.
        If _dziennikSesji.TextLength > 26000 Then
            _dziennikSesji.ReadOnly = False
            Try
                _dziennikSesji.Select(0, 12000)
                _dziennikSesji.SelectedText = ""
            Finally
                _dziennikSesji.ReadOnly = True
            End Try
        End If
        _dziennikSesji.SelectionStart = _dziennikSesji.TextLength
        _dziennikSesji.SelectionColor = If(kolor = Color.Red, Color.Firebrick, SystemColors.WindowText)
        _dziennikSesji.AppendText($"{Date.Now:HH:mm}  {tekst}{vbCrLf}")
        _dziennikSesji.ScrollToCaret()
        Dim maTekst = _dziennikSesji.TextLength > 0
        If _btnKopiujKomunikaty IsNot Nothing Then _btnKopiujKomunikaty.Enabled = maTekst
        If _btnWyczyscKomunikaty IsNot Nothing Then _btnWyczyscKomunikaty.Enabled = maTekst
        If kolor = Color.Red AndAlso _zakladkiPracy.SelectedTab IsNot _zakladkaKomunikatow Then
            _liczbaBledowSesji += 1
            DopasujSzerokoscZakladek()
        End If
    End Sub

    Friend Sub WyzerujLicznikBledow()
        If _liczbaBledowSesji = 0 Then Return
        _liczbaBledowSesji = 0
        DopasujSzerokoscZakladek()
    End Sub

    ''' <summary>Błąd ostatniej czynności użytkownika pokazywany w karcie „Stan sesji” do najbliższej zmiany obszaru, warstw lub parametrów.</summary>
    Private _bladSesji As String = ""
    Private _naglowekBleduSesji As String = ""

    ''' <summary>
    ''' Błąd, który zatrzymał czynność użytkownika (np. start pobierania). Dawniej trafiał tylko do zakładki „Komunikaty”,
    ''' więc przy otwartym podglądzie po kliknięciu „Pobierz segmenty” nic się nie działo (audyt ER-01). Teraz karta
    ''' „Stan sesji” pokazuje go na czerwono, a dziennik zachowuje historię.
    ''' </summary>
    Friend Sub PokazBladSesji(tekst As String, Optional naglowek As String = "Nie można kontynuować")
        Komunikat(tekst, Color.Red)
        _bladSesji = tekst
        _naglowekBleduSesji = naglowek
        OdswiezPodpowiedzBelki()
    End Sub

    ''' <summary>Usuwa z karty „Stan sesji” błąd poprzedniej czynności (po zmianie, która mogła go usunąć).</summary>
    Friend Sub WyczyscBladSesji()
        If _bladSesji = "" Then Return
        _bladSesji = ""
        _naglowekBleduSesji = ""
        OdswiezPodpowiedzBelki()
    End Sub

    ''' <summary>Tekst błędu pokazywanego w karcie „Stan sesji” (pusty - brak błędu).</summary>
    Friend ReadOnly Property BladSesji As String
        Get
            Return _bladSesji
        End Get
    End Property

    Private Sub OdswiezPodpowiedzBelki()
        If _podpowiedzBelki Is Nothing Then Return
        Dim trwa = _przerwanie IsNot Nothing
        Dim tekst = lblStatusSesji.Text
        Dim szczegoly As String = ""
        If tekst = "" Then
            If _zestaw.Liczba = 0 Then
                tekst = "Wybierz warstwę"
                szczegoly = "Zaznacz ją w katalogu warstw."
            ElseIf BiezacaSiatka().Poprawna Then
                tekst = "Gotowe do pobrania"
                If lblSzacowanyCzas.Text <> "-" AndAlso lblSzacowanyCzas.Text <> "" Then
                    szczegoly = $"Szacunek: {lblSzacowanyRozmiar.Text} · {lblSzacowanyCzas.Text}"
                End If
            Else
                tekst = "Zaznacz obszar"
                szczegoly = "Kliknij Zaznacz obszar albo przeciągnij prawym przyciskiem myszy na mapie."
            End If
        ElseIf tekst.StartsWith("Zmieniono ", StringComparison.Ordinal) Then
            Dim kropka = tekst.IndexOf("."c)
            If kropka > 0 Then
                szczegoly = tekst.Substring(kropka + 1).Trim()
                tekst = tekst.Substring(0, kropka)
            End If
        ElseIf _podswietlEksport Then
            szczegoly = "Utwórz plik mapy z pobranych segmentów."
        ElseIf trwa Then
            szczegoly = stSegment.Text
        ElseIf _ostatniWynikPobierania.HasValue AndAlso _ostatniWynikPobierania.Value.Nieudane > 0 Then
            szczegoly = "Pobierz brakujące albo eksportuj mapę z białymi polami. Wykaz błędów: zakładka Komunikaty."
        ElseIf _czesciowoPobrana Then
            szczegoly = "Część segmentów jest pobrana - dokończ pobieranie albo eksportuj to, co jest."
        End If
        Dim blad = Not trwa AndAlso _bladSesji <> ""
        If blad Then
            tekst = _naglowekBleduSesji
            szczegoly = _bladSesji
        End If
        _podpowiedzBelki.Text = tekst.TrimEnd("."c)
        _podpowiedzBelki.ForeColor = If(blad, Color.Firebrick, If(lblStatusSesji.Text = "", Atrament, lblStatusSesji.ForeColor))
        _szczegolyStanuBelki.ForeColor = If(blad, Color.Firebrick, KolorPomocniczy)
        _szczegolyStanuBelki.Text = szczegoly
        ToolTip1.SetToolTip(_szczegolyStanuBelki, szczegoly)
        _postepBelki.Visible = trwa
        btnPrzerwij.Visible = trwa
        btnPobierz.Visible = Not trwa
        btnPrzejdzDoEksportu.Visible = Not trwa AndAlso (_podswietlEksport OrElse _czesciowoPobrana)
        btnPobierz.Enabled = Not trwa AndAlso _zestaw.Liczba > 0 AndAlso BiezacaSiatka().Poprawna
        OdswiezWygladPobierania()
        _szczegolyBledow.Visible = Not trwa AndAlso Not String.IsNullOrWhiteSpace(FolderSesji) AndAlso
                                  File.Exists(Path.Combine(FolderSesji, "error.txt"))
        OdswiezPasekSesji()
    End Sub

    ''' <summary>
    ''' Wygląd przycisku pobierania: wyróżniony kolorem, gdy pobieranie jest główną czynnością; biały, gdy główną
    ''' czynnością jest eksport; przygaszony, gdy brakuje warstwy lub obszaru (powód podaje karta „Stan sesji”).
    ''' </summary>
    Private Sub OdswiezWygladPobierania()
        If _czcionkaAkcji Is Nothing Then Return
        Dim glowna = btnPobierz.Enabled AndAlso Not _podswietlEksport AndAlso _przerwanie Is Nothing
        If Not btnPobierz.Enabled Then
            btnPobierz.BackColor = TloNieaktywne
            btnPobierz.ForeColor = KolorPomocniczy
            btnPobierz.FlatAppearance.BorderColor = KolorLinii
            btnPobierz.FlatAppearance.MouseOverBackColor = TloNieaktywne
            btnPobierz.FlatAppearance.MouseDownBackColor = TloNieaktywne
        Else
            StylPrzyciskuAkcji(btnPobierz, glowna)
        End If
        btnPobierz.Font = If(glowna, _czcionkaAkcji, _czcionkaRobocza)
        Dim powod = If(_zestaw.Liczba = 0, "Najpierw zaznacz warstwę w katalogu po prawej.",
                       If(Not BiezacaSiatka().Poprawna, "Najpierw zaznacz obszar na mapie lub wpisz współrzędne.", ""))
        ToolTip1.SetToolTip(_panelPobierania, If(btnPobierz.Enabled, "", powod))
    End Sub

    Private Sub PokazSzczegolyPobierania()
        _zakladkiPracy.SelectedTab = _zakladkaKomunikatow
        Try
            Dim plik = Path.Combine(FolderSesji, "error.txt")
            Using czytnik As New StreamReader(plik, KodowanieSystemowe())
                Dim znaki(1999) As Char
                Dim liczba = czytnik.ReadBlock(znaki, 0, znaki.Length)
                Dim tekst = New String(znaki, 0, liczba)
                If Not czytnik.EndOfStream Then tekst &= vbCrLf & "Pełny wykaz znajduje się w pliku error.txt w folderze sesji."
                DopiszKomunikatSesji(MetadaneMapy.MaskujSekrety(tekst), SystemColors.WindowText)
            End Using
        Catch ex As IOException
            DopiszKomunikatSesji("Nie udało się odczytać wykazu błędów: " & OpisBledu.Z(ex), Color.Red)
        Catch ex As UnauthorizedAccessException
            DopiszKomunikatSesji("Nie udało się odczytać wykazu błędów: " & OpisBledu.Z(ex), Color.Red)
        End Try
    End Sub

    Private Class PlaskaTabelaKolorow
        Inherits ProfessionalColorTable

        Public Overrides ReadOnly Property MenuStripGradientBegin As Color
            Get
                Return Color.White
            End Get
        End Property
        Public Overrides ReadOnly Property MenuStripGradientEnd As Color
            Get
                Return Color.White
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientBegin As Color
            Get
                Return TloPaneli
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientEnd As Color
            Get
                Return TloPaneli
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return Color.White
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return TloPaneli
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return TloPaneli
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return TloPaneli
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return TloNaglowka
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return TloNaglowka
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return TloNaglowka
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
            Get
                Return TloBelki
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientMiddle As Color
            Get
                Return TloBelki
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
            Get
                Return TloBelki
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return KolorRamkiKarty
            End Get
        End Property
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return KolorRamkiKarty
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return KolorLinii
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return Color.White
            End Get
        End Property
    End Class

    Private Class PlaskiRendererPaska
        Inherits ToolStripProfessionalRenderer

        Public Sub New()
            MyBase.New(New PlaskaTabelaKolorow())
            RoundedEdges = False
        End Sub

        Protected Overrides Sub OnRenderToolStripBorder(e As ToolStripRenderEventArgs)
            If TypeOf e.ToolStrip Is StatusStrip Then
                Using pioro As New Pen(KolorRamkiKarty)
                    e.Graphics.DrawLine(pioro, 0, 0, e.ToolStrip.Width - 1, 0)
                End Using
            ElseIf TypeOf e.ToolStrip Is MenuStrip Then
                Using pioro As New Pen(KolorLinii)
                    e.Graphics.DrawLine(pioro, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1)
                End Using
            ElseIf TypeOf e.ToolStrip Is ToolStripDropDown Then
                Using pioro As New Pen(KolorRamkiKarty)
                    e.Graphics.DrawRectangle(pioro, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1)
                End Using
            Else
                MyBase.OnRenderToolStripBorder(e)
            End If
        End Sub
    End Class

    Private Class PlaskieZakladki
        Inherits TabControl

        Private _najechana As Integer = -1
        Public RysujZakladke As Action(Of Graphics, Integer, Rectangle, Boolean, Boolean)

        Public Sub New()
            SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g = e.Graphics
            g.Clear(TloPaneli)
            If TabCount = 0 OrElse ClientSize.Width < 2 OrElse ClientSize.Height < 2 Then Return
            Dim dolPaska = GetTabRect(0).Bottom
            Dim tresc As New Rectangle(0, dolPaska, ClientSize.Width, Math.Max(1, ClientSize.Height - dolPaska))
            Using biale As New SolidBrush(Color.White)
                g.FillRectangle(biale, tresc)
            End Using
            Using ramka As New Pen(KolorRamkiKarty)
                g.DrawRectangle(ramka, 0, dolPaska, ClientSize.Width - 1, Math.Max(1, ClientSize.Height - dolPaska - 1))
                Dim pasekAkcentu = Math.Max(2, CInt(3 * DeviceDpi / 96.0))
                For i = 0 To TabCount - 1
                    Dim r = GetTabRect(i)
                    Dim wybrana = (i = SelectedIndex)
                    Dim najechana = (i = _najechana)
                    Dim karta As New Rectangle(r.Left, r.Top, r.Width, Math.Max(1, dolPaska - r.Top))
                    If wybrana Then
                        Using biale As New SolidBrush(Color.White)
                            g.FillRectangle(biale, karta.Left, karta.Top, karta.Width, karta.Height + 1)
                        End Using
                        g.DrawLine(ramka, karta.Left, karta.Top, karta.Left, dolPaska)
                        g.DrawLine(ramka, karta.Right - 1, karta.Top, karta.Right - 1, dolPaska)
                        Using akcent As New SolidBrush(KolorAkcjiOkna)
                            g.FillRectangle(akcent, karta.Left, karta.Top, karta.Width, pasekAkcentu)
                        End Using
                    Else
                        Using tlo As New SolidBrush(If(najechana, TloNaglowka, TloPaneli))
                            g.FillRectangle(tlo, karta)
                        End Using
                    End If
                    RysujZakladke?.Invoke(g, i, r, wybrana, najechana)
                Next
            End Using
        End Sub

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            If Not IsHandleCreated Then Return
            Dim pod = -1
            For i = 0 To TabCount - 1
                If GetTabRect(i).Contains(e.Location) Then pod = i : Exit For
            Next
            If pod <> _najechana Then
                _najechana = pod
                Invalidate()
            End If
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            If _najechana <> -1 Then
                _najechana = -1
                Invalidate()
            End If
        End Sub
    End Class
End Class
