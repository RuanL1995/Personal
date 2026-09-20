using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SemanticTest.Configuration;
public sealed record PlannerModelSettings(string DeploymentName, string Endpoint, string ApiKey, double Temperature, int MaxTokens, string ResponseFormat = "json_object") {
 public static PlannerModelSettings FromEnvironment() => new(
  Required("AZURE_OPENAI_DEPLOYMENT_NAME"),
  Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT") ?? Required("AZURE_OPENAI_ENDPOINT"),
  Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_API_KEY") ?? Required("AZURE_OPENAI_API_KEY"), 0, 800);
 static string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"{name} must be provided through the environment.");
}
public static class PlannerKernelFactory {
 public static Kernel Create(PlannerModelSettings settings) { var b=Kernel.CreateBuilder(); b.AddAzureOpenAIChatCompletion(settings.DeploymentName, settings.Endpoint, settings.ApiKey); return b.Build(); }
 public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy=JsonNamingPolicy.CamelCase, WriteIndented=false };
}
