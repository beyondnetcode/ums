namespace Ums.Application.Authorization.Template.Commands;

using Ums.Domain.Authorization;
using Ums.Domain.Enums;
using BeyondNetCode.Shell.Ddd;

public sealed class AddTemplateItemCommandHandler : ICommandHandler<AddTemplateItemCommand>
{
    private readonly IPermissionTemplateRepository _repository;
    private readonly IUserContext _userContext;

    public AddTemplateItemCommandHandler(
        IPermissionTemplateRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(
        AddTemplateItemCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var template = await _repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null) return Result.Failure("Template not found.");

        var targetType = DomainEnumeration.FromDisplayName<ExclusiveArcTarget>(request.TargetType);
        if (targetType is null)
            return Result.Failure($"Invalid target type '{request.TargetType}'. Valid: SystemSuite, Module, Submodule, Option.");

        var result = template.AddItem(
            targetType,
            IdValueObject.Load(request.TargetId),
            ActionId.Load(request.ActionId),
            request.IsAllowed,
            request.IsDenied,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return result;

        await _repository.UpdateAsync(template, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
