'Copyright (C) <2026>  pajakt
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System
Imports System.Collections.Generic
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' Narzędzia do analizy pobranych segmentów i kafelków:
''' - detekcja „fałszywych sukcesów” (czysto białych lub pustych segmentów zwracanych z kodem HTTP 200),
''' - monitor powtarzających się plansz błędów, znaków wodnych lub blokad na serwerach kafelkowych XYZ/TMS.
''' </summary>
Public NotInheritable Class DetektorSegmentow

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Sprawdza, czy segment rastrowy składa się niemal wyłącznie z czystej bieli (lub bieli z marginesem szumu JPEG).
    ''' Dla map ortofoto lub cieniowania NMT jednolita biel oznacza błąd generowania widoku przez węzeł serwera.
    ''' Dla map ewidencyjnych nawet cienka linia (ułamek procenta pikseli) powoduje natychmiastowe zwrócenie False.
    ''' </summary>
    ''' <param name="piksele">Tablica pikseli Format32bppArgb (ARGB).</param>
    ''' <param name="progBieli">Minimalna wartość składowych R, G, B uznawana za biel (domyślnie 250).</param>
    ''' <param name="maksProcentNieBialych">Maksymalny dopuszczalny ułamek pikseli nie-białych (np. 0.001 = 0.1%), powyżej którego kadr ma treść.</param>
    Public Shared Function CzyPustyBialy(piksele As Integer(), Optional progBieli As Byte = 250, Optional maksProcentNieBialych As Double = 0.0005) As Boolean
        If piksele Is Nothing OrElse piksele.Length = 0 Then Return True

        Dim limitNieBialych As Integer = Math.Max(1, CInt(Math.Ceiling(piksele.Length * maksProcentNieBialych)))
        Dim licznikNieBialych As Integer = 0

        For i = 0 To piksele.Length - 1
            Dim c As Integer = piksele(i)
            Dim a As Integer = (c >> 24) And &HFF
            If a = 0 Then
                ' Piksel przezroczysty - w warstwach nieprzezroczystych nie występuje;
                ' w warstwach z przezroczystością pustka to norma.
                Continue For
            End If

            Dim r As Integer = (c >> 16) And &HFF
            Dim g As Integer = (c >> 8) And &HFF
            Dim b As Integer = c And &HFF

            If r < progBieli OrElse g < progBieli OrElse b < progBieli Then
                licznikNieBialych += 1
                ' Wczesne wyjście - jeśli znaleźliśmy choć kilkanaście pikseli o kolorze innym niż biel, kadr ma treść!
                If licznikNieBialych >= limitNieBialych Then Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' Oblicza szybki skrót SHA-256 z bufora bajtów (np. pobranego pliku JPEG lub PNG).
    ''' </summary>
    Public Shared Function SkrotSha256(dane As Byte()) As String
        If dane Is Nothing OrElse dane.Length = 0 Then Return ""
        Using sha = SHA256.Create()
            Dim hash = sha.ComputeHash(dane)
            Dim sb As New StringBuilder(hash.Length * 2)
            For Each b In hash
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

End Class

''' <summary>
''' Śledzi powtarzalność identycznych kafelków w ramach sesji pobierania z serwerów XYZ/TMS.
''' Pozwala wykryć serwowanie plansz blokad („Access Blocked”, „API Key Required”, brak kafelka)
''' pod różnymi współrzędnymi geograficznymi bez przerywania pobierania dla naturalnego oceanu.
''' </summary>
Public Class MonitorKafliXyz

    Private ReadOnly _wystapienia As New Dictionary(Of String, HashSet(Of Long))(StringComparer.Ordinal)
    Private ReadOnly _blokada As New Object()

    ''' <summary>Liczba powtórzeń tego samego kafelka w różnych współrzędnych uznawana za planszę blokady/błędu.</summary>
    Public Property ProgPowtorzen As Integer = 3

    ''' <summary>Czyści zarejestrowane sumy kontrolne kafli.</summary>
    Public Sub Wyczysc()
        SyncLock _blokada
            _wystapienia.Clear()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Rejestruje pobrany kafel. Zwraca True, jeśli ten sam niepusty kafel powtórzył się w różnych współrzędnych
    ''' osiągając próg podejrzenia planszy blokady/błędu.
    ''' </summary>
    Public Function ZarejestrujISprawdzCzyBlokada(dane As Byte(), kafelX As Integer, kafelY As Integer, zoom As Integer, ByRef liczbaPowtorzen As Integer) As Boolean
        liczbaPowtorzen = 0
        If dane Is Nothing OrElse dane.Length = 0 Then Return False

        Dim hasz = DetektorSegmentow.SkrotSha256(dane)
        If String.IsNullOrEmpty(hasz) Then Return False

        Dim kluczWspolrzednych As Long = (CLng(zoom) << 48) Or (CLng(kafelX And &HFFFFFF) << 24) Or CLng(kafelY And &HFFFFFF)

        SyncLock _blokada
            Dim zbiór As HashSet(Of Long) = Nothing
            If Not _wystapienia.TryGetValue(hasz, zbiór) Then
                zbiór = New HashSet(Of Long)()
                _wystapienia(hasz) = zbiór
            End If

            zbiór.Add(kluczWspolrzednych)
            liczbaPowtorzen = zbiór.Count

            Return liczbaPowtorzen >= ProgPowtorzen
        End SyncLock
    End Function

End Class
