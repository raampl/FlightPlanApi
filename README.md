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

# Build and run
dotnet run --profile https
```

The API will be available at:
- **HTTP**: `http://localhost:5250`
- **HTTPS**: `https://localhost:7001`

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

## API Endpoints

All endpoints require authentication via the `Authorize` header.

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/flightplan` | List all flight plans |
| `GET` | `/api/v1/flightplan/{flightPlanId}` | Get flight plan by ID |
| `POST` | `/api/v1/flightplan/file` | File a new flight plan |
| `PUT` | `/api/v1/flightplan` | Update an existing flight plan |
| `DELETE` | `/api/v1/flightplan/{flightPlanId}` | Delete a flight plan |
| `GET` | `/api/v1/flightplan/airport/departure/{flightPlanId}` | Get departure airport |
| `GET` | `/api/v1/flightplan/route/{flightPlanId}` | Get flight route |
| `GET` | `/api/v1/flightplan/time/enroute/{flightPlanId}` | Get estimated time enroute |

## Swagger

Access Swagger UI at `https://localhost:7001/swagger` or `http://localhost:5250/swagger/index.html`

## Development

```bash
# Start MongoDB
brew services start mongodb/brew/mongodb-community

# Stop MongoDB
brew services stop mongodb/brew/mongodb-community

# Build
dotnet build

# Run in development mode
dotnet run --environment Development
```

## Project Structure

```
FlightPlanApi/
├── Authentication/       # Basic auth handler, user service
├── Configuration/        # Swagger setup
├── Controllers/          # API controllers
├── Data/                 # Database adapter, repository
├── Models/               # Domain models
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration
```

## Test Data

https://mockaroo.com/d5a655d0
