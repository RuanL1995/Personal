# Vacation planner local verification checklist

- [x] No real endpoint or secret is stored in source; use environment variables only.
- [x] Start API: `dotnet run --project AI3/apiTest` (listens at `http://localhost:5180`).
- [x] Run semantic mock: `dotnet run --project AI3/semanticTest` (deterministic JSON output).
- [x] Run website mock: `dotnet run --project AI3/websiteTest` (listens at `http://localhost:5190`).
- [x] Exercise search and booking with an `Idempotency-Key`; repeat requests and confirm the same result.
- [x] Sign webhook payload with `VACATION_WEBHOOK_SECRET` and `HmacSHA256`; invalid signatures return 401.
- [x] Pass `X-Correlation-ID` and confirm it is returned on every response.
- [x] Run `dotnet test AI3/semanticTest` and `dotnet test AI3/websiteTest`.
- [x] Open the AI3 browser-hosted website at `http://localhost:5190` after starting
  both `apiTest` and `websiteTest`.

All integrations are synthetic/local and must not be pointed at production systems.
