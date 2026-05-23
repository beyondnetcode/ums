using Ums.Application.Configuration.AppConfiguration.DTOs;

namespace Ums.Application.Configuration.AppConfiguration.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Configuration;

public sealed class CreateAppConfigurationCommandHandler : ICommandHandler<CreateAppConfigurationCommand, CreateAppConfigurationResponse>
{
    private readonly IAppConfigurationRepository _repository;
    private readonly IUserContext _userContext;

    public CreateAppConfigurationCommandHandler(IAppConfigurationRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<CreateAppConfigurationResponse>> Handle(CreateAppConfigurationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateAppConfigurationResponse>.Failure("Authenticated user is required.");
        }

        var code = Code.Create(request.Code);
        var existing = await _repository.GetByScopeAndCodeAsync(
            request.TenantId,
            request.SystemSuiteId,
            request.ModuleId,
            code.GetValue(),
            cancellationToken);

        if (existing is not null)
        {
            return Result<CreateAppConfigurationResponse>.Failure("App configuration code already exists for the selected scope.");
        }

        var result = Ums.Domain.Configuration.AppConfiguration.AppConfiguration.Create(
            request.TenantId.HasValue ? TenantId.Load(request.TenantId.Value) : null,
            request.SystemSuiteId.HasValue ? SystemSuiteId.Load(request.SystemSuiteId.Value) : null,
            request.ModuleId.HasValue ? IdValueObject.Load(request.ModuleId.Value) : null,
            code,
            ConfigurationValue.Create(request.Value),
            Description.Create(request.Description),
            request.IsInheritable,
            request.IsEncrypted,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<CreateAppConfigurationResponse>.Failure(result.Error);
        }

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateAppConfigurationResponse>.Success(new CreateAppConfigurationResponse(result.Value.Props.Id.GetValue()));
    }
}
