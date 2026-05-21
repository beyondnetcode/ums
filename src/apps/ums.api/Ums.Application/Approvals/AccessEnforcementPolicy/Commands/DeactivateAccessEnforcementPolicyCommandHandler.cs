using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Approvals;

public sealed class DeactivateAccessEnforcementPolicyCommandHandler : ICommandHandler<DeactivateAccessEnforcementPolicyCommand>
{
    private readonly IAccessEnforcementPolicyRepository _repository;
    private readonly IUserContext _userContext;

    public DeactivateAccessEnforcementPolicyCommandHandler(IAccessEnforcementPolicyRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(DeactivateAccessEnforcementPolicyCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.AccessEnforcementPolicyId, cancellationToken);
        if (entity is null) return Result.Failure("Access enforcement policy not found.");

        var result = entity.Deactivate(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
