namespace Ums.Application.Common.Notifications;

public sealed record UmsNotification(
    string Recipient,
    string Subject,
    string Body,
    NotificationChannel Channel = NotificationChannel.Email,
    string? RecipientName = null,
    Dictionary<string, string>? Metadata = null
);
