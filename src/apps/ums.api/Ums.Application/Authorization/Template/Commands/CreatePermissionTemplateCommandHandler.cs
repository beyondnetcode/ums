using Ums.Application.Authorization.Template.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Authorization.Template.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Template;

public sealed class CreatePermissionTemplateCommandHandler : ICommandHandler<CreatePermissionTemplateCommand, CreatePermissionTemplateResponse>
{
    private readonly IPermissionTemplateRepository _templateRepository;
    private readonly IUserContext _userContext;

    public CreatePermissionTemplateCommandHandler(
        IPermissionTemplateRepository templateRepository,
        IUserContext userContext)
    {
        _templateRepository = templateRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreatePermissionTemplateResponse>> Handle(
        CreatePermissionTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreatePermissionTemplateResponse>.Failure("Authenticated user is required to create a permission template.");
        }

        var templateResult = PermissionTemplate.Create(
            TenantId.Load(request.TenantId),
            RoleId.Load(request.RoleId),
            SystemSuiteId.Load(request.SystemSuiteId),
            ActorId.Create(_userContext.UserId));

        if (templateResult.IsFailure)
        {
            return Result<CreatePermissionTemplateResponse>.Failure(templateResult.Error);
        }

        await _templateRepository.AddAsync(templateResult.Value, cancellationToken);
        await _templateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreatePermissionTemplateResponse>.Success(
            new CreatePermissionTemplateResponse(templateResult.Value.Props.Id.GetValue()));
    }
}
