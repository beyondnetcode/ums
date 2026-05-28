namespace Ums.Domain.Test.Kernel.ValueObjects;

using Xunit;

public class DnsCnameTargetTests
{
    [Fact]
    public void Create_ReturnsExpectedTarget()
    {
        var result = DnsCnameTarget.Create();

        Assert.Equal("edge.platform.io", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void GetExpectedTarget_ReturnsCorrectValue()
    {
        var target = DnsCnameTarget.GetExpectedTarget();

        Assert.Equal("edge.platform.io", target);
    }
}
