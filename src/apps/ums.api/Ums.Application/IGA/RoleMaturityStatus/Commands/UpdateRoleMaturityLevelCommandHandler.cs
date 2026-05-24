using Ums.Application.IGA.RoleMaturityStatus.DTOs;

namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

using Ums.Domain;
using Ums.Domain.IGA;
using Ums.Domain.Enums;

public sealed class UpdateRoleMaturityLevelCommandHandler : ICommandHandler<UpdateRoleMaturityLevelCommand>
{
    private readonly IRoleMaturityStatusRepository _repository;
    private readonly IUserContext _userContext;

    public UpdateRoleMaturityLevelCommandHandler(IRoleMaturityStatusRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateRoleMaturityLevelCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.RoleMaturityStatusId, cancellationToken);
        if (entity is null) return Result.Failure("Role maturity status not found.");

        var level = Enum.Parse<RoleMaturityLevel>(request.NewLevel, true);
        if (level == default) return Result.Failure("Invalid maturity level.");

        var result = entity.UpdateMaturityLevel(level, ActorId.Create(_userContext.UserId));
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
