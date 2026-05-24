using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Approvals.UserDocument.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.UserDocument;
using Ums.Domain.Enums;

public sealed class UploadUserDocumentCommandHandler : ICommandHandler<UploadUserDocumentCommand, UploadUserDocumentResponse>
{
    private readonly IUserDocumentRepository _repository;
    private readonly IUserContext _userContext;

    public UploadUserDocumentCommandHandler(IUserDocumentRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<UploadUserDocumentResponse>> Handle(UploadUserDocumentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<UploadUserDocumentResponse>.Failure("Authenticated user is required.");

        var criticity = DomainEnumerationParser.FromName<DocumentCriticity>(request.Criticity)!;

        var result = UserDocument.Upload(
            UserId.Load(request.UserId),
            DocumentTypeId.Load(request.DocumentTypeId),
            request.IssueDate,
            request.ExpirationDate,
            criticity,
            TextValueObject.Create(request.FileStoragePath),
            request.FileChecksum,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<UploadUserDocumentResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<UploadUserDocumentResponse>.Success(new UploadUserDocumentResponse(result.Value.Props.Id.GetValue()));
    }
}
