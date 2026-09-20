using WebsiteTest.Contracts;
namespace WebsiteTest.Mocks;
public sealed class MockTravelWebsite : ITravelWebsite { public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default) => Task.FromResult(true); public Task<IReadOnlyList<WebsiteFlight>> SearchAsync(VacationSearch search, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WebsiteFlight>>(new[] { new WebsiteFlight("mock-1", "MockAir", 499.99m, "USD"), new WebsiteFlight("mock-2", "Synthetic Airways", 599.99m, "USD") }); }
