using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserManagementDelegation.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;
using Ums.Domain.Identity.UserManagementDelegation;

namespace Ums.Application.Identity.UserManagementDelegation.Commands;

public sealed class CreateDelegationCommandHandler : ICommandHandler<CreateDelegationCommand, CreateDelegationResponse>
{
    private readonly IUserManagementDelegationRepository _repository;
    private readonly IUserContext _userContext;

    public CreateDelegationCommandHandler(
        IUserManagementDelegationRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<CreateDelegationResponse>> Handle(
        CreateDelegationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateDelegationResponse>.Failure("Authenticated user is required to create a delegation.");
        }

        var scopeType = DomainEnumerationParser.FromName<DelegationScopeType>(request.ScopeType);
        if (scopeType is null)
        {
            return Result<CreateDelegationResponse>.Failure($"Invalid scope type: {request.ScopeType}.");
        }

        var allowedActions = request.AllowedActions
            .Select(a => DomainEnumerationParser.FromName<DelegatedAction>(a))
            .Where(a => a is not null)
            .Select(a => a!)
            .ToList();

        if (allowedActions.Count == 0)
        {
            return Result<CreateDelegationResponse>.Failure("At least one valid allowed action is required.");
        }

        var delegationResult = Ums.Domain.Identity.UserManagementDelegation.UserManagementDelegation.Create(
            TenantId.Load(request.TenantId),
            UserAccountId.Load(request.DelegatingAdminId),
            UserAccountId.Load(request.DelegatedAdminId),
            scopeType,
            request.ScopeId,
            allowedActions,
            request.ValidFrom,
            request.ValidUntil,
            request.MaxDurationDays,
            request.RequiresApproval,
            ActorId.Create(_userContext.UserId));

        if (delegationResult.IsFailure)
        {
            return Result<CreateDelegationResponse>.Failure(delegationResult.Error);
        }

        await _repository.AddAsync(delegationResult.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateDelegationResponse>.Success(
            new CreateDelegationResponse(delegationResult.Value.GetId().GetValue()));
    }
}
