'Copyright (C) <2015>  pajakt
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes

''' <summary>
''' Dekoder obrazów WebP oparty o wbudowany w system Windows mechanizm WIC (<c>windowscodecs.dll</c>),
''' bez zewnętrznych bibliotek natywnych.
''' </summary>
Public NotInheritable Class DekoderWebp

    Private Sub New()
    End Sub

    Private Shared ReadOnly CLSID_WICImagingFactory As New Guid("CACAF262-9370-4615-A13B-9F5539DA4C0A")
    Private Shared ReadOnly GUID_WICPixelFormat32bppPBGRA As New Guid("6FDDC324-4E03-4BFE-B185-3D77768DC910")
    Private Const WINCODEC_ERR_COMPONENTNOTFOUND As Integer = &H88982F50

    <DllImport("shlwapi.dll", ExactSpelling:=True)>
    Private Shared Function SHCreateMemStream(<[In]> pInit As Byte(), cbInit As UInteger) As IStream
    End Function

    <ComImport, Guid("EC5EC8A9-C395-4314-9C77-54D7A935FF70"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IWICImagingFactory
        Function CreateDecoderFromFilename(<MarshalAs(UnmanagedType.LPWStr)> wzFilename As String, pguidVendor As IntPtr, dwDesiredAccess As UInteger, metadataOptions As UInteger) As IWICBitmapDecoder
        Function CreateDecoderFromStream(pIStream As IStream, pguidVendor As IntPtr, metadataOptions As UInteger) As IWICBitmapDecoder
        Function CreateDecoderFromFileHandle(hFile As IntPtr, pguidVendor As IntPtr, metadataOptions As UInteger) As IWICBitmapDecoder
        Function CreateComponentInfo(ByRef clsidComponent As Guid) As IntPtr
        Function CreateDecoder(ByRef guidContainerFormat As Guid, pguidVendor As IntPtr) As IWICBitmapDecoder
        Function CreateEncoder(ByRef guidContainerFormat As Guid, pguidVendor As IntPtr) As IntPtr
        Function CreatePalette() As IntPtr
        Function CreateFormatConverter() As IWICFormatConverter
    End Interface

    <ComImport, Guid("9EDDE9E7-8DEE-47EA-99DF-E6FAF2ED44BF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IWICBitmapDecoder
        Function QueryCapability(pIStream As IStream) As UInteger
        Sub Initialize(pIStream As IStream, cacheOptions As UInteger)
        Sub GetContainerFormat(ByRef pguidContainerFormat As Guid)
        Function GetDecoderInfo() As IntPtr
        Sub CopyPalette(pIPalette As IntPtr)
        Function GetMetadataQueryReader() As IntPtr
        Function GetPreview() As IWICBitmapSource
        Sub GetColorContexts(cCount As UInteger, ppIColorContexts As IntPtr, ByRef pcActualCount As UInteger)
        Function GetThumbnail() As IWICBitmapSource
        Function GetFrameCount() As UInteger
        Function GetFrame(index As UInteger) As IWICBitmapFrameDecode
    End Interface

    <ComImport, Guid("00000120-A8F2-4877-BA0A-FD2B6645FB94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IWICBitmapSource
        Sub GetSize(ByRef puiWidth As UInteger, ByRef puiHeight As UInteger)
        Sub GetPixelFormat(ByRef pPixelFormat As Guid)
        Sub GetResolution(ByRef pDpiX As Double, ByRef pDpiY As Double)
        Sub CopyPalette(pIPalette As IntPtr)
        Sub CopyPixels(prc As IntPtr, cbStride As UInteger, cbBufferSize As UInteger, pbBuffer As IntPtr)
    End Interface

    <ComImport, Guid("3B16811B-6A43-4EC9-A813-3D930C13B940"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IWICBitmapFrameDecode
        Sub GetSize(ByRef puiWidth As UInteger, ByRef puiHeight As UInteger)
        Sub GetPixelFormat(ByRef pPixelFormat As Guid)
        Sub GetResolution(ByRef pDpiX As Double, ByRef pDpiY As Double)
        Sub CopyPalette(pIPalette As IntPtr)
        Sub CopyPixels(prc As IntPtr, cbStride As UInteger, cbBufferSize As UInteger, pbBuffer As IntPtr)
    End Interface

    <ComImport, Guid("00000301-A8F2-4877-BA0A-FD2B6645FB94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IWICFormatConverter
        Sub GetSize(ByRef puiWidth As UInteger, ByRef puiHeight As UInteger)
        Sub GetPixelFormat(ByRef pPixelFormat As Guid)
        Sub GetResolution(ByRef pDpiX As Double, ByRef pDpiY As Double)
        Sub CopyPalette(pIPalette As IntPtr)
        Sub CopyPixels(prc As IntPtr, cbStride As UInteger, cbBufferSize As UInteger, pbBuffer As IntPtr)
        Sub Initialize(pISource As IWICBitmapSource, ByRef dstFormat As Guid, dither As Integer, pIPalette As IntPtr, alphaThresholdPercent As Double, paletteTranslate As Integer)
        Function CanConvert(ByRef srcPixelFormat As Guid, ByRef dstPixelFormat As Guid) As Boolean
    End Interface

    ''' <summary>Czy bajty zaczynają się od standardowego nagłówka RIFF....WEBP.</summary>
    Private Shared Function CzySygnaturaWebp(dane As Byte()) As Boolean
        If dane Is Nothing OrElse dane.Length < 12 Then Return False
        Return dane(0) = &H52 AndAlso dane(1) = &H49 AndAlso dane(2) = &H46 AndAlso dane(3) = &H46 AndAlso
               dane(8) = &H57 AndAlso dane(9) = &H45 AndAlso dane(10) = &H42 AndAlso dane(11) = &H50
    End Function

    ''' <summary>
    ''' Dekoduje obraz z bajtów (WebP lub inne formaty obsługiwane przez WIC) do <see cref="Bitmap"/> (32bppPArgb).
    ''' W razie braku systemowego kodeka WebP ustawia <paramref name="bladKodeka"/> = True i zwraca Nothing.
    ''' </summary>
    Public Shared Function DekodujZBajtow(dane As Byte(), ByRef bladKodeka As Boolean) As Bitmap
        bladKodeka = False
        If dane Is Nothing OrElse dane.Length = 0 Then Return Nothing

        'Najpierw spróbuj zwykłego GDI+ (jeśli obraz okaże się PNG/JPEG), jeśli nie — użyj WIC (WebP).
        Try
            Using ms As New MemoryStream(dane)
                Using img = Image.FromStream(ms, False, False)
                    Return New Bitmap(img)
                End Using
            End Using
        Catch
        End Try

        Dim factoryObj As Object = Nothing
        Dim stream As IStream = Nothing
        Dim decoder As IWICBitmapDecoder = Nothing
        Dim frame As IWICBitmapFrameDecode = Nothing
        Dim converter As IWICFormatConverter = Nothing
        Try
            Dim factoryType = Type.GetTypeFromCLSID(CLSID_WICImagingFactory)
            If factoryType Is Nothing Then bladKodeka = True : Return Nothing
            factoryObj = Activator.CreateInstance(factoryType)
            Dim factory = DirectCast(factoryObj, IWICImagingFactory)

            stream = SHCreateMemStream(dane, CUInt(dane.Length))
            If stream Is Nothing Then Return Nothing

            Const WICDecodeMetadataCacheOnDemand As UInteger = 0
            decoder = factory.CreateDecoderFromStream(stream, IntPtr.Zero, WICDecodeMetadataCacheOnDemand)
            If decoder Is Nothing OrElse decoder.GetFrameCount() = 0UI Then Return Nothing

            frame = decoder.GetFrame(0UI)
            Dim w As UInteger, h As UInteger
            frame.GetSize(w, h)
            If w = 0UI OrElse h = 0UI Then Return Nothing

            converter = factory.CreateFormatConverter()
            Dim dstFormat = GUID_WICPixelFormat32bppPBGRA
            converter.Initialize(DirectCast(frame, IWICBitmapSource), dstFormat, 0, IntPtr.Zero, 0.0, 0)

            Dim bmp As New Bitmap(CInt(w), CInt(h), PixelFormat.Format32bppPArgb)
            Try
                Dim rect As New Rectangle(0, 0, CInt(w), CInt(h))
                Dim bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb)
                Try
                    Dim stride = CUInt(Math.Abs(bd.Stride))
                    Dim rozmiar = stride * h
                    converter.CopyPixels(IntPtr.Zero, stride, rozmiar, bd.Scan0)
                Finally
                    bmp.UnlockBits(bd)
                End Try
            Catch
                'uszkodzony obraz - bitmapa nie wraca do wywołującego, więc nikt inny jej nie zwolni
                bmp.Dispose()
                Throw
            End Try
            Return bmp
        Catch ex As COMException When ex.ErrorCode = WINCODEC_ERR_COMPONENTNOTFOUND OrElse CzySygnaturaWebp(dane)
            bladKodeka = True
            Return Nothing
        Catch
            Return Nothing
        Finally
            If converter IsNot Nothing Then Marshal.FinalReleaseComObject(converter)
            If frame IsNot Nothing Then Marshal.FinalReleaseComObject(frame)
            If decoder IsNot Nothing Then Marshal.FinalReleaseComObject(decoder)
            If stream IsNot Nothing Then Marshal.FinalReleaseComObject(stream)
            If factoryObj IsNot Nothing Then Marshal.FinalReleaseComObject(factoryObj)
        End Try
    End Function

    ''' <summary>Wczytuje i dekoduje obraz z pliku na dysku przez WIC.</summary>
    Public Shared Function DekodujZPliku(sciezka As String, ByRef bladKodeka As Boolean) As Bitmap
        bladKodeka = False
        If String.IsNullOrWhiteSpace(sciezka) OrElse Not File.Exists(sciezka) Then Return Nothing
        Return DekodujZBajtow(File.ReadAllBytes(sciezka), bladKodeka)
    End Function
End Class
