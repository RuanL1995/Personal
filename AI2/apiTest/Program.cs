using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); builder.Logging.AddJsonConsole();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.PropertyNameCaseInsensitive = true);
builder.Services.AddSingleton<PlannerStore>();
builder.Services.AddSingleton<BoundedRetry>();
var app = builder.Build();
app.Use(async (context, next) => {
    var correlation = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    if (!Guid.TryParse(correlation, out _)) correlation = Guid.NewGuid().ToString("D");
    context.Response.Headers["X-Correlation-ID"] = correlation;
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlation!, ["path"] = context.Request.Path.ToString() })) await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", synthetic = true }));
app.MapPost("/api/flights/search", async (FlightSearchRequest request, PlannerStore store, BoundedRetry retry, HttpContext http) => {
    if (request is null || string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination) || request.DepartureDate == default) return Results.BadRequest(new { error = "origin, destination and departureDate are required" });
    var key = Key(http); if (store.TryIdempotent(key, out var prior)) return Results.Ok(prior);
    var flights = await retry.RunAsync(() => Task.FromResult(store.Search(request)));
    store.SaveIdempotent(key, flights); store.RecordAudit("flight_search", key);
    return Results.Ok(flights);
});
app.MapPost("/api/bookings", async (BookingRequest request, PlannerStore store, BoundedRetry retry, HttpContext http) => {
    if (request is null || request.FlightId == Guid.Empty || string.IsNullOrWhiteSpace(request.TravelerName)) return Results.BadRequest(new { error = "flightId and travelerName are required" });
    var key = Key(http); if (store.TryIdempotent(key, out var prior)) return Results.Ok(prior);
    var booking = await retry.RunAsync(() => Task.FromResult(store.Book(request))); store.SaveIdempotent(key, booking); store.RecordAudit("booking_created", booking.BookingId.ToString("D"));
    return Results.Created($"/api/bookings/{booking.BookingId}", booking);
});
app.MapPost("/api/webhooks/booking", async (HttpRequest request, PlannerStore store, ILogger<Program> log) => {
    using var reader = new StreamReader(request.Body); var body = await reader.ReadToEndAsync();
    var secret = Environment.GetEnvironmentVariable("VACATION_WEBHOOK_SECRET");
    if (string.IsNullOrWhiteSpace(secret)) return Results.Problem("VACATION_WEBHOOK_SECRET must be configured in the environment.", statusCode: StatusCodes.Status503ServiceUnavailable);
    var signature = request.Headers["X-Webhook-Signature"].FirstOrDefault() ?? "";
    if (!Hmac.Verify(body, signature, secret)) return Results.Unauthorized();
    var evt = JsonSerializer.Deserialize<BookingWebhook>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); if (evt is null || evt.BookingId == Guid.Empty) return Results.BadRequest(new { error = "invalid booking event" });
    var accepted = store.RecordWebhook(evt); log.LogInformation("booking_webhook_received {EventId} {Duplicate}", evt.EventId, !accepted);
    return Results.Ok(new { accepted = true, duplicate = !accepted, eventId = evt.EventId });
});
app.MapPost("/api/pad/callback", (PadCallback callback, PlannerStore store, HttpContext http) => {
    if (callback is null || callback.RunId == Guid.Empty || string.IsNullOrWhiteSpace(callback.Status)) return Results.BadRequest(new { error = "runId and status are required" });
    store.RecordPad(callback); return Results.Ok(new { accepted = true, correlationId = http.Response.Headers["X-Correlation-ID"].ToString() });
});
app.Run("http://localhost:5080");

static string Key(HttpContext c) => c.Request.Headers["Idempotency-Key"].FirstOrDefault() ?? "";
public sealed record FlightSearchRequest(string Origin, string Destination, DateOnly DepartureDate, DateOnly? ReturnDate, int Travelers = 1);
public sealed record FlightOption(Guid FlightId, string Carrier, string FlightNumber, decimal Price, string Currency, DateOnly DepartureDate);
public sealed record FlightSearchResponse(IReadOnlyList<FlightOption> Flights, bool Synthetic = true);
public sealed record BookingRequest(Guid FlightId, string TravelerName);
public sealed record Booking(Guid BookingId, Guid FlightId, string TravelerName, string Status, DateTimeOffset CreatedAt, bool Synthetic = true);
public sealed record BookingWebhook(Guid EventId, Guid BookingId, string Status);
public sealed record PadCallback(Guid RunId, string Status, string? Message = null);
public sealed record AuditEvent(string Action, string Subject, DateTimeOffset RecordedAt);

public sealed class PlannerStore {
    private readonly ConcurrentDictionary<string, object> idem = new();
    private readonly ConcurrentDictionary<Guid, Booking> bookings = new();
    private readonly ConcurrentDictionary<Guid, BookingWebhook> webhooks = new();
    private readonly ConcurrentQueue<AuditEvent> audit = new();
    public FlightSearchResponse Search(FlightSearchRequest r) => new(new[] { new FlightOption(Guid.NewGuid(), "MockAir", "MA101", 499.99m, "USD", r.DepartureDate), new FlightOption(Guid.NewGuid(), "Synthetic Airways", "SA202", 599.99m, "USD", r.DepartureDate) });
    public Booking Book(BookingRequest r) { var b = new Booking(Guid.NewGuid(), r.FlightId, r.TravelerName, "confirmed", DateTimeOffset.UtcNow); bookings[b.BookingId] = b; return b; }
    public bool TryIdempotent(string key, out object? value)
    {
        value = null;
        return !string.IsNullOrWhiteSpace(key) && idem.TryGetValue(key, out value);
    }
    public void SaveIdempotent(string key, object value) { if (!string.IsNullOrWhiteSpace(key)) idem.TryAdd(key, value); }
    public bool RecordWebhook(BookingWebhook e)
    {
        var added = webhooks.TryAdd(e.EventId, e);
        if (added) RecordAudit("booking_webhook_received", e.EventId.ToString("D"));
        return added;
    }
    public void RecordPad(PadCallback c) => RecordAudit("pad_callback_received", c.RunId.ToString("D"));
    public void RecordAudit(string action, string subject) => audit.Enqueue(new AuditEvent(action, subject, DateTimeOffset.UtcNow));
}
public sealed class BoundedRetry { public async Task<T> RunAsync<T>(Func<Task<T>> action) { Exception? last = null; for (var i=0; i<3; i++) try { return await action(); } catch (Exception ex) { last = ex; await Task.Delay(10); } throw new InvalidOperationException("bounded retry exhausted", last); } }
public static class Hmac { public static bool Verify(string body, string supplied, string secret) { using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret)); var expected = Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant(); return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied.Trim().ToLowerInvariant())); } public static string Sign(string body, string secret) { using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret)); return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant(); } }
