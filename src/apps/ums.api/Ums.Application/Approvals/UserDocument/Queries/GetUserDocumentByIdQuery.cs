using Ums.Application.Approvals.UserDocument.DTOs;

namespace Ums.Application.Approvals.UserDocument.Queries;

public sealed record GetUserDocumentByIdQuery(Guid UserDocumentId) : IQuery<UserDocumentDto>;
