namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

using Ums.Domain.IGA;

public sealed class RecordCertificationCompletionCommandHandler
    : ICommandHandler<RecordCertificationCompletionCommand>
{
    private readonly IRoleMaturityStatusRepository _repository;
    private readonly IUserContext _userContext;

    public RecordCertificationCompletionCommandHandler(
        IRoleMaturityStatusRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(
        RecordCertificationCompletionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.RoleMaturityStatusId, cancellationToken);
        if (entity is null) return Result.Failure("Role maturity status not found.");

        var result = entity.RecordCertificationCompletion(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
