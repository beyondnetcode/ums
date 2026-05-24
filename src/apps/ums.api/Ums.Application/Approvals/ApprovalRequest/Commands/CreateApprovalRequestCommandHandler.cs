using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Application.Approvals.ApprovalRequest.Services;

public sealed class CreateApprovalRequestCommandHandler : ICommandHandler<CreateApprovalRequestCommand, CreateApprovalRequestResponse>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly IApprovalWorkflowRepository _workflowRepository;
    private readonly IApprovalRequestCreationPolicyResolver _creationPolicyResolver;
    private readonly IUserContext _userContext;

    public CreateApprovalRequestCommandHandler(
        IApprovalRequestRepository repository,
        IApprovalWorkflowRepository workflowRepository,
        IApprovalRequestCreationPolicyResolver creationPolicyResolver,
        IUserContext userContext)
    {
        _repository = repository;
        _workflowRepository = workflowRepository;
        _creationPolicyResolver = creationPolicyResolver;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateApprovalRequestResponse>> Handle(CreateApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateApprovalRequestResponse>.Failure("Authenticated user is required.");

        var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
        if (workflow is null)
            return Result<CreateApprovalRequestResponse>.Failure("Approval workflow not found.");

        var result = _creationPolicyResolver.Create(
            workflow,
            UserId.Load(request.TargetUserId),
            request.TargetProfileId.HasValue ? ProfileId.Load(request.TargetProfileId.Value) : null,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreateApprovalRequestResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateApprovalRequestResponse>.Success(new CreateApprovalRequestResponse(result.Value.Props.Id.GetValue()));
    }
}
