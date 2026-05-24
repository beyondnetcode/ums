using Ums.Application.Approvals.UserDocument.DTOs;

namespace Ums.Application.Approvals.UserDocument.Commands;

using Ums.Domain.Approvals;

public sealed class ValidateUserDocumentCommandHandler : ICommandHandler<ValidateUserDocumentCommand>
{
    private readonly IUserDocumentRepository _repository;
    private readonly IUserContext _userContext;

    public ValidateUserDocumentCommandHandler(IUserDocumentRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ValidateUserDocumentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.UserDocumentId, cancellationToken);
        if (entity is null) return Result.Failure("User document not found.");

        var result = entity.Validate(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
