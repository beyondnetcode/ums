namespace Ums.Domain.Approvals.DocumentType;

using Ums.Domain.Approvals.DocumentType.Events;
using Ums.Domain.Approvals.DocumentType.NotificationRule;
using Ums.Domain.Approvals.DocumentType.EnforcementPolicy;
using NotificationRuleEntity = Ums.Domain.Approvals.DocumentType.NotificationRule.NotificationRule;
using EnforcementPolicyEntity = Ums.Domain.Approvals.DocumentType.EnforcementPolicy.EnforcementPolicy;

public sealed class DocumentType : AggregateRoot<DocumentType, DocumentTypeProps>
{
    private readonly List<NotificationRuleEntity> _notificationRules = new();
    private EnforcementPolicyEntity? _enforcementPolicy;

    public new DocumentTypeDomainEventsManager DomainEvents { get; }

    private DocumentType(DocumentTypeProps props) : base(props)
    {
        DomainEvents = new DocumentTypeDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new DocumentTypeRegisteredEvent(
                props.Id.GetValue(), props.Criticity.Name, props.TenantId.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public DocumentCriticity Criticity => Props.Criticity;

    public IReadOnlyCollection<NotificationRuleEntity> NotificationRules => _notificationRules.AsReadOnly();
    public EnforcementPolicyEntity? EnforcementPolicy => _enforcementPolicy;

    public DocumentTypeId GetId() => DocumentTypeId.Load(Props.Id.GetValue());

    public static Result<DocumentType> Create(
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        DocumentCriticity criticity,
        ActorId createdBy)
    {
        var props = new DocumentTypeProps(IdValueObject.Create(), tenantId, code, name, description, criticity, createdBy);
        var documentType = new DocumentType(props);

        if (!documentType.IsValid())
        {
            return Result<DocumentType>.Failure(documentType.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<DocumentType>.Success(documentType);
    }

    public Result Update(Name name, Description description, ActorId updatedBy)
    {
        Props.Name = name;
        Props.Description = description;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // INV-DT2: DaysBefore values must be unique and descending across rules
    public Result ConfigureNotificationRule(
        int daysBefore,
        NotificationChannel[] channels,
        Code code,
        Description description,
        ActorId updatedBy)
    {
        if (_notificationRules.Any(r => r.DaysBefore == daysBefore))
        {
            BrokenRules.Add(new BrokenRule(nameof(NotificationRules), DomainErrors.Compliance.NotificationRuleDaysBeforeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var ruleResult = NotificationRuleEntity.Create(daysBefore, channels, code, description);
        if (ruleResult.IsFailure)
        {
            return Result.Failure(ruleResult.Error);
        }

        _notificationRules.Add(ruleResult.Value);
        DomainEvents.RaiseEvent(new NotificationRuleConfiguredEvent(
            ruleResult.Value.Id.GetValue(), Props.Id.GetValue(), daysBefore));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveNotificationRule(IdValueObject ruleId, ActorId updatedBy)
    {
        var rule = _notificationRules.FirstOrDefault(r => r.Id.GetValue() == ruleId.GetValue());
        if (rule is null)
        {
            return Result.Failure(DomainErrors.Compliance.NotificationRuleNotFound);
        }

        _notificationRules.Remove(rule);
        DomainEvents.RaiseEvent(new NotificationRuleRemovedEvent(ruleId.GetValue(), Props.Id.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // INV-DT1: CRITICAL must have at least one enforcement policy
    // INV-DT3: Only one active enforcement policy per DocumentType
    // INV-DT4: Non-critical cannot have BLOCK_ACCESS or DOWNGRADE_ROLE
    public Result DefineEnforcementPolicy(
        AccessEnforcementAction actionOnExpiration,
        int? gracePeriodDays,
        ActorId updatedBy)
    {
        if (_enforcementPolicy is not null)
        {
            BrokenRules.Add(new BrokenRule(nameof(EnforcementPolicy), DomainErrors.Compliance.EnforcementPolicyAlreadyDefined));
        }

        if (Criticity != DocumentCriticity.Critical && Criticity != DocumentCriticity.High
            && (actionOnExpiration == AccessEnforcementAction.BlockUser || actionOnExpiration == AccessEnforcementAction.RestrictProfile))
        {
            BrokenRules.Add(new BrokenRule(nameof(Criticity), DomainErrors.Compliance.CriticalRequiresEnforcementPolicy));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var policyResult = EnforcementPolicyEntity.Create(actionOnExpiration, gracePeriodDays);
        if (policyResult.IsFailure)
        {
            return Result.Failure(policyResult.Error);
        }

        _enforcementPolicy = policyResult.Value;
        DomainEvents.RaiseEvent(new EnforcementPolicyDefinedEvent(
            _enforcementPolicy.Id.GetValue(), Props.Id.GetValue(), actionOnExpiration.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateEnforcementPolicy(
        AccessEnforcementAction actionOnExpiration,
        int? gracePeriodDays,
        ActorId updatedBy)
    {
        if (_enforcementPolicy is null)
        {
            return Result.Failure(DomainErrors.Compliance.EnforcementPolicyNotFound);
        }

        var updateResult = _enforcementPolicy.Update(actionOnExpiration, gracePeriodDays);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        DomainEvents.RaiseEvent(new EnforcementPolicyUpdatedEvent(
            _enforcementPolicy.Id.GetValue(), Props.Id.GetValue(), actionOnExpiration.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
