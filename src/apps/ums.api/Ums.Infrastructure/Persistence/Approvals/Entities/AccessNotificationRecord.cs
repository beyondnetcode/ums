using System;
using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Approvals.Entities;

public sealed class AccessNotificationRecord
{
    public Guid Id { get; set; }
    public Guid UserDocumentId { get; set; }
    public int Step { get; set; }
    public int ChannelId { get; set; }
    public int DaysRemaining { get; set; }
    public DateTime SentAt { get; set; }

    public UserDocumentRecord UserDocument { get; set; } = null!;
}
