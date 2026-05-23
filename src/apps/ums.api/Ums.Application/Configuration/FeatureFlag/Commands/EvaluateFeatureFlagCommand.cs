using Ums.Application.Configuration.FeatureFlag.DTOs;

namespace Ums.Application.Configuration.FeatureFlag.Commands;

public sealed record EvaluateFeatureFlagCommand(
    Guid FeatureFlagId,
    string Context) : ICommand<EvaluateFeatureFlagResponse>;
