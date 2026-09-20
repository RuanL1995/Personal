using ApiTest.Models;
using ApiTest.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InMemoryKycStore>();

var app = builder.Build();

app.MapPost("/api/kyc/cases", (CreateKycCaseRequest? request, InMemoryKycStore store) =>
{
    if (request is null ||
        string.IsNullOrWhiteSpace(request.FullName) ||
        string.IsNullOrWhiteSpace(request.DateOfBirth) ||
        string.IsNullOrWhiteSpace(request.Country) ||
        string.IsNullOrWhiteSpace(request.DocumentNumber))
    {
        return Results.BadRequest(new { error = "fullName, dateOfBirth, country, and documentNumber are required." });
    }

    var kycCase = store.Add(request);
    return Results.Created(
        $"/api/kyc/cases/{kycCase.CaseId}",
        new { caseId = kycCase.CaseId, status = kycCase.Status, createdAt = kycCase.CreatedAt });
});

app.MapGet("/api/kyc/cases/{caseId:guid}", (Guid caseId, InMemoryKycStore store) =>
{
    if (!store.TryGet(caseId, out var kycCase) || kycCase is null)
    {
        return Results.NotFound(new { error = "KYC case was not found." });
    }

    var riskFlags = kycCase.Country == "US"
        ? Array.Empty<string>()
        : new[] { "country-review" };
    var riskLevel = riskFlags.Length == 0 ? "low" : "medium";

    return Results.Ok(new KycCaseSummary(
        kycCase.CaseId,
        kycCase.Status,
        riskLevel,
        riskFlags,
        kycCase.CreatedAt));
});

app.MapPost(
    "/api/kyc/cases/{caseId:guid}/documents/validate",
    (Guid caseId, ValidateDocumentRequest? request, InMemoryKycStore store) =>
    {
        if (!store.TryGet(caseId, out var kycCase) || kycCase is null)
        {
            return Results.NotFound(new { error = "KYC case was not found." });
        }

        if (request is null ||
            string.IsNullOrWhiteSpace(request.DocumentType) ||
            string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            return Results.BadRequest(new { error = "documentType and documentNumber are required." });
        }

        var isValid = string.Equals(
            request.DocumentNumber.Trim(),
            kycCase.DocumentNumber,
            StringComparison.OrdinalIgnoreCase);
        var reason = isValid ? "Document number matches the case." : "Document number does not match the case.";
        var riskFlags = isValid ? Array.Empty<string>() : new[] { "document-mismatch" };

        return Results.Ok(new DocumentValidationResult(caseId, isValid, reason, riskFlags));
    });

app.Run("http://localhost:5080");
