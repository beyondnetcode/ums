namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record ReviewEligibilityCommand(Guid RoleMaturityStatusId) : ICommand;
