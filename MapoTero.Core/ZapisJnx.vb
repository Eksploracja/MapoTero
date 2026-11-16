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

Imports System.IO
Imports System.Text

''' <summary>Dane pojedynczego kafla w formacie JNX.</summary>
    Public Class KafelJnx
        Public Property Zasieg As ProstokatGeo
        Public Property Szerokosc As Short = 256
        Public Property Wysokosc As Short = 256
        Public Property ObrazJpeg As Byte()

        Public Sub New()
        End Sub

        Public Sub New(zasieg As ProstokatGeo, obraz() As Byte, Optional szer As Short = 256, Optional wys As Short = 256)
            Me.Zasieg = zasieg
            Me.ObrazJpeg = obraz
            Me.Szerokosc = szer
            Me.Wysokosc = wys
        End Sub
    End Class

    ''' <summary>Poziom powiększenia (warstwa piramidy) w formacie JNX.</summary>
    Public Class PoziomJnx
        ''' <summary>Skala widoczności Garmina (mniejsza wartość = większe przybliżenie).</summary>
        Public Property Skala As Integer
        Public ReadOnly Property Kafle As New List(Of KafelJnx)

        Public Sub New()
        End Sub

        Public Sub New(skala As Integer)
            Me.Skala = skala
        End Sub
    End Class

    ''' <summary>
    ''' Zapis map rastrowych do binarnego formatu Garmin BirdsEye (JNX wersja 4).
    ''' Format natywnie obsługuje piramidy powiększeń, do 50 000 kafli na plik oraz kolejność rysowania (drawOrder).
    ''' </summary>
    Public NotInheritable Class ZapisJnx

        Private Sub New()
        End Sub

        Public Const RozmiarNaglowka As Integer = 52
        Public Const RozmiarOpisuPoziomu As Integer = 17
        Public Const RozmiarOpisuKafla As Integer = 28

        ''' <summary>Przelicza stopnie geograficzne na format całkowity semicircles Garmina (2^31 na 180 stopni).</summary>
        Public Shared Function StopnieDoSemicircles(stopnie As Double) As Integer
            Dim semi As Double = Math.Round(stopnie * (2147483648.0 / 180.0))
            If semi >= Integer.MaxValue Then Return Integer.MaxValue
            If semi <= Integer.MinValue Then Return Integer.MinValue
            Return CInt(semi)
        End Function

        ''' <summary>Przelicza format semicircles Garmina na stopnie geograficzne.</summary>
        Public Shared Function SemicirclesDoStopni(semi As Integer) As Double
            Return semi * (180.0 / 2147483648.0)
        End Function

        ''' <summary>
        ''' Oblicza domyślny parametr skali JNX dla danego poziomu powiększenia Web Mercator i szerokości geograficznej.
        ''' </summary>
        Public Shared Function SkalaDlaPoziomu(poziomZoom As Integer, szerokoscGeo As Double) As Integer
            Dim rad As Double = szerokoscGeo * Math.PI / 180.0
            Dim s As Double = 34115555.0 / Math.Pow(2, poziomZoom) * Math.Cos(rad) / 1.1
            Return Math.Max(1, CInt(Math.Round(s)))
        End Function

        ''' <summary>Generuje deterministyczny identyfikator ProductID na podstawie nazwy mapy.</summary>
        Public Shared Function GenerujProductId(nazwa As String) As Short
            If String.IsNullOrEmpty(nazwa) Then Return 1
            Dim h As Integer = 17
            For Each c In nazwa
                h = (h * 31 + AscW(c)) And &H7FFF
            Next
            Return CShort(Math.Max(1, h))
        End Function

        ''' <summary>
        ''' Zapisuje plik JNX z podanymi poziomami i kaflami.
        ''' Poziomy powinny być posortowane od najniższego poziomu szczegółowości (przeglądowego) do najwyższego (detalicznego).
        ''' </summary>
        Public Shared Sub Zapisz(sciezka As String, nazwa As String, zOrder As Integer, poziomy As IList(Of PoziomJnx),
                                 Optional productId As Short = 0, Optional grupa As String = "")
            If poziomy Is Nothing OrElse poziomy.Count = 0 Then
                Throw New ArgumentException("Plik JNX musi zawierać co najmniej jeden poziom powiększenia.", NameOf(poziomy))
            End If

            Dim liczbaPoziomow As Integer = poziomy.Count
            Dim lacznaLiczbaKafli As Integer = 0
            Dim polnoc As Double = Double.MinValue, poludnie As Double = Double.MaxValue
            Dim wschod As Double = Double.MinValue, zachod As Double = Double.MaxValue

            For Each p In poziomy
                lacznaLiczbaKafli += p.Kafle.Count
                For Each k In p.Kafle
                    polnoc = Math.Max(polnoc, k.Zasieg.Polnoc)
                    poludnie = Math.Min(poludnie, k.Zasieg.Poludnie)
                    wschod = Math.Max(wschod, k.Zasieg.Wschod)
                    zachod = Math.Min(zachod, k.Zasieg.Zachod)
                Next
            Next

            If lacznaLiczbaKafli = 0 Then
                Throw New InvalidOperationException("Plik JNX nie może być pusty (brak kafli we wszystkich poziomach).")
            End If

            Dim idProd As Short = If(productId > 0, productId, GenerujProductId(nazwa))
            Dim nazwaGrupy As String = If(Not String.IsNullOrEmpty(grupa), grupa, If(Not String.IsNullOrEmpty(nazwa), nazwa, "MapoTero"))
            Dim tytul As String = If(Not String.IsNullOrEmpty(nazwa), nazwa, "MapoTero")

            Using fs As New FileStream(sciezka, FileMode.Create, FileAccess.Write, FileShare.None),
                  bw As New BinaryWriter(fs, Encoding.UTF8)

                ' 1. Nagłówek JNX (52 bajty)
                bw.Write(4) ' Wersja JNX = 4
                bw.Write(0) ' Device ID = 0 (działa na każdym urządzeniu)
                bw.Write(StopnieDoSemicircles(polnoc))
                bw.Write(StopnieDoSemicircles(wschod))
                bw.Write(StopnieDoSemicircles(poludnie))
                bw.Write(StopnieDoSemicircles(zachod))
                bw.Write(liczbaPoziomow)
                bw.Write(0) ' Data wygaśnięcia = 0 (bezterminowa)
                bw.Write(CInt(idProd))
                bw.Write(0) ' CRC = 0
                bw.Write(0) ' SigVersion = 0 (niepodpisany, dla firmware z odblokowanym BirdsEye)
                bw.Write(0UI) ' SigOffset = 0
                bw.Write(zOrder)

                ' 2. Obliczenie przesunięć w pliku
                ' Blok opisu mapy zaczyna się tuż za tablicą poziomów:
                Dim offsetOpisuMapy As Long = RozmiarNaglowka + CLng(RozmiarOpisuPoziomu) * liczbaPoziomow

                ' Zbudowanie bajtów bloku opisu mapy, aby poznać jego dokładną długość
                Dim bajtyOpisu As Byte()
                Using msOpis As New MemoryStream(), bwOpis As New BinaryWriter(msOpis, Encoding.UTF8)
                    bwOpis.Write(9) ' Wersja sekcji opisu = 9
                    ZapiszNapisZZerem(bwOpis, "00000000-0000-0000-0000-000000000000") ' GUID
                    ZapiszNapisZZerem(bwOpis, tytul)
                    bwOpis.Write(CByte(0)) ' Pusty napis
                    bwOpis.Write(idProd)
                    ZapiszNapisZZerem(bwOpis, nazwaGrupy)
                    bwOpis.Write(liczbaPoziomow)
                    For i = 0 To liczbaPoziomow - 1
                        bwOpis.Write(CByte(0))
                        bwOpis.Write(CByte(0))
                        bwOpis.Write(CByte(0))
                        bwOpis.Write(i)
                    Next
                    bajtyOpisu = msOpis.ToArray()
                End Using

                Dim offsetPierwszegoOpisuKafla As Long = offsetOpisuMapy + bajtyOpisu.Length

                ' 3. Tablica poziomów (Level Info - 17 bajtów na poziom)
                Dim biezacyOffsetOpisow As Long = offsetPierwszegoOpisuKafla
                For Each p In poziomy
                    bw.Write(p.Kafle.Count)
                    bw.Write(CUInt(biezacyOffsetOpisow))
                    bw.Write(p.Skala)
                    bw.Write(2) ' Format obrazu = 2 (JPEG)
                    bw.Write(CByte(0)) ' Bajt rezerwowy
                    biezacyOffsetOpisow += CLng(RozmiarOpisuKafla) * p.Kafle.Count
                Next

                ' 4. Zapis sekcji opisu mapy
                bw.Write(bajtyOpisu)

                ' 5. Tablica opisów kafli (Tile Descriptors - 28 bajtów na kafel)
                ' Dane JPEG zaczynają się tuż za wszystkimi opisami kafli
                Dim biezacyOffsetDanychJpeg As Long = biezacyOffsetOpisow
                For Each p In poziomy
                    For Each k In p.Kafle
                        bw.Write(StopnieDoSemicircles(k.Zasieg.Polnoc))
                        bw.Write(StopnieDoSemicircles(k.Zasieg.Wschod))
                        bw.Write(StopnieDoSemicircles(k.Zasieg.Poludnie))
                        bw.Write(StopnieDoSemicircles(k.Zasieg.Zachod))
                        bw.Write(k.Szerokosc)
                        bw.Write(k.Wysokosc)
                        bw.Write(k.ObrazJpeg.Length)
                        bw.Write(CUInt(biezacyOffsetDanychJpeg))
                        biezacyOffsetDanychJpeg += k.ObrazJpeg.Length
                    Next
                Next

                ' 6. Zapis surowych danych JPEG kafli
                For Each p In poziomy
                    For Each k In p.Kafle
                        bw.Write(k.ObrazJpeg)
                    Next
                Next
            End Using
        End Sub

        Private Shared Sub ZapiszNapisZZerem(bw As BinaryWriter, tekst As String)
            Dim b = Encoding.UTF8.GetBytes(tekst)
            bw.Write(b)
            bw.Write(CByte(0))
        End Sub

        ''' <summary>Podstawowa weryfikacja nagłówka pliku JNX (używana m.in. w testach jednostkowych).</summary>
        Public Shared Function WeryfikujJnx(sciezka As String, ByRef wersja As Integer, ByRef liczbaPoziomow As Integer,
                                            ByRef zOrder As Integer, ByRef productId As Integer, ByRef tytul As String) As Boolean
            wersja = 0 : liczbaPoziomow = 0 : zOrder = 0 : productId = 0 : tytul = ""
            If Not File.Exists(sciezka) Then Return False
            Using fs As New FileStream(sciezka, FileMode.Open, FileAccess.Read, FileShare.Read),
                  br As New BinaryReader(fs, Encoding.UTF8)
                If fs.Length < RozmiarNaglowka Then Return False
                wersja = br.ReadInt32()
                Dim devId = br.ReadInt32()
                Dim n = br.ReadInt32()
                Dim e = br.ReadInt32()
                Dim s = br.ReadInt32()
                Dim w = br.ReadInt32()
                liczbaPoziomow = br.ReadInt32()
                Dim exp = br.ReadInt32()
                productId = br.ReadInt32()
                Dim crc = br.ReadInt32()
                Dim sigVer = br.ReadInt32()
                Dim sigOff = br.ReadUInt32()
                zOrder = br.ReadInt32()

                If wersja < 3 OrElse wersja > 4 OrElse liczbaPoziomow <= 0 Then Return False

                ' Przejście do sekcji opisu
                fs.Seek(RozmiarNaglowka + CLng(RozmiarOpisuPoziomu) * liczbaPoziomow, SeekOrigin.Begin)
                Dim secVer = br.ReadInt32()
                OdczytajNapisZZerem(br) ' GUID
                tytul = OdczytajNapisZZerem(br)
                Return True
            End Using
        End Function

        Private Shared Function OdczytajNapisZZerem(br As BinaryReader) As String
            Dim sb As New StringBuilder()
            While True
                Dim b = br.ReadByte()
                If b = 0 Then Exit While
                sb.Append(ChrW(b))
            End While
            Return sb.ToString()
        End Function

    End Class
