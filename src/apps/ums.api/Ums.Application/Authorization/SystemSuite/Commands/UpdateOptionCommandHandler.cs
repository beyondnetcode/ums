namespace Ums.Application.Authorization.SystemSuite.Commands;

using Ums.Domain.Authorization;

public sealed class UpdateOptionCommandHandler : ICommandHandler<UpdateOptionCommand>
{
    private readonly ISystemSuiteRepository _repository;
    private readonly IUserContext _userContext;

    public UpdateOptionCommandHandler(ISystemSuiteRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateOptionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var suite = await _repository.GetByIdAsync(request.SystemSuiteId, cancellationToken);
        if (suite is null) return Result.Failure("System suite not found.");

        var result = suite.UpdateOption(
            IdValueObject.Load(request.ModuleId),
            IdValueObject.Load(request.MenuId),
            IdValueObject.Load(request.SubMenuId),
            IdValueObject.Load(request.OptionId),
            Name.Create(request.Label),
            Description.Create(request.Description),
            ActionCode.Create(request.ActionCode),
            request.SortOrder,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return result;

        await _repository.UpdateAsync(suite, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
