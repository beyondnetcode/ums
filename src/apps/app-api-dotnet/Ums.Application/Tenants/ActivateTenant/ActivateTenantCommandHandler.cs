namespace Ums.Application.Tenants.ActivateTenant;

using Ums.Application.Abstractions.Messaging;
using Ums.Application.Abstractions.Persistence;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;

public sealed class ActivateTenantCommandHandler : ICommandHandler<ActivateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public ActivateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to activate a tenant.");
        }

        var tenant = await _tenantRepository.FindByIdAsync(TenantId.Load(request.TenantId), cancellationToken);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
