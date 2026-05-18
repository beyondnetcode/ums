namespace Ums.Application.Tenants.AddBranch;

using FluentValidation;

public sealed class AddBranchCommandValidator : AbstractValidator<AddBranchCommand>
{
    public AddBranchCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();

        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.GeofencingMetadata)
            .MaximumLength(1000)
            .When(command => !string.IsNullOrWhiteSpace(command.GeofencingMetadata));
    }
}
