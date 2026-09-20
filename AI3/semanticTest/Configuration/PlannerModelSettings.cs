using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
namespace SemanticTest.Configuration;
public sealed record PlannerModelSettings(string ModelName, string Endpoint, string ApiKey, double Temperature = 0, int MaxTokens = 500)
{
    public static PlannerModelSettings FromEnvironment() => new(Required("AI_FOUNDRY_MODEL_NAME"), Required("AI_FOUNDRY_ENDPOINT"), Required("AI_FOUNDRY_API_KEY"));
    private static string Required(string name) => Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : throw new InvalidOperationException($"{name} must be provided through the environment.");
}
public static class PlannerKernelFactory
{
    public static Kernel Create(PlannerModelSettings settings) { var b = Kernel.CreateBuilder(); b.AddAzureOpenAIChatCompletion(settings.ModelName, settings.Endpoint, settings.ApiKey); return b.Build(); }
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
}
