# AGENTS.md

ASP.NET Core (.NET 10) Web API for filing/managing flight plans, backed by MongoDB. Single-project repo, no monorepo, no tests.

## Commands

- `dotnet build` — build; emits an expected `NU1902` warning (Swashbuckle 6.2.3 vulnerability). Do not "fix" it.
- `dotnet run --profile https` — run with HTTPS on `https://localhost:3001` (http profile uses port 5250).
- Requires a local MongoDB on `localhost:27017` (`brew services start mongodb/brew/mongodb-community`). The app will fail at request time without it.
- There is no test project — `dotnet test` has nothing to run.

## Architecture

- `Program.cs` wires DI: requests flow through `IDatabaseAdapter` (scoped) → `MongoDbDatabase`, which reads/writes raw `BsonDocument`.
- `GlobalUsings.cs` supplies global usings (Swagger, MongoDB, Mvc, etc.) — source files intentionally omit these `using`s; keep new files consistent.
- All endpoints are `[Authorize]`. Basic auth via `Authentication/BasicAuthenticationHandler.cs`; credentials come from `appsettings.json` `AdminCredentials` (default `admin` / `P@ssw0rd`).
- Swagger is only enabled when `ASPNETCORE_ENVIRONMENT=Development` (guarded in `Program.cs`); Swagger config lives in `Configuration/SwaggerConfiguration.cs`.
- CORS in `Program.cs` allows any origin/method/header — code comment says it must not ship to prod.

## Conventions / gotchas

- JSON uses snake_case via `[JsonPropertyName]` on `Models/FlightPlan.cs` (e.g. `aircraft_identification`, `estimated_arrival_time`).
- The model is immutable (`init`-only properties). `FlightPlanId` is generated server-side on file (Guid) — never supplied by the client.
- MongoDB field names are snake_case and don't always match property names: Bson `departing_airport` → `DepartureAirport`, `estimated_arrival_time` → `ArrivalTime`, `flight_plan_id` → `FlightPlanId`. Keep this mapping in sync in `Data/MongoDbDatabase.cs` (`ConvertBsonToFlightPlan` / `FileFlightPlan` / `UpdateFlightPlan`).
- `appsettings.Development.json` is committed despite `.gitignore` listing `appsettings.*.json` — don't assume gitignore is enforced.
- `FlightPlanApi.http` references a `/weatherforecast/` route that does not exist (stale).
- Time-enroute endpoint computes `ArrivalTime - DepartureTime` in the controller.
