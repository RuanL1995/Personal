namespace SemanticTest.Contracts;
public sealed record VacationPlan(string Origin, string Destination, string DepartureDate, string ReturnDate, IReadOnlyList<string> Activities);
public interface IPlannerModel { Task<VacationPlan> CreatePlanAsync(string request, CancellationToken cancellationToken = default); }
public sealed class MockPlannerModel : IPlannerModel { public Task<VacationPlan> CreatePlanAsync(string request, CancellationToken cancellationToken = default) => Task.FromResult(new VacationPlan("JFK", "LHR", "2030-06-01", "2030-06-08", new[] { "museum", "walking tour" })); }
