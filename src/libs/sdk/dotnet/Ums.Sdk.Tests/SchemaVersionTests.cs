using FluentAssertions;
using Ums.Sdk.Contracts;
using Xunit;

namespace Ums.Sdk.Tests;

public sealed class SchemaVersionTests
{
    [Theory]
    [InlineData("1.0.0", true)]
    [InlineData("1.0.5", true)]
    [InlineData("1.99.0", true)]
    [InlineData("2.0.0", false)]
    [InlineData("2.5.3", false)]
    [InlineData("0.9.0", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("not-a-version", false)]
    public void IsSupported_ReturnsExpected(string? input, bool expected)
    {
        SchemaVersion.IsSupported(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.1.0", true)]
    [InlineData("1.0.0", false)]
    [InlineData("0.9.0", false)]
    public void IsMinorAhead_DetectsServerNewer(string version, bool expected)
    {
        SchemaVersion.IsMinorAhead(version).Should().Be(expected);
    }

    [Fact]
    public void Current_IsConsistentWithCompatibilityFloor()
    {
        SchemaVersion.IsSupported(SchemaVersion.Current).Should().BeTrue();
        SchemaVersion.IsSupported(SchemaVersion.CompatibilityMinInclusive).Should().BeTrue();
        SchemaVersion.IsSupported(SchemaVersion.CompatibilityMaxExclusive).Should().BeFalse();
    }
}
