namespace Ums.Application.Identity.UserAccount.Commands;

public sealed class AddUserAccountPasswordCommandValidator : AbstractValidator<AddUserAccountPasswordCommand>
{
    public AddUserAccountPasswordCommandValidator()
    {
        RuleFor(x => x.UserAccountId).NotEmpty();
        RuleFor(x => x.PasswordHash).NotEmpty().MaximumLength(500);
    }
}
