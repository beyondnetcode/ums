using Ums.Application.Common.Interfaces;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Identity.UserAccount.Commands;

/// <summary>
/// REC-16: Handles DeleteUserAccountCommand.
///
/// Execution order (important for EF change-tracking correctness):
/// 1. Load aggregate — validates it exists and is not already deleted.
/// 2. aggregate.Delete() — transitions status to Deleted, raises UserDeletedEvent.
/// 3. UpdateAsync — registers the aggregate in _trackedAggregates (needed for outbox) and
///    stages Apply() changes (StatusId=Deleted, audit fields) on the EF-tracked entity.
/// 4. SoftDeleteAsync — using EF identity map it gets the SAME tracked entity and adds
///    the soft-delete + GDPR fields (IsDeleted=true, anonymized email) on top.
/// 5. SaveEntitiesAsync — atomically: creates UserDeletedEvent outbox message, then
///    calls SaveChangesAsync which commits the EF entity changes + outbox row.
/// </summary>
public sealed class DeleteUserAccountCommandHandler : ICommandHandler<DeleteUserAccountCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public DeleteUserAccountCommandHandler(
        IUserAccountRepository userAccountRepository,
        IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(DeleteUserAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to delete a user account.");
        }

        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (userAccount is null)
        {
            return Result.Failure("User account was not found.");
        }

        // Domain validation: transition to Deleted state and raise UserDeletedEvent.
        var result = userAccount.Delete(ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        // Register the aggregate in _trackedAggregates so SaveEntitiesAsync creates
        // the UserDeletedEvent outbox message. Also stages StatusId + audit changes.
        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);

        // Apply soft-delete + GDPR anonymization on top of the already-staged EF entity.
        // Uses EF identity map, so no extra DB round-trip is required.
        var deleted = await _userAccountRepository.SoftDeleteAsync(
            request.UserAccountId, _userContext.UserId, cancellationToken);

        if (!deleted)
        {
            // Race condition: concurrent request already soft-deleted this account.
            return Result.Failure("User account was not found or is already deleted.");
        }

        // Atomically commit: EF entity changes (IsDeleted=true, anonymized email, StatusId)
        // + UserDeletedEvent outbox message in a single SaveChangesAsync call.
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
