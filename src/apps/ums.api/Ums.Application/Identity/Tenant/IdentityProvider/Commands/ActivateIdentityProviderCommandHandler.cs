using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;

namespace Ums.Application.Identity.Tenant.IdentityProvider.Commands;

public sealed class ActivateIdentityProviderCommandHandler : ICommandHandler<ActivateIdentityProviderCommand, ActivateIdentityProviderResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public ActivateIdentityProviderCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<ActivateIdentityProviderResponse>> Handle(
        ActivateIdentityProviderCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<ActivateIdentityProviderResponse>.Failure("Authenticated user is required to activate an identity provider.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<ActivateIdentityProviderResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.ActivateIdentityProvider(
            IdValueObject.Load(request.IdentityProviderId),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<ActivateIdentityProviderResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<ActivateIdentityProviderResponse>.Success(new ActivateIdentityProviderResponse(request.TenantId));
    }
}
