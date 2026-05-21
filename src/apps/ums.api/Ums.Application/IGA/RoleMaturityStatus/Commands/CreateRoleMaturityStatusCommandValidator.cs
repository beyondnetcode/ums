namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

using FluentValidation;

public sealed class CreateRoleMaturityStatusCommandValidator : AbstractValidator<CreateRoleMaturityStatusCommand>
{
    public CreateRoleMaturityStatusCommandValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.RoleId).NotEmpty();
        RuleFor(c => c.CurrentMaturityLevel).NotEmpty();
    }
}
