namespace Ums.Domain.Configuration.FeatureFlag;

using Ums.Domain.Configuration.FeatureFlag.Events;
using Ums.Domain.Configuration.FeatureFlag.FlagEvaluationLog;

public sealed class FeatureFlag : AggregateRoot<FeatureFlag, FeatureFlagProps>
{
    private readonly List<FlagEvaluationLog> _evaluationLog = new();

    public new FeatureFlagDomainEventsManager DomainEvents { get; }

    private FeatureFlag(FeatureFlagProps props) : base(props)
    {
        DomainEvents = new FeatureFlagDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new FeatureFlagCreatedEvent(
                props.Id.GetValue(), props.FlagCode, props.FlagType.Name));
        }
    }

    public string FlagCode => Props.FlagCode;
    public FlagType FlagType => Props.FlagType;
    public FlagStatus Status => Props.Status;
    public int? RolloutPercentage => Props.RolloutPercentage;

    public IReadOnlyCollection<FlagEvaluationLog> EvaluationLog => _evaluationLog.AsReadOnly();

    public FeatureFlagId GetId() => FeatureFlagId.Load(Props.Id.GetValue());

    public static Result<FeatureFlag> Create(
        string flagCode,
        FlagType flagType,
        string flagTargets,
        LinkedResourceType? linkedResourceType,
        IdValueObject? linkedResourceId,
        int? rolloutPercentage,
        ActorId createdBy)
    {
        if (flagType == FlagType.Percentage && (rolloutPercentage is null || rolloutPercentage < 0 || rolloutPercentage > 100))
        {
            return Result<FeatureFlag>.Failure(DomainErrors.Configuration.FlagPercentageOutOfRange);
        }

        var props = new FeatureFlagProps(
            IdValueObject.Create(), flagCode, flagType, flagTargets,
            linkedResourceType, linkedResourceId, rolloutPercentage, createdBy);

        var flag = new FeatureFlag(props);

        if (!flag.IsValid())
        {
            return Result<FeatureFlag>.Failure(flag.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<FeatureFlag>.Success(flag);
    }

    public Result Activate(ActorId updatedBy)
    {
        if (Status == FlagStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Configuration.FlagArchivedCannotChange));
        }
        else if (Status == FlagStatus.Active)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Configuration.FlagAlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = FlagStatus.Active;
        DomainEvents.RaiseEvent(new FeatureFlagActivatedEvent(Props.Id.GetValue(), Props.FlagCode));
        DomainEvents.RaiseEvent(new FeatureFlagStateChangedEvent(Props.Id.GetValue(), Props.FlagCode, FlagStatus.Active.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        if (Status == FlagStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Configuration.FlagArchivedCannotChange));
        }
        else if (Status == FlagStatus.Inactive)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Configuration.FlagAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = FlagStatus.Inactive;
        DomainEvents.RaiseEvent(new FeatureFlagDeactivatedEvent(Props.Id.GetValue(), Props.FlagCode));
        DomainEvents.RaiseEvent(new FeatureFlagStateChangedEvent(Props.Id.GetValue(), Props.FlagCode, FlagStatus.Inactive.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Archive(ActorId updatedBy)
    {
        if (Status == FlagStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Configuration.FlagArchivedCannotChange));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = FlagStatus.Archived;
        DomainEvents.RaiseEvent(new FeatureFlagArchivedEvent(Props.Id.GetValue(), Props.FlagCode));
        DomainEvents.RaiseEvent(new FeatureFlagStateChangedEvent(Props.Id.GetValue(), Props.FlagCode, FlagStatus.Archived.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result<bool> Evaluate(Guid evaluatedBy, string context)
    {
        if (Status == FlagStatus.Archived)
        {
            return Result<bool>.Failure(DomainErrors.Configuration.FlagArchivedCannotChange);
        }

        var result = Status == FlagStatus.Active;

        _evaluationLog.Add(FlagEvaluationLog.Record(evaluatedBy, result, context));
        DomainEvents.RaiseEvent(new FlagEvaluatedEvent(Props.Id.GetValue(), Props.FlagCode, result, context));
        return Result<bool>.Success(result);
    }
}
