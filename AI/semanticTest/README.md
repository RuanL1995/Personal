# semanticTest

`semanticTest` is a Semantic Kernel console agent that calls the local
`apiTest` mock KYC API and asks the configured Azure OpenAI deployment to
summarize the synthetic results.

## Configuration

Set these environment variables without committing them:

```powershell
$env:AZURE_OPENAI_ENDPOINT="https://<resource>.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME="<deployment-name>"
$env:AZURE_OPENAI_API_KEY="<secret>"
$env:KYC_API_BASE_URL="http://localhost:5080"
```

`KYC_API_BASE_URL` defaults to `http://localhost:5080`.

## Run

Start `apiTest` in another terminal, then run:

```powershell
dotnet run --project AI/semanticTest
```

The Azure endpoint and API key are read at startup and are never stored in
source code. The KYC data in this demo is synthetic.
