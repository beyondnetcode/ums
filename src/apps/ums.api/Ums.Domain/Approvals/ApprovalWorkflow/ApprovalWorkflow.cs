namespace Ums.Domain.Approvals.ApprovalWorkflow;

using Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument;
using Ums.Domain.Approvals.ApprovalWorkflow.Events;
using ApprovalRequiredDocumentEntity = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument.ApprovalRequiredDocument;

public sealed class ApprovalWorkflow : AggregateRoot<ApprovalWorkflow, ApprovalWorkflowProps>
{
    public new ApprovalWorkflowDomainEventsManager DomainEvents { get; }
    private readonly List<ApprovalRequiredDocumentEntity> _requiredDocuments = new();

    public ApprovalWorkflowId GetId() => ApprovalWorkflowId.Load(Props.Id.GetValue());

    public TenantId TenantId => Props.TenantId;
    public SystemSuiteId? SystemSuiteId => Props.SystemSuiteId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public UserCategory TargetUserCategory => Props.TargetUserCategory;
    public bool RequiresApproval => Props.RequiresApproval;

    public IReadOnlyCollection<ApprovalRequiredDocumentEntity> RequiredDocuments => _requiredDocuments.AsReadOnly();

    private ApprovalWorkflow(ApprovalWorkflowProps props) : base(props)
    {
        DomainEvents = new ApprovalWorkflowDomainEventsManager(this);
    }

    public static Result<ApprovalWorkflow> Create(
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        UserCategory targetUserCategory,
        bool requiresApproval,
        SystemSuiteId? systemSuiteId,
        ActorId createdBy,
        int requiredDocumentCount = 0)
    {
        var props = new ApprovalWorkflowProps(IdValueObject.Create(), tenantId, systemSuiteId, code, name, description, targetUserCategory, requiresApproval, createdBy);
        var workflow = new ApprovalWorkflow(props);

        if (requiresApproval && requiredDocumentCount <= 0)
        {
            workflow.BrokenRules.Add(new BrokenRule(nameof(RequiredDocuments), DomainErrors.Approvals.RequiresDocumentsIfApprovalRequired));
        }

        if (!workflow.IsValid())
        {
            return Result<ApprovalWorkflow>.Failure(workflow.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<ApprovalWorkflow>.Success(workflow);
    }

    public Result AddRequiredDocument(DocumentTypeId documentTypeId, bool isMandatory, ActorId createdBy)
    {
        if (_requiredDocuments.Any(d => d.DocumentTypeId == documentTypeId))
        {
            BrokenRules.Add(new BrokenRule(nameof(RequiredDocuments), DomainErrors.Approvals.DocumentTypeAlreadyRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var documentResult = ApprovalRequiredDocumentEntity.Create(GetId(), documentTypeId, isMandatory, createdBy);
        if (documentResult.IsFailure)
        {
            return Result.Failure(documentResult.Error);
        }

        _requiredDocuments.Add(documentResult.Value);
        DomainEvents.RaiseEvent(new ApprovalWorkflowDocumentAddedEvent(
            Props.Id.GetValue(),
            documentTypeId.GetValue(),
            isMandatory));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveRequiredDocument(IdValueObject documentId, ActorId updatedBy)
    {
        var document = FindRequiredDocument(documentId);
        if (document.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(RequiredDocuments), DomainErrors.Common.NotFound));
        }

        // If RequiresApproval is true, at least one document must remain after removal
        if (Props.RequiresApproval && _requiredDocuments.Count <= 1)
        {
            BrokenRules.Add(new BrokenRule(nameof(RequiredDocuments), DomainErrors.Approvals.RequiresDocumentsIfApprovalRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var documentTypeId = document.Value.DocumentTypeId;
        _requiredDocuments.Remove(document.Value);
        DomainEvents.RaiseEvent(new ApprovalWorkflowDocumentRemovedEvent(
            Props.Id.GetValue(),
            documentTypeId.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<ApprovalRequiredDocumentEntity> FindRequiredDocument(IdValueObject documentId)
    {
        var document = _requiredDocuments.FirstOrDefault(d => d.Id.GetValue() == documentId.GetValue());
        return document is null
            ? Result<ApprovalRequiredDocumentEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ApprovalRequiredDocumentEntity>.Success(document);
    }
}
