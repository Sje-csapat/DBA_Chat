# DBA Chat
Ez az alkalmazás a __Selye János Egyetem__ - __Gazdaságtudományi és Informatikai Kar__ - __Alkalmazott informatika__ szakának __KMI/AIdb/DBA/15__ azonosítójú __Adatbázis alkalmazások készítése__ órára készült a 2020/2021-es akadémiai évben.

Az alkalmazás megalkotásában __személyesen közösen dolgoztak__ az itt felsorolt hallgatók:
- Molnár Gejza (AIS ID: 126065)
- Salma Kristián (AIS ID: 124812)

------
# Rövid leírás

A megalkotott alkalmazás egy 'real-time' féle chat SQLite adatbázissal. Az adatbázisan vannak alapjáraton felhasználók melyek bejelentkezési adatai a következők:
| Felhasználónév |  Jelszó   |
| -------------  |:---------:| 
| admin          | admin     |
| geza           | geza      |
| krisztian      | krisztian |

A chatalkalmazás támogatja a következő műveleteket:
- AJAX segítségével (és JavaScript időzítők használatával) történő kezdetleges real-time beszélgetés 3 másodperces üzenet frissítéssel (legutolsó közös 25 db üzenet)
- 30 másodpercenkénti felhasználófrissítés
- Bizonyos esetben a munkafolyamat elvesztés lekezelése
- Űrlapok beküldésénél hibakezelés
- Bootstrap keretrendszerrel kezdetleges dizájn
- Dinamikus folyamatok lekezelése jQuery segítségével
- Regisztráció / Bejelentkezés
- Jelszavak hashelt formában történő tárolása
- Sikeres illetve sikertelen bejelentkezési kísérletek naplózása

Igyekeztünk betartani az MVC szerkezetet, bár ez __Web API__ készítésnél 100%-ban nem kivitelezhető - ugyanis vitatott az olyan komponensek használata, mint pl.: a munkafolyamat (Session), sütik (Cookies) ...

__A real-time komponenseknél megtörténhet, hogy az adatbázis a zárolás miatt elérhetetlenné válik, így kifagyhat az alkalmazás!__
Ezt a hibát próbáltuk valamennyire kiküszöbölni, de véglegesen nem sikerült eltávolítani. Megoldásként feltételezzük, hogy SQL szerver használata esetén ez a hiba nem jelentkezne!

Szükség esetén tudunk fejlesztési naplót felmutatni (GitHub).
