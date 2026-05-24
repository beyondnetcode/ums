using Ums.Application.Identity.Tenant.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;


namespace Ums.Application.Identity.Tenant.Commands;

using Ums.Application.Common.Interfaces;

public sealed class ActivateTenantCommandHandler : ICommandHandler<ActivateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public ActivateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to activate a tenant.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure("Tenant was not found.");
        }

        var result = tenant.Activate(ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
