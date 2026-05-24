using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.SystemSuite;

namespace Ums.Application.Authorization.SystemSuite.Queries;

public sealed class GetSystemSuiteByIdQueryHandler : IQueryHandler<GetSystemSuiteByIdQuery, SystemSuiteDto>
{
    private readonly ISystemSuiteRepository _systemSuiteRepository;

    public GetSystemSuiteByIdQueryHandler(ISystemSuiteRepository systemSuiteRepository)
    {
        _systemSuiteRepository = systemSuiteRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<SystemSuiteDto>> Handle(
        GetSystemSuiteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var systemSuite = await _systemSuiteRepository.GetByIdAsync(request.SystemSuiteId, cancellationToken);

        if (systemSuite is null)
        {
            return Result<SystemSuiteDto>.Failure("System suite not found.");
        }

        return Result<SystemSuiteDto>.Success(new SystemSuiteDto(
            systemSuite.Props.Id.GetValue(),
            systemSuite.Props.TenantId.GetValue(),
            systemSuite.Props.Code.GetValue(),
            systemSuite.Props.Name.GetValue(),
            systemSuite.Props.Description.GetValue(),
            systemSuite.Props.Status.ToString()));
    }
}
