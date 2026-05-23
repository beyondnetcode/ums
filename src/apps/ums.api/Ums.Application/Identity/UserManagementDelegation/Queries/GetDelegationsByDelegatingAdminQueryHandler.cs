using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed class GetDelegationsByDelegatingAdminQueryHandler : IQueryHandler<GetDelegationsByDelegatingAdminQuery, IReadOnlyList<DelegationDto>>
{
    private readonly IUserManagementDelegationRepository _repository;

    public GetDelegationsByDelegatingAdminQueryHandler(IUserManagementDelegationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<DelegationDto>>> Handle(
        GetDelegationsByDelegatingAdminQuery request,
        CancellationToken cancellationToken)
    {
        var delegations = await _repository.GetByDelegatingAdminAsync(
            request.DelegatingAdminId,
            request.TenantId,
            cancellationToken);

        var dtos = delegations.Select(GetDelegationByIdQueryHandler.ToDto).ToList();
        return Result<IReadOnlyList<DelegationDto>>.Success(dtos);
    }
}
