using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed class AddUserAccountPasswordCommandHandler : ICommandHandler<AddUserAccountPasswordCommand, AddUserAccountPasswordResponse>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public AddUserAccountPasswordCommandHandler(IUserAccountRepository userAccountRepository, IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<AddUserAccountPasswordResponse>> Handle(AddUserAccountPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<AddUserAccountPasswordResponse>.Failure("Authenticated user is required to add a password.");
        }

        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (userAccount is null)
        {
            return Result<AddUserAccountPasswordResponse>.Failure("User account was not found.");
        }

        var result = userAccount.AddPassword(PasswordHash.Create(request.PasswordHash), ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return Result<AddUserAccountPasswordResponse>.Failure(result.Error);
        }

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var activeCredentialId = userAccount.PasswordCredentials.Single(x => x.IsActive).GetId().GetValue();
        return Result<AddUserAccountPasswordResponse>.Success(new AddUserAccountPasswordResponse(activeCredentialId));
    }
}
