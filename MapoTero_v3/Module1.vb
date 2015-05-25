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
	
Imports System
Imports System.Threading
Imports System.Diagnostics
Imports System.Net
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

Module Module1
    'deklaracja procedury w nagłówku (sekcja General)
    'cała deklaracja ma być w jednej linii!
    Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Long)

    Public adresSerwera As String
    Public warstwy(11) As String                 'tablica przechowująca nazwy warstw
    Public nrWarstwy As Integer                 'zmienia się w miarę wybierania kolejnych warstw, po to aby wyznaczać im kolejne miejsca w tabeli
    Public strUrlparts(8) As String             'tablica zbierająca części do stworzenia linku
    Public strUrl As String                     'zmienna zawierająca link
    Public ileSegHoriz As Integer
    Public ileSegVert As Integer
    Public wspolnaNazwaKwadratu As String = ""       'wspólna część nazwy kwadratów - bez kolejnych numerów
    Public nazwaKwadratu As String           'nazwa i numer kwadratu
    Public pion As Integer                      'zmienna do pętli odliczającej kwadraty (zamiast "i")
    Public poz As Integer
    Public nrKwadratu As String                 'numer segmentu
    Public folderSegmentow As String
    Public pobierz As Boolean
    Public myPath As String                     'ścieżka tej aplikacji
    Public folderWarstwa1 As String           'folder warstwy dolnej
    Public folderWarstwa2 As String          'folder warstwy górnej
    Public folderWynikowy As String            'folder złączonych warstw
    Public iloscProbPobrania As Integer = 3   'docelowa ilość prób
    Public przerwaMiedzyProbami As Integer = 5 'odstęp między kolejnymi próbami w sekundach
    Public numeracja As String = "NrWiersza_NrKolumny"                    'sposób numerowania segmentów
 



    Public format As String                  'jpeg / png / svg+xml
    Public rozszerzenie As String = "jpg"              'jpg / png / svg
    Public CheckMap As Boolean = False          'tworzyć .map
    Public CheckGmi As Boolean = False          'tworzyć .gmi
    Public CheckWldPoints As Boolean = False    'tworzyc .wld i .points
    Public CheckJpgw As Boolean = False         'tworzyć .jpgw
    Public CheckKml As Boolean = False         'tworzyć .kml
    Public CheckTab As Boolean = False         'tworzyć .tab
    Public XYswitched As Boolean = False        'geoportal zwariował - zamienił x i y w zapytaniu
    Public CheckTB As Boolean = False           'tworzy mapy Trek Buddy
    Public pobierajPowyzejOstatniego As Boolean = False 'określa czy przy powtórnym pobieraniu ściągać wszystko, czy tylko powyżej segmentu o największym numerze
    Public editXY As Boolean = False          'włącza edycję pól formularza XY zasięgu mapy
    Public kursorWGS84 As Boolean = False          'wyświetla współrzędne kursora w układzie WGS84
    Public kursor_i_srodekmapy As Boolean = True          'wyświetla współrzędne kursora i środka mapy


    Public zaznaczenieWGS84 As Boolean = False          'wyświetla współrzędne zaznaczenia w układzie WGS84
    Public georef_scalanie_qgis As Boolean = False        'opcja referencji qgis dla scalanych plików
    Public georef_scalanie_kml As Boolean = False        'opcja referencji kml dla scalanych plików
    Public georef_scalanie_map As Boolean = False        'opcja referencji map dla scalanych plików
    Public georef_scalanie_tab As Boolean = False        'opcja referencji map dla scalanych plików

    Public Lx As Long                           'używane w segmentach
    Public Ly As Long
    Public Px As Long
    Public Py As Long

    Public TBseg As Long                        'ilość segmentów w boku mapy w jednym folderze
    Public TBbok As Integer                     'bok jednego segmentu w pikselach
    Public TBmax As Integer                     'zmienna określająca jaki maksymalnie rozmiar mapy trafi do jednego katalogu SET
    Public folderTB As String                   'główny folder mapy
    Public folderTBset As String                'folder SET


    Public dlY As Long                          'romiar Y mapy w pixelach
    Public dlX As Long                          'romiar X mapy w pixelach
    Public folY As Integer                      'ilość folderów w poziomie
    Public folX As Integer                      'ilość folderów w pionie


    Public x_start As String       'współrzędne x środka mapy zapisywane do conf.txt i odczytywane przy wznowieniu sesji
    Public y_start As String          'współrzędne y środka mapy zapisywane do conf.txt i odczytywane przy wznowieniu sesji"
    Public zoom_start As String       'skala mapy zapisywana do conf.txt i odczytywane przy wznowieniu sesji"






    Public Sub proceduraGlowna()

        Dim width As Long             'rozmiary mapy w pixelach
        Dim height As Long
        Dim proba As Integer                'aktualna próba pobrania
        Dim iloscProbPobrania As Integer    'docelowa ilość prób
        Dim przerwaMiedzyProbami As Integer 'odstęp między kolejnymi próbami w sekundach
        Dim Error1Linia As String = ""      'zmienna przechowująca pierwszy wiersz pliku error.txt
        Dim ostatniSegment As Integer      'przechowuje numer najwyższego segmentu w katalogu


        Form1.Button1.Enabled = False
        Form1.ComboBox3.Enabled = False
        Form1.ListBox1.Enabled = False
        Form1.Button8.Enabled = False

        If Module1.CheckTB = True Then
            TBfoldery()
        End If

        'OBSŁUGA BŁĘDÓW - czy wpisano ilość m/pix

        If Form1.TextBox10.Text = "" Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Nie podano rozmiaru pojedyńczego piksela segmentu"

            GoTo errorhandler
        ElseIf Val(Form1.TextBox10.Text) <= 0 Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Rozmiar pojedyńczego piksela segmentu musi być większy od zera"
            GoTo errorhandler
        End If

        If Form1.TextBox9.Text = "" Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Nie podano długości boku segmentu."
            GoTo errorhandler
        ElseIf Val(Form1.TextBox9.Text) < 1 Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Długość boku segmentu musi być większa od zera."
            GoTo errorhandler
        ElseIf Val(Form1.TextBox9.Text) > 2048 Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Długość boku segmentu musi być mniejsza od 2048px."
            GoTo errorhandler

        End If

        'OBSŁUGA BŁĘDÓW - koniec

        width = (Val(Form1.TextBox4.Text) - Val(Form1.TextBox2.Text)) / Val(Form1.TextBox10.Text)
        ileSegHoriz = Math.Ceiling(width / Val(Form1.TextBox9.Text)) 'Math.Ceiling() - zaokrąglenie w górę
        ileSegVert = Math.Ceiling(height / Val(Form1.TextBox9.Text))
        width = ileSegHoriz * Val(Form1.TextBox9.Text)
        height = (Val(Form1.TextBox3.Text) - Val(Form1.TextBox1.Text)) / Val(Form1.TextBox10.Text)
        ileSegVert = Math.Ceiling(height / Val(Form1.TextBox9.Text))
        height = ileSegVert * Val(Form1.TextBox9.Text)



        'OBSŁUGA BŁĘDÓW - czy prawe współrzędne nie są niższe od lewych

        If width <= 0 Or height <= 0 Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Niepoprawne współrzędne X, Y."
            GoTo errorhandler
        End If

        'czy wybrano chociaż jedną warstwę
        ' If nrWarstwy < 1 Then
        If warstwy(0) = "" And pobierz = True Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Nie wybrano żadnej warstwy. Aby wybrać warstwę kliknij w jej nazwę."
            GoTo errorhandler
        End If


        strUrlparts(0) = adresSerwera

        For i = 0 To nrWarstwy - 1


            If i < nrWarstwy - 1 Then
                strUrlparts(1) = strUrlparts(1) & warstwy(i) & ","
            Else
                strUrlparts(1) = strUrlparts(1) & warstwy(i) & "&bbox="
            End If

        Next


        'jeśli ma pobierać tylko powyżej najwyższego segmentu oblicza, który to segment
        If pobierajPowyzejOstatniego = True Then

            If numeracja = "1_2_3" Then
                For i = 1 To ileSegHoriz * ileSegVert


                    If Dir(folderSegmentow & wspolnaNazwaKwadratu & i & "." & rozszerzenie) <> "" Then
                        ostatniSegment = i
                    End If


                Next
            ElseIf numeracja = "01_02_03" Then
                For i = 1 To ileSegHoriz * ileSegVert


                    If Dir(folderSegmentow & wspolnaNazwaKwadratu & i.ToString("D2") & "." & rozszerzenie) <> "" Then
                        ostatniSegment = i
                    End If

                Next

            ElseIf numeracja = "NrWiersza_NrKolumny" Then


                For Module1.pion = 1 To ileSegVert

                    For Module1.poz = 1 To ileSegHoriz
                        If Dir(folderSegmentow & wspolnaNazwaKwadratu & pion.ToString("D2") & "_" & poz.ToString("D2") & "." & rozszerzenie) <> "" Then
                            ostatniSegment = poz
                        End If
                    Next
                    If Dir(folderSegmentow & wspolnaNazwaKwadratu & pion.ToString("D2") & "_" & poz.ToString("D2") & "." & rozszerzenie) <> "" Then
                        ostatniSegment = pion
                    End If


                Next
            End If
        End If

        Dim nrKwadratu As Integer

