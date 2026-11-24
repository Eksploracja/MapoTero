Option Strict On
Option Explicit On

Imports System.Runtime.ExceptionServices
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>
''' Jednorazowa konfiguracja środowiska testów okien: wyłącza ruch sieciowy w formularzach WinForms
''' oraz utrzymuje wspólny, żywy wątek STA na osobnym, ukrytym pulpicie Win32 (CreateDesktop + SetThreadDesktop),
''' żeby okna testowe i SystemEvents nie zakłócały pulpitu użytkownika ani klikania w Windows.
''' </summary>
Public NotInheritable Class OknaKreatoraFixture
    Implements IDisposable

    Public Sub New()
        Global.MapoTero.Form1.TrybTestowyBezSieci = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        OknoTestowe.Zakoncz()
    End Sub

End Class

<CollectionDefinition("OknaKreatora")>
Public Class OknaKreatoraKolekcja
    Implements ICollectionFixture(Of OknaKreatoraFixture)
End Class

Friend Module OknoTestowe

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TRANSPARENT As Long = &H20L
    Private Const WS_EX_TOOLWINDOW As Long = &H80L
    Private Const WS_EX_NOACTIVATE As Long = &H8000000L
    Private Const GENERIC_ALL As UInteger = &H10000000UI

    <DllImport("user32.dll", EntryPoint:="GetWindowLongPtrW")>
    Private Function GetWindowLongPtr(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongPtrW")>
    Private Function SetWindowLongPtr(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function CreateDesktop(lpszDesktop As String, lpszDevice As IntPtr, pDevmode As IntPtr,
                                   dwFlags As UInteger, dwDesiredAccess As UInteger, lpsa As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Function SetThreadDesktop(hDesktop As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Function CloseDesktop(hDesktop As IntPtr) As Boolean
    End Function

    Private ReadOnly _blokada As New Object()
    Private _watekSta As Thread
    Private _kotwica As Control
    Private _hPulpit As IntPtr = IntPtr.Zero

    Private Sub UpewnijSieZeWatekDziala()
        If _kotwica IsNot Nothing AndAlso _watekSta IsNot Nothing AndAlso _watekSta.IsAlive Then Exit Sub
        SyncLock _blokada
            If _kotwica IsNot Nothing AndAlso _watekSta IsNot Nothing AndAlso _watekSta.IsAlive Then Exit Sub
            If Environment.GetEnvironmentVariable("MAPOTERO_AUDYT") <> "1" Then
                Try
                    _hPulpit = CreateDesktop("MapoTero_Testy_" & Guid.NewGuid().ToString("N"),
                                             IntPtr.Zero, IntPtr.Zero, 0UI, GENERIC_ALL, IntPtr.Zero)
                Catch
                    _hPulpit = IntPtr.Zero
                End Try
            End If

            Using gotowy As New ManualResetEventSlim(False)
                Dim startBlad As Exception = Nothing
                _watekSta = New Thread(
                    Sub()
                        Try
                            If _hPulpit <> IntPtr.Zero Then
                                If Not SetThreadDesktop(_hPulpit) Then
                                    CloseDesktop(_hPulpit)
                                    _hPulpit = IntPtr.Zero
                                End If
                            End If
                            Dim k As New Control()
                            Dim __ = k.Handle
                            _kotwica = k
                            gotowy.Set()
                            Application.Run()
                        Catch ex As Exception
                            startBlad = ex
                            gotowy.Set()
                        End Try
                    End Sub)
                _watekSta.IsBackground = True
                _watekSta.SetApartmentState(ApartmentState.STA)
                _watekSta.Start()
                gotowy.Wait()
                If startBlad IsNot Nothing Then ExceptionDispatchInfo.Capture(startBlad).Throw()
            End Using
        End SyncLock
    End Sub

    ''' <summary>
    ''' Wykonuje kod testu UI na wspólnym, żywym wątku STA z pętlą komunikatów i (gdy dostępne) na osobnym ukrytym pulpicie.
    ''' Zapobiega osierocaniu okna SystemEvents na martwych wątkach oraz blokowaniu pulpitu roboczego.
    ''' </summary>
    Friend Sub WykonywajNaSta(akcja As Action)
        Global.MapoTero.Form1.TrybTestowyBezSieci = True
        UpewnijSieZeWatekDziala()
        Dim blad As Exception = Nothing
        _kotwica.Invoke(
            Sub()
                Try
                    akcja()
                Catch ex As Exception
                    blad = ex
                End Try
            End Sub)
        If blad IsNot Nothing Then ExceptionDispatchInfo.Capture(blad).Throw()
    End Sub

    Friend Sub Zakoncz()
        SyncLock _blokada
            If _kotwica IsNot Nothing AndAlso _watekSta IsNot Nothing AndAlso _watekSta.IsAlive Then
                Try
                    _kotwica.Invoke(Sub() Application.ExitThread())
                    _watekSta.Join(5000)
                Catch
                End Try
            End If
            Try : _kotwica?.Dispose() : Catch : End Try
            _kotwica = Nothing
            _watekSta = Nothing
            If _hPulpit <> IntPtr.Zero Then
                Try : CloseDesktop(_hPulpit) : Catch : End Try
                _hPulpit = IntPtr.Zero
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Pokazuje formularz testowy w sposób niewidoczny i nieprzechwytujący fokusu ani kliknięć myszy
    ''' (nie blokuje pracy na pulpicie Windows w trakcie testów UI).
    ''' </summary>
    Friend Sub Pokaz(f As Form)
        f.ShowInTaskbar = False
        f.Opacity = 0
        AddHandler f.HandleCreated,
            Sub()
                Dim styl = GetWindowLongPtr(f.Handle, GWL_EXSTYLE).ToInt64()
                SetWindowLongPtr(f.Handle, GWL_EXSTYLE, New IntPtr(styl Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW Or WS_EX_TRANSPARENT))
            End Sub
        f.Show()
    End Sub

End Module
