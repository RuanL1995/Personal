namespace ApiTest.Models;

public sealed record CreateKycCaseRequest(
    string? FullName,
    string? DateOfBirth,
    string? Country,
    string? DocumentNumber);

public sealed record ValidateDocumentRequest(
    string? DocumentType,
    string? DocumentNumber);

public sealed record KycCase(
    Guid CaseId,
    string FullName,
    string DateOfBirth,
    string Country,
    string DocumentNumber,
    DateTimeOffset CreatedAt,
    string Status);

public sealed record KycCaseSummary(
    Guid CaseId,
    string Status,
    string RiskLevel,
    IReadOnlyList<string> RiskFlags,
    DateTimeOffset CreatedAt);

public sealed record DocumentValidationResult(
    Guid CaseId,
    bool IsValid,
    string Reason,
    IReadOnlyList<string> RiskFlags);
