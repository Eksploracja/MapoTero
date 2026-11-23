Imports GMap.NET
Imports GMap.NET.MapProviders
Imports GMap.NET.WindowsForms

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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.btnPobierz = New System.Windows.Forms.Button()
        Me.grpDaneZrodlowe = New System.Windows.Forms.GroupBox()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.tvWarstwy = New MapoTero.DrzewoWarstw()
        Me.txtSzukajWarstwy = New System.Windows.Forms.TextBox()
        Me.btnZwinRozwin = New System.Windows.Forms.Button()
        Me.btnGaleriaWarstw = New System.Windows.Forms.Button()
        Me.lblUklad = New System.Windows.Forms.Label()
        Me.cmbUklad = New System.Windows.Forms.ComboBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.stPostep = New System.Windows.Forms.ToolStripProgressBar()
        Me.stFormat = New System.Windows.Forms.ToolStripStatusLabel()
        Me.stFolder = New System.Windows.Forms.ToolStripStatusLabel()
        Me.stSegment = New System.Windows.Forms.ToolStripStatusLabel()
        Me.btnPrzerwij = New System.Windows.Forms.Button()
        Me.SesjaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZapiszToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.WczytajToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.SesjaSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.KorytarzToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UsunKorytarzToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SesjaSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.OdswiezToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UsunPobraneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NarzedziaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DodajSerwerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FolderWlasnychWarstwToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZnajdzDzialkeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KonwertujOziToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KonwertujGeoTiffToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ProfilNmtToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.btnSzukajDzialki = New System.Windows.Forms.Button()
        Me.NarzedziaSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnDodajSerwer = New System.Windows.Forms.Button()
        Me.ScalanieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EksportToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EksportCadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KorektaFolderuToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NarzedziaSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.PrzeslijNaTelefonToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KopiujNaGarminaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UstawieniaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PomocOnlineToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GaleriaWarstwToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.InstrukcjaObsługiToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZglosBladToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FolderDanychToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.CoNowegoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.WesprzyjToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.Label47 = New System.Windows.Forms.Label()
        Me.Label46 = New System.Windows.Forms.Label()
        Me.txtZasiegSegmentuKm = New System.Windows.Forms.TextBox()
        Me.txtBokSegmentu = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblRozmiarPikselaOpis = New System.Windows.Forms.Label()
        Me.txtRozmiarPiksela = New System.Windows.Forms.TextBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpWybraneWarstwy = New System.Windows.Forms.GroupBox()
        Me.lstWybraneWarstwy = New MapoTero.ListaWarstw()
        Me.btnWarstwaWyzej = New System.Windows.Forms.Button()
        Me.btnWarstwaNizej = New System.Windows.Forms.Button()
        Me.btnUsunWarstwe = New System.Windows.Forms.Button()
        Me.lblKrycie = New System.Windows.Forms.Label()
        Me.numKrycie = New System.Windows.Forms.NumericUpDown()
        Me.btnWarstwyZaawansowane = New System.Windows.Forms.Button()
        Me.btnPowieksz = New System.Windows.Forms.Button()
        Me.btnPomniejsz = New System.Windows.Forms.Button()
        Me.pnlBelkaAkcji = New System.Windows.Forms.Panel()
        Me.btnKreatorDolny = New System.Windows.Forms.Button()
        Me.lblStatusSesji = New System.Windows.Forms.Label()
        Me.lnkSzczegoly = New System.Windows.Forms.LinkLabel()
        Me.btnOtworzFolderDolny = New System.Windows.Forms.Button()
        Me.lblLiniaBelki = New System.Windows.Forms.Label()
        Me.lblSeparator1 = New System.Windows.Forms.Label()
        Me.lblSeparator2 = New System.Windows.Forms.Label()
        Me.lblTytulObszar = New System.Windows.Forms.Label()
        Me.lblTytulParametry = New System.Windows.Forms.Label()
        Me.lblTytulPodglad = New System.Windows.Forms.Label()
        Me.grpObszar = New System.Windows.Forms.Panel()
        Me.lblOpisWymiary = New System.Windows.Forms.Label()
        Me.lblBadgeSegmenty = New System.Windows.Forms.Label()
        Me.lblWymiaryTerenu = New System.Windows.Forms.Label()
        Me.lblPowierzchniaTerenu = New System.Windows.Forms.Label()
        Me.pnlSzacunek = New System.Windows.Forms.Panel()
        Me.lblTytulRozmiar = New System.Windows.Forms.Label()
        Me.lblSzacowanyRozmiar = New System.Windows.Forms.Label()
        Me.lblSeparatorSzacunku = New System.Windows.Forms.Label()
        Me.lblTytulCzas = New System.Windows.Forms.Label()
        Me.lblSzacowanyCzas = New System.Windows.Forms.Label()
        Me.lblUkladZasiegSkrot = New System.Windows.Forms.Label()
        Me.btnWpiszWspolrzedne = New System.Windows.Forms.Button()
        Me.pnlUkrytePolaZasiegu = New System.Windows.Forms.Panel()
        Me.grpRozdzielczoscFormat = New System.Windows.Forms.Panel()
        Me.lblOpisPiksela = New System.Windows.Forms.Label()
        Me.lblJednostkaPiksela = New System.Windows.Forms.Label()
        Me.cmbRozmiarPiksela = New System.Windows.Forms.ComboBox()
        Me.lblOpisSegmentu = New System.Windows.Forms.Label()
        Me.lblJednostkaSegmentu = New System.Windows.Forms.Label()
        Me.lblBokKm = New System.Windows.Forms.Label()
        Me.lblOpisFormatu = New System.Windows.Forms.Label()
        Me.cmbFormatDocelowy = New System.Windows.Forms.ComboBox()
        Me.btnPrzejdzDoEksportu = New System.Windows.Forms.Button()
        Me.grpRozmiarSiatki = New System.Windows.Forms.GroupBox()
        Me.grpPodglad = New System.Windows.Forms.Panel()
        Me.picPodglad = New System.Windows.Forms.PictureBox()
        Me.btnPodgladZaawansowane = New System.Windows.Forms.Button()
        Me.btnZoomPlus = New System.Windows.Forms.Button()
        Me.btnZoomMinus = New System.Windows.Forms.Button()
        Me.btnZoomReset = New System.Windows.Forms.Button()
        Me.btnPodgladOdswiez = New System.Windows.Forms.Button()
        Me.lblPodgladLiczbaWarstw = New System.Windows.Forms.Label()
        Me.lblPodgladSkala = New System.Windows.Forms.Label()
        Me.lblPodgladStan = New System.Windows.Forms.Label()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.txtLiczbaWierszy = New System.Windows.Forms.TextBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.txtLiczbaKolumn = New System.Windows.Forms.TextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.txtWysokoscPx = New System.Windows.Forms.TextBox()
        Me.txtSzerokoscPx = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.txtSzerokoscKm = New System.Windows.Forms.TextBox()
        Me.txtWysokoscKm = New System.Windows.Forms.TextBox()
        Me.Label42 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.btnUsunPobrane = New System.Windows.Forms.Button()
        Me.btnZnaczniki = New System.Windows.Forms.Button()
        Me.btnScalanie = New System.Windows.Forms.Button()
        Me.btnOtworzFolder = New System.Windows.Forms.Button()
        Me.mapa = New GMap.NET.WindowsForms.GMapControl()
        Me.rbOsm = New System.Windows.Forms.RadioButton()
        Me.rbGoogle = New System.Windows.Forms.RadioButton()
        Me.rbBing = New System.Windows.Forms.RadioButton()
        Me.lblSrodekSzer = New System.Windows.Forms.Label()
        Me.lblSrodekOpisSzer = New System.Windows.Forms.Label()
        Me.lblSrodekTytul = New System.Windows.Forms.Label()
        Me.lblKursorTytul = New System.Windows.Forms.Label()
        Me.lblKursorUkladOpis = New System.Windows.Forms.Label()
        Me.lblKursorUklad = New System.Windows.Forms.Label()
        Me.lblSrodekDlug = New System.Windows.Forms.Label()
        Me.lblZoom = New System.Windows.Forms.Label()
        Me.lblSrodekOpisDlug = New System.Windows.Forms.Label()
        Me.Label67 = New System.Windows.Forms.Label()
        Me.txtKomunikaty = New System.Windows.Forms.RichTextBox()
        Me.lblKursorWgs = New System.Windows.Forms.Label()
        Me.grpSchemat = New System.Windows.Forms.GroupBox()
        Me.lblZaznDlugLewa = New System.Windows.Forms.Label()
        Me.lblZaznSzerDol = New System.Windows.Forms.Label()
        Me.lblZaznDlugPrawa = New System.Windows.Forms.Label()
        Me.lblZaznSzerGora = New System.Windows.Forms.Label()
        Me.lblOpisYPrawy = New System.Windows.Forms.Label()
        Me.lblOpisXGora = New System.Windows.Forms.Label()
        Me.txtYPrawy = New System.Windows.Forms.TextBox()
        Me.txtXGora = New System.Windows.Forms.TextBox()
        Me.lblOpisYLewy = New System.Windows.Forms.Label()
        Me.lblOpisXDol = New System.Windows.Forms.Label()
        Me.txtYLewy = New System.Windows.Forms.TextBox()
        Me.txtXDol = New System.Windows.Forms.TextBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.chkEdycjaXY = New System.Windows.Forms.CheckBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.splPanel = New System.Windows.Forms.Splitter()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.grpDaneZrodlowe.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.grpWybraneWarstwy.SuspendLayout()
        CType(Me.numKrycie, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpObszar.SuspendLayout()
        Me.pnlSzacunek.SuspendLayout()
        Me.pnlUkrytePolaZasiegu.SuspendLayout()
        Me.grpRozdzielczoscFormat.SuspendLayout()
        Me.grpPodglad.SuspendLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlBelkaAkcji.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnPobierz
        '
        Me.btnPobierz.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPobierz.BackColor = System.Drawing.SystemColors.ActiveBorder
        Me.btnPobierz.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPobierz.Location = New System.Drawing.Point(110, 569)
        Me.btnPobierz.Name = "btnPobierz"
        Me.btnPobierz.Size = New System.Drawing.Size(125, 36)
        Me.btnPobierz.TabIndex = 6
        Me.btnPobierz.Tag = ""
        Me.btnPobierz.Text = "Pobierz segmenty"
        Me.ToolTip1.SetToolTip(Me.btnPobierz, "Rozpocznij pobieranie mapy (albo wznów pobieranie bieżącej sesji)")
        Me.btnPobierz.UseVisualStyleBackColor = False
        '
        'grpDaneZrodlowe
        '
        Me.grpDaneZrodlowe.Controls.Add(Me.Label29)
        Me.grpDaneZrodlowe.Controls.Add(Me.tvWarstwy)
        Me.grpDaneZrodlowe.Controls.Add(Me.txtSzukajWarstwy)
        Me.grpDaneZrodlowe.Controls.Add(Me.btnZwinRozwin)
        Me.grpDaneZrodlowe.Controls.Add(Me.btnDodajSerwer)
        Me.grpDaneZrodlowe.Controls.Add(Me.btnGaleriaWarstw)
        Me.grpDaneZrodlowe.Controls.Add(Me.lblUklad)
        Me.grpDaneZrodlowe.Controls.Add(Me.cmbUklad)
        Me.grpDaneZrodlowe.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpDaneZrodlowe.Location = New System.Drawing.Point(5, 3)
        Me.grpDaneZrodlowe.Name = "grpDaneZrodlowe"
        Me.grpDaneZrodlowe.Size = New System.Drawing.Size(212, 318)
        Me.grpDaneZrodlowe.TabIndex = 210
        Me.grpDaneZrodlowe.TabStop = False
        Me.grpDaneZrodlowe.Text = "Dane źródłowe"
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label29.Location = New System.Drawing.Point(6, 16)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(196, 13)
        Me.Label29.TabIndex = 216
        Me.Label29.Text = " Wybierz mapy (zbiory map i ich warstwy)"
        Me.ToolTip1.SetToolTip(Me.Label29, "Zbiory map (serwery WMS / WMTS z plików w folderze warstwy) i ich warstwy. Zaznacz warstwy jednego " &
        "lub kilku serwerów - trafią na listę wybranych warstw poniżej.")
        '
        'tvWarstwy
        '
        Me.tvWarstwy.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tvWarstwy.Location = New System.Drawing.Point(6, 54)
        Me.tvWarstwy.Name = "tvWarstwy"
        Me.tvWarstwy.Size = New System.Drawing.Size(198, 216)
        Me.tvWarstwy.TabIndex = 219
        '
        'txtSzukajWarstwy
        '
        Me.txtSzukajWarstwy.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSzukajWarstwy.Location = New System.Drawing.Point(6, 30)
        Me.txtSzukajWarstwy.Name = "txtSzukajWarstwy"
        Me.txtSzukajWarstwy.PlaceholderText = "szukaj warstwy…"
        Me.txtSzukajWarstwy.Size = New System.Drawing.Size(118, 20)
        Me.txtSzukajWarstwy.TabIndex = 215
        Me.ToolTip1.SetToolTip(Me.txtSzukajWarstwy, "Pokaż tylko warstwy, których nazwa lub nazwa zbioru map zawiera wpisane słowa" & _
        " (wielkość liter i polskie znaki nie mają znaczenia). Esc - wyczyść.")
        '
        'btnZwinRozwin
        '
        Me.btnZwinRozwin.Font = New System.Drawing.Font("Segoe UI Symbol", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZwinRozwin.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZwinRozwin.Location = New System.Drawing.Point(127, 28)
        Me.btnZwinRozwin.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZwinRozwin.Name = "btnZwinRozwin"
        Me.btnZwinRozwin.Size = New System.Drawing.Size(25, 23)
        Me.btnZwinRozwin.TabIndex = 216
        Me.btnZwinRozwin.Text = "⊞"
        Me.ToolTip1.SetToolTip(Me.btnZwinRozwin, "Rozwiń wszystkie zbiory map")
        Me.btnZwinRozwin.UseVisualStyleBackColor = True
        '
        'btnDodajSerwer
        '
        Me.btnDodajSerwer.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnDodajSerwer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDodajSerwer.Location = New System.Drawing.Point(153, 28)
        Me.btnDodajSerwer.Name = "btnDodajSerwer"
        Me.btnDodajSerwer.Size = New System.Drawing.Size(25, 23)
        Me.btnDodajSerwer.TabIndex = 217
        Me.btnDodajSerwer.Text = "+"
        Me.ToolTip1.SetToolTip(Me.btnDodajSerwer, "Dodaj nowy serwer WMS / WMTS (kreator pobierania warstw)...")
        Me.btnDodajSerwer.UseVisualStyleBackColor = True
        '
        'btnGaleriaWarstw
        '
        Me.btnGaleriaWarstw.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnGaleriaWarstw.Location = New System.Drawing.Point(179, 28)
        Me.btnGaleriaWarstw.Margin = New System.Windows.Forms.Padding(0)
        Me.btnGaleriaWarstw.Name = "btnGaleriaWarstw"
        Me.btnGaleriaWarstw.Size = New System.Drawing.Size(25, 23)
        Me.btnGaleriaWarstw.TabIndex = 218
        Me.ToolTip1.SetToolTip(Me.btnGaleriaWarstw, "Galeria warstw map (podgląd online) - zobacz wygląd warstw i dodaj wybraną jednym kliknięciem")
        Me.btnGaleriaWarstw.UseVisualStyleBackColor = True
        '
        'lblUklad
        '
        Me.lblUklad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblUklad.AutoSize = True
        Me.lblUklad.Location = New System.Drawing.Point(6, 272)
        Me.lblUklad.Name = "lblUklad"
        Me.lblUklad.Size = New System.Drawing.Size(160, 13)
        Me.lblUklad.TabIndex = 217
        Me.lblUklad.Text = " Układ współrzędnych pobierania"
        Me.ToolTip1.SetToolTip(Me.lblUklad, "Układ, w którym pobierane są segmenty i zapisywane ich pliki georeferencyjne. Serwer WMS musi go obsługiwać.")
        '
        'cmbUklad
        '
        Me.cmbUklad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbUklad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUklad.FormattingEnabled = True
        Me.cmbUklad.Location = New System.Drawing.Point(6, 287)
        Me.cmbUklad.Name = "cmbUklad"
        Me.cmbUklad.Size = New System.Drawing.Size(198, 21)
        Me.cmbUklad.TabIndex = 218
        Me.ToolTip1.SetToolTip(Me.cmbUklad, "Układ, w którym pobierane są segmenty i zapisywane ich pliki georeferencyjne. Serwer WMS musi go obsługiwać.")
        '
        'Panel1
        '
        Me.Panel1.AutoSize = True
        Me.Panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.Location = New System.Drawing.Point(14, 315)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(0, 0)
        Me.Panel1.TabIndex = 213
        '
        'StatusStrip1
        '
        Me.StatusStrip1.ShowItemToolTips = True
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.stPostep, Me.stFormat, Me.stFolder, Me.stSegment})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 662)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(884, 22)
        Me.StatusStrip1.TabIndex = 237
        Me.StatusStrip1.Text = "StatusStrip1"
        Me.ToolTip1.SetToolTip(Me.StatusStrip1, "Informacje o aktualnym formacie plików rastrowych oraz lokalizacji katalogu ""down" &
        "load""")
        '
        'stPostep
        '
        Me.stPostep.Name = "stPostep"
        Me.stPostep.Size = New System.Drawing.Size(100, 16)
        '
        'stFormat
        '
        Me.stFormat.BackColor = System.Drawing.SystemColors.Window
        Me.stFormat.Name = "stFormat"
        Me.stFormat.Size = New System.Drawing.Size(70, 17)
        Me.stFormat.Text = "rozszerzenie"
        Me.stFormat.IsLink = True
        Me.stFormat.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.stFormat.ToolTipText = "Format pobieranych segmentów - kliknij, aby go zmienić w oknie Ustawienia"
        '
        'stFolder
        '
        Me.stFolder.Name = "stFolder"
        Me.stFolder.Size = New System.Drawing.Size(81, 17)
        Me.stFolder.Text = "wybierz folder"
        '
        'stSegment
        '
        Me.stSegment.Name = "stSegment"
        Me.stSegment.Size = New System.Drawing.Size(49, 17)
        Me.stSegment.Text = "kwadrat"
        '
        'btnPrzerwij
        '
        Me.btnPrzerwij.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPrzerwij.Enabled = False
        Me.btnPrzerwij.Location = New System.Drawing.Point(110, 613)
        Me.btnPrzerwij.Name = "btnPrzerwij"
        Me.btnPrzerwij.Size = New System.Drawing.Size(105, 21)
        Me.btnPrzerwij.TabIndex = 245
        Me.btnPrzerwij.Tag = ""
        Me.btnPrzerwij.Text = "Przerwij pobieranie"
        Me.ToolTip1.SetToolTip(Me.btnPrzerwij, "Przerwij pobieranie mapy - pobrane segmenty zostają w folderze")
        Me.btnPrzerwij.UseVisualStyleBackColor = True
        '
        'SesjaToolStripMenuItem
        '
        Me.SesjaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZapiszToolStripMenuItem, Me.WczytajToolStripMenuItem1, Me.SesjaSeparator1, Me.KorytarzToolStripMenuItem, Me.UsunKorytarzToolStripMenuItem, Me.SesjaSeparator2, Me.OdswiezToolStripMenuItem, Me.UsunPobraneToolStripMenuItem})
        Me.SesjaToolStripMenuItem.Name = "SesjaToolStripMenuItem"
        Me.SesjaToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.SesjaToolStripMenuItem.Size = New System.Drawing.Size(104, 20)
        Me.SesjaToolStripMenuItem.Text = "Sesja pobierania"
        Me.SesjaToolStripMenuItem.ToolTipText = "Wczytaj lub zapisz plik conf.txt, który przechowuje informacje m.in. o zasięgu po" &
    "bieranego obszaru"
        '
        'ZapiszToolStripMenuItem
        '
        Me.ZapiszToolStripMenuItem.Name = "ZapiszToolStripMenuItem"
        Me.ZapiszToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Z), System.Windows.Forms.Keys)
        Me.ZapiszToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.ZapiszToolStripMenuItem.Text = "Zapisz ustawienia sesji (conf.txt)"
        '
        'WczytajToolStripMenuItem1
        '
        Me.WczytajToolStripMenuItem1.Name = "WczytajToolStripMenuItem1"
        Me.WczytajToolStripMenuItem1.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.W), System.Windows.Forms.Keys)
        Me.WczytajToolStripMenuItem1.Size = New System.Drawing.Size(290, 22)
        Me.WczytajToolStripMenuItem1.Text = "Wczytaj ustawienia sesji (conf.txt)"
        '
        'SesjaSeparator1
        '
        Me.SesjaSeparator1.Name = "SesjaSeparator1"
        Me.SesjaSeparator1.Size = New System.Drawing.Size(287, 6)
        '
        'KorytarzToolStripMenuItem
        '
        Me.KorytarzToolStripMenuItem.Name = "KorytarzToolStripMenuItem"
        Me.KorytarzToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.KorytarzToolStripMenuItem.Text = "Obszar pobierania wzdłuż trasy (GPX, KML, KMZ)..."
        Me.KorytarzToolStripMenuItem.ToolTipText = "Pobieranie tylko segmentów w pasie wzdłuż trasy zaplanowanej np. w Mapy.com, Komoot lub Locus Map"
        '
        'UsunKorytarzToolStripMenuItem
        '
        Me.UsunKorytarzToolStripMenuItem.Enabled = False
        Me.UsunKorytarzToolStripMenuItem.Name = "UsunKorytarzToolStripMenuItem"
        Me.UsunKorytarzToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.UsunKorytarzToolStripMenuItem.Text = "Usuń obszar wzdłuż trasy (pobieranie całego prostokąta)"
        '
        'SesjaSeparator2
        '
        Me.SesjaSeparator2.Name = "SesjaSeparator2"
        Me.SesjaSeparator2.Size = New System.Drawing.Size(287, 6)
        '
        'OdswiezToolStripMenuItem
        '
        Me.OdswiezToolStripMenuItem.Name = "OdswiezToolStripMenuItem"
        Me.OdswiezToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.OdswiezToolStripMenuItem.Text = "Odśwież mapę (pobierz ponownie wszystkie segmenty)"
        Me.OdswiezToolStripMenuItem.ToolTipText = "Ponowne pobranie wszystkich segmentów bieżącej sesji (np. po aktualizacji ortofotomapy); stary plik segmentu jest zastępowany dopiero po udanym pobraniu nowego"
        '
        'UsunPobraneToolStripMenuItem
        '
        Me.UsunPobraneToolStripMenuItem.Name = "UsunPobraneToolStripMenuItem"
        Me.UsunPobraneToolStripMenuItem.Size = New System.Drawing.Size(290, 22)
        Me.UsunPobraneToolStripMenuItem.Text = "Usuń pobrane pliki bieżącej sesji... (do Kosza)"
        Me.UsunPobraneToolStripMenuItem.ToolTipText = "Przenosi pobrane pliki bieżącej sesji do Kosza systemu Windows"
        '
        'NarzedziaToolStripMenuItem
        '
        Me.NarzedziaToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DodajSerwerToolStripMenuItem, Me.FolderWlasnychWarstwToolStripMenuItem, Me.ZnajdzDzialkeToolStripMenuItem, Me.KonwertujOziToolStripMenuItem, Me.KonwertujGeoTiffToolStripMenuItem, Me.ProfilNmtToolStripMenuItem, Me.NarzedziaSeparator1, Me.ScalanieToolStripMenuItem, Me.EksportToolStripMenuItem, Me.EksportCadToolStripMenuItem, Me.KorektaFolderuToolStripMenuItem, Me.NarzedziaSeparator2, Me.PrzeslijNaTelefonToolStripMenuItem, Me.KopiujNaGarminaToolStripMenuItem})
        Me.NarzedziaToolStripMenuItem.Name = "NarzedziaToolStripMenuItem"
        Me.NarzedziaToolStripMenuItem.Size = New System.Drawing.Size(70, 20)
        Me.NarzedziaToolStripMenuItem.Text = "Narzędzia"
        Me.NarzedziaToolStripMenuItem.ToolTipText = "Scalanie i eksport mapy, eksport do CAD, kreator serwerów, wyszukiwarka, konwerter OziExplorer, profil wysokościowy i korekta obrazów"
        '
        '
        'DodajSerwerToolStripMenuItem
        '
        Me.DodajSerwerToolStripMenuItem.Name = "DodajSerwerToolStripMenuItem"
        Me.DodajSerwerToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.DodajSerwerToolStripMenuItem.Text = "Dodaj nowy serwer WMS / WMTS..."
        Me.DodajSerwerToolStripMenuItem.ToolTipText = "Kreator dodawania serwera WMS lub WMTS (analiza GetCapabilities i wybór warstw)"
        '
        'FolderWlasnychWarstwToolStripMenuItem
        '
        Me.FolderWlasnychWarstwToolStripMenuItem.Name = "FolderWlasnychWarstwToolStripMenuItem"
        Me.FolderWlasnychWarstwToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.FolderWlasnychWarstwToolStripMenuItem.Text = "Otwórz folder własnych warstw..."
        Me.FolderWlasnychWarstwToolStripMenuItem.ToolTipText = "Otwiera folder na własne zbiory map (*.txt), chronione przed nadpisaniem przy aktualizacji programu"
        '
        'ZnajdzDzialkeToolStripMenuItem
        '
        Me.ZnajdzDzialkeToolStripMenuItem.Name = "ZnajdzDzialkeToolStripMenuItem"
        Me.ZnajdzDzialkeToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.F), System.Windows.Forms.Keys)
        Me.ZnajdzDzialkeToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.ZnajdzDzialkeToolStripMenuItem.Text = "Wyszukaj miejscowość lub działkę..."
        Me.ZnajdzDzialkeToolStripMenuItem.ToolTipText = "Wyszukuje miejscowość, adres lub działkę katastralną (Ctrl+F, Ctrl+D)"
        '
        'KonwertujOziToolStripMenuItem
        '
        Me.KonwertujOziToolStripMenuItem.Name = "KonwertujOziToolStripMenuItem"
        Me.KonwertujOziToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.KonwertujOziToolStripMenuItem.Text = "Konwertuj mapę OziExplorer (.map)..."
        Me.KonwertujOziToolStripMenuItem.ToolTipText = "Konwertuje kalibrowany skan mapy OziExplorer (.map) do JNX, KMZ, MBTiles, SQLiteDB albo OruxMaps"
        '
        'KonwertujGeoTiffToolStripMenuItem
        '
        Me.KonwertujGeoTiffToolStripMenuItem.Name = "KonwertujGeoTiffToolStripMenuItem"
        Me.KonwertujGeoTiffToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.KonwertujGeoTiffToolStripMenuItem.Text = "Konwertuj mapę GeoTIFF..."
        Me.KonwertujGeoTiffToolStripMenuItem.ToolTipText = "Kafelkowanie i konwersja rastra GeoTIFF do MBTiles, KMZ, JNX, SQLiteDB albo OruxMaps"
        '
        'ProfilNmtToolStripMenuItem
        '
        Me.ProfilNmtToolStripMenuItem.Name = "ProfilNmtToolStripMenuItem"
        Me.ProfilNmtToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.ProfilNmtToolStripMenuItem.Text = "Profil wysokościowy trasy (GPX, KML)..."
        Me.ProfilNmtToolStripMenuItem.ToolTipText = "Wysokości terenu z NMT GUGiK dla punktów trasy, wykres profilu i zapis GPX z wysokościami"
        '
        'NarzedziaSeparator1
        '
        Me.NarzedziaSeparator1.Name = "NarzedziaSeparator1"
        Me.NarzedziaSeparator1.Size = New System.Drawing.Size(312, 6)
        'ScalanieToolStripMenuItem
        '
        Me.ScalanieToolStripMenuItem.Name = "ScalanieToolStripMenuItem"
        Me.ScalanieToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.L), System.Windows.Forms.Keys)
        Me.ScalanieToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.ScalanieToolStripMenuItem.Text = "Scalanie segmentów"
        '
        'EksportToolStripMenuItem
        '
        Me.EksportToolStripMenuItem.Name = "EksportToolStripMenuItem"
        Me.EksportToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.E), System.Windows.Forms.Keys)
        Me.EksportToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.EksportToolStripMenuItem.Text = "Eksport mapy (JNX, KMZ, MBTiles, SQLiteDB, OruxMaps, arkusz, TrekBuddy)"
        '
        'EksportCadToolStripMenuItem
        '
        Me.EksportCadToolStripMenuItem.Name = "EksportCadToolStripMenuItem"
        Me.EksportCadToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.EksportCadToolStripMenuItem.Text = "Eksport do CAD (DXF, skrypt SCR, LISP)..."
        Me.EksportCadToolStripMenuItem.ToolTipText = "Rysunek DXF i skrypt SCR dla AutoCAD, AutoCAD LT, BricsCAD, ZWCAD: rastry na właściwych współrzędnych, obrys i siatka segmentów"
        '
        'KorektaFolderuToolStripMenuItem
        '
        Me.KorektaFolderuToolStripMenuItem.Name = "KorektaFolderuToolStripMenuItem"
        Me.KorektaFolderuToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.KorektaFolderuToolStripMenuItem.Text = "Korekta obrazów w folderze..."
        Me.KorektaFolderuToolStripMenuItem.ToolTipText = "Wsadowe przetwarzanie obrazów: kontrast, ostrość, rozjaśnienie lasów i profil Garmin MIP"
        '
        'NarzedziaSeparator2
        '
        Me.NarzedziaSeparator2.Name = "NarzedziaSeparator2"
        Me.NarzedziaSeparator2.Size = New System.Drawing.Size(312, 6)
        '
        'PrzeslijNaTelefonToolStripMenuItem
        '
        Me.PrzeslijNaTelefonToolStripMenuItem.Name = "PrzeslijNaTelefonToolStripMenuItem"
        Me.PrzeslijNaTelefonToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.PrzeslijNaTelefonToolStripMenuItem.Text = "Prześlij mapę na telefon (Wi-Fi)..."
        Me.PrzeslijNaTelefonToolStripMenuItem.ToolTipText = "Przesyłanie wyeksportowanej mapy (MBTiles, GeoPackage, GeoTIFF, KMZ) na telefon przez sieć lokalną z kodem QR"
        '
        'KopiujNaGarminaToolStripMenuItem
        '
        Me.KopiujNaGarminaToolStripMenuItem.Name = "KopiujNaGarminaToolStripMenuItem"
        Me.KopiujNaGarminaToolStripMenuItem.Size = New System.Drawing.Size(315, 22)
        Me.KopiujNaGarminaToolStripMenuItem.Text = "Kopiuj mapę do Garmina (USB)..."
        Me.KopiujNaGarminaToolStripMenuItem.ToolTipText = "Wykryj odbiornik Garmin lub kartę microSD i skopiuj plik .kmz lub .jnx do odpowiedniego folderu."
        '
        'UstawieniaToolStripMenuItem
        '
        Me.UstawieniaToolStripMenuItem.Name = "UstawieniaToolStripMenuItem"
        Me.UstawieniaToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
        Me.UstawieniaToolStripMenuItem.Text = "Ustawienia"
        Me.UstawieniaToolStripMenuItem.ToolTipText = "Główne ustawienia programu"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PomocOnlineToolStripMenuItem, Me.GaleriaWarstwToolStripMenuItem, Me.InstrukcjaObsługiToolStripMenuItem, Me.HelpSeparator1, Me.PomocPomorskieForumEksploracyjneToolStripMenuItem, Me.ZglosBladToolStripMenuItem, Me.FolderDanychToolStripMenuItem, Me.HelpSeparator2, Me.CoNowegoToolStripMenuItem, Me.WesprzyjToolStripMenuItem, Me.AboutToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(57, 20)
        Me.HelpToolStripMenuItem.Text = "Pomoc"
        '
        'PomocOnlineToolStripMenuItem
        '
        Me.PomocOnlineToolStripMenuItem.Name = "PomocOnlineToolStripMenuItem"
        Me.PomocOnlineToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
        Me.PomocOnlineToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.PomocOnlineToolStripMenuItem.Text = "Podręcznik użytkownika (online)"
        Me.PomocOnlineToolStripMenuItem.ToolTipText = "Otwiera pełny podręcznik użytkownika i pomoc programu w przeglądarce internetowej"
        '
        'GaleriaWarstwToolStripMenuItem
        '
        Me.GaleriaWarstwToolStripMenuItem.Name = "GaleriaWarstwToolStripMenuItem"
        Me.GaleriaWarstwToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.GaleriaWarstwToolStripMenuItem.Text = "Galeria warstw map (podgląd online)"
        Me.GaleriaWarstwToolStripMenuItem.ToolTipText = "Otwiera galerię podglądów warstw WMS/WMTS/XYZ w przeglądarce z możliwością dodania wybranej warstwy do programu"
        '
        'InstrukcjaObsługiToolStripMenuItem
        '
        Me.InstrukcjaObsługiToolStripMenuItem.Name = "InstrukcjaObsługiToolStripMenuItem"
        Me.InstrukcjaObsługiToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.InstrukcjaObsługiToolStripMenuItem.Text = "Krótka instrukcja (bez internetu)"
        Me.InstrukcjaObsługiToolStripMenuItem.ToolTipText = "Otwiera wbudowaną skróconą instrukcję obsługi w oknie programu"
        '
        'PomocPomorskieForumEksploracyjneToolStripMenuItem
        '
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Name = "PomocPomorskieForumEksploracyjneToolStripMenuItem"
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.Text = "Zadaj pytanie na forum (PFE)"
        Me.PomocPomorskieForumEksploracyjneToolStripMenuItem.ToolTipText = "Otwiera dział MapoTero na Pomorskim Forum Eksploracyjnym w przeglądarce internetowej"
        '
        'HelpSeparator1
        '
        Me.HelpSeparator1.Name = "HelpSeparator1"
        Me.HelpSeparator1.Size = New System.Drawing.Size(332, 6)
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.AboutToolStripMenuItem.Text = "O programie"
        '
        'ZglosBladToolStripMenuItem
        '
        Me.ZglosBladToolStripMenuItem.Name = "ZglosBladToolStripMenuItem"
        Me.ZglosBladToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.ZglosBladToolStripMenuItem.Text = "Zgłoś błąd lub pomysł (GitHub)"
        Me.ZglosBladToolStripMenuItem.ToolTipText = "Otwiera stronę zgłoszeń programu na GitHubie w przeglądarce internetowej"
        '
        'FolderDanychToolStripMenuItem
        '
        Me.FolderDanychToolStripMenuItem.Name = "FolderDanychToolStripMenuItem"
        Me.FolderDanychToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.FolderDanychToolStripMenuItem.Text = "Otwórz folder danych programu"
        Me.FolderDanychToolStripMenuItem.ToolTipText = "Otwiera folder z ustawieniami (lastsettings.txt), pobranymi mapami (download) i plikiem awarie.log"
        '
        'HelpSeparator2
        '
        Me.HelpSeparator2.Name = "HelpSeparator2"
        Me.HelpSeparator2.Size = New System.Drawing.Size(332, 6)
        '
        'CoNowegoToolStripMenuItem
        '
        Me.CoNowegoToolStripMenuItem.Name = "CoNowegoToolStripMenuItem"
        Me.CoNowegoToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.CoNowegoToolStripMenuItem.Text = "Co nowego w programie"
        Me.CoNowegoToolStripMenuItem.ToolTipText = "Otwiera listę wydań programu z opisem zmian w przeglądarce internetowej"
        '
        'WesprzyjToolStripMenuItem
        '
        Me.WesprzyjToolStripMenuItem.Name = "WesprzyjToolStripMenuItem"
        Me.WesprzyjToolStripMenuItem.Size = New System.Drawing.Size(335, 22)
        Me.WesprzyjToolStripMenuItem.Text = "Wesprzyj rozwój programu"
        Me.WesprzyjToolStripMenuItem.ToolTipText = "Postaw kawę autorowi programu (buycoffee.to)"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SesjaToolStripMenuItem, Me.NarzedziaToolStripMenuItem, Me.UstawieniaToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(884, 24)
        Me.MenuStrip1.TabIndex = 230
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'Label47
        '
        Me.Label47.AutoSize = True
        Me.Label47.Enabled = False
        Me.Label47.Location = New System.Drawing.Point(276, 94)
        Me.Label47.Name = "Label47"
        Me.Label47.Size = New System.Drawing.Size(13, 13)
        Me.Label47.TabIndex = 221
        Me.Label47.Text = "="
        Me.ToolTip1.SetToolTip(Me.Label47, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" &
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'Label46
        '
        Me.Label46.AutoSize = True
        Me.Label46.Enabled = False
        Me.Label46.Location = New System.Drawing.Point(277, 57)
        Me.Label46.Name = "Label46"
        Me.Label46.Size = New System.Drawing.Size(12, 13)
        Me.Label46.TabIndex = 220
        Me.Label46.Text = "x"
        Me.ToolTip1.SetToolTip(Me.Label46, "Domyślna wartość zapewnia optymalną jakość pobranego obrazu. Im większy rozmiar p" &
        "ojedynczego piksela, tym gorsza jakość obrazu.")
        '
        'txtZasiegSegmentuKm
        '
        Me.txtZasiegSegmentuKm.Enabled = False
        Me.txtZasiegSegmentuKm.Location = New System.Drawing.Point(246, 108)
        Me.txtZasiegSegmentuKm.Name = "txtZasiegSegmentuKm"
        Me.txtZasiegSegmentuKm.Size = New System.Drawing.Size(72, 20)
        Me.txtZasiegSegmentuKm.TabIndex = 219
        Me.txtZasiegSegmentuKm.Text = "2000"
        Me.txtZasiegSegmentuKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtBokSegmentu
        '
        Me.txtBokSegmentu.Location = New System.Drawing.Point(60, 44)
        Me.txtBokSegmentu.Name = "txtBokSegmentu"
        Me.txtBokSegmentu.Size = New System.Drawing.Size(38, 20)
        Me.txtBokSegmentu.TabIndex = 211
        Me.txtBokSegmentu.Text = "2000"
        Me.txtBokSegmentu.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(321, 75)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(73, 13)
        Me.Label1.TabIndex = 213
        Me.Label1.Text = "Segment [pix]"
        Me.ToolTip1.SetToolTip(Me.Label1, "Rozmiar pojedynczego segmentu - najmniejszej komórki siatki kwadratów, na które z" &
        "ostanie podzielony pobierany obszar mapy. Uwaga - maksymalny rozmiar segmentu d" &
        "la Geoportalu2 wynosi 2048px")
        '
        'lblRozmiarPikselaOpis
        '
        Me.lblRozmiarPikselaOpis.AutoSize = True
        Me.lblRozmiarPikselaOpis.Location = New System.Drawing.Point(321, 39)
        Me.lblRozmiarPikselaOpis.Name = "lblRozmiarPikselaOpis"
        Me.lblRozmiarPikselaOpis.Size = New System.Drawing.Size(75, 13)
        Me.lblRozmiarPikselaOpis.TabIndex = 214
        Me.lblRozmiarPikselaOpis.Text = "Piksel [m/pix]"
        Me.ToolTip1.SetToolTip(Me.lblRozmiarPikselaOpis, "Domyślna wartość jest optymalna. Im większy rozmiar piksela, tym gorsza jakość ob" &
        "razu, ale jednocześnie tym większy jego przestrzenny zasięg.")
        '
        'txtRozmiarPiksela
        '
        Me.txtRozmiarPiksela.Location = New System.Drawing.Point(44, 17)
        Me.txtRozmiarPiksela.Name = "txtRozmiarPiksela"
        Me.txtRozmiarPiksela.Size = New System.Drawing.Size(36, 20)
        Me.txtRozmiarPiksela.TabIndex = 212
        Me.txtRozmiarPiksela.Text = "1"
        Me.txtRozmiarPiksela.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'ToolTip1
        '
        Me.ToolTip1.AutoPopDelay = 15000
        Me.ToolTip1.InitialDelay = 500
        Me.ToolTip1.IsBalloon = True
        Me.ToolTip1.ReshowDelay = 500
        '
        'grpWybraneWarstwy
        '
        Me.grpWybraneWarstwy.Controls.Add(Me.lstWybraneWarstwy)
        Me.grpWybraneWarstwy.Controls.Add(Me.btnWarstwaWyzej)
        Me.grpWybraneWarstwy.Controls.Add(Me.btnWarstwaNizej)
        Me.grpWybraneWarstwy.Controls.Add(Me.btnUsunWarstwe)
        Me.grpWybraneWarstwy.Controls.Add(Me.lblKrycie)
        Me.grpWybraneWarstwy.Controls.Add(Me.numKrycie)
        Me.grpWybraneWarstwy.Controls.Add(Me.btnWarstwyZaawansowane)
        Me.grpWybraneWarstwy.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpWybraneWarstwy.Location = New System.Drawing.Point(5, 322)
        Me.grpWybraneWarstwy.Name = "grpWybraneWarstwy"
        Me.grpWybraneWarstwy.Size = New System.Drawing.Size(210, 240)
        Me.grpWybraneWarstwy.TabIndex = 268
        Me.grpWybraneWarstwy.TabStop = False
        Me.grpWybraneWarstwy.Text = "Wybrane warstwy oraz ich kolejność"
        Me.ToolTip1.SetToolTip(Me.grpWybraneWarstwy, """Kanapka"" warstw. Pierwsza warstwa to bazowy podkład, który można przykryć kolejn" &
        "ymi warstwami o wyższym numerze, o ile mają one przeźroczyste tło, lub mają niep" &
        "ełne pokrycie.")
        '
        'lstWybraneWarstwy
        '
        Me.lstWybraneWarstwy.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstWybraneWarstwy.Location = New System.Drawing.Point(4, 15)
        Me.lstWybraneWarstwy.Name = "lstWybraneWarstwy"
        Me.lstWybraneWarstwy.Size = New System.Drawing.Size(202, 194)
        Me.lstWybraneWarstwy.TabIndex = 300
        '
        'btnWarstwaWyzej
        '
        Me.btnWarstwaWyzej.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnWarstwaWyzej.Font = New System.Drawing.Font("Segoe UI Symbol", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnWarstwaWyzej.Location = New System.Drawing.Point(4, 212)
        Me.btnWarstwaWyzej.Margin = New System.Windows.Forms.Padding(0)
        Me.btnWarstwaWyzej.Name = "btnWarstwaWyzej"
        Me.btnWarstwaWyzej.Size = New System.Drawing.Size(20, 23)
        Me.btnWarstwaWyzej.TabIndex = 301
        Me.btnWarstwaWyzej.Text = "▲"
        Me.ToolTip1.SetToolTip(Me.btnWarstwaWyzej, "Przesuń zaznaczoną warstwę wyżej w kanapce (Ctrl+↑)")
        Me.btnWarstwaWyzej.UseVisualStyleBackColor = True
        '
        'btnWarstwaNizej
        '
        Me.btnWarstwaNizej.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnWarstwaNizej.Font = New System.Drawing.Font("Segoe UI Symbol", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnWarstwaNizej.Location = New System.Drawing.Point(25, 212)
        Me.btnWarstwaNizej.Margin = New System.Windows.Forms.Padding(0)
        Me.btnWarstwaNizej.Name = "btnWarstwaNizej"
        Me.btnWarstwaNizej.Size = New System.Drawing.Size(20, 23)
        Me.btnWarstwaNizej.TabIndex = 302
        Me.btnWarstwaNizej.Text = "▼"
        Me.ToolTip1.SetToolTip(Me.btnWarstwaNizej, "Przesuń zaznaczoną warstwę niżej w kanapce (Ctrl+↓)")
        Me.btnWarstwaNizej.UseVisualStyleBackColor = True
        '
        'btnUsunWarstwe
        '
        Me.btnUsunWarstwe.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnUsunWarstwe.Font = New System.Drawing.Font("Segoe UI Symbol", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnUsunWarstwe.Location = New System.Drawing.Point(46, 212)
        Me.btnUsunWarstwe.Margin = New System.Windows.Forms.Padding(0)
        Me.btnUsunWarstwe.Name = "btnUsunWarstwe"
        Me.btnUsunWarstwe.Size = New System.Drawing.Size(20, 23)
        Me.btnUsunWarstwe.TabIndex = 303
        Me.btnUsunWarstwe.Text = "✕"
        Me.ToolTip1.SetToolTip(Me.btnUsunWarstwe, "Usuń zaznaczoną warstwę (Delete)")
        Me.btnUsunWarstwe.UseVisualStyleBackColor = True
        '
        'lblKrycie
        '
        Me.lblKrycie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblKrycie.AutoSize = True
        Me.lblKrycie.Location = New System.Drawing.Point(67, 217)
        Me.lblKrycie.Name = "lblKrycie"
        Me.lblKrycie.Size = New System.Drawing.Size(42, 13)
        Me.lblKrycie.TabIndex = 305
        Me.lblKrycie.Text = "Krycie %"
        Me.ToolTip1.SetToolTip(Me.lblKrycie, "Krycie zaznaczonej warstwy: 100% - warstwa nieprzezroczysta, mniej - prześwituje przez nią niższa warstwa.")
        '
        'numKrycie
        '
        Me.numKrycie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.numKrycie.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        Me.numKrycie.Location = New System.Drawing.Point(111, 214)
        Me.numKrycie.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.numKrycie.Name = "numKrycie"
        Me.numKrycie.Size = New System.Drawing.Size(50, 20)
        Me.numKrycie.TabIndex = 306
        Me.numKrycie.Value = New Decimal(New Integer() {100, 0, 0, 0})
        Me.ToolTip1.SetToolTip(Me.numKrycie, "Krycie zaznaczonej warstwy: 100% - warstwa nieprzezroczysta, mniej - prześwituje przez nią niższa warstwa.")
        '
        'btnWarstwyZaawansowane
        '
        Me.btnWarstwyZaawansowane.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnWarstwyZaawansowane.Location = New System.Drawing.Point(184, 212)
        Me.btnWarstwyZaawansowane.Margin = New System.Windows.Forms.Padding(0)
        Me.btnWarstwyZaawansowane.Name = "btnWarstwyZaawansowane"
        Me.btnWarstwyZaawansowane.Size = New System.Drawing.Size(22, 23)
        Me.btnWarstwyZaawansowane.TabIndex = 307
        Me.btnWarstwyZaawansowane.Text = "…"
        Me.ToolTip1.SetToolTip(Me.btnWarstwyZaawansowane, "Warstwy - zaawansowane: suwaki krycia i podgląd kanapki")
        Me.btnWarstwyZaawansowane.UseVisualStyleBackColor = True
        '
        'btnPowieksz
        '
        Me.btnPowieksz.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPowieksz.Location = New System.Drawing.Point(6, 28)
        Me.btnPowieksz.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPowieksz.Name = "btnPowieksz"
        Me.btnPowieksz.Size = New System.Drawing.Size(23, 31)
        Me.btnPowieksz.TabIndex = 295
        Me.btnPowieksz.Text = "+"
        Me.ToolTip1.SetToolTip(Me.btnPowieksz, "Powiększ")
        Me.btnPowieksz.UseVisualStyleBackColor = True
        '
        'btnPomniejsz
        '
        Me.btnPomniejsz.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPomniejsz.Location = New System.Drawing.Point(6, 74)
        Me.btnPomniejsz.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPomniejsz.Name = "btnPomniejsz"
        Me.btnPomniejsz.Size = New System.Drawing.Size(23, 30)
        Me.btnPomniejsz.TabIndex = 296
        Me.btnPomniejsz.Text = "-"
        Me.ToolTip1.SetToolTip(Me.btnPomniejsz, "Pomniejsz")
        Me.btnPomniejsz.UseVisualStyleBackColor = True
        '
        'btnSzukajDzialki
        '
        Me.btnSzukajDzialki.Font = New System.Drawing.Font("Segoe UI", 6.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnSzukajDzialki.Location = New System.Drawing.Point(6, 116)
        Me.btnSzukajDzialki.Margin = New System.Windows.Forms.Padding(0)
        Me.btnSzukajDzialki.Name = "btnSzukajDzialki"
        Me.btnSzukajDzialki.Padding = New System.Windows.Forms.Padding(0)
        Me.btnSzukajDzialki.Size = New System.Drawing.Size(23, 26)
        Me.btnSzukajDzialki.TabIndex = 297
        Me.btnSzukajDzialki.Text = ""
        Me.ToolTip1.SetToolTip(Me.btnSzukajDzialki, "Wyszukaj miejscowość, adres lub działkę katastralną (Ctrl+F, Ctrl+D)")
        Me.btnSzukajDzialki.UseCompatibleTextRendering = True
        Me.btnSzukajDzialki.UseVisualStyleBackColor = True
        '
        'pnlUkrytePolaZasiegu
        '
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label22)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label19)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label18)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtLiczbaWierszy)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label21)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtLiczbaKolumn)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label20)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtWysokoscPx)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtSzerokoscPx)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label9)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label17)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label10)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtSzerokoscKm)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtWysokoscKm)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label42)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label7)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label8)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label47)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblZaznDlugLewa)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtZasiegSegmentuKm)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label46)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblZaznSzerDol)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblZaznDlugPrawa)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblZaznSzerGora)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblRozmiarPikselaOpis)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.Label1)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblOpisYPrawy)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblOpisXGora)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtYPrawy)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtXGora)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblOpisYLewy)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.lblOpisXDol)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtYLewy)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.txtXDol)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.chkEdycjaXY)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.PictureBox2)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.grpRozmiarSiatki)
        Me.pnlUkrytePolaZasiegu.Controls.Add(Me.grpSchemat)
        Me.pnlUkrytePolaZasiegu.Location = New System.Drawing.Point(0, 0)
        Me.pnlUkrytePolaZasiegu.Name = "pnlUkrytePolaZasiegu"
        Me.pnlUkrytePolaZasiegu.Size = New System.Drawing.Size(1, 1)
        Me.pnlUkrytePolaZasiegu.TabIndex = 99
        Me.pnlUkrytePolaZasiegu.Visible = False
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(0, 0)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(12, 13)
        Me.Label22.TabIndex = 242
        Me.Label22.Text = "x"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(0, 0)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(12, 13)
        Me.Label19.TabIndex = 241
        Me.Label19.Text = "x"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(0, 0)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(12, 13)
        Me.Label18.TabIndex = 240
        Me.Label18.Text = "x"
        '
        'txtLiczbaWierszy
        '
        Me.txtLiczbaWierszy.Location = New System.Drawing.Point(0, 0)
        Me.txtLiczbaWierszy.Name = "txtLiczbaWierszy"
        Me.txtLiczbaWierszy.Size = New System.Drawing.Size(55, 20)
        Me.txtLiczbaWierszy.TabIndex = 239
        Me.txtLiczbaWierszy.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(0, 0)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(38, 13)
        Me.Label21.TabIndex = 238
        Me.Label21.Text = "[segm]"
        '
        'txtLiczbaKolumn
        '
        Me.txtLiczbaKolumn.Location = New System.Drawing.Point(0, 0)
        Me.txtLiczbaKolumn.Name = "txtLiczbaKolumn"
        Me.txtLiczbaKolumn.Size = New System.Drawing.Size(55, 20)
        Me.txtLiczbaKolumn.TabIndex = 237
        Me.txtLiczbaKolumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(0, 0)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(26, 13)
        Me.Label20.TabIndex = 235
        Me.Label20.Text = "[pix]"
        '
        'txtWysokoscPx
        '
        Me.txtWysokoscPx.Location = New System.Drawing.Point(0, 0)
        Me.txtWysokoscPx.Name = "txtWysokoscPx"
        Me.txtWysokoscPx.Size = New System.Drawing.Size(55, 20)
        Me.txtWysokoscPx.TabIndex = 234
        Me.txtWysokoscPx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtSzerokoscPx
        '
        Me.txtSzerokoscPx.Location = New System.Drawing.Point(0, 0)
        Me.txtSzerokoscPx.Name = "txtSzerokoscPx"
        Me.txtSzerokoscPx.Size = New System.Drawing.Size(55, 20)
        Me.txtSzerokoscPx.TabIndex = 233
        Me.txtSzerokoscPx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(0, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(84, 13)
        Me.Label9.TabIndex = 214
        Me.Label9.Text = "Szerokość siatki"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(0, 0)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(27, 13)
        Me.Label17.TabIndex = 231
        Me.Label17.Text = "[km]"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(0, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(84, 13)
        Me.Label10.TabIndex = 215
        Me.Label10.Text = "Wysokość siatki"
        '
        'txtSzerokoscKm
        '
        Me.txtSzerokoscKm.Location = New System.Drawing.Point(0, 0)
        Me.txtSzerokoscKm.Name = "txtSzerokoscKm"
        Me.txtSzerokoscKm.Size = New System.Drawing.Size(55, 20)
        Me.txtSzerokoscKm.TabIndex = 216
        Me.txtSzerokoscKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtWysokoscKm
        '
        Me.txtWysokoscKm.Location = New System.Drawing.Point(0, 0)
        Me.txtWysokoscKm.Name = "txtWysokoscKm"
        Me.txtWysokoscKm.Size = New System.Drawing.Size(55, 20)
        Me.txtWysokoscKm.TabIndex = 217
        Me.txtWysokoscKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label42
        '
        Me.Label42.AutoSize = True
        Me.Label42.Location = New System.Drawing.Point(0, 0)
        Me.Label42.Name = "Label42"
        Me.Label42.Size = New System.Drawing.Size(87, 13)
        Me.Label42.TabIndex = 215
        Me.Label42.Text = "Zasięg w terenie"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(0, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(77, 13)
        Me.Label7.TabIndex = 316
        Me.Label7.Text = "segmentu [km]"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(0, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(166, 13)
        Me.Label8.TabIndex = 317
        Me.Label8.Text = "Rozmiar pojedynczego segmentu:"
        '
        'lblZaznDlugLewa
        '
        Me.lblZaznDlugLewa.AutoSize = True
        Me.lblZaznDlugLewa.Location = New System.Drawing.Point(0, 0)
        Me.lblZaznDlugLewa.Name = "lblZaznDlugLewa"
        Me.lblZaznDlugLewa.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznDlugLewa.TabIndex = 315
        Me.lblZaznDlugLewa.Text = "."
        '
        'lblZaznSzerDol
        '
        Me.lblZaznSzerDol.AutoSize = True
        Me.lblZaznSzerDol.Location = New System.Drawing.Point(0, 0)
        Me.lblZaznSzerDol.Name = "lblZaznSzerDol"
        Me.lblZaznSzerDol.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznSzerDol.TabIndex = 314
        Me.lblZaznSzerDol.Text = "."
        '
        'lblZaznDlugPrawa
        '
        Me.lblZaznDlugPrawa.AutoSize = True
        Me.lblZaznDlugPrawa.Location = New System.Drawing.Point(0, 0)
        Me.lblZaznDlugPrawa.Name = "lblZaznDlugPrawa"
        Me.lblZaznDlugPrawa.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznDlugPrawa.TabIndex = 313
        Me.lblZaznDlugPrawa.Text = "."
        '
        'lblZaznSzerGora
        '
        Me.lblZaznSzerGora.AutoSize = True
        Me.lblZaznSzerGora.Location = New System.Drawing.Point(0, 0)
        Me.lblZaznSzerGora.Name = "lblZaznSzerGora"
        Me.lblZaznSzerGora.Size = New System.Drawing.Size(10, 13)
        Me.lblZaznSzerGora.TabIndex = 312
        Me.lblZaznSzerGora.Text = "."
        '
        'lblOpisYPrawy
        '
        Me.lblOpisYPrawy.AutoSize = True
        Me.lblOpisYPrawy.Location = New System.Drawing.Point(0, 0)
        Me.lblOpisYPrawy.Name = "lblOpisYPrawy"
        Me.lblOpisYPrawy.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisYPrawy.TabIndex = 310
        Me.lblOpisYPrawy.Text = "y"
        '
        'lblOpisXGora
        '
        Me.lblOpisXGora.AutoSize = True
        Me.lblOpisXGora.Location = New System.Drawing.Point(0, 0)
        Me.lblOpisXGora.Name = "lblOpisXGora"
        Me.lblOpisXGora.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisXGora.TabIndex = 311
        Me.lblOpisXGora.Text = "x"
        '
        'txtYPrawy
        '
        Me.txtYPrawy.Location = New System.Drawing.Point(0, 0)
        Me.txtYPrawy.Name = "txtYPrawy"
        Me.txtYPrawy.Size = New System.Drawing.Size(62, 20)
        Me.txtYPrawy.TabIndex = 309
        Me.txtYPrawy.Text = "571000"
        '
        'txtXGora
        '
        Me.txtXGora.Location = New System.Drawing.Point(0, 0)
        Me.txtXGora.Name = "txtXGora"
        Me.txtXGora.Size = New System.Drawing.Size(62, 20)
        Me.txtXGora.TabIndex = 308
        Me.txtXGora.Text = "684000"
        '
        'lblOpisYLewy
        '
        Me.lblOpisYLewy.AutoSize = True
        Me.lblOpisYLewy.Location = New System.Drawing.Point(0, 0)
        Me.lblOpisYLewy.Name = "lblOpisYLewy"
        Me.lblOpisYLewy.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisYLewy.TabIndex = 307
        Me.lblOpisYLewy.Text = "y"
        '
        'lblOpisXDol
        '
        Me.lblOpisXDol.AutoSize = True
        Me.lblOpisXDol.Location = New System.Drawing.Point(0, 0)
        Me.lblOpisXDol.Name = "lblOpisXDol"
        Me.lblOpisXDol.Size = New System.Drawing.Size(12, 13)
        Me.lblOpisXDol.TabIndex = 306
        Me.lblOpisXDol.Text = "x"
        '
        'txtYLewy
        '
        Me.txtYLewy.Location = New System.Drawing.Point(0, 0)
        Me.txtYLewy.Name = "txtYLewy"
        Me.txtYLewy.Size = New System.Drawing.Size(62, 20)
        Me.txtYLewy.TabIndex = 305
        Me.txtYLewy.Text = "569000"
        '
        'txtXDol
        '
        Me.txtXDol.Location = New System.Drawing.Point(0, 0)
        Me.txtXDol.Name = "txtXDol"
        Me.txtXDol.Size = New System.Drawing.Size(62, 20)
        Me.txtXDol.TabIndex = 304
        Me.txtXDol.Text = "682000"
        '
        'chkEdycjaXY
        '
        Me.chkEdycjaXY.Location = New System.Drawing.Point(0, 0)
        Me.chkEdycjaXY.Name = "chkEdycjaXY"
        Me.chkEdycjaXY.Size = New System.Drawing.Size(14, 14)
        Me.chkEdycjaXY.TabIndex = 307
        Me.chkEdycjaXY.AccessibleName = "Wpisywanie zasięgu XY z klawiatury"
        '
        'PictureBox2
        '
        Me.PictureBox2.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(10, 10)
        Me.PictureBox2.TabIndex = 303
        Me.PictureBox2.TabStop = False
        '
        'grpRozmiarSiatki
        '
        Me.grpRozmiarSiatki.Location = New System.Drawing.Point(0, 0)
        Me.grpRozmiarSiatki.Name = "grpRozmiarSiatki"
        Me.grpRozmiarSiatki.Size = New System.Drawing.Size(1, 1)
        Me.grpRozmiarSiatki.TabIndex = 300
        Me.grpRozmiarSiatki.TabStop = False
        '
        'grpSchemat
        '
        Me.grpSchemat.Location = New System.Drawing.Point(0, 0)
        Me.grpSchemat.Name = "grpSchemat"
        Me.grpSchemat.Size = New System.Drawing.Size(1, 1)
        Me.grpSchemat.TabIndex = 303
        Me.grpSchemat.TabStop = False
        '
        'pnlBelkaAkcji
        '
        Me.pnlBelkaAkcji.Controls.Add(Me.btnKreatorDolny)
        Me.pnlBelkaAkcji.Controls.Add(Me.lblStatusSesji)
        Me.pnlBelkaAkcji.Controls.Add(Me.lnkSzczegoly)
        Me.pnlBelkaAkcji.Controls.Add(Me.btnOtworzFolderDolny)
        Me.pnlBelkaAkcji.Controls.Add(Me.btnPrzejdzDoEksportu)
        Me.pnlBelkaAkcji.Controls.Add(Me.lblLiniaBelki)
        Me.pnlBelkaAkcji.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlBelkaAkcji.Location = New System.Drawing.Point(0, 0)
        Me.pnlBelkaAkcji.Name = "pnlBelkaAkcji"
        Me.pnlBelkaAkcji.Size = New System.Drawing.Size(857, 34)
        Me.pnlBelkaAkcji.TabIndex = 0
        '
        'btnKreatorDolny
        '
        Me.btnKreatorDolny.BackColor = System.Drawing.Color.FromArgb(CType(CType(37, Byte), Integer), CType(CType(99, Byte), Integer), CType(CType(235, Byte), Integer))
        Me.btnKreatorDolny.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnKreatorDolny.FlatAppearance.BorderSize = 0
        Me.btnKreatorDolny.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnKreatorDolny.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnKreatorDolny.ForeColor = System.Drawing.Color.White
        Me.btnKreatorDolny.Location = New System.Drawing.Point(4, 4)
        Me.btnKreatorDolny.Name = "btnKreatorDolny"
        Me.btnKreatorDolny.Size = New System.Drawing.Size(180, 26)
        Me.btnKreatorDolny.TabIndex = 0
        Me.btnKreatorDolny.Text = "🪄 Szybki kreator... (Ctrl+K)"
        Me.ToolTip1.SetToolTip(Me.btnKreatorDolny, "Uruchom szybki kreator mapy krok po kroku (styl, obszar, skale i pobieranie w jednym oknie)")
        Me.btnKreatorDolny.UseVisualStyleBackColor = False
        '
        'lblStatusSesji
        '
        Me.lblStatusSesji.AutoEllipsis = True
        Me.lblStatusSesji.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblStatusSesji.ForeColor = System.Drawing.Color.FromArgb(CType(CType(71, Byte), Integer), CType(CType(85, Byte), Integer), CType(CType(105, Byte), Integer))
        Me.lblStatusSesji.Location = New System.Drawing.Point(192, 4)
        Me.lblStatusSesji.Name = "lblStatusSesji"
        Me.lblStatusSesji.Size = New System.Drawing.Size(395, 26)
        Me.lblStatusSesji.TabIndex = 2
        Me.lblStatusSesji.Text = ""
        Me.lblStatusSesji.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lnkSzczegoly
        '
        Me.lnkSzczegoly.AutoSize = True
        Me.lnkSzczegoly.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lnkSzczegoly.Location = New System.Drawing.Point(568, 10)
        Me.lnkSzczegoly.Name = "lnkSzczegoly"
        Me.lnkSzczegoly.Size = New System.Drawing.Size(60, 13)
        Me.lnkSzczegoly.TabIndex = 3
        Me.lnkSzczegoly.TabStop = True
        Me.lnkSzczegoly.Text = "[Szczegóły]"
        Me.lnkSzczegoly.Visible = False
        '
        'btnOtworzFolderDolny
        '
        Me.btnOtworzFolderDolny.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOtworzFolderDolny.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnOtworzFolderDolny.Location = New System.Drawing.Point(595, 4)
        Me.btnOtworzFolderDolny.Name = "btnOtworzFolderDolny"
        Me.btnOtworzFolderDolny.Size = New System.Drawing.Size(95, 26)
        Me.btnOtworzFolderDolny.TabIndex = 4
        Me.btnOtworzFolderDolny.Text = "📁 Folder sesji"
        Me.ToolTip1.SetToolTip(Me.btnOtworzFolderDolny, "Otwórz folder bieżącej sesji w Eksploratorze Windows")
        Me.btnOtworzFolderDolny.UseVisualStyleBackColor = True
        '
        'btnPrzejdzDoEksportu
        '
        Me.btnPrzejdzDoEksportu.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPrzejdzDoEksportu.BackColor = System.Drawing.Color.FromArgb(CType(CType(71, Byte), Integer), CType(CType(85, Byte), Integer), CType(CType(105, Byte), Integer))
        Me.btnPrzejdzDoEksportu.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnPrzejdzDoEksportu.FlatAppearance.BorderSize = 0
        Me.btnPrzejdzDoEksportu.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPrzejdzDoEksportu.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPrzejdzDoEksportu.ForeColor = System.Drawing.Color.White
        Me.btnPrzejdzDoEksportu.Location = New System.Drawing.Point(695, 4)
        Me.btnPrzejdzDoEksportu.Name = "btnPrzejdzDoEksportu"
        Me.btnPrzejdzDoEksportu.Size = New System.Drawing.Size(158, 26)
        Me.btnPrzejdzDoEksportu.TabIndex = 5
        Me.btnPrzejdzDoEksportu.Text = "Eksportuj mapę…"
        Me.ToolTip1.SetToolTip(Me.btnPrzejdzDoEksportu, "Otwórz okno eksportu pobranej mapy dla bieżącego formatu")
        Me.btnPrzejdzDoEksportu.UseVisualStyleBackColor = False
        '
        'lblLiniaBelki
        '
        Me.lblLiniaBelki.BackColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(232, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.lblLiniaBelki.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lblLiniaBelki.Location = New System.Drawing.Point(0, 33)
        Me.lblLiniaBelki.Name = "lblLiniaBelki"
        Me.lblLiniaBelki.Size = New System.Drawing.Size(857, 1)
        Me.lblLiniaBelki.TabIndex = 3
        '
        'pnlSzacunek
        '
        Me.pnlSzacunek.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(252, Byte), Integer))
        Me.pnlSzacunek.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlSzacunek.Controls.Add(Me.lblTytulRozmiar)
        Me.pnlSzacunek.Controls.Add(Me.lblSzacowanyRozmiar)
        Me.pnlSzacunek.Controls.Add(Me.lblSeparatorSzacunku)
        Me.pnlSzacunek.Controls.Add(Me.lblTytulCzas)
        Me.pnlSzacunek.Controls.Add(Me.lblSzacowanyCzas)
        Me.pnlSzacunek.Location = New System.Drawing.Point(0, 50)
        Me.pnlSzacunek.Name = "pnlSzacunek"
        Me.pnlSzacunek.Size = New System.Drawing.Size(276, 24)
        Me.pnlSzacunek.TabIndex = 4
        '
        'lblTytulRozmiar
        '
        Me.lblTytulRozmiar.AutoSize = True
        Me.lblTytulRozmiar.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTytulRozmiar.ForeColor = System.Drawing.Color.DimGray
        Me.lblTytulRozmiar.Location = New System.Drawing.Point(6, 5)
        Me.lblTytulRozmiar.Name = "lblTytulRozmiar"
        Me.lblTytulRozmiar.Size = New System.Drawing.Size(53, 12)
        Me.lblTytulRozmiar.TabIndex = 0
        Me.lblTytulRozmiar.Text = "Szacunek:"
        '
        'lblSzacowanyRozmiar
        '
        Me.lblSzacowanyRozmiar.AutoSize = True
        Me.lblSzacowanyRozmiar.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblSzacowanyRozmiar.ForeColor = System.Drawing.Color.FromArgb(CType(CType(15, Byte), Integer), CType(CType(80, Byte), Integer), CType(CType(40, Byte), Integer))
        Me.lblSzacowanyRozmiar.Location = New System.Drawing.Point(62, 4)
        Me.lblSzacowanyRozmiar.Name = "lblSzacowanyRozmiar"
        Me.lblSzacowanyRozmiar.Size = New System.Drawing.Size(76, 15)
        Me.lblSzacowanyRozmiar.TabIndex = 1
        Me.lblSzacowanyRozmiar.Text = "~ 15–22 MB"
        '
        'lblSeparatorSzacunku
        '
        Me.lblSeparatorSzacunku.AutoSize = True
        Me.lblSeparatorSzacunku.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblSeparatorSzacunku.ForeColor = System.Drawing.Color.FromArgb(CType(CType(180, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(205, Byte), Integer))
        Me.lblSeparatorSzacunku.Location = New System.Drawing.Point(142, 3)
        Me.lblSeparatorSzacunku.Name = "lblSeparatorSzacunku"
        Me.lblSeparatorSzacunku.Size = New System.Drawing.Size(12, 15)
        Me.lblSeparatorSzacunku.TabIndex = 2
        Me.lblSeparatorSzacunku.Text = "•"
        '
        'lblTytulCzas
        '
        Me.lblTytulCzas.Location = New System.Drawing.Point(0, 0)
        Me.lblTytulCzas.Name = "lblTytulCzas"
        Me.lblTytulCzas.Size = New System.Drawing.Size(1, 1)
        Me.lblTytulCzas.TabIndex = 3
        Me.lblTytulCzas.Visible = False
        '
        'lblSzacowanyCzas
        '
        Me.lblSzacowanyCzas.AutoSize = True
        Me.lblSzacowanyCzas.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblSzacowanyCzas.ForeColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(90, Byte), Integer))
        Me.lblSzacowanyCzas.Location = New System.Drawing.Point(158, 4)
        Me.lblSzacowanyCzas.Name = "lblSzacowanyCzas"
        Me.lblSzacowanyCzas.Size = New System.Drawing.Size(49, 15)
        Me.lblSzacowanyCzas.TabIndex = 4
        Me.lblSzacowanyCzas.Text = "ok. 18 s"
        '
        'grpObszar
        '
        Me.grpObszar.Controls.Add(Me.pnlUkrytePolaZasiegu)
        Me.grpObszar.Controls.Add(Me.lblOpisWymiary)
        Me.grpObszar.Controls.Add(Me.lblBadgeSegmenty)
        Me.grpObszar.Controls.Add(Me.lblTytulObszar)
        Me.grpObszar.Controls.Add(Me.lblWymiaryTerenu)
        Me.grpObszar.Controls.Add(Me.lblPowierzchniaTerenu)
        Me.grpObszar.Controls.Add(Me.pnlSzacunek)
        Me.grpObszar.Controls.Add(Me.lblUkladZasiegSkrot)
        Me.grpObszar.Controls.Add(Me.btnWpiszWspolrzedne)
        Me.grpObszar.Location = New System.Drawing.Point(4, 38)
        Me.grpObszar.Name = "grpObszar"
        Me.grpObszar.Size = New System.Drawing.Size(276, 104)
        Me.grpObszar.TabIndex = 300
        '
        'lblTytulObszar
        '
        Me.lblTytulObszar.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTytulObszar.ForeColor = System.Drawing.Color.DimGray
        Me.lblTytulObszar.Location = New System.Drawing.Point(0, 0)
        Me.lblTytulObszar.Name = "lblTytulObszar"
        Me.lblTytulObszar.Size = New System.Drawing.Size(276, 14)
        Me.lblTytulObszar.TabIndex = 0
        Me.lblTytulObszar.Text = "ZAZNACZONY OBSZAR"
        '
        'lblBadgeSegmenty
        '
        Me.lblBadgeSegmenty.AutoEllipsis = True
        Me.lblBadgeSegmenty.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblBadgeSegmenty.ForeColor = System.Drawing.Color.FromArgb(CType(CType(15, Byte), Integer), CType(CType(23, Byte), Integer), CType(CType(42, Byte), Integer))
        Me.lblBadgeSegmenty.Location = New System.Drawing.Point(0, 15)
        Me.lblBadgeSegmenty.Name = "lblBadgeSegmenty"
        Me.lblBadgeSegmenty.Size = New System.Drawing.Size(276, 18)
        Me.lblBadgeSegmenty.TabIndex = 8
        Me.lblBadgeSegmenty.Text = "Siatka: 6 segmentów (2 × 3)"
        Me.lblBadgeSegmenty.Visible = True
        '
        'lblWymiaryTerenu
        '
        Me.lblWymiaryTerenu.AutoSize = True
        Me.lblWymiaryTerenu.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblWymiaryTerenu.ForeColor = System.Drawing.Color.FromArgb(CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(85, Byte), Integer))
        Me.lblWymiaryTerenu.Location = New System.Drawing.Point(0, 33)
        Me.lblWymiaryTerenu.Name = "lblWymiaryTerenu"
        Me.lblWymiaryTerenu.Size = New System.Drawing.Size(73, 13)
        Me.lblWymiaryTerenu.TabIndex = 1
        Me.lblWymiaryTerenu.Text = "6.0 × 4.0 km"
        '
        'lblPowierzchniaTerenu
        '
        Me.lblPowierzchniaTerenu.AutoSize = True
        Me.lblPowierzchniaTerenu.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPowierzchniaTerenu.ForeColor = System.Drawing.Color.DimGray
        Me.lblPowierzchniaTerenu.Location = New System.Drawing.Point(76, 33)
        Me.lblPowierzchniaTerenu.Name = "lblPowierzchniaTerenu"
        Me.lblPowierzchniaTerenu.Size = New System.Drawing.Size(59, 13)
        Me.lblPowierzchniaTerenu.TabIndex = 2
        Me.lblPowierzchniaTerenu.Text = "(24.0 km²)"
        '
        'lblUkladZasiegSkrot
        '
        Me.lblUkladZasiegSkrot.AutoEllipsis = True
        Me.lblUkladZasiegSkrot.Font = New System.Drawing.Font("Segoe UI", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblUkladZasiegSkrot.ForeColor = System.Drawing.Color.DimGray
        Me.lblUkladZasiegSkrot.Location = New System.Drawing.Point(0, 78)
        Me.lblUkladZasiegSkrot.Name = "lblUkladZasiegSkrot"
        Me.lblUkladZasiegSkrot.Size = New System.Drawing.Size(185, 22)
        Me.lblUkladZasiegSkrot.TabIndex = 5
        Me.lblUkladZasiegSkrot.Text = "PL-1992 (N: 684k, W: 569k)"
        Me.lblUkladZasiegSkrot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnWpiszWspolrzedne
        '
        Me.btnWpiszWspolrzedne.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnWpiszWspolrzedne.Location = New System.Drawing.Point(190, 76)
        Me.btnWpiszWspolrzedne.Name = "btnWpiszWspolrzedne"
        Me.btnWpiszWspolrzedne.Size = New System.Drawing.Size(86, 24)
        Me.btnWpiszWspolrzedne.TabIndex = 6
        Me.btnWpiszWspolrzedne.Text = "✏️ Wpisz XY"
        Me.ToolTip1.SetToolTip(Me.btnWpiszWspolrzedne, "Otwórz okno ręcznego wpisywania współrzędnych obszaru lub zaokrąglania do pełnych kilometrów")
        Me.btnWpiszWspolrzedne.UseVisualStyleBackColor = True
        '
        'lblOpisWymiary
        '
        Me.lblOpisWymiary.Location = New System.Drawing.Point(0, 0)
        Me.lblOpisWymiary.Name = "lblOpisWymiary"
        Me.lblOpisWymiary.Size = New System.Drawing.Size(1, 1)
        Me.lblOpisWymiary.TabIndex = 7
        Me.lblOpisWymiary.Visible = False
        '
        'lblSeparator1
        '
        Me.lblSeparator1.BackColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(232, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.lblSeparator1.Location = New System.Drawing.Point(286, 42)
        Me.lblSeparator1.Name = "lblSeparator1"
        Me.lblSeparator1.Size = New System.Drawing.Size(1, 96)
        Me.lblSeparator1.TabIndex = 304
        '
        'grpRozdzielczoscFormat
        '
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblTytulParametry)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblOpisPiksela)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.txtRozmiarPiksela)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblJednostkaPiksela)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.cmbRozmiarPiksela)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblOpisSegmentu)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.txtBokSegmentu)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblJednostkaSegmentu)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblBokKm)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.lblOpisFormatu)
        Me.grpRozdzielczoscFormat.Controls.Add(Me.cmbFormatDocelowy)
        Me.grpRozdzielczoscFormat.Location = New System.Drawing.Point(294, 38)
        Me.grpRozdzielczoscFormat.Name = "grpRozdzielczoscFormat"
        Me.grpRozdzielczoscFormat.Size = New System.Drawing.Size(256, 104)
        Me.grpRozdzielczoscFormat.TabIndex = 301
        '
        'lblTytulParametry
        '
        Me.lblTytulParametry.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTytulParametry.ForeColor = System.Drawing.Color.DimGray
        Me.lblTytulParametry.Location = New System.Drawing.Point(0, 0)
        Me.lblTytulParametry.Name = "lblTytulParametry"
        Me.lblTytulParametry.Size = New System.Drawing.Size(256, 14)
        Me.lblTytulParametry.TabIndex = 0
        Me.lblTytulParametry.Text = "PARAMETRY POBRANIA"
        '
        'lblOpisPiksela
        '
        Me.lblOpisPiksela.AutoSize = True
        Me.lblOpisPiksela.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblOpisPiksela.Location = New System.Drawing.Point(0, 20)
        Me.lblOpisPiksela.Name = "lblOpisPiksela"
        Me.lblOpisPiksela.Size = New System.Drawing.Size(39, 13)
        Me.lblOpisPiksela.TabIndex = 1
        Me.lblOpisPiksela.Text = "Piksel:"
        '
        'lblJednostkaPiksela
        '
        Me.lblJednostkaPiksela.AutoSize = True
        Me.lblJednostkaPiksela.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblJednostkaPiksela.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblJednostkaPiksela.Location = New System.Drawing.Point(82, 21)
        Me.lblJednostkaPiksela.Name = "lblJednostkaPiksela"
        Me.lblJednostkaPiksela.Size = New System.Drawing.Size(32, 12)
        Me.lblJednostkaPiksela.TabIndex = 3
        Me.lblJednostkaPiksela.Text = "m/px"
        '
        'cmbRozmiarPiksela
        '
        Me.cmbRozmiarPiksela.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRozmiarPiksela.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.cmbRozmiarPiksela.FormattingEnabled = True
        Me.cmbRozmiarPiksela.Location = New System.Drawing.Point(118, 17)
        Me.cmbRozmiarPiksela.Name = "cmbRozmiarPiksela"
        Me.cmbRozmiarPiksela.Size = New System.Drawing.Size(134, 21)
        Me.cmbRozmiarPiksela.TabIndex = 4
        '
        'lblOpisSegmentu
        '
        Me.lblOpisSegmentu.AutoSize = True
        Me.lblOpisSegmentu.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblOpisSegmentu.Location = New System.Drawing.Point(0, 48)
        Me.lblOpisSegmentu.Name = "lblOpisSegmentu"
        Me.lblOpisSegmentu.Size = New System.Drawing.Size(55, 13)
        Me.lblOpisSegmentu.TabIndex = 5
        Me.lblOpisSegmentu.Text = "Segment:"
        '
        'lblJednostkaSegmentu
        '
        Me.lblJednostkaSegmentu.AutoSize = True
        Me.lblJednostkaSegmentu.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblJednostkaSegmentu.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblJednostkaSegmentu.Location = New System.Drawing.Point(100, 49)
        Me.lblJednostkaSegmentu.Name = "lblJednostkaSegmentu"
        Me.lblJednostkaSegmentu.Size = New System.Drawing.Size(16, 12)
        Me.lblJednostkaSegmentu.TabIndex = 7
        Me.lblJednostkaSegmentu.Text = "px"
        '
        'lblBokKm
        '
        Me.lblBokKm.AutoSize = True
        Me.lblBokKm.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblBokKm.ForeColor = System.Drawing.Color.DimGray
        Me.lblBokKm.Location = New System.Drawing.Point(120, 49)
        Me.lblBokKm.Name = "lblBokKm"
        Me.lblBokKm.Size = New System.Drawing.Size(95, 12)
        Me.lblBokKm.TabIndex = 8
        Me.lblBokKm.Text = "= 2.0 km w terenie"
        '
        'lblOpisFormatu
        '
        Me.lblOpisFormatu.AutoSize = True
        Me.lblOpisFormatu.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblOpisFormatu.Location = New System.Drawing.Point(0, 77)
        Me.lblOpisFormatu.Name = "lblOpisFormatu"
        Me.lblOpisFormatu.Size = New System.Drawing.Size(46, 13)
        Me.lblOpisFormatu.TabIndex = 9
        Me.lblOpisFormatu.Text = "Format:"
        '
        'cmbFormatDocelowy
        '
        Me.cmbFormatDocelowy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFormatDocelowy.Font = New System.Drawing.Font("Segoe UI", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.cmbFormatDocelowy.FormattingEnabled = True
        Me.cmbFormatDocelowy.Location = New System.Drawing.Point(54, 74)
        Me.cmbFormatDocelowy.Name = "cmbFormatDocelowy"
        Me.cmbFormatDocelowy.Size = New System.Drawing.Size(198, 21)
        Me.cmbFormatDocelowy.TabIndex = 10
        '
        'lblSeparator2
        '
        Me.lblSeparator2.BackColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(232, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.lblSeparator2.Location = New System.Drawing.Point(556, 42)
        Me.lblSeparator2.Name = "lblSeparator2"
        Me.lblSeparator2.Size = New System.Drawing.Size(1, 96)
        Me.lblSeparator2.TabIndex = 305
        '
        'btnUsunPobrane
        '
        Me.btnUsunPobrane.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnUsunPobrane.Image = Global.MapoTero.My.Resources.Resources.ico_folde_delete
        Me.btnUsunPobrane.Location = New System.Drawing.Point(39, 603)
        Me.btnUsunPobrane.Margin = New System.Windows.Forms.Padding(0)
        Me.btnUsunPobrane.Name = "btnUsunPobrane"
        Me.btnUsunPobrane.Size = New System.Drawing.Size(30, 30)
        Me.btnUsunPobrane.TabIndex = 306
        Me.ToolTip1.SetToolTip(Me.btnUsunPobrane, "Usuń pobrane mapy bieżącej sesji (do Kosza)")
        Me.btnUsunPobrane.UseVisualStyleBackColor = True
        '
        'btnZnaczniki
        '
        Me.btnZnaczniki.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnZnaczniki.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZnaczniki.Image = Global.MapoTero.My.Resources.Resources.marker_ico
        Me.btnZnaczniki.Location = New System.Drawing.Point(39, 569)
        Me.btnZnaczniki.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZnaczniki.Name = "btnZnaczniki"
        Me.btnZnaczniki.Size = New System.Drawing.Size(30, 30)
        Me.btnZnaczniki.TabIndex = 304
        Me.ToolTip1.SetToolTip(Me.btnZnaczniki, "Pokaż na mapie siatkę segmentów dla zasięgu z pól XY i wyśrodkuj na niej mapę")
        Me.btnZnaczniki.UseVisualStyleBackColor = True
        '
        'btnScalanie
        '
        Me.btnScalanie.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnScalanie.Image = Global.MapoTero.My.Resources.Resources.ico_merge
        Me.btnScalanie.Location = New System.Drawing.Point(73, 603)
        Me.btnScalanie.Margin = New System.Windows.Forms.Padding(0)
        Me.btnScalanie.Name = "btnScalanie"
        Me.btnScalanie.Size = New System.Drawing.Size(30, 30)
        Me.btnScalanie.TabIndex = 307
        Me.ToolTip1.SetToolTip(Me.btnScalanie, "Scalanie segmentów - arkusz GeoTIFF, JPEG albo PNG (okno Eksport mapy, format Arkusz)")
        Me.btnScalanie.UseVisualStyleBackColor = True
        '
        'btnOtworzFolder
        '
        Me.btnOtworzFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnOtworzFolder.Image = Global.MapoTero.My.Resources.Resources.ico_folder
        Me.btnOtworzFolder.Location = New System.Drawing.Point(5, 603)
        Me.btnOtworzFolder.Margin = New System.Windows.Forms.Padding(0)
        Me.btnOtworzFolder.Name = "btnOtworzFolder"
        Me.btnOtworzFolder.Size = New System.Drawing.Size(30, 30)
        Me.btnOtworzFolder.TabIndex = 305
        Me.ToolTip1.SetToolTip(Me.btnOtworzFolder, "Wyświetl folder z segmentami pobranej mapy")
        Me.btnOtworzFolder.UseVisualStyleBackColor = True
        '
        'mapa
        '
        Me.mapa.Bearing = 0!
        Me.mapa.CanDragMap = True
        Me.mapa.Cursor = System.Windows.Forms.Cursors.Hand
        Me.mapa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mapa.EmptyTileColor = System.Drawing.Color.Navy
        Me.mapa.GrayScaleMode = False
        Me.mapa.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow
        Me.mapa.LevelsKeepInMemory = 5
        Me.mapa.Location = New System.Drawing.Point(0, 24)
        Me.mapa.MarkersEnabled = True
        Me.mapa.MaxZoom = 17
        Me.mapa.MinZoom = 3
        Me.mapa.MouseWheelZoomEnabled = True
        Me.mapa.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter
        Me.mapa.Name = "mapa"
        Me.mapa.NegativeMode = False
        Me.mapa.PolygonsEnabled = True
        Me.mapa.RetryLoadTile = 0
        Me.mapa.RoutesEnabled = True
        Me.mapa.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional
        Me.mapa.SelectedAreaFillColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.mapa.ShowTileGridLines = False
        Me.mapa.Size = New System.Drawing.Size(661, 491)
        Me.mapa.TabIndex = 249
        Me.mapa.Zoom = 0R
        '
        'rbOsm
        '
        Me.rbOsm.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbOsm.AutoSize = True
        Me.rbOsm.Location = New System.Drawing.Point(544, 64)
        Me.rbOsm.Name = "rbOsm"
        Me.rbOsm.Size = New System.Drawing.Size(109, 17)
        Me.rbOsm.TabIndex = 258
        Me.rbOsm.Checked = True
        Me.rbOsm.Text = "OpenStreetMap   "
        Me.rbOsm.UseVisualStyleBackColor = True
        '
        'rbGoogle
        '
        Me.rbGoogle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbGoogle.AutoSize = True
        Me.rbGoogle.Location = New System.Drawing.Point(544, 26)
        Me.rbGoogle.Name = "rbGoogle"
        Me.rbGoogle.Size = New System.Drawing.Size(110, 17)
        Me.rbGoogle.TabIndex = 259
        Me.rbGoogle.TabStop = True
        Me.rbGoogle.Text = "GoogleMap          "
        Me.rbGoogle.UseVisualStyleBackColor = True
        '
        'rbBing
        '
        Me.rbBing.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rbBing.AutoSize = True
        Me.rbBing.Location = New System.Drawing.Point(544, 44)
        Me.rbBing.Name = "rbBing"
        Me.rbBing.Size = New System.Drawing.Size(109, 17)
        Me.rbBing.TabIndex = 260
        Me.rbBing.Text = "SatelliteBingsMap"
        Me.rbBing.UseVisualStyleBackColor = True
        '
        'lblSrodekSzer
        '
        Me.lblSrodekSzer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekSzer.AutoSize = True
        Me.lblSrodekSzer.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekSzer.Location = New System.Drawing.Point(528, 499)
        Me.lblSrodekSzer.Name = "lblSrodekSzer"
        Me.lblSrodekSzer.Size = New System.Drawing.Size(28, 13)
        Me.lblSrodekSzer.TabIndex = 261
        Me.lblSrodekSzer.Text = "52.3"
        '
        'lblSrodekOpisSzer
        '
        Me.lblSrodekOpisSzer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekOpisSzer.AutoSize = True
        Me.lblSrodekOpisSzer.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekOpisSzer.Location = New System.Drawing.Point(455, 499)
        Me.lblSrodekOpisSzer.Name = "lblSrodekOpisSzer"
        Me.lblSrodekOpisSzer.Size = New System.Drawing.Size(75, 13)
        Me.lblSrodekOpisSzer.TabIndex = 263
        Me.lblSrodekOpisSzer.Text = "WGS84  Lat ="
        '
        'lblSrodekTytul
        '
        Me.lblSrodekTytul.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekTytul.AutoSize = True
        Me.lblSrodekTytul.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekTytul.Location = New System.Drawing.Point(455, 482)
        Me.lblSrodekTytul.Name = "lblSrodekTytul"
        Me.lblSrodekTytul.Size = New System.Drawing.Size(161, 13)
        Me.lblSrodekTytul.TabIndex = 264
        Me.lblSrodekTytul.Text = "Współrzędne środka okna mapy"
        '
        'lblKursorTytul
        '
        Me.lblKursorTytul.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorTytul.AutoSize = True
        Me.lblKursorTytul.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorTytul.Location = New System.Drawing.Point(6, 463)
        Me.lblKursorTytul.Name = "lblKursorTytul"
        Me.lblKursorTytul.Size = New System.Drawing.Size(109, 13)
        Me.lblKursorTytul.TabIndex = 265
        Me.lblKursorTytul.Text = "Współrzędne kursora"
        '
        'lblKursorUkladOpis
        '
        Me.lblKursorUkladOpis.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorUkladOpis.AutoSize = True
        Me.lblKursorUkladOpis.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorUkladOpis.Location = New System.Drawing.Point(7, 480)
        Me.lblKursorUkladOpis.Name = "lblKursorUkladOpis"
        Me.lblKursorUkladOpis.Size = New System.Drawing.Size(31, 13)
        Me.lblKursorUkladOpis.TabIndex = 266
        Me.lblKursorUkladOpis.Text = "1992"
        '
        'lblKursorUklad
        '
        Me.lblKursorUklad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorUklad.AutoSize = True
        Me.lblKursorUklad.BackColor = System.Drawing.SystemColors.Window
        Me.lblKursorUklad.Location = New System.Drawing.Point(40, 480)
        Me.lblKursorUklad.Name = "lblKursorUklad"
        Me.lblKursorUklad.Size = New System.Drawing.Size(31, 13)
        Me.lblKursorUklad.TabIndex = 267
        Me.lblKursorUklad.Text = ""
        '
        'lblSrodekDlug
        '
        Me.lblSrodekDlug.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekDlug.AutoSize = True
        Me.lblSrodekDlug.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekDlug.Location = New System.Drawing.Point(609, 499)
        Me.lblSrodekDlug.Name = "lblSrodekDlug"
        Me.lblSrodekDlug.Size = New System.Drawing.Size(31, 13)
        Me.lblSrodekDlug.TabIndex = 290
        Me.lblSrodekDlug.Text = "19.2 "
        '
        'lblZoom
        '
        Me.lblZoom.AutoSize = True
        Me.lblZoom.Location = New System.Drawing.Point(9, 60)
        Me.lblZoom.Name = "lblZoom"
        Me.lblZoom.Size = New System.Drawing.Size(13, 13)
        Me.lblZoom.TabIndex = 292
        Me.lblZoom.Text = "6"
        '
        'lblSrodekOpisDlug
        '
        Me.lblSrodekOpisDlug.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSrodekOpisDlug.AutoSize = True
        Me.lblSrodekOpisDlug.BackColor = System.Drawing.SystemColors.Window
        Me.lblSrodekOpisDlug.Location = New System.Drawing.Point(576, 499)
        Me.lblSrodekOpisDlug.Name = "lblSrodekOpisDlug"
        Me.lblSrodekOpisDlug.Size = New System.Drawing.Size(31, 13)
        Me.lblSrodekOpisDlug.TabIndex = 293
        Me.lblSrodekOpisDlug.Text = "Lng="
        '
        'Label67
        '
        Me.Label67.AutoSize = True
        Me.Label67.BackColor = System.Drawing.SystemColors.ControlLight
        Me.Label67.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Label67.ForeColor = System.Drawing.Color.Green
        Me.Label67.Location = New System.Drawing.Point(334, 5)
        Me.Label67.Name = "Label67"
        Me.Label67.Size = New System.Drawing.Size(320, 13)
        Me.Label67.TabIndex = 297
        Me.Label67.Text = "Zaznacz na mapie  prawym  przyciskiem myszy obszar do pobrania"
        '
        'txtKomunikaty
        '
        Me.txtKomunikaty.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtKomunikaty.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.txtKomunikaty.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtKomunikaty.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtKomunikaty.Location = New System.Drawing.Point(5, 560)
        Me.txtKomunikaty.Name = "txtKomunikaty"
        Me.txtKomunikaty.Size = New System.Drawing.Size(1, 1)
        Me.txtKomunikaty.TabIndex = 301
        Me.txtKomunikaty.Text = "Komunikaty:"
        Me.txtKomunikaty.Visible = False
        '
        'lblKursorWgs
        '
        Me.lblKursorWgs.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblKursorWgs.AutoSize = True
        Me.lblKursorWgs.Location = New System.Drawing.Point(83, 499)
        Me.lblKursorWgs.Name = "lblKursorWgs"
        Me.lblKursorWgs.Size = New System.Drawing.Size(36, 13)
        Me.lblKursorWgs.TabIndex = 260
        Me.lblKursorWgs.Text = ""
        '
        'grpPodglad
        '
        Me.grpPodglad.Controls.Add(Me.picPodglad)
        Me.grpPodglad.Controls.Add(Me.lblTytulPodglad)
        Me.grpPodglad.Controls.Add(Me.btnPodgladZaawansowane)
        Me.grpPodglad.Controls.Add(Me.btnZoomMinus)
        Me.grpPodglad.Controls.Add(Me.btnZoomReset)
        Me.grpPodglad.Controls.Add(Me.btnZoomPlus)
        Me.grpPodglad.Controls.Add(Me.btnPodgladOdswiez)
        Me.grpPodglad.Controls.Add(Me.lblPodgladLiczbaWarstw)
        Me.grpPodglad.Controls.Add(Me.lblPodgladSkala)
        Me.grpPodglad.Controls.Add(Me.lblPodgladStan)
        Me.grpPodglad.Location = New System.Drawing.Point(564, 38)
        Me.grpPodglad.Name = "grpPodglad"
        Me.grpPodglad.Size = New System.Drawing.Size(286, 104)
        Me.grpPodglad.TabIndex = 302
        '
        'picPodglad
        '
        Me.picPodglad.BackColor = System.Drawing.Color.White
        Me.picPodglad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picPodglad.Cursor = System.Windows.Forms.Cursors.Hand
        Me.picPodglad.Location = New System.Drawing.Point(2, 2)
        Me.picPodglad.Name = "picPodglad"
        Me.picPodglad.Size = New System.Drawing.Size(88, 98)
        Me.picPodglad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.picPodglad.TabIndex = 0
        Me.picPodglad.TabStop = False
        Me.ToolTip1.SetToolTip(Me.picPodglad, "Bieżący podgląd zestawu warstw. Kółko myszy: przybliż/oddal. Kliknij, aby otworzyć mikser warstw i krycia.")
        '
        'lblTytulPodglad
        '
        Me.lblTytulPodglad.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTytulPodglad.ForeColor = System.Drawing.Color.DimGray
        Me.lblTytulPodglad.Location = New System.Drawing.Point(96, 0)
        Me.lblTytulPodglad.Name = "lblTytulPodglad"
        Me.lblTytulPodglad.Size = New System.Drawing.Size(186, 14)
        Me.lblTytulPodglad.TabIndex = 1
        Me.lblTytulPodglad.Text = "PODGLĄD WARSTW"
        '
        'btnPodgladZaawansowane
        '
        Me.btnPodgladZaawansowane.Font = New System.Drawing.Font("Segoe UI", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPodgladZaawansowane.Location = New System.Drawing.Point(96, 17)
        Me.btnPodgladZaawansowane.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPodgladZaawansowane.Name = "btnPodgladZaawansowane"
        Me.btnPodgladZaawansowane.Size = New System.Drawing.Size(186, 23)
        Me.btnPodgladZaawansowane.TabIndex = 2
        Me.btnPodgladZaawansowane.Text = "Mikser warstw i krycia..."
        Me.ToolTip1.SetToolTip(Me.btnPodgladZaawansowane, "Otwórz okno zaawansowanego miksera warstw, krycia i korekty obrazu")
        Me.btnPodgladZaawansowane.UseVisualStyleBackColor = True
        '
        'btnZoomMinus
        '
        Me.btnZoomMinus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZoomMinus.Location = New System.Drawing.Point(96, 43)
        Me.btnZoomMinus.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZoomMinus.Name = "btnZoomMinus"
        Me.btnZoomMinus.Size = New System.Drawing.Size(22, 22)
        Me.btnZoomMinus.TabIndex = 3
        Me.btnZoomMinus.Text = "-"
        Me.ToolTip1.SetToolTip(Me.btnZoomMinus, "Oddal kadr podglądu (lupa rastrowa)")
        Me.btnZoomMinus.UseVisualStyleBackColor = True
        '
        'btnZoomReset
        '
        Me.btnZoomReset.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZoomReset.Location = New System.Drawing.Point(120, 43)
        Me.btnZoomReset.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZoomReset.Name = "btnZoomReset"
        Me.btnZoomReset.Size = New System.Drawing.Size(26, 22)
        Me.btnZoomReset.TabIndex = 4
        Me.btnZoomReset.Text = "1:1"
        Me.ToolTip1.SetToolTip(Me.btnZoomReset, "Przywróć skalę 1:1 podglądu (100%)")
        Me.btnZoomReset.UseVisualStyleBackColor = True
        '
        'btnZoomPlus
        '
        Me.btnZoomPlus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnZoomPlus.Location = New System.Drawing.Point(148, 43)
        Me.btnZoomPlus.Margin = New System.Windows.Forms.Padding(0)
        Me.btnZoomPlus.Name = "btnZoomPlus"
        Me.btnZoomPlus.Size = New System.Drawing.Size(22, 22)
        Me.btnZoomPlus.TabIndex = 5
        Me.btnZoomPlus.Text = "+"
        Me.ToolTip1.SetToolTip(Me.btnZoomPlus, "Przybliż kadr podglądu (lupa rastrowa)")
        Me.btnZoomPlus.UseVisualStyleBackColor = True
        '
        'btnPodgladOdswiez
        '
        Me.btnPodgladOdswiez.Font = New System.Drawing.Font("Segoe UI", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnPodgladOdswiez.Location = New System.Drawing.Point(173, 43)
        Me.btnPodgladOdswiez.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPodgladOdswiez.Name = "btnPodgladOdswiez"
        Me.btnPodgladOdswiez.Padding = New System.Windows.Forms.Padding(0)
        Me.btnPodgladOdswiez.Size = New System.Drawing.Size(58, 22)
        Me.btnPodgladOdswiez.TabIndex = 6
        Me.btnPodgladOdswiez.Text = "Odśwież"
        Me.ToolTip1.SetToolTip(Me.btnPodgladOdswiez, "Wymuś ponowne pobranie próbki podglądu z serwerów.")
        Me.btnPodgladOdswiez.UseVisualStyleBackColor = True
        '
        'lblPodgladLiczbaWarstw
        '
        Me.lblPodgladLiczbaWarstw.Font = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPodgladLiczbaWarstw.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblPodgladLiczbaWarstw.Location = New System.Drawing.Point(96, 68)
        Me.lblPodgladLiczbaWarstw.Name = "lblPodgladLiczbaWarstw"
        Me.lblPodgladLiczbaWarstw.Size = New System.Drawing.Size(186, 14)
        Me.lblPodgladLiczbaWarstw.TabIndex = 7
        Me.lblPodgladLiczbaWarstw.Text = "Warstw: 0"
        '
        'lblPodgladSkala
        '
        Me.lblPodgladSkala.Font = New System.Drawing.Font("Segoe UI", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPodgladSkala.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblPodgladSkala.Location = New System.Drawing.Point(96, 84)
        Me.lblPodgladSkala.Name = "lblPodgladSkala"
        Me.lblPodgladSkala.Size = New System.Drawing.Size(90, 14)
        Me.lblPodgladSkala.TabIndex = 8
        Me.lblPodgladSkala.Text = "-"
        '
        'lblPodgladStan
        '
        Me.lblPodgladStan.AutoEllipsis = True
        Me.lblPodgladStan.Font = New System.Drawing.Font("Segoe UI", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblPodgladStan.ForeColor = System.Drawing.Color.DimGray
        Me.lblPodgladStan.Location = New System.Drawing.Point(188, 84)
        Me.lblPodgladStan.Name = "lblPodgladStan"
        Me.lblPodgladStan.Size = New System.Drawing.Size(94, 14)
        Me.lblPodgladStan.TabIndex = 9
        Me.lblPodgladStan.Text = "Brak warstw"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.btnUsunPobrane)
        Me.Panel2.Controls.Add(Me.grpDaneZrodlowe)
        Me.Panel2.Controls.Add(Me.btnZnaczniki)
        Me.Panel2.Controls.Add(Me.grpWybraneWarstwy)
        Me.Panel2.Controls.Add(Me.btnScalanie)
        Me.Panel2.Controls.Add(Me.txtKomunikaty)
        Me.Panel2.Controls.Add(Me.btnOtworzFolder)
        Me.Panel2.Controls.Add(Me.btnPobierz)
        Me.Panel2.Controls.Add(Me.btnPrzerwij)
        Me.Panel2.AutoScroll = True
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Right
        Me.Panel2.Location = New System.Drawing.Point(667, 24)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(217, 638)
        Me.Panel2.TabIndex = 307
        '
        'splPanel
        '
        Me.splPanel.Dock = System.Windows.Forms.DockStyle.Right
        Me.splPanel.Location = New System.Drawing.Point(661, 24)
        Me.splPanel.Name = "splPanel"
        Me.splPanel.Size = New System.Drawing.Size(6, 638)
        Me.splPanel.TabIndex = 309
        Me.splPanel.TabStop = False
        '
        'Panel3
        '
        Me.Panel3.AutoScroll = True
        Me.Panel3.Controls.Add(Me.pnlBelkaAkcji)
        Me.Panel3.Controls.Add(Me.grpObszar)
        Me.Panel3.Controls.Add(Me.lblSeparator1)
        Me.Panel3.Controls.Add(Me.grpRozdzielczoscFormat)
        Me.Panel3.Controls.Add(Me.lblSeparator2)
        Me.Panel3.Controls.Add(Me.grpPodglad)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel3.Location = New System.Drawing.Point(0, 515)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(857, 147)
        Me.Panel3.TabIndex = 308
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoScroll = False
        Me.ClientSize = New System.Drawing.Size(1080, 684)
        Me.Controls.Add(Me.lblKursorWgs)
        Me.Controls.Add(Me.lblKursorUklad)
        Me.Controls.Add(Me.lblKursorUkladOpis)
        Me.Controls.Add(Me.lblKursorTytul)
        Me.Controls.Add(Me.btnSzukajDzialki)
        Me.Controls.Add(Me.btnPomniejsz)
        Me.Controls.Add(Me.btnPowieksz)
        Me.Controls.Add(Me.lblSrodekOpisDlug)
        Me.Controls.Add(Me.lblZoom)
        Me.Controls.Add(Me.lblSrodekDlug)
        Me.Controls.Add(Me.lblSrodekTytul)
        Me.Controls.Add(Me.lblSrodekOpisSzer)
        Me.Controls.Add(Me.lblSrodekSzer)
        Me.Controls.Add(Me.rbBing)
        Me.Controls.Add(Me.rbGoogle)
        Me.Controls.Add(Me.rbOsm)
        Me.Controls.Add(Me.mapa)
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.splPanel)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Label67)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.DoubleBuffered = True
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Form1"
        Me.grpDaneZrodlowe.ResumeLayout(False)
        Me.grpDaneZrodlowe.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.grpWybraneWarstwy.ResumeLayout(False)
        Me.grpWybraneWarstwy.PerformLayout()
        CType(Me.numKrycie, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpObszar.ResumeLayout(False)
        Me.grpObszar.PerformLayout()
        Me.pnlSzacunek.ResumeLayout(False)
        Me.pnlSzacunek.PerformLayout()
        Me.pnlUkrytePolaZasiegu.ResumeLayout(False)
        Me.pnlUkrytePolaZasiegu.PerformLayout()
        Me.grpRozdzielczoscFormat.ResumeLayout(False)
        Me.grpRozdzielczoscFormat.PerformLayout()
        Me.grpPodglad.ResumeLayout(False)
        Me.grpPodglad.PerformLayout()
        CType(Me.picPodglad, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlBelkaAkcji.ResumeLayout(False)
        Me.pnlBelkaAkcji.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnPobierz As System.Windows.Forms.Button
    Friend WithEvents grpDaneZrodlowe As System.Windows.Forms.GroupBox
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents stPostep As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents stFolder As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents stFormat As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents btnPrzerwij As System.Windows.Forms.Button
    Friend WithEvents stSegment As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents SesjaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZapiszToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WczytajToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SesjaSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents KorytarzToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UsunKorytarzToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SesjaSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents OdswiezToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NarzedziaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DodajSerwerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FolderWlasnychWarstwToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NarzedziaSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents btnDodajSerwer As System.Windows.Forms.Button
    Friend WithEvents UstawieniaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PomocOnlineToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZglosBladToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FolderDanychToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents CoNowegoToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WesprzyjToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PomocPomorskieForumEksploracyjneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents txtSzukajWarstwy As System.Windows.Forms.TextBox
    Friend WithEvents btnZwinRozwin As System.Windows.Forms.Button
    Friend WithEvents btnGaleriaWarstw As System.Windows.Forms.Button
    Friend WithEvents lblUklad As System.Windows.Forms.Label
    Friend WithEvents cmbUklad As System.Windows.Forms.ComboBox
    Friend WithEvents tvWarstwy As MapoTero.DrzewoWarstw
    Friend WithEvents GaleriaWarstwToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents InstrukcjaObsługiToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label29 As System.Windows.Forms.Label
    Friend WithEvents KorektaFolderuToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NarzedziaSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents PrzeslijNaTelefonToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents KopiujNaGarminaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents txtBokSegmentu As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblRozmiarPikselaOpis As System.Windows.Forms.Label
    Friend WithEvents txtRozmiarPiksela As System.Windows.Forms.TextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents mapa As GMap.NET.WindowsForms.GMapControl
    Friend WithEvents rbOsm As System.Windows.Forms.RadioButton
    Friend WithEvents rbGoogle As System.Windows.Forms.RadioButton
    Friend WithEvents rbBing As System.Windows.Forms.RadioButton
    Friend WithEvents lblSrodekSzer As System.Windows.Forms.Label
    Friend WithEvents lblSrodekOpisSzer As System.Windows.Forms.Label
    'Friend WithEvents lblKursorWgs As System.Windows.Forms.Label
    Friend WithEvents lblSrodekTytul As System.Windows.Forms.Label
    Friend WithEvents lblKursorTytul As System.Windows.Forms.Label
    Friend WithEvents lblKursorUkladOpis As System.Windows.Forms.Label
    Friend WithEvents lblKursorUklad As System.Windows.Forms.Label
    Friend WithEvents grpWybraneWarstwy As System.Windows.Forms.GroupBox
    Friend WithEvents lblSrodekDlug As System.Windows.Forms.Label
    Friend WithEvents lblZoom As System.Windows.Forms.Label
    Friend WithEvents lblSrodekOpisDlug As System.Windows.Forms.Label
    Friend WithEvents btnPowieksz As System.Windows.Forms.Button
    Friend WithEvents btnPomniejsz As System.Windows.Forms.Button
    Friend WithEvents Label67 As System.Windows.Forms.Label
    Friend WithEvents ScalanieToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EksportToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EksportCadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents grpRozmiarSiatki As System.Windows.Forms.GroupBox
    Friend WithEvents txtLiczbaWierszy As System.Windows.Forms.TextBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents txtLiczbaKolumn As System.Windows.Forms.TextBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents txtWysokoscPx As System.Windows.Forms.TextBox
    Friend WithEvents txtSzerokoscPx As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtSzerokoscKm As System.Windows.Forms.TextBox
    Friend WithEvents txtWysokoscKm As System.Windows.Forms.TextBox
    Friend WithEvents txtKomunikaty As System.Windows.Forms.RichTextBox
    Friend WithEvents lblKursorWgs As System.Windows.Forms.Label
    Friend WithEvents txtZasiegSegmentuKm As System.Windows.Forms.TextBox
    Friend WithEvents Label47 As System.Windows.Forms.Label
    Friend WithEvents Label46 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents grpSchemat As System.Windows.Forms.GroupBox
    Friend WithEvents lblZaznDlugLewa As System.Windows.Forms.Label
    Friend WithEvents lblZaznSzerDol As System.Windows.Forms.Label
    Friend WithEvents lblZaznDlugPrawa As System.Windows.Forms.Label
    Friend WithEvents lblZaznSzerGora As System.Windows.Forms.Label
    Friend WithEvents lblOpisYPrawy As System.Windows.Forms.Label
    Friend WithEvents lblOpisXGora As System.Windows.Forms.Label
    Friend WithEvents txtYPrawy As System.Windows.Forms.TextBox
    Friend WithEvents txtXGora As System.Windows.Forms.TextBox
    Friend WithEvents lblOpisYLewy As System.Windows.Forms.Label
    Friend WithEvents lblOpisXDol As System.Windows.Forms.Label
    Friend WithEvents txtYLewy As System.Windows.Forms.TextBox
    Friend WithEvents txtXDol As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents chkEdycjaXY As System.Windows.Forms.CheckBox
    Friend WithEvents btnOtworzFolder As System.Windows.Forms.Button
    Friend WithEvents btnScalanie As System.Windows.Forms.Button
    Friend WithEvents lstWybraneWarstwy As MapoTero.ListaWarstw
    Friend WithEvents btnWarstwaWyzej As System.Windows.Forms.Button
    Friend WithEvents btnWarstwaNizej As System.Windows.Forms.Button
    Friend WithEvents btnUsunWarstwe As System.Windows.Forms.Button
    Friend WithEvents lblKrycie As System.Windows.Forms.Label
    Friend WithEvents numKrycie As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnWarstwyZaawansowane As System.Windows.Forms.Button
    Friend WithEvents Label42 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents btnZnaczniki As System.Windows.Forms.Button
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents splPanel As System.Windows.Forms.Splitter
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Label8 As Label
    Friend WithEvents btnUsunPobrane As Button
    Friend WithEvents pnlBelkaAkcji As System.Windows.Forms.Panel
    Friend WithEvents btnKreatorDolny As System.Windows.Forms.Button
    Friend WithEvents lblLiniaBelki As System.Windows.Forms.Label
    Friend WithEvents lblSeparator1 As System.Windows.Forms.Label
    Friend WithEvents lblSeparator2 As System.Windows.Forms.Label
    Friend WithEvents lblTytulObszar As System.Windows.Forms.Label
    Friend WithEvents lblTytulParametry As System.Windows.Forms.Label
    Friend WithEvents lblTytulPodglad As System.Windows.Forms.Label
    Friend WithEvents grpPodglad As System.Windows.Forms.Panel
    Friend WithEvents picPodglad As System.Windows.Forms.PictureBox
    Friend WithEvents btnPodgladZaawansowane As System.Windows.Forms.Button
    Friend WithEvents btnZoomPlus As System.Windows.Forms.Button
    Friend WithEvents btnZoomMinus As System.Windows.Forms.Button
    Friend WithEvents btnZoomReset As System.Windows.Forms.Button
    Friend WithEvents btnPodgladOdswiez As System.Windows.Forms.Button
    Friend WithEvents lblPodgladLiczbaWarstw As System.Windows.Forms.Label
    Friend WithEvents lblPodgladSkala As System.Windows.Forms.Label
    Friend WithEvents lblPodgladStan As System.Windows.Forms.Label
    Friend WithEvents ZnajdzDzialkeToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents KonwertujOziToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents KonwertujGeoTiffToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ProfilNmtToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnSzukajDzialki As System.Windows.Forms.Button
    Friend WithEvents grpObszar As System.Windows.Forms.Panel
    Friend WithEvents lblOpisWymiary As System.Windows.Forms.Label
    Friend WithEvents lblBadgeSegmenty As System.Windows.Forms.Label
    Friend WithEvents lblWymiaryTerenu As System.Windows.Forms.Label
    Friend WithEvents lblPowierzchniaTerenu As System.Windows.Forms.Label
    Friend WithEvents pnlSzacunek As System.Windows.Forms.Panel
    Friend WithEvents lblTytulRozmiar As System.Windows.Forms.Label
    Friend WithEvents lblSzacowanyRozmiar As System.Windows.Forms.Label
    Friend WithEvents lblSeparatorSzacunku As System.Windows.Forms.Label
    Friend WithEvents lblTytulCzas As System.Windows.Forms.Label
    Friend WithEvents lblSzacowanyCzas As System.Windows.Forms.Label
    Friend WithEvents lblUkladZasiegSkrot As System.Windows.Forms.Label
    Friend WithEvents btnWpiszWspolrzedne As System.Windows.Forms.Button
    Friend WithEvents pnlUkrytePolaZasiegu As System.Windows.Forms.Panel
    Friend WithEvents grpRozdzielczoscFormat As System.Windows.Forms.Panel
    Friend WithEvents lblOpisPiksela As System.Windows.Forms.Label
    Friend WithEvents lblJednostkaPiksela As System.Windows.Forms.Label
    Friend WithEvents cmbRozmiarPiksela As System.Windows.Forms.ComboBox
    Friend WithEvents lblOpisSegmentu As System.Windows.Forms.Label
    Friend WithEvents lblJednostkaSegmentu As System.Windows.Forms.Label
    Friend WithEvents lblBokKm As System.Windows.Forms.Label
    Friend WithEvents lblOpisFormatu As System.Windows.Forms.Label
    Friend WithEvents cmbFormatDocelowy As System.Windows.Forms.ComboBox
    Friend WithEvents btnPrzejdzDoEksportu As System.Windows.Forms.Button
    Friend WithEvents lblStatusSesji As System.Windows.Forms.Label
    Friend WithEvents lnkSzczegoly As System.Windows.Forms.LinkLabel
    Friend WithEvents btnOtworzFolderDolny As System.Windows.Forms.Button
    Friend WithEvents UsunPobraneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
