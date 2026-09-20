namespace WebsiteTest.Contracts;
public sealed record VacationSearch(string Origin, string Destination, DateOnly DepartureDate, DateOnly? ReturnDate);
public interface ITravelWebsite { Task<IReadOnlyList<WebsiteFlight>> SearchAsync(VacationSearch search, CancellationToken cancellationToken = default); Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default); }
public sealed record WebsiteFlight(string Id, string Carrier, decimal Price, string Currency);
