namespace Ums.Application.Identity.UserManagementDelegation.Commands;

public sealed record ExpireDelegationCommand(Guid DelegationId) : ICommand;
