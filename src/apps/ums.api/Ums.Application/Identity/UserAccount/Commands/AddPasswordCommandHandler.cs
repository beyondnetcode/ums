using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed class AddPasswordCommandHandler : ICommandHandler<AddPasswordCommand, AddPasswordResponse>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public AddPasswordCommandHandler(
        IUserAccountRepository userAccountRepository,
        IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<AddPasswordResponse>> Handle(AddPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<AddPasswordResponse>.Failure("Authenticated user is required to add a password.");
        }

        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (userAccount is null)
        {
            return Result<AddPasswordResponse>.Failure("User account was not found.");
        }

        var result = userAccount.AddPassword(PasswordHash.Create(request.PasswordHash), ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return Result<AddPasswordResponse>.Failure(result.Error);
        }

        var credential = userAccount.PasswordCredentials.First(c => c.IsActive);
        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<AddPasswordResponse>.Success(new AddPasswordResponse(credential.Id.GetValue()));
    }
}
