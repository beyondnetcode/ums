using Ums.Application.Authorization.SystemSuite.DTOs;

namespace Ums.Application.Authorization.SystemSuite.Commands;

using Ums.Domain.Authorization;
using Ums.Domain.Authorization.SystemSuite;

public sealed class CreateSystemSuiteCommandHandler : ICommandHandler<CreateSystemSuiteCommand, CreateSystemSuiteResponse>
{
    private readonly ISystemSuiteRepository _systemSuiteRepository;
    private readonly IUserContext _userContext;

    public CreateSystemSuiteCommandHandler(
        ISystemSuiteRepository systemSuiteRepository,
        IUserContext userContext)
    {
        _systemSuiteRepository = systemSuiteRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateSystemSuiteResponse>> Handle(
        CreateSystemSuiteCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateSystemSuiteResponse>.Failure("Authenticated user is required to create a system suite.");
        }

        var code = Code.Create(request.Code);
        var existingSuite = await _systemSuiteRepository.GetByCodeAsync(code, cancellationToken);
        if (existingSuite is not null)
        {
            return Result<CreateSystemSuiteResponse>.Failure("System suite code already exists.");
        }

        var systemSuiteResult = SystemSuite.Create(
            TenantId.Load(request.TenantId),
            code,
            Name.Create(request.Name),
            Description.Create(request.Description),
            ActorId.Create(_userContext.UserId));

        if (systemSuiteResult.IsFailure)
        {
            return Result<CreateSystemSuiteResponse>.Failure(systemSuiteResult.Error);
        }

        await _systemSuiteRepository.AddAsync(systemSuiteResult.Value, cancellationToken);
        await _systemSuiteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateSystemSuiteResponse>.Success(
            new CreateSystemSuiteResponse(systemSuiteResult.Value.Props.Id.GetValue()));
    }
}
