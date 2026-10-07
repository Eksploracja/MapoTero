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

Imports MapoTero.Core

''' <summary>
''' Zapis plików georeferencyjnych obrazu (segmentu lub scalonego arkusza) - jedna procedura zamiast
''' dawnych PlikGeoreferencyjny_map/_gmi/_wld/_points/_jpgw/_kml/_tab z osobnymi gałęziami dla scalania.
''' </summary>
Public Module ZapisGeoreferencji

    ''' <summary>
    ''' Zapisuje wybrane pliki georeferencyjne obok pliku obrazu.
    ''' </summary>
    ''' <param name="obraz">Położenie obrazu; NazwaPliku i SciezkaPliku wskazują plik obrazu.</param>
    ''' <param name="folder">Folder obrazu (zakończony "\").</param>
    ''' <param name="nazwa">Nazwa pliku obrazu bez rozszerzenia.</param>
    ''' <param name="rozszerzenie">Rozszerzenie pliku obrazu (jpg, png, tif, gif).</param>
    Public Sub ZapiszPliki(obraz As ObrazGeoreferencyjny, folder As String, nazwa As String, rozszerzenie As String, opcje As OpcjeGeoreferencji)
        Dim kod = KodowanieSystemowe()
        Dim baza As String = folder & nazwa

        If opcje.Map Then Georeferencja.Zapisz(baza & ".map", Georeferencja.MapOzi(obraz), kod)
        If opcje.Gmi Then Georeferencja.Zapisz(baza & ".gmi", Georeferencja.Gmi(obraz), kod)
        If opcje.WldPoints Then
            Georeferencja.Zapisz(baza & ".wld", Georeferencja.Wld(obraz), kod)
            Georeferencja.Zapisz(baza & "." & rozszerzenie & ".points", Georeferencja.Points(obraz), kod)
        End If
        If opcje.WorldFile Then
            Georeferencja.Zapisz(baza & "." & Georeferencja.RozszerzenieWorldFile(rozszerzenie), Georeferencja.WorldFile(obraz), kod)
            'definicja układu - QGIS i ArcGIS odczytują ją automatycznie
            Georeferencja.Zapisz(baza & ".prj", Georeferencja.Prj(obraz), kod)
        End If
        If opcje.Kml Then Georeferencja.Zapisz(baza & ".kml", Georeferencja.Kml(obraz, nazwa), Georeferencja.Utf8BezBom)
        If opcje.Tab Then Georeferencja.Zapisz(baza & ".tab", Georeferencja.Tab(obraz), kod)
    End Sub

    ''' <summary>Obraz georeferencyjny dla pliku w folderze.</summary>
    Public Function Obraz(uklad As UkladWspolrzednych, zasieg As Zasieg, szerokoscPx As Long, wysokoscPx As Long,
                          folder As String, nazwaPliku As String) As ObrazGeoreferencyjny
        Return New ObrazGeoreferencyjny With {.Uklad = uklad, .Zasieg = zasieg, .SzerokoscPx = szerokoscPx, .WysokoscPx = wysokoscPx,
                                              .NazwaPliku = nazwaPliku, .SciezkaPliku = folder & nazwaPliku}
    End Function

End Module
