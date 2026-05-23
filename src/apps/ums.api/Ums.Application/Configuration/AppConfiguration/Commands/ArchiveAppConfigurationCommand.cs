namespace Ums.Application.Configuration.AppConfiguration.Commands;

public sealed record ArchiveAppConfigurationCommand(Guid AppConfigurationId) : ICommand;
