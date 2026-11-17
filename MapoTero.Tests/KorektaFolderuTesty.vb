'Copyright (C) <2026>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

Public Class KorektaFolderuTesty

    ''' <summary>
    ''' Kopiowane pliki georeferencji obejmują world file, które zapisuje sam program. (Błąd: brakowało .jpgw i .tifw,
    ''' więc poprawione segmenty JPEG i TIFF traciły położenie.)
    ''' </summary>
    <Theory>
    <InlineData(".jpg")>
    <InlineData(".png")>
    <InlineData(".tif")>
    <InlineData(".gif")>
    Public Sub KopiujeWorldFileProgramu(rozszerzenieObrazu As String)
        Dim worldFile = "." & Georeferencja.RozszerzenieWorldFile(rozszerzenieObrazu)
        Assert.Contains(worldFile, MapoTero.FormKorektaFolderu.RozszerzeniaGeoreferencji)
    End Sub

    ''' <summary>
    ''' Folder podany przed otwarciem okna (z eksportu do CAD) jest folderem źródłowym. (Błąd: Load nadpisywał
    ''' wpisany wcześniej folder folderem bieżącej sesji.)
    ''' </summary>
    <Fact>
    Public Sub FolderPoczatkowyJestFolderemZrodlowym()
        Dim folder = Path.Combine(Path.GetTempPath(), "mapotero_korekta_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Dim zrodlo As String = Nothing, cel As String = Nothing
        Dim blad As Exception = Nothing
        Try
            Dim t As New Thread(
                Sub()
                    Try
                        Using f As New MapoTero.FormKorektaFolderu() With {.FolderPoczatkowy = folder}
                            f.StartPosition = FormStartPosition.Manual
                            f.Show()
                            Application.DoEvents()
                            zrodlo = f.txtFolderZrodlo.Text
                            cel = f.txtFolderCel.Text
                            f.Close()
                        End Using
                    Catch ex As Exception
                        blad = ex
                    End Try
                End Sub)
            t.SetApartmentState(ApartmentState.STA)
            t.Start()
            t.Join()
            If blad IsNot Nothing Then Throw blad
            Assert.Equal(folder, zrodlo)
            Assert.Equal(Path.Combine(folder, "korekta"), cel)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

End Class
