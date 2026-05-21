using Ums.Application.Approvals.DocumentType.DTOs;

namespace Ums.Application.Approvals.DocumentType.Queries;

public sealed record GetDocumentTypeByIdQuery(Guid DocumentTypeId) : IQuery<DocumentTypeDto>;
