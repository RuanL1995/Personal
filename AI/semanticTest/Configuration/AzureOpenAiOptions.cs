namespace SemanticTest.Configuration;

public sealed record AzureOpenAiOptions(
    string Endpoint,
    string DeploymentName,
    string ApiKey)
{
    public static AzureOpenAiOptions FromEnvironment()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        var missing = new[]
        {
            string.IsNullOrWhiteSpace(endpoint) ? "AZURE_OPENAI_ENDPOINT" : null,
            string.IsNullOrWhiteSpace(deploymentName) ? "AZURE_OPENAI_DEPLOYMENT_NAME" : null,
            string.IsNullOrWhiteSpace(apiKey) ? "AZURE_OPENAI_API_KEY" : null
        }.Where(name => name is not null);

        if (missing.Any())
        {
            throw new InvalidOperationException(
                $"Missing required environment variables: {string.Join(", ", missing)}");
        }

        return new AzureOpenAiOptions(endpoint!, deploymentName!, apiKey!);
    }
}
