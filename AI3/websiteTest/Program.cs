// Environment variables: VACATION_API_BASE_URL (default http://localhost:5080), PORT (default 5090).
using System.Collections.Concurrent;
using System.Net.Http.Json;
using WebsiteTest.Contracts;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("VACATION_API_BASE_URL") ?? "http://localhost:5080") });
var app = builder.Build(); app.UseDefaultFiles(); app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok(new { status = "ok", browserHosted = true, synthetic = true }));
var flights = new ConcurrentBag<object>();
var bookings = new ConcurrentBag<object>();
var decisions = new ConcurrentBag<object>();
app.MapGet("/flights", (string? origin, string? destination, string? departDate) =>
{
    var result = new[] { new { id = Guid.NewGuid(), origin, destination, departDate, price = 499.99m, synthetic = true } };
    foreach (var item in result) flights.Add(item); return Results.Ok(result);
});
app.MapPost("/bookings", (BookingRequest request) => { var booking = new { id = Guid.NewGuid(), request.FlightId, request.Name, status = "confirmed" }; bookings.Add(booking); return Results.Created("/bookings/" + booking.id, booking); });
app.MapPost("/decisions", (DecisionRequest request) => { var decision = new { id = Guid.NewGuid(), request.PlanId, decision = request.Value, at = DateTimeOffset.UtcNow }; decisions.Add(decision); return Results.Created("/decisions/" + decision.id, decision); });
app.MapGet("/decisions", () => Results.Ok(decisions.ToArray()));
app.MapPost("/api/vacation/search", async (VacationSearch request, HttpClient api, HttpContext ctx, CancellationToken ct) => await Proxy(api, "/api/vacation/search", request, ctx, ct));
app.MapPost("/api/plans", async (PlanRequest request, HttpClient api, HttpContext ctx, CancellationToken ct) => await Proxy(api, "/api/plans", request, ctx, ct));
app.MapGet("/api/plans/{id:guid}/status", async (Guid id, HttpClient api, CancellationToken ct) => await GetProxy(api, $"/api/plans/{id}/status", ct));
app.MapPost("/api/plans/{id:guid}/approval", async (Guid id, HttpClient api, CancellationToken ct) => await PostProxy(api, $"/api/plans/{id}/approval", ct));
app.MapPost("/api/plans/{id:guid}/callback", async (Guid id, PlanCallback callback, HttpClient api, CancellationToken ct) => await Proxy(api, $"/api/plans/{id}/callback", callback, new DefaultHttpContext(), ct));
app.MapPost("/api/search", async (VacationSearch request, HttpClient api, HttpContext ctx, CancellationToken ct) => await Proxy(api, "/api/flights/search", request, ctx, ct));
app.MapPost("/api/book", async (BookingForm request, HttpClient api, HttpContext ctx, CancellationToken ct) => await Proxy(api, "/api/bookings", request, ctx, ct));
app.MapGet("/api/status/{runId:guid}", async (Guid runId, HttpClient api, CancellationToken ct) => { using var response = await api.GetAsync($"/api/status/{runId}", ct); return Results.Content(await response.Content.ReadAsStringAsync(ct), "application/json", statusCode: (int)response.StatusCode); });
app.Run($"http://localhost:{Environment.GetEnvironmentVariable("PORT") ?? "5090"}");
static async Task<IResult> GetProxy(HttpClient api, string path, CancellationToken ct) { using var response = await api.GetAsync(path, ct); return Results.Content(await response.Content.ReadAsStringAsync(ct), "application/json", statusCode: (int)response.StatusCode); }
static async Task<IResult> PostProxy(HttpClient api, string path, CancellationToken ct) { using var response = await api.PostAsync(path, null, ct); return Results.Content(await response.Content.ReadAsStringAsync(ct), "application/json", statusCode: (int)response.StatusCode); }
static async Task<IResult> Proxy<T>(HttpClient api, string path, T body, HttpContext ctx, CancellationToken ct) { using var message = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) }; if (ctx.Request.Headers.TryGetValue("Idempotency-Key", out var key)) message.Headers.TryAddWithoutValidation("Idempotency-Key", key.ToString()); using var response = await api.SendAsync(message, ct); return Results.Content(await response.Content.ReadAsStringAsync(ct), "application/json", statusCode: (int)response.StatusCode); }
public sealed record PlanRequest(string Request, bool RequireApproval = true);
public sealed record PlanCallback(string Status, string? Message = null);
public sealed record BookingForm(Guid FlightId, string TravelerName);
public sealed record BookingRequest(Guid FlightId, string Name);
public sealed record DecisionRequest(Guid PlanId, string Value);

