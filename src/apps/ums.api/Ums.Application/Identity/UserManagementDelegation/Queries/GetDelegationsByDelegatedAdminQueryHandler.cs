using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed class GetDelegationsByDelegatedAdminQueryHandler : IQueryHandler<GetDelegationsByDelegatedAdminQuery, IReadOnlyList<DelegationDto>>
{
    private readonly IUserManagementDelegationRepository _repository;

    public GetDelegationsByDelegatedAdminQueryHandler(IUserManagementDelegationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<DelegationDto>>> Handle(
        GetDelegationsByDelegatedAdminQuery request,
        CancellationToken cancellationToken)
    {
        var delegations = await _repository.GetByDelegatedAdminAsync(
            request.DelegatedAdminId,
            request.TenantId,
            cancellationToken);

        var dtos = delegations.Select(GetDelegationByIdQueryHandler.ToDto).ToList();
        return Result<IReadOnlyList<DelegationDto>>.Success(dtos);
    }
}