pobieranieJeszczeRaz:

        'przy każdym przebiegu tworzy na nowo pusty plik error.txt
        FileOpen(1, folderSegmentow & "error.txt", OpenMode.Output)
        FileClose(1)

        proba = proba + 1


        '/////////////////////////////////////////////////////////////
        'stąd zaczyna się fragment linku, który jest inny dla każdego segmentu


        For Module1.pion = 1 To ileSegVert

            For Module1.poz = 1 To ileSegHoriz

                Dim pionPix As Long = (pion * Form1.TextBox9.Text) - Form1.TextBox9.Text     'TB
                Dim pozPix As Long = (poz * Form1.TextBox9.Text) - Form1.TextBox9.Text       'TB
                Dim nrKwadratuPix As String = "_" & pozPix & "_" & pionPix                  'stosowany w mapach TB

                Dim nrKwadratu_seg As String = pion.ToString("D2") & "_" & poz.ToString("D2")                'domyślna numeracja typu _wiersz_kolumna

                nrKwadratu = nrKwadratu + 1

                'jeśli dla TB to inne nazwy plikówdouble
                Select Case CheckTB
                    Case True
                        nazwaKwadratu = wspolnaNazwaKwadratu & nrKwadratuPix
                    Case False
                        If numeracja = "NrWiersza_NrKolumny" Then nazwaKwadratu = wspolnaNazwaKwadratu & nrKwadratu_seg
                        If numeracja = "01_02_03" Then nazwaKwadratu = wspolnaNazwaKwadratu & nrKwadratu.ToString("D2")
                        If numeracja = "1_2_3" Then nazwaKwadratu = wspolnaNazwaKwadratu & nrKwadratu


                End Select


                Form1.ToolStripStatusLabel3.Text = nazwaKwadratu

                Select Case XYswitched
                    'po staremu
                    Case False
                        Lx = (Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (ileSegVert - pion)))
                        Ly = (Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (poz - 1)))
                        'geoportal zwariował i zamienił "x" z "y"
                    Case True
                        Lx = (Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (ileSegVert - pion)))
                        Ly = (Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (poz - 1)))

                End Select







                Lx = (Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (ileSegVert - pion)))
                Ly = (Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (poz - 1)))
                Px = (Lx + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)))
                Py = (Ly + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)))




                ' Select georef_scalanie_qgis

                'Case False
                'Lx = (Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (ileSegVert - pion)))
                'Ly = (Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)) * (poz - 1)))
                'Px = (Lx + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)))
                'Py = (Ly + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox10.Text)))
                'Case True
                ''Lx = (Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox12.Text))))
                ''Ly = (Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox9.Text) * Val(Form1.TextBox11.Text))))
                ''Px = (Lx + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox12.Text)))
                ''Py = (Ly + (Val(Form1.TextBox9.Text) * Val(Form1.TextBox12.Text)))
                'End Select



                strUrlparts(2) = Lx & ","               'lewy X
                strUrlparts(3) = Ly & ","               'lewy Y
                strUrlparts(4) = Px & ","

                strUrlparts(5) = Py & "&format=image/" & format & "&styles=&width="
                'Edit 1.04.2015 by Kazik - likwiduję wpis "-1" dla zachowania  rozdzielczości pobieranego segmentu zgodnie z zadaną w formularzu
                'strUrlparts(6) = Form1.TextBox9.Text - 1 & "&height="

                'strUrlparts(7) = Form1.TextBox9.Text - 1

                strUrlparts(6) = Form1.TextBox9.Text & "&height="

                strUrlparts(7) = Form1.TextBox9.Text

                'składa pozycje tablicy w link
                strUrl = ""
                For k = 0 To 7
                    strUrl = strUrl & strUrlparts(k)
                Next

                If pobierz = True Then


                    Select Case pobierajPowyzejOstatniego
                        Case True

                            If nrKwadratu > ostatniSegment Then
                                downloadPic()
                                plikGeoreferencyjny_map()
                                plikGeoreferencyjny_gmi()
                                plikGeoreferencyjny_wld()
                                plikGeoreferencyjny_points()
                                plikGeoreferencyjny_jpgw()
                                plikGeoreferencyjny_kml()
                                plikGeoreferencyjny_tab()

                            End If
                        Case False
                            downloadPic()
                            plikGeoreferencyjny_map()
                            plikGeoreferencyjny_gmi()
                            plikGeoreferencyjny_wld()
                            plikGeoreferencyjny_points()
                            plikGeoreferencyjny_jpgw()
                            plikGeoreferencyjny_kml()
                            plikGeoreferencyjny_tab()
                            plikTB_Set()
                    End Select


                    If proba = 1 Then
                        plikZkoordynatami()
                    End If

                Else

                    GoTo errorhandler

                End If


                Form1.ToolStripProgressBar1.Value = (nrKwadratu / (ileSegHoriz * ileSegVert)) * 100
            Next



        Next


        'zaczynamy od początku - kolejna próba jeśli plik error.txt istnieje i nie jest pusty


        'jeśli plik error.txt istnieje i nie jest pusty, to znaczy, że są błędy - nie pobrano wszystkich segmentów
        If Dir(folderSegmentow & "error.txt") <> "" Then
            FileOpen(1, folderSegmentow & "error.txt", OpenMode.Input)
            Do While Not EOF(1)
                Input(1, Error1Linia)
            Loop
            FileClose(1)
        End If

        If Len(Error1Linia) > 0 And proba < iloscProbPobrania Then
            nrKwadratu = 0

            Sleep(przerwaMiedzyProbami * 1000)

            GoTo pobieranieJeszczeRaz
        End If


        Form1.ComboBox3.Enabled = True
        Form1.ListBox1.Enabled = True


        Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
        Form1.RichTextBox1.Text = "Zakończono pobieranie"

errorhandler:

        Form1.Button1.Enabled = True

        Form1.Button3.Enabled = False

        Form1.ComboBox3.Enabled = True
        Form1.ListBox1.Enabled = True

        Form1.ToolStripProgressBar1.Value = 0
        Form1.ToolStripStatusLabel3.Text = ""





        'kasowanie zmiennych

        nrKwadratu = 0
        pobierz = False

        For i = 0 To nrWarstwy - 1

            strUrlparts(1) = ""

        Next

        If Len(Error1Linia) > 0 Then
            'MsgBox("Nie udało się ściągnąć wszystkich segmentów." & Chr(13) & Chr(10) & _
            '"Wykaz tych segmentów w pliku:" & Chr(13) & Chr(10) & _
            'folderSegmentow & "error.txt")
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Red
            Form1.RichTextBox1.Text = "Nie udało się ściągnąć wszystkich segmentów. Wykaz tych segmentów w pliku error.txt"
        Else
            File.Delete(folderSegmentow & "error.txt")
        End If

        If File.Exists(myPath & "\download\error.txt") = False Then Form1.Button8.Enabled = True


    End Sub



    'procedura pobiera segmenty z internetu i zapisuje je na dysku

    Public Sub downloadPic()


        Application.DoEvents()  'dzięki temu form1 nie "zamraża" się podczas pracy programu

        Select Case CheckTB
            Case True
                If Dir(folderTBset & nazwaKwadratu & "." & rozszerzenie) = "" Then

                    'zmienne niezbędne do image.fromstream
                    Dim requestPic As WebRequest = WebRequest.Create(strUrl)
                    On Error Resume Next
                    Dim responsePic As WebResponse = requestPic.GetResponse

                    On Error Resume Next
                    Image.FromStream(responsePic.GetResponseStream).Save(folderTBset & nazwaKwadratu & "." & rozszerzenie)
                    responsePic.Close()

                End If
                'po instrukcji pobrania jeszcze raz sprawdza, czy plik istnieje, jeśl nie powstał (nie został ściągnięty)
                'to dodaje wpis do pliku error.txt
                If Dir(folderTBset & nazwaKwadratu & "." & rozszerzenie) = "" Then
                    'dodaje wpis do pliku o błędach
                    plikError()
                End If

            Case False


                If Dir(folderSegmentow & nazwaKwadratu & "." & rozszerzenie) = "" Then

                    'zmienne niezbędne do image.fromstream
                    Dim requestPic As WebRequest = WebRequest.Create(strUrl)
                    On Error Resume Next
                    Dim responsePic As WebResponse = requestPic.GetResponse

                    On Error Resume Next

                    Image.FromStream(responsePic.GetResponseStream).Save(folderSegmentow & nazwaKwadratu & "." & rozszerzenie)
                    responsePic.Close()

                End If
                'po instrukcji pobrania jeszcze raz sprawdza, czy plik istnieje, jeśl nie powstał (nie został ściągnięty)
                'to dodaje wpis do pliku error.txt
                If Dir(folderSegmentow & nazwaKwadratu & "." & rozszerzenie) = "" Then
                    'dodaje wpis do pliku o błędach
                    plikError()
                End If


        End Select



    End Sub

    'procedura przerywa pobieranie

    Public Sub przerwij()

        nrWarstwy = 0


        Form1.ComboBox3.Enabled = True
        Form1.ListBox1.Enabled = True

        'For i = 0 To 11
        'warstwy(i) = ""
        'Next

        ' Form1.Label11.Text = "1) " & warstwy(0)
        'Form1.Label12.Text = "2) " & warstwy(1)
        ' Form1.Label13.Text = "3) " & warstwy(2)
        'Form1.Label14.Text = "4) " & warstwy(3)
        ' Form1.Label15.Text = "5) " & warstwy(4)
        'Form1.Label16.Text = "6) " & warstwy(5)
        'Form1.Label27.Text = "7) " & warstwy(6)
        'Form1.Label24.Text = "8) " & warstwy(7)
        'Form1.Label28.Text = "9) " & warstwy(8)
        ' Form1.Label26.Text = "10) " & warstwy(9)
        ' Form1.Label25.Text = "11) " & warstwy(10)
        ' Form1.Label23.Text = "12) " & warstwy(11)

        'strUrlparts(1) = ""


        pobierz = False

      
    End Sub
    'procedura resetowania wprowadzonych warstw

    Public Sub resetuj()

        nrWarstwy = 0


        Form1.ComboBox3.Enabled = True
        Form1.ListBox1.Enabled = True

        For i = 0 To 11
            warstwy(i) = ""
        Next

        Form1.Label11.Text = "1) " & warstwy(0)
        Form1.Label12.Text = "2) " & warstwy(1)
        Form1.Label13.Text = "3) " & warstwy(2)
        Form1.Label14.Text = "4) " & warstwy(3)
        Form1.Label15.Text = "5) " & warstwy(4)
        Form1.Label16.Text = "6) " & warstwy(5)
        Form1.Label27.Text = "7) " & warstwy(6)
        Form1.Label24.Text = "8) " & warstwy(7)
        Form1.Label28.Text = "9) " & warstwy(8)
        Form1.Label26.Text = "10) " & warstwy(9)
        Form1.Label25.Text = "11) " & warstwy(10)
        Form1.Label23.Text = "12) " & warstwy(11)

        strUrlparts(1) = ""


        pobierz = True

        Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
        Form1.RichTextBox1.Text = "Zresetowano listę wprowadzonych warstw mapy wskazanych do pobrania"
    End Sub
