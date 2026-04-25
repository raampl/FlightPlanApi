# FlightPlanApi

ASP.NET Core Web API project.

## Getting Started

```bash
dotnet run --profile https
```

The API will be available at:
- **HTTP**: `http://localhost:5250`
- **HTTPS**: `https://localhost:7001`

## Swagger

Access Swagger UI at `https://localhost:7001/swagger`

## API Endpoints

- `GET /api/v1/flightplan` - Flight plan operations

## Development

```bash
# Build
dotnet build

# Run in development mode
dotnet run --environment Development
```