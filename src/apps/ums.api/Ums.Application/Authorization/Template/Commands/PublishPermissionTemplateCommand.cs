using Ums.Application.Authorization.Template.DTOs;

namespace Ums.Application.Authorization.Template.Commands;

public sealed record PublishPermissionTemplateCommand(Guid TemplateId) : ICommand;
