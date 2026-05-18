namespace Ums.Domain.Approvals.DocumentType.NotificationRule;

public sealed class NotificationRule : Entity<NotificationRule, NotificationRuleProps>
{
    private NotificationRule(NotificationRuleProps props) : base(props) { }

    public int DaysBefore => Props.DaysBefore;
    public NotificationChannel[] Channels => Props.Channels;
    public Code Code => Props.Code;
    public Description Description => Props.Description;

    public static Result<NotificationRule> Create(
        int daysBefore,
        NotificationChannel[] channels,
        Code code,
        Description description)
    {
        if (daysBefore <= 0)
        {
            return Result<NotificationRule>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        if (channels is null || channels.Length == 0)
        {
            return Result<NotificationRule>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        var props = new NotificationRuleProps(IdValueObject.Create(), daysBefore, channels, code, description);
        var rule = new NotificationRule(props);

        if (!rule.IsValid())
        {
            return Result<NotificationRule>.Failure(rule.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<NotificationRule>.Success(rule);
    }
}
