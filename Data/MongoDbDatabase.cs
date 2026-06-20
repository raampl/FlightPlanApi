namespace FlightPlanApi.Data
{
    public class MongoDbDatabase : IDatabaseAdapter
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly ILogger<MongoDbDatabase> _logger;

        public MongoDbDatabase(IOptions<MongoDbOptions> options, ILogger<MongoDbDatabase> logger)
        {
            _logger = logger;
            var mongoOptions = options.Value;
            var client = new MongoClient(mongoOptions.ConnectionString);
            var database = client.GetDatabase(mongoOptions.DatabaseName);
            _collection = database.GetCollection<BsonDocument>(mongoOptions.CollectionName);
        }

        public async Task<List<FlightPlan>> GetAllFlightPlans()
        {
            var documents = await _collection.Find(_ => true).ToListAsync();

            return documents?.Select(ConvertBsonToFlightPlan).OfType<FlightPlan>().ToList() ?? [];
        }

        public async Task<FlightPlan?> GetFlightPlanById(string flightPlanId)
        {
            var document = await _collection.Find(
                Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId)).FirstOrDefaultAsync();

            return ConvertBsonToFlightPlan(document);
        }

        public async Task<TransactionResult> FileFlightPlan(FlightPlan flightPlan)
        {

            var document = new BsonDocument
            {
                {"flight_plan_id", Guid.NewGuid().ToString("N") },
                {"altitude", flightPlan.Altitude },
                {"airspeed", flightPlan.Airspeed },
                {"aircraft_identification", flightPlan.AircraftIdentification },
                {"aircraft_type", flightPlan.AircraftType },
                {"arrival_airport", flightPlan.ArrivalAirport },
                {"flight_type", flightPlan.FlightType },
                {"departing_airport", flightPlan.DepartureAirport },
                {"departure_time", flightPlan.DepartureTime },
                {"estimated_arrival_time", flightPlan.ArrivalTime },
                {"route", flightPlan.Route },
                {"remarks", flightPlan.Remarks },
                {"fuel_hours", flightPlan.FuelHours },
                {"fuel_minutes", flightPlan.FuelMinutes },
                {"number_onboard", flightPlan.NumberOnBoard }
            };

            try
            {
                await _collection.InsertOneAsync(document);
                
                if (document["_id"].IsObjectId)
                {
                    return TransactionResult.Success; 
                }

                return TransactionResult.BadRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to file flight plan.");
                return TransactionResult.ServerError;
            }
        }

        public async Task<bool> DeleteFlightPlanById(string flightPlanId)
        {
            var result = await _collection.DeleteOneAsync(
                Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId));

            return result.DeletedCount > 0;
        }

        public async Task<TransactionResult> UpdateFlightPlan(string flightPlanId, FlightPlan flightPlan)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId);
            var update = Builders<BsonDocument>.Update
                .Set("altitude", flightPlan.Altitude)
                .Set("airspeed", flightPlan.Airspeed)
                .Set("aircraft_identification", flightPlan.AircraftIdentification)
                .Set("aircraft_type", flightPlan.AircraftType)
                .Set("arrival_airport", flightPlan.ArrivalAirport)
                .Set("flight_type", flightPlan.FlightType)
                .Set("departing_airport", flightPlan.DepartureAirport)
                .Set("departure_time", flightPlan.DepartureTime)
                .Set("estimated_arrival_time", flightPlan.ArrivalTime)
                .Set("route", flightPlan.Route)
                .Set("remarks", flightPlan.Remarks)
                .Set("fuel_hours", flightPlan.FuelHours)
                .Set("fuel_minutes", flightPlan.FuelMinutes)
                .Set("number_onboard", flightPlan.NumberOnBoard);
            var result = await _collection.UpdateOneAsync(filter, update);

            if(result.MatchedCount == 0)
            {
                return TransactionResult.NotFound;
            }

            if(result.ModifiedCount > 0)
            {
                return TransactionResult.Success;
            }

            return TransactionResult.ServerError;
        }

        private FlightPlan? ConvertBsonToFlightPlan(BsonDocument document)
        {
            if (document == null) return null;

            return new FlightPlan
            {
                FlightPlanId = document["flight_plan_id"].AsString,
                Altitude = document["altitude"].AsInt32,
                Airspeed = document["airspeed"].AsInt32,
                AircraftIdentification = document["aircraft_identification"].AsString,
                AircraftType = document["aircraft_type"].AsString,
                ArrivalAirport = document["arrival_airport"].AsString,
                FlightType = document["flight_type"].AsString,
                DepartureAirport = document["departing_airport"].AsString,
                DepartureTime = document["departure_time"].AsBsonDateTime.ToUniversalTime(),
                ArrivalTime = document["estimated_arrival_time"].AsBsonDateTime.ToUniversalTime(),
                Route = document["route"].AsString,
                Remarks = document["remarks"].AsString,
                FuelHours = document["fuel_hours"].AsInt32,
                FuelMinutes = document["fuel_minutes"].AsInt32,
                NumberOnBoard = document["number_onboard"].AsInt32
            };
        }
    }
}
