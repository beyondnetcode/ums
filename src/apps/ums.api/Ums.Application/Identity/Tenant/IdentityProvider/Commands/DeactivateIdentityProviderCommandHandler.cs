using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;

namespace Ums.Application.Identity.Tenant.IdentityProvider.Commands;

public sealed class DeactivateIdentityProviderCommandHandler : ICommandHandler<DeactivateIdentityProviderCommand, DeactivateIdentityProviderResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public DeactivateIdentityProviderCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<DeactivateIdentityProviderResponse>> Handle(
        DeactivateIdentityProviderCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<DeactivateIdentityProviderResponse>.Failure("Authenticated user is required to deactivate an identity provider.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<DeactivateIdentityProviderResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.DeactivateIdentityProvider(
            IdValueObject.Load(request.IdentityProviderId),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<DeactivateIdentityProviderResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<DeactivateIdentityProviderResponse>.Success(new DeactivateIdentityProviderResponse(request.TenantId));
    }
}
