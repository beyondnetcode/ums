namespace Ums.Application.Configuration.AppConfiguration.Commands;

public sealed record UpdateAppConfigurationCommand(
    Guid AppConfigurationId,
    string Value,
    string Description) : ICommand;
