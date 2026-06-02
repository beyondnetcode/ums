using Ums.Application.Identity.Tenant.DTOs;
using Ums.Domain.Identity.Tenant;

namespace Ums.Application.Identity.Tenant.Queries;

public sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<TenantDto>> Handle(
        GetTenantByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result<TenantDto>.Failure("Tenant not found.");
        }

        return Result<TenantDto>.Success(new TenantDto(
            tenant.Props.Id.GetValue(),
            tenant.Props.Code.GetValue(),
            tenant.Props.Name.GetValue(),
            tenant.Props.Type.ToString(),
            tenant.Props.Status.ToString(),
            tenant.Props.ParentTenantId?.GetValue(),
            tenant.Props.CompanyReference?.GetValue(),
            tenant.Props.IsManagementOwner));
    }
}
