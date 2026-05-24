namespace Ums.Application.Authorization.Template.Commands;

public sealed record AddTemplateItemCommand(
    Guid TemplateId,
    string TargetType,
    Guid TargetId,
    Guid ActionId,
    bool IsAllowed,
    bool IsDenied) : ICommand;
