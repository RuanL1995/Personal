using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var plans = new ConcurrentDictionary<Guid, VacationPlan>();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "WebsiteTest" }));
app.MapGet("/api/plans", () => Results.Ok(plans.Values.OrderByDescending(plan => plan.CreatedAt)));
app.MapPost("/api/plans", (VacationPlan plan) =>
{
    var stored = plan with { Id = plan.Id == Guid.Empty ? Guid.NewGuid() : plan.Id, CreatedAt = DateTimeOffset.UtcNow };
    plans[stored.Id] = stored;
    return Results.Ok(stored);
});

app.Run("http://localhost:5190");

public sealed record Flight(string Id, string Origin, string Destination, string DepartureDate,
    string ReturnDate, decimal Price, string Airline, string Stops);
public sealed record Hotel(string Id, string Name, string City, decimal NightlyRate,
    int Rating, string Description);
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
