'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>Opis zasięgu przestrzennego warstwy w spisie galerii (<c>warstwy.json</c>).</summary>
Public Class ZasiegWarstwyGalerii
    <JsonPropertyName("typ")>
    Public Property Typ As String = "kraj"

    <JsonPropertyName("kod")>
    Public Property Kod As String = ""

    <JsonPropertyName("etykieta")>
    Public Property Etykieta As String = ""

    <JsonPropertyName("opis")>
    Public Property Opis As String = ""

    <JsonPropertyName("procent_pl")>
    Public Property ProcentPl As Integer?

    <JsonPropertyName("procent_total")>
    Public Property ProcentTotal As Integer?

    <JsonPropertyName("komorki_total")>
    Public Property KomorkiTotal As Integer?

    <JsonPropertyName("maska_hex")>
    Public Property MaskaHex As String = ""

    <JsonPropertyName("clip_polska")>
    Public Property ClipPolska As Boolean?

    <JsonPropertyName("bbox")>
    Public Property Bbox As Double()
End Class

''' <summary>Pojedyncza warstwa z pliku <c>pomoc/galeria/warstwy.json</c>.</summary>
Public Class PozycjaGaleriiWarstw
    <JsonPropertyName("id")>
    Public Property Id As String = ""

    <JsonPropertyName("zbior")>
    Public Property Zbior As String = ""

    <JsonPropertyName("warstwa")>
    Public Property Warstwa As String = ""

    <JsonPropertyName("nazwa")>
    Public Property Nazwa As String = ""

    <JsonPropertyName("identyfikator")>
    Public Property Identyfikator As String = ""

    <JsonPropertyName("grupa")>
    Public Property Grupa As String = ""

    <JsonPropertyName("podgrupa")>
    Public Property Podgrupa As String = ""

    <JsonPropertyName("typ")>
    Public Property Typ As String = "WMS"

    <JsonPropertyName("nakladka")>
    Public Property Nakladka As Boolean

    <JsonPropertyName("zalecany_piksel")>
    Public Property ZalecanyPiksel As Double?

    <JsonPropertyName("zrodlo")>
    Public Property Zrodlo As String = ""

    <JsonPropertyName("obrazek")>
    Public Property Obrazek As String = ""

    <JsonPropertyName("miejsce_probki")>
    Public Property MiejsceProbki As String = ""

    <JsonPropertyName("zasieg")>
    Public Property Zasieg As ZasiegWarstwyGalerii
End Class

''' <summary>Zawartość pliku <c>warstwy.json</c> udostępnianego przez galerię online.</summary>
Public Class KatalogGaleriiWarstw
    Public Property Wygenerowano As String = ""
    Public Property WersjaProgramu As String = ""
    Public ReadOnly Property Pozycje As New List(Of PozycjaGaleriiWarstw)()

    Private ReadOnly _indeks As New Dictionary(Of String, PozycjaGaleriiWarstw)(StringComparer.OrdinalIgnoreCase)

    Private Class SurowyPlikGalerii
        <JsonPropertyName("wygenerowano")>
        Public Property Wygenerowano As String = ""

        <JsonPropertyName("wersja_programu")>
        Public Property WersjaProgramu As String = ""

        <JsonPropertyName("warstwy")>
        Public Property Warstwy As List(Of PozycjaGaleriiWarstw)
    End Class

    Public Shared Function Klucz(zbior As String, warstwa As String) As String
        Return (If(zbior, "").Trim() & "|" & If(warstwa, "").Trim()).ToLowerInvariant()
    End Function

    ''' <summary>Wyszukuje warstwę w katalogu galerii po nazwie pliku zbioru (bez .txt) i nazwie technicznej warstwy.</summary>
    Public Function Znajdz(zbior As String, warstwa As String) As PozycjaGaleriiWarstw
        If String.IsNullOrWhiteSpace(zbior) OrElse String.IsNullOrWhiteSpace(warstwa) Then Return Nothing
        Dim p As PozycjaGaleriiWarstw = Nothing
        If _indeks.TryGetValue(Klucz(zbior, warstwa), p) Then Return p
        Return Nothing
    End Function

    ''' <summary>Czy warstwa występuje w wczytanym katalogu galerii.</summary>
    Public Function Zawiera(zbior As String, warstwa As String) As Boolean
        Return Znajdz(zbior, warstwa) IsNot Nothing
    End Function

    ''' <summary>Parsuje treść pliku <c>warstwy.json</c>; zwraca pusty katalog przy błędnym albo pustym JSON-ie.</summary>
    Public Shared Function ZTekstu(json As String) As KatalogGaleriiWarstw
        Dim wynik As New KatalogGaleriiWarstw()
        If String.IsNullOrWhiteSpace(json) Then Return wynik
        Try
            Dim opcje As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True
            }
            Dim surowy = JsonSerializer.Deserialize(Of SurowyPlikGalerii)(json, opcje)
            If surowy Is Nothing Then Return wynik
            wynik.Wygenerowano = If(surowy.Wygenerowano, "")
            wynik.WersjaProgramu = If(surowy.WersjaProgramu, "")
            If surowy.Warstwy IsNot Nothing Then
                For Each p In surowy.Warstwy
                    If p Is Nothing OrElse String.IsNullOrWhiteSpace(p.Zbior) OrElse String.IsNullOrWhiteSpace(p.Warstwa) Then Continue For
                    wynik.Pozycje.Add(p)
                    Dim k = Klucz(p.Zbior, p.Warstwa)
                    If Not wynik._indeks.ContainsKey(k) Then
                        wynik._indeks(k) = p
                    End If
                Next
            End If
        Catch
            Return New KatalogGaleriiWarstw()
        End Try
        Return wynik
    End Function
End Class

''' <summary>Prostokąt w układzie współrzędnych podglądu zasięgu (viewBox 25..340 × 0..275).</summary>
Public Structure ProstokatMapyGalerii
    Public ReadOnly X As Single
    Public ReadOnly Y As Single
    Public ReadOnly Szerokosc As Single
    Public ReadOnly Wysokosc As Single

    Public Sub New(x As Single, y As Single, szerokosc As Single, wysokosc As Single)
        Me.X = x
        Me.Y = y
        Me.Szerokosc = szerokosc
        Me.Wysokosc = wysokosc
    End Sub
