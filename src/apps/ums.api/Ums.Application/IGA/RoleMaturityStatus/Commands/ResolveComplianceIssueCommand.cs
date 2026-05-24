namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record ResolveComplianceIssueCommand(Guid RoleMaturityStatusId) : ICommand;
