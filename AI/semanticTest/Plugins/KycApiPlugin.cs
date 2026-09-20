using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.SemanticKernel;
using SemanticTest.Models;

namespace SemanticTest.Plugins;

public sealed class KycApiPlugin(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [KernelFunction("create_kyc_case")]
    [Description("Creates a synthetic KYC case in the local mock API.")]
    public async Task<string> CreateCaseAsync(
        [Description("The person's full legal name")] string fullName,
        [Description("Date of birth in YYYY-MM-DD format")] string dateOfBirth,
        [Description("Two-letter country code")] string country,
        [Description("Synthetic document number")] string documentNumber,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/kyc/cases",
            new CreateKycCaseRequest(fullName, dateOfBirth, country, documentNumber),
            JsonOptions,
            cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    [KernelFunction("get_kyc_status")]
    [Description("Gets the current synthetic KYC status and risk summary.")]
    public async Task<string> GetStatusAsync(
        [Description("The KYC case ID")] string caseId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/kyc/cases/{Uri.EscapeDataString(caseId)}",
            cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    [KernelFunction("validate_document")]
    [Description("Validates a synthetic document against a local KYC case.")]
    public async Task<string> ValidateDocumentAsync(
        [Description("The KYC case ID")] string caseId,
        [Description("The document type")] string documentType,
        [Description("The document number")] string documentNumber,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/kyc/cases/{Uri.EscapeDataString(caseId)}/documents/validate",
            new ValidateDocumentRequest(documentType, documentNumber),
            JsonOptions,
            cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    private static async Task<string> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"KYC API returned {(int)response.StatusCode}: {body}");
        }

        return body;
    }
}
