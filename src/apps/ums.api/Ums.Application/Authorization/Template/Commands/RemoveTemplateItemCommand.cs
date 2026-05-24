namespace Ums.Application.Authorization.Template.Commands;

public sealed record RemoveTemplateItemCommand(Guid TemplateId, Guid ItemId) : ICommand;
