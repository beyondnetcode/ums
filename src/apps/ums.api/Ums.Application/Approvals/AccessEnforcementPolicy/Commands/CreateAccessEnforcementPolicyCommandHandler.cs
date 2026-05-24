using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;

using Ums.Domain;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Ums.Domain.Enums;

public sealed class CreateAccessEnforcementPolicyCommandHandler : ICommandHandler<CreateAccessEnforcementPolicyCommand, CreateAccessEnforcementPolicyResponse>
{
    private readonly IAccessEnforcementPolicyRepository _repository;
    private readonly IUserContext _userContext;

    public CreateAccessEnforcementPolicyCommandHandler(IAccessEnforcementPolicyRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateAccessEnforcementPolicyResponse>> Handle(CreateAccessEnforcementPolicyCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateAccessEnforcementPolicyResponse>.Failure("Authenticated user is required.");

        var action = DomainEnumerationParser.FromName<AccessEnforcementAction>(request.EnforcementAction)!;

        var result = AccessEnforcementPolicy.Create(
            TenantId.Load(request.TenantId),
            request.ProfileId.HasValue ? ProfileId.Load(request.ProfileId.Value) : null,
            request.RoleId.HasValue ? RoleId.Load(request.RoleId.Value) : null,
            action,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreateAccessEnforcementPolicyResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateAccessEnforcementPolicyResponse>.Success(new CreateAccessEnforcementPolicyResponse(result.Value.Props.Id.GetValue()));
    }
}
