using System.Collections.Concurrent;
using ApiTest.Models;

namespace ApiTest.Services;

public sealed class InMemoryKycStore
{
    private readonly ConcurrentDictionary<Guid, KycCase> cases = new();

    public KycCase Add(CreateKycCaseRequest request)
    {
        var kycCase = new KycCase(
            Guid.NewGuid(),
            request.FullName!.Trim(),
            request.DateOfBirth!.Trim(),
            request.Country!.Trim().ToUpperInvariant(),
            request.DocumentNumber!.Trim(),
            DateTimeOffset.UtcNow,
            "pending");

        cases[kycCase.CaseId] = kycCase;
        return kycCase;
    }

    public bool TryGet(Guid caseId, out KycCase? kycCase) =>
        cases.TryGetValue(caseId, out kycCase);
}
