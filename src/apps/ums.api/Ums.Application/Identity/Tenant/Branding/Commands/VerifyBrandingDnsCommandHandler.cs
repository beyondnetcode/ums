using Ums.Application.Identity.Tenant.Branding.DTOs;

namespace Ums.Application.Identity.Tenant.Branding.Commands;

public sealed class VerifyBrandingDnsCommandHandler : ICommandHandler<VerifyBrandingDnsCommand, VerifyBrandingDnsResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public VerifyBrandingDnsCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<VerifyBrandingDnsResponse>> Handle(
        VerifyBrandingDnsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<VerifyBrandingDnsResponse>.Failure("Authenticated user is required to verify branding DNS.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<VerifyBrandingDnsResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.VerifyBrandingDns(ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<VerifyBrandingDnsResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<VerifyBrandingDnsResponse>.Success(new VerifyBrandingDnsResponse(request.TenantId));
    }
}
