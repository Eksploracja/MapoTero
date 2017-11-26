Opis projektu

MapoTero to program służący do pobierania rastrowych map z internetu, opublikowanych za pośrednictwem serwerów WMS takich jak Geoportal2. Program pobiera mapy dzieląc je na siatkę kwadratów, których dopuszczalny rozmiar wynosi 2048px. Pobrane segmenty map można następnie:
 - scalić w jeden duży plik rastrowy przy użyciu specjalnej funkcji "złącz pobrane segmenty w jeden arkusz"
 - wgrać do urządzenia GPS przy użyciu takich programów jak TrekBuddy, OziExplorer;
 - wyświetlić jako podkład georeferencyjny w programach GIS i GPS, takich jak: QGIS, Google Earth, ArcGIS, TrekBuddy, GPS Tuner, MapInfo, OziExplorer.

 Obsługiwane formaty rastrowe: jpeg, tiff, png, gif.
 Obsługiwane formaty georeferencji: map, kml, tab, jpgw, wld, gmi.

Skrócona instrukcja obsługi

Wybierz z listy po prawej stronie okna programu zbór map, z którego chcesz pobrać dane, lub pozostaw domyślny i najczęściej używany zbiór "skany map topograficznych";
Po wybraniu zbioru map, wybierz jego warstwę, którą chcesz pobrać, klikając na nią jeden raz lewym klawiszem myszy. Najczęściej używane warstwy to "Raster_25_1965" (czyli mapa topograficzna w skali 1:25 000 w układzie współrzędnych "1965"), oraz "Raster_10_1965" (czyli mapa topograficzna w skali 1:10 000 w układzie współrzędnych "1965"). Nazwa prawidłowo zaznaczonej warstwy wyświetli się w polu "Wybrane warstwy";
Zaznacz na poglądowej mapie prawym klawiszem myszki zasięg pobieranej mapy;
Kliknij button "Pobierz". Rozpocznie się wówczas zapis segmentów map do katalogu "download";
Wyświetl pobrane segmenty map z katalogu "download", klikając na ikonkę z żółtym folderem. Opcjonalnie scal w jeden arkusz pobrane segmenty map, klikając na niżej umieszczoną ikonkę z szachownicą "scal pobrane segmenty...". Wydajność narzędzia scalania arkuszy zależy od ilości dostępnej pamięci RAM. Działa ono poprawnie dla arkuszy, których końcowa rozdzielczość nie przekracza 10 000 x 10 000 px. W przypadku potrzeby scalenia większej liczby segmentów w jeden arkusz, sięgnij po narzędzie tworzenia panoram programu IfranView.
Uproszczona instrukcja obsługi

Źródła informacji

Aktualne informacje o programie znajdziesz w dziale "Programy GPS/GIS wspierające eksplorację" na Pomorskim Forum Eksploracyjnym.
Fanpage: www.facebook.com/mapotero.opensource
Aktualnie obsługiwane serwery WMS

Geoportal2
Państwowy Instytut Geologiczny
Generalna Dyrekcja Ochrony Środowiska

Wspomóż nas!

Serdecznie zapraszamy wolontariuszy do współpracy przy rozwijaniu programu MapoTero! Pomysłów na dalszy rozwój jest tysiące, zaś programistycznych rąk do pracy zaledwie kilka :) Poszukujemy chętnych do testowania programu, porządkowania jego kodu źródłowego, tworzeniu nowych funkcji. Czym prędzej zgłoś swoją chęć pomocy przy programowaniu do koordynatora projektu (za pośrednictwem Codeplext, FB lub forum PFE).

Program został napisany w języku VB.NET. Rozważamy możliwość przesiadki na popularniejsze języki c-pochodne. Pliki kodu źródłowego współpracują z programami IDE: SharpDevelop 4 (Open Source), Visual Studio Express 2015 (microsoft)

Ekipa MapoTero

Twórca programu: Pajakt (2009-2014);
Rozwój od 2015: Kazimierz Niecikowski;
Pomoc w programowaniu: Paweł_gdn, AAA222 (dodatek NoTo), Edward Zadorski (kod modułu przeliczania współrzędnych);

