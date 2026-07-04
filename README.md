# Travel Planner

Travel Planner je web aplikacija za planiranje putovanja. Sistem podrzava planove putovanja, destinacije, aktivnosti po danima, troskove i budzet, checklistu, autentikaciju, admin uloge i dijeljenje plana preko share linka/QR koda.

## Tehnologije

- Backend: .NET 8, ASP.NET Core, Microsoft Service Fabric
- Baza: Microsoft SQL Server / LocalDB
- Frontend: React, TypeScript, Vite
- Autentikacija: JWT tokeni, bcrypt hesiranje lozinki

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

