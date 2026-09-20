# Local vacation planner

This solution contains three independent .NET 9 projects:

| Project | Role | Local URL |
| --- | --- | --- |
| `APITest` | Mock flights, hotels, and plan storage API | `http://localhost:5180` |
| `WebsiteTest` | Bootstrap website and plan display API | `http://localhost:5190` |
| `semanticTest` | Console Semantic Kernel planner and orchestration client | n/a |

## Run locally

Open three PowerShell windows from `testAI\projTest`:

```powershell
dotnet run --project APITest
dotnet run --project WebsiteTest
dotnet run --project semanticTest -- --offline
```

Open `http://localhost:5190` before or after submitting the prompt. The offline mode uses deterministic requirements and is useful for checking the complete local flow.

## Azure AI Foundry

The project is preconfigured for the authenticated Azure subscription's
`aiwork-foundry-swe-test` resource and `gpt-5-mini-test` deployment. Online mode
uses the Azure API key from `AZURE_AI_FOUNDRY_API_KEY` (or the standard
`AZURE_OPENAI_API_KEY`) and never silently falls back to an unauthorized
identity:

```powershell
dotnet run --project semanticTest
```

You can override the resource with `AZURE_AI_FOUNDRY_ENDPOINT` and
`AZURE_AI_FOUNDRY_DEPLOYMENT`. The Semantic Kernel Azure OpenAI connector
converts the console prompt into structured vacation requirements. The console
then calls the local mock APIs, selects a flight and hotel, and posts the
resulting plan to `WebsiteTest`. The browser polls for new plans every five
seconds. No real travel provider or booking is contacted.
