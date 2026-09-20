using System.Text.Json;
using SemanticTest.Configuration;
using SemanticTest.Contracts;

var model = new MockPlannerModel();
var plan = await model.CreatePlanAsync("Plan a synthetic vacation");
Console.WriteLine(JsonSerializer.Serialize(plan, PlannerKernelFactory.JsonOptions));

var azureConfigured = new[]
{
    Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT") ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
    Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME"),
    Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_API_KEY") ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
}.All(value => !string.IsNullOrWhiteSpace(value));

if (azureConfigured)
{
    var settings = PlannerModelSettings.FromEnvironment();
    _ = PlannerKernelFactory.Create(settings);
    Console.WriteLine("Azure AI Foundry kernel configured; mock output remains the default local-safe execution path.");
}
else
{
    Console.WriteLine("Synthetic mode. Set Azure AI Foundry/Azure OpenAI environment variables to configure the kernel.");
}
