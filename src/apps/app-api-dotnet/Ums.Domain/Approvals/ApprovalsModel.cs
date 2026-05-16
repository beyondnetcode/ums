namespace Ums.Domain.Approvals;

using Ums.Domain.Kernel;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

public class ApprovalWorkflowProps : ParametricCatalogProps
{
    public StringValueObject RequestType { get; set; } = default!;
    public int RequiredApprovals { get; set; }
    public LifecycleStatus Status { get; set; } = LifecycleStatus.Draft;

    public ApprovalWorkflowProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class ApprovalWorkflow : ParametricCatalogEntity<ApprovalWorkflow, ApprovalWorkflowProps>
{
    private ApprovalWorkflow(ApprovalWorkflowProps props) : base(props) { }

    public string RequestType => Props.RequestType.GetValue();
    public int RequiredApprovals => Props.RequiredApprovals;
    public LifecycleStatus Status => Props.Status;

    public static Result<ApprovalWorkflow> Create(Guid tenantId, string code, string value, string description, string requestType, int requiredApprovals, string version = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(requestType))
            return Result<ApprovalWorkflow>.Failure("Request type is required.");

        if (requiredApprovals <= 0)
            return Result<ApprovalWorkflow>.Failure("At least one approval is required.");

        var props = new ApprovalWorkflowProps
        {
            RequestType = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(requestType.Trim()),
            RequiredApprovals = requiredApprovals
        };

        var workflow = new ApprovalWorkflow(props);
        var result = workflow.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<ApprovalWorkflow>.Failure(result.Error) : Result<ApprovalWorkflow>.Success(workflow);
    }
}

