using Ums.Domain.Authorization;

namespace Ums.Application.Authorization.Role.Commands;

public sealed class SetRoleStatusCommandHandler : ICommandHandler<SetRoleStatusCommand>
{
    private readonly IRoleRepository _repository;
    private readonly IUserContext _userContext;

    public SetRoleStatusCommandHandler(IRoleRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(SetRoleStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to change a role status.");
        }

        var role = await _repository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure("No se pudo cambiar el estado porque el rol no existe.");
        }

        var actor = ActorId.Create(_userContext.UserId);
        var result = request.IsActive ? role.Activate(actor) : role.Deactivate(actor);
        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UpdateAsync(role, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
