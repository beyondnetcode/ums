namespace Ums.Domain.Identity.Tenant.TenantParameter;

using BeyondNetCode.Shell.Ddd;
using BeyondNetCode.Shell.Ddd.Rules.Impl;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.Tenant.TenantParameter.Events;
using CodeEntity = Ums.Domain.Kernel.ValueObjects.Code;
using DescriptionEntity = Ums.Domain.Kernel.ValueObjects.Description;

public sealed class TenantParameter : AggregateRoot<TenantParameter, TenantParameterProps>
{
    public new TenantParameterDomainEventsManager DomainEvents { get; }

    private TenantParameter(TenantParameterProps props) : base(props)
    {
        DomainEvents = new TenantParameterDomainEventsManager(this);
        AddValidationRules();
    }

    public TenantParameterId GetId() => TenantParameterId.Load(Props.Id.GetValue());
    public TenantId TenantId => Props.TenantId;
    public CodeEntity Code => Props.Code;
    public DescriptionEntity Description => Props.Description;
    public string Value => Props.Value;
    public TenantParameterValueType ValueType => Props.ValueType;
    public TenantParameterCategory Category => Props.Category;
    public bool IsActive => Props.IsActive;
    public bool IsSensitive => Props.IsSensitive;
    public string? DefaultValue => Props.DefaultValue;
    public string? AllowedValues => Props.AllowedValues;

    internal bool ValidateValue(string value)
    {
        return Props.ValueType.Id switch
        {
            1 => !string.IsNullOrEmpty(value),
            2 => int.TryParse(value, out _),
            3 => bool.TryParse(value, out _),
            4 => !string.IsNullOrWhiteSpace(value),
            5 => true,
            _ => false
        };
    }

    public static Result<TenantParameter> Create(
        TenantId tenantId,
        string code,
        string description,
        string value,
        TenantParameterValueType valueType,
        TenantParameterCategory category,
        bool isSensitive,
        string? defaultValue,
        string? allowedValues,
        ActorId createdBy)
    {
        var props = new TenantParameterProps(
            IdValueObject.Create(),
            tenantId,
            CodeEntity.Create(code),
            DescriptionEntity.Create(description),
            value,
            valueType,
            category,
            true,
            isSensitive,
            defaultValue,
            allowedValues,
            AuditValueObject.Create(createdBy.GetValue()));

        var parameter = new TenantParameter(props);

        if (!parameter.IsValid())
        {
            return Result<TenantParameter>.Failure(parameter.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<TenantParameter>.Success(parameter);
    }

    public Result UpdateValue(string newValue, ActorId updatedBy)
    {
        if (!ValidateValue(newValue))
        {
            BrokenRules.Add(new BrokenRule(nameof(Value), DomainErrors.TenantParameter.InvalidValueType));
        }

        if (!string.IsNullOrEmpty(Props.AllowedValues))
        {
            var allowed = Props.AllowedValues.Split(',').Select(s => s.Trim()).ToArray();
            if (!allowed.Contains(newValue))
            {
                BrokenRules.Add(new BrokenRule(nameof(Value), DomainErrors.TenantParameter.ValueNotInAllowedList));
            }
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var oldValue = Props.Value;
        SetProps(Props.WithValue(newValue));
        Props.Audit.Update(updatedBy.GetValue());
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new TenantParameterUpdatedEvent(Props.TenantId.GetValue(), GetId().GetValue(), Props.Code.GetValue(), oldValue, newValue));
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        SetProps(Props.WithIsActive(false));
        Props.Audit.Update(updatedBy.GetValue());
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new TenantParameterDeactivatedEvent(Props.TenantId.GetValue(), GetId().GetValue(), Props.Code.GetValue()));
        return Result.Success();
    }

    public Result Reactivate(ActorId updatedBy)
    {
        SetProps(Props.WithIsActive(true));
        Props.Audit.Update(updatedBy.GetValue());
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new TenantParameterReactivatedEvent(Props.TenantId.GetValue(), GetId().GetValue(), Props.Code.GetValue()));
        return Result.Success();
    }

    public string GetTypedValue()
    {
        return Props.Value;
    }

    public bool GetBoolValue(bool defaultValue = false)
    {
        if (Props.ValueType.Id != 3)
            return defaultValue;

        return bool.TryParse(Props.Value, out var result) ? result : defaultValue;
    }

    public int GetIntValue(int defaultValue = 0)
    {
        if (Props.ValueType.Id != 2)
            return defaultValue;

        return int.TryParse(Props.Value, out var result) ? result : defaultValue;
    }

    public string[] GetStringListValue(string[] defaultValue = null!)
    {
        if (Props.ValueType.Id != 4)
            return defaultValue;

        if (string.IsNullOrWhiteSpace(Props.Value))
            return defaultValue;

        return Props.Value.Split(',').Select(s => s.Trim()).ToArray();
    }

    private void AddValidationRules()
    {
        ValidatorRules.Add(new TenantParameterCodeValidator(this));
    }
}

public class TenantParameterCodeValidator : AbstractRuleValidator<TenantParameter>
{
    public TenantParameterCodeValidator(TenantParameter subject) : base(subject) { }

    public override void AddRules(BeyondNetCode.Shell.Ddd.Rules.RuleContext? context)
    {
        if (string.IsNullOrWhiteSpace(Subject.Code.GetValue()))
        {
            AddBrokenRule(nameof(TenantParameter.Code), DomainErrors.Common.Required);
        }
    }
}