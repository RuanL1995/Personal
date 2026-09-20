using Xunit;
using SemanticTest.Contracts;
namespace SemanticTest.Tests;
public class PlannerModelTests { [Fact] public async Task Mock_returns_deterministic_json_shape() { var p=await new MockPlannerModel().CreatePlanAsync("anything"); Assert.Equal("JFK",p.Origin); Assert.NotEmpty(p.Activities); } }
