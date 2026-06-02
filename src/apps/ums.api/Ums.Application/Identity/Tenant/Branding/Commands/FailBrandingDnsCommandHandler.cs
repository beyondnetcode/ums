using Ums.Application.Identity.Tenant.Branding.DTOs;

namespace Ums.Application.Identity.Tenant.Branding.Commands;

public sealed class FailBrandingDnsCommandHandler : ICommandHandler<FailBrandingDnsCommand, FailBrandingDnsResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;
    private readonly ITenantScopePolicy _tenantScopePolicy;

    public FailBrandingDnsCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext,
        ITenantScopePolicy tenantScopePolicy)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
        _tenantScopePolicy = tenantScopePolicy;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<FailBrandingDnsResponse>> Handle(
        FailBrandingDnsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<FailBrandingDnsResponse>.Failure("Authenticated user is required to fail branding DNS.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<FailBrandingDnsResponse>.Failure("Tenant was not found.");
        }

        var scopeResult = await _tenantScopePolicy.EnsureManagementOwnerScopeAsync(request.TenantId, cancellationToken);
        if (scopeResult.IsFailure)
        {
            return Result<FailBrandingDnsResponse>.Failure(scopeResult.Error);
        }

        var result = tenant.FailBrandingDns(ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<FailBrandingDnsResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<FailBrandingDnsResponse>.Success(new FailBrandingDnsResponse(request.TenantId));
    }
}
