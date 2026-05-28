namespace Ums.Domain.Test.Kernel.ValueObjects;

using Ums.Domain.Kernel.ValueObjects;
using Xunit;

public class VersionValueObjectTests
{
    [Fact]
    public void Create_WithValidVersion_ReturnsVersion()
    {
        var result = Ums.Domain.Kernel.ValueObjects.Version.Create("1.0.0");

        Assert.Equal("1.0.0", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_TrimsValue()
    {
        var result = Ums.Domain.Kernel.ValueObjects.Version.Create("  2.1.0  ");

        Assert.Equal("2.1.0", result.GetValue());
    }

    [Fact]
    public void Create_WithNull_ReturnsEmptyVersion()
    {
        var result = Ums.Domain.Kernel.ValueObjects.Version.Create(null!);

        Assert.Equal(string.Empty, result.GetValue());
    }

    [Fact]
    public void Default_ReturnsEmptyVersion()
    {
        var result = Ums.Domain.Kernel.ValueObjects.Version.Default();

        Assert.Equal(string.Empty, result.GetValue());
    }
}
