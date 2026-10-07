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
Imports GMap.NET
Imports GMap.NET.MapProviders

Public Class Form1

    Private Property Form1loaded As Boolean = False   'wskazuje, że form1 została już załadowana



    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId:="System.Double.ToString")> _
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'wyświetla nazwę i wersję 
        Me.Text = My.Application.Info.Title & " " & My.Application.Info.Version.ToString

        TBseg = 22
        TBbok = 512

        ' pozostałe parametry
        myPath = My.Application.Info.DirectoryPath.ToString()
        folderDanych = Module1.UstalFolderDanych(myPath)
        Module1.folderSegmentow = folderDanych & "\download\"
        If Directory.Exists(folderDanych & "\download\") = False Then Directory.CreateDirectory(folderDanych & "\download\")

        'serwery WMS (np. Geoportal) wymagają TLS 1.2 - na starszych wersjach Windows nie jest on domyślnie włączony
        System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol Or System.Net.SecurityProtocolType.Tls12

        Me.SetDesktopLocation(0, 0)

        'ustawienia startowe okna mapy

        'punkt startowy mapy głównej
        Me.GMapControl1.Manager.Mode = AccessMode.ServerAndCache
        Me.GMapControl1.DragButton = Windows.Forms.MouseButtons.Left
        Me.GMapControl1.MapScaleInfoEnabled = False
        Me.GMapControl1.DisableAltForSelection = True
        Me.GMapControl1.Zoom = Val(Label65.Text)

        TextBox1.Enabled = False
        TextBox2.Enabled = False
        TextBox3.Enabled = False
        TextBox4.Enabled = False
        Label3.Enabled = False
        Label4.Enabled = False
        Label5.Enabled = False
        Label6.Enabled = False


        Label31.Visible = False
        Label32.Visible = False
        Label33.Visible = False
        Label34.Visible = False


        'wczytywanie ustaleń okna z lastsetting. Jeśli go nie ma, to szuka conf. Gdy go zabraknie, to sięgamy po sztywny start

        If File.Exists(folderSegmentow & "\conf.txt") = False Then
            Button8.Enabled = False
        End If

        If File.Exists(folderSegmentow & "\conf.txt") = False Then
            Me.GMapControl1.Position = New PointLatLng(52.3, 19.2)
            Me.GMapControl1.Zoom = 6
        End If





        'przypisuje wartość zmiennej publicznej nrWarstwy
        nrWarstwy = 0
        wczytajConf()
        wczytaj_lastsettings()


        'tworzy combobox z listą dostępnych warstw
        wczytaj_warstwyTxt()
        wczytaj_warstwy_z_pliku()

        ToolStripStatusLabel2.Text = "." & format
        If format = "" Then
            ToolStripStatusLabel2.Text = "jpeg"
            Form2.ComboBox1.Text = "jpeg"
        End If
        ToolStripStatusLabel3.Text = ""

        form1loaded = True


    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Module1.plik_lastsettings()

    End Sub

    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
        RichTextBox1.ForeColor = System.Drawing.Color.Black
        RichTextBox1.Text = "Trwa pobieranie segmentów"

        'w zależności od formatu obrazka odpowiednie rozszerzenie pliku
        Select Case Module1.format
            Case "jpeg"
                Module1.rozszerzenie = "jpg" 'format wysyłania zapytań do wms
            Case "tiff"
                Module1.rozszerzenie = "tif"
            Case "png"
                Module1.rozszerzenie = "png"
            Case "png8"
                Module1.rozszerzenie = "png"
            Case "png24"
                Module1.rozszerzenie = "png"
            Case "png32"
                Module1.rozszerzenie = "png"
            Case "gif"
                Module1.rozszerzenie = "gif"
            Case "svg+xml"
                'Module1.rozszerzenie = "svg"
                '==> dopóki svg nie działa będzie tak:
                MsgBox("Ten format jeszcze nie działa :o(", , "Zmień format.")
                Form2.ShowDialog()
                GoTo errorhandler

        End Select

        pobierz = True

        Button3.Enabled = True

        folderSegmentow = ToolStripStatusLabel1.Text
        Module1.utworzPlikConf() 'na wszelki wypadek - żeby nie okazało się, że zapisuje w folderze głównym programu


        If folderSegmentow = "" Then
            MsgBox("folder segmentów jest pusty")
            GoTo errorhandler
        End If

        'tworzy plik koordynaty, kolejne procedury dodadzą do niego dane
        FileOpen(1, folderSegmentow & "koordynaty.txt", OpenMode.Output)
        WriteLine(1, folderSegmentow)           'zapisuje ścieżkę dostępu do folderu
        WriteLine(1, Val(TextBox9.Text))        'długość boku segmentu
        FileClose(1)

        Module1.proceduraGlowna()

errorhandler:
    End Sub


    'dodawanie warstw
    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged


        If Form2.ComboBox3.SelectedIndex = 0 Then
            styl_nazwy_TB = ""
            Form2.Label10.Text = Form2.TextBox6.Text
        End If
        If Form2.ComboBox3.SelectedIndex = 1 Then
            styl_nazwy_TB = ComboBox3.Text
            Form2.Label10.Text = Form2.TextBox6.Text & Module1.styl_nazwy_TB
        End If
        If Form2.ComboBox3.SelectedIndex = 2 Then
            styl_nazwy_TB = Label11.Text.Replace("1) ", "")
            Form2.Label10.Text = Form2.TextBox6.Text & Module1.styl_nazwy_TB
        End If
        If Form2.ComboBox3.SelectedIndex = 3 Then
            styl_nazwy_TB = ComboBox3.Text & "_" & Label11.Text.Replace("1) ", "")
            Form2.Label10.Text = Form2.TextBox6.Text & Module1.styl_nazwy_TB
        End If




        Dim linia As String = ""    'przechowuje linię pliku

        If nrWarstwy > 11 Then       'jeśli zechcesz wyznaczyć więcej warstw niż miejsca w tablicy
            MsgBox("TEJ WARSTWY NIE MOŻNA JUŻ DODAĆ.", , )
            GoTo errororhandler
        End If

        'zapisuje w tablicy dane z zaznaczonego wiersza
        warstwy(nrWarstwy) = ListBox1.SelectedItem

        nrWarstwy += 1

        Label11.Text = "1) " & warstwy(0)
        Label12.Text = "2) " & warstwy(1)
        Label13.Text = "3) " & warstwy(2)
        Label14.Text = "4) " & warstwy(3)
        Label15.Text = "5) " & warstwy(4)
        Label16.Text = "6) " & warstwy(5)
        Label27.Text = "7) " & warstwy(6)
        Label24.Text = "8) " & warstwy(7)
        Label28.Text = "9) " & warstwy(8)
        Label26.Text = "10) " & warstwy(9)
        Label25.Text = "11) " & warstwy(10)
        Label23.Text = "12) " & warstwy(11)


        'wpisuje do TextBox10 wartość m/pix przypisaną do danej warstwy
        FileOpen(1, myPath & "\warstwy\" & ComboBox3.Text & ".txt", OpenMode.Input)

        Input(1, adresSerwera)

        'przetważa plik, aż znajdzie linię zgodną z zaznaczeniem listbox1
        Do Until linia = ListBox1.SelectedItem Or EOF(1)
            Input(1, linia)
        Loop
        'wtedy przechodzi linię niżej i pobiera ją jako ilość m/piksel
        Input(1, TextBox10.Text)

        FileClose(1)

        If warstwy(0) <> "" Then
            RichTextBox1.ForeColor = System.Drawing.Color.Green
            RichTextBox1.Text = "Wskazano warstwę: " & ListBox1.SelectedItem
        End If
        'tymczasowe komunikaty informujące o wybraniu WMS HGIS, który posiada zabójcze ograniczenia rozdzielczości
        Select Case ListBox1.SelectedItem
            Case "m25k"
                TextBox9.Text = 250
                RichTextBox1.ForeColor = System.Drawing.Color.Blue
                RichTextBox1.Text = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:25 000 Messtischblatt. Pokrywa ona swoim zasięgiem terytorium Zaboru Pruskiego. Wskazany WMS portalu hgis.cartomatic.pl, który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "wig25k"
                TextBox9.Text = 250
                RichTextBox1.ForeColor = System.Drawing.Color.Blue
                RichTextBox1.Text = "Wybrałeś warstwę polskiej mapy topograficznej 1:25 000 Wojskowego Instytutu Geograficznego. Pokrywa ona głównie środkową i północną część terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."

            Case "wig100k"
                TextBox9.Text = 250
                RichTextBox1.ForeColor = System.Drawing.Color.Blue
                RichTextBox1.Text = "Wybrałeś warstwę polskiej mapy topograficznej 1:100 000 Wojskowego Instytutu Geograficznego. Pokrywa ona terytorium II RP. Wybrany WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr"
                TextBox9.Text = 250
                RichTextBox1.ForeColor = System.Drawing.Color.Blue
                RichTextBox1.Text = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Karte des Deutschen Reiches. Obejmuje ona terytorium Zaboru Pruskiego. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
            Case "kdr_gb"
                TextBox9.Text = 250
                RichTextBox1.ForeColor = System.Drawing.Color.Blue
                RichTextBox1.Text = "Wybrałeś warstwę niemieckiej mapy topograficznej 1:100 000  Grossblatt. Obejmuje ona większość terytorium IIIRP. Wybrany WMS historycznych map hgis.cartomatic.pl posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
        End Select
        If ListBox1.SelectedItem = "m25k" Or ListBox1.SelectedItem = "wig100k" Then

            TextBox9.Text = 250
            RichTextBox1.ForeColor = System.Drawing.Color.Green
            RichTextBox1.Text = "Wybrałeś WMS historycznych map hgis.cartomatic.pl,który posiada ograniczenia maksymalnego rozmiaru segmentu 256px."
        End If
errororhandler:
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox8.Text = Val(TextBox11.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
    End Sub
    Private Sub TextBox4_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox4.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox8.Text = Val(TextBox11.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
    End Sub
    Private Sub TextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Val(TextBox12.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
    End Sub
    Private Sub TextBox3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Val(TextBox12.Text) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
    End Sub
    Private Sub TextBox9_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox9.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox8.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox13.Text = (Val(TextBox10.Text) * Val(TextBox9.Text)) / 1000
        If Me.GMapControl1.SelectedArea.IsEmpty = False Then
            Me.GMapControl1.Overlays.Clear()
            Module1.Markery()
        End If
    End Sub
    Private Sub TextBox10_TextChanged(sender As Object, e As EventArgs) Handles TextBox10.TextChanged
        TextBox5.Text = (Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox6.Text = (Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000
        TextBox7.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox8.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text)
        TextBox11.Text = Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox12.Text = Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text)))
        TextBox13.Text = (Val(TextBox10.Text) * Val(TextBox9.Text)) / 1000

        If Me.GMapControl1.SelectedArea.IsEmpty = False Then
            Me.GMapControl1.Overlays.Clear()
            Module1.Markery()
        End If
    End Sub

    Private Sub ZapiszToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZapiszToolStripMenuItem.Click
        Module1.utworzPlikConf()
        RichTextBox1.ForeColor = System.Drawing.Color.Green
        RichTextBox1.Text = "Zapisano ustawienia sesji do pliku conf.txt zlokalizowanym w folderze " & folderSegmentow
    End Sub

    Private Sub WczytajToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WczytajToolStripMenuItem1.Click
        FileClose(1) ' na wypadek gdyby był otwarty
        'dawniej wczytywano conf dwukrotnie (obejście błędu opisanego w WczytajConf) - po poprawce wystarcza jeden odczyt
        wczytajConf()
    End Sub

    Private Sub OtwórzOknoUstawieńToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UstawieniaToolStripMenuItem.Click
        Form2.ShowDialog()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox1.ShowDialog()
    End Sub

    Private Sub PomocPomorskieForumEksploracyjneToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PomocPomorskieForumEksploracyjneToolStripMenuItem.Click
        pomoc_pfe.ShowDialog()
    End Sub

    Private Sub Instrukcja_obslugi_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles InstrukcjaObsługiToolStripMenuItem.Click
        Instrukcja_Obslugi.ShowDialog()
    End Sub

    Public Sub Wczytaj_lastsettings()

        Dim plik As String = folderDanych & "\lastsettings.txt"
        If File.Exists(plik) = False Then Exit Sub

        'plik zawiera pary wierszy: nazwa ustawienia, wartość. Odczyt odbywa się po nazwach, a nie po kolejności wierszy,
        'dzięki czemu brak któregoś ustawienia (np. starszy plik bez chkkml/chktab) nie przesuwa wszystkich kolejnych wartości
        Dim ustawienia As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Try
            'kodowanie systemowe (ANSI) - takie samo, jakim plik jest zapisywany przez PrintLine
            Dim linie() As String = File.ReadAllLines(plik, System.Text.Encoding.Default)
            For i = 0 To linie.Length - 2 Step 2
                ustawienia(linie(i).Trim()) = linie(i + 1).Trim()
            Next
        Catch
            Exit Sub
        End Try

        Dim w As String = ""
        If ustawienia.TryGetValue("folder segmentow", w) AndAlso w <> "" Then folderSegmentow = w   'ostatni folder segmentów, z niego wczytany zostanie plik conf
        CheckGmi = WartoscLogiczna(ustawienia, "chkgmi", CheckGmi)                  'czy tworzyć gmi
        CheckMap = WartoscLogiczna(ustawienia, "chkmap", CheckMap)                  'czy tworzyć map
        CheckWldPoints = WartoscLogiczna(ustawienia, "chkwldpoints", CheckWldPoints) 'czy tworzyć wld i points
        CheckJpgw = WartoscLogiczna(ustawienia, "chkjpgw", CheckJpgw)               'czy tworzyć jpgw
        CheckKml = WartoscLogiczna(ustawienia, "chkkml", CheckKml)                  'czy tworzyć kml
        CheckTab = WartoscLogiczna(ustawienia, "chktab", CheckTab)                  'czy tworzyć tab
        If ustawienia.TryGetValue("dolna", w) AndAlso w <> "" Then folderWarstwa1 = w      'foldery łączonych warstw
        If ustawienia.TryGetValue("gorna", w) AndAlso w <> "" Then folderWarstwa2 = w
        If ustawienia.TryGetValue("polaczone", w) AndAlso w <> "" Then folderWynikowy = w
        XYswitched = WartoscLogiczna(ustawienia, "XYswitched", XYswitched)
        'zapisywane jako "numeracja_" (starsze pliki domyślne zawierały "numeracja")
        If ustawienia.TryGetValue("numeracja_", w) AndAlso w <> "" Then
            numeracja = w
        ElseIf ustawienia.TryGetValue("numeracja", w) AndAlso w <> "" Then
            numeracja = w
        End If
        Dim liczba As Integer
        If ustawienia.TryGetValue("iloscProbPobrania", w) AndAlso Integer.TryParse(w, liczba) Then iloscProbPobrania = liczba
        If ustawienia.TryGetValue("przerwaMiedzyProbami", w) AndAlso Integer.TryParse(w, liczba) Then przerwaMiedzyProbami = liczba
        If ustawienia.TryGetValue("x_start", w) AndAlso w <> "" Then Label35.Text = w
        If ustawienia.TryGetValue("y_start", w) AndAlso w <> "" Then Label63.Text = w
        If ustawienia.TryGetValue("zoom_start", w) AndAlso w <> "" Then Label65.Text = w

        'wczytywanie ostatnio zapisanej pozycji okna mapy
        Me.GMapControl1.Zoom = Val(Label65.Text)
        Me.GMapControl1.Position = New PointLatLng(Val(Label35.Text.ToString), Val(Label63.Text.ToString))


        RichTextBox1.ForeColor = System.Drawing.Color.Green
        RichTextBox1.Text = "Wczytano ostatnio zapisane ustawienia programu z lastsettings.txt. Styl numerowania segmentów to: " & numeracja

    End Sub

    'odczytuje wartość True/False ustawienia; gdy jej brak lub jest nieczytelna - pozostawia dotychczasową
    Private Function WartoscLogiczna(ByVal ustawienia As Dictionary(Of String, String), ByVal nazwa As String, ByVal domyslna As Boolean) As Boolean
        Dim w As String = ""
        Dim wynik As Boolean
        If ustawienia.TryGetValue(nazwa, w) AndAlso Boolean.TryParse(w, wynik) Then Return wynik
        Return domyslna
    End Function

    Private Sub WczytajConf()

        Dim formatNaProbe As String = "" 'służy do wczytania rozszerzenia obrazka z pliku i jeśli module1.format jest inny to następuje zamiana

        If Form1loaded = True Then
            'Me.FolderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer
            Me.FolderBrowserDialog1.SelectedPath = folderDanych & "\download\"
            If Me.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                folderSegmentow = Me.FolderBrowserDialog1.SelectedPath & "\"
            End If
            If Dir(folderSegmentow & "\conf.txt") = "" Then
                MsgBox("Brak pliku w podanej lokalizacji.")
                Exit Sub
            End If
        End If

        'wyświetla nazwę kwadratu na pasku stanu
        ToolStripStatusLabel1.Text = folderSegmentow

        'przy pierwszym uruchomieniu conf.txt jeszcze nie ma
        If File.Exists(folderSegmentow & "\conf.txt") = False Then Exit Sub

        'Cały plik jest najpierw wczytywany do zmiennych, a dopiero potem przypisywany do kontrolek.
        'Przypisanie rodzaju mapy (ComboBox3) uruchamia wczytanie listy warstw, które zamyka plik nr 1 -
        'gdy działo się to w trakcie czytania conf.txt, reszta pliku nie była wczytywana
        '(dawny "nierozwiązany bug", obchodzony dwukrotnym wczytywaniem conf.txt).
        Dim folderZPliku As String = ""         'ścieżka zapisana w conf.txt - nieużywana: obowiązuje folder, w którym faktycznie znaleziono conf.txt
        Dim rodzajMapy As String = ""
        Dim x1 As String = "", y1 As String = "", x2 As String = "", y2 As String = ""
        Dim bokSegmentu As String = "", rozmiarPiksela As String = ""
        Dim warstwyZPliku(11) As String
        Dim nrWarstwyZPliku As Integer
        Dim nazwaKwadratuZPliku As String = ""
        Dim powyzejOstatniegoZPliku As Boolean
        Dim srodekX As String = "", srodekY As String = "", zoomZPliku As String = ""
        Dim numeracjaZPliku As String = ""

        Dim nrPliku As Integer = FreeFile()
        Try
            FileOpen(nrPliku, folderSegmentow & "\conf.txt", OpenMode.Input)
            Input(nrPliku, folderZPliku)
            Input(nrPliku, rodzajMapy)
            Input(nrPliku, x1)
            Input(nrPliku, y1)
            Input(nrPliku, x2)
            Input(nrPliku, y2)
            Input(nrPliku, bokSegmentu)
            Input(nrPliku, rozmiarPiksela)
            For i = 0 To 11
                Input(nrPliku, warstwyZPliku(i))
            Next
            Input(nrPliku, nrWarstwyZPliku)
            Input(nrPliku, formatNaProbe)
            Input(nrPliku, nazwaKwadratuZPliku)
            Input(nrPliku, powyzejOstatniegoZPliku)
            Input(nrPliku, srodekX)
            Input(nrPliku, srodekY)
            Input(nrPliku, zoomZPliku)
            Try
                Input(nrPliku, numeracjaZPliku)  'starsze pliki conf.txt mogą nie zawierać stylu numeracji
            Catch
                numeracjaZPliku = ""
            End Try
        Catch
            FileClose(nrPliku)
            Exit Sub
        End Try
        FileClose(nrPliku)

        ComboBox3.Text = rodzajMapy
        TextBox1.Text = x1
        TextBox2.Text = y1
        TextBox3.Text = x2
        TextBox4.Text = y2
        TextBox9.Text = bokSegmentu
        TextBox10.Text = rozmiarPiksela
        For i = 0 To 11
            If warstwyZPliku(i) = "#ERROR 448#" Then warstwyZPliku(i) = ""
            warstwy(i) = warstwyZPliku(i)
        Next
        nrWarstwy = nrWarstwyZPliku
        wspolnaNazwaKwadratu = nazwaKwadratuZPliku
        pobierajPowyzejOstatniego = powyzejOstatniegoZPliku
        Label35.Text = srodekX
        Label63.Text = srodekY
        Label65.Text = zoomZPliku
        If numeracjaZPliku <> "" Then
            Form2.ComboBox2.Text = numeracjaZPliku
            Module1.numeracja = numeracjaZPliku
        End If

        If formatNaProbe <> Module1.format Then

            Module1.format = formatNaProbe

        End If

        ToolStripStatusLabel2.Text = "." & format
        Label11.Text = "1) " & warstwy(0)
        Label12.Text = "2) " & warstwy(1)
        Label13.Text = "3) " & warstwy(2)
        Label14.Text = "4) " & warstwy(3)
        Label15.Text = "5) " & warstwy(4)
        Label16.Text = "6) " & warstwy(5)
        Label27.Text = "7) " & warstwy(6)
        Label24.Text = "8) " & warstwy(7)
        Label28.Text = "9) " & warstwy(8)
        Label26.Text = "10) " & warstwy(9)
        Label25.Text = "11) " & warstwy(10)
        Label23.Text = "12) " & warstwy(11)

        'wyłączyłem wyskakujące okienko folderu download (30. 03. 2015. Kazik)
        'If folderSegmentow <> "" Then
        'Process.Start(folderSegmentow)
        ' End If

        'odświeżenie widoku okna mapy po wprowadzeniu nowych ustawień conf.txt
        Me.GMapControl1.Refresh()
        Me.GMapControl1.ReloadMap()
        Me.GMapControl1.Zoom = Val(Label65.Text)
        Me.GMapControl1.Position = New PointLatLng(Val(Label35.Text.ToString), Val(Label63.Text.ToString))
        x_start = Label35.Text
        y_start = Label63.Text
        zoom_start = Label65.Text
        'End If

        Me.Refresh()


        RichTextBox1.ForeColor = System.Drawing.Color.Green
        RichTextBox1.Text = "Wczytano ostatnio zapisane ustawienia sesji z pliku conf.txt. Format graficzny pobieranych segmentów to " & format & " . Styl ich numerowania to: " & numeracja

    End Sub
    'wczytanie listy plików z warstwami do combobox3
    Private Sub Wczytaj_warstwyTxt()

        Dim pozycjaListy As String = ""

        FileClose(1) 'w razie gyby był otwarty

        FileOpen(1, myPath & "\warstwy\warstwy.txt", OpenMode.Input)

        Do Until EOF(1)
            Input(1, pozycjaListy)
            ComboBox3.Items.Add(pozycjaListy)
        Loop

        FileClose(1)

errorhandler:
    End Sub
    'wcztanie danych do listbox1 w zależności od wyboru w combobox3
    Private Sub Wczytaj_warstwy_z_pliku()

        Dim pozycjaListy As String = ""
        Dim pusta As String = ""    'zmienna na linie, które nie będą użyte

        FileClose(1) 'w razie gyby był otwarty

        ListBox1.Items.Clear()

        'brak pliku (np. zbiór map zapisany w conf.txt, a usunięty w nowszej wersji programu) - dawniej przy
        '"On Error Resume Next" pętla Do Until EOF nigdy się nie kończyła i program zawieszał się przy starcie
        Dim plikWarstw As String = myPath & "\warstwy\" & ComboBox3.Text & ".txt"
        If File.Exists(plikWarstw) = False Then Exit Sub

        Try
            FileOpen(1, plikWarstw, OpenMode.Input)

            Input(1, adresSerwera)

            Do Until EOF(1)
                Input(1, pozycjaListy)
                ListBox1.Items.Add(pozycjaListy)
                If Not EOF(1) Then Input(1, pusta)
            Loop
        Catch
            'uszkodzony plik warstw - lista zawiera to, co udało się odczytać
        Finally
            FileClose(1)
        End Try

    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox3.SelectedIndexChanged
        'zeruje wybrane warstwy
        Module1.przerwij()
        'wczytuje nowe do wyboru
        wczytaj_warstwy_z_pliku()


        'czyszczenie listy dotychczasowych warstw
        nrWarstwy = 0
        For i = 0 To 11
            warstwy(i) = ""
        Next
        Label11.Text = "1) " & warstwy(0)
        Label12.Text = "2) " & warstwy(1)
        Label13.Text = "3) " & warstwy(2)
        Label14.Text = "4) " & warstwy(3)
        Label15.Text = "5) " & warstwy(4)
        Label16.Text = "6) " & warstwy(5)
        Label27.Text = "7) " & warstwy(6)
        Label24.Text = "8) " & warstwy(7)
        Label28.Text = "9) " & warstwy(8)
        Label26.Text = "10) " & warstwy(9)
        Label25.Text = "11) " & warstwy(10)
        Label23.Text = "12) " & warstwy(11)

        strUrlparts(1) = ""


        If warstwy(0) = "" Then
            RichTextBox1.ForeColor = System.Drawing.Color.Red
            RichTextBox1.Text = "Zmieniono rodzaj mapy. Wybierz która dokładnie jej warstwa ma zostać pobrana, klikając na nią myszką"
        End If

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Module1.przerwij()
        Button1.Enabled = True
        Button3.Enabled = False
        RichTextBox1.ForeColor = System.Drawing.Color.Red
        RichTextBox1.Text = "Przerwano pobieranie segmentów"
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Module1.resetuj()
        Module1.utworzPlikConf()
        Button1.Enabled = True
        Button3.Enabled = False

    End Sub

    Private Sub UsuńPusteSegmentyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UsuwaniePustychSegmentówToolStripMenuItem.Click
        usuwanie_p_seg.ShowDialog()
    End Sub
    Private Sub SkładajWarstwyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NakładanieWarstwNaSiebieToolStripMenuItem.Click
        Nakladanie_Map.ShowDialog()
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        MapProviders.GMapProvider.UserAgent = "MapoTero 3"
        Me.GMapControl1.MapProvider = GMapProviders.OpenStreetMap
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        Me.GMapControl1.MapProvider = GMapProviders.GoogleMap
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        Me.GMapControl1.MapProvider = GMapProviders.BingSatelliteMap
    End Sub

    Private Sub GMapControl1_MouseMove(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseMove

        'współrzędne bbox podczas przeciągania zaznaczenia
        If Module1.editXY = False Then
            If Me.GMapControl1.SelectedArea.IsEmpty = False Then
                Dim X0_92 As Integer
                Dim Y0_92 As Integer
                Dim X1_92 As Integer
                Dim Y1_92 As Integer
                Dim X0_84 = Me.GMapControl1.SelectedArea.Lng
                Dim Y1_84 = Me.GMapControl1.SelectedArea.Lat
                Dim X1_84 = Me.GMapControl1.SelectedArea.Lng + Me.GMapControl1.SelectedArea.WidthLng
                Dim Y0_84 = Me.GMapControl1.SelectedArea.Lat - Me.GMapControl1.SelectedArea.HeightLat


                X0_92 = Transform.GetXU92(Y1_84, X1_84)   'prawy gorny x
                TextBox3.Text = X0_92
                Y0_92 = Transform.GetYU92(Y1_84, X1_84)  'prawy gorny y
                TextBox4.Text = Y0_92
                X1_92 = Transform.GetXU92(Y0_84, X0_84)   'lewy dolny x
                TextBox1.Text = X1_92
                Y1_92 = Transform.GetYU92(Y0_84, X0_84)  'lewy gorny y
                TextBox2.Text = Y1_92


                Label31.Text = "lat= " + Convert.ToString(Round(Y1_84, 4))
                Label32.Text = "lat= " + Convert.ToString(Round(Y0_84, 4))
                Label33.Text = "lng= " + Convert.ToString(Round(X1_84, 4))
                Label34.Text = "lng= " + Convert.ToString(Round(X0_84, 4))

                If Val(TextBox8.Text) * Val(TextBox7.Text) < 5000000000 Then
                    RichTextBox1.ForeColor = System.Drawing.Color.Green
                    RichTextBox1.Text = "Zaznaczyłeś obszar do pobrania o powierzchni " & ((Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) * ((Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) & " km2"

                Else
                    RichTextBox1.ForeColor = System.Drawing.Color.Red
                    RichTextBox1.Text = "Zaznaczyłeś obszar do pobrania o powierzchni " & ((Math.Ceiling(((Val(TextBox4.Text) - Val(TextBox2.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) * ((Math.Ceiling(((Val(TextBox3.Text) - Val(TextBox1.Text))) / (Val(TextBox10.Text) * Val(TextBox9.Text))) * Val(TextBox9.Text) * Val(TextBox10.Text)) / 1000) & " km2. To dużo. Poradzę sobie. Jednak uzbroj się lepiej w kubek gorącej kawy :)"
                End If
            End If
        End If

        'współrzędne środka mapy w wgs84
        Dim Lat_srodek As Double = Me.GMapControl1.Position.Lat
        Dim Lng_srodek As Double = Me.GMapControl1.Position.Lng
        Label35.Text = Convert.ToString(Round(Lat_srodek, 4))
        Label63.Text = Convert.ToString(Round(Lng_srodek, 4))
        'współrzędne podczas ruchu myszą
        Dim lat_mysz As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat
        Dim lng_mysz As Double = GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng

        Dim lng_92 As Integer = Transform.GetXU92(lat_mysz, lng_mysz)   'prawy gorny x
        Dim lat_92 As Integer = Transform.GetYU92(lat_mysz, lng_mysz)   'prawy gorny y
        Label41.Text = "x= " + Convert.ToString(lng_92) + "   y= " + Convert.ToString(lat_92)

        Dim mouseY As Double = e.Location.Y
        Dim mouseX As Double = e.Location.X
        Label36.BackColor = Color.Transparent
        Label36.Location = New Point(mouseX - 140, mouseY - 10)
        'Label43.Text = "lat= " + Convert.ToString(Round(lat_mysz, 4)) + "   lng= " + Convert.ToString(Round(lng_mysz, 4))
        If kursorWGS84 = True Then
            Label36.Text = "lat= " + Convert.ToString(Round(lat_mysz, 4)) + "   lng= " + Convert.ToString(Round(lng_mysz, 4))
            Label36.Visible = True
        Else
            Label36.Visible = False
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim zoom As Integer = Me.GMapControl1.Zoom
        Me.GMapControl1.Zoom = zoom + 1
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim zoom As Integer = Me.GMapControl1.Zoom
        Me.GMapControl1.Zoom = zoom - 1
    End Sub

    Private Sub GMapControl1_OnMapZoomChanged() Handles GMapControl1.OnMapZoomChanged
        Label65.Text = Me.GMapControl1.Zoom.ToString
    End Sub

    Private Sub ScalanieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScalanieToolStripMenuItem.Click
        Form3.ShowDialog()
    End Sub


    Private Sub Label11_TextChanged(sender As Object, e As EventArgs) Handles Label11.TextChanged
        If warstwy(0) <> "" Then
            Button6.Enabled = True
        Else
            Button6.Enabled = False
        End If
    End Sub


    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If Directory.Exists(folderDanych & "\download\") = False Then
            Directory.CreateDirectory(folderDanych & "\download\")
            Process.Start(folderDanych & "\download")
        Else
            Process.Start(folderDanych & "\download")
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click

        Dim OdpMBox As DialogResult = MessageBox.Show("Czy na pewno usunąć cały katalog 'download' z pobranymi mapami?", "Usuwanie zawartości katalogu download", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If OdpMBox = Windows.Forms.DialogResult.Yes Then
            If Directory.Exists(folderDanych & "\download\") = True Then
                System.IO.Directory.Delete(folderDanych & "\download\", True)
                Directory.CreateDirectory(folderDanych & "\download\")
            End If
            RichTextBox1.ForeColor = System.Drawing.Color.Green
            RichTextBox1.Text = "Usunięto całą zawartość katalogu download"

        End If


    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Form3.ShowDialog()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Module1.resetuj()
        Module1.utworzPlikConf()
        Button1.Enabled = True
        Button3.Enabled = False
    End Sub


    Private Sub GMapControl1_MouseDoubleClick(ByValsender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GMapControl1.MouseDoubleClick
        Dim zoom As Integer = Me.GMapControl1.Zoom
        If e.Button = MouseButtons.Right Then
            Me.GMapControl1.Zoom = zoom - 1
        Else
            Me.GMapControl1.Zoom = zoom + 1
        End If
    End Sub

    'markery rzeczywistego zasięgu pobieranej mapy
    Private Sub Button4_Click_1(sender As Object, e As EventArgs) Handles Button4.Click
        Me.GMapControl1.Overlays.Clear()
        Module1.Markery()
    End Sub

    'markery wyskakujące po zaznaczeniu obszaru pobierania
    Private Sub GMapControl1_MouseClick(ByValsender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GMapControl1.MouseClick
        If Me.GMapControl1.SelectedArea.IsEmpty = False Then
            If e.Button = MouseButtons.Right Then
                Me.GMapControl1.Overlays.Clear()
                Module1.Markery()
            End If
        End If
    End Sub

    'zamiana przecinka na kropkę w textbox10 odpowiedzialnym za rozmiar piksela
    Private Sub TextBox10_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox10.KeyUp
        TextBox10.Text = TextBox10.Text.Replace(",", ".")
        Dim pozycja As String
        pozycja = TextBox10.SelectionStart 'pozycja kursora
        TextBox10.SelectionStart = TextBox10.Text.Length 'ustawienie kursora na koncu
    End Sub
End Class



