namespace VacationPlanner.Api.Contracts;
public sealed record PlanRequest(string Request, bool RequireApproval = true);
public interface IAgentService
{
    RunStatus Start(PlanRequest request, CancellationToken cancellationToken = default);
    Task ExecuteAsync(Guid runId, PlanRequest request, CancellationToken cancellationToken = default);
}
public sealed record RunStatus(Guid RunId, string Status, DateTimeOffset CreatedAt, string? Message = null);
public interface IPlannerStore
{
    bool TryGetRun(Guid runId, out RunStatus status);
    void SetRun(RunStatus status);
    bool TryIdempotent(string key, out object? value);
    void SaveIdempotent(string key, object value);
}
public interface IAuditStore { void Record(AuditRecord record); IReadOnlyCollection<AuditRecord> Records { get; } }
public interface IToolExecutor { Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken = default); }
public sealed record AuditRecord(string EventType, string Subject, string? CorrelationId, DateTimeOffset Timestamp, object? Data = null);
public sealed record ToolRequest(string Name, object? Arguments = null);
public sealed record ToolResult(bool Success, object? Value = null, string? Error = null);
