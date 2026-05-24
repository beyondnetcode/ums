using Ums.Application.IGA.PromotionRequest.DTOs;

namespace Ums.Application.IGA.PromotionRequest.Commands;

using Ums.Domain.IGA;

public sealed class SubmitPromotionRequestCommandHandler : ICommandHandler<SubmitPromotionRequestCommand>
{
    private readonly IPromotionRequestRepository _repository;
    private readonly IUserContext _userContext;

    public SubmitPromotionRequestCommandHandler(IPromotionRequestRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(SubmitPromotionRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.PromotionRequestId, cancellationToken);
        if (entity is null) return Result.Failure("Promotion request not found.");

        var result = entity.Submit(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
