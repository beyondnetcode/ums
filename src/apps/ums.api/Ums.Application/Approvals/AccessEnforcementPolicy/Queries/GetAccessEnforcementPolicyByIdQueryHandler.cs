using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.AccessEnforcementPolicy;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Queries;

public sealed class GetAccessEnforcementPolicyByIdQueryHandler : IQueryHandler<GetAccessEnforcementPolicyByIdQuery, AccessEnforcementPolicyDto>
{
    private readonly IAccessEnforcementPolicyRepository _repository;

    public GetAccessEnforcementPolicyByIdQueryHandler(IAccessEnforcementPolicyRepository repository) => _repository = repository;

    public async Task<Result<AccessEnforcementPolicyDto>> Handle(GetAccessEnforcementPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.AccessEnforcementPolicyId, cancellationToken);
        if (entity is null) return Result<AccessEnforcementPolicyDto>.Failure("Access enforcement policy not found.");

        return Result<AccessEnforcementPolicyDto>.Success(new AccessEnforcementPolicyDto(
            entity.Props.Id.GetValue(), entity.Props.TenantId.GetValue(), entity.Props.ProfileId?.GetValue(),
            entity.Props.RoleId?.GetValue(), entity.Props.EnforcementAction.ToString(), entity.Props.IsActive));
    }
}
