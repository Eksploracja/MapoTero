'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

Imports System.IO

''' <summary>
''' Krok 4 i generowanie w szybkim kreatorze mapy: nazwy plików i folderów, siatka pobierania, sprawdzenie obszaru przed
''' generowaniem, opisy szacunku, formatu i wyniku.
''' </summary>
Public Module PodsumowanieKreatora

#Region "Pliki i foldery"

    ''' <summary>Nazwa bez znaków niedozwolonych w nazwie pliku, spacje zamienione na podkreślenia (pusta - „mapa”).</summary>
    Public Function OczyscNazwePliku(nazwa As String) As String
        If String.IsNullOrWhiteSpace(nazwa) Then Return "mapa"
        Dim niedozwolone = Path.GetInvalidFileNameChars()
        Dim czyste = New String(nazwa.Trim().Select(Function(c) If(c = " "c, "_"c, c)).Where(Function(c) Not niedozwolone.Contains(c)).ToArray())
        Return If(String.IsNullOrWhiteSpace(czyste), "mapa", czyste)
    End Function

    ''' <summary>
    ''' Nazwa pliku z miejsca, stylu i daty, np. „Mapa_Zakopane_piesza_gory_2026-11-21” (dawniej bez miejsca powstawało
    ''' „Mapa_Mapa_…”). <paramref name="nazwaMiejsca"/> - nazwa trasy albo miejscowości; Nothing - nazwa bez miejsca.
    ''' </summary>
    Public Function ProponowanaNazwaPliku(nazwaMiejsca As String, idSzablonu As String, data As DateTime) As String
        Dim czesci As New List(Of String) From {"Mapa"}
        If nazwaMiejsca IsNot Nothing Then czesci.Add(OczyscNazwePliku(nazwaMiejsca))
        czesci.Add(If(String.IsNullOrEmpty(idSzablonu), "kreator", idSzablonu))
        czesci.Add(data.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture))
        Return String.Join("_", czesci)
    End Function

    ''' <summary>
    ''' Ścieżka, pod którą nie ma jeszcze mapy (dopisuje _2, _3…), żeby nie nadpisać poprzedniej.
    ''' <paramref name="istnieje"/> - czy pod ścieżką jest już mapa (plik, folder albo tomy KMZ).
    ''' </summary>
    Public Function UnikalnaSciezka(sciezka As String, istnieje As Func(Of String, Boolean)) As String
        If Not istnieje(sciezka) Then Return sciezka
        Dim folder = Path.GetDirectoryName(sciezka)
        Dim nazwa = Path.GetFileNameWithoutExtension(sciezka)
        Dim ext = Path.GetExtension(sciezka)
        For i = 2 To Integer.MaxValue
            Dim kandydat = Path.Combine(folder, $"{nazwa}_{i}{ext}")
            If Not istnieje(kandydat) Then Return kandydat
        Next
        Throw New IOException("Nie udało się znaleźć wolnej nazwy mapy.")
    End Function

    ''' <summary>Nazwa folderu segmentów sesji kreatora: „Kreator_” + nazwa mapy + czas z milisekundami (każde generowanie osobno).</summary>
    Public Function NazwaFolderuSesji(plikDocelowy As String, czas As DateTime) As String
        Return "Kreator_" & OczyscNazwePliku(Path.GetFileNameWithoutExtension(plikDocelowy)) & "_" &
               czas.ToString("yyyyMMdd_HHmmss_fff", Globalization.CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Folder sesji stopnia skali o numerze <paramref name="indeks"/>: najdokładniejszy w folderze sesji, pozostałe obok
    ''' (_skala2, _skala3…). Ścieżki kończą się ukośnikiem.
    ''' </summary>
    Public Function FolderStopnia(folderSesji As String, indeks As Integer) As String
        If indeks = 0 Then Return folderSesji
        Return folderSesji.TrimEnd("\"c) & "_skala" & (indeks + 1) & "\"
    End Function

#End Region

#Region "Sprawdzenie przed generowaniem"

    ''' <summary>Siatka pobierania kreatora: bok segmentu dobrany do małego obszaru bez zmiany piksela.</summary>
    Public Function SiatkaKreatora(zasieg As Zasieg, piksel As Double) As Siatka
        Return New Siatka(zasieg, piksel, ZarzadcaSzablonow.BokSegmentuKreatora(zasieg, piksel))
    End Function

    ''' <summary>Powód, dla którego z siatki nie powstanie mapa („obszar jest zbyt duży” albo opis błędu siatki); Nothing - siatka dobra.</summary>
    Public Function BladSiatki(siatka As Siatka) As String
        If siatka.Poprawna AndAlso Not siatka.ZbytDuza Then Return Nothing
        Return If(siatka.ZbytDuza, "obszar jest zbyt duży", siatka.PowodNiepoprawnosci)
    End Function

    ''' <summary>Miejsce potrzebne na dysku: segmenty sesji i plik wynikowy zajmują razem ok. 2 razy szacowany rozmiar pliku.</summary>
    Public Function PotrzebneMiejsceBajty(szacunek As SzacunekKreatora) As Long
        Return CLng(szacunek.SzacowanyRozmiarMbMax * 2.0 * 1024 * 1024)
    End Function

    ''' <summary>Pytanie przed pobraniem dużego obszaru - te same progi i ten sam format, co szacunek w kroku 4.</summary>
    Public Function PytanieDuzyObszar(szacunek As SzacunekKreatora) As String
        Return szacunek.Ostrzezenie & vbCrLf & vbCrLf &
               $"Segmenty: {szacunek.LiczbaSegmentow:N0}, plik ok. {szacunek.SformatowanyPrzedzialRozmiaru()}, czas ok. {szacunek.SformatowanyCzas()}." & vbCrLf & vbCrLf &
               "Czy mimo to rozpocząć pobieranie?"
    End Function

    ''' <summary>
    ''' Stan, w którym kreator nie zapisuje mapy po pobraniu (Nothing - zapisuje). Bez żadnego segmentu nie ma czego zapisać.
    ''' Cudzą lub wcześniejszą mapę chroni zapis tylko kompletnej, ale własny wynik z brakami tej samej sesji
    ''' (<paramref name="plikZBrakami"/>) ponowienie zastępuje uzupełnioną mapą (dawniej zostawał stary plik z większymi brakami).
    ''' </summary>
    Public Function PowodWstrzymaniaZapisu(wszystkie As Integer, nieudane As Integer, istniejeWynik As Boolean,
                                           plikDocelowy As String, plikZBrakami As String) As String
        If wszystkie <= nieudane Then Return "Nie pobrano danych mapy."
        Dim chroniony = istniejeWynik AndAlso Not String.Equals(plikDocelowy, plikZBrakami, StringComparison.OrdinalIgnoreCase)
        If nieudane > 0 AndAlso chroniony Then Return "Zachowano poprzednią mapę."
        Return Nothing
    End Function

#End Region

#Region "Opisy kroku 4 i wyniku"

    ''' <summary>Opis liczby segmentów do pobrania; <paramref name="liczbaSkal"/> &gt; 0 - mapa wieloskalowa.</summary>
    Public Function OpisPobierania(szacunek As SzacunekKreatora, liczbaSkal As Integer) As String
        Dim segmenty = $"{szacunek.LiczbaSegmentow:N0} {ZarzadcaSzablonow.Odmiana(szacunek.LiczbaSegmentow, "segment", "segmenty", "segmentów")}"
        If liczbaSkal > 0 Then Return $"Pobieranie: {segmenty} w {liczbaSkal} {ZarzadcaSzablonow.Odmiana(liczbaSkal, "skali", "skalach", "skalach")}"
        If szacunek.CzyKorytarz Then Return $"Korytarz trasy: {segmenty} (oszczędność {szacunek.OszczednoscProcent}% względem prostokąta)"
        Return $"Segmenty do pobrania: {szacunek.SiatkaKolumny} × {szacunek.SiatkaWiersze}"
    End Function

    ''' <summary>Opis formatu wyniku, np. „Format: MBTiles wieloskalowy, powiększenia 8-16”.</summary>
    Public Function OpisFormatu(format As RodzajEksportu, wieloskalowa As Boolean, poziomMin As Integer, poziomMax As Integer) As String
        Return "Format: " & ZarzadcaSzablonow.NazwaFormatu(format) & If(wieloskalowa, " wieloskalowy", "") &
               If(ZarzadcaSzablonow.CzyFormatWielopoziomowy(format), $", powiększenia {poziomMin}-{poziomMax}", ", jeden poziom szczegółowości")
    End Function

    ''' <summary>Opis piksela obok pola: skala wydruku o tej samej rozdzielczości i szczegółowość.</summary>
    Public Function OpisPiksela(piksel As Double) As String
        Return $"m/px  -  jak wydruk w skali 1:{ZarzadcaSzablonow.SkalaWydruku(piksel):N0}; {ZarzadcaSzablonow.OpisSzczegolowosci(piksel)}"
    End Function

    ''' <summary>
    ''' Piksel podpowiadany, gdy wybrany jest dokładniejszy niż dane warstw (mniejszy nie doda szczegółów, a powiększy
    ''' plik); 0 - piksel jest w porządku.
    ''' </summary>
    Public Function PikselZalecany(zestaw As ZestawWarstw, piksel As Double) As Double
        Dim najmniejszy = ZarzadcaSzablonow.NajmniejszyPrzydatnyPiksel(zestaw)
        If najmniejszy > 0 AndAlso piksel < najmniejszy - 0.0001 Then Return najmniejszy * 2
        Return 0
    End Function

    ''' <summary>
    ''' Zakres powiększeń kroku 4: z planu skal mapy wieloskalowej (Nothing - mapa jednoskalowa), inaczej zalecany dla
    ''' piksela. Nothing - zostaje zakres ustawiony przez użytkownika.
    ''' </summary>
    Public Function ZakresPowiekszen(plan As IList(Of PlanStopnia), piksel As Double, szerokoscGeo As Double, wlasnyZakres As Boolean) As Tuple(Of Integer, Integer)
        If plan IsNot Nothing AndAlso plan.Count > 0 Then Return Tuple.Create(plan.Min(Function(ps) ps.PoziomMin), plan.Max(Function(ps) ps.PoziomMax))
        If wlasnyZakres Then Return Nothing
        Return ZarzadcaSzablonow.ZalecanyZakresPowiekszen(piksel, szerokoscGeo)
    End Function

    ''' <summary>Podpowiedź pola „Mapa wieloskalowa”: piksel i powiększenia każdej skali planu.</summary>
    Public Function OpisPlanu(plan As IEnumerable(Of PlanStopnia)) As String
        Return String.Join(vbCrLf, plan.Select(Function(ps) $"{ps.Stopien.Opis}: piksel {ps.Piksel:0.##} m, powiększenia {ps.PoziomMin}-{ps.PoziomMax}"))
    End Function

    ''' <summary>Tekst pola „Ortofotomapa przy zbliżeniu” z powiększeniami, które dostaną ortofotomapę.</summary>
    Public Function OpisOrtofotomapy(planOrtofotomapy As PlanStopnia) As String
        Dim tekst = "Ortofotomapa przy zbliżeniu"
        If planOrtofotomapy Is Nothing Then Return tekst
        If planOrtofotomapy.PoziomMin = planOrtofotomapy.PoziomMax Then Return tekst & $" (zoom {planOrtofotomapy.PoziomMin})"
        Return tekst & $" (zoom {planOrtofotomapy.PoziomMin}-{planOrtofotomapy.PoziomMax})"
    End Function

    ''' <summary>Rozmiar pliku mapy w MB (od 1 MB) albo KB (najmniej 1 KB).</summary>
    Public Function OpisRozmiaru(bajty As Long) As String
        If bajty >= 1024 * 1024 Then Return $"{bajty / (1024.0 * 1024.0):0.0} MB"
        Return $"{Math.Max(1, bajty / 1024.0):0} KB"
    End Function

    ''' <summary>Skale gotowej mapy wieloskalowej pod nazwą pliku, np. „Skale: 3 · Powiększenia: 8–16”.</summary>
    Public Function OpisSkalWyniku(plan As IList(Of PlanStopnia)) As String
        Return $"Skale: {plan.Count} · Powiększenia: {plan.Min(Function(ps) ps.PoziomMin)}–{plan.Max(Function(ps) ps.PoziomMax)}"
    End Function

    ''' <summary>Wykaz skal gotowej mapy w podpowiedzi, po jednej w wierszu: „1:25 000 (14–16)”.</summary>
    Public Function WykazSkalWyniku(plan As IEnumerable(Of PlanStopnia)) As String
        Return String.Join(vbCrLf, plan.Select(Function(ps) $"{ps.Stopien.Opis} ({ps.PoziomMin}–{ps.PoziomMax})"))
    End Function

#End Region

End Module
