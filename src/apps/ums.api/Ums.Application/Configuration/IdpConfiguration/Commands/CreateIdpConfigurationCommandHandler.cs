using Ums.Application.Configuration.IdpConfiguration.DTOs;

namespace Ums.Application.Configuration.IdpConfiguration.Commands;

using Ums.Domain.Configuration;
using Ums.Domain.Enums;

public sealed class CreateIdpConfigurationCommandHandler : ICommandHandler<CreateIdpConfigurationCommand, CreateIdpConfigurationResponse>
{
    private readonly IIdpConfigurationRepository _repository;
    private readonly IUserContext _userContext;

    public CreateIdpConfigurationCommandHandler(IIdpConfigurationRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateIdpConfigurationResponse>> Handle(CreateIdpConfigurationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateIdpConfigurationResponse>.Failure("Authenticated user is required.");
        }

        var providerType = DomainEnumerationParser.FromName<ProviderType>(request.ProviderType);
        if (providerType is null)
        {
            return Result<CreateIdpConfigurationResponse>.Failure("Invalid identity provider type.");
        }

        var result = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration.Create(
            TenantId.Load(request.TenantId),
            SystemSuiteId.Load(request.SystemSuiteId),
            providerType,
            request.DomainHints.Where(hint => !string.IsNullOrWhiteSpace(hint)).Select(hint => hint.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.ConfigPayload.Trim(),
            request.SecretRef.Trim(),
            request.ResolutionPriority,
            request.FallbackToId,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<CreateIdpConfigurationResponse>.Failure(result.Error);
        }

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateIdpConfigurationResponse>.Success(new CreateIdpConfigurationResponse(result.Value.Props.Id.GetValue()));
    }
}
