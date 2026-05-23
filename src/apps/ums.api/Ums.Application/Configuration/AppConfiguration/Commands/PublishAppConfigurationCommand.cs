namespace Ums.Application.Configuration.AppConfiguration.Commands;

public sealed record PublishAppConfigurationCommand(Guid AppConfigurationId) : ICommand;
