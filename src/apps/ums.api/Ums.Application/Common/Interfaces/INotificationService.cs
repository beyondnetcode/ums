using Ums.Application.Common.Notifications;

namespace Ums.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(UmsNotification notification, CancellationToken cancellationToken = default);
    Task SendBulkAsync(IEnumerable<UmsNotification> notifications, CancellationToken cancellationToken = default);
}
