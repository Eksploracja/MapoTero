﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Ten kod został wygenerowany przez narzędzie.
'     Wersja wykonawcza:4.0.30319.34209
'
'     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
'     kod zostanie ponownie wygenerowany.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Ta klasa została automatycznie wygenerowana za pomocą klasy StronglyTypedResourceBuilder
    'przez narzędzie, takie jak ResGen lub Visual Studio.
    'Aby dodać lub usunąć członka, edytuj plik .ResX, a następnie ponownie uruchom ResGen
    'z opcją /str lub ponownie utwórz projekt VS.
    '''<summary>
    '''  Klasa zasobu wymagająca zdefiniowania typu do wyszukiwania zlokalizowanych ciągów itd.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''Zwraca buforowane wystąpienie ResourceManager używane przez tę klasę.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("MapoTero.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Zastępuje właściwość CurrentUICulture bieżącego wątku dla wszystkich
        '''  przypadków przeszukiwania zasobów za pomocą tej klasy zasobów wymagającej zdefiniowania typu.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Wyszukuje zlokalizowany zasób typu System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property MapoTero() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("MapoTero", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
    End Module
End Namespace
