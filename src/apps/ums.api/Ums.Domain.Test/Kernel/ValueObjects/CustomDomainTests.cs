namespace Ums.Domain.Test.Kernel.ValueObjects;

using Xunit;

public class CustomDomainTests
{
    [Fact]
    public void Create_WithValidDomain_ReturnsCustomDomain()
    {
        var result = CustomDomain.Create("login.example.com");

        Assert.Equal("login.example.com", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_TrimsAndLowercasesValue()
    {
        var result = CustomDomain.Create("  Login.EXAMPLE.COM  ");

        Assert.Equal("login.example.com", result.GetValue());
    }

    [Fact]
    public void Create_WithNull_ReturnsEmptyDomain()
    {
        var result = CustomDomain.Create(null);

        Assert.Equal(string.Empty, result.GetValue());
    }

    [Fact]
    public void Create_WithEmptyString_HasBrokenRuleForRequired()
    {
        var result = CustomDomain.Create("");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
        Assert.Contains(DomainErrors.ValueObject.PropertyRequired, brokenRules.First().Message);
    }

    [Fact]
    public void Create_WithWhitespaceOnly_HasBrokenRuleForRequired()
    {
        var result = CustomDomain.Create("   ");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
    }

    [Fact]
    public void Create_WithInvalidDomain_HasBrokenRuleForInvalidFormat()
    {
        var result = CustomDomain.Create("not-a-domain");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
        Assert.Contains(DomainErrors.Branding.InvalidCustomDomain, brokenRules.First().Message);
    }

    [Fact]
    public void Create_WithDomainMissingTld_HasBrokenRuleForInvalidFormat()
    {
        var result = CustomDomain.Create("login");

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
    }

    [Fact]
    public void Create_WithSubdomain_ReturnsValid()
    {
        var result = CustomDomain.Create("auth.portal.example.com");

        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }
}
