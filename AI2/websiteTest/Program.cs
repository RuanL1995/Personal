using System.Net.Http.Json;
using WebsiteTest.Contracts;
using WebsiteTest.Mocks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("VACATION_API_BASE_URL") ?? "http://localhost:5080")
});
builder.Services.AddSingleton<MockTravelWebsite>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok", browserHosted = true, synthetic = true }));
app.MapPost("/api/search", async (VacationSearch request, HttpClient api, CancellationToken cancellationToken) =>
{
    using var response = await api.PostAsJsonAsync("/api/flights/search", request, cancellationToken);
    return Results.Content(await response.Content.ReadAsStringAsync(cancellationToken),
        "application/json", statusCode: (int)response.StatusCode);
});
app.MapPost("/api/book", async (BookingForm request, HttpClient api, CancellationToken cancellationToken) =>
{
    using var response = await api.PostAsJsonAsync("/api/bookings", request, cancellationToken);
    return Results.Content(await response.Content.ReadAsStringAsync(cancellationToken),
        "application/json", statusCode: (int)response.StatusCode);
});

app.Run("http://localhost:5090");

public sealed record BookingForm(Guid FlightId, string TravelerName);
