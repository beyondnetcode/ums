using System.Security.Cryptography;
using Ums.Application.Common.Interfaces;
using Ums.Application.Common.Notifications;

namespace Ums.Application.Identity.Auth.Commands;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly INotificationService _notificationService;

    public ForgotPasswordCommandHandler(
        ITenantRepository tenantRepository,
        IUserAccountRepository userAccountRepository,
        IPasswordHashingService passwordHashingService,
        INotificationService notificationService)
    {
        _tenantRepository = tenantRepository;
        _userAccountRepository = userAccountRepository;
        _passwordHashingService = passwordHashingService;
        _notificationService = notificationService;
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByCodeAsync(request.TenantCode, cancellationToken);
        if (tenant is null)
            return Ambiguous();

        var email = Email.Create(request.Email);
        var userAccount = await _userAccountRepository.GetByEmailAsync(email, cancellationToken);

        // Always return success to avoid user enumeration
        var tenantId = tenant.Props.Id.GetValue();
        if (userAccount is null
            || userAccount.Props.TenantId.GetValue() != tenantId
            || userAccount.IdentityReference is not null)
            return Ambiguous();

        var tempPassword = GenerateTemporaryPassword();
        var hash = _passwordHashingService.Hash(tempPassword);
        var actor = ActorId.Create("00000000-0000-0000-0000-000000000001"); // system actor

        var result = userAccount.AddPassword(PasswordHash.Create(hash), actor);
        if (result.IsFailure)
            return Ambiguous();

        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        await _notificationService.SendAsync(
            NotificationTemplates.PasswordReset(
                recipient: userAccount.Email.GetValue(),
                recipientName: userAccount.Email.GetValue().Split('@')[0],
                temporaryPassword: tempPassword),
            cancellationToken);

        return Result<ForgotPasswordResponse>.Success(new ForgotPasswordResponse(
            Message: "Si el correo está registrado, recibirá instrucciones para restablecer su contraseña.",
            SimulatedTemporaryPassword: tempPassword
        ));
    }

    private static Result<ForgotPasswordResponse> Ambiguous() =>
        Result<ForgotPasswordResponse>.Success(new ForgotPasswordResponse(
            Message: "Si el correo está registrado, recibirá instrucciones para restablecer su contraseña.",
            SimulatedTemporaryPassword: null
        ));

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%&";
        const string all = upper + lower + digits + special;

        var chars = new char[16];
        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];
        for (var i = 4; i < chars.Length; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }
}
