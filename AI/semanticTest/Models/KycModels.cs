namespace SemanticTest.Models;

public sealed record CreateKycCaseRequest(
    string FullName,
    string DateOfBirth,
    string Country,
    string DocumentNumber);

public sealed record ValidateDocumentRequest(
    string DocumentType,
    string DocumentNumber);
