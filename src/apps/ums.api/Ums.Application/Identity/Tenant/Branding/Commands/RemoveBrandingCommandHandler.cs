using Ums.Application.Identity.Tenant.Branding.DTOs;



namespace Ums.Application.Identity.Tenant.Branding.Commands;

using Ums.Application.Common.Interfaces;

public sealed class RemoveBrandingCommandHandler : ICommandHandler<RemoveBrandingCommand, RemoveBrandingResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public RemoveBrandingCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<RemoveBrandingResponse>> Handle(
        RemoveBrandingCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<RemoveBrandingResponse>.Failure("Authenticated user is required to remove branding.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<RemoveBrandingResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.RemoveBranding(ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<RemoveBrandingResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<RemoveBrandingResponse>.Success(new RemoveBrandingResponse(request.TenantId));
    }
}
