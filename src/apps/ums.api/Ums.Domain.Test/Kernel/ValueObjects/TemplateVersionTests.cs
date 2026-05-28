namespace Ums.Domain.Test.Kernel.ValueObjects;

using Xunit;

public class TemplateVersionTests
{
    [Fact]
    public void Create_WithValidSemVer_ReturnsTemplateVersion()
    {
        var result = TemplateVersion.Create(1, 2, 3);

        Assert.Equal("1.2.3", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithZeroVersion_ReturnsValid()
    {
        var result = TemplateVersion.Create(0, 0, 0);

        Assert.Equal("0.0.0", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithLargeNumbers_ReturnsValid()
    {
        var result = TemplateVersion.Create(999, 999, 999);

        Assert.Equal("999.999.999", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Initial_ReturnsVersion010()
    {
        var result = TemplateVersion.Initial();

        Assert.Equal("0.1.0", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }
}
