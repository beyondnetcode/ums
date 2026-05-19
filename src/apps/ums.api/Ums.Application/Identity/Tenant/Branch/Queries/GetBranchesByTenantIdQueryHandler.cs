using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.Tenant.Branch.DTOs;
using Ums.Domain.Identity.Tenant;

namespace Ums.Application.Identity.Tenant.Branch.Queries;

public sealed class GetBranchesByTenantIdQueryHandler : IQueryHandler<GetBranchesByTenantIdQuery, IReadOnlyList<BranchDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetBranchesByTenantIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<IReadOnlyList<BranchDto>>> Handle(
        GetBranchesByTenantIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result<IReadOnlyList<BranchDto>>.Failure("Tenant not found.");
        }

        var branches = tenant.Branches
            .Select(b => new BranchDto(
                b.GetId().GetValue(),
                b.Code.GetValue(),
                b.Name.GetValue(),
                b.IsActive,
                b.GeofencingMetadata?.GetValue()))
            .ToList();

        return Result<IReadOnlyList<BranchDto>>.Success(branches.AsReadOnly());
    }
}
