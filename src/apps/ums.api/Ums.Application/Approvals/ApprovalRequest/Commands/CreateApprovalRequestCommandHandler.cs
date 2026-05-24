using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalRequest;

public sealed class CreateApprovalRequestCommandHandler : ICommandHandler<CreateApprovalRequestCommand, CreateApprovalRequestResponse>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly IUserContext _userContext;

    public CreateApprovalRequestCommandHandler(IApprovalRequestRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateApprovalRequestResponse>> Handle(CreateApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateApprovalRequestResponse>.Failure("Authenticated user is required.");

        var result = ApprovalRequest.Create(
            ApprovalWorkflowId.Load(request.WorkflowId),
            UserId.Load(request.TargetUserId),
            request.TargetProfileId.HasValue ? ProfileId.Load(request.TargetProfileId.Value) : null,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreateApprovalRequestResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateApprovalRequestResponse>.Success(new CreateApprovalRequestResponse(result.Value.Props.Id.GetValue()));
    }
}
