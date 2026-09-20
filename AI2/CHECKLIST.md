# Vacation planner local verification checklist

- [x] No real endpoint or secret is stored in source; use environment variables only.
- [x] Start API: `dotnet run --project AI2/apiTest` (listens on the configured local URL).
- [x] Run semantic mock: `dotnet run --project AI2/semanticTest` (deterministic JSON output).
- [x] Run website mock: `dotnet run --project AI2/websiteTest`.
- [x] Exercise search and booking with an `Idempotency-Key`; repeat requests and confirm the same result.
- [x] Sign webhook payload with `VACATION_WEBHOOK_SECRET` and `HmacSHA256`; invalid signatures return 401.
- [x] Pass `X-Correlation-ID` and confirm it is returned on every response.
- [x] Run `dotnet test AI2/semanticTest` and `dotnet test AI2/websiteTest`.
- [x] Open the browser-hosted website at `http://localhost:5090` after starting
  both `apiTest` and `websiteTest`.

All integrations are synthetic/local and must not be pointed at production systems.
