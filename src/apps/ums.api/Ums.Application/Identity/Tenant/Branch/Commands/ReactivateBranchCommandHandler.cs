using Ums.Application.Identity.Tenant.Branch.DTOs;



namespace Ums.Application.Identity.Tenant.Branch.Commands;

using Ums.Application.Common.Interfaces;

public sealed class ReactivateBranchCommandHandler : ICommandHandler<ReactivateBranchCommand, ReactivateBranchResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public ReactivateBranchCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<ReactivateBranchResponse>> Handle(
        ReactivateBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<ReactivateBranchResponse>.Failure("Authenticated user is required to reactivate a branch.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<ReactivateBranchResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.ReactivateBranch(
            IdValueObject.Load(request.BranchId),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<ReactivateBranchResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<ReactivateBranchResponse>.Success(new ReactivateBranchResponse(request.TenantId));
    }
}
