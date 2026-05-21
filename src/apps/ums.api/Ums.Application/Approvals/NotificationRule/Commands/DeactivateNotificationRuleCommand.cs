using Ums.Application.Approvals.NotificationRule.DTOs;

namespace Ums.Application.Approvals.NotificationRule.Commands;

public sealed record DeactivateNotificationRuleCommand(Guid NotificationRuleId) : ICommand;
