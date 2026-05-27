namespace Ums.Application.Authorization.Template.Commands;

using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Template;

public sealed class DeletePermissionTemplateCommandHandler : ICommandHandler<DeletePermissionTemplateCommand>
{
    private readonly IPermissionTemplateRepository _templateRepository;

    public DeletePermissionTemplateCommandHandler(IPermissionTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(DeletePermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result.Failure("Template was not found.");
        }

        if (template.Status != TemplateStatus.Draft)
        {
            return Result.Failure(DomainErrors.Authorization.TemplateNotDraft);
        }

        var deleted = await _templateRepository.DeleteAsync(request.TemplateId, cancellationToken);
        if (!deleted)
        {
            return Result.Failure("Template could not be deleted.");
        }

        await _templateRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
