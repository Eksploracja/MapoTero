'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports MapoTero.Core
Imports Xunit

Public Class OpisBleduTesty

    <Fact>
    Public Sub BrakDostepu_PoPolsku_ZeSciezka()
        Dim opis = OpisBledu.Z(New UnauthorizedAccessException("Access to the path 'C:\Program Files\MapoTero\conf.txt' is denied."))
        Assert.StartsWith("brak uprawnień", opis)
        Assert.EndsWith(": C:\Program Files\MapoTero\conf.txt", opis)
    End Sub

    <Fact>
    Public Sub PlikOtwartyWInnymProgramie()
        Dim plik = Path.GetTempFileName()
        Try
            Using blokada = File.Open(plik, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Dim ex = Assert.ThrowsAny(Of IOException)(Sub() File.ReadAllText(plik))
                Assert.StartsWith("plik jest otwarty w innym programie", OpisBledu.Z(ex))
            End Using
        Finally
            File.Delete(plik)
        End Try
    End Sub

    <Fact>
    Public Sub BrakFolderu_IBrakPliku()
        Dim exF = Assert.ThrowsAny(Of IOException)(Sub() File.ReadAllText("C:\nie_ma_takiego_folderu_mapotero\a.txt"))
        Assert.StartsWith("folder nie istnieje", OpisBledu.Z(exF))
        Dim exP = Assert.ThrowsAny(Of IOException)(Sub() File.ReadAllText(Path.Combine(Path.GetTempPath(), "nie_ma_pliku_mapotero.txt")))
        Assert.StartsWith("plik nie istnieje", OpisBledu.Z(exP))
    End Sub

    <Fact>
    Public Sub LimitCzasuIBledyHttp()
        Assert.StartsWith("serwer nie odpowiedział", OpisBledu.Z(New TaskCanceledException("x", New TimeoutException())))
        Assert.Equal("serwer zwrócił błąd HTTP 503 (ServiceUnavailable)",
                     OpisBledu.Z(New HttpRequestException("x", Nothing, HttpStatusCode.ServiceUnavailable)))
        Assert.Equal("błąd połączenia z serwerem: Nieznany host.",
                     OpisBledu.Z(New HttpRequestException("y", New Exception("Nieznany host."))))
    End Sub

    <Fact>
    Public Sub InneBledy_TrescOryginalna()
        Assert.Equal("Własny opis błędu", OpisBledu.Z(New InvalidOperationException("Własny opis błędu")))
        Assert.Equal("nieznany błąd", OpisBledu.Z(Nothing))
        Assert.Equal("Własny", OpisBledu.Z(New AggregateException(New InvalidOperationException("Własny"))))
    End Sub

End Class
