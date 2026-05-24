namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record MarkComplianceIssueCommand(Guid RoleMaturityStatusId, string Reason) : ICommand;
