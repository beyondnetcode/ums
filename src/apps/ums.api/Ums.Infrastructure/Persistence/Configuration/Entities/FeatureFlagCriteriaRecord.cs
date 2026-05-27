using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Configuration.Entities;

public sealed class FeatureFlagCriteriaRecord
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public string CriteriaType { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public FeatureFlagRecord FeatureFlag { get; set; } = null!;
}
