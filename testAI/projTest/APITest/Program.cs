using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var plans = new ConcurrentDictionary<Guid, VacationPlan>();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "APITest" }));

app.MapPost("/api/mock/flights", (FlightSearch request) =>
{
    var flights = new[]
    {
        new Flight("FL-100", request.Origin, request.Destination, request.DepartureDate,
            request.ReturnDate, 825m, "SkyLink Airways", "1 stop"),
        new Flight("FL-200", request.Origin, request.Destination, request.DepartureDate,
            request.ReturnDate, 1090m, "Northstar Air", "Non-stop")
    };
    return Results.Ok(flights);
});

app.MapPost("/api/mock/hotels", (HotelSearch request) =>
{
    var hotels = new[]
    {
        new Hotel("HT-100", "Harbor View Hotel", request.Destination, 145m, 4,
            "Central location with breakfast"),
        new Hotel("HT-200", "The Grand Residence", request.Destination, 220m, 5,
            "Luxury stay with airport transfer")
    };
    return Results.Ok(hotels);
});

app.MapPost("/api/plans", (VacationPlan plan) =>
{
    var stored = plan with { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
    plans[stored.Id] = stored;
    return Results.Created($"/api/plans/{stored.Id}", stored);
});

app.MapGet("/api/plans", () => Results.Ok(plans.Values.OrderByDescending(plan => plan.CreatedAt)));
app.MapGet("/api/plans/{id:guid}", (Guid id) =>
    plans.TryGetValue(id, out var plan) ? Results.Ok(plan) : Results.NotFound());

app.Run("http://localhost:5180");

public sealed record Flight(string Id, string Origin, string Destination, string DepartureDate,
    string ReturnDate, decimal Price, string Airline, string Stops);
public sealed record Hotel(string Id, string Name, string City, decimal NightlyRate,
    int Rating, string Description);
public sealed record FlightSearch(string Origin, string Destination, string DepartureDate, string ReturnDate);
public sealed record HotelSearch(string Destination, string DepartureDate, string ReturnDate);
public sealed record VacationPlan(
    Guid Id,
    string TravelerName,
    string Origin,
    string Destination,
    string DepartureDate,
    string ReturnDate,
    decimal Budget,
    string Summary,
    string Recommendation,
    Flight? Flight,
    Hotel? Hotel,
    IReadOnlyList<string> Activities,
    DateTimeOffset CreatedAt);
