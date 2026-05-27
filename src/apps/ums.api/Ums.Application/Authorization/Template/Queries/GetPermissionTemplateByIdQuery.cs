using Ums.Application.Authorization.Template.DTOs;

namespace Ums.Application.Authorization.Template.Queries;

public sealed record GetPermissionTemplateByIdQuery(Guid TemplateId) : IQuery<PermissionTemplateDetailDto>;
