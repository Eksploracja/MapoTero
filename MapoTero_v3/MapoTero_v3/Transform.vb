Imports System



Public Class Transform



  

    ' Transformacja z ukladu BL na XY (uklad 1992)
    ' na podstawie http://ezador.w.interia.pl/gps/wzory_opis.htm
    'http://caribou.iisg.agh.edu.pl/trac/cast/browser/branches/plugins/kass/KASS_code/src/gis/Transform.java?rev=1525
    ' stale dla ukladu 1992:
    Const U92_L_Zero As Double = 19.0 ' Poludnik srodkowy
    Const U92_FE As Double = 500000.0 ' False Easting
    Const U92_FN As Double = -5300000.0 ' False Northing
    Const U92_M_ZERO As Double = 0.9993 ' Wsp. skali
    Const U92_E As Double = 0.0818191910428
    ' pozostale stale
   Const PI As Double = 3.14159265358979
    Const RO As Double = 6367449.14577
    Const A2 As Double = 0.0008377318247344
    Const A4 As Double = 0.0000007608527788826
    Const A6 As Double = 0.000000001197638019173
    Const A8 As Double = 0.00000000000244337624251

    Public Shared Function GetXU92(ByVal br As Double, ByVal lr As Double) As Double
        Dim brad, lrad, lorad, k, c, firad, xmer, ymer, xgk As Double
        brad = br * PI / 180
        lrad = lr * PI / 180
        lorad = U92_L_Zero * PI / 180
        k = Math.Pow(((1 - U92_E * Math.Sin(brad)) / (1 + U92_E * Math.Sin(brad))), U92_E / 2)
        c = k * Math.Tan(brad / 2 + PI / 4)
        firad = 2 * Math.Atan(c) - PI / 2
        xmer = Math.Atan(Math.Sin(firad) / (Math.Cos(firad) * Math.Cos(lrad - lorad)))
        ymer = 0.5 * Math.Log((1 + Math.Cos(firad) * Math.Sin(lrad - lorad)) / (1 - Math.Cos(firad) * Math.Sin(lrad - lorad)))
        xgk = RO * (xmer + A2 * Math.Sin(2 * xmer) * Math.Cosh(2 * ymer) + A4 * Math.Sin(4 * xmer) * Math.Cosh(4 * ymer) + A6 * Math.Sin(6 * xmer) * Math.Cosh(6 * ymer) + A8 * Math.Sin(8 * xmer) * Math.Cosh(8 * ymer))
        Return U92_M_ZERO * xgk + U92_FN
    End Function

    Public Shared Function GetYU92(ByVal br As Double, ByVal lr As Double) As Double
        Dim brad, lrad, lorad, k, c, firad, xmer, ymer, ygk As Double
        brad = br * PI / 180
        lrad = lr * PI / 180
        lorad = U92_L_Zero * PI / 180
        k = Math.Pow(((1 - U92_E * Math.Sin(brad)) / (1 + U92_E * Math.Sin(brad))), U92_E / 2)
        c = k * Math.Tan(brad / 2 + PI / 4)
        firad = 2 * Math.Atan(c) - PI / 2
        xmer = Math.Atan(Math.Sin(firad) / (Math.Cos(firad) * Math.Cos(lrad - lorad)))
        ymer = 0.5 * Math.Log((1 + Math.Cos(firad) * Math.Sin(lrad - lorad)) / (1 - Math.Cos(firad) * Math.Sin(lrad - lorad)))
        ygk = RO * (ymer + A2 * Math.Cos(2 * xmer) * Math.Sinh(2 * ymer) + A4 * Math.Cos(4 * xmer) * Math.Sinh(4 * ymer) + A6 * Math.Cos(6 * xmer) * Math.Sinh(6 * ymer) + A8 * Math.Cos(8 * xmer) * Math.Sinh(8 * ymer))
        Return U92_M_ZERO * ygk + U92_FE
    End Function

   


End Class

