namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record RecordCertificationCompletionCommand(Guid RoleMaturityStatusId) : ICommand;
