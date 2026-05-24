using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Identity.UserAccount.Commands;

using Ums.Application.Common.Interfaces;

public sealed class BlockUserAccountCommandHandler : ICommandHandler<BlockUserAccountCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public BlockUserAccountCommandHandler(
        IUserAccountRepository userAccountRepository,
        IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(BlockUserAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to block a user account.");
        }

        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (userAccount is null)
        {
            return Result.Failure("User account was not found.");
        }

        var result = userAccount.Block(Reason.Create(request.Reason), ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
