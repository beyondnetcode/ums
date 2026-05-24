using Ums.Application.Identity.Tenant.Branch.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;



namespace Ums.Application.Identity.Tenant.Branch.Commands;

using Ums.Application.Common.Interfaces;

public sealed class RemoveBranchCommandHandler : ICommandHandler<RemoveBranchCommand, RemoveBranchResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public RemoveBranchCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<RemoveBranchResponse>> Handle(
        RemoveBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<RemoveBranchResponse>.Failure("Authenticated user is required to remove a branch.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<RemoveBranchResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.RemoveBranch(
            IdValueObject.Load(request.BranchId),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<RemoveBranchResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<RemoveBranchResponse>.Success(new RemoveBranchResponse(request.TenantId));
    }
}
