using Crip.AspNetCore.Correlation.Features;

namespace Crip.AspNetCore.Correlation.Tests.Features;

public class CorrelationFeatureShould
{
    [Fact, Trait("Category", "Unit")]
    public void Constructor_SetsIdValue()
    {
        var feature = new CorrelationFeature("id");

        feature.Id.Should().Be("id");
    }
}