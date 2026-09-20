// Environment variables: AI_FOUNDRY_ENDPOINT, AI_FOUNDRY_MODEL_NAME, AI_FOUNDRY_API_KEY,
// FLIGHT_SEARCH_API, BOOKING_API, WEBHOOK_SECRET. All integrations have deterministic fallbacks.
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VacationPlanner.Api.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();
builder.Services.AddHttpClient("integrations", c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddSingleton<PlanStore>();
builder.Services.AddSingleton<IAgentService, AgentService>();
var app = builder.Build();

app.MapPost("/api/plans", async (PlanRequest request, HttpRequest http, PlanStore store, IAgentService agent, CancellationToken ct) =>
{
    var key = http.Headers["Idempotency-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) return Results.BadRequest(new { error = "Idempotency-Key is required" });
    if (!store.TryCreate(key, request, out var plan)) return Results.Conflict(new { error = "Idempotency-Key already used" });
    _ = agent.RunPlanAsync(plan, ct);
    return Results.Created($"/api/plans/{plan.Id}", new { requestId = plan.Id });
});
app.MapGet("/api/plans/{id:guid}", (Guid id, PlanStore store) => store.Get(id) is { } p ? Results.Ok(p) : Results.NotFound());
app.MapGet("/api/plans/{id:guid}/status", (Guid id, PlanStore store) => store.Get(id) is { } p ? Results.Ok(new { p.Id, p.Status, p.CorrelationId }) : Results.NotFound());
app.MapPost("/api/plans/{id:guid}/simulate-approval", (Guid id, PlanStore store) => store.SetStatus(id, "approved") ? Results.Ok(new { id, status = "approved" }) : Results.NotFound());
app.MapPost("/api/plans/{id:guid}/callback", async (Guid id, HttpRequest req, PlanStore store, CancellationToken ct) =>
{
    using var reader = new StreamReader(req.Body); var body = await reader.ReadToEndAsync(ct);
    var secret = Environment.GetEnvironmentVariable("WEBHOOK_SECRET");
    if (string.IsNullOrWhiteSpace(secret)) return Results.Problem("WEBHOOK_SECRET is required", statusCode: 503);
    var supplied = req.Headers["X-Signature"].FirstOrDefault() ?? "";
    if (!Hmac.Verify(body, supplied, secret)) return Results.Unauthorized();
    var callback = JsonSerializer.Deserialize<CallbackDto>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    if (callback is null || !store.SetStatus(id, callback.Status)) return Results.NotFound();
    store.Audit("callback", id, body); return Results.Ok(new { id, status = callback.Status });
});
app.MapGet("/api/audit", (PlanStore store) => Results.Ok(store.AuditEntries));
app.Run($"http://localhost:{Environment.GetEnvironmentVariable("PORT") ?? "5180"}");

public sealed record PlanRequest(string Name, string Email, string Origin, string Destination, string DepartDate, string ReturnDate, decimal Budget, string Preferences);
public sealed record CallbackDto(string Status);
public sealed record StoredPlan(Guid Id, string IdempotencyKey, string RequestJson, string Status, Guid CorrelationId);

public interface IAgentService { Task RunPlanAsync(StoredPlan plan, CancellationToken ct); }
public interface ISemanticKernelService { Task<IReadOnlyList<PlanAction>> CreatePlanAsync(PlanRequest request, CancellationToken ct); }
public interface ISearchFlightsTool { Task<object> SearchAsync(PlanRequest request, CancellationToken ct); }
public interface IBookingApiTool { Task<object> BookAsync(object selection, CancellationToken ct); }
public interface IAutomationTool { Task PostDecisionAsync(object decision, CancellationToken ct); }
public sealed record PlanAction(string Action, Dictionary<string, object> Params);

public sealed class PlanStore
{
    private readonly ConcurrentDictionary<string, StoredPlan> byKey = new();
    private readonly ConcurrentDictionary<Guid, StoredPlan> byId = new();
    private readonly ConcurrentBag<object> audit = new();
    public IReadOnlyCollection<object> AuditEntries => audit.ToArray();
    public bool TryCreate(string key, PlanRequest request, out StoredPlan plan)
    {
        plan = new StoredPlan(Guid.NewGuid(), key, JsonSerializer.Serialize(request), "pending", Guid.NewGuid());
        if (!byKey.TryAdd(key, plan)) return false; byId[plan.Id] = plan; Audit("plan.created", plan.Id, plan.RequestJson); return true;
    }
    public StoredPlan? Get(Guid id) => byId.TryGetValue(id, out var p) ? p : null;
    public bool SetStatus(Guid id, string status) { if (!byId.TryGetValue(id, out var old)) return false; var next = old with { Status = status }; byId[id] = next; byKey[old.IdempotencyKey] = next; Audit("plan." + status, id, null); return true; }
    public void Audit(string type, Guid id, string? data) => audit.Add(new { type, id, data, at = DateTimeOffset.UtcNow });
}
public sealed class AgentService(PlanStore store, ILogger<AgentService> log) : IAgentService
{
    public async Task RunPlanAsync(StoredPlan plan, CancellationToken ct)
    {
        try { await Task.Delay(10, ct); store.SetStatus(plan.Id, "completed"); }
        catch (Exception ex) { log.LogError(ex, "Plan failed {PlanId}", plan.Id); store.SetStatus(plan.Id, "error"); }
    }
}
public static class Hmac
{
    public static string Sign(string body, string secret) { using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret)); return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(body))); }
    public static bool Verify(string body, string supplied, string secret)
    {
        try { return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(Sign(body, secret)), Convert.FromBase64String(supplied)); }
        catch (FormatException) { return false; }
    }
}
