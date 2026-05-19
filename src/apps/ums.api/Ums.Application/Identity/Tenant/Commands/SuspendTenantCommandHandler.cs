using Ums.Application.Identity.Tenant.DTOs;


namespace Ums.Application.Identity.Tenant.Commands;

using Ums.Application.Common.Interfaces;

public sealed class SuspendTenantCommandHandler : ICommandHandler<SuspendTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public SuspendTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to suspend a tenant.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure("Tenant was not found.");
        }

        var result = tenant.Suspend(ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
