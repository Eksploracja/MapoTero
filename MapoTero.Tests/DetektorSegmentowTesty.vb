'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.Text
Imports MapoTero.Core
Imports Xunit

Public Class DetektorSegmentowTesty

    <Fact>
    Public Sub CzyPustyBialy_KafelCzystoBialy_ZwracaTrue()
        ' Kafel 100x100 czysto biały
        Dim piksele(9999) As Integer
        Dim bialy = &HFF000000 Or (&HFF << 16) Or (&HFF << 8) Or &HFF
        For i = 0 To piksele.Length - 1
            piksele(i) = bialy
        Next

        Assert.True(DetektorSegmentow.CzyPustyBialy(piksele))
    End Sub

    <Fact>
    Public Sub CzyPustyBialy_KafelBialyZCienkaLiniaDzialki_ZwracaFalse()
        ' Kafel 100x100 biały z cienką czarną linią granicy działki (np. 10 pikseli na 10000)
        Dim piksele(9999) As Integer
        Dim bialy = &HFF000000 Or (&HFF << 16) Or (&HFF << 8) Or &HFF
        For i = 0 To piksele.Length - 1
            piksele(i) = bialy
        Next

        ' Cienka granica działki - 10 pikseli o kolorze brązowym/czarnym
        Dim czarny = &HFF000000
        For i = 500 To 509
            piksele(i) = czarny
        Next

        ' Detektor ma natychmiast rozpoznać, że kafel ma treść!
        Assert.False(DetektorSegmentow.CzyPustyBialy(piksele))
    End Sub

    <Fact>
    Public Sub MonitorKafliXyz_PowtarzajacaSiePlansza_WykrywaBlokade()
        Dim monitor As New MonitorKafliXyz With {.ProgPowtorzen = 3}

        ' Symulujemy pobranie planszy błędu z serwera (np. obrazek 100 bajtów "Access Blocked")
        Dim planszaBledu = Encoding.UTF8.GetBytes("BLOCKED_TILE_WATERMARK_PNG_SIMULATION_BYTES")
        Dim normalnyKafel1 = Encoding.UTF8.GetBytes("TILE_COORD_10_20_MAP_CONTENT_A")
        Dim normalnyKafel2 = Encoding.UTF8.GetBytes("TILE_COORD_10_21_MAP_CONTENT_B")

        Dim powtorzenia As Integer = 0
        ' Kafel 1 - normalny
        Assert.False(monitor.ZarejestrujISprawdzCzyBlokada(normalnyKafel1, 10, 20, 15, powtorzenia))
        Assert.Equal(1, powtorzenia)

        ' Kafel 2 - normalny
        Assert.False(monitor.ZarejestrujISprawdzCzyBlokada(normalnyKafel2, 10, 21, 15, powtorzenia))
        Assert.Equal(1, powtorzenia)

        ' Kafel 3 - plansza błędu pod współrzędną (15, 30)
        Assert.False(monitor.ZarejestrujISprawdzCzyBlokada(planszaBledu, 15, 30, 15, powtorzenia))
        Assert.Equal(1, powtorzenia)

        ' Kafel 4 - ta sama plansza pod współrzędną (15, 31) -> 2. powtórzenie
        Assert.False(monitor.ZarejestrujISprawdzCzyBlokada(planszaBledu, 15, 31, 15, powtorzenia))
        Assert.Equal(2, powtorzenia)

        ' Kafel 5 - ta sama plansza pod współrzędną (15, 32) -> 3. powtórzenie (osiągnięcie progu!)
        Assert.True(monitor.ZarejestrujISprawdzCzyBlokada(planszaBledu, 15, 32, 15, powtorzenia))
        Assert.Equal(3, powtorzenia)
    End Sub

End Class
