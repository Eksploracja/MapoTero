Imports System.Runtime.CompilerServices

'biblioteka zgodna z CLS - używana przez program MapoTero oznaczony atrybutem CLSCompliant(True)
<Assembly: CLSCompliant(True)>
'testy wywołują składowe Friend silnika bezpośrednio (bez refleksji i bez tworzenia okien programu)
<Assembly: InternalsVisibleTo("MapoTero.Tests")>
