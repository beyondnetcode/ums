using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed class EnrollUserAccountMfaCommandHandler : ICommandHandler<EnrollUserAccountMfaCommand, EnrollUserAccountMfaResponse>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public EnrollUserAccountMfaCommandHandler(IUserAccountRepository userAccountRepository, IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<EnrollUserAccountMfaResponse>> Handle(EnrollUserAccountMfaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<EnrollUserAccountMfaResponse>.Failure("Authenticated user is required to enroll MFA.");
        }

        var method = DomainEnumerationParser.FromName<MfaMethod>(request.Method);
        if (method is null)
        {
            return Result<EnrollUserAccountMfaResponse>.Failure("MFA method is invalid.");
        }

        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);
        if (userAccount is null)
        {
            return Result<EnrollUserAccountMfaResponse>.Failure("User account was not found.");
        }

        var result = userAccount.EnrollMfa(method, ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return Result<EnrollUserAccountMfaResponse>.Failure(result.Error);
        }

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var enrollmentId = userAccount.MfaEnrollments.Single(x => x.Method == method).GetId().GetValue();
        return Result<EnrollUserAccountMfaResponse>.Success(new EnrollUserAccountMfaResponse(enrollmentId));
    }
}
