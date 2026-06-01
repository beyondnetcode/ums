using Ums.Application.Identity.Tenant.SignupRequests.DTOs;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Identity.TenantSignupRequest;

namespace Ums.Application.Identity.Tenant.SignupRequests.Queries;

public sealed class GetPendingTenantSignupRequestsQueryHandler : IQueryHandler<GetPendingTenantSignupRequestsQuery, IReadOnlyList<TenantSignupRequestDto>>
{
    private readonly ITenantSignupRequestRepository _requestRepository;
    private readonly ITenantContext _tenantContext;

    public GetPendingTenantSignupRequestsQueryHandler(ITenantSignupRequestRepository requestRepository, ITenantContext tenantContext)
    {
        _requestRepository = requestRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<TenantSignupRequestDto>>> Handle(GetPendingTenantSignupRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsInternalAdmin)
        {
            return Result<IReadOnlyList<TenantSignupRequestDto>>.Failure("Internal admin access is required.");
        }

        var requests = await _requestRepository.GetPendingAsync(cancellationToken);
        var dto = requests.Select(x => new TenantSignupRequestDto(
            x.GetId().GetValue(),
            x.CompanyName.GetValue(),
            x.CompanyReference.GetValue(),
            x.ContactName.GetValue(),
            x.ContactEmail.GetValue(),
            x.Status.ToString(),
            x.ApprovedTenantId?.GetValue(),
            x.Props.Audit.GetValue().CreatedAt)).ToList();

        return Result<IReadOnlyList<TenantSignupRequestDto>>.Success(dto);
    }
}
