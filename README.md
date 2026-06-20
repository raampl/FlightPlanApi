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

Access Swagger UI at `https://localhost:7001/swagger` or `http://localhost:5250/swagger/index.html`

## API Endpoints

- `GET /api/v1/flightplan` - Flight plan operations

## Development

```bash
# Start Mango db
brew services start mongodb/brew/mongodb-community

# Stop Mango db
brew services stop mongodb/brew/mongodb-community

# Build
dotnet build

# Run in development mode
dotnet run --environment Development
```

## More Test Data
https://mockaroo.com/d5a655d0