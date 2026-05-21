using Ums.Application.Approvals.UserDocument.DTOs;

namespace Ums.Application.Approvals.UserDocument.Commands;

public sealed record ValidateUserDocumentCommand(Guid UserDocumentId) : ICommand;
