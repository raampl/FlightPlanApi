namespace FlightPlanApi.Data;

public class MongoDbOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "pluralsight";
    public string CollectionName { get; set; } = "flight_plans";
}
