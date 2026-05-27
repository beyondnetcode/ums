using Ums.Application.Authorization.Role.DTOs;
using Ums.Domain.Authorization;
using RoleAggregate = Ums.Domain.Authorization.Role.Role;

namespace Ums.Application.Authorization.Role.Commands;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ISystemSuiteRepository _suiteRepository;
    private readonly IUserContext _userContext;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        ISystemSuiteRepository suiteRepository,
        IUserContext userContext)
    {
        _roleRepository = roleRepository;
        _suiteRepository = suiteRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateRoleResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateRoleResponse>.Failure("Authenticated user is required to create a role.");
        }

        var suite = await _suiteRepository.GetByIdAsync(request.SystemSuiteId, cancellationToken);
        if (suite is null)
        {
            return Result<CreateRoleResponse>.Failure("No se pudo registrar el rol porque la suite seleccionada no existe.");
        }

        if (await _roleRepository.GetByCodeAsync(request.SystemSuiteId, Code.Create(request.Code), cancellationToken) is not null)
        {
            return Result<CreateRoleResponse>.Failure($"No se pudo registrar el rol porque el campo Código ya existe en esta suite. Valor ingresado: {request.Code}.");
        }

        var parentValidation = await ValidateParentAsync(request.ParentRoleId, request.SystemSuiteId, request.HierarchyLevel, cancellationToken);
        if (parentValidation.IsFailure)
        {
            return Result<CreateRoleResponse>.Failure(parentValidation.Error);
        }

        var result = RoleAggregate.Create(
            suite.TenantId,
            SystemSuiteId.Load(request.SystemSuiteId),
            Code.Create(request.Code),
            Name.Create(request.Value),
            Description.Create(request.Description),
            request.ParentRoleId.HasValue ? RoleId.Load(request.ParentRoleId.Value) : null,
            request.HierarchyLevel,
            request.PromotionOrder,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<CreateRoleResponse>.Failure(result.Error);
        }

        await _roleRepository.AddAsync(result.Value, cancellationToken);
        await _roleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result<CreateRoleResponse>.Success(new CreateRoleResponse(result.Value.GetId().GetValue()));
    }

    private async Task<Result> ValidateParentAsync(Guid? parentId, Guid suiteId, int hierarchyLevel, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return hierarchyLevel == 0
                ? Result.Success()
                : Result.Failure("No se pudo registrar el rol porque un rol raíz debe tener nivel jerárquico 0.");
        }

        var parent = await _roleRepository.GetByIdAsync(parentId.Value, cancellationToken);
        if (parent is null || parent.SystemSuiteId.GetValue() != suiteId)
        {
            return Result.Failure("No se pudo registrar el rol porque el rol padre no pertenece a la suite seleccionada.");
        }

        return hierarchyLevel == parent.HierarchyLevel + 1
            ? Result.Success()
            : Result.Failure($"No se pudo registrar el rol porque el nivel jerárquico debe ser {parent.HierarchyLevel + 1} para el rol padre seleccionado.");
    }
}
