namespace Ums.Application.Identity.UserAccount.Commands;

public sealed class AddUserAccountPasswordCommandValidator : AbstractValidator<AddUserAccountPasswordCommand>
{
    public AddUserAccountPasswordCommandValidator()
    {
        RuleFor(x => x.UserAccountId).NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña temporal es obligatoria.")
            .MinimumLength(12).WithMessage("La contraseña temporal debe tener al menos 12 caracteres.")
            .MaximumLength(128).WithMessage("La contraseña temporal no puede superar 128 caracteres.");
    }
}
