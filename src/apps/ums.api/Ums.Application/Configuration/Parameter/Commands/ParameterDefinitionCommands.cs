namespace Ums.Application.Configuration.Parameter.Commands;

using Ums.Domain.Configuration;
using Ums.Domain.Configuration.Parameter;
using Ums.Domain.Configuration.Parameter.ValueObjects;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed record CreateParameterDefinitionCommand(
    string Code,
    string Name,
    string Description,
    int    DataTypeId,
    string DefaultValue,
    int    ScopeId,
    bool   IsMandatory,
    int    DisplayOrder) : ICommand<Guid>;

public sealed class CreateParameterDefinitionCommandHandler(
    IParameterDefinitionRepository repo,
    IUserContext userContext)
    : ICommandHandler<CreateParameterDefinitionCommand, Guid>
{
    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<Guid>> Handle(CreateParameterDefinitionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
            return Result<Guid>.Failure("Authenticated user is required.");

        var count = await repo.CountByCodeAsync(cmd.Code.ToUpperInvariant(), ct);
        if (count > 0)
            return Result<Guid>.Failure(DomainErrors.Configuration.ParameterCodeNotUnique);

        var result = ParameterDefinition.Create(
            Code.Create(cmd.Code),
            ParameterName.Create(cmd.Name),
            Description.Create(cmd.Description),
            ParameterDataType.FromValue(cmd.DataTypeId),
            DefaultValue.Create(cmd.DefaultValue),
            ParameterScope.FromValue(cmd.ScopeId),
            isActive:     true,
            isMandatory:  cmd.IsMandatory,
            displayOrder: cmd.DisplayOrder,
            ActorId.Create(userContext.UserId));

        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value.Props.Id.GetValue());
    }
}

// ── Update ───────────────────────────────────────────────────────────────────

public sealed record UpdateParameterDefinitionCommand(
    Guid   Id,
    string Name,
    string Description,
    string DefaultValue,
    int    ScopeId,
    bool   IsActive,
    bool   IsMandatory,
    int    DisplayOrder) : ICommand;

public sealed class UpdateParameterDefinitionCommandHandler(
    IParameterDefinitionRepository repo,
    IUserContext userContext)
    : ICommandHandler<UpdateParameterDefinitionCommand>
{
    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateParameterDefinitionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var definition = await repo.GetByIdAsync(cmd.Id, ct);
        if (definition is null) return Result.Failure(DomainErrors.Common.NotFound);

        var result = definition.Update(
            ParameterName.Create(cmd.Name),
            Description.Create(cmd.Description),
            DefaultValue.Create(cmd.DefaultValue),
            ParameterScope.FromValue(cmd.ScopeId),
            cmd.IsActive, cmd.IsMandatory, cmd.DisplayOrder,
            ActorId.Create(userContext.UserId));

        if (result.IsFailure) return result;

        await repo.UpdateAsync(definition, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Archive ──────────────────────────────────────────────────────────────────

public sealed record ArchiveParameterDefinitionCommand(Guid Id) : ICommand;

public sealed class ArchiveParameterDefinitionCommandHandler(
    IParameterDefinitionRepository repo,
    IUserContext userContext)
    : ICommandHandler<ArchiveParameterDefinitionCommand>
{
    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ArchiveParameterDefinitionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var definition = await repo.GetByIdAsync(cmd.Id, ct);
        if (definition is null) return Result.Failure(DomainErrors.Common.NotFound);

        var globalCount = await repo.CountGlobalValuesAsync(cmd.Id, ct);
        var tenantCount = await repo.CountTenantValuesAsync(cmd.Id, ct);

        var result = definition.Archive(ActorId.Create(userContext.UserId), globalCount, tenantCount);
        if (result.IsFailure) return result;

        await repo.UpdateAsync(definition, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
