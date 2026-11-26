'Copyright (C) <2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports Xunit

Public Class PolaSzukaniaTesty
    <Theory>
    <InlineData(96, False, 12.3333F)>
    <InlineData(96, True, 10.6667F)>
    <InlineData(120, False, 13.75F)>
    <InlineData(120, True, 13.3333F)>
    <InlineData(144, False, 16.5F)>
    <InlineData(144, True, 16.0F)>
    Public Sub RozmiarCzcionkiPola_ZalezyOdSkaliISystemu(dpi As Integer, windows11 As Boolean, rozmiarPiksele As Single)
        Dim wynik = MapoTero.PoleWyszukiwaniaMiejsca.RozmiarCzcionkiPola(dpi, 9.25F, windows11)
        Assert.InRange(wynik, rozmiarPiksele - 0.001F, rozmiarPiksele + 0.001F)
    End Sub
End Class
