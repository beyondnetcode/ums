namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Shell.Ddd;
public sealed class UpdateAccessEnforcementActionCommandHandler : ICommandHandler<UpdateAccessEnforcementActionCommand>
{
    private readonly IAccessEnforcementPolicyRepository _repository;
    private readonly IUserContext _userContext;
    public UpdateAccessEnforcementActionCommandHandler(IAccessEnforcementPolicyRepository repository, IUserContext userContext) { _repository = repository; _userContext = userContext; }
    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateAccessEnforcementActionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId)) return Result.Failure("Authenticated user is required.");
        var entity = await _repository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (entity is null) return Result.Failure("Access enforcement policy not found.");
        var action = DomainEnumeration.FromDisplayName<AccessEnforcementAction>(request.NewAction);
        if (action is null) return Result.Failure($"Invalid action '{request.NewAction}'. Valid: BlockUser, RestrictProfile, LogOnly.");
        var result = entity.UpdateAction(action, ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
