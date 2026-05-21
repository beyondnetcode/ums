using Ums.Application.Approvals.ApprovalWorkflow.DTOs;

namespace Ums.Application.Approvals.ApprovalWorkflow.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Enums;

public sealed class CreateApprovalWorkflowCommandHandler : ICommandHandler<CreateApprovalWorkflowCommand, CreateApprovalWorkflowResponse>
{
    private readonly IApprovalWorkflowRepository _repository;
    private readonly IUserContext _userContext;

    public CreateApprovalWorkflowCommandHandler(IApprovalWorkflowRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<CreateApprovalWorkflowResponse>> Handle(CreateApprovalWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateApprovalWorkflowResponse>.Failure("Authenticated user is required.");

        var category = DomainEnumerationParser.FromName<UserCategory>(request.TargetUserCategory)!;

        var result = ApprovalWorkflow.Create(
            TenantId.Load(request.TenantId),
            Code.Create(request.Code),
            Name.Create(request.Name),
            Description.Create(request.Description),
            category,
            request.RequiresApproval,
            request.SystemSuiteId.HasValue ? SystemSuiteId.Load(request.SystemSuiteId.Value) : null,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
            return Result<CreateApprovalWorkflowResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateApprovalWorkflowResponse>.Success(new CreateApprovalWorkflowResponse(result.Value.Props.Id.GetValue()));
    }
}
