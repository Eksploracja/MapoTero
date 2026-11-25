'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core
Imports Xunit

''' <summary>Nazwa pliku wynikowego okna Eksport mapy (audyt ER-10).</summary>
Public Class NazwaPlikuEksportuTesty

    <Fact>
    Public Sub NazwaSesji_BezPrzedrostkaSesji()
        Assert.Equal("ortofotomapa_2026-11-25_101233", NazwaPlikuEksportu.NazwaSesji("C:\MapoTero\download\sesja_ortofotomapa_2026-11-25_101233\"))
        Assert.Equal("Projekt Gdańsk", NazwaPlikuEksportu.NazwaSesji("D:\GIS\Projekt Gdańsk"))
    End Sub

    <Fact>
    Public Sub NazwaSesji_FolderPobranLubPusty_Mapa()
        Assert.Equal("mapa", NazwaPlikuEksportu.NazwaSesji("C:\MapoTero\download\"))
        Assert.Equal("mapa", NazwaPlikuEksportu.NazwaSesji(""))
        Assert.Equal("mapa", NazwaPlikuEksportu.NazwaSesji("C:\x\sesja_\"))
    End Sub

    <Fact>
    Public Sub NazwaDomyslna_KafleOdSesji_ArkuszZPrzedrostkiem()
        Dim folder = "C:\d\sesja_topo_2026-11-25_100000\"
        Assert.Equal("topo_2026-11-25_100000.mbtiles",
                     NazwaPlikuEksportu.NazwaDomyslna(folder, RodzajEksportu.MBTiles, FormatArkusza.GeoTiff, 6, 4, "_scalone_segmenty_"))
        Assert.Equal("topo_2026-11-25_100000_garmin.kmz",
                     NazwaPlikuEksportu.NazwaDomyslna(folder, RodzajEksportu.KmzGarmin, FormatArkusza.GeoTiff, 6, 4, "_scalone_segmenty_"))
        Assert.Equal("_scalone_segmenty_6x4.jpg",
                     NazwaPlikuEksportu.NazwaDomyslna(folder, RodzajEksportu.GeoTiff, FormatArkusza.Jpeg, 6, 4, "_scalone_segmenty_"))
    End Sub

    <Theory>
    <InlineData("D:\mapy\Tatry.mbtiles", "Tatry")>
    <InlineData("D:\mapy\Tatry_garmin.kmz", "Tatry")>
    <InlineData("D:\mapy\Tatry_oruxmaps", "Tatry")>
    <InlineData("D:\mapy\Tatry.v2.jnx", "Tatry.v2")>
    <InlineData("D:\mapy\Tatry", "Tatry")>
    Public Sub PodstawaZeSciezki_OdcinaKoncowkeFormatu(sciezka As String, oczekiwana As String)
        Assert.Equal(oczekiwana, NazwaPlikuEksportu.PodstawaZeSciezki(sciezka))
    End Sub
End Class
