# Application Flow — Sequence Diagrams

Request flows through `BasicAuthenticationHandler` → `FlightPlanController` → `MongoDbDatabase` → MongoDB, as wired in `Program.cs`.

## Middleware pipeline

Order matters: HTTPS redirection and CORS run before authentication, and authorization runs before the controller.

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant MW as ASP.NET Core Pipeline
    participant HTTPS as UseHttpsRedirection
    participant CORS as UseCors (allow any)
    participant Auth as UseAuthentication
    participant Authz as UseAuthorization
    participant Ctrl as MapControllers

    Client->>MW: Request
    MW->>HTTPS: Enforce HTTPS
    HTTPS->>CORS: Apply CORS headers
    CORS->>Auth: Authenticate via BasicAuthentication
    Auth->>Authz: Principal (or 401)
    Authz->>Ctrl: [Authorize] satisfied
    Ctrl-->>Client: Response
```

## Authenticated request (full flow)

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant MW as ASP.NET Core Pipeline
    participant Auth as BasicAuthenticationHandler
    participant UserSvc as UserService
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB (flight_plans)

    Client->>MW: HTTP request + Authorization: Basic base64(user:pass)
    MW->>MW: HTTPS redirect, CORS
    MW->>Auth: HandleAuthenticateAsync()
    Auth->>Auth: Parse header, base64-decode, split on ':'

    alt Header missing or malformed
        Auth-->>MW: AuthenticateResult.Fail
        MW-->>Client: 401 Unauthorized
    else Header parsed
        Auth->>UserSvc: Authenticate(username, password)
        UserSvc-->>Auth: User? (compared with AdminCredentials)
        alt Invalid credentials
            Auth-->>MW: Fail("Invalid Credentials")
            MW-->>Client: 401 Unauthorized
        else Valid credentials
            Auth-->>MW: Success (ClaimsPrincipal)
            MW->>Ctrl: Route to [Authorize] action
            Ctrl->>DB: IDatabaseAdapter call
            DB->>Mongo: Find / InsertOne / UpdateOne / DeleteOne
            Mongo-->>DB: BsonDocument(s) or write result
            DB-->>Ctrl: FlightPlan / TransactionResult / bool
            Ctrl-->>MW: 200 / 204 / 400 / 404 / 500
            MW-->>Client: HTTP response
        end
    end
```

## GET `/api/v1/flightplan` (list)

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB

    Client->>Ctrl: GET /api/v1/flightplan
    Ctrl->>DB: GetAllFlightPlans()
    DB->>Mongo: find({})
    Mongo-->>DB: BsonDocument list
    DB->>DB: ConvertBsonToFlightPlan for each document
    DB-->>Ctrl: List of FlightPlan
    alt List is empty
        Ctrl-->>Client: 204 No Content
    else Has flight plans
        Ctrl-->>Client: 200 OK + JSON array (snake_case)
    end
```

## GET `/api/v1/flightplan/{flightPlanId}` and derived views

The departure-airport, route, and time-enroute endpoints all load the full plan first, then return one derived value.

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB

    Client->>Ctrl: GET /{id} | /airport/departure/{id} | /route/{id} | /time/enroute/{id}
    Ctrl->>DB: GetFlightPlanById(flightPlanId)
    DB->>Mongo: findOne({ flight_plan_id: id })
    Mongo-->>DB: BsonDocument or null
    DB->>DB: ConvertBsonToFlightPlan(document)
    DB-->>Ctrl: FlightPlan?

    alt Flight plan not found
        Ctrl-->>Client: 404 Not Found
    else GET /{id}
        Ctrl-->>Client: 200 OK + full flight plan
    else GET /airport/departure/{id}
        Ctrl-->>Client: 200 OK + departing_airport
    else GET /route/{id}
        Ctrl-->>Client: 200 OK + route
    else GET /time/enroute/{id}
        Ctrl->>Ctrl: arrival_time - departure_time
        Ctrl-->>Client: 200 OK + TimeSpan
    end
```

## POST `/api/v1/flightplan/file`

`flight_plan_id` is generated server-side with `Guid.NewGuid()`. Success is only reported when the inserted document receives an `_id` of type `ObjectId`.

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB

    Client->>Ctrl: POST /api/v1/flightplan/file + JSON body
    Ctrl->>Ctrl: Validate ModelState
    alt Model invalid
        Ctrl-->>Client: 400 Bad Request
    else Model valid
        Ctrl->>DB: FileFlightPlan(flightPlan)
        DB->>DB: Build BsonDocument + generate flight_plan_id (GUID)
        DB->>Mongo: insertOne(document)
        alt Insert throws (e.g. MongoDB unavailable)
            Mongo-->>DB: exception (logged)
            DB-->>Ctrl: ServerError
            Ctrl-->>Client: 500 Internal Server Error
        else Insert acknowledged
            Mongo-->>DB: write result with _id
            alt _id is ObjectId
                DB-->>Ctrl: Success
                Ctrl-->>Client: 200 OK
            else _id missing or not ObjectId
                DB-->>Ctrl: BadRequest
                Ctrl-->>Client: 400 Bad Request
            end
        end
    end
```

## PUT `/api/v1/flightplan`

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB

    Client->>Ctrl: PUT /api/v1/flightplan + JSON body (incl. flight_plan_id)
    Ctrl->>Ctrl: Validate ModelState
    alt Model invalid
        Ctrl-->>Client: 400 Bad Request
    else Model valid
        Ctrl->>DB: UpdateFlightPlan(flight_plan_id, flightPlan)
        DB->>Mongo: updateOne({ flight_plan_id }, $set {...})
        Mongo-->>DB: UpdateResult (MatchedCount, ModifiedCount)
        alt MatchedCount == 0
            DB-->>Ctrl: NotFound
            Ctrl-->>Client: 404 Not Found
        else ModifiedCount > 0
            DB-->>Ctrl: Success
            Ctrl-->>Client: 200 OK
        else MatchedCount > 0 and ModifiedCount == 0
            DB-->>Ctrl: ServerError
            Ctrl-->>Client: 500 Internal Server Error
        end
    end
```

## DELETE `/api/v1/flightplan/{flightPlanId}`

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant Ctrl as FlightPlanController
    participant DB as MongoDbDatabase
    participant Mongo as MongoDB

    Client->>Ctrl: DELETE /api/v1/flightplan/{flightPlanId}
    Ctrl->>DB: DeleteFlightPlanById(flightPlanId)
    DB->>Mongo: deleteOne({ flight_plan_id: id })
    Mongo-->>DB: DeleteResult (DeletedCount)
    alt DeletedCount > 0
        DB-->>Ctrl: true
        Ctrl-->>Client: 200 OK
    else Nothing deleted
        DB-->>Ctrl: false
        Ctrl-->>Client: 404 Not Found
    end
```

## Notes

- All routes are under `/api/v1/flightplan` and carry `[Authorize]`; failures surface as `401 Unauthorized` from the authentication middleware.
- JSON payloads use snake_case (`Models/FlightPlan.cs`), while MongoDB field names are mapped explicitly in `MongoDbDatabase.ConvertBsonToFlightPlan`, `FileFlightPlan`, and `UpdateFlightPlan`.
- There is no repository layer despite older docs: the controller depends directly on `IDatabaseAdapter` (`MongoDbDatabase`), which is registered scoped in `Program.cs`.
