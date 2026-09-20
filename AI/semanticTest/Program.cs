using Microsoft.SemanticKernel;
using SemanticTest.Configuration;
using SemanticTest.Plugins;

var options = AzureOpenAiOptions.FromEnvironment();
var apiBaseUrl = Environment.GetEnvironmentVariable("KYC_API_BASE_URL")
    ?? "http://localhost:5080";

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/")
};

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(
    options.DeploymentName,
    options.Endpoint,
    options.ApiKey);
var kernel = kernelBuilder.Build();
kernel.Plugins.AddFromObject(new KycApiPlugin(httpClient), "KycApi");

var kycApi = kernel.Plugins["KycApi"];
var created = await kernel.InvokeAsync(
    kycApi["create_kyc_case"],
    new KernelArguments
    {
        ["fullName"] = "Ada Lovelace",
        ["dateOfBirth"] = "1815-12-10",
        ["country"] = "US",
        ["documentNumber"] = "MOCK-1843"
    });

Console.WriteLine("Created case:");
Console.WriteLine(created);

var createdJson = System.Text.Json.JsonDocument.Parse(created.ToString());
var caseId = createdJson.RootElement.GetProperty("caseId").GetString()
    ?? throw new InvalidOperationException("The API did not return a case ID.");

var validation = await kernel.InvokeAsync(
    kycApi["validate_document"],
    new KernelArguments
    {
        ["caseId"] = caseId,
        ["documentType"] = "mock-passport",
        ["documentNumber"] = "MOCK-1843"
    });
var status = await kernel.InvokeAsync(
    kycApi["get_kyc_status"],
    new KernelArguments { ["caseId"] = caseId });

var summary = await kernel.InvokePromptAsync(
    """
    You are a KYC workflow assistant for a synthetic demo. Summarize these
    mock API results in three concise bullets. State clearly that the result
    is synthetic and not a real identity-verification decision.

    Case creation: {{$created}}
    Document validation: {{$validation}}
    Case status: {{$status}}
    """,
    new KernelArguments
    {
        ["created"] = created.ToString(),
        ["validation"] = validation.ToString(),
        ["status"] = status.ToString()
    });

Console.WriteLine();
Console.WriteLine("Semantic Kernel summary:");
Console.WriteLine(summary);
