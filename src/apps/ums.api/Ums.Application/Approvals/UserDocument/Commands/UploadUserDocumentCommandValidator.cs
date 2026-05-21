namespace Ums.Application.Approvals.UserDocument.Commands;

using FluentValidation;

public sealed class UploadUserDocumentCommandValidator : AbstractValidator<UploadUserDocumentCommand>
{
    public UploadUserDocumentCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DocumentTypeId).NotEmpty();
        RuleFor(c => c.IssueDate).NotEmpty();
        RuleFor(c => c.ExpirationDate).NotEmpty();
        RuleFor(c => c.Criticity).NotEmpty();
        RuleFor(c => c.FileStoragePath).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileChecksum).NotEmpty().MaximumLength(128);
    }
}
