namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record UpdatePerformanceScoreCommand(Guid RoleMaturityStatusId, decimal Score) : ICommand;
