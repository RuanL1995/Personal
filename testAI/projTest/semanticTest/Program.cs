using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

var apiUrl = Environment.GetEnvironmentVariable("VACATION_API_URL") ?? "http://localhost:5180";
var websiteUrl = Environment.GetEnvironmentVariable("VACATION_WEBSITE_URL") ?? "http://localhost:5190";
var offline = args.Contains("--offline", StringComparer.OrdinalIgnoreCase);
var apiKey = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_API_KEY")
    ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

Console.WriteLine("Vacation planner ready.");
if (!offline && string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Azure API key is missing. Open a new terminal after setting AZURE_AI_FOUNDRY_API_KEY, then try again.");
    return 1;
}
Console.WriteLine(offline
    ? "Running in offline mode (set Azure AI Foundry variables to use the LLM)."
    : "Running online with the Azure AI Foundry Semantic Kernel connector.");
Console.Write("Describe your vacation (for example: \"2 people from JNB to Paris in June under $3000\"): ");
var request = Console.ReadLine();
if (string.IsNullOrWhiteSpace(request))
{
    Console.Error.WriteLine("A vacation description is required.");
    return 1;
}

var requirements = offline ? VacationRequirements.FromText(request) : await AskAzureKernelAsync(request);
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

Console.WriteLine("Searching local mock flight and hotel APIs...");
var flights = await http.PostAsJsonAsync($"{apiUrl}/api/mock/flights", requirements);
flights.EnsureSuccessStatusCode();
var flightOptions = await flights.Content.ReadFromJsonAsync<List<Flight>>() ?? [];
var hotels = await http.PostAsJsonAsync($"{apiUrl}/api/mock/hotels", requirements);
hotels.EnsureSuccessStatusCode();
var hotelOptions = await hotels.Content.ReadFromJsonAsync<List<Hotel>>() ?? [];

var selectedFlight = flightOptions.OrderBy(flight => flight.Price).FirstOrDefault();
var selectedHotel = hotelOptions.OrderBy(hotel => hotel.NightlyRate).FirstOrDefault();
var plan = new VacationPlan(
    Guid.Empty,
    requirements.TravelerName,
    requirements.Origin,
    requirements.Destination,
    requirements.DepartureDate,
    requirements.ReturnDate,
    requirements.Budget,
    $"A {requirements.PreferencesText} trip from {requirements.Origin} to {requirements.Destination}.",
    $"Choose {selectedFlight?.Airline} ({selectedFlight?.Stops}) and stay at {selectedHotel?.Name}.",
    selectedFlight,
    selectedHotel,
    ["Local food tour", "City highlights walk", "Flexible free day"],
    DateTimeOffset.UtcNow);

var posted = await http.PostAsJsonAsync($"{websiteUrl}/api/plans", plan);
posted.EnsureSuccessStatusCode();
var savedPlan = await posted.Content.ReadFromJsonAsync<VacationPlan>();
Console.WriteLine($"Plan published to {websiteUrl}/");
Console.WriteLine(JsonSerializer.Serialize(savedPlan, new JsonSerializerOptions { WriteIndented = true }));
return 0;

static async Task<VacationRequirements> AskAzureKernelAsync(string request)
{
    var endpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT")
        ?? "https://aiwork-foundry-swe-test.cognitiveservices.azure.com/";
    var deployment = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_DEPLOYMENT")
        ?? "gpt-5-mini-test";
    var kernelBuilder = Kernel.CreateBuilder();
    var apiKey = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_API_KEY")
        ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("Azure API key is missing. Set AZURE_AI_FOUNDRY_API_KEY or AZURE_OPENAI_API_KEY.");
    kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey);
    var kernel = kernelBuilder.Build();
    var prompt = $$"""
        You are a vacation-planning assistant. Convert the user request below to JSON only.
        Use exactly these properties: travelerName, origin, destination, departureDate, returnDate,
        budget, preferences. Dates must be yyyy-MM-dd and budget must be a number.
        If a value is missing, infer a sensible value. User request: {{request}}
        """;
    var response = await kernel.InvokePromptAsync(prompt);
    var json = response.ToString().Trim().Trim('`');
    if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase))
        json = json[4..].Trim();
    return JsonSerializer.Deserialize<VacationRequirements>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("The Azure model returned no vacation requirements.");
}

public sealed record VacationRequirements(
    string TravelerName,
    string Origin,
    string Destination,
    string DepartureDate,
    string ReturnDate,
    decimal Budget,
    JsonElement Preferences)
{
    [JsonIgnore]
    public string PreferencesText => Preferences.ValueKind == JsonValueKind.Array
        ? string.Join(", ", Preferences.EnumerateArray().Select(FormatPreference))
        : FormatPreference(Preferences);

    private static string FormatPreference(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Object => string.Join(", ", value.EnumerateObject()
            .Select(property => $"{property.Name}: {FormatPreference(property.Value)}")),
        JsonValueKind.Array => string.Join(", ", value.EnumerateArray().Select(FormatPreference)),
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        _ => value.ToString()
    };

    public static VacationRequirements FromText(string text) =>
        new("Demo traveler", "JNB", "CPT", DateTime.UtcNow.AddMonths(2).ToString("yyyy-MM-dd"),
            DateTime.UtcNow.AddMonths(2).AddDays(7).ToString("yyyy-MM-dd"), 3000m,
            JsonDocument.Parse(JsonSerializer.Serialize(text)).RootElement.Clone());
}

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
