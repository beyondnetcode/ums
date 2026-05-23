namespace Ums.Application.Configuration.FeatureFlag.Commands;

public sealed record ArchiveFeatureFlagCommand(Guid FeatureFlagId) : ICommand;
