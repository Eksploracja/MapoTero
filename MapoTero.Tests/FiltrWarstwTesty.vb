'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class FiltrWarstwTesty

    <Fact>
    Public Sub UproszczenieBezPolskichZnakowIPodkreslen()
        Assert.Equal("dzialki ewid egib", FiltrWarstw.Uprosc("Działki_ewid-EGIB"))
        Assert.Equal("zolta laka", FiltrWarstw.Uprosc("ŻÓŁTA ŁĄKA"))
        Assert.Equal("", FiltrWarstw.Uprosc(Nothing))
    End Sub

    <Fact>
    Public Sub PustaFrazaPasujeDoWszystkiego()
        Assert.True(FiltrWarstw.Pasuje("ortofotomapa", "Raster", ""))
        Assert.True(FiltrWarstw.Pasuje("ortofotomapa", "Raster", "   "))
    End Sub

    <Fact>
    Public Sub FrazaWNazwieWarstwyLubZbioru()
        Assert.True(FiltrWarstw.Pasuje("dzialki_ewid_EGIB", "numery_dzialek", "działek"))
        Assert.True(FiltrWarstw.Pasuje("mapy_historyczne_IHPAN", "AHP", "mapy historyczne"))
        Assert.False(FiltrWarstw.Pasuje("ortofotomapa", "Raster", "dzialki"))
    End Sub

    <Fact>
    Public Sub KazdeSlowoMusiWystapic()
        Assert.True(FiltrWarstw.Pasuje("ortofotomapa_WMTS", "ORTOFOTOMAPA", "orto wmts"))
        Assert.False(FiltrWarstw.Pasuje("ortofotomapa", "Raster", "orto wmts"))
    End Sub

End Class
