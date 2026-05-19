namespace Ums.Domain.Approvals.DocumentType.NotificationRule;

public class NotificationRuleProps : IProps
{
    public IdValueObject Id { get; set; }
    public int DaysBefore { get; set; }
    public NotificationChannel[] Channels { get; set; }
    public Code Code { get; set; }
    public Description Description { get; set; }

    public NotificationRuleProps(
        IdValueObject id,
        int daysBefore,
        NotificationChannel[] channels,
        Code code,
        Description description)
    {
        Id = id;
        DaysBefore = daysBefore;
        Channels = channels;
        Code = code;
        Description = description;
    }

    public object Clone() => MemberwiseClone();
}
