
<h2><a href="https://github.com/Eksploracja/MapoTero/releases/download/Mapotero_v3.10/MapoTero_v3.10.zip" target="_blank">Pobierz najnowszą wersję MapoTero 3.10</a></h2>



<p style="text-align: justify;"><strong>Aktualności:<br /> </strong></p>
<p style="text-align: justify;">20.10.2020 - Ukazała się najnowsza wersja MapoTero_v3.10. Najważniejsze zmiany:
  <ul><li>usunięcie usterki związanej z niewyświetlaniem podkładowej warstwy OpenStreetMap w systemie Windows 10</li>
    <li>aktualizacja warstw WMS,</li> 
  <li>zmiana platformy programistycznej .NET Framework z wersji 4.5 na 4.7.2,</li>
  </ul></p>
<p style="text-align: justify;">09.09.2019 - Opublikowana została najnowsza wersja MapoTero_v3.0.0.9. Najważniejsze zmiany:
  <ul><li>aktualizacja warstw WMS,</li> 
  <li>zmiana platformy programistycznej .NET Framework z wersji 3.5 na 4.5,</li>
  <li>naprawa błędu wyświetlania podkładowej warstwy OpenStreetMap.</li></ul></p>
<p style="text-align: justify;">26.11.2017 - Repozytorium mapotero.codeplex.com przechodzi w tryb "tylko do odczytu". Wciąż będzie można pobierać z niego obecnie najnowszą wersję MapoTero v. 3.0.0.7 Jednak ewentualne kolejne wydania aplikacji będą ukazywać się&nbsp;w nowym repozytorium&nbsp;<a href="https://github.com/Eksploracja/MapoTero/releases" target="_blank">https://github.com/Eksploracja/MapoTero/releases</a></p>
<p style="text-align: justify;"><strong>Opis projektu</strong><br /> MapoTero to program służący do pobierania rastrowych map z internetu, opublikowanych za pośrednictwem serwer&oacute;w WMS takich jak Geoportal2. Program pobiera mapy dzieląc je na siatkę kwadrat&oacute;w, kt&oacute;rych dopuszczalny rozmiar wynosi 2048px. Pobrane segmenty map można następnie:</p>
<ul style="text-align: justify;">
<li>scalić w jeden duży plik rastrowy przy użyciu specjalnej funkcji "złącz pobrane segmenty w jeden arkusz"</li>
<li>wgrać do urządzenia GPS przy użyciu takich program&oacute;w jak <a href="http://www.trekbuddy.net/forum/index.php"> TrekBuddy</a>, <a href="http://www.oziexplorer.com/">OziExplorer</a>;</li>
<li>wyświetlić jako podkład georeferencyjny w programach GIS i GPS, takich jak: <a href="http://www.qgis.org/pl/site/"> QGIS</a>, <a href="http://www.google.pl/intl/pl/earth/" target="_blank">Google Earth</a>, ArcGIS, TrekBuddy, GPS Tuner, MapInfo, OziExplorer.</li>
</ul>
<p style="text-align: justify;">Obsługiwane formaty rastrowe: jpeg, tiff, png, gif.</p>
<p style="text-align: justify;">Obsługiwane formaty georeferencji: map, kml, tab, jpgw, wld, gmi.</p>
<hr />
<p style="text-align: justify;">&nbsp;<strong>Skr&oacute;cona instrukcja obsługi</strong></p>
<ol>
<li>Wybierz z listy po prawej stronie okna programu zb&oacute;r map, z kt&oacute;rego chcesz pobrać dane, lub pozostaw domyślny i najczęściej używany zbi&oacute;r "skany map topograficznych";</li>
<li>Po wybraniu zbioru map, wybierz jego warstwę, kt&oacute;rą chcesz pobrać, klikając na nią jeden raz lewym klawiszem myszy. Najczęściej używane warstwy to "Raster_25_1965" (czyli mapa topograficzna w skali 1:25 000 w układzie wsp&oacute;łrzędnych "1965"), oraz "Raster_10_1965" (czyli mapa topograficzna w skali 1:10 000 w układzie wsp&oacute;łrzędnych "1965"). Nazwa prawidłowo zaznaczonej warstwy wyświetli się w polu "Wybrane warstwy";</li>
<li>Zaznacz na poglądowej mapie prawym klawiszem myszki zasięg pobieranej mapy;</li>
<li>Kliknij button "Pobierz". Rozpocznie się w&oacute;wczas zapis segment&oacute;w map do katalogu "download";</li>
<li>Wyświetl pobrane segmenty map z katalogu "download", klikając na ikonkę z ż&oacute;łtym folderem. Opcjonalnie scal w jeden arkusz pobrane segmenty map, klikając na niżej umieszczoną ikonkę z szachownicą "scal pobrane segmenty...". Wydajność narzędzia scalania arkuszy zależy od ilości dostępnej pamięci RAM. Działa ono poprawnie dla arkuszy, kt&oacute;rych końcowa rozdzielczość nie przekracza 10 000 x 10 000 px. W przypadku potrzeby scalenia większej liczby segment&oacute;w w jeden arkusz, sięgnij po narzędzie tworzenia panoram programu IfranView.</li>
</ol>
<p><img src="https://github.com/Eksploracja/MapoTero/blob/master/instrukcja.jpg" alt="Uproszczona instrukcja obsługi" width="640" height="486" /></p>
<hr />
<p style="text-align: justify;"><strong>Źr&oacute;dła informacji</strong></p>
<ul style="text-align: justify;">
<li>Aktualne informacje o programie znajdziesz w dziale "Programy GPS/GIS wspierające eksplorację" na <a href="http://www.forum.eksploracja.pl/viewforum.php?f=205" target="_blank">Pomorskim Forum Eksploracyjnym</a>.</li>
<li>Fanpage: <a href="http://www.facebook.com/mapotero.opensource">www.facebook.com/mapotero.opensource</a></li>
</ul>
<hr />
<p style="text-align: justify;"><strong>Aktualnie obsługiwane serwery WMS</strong></p>
<ul>
<li>Geoportal2</li>
<li>Państwowy Instytut Geologiczny</li>
<li>Generalna Dyrekcja Ochrony Środowiska</li>
</ul>
<p style="text-align: justify;"><strong>Wspom&oacute;ż nas!</strong></p>
<p style="text-align: justify;">Serdecznie zapraszamy wolontariuszy do wsp&oacute;łpracy przy rozwijaniu programu MapoTero! Pomysł&oacute;w na dalszy rozw&oacute;j jest tysiące, zaś programistycznych rąk do pracy zaledwie kilka :) Poszukujemy chętnych do testowania programu, porządkowania jego kodu źr&oacute;dłowego, tworzeniu nowych funkcji. Czym prędzej zgłoś swoją chęć pomocy przy programowaniu do koordynatora projektu (za pośrednictwem <a href="https://www.codeplex.com/site/users/contact/mapotero?OriginalUrl=https%3a%2f%2fwww.codeplex.com%2fsite%2fusers%2fview%2fmapotero"> Codeplext</a>,&nbsp;<a href="http://www.facebook.com/mapotero.opensource" target="_blank">FB</a> lub forum <a href="http://www.forum.eksploracja.pl/viewforum.php?f=205" target="_blank">PFE</a>).</p>
<p style="text-align: justify;">Program został napisany w języku VB.NET. Rozważamy możliwość przesiadki na popularniejsze języki c-pochodne. Pliki kodu źr&oacute;dłowego wsp&oacute;łpracują z programami IDE:&nbsp;<a href="http://www.icsharpcode.net/OpenSource/SD/Download/#SharpDevelop4x" target="_blank">SharpDevelop 4</a> (Open Source), Visual Studio Express 2015 (microsoft)</p>
<p style="text-align: justify;"><strong>Ekipa MapoTero</strong></p>
<ul>
<li>Tw&oacute;rca programu: <a href="http://www.forum.eksploracja.pl/viewtopic.php?f=205&amp;t=15196" target="_blank"> Pajakt</a>&nbsp;(2009-2014);</li>
<li>Rozw&oacute;j od 2015: <a href="http://labgis.pl/" target="_blank">Kazimierz Niecikowski</a>;</li>
<li>Pomoc w programowaniu: <a href="http://www.forum.eksploracja.pl/memberlist.php?mode=viewprofile&amp;u=3289" target="_blank"> Paweł_gdn</a>, <a href="http://www.my-navia.pl/viewtopic.php?p=422691#422691" target="_blank"> AAA222</a>&nbsp;(dodatek <a href="http://www.forum.eksploracja.pl/viewtopic.php?f=205&amp;t=35039" target="_blank"> NoTo</a>), Edward Zadorski (kod modułu przeliczania wsp&oacute;łrzędnych);</li>
</ul>
<hr />
<p style="text-align: justify;">&nbsp;<strong>Wygląd okien programu</strong></p>
<p style="text-align: justify;"><strong>MapoTero, Wersja 3.0.0.4<br /> </strong></p>
<p style="text-align: justify;">Gł&oacute;wne okno MapoTero</p>
<p style="text-align: justify;"><img src="https://github.com/Eksploracja/MapoTero/blob/master/przod.jpg" alt="Gł&oacute;wne okno programu" width="640" height="514" /></p>
<p style="text-align: justify;">Okno ustawień programu</p>
<p style="text-align: justify;"><img src="https://github.com/Eksploracja/MapoTero/blob/master/tyl.jpg" alt="okno ustawień" width="640" height="321" /></p>
<hr />
<p style="text-align: justify;">&nbsp;</p>
<p style="text-align: justify;"><strong>Najlepsze alternatywne rozwiązania:</strong></p>
<ul style="text-align: justify;">
<li><a href="http://www.my-navia.pl/viewtopic.php?p=422691#422691" target="_blank">Kafelkarz</a>&nbsp;- program autorstwa AAA222 umożliwiający pobieranie danych zar&oacute;wno z serwer&oacute;w WMS jak i WMTS. Jest on zaawansowaną i nieco bardziej skomplikowaną w obsłudze alternatywą programu MapoTero</li>
</ul>
<hr />
<p style="text-align: justify;">&nbsp;</p>
