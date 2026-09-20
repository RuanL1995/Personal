using WebsiteTest.Mocks;
using WebsiteTest.Contracts;
using Xunit;
namespace WebsiteTest.Tests;
public class MockTravelWebsiteTests { [Fact] public async Task Search_returns_synthetic_flights() { var result=await new MockTravelWebsite().SearchAsync(new("JFK","LHR",new(2030,1,1),null)); Xunit.Assert.Equal(2,result.Count); } }
