using System.Text.Json.Serialization;

namespace FlightPlanApi.Models;

public class FlightPlan
{
    [JsonPropertyName("flight_plan_id")]
    public required string FlightPlanId { get; init; }

    [JsonPropertyName("aircraft_identification")]
    public required string AircraftIdentification { get; init; }

    [JsonPropertyName("aircraft_type")]
    public required string AircraftType { get; init; }

    [JsonPropertyName("airspeed")]
    public int Airspeed { get; init; }

    [JsonPropertyName("altitude")]
    public int Altitude { get; init; }

    [JsonPropertyName("flight_type")]
    public required string FlightType { get; init; }

    [JsonPropertyName("fuel_hours")]
    public int FuelHours { get; init; }

    [JsonPropertyName("fuel_minutes")]
    public int FuelMinutes { get; init; }

    [JsonPropertyName("departure_time")]
    public DateTime DepartureTime { get; init; }

    [JsonPropertyName("estimated_arrival_time")]
    public DateTime ArrivalTime { get; init; }

    [JsonPropertyName("departing_airport")]
    public required string DepartureAirport { get; init; }

    [JsonPropertyName("arrival_airport")]
    public required string ArrivalAirport { get; init; }

    [JsonPropertyName("route")]
    public required string Route { get; init; }

    [JsonPropertyName("remarks")]
    public required string Remarks { get; init; }

    [JsonPropertyName("number_onboard")]
    public int NumberOnBoard { get; init; }
}

