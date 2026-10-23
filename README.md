# FlightPlanApi

ASP.NET Core Web API for filing and managing flight plans, backed by MongoDB.

## Tech Stack

- **.NET 10** (target framework `net10.0`)
- **MongoDB** (via `MongoDB.Driver`)
- **Swagger / Swashbuckle** (API documentation)
- **Basic Authentication**

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MongoDB (running locally on `localhost:27017`)

## Getting Started

```bash
# Start MongoDB
brew services start mongodb/brew/mongodb-community

# Build and run (HTTPS profile, Development environment)
dotnet run --profile https
```

Available launch profiles (`Properties/launchSettings.json`):

| Profile | URL |
|---|---|
| `https` | `https://localhost:3001` (also binds `http://localhost:3000`) |
| `http` | `http://localhost:5250` |

Both profiles set `ASPNETCORE_ENVIRONMENT=Development`.

## Swagger

Swagger UI is available at `https://localhost:3001/swagger` (or `http://localhost:5250/swagger/index.html` on the `http` profile).

> **Note:** Swagger is only enabled when the environment is `Development` (guarded in `Program.cs`). The Swagger config lives in `Configuration/SwaggerConfiguration.cs`.

## Configuration

All settings are in `appsettings.json`:

| Section | Key | Default |
|---|---|---|
| `AdminCredentials` | `Username` | `admin` |
| `AdminCredentials` | `Password` | `P@ssw0rd` |
| `MongoDb` | `ConnectionString` | `mongodb://localhost:27017` |
| `MongoDb` | `DatabaseName` | `pluralsight` |
| `MongoDb` | `CollectionName` | `flight_plans` |

## Authentication

The API uses **Basic Authentication**. Include an `Authorization: Basic <base64>` header with valid credentials.

Default credentials: `admin` / `P@ssw0rd`

## CORS

Cross-Origin Resource Sharing is enabled to allow any origin, method, and header (development convenience — should be restricted before production).

## API Endpoints

All endpoints require authentication via the `Authorize` header.

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/flightplan` | List all flight plans (`204` when none are filed) |
| `GET` | `/api/v1/flightplan/{flightPlanId}` | Get flight plan by ID |
| `POST` | `/api/v1/flightplan/file` | File a new flight plan |
| `PUT` | `/api/v1/flightplan` | Update an existing flight plan |
| `DELETE` | `/api/v1/flightplan/{flightPlanId}` | Delete a flight plan |
| `GET` | `/api/v1/flightplan/airport/departure/{flightPlanId}` | Get departure airport |
| `GET` | `/api/v1/flightplan/route/{flightPlanId}` | Get flight route |
| `GET` | `/api/v1/flightplan/time/enroute/{flightPlanId}` | Get estimated time enroute (`arrival_time - departure_time`) |

### Sample request — `POST /api/v1/flightplan/file`

JSON properties use **snake_case** (`[JsonPropertyName]` on `Models/FlightPlan.cs`).

```json
{
  "aircraft_identification": "N67SVS",
  "aircraft_type": "PA-34 Piper Seneca",
  "airspeed": 128,
  "altitude": 12000,
  "flight_type": "VFR",
  "fuel_hours": 3,
  "fuel_minutes": 41,
  "departure_time": "2022-07-08T00:26:45Z",
  "estimated_arrival_time": "2022-07-08T03:49:45Z",
  "departing_airport": "KBXA",
  "arrival_airport": "KNZY",
  "route": "KBXA JOH J46 DMDUP J46 KNZY",
  "remarks": "",
  "number_onboard": 4
}
```

Notes:

- `flight_plan_id` is generated server-side (GUID) when a plan is filed — do not supply it when filing. It is required on `PUT`/`DELETE`/lookups.
- The `FlightPlan` model is immutable (`init`-only properties).

## Development

```bash
# Start MongoDB
brew services start mongodb/brew/mongodb-community

# Stop MongoDB
brew services stop mongodb/brew/mongodb-community

# Build (emits an expected NU1902 vulnerability warning for Swashbuckle 6.2.3 — known issue)
dotnet build

# Run (both profiles start in Development mode)
dotnet run --profile https
dotnet run --profile http
```

- There is **no test project** — `dotnet test` has nothing to run.
- The app requires MongoDB at request time; it fails without a local instance.
- `FlightPlanApi.http` references a stale `/weatherforecast/` route that does not exist — use the endpoints table above instead.

## Project Structure

```
FlightPlanApi/
├── Authentication/           # BasicAuthenticationHandler, UserService, AdminCredentials
├── Configuration/            # SwaggerConfiguration
├── Controllers/              # FlightPlanController
├── Data/                     # IDatabaseAdapter, MongoDbDatabase, MongoDbOptions, TransactionResult
├── Models/                   # FlightPlan (snake_case JSON via JsonPropertyName)
├── Properties/               # launchSettings.json (http/https profiles)
├── docs/                     # Mermaid sequence diagrams (application flow)
├── GlobalUsings.cs           # Global using directives
├── Program.cs                # Application entry point (DI, Swagger, CORS, auth wiring)
├── appsettings.json          # Configuration
└── appsettings.Development.json
```

## Request Flow

See [docs/sequence-diagram.md](docs/sequence-diagram.md) for Mermaid sequence diagrams covering the middleware pipeline, authentication, and each endpoint's controller → `IDatabaseAdapter` → MongoDB path.

## Test Data

https://mockaroo.com/d5a655d0
