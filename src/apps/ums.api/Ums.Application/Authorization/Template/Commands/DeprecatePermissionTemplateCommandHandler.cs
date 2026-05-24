namespace Ums.Application.Authorization.Template.Commands;

using Ums.Domain.Authorization;

public sealed class DeprecatePermissionTemplateCommandHandler
    : ICommandHandler<DeprecatePermissionTemplateCommand>
{
    private readonly IPermissionTemplateRepository _repository;
    private readonly IUserContext _userContext;

    public DeprecatePermissionTemplateCommandHandler(
        IPermissionTemplateRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(
        DeprecatePermissionTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var template = await _repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null) return Result.Failure("Template not found.");

        var result = template.Deprecate(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(template, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
