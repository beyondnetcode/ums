namespace Ums.Domain.Compliance;

public sealed class NotificationRule : ParametricCatalogEntity<NotificationRule, NotificationRuleProps>
{
    private NotificationRule(NotificationRuleProps props) : base(props) { }

    public string TriggerEvent => Props.TriggerEvent.GetValue();
    public string Channel => Props.Channel.GetValue();

    public static Result<NotificationRule> Create(Guid tenantId, string code, string value, string description, string triggerEvent, string channel, string version = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent) || string.IsNullOrWhiteSpace(channel))
            return Result<NotificationRule>.Failure(DomainErrors.Compliance.NotificationTriggerRequired);

        var props = new NotificationRuleProps
        {
            TriggerEvent = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(triggerEvent.Trim()),
            Channel = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(channel.Trim())
        };

        var rule = new NotificationRule(props);
        var result = rule.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<NotificationRule>.Failure(result.Error) : Result<NotificationRule>.Success(rule);
    }
}
