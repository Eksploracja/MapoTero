### Opis zmian
<!-- Zwięzły opis: co zostało dodane, zmienione lub naprawione i dlaczego -->

### Powiązane zgłoszenia (Issues)
<!-- np. Naprawia #12, Rozwiązuje #5 -->

### Lista kontrolna przed wysłaniem PR
- [ ] Program kompiluje się bez błędów i ostrzeżeń (`dotnet build MapoTero.App/MapoTero_v3.vbproj -c Release`)
- [ ] Wszystkie testy jednostkowe przechodzą (`dotnet test MapoTero.Tests/MapoTero.Tests.vbproj -c Release`)
- [ ] Zachowano zasadę **jednego pliku exe** (brak dodatkowych plików DLL przy publikacji)
- [ ] Logika obliczeniowa, algorytmy i obsługa formatów znajdują się w `MapoTero.Core` wraz z testami xUnit
- [ ] Zachowano konwencje projektu (język polski w kodzie i komunikatach, Option Strict On, współrzędne X-północ / Y-wschód, kropka dziesiętna w plikach i zapytaniach)
- [ ] Zaktualizowano `README.md`, jeśli zmiana wpływa na obsługę programu przez użytkownika
