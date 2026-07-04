# Arhitektura sistema

```mermaid
flowchart LR
    User["Korisnik / Admin"] --> Client["React klijent"]
    Client -->|"REST API, JWT"| Gateway["Gateway<br/>Service Fabric stateless"]
    Gateway -->|"interni HTTP"| Auth["AuthService<br/>Service Fabric stateful"]
    Gateway -->|"interni HTTP"| Trip["TripService<br/>Service Fabric stateful"]
    Auth --> AuthCore["AuthService.Core"]
    Auth --> AuthData["AuthService.Data"]
    Trip --> TripCore["TripService.Core"]
    Trip --> TripData["TripService.Data"]
    AuthData --> Sql["Microsoft SQL Server<br/>TravelPlanner baza"]
    TripData --> Sql
    Gateway --> Contracts["Contracts<br/>DTO modeli i enum-i"]
    AuthCore --> Contracts
    TripCore --> Contracts
```

Gateway je jedini javni backend ulaz. AuthService i TripService su odvojeni stateful servisi sa internim HTTP endpointima. DTO modeli su u `Contracts`, modeli baze i DbContext klase su u `*.Data` projektima, a poslovna logika je u `*.Core` projektima.
