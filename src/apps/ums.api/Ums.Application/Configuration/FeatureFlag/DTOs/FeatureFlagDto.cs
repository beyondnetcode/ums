namespace Ums.Application.Configuration.FeatureFlag.DTOs;

public sealed record FeatureFlagDto(
    Guid FeatureFlagId,
    string FlagCode,
    string FlagType,
    string FlagTargets,
    string Status,
    string? LinkedResourceType,
    Guid? LinkedResourceId,
    int? RolloutPercentage);
