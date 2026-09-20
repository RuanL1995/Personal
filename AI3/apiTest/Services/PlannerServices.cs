using VacationPlanner.Api.Contracts;
namespace VacationPlanner.Api.Services;
public sealed class AuditService : IAuditStore
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<AuditRecord> records = new();
    public IReadOnlyCollection<AuditRecord> Records => records.ToArray();
    public void Record(AuditRecord record) => records.Enqueue(record);
}
public sealed class ToolService : IToolExecutor
{
    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken = default) => request.Name switch
    {
        "search_flights" or "create_booking" or "request_approval" => Task.FromResult(new ToolResult(true, new { tool = request.Name, synthetic = true })),
        _ => Task.FromResult(new ToolResult(false, null, $"Unknown tool: {request.Name}"))
    };
}

