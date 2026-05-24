namespace Ums.Infrastructure.Approvals.NotificationRule;

internal interface INotificationRecipientStrategy
{
    Result<string> Normalize(string recipient);
}
