using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed class GetAllDelegationsQueryHandler : IQueryHandler<GetAllDelegationsQuery, IReadOnlyList<DelegationDto>>
{
    private readonly IUserManagementDelegationRepository _repository;

    public GetAllDelegationsQueryHandler(IUserManagementDelegationRepository repository)
    {
        _repository = repository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<IReadOnlyList<DelegationDto>>> Handle(
        GetAllDelegationsQuery request,
        CancellationToken cancellationToken)
    {
        var delegations = await _repository.GetAllAsync(cancellationToken);
        var dtos = delegations.Select(GetDelegationByIdQueryHandler.ToDto).ToList();
        return Result<IReadOnlyList<DelegationDto>>.Success(dtos);
    }
}