End Structure

''' <summary>Punkt w układzie współrzędnych podglądu zasięgu.</summary>
Public Structure PunktMapyGalerii
    Public ReadOnly X As Single
    Public ReadOnly Y As Single

    Public Sub New(x As Single, y As Single)
        Me.X = x
        Me.Y = y
    End Sub
End Structure

''' <summary>Etykieta miasta orientacyjnego na podglądzie zasięgu.</summary>
Public Class MiastoMapyGalerii
    Public ReadOnly Property Nazwa As String
    Public ReadOnly Property X As Single
    Public ReadOnly Property Y As Single

    Public Sub New(nazwa As String, x As Single, y As Single)
        Me.Nazwa = nazwa
        Me.X = x
        Me.Y = y
    End Sub
End Class

''' <summary>
''' Geometria i dekoder zasięgu z galerii warstw (zgodna z <c>pomoc/galeria/galeria.js</c> i <c>maska_hex</c>
''' w <c>warstwy.json</c> — uwzględnia rzeczywiste pokrycie i „dziury” wewnątrz Polski).
''' </summary>
Public NotInheritable Class GeometriaZasieguGalerii

    Private Sub New()
    End Sub

    Public Const Skala As Double = 30.9524
    Public Const PrzesuniecieX As Double = 42.46
    Public Const PrzesuniecieY As Double = 10.0
    Public Const CosLat0 As Double = 0.61291
    Public Const LonMin As Double = 11.0
    Public Const LatMax As Double = 56.4

    Public Const LiczbaKolumnMaski As Integer = 28
    Public Const LiczbaWierszyMaski As Integer = 20

    Public Shared ReadOnly Polska As PunktMapyGalerii() = ParsujPunkty("281.6,161.1 283.0,164.9 282.7,167.5 286.5,173.2 288.8,179.0 290.9,181.1 291.1,181.9 288.7,183.8 290.0,185.7 290.9,189.0 290.8,191.7 288.6,195.4 283.6,196.4 270.8,211.2 264.5,220.3 263.5,222.4 265.0,229.9 264.5,233.7 267.2,236.5 266.5,238.4 262.1,236.7 260.1,236.5 251.5,232.6 250.5,229.8 249.1,228.4 244.3,226.3 238.8,225.8 233.7,226.1 232.2,228.5 229.7,229.3 224.9,226.9 220.1,227.1 216.3,229.3 214.3,233.4 211.6,232.2 209.4,233.1 208.6,232.7 209.2,230.7 208.9,227.5 206.2,226.5 202.6,220.5 199.0,223.2 197.1,226.7 193.6,226.8 193.1,223.6 191.0,223.2 190.6,220.1 186.5,215.6 185.9,211.8 181.9,210.3 181.0,210.8 175.8,207.0 174.9,208.1 172.9,209.0 172.1,208.6 169.2,205.0 168.2,204.5 167.5,203.2 169.7,202.1 170.2,201.0 169.6,198.6 166.8,199.9 164.2,200.2 159.2,196.4 154.0,194.9 156.1,200.8 153.3,202.3 150.2,205.1 149.4,204.9 143.7,196.7 141.7,195.9 141.3,195.0 142.7,193.1 144.1,192.6 145.3,190.3 142.7,187.8 137.5,189.2 136.3,187.3 132.2,185.2 125.1,183.0 123.6,180.8 123.2,178.4 118.2,176.7 118.0,180.7 114.7,181.5 117.7,174.2 118.7,169.3 116.6,162.8 113.1,160.9 113.1,156.7 110.8,151.4 112.2,149.1 113.6,143.7 112.7,142.8 112.3,138.5 111.1,137.6 109.9,135.1 111.1,129.8 109.1,126.2 104.2,122.0 101.8,119.0 102.0,117.3 104.9,114.4 106.4,112.0 107.2,109.1 107.2,106.5 104.3,92.7 110.4,95.4 109.8,93.3 110.0,89.8 103.4,88.3 103.4,85.8 106.7,86.6 113.0,83.7 123.8,80.0 140.9,75.3 147.9,67.1 154.1,65.8 161.3,61.7 176.9,58.4 181.4,58.3 189.7,63.1 190.4,64.7 184.8,61.3 183.5,61.2 188.0,70.9 191.1,72.8 193.8,73.5 202.0,72.3 205.7,70.1 217.1,71.3 254.3,73.1 265.0,73.4 266.8,72.0 268.1,72.2 270.4,73.5 270.9,74.9 275.5,76.8 278.7,79.8 279.2,85.6 281.5,96.7 285.1,106.9 286.9,114.4 287.2,124.4 286.1,125.6 277.9,130.2 273.4,137.3 273.8,138.2 281.5,143.0 282.5,144.9 281.6,149.9 282.0,152.1 280.4,155.2 280.4,158.0 281.6,161.1")
    Public Shared ReadOnly Baltyk As PunktMapyGalerii() = ParsujPunkty("25.0,0.0 340.0,0.0 340.0,102.0 25.0,102.0")
    Public Shared ReadOnly Czechy As PunktMapyGalerii() = ParsujPunkty("191.0,223.2 186.6,223.8 183.1,226.9 182.2,228.6 178.3,231.1 176.9,237.0 173.6,239.1 170.7,242.5 168.2,243.9 165.4,244.4 159.9,243.4 158.9,244.0 157.5,245.8 155.4,251.5 154.1,248.2 150.8,247.3 147.6,245.4 146.4,245.2 144.3,247.1 138.4,246.6 134.0,243.2 131.6,243.4 124.2,239.9 121.4,240.7 119.6,239.1 118.2,239.0 116.9,246.1 115.0,246.0 114.3,246.9 112.8,249.2 112.5,251.5 108.6,250.6 106.3,252.2 103.0,252.1 100.3,251.4 99.2,248.6 95.9,246.3 90.8,240.3 88.8,240.4 85.9,236.0 83.1,234.1 78.8,228.8 76.9,228.9 74.4,226.2 68.8,216.2 71.1,211.3 70.1,209.5 65.4,205.1 63.1,198.8 64.0,198.5 64.7,199.2 66.7,202.5 70.0,197.3 71.8,195.9 76.0,194.8 79.3,195.5 80.7,192.9 83.8,192.3 84.9,190.2 87.5,188.9 88.7,189.5 91.0,186.3 97.4,184.5 106.4,180.3 104.2,177.1 104.7,176.2 105.4,176.0 109.7,177.3 110.7,179.7 111.2,179.8 111.0,181.6 113.1,182.9 113.9,182.8 114.7,181.5 118.0,180.7 118.2,176.7 123.2,178.4 123.6,180.8 125.1,183.0 132.2,185.2 136.3,187.3 137.5,189.2 142.7,187.8 144.1,188.9 145.3,190.3 144.5,192.1 142.7,193.1 141.3,195.0 141.7,195.9 143.7,196.7 149.4,204.9 150.2,205.1 153.3,202.3 156.1,200.8 154.0,194.9 159.2,196.4 164.2,200.2 166.8,199.9 169.6,198.6 170.2,201.0 169.7,202.1 167.5,203.2 168.2,204.5 169.2,205.0 172.1,208.6 172.9,209.0 174.9,208.1 175.8,207.0 181.0,210.8 181.9,210.3 185.9,211.8 186.5,215.6 190.6,220.1 191.0,223.2")
    Public Shared ReadOnly Slowacja As PunktMapyGalerii() = ParsujPunkty("261.4,236.8 258.5,243.0 256.7,248.8 253.9,252.4 253.3,257.8 246.7,259.5 245.9,259.3 242.9,254.7 240.7,252.9 233.4,254.3 229.6,253.1 225.4,253.0 222.5,253.7 219.5,260.9 211.3,265.9 206.1,263.1 204.9,263.4 203.1,266.6 199.3,267.7 192.6,268.4 190.3,270.0 189.5,271.9 189.8,275.0 167.3,275.0 162.3,270.3 159.1,269.8 157.9,268.8 153.7,258.0 153.7,256.3 155.2,252.9 156.0,249.0 158.9,244.0 159.9,243.4 165.4,244.4 168.2,243.9 170.7,242.5 173.6,239.1 176.9,237.0 178.3,231.1 182.2,228.6 183.1,226.9 186.6,223.8 191.0,223.2 193.1,223.6 193.6,226.8 197.1,226.7 199.0,223.2 202.6,220.5 206.2,226.5 208.9,227.5 209.2,230.7 208.6,232.7 209.4,233.1 211.6,232.2 214.3,233.4 216.3,229.3 220.1,227.1 224.9,226.9 227.0,227.6 228.4,228.9 231.2,229.2 233.7,226.1 238.8,225.8 244.3,226.3 249.1,228.4 250.5,229.8 251.5,232.6 261.4,236.8")
    Public Shared ReadOnly NiemcyBrandenburgia As PunktMapyGalerii() = ParsujPunkty("97.1,99.7 97.8,102.2 99.4,101.8 100.4,102.5 101.7,101.6 103.9,102.0 103.8,103.8 101.3,107.1 104.3,107.2 105.4,105.6 107.0,104.9 107.8,106.7 106.3,109.7 106.7,111.0 106.2,113.2 102.1,116.4 102.3,119.0 101.7,120.2 105.8,122.7 108.2,125.8 111.5,128.2 110.8,129.7 111.3,130.7 109.7,132.6 109.4,133.9 110.5,136.6 110.2,137.2 112.6,138.7 112.1,142.4 113.4,143.9 112.3,149.1 110.5,151.2 110.7,152.2 111.7,152.5 111.8,154.5 113.1,155.8 113.5,157.9 112.9,159.0 112.4,158.6 112.0,160.1 111.0,160.3 110.2,159.4 109.4,160.2 106.9,160.5 105.9,161.6 104.8,160.7 102.3,160.3 102.4,160.9 101.1,161.8 101.4,162.3 100.3,162.3 100.9,163.3 99.7,165.6 98.8,164.8 95.2,165.9 92.2,165.7 92.0,165.1 91.0,165.7 90.6,164.6 88.4,163.1 87.9,164.0 86.8,163.5 86.2,164.8 85.6,164.7 85.8,165.2 84.8,164.8 84.6,163.8 84.1,163.9 84.7,160.9 84.3,159.9 83.5,159.8 83.6,158.6 83.0,158.0 82.3,158.4 81.6,157.1 84.2,155.0 83.0,150.9 83.5,150.1 81.3,150.2 81.5,149.3 80.1,149.3 79.9,148.2 77.8,148.3 77.7,147.3 76.6,147.5 74.4,145.9 73.4,146.8 71.9,146.8 70.5,145.2 69.7,145.7 66.8,143.0 65.6,141.1 67.2,139.4 66.2,138.4 67.5,135.6 67.0,135.1 67.4,133.8 66.8,133.4 67.9,131.5 66.7,131.2 66.0,130.1 65.3,131.0 64.8,130.6 65.1,129.8 64.3,130.0 65.0,128.6 64.8,127.0 66.1,126.9 66.0,125.0 65.3,124.2 65.6,122.4 66.4,121.7 66.0,119.7 65.2,119.2 63.9,119.9 63.9,118.8 63.6,119.2 62.0,118.8 61.1,119.2 58.5,118.2 58.7,116.9 56.7,115.7 55.6,116.0 55.5,115.1 54.5,115.0 54.6,114.0 52.4,113.9 51.2,112.8 49.1,113.6 47.6,111.5 50.0,111.9 50.0,111.0 52.7,111.5 53.4,110.2 53.2,108.7 54.5,107.8 55.8,107.8 56.4,108.6 58.3,108.3 57.7,107.5 59.0,107.7 59.5,106.7 60.5,107.0 62.0,106.0 62.5,103.9 63.6,104.7 64.5,104.2 64.7,104.9 65.9,104.3 66.5,105.4 69.0,106.0 69.9,107.6 74.2,107.5 74.3,108.3 75.9,108.4 75.5,109.2 76.0,109.6 77.6,109.2 78.2,109.9 80.0,109.2 79.4,110.0 80.1,110.4 82.4,108.7 82.7,107.9 83.9,107.6 84.8,108.6 85.2,107.4 86.1,106.7 87.3,106.7 88.1,107.7 88.7,106.1 89.5,106.4 89.9,105.2 90.4,105.5 90.8,102.9 92.2,102.6 92.5,101.5 93.9,100.4 95.1,100.7 96.1,99.9 95.2,99.4 95.4,98.0 95.9,99.3 97.1,99.7")
    Public Shared ReadOnly NiemcySaksonia As PunktMapyGalerii() = ParsujPunkty("78.1,156.3 78.2,156.9 78.7,156.6 78.6,157.1 79.5,157.4 80.7,156.5 82.3,158.4 83.0,158.0 83.6,158.6 83.5,159.8 84.3,159.9 84.7,160.9 84.1,163.9 84.6,163.8 84.8,164.8 85.8,165.2 85.6,164.7 86.2,164.8 86.8,163.5 87.9,164.0 88.4,163.1 90.6,164.6 91.0,165.7 92.0,165.1 92.2,165.7 95.2,165.9 98.8,164.8 99.7,165.6 100.9,163.3 100.3,162.3 101.4,162.3 101.1,161.8 102.4,160.9 102.3,160.3 104.8,160.7 105.9,161.6 106.9,160.5 109.4,160.2 110.2,159.4 111.0,160.3 112.0,160.1 112.5,158.6 113.4,160.9 116.6,161.9 117.7,163.1 117.7,166.2 118.9,167.6 119.1,169.7 118.4,170.6 117.9,174.7 116.5,178.6 114.9,180.5 114.6,182.4 112.9,182.5 112.9,182.1 111.0,181.6 111.7,179.2 110.0,179.5 110.7,177.5 108.9,176.6 108.9,175.5 108.7,176.5 107.9,176.2 106.9,176.9 106.1,176.1 105.4,176.4 105.0,175.5 104.1,178.0 107.0,178.9 106.4,180.6 104.2,180.7 103.4,181.9 100.5,183.3 98.5,183.2 98.4,183.8 97.4,183.8 97.5,185.1 96.5,185.9 93.5,185.9 93.1,185.4 90.8,186.0 90.4,188.4 89.2,189.6 87.5,188.2 86.6,190.1 85.1,189.8 84.1,192.6 81.0,192.4 80.9,194.2 80.0,195.3 79.2,195.5 77.2,193.9 76.8,194.7 75.9,194.5 74.8,195.8 73.1,195.1 71.2,195.7 70.8,197.3 69.1,198.2 69.2,198.9 68.3,199.5 67.6,202.5 66.8,202.4 66.8,201.1 65.9,200.3 66.2,199.6 65.1,199.5 65.1,198.1 63.7,198.4 61.0,197.1 61.1,195.9 59.4,194.3 60.2,193.6 60.6,194.0 60.9,192.9 60.1,191.7 59.3,192.0 59.1,191.2 60.2,190.7 60.2,189.8 61.8,188.3 62.2,190.8 63.0,190.7 63.1,190.0 64.6,190.1 64.4,188.5 65.3,188.9 65.4,188.1 66.8,188.3 67.8,186.8 66.9,186.6 66.5,185.7 66.9,185.3 66.0,185.0 66.3,184.1 67.2,183.9 66.4,182.6 68.3,182.4 68.8,181.5 70.0,181.9 70.8,180.3 73.8,180.1 74.1,179.4 73.2,177.1 72.7,177.5 71.6,177.0 71.9,176.3 71.3,176.5 71.2,175.1 70.6,175.1 70.3,174.4 69.4,174.8 68.1,173.9 67.1,174.2 65.8,173.6 66.2,173.1 65.4,172.7 65.6,171.5 65.0,171.4 65.5,170.4 64.4,167.2 65.2,166.9 65.3,164.0 64.4,162.8 65.4,162.1 65.2,161.0 66.1,159.6 69.7,159.1 70.0,158.3 70.8,158.8 72.7,157.8 73.7,158.1 74.7,156.7 76.4,157.2 77.4,156.1 78.1,156.3")
    Public Shared ReadOnly NiemcyReszta As PunktMapyGalerii() = ParsujPunkty("25.0,275.0 25.0,69.3 26.2,69.2 27.5,71.6 30.3,70.7 37.4,74.5 42.7,72.6 43.7,75.6 42.6,78.7 38.9,82.0 39.7,84.0 40.9,84.4 44.4,84.0 50.0,86.0 57.6,79.8 63.5,79.1 64.6,77.3 72.3,69.8 80.9,71.6 83.2,75.5 88.9,79.9 94.1,79.5 96.0,83.7 96.8,88.8 99.8,91.5 104.3,92.7 107.2,106.5 107.2,109.1 106.4,112.0 104.9,114.4 102.0,117.3 101.8,119.0 104.2,122.0 109.1,126.2 111.1,129.8 109.9,135.1 111.1,137.6 112.3,138.5 112.7,142.8 113.6,143.7 112.2,149.1 110.8,151.4 113.1,156.7 113.1,160.9 116.6,162.8 118.7,169.3 116.8,176.9 114.5,182.0 113.1,182.9 111.0,181.6 111.2,179.8 110.7,179.7 109.7,177.3 105.4,176.0 104.7,176.2 104.2,177.1 106.4,180.3 97.4,184.5 91.0,186.3 88.7,189.5 87.5,188.9 84.9,190.2 83.8,192.3 80.7,192.9 79.3,195.5 76.0,194.8 71.8,195.9 70.0,197.3 66.7,202.5 64.7,199.2 64.0,198.5 63.1,198.8 65.4,205.1 70.1,209.5 71.1,211.3 68.8,216.2 74.4,226.2 76.9,228.9 78.8,228.8 83.1,234.1 85.9,236.0 88.0,239.7 88.8,240.4 90.8,240.3 95.9,246.3 95.3,251.8 94.1,253.2 93.2,253.8 89.6,252.0 87.5,258.8 78.5,263.7 75.9,266.7 75.9,267.6 79.5,273.4 79.5,275.0")
    Public Shared ReadOnly NiemcyResztaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("25.0,275.0 25.0,69.3 26.2,69.2 27.5,71.6 30.3,70.7 37.4,74.5 42.7,72.6 43.7,75.6 42.6,78.7 38.9,82.0 39.7,84.0 40.9,84.4 44.4,84.0 50.0,86.0 57.6,79.8 63.5,79.1 64.6,77.3 72.3,69.8 80.9,71.6 83.2,75.5 88.9,79.9 94.1,79.5 96.0,83.7 96.8,88.8 99.8,91.5 104.3,92.7 107.2,106.5 107.2,109.1 106.4,112.0 104.9,114.4 102.0,117.3 101.8,119.0 104.2,122.0 109.1,126.2 111.1,129.8 109.9,135.1 111.1,137.6 112.3,138.5 112.7,142.8 113.6,143.7 112.2,149.1 110.8,151.4 113.1,156.7 113.1,160.9 116.6,162.8 118.7,169.3 116.8,176.9 114.5,182.0 113.1,182.9 111.0,181.6 111.2,179.8 110.7,179.7 109.7,177.3 105.4,176.0 104.7,176.2 104.2,177.1 106.4,180.3 97.4,184.5 91.0,186.3 88.7,189.5 87.5,188.9 84.9,190.2 83.8,192.3 80.7,192.9 79.3,195.5 76.0,194.8 71.8,195.9 70.0,197.3 66.7,202.5 64.7,199.2 64.0,198.5 63.1,198.8 65.4,205.1 70.1,209.5 71.1,211.3 68.8,216.2 74.4,226.2 76.9,228.9 78.8,228.8 83.1,234.1 85.9,236.0 88.0,239.7 88.8,240.4 90.8,240.3 95.9,246.3 95.3,251.8 94.1,253.2 93.2,253.8 89.6,252.0 87.5,258.8 78.5,263.7 75.9,266.7 75.9,267.6 79.5,273.4 79.5,275.0;93.9,72.4 94.3,74.5 93.8,75.6 91.7,73.8 89.5,73.8 88.3,76.6 87.3,76.7 84.0,74.2 83.4,72.0 83.8,67.4 84.8,66.2 85.0,64.5 86.8,62.7 88.4,62.6 89.7,65.2 92.9,67.0 93.1,67.7 91.4,69.9 91.8,71.1 93.9,72.4;103.4,85.8 103.4,88.3 98.0,88.0 97.5,86.2 97.9,84.4 96.1,82.4 96.1,80.3 103.4,85.8;47.8,71.3 44.9,71.4 42.7,69.9 43.3,68.3 44.1,67.8 46.9,68.8 47.8,71.3")
    Public Shared ReadOnly Litwa As PunktMapyGalerii() = ParsujPunkty("231.4,44.7 230.3,44.5 232.4,40.9 234.3,34.3 234.1,38.2 232.8,42.5 231.4,44.7")
    Public Shared ReadOnly LitwaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("231.4,44.7 230.3,44.5 232.4,40.9 234.3,34.3 234.1,38.2 232.8,42.5 231.4,44.7;265.7,73.2 264.0,69.0 264.1,66.9 266.8,57.3 263.0,54.2 261.9,51.5 252.5,51.4 242.7,47.3 239.6,44.8 236.6,45.2 236.0,42.7 236.7,39.2 233.3,28.2 233.0,20.2 244.6,12.6 252.7,9.8 262.3,10.8 267.8,10.1 269.5,10.6 270.9,12.4 272.4,12.1 273.8,11.0 285.5,12.2 291.4,14.2 298.1,13.6 302.4,10.6 305.0,9.7 306.2,10.1 309.4,16.2 320.6,19.1 328.6,25.6 332.4,30.1 334.6,31.5 338.3,32.7 337.8,36.4 335.7,42.7 336.4,43.5 340.0,43.9 340.0,46.3 338.4,49.3 332.6,49.0 331.8,49.5 330.3,53.2 328.8,54.5 324.4,55.8 322.9,58.5 321.8,62.1 321.8,66.8 319.8,70.0 318.4,74.0 318.6,74.7 321.4,75.2 322.2,76.2 322.3,79.4 318.9,80.0 317.7,79.3 318.1,77.6 316.8,75.2 315.1,76.5 313.4,76.5 311.5,77.7 308.9,80.2 305.6,79.8 304.7,80.6 303.7,85.1 300.9,84.9 295.1,87.6 292.7,85.8 284.0,87.0 280.7,86.8 279.3,86.1 278.7,79.8 277.1,78.1 273.3,75.6 270.9,74.9 270.4,73.5 268.1,72.2 266.8,72.0 265.7,73.2")
    Public Shared ReadOnly SzwecjaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("147.3,13.4 146.4,14.9 145.5,14.8 144.9,12.8 145.1,4.8 147.2,0.0 151.3,0.0 147.3,13.4;144.1,0.0 141.4,4.1 137.3,15.5 135.8,17.2 134.0,18.5 130.2,16.6 124.5,17.7 119.3,17.1 114.2,17.4 112.9,18.2 113.7,21.4 111.8,21.8 110.0,20.9 107.0,23.1 103.5,27.6 103.2,30.8 105.9,37.0 102.7,41.1 95.7,40.1 86.5,42.6 78.2,40.6 79.3,38.4 79.9,30.2 70.4,13.4 74.8,15.1 76.6,14.2 73.9,8.7 77.7,8.4 78.9,6.4 78.2,3.3 75.0,1.9 74.0,0.0")
    Public Shared ReadOnly DaniaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("72.2,29.0 71.8,33.0 67.5,35.1 66.0,36.7 65.5,38.9 66.7,40.5 68.7,41.4 69.3,44.5 67.5,46.0 63.1,47.5 62.4,59.0 58.8,60.4 56.5,56.0 54.9,47.5 51.5,46.8 47.9,47.0 45.7,43.2 46.1,38.9 44.9,36.8 44.8,34.7 42.6,33.4 42.1,31.0 48.6,30.0 51.3,26.1 51.5,24.1 54.4,23.7 55.7,25.2 55.6,30.8 58.0,31.7 60.0,27.7 60.2,25.6 58.9,23.4 65.6,18.7 67.6,18.6 72.4,20.4 73.0,21.4 71.4,24.9 72.2,29.0;25.0,0.0 29.0,0.0 28.9,3.2 30.8,5.2 32.8,6.3 39.5,6.2 41.1,8.7 39.7,13.2 37.8,14.9 35.3,16.1 33.7,16.2 31.6,13.8 30.6,14.6 29.5,15.8 27.0,26.6 26.5,26.9 25.0,26.3;35.7,34.5 39.0,43.4 38.4,45.0 38.8,47.0 38.4,49.2 35.3,51.7 31.9,51.8 25.0,49.1 25.0,36.1 28.9,34.4 31.5,36.0 33.1,36.1 35.7,34.5;49.3,56.7 52.7,58.6 54.9,58.5 56.5,59.3 57.0,63.3 51.1,64.8 43.1,60.4 43.6,55.2 47.4,54.8 49.3,56.7;37.4,61.0 36.6,61.2 35.3,57.9 39.7,51.7 41.0,48.4 41.5,48.5 37.4,61.0;71.9,54.4 71.1,54.9 68.2,54.5 64.9,56.7 63.7,56.0 64.5,54.1 66.3,52.7 66.6,51.3 67.3,52.1 70.3,52.8 71.9,54.4;74.1,34.9 71.9,36.1 71.3,34.3 72.8,32.3 74.1,34.9;32.7,58.1 31.4,58.4 29.9,57.7 27.3,54.5 33.1,57.6 32.7,58.1;35.0,29.1 34.7,29.7 33.5,29.1 33.8,25.3 33.3,23.7 33.9,22.6 36.0,26.2 35.0,29.1;120.0,52.7 119.3,53.2 116.2,52.3 112.4,50.2 112.9,46.0 113.9,44.1 120.9,48.9 120.9,50.6 120.0,52.7")
    Public Shared ReadOnly BialorusWielokaty As PunktMapyGalerii()() = ParsujWielokaty("340.0,152.9 335.6,152.0 334.5,151.0 323.0,148.5 313.1,148.1 298.5,149.8 295.9,150.3 294.4,153.2 288.7,158.8 285.1,157.4 282.4,157.7 281.7,158.2 281.6,161.1 280.4,158.0 280.4,155.2 282.0,152.1 281.6,149.9 282.5,144.9 281.5,143.0 273.8,138.2 273.4,137.3 277.9,130.2 286.1,125.6 287.2,124.4 287.5,122.3 286.4,111.8 281.5,96.7 279.3,86.1 284.0,87.0 292.7,85.8 295.1,87.6 300.9,84.9 303.7,85.1 304.7,80.6 305.6,79.8 308.9,80.2 311.5,77.7 313.4,76.5 315.1,76.5 316.8,75.2 318.1,77.6 317.7,79.3 318.9,80.0 322.3,79.4 322.2,76.2 321.4,75.2 318.6,74.7 318.4,74.0 319.8,70.0 321.8,66.8 321.8,62.1 322.9,58.5 324.4,55.8 328.8,54.5 330.3,53.2 331.8,49.5 332.6,49.0 338.4,49.3 340.0,46.3 340.0,43.9 336.4,43.5 335.7,42.7 337.8,36.4 338.3,32.7 340.0,32.1")
    Public Shared ReadOnly UkrainaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("340.0,260.3 338.8,262.0 332.8,263.7 332.3,266.5 330.1,270.2 316.9,272.8 312.3,275.0 302.6,275.0 300.1,272.1 298.3,271.6 292.5,272.9 290.2,271.7 289.1,272.0 282.8,270.2 277.9,270.3 274.0,267.4 272.8,267.3 271.2,269.8 267.8,271.6 267.4,269.1 265.7,266.6 264.0,266.8 262.2,265.9 261.0,263.6 257.8,262.1 256.0,257.4 253.6,257.5 253.9,252.4 256.7,248.8 258.5,243.0 261.4,236.8 266.5,238.4 267.3,237.1 264.5,233.7 265.0,229.9 263.5,222.4 264.5,220.3 270.8,211.2 283.6,196.4 288.6,195.4 290.8,191.7 290.9,189.0 290.0,185.7 288.7,183.8 291.1,181.9 290.9,181.1 288.8,179.0 286.5,173.2 282.7,167.5 283.0,164.9 281.6,161.1 281.7,158.2 283.5,157.3 285.1,157.4 288.2,159.0 291.5,156.6 295.9,150.3 305.5,149.3 309.3,148.3 325.6,148.9 340.0,152.9")
    Public Shared ReadOnly RosjaWielokaty As PunktMapyGalerii()() = ParsujWielokaty("231.4,44.7 224.5,53.9 230.0,56.1 232.1,56.3 235.8,55.3 236.6,45.2 239.6,44.8 242.7,47.3 252.5,51.4 261.9,51.5 263.0,54.2 266.8,57.3 264.1,66.9 264.0,69.0 265.7,73.2 217.1,71.3 205.7,70.1 208.6,67.4 210.5,64.7 212.1,61.1 212.7,55.8 215.2,54.7 220.7,54.8 223.1,53.5 230.3,44.5 231.4,44.7")

    Public Shared ReadOnly WislaOdcinki As PunktMapyGalerii()() = ParsujWielokaty("190.6,210.4 193.0,210.2;191.9,219.1 190.2,213.7 189.9,211.6 190.6,210.4;193.0,210.2 195.5,209.6 198.0,206.8 200.8,206.7 205.4,208.1 210.4,207.9 223.1,203.8 225.4,202.3 227.2,200.0 230.0,198.6 233.7,198.0 237.7,196.2 241.8,193.1 244.9,190.0 247.0,186.8 247.8,181.9 247.0,171.3 247.4,169.7 249.9,166.8 250.4,165.2 249.9,163.4 247.9,161.6 247.8,160.4 246.1,158.9 242.7,157.1 240.7,155.3 240.3,153.5 236.7,148.7 237.2,147.9 237.1,146.4 236.4,144.1 231.7,136.8 229.2,134.3 227.1,133.3 223.1,133.2 217.4,134.1 212.5,133.2 206.4,129.2 198.1,126.3 193.5,122.7 189.8,117.5 185.5,114.1 180.5,112.5 179.0,109.9 181.2,106.2 184.0,103.4 187.5,101.5 189.3,99.2 190.0,94.4 191.1,93.1 191.8,91.2 191.8,85.9 191.2,83.0 191.4,80.3 192.6,78.0 193.8,73.5")
    Public Shared ReadOnly Wisla As PunktMapyGalerii() = WislaOdcinki(WislaOdcinki.Length - 1)
    Public Shared ReadOnly OdraOdcinki As PunktMapyGalerii()() = ParsujWielokaty("168.0,216.2 172.4,218.8 174.2,219.3 177.9,216.5 181.1,211.6 181.3,208.9 180.0,206.4 179.6,203.8 180.0,201.3 178.8,198.2 175.9,194.4 174.5,191.6 174.1,187.8 173.0,185.8 170.7,183.9 164.8,180.8 163.1,179.4 161.4,176.4 160.0,175.0 156.6,173.1 153.8,170.1 151.3,169.3 148.2,169.8 146.3,169.6 145.7,167.4 146.2,166.2 146.1,161.1 145.0,158.7 143.2,156.9 141.5,156.2 140.1,156.4 134.4,154.1 132.9,152.0 133.3,149.5 132.7,147.4 131.3,145.9 126.4,144.8 113.8,143.8 112.7,142.8 112.8,139.8 112.3,138.5 110.3,136.5 109.9,135.1 111.1,129.8 109.1,126.2 104.2,122.0 101.8,119.0 102.0,117.3 103.0,115.8 104.9,114.4 106.4,112.0 107.8,106.7 110.4,101.5 111.1,99.0 110.4,95.4")
    Public Shared ReadOnly Odra As PunktMapyGalerii() = OdraOdcinki(0)

    Public Shared ReadOnly Miasta As MiastoMapyGalerii() = {
        New MiastoMapyGalerii("Warszawa", 232.4F, 139.1F),
        New MiastoMapyGalerii("Kraków", 212.0F, 206.2F),
        New MiastoMapyGalerii("Gdańsk", 187.5F, 73.4F),
        New MiastoMapyGalerii("Wrocław", 157.0F, 173.8F),
        New MiastoMapyGalerii("Poznań", 154.9F, 133.6F)
    }

    Public Shared ReadOnly MiastaTrueOrtho As MiastoMapyGalerii() = {
        New MiastoMapyGalerii("Warszawa", 232.4F, 139.1F),
        New MiastoMapyGalerii("Kraków", 212.0F, 206.2F),
        New MiastoMapyGalerii("Trójmiasto", 187.5F, 73.4F),
        New MiastoMapyGalerii("Wrocław", 157.0F, 173.8F),
        New MiastoMapyGalerii("Poznań", 154.9F, 133.6F),
        New MiastoMapyGalerii("Łódź", 202.9F, 153.6F),
        New MiastoMapyGalerii("GOP / Katowice", 194.7F, 199.9F)
    }

    Private Shared Function ParsujWielokaty(ciag As String) As PunktMapyGalerii()()
        Dim grupy = ciag.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim wynik As New List(Of PunktMapyGalerii())(grupy.Length)
        For Each g In grupy
            Dim pts = ParsujPunkty(g)
            If pts.Length >= 2 Then wynik.Add(pts)
        Next
        Return wynik.ToArray()
    End Function

    Private Shared Function ParsujPunkty(ciag As String) As PunktMapyGalerii()
        Dim czesci = ciag.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim lista As New List(Of PunktMapyGalerii)(czesci.Length)
        For Each c In czesci
            Dim xy = c.Split(","c)
            If xy.Length = 2 Then
                Dim x = Single.Parse(xy(0), CultureInfo.InvariantCulture)
                Dim y = Single.Parse(xy(1), CultureInfo.InvariantCulture)
                lista.Add(New PunktMapyGalerii(x, y))
            End If
        Next
        Return lista.ToArray()
    End Function

    ''' <summary>Przelicza współrzędne WGS84 (długość, szerokość) na układ mapy poglądowej.</summary>
    Public Shared Function GeoDoMapy(lon As Double, lat As Double) As PunktMapyGalerii
        Dim x = CSng(PrzesuniecieX + (lon - LonMin) * CosLat0 * Skala)
        Dim y = CSng(PrzesuniecieY + (LatMax - lat) * Skala)
        Return New PunktMapyGalerii(x, y)
    End Function

    ''' <summary>
    ''' Dekoduje 140-znakowy zapis <c>maska_hex</c> (siatka 28×20 komórek w zakresie 14.0..24.2° E, 49.0..55.0° N)
    ''' na prostokąty komórek gotowe do narysowania na mapie zasięgu.
    ''' </summary>
    Public Shared Function DekodujMaskeHex(maskaHex As String) As List(Of ProstokatMapyGalerii)
        Dim komorki As New List(Of ProstokatMapyGalerii)()
        If String.IsNullOrWhiteSpace(maskaHex) OrElse maskaHex.Length <> 140 Then Return komorki

        Const dLon As Double = 10.2 / LiczbaKolumnMaski
        Const dLat As Double = 6.0 / LiczbaWierszyMaski

        For b As Integer = 0 To 69
            Dim bajt As Integer
            If Not Integer.TryParse(maskaHex.AsSpan(b * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, bajt) Then
                Continue For
            End If
            For bit As Integer = 0 To 7
                If (bajt And (1 << (7 - bit))) <> 0 Then
                    Dim idx As Integer = b * 8 + bit
                    Dim r As Integer = idx \ LiczbaKolumnMaski
                    Dim c As Integer = idx Mod LiczbaKolumnMaski
                    Dim lon1 As Double = 14.0 + c * dLon
                    Dim lon2 As Double = lon1 + dLon
                    Dim latTop As Double = 55.0 - r * dLat
                    Dim latBottom As Double = latTop - dLat
                    Dim p1 = GeoDoMapy(lon1, latTop)
                    Dim p2 = GeoDoMapy(lon2, latBottom)
                    komorki.Add(New ProstokatMapyGalerii(
                        Math.Min(p1.X, p2.X),
                        Math.Min(p1.Y, p2.Y),
                        Math.Abs(p2.X - p1.X) + 0.35F,
                        Math.Abs(p2.Y - p1.Y) + 0.35F))
                End If
            Next
        Next
        Return komorki
    End Function

    ''' <summary>Opis szczegółowy zasięgu wyświetlany pod mapą poglądową.</summary>
    Public Shared Function OpisZasiegu(p As PozycjaGaleriiWarstw) As String
        Dim z = If(p?.Zasieg, New ZasiegWarstwyGalerii())
        If Not String.IsNullOrWhiteSpace(z.Opis) Then Return z.Opis
        Dim typ = If(z.Typ, "kraj").ToLowerInvariant()
        Select Case typ
            Case "kraj"
                Return "Warstwa obejmuje obszar całej Polski (pokrycie ogólnokrajowe)."
            Case "zagranica"
                Return "Urzędowa warstwa zagraniczna (" & If(Not String.IsNullOrWhiteSpace(z.Kod), z.Kod, "sąsiedzi") & ")."
            Case "morski"
                Return "Polskie obszary morskie, wyłączna strefa ekonomiczna (WZE) lub dane batymetryczne Bałtyku."
            Case "czesciowy"
                Return "Pokrycie częściowe — warstwa dostępna wyłącznie dla wybranych obszarów kraju."
            Case "regionalny"
                If z.Bbox IsNot Nothing AndAlso z.Bbox.Length = 4 Then
                    Return "Zasięg regionalny ograniczony do wybranego obszaru."
                End If
                Return "Zasięg regionalny obejmujący wybrane obszary."
            Case "globalny"
                Return "Warstwa o zasięgu globalnym (pokrycie całego świata)."
            Case Else
                Return If(Not String.IsNullOrWhiteSpace(z.Etykieta), z.Etykieta, "Cała Polska")
        End Select
    End Function
End Class
