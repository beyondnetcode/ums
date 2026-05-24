using Ums.Application.Identity.Tenant.Branch.DTOs;

namespace Ums.Application.Identity.Tenant.Branch.Commands;

public sealed class AddBranchCommandHandler : ICommandHandler<AddBranchCommand, AddBranchResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public AddBranchCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<AddBranchResponse>> Handle(
        AddBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<AddBranchResponse>.Failure("Authenticated user is required to add a branch.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<AddBranchResponse>.Failure("Tenant was not found.");
        }

        var result = tenant.AddBranch(
            Code.Create(request.Code),
            Name.Create(request.Name),
            ActorId.Create(_userContext.UserId),
            string.IsNullOrWhiteSpace(request.GeofencingMetadata) ? null : Value.Create(request.GeofencingMetadata));

        if (result.IsFailure)
        {
            return Result<AddBranchResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<AddBranchResponse>.Success(new AddBranchResponse(request.TenantId));
    }
}
