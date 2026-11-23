'Copyright (C) <2026> pajakt, Kazimierz Niecikowski
'This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

''' <summary>Co kreator zrobił z zestawem zatwierdzonym w oknie „Warstwy - zaawansowane”.</summary>
Public Enum ZmianaZestawuOkna
    ''' <summary>Użytkownik nie zgodził się na jedną kompozycję na wszystkich powiększeniach - styl bez zmian.</summary>
    Brak
    ''' <summary>Ten sam skład i kolejność warstw: zmiany krycia i mieszania obowiązują we wszystkich skalach.</summary>
    ZmianaWygladu
    ''' <summary>Zmieniony skład lub kolejność: styl używa odtąd tego zestawu (zestaw niestandardowy).</summary>
    ZestawNiestandardowy
End Enum

''' <summary>
''' Styl mapy wybrany w kroku 3 szybkiego kreatora: szablon, zestaw niestandardowy (po oknie „Warstwy - zaawansowane”
''' albo narzędziu MBTiles wieloskalowy), zmiany wyglądu warstw wspólne dla skal, plan skal z okna „Ustawienia skal”,
''' suwaki krycia grup warstw i grupy wyłączone polami wyboru. Składa z nich zestawy warstw do podglądu i pobierania.
''' </summary>
Public Class StylKreatora

    ''' <summary>Id stylu „warstwy z okna głównego” na liście stylów.</summary>
    Public Const IdWlasnegoZestawu As String = "wlasny_zestaw"

    Private ReadOnly _zrodlaSesji As Func(Of IEnumerable(Of KeyValuePair(Of String, PlikWarstw)))
    Private ReadOnly _zestawOknaGlownego As Func(Of ZestawWarstw)
    Private ReadOnly _zrodla As New Dictionary(Of String, PlikWarstw)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _brakujaceZbiory As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _blokadaZrodel As New Object()
    Private _zrodlaSzablonowZaladowane As Boolean
    Private _migawkaZrodel As Dictionary(Of String, PlikWarstw)

    ''' <param name="zrodlaSesji">Pliki zbiorów map wczytane w oknie głównym (Nothing - tylko pliki z folderu warstwy).</param>
    ''' <param name="zestawOknaGlownego">Warstwy wybrane w oknie głównym - styl <see cref="IdWlasnegoZestawu"/>.</param>
    Public Sub New(Optional zrodlaSesji As Func(Of IEnumerable(Of KeyValuePair(Of String, PlikWarstw))) = Nothing,
                   Optional zestawOknaGlownego As Func(Of ZestawWarstw) = Nothing)
        _zrodlaSesji = zrodlaSesji
        _zestawOknaGlownego = zestawOknaGlownego
    End Sub

    Public Property Szablon As SzablonMapy
    ''' <summary>Zestaw zatwierdzony w oknie warstw ze zmienionym składem albo z narzędzia MBTiles wieloskalowy (Nothing - zestaw szablonu).</summary>
    Public Property ZestawNiestandardowy As ZestawWarstw
    ''' <summary>Krycie i tryby mieszania zmienione w oknie warstw - stosowane do odpowiadających warstw każdej skali.</summary>
    Public Property ZmianyWygladu As New ZestawWarstw.ZmianyWygladu()
    ''' <summary>Plan skal ustawiony w oknie „Ustawienia skal” (Nothing - plan automatyczny).</summary>
    Public Property PlanWlasny As List(Of PlanStopnia)
    ''' <summary>Wartości suwaków krycia grup warstw.</summary>
    Public ReadOnly Property Regulacje As New Dictionary(Of RodzajRegulacji, Integer)()
    ''' <summary>Krycie grup warstw w szablonie albo w zestawie własnym - podstawa proporcji przy przesunięciu suwaka.</summary>
    Public ReadOnly Property BazoweRegulacje As New Dictionary(Of RodzajRegulacji, Integer)()
    ''' <summary>Grupy warstw wyłączone polami wyboru (np. „Zabytki nieruchome”).</summary>
    Public ReadOnly Property WylaczoneGrupy As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    Public ReadOnly Property CzyWlasnyZestaw As Boolean
        Get
            Return Szablon IsNot Nothing AndAlso Szablon.Id = IdWlasnegoZestawu
        End Get
    End Property

    Private Function ZestawZOknaGlownego() As ZestawWarstw
        Return If(_zestawOknaGlownego Is Nothing, Nothing, _zestawOknaGlownego())
    End Function

    ''' <summary>
    ''' Wybór stylu z listy. Inny styl kasuje zestaw niestandardowy, plan skal i zmiany wyglądu; ten sam (np. po powrocie
    ''' z kroku 4) zostawia je. Zwraca True, gdy wybrano inny styl.
    ''' </summary>
    Public Function Wybierz(s As SzablonMapy) As Boolean
        Dim inny = Szablon Is Nothing OrElse s Is Nothing OrElse Szablon.Id <> s.Id
        If inny Then
            ZestawNiestandardowy = Nothing
            PlanWlasny = Nothing
            ZmianyWygladu = New ZestawWarstw.ZmianyWygladu()
        End If
        Szablon = s
        Return inny
    End Function

    ''' <summary>
    ''' Pliki zbiorów map: z okna głównego oraz wszystkich szablonów, także warstw mniej dokładnych skal (dawniej tylko
    ''' najdokładniejszej). Brakujący plik jest zapamiętywany, żeby każde złożenie zestawu nie czytało go z dysku od nowa.
    ''' </summary>
    Public Function ZrodlaMap() As Dictionary(Of String, PlikWarstw)
        SyncLock _blokadaZrodel
            Dim zmieniono As Boolean = False
            If _zrodla.Count = 0 AndAlso _zrodlaSesji IsNot Nothing Then
                For Each kv In _zrodlaSesji()
                    _zrodla(kv.Key) = kv.Value
                    zmieniono = True
                Next
            End If
            If Not _zrodlaSzablonowZaladowane Then
                For Each sz In ZarzadcaSzablonow.PobierzWszystkie()
                    For Each w In sz.Warstwy.Concat(sz.StopnieSkali.SelectMany(Function(st) st.Warstwy))
                        If Not String.IsNullOrEmpty(w.Zbior) AndAlso Not _zrodla.ContainsKey(w.Zbior) AndAlso Not _brakujaceZbiory.Contains(w.Zbior) Then
                            Dim plik = ZarzadcaSzablonow.WczytajPlikZbioru(w.Zbior)
                            If plik IsNot Nothing Then
                                _zrodla(w.Zbior) = plik
                                zmieniono = True
                            Else
                                _brakujaceZbiory.Add(w.Zbior)
                            End If
                        End If
                    Next
                Next
                If ZarzadcaSzablonow.PobierzWszystkie().Count > 0 Then _zrodlaSzablonowZaladowane = True
            End If
            If _migawkaZrodel Is Nothing OrElse zmieniono Then
                _migawkaZrodel = New Dictionary(Of String, PlikWarstw)(_zrodla, StringComparer.OrdinalIgnoreCase)
            End If
            Return _migawkaZrodel
        End SyncLock
    End Function

#Region "Zestawy warstw"

    ''' <summary>
    ''' Zestaw warstw stylu z kryciem po suwakach, bez wyłączonych grup i ze zmianami wyglądu.
    ''' </summary>
    ''' <param name="piksel">Piksel pobierania - w stylu wieloskalowym wybiera stopień skali (0 - najdokładniejszy).</param>
    ''' <param name="stopien">Skala wybrana wprost (Nothing - skala dla piksela).</param>
    Public Function UtworzZestaw(Optional piksel As Double = 0, Optional stopien As StopienSkali = Nothing) As ZestawWarstw
        Dim zestaw As ZestawWarstw
        If ZestawNiestandardowy IsNot Nothing Then
            zestaw = ZestawNiestandardowy.Kopia()
        ElseIf Szablon Is Nothing Then
            Return New ZestawWarstw()
        ElseIf CzyWlasnyZestaw Then
            Dim zOkna = ZestawZOknaGlownego()
            zestaw = If(zOkna IsNot Nothing, zOkna.Kopia(), New ZestawWarstw())
        Else
            Dim plan = If(stopien Is Nothing, Nothing, PlanWlasny?.FirstOrDefault(Function(ps) ps.Stopien Is stopien))
            If plan IsNot Nothing Then Return UtworzZestawStopnia(plan)
            Dim warstwy = SzablonMapy.BezGrup(If(stopien Is Nothing, Szablon.WarstwyDlaPiksela(piksel), stopien.Warstwy), WylaczoneGrupy)
            zestaw = Szablon.UtworzZestaw(ZrodlaMap(), warstwy)
            ZastosujRegulacje(zestaw, warstwy)
            ZmianyWygladu.Zastosuj(zestaw)
            Return zestaw
        End If
        ZastosujRegulacje(zestaw, Nothing)
        Return zestaw
    End Function

    ''' <summary>Zestaw jednego stopnia planu skal (warstwy stopnia albo ustawione w oknie „Ustawienia skal”).</summary>
    Public Function UtworzZestawStopnia(plan As PlanStopnia) As ZestawWarstw
        Dim warstwy = SzablonMapy.BezGrup(plan.WarstwyEfektywne, WylaczoneGrupy)
        Dim zestaw = Szablon.UtworzZestaw(ZrodlaMap(), warstwy)
        ZastosujRegulacje(zestaw, warstwy)
        ZmianyWygladu.Zastosuj(zestaw)
        Return zestaw
    End Function

    ''' <summary>
    ''' Ustawia krycie warstw zestawu według suwaków. <paramref name="warstwySzablonu"/> - warstwy szablonu, z których
    ''' utworzono zestaw (krycie proporcjonalne do bazowego); Nothing - zestaw własny (krycie proporcjonalne do krycia
    ''' pierwszej warstwy grupy, bo krycie ustawione w oknie zaawansowanym jest ostateczne).
    ''' </summary>
    Public Sub ZastosujRegulacje(zestaw As ZestawWarstw, warstwySzablonu As IList(Of WarstwaSzablonu))
        If zestaw Is Nothing OrElse Regulacje.Count = 0 Then Exit Sub
        If warstwySzablonu IsNot Nothing Then
            ZarzadcaSzablonow.ZastosujRegulacje(warstwySzablonu, zestaw, Regulacje, BazoweRegulacje)
            Exit Sub
        End If
        For i = 0 To zestaw.Liczba - 1
            Dim r = ZarzadcaSzablonow.RegulacjaWarstwy(zestaw(i), i)
            Dim wartosc As Integer
            If r = RodzajRegulacji.Brak OrElse Not Regulacje.TryGetValue(r, wartosc) Then Continue For
            Dim baza As Integer = 0
            BazoweRegulacje.TryGetValue(r, baza)
            zestaw(i).Krycie = If(baza > 0, Math.Max(0, Math.Min(100, CInt(Math.Round(wartosc * zestaw(i).Krycie / CDbl(baza), MidpointRounding.AwayFromZero)))), wartosc)
        Next
    End Sub

    ''' <summary>Zestaw, z którego suwaki biorą krycie: niestandardowy, z okna głównego (styl własny) albo Nothing (szablon).</summary>
    Public Function ZestawSuwakow() As ZestawWarstw
        If ZestawNiestandardowy IsNot Nothing Then Return ZestawNiestandardowy
        If CzyWlasnyZestaw Then Return ZestawZOknaGlownego()
        Return Nothing
    End Function

    ''' <summary>
    ''' Rozpoznaje grupy warstw regulowane suwakami i ustawia ich krycie bazowe. Bez <paramref name="zachowajUstawienia"/>
    ''' suwaki wracają do krycia stylu, a wszystkie grupy warstw są włączone. Zwraca grupy w kolejności wierszy suwaków.
    ''' </summary>
    ''' <param name="zachowajUstawienia">True - ten sam styl wybrany ponownie: suwaki i pola grup zachowują ustawienia użytkownika.</param>
    Public Function UstawRegulacje(zachowajUstawienia As Boolean) As List(Of KeyValuePair(Of RodzajRegulacji, Integer))
        Dim wykryte As New List(Of KeyValuePair(Of RodzajRegulacji, Integer))()
        BazoweRegulacje.Clear()
        If Szablon Is Nothing Then Return wykryte
        Dim zestawWlasny = ZestawSuwakow()
        If zestawWlasny IsNot Nothing Then
            ' suwak startuje z krycia pierwszej warstwy grupy i dopiero po przesunięciu zmienia wszystkie warstwy grupy
            ' proporcjonalnie (dawniej nadpisywał je jedną wartością)
            For i = 0 To zestawWlasny.Liczba - 1
                Dim r = ZarzadcaSzablonow.RegulacjaWarstwy(zestawWlasny(i), i)
                If r <> RodzajRegulacji.Brak AndAlso Not wykryte.Any(Function(kv) kv.Key = r) Then
                    wykryte.Add(New KeyValuePair(Of RodzajRegulacji, Integer)(r, zestawWlasny(i).Krycie))
                End If
            Next
        Else
            wykryte = ZarzadcaSzablonow.RegulacjeSzablonu(Szablon)
        End If
        For Each kv In wykryte
            BazoweRegulacje(kv.Key) = kv.Value
        Next
        If Not zachowajUstawienia Then
            Regulacje.Clear()
            For Each kv In wykryte
                Regulacje(kv.Key) = kv.Value
            Next
            WylaczoneGrupy.Clear()
        End If
        Return wykryte
    End Function

    ''' <summary>Grupy warstw z polami wyboru w kroku 3 (zestaw własny ich nie ma).</summary>
    Public Function GrupyDoWyboru() As List(Of String)
        If Szablon Is Nothing OrElse ZestawSuwakow() IsNot Nothing Then Return New List(Of String)()
        Return Szablon.GrupyWarstw()
    End Function

    ''' <summary>
    ''' Przyjmuje zestaw zatwierdzony w oknie „Warstwy - zaawansowane” (<paramref name="przed"/> - zestaw przekazany do okna).
    ''' Ten sam skład zapisuje zmiany wyglądu, a zmieniony skład zastępuje styl zestawem niestandardowym - w stylu
    ''' wieloskalowym dopiero po potwierdzeniu, bo wtedy wszystkie powiększenia dostaną jedną kompozycję.
    ''' </summary>
    Public Function PrzyjmijZestawOkna(przed As ZestawWarstw, po As ZestawWarstw, potwierdzJednaKompozycja As Func(Of Boolean)) As ZmianaZestawuOkna
        If przed.TaSamaKompozycja(po) AndAlso Not CzyWlasnyZestaw AndAlso ZestawNiestandardowy Is Nothing Then
            ZmianyWygladu.Zapisz(przed, po)
            Return ZmianaZestawuOkna.ZmianaWygladu
        End If
        If Szablon IsNot Nothing AndAlso Szablon.Wieloskalowy AndAlso ZestawNiestandardowy Is Nothing AndAlso
           (potwierdzJednaKompozycja Is Nothing OrElse Not potwierdzJednaKompozycja()) Then Return ZmianaZestawuOkna.Brak
        ZestawNiestandardowy = po
        Return ZmianaZestawuOkna.ZestawNiestandardowy
    End Function

    ''' <summary>Warstwy zestawu z kryciem, np. „TOPO + ISOK_Cien (60%)”.</summary>
    Public Shared Function OpisZestawu(zestaw As ZestawWarstw) As String
        If zestaw Is Nothing OrElse zestaw.Liczba = 0 Then Return ""
        Return String.Join(" + ", zestaw.Warstwy.Select(Function(w) w.Nazwa & If(w.Krycie < 100, " (" & w.Krycie & "%)", "")))
    End Function

    ''' <summary>Skład stylu pod jego opisem w kroku 3.</summary>
    Public Function OpisSkladu() As String
        If Szablon Is Nothing Then Return ""
        If ZestawNiestandardowy IsNot Nothing Then Return OpisZestawu(ZestawNiestandardowy)
        If CzyWlasnyZestaw Then
            Dim z = ZestawZOknaGlownego()
            Return If(z IsNot Nothing AndAlso z.Liczba > 0, OpisZestawu(z), "(w oknie głównym nie wybrano jeszcze żadnej warstwy)")
        End If
        Return Szablon.OpisSkladnikow() & If(Szablon.Wieloskalowy, vbCrLf & "Skale: " & Szablon.OpisSkal(), "")
    End Function

    ''' <summary>
    ''' Komunikat, który zatrzymuje przejście z kroku 3 (Nothing - można iść dalej). Pusty zestaw dawniej przechodził
    ''' dalej i kończył się błędem dopiero przy generowaniu.
    ''' </summary>
    Public Function BladWyboru() As String
        If Szablon Is Nothing Then Return "Wybierz styl mapy z listy."
        If UtworzZestaw().Liczba > 0 Then Return Nothing
        Return If(CzyWlasnyZestaw,
                  "W oknie głównym nie wybrano żadnej warstwy." & vbCrLf & vbCrLf &
                  "Wybierz gotowy styl z listy albo dodaj warstwy w oknie głównym (drzewo „Wybierz mapy””).",
                  "Wybrany styl nie zawiera żadnej warstwy.")
    End Function

#End Region

#Region "Skale"

    ''' <summary>Formaty, w których kreator łączy osobno pobrane skale: MBTiles, SQLiteDB (OsmAnd, Locus) i JNX (Garmin BirdsEye).</summary>
    Public Shared Function FormatWieloskalowy(format As RodzajEksportu) As Boolean
        Return format = RodzajEksportu.MBTiles OrElse format = RodzajEksportu.SQLiteDB OrElse format = RodzajEksportu.Jnx
    End Function

    ''' <summary>Czy styl i format pozwalają utworzyć mapę wieloskalową (pole „Mapa wieloskalowa” widoczne).</summary>
    Public Function MozliwaWieloskalowa(format As RodzajEksportu) As Boolean
        Return Szablon IsNot Nothing AndAlso Szablon.Wieloskalowy AndAlso ZestawNiestandardowy Is Nothing AndAlso FormatWieloskalowy(format)
    End Function

    ''' <summary>Czy kreator utworzy mapę wieloskalową (osobne pobranie każdego stopnia skali).</summary>
    Public Function WieloskalowaAktywna(format As RodzajEksportu, wlaczona As Boolean) As Boolean
        Return wlaczona AndAlso MozliwaWieloskalowa(format)
    End Function

    Public Function PlanAutomatyczny(piksel As Double, szerokoscGeo As Double, zOrtofotomapa As Boolean) As List(Of PlanStopnia)
        Return ZarzadcaSzablonow.PlanStopni(Szablon, piksel, szerokoscGeo, zOrtofotomapa)
    End Function

    ''' <summary>Plan skal: ustawiony w oknie „Ustawienia skal” albo automatyczny (kopia - zmiany nie psują planu własnego).</summary>
    Public Function PlanBiezacy(piksel As Double, szerokoscGeo As Double, zOrtofotomapa As Boolean) As List(Of PlanStopnia)
        If PlanWlasny IsNot Nothing Then Return PlanWlasny.Select(Function(ps) ps.Kopia()).ToList()
        Return PlanAutomatyczny(piksel, szerokoscGeo, zOrtofotomapa)
    End Function

    ''' <summary>Skale do wyboru na liście podglądu (poza „Skala automatyczna”).</summary>
    Public Function StopniePodgladu() As List(Of StopienSkali)
        If Szablon Is Nothing OrElse ZestawNiestandardowy IsNot Nothing Then Return New List(Of StopienSkali)()
        If PlanWlasny Is Nothing Then Return Szablon.StopnieSkali.ToList()
        Return PlanWlasny.Select(Function(ps) ps.Stopien).ToList()
    End Function

    ''' <summary>
    ''' Skala, którą pokazuje okno „Warstwy - zaawansowane”, gdy podgląd ma skalę automatyczną: najdokładniejsza skala
    ''' planu (z okna „Ustawienia skal”) albo szablonu - ma najwięcej warstw (np. w mapach WIG także arkusze 1:25 000).
    ''' Nothing - okno pokazuje warstwy podglądu (skala wybrana ręcznie, mapa jednoskalowa, zestaw własny).
    ''' </summary>
    Public Function StopienOknaWarstw(stopienPodgladu As StopienSkali) As StopienSkali
        If stopienPodgladu IsNot Nothing OrElse Szablon Is Nothing OrElse Not Szablon.Wieloskalowy Then Return Nothing
        If ZestawNiestandardowy IsNot Nothing OrElse CzyWlasnyZestaw Then Return Nothing
        If PlanWlasny IsNot Nothing AndAlso PlanWlasny.Count > 0 Then Return PlanWlasny.OrderBy(Function(ps) ps.Piksel).First().Stopien
        Return If(Szablon.StopnieGlowne.FirstOrDefault(), Szablon.StopnieSkali.FirstOrDefault())
    End Function

    ''' <summary>Status gotowej próbki stylu: skala pokazywana w podglądzie stylu wieloskalowego.</summary>
    Public Function OpisSkaliPodgladu(stopienPodgladu As StopienSkali, pikselPodgladu As Double) As String
        If Szablon Is Nothing OrElse Not Szablon.Wieloskalowy OrElse ZestawNiestandardowy IsNot Nothing Then Return "Podgląd gotowy"
        Dim st = If(stopienPodgladu, Szablon.StopienDlaPiksela(pikselPodgladu))
        Return If(st Is Nothing, "Podgląd gotowy", "Skala " & st.Opis)
    End Function

    ''' <summary>
    ''' Piksel próbki podglądu stylu gotowego (nie własnego): piksel skali z planu, skali albo próbki szablonu, podzielony
    ''' przez powiększenie podglądu.
    ''' </summary>
    Public Function PikselProbki(stopien As StopienSkali, powiekszenie As Double) As Double
        If Szablon Is Nothing Then Return 1.0
        Dim podgladPlanu = PlanWlasny?.FirstOrDefault(Function(ps) ps.Stopien Is stopien)
        Return Math.Max(0.05, If(podgladPlanu?.Piksel, If(stopien?.Piksel, Szablon.ProbkaPiksel)) / powiekszenie)
    End Function

    ''' <summary>Punkt próbki stylu: przesunięty myszą, środek obszaru użytkownika albo przykładowe miejsce stylu.</summary>
    Public Function PunktProbki(punktWlasny As PunktXY?, obszar As Zasieg) As PunktXY
        If punktWlasny.HasValue Then Return punktWlasny.Value
        If obszar.Poprawny Then Return obszar.Srodek
        If Szablon Is Nothing Then Return New PunktXY(0, 0)
        Return UkladWspolrzednych.PL1992.ZWgs84(Szablon.ProbkaLat, Szablon.ProbkaLon)
    End Function

#End Region

#Region "Ustawienia domyślne i korekta obrazu"

    ''' <summary>Klucz celu i stylu - ustawienia domyślne kroku 4 stosowane są tylko przy jego zmianie.</summary>
    Public Function KluczDomyslnych(cel As CelMapy) As String
        Return cel.ToString() & "|" & If(Szablon IsNot Nothing, Szablon.Id, "")
    End Function

    ''' <summary>
    ''' Piksel domyślny: z szablonu (dawniej brany tylko przy wartości &lt;= 0, czego pole nie dopuszcza, więc kataster
    ''' dostawał 1 m zamiast 0,25 m), a dla warstw z okna głównego - piksel jego siatki, jeśli jest.
    ''' </summary>
    Public Function PikselDomyslny(pikselSiatkiOkna As Double) As Double
        Dim piksel = If(Szablon IsNot Nothing, Szablon.DomyslnyPiksel, 1.0)
        If CzyWlasnyZestaw AndAlso pikselSiatkiOkna > 0 Then piksel = pikselSiatkiOkna
        Return piksel
    End Function

    ''' <summary>Przykładowy obszar stylu wokół punktu próbki (1,5 km dla katastru, 8 km dla map przeglądowych, inaczej 4 km).</summary>
    Public Function WzorcowyZasieg() As Zasieg
        If Szablon Is Nothing Then Return New Zasieg()
        Dim srodek = UkladWspolrzednych.PL1992.ZWgs84(Szablon.ProbkaLat, Szablon.ProbkaLon)
        Dim szerMetry = If(Szablon.DomyslnyPiksel <= 0.5, 1500.0, If(Szablon.DomyslnyPiksel >= 4.0, 8000.0, 4000.0))
        Dim pol = szerMetry / 2.0
        Return New Zasieg(srodek.X - pol, srodek.Y - pol, srodek.X + pol, srodek.Y + pol)
    End Function

    ''' <summary>Profil korekty podpowiadany użytkownikowi: Garmin MIP dla odbiorników Garmin, inaczej profil stylu.</summary>
    Public Function ProfilSugerowany(cel As CelMapy) As ProfilKorektyRastra
        If cel = CelMapy.OdbiornikGarmin Then Return ProfilKorektyRastra.GarminMip
        Return If(Szablon IsNot Nothing, Szablon.ProfilKorekty, ProfilKorektyRastra.Brak)
    End Function

    ''' <summary>Opcje do okna korekty: wyłączona korekta bez profilu dostaje profil sugerowany (podgląd od razu pokazuje efekt).</summary>
    Public Function OpcjeKorektyDoOkna(opcje As OpcjeKorekcjiMip, cel As CelMapy) As OpcjeKorekcjiMip
        Dim o = opcje.Kopia()
        Dim sugerowany = ProfilSugerowany(cel)
        If o.Profil = ProfilKorektyRastra.Brak AndAlso sugerowany <> ProfilKorektyRastra.Brak Then
            o.UstawWgProfilu(sugerowany)
            o.Wlaczona = False
        End If
        Return o
    End Function

    ''' <summary>Zaznaczenie pola „Korekta obrazu” bez ustawionego profilu włącza profil sugerowany (inaczej korekta nic by nie zmieniała).</summary>
    Public Sub WlaczKorekte(opcje As OpcjeKorekcjiMip, wlaczona As Boolean, cel As CelMapy)
        If wlaczona AndAlso opcje.Profil = ProfilKorektyRastra.Brak Then
            Dim sugerowany = ProfilSugerowany(cel)
            opcje.UstawWgProfilu(If(sugerowany <> ProfilKorektyRastra.Brak, sugerowany, ProfilKorektyRastra.WyostrzenieKrawedzi))
        End If
        opcje.Wlaczona = wlaczona
    End Sub

    ''' <summary>Opis korekty obok pola „Korekta obrazu”.</summary>
    Public Function OpisKorekty(wlaczona As Boolean, opcje As OpcjeKorekcjiMip, cel As CelMapy) As String
        If wlaczona AndAlso opcje.Profil <> ProfilKorektyRastra.Brak Then Return "Profil: " & OpcjeKorekcjiMip.KrotkaNazwaProfilu(opcje.Profil)
        Dim sugerowany = ProfilSugerowany(cel)
        If sugerowany <> ProfilKorektyRastra.Brak Then Return "Wyłączona (sugerowany profil: " & OpcjeKorekcjiMip.KrotkaNazwaProfilu(sugerowany) & ")"
        Return "Wyłączona"
    End Function

#End Region

#Region "Własne style i opis mapy"

    ''' <summary>
    ''' Styl użytkownika z bieżącej kompozycji: warstwy z kryciem i trybem mieszania po suwakach, polach grup i oknie
    ''' „Warstwy - zaawansowane”; w mapie wieloskalowej osobno każda skala (z planu „Ustawienia skal”, jeśli jest).
    ''' Punkt próbki - środek <paramref name="obszar"/> (bez obszaru - punkt próbki stylu bazowego).
    ''' </summary>
    Public Function UtworzStylWlasny(nazwa As String, obszar As Zasieg) As SzablonMapy
        Dim baza = Szablon
        If baza Is Nothing Then Return Nothing
        Dim srodek = If(obszar.Poprawny, UkladWspolrzednych.PL1992.DoWgs84(obszar.Srodek), New PunktGeo(baza.ProbkaLat, baza.ProbkaLon))
        Dim s As New SzablonMapy(SzablonyWlasne.NoweId(), nazwa, SzablonyWlasne.KategoriaWlasna,
                                 "Mój styl na podstawie: " & baza.Nazwa & ".", "⭐", baza.DomyslnyPiksel, baza.ProfilKorekty,
                                 baza.DozwoloneCele.ToArray()) With {
            .ProbkaLat = srodek.Szerokosc, .ProbkaLon = srodek.Dlugosc, .ProbkaPiksel = baza.ProbkaPiksel, .UwagaPokrycia = baza.UwagaPokrycia}
        Dim wzorce = baza.Warstwy.Concat(baza.StopnieSkali.SelectMany(Function(st) st.Warstwy)).ToList()
        Dim konwertuj = Function(zestaw As ZestawWarstw) zestaw.Warstwy.Select(
            Function(w) WarstwaSzablonu.ZWarstwyWybranej(w, wzorce.FirstOrDefault(Function(x) String.Equals(x.Zbior, w.Zbior, StringComparison.OrdinalIgnoreCase) AndAlso
                                                                                               String.Equals(x.Warstwa, w.Nazwa, StringComparison.OrdinalIgnoreCase)))).ToArray()
        If baza.Wieloskalowy AndAlso ZestawNiestandardowy Is Nothing Then
            Dim skale = If(PlanWlasny IsNot Nothing AndAlso PlanWlasny.Count > 0,
                           PlanWlasny.Select(Function(ps) Tuple.Create(ps.Stopien, ps.Piksel)).ToList(),
                           baza.StopnieSkali.Select(Function(st) Tuple.Create(st, st.Piksel)).ToList())
            For Each sk In skale.OrderBy(Function(t) If(t.Item1.Opcjonalny, 1, 0)).ThenBy(Function(t) t.Item2)
                Dim warstwy = konwertuj(UtworzZestaw(0, sk.Item1))
                If warstwy.Length = 0 Then Continue For
                'stopień opcjonalny zostaje opcjonalny - inaczej ortofotomapa (najmniejszy piksel) stałaby się główną skalą stylu
                If sk.Item1.Opcjonalny Then
                    s.DodajStopienOpcjonalny(sk.Item1.Opis, sk.Item2, warstwy)
                Else
                    s.DodajStopien(sk.Item1.Opis, sk.Item2, warstwy)
                End If
            Next
        Else
            s.Warstwy.AddRange(konwertuj(UtworzZestaw(baza.DomyslnyPiksel, Nothing)))
        End If
        Return s
    End Function

    ''' <summary>Opis mapy wieloskalowej w metadanych MBTiles: styl i jego skale.</summary>
    Public Function OpisMapyWieloskalowej(plan As IList(Of PlanStopnia)) As String
        Return If(Szablon IsNot Nothing, Szablon.Nazwa, "") & " – " & String.Join(", ", plan.Select(Function(ps) ps.Stopien.Opis))
    End Function

    ''' <summary>Atrybucja mapy wieloskalowej: etykiety warstw wszystkich skal bez powtórzeń i bez wyłączonych grup.</summary>
    Public Function AtrybucjaWieloskalowej(plan As IList(Of PlanStopnia)) As String
        Return "MapoTero – " & String.Join(", ", plan.SelectMany(Function(ps) SzablonMapy.BezGrup(ps.WarstwyEfektywne, WylaczoneGrupy)).
                                                 Select(Function(w) w.Etykieta).Distinct())
    End Function

#End Region

End Class
