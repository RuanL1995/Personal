// Environment variables: AI_FOUNDRY_ENDPOINT, AI_FOUNDRY_MODEL_NAME, AI_FOUNDRY_API_KEY. Optional --test is deterministic/offline.
using System.Text.Json;
using SemanticTest.Configuration;
using SemanticTest.Contracts;

var test = args.Contains("--test", StringComparer.OrdinalIgnoreCase);
var input = test ? "Test User,test@example.com,JNB,CPT,2026-10-01,2026-10-05,10000,cheap" :
    ReadInput();
var plan = await new MockPlannerModel().CreatePlanAsync(input);
Console.WriteLine(JsonSerializer.Serialize(plan, PlannerKernelFactory.JsonOptions));
if (!test) { _ = PlannerKernelFactory.Create(PlannerModelSettings.FromEnvironment()); Console.WriteLine("AI Foundry kernel configured."); }
else Console.WriteLine("Deterministic test mode.");

static string ReadInput()
{
    Console.Write("name: "); var name = Console.ReadLine() ?? "";
    Console.Write("email: "); var email = Console.ReadLine() ?? "";
    Console.Write("origin: "); var origin = Console.ReadLine() ?? "";
    Console.Write("destination: "); var destination = Console.ReadLine() ?? "";
    Console.Write("departDate: "); var depart = Console.ReadLine() ?? "";
    Console.Write("returnDate: "); var ret = Console.ReadLine() ?? "";
    Console.Write("budget: "); var budget = Console.ReadLine() ?? "";
    Console.Write("preferences: "); var prefs = Console.ReadLine() ?? "";
    return string.Join(",", name, email, origin, destination, depart, ret, budget, prefs);
}
