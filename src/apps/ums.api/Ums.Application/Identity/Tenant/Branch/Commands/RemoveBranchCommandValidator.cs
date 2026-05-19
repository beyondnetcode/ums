namespace Ums.Application.Identity.Tenant.Branch.Commands;

using FluentValidation;

public sealed class RemoveBranchCommandValidator : AbstractValidator<RemoveBranchCommand>
{
    public RemoveBranchCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();

        RuleFor(command => command.BranchId)
            .NotEmpty();
    }
}
