namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record RecordTrainingCompletionCommand(Guid RoleMaturityStatusId) : ICommand;
