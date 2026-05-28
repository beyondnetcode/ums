namespace Ums.Domain.Test.Kernel.ValueObjects;

using Xunit;

public class LoginTextTests
{
    [Fact]
    public void Create_WithValidText_ReturnsLoginText()
    {
        var result = LoginText.Create("Welcome to our portal");

        Assert.Equal("Welcome to our portal", result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_TrimsValue()
    {
        var result = LoginText.Create("  trimmed text  ");

        Assert.Equal("trimmed text", result.GetValue());
    }

    [Fact]
    public void Create_WithNull_ReturnsEmptyLoginText()
    {
        var result = LoginText.Create(null);

        Assert.Equal(string.Empty, result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithEmptyString_ReturnsValidEmptyLoginText()
    {
        var result = LoginText.Create("");

        Assert.Equal(string.Empty, result.GetValue());
        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithTextExactly200Chars_ReturnsValid()
    {
        var text = new string('A', 200);
        var result = LoginText.Create(text);

        Assert.Empty(result.BrokenRules.GetBrokenRules());
    }

    [Fact]
    public void Create_WithTextOver200Chars_HasBrokenRuleForTooLong()
    {
        var text = new string('A', 201);
        var result = LoginText.Create(text);

        var brokenRules = result.BrokenRules.GetBrokenRules();
        Assert.NotEmpty(brokenRules);
        Assert.Contains(DomainErrors.Branding.LoginTextTooLong, brokenRules.First().Message);
    }

    [Fact]
    public void Default_ReturnsEmptyLoginText()
    {
        var result = LoginText.Default();

        Assert.Equal(string.Empty, result.GetValue());
    }
}
