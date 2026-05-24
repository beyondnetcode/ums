namespace Ums.Application.Authorization.Template.Commands;

public sealed record ActivateTemplateItemCommand(Guid TemplateId, Guid ItemId) : ICommand;
