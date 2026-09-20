using Xunit;
public sealed class VacationPlannerApiTests
{
    [Fact] public void Hmac_signatures_are_deterministic() => Assert.Equal(Hmac.Sign("body", "secret"), Hmac.Sign("body", "secret"));
    [Fact] public void Hmac_rejects_tampering() => Assert.False(Hmac.Verify("body", "bad", "secret"));
}
