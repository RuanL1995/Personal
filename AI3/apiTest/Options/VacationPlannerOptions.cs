namespace VacationPlanner.Api.Options;
public sealed class VacationPlannerOptions
{
    public const string SectionName = "VacationPlanner";
    public string WebhookSecretEnvironmentVariable { get; set; } = "VACATION_WEBHOOK_SECRET";
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 100;
    public bool RequireSimulationApproval { get; set; }
}
