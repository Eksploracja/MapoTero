'Copyright (C) <2015>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Drawing
Imports System.Windows.Forms

Public Class FormPomocMbtiles

    Private Sub FormPomocMbtiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        rtbTresc.Clear()
        GenerujTrescPomocy()
        rtbTresc.SelectionStart = 0
        rtbTresc.SelectionLength = 0
        rtbTresc.ScrollToCaret()
    End Sub

    Private Sub btnZamknij_Click(sender As Object, e As EventArgs) Handles btnZamknij.Click
        Close()
    End Sub

    Private Sub GenerujTrescPomocy()
        DodajTytul("Kreator wielowarstwowego i wieloskalowego MBTiles")
        DodajAkapit("Narzędzie umożliwia skompilowanie wielu wcześniej pobranych sesji mapowych do jednego, uniwersalnego pliku bazy kafelków MBTiles. " &
                    "Dzięki niemu w aplikacjach mobilnych (OruxMaps, Locus Map, OsmAnd i innych) możesz w zależności od skali powiększenia oglądać zupełnie inne mapy " &
                    "(np. z daleka przeglądową topo 1:100k, w zbliżeniu szczegółową topo 1:10k, a w maksymalnym powiększeniu ortofotomapę) lub łączyć je ze sobą.")

        DodajNaglowek1("1. Przygotowanie danych przed uruchomieniem kreatora")
        DodajPunktor("Każdą mapę (warstwę) pobierz wcześniej w głównym oknie MapoTero do osobnego folderu (np. 'sesja_topo25', 'sesja_topo10', 'sesja_orto').")
        DodajPunktor("Pobierane foldery muszą obejmować ten sam rejon geograficzny i zawierać plik konfiguracyjny conf.txt oraz pobrane kafelki.")
        DodajPunktor("Kreator nie pobiera kafelków z internetu - kompiluje lokalnie pobrane sesje w jedną zoptymalizowaną bazę SQLite MBTiles.")

        DodajNaglowek1("2. Dodawanie warstw do projektu")
        DodajPunktor("Kliknij 'Dodaj warstwę...' i wskaż folder z pobraną warstwą mapy.")
        DodajPunktor("Możesz także wskazać folder nadrzędny (np. 'download') - program wykryje wszystkie podfoldery z sesjami i zaproponuje dodanie ich wszystkich naraz.")
        DodajPunktor("Program automatycznie odczyta rozdzielczość piksela i zaproponuje optymalne poziomy powiększeń (Zoom Od - Zoom Do).")

        DodajNaglowek1("3. Zakresy wyświetlania (Zoom Od – Zoom Do)")
        DodajPunktor("W kolumnach 'Zoom Od' i 'Zoom Do' precyzyjnie wskaż, w jakim przedziale powiększeń dana warstwa ma być widoczna.")
        DodajPunktor("Przykładowa konfiguracja wieloskalowa:" & vbCrLf &
                     "  • Poziom 11 - 13: Mapa topograficzna ogólna 1:100 000" & vbCrLf &
                     "  • Poziom 14 - 15: Mapa topograficzna 1:25 000" & vbCrLf &
                     "  • Poziom 16 - 17: Mapa topograficzna 1:10 000" & vbCrLf &
                     "  • Poziom 18 - 19: Ortofotomapa o wysokiej rozdzielczości")
        DodajPunktor("Dwie lub więcej warstw mogą mieć ten sam zakres zoom - zostaną wtedy zblendowane (nałożone na siebie).")

        DodajNaglowek1("4. Hierarchia i kolejność warstw (Stos warstw)")
        DodajPunktor("GÓRA TABELI = WIERZCH MAPY (tak samo jak w programach GIS, np. QGIS czy Photoshopie).")
        DodajPunktor("DÓŁ TABELI = SPÓD / PODKŁAD (warstwa bazowa, tło).")
        DodajPunktor("Użyj przycisków 'W górę' i 'W dół':" & vbCrLf &
                     "  • 'W górę' - wynosi warstwę wyżej (przesuwa na wierzch stosu)," & vbCrLf &
                     "  • 'W dół' - spycha warstwę niżej (pod spód, jako podkład).")

        DodajNaglowek1("5. Przezroczystość, odcienie szarości i tryby mieszania (Styl QGIS)")
        DodajPunktor("Krycie %: określa krycie warstwy od 0% (niewidoczna) do 100% (pełne krycie).")
        DodajPunktor("Szarość: konwertuje kolorową warstwę do odcieni szarości. Świetne jako neutralny podkład satelitarny dla kolorowych szlaków lub do stonowania pstrej mapy topo.")
        DodajPunktor("Białe przezr.: zamienia białe tło (np. skanów papierowych lub WMS bez obsługi kanału alfa) w przezroczyste.")
        DodajPunktor("Tryb mieszania (kolumna 'Mieszanie'):" & vbCrLf &
                     "  • 1 Zwykły (Normal) - standardowe nakładanie z kryciem." & vbCrLf &
                     "  • 2 Mnożenie (Multiply) - tryb znany z QGIS i Photoshopa. Mnoży kolory warstwy przez podkład. " &
                     "Białe tło staje się całkowicie niewidoczne, a czarne napisy, rzędne wysokościowe i poziomice zachowują maksymalny kontrast na tle ortofotomapy (brak wypłowienia i 'mlecznej mgły')!" & vbCrLf &
                     "  • 3 Ciemniej (Darken) - wybiera ciemniejszy odcień z obu warstw." & vbCrLf &
                     "  • 4 Jaśniej (Lighten) - wybiera jaśniejszy kolor z obu warstw (np. jasne drogi/cieki wodne na ciemnym tle)." & vbCrLf &
                     "  • 5 Ekran (Screen) - rozjaśnia tło, czerń znika (odwrotność mnożenia, np. dla nocnych świateł czy radarów)." & vbCrLf &
                     "  • 6 Nakładka (Overlay) - łączy mnożenie i ekran, podbijając kontrast (idealne dla cieniowania rzeźby terenu / hillshade).")

        DodajNaglowek1("6. Symulator podglądu na żywo")
        DodajPunktor("Przesuwaj suwak 'Poziom zoom' w prawym panelu, aby podejrzeć kafel podglądu dla każdego powiększenia.")
        DodajPunktor("Podgląd uwzględnia kolejność warstw, przezroczystości, krycie oraz tryb mnożenia w czasie rzeczywistym.")
        DodajPunktor("Przycisk 'Pobierz środek z mapy głównej' centruje punkt podglądu na aktualnym widoku mapy z okna głównego.")

        DodajNaglowek1("7. Parametry kompresji i zapis pliku")
        DodajPunktor("Format Smart Hybrid (zalecany): kafle bez przezroczystości zapisuje jako mały JPEG, a kafle z przezroczystością (lub na krawędziach) jako PNG. Zapewnia optymalny rozmiar i brak czarnych/białych ramek.")
        DodajPunktor("Szybka piramida w pasmach: dla poziomów, na których widoczna jest ta sama warstwa, generuje niższe powiększenia przez szybkie zmniejszanie (downscale 2x), skracając czas zapisu nawet o 70%.")
        DodajPunktor("Wypełnij brakujące poziomy: jeśli na danym zoomie brak dedykowanej warstwy, kafelki zostaną przeskalowane z najbliższej niższej warstwy.")

        DodajNaglowek1("8. Szacunek rozmiaru i kompatybilność z FAT32")
        DodajPunktor("Kliknij 'Oszacuj rozmiar i liczbę kafli' przed rozpoczęciem kompilacji.")
        DodajPunktor("Program próbkując statystycznie kafelki obliczy prognozowaną wagę pliku i sprawdzi, czy nie przekracza limitu 4 GB (maksymalny rozmiar pliku na kartach pamięci sformatowanych w FAT32 dla telefonów/GPS).")

        DodajNaglowek1("9. Kompatybilność z aplikacjami mobilnymi")
        DodajPunktor("Wygenerowany plik .mbtiles wystarczy skopiować do folderu map offline:" & vbCrLf &
                     "  • OruxMaps: folder 'oruxmaps/mapfiles/'" & vbCrLf &
                     "  • Locus Map: folder 'Locus/maps/' lub 'Wczytaj mapę zewnętrzną'" & vbCrLf &
                     "  • OsmAnd / inne aplikacje obsługujące standard MBTiles.")

        DodajAkapit(vbCrLf & "Wskazówka: Aby uzyskać najbardziej czytelną mapę topo na tle ortofotomapy, ustaw mapę topo jako wierzchnią, wybierz tryb 'Mnożenie', zaznacz 'Białe przezr.' i ustaw krycie na 75-85%.")
    End Sub

    Private Sub DodajTytul(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        rtbTresc.SelectionColor = Color.DarkBlue
        rtbTresc.AppendText(tekst & vbCrLf & vbCrLf)
    End Sub

    Private Sub DodajNaglowek1(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 10.5!, FontStyle.Bold)
        rtbTresc.SelectionColor = Color.FromArgb(0, 70, 140)
        rtbTresc.AppendText(vbCrLf & tekst & vbCrLf)
    End Sub

    Private Sub DodajAkapit(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 9.5!, FontStyle.Regular)
        rtbTresc.SelectionColor = Color.FromArgb(30, 30, 30)
        rtbTresc.AppendText(tekst & vbCrLf)
    End Sub

    Private Sub DodajPunktor(tekst As String)
        rtbTresc.SelectionFont = New Font("Segoe UI", 9.5!, FontStyle.Regular)
        rtbTresc.SelectionColor = Color.Black
        rtbTresc.AppendText(" • " & tekst & vbCrLf)
    End Sub

End Class
