# F1 svetainė

Šis projektas – ASP.NET Core 9.0 pagrindu sukurta F1 svetainė su integruota Identity autentifikacija, SQLite duomenų baze ir EF Core migracijomis. Svetainėje yra naujienų ir straipsnių turinys, sezono rezultatų lentelės, bei interaktyvus 3D F1 žaidimas (Three.js) su asmeniniu geriausio rato išsaugojimu prisijungusiems naudotojams.

## Kaip tai veikia
- **Žiniatinklio sluoksnis:** MVC ir Razor Pages (kataloge `src/F1.Web`) renderina puslapius, navigaciją ir komponentus.
- **Autentifikacija:** ASP.NET Core Identity (`ApplicationUser`) leidžia registraciją/prisijungimą ir personalizuotas funkcijas (pvz., geriausio rato saugojimą žaidime).
- **Duomenys:** EF Core + SQLite (`f1blog.db`). Domeno modeliai (komandos, lenktynininkai, savaitgaliai, komentarai, žaidimo rezultatai) deklaruoti `Models/`, konfigūruojami `ApplicationDbContext`.
- **Migracijos:** saugomos `src/F1.Web/Migrations`. Startuojant (`Program.cs`) automatiškai pritaikomos laukiančios migracijos.
- **3D F1 žaidimas:** `wwwroot/js/f1game3d.js` + `f1trackData.js` kuria trasą, automobilių fizikos logiką ir HUD. Užbaigus važiavimą, prisijungusių naudotojų geriausias ratas siunčiamas į `F1GameController` ir įrašomas į DB.
- **Statiniai resursai:** stiliai ir skriptai laikomi `wwwroot/`; papildomi vaizdai tiekiami per `images` ir `images2` virtualius katalogus.

## Paleidimas lokaliai
1. `dotnet restore`.
2. Pritaikykite migracijas ir sukurkite DB: `dotnet ef database update` (kataloge `src/F1.Web`).
3. Paleiskite svetainę: `dotnet run` (kataloge `src/F1.Web`).
