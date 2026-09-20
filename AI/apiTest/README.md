# apiTest

`apiTest` is a deterministic, in-memory mock KYC API for local Semantic Kernel
experiments. It does not persist data or perform real identity verification.

## Run

```powershell
dotnet run --project AI/apiTest
```

The API listens on `http://localhost:5080`.

## Endpoints

- `POST /api/kyc/cases` with `fullName`, `dateOfBirth`, `country`, and
  `documentNumber`.
- `GET /api/kyc/cases/{caseId}` for status and synthetic risk flags.
- `POST /api/kyc/cases/{caseId}/documents/validate` with `documentType` and
  `documentNumber`.
