namespace Ums.Domain.Test.Kernel.ValueObjects;

using Xunit;

public class HexColorTests
{
    [Fact]
    public void Create_WithValid6DigitHex_ReturnsHexColor()
    {
        var result = HexColor.Create("#FF5733");

        Assert.Equal("#FF5733", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithValid8DigitHex_ReturnsHexColor()
    {
        var result = HexColor.Create("#FF5733FF");

        Assert.Equal("#FF5733FF", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithLowercaseHex_ReturnsHexColor()
    {
        var result = HexColor.Create("#ff5733");

        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_TrimsValue()
    {
        var result = HexColor.Create("  #FF5733  ");

        Assert.Equal("#FF5733", result.GetValue());
    }

    [Fact]
    public void Create_WithNull_ReturnsEmptyColor()
    {
        var result = HexColor.Create(null);

        Assert.Equal(string.Empty, result.GetValue());
    }

    [Fact]
    public void Create_WithEmptyString_HasBrokenRuleForRequired()
    {
        var result = HexColor.Create("");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
        Assert.Contains(DomainErrors.ValueObject.PropertyRequired, brokenRules.First().Message);
    }

    [Fact]
    public void Create_WithWhitespaceOnly_HasBrokenRuleForRequired()
    {
        var result = HexColor.Create("   ");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
    }

    [Fact]
    public void Create_WithInvalidHex_HasBrokenRuleForInvalidFormat()
    {
        var result = HexColor.Create("INVALID");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
        Assert.Contains(DomainErrors.Branding.InvalidHexColor, brokenRules.First().Message);
    }

    [Fact]
    public void Create_WithMissingHash_HasBrokenRuleForInvalidFormat()
    {
        var result = HexColor.Create("FF5733");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
    }

    [Fact]
    public void Create_WithWrongLength_HasBrokenRuleForInvalidFormat()
    {
        var result = HexColor.Create("#FFF");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
    }
}
