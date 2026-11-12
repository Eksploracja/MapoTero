'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports MapoTero.Core
Imports Xunit

Public Class WersjaProgramuTesty

    <Theory>
    <InlineData(3, 16, 0, 0, "3.16")>
    <InlineData(3, 0, 0, 0, "3.0")>
    <InlineData(0, 0, 0, 0, "0.0")>
    <InlineData(3, 16, 1, 0, "3.16.1")>
    <InlineData(3, 16, 0, 2, "3.16.0.2")>
    <InlineData(3, 16, 1, 2, "3.16.1.2")>
    Public Sub Formatuj_UcinaKoncoweZeraPonadDwaCzlony(major As Integer, minor As Integer, build As Integer, revision As Integer, oczekiwana As String)
        Assert.Equal(oczekiwana, WersjaProgramu.Formatuj(New Version(major, minor, build, revision)))
    End Sub

    <Fact>
    Public Sub Formatuj_ObslugujeWersjeDwuTrzyCzescioweINothing()
        Assert.Equal("", WersjaProgramu.Formatuj(Nothing))
        Assert.Equal("3.16", WersjaProgramu.Formatuj(New Version(3, 16)))
        Assert.Equal("3.16", WersjaProgramu.Formatuj(New Version(3, 16, 0)))
        Assert.Equal("3.16.2", WersjaProgramu.Formatuj(New Version(3, 16, 2)))
    End Sub

    <Fact>
    Public Sub WersjaZestawuIUserAgent_BezKoncowychZer()
        Assert.Equal("", WersjaProgramu.Z(Nothing))
        Assert.Equal("MapoTero", WersjaProgramu.UserAgent(Nothing))

        Dim oczekiwanaApp = WersjaProgramu.Formatuj(GetType(MapoTero.Program).Assembly.GetName().Version)
        Assert.Equal(oczekiwanaApp, MapoTero.Program.Wersja)
        Assert.Equal("MapoTero/" & oczekiwanaApp, MapoTero.Program.UserAgentProgramu)
        Assert.DoesNotContain(".0.0", MapoTero.Program.Wersja)

        Dim oczekiwanaCore = WersjaProgramu.Formatuj(GetType(WersjaProgramu).Assembly.GetName().Version)
        Assert.Equal(oczekiwanaCore, WersjaProgramu.Z(GetType(WersjaProgramu).Assembly))
        Assert.Equal("MapoTero/" & oczekiwanaCore, WersjaProgramu.UserAgent(GetType(WersjaProgramu).Assembly))
    End Sub

End Class
