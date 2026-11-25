'Copyright (C) <2026>  MapoTero
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports Xunit

''' <summary>Poprawki z audytu ergonomii interfejsu (docs-ai/audyt/07-ergonomia.md, ER-01…ER-34) w oknie głównym.</summary>
<Collection("OknaKreatora")>
<Trait("Kategoria", "UI")>
Public Class ErgonomiaInterfejsuTesty

    Private Const Prywatne As BindingFlags = BindingFlags.Instance Or BindingFlags.NonPublic

    Private Shared Function Pole(Of T)(f As MapoTero.Form1, nazwa As String) As T
        Return DirectCast(GetType(MapoTero.Form1).GetField(nazwa, Prywatne).GetValue(f), T)
    End Function

    <Fact>
    Public Sub BladCzynnosciWidacWKarcieStanuSesji()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    'start pobierania bez warstw z menu (F5) - błąd w karcie „Stan sesji”, nie tylko w zakładce Komunikaty (ER-01)
                    GetType(MapoTero.Form1).GetMethod("PobierzZMenu", Prywatne).Invoke(f, Nothing)
                    Dim naglowek = Pole(Of Label)(f, "_podpowiedzBelki")
                    Dim szczegoly = Pole(Of Label)(f, "_szczegolyStanuBelki")
                    Assert.Equal("Brak warstwy", naglowek.Text)
                    Assert.Contains("katalogu warstw", szczegoly.Text)
                    Assert.Equal(Drawing.Color.Firebrick, szczegoly.ForeColor)
                    'zmiana parametrów usuwa błąd z karty
                    f.PrzeliczSiatke()
                    Assert.Equal("", f.BladSesji)
                    Assert.NotEqual("Brak warstwy", naglowek.Text)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub PobranieZBrakamiZostawiaPrzyciskEksportu()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim eksport = DirectCast(f.Controls.Find("btnPrzejdzDoEksportu", True)(0), Button)
                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {.Wszystkie = 10, .Nieudane = 3, .Folder = "C:\dummy"}
                    f.AktualizujStanBelki()
                    'eksport z białymi polami dostępny także przy brakach (ER-05), ale bez wyróżnienia
                    Assert.True(eksport.Visible)
                    Assert.False(f._podswietlEksport)
                    f._ostatniWynikPobierania = New MapoTero.Form1.WynikPobieraniaInfo With {.Wszystkie = 10, .Nieudane = 10, .Folder = "C:\dummy"}
                    f.AktualizujStanBelki()
                    Assert.False(eksport.Visible)
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub MenuSesjiMaGlowneCzynnosciISkroty()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim sesja = DirectCast(f.MainMenuStrip.Items.Find("SesjaToolStripMenuItem", False)(0), ToolStripMenuItem)
                    Dim pozycje = sesja.DropDownItems.OfType(Of ToolStripMenuItem)().ToList()
                    Dim pobierz = pozycje.Single(Function(p) p.Name = "PobierzToolStripMenuItem")
                    Assert.Equal(Keys.F5, pobierz.ShortcutKeys)
                    Assert.Contains(pozycje, Function(p) p.Name = "OstatnieSesjeToolStripMenuItem")
                    Assert.Contains(pozycje, Function(p) p.Name = "ZaznaczObszarToolStripMenuItem")
                    Assert.Contains(pozycje, Function(p) p.Name = "EksportToolStripMenuItem" AndAlso p.ShortcutKeys = (Keys.Control Or Keys.E))
                    'zapis i odczyt sesji pod Ctrl+S / Ctrl+O zamiast Alt+Z / Alt+W, które kolidują z obsługą menu z klawiatury (ER-06)
                    Assert.DoesNotContain(pozycje, Function(p) (p.ShortcutKeys And Keys.Alt) = Keys.Alt)
                    Assert.Contains(pozycje, Function(p) p.ShortcutKeys = (Keys.Control Or Keys.O))
                    Assert.Contains(pozycje, Function(p) p.ShortcutKeys = (Keys.Control Or Keys.S))
                    Assert.Contains(pozycje, Function(p) p.ShortcutKeys = (Keys.Control Or Keys.N))
                    f.Close()
                End Using
            End Sub)
    End Sub

    <Fact>
    Public Sub KartaParametrowPokazujeFormatSegmentow()
        OknoTestowe.WykonywajNaSta(
            Sub()
                Using f As New MapoTero.Form1()
                    f.Show()
                    Application.DoEvents()
                    Dim pole = DirectCast(f.Controls.Find("cmbFormatSegmentow", True)(0), ComboBox)
                    Assert.False(DirectCast(f.Controls.Find("cmbFormatDocelowy", True)(0), ComboBox).Visible)
                    'wybór PNG w karcie zmienia format sesji (ER-03, ER-04)
                    Dim png = Array.FindIndex(MapoTero.Form1.OpisyFormatowSegmentow, Function(x) x.Format = "png")
                    pole.SelectedIndex = png
                    Assert.Equal("png", MapoTero.Ustawienia.Format)
                    Assert.Equal(".png", f.stFormat.Text)
                    'zmiana formatu z zewnątrz (okno Ustawienia, conf.txt) widoczna w polu
                    MapoTero.Ustawienia.Format = "jpeg"
                    f.stFormat.Text = ".jpeg"
                    Assert.Equal("jpeg", MapoTero.Form1.OpisyFormatowSegmentow(pole.SelectedIndex).Format)
                    f.Close()
                End Using
            End Sub)
    End Sub
End Class
