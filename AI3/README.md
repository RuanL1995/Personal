# AI3 Vacation Planner

AI3 is a compileable .NET 9 synthetic vacation planner. It contains a Semantic Kernel console, an API, browser website, xUnit skeletons, and an offline end-to-end harness. No real travel provider is contacted.

```powershell
dotnet build AI3/apiTest/apiTest.csproj
dotnet build AI3/semanticTest/semanticTest.csproj
dotnet build AI3/websiteTest/websiteTest.csproj
dotnet test AI3/semanticTest/semanticTest.csproj
dotnet test AI3/websiteTest/websiteTest.csproj
 dotnet run --project AI3/semanticTest -- --test
```

API environment variables are documented at the top of `apiTest/Program.cs`; Semantic Kernel requires `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_DEPLOYMENT_NAME`, and `AZURE_OPENAI_API_KEY` only outside `--test` mode. Webhook requests use `VACATION_WEBHOOK_SECRET` and HMAC-SHA256. All state is in-memory and all agent work is synthetic.

## Environment variables

Semantic console: AZURE_AI_FOUNDRY_ENDPOINT/AZURE_OPENAI_ENDPOINT, AZURE_AI_FOUNDRY_API_KEY/AZURE_OPENAI_API_KEY, and AZURE_AI_FOUNDRY_CHAT_DEPLOYMENT/AZURE_OPENAI_DEPLOYMENT_NAME; --test is offline. API: VACATION_WEBHOOK_SECRET, optional PORT, VacationPlanner:RetryCount, and VacationPlanner:RequireSimulationApproval. Website: VACATION_API_BASE_URL and optional PORT.

