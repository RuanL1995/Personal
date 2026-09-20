# AI2 vacation planner

Three standalone .NET 9 projects are provided. Existing `AI/` projects are unchanged.

`websiteTest` is browser-hosted at `http://localhost:5090` and proxies search and
booking requests to the local API at `http://localhost:5080`. Start both apps:

```powershell
dotnet run --project AI2/apiTest
dotnet run --project AI2/websiteTest
Start-Process http://localhost:5090
```

## Run and verify

```powershell
dotnet restore AI2/apiTest/apiTest.csproj
dotnet restore AI2/semanticTest/semanticTest.csproj
dotnet restore AI2/websiteTest/websiteTest.csproj
dotnet build AI2/apiTest/apiTest.csproj --no-restore
dotnet build AI2/semanticTest/semanticTest.csproj --no-restore
dotnet build AI2/websiteTest/websiteTest.csproj --no-restore
dotnet test AI2/semanticTest/semanticTest.csproj --no-build
dotnet test AI2/websiteTest/websiteTest.csproj --no-build
.\AI2\e2e-harness.ps1
```

The API uses only in-memory synthetic data. Azure OpenAI settings are read only
from `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_DEPLOYMENT_NAME`, and
`AZURE_OPENAI_API_KEY`; webhook signing uses `VACATION_WEBHOOK_SECRET`.
No real endpoints or secrets are required for local verification.
