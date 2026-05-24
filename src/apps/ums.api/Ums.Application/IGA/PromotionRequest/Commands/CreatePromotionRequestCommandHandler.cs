using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.IGA.PromotionRequest.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain;
using Ums.Domain.IGA;
using Ums.Domain.IGA.PromotionRequest;

public sealed class CreatePromotionRequestCommandHandler : ICommandHandler<CreatePromotionRequestCommand, CreatePromotionRequestResponse>
{
    private readonly IPromotionRequestRepository _repository;
    private readonly IUserContext _userContext;

    public CreatePromotionRequestCommandHandler(IPromotionRequestRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreatePromotionRequestResponse>> Handle(CreatePromotionRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreatePromotionRequestResponse>.Failure("Authenticated user is required.");

        var reason = string.IsNullOrWhiteSpace(request.RequestReason)
            ? null
            : TextValueObject.Create(request.RequestReason);

        var result = PromotionRequest.Create(
            TenantId.Load(request.TenantId),
            UserId.Load(request.UserId),
            RoleId.Load(request.CurrentRoleId),
            RoleId.Load(request.TargetRoleId),
            UserId.Load(request.ManagerId),
            reason,
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreatePromotionRequestResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreatePromotionRequestResponse>.Success(new CreatePromotionRequestResponse(result.Value.Props.Id.GetValue()));
    }
}
