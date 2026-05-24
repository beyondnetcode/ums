using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain;
using Ums.Domain.IGA;
using Ums.Domain.IGA.RoleMaturityStatus;
using Ums.Domain.Enums;

public sealed class CreateRoleMaturityStatusCommandHandler : ICommandHandler<CreateRoleMaturityStatusCommand, CreateRoleMaturityStatusResponse>
{
    private readonly IRoleMaturityStatusRepository _repository;
    private readonly IUserContext _userContext;

    public CreateRoleMaturityStatusCommandHandler(IRoleMaturityStatusRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateRoleMaturityStatusResponse>> Handle(CreateRoleMaturityStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateRoleMaturityStatusResponse>.Failure("Authenticated user is required.");

        var level = Enum.Parse<RoleMaturityLevel>(request.CurrentMaturityLevel, true);

        var result = RoleMaturityStatus.Create(
            TenantId.Load(request.TenantId),
            UserId.Load(request.UserId),
            RoleId.Load(request.RoleId),
            level,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreateRoleMaturityStatusResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateRoleMaturityStatusResponse>.Success(new CreateRoleMaturityStatusResponse(result.Value.Props.Id.GetValue()));
    }
}
