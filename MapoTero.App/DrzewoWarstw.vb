'Copyright (C) <2015>  pajakt

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Runtime.InteropServices
Imports MapoTero.Core

''' <summary>Zbiór map wczytany do drzewa warstw: nazwa (plik w folderze warstwy) i zawartość pliku (Nothing - błąd odczytu).</summary>
Friend Class ZbiorMapy
    Public Nazwa As String = ""
    Public Plik As PlikWarstw
    Public Blad As String = ""
End Class

''' <summary>Warstwa w drzewie (znacznik Tag węzła): zbiór map, nazwa warstwy na serwerze i nazwa wyświetlana z katalogu.</summary>
Friend Class WarstwaWDrzewie
    Public Zbior As ZbiorMapy
    Public Warstwa As String = ""
    Public Nazwa As String = ""
End Class

''' <summary>
''' Drzewo „Wybierz mapy”: grupy tematyczne i podgrupy (z katalogu warstw, Tag - nazwa grupy) bez pól wyboru oraz
''' warstwy (Tag - <see cref="WarstwaWDrzewie"/>) z polami wyboru. Zwykły TreeView z CheckBoxes pokazuje pola przy
''' wszystkich węzłach i ma pułapki, obsłużone tutaj:
''' * pole wyboru węzła ukrywa się tylko komunikatem TVM_SETITEM (indeks obrazu stanu 0); kliknięcie w miejsce pola
'''   albo spacja przywróciłyby je w kontrolce systemowej, więc są przechwytywane,
''' * podwójne kliknięcie pola wyboru zmienia jego wygląd bez zdarzenia AfterCheck (pole i lista wybranych warstw
'''   rozjeżdżały się) - zamieniane jest na zwykłe kliknięcie,
''' * podwójne kliknięcie nazwy grupy jest pomijane, bo okno rozwija grupę już pojedynczym kliknięciem.
''' </summary>
Public Class DrzewoWarstw
    Inherits TreeView

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_CHAR As Integer = &H102
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_LBUTTONDBLCLK As Integer = &H203
    Private Const TVM_SETITEMW As Integer = &H1100 + 63
    Private Const WM_HSCROLL As Integer = &H114
    Private Const SB_LEFT As Integer = 6
    Private Const TVIF_STATE As Integer = &H8
    Private Const TVIS_STATEIMAGEMASK As Integer = &HF000

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure TVITEM
        Public mask As Integer
        Public hItem As IntPtr
        Public state As Integer
        Public stateMask As Integer
        Public pszText As IntPtr
        Public cchTextMax As Integer
        Public iImage As Integer
        Public iSelectedImage As Integer
        Public cChildren As Integer
        Public lParam As IntPtr
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, ByRef lParam As TVITEM) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="SendMessageW")>
    Private Shared Function SendMessageProsty(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Public Sub New()
        CheckBoxes = True
        ShowNodeToolTips = True
        HideSelection = False
        'trzy poziomy (grupa, podgrupa, warstwa) w wąskim panelu - mniejsze wcięcie niż domyślne 19 px
        Indent = 12
    End Sub

    ''' <summary>
    ''' Przewija drzewo do węzła i z powrotem do lewej krawędzi - EnsureVisible przewija też w poziomie do początku
    ''' tekstu głębokiego węzła, przez co nazwy grup były ucięte z lewej.
    ''' </summary>
    Public Sub PokazWezel(wezel As TreeNode)
        If wezel Is Nothing Then Exit Sub
        wezel.EnsureVisible()
        If IsHandleCreated Then SendMessageProsty(Handle, WM_HSCROLL, New IntPtr(SB_LEFT), IntPtr.Zero)
    End Sub

    ''' <summary>Czy węzeł jest grupą (każdy węzeł, który nie jest warstwą).</summary>
    Friend Shared Function CzyGrupa(wezel As TreeNode) As Boolean
        Return wezel IsNot Nothing AndAlso TypeOf wezel.Tag IsNot WarstwaWDrzewie
    End Function

    ''' <summary>Warstwa węzła (Nothing - grupa).</summary>
    Friend Shared Function WarstwaWezla(wezel As TreeNode) As WarstwaWDrzewie
        Return TryCast(wezel?.Tag, WarstwaWDrzewie)
    End Function

    ''' <summary>Wszystkie węzły drzewa (w głąb, w kolejności wyświetlania).</summary>
    Friend Iterator Function WszystkieWezly(Optional wezly As TreeNodeCollection = Nothing) As IEnumerable(Of TreeNode)
        For Each wezel As TreeNode In If(wezly, Nodes)
            Yield wezel
            For Each d In WszystkieWezly(wezel.Nodes)
                Yield d
            Next
        Next
    End Function

    ''' <summary>Ukrywa pola wyboru grup - wywołaj po dodaniu węzłów (wymaga utworzonego uchwytu okna).</summary>
    Public Sub UkryjPolaGrup()
        If Not IsHandleCreated Then Exit Sub
        For Each wezel In WszystkieWezly().Where(Function(w) CzyGrupa(w))
            Dim item As New TVITEM With {.mask = TVIF_STATE, .hItem = wezel.Handle, .stateMask = TVIS_STATEIMAGEMASK, .state = 0}
            SendMessage(Handle, TVM_SETITEMW, IntPtr.Zero, item)
        Next
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        UkryjPolaGrup()
    End Sub

    Protected Overrides Sub OnBeforeCheck(e As TreeViewCancelEventArgs)
        If CzyGrupa(e.Node) Then e.Cancel = True : Exit Sub
        MyBase.OnBeforeCheck(e)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_LBUTTONDOWN, WM_LBUTTONDBLCLK
                Dim p As New Point(CInt(m.LParam.ToInt64() And &HFFFF), CInt((m.LParam.ToInt64() >> 16) And &HFFFF))
                Dim trafienie = HitTest(p)
                If trafienie.Location = TreeViewHitTestLocations.StateImage Then
                    If CzyGrupa(trafienie.Node) Then
                        'miejsce ukrytego pola grupy - rozwinięcie zamiast pojawienia się pola wyboru
                        If m.Msg = WM_LBUTTONDOWN Then trafienie.Node.Toggle()
                        Exit Sub
                    End If
                    If m.Msg = WM_LBUTTONDBLCLK Then m.Msg = WM_LBUTTONDOWN
                ElseIf m.Msg = WM_LBUTTONDBLCLK AndAlso trafienie.Location = TreeViewHitTestLocations.Label AndAlso CzyGrupa(trafienie.Node) Then
                    'nazwę grupy rozwija już pojedyncze kliknięcie (Form1) - systemowe rozwinięcie podwójnym kliknięciem by je cofało
                    Exit Sub
                End If
            Case WM_KEYDOWN, WM_CHAR
                If CInt(m.WParam.ToInt64()) = Keys.Space AndAlso CzyGrupa(SelectedNode) Then
                    If m.Msg = WM_KEYDOWN Then SelectedNode.Toggle()
                    Exit Sub
                End If
        End Select
        MyBase.WndProc(m)
    End Sub

End Class
