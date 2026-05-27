namespace Ums.Application.Authorization.SystemSuite.Commands;

using Ums.Domain.Authorization;

public sealed class AddSubMenuCommandHandler : ICommandHandler<AddSubMenuCommand>
{
    private readonly ISystemSuiteRepository _repository;
    private readonly IUserContext _userContext;

    public AddSubMenuCommandHandler(ISystemSuiteRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(AddSubMenuCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var suite = await _repository.GetByIdAsync(request.SystemSuiteId, cancellationToken);
        if (suite is null) return Result.Failure("System suite not found.");

        var result = suite.AddSubMenu(
            IdValueObject.Load(request.ModuleId),
            IdValueObject.Load(request.MenuId),
            Code.Create(request.Code),
            Name.Create(request.Label),
            Description.Create(request.Description),
            request.SortOrder,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return result;

        await _repository.UpdateAsync(suite, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
