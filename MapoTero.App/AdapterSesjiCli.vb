Option Strict On
Option Explicit On

Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Adapter sesji dla trybu wiersza poleceń (CLI) - NIEGOTOWY.
''' Pierwsza wersja tylko sprawdzała conf.txt i zgłaszała sukces (kod 0) bez pobierania i bez eksportu,
''' więc skrypt użytkownika uznałby, że mapa powstała. Dopóki adapter nie wywołuje pobierania
''' (MapoTero.Silnik/PobieranieSegmentow.vb) i eksportu (EksportMapy), zgłasza błąd zamiast udawać pracę.
''' Tryb CLI nie jest podłączony do startu programu (My Project/ApplicationEvents.vb).
''' </summary>
Friend NotInheritable Class AdapterSesjiCli
    Implements IWykonawcaSesjiCli

    Public Function PobierzSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task _
        Implements IWykonawcaSesjiCli.PobierzSesjeAsync
        Return Task.FromException(New NotSupportedException("Pobieranie z wiersza poleceń nie jest jeszcze zaimplementowane."))
    End Function

    Public Function EksportujSesjeAsync(opcje As OpcjeCli, postep As IProgress(Of PostepCli), ct As CancellationToken) As Task(Of String) _
        Implements IWykonawcaSesjiCli.EksportujSesjeAsync
        Return Task.FromException(Of String)(New NotSupportedException("Eksport z wiersza poleceń nie jest jeszcze zaimplementowany."))
    End Function

End Class
