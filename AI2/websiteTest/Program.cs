using WebsiteTest.Contracts; using WebsiteTest.Mocks;
var website = new MockTravelWebsite();
var results = await website.SearchAsync(new VacationSearch("JFK", "LHR", new DateOnly(2030,6,1), new DateOnly(2030,6,8)));
Console.WriteLine($"Mock website healthy: {await website.IsHealthyAsync()}; flights: {results.Count}");
