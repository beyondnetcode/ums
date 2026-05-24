namespace Ums.Application.Identity.UserAccount.Commands;

using Ums.Domain.Identity;

public sealed class RecordAuthenticationAttemptCommandHandler
    : ICommandHandler<RecordAuthenticationAttemptCommand>
{
    private readonly IUserAccountRepository _repository;
    private readonly IUserContext _userContext;

    public RecordAuthenticationAttemptCommandHandler(
        IUserAccountRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(
        RecordAuthenticationAttemptCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (entity is null) return Result.Failure("User account not found.");

        var actor = ActorId.Create(_userContext.UserId);
        var result = entity.RecordAuthenticationAttempt(
            request.Success,
            request.Reason,
            request.IpAddress,
            actor);

        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
