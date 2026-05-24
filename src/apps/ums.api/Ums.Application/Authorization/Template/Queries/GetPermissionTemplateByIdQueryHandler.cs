using Ums.Application.Authorization.Template.DTOs;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Template;

namespace Ums.Application.Authorization.Template.Queries;

public sealed class GetPermissionTemplateByIdQueryHandler : IQueryHandler<GetPermissionTemplateByIdQuery, PermissionTemplateDto>
{
    private readonly IPermissionTemplateRepository _templateRepository;

    public GetPermissionTemplateByIdQueryHandler(IPermissionTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PermissionTemplateDto>> Handle(
        GetPermissionTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);

        if (template is null)
        {
            return Result<PermissionTemplateDto>.Failure("Permission template not found.");
        }

        return Result<PermissionTemplateDto>.Success(new PermissionTemplateDto(
            template.Props.Id.GetValue(),
            template.Props.TenantId.GetValue(),
            template.Props.RoleId.GetValue(),
            template.Props.SystemSuiteId.GetValue(),
            template.Props.Version.GetValue(),
            template.Props.Status.ToString()));
    }
}
