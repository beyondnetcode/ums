namespace Ums.Application.Authorization.Template.Commands;

public sealed record DeprecatePermissionTemplateCommand(Guid TemplateId) : ICommand;
