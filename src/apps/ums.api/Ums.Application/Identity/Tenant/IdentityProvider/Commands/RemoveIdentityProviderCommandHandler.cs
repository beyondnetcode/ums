using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;



namespace Ums.Application.Identity.Tenant.IdentityProvider.Commands;

using Ums.Application.Common.Interfaces;

public sealed class RemoveIdentityProviderCommandHandler : ICommandHandler<RemoveIdentityProviderCommand, RemoveIdentityProviderResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public RemoveIdentityProviderCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<RemoveIdentityProviderResponse>> Handle(
        RemoveIdentityProviderCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<RemoveIdentityProviderResponse>.Failure("Authenticated user is required to remove an identity provider.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<RemoveIdentityProviderResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.RemoveIdentityProvider(
            IdValueObject.Load(request.IdentityProviderId),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<RemoveIdentityProviderResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<RemoveIdentityProviderResponse>.Success(new RemoveIdentityProviderResponse(request.TenantId));
    }
}
