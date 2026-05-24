using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using Ums.Domain.Approvals;

public sealed class ApproveRequestCommandHandler : ICommandHandler<ApproveRequestCommand>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly IUserContext _userContext;

    public ApproveRequestCommandHandler(IApprovalRequestRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.ApprovalRequestId, cancellationToken);
        if (entity is null) return Result.Failure("Approval request not found.");

        var result = entity.Approve(ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