public class ApprovalRequestProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Approvals.ValueObjects.WorkflowId WorkflowId { get; private set; }
    public IdValueObject RequestedBy { get; private set; }
    public IdValueObject? ResolvedBy { get; set; }
    public StringValueObject RequestType { get; private set; }
    public StringValueObject Justification { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.Payload Payload { get; private set; }
    public ApprovalRequestStatus Status { get; set; }
    public StringValueObject? ResolutionComment { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ApprovalRequestProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Approvals.ValueObjects.WorkflowId workflowId, IdValueObject requestedBy, StringValueObject requestType, StringValueObject justification, global::Ums.Domain.Authorization.ValueObjects.Payload payload)
    {
        Id = id;
        TenantId = tenantId;
        WorkflowId = workflowId;
        RequestedBy = requestedBy;
        RequestType = requestType;
        Justification = justification;
        Payload = payload;
        Status = ApprovalRequestStatus.Pending;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class ApprovalRequest : AggregateRoot<ApprovalRequest, ApprovalRequestProps>
{
    private ApprovalRequest(ApprovalRequestProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new ApprovalRequestedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.RequestType.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid WorkflowId => Props.WorkflowId.GetValue();
    public Guid RequestedBy => Props.RequestedBy.GetValue();
    public Guid? ResolvedBy => Props.ResolvedBy?.GetValue();
    public string RequestType => Props.RequestType.GetValue();
    public string Justification => Props.Justification.GetValue();
    public string Payload => Props.Payload.GetValue();
    public ApprovalRequestStatus Status => Props.Status;
    public string? ResolutionComment => Props.ResolutionComment?.GetValue();

    public static Result<ApprovalRequest> Submit(Guid tenantId, Guid workflowId, Guid requestedBy, string requestType, string justification, string payload = "{}")
    {
        if (tenantId == Guid.Empty || workflowId == Guid.Empty || requestedBy == Guid.Empty)
            return Result<ApprovalRequest>.Failure("Tenant, workflow, and requester identifiers are required.");

        if (string.IsNullOrWhiteSpace(requestType))
            return Result<ApprovalRequest>.Failure("Request type is required.");

        if (string.IsNullOrWhiteSpace(justification))
            return Result<ApprovalRequest>.Failure("Justification is required.");

        var props = new ApprovalRequestProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Approvals.ValueObjects.WorkflowId.Load(workflowId),
            IdValueObject.Load(requestedBy),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(requestType.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(justification.Trim()),
            global::Ums.Domain.Authorization.ValueObjects.Payload.Create(string.IsNullOrWhiteSpace(payload) ? "{}" : payload.Trim()));

        var request = new ApprovalRequest(props);
        return Result<ApprovalRequest>.Success(request);
    }

    public Result Resolve(Guid resolvedBy, ApprovalDecision decision, string comment)
    {
        if (Props.Status != ApprovalRequestStatus.Pending)
            return Result.Failure("Only pending approval requests can be resolved.");

        if (resolvedBy == Guid.Empty)
            return Result.Failure("Resolver identifier is required.");

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure("Resolution comment is required.");

        Props.ResolvedBy = IdValueObject.Load(resolvedBy);
        Props.ResolutionComment = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(comment.Trim());
        
        if (decision == ApprovalDecision.Approved)
            Props.Status = ApprovalRequestStatus.Approved;
        else if (decision == ApprovalDecision.Rejected)
            Props.Status = ApprovalRequestStatus.Rejected;
        else
            Props.Status = ApprovalRequestStatus.Cancelled;

        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new ApprovalCompletedEvent(TenantId, GetId(), decision.Name), true);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}

public class ExternalAccessRequestProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId SponsorUserId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.OrganizationId TargetOrganizationId { get; private set; }
    public EmailAddress TargetUserEmail { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.ProfileId RequestedProfileId { get; private set; }
    public StringValueObject Justification { get; private set; }
    public ApprovalRequestStatus Status { get; set; }
    public IdValueObject? ApprovalRequestId { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ExternalAccessRequestProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId sponsorUserId, global::Ums.Domain.Kernel.ValueObjects.OrganizationId targetOrganizationId, EmailAddress targetUserEmail, global::Ums.Domain.Authorization.ValueObjects.ProfileId requestedProfileId, StringValueObject justification)
    {
        Id = id;
        TenantId = tenantId;
        SponsorUserId = sponsorUserId;
        TargetOrganizationId = targetOrganizationId;
        TargetUserEmail = targetUserEmail;
        RequestedProfileId = requestedProfileId;
        Justification = justification;
        Status = ApprovalRequestStatus.Draft;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class ExternalAccessRequest : AggregateRoot<ExternalAccessRequest, ExternalAccessRequestProps>
{
    private ExternalAccessRequest(ExternalAccessRequestProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SponsorUserId => Props.SponsorUserId.GetValue();
    public Guid TargetOrganizationId => Props.TargetOrganizationId.GetValue();
    public string TargetUserEmail => Props.TargetUserEmail.GetValue();
    public Guid RequestedProfileId => Props.RequestedProfileId.GetValue();
    public string Justification => Props.Justification.GetValue();
    public ApprovalRequestStatus Status => Props.Status;
    public Guid? ApprovalRequestId => Props.ApprovalRequestId?.GetValue();

    public static Result<ExternalAccessRequest> Draft(
        Guid tenantId,
        Guid sponsorUserId,
        Guid targetOrganizationId,
        string targetUserEmail,
        Guid requestedProfileId,
        string justification)
    {
        if (tenantId == Guid.Empty || sponsorUserId == Guid.Empty || targetOrganizationId == Guid.Empty || requestedProfileId == Guid.Empty)
            return Result<ExternalAccessRequest>.Failure("Tenant, sponsor, target organization, and requested profile identifiers are required.");

        var targetEmailResult = global::Ums.Domain.Kernel.ValueObjects.EmailAddress.Create(targetUserEmail);
        if (targetEmailResult.IsFailure) return Result<ExternalAccessRequest>.Failure(targetEmailResult.Error);
        var targetEmail = targetEmailResult.Value;

        if (string.IsNullOrWhiteSpace(justification))
            return Result<ExternalAccessRequest>.Failure("Justification is required.");

        var props = new ExternalAccessRequestProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(sponsorUserId),
            global::Ums.Domain.Kernel.ValueObjects.OrganizationId.Load(targetOrganizationId),
            targetEmail,
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(requestedProfileId),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(justification.Trim()));

        return Result<ExternalAccessRequest>.Success(new ExternalAccessRequest(props));
    }

    public Result Submit(Guid approvalRequestId)
    {
        if (approvalRequestId == Guid.Empty)
            return Result.Failure("Approval request identifier is required.");

        if (Props.Status != ApprovalRequestStatus.Draft)
            return Result.Failure("Only draft external access requests can be submitted.");

        Props.ApprovalRequestId = IdValueObject.Load(approvalRequestId);
        Props.Status = ApprovalRequestStatus.Pending;
        Props.Audit.Update("system");
        
        return Result.Success();
    }
}
