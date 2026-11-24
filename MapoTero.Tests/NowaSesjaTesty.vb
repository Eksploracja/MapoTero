'Copyright (C) <2015-2026>  pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports MapoTero.Core
Imports Xunit

''' <summary>Polecenie „Nowa sesja” okna głównego: przycisk, pozycja menu i wyczyszczenie stanu bieżącej sesji.</summary>
<Collection("OknaKreatora")>
<Trait("Kategoria", "UI")>
Public Class NowaSesjaTesty

    Private Shared Sub WWatkuSta(akcja As Action)
        OknoTestowe.WykonywajNaSta(akcja)
    End Sub

    Private Shared Function Kontrolka(Of T As Control)(f As Form, nazwa As String) As T
        Return DirectCast(f.Controls.Find(nazwa, True)(0), T)
    End Function

    <Fact>
    Public Sub PolecenieNowejSesjiJestWMenuSesja()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim menu = Kontrolka(Of MenuStrip)(f, "MenuStrip1")
                    Dim sesja = menu.Items.OfType(Of ToolStripMenuItem)().First(Function(m) m.Name = "SesjaToolStripMenuItem")
                    Assert.Equal("Nowa sesja", sesja.DropDownItems(0).Text)
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub NowaSesjaCzysciStanBiezacejSesji()
        WWatkuSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim typ = GetType(MapoTero.Form1)
                    Dim flagi = BindingFlags.NonPublic Or BindingFlags.Instance

                    'stan sesji „w trakcie pracy”: warstwy, zasięg, piksel, rozwinięte grupy, wyszukiwanie, powiększona mapa, folder sesji
                    Dim zestaw As New ZestawWarstw()
                    zestaw.Dodaj("W1", 100, "", "ortofotomapa")
                    typ.GetField("_zestaw", flagi).SetValue(f, zestaw)
                    typ.GetMethod("PoZmianieWarstw", flagi).Invoke(f, New Object() {0})
                    Kontrolka(Of TextBox)(f, "txtXDol").Text = "480000"
                    Kontrolka(Of TextBox)(f, "txtYLewy").Text = "630000"
                    Kontrolka(Of TextBox)(f, "txtXGora").Text = "481000"
                    Kontrolka(Of TextBox)(f, "txtYPrawy").Text = "631000"
                    Kontrolka(Of TextBox)(f, "txtBokSegmentu").Text = "1000"
                    Kontrolka(Of TextBox)(f, "txtRozmiarPiksela").Text = "0.25"
                    Dim tv = Kontrolka(Of TreeView)(f, "tvWarstwy")
                    For Each n As TreeNode In tv.Nodes
                        n.ExpandAll()
                    Next
                    Dim mapa As Object = f.Controls.Find("mapa", True)(0)
                    mapa.GetType().GetProperty("Zoom").SetValue(mapa, 14.0)
                    f.FolderSesji = MapoTero.FolderDownload & "sesja_test\"
                    Application.DoEvents()

                    f.NowaSesja(pytaj:=False)
                    Application.DoEvents()

                    Assert.Equal(0, Kontrolka(Of MapoTero.ListaWarstw)(f, "lstWybraneWarstwy").Zestaw.Liczba)
                    For Each nazwa In {"txtXDol", "txtYLewy", "txtXGora", "txtYPrawy"}
                        Assert.Equal("", Kontrolka(Of TextBox)(f, nazwa).Text)
                    Next
                    Assert.Equal("2000", Kontrolka(Of TextBox)(f, "txtBokSegmentu").Text)
                    Assert.Equal("1", Kontrolka(Of TextBox)(f, "txtRozmiarPiksela").Text)
                    Assert.Equal("", Kontrolka(Of TextBox)(f, "txtSzukajWarstwy").Text)
                    Assert.Equal(MapoTero.FolderDownload, f.FolderSesji)
                    Assert.Equal(6.0, CDbl(mapa.GetType().GetProperty("Zoom").GetValue(mapa)))
                    Dim pozycja As Object = mapa.GetType().GetProperty("Position").GetValue(mapa)
                    Assert.Equal(52.3, CDbl(pozycja.GetType().GetProperty("Lat").GetValue(pozycja)), 1)
                    Assert.Null(typ.GetField("_korytarz", flagi).GetValue(f))
                    Assert.Null(typ.GetField("_dzialkaKatastralna", flagi).GetValue(f))

                    'drzewo: rozwinięte są tylko grupy prowadzące do skanów map topograficznych GUGiK
                    If tv.SelectedNode IsNot Nothing Then
                        For Each wezel In WszystkieWezly(tv.Nodes)
                            If Not wezel.IsExpanded Then Continue For
                            Assert.True(tv.SelectedNode Is wezel OrElse tv.SelectedNode.FullPath.StartsWith(wezel.FullPath & tv.PathSeparator),
                                        "rozwinięta grupa poza ścieżką domyślnego zbioru: " & wezel.FullPath)
                        Next
                    End If
                End Using
            End Sub)
    End Sub

    Private Shared Iterator Function WszystkieWezly(wezly As TreeNodeCollection) As IEnumerable(Of TreeNode)
        For Each n As TreeNode In wezly
            Yield n
            For Each d In WszystkieWezly(n.Nodes)
                Yield d
            Next
        Next
    End Function

End Class
