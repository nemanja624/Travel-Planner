# Travel Planner

Travel Planner je web aplikacija za planiranje putovanja. Sistem podrzava planove putovanja, destinacije, aktivnosti po danima, troskove i budzet, checklistu, autentikaciju, admin uloge i dijeljenje plana preko share linka/QR koda.

## Tehnologije

- Backend: .NET 8, ASP.NET Core, Microsoft Service Fabric
- Baza: Microsoft SQL Server / LocalDB
- Frontend: React, TypeScript, Vite
- Autentikacija: JWT tokeni, bcrypt hesiranje lozinki

## Preduslovi

- Windows
- Visual Studio sa Service Fabric alatima
- Service Fabric SDK i lokalni klaster
- .NET 8 SDK
- Microsoft SQL Server LocalDB ili SQL Server
- Node.js 18+
- npm

## Arhitektura

| Komponenta | Tip | Uloga |
| --- | --- | --- |
| `Gateway` | Service Fabric stateless ASP.NET Core servis | Javni REST API, autentikacija zahtjeva, autorizacija, pozivi ka core servisima |
| `AuthService` | Service Fabric stateful servis | Logicki servis za korisnike i autentikaciju |
| `TripService` | Service Fabric stateful servis | Logicki servis za putovanja, planove, budzet i dijeljenje |
| `AuthService.Core` | Class library | Registracija, login, bcrypt hesiranje lozinki, JWT tokeni, admin logika |
| `AuthService.Data` | Class library | `User` model i `AuthDbContext` |
| `TripService.Core` | Class library | Poslovna logika za planove, destinacije, aktivnosti, troskove, checklistu i share linkove |
| `TripService.Data` | Class library | Modeli putovanja i `TripDbContext` |
| `Contracts` | Class library | DTO modeli i enum-i koje koriste backend slojevi |
| `Client` | React + TypeScript + Vite | Frontend aplikacija |

Klijent komunicira sa backendom preko `Gateway` REST API-ja. `Gateway` je stateless servis, dok su `AuthService` i `TripService` stateful Service Fabric servisi. DTO modeli su odvojeni od modela baze, a mapiranje se nalazi u data/core slojevima.

## Struktura projekta

- `Contracts` - DTO modeli i zajednicki enum-i
- `AuthService.Data` - modeli i DbContext za korisnike
- `AuthService.Core` - autentikacija, JWT i admin logika
- `TripService.Data` - modeli i DbContext za putovanja
- `TripService.Core` - poslovna logika za putovanja i share linkove
- `Gateway` - REST API kontroleri
- `TravelPlanner` - Service Fabric aplikacija
- `Client` - React frontend
- `Database/Migrations` - SQL migracije

## Baza podataka

Podrazumijevana konekcija u `Gateway/appsettings.json` koristi LocalDB:

```json
Server=(localdb)\\MSSQLLocalDB;Database=TravelPlanner;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Prije pokretanja backend-a izvrsiti SQL migracije redom:

1. `Database/Migrations/001_create_auth_schema.sql`
2. `Database/Migrations/002_create_trip_schema.sql`

Migracije kreiraju tabele za korisnike, putovanja, destinacije, aktivnosti, troskove, checklistu i share linkove.

## Backend konfiguracija

Glavna konfiguracija je u `Gateway/appsettings.json`.

Bitna podesavanja:

- `ConnectionStrings:TravelPlannerDatabase`
- `ConnectionStrings:AuthDatabase`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SigningKey`
- `Jwt:ExpirationMinutes`
- `ShareLinks:PublicBaseUrl`
- `ShareLinks:QrCodeUrlTemplate`

Za lokalni razvoj `ShareLinks:PublicBaseUrl` treba da pokazuje na frontend adresu, npr. `http://localhost:3000`.

## Pokretanje backend-a

Backend se pokrece kroz Visual Studio kao Service Fabric aplikacija.

1. Otvoriti `TravelPlanner.sln`.
2. Provjeriti da je baza kreirana i da su migracije izvrsene.
3. Provjeriti konekcione stringove u `Gateway/appsettings.json`.
4. Pokrenuti Service Fabric aplikaciju iz Visual Studio-a.

API Gateway je konfigurisan da frontend koristi adresu iz `.env` fajla.

Napomena: za Service Fabric lokalni razvoj najcesce je potrebno da je lokalni klaster pokrenut i da se Visual Studio pokrene kao administrator.

Za provjeru build-a:

```bash
dotnet build TravelPlanner.sln --no-restore
```

## Frontend konfiguracija

Frontend se nalazi u `Client`.

Primjer konfiguracije je u `Client/.env.example`:

```env
VITE_API_BASE_URL=http://localhost:8080
```

Za lokalni rad napraviti ili provjeriti `Client/.env.development` sa istom vrijednoscu ako Gateway radi na portu `8080`.

## Pokretanje frontenda

Iz foldera `Client`:

```bash
npm install
npm run dev
```

Vite dev server se podrazumijevano pokrece na:

```text
http://localhost:3000
```

Za provjeru produkcionog build-a:

```bash
npm run build
```

## Glavne funkcionalnosti

- Registracija i logovanje korisnika
- JWT autentikacija
- Uloge `User` i `Admin`
- Kreiranje, pregled, izmjena i brisanje planova putovanja
- Destinacije po putovanju
- Aktivnosti po danima
- Troskovi i automatski pregled budzeta
- Checklist / packing lista
- Admin pregled korisnika, promjena uloge i aktiviranje/deaktiviranje naloga
- Dijeljenje plana preko share linka i QR koda
- VIEW share pristup za pregled plana
- EDIT share pristup za izmjenu osnovnih podataka plana

## Napomene za testiranje

Preporuceni redoslijed rucne provjere:

1. Registracija novog korisnika.
2. Logovanje korisnika.
3. Kreiranje plana putovanja.
4. Dodavanje destinacija, aktivnosti, troskova i checklist stavki.
5. Provjera budzeta nakon dodavanja/izmjene/brisanja troskova.
6. Kreiranje VIEW share linka i otvaranje `/shared/{token}`.
7. Kreiranje EDIT share linka i izmjena osnovnih podataka plana.
8. Logovanje admin korisnika i provjera administracije korisnika.

## Lokalni admin korisnik

Registracija kroz aplikaciju kreira korisnika sa ulogom `User`. Za lokalno testiranje admin ekrana moze se registrovanom korisniku rucno promijeniti uloga u bazi:

```sql
UPDATE [dbo].[Users]
SET [Role] = 'Admin'
WHERE [Email] = 'admin@example.com';
```

Nakon toga se treba ponovo ulogovati da bi novi JWT token imao admin ulogu.

## Napomene

- `Client/.env.development` i `Client/.env.example` drze URL backend-a kroz `VITE_API_BASE_URL`.
- `Gateway/appsettings.json` drzi konekcione stringove, JWT konfiguraciju i share link konfiguraciju.
- `node_modules`, build output i lokalni fajlovi ne treba da se komituju.
- `dotnet build TravelPlanner.sln` moze pokusati restore i citanje korisnickog NuGet config-a; za brzu lokalnu provjeru nakon restore-a moze se koristiti `dotnet build TravelPlanner.sln --no-restore`.
