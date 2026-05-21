using Ums.Application.Approvals.DocumentType.DTOs;

namespace Ums.Application.Approvals.DocumentType.Commands;

public sealed record CreateDocumentTypeCommand(
    Guid TenantId, string Code, string Name, string Description, string Criticity) : ICommand<CreateDocumentTypeResponse>;