#Region "lastsetting.txt - tworzenie pliku z ustawieniami "

    Public Sub plik_lastsettings()

        Dim x_start_lastset As String = Replace(Form1.Label35.Text, ",", ".") 'z startowy odczytywany z lastsetting
        Dim y_start_lastset As String = Replace(Form1.Label63.Text, ",", ".") 'y startowy odczytywany z lastsetting
        Dim zoom_start_lastset As String = Replace(Form1.Label65.Text, ",", ".") 'zoom startowy odczytywany z lastsetting
        'tworzy plik lastsettings.txt

        FileClose(1) 'w razie gyby był otwarty

        FileOpen(1, myPath & "\lastsettings.txt", OpenMode.Output)

        PrintLine(1, "folder segmentow")        'zapisuje ostatni folder segmentów, z niego wczytany zostanie plik conf
        If folderSegmentow = "" Then
            PrintLine(1, myPath & "\download\")
        Else
            PrintLine(1, folderSegmentow)
        End If

        PrintLine(1, "chkgmi")
        PrintLine(1, CheckGmi)          'zapisuje czy tworzyć gmi
        PrintLine(1, "chkmap")
        PrintLine(1, CheckMap)          'zapisuje czy tworzyć map
        PrintLine(1, "chkwldpoints")
        PrintLine(1, CheckWldPoints)    'zapisuje czy tworzyć wld i points
        PrintLine(1, "chkjpgw")
        PrintLine(1, CheckJpgw)          'zapisuje czy tworzyć jpgw
        PrintLine(1, "chkkml")
        PrintLine(1, CheckKml)          'zapisuje czy tworzyć kml
        PrintLine(1, "chktab")
        PrintLine(1, CheckTab)          'zapisuje czy tworzyć kml
        PrintLine(1, "dolna")           'zapisuje foldery łączonych warstw
        If folderWarstwa1 = "" Then
            PrintLine(1, myPath & "\download\dolna\")
        Else
            PrintLine(1, folderWarstwa1)
        End If

        PrintLine(1, "gorna")
        If folderWarstwa2 = "" Then
            PrintLine(1, myPath & "\download\gorna\")
        Else
            PrintLine(1, folderWarstwa2)
        End If

        PrintLine(1, "polaczone")
        If folderWynikowy = "" Then
            PrintLine(1, myPath & "\download\polaczone\")
        Else
            PrintLine(1, folderWynikowy)
        End If

        PrintLine(1, "XYswitched")
        PrintLine(1, XYswitched)

        'PrintLine(1, "chkTB")
        'PrintLine(1, CheckTB)


        PrintLine(1, "numeracja_")
        PrintLine(1, numeracja)
        PrintLine(1, "iloscProbPobrania")
        PrintLine(1, iloscProbPobrania)
        PrintLine(1, "przerwaMiedzyProbami")
        PrintLine(1, przerwaMiedzyProbami)

        PrintLine(1, "x_start")
        PrintLine(1, x_start_lastset)
        PrintLine(1, "y_start")
        PrintLine(1, y_start_lastset)
        PrintLine(1, "zoom_start")
        PrintLine(1, zoom_start_lastset)
        FileClose(1)

    End Sub
#End Region
#Region "conf.txt - tworzenie pliku z parametrami sesji pobierania "
    Public Sub utworzPlikConf()


        Dim x_start_lastset As String = Replace(Form1.Label35.Text, ",", ".") 'y startowy odczytywany z lastsetting
        Dim y_start_lastset As String = Replace(Form1.Label63.Text, ",", ".") 'y startowy odczytywany z lastsetting
        Dim zoom_start_lastset As String = Replace(Form1.Label65.Text, ",", ".") 'y startowy odczytywany z lastsetting

        If pobierz = False Then

            Form1.FolderBrowserDialog1.SelectedPath = folderSegmentow
            If Form1.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                folderSegmentow = Form1.FolderBrowserDialog1.SelectedPath & "\"


            End If

        End If

        'wyświetla nazwę kwadratu na pasku stanu
        Form1.ToolStripStatusLabel1.Text = folderSegmentow


        'tworzy plik z danymi sesji
        FileOpen(1, folderSegmentow & "\conf.txt", OpenMode.Output)
        WriteLine(1, folderSegmentow)
        WriteLine(1, Form1.ComboBox3.Text)
        WriteLine(1, Form1.TextBox1.Text)
        WriteLine(1, Form1.TextBox2.Text)
        WriteLine(1, Form1.TextBox3.Text)
        WriteLine(1, Form1.TextBox4.Text)
        WriteLine(1, Form1.TextBox9.Text)
        WriteLine(1, Form1.TextBox10.Text)
        WriteLine(1, warstwy(0))
        WriteLine(1, warstwy(1))
        WriteLine(1, warstwy(2))
        WriteLine(1, warstwy(3))
        WriteLine(1, warstwy(4))
        WriteLine(1, warstwy(5))
        WriteLine(1, warstwy(6))
        WriteLine(1, warstwy(7))
        WriteLine(1, warstwy(8))
        WriteLine(1, warstwy(9))
        WriteLine(1, warstwy(10))
        WriteLine(1, warstwy(11))
        WriteLine(1, nrWarstwy)
        WriteLine(1, format)
        WriteLine(1, wspolnaNazwaKwadratu)
        WriteLine(1, pobierajPowyzejOstatniego)
            WriteLine(1, x_start_lastset)
            WriteLine(1, y_start_lastset)
            WriteLine(1, zoom_start_lastset)
        WriteLine(1, Module1.numeracja)
        FileClose(1)

        If pobierz = False Then
            Form1.RichTextBox1.ForeColor = System.Drawing.Color.Green
            Form1.RichTextBox1.Text = "Zapisano plik z parametrami sesji"
        End If

        'If folderSegmentow <> "" Then

        'Process.Start(folderSegmentow)
        'End If

errorhandler:
    End Sub
#End Region


    Public Sub plikGeoreferencyjny_gmi()

        If CheckGmi = True Then

            Dim MMPLL1sz As String  'lewy górny
            Dim MMPLL1dl As String
            Dim MMPLL2sz As String  'prawy górny
            Dim MMPLL2dl As String
            Dim MMPLL3sz As String  'prawy dolny
            Dim MMPLL3dl As String
            Dim MMPLL4sz As String  'lewy dolny
            Dim MMPLL4dl As String

            MMPLL1sz = Round(SzerokoscWgs_z1992(Px, Ly), 13)
            MMPLL1dl = Round(DlugoscWgs_z1992(Px, Ly), 13)
            MMPLL2sz = Round(SzerokoscWgs_z1992(Px, Py), 13)
            MMPLL2dl = Round(DlugoscWgs_z1992(Px, Py), 13)
            MMPLL3sz = Round(SzerokoscWgs_z1992(Lx, Py), 13)
            MMPLL3dl = Round(DlugoscWgs_z1992(Lx, Py), 13)
            MMPLL4sz = Round(SzerokoscWgs_z1992(Lx, Ly), 13)
            MMPLL4dl = Round(DlugoscWgs_z1992(Lx, Ly), 13)


            'usuwa przecinki i zamienia je na kropki
            Dim MMPLL1szDot As String = Replace(MMPLL1sz, ",", ".")     'lewy górny
            Dim MMPLL1dlDot As String = Replace(MMPLL1dl, ",", ".")
            Dim MMPLL2szDot As String = Replace(MMPLL2sz, ",", ".")     'prawy górny
            Dim MMPLL2dlDot As String = Replace(MMPLL2dl, ",", ".")
            Dim MMPLL3szDot As String = Replace(MMPLL3sz, ",", ".")     'prawy dolny
            Dim MMPLL3dlDot As String = Replace(MMPLL3dl, ",", ".")
            Dim MMPLL4szDot As String = Replace(MMPLL4sz, ",", ".")     'lewy dolny
            Dim MMPLL4dlDot As String = Replace(MMPLL4dl, ",", ".")


            'tworzy plik .gmi
            FileOpen(4, folderSegmentow & nazwaKwadratu & ".gmi", OpenMode.Output)
            Print(4, "Map Calibration data file v3.0" & Chr(13) & Chr(10) & _
                 nazwaKwadratu & "." & rozszerzenie & Chr(13) & Chr(10) & _
                 Form1.TextBox9.Text & Chr(13) & Chr(10) & _
                 Form1.TextBox9.Text & Chr(13) & Chr(10) & _
                 "0;0;" & MMPLL1dlDot & ";" & MMPLL1szDot & Chr(13) & Chr(10) & _
                 Form1.TextBox9.Text & ";0;" & MMPLL2dlDot & ";" & MMPLL2szDot & Chr(13) & Chr(10) & _
                 Form1.TextBox9.Text & ";" & Form1.TextBox9.Text & ";" & MMPLL3dlDot & ";" & MMPLL3szDot & Chr(13) & Chr(10) & _
                 "0;" & Form1.TextBox9.Text & ";" & MMPLL4dlDot & ";" & MMPLL4szDot)
            FileClose(4)

        End If


    End Sub

    Public Sub plikGeoreferencyjny_wld()

        If CheckWldPoints = True Then

            Dim LGsz As String  'lewy górny
            Dim LGdl As String


            LGsz = Round(SzerokoscWgs_z1992(Px, Ly), 15)
            LGdl = Round(DlugoscWgs_z1992(Px, Ly), 15)


            'usuwa przecinki i zamienia je na kropki
            Dim LGszDot As String = Replace(LGsz, ",", ".")     'lewy górny
            Dim LGdlDot As String = Replace(LGdl, ",", ".")

            'sprawdza ilość znaków w zmiennej i w razie potrzeby dodaje zera na końcu
            LGszDot = Dodaj_zera(LGszDot, 18)
            LGdlDot = Dodaj_zera(LGdlDot, 18)


            'tworzy plik .wld
            FileOpen(1, folderSegmentow & nazwaKwadratu & ".wld", OpenMode.Output)
            Print(1, "0.000000000000000" & Chr(13) & Chr(10) & _
                 "0" & Chr(13) & Chr(10) & _
                 "0" & Chr(13) & Chr(10) & _
                 "-0.000000000000000" & Chr(13) & Chr(10) & _
                 LGdlDot & Chr(13) & Chr(10) & _
                 LGszDot)
            FileClose(1)

        End If

    End Sub

    Public Sub plikGeoreferencyjny_points()

        If CheckWldPoints = True Then

            Dim bok As String = Form1.TextBox9.Text & ".000000000000000"

            Dim LGsz As String  'lewy górny
            Dim LGdl As String
            Dim PGsz As String  'prawy górny
            Dim PGdl As String
            Dim PDsz As String  'prawy dolny
            Dim PDdl As String
            Dim LDsz As String  'lewy dolny
            Dim LDdl As String

            LGsz = Round(SzerokoscWgs_z1992(Px, Ly), 15)
            LGdl = Round(DlugoscWgs_z1992(Px, Ly), 15)
            PGsz = Round(SzerokoscWgs_z1992(Px, Py), 15)
            PGdl = Round(DlugoscWgs_z1992(Px, Py), 15)
            PDsz = Round(SzerokoscWgs_z1992(Lx, Py), 15)
            PDdl = Round(DlugoscWgs_z1992(Lx, Py), 15)
            LDsz = Round(SzerokoscWgs_z1992(Lx, Ly), 15)
            LDdl = Round(DlugoscWgs_z1992(Lx, Ly), 15)

            'usuwa przecinki i zamienia je na kropki
            Dim LGszDot As String = Replace(LGsz, ",", ".")     'lewy górny
            Dim LGdlDot As String = Replace(LGdl, ",", ".")
            Dim PGszDot As String = Replace(PGsz, ",", ".")     'prawy górny
            Dim PGdlDot As String = Replace(PGdl, ",", ".")
            Dim PDszDot As String = Replace(PDsz, ",", ".")     'prawy dolny
            Dim PDdlDot As String = Replace(PDdl, ",", ".")
            Dim LDszDot As String = Replace(LDsz, ",", ".")     'lewy dolny
            Dim LDdlDot As String = Replace(LDdl, ",", ".")

            'sprawdza ilość znaków w zmiennej i w razie potrzeby dodaje zera na końcu
            LGszDot = Dodaj_zera(LGszDot, 18)
            LGdlDot = Dodaj_zera(LGdlDot, 18)
            PGszDot = Dodaj_zera(PGszDot, 18)
            PGdlDot = Dodaj_zera(PGdlDot, 18)
            PDszDot = Dodaj_zera(PDszDot, 18)
            PDdlDot = Dodaj_zera(PDdlDot, 18)
            LDszDot = Dodaj_zera(LDszDot, 18)
            LDdlDot = Dodaj_zera(LDdlDot, 18)


            'tworzy plik .points
            Select Case rozszerzenie
                Case "jpg"
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".jpg.points", OpenMode.Output)
                Case "png" & "png8" & "png24" & "png32"
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".png.points", OpenMode.Output)
                Case "tif"
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".tif.points", OpenMode.Output)
                Case "gif"
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".gif.points", OpenMode.Output)
            End Select

            Print(1, "mapX mapY pixelX pixelY" & Chr(13) & Chr(10) & _
                LGdlDot & Chr(9) & LGszDot & Chr(9) & "0.000000000000000" & Chr(9) & "0.000000000000000" & Chr(13) & Chr(10) & _
                PGdlDot & Chr(9) & PGszDot & Chr(9) & bok & Chr(9) & "0.000000000000000" & Chr(13) & Chr(10) & _
                PDdlDot & Chr(9) & PDszDot & Chr(9) & bok & Chr(9) & "-" & bok & Chr(13) & Chr(10) & _
                LDdlDot & Chr(9) & LDszDot & Chr(9) & "0.000000000000000" & Chr(9) & "-" & bok & Chr(13) & Chr(10))
            FileClose(1)

        End If

    End Sub
    Public Sub plikGeoreferencyjny_jpgw()

        Select Case georef_scalanie_qgis

            Case False

                If CheckJpgw = True Then

                    'tworzy plik .jpgw
                    'FileOpen(4, folderSegmentow & nazwaKwadratu & ".jpgw", OpenMode.Output)
                    Select Case rozszerzenie
                        Case "jpg"
                            FileOpen(4, folderSegmentow & nazwaKwadratu & ".jpgw", OpenMode.Output)
                        Case "png" ' & "png8" & "png24" & "png32"
                            FileOpen(4, folderSegmentow & nazwaKwadratu & ".pngw", OpenMode.Output)
                        Case "tif"
                            FileOpen(4, folderSegmentow & nazwaKwadratu & ".tifw", OpenMode.Output)
                        Case "gif"
                            FileOpen(4, folderSegmentow & nazwaKwadratu & ".gifw", OpenMode.Output)
                    End Select
                    Print(4, Form1.TextBox10.Text & Chr(13) & Chr(10) & _
                    "0" & Chr(13) & Chr(10) & _
                    "0" & Chr(13) & Chr(10) & _
                    "-" & Form1.TextBox10.Text & Chr(13) & Chr(10) & _
                   Ly & Chr(13) & Chr(10) & _
                    Px)
                    FileClose(4)

                End If
            Case True
                Ly = Val(Form3.TextBox5.Text)
                Px = Val(Form3.TextBox4.Text) + (Val(Form3.TextBox9.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))
                'tworzy plik .jpgw
                'FileOpen(4, folderSegmentow & nazwaKwadratu & ".jpgw", OpenMode.Output)
                Select Case rozszerzenie
                    Case "jpg"
                        FileOpen(4, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".jpgw", OpenMode.Output)
                    Case "png" ' & "png8" & "png24" & "png32"
                        FileOpen(4, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".pngw", OpenMode.Output)
                    Case "tif"
                        FileOpen(4, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".tifw", OpenMode.Output)
                    Case "gif"
                        FileOpen(4, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".gifw", OpenMode.Output)
                End Select
                Print(4, Form1.TextBox10.Text & Chr(13) & Chr(10) & _
                "0" & Chr(13) & Chr(10) & _
                "0" & Chr(13) & Chr(10) & _
                "-" & Form1.TextBox10.Text & Chr(13) & Chr(10) & _
               Ly & Chr(13) & Chr(10) & _
                Px)
                FileClose(4)




        End Select
    End Sub

    Public Sub plikGeoreferencyjny_tab()

        Select Case georef_scalanie_tab

            Case False

                If CheckTab = True Then

                    'tworzy plik .tab
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".tab", OpenMode.Output)

                    Print(1, "!table" & Chr(13) & Chr(10) & _
                    "!version 300" & Chr(13) & Chr(10) & _
                    "!charset WindowsLatin2" & Chr(13) & Chr(10) & _
                    "Definition Table" & Chr(13) & Chr(10) & _
                     "  File" & Chr(34) & nazwaKwadratu & "." & rozszerzenie & Chr(34) & Chr(13) & Chr(10) & _
                     "  Type " & Chr(34) & "RASTER" & Chr(34) & Chr(13) & Chr(10) & _
                   "  (" & Ly & "," & Px & ")" & " (0,0) Label " & Chr(34) & "Punkt 1" & Chr(34) & "," & Chr(13) & Chr(10) & _
                   "  (" & Py & "," & Px & ")" & " (" & Form1.TextBox9.Text & ",0) " & "Label " & Chr(34) & "Punkt 2" & Chr(34) & "," & Chr(13) & Chr(10) & _
                   "  (" & Py & "," & Lx & ")" & " (" & Form1.TextBox9.Text & "," & Form1.TextBox9.Text & ") " & "Label " & Chr(34) & "Punkt 3" & Chr(34) & "," & Chr(13) & Chr(10) & _
                   "  (" & Ly & "," & Lx & ")" & " (0," & Form1.TextBox9.Text & ") " & "Label " & Chr(34) & "Punkt 4" & Chr(34) & "," & Chr(13) & Chr(10) & _
                   "  CoordSys Earth Projection 8, 33, 7, 19, 0, 0.9993, 500000, -5300000" & Chr(13) & Chr(10) & _
                   "")

                    FileClose(1)

                End If
            Case True

                Lx = (Val(Form3.TextBox4.Text))
                Ly = (Val(Form3.TextBox5.Text))
                Px = Val(Form3.TextBox4.Text) + (Val(Form3.TextBox9.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))
                Py = Val(Form3.TextBox5.Text) + (Val(Form3.TextBox8.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))

                FileOpen(1, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".tab", OpenMode.Output)
                Print(1, "!table" & Chr(13) & Chr(10) & _
                   "!version 300" & Chr(13) & Chr(10) & _
                   "!charset WindowsLatin2" & Chr(13) & Chr(10) & _
                   "Definition Table" & Chr(13) & Chr(10) & _
                    "  File" & Chr(34) & nazwaKwadratu & "." & format & Chr(34) & Chr(13) & Chr(10) & _
                    "  Type " & Chr(34) & "RASTER" & Chr(34) & Chr(13) & Chr(10) & _
                  "  (" & Ly & "," & Px & ")" & " (0,0) Label " & Chr(34) & "Punkt 1" & Chr(34) & "," & Chr(13) & Chr(10) & _
                  "  (" & Py & "," & Px & ")" & " (" & Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text) & ",0) " & "Label " & Chr(34) & "Punkt 2" & Chr(34) & "," & Chr(13) & Chr(10) & _
                  "  (" & Py & "," & Lx & ")" & " (" & Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text) & "," & Val(Form3.TextBox7.Text) * Val(Form3.TextBox9.Text) & ") " & "Label " & Chr(34) & "Punkt 3" & Chr(34) & "," & Chr(13) & Chr(10) & _
                  "  (" & Ly & "," & Lx & ")" & " (0," & Val(Form3.TextBox7.Text) * Val(Form3.TextBox9.Text) & ") " & "Label " & Chr(34) & "Punkt 4" & Chr(34) & "," & Chr(13) & Chr(10) & _
                  "  CoordSys Earth Projection 8, 33, 7, 19, 0, 0.9993, 500000, -5300000" & Chr(13) & Chr(10) & _
                  "")

                FileClose(1)
               
        End Select
    End Sub



    Function Dodaj_zera(ByVal X As String, ByVal L As Integer) As String
        'funkcja dodająca odpowiednią ilość zer do ciagu znaków
        'X - ciąg znaków; L - porządana ilość znaków w ciągu

        Dim dlugosc As Integer  'ilość znaków we współrzędnych
        Dim brak As Integer     'ile znaków brakuje

        dlugosc = Len(X)

        If dlugosc < L Then
            brak = L - dlugosc
            For i = 1 To brak
                X = X & "0"
            Next
        End If

        Dodaj_zera = X

    End Function

    Public Sub plikGeoreferencyjny_map()

        Select Case georef_scalanie_map

            Case False
                If CheckMap = True Then

                    Dim MMPLL1sz As String  'lewy górny
                    Dim MMPLL1dl As String
                    Dim MMPLL2sz As String  'prawy górny
                    Dim MMPLL2dl As String
                    Dim MMPLL3sz As String  'prawy dolny
                    Dim MMPLL3dl As String
                    Dim MMPLL4sz As String  'lewy dolny
                    Dim MMPLL4dl As String

                    MMPLL1sz = Round(SzerokoscWgs_z1992(Px, Ly), 8)
                    MMPLL1dl = Round(DlugoscWgs_z1992(Px, Ly), 8)
                    MMPLL2sz = Round(SzerokoscWgs_z1992(Px, Py), 8)
                    MMPLL2dl = Round(DlugoscWgs_z1992(Px, Py), 8)
                    MMPLL3sz = Round(SzerokoscWgs_z1992(Lx, Py), 8)
                    MMPLL3dl = Round(DlugoscWgs_z1992(Lx, Py), 8)
                    MMPLL4sz = Round(SzerokoscWgs_z1992(Lx, Ly), 8)
                    MMPLL4dl = Round(DlugoscWgs_z1992(Lx, Ly), 8)

                    'przelicza na stopnie, minuty, sekundy i ułamki sekund
                    Dim Point01N As String = StMinSek(MMPLL1sz)
                    Dim Point01E As String = StMinSek(MMPLL1dl)
                    Dim Point02N As String = StMinSek(MMPLL2sz)
                    Dim Point02E As String = StMinSek(MMPLL2dl)
                    Dim Point03N As String = StMinSek(MMPLL3sz)
                    Dim Point03E As String = StMinSek(MMPLL3dl)
                    Dim Point04N As String = StMinSek(MMPLL4sz)
                    Dim Point04E As String = StMinSek(MMPLL4dl)

                    'usuwa przecinki i zamienia je na kropki
                    Dim MMPLL1szDot As String = Replace(MMPLL1sz, ",", ".")     'lewy górny
                    Dim MMPLL1dlDot As String = Replace(MMPLL1dl, ",", ".")
                    Dim MMPLL2szDot As String = Replace(MMPLL2sz, ",", ".")     'prawy górny
                    Dim MMPLL2dlDot As String = Replace(MMPLL2dl, ",", ".")
                    Dim MMPLL3szDot As String = Replace(MMPLL3sz, ",", ".")     'prawy dolny
                    Dim MMPLL3dlDot As String = Replace(MMPLL3dl, ",", ".")
                    Dim MMPLL4szDot As String = Replace(MMPLL4sz, ",", ".")     'lewy dolny
                    Dim MMPLL4dlDot As String = Replace(MMPLL4dl, ",", ".")


                    'tworzy plik .map
                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".map", OpenMode.Output)
                    Print(1, "OziExplorer Map Data File Version 2.2" & Chr(13) & Chr(10) & _
                         nazwaKwadratu & "." & rozszerzenie & Chr(13) & Chr(10) & _
                         folderSegmentow & nazwaKwadratu & "." & rozszerzenie & Chr(13) & Chr(10) & _
                         "1 ,Map Code," & Chr(13) & Chr(10) & _
                         "WGS 84,,0.0000,0.0000," & Chr(13) & Chr(10) & _
                         "Reserved 1" & Chr(13) & Chr(10) & _
                         "Reserved 2" & Chr(13) & Chr(10) & _
                         "Magnetic Variation,,,E" & Chr(13) & Chr(10) & _
                         "Map Projection,Transverse Mercator,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No" & Chr(13) & Chr(10) & _
                         "Point01,xy,    0,    0,in, deg,  " & Point01N & ",N,  " & Point01E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point02,xy, " & Form1.TextBox9.Text & ",    0,in, deg,  " & Point02N & ",N,  " & Point02E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point03,xy, " & Form1.TextBox9.Text & ", " & Form1.TextBox9.Text & ",in, deg,  " & Point03N & ",N,  " & Point03E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point04,xy,    0, " & Form1.TextBox9.Text & ",in, deg,  " & Point04N & ",N,  " & Point04E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point05,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point06,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point07,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point08,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point09,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point10,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point11,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point12,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point13,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point14,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point15,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point16,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point17,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point18,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point19,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point20,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point21,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point22,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point23,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point24,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point25,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point26,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point27,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point28,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point29,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Point30,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                         "Projection Setup,     0.000000000,    19.000000000,     0.999300000,       500000.00,     -5300000.00,,,," & Chr(13) & Chr(10) & _
                         "Map Feature = MF ; Map Comment = MC     These follow if they exist" & Chr(13) & Chr(10) & _
                         "Track File = TF      These follow if they exist" & Chr(13) & Chr(10) & _
                         "Moving Map Parameters = MM?    These follow if they exist" & Chr(13) & Chr(10) & _
                         "MM0,Yes" & Chr(13) & Chr(10) & _
                         "MMPNUM,4" & Chr(13) & Chr(10) & _
                         "MMPXY,1,0,0" & Chr(13) & Chr(10) & _
                         "MMPXY,2," & Form1.TextBox9.Text & ",0" & Chr(13) & Chr(10) & _
                         "MMPXY,3," & Form1.TextBox9.Text & "," & Form1.TextBox9.Text & Chr(13) & Chr(10) & _
                         "MMPXY,4,0," & Form1.TextBox9.Text & Chr(13) & Chr(10) & _
                         "MMPLL,1,  " & MMPLL1dlDot & ",  " & MMPLL1szDot & Chr(13) & Chr(10) & _
                         "MMPLL,2,  " & MMPLL2dlDot & ",  " & MMPLL2szDot & Chr(13) & Chr(10) & _
                         "MMPLL,3,  " & MMPLL3dlDot & ",  " & MMPLL3szDot & Chr(13) & Chr(10) & _
                         "MMPLL,4,   " & MMPLL4dlDot & ",  " & MMPLL4szDot & Chr(13) & Chr(10) & _
                         "MM1B,     " & Form1.TextBox10.Text & Chr(13) & Chr(10) & _
                         "MOP,Map Open Position,0,0" & Chr(13) & Chr(10) & _
                         "IWH,Map Image Width/Height," & Form1.TextBox9.Text & "," & Form1.TextBox9.Text)
                    FileClose(1)

                End If

            Case True

                Lx = (Val(Form3.TextBox4.Text))
                Ly = (Val(Form3.TextBox5.Text))
                Px = Val(Form3.TextBox4.Text) + (Val(Form3.TextBox9.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))
                Py = Val(Form3.TextBox5.Text) + (Val(Form3.TextBox8.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))


                Dim MMPLL1sz As String  'lewy górny
                Dim MMPLL1dl As String
                Dim MMPLL2sz As String  'prawy górny
                Dim MMPLL2dl As String
                Dim MMPLL3sz As String  'prawy dolny
                Dim MMPLL3dl As String
                Dim MMPLL4sz As String  'lewy dolny
                Dim MMPLL4dl As String

                MMPLL1sz = Round(SzerokoscWgs_z1992(Px, Ly), 8)
                MMPLL1dl = Round(DlugoscWgs_z1992(Px, Ly), 8)
                MMPLL2sz = Round(SzerokoscWgs_z1992(Px, Py), 8)
                MMPLL2dl = Round(DlugoscWgs_z1992(Px, Py), 8)
                MMPLL3sz = Round(SzerokoscWgs_z1992(Lx, Py), 8)
                MMPLL3dl = Round(DlugoscWgs_z1992(Lx, Py), 8)
                MMPLL4sz = Round(SzerokoscWgs_z1992(Lx, Ly), 8)
                MMPLL4dl = Round(DlugoscWgs_z1992(Lx, Ly), 8)

                'przelicza na stopnie, minuty, sekundy i ułamki sekund
                Dim Point01N As String = StMinSek(MMPLL1sz)
                Dim Point01E As String = StMinSek(MMPLL1dl)
                Dim Point02N As String = StMinSek(MMPLL2sz)
                Dim Point02E As String = StMinSek(MMPLL2dl)
                Dim Point03N As String = StMinSek(MMPLL3sz)
                Dim Point03E As String = StMinSek(MMPLL3dl)
                Dim Point04N As String = StMinSek(MMPLL4sz)
                Dim Point04E As String = StMinSek(MMPLL4dl)

                'usuwa przecinki i zamienia je na kropki
                Dim MMPLL1szDot As String = Replace(MMPLL1sz, ",", ".")     'lewy górny
                Dim MMPLL1dlDot As String = Replace(MMPLL1dl, ",", ".")
                Dim MMPLL2szDot As String = Replace(MMPLL2sz, ",", ".")     'prawy górny
                Dim MMPLL2dlDot As String = Replace(MMPLL2dl, ",", ".")
                Dim MMPLL3szDot As String = Replace(MMPLL3sz, ",", ".")     'prawy dolny
                Dim MMPLL3dlDot As String = Replace(MMPLL3dl, ",", ".")
                Dim MMPLL4szDot As String = Replace(MMPLL4sz, ",", ".")     'lewy dolny
                Dim MMPLL4dlDot As String = Replace(MMPLL4dl, ",", ".")


                'tworzy plik .map
                FileOpen(1, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".map", OpenMode.Output)
                Print(1, "OziExplorer Map Data File Version 2.2" & Chr(13) & Chr(10) & _
                     "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & "." & rozszerzenie & Chr(13) & Chr(10) & _
                     folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & "." & rozszerzenie & Chr(13) & Chr(10) & _
                     "1 ,Map Code," & Chr(13) & Chr(10) & _
                     "WGS 84,,0.0000,0.0000," & Chr(13) & Chr(10) & _
                     "Reserved 1" & Chr(13) & Chr(10) & _
                     "Reserved 2" & Chr(13) & Chr(10) & _
                     "Magnetic Variation,,,E" & Chr(13) & Chr(10) & _
                     "Map Projection,Transverse Mercator,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No" & Chr(13) & Chr(10) & _
                     "Point01,xy,    0,    0,in, deg,  " & Point01N & ",N,  " & Point01E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point02,xy, " & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text)) & ",    0,in, deg,  " & Point02N & ",N,  " & Point02E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point03,xy, " & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text)) & ", " & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox9.Text)) & ",in, deg,  " & Point03N & ",N,  " & Point03E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point04,xy,    0, " & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox9.Text)) & ",in, deg,  " & Point04N & ",N,  " & Point04E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point05,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point06,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point07,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point08,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point09,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point10,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point11,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point12,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point13,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point14,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point15,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point16,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point17,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point18,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point19,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point20,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point21,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point22,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point23,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point24,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point25,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point26,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point27,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point28,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point29,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Point30,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                     "Projection Setup,     0.000000000,    19.000000000,     0.999300000,       500000.00,     -5300000.00,,,," & Chr(13) & Chr(10) & _
                     "Map Feature = MF ; Map Comment = MC     These follow if they exist" & Chr(13) & Chr(10) & _
                     "Track File = TF      These follow if they exist" & Chr(13) & Chr(10) & _
                     "Moving Map Parameters = MM?    These follow if they exist" & Chr(13) & Chr(10) & _
                     "MM0,Yes" & Chr(13) & Chr(10) & _
                     "MMPNUM,4" & Chr(13) & Chr(10) & _
                     "MMPXY,1,0,0" & Chr(13) & Chr(10) & _
                     "MMPXY,2," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text)) & ",0" & Chr(13) & Chr(10) & _
                     "MMPXY,3," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text)) & "," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox9.Text)) & Chr(13) & Chr(10) & _
                     "MMPXY,4,0," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox9.Text)) & Chr(13) & Chr(10) & _
                     "MMPLL,1,  " & MMPLL1dlDot & ",  " & MMPLL1szDot & Chr(13) & Chr(10) & _
                     "MMPLL,2,  " & MMPLL2dlDot & ",  " & MMPLL2szDot & Chr(13) & Chr(10) & _
                     "MMPLL,3,  " & MMPLL3dlDot & ",  " & MMPLL3szDot & Chr(13) & Chr(10) & _
                     "MMPLL,4,   " & MMPLL4dlDot & ",  " & MMPLL4szDot & Chr(13) & Chr(10) & _
                     "MM1B,     " & Form3.TextBox7.Text & Chr(13) & Chr(10) & _
                     "MOP,Map Open Position,0,0" & Chr(13) & Chr(10) & _
                     "IWH,Map Image Width/Height," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox8.Text)) & "," & (Val(Form3.TextBox6.Text) * Val(Form3.TextBox9.Text)))
                FileClose(1)

        End Select
       
    End Sub


    Public Sub plikGeoreferencyjny_kml()
        Select georef_scalanie_kml

            Case False


                If CheckKml = True Then

                    'Dim bok As String = Form1.TextBox9.Text & ".000000000000000"

                    Dim LGsz As String  'lewy górny
                    Dim LGdl As String
                    Dim PGsz As String  'prawy górny
                    Dim PGdl As String
                    Dim PDsz As String  'prawy dolny
                    Dim PDdl As String
                    Dim LDsz As String  'lewy dolny
                    Dim LDdl As String

                    LGsz = Round(SzerokoscWgs_z1992(Px, Ly), 15)
                    LGdl = Round(DlugoscWgs_z1992(Px, Ly), 15)
                    PGsz = Round(SzerokoscWgs_z1992(Px, Py), 15)
                    PGdl = Round(DlugoscWgs_z1992(Px, Py), 15)
                    PDsz = Round(SzerokoscWgs_z1992(Lx, Py), 15)
                    PDdl = Round(DlugoscWgs_z1992(Lx, Py), 15)
                    LDsz = Round(SzerokoscWgs_z1992(Lx, Ly), 15)
                    LDdl = Round(DlugoscWgs_z1992(Lx, Ly), 15)

                    'usuwa przecinki i zamienia je na kropki
                    Dim LGszDot As String = Replace(LGsz, ",", ".")     'lewy górny
                    Dim LGdlDot As String = Replace(LGdl, ",", ".")
                    Dim PGszDot As String = Replace(PGsz, ",", ".")     'prawy górny
                    Dim PGdlDot As String = Replace(PGdl, ",", ".")
                    Dim PDszDot As String = Replace(PDsz, ",", ".")     'prawy dolny
                    Dim PDdlDot As String = Replace(PDdl, ",", ".")
                    Dim LDszDot As String = Replace(LDsz, ",", ".")     'lewy dolny
                    Dim LDdlDot As String = Replace(LDdl, ",", ".")

                    'sprawdza ilość znaków w zmiennej i w razie potrzeby dodaje zera na końcu
                    LGszDot = Dodaj_zera(LGszDot, 18)
                    LGdlDot = Dodaj_zera(LGdlDot, 18)
                    PGszDot = Dodaj_zera(PGszDot, 18)
                    PGdlDot = Dodaj_zera(PGdlDot, 18)
                    PDszDot = Dodaj_zera(PDszDot, 18)
                    PDdlDot = Dodaj_zera(PDdlDot, 18)
                    LDszDot = Dodaj_zera(LDszDot, 18)
                    LDdlDot = Dodaj_zera(LDdlDot, 18)




                    FileOpen(1, folderSegmentow & nazwaKwadratu & ".kml", OpenMode.Output)



                    Print(1, "<?xml version=" & Chr(34) & "1.0" & Chr(34) & " encoding=" & Chr(34) & "UTF-8" & Chr(34) & "?>" & Chr(13) & Chr(10) & _
                    "<kml xmlns=" & Chr(34) & "http://www.opengis.net/kml/2.2" & Chr(34) & " xmlns:gx=" & Chr(34) & "http://www.google.com/kml/ext/2.2" & Chr(34) & " xmlns:kml=" & Chr(34) & "http://www.opengis.net/kml/2.2" & Chr(34) & " xmlns:atom=" & Chr(34) & "http://www.w3.org/2005/Atom" & Chr(34) & ">" & Chr(13) & Chr(10) & _
        "<GroundOverlay>" & Chr(13) & Chr(10) & _
                    "<name>" & nazwaKwadratu & "</name>" & Chr(13) & Chr(10) & _
                    "<Icon>" & Chr(13) & Chr(10) & _
                    "<href>" & nazwaKwadratu & "." & rozszerzenie & "</href>" & Chr(13) & Chr(10) & _
                    "<viewBoundScale>" & Form1.TextBox10.Text & "</viewBoundScale>" & Chr(13) & Chr(10) & _
                    "</Icon>" & Chr(13) & Chr(10) & _
                    "<LatLonBox>" & Chr(13) & Chr(10) & _
                    "<north>" & PGszDot & "</north>" & Chr(13) & Chr(10) & _
                    "<south>" & PDszDot & "</south>" & Chr(13) & Chr(10) & _
                    "<east>" & PGdlDot & "</east>" & Chr(13) & Chr(10) & _
                    "<west>" & LGdlDot & "</west>" & Chr(13) & Chr(10) & _
                    "</LatLonBox>" & Chr(13) & Chr(10) & _
                    "</GroundOverlay>" & Chr(13) & Chr(10) & _
                    "</kml>")

                    FileClose(1)

                End If

            Case True


                Lx = (Val(Form3.TextBox4.Text))
                Ly = (Val(Form3.TextBox5.Text))
                Px = Val(Form3.TextBox4.Text) + (Val(Form3.TextBox9.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))
                Py = Val(Form3.TextBox5.Text) + (Val(Form3.TextBox8.Text) * Val(Form3.TextBox7.Text) * Val(Form3.TextBox6.Text))

                'Dim bok As String = Form1.TextBox9.Text & ".000000000000000"

                Dim LGsz As String  'lewy górny
                Dim LGdl As String
                Dim PGsz As String  'prawy górny
                Dim PGdl As String
                Dim PDsz As String  'prawy dolny
                Dim PDdl As String
                Dim LDsz As String  'lewy dolny
                Dim LDdl As String

                LGsz = Round(SzerokoscWgs_z1992(Px, Ly), 15)
                LGdl = Round(DlugoscWgs_z1992(Px, Ly), 15)
                PGsz = Round(SzerokoscWgs_z1992(Px, Py), 15)
                PGdl = Round(DlugoscWgs_z1992(Px, Py), 15)
                PDsz = Round(SzerokoscWgs_z1992(Lx, Py), 15)
                PDdl = Round(DlugoscWgs_z1992(Lx, Py), 15)
                LDsz = Round(SzerokoscWgs_z1992(Lx, Ly), 15)
                LDdl = Round(DlugoscWgs_z1992(Lx, Ly), 15)

                'usuwa przecinki i zamienia je na kropki
                Dim LGszDot As String = Replace(LGsz, ",", ".")     'lewy górny
                Dim LGdlDot As String = Replace(LGdl, ",", ".")
                Dim PGszDot As String = Replace(PGsz, ",", ".")     'prawy górny
                Dim PGdlDot As String = Replace(PGdl, ",", ".")
                Dim PDszDot As String = Replace(PDsz, ",", ".")     'prawy dolny
                Dim PDdlDot As String = Replace(PDdl, ",", ".")
                Dim LDszDot As String = Replace(LDsz, ",", ".")     'lewy dolny
                Dim LDdlDot As String = Replace(LDdl, ",", ".")

                'sprawdza ilość znaków w zmiennej i w razie potrzeby dodaje zera na końcu
                LGszDot = Dodaj_zera(LGszDot, 18)
                LGdlDot = Dodaj_zera(LGdlDot, 18)
                PGszDot = Dodaj_zera(PGszDot, 18)
                PGdlDot = Dodaj_zera(PGdlDot, 18)
                PDszDot = Dodaj_zera(PDszDot, 18)
                PDdlDot = Dodaj_zera(PDdlDot, 18)
                LDszDot = Dodaj_zera(LDszDot, 18)
                LDdlDot = Dodaj_zera(LDdlDot, 18)


                FileOpen(1, folderSegmentow & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & ".kml", OpenMode.Output)


                Print(1, "<?xml version=" & Chr(34) & "1.0" & Chr(34) & " encoding=" & Chr(34) & "UTF-8" & Chr(34) & "?>" & Chr(13) & Chr(10) & _
                "<kml xmlns=" & Chr(34) & "http://www.opengis.net/kml/2.2" & Chr(34) & " xmlns:gx=" & Chr(34) & "http://www.google.com/kml/ext/2.2" & Chr(34) & " xmlns:kml=" & Chr(34) & "http://www.opengis.net/kml/2.2" & Chr(34) & " xmlns:atom=" & Chr(34) & "http://www.w3.org/2005/Atom" & Chr(34) & ">" & Chr(13) & Chr(10) & _
    "<GroundOverlay>" & Chr(13) & Chr(10) & _
                "<name>" & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & "</name>" & Chr(13) & Chr(10) & _
                "<Icon>" & Chr(13) & Chr(10) & _
                "<href>" & "_scalone_segmenty_" & Form3.TextBox8.Text & "x" & Form3.TextBox9.Text & "." & rozszerzenie & "</href>" & Chr(13) & Chr(10) & _
                "<viewBoundScale>" & Form1.TextBox10.Text & "</viewBoundScale>" & Chr(13) & Chr(10) & _
                "</Icon>" & Chr(13) & Chr(10) & _
                "<LatLonBox>" & Chr(13) & Chr(10) & _
                "<north>" & PGszDot & "</north>" & Chr(13) & Chr(10) & _
                "<south>" & PDszDot & "</south>" & Chr(13) & Chr(10) & _
                "<east>" & PGdlDot & "</east>" & Chr(13) & Chr(10) & _
                "<west>" & LGdlDot & "</west>" & Chr(13) & Chr(10) & _
                "</LatLonBox>" & Chr(13) & Chr(10) & _
                "</GroundOverlay>" & Chr(13) & Chr(10) & _
                "</kml>")

                FileClose(1)

        End Select
    End Sub
    Public Sub plikError()

        'zapisuje w folderze z segmentami plik error.txt
        FileOpen(1, folderSegmentow & "error.txt", OpenMode.Append)
        PrintLine(1, nazwaKwadratu)             'nazwa obrazka
        PrintLine(1, strUrl)                    'link do obrazka


        FileClose(1)

    End Sub
    Public Sub plikZkoordynatami()

        'zapisuje w folderze z segmentami plik koordynaty.txt
        FileOpen(1, folderSegmentow & "koordynaty.txt", OpenMode.Append)
        PrintLine(1, nazwaKwadratu)             'nazwa obrazka
        PrintLine(1, strUrl)                    'link do obrazka
        PrintLine(1, Ly)        'Ly
        PrintLine(1, Lx)        'Lx
        PrintLine(1, Py)        'Py
        PrintLine(1, Px)        'Px

        FileClose(1)

    End Sub
    Public Sub plikTB_Set()
        'zapisuje w folderze z segmentami plik .set
        If CheckTB = True Then

            FileOpen(1, folderTB & wspolnaNazwaKwadratu & ".set", OpenMode.Append)
            PrintLine(1, nazwaKwadratu & "." & rozszerzenie)

            FileClose(1)

        End If
    End Sub


    Public Sub plikTB_map()

        If CheckTB = True Then

            Px = Val(Form1.TextBox1.Text) + ((Val(Form1.TextBox12.Text) * Val(Form1.TextBox9.Text)) * Val(Form1.TextBox10.Text))
            Py = Val(Form1.TextBox2.Text) + ((Val(Form1.TextBox11.Text) * Val(Form1.TextBox9.Text)) * Val(Form1.TextBox10.Text))
            Lx = Form1.TextBox1.Text
            Ly = Form1.TextBox2.Text

            Dim MMPLL1sz As String  'lewy górny
            Dim MMPLL1dl As String
            Dim MMPLL2sz As String  'prawy górny
            Dim MMPLL2dl As String
            Dim MMPLL3sz As String  'prawy dolny
            Dim MMPLL3dl As String
            Dim MMPLL4sz As String  'lewy dolny
            Dim MMPLL4dl As String

            MMPLL1sz = Round(SzerokoscWgs_z1992(Px, Ly), 6)
            MMPLL1dl = Round(DlugoscWgs_z1992(Px, Ly), 6)
            MMPLL2sz = Round(SzerokoscWgs_z1992(Px, Py), 6)
            MMPLL2dl = Round(DlugoscWgs_z1992(Px, Py), 6)
            MMPLL3sz = Round(SzerokoscWgs_z1992(Lx, Py), 6)
            MMPLL3dl = Round(DlugoscWgs_z1992(Lx, Py), 6)
            MMPLL4sz = Round(SzerokoscWgs_z1992(Lx, Ly), 6)
            MMPLL4dl = Round(DlugoscWgs_z1992(Lx, Ly), 6)

            'przelicza na stopnie, minuty, sekundy i ułamki sekund
            Dim Point01N As String = StMinSek(MMPLL1sz)
            Dim Point01E As String = StMinSek(MMPLL1dl)
            Dim Point02N As String = StMinSek(MMPLL2sz)
            Dim Point02E As String = StMinSek(MMPLL2dl)
            Dim Point03N As String = StMinSek(MMPLL3sz)
            Dim Point03E As String = StMinSek(MMPLL3dl)
            Dim Point04N As String = StMinSek(MMPLL4sz)
            Dim Point04E As String = StMinSek(MMPLL4dl)

            'usuwa przecinki i zamienia je na kropki
            Dim MMPLL1szDot As String = Replace(MMPLL1sz, ",", ".")     'lewy górny
            Dim MMPLL1dlDot As String = Replace(MMPLL1dl, ",", ".")
            Dim MMPLL2szDot As String = Replace(MMPLL2sz, ",", ".")     'prawy górny
            Dim MMPLL2dlDot As String = Replace(MMPLL2dl, ",", ".")
            Dim MMPLL3szDot As String = Replace(MMPLL3sz, ",", ".")     'prawy dolny
            Dim MMPLL3dlDot As String = Replace(MMPLL3dl, ",", ".")
            Dim MMPLL4szDot As String = Replace(MMPLL4sz, ",", ".")     'lewy dolny
            Dim MMPLL4dlDot As String = Replace(MMPLL4dl, ",", ".")


            'tworzy plik .map
            FileOpen(1, folderTB & wspolnaNazwaKwadratu & ".map", OpenMode.Output)
            Print(1, "OziExplorer Map Data File Version 2.2" & Chr(13) & Chr(10) & _
                 wspolnaNazwaKwadratu & "." & rozszerzenie & Chr(13) & Chr(10) & _
                 folderSegmentow & wspolnaNazwaKwadratu & "." & rozszerzenie & Chr(13) & Chr(10) & _
                 "1 ,Map Code," & Chr(13) & Chr(10) & _
                 "WGS 84,,0.0000,0.0000," & Chr(13) & Chr(10) & _
                 "Reserved 1" & Chr(13) & Chr(10) & _
                 "Reserved 2" & Chr(13) & Chr(10) & _
                 "Magnetic Variation,,,E" & Chr(13) & Chr(10) & _
                 "Map Projection,Transverse Mercator,PolyCal,No,AutoCalOnly,No,BSBUseWPX,No" & Chr(13) & Chr(10) & _
                 "Point01,xy,    0,    0,in, deg,  " & Point01N & ",N,  " & Point01E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point02,xy, " & Form1.TextBox9.Text * Form1.TextBox11.Text & ",    0,in, deg,  " & Point02N & ",N,  " & Point02E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point03,xy, " & Form1.TextBox9.Text * Form1.TextBox11.Text & ", " & Form1.TextBox9.Text * Form1.TextBox12.Text & ",in, deg,  " & Point03N & ",N,  " & Point03E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point04,xy,    0, " & Form1.TextBox9.Text * Form1.TextBox12.Text & ",in, deg,  " & Point04N & ",N,  " & Point04E & ",E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point05,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point06,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point07,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point08,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point09,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point10,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point11,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point12,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point13,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point14,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point15,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point16,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point17,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point18,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point19,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point20,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point21,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point22,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point23,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point24,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point25,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point26,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point27,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point28,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point29,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Point30,xy,     ,     ,in, deg,    ,        ,N,    ,        ,E, grid,   ,           ,           ,N" & Chr(13) & Chr(10) & _
                 "Projection Setup,     0.000000000,    19.000000000,     0.999300000,       500000.00,     -5300000.00,,,," & Chr(13) & Chr(10) & _
                 "Map Feature = MF ; Map Comment = MC     These follow if they exist" & Chr(13) & Chr(10) & _
                 "Track File = TF      These follow if they exist" & Chr(13) & Chr(10) & _
                 "Moving Map Parameters = MM?    These follow if they exist" & Chr(13) & Chr(10) & _
                 "MM0,Yes" & Chr(13) & Chr(10) & _
                 "MMPNUM,4" & Chr(13) & Chr(10) & _
                 "MMPXY,1,0,0" & Chr(13) & Chr(10) & _
                 "MMPXY,2," & Form1.TextBox9.Text * Form1.TextBox11.Text & ",0" & Chr(13) & Chr(10) & _
                 "MMPXY,3," & Form1.TextBox9.Text * Form1.TextBox11.Text & "," & Form1.TextBox9.Text * Form1.TextBox12.Text & Chr(13) & Chr(10) & _
                 "MMPXY,4,0," & Form1.TextBox9.Text * Form1.TextBox12.Text & Chr(13) & Chr(10) & _
                 "MMPLL,1,  " & MMPLL1dlDot & ",  " & MMPLL1szDot & Chr(13) & Chr(10) & _
                 "MMPLL,2,  " & MMPLL2dlDot & ",  " & MMPLL2szDot & Chr(13) & Chr(10) & _
                 "MMPLL,3,  " & MMPLL3dlDot & ",  " & MMPLL3szDot & Chr(13) & Chr(10) & _
                 "MMPLL,4,   " & MMPLL4dlDot & ",  " & MMPLL4szDot & Chr(13) & Chr(10) & _
                 "MM1B,     " & Form1.TextBox10.Text & Chr(13) & Chr(10) & _
                 "MOP,Map Open Position,0,0" & Chr(13) & Chr(10) & _
                 "IWH,Map Image Width/Height," & Form1.TextBox9.Text * Form1.TextBox11.Text & "," & Form1.TextBox9.Text * Form1.TextBox12.Text)
            FileClose(1)

        End If

        '       "Point01,xy,0," & TextBox9.Text & ",in, deg,,,N,,,E, grid,," & Ly & "," & Lx & ", N" & Chr(13) & Chr(10) & _
        '       "Point02,xy," & TextBox9.Text & ",0,in, deg,,,N,,,E, grid,," & Py & "," & Px & ", N" & Chr(13) & Chr(10) & _
    End Sub
    Function StMinSek(ByVal MMPLL As Double)
        'zamienia stopnie i dziesiąte stopni na stopnie, minuty i dziesiąte minut
        Dim st As String
        Dim min As String

        st = System.Math.Floor(MMPLL)       'pozostawia całości - stopnie
        min = (MMPLL - st) * 60             'od całości odejmuje ułamek i przelicza na minuty
        min = Round(min, 6)                 'zaokrągla je do 8 miejsc po przecinku
        min = Replace(min, ",", ".")        'zamienia przecinek na kropkę

        StMinSek = st & ", " & min
    End Function
    Function Round(ByVal X As Double, ByVal DP As Integer) As Double
        'funkcja zaokroąglająca liczbę X (double) do DP miejsc po przecinku
        Round = Int((X * 10 ^ DP) + 0.5) / 10 ^ DP

    End Function

    Function SzerokoscWgs_z1992(ByVal X, ByVal Y)
        Const mo = 0.9993
        Const Pi = 3.14159265358979
        Const Ro = 6367449.14577
        Const b2 = -0.000837732168164
        Const b4 = -0.000000059058696
        Const b6 = -0.000000000167349
        Const b8 = -0.000000000000217
        Const c2 = 0.003356551485597
        Const c4 = 0.000006571873148
        Const c6 = 0.000000017646564
        Const c8 = 0.000000000054005
        'Const Lo = 19
        Const FE = 500000
        Const FN = -5300000
        Dim Xgk
        Dim Ygk
        Dim u
        Dim v
        Dim Xmer
        Dim Ymer
        Dim wsw
        Dim fi
        Dim Brad
        Xgk = (X - FN) / mo
        Ygk = (Y - FE) / mo
        u = Xgk / Ro
        v = Ygk / Ro
        Xmer = u + (b2 * System.Math.Sin(2 * u) * (System.Math.Exp(2 * v) + System.Math.Exp(-2 * v)) / 2 + b4 * System.Math.Sin(4 * u) * (System.Math.Exp(4 * v) + System.Math.Exp(-4 * v)) / 2 + b6 * System.Math.Sin(6 * u) * (System.Math.Exp(6 * v) + System.Math.Exp(-6 * v)) / 2 + (b8 * System.Math.Sin(8 * u) * (System.Math.Exp(8 * v) + System.Math.Exp(-8 * v)) / 2))
        Ymer = v + (b2 * System.Math.Cos(2 * u) * (System.Math.Exp(2 * v) - System.Math.Exp(-2 * v)) / 2 + b4 * System.Math.Cos(4 * u) * (System.Math.Exp(4 * v) - System.Math.Exp(-4 * v)) / 2 + b6 * System.Math.Cos(6 * u) * (System.Math.Exp(6 * v) - System.Math.Exp(-6 * v)) / 2 + (b8 * System.Math.Cos(8 * u) * (System.Math.Exp(8 * v) - System.Math.Exp(-8 * v)) / 2))
        wsw = 2 * System.Math.Atan(System.Math.Exp(Ymer)) - Pi / 2
        fi = System.Math.Atan((System.Math.Cos(wsw) * System.Math.Sin(Xmer)) / System.Math.Sqrt(-(System.Math.Cos(wsw) * System.Math.Sin(Xmer)) * (System.Math.Cos(wsw) * System.Math.Sin(Xmer)) + 1))
        Brad = fi + c2 * System.Math.Sin(2 * fi) + c4 * System.Math.Sin(4 * fi) + c6 * System.Math.Sin(6 * fi) + c8 * System.Math.Sin(8 * fi) + 0.0000000008
        SzerokoscWgs_z1992 = Brad * 180 / Pi
    End Function
    Function DlugoscWgs_z1992(ByVal X, ByVal Y)
        Const mo = 0.9993
        Const Pi = 3.14159265358979
        Const Ro = 6367449.14577
        Const b2 = -0.000837732168164
        Const b4 = -0.000000059058696
        Const b6 = -0.000000000167349
        Const b8 = -0.000000000000217
        Const Lo = 19
        Const FE = 500000
        Const FN = -5300000
        Dim Xgk
        Dim Ygk
        Dim u
        Dim v
        Dim Xmer
        Dim Ymer
        Dim wsw
        'Dim fi
        'Dim Brad
        Dim deltaL
        Xgk = (X - FN) / mo
        Ygk = (Y - FE) / mo
        u = Xgk / Ro
        v = Ygk / Ro
        Xmer = u + (b2 * System.Math.Sin(2 * u) * (System.Math.Exp(2 * v) + System.Math.Exp(-2 * v)) / 2 + b4 * System.Math.Sin(4 * u) * (System.Math.Exp(4 * v) + System.Math.Exp(-4 * v)) / 2 + b6 * System.Math.Sin(6 * u) * (System.Math.Exp(6 * v) + System.Math.Exp(-6 * v)) / 2 + (b8 * System.Math.Sin(8 * u) * (System.Math.Exp(8 * v) + System.Math.Exp(-8 * v)) / 2))
        Ymer = v + (b2 * System.Math.Cos(2 * u) * (System.Math.Exp(2 * v) - System.Math.Exp(-2 * v)) / 2 + b4 * System.Math.Cos(4 * u) * (System.Math.Exp(4 * v) - System.Math.Exp(-4 * v)) / 2 + b6 * System.Math.Cos(6 * u) * (System.Math.Exp(6 * v) - System.Math.Exp(-6 * v)) / 2 + (b8 * System.Math.Cos(8 * u) * (System.Math.Exp(8 * v) - System.Math.Exp(-8 * v)) / 2))
        wsw = 2 * System.Math.Atan(System.Math.Exp(Ymer)) - Pi / 2
        deltaL = System.Math.Atan((System.Math.Tan(wsw)) / System.Math.Cos(Xmer))
        DlugoscWgs_z1992 = Lo + deltaL * 180 / Pi
    End Function
    Public Sub TBfoldery()


        TBmax = TBbok * TBseg
        dlY = Form1.TextBox11.Text * TBbok
        dlX = Form1.TextBox12.Text * TBbok

        folY = Math.Ceiling(dlY / TBmax)
        folX = Math.Ceiling(dlX / TBmax)


        If Directory.Exists(folderSegmentow & wspolnaNazwaKwadratu & "\") = False Then Directory.CreateDirectory(folderSegmentow & wspolnaNazwaKwadratu & "\")
        Sleep(500 * 1) 'czas na utworzenie folderu
        folderTB = folderSegmentow & wspolnaNazwaKwadratu & "\"


        If Directory.Exists(folderTB & "set\") = False Then Directory.CreateDirectory(folderTB & "set\")
        Sleep(500 * 1) 'czas na utworzenie folderu SET
        folderTBset = folderTB & "set\"

        If File.Exists(folderTB & wspolnaNazwaKwadratu & ".set") Then File.Delete(folderTB & wspolnaNazwaKwadratu & ".set")
        plikTB_map()



    End Sub




End Module
