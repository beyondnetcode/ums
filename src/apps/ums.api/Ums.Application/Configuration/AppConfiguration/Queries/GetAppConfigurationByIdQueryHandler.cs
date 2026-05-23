using Ums.Application.Configuration.AppConfiguration.DTOs;

namespace Ums.Application.Configuration.AppConfiguration.Queries;

using Ums.Domain.Configuration;

public sealed class GetAppConfigurationByIdQueryHandler : IQueryHandler<GetAppConfigurationByIdQuery, AppConfigurationDto>
{
    private readonly IAppConfigurationRepository _repository;

    public GetAppConfigurationByIdQueryHandler(IAppConfigurationRepository repository) => _repository = repository;

    public async Task<Result<AppConfigurationDto>> Handle(GetAppConfigurationByIdQuery request, CancellationToken cancellationToken)
    {
        var appConfiguration = await _repository.GetByIdAsync(request.AppConfigurationId, cancellationToken);
        if (appConfiguration is null)
        {
            return Result<AppConfigurationDto>.Failure("App configuration was not found.");
        }

        return Result<AppConfigurationDto>.Success(new AppConfigurationDto(
            appConfiguration.Props.Id.GetValue(),
            appConfiguration.Props.TenantId?.GetValue(),
            appConfiguration.Props.SystemSuiteId?.GetValue(),
            appConfiguration.Props.ModuleId?.GetValue(),
            appConfiguration.Props.Code.GetValue(),
            appConfiguration.Props.Value.GetValue(),
            appConfiguration.Props.Description.GetValue(),
            appConfiguration.Props.Scope.Name,
            appConfiguration.Props.IsInheritable,
            appConfiguration.Props.IsEncrypted,
            appConfiguration.Props.Version,
            appConfiguration.Props.Status.Name));
    }
}
