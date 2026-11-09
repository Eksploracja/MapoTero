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

Namespace My

    Partial Friend Class MyApplication

#If NETCOREAPP Then
        ''' <summary>
        ''' Domyślna czcionka okien taka jak w .NET Framework (Microsoft Sans Serif 8,25 pt). W .NET 8 domyślną czcionką
        ''' jest Segoe UI 9 pt, a formularze skalują się według czcionki (AutoScaleMode.Font) - wszystkie okna były
        ''' o ok. 20% większe niż w projekcie, okno główne nie mieściło się na ekranie, a przyciski i pole komunikatów
        ''' zakotwiczone do dolnej krawędzi prawego panelu wjeżdżały pod grupę "Wybrane warstwy oraz ich kolejność".
        ''' </summary>
        Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As Global.Microsoft.VisualBasic.ApplicationServices.ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
            e.Font = New Font("Microsoft Sans Serif", 8.25!, FontStyle.Regular, GraphicsUnit.Point)
        End Sub
#End If

        ''' <summary>Liczba nieobsłużonych błędów w tej sesji programu.</summary>
        Private _liczbaAwarii As Integer

        ''' <summary>Okno z opisem błędu jest otwarte (błąd zgłoszony w trakcie jego wyświetlania tylko zapisywany do pliku).</summary>
        Private _komunikatAwarii As Boolean

        ''' <summary>Po tylu błędach w jednej sesji program jest zamykany bez pytania (np. błąd powtarzany przy każdym odrysowaniu okna).</summary>
        Private Const MaksAwariiWSesji As Integer = 3

        Private Sub MyApplication_Startup(sender As Object, e As Global.Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AwariaWatkuTla
        End Sub

        ''' <summary>
        ''' Nieobsłużony wyjątek w wątku okien - dawniej program znikał bez komunikatu. Opis błędu zapisywany jest do pliku
        ''' awarie.log w folderze danych, a użytkownik decyduje, czy kontynuować pracę.
        ''' (Zdarzenie nie jest wywoływane, gdy program uruchomiono pod debugerem.)
        ''' </summary>
        Private Sub MyApplication_UnhandledException(sender As Object, e As Global.Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim plik As String = ZapiszAwarie(e.Exception)
            _liczbaAwarii += 1
            If _komunikatAwarii Then
                e.ExitApplication = False
                Return
            End If

            Dim ostatnia As Boolean = _liczbaAwarii >= MaksAwariiWSesji
            Dim tekst As String = OpisAwarii(e.Exception, plik) & vbCrLf & vbCrLf &
                If(ostatnia, "Błąd powtarza się - program zostanie zamknięty.",
                   "Kontynuować pracę? Zalecane jest zapisanie wyników i ponowne uruchomienie programu." & vbCrLf &
                   "(Nie - zamknięcie programu)")
            _komunikatAwarii = True
            Try
                If ostatnia Then
                    MsgBox(tekst, MsgBoxStyle.Critical, "MapoTero - błąd programu")
                    e.ExitApplication = True
                Else
                    e.ExitApplication = MsgBox(tekst, MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, "MapoTero - błąd programu") <> MsgBoxResult.Yes
                End If
            Finally
                _komunikatAwarii = False
            End Try
        End Sub

        ''' <summary>
        ''' Nieobsłużony wyjątek w innym wątku - system i tak zamyka program, więc błąd jest tylko zapisywany i pokazywany.
        ''' </summary>
        Private Sub AwariaWatkuTla(sender As Object, e As System.UnhandledExceptionEventArgs)
            Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
            Dim plik As String = ZapiszAwarie(ex)
            If e.IsTerminating Then
                MessageBox.Show(OpisAwarii(ex, plik) & vbCrLf & vbCrLf & "Program zostanie zamknięty.",
                                "MapoTero - błąd programu", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Sub

        Private Shared Function OpisAwarii(ex As Exception, plik As String) As String
            Return "Wystąpił nieoczekiwany błąd programu:" & vbCrLf &
                If(ex Is Nothing, "(brak opisu)", ex.Message) & vbCrLf & vbCrLf &
                If(plik = "", "Nie udało się zapisać opisu błędu do pliku.",
                   "Opis błędu zapisano w pliku:" & vbCrLf & plik) & vbCrLf & vbCrLf &
                "Pomóż usunąć usterkę - prześlij ten plik i opisz, co działo się przed błędem:" & vbCrLf & AdresZgloszenBledow
        End Function

    End Class

End Namespace
